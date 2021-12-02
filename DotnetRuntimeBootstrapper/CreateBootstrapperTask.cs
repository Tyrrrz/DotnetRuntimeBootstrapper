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

namespace DotnetRuntimeBootstrapper
{
    public class CreateBootstrapperTask : Task
    {
        [Required]
        public string TargetFilePath { get; set; } = default!;

        public string TargetFileName => Path.GetFileName(TargetFilePath);

        public string BootstrapperFilePath => Path.ChangeExtension(TargetFilePath, "exe");

        public string BootstrapperFileName => Path.GetFileName(BootstrapperFilePath);

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

        private void InjectTargetBinding()
        {
            using var assembly = AssemblyDefinition.ReadAssembly(
                BootstrapperFilePath,
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

            Log.LogMessage("Injected target assembly binding to '{0}'.", BootstrapperFileName);
        }

        private void InjectManifest()
        {
            var targetPortableExecutable = new PortableExecutable(TargetFilePath);
            var targetManifest = targetPortableExecutable.TryGetManifest();

            var bootstrapperPortableExecutable = new PortableExecutable(BootstrapperFilePath);
            bootstrapperPortableExecutable.RemoveManifest();

            if (!string.IsNullOrWhiteSpace(targetManifest))
            {
                bootstrapperPortableExecutable.SetManifest(targetManifest);
                Log.LogMessage("Injected manifest to '{0}'.", BootstrapperFileName);
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

            var bootstrapperPortableExecutable = new PortableExecutable(BootstrapperFilePath);
            bootstrapperPortableExecutable.RemoveIcon();

            if (targetIconResourceIdentifiers.Any())
            {
                foreach (var identifier in targetIconResourceIdentifiers)
                {
                    bootstrapperPortableExecutable.SetResource(
                        identifier,
                        targetPortableExecutable.GetResource(identifier).Data
                    );
                }

                Log.LogMessage("Injected icon to '{0}'.", BootstrapperFileName);
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

            var bootstrapperPortableExecutable = new PortableExecutable(BootstrapperFilePath);
            bootstrapperPortableExecutable.RemoveVersionInfo();

            if (targetVersionInfo is not null)
            {
                var bootstrapperVersion = typeof(CreateBootstrapperTask).Assembly.GetName().Version.ToString(3);

                bootstrapperPortableExecutable.SetVersionInfo(new VersionInfoBuilder()
                    .SetAll(targetVersionInfo)
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
            else
            {
                // This is very unusual, so log a warning instead of info
                Log.LogWarning("Could not read version info from '{0}'.", TargetFileName);
            }
        }

        public override bool Execute()
        {
            Log.LogMessage("Extracting bootstrapper...");
            ExtractBootstrapper();

            Log.LogMessage("Injecting target binding...");
            InjectTargetBinding();

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