using System;
using System.IO;

namespace Ahri.Networks.Utilities
{
    internal class FixedBinaryWriter
    {
        private Action<byte[]> m_Endian;

        /// <summary>
        /// Initialize a new <see cref="FixedBinaryWriter"/> instance.
        /// </summary>
        /// <param name="Segment"></param>
        /// <param name="LittleEndian"></param>
        public FixedBinaryWriter(ArraySegment<byte> Segment, bool LittleEndian = true)
        {
            Array = Segment.Array;
            Index = Segment.Offset;
            Length = Segment.Count;

            if (LittleEndian != BitConverter.IsLittleEndian)
                m_Endian = System.Array.Reverse;

            else
                m_Endian = _ => { };
        }

        /// <summary>
        /// The array.
        /// </summary>
        public byte[] Array { get; set; }

        /// <summary>
        /// Index in the array.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Length of slice.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Throw <see cref="EndOfStreamException"/> exception.
        /// </summary>
        private void ThrowNoMoreBytes() => throw new EndOfStreamException("No more bytes available.");

        /// <summary>
        /// Write bytes into the array.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Index"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        public int Write(ArraySegment<byte> Buffer)
            => Write(Buffer.Array, Buffer.Offset, Buffer.Count);

        /// <summary>
        /// Write bytes into the array.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Index"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        public int Write(byte[] Buffer, int Index, int Length)
        {
            if ((Length = Math.Min(Length, this.Length)) <= 0)
                return 0;

            System.Buffer.BlockCopy(Buffer, Index, Array, this.Index, Length);
            this.Index += Length; this.Length -= Length;
            return Length;
        }

        /// <summary>
        /// Write a byte to the array.
        /// </summary>
        /// <param name="Byte"></param>
        public void WriteByte(byte Byte)
        {
            if (Length <= 0)
                ThrowNoMoreBytes();

            Array[Index++] = Byte;
            Length--;
        }

        /// <summary>
        /// Try to write a byte to the array.
        /// </summary>
        /// <param name="Byte"></param>
        /// <returns></returns>
        public bool TryWriteByte(byte Byte)
        {
            if (Length <= 0)
                return false;

            Array[Index++] = Byte;
            Length--;
            return true;
        }

        /// <summary>
        /// Write integer bytes.
        /// </summary>
        /// <param name="Buffer"></param>
        private void WriteInt(byte[] Buffer)
        {
            if (!TryWriteInt(Buffer))
                 ThrowNoMoreBytes();
        }

        /// <summary>
        /// Try to write integer bytes.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <returns></returns>
        private bool TryWriteInt(byte[] Buffer)
        {
            m_Endian.Invoke(Buffer);

            var Size = Write(Buffer, 0, Buffer.Length);
            if (Size != Buffer.Length)
            {
                Index -= Size;
                Length += Size;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write a 16bit integer to the array.
        /// </summary>
        /// <param name="Value"></param>
        public void WriteInt16(short Value) => WriteInt(BitConverter.GetBytes(Value));

        /// <summary>
        /// Try to write a 16bit integer to the array.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool TryWriteInt16(short Value) => TryWriteInt(BitConverter.GetBytes(Value));

        /// <summary>
        /// Write a 32bit integer to the array.
        /// </summary>
        /// <param name="Value"></param>
        public void WriteInt32(int Value) => WriteInt(BitConverter.GetBytes(Value));

        /// <summary>
        /// Try to write a 32bit integer to the array.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool TryWriteInt32(int Value) => TryWriteInt(BitConverter.GetBytes(Value));

        /// <summary>
        /// Write a 64bit integer to the array.
        /// </summary>
        /// <param name="Value"></param>
        public void WriteInt64(long Value) => WriteInt(BitConverter.GetBytes(Value));

        /// <summary>
        /// Try to write a 64bit integer to the array.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool TryWriteInt64(long Value) => TryWriteInt(BitConverter.GetBytes(Value));

        /// <summary>
        /// Write an unsigned 16bit integer to the array.
        /// </summary>
        /// <param name="Value"></param>
        public void WriteUInt16(ushort Value) => WriteInt(BitConverter.GetBytes(Value));

        /// <summary>
        /// Try to write a unsigned 16bit integer to the array.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool TryWriteUInt16(ushort Value) => TryWriteInt(BitConverter.GetBytes(Value));

        /// <summary>
        /// Write an unsigned 32bit integer to the array.
        /// </summary>
        /// <param name="Value"></param>
        public void WriteUInt32(uint Value) => WriteInt(BitConverter.GetBytes(Value));

        /// <summary>
        /// Try to write a unsigned 32bit integer to the array.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool TryWriteUInt32(uint Value) => TryWriteInt(BitConverter.GetBytes(Value));

        /// <summary>
        /// Write an unsigned 64bit integer to the array.
        /// </summary>
        /// <param name="Value"></param>
        public void WriteUInt64(ulong Value) => WriteInt(BitConverter.GetBytes(Value));

        /// <summary>
        /// Try to write a unsigned 64bit integer to the array.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool TryWriteUInt64(uint Value) => TryWriteInt(BitConverter.GetBytes(Value));

        /// <summary>
        /// Write a 32bit floating point number to the array.
        /// </summary>
        /// <param name="Value"></param>
        public void WriteSingle(float Value) => WriteInt(BitConverter.GetBytes(Value));

        /// <summary>
        /// Try to write a 32bit floating point number to the array.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool TryWriteSingle(float Value) => TryWriteInt(BitConverter.GetBytes(Value));

        /// <summary>
        /// Write a 64bit floating point number to the array.
        /// </summary>
        /// <param name="Value"></param>
        public void WriteDouble(double Value) => WriteInt(BitConverter.GetBytes(Value));

        /// <summary>
        /// Try to write a 64bit floating point number to the array.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool TryWriteDouble(double Value) => TryWriteInt(BitConverter.GetBytes(Value));

    }
}
