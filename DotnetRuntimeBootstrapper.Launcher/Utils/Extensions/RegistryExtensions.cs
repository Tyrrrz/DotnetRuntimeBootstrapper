using Microsoft.Win32;

namespace DotnetRuntimeBootstrapper.Launcher.Utils.Extensions
{
    internal static class RegistryExtensions
    {
        public static bool ContainsSubKey(this RegistryKey key, string name) =>
            key.OpenSubKey(name, false) is not null;
    }
}