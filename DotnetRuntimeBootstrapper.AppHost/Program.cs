using System;
using System.Linq;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.AppHost.Utils;
using OperatingSystem = DotnetRuntimeBootstrapper.AppHost.Platform.OperatingSystem;

namespace DotnetRuntimeBootstrapper.AppHost
{
    public partial class Program
    {
        private readonly TargetAssembly _targetAssembly;

        public Program(TargetAssembly targetAssembly) =>
            _targetAssembly = targetAssembly;

        private bool InstallMissingPrerequisites()
        {
            var missingPrerequisites = _targetAssembly.GetMissingPrerequisites();
            if (!missingPrerequisites.Any())
                return true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            static DialogResult ShowForm(Form form)
            {
                using (form)
                {
                    Application.Run(form);
                    return form.DialogResult;
                }
            }

            bool PromptInstallation() =>
                ShowForm(new InstallationPromptForm(_targetAssembly, missingPrerequisites)) == DialogResult.Yes;

            void PerformInstallation() =>
                ShowForm(new InstallationForm(_targetAssembly, missingPrerequisites));

            bool PromptReboot() => MessageBox.Show(
                @$"You need to restart Windows before you can run {_targetAssembly.Title}. " +
                @"Would you like to do it now?",
                @"Restart required",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            ) == DialogResult.Yes;

            // Prompt
            if (!PromptInstallation())
                return false;

            // Install
            PerformInstallation();

            // Check if reboot is required
            if (missingPrerequisites.Any(p => p.IsRebootRequired))
            {
                if (PromptReboot())
                    OperatingSystem.Reboot();

                return false;
            }

            // Reset environment variables to update PATH and other variables
            // that may have been changed by the installation process.
            EnvironmentEx.ResetEnvironmentVariables();

            return true;
        }

        private int Run(string[] args)
        {
            try
            {
                // Attempt to run the target first without any checks (hot path)
                return _targetAssembly.Run(args);
            }
            catch
            {
                if (!InstallMissingPrerequisites())
                {
                    // User canceled or reboot is required
                    return 0xB007;
                }

                // Attempt to run the target again
                return _targetAssembly.Run(args);
            }
        }
    }

    public partial class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (_, e) => Error.Report(e.ExceptionObject.ToString());
            Application.ThreadException += (_, e) => Error.Report(e.Exception);

            try
            {
                return new Program(TargetAssembly.Resolve()).Run(args);
            }
            catch (Exception ex)
            {
                Error.Report(ex);
                return 0xDEAD;
            }
        }
    }
}