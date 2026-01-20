namespace HyperView.Forms
{
    partial class ValidationProgressForm
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
            ProgressBar = new ProgressBar();
            SuspendLayout();
            // 
            // ProgressBar
            // 
            ProgressBar.Location = new Point(1, 1);
            ProgressBar.Name = "ProgressBar";
            ProgressBar.Size = new Size(470, 23);
            ProgressBar.Style = ProgressBarStyle.Marquee;
            ProgressBar.TabIndex = 0;
            // 
            // ValidationProgressForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(472, 26);
            ControlBox = false;
            Controls.Add(ProgressBar);
            Name = "ValidationProgressForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Loading data, please wait...";
            ResumeLayout(false);
        }

        #endregion

        private ProgressBar ProgressBar;
    }
}