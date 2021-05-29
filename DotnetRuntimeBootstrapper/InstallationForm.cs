﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.RuntimeComponents;
using DotnetRuntimeBootstrapper.Utils;
using OperatingSystem = DotnetRuntimeBootstrapper.Env.OperatingSystem;

// ReSharper disable LocalizableElement

namespace DotnetRuntimeBootstrapper
{
    public partial class InstallationForm : Form
    {
        private readonly IRuntimeComponent[] _missingRuntimeComponents;

        public DialogResult Result { get; private set; } = DialogResult.None;

        public InstallationForm(IRuntimeComponent[] missingRuntimeComponents)
        {
            _missingRuntimeComponents = missingRuntimeComponents;

            InitializeComponent();
        }

        private void Exit(DialogResult result)
        {
            Result = result;
            Application.Exit();
        }

        private void InvokeOnUI(Action updateProgress) => Invoke(updateProgress);

        private void UpdateProgress(double progress) => InvokeOnUI(() =>
        {
            ProgressBar.Value = (int) (progress * 100);
            ProgressBar.Visible = true;
        });

        private void ReportError(Exception exception) => InvokeOnUI(() =>
        {
            MessageBox.Show(
                "An error occurred:" + Environment.NewLine + exception,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            Exit(DialogResult.No);
        });

        private void PromptReboot() => InvokeOnUI(() =>
        {
            var result = MessageBox.Show(
                $"You need to restart Windows before you can run {Inputs.TargetApplicationName}. " +
                "Would you like to do it now?",
                "Restart required",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                OperatingSystem.InitiateReboot();
            }

            Exit(DialogResult.No);
        });

        private void InstallationForm_Load(object sender, EventArgs args)
        {
            Text = $"Missing Dependencies: {Inputs.TargetApplicationName}";
            PictureBox.Image = SystemIcons.Warning.ToBitmap();

            DescriptionLabel.Text =
                $"Your system is missing runtime components required by {Inputs.TargetApplicationName}. " +
                "Would you like to download and install them?" +
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

            PictureBox.Image = SystemIcons.Information.ToBitmap();
            DescriptionLabel.Text = "Downloading files...";

            new Thread(() =>
            {
                try
                {
                    // Download
                    var componentInstallers = new List<DownloadedRuntimeComponentInstaller>();
                    var componentsDownloaded = 0;
                    foreach (var component in _missingRuntimeComponents)
                    {
                        var currentComponentsDownloaded = componentsDownloaded;

                        InvokeOnUI(() =>
                        {
                            DescriptionLabel.Text =
                                $"Downloading {component.DisplayName}... " +
                                $"({currentComponentsDownloaded + 1} of {2 * _missingRuntimeComponents.Length})";
                        });

                        var progressOffset = 1.0 * componentsDownloaded / _missingRuntimeComponents.Length;

                        var installer = component.DownloadInstaller(p =>
                            UpdateProgress(progressOffset + p * 1.0 / _missingRuntimeComponents.Length)
                        );

                        componentInstallers.Add(installer);
                        componentsDownloaded++;
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
                                $"Installing {componentInstaller.Component.DisplayName}... " +
                                $"({2 * currentComponentsInstalledCount + 1} of {2 * _missingRuntimeComponents.Length})";
                        });

                        componentInstaller.Run();
                        componentsInstalledCount++;

                        FileEx.TryDelete(componentInstaller.FilePath);
                    }

                    // Finalize
                    if (_missingRuntimeComponents.Any(c => c.IsRebootRequired))
                    {
                        PromptReboot();
                    }
                    else
                    {
                        Exit(DialogResult.OK);
                    }
                }
                catch (Exception ex)
                {
                    ReportError(ex);
                }
            }).Start();
        }

        private void ExitButton_Click(object sender, EventArgs e) => Exit(DialogResult.Cancel);
    }
}