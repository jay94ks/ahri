using System;
using System.IO;

namespace Ahri.Networks.Utilities
{
    public class FixedBinaryReader
    {
        private Action<byte[]> m_Endian;
        private byte[] m_Buf64, m_Buf32, m_Buf16;

        /// <summary>
        /// Initialize a new <see cref="FixedBinaryReader"/> instance.
        /// </summary>
        /// <param name="Segment"></param>
        /// <param name="LittleEndian"></param>
        public FixedBinaryReader(ArraySegment<byte> Segment, bool LittleEndian = true)
        {
            Array = Segment.Array;
            Index = Segment.Offset;
            Length = Segment.Count;

            if (LittleEndian != BitConverter.IsLittleEndian)
                m_Endian = System.Array.Reverse;

            else
                m_Endian = _ => { };
        }

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
        /// Read bytes from the array.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Index"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        public int Read(byte[] Buffer, int Index, int Length)
        {
            if ((Length = Math.Min(Length, this.Length)) <= 0)
                return 0;

            System.Buffer.BlockCopy(Array, this.Index, Buffer, Index, Length);
            this.Index += Length; this.Length -= Length;
            return Length;
        }

        /// <summary>
        /// Throw <see cref="EndOfStreamException"/> exception.
        /// </summary>
        private void ThrowNoMoreBytes() => throw new EndOfStreamException("No more bytes available.");

        /// <summary>
        /// Read a byte from the array.
        /// </summary>
        /// <exception cref="EndOfStreamException"></exception>
        /// <returns></returns>
        public byte ReadByte()
        {
            if (Length <= 0)
                ThrowNoMoreBytes();

            byte RetVal = Array[Index];
            Index++; Length--;

            return RetVal;
        }

        /// <summary>
        /// Try to read a byte from the array.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool TryReadByte(out byte Value)
        {
            if (Length > 0)
            {
                Value = Array[Index];
                Index++; Length--;
                return true;
            }

            Value = 0;
            return false;
        }

        /// <summary>
        /// Try to read integer byte from the array using buffer and expected length.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Expects"></param>
        /// <returns></returns>
        private bool TryReadIntBytes(ref byte[] Buffer, int Expects)
        {
            if (Buffer is null)
                Buffer = new byte[Expects];

            else if (Buffer.Length != Expects)
                System.Array.Resize(ref Buffer, Expects);

            var Size = Read(Buffer, 0, Buffer.Length);
            if (Size != Buffer.Length)
            {
                Index -= Size;
                Length -= Size;
                return false;
            }

            m_Endian.Invoke(Buffer);
            return true;
        }

        /// <summary>
        /// Read a 16bit integer from the array.
        /// </summary>
        /// <returns></returns>
        public short ReadInt16()
        {
            if (!TryReadIntBytes(ref m_Buf16, sizeof(short)))
                ThrowNoMoreBytes();

            return BitConverter.ToInt16(m_Buf16);
        }

        /// <summary>
        /// Try to read a 16bit integer from the array.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool TryReadInt16(out short Value)
        {
            if (TryReadIntBytes(ref m_Buf16, sizeof(short)))
            {
                Value = BitConverter.ToInt16(m_Buf16);
                return true;
            }

            Value = 0;
            return false;
        }

        /// <summary>
        /// Read a 32bit integer from the array.
        /// </summary>
        /// <returns></returns>
        public int ReadInt32()
        {
            if (!TryReadIntBytes(ref m_Buf32, sizeof(int)))
                ThrowNoMoreBytes();

            return BitConverter.ToInt32(m_Buf32);
        }

        /// <summary>
        /// Try to read a 32bit integer from the array.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool TryReadInt32(out int Value)
        {
            if (TryReadIntBytes(ref m_Buf32, sizeof(int)))
            {
                Value = BitConverter.ToInt32(m_Buf32);
                return true;
            }

            Value = 0;
            return false;
        }

