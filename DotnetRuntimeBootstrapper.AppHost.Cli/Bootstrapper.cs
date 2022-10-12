using System;
using DotnetRuntimeBootstrapper.AppHost.Cli.Utils;
using DotnetRuntimeBootstrapper.AppHost.Core;
using DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;

namespace DotnetRuntimeBootstrapper.AppHost.Cli;

public class Bootstrapper : BootstrapperBase
{
    protected override void ReportError(string message)
    {
        base.ReportError(message);

        try
        {
            using (ConsoleEx.WithForegroundColor(ConsoleColor.DarkRed))
                Console.Error.WriteLine("ERROR: " + message);
        }
        catch
        {
            // Ignore
        }
    }

    protected override bool InstallPrerequisites(TargetAssembly targetAssembly, IPrerequisite[] missingPrerequisites)
    {
        var controller = new InstallationController(targetAssembly, missingPrerequisites);

        if (!controller.PromptInstallation())
            return false;

        return controller.PerformInstall();
    }

    public static int Main(string[] args) => new Bootstrapper().Run(args);
}