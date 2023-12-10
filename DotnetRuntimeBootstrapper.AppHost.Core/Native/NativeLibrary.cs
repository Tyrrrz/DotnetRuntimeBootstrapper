using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Native;

internal partial class NativeLibrary(nint handle) : NativeResource(handle)
{
    private readonly Dictionary<string, Delegate> _functionsByName = new(StringComparer.Ordinal);

    public TDelegate GetFunction<TDelegate>(string functionName)
        where TDelegate : Delegate
    {
        if (
            _functionsByName.TryGetValue(functionName, out var cached)
            && cached is TDelegate cachedCasted
        )
            return cachedCasted;

        var address = NativeMethods.GetProcAddress(Handle, functionName);
        if (address == 0)
            throw new Win32Exception();

        var function = (TDelegate)Marshal.GetDelegateForFunctionPointer(address, typeof(TDelegate));
        _functionsByName[functionName] = function;

        return function;
    }

    protected override void Dispose(bool disposing) => NativeMethods.FreeLibrary(Handle);
}

internal partial class NativeLibrary
{
    public static NativeLibrary Load(string filePath)
    {
        var handle = NativeMethods.LoadLibrary(filePath);
        return handle != 0 ? new NativeLibrary(handle) : throw new Win32Exception();
    }
}
