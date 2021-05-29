using System;
using System.Collections.Generic;
using DotnetRuntimeBootstrapper.Utils;
using DotnetRuntimeBootstrapper.Utils.Extensions;

namespace DotnetRuntimeBootstrapper
{
    // Inputs are provided to the bootstrapper when the target assembly
    // (i.e. the one being bootstrapped) is built.
    // This is performed by injecting an embedded resource inside the
    // bootstrapper assembly via an MSBuild task.
    internal static class Inputs
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

        private static Dictionary<string, string>? _map;
        private static Dictionary<string, string> Map => _map ??= ResolveMap();

        private static string? TryGetValue(string key) =>
            Map.TryGetValue(key, out var value)
                ? value
                : null;

        private static string GetValue(string key) =>
            TryGetValue(key) ?? throw new InvalidOperationException($"Required input '{key}' is missing.");

        public static string TargetApplicationName =>
            GetValue(nameof(TargetApplicationName));

        public static string TargetExecutableFilePath =>
            GetValue(nameof(TargetExecutableFilePath));

        public static string TargetRuntimeName =>
            GetValue(nameof(TargetRuntimeName));

        public static SemanticVersion TargetRuntimeVersion =>
            GetValue(nameof(TargetRuntimeVersion)).Pipe(SemanticVersion.Parse);
    }
}