using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.RuntimeComponents;
using DotnetRuntimeBootstrapper.Utils;
using OperatingSystem = DotnetRuntimeBootstrapper.Env.OperatingSystem;

// ReSharper disable LocalizableElement

namespace DotnetRuntimeBootstrapper
{
    public partial class MainForm : Form
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly IRuntimeComponent[] _missingRuntimeComponents;

        public DialogResult Result { get; private set; } = DialogResult.None;

        public MainForm(IRuntimeComponent[] missingRuntimeComponents)
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
                "You need to restart Windows to finish installation. " +
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

        private Task InstallComponentsAsync()
        {
            // Download
            var componentInstallerFiles = new Dictionary<IRuntimeComponent, TempFile>();
            foreach (var component in _missingRuntimeComponents)
            {
                var installerUrl = component.GetInstallerDownloadUrl();

                var installerFile = TempFile.Create(
                    Path.GetFileNameWithoutExtension(installerUrl),
                    Path.GetExtension(installerUrl)
                );

                componentInstallerFiles[component] = installerFile;
            }

            var downloadTask = Task.WhenAll(_missingRuntimeComponents
                .Select(component =>
                {
                    var installerUrl = component.GetInstallerDownloadUrl();
                    var installerFile = componentInstallerFiles[component];

                    return _httpClient.DownloadAsync(installerUrl, installerFile.Path, UpdateProgress);
                })
                .ToArray()
            );

            // Install
            var installTask = downloadTask
                .Then(() =>
                {
                    var componentsInstalledCount = 0;

                    foreach (var component in _missingRuntimeComponents)
                    {
                        var currentComponentsInstalledCount = componentsInstalledCount;

                        InvokeOnUI(() =>
                        {
                            DescriptionLabel.Text =
                                $"Installing {component.DisplayName}... ({currentComponentsInstalledCount + 1} of {_missingRuntimeComponents.Length})";
                        });

                        using (var installerFile = componentInstallerFiles[component])
                        {
                            component.RunInstaller(installerFile.Path);
                        }

                        componentsInstalledCount++;
                    }
                });

            return installTask;
        }

        private void MainForm_Load(object sender, EventArgs args)
        {
            Text = $"Missing Dependencies: {Inputs.TargetApplicationName}";
            PictureBox.Image = SystemIcons.Warning.ToBitmap();

            var descriptionBuffer = new StringBuilder();

            descriptionBuffer
                .AppendLine(
                    $"Your system is missing runtime components required to run {Inputs.TargetApplicationName}. " +
                    "Would you like to download and install them?"
                )
                .AppendLine();

            foreach (var component in _missingRuntimeComponents)
            {
                descriptionBuffer
                    .Append("• ")
                    .AppendLine(component.DisplayName);
            }

            DescriptionLabel.Text = descriptionBuffer.ToString();
        }

        private void InstallButton_Click(object sender, EventArgs args)
        {
            InstallButton.Visible = false;
            ExitButton.Visible = false;
            PictureBox.Image = SystemIcons.Information.ToBitmap();
            DescriptionLabel.Text = "Downloading files...";

            InstallComponentsAsync()
                .Then(() =>
                {
                    if (_missingRuntimeComponents.Any(c => c.IsRebootRequired))
                    {
                        PromptReboot();
                    }
                    else
                    {
                        Exit(DialogResult.OK);
                    }
                })
                .Catch(ReportError);
        }

        private void ExitButton_Click(object sender, EventArgs e) => Exit(DialogResult.Cancel);
    }
}