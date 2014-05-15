using System;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32.SafeHandles;

namespace KSoft.IO
{
    internal class SafeFindHandle : SafeHandleMinusOneIsInvalid
    {
        public SafeFindHandle()
            : base(true)
        { }

        protected override bool ReleaseHandle()
        {
            return FindClose(handle);
        }

        #region WinAPI

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FindClose(IntPtr findHandle);

        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ByHandleFileInformation
    {
        public System.IO.FileAttributes FileAttributes;
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
}