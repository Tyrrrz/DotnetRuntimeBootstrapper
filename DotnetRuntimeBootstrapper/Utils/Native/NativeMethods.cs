using System.Runtime.InteropServices;

namespace DotnetRuntimeBootstrapper.Utils.Native
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void GetNativeSystemInfo(ref SystemInfo lpSystemInfo);

        [DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int RtlGetVersion(ref SystemVersionInfo versionInfo);
    }
}