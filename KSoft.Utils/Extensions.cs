using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSoft
{
    public static class Extensions
    {
        #region Support Buffer<T> for common classes

        public static int Read(this System.IO.Stream stream, Collections.Buffer<byte> buffer)
        {
            if (buffer.EndOffset == buffer.Capacity)
                throw new ArgumentException("buffer", "Buffer is full");
            int count = stream.Read(buffer.Array, buffer.StartOffset, buffer.Capacity - buffer.EndOffset);
            buffer.EndOffset += count;
            return count;
        }

        public static int Read(this System.IO.TextReader reader, Collections.Buffer<char> buffer)
        {
            if (buffer.EndOffset == buffer.Capacity)
                throw new ArgumentException("buffer", "Buffer is full");
            int count = reader.Read(buffer.Array, buffer.StartOffset, buffer.Capacity - buffer.EndOffset);
            buffer.EndOffset += count;
            return count;
        }

        public static void Convert(this System.Text.Decoder decoder, Collections.Buffer<byte> bytes, Collections.Buffer<char> charBuffer, out int bytesUsed, out int charsUsed, out bool completed)
        {
            decoder.Convert(bytes.Array, bytes.StartOffset, bytes.Count, charBuffer.Array, charBuffer.EndOffset, charBuffer.Capacity - charBuffer.EndOffset, false, out bytesUsed, out charsUsed, out completed);
            bytes.StartOffset += bytesUsed;
            charBuffer.EndOffset += charsUsed;
        }

        public static void Convert(this System.Text.Decoder decoder, Collections.Buffer<byte> bytes, Collections.Buffer<char> charBuffer)
        {
            int bytesUsed;
            int charsUsed;
            bool completed;
            decoder.Convert(bytes, charBuffer, out bytesUsed, out charsUsed, out completed);
        }

        public static void Convert(this System.Text.Decoder decoder, Collections.Buffer<byte> bytes, Collections.Buffer<char> charBuffer, out int bytesUsed, out int charsUsed)
        {
            bool completed;
            decoder.Convert(bytes, charBuffer, out bytesUsed, out charsUsed, out completed);
        }

        #endregion

        #region HexString

        public static string ToHexString(this IEnumerable<byte> bytes, int startIndex, int length, string separator = "", bool upper = false)
        {
            return GetHexString(bytes, startIndex, length, separator, upper);
        }

        public static string ToHexString(this IEnumerable<byte> bytes, int startIndex = 0, string separator = "", bool upper = false)
        {
            return GetHexString(bytes, startIndex, -1, separator, upper);
        }

        static string GetHexString(IEnumerable<byte> bytes, int startIndex, int length, string separator, bool upper)
        {
            if (length == 0)
                return String.Empty;

            int separatorLength = separator == null ? 0 : separator.Length;
            if (bytes is IList<byte>)
            {
                // Если можно обращаться по индексу, то сразу создаем массив char'ов и пишем в него
                var list = (IList<byte>)bytes;
                if (length == -1)
                {
                    length = list.Count - startIndex;
                    if (length == 0)
                        return String.Empty;
                }

                char[] chars = new char[length * 2 + (length - 1) * separatorLength];
                int stringLength = 0;
                for (int i = 0; i < length; i++)
                {
                    if (stringLength > 0 && separatorLength > 0)
                    {
                        separator.CopyTo(0, chars, stringLength, separatorLength);
                        stringLength += separatorLength;
                    }
                    byte b = list[i + startIndex];
                    chars[stringLength] = GetHexValue(b / 16, upper);
                    stringLength++;
                    chars[stringLength] = GetHexValue(b % 16, upper);
                    stringLength++;
                }
                return new String(chars);
            }
            else
            {
                // Если по индексу обращаться нельзя, то пойдем через foreach
                StringBuilder sb;
                if (length > 0)
                    sb = new StringBuilder(length * 2 + (length - 1) * separatorLength);
                else
                    sb = new StringBuilder();

                int i = 0;
                foreach (byte b in bytes)
                {
                    if (i >= startIndex)
                    {
                        if (sb.Length > 0 && separatorLength > 0)
                            sb.Append(separator);
                        sb.Append(GetHexValue(b >> 4, upper));
                        sb.Append(GetHexValue(b & 0x0F, upper));
                    }
                    i++;
                    if (length > 0 && i + startIndex == length)
                        break;
                }
                return sb.ToString();
            }
        }

        private static char GetHexValue(int i, bool upper)
        {
            if (i < 10)
                return (char)(i + 48);
            else
                return (char)(i - 10 + (upper ? 65 : 97));
        }

        #endregion
    }
}
