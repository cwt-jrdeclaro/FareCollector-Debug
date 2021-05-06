namespace FareCollector
{
    partial class DebugStartupForm
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
            this.StartAppButton = new System.Windows.Forms.Button();
            this.applicationStatusLabel = new System.Windows.Forms.Label();
            this.stopButton = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // StartAppButton
            // 
            this.StartAppButton.Location = new System.Drawing.Point(12, 12);
            this.StartAppButton.Name = "StartAppButton";
            this.StartAppButton.Size = new System.Drawing.Size(92, 37);
            this.StartAppButton.TabIndex = 0;
            this.StartAppButton.Text = "Start";
            this.StartAppButton.UseVisualStyleBackColor = true;
            this.StartAppButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // applicationStatusLabel
            // 
            this.applicationStatusLabel.AutoSize = true;
            this.applicationStatusLabel.Location = new System.Drawing.Point(12, 57);
            this.applicationStatusLabel.Name = "applicationStatusLabel";
            this.applicationStatusLabel.Size = new System.Drawing.Size(0, 13);
            this.applicationStatusLabel.TabIndex = 1;
            // 
            // stopButton
            // 
            this.stopButton.Enabled = false;
            this.stopButton.Location = new System.Drawing.Point(178, 12);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(92, 37);
            this.stopButton.TabIndex = 2;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(116, 75);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 13);
            this.statusLabel.TabIndex = 3;
            // 
            // DebugStartupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 110);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.applicationStatusLabel);
            this.Controls.Add(this.StartAppButton);
            this.Name = "DebugStartupForm";
            this.Text = "DebugStartupForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button StartAppButton;
        private System.Windows.Forms.Label applicationStatusLabel;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Label statusLabel;
    }
}