using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO = System.IO;
using System.Diagnostics;

namespace KSoft.Utils
{
    /// <summary>
    /// Implements <see cref="T:System.IO.TextReader" /> that reads characters from a byte stream in a particular encoding.
    /// </summary>
    /// <remarks>
    /// This class is similar to <see cref="T:System.IO.StreamReader" />. In addition, it tracks current line number, char position (absolute and in current line).
    /// Also, it supports ReadTo specific characters and quoted strings.
    /// </remarks>
    public sealed class StreamReader : IO.TextReader
    {
        char[] charBuffer;
        int charLen;
        int charPos;

        byte[] byteBuffer;
        int byteLen;
        int bytePos;

        IO.Stream stream;
        bool closable;

        Encoding encoding;
        Decoder decoder;
        bool detectEncoding;
        int maxCharsPerBuffer;

        bool checkPreamble;
        byte[] preamble;

        bool isBlocked;

        void Init(IO.Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen)
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
            checkPreamble = preamble.Length > 0;
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
            if (!checkPreamble)
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
                if (!IsPreamble())
                {

                }
            }
        }

        bool IsPreamble()
        {
            if (checkPreamble)
            {
                int num = byteLen > 
            }
        }
    }
}
