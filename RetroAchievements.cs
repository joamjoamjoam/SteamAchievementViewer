using Newtonsoft.Json.Linq;
using System;
using System.CodeDom;
using System.Collections;
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
        public ulong totalPlayers = 0;
        public ulong totalBasePoints = 0; // Base
        public List<String> validHashes = new List<String>();
        public Dictionary<String, List<RetroAchievementsAchievemnt>> achievements = new Dictionary<string, List<RetroAchievementsAchievemnt>>();

        public RetroAchievementsGame(JObject json)
        {
            id = (ulong)json["ID"];
            consoleID = (ulong)json["ConsoleID"];
            name = (String)json["Title"];

            if (json.ContainsKey("Hashes"))
            {
                validHashes = ((JArray)json["Hashes"]).Select(p => (String)p).Select(p => p.ToLower()).ToList();
            }


            webLink = $"https://retroachievements.org/game/{id}";
        }

        public String validateHashList(Dictionary<String, String> hashMap)
        {
            String rv = "";
            foreach (KeyValuePair<String, String> kvp in hashMap)
            {
                if (kvp.Key.Contains("3 Ninjas"))
                {

                }
                if (validHashes.Contains(kvp.Value))
                {
                    rv = kvp.Key;
                    break;
                }
            }
            return rv;
        }

        public bool isBeaten()
        {
            List<RetroAchievementsAchievemnt> achList = GetAllAchievments();
            List<RetroAchievementsAchievemnt> progressionList = achList.Where(ach => ach.achType.ToLower() == "progression").ToList();
            List<RetroAchievementsAchievemnt> winConditionList = achList.Where(ach => ach.achType.ToLower() == "win_condition").ToList();
            return (achList.Count > 0 && progressionList.Where(ach => !ach.getUnlockStatus()).Count() == 0 && winConditionList.Where(ach => ach.getUnlockStatus()).Count() > 0);
        }

        public bool isMastered()
        {
            List<RetroAchievementsAchievemnt> achList = GetAllAchievments();
            return (achList.Count > 0 && achList.Where(ach => (!ach.getUnlockStatus())).Count() == 0);
        }

        public void addAchievementsToObject(JObject json)
        {
            achievements = new Dictionary<string, List<RetroAchievementsAchievemnt>>();
            guideURL = (json["GuideURL"] != null) ? (String)json["GuideURL"] : "";
            totalPlayers = (ulong)json["NumDistinctPlayers"];
            totalBasePoints = (ulong)json["points_total"];
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
                    case AchievementSortOrder.SHOWMISSABLEONLY:
                        rv.Add(key, achievements[key].OrderBy(c => c.displayOrder).Where(c => c.achType == "missable").ToList());
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
                    case AchievementSortOrder.MOSTRETROPOINTS:
                        rv.Add(key, achievements[key].OrderByDescending(c => c.retroRatio).ToList());
                        break;
                    case AchievementSortOrder.LEASTUNLOCKED:
                        rv.Add(key, achievements[key].OrderBy(c => c.numAwarded).ToList());
                        break;
                    case AchievementSortOrder.LEFTTOBEAT:
                        rv.Add(key, achievements[key].Where(c => c.achType == "progression" || c.achType == "win_condition").ToList());
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

            css.Add(".achImage {width: 105px; height: 105px;}");
            css.Add(".achImageCont {width: 120px; height: 75px}");
            css.Add(".achRootTable {border: 3px solid #eee; width: 100%; border-collapse: collapse;}");
            css.Add(".achRootRow {border-bottom: 2px solid #eee; width: 100%;}");
            css.Add(".acRootRowUnlocked {background-color: rgba(51, 255, 94, .4);}");
            css.Add(".achTitle {font-size: 30px; text-align: left; display:inline-block; padding-bottom: 10px; padding-top: 10px; margin: 0px 10px 0px 0px}");
            css.Add(".achTextTable {border-collapse: collapse;}");
            css.Add(".achDescription {font-size: 20px; text-align: left}");
            css.Add(".achUnlocked {font-size: 15px; text-align: left}");
            css.Add(".dividerHeader {font-size: 25px;}");
            css.Add(".offlineHeader {font-size: 15px; color: rgba(218, 227, 108, 1);}");
            css.Add(".achRootRowDetails {width: 25%;}");
            css.Add(".gameHeader {font-size 30px;}");
            css.Add(".achTypeTitle {background-color: rgba(0, 0, 0, 1); border-radius:15px; font-size: 20px;display: inline-block; padding: 0px 5px;   position: relative; float: right; margin: 0px 5px 0px 0px;}");
            css.Add(".achmissableTitle {background-color: rgba(250, 90, 90, 1)}");
            css.Add(".achprogressionTitle {background-color: #00A5D6}");
            css.Add(".achwin_conditionTitle {background-color: gold; color: black}");
            css.Add(".achPointsTitle {background-color: #00A5D6; border-radius:15px; font-size: 15px;display: inline-block; padding: 0px 5px; position: relative; float: right; margin: 0px 5px 0px 0px;}");
            css.Add(".achFakeTypeTitle {background-color: rgba(120, 120, 120, 1); color: rgba(120, 120, 120, 1); visibility: hidden;}");
            css.Add("progress[value] {   -webkit-appearance: none;    appearance: none; }  progress[value]::-webkit-progress-bar {   background-color: black;   display: inline-block;   border-radius: 100px;   height: 7px; width:100%; }  progress[value]::-webkit-progress-value { background-color: gold; border-radius: 100px; height: 7px; ; width:100%;} progress {width: 0; min-width: 100%; padding-right: 5px}");
            css.Add(".usersEarnedText { text-align: center; white-space:pre; }");
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


            html.Add($"<h1 id=\"{name}Header\">{name} {GetAllAchievments().Where(a => a.getUnlockStatus()).Count()}/{GetAllAchievments().Count} Achievements {((isMastered()) ? $"(Mastered)" : (isBeaten()) ? $"(Beaten)" : "")}</h1>");

            // Add Controls
            if (showOfflineHeader)
            {
                html.Add("<h2 class=\"offlineHeader\">Offline Mode Enabled (Error Contacting Steam)</h2>");
            }
            //string masteryBadgeHtml = $"<h1 id=\"{name}MasteryHeader\" class=\"gameBadge {((isMastered()) ? $"gameMasteryBadge\"> Mastered" : (isBeaten()) ? $"gameBeatenBadge\"> Beaten" : "")}</h1>";
            //if (isBeaten() || isMastered())
            //{
            //    html.Add(masteryBadgeHtml);
            //}

            String buttonHTML = $"<table><tr>";
            if (achievements.Keys.Count > 1)
            {
                buttonHTML += "<td><input type='button' value='Collapse All' onclick=\"collapseAll(true)\"></td><td><input type='button' value='Expand All' onclick=\"collapseAll(false)\"></td>";
            }
            buttonHTML += "<td><input type='button' value='Refresh' onclick=\"reloadFrame()\"></td>";
            buttonHTML += $"<td><input type='button' value='Open in Web ...' onclick=\"window.location.href = '{webLink}'\"></td>";
            if (guideURL != "")
            {
                buttonHTML += $"<td><input type='button' value='Open Guide ...' onclick=\"window.location.href = '{guideURL}'\"></td>";
            }
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
                        html.AddRange(a.getHtml(totalPlayers, showHidden: showHidden));
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
        public readonly String achType = "";
        public readonly String description = "";
        public readonly ulong displayOrder = 0;
        public readonly ulong basePoints = 0;
        public readonly float retroRatio = 0;
        public readonly ulong numAwarded = 0;
        public readonly ulong numAwardedHardcore = 0;

        public readonly bool unlocked = false;
        public readonly DateTime unlockedTime = DateTime.UnixEpoch;

        public RetroAchievementsAchievemnt(JObject json)
        {
            badgeName = (String)json["BadgeName"];
            id = (ulong)json["ID"];
            name = (String)json["Title"];
            description = (String)json["Description"];
            displayOrder = (ulong)json["DisplayOrder"];
            numAwarded = (ulong)json["NumAwarded"];
            numAwardedHardcore = (ulong)json["NumAwardedHardcore"];
            basePoints = (ulong)json["Points"];
            retroRatio = (float)json["TrueRatio"];
            achType = (json["type"] != null && json["type"].Type == JTokenType.String) ? (String)json["type"] : "";

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

        public List<String> getHtml(ulong totalPlayers, bool showHidden = false)
        {
            List<String> rv = new List<string>();

            double percentUnlocked = Math.Round(((float)numAwarded / (float)totalPlayers) * 100, 2);
            double percentUnlockedHardcore = Math.Round(((float)numAwardedHardcore / (float)totalPlayers) * 100, 2);

            rv.Add($"<tr class=\"achRootRow{((getUnlockStatus()) ? " acRootRowUnlocked" : " acRootRowLocked")}\"><td class=\"achImageCont\"><a href=\"{achievmentLink}\"><img class=\"achImage\" src=\"{((getUnlockStatus()) ? unlockedImage : lockedImage)}\"></a></td><td><table class=\"achTextTable\"><tr><td><p class=\"achTitle\">{name}</p></td></tr><tr><td class=\"achDescription\">{description}</td></tr><tr><td class=\"achUnlocked\">{(getUnlockStatus() ? $"Unlocked {unlockedTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")}" : "")}</td></tr></table></td><td class=\"achRootRowDetails\"><table style=\"width: 100%\"><tr><td><p class=\"achTypeTitle {((achType != "") ? $"ach{achType}Title" : "achFakeTypeTitle")}\">{System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(((achType != "") ? achType.ToLower() : "faketype")).Replace("_", " ")}</p></td></tr><tr><td><p class=\"achPointsTitle\">Points: Base {basePoints} |  Retro: {retroRatio}</p></td></tr><tr><td><progress id=\"file\" class=\"achEarnedProgressBar\" value=\"{percentUnlocked}\" max=\"100\"></progress><table style=\"width: 100%\"><tbody><tr><td class=\"usersEarnedText\">{numAwarded} ({numAwardedHardcore}) of {totalPlayers} players\n{percentUnlocked}% ({percentUnlockedHardcore}%) Unlock Rate</td></tr></tbody></table></td></tr></td></tr></table></td></tr>");

            return rv;
        }
    }   


}
