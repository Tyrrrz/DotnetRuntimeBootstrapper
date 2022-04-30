namespace DotnetRuntimeBootstrapper.AppHost.Core.Platform;

internal enum ProcessorArchitecture
{
    X86,
    X64,
    Arm,
    Arm64
}

internal static class ProcessorArchitectureExtensions
{
    public static bool Is64Bit(this ProcessorArchitecture arch) =>
        arch is ProcessorArchitecture.X64 or ProcessorArchitecture.Arm64;

    public static string GetMoniker(this ProcessorArchitecture arch) =>
        arch.ToString().ToLowerInvariant();
}