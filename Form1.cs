using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SteamAchievmentViewer
{
    public partial class Form1 : Form
    {
        public Steam steamClient = null;
        public RetroAchievements raClient = null;
        public List<SteamGame> games = new List<SteamGame>();
        public object selectedGame = null;
        public String htmlPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\index.html";
        public String introPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\intro.html";
        public String hiddenImgPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\locked.png";
        public String configPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\config\\config.json";
        public String divStatePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\divStates.json";
        private String steamWebKey = "";
        private ulong steamAcctId = 0;
        private String raWebKey = "";
        private String raUsername = "";
        private JObject divStateModel;
        public delegate void ReloadGameListDelegate(String raConsole = "");
        public delegate void AddToGameListSelectorDelegate();




        public Form1()
        {
            InitializeComponent();
            InitializeAsync();

            try
            {
                JObject configObj = JObject.Parse(File.ReadAllText(configPath));
                steamWebKey = configObj["steamWebKey"] == null ? "" : (String)configObj["steamWebKey"];
                steamAcctId = configObj["steamAcctID"] == null ? 0 : (ulong)configObj["steamAcctID"];
                raUsername = configObj["RAUsername"] == null ? "" : (String)configObj["RAUsername"];
                raWebKey = configObj["RAWebKey"] == null ? "" : (String)configObj["RAWebKey"];

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
                groupAchievmentsBtn.Visible = false;

                if (steamWebKey.Length == 0 || steamAcctId == 0)
                {
                    //throw new Exception("Please load your Steam Web Key and Account ID");
                }
                else
                {
                    steamClient = new Steam(steamAcctId, steamWebKey, startOffline: offlineModeCB.Checked);
                    steamClient.OnSteamAppListLoaded += OnSteamLoadedAppList;
                    gameListSelectionCmbBox.Enabled = false;

                }

                if (raWebKey.Length == 0 || raUsername.Length == 0)
                {
                    gameListSelectionCmbBox.Items.Add("RA Config Needed");
                    //throw new Exception("Please Load Your Retro Achievment Username and Webkey");
                }
                else
                {
                    raClient = new RetroAchievements(raUsername, raWebKey, startOffline: offlineModeCB.Checked);
                    raClient.OnRAConsoleListLoaded += OnRAConsoleListLoaded;
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Configuration Needed");
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

            gameListSelectionCmbBox.BackColor = Color.FromArgb(120, 120, 120);
            gameListSelectionCmbBox.ForeColor = Color.White;

            backBtn.BackColor = Color.FromArgb(120, 120, 120);
            backBtn.ForeColor = Color.White;
            backBtn.Visible = false;

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
                        String key = $"";
                        if (selectedGame.GetType() == typeof(SteamGame))
                        {
                            SteamGame game = (SteamGame)selectedGame;
                            key = $"{game.id}-{content.Split(new char[] { '-' })[1]}";
                        }
                        else
                        {
                            RetroAchievementsGame game = (RetroAchievementsGame)selectedGame;
                            key = $"RA{game.id}-{content.Split(new char[] { '-' })[1]}";
                        }
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
                    String key = $"";
                    if (selectedGame != null)
                    {
                        if (selectedGame.GetType() == typeof(SteamGame))
                        {
                            SteamGame game = (SteamGame)selectedGame;
                            key = $"{game.id}-{content.Split(new char[] { '-' })[1]}";
                        }
                        else
                        {
                            RetroAchievementsGame game = (RetroAchievementsGame)selectedGame;
                            key = $"RA{game.id}-{content.Split(new char[] { '-' })[1]}";
                        }
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

        public AchievementSortOrder getSortOrder(String sortOrder)
        {
            AchievementSortOrder rv = AchievementSortOrder.ABSOLUTE;

            switch (sortOrder)
            {
                case "Game Order":
                    rv = AchievementSortOrder.ABSOLUTE;
                    break;
                case "Name A-Z":
                    rv = AchievementSortOrder.NAMEASC;
                    break;
                case "Name Z-A":
                    rv = AchievementSortOrder.NAMEDESC;
                    break;
                case "Unlocked First A - Z":
                    rv = AchievementSortOrder.UNLOCKED;
                    break;
                case "Locked First A - Z":
                    rv = AchievementSortOrder.LOCKED;
                    break;
                case "Unlocked First Game Order":
                    rv = AchievementSortOrder.UNLOCKEDGAME;
                    break;
                case "Locked First Game Order":
                    rv = AchievementSortOrder.LOCKEDGAME;
                    break;
                case "Show Missable Only":
                    rv = AchievementSortOrder.SHOWMISSABLEONLY;
                    break;
            }

            return rv;
        }

        private void reloadFrame()
        {
            if (selectedGame != null && steamClient != null)
            {
                if (selectedGame.GetType() == typeof(SteamGame))
                {
                    SteamGame game = (SteamGame)selectedGame;
                    steamClient.saveHTMLForGame(game.id, htmlPath, getSortOrder((String)sortByComboBox.Items[sortByComboBox.SelectedIndex]), showHidden: checkBox1.Checked, divStates: divStateModel);
                    offlineModeCB.Checked = !steamClient.isOnline;
                }
                else if (selectedGame.GetType() == typeof(RetroAchievementsGame))
                {
                    // Implement saveHTMLForGame for RA
                    RetroAchievementsGame game = (RetroAchievementsGame)selectedGame;
                    raClient.saveHTMLForGame(game, htmlPath, (AchievementSortOrder)sortByComboBox.SelectedIndex, showHidden: checkBox1.Checked, divStates: divStateModel);
                    offlineModeCB.Checked = !steamClient.isOnline;
                }

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

            selectedGame = gamesListbox.Items[gamesListbox.SelectedIndex];
            if (selectedGame.GetType() == typeof(RetroAchievementsGame))
            {
                if (!sortByComboBox.Items.Contains("Show Missable Only"))
                {
                    sortByComboBox.Items.Add("Show Missable Only");
                }
            }
            else
            {
                bool resetSelectedIndex = ((String)sortByComboBox.Items[sortByComboBox.SelectedIndex] == "Show Missable Only");
                if (!sortByComboBox.Items.Contains("Show Missable Only"))
                {
                    sortByComboBox.Items.Remove("Show Missable Only");
                }
                if (resetSelectedIndex)
                {
                    sortByComboBox.SelectedIndex = 0;
                }
            }
            reloadFrame();
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

            if (raClient != null)
            {
                raClient.isOnline = !cb.Checked;
            }
        }

        private void groupAchievmentsBtn_Click(object sender, EventArgs e)
        {
            if (selectedGame != null)
            {
                GroupAchievments tmpForm = null;
                if (selectedGame.GetType() == typeof(SteamGame))
                {
                    SteamGame game = (SteamGame)selectedGame;
                    tmpForm = new GroupAchievments(steamClient, game.id);
                }
                else if (selectedGame.GetType() == typeof(RetroAchievementsGame))
                {

                }

                if (tmpForm != null)
                {
                    tmpForm.ShowDialog();
                    reloadFrame();
                }
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

        private void sortGameListSelectionBox()
        {
            List<String> rv = new List<String>();

            foreach (String item in gameListSelectionCmbBox.Items)
            {
                rv.Add(item);
            }

            rv = rv.OrderBy(name => name).ToList();

            gameListSelectionCmbBox.Items.Clear();
            foreach (String item in rv)
            {
                gameListSelectionCmbBox.Items.Add(item);
            }
        }

        public void reloadGameList(String raConsole = "")
        {
            bool frameShown = false;
            if (raConsole.Length > 0)
            {
                gamesListbox.Items.Clear();
                if (raConsole == "RA Config Needed")
                {
                    frameShown = true;
                    showIntroFrame($"<head><style>body {{background-color: rgba(120, 120, 120, 1); color: #eee;}}</style></head><body>{((steamClient == null) ? $"<h1>Steam API Configuration Needed!!</h1> <h3>Load your Steam Web Key and Steam Account ID (SteamID) in config/config.json</h3><p>Get Your Steam Web Key <a href=\"https://steamcommunity.com/dev/apikey\">HERE</a></p><p>Get Your SteamAccount ID (SteamID) <a href=\"https://steamdb.info/calculator/\">HERE</a></p>" : "")}{((raClient != null) ? "" : "<h1>RetroAchievment API Configuration Needed!!</h1> <h3>Load your RA Web Key and Username in config/config.json</h3><p>Get Your RA Web Key <a href=\"https://retroachievements.org/controlpanel.php\">HERE</a></p>")}</body>");
                }
                else
                {
                    backBtn.Visible = true;
                    groupAchievmentsBtn.Visible = false;
                    if (raClient != null && raClient.getConsoles().Contains(raConsole))
                    {
                        ulong consoleID = raClient.getConsoleIDForName(raConsole);
                        foreach (RetroAchievementsGame game in raClient.getGameListForConsole(consoleID))
                        {
                            gamesListbox.Items.Add(game);
                        }
                    }
                }
            }
            else
            {
                if (steamClient != null)
                {
                    if (!gameListSelectionCmbBox.Items.Contains("Steam"))
                    {
                        gameListSelectionCmbBox.Items.Add("Steam");
                        sortGameListSelectionBox();
                        gameListSelectionCmbBox.SelectedItem = "Steam";
                    }
                    gameListSelectionCmbBox.Enabled = true;
                    if (steamClient.getAppList().Keys.Count == 0 && steamClient.webKey != "")
                    {
                        MessageBox.Show("App List Cache is outdated or Steam Credentials are invalid. Please Connect to the internet to download the new cache.", "");
                    }
                    else
                    {
                        backBtn.Visible = false;
                        groupAchievmentsBtn.Visible = true;
                        gamesListbox.Items.Clear();
                        List<SteamGame> gameList = steamClient.getGames().OrderBy(g => g.gameName).ToList();
                        foreach (SteamGame g in gameList)
                        {
                            gamesListbox.Items.Add(g);
                        }
                    }
                }
            }
            if (!frameShown)
            {
                showIntroFrame($"<head><style>body {{background-color: rgba(120, 120, 120, 1); color: #eee;}}</style></head><body>{((gamesListbox.Items.Count > 0) ? "<h1>Select Game From Game List</h1>" : (offlineModeCB.Checked) ? $"<head><style>body {{background-color: rgba(120, 120, 120, 1); color: #eee;}}</style></head><body><h1>Offline Mode</h1><h3>No Cached Data for This System.</h3></body>" : "<h1>Error Loading Game Data ...</h1>")}</body>");
            }
        }

        private void addToGameListSelector()
        {
            if (!(gameListSelectionCmbBox.Items.Count > 2))
            {
                if (raClient == null && !gameListSelectionCmbBox.Items.Contains("RA Config Needed"))
                {
                    gameListSelectionCmbBox.Items.Add("RA Config Needed");
                    String selected = (String)gameListSelectionCmbBox.Items[gameListSelectionCmbBox.SelectedIndex];
                    sortGameListSelectionBox();
                    gameListSelectionCmbBox.SelectedItem = selected;
                    offlineModeCB.Checked = true;
                }
                else
                {
                    foreach (String console in raClient.getConsoles())
                    {
                        gameListSelectionCmbBox.Items.Add(console);
                    }
                    String selected = "";
                    if (gameListSelectionCmbBox.SelectedIndex >= 0)
                    {
                        selected = (String)gameListSelectionCmbBox.Items[gameListSelectionCmbBox.SelectedIndex];
                    }
                    
                    sortGameListSelectionBox();

                    if (selected != "")
                    {
                        gameListSelectionCmbBox.SelectedItem = selected;
                    }
                    
                }
            }
        }

        private void OnSteamLoadedAppList(object sender, EventArgs e)
        {
            InvokeReloadGameList();
        }

        private void OnRAConsoleListLoaded(object sender, EventArgs e)
        {
            InvokeAddToGameListSelector();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            // Generate Intro HTML
            String progressHTML = "<head><style>body {background-color: rgba(120, 120, 120, 1); color: #eee;} #myProgress { width: 100%; background-color: grey; } #myBar { width: 1%; height: 30px; background-color: #04AA6D; text-align: center; line-height: 30px; font-weight: bold; color: white; }</style><script>var i = 0; function move() { if (i == 0) { i = 1; var elem = document.getElementById('myBar'); var width = 1; var id = setInterval(frame, 500); function frame() { incr = Math.floor(Math.random() * 10); cont = true; if (width >= 100) { cont = false; width = 100; } else { width+= incr; if(width > 95){ \twidth = 95; } elem.style.width = width + '%'; elem.innerHTML = width + '%'; } clearInterval(id); if(cont){ \tid = setInterval(frame, incr * 400); } } } }</script></head><body onload=\"move()\"><h1 style=\"text-align: center;\">Loading Applist from Steam ...</h1><div id=\"myProgress\"> <div id=\"myBar\">1%</div> </div></body>";
            String configHtml = $"<head><style>body {{background-color: rgba(120, 120, 120, 1); color: #eee;}}</style></head><body><h1>Steam API Configuration Needed!!</h1> <h3>Load your Steam Web Key and Steam Account ID (SteamID) in config/config.json</h3><p>Get Your Steam Web Key <a href=\"https://steamcommunity.com/dev/apikey\">HERE</a></p><p>Get Your SteamAccount ID (SteamID) <a href=\"https://steamdb.info/calculator/\">HERE</a></p>{((raClient != null) ? "" : "<h1>RetroAchievment API Configuration Needed!!</h1> <h3>Load your RA Web Key and Username in config/config.json</h3><p>Get Your RA Web Key <a href=\"https://retroachievements.org/controlpanel.php\">HERE</a></p>")}</body>";
            showIntroFrame(steamClient != null ? progressHTML : configHtml);
        }

        public void InvokeReloadGameList()
        {
            if (this.InvokeRequired)
            {
                object[] parameters = new object[] { "" };
                this.Invoke(new ReloadGameListDelegate(reloadGameList), parameters);
            }
            else
            {
                reloadGameList();
            }
        }

        public void InvokeAddToGameListSelector()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new AddToGameListSelectorDelegate(addToGameListSelector));
            }
            else
            {
                addToGameListSelector();
            }
        }

        private void gameListSelectionCmbBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;

            if ((string)cb.Items[cb.SelectedIndex] == "Steam")
            {
                reloadGameList();
            }
            else
            {
                reloadGameList((String)cb.Items[cb.SelectedIndex]);
            }
        }

        private void backBtn_Click(object sender, EventArgs e)
        {
            achWebView.GoBack();
        }
    }
}