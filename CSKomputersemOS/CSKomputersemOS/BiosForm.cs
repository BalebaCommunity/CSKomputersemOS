using System;
using System.Drawing;
using System.Windows.Forms;

namespace CSKomputersemOS
{
    public class BiosForm : Form
    {
        private Label biosLabel;
        private ProgressBar progressBar;
        private Timer timer;
        private string[] biosMessages = {
            "Initializing system...",
            "Checking hardware...",
            "Loading drivers...",
            "Configuring settings...",
            "Preparing to boot CSKomputersemOS..."
        };
        private int currentMessage = 0;

        public BiosForm()
        {
            InitializeComponent();
            StartBiosSequence();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(800, 600);
            this.BackColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;

            biosLabel = new Label
            {
                AutoSize = false,
                Size = new Size(700, 30),
                Location = new Point(50, 250),
                Text = "CSKomputersemOS BIOS",
                Font = new Font("Consolas", 16),
                ForeColor = Color.White,
                BackColor = Color.Black
            };

            progressBar = new ProgressBar
            {
                Size = new Size(700, 30),
                Location = new Point(50, 300),
                Style = ProgressBarStyle.Continuous
            };

            this.Controls.Add(biosLabel);
            this.Controls.Add(progressBar);

            timer = new Timer
            {
                Interval = 1000
            };
            timer.Tick += Timer_Tick;
        }

        private void StartBiosSequence()
        {
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (currentMessage < biosMessages.Length)
            {
                biosLabel.Text = biosMessages[currentMessage];
                progressBar.Value = (int)((float)(currentMessage + 1) / biosMessages.Length * 100);
                currentMessage++;
            }
            else
            {
                timer.Stop();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}