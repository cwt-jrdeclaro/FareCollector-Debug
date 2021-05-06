using log4net.Config;
using System;
using System.Threading;
using System.Windows.Forms;

namespace FareCollector
{
    public partial class DebugStartupForm : Form
    {
        public DebugStartupForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.DoEvents();
            XmlConfigurator.Configure();
            Program.Start();
            statusLabel.Text = "Started";
            StartAppButton.Enabled = false;
            stopButton.Enabled = true;
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            statusLabel.Text = "Stopping...";
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Program.Stop();
            statusLabel.Text = "Stopped";
            StartAppButton.Enabled = true;
            stopButton.Enabled = false;
        }
    }
}