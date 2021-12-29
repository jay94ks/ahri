using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Ahri.Networks.Utilities
{
    /// <summary>
    /// Provides the conversion from <see cref="ChannelReader{T}"/> or <see cref="ChannelWriter{T}"/>.
    /// </summary>
    public class ChannelStream : Stream
    {
        private ChannelWriter<byte[]> m_Outputs;
        private ChannelReader<byte[]> m_Inputs;
        private PacketFragment m_Current;

        private Action m_RequestClose;
        private Action m_RequestInitiate;

        /// <summary>
        /// Initialize a new <see cref="ChannelStream"/> instance from the reader.
        /// </summary>
        /// <param name="Inputs"></param>
        public ChannelStream(ChannelReader<byte[]> Inputs, Action RequestClose)
        {
            m_Current = PacketFragment.Empty;
            m_Inputs = Inputs;
            m_Outputs = null;
            m_RequestClose = RequestClose;
        }

        /// <summary>
        /// Initialize a new <see cref="ChannelStream"/> instance from the writer.
        /// </summary>
        /// <param name="Reader"></param>
        public ChannelStream(ChannelWriter<byte[]> Outputs, Action RequestInitiate)
        {
            m_Current = PacketFragment.Empty;
            m_Inputs = null;
            m_Outputs = Outputs;
            m_RequestInitiate = RequestInitiate;
        }

        /// <summary>
        /// Readable if the <see cref="ChannelStream"/> created with <see cref="ChannelReader{T}"/>.
        /// </summary>
        public override bool CanRead => m_Inputs != null;

        /// <summary>
        /// Writable if the <see cref="ChannelStream"/> created with <see cref="ChannelWriter{T}"/>.
        /// </summary>
        public override bool CanWrite => m_Outputs != null;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        /// <summary>
        /// Write the <paramref name="Buffer"/> to <see cref="ChannelWriter{T}"/> asynchronously.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Offset"></param>
        /// <param name="Count"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        public override Task WriteAsync(byte[] Buffer, int Offset, int Count, CancellationToken Token = default)
        {
            if (m_Outputs is null)
                return Task.FromException(new NotSupportedException("This channel stream has no writer."));

            m_RequestInitiate?.Invoke();
            m_RequestInitiate = null;

            return m_Outputs.WriteAsync(new PacketFragment(Buffer, Offset, Count).ToArray(), Token).AsTask();
        }

        /// <inheritdoc/>
        public override void Write(byte[] Buffer, int Offset, int Count)
            => WriteAsync(Buffer, Offset, Count, default).GetAwaiter().GetResult();

        /// <summary>
        /// Read the <see cref="ChannelReader{T}"/> and copies its result to <paramref name="Buffer"/> progressively.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Offset"></param>
        /// <param name="Count"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        public override async Task<int> ReadAsync(byte[] Buffer, int Offset, int Count, CancellationToken Token = default)
        {
            if (m_Inputs is null)
                throw new NotSupportedException("This channel stream has no reader.");

            int TotalLength = 0;

            while(Count > 0)
            {
                if (m_Current.Length > 0)
                {
                    var Length = m_Current.CopyTo(Buffer, Offset, Count);

                    m_Current = m_Current.Skip(Length); TotalLength += Length;
                    Offset += Length; Count -= Length;
                    continue;
                }

                try { m_Current = await m_Inputs.ReadAsync(Token); }
                catch
                {
                    if (TotalLength >= 0)
                        break;

                    throw;
                }
            }

            return TotalLength;
        }

        /// <inheritdoc/>
        public override int Read(byte[] Buffer, int Offset, int Count)
            => ReadAsync(Buffer, Offset, Count, default).GetAwaiter().GetResult();

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Flush() { }

        /// <inheritdoc/>
        public override void Close()
        {
            if (m_Outputs != null)
                m_Outputs.TryComplete();

            m_RequestClose?.Invoke();
            m_RequestClose = null;

            base.Close();
        }
    }
}
