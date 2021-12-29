using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ahri.Networks
{
    public struct PacketFragment : IEnumerable<byte>, IEquatable<PacketFragment>
    {
        private static readonly byte[] EMPTY_BYTES = new byte[0];

        /// <summary>
        /// An empty packet value.
        /// </summary>
        public static readonly PacketFragment Empty = new PacketFragment(EMPTY_BYTES);

        /// <summary>
        /// Initialize a new <see cref="PacketFragment"/> from byte array.
        /// </summary>
        /// <param name="Bytes"></param>
        /// <param name="Copy"></param>
        public PacketFragment(byte[] Bytes, bool Copy = false)
        {
            if ((this.Bytes = Bytes) is null || Bytes.Length <= 0)
            {
                this.Bytes = EMPTY_BYTES;
                this.Offset = this.Length = 0;
            }

            else
            {
                Offset = 0;
                Length = Bytes.Length;

                if (Copy)
                {
                    Bytes = ToArray();
                    this.Offset = 0;
                }
            }
        }

        /// <summary>
        /// Initialize a new <see cref="PacketFragment"/> from buffer.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Offset"></param>
        /// <param name="Length"></param>
        /// <param name="Copy"></param>
        public PacketFragment(byte[] Buffer, int Offset, int Length, bool Copy = false)
        {
            if ((Bytes = Buffer) is null || Length <= 0)
            {
                Bytes = EMPTY_BYTES;
                this.Offset = this.Length = 0;
            }

            else
            {
                this.Offset = Math.Max(0, Math.Min(Offset, Buffer.Length));
                this.Length = Math.Max(0, Math.Min(Length, Buffer.Length - this.Offset));

                if (Copy)
                {
                    Bytes = ToArray();
                    this.Offset = 0;
                }
            }
        }

        /// <summary>
        /// Initialize a packet from byte array.
        /// </summary>
        /// <param name="Bytes"></param>
        public static implicit operator PacketFragment(byte[] Bytes) => new PacketFragment(Bytes);

        /// <summary>
        /// Initialize a packet from byte array segment.
        /// </summary>
        /// <param name="Bytes"></param>
        public static implicit operator PacketFragment(ArraySegment<byte> Bytes) => new PacketFragment(Bytes.Array ?? EMPTY_BYTES, Bytes.Offset, Bytes.Count);

        /// <summary>
        /// Initialize a byte array segment from the packet.
        /// </summary>
        /// <param name="Packet"></param>
        public static implicit operator ArraySegment<byte>(PacketFragment Packet) => new ArraySegment<byte>(Packet.Bytes ?? EMPTY_BYTES, Packet.Offset, Packet.Length);

        /// <summary>
        /// Initialize a byte array segment from the packet.
        /// </summary>
        /// <param name="Packet"></param>
        public static implicit operator Memory<byte>(PacketFragment Packet) => new Memory<byte>(Packet.Bytes ?? EMPTY_BYTES, Packet.Offset, Packet.Length);

        /// <summary>
        /// Initialize a byte array segment from the packet.
        /// </summary>
        /// <param name="Packet"></param>
        public static implicit operator ReadOnlyMemory<byte>(PacketFragment Packet) => new ReadOnlyMemory<byte>(Packet.Bytes ?? EMPTY_BYTES, Packet.Offset, Packet.Length);

        /// <summary>
        /// Bytes that stored in this packet.
        /// </summary>
        public byte[] Bytes { get; }

        /// <summary>
        /// Offset in the <see cref="Bytes"/>.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// Length in the <see cref="Bytes"/>.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets a byte at the specified index.
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public byte this[Index Index]
        {
            get
            {
                if (Index.Value < 0 || Index.Value >= Length)
                    throw new ArgumentOutOfRangeException(nameof(Index));

                return Bytes[Index.Value + Offset];
            }
        }

        /// <summary>
        /// Returns an array that contains only the packet range.
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray(bool Force = true)
        {
            if (Force || Offset > 0 || Length != Bytes.Length)
            {
                var Output = new byte[Length];
                if (Bytes != null)
                    Buffer.BlockCopy(Bytes, Offset, Output, 0, Length);

                return Output;
            }

            return Bytes;
        }

        /// <summary>
        /// Copy the packet in range.
        /// </summary>
        /// <returns></returns>
        public PacketFragment Copy() => ToArray();

        /// <summary>
        /// Returns an array that contains only the packet range.
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray(int Offset, int Length)
        {
            Offset = Math.Max(0, Math.Min(Offset, this.Length));
            Length = Math.Max(0, Math.Min(Length, this.Length - Offset));

            var Output = new byte[Length];
            if (Bytes != null)
                Buffer.BlockCopy(Bytes, Offset, Output, 0, Length);

            return Output;
        }

        /// <summary>
        /// Get a packet bytes in the specified range.
        /// </summary>
        /// <param name="Offset"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        public PacketFragment Range(int Offset, int Length = int.MaxValue, bool Copy = false)
        {
            if (Copy)
                return new PacketFragment(ToArray(Offset, Length));

            Offset = Math.Max(0, Math.Min(Offset, this.Length));
            Length = Math.Max(0, Math.Min(Length, this.Length - Offset));
            return new PacketFragment(Bytes, this.Offset + Offset, Length);
        }

        /// <summary>
        /// Skip N bytes on the packet.
        /// </summary>
        /// <param name="Length"></param>
        /// <param name="Copy"></param>
        /// <returns></returns>
        public PacketFragment Skip(int Length, bool Copy = false) => Range(Length, int.MaxValue, Copy);

        /// <summary>
        /// Skip last N bytes on the packet.
        /// </summary>
        /// <param name="Length"></param>
        /// <param name="Copy"></param>
        /// <returns></returns>
        public PacketFragment SkipLast(int Length, bool Copy = false) => Range(0, this.Length - Length, Copy);

        /// <summary>
        /// Take N bytes on the packet.
        /// </summary>
        /// <param name="Length"></param>
        /// <returns></returns>
        public PacketFragment Take(int Length, bool Copy = false) => Range(0, Length, Copy);

        /// <summary>
        /// Take last N bytes on the packet.
        /// </summary>
        /// <param name="Length"></param>
        /// <returns></returns>
        public PacketFragment TakeLast(int Length, bool Copy = false) => Range(this.Length - Length, Length, Copy);

        /// <summary>
        /// Copy bytes to specified buffer.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <returns></returns>
        public int CopyTo(byte[] Buffer) => CopyTo(new PacketFragment(Buffer, 0, (Buffer ?? EMPTY_BYTES).Length));

        /// <summary>
        /// Copy bytes to specified buffer.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        public int CopyTo(byte[] Buffer, int Index) => CopyTo(new PacketFragment(Buffer, Index, (Buffer ?? EMPTY_BYTES).Length - Index));

        /// <summary>
        /// Copy bytes to specified buffer.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Index"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        public int CopyTo(byte[] Buffer, int Index, int Length) => CopyTo(new PacketFragment(Buffer, Index, Math.Min(Length, this.Length)));

        /// <summary>
        /// Copy bytes to specified buffer.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <returns></returns>
        public int CopyTo(int Offset, byte[] Buffer) => CopyTo(Offset, Buffer, 0, (Buffer ?? EMPTY_BYTES).Length);

        /// <summary>
        /// Copy bytes to specified buffer.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        public int CopyTo(int Offset, byte[] Buffer, int Index) => CopyTo(Offset, Buffer, Index, (Buffer ?? EMPTY_BYTES).Length - Index);

        /// <summary>
        /// Copy bytes to specified buffer.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="Index"></param>
        /// <param name="Size"></param>
        /// <returns></returns>
        public int CopyTo(int Offset, byte[] Buffer, int Index, int Size) => Range(Offset).CopyTo(Buffer, Index, Size);

        /// <summary>
        /// Copy bytes to specified packet in slice.
        /// </summary>
        /// <param name="Other"></param>
        /// <returns></returns>
        public int CopyTo(PacketFragment Other)
        {
            if (Other.Length > 0 && Length > 0)
            {
                var Slice = Math.Min(Length, Other.Length);
                Buffer.BlockCopy(Bytes, Offset, Other.Bytes, Other.Offset, Slice);
                return Slice;
            }

            return 0;
        }

        /// <summary>
        /// Copy bytes to specified packet in slice.
        /// </summary>
        /// <param name="Other"></param>
        /// <returns></returns>
        public int CopyTo(int Offset, PacketFragment Other) => Range(Offset).CopyTo(Other);

        /// <summary>
        /// Copy bytes to specified packet in slice.
        /// </summary>
        /// <param name="Other"></param>
        /// <returns></returns>
        public int CopyTo(int Offset, int Length, PacketFragment Other) => Range(Offset, Length).CopyTo(Other);

        /// <summary>
        /// Find the index of the single byte.
        /// </summary>
        /// <param name="Single"></param>
        /// <returns></returns>
        public int IndexOf(byte Single)
        {
            for (int i = 0, j = Offset; i < Length; ++j, ++i)
            {
                if (Bytes[j] == Single)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Find the index of the single byte after specified index.
        /// </summary>
        /// <param name="Single"></param>
        /// <returns></returns>
        public int IndexOf(byte Single, int Index)
        {
            var Sub = Range(Index).IndexOf(Single);
            if (Sub >= 0)
            {
                return Sub + Index;
            }

            return -1;
        }

        /// <summary>
        /// Find the index of the single byte in specified range.
        /// </summary>
        /// <param name="Single"></param>
        /// <returns></returns>
        public int IndexOf(byte Single, int Index, int Length)
        {
            var Sub = Range(Index, Length).IndexOf(Single);
            if (Sub >= 0)
            {
                return Sub + Index;
            }

            return -1;
        }

        /// <summary>
        /// Find the last index of the single byte.
        /// </summary>
        /// <param name="Single"></param>
        /// <returns></returns>
        public int LastIndexOf(byte Single)
        {
            for (int i = Length - 1, j = Offset + Length - 1; i >= 0; --j, --i)
            {
                if (Bytes[j] == Single)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Find the last index of the single byte after specified index.
        /// </summary>
        /// <param name="Single"></param>
        /// <returns></returns>
        public int LastIndexOf(byte Single, int Index)
        {
            var Sub = Range(Index).LastIndexOf(Single);
            if (Sub >= 0)
            {
                return Sub + Index;
            }

            return -1;
        }

        /// <summary>
        /// Find the last index of the single byte in specified range.
        /// </summary>
        /// <param name="Single"></param>
        /// <returns></returns>
        public int LastIndexOf(byte Single, int Index, int Offset)
        {
            var Sub = Range(Index, Offset).LastIndexOf(Single);
            if (Sub >= 0)
            {
                return Sub + Index;
            }

            return -1;
        }

        /// <summary>
        /// Find the index of the byte array presents.
        /// </summary>
        /// <param name="Other"></param>
        /// <returns></returns>
        public int IndexOf(ArraySegment<byte> Other)
        {
            if (Other.Count == 0)
                return 0;

            if (Other.Count > Length)
                return -1;

            for (int i = 0; i < Length - Other.Count; ++i)
            {
                if (Skip(i) == Other)
                    return  i;
            }

            return -1;
        }

        /// <summary>
        /// Find the index of the byte array presents.
        /// </summary>
        /// <param name="Other"></param>
        /// <returns></returns>
        public int IndexOf(ArraySegment<byte> Other, int Index)
        {
            var Sub = Range(Index).IndexOf(Other);
            if (Sub >= 0)
            {
                return Sub + Index;
            }

            return -1;
        }

        /// <summary>
        /// Find the index of the byte array presents.
        /// </summary>
        /// <param name="Other"></param>
        /// <returns></returns>
        public int IndexOf(ArraySegment<byte> Other, int Index, int Length)
        {
            var Sub = Range(Index, Length).IndexOf(Other);
            if (Sub >= 0)
            {
                return Sub + Index;
            }

            return -1;
        }

        /// <summary>
        /// Find the last index of the byte array presents.
        /// </summary>
        /// <param name="Other"></param>
        /// <returns></returns>
        public int LastIndexOf(ArraySegment<byte> Other)
        {
            if (Other.Count == 0)
                return 0;

            if (Other.Count > Length)
                return -1;

            for (int i = Length - 1; i >= 0; --i)
            {
                if (Skip(i) == Other)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Test whether the single byte is contained on the packet bytes.
        /// </summary>
        /// <param name="Single"></param>
        /// <returns></returns>
        public bool Contains(byte Single) => IndexOf(Single) >= 0;

        /// <summary>
        /// Test whether the byte array segment is contained on the packet bytes.
        /// </summary>
        /// <param name="Other"></param>
        /// <returns></returns>
        public bool Contains(ArraySegment<byte> Other) => IndexOf(Other) >= 0;

        /// <summary>
        /// Test whether the packet is started with a single byte.
        /// </summary>
        /// <param name="Single"></param>
        /// <returns></returns>
        public bool StartsWith(byte Single)
        {
            if (Length > 0)
                return Bytes[Offset] == Single;

            return false;
        }

        /// <summary>
        /// Test whether the packet is started with the byte array segment.
        /// </summary>
        /// <param name="Single"></param>
        /// <returns></returns>
        public bool StartsWith(ArraySegment<byte> Other)
        {
            if (Other.Count <= 0)
                return true;

            if (Length < Other.Count)
                return false;

            return Take(Other.Count) == Other;
        }

        /// <summary>
        /// Test whether the packet is ended with a single byte.
        /// </summary>
        /// <param name="Single"></param>
        /// <returns></returns>
        public bool EndsWith(byte Single)
        {
            if (Length > 0)
                return Bytes[Offset + (Length - 1)] == Single;

            return false;
        }

        /// <summary>
        /// Test whether the packet is ended with the byte array segment.
        /// </summary>
        /// <param name="Single"></param>
        /// <returns></returns>
        public bool EndsWith(ArraySegment<byte> Other)
        {
            if (Other.Count <= 0)
                return true;

            if (Length < Other.Count)
                return false;

            return TakeLast(Other.Count) == Other;
        }

        /// <summary>
        /// Compare two packets.
        /// </summary>
        /// <param name="Left"></param>
        /// <param name="Right"></param>
        /// <returns></returns>
        public static int Compare(PacketFragment Left, PacketFragment Right)
        {
            if (Left.Length <= 0 || Right.Length <= 0)
                return Left.Length - Right.Length;

            /* Two packets share same byte array and in same offset, */
            if (Left.Bytes == Right.Bytes && Left.Offset == Right.Offset)
                return Left.Length - Right.Length; /* just diff the length. */

            int Min = Math.Min(Left.Length, Right.Length);
            for (int i = 0, oL = Left.Offset, oR = Right.Offset;
                     i < Min; ++i, ++oL, ++oR)
            {
                var Value = Left.Bytes[oL] - Right.Bytes[oR];
                if (Value != 0) return Value;
            }

            return Left.Length - Right.Length;
        }

        /// <summary>
        /// Test whether two packets are equals.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator ==(PacketFragment A, PacketFragment B) => Compare(A, B) == 0;

        /// <summary>
        /// Test whether two packets are different.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator !=(PacketFragment A, PacketFragment B) => Compare(A, B) != 0;

        /// <summary>
        /// Test whether two packets are equals.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator ==(PacketFragment A, ArraySegment<byte> B) => Compare(A, B) == 0;

        /// <summary>
        /// Test whether two packets are different.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator !=(PacketFragment A, ArraySegment<byte> B) => Compare(A, B) != 0;

        /// <summary>
        /// Split the packet to multiple packets.
        /// </summary>
        /// <param name="Delimiter"></param>
        /// <param name="Limit"></param>
        /// <param name="NoEmpty"></param>
        /// <returns></returns>
        public IEnumerable<PacketFragment> Split(byte Delimiter, bool NoEmpty = false, bool Copy = false, int Limit = int.MaxValue)
        {
            var Cursor = 0;

            while (Cursor >= 0 && Cursor < Length && Limit > 1)
            {
                var Index = IndexOf(Delimiter);
                if (Index <= 0)
                    break;

                if (!NoEmpty || (Index - Cursor) > 0)
                {
                    yield return Range(Cursor, Index - Cursor, Copy);
                    Limit--;
                }

                Cursor = Index + 1;
            }

            if (!NoEmpty || (Length - Cursor) > 0)
                yield return Range(Cursor, Length - Cursor, Copy);
        }

        /// <summary>
        /// Split the packet to multiple packets.
        /// </summary>
        /// <param name="Delimiter"></param>
        /// <param name="Limit"></param>
        /// <param name="NoEmpty"></param>
        /// <returns></returns>
        public IEnumerable<PacketFragment> Split(ArraySegment<byte> Delimiter, bool NoEmpty = false, bool Copy = false, int Limit = int.MaxValue)
        {
            var Cursor = 0;

            if (Delimiter.Count <= 0)
            {
                for (var i = 0; i < Length - 1; ++i)
                    yield return Range(i, 1, Copy);

                yield break;
            }

            while (Cursor >= 0 && Cursor < Length && Limit > 1)
            {
                var Index = IndexOf(Delimiter);
                if (Index <= 0)
                    break;

                if (!NoEmpty || (Index - Cursor) > 0)
                {
                    yield return Range(Cursor, Index - Cursor, Copy);
                    Limit--;
                }

                Cursor = Index + Delimiter.Count;
            }

            if (!NoEmpty || (Length - Cursor) > 0)
                yield return Range(Cursor, Length - Cursor, Copy);

        }

        /// <summary>
        /// Join all packets to single packet.
        /// </summary>
        /// <param name="Delimiter"></param>
        /// <param name="Packets"></param>
        /// <returns></returns>
        public static PacketFragment Join(IEnumerable<PacketFragment> Packets)
        {
            var Count = Packets.Count();
            var Length = Packets.Sum(X => X.Length);
            var Output = new byte[Length];
            var Offset = 0;

            foreach (var Each in Packets)
            {
                if (Each.Length <= 0)
                    continue;

                Buffer.BlockCopy(Each.Bytes, Each.Offset, Output, Offset, Each.Length);
                Offset += Each.Length;
            }

            return new PacketFragment(Output);
        }

        /// <summary>
        /// Join all packets to single packet.
        /// </summary>
        /// <param name="Delimiter"></param>
        /// <param name="Packets"></param>
        /// <returns></returns>
        public static PacketFragment Join(params PacketFragment[] Packets) => Join(Packets as IEnumerable<PacketFragment>);

        /// <summary>
        /// Join all packets to single packet.
        /// </summary>
        /// <param name="Delimiter"></param>
        /// <param name="Packets"></param>
        /// <returns></returns>
        public static PacketFragment Join(byte Delimiter, IEnumerable<PacketFragment> Packets)
        {
            var Count = Packets.Count();
            var Length = Packets.Sum(X => X.Length) + 1 * Math.Max(Count - 1, 0);
            var Output = new byte[Length];
            var Offset = 0;

            foreach(var Each in Packets)
            {
                if (Offset > 0)
                    Output[Offset++] = Delimiter;

                if (Each.Length <= 0)
                    continue;

                Buffer.BlockCopy(Each.Bytes, Each.Offset, Output, Offset, Each.Length);
                Offset += Each.Length;
            }

            return new PacketFragment(Output);
        }

        /// <summary>
        /// Join all packets to single packet.
        /// </summary>
        /// <param name="Delimiter"></param>
        /// <param name="Packets"></param>
        /// <returns></returns>
        public static PacketFragment Join(byte Delimiter, params PacketFragment[] Packets) => Join(Delimiter, Packets as IEnumerable<PacketFragment>);

        /// <summary>
        /// Join all packets to single packet.
        /// </summary>
        /// <param name="Delimiter"></param>
        /// <param name="Packets"></param>
        /// <returns></returns>
        public static PacketFragment Join(ArraySegment<byte> Delimiter, IEnumerable<PacketFragment> Packets)
        {
            var Count = Packets.Count();
            var Length = Packets.Sum(X => X.Length) + Delimiter.Count * Math.Max(Count - 1, 0);
            var Output = new byte[Length];
            var Offset = 0;

            foreach (var Each in Packets)
            {
                if (Offset > 0 && Delimiter.Count > 0)
                {
                    Buffer.BlockCopy(Delimiter.Array, Delimiter.Offset, Output, Offset, Delimiter.Count);
                    Offset += Delimiter.Count;
                }

                if (Each.Length <= 0)
                    continue;

                Buffer.BlockCopy(Each.Bytes, Each.Offset, Output, Offset, Each.Length);
                Offset += Each.Length;
            }

            return new PacketFragment(Output);
        }

        /// <summary>
        /// Join all packets to single packet.
        /// </summary>
        /// <param name="Delimiter"></param>
        /// <param name="Packets"></param>
        /// <returns></returns>
        public static PacketFragment Join(ArraySegment<byte> Delimiter, params PacketFragment[] Packets) => Join(Delimiter, Packets as IEnumerable<PacketFragment>);

        /// <summary>
        /// Test whether the other packet is same range with the packet or not.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(PacketFragment other) => Compare(this, other) == 0;

        /// <summary>
        /// Test whether the other object is same ranged packet with the packet or not.
        /// </summary>
        /// <param name="Obj"></param>
        /// <returns></returns>
        public override bool Equals(object Obj)
        {
            if (Obj is PacketFragment Packet)
                return Compare(this, Packet) == 0;

            return base.Equals(Obj);
        }

        /// <summary>
        /// Get enumerator that iterates all bytes stored in packet.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<byte> GetEnumerator()
        {
            for (int i = 0, j = Offset; i < Length; ++j, ++i)
                yield return Bytes[j];
        }

        /// <summary>
        /// Get enumerator that iterates all bytes stored in packet.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Get hash-code of the byte array and its range.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => (Bytes ?? EMPTY_BYTES, Offset, Length).GetHashCode();
    }
}
