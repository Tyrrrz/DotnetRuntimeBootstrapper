using System;
using System.IO;
using System.Reflection;
using System.Text;
using DotnetRuntimeBootstrapper.Utils.Extensions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Ressy;
using Ressy.HighLevel.Versions;

namespace DotnetRuntimeBootstrapper;

public class BootstrapperTask : Task
{
    private Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;

    [Required]
    public required string Variant { get; init; }

    [Required]
    public required bool IsPromptRequired { get; init; }

    [Required]
    public required string TargetFilePath { get; init; }

    public string TargetFileName => Path.GetFileName(TargetFilePath);

    public string AppHostFilePath => Path.ChangeExtension(TargetFilePath, "exe");

    public string AppHostFileName => Path.GetFileName(AppHostFilePath);

    private void ExtractAppHost()
    {
        Log.LogMessage("Extracting apphost...");

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = GetType().Namespace + Variant.ToUpperInvariant() switch
        {
            "CLI" => ".AppHost.Cli.exe",
            "GUI" => ".AppHost.Gui.exe",
            _ => throw new InvalidOperationException($"Unknown bootstrapper variant '{Variant}'.")
        };

        // Executable file
        assembly.ExtractManifestResource(
            resourceName,
            AppHostFilePath
        );

        Log.LogMessage("Extracted apphost to '{0}'.", AppHostFilePath);

        // Config file
        assembly.ExtractManifestResource(
            resourceName + ".config",
            AppHostFilePath + ".config"
        );

        Log.LogMessage("Extracted apphost config to '{0}'.", AppHostFilePath + ".config");
    }

    private void InjectConfiguration()
    {
        Log.LogMessage("Injecting configuration...");

        var configuration =
            $"""
            TargetFileName={TargetFileName}
            IsPromptRequired={IsPromptRequired}
            """;

        using var assembly = AssemblyDefinition.ReadAssembly(
            AppHostFilePath,
            new ReaderParameters { ReadWrite = true }
        );

        assembly.MainModule.Resources.RemoveAll(r =>
            string.Equals(r.Name, "BootstrapperConfiguration", StringComparison.OrdinalIgnoreCase)
        );

        assembly.MainModule.Resources.Add(new EmbeddedResource(
            "BootstrapperConfiguration",
            ManifestResourceAttributes.Public,
            Encoding.UTF8.GetBytes(configuration)
        ));

        assembly.Write();

        Log.LogMessage("Injected configuration into '{0}'.", AppHostFileName);
    }

    private void InjectResources()
    {
        Log.LogMessage("Injecting resources...");

        var sourcePortableExecutable = new PortableExecutable(TargetFilePath);
        var targetPortableExecutable = new PortableExecutable(AppHostFilePath);

        targetPortableExecutable.ClearResources();

        // Copy resources
        foreach (var identifier in sourcePortableExecutable.GetResourceIdentifiers())
        {
            targetPortableExecutable.SetResource(
                identifier,
                sourcePortableExecutable.GetResource(identifier).Data
            );
        }

        // Modify the version info resource
        targetPortableExecutable.SetVersionInfo(v => v
            .SetFileType(FileType.Application)
            .SetAttribute(VersionAttributeName.InternalName, AppHostFileName)
            .SetAttribute(VersionAttributeName.OriginalFilename, AppHostFileName)
            .SetAttribute("AppHost", $".NET Runtime Bootstrapper v{Version.ToString(3)} ({Variant})")
        );

        Log.LogMessage("Injected resources into '{0}'.", AppHostFileName);
    }

    public override bool Execute()
    {
        Log.LogMessage("Version: '{0}'.", Version);
        Log.LogMessage("Variant: '{0}'.", Variant);
        Log.LogMessage("Prompt required: '{0}'.", IsPromptRequired);
        Log.LogMessage("Target: '{0}'.", TargetFilePath);

        ExtractAppHost();
        InjectConfiguration();
        InjectResources();

        Log.LogMessage("Bootstrapper successfully created.");
        return true;
    }
}