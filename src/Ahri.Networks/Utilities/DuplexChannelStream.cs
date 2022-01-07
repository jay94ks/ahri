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
    /// Provides <see cref="Channel{T}"/> pair to handle <see cref="Stream"/>'s read/write.
    /// </summary>
    public class DuplexChannelStream : Stream
    {
        private PacketFragment m_Current;

        /// <summary>
        /// Initialize a new <see cref="HttpOpaqueStream"/> instance.
        /// </summary>
        public DuplexChannelStream()
        {
            ChannelRead = Channel.CreateUnbounded<byte[]>();
            ChannelWrite = Channel.CreateUnbounded<byte[]>();
        }

        /// <summary>
        /// Read Channel.
        /// </summary>
        public Channel<byte[]> ChannelRead { get; }

        /// <summary>
        /// Write Channel.
        /// </summary>
        public Channel<byte[]> ChannelWrite { get; }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanWrite => true;

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
            => ChannelWrite.Writer.WriteAsync(new PacketFragment(Buffer, Offset, Count).ToArray(), Token).AsTask();

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
            int TotalLength = 0;

            while (Count > 0)
            {
                if (m_Current.Length > 0)
                {
                    var Length = m_Current.CopyTo(Buffer, Offset, Count);

                    m_Current = m_Current.Skip(Length); TotalLength += Length;
                    Offset += Length; Count -= Length;
                    continue;
                }

                try { m_Current = await ChannelRead.Reader.ReadAsync(Token); }
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
            ChannelRead.Writer.TryComplete();
            ChannelWrite.Writer.TryComplete();
            base.Close();
        }
    }
}
