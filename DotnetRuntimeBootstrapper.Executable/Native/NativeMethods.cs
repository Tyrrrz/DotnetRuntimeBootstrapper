using System.Runtime.InteropServices;

namespace DotnetRuntimeBootstrapper.Executable.Native
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void GetNativeSystemInfo(ref SystemInfo lpSystemInfo);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int RtlGetVersion(ref SystemVersionInfo versionInfo);
    }
}