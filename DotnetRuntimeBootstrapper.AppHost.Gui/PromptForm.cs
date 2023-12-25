using System;
using System.Linq;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.AppHost.Core;
using DotnetRuntimeBootstrapper.AppHost.Core.Prerequisites;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils;

namespace DotnetRuntimeBootstrapper.AppHost.Gui;

public partial class PromptForm : Form
{
    private readonly TargetAssembly _targetAssembly;
    private readonly IPrerequisite[] _missingPrerequisites;

    public bool IsSuccess { get; private set; }

    public PromptForm(TargetAssembly targetAssembly, IPrerequisite[] missingPrerequisites)
    {
        _targetAssembly = targetAssembly;
        _missingPrerequisites = missingPrerequisites;

        InitializeComponent();
    }

    private void InstallationPromptForm_Load(object sender, EventArgs e)
    {
        Text = @$"{_targetAssembly.Name}: prerequisites missing";
        Icon = IconEx.TryExtractAssociatedIcon(Application.ExecutablePath);
        MissingPrerequisitesTextBox.Lines = _missingPrerequisites
            .Select(c => $"• {c.DisplayName}")
            .ToArray();
    }

    private void InstallButton_Click(object sender, EventArgs e)
    {
        IsSuccess = true;
        Close();
    }

    private void ExitButton_Click(object sender, EventArgs e)
    {
        IsSuccess = false;
        Close();
    }
}
