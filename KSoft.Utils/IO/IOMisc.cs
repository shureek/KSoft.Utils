using System;

namespace KSoft.IO
{
    public class CopyProgressInfo
    {
        /// <summary>
        /// The total size of the file, in bytes.
        /// </summary>
        public readonly long TotalFileSize;
        /// <summary>
        /// The total number of bytes transferred from the source file to the destination file since the copy operation began.
        /// </summary>
        public readonly long TotalBytesTransferred;
        /// <summary>
        /// The total size of the current file stream, in bytes.
        /// </summary>
        public readonly long StreamSize;
        /// <summary>
        /// The total number of bytes in the current stream that have been transferred from the source file to the destination file since the copy operation began.
        /// </summary>
        public readonly long StreamBytesTransferred;
        /// <summary>
        /// A handle to the current stream. The first time CopyProgressRoutine is called, the stream number is 1.
        /// </summary>
        public readonly uint StreamNumber;
        /// <summary>
        /// Event occured during copying routine.
        /// </summary>
        public readonly CopyEvent CopyEvent;

        public CopyProgressInfo(long totalFileSize, long totalBytesTransferred, long streamSize, long streamBytesTransferred, uint streamNumber, CopyEvent copyEvent)
        {
            this.TotalFileSize = totalFileSize;
            this.TotalBytesTransferred = totalBytesTransferred;
            this.StreamSize = streamSize;
            this.StreamBytesTransferred = streamBytesTransferred;
            this.StreamNumber = streamNumber;
            this.CopyEvent = copyEvent;
        }
    }

    public enum CopyEvent : uint
    {
        /// <summary>
        /// Another part of the data file was copied.
        /// </summary>
        ChunkFinished = 0x00000000,
        /// <summary>
        /// Another stream was created and is about to be copied. This is the callback reason given when the callback routine is first invoked.
        /// </summary>
        StreamSwitch = 0x00000001
    }

    [Flags]
    public enum CopyOptions : uint
    {
        None = 0x00000000,
        /// <summary>
        /// An attempt to copy an encrypted file will succeed even if the destination copy cannot be encrypted.
        /// </summary>
        AllowDecryptedDestination = 0x00000008,
        /// <summary>
        /// If the source file is a symbolic link, the destination file is also a symbolic link pointing to the same file that the source symbolic link is pointing to.
        /// </summary>
        CopySymlink = 0x00000800,
        /// <summary>
        /// The copy operation fails immediately if the target file already exists.
        /// </summary>
        FailIfExists = 0x00000001,
        /// <summary>
        /// The copy operation is performed using unbuffered I/O, bypassing system I/O cache resources. Recommended for very large file transfers.
        /// </summary>
        NoBuffering = 0x00001000,
        /// <summary>
        /// The file is copied and the original file is opened for write access.
        /// </summary>
        OpenSourceForWrite = 0x00000004,
        /// <summary>
        /// Progress of the copy is tracked in the target file in case the copy fails. The failed copy can be restarted at a later time by specifying the same values for lpExistingFileName and lpNewFileName as those used in the call that failed. This can significantly slow down the copy operation as the new file may be flushed multiple times during the copy operation.
        /// </summary>
        Restartable = 0x00000002
    }
}