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
    }
}
