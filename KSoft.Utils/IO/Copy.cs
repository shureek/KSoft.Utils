using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace KSoft.IO
{
    public static partial class FileEx
    {
        /// <summary>
        /// Copies specified file to destination.
        /// </summary>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="destFileName">Destination file name.</param>
        /// <param name="options">CopyOptions.</param>
        /// <param name="cancellationToken">A token for cancelling copy operation.</param>
        /// <param name="progress">IProgress object to get progress notifications on copy operation.</param>
        public static void CopyFile(string sourceFileName, string destFileName, CopyOptions options = CopyOptions.None, CancellationToken? cancellationToken = null, IProgress<CopyProgressInfo> progress = null)
        {
            CopyFileCore(sourceFileName, destFileName, options, cancellationToken, progress);
        }

        /// <summary>
        /// Copies specified file to destination asynchronously.
        /// </summary>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="destFileName">Destination file name.</param>
        /// <param name="options">CopyOptions.</param>
        /// <param name="cancellationToken">A token for cancelling copy operation.</param>
        /// <param name="progress">IProgress object to get progress notifications on copy operation.</param>
        /// <returns>A task to monitor operation progress.</returns>
        public static Task CopyFileAsync(string sourceFileName, string destFileName, CopyOptions options = CopyOptions.None, CancellationToken? cancellationToken = null, IProgress<CopyProgressInfo> progress = null)
        {
            return Task.Factory.StartNew(() =>
            {
                CopyFileCore(sourceFileName, destFileName, options, cancellationToken, progress);
            });
        }

        static void CopyFileCore(string sourceFileName, string destFileName, CopyOptions options, CancellationToken? cancellationToken, IProgress<CopyProgressInfo> progress)
        {
            GCHandle? hProgress = null;
            try
            {
                bool cancelFlag = false;
                if (cancellationToken != null)
                    cancellationToken.Value.Register(() => { cancelFlag = true; });

                CopyProgressRoutine copyCallback = null;
                IntPtr pData = IntPtr.Zero;
                if (progress != null)
                {
                    hProgress = GCHandle.Alloc(progress, GCHandleType.Normal); // для передачи progress через IntPtr
                    pData = GCHandle.ToIntPtr(hProgress.Value);
                    copyCallback = CopyCallbackProc;
                }
                bool ok = CopyFileEx(sourceFileName, destFileName, copyCallback, pData, ref cancelFlag, options);
                if (!ok)
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            finally
            {
                if (hProgress != null)
                    hProgress.Value.Free();
            }
        }

        static CopyCallbackResult CopyCallbackProc(long totalFileSize, long totalBytesTransferred, long streamSize, long streamBytesTransferred, uint streamNumber, CopyEvent callbackReason, IntPtr sourceFileHandle, IntPtr destinationFileHandle, IntPtr pData)
        {
            IProgress<CopyProgressInfo> progress = null;
            if (pData != IntPtr.Zero)
            {
                progress = (IProgress<CopyProgressInfo>)GCHandle.FromIntPtr(pData).Target;
                progress.Report(new CopyProgressInfo(totalFileSize, totalBytesTransferred, streamSize, streamBytesTransferred, streamNumber, callbackReason));
            }
            return CopyCallbackResult.Continue;
        }

        #region WinAPI

        /// <summary>
        /// Copies an existing file to a new file, notifying the application of its progress through a callback function.
        /// </summary>
        /// <param name="existingFileName">The name of an existing file.</param>
        /// <param name="newFileName">The name of the new file.</param>
        /// <param name="progressRoutine">The address of a callback function of type LPPROGRESS_ROUTINE that is called each time another portion of the file has been copied. This parameter can be <value>null</value>.</param>
        /// <param name="data">The argument to be passed to the callback function.</param>
        /// <param name="cancelFlag">If this flag is set to <value>true</value> during the copy operation, the operation is canceled. Otherwise, the copy operation will continue to completion.</param>
        /// <param name="swCopyFlags">Flags that specify how the file is to be copied.</param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CopyFileEx(
            string existingFileName,
            string newFileName,
            CopyProgressRoutine progressRoutine,
            IntPtr data,
            ref bool cancelFlag,
            IO.CopyOptions copyFlags);

        #endregion
    }

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
        CopyEvent callbackReason,
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