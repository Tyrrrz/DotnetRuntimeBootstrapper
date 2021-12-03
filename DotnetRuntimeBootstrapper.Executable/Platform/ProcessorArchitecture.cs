using System;

namespace DotnetRuntimeBootstrapper.Executable.Platform
{
    internal enum ProcessorArchitecture
    {
        X86,
        X64,
        Arm,
        Arm64
    }

    internal static class ProcessorArchitectureExtensions
    {
        public static string GetMoniker(this ProcessorArchitecture arch) => arch switch
        {
            ProcessorArchitecture.X86 => "x86",
            ProcessorArchitecture.X64 => "x64",
            ProcessorArchitecture.Arm => "arm",
            ProcessorArchitecture.Arm64 => "arm64",
            _ => throw new ArgumentOutOfRangeException(nameof(arch), "Unknown processor architecture.")
        };
    }
}