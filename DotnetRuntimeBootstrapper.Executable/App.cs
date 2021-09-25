using System.IO;
using System.Linq;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.Executable.Env;
using DotnetRuntimeBootstrapper.Executable.Prerequisites;
using DotnetRuntimeBootstrapper.Executable.Utils;

namespace DotnetRuntimeBootstrapper.Executable
{
    public class App
    {
        private readonly ExecutionParameters _parameters;
        private readonly IPrerequisite[] _missingPrerequisites;

        public App()
        {
            _parameters = ExecutionParameters.Resolve();

            _missingPrerequisites = new IPrerequisite[]
            {
                // Low-level dependencies first, high-level last
                new WindowsUpdate2999226Prerequisite(),
                new WindowsUpdate3063858Prerequisite(),
                new VisualCppPrerequisite(),
                new DotnetPrerequisite(_parameters.TargetRuntimeName, _parameters.TargetRuntimeVersion)
            }.Where(p => !p.CheckIfInstalled()).ToArray();
        }

        private void ReportError(string message) =>
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        private InstallationPromptResult ShowInstallationPromptForm()
        {
            using var form = new InstallationPromptForm(_parameters, _missingPrerequisites);
            Application.Run(form);
            return form.Result;
        }

        private InstallationResult ShowInstallationForm()
        {
            using var form = new InstallationForm(_parameters, _missingPrerequisites);
            Application.Run(form);
            return form.Result;
        }

        private bool CheckPerformInstall()
        {
            // Nothing to install
            if (_missingPrerequisites.Length <= 0)
                return true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var canProceed = ShowInstallationPromptForm() switch
            {
                InstallationPromptResult.Install => ShowInstallationForm() switch
                {
                    // Ready to run the target application
                    InstallationResult.Ready => true,

                    // Failed or requires reboot
                    _ => false
                },

                // When ignored, return as if the installation already completed successfully
                InstallationPromptResult.Ignore => true,

                // Canceled
                _ => false
            };

            // Reset environment variables after the installation process to update PATH
            EnvironmentEx.ResetEnvironmentVariables();

            return canProceed;
        }

        public int Run(string[] args)
        {
            // Find target assembly
            var targetFilePath = Path.Combine(PathEx.ExecutingDirectoryPath, _parameters.TargetFileName);
            if (!File.Exists(targetFilePath))
            {
                ReportError($"Target assembly not found: '{_parameters.TargetFileName}'.");
                return 1;
            }

            // Ensure prerequisites are installed
            if (!CheckPerformInstall())
            {
                // Installation failed, was canceled, or still requires reboot
                return 1;
            }

            // Run the target
            return Dotnet.Run(targetFilePath, args);
        }
    }
}