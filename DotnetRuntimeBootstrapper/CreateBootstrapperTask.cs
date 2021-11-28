using System;
using System.IO;
using System.Text;
using DotnetRuntimeBootstrapper.Utils.Extensions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Ressy;
using Ressy.HighLevel.Icons;
using Ressy.HighLevel.Manifests;
using Ressy.HighLevel.Versions;

namespace DotnetRuntimeBootstrapper
{
    public class CreateBootstrapperTask : Task
    {
        [Required]
        public string TargetTitle { get; set; } = default!;

        [Required]
        public string TargetFilePath { get; set; } = default!;

        public string TargetFileName => Path.GetFileName(TargetFilePath);

        public string BootstrapperFilePath => Path.ChangeExtension(TargetFilePath, "exe");

        public string BootstrapperFileName => Path.GetFileName(BootstrapperFilePath);

        [Required]
        public string TargetRuntimeName { get; set; } = default!;

        [Required]
        public string TargetRuntimeVersion { get; set; } = default!;

        public string? IconFilePath { get; set; }

        public string? ManifestFilePath { get; set; }

        private void ExtractBootstrapper()
        {
            var assembly = typeof(CreateBootstrapperTask).Assembly;
            var resourceName = $"{typeof(CreateBootstrapperTask).Namespace}.Bootstrapper.exe";

            // Executable file
            assembly.ExtractManifestResource(
                resourceName,
                BootstrapperFilePath
            );

            Log.LogMessage("Extracted bootstrapper executable to '{0}'.", BootstrapperFilePath);

            // Config file
            assembly.ExtractManifestResource(
                resourceName + ".config",
                BootstrapperFilePath + ".config"
            );

            Log.LogMessage("Extracted bootstrapper config to '{0}'.", BootstrapperFilePath + ".config");
        }

        private void InjectParameters()
        {
            using var assembly = AssemblyDefinition.ReadAssembly(
                BootstrapperFilePath,
                new ReaderParameters {ReadWrite = true}
            );

            assembly.MainModule.Resources.RemoveAll(
                r => string.Equals(r.Name, "ExecutionParameters", StringComparison.OrdinalIgnoreCase)
            );

            var parameters = new[]
            {
                $"{nameof(TargetTitle)}={TargetTitle}",
                $"{nameof(TargetFileName)}={TargetFileName}",
                $"{nameof(TargetRuntimeName)}={TargetRuntimeName}",
                $"{nameof(TargetRuntimeVersion)}={TargetRuntimeVersion}"
            };

            assembly.MainModule.Resources.Add(new EmbeddedResource(
                "ExecutionParameters",
                ManifestResourceAttributes.Public,
                Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, parameters))
            ));

            assembly.Write();

            Log.LogMessage(
                "Injected execution parameters to '{0}': [{1}]",
                BootstrapperFileName,
                string.Join(", ", parameters)
            );
        }

        private void InjectManifest()
        {
            if (string.IsNullOrWhiteSpace(ManifestFilePath))
            {
                Log.LogMessage("No manifest file specified.");
                return;
            }

            var bootstrapperPortableExecutable = new PortableExecutable(BootstrapperFilePath);
            bootstrapperPortableExecutable.RemoveManifest();
            bootstrapperPortableExecutable.SetManifest(File.ReadAllText(ManifestFilePath));

            Log.LogMessage("Injected manifest from '{0}' to '{1}'.", ManifestFilePath, BootstrapperFileName);
        }

        private void InjectIcon()
        {
            if (string.IsNullOrWhiteSpace(IconFilePath))
            {
                Log.LogMessage("No icon file specified.");
                return;
            }

            var bootstrapperPortableExecutable = new PortableExecutable(BootstrapperFilePath);
            bootstrapperPortableExecutable.RemoveIcon();
            bootstrapperPortableExecutable.SetIcon(IconFilePath);

            Log.LogMessage("Injected icon from '{0}' to '{1}'.", IconFilePath, BootstrapperFileName);
        }

        private void InjectVersionInfo()
        {
            var targetPortableExecutable = new PortableExecutable(TargetFilePath);
            var bootstrapperPortableExecutable = new PortableExecutable(BootstrapperFilePath);

            var versionInfo = targetPortableExecutable.TryGetVersionInfo();
            if (versionInfo is null)
            {
                Log.LogWarning("Could not read version info from '{0}'.", TargetFilePath);
                return;
            }

            var bootstrapperVersion = typeof(CreateBootstrapperTask).Assembly.GetName().Version.ToString(3);

            bootstrapperPortableExecutable.RemoveVersionInfo();
            bootstrapperPortableExecutable.SetVersionInfo(new VersionInfoBuilder()
                .SetAll(versionInfo)
                .SetFileFlags(FileFlags.None)
                .SetFileType(FileType.Application)
                .SetFileSubType(FileSubType.Unknown)
                .SetAttribute(VersionAttributeName.InternalName, BootstrapperFileName)
                .SetAttribute(VersionAttributeName.OriginalFilename, BootstrapperFileName)
                .SetAttribute("Bootstrapper", $".NET Runtime Bootstrapper (v{bootstrapperVersion})")
                .Build()
            );

            Log.LogMessage("Injected version info to '{0}'.", BootstrapperFileName);
        }

        public override bool Execute()
        {
            Log.LogMessage("Extracting bootstrapper...");
            ExtractBootstrapper();

            Log.LogMessage("Injecting execution parameters...");
            InjectParameters();

            Log.LogMessage("Injecting manifest...");
            InjectManifest();

            Log.LogMessage("Injecting icon...");
            InjectIcon();

            Log.LogMessage("Injecting version info...");
            InjectVersionInfo();

            Log.LogMessage("Bootstrapper successfully created.");
            return true;
        }
    }
}