using System;
using System.Collections.Generic;
using DotnetRuntimeBootstrapper.Executable.Utils.Extensions;

namespace DotnetRuntimeBootstrapper.Executable
{
    // Execution parameters are provided to the bootstrapper when the
    // target assembly (i.e. the one being bootstrapped) is built.
    // This is performed by injecting an embedded resource inside the
    // bootstrapper assembly via an MSBuild task.
    public partial class ExecutionParameters
    {
        public string TargetTitle { get; }

        public string TargetFileName { get; }

        public string TargetRuntimeName { get; }

        public string TargetRuntimeVersion { get; }

        public ExecutionParameters(
            string targetTitle,
            string targetFileName,
            string targetRuntimeName,
            string targetRuntimeVersion)
        {
            TargetTitle = targetTitle;
            TargetFileName = targetFileName;
            TargetRuntimeName = targetRuntimeName;
            TargetRuntimeVersion = targetRuntimeVersion;
        }
    }

    public partial class ExecutionParameters
    {
        private static Dictionary<string, string> ResolveMap()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var lines = typeof(Program)
                .Assembly
                .GetManifestResourceString("ExecutionParameters")
                .Split('\n');

            foreach (var line in lines)
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

        public static ExecutionParameters Resolve()
        {
            var map = ResolveMap();

            return new ExecutionParameters(
                map[nameof(TargetTitle)],
                map[nameof(TargetFileName)],
                map[nameof(TargetRuntimeName)],
                map[nameof(TargetRuntimeVersion)]
            );
        }
    }
}