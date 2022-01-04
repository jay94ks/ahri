using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Actions.Internals
{
    internal class RangeStream : Stream
    {
        private static readonly Task<int> READ_ZERO = Task.FromResult(0);
        private static readonly ValueTask<int> READ_ZERO_VAL = ValueTask.FromResult(0);

        private Stream m_BaseStream;
        private long m_Length, m_Offset;
        private bool m_LeaveOpen;

        /// <summary>
        /// Initialize a new <see cref="RangeStream"/> instance.
        /// </summary>
        /// <param name="BaseStream"></param>
        /// <param name="Offset"></param>
        /// <param name="Length"></param>
        public RangeStream(Stream BaseStream, long Offset, long Length, bool LeaveOpen = false)
        {
            (m_BaseStream = BaseStream).Seek(Offset = Math.Max(Offset, 0), SeekOrigin.Begin);
            m_Length = Math.Max(0, Math.Min(m_BaseStream.Length - Offset, Length));
            m_LeaveOpen = LeaveOpen; m_Offset = m_BaseStream.Position;
        }

        /// <inheritdoc/>
        public override bool CanRead => m_BaseStream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => m_BaseStream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => m_BaseStream.CanWrite;

        /// <inheritdoc/>
        public override long Length => m_Length;

        /// <inheritdoc/>
        public override long Position
        {
            get => Math.Max(m_BaseStream.Position - m_Offset, 0);
            set => m_BaseStream.Position = Math.Max(Math.Min(value, m_Length) + m_Offset, 0);
        }

        /// <inheritdoc/>
        public override void Flush() => m_BaseStream.Flush();

        /// <inheritdoc/>
        public override int Read(Span<byte> Buffer)
        {
            var Remainder = (int)Math.Min(Math.Min(m_Length - Position, Buffer.Length), int.MaxValue);
            if (Remainder > 0)
                return m_BaseStream.Read(Buffer.Slice(0, Remainder));

            return 0;
        }

        /// <inheritdoc/>
        public override int Read(byte[] Buffer, int Offset, int Count)
        {
            var Remainder = Math.Min(Math.Min(m_Length - Position, Count), int.MaxValue);
            if ((Count = (int)Math.Max(Remainder, 0)) > 0)
                return m_BaseStream.Read(Buffer, Offset, Count);

            return 0;
        }

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] Buffer, int Offset, int Count, CancellationToken Token = default)
        {
            var Remainder = (int)Math.Min(Math.Min(m_Length - Position, Count), int.MaxValue);
            if ((Count = Remainder) > 0)
                return m_BaseStream.ReadAsync(Buffer, Offset, Count, Token);

            return READ_ZERO;
        }

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> Buffer, CancellationToken Token = default)
        {
            var Remainder = (int)Math.Min(Math.Min(m_Length - Position, Buffer.Length), int.MaxValue);
            if (Remainder > 0)
                return m_BaseStream.ReadAsync(Buffer.Slice(0, Remainder), Token);

            return READ_ZERO_VAL;
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> Buffer)
        {
            var Remainder = (int)Math.Min(Math.Min(m_Length - Position, Buffer.Length), int.MaxValue);
            if (Remainder > 0)
                m_BaseStream.Write(Buffer.Slice(0, Remainder));
        }

        /// <inheritdoc/>
        public override void Write(byte[] Buffer, int Offset, int Count)
        {
            var Remainder = (int) Math.Min(Math.Min(m_Length - Position, Count), int.MaxValue);
            if ((Count = Remainder) > 0)
                m_BaseStream.Write(Buffer, Offset, Count);
        }

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] Buffer, int Offset, int Count, CancellationToken Token = default)
        {
            var Remainder = (int)Math.Min(Math.Min(m_Length - Position, Count), int.MaxValue);
            if ((Count = Remainder) > 0)
                return m_BaseStream.WriteAsync(Buffer, Offset, Count);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> Buffer, CancellationToken Token = default)
        {
            var Remainder = (int)Math.Min(Math.Min(m_Length - Position, Buffer.Length), int.MaxValue);
            if (Remainder > 0)
                return m_BaseStream.WriteAsync(Buffer.Slice(0, Remainder), Token);

            return ValueTask.CompletedTask;
        }
        
        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin Origin)
        {
            switch(Origin)
            {
                case SeekOrigin.Begin:
                    return Position = offset;

                case SeekOrigin.Current:
                    return Position += offset;

                case SeekOrigin.End:
                    return Position = m_Length + offset;

                default: break;
            }

            return Position;
        }

        /// <inheritdoc/>
        public override void SetLength(long value) => m_BaseStream.SetLength(value);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing && !m_LeaveOpen)
            {
                m_LeaveOpen = true;
                m_BaseStream.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            if (!m_LeaveOpen)
            {
                m_LeaveOpen = true;
                await m_BaseStream.DisposeAsync();
            }

            await base.DisposeAsync();
        }
    }
}
