using System.Runtime.InteropServices;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct JobObjectExtendedLimitInformation
{
    public JobObjectBasicLimitInformation BasicLimitInformation;
    public IOCounters IOInfo;
    public nint ProcessMemoryLimit;
    public nint JobMemoryLimit;
    public nint PeakProcessMemoryUsed;
    public nint PeakJobMemoryUsed;
}