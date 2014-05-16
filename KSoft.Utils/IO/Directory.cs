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
            System.IO.Directory.EnumerateFileSystemEntries()
        }

        #region FileSystemInfo enumerator

        class FSICollection<T> : IEnumerable<T>
        {
            public IEnumerator<T> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        class FSIEnumerator<T> : IEnumerator<T>
        {
            Win32FindData findData;
            int state;
            SafeFindHandle findHandle;

            SearchAdditionalFlags searchFlags;
            string path;
            string pattern;

            public FSIEnumerator(string path, string pattern, bool caseSensitive = false, bool largeFetch = false)
            {
                this.path = path;
                this.pattern = pattern;

                searchFlags = 0;
                if (caseSensitive)
                    searchFlags |= SearchAdditionalFlags.CaseSensitive;
                if (largeFetch)
                    searchFlags |= SearchAdditionalFlags.LargeFetch;
            }

            public T Current
            {
                get
                {
                    if (state != 1)
                        throw new InvalidOperationException();

                    if (typeof(T) == typeof(string))
                        return findData.FileName as T;
                }
            }

            public void Dispose()
            { }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                if (state > 1)
                    return false;

                bool ok;

                do
                {
                    if (state == 0)
                    {
                        string filename = Path.Combine(path, pattern);
                        findHandle = FindFirstFileEx(filename, FIndexInfoLevels.Basic, findData, FIndexSearchOps.NameMatch, IntPtr.Zero, searchFlags);
                        ok = !findHandle.IsInvalid;
                        state = 1;
                    }
                    else
                        ok = FindNextFile(findHandle, findData);

                    if (ok)
                    {
                        //TODO: Check result
                        return true;
                    }
                    else
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        if (errorCode == ERROR_FILE_NOT_FOUND || errorCode == ERROR_NO_MORE_FILES || errorCode == ERROR_PATH_NOT_FOUND)
                        {
                            // Search finished
                            state = 2;
                            return false;
                        }
                    }
                }
                while (true);
            }

            public void Reset()
            {
                throw new NotSupportedException();
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

        #endregion
    }
}
