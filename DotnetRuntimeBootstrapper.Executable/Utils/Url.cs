using System.Text.RegularExpressions;
using DotnetRuntimeBootstrapper.Executable.Utils.Extensions;

namespace DotnetRuntimeBootstrapper.Executable.Utils
{
    internal static class Url
    {
        public static string? TryExtractFileName(string url) =>
            Regex.Match(url, @".+/([^?]*)").Groups[1].Value.NullIfEmptyOrWhiteSpace();
    }
}