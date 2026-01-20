namespace HyperView.Forms
{
    public partial class ValidationProgressForm : Form
    {
        public ValidationProgressForm()
        {
            InitializeComponent();
            
            // Ensure the progress bar animates
            this.Load += ValidationProgressForm_Load;
            this.Shown += ValidationProgressForm_Shown;
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
            this.Refresh();
            Application.DoEvents();
        }
    }
}

