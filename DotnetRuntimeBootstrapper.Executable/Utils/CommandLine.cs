using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using DotnetRuntimeBootstrapper.Executable.Native;

namespace DotnetRuntimeBootstrapper.Executable.Utils
{
    internal static class CommandLine
    {
        private static ProcessJob? ProcessJob { get; } = TryCreateProcessJob();

        private static ProcessJob? TryCreateProcessJob()
        {
            try
            {
                var processJob = ProcessJob.Create();

                processJob.Configure(new JobObjectExtendedLimitInformation
                {
                    BasicLimitInformation = new JobObjectBasicLimitInformation
                    {
                        // JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
                        LimitFlags = 0x2000
                    }
                });

                return processJob;
            }
            catch
            {
                // Not critical, ignore errors
                return null;
            }
        }

        private static string EscapeArgument(string argument) =>
            '"' + argument.Replace("\"", "\\\"") + '"';

        private static Process CreateProcess(string executableFilePath, string[] arguments, bool isElevated = false)
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

        public static string RunWithOutput(string executableFilePath, string[] arguments)
        {
            using var process = CreateProcess(executableFilePath, arguments);

            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            using var stdOutSignal = new ManualResetEvent(false);
            using var stdErrSignal = new ManualResetEvent(false);

            var stdOutBuffer = new StringBuilder();
            var stdErrBuffer = new StringBuilder();

            process.EnableRaisingEvents = true;

            process.OutputDataReceived += (_, args) =>
            {
                if (args.Data is not null)
                {
                    stdOutBuffer.AppendLine(args.Data);
                }
                else
                {
                    stdOutSignal.Set();
                }
            };

            process.ErrorDataReceived += (_, args) =>
            {
                if (args.Data is not null)
                {
                    stdErrBuffer.AppendLine(args.Data);
                }
                else
                {
                    stdErrSignal.Set();
                }
            };

            process.Start();
            ProcessJob?.AddProcess(process);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
            stdOutSignal.WaitOne();
            stdErrSignal.WaitOne();

            if (process.ExitCode != 0)
            {
                throw new ApplicationException(
                    $"Process returned a non-zero exit code ({process.ExitCode})." +
                    Environment.NewLine +
                    $"Command: {process.StartInfo.FileName} {process.StartInfo.Arguments}" +
                    Environment.NewLine +
                    "Standard error:" +
                    Environment.NewLine +
                    stdOutBuffer
                );
            }

            return stdOutBuffer.ToString();
        }

        public static string? TryRunWithOutput(string executableFilePath, string[] arguments)
        {
            try
            {
                return RunWithOutput(executableFilePath, arguments);
            }
            catch
            {
                return null;
            }
        }
    }
}