using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;

namespace DotnetRuntimeBootstrapper.AppHost.Utils.Extensions
{
    internal static class AssemblyExtensions
    {
        public static string GetManifestResourceString(this Assembly assembly, string resourceName)
        {
            using var stream =
                assembly.GetManifestResourceStream(resourceName) ??
                throw new MissingManifestResourceException($"Could not resolve resource '{resourceName}'.");

            using var reader = new StreamReader(stream, Encoding.UTF8);

            return reader.ReadToEnd();
        }
    }
}