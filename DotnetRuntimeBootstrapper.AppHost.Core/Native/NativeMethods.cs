using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
namespace DotnetRuntimeBootstrapper.AppHost.Core.Native;

internal static class NativeMethods
{
    private const string NtDll = "ntdll.dll";
    private const string Kernel32 = "kernel32.dll";
    private const string Shell32 = "shell32.dll";

    [DllImport(NtDll, SetLastError = true)]
    public static extern void RtlGetVersion(ref SystemVersionInfo versionInfo);

    [DllImport(Kernel32, SetLastError = true)]
    public static extern void GetNativeSystemInfo(ref SystemInfo lpSystemInfo);

    [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern nint CreateJobObject(nint hAttributes, string? lpName);

    [DllImport(Kernel32, SetLastError = true)]
    public static extern bool SetInformationJobObject(
        nint hJob,
        JobObjectInfoType infoType,
        nint lpJobObjectInfo,
        uint cbJobObjectInfoLength
    );

    [DllImport(Kernel32, SetLastError = true)]
    public static extern bool AssignProcessToJobObject(nint hJob, nint hProcess);

    [DllImport(Kernel32, SetLastError = true)]
    public static extern bool CloseHandle(nint hObject);

    [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern nint LoadLibrary(string lpFileName);

    [DllImport(Kernel32, SetLastError = true)]
    public static extern bool FreeLibrary(nint hModule);

    // This function doesn't come in the Unicode variant
    [DllImport(Kernel32, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    public static extern nint GetProcAddress(nint hModule, string lpProcName);

    [DllImport(Kernel32, SetLastError = true)]
    public static extern nint GetConsoleWindow();

    [DllImport(Kernel32, SetLastError = true)]
    public static extern nint GetStdHandle(int nStdHandle);

    [DllImport(Kernel32, SetLastError = true)]
    public static extern int GetFileType(nint hFile);

    [DllImport(Shell32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern nint ExtractAssociatedIcon(nint hInst, string lpIconPath, out ushort lpiIcon);
}