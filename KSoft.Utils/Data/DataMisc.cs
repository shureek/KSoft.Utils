using System;
using System.Runtime.InteropServices;

namespace KSoft.Data
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ByteInt
    {
        [FieldOffset(0)]
        public uint UIntValue;
        [FieldOffset(0)]
        public int IntValue;
        [FieldOffset(0)]
        public byte Byte0;
        [FieldOffset(1)]
        public byte Byte1;
        [FieldOffset(2)]
        public byte Byte2;
        [FieldOffset(3)]
        public byte Byte3;
    }
}