using System.Linq;
using System.Text;
using DotnetRuntimeBootstrapper.Utils.Extensions;
using Mono.Cecil;

namespace DotnetRuntimeBootstrapper
{
    internal static class Bootstrapper
    {
        public static void Deploy(string filePath)
        {
            var assembly = typeof(Bootstrapper).Assembly;
            var rootNamespace = typeof(Bootstrapper).Namespace;

            var bootstrapperExecutableResourceName = $"{rootNamespace}.Bootstrapper.exe";

            assembly.ExtractManifestResource(
                bootstrapperExecutableResourceName,
                filePath
            );

            var bootstrapperConfigResourceName = $"{rootNamespace}.Bootstrapper.exe.config";

            assembly.ExtractManifestResource(
                bootstrapperConfigResourceName,
                filePath + ".config"
            );
        }

        public static void InjectInputs(string filePath, BootstrapperInputs bootstrapperInputs)
        {
            using var assembly = AssemblyDefinition.ReadAssembly(filePath, new ReaderParameters {ReadWrite = true});

            var existingInputsResource = assembly.MainModule.Resources.FirstOrDefault(r => r.Name == "DotnetRuntimeBootstrapper.Launcher.Inputs.cfg");
            if (existingInputsResource is not null)
                assembly.MainModule.Resources.Remove(existingInputsResource);

            var inputsData = Encoding.UTF8.GetBytes(bootstrapperInputs.Serialize());
            assembly.MainModule.Resources.Add(
                new EmbeddedResource("DotnetRuntimeBootstrapper.Launcher.Inputs.cfg", ManifestResourceAttributes.Public, inputsData)
            );

            assembly.Write();
        }
    }
}