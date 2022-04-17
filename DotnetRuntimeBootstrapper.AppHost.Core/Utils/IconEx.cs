using System;
using System.Drawing;
using DotnetRuntimeBootstrapper.AppHost.Native;

namespace DotnetRuntimeBootstrapper.AppHost.Utils;

internal static class IconEx
{
    // Built-in method fails for network paths
    // https://github.com/Tyrrrz/DotnetRuntimeBootstrapper/issues/29
    public static Icon? TryExtractAssociatedIcon(string filePath)
    {
        var handle = NativeMethods.ExtractAssociatedIcon(IntPtr.Zero, filePath, out _);
        if (handle == IntPtr.Zero)
            return null;

        try
        {
            return Icon.FromHandle(handle);
        }
        catch
        {
            return null;
        }
    }
}