namespace HVTools.Forms
{
    partial class ConnectionSettingsDialog
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
            groupBoxConnection = new GroupBox();
            numericPort = new NumericUpDown();
            labelPort = new Label();
            checkBoxUseSSL = new CheckBox();
            groupBoxAuthentication = new GroupBox();
            comboBoxAuthMechanism = new ComboBox();
            labelAuthMechanism = new Label();
            textBoxPassword = new TextBox();
            labelPassword = new Label();
            textBoxUsername = new TextBox();
            labelUsername = new Label();
            radioButtonExplicitCreds = new RadioButton();
            radioButtonCurrentUser = new RadioButton();
            groupBoxAdvanced = new GroupBox();
            numericTimeout = new NumericUpDown();
            labelTimeout = new Label();
            checkBoxSkipCNCheck = new CheckBox();
            checkBoxSkipCACheck = new CheckBox();
            buttonOK = new Button();
            buttonCancel = new Button();
            buttonReset = new Button();
            labelDescription = new Label();
            groupBoxConnection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericPort).BeginInit();
            groupBoxAuthentication.SuspendLayout();
            groupBoxAdvanced.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericTimeout).BeginInit();
            SuspendLayout();
            // 
            // groupBoxConnection
            // 
            groupBoxConnection.Controls.Add(numericPort);
            groupBoxConnection.Controls.Add(labelPort);
            groupBoxConnection.Controls.Add(checkBoxUseSSL);
            groupBoxConnection.Location = new Point(12, 75);
            groupBoxConnection.Name = "groupBoxConnection";
            groupBoxConnection.Size = new Size(460, 90);
            groupBoxConnection.TabIndex = 0;
            groupBoxConnection.TabStop = false;
            groupBoxConnection.Text = "Connection Settings";
            // 
            // numericPort
            // 
            numericPort.Location = new Point(120, 52);
            numericPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numericPort.Name = "numericPort";
            numericPort.Size = new Size(100, 23);
            numericPort.TabIndex = 2;
            // 
            // labelPort
            // 
            labelPort.AutoSize = true;
            labelPort.Location = new Point(15, 54);
            labelPort.Name = "labelPort";
            labelPort.Size = new Size(99, 15);
            labelPort.TabIndex = 1;
            labelPort.Text = "Port (0 = default):";
            // 
            // checkBoxUseSSL
            // 
            checkBoxUseSSL.AutoSize = true;
            checkBoxUseSSL.Location = new Point(15, 25);
            checkBoxUseSSL.Name = "checkBoxUseSSL";
            checkBoxUseSSL.Size = new Size(221, 19);
            checkBoxUseSSL.TabIndex = 0;
            checkBoxUseSSL.Text = "Use SSL/HTTPS (port 5986 by default)";
            checkBoxUseSSL.UseVisualStyleBackColor = true;
            // 
            // groupBoxAuthentication
            // 
            groupBoxAuthentication.Controls.Add(comboBoxAuthMechanism);
            groupBoxAuthentication.Controls.Add(labelAuthMechanism);
            groupBoxAuthentication.Controls.Add(textBoxPassword);
            groupBoxAuthentication.Controls.Add(labelPassword);
            groupBoxAuthentication.Controls.Add(textBoxUsername);
            groupBoxAuthentication.Controls.Add(labelUsername);
            groupBoxAuthentication.Controls.Add(radioButtonExplicitCreds);
            groupBoxAuthentication.Controls.Add(radioButtonCurrentUser);
            groupBoxAuthentication.Location = new Point(12, 171);
            groupBoxAuthentication.Name = "groupBoxAuthentication";
            groupBoxAuthentication.Size = new Size(460, 180);
            groupBoxAuthentication.TabIndex = 1;
            groupBoxAuthentication.TabStop = false;
            groupBoxAuthentication.Text = "Authentication";
            // 
            // comboBoxAuthMechanism
            // 
            comboBoxAuthMechanism.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxAuthMechanism.FormattingEnabled = true;
            comboBoxAuthMechanism.Items.AddRange(new object[] { "Default", "Kerberos", "Negotiate", "Basic", "CredSSP" });
            comboBoxAuthMechanism.Location = new Point(150, 140);
            comboBoxAuthMechanism.Name = "comboBoxAuthMechanism";
            comboBoxAuthMechanism.Size = new Size(150, 23);
            comboBoxAuthMechanism.TabIndex = 7;
            // 
            // labelAuthMechanism
            // 
            labelAuthMechanism.AutoSize = true;
            labelAuthMechanism.Location = new Point(15, 143);
            labelAuthMechanism.Name = "labelAuthMechanism";
            labelAuthMechanism.Size = new Size(129, 15);
            labelAuthMechanism.TabIndex = 6;
            labelAuthMechanism.Text = "Authentication Method:";
            // 
            // textBoxPassword
            // 
            textBoxPassword.Enabled = false;
            textBoxPassword.Location = new Point(120, 108);
            textBoxPassword.Name = "textBoxPassword";
            textBoxPassword.Size = new Size(320, 23);
            textBoxPassword.TabIndex = 5;
            textBoxPassword.UseSystemPasswordChar = true;
            // 
            // labelPassword
            // 
            labelPassword.AutoSize = true;
            labelPassword.Location = new Point(35, 111);
            labelPassword.Name = "labelPassword";
            labelPassword.Size = new Size(60, 15);
            labelPassword.TabIndex = 4;
            labelPassword.Text = "Password:";
            // 
            // textBoxUsername
            // 
            textBoxUsername.Enabled = false;
            textBoxUsername.Location = new Point(120, 79);
            textBoxUsername.Name = "textBoxUsername";
            textBoxUsername.Size = new Size(320, 23);
            textBoxUsername.TabIndex = 3;
            // 
            // labelUsername
            // 
            labelUsername.AutoSize = true;
            labelUsername.Location = new Point(35, 82);
            labelUsername.Name = "labelUsername";
            labelUsername.Size = new Size(63, 15);
            labelUsername.TabIndex = 2;
            labelUsername.Text = "Username:";
            // 
            // radioButtonExplicitCreds
            // 
            radioButtonExplicitCreds.AutoSize = true;
            radioButtonExplicitCreds.Location = new Point(15, 54);
            radioButtonExplicitCreds.Name = "radioButtonExplicitCreds";
            radioButtonExplicitCreds.Size = new Size(128, 19);
            radioButtonExplicitCreds.TabIndex = 1;
            radioButtonExplicitCreds.Text = "Explicit Credentials";
            radioButtonExplicitCreds.UseVisualStyleBackColor = true;
            radioButtonExplicitCreds.CheckedChanged += RadioButtonAuth_CheckedChanged;
            // 
            // radioButtonCurrentUser
            // 
            radioButtonCurrentUser.AutoSize = true;
            radioButtonCurrentUser.Checked = true;
            radioButtonCurrentUser.Location = new Point(15, 29);
            radioButtonCurrentUser.Name = "radioButtonCurrentUser";
            radioButtonCurrentUser.Size = new Size(226, 19);
            radioButtonCurrentUser.TabIndex = 0;
            radioButtonCurrentUser.TabStop = true;
            radioButtonCurrentUser.Text = "Current User (Kerberos/NTLM/CredSSP)";
            radioButtonCurrentUser.UseVisualStyleBackColor = true;
            radioButtonCurrentUser.CheckedChanged += RadioButtonAuth_CheckedChanged;
            // 
            // groupBoxAdvanced
            // 
            groupBoxAdvanced.Controls.Add(numericTimeout);
            groupBoxAdvanced.Controls.Add(labelTimeout);
            groupBoxAdvanced.Controls.Add(checkBoxSkipCNCheck);
            groupBoxAdvanced.Controls.Add(checkBoxSkipCACheck);
            groupBoxAdvanced.Location = new Point(12, 357);
            groupBoxAdvanced.Name = "groupBoxAdvanced";
            groupBoxAdvanced.Size = new Size(460, 115);
            groupBoxAdvanced.TabIndex = 2;
            groupBoxAdvanced.TabStop = false;
            groupBoxAdvanced.Text = "Advanced Settings";
            // 
            // numericTimeout
            // 
            numericTimeout.Location = new Point(150, 80);
            numericTimeout.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            numericTimeout.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            numericTimeout.Name = "numericTimeout";
            numericTimeout.Size = new Size(100, 23);
            numericTimeout.TabIndex = 3;
            numericTimeout.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // labelTimeout
            // 
            labelTimeout.AutoSize = true;
            labelTimeout.Location = new Point(15, 82);
            labelTimeout.Name = "labelTimeout";
            labelTimeout.Size = new Size(122, 15);
            labelTimeout.TabIndex = 2;
            labelTimeout.Text = "Timeout (in seconds):";
            // 
            // checkBoxSkipCNCheck
            // 
            checkBoxSkipCNCheck.AutoSize = true;
            checkBoxSkipCNCheck.Location = new Point(15, 51);
            checkBoxSkipCNCheck.Name = "checkBoxSkipCNCheck";
            checkBoxSkipCNCheck.Size = new Size(285, 19);
            checkBoxSkipCNCheck.TabIndex = 1;
            checkBoxSkipCNCheck.Text = "Skip CN (Common Name) check for SSL (unsafe)";
            checkBoxSkipCNCheck.UseVisualStyleBackColor = true;
            // 
            // checkBoxSkipCACheck
            // 
            checkBoxSkipCACheck.AutoSize = true;
            checkBoxSkipCACheck.Location = new Point(15, 26);
            checkBoxSkipCACheck.Name = "checkBoxSkipCACheck";
            checkBoxSkipCACheck.Size = new Size(341, 19);
            checkBoxSkipCACheck.TabIndex = 0;
            checkBoxSkipCACheck.Text = "Skip CA (Certificate Authority) check for SSL certificates (unsafe)";
            checkBoxSkipCACheck.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            buttonOK.Location = new Point(291, 488);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(85, 30);
            buttonOK.TabIndex = 3;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += ButtonOK_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(387, 488);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(85, 30);
            buttonCancel.TabIndex = 4;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonReset
            // 
            buttonReset.Location = new Point(12, 488);
            buttonReset.Name = "buttonReset";
            buttonReset.Size = new Size(120, 30);
            buttonReset.TabIndex = 5;
            buttonReset.Text = "Reset to Defaults";
            buttonReset.UseVisualStyleBackColor = true;
            buttonReset.Click += ButtonReset_Click;
            // 
            // labelDescription
            // 
            labelDescription.Location = new Point(12, 9);
            labelDescription.Name = "labelDescription";
            labelDescription.Size = new Size(460, 60);
            labelDescription.TabIndex = 6;
            labelDescription.Text = "Configure advanced connection settings for remote Hyper-V connections. These settings allow you to customize authentication, SSL/TLS encryption, timeouts, and certificate validation.\r\n\r\nNote: These settings apply to remote connections only.";
            // 
            // ConnectionSettingsDialog
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(484, 530);
            Controls.Add(labelDescription);
            Controls.Add(buttonReset);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(groupBoxAdvanced);
            Controls.Add(groupBoxAuthentication);
            Controls.Add(groupBoxConnection);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConnectionSettingsDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Connection Settings";
            groupBoxConnection.ResumeLayout(false);
            groupBoxConnection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericPort).EndInit();
            groupBoxAuthentication.ResumeLayout(false);
            groupBoxAuthentication.PerformLayout();
            groupBoxAdvanced.ResumeLayout(false);
            groupBoxAdvanced.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericTimeout).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBoxConnection;
        private CheckBox checkBoxUseSSL;
        private NumericUpDown numericPort;
        private Label labelPort;
        private GroupBox groupBoxAuthentication;
        private RadioButton radioButtonCurrentUser;
        private RadioButton radioButtonExplicitCreds;
        private TextBox textBoxPassword;
        private Label labelPassword;
        private TextBox textBoxUsername;
        private Label labelUsername;
        private ComboBox comboBoxAuthMechanism;
        private Label labelAuthMechanism;
        private GroupBox groupBoxAdvanced;
        private CheckBox checkBoxSkipCACheck;
        private CheckBox checkBoxSkipCNCheck;
        private NumericUpDown numericTimeout;
        private Label labelTimeout;
        private Button buttonOK;
        private Button buttonCancel;
        private Button buttonReset;
        private Label labelDescription;
    }
}
