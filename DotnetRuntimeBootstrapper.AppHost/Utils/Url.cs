using System.Text.RegularExpressions;
using DotnetRuntimeBootstrapper.AppHost.Utils.Extensions;

namespace DotnetRuntimeBootstrapper.AppHost.Utils
{
    internal static class Url
    {
        public static string? TryExtractFileName(string url) =>
            Regex.Match(url, @".+/([^?]*)").Groups[1].Value.NullIfEmptyOrWhiteSpace();
    }
}