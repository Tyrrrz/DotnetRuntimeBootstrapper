using System;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;

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
            trimmed = trimmed[sub.Length..];

        return trimmed;
    }

    public static string TrimEnd(
        this string str,
        string sub,
        StringComparison comparison = StringComparison.Ordinal)
    {
        var trimmed = str;

        while (trimmed.EndsWith(sub, comparison))
            trimmed = trimmed[..^sub.Length];

        return trimmed;
    }
}