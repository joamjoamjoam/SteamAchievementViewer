using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Reflection;

namespace SteamAchievmentViewer
{
    public partial class Form1 : Form
    {
        public Steam steamClient = null;
        public List<SteamGame> games = new List<SteamGame>();
        public SteamGame selectedGame = null;
        public String htmlPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\index.html";
        public String introPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\intro.html";
        private String steamWebKey = "";
        private ulong steamAcctId = 0;

        public Form1()
        {
            InitializeComponent();

            try
            {
                JObject configObj = JObject.Parse(File.ReadAllText("config/config.json"));
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
            }
            catch
            {
                MessageBox.Show("Please load your Steam Web Key and Account ID", "Configuration Needed");
            }


            sortByComboBox.SelectedIndex = 0;
            checkBox1.Checked = true;

            // Generate Intro HTML

            File.WriteAllText("intro.html", $"<head><style>body {{background-color: rgba(120, 120, 120, 1); color: #eee;}}</style></head><body>{((steamClient == null) ? "<h1>Configuration Needed!!</h1> <h3>Load your Steam Web Key and Steam Account ID (SteamID) in config/config.json</h3><p>Get Your Steam Web Key <a href=\"https://steamcommunity.com/dev/apikey\">HERE</a></p><p>Get Your SteamAccount ID (SteamID) <a href=\"https://steamdb.info/calculator/\">HERE</a></p>" : "<h1>Select Game From Game List</h1>")}</body>");
            achWebView.Source = new Uri(introPath);
            Properties.Resources.locked.Save("locked.png", System.Drawing.Imaging.ImageFormat.Png);

            this.BackColor = Color.FromArgb(80, 84, 82);
            this.ForeColor = Color.White;

            gamesListbox.BackColor = Color.FromArgb(120, 120, 120);
            gamesListbox.ForeColor = Color.White;

            sortByComboBox.BackColor = Color.FromArgb(120, 120, 120);
            sortByComboBox.ForeColor = Color.White;

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
                steamClient.saveHTMLForGame(selectedGame.id, htmlPath, (AchievementSortOrder)sortByComboBox.SelectedIndex, showHidden: checkBox1.Checked);
                achWebView.Source = new Uri(introPath);
                achWebView.Source = new Uri(htmlPath);
            }
        }

        private void gamesListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SteamGame g = (SteamGame)((ListBox)sender).SelectedItem;
            selectedGame = g;
            reloadFrame();
        }

        private void sortByComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            reloadFrame();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            reloadFrame();
        }
    }
}