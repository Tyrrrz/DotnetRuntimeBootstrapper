using System;
using System.Collections.Generic;
using DotnetRuntimeBootstrapper.Executable.Utils;
using DotnetRuntimeBootstrapper.Executable.Utils.Extensions;

namespace DotnetRuntimeBootstrapper.Executable
{
    // Config is provided to the bootstrapper when the target assembly
    // (i.e. the one being bootstrapped) is built.
    // This is performed by injecting an embedded resource inside the
    // bootstrapper assembly via an MSBuild task.
    public partial class BootstrapperConfig
    {
        public string TargetApplicationName { get; }

        public string TargetFileName { get; }

        public string TargetRuntimeName { get; }

        public SemanticVersion TargetRuntimeVersion { get; }

        public BootstrapperConfig(
            string targetApplicationName,
            string targetFileName,
            string targetRuntimeName,
            SemanticVersion targetRuntimeVersion)
        {
            TargetApplicationName = targetApplicationName;
            TargetFileName = targetFileName;
            TargetRuntimeName = targetRuntimeName;
            TargetRuntimeVersion = targetRuntimeVersion;
        }
    }

    public partial class BootstrapperConfig
    {
        private static Dictionary<string, string> ResolveMap()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var assembly = typeof(Program).Assembly;
            var rootNamespace = typeof(Program).Namespace;

            foreach (var line in assembly.GetManifestResourceString($"{rootNamespace}.Config.cfg").Split('\n'))
            {
                var equalsPos = line.IndexOf('=');
                if (equalsPos < 0)
                    continue;

                var key = line.Substring(0, equalsPos).Trim();
                var value = line.Substring(equalsPos + 1).Trim();

                result[key] = value;
            }

            return result;
        }

        public static BootstrapperConfig Resolve()
        {
            var map = ResolveMap();

            return new BootstrapperConfig(
                map[nameof(TargetApplicationName)],
                map[nameof(TargetFileName)],
                map[nameof(TargetRuntimeName)],
                map[nameof(TargetRuntimeVersion)].Pipe(SemanticVersion.Parse)
            );
        }
    }
}