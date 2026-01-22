namespace HyperView.Forms
{
    partial class AboutForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            tableLayoutPanel = new TableLayoutPanel();
            logoPictureBox = new PictureBox();
            labelProductName = new Label();
            labelVersion = new Label();
            labelCopyright = new Label();
            labelCompanyName = new Label();
            textBoxDescription = new TextBox();
            okButton = new Button();
            linkLabelBlog = new LinkLabel();
            linkLabelGitHub = new LinkLabel();
            pictureBoxBuyMeACoffee = new PictureBox();
            linkLabelLinkedIn = new LinkLabel();
            labelSignedBuildState = new Label();
            tableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)logoPictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxBuyMeACoffee).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 67F));
            tableLayoutPanel.Controls.Add(logoPictureBox, 0, 0);
            tableLayoutPanel.Controls.Add(labelProductName, 1, 0);
            tableLayoutPanel.Controls.Add(labelVersion, 1, 1);
            tableLayoutPanel.Controls.Add(labelCopyright, 1, 2);
            tableLayoutPanel.Controls.Add(labelCompanyName, 1, 3);
            tableLayoutPanel.Controls.Add(textBoxDescription, 1, 4);
            tableLayoutPanel.Controls.Add(okButton, 1, 5);
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(10, 10);
            tableLayoutPanel.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 6;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));
            tableLayoutPanel.Size = new Size(487, 307);
            tableLayoutPanel.TabIndex = 0;
            // 
            // logoPictureBox
            // 
            logoPictureBox.Dock = DockStyle.Fill;
            logoPictureBox.Image = Properties.Resources.hyper_v;
            logoPictureBox.Location = new Point(4, 3);
            logoPictureBox.Margin = new Padding(4, 3, 4, 3);
            logoPictureBox.Name = "logoPictureBox";
            tableLayoutPanel.SetRowSpan(logoPictureBox, 6);
            logoPictureBox.Size = new Size(152, 301);
            logoPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            logoPictureBox.TabIndex = 12;
            logoPictureBox.TabStop = false;
            // 
            // labelProductName
            // 
            labelProductName.Dock = DockStyle.Fill;
            labelProductName.Location = new Point(167, 0);
            labelProductName.Margin = new Padding(7, 0, 4, 0);
            labelProductName.MaximumSize = new Size(0, 20);
            labelProductName.Name = "labelProductName";
            labelProductName.Size = new Size(316, 20);
            labelProductName.TabIndex = 19;
            labelProductName.Text = "Product Name";
            labelProductName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelVersion
            // 
            labelVersion.Dock = DockStyle.Fill;
            labelVersion.Location = new Point(167, 30);
            labelVersion.Margin = new Padding(7, 0, 4, 0);
            labelVersion.MaximumSize = new Size(0, 20);
            labelVersion.Name = "labelVersion";
            labelVersion.Size = new Size(316, 20);
            labelVersion.TabIndex = 0;
            labelVersion.Text = "Version";
            labelVersion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelCopyright
            // 
            labelCopyright.Dock = DockStyle.Fill;
            labelCopyright.Location = new Point(167, 60);
            labelCopyright.Margin = new Padding(7, 0, 4, 0);
            labelCopyright.MaximumSize = new Size(0, 20);
            labelCopyright.Name = "labelCopyright";
            labelCopyright.Size = new Size(316, 20);
            labelCopyright.TabIndex = 21;
            labelCopyright.Text = "Copyright";
            labelCopyright.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelCompanyName
            // 
            labelCompanyName.Dock = DockStyle.Fill;
            labelCompanyName.Location = new Point(167, 90);
            labelCompanyName.Margin = new Padding(7, 0, 4, 0);
            labelCompanyName.MaximumSize = new Size(0, 20);
            labelCompanyName.Name = "labelCompanyName";
            labelCompanyName.Size = new Size(316, 20);
            labelCompanyName.TabIndex = 22;
            labelCompanyName.Text = "Company Name";
            labelCompanyName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // textBoxDescription
            // 
            textBoxDescription.Dock = DockStyle.Fill;
            textBoxDescription.Location = new Point(167, 123);
            textBoxDescription.Margin = new Padding(7, 3, 4, 3);
            textBoxDescription.Multiline = true;
            textBoxDescription.Name = "textBoxDescription";
            textBoxDescription.ReadOnly = true;
            textBoxDescription.ScrollBars = ScrollBars.Both;
            textBoxDescription.Size = new Size(316, 147);
            textBoxDescription.TabIndex = 23;
            textBoxDescription.TabStop = false;
            textBoxDescription.Text = resources.GetString("textBoxDescription.Text");
            // 
            // okButton
            // 
            okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            okButton.DialogResult = DialogResult.Cancel;
            okButton.Location = new Point(395, 277);
            okButton.Margin = new Padding(4, 3, 4, 3);
            okButton.Name = "okButton";
            okButton.Size = new Size(88, 27);
            okButton.TabIndex = 24;
            okButton.Text = "&OK";
            // 
            // linkLabelBlog
            // 
            linkLabelBlog.AutoSize = true;
            linkLabelBlog.Location = new Point(178, 292);
            linkLabelBlog.Margin = new Padding(4, 0, 4, 0);
            linkLabelBlog.Name = "linkLabelBlog";
            linkLabelBlog.Size = new Size(51, 15);
            linkLabelBlog.TabIndex = 1;
            linkLabelBlog.TabStop = true;
            linkLabelBlog.Text = "My blog";
            linkLabelBlog.LinkClicked += linkLabelBlog_LinkClicked;
            // 
            // linkLabelGitHub
            // 
            linkLabelGitHub.AutoSize = true;
            linkLabelGitHub.Location = new Point(236, 292);
            linkLabelGitHub.Margin = new Padding(4, 0, 4, 0);
            linkLabelGitHub.Name = "linkLabelGitHub";
            linkLabelGitHub.Size = new Size(45, 15);
            linkLabelGitHub.TabIndex = 2;
            linkLabelGitHub.TabStop = true;
            linkLabelGitHub.Text = "GitHub";
            linkLabelGitHub.LinkClicked += linkLabelGitHub_LinkClicked;
            // 
            // pictureBoxBuyMeACoffee
            // 
            pictureBoxBuyMeACoffee.Image = (Image)resources.GetObject("pictureBoxBuyMeACoffee.Image");
            pictureBoxBuyMeACoffee.InitialImage = null;
            pictureBoxBuyMeACoffee.Location = new Point(27, 269);
            pictureBoxBuyMeACoffee.Margin = new Padding(4, 3, 4, 3);
            pictureBoxBuyMeACoffee.Name = "pictureBoxBuyMeACoffee";
            pictureBoxBuyMeACoffee.Size = new Size(126, 44);
            pictureBoxBuyMeACoffee.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxBuyMeACoffee.TabIndex = 3;
            pictureBoxBuyMeACoffee.TabStop = false;
            pictureBoxBuyMeACoffee.Click += pictureBoxBuyMeACoffee_Click;
            // 
            // linkLabelLinkedIn
            // 
            linkLabelLinkedIn.AutoSize = true;
            linkLabelLinkedIn.Location = new Point(288, 292);
            linkLabelLinkedIn.Margin = new Padding(4, 0, 4, 0);
            linkLabelLinkedIn.Name = "linkLabelLinkedIn";
            linkLabelLinkedIn.Size = new Size(52, 15);
            linkLabelLinkedIn.TabIndex = 4;
            linkLabelLinkedIn.TabStop = true;
            linkLabelLinkedIn.Text = "LinkedIn";
            linkLabelLinkedIn.LinkClicked += linkLabelLinkedIn_LinkClicked;
            // 
            // labelSignedBuildState
            // 
            labelSignedBuildState.AutoSize = true;
            labelSignedBuildState.Location = new Point(271, 43);
            labelSignedBuildState.Margin = new Padding(4, 0, 4, 0);
            labelSignedBuildState.Name = "labelSignedBuildState";
            labelSignedBuildState.Size = new Size(70, 15);
            labelSignedBuildState.TabIndex = 5;
            labelSignedBuildState.Text = "SignedBuild";
            // 
            // AboutForm
            // 
            AcceptButton = okButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(507, 327);
            Controls.Add(labelSignedBuildState);
            Controls.Add(linkLabelLinkedIn);
            Controls.Add(pictureBoxBuyMeACoffee);
            Controls.Add(linkLabelGitHub);
            Controls.Add(linkLabelBlog);
            Controls.Add(tableLayoutPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutForm";
            Padding = new Padding(10);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "AboutForm";
            tableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)logoPictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxBuyMeACoffee).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.PictureBox logoPictureBox;
        private System.Windows.Forms.Label labelProductName;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelCopyright;
        private System.Windows.Forms.Label labelCompanyName;
        private System.Windows.Forms.TextBox textBoxDescription;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.LinkLabel linkLabelBlog;
        private System.Windows.Forms.LinkLabel linkLabelGitHub;
        private System.Windows.Forms.PictureBox pictureBoxBuyMeACoffee;
        private System.Windows.Forms.LinkLabel linkLabelLinkedIn;
        private System.Windows.Forms.Label labelSignedBuildState;
    }
}
