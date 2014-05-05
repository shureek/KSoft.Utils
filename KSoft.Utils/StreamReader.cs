using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace KSoft.IO
{
    /// <summary>
    /// Implements <see cref="T:System.IO.TextReader" /> that reads characters from a byte stream in a particular encoding.
    /// </summary>
    /// <remarks>
    /// This class is similar to <see cref="T:System.IO.StreamReader" />. In addition, it tracks current line number, char position (absolute and in current line).
    /// Also, it supports ReadTo specific characters and quoted strings.
    /// </remarks>
    public class StreamReader : System.IO.TextReader
    {
        char[] charBuffer;
        int charLen;
        int charPos;

        byte[] byteBuffer;
        int byteLen;
        int bytePos;

        System.IO.Stream stream;
        bool closable;

        bool isBlocked;

        void Init(System.IO.Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen)
        {
            this.stream = stream;
            this.encoding = encoding;
            this.decoder = encoding.GetDecoder();
            this.byteBuffer = new byte[bufferSize];
            this.maxCharsPerBuffer = encoding.GetMaxCharCount(bufferSize);
            this.charBuffer = new char[maxCharsPerBuffer];
            charLen = 0;
            charPos = 0;
            byteLen = 0;
            bytePos = 0;
            detectEncoding = detectEncodingFromByteOrderMarks;
            preamble = encoding.GetPreamble();
            isPreamble = preamble.Length > 0;
            isBlocked = false;
            closable = !leaveOpen;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            return base.Read(buffer, index, count);
        }

        public override int Read()
        {
            return base.Read();
        }

        public override int Peek()
        {
            return base.Peek();
        }

        public override string ReadLine()
        {
            return base.ReadLine();
        }

        public override string ReadToEnd()
        {
            return base.ReadToEnd();
        }

        int ReadBuffer()
        {
            charLen = 0;
            charPos = 0;
            if (!isPreamble)
            {
                bytePos = 0;
                byteLen = 0;
            }

            while(true)
            {
                int num = stream.Read(byteBuffer, bytePos, byteBuffer.Length - bytePos);
                if (num == 0)
                    break;
                byteLen += num;
                isBlocked = byteLen < byteBuffer.Length;
                //if (!SkipPreamble())
                //{

                //}
            }
        }

        #region Encoding handling

        Encoding encoding;
        Decoder decoder;
        bool detectEncoding;
        int maxCharsPerBuffer;

        bool isPreamble;
        byte[] preamble;

        bool CheckPreamble()
        {
            int num = byteLen > preamble.Length ? byteLen - preamble.Length : preamble.Length - byteLen;
            int i = 0;
            while (i < num)
            {
                if (byteBuffer[bytePos] != )
            }
        }

        #endregion
    }
}
