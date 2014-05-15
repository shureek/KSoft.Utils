using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace KSoft.IO
{
    public static partial class FileEx
    {
        /// <summary>
        /// Gets file hardlinks count.
        /// </summary>
        /// <param name="filepath">Full filename.</param>
        /// <returns>Number of file hardlinks.</returns>
        public static int GetFileLinkCount(string filepath)
        {
            int result = 0;
            using (var handle = CreateFile(filepath, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite, IntPtr.Zero, System.IO.FileMode.Open, System.IO.FileAttributes.Archive, null))
            {
                if (handle.IsInvalid)
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

                ByHandleFileInformation fileInfo;
                if (GetFileInformationByHandle(handle, out fileInfo))
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
            StringBuilder linkName = new StringBuilder(MaxPath);
            GetVolumePathName(fileFile, linkName, linkName.Capacity);
            string volume = linkName.ToString();
            linkName.Length = 0;
            int stringLength = MaxPath;
            using (var findHandle = FindFirstFileNameW(fileFile, 0, ref stringLength, linkName))
            {
                if (findHandle.IsInvalid)
                    Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());

                do
                {
                    StringBuilder pathSb = new StringBuilder(volume, MaxPath);
                    PathAppend(pathSb, linkName.ToString());
                    result.Add(pathSb.ToString());
                    linkName.Length = 0;
                    stringLength = MaxPath;
                } while (FindNextFileNameW(findHandle, ref stringLength, linkName));
            }
            return result.ToArray();
        }

        #region WinAPI

        /// <summary>
        /// Creates an enumeration of all the hard links to the specified file.
        /// The FindFirstFileNameW function returns a handle to the enumeration that can be used on subsequent calls to the FindNextFileNameW function.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="flags">Reserved; specify zero (0).</param>
        /// <param name="stringLength">The size of the buffer pointed to by the LinkName parameter, in characters. If this call fails and the error returned from the GetLastError function is ERROR_MORE_DATA (234), the value that is returned by this parameter is the size that the buffer pointed to by LinkName must be to contain all the data.</param>
        /// <param name="linkName">A pointer to a buffer to store the first link name found for lpFileName.</param>
        /// <returns>If the function succeeds, the return value is a search handle that can be used with the FindNextFileNameW function or closed with the FindClose function. If the function fails, the return value is INVALID_HANDLE_VALUE (0xffffffff). To get extended error information, call the GetLastError function.</returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern SafeFindHandle FindFirstFileNameW(
            string fileName,
            int flags,
            [In, Out]ref int stringLength,
            [In, Out]StringBuilder linkName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool FindNextFileNameW(
            SafeFindHandle findStream,
            ref int stringLength,
            StringBuilder fileName);

        #endregion
    }
}