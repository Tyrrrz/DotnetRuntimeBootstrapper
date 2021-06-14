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