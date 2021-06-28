using System.Collections.Generic;
using System.Linq;
using DotnetRuntimeBootstrapper.Executable.Utils;
using DotnetRuntimeBootstrapper.Executable.Utils.Extensions;

namespace DotnetRuntimeBootstrapper.Executable.Env
{
    internal static class Dotnet
    {
        public static IEnumerable<string> ListRuntimes()
        {
            var commandOutput = CommandLine.TryRunWithOutput("dotnet", new[] {"--list-runtimes"});
            if (string.IsNullOrEmpty(commandOutput))
                yield break;

            foreach (var runtimeLine in commandOutput.Split('\n'))
            {
                yield return runtimeLine.Trim();
            }
        }

        public static int Run(string targetFilePath, string[] arguments) =>
            CommandLine.Run("dotnet", arguments.Prepend(targetFilePath).ToArray());
    }
}