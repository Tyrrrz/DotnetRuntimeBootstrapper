using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DotnetRuntimeBootstrapper
{
    public class CreateBootstrapperTask : Task
    {
        [Required]
        public string? TargetApplicationName { get; set; }

        [Required]
        public string? TargetExecutableFilePath { get; set; }

        [Required]
        public string? TargetRuntimeName { get; set; }

        [Required]
        public string? TargetRuntimeVersion { get; set; }

        public override bool Execute()
        {
            // Validate properties
            if (string.IsNullOrWhiteSpace(TargetApplicationName))
            {
                Log.LogError("Missing property '{0}'.", nameof(TargetApplicationName));
                return false;
            }

            if (string.IsNullOrWhiteSpace(TargetExecutableFilePath))
            {
                Log.LogError("Missing property '{0}'.", nameof(TargetExecutableFilePath));
                return false;
            }

            if (string.IsNullOrWhiteSpace(TargetRuntimeName))
            {
                Log.LogError("Missing property '{0}'.", nameof(TargetRuntimeName));
                return false;
            }

            if (string.IsNullOrWhiteSpace(TargetRuntimeVersion))
            {
                Log.LogError("Missing property '{0}'.", nameof(TargetRuntimeVersion));
                return false;
            }

            // Deploy bootstrapper
            Log.LogMessage("Deploying bootstrapper...");

            var bootstrapperFilePath = Path.ChangeExtension(TargetExecutableFilePath ?? "", "exe");
            Bootstrapper.Deploy(bootstrapperFilePath);

            // Inject inputs in the bootstrapper
            Log.LogMessage("Injecting bootstrapper inputs...");

            Bootstrapper.InjectInputs(
                bootstrapperFilePath,
                new BootstrapperInputs(
                    TargetApplicationName,
                    Path.GetFileName(TargetExecutableFilePath),
                    TargetRuntimeName,
                    TargetRuntimeVersion
                )
            );

            // TODO: copy metadata (file version, description, etc)

            // TODO: copy icon

            return true;
        }
    }
}