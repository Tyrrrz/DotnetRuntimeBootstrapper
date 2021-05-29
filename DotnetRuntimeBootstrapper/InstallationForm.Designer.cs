namespace DotnetRuntimeBootstrapper
{
    partial class InstallationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.ExitButton = new System.Windows.Forms.Button();
            this.InstallButton = new System.Windows.Forms.Button();
            this.MainPanel = new System.Windows.Forms.Panel();
            this.PictureBox = new System.Windows.Forms.PictureBox();
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.DescriptionLabel = new System.Windows.Forms.Label();
            this.ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.MainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.PictureBox)).BeginInit();
            this.SuspendLayout();
            //
            // ExitButton
            //
            this.ExitButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ExitButton.AutoEllipsis = true;
            this.ExitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ExitButton.Location = new System.Drawing.Point(419, 214);
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.Size = new System.Drawing.Size(80, 38);
            this.ExitButton.TabIndex = 1;
            this.ExitButton.Text = "Cancel";
            this.ToolTip.SetToolTip(this.ExitButton, "Exit the application");
            this.ExitButton.UseVisualStyleBackColor = true;
            this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
            //
            // InstallButton
            //
            this.InstallButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.InstallButton.AutoEllipsis = true;
            this.InstallButton.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.InstallButton.Location = new System.Drawing.Point(334, 214);
            this.InstallButton.Name = "InstallButton";
            this.InstallButton.Size = new System.Drawing.Size(80, 38);
            this.InstallButton.TabIndex = 0;
            this.InstallButton.Text = "Install";
            this.ToolTip.SetToolTip(this.InstallButton, "Download and install the missing components");
            this.InstallButton.UseVisualStyleBackColor = true;
            this.InstallButton.Click += new System.EventHandler(this.InstallButton_Click);
            //
            // MainPanel
            //
            this.MainPanel.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.MainPanel.BackColor = System.Drawing.Color.White;
            this.MainPanel.Controls.Add(this.PictureBox);
            this.MainPanel.Controls.Add(this.ProgressBar);
            this.MainPanel.Controls.Add(this.DescriptionLabel);
            this.MainPanel.Location = new System.Drawing.Point(0, -3);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(510, 210);
            this.MainPanel.TabIndex = 6;
            //
            // PictureBox
            //
            this.PictureBox.Location = new System.Drawing.Point(16, 16);
            this.PictureBox.Name = "PictureBox";
            this.PictureBox.Size = new System.Drawing.Size(32, 32);
            this.PictureBox.TabIndex = 6;
            this.PictureBox.TabStop = false;
            //
            // ProgressBar
            //
            this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.ProgressBar.Location = new System.Drawing.Point(9, 180);
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(490, 22);
            this.ProgressBar.TabIndex = 6;
            this.ProgressBar.Visible = false;
            //
            // DescriptionLabel
            //
            this.DescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.DescriptionLabel.Location = new System.Drawing.Point(59, 12);
            this.DescriptionLabel.Name = "DescriptionLabel";
            this.DescriptionLabel.Size = new System.Drawing.Size(440, 190);
            this.DescriptionLabel.TabIndex = 5;
            this.DescriptionLabel.Text = "Foo bar\r\n  - bar foo\r\n\r\nBaz baz";
            //
            // InstallationForm
            //
            this.AcceptButton = this.InstallButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(509, 261);
            this.Controls.Add(this.MainPanel);
            this.Controls.Add(this.InstallButton);
            this.Controls.Add(this.ExitButton);
            this.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(250, 200);
            this.Name = "InstallationForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = ".NET Runtime Bootstrapper";
            this.Load += new System.EventHandler(this.InstallationForm_Load);
            this.MainPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.PictureBox)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.PictureBox PictureBox;

        private System.Windows.Forms.ToolTip ToolTip;

        private System.Windows.Forms.Panel MainPanel;

        private System.Windows.Forms.Label DescriptionLabel;

        private System.Windows.Forms.Button ExitButton;

        private System.Windows.Forms.ProgressBar ProgressBar;

        private System.Windows.Forms.Button InstallButton;

        #endregion
    }
}