namespace Xunit.Installer
{
    partial class ResetDevEnvForm
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
            this.labelPrimary = new System.Windows.Forms.Label();
            this.labelSecondary = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelPrimary
            // 
            this.labelPrimary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPrimary.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPrimary.ForeColor = System.Drawing.Color.Maroon;
            this.labelPrimary.Location = new System.Drawing.Point(13, 20);
            this.labelPrimary.Name = "labelPrimary";
            this.labelPrimary.Size = new System.Drawing.Size(431, 30);
            this.labelPrimary.TabIndex = 0;
            this.labelPrimary.Text = "Resetting Visual Studio, please wait...";
            this.labelPrimary.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelPrimary.UseWaitCursor = true;
            // 
            // labelSecondary
            // 
            this.labelSecondary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSecondary.Location = new System.Drawing.Point(13, 54);
            this.labelSecondary.Name = "labelSecondary";
            this.labelSecondary.Size = new System.Drawing.Size(431, 23);
            this.labelSecondary.TabIndex = 1;
            this.labelSecondary.Text = "If Visual Studio is currently running, you will need to restart it.";
            this.labelSecondary.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelSecondary.UseWaitCursor = true;
            // 
            // ResetDevEnvForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 98);
            this.ControlBox = false;
            this.Controls.Add(this.labelSecondary);
            this.Controls.Add(this.labelPrimary);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "ResetDevEnvForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.UseWaitCursor = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelPrimary;
        private System.Windows.Forms.Label labelSecondary;
    }
}