namespace ClientUI
{
    partial class ClientUI
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            IPTextBox = new TextBox();
            IPLabel = new Label();
            StartButton = new Button();
            IPCheckBox = new CheckBox();
            PortLabel = new Label();
            PortTextBox = new TextBox();
            PortCheckBox = new CheckBox();
            StopButton = new Button();
            MarginLabel = new Label();
            MarginTextBox = new TextBox();
            toolTip1 = new ToolTip(components);
            EdgeLabel = new Label();
            EdgeComboBox = new ComboBox();
            AddServer = new Button();
            flowLayoutPanel1 = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // IPTextBox
            // 
            IPTextBox.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            IPTextBox.Location = new Point(141, 44);
            IPTextBox.Name = "IPTextBox";
            IPTextBox.Size = new Size(187, 29);
            IPTextBox.TabIndex = 0;
            // 
            // IPLabel
            // 
            IPLabel.AutoSize = true;
            IPLabel.BackColor = Color.Transparent;
            IPLabel.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            IPLabel.Location = new Point(12, 47);
            IPLabel.Name = "IPLabel";
            IPLabel.Size = new Size(123, 21);
            IPLabel.TabIndex = 1;
            IPLabel.Text = "Enter IP Address";
            // 
            // StartButton
            // 
            StartButton.Location = new Point(671, 380);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(92, 23);
            StartButton.TabIndex = 2;
            StartButton.Text = "Start Client";
            StartButton.UseVisualStyleBackColor = true;
            StartButton.Click += StartButtonClick;
            // 
            // IPCheckBox
            // 
            IPCheckBox.AutoSize = true;
            IPCheckBox.Location = new Point(141, 79);
            IPCheckBox.Name = "IPCheckBox";
            IPCheckBox.Size = new Size(63, 19);
            IPCheckBox.TabIndex = 3;
            IPCheckBox.Text = "Save IP";
            IPCheckBox.UseVisualStyleBackColor = true;
            // 
            // PortLabel
            // 
            PortLabel.AutoSize = true;
            PortLabel.Font = new Font("Segoe UI", 12F);
            PortLabel.Location = new Point(12, 112);
            PortLabel.Name = "PortLabel";
            PortLabel.Size = new Size(78, 21);
            PortLabel.TabIndex = 4;
            PortLabel.Text = "Enter Port";
            // 
            // PortTextBox
            // 
            PortTextBox.Location = new Point(141, 114);
            PortTextBox.Name = "PortTextBox";
            PortTextBox.Size = new Size(187, 23);
            PortTextBox.TabIndex = 5;
            // 
            // PortCheckBox
            // 
            PortCheckBox.AutoSize = true;
            PortCheckBox.Location = new Point(141, 143);
            PortCheckBox.Name = "PortCheckBox";
            PortCheckBox.Size = new Size(75, 19);
            PortCheckBox.TabIndex = 6;
            PortCheckBox.Text = "Save Port";
            PortCheckBox.UseVisualStyleBackColor = true;
            // 
            // StopButton
            // 
            StopButton.Location = new Point(769, 380);
            StopButton.Name = "StopButton";
            StopButton.Size = new Size(92, 23);
            StopButton.TabIndex = 7;
            StopButton.Text = "Stop Client";
            StopButton.UseVisualStyleBackColor = true;
            StopButton.Click += StopButtonClick;
            // 
            // MarginLabel
            // 
            MarginLabel.AutoSize = true;
            MarginLabel.Location = new Point(369, 15);
            MarginLabel.Name = "MarginLabel";
            MarginLabel.Size = new Size(45, 15);
            MarginLabel.TabIndex = 8;
            MarginLabel.Text = "Margin";
            toolTip1.SetToolTip(MarginLabel, "The number of pixels away from the edge of the screen it will connect to the server.");
            // 
            // MarginTextBox
            // 
            MarginTextBox.Location = new Point(420, 12);
            MarginTextBox.Name = "MarginTextBox";
            MarginTextBox.Size = new Size(100, 23);
            MarginTextBox.TabIndex = 9;
            MarginTextBox.Text = "1";
            MarginTextBox.KeyPress += MarginKeyPress;
            // 
            // EdgeLabel
            // 
            EdgeLabel.AutoSize = true;
            EdgeLabel.Location = new Point(568, 15);
            EdgeLabel.Name = "EdgeLabel";
            EdgeLabel.Size = new Size(33, 15);
            EdgeLabel.TabIndex = 11;
            EdgeLabel.Text = "Edge";
            toolTip1.SetToolTip(EdgeLabel, "Which side to drag the mouse towards to connect to the server.");
            // 
            // EdgeComboBox
            // 
            EdgeComboBox.FormattingEnabled = true;
            EdgeComboBox.Location = new Point(619, 12);
            EdgeComboBox.Name = "EdgeComboBox";
            EdgeComboBox.Size = new Size(100, 23);
            EdgeComboBox.TabIndex = 10;
            EdgeComboBox.Text = "None";
            EdgeComboBox.SelectedValueChanged += EdgeComboBox_SelectedValueChanged;
            // 
            // AddServer
            // 
            AddServer.Location = new Point(459, 356);
            AddServer.Name = "AddServer";
            AddServer.Size = new Size(115, 23);
            AddServer.TabIndex = 12;
            AddServer.Text = "Add New Server";
            AddServer.UseVisualStyleBackColor = true;
            AddServer.Click += AddServer_Click;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Location = new Point(12, 168);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(384, 238);
            flowLayoutPanel1.TabIndex = 13;
            // 
            // ClientUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(873, 415);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(AddServer);
            Controls.Add(EdgeLabel);
            Controls.Add(EdgeComboBox);
            Controls.Add(MarginTextBox);
            Controls.Add(MarginLabel);
            Controls.Add(StopButton);
            Controls.Add(PortCheckBox);
            Controls.Add(PortTextBox);
            Controls.Add(PortLabel);
            Controls.Add(IPCheckBox);
            Controls.Add(StartButton);
            Controls.Add(IPLabel);
            Controls.Add(IPTextBox);
            Name = "ClientUI";
            Text = "Client Interface";
            Load += ClientUI_Load;
            Paint += ClientUI_Paint;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox IPTextBox;
        private Label IPLabel;
        private Button StartButton;
        private CheckBox IPCheckBox;
        private Label PortLabel;
        private TextBox PortTextBox;
        private CheckBox PortCheckBox;
        private Button StopButton;
        private Label MarginLabel;
        private TextBox MarginTextBox;
        private ToolTip toolTip1;
        private ComboBox EdgeComboBox;
        private Label EdgeLabel;
        private Button AddServer;
        private FlowLayoutPanel flowLayoutPanel1;
    }
}
