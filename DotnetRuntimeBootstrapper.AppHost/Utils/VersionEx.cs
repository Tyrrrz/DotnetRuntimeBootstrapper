using System;
using System.Text.RegularExpressions;

namespace DotnetRuntimeBootstrapper.AppHost.Utils;

internal static class VersionEx
{
    public static Version? TryParse(string value) =>
        Regex.IsMatch(value, @"^\d+\.\d+(?:\.\d+)?(?:\.\d+)?$")
            ? Parse(value)
            : null;

    public static Version Parse(string value) => new(value);
}