namespace SteamAchievmentViewer
{
    partial class GroupAchievments
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
            unmappedListBox = new CheckedListBox();
            mappedTreeView = new TreeView();
            addBtn = new Button();
            addNameTextBox = new TextBox();
            deleteBtn = new Button();
            label1 = new Label();
            label2 = new Label();
            saveAchievementBtn = new Button();
            selectAllBtn = new Button();
            selectNoneBtn = new Button();
            SuspendLayout();
            // 
            // unmappedListBox
            // 
            unmappedListBox.CheckOnClick = true;
            unmappedListBox.FormattingEnabled = true;
            unmappedListBox.Location = new Point(12, 97);
            unmappedListBox.Name = "unmappedListBox";
            unmappedListBox.Size = new Size(363, 382);
            unmappedListBox.TabIndex = 0;
            // 
            // mappedTreeView
            // 
            mappedTreeView.CheckBoxes = true;
            mappedTreeView.Location = new Point(381, 97);
            mappedTreeView.Name = "mappedTreeView";
            mappedTreeView.Size = new Size(589, 382);
            mappedTreeView.TabIndex = 1;
            mappedTreeView.AfterCheck += mappedTreeView_AfterCheck;
            mappedTreeView.AfterSelect += mappedTreeView_AfterSelect;
            // 
            // addBtn
            // 
            addBtn.Location = new Point(266, 32);
            addBtn.Name = "addBtn";
            addBtn.Size = new Size(110, 26);
            addBtn.TabIndex = 2;
            addBtn.Text = "Add Mapping";
            addBtn.UseVisualStyleBackColor = true;
            addBtn.Click += addBtn_Click;
            // 
            // addNameTextBox
            // 
            addNameTextBox.Location = new Point(12, 34);
            addNameTextBox.Name = "addNameTextBox";
            addNameTextBox.PlaceholderText = "Mapping Category Name ...";
            addNameTextBox.Size = new Size(248, 23);
            addNameTextBox.TabIndex = 3;
            // 
            // deleteBtn
            // 
            deleteBtn.Location = new Point(733, 64);
            deleteBtn.Name = "deleteBtn";
            deleteBtn.Size = new Size(237, 25);
            deleteBtn.TabIndex = 4;
            deleteBtn.Text = "Delete Category/Achievement Mapping";
            deleteBtn.UseVisualStyleBackColor = true;
            deleteBtn.Click += deleteBtn_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(54, 6);
            label1.Name = "label1";
            label1.Size = new Size(238, 25);
            label1.TabIndex = 5;
            label1.Text = "Unmapped Achievements";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            label2.Location = new Point(557, 5);
            label2.Name = "label2";
            label2.Size = new Size(171, 25);
            label2.TabIndex = 6;
            label2.Text = "Achievement Map";
            // 
            // saveAchievementBtn
            // 
            saveAchievementBtn.Location = new Point(575, 64);
            saveAchievementBtn.Name = "saveAchievementBtn";
            saveAchievementBtn.Size = new Size(153, 25);
            saveAchievementBtn.TabIndex = 7;
            saveAchievementBtn.Text = "Save Achievment Map";
            saveAchievementBtn.UseVisualStyleBackColor = true;
            saveAchievementBtn.Click += saveAchievementBtn_Click;
            // 
            // selectAllBtn
            // 
            selectAllBtn.Location = new Point(12, 65);
            selectAllBtn.Name = "selectAllBtn";
            selectAllBtn.Size = new Size(170, 26);
            selectAllBtn.TabIndex = 8;
            selectAllBtn.Text = "Select All";
            selectAllBtn.UseVisualStyleBackColor = true;
            selectAllBtn.Click += selectAllBtn_Click;
            // 
            // selectNoneBtn
            // 
            selectNoneBtn.Location = new Point(188, 65);
            selectNoneBtn.Name = "selectNoneBtn";
            selectNoneBtn.Size = new Size(187, 26);
            selectNoneBtn.TabIndex = 9;
            selectNoneBtn.Text = "Select None";
            selectNoneBtn.UseVisualStyleBackColor = true;
            selectNoneBtn.Click += selectNoneBtn_Click;
            // 
            // GroupAchievments
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(982, 500);
            Controls.Add(selectNoneBtn);
            Controls.Add(selectAllBtn);
            Controls.Add(saveAchievementBtn);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(deleteBtn);
            Controls.Add(addNameTextBox);
            Controls.Add(addBtn);
            Controls.Add(mappedTreeView);
            Controls.Add(unmappedListBox);
            Name = "GroupAchievments";
            Text = "GroupAchievments";
            Shown += GroupAchievments_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckedListBox unmappedListBox;
        private TreeView mappedTreeView;
        private Button addBtn;
        private TextBox addNameTextBox;
        private Button deleteBtn;
        private Label label1;
        private Label label2;
        private Button saveAchievementBtn;
        private Button selectAllBtn;
        private Button selectNoneBtn;
    }
}