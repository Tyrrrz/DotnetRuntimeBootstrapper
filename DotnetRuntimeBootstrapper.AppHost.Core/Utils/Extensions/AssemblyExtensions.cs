using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;

internal static class AssemblyExtensions
{
    extension(Assembly assembly)
    {
        public string GetManifestResourceString(string resourceName)
        {
            using var stream =
                assembly.GetManifestResourceStream(resourceName)
                ?? throw new MissingManifestResourceException(
                    $"Failed to resolve resource '{resourceName}'."
                );

            using var reader = new StreamReader(stream, Encoding.UTF8);

            return reader.ReadToEnd();
        }
    }
}
