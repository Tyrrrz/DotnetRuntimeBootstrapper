using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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

public class CreateBootstrapperTask : Task
{
    [Required]
    public string TargetFilePath { get; set; } = default!;

    public string MicrosoftNETCoreApp { get; set; } = default!;

    public string MicrosoftWindowsDesktopApp { get; set; } = default!;

    public string MicrosoftAspNetCoreApp { get; set; } = default!;

    public string MicrosoftWebView2Runtime { get; set; } = default!;

    public string TargetFileName => Path.GetFileName(TargetFilePath);

    public string AppHostFilePath => Path.ChangeExtension(TargetFilePath, "exe");

    public string TargetRuntimeConfigFilePath => Path.ChangeExtension(TargetFilePath, "runtimeconfig.json");

    public string AppHostFileName => Path.GetFileName(AppHostFilePath);

    private void ExtractAppHost()
    {
        var assembly = typeof(CreateBootstrapperTask).Assembly;
        var resourceName = $"{typeof(CreateBootstrapperTask).Namespace}.AppHost.exe";

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
            new ReaderParameters {ReadWrite = true}
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

        Log.LogMessage("Injected target binding to '{0}'.", AppHostFileName);
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
            Log.LogMessage("Injected manifest to '{0}'.", AppHostFileName);
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

            Log.LogMessage("Injected icon to '{0}'.", AppHostFileName);
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
            var bootstrapperVersion = typeof(CreateBootstrapperTask).Assembly.GetName().Version.ToString(3);

            appHostPortableExecutable.SetVersionInfo(new VersionInfoBuilder()
                .SetAll(targetVersionInfo)
                .SetFileFlags(FileFlags.None)
                .SetFileType(FileType.Application)
                .SetFileSubType(FileSubType.Unknown)
                .SetAttribute(VersionAttributeName.InternalName, AppHostFileName)
                .SetAttribute(VersionAttributeName.OriginalFilename, AppHostFileName)
                .SetAttribute("AppHost", $".NET Runtime Bootstrapper v{bootstrapperVersion}")
                .Build()
            );

            Log.LogMessage("Injected version info to '{0}'.", AppHostFileName);
        }
        else
        {
            // This is very unusual, so log a warning instead of info
            Log.LogWarning("Could not read version info from '{0}'.", TargetFileName);
        }
    }

    private void AddAdditionalRuntimes()
    {
        var fileContent = File.ReadAllBytes(TargetRuntimeConfigFilePath);
        using (MemoryStream ms = new MemoryStream())
        {
            using (Utf8JsonWriter utf8JsonWriter1 = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true }))
            {
                using (JsonDocument jsonDocument = JsonDocument.Parse(fileContent))
                {
                    utf8JsonWriter1.WriteStartObject();

                    foreach (var element in jsonDocument.RootElement.EnumerateObject())
                    {
                        element.WriteTo(utf8JsonWriter1);
                    }

                    //additional runtimes
                    utf8JsonWriter1.WriteStartArray("additionalRuntimes");

                    if (!string.IsNullOrWhiteSpace(MicrosoftNETCoreApp))
                    {
                        utf8JsonWriter1.WriteStartObject();
                        utf8JsonWriter1.WriteString("name", "Microsoft.NETCore.App");
                        utf8JsonWriter1.WriteString("version", MicrosoftNETCoreApp);
                        utf8JsonWriter1.WriteEndObject();
                    }
                    if (!string.IsNullOrWhiteSpace(MicrosoftWindowsDesktopApp))
                    {
                        utf8JsonWriter1.WriteStartObject();
                        utf8JsonWriter1.WriteString("name", "Microsoft.WindowsDesktop.App");
                        utf8JsonWriter1.WriteString("version", MicrosoftWindowsDesktopApp);
                        utf8JsonWriter1.WriteEndObject();
                    }
                    if (!string.IsNullOrWhiteSpace(MicrosoftAspNetCoreApp))
                    {
                        utf8JsonWriter1.WriteStartObject();
                        utf8JsonWriter1.WriteString("name", "Microsoft.AspNetCore.App");
                        utf8JsonWriter1.WriteString("version", MicrosoftAspNetCoreApp);
                        utf8JsonWriter1.WriteEndObject();
                    }
                    if (!string.IsNullOrWhiteSpace(MicrosoftWebView2Runtime))
                    {
                        utf8JsonWriter1.WriteStartObject();
                        utf8JsonWriter1.WriteString("name", "Microsoft.WebView2.Runtime");
                        utf8JsonWriter1.WriteString("version", MicrosoftWebView2Runtime);
                        utf8JsonWriter1.WriteEndObject();
                    }

                    utf8JsonWriter1.WriteEndArray();

                    utf8JsonWriter1.WriteEndObject();
                }
            }

            File.WriteAllBytes(TargetRuntimeConfigFilePath, ms.ToArray());
        }
    }

    public override bool Execute()
    {
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

        if (!string.IsNullOrWhiteSpace(MicrosoftNETCoreApp)
            || !string.IsNullOrWhiteSpace(MicrosoftWindowsDesktopApp)
            || !string.IsNullOrWhiteSpace(MicrosoftAspNetCoreApp)
            || !string.IsNullOrWhiteSpace(MicrosoftWebView2Runtime))
        {
            Log.LogMessage("Adding additional runtimes...");
            AddAdditionalRuntimes();
        }

        Log.LogMessage("Bootstrapper created successfully.");
        return true;
    }
}