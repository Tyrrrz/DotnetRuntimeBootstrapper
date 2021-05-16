using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
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
            var assembly = typeof(Program).Assembly;
            var rootNamespace = typeof(Program).Namespace;

            var resourceStream = assembly.GetManifestResourceStream($"{rootNamespace}.Inputs.cfg");
            if (resourceStream == null)
            {
                throw new MissingManifestResourceException(
                    "Missing manifest resource containing inputs."
                );
            }

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using (resourceStream)
            using (var resourceReader = new StreamReader(resourceStream))
            {
                foreach (var line in resourceReader.ReadAllLines())
                {
                    var equalsPos = line.IndexOf('=');
                    if (equalsPos < 0)
                        continue;

                    var key = line.Substring(0, equalsPos).Trim();
                    var value = line.Substring(equalsPos + 1).Trim();

                    result[key] = value;
                }
            }

            return result;
        }

        private static Dictionary<string, string> _map;
        private static Dictionary<string, string> Map => _map ?? (_map = ResolveMap());

        private static string TryGetValue(string key) =>
            Map.TryGetValue(key, out var value)
                ? value
                : null;

        public static string TargetApplicationName =>
            TryGetValue(nameof(TargetApplicationName));

        public static string TargetExecutableFilePath =>
            TryGetValue(nameof(TargetExecutableFilePath));

        public static string TargetRuntimeName =>
            TryGetValue(nameof(TargetRuntimeName));

        public static SemanticVersion TargetRuntimeVersion =>
            TryGetValue(nameof(TargetRuntimeVersion))?.Pipe(SemanticVersion.TryParse);
    }
}