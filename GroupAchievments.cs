using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SteamAchievmentViewer
{
    public partial class GroupAchievments : Form
    {
        private Steam steamClient;
        ulong appid = 0;
        Dictionary<String, List<SteamAchievment>> achMap = new Dictionary<string, List<SteamAchievment>>();
        private String mapPath = "";
        public GroupAchievments(Steam steamClient, ulong appid)
        {
            InitializeComponent();
            this.steamClient = steamClient;
            this.appid = appid;
            mapPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"\\achievementMaps\\{appid}.json";

            if (steamClient != null)
            {
                this.Text = $"Acheivement Map Creator - {steamClient.getAppName(appid)}";
            }
            

            this.BackColor = Color.FromArgb(80, 84, 82);
            this.ForeColor = Color.White;

            unmappedListBox.BackColor = Color.FromArgb(120, 120, 120);
            unmappedListBox.ForeColor = Color.White;

            mappedTreeView.BackColor = Color.FromArgb(120, 120, 120);
            mappedTreeView.ForeColor = Color.White;

            selectAllBtn.BackColor = Color.FromArgb(120, 120, 120);
            selectAllBtn.ForeColor = Color.White;

            selectAllBtn.BackColor = Color.FromArgb(120, 120, 120);
            selectAllBtn.ForeColor = Color.White;
            selectNoneBtn.BackColor = Color.FromArgb(120, 120, 120);
            selectNoneBtn.ForeColor = Color.White;
            saveAchievementBtn.BackColor = Color.FromArgb(120, 120, 120);
            saveAchievementBtn.ForeColor = Color.White;
            deleteBtn.BackColor = Color.FromArgb(120, 120, 120);
            deleteBtn.ForeColor = Color.White;
            addBtn.BackColor = Color.FromArgb(120, 120, 120);
            addBtn.ForeColor = Color.White;
            addNameTextBox.BackColor = Color.FromArgb(120, 120, 120);
            addNameTextBox.ForeColor = Color.White;
        }

        private void GroupAchievments_Shown(object sender, EventArgs e)
        {
            if (steamClient != null)
            {
                achMap = steamClient.getAchievmentsForApp(appid);
                if (achMap.Keys.Count > 0)
                {
                    foreach (String key in achMap.Keys)
                    {
                        if (key == "Unmapped")
                        {
                            foreach (SteamAchievment ach in achMap[key])
                            {
                                unmappedListBox.Items.Add(ach);
                            }
                        }
                        else
                        {

                            TreeNode parentNode = new TreeNode(key);
                            foreach (SteamAchievment ach in achMap[key])
                            {
                                parentNode.Nodes.Add(new TreeNode(ach.ToString()));
                            }
                            mappedTreeView.Nodes.Add(parentNode);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("", $"Error Loading Achievements for {steamClient.getAppName(appid)}");
                    Close();
                }
            }
        }

        private String getCleanNodeName(TreeNode node)
        {
            return Regex.Replace(node.Text, @"( \([0-9]+ Achievements\)$)", $"", RegexOptions.Multiline);
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            if (addNameTextBox.Text.Length > 0 && unmappedListBox.CheckedItems.Count > 0)
            {
                TreeNode parentNode = new TreeNode(addNameTextBox.Text + " (0 Achievements)");
                List<TreeNode> nodes = new List<TreeNode>();
                foreach (SteamAchievment ach in unmappedListBox.CheckedItems)
                {
                    nodes.Add(new TreeNode(ach.ToString()));
                }

                int matchingNode = -1;

                for (int i = 0; i < mappedTreeView.Nodes.Count; i++)
                {
                    if (getCleanNodeName(mappedTreeView.Nodes[i]) == addNameTextBox.Text)
                    {
                        matchingNode = i;
                        break;
                    }
                }

                if (matchingNode >= 0)
                {
                    mappedTreeView.Nodes[matchingNode].Nodes.AddRange(nodes.ToArray());
                    parentNode = mappedTreeView.Nodes[matchingNode];
                }
                else
                {
                    parentNode.Nodes.AddRange(nodes.ToArray());
                    mappedTreeView.Nodes.Add(parentNode);
                }

                parentNode.Text = Regex.Replace(parentNode.Text, @"( \([0-9]+ Achievements\)$)", $" ({parentNode.Nodes.Count} Achievements)", RegexOptions.Multiline);

                while (unmappedListBox.CheckedItems.Count > 0)
                {
                    unmappedListBox.Items.RemoveAt(unmappedListBox.CheckedIndices[0]);
                }
            }
            else
            {
                MessageBox.Show("Enter a Name for the Achievement Group and Select Some Achievements");
            }
        }

        private SteamAchievment getAchievementForString(String achStr)
        {
            SteamAchievment ach = null;
            try
            {
                String qualString = achStr.Split(new char[] { '-' })[0].Trim();

                if (achMap != null)
                {
                    foreach (String key in achMap.Keys)
                    {
                        foreach (SteamAchievment a in achMap[key])
                        {
                            if (a.qualifiedName == qualString)
                            {
                                ach = a;
                                break;
                            }

                        }
                    }
                }
            }
            catch
            {

            }


            return ach;
        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            for (int i = mappedTreeView.Nodes.Count - 1; i >= 0; i--)
            {
                TreeNode node = mappedTreeView.Nodes[i];
                for (int j = node.Nodes.Count - 1; j >= 0; j--)
                {
                    TreeNode innerNode = node.Nodes[j];
                    if (innerNode.Checked)
                    {
                        SteamAchievment a = getAchievementForString(innerNode.Text);
                        if (a != null)
                        {
                            unmappedListBox.Items.Add(a);
                            innerNode.Remove();
                        }
                    }
                }
                if (node.Checked && node.Nodes.Count == 0)
                {
                    node.Remove();
                }
            }
        }

        private void mappedTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            mappedTreeView.AfterCheck -= mappedTreeView_AfterCheck;
            if (e.Node.Nodes.Count > 0)
            {
                foreach (TreeNode tn in e.Node.Nodes)
                {
                    tn.Checked = e.Node.Checked;
                }

            }
            else
            {
                if (e.Node.Parent != null)
                {
                    bool allChecked = true;
                    foreach (TreeNode n in e.Node.Parent.Nodes)
                    {
                        if (!n.Checked)
                        {
                            allChecked = false;
                        }
                    }

                    e.Node.Parent.Checked = allChecked;
                }
            }
            mappedTreeView.AfterCheck += mappedTreeView_AfterCheck;
        }

        private void mappedTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            e.Node.Checked = !e.Node.Checked;
            mappedTreeView.AfterSelect -= mappedTreeView_AfterSelect;
            mappedTreeView.SelectedNode = null;
            mappedTreeView.AfterSelect += mappedTreeView_AfterSelect;
        }

        private void saveAchievementBtn_Click(object sender, EventArgs e)
        {

            if (unmappedListBox.Items.Count > 0 && mappedTreeView.Nodes.Count > 0)
            {
                MessageBox.Show($"There are Unmapped Acheievemnts. Please Map all Achievements", "Unmapped Achievements Error");
            }
            else
            {
                DialogResult res = MessageBox.Show($"Save Achievment Map for {steamClient.getAppName(appid)}?", "Confirm", MessageBoxButtons.YesNo);

                if (res == DialogResult.Yes)
                {
                    JObject obj = new JObject();

                    foreach (TreeNode node in mappedTreeView.Nodes)
                    {
                        List<String> qualNameArray = new List<string>();
                        foreach (TreeNode innerNode in node.Nodes)
                        {
                            qualNameArray.Add(innerNode.Text.Split(new char[] { '-' })[0].Trim());
                        }

                        obj[getCleanNodeName(node)] = new JArray(qualNameArray);
                    }

                    File.WriteAllText(mapPath, obj.ToString());
                    Close();
                }
            }
        }

        private void selectNoneBtn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < unmappedListBox.Items.Count; i++)
            {
                unmappedListBox.SetItemChecked(i, false);
            }
        }

        private void selectAllBtn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < unmappedListBox.Items.Count; i++)
            {
                unmappedListBox.SetItemChecked(i, true);
            }
        }
    }
}
