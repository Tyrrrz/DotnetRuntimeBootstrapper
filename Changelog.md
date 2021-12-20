### v2.0.3 (20-Dec-2021)

- Extended "Failed to initialize .NET host..." error with additional information. It will now contain error messages logged by `hostfxr.dll`, if they are available. This should help clarify the reason for the error in most of the cases.

### v2.0.2 (05-Dec-2021)

- Fixed an issue where the application host was not writing error dumps to `%localappdata%` if the subdirectory didn't already exist.

### v2.0.1 (05-Dec-2021)

- Fixed an issue where the MSBuild task was refusing to execute under Visual Studio, incorrectly claiming that the project is not targeting .NET Core.
- Fixed an issue where the MSBuild task warnings were not produced on build when `GenerateBootstrapperOnBuild` was enabled.

### v2.0 (05-Dec-2021)

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

### v1.1.2 (14-Jun-2021)

- Fixed an issue where the package was missing some required files. Again. I really hate MSBuild.

### v1.1.1 (14-Jun-2021)

- Fixed an issue where the package was missing some required files.

### v1.1 (14-Jun-2021)

- Changed how the installation window displays .NET runtime in the list of missing components. Now it's displayed as `.NET Runtime (WindowsDesktop) v5.0.0` instead of `Microsoft.WindowsDesktop.App v5.0.0`.
- Added "Ignore" button to the installation window that allows the user to bypass prerequisite checks and attempt to run the application anyway.
- Added supersession checks for KB3063858 (against KB3068708) and KB3154518 (against KB3125574). This should prevent detecting these Windows updates as missing in cases where the corresponding superseding update has already been installed.
- Added a safeguard that prevents the bootstrapper from attempting to install the same Windows update multiple times. This should help avoid installation dead loops caused by trying to install updates which are not applicable.
- Added a build step to inject application manifest inside the bootstrapper executable (as specified by the `<ApplicationManifest>` project property).
- Fixed an issue where the bootstrapper crashed in certain circumstances when attempting to set HTTPS security protocol.
- Fixed an issue where the product version on the bootstrapper executable was not injected properly during build.
- Fixed an issue where the description label was slightly misaligned in the installation window.
- Fixed an issue where the installation thread did not terminate if the installation window was closed.

### v1.0.1 (08-Jun-2021)

- Fixed an issue where the MSBuild task failed during publish if provided with an absolute path for the output directory.

### v1.0 (08-Jun-2021)

Initial release