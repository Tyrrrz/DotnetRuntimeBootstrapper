using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace DotnetRuntimeBootstrapper.AppHost.Native;

internal static class NativeMethods
{
    private const string Kernel32 = "kernel32.dll";
    private const string NtDll = "ntdll.dll";
    private const string Advapi32 = "advapi32.dll";

    [DllImport(Kernel32, SetLastError = true)]
    public static extern void GetNativeSystemInfo(ref SystemInfo lpSystemInfo);

    [DllImport(NtDll, SetLastError = true)]
    public static extern void RtlGetVersion(ref SystemVersionInfo versionInfo);

    [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr CreateJobObject(IntPtr hAttributes, string? lpName);

    [DllImport(Kernel32, SetLastError = true)]
    public static extern bool SetInformationJobObject(
        IntPtr hJob,
        JobObjectInfoType infoType,
        IntPtr lpJobObjectInfo,
        uint cbJobObjectInfoLength
    );

    [DllImport(Kernel32, SetLastError = true)]
    public static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

    [DllImport(Kernel32, SetLastError = true)]
    public static extern bool CloseHandle(IntPtr hObject);

    [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport(Kernel32, SetLastError = true)]
    public static extern bool FreeLibrary(IntPtr hModule);

    // This function doesn't come in the Unicode variant
    [DllImport(Kernel32, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport(Advapi32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr RegisterEventSource(string? lpUNCServerName, string lpSourceName);

    [DllImport(Advapi32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool ReportEvent(
        IntPtr hEventLog,
        ushort wType,
        ushort wCategory,
        uint dwEventID,
        IntPtr lpUserSid,
        ushort wNumStrings,
        uint dwDataSize,
        string[] lpStrings,
        IntPtr lpRawData
    );

    [DllImport(Advapi32, SetLastError = true)]
    public static extern bool DeregisterEventSource(IntPtr hEventLog);
}