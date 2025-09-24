using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;

namespace DotnetRuntimeBootstrapper.AppHost.Core;

public abstract class BootstrapperBase
{
    protected const string LegacyAcceptPromptEnvironmentVariable = "DOTNET_INSTALL_PREREQUISITES";
    protected const string AcceptPromptEnvironmentVariable = "DOTNET_ENABLE_BOOTSTRAPPER";

    protected BootstrapperConfiguration Configuration { get; } =
        BootstrapperConfiguration.Resolve();

    protected abstract void ReportError(string message);

    private void HandleException(Exception exception)
    {
        // For domain-level exceptions, report only the message
        if (exception is BootstrapperException bootstrapperException)
        {
            try
            {
                ReportError(bootstrapperException.Message);
            }
            catch
            {
                // Ignore
            }
        }
        // For other (unexpected) exceptions, report the full stack trace and
        // record the error to the Windows Event Log.
        else
        {
            try
            {
                ReportError(exception.ToString());
            }
            catch
            {
                // Ignore
            }

            // Report to the Windows Event Log. Adapted from:
            // https://github.com/dotnet/runtime/blob/57bfe474518ab5b7cfe6bf7424a79ce3af9d6657/src/native/corehost/apphost/apphost.windows.cpp#L37-L51
            try
            {
                var applicationFilePath = Assembly.GetExecutingAssembly().Location;
                var applicationName = Path.GetFileName(applicationFilePath);

                var bootstrapperVersion = Assembly
                    .GetExecutingAssembly()
                    .GetName()
                    .Version.ToString(3);

                var content = $"""
                    Description: Bootstrapper for a .NET application has failed.
                    Application: {applicationName}
                    Path: {applicationFilePath}
                    AppHost: .NET Runtime Bootstrapper v{bootstrapperVersion}

                    {exception}
                    """;

                EventLog.WriteEntry(".NET Runtime", content, EventLogEntryType.Error, 1023);
            }
            catch
            {
                // Ignore
            }
        }
    }

    protected abstract bool Prompt(
        TargetAssembly targetAssembly,
        IPrerequisite[] missingPrerequisites
    );

    protected abstract bool Install(
        TargetAssembly targetAssembly,
        IPrerequisite[] missingPrerequisites
    );

    private bool PromptAndInstall(
        TargetAssembly targetAssembly,
        IPrerequisite[] missingPrerequisites
    )
    {
        // Install prompt can be disabled in bootstrap configuration or via environment variable
        var isPromptPreAccepted =
            !Configuration.IsPromptRequired
            || string.Equals(
                Environment.GetEnvironmentVariable(AcceptPromptEnvironmentVariable),
                "true",
                StringComparison.OrdinalIgnoreCase
            )
            || string.Equals(
                Environment.GetEnvironmentVariable(LegacyAcceptPromptEnvironmentVariable),
                "true",
                StringComparison.OrdinalIgnoreCase
            );

        var isPromptAccepted = isPromptPreAccepted || Prompt(targetAssembly, missingPrerequisites);

        return isPromptAccepted && Install(targetAssembly, missingPrerequisites);
    }

    private int Run(TargetAssembly targetAssembly, string[] args)
    {
        // need to check the prerequisites first, otherwise the application may start but fail later
        // e.g. VS runtime 2019 are installed, but app needs 2022
        // e.g. .NET 8.0.4 is installed but 8.0.20 is desired
        // both conditions does not prevent starting the app, but is not what the dev desired.

	    // Check for missing prerequisites and install them
	    var missingPrerequisites = targetAssembly.GetMissingPrerequisites();
	    if (!missingPrerequisites.Any()) 
		    return targetAssembly.Run(args);

	    var isReadyToRun = PromptAndInstall(targetAssembly, missingPrerequisites);

	    // User did not accept the installation or reboot is required
	    if (!isReadyToRun)
		    return 0xB007;

	    // Reset the environment to update PATH and other variables
	    // that may have been changed by the installation process.
	    EnvironmentEx.RefreshEnvironmentVariables();

	    // Attempt to run the target
	    return targetAssembly.Run(args);
    }

    public int Run(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            HandleException((Exception)e.ExceptionObject);

        try
        {
            var targetAssembly = TargetAssembly.Resolve(
                Path.Combine(
                    Path.GetDirectoryName(EnvironmentEx.ProcessPath)
                        ?? AppDomain.CurrentDomain.BaseDirectory,
                    Configuration.TargetFileName
                )
            );

            return Run(targetAssembly, args);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return 0xDEAD;
        }
    }
}
