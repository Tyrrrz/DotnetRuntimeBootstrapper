using System;
using System.IO;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.AppHost.Native;

namespace DotnetRuntimeBootstrapper.AppHost.Utils;

internal static class Error
{
    private static void ReportToEventLog(string message)
    {
        // Inspired by:
        // https://github.com/dotnet/runtime/blob/57bfe474518ab5b7cfe6bf7424a79ce3af9d6657/src/native/corehost/apphost/apphost.windows.cpp#L37-L51

        try
        {
            var eventSourceHandle = NativeMethods.RegisterEventSource(null, ".NET Runtime");

            try
            {
                var bootstrapperVersion = typeof(Error).Assembly.GetName().Version.ToString(3);
                var applicationName = Path.GetFileName(typeof(Error).Assembly.Location);
                var applicationFilePath = typeof(Error).Assembly.Location;

                var content =
                    "Description: Bootstrapper for a .NET application has failed." +
                    Environment.NewLine +
                    $"Application: {applicationName}" +
                    Environment.NewLine +
                    $"Path: {applicationFilePath}" +
                    Environment.NewLine +
                    $"AppHost: .NET Runtime Bootstrapper v{bootstrapperVersion} (https://github.com/Tyrrrz/DotnetRuntimeBootstrapper)" +
                    Environment.NewLine +
                    $"Message: {message}";

                NativeMethods.ReportEvent(
                    eventSourceHandle,
                    0x0001,
                    0,
                    1023, // matches standard .NET Runtime event ID
                    IntPtr.Zero,
                    1,
                    0,
                    new[] { content },
                    IntPtr.Zero
                );
            }
            finally
            {
                NativeMethods.DeregisterEventSource(eventSourceHandle);
            }
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
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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