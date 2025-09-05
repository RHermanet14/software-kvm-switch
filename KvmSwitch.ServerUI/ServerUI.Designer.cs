namespace ServerUI
{
    partial class ServerUI
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
            PortLabel = new Label();
            PortTextBox = new TextBox();
            StartButton = new Button();
            StopButton = new Button();
            IPLabel = new Label();
            ToggleButton = new Button();
            SaveButton = new Button();
            toolTip1 = new ToolTip(components);
            SuspendLayout();
            // 
            // PortLabel
            // 
            PortLabel.AutoSize = true;
            PortLabel.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            PortLabel.Location = new Point(252, 14);
            PortLabel.Name = "PortLabel";
            PortLabel.Size = new Size(73, 20);
            PortLabel.TabIndex = 0;
            PortLabel.Text = "Enter Port";
            // 
            // PortTextBox
            // 
            PortTextBox.Location = new Point(331, 11);
            PortTextBox.Name = "PortTextBox";
            PortTextBox.Size = new Size(84, 23);
            PortTextBox.TabIndex = 1;
            PortTextBox.KeyPress += PortTextBox_KeyPress;
            // 
            // StartButton
            // 
            StartButton.Location = new Point(259, 103);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(75, 23);
            StartButton.TabIndex = 2;
            StartButton.Text = "Start Server";
            StartButton.UseVisualStyleBackColor = true;
            StartButton.Click += StartButton_Click;
            // 
            // StopButton
            // 
            StopButton.Location = new Point(340, 103);
            StopButton.Name = "StopButton";
            StopButton.Size = new Size(75, 23);
            StopButton.TabIndex = 3;
            StopButton.Text = "Stop Server";
            StopButton.UseVisualStyleBackColor = true;
            StopButton.Click += StopButton_Click;
            // 
            // IPLabel
            // 
            IPLabel.AutoSize = true;
            IPLabel.Font = new Font("Segoe UI", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            IPLabel.Location = new Point(39, 38);
            IPLabel.Name = "IPLabel";
            IPLabel.Size = new Size(155, 37);
            IPLabel.TabIndex = 4;
            IPLabel.Text = "192.168.1.1";
            IPLabel.Click += IPLabel_Click;
            IPLabel.MouseMove += IPLabel_MouseMove;
            // 
            // ToggleButton
            // 
            ToggleButton.Location = new Point(12, 11);
            ToggleButton.Name = "ToggleButton";
            ToggleButton.Size = new Size(218, 24);
            ToggleButton.TabIndex = 5;
            ToggleButton.Text = "Toggle Display IP";
            ToggleButton.UseVisualStyleBackColor = true;
            ToggleButton.Click += ToggleButton_Click;
            // 
            // SaveButton
            // 
            SaveButton.Location = new Point(331, 40);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new Size(84, 23);
            SaveButton.TabIndex = 6;
            SaveButton.Text = "Save Port";
            SaveButton.UseVisualStyleBackColor = true;
            SaveButton.Click += SaveButton_Click;
            // 
            // ServerUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(427, 138);
            Controls.Add(SaveButton);
            Controls.Add(ToggleButton);
            Controls.Add(IPLabel);
            Controls.Add(StopButton);
            Controls.Add(StartButton);
            Controls.Add(PortTextBox);
            Controls.Add(PortLabel);
            Name = "ServerUI";
            Text = "Server Interface";
            Load += ServerUI_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label PortLabel;
        private TextBox PortTextBox;
        private Button StartButton;
        private Button StopButton;
        private Label IPLabel;
        private Button ToggleButton;
        private Button SaveButton;
        private ToolTip toolTip1;
    }
}
