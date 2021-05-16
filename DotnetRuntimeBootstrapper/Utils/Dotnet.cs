using System.Collections.Generic;
using System.Diagnostics;

namespace DotnetRuntimeBootstrapper.Utils
{
    internal static class Dotnet
    {
        public static IEnumerable<string> ListRuntimes()
        {
            var commandOutput = CommandLine.TryExecuteProcess("dotnet", "--list-runtimes");
            if (string.IsNullOrEmpty(commandOutput))
                yield break;

            foreach (var runtimeIdentifier in commandOutput.Split('\n'))
            {
                yield return runtimeIdentifier.Trim();
            }
        }

        public static void Run(string targetExecutableFilePath, string[] arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"{CommandLine.EscapeArgument(targetExecutableFilePath)} -- {CommandLine.EscapeArgument(string.Join(" ", arguments))}",
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
            }
        }
    }
}