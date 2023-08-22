using System;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Utils;

internal static class RandomEx
{
    public static Random Instance { get; } = new();
}
