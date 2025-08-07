//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// The Command & Conquer Map Editor and corresponding source code is distributed
// in the hope that it will be useful, but with permitted additional restrictions
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT
// distributed with this program. You should have received a copy of the
// GNU General Public License along with permitted additional restrictions
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using MobiusEditor.Controls;
using MobiusEditor.Controls.ControlsList;
using MobiusEditor.Event;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class TeamTypesDialog : Form, ListedControlController<TeamTypeClass>, ListedControlController<TeamTypeMission>
    {
        private Bitmap infoImage;
        private Control tooltipShownOn;
        private string triggerInfoToolTip;
        private string triggerToolTip;
        private string[] filteredEvents;
        private string[] filteredActions;

        private const int maxLength = 8;
        private readonly IGamePlugin plugin;
        private readonly int maxTeams;
        private readonly IEnumerable<ITechnoType> technoTypes;
        private readonly List<TeamType> teamTypes;
        private readonly List<TeamType> backupTeamTypes;
        public IEnumerable<TeamType> TeamTypes => teamTypes;

        private readonly List<(string Name1, string Name2)> renameActions;
        public List<(string Name1, string Name2)> RenameActions => renameActions;

        private ListViewItem SelectedItem => (teamTypesListView.SelectedItems.Count > 0) ? teamTypesListView.SelectedItems[0] : null;

        private TeamType lastEditedTeam = null;
        private TeamType SelectedTeamType => SelectedItem?.Tag as TeamType;

        private TeamItemInfo teamItemInfo;
        private MissionItemInfo missionItemInfo;

        private readonly IEnumerable<TeamMission> teamMissionTypes;
        private readonly ListItem<int>[] wayPoints;
        private readonly ITechnoType defaultTeam;
        private readonly TeamMission defaultMission;
        private readonly ToolTipFixer ttf;
        private string initialTeam;

        public static void ShowTeamTypesEditor(IWin32Window owner, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> url, string teamtypeName)
        {
            using (TeamTypesDialog ttd = new TeamTypesDialog(plugin, teamtypeName))
            {
                ttd.StartPosition = FormStartPosition.CenterParent;
                if (ttd.ShowDialog(owner) == DialogResult.OK)
                {
                    List<TeamType> oldTeamTypes = plugin.Map.TeamTypes.ToList();
                    // Clone of old triggers
                    List<Trigger> oldTriggers = plugin.Map.Triggers.Select(tr => tr.Clone()).ToList();
                    plugin.Map.TeamTypes.Clear();
                    plugin.Map.ApplyTeamTypeRenames(ttd.RenameActions);
                    // Triggers in their new state after the teamtype item renames.
                    List<Trigger> newTriggers = plugin.Map.Triggers.Select(tr => tr.Clone()).ToList();
                    plugin.Map.TeamTypes.AddRange(ttd.TeamTypes.OrderBy(t => t.Name, new ExplorerComparer()).Select(t => t.Clone()));
                    List<TeamType> newTeamTypes = plugin.Map.TeamTypes.ToList();
                    bool origEmptyState = plugin.Empty;
                    void undoAction(UndoRedoEventArgs ev)
                    {
                        DialogResult dr = MessageBox.Show(ev.MapPanel, "This will undo all teamtype editing actions you performed. Are you sure you want to continue?",
                            "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (dr == DialogResult.No)
                        {
                            ev.Cancelled = true;
                        }
                        else if (ev.Plugin != null)
                        {
                            ev.Map.Triggers = oldTriggers;
                            ev.Map.TeamTypes.Clear();
                            ev.Map.TeamTypes.AddRange(oldTeamTypes);
                            ev.Plugin.Empty = origEmptyState;
                            ev.Plugin.Dirty = !ev.NewStateIsClean;
                        }
                    }
                    void redoAction(UndoRedoEventArgs ev)
                    {
                        DialogResult dr = MessageBox.Show(ev.MapPanel, "This will redo all teamtype editing actions you undid. Are you sure you want to continue?",
                            "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (dr == DialogResult.No)
                        {
                            ev.Cancelled = true;
                        }
                        else if (ev.Plugin != null)
                        {
                            ev.Map.TeamTypes.Clear();
                            ev.Map.TeamTypes.AddRange(newTeamTypes);
                            ev.Map.Triggers = newTriggers;
                            // Redo can never restore the "empty" state, but CAN be the point at which a save was done.
                            ev.Plugin.Empty = false;
                            ev.Plugin.Dirty = !ev.NewStateIsClean;
                        }
                    }
                    url.Track(undoAction, redoAction, ToolType.Waypoint);
                    plugin.Dirty = true;
                }
            }
        }

        public TeamTypesDialog(IGamePlugin plugin, string selectteam)
        {
            initialTeam = selectteam;
            this.plugin = plugin;
            maxTeams = plugin.GameInfo.MaxTeams;
            technoTypes = plugin.Map.TeamTechnoTypes;

            InitializeComponent();
            lblTooLong.Text = "Teamtype length exceeds " + maxLength + " characters!";
            int extraWidthDropdowns = nudRecruitPriority.Width + nudRecruitPriority.Margin.Left + nudRecruitPriority.Margin.Right;
            int extraWidthCheckboxes = nudRecruitPriority.Width - chbAutocreate.Width;
            ttf = new ToolTipFixer(this, toolTip1, 10000, new Dictionary<Type, int>
            {
                { typeof(Label), extraWidthDropdowns },
                { typeof(CheckBox), extraWidthCheckboxes }
            });
            switch (plugin.GameInfo.GameType)
            {
                case GameType.TiberianDawn:
                case GameType.SoleSurvivor:
                    lblTrigger.Visible = cmbTrigger.Visible = lblTriggerInfo.Visible = false;
                    lblWaypoint.Visible = cmbWaypoint.Visible = false;
                    break;
                case GameType.RedAlert:
                    infoImage = InitTriggerInfoImage(lblTriggerInfo);
                    chbLearning.Visible = false;
                    chbMercenary.Visible = false;
                    break;
            }
            teamTypes = new List<TeamType>();
            renameActions = new List<(string Name1, string Name2)>();
            backupTeamTypes = new List<TeamType>();
            Waypoint[] wps = plugin.Map.Waypoints;
            wayPoints = Enumerable.Range(0, wps.Length).Select(wp => new ListItem<int>(wp, wps[wp].ToString())).ToArray();

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

            cmbHouse.DataSource = plugin.Map.Houses.Select(t => new ListItem<HouseType>(t.Type, t.Type.Name)).ToArray();
            cmbWaypoint.DataSource = new ListItem<int>(-1, Waypoint.None).Yield().Concat(wayPoints).ToArray();
            cmbWaypoint.ValueMember = "Value";
            cmbWaypoint.DisplayMember = "Label";

            string[] items = plugin.Map.FilterUnitTriggers().Select(t => t.Name).Distinct().ToArray();
            filteredEvents = plugin.Map.EventTypes.Where(ev => plugin.Map.UnitEventTypes.Contains(ev)).Distinct().ToArray();
            filteredActions = plugin.Map.ActionTypes.Where(ac => plugin.Map.UnitActionTypes.Contains(ac)).Distinct().ToArray();
            triggerInfoToolTip = Map.MakeAllowedTriggersToolTip(filteredEvents, filteredActions);
            HashSet<string> allowedTriggers = new HashSet<string>(items);
            items = Trigger.None.Yield().Concat(plugin.Map.Triggers.Select(t => t.Name).Where(t => allowedTriggers.Contains(t)).Distinct()).ToArray();
            cmbTrigger.DataSource = items;
            defaultTeam = technoTypes.FirstOrDefault();
            // Fix for case sensitivity issue in teamtype missions
            TeamMission[] missions = plugin.Map.TeamMissionTypes;
            teamMissionTypes = missions.ToArray();
            defaultMission = missions.FirstOrDefault();
            teamTypeTableLayoutPanel.Visible = false;
        }

        private Bitmap InitTriggerInfoImage(Label label)
        {
            Bitmap image = new Bitmap(27, 27);
            image.SetResolution(96, 96);
            using (Graphics g = Graphics.FromImage(image))
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawIcon(SystemIcons.Information, new Rectangle(0, 0, image.Width, image.Height));
            }
            label.Image = image;
            label.ImageAlign = ContentAlignment.MiddleCenter;
            return image;
        }

        private void CmbTrigger_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = cmbTrigger.SelectedItem as string;
            Trigger trig = plugin.Map.Triggers.FirstOrDefault(t => String.Equals(t.Name, selected, StringComparison.OrdinalIgnoreCase));
            triggerInfoToolTip = Map.MakeAllowedTriggersToolTip(filteredEvents, filteredActions, trig);
            triggerToolTip = plugin.TriggerSummary(trig, true, false);
            Point pt = MousePosition;
            Point lblPos = lblTriggerInfo.PointToScreen(Point.Empty);
            Point cmbPos = cmbTrigger.PointToScreen(Point.Empty);
            Rectangle lblInfoRect = new Rectangle(lblPos, lblTriggerInfo.Size);
            Rectangle cmbTrigRect = new Rectangle(cmbPos, cmbTrigger.Size);
            if (lblInfoRect.Contains(pt))
            {
                toolTip1.Hide(lblTriggerInfo);
                LblTriggerInfo_MouseEnter(lblTriggerInfo, e);
            }
            else if (cmbTrigRect.Contains(pt))
            {
                toolTip1.Hide(cmbTrigger);
                CmbTrigger_MouseEnter(cmbTrigger, e);
            }
        }

        private void CmbTrigger_MouseEnter(object sender, EventArgs e)
        {
            Control target = sender as Control;
            ShowToolTip(target, triggerToolTip);
        }

        private void CmbTrigger_MouseMove(object sender, MouseEventArgs e)
        {
            if (tooltipShownOn != sender)
            {
                CmbTrigger_MouseEnter(sender, e);
            }
        }

        private void LblTriggerInfo_MouseEnter(object sender, EventArgs e)
        {
            Control target = sender as Control;
            ShowToolTip(target, triggerInfoToolTip);
        }

        private void LblTriggerInfo_MouseMove(object sender, MouseEventArgs e)
        {
            if (tooltipShownOn != sender)
            {
                LblTriggerInfo_MouseEnter(sender, e);
            }
        }

        private void ShowToolTip(Control target, string message)
        {
            if (target == null || message == null)
            {
                HideToolTip(target, null);
                return;
            }
            Point resPoint = target.PointToScreen(new Point(0, target.Height));
            MethodInfo m = toolTip1.GetType().GetMethod("SetTool",
                       BindingFlags.Instance | BindingFlags.NonPublic);
            m.Invoke(toolTip1, new object[] { target, message, 2, resPoint });
            tooltipShownOn = target;
        }

        private void HideToolTip(object sender, EventArgs e)
        {
            try
            {
                if (tooltipShownOn != null)
                {
                    toolTip1.Hide(tooltipShownOn);
                }
                if (sender is Control target)
                {
                    toolTip1.Hide(target);
                }
            }
            catch { /* ignore */ }
            tooltipShownOn = null;
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
                missionItemInfo = new MissionItemInfo(null, selected.Missions, teamMissionTypes, wayPoints, plugin.Map.Metrics.Length, toolTip1);
                milMissions.Populate(missionItemInfo, this);
                milMissions.TabStop = selected.Missions.Count > 0;
                btnAddTeam.Enabled = selected.Classes.Count < Globals.MaxTeamClasses;
                btnAddMission.Enabled = SelectedTeamType.Missions.Count < Globals.MaxTeamMissions;
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
                tsmiAddTeamType.Visible = true;
                tsmiAddTeamType.Enabled = teamTypesListView.Items.Count < maxTeams;
                tsmiRenameTeamType.Visible = itemExists;
                tsmiCloneTeamType.Visible = itemExists;
                tsmiCloneTeamType.Enabled = teamTypesListView.Items.Count < maxTeams;
                tsmiRemoveTeamType.Visible = itemExists;
                teamTypesContextMenuStrip.Show(Cursor.Position);
            }
        }

        private void TeamTypesListView_KeyDown(object sender, KeyEventArgs e)
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
            else if (e.KeyData == (Keys.C | Keys.Control))
            {
                CloneTeamType();
            }
        }

        private void TeamTypesDialog_KeyDown(object sender, KeyEventArgs e)
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
            if (DialogResult == DialogResult.OK)
            {
                // Remove rename chains of newly added items.
                RemoveNewRenames(renameActions, false);
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
                hasChanges = RemoveNewRenames(renameActions, true).Count > 0;
            }
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
                DialogResult dr = MessageBox.Show(this, "Teams have been changed! Are you sure you want to cancel?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                    return;
                DialogResult = DialogResult.None;
                e.Cancel = true;
            }
        }

        private List<(string Name1, string Name2)> RemoveNewRenames(List<(string Name1, string Name2)> renameActions, bool clone)
        {
            List<(string Name1, string Name2)> renActions;
            if (clone)
            {
                renActions = new List<(string Name1, string Name2)>();
                foreach ((string name1, string name2) in renameActions)
                {
                    renActions.Add((name1, name2));
                }
            }
            else
            {
                renActions = renameActions;
            }
            for (int i = 0; i < renActions.Count; ++i)
            {
                (string Name1, string Name2) foundNew = renActions[i];
                if (foundNew.Name1 == null)
                {
                    renActions[i] = (Trigger.None, foundNew.Name2);
                    string currentname = foundNew.Name2;
                    // Follow rename chain
                    for (int j = i + 1; j < renActions.Count; ++j)
                    {
                        (string Name1, string Name2) chained = renActions[j];
                        if (!TeamType.IsEmpty(chained.Name1) && String.Equals(chained.Name1, currentname, StringComparison.OrdinalIgnoreCase))
                        {
                            // Remove from further searches and mark for deletion.
                            renActions[j] = (Trigger.None, chained.Name2);
                            currentname = chained.Name2;
                        }
                    }
                }
            }
            renActions.RemoveAll(ren => TeamType.IsEmpty(ren.Name1));
            return renActions;
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

        private void TsmiAddTeamType_Click(object sender, EventArgs e)
        {
            AddTeamType();
        }

        private void TsmiRenameTeamType_Click(object sender, EventArgs e)
        {
            if (SelectedItem != null)
                SelectedItem.BeginEdit();
        }

        private void TsmiCloneTeamType_Click(object sender, EventArgs e)
        {
            CloneTeamType();
        }

        private void TsmiRemoveTeamType_Click(object sender, EventArgs e)
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
            renameActions.Add((null, teamType.Name));
            teamTypesListView.Items.Add(item).ToolTipText = teamType.Name;
            btnAddTeamType.Enabled = teamTypes.Count < maxTeams;
            CalcListColSizes();
            item.Selected = true;
            teamTypesListView.SelectedItems.Cast<ListViewItem>().FirstOrDefault()?.EnsureVisible();
            item.EnsureVisible();
            item.BeginEdit();
        }

        private void CloneTeamType()
        {
            if (teamTypesListView.Items.Count >= maxTeams)
                return;
            ListViewItem selected = SelectedItem;
            TeamType originTeamType = selected?.Tag as TeamType;
            if (selected == null || originTeamType == null)
            {
                return;
            }
            string name = GeneralUtils.MakeNew4CharName(teamTypes.Select(t => t.Name), "????", TeamType.None);
            var teamType = originTeamType.Clone();
            teamType.Name = name;
            var item = new ListViewItem(teamType.Name)
            {
                Tag = teamType
            };
            item.SubItems.Add(teamType.House.Name);
            teamTypes.Add(teamType);
            renameActions.Add((null, teamType.Name));
            teamTypesListView.Items.Add(item).ToolTipText = teamType.Name;
            btnAddTeamType.Enabled = teamTypes.Count < maxTeams;
            CalcListColSizes();
            item.Selected = true;
            teamTypesListView.SelectedItems.Cast<ListViewItem>().FirstOrDefault()?.EnsureVisible();
            item.EnsureVisible();
            item.BeginEdit();
        }

        private void RemoveTeamType()
        {
            ListViewItem selected = SelectedItem;
            int index = teamTypesListView.SelectedIndices.Count == 0 ? -1 : teamTypesListView.SelectedIndices[0];
            TeamType teamType = selected?.Tag as TeamType;
            if (selected == null || teamType == null || index == -1)
            {
                return;
            }
            string name = teamType.Name;
            teamTypes.Remove(teamType);
            renameActions.Add((name, TeamType.None));
            teamTypesListView.Items.Remove(selected);
            if (teamTypesListView.Items.Count == index)
                index--;
            if (index >= 0 && teamTypesListView.Items.Count > index)
                teamTypesListView.Items[index].Selected = true;
            btnAddTeamType.Enabled = teamTypes.Count < maxTeams;
            CalcListColSizes();
        }

        private void TeamTypesListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            string curName = e.Label;
            if (string.IsNullOrEmpty(curName))
            {
                e.CancelEdit = true;
            }
            else if (curName.Length > maxLength)
            {
                e.CancelEdit = true;
                MessageBox.Show(this, string.Format("Team name is longer than {0} characters.", maxLength), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (TeamType.IsEmpty(curName))
            {
                e.CancelEdit = true;
                MessageBox.Show(this, string.Format("Team name '{0}' is reserved and cannot be used.", TeamType.None), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!INITools.IsValidKey(curName))
            {
                e.CancelEdit = true;
                MessageBox.Show(this, string.Format("Team name '{0}' contains illegal characters. This format only supports simple ASCII, and cannot contain '=', '[' or ']'.", curName.ToUpperInvariant()), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (teamTypes.Where(t => (t != SelectedTeamType) && t.Name.Equals(curName, StringComparison.OrdinalIgnoreCase)).Any())
            {
                e.CancelEdit = true;
                MessageBox.Show(this, string.Format("Team with name '{0}' already exists.", curName.ToUpperInvariant()), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string oldName = SelectedTeamType.Name;
                SelectedTeamType.Name = curName;
                renameActions.Add((oldName, curName));
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
                missionItemInfo = new MissionItemInfo(null, SelectedTeamType.Missions, teamMissionTypes, wayPoints, plugin.Map.Metrics.Length, toolTip1);
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

        private void BtnAddTeam_Click(object sender, EventArgs e)
        {
            if (SelectedTeamType != null)
            {
                if (SelectedTeamType.Classes.Count <= Globals.MaxTeamClasses)
                {
                    TeamTypeClass newItem = new TeamTypeClass() { Type = defaultTeam, Count = 1 };
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

        private void BtnAddMission_Click(object sender, EventArgs e)
        {
            if (SelectedTeamType != null)
            {
                if (SelectedTeamType.Missions.Count <= Globals.MaxTeamMissions)
                {
                    TeamTypeMission newItem = new TeamTypeMission() { Mission = defaultMission, Argument = -1 };
                    SelectedTeamType.Missions.Add(newItem);
                    missionItemInfo = new MissionItemInfo(null, SelectedTeamType.Missions, teamMissionTypes, wayPoints, plugin.Map.Metrics.Length, toolTip1);
                    milMissions.Populate(missionItemInfo, this);
                    MissionItemControl newCtrl = missionItemInfo.GetControlByProperty(newItem, milMissions.Contents);
                    pnlMissionsScroll.ScrollControlIntoView(newCtrl);
                }
                milMissions.TabStop = SelectedTeamType.Missions.Count > 0;
                btnAddMission.Enabled = SelectedTeamType.Missions.Count < Globals.MaxTeamMissions;
            }
        }

        private void chbAutocreate_CheckedChanged(object sender, EventArgs e)
        {
            CheckMaxAllowed();
        }

        private void maxAllowedNud_ValueChanged(object sender, EventArgs e)
        {
            CheckMaxAllowed();
        }

        private void maxAllowedNud_ValueEntered(object sender, ValueEnteredEventArgs e)
        {
            CheckMaxAllowed();
        }

        private void nudInitNum_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CheckMaxAllowed();
        }

        private void maxAllowedNud_ValueUpDown(object sender, Controls.UpDownEventArgs e)
        {
            CheckMaxAllowed();
        }

        private void CheckMaxAllowed()
        {
            bool err = maxAllowedNud.Value == 0 && chbAutocreate.Checked;
            errorProvider1.SetError(chbAutocreate, err ? "The AI will not produce teams when \"Max Allowed\" is set to 0." : null);
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
                lblTriggerInfo.Image = null;
                if (infoImage != null)
                {
                    try { infoImage.Dispose(); }
                    catch { /*ignore*/}
                    infoImage = null;
                }
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void TeamTypesDialog_Resize(object sender, EventArgs e)
        {
            CalcListColSizes();
        }

        private void teamTypesListView_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
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
                // Adds a single pixel to each column until the remainder is used up.
                if (remainder > 0)
                {
                    cwidth++;
                    remainder--;
                }
                col.Width = cwidth;
            }
        }

        private void cmbHouse_SelectedValueChanged(object sender, EventArgs e)
        {
            if (teamTypesListView.SelectedItems.Count == 0 || !(cmbHouse.SelectedItem is ListItem<HouseType> selectedHouse))
            {
                return;
            }
            ListViewItem item = teamTypesListView.SelectedItems[0];
            if (item.SubItems.Count > 1 && item.SubItems[1].Text != selectedHouse.Label)
            {
                item.SubItems[1].Text = selectedHouse.Label;
            }
        }

        private void TeamTypesDialog_Shown(object sender, EventArgs e)
        {
            CalcListColSizes();
            if (initialTeam == null)
            {
                return;
            }
            int index = -1;
            foreach (ListViewItem lvi in teamTypesListView.Items)
            {
                index++;
                TeamType team = lvi.Tag as TeamType;
                if (team == null) {
                    continue;
                }
                if (initialTeam.Equals(team.Name, StringComparison.OrdinalIgnoreCase))
                {
                    teamTypesListView.SelectedIndices.Clear();
                    lvi.Selected = true;
                    teamTypesListView.EnsureVisible(index);
                    break;
                }
            }
        }
    }
}
