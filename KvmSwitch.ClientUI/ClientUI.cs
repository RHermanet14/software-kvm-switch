using Shared;
using System.Diagnostics;

namespace ClientUI
{
    public partial class ClientUI : Form
    {
        private Direction dir = Direction.Left;
        private Process? _clientProcess;
        public ClientUI()
        {
            InitializeComponent();
        }

        private void ClientUI_Load(object sender, EventArgs e)
        {
            EdgeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            EdgeComboBox.Items.Add("Up");
            EdgeComboBox.Items.Add("Down");
            EdgeComboBox.Items.Add("Left");
            EdgeComboBox.Items.Add("Right");
            EdgeComboBox.SelectedIndex = 2;
            StopButton.Enabled = false;
            IPTextBox.Text = Properties.Settings.Default.IP;
            PortTextBox.Text = Properties.Settings.Default.Port;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(IPTextBox.Text))
            {
                MessageBox.Show("Error: no IP provided");
            }
            else if (string.IsNullOrEmpty(PortTextBox.Text))
            {
                MessageBox.Show("Error: no port provided");
            }
            else
            {
                if (IPCheckBox.Checked)
                {
                    Properties.Settings.Default.IP = IPTextBox.Text;
                }
                if (PortCheckBox.Checked)
                {
                    Properties.Settings.Default.Port = PortTextBox.Text;
                }
                Properties.Settings.Default.Save();
                StartButton.Enabled = false;
                StopButton.Enabled = true;
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
            StartButton.Enabled = true;
            StopButton.Enabled = false;
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

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != 8) // char 8 = Backspace
            {
                e.Handled = true;
            }
        }

        private void ClientUI_Paint(object sender, PaintEventArgs e)
        {
            DrawMonitor(575, 150, "Client", e);
            switch (dir)
            {
                case Direction.Up:
                    DrawMonitor(575, 30, "Server 1", e);
                    break;
                case Direction.Down:
                    DrawMonitor(575, 270, "Server 1", e);
                    break;
                case Direction.Left:
                    DrawMonitor(450, 150, "Server 1", e);
                    break;
                case Direction.Right:
                    DrawMonitor(700, 150, "Server 1", e);
                    break;
                default:
                    MessageBox.Show("Error: invalid direction inputted");
                    break;
            }

        }
        private void DrawMonitor(int x, int y, string text, PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(x, y, 100, 70);
            e.Graphics.DrawRectangle(Pens.Gray, rect);
            e.Graphics.DrawLine(Pens.Gray, new Point(x + 50, y + 70), new Point(x + 50, y + 95));
            e.Graphics.DrawLine(Pens.Gray, new Point(x + 25, y + 95), new Point(x + 75, y + 95));
            string s = text;
            Font font = new Font("Arial", 12);
            Brush brush = Brushes.Black;
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            e.Graphics.DrawString(s, font, brush, rect, sf);
        }

        private void EdgeComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            dir = (Direction)EdgeComboBox.SelectedIndex;
            Refresh();
        }
    }
}
