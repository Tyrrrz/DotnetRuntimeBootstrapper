# .NET Runtime Bootstrapper

[![Status](https://img.shields.io/badge/status-maintenance-ffd700.svg)](https://github.com/Tyrrrz/.github/blob/master/docs/project-status.md)
[![Made in Ukraine](https://img.shields.io/badge/made_in-ukraine-ffd700.svg?labelColor=0057b7)](https://tyrrrz.me/ukraine)
[![Build](https://img.shields.io/github/actions/workflow/status/Tyrrrz/DotnetRuntimeBootstrapper/main.yml?branch=master)](https://github.com/Tyrrrz/DotnetRuntimeBootstrapper/actions)
[![Version](https://img.shields.io/nuget/v/DotnetRuntimeBootstrapper.svg)](https://nuget.org/packages/DotnetRuntimeBootstrapper)
[![Downloads](https://img.shields.io/nuget/dt/DotnetRuntimeBootstrapper.svg)](https://nuget.org/packages/DotnetRuntimeBootstrapper)
[![Discord](https://img.shields.io/discord/869237470565392384?label=discord)](https://discord.gg/2SUWKFnHSm)
[![Fuck Russia](https://img.shields.io/badge/fuck-russia-e4181c.svg?labelColor=000000)](https://twitter.com/tyrrrz/status/1495972128977571848)

<table>
    <tr>
        <td width="99999" align="center">Development of this project is entirely funded by the community. <b><a href="https://tyrrrz.me/donate">Consider donating to support!</a></b></td>
    </tr>
</table>

<p align="center">
    <img src="favicon.png" alt="Icon" />
</p>

**.NET Runtime Bootstrapper** is an MSBuild plugin that replaces the default application host `exe` file — generated for Windows executables during the build process — with a fully featured bootstrapper that can automatically download and install the .NET runtime and other missing components required by your application.

## Terms of use<sup>[[?]](https://github.com/Tyrrrz/.github/blob/master/docs/why-so-political.md)</sup>

By using this project or its source code, for any purpose and in any shape or form, you grant your **implicit agreement** to all the following statements:

- You **condemn Russia and its military aggression against Ukraine**
- You **recognize that Russia is an occupant that unlawfully invaded a sovereign state**
- You **support Ukraine's territorial integrity, including its claims over temporarily occupied territories of Crimea and Donbas**
- You **reject false narratives perpetuated by Russian state propaganda**

To learn more about the war and how you can help, [click here](https://tyrrrz.me/ukraine). Glory to Ukraine! 🇺🇦

## Install

- 📦 [NuGet](https://nuget.org/packages/DotnetRuntimeBootstrapper): `dotnet add package DotnetRuntimeBootstrapper`

## Why?

Currently, .NET offers two main ways of [distributing applications](https://docs.microsoft.com/en-us/dotnet/core/deploying): **framework-dependent** deployment and **self-contained** deployment.
Both of them come with a set of obvious and somewhat less obvious drawbacks.

- **Framework-dependent** deployment:
  - Requires the user to have the correct .NET runtime installed on their machine. Not only will many users inevitably miss or ignore this requirement, the task of installing the _correct_ .NET runtime can be very challenging for non-technical individuals. Depending on their machine and the specifics of your application, they will need to carefully examine the [download page](https://dotnet.microsoft.com/download/dotnet/8.0/runtime) to find the installer for the right version, framework (i.e. base, desktop, or aspnet), CPU architecture, and operating system.
  - Comes with an application host that is _not platform-agnostic_. Even though the application itself (the `dll` file) is portable in the sense that it can run on any platform where the target runtime is supported, the application host (the `exe` file) is a native executable built for a specific platform (by default, the same platform as the dev machine). This means that if the application was built on Windows x64, a user running on Windows x86 will not be able to launch the application through the `exe` file, even if they have the correct runtime installed (`dotnet myapp.dll` will still work, however).
- **Self-contained** deployment:
  - While eliminating the need for installing the correct runtime, this method comes at a significant file size overhead. A very basic WinForms application, for example, starts at around 100 MB in size. This can be very cumbersome when doing auto-updates, but also seems quite wasteful if you consider that the user may end up with multiple .NET applications each bringing their own runtime.
  - Targets a specific platform, which means that you have to provide separate binaries for each operating system and processor architecture that you intend to support. Additionally, it can also create confusion among non-technical users, who may have a hard time figuring out which of the release binaries they need to download.
  - Snapshots a specific version of the runtime when it's produced. This means that your application won't be able to benefit from newer releases of the runtime — which may potentially contain performance or security improvements — unless you deploy a new version of the application.
  - Is, in fact, _not completely self-contained_. Depending on the user's machine, they might still need to install the Visual C++ runtime or certain Windows updates, neither of which are packaged with the application. Although this is only required for older operating systems, it may still affect a significant portion of your user base.

**.NET Runtime Bootstrapper** seeks to solve all the above problems by providing an alternative, third deployment option — **bootstrapped** deployment.

- **Bootstrapped** deployment:
  - Takes care of installing the target .NET runtime automatically. All the user has to do is accept the prompt and the bootstrapper will download and install the correct version of the runtime on its own.
  - Can also automatically install the Visual C++ runtime and missing Windows updates, when running on older operating systems. This means that users who are still using Windows 7 will have just as seamless experience as those running on Windows 11.
  - Does not impose any file size overhead as it does not package additional files. Missing prerequisites are downloaded on-demand.
  - Allows your application to benefit from newer releases of the runtime that the user might install in the future. When deploying your application, you are only tying it to a _minimum_ .NET version within the same major.
  - Is _truly portable_ because the provided application host is a platform-agnostic .NET Framework 3.5 executable that works out-of-the-box on all environments starting with Windows 7. This means that you only need to share a single distribution of your application, without worrying about different CPU architectures or other details.

## Features

- Executes the target assembly in-process using a custom runtime host
- Provides a GUI-based or a CLI-based host, depending on the application
- Detects and installs missing dependencies:
  - Required version of the .NET runtime
  - Required Visual C++ binaries
  - Required Windows updates
- Works out-of-the-box on Windows 7 and higher
- Supports all platforms in a single executable
- Integrates seamlessly inside the build process
- Retains native resources, such as version info, manifest, and icons
- Imposes no overhead in file size or performance

## Video

https://user-images.githubusercontent.com/1935960/123711355-346ed380-d825-11eb-982f-6272a9e55ebd.mp4

## Usage

To add **.NET Runtime Bootstrapper** to your project, simply install the corresponding [NuGet package](https://nuget.org/packages/DotnetRuntimeBootstrapper).
MSBuild will automatically pick up the `props` and `targets` files provided by the package and integrate them inside the build process.
After that, no further configuration is required.

### Publishing

In order to create a sharable distribution of your application, run `dotnet publish` as you normally would.
This should produce the following files in the output directory:

```txt
MyApp.exe                 <-- bootstrapper's application host
MyApp.exe.config          <-- assembly config required by the application host
MyApp.runtimeconfig.json  <-- runtime config required by the application host
MyApp.dll                 <-- main assembly of your application
MyApp.pdb
MyApp.deps.json
... other application dependencies ...
```

Make sure to include all highlighted files in your application distribution.

> **Warning**:
> Single-file deployment (`/p:PublishSingleFile=true`) is not supported by the bootstrapper.

### Application host

The client-facing side of **.NET Runtime Bootstrapper** is implemented as a [custom .NET runtime host](https://docs.microsoft.com/en-us/dotnet/core/tutorials/netcore-hosting).
It's generated during the build process by injecting project-specific instructions into a special pre-compiled executable provided by the package.
Internally, the host executable is a managed .NET Framework v3.5 assembly, which allows it to run out-of-the-box on all platforms starting with Windows 7.

When the user executes the application using the bootstrapper, it goes through the following steps:

```mermaid
flowchart
    1[Locate an existing .NET installation] --> 1a(Found?)
    1a -- Yes --> 2
    1a -- No --> 3

    2[Run the app using latest hostfxr.dll] --> 2a(Successful?)
    2a -- Yes --> 2b[Wait until exit]
    2a -- No --> 3

    3[Resolve target runtime from runtimeconfig.json] -->
    4[Identify missing prerequisites] -->
    5[Prompt the user to install them] -->
    6[Download and install] --> 6a(Reboot required?)
    6a -- Yes --> 6b[Prompt the user to reboot] --> 6c[Reboot] --> 1
    6a -- No --> 1
```

### Application resources

When the bootstrapper is created, the build task copies all native resources from the target assembly into the application host.
This includes:

- Application manifest (resource type: `24`). Configured by the `<ApplicationManifest>` project property.
- Application icon (resource types: `3` and `14`). Configured by the `<ApplicationIcon>` project property.
- Version info (resource type: `16`). Contains values configured by `<FileVersion>`, `<InformationalVersion>`, `<Product>`, `<Copyright>`, and other similar project properties.

Additionally, version info resource is further modified to contain the following attributes:

- `InternalName` set to the application host's file name.
- `OriginalName` set to the application host's file name.
- `AppHost` set to `.NET Runtime Bootstrapper vX.Y.Z (VARIANT)` where `X.Y.Z` is the version of the bootstrapper and `VARIANT` is either `CLI` or `GUI`.

### Customizing behavior

#### Generate bootstrapper on build

By default, bootstrapper is only created when publishing the project (i.e. when running `dotnet publish`).
If you want to also have it created on regular builds as well, set the `<GenerateBootstrapperOnBuild>` project property to `true`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <!-- ... -->

    <!-- Create bootstrapper on every build, in addition to every publish -->
    <GenerateBootstrapperOnBuild>true</GenerateBootstrapperOnBuild>
  </PropertyGroup>

  <!-- ... -->

</Project>
```

> **Warning**:
> Bootstrapper's application host does not support debugging.
> In order to retain debugging capabilities of your application during local development, keep `<GenerateBootstrapperOnBuild>` set to `false` (default).

#### Override bootstrapper variant

Depending on your application type (i.e. the value of the `<OutputType>` project property), the build process will generate either a CLI-based or a GUI-based bootstrapper.
You can override the default behavior and specify the preferred variant explicitly using the `<BootstrapperVariant>` project property:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <!-- ... -->

    <!-- Specify bootstrapper variant explicitly (GUI or CLI) -->
    <BootstrapperVariant>GUI</BootstrapperVariant>
  </PropertyGroup>

  <!-- ... -->

</Project>
```

#### Override runtime version

**DotnetRuntimeBootstrapper** relies on the autogenerated `runtimeconfig.json` file to determine the version of the runtime required by your application.
You can override the default value (which is inferred from the `<TargetFramework>` project property) by using the `<RuntimeFrameworkVersion>` project property:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <!-- ... -->

    <!-- Specify target runtime version explicitly -->
    <RuntimeFrameworkVersion>8.0.1</RuntimeFrameworkVersion>
  </PropertyGroup>

  <!-- ... -->

</Project>
```

#### Disable confirmation prompt

By default, the bootstrapper will prompt the user to confirm the installation of missing prerequisites.
You can disable this prompt by setting the `<BootstrapperPromptRequired>` project property to `false`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <!-- ... -->

    <!-- Skip the confirmation prompt and install prerequisites straight away -->
    <BootstrapperPromptRequired>false</BootstrapperPromptRequired>
  </PropertyGroup>

  <!-- ... -->

</Project>
```

### Troubleshooting

#### Build task logs

If the build process does not seem to generate the bootstrapper correctly, you may be able to get more information by running the command with higher verbosity.
For example, running `dotnet publish --verbosity normal` should produce an output that includes the following section:

```txt
CreateBootstrapperAfterPublish:
 Bootstrapper target: 'f:\Projects\Softdev\DotnetRuntimeBootstrapper\DotnetRuntimeBootstrapper.Demo.Gui\bin\Debug\net6.0-windows\DotnetRuntimeBootstrapper.Demo.dll'.
 Bootstrapper variant: 'GUI'.
 Extracting apphost...
 Extracted apphost to 'f:\Projects\Softdev\DotnetRuntimeBootstrapper\DotnetRuntimeBootstrapper.Demo.Gui\bin\Debug\net6.0-windows\DotnetRuntimeBootstrapper.Demo.Gui.exe'.
 Extracted apphost config to 'f:\Projects\Softdev\DotnetRuntimeBootstrapper\DotnetRuntimeBootstrapper.Demo.Gui\bin\Debug\net6.0-windows\DotnetRuntimeBootstrapper.Demo.Gui.exe.config'.
 Injecting target binding...
 Injected target binding to 'DotnetRuntimeBootstrapper.Demo.Gui.exe'.
 Injecting manifest...
 Injected manifest to 'DotnetRuntimeBootstrapper.Demo.Gui.exe'.
 Injecting icon...
 Injected icon to 'DotnetRuntimeBootstrapper.Demo.Gui.exe'.
 Injecting version info...
 Injected version info to 'DotnetRuntimeBootstrapper.Demo.Gui.exe'.
 Bootstrapper created successfully.
```

#### Application host logs

In the event of a fatal error, bootstrapper will produce an error dump (in addition to showing a message to the user).
It can be found in the Windows event log under **Windows Logs** → **Application** with event ID `1023` and source `.NET Runtime`.
The dump has the following format:

```txt
Description: Bootstrapper for a .NET application has failed.
Application: DotnetRuntimeBootstrapper.Demo.Gui.exe
Path: F:\Projects\Softdev\DotnetRuntimeBootstrapper\DotnetRuntimeBootstrapper.Demo.Gui\bin\Debug\net6.0-windows\DotnetRuntimeBootstrapper.Demo.Gui.exe
AppHost: .NET Runtime Bootstrapper v2.3.0
Message: System.Exception: Test failure
   at DotnetRuntimeBootstrapper.AppHost.Core.ApplicationShellBase.Run(String[] args)
   at DotnetRuntimeBootstrapper.AppHost.Gui.Program.Main(String[] args)
```
