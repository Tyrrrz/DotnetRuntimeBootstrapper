using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotnetRuntimeBootstrapper.Launcher.Utils;

namespace DotnetRuntimeBootstrapper.Launcher.Env
{
    internal static class Dotnet
    {
        public static IEnumerable<string> ListRuntimes()
        {
            var commandOutput = CommandLine.TryRun("dotnet", "--list-runtimes");
            if (string.IsNullOrEmpty(commandOutput))
                yield break;

            foreach (var runtimeIdentifier in commandOutput.Split('\n'))
            {
                yield return runtimeIdentifier.Trim();
            }
        }

        public static int Run(string targetExecutableFilePath, string[] arguments)
        {
            var argumentsFormatted =
                CommandLine.EscapeArgument(targetExecutableFilePath) +
                " -- " +
                string.Join(" ", arguments.Select(CommandLine.EscapeArgument).ToArray());

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = argumentsFormatted,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process
            {
                StartInfo = startInfo
            };

            using (process)
            {
                process.Start();
                process.WaitForExit();

                return process.ExitCode;
            }
        }
    }
}