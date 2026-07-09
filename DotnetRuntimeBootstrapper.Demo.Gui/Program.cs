using System;
using System.Linq;
using System.Windows.Forms;

namespace DotnetRuntimeBootstrapper.Demo.Gui;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Show form
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());

        // Show command line arguments
        if (args.Any())
        {
            MessageBox.Show(string.Join(" ", args), "Command line arguments");
        }
    }
}
