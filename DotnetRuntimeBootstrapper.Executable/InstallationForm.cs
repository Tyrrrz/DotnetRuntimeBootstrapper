using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.Executable.RuntimeComponents;
using DotnetRuntimeBootstrapper.Executable.Utils;
using OperatingSystem = DotnetRuntimeBootstrapper.Executable.Env.OperatingSystem;

namespace DotnetRuntimeBootstrapper.Executable
{
    public partial class InstallationForm : Form
    {
        private readonly ExecutionParameters _parameters;
        private readonly IRuntimeComponent[] _missingRuntimeComponents;

        public InstallationFormResult Result { get; private set; } = InstallationFormResult.Failed;

        public InstallationForm(ExecutionParameters parameters, IRuntimeComponent[] missingRuntimeComponents)
        {
            _parameters = parameters;
            _missingRuntimeComponents = missingRuntimeComponents;

            InitializeComponent();
        }

        private void Exit(InstallationFormResult result)
        {
            Result = result;
            Application.Exit();
        }

        private void InvokeOnUI(Action action) => Invoke(action);

        private void UpdateProgress(double progress) => InvokeOnUI(() =>
        {
            ProgressBar.Value = (int) (progress * 100);
            ProgressBar.Visible = true;
        });

        private void ReportError(Exception exception) => InvokeOnUI(() =>
        {
            MessageBox.Show(
                @"An error occurred:" + Environment.NewLine + exception,
                @"Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            Exit(InstallationFormResult.Failed);
        });

        private void PromptReboot() => InvokeOnUI(() =>
        {
            var result = MessageBox.Show(
                @$"You need to restart Windows before you can run {_parameters.TargetApplicationName}. " +
                @"Would you like to do it now?",
                @"Restart required",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                OperatingSystem.InitiateReboot();
            }
        });

        private void InstallationForm_Load(object sender, EventArgs args)
        {
            Text = @$"{_parameters.TargetApplicationName} (Dependencies Missing)";
            PictureBox.Image = SystemIcons.Warning.ToBitmap();

            DescriptionLabel.Text =
                @$"Your system is missing runtime components required by {_parameters.TargetApplicationName}. " +
                @"Would you like to download and install them?" +
                Environment.NewLine +
                Environment.NewLine +
                string.Join(Environment.NewLine, _missingRuntimeComponents.Select(c =>
                    $"• {c.DisplayName}"
                ).ToArray());
        }

        private void InstallButton_Click(object sender, EventArgs args)
        {
            InstallButton.Visible = false;
            InstallButton.Enabled = false;
            ExitButton.Visible = false;
            ExitButton.Enabled = false;
            IgnoreButton.Visible = false;
            IgnoreButton.Enabled = false;

            PictureBox.Image = SystemIcons.Information.ToBitmap();
            DescriptionLabel.Text = @"Downloading files...";

            new Thread(() =>
            {
                try
                {
                    // Download
                    var componentInstallers = new List<RuntimeComponentInstaller>();
                    foreach (var component in _missingRuntimeComponents)
                    {
                        InvokeOnUI(() =>
                        {
                            DescriptionLabel.Text =
                                @$"Downloading {component.DisplayName}... " +
                                @$"({componentInstallers.Count + 1} of {_missingRuntimeComponents.Length})";
                        });

                        var progressOffset = 1.0 * componentInstallers.Count / _missingRuntimeComponents.Length;

                        var installer = component.DownloadInstaller(p =>
                            UpdateProgress(progressOffset + p * 1.0 / _missingRuntimeComponents.Length)
                        );

                        componentInstallers.Add(installer);
                    }

                    InvokeOnUI(() => ProgressBar.Style = ProgressBarStyle.Marquee);

                    // Install
                    var componentsInstalledCount = 0;
                    foreach (var componentInstaller in componentInstallers)
                    {
                        var currentComponentsInstalledCount = componentsInstalledCount;

                        InvokeOnUI(() =>
                        {
                            DescriptionLabel.Text =
                                @$"Installing {componentInstaller.Component.DisplayName}... " +
                                @$"({currentComponentsInstalledCount + 1} of {_missingRuntimeComponents.Length})";
                        });

                        componentInstaller.Run();
                        componentsInstalledCount++;

                        FileEx.TryDelete(componentInstaller.FilePath);
                    }

                    // Finalize
                    if (_missingRuntimeComponents.Any(c => c.IsRebootRequired))
                    {
                        PromptReboot();
                        Exit(InstallationFormResult.CompletedAndRequiresReboot);
                    }
                    else
                    {
                        Exit(InstallationFormResult.CompletedAndReady);
                    }
                }
                catch (Exception ex)
                {
                    ReportError(ex);
                }
            }).Start();
        }

        private void ExitButton_Click(object sender, EventArgs e) => Exit(InstallationFormResult.Canceled);

        private void IgnoreButton_Click(object sender, EventArgs e) => Exit(InstallationFormResult.Ignored);
    }
}