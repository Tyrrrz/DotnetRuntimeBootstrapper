using System.Runtime.InteropServices;

namespace DotnetRuntimeBootstrapper.AppHost.Native;

[StructLayout(LayoutKind.Sequential)]
internal readonly partial struct SystemVersionInfo
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

internal partial struct SystemVersionInfo
{
    public static SystemVersionInfo Instance { get; } = Resolve();

    private static SystemVersionInfo Resolve()
    {
        var systemVersionInfo = default(SystemVersionInfo);
        NativeMethods.RtlGetVersion(ref systemVersionInfo);

        return systemVersionInfo;
    }
}