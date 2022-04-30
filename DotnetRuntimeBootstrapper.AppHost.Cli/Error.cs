using System;
using System.Diagnostics;
using System.IO;

namespace DotnetRuntimeBootstrapper.AppHost.Cli;

internal static class Error
{
    private static void ReportToEventLog(string message)
    {
        // Inspired by:
        // https://github.com/dotnet/runtime/blob/57bfe474518ab5b7cfe6bf7424a79ce3af9d6657/src/native/corehost/apphost/apphost.windows.cpp#L37-L51

        try
        {
            var bootstrapperVersion = typeof(Error).Assembly.GetName().Version.ToString(3);
            var applicationFilePath = typeof(Error).Assembly.Location;
            var applicationName = Path.GetFileName(applicationFilePath);

            var content =
                "Description: Bootstrapper for a .NET application has failed." +
                Environment.NewLine +
                $"Application: {applicationName}" +
                Environment.NewLine +
                $"Path: {applicationFilePath}" +
                Environment.NewLine +
                $"AppHost: .NET Runtime Bootstrapper v{bootstrapperVersion} (console)" +
                Environment.NewLine +
                $"Message: {message}";

            EventLog.WriteEntry(".NET Runtime", content, EventLogEntryType.Error, 1023);
        }
        catch
        {
            // Ignore
        }
    }

    private static void ReportToUser(string message)
    {
        try
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Error.WriteLine("ERROR: " + message);
            Console.ResetColor();
        }
        catch
        {
            // Ignore
        }
    }

    public static void Report(string message)
    {
        ReportToEventLog(message);
        ReportToUser(message);
    }

    public static void Report(Exception ex) => Report(ex.ToString());
}