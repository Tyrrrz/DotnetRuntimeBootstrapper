﻿using System;
using System.Runtime.InteropServices;

namespace DotnetRuntimeBootstrapper.Executable.Native.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct SystemInfo
    {
        public readonly ushort ProcessorArchitecture;
        public readonly ushort Reserved;
        public readonly uint PageSize;
        public readonly IntPtr MinimumApplicationAddress;
        public readonly IntPtr MaximumApplicationAddress;
        public readonly UIntPtr ActiveProcessorMask;
        public readonly uint NumberOfProcessors;
        public readonly uint ProcessorType;
        public readonly uint AllocationGranularity;
        public readonly ushort ProcessorLevel;
        public readonly ushort ProcessorRevision;
    }
}