using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.AppHost.Core;
using DotnetRuntimeBootstrapper.AppHost.Core.Platform;
using DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;

namespace DotnetRuntimeBootstrapper.AppHost.Gui;

public partial class InstallForm : Form
{
    private readonly TargetAssembly _targetAssembly;
    private readonly IPrerequisite[] _missingPrerequisites;

    // Disable close button
    protected override CreateParams CreateParams
    {
        get
        {
            var result = base.CreateParams;
            result.ClassStyle |= 0x200;
            return result;
        }
    }

    public bool IsSuccess { get; private set; }

    public InstallForm(TargetAssembly targetAssembly, IPrerequisite[] missingPrerequisites)
    {
        _targetAssembly = targetAssembly;
        _missingPrerequisites = missingPrerequisites;

        InitializeComponent();
    }

    private void InvokeOnUI(Action action) => Invoke(action);

    private void UpdateStatus(string status) => InvokeOnUI(() => StatusLabel.Text = status);

    private void UpdateCurrentProgress(double progress) =>
        InvokeOnUI(() =>
        {
            if (progress >= 0)
            {
                CurrentProgressBar.Style = ProgressBarStyle.Continuous;
                CurrentProgressBar.Value = (int)(progress * 100);
            }
            else
            {
                CurrentProgressBar.Style = ProgressBarStyle.Marquee;
            }
        });

    private void UpdateTotalProgress(double totalProgress) =>
        InvokeOnUI(() =>
        {
            if (totalProgress >= 0)
            {
                TotalProgressBar.Style = ProgressBarStyle.Continuous;
                TotalProgressBar.Value = (int)(totalProgress * 100);
                TotalProgressLabel.Text = @$"Total progress: {totalProgress:P0}";
            }
            else
            {
                TotalProgressBar.Style = ProgressBarStyle.Marquee;
            }
        });

    private void Execute()
    {
        var totalSteps = _missingPrerequisites.Length * 2;

        // Download
        var installers = new List<IPrerequisiteInstaller>();
        foreach (var (i, prerequisite) in _missingPrerequisites.Index())
        {
            UpdateStatus(
                @$"[{i + 1}/{totalSteps}] Downloading {prerequisite.DisplayName}..."
            );
            UpdateCurrentProgress(0);

            var installer = prerequisite.DownloadInstaller(p =>
            {
                UpdateCurrentProgress(p);
                UpdateTotalProgress((i + p) / (2.0 * _missingPrerequisites.Length));
            });

            installers.Add(installer);
        }

        // Install
        var isRebootRequired = false;
        try
        {
            foreach (var (i, installer) in installers.Index())
            {
                UpdateStatus(
                    @$"[{_missingPrerequisites.Length + i + 1}/{totalSteps}] Installing {installer.Prerequisite.DisplayName}..."
                );
                UpdateCurrentProgress(-1);

                var installationResult = installer.Run();

                if (installationResult == PrerequisiteInstallerResult.RebootRequired)
                    isRebootRequired = true;

                UpdateTotalProgress(0.5 + (i + 1) / (2.0 * installers.Count));
            }
        }
        finally
        {
            foreach (var installer in installers)
                installer.Dispose();
        }

        // Finalize
        if (isRebootRequired)
        {
            var isRebootAccepted =
                MessageBox.Show(
                    @$"You need to restart Windows before you can run {_targetAssembly.Name}. "
                        + @"Would you like to do it now?",
                    @"Restart required",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                ) == DialogResult.Yes;

            if (isRebootAccepted)
                OperatingSystemEx.Reboot();

            IsSuccess = false;
        }
        else
        {
            IsSuccess = true;
        }

        Close();
    }

    private void InstallationForm_Load(object sender, EventArgs e)
    {
        Text = @$"{_targetAssembly.Name}: installing prerequisites";
        Icon = System.Drawing.Icon.TryExtractAssociatedIcon(Application.ExecutablePath);

        UpdateStatus(@"Preparing installation");

        new Thread(Execute) { Name = nameof(Execute), IsBackground = true }.Start();
    }
}
