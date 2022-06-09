using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.AppHost.Core;
using DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;
using OperatingSystem = DotnetRuntimeBootstrapper.AppHost.Core.Platform.OperatingSystem;

namespace DotnetRuntimeBootstrapper.AppHost.Gui;

public partial class InstallationForm : Form
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

    public bool Result { get; private set; }

    public InstallationForm(TargetAssembly targetAssembly, IPrerequisite[] missingPrerequisites)
    {
        _targetAssembly = targetAssembly;
        _missingPrerequisites = missingPrerequisites;

        InitializeComponent();
    }

    private void InvokeOnUI(Action action) => Invoke(action);

    private void UpdateStatus(string status) => InvokeOnUI(() =>
        StatusLabel.Text = status
    );

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

    private void PerformInstall()
    {
        var currentStep = 1;
        var totalSteps = _missingPrerequisites.Length * 2;

        // Download
        var installers = new List<IPrerequisiteInstaller>();
        foreach (var prerequisite in _missingPrerequisites)
        {
            UpdateStatus(@$"[{currentStep}/{totalSteps}] Downloading {prerequisite.DisplayName}...");
            UpdateCurrentProgress(0);

            var installer = prerequisite.DownloadInstaller(p =>
            {
                UpdateCurrentProgress(p);
                UpdateTotalProgress((installers.Count + p) / (2.0 * _missingPrerequisites.Length));
            });

            installers.Add(installer);

            currentStep++;
        }

        // Install
        var isRebootRequired = false;
        var installersFinishedCount = 0;
        foreach (var installer in installers)
        {
            UpdateStatus(@$"[{currentStep}/{totalSteps}] Installing {installer.Prerequisite.DisplayName}...");
            UpdateCurrentProgress(-1);

            var installationResult = installer.Run();

            FileEx.TryDelete(installer.FilePath);

            if (installationResult == PrerequisiteInstallerResult.RebootRequired)
                isRebootRequired = true;

            UpdateTotalProgress(0.5 + ++installersFinishedCount / (2.0 * installers.Count));
            currentStep++;
        }

        // Finalize
        if (isRebootRequired)
        {
            var isRebootAccepted = MessageBox.Show(
                @$"You need to restart Windows before you can run {_targetAssembly.Title}. " +
                @"Would you like to do it now?",
                @"Restart required",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            ) == DialogResult.Yes;

            if (isRebootAccepted)
                OperatingSystem.Reboot();

            Result = false;
        }
        else
        {
            Result = true;
        }

        Close();
    }

    private void InstallationForm_Load(object sender, EventArgs e)
    {
        Text = @$"{_targetAssembly.Title}: installing prerequisites";
        Icon = IconEx.TryExtractAssociatedIcon(Application.ExecutablePath);

        UpdateStatus(@"Preparing installation");

        new Thread(PerformInstall)
        {
            Name = nameof(PerformInstall),
            IsBackground = true
        }.Start();
    }
}