using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using KSoft.Collections;

namespace KSoft.IO
{
    /// <summary>
    /// Implements <see cref="T:System.IO.TextReader" /> that reads characters from a byte stream in a particular encoding.
    /// </summary>
    /// <remarks>
    /// This class is similar to <see cref="T:System.IO.StreamReader" />. In addition, it tracks current line number, char position (absolute and in current line) and byte position (absolute).
    /// Also, it supports ReadTo specific characters and quoted strings.
    /// </remarks>
    public class StreamReader : System.IO.TextReader
    {
        Buffer<char> charBuffer;
        Buffer<byte> byteBuffer;

        System.IO.Stream stream;
        bool closable;
        int byteDelta;
        long bytePosition;
        long charPosition;

        bool beginOfStream;

        bool isBlocked;

        public StreamReader(int charBufferSize = 2048)
        {
            if (charBufferSize < 1)
                throw new ArgumentOutOfRangeException("charBufferSize", "Buffer size must be positive");
            charBuffer = new Buffer<char>(charBufferSize);
            byteBuffer = new Buffer<byte>(); // capacity will be set when encoding is specified
        }

        /// <summary>
        /// Opens stream for reading.
        /// </summary>
        /// <param name="stream">Stream to open.</param>
        /// <param name="encoding">If not specified, encoding will be automatically detected.</param>
        /// <param name="leaveOpen">If <value>false</value>, stream will be closed when Close method will be called.</param>
        public void Open(System.IO.Stream stream, Encoding encoding = null, bool leaveOpen = true)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            this.stream = stream;

            if (encoding != null)
                SetEncoding(encoding);

            beginOfStream = true;
            isBlocked = false;
            closable = !leaveOpen;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            return base.Read(buffer, index, count);
        }

        public override int Read()
        {
            if (!FillBuffer())
                return -1;
            int result = (int)charBuffer[0];
            charBuffer.StartOffset++;
            return result;
        }

        public override int Peek()
        {
            if (!FillBuffer())
                return -1;
            return (int)charBuffer[0];
        }

        public override string ReadLine()
        {
            return base.ReadLine();
        }

        public override string ReadToEnd()
        {
            return base.ReadToEnd();
        }

        bool FillBuffer()
        {
            if (charBuffer.Count == 0)
            {
                if (byteBuffer.Count == 0)
                {
                    byteBuffer.Clear();
                    stream.Read(byteBuffer);
                }
                if (byteBuffer.Count > 0)
                {
                    int bytesUsed;
                    int charsUsed;
                    decoder.Convert(byteBuffer, charBuffer, out bytesUsed, out charsUsed);
                }
            }
            return charBuffer.Count > 0;
        }

        [DebuggerHidden]
        void EnsureNotClosed()
        {
            if (stream == null)
                throw new ObjectDisposedException(null, "Reader is closed");
        }

        #region Encoding handling

        Encoding encoding;
        Decoder decoder;

        /// <summary>
        /// Skips encoding preamble bytes if they match.
        /// </summary>
        void SkipPreamble()
        {
            var preamble = encoding.GetPreamble();
            if (preamble.Length > byteBuffer.Count)
            {
                Debug.WriteLine(String.Format("ByteLen ({0}) < Preamble.Length ({1})", byteBuffer.Count, preamble.Length));
                return;
            }
            int i = 0;
            while (i < preamble.Length)
            {
                if (byteBuffer[i] != preamble[i])
                    return;
            }
            byteBuffer.StartOffset += i;
        }

        public Encoding CurrentEncoding
        {
            get { return encoding; }
        }

        void SetEncoding(Encoding encoding)
        {
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            this.encoding = encoding;
            decoder = encoding.GetDecoder();
            byteBuffer.SetCapacity(GetOptimalByteBufferSize());
        }

        /// <summary>
        /// Finds maximum byte buffer size for which encoding.GetMaxCharCount() is less or equal to char buffer size.
        /// </summary>
        /// <returns>Optimal byte buffer size.</returns>
        int GetOptimalByteBufferSize()
        {
            int bytesCount = encoding.GetMaxByteCount(charBuffer.Count);
            while (encoding.GetMaxCharCount(bytesCount) > charBuffer.Count)
                bytesCount--;
            return bytesCount;
        }

        void DetectEncoding()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
