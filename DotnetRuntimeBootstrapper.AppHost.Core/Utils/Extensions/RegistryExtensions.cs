using Microsoft.Win32;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;

internal static class RegistryExtensions
{
    extension(RegistryKey key)
    {
        public bool ContainsSubKey(string name) => key.OpenSubKey(name, false) is not null;
    }
}
