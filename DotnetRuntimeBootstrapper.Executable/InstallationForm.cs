using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.Executable.Platform;
using DotnetRuntimeBootstrapper.Executable.Prerequisites;
using DotnetRuntimeBootstrapper.Executable.Utils;

namespace DotnetRuntimeBootstrapper.Executable
{
    public partial class InstallationForm : Form
    {
        private readonly TargetAssembly _targetAssembly;
        private readonly IPrerequisite[] _missingPrerequisites;

        public InstallationResult Result { get; private set; } = InstallationResult.Failed;

        public InstallationForm(TargetAssembly targetAssembly, IPrerequisite[] missingPrerequisites)
        {
            _targetAssembly = targetAssembly;
            _missingPrerequisites = missingPrerequisites;

            InitializeComponent();
        }

        private void Close(InstallationResult result)
        {
            Result = result;
            Close();
        }

        private void InvokeOnUI(Action action) => Invoke(action);

        private void UpdateStatus(string status) => InvokeOnUI(() => StatusLabel.Text = status);

        private void UpdateCurrentProgress(double progress) => InvokeOnUI(() =>
        {
            if (progress >= 0)
            {
                CurrentProgressBar.Style = ProgressBarStyle.Continuous;
                CurrentProgressBar.Value = (int) (progress * 100);
            }
            else
            {
                CurrentProgressBar.Style = ProgressBarStyle.Marquee;
            }
        });

        private void UpdateTotalProgress(double totalProgress) => InvokeOnUI(() =>
        {
            if (totalProgress >= 0)
            {
                TotalProgressBar.Style = ProgressBarStyle.Continuous;
                TotalProgressBar.Value = (int) (totalProgress * 100);
                TotalProgressLabel.Text = @$"Total progress: {totalProgress:P0}";
            }
            else
            {
                TotalProgressBar.Style = ProgressBarStyle.Marquee;
            }
        });

        private void ReportError(Exception exception) => InvokeOnUI(() =>
            MessageBox.Show(
                @"An error occurred:" + Environment.NewLine + exception,
                @"Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )
        );

        private void PromptReboot() => InvokeOnUI(() =>
        {
            var result = MessageBox.Show(
                @$"You need to restart Windows before you can run {_targetAssembly.Title}. " +
                @"Would you like to do it now?",
                @"Restart required",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
                PlatformInfo.InitiateReboot();
        });

        private void PerformInstall()
        {
            try
            {
                // Download
                var installers = new List<IPrerequisiteInstaller>();
                foreach (var prerequisite in _missingPrerequisites)
                {
                    UpdateStatus(@$"Downloading {prerequisite.DisplayName}");
                    UpdateCurrentProgress(0);

                    var installer = prerequisite.DownloadInstaller(p =>
                    {
                        UpdateCurrentProgress(p);
                        UpdateTotalProgress((installers.Count + p) / (2.0 * _missingPrerequisites.Length));
                    });

                    installers.Add(installer);
                }

                // Install
                var installersFinishedCount = 0;
                foreach (var installer in installers)
                {
                    UpdateStatus(@$"Installing {installer.Prerequisite.DisplayName}");
                    UpdateCurrentProgress(-1);

                    installer.Run();
                    FileEx.TryDelete(installer.FilePath);

                    UpdateTotalProgress(0.5 + ++installersFinishedCount / (2.0 * installers.Count));
                }

                // Finalize
                if (_missingPrerequisites.Any(c => c.IsRebootRequired))
                {
                    PromptReboot();
                    Close(InstallationResult.RebootRequired);
                }
                else
                {
                    Close(InstallationResult.Succeeded);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex);
                Close(InstallationResult.Failed);
            }
        }

        private void InstallationForm_Load(object sender, EventArgs e)
        {
            Text = @$"{_targetAssembly.Title} (installing prerequisites)";
            Icon = Icon.ExtractAssociatedIcon(typeof(InstallationForm).Assembly.Location);
            UpdateStatus(@"Downloading files...");

            new Thread(PerformInstall)
            {
                Name = nameof(PerformInstall),
                IsBackground = true
            }.Start();
        }
    }
}