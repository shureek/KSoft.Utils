using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace KSoft.IO
{
    public class DirectoryEx
    {
        public IEnumerable<string> EnumerateFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            IFindResultHandler<string> findResultHandler = new StringFindResultHandler(true, false);
            return EnumerateFiles(path, searchPattern, searchOption == SearchOption.AllDirectories, findResultHandler, false);
        }

        IEnumerable<TResult> EnumerateFiles<TResult>(string path, string pattern, bool recursive, IFindResultHandler<TResult> findResultHandler, bool caseSensitive)
        {
            string filename = Path.Combine(path, pattern);
            Win32FindData findData = new Win32FindData();
            SearchAdditionalFlags flags = SearchAdditionalFlags.LargeFetch;
            if (caseSensitive)
                flags |= SearchAdditionalFlags.CaseSensitive;

            List<string> directories = null;
            if (recursive)
                directories = new List<string>();

            using (SafeFindHandle findHandle = FindFirstFileEx(filename, FIndexInfoLevels.Basic, findData, FIndexSearchOps.NameMatch, IntPtr.Zero, flags))
            {
                bool ok = !findHandle.IsInvalid;
                while (ok)
                {
                    if (findResultHandler.IsResultOK(path, findData))
                        yield return findResultHandler.GetResult(path, findData);
                    if (recursive && (findData.FileAttributes & FileAttributes.Directory) != 0)
                        directories.Add(findData.FileName);
                    ok = FindNextFile(findHandle, findData);
                }
                int errorCode = Marshal.GetLastWin32Error();
                if (!(errorCode == ERROR_FILE_NOT_FOUND || errorCode == ERROR_NO_MORE_FILES || errorCode == ERROR_PATH_NOT_FOUND))
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            if (recursive)
            {
                foreach (string directory in directories)
                    foreach (TResult innerFile in EnumerateFiles<TResult>(Path.Combine(path, directory), pattern, true, findResultHandler, caseSensitive))
                        yield return innerFile;
            }
        }

        interface IFindResultHandler<TResult>
        {
            bool IsResultOK(string path, Win32FindData findData);
            TResult GetResult(string path, Win32FindData findData);
        }

        class StringFindResultHandler : IFindResultHandler<string>
        {
            bool includeFiles;
            bool includeDirectories;

            public StringFindResultHandler(bool includeFiles, bool includeDirectories)
            {
                this.includeFiles = includeFiles;
                this.includeDirectories = includeDirectories;
            }

            public bool IsResultOK(string path, Win32FindData findData)
            {
                if ((findData.FileAttributes & FileAttributes.Directory) != 0)
                    return includeDirectories;
                else
                    return includeFiles;
            }

            public string GetResult(string path, Win32FindData findData)
            {
                return findData.FileName;
            }
        }

        class FSIFindResultHandler : IFindResultHandler<FileSystemInfo>
        {
            bool includeFiles;
            bool includeDirectories;

            public FSIFindResultHandler(bool includeFiles, bool includeDirectories)
            {
                this.includeFiles = includeFiles;
                this.includeDirectories = includeDirectories;
            }

            public bool IsResultOK(string path, Win32FindData findData)
            {
                if ((findData.FileAttributes & FileAttributes.Directory) != 0)
                    return includeDirectories;
                else
                    return includeFiles;
            }

            public FileSystemInfo GetResult(string path, Win32FindData findData)
            {
                string filename = Path.Combine(path, findData.FileName);
                if ((findData.FileAttributes & FileAttributes.Directory) != 0)
                    return new DirectoryInfo(filename);
                else
                    return new FileInfo(filename);
            }
        }

        #region WinAPI

        const int ERROR_FILE_NOT_FOUND = 2;
        const int ERROR_PATH_NOT_FOUND = 3;
        const int ERROR_NO_MORE_FILES = 18;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false)]
        static extern SafeFindHandle FindFirstFileEx(string fileName, FIndexInfoLevels infoLevel, [In, Out]Win32FindData fineFileData, FIndexSearchOps searchOp, IntPtr searchFilter, SearchAdditionalFlags flags);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false)]
        static extern bool FindNextFile(SafeFindHandle findHandle, [In, Out]Win32FindData findFileData);

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto), BestFitMapping(false)]
        class Win32FindData
        {
            public FileAttributes FileAttributes;
            public ComTypes.FILETIME CreationTime;
            public ComTypes.FILETIME LastAccessTime;
            public ComTypes.FILETIME LastWriteTime;
            public long FileSize;
            long reserved;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string FileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string AlternateFileName;
        }

        enum FIndexInfoLevels
        {
            /// <summary>
            /// Standard set of attribute information.
            /// </summary>
            Standard,
            /// <summary>
            /// Does not query short file names.
            /// </summary>
            Basic
        }

        enum FIndexSearchOps
        {
            /// <summary>
            /// The search for a file that matches a specified file name.
            /// </summary>
            NameMatch,
            /// <summary>
            /// If the file system supports directory filtering, the functions searches for directories only.
            /// </summary>
            /// <remarks>
            /// The searchFilter parameter must be <value>null</value> when this search value is used.
            /// </remarks>
            LimitToDirectories
        }

        [Flags]
        enum SearchAdditionalFlags
        {
            None = 0,
            /// <summary>
            /// Searches are case-sensitive.
            /// </summary>
            CaseSensitive = 1,
            /// <summary>
            /// Uses larger buffer for directory queries, which can increase performance of the find operation.
            /// </summary>
            LargeFetch = 2
        }

        #endregion
    }
}
