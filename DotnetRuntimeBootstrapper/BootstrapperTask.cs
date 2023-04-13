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
    public string Variant { get; set; } = default!;

    [Required]
    public bool IsPromptRequired { get; set; } = true;

    [Required]
    public string TargetFilePath { get; set; } = default!;

    public string TargetFileName => Path.GetFileName(TargetFilePath);

    public string AppHostFilePath => Path.ChangeExtension(TargetFilePath, "exe");

    public string AppHostFileName => Path.GetFileName(AppHostFilePath);

    private void ExtractAppHost()
    {
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
            string.Equals(r.Name, "Configuration", StringComparison.OrdinalIgnoreCase)
        );

        assembly.MainModule.Resources.Add(new EmbeddedResource(
            "Configuration",
            ManifestResourceAttributes.Public,
            Encoding.UTF8.GetBytes(configuration)
        ));

        assembly.Write();

        Log.LogMessage("Injected configuration into '{0}'.", AppHostFileName);
    }

    private void InjectResources()
    {
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
        Log.LogMessage("Bootstrapper target: '{0}'.", TargetFilePath);
        Log.LogMessage("Bootstrapper variant: '{0}'.", Variant);

        Log.LogMessage("Extracting apphost...");
        ExtractAppHost();

        Log.LogMessage("Injecting configuration...");
        InjectConfiguration();

        Log.LogMessage("Injecting resources...");
        InjectResources();

        Log.LogMessage("Bootstrapper created successfully.");
        return true;
    }
}