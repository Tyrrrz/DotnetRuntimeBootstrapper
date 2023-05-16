using System.Drawing;
using DotnetRuntimeBootstrapper.AppHost.Core.Native;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Utils;

internal static class IconEx
{
    // Built-in method in WinForms fails for network paths
    // https://github.com/Tyrrrz/DotnetRuntimeBootstrapper/issues/29
    public static Icon? TryExtractAssociatedIcon(string filePath)
    {
        var handle = NativeMethods.ExtractAssociatedIcon(0, filePath, out _);
        if (handle == 0)
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