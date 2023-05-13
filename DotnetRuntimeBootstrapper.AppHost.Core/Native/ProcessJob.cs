using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Native;

internal partial class ProcessJob : NativeResource
{
    public ProcessJob(IntPtr handle)
        : base(handle)
    {
    }

    public void Configure(JobObjectExtendedLimitInformation limitInfo)
    {
        var limitInfoSize = Marshal.SizeOf(typeof(JobObjectExtendedLimitInformation));
        var limitInfoHandle = Marshal.AllocHGlobal(limitInfoSize);

        try
        {
            Marshal.StructureToPtr(limitInfo, limitInfoHandle, false);

            if (!NativeMethods.SetInformationJobObject(
                    Handle,
                    JobObjectInfoType.ExtendedLimitInformation,
                    limitInfoHandle,
                    (uint) limitInfoSize))
            {
                throw new Win32Exception();
            }
        }
        finally
        {
            Marshal.FreeHGlobal(limitInfoHandle);
        }
    }

    public void AddProcess(IntPtr processHandle) =>
        NativeMethods.AssignProcessToJobObject(Handle, processHandle);

    public void AddProcess(Process process) =>
        AddProcess(process.Handle);

    protected override void Dispose(bool disposing) =>
        NativeMethods.CloseHandle(Handle);
}

internal partial class ProcessJob
{
    public static ProcessJob Create()
    {
        var handle = NativeMethods.CreateJobObject(IntPtr.Zero, null);
        return handle != IntPtr.Zero
            ? new ProcessJob(handle)
            : throw new Win32Exception();
    }
}