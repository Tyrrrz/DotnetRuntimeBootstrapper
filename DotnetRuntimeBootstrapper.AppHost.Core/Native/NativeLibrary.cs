using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Native;

internal partial class NativeLibrary : IDisposable
{
    private readonly IntPtr _handle;
    private readonly Dictionary<string, Delegate> _functionTable = new(StringComparer.Ordinal);

    private bool _isDisposed;

    public NativeLibrary(IntPtr handle) => _handle = handle;

    ~NativeLibrary() => Dispose();

    public TDelegate GetFunction<TDelegate>(string functionName) where TDelegate : Delegate
    {
        if (_functionTable.TryGetValue(functionName, out var funcCached))
            return (TDelegate)funcCached;

        var address = NativeMethods.GetProcAddress(_handle, functionName);
        if (address == IntPtr.Zero)
            throw new Win32Exception();

        var func = (TDelegate)Marshal.GetDelegateForFunctionPointer(address, typeof(TDelegate));
        _functionTable[functionName] = func;

        return func;
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        NativeMethods.FreeLibrary(_handle);
        GC.SuppressFinalize(this);
    }
}

internal partial class NativeLibrary
{
    public static NativeLibrary Load(string filePath)
    {
        var handle = NativeMethods.LoadLibrary(filePath);
        return handle != IntPtr.Zero
            ? new NativeLibrary(handle)
            : throw new Win32Exception();
    }
}