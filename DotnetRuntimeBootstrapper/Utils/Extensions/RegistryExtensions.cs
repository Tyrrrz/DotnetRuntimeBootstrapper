using Microsoft.Win32;

namespace DotnetRuntimeBootstrapper.Utils.Extensions
{
    internal static class RegistryExtensions
    {
        public static bool ContainsSubKey(this RegistryKey key, string name) =>
            key.OpenSubKey(name, false) != null;
    }
}