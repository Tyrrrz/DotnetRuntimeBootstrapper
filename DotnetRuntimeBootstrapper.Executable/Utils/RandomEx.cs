using System;

namespace DotnetRuntimeBootstrapper.Executable.Utils
{
    internal static class RandomEx
    {
        public static Random Instance { get; } = new();
    }
}