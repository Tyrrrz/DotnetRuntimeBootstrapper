using System;
using System.Collections.Generic;
using DotnetRuntimeBootstrapper.AppHost.Cli.Utils;
using DotnetRuntimeBootstrapper.AppHost.Core;
using DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;
using OperatingSystem = DotnetRuntimeBootstrapper.AppHost.Core.Platform.OperatingSystem;

namespace DotnetRuntimeBootstrapper.AppHost.Cli;

public class InstallationController
{
    private const string PromptEnvironmentVariableName = "DOTNET_INSTALL_PREREQUISITES";

    private readonly TargetAssembly _targetAssembly;
    private readonly IPrerequisite[] _missingPrerequisites;

    public InstallationController(TargetAssembly targetAssembly, IPrerequisite[] missingPrerequisites)
    {
        _targetAssembly = targetAssembly;
        _missingPrerequisites = missingPrerequisites;
    }

    public bool PromptInstallation()
    {
        // Installation must be requested explicitly by setting the environment variable
        var isInstallationAccepted = string.Equals(
            Environment.GetEnvironmentVariable(PromptEnvironmentVariableName),
            "true",
            StringComparison.OrdinalIgnoreCase
        );

        if (!isInstallationAccepted)
        {
            using (ConsoleEx.WithForegroundColor(ConsoleColor.DarkRed))
            {
                Console.Error.WriteLine($"Your system is missing runtime components required by {_targetAssembly.Title}:");
                foreach (var prerequisite in _missingPrerequisites)
                    Console.Error.WriteLine($"  - {prerequisite.DisplayName}");
                Console.Error.WriteLine();
            }

            using (ConsoleEx.WithForegroundColor(ConsoleColor.Gray))
            {
                Console.Error.Write("To install the missing components automatically, set the environment variable ");

                using (ConsoleEx.WithForegroundColor(ConsoleColor.DarkCyan))
                    Console.Error.Write(PromptEnvironmentVariableName);

                Console.Error.Write(" to ");

                using (ConsoleEx.WithForegroundColor(ConsoleColor.DarkCyan))
                    Console.Error.Write("true");

                Console.Error.Write(", and then run the application again:");
                Console.Error.WriteLine();
            }

            using (ConsoleEx.WithForegroundColor(ConsoleColor.White))
            {
                Console.Error.Write($"  set {PromptEnvironmentVariableName}=true");
                using (ConsoleEx.WithForegroundColor(ConsoleColor.DarkGray))
                    Console.Error.Write("      (Command Prompt)");

                Console.Error.WriteLine();

                Console.Error.Write($"  $env:{PromptEnvironmentVariableName}=\"true\"");
                using (ConsoleEx.WithForegroundColor(ConsoleColor.DarkGray))
                    Console.Error.Write("   (Powershell)");

                Console.Error.WriteLine();
            }

            return false;
        }

        return true;
    }

    public bool PerformInstall()
    {
        using (ConsoleEx.WithForegroundColor(ConsoleColor.White))
            Console.Out.WriteLine($"{_targetAssembly.Title}: installing prerequisites");

        var currentStep = 1;
        var totalSteps = _missingPrerequisites.Length * 2;

        // Download
        var installers = new List<IPrerequisiteInstaller>();
        foreach (var prerequisite in _missingPrerequisites)
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
                Console.Out.WriteLine($"You need to restart Windows before you can run {_targetAssembly.Title}.");
                Console.Out.WriteLine("Would you like to do it now? [y/n]");
            }

            var isRebootAccepted = Console.ReadKey(true).Key == ConsoleKey.Y;
            if (isRebootAccepted)
                OperatingSystem.Reboot();

            return false;
        }

        return true;
    }
}