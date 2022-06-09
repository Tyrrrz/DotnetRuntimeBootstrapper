using System;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;

namespace DotnetRuntimeBootstrapper.AppHost.Cli.Utils;

internal static class ConsoleEx
{
    public static IDisposable WithForegroundColor(ConsoleColor color)
    {
        var lastColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        return Disposable.Create(() => Console.ForegroundColor = lastColor);
    }
}