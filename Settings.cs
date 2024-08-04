using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SteamAchievmentViewer
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();

            steamAcctIDTxtBox.Text = Properties.Settings1.Default.steamAcctID.ToString();
            steamWebTxtBox.Text = Properties.Settings1.Default.steamWebAPIKey;

            raUsernameTxtBox.Text = Properties.Settings1.Default.RAUsername;
            raAPITxtBox.Text = Properties.Settings1.Default.RAAPIKey;
            emuDeckRomsPathTextBox.Text = Properties.Settings1.Default.emuDeckRomsPath;

            this.BackColor = Color.FromArgb(80, 84, 82);
            this.ForeColor = Color.White;

            foreach (Control c in Controls)
            {
                if (c.GetType() == typeof(Button) || c.GetType() == typeof(TextBox))
                {
                    colorControl(c);
                }
                else if (c.GetType() == typeof(GroupBox))
                {
                    colorControl(c);
                    foreach (Control cc in c.Controls)
                    {
                        colorControl(cc);
                    }
                }
            }
        }

        public void colorControl(Control c)
        {
            if (c.GetType() == typeof(TextBox))
            {
                c.BackColor = Color.FromArgb(120, 120, 120);
                c.ForeColor = Color.White;
            }
            else if (c.GetType() == typeof(Button))
            {
                c.BackColor = Color.FromArgb(120, 120, 120);
                c.ForeColor = Color.White;
            }
            else if (c.GetType() == typeof(GroupBox))
            {
                c.BackColor = Color.FromArgb(80, 84, 82);
                c.ForeColor = Color.White;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog ofd = new FolderBrowserDialog();
            ofd.Description = "Select EmuDeck 'roms' folder";

            if (ofd.ShowDialog() == DialogResult.OK && Directory.Exists(ofd.SelectedPath) && Directory.Exists(ofd.SelectedPath + "\\snes"))
            {
                emuDeckRomsPathTextBox.Text = ofd.SelectedPath;
            }
            else
            {
                MessageBox.Show("Please select the Emudeck 'roms' Folder.", "Incorrect Folder Selected");
            }

        }

        private void savebtn_Click(object sender, EventArgs e)
        {
            try
            {
                Properties.Settings1.Default.steamAcctID = ulong.Parse(steamAcctIDTxtBox.Text);
            }
            catch
            {

            }

            Properties.Settings1.Default.steamWebAPIKey = steamWebTxtBox.Text;
            Properties.Settings1.Default.RAAPIKey = raAPITxtBox.Text;
            Properties.Settings1.Default.RAUsername = raUsernameTxtBox.Text;
            Properties.Settings1.Default.emuDeckRomsPath = emuDeckRomsPathTextBox.Text;

            Properties.Settings1.Default.Save();

            Close();

        }
    }
}
