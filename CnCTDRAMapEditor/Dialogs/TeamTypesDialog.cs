//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free 
// software: you can redistribute it and/or modify it under the terms of 
// the GNU General Public License as published by the Free Software Foundation, 
// either version 3 of the License, or (at your option) any later version.

// The Command & Conquer Map Editor and corresponding source code is distributed 
// in the hope that it will be useful, but with permitted additional restrictions 
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT 
// distributed with this program. You should have received a copy of the 
// GNU General Public License along with permitted additional restrictions 
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using MobiusEditor.Controls;
using MobiusEditor.Controls.ControlsList;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class TeamTypesDialog : Form, ListedControlController<TeamTypeClass>, ListedControlController<TeamTypeMission>
    {
        private Bitmap infoImage;
        private string triggerToolTip;

        private const int maxLength = 8;
        private readonly IGamePlugin plugin;
        private readonly int maxTeams;
        private readonly IEnumerable<ITechnoType> technoTypes;
        private readonly List<TeamType> teamTypes;
        private readonly List<TeamType> backupTeamTypes;
        public IEnumerable<TeamType> TeamTypes => teamTypes;

        private ListViewItem SelectedItem => (teamTypesListView.SelectedItems.Count > 0) ? teamTypesListView.SelectedItems[0] : null;

        private TeamType lastEditedTeam = null;
        private TeamType SelectedTeamType => SelectedItem?.Tag as TeamType;

        private TeamItemInfo teamItemInfo;
        private MissionItemInfo missionItemInfo;

        private IEnumerable<TeamMission> teamMissionTypes;
        private ListItem<int>[] wayPoints;
        //private Dictionary<string, string> teamMissionTooltips;
        private ITechnoType defaultTeam;
        private TeamMission defaultMission;
        private ToolTipFixer ttf;

        public TeamTypesDialog(IGamePlugin plugin, int maxTeams)
        {
            this.plugin = plugin;
            this.maxTeams = maxTeams;
            this.technoTypes = plugin.Map.TeamTechnoTypes;

            InitializeComponent();
            lblTooLong.Text = "Teamtype length exceeds " + maxLength + " characters!";
            infoImage = new Bitmap(27, 27);
            using (Graphics g = Graphics.FromImage(infoImage))
            {
                g.DrawIcon(SystemIcons.Information, new Rectangle(0, 0, infoImage.Width, infoImage.Height));
            }
            lblTriggerInfo.Image = infoImage;
            lblTriggerInfo.ImageAlign = ContentAlignment.MiddleCenter;

            int extraWidth = nudRecruitPriority.Width + nudRecruitPriority.Margin.Left + nudRecruitPriority.Margin.Right;
            ttf = new ToolTipFixer(this, toolTip1, 10000, new Dictionary<Type, int> { { typeof(Label), extraWidth } });

            switch (plugin.GameType)
            {
                case GameType.TiberianDawn:
                case GameType.SoleSurvivor:
                    lblTrigger.Visible = cmbTrigger.Visible = false;
                    lblWaypoint.Visible = cmbWaypoint.Visible = false;
                    break;
                case GameType.RedAlert:
                    chbLearning.Visible = false;
                    chbMercenary.Visible = false;
                    break;
            }
            teamTypes = new List<TeamType>();
            backupTeamTypes = new List<TeamType>();
            Waypoint[] wps = plugin.Map.Waypoints;
            this.wayPoints = Enumerable.Range(0, wps.Length).Select(wp => new ListItem<int>(wp, wps[wp].ToString())).ToArray();

            int nrOfTeams = Math.Min(maxTeams, plugin.Map.TeamTypes.Count);
            btnAddTeamType.Enabled = nrOfTeams < maxTeams;
            teamTypesListView.BeginUpdate();
            for (int i = 0; i < nrOfTeams; ++i)
            {
                TeamType teamType = plugin.Map.TeamTypes[i].Clone();
                teamTypes.Add(teamType);
                backupTeamTypes.Add(teamType.Clone());
                var item = new ListViewItem(teamType.Name)
                {
                    Tag = teamType
                };
                item.SubItems.Add(teamType.House.Name);
                teamTypesListView.Items.Add(item).ToolTipText = teamType.Name;
            }
            teamTypesListView.EndUpdate();

            cmbHouse.DataSource = plugin.Map.Houses.Select(t => new TypeItem<HouseType>(t.Type.Name, t.Type)).ToArray();
            cmbWaypoint.DataSource = new ListItem<int>(-1, Waypoint.None).Yield().Concat(wayPoints).ToArray();
            cmbWaypoint.ValueMember = "Value";
            cmbWaypoint.DisplayMember = "Label";

            string[] items = plugin.Map.FilterUnitTriggers().Select(t => t.Name).Distinct().ToArray();
            string[] filteredEvents = plugin.Map.EventTypes.Where(ev => plugin.Map.UnitEventTypes.Contains(ev)).Distinct().ToArray();
            string[] filteredActions = plugin.Map.ActionTypes.Where(ac => plugin.Map.UnitActionTypes.Contains(ac)).Distinct().ToArray();
            triggerToolTip = Map.MakeAllowedTriggersToolTip(filteredEvents, filteredActions);
            HashSet<string> allowedTriggers = new HashSet<string>(items);
            items = Trigger.None.Yield().Concat(plugin.Map.Triggers.Select(t => t.Name).Where(t => allowedTriggers.Contains(t)).Distinct()).ToArray();
            cmbTrigger.DataSource = items;

            defaultTeam = technoTypes.FirstOrDefault();
            // Fix for case sensitivity issue in teamtype missions
            
            TeamMission[] missions = plugin.Map.TeamMissionTypes;
            this.teamMissionTypes = missions.ToArray();
            this.defaultMission = missions.FirstOrDefault();
            teamTypeTableLayoutPanel.Visible = false;
        }

        private void LblTriggerInfo_MouseEnter(Object sender, EventArgs e)
        {
            Control target = sender as Control;
            ShowToolTip(target, triggerToolTip);
        }

        private void ShowToolTip(Control target, string message)
        {

            if (target == null || message == null)
            {
                this.toolTip1.Hide(target);
                return;
            }
            Point resPoint = target.PointToScreen(new Point(0, target.Height));
            MethodInfo m = toolTip1.GetType().GetMethod("SetTool",
                       BindingFlags.Instance | BindingFlags.NonPublic);
            // private void SetTool(IWin32Window win, string text, TipInfo.Type type, Point position)
            m.Invoke(toolTip1, new object[] { target, message, 2, resPoint });
            //this.toolTip1.Show(triggerToolTip, target, target.Width, 0, 10000);
        }

        private void LblTriggerInfo_MouseLeave(Object sender, EventArgs e)
        {
            Control target = sender as Control;
            this.toolTip1.Hide(target);
        }

        private void TeamTypesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lastEditedTeam != null)
            {
                OptimizeTeams(lastEditedTeam);
            }
            cmbHouse.DataBindings.Clear();
            chbRoundabout.DataBindings.Clear();
            chbLearning.DataBindings.Clear();
            chbSuicide.DataBindings.Clear();
            chbAutocreate.DataBindings.Clear();
            chbMercenary.DataBindings.Clear();
            chbReinforcable.DataBindings.Clear();
            chbPrebuilt.DataBindings.Clear();
            nudRecruitPriority.DataBindings.Clear();
            nudInitNum.DataBindings.Clear();
            maxAllowedNud.DataBindings.Clear();
            nudFear.DataBindings.Clear();
            cmbWaypoint.DataBindings.Clear();
            cmbTrigger.DataBindings.Clear();
            teamItemInfo = null;
            missionItemInfo = null;

            TeamType selected = SelectedTeamType;
            lastEditedTeam = selected;
            lblTooLong.Visible = SelectedTeamType != null && SelectedTeamType.Name != null && SelectedTeamType.Name.Length > maxLength;
            if (selected != null)
            {
                cmbHouse.DataBindings.Add("SelectedValue", selected, "House");
                chbRoundabout.DataBindings.Add("Checked", selected, "IsRoundAbout");
                chbLearning.DataBindings.Add("Checked", selected, "IsLearning");
                chbSuicide.DataBindings.Add("Checked", selected, "IsSuicide");
                chbAutocreate.DataBindings.Add("Checked", selected, "IsAutocreate");
                chbMercenary.DataBindings.Add("Checked", selected, "IsMercenary");
                chbReinforcable.DataBindings.Add("Checked", selected, "IsReinforcable");
                chbPrebuilt.DataBindings.Add("Checked", selected, "IsPrebuilt");
                selected.RecruitPriority = CheckBounds(selected.RecruitPriority, nudRecruitPriority);
                nudRecruitPriority.DataBindings.Add("Value", selected, "RecruitPriority");
                selected.InitNum = CheckBounds(selected.InitNum, nudInitNum);
                nudInitNum.DataBindings.Add("Value", selected, "InitNum");
                selected.MaxAllowed = CheckBounds(selected.MaxAllowed, maxAllowedNud);
                maxAllowedNud.DataBindings.Add("Value", selected, "MaxAllowed");
                selected.Fear = CheckBounds(selected.Fear, nudFear);
                nudFear.DataBindings.Add("Value", selected, "Fear");
                cmbWaypoint.DataBindings.Add("SelectedValue", selected, "Origin"); 
                //cmbWaypoint.DataBindings.Add("SelectedIndex", selected, "Origin");
                cmbTrigger.DataBindings.Add("SelectedItem", selected, "Trigger");

                teamItemInfo = new TeamItemInfo(null, selected.Classes, technoTypes);
                tilTeams.Populate(teamItemInfo, this);
                tilTeams.TabStop = selected.Classes.Count > 0;
                missionItemInfo = new MissionItemInfo(null, selected.Missions, teamMissionTypes, this.wayPoints, plugin.Map.Metrics.Length, toolTip1);
                milMissions.Populate(missionItemInfo, this);
                milMissions.TabStop = selected.Missions.Count > 0;

                teamTypeTableLayoutPanel.Visible = true;
            }
            else
            {
                teamTypeTableLayoutPanel.Visible = false;
            }
        }

        private byte CheckBounds(byte value, NumericUpDown nud)
        {
            return (byte)Math.Min(Byte.MaxValue, Math.Max(0, CheckBounds((int)value, nud)));
        }

        private int CheckBounds(int value, NumericUpDown nud)
        {
            return Math.Min((int)nud.Maximum, Math.Max((int)nud.Minimum, value));
        }

        private void TeamTypesListView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = teamTypesListView.HitTest(e.Location);
                bool itemExists = hitTest.Item != null;
                addTeamTypeToolStripMenuItem.Visible = true;
                addTeamTypeToolStripMenuItem.Enabled = teamTypesListView.Items.Count < maxTeams;
                renameTeamTypeToolStripMenuItem.Visible = itemExists;
                removeTeamTypeToolStripMenuItem.Visible = itemExists;
                teamTypesContextMenuStrip.Show(Cursor.Position);
            }
        }

        private void TeamTypesListView_KeyDown(Object sender, KeyEventArgs e)
        {
            ListViewItem selected = SelectedItem;
            if (e.KeyData == Keys.F2)
            {
                if (selected != null)
                    selected.BeginEdit();
            }
            else if (e.KeyData == Keys.Delete)
            {
                RemoveTeamType();
            }
        }

        private void TeamTypesDialog_KeyDown(Object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.A | Keys.Control))
            {
                AddTeamType();
            }
        }

        private void BtnAddTeamType_Click(object sender, EventArgs e)
        {
            AddTeamType();
        }

        private void TeamTypesDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If user pressed ok, nevermind,just go on.
            if (this.DialogResult == DialogResult.OK)
            {
                // Remove all 0-items from teams, optimise types.
                foreach (TeamType team in teamTypes)
                {
                    OptimizeTeams(team);
                }
                return;
            }
            bool hasChanges = teamTypes.Count != backupTeamTypes.Count;
            if (!hasChanges)
            {
                foreach (TeamType team in teamTypes)
                {
                    TeamType oldTeam = backupTeamTypes.Find(t => t.Name.Equals(team.Name));
                    if (oldTeam == null)
                    {
                        hasChanges = true;
                        break;
                    }
                    hasChanges = !team.EqualsOther(oldTeam);
                    if (hasChanges)
                        break;
                }
            }
            if (hasChanges)
            {
                DialogResult dr = MessageBox.Show("Teams have been changed! Are you sure you want to cancel?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                    return;
                this.DialogResult = DialogResult.None;
                e.Cancel = true;
            }
        }

        private void OptimizeTeams(TeamType team)
        {
            Dictionary<ITechnoType, byte> counts = new Dictionary<ITechnoType, byte>();
            List<TeamTypeClass> firstTypeClasses = new List<TeamTypeClass>();
            foreach (TeamTypeClass ttClass in team.Classes)
            {
                if (ttClass.Type == null)
                {
                    continue;
                }
                if (!counts.ContainsKey(ttClass.Type))
                {
                    firstTypeClasses.Add(ttClass);
                    counts.Add(ttClass.Type, ttClass.Count);
                }
                else
                {
                    counts[ttClass.Type] += ttClass.Count;
                }
            }
            team.Classes.Clear();
            foreach (TeamTypeClass ttc in firstTypeClasses)
            {
                byte count = counts[ttc.Type];
                if (count != 0)
                {
                    ttc.Count = count;
                    team.Classes.Add(ttc);
                }
            }
        }

        private void AddTeamTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddTeamType();
        }

        private void renameTeamTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedItem != null)
                SelectedItem.BeginEdit();
        }

        private void RemoveTeamTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveTeamType();
        }

        private void AddTeamType()
        {
            if (teamTypesListView.Items.Count >= maxTeams)
                return;
            string name = GeneralUtils.MakeNew4CharName(teamTypes.Select(t => t.Name), "????", TeamType.None);
            var teamType = new TeamType { Name = name, House = plugin.Map.HouseTypes.First() };
            var item = new ListViewItem(teamType.Name)
            {
                Tag = teamType
            };
            item.SubItems.Add(teamType.House.Name);
            teamTypes.Add(teamType);
            teamTypesListView.Items.Add(item).ToolTipText = teamType.Name;
            btnAddTeamType.Enabled = teamTypes.Count < maxTeams;
            CalcListColSizes();
            item.Selected = true;
            teamTypesListView.SelectedItems.Cast<ListViewItem>().FirstOrDefault()?.EnsureVisible();
            item.BeginEdit();
        }

        private void RemoveTeamType()
        {
            ListViewItem selected = SelectedItem;
            int index = teamTypesListView.SelectedIndices.Count == 0 ? -1 : teamTypesListView.SelectedIndices[0];
            if (selected != null)
            {
                teamTypes.Remove(selected.Tag as TeamType);
                teamTypesListView.Items.Remove(selected);
            }
            if (teamTypesListView.Items.Count == index)
                index--;
            if (index >= 0 && teamTypesListView.Items.Count > index)
                teamTypesListView.Items[index].Selected = true;
            btnAddTeamType.Enabled = teamTypes.Count < maxTeams;
            CalcListColSizes();
        }

        private void TeamTypesListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            String curName = e.Label;
            if (string.IsNullOrEmpty(curName))
            {
                e.CancelEdit = true;
            }
            else if (curName.Length > maxLength)
            {
                e.CancelEdit = true;
                MessageBox.Show(string.Format("Team name is longer than {0} characters.", maxLength), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (TeamType.None.Equals(curName, StringComparison.InvariantCultureIgnoreCase))
            {
                e.CancelEdit = true;
                MessageBox.Show(string.Format("Team name 'None' is reserved and cannot be used."), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!INIHelpers.IsValidKey(curName))
            {
                e.CancelEdit = true;
                MessageBox.Show(string.Format("Team name '{0}' contains illegal characters. This format only supports simple ASCII, and cannot contain '=', '[' or ']'.", curName), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (teamTypes.Where(t => (t != SelectedTeamType) && t.Name.Equals(curName, StringComparison.InvariantCultureIgnoreCase)).Any())
            {
                e.CancelEdit = true;
                MessageBox.Show(string.Format("Team with name '{0}' already exists.", curName.ToUpperInvariant()), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                SelectedTeamType.Name = curName;
                teamTypesListView.Items[e.Item].ToolTipText = SelectedTeamType.Name;
            }
        }

        public void UpdateControlInfo(TeamTypeClass updateInfo)
        {
            // Detect delete signal
            if (SelectedTeamType != null && updateInfo.Type == null)
            {
                // Still the same object reference, so this should be found.
                int index = SelectedTeamType.Classes.IndexOf(updateInfo);
                SelectedTeamType.Classes.Remove(updateInfo);
                // Reset list controller with new list
                teamItemInfo = new TeamItemInfo(null, SelectedTeamType.Classes, technoTypes);
                tilTeams.Populate(teamItemInfo, this);
                btnAddTeam.Enabled = SelectedTeamType.Classes.Count < Globals.MaxTeamClasses;
                int teams = SelectedTeamType.Classes.Count;
                if (teams > 0)
                {
                    index = Math.Min(index, teams - 1);
                    TeamItemControl newCtrl = teamItemInfo.GetControlByProperty(SelectedTeamType.Classes[index], tilTeams.Contents);
                    pnlTeamsScroll.ScrollControlIntoView(newCtrl);
                }
                tilTeams.TabStop = teams > 0;
            }
        }

        public void UpdateControlInfo(TeamTypeMission updateInfo)
        {
            // Detect delete signal
            if (SelectedTeamType != null && updateInfo.Mission == null)
            {
                // Still the same object reference, so this should be found.
                int index = SelectedTeamType.Missions.IndexOf(updateInfo);
                SelectedTeamType.Missions.Remove(updateInfo);
                // Reset list controller with new list
                missionItemInfo = new MissionItemInfo(null, SelectedTeamType.Missions, teamMissionTypes, this.wayPoints, plugin.Map.Metrics.Length, toolTip1);
                milMissions.Populate(missionItemInfo, this);
                btnAddMission.Enabled = SelectedTeamType.Missions.Count < Globals.MaxTeamMissions;
                int missions = SelectedTeamType.Missions.Count;
                if (missions > 0)
                {
                    index = Math.Min(index, missions - 1);
                    MissionItemControl newCtrl = missionItemInfo.GetControlByProperty(SelectedTeamType.Missions[index], milMissions.Contents);
                    pnlTeamsScroll.ScrollControlIntoView(newCtrl);
                }
                tilTeams.TabStop = missions > 0;
                milMissions.TabStop = missions > 0;
            }
        }

        private void BtnAddTeam_Click(Object sender, EventArgs e)
        {
            if (SelectedTeamType != null)
            {
                if (SelectedTeamType.Classes.Count <= Globals.MaxTeamClasses)
                {
                    TeamTypeClass newItem = new TeamTypeClass() { Type = defaultTeam, Count = 0 };
                    SelectedTeamType.Classes.Add(newItem);
                    teamItemInfo = new TeamItemInfo(null, SelectedTeamType.Classes, technoTypes);
                    tilTeams.Populate(teamItemInfo, this);
                    TeamItemControl newCtrl = teamItemInfo.GetControlByProperty(newItem, tilTeams.Contents);
                    pnlTeamsScroll.ScrollControlIntoView(newCtrl);
                }
                btnAddTeam.Enabled = SelectedTeamType.Classes.Count < Globals.MaxTeamClasses;
                tilTeams.TabStop = SelectedTeamType.Classes.Count > 0;
 
            }
        }

        private void BtnAddMission_Click(Object sender, EventArgs e)
        {
            if (SelectedTeamType != null)
            {
                if (SelectedTeamType.Missions.Count <= Globals.MaxTeamMissions)
                {
                    TeamTypeMission newItem = new TeamTypeMission() { Mission = defaultMission, Argument = -1 };
                    SelectedTeamType.Missions.Add(newItem);
                    missionItemInfo = new MissionItemInfo(null, SelectedTeamType.Missions, teamMissionTypes, this.wayPoints, plugin.Map.Metrics.Length, toolTip1);
                    milMissions.Populate(missionItemInfo, this);
                    MissionItemControl newCtrl = missionItemInfo.GetControlByProperty(newItem, milMissions.Contents);
                    pnlMissionsScroll.ScrollControlIntoView(newCtrl);
                }
                milMissions.TabStop = SelectedTeamType.Missions.Count > 0;
                btnAddMission.Enabled = SelectedTeamType.Missions.Count < Globals.MaxTeamMissions;
            }

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            ttf.Dispose();
            if (disposing && (components != null))
            {
                try
                {
                    lblTriggerInfo.Image = null;
                }
                catch { /*ignore*/}
                try
                {
                    infoImage.Dispose();
                    infoImage = null;
                }
                catch { /*ignore*/}
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void TeamTypesDialog_Resize(Object sender, EventArgs e)
        {
            CalcListColSizes();
        }
        
        private void teamTypesListView_ColumnWidthChanging(Object sender, ColumnWidthChangingEventArgs e)
        {
            e.Cancel = true;
            //CalcListColSizes();
            e.NewWidth = teamTypesListView.Columns[e.ColumnIndex].Width;
        }


        private void CalcListColSizes()
        {
            int amount = teamTypesListView.Columns.Count;
            int width = teamTypesListView.ClientSize.Width;
            int colWidth = width / amount;
            int remainder = width % amount;
            foreach (ColumnHeader col in teamTypesListView.Columns)
            {
                int cwidth = colWidth;
                if (remainder > 0)
                {
                    width += remainder--;
                }
                col.Width = cwidth;
            }
        }

        private void cmbHouse_SelectedValueChanged(Object sender, EventArgs e)
        {
            if (teamTypesListView.SelectedItems.Count == 0 || !(cmbHouse.SelectedItem is TypeItem<HouseType> selectedHouse))
            {
                return;
            }
            ListViewItem item = teamTypesListView.SelectedItems[0];
            if (item.SubItems.Count > 1 && item.SubItems[1].Text != selectedHouse.Name)
            {
                item.SubItems[1].Text = selectedHouse.Name;
            }
        }

        private void TeamTypesDialog_Shown(Object sender, EventArgs e)
        {
            CalcListColSizes();
        }
    }
}
