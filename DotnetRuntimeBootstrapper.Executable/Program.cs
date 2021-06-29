using System;
using System.IO;

namespace DotnetRuntimeBootstrapper.Executable
{
    public static class Program
    {
        private static void DumpError(string message)
        {
            // Write to a file because it might not always be possible to show an error dialog
            try
            {
                File.WriteAllText("BootstrapperErrorDump.txt", message);
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