using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace DotnetRuntimeBootstrapper.Utils
{
    internal static class CommandLine
    {
        private static string EscapeArgument(string argument) =>
            '"' + argument.Replace("\"", "\\\"") + '"';

        public static string RunWithOutput(string executableFilePath, IReadOnlyList<string> arguments)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executableFilePath,
                    Arguments = string.Join(" ", arguments.Select(EscapeArgument)),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

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

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
            stdOutSignal.WaitOne();
            stdErrSignal.WaitOne();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Process returned a non-zero exit code ({process.ExitCode})." +
                    Environment.NewLine +
                    $"Command: {process.StartInfo.FileName} {process.StartInfo.Arguments}" +
                    Environment.NewLine +
                    "Standard error:" +
                    Environment.NewLine +
                    stdErrBuffer
                );
            }

            return stdOutBuffer.ToString();
        }

        public static string? TryRunWithOutput(string executableFilePath, IReadOnlyList<string> arguments)
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