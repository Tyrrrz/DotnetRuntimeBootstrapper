using System;
using System.IO;
using System.Windows.Forms;

namespace DotnetRuntimeBootstrapper.Executable
{
    public static class Program
    {
        private static void DumpError(string message)
        {
            // Report errors by writing them to a file and showing a dialog.
            // Either one of the two can fail for various reasons, which
            // is why we use two approaches for redundancy.

            try
            {
                File.WriteAllText("BootstrapperErrorDump.txt", message);
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

        [STAThread]
        public static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (_, e) => DumpError(e.ExceptionObject.ToString());

            try
            {
                var app = new App();
                return app.Run(args);
            }
            catch (Exception ex)
            {
                DumpError(ex.ToString());
                return 1;
            }
        }
    }
}