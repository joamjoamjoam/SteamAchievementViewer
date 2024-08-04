namespace SteamAchievmentViewer
{
    partial class DownloadProgress
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
            progressBar1 = new ProgressBar();
            romNameLabel = new Label();
            bytesLbl = new Label();
            SuspendLayout();
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(35, 95);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(791, 72);
            progressBar1.TabIndex = 0;
            // 
            // romNameLabel
            // 
            romNameLabel.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            romNameLabel.Location = new Point(35, 22);
            romNameLabel.Name = "romNameLabel";
            romNameLabel.Size = new Size(791, 40);
            romNameLabel.TabIndex = 1;
            romNameLabel.Text = "Downloading: RomName";
            romNameLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // bytesLbl
            // 
            bytesLbl.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            bytesLbl.Location = new Point(35, 62);
            bytesLbl.Name = "bytesLbl";
            bytesLbl.Size = new Size(791, 30);
            bytesLbl.TabIndex = 2;
            bytesLbl.Text = "0 / ? MB";
            bytesLbl.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // DownloadProgress
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(857, 180);
            Controls.Add(bytesLbl);
            Controls.Add(romNameLabel);
            Controls.Add(progressBar1);
            MaximumSize = new Size(873, 219);
            MinimumSize = new Size(873, 219);
            Name = "DownloadProgress";
            StartPosition = FormStartPosition.CenterParent;
            Text = "DownloadProgress";
            Shown += DownloadProgress_Shown;
            ResumeLayout(false);
        }

        #endregion

        private ProgressBar progressBar1;
        private Label romNameLabel;
        private Label label1;
        private Label bytesLbl;
    }
}