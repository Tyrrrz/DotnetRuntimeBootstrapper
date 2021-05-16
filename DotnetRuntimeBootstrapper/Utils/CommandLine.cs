using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace DotnetRuntimeBootstrapper.Utils
{
    internal static class CommandLine
    {
        public static string EscapeArgument(string argument) =>
            '"' + argument.Replace("\"", "\\\"") + '"';

        public static string ExecuteProcess(string executableFilePath, string arguments = "")
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

            var process = new Process
            {
                StartInfo = startInfo
            };

            using (process)
            using (var stdOutSignal = new ManualResetEvent(false))
            using (var stdErrSignal = new ManualResetEvent(false))
            {
                var stdOutBuffer = new StringBuilder();

                process.EnableRaisingEvents = true;

                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        stdOutBuffer.AppendLine(args.Data);
                    }
                    else
                    {
                        stdOutSignal.Set();
                    }
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        // We don't care about stderr
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

                return stdOutBuffer.ToString();
            }
        }

        public static string TryExecuteProcess(string executableFilePath, string arguments = "")
        {
            try
            {
                return ExecuteProcess(executableFilePath, arguments);
            }
            catch
            {
                return null;
            }
        }
    }
}