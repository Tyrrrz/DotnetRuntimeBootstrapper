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
        public string TargetExecutableFilePath { get; set; } = default!;

        public string TargetExecutableFileName => Path.GetFileName(TargetExecutableFilePath);

        [Required]
        public string TargetRuntimeName { get; set; } = default!;

        [Required]
        public string TargetRuntimeVersion { get; set; } = default!;

        public string? ApplicationIconFilePath { get; set; }

        private string BootstrapperExecutableFilePath => Path.ChangeExtension(TargetExecutableFilePath, "exe");

        private void DeployExecutable()
        {
            var assembly = typeof(CreateBootstrapperTask).Assembly;
            var rootNamespace = typeof(CreateBootstrapperTask).Namespace;

            // Executable file
            var bootstrapperExecutableResourceName = $"{rootNamespace}.Bootstrapper.exe";

            assembly.ExtractManifestResource(
                bootstrapperExecutableResourceName,
                BootstrapperExecutableFilePath
            );

            // Manifest file
            var bootstrapperManifestResourceName = $"{rootNamespace}.Bootstrapper.exe.config";

            assembly.ExtractManifestResource(
                bootstrapperManifestResourceName,
                BootstrapperExecutableFilePath + ".config"
            );
        }

        private void InjectConfig()
        {
            const string resourceName = "DotnetRuntimeBootstrapper.Executable.Config.cfg";

            using var assembly = AssemblyDefinition.ReadAssembly(
                BootstrapperExecutableFilePath,
                new ReaderParameters {ReadWrite = true}
            );

            // Delete existing resource if it exists
            assembly.MainModule.Resources.RemoveAll(
                r => string.Equals(r.Name, resourceName, StringComparison.OrdinalIgnoreCase)
            );

            // Inject new resource
            var configData = Encoding.UTF8.GetBytes(
                $"{nameof(TargetApplicationName)}={TargetApplicationName}" + Environment.NewLine +
                $"{nameof(TargetExecutableFilePath)}={TargetExecutableFileName}" + Environment.NewLine +
                $"{nameof(TargetRuntimeName)}={TargetRuntimeName}" + Environment.NewLine +
                $"{nameof(TargetRuntimeVersion)}={TargetRuntimeVersion}"
            );

            var resource = new EmbeddedResource(resourceName, ManifestResourceAttributes.Public, configData);
            assembly.MainModule.Resources.Add(resource);

            assembly.Write();
        }

        private void InjectMetadata()
        {
            // Read metadata
            var author = FileMetadata.GetAuthor(TargetExecutableFilePath);
            var productName = FileMetadata.GetProductName(TargetExecutableFilePath);
            var description = FileMetadata.GetDescription(TargetExecutableFilePath);
            var fileVersion = FileMetadata.GetFileVersion(TargetExecutableFilePath);
            var productVersion = FileMetadata.GetProductVersion(TargetExecutableFilePath);
            var copyright = FileMetadata.GetCopyright(TargetExecutableFilePath);

            // Inject metadata
            if (!string.IsNullOrWhiteSpace(author))
                FileMetadata.SetAuthor(BootstrapperExecutableFilePath, author);

            if (!string.IsNullOrWhiteSpace(productName))
                FileMetadata.SetProductName(BootstrapperExecutableFilePath, productName);

            if (!string.IsNullOrEmpty(description))
                FileMetadata.SetDescription(BootstrapperExecutableFilePath, description);

            if (!string.IsNullOrWhiteSpace(fileVersion))
                FileMetadata.SetFileVersion(BootstrapperExecutableFilePath, fileVersion);

            if (!string.IsNullOrEmpty(productVersion))
                FileMetadata.SetProductVersion(BootstrapperExecutableFilePath, productVersion);

            if (!string.IsNullOrWhiteSpace(copyright))
                FileMetadata.SetCopyright(BootstrapperExecutableFilePath, copyright);

            // Inject icon
            if (!string.IsNullOrWhiteSpace(ApplicationIconFilePath))
                FileMetadata.SetIcon(BootstrapperExecutableFilePath, ApplicationIconFilePath);
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