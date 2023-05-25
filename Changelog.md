# Changelog

## v2.5.1 (25-May-2023)

- Fixed an issue where the CLI-based bootstrapper crashed if one of the standard streams were redirected.

## v2.5 (18-May-2023)

- Added an interactive installation prompt to the CLI-based bootstrapper. This prompt is only shown if the user is running the application in a console window, and won't be shown in headless scenarios, such as CI workflows. The previous behavior, which involves setting an environment variable to accept the prompt, is also supported and will bypass the interactive prompt.

## v2.4.2 (27-Apr-2023)

- Improved bootstrapper performance when running on older operating systems.

## v2.4.1 (13-Apr-2023)

- Simplified the process of injecting native resources into the application host. Instead of searching for specific resource types and copying them, the build task now simply copies all resources from the target assembly to the host.

## v2.4 (01-Dec-2022)

- Changed the name of the environment variable used for accepting the installation prompt in the console bootstrapper from `DOTNET_INSTALL_PREREQUISITES` to `DOTNET_ENABLE_BOOTSTRAPPER`, for more consistency with existing .NET environment variables. The old environment variable will also continue to be supported for backwards compatibility.
- Added support for accepting the installation prompt using an environment variable in the GUI bootstrapper as well. Setting the environment variable `DOTNET_ENABLE_BOOTSTRAPPER` to `true` will instruct the bootstrapper to skip the confirmation prompt and immediately begin installing the missing prerequisites.
- Added an option to disable the installation prompt by setting the `<BootstrapperPromptRequired>` project property to `false`. This is particularly useful if you're bootstrapping an interactive console application, as it removes the requirement to set the environment variable. With this property set to `false`, bootstrapper will immediately begin installing the missing prerequisites without awaiting confirmation from the user.
- Fixed an issue where the bootstrapper failed silently if the target assembly crashed with an unhandled exception. It now shows an error message indicating the failure and provides tips on how to diagnose it. Unfortunately, it's still not possible to retrieve the message or the stacktrace of the original exception. To avoid this issue altogether, it's recommended to add a global unhandled exception handler in your application, where you can provide a more relevant error message to the user.
- Fixed an issue where the bootstrapper made insecure HTTP requests even on operating systems that support modern security protocols. Now it only downgrades HTTPS to HTTP if the current system doesn't support it.

## v2.3.1 (10-Jun-2022)

- Fixed an issue where the CLI-based bootstrapper was incorrectly created for GUI applications.
- Fixed an issue where running the CLI-based bootstrapper with output redirected resulted in a crash.
- Fixed minor stylistic issues in the GUI-based bootstrapper.

## v2.3 (10-Jun-2022)

- Added console variant of the application host. This variant will be used automatically for non-desktop applications (i.e. not `WinExe`). You can also specify the variant explicitly by setting the `<BootstrapperVariant>` project property to either `CLI` or `GUI`.
- Fixed an issue where the base runtime was not installed for applications targeting the ASP.NET runtime.

## v2.2 (10-Feb-2022)

- Changed error logging approach to use Windows Event Log instead of relying on the file system. If the bootstrapper crashes with a fatal error, it will now write a new entry to the event log with `1023` as event ID and `.NET Runtime` as source. Readme has been updated with new troubleshooting instructions.
- Improved error messages. When .NET host fails to initialize, the error will now also show the returned status code.
- Fixed an issue which caused the bootstrapper to crash when running from a network directory.

## v2.1 (06-Feb-2022)

