namespace SteamAchievmentViewer
{
    partial class Settings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            groupBox1 = new GroupBox();
            steamAcctIDTxtBox = new TextBox();
            steamWebTxtBox = new TextBox();
            label2 = new Label();
            groupBox2 = new GroupBox();
            button1 = new Button();
            emuDeckRomsPathTextBox = new TextBox();
            label5 = new Label();
            raUsernameTxtBox = new TextBox();
            raAPITxtBox = new TextBox();
            label3 = new Label();
            label4 = new Label();
            savebtn = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(22, 22);
            label1.Name = "label1";
            label1.Size = new Size(74, 15);
            label1.TabIndex = 0;
            label1.Text = "Web API Key";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(steamAcctIDTxtBox);
            groupBox1.Controls.Add(steamWebTxtBox);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new Point(12, 21);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(582, 96);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Steam";
            // 
            // steamAcctIDTxtBox
            // 
            steamAcctIDTxtBox.Location = new Point(134, 47);
            steamAcctIDTxtBox.Name = "steamAcctIDTxtBox";
            steamAcctIDTxtBox.Size = new Size(360, 23);
            steamAcctIDTxtBox.TabIndex = 3;
            // 
            // steamWebTxtBox
            // 
            steamWebTxtBox.Location = new Point(134, 16);
            steamWebTxtBox.Name = "steamWebTxtBox";
            steamWebTxtBox.Size = new Size(360, 23);
            steamWebTxtBox.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(28, 54);
            label2.Name = "label2";
            label2.Size = new Size(66, 15);
            label2.TabIndex = 1;
            label2.Text = "Account ID";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(button1);
            groupBox2.Controls.Add(emuDeckRomsPathTextBox);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(raUsernameTxtBox);
            groupBox2.Controls.Add(raAPITxtBox);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(label4);
            groupBox2.Location = new Point(12, 140);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(582, 129);
            groupBox2.TabIndex = 4;
            groupBox2.TabStop = false;
            groupBox2.Text = "Retro Achievements";
            // 
            // button1
            // 
            button1.Location = new Point(498, 87);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 6;
            button1.Text = " Browse ...";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // emuDeckRomsPathTextBox
            // 
            emuDeckRomsPathTextBox.Location = new Point(134, 87);
            emuDeckRomsPathTextBox.Name = "emuDeckRomsPathTextBox";
            emuDeckRomsPathTextBox.Size = new Size(360, 23);
            emuDeckRomsPathTextBox.TabIndex = 5;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(11, 91);
            label5.Name = "label5";
            label5.Size = new Size(113, 15);
            label5.TabIndex = 4;
            label5.Text = "Emudeck roms Path";
            // 
            // raUsernameTxtBox
            // 
            raUsernameTxtBox.Location = new Point(134, 50);
            raUsernameTxtBox.Name = "raUsernameTxtBox";
            raUsernameTxtBox.Size = new Size(360, 23);
            raUsernameTxtBox.TabIndex = 3;
            // 
            // raAPITxtBox
            // 
            raAPITxtBox.Location = new Point(134, 19);
            raAPITxtBox.Name = "raAPITxtBox";
            raAPITxtBox.Size = new Size(360, 23);
            raAPITxtBox.TabIndex = 2;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(33, 54);
            label3.Name = "label3";
            label3.Size = new Size(60, 15);
            label3.TabIndex = 1;
            label3.Text = "Username";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(38, 23);
            label4.Name = "label4";
            label4.Size = new Size(47, 15);
            label4.TabIndex = 0;
            label4.Text = "API Key";
            // 
            // savebtn
            // 
            savebtn.Location = new Point(12, 275);
            savebtn.Name = "savebtn";
            savebtn.Size = new Size(582, 48);
            savebtn.TabIndex = 5;
            savebtn.Text = "Save Settings";
            savebtn.UseVisualStyleBackColor = true;
            savebtn.Click += savebtn_Click;
            // 
            // Settings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(614, 345);
            Controls.Add(savebtn);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            MinimumSize = new Size(630, 384);
            Name = "Settings";
            Text = "Settings";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private GroupBox groupBox1;
        private TextBox steamAcctIDTxtBox;
        private TextBox steamWebTxtBox;
        private Label label2;
        private GroupBox groupBox2;
        private TextBox raUsernameTxtBox;
        private TextBox raAPITxtBox;
        private Label label3;
        private Label label4;
        private Button button1;
        private TextBox emuDeckRomsPathTextBox;
        private Label label5;
        private Button savebtn;
    }
}