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
        private static string Version { get; } = typeof(CreateBootstrapperTask).Assembly.GetName().Version.ToString(3);

        [Required]
        public string TargetTitle { get; set; } = default!;

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
                $"{nameof(TargetTitle)}={TargetTitle}" + Environment.NewLine +
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

        private void InjectManifest()
        {
            if (string.IsNullOrWhiteSpace(ManifestFilePath))
            {
                Log.LogMessage("No manifest file specified.");
                return;
            }

            var outputPortableExecutable = new PortableExecutable(TargetExecutableFilePath);
            outputPortableExecutable.RemoveManifest();
            outputPortableExecutable.SetManifest(File.ReadAllText(ManifestFilePath));
        }

        private void InjectIcon()
        {
            if (string.IsNullOrWhiteSpace(IconFilePath))
            {
                Log.LogMessage("No icon file specified.");
                return;
            }

            var outputPortableExecutable = new PortableExecutable(TargetExecutableFilePath);
            outputPortableExecutable.RemoveIcon();
            outputPortableExecutable.SetIcon(IconFilePath);
        }

        private void InjectVersionInfo()
        {
            var targetPortableExecutable = new PortableExecutable(TargetFilePath);
            var outputPortableExecutable = new PortableExecutable(TargetExecutableFilePath);

            var versionInfo = targetPortableExecutable.TryGetVersionInfo();
            if (versionInfo is null)
            {
                Log.LogWarning("Could not read version info from '{0}'.", TargetFilePath);
                return;
            }

            outputPortableExecutable.RemoveVersionInfo();
            outputPortableExecutable.SetVersionInfo(new VersionInfoBuilder()
                .SetAll(versionInfo)
                .SetFileFlags(FileFlags.None)
                .SetFileType(FileType.Application)
                .SetFileSubType(FileSubType.Unknown)
                .SetAttribute(VersionAttributeName.InternalName, Path.ChangeExtension(TargetFileName, "exe"))
                .SetAttribute(VersionAttributeName.OriginalFilename, Path.ChangeExtension(TargetFileName, "exe"))
                .SetAttribute("Bootstrapper", $".NET Runtime Bootstrapper (v{Version})")
                .Build()
            );
        }

        public override bool Execute()
        {
            Log.LogMessage("Extracting executable...");
            ExtractExecutable();

            Log.LogMessage("Injecting parameters...");
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