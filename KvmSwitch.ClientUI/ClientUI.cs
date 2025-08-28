using System.Diagnostics;

namespace ClientUI
{
    public partial class ClientUI : Form
    {
        private Process? _clientProcess;
        public ClientUI()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void ClientUI_Load(object sender, EventArgs e)
        {
            button2.Enabled = false;
            textBox1.Text = Properties.Settings.Default.IP;
            textBox2.Text = Properties.Settings.Default.Port;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Error: no IP provided");
            }
            else if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("Error: no port provided");
            }
            else
            {
                if (checkBox1.Checked)
                {
                    Properties.Settings.Default.IP = textBox1.Text;
                }
                if (checkBox2.Checked)
                {
                    Properties.Settings.Default.Port = textBox2.Text;
                }
                Properties.Settings.Default.Save();
                button1.Enabled = false;
                button2.Enabled = true;
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "KvmSwitch.Client.exe",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                _clientProcess = Process.Start(startInfo);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = false;
            KillClient();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            KillClient();
        }
        private void KillClient()
        {
            if (_clientProcess != null && !_clientProcess.HasExited)
            {
                _clientProcess.Kill();
                _clientProcess.Dispose();
                _clientProcess = null;
            }
        }
    }
}
