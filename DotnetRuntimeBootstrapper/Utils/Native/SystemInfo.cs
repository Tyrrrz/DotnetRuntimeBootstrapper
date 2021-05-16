using System;
using System.Runtime.InteropServices;

namespace DotnetRuntimeBootstrapper.Utils.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SystemInfo
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