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
            StartButton = new Button();
            StopButton = new Button();
            toolTip1 = new ToolTip(components);
            AddServer = new Button();
            flowLayoutPanelServers = new FlowLayoutPanel();
            SavePreferences = new Button();
            SuspendLayout();
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
            // AddServer
            // 
            AddServer.Location = new Point(369, 336);
            AddServer.Name = "AddServer";
            AddServer.Size = new Size(115, 23);
            AddServer.TabIndex = 12;
            AddServer.Text = "Add New Server";
            AddServer.UseVisualStyleBackColor = true;
            AddServer.Click += AddServer_Click;
            // 
            // flowLayoutPanelServers
            // 
            flowLayoutPanelServers.Location = new Point(12, 12);
            flowLayoutPanelServers.Name = "flowLayoutPanelServers";
            flowLayoutPanelServers.Size = new Size(330, 391);
            flowLayoutPanelServers.TabIndex = 13;
            // 
            // SavePreferences
            // 
            SavePreferences.Location = new Point(369, 380);
            SavePreferences.Name = "SavePreferences";
            SavePreferences.Size = new Size(115, 23);
            SavePreferences.TabIndex = 14;
            SavePreferences.Text = "Save Preferences";
            SavePreferences.UseVisualStyleBackColor = true;
            SavePreferences.Click += SavePreferences_Click;
            // 
            // ClientUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(873, 415);
            Controls.Add(SavePreferences);
            Controls.Add(flowLayoutPanelServers);
            Controls.Add(AddServer);
            Controls.Add(StopButton);
            Controls.Add(StartButton);
            Name = "ClientUI";
            Text = "Client Interface";
            Load += ClientUI_Load;
            Paint += ClientUI_Paint;
            ResumeLayout(false);
        }

        #endregion
        private Button StartButton;
        private Button StopButton;
        private ToolTip toolTip1;
        private Button AddServer;
        private FlowLayoutPanel flowLayoutPanelServers;
        private Button SavePreferences;
    }
}
