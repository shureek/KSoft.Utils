using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.IO;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace KSoft.Native
{
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    public static class WinAPI
    {
        public const int MaxPath = 260;
        public const int MaxLongPath = 0x8000;
        public const uint InvalidFileSize = 0xFFFFFFFF;

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
        public static extern SafeFileHandle CreateFile(
            string fileName,
            FileAccess desiredAccess,
            FileShare shareMode,
            IntPtr securityAttributes,
            FileMode creationDisposition,
            FileAttributes flagsAndAttributes,
            SafeFileHandle templateFile);

        /// <summary>
        /// Retrieves file information for the specified file.
        /// </summary>
        /// <param name="handle">A handle to the file that contains the information to be retrieved. This handle should not be a pipe handle.</param>
        /// <param name="fileInformation">A pointer to a BY_HANDLE_FILE_INFORMATION structure that receives the file information.</param>
        /// <returns>If the function succeeds, the return value is nonzero and file information data is contained in the buffer pointed to by the lpFileInformation parameter.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetFileInformationByHandle(SafeFileHandle handle, out ByHandleFileInformation fileInfo);

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
        public static extern SafeFindHandle FindFirstFileNameW(
            string fileName,
            int flags,
            [In, Out]ref int stringLength,
            [In, Out]StringBuilder linkName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool FindNextFileNameW(
            SafeFindHandle findStream,
            ref int stringLength,
            StringBuilder fileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FindClose(SafeFindHandle findHandle);

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
        public static extern bool GetVolumePathName(string fileName, [Out] StringBuilder volumePathName, int bufferLength);

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
        public static extern bool PathAppend([In, Out] StringBuilder path, string more);

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

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetCompressedFileSize(string fileName, out uint fileSizeHigh);
    }
}
