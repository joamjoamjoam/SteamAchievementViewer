﻿namespace SteamAchievmentViewer
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
            ((System.ComponentModel.ISupportInitialize)achWebView).BeginInit();
            SuspendLayout();
            // 
            // gamesListbox
            // 
            gamesListbox.FormattingEnabled = true;
            gamesListbox.ItemHeight = 15;
            gamesListbox.Location = new Point(12, 47);
            gamesListbox.Name = "gamesListbox";
            gamesListbox.Size = new Size(280, 754);
            gamesListbox.TabIndex = 2;
            gamesListbox.SelectedIndexChanged += gamesListbox_SelectedIndexChanged;
            // 
            // achWebView
            // 
            achWebView.AllowExternalDrop = true;
            achWebView.CreationProperties = null;
            achWebView.DefaultBackgroundColor = Color.White;
            achWebView.Location = new Point(298, 47);
            achWebView.Name = "achWebView";
            achWebView.Size = new Size(821, 754);
            achWebView.TabIndex = 3;
            achWebView.ZoomFactor = 1D;
            // 
            // sortByComboBox
            // 
            sortByComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            sortByComboBox.FlatStyle = FlatStyle.Flat;
            sortByComboBox.FormattingEnabled = true;
            sortByComboBox.Items.AddRange(new object[] { "Game Order", "Name A-Z", "Name Z-A", "Unlocked First", "Locked First" });
            sortByComboBox.Location = new Point(909, 12);
            sortByComboBox.Name = "sortByComboBox";
            sortByComboBox.Size = new Size(210, 23);
            sortByComboBox.TabIndex = 4;
            sortByComboBox.SelectedIndexChanged += sortByComboBox_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(856, 15);
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
            label3.Location = new Point(507, 12);
            label3.Name = "label3";
            label3.Size = new Size(134, 25);
            label3.TabIndex = 8;
            label3.Text = "Acheivements";
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(753, 15);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(97, 19);
            checkBox1.TabIndex = 9;
            checkBox1.Text = "Show Hidden";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1131, 815);
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
    }
}