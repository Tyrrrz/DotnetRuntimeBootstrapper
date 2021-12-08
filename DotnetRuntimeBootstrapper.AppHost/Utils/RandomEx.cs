using System;

namespace DotnetRuntimeBootstrapper.AppHost.Utils;

internal static class RandomEx
{
    public static Random Instance { get; } = new();
}