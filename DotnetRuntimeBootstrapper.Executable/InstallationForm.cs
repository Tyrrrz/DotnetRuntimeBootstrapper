using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.Executable.Prerequisites;
using DotnetRuntimeBootstrapper.Executable.Utils;
using OperatingSystem = DotnetRuntimeBootstrapper.Executable.Env.OperatingSystem;

namespace DotnetRuntimeBootstrapper.Executable
{
    public partial class InstallationForm : Form
    {
        private readonly ExecutionParameters _parameters;
        private readonly IPrerequisite[] _missingPrerequisites;

        public InstallationFormResult Result { get; private set; } = InstallationFormResult.Failed;

        public InstallationForm(ExecutionParameters parameters, IPrerequisite[] missingPrerequisites)
        {
            _parameters = parameters;
            _missingPrerequisites = missingPrerequisites;

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
                @$"You need to restart Windows before you can run {_parameters.TargetTitle}. " +
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

        private void InstallationForm_Load(object sender, EventArgs e)
        {
            Text = @$"{_parameters.TargetTitle} (Prerequisites Missing)";
            Icon = Icon.ExtractAssociatedIcon(typeof(InstallationForm).Assembly.Location);
            PictureBox.Image = SystemIcons.Warning.ToBitmap();

            DescriptionLabel.Text =
                @$"Your system is missing runtime components required by {_parameters.TargetTitle}. " +
                @"Would you like to download and install them?" +
                Environment.NewLine +
                Environment.NewLine +
                string.Join(Environment.NewLine, _missingPrerequisites.Select(c =>
                    $"• {c.DisplayName}"
                ).ToArray());
        }

        private void InstallButton_Click(object sender, EventArgs e)
        {
            InstallButton.Visible = false;
            InstallButton.Enabled = false;
            ExitButton.Visible = false;
            ExitButton.Enabled = false;
            IgnoreButton.Visible = false;
            IgnoreButton.Enabled = false;

            PictureBox.Image = SystemIcons.Information.ToBitmap();
            DescriptionLabel.Text = @"Downloading files...";

            var thread = new Thread(() =>
            {
                try
                {
                    // Download
                    var installers = new List<IPrerequisiteInstaller>();
                    for (var i = 0; i < _missingPrerequisites.Length; i++)
                    {
                        var downloadNumber = i + 1;
                        var prerequisite = _missingPrerequisites[i];

                        InvokeOnUI(() =>
                        {
                            DescriptionLabel.Text =
                                @$"[{downloadNumber} of {_missingPrerequisites.Length}] " +
                                @$"Downloading {prerequisite.DisplayName}";
                        });

                        var progressOffset = 1.0 * installers.Count / _missingPrerequisites.Length;

                        var installer = prerequisite.DownloadInstaller(p =>
                            UpdateProgress(progressOffset + p * 1.0 / _missingPrerequisites.Length)
                        );

                        installers.Add(installer);
                    }

                    InvokeOnUI(() => ProgressBar.Style = ProgressBarStyle.Marquee);

                    // Install
                    for (var i = 0; i < installers.Count; i++)
                    {
                        var installNumber = i + 1;
                        var installer = installers[i];

                        InvokeOnUI(() =>
                        {
                            DescriptionLabel.Text =
                                @$"[{installNumber} of {_missingPrerequisites.Length}] " +
                                @$"Installing {installer.Prerequisite.DisplayName}";
                        });

                        installer.Run();
                        FileEx.TryDelete(installer.FilePath);
                    }

                    // Finalize
                    if (_missingPrerequisites.Any(c => c.IsRebootRequired))
                    {
                        PromptReboot();
                        Exit(InstallationFormResult.PendingReboot);
                    }
                    else
                    {
                        Exit(InstallationFormResult.Completed);
                    }
                }
                catch (Exception ex)
                {
                    ReportError(ex);
                }
            });

            thread.Name = "InstallProcess";
            thread.IsBackground = true;
            thread.Start();
        }

        private void ExitButton_Click(object sender, EventArgs e) => Exit(InstallationFormResult.Canceled);

        private void IgnoreButton_Click(object sender, EventArgs e) => Exit(InstallationFormResult.Ignored);
    }
}