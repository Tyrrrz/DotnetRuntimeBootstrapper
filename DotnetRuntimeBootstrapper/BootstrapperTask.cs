using System;
using System.IO;
using System.Linq;
using System.Text;
using DotnetRuntimeBootstrapper.Utils.Extensions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Ressy;
using Ressy.HighLevel.Icons;
using Ressy.HighLevel.Manifests;
using Ressy.HighLevel.Versions;
using ResourceType = Ressy.ResourceType;

namespace DotnetRuntimeBootstrapper;

public class BootstrapperTask : Task
{
    [Required]
    public string Variant { get; set; }

    [Required]
    public string TargetFilePath { get; set; } = default!;

    public string TargetFileName => Path.GetFileName(TargetFilePath);

    public string AppHostFilePath => Path.ChangeExtension(TargetFilePath, "exe");

    public string AppHostFileName => Path.GetFileName(AppHostFilePath);

    private void ExtractAppHost()
    {
        var assembly = typeof(BootstrapperTask).Assembly;
        var resourceName = typeof(BootstrapperTask).Namespace + Variant.ToUpperInvariant() switch
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

    private void InjectTargetBinding()
    {
        using var assembly = AssemblyDefinition.ReadAssembly(
            AppHostFilePath,
            new ReaderParameters { ReadWrite = true }
        );

        assembly.MainModule.Resources.RemoveAll(r =>
            string.Equals(r.Name, "TargetAssembly", StringComparison.OrdinalIgnoreCase)
        );

        assembly.MainModule.Resources.Add(new EmbeddedResource(
            "TargetAssembly",
            ManifestResourceAttributes.Public,
            Encoding.UTF8.GetBytes(TargetFileName)
        ));

        assembly.Write();

        Log.LogMessage("Injected target binding into '{0}'.", AppHostFileName);
    }

    private void InjectManifest()
    {
        var targetPortableExecutable = new PortableExecutable(TargetFilePath);
        var targetManifest = targetPortableExecutable.TryGetManifest();

        var appHostPortableExecutable = new PortableExecutable(AppHostFilePath);
        appHostPortableExecutable.RemoveManifest();

        if (!string.IsNullOrWhiteSpace(targetManifest))
        {
            appHostPortableExecutable.SetManifest(targetManifest);
            Log.LogMessage("Injected manifest into '{0}'.", AppHostFileName);
        }
        else
        {
            Log.LogMessage("Could not find manifest resource in '{0}'.", TargetFileName);
        }
    }

    private void InjectIcon()
    {
        var targetPortableExecutable = new PortableExecutable(TargetFilePath);

        var targetIconResourceIdentifiers = targetPortableExecutable.GetResourceIdentifiers()
            .Where(r => r.Type.Code == ResourceType.Icon.Code || r.Type.Code == ResourceType.IconGroup.Code)
            .ToArray();

        var appHostPortableExecutable = new PortableExecutable(AppHostFilePath);
        appHostPortableExecutable.RemoveIcon();

        if (targetIconResourceIdentifiers.Any())
        {
            foreach (var identifier in targetIconResourceIdentifiers)
            {
                appHostPortableExecutable.SetResource(
                    identifier,
                    targetPortableExecutable.GetResource(identifier).Data
                );
            }

            Log.LogMessage("Injected icon into '{0}'.", AppHostFileName);
        }
        else
        {
            Log.LogMessage("Could not find icon resources in '{0}'.", TargetFileName);
        }
    }

    private void InjectVersionInfo()
    {
        var targetPortableExecutable = new PortableExecutable(TargetFilePath);
        var targetVersionInfo = targetPortableExecutable.TryGetVersionInfo();

        var appHostPortableExecutable = new PortableExecutable(AppHostFilePath);
        appHostPortableExecutable.RemoveVersionInfo();

        if (targetVersionInfo is not null)
        {
            var bootstrapperVersion = typeof(BootstrapperTask).Assembly.GetName().Version.ToString(3);

            appHostPortableExecutable.SetVersionInfo(new VersionInfoBuilder()
                .SetAll(targetVersionInfo)
                .SetFileFlags(FileFlags.None)
                .SetFileType(FileType.Application)
                .SetFileSubType(FileSubType.Unknown)
                .SetAttribute(VersionAttributeName.InternalName, AppHostFileName)
                .SetAttribute(VersionAttributeName.OriginalFilename, AppHostFileName)
                .SetAttribute("AppHost", $".NET Runtime Bootstrapper v{bootstrapperVersion} ({Variant})")
                .Build()
            );

            Log.LogMessage("Injected version info into '{0}'.", AppHostFileName);
        }
        else
        {
            // This is very unusual, so log a warning instead of info
            Log.LogWarning("Could not read version info from '{0}'.", TargetFileName);
        }
    }

    public override bool Execute()
    {
        Log.LogMessage("Bootstrapper target: '{0}'.", TargetFilePath);
        Log.LogMessage("Bootstrapper variant: '{0}'.", Variant);

        Log.LogMessage("Extracting apphost...");
        ExtractAppHost();

        Log.LogMessage("Injecting target binding...");
        InjectTargetBinding();

        Log.LogMessage("Injecting manifest...");
        InjectManifest();

        Log.LogMessage("Injecting icon...");
        InjectIcon();

        Log.LogMessage("Injecting version info...");
        InjectVersionInfo();

        Log.LogMessage("Bootstrapper created successfully.");
        return true;
    }
}