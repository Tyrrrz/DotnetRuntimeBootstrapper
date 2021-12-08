using System.Runtime.InteropServices;

namespace DotnetRuntimeBootstrapper.AppHost.Native;

// ReSharper disable InconsistentNaming
[StructLayout(LayoutKind.Sequential)]
internal struct IOCounters
{
    public ulong ReadOperationCount;
    public ulong WriteOperationCount;
    public ulong OtherOperationCount;
    public ulong ReadTransferCount;
    public ulong WriteTransferCount;
    public ulong OtherTransferCount;
}