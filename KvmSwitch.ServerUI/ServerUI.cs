using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Server;

namespace ServerUI
{
    public partial class ServerUI : Form
    {
        private Process? _serverProcess;

        public ServerUI()
        {
            InitializeComponent();
        }

        private void ToggleButton_Click(object sender, EventArgs e)
        {
            IPLabel.Visible = !IPLabel.Visible;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            StartButton.Enabled = false;
            StopButton.Enabled = true;
            ProcessStartInfo startInfo = new()
            {
                FileName = "KvmSwitch.Server.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
            };
            startInfo.ArgumentList.Add(PortTextBox.Text);
            _serverProcess = Process.Start(startInfo);
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            StartButton.Enabled = true;
            StopButton.Enabled = false;
            KillServer();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            KillServer();
        }

        private void KillServer()
        {
            if (_serverProcess != null && !_serverProcess.HasExited)
            {
                _serverProcess.StandardInput.WriteLine("exit");
                _serverProcess.StandardInput.Flush();
                _serverProcess.WaitForExit(3000);
                if (!_serverProcess.HasExited)
                    _serverProcess.Kill();
                _serverProcess.Dispose();
                _serverProcess = null;
            }
        }

        private void ServerUI_Load(object sender, EventArgs e)
        {
            StopButton.Enabled = false;
            IPLabel.Visible = false;
            PortTextBox.Text = KvmSwitch.ServerUI.Properties.Settings.Default.Port;
            try
            {
                IPLabel.Text = GetIPAddress();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        
        private static string GetIPAddress()
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up && networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    var ipProperties = networkInterface.GetIPProperties();

                    foreach (var ip in ipProperties.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            return ip.Address.ToString();
                    }
                }
            }
            throw new Exception("No network adapters with an IPv$ address in the system!");
        }

        private void IPLabel_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(IPLabel.Text);
            MessageBox.Show("IP Address Was Copied!");
        }

        private void PortTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != 8) // char 8 = Backspace
            {
                e.Handled = true;
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            KvmSwitch.ServerUI.Properties.Settings.Default.Port = PortTextBox.Text;
            MessageBox.Show("Preference Was Saved.");
        }

        private void IPLabel_MouseMove(object sender, MouseEventArgs e)
        {
            toolTip1.SetToolTip(IPLabel, "Click To Copy IP To Clipboard.");
        }
    }
}
