using System;

namespace DotnetRuntimeBootstrapper.AppHost.Utils.Extensions;

internal static class StringExtensions
{
    public static string? NullIfEmptyOrWhiteSpace(this string str) =>
        !string.IsNullOrEmpty(str.Trim())
            ? str
            : null;

    public static string TrimStart(
        this string str,
        string sub,
        StringComparison comparison = StringComparison.Ordinal)
    {
        var trimmed = str;

        while (trimmed.StartsWith(sub, comparison))
            trimmed = trimmed.Substring(sub.Length);

        return trimmed;
    }

    public static string TrimEnd(
        this string str,
        string sub,
        StringComparison comparison = StringComparison.Ordinal)
    {
        var trimmed = str;

        while (trimmed.EndsWith(sub, comparison))
            trimmed = trimmed.Substring(0, trimmed.Length - sub.Length);

        return trimmed;
    }
}