using System;

namespace DotnetRuntimeBootstrapper
{
    internal class BootstrapperInputs
    {
        public string TargetApplicationName { get; }

        public string TargetExecutableFilePath { get; }

        public string TargetRuntimeName { get; }

        public string TargetRuntimeVersion { get; }

        public BootstrapperInputs(
            string targetApplicationName,
            string targetExecutableFilePath,
            string targetRuntimeName,
            string targetRuntimeVersion)
        {
            TargetApplicationName = targetApplicationName;
            TargetExecutableFilePath = targetExecutableFilePath;
            TargetRuntimeName = targetRuntimeName;
            TargetRuntimeVersion = targetRuntimeVersion;
        }

        public string Serialize() =>
            $"{nameof(TargetApplicationName)}={TargetApplicationName}" + Environment.NewLine +
            $"{nameof(TargetExecutableFilePath)}={TargetExecutableFilePath}" + Environment.NewLine +
            $"{nameof(TargetRuntimeName)}={TargetRuntimeName}" + Environment.NewLine +
            $"{nameof(TargetRuntimeVersion)}={TargetRuntimeVersion}";
    }
}