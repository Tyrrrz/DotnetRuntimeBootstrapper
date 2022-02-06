using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace DotnetRuntimeBootstrapper.AppHost.Utils;

internal static class Error
{
    private static void ReportToFile(string message)
    {
        try
        {
            var timestamp = DateTimeOffset.Now;
            var timestampFileSafe = timestamp.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            var version = typeof(Error).Assembly.GetName().Version.ToString(3);

            var content =
                $"Timestamp: {timestamp}" +
                Environment.NewLine +
                $"AppHost: .NET Runtime Bootstrapper v{version} (https://github.com/Tyrrrz/DotnetRuntimeBootstrapper)" +
                Environment.NewLine +
                $"Message: {message}";

            try
            {
                // Try writing to executing directory first
                var filePath = Path.Combine(
                    PathEx.ExecutingDirectoryPath,
                    $"AppHost_Error_{timestampFileSafe}.txt"
                );

                File.WriteAllText(filePath, content);
            }
            catch
            {
                // Otherwise, write to local app data
                var title = Path.GetFileNameWithoutExtension(typeof(Error).Assembly.Location);

                var dirPath = Path.Combine(
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Tyrrrz"
                    ),
                    "DotnetRuntimeBootstrapper"
                );

                Directory.CreateDirectory(dirPath);

                var filePath = Path.Combine(
                    dirPath,
                    $"AppHost_{title}_Error_{timestampFileSafe}.txt"
                );

                File.WriteAllText(filePath, content);
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
        // Report the error by writing it to a file and showing in a dialog.
        // Either one of the two can fail for various reasons, which
        // is why we use two approaches for redundancy.
        ReportToFile(message);
        ReportToUser(message);
    }

    public static void Report(Exception ex) => Report(ex.ToString());
}