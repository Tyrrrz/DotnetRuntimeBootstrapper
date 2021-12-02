﻿using System.Runtime.InteropServices;

namespace DotnetRuntimeBootstrapper.Executable.Native.Windows
{
    // ReSharper disable InconsistentNaming
    [StructLayout(LayoutKind.Sequential)]
    internal struct IOCounters
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }
}