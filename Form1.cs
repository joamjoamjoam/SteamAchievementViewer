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
        public delegate void ReloadGameListDelegate();




        public Form1()
        {
            InitializeComponent();
            InitializeAsync();

            try
            {
                JObject configObj = JObject.Parse(File.ReadAllText(configPath));
                steamWebKey = configObj["steamWebKey"] == null ? "" : (String)configObj["steamWebKey"];
                steamAcctId = (ulong)configObj["steamAcctID"];

                // load Settings 
                if (sortByComboBox.Items.Count >= Properties.Settings1.Default.sortComboBox)
                {
                    sortByComboBox.SelectedIndex = Properties.Settings1.Default.sortComboBox;
                }
                else
                {
                    sortByComboBox.SelectedIndex = 5;
                }

                checkBox1.Checked = Properties.Settings1.Default.showHiddenTrophiesSet;
                offlineModeCB.Checked = Properties.Settings1.Default.offlineModeSet;

                if (steamWebKey.Length == 0 || steamAcctId == 0)
                {
                    throw new Exception();
                }
                else
                {
                    steamClient = new Steam(steamAcctId, steamWebKey, startOffline: offlineModeCB.Checked);
                    steamClient.OnSteamAppListLoaded += OnSteamLoadedAppList;
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

        private void showIntroFrame(String html)
        {
            File.WriteAllText(introPath, html);
            if (achWebView.Source.AbsoluteUri == new Uri(introPath).AbsoluteUri)
            {
                achWebView.Reload();
            }
            else
            {
                achWebView.Source = new Uri(introPath);
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
            ComboBox comboBox = (ComboBox)sender;
            Properties.Settings1.Default.sortComboBox = comboBox.SelectedIndex;
            Properties.Settings1.Default.Save();
            reloadFrame();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            Properties.Settings1.Default.showHiddenTrophiesSet = cb.Checked;
            Properties.Settings1.Default.Save();
            reloadFrame();
        }

        private void offlineModeCB_CheckedChanged(object sender, EventArgs e)
        {

            CheckBox cb = (CheckBox)sender;
            Properties.Settings1.Default.offlineModeSet = cb.Checked;
            Properties.Settings1.Default.Save();

            if (steamClient != null)
            {
                steamClient.isOnline = !cb.Checked;
            }
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

        public void reloadGameList()
        {

            if (steamClient != null)
            {
                if (steamClient.getAppList().Keys.Count == 0 && steamClient.webKey != "")
                {
                    MessageBox.Show("App List Cache is outdated or Steam Credentials are invalid. Please Connect to the internet to download the new cache.", "");
                }
                else
                {

                    gamesListbox.Items.Clear();
                    List<SteamGame> gameList = steamClient.getGames().OrderBy(g => g.gameName).ToList();
                    foreach (SteamGame g in gameList)
                    {
                        gamesListbox.Items.Add(g);
                    }
                }
            }
            showIntroFrame($"<head><style>body {{background-color: rgba(120, 120, 120, 1); color: #eee;}}</style></head><body>{((gamesListbox.Items.Count > 0) ? "<h1>Select Game From Game List</h1>" : "<h1>Error Loading Game Data From Steam and Local Cache</h1>")}</body>");
        }

        private void OnSteamLoadedAppList(object sender, EventArgs e)
        {
            InvokeReloadGameList();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            // Generate Intro HTML
            String progressHTML = "<head><style>body {background-color: rgba(120, 120, 120, 1); color: #eee;} #myProgress { width: 100%; background-color: grey; } #myBar { width: 1%; height: 30px; background-color: #04AA6D; text-align: center; line-height: 30px; font-weight: bold; color: white; }</style><script>var i = 0; function move() { if (i == 0) { i = 1; var elem = document.getElementById('myBar'); var width = 1; var id = setInterval(frame, 500); function frame() { incr = Math.floor(Math.random() * 10); cont = true; if (width >= 100) { cont = false; width = 100; } else { width+= incr; if(width > 95){ \twidth = 95; } elem.style.width = width + '%'; elem.innerHTML = width + '%'; } clearInterval(id); if(cont){ \tid = setInterval(frame, incr * 400); } } } }</script></head><body onload=\"move()\"><h1 style=\"text-align: center;\">Loading Applist from Steam ...</h1><div id=\"myProgress\"> <div id=\"myBar\">1%</div> </div></body>";
            String configHtml = $"<head><style>body {{background-color: rgba(120, 120, 120, 1); color: #eee;}}</style></head><body><h1>Configuration Needed!!</h1> <h3>Load your Steam Web Key and Steam Account ID (SteamID) in config/config.json</h3><p>Get Your Steam Web Key <a href=\"https://steamcommunity.com/dev/apikey\">HERE</a></p><p>Get Your SteamAccount ID (SteamID) <a href=\"https://steamdb.info/calculator/\">HERE</a></p></body>";
            showIntroFrame(steamClient != null ? progressHTML : configHtml);
        }

        public void InvokeReloadGameList()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new ReloadGameListDelegate(reloadGameList));
            }
            else
            {
                reloadGameList();
            }
        }
    }
}