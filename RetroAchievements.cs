﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SteamAchievmentViewer
{
    public class RetroAchievements
    {
        String cacheRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\cache";
        Dictionary <ulong, String> consoleList = new Dictionary<ulong, String> ();
        Dictionary<ulong, List<RetroAchievementsGame>> gameList = new Dictionary<ulong, List<RetroAchievementsGame>>();
        public bool isOnline = true;
        String username = "";
        String raWebKey = "";
        public EventHandler OnRAConsoleListLoaded;
        public RetroAchievements(String username, String raWebKey, bool startOffline = false)
        {
            isOnline = !startOffline;
            this.username = username;
            this.raWebKey = raWebKey;

            Thread myThread = new Thread(new ThreadStart(fetchConsoles));
            myThread.Start();
        }

        public object? sendHTTPRequest(String url, String method = "GET")
        {
            object rv = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = method;

            if (isOnline)
            {
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    isOnline = true;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        String responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                        try
                        {
                            rv = JObject.Parse(responseString);
                        }
                        catch
                        {
                            rv = JArray.Parse(responseString);
                        }
                        
                    }
                }
                catch
                {
                    isOnline = false;
                }
            }

            return rv;
        }

        public void fetchConsoles()
        {
            String cachePath = $"{cacheRootPath}\\RAConsoleList.json";
            JArray consolesArr = null;
            if (consoleList.Keys.Count == 0)
            {
                consolesArr = (JArray)sendHTTPRequest($"https://retroachievements.org/API/API_GetConsoleIDs.php?z={username}&y={raWebKey}&a=1&g=1");
                try
                {
                    if (consolesArr == null)
                    {
                        // Load arr from cache
                        if (File.Exists(cachePath))
                        {
                            consolesArr = JArray.Parse(File.ReadAllText(cachePath));
                        }
                    }
                    else
                    {
                        File.WriteAllText(cachePath, consolesArr.ToString());
                    }

                    if (consolesArr != null)
                    {
                        foreach (JObject consoleObj in consolesArr)
                        {
                            consoleList.Add((ulong)consoleObj["ID"], (String)consoleObj["Name"]);
                        }
                    }
                }
                catch
                {
                    consoleList = new Dictionary<ulong, string> ();
                }
            }
            OnRAConsoleListLoaded?.Invoke(this, EventArgs.Empty);
        }

        public List<String> getConsoles()
        {
            return consoleList.Values.OrderBy(name => name).ToList();
        }

        public Dictionary<ulong, List<RetroAchievementsGame>> getGameListMap()
        {
            return gameList;
        }

        public List<RetroAchievementsGame> getGameListForConsole(ulong consoleID)
        {
            List<RetroAchievementsGame> rv = new List<RetroAchievementsGame>();
            fetchGamesForConsole(consoleID);
            if (gameList.ContainsKey(consoleID))
            {
                rv = gameList[consoleID];
            }

            return rv;
        }

        public ulong getConsoleIDForName(String name)
        {
            ulong consoleID = 0;
            foreach (KeyValuePair<ulong, string> kvp in consoleList)
            {
                if (name == kvp.Value)
                {
                    consoleID = kvp.Key;
                    break;
                }
            }
            return consoleID;
        }

        public void fetchAchievementsForGame(RetroAchievementsGame game)
        {
            String cachePath = $"{cacheRootPath}\\RAConsole{game.consoleID}Game{game.id}AchList.json";
            JObject achObj = null;
            achObj = (JObject)sendHTTPRequest($"https://retroachievements.org/API/API_GetGameInfoAndUserProgress.php?z={username}&y={raWebKey}&g={game.id}&u={username}&a=1");
            try
            {
                if (achObj == null)
                {
                    // Load arr from cache
                    if (File.Exists(cachePath))
                    {
                        achObj = JObject.Parse(File.ReadAllText(cachePath));
                    }
                }
                else
                {
                    File.WriteAllText(cachePath, achObj.ToString());
                }

                if (achObj != null)
                {
                    game.addAchievementsToObject(achObj);
                }
            }
            catch
            {
                game.achievements = new Dictionary<string, List<RetroAchievementsAchievemnt>>();
            }
        }

        public void saveHTMLForGame(RetroAchievementsGame game, String path, AchievementSortOrder sortOrder = AchievementSortOrder.ABSOLUTE, Boolean showHidden = false, JObject divStates = null)
        {
            bool offlineButNoData = false;
            try
            {
                fetchAchievementsForGame(game);
                game.saveHTMLForGame(path, sortOrder: sortOrder, showHidden: showHidden, showOfflineHeader: !isOnline, divStates: divStates);
            }
            catch
            {
                // Its ok to fail some games have 0 achievments
                offlineButNoData = true;
                File.WriteAllText(path, $"<head><style>body {{background-color: rgba(120, 120, 120, 1); color: #eee;}}</style></head><body><h1>Nothing to See Here</h1><h3>{game.name} has no achievements</h3></body>");
            }

            if ((offlineButNoData || game.achievements.Count == 0) && !isOnline)
            {
                // Load A Missing Data HTML
                File.WriteAllText(path, $"<head><style>body {{background-color: rgba(120, 120, 120, 1); color: #eee;}}</style></head><body><h1>Offline Mode</h1><h3>No Cached Data for {game.name}</h3></body>");

            }
        }

        public void fetchGamesForConsole(ulong consoleID)
        {
            String cachePath = $"{cacheRootPath}\\RAConsole{consoleID}GameList.json";
            JArray gameArr = null;
            if (!gameList.ContainsKey(consoleID))
            {
                gameArr = (JArray)sendHTTPRequest($"https://retroachievements.org/API/API_GetGameList.php?z={username}&y={raWebKey}&i={consoleID}&h=1&f=1");
                try
                {
                    if (gameArr == null)
                    {
                        // Load arr from cache
                        if (File.Exists(cachePath))
                        {
                            gameArr = JArray.Parse(File.ReadAllText(cachePath));
                        }
                    }
                    else
                    {
                        File.WriteAllText(cachePath, gameArr.ToString());
                    }

                    if (gameArr != null)
                    {
                        gameList[consoleID] = new List<RetroAchievementsGame> ();
                        foreach (JObject gameObj in gameArr)
                        {
                            gameList[consoleID].Add(new RetroAchievementsGame(gameObj));
                        }
                    }
                }
                catch
                {
                    if (gameList.ContainsKey(consoleID))
                    {
                        gameList.Remove(consoleID);
                    }
                }
            }
        }
    }
    public class RetroAchievementsGame
    {
        public readonly String name = "";
        public String guideURL = "";
        public readonly ulong id = 0;
        public readonly ulong consoleID = 0;
        public readonly String webLink = "";
        public Dictionary<String, List<RetroAchievementsAchievemnt>> achievements = new Dictionary<string, List<RetroAchievementsAchievemnt>>();

        public RetroAchievementsGame(JObject json)
        {
            id = (ulong)json["ID"];
            consoleID = (ulong)json["ConsoleID"];
            name = (String)json["Title"];

            webLink = $"https://retroachievements.org/game/{id}";
        }

        public void addAchievementsToObject(JObject json)
        {
            achievements = new Dictionary<string, List<RetroAchievementsAchievemnt>>();
            guideURL = (json["GuideURL"] != null) ? (String)json["GuideURL"] : "";
            foreach (String key in ((JObject)json["Achievements"]).Properties().Select(prop => prop.Name))
            {
                if (!achievements.ContainsKey("Unmapped"))
                {
                    achievements["Unmapped"] = new List<RetroAchievementsAchievemnt>();
                }
                achievements["Unmapped"].Add(new RetroAchievementsAchievemnt((JObject)json["Achievements"][key]));
            }

            foreach (String key in achievements.Keys)
            {
                achievements[key] = achievements[key].OrderBy(c => c.displayOrder).ToList();
            }
        }

        public Dictionary<String, List<RetroAchievementsAchievemnt>> getSortedAcheivements(AchievementSortOrder sortOrder)
        {
            Dictionary<String, List<RetroAchievementsAchievemnt>> rv = new Dictionary<string, List<RetroAchievementsAchievemnt>>();
            foreach (String key in achievements.Keys)
            {
                
                List<RetroAchievementsAchievemnt> achievmentsUnlocked = achievements[key].Where(c => c.getUnlockStatus()).OrderBy(c => c.name).ToList();
                List<RetroAchievementsAchievemnt> achievmentsLocked = achievements[key].Where(c => !c.getUnlockStatus()).OrderBy(c => c.name).ToList();
                achievmentsUnlocked.AddRange(achievements[key].Where(c => !c.getUnlockStatus()).OrderBy(c => c.name));
                achievmentsLocked.AddRange(achievements[key].Where(c => c.getUnlockStatus()).OrderBy(c => c.name));

                List<RetroAchievementsAchievemnt> achievmentsUnlockedGameOrder = achievements[key].Where(c => c.getUnlockStatus()).ToList();
                List<RetroAchievementsAchievemnt> achievmentsLockedGameOrder = achievements[key].Where(c => !c.getUnlockStatus()).ToList();
                achievmentsUnlockedGameOrder.AddRange(achievements[key].Where(c => !c.getUnlockStatus()));
                achievmentsLockedGameOrder.AddRange(achievements[key].Where(c => c.getUnlockStatus()));

                switch (sortOrder)
                {
                    case AchievementSortOrder.ABSOLUTE:
                        rv.Add(key, achievements[key].OrderBy(c => c.displayOrder).ToList());
                        break;
                    case AchievementSortOrder.NAMEASC:
                        rv.Add(key, achievements[key].OrderBy(c => c.name).ToList());
                        break;
                    case AchievementSortOrder.NAMEDESC:
                        rv.Add(key, achievements[key].OrderByDescending(c => c.name).ToList());
                        break;
                    case AchievementSortOrder.UNLOCKED:
                        rv.Add(key, achievmentsUnlocked);
                        break;
                    case AchievementSortOrder.LOCKED:
                        rv.Add(key, achievmentsLocked);
                        break;
                    case AchievementSortOrder.UNLOCKEDGAME:
                        rv.Add(key, achievmentsUnlockedGameOrder);
                        break;
                    case AchievementSortOrder.LOCKEDGAME:
                        rv.Add(key, achievmentsLockedGameOrder);
                        break;
                }
            }

            return rv;
        }

        public List<RetroAchievementsAchievemnt> GetAllAchievments()
        {
            List<RetroAchievementsAchievemnt> rv = new List<RetroAchievementsAchievemnt>();
            foreach (String key in achievements.Keys)
            {
                rv.AddRange(achievements[key]);
            }
            return rv;
        }

        private Boolean isCollapsed(JObject divStates, String headerName)
        {
            return !(divStates == null || (divStates != null && !divStates.ContainsKey($"RA{id}-{headerName}")) || (divStates != null && divStates.ContainsKey($"RA{id}-{headerName}") && divStates[$"RA{id}-{headerName}"].Value<String>() == "expanded"));
        }

        public Boolean saveHTMLForGame(String path, AchievementSortOrder sortOrder = AchievementSortOrder.ABSOLUTE, Boolean showHidden = false, JObject divStates = null, Boolean showOfflineHeader = false)
        {
            bool rv = false;
            List<String> htmlFile = new List<String>();
            List<String> html = new List<String>();
            List<String> javascript = new List<String>();
            List<String> css = new List<String>();

            css.Add(".achImage {width: 75px; height: 75px;}");
            css.Add(".achImageCont {width: 100px; height: 75px}");
            css.Add(".achRootTable {border: 3px solid #eee; width: 100%; border-collapse: collapse;}");
            css.Add(".achRootRow {border-bottom: 2px solid #eee; width: 100%;}");
            css.Add(".acRootRowUnlocked {background-color: rgba(51, 255, 94, .4);}");
            css.Add(".achTitle {font-size: 30px; text-align: left}");
            css.Add(".achDescription {font-size: 20px; text-align: left}");
            css.Add(".achUnlocked {font-size: 15px; text-align: left}");
            css.Add(".dividerHeader {font-size: 25px;}");
            css.Add(".offlineHeader {font-size: 15px; color: rgba(218, 227, 108, 1);}");
            css.Add(".gameHeader {font-size 30px;}");
            css.Add("body {background-color: rgba(120, 120, 120, 1); color: #eee;}");

            javascript.Add("function togglediv(id, headerId) { var e = document.getElementById(id); var headerElem = document.getElementById(headerId); if (headerElem.innerHTML[0] == '+') {headerElem.innerHTML = headerElem.innerHTML.replace(/^\\+/, \"-\")} else { headerElem.innerHTML = headerElem.innerHTML.replace(/^\\-/, \"+\") }; if ( e.style.display !== 'none' ) { e.style.display = 'none';  sendCollapsedEvent(headerId); } else { e.style.display = ''; sendExpandEvent(headerId);} }");
            javascript.Add("function collapseFunc(elem) { elem.style.display = \"none\"; sendCollapsedEvent(elem.id);}");
            javascript.Add("function expandFunc(elem) { elem.style.display = \"block\"; sendExpandEvent(elem.id);}");
            javascript.Add("function updateHeaderIconMinus(elem) { elem.innerHTML = elem.innerHTML.replace(/^[\\-+]/, \"-\"); }");
            javascript.Add("function updateHeaderIconPlus(elem) { elem.innerHTML = elem.innerHTML.replace(/^[\\-+]/, \"+\"); }");
            javascript.Add("function collapseAll(collapse) { if (collapse) { document.querySelectorAll('.dividerDiv').forEach(collapseFunc); document.querySelectorAll('.dividerHeader').forEach(updateHeaderIconPlus);} else { document.querySelectorAll('.dividerDiv').forEach(expandFunc); document.querySelectorAll('.dividerHeader').forEach(updateHeaderIconMinus);}}");
            javascript.Add("function reloadFrame() {window.chrome.webview.postMessage('reload');}");
            javascript.Add("function sendCollapsedEvent(id) {window.chrome.webview.postMessage('collapsed-' + id);}");
            javascript.Add("function sendExpandEvent(id) {window.chrome.webview.postMessage('expanded-' + id);}");


            html.Add($"<h1 id=\"{name}Header\">{name} {GetAllAchievments().Where(a => a.getUnlockStatus()).Count()}/{GetAllAchievments().Count} Achievements</h1>");
            // Add Controls
            if (showOfflineHeader)
            {
                html.Add("<h2 class=\"offlineHeader\">Offline Mode Enabled (Error Contacting Steam)</h2>");
            }

            String buttonHTML = $"<table><tr>";
            if (achievements.Keys.Count > 1)
            {
                buttonHTML += "<td><input type='button' value='Collapse All' onclick=\"collapseAll(true)\"></td><td><input type='button' value='Expand All' onclick=\"collapseAll(false)\"></td>";
            }
            buttonHTML += "<td><input type='button' value='Refresh' onclick=\"reloadFrame()\"></td>";
            buttonHTML += $"<td><input type='button' value='Open in Web ...' onclick=\"window.location.href = '{webLink}'\"></td>";
            buttonHTML += "</tr></table>";
            html.Add(buttonHTML);
            html.Add($"<div id=\"{name}Div\" class=\"gameHeader\">");

            foreach (String key in achievements.Keys)
            {
                Dictionary<String, List<RetroAchievementsAchievemnt>> achievementsMap = getSortedAcheivements(sortOrder);
                if (achievements[key].Count > 0)
                {
                    if (achievements.Keys.Count > 1)
                    {
                        html.Add($"<h1 id=\"{key}Header\" class=\"dividerHeader\" onclick=\"togglediv('{key}Div', '{key}Header')\">{(isCollapsed(divStates, key) ? "+" : "-")} {key} {achievements[key].Where(a => a.getUnlockStatus()).Count()}/{achievements[key].Count} Acheivements</h1>");
                        html.Add($"<div id=\"{key}Div\" class=\"dividerDiv\" style=\"display: {(isCollapsed(divStates, key) ? "none" : "block")}\">");
                    }

                    html.Add($"<table class=\"achRootTable\">");

                    foreach (RetroAchievementsAchievemnt a in achievementsMap[key])
                    {
                        html.AddRange(a.getHtml(showHidden: showHidden));
                    }

                    html.Add($"</table>");
                    if (achievementsMap.Keys.Count > 1)
                    {
                        html.Add($"</div>");
                    }
                }
            }

            html.Add($"</div>");

            htmlFile.Add($"<head><style>{String.Join("\n", css)}</style><script type=\"text/javascript\">{String.Join("\n", javascript)}</script></head><body>{String.Join("\n", html)}</body>");

            try
            {
                File.WriteAllLines(path, htmlFile);
                rv = true;
            }
            catch
            {
                rv = false;
            }


            return rv;
        }

        public override string ToString()
        {
            return $"{name}";
        }
    }
    public class RetroAchievementsAchievemnt
    {
        public readonly ulong id = 0;
        public readonly String badgeName = "";
        // 64 x 64 img
        public readonly String lockedImage = "";
        public readonly String unlockedImage = "";
        public readonly String achievmentLink = "";
        public readonly String name = "";
        public readonly String description = "";
        public readonly ulong displayOrder = 0;

        public readonly bool unlocked = false;
        public readonly DateTime unlockedTime = DateTime.UnixEpoch;

        public RetroAchievementsAchievemnt(JObject json)
        {
            badgeName = (String)json["BadgeName"];
            id = (ulong)json["ID"];
            name = (String)json["Title"];
            description = (String)json["Description"];
            displayOrder = (ulong)json["DisplayOrder"];

            unlockedTime = (json["DateEarned"] != null) ? getDateTimeForDateString((String)json["DateEarned"]) : (json["DateEarnedHardcore"] != null) ? getDateTimeForDateString((String)json["DateEarnedHardcore"]) : DateTime.UnixEpoch;

            if (unlockedTime != DateTime.UnixEpoch)
            {
                unlocked = true;
            }

            lockedImage = $"https://media.retroachievements.org/Badge/{badgeName}_lock.png";
            unlockedImage = $"https://media.retroachievements.org/Badge/{badgeName}.png";
            achievmentLink = $"https://retroachievements.org/achievement/{id}";
        }

        private DateTime getDateTimeForDateString(String dateString)
        {
            DateTime rv = DateTime.UnixEpoch;
            //2016-03-12 17:47:29 format
            rv = DateTime.ParseExact(dateString, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            return rv;
        }

        public bool getUnlockStatus()
        {
            return unlocked;
        }

        public List<String> getHtml(bool showHidden = false)
        {
            List<String> rv = new List<string>();

            rv.Add($"<tr class=\"achRootRow{((unlocked) ? " acRootRowUnlocked" : " acRootRowLocked")}\"><td class=\"achImageCont\"><a href=\"{achievmentLink}\"><img class=\"achImage\" src=\"{((unlocked) ? unlockedImage : lockedImage)}\"></a></td><td><table class=\"achTextTable\"><tr><td class=\"achTitle\">{name}</td></tr><tr><td class=\"achDescription\">{description}</td></tr><tr><td class=\"achUnlocked\">{(unlocked ? $"Unlocked {unlockedTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")}" : "")}</td></tr></table></td></tr>");

            return rv;
        }
    }   


}
