using System.ComponentModel;

namespace DotnetRuntimeBootstrapper.Executable
{
    partial class InstallationPromptForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.IgnoreButton = new System.Windows.Forms.Button();
            this.InstallButton = new System.Windows.Forms.Button();
            this.ExitButton = new System.Windows.Forms.Button();
            this.MainPanel = new System.Windows.Forms.Panel();
            this.MissingComponentsLabel = new System.Windows.Forms.Label();
            this.MissingPrerequisitesTextBox = new System.Windows.Forms.TextBox();
            this.DescriptionLabel = new System.Windows.Forms.Label();
            this.IconPictureBox = new System.Windows.Forms.PictureBox();
            this.MainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // IgnoreButton
            // 
            this.IgnoreButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.IgnoreButton.AutoEllipsis = true;
            this.IgnoreButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.IgnoreButton.Location = new System.Drawing.Point(10, 227);
            this.IgnoreButton.Name = "IgnoreButton";
            this.IgnoreButton.Size = new System.Drawing.Size(90, 42);
            this.IgnoreButton.TabIndex = 10;
            this.IgnoreButton.Text = "Ignore";
            this.ToolTip.SetToolTip(this.IgnoreButton, "Ignore missing components and attempt to run the application anyway.\r\n⚠️ Warning:" +
        " Use this only if you believe that this message was shown in error.");
            this.IgnoreButton.UseVisualStyleBackColor = true;
            this.IgnoreButton.Click += new System.EventHandler(this.IgnoreButton_Click);
            // 
            // InstallButton
            // 
            this.InstallButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.InstallButton.AutoEllipsis = true;
            this.InstallButton.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InstallButton.Location = new System.Drawing.Point(305, 227);
            this.InstallButton.Name = "InstallButton";
            this.InstallButton.Size = new System.Drawing.Size(90, 42);
            this.InstallButton.TabIndex = 8;
            this.InstallButton.Text = "Install";
            this.ToolTip.SetToolTip(this.InstallButton, "Download and install the missing components");
            this.InstallButton.UseVisualStyleBackColor = true;
            this.InstallButton.Click += new System.EventHandler(this.InstallButton_Click);
            // 
            // ExitButton
            // 
            this.ExitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ExitButton.AutoEllipsis = true;
            this.ExitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ExitButton.Location = new System.Drawing.Point(400, 227);
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.Size = new System.Drawing.Size(90, 42);
            this.ExitButton.TabIndex = 9;
            this.ExitButton.Text = "Cancel";
            this.ToolTip.SetToolTip(this.ExitButton, "Exit without running the application");
            this.ExitButton.UseVisualStyleBackColor = true;
            this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // MainPanel
            // 
            this.MainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainPanel.BackColor = System.Drawing.SystemColors.Window;
            this.MainPanel.Controls.Add(this.MissingComponentsLabel);
            this.MainPanel.Controls.Add(this.MissingPrerequisitesTextBox);
            this.MainPanel.Controls.Add(this.DescriptionLabel);
            this.MainPanel.Controls.Add(this.IconPictureBox);
            this.MainPanel.Location = new System.Drawing.Point(-1, -1);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(505, 220);
            this.MainPanel.TabIndex = 11;
            // 
            // MissingComponentsLabel
            // 
            this.MissingComponentsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MissingComponentsLabel.Location = new System.Drawing.Point(8, 65);
            this.MissingComponentsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.MissingComponentsLabel.Name = "MissingComponentsLabel";
            this.MissingComponentsLabel.Size = new System.Drawing.Size(484, 20);
            this.MissingComponentsLabel.TabIndex = 8;
            this.MissingComponentsLabel.Text = "Missing components:";
            // 
            // MissingPrerequisitesTextBox
            // 
            this.MissingPrerequisitesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MissingPrerequisitesTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.MissingPrerequisitesTextBox.Location = new System.Drawing.Point(12, 90);
            this.MissingPrerequisitesTextBox.Multiline = true;
            this.MissingPrerequisitesTextBox.Name = "MissingPrerequisitesTextBox";
            this.MissingPrerequisitesTextBox.ReadOnly = true;
            this.MissingPrerequisitesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.MissingPrerequisitesTextBox.Size = new System.Drawing.Size(480, 115);
            this.MissingPrerequisitesTextBox.TabIndex = 7;
            this.MissingPrerequisitesTextBox.TabStop = false;
            // 
            // DescriptionLabel
            // 
            this.DescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DescriptionLabel.Location = new System.Drawing.Point(52, 13);
            this.DescriptionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.DescriptionLabel.Name = "DescriptionLabel";
            this.DescriptionLabel.Size = new System.Drawing.Size(440, 45);
            this.DescriptionLabel.TabIndex = 6;
            this.DescriptionLabel.Text = "Your system is missing runtime components required by this application. Would you" +
    " like to download and install them now?";
            // 
            // IconPictureBox
            // 
            this.IconPictureBox.Location = new System.Drawing.Point(16, 16);
            this.IconPictureBox.Margin = new System.Windows.Forms.Padding(4);
            this.IconPictureBox.Name = "IconPictureBox";
            this.IconPictureBox.Size = new System.Drawing.Size(32, 32);
            this.IconPictureBox.TabIndex = 5;
            this.IconPictureBox.TabStop = false;
            // 
            // InstallationPromptForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 281);
            this.Controls.Add(this.MainPanel);
            this.Controls.Add(this.IgnoreButton);
            this.Controls.Add(this.InstallButton);
            this.Controls.Add(this.ExitButton);
            this.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(520, 320);
            this.Name = "InstallationPromptForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = ".NET Runtime Bootstrapper";
            this.Load += new System.EventHandler(this.InstallationPromptForm_Load);
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button IgnoreButton;
        private System.Windows.Forms.Button InstallButton;
        private System.Windows.Forms.Button ExitButton;
        private System.Windows.Forms.ToolTip ToolTip;
        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.Label MissingComponentsLabel;
        private System.Windows.Forms.TextBox MissingPrerequisitesTextBox;
        private System.Windows.Forms.Label DescriptionLabel;
        private System.Windows.Forms.PictureBox IconPictureBox;
    }
}