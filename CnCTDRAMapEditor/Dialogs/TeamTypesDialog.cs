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
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class TeamTypesDialog : Form
    {
        private readonly IGamePlugin plugin;
        private readonly int maxTeams;
        private readonly IEnumerable<ITechnoType> technoTypes;

        private readonly List<TeamType> teamTypes;
        private readonly List<TeamType> backupTeamTypes;
        public IEnumerable<TeamType> TeamTypes => teamTypes;

        private ListViewItem SelectedItem => (teamTypesListView.SelectedItems.Count > 0) ? teamTypesListView.SelectedItems[0] : null;

        private TeamType SelectedTeamType => SelectedItem?.Tag as TeamType;

        private TeamTypeClass mockClass;
        private TeamTypeMission mockMission;
        private int classEditRow = -1;
        private int missionEditRow = -1;
        // Used to block detection of state change.
        private bool blockStateChange;
        // Remembers which dropdown was last selected, to avoid the auto-opening mechanism from triggering after making a selection in a dropdown.
        private int lastEditedMissionRow = -1;
        private int lastEditedTeamRow = -1;
        private Dictionary<string, string> teamMissionTypes;
        private Dictionary<string, string> teamMissionTooltips;
        private String defaultMission;
        private ToolTipFixer ttf;

        public TeamTypesDialog(IGamePlugin plugin, int maxTeams)
        {
            this.plugin = plugin;
            this.maxTeams = maxTeams;
            technoTypes = plugin.Map.TeamTechnoTypes;

            InitializeComponent();
            int extraWidth = recruitPriorityNud.Width + recruitPriorityNud.Margin.Left + recruitPriorityNud.Margin.Right;
            ttf = new ToolTipFixer(this, toolTip1, 10000, new Dictionary<Type, int> { { typeof(Label), extraWidth } });

            switch (plugin.GameType)
            {
                case GameType.TiberianDawn:
                    triggerLabel.Visible = triggerComboBox.Visible = false;
                    waypointLabel.Visible = waypointComboBox.Visible = false;
                    break;
                case GameType.RedAlert:
                    learningCheckBox.Visible = false;
                    mercernaryCheckBox.Visible = false;
                    break;
            }
            teamTypes = new List<TeamType>(plugin.Map.TeamTypes.Select(t => t.Clone()));
            backupTeamTypes = new List<TeamType>(plugin.Map.TeamTypes.Select(t => t.Clone()));
            int nrOfTeams = Math.Min(maxTeams, teamTypes.Count);
            btnAdd.Enabled = nrOfTeams < maxTeams;
            teamTypesListView.BeginUpdate();
            {
                for (int i = 0; i < nrOfTeams; ++i)
                {
                    TeamType teamType = teamTypes[i];
                    var item = new ListViewItem(teamType.Name)
                    {
                        Tag = teamType
                    };
                    teamTypesListView.Items.Add(item).ToolTipText = teamType.Name;
                }
            }
            teamTypesListView.EndUpdate();

            houseComboBox.DataSource = plugin.Map.Houses.Select(t => new TypeItem<HouseType>(t.Type.Name, t.Type)).ToArray();
            waypointComboBox.DataSource = "(none)".Yield().Concat(plugin.Map.Waypoints.Select(w => w.Name)).ToArray();
            triggerComboBox.DataSource = Trigger.None.Yield().Concat(plugin.Map.Triggers.Select(t => t.Name)).ToArray();

            teamsTypeColumn.DisplayMember = "Name";
            teamsTypeColumn.ValueMember = "Type";
            teamsTypeColumn.DataSource = technoTypes.Select(t => new TypeItem<ITechnoType>(t.Name, t)).ToArray();
            // Fix for case sensitivity issue in teamtype missions
            String[] missions = plugin.Map.TeamMissionTypes;
            teamMissionTypes = Enumerable.ToDictionary(missions, t => t, StringComparer.OrdinalIgnoreCase);
            if (!teamMissionTypes.TryGetValue("Guard", out defaultMission))
                defaultMission = missions.FirstOrDefault() ?? String.Empty;
            missionsMissionColumn.DataSource = missions;
            teamMissionTooltips = new Dictionary<string, string>();
            for (int i = 0; i < missions.Length; i++)
            {
                string missionInfo;
                if (plugin.Map.TeamMissionTypesInfo.TryGetValue(missions[i], out missionInfo))
                    teamMissionTooltips.Add(missions[i], missionInfo);
            }

            teamTypeTableLayoutPanel.Visible = false;
        }

        private void teamTypesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            houseComboBox.DataBindings.Clear();
            roundaboutCheckBox.DataBindings.Clear();
            learningCheckBox.DataBindings.Clear();
            suicideCheckBox.DataBindings.Clear();
            autocreateCheckBox.DataBindings.Clear();
            mercernaryCheckBox.DataBindings.Clear();
            reinforcableCheckBox.DataBindings.Clear();
            prebuiltCheckBox.DataBindings.Clear();
            recruitPriorityNud.DataBindings.Clear();
            initNumNud.DataBindings.Clear();
            maxAllowedNud.DataBindings.Clear();
            fearNud.DataBindings.Clear();
            waypointComboBox.DataBindings.Clear();
            triggerComboBox.DataBindings.Clear();

            if (SelectedTeamType != null)
            {
                houseComboBox.DataBindings.Add("SelectedValue", SelectedTeamType, "House");
                roundaboutCheckBox.DataBindings.Add("Checked", SelectedTeamType, "IsRoundAbout");
                learningCheckBox.DataBindings.Add("Checked", SelectedTeamType, "IsLearning");
                suicideCheckBox.DataBindings.Add("Checked", SelectedTeamType, "IsSuicide");
                autocreateCheckBox.DataBindings.Add("Checked", SelectedTeamType, "IsAutocreate");
                mercernaryCheckBox.DataBindings.Add("Checked", SelectedTeamType, "IsMercenary");
                reinforcableCheckBox.DataBindings.Add("Checked", SelectedTeamType, "IsReinforcable");
                prebuiltCheckBox.DataBindings.Add("Checked", SelectedTeamType, "IsPrebuilt");
                SelectedTeamType.RecruitPriority = CheckBounds(SelectedTeamType.RecruitPriority, recruitPriorityNud);
                recruitPriorityNud.DataBindings.Add("Value", SelectedTeamType, "RecruitPriority");
                SelectedTeamType.InitNum = CheckBounds(SelectedTeamType.InitNum, initNumNud);
                initNumNud.DataBindings.Add("Value", SelectedTeamType, "InitNum");
                SelectedTeamType.MaxAllowed = CheckBounds(SelectedTeamType.MaxAllowed, maxAllowedNud);
                maxAllowedNud.DataBindings.Add("Value", SelectedTeamType, "MaxAllowed");
                SelectedTeamType.Fear = CheckBounds(SelectedTeamType.Fear, fearNud);
                fearNud.DataBindings.Add("Value", SelectedTeamType, "Fear");
                waypointComboBox.DataBindings.Add("SelectedIndex", SelectedTeamType, "Origin");
                triggerComboBox.DataBindings.Add("SelectedItem", SelectedTeamType, "Trigger");

                mockClass = null;
                mockMission = null;
                classEditRow = -1;
                missionEditRow = -1;

                teamsDataGridView.Rows.Clear();
                missionsDataGridView.Rows.Clear();

                teamsDataGridView.RowCount = SelectedTeamType.Classes.Count + 1;
                missionsDataGridView.RowCount = SelectedTeamType.Missions.Count + 1;

                updateDataGridViewAddRows(teamsDataGridView, Globals.MaxTeamClasses);
                updateDataGridViewAddRows(missionsDataGridView, Globals.MaxTeamMissions);

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

        private void teamTypesListView_MouseDown(object sender, MouseEventArgs e)
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

        private void teamTypesListView_KeyDown(Object sender, KeyEventArgs e)
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddTeamType();
        }
        private void TeamTypesDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If user pressed ok, nevermind,just go on.
            if (this.DialogResult == DialogResult.OK)
                return;
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

        private void addTeamTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddTeamType();
        }

        private void renameTeamTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedItem != null)
                SelectedItem.BeginEdit();
        }

        private void removeTeamTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveTeamType();
        }

        private void AddTeamType()
        {
            if (teamTypesListView.Items.Count >= maxTeams)
                return;
            string name = INIHelpers.MakeNew4CharName(teamTypes.Select(t => t.Name), "????");
            var teamType = new TeamType { Name = name, House = plugin.Map.HouseTypes.First() };
            var item = new ListViewItem(teamType.Name)
            {
                Tag = teamType
            };
            teamTypes.Add(teamType);
            teamTypesListView.Items.Add(item).ToolTipText = teamType.Name;
            btnAdd.Enabled = teamTypes.Count < maxTeams;
            item.Selected = true;
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
            btnAdd.Enabled = teamTypes.Count < maxTeams;
        }

        private void teamTypesListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            int maxLength = int.MaxValue;
            switch (plugin.GameType)
            {
                case GameType.TiberianDawn:
                    maxLength = 8;
                    break;
                case GameType.RedAlert:
                    maxLength = 23;
                    break;
            }
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

        private void teamsDataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (SelectedTeamType == null)
            {
                return;
            }
            TeamTypeClass teamTypeClass = null;
            if (e.RowIndex == classEditRow)
            {
                teamTypeClass = mockClass;
            }
            else if (e.RowIndex < SelectedTeamType.Classes.Count)
            {
                teamTypeClass = SelectedTeamType.Classes[e.RowIndex];
            }
            if (teamTypeClass == null)
            {
                return;
            }
            switch (e.ColumnIndex)
            {
                case 0:
                    e.Value = teamTypeClass.Type;
                    break;
                case 1:
                    e.Value = teamTypeClass.Count;
                    break;
            }
        }

        private void teamsDataGridView_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
        {
            if (SelectedTeamType == null)
            {
                return;
            }

            if (mockClass == null)
            {
                mockClass = (e.RowIndex < SelectedTeamType.Classes.Count) ?
                    new TeamTypeClass { Type = SelectedTeamType.Classes[e.RowIndex].Type, Count = SelectedTeamType.Classes[e.RowIndex].Count } :
                    new TeamTypeClass { Type = technoTypes.First(), Count = 0 };
            }
            classEditRow = e.RowIndex;

            switch (e.ColumnIndex)
            {
                case 0:
                    mockClass.Type = e.Value as ITechnoType;
                    break;
                case 1:
                    mockClass.Count = int.TryParse(e.Value as string, out int value) ? (byte)Math.Max(0, Math.Min(255, value)) : (byte)0;
                    break;
            }
        }

        private void teamsDataGridView_NewRowNeeded(object sender, DataGridViewRowEventArgs e)
        {
            mockClass = new TeamTypeClass { Type = technoTypes.First(), Count = 0 };
            classEditRow = teamsDataGridView.RowCount - 1;
        }

        private void teamsDataGridView_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            if ((mockClass != null) && (e.RowIndex >= SelectedTeamType.Classes.Count) && ((teamsDataGridView.Rows.Count > 1) || (e.RowIndex < (teamsDataGridView.Rows.Count - 1))))
            {
                SelectedTeamType.Classes.Add(mockClass);
                mockClass = null;
                classEditRow = -1;
            }
            else if ((mockClass != null) && (e.RowIndex < SelectedTeamType.Classes.Count))
            {
                SelectedTeamType.Classes[e.RowIndex] = mockClass;
                mockClass = null;
                classEditRow = -1;
            }
            else if (teamsDataGridView.ContainsFocus)
            {
                mockClass = null;
                classEditRow = -1;
            }
        }

        private void teamsDataGridView_RowDirtyStateNeeded(object sender, QuestionEventArgs e)
        {
            e.Response = teamsDataGridView.IsCurrentCellDirty;
        }

        private void teamsDataGridView_CancelRowEdit(object sender, QuestionEventArgs e)
        {
            if ((classEditRow == (teamsDataGridView.Rows.Count - 2)) && (classEditRow == SelectedTeamType.Classes.Count))
            {
                mockClass = new TeamTypeClass { Type = technoTypes.First(), Count = 0 };
            }
            else
            {
                mockClass = null;
                classEditRow = -1;
            }
        }

        private void teamsDataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (e.Row.Index < SelectedTeamType.Classes.Count)
            {
                SelectedTeamType.Classes.RemoveAt(e.Row.Index);
            }

            if (e.Row.Index == classEditRow)
            {
                mockClass = null;
                classEditRow = -1;
            }
            // Prevent dropdown from popping up.
            lastEditedTeamRow = e.Row.Index - 1;
            lastEditedMissionRow = -1;
        }

        private void teamsDataGridView_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            updateDataGridViewAddRows(teamsDataGridView, Globals.MaxTeamClasses);
        }

        private void teamsDataGridView_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            updateDataGridViewAddRows(teamsDataGridView, Globals.MaxTeamClasses);
        }
        private void DataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            String owner = (sender as DataGridView)?.Name;
            Exception ex = e.Exception;
            String message = ex.Message;
            String stackTrace = ex.StackTrace;
        }

        private void missionsDataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (SelectedTeamType == null)
            {
                return;
            }

            TeamTypeMission teamMissionType = null;
            if (e.RowIndex == missionEditRow)
            {
                teamMissionType = mockMission;
            }
            else if (e.RowIndex < SelectedTeamType.Missions.Count)
            {
                teamMissionType = SelectedTeamType.Missions[e.RowIndex];
            }
            if (teamMissionType == null)
            {
                return;
            }
            // Fix for case sensitivity issue in teamtype missions
            String mission = null;
            if (teamMissionType.Mission != null && !teamMissionTypes.TryGetValue(teamMissionType.Mission, out mission))
                mission = defaultMission;
            switch (e.ColumnIndex)
            {
                case 0:
                    e.Value = mission;
                    break;
                case 1:
                    e.Value = teamMissionType.Argument;
                    break;
            }
        }

        private void missionsDataGridView_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
        {
            if (SelectedTeamType == null)
            {
                return;
            }

            if (mockMission == null)
            {
                mockMission = (e.RowIndex < SelectedTeamType.Missions.Count) ?
                    new TeamTypeMission { Mission = SelectedTeamType.Missions[e.RowIndex].Mission, Argument = SelectedTeamType.Missions[e.RowIndex].Argument } :
                    new TeamTypeMission { Mission = plugin.Map.TeamMissionTypes.First(), Argument = 0 };
            }
            missionEditRow = e.RowIndex;

            switch (e.ColumnIndex)
            {
                case 0:
                    mockMission.Mission = e.Value as string;
                    break;
                case 1:
                    mockMission.Argument = int.TryParse(e.Value as string, out int value) ? value : 0;
                    break;
            }
        }

        private void missionsDataGridView_NewRowNeeded(object sender, DataGridViewRowEventArgs e)
        {
            mockMission = new TeamTypeMission { Mission = plugin.Map.TeamMissionTypes.First(), Argument = 0 };
            missionEditRow = missionsDataGridView.RowCount - 1;
        }

        private void missionsDataGridView_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            if ((mockMission != null) && (e.RowIndex >= SelectedTeamType.Missions.Count) && ((missionsDataGridView.Rows.Count > 1) || (e.RowIndex < (missionsDataGridView.Rows.Count - 1))))
            {
                SelectedTeamType.Missions.Add(mockMission);
                mockMission = null;
                missionEditRow = -1;
            }
            else if ((mockMission != null) && (e.RowIndex < SelectedTeamType.Missions.Count))
            {
                SelectedTeamType.Missions[e.RowIndex] = mockMission;
                mockMission = null;
                missionEditRow = -1;
            }
            else if (missionsDataGridView.ContainsFocus)
            {
                mockMission = null;
                missionEditRow = -1;
            }
            SetToolTip(sender as DataGridView, e.RowIndex, teamMissionTooltips);
        }

        private void missionsDataGridView_RowDirtyStateNeeded(object sender, QuestionEventArgs e)
        {
            e.Response = missionsDataGridView.IsCurrentCellDirty;
        }

        private void missionsDataGridView_CancelRowEdit(object sender, QuestionEventArgs e)
        {
            if ((missionEditRow == (missionsDataGridView.Rows.Count - 2)) && (missionEditRow == SelectedTeamType.Missions.Count))
            {
                mockMission = new TeamTypeMission { Mission = plugin.Map.TeamMissionTypes.First(), Argument = 0 };
            }
            else
            {
                mockMission = null;
                missionEditRow = -1;
            }
        }

        private void missionsDataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (e.Row.Index < SelectedTeamType.Missions.Count)
            {
                SelectedTeamType.Missions.RemoveAt(e.Row.Index);
            }

            if (e.Row.Index == missionEditRow)
            {
                mockMission = null;
                missionEditRow = -1;
            }
            // Prevent dropdown from popping up.
            lastEditedMissionRow = e.Row.Index - 1;
            lastEditedTeamRow = -1;

        }

        private void missionsDataGridView_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            updateDataGridViewAddRows(missionsDataGridView, Globals.MaxTeamMissions);
        }

        private void missionsDataGridView_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            updateDataGridViewAddRows(missionsDataGridView, Globals.MaxTeamMissions);
        }

        private void updateDataGridViewAddRows(DataGridView dataGridView, int maxItems)
        {
            dataGridView.AllowUserToAddRows = dataGridView.Rows.Count <= maxItems;
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
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void missionsDataGridView_CellValueChanged(Object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 0)
            {
                return;
            }
            SetToolTip(sender as DataGridView, e.RowIndex, teamMissionTooltips);
        }

        private void missionsDataGridView_CurrentCellDirtyStateChanged(Object sender, EventArgs e)
        {
            if (blockStateChange)
            {
                return;
            }
            DataGridView src = sender as DataGridView;
            if (src == null)
            {
                return;
            }
            if (src.IsCurrentCellDirty)
                ConfirmDropDown(src, true, true);
        }

        private void teamsDataGridView_CurrentCellDirtyStateChanged(Object sender, EventArgs e)
        {
            if (blockStateChange)
            {
                return;
            }
            DataGridView src = sender as DataGridView;
            if (src == null)
            {
                return;
            }
            ConfirmDropDown(src, true, false);
        }

        private void missionsDataGridView_CellEnter(Object sender, DataGridViewCellEventArgs e)
        {
            DataGridView src;
            if (e.RowIndex < 0 || (src = sender as DataGridView) == null || e.RowIndex == lastEditedMissionRow)
            {
                return;
            }
            DataGridViewCell missCell = src.Rows[e.RowIndex].Cells[0];
            string mission = missCell.Value as String;
            if (SelectedTeamType.Missions.Count == e.RowIndex)
            {
                // Refresh so the width adapts correctly.
                missCell.Value = null;
                missCell.Value = mission;
            }
            SetToolTip(src, e.RowIndex, teamMissionTooltips);
            CellEnter(sender, e, true);
        }

        private void teamsDataGridView_CellEnter(Object sender, DataGridViewCellEventArgs e)
        {
            DataGridView src;
            if (e.RowIndex < 0 || (src = sender as DataGridView) == null || e.RowIndex == lastEditedMissionRow)
            {
                return;
            }
            DataGridViewCell unitCell = src.Rows[e.RowIndex].Cells[0];
            string unit = unitCell.Value as String;
            if (SelectedTeamType.Classes.Count == e.RowIndex)
            {
                // Refresh so the width adapts correctly.
                unitCell.Value = null;
                unitCell.Value = unit;
            }
            CellEnter(sender, e, false);
        }

        private void CellEnter(Object sender, DataGridViewCellEventArgs e, Boolean forMission)
        {
            int lastEdited = forMission ? lastEditedMissionRow : lastEditedTeamRow;
            DataGridView src;
            if (e.RowIndex < 0 || (src = sender as DataGridView) == null || e.RowIndex == lastEdited)
            {
                return;
            }
            if (e.ColumnIndex == 0)
            {
                ConfirmDropDown(src, false, forMission);
                src.BeginEdit(true);
                if (src.Columns[e.ColumnIndex] is DataGridViewComboBoxColumn)
                {
                    ComboBox combo = (ComboBox)src.EditingControl;
                    combo.DropDownClosed -= ConfirmEdit;
                    combo.DropDownClosed += ConfirmEdit;
                    String val = combo.SelectedValue as String;
                    try
                    {
                        blockStateChange = true;
                        // Removes the jumping around of values caused by it recycling the same dropdown control
                        combo.Text = null;
                        combo.DroppedDown = true;
                        combo.Text = val;
                    }
                    finally
                    {
                        blockStateChange = false;
                    }
                }
            }
        }

        private void ConfirmEdit(Object sender, EventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            Control ctrl = combo.Parent;
            DataGridView parent = null;
            while (ctrl != null && (parent = ctrl as DataGridView) == null)
                ctrl = ctrl.Parent;
            if (parent != null)
                ConfirmDropDown(parent, true, parent == missionsDataGridView);
        }

        private void SetToolTip(DataGridView sender, int row, Dictionary<string, string> tooltips)
        {
            if (sender == null || row < 0)
            {
                return;
            }
            string mission = sender.Rows[row].Cells[0].Value as string;
            if (mission == null)
            {
                return;
            }
            DataGridViewCell argCell = sender.Rows[row].Cells[1];
            if (tooltips.TryGetValue(mission, out string tooltip))
            {
                argCell.ToolTipText = tooltip;
            }
        }

        private void ConfirmDropDown(DataGridView src, bool removeEditor, bool forMission)
        {
            if (src.IsCurrentCellDirty)
            {
                // check if it's the dropdown
                if (src.CurrentCell != null && src.CurrentCell.ColumnIndex == 1)
                {
                    // This fires the cell value changed handler.
                    src.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            }
            if (removeEditor)
            {
                // Gets rid of the editor control. Only do this when ending an
                // edit, never when starting one; it'll crash because it changes
                // the selected cell in the function to handle selecting a cell.
                DataGridViewCell cur = src.CurrentCell;
                src.CurrentCell = null;
                src.CurrentCell = cur;
            }
            if (forMission)
            {
                lastEditedMissionRow = src.CurrentCell.RowIndex;
                lastEditedTeamRow = -1;
            }
            else
            {
                lastEditedTeamRow = src.CurrentCell.RowIndex;
                lastEditedMissionRow = -1;
            }
        }

        private void teamsDataGridView_Leave(Object sender, EventArgs e)
        {
            lastEditedTeamRow = -1;
        }

        private void missionsDataGridView_Leave(Object sender, EventArgs e)
        {
            lastEditedMissionRow = -1;
        }

        private void missionsDataGridView_CellMouseDown(Object sender, DataGridViewCellMouseEventArgs e)
        {
            // Prevent dropdown from popping up when clicking row header column.
            if (e.ColumnIndex == -1 && e.RowIndex >= 0)
            {
                lastEditedMissionRow = e.RowIndex;
                lastEditedTeamRow = -1;
            }
        }

        private void teamsDataGridView_CellMouseDown(Object sender, DataGridViewCellMouseEventArgs e)
        {
            // Prevent dropdown from popping up when clicking row header column.
            if (e.ColumnIndex == -1 && e.RowIndex >= 0)
            {
                lastEditedTeamRow = e.RowIndex;
                lastEditedMissionRow = -1;
            }
        }
    }
}
