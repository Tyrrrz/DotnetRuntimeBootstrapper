﻿using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DotnetRuntimeBootstrapper.Executable.Prerequisites;

namespace DotnetRuntimeBootstrapper.Executable
{
    public partial class InstallationPromptForm : Form
    {
        private readonly ExecutionParameters _parameters;
        private readonly IPrerequisite[] _missingPrerequisites;

        public InstallationPromptResult Result { get; private set; } = InstallationPromptResult.Cancel;

        public InstallationPromptForm(ExecutionParameters parameters, IPrerequisite[] missingPrerequisites)
        {
            _parameters = parameters;
            _missingPrerequisites = missingPrerequisites;

            InitializeComponent();
        }

        private void Close(InstallationPromptResult result)
        {
            Result = result;
            Close();
        }

        private void InstallationPromptForm_Load(object sender, EventArgs e)
        {
            Text = @$"{_parameters.TargetTitle} (missing prerequisites)";
            Icon = Icon.ExtractAssociatedIcon(typeof(InstallationForm).Assembly.Location);
            IconPictureBox.Image = SystemIcons.Warning.ToBitmap();
            MissingPrerequisitesTextBox.Lines = _missingPrerequisites.Select(c => $"• {c.DisplayName}").ToArray();
        }

        private void InstallButton_Click(object sender, EventArgs e) => Close(InstallationPromptResult.Install);

        private void ExitButton_Click(object sender, EventArgs e) => Close(InstallationPromptResult.Cancel);

        private void IgnoreButton_Click(object sender, EventArgs e) => Close(InstallationPromptResult.Ignore);
    }
}