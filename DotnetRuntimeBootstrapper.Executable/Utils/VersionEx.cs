using System;
using System.Text.RegularExpressions;

namespace DotnetRuntimeBootstrapper.Executable.Utils
{
    internal static class VersionEx
    {
        public static Version? TryParse(string value) =>
            Regex.IsMatch(value, @"^\d+\.\d+(?:\.\d+)?(?:\.\d+)?$")
                ? new Version(value)
                : null;
    }
}