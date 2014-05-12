using System;
using System.IO;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32.SafeHandles;

namespace KSoft.Native
{
    public class SafeFindHandle : SafeHandleMinusOneIsInvalid
    {
        public SafeFindHandle()
            : base(true)
        { }

        public SafeFindHandle(IntPtr preexistingHandle, bool ownsHandle)
            : base(ownsHandle)
        {
            SetHandle(preexistingHandle);
        }

        protected override bool ReleaseHandle()
        {
            return WinAPI.FindClose(this);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ByHandleFileInformation
    {
        public FileAttributes FileAttributes;
        public ComTypes.FILETIME CreationTime;
        public ComTypes.FILETIME LastAccessTime;
        public ComTypes.FILETIME LastWriteTime;
        public uint VolumeSerialNumber;
        //public uint FileSizeHigh;
        //public uint FileSizeLow;
        public ulong FileSize;
        public int NumberOfLinks;
        //public uint FileIndexHigh;
        //public uint FileIndexLow;
        public ulong FileIndex;
    }

    /// <summary>
    /// An application-defined callback function used with the CopyFileEx, MoveFileTransacted, and MoveFileWithProgress functions. It is called when a portion of a copy or move operation is completed.
    /// </summary>
    /// <param name="totalFileSize">The total size of the file, in bytes.</param>
    /// <param name="totalBytesTransferred">The total number of bytes transferred from the source file to the destination file since the copy operation began.</param>
    /// <param name="streamSize">The total size of the current file stream, in bytes.</param>
    /// <param name="streamBytesTransferred">The total number of bytes in the current stream that have been transferred from the source file to the destination file since the copy operation began.</param>
    /// <param name="streamNumber">A handle to the current stream. The first time CopyProgressRoutine is called, the stream number is 1.</param>
    /// <param name="callbackReason">The reason that CopyProgressRoutine was called.</param>
    /// <param name="sourceFileHandle">A handle to the source file.</param>
    /// <param name="destinationFileHandle">A handle to the destination file.</param>
    /// <param name="pData">Argument passed to CopyProgressRoutine by CopyFileEx, MoveFileTransacted, or MoveFileWithProgress.</param>
    /// <returns>An action to perform on current operation.</returns>
    public delegate CopyCallbackResult CopyProgressRoutine(
        long totalFileSize,
        long totalBytesTransferred,
        long streamSize,
        long streamBytesTransferred,
        uint streamNumber,
        IO.CopyEvent callbackReason,
        IntPtr sourceFileHandle,
        IntPtr destinationFileHandle,
        IntPtr pData);

    public enum CopyCallbackResult : uint
    {
        /// <summary>
        /// Continue the copy operation.
        /// </summary>
        Continue = 0,
        /// <summary>
        /// Cancel the copy operation and delete the destination file.
        /// </summary>
        Cancel = 1,
        /// <summary>
        /// Stop the copy operation. It can be restarted at a later time.
        /// </summary>
        Stop = 2,
        /// <summary>
        /// Continue the copy operation, but stop invoking CopyProgressRoutine to report progress.
        /// </summary>
        Quiet = 3
    }
}