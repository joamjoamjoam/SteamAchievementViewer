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
            achWebView = new Microsoft.Web.WebView2.WinForms.WebView2();
            sortByComboBox = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            checkBox1 = new CheckBox();
            offlineModeCB = new CheckBox();
            groupAchievmentsBtn = new Button();
            updateMapsBtn = new Button();
            gameListSelectionCmbBox = new ComboBox();
            backBtn = new Button();
            menuStrip1 = new MenuStrip();
            toolsToolStripMenuItem = new ToolStripMenuItem();
            setupEmudeckInstallToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            gamesListbox = new CheckedListBox();
            ((System.ComponentModel.ISupportInitialize)achWebView).BeginInit();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // achWebView
            // 
            achWebView.AllowExternalDrop = true;
            achWebView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            achWebView.CreationProperties = null;
            achWebView.DefaultBackgroundColor = Color.White;
            achWebView.Location = new Point(298, 92);
            achWebView.Name = "achWebView";
            achWebView.Size = new Size(863, 452);
            achWebView.TabIndex = 3;
            achWebView.ZoomFactor = 1D;
            // 
            // sortByComboBox
            // 
            sortByComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            sortByComboBox.FlatStyle = FlatStyle.Flat;
            sortByComboBox.FormattingEnabled = true;
            sortByComboBox.Items.AddRange(new object[] { "Game Order", "Name A-Z", "Name Z-A", "Unlocked First A-Z", "Locked First A-Z", "Unlocked First Game Order", "Locked First Game Order", "Show Req to Beat Game" });
            sortByComboBox.Location = new Point(943, 34);
            sortByComboBox.Name = "sortByComboBox";
            sortByComboBox.Size = new Size(210, 23);
            sortByComboBox.TabIndex = 4;
            sortByComboBox.SelectedIndexChanged += sortByComboBox_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(890, 37);
            label1.Name = "label1";
            label1.Size = new Size(47, 15);
            label1.TabIndex = 5;
            label1.Text = "Sort By:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(104, 32);
            label2.Name = "label2";
            label2.Size = new Size(71, 25);
            label2.TabIndex = 7;
            label2.Text = "Games";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            label3.Location = new Point(373, 30);
            label3.Name = "label3";
            label3.Size = new Size(134, 25);
            label3.TabIndex = 8;
            label3.Text = "Achievements";
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(787, 25);
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
            offlineModeCB.Location = new Point(787, 48);
            offlineModeCB.Name = "offlineModeCB";
            offlineModeCB.Size = new Size(96, 19);
            offlineModeCB.TabIndex = 10;
            offlineModeCB.Text = "Offline Mode";
            offlineModeCB.UseVisualStyleBackColor = true;
            offlineModeCB.CheckedChanged += offlineModeCB_CheckedChanged;
            // 
            // groupAchievmentsBtn
            // 
            groupAchievmentsBtn.Location = new Point(533, 25);
            groupAchievmentsBtn.Name = "groupAchievmentsBtn";
            groupAchievmentsBtn.Size = new Size(121, 38);
            groupAchievmentsBtn.TabIndex = 11;
            groupAchievmentsBtn.Text = "Group Achievements";
            groupAchievmentsBtn.UseVisualStyleBackColor = true;
            groupAchievmentsBtn.Click += groupAchievmentsBtn_Click;
            // 
            // updateMapsBtn
            // 
            updateMapsBtn.Location = new Point(660, 25);
            updateMapsBtn.Name = "updateMapsBtn";
            updateMapsBtn.Size = new Size(121, 38);
            updateMapsBtn.TabIndex = 12;
            updateMapsBtn.Text = "Update Achievement Maps";
            updateMapsBtn.UseVisualStyleBackColor = true;
            updateMapsBtn.Click += updateMapsBtn_Click;
            // 
            // gameListSelectionCmbBox
            // 
            gameListSelectionCmbBox.DropDownStyle = ComboBoxStyle.DropDownList;
            gameListSelectionCmbBox.FlatStyle = FlatStyle.Flat;
            gameListSelectionCmbBox.FormattingEnabled = true;
            gameListSelectionCmbBox.Location = new Point(13, 57);
            gameListSelectionCmbBox.Name = "gameListSelectionCmbBox";
            gameListSelectionCmbBox.Size = new Size(279, 23);
            gameListSelectionCmbBox.TabIndex = 13;
            gameListSelectionCmbBox.SelectedIndexChanged += gameListSelectionCmbBox_SelectedIndexChanged;
            // 
            // backBtn
            // 
            backBtn.Location = new Point(307, 57);
            backBtn.Name = "backBtn";
            backBtn.Size = new Size(75, 23);
            backBtn.TabIndex = 14;
            backBtn.Text = "Back";
            backBtn.UseVisualStyleBackColor = true;
            backBtn.Click += backBtn_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { toolsToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1173, 24);
            menuStrip1.TabIndex = 15;
            menuStrip1.Text = "menuStrip1";
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { setupEmudeckInstallToolStripMenuItem, settingsToolStripMenuItem });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new Size(46, 20);
            toolsToolStripMenuItem.Text = "Tools";
            // 
            // setupEmudeckInstallToolStripMenuItem
            // 
            setupEmudeckInstallToolStripMenuItem.Name = "setupEmudeckInstallToolStripMenuItem";
            setupEmudeckInstallToolStripMenuItem.Size = new Size(251, 22);
            setupEmudeckInstallToolStripMenuItem.Text = "Calculate RA Hashes for ED Install";
            setupEmudeckInstallToolStripMenuItem.Click += setupEmudeckInstallToolStripMenuItem_Click;
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(251, 22);
            settingsToolStripMenuItem.Text = "Settings...";
            settingsToolStripMenuItem.Click += settingsToolStripMenuItem_Click;
            // 
            // gamesListbox
            // 
            gamesListbox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            gamesListbox.FormattingEnabled = true;
            gamesListbox.HorizontalScrollbar = true;
            gamesListbox.Location = new Point(12, 90);
            gamesListbox.Name = "gamesListbox";
            gamesListbox.Size = new Size(280, 454);
            gamesListbox.TabIndex = 16;
            gamesListbox.ItemCheck += gamesListbox_ItemCheck;
            gamesListbox.SelectedIndexChanged += gamesListbox_SelectedIndexChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1173, 552);
            Controls.Add(gamesListbox);
            Controls.Add(backBtn);
            Controls.Add(gameListSelectionCmbBox);
            Controls.Add(updateMapsBtn);
            Controls.Add(groupAchievmentsBtn);
            Controls.Add(offlineModeCB);
            Controls.Add(checkBox1);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(sortByComboBox);
            Controls.Add(achWebView);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            MinimumSize = new Size(1189, 591);
            Name = "Form1";
            Text = "Steam Achievement Viewer";
            Load += Form1_Load;
            Shown += Form1_Shown;
            ((System.ComponentModel.ISupportInitialize)achWebView).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Microsoft.Web.WebView2.WinForms.WebView2 achWebView;
        private ComboBox sortByComboBox;
        private Label label1;
        private Label label2;
        private Label label3;
        private CheckBox checkBox1;
        private CheckBox offlineModeCB;
        private Button groupAchievmentsBtn;
        private Button updateMapsBtn;
        private ComboBox gameListSelectionCmbBox;
        private Button backBtn;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem setupEmudeckInstallToolStripMenuItem;
        private CheckedListBox gamesListbox;
        private ToolStripMenuItem settingsToolStripMenuItem;
    }
}