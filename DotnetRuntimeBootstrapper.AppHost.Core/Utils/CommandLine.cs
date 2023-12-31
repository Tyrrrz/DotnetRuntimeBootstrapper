using System;
using System.Diagnostics;
using System.Linq;
using DotnetRuntimeBootstrapper.AppHost.Core.Native;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Utils;

internal static class CommandLine
{
    // Process job that will ensure that all child processes will be killed
    // when the parent process terminates.
    // Ensures we don't leave installers or other executables running if the
    // user decides to cancel or force exit.
    private static ProcessJob? ProcessJob { get; } = TryCreateProcessJob();

    private static ProcessJob? TryCreateProcessJob()
    {
        try
        {
            var processJob = ProcessJob.Create();

            processJob.Configure(
                new JobObjectExtendedLimitInformation
                {
                    BasicLimitInformation = new JobObjectBasicLimitInformation
                    {
                        // JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
                        LimitFlags = 0x2000
                    }
                }
            );

            return processJob;
        }
        catch
        {
            // Not critical, ignore errors
            return null;
        }
    }

    private static string EscapeArgument(string argument) =>
        '"' + argument.Replace("\"", "\\\"", StringComparison.Ordinal) + '"';

    private static Process CreateProcess(
        string executableFilePath,
        string[] arguments,
        bool isElevated = false
    )
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = executableFilePath,
                Arguments = string.Join(" ", arguments.Select(EscapeArgument).ToArray()),
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        if (isElevated)
        {
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = "runas";
        }

        return process;
    }

    public static int Run(string executableFilePath, string[] arguments, bool isElevated = false)
    {
        using var process = CreateProcess(executableFilePath, arguments, isElevated);

        process.Start();
        ProcessJob?.AddProcess(process);

        process.WaitForExit();

        return process.ExitCode;
    }
}
