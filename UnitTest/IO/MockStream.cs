using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UnitTest.IO
{
    /// <summary>
    /// This is a "mock" stream.
    /// Reads random bytes and writes to nowhere.
    /// </summary>
    public class MockStream : Stream
    {
        protected readonly bool canRead;
        protected readonly bool canSeek;
        protected readonly bool canWrite;
        protected long position = 0;
        protected long length;
        
        public MockStream(bool canRead = true, bool canSeek = true, bool canWrite = true, long length = -1L)
        {
            this.canRead = canRead;
            this.canSeek = canSeek;
            this.canWrite = canWrite;
            this.length = length;
        }

        public override bool CanRead
        {
            get { return canRead; }
        }

        public override bool CanSeek
        {
            get { return canSeek; }
        }

        public override bool CanWrite
        {
            get { return canWrite; }
        }

        public override long Position
        {
            get
            {
                return position;
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!CanSeek)
                throw new NotSupportedException("This stream is not seekable");

            switch(origin)
            {
                case SeekOrigin.Begin:
                    position = offset;
                    break;
                case SeekOrigin.Current:
                    position += offset;
                    break;
                case SeekOrigin.End:
                    position = Length + offset;
                    break;
            }

            return Position;
        }
        
        public override void Flush()
        { }

        public override long Length
        {
	        get
            {
                if (length >= 0)
                    return length;
                else
                    throw new NotSupportedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
 	        if (!CanRead)
                throw new NotSupportedException("This stream is not readable");
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0 || count < 0)
                throw new ArgumentOutOfRangeException(offset < 0 ? "offset" : "count", "offset and count must not be negative");
            if (offset + count > buffer.Length)
                throw new ArgumentException("The sum of offset and count is larger than the buffer length");

            var rnd = new Random();
            int readBytes = Math.Min(rnd.Next(count * 3), count); // we'll read count (67%) or less bytes
            if (length >= 0 && Position + readBytes > length) // we cannot read beyond end of stream
                readBytes = (int)(length - Position);

            if (readBytes > 0)
            {
                if (offset == 0 && readBytes == buffer.Length)
                    rnd.NextBytes(buffer);
                else
                {
                    byte[] bytes = new byte[readBytes];
                    rnd.NextBytes(bytes);
                    Array.Copy(bytes, 0, buffer, offset, readBytes);
                }
            }
            return readBytes;
        }

        public override void SetLength(long value)
        {
            if (!CanWrite)
                throw new NotSupportedException("This stream is not writable");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0 || count < 0)
                throw new ArgumentOutOfRangeException(offset < 0 ? "offset" : "count", "offset and count must not be negative");
            if (offset + count > buffer.Length)
                throw new ArgumentException("The sum of offset and count is larger than the buffer length");

            if (!CanWrite)
                throw new NotSupportedException("This stream is not writable");
        }
    }

    public class MockCollectionStream : MockStream
    {
        IEnumerable<byte> bytes;

        public MockCollectionStream(IEnumerable<byte> bytes, bool canRead = true, bool canSeek = true, bool canWrite = true)
            : base(canRead, canSeek, canWrite)
        {
            this.bytes = bytes;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get
            {
                var collection = bytes as ICollection<byte>;
                if (collection == null)
                    throw new NotSupportedException();
                return collection.Count;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!CanRead)
                throw new NotSupportedException("This stream is not readable");

            if (bytes is IList<byte>)
            {
                var list = (IList<byte>)bytes;
                int readBytes = Math.Min(count, (int)(Length - Position));
                for (int i = 0; i < readBytes; i++)
                    buffer[offset + i] = list[(int)Position + offset];
                position += readBytes;
                return readBytes;
            }
            else
                throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            if (!CanWrite)
                throw new NotSupportedException("This stream is not writable");

            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!CanWrite)
                throw new NotSupportedException("This stream is not writable");

            if (bytes is List<byte> && offset == 0 && count == buffer.Length)
                ((List<byte>)bytes).AddRange(buffer);
            else if (bytes is IList<byte>)
            {
                var list = (IList<byte>)bytes;
                for (int i = 0; i < count; i++)
                    list.Add(buffer[offset + i]);
            }
            else
                throw new NotImplementedException();
        }
    }
}
