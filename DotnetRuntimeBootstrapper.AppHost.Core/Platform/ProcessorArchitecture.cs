namespace DotnetRuntimeBootstrapper.AppHost.Core.Platform;

internal enum ProcessorArchitecture
{
    X86,
    X64,
    Arm,
    Arm64,
}

internal static class ProcessorArchitectureExtensions
{
    extension(ProcessorArchitecture arch)
    {
        public bool Is64Bit() => arch is ProcessorArchitecture.X64 or ProcessorArchitecture.Arm64;

        public string GetMoniker() => arch.ToString().ToLowerInvariant();
    }
}
