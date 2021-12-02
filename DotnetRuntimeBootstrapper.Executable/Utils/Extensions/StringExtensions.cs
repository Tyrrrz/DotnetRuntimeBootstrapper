using System;

namespace DotnetRuntimeBootstrapper.Executable.Utils.Extensions
{
    internal static class StringExtensions
    {
        public static string? NullIfEmptyOrWhiteSpace(this string str) =>
            str.Trim().Length != 0
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
}