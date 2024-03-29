﻿using System;
using DotnetRuntimeBootstrapper.AppHost.Core.Native;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;

namespace DotnetRuntimeBootstrapper.AppHost.Cli.Utils;

internal static class ConsoleEx
{
    public static bool IsInteractive =>
        NativeMethods.GetConsoleWindow() != 0
        && NativeMethods.GetFileType(NativeMethods.GetStdHandle(-10)) == 2
        && NativeMethods.GetFileType(NativeMethods.GetStdHandle(-11)) == 2
        && NativeMethods.GetFileType(NativeMethods.GetStdHandle(-12)) == 2;

    public static IDisposable WithForegroundColor(ConsoleColor color)
    {
        var lastColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        return Disposable.Create(() => Console.ForegroundColor = lastColor);
    }
}
