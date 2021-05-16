using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.RuntimeComponents;
using DotnetRuntimeBootstrapper.Utils;
using OperatingSystem = DotnetRuntimeBootstrapper.Utils.OperatingSystem;

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

        private void Invoke(Action updateProgress) => base.Invoke(updateProgress);

        private void UpdateProgress(double progress)
        {
            Invoke(() =>
            {
                ProgressBar.Value = (int) (progress * 100);
                ProgressBar.Visible = true;
            });
        }

        private void ReportError(Exception exception)
        {
            Invoke(() =>
            {
                MessageBox.Show(
                    "An error occurred:" + Environment.NewLine + exception,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                Exit(DialogResult.No);
            });
        }

        private void PromptReboot()
        {
            Invoke(() =>
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
        }

        private Task InstallComponentsAsync()
        {
            var task = Task.Successful;
            var componentsInstalled = 0;

            foreach (var component in _missingRuntimeComponents)
            {
                var installerUrl = component.GetInstallerDownloadUrl();

                var installerFile = TempFile.Create(
                    Path.GetFileNameWithoutExtension(installerUrl),
                    Path.GetExtension(installerUrl)
                );

                task = task
                    .Then(() =>
                    {
                        Invoke(() =>
                        {
                            DescriptionLabel.Text =
                                $"Downloading {component.DisplayName}... [{componentsInstalled + 1} / {_missingRuntimeComponents.Length}]";
                        });

                        return _httpClient.DownloadAsync(installerUrl, installerFile.Path, UpdateProgress);
                    })
                    .Then(() =>
                    {
                        Invoke(() =>
                        {
                            DescriptionLabel.Text =
                                $"Installing {component.DisplayName}... [{componentsInstalled + 1} / {_missingRuntimeComponents.Length}]";
                        });

                        using (installerFile)
                        {
                            component.RunInstaller(installerFile.Path);
                        }

                        UpdateProgress(0);
                    })
                    .Then(() => componentsInstalled++);
            }

            return task;
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
            DescriptionLabel.Text = "Downloading dependencies...";

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