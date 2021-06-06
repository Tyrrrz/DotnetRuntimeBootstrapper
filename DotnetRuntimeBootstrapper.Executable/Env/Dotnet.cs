using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotnetRuntimeBootstrapper.Executable.Utils;
using DotnetRuntimeBootstrapper.Executable.Utils.Extensions;

namespace DotnetRuntimeBootstrapper.Executable.Env
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

        public static int Run(string targetFilePath, string[] arguments)
        {
            var argumentsFormatted = string.Join(
                " ",
                arguments.Prepend(targetFilePath).Select(CommandLine.EscapeArgument).ToArray()
            );

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = argumentsFormatted,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            return process.ExitCode;
        }
    }
}