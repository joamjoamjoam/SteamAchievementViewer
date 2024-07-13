using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SteamAchievmentViewer
{
    public class Steam
    {
        private ulong acctID = 0;
        public readonly String webKey = "";
        private String cacheRootPath = "";
        public Boolean isOnline = true;
        private Dictionary<ulong, String> appList = new Dictionary<ulong, string>();
        private List<SteamGame> games = new List<SteamGame>();
        public EventHandler OnSteamAppListLoaded;

        public Steam(ulong steamAccountID, String steamWebKey, bool startOffline = false)
        {
            acctID = steamAccountID;
            webKey = steamWebKey;
            isOnline = !startOffline;
            cacheRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\cache";
            if (!Directory.Exists(cacheRootPath))
            {
                Directory.CreateDirectory(cacheRootPath);
            }

            Thread myThread = new Thread(new ThreadStart(fetchSteamGames));
            myThread.Start();
        }

        public JObject? sendHTTPRequest(String url, String method = "GET")
        {
            JObject rv = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            // Request Body
            //String postData = "";
            //byte[] data = Encoding.ASCII.GetBytes(postData);

            request.Method = method;
            //request.ContentType = "application/x-www-form-urlencoded";
            //request.ContentLength = data.Length;

            //using (Stream stream = request.GetRequestStream())
            //{
            //    stream.Write(data, 0, data.Length);
            //}
            if (isOnline)
            {
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    isOnline = true;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        String responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                        rv = JObject.Parse(responseString);
                    }
                }
                catch
                {
                    isOnline = false;
                }
            }

            return rv;
        }

        public Dictionary<ulong, String> getAppList()
        {
            return appList;
        }

        private void getAppListFromSteam()
        {
            ulong lastAppID = 0;
            int loops = 0;
            if (appList.Keys.Count == 0)
            {
                try
                {
                    Boolean fetchContinue = true;
                    JObject? appListObj = null;

                    while (fetchContinue)
                    {
                        loops++;
                        appListObj = sendHTTPRequest($"https://api.steampowered.com/IStoreService/GetAppList/v1/?key={webKey}&include_games=true&last_appid={lastAppID}");
                        fetchContinue = (appListObj["response"]["have_more_results"] != null) ? (bool)appListObj["response"]["have_more_results"] : false;
                        lastAppID = (appListObj["response"]["last_appid"] != null) ? (ulong)appListObj["response"]["last_appid"] : lastAppID;
                        foreach (JObject entry in appListObj["response"]["apps"])
                        {
                            appList[(ulong)entry["appid"]] = ((String)entry["name"]);
                        }

                    }
                }
                catch (Exception ex)
                {
                    appList = new Dictionary<ulong, String>();
                }

                if (appList.Keys.Count == 0)
                {
                    try
                    {
                        if (File.Exists(cacheRootPath + "\\appListCache.json"))
                        {
                            JObject appObj = JObject.Parse(File.ReadAllText(cacheRootPath + "\\appListCache.json"));
                            foreach (JProperty prop in appObj.Properties())
                            {
                                appList.Add(ulong.Parse(prop.Name), Regex.Replace(prop.Value.Value<String>(), @"[^\u0000-\u007F]+", string.Empty));
                            }
                        }
                    }
                    catch
                    {
                        // return empty applist json to app to throw an error.
                        appList = new Dictionary<ulong, String>();
                    }

                }
                else
                {
                    // Serialize app list to a file
                    String appListJson = "{\n";
                    appListJson += String.Join(",\n", appList.Select(kvp => $"\t\"{kvp.Key}\": \"{kvp.Value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\""));
                    appListJson += "}";
                    File.WriteAllText(cacheRootPath + "\\appListCache.json", appListJson);
                }
            }
        }

        public void updateAchievmentStatusForAppID(SteamGame game, ulong appid)
        {
            String userAchievmentsCachePath = $"{cacheRootPath}\\{appid}UserAch.json";

            JObject? userAchObj = null;

            userAchObj = sendHTTPRequest($"https://api.steampowered.com/ISteamUserStats/GetPlayerAchievements/v1/?key={webKey}&steamid={acctID}&appid={appid}");
            if (userAchObj != null)
            {
                File.WriteAllText(userAchievmentsCachePath, userAchObj.ToString());
            }
            else
            {
                if (userAchObj == null)
                {
                    if (File.Exists(userAchievmentsCachePath))
                    {
                        userAchObj = JObject.Parse(File.ReadAllText(userAchievmentsCachePath));
                    }
                }
            }


            if (userAchObj != null)
            {
                game.updateAchievmentsStatus(userAchObj);
            }
            else
            {
                throw new Exception($"Error Fetching appdetails for {appid} - Steam return a failure and no cache");
            }
        }



        private void fetchSteamGames()
        {

            if (appList.Count == 0)
            {
                getAppListFromSteam();

                games.Clear();

                JObject? gameIDObj = sendHTTPRequest($"https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key={webKey}&steamid={acctID}&include_played_free_games=true&include_extended_appinfo=true");

                if (gameIDObj == null)
                {
                    if (File.Exists(cacheRootPath + "\\gamesList.json"))
                    {
                        gameIDObj = JObject.Parse(File.ReadAllText(cacheRootPath + "\\gamesList.json"));
                    }
                }
                else
                {
                    File.WriteAllText(cacheRootPath + "\\gamesList.json", gameIDObj.ToString());
                }


                if (gameIDObj != null)
                {
                    foreach (JObject prop in (JArray)gameIDObj["response"]["games"])
                    {
                        ulong appid = (ulong)prop["appid"];
                        try
                        {
                            ulong playtimeForever = (ulong)prop["playtime_forever"];
                            games.Add(new SteamGame(appList[appid], appid));
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine($"Error Fetching appdetails for {appid} - {e.Message}");
                        }
                    }
                }
                else
                {
                    //throw new Exception($"Error Fetching appdetails - Steam return a failure");
                }
            }
            OnSteamAppListLoaded?.Invoke(this, EventArgs.Empty);
        }

        public List<SteamGame> getGames()
        {
            return games;
        }
        public SteamGame getGame(ulong id)
        {
            return games.Where(g => g.id == id).FirstOrDefault();
        }


        public Dictionary<String, List<SteamAchievment>> getAchievmentsForApp(ulong id)
        {
            Dictionary<String, List<SteamAchievment>> rv = new Dictionary<String, List<SteamAchievment>>();

            SteamGame g = games.Where(g => g.id == id).FirstOrDefault();
            if (g != null)
            {
                SteamGame gameCopy = new SteamGame(g.gameName, id);

                JObject? achDataObj = null;
                achDataObj = sendHTTPRequest($"https://api.steampowered.com/ISteamUserStats/GetSchemaForGame/v2/?key={webKey}&appid={id}");

                if (achDataObj == null)
                {
                    if (File.Exists(cacheRootPath + $"\\{id}Ach.json"))
                    {
                        achDataObj = JObject.Parse(File.ReadAllText(cacheRootPath + $"\\{id}Ach.json"));
                    }
                }
                else
                {
                    File.WriteAllText(cacheRootPath + $"\\{id}Ach.json", achDataObj.ToString());
                }

                try
                {
                    if (achDataObj[$"game"]["availableGameStats"]["achievements"] != null)
                    {
                        //File.WriteAllText(gameAchievmentsCachePath, achDataObj.ToString());
                        gameCopy.loadAchievmentsModel((JArray)achDataObj[$"game"]["availableGameStats"]["achievements"]);
                        rv = gameCopy.GetAchievments();

                    }
                }
                catch
                {

                }
            }

            return rv;
        }

        public Boolean saveHTMLForGame(ulong id, String path, AchievementSortOrder sortOrder = AchievementSortOrder.ABSOLUTE, Boolean showHidden = false, JObject divStates = null)
        {
            Boolean rv = false;
            Boolean offlineButNoData = false;
            SteamGame g = games.Where(g => g.id == id).FirstOrDefault();
            if (g != null)
            {
                SteamGame gameCopy = new SteamGame(g.gameName, id);

                JObject? achDataObj = null;
                achDataObj = sendHTTPRequest($"https://api.steampowered.com/ISteamUserStats/GetSchemaForGame/v2/?key={webKey}&appid={id}");

                if (achDataObj == null)
                {
                    if (File.Exists(cacheRootPath + $"\\{id}Ach.json"))
                    {
                        achDataObj = JObject.Parse(File.ReadAllText(cacheRootPath + $"\\{id}Ach.json"));
                    }
                    else
                    {
                        offlineButNoData = true;
                    }
                }
                else
                {
                    File.WriteAllText(cacheRootPath + $"\\{id}Ach.json", achDataObj.ToString());
                }

                try
                {
                    if (achDataObj[$"game"]["availableGameStats"]["achievements"] != null)
                    {
                        //File.WriteAllText(gameAchievmentsCachePath, achDataObj.ToString());
                        gameCopy.loadAchievmentsModel((JArray)achDataObj[$"game"]["availableGameStats"]["achievements"]);
                        updateAchievmentStatusForAppID(gameCopy,id);
                        gameCopy.saveHTMLTo(path, sortOrder: sortOrder, showHidden: showHidden, showOfflineHeader: !isOnline, divStates: divStates);
                        rv  = true;
                    }
                    else
                    {
                        throw new Exception($"Error Fetching achievments for {id} - Steam return a failure");
                    }
                }
                catch
                {
                    // Its ok to fail some games have 0 achievments
                    offlineButNoData = true;
                    File.WriteAllText(path, $"<head><style>body {{background-color: rgba(120, 120, 120, 1); color: #eee;}}</style></head><body><h1>Nothing to See Here</h1><h3>{getAppName(id)} has no achievements</h3></body>");
                }
            }

            if (offlineButNoData && !isOnline)
            {
                // Load A Missing Data HTML
                File.WriteAllText(path, $"<head><style>body {{background-color: rgba(120, 120, 120, 1); color: #eee;}}</style></head><body><h1>Offline Mode</h1><h3>No Cached Data for {getAppName(id)}</h3></body>");

            }

            return rv;
        }

        public String getAppName(ulong id)
        {
            return ((appList.Keys.Contains(id)) ? $"{appList[id]}" : $"App ID {id}");
        }
    }

    public class SteamAppListEntry
    {
        public readonly String name;
        public readonly ulong id;

        public SteamAppListEntry(String name, ulong id)
        {
            this.name = name;
            this.id = id;
        }
    }


    public class SteamGame
    {
        public readonly JObject json;
        public readonly ulong id = 0;
        private Dictionary<String, List<SteamAchievment>> achievements = new Dictionary<String, List<SteamAchievment>>();
        public readonly String gameName;
        public readonly ulong playtime;
        private String capsuleImageURL = "";
        private String cacheRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\cache";
        private String AchievmentMapRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\achievementMaps";

        public SteamGame(JObject json, ulong playtimeForever, JArray acheivmentsJson)
        {
            this.json = json;
            id = (ulong)json["steam_appid"];
            playtime = playtimeForever;
            gameName = (String)json["name"];
            capsuleImageURL = (String)json["capsule_image"];

            loadAchievmentsModel(acheivmentsJson);
        }

        public SteamGame(String name, ulong id)
        {
            this.id = id;
            this.gameName = name;
        }

        public Dictionary<String, List<SteamAchievment>> GetAchievments(bool flatten=false)
        {
            return achievements;
        }

        public List<SteamAchievment> GetAllAchievments()
        {
            List<SteamAchievment> achList = new List<SteamAchievment>();
            
            foreach(String key in achievements.Keys)
            {
                achList.AddRange(achievements[key]);
            }

            return achList;
        }

        public Dictionary<String, List<SteamAchievment>> getSortedAcheivements(AchievementSortOrder sortOrder)
        {
            Dictionary<String, List<SteamAchievment>> rv = new Dictionary<string, List<SteamAchievment>>();
            foreach (String key in achievements.Keys)
            {
                List<SteamAchievment> steamAchievmentsUnlocked = achievements[key].Where(c => c.getUnlockStatus()).OrderBy(c => c.displayName).ToList();
                List<SteamAchievment> steamAchievmentsLocked = achievements[key].Where(c => !c.getUnlockStatus()).OrderBy(c => c.displayName).ToList();
                steamAchievmentsUnlocked.AddRange(achievements[key].Where(c => !c.getUnlockStatus()).OrderBy(c => c.displayName));
                steamAchievmentsLocked.AddRange(achievements[key].Where(c => c.getUnlockStatus()).OrderBy(c => c.displayName));

                List<SteamAchievment> steamAchievmentsUnlockedGameOrder = achievements[key].Where(c => c.getUnlockStatus()).ToList();
                List<SteamAchievment> steamAchievmentsLockedGameOrder = achievements[key].Where(c => !c.getUnlockStatus()).ToList();
                steamAchievmentsUnlockedGameOrder.AddRange(achievements[key].Where(c => !c.getUnlockStatus()));
                steamAchievmentsLockedGameOrder.AddRange(achievements[key].Where(c => c.getUnlockStatus()));

                switch (sortOrder)
                {
                    case AchievementSortOrder.ABSOLUTE:
                        rv.Add(key, achievements[key].OrderBy(c => c.qualifiedName).ToList());
                        break;
                    case AchievementSortOrder.NAMEASC:
                        rv.Add(key, achievements[key].OrderBy(c => c.displayName).ToList());
                        break;
                    case AchievementSortOrder.NAMEDESC:
                        rv.Add(key, achievements[key].OrderByDescending(c => c.displayName).ToList());
                        break;
                    case AchievementSortOrder.UNLOCKED:
                        rv.Add(key, steamAchievmentsUnlocked);
                        break;
                    case AchievementSortOrder.LOCKED:
                        rv.Add(key, steamAchievmentsLocked);
                        break;
                    case AchievementSortOrder.UNLOCKEDGAME:
                        rv.Add(key, steamAchievmentsUnlockedGameOrder);
                        break;
                    case AchievementSortOrder.LOCKEDGAME:
                        rv.Add(key, steamAchievmentsLockedGameOrder);
                        break;
                }
            }

            return rv;
        }

        private Boolean isCollapsed(JObject divStates, String headerName)
        {
            return !(divStates == null || (divStates != null && !divStates.ContainsKey($"{id}-{headerName}")) || (divStates != null && divStates.ContainsKey($"{id}-{headerName}") && divStates[$"{id}-{headerName}"].Value<String>() == "expanded"));
        }

        public bool saveHTMLTo(String path, AchievementSortOrder sortOrder = AchievementSortOrder.ABSOLUTE, bool showHidden = false, Boolean showOfflineHeader = false, JObject divStates = null)
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


            html.Add($"<h1 id=\"{gameName}Header\">{gameName} {GetAllAchievments().Where(a => a.getUnlockStatus()).Count()}/{GetAllAchievments().Count} Achievements</h1>");
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
            buttonHTML += "</tr></table>";
            html.Add(buttonHTML);
            html.Add($"<div id=\"{gameName}Div\" class=\"gameHeader\">");

            foreach (String key in achievements.Keys)
            {
                Dictionary<String, List<SteamAchievment>> achievementsMap = getSortedAcheivements(sortOrder);
                if (achievements[key].Count > 0)
                {
                    if (achievements.Keys.Count > 1) 
                    {
                        html.Add($"<h1 id=\"{key}Header\" class=\"dividerHeader\" onclick=\"togglediv('{key}Div', '{key}Header')\">{(isCollapsed(divStates, key) ? "+" : "-")} {key} {achievements[key].Where(a => a.getUnlockStatus()).Count()}/{achievements[key].Count} Acheivements</h1>");
                        html.Add($"<div id=\"{key}Div\" class=\"dividerDiv\" style=\"display: {(isCollapsed(divStates, key) ? "none" : "block")}\">");
                    }

                    html.Add($"<table class=\"achRootTable\">");

                    foreach (SteamAchievment a in achievementsMap[key])
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

        public void loadAchievmentsModel(JArray achievementsJson)
        {
            if (achievementsJson != null)
            {


                JObject achMapObj = new JObject();
                Dictionary<String, List<String>> achMap = new Dictionary<String, List<String>>();
                achievements = new Dictionary<string, List<SteamAchievment>>();
                if (File.Exists(AchievmentMapRootPath + $"\\{id}.json"))
                {
                    achMapObj = JObject.Parse(File.ReadAllText(AchievmentMapRootPath + $"\\{id}.json"));
                    // Flatten Map to List cause lazy
                    foreach (JProperty prop in achMapObj.Properties())
                    {
                        achMap[prop.Name] = prop.Value.Select(ach => (String) ach).ToList();
                        achievements[prop.Name] = new List<SteamAchievment>();
                    }
                }


                
                achievements["Unmapped"] = new List<SteamAchievment>();
                foreach (JObject ach in achievementsJson)
                {
                    SteamAchievment tmp = new SteamAchievment(ach);
                    bool added = false;
                    foreach (String divider in achMap.Keys)
                    {
                        if (achMap[divider].Contains(tmp.qualifiedName))
                        {
                            achievements[divider].Add(tmp);
                            added = true;
                            break;
                        }
                    }
                    if (!added)
                    {
                        achievements["Unmapped"].Add(tmp);
                    }
                }
                
            }
        }

        public void updateAchievmentsStatus(JObject userAchStatusJson)
        {
            try
            {
                foreach (JObject achStatus in userAchStatusJson["playerstats"]["achievements"])
                {
                    try
                    {
                        string qualifiedName = (String)achStatus["apiname"];
                        DateTime unlockedStamp = DateTimeOffset.FromUnixTimeSeconds((long)achStatus["unlocktime"]).UtcDateTime;
                        foreach (String key in achievements.Keys)
                        {
                            SteamAchievment tmp = achievements[key].Where(a => a.qualifiedName == qualifiedName).FirstOrDefault();
                            if (tmp != default(SteamAchievment))
                            {
                                tmp.setUnlockedStatus((ulong)achStatus["achieved"] > 0, unlockedStamp);

                                break;
                            }
                        }
                    }
                    catch
                    {
                        Debug.WriteLine($"Error updating Achiement Status for game {gameName}");
                    } 
                }
            }
            catch
            {

            }
        }

        public string print()
        {
            int totalAchievements = 0;
            int totalUnlocked = 0;
            String rv = "";
            String dividersText = "\n";

            foreach (String key in achievements.Keys)
            {
                if (achievements[key].Count > 0)
                {
                    totalAchievements += achievements[key].Count;
                    totalUnlocked += achievements[key].Where(ach => ach.getUnlockStatus()).Count();
                    bool skipHeaders = false;
                    if (achievements.Keys.Count == 1)
                    {
                        skipHeaders = true;
                    }
                    else
                    {
                        dividersText += $"\t{key} {achievements[key].Where(ach => ach.getUnlockStatus()).Count()}/{achievements[key].Count} Achievements\n";
                    }


                    foreach (SteamAchievment ach in achievements[key])
                    {
                        dividersText += $"\t{((!skipHeaders) ? "\t" : "")}{ach}\n";
                    }
                }
            }

            rv = $"{gameName} - {totalUnlocked}/{totalAchievements} Achievements{dividersText}";
            return rv;
        }

        public override string ToString()
        {
            int totalAchievements = 0;
            int totalUnlocked = 0;
            String rv = "";
            String dividersText = "";

            foreach (String key in achievements.Keys)
            {
                totalAchievements += achievements[key].Count;
                totalUnlocked += achievements[key].Where(ach => ach.getUnlockStatus()).Count();
            }
            
            return $"{gameName}";
        }
    }

    public enum AchievementSortOrder
    {
        ABSOLUTE,
        NAMEASC,
        NAMEDESC,
        UNLOCKED,
        LOCKED,
        UNLOCKEDGAME,
        LOCKEDGAME,
        SHOWMISSABLEONLY,
        MOSTRETROPOINTS,
        LEASTUNLOCKED,
        LEFTTOBEAT
    }

    public class SteamAchievment
    {
        public readonly JObject json;
        private Boolean unlocked = false;
        public readonly String groupName = "";
        public readonly String qualifiedName;
        public readonly String displayName;
        public readonly String description;
        public readonly String lockedImageURL;
        public readonly String unlockedImageURL;
        public readonly Boolean hidden = false;
        private DateTime unlockedTime = DateTimeOffset.FromUnixTimeSeconds(0).UtcDateTime;

        public SteamAchievment(JObject json, String groupName="")
        {
            this.json = json;
            this.groupName = groupName;
            qualifiedName = (String) json["name"];
            displayName = (String) json["displayName"];
            lockedImageURL = (String)json["icongray"];
            unlockedImageURL = (String)json["icon"];
            hidden = ((ulong) json["hidden"]) > 0;

            try 
            {
                description = (String)json["description"];
            }
            catch
            {

            }
            

        }

        public List<String> getHtml(bool showHidden=false)
        {
            List<String> rv = new List<string>();

            rv.Add($"<tr class=\"achRootRow{((unlocked) ? " acRootRowUnlocked" : " acRootRowLocked")}\"><td class=\"achImageCont\"><img class=\"achImage\" src=\"{(showHidden || (!hidden || (hidden && unlocked)) ? ((unlocked) ? unlockedImageURL : lockedImageURL) : "locked.png")}\"></td><td><table class=\"achTextTable\"><tr><td class=\"achTitle\">{(showHidden || (!hidden || (hidden && unlocked)) ? $"{displayName}{((hidden) ? " (Hidden)" : "")}" : "Hidden Trophy")}</td></tr><tr><td class=\"achDescription\">{description}</td></tr><tr><td class=\"achUnlocked\">{(unlocked ? $"Unlocked {unlockedTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")}" : "")}</td></tr></table></td></tr>");

            return rv;
        }
        public void setUnlockedStatus(bool unlocked, DateTime unlockedStamp)
        {
            this.unlocked = unlocked;
            this.unlockedTime = unlockedStamp;
        }

        public Boolean getUnlockStatus()
        {
            return unlocked;
        }

        public DateTime getUnlockTime()
        {
            return unlockedTime;
        }
        public override string ToString()
        {
            return $"{qualifiedName}-{displayName}";
        }
    }
}
