using System.IO;
using System.Reflection;
using System.Resources;

namespace DotnetRuntimeBootstrapper.Utils.Extensions;

internal static class AssemblyExtensions
{
    public static void ExtractManifestResource(this Assembly assembly, string resourceName, string filePath)
    {
        var resourceStream =
            assembly.GetManifestResourceStream(resourceName) ??
            throw new MissingManifestResourceException($"Could not find resource '{resourceName}'.");

        using var fileStream = File.Create(filePath);
        resourceStream.CopyTo(fileStream);
    }
}