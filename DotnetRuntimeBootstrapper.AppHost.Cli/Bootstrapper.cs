using System;
using System.Collections.Generic;
using DotnetRuntimeBootstrapper.AppHost.Cli.Utils;
using DotnetRuntimeBootstrapper.AppHost.Core;
using DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;
using OperatingSystem = DotnetRuntimeBootstrapper.AppHost.Core.Platform.OperatingSystem;

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

    protected override bool Prompt(
        TargetAssembly targetAssembly,
        IPrerequisite[] missingPrerequisites)
    {
        // Command line bootstrapper relies only on the environment variable for accepting the prompt.
        // If this method is called from the base class, it means that the environment variable is not set.
        // In this case, just display instructions on how to set the environment variable and return false.

        using (ConsoleEx.WithForegroundColor(ConsoleColor.DarkRed))
        {
            Console.Error.WriteLine($"Your system is missing runtime components required by {targetAssembly.Title}:");
            foreach (var prerequisite in missingPrerequisites)
                Console.Error.WriteLine($"  - {prerequisite.DisplayName}");
            Console.Error.WriteLine();
        }

        using (ConsoleEx.WithForegroundColor(ConsoleColor.Gray))
        {
            Console.Error.Write("To install the missing components automatically, set the environment variable ");

            using (ConsoleEx.WithForegroundColor(ConsoleColor.DarkCyan))
                Console.Error.Write(AcceptPromptEnvironmentVariable);

            Console.Error.Write(" to ");

            using (ConsoleEx.WithForegroundColor(ConsoleColor.DarkCyan))
                Console.Error.Write("true");

            Console.Error.Write(", and then run the application again:");
            Console.Error.WriteLine();
        }

        using (ConsoleEx.WithForegroundColor(ConsoleColor.White))
        {
            Console.Error.Write($"  set {AcceptPromptEnvironmentVariable}=true");
            using (ConsoleEx.WithForegroundColor(ConsoleColor.DarkGray))
                Console.Error.Write("      (Command Prompt)");

            Console.Error.WriteLine();

            Console.Error.Write($"  $env:{AcceptPromptEnvironmentVariable}=\"true\"");
            using (ConsoleEx.WithForegroundColor(ConsoleColor.DarkGray))
                Console.Error.Write("   (Powershell)");

            Console.Error.WriteLine();
        }

        return false;
    }

    protected override bool Install(
        TargetAssembly targetAssembly,
        IPrerequisite[] missingPrerequisites)
    {
        using (ConsoleEx.WithForegroundColor(ConsoleColor.White))
            Console.Out.WriteLine($"{targetAssembly.Title}: installing prerequisites");

        var currentStep = 1;
        var totalSteps = missingPrerequisites.Length * 2;

        // Download
        var installers = new List<IPrerequisiteInstaller>();
        foreach (var prerequisite in missingPrerequisites)
        {
            Console.Out.Write($"[{currentStep}/{totalSteps}] ");
            Console.Out.Write($"Downloading {prerequisite.DisplayName}... ");

            using (var progress = new ConsoleProgress(Console.Error))
            {
                var installer = prerequisite.DownloadInstaller(progress.Report);
                installers.Add(installer);
            }

            Console.Out.Write("Done");
            Console.Out.WriteLine();

            currentStep++;
        }

        // Install
        var isRebootRequired = false;
        foreach (var installer in installers)
        {
            Console.Out.Write($"[{currentStep}/{totalSteps}] ");
            Console.Out.Write($"Installing {installer.Prerequisite.DisplayName}... ");

            var installationResult = installer.Run();

            Console.Out.Write("Done");
            Console.Out.WriteLine();

            FileEx.TryDelete(installer.FilePath);

            if (installationResult == PrerequisiteInstallerResult.RebootRequired)
                isRebootRequired = true;

            currentStep++;
        }

        using (ConsoleEx.WithForegroundColor(ConsoleColor.White))
            Console.Out.WriteLine("Prerequisites installed successfully.");
        Console.Out.WriteLine();

        // Finalize
        if (isRebootRequired)
        {
            using (ConsoleEx.WithForegroundColor(ConsoleColor.DarkYellow))
            {
                Console.Out.WriteLine($"You need to restart Windows before you can run {targetAssembly.Title}.");
                Console.Out.WriteLine("Would you like to do it now? [y/n]");
            }

            var isRebootAccepted = Console.ReadKey(true).Key == ConsoleKey.Y;
            if (isRebootAccepted)
                OperatingSystem.Reboot();

            return false;
        }

        return true;
    }

    public static int Main(string[] args) => new Bootstrapper().Run(args);
}