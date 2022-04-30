using System;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Platform;

internal static class OperatingSystemVersion
{
    public static Version Windows7 { get; } = new(6, 1);

    public static Version Windows8 { get; } = new(6, 2);

    // ReSharper disable once InconsistentNaming
    public static Version Windows8_1 { get; } = new(6, 3);

    public static Version Windows10 { get; } = new(10, 0);
}