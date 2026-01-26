namespace HVTools.Forms
{
    public partial class ChangelogForm : Form
    {
        public ChangelogForm()
        {
            InitializeComponent();
        }

        private void PopulateChangelog()
        {
            // Changelog content
            var changelogContent =
                                   " Version 1.0.0.0 (xx-xx-xxxx):\n" +
                                   " New Features\n" +
                                   " - Export command script (.ps1) feature:\n" +
                                   "   - Supports Windows Certificate Store, PFX, and Azure Trusted Signing modes with per-file signing and exit code checks\n" +
                                   "   - Optional BatchMode for Trusted Signing (single signtool call for multiple files)";

            // Set the content in the RichTextBox control
            richTextBoxChangelog.Text = changelogContent;
        }

        private void ChangelogForm_Load(object sender, EventArgs e)
        {
            PopulateChangelog();
        }
    }
}
