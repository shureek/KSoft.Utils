using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using KSoft.Native;

namespace KSoft.IO
{
    public static class File
    {
        #region HardLink methods

        /// <summary>
        /// Gets file hardlinks count.
        /// </summary>
        /// <param name="filepath">Full filename.</param>
        /// <returns>Number of file hardlinks.</returns>
        public static int GetFileLinkCount(string filepath)
        {
            int result = 0;
            using (var handle = WinAPI.CreateFile(filepath, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite, IntPtr.Zero, System.IO.FileMode.Open, System.IO.FileAttributes.Archive, null))
            {
                if (handle.IsInvalid)
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

                ByHandleFileInformation fileInfo;
                if (WinAPI.GetFileInformationByHandle(handle, out fileInfo))
                    result = fileInfo.NumberOfLinks;
                else
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            return result;
        }

        /// <summary>
        /// Gets all file hardlinks.
        /// </summary>
        /// <param name="fileFile">Full filename.</param>
        /// <returns>Array of file hardlinks.</returns>
        public static string[] GetFileSiblingHardLinks(string fileFile)
        {
            List<string> result = new List<string>();
            StringBuilder linkName = new StringBuilder(WinAPI.MaxPath);
            WinAPI.GetVolumePathName(fileFile, linkName, linkName.Capacity);
            string volume = linkName.ToString();
            linkName.Length = 0;
            int stringLength = WinAPI.MaxPath;
            using (var findHandle = WinAPI.FindFirstFileNameW(fileFile, 0, ref stringLength, linkName))
            {
                if (findHandle.IsInvalid)
                    Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());

                do
                {
                    StringBuilder pathSb = new StringBuilder(volume, WinAPI.MaxPath);
                    WinAPI.PathAppend(pathSb, linkName.ToString());
                    result.Add(pathSb.ToString());
                    linkName.Length = 0;
                    stringLength = WinAPI.MaxPath;
                } while (WinAPI.FindNextFileNameW(findHandle, ref stringLength, linkName));
            }
            return result.ToArray();
        }

        #endregion

        #region Copy methods

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
                bool ok = WinAPI.CopyFileEx(sourceFileName, destFileName, copyCallback, pData, ref cancelFlag, options);
                if (!ok)
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            finally
            {
                if (hProgress != null)
                    hProgress.Value.Free();
            }
        }

        static CopyCallbackResult CopyCallbackProc(long totalFileSize, long totalBytesTransferred, long streamSize, long streamBytesTransferred, uint streamNumber, IO.CopyEvent callbackReason, IntPtr sourceFileHandle, IntPtr destinationFileHandle, IntPtr pData)
        {
            IProgress<CopyProgressInfo> progress = null;
            if (pData != IntPtr.Zero)
            {
                progress = (IProgress<CopyProgressInfo>)GCHandle.FromIntPtr(pData).Target;
                progress.Report(new CopyProgressInfo(totalFileSize, totalBytesTransferred, streamSize, streamBytesTransferred, streamNumber, callbackReason));
            }
            return CopyCallbackResult.Continue;
        }

        #endregion

        #region Other methods

        /// <summary>
        /// Retrieves the actual number of bytes of disk storage used to store a specified file.
        /// </summary>
        /// <remarks>
        /// If the file is located on a volume that supports compression and the file is compressed, the value obtained is the compressed size of the specified file.
        /// If the file is located on a volume that supports sparse files and the file is a sparse file, the value obtained is the sparse size of the specified file.
        /// </remarks>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>Compressed file size.</returns>
        public static long GetCompressedFileSize(string fileName)
        {
            uint highOrder;
            uint lowOrder;
            lowOrder = WinAPI.GetCompressedFileSize(fileName, out highOrder);
            if (lowOrder == WinAPI.InvalidFileSize)
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            return ((long)highOrder << 32) + lowOrder;
        }

        #endregion
    }
}