        /// <summary>
        /// Read a 64bit integer from the array.
        /// </summary>
        /// <returns></returns>
        public long ReadInt64()
        {
            if (!TryReadIntBytes(ref m_Buf64, sizeof(long)))
                ThrowNoMoreBytes();

            return BitConverter.ToInt64(m_Buf64);
        }

        /// <summary>
        /// Try to read a 64bit integer from the array.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool TryReadInt64(out long Value)
        {
            if (TryReadIntBytes(ref m_Buf64, sizeof(long)))
            {
                Value = BitConverter.ToInt32(m_Buf64);
                return true;
            }

            Value = 0;
            return false;
        }

        /// <summary>
        /// Read an unsigned 16bit integer from the array.
        /// </summary>
        /// <returns></returns>
        public ushort ReadUInt16()
        {
            if (!TryReadIntBytes(ref m_Buf16, sizeof(ushort)))
                ThrowNoMoreBytes();

            return BitConverter.ToUInt16(m_Buf16);
        }

        /// <summary>
        /// Try to read an unsigned 64bit integer from the array.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool TryReadUInt16(out ushort Value)
        {
            if (TryReadIntBytes(ref m_Buf16, sizeof(ushort)))
            {
                Value = BitConverter.ToUInt16(m_Buf16);
                return true;
            }

            Value = 0;
            return false;
        }

        /// <summary>
        /// Read an unsigned 32bit integer from the array.
        /// </summary>
        /// <returns></returns>
        public uint ReadUInt32()
        {
            if (!TryReadIntBytes(ref m_Buf32, sizeof(uint)))
                ThrowNoMoreBytes();

            return BitConverter.ToUInt32(m_Buf32);
        }

        /// <summary>
        /// Try to read an unsigned 64bit integer from the array.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool TryReadUInt32(out uint Value)
        {
            if (TryReadIntBytes(ref m_Buf32, sizeof(uint)))
            {
                Value = BitConverter.ToUInt32(m_Buf32);
                return true;
            }

            Value = 0;
            return false;
        }

        /// <summary>
        /// Read an unsigned 64bit integer from the array.
        /// </summary>
        /// <returns></returns>
        public ulong ReadUInt64()
        {
            if (!TryReadIntBytes(ref m_Buf64, sizeof(ulong)))
                ThrowNoMoreBytes();

            return BitConverter.ToUInt64(m_Buf64);
        }

        /// <summary>
        /// Try to read an unsigned 64bit integer from the array.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool TryReadUInt64(out ulong Value)
        {
            if (TryReadIntBytes(ref m_Buf64, sizeof(ulong)))
            {
                Value = BitConverter.ToUInt32(m_Buf64);
                return true;
            }

            Value = 0;
            return false;
        }

        /// <summary>
        /// Read a 32bit floating-point number from the array.
        /// </summary>
        /// <returns></returns>
        public float ReadSingle()
        {
            if (!TryReadIntBytes(ref m_Buf32, sizeof(float)))
                ThrowNoMoreBytes();

            return BitConverter.ToSingle(m_Buf32);
        }

        /// <summary>
        /// Try to read a 32bit floating point number from the array.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool TryReadSingle(out float Value)
        {
            if (TryReadIntBytes(ref m_Buf32, sizeof(float)))
            {
                Value = BitConverter.ToSingle(m_Buf32);
                return true;
            }

            Value = 0;
            return false;
        }

        /// <summary>
        /// Read a 64bit floating-point number from the array.
        /// </summary>
        /// <returns></returns>
        public double ReadDouble()
        {
            if (!TryReadIntBytes(ref m_Buf64, sizeof(double)))
                ThrowNoMoreBytes();

            return BitConverter.ToDouble(m_Buf64);
        }

        /// <summary>
        /// Try to read a 64bit floating point number from the array.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public bool TryReadDouble(out double Value)
        {
            if (TryReadIntBytes(ref m_Buf64, sizeof(double)))
            {
                Value = BitConverter.ToDouble(m_Buf64);
                return true;
            }

            Value = 0;
            return false;
        }
    }
}
