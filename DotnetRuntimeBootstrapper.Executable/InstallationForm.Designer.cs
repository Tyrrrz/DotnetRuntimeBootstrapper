using System;

namespace DotnetRuntimeBootstrapper.Executable
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
            this.TotalProgressBar = new System.Windows.Forms.ProgressBar();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.CurrentProgressBar = new System.Windows.Forms.ProgressBar();
            this.TotalProgressLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // TotalProgressBar
            //
            this.TotalProgressBar.Location = new System.Drawing.Point(12, 110);
            this.TotalProgressBar.Name = "TotalProgressBar";
            this.TotalProgressBar.Size = new System.Drawing.Size(489, 30);
            this.TotalProgressBar.TabIndex = 0;
            //
            // StatusLabel
            //
            this.StatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.StatusLabel.Location = new System.Drawing.Point(8, 10);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(490, 20);
            this.StatusLabel.TabIndex = 1;
            this.StatusLabel.Text = "Preparing...";
            //
            // CurrentProgressBar
            //
            this.CurrentProgressBar.Location = new System.Drawing.Point(12, 40);
            this.CurrentProgressBar.Name = "CurrentProgressBar";
            this.CurrentProgressBar.Size = new System.Drawing.Size(489, 30);
            this.CurrentProgressBar.TabIndex = 2;
            //
            // TotalProgressLabel
            //
            this.TotalProgressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TotalProgressLabel.Location = new System.Drawing.Point(8, 80);
            this.TotalProgressLabel.Name = "TotalProgressLabel";
            this.TotalProgressLabel.Size = new System.Drawing.Size(490, 20);
            this.TotalProgressLabel.TabIndex = 3;
            this.TotalProgressLabel.Text = "Total progress: ...";
            //
            // InstallationForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(509, 161);
            this.Controls.Add(this.TotalProgressLabel);
            this.Controls.Add(this.CurrentProgressBar);
            this.Controls.Add(this.StatusLabel);
            this.Controls.Add(this.TotalProgressBar);
            this.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(525, 200);
            this.Name = "InstallationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = ".NET Runtime Bootstrapper";
            this.Load += new System.EventHandler(this.InstallationForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar TotalProgressBar;
        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.ProgressBar CurrentProgressBar;
        private System.Windows.Forms.Label TotalProgressLabel;
    }
}