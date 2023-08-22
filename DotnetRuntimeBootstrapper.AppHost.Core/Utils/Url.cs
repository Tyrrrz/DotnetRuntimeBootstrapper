using System;
using System.Text.RegularExpressions;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Utils;

internal static class Url
{
    public static string ReplaceProtocol(string url, string protocol)
    {
        if (url.StartsWith(protocol + "://", StringComparison.Ordinal))
            return url;

        var index = url.IndexOf("://", StringComparison.Ordinal);
        if (index < 0)
            return url;

        return protocol + url[index..];
    }

    public static string? TryExtractFileName(string url) =>
        Regex.Match(url, @".+/([^?]*)").Groups[1].Value.NullIfEmptyOrWhiteSpace();
}
