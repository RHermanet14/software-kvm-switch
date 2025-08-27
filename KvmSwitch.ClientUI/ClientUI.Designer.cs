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
            textBox1 = new TextBox();
            label1 = new Label();
            button1 = new Button();
            checkBox1 = new CheckBox();
            label2 = new Label();
            textBox2 = new TextBox();
            checkBox2 = new CheckBox();
            button2 = new Button();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox1.Location = new Point(141, 44);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(187, 29);
            textBox1.TabIndex = 0;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(12, 47);
            label1.Name = "label1";
            label1.Size = new Size(123, 21);
            label1.TabIndex = 1;
            label1.Text = "Enter IP Address";
            label1.Click += label1_Click;
            // 
            // button1
            // 
            button1.Location = new Point(236, 190);
            button1.Name = "button1";
            button1.Size = new Size(92, 23);
            button1.TabIndex = 2;
            button1.Text = "Start Client";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(141, 79);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(63, 19);
            checkBox1.TabIndex = 3;
            checkBox1.Text = "Save IP";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F);
            label2.Location = new Point(12, 112);
            label2.Name = "label2";
            label2.Size = new Size(78, 21);
            label2.TabIndex = 4;
            label2.Text = "Enter Port";
            label2.Click += label2_Click;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(141, 114);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(187, 23);
            textBox2.TabIndex = 5;
            textBox2.TextChanged += textBox2_TextChanged;
            // 
            // checkBox2
            // 
            checkBox2.AutoSize = true;
            checkBox2.Location = new Point(141, 143);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new Size(75, 19);
            checkBox2.TabIndex = 6;
            checkBox2.Text = "Save Port";
            checkBox2.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(236, 232);
            button2.Name = "button2";
            button2.Size = new Size(92, 23);
            button2.TabIndex = 7;
            button2.Text = "Stop Client";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // ClientUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(352, 316);
            Controls.Add(button2);
            Controls.Add(checkBox2);
            Controls.Add(textBox2);
            Controls.Add(label2);
            Controls.Add(checkBox1);
            Controls.Add(button1);
            Controls.Add(label1);
            Controls.Add(textBox1);
            Name = "ClientUI";
            Text = "Client Interface";
            Load += ClientUI_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private Label label1;
        private Button button1;
        private CheckBox checkBox1;
        private Label label2;
        private TextBox textBox2;
        private CheckBox checkBox2;
        private Button button2;
    }
}
