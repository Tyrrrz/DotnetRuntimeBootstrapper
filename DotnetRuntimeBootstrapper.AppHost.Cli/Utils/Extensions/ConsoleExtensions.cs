using System;
using DotnetRuntimeBootstrapper.AppHost.Core.Native;

namespace DotnetRuntimeBootstrapper.AppHost.Cli.Utils.Extensions;

internal static class ConsoleExtensions
{
    extension(Console)
    {
        public static bool IsInteractive =>
            NativeMethods.GetConsoleWindow() != 0
            && NativeMethods.GetFileType(NativeMethods.GetStdHandle(-10)) == 2
            && NativeMethods.GetFileType(NativeMethods.GetStdHandle(-11)) == 2
            && NativeMethods.GetFileType(NativeMethods.GetStdHandle(-12)) == 2;
    }
}
