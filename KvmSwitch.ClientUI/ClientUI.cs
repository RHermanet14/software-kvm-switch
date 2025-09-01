using Shared;
using System.Diagnostics;

namespace ClientUI
{
    public partial class ClientUI : Form
    {
        private int objectCount = 0;
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
            flowLayoutPanel1.AutoScroll = true;
            NewServer();
        }

        private void StartButtonClick(object sender, EventArgs e)
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
                ProcessStartInfo startInfo = new()
                {
                    FileName = "KvmSwitch.Client.exe",
                    //UseShellExecute = false,
                    //CreateNoWindow = true,
                };
                startInfo.ArgumentList.Add(IPTextBox.Text);
                startInfo.ArgumentList.Add(PortTextBox.Text);
                startInfo.ArgumentList.Add(((int)dir).ToString());
                startInfo.ArgumentList.Add(MarginTextBox.Text);
                _clientProcess = Process.Start(startInfo);
            }

        }

        private void StopButtonClick(object sender, EventArgs e)
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

        private void MarginKeyPress(object sender, KeyPressEventArgs e)
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
        private void NewServer()
        {
            Panel objectPanel = new Panel();
            objectPanel.BorderStyle = BorderStyle.FixedSingle;
            objectPanel.Width = 300;
            objectPanel.Height = 130;
            objectPanel.Margin = new Padding(5);

            Label lblName = new Label();
            lblName.Text = $"Enter IP Address";
            lblName.Location = new Point(10, 10);
            lblName.AutoSize = true;

            TextBox txtName = new TextBox();
            txtName.Location = new Point(100, 10);
            txtName.Width = 150;

            Label lblAge = new Label();
            lblAge.Text = $"Enter Port";
            lblAge.Location = new Point(10, 40);
            lblAge.AutoSize = true;

            TextBox txtAge = new TextBox();
            txtAge.Location = new Point(100, 40);
            txtAge.Width = 55;

            Label lblMargin = new Label();
            lblMargin.Text = $"Margin";
            lblMargin.Location = new Point(10, 70);
            lblMargin.AutoSize = true;

            TextBox txtMargin = new();
            txtMargin.Name = $"txtMargin_{objectCount}";
            txtMargin.Location = new Point(100, 70);
            txtMargin.Width = 55;
            txtMargin.KeyPress += (sender, e) =>
            {
                if (!char.IsDigit(e.KeyChar) && e.KeyChar != 8) // char 8 = Backspace
                {
                    e.Handled = true;
                }
            };

            Label lblEdge = new();
            lblEdge.Text = $"Edge";
            lblEdge.Location = new Point(10, 100);
            lblEdge.AutoSize = true;

            ComboBox cbEdge = new();
            cbEdge.DropDownStyle = ComboBoxStyle.DropDownList;
            cbEdge.Items.Add("Up");
            cbEdge.Items.Add("Down");
            cbEdge.Items.Add("Left");
            cbEdge.Items.Add("Right");
            cbEdge.SelectedIndex = objectCount;
            cbEdge.Location = new Point(100, 100);
            cbEdge.Width = 55;

            Button btnRemove = new();
            btnRemove.Text = "Remove";
            btnRemove.Location = new Point(200, 100);
            btnRemove.Click += (s, args) =>
            {
                flowLayoutPanel1.Controls.Remove(objectPanel);
                objectPanel.Dispose();
                objectCount--;
            };

            objectPanel.Controls.Add(lblName);
            objectPanel.Controls.Add(txtName);
            objectPanel.Controls.Add(lblAge);
            objectPanel.Controls.Add(txtAge);
            objectPanel.Controls.Add(lblMargin);
            objectPanel.Controls.Add(txtMargin);
            objectPanel.Controls.Add(lblEdge);
            objectPanel.Controls.Add(cbEdge);
            objectPanel.Controls.Add(btnRemove);

            flowLayoutPanel1.Controls.Add(objectPanel);
        }
        private void AddServer_Click(object sender, EventArgs e)
        {
            if (objectCount >= 3) return;
            objectCount++;
            NewServer();
        }
    }
}
