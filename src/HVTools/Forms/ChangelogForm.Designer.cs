namespace HVTools.Forms
{
    partial class ChangelogForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChangelogForm));
            richTextBoxChangelog = new TextBox();
            SuspendLayout();
            // 
            // richTextBoxChangelog
            // 
            richTextBoxChangelog.Location = new Point(1, 0);
            richTextBoxChangelog.Multiline = true;
            richTextBoxChangelog.Name = "richTextBoxChangelog";
            richTextBoxChangelog.ReadOnly = true;
            richTextBoxChangelog.Size = new Size(661, 485);
            richTextBoxChangelog.TabIndex = 1;
            // 
            // ChangelogForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(663, 486);
            Controls.Add(richTextBoxChangelog);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ChangelogForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Changelog";
            Load += ChangelogForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox richTextBoxChangelog;
    }
}