- Added support for bootstrapping applications that have multiple non-base frameworks declared in their `runtimeconfig.json` (for example `WindowsDesktop` together with `AspNetCore`). The bootstrapper will now install all of the corresponding runtimes instead of just one. (Thanks [@Tomasz](https://github.com/Misiu))
- Added logic that determines whether a system restart is required after prerequisite installation based on the exit code provided by the installer process. Previously this behavior was hard-codded.
- Added zero exit code assertion for prerequisite installers. If an installer terminates with a non-zero exit code (except when it signals a system restart), the bootstrapper will fail with an error.
- Fixed an issue which caused the bootstrapper to show an error saying "Hosting components are already initialized. Re-initialization for an app is not allowed." when the target application crashed due to an unhandled exception. This error message should no longer be shown in such cases.

## v2.0.3 (20-Dec-2021)

- Extended "Failed to initialize .NET host..." error with additional information. It will now contain error messages logged by `hostfxr.dll`, if they are available. This should help clarify the reason for the error in most of the cases.

## v2.0.2 (05-Dec-2021)

- Fixed an issue where the application host was not writing error dumps to `%localappdata%` if the subdirectory didn't already exist.

## v2.0.1 (05-Dec-2021)

- Fixed an issue where the MSBuild task was refusing to execute under Visual Studio, incorrectly claiming that the project is not targeting .NET Core.
- Fixed an issue where the MSBuild task warnings were not produced on build when `GenerateBootstrapperOnBuild` was enabled.

## v2.0 (05-Dec-2021)

- Reimplemented the bootstrapper executable as a custom .NET runtime host. It now executes the target application by leveraging `hostfxr.dll` instead of the .NET CLI. This means that it no longer creates a separate process to host the application.
- Optimized bootstrapper's execution flow to prioritize hot path. It will now attempt to run the application first and only check for missing prerequisites if that attempt fails.
- Improved the strategy used to resolve the target .NET runtime name and version. It now relies on the `runtimeconfig.json` file instead of the `<TargetFramework>` project property.
- Improved the strategy used to resolve the download URL for the target .NET runtime. It now relies on the release manifest resources published to .NET CLI's blob storage, instead of manual web scraping of the download page.
- Improved the strategy used to resolve .NET installation path. It now relies on the system registry and additional fallback heuristics, instead of assuming that `dotnet` is always on the `PATH`.
- Improved bootstrapper's user interface. Bootstrapper now also displays the correct application icon in the title bar and task bar.
- Improved error handling in the bootstrapper. If it doesn't have enough permissions to write the error dump to the application directory, it will now write it to `%localappdata%\Tyrrrz\DotnetRuntimeBootstrapper` instead.
- Removed the "Ignore" button from the prerequisite installation prompt. Now that the prerequisite check happens after the initial attempt to execute the application, this is no longer necessary.
- Removed `KB3154518` from the list of prerequisites for Windows 7 as it is no longer available. It wasn't strictly required anyway.
- Fixed an issue where the target application did not inherit native resources from the bootstrapper's PE file.
- Fixed an issue where the launched child processes (such as installers) were not killed if the bootstrapper was forcibly closed.
- Fixed an issue where the installation of Visual C++ Redistributable was not correctly verified on 32-bit systems.
- Replaced `rcedit` CLI in MSBuild task with [Ressy](https://github.com/Tyrrrz/Ressy).
- Changed the default behavior of the MSBuild task to not create a bootstrapper on build, but instead only do it on publish. This can be re-enabled by setting the `<GenerateBootstrapperOnBuild>` project property to `true`.
- Other minor improvements and changes which are covered in the readme.

If you previously relied on `CurrentProcess.MainModule.FileName` or similar to get the path to your application, note that it will now return the correct path to the executable instead of the path to .NET CLI.

If you previously used a conditional package reference for DotnetRuntimeBootstrapper (e.g. using it only in `Release` configuration), note that it's not required anymore because the bootstrapper is no longer created for build targets (by default).

## v1.1.2 (14-Jun-2021)

- Fixed an issue where the package was missing some required files. Again. I really hate MSBuild.

## v1.1.1 (14-Jun-2021)

- Fixed an issue where the package was missing some required files.

## v1.1 (14-Jun-2021)

- Changed how the installation window displays .NET runtime in the list of missing components. Now it's displayed as `.NET Runtime (WindowsDesktop) v5.0.0` instead of `Microsoft.WindowsDesktop.App v5.0.0`.
- Added "Ignore" button to the installation window that allows the user to bypass prerequisite checks and attempt to run the application anyway.
- Added supersession checks for KB3063858 (against KB3068708) and KB3154518 (against KB3125574). This should prevent detecting these Windows updates as missing in cases where the corresponding superseding update has already been installed.
- Added a safeguard that prevents the bootstrapper from attempting to install the same Windows update multiple times. This should help avoid installation dead loops caused by trying to install updates which are not applicable.
- Added a build step to inject application manifest inside the bootstrapper executable (as specified by the `<ApplicationManifest>` project property).
- Fixed an issue where the bootstrapper crashed in certain circumstances when attempting to set HTTPS security protocol.
- Fixed an issue where the product version on the bootstrapper executable was not injected properly during build.
- Fixed an issue where the description label was slightly misaligned in the installation window.
- Fixed an issue where the installation thread did not terminate if the installation window was closed.

## v1.0.1 (08-Jun-2021)

- Fixed an issue where the MSBuild task failed during publish if provided with an absolute path for the output directory.

## v1.0 (08-Jun-2021)

Initial release