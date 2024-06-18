namespace SteamAchievmentViewer
{
    partial class Form1
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
            gamesListbox = new ListBox();
            achWebView = new Microsoft.Web.WebView2.WinForms.WebView2();
            sortByComboBox = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            checkBox1 = new CheckBox();
            offlineModeCB = new CheckBox();
            groupAchievmentsBtn = new Button();
            updateMapsBtn = new Button();
            ((System.ComponentModel.ISupportInitialize)achWebView).BeginInit();
            SuspendLayout();
            // 
            // gamesListbox
            // 
            gamesListbox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            gamesListbox.FormattingEnabled = true;
            gamesListbox.ItemHeight = 15;
            gamesListbox.Location = new Point(12, 47);
            gamesListbox.Name = "gamesListbox";
            gamesListbox.Size = new Size(280, 439);
            gamesListbox.TabIndex = 2;
            gamesListbox.SelectedIndexChanged += gamesListbox_SelectedIndexChanged;
            // 
            // achWebView
            // 
            achWebView.AllowExternalDrop = true;
            achWebView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            achWebView.CreationProperties = null;
            achWebView.DefaultBackgroundColor = Color.White;
            achWebView.Location = new Point(298, 47);
            achWebView.Name = "achWebView";
            achWebView.Size = new Size(863, 442);
            achWebView.TabIndex = 3;
            achWebView.ZoomFactor = 1D;
            // 
            // sortByComboBox
            // 
            sortByComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            sortByComboBox.FlatStyle = FlatStyle.Flat;
            sortByComboBox.FormattingEnabled = true;
            sortByComboBox.Items.AddRange(new object[] { "Game Order", "Name A-Z", "Name Z-A", "Unlocked First", "Locked First" });
            sortByComboBox.Location = new Point(943, 12);
            sortByComboBox.Name = "sortByComboBox";
            sortByComboBox.Size = new Size(210, 23);
            sortByComboBox.TabIndex = 4;
            sortByComboBox.SelectedIndexChanged += sortByComboBox_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(890, 15);
            label1.Name = "label1";
            label1.Size = new Size(47, 15);
            label1.TabIndex = 5;
            label1.Text = "Sort By:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(104, 12);
            label2.Name = "label2";
            label2.Size = new Size(71, 25);
            label2.TabIndex = 7;
            label2.Text = "Games";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            label3.Location = new Point(338, 12);
            label3.Name = "label3";
            label3.Size = new Size(134, 25);
            label3.TabIndex = 8;
            label3.Text = "Achievements";
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(787, 3);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(97, 19);
            checkBox1.TabIndex = 9;
            checkBox1.Text = "Show Hidden";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // offlineModeCB
            // 
            offlineModeCB.AutoSize = true;
            offlineModeCB.Location = new Point(787, 26);
            offlineModeCB.Name = "offlineModeCB";
            offlineModeCB.Size = new Size(96, 19);
            offlineModeCB.TabIndex = 10;
            offlineModeCB.Text = "Offline Mode";
            offlineModeCB.UseVisualStyleBackColor = true;
            offlineModeCB.CheckedChanged += offlineModeCB_CheckedChanged;
            // 
            // groupAchievmentsBtn
            // 
            groupAchievmentsBtn.Location = new Point(533, 3);
            groupAchievmentsBtn.Name = "groupAchievmentsBtn";
            groupAchievmentsBtn.Size = new Size(121, 38);
            groupAchievmentsBtn.TabIndex = 11;
            groupAchievmentsBtn.Text = "Group Achievements";
            groupAchievmentsBtn.UseVisualStyleBackColor = true;
            groupAchievmentsBtn.Click += groupAchievmentsBtn_Click;
            // 
            // updateMapsBtn
            // 
            updateMapsBtn.Location = new Point(660, 3);
            updateMapsBtn.Name = "updateMapsBtn";
            updateMapsBtn.Size = new Size(121, 38);
            updateMapsBtn.TabIndex = 12;
            updateMapsBtn.Text = "Update Achievement Maps";
            updateMapsBtn.UseVisualStyleBackColor = true;
            updateMapsBtn.Click += updateMapsBtn_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1173, 503);
            Controls.Add(updateMapsBtn);
            Controls.Add(groupAchievmentsBtn);
            Controls.Add(offlineModeCB);
            Controls.Add(checkBox1);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(sortByComboBox);
            Controls.Add(achWebView);
            Controls.Add(gamesListbox);
            Name = "Form1";
            Text = "Steam Achievement Viewer";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)achWebView).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ListBox gamesListbox;
        private Microsoft.Web.WebView2.WinForms.WebView2 achWebView;
        private ComboBox sortByComboBox;
        private Label label1;
        private Label label2;
        private Label label3;
        private CheckBox checkBox1;
        private CheckBox offlineModeCB;
        private Button groupAchievmentsBtn;
        private Button updateMapsBtn;
    }
}