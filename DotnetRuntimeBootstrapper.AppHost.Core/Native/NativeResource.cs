using System;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Native;

internal abstract class NativeResource : IDisposable
{
    public IntPtr Handle { get; }

    protected NativeResource(IntPtr handle) => Handle = handle;

    ~NativeResource() => Dispose(false);

    protected abstract void Dispose(bool disposing);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}