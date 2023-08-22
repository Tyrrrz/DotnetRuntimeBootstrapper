using System;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Native;

internal abstract class NativeResource : IDisposable
{
    public nint Handle { get; }

    protected NativeResource(nint handle) => Handle = handle;

    ~NativeResource() => Dispose(false);

    protected abstract void Dispose(bool disposing);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
