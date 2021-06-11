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

        private string TargetExecutableFilePath => Path.ChangeExtension(TargetFilePath, "exe");

        [Required]
        public string TargetRuntimeName { get; set; } = default!;

        [Required]
        public string TargetRuntimeVersion { get; set; } = default!;

        public string? IconFilePath { get; set; }

        public string? ManifestFilePath { get; set; }

        private void ExtractExecutable()
        {
            var assembly = typeof(CreateBootstrapperTask).Assembly;
            var rootNamespace = typeof(CreateBootstrapperTask).Namespace;

            var resourceName = $"{rootNamespace}.Bootstrapper.exe";

            // Executable file
            assembly.ExtractManifestResource(
                resourceName,
                TargetExecutableFilePath
            );

            Log.LogMessage("Extracted bootstrapper executable to '{0}'.", TargetExecutableFilePath);

            // Config file
            assembly.ExtractManifestResource(
                resourceName + ".config",
                TargetExecutableFilePath + ".config"
            );

            Log.LogMessage("Extracted bootstrapper config to '{0}'.", TargetExecutableFilePath + ".config");
        }

        private void InjectParameters()
        {
            using var assembly = AssemblyDefinition.ReadAssembly(
                TargetExecutableFilePath,
                new ReaderParameters {ReadWrite = true}
            );

            // Delete existing resource if it exists
            assembly.MainModule.Resources.RemoveAll(
                r => string.Equals(r.Name, "ExecutionParameters", StringComparison.OrdinalIgnoreCase)
            );

            // Inject new resource
            var parameters =
                $"{nameof(TargetApplicationName)}={TargetApplicationName}" + Environment.NewLine +
                $"{nameof(TargetFileName)}={TargetFileName}" + Environment.NewLine +
                $"{nameof(TargetRuntimeName)}={TargetRuntimeName}" + Environment.NewLine +
                $"{nameof(TargetRuntimeVersion)}={TargetRuntimeVersion}";

            var resource = new EmbeddedResource(
                "ExecutionParameters",
                ManifestResourceAttributes.Public,
                Encoding.UTF8.GetBytes(parameters)
            );

            assembly.MainModule.Resources.Add(resource);

            assembly.Write();

            Log.LogMessage("Injected execution parameters: {0}", parameters.Replace(Environment.NewLine, "; "));
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
                FileMetadata.SetAuthor(TargetExecutableFilePath, author);
                Log.LogMessage("Injected author string '{0}'.", author);
            }

            if (!string.IsNullOrWhiteSpace(productName))
            {
                FileMetadata.SetProductName(TargetExecutableFilePath, productName);
                Log.LogMessage("Injected product name string '{0}'.", productName);
            }

            if (!string.IsNullOrEmpty(description))
            {
                FileMetadata.SetDescription(TargetExecutableFilePath, description);
                Log.LogMessage("Injected description string '{0}'.", description);
            }

            if (!string.IsNullOrWhiteSpace(fileVersion))
            {
                FileMetadata.SetFileVersion(TargetExecutableFilePath, fileVersion);
                Log.LogMessage("Injected file version string '{0}'.", fileVersion);
            }

            if (!string.IsNullOrEmpty(productVersion))
            {
                FileMetadata.SetProductVersion(TargetExecutableFilePath, productVersion);
                Log.LogMessage("Injected product version string '{0}'.", productVersion);
            }

            if (!string.IsNullOrWhiteSpace(copyright))
            {
                FileMetadata.SetCopyright(TargetExecutableFilePath, copyright);
                Log.LogMessage("Injected copyright string '{0}'.", copyright);
            }

            // Inject icon
            if (!string.IsNullOrWhiteSpace(IconFilePath))
            {
                FileMetadata.SetIcon(TargetExecutableFilePath, IconFilePath);
                Log.LogMessage("Injected icon resource '{0}'.", IconFilePath);
            }

            // Inject manifest
            if (!string.IsNullOrWhiteSpace(ManifestFilePath))
            {
                FileMetadata.SetManifest(TargetExecutableFilePath, ManifestFilePath);
                Log.LogMessage("Injected manifest resource '{0}'.", ManifestFilePath);
            }
        }

        public override bool Execute()
        {
            // Extract bootstrapper
            Log.LogMessage("Extracting executable...");
            ExtractExecutable();

            // Inject parameters in the bootstrapper
            Log.LogMessage("Injecting parameters...");
            InjectParameters();

            // Inject metadata
            Log.LogMessage("Injecting metadata...");
            InjectMetadata();

            Log.LogMessage("Bootstrapper successfully created.");
            return true;
        }
    }
}