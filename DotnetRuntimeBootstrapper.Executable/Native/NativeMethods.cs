using System;
using System.Runtime.InteropServices;

namespace DotnetRuntimeBootstrapper.Executable.Native
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void GetNativeSystemInfo(ref SystemInfo lpSystemInfo);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int RtlGetVersion(ref SystemVersionInfo versionInfo);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateJobObject(IntPtr hAttributes, string? lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetInformationJobObject(
            IntPtr hJob,
            JobObjectInfoType infoType,
            IntPtr lpJobObjectInfo,
            uint cbJobObjectInfoLength
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);
    }
}