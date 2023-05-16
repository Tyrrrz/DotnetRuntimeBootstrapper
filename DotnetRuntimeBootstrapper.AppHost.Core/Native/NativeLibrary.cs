using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Native;

internal partial class NativeLibrary : NativeResource
{
    private readonly Dictionary<string, Delegate> _functionTable = new(StringComparer.Ordinal);

    public NativeLibrary(nint handle)
        : base(handle)
    {
    }

    public TDelegate GetFunction<TDelegate>(string functionName) where TDelegate : Delegate
    {
        if (_functionTable.TryGetValue(functionName, out var funcCached))
            return (TDelegate)funcCached;

        var address = NativeMethods.GetProcAddress(Handle, functionName);
        if (address == 0)
            throw new Win32Exception();

        var func = (TDelegate)Marshal.GetDelegateForFunctionPointer(address, typeof(TDelegate));
        _functionTable[functionName] = func;

        return func;
    }

    protected override void Dispose(bool disposing) =>
        NativeMethods.FreeLibrary(Handle);
}

internal partial class NativeLibrary
{
    public static NativeLibrary Load(string filePath)
    {
        var handle = NativeMethods.LoadLibrary(filePath);
        return handle != 0
            ? new NativeLibrary(handle)
            : throw new Win32Exception();
    }
}