using System;
using System.IO;

namespace DotnetRuntimeBootstrapper.Executable.Utils
{
    internal static class PathEx
    {
        public static string ExecutingDirectoryPath { get; } =
            Path.GetDirectoryName(typeof(PathEx).Assembly.Location) ??
            AppDomain.CurrentDomain.BaseDirectory;
    }
}