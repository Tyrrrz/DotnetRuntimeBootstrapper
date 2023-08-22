using System.Runtime.InteropServices;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Native;

[StructLayout(LayoutKind.Sequential)]
internal readonly partial struct SystemInfo
{
    public readonly ushort ProcessorArchitecture;
    public readonly ushort Reserved;
    public readonly uint PageSize;
    public readonly nint MinimumApplicationAddress;
    public readonly nint MaximumApplicationAddress;
    public readonly nuint ActiveProcessorMask;
    public readonly uint NumberOfProcessors;
    public readonly uint ProcessorType;
    public readonly uint AllocationGranularity;
    public readonly ushort ProcessorLevel;
    public readonly ushort ProcessorRevision;
}

internal partial struct SystemInfo
{
    public static SystemInfo Instance { get; } = Resolve();

    private static SystemInfo Resolve()
    {
        var systemInfo = default(SystemInfo);
        NativeMethods.GetNativeSystemInfo(ref systemInfo);

        return systemInfo;
    }
}
