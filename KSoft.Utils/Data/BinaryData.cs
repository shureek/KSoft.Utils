using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSoft.Data
{
    /// <summary>
    /// Двоичные данные. Например, хэш.
    /// </summary>
    public class BinaryData : IEquatable<BinaryData>, IComparable<BinaryData>
    {
        uint[] bits;
        int bytesCount;

        public int Length
        {
            get { return bytesCount; }
        }

        public BinaryData(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            bytesCount = bytes.Length;
            int tail = bytesCount % 4;
            int intsCount = bytesCount / 4 + ((tail == 0) ? 0 : 1);
            bits = new uint[intsCount];
            int j = 3;
            int k;
            for (k = 0; k < intsCount - ((tail == 0) ? 0 : 1); k++)
            {
                for (int l = 0; l < 4; l++)
                {
                    bits[k] <<= 8;
                    bits[k] |= (uint)bytes[j];
                    j--;
                }
                j += 8;
            }
            if (tail != 0)
            {
                for (j = bytesCount - 1; j >= bytesCount - tail; j--)
                {
                    bits[k] <<= 8;
                    bits[k] |= (uint)bytes[j];
                }
            }
        }

        public byte[] ToByteArray()
        {
            if (this.bits == null)
                return new byte[0];

            uint[] array = this.bits;
            byte b;
            b = 0;
            byte[] array2 = new byte[checked(4 * array.Length)];
            int num = 0;
            for (int i = 0; i < array.Length; i++)
            {
                uint num2 = array[i];
                for (int j = 0; j < 4; j++)
                {
                    array2[num++] = (byte)(num2 & 255u);
                    num2 >>= 8;
                }
            }
            int num3 = array2.Length - 1;
            while (num3 > 0 && array2[num3] == b)
            {
                num3--;
            }
            byte[] array3 = new byte[num3 + 1];
            Array.Copy(array2, array3, num3 + 1);
            return array3;
        }

        public override string ToString()
        {
            var chars = new char[bytesCount * 2];
            for (int i = 0; i < bytesCount; i++)
            {
                uint b = (bits[i / 4] >> (i % 4 * 8)) & 0xFF;

                chars[i * 2] = GetHexValue((b & 0xF0) >> 4);
                chars[i * 2 + 1] = GetHexValue(b & 0x0F);
            }
            return new String(chars);
        }

        static char GetHexValue(uint b)
        {
            return (char)(b + (b > 9 ? 55 : 48));
        }

        public bool Equals(BinaryData other)
        {
            if (this.bits != null && other.bits != null)
            {
                if (this.bytesCount == other.bytesCount)
                {
                    for (int i = 0; i < this.bits.Length; i++)
                        if (this.bits[i] != other.bits[i])
                            return false;
                    return true;
                }
                else
                    return false;
            }
            else
                return (this.bits == null && other.bits == null);
        }

        public override bool Equals(object obj)
        {
            return obj is BinaryData ? Equals((BinaryData)obj) : false;
        }

        public override int GetHashCode()
        {
            if (bits == null)
                return 0;

            int hash = 0;
            int i = bits.Length;
            while (--i >= 0)
                hash = CombineHash(hash, (int)bits[i]);
            return hash;
        }

        static int CombineHash(int n1, int n2)
        {
            return (n1 << 7 | n1 >> 25) ^ n2;
        }

        static uint CombineHash(uint n1, uint n2)
        {
            return (uint)CombineHash((int)n1, (int)n2);
        }

        public int CompareTo(BinaryData other)
        {
            if (this.bits != null && other.bits != null)
            {
                for (int i = 0; i < Math.Min(this.bits.Length, other.bits.Length); i++)
                {
                    if (this.bits[i] > other.bits[i])
                        return 1;
                    else if (this.bits[i] < other.bits[i])
                        return -1;
                }
                if (this.bytesCount > other.bytesCount)
                    return 1;
                else if (this.bytesCount < other.bytesCount)
                    return -1;
                else
                    return 0;
            }
            else if (this.bits == null && other.bits == null)
                return 0;
            else if (this.bits == null)
                return -1;
            else
                return 1;
        }
    }
}
