namespace HVTools.Forms
{
    partial class LoginForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            labelLoginFormToolName = new Label();
            pictureBox1 = new PictureBox();
            labelServer = new Label();
            textboxServer = new TextBox();
            groupAuth = new GroupBox();
            buttonCancel = new Button();
            ButtonLogin = new Button();
            checkboxRemember = new CheckBox();
            labelPassword = new Label();
            labelUsername = new Label();
            textboxPassword = new TextBox();
            textboxUsername = new TextBox();
            radioCustom = new RadioButton();
            radioWindows = new RadioButton();
            linkLabelToggleAdvanced = new LinkLabel();
            groupConnectionSettings = new GroupBox();
            buttonReset = new Button();
            numericTimeout = new NumericUpDown();
            labelTimeout = new Label();
            checkboxSkipCNCheck = new CheckBox();
            checkboxSkipCACheck = new CheckBox();
            comboAuthMechanism = new ComboBox();
            labelAuthMechanism = new Label();
            numericPort = new NumericUpDown();
            labelPort = new Label();
            checkboxUseSSL = new CheckBox();
            buttonHelpConnectGuide = new Button();
            statusStripLoginForm = new StatusStrip();
            toolStripStatusLabelLoginForm = new ToolStripStatusLabel();
            toolStripStatusLabelTextLoginForm = new ToolStripStatusLabel();
            pictureboxSupportMe = new PictureBox();
            menuStripLoginForm = new MenuStrip();
            menuToolStripMenuItem = new ToolStripMenuItem();
            onlineToolStripMenuItem = new ToolStripMenuItem();
            myWebpageToolStripMenuItem = new ToolStripMenuItem();
            myBlogToolStripMenuItem = new ToolStripMenuItem();
            guideToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            groupAuth.SuspendLayout();
            groupConnectionSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericTimeout).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericPort).BeginInit();
            statusStripLoginForm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureboxSupportMe).BeginInit();
            menuStripLoginForm.SuspendLayout();
            SuspendLayout();
            // 
            // labelLoginFormToolName
            // 
            labelLoginFormToolName.AutoSize = true;
            labelLoginFormToolName.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelLoginFormToolName.Location = new Point(160, 85);
            labelLoginFormToolName.Name = "labelLoginFormToolName";
            labelLoginFormToolName.Size = new Size(135, 30);
            labelLoginFormToolName.TabIndex = 0;
            labelLoginFormToolName.Text = "%toolname%";
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.hyper_v;
            pictureBox1.Location = new Point(49, 47);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(105, 106);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // labelServer
            // 
            labelServer.AutoSize = true;
            labelServer.Location = new Point(21, 195);
            labelServer.Name = "labelServer";
            labelServer.Size = new Size(83, 15);
            labelServer.TabIndex = 2;
            labelServer.Text = "Server IP/DNS:";
            // 
            // textboxServer
            // 
            textboxServer.Location = new Point(110, 192);
            textboxServer.Name = "textboxServer";
            textboxServer.Size = new Size(280, 23);
            textboxServer.TabIndex = 3;
            textboxServer.TextChanged += textboxServer_TextChanged;
            textboxServer.KeyDown += TextboxServer_KeyDown;
            // 
            // groupAuth
            // 
            groupAuth.Controls.Add(buttonCancel);
            groupAuth.Controls.Add(ButtonLogin);
            groupAuth.Controls.Add(checkboxRemember);
            groupAuth.Controls.Add(labelPassword);
            groupAuth.Controls.Add(labelUsername);
            groupAuth.Controls.Add(textboxPassword);
            groupAuth.Controls.Add(textboxUsername);
            groupAuth.Controls.Add(radioCustom);
            groupAuth.Controls.Add(radioWindows);
            groupAuth.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupAuth.Location = new Point(12, 230);
            groupAuth.Name = "groupAuth";
            groupAuth.Size = new Size(476, 209);
            groupAuth.TabIndex = 4;
            groupAuth.TabStop = false;
            groupAuth.Text = "Authentication Method";
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(132, 176);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(94, 23);
            buttonCancel.TabIndex = 8;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += ButtonCancel_Click;
            // 
            // ButtonLogin
            // 
            ButtonLogin.Location = new Point(251, 176);
            ButtonLogin.Name = "ButtonLogin";
            ButtonLogin.Size = new Size(94, 23);
            ButtonLogin.TabIndex = 7;
            ButtonLogin.Text = "Login";
            ButtonLogin.UseVisualStyleBackColor = true;
            ButtonLogin.Click += ButtonLogin_Click;
            // 
            // checkboxRemember
            // 
            checkboxRemember.AutoSize = true;
            checkboxRemember.Font = new Font("Segoe UI", 9F);
            checkboxRemember.Location = new Point(139, 143);
            checkboxRemember.Name = "checkboxRemember";
            checkboxRemember.Size = new Size(208, 19);
            checkboxRemember.TabIndex = 6;
            checkboxRemember.Text = "Remember credentials (encrypted)";
            checkboxRemember.UseVisualStyleBackColor = true;
            // 
            // labelPassword
            // 
            labelPassword.AutoSize = true;
            labelPassword.Font = new Font("Segoe UI", 9F);
            labelPassword.Location = new Point(37, 111);
            labelPassword.Name = "labelPassword";
            labelPassword.Size = new Size(60, 15);
            labelPassword.TabIndex = 5;
            labelPassword.Text = "Password:";
            // 
            // labelUsername
            // 
            labelUsername.AutoSize = true;
            labelUsername.Font = new Font("Segoe UI", 9F);
            labelUsername.Location = new Point(37, 79);
            labelUsername.Name = "labelUsername";
            labelUsername.Size = new Size(63, 15);
            labelUsername.TabIndex = 4;
            labelUsername.Text = "Username:";
            // 
            // textboxPassword
            // 
            textboxPassword.Font = new Font("Segoe UI", 9F);
            textboxPassword.Location = new Point(106, 108);
            textboxPassword.Name = "textboxPassword";
            textboxPassword.Size = new Size(284, 23);
            textboxPassword.TabIndex = 3;
            textboxPassword.KeyDown += TextboxPassword_KeyDown;
            // 
            // textboxUsername
            // 
            textboxUsername.Font = new Font("Segoe UI", 9F);
            textboxUsername.Location = new Point(106, 79);
            textboxUsername.Name = "textboxUsername";
            textboxUsername.Size = new Size(284, 23);
            textboxUsername.TabIndex = 2;
            textboxUsername.KeyDown += TextboxUsername_KeyDown;
            // 
            // radioCustom
            // 
            radioCustom.AutoSize = true;
            radioCustom.Font = new Font("Segoe UI", 9F);
            radioCustom.Location = new Point(23, 50);
            radioCustom.Name = "radioCustom";
            radioCustom.Size = new Size(147, 19);
            radioCustom.TabIndex = 1;
            radioCustom.TabStop = true;
            radioCustom.Text = "Use specific credentials";
            radioCustom.UseVisualStyleBackColor = true;
            radioCustom.CheckedChanged += RadioAuth_CheckedChanged;
            // 
            // radioWindows
            // 
            radioWindows.AutoSize = true;
            radioWindows.Checked = true;
            radioWindows.Font = new Font("Segoe UI", 9F);
            radioWindows.Location = new Point(23, 25);
            radioWindows.Name = "radioWindows";
            radioWindows.Size = new Size(238, 19);
            radioWindows.TabIndex = 0;
            radioWindows.TabStop = true;
            radioWindows.Text = "Use current Windows session credentials";
            radioWindows.UseVisualStyleBackColor = true;
            radioWindows.CheckedChanged += RadioAuth_CheckedChanged;
            // 
            // linkLabelToggleAdvanced
            // 
            linkLabelToggleAdvanced.AutoSize = true;
            linkLabelToggleAdvanced.Location = new Point(12, 444);
            linkLabelToggleAdvanced.Name = "linkLabelToggleAdvanced";
            linkLabelToggleAdvanced.Size = new Size(150, 15);
            linkLabelToggleAdvanced.TabIndex = 10;
            linkLabelToggleAdvanced.TabStop = true;
            linkLabelToggleAdvanced.Text = "▼ Show Advanced Settings";
            linkLabelToggleAdvanced.LinkClicked += LinkLabelToggleAdvanced_LinkClicked;
            // 
            // groupConnectionSettings
            // 
            groupConnectionSettings.Controls.Add(buttonReset);
            groupConnectionSettings.Controls.Add(numericTimeout);
            groupConnectionSettings.Controls.Add(labelTimeout);
            groupConnectionSettings.Controls.Add(checkboxSkipCNCheck);
            groupConnectionSettings.Controls.Add(checkboxSkipCACheck);
            groupConnectionSettings.Controls.Add(comboAuthMechanism);
            groupConnectionSettings.Controls.Add(labelAuthMechanism);
            groupConnectionSettings.Controls.Add(numericPort);
            groupConnectionSettings.Controls.Add(labelPort);
            groupConnectionSettings.Controls.Add(checkboxUseSSL);
            groupConnectionSettings.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupConnectionSettings.Location = new Point(12, 470);
            groupConnectionSettings.Name = "groupConnectionSettings";
            groupConnectionSettings.Size = new Size(476, 140);
            groupConnectionSettings.TabIndex = 9;
            groupConnectionSettings.TabStop = false;
            groupConnectionSettings.Text = "Advanced Connection Settings";
            // 
            // buttonReset
            // 
            buttonReset.Location = new Point(350, 15);
            buttonReset.Name = "buttonReset";
            buttonReset.Size = new Size(120, 27);
            buttonReset.TabIndex = 9;
            buttonReset.Text = "Reset to Defaults";
            buttonReset.UseVisualStyleBackColor = true;
            buttonReset.Click += buttonReset_Click;
            // 
            // numericTimeout
            // 
            numericTimeout.Font = new Font("Segoe UI", 9F);
            numericTimeout.Location = new Point(366, 84);
            numericTimeout.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            numericTimeout.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numericTimeout.Name = "numericTimeout";
            numericTimeout.Size = new Size(63, 23);
            numericTimeout.TabIndex = 8;
            numericTimeout.Value = new decimal(new int[] { 30, 0, 0, 0 });
            numericTimeout.ValueChanged += NumericTimeout_ValueChanged;
            // 
            // labelTimeout
            // 
            labelTimeout.AutoSize = true;
            labelTimeout.Font = new Font("Segoe UI", 9F);
            labelTimeout.Location = new Point(255, 88);
            labelTimeout.Name = "labelTimeout";
            labelTimeout.Size = new Size(109, 15);
            labelTimeout.TabIndex = 7;
            labelTimeout.Text = "Timeout (seconds):";
            // 
            // checkboxSkipCNCheck
            // 
            checkboxSkipCNCheck.AutoSize = true;
            checkboxSkipCNCheck.Enabled = false;
            checkboxSkipCNCheck.Font = new Font("Segoe UI", 9F);
            checkboxSkipCNCheck.Location = new Point(366, 56);
            checkboxSkipCNCheck.Name = "checkboxSkipCNCheck";
            checkboxSkipCNCheck.Size = new Size(104, 19);
            checkboxSkipCNCheck.TabIndex = 6;
            checkboxSkipCNCheck.Text = "Skip CN Check";
            checkboxSkipCNCheck.UseVisualStyleBackColor = true;
            checkboxSkipCNCheck.CheckedChanged += CheckboxSkipCNCheck_CheckedChanged;
            // 
            // checkboxSkipCACheck
            // 
            checkboxSkipCACheck.AutoSize = true;
            checkboxSkipCACheck.Enabled = false;
            checkboxSkipCACheck.Font = new Font("Segoe UI", 9F);
            checkboxSkipCACheck.Location = new Point(259, 56);
            checkboxSkipCACheck.Name = "checkboxSkipCACheck";
            checkboxSkipCACheck.Size = new Size(103, 19);
            checkboxSkipCACheck.TabIndex = 5;
            checkboxSkipCACheck.Text = "Skip CA Check";
            checkboxSkipCACheck.UseVisualStyleBackColor = true;
            checkboxSkipCACheck.CheckedChanged += CheckboxSkipCACheck_CheckedChanged;
            // 
            // comboAuthMechanism
            // 
            comboAuthMechanism.DropDownStyle = ComboBoxStyle.DropDownList;
            comboAuthMechanism.Font = new Font("Segoe UI", 9F);
            comboAuthMechanism.FormattingEnabled = true;
            comboAuthMechanism.Items.AddRange(new object[] { "Default", "Basic", "Negotiate", "NegotiateWithImplicitCredential", "Credssp", "Digest", "Kerberos" });
            comboAuthMechanism.Location = new Point(23, 105);
            comboAuthMechanism.Name = "comboAuthMechanism";
            comboAuthMechanism.Size = new Size(218, 23);
            comboAuthMechanism.TabIndex = 4;
            comboAuthMechanism.SelectedIndexChanged += ComboAuthMechanism_SelectedIndexChanged;
            // 
            // labelAuthMechanism
            // 
            labelAuthMechanism.AutoSize = true;
            labelAuthMechanism.Font = new Font("Segoe UI", 9F);
            labelAuthMechanism.Location = new Point(23, 87);
            labelAuthMechanism.Name = "labelAuthMechanism";
            labelAuthMechanism.Size = new Size(154, 15);
            labelAuthMechanism.TabIndex = 3;
            labelAuthMechanism.Text = "Authentication Mechanism:";
            // 
            // numericPort
            // 
            numericPort.Font = new Font("Segoe UI", 9F);
            numericPort.Location = new Point(132, 55);
            numericPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numericPort.Name = "numericPort";
            numericPort.Size = new Size(109, 23);
            numericPort.TabIndex = 2;
            numericPort.ValueChanged += NumericPort_ValueChanged;
            // 
            // labelPort
            // 
            labelPort.AutoSize = true;
            labelPort.Font = new Font("Segoe UI", 9F);
            labelPort.Location = new Point(23, 57);
            labelPort.Name = "labelPort";
            labelPort.Size = new Size(81, 15);
            labelPort.TabIndex = 1;
            labelPort.Text = "Port (0=auto):";
            // 
            // checkboxUseSSL
            // 
            checkboxUseSSL.AutoSize = true;
            checkboxUseSSL.Font = new Font("Segoe UI", 9F);
            checkboxUseSSL.Location = new Point(23, 28);
            checkboxUseSSL.Name = "checkboxUseSSL";
            checkboxUseSSL.Size = new Size(167, 19);
            checkboxUseSSL.TabIndex = 0;
            checkboxUseSSL.Text = "Use SSL/HTTPS (Port 5986)";
            checkboxUseSSL.UseVisualStyleBackColor = true;
            checkboxUseSSL.CheckedChanged += CheckboxUseSSL_CheckedChanged;
            // 
            // buttonHelpConnectGuide
            // 
            buttonHelpConnectGuide.Location = new Point(396, 192);
            buttonHelpConnectGuide.Name = "buttonHelpConnectGuide";
            buttonHelpConnectGuide.Size = new Size(23, 23);
            buttonHelpConnectGuide.TabIndex = 5;
            buttonHelpConnectGuide.Text = "?";
            buttonHelpConnectGuide.UseVisualStyleBackColor = true;
            buttonHelpConnectGuide.Click += buttonHelpConnectGuide_Click;
            // 
            // statusStripLoginForm
            // 
            statusStripLoginForm.BackColor = Color.White;
            statusStripLoginForm.Items.AddRange(new ToolStripItem[] { toolStripStatusLabelLoginForm, toolStripStatusLabelTextLoginForm });
            statusStripLoginForm.Location = new Point(0, 458);
            statusStripLoginForm.Name = "statusStripLoginForm";
            statusStripLoginForm.Size = new Size(500, 22);
            statusStripLoginForm.SizingGrip = false;
            statusStripLoginForm.TabIndex = 6;
            statusStripLoginForm.Text = "statusStrip1";
            // 
            // toolStripStatusLabelLoginForm
            // 
            toolStripStatusLabelLoginForm.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            toolStripStatusLabelLoginForm.Name = "toolStripStatusLabelLoginForm";
            toolStripStatusLabelLoginForm.Size = new Size(45, 17);
            toolStripStatusLabelLoginForm.Text = "Status:";
            // 
            // toolStripStatusLabelTextLoginForm
            // 
            toolStripStatusLabelTextLoginForm.Name = "toolStripStatusLabelTextLoginForm";
            toolStripStatusLabelTextLoginForm.Size = new Size(67, 17);
            toolStripStatusLabelTextLoginForm.Text = "%STATUS%";
            // 
            // pictureboxSupportMe
            // 
            pictureboxSupportMe.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pictureboxSupportMe.BackColor = Color.Transparent;
            pictureboxSupportMe.Image = (Image)resources.GetObject("pictureboxSupportMe.Image");
            pictureboxSupportMe.Location = new Point(384, 9);
            pictureboxSupportMe.Name = "pictureboxSupportMe";
            pictureboxSupportMe.Size = new Size(107, 30);
            pictureboxSupportMe.SizeMode = PictureBoxSizeMode.Zoom;
            pictureboxSupportMe.TabIndex = 7;
            pictureboxSupportMe.TabStop = false;
            pictureboxSupportMe.Click += pictureboxSupportMe_Click;
            // 
            // menuStripLoginForm
            // 
            menuStripLoginForm.BackColor = Color.White;
            menuStripLoginForm.Items.AddRange(new ToolStripItem[] { menuToolStripMenuItem });
            menuStripLoginForm.Location = new Point(0, 0);
            menuStripLoginForm.Name = "menuStripLoginForm";
            menuStripLoginForm.Size = new Size(500, 24);
            menuStripLoginForm.TabIndex = 8;
            menuStripLoginForm.Text = "menuStrip1";
            // 
            // menuToolStripMenuItem
            // 
            menuToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { onlineToolStripMenuItem, aboutToolStripMenuItem, toolStripSeparator1, exitToolStripMenuItem });
            menuToolStripMenuItem.Name = "menuToolStripMenuItem";
            menuToolStripMenuItem.Size = new Size(37, 20);
            menuToolStripMenuItem.Text = "File";
            // 
            // onlineToolStripMenuItem
            // 
            onlineToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { myWebpageToolStripMenuItem, myBlogToolStripMenuItem, guideToolStripMenuItem });
            onlineToolStripMenuItem.Name = "onlineToolStripMenuItem";
            onlineToolStripMenuItem.Size = new Size(109, 22);
            onlineToolStripMenuItem.Text = "Online";
            // 
            // myWebpageToolStripMenuItem
            // 
            myWebpageToolStripMenuItem.Name = "myWebpageToolStripMenuItem";
            myWebpageToolStripMenuItem.Size = new Size(142, 22);
            myWebpageToolStripMenuItem.Text = "My webpage";
            myWebpageToolStripMenuItem.Click += myWebpageToolStripMenuItem_Click;
            // 
            // myBlogToolStripMenuItem
            // 
            myBlogToolStripMenuItem.Name = "myBlogToolStripMenuItem";
            myBlogToolStripMenuItem.Size = new Size(142, 22);
            myBlogToolStripMenuItem.Text = "My blog";
            myBlogToolStripMenuItem.Click += myBlogToolStripMenuItem_Click;
            // 
            // guideToolStripMenuItem
            // 
            guideToolStripMenuItem.Name = "guideToolStripMenuItem";
            guideToolStripMenuItem.Size = new Size(142, 22);
            guideToolStripMenuItem.Text = "Guide";
            guideToolStripMenuItem.Click += guideToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(109, 22);
            aboutToolStripMenuItem.Text = "About";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(106, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(109, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(500, 480);
            Controls.Add(linkLabelToggleAdvanced);
            Controls.Add(groupConnectionSettings);
            Controls.Add(pictureboxSupportMe);
            Controls.Add(statusStripLoginForm);
            Controls.Add(menuStripLoginForm);
            Controls.Add(buttonHelpConnectGuide);
            Controls.Add(groupAuth);
            Controls.Add(textboxServer);
            Controls.Add(labelServer);
            Controls.Add(pictureBox1);
            Controls.Add(labelLoginFormToolName);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStripLoginForm;
            MaximizeBox = false;
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "LoginForm";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            groupAuth.ResumeLayout(false);
            groupAuth.PerformLayout();
            groupConnectionSettings.ResumeLayout(false);
            groupConnectionSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericTimeout).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericPort).EndInit();
            statusStripLoginForm.ResumeLayout(false);
            statusStripLoginForm.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureboxSupportMe).EndInit();
            menuStripLoginForm.ResumeLayout(false);
            menuStripLoginForm.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelLoginFormToolName;
        private PictureBox pictureBox1;
        private Label labelServer;
        private TextBox textboxServer;
        private GroupBox groupAuth;
        private Button buttonCancel;
        private Button ButtonLogin;
        private CheckBox checkboxRemember;
        private Label labelPassword;
        private Label labelUsername;
        private TextBox textboxPassword;
        private TextBox textboxUsername;
        private RadioButton radioCustom;
        private RadioButton radioWindows;
        private GroupBox groupConnectionSettings;
        private CheckBox checkboxUseSSL;
        private NumericUpDown numericPort;
        private Label labelPort;
        private ComboBox comboAuthMechanism;
        private Label labelAuthMechanism;
        private CheckBox checkboxSkipCACheck;
        private CheckBox checkboxSkipCNCheck;
        private NumericUpDown numericTimeout;
        private Label labelTimeout;
        private Button buttonHelpConnectGuide;
        private StatusStrip statusStripLoginForm;
        private ToolStripStatusLabel toolStripStatusLabelLoginForm;
        private ToolStripStatusLabel toolStripStatusLabelTextLoginForm;
        private PictureBox pictureboxSupportMe;
        private MenuStrip menuStripLoginForm;
        private ToolStripMenuItem menuToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem onlineToolStripMenuItem;
        private ToolStripMenuItem myWebpageToolStripMenuItem;
        private ToolStripMenuItem myBlogToolStripMenuItem;
        private ToolStripMenuItem guideToolStripMenuItem;
        private Button buttonReset;
        private LinkLabel linkLabelToggleAdvanced;
        private ToolStripSeparator toolStripSeparator1;
    }
}