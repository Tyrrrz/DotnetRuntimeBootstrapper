using System;
using System.Collections.Generic;
using DotnetRuntimeBootstrapper.Launcher.Utils;
using DotnetRuntimeBootstrapper.Launcher.Utils.Extensions;

namespace DotnetRuntimeBootstrapper.Launcher
{
    // Inputs are provided to the bootstrapper when the target assembly
    // (i.e. the one being bootstrapped) is built.
    // This is performed by injecting an embedded resource inside the
    // bootstrapper assembly via an MSBuild task.
    public partial class BootstrapperInputs
    {
        public string TargetApplicationName { get; }

        public string TargetExecutableFilePath { get; }

        public string TargetRuntimeName { get; }

        public SemanticVersion TargetRuntimeVersion { get; }

        public BootstrapperInputs(
            string targetApplicationName,
            string targetExecutableFilePath,
            string targetRuntimeName,
            SemanticVersion targetRuntimeVersion)
        {
            TargetApplicationName = targetApplicationName;
            TargetExecutableFilePath = targetExecutableFilePath;
            TargetRuntimeName = targetRuntimeName;
            TargetRuntimeVersion = targetRuntimeVersion;
        }
    }

    public partial class BootstrapperInputs
    {
        private static Dictionary<string, string> ResolveMap()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var assembly = typeof(Program).Assembly;
            var rootNamespace = typeof(Program).Namespace;

            foreach (var line in assembly.GetManifestResourceString($"{rootNamespace}.Inputs.cfg").Split('\n'))
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

        public static BootstrapperInputs Resolve()
        {
            var map = ResolveMap();

            return new BootstrapperInputs(
                map["TargetApplicationName"],
                map["TargetExecutableFilePath"],
                map["targetRuntimeName"],
                map["TargetRuntimeVersion"].Pipe(SemanticVersion.Parse)
            );
        }
    }
}