using System;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Native;

internal abstract class NativeResource(nint handle) : IDisposable
{
    public nint Handle { get; } = handle;

    ~NativeResource() => Dispose(false);

    protected abstract void Dispose(bool disposing);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
