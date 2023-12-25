using System;
using System.Collections.Generic;
using System.IO;
using DotnetRuntimeBootstrapper.AppHost.Cli.Utils;
using DotnetRuntimeBootstrapper.AppHost.Core;
using DotnetRuntimeBootstrapper.AppHost.Core.Platform;
using DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;

namespace DotnetRuntimeBootstrapper.AppHost.Cli;

public class Bootstrapper : BootstrapperBase
{
    protected override void ReportError(string message)
    {
        using (ConsoleEx.WithForegroundColor(ConsoleColor.DarkRed))
            Console.Error.WriteLine("ERROR: " + message);
    }

    protected override bool Prompt(
        TargetAssembly targetAssembly,
        IPrerequisite[] missingPrerequisites
    )
    {
        using (ConsoleEx.WithForegroundColor(ConsoleColor.DarkRed))
        {
            Console
                .Error
                .WriteLine(
                    $"Your system is missing runtime components required by {targetAssembly.Name}:"
                );

            foreach (var prerequisite in missingPrerequisites)
                Console.Error.WriteLine($"  - {prerequisite.DisplayName}");

            Console.Error.WriteLine();
        }

        // When running in interactive mode, prompt the user directly
        if (ConsoleEx.IsInteractive)
        {
            Console.Error.Write("Would you like to download and install them now?");

            using (ConsoleEx.WithForegroundColor(ConsoleColor.DarkCyan))
                Console.Error.Write(" [y/n]");
            Console.Error.WriteLine();

            return Console.ReadKey(true).Key == ConsoleKey.Y;
        }
        // When not running in interactive mode, instruct the user to set the environment variable instead
        else
        {
            Console
                .Error
                .Write(
                    "To install the missing components automatically, set the environment variable "
                );

            using (ConsoleEx.WithForegroundColor(ConsoleColor.DarkCyan))
                Console.Error.Write(AcceptPromptEnvironmentVariable);

            Console.Error.Write(" to ");

            using (ConsoleEx.WithForegroundColor(ConsoleColor.DarkCyan))
                Console.Error.Write("true");

            Console.Error.Write(", and then run the application again:");
            Console.Error.WriteLine();

            using (ConsoleEx.WithForegroundColor(ConsoleColor.White))
                Console.Error.Write($"  set {AcceptPromptEnvironmentVariable}=true");

            Console.Error.Write("      (Command Prompt)");
            Console.Error.WriteLine();

            using (ConsoleEx.WithForegroundColor(ConsoleColor.White))
                Console.Error.Write($"  $env:{AcceptPromptEnvironmentVariable}=\"true\"");

            Console.Error.Write("   (Powershell)");
            Console.Error.WriteLine();
        }

        return false;
    }

    protected override bool Install(
        TargetAssembly targetAssembly,
        IPrerequisite[] missingPrerequisites
    )
    {
        using (ConsoleEx.WithForegroundColor(ConsoleColor.White))
            Console.Out.WriteLine($"{targetAssembly.Name}: installing prerequisites");

        var currentStep = 1;
        var totalSteps = missingPrerequisites.Length * 2;

        // Download
        var installers = new List<IPrerequisiteInstaller>();
        foreach (var prerequisite in missingPrerequisites)
        {
            Console.Out.Write($"[{currentStep}/{totalSteps}] ");
            Console.Out.Write($"Downloading {prerequisite.DisplayName}... ");

            // Only write progress if running in interactive mode
            using (
                var progress = new ConsoleProgress(
                    ConsoleEx.IsInteractive ? Console.Out : TextWriter.Null
                )
            )
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
                Console
                    .Out
                    .WriteLine(
                        $"You need to restart Windows before you can run {targetAssembly.Name}."
                    );

            // Only prompt for reboot if running in interactive mode
            if (ConsoleEx.IsInteractive)
            {
                Console.Out.WriteLine("Would you like to do it now?");
                using (ConsoleEx.WithForegroundColor(ConsoleColor.DarkCyan))
                    Console.Out.Write(" [y/n]");
                Console.Out.WriteLine();

                var isRebootAccepted = Console.ReadKey(true).Key == ConsoleKey.Y;
                if (isRebootAccepted)
                    OperatingSystemEx.Reboot();
            }

            return false;
        }

        return true;
    }

    public static int Main(string[] args) => new Bootstrapper().Run(args);
}
