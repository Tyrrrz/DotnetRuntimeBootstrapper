using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Native;

internal partial class ProcessJob : IDisposable
{
    private readonly IntPtr _handle;

    private bool _isDisposed;

    public ProcessJob(IntPtr handle) =>
        _handle = handle;

    ~ProcessJob() => Dispose();

    public void Configure(JobObjectExtendedLimitInformation limitInfo)
    {
        var limitInfoSize = Marshal.SizeOf(typeof(JobObjectExtendedLimitInformation));
        var limitInfoHandle = Marshal.AllocHGlobal(limitInfoSize);

        try
        {
            Marshal.StructureToPtr(limitInfo, limitInfoHandle, false);

            if (!NativeMethods.SetInformationJobObject(
                    _handle,
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
        NativeMethods.AssignProcessToJobObject(_handle, processHandle);

    public void AddProcess(Process process) =>
        AddProcess(process.Handle);

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        NativeMethods.CloseHandle(_handle);
        GC.SuppressFinalize(this);
    }
}

internal partial class ProcessJob
{
    public static ProcessJob Create()
    {
        var handle = NativeMethods.CreateJobObject(IntPtr.Zero, null);
        if (handle == IntPtr.Zero)
            throw new Win32Exception();

        return new ProcessJob(handle);
    }
}