using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DotnetRuntimeBootstrapper.AppHost.Cli.Utils;
using DotnetRuntimeBootstrapper.AppHost.Core;
using DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;
using OperatingSystem = DotnetRuntimeBootstrapper.AppHost.Core.Platform.OperatingSystem;

namespace DotnetRuntimeBootstrapper.AppHost.Cli;

public class Bootstrapper : BootstrapperBase
{
    private const string PromptEnvironmentVariableName = "DOTNET_INSTALL_PREREQUISITES";

    protected override void ReportError(string message)
    {
        // Report to Windows Event Log
        // Inspired by:
        // https://github.com/dotnet/runtime/blob/57bfe474518ab5b7cfe6bf7424a79ce3af9d6657/src/native/corehost/apphost/apphost.windows.cpp#L37-L51
        try
        {
            var applicationFilePath = typeof(Bootstrapper).Assembly.Location;
            var applicationName = Path.GetFileName(applicationFilePath);
            var bootstrapperVersion = typeof(Bootstrapper).Assembly.GetName().Version.ToString(3);

            var content = string.Join(
                Environment.NewLine,
                new[]
                {
                    "Description: Bootstrapper for a .NET application has failed.",
                    "Application: " + applicationName,
                    "Path: " + applicationFilePath,
                    "AppHost: " + $".NET Runtime Bootstrapper v{bootstrapperVersion} (CLI)",
                    "Message: " + message
                }
            );

            EventLog.WriteEntry(".NET Runtime", content, EventLogEntryType.Error, 1023);
        }
        catch
        {
            // Ignore
        }

        // Report to the console
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

    private bool PromptInstallation(
        TargetAssembly targetAssembly,
        IPrerequisite[] missingPrerequisites)
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
                Console.Error.WriteLine($"Your system is missing runtime components required by {targetAssembly.Title}:");
                foreach (var prerequisite in missingPrerequisites)
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

    private bool PerformInstallation(
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
            Console.Out.Write($"[{currentStep} / {totalSteps}] ");
            Console.Out.Write($"Downloading {prerequisite.DisplayName}... ");

            using var progress = new ConsoleProgress(Console.Out);
            {
                var installer = prerequisite.DownloadInstaller(progress.Report);
                installers.Add(installer);
            }

            Console.Out.WriteLine();

            currentStep++;
        }

        // Install
        var isRebootRequired = false;
        foreach (var installer in installers)
        {
            Console.Out.Write($"[{currentStep} / {totalSteps}] ");
            Console.Out.Write($"Installing {installer.Prerequisite.DisplayName}...");
            Console.Out.WriteLine();

            if (installer.Run() == PrerequisiteInstallerResult.RebootRequired)
                isRebootRequired = true;

            FileEx.TryDelete(installer.FilePath);

            currentStep++;
        }

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

    protected override bool InstallPrerequisites(
        TargetAssembly targetAssembly,
        IPrerequisite[] missingPrerequisites)
    {
        // Prompt the user
        if (!PromptInstallation(targetAssembly, missingPrerequisites))
            return false;

        // Perform the installation
        return PerformInstallation(targetAssembly, missingPrerequisites);
    }
}