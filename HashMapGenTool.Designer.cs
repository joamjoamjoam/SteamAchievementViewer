namespace SteamAchievmentViewer
{
    partial class HashMapGenTool
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
            displayTxtBox = new RichTextBox();
            progressBar = new ProgressBar();
            SuspendLayout();
            // 
            // displayTxtBox
            // 
            displayTxtBox.Location = new Point(3, 74);
            displayTxtBox.Name = "displayTxtBox";
            displayTxtBox.Size = new Size(797, 374);
            displayTxtBox.TabIndex = 0;
            displayTxtBox.Text = "";
            // 
            // progressBar
            // 
            progressBar.Location = new Point(12, 12);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(776, 46);
            progressBar.TabIndex = 1;
            // 
            // HashMapGenTool
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(progressBar);
            Controls.Add(displayTxtBox);
            Name = "HashMapGenTool";
            Text = "HashMapGenTool";
            Shown += HashMapGenTool_Shown;
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox displayTxtBox;
        private ProgressBar progressBar;
    }
}