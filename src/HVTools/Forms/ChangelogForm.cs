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
    $" Version 1.0.0.0 alpha 1 (11-03-2026):{Environment.NewLine}" +
    $" First public release";

            // Set the content in the RichTextBox control
            richTextBoxChangelog.Text = changelogContent;
        }

        private void ChangelogForm_Load(object sender, EventArgs e)
        {
            PopulateChangelog();
        }
    }
}