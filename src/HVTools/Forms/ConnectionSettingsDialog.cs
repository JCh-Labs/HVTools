using System.Security;
using HVTools.Helpers;
using static HVTools.Helpers.FileLogger;

namespace HVTools.Forms
{
    public partial class ConnectionSettingsDialog : Form
    {
        public ConnectionSettings Settings { get; private set; }

        public ConnectionSettingsDialog(ConnectionSettings? currentSettings = null)
        {
            InitializeComponent();

            // Use provided settings or create defaults
            Settings = currentSettings?.Clone() ?? ConnectionSettings.GetDefault();

            // Load settings into UI
            LoadSettings();

            Message("Connection Settings Dialog opened", EventType.Information, 2000);
        }

        private void LoadSettings()
        {
            // Connection settings
            checkBoxUseSSL.Checked = Settings.UseSSL;
            numericPort.Value = Settings.Port;

            // Authentication settings
            radioButtonCurrentUser.Checked = Settings.UseCurrentUser;
            radioButtonExplicitCreds.Checked = !Settings.UseCurrentUser;
            textBoxUsername.Text = Settings.Username ?? string.Empty;

            // Convert SecureString to plain text for display (if exists)
            if (Settings.SecurePassword != null && Settings.SecurePassword.Length > 0)
            {
                IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(Settings.SecurePassword);
                try
                {
                    textBoxPassword.Text = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
                }
            }
            else
            {
                textBoxPassword.Text = string.Empty;
            }

            comboBoxAuthMechanism.SelectedItem = Settings.AuthenticationMechanism;
            if (comboBoxAuthMechanism.SelectedIndex < 0)
            {
                comboBoxAuthMechanism.SelectedIndex = 0; // Default
            }

            // Advanced settings
            checkBoxSkipCACheck.Checked = Settings.SkipCACheck;
            checkBoxSkipCNCheck.Checked = Settings.SkipCNCheck;
            numericTimeout.Value = Settings.TimeoutSeconds;

            // Update UI state
            UpdateUIState();
        }

        private void SaveSettings()
        {
            // Connection settings
            Settings.UseSSL = checkBoxUseSSL.Checked;
            Settings.Port = (int)numericPort.Value;

            // Authentication settings
            Settings.UseCurrentUser = radioButtonCurrentUser.Checked;

            if (!Settings.UseCurrentUser)
            {
                Settings.Username = textBoxUsername.Text.Trim();

                // Convert password to SecureString
                SecureString securePassword = new SecureString();
                foreach (char c in textBoxPassword.Text)
                {
                    securePassword.AppendChar(c);
                }
                securePassword.MakeReadOnly();
                Settings.SecurePassword = securePassword;
            }
            else
            {
                Settings.Username = null;
                Settings.SecurePassword = null;
            }

            Settings.AuthenticationMechanism = comboBoxAuthMechanism.SelectedItem?.ToString() ?? "Default";

            // Advanced settings
            Settings.SkipCACheck = checkBoxSkipCACheck.Checked;
            Settings.SkipCNCheck = checkBoxSkipCNCheck.Checked;
            Settings.TimeoutSeconds = (int)numericTimeout.Value;

            Message("Connection settings saved", EventType.Information, 2001);
        }

        private void UpdateUIState()
        {
            bool useExplicitCreds = radioButtonExplicitCreds.Checked;
            textBoxUsername.Enabled = useExplicitCreds;
            textBoxPassword.Enabled = useExplicitCreds;
            labelUsername.Enabled = useExplicitCreds;
            labelPassword.Enabled = useExplicitCreds;
        }

        private void RadioButtonAuth_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateUIState();
        }

        private void ButtonOK_Click(object? sender, EventArgs e)
        {
            // Validate settings
            if (radioButtonExplicitCreds.Checked)
            {
                if (string.IsNullOrWhiteSpace(textBoxUsername.Text))
                {
                    MessageBox.Show("Please enter a username for explicit credentials.",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxUsername.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(textBoxPassword.Text))
                {
                    MessageBox.Show("Please enter a password for explicit credentials.",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxPassword.Focus();
                    return;
                }
            }

            // Save settings and close
            SaveSettings();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonReset_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset all connection settings to their default values?",
                "Reset Settings",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Settings = ConnectionSettings.GetDefault();
                LoadSettings();
                Message("Connection settings reset to defaults", EventType.Information, 2002);
            }
        }
    }
}
