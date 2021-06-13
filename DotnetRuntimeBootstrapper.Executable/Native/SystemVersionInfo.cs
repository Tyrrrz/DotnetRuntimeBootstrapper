using System.Runtime.InteropServices;

namespace DotnetRuntimeBootstrapper.Executable.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct SystemVersionInfo
    {
        public readonly int OSVersionInfoSize;
        public readonly int MajorVersion;
        public readonly int MinorVersion;
        public readonly int BuildNumber;
        public readonly int PlatformId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public readonly string CSDVersion;

        public readonly ushort ServicePackMajor;
        public readonly ushort ServicePackMinor;
        public readonly short SuiteMask;
        public readonly byte ProductType;
        public readonly byte Reserved;
    }
}