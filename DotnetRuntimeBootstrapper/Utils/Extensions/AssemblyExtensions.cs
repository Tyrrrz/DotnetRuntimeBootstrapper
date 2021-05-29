using System.IO;
using System.Reflection;
using System.Resources;

namespace DotnetRuntimeBootstrapper.Utils.Extensions
{
    internal static class AssemblyExtensions
    {
        public static string GetManifestResourceString(this Assembly assembly, string resourceName)
        {
            var resourceStream =
                assembly.GetManifestResourceStream(resourceName) ??
                throw new MissingManifestResourceException($"Could not find resource '{resourceName}'.");

            using (resourceStream)
            using (var resourceReader = new StreamReader(resourceStream))
                return resourceReader.ReadToEnd();
        }
    }
}