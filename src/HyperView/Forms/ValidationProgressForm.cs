namespace HVTools.Forms
{
    public partial class ValidationProgressForm : Form
    {
        public ValidationProgressForm()
        {
            InitializeComponent();
        }

        private void ValidationProgressForm_Load(object sender, EventArgs e)
        {
            // Set marquee animation speed (milliseconds for each step)
            ProgressBar.Style = ProgressBarStyle.Marquee;
            ProgressBar.MarqueeAnimationSpeed = 30; // Faster animation
        }

        private void ValidationProgressForm_Shown(object sender, EventArgs e)
        {
            // Force the progress bar to start animating when form is shown
            ProgressBar.MarqueeAnimationSpeed = 30;
            Refresh();
            Application.DoEvents();
        }
    }
}

