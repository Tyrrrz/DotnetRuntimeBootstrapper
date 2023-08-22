using System.Runtime.InteropServices;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct JobObjectBasicLimitInformation
{
    public long PerProcessUserTimeLimit;
    public long PerJobUserTimeLimit;
    public uint LimitFlags;
    public nint MinimumWorkingSetSize;
    public nint MaximumWorkingSetSize;
    public uint ActiveProcessLimit;
    public nint Affinity;
    public uint PriorityClass;
    public uint SchedulingClass;
}
