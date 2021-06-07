using System;
using System.IO;
using System.Text;
using DotnetRuntimeBootstrapper.Utils;
using DotnetRuntimeBootstrapper.Utils.Extensions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;

namespace DotnetRuntimeBootstrapper
{
    public class CreateBootstrapperTask : Task
    {
        [Required]
        public string TargetApplicationName { get; set; } = default!;

        [Required]
        public string TargetFilePath { get; set; } = default!;

        public string TargetFileName => Path.GetFileName(TargetFilePath);

        [Required]
        public string TargetRuntimeName { get; set; } = default!;

        [Required]
        public string TargetRuntimeVersion { get; set; } = default!;

        public string? IconFilePath { get; set; }

        private string BootstrapperFilePath => Path.ChangeExtension(TargetFilePath, "exe");

        private void DeployExecutable()
        {
            var assembly = typeof(CreateBootstrapperTask).Assembly;
            var rootNamespace = typeof(CreateBootstrapperTask).Namespace;

            var resourceName = $"{rootNamespace}.Bootstrapper.exe";

            // Executable file
            assembly.ExtractManifestResource(
                resourceName,
                BootstrapperFilePath
            );

            Log.LogMessage("Extracted bootstrapper executable to {0}.", BootstrapperFilePath);

            // Manifest file
            assembly.ExtractManifestResource(
                resourceName + ".config",
                BootstrapperFilePath + ".config"
            );

            Log.LogMessage("Extracted bootstrapper manifest to {0}.", BootstrapperFilePath + ".config");
        }

        private void InjectConfig()
        {
            using var assembly = AssemblyDefinition.ReadAssembly(
                BootstrapperFilePath,
                new ReaderParameters {ReadWrite = true}
            );

            const string resourceName = "DotnetRuntimeBootstrapper.Executable.Config.cfg";

            // Delete existing resource if it exists
            assembly.MainModule.Resources.RemoveAll(
                r => string.Equals(r.Name, resourceName, StringComparison.OrdinalIgnoreCase)
            );

            // Inject new resource
            var config =
                $"{nameof(TargetApplicationName)}={TargetApplicationName}" + Environment.NewLine +
                $"{nameof(TargetFileName)}={TargetFileName}" + Environment.NewLine +
                $"{nameof(TargetRuntimeName)}={TargetRuntimeName}" + Environment.NewLine +
                $"{nameof(TargetRuntimeVersion)}={TargetRuntimeVersion}";

            var resource = new EmbeddedResource(
                resourceName,
                ManifestResourceAttributes.Public,
                Encoding.UTF8.GetBytes(config)
            );

            assembly.MainModule.Resources.Add(resource);

            assembly.Write();

            Log.LogMessage("Injected bootstrapper config: {0}", config);
        }

        private void InjectMetadata()
        {
            // Read metadata
            var author = FileMetadata.GetAuthor(TargetFilePath);
            var productName = FileMetadata.GetProductName(TargetFilePath);
            var description = FileMetadata.GetDescription(TargetFilePath);
            var fileVersion = FileMetadata.GetFileVersion(TargetFilePath);
            var productVersion = FileMetadata.GetProductVersion(TargetFilePath);
            var copyright = FileMetadata.GetCopyright(TargetFilePath);

            // Inject metadata
            if (!string.IsNullOrWhiteSpace(author))
            {
                FileMetadata.SetAuthor(BootstrapperFilePath, author);
                Log.LogMessage("Injected author string '{0}'.", author);
            }

            if (!string.IsNullOrWhiteSpace(productName))
            {
                FileMetadata.SetProductName(BootstrapperFilePath, productName);
                Log.LogMessage("Injected product name string '{0}'.", productName);
            }

            if (!string.IsNullOrEmpty(description))
            {
                FileMetadata.SetDescription(BootstrapperFilePath, description);
                Log.LogMessage("Injected description string '{0}'.", description);
            }

            if (!string.IsNullOrWhiteSpace(fileVersion))
            {
                FileMetadata.SetFileVersion(BootstrapperFilePath, fileVersion);
                Log.LogMessage("Injected file version string '{0}'.", fileVersion);
            }

            if (!string.IsNullOrEmpty(productVersion))
            {
                FileMetadata.SetProductVersion(BootstrapperFilePath, productVersion);
                Log.LogMessage("Injected product version string '{0}'.", productVersion);
            }

            if (!string.IsNullOrWhiteSpace(copyright))
            {
                FileMetadata.SetCopyright(BootstrapperFilePath, copyright);
                Log.LogMessage("Injected copyright string '{0}'.", copyright);
            }

            // Inject icon
            if (!string.IsNullOrWhiteSpace(IconFilePath))
            {
                FileMetadata.SetIcon(BootstrapperFilePath, IconFilePath);
                Log.LogMessage("Injected icon resource '{0}'.", IconFilePath);
            }
        }

        public override bool Execute()
        {
            // Deploy bootstrapper
            Log.LogMessage("Deploying bootstrapper...");
            DeployExecutable();

            // Inject config in the bootstrapper
            Log.LogMessage("Injecting bootstrapper config...");
            InjectConfig();

            // Inject metadata
            Log.LogMessage("Injecting metadata...");
            InjectMetadata();

            Log.LogMessage("Bootstrapper successfully created.");
            return true;
        }
    }
}