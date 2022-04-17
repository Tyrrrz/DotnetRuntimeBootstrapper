using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.AppHost.Prerequisites;
using DotnetRuntimeBootstrapper.AppHost.Utils;

namespace DotnetRuntimeBootstrapper.AppHost;

public partial class InstallationPromptForm : Form
{
    private readonly TargetAssembly _targetAssembly;
    private readonly IPrerequisite[] _missingPrerequisites;

    public InstallationPromptForm(TargetAssembly targetAssembly, IPrerequisite[] missingPrerequisites)
    {
        _targetAssembly = targetAssembly;
        _missingPrerequisites = missingPrerequisites;

        InitializeComponent();
    }

    private void InstallationPromptForm_Load(object sender, EventArgs e)
    {
        Text = @$"{_targetAssembly.Title}: prerequisites missing";
        Icon = IconEx.TryExtractAssociatedIcon(Application.ExecutablePath);
        IconPictureBox.Image = SystemIcons.Warning.ToBitmap();
        MissingPrerequisitesTextBox.Lines = _missingPrerequisites.Select(c => $"• {c.DisplayName}").ToArray();
    }

    private void InstallButton_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Yes;
        Close();
    }

    private void ExitButton_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}