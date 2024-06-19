using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Text.RegularExpressions;

namespace SteamAchievmentViewer
{
    public partial class Form1 : Form
    {
        public Steam steamClient = null;
        public List<SteamGame> games = new List<SteamGame>();
        public SteamGame selectedGame = null;
        public String htmlPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\index.html";
        public String introPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\intro.html";
        public String hiddenImgPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\locked.png";
        public String configPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\config\\config.json";
        public String divStatePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\divStates.json";
        private String steamWebKey = "";
        private ulong steamAcctId = 0;
        private JObject divStateModel;

        public Form1()
        {
            InitializeComponent();
            InitializeAsync();

            try
            {
                JObject configObj = JObject.Parse(File.ReadAllText(configPath));
                steamWebKey = configObj["steamWebKey"] == null ? "" : (String)configObj["steamWebKey"];
                steamAcctId = (ulong)configObj["steamAcctID"];

                if (steamWebKey.Length == 0 || steamAcctId == 0)
                {
                    throw new Exception();
                }
                else
                {
                    steamClient = new Steam(steamAcctId, steamWebKey);
                }
                groupAchievmentsBtn.Visible = false;
            }
            catch
            {
                MessageBox.Show("Please load your Steam Web Key and Account ID", "Configuration Needed");
            }

            if (File.Exists(divStatePath))
            {
                divStateModel = JObject.Parse(File.ReadAllText(divStatePath));
            }
            else
            {
                divStateModel = new JObject();
            }


            sortByComboBox.SelectedIndex = 0;
            checkBox1.Checked = true;

            // Generate Intro HTML

            File.WriteAllText(introPath, $"<head><style>body {{background-color: rgba(120, 120, 120, 1); color: #eee;}}</style></head><body>{((steamClient == null) ? "<h1>Configuration Needed!!</h1> <h3>Load your Steam Web Key and Steam Account ID (SteamID) in config/config.json</h3><p>Get Your Steam Web Key <a href=\"https://steamcommunity.com/dev/apikey\">HERE</a></p><p>Get Your SteamAccount ID (SteamID) <a href=\"https://steamdb.info/calculator/\">HERE</a></p>" : "<h1>Select Game From Game List</h1>")}</body>");
            achWebView.Source = new Uri(introPath);
            Properties.Resources.locked.Save(hiddenImgPath, System.Drawing.Imaging.ImageFormat.Png);

            this.BackColor = Color.FromArgb(80, 84, 82);
            this.ForeColor = Color.White;

            gamesListbox.BackColor = Color.FromArgb(120, 120, 120);
            gamesListbox.ForeColor = Color.White;

            groupAchievmentsBtn.BackColor = Color.FromArgb(120, 120, 120);
            groupAchievmentsBtn.ForeColor = Color.White;

            sortByComboBox.BackColor = Color.FromArgb(120, 120, 120);
            sortByComboBox.ForeColor = Color.White;

            updateMapsBtn.BackColor = Color.FromArgb(120, 120, 120);
            updateMapsBtn.ForeColor = Color.White;

        }

        async void InitializeAsync()
        {
            await achWebView.EnsureCoreWebView2Async(null);
            achWebView.CoreWebView2.WebMessageReceived += MessageReceived;
        }

        void MessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            String content = args.TryGetWebMessageAsString();
            try
            {
                if (content == "reload")
                {
                    reloadFrame();
                }
                else if (content.StartsWith("expanded-"))
                {

                    if (selectedGame != null)
                    {
                        String key = $"{selectedGame.id}-{content.Split(new char[] { '-' })[1]}";
                        key = Regex.Replace(key, "(Header|Div)$", "");
                        //MessageBox.Show($"{key} was Expanded");
                        if (divStateModel.ContainsKey(key))
                        {
                            divStateModel[key] = "expanded";
                        }
                        else
                        {
                            divStateModel.Add(key, "expanded");
                        }
                        File.WriteAllText(divStatePath, divStateModel.ToString());
                    }
                }
                else if (content.StartsWith("collapsed-"))
                {
                    if (selectedGame != null)
                    {
                        String key = $"{selectedGame.id}-{content.Split(new char[] { '-' })[1]}";
                        key = Regex.Replace(key, "(Header|Div)$", "");
                        //MessageBox.Show($"{key} was Collapsed");
                        if (divStateModel.ContainsKey(key))
                        {
                            divStateModel[key] = "collapsed";
                        }
                        else
                        {
                            divStateModel.Add(key, "collapsed");
                        }
                        File.WriteAllText(divStatePath, divStateModel.ToString());
                    }
                }
            }
            catch (Exception ex)
            {

            }


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (steamClient != null)
            {
                steamClient.fetchSteamGames();
                gamesListbox.Items.Clear();
                List<SteamGame> gameList = steamClient.getGames().OrderBy(g => g.gameName).ToList();
                foreach (SteamGame g in gameList)
                {
                    gamesListbox.Items.Add(g);
                }
            }
        }

        private void reloadFrame()
        {
            if (selectedGame != null && steamClient != null)
            {
                steamClient.saveHTMLForGame(selectedGame.id, htmlPath, (AchievementSortOrder)sortByComboBox.SelectedIndex, showHidden: checkBox1.Checked, divStates: divStateModel);
                offlineModeCB.Checked = !steamClient.isOnline;
                achWebView.Source = new Uri(introPath);
                achWebView.Source = new Uri(htmlPath);
            }
        }

        private void gamesListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SteamGame g = (SteamGame)((ListBox)sender).SelectedItem;
            selectedGame = g;
            reloadFrame();
            groupAchievmentsBtn.Visible = true;
        }

        private void sortByComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            reloadFrame();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            reloadFrame();
        }

        private void offlineModeCB_CheckedChanged(object sender, EventArgs e)
        {
            steamClient.isOnline = !((CheckBox)sender).Checked;
        }

        private void groupAchievmentsBtn_Click(object sender, EventArgs e)
        {
            if (selectedGame != null)
            {
                GroupAchievments tmpForm = new GroupAchievments(steamClient, selectedGame.id);
                tmpForm.ShowDialog();
                reloadFrame();
            }
        }

        private Boolean downloadGitRepo(string path)
        {
            Boolean rv = false;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://github.com/joamjoamjoam/SteamAchievementViewer/archive/main.zip");

            request.Method = "GET";
            String tmpZipPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"gitPull.zip";

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    if (File.Exists(tmpZipPath))
                    {
                        File.Delete(tmpZipPath);
                    }

                    MemoryStream memoryStream = new MemoryStream();
                    response.GetResponseStream().CopyTo(memoryStream);
                    byte[] responseArr = memoryStream.ToArray();

                    File.WriteAllBytes(tmpZipPath, responseArr);
                    System.IO.Compression.ZipFile.ExtractToDirectory(tmpZipPath, path);

                    rv = true;
                    if (File.Exists(tmpZipPath))
                    {
                        File.Delete(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"gitPull.zip");
                    }
                }
            }
            catch
            {
                try
                {
                    if (File.Exists(tmpZipPath))
                    {
                        File.Delete(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"gitPull.zip");
                    }
                }
                catch
                {

                }
                rv = false;
            }


            return rv;
        }

        private void updateMapsBtn_Click(object sender, EventArgs e)
        {
            String clonePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\\steamachievementviewer";
            String cloneMapsPath = clonePath + @"\\SteamAchievementViewer-main\\achievementMaps";
            if (Directory.Exists(clonePath))
            {
                Directory.Delete(clonePath, true);
            }
            try
            {
                downloadGitRepo(clonePath);
                if (Directory.Exists(cloneMapsPath))
                {
                    foreach (string fileName in Directory.GetFiles(cloneMapsPath, "*.json"))
                    {
                        File.Copy(fileName, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\achievementMaps\\" + Path.GetFileName(fileName), true);
                    }
                    MessageBox.Show("Achievement Maps Updated Successfully");
                    Directory.Delete(clonePath, true);
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                MessageBox.Show("Error updating Achievement Maps from Git Repository:\nhttps://github.com/joamjoamjoam/SteamAchievementViewer");
            }

        }
    }
}