using Shared;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms.VisualStyles;

namespace ClientUI
{
    public partial class ClientUI : Form
    {
        private readonly List<string> serverOptions = ["Up", "Down", "Left", "Right"];
        private int serverCount = 0;
        private Process? _clientProcess;
       
        public ClientUI()
        {
            InitializeComponent();
        }
        
        private void LoadServerPreferences()
        {
            string json = KvmSwitch.ClientUI.Properties.Settings.Default.SavedServersJson;
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    List<ConnectInfo>? servers = JsonSerializer.Deserialize<List<ConnectInfo>>(json);
                    if (servers != null)
                    {
                        foreach (ConnectInfo server in servers)
                        {
                            NewServer(server.IP, server.Port, server.Display.edge, server.Display.margin);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load preferences: " + ex.Message);
                }
            }
        }

        private void ClientUI_Load(object sender, EventArgs e)
        {
            StopButton.Enabled = false;
            flowLayoutPanelServers.AutoScroll = true;
            LoadServerPreferences();
        }

        private void StartButtonClick(object sender, EventArgs e)
        {
            int serverCount = 0;
            bool isReady = true;
            ProcessStartInfo startInfo = new()
            {
                FileName = "KvmSwitch.Client.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            foreach (Panel panel in flowLayoutPanelServers.Controls)
            {
                serverCount++;
                foreach (Control ctrl in panel.Controls)
                {
                    if (ctrl is TextBox txt)
                    {
                        if (string.IsNullOrEmpty(txt.Text) || txt.Text == "-1")
                        {
                            isReady = false;
                        } else
                        {
                            if (txt.Name.StartsWith("txtIP"))
                            {
                                startInfo.ArgumentList.Add(txt.Text);
                            }
                            else if (txt.Name.StartsWith("txtPort"))
                            {
                                startInfo.ArgumentList.Add(txt.Text);
                            }
                            else // txtMargin
                            {
                                startInfo.ArgumentList.Add(txt.Text);
                            }
                        }
                        
                    }
                    else if (ctrl is ComboBox cb)
                    {
                        if (cb.SelectedItem == null)
                        {
                            isReady = false;
                        } else
                        {
                            startInfo.ArgumentList.Add(cb.SelectedIndex.ToString());
                        }
                    }
                }
            }
            if (isReady && serverCount > 0)
            {
                StartButton.Enabled = false;
                StopButton.Enabled = true;
                _clientProcess = Process.Start(startInfo);
            } else
            {
                MessageBox.Show("Error: missing parameters in one or multiple servers");
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
       
        private void ClientUI_Paint(object sender, PaintEventArgs e)
        {
            int serverNum = 0;
            DrawMonitor(575, 150, "Client", e);
            foreach (Panel panel in flowLayoutPanelServers.Controls)
            {
                serverNum++;
                foreach (Control ctrl in panel.Controls)
                {
                    if (ctrl is ComboBox cb)
                    {
                        switch ((Direction)cb.SelectedIndex)
                        {
                            case Direction.Up:
                                DrawMonitor(575, 30, $"Server {serverNum}", e);
                                break;
                            case Direction.Down:
                                DrawMonitor(575, 270, $"Server {serverNum}", e);
                                break;
                            case Direction.Left:
                                DrawMonitor(450, 150, $"Server {serverNum}", e);
                                break;
                            case Direction.Right:
                                DrawMonitor(700, 150, $"Server {serverNum}", e);
                                break;
                            default:
                                //MessageBox.Show("Error: invalid direction inputted");
                                break;
                        }
                    }
                }
            }
        }
        
        private static void DrawMonitor(int x, int y, string text, PaintEventArgs e)
        {
            Rectangle rect = new(x, y, 100, 70);
            e.Graphics.DrawRectangle(Pens.Gray, rect);
            e.Graphics.DrawLine(Pens.Gray, new Point(x + 50, y + 70), new Point(x + 50, y + 95));
            e.Graphics.DrawLine(Pens.Gray, new Point(x + 25, y + 95), new Point(x + 75, y + 95));
            string s = text;
            Font font = new("Arial", 12);
            Brush brush = Brushes.Black;
            StringFormat sf = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            e.Graphics.DrawString(s, font, brush, rect, sf);
        }
       
        private void NewServer(string ip = "", int port = -1, Direction edge = Direction.None, int margin = -1)
        {
            Panel objectPanel = new()
            {
                BorderStyle = BorderStyle.FixedSingle,
                Width = 300,
                Height = 130,
                Margin = new Padding(5)
            };

            Label lblIP = new()
            {
                Text = $"Enter IP Address",
                Location = new Point(10, 10),
                AutoSize = true
            };

            TextBox txtIP = new()
            {
                Name = $"txtIP_{serverCount}",
                Location = new Point(100, 10),
                Width = 150,
                Text = ip
            };

            Label lblPort = new()
            {
                Text = $"Enter Port",
                Location = new Point(10, 40),
                AutoSize = true
            };

            TextBox txtPort = new()
            {
                Name = $"txtPort_{serverCount}",
                Location = new Point(100, 40),
                Width = 55,
                Text = port.ToString()
            };

            Label lblMargin = new()
            {
                Text = $"Margin",
                Location = new Point(10, 70),
                AutoSize = true
            };

            TextBox txtMargin = new()
            {
                Name = $"txtMargin_{serverCount}",
                Location = new Point(100, 70),
                Width = 55,
                Text = margin.ToString()
            };
            txtMargin.KeyPress += (sender, e) =>
            {
                if (!char.IsDigit(e.KeyChar) && e.KeyChar != 8) // char 8 = Backspace
                {
                    e.Handled = true;
                }
            };

            Label lblEdge = new()
            {
                Text = $"Edge",
                Location = new Point(10, 100),
                AutoSize = true
            };

            ComboBox cbEdge = new()
            {
                Name = $"cbEdge_{serverCount}",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(100, 100),
                Width = 55,
            };
            cbEdge.Items.AddRange([.. serverOptions]);
            if (edge != Direction.None)
            {
                cbEdge.SelectedIndex = (int)edge;
            }

            cbEdge.SelectedValueChanged += (s, args) =>
            {
                Refresh();
            };
            Button btnRemove = new()
            {
                Text = "Remove",
                Location = new Point(200, 100)
            };
            btnRemove.Click += (s, args) =>
            {
                flowLayoutPanelServers.Controls.Remove(objectPanel);
                objectPanel.Dispose();
                Refresh();
            };

            objectPanel.Controls.Add(lblIP);
            objectPanel.Controls.Add(txtIP);
            objectPanel.Controls.Add(lblPort);
            objectPanel.Controls.Add(txtPort);
            objectPanel.Controls.Add(lblMargin);
            objectPanel.Controls.Add(txtMargin);
            objectPanel.Controls.Add(lblEdge);
            objectPanel.Controls.Add(cbEdge);
            objectPanel.Controls.Add(btnRemove);

            flowLayoutPanelServers.Controls.Add(objectPanel);
            serverCount++;
        }
        
        private void AddServer_Click(object sender, EventArgs e)
        {
            if (flowLayoutPanelServers.Controls.Count > 3) return;
            NewServer();
        }
       
        private void SavePreferences_Click(object sender, EventArgs e)
        {
            int count = 0;
            List<ConnectInfo> servers = [];
            foreach (Panel panel in flowLayoutPanelServers.Controls)
            {
                string IP = "";
                int Port = -1;
                Direction Edge = Direction.None;
                int Margin = -1;
                foreach (Control ctrl in panel.Controls)
                {
                    if (ctrl is TextBox txt)
                    {
                        if (txt.Name.StartsWith("txtIP"))
                        {
                            IP = txt.Text;
                        }
                        else if (txt.Name.StartsWith("txtPort"))
                        {
                            _ = int.TryParse(txt.Text, out Port);
                        }
                        else // txtMargin
                        {
                            count++;
                            _ = int.TryParse(txt.Text, out Margin);
                        }
                    }
                    else if (ctrl is ComboBox cb)
                    {
                        count++;
                        Edge = (Direction)cb.SelectedIndex;
                    }
                }
                servers.Add(new(IP, Port, Edge, Margin));
            }
            string json = JsonSerializer.Serialize(servers);
            KvmSwitch.ClientUI.Properties.Settings.Default.SavedServersJson = json;
            KvmSwitch.ClientUI.Properties.Settings.Default.Save();
            MessageBox.Show("Preferences Saved.");
        }
    }
}
