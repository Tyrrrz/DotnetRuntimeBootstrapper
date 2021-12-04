using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.AppHost.Utils;

namespace DotnetRuntimeBootstrapper.AppHost
{
    // Global error sink
    public static class Error
    {
        public static void Report(string message)
        {
            // Report the error by writing it to a file and showing in a dialog.
            // Either one of the two can fail for various reasons, which
            // is why we use two approaches for redundancy.

            try
            {
                var timestamp = DateTimeOffset.Now;
                var timestampFileSafe = timestamp.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                var version = typeof(Error).Assembly.GetName().Version.ToString(3);

                var filePath = Path.Combine(PathEx.ExecutingDirectoryPath, $"AppHost_Error_{timestampFileSafe}.txt");

                File.WriteAllText(
                    filePath,
                    $"Timestamp: {timestamp}" +
                    Environment.NewLine +
                    $"AppHost: .NET Runtime Bootstrapper v{version} (https://github.com/Tyrrrz/DotnetRuntimeBootstrapper)" +
                    Environment.NewLine +
                    $"Message: {message}"
                );
            }
            catch
            {
                // Ignore
            }

            try
            {
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                // Ignore
            }
        }

        public static void Report(Exception ex) => Report(ex.ToString());
    }
}