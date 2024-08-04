using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;
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
        public String iaLinkPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\cache\\links.json";
        public String emuDeckCachePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\cache\\emuDeckHashes.json";
        public Dictionary<String, Dictionary<String, String>> hashMap = new Dictionary<String, Dictionary<String, String>>();
        private JObject divStateModel;
        private JObject iaLinksModel;
        public delegate void ReloadGameListDelegate(String raConsole = "");
        public delegate void ShowIntroFrameDelegate(String html);
        public delegate void AddToGameListSelectorDelegate();
        public bool disableCheckBoxes = true;

        Stopwatch hashMapTimer = new Stopwatch();

        public Dictionary<ulong, String> edFolderMap = new Dictionary<ulong, String>()
        {
            { 1, "genesis" },
            { 2, "n64" },
            { 3, "snes" },
            { 4, "gb" },
            { 5, "gba" },
            { 6, "gbc" },
            { 7, "nes" },
            { 8, "pcengine" },
            { 9, "segacd" },
            { 10, "sega32x" },
            { 11, "mastersystem" },
            { 12, "psx" },
            { 13, "atarilynx" },
            { 14, "ngp" },
            { 15, "gamegear" },
            { 16, "gc" },
            { 17, "atarijaguar" },
            { 18, "nds" },
            { 21, "ps2" },
            { 23, "odyssey2" },
            { 24, "pokemini" },
            { 25, "atari2600" },
            { 27, "fbneo" },
            { 28, "virtualboy" },
            { 29, "msx" },
            { 33, "sg-1000" },
            { 37, "amstradcpc" },
            { 38, "apple2" },
            { 39, "saturn" },
            { 40, "dreamcast" },
            { 41, "psp" },
            { 43, "3do" },
            { 44, "colecovision" },
            { 45, "intellivision" },
            { 46, "vectrex" },
            { 47, "pc88" },
            { 49, "pcfx" },
            { 51, "atari7800" },
            { 53, "wonderswan" },
            { 56, "neogeocd" },
            { 63, "supervision" },
            { 69, "megaduck" },
            { 71, "arduboy" },
            { 72, "wasm4" },
            { 73, "arcadia" },
            { 76, "pcenginecd" },
            { 77, "atarijaguarcd" },
            { 78, "nds" }
        };



        public Form1()
        {
            InitializeComponent();
            InitializeAsync();

            try
            {

                if (!Directory.Exists(Properties.Settings1.Default.emuDeckRomsPath))
                {
                    setupEmudeckInstallToolStripMenuItem.Enabled = false;
                }

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

                if (Properties.Settings1.Default.steamWebAPIKey.Length == 0 || Properties.Settings1.Default.steamAcctID == 0)
                {
                    //throw new Exception("Please load your Steam Web Key and Account ID");
                }
                else
                {
                    steamClient = new Steam(Properties.Settings1.Default.steamAcctID, Properties.Settings1.Default.steamWebAPIKey, startOffline: offlineModeCB.Checked);
                    steamClient.OnSteamAppListLoaded += OnSteamLoadedAppList;
                    gameListSelectionCmbBox.Enabled = false;

                }

                if (Properties.Settings1.Default.RAAPIKey.Length == 0 || Properties.Settings1.Default.RAUsername.Length == 0)
                {
                    gameListSelectionCmbBox.Items.Add("RA Config Needed");
                    //throw new Exception("Please Load Your Retro Achievment Username and Webkey");
                }
                else
                {
                    raClient = new RetroAchievements(Properties.Settings1.Default.RAUsername, Properties.Settings1.Default.RAAPIKey, startOffline: offlineModeCB.Checked);
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

            if (File.Exists(iaLinkPath))
            {
                iaLinksModel = JObject.Parse(File.ReadAllText(iaLinkPath));
            }
            else
            {
                iaLinksModel = new JObject();
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

            menuStrip1.BackColor = Color.FromArgb(65, 65, 65);
            menuStrip1.ForeColor = Color.White;


            backBtn.BackColor = Color.FromArgb(120, 120, 120);
            backBtn.ForeColor = Color.White;
            backBtn.Visible = false;

            hashMap = loadHashMap(emuDeckCachePath);
        }

        public static Dictionary<String, Dictionary<String, String>> loadHashMap(String path)
        {
            Dictionary<String, Dictionary<String, String>> rv = new Dictionary<string, Dictionary<string, string>>();
            if (File.Exists(path))
            {
                try
                {
                    JObject obj = JObject.Parse(File.ReadAllText(path));
                    foreach (JProperty prop in obj.Properties())
                    {
                        String filePath = prop.Name;
                        String hash = (String)obj[filePath]["hash"];
                        String modTime = (String)obj[filePath]["modified"];
                        rv.Add(prop.Name, new Dictionary<string, string>() { { "hash", hash }, { "modified", modTime } });
                    }
                }
                catch
                {
                    MessageBox.Show("Error Loading Emudeck Hash Cache. Deleting it.");
                    File.Delete(path);
                }
            }
            return rv;
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
                else if (content.StartsWith("download") && iaLinksModel.Count > 0)
                {
                    bool bSuccess = false;
                    String hash = content.Split(new char[] { '-' })[0];
                    String downloadPath = "";
                    String url = "https://archive.org/download/retroachievements_collection_";
                    String collectionName = "v5";
                    RetroAchievementsGame game = (RetroAchievementsGame)selectedGame;
                    switch (game.consoleID)
                    {
                        case 7:
                            collectionName = "NES-Famicom";
                            break;
                        case 3:
                            collectionName = "SNES-Super Famicom";
                            break;
                        case 12:
                            collectionName = "PlayStation";
                            break;
                        case 21:
                            if (Regex.IsMatch(game.name.Trim(), "^[N-Zn-z]"))
                            {
                                collectionName = "PlayStation_2_N-Z";
                            }
                            else
                            {
                                collectionName = "PlayStation_2_A-M";
                            }
                            break;
                        case 41:
                            collectionName = "PlayStation Portable";
                            break;
                        case 16:
                            collectionName = "GameCube";
                            break;
                        case 27:
                            url = "https://archive.org/download/2020_01_06_fbn/roms";
                            collectionName = "";
                            break;
                    }

                    collectionName = collectionName.Replace(" ", "_");

                    String qualPath = ((JProperty)(((JObject)(iaLinksModel[$"{game.id}"][0])).First)).Value.Value<String>();

                    if (qualPath.Contains("\\"))
                    {
                        String[] splitArr = qualPath.Split('\\');
                        // FB Neo Rom
                        String system = splitArr[0].Replace("megadriv", "megadrive");
                        String fileName = splitArr[splitArr.Length - 1];
                        collectionName = "";
                        qualPath = $"{system}.zip/{system}/{fileName}";
                    }

                    url = url + collectionName + "/" + Uri.EscapeUriString(qualPath); //HttpUtility.UrlEncode(qualPath);

                    List<ulong> largeFileRoms = new List<ulong>() { 7, 3, 12, 21, 41, 16 };

                    if (largeFileRoms.Contains(game.consoleID))
                    {
                        MessageBox.Show($"Rom is very large. Passing Download to your default browser.");
                        try
                        {
                            Process.Start(url);
                        }
                        catch
                        {
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                url = url.Replace("&", "^&");
                                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            if (edFolderMap.ContainsKey(game.consoleID))
                            {
                                String dir = Properties.Settings1.Default.emuDeckRomsPath + "\\" + edFolderMap[game.consoleID];
                                if (!Directory.Exists(dir))
                                {
                                    Directory.CreateDirectory(dir);
                                }
                                downloadPath = dir + "\\" + Path.GetFileName(qualPath);

                            }
                            else
                            {
                                MessageBox.Show($"Couldnt Find matching console folder for Console {gameListSelectionCmbBox.SelectedText}.\nSaving Rom to project folder.");
                                downloadPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + Path.GetFileName(qualPath);
                            }
                            Debug.WriteLine($"Downloading {url} to {downloadPath}");
                            //MessageBox.Show($"Downloading {url} to {downloadPath}");

                            DownloadProgress tmp = new DownloadProgress(url, downloadPath);
                            tmp.Show();
                        }
                        catch
                        {
                            bSuccess = false;
                        }
                    }

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
                case "Most Retro Points":
                    rv = AchievementSortOrder.MOSTRETROPOINTS;
                    break;
                case "Least Achieved":
                    rv = AchievementSortOrder.LEASTUNLOCKED;
                    break;
                case "Show Req to Beat Game":
                    rv = AchievementSortOrder.LEFTTOBEAT;
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
                    raClient.saveHTMLForGame(game, htmlPath, getSortOrder((String)sortByComboBox.Items[sortByComboBox.SelectedIndex]), showHidden: checkBox1.Checked, divStates: divStateModel);
                    offlineModeCB.Checked = !steamClient.isOnline;
                }

                achWebView.Source = new Uri(introPath);
                achWebView.Source = new Uri(htmlPath);
            }
        }

        private void showIntroFrame(String html)
        {
            File.WriteAllText(introPath, html);
            try
            {
                if (achWebView.Source.AbsoluteUri == new Uri(introPath).AbsoluteUri)
                {
                    achWebView.Reload();
                }
                else
                {
                    achWebView.Source = new Uri(introPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception:\n{ex.Message}");
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
                if (!sortByComboBox.Items.Contains("Most Retro Points"))
                {
                    sortByComboBox.Items.Add("Most Retro Points");
                }
                if (!sortByComboBox.Items.Contains("Least Achieved"))
                {
                    sortByComboBox.Items.Add("Least Achieved");
                }
            }
            else
            {
                String selectedString = (String)sortByComboBox.Items[sortByComboBox.SelectedIndex];
                bool resetSelectedIndex = (selectedString == "Show Missable Only" || selectedString == "Most Retro Points" || selectedString == "Least Achieved");
                if (!sortByComboBox.Items.Contains("Show Missable Only"))
                {
                    sortByComboBox.Items.Remove("Show Missable Only");
                }
                if (!sortByComboBox.Items.Contains("Most Retro Points"))
                {
                    sortByComboBox.Items.Remove("Most Retro Points");
                }
                if (!sortByComboBox.Items.Contains("Least Achieved"))
                {
                    sortByComboBox.Items.Remove("Least Achieved");
                }
                if (resetSelectedIndex)
                {
                    sortByComboBox.SelectedIndex = 0;
                }
            }
            reloadFrame();

            // Try to map dumb exponent scores to a usable linear scale
            //if (selectedGame.GetType() == typeof(RetroAchievementsGame))
            //{
            //    // Implement saveHTMLForGame for RA
            //    RetroAchievementsGame game = (RetroAchievementsGame)selectedGame;
            //    if (game.consoleID == 12)
            //    {
            //        List<RetroAchievementsGame> games =  raClient.getCompleteListForConsole(12);
            //        List<double> ratiosRaw = games.Select(gm => Math.Log10(gm.getRetroRatio())).ToList();
            //        ratiosRaw.Sort();
            //        List<double> ratios = games.Select(gm => map(Math.Log10(gm.getRetroRatio()), 0, ratiosRaw[ratiosRaw.Count-1], 1, 10)).ToList();
            //        ratios.Sort();
                    
            //        Debug.WriteLine($"Median: {ratios[ratios.Count/2]}");
            //        Debug.WriteLine($"Avg: {ratios.Sum()/ratios.Count}");
            //        Debug.WriteLine($"num > 10: {ratios.Where(r => r == 10).Count()}");
            //    }
            //}

            // Cache all games
            //if (selectedGame.GetType() == typeof(RetroAchievementsGame))
            //{
            //    // Implement saveHTMLForGame for RA
            //    RetroAchievementsGame game = (RetroAchievementsGame)selectedGame;
            //    if (game.consoleID == 12)
            //    {
            //        int i = 0;
            //        foreach (RetroAchievementsGame g in gamesListbox.Items)
            //        {
            //            i++;
            //            Debug.WriteLine($"Caching Games {g.ToString()} {i}/{gamesListbox.Items.Count}");
            //            try
            //            {

            //                raClient.saveHTMLForGame(g, htmlPath, getSortOrder((String)sortByComboBox.Items[sortByComboBox.SelectedIndex]), showHidden: checkBox1.Checked, divStates: divStateModel);
            //                Thread.Sleep(3000);
            //            }
            //            catch
            //            {
            //                Debug.WriteLine($"Exception Caching Games {g.ToString()}");
            //            }
            //        }
            //    }
            //    offlineModeCB.Checked = !steamClient.isOnline;
            //}
        }

        private static double map(double value, double fromLow, double fromHigh, double toLow, double toHigh)
        {
            return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
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
                    showIntroFrame($"<head><style>body {{background-color: rgba(120, 120, 120, 1); color: #eee;}}</style></head><body>{((steamClient == null) ? $"<h1>Steam API Configuration Needed!!</h1> <h3>Load your Steam Web Key and Steam Account ID (SteamID) in Tools > Settings...</h3><p>Get Your Steam Web Key <a href=\"https://steamcommunity.com/dev/apikey\">HERE</a></p><p>Get Your SteamAccount ID (SteamID) <a href=\"https://steamdb.info/calculator/\">HERE</a></p>" : "")}{((raClient != null) ? "" : "<h1>RetroAchievment API Configuration Needed!!</h1> <h3>Load your RA Web Key and Username in Tools > Settings</h3><p>Get Your RA Web Key <a href=\"https://retroachievements.org/controlpanel.php\">HERE</a></p>")}</body>");
                }
                else
                {
                    backBtn.Visible = true;
                    groupAchievmentsBtn.Visible = false;
                    if (raClient != null && raClient.getConsoles().Contains(raConsole))
                    {
                        ulong consoleID = raClient.getConsoleIDForName(raConsole);
                        disableCheckBoxes = false;
                        foreach (RetroAchievementsGame game in raClient.getGameListForConsole(consoleID))
                        {
                            CheckState state = CheckState.Unchecked;
                            if ((game.validateHashList(hashMap) != ""))
                            {

                                game.showIALink = false;
                                state = CheckState.Checked;
                            }
                            else
                            {
                                // Show Download Link Button
                                if (iaLinksModel.Count > 0 && iaLinksModel.ContainsKey($"{game.id}"))
                                {
                                    game.showIALink = true;
                                }
                            }
                            gamesListbox.Items.Add(game, state);
                        }
                        disableCheckBoxes = true;
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
                        disableCheckBoxes = false;
                        foreach (SteamGame g in gameList)
                        {
                            gamesListbox.Items.Add(g, CheckState.Checked);
                        }
                        disableCheckBoxes = true;
                    }
                }
            }

            // DIsable Checkboxes


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
            String configHtml = $"<head><style>body {{background-color: rgba(120, 120, 120, 1); color: #eee;}}</style></head><body><h1>Steam API Configuration Needed!!</h1> <h3>Load your Steam Web Key and Steam Account ID (SteamID) in Tools > Settings ...</h3><p>Get Your Steam Web Key <a href=\"https://steamcommunity.com/dev/apikey\">HERE</a></p><p>Get Your SteamAccount ID (SteamID) <a href=\"https://steamdb.info/calculator/\">HERE</a></p>{((raClient != null) ? "" : "<h1>RetroAchievment API Configuration Needed!!</h1> <h3>Load your RA Web Key and Username in Tools -> Settings ...</h3><p>Get Your RA Web Key <a href=\"https://retroachievements.org/controlpanel.php\">HERE</a></p>")}</body>";
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

        public void InvokeShowIntroFrame(String html)
        {
            if (this.InvokeRequired)
            {
                object[] parameters = new object[] { html };
                this.Invoke(new ShowIntroFrameDelegate(showIntroFrame), parameters);
            }
            else
            {
                showIntroFrame(html);
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

        private void hashMapFinished()
        {
            hashMapTimer.Stop();
            if (File.Exists(emuDeckCachePath))
            {
                InvokeShowIntroFrame($"<head><style>body {{background-color: rgba(120, 120, 120, 1); color: #eee;}}</style></head><body><h1>EmuDeck Hashes Calculated Successfully</h1></body>");
            }
            else
            {
                InvokeShowIntroFrame($"<head><style>body {{background-color: rgba(120, 120, 120, 1); color: #eee;}}</style></head><body><h1>Error Calculating EmuDeck Hashes</h1></body>");
            }

            MessageBox.Show((File.Exists(emuDeckCachePath)) ? "Success" : "Failed");
            hashMap = loadHashMap(emuDeckCachePath);
        }



        private void setupEmudeckInstallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool exitBtnEnabled = backBtn.Enabled;

            if (Directory.Exists(Properties.Settings1.Default.emuDeckRomsPath))
            {
                backBtn.Visible = false;
                gamesListbox.Enabled = false;
                gameListSelectionCmbBox.Enabled = false;
                //MessageBox.Show("Calculating Emudeck Rom Hashes");
                showIntroFrame($"<head><style>body {{background-color: rgba(120, 120, 120, 1); color: #eee;}}</style></head><body><h1>Caclulating EmuDeck Hashes</h1></body>");
                HashMapGenTool tool = new HashMapGenTool(Properties.Settings1.Default.emuDeckRomsPath, emuDeckCachePath);
                hashMapTimer.Reset();
                hashMapTimer.Start();
                tool.hashMapReady += hashMapFinished;
                tool.Show();
            }
            backBtn.Visible = exitBtnEnabled;
            gamesListbox.Enabled = true;
            gameListSelectionCmbBox.Enabled = true;
        }

        private void gamesListbox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (disableCheckBoxes)
            {
                e.NewValue = e.CurrentValue;
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.ShowDialog();

            bool reset = false;
            if (steamClient == null && Properties.Settings1.Default.steamAcctID != 0 && Properties.Settings1.Default.steamWebAPIKey.Length > 0)
            {
                reset = true;
            }

            if (raClient == null && Properties.Settings1.Default.RAUsername.Length > 0 && Properties.Settings1.Default.RAAPIKey.Length > 0)
            {
                reset = true;
            }

            if (reset)
            {
                MessageBox.Show("Steam or RA Client Settings Changed. Restarting Application");
                Application.Restart();
                Environment.Exit(0);
            }
        }
    }
}