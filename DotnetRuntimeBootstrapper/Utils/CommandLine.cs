using System.Diagnostics;
using System.Text;
using System.Threading;

namespace DotnetRuntimeBootstrapper.Utils
{
    internal static class CommandLine
    {
        public static string EscapeArgument(string argument) =>
            '"' + argument.Replace("\"", "\\\"") + '"';

        public static string Run(string executableFilePath, string arguments = "")
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = executableFilePath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process {StartInfo = startInfo};
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

            return process.ExitCode == 0
                ? stdOutBuffer.ToString()
                : stdErrBuffer.ToString();
        }

        public static string? TryRun(string executableFilePath, string arguments = "")
        {
            try
            {
                return Run(executableFilePath, arguments);
            }
            catch
            {
                return null;
            }
        }
    }
}