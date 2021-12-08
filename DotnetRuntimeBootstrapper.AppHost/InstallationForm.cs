using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.AppHost.Prerequisites;
using DotnetRuntimeBootstrapper.AppHost.Utils;

namespace DotnetRuntimeBootstrapper.AppHost;

public partial class InstallationForm : Form
{
    private readonly TargetAssembly _targetAssembly;
    private readonly IPrerequisite[] _missingPrerequisites;

    public InstallationForm(TargetAssembly targetAssembly, IPrerequisite[] missingPrerequisites)
    {
        _targetAssembly = targetAssembly;
        _missingPrerequisites = missingPrerequisites;

        InitializeComponent();
    }

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

    private void PerformInstall()
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
        Close();
    }

    private void InstallationForm_Load(object sender, EventArgs e)
    {
        Text = @$"{_targetAssembly.Title}: installing prerequisites";
        Icon = Icon.ExtractAssociatedIcon(typeof(InstallationForm).Assembly.Location);

        UpdateStatus(@"Downloading files...");

        new Thread(PerformInstall)
        {
            Name = nameof(PerformInstall),
            IsBackground = true
        }.Start();
    }
}