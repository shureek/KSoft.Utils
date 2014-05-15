using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace KSoft.IO
{
    public static partial class FileEx
    {
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
            lowOrder = GetCompressedFileSize(fileName, out highOrder);
            if (lowOrder == InvalidFileSize)
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            return ((long)highOrder << 32) + lowOrder;
        }

        #region WinAPI

        const int MaxPath = 260;
        const int MaxLongPath = 0x8000;
        const uint InvalidFileSize = 0xFFFFFFFF;

        /// <summary>
        /// Creates or opens a file or I/O device.
        /// </summary>
        /// <param name="fileName">The name of the file or device to be created or opened. You may use either forward slashes (/) or backslashes (\) in this name.</param>
        /// <param name="desiredAccess">The requested access to the file or device, which can be summarized as read, write, both or neither (zero).</param>
        /// <param name="shareMode">The requested sharing mode of the file or device, which can be read, write, both, delete, all of these, or none.</param>
        /// <param name="securityAttributes">A pointer to a SECURITY_ATTRIBUTES structure that contains two separate but related data members: an optional security descriptor, and a Boolean value that determines whether the returned handle can be inherited by child processes. This parameter can be <value>null</value>.</param>
        /// <param name="creationDisposition">An action to take on a file or device that exists or does not exist.</param>
        /// <param name="flagsAndAttributes">The file or device attributes and flags, FILE_ATTRIBUTE_NORMAL being the most common default value for files.</param>
        /// <param name="templateFile">A valid handle to a template file with the GENERIC_READ access right. The template file supplies file attributes and extended attributes for the file that is being created. This parameter can be <value>null</value>.</param>
        /// <returns>If the function succeeds, the return value is an open handle to the specified file, device, named pipe, or mail slot. If the function fails, the return value is INVALID_HANDLE_VALUE. To get extended error information, call GetLastError.</returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern SafeFileHandle CreateFile(
            string fileName,
            System.IO.FileAccess desiredAccess,
            System.IO.FileShare shareMode,
            IntPtr securityAttributes,
            System.IO.FileMode creationDisposition,
            System.IO.FileAttributes flagsAndAttributes,
            SafeFileHandle templateFile);

        /// <summary>
        /// Retrieves file information for the specified file.
        /// </summary>
        /// <param name="handle">A handle to the file that contains the information to be retrieved. This handle should not be a pipe handle.</param>
        /// <param name="fileInformation">A pointer to a BY_HANDLE_FILE_INFORMATION structure that receives the file information.</param>
        /// <returns>If the function succeeds, the return value is nonzero and file information data is contained in the buffer pointed to by the lpFileInformation parameter.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetFileInformationByHandle(SafeFileHandle handle, out ByHandleFileInformation fileInfo);

        /// <summary>
        /// Retrieves the volume mount point where the specified path is mounted.
        /// </summary>
        /// <remarks>
        /// For example, assume that you have volume D mounted at C:\Mnt\Ddrive and volume E mounted at "C:\Mnt\Ddrive\Mnt\Edrive". Also assume that you have a file with the path <value>"E:\Dir\Subdir\MyFile"</value>. If you pass <value>"C:\Mnt\Ddrive\Mnt\Edrive\Dir\Subdir\MyFile"</value> to GetVolumePathName, it returns the path <value>"C:\Mnt\Ddrive\Mnt\Edrive\"</value>.
        /// </remarks>
        /// <param name="fileName">A pointer to the input path string. Both absolute and relative file and directory names, for example "..", are acceptable in this path.</param>
        /// <param name="volumePathName">A pointer to a string that receives the volume mount point for the input path.</param>
        /// <param name="bufferLength">The length of the output buffer, in TCHARs.</param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool GetVolumePathName(string fileName, [Out] StringBuilder volumePathName, int bufferLength);

        /// <summary>
        /// Appends one path to the end of another.
        /// </summary>
        /// <remarks>
        /// This function automatically inserts a backslash between the two strings, if one is not already present.
        /// The path supplied in pszPath cannot begin with "..\\" or ".\\" to produce a relative path string. If present,
        /// those periods are stripped from the output string. For example, appending "path3" to "..\\path1\\path2" results
        /// in an output of "\path1\path2\path3" rather than "..\path1\path2\path3".
        /// Misuse of this function can lead to a buffer overrun. We recommend the use of the safer PathCchAppend or PathCchAppendEx function in its place.
        /// </remarks>
        /// <param name="path">A pointer to a null-terminated string to which the path specified in pszMore is appended.</param>
        /// <param name="more">A pointer to a null-terminated string of maximum length MAX_PATH that contains the path to be appended.</param>
        /// <returns>Returns <value>true</value> if successful, or <value>false</value> otherwise.</returns>
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        static extern bool PathAppend([In, Out] StringBuilder path, string more);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint GetCompressedFileSize(string fileName, out uint fileSizeHigh);

        #endregion
    }
}