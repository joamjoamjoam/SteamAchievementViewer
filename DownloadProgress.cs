using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamAchievmentViewer
{
    public partial class DownloadProgress : Form
    {
        String url = "";
        String path = "";
        public Action downloadComplete = null;

        public DownloadProgress(String url, String savePath)
        {
            InitializeComponent();
            romNameLabel.Text = $"Downloading: {Path.GetFileName(savePath)}";


            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 1;

            this.url = url;
            this.path = savePath;

            this.BackColor = Color.FromArgb(80, 84, 82);
            this.ForeColor = Color.White;
        }


        public void downloadCompleted(object? sender, AsyncCompletedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                progressBar1.Value = 100;
            });
            // Fire Completed Event
            if (downloadComplete != null)
            {
                downloadComplete();
            }

            MessageBox.Show($"{((e.Error == null) ? $"Rom Saved to {path}" : "Failed to download Rom")}", $"Rom Download {((e.Error == null) ? "Success" : "Failed")}");

            this.Invoke((MethodInvoker)delegate
            {
                this.Close();
            });
        }

        public void downloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                progressBar1.Value = e.ProgressPercentage;
                bytesLbl.Text = $"{Math.Round((double)e.BytesReceived / (double)1000000,1)} / {Math.Round((double)e.TotalBytesToReceive / (double)1000000, 1)} MB";
            });

        }

        public void downloadFile(String url, String path)
        {
            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadProgressChanged += downloadProgressChanged;
                    client.DownloadFileCompleted += downloadCompleted;
                    client.DownloadFileAsync(new Uri(url), path);
                }
                catch (Exception ex)
                {
                }


            }
        }

        private void DownloadProgress_Shown(object sender, EventArgs e)
        {
            downloadFile(url, path);
        }
    }
}
