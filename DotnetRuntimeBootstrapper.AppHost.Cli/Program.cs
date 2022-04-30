using System;
using System.Linq;
using DotnetRuntimeBootstrapper.AppHost.Core;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;

namespace DotnetRuntimeBootstrapper.AppHost.Cli;

public partial class Program
{
    private const string PromptEnvironmentVariableName = "DOTNET_INSTALL_PREREQUISITES";

    private readonly TargetAssembly _targetAssembly;

    public Program(TargetAssembly targetAssembly) =>
        _targetAssembly = targetAssembly;

    private bool EnsurePrerequisites()
    {
        var missingPrerequisites = _targetAssembly.GetMissingPrerequisites();
        if (!missingPrerequisites.Any())
            return true;

        var shouldInstall = string.Equals(
            Environment.GetEnvironmentVariable(PromptEnvironmentVariableName),
            "true",
            StringComparison.OrdinalIgnoreCase
        );

        // Perform installation
        if (shouldInstall)
        {
            // Refresh environment variables to update PATH and other variables
            // that may have been changed by the installation process.
            EnvironmentEx.RefreshEnvironmentVariables();

            return true;
        }
        // Prompt installation
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Error.WriteLine($"Your system is missing runtime components required by {_targetAssembly.Title}:");
            foreach (var prerequisite in missingPrerequisites)
                Console.Error.WriteLine($"  - {prerequisite.DisplayName}");
            Console.Error.WriteLine();
            Console.ResetColor();

            Console.Error.Write("To install the missing components automatically, set environment variable ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Error.Write(PromptEnvironmentVariableName);
            Console.ResetColor();
            Console.Error.Write(" to ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Error.Write("true");
            Console.ResetColor();
            Console.Error.Write(", and then run the application again:");
            Console.Error.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.Error.Write($"  set {PromptEnvironmentVariableName}=true");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Error.Write("      (Command Prompt)");
            Console.Error.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Error.Write($"  $env:{PromptEnvironmentVariableName}=\"true\"");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Error.Write("   (Powershell)");
            Console.Error.WriteLine();
            Console.ResetColor();

            return false;
        }
    }

    private int Run(string[] args)
    {
        try
        {
            // Attempt to run the target first without any checks (hot path)
            return _targetAssembly.Run(args);
        }
        // Possible exception causes:
        // - .NET host not found (DirectoryNotFoundException)
        // - .NET host failed to initialize (ApplicationException)
        catch
        {
            if (!EnsurePrerequisites())
            {
                // User canceled or reboot is required
                return 0xB007;
            }

            // Attempt to run the target again
            return _targetAssembly.Run(args);
        }
    }
}

public partial class Program
{
    public static int Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Error.Report(e.ExceptionObject.ToString());

        try
        {
            return new Program(TargetAssembly.Resolve()).Run(args);
        }
        catch (Exception ex)
        {
            Error.Report(ex);
            return 0xDEAD;
        }
    }
}