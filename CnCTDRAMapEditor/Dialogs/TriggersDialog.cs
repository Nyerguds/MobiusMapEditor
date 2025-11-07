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
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class TriggersDialog : Form
    {
        private const long defaultData = -1;
        private const int maxLength = 4;
        private readonly IGamePlugin plugin;
        private readonly int maxTriggers;
        // what even is this shit srsly. Replaced by something more human readable.
        //private string[] persistenceNamesTd = new string[] { "No", "And", "Or" };
        //private string[] persistenceNamesRa = new string[] { "Temporary", "Semi-Constant", "Constant" };
        private string[] persistenceNames = new string[] { "On first triggering", "When all linked objects are triggered", "On each triggering" };
        private string[] typeNames = new string[]
        {
                "Event → Action1 [+ Action2]",
                "Event1 AND Event2 → Action1 [+ Action2]",
                "Event1 OR Event2 → Action1 [+ Action2]",
                "Event1 → Action1; Event2 → Action2",
        };

        /// <summary>Argument types to generate a special tooltip for.</summary>
        private enum ArgType
        {
            None,
            TeamType,
            Trigger
        }

        private TriggerFilter triggerFilter;
        private readonly List<Trigger> backupTriggers;
        private readonly List<Trigger> triggers;
        public List<Trigger> Triggers => triggers;
        private readonly List<(string Name1, string Name2)> renameActions;
        public List<(string Name1, string Name2)> RenameActions => renameActions;
        private ArgType eventArgType1 = ArgType.None;
        private ArgType eventArgType2 = ArgType.None;
        private ArgType actionArgType1 = ArgType.None;
        private ArgType actionArgType2 = ArgType.None;
        private Control tooltipShownOn = null;

        private ListViewItem SelectedItem => (triggersListView.SelectedItems.Count > 0) ? triggersListView.SelectedItems[0] : null;

        private Trigger SelectedTrigger => SelectedItem?.Tag as Trigger;

        public TriggersDialog(IGamePlugin plugin)
        {
            this.plugin = plugin;
            maxTriggers = plugin.GameInfo.MaxTriggers;
            InitializeComponent();
            SetTriggerFilter(new TriggerFilter(plugin));
            lblTooLong.Text = "Trigger length exceeds " + maxLength + " characters!";
            switch (plugin.GameInfo.GameType)
            {
                case GameType.TiberianDawn:
                case GameType.SoleSurvivor:
                    //persistenceLabel.Text = "Loop";
                    event1Label.Text = "Event";
                    action1Label.Text = "Action";
                    typeLabel.Visible = typeComboBox.Visible = false;
                    event2Label.Visible = event2ComboBox.Visible = event2Flp.Visible = false;
                    action2Label.Visible = action2ComboBox.Visible = action2Flp.Visible = false;
                    break;
                case GameType.RedAlert:
                    teamLabel.Visible = teamComboBox.Visible = false;
                    break;
            }
            triggers = new List<Trigger>(plugin.Map.Triggers.Select(t => t.Clone()));
            renameActions = new List<(string Name1, string Name2)>();
            backupTriggers = new List<Trigger>(triggers.Select(t => t.Clone()));
            int nrOfTriggers = Math.Min(maxTriggers, triggers.Count);
            if (triggers.Count > maxTriggers)
            {
                Trigger[] trigArr = triggers.ToArray();
                Trigger[] trigCut = new Trigger[nrOfTriggers];
                Array.Copy(trigArr, trigCut, nrOfTriggers);
                triggers = new List<Trigger>(trigCut);
            }
            btnAdd.Enabled = nrOfTriggers < maxTriggers;
            RefreshTriggers();
            houseComboBox.ValueMember = "Value";
            houseComboBox.DisplayMember = "Label";
            houseComboBox.DataSource = House.None.Yield().Concat(plugin.Map.Houses.Select(t => t.Type.Name))
                .Select(v => ListItem.Create(v)).ToArray();
            persistenceComboBox.ValueMember = "Value";
            persistenceComboBox.DisplayMember = "Label";
            persistenceComboBox.DataSource = Enum.GetValues(typeof(TriggerPersistentType)).Cast<TriggerPersistentType>()
                .Select(v => ListItem.Create(v, persistenceNames[(int)v])).ToArray();
            typeComboBox.ValueMember = "Value";
            typeComboBox.DisplayMember = "Label";
            typeComboBox.DataSource = Enum.GetValues(typeof(TriggerMultiStyleType)).Cast<TriggerMultiStyleType>()
                .Select(v => ListItem.Create(v, typeNames[(int)v])).ToArray();
            event1ComboBox.DataSource = plugin.Map.EventTypes.Where(t => !string.IsNullOrEmpty(t)).ToArray();
            event2ComboBox.DataSource = plugin.Map.EventTypes.Where(t => !string.IsNullOrEmpty(t)).ToArray();
            action1ComboBox.DataSource = plugin.Map.ActionTypes.Where(t => !string.IsNullOrEmpty(t)).ToArray();
            action2ComboBox.DataSource = plugin.Map.ActionTypes.Where(t => !string.IsNullOrEmpty(t)).ToArray();
            teamComboBox.ValueMember = "Value";
            teamComboBox.DisplayMember = "Label";
            teamComboBox.DataSource = ListItem.Create(TeamType.None).Yield().Concat(plugin.Map.TeamTypes.Select(t => ListItem.Create(t.Name))).ToArray();
            triggersTableLayoutPanel.Visible = false;
        }

        private void RefreshTriggers()
        {
            bool hasFilter = triggerFilter != null && !triggerFilter.IsEmpty;
            triggersListView.BeginUpdate();
            triggersListView.Items.Clear();
            {
                foreach (Trigger trigger in triggers)
                {
                    if (hasFilter && !triggerFilter.MatchesFilter(trigger))
                    {
                        continue;
                    }
                    var item = new ListViewItem(trigger.Name)
                    {
                        Tag = trigger
                    };
                    triggersListView.Items.Add(item).ToolTipText = trigger.Name;
                }
            }
            triggersListView.EndUpdate();
            AdjustTriggersListViewColWidth();
        }

        private void triggersListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshListSelection();
        }

        private void RefreshListSelection()
        {
            houseComboBox.DataBindings.Clear();
            persistenceComboBox.DataBindings.Clear();
            typeComboBox.DataBindings.Clear();
            // no longer managed by data bindings; too many weird race conditions.
            //event1ComboBox.DataBindings.Clear();
            //event2ComboBox.DataBindings.Clear();
            //action1ComboBox.DataBindings.Clear();
            //action2ComboBox.DataBindings.Clear();
            teamComboBox.DataBindings.Clear();
            lblTooLong.Visible = SelectedTrigger != null && SelectedTrigger.Name != null && SelectedTrigger.Name.Length > maxLength;
            if (SelectedTrigger != null)
            {
                houseComboBox.DataBindings.Add("SelectedValue", SelectedTrigger, "House", true, DataSourceUpdateMode.OnPropertyChanged);
                persistenceComboBox.DataBindings.Add("SelectedValue", SelectedTrigger, "PersistentType", true, DataSourceUpdateMode.OnPropertyChanged);
                // Set event 1
                TriggerEvent evt1 = SelectedTrigger.Event1.Clone();
                event1ComboBox.SelectedItem = SelectedTrigger.Event1.EventType;
                bool otherToolTipShownEvt = tooltipShownOn != null && tooltipShownOn != event1ComboBox && tooltipShownOn.ClientRectangle.Contains(tooltipShownOn.PointToClient(Cursor.Position));
                UpdateTriggerEventControls(SelectedTrigger.Event1, event1Nud, event1ValueComboBox, ref eventArgType1, otherToolTipShownEvt, evt1);
                UpdateEvent1ComboBoxToolTip();
                SelectedTrigger.Event1.FillDataFrom(evt1);
                // Set action 1
                TriggerAction act1 = SelectedTrigger.Action1.Clone();
                action1ComboBox.SelectedItem = SelectedTrigger.Action1.ActionType;
                bool otherToolTipShownAct = tooltipShownOn != null && tooltipShownOn != action1ValueComboBox && tooltipShownOn.ClientRectangle.Contains(tooltipShownOn.PointToClient(Cursor.Position));
                UpdateTriggerActionControls(SelectedTrigger.Action1, action1Nud, action1ValueComboBox, ref actionArgType1, otherToolTipShownAct, act1);
                UpdateAction1ComboBoxToolTip();
                SelectedTrigger.Action1.FillDataFrom(act1);
                switch (plugin.GameInfo.GameType)
                {
                    case GameType.TiberianDawn:
                    case GameType.SoleSurvivor:
                        string team = SelectedTrigger.Action1.Team ?? TeamType.None;                        
                        string[] teamData = TeamType.None.Yield().Concat(plugin.Map.TeamTypes.Select(t =>  t.Name)).ToArray();
                        teamComboBox.DataSource = teamData.Select(tn => ListItem.Create(tn)).ToArray();
                        string correctedTeam = teamData.FirstOrDefault(tm => tm.Equals(team, StringComparison.OrdinalIgnoreCase)) ?? TeamType.None;
                        SelectedTrigger.Action1.Team = correctedTeam;
                        teamComboBox.DataBindings.Add("SelectedValue", SelectedTrigger.Action1, "Team", true, DataSourceUpdateMode.OnPropertyChanged);
                        string teamDescrTd = GetTeamLabel(SelectedTrigger.Action1.Team);
                        if (teamDescrTd != null && teamComboBox.ClientRectangle.Contains(teamComboBox.PointToClient(Cursor.Position)))
                        {
                            ShowToolTip(toolTip1, teamComboBox, teamDescrTd);
                        }
                        break;
                    case GameType.RedAlert:
                        typeComboBox.DataBindings.Add("SelectedValue", SelectedTrigger, "EventControl", true, DataSourceUpdateMode.OnPropertyChanged);
                        // Set event 2
                        TriggerEvent evt2 = SelectedTrigger.Event2.Clone();
                        event2ComboBox.SelectedItem = SelectedTrigger.Event2.EventType;
                        otherToolTipShownEvt = tooltipShownOn != null && tooltipShownOn != event2ComboBox && tooltipShownOn.ClientRectangle.Contains(tooltipShownOn.PointToClient(Cursor.Position));
                        UpdateTriggerEventControls(SelectedTrigger.Event2, event2Nud, event2ValueComboBox, ref eventArgType2, otherToolTipShownEvt, evt2);
                        SelectedTrigger.Event2.FillDataFrom(evt2);
                        // Set action 2
                        TriggerAction act2 = SelectedTrigger.Action2.Clone();
                        action2ComboBox.SelectedItem = SelectedTrigger.Action2?.ActionType;
                        otherToolTipShownAct = tooltipShownOn != null && tooltipShownOn != action2ValueComboBox && tooltipShownOn.ClientRectangle.Contains(tooltipShownOn.PointToClient(Cursor.Position));
                        UpdateTriggerActionControls(SelectedTrigger.Action2, action2Nud, action2ValueComboBox, ref actionArgType2, otherToolTipShownAct, act2);
                        SelectedTrigger.Action2.FillDataFrom(act2);
                        break;
                }
                triggersTableLayoutPanel.Visible = true;
                // Force this to update
                TypeComboBox_SelectedValueChanged(typeComboBox, new EventArgs());
            }
            else
            {
                triggersTableLayoutPanel.Visible = false;
            }
        }

        private void triggersListView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = triggersListView.HitTest(e.Location);
                bool itemExists = hitTest.Item != null;
                tsmiAddTrigger.Visible = true;
                tsmiAddTrigger.Enabled = triggersListView.Items.Count < maxTriggers;
                tsmiRenameTrigger.Visible = itemExists;
                tsmiCloneTrigger.Visible = itemExists;
                tsmiCloneTrigger.Enabled = triggersListView.Items.Count < maxTriggers;
                tsmiRemoveTrigger.Visible = itemExists;
                triggersContextMenuStrip.Show(Cursor.Position);
            }
        }

        private void triggersListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.F2)
            {
                ListViewItem selected = SelectedItem;
                if (selected != null)
                    selected.BeginEdit();
            }
            else if (e.KeyData == Keys.Delete)
            {
                RemoveTrigger();
            }
            else if (e.KeyData == (Keys.C | Keys.Control))
            {
                CloneTrigger();
            }
        }

        private void TriggersDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.A | Keys.Control))
            {
                AddTrigger();
            }
        }

        private void Event1Nud_ValueChanged(object sender, EventArgs e)
        {
            Trigger trig = SelectedTrigger;
            if (trig?.Event1 != null)
            {
                trig.Event1.Data = (long)event1Nud.Value;
            }
        }

        private void Event2Nud_ValueChanged(object sender, EventArgs e)
        {
            Trigger trig = SelectedTrigger;
            if (trig?.Event2 != null)
            {
                trig.Event2.Data = (long)event2Nud.Value;
            }
        }

        private void Action1Nud_ValueChanged(object sender, EventArgs e)
        {
            Trigger trig = SelectedTrigger;
            if (trig?.Action1 != null)
            {
                trig.Action1.Data = (long)action1Nud.Value;
            }
        }

        private void Action2Nud_ValueChanged(object sender, EventArgs e)
        {
            Trigger trig = SelectedTrigger;
            if (trig?.Action2 != null)
            {
                trig.Action2.Data = (long)action2Nud.Value;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            AddTrigger();
        }

        private void BtnSetFilter_Click(object sender, EventArgs e)
        {
            ChangeFilter();
        }

        private void TriggersDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If user pressed ok, nevermind,just go on.
            if (DialogResult == DialogResult.OK)
            {
                if (CancelForFatalErrors())
                {
                    e.Cancel = true;
                    return;
                }
                // Remove rename chains of newly added items.
                RemoveNewRenames(renameActions, false);
                return;
            }
            bool hasChanges = triggers.Count != backupTriggers.Count;
            if (!hasChanges)
            {
                // Apply on clone to not break the rename chains of current new items, which would prevent them from being filtered out later.
                hasChanges = RemoveNewRenames(renameActions, true).Count > 0;
            }
            if (!hasChanges)
            {
                hasChanges = Trigger.CheckForChanges(triggers, backupTriggers);
            }
            if (hasChanges)
            {
                DialogResult dr = MessageBox.Show(this, "Triggers have been changed! Are you sure you want to cancel?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                    return;
                DialogResult = DialogResult.None;
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Checks if the dialog should stay open to allow the user to correct fatal errors.
        /// </summary>
        /// <returns>True if the triggers dialog should remain open.</returns>
        private bool CancelForFatalErrors()
        {
            string[] errors = plugin.CheckTriggers(triggers, false, false, true, out bool fatal, false, out _).ToArray();
            if (!fatal)
            {
                return false;
            }
            using (ErrorMessageBox emb = new ErrorMessageBox(true))
            {
                emb.Title = "Triggers check";
                emb.Message = "The following serious issues were encountered. Press \"OK\" to ignore, or \"Cancel\" to go back and edit them.";
                emb.Errors = errors;
                emb.UseWordWrap = true;
                return emb.ShowDialog() == DialogResult.Cancel;
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
                        if (!Trigger.IsEmpty(chained.Name1) && String.Equals(chained.Name1, currentname, StringComparison.OrdinalIgnoreCase))
                        {
                            // Remove from further searches and mark for deletion.
                            renActions[j] = (Trigger.None, chained.Name2);
                            currentname = chained.Name2;
                        }
                    }
                }
            }
            renActions.RemoveAll(ren => Trigger.IsEmpty(ren.Name1));
            return renActions;
        }

        private void TsmiAddTrigger_Click(object sender, EventArgs e)
        {
            AddTrigger();
        }

        private void TsmiRenameTrigger_Click(object sender, EventArgs e)
        {
            if (SelectedItem != null)
                SelectedItem.BeginEdit();
        }

        private void TsmiCloneTrigger_Click(object sender, EventArgs e)
        {

            CloneTrigger();
        }

        private void TsmiRemoveTrigger_Click(object sender, EventArgs e)
        {
            RemoveTrigger();
        }

        private void ChangeFilter()
        {
            string[] currentTriggers = triggers.Select(t => t.Name).ToArray();
            using (TriggerFilterDialog tfd = new TriggerFilterDialog(plugin, persistenceLabel.Text, persistenceNames, typeNames, currentTriggers))
            {
                tfd.Filter = triggerFilter;
                tfd.StartPosition = FormStartPosition.CenterParent;
                if (tfd.ShowDialog() == DialogResult.OK)
                {
                    ApplyFilter(tfd.Filter);
                }
            }
        }

        private void ApplyFilter(TriggerFilter filter)
        {
            Trigger selectedItem = SelectedTrigger;
            SetTriggerFilter(filter);
            RefreshTriggers();
            bool selected = false;
            if (selectedItem != null)
            {
                for (int i = 0; i < triggersListView.Items.Count; ++i)
                {
                    if (triggersListView.Items[i].Tag == selectedItem)
                    {
                        triggersListView.Items[i].Selected = true;
                        triggersListView.Select();
                        selected = true;
                        break;
                    }
                }
            }
            if (!selected)
            {
                RefreshListSelection();
            }
        }

        private void SetTriggerFilter(TriggerFilter triggerFilter)
        {
            this.triggerFilter = triggerFilter;
            string filter = triggerFilter.ToString(persistenceLabel.Text[0], persistenceNames, typeNames);
            lblFilterDetails.Text = triggerFilter.IsEmpty ? "No filters selected." : String.Format("Active filters: {0}", filter);
            lblFilterDetails.ForeColor = triggerFilter.IsEmpty ? SystemColors.ControlText : Color.Red;
        }

        private void AddTrigger()
        {
            if (triggerFilter != null && !triggerFilter.IsEmpty)
            {
                MessageBox.Show(this, "New triggers cannot be added while a filter is active. Reset the filter first.", "Error");
                return;
            }
            if (triggersListView.Items.Count >= maxTriggers)
                return;
            string name = GeneralUtils.MakeNew4CharName(triggers.Select(t => t.Name), "????", Trigger.None);
            var trigger = new Trigger { Name = name, House = plugin.Map.HouseTypes.First().Name };
            var item = new ListViewItem(trigger.Name)
            {
                Tag = trigger
            };
            triggers.Add(trigger);
            renameActions.Add((null, trigger.Name));
            triggersListView.Items.Add(item).ToolTipText = trigger.Name;
            btnAdd.Enabled = triggers.Count < maxTriggers;
            item.Selected = true;
            triggersListView.SelectedItems.Cast<ListViewItem>().FirstOrDefault()?.EnsureVisible();
            AdjustTriggersListViewColWidth();
            item.EnsureVisible();
            item.BeginEdit();
        }

        private void CloneTrigger()
        {
            if (triggersListView.Items.Count >= maxTriggers)
                return;
            ListViewItem selected = SelectedItem;
            Trigger originTrigger = selected?.Tag as Trigger;
            if (selected == null || originTrigger == null)
            {
                return;
            }
            string name = GeneralUtils.MakeNew4CharName(triggers.Select(t => t.Name), "----", Trigger.None);
            var trigger = originTrigger.Clone();
            trigger.Name = name;
            var item = new ListViewItem(trigger.Name)
            {
                Tag = trigger
            };
            triggers.Add(trigger);
            renameActions.Add((null, trigger.Name));
            triggersListView.Items.Add(item).ToolTipText = trigger.Name;
            btnAdd.Enabled = triggers.Count < maxTriggers;
            item.Selected = true;
            triggersListView.SelectedItems.Cast<ListViewItem>().FirstOrDefault()?.EnsureVisible();
            AdjustTriggersListViewColWidth();
            item.EnsureVisible();
            item.BeginEdit();
        }

        private void RemoveTrigger()
        {
            ListViewItem selected = SelectedItem;
            int index = triggersListView.SelectedIndices.Count == 0 ? -1 : triggersListView.SelectedIndices[0];
            Trigger trigger = selected?.Tag as Trigger;
            if (selected == null || trigger == null || index == -1)
            {
                return;
            }
            string name = trigger.Name;
            triggers.Remove(trigger);
            // Trigger is removed; add as "rename to None" action.
            renameActions.Add((name, Trigger.None));
            // Go over all triggers to clear any that have this trigger as argument
            RenameInCurrentTriggerActions(name, Trigger.None);
            triggersListView.Items.Remove(selected);
            if (triggersListView.Items.Count == index)
                index--;
            if (index >= 0 && triggersListView.Items.Count > index)
                triggersListView.Items[index].Selected = true;
            btnAdd.Enabled = triggers.Count < maxTriggers;
            if (triggerFilter.FilterTrigger && String.Equals(triggerFilter.Trigger, name, StringComparison.OrdinalIgnoreCase))
            {
                triggerFilter.FilterTrigger = false;
                triggerFilter.Trigger = Trigger.None;
                ApplyFilter(triggerFilter);
            }
            AdjustTriggersListViewColWidth();
        }

        private void triggersListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            string curName = e.Label;
            if (string.IsNullOrEmpty(curName))
            {
                e.CancelEdit = true;
            }
            else if (curName.Length > maxLength)
            {
                e.CancelEdit = true;
                MessageBox.Show(this, String.Format("Trigger name is longer than {0} characters.", maxLength), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (Trigger.IsEmpty(curName))
            {
                e.CancelEdit = true;
                MessageBox.Show(this, String.Format("Trigger name '{0}' is reserved and cannot be used.", Trigger.None), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!INITools.IsValidKey(curName))
            {
                e.CancelEdit = true;
                MessageBox.Show(this, String.Format("Trigger name '{0}' contains illegal characters. This format only supports simple ASCII, and cannot contain '=', '[' or ']'.", curName), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (triggers.Where(t => (t != SelectedTrigger) && t.Name.Equals(curName, StringComparison.OrdinalIgnoreCase)).Any())
            {
                e.CancelEdit = true;
                MessageBox.Show(this, String.Format("Trigger with name '{0}' already exists.", curName.ToUpperInvariant()), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string oldName = SelectedTrigger.Name;
                // Go over all triggers to adapt any that have this trigger as argument.
                RenameInCurrentTriggerActions(oldName, curName);
                renameActions.Add((oldName, curName));
                SelectedTrigger.Name = curName;
                // Normally always false
                lblTooLong.Visible = curName.Length > maxLength;
                // Force text in there already so listview width recalculation works.
                triggersListView.Items[e.Item].Text = curName;
                triggersListView.Items[e.Item].ToolTipText = curName;
                if (triggerFilter.FilterTrigger && String.Equals(triggerFilter.Trigger, oldName, StringComparison.OrdinalIgnoreCase))
                {
                    triggerFilter.Trigger = curName;
                    ApplyFilter(triggerFilter);
                }
                AdjustTriggersListViewColWidth();
            }
        }

        /// <summary>
        /// Sets the column size to the exact width of the box, or more if there are longer strings inside.
        /// </summary>
        private void AdjustTriggersListViewColWidth()
        {
            triggersListView.BeginUpdate();
            triggersListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            ColumnHeader col = triggersListView.Columns[0];
            // Doesn't matter if there is a scrollbar; this is always correct.
            int fullWidth = triggersListView.ClientRectangle.Width;
            if (col.Width < fullWidth)
            {
                col.Width = fullWidth;
            }
            triggersListView.EndUpdate();
        }

        private void RenameInCurrentTriggerActions(string name, string newName)
        {
            // You never know if someone makes a circular trigger...
            Trigger curr = SelectedTrigger;
            bool updateUi = curr != null && curr.Action1.Trigger == name || curr.Action2.Trigger == name;
            if (plugin.GameInfo.GameType != GameType.RedAlert)
            {
                return;
            }
            foreach (Trigger trig in triggers)
            {
                if (String.Equals(trig.Action1.Trigger, name, StringComparison.OrdinalIgnoreCase))
                {
                    trig.Action1.Trigger = newName;
                }
                if (String.Equals(trig.Action2.Trigger, name, StringComparison.OrdinalIgnoreCase))
                {
                    trig.Action2.Trigger = newName;
                }
            }
            if (updateUi)
            {
                triggersListView_SelectedIndexChanged(triggersListView, new EventArgs());
            }
        }

        private void TypeComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (plugin.GameInfo.GameType == GameType.RedAlert && SelectedTrigger != null)
            {
                var eventType = (TriggerMultiStyleType)typeComboBox.SelectedValue;
                bool hasEvent2 = eventType != TriggerMultiStyleType.Only;
                if (!hasEvent2)
                {
                    // Set event to "None".
                    event2ComboBox.SelectedIndex = 0;
                }
                triggersTableLayoutPanel.SuspendLayout();
                RemoveFromLayout(triggersTableLayoutPanel, event2Label, event2ComboBox, event2Flp);
                RemoveFromLayout(triggersTableLayoutPanel, action1Label, action1ComboBox, action1Flp);
                if (eventType != TriggerMultiStyleType.Linked)
                {
                    // Normal order: E1, [E2 → A1], A2
                    AddToLayout(triggersTableLayoutPanel, event2Label, event2ComboBox, event2Flp, 4);
                    AddToLayout(triggersTableLayoutPanel, action1Label, action1ComboBox, action1Flp, 5);
                }
                else
                {
                    // Flipped order: E1 → [A1, E2] → A2
                    AddToLayout(triggersTableLayoutPanel, action1Label, action1ComboBox, action1Flp, 4);
                    AddToLayout(triggersTableLayoutPanel, event2Label, event2ComboBox, event2Flp, 5);
                }
                event2Label.Visible = event2ComboBox.Visible = event2Flp.Visible = hasEvent2;
                triggersTableLayoutPanel.ResumeLayout(false);
                triggersTableLayoutPanel.PerformLayout();
            }
        }

        private void Event1ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedTrigger != null)
            {
                SelectedTrigger.Event1.EventType = event1ComboBox.SelectedItem.ToString();
            }
            bool otherToolTipIsShown = tooltipShownOn != sender && tooltipShownOn != null && tooltipShownOn.ClientRectangle.Contains(tooltipShownOn.PointToClient(Cursor.Position));
            UpdateTriggerEventControls(SelectedTrigger?.Event1, event1Nud, event1ValueComboBox, ref eventArgType1, otherToolTipIsShown, null);
            UpdateEvent1ComboBoxToolTip();
        }

        private void UpdateEvent1ComboBoxToolTip()
        {
            if (event1ComboBox != null)
            {
                Event1ComboBox_MouseEnter(event1ComboBox, EventArgs.Empty);
            }
        }

        private void Event1ComboBox_MouseEnter(object sender, EventArgs e)
        {
            Control target = sender as Control;
            string evt = SelectedTrigger?.Event1?.EventType;
            string trigDescrTd = null;
            bool hasDesc = evt != null && TiberianDawn.EventTypes.TypesInfo.TryGetValue(evt, out trigDescrTd);
            if (hasDesc && !String.IsNullOrEmpty(trigDescrTd))
            {
                ShowToolTip(toolTip1, target, trigDescrTd);
            }
            else if (tooltipShownOn != null && !tooltipShownOn.ClientRectangle.Contains(tooltipShownOn.PointToClient(Cursor.Position)))
            {
                HideToolTip(toolTip1, tooltipShownOn, true);
            }
        }

        private void Event1ComboBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (tooltipShownOn != sender)
            {
                Event1ComboBox_MouseEnter(sender, e);
            }
        }

        private void Action1ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedTrigger != null)
            {
                SelectedTrigger.Action1.ActionType = action1ComboBox.SelectedItem.ToString();
            }
            bool otherToolTipIsShown = tooltipShownOn != sender && tooltipShownOn != null && tooltipShownOn.ClientRectangle.Contains(tooltipShownOn.PointToClient(Cursor.Position));
            UpdateTriggerActionControls(SelectedTrigger?.Action1, action1Nud, action1ValueComboBox, ref actionArgType1, otherToolTipIsShown, null);
            UpdateAction1ComboBoxToolTip();
        }

        private void UpdateAction1ComboBoxToolTip()
        {
            if (action1ComboBox != null)
            {
                Action1ComboBox_MouseEnter(action1ComboBox, EventArgs.Empty);
            }
        }

        private void Action1ComboBox_MouseEnter(object sender, EventArgs e)
        {
            Control target = sender as Control;
            string trigDescrTd = CheckTdSpecialTrigger(SelectedTrigger?.Action1);
            if (trigDescrTd != null)
            {
                ShowToolTip(toolTip1, target, trigDescrTd);
            }
            else if (tooltipShownOn != null && !tooltipShownOn.ClientRectangle.Contains(tooltipShownOn.PointToClient(Cursor.Position)))
            {
                HideToolTip(toolTip1, tooltipShownOn, true);
            }
        }

        private void Action1ComboBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (tooltipShownOn != sender)
            {
                Action1ComboBox_MouseEnter(sender, e);
            }
        }

        private string CheckTdSpecialTrigger(TriggerAction action)
        {
            if (plugin.GameInfo.GameType != GameType.TiberianDawn
                || action == null || action.ActionType == null)
            {
                return null;
            }
            string delTrig = null;
            bool isFlare = false;
            switch (action.ActionType)
            {
                case TiberianDawn.ActionTypes.ACTION_DESTROY_XXXX: delTrig = "XXXX"; break;
                case TiberianDawn.ActionTypes.ACTION_DESTROY_YYYY: delTrig = "YYYY"; break;
                case TiberianDawn.ActionTypes.ACTION_DESTROY_ZZZZ: delTrig = "ZZZZ"; break;
                case TiberianDawn.ActionTypes.ACTION_DESTROY_UUUU: delTrig = "UUUU"; break;
                case TiberianDawn.ActionTypes.ACTION_DESTROY_VVVV: delTrig = "VVVV"; break;
                case TiberianDawn.ActionTypes.ACTION_DESTROY_WWWW: delTrig = "WWWW"; break;
                case TiberianDawn.ActionTypes.ACTION_DZ: isFlare= true; break;
            }
            if (delTrig != null)
            {
                Trigger toDestr = triggers.FirstOrDefault(tr => delTrig.Equals(tr.Name, StringComparison.OrdinalIgnoreCase));
                if (toDestr == null)
                {
                    return delTrig + ": not found";
                }
                else
                {
                    return plugin.TriggerSummary(toDestr, true, true);
                }
            }
            else if (isFlare)
            {
                string wp = "Waypoint 'Z' (flare): ";
                Waypoint z = plugin.Map.Waypoints.FirstOrDefault(w => w.Flags.HasFlag(WaypointFlag.Flare));
                if (z == null)
                    return wp + "Not found.";
                if (!z.Point.HasValue)
                    return wp + "Not set.";
                Point p = z.Point.Value;
                return String.Format("{0}[{1},{2}] (cell {3})", wp, p.X, p.Y, z.Cell.Value);
            }
            return null;
        }

        private void Event1ValueComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox valCombo = sender as ComboBox;
            if (valCombo.ClientRectangle.Contains(valCombo.PointToClient(Cursor.Position)))
            {
                Event1ValueComboBox_MouseEnter(sender, e);
            }
        }

        private void Event1ValueComboBox_MouseEnter(object sender, EventArgs e)
        {
            ComboBox valCombo = sender as ComboBox;
            string eventValueLabel = GetActionValueLabel(valCombo, eventArgType1);
            if (eventValueLabel != null)
            {
                ShowToolTip(toolTip1, valCombo, eventValueLabel);
            }
            else if (tooltipShownOn != null && !tooltipShownOn.ClientRectangle.Contains(tooltipShownOn.PointToClient(Cursor.Position)))
            {
                HideToolTip(toolTip1, valCombo, true);
            }
        }

        private void Event1ValueComboBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (tooltipShownOn != sender)
            {
                Event1ValueComboBox_MouseEnter(sender, e);
            }
        }

        private void Event2ValueComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox valCombo = sender as ComboBox;
            if (valCombo.ClientRectangle.Contains(valCombo.PointToClient(Cursor.Position)))
            {
                Event2ValueComboBox_MouseEnter(sender, e);
            }
        }

        private void Event2ValueComboBox_MouseEnter(object sender, EventArgs e)
        {
            ComboBox valCombo = sender as ComboBox;
            string eventValueLabel = GetActionValueLabel(valCombo, eventArgType2);
            if (eventValueLabel != null)
            {
                ShowToolTip(toolTip1, valCombo, eventValueLabel);
            }
            else if (tooltipShownOn != null && !tooltipShownOn.ClientRectangle.Contains(tooltipShownOn.PointToClient(Cursor.Position)))
            {
                HideToolTip(toolTip1, valCombo, true);
            }
        }

        private void Event2ValueComboBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (tooltipShownOn != sender)
            {
                Event2ValueComboBox_MouseEnter(sender, e);
            }
        }

        private void Action1ValueComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox valCombo = sender as ComboBox;
            if (valCombo != null && valCombo.ClientRectangle.Contains(valCombo.PointToClient(Cursor.Position)))
            {
                Action1ValueComboBox_MouseEnter(sender, e);
            }
        }

        private void Action1ValueComboBox_MouseEnter(object sender, EventArgs e)
        {
            ComboBox valCombo = sender as ComboBox;
            string actionValueLabel = GetActionValueLabel(valCombo, actionArgType1);
            if (actionValueLabel != null)
            {
                ShowToolTip(toolTip1, valCombo, actionValueLabel);
            }
            else if (tooltipShownOn != null && !tooltipShownOn.ClientRectangle.Contains(tooltipShownOn.PointToClient(Cursor.Position)))
            {
                HideToolTip(toolTip1, valCombo, true);
            }
        }

        private void Action1ValueComboBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (tooltipShownOn != sender)
            {
                Action1ValueComboBox_MouseEnter(sender, e);
            }
        }

        private void Event2ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedTrigger != null)
            {
                SelectedTrigger.Event2.EventType = event2ComboBox.SelectedItem.ToString();
            }
            bool otherToolTipShownEvt = tooltipShownOn != null && tooltipShownOn != event2ComboBox && tooltipShownOn.ClientRectangle.Contains(tooltipShownOn.PointToClient(Cursor.Position));
            UpdateTriggerEventControls(SelectedTrigger?.Event2, event2Nud, event2ValueComboBox, ref eventArgType2, otherToolTipShownEvt, null);
        }

        private void Action2ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedTrigger != null)
            {
                SelectedTrigger.Action2.ActionType = action2ComboBox.SelectedItem.ToString();
            }
            bool otherToolTipIsShown = action1ValueComboBox.ClientRectangle.Contains(action1ValueComboBox.PointToClient(Cursor.Position));
            UpdateTriggerActionControls(SelectedTrigger?.Action2, action2Nud, action2ValueComboBox, ref actionArgType2, otherToolTipIsShown, null);
        }

        private void Action2ValueComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox valCombo = sender as ComboBox;
            if (valCombo != null && valCombo.ClientRectangle.Contains(valCombo.PointToClient(Cursor.Position)))
            {
                Action2ValueComboBox_MouseEnter(sender, e);
            }
        }

        private void Action2ValueComboBox_MouseEnter(object sender, EventArgs e)
        {
            ComboBox valCombo = sender as ComboBox;
            string actionValueLabel = GetActionValueLabel(valCombo, actionArgType2);
            if (actionValueLabel != null)
            {
                ShowToolTip(toolTip1, valCombo, actionValueLabel);
            }
            else if (tooltipShownOn != null && !tooltipShownOn.ClientRectangle.Contains(tooltipShownOn.PointToClient(Cursor.Position)))
            {
                HideToolTip(toolTip1, valCombo, true);
            }
        }

        private void Action2ValueComboBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (tooltipShownOn != sender)
            {
                Action2ValueComboBox_MouseEnter(sender, e);
            }
        }

        private void TeamComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox teamCombo = sender as ComboBox;
            if (teamCombo.ClientRectangle.Contains(teamCombo.PointToClient(Cursor.Position)))
            {
                TeamComboBox_MouseEnter(sender, e);
            }
        }

        private void TeamComboBox_MouseEnter(object sender, EventArgs e)
        {
            ComboBox teamCombo = sender as ComboBox;
            string teamDescrTd = GetActionValueLabel(teamCombo, ArgType.TeamType);
            if (teamDescrTd != null)
            {
                ShowToolTip(toolTip1, teamCombo, teamDescrTd);
            }
            else if (tooltipShownOn != null && !tooltipShownOn.ClientRectangle.Contains(tooltipShownOn.PointToClient(Cursor.Position)))
            {
                HideToolTip(toolTip1, teamCombo, true);
            }
        }

        private void TeamComboBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (tooltipShownOn != sender)
            {
                TeamComboBox_MouseEnter(sender, e);
            }
        }

        private void ToolTipComboBox_MouseLeave(object sender, EventArgs e)
        {
            Control target = sender as Control;
            HideToolTip(toolTip1, target, false);
        }

        private void RemoveFromLayout(TableLayoutPanel panel, Label lblname, ComboBox cmbselect, FlowLayoutPanel flpargs)
        {
            panel.Controls.Remove(lblname);
            panel.Controls.Remove(cmbselect);
            panel.Controls.Remove(flpargs);
        }

        private void AddToLayout(TableLayoutPanel panel, Label lblname, ComboBox cmbselect, FlowLayoutPanel flpargs, int row)
        {
            panel.Controls.Add(lblname, 0, row);
            panel.Controls.Add(cmbselect, 1, row);
            panel.Controls.Add(flpargs, 2, row);
            panel.SetColumnSpan(flpargs, 2);
        }

        private void UpdateTriggerEventControls(TriggerEvent triggerEvent, NumericUpDown eventNud, ComboBox eventValueComboBox, ref ArgType eventArgType, bool dontHideTooltip,
            TriggerEvent triggerEventData)
        {
            eventNud.Visible = false;
            eventNud.DataBindings.Clear();
            eventNud.Minimum = Int32.MinValue;
            eventNud.Maximum = Int32.MaxValue;
            eventValueComboBox.Visible = false;
            eventValueComboBox.DataBindings.Clear();
            eventValueComboBox.DataSource = null;
            eventValueComboBox.DisplayMember = null;
            eventValueComboBox.ValueMember = null;
            long data = triggerEventData == null ? defaultData : triggerEventData.Data;
            string team = triggerEventData == null ? TeamType.None : triggerEventData.Team;
            eventArgType = ArgType.None;
            if (triggerEvent != null)
            {
                if (triggerEventData == null)
                {
                    triggerEvent.Data = defaultData;
                    triggerEvent.Team = TeamType.None;
                }
                else
                {
                    triggerEvent.FillDataFrom(triggerEventData);
                }
                long correctedData = triggerEvent.Data;
                switch (plugin.GameInfo.GameType)
                {
                    case GameType.TiberianDawn:
                    case GameType.SoleSurvivor:
                        switch (triggerEvent.EventType)
                        {
                            case TiberianDawn.EventTypes.EVENT_NONE:
                            case TiberianDawn.EventTypes.EVENT_PLAYER_ENTERED:
                            case TiberianDawn.EventTypes.EVENT_DISCOVERED:
                            case TiberianDawn.EventTypes.EVENT_ATTACKED:
                            case TiberianDawn.EventTypes.EVENT_DESTROYED:
                            case TiberianDawn.EventTypes.EVENT_ANY:
                            case TiberianDawn.EventTypes.EVENT_HOUSE_DISCOVERED:
                            case TiberianDawn.EventTypes.EVENT_UNITS_DESTROYED:
                            case TiberianDawn.EventTypes.EVENT_BUILDINGS_DESTROYED:
                            case TiberianDawn.EventTypes.EVENT_ALL_DESTROYED:
                            case TiberianDawn.EventTypes.EVENT_NOFACTORIES:
                            case TiberianDawn.EventTypes.EVENT_EVAC_CIVILIAN:
                                correctedData = 0;
                                triggerEvent.Data = correctedData;
                                //triggerEvent.Team = TeamType.None;
                                break;
                            case TiberianDawn.EventTypes.EVENT_TIME:
                            case TiberianDawn.EventTypes.EVENT_CREDITS:
                            case TiberianDawn.EventTypes.EVENT_NUNITS_DESTROYED:
                            case TiberianDawn.EventTypes.EVENT_NBUILDINGS_DESTROYED:
                                eventNud.Visible = true;
                                eventNud.Minimum = 0;
                                eventNud.Maximum = Int32.MaxValue;
                                correctedData = data.Restrict(0, Int32.MaxValue);
                                triggerEvent.Data = correctedData;
                                eventNud.Value = correctedData;
                                break;
                            case TiberianDawn.EventTypes.EVENT_BUILD:
                                eventValueComboBox.Visible = true;
                                eventValueComboBox.DisplayMember = "Label";
                                eventValueComboBox.ValueMember = "Value";
                                ListItem<long>[] bldData = plugin.Map.BuildingTypes.Select(t => ListItem.Create((long)t.ID, t.DisplayNameWithTheaterInfo)).ToArray();
                                eventValueComboBox.DataSource = bldData;
                                correctedData = ListItem.CheckInList(data, bldData);
                                triggerEvent.Data = correctedData;
                                eventValueComboBox.DataBindings.Add("SelectedValue", triggerEvent, "Data", true, DataSourceUpdateMode.OnPropertyChanged);
                                eventValueComboBox.SelectedValue = correctedData;
                                break;
                            default:
                                break;
                        }
                        break;
                    case GameType.RedAlert:
                        switch (triggerEvent.EventType)
                        {
                            case RedAlert.EventTypes.TEVENT_NONE:
                            case RedAlert.EventTypes.TEVENT_SPIED:
                            case RedAlert.EventTypes.TEVENT_DISCOVERED:
                            case RedAlert.EventTypes.TEVENT_ATTACKED:
                            case RedAlert.EventTypes.TEVENT_DESTROYED:
                            case RedAlert.EventTypes.TEVENT_ANY:
                            case RedAlert.EventTypes.TEVENT_MISSION_TIMER_EXPIRED:
                            case RedAlert.EventTypes.TEVENT_NOFACTORIES:
                            case RedAlert.EventTypes.TEVENT_EVAC_CIVILIAN:
                            case RedAlert.EventTypes.TEVENT_FAKES_DESTROYED:
                            case RedAlert.EventTypes.TEVENT_ALL_BRIDGES_DESTROYED:
                                // Special case: always blank out the info.
                                correctedData = 0;
                                triggerEvent.Data = correctedData;
                                triggerEvent.Team = TeamType.None;
                                break;
                            case RedAlert.EventTypes.TEVENT_LEAVES_MAP:
                                eventValueComboBox.Visible = true;
                                string[] teamData = TeamType.None.Yield().Concat(plugin.Map.TeamTypes.Select(t => t.Name)).ToArray();
                                eventValueComboBox.DataSource = teamData;
                                eventValueComboBox.DataBindings.Add("SelectedValue", triggerEvent, "Team", true, DataSourceUpdateMode.OnPropertyChanged);
                                correctedData = 0;
                                triggerEvent.Data = correctedData;
                                string correctedTeam = teamData.FirstOrDefault(tm => tm.Equals(team, StringComparison.OrdinalIgnoreCase)) ?? TeamType.None;
                                triggerEvent.Team = correctedTeam;
                                eventValueComboBox.SelectedItem = correctedTeam;
                                eventArgType = ArgType.TeamType;
                                break;
                            case RedAlert.EventTypes.TEVENT_PLAYER_ENTERED:
                            case RedAlert.EventTypes.TEVENT_CROSS_HORIZONTAL:
                            case RedAlert.EventTypes.TEVENT_CROSS_VERTICAL:
                            case RedAlert.EventTypes.TEVENT_ENTERS_ZONE:
                            case RedAlert.EventTypes.TEVENT_LOW_POWER:
                            case RedAlert.EventTypes.TEVENT_THIEVED:
                            case RedAlert.EventTypes.TEVENT_HOUSE_DISCOVERED:
                            case RedAlert.EventTypes.TEVENT_BUILDINGS_DESTROYED:
                            case RedAlert.EventTypes.TEVENT_UNITS_DESTROYED:
                            case RedAlert.EventTypes.TEVENT_ALL_DESTROYED:
                                eventValueComboBox.Visible = true;
                                eventValueComboBox.DisplayMember = "Label";
                                eventValueComboBox.ValueMember = "Value";
                                ListItem<long>[] houseData = ListItem.Create((long)-1, House.None).Yield()
                                    .Concat(plugin.Map.Houses.Select(t => ListItem.Create((long)t.Type.ID, t.Type.Name))).ToArray();
                                eventValueComboBox.DataSource = houseData;
                                correctedData = ListItem.CheckInList(data, houseData);
                                triggerEvent.Data = correctedData;
                                eventValueComboBox.DataBindings.Add("SelectedValue", triggerEvent, "Data", true, DataSourceUpdateMode.OnPropertyChanged);
                                eventValueComboBox.SelectedValue = correctedData;
                                break;
                            case RedAlert.EventTypes.TEVENT_BUILDING_EXISTS:
                            case RedAlert.EventTypes.TEVENT_BUILD:
                                eventValueComboBox.Visible = true;
                                eventValueComboBox.DisplayMember = "Label";
                                eventValueComboBox.ValueMember = "Value";
                                ListItem<long>[] bldData = plugin.Map.BuildingTypes.Select(t => ListItem.Create((long)t.ID, t.DisplayNameWithTheaterInfo)).ToArray();
                                eventValueComboBox.DataSource = bldData;
                                correctedData = ListItem.CheckInList(data, bldData);
                                triggerEvent.Data = correctedData;
                                eventValueComboBox.DataBindings.Add("SelectedValue", triggerEvent, "Data", true, DataSourceUpdateMode.OnPropertyChanged);
                                eventValueComboBox.SelectedValue = correctedData;
                                break;
                            case RedAlert.EventTypes.TEVENT_BUILD_UNIT:
                                eventValueComboBox.Visible = true;
                                eventValueComboBox.DisplayMember = "Label";
                                eventValueComboBox.ValueMember = "Value";
                                ListItem<long>[] unitData = plugin.Map.UnitTypes.OfType<VehicleType>().Select(t => ListItem.Create((long)t.ID, t.DisplayName)).ToArray();
                                eventValueComboBox.DataSource = unitData;
                                correctedData = ListItem.CheckInList(data, unitData);
                                triggerEvent.Data = correctedData;
                                eventValueComboBox.DataBindings.Add("SelectedValue", triggerEvent, "Data", true, DataSourceUpdateMode.OnPropertyChanged);
                                eventValueComboBox.SelectedValue = correctedData;
                                break;
                            case RedAlert.EventTypes.TEVENT_BUILD_INFANTRY:
                                eventValueComboBox.Visible = true;
                                eventValueComboBox.DisplayMember = "Label";
                                eventValueComboBox.ValueMember = "Value";
                                ListItem<long>[] infData = plugin.Map.InfantryTypes.Select(t => ListItem.Create((long)t.ID, t.DisplayName)).ToArray();
                                eventValueComboBox.DataSource = infData;
                                correctedData = ListItem.CheckInList(data, infData);
                                triggerEvent.Data = correctedData;
                                eventValueComboBox.DataBindings.Add("SelectedValue", triggerEvent, "Data", true, DataSourceUpdateMode.OnPropertyChanged);
                                eventValueComboBox.SelectedValue = correctedData;
                                break;
                            case RedAlert.EventTypes.TEVENT_BUILD_AIRCRAFT:
                                eventValueComboBox.Visible = true;
                                eventValueComboBox.DisplayMember = "Label";
                                eventValueComboBox.ValueMember = "Value";
                                ListItem<long>[] airData = plugin.Map.TeamTechnoTypes.OfType<AircraftType>()
                                    .Select(t => ListItem.Create((long)t.ID, t.DisplayName)).ToArray();
                                eventValueComboBox.DataSource = airData;
                                correctedData = ListItem.CheckInList(data, airData);
                                triggerEvent.Data = correctedData;
                                eventValueComboBox.DataBindings.Add("SelectedValue", triggerEvent, "Data", true, DataSourceUpdateMode.OnPropertyChanged);
                                eventValueComboBox.SelectedValue = correctedData;
                                break;
                            case RedAlert.EventTypes.TEVENT_NUNITS_DESTROYED:
                            case RedAlert.EventTypes.TEVENT_NBUILDINGS_DESTROYED:
                            case RedAlert.EventTypes.TEVENT_CREDITS:
                            case RedAlert.EventTypes.TEVENT_TIME:
                                eventNud.Visible = true;
                                eventNud.Minimum = 0;
                                eventNud.Maximum = Int32.MaxValue;
                                correctedData = data.Restrict(0, Int32.MaxValue);
                                triggerEvent.Data = correctedData;
                                eventNud.Value = correctedData;
                                break;
                            case RedAlert.EventTypes.TEVENT_GLOBAL_SET:
                            case RedAlert.EventTypes.TEVENT_GLOBAL_CLEAR:
                                eventNud.Visible = true;
                                eventNud.Minimum = 0;
                                eventNud.Maximum = RedAlert.Constants.HighestGlobal;
                                correctedData = data.Restrict(0, RedAlert.Constants.HighestGlobal);
                                triggerEvent.Data = correctedData;
                                eventNud.Value = correctedData;
                                break;
                            default:
                                break;
                        }
                        break;
                }
            }
        }

        private void UpdateTriggerActionControls(TriggerAction triggerAction, NumericUpDown actionNud, ComboBox actionValueComboBox, ref ArgType actionArgType,
            bool dontHideTooltip, TriggerAction triggerActionData)
        {
            actionNud.Visible = false;
            actionNud.DataBindings.Clear();
            actionNud.Minimum = Int32.MinValue;
            actionNud.Maximum = Int32.MaxValue;
            actionValueComboBox.Visible = false;
            actionValueComboBox.DataBindings.Clear();
            actionValueComboBox.DataSource = null;
            actionValueComboBox.DisplayMember = null;
            actionValueComboBox.ValueMember = null;
            long data = triggerActionData == null ? defaultData : triggerActionData.Data;
            string team = triggerActionData == null ? TeamType.None : triggerActionData.Team;
            string trig = triggerActionData == null ? Trigger.None : triggerActionData.Trigger;
            actionArgType = ArgType.None;
            if (triggerAction != null)
            {
                if (triggerActionData == null)
                {
                    triggerAction.Data = defaultData;
                    triggerAction.Trigger = Trigger.None;
                    triggerAction.Team = TeamType.None;
                }
                else
                {
                    triggerAction.FillDataFrom(triggerActionData);
                }
                bool tooltipShown = false;
                long correctedData = triggerAction.Data;
                switch (plugin.GameInfo.GameType)
                {
                    case GameType.RedAlert:
                        switch (triggerAction.ActionType)
                        {
                            case RedAlert.ActionTypes.TACTION_CREATE_TEAM:
                            case RedAlert.ActionTypes.TACTION_DESTROY_TEAM:
                            case RedAlert.ActionTypes.TACTION_REINFORCEMENTS:
                                actionValueComboBox.Visible = true;
                                string[] teamData = TeamType.None.Yield().Concat(plugin.Map.TeamTypes.Select(t => t.Name)).ToArray();
                                actionValueComboBox.DataSource = teamData;
                                string correctedTeam = teamData.FirstOrDefault(tm => tm.Equals(team, StringComparison.OrdinalIgnoreCase)) ?? TeamType.None;
                                triggerAction.Team = correctedTeam;
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Team", true, DataSourceUpdateMode.OnPropertyChanged);
                                actionValueComboBox.SelectedItem = correctedTeam;
                                string teamTooltip = GetTeamLabel(correctedTeam);
                                actionArgType = ArgType.TeamType;
                                if (teamTooltip != null && actionValueComboBox.ClientRectangle.Contains(actionValueComboBox.PointToClient(Cursor.Position)))
                                {
                                    ShowToolTip(toolTip1, actionValueComboBox, teamTooltip);
                                    tooltipShown = true;
                                }
                                break;
                            case RedAlert.ActionTypes.TACTION_WIN:
                            case RedAlert.ActionTypes.TACTION_LOSE:
                            case RedAlert.ActionTypes.TACTION_BEGIN_PRODUCTION:
                            case RedAlert.ActionTypes.TACTION_FIRE_SALE:
                            case RedAlert.ActionTypes.TACTION_AUTOCREATE:
                            case RedAlert.ActionTypes.TACTION_ALL_HUNT:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DisplayMember = "Label";
                                actionValueComboBox.ValueMember = "Value";
                                ListItem<long>[] houseData = ListItem.Create((long)-1, House.None).Yield().Concat(
                                    plugin.Map.Houses.Select(t => ListItem.Create((long)t.Type.ID, t.Type.Name))).ToArray();
                                actionValueComboBox.DataSource = houseData;
                                correctedData = ListItem.CheckInList(data, houseData);
                                triggerAction.Data = correctedData;
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Data", true, DataSourceUpdateMode.OnPropertyChanged);
                                actionValueComboBox.SelectedValue = correctedData;
                                break;
                            case RedAlert.ActionTypes.TACTION_FORCE_TRIGGER:
                            case RedAlert.ActionTypes.TACTION_DESTROY_TRIGGER:
                                actionValueComboBox.Visible = true;
                                var trigsData = Trigger.None.Yield().Concat(triggers.Select(t => t.Name).OrderBy(t => t, new ExplorerComparer())).ToArray();
                                actionValueComboBox.DataSource = trigsData;
                                string correctedTrigger = trigsData.Contains(trig, StringComparer.OrdinalIgnoreCase) ? trig : Trigger.None;
                                triggerAction.Trigger = correctedTrigger;
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Trigger", true, DataSourceUpdateMode.OnPropertyChanged);
                                actionValueComboBox.SelectedItem = correctedTrigger;
                                actionArgType = ArgType.Trigger;
                                string trigTooltip = RefreshTriggerLabel(correctedTrigger);
                                if (trigTooltip != null && actionValueComboBox.ClientRectangle.Contains(actionValueComboBox.PointToClient(Cursor.Position)))
                                {
                                    ShowToolTip(toolTip1, actionValueComboBox, trigTooltip);
                                    tooltipShown = true;
                                }
                                break;
                            case RedAlert.ActionTypes.TACTION_DZ:
                            case RedAlert.ActionTypes.TACTION_REVEAL_SOME:
                            case RedAlert.ActionTypes.TACTION_REVEAL_ZONE:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DisplayMember = "Label";
                                actionValueComboBox.ValueMember = "Value";
                                Waypoint[] wps = plugin.Map.Waypoints;
                                ListItem<long>[] wpsData = ListItem.Create((long)-1, Waypoint.None).Yield().Concat(
                                    wps.Select((wp, i) => ListItem.Create((long)i, wp.ToString()))).ToArray();
                                actionValueComboBox.DataSource = wpsData;
                                correctedData = ListItem.CheckInList(data, wpsData);
                                triggerAction.Data = correctedData;
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Data", true, DataSourceUpdateMode.OnPropertyChanged);
                                actionValueComboBox.SelectedValue = correctedData;
                                break;
                            case RedAlert.ActionTypes.TACTION_1_SPECIAL:
                            case RedAlert.ActionTypes.TACTION_FULL_SPECIAL:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DisplayMember = "Label";
                                actionValueComboBox.ValueMember = "Value";
                                ListItem<long>[] superData = ListItem.Create((long)-1, "None").Yield().Concat(
                                    RedAlert.ActionDataTypes.SuperTypes.Select((t, i) => ListItem.Create((long)i, t)))
                                    .OrderBy(t => t.Label).ToArray();
                                actionValueComboBox.DataSource = superData;
                                correctedData = ListItem.CheckInList(data, superData);
                                triggerAction.Data = correctedData;
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Data", true, DataSourceUpdateMode.OnPropertyChanged);
                                actionValueComboBox.SelectedValue = correctedData;
                                break;
                            case RedAlert.ActionTypes.TACTION_PLAY_MUSIC:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DisplayMember = "Label";
                                actionValueComboBox.ValueMember = "Value";
                                List<ListItem<long>> musData = plugin.Map.ThemeTypes.Select((t, i) => ListItem.Create((long)i - 1, t)).OrderBy(t => t.Label).ToList();
                                ListItem<long> musDefItem = musData.Where(t => t.Value == -1).FirstOrDefault();
                                if (musDefItem != null)
                                {
                                    musData.Remove(musDefItem);
                                    musData.Insert(0, musDefItem);
                                }
                                actionValueComboBox.DataSource = musData.ToArray();
                                correctedData = ListItem.CheckInList(data, musData);
                                triggerAction.Data = correctedData;
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Data", true, DataSourceUpdateMode.OnPropertyChanged);
                                actionValueComboBox.SelectedValue = correctedData;
                                break;
                            case RedAlert.ActionTypes.TACTION_PLAY_MOVIE:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DisplayMember = "Label";
                                actionValueComboBox.ValueMember = "Value";
                                ExplorerComparer sorter = new ExplorerComparer();
                                // First video is the "None" entry; expose as -1.
                                List<ListItem<long>> movData = plugin.Map.MovieTypes.Select((t, i) => ListItem.Create((long)(i - 1), t)).OrderBy(t => t.Label, sorter).ToList();
                                ListItem<long> movDefItem = movData.Where(t => t.Value == -1).FirstOrDefault();
                                if (movDefItem != null)
                                {
                                    movData.Remove(movDefItem);
                                    movData.Insert(0, movDefItem);
                                }
                                actionValueComboBox.DataSource = movData.ToArray();
                                correctedData = ListItem.CheckInList(data, movData);
                                triggerAction.Data = correctedData;
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Data", true, DataSourceUpdateMode.OnPropertyChanged);
                                actionValueComboBox.SelectedValue = correctedData;
                                break;
                            case RedAlert.ActionTypes.TACTION_PLAY_SOUND:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DisplayMember = "Label";
                                actionValueComboBox.ValueMember = "Value";
                                ListItem<long>[] vocData = ListItem.Create((long)-1, "None").Yield().Concat(
                                    RedAlert.ActionDataTypes.VocDesc.Select((t, i) => ListItem.Create((long)i, t + " (" + RedAlert.ActionDataTypes.VocNames[i] + ")"))
                                    .Where(t => !String.Equals(RedAlert.ActionDataTypes.VocNames[t.Value], "x", StringComparison.OrdinalIgnoreCase))).ToArray();
                                actionValueComboBox.DataSource = vocData;
                                correctedData = ListItem.CheckInList(data, vocData);
                                triggerAction.Data = correctedData;
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Data", true, DataSourceUpdateMode.OnPropertyChanged);
                                actionValueComboBox.SelectedValue = correctedData;
                                break;
                            case RedAlert.ActionTypes.TACTION_PLAY_SPEECH:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DisplayMember = "Label";
                                actionValueComboBox.ValueMember = "Value";
                                ListItem<long>[] voxData = ListItem.Create((long)-1, "None").Yield().Concat(
                                    RedAlert.ActionDataTypes.VoxDesc.Select((t, i) => ListItem.Create((long)i, t + " (" + RedAlert.ActionDataTypes.VoxNames[i] + ")"))
                                    .Where(t => !String.Equals(RedAlert.ActionDataTypes.VoxNames[t.Value], "none", StringComparison.OrdinalIgnoreCase))).ToArray();
                                actionValueComboBox.DataSource = voxData;
                                correctedData = ListItem.CheckInList(data, voxData);
                                triggerAction.Data = correctedData;
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Data", true, DataSourceUpdateMode.OnPropertyChanged);
                                actionValueComboBox.SelectedValue = correctedData;
                                break;
                            case RedAlert.ActionTypes.TACTION_PREFERRED_TARGET:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DisplayMember = "Label";
                                actionValueComboBox.ValueMember = "Value";
                                ListItem<long>[] quarryData = RedAlert.TeamMissionTypes.Attack.DropdownOptions.Select(t => ListItem.Create((long)t.Value, t.Label)).ToArray();
                                actionValueComboBox.DataSource = quarryData;
                                correctedData = ListItem.CheckInList(data, quarryData);
                                triggerAction.Data = correctedData;
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Data", true, DataSourceUpdateMode.OnPropertyChanged);
                                actionValueComboBox.SelectedValue = correctedData;
                                break;
                            case RedAlert.ActionTypes.TACTION_BASE_BUILDING:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DisplayMember = "Label";
                                actionValueComboBox.ValueMember = "Value";
                                ListItem<long>[] trueFalseData = new long[] { 0, 1 }.Select(b => ListItem.Create(b, b == 0 ? "Off" : "On")).ToArray();
                                actionValueComboBox.DataSource = trueFalseData;
                                correctedData = ListItem.CheckInList(data, trueFalseData);
                                triggerAction.Data = correctedData;
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Data", true, DataSourceUpdateMode.OnPropertyChanged);
                                actionValueComboBox.SelectedValue = correctedData;
                                break;
                            case RedAlert.ActionTypes.TACTION_TEXT_TRIGGER:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DisplayMember = "Label";
                                actionValueComboBox.ValueMember = "Value";
                                ListItem<long>[] txtData = RedAlert.ActionDataTypes.TextDesc
                                    .Select((t, i) => ListItem.Create((long)i + 1, (i + 1).ToString("000") + " " + t)).ToArray();
                                actionValueComboBox.DataSource = txtData;
                                correctedData = ListItem.CheckInList(data, txtData);
                                triggerAction.Data = correctedData;
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Data", true, DataSourceUpdateMode.OnPropertyChanged);
                                actionValueComboBox.SelectedValue = correctedData;
                                break;
                            case RedAlert.ActionTypes.TACTION_ADD_TIMER:
                            case RedAlert.ActionTypes.TACTION_SUB_TIMER:
                            case RedAlert.ActionTypes.TACTION_SET_TIMER:
                                actionNud.Minimum = 0;
                                actionNud.Maximum = Int32.MaxValue;
                                actionNud.Visible = true;
                                correctedData = data.Restrict(0, Int32.MaxValue);
                                triggerAction.Data = correctedData;
                                actionNud.Value = correctedData;
                                break;
                            case RedAlert.ActionTypes.TACTION_SET_GLOBAL:
                            case RedAlert.ActionTypes.TACTION_CLEAR_GLOBAL:
                                actionNud.Minimum = 0;
                                actionNud.Maximum = 29;
                                actionNud.Visible = true;
                                correctedData = data.Restrict(0, 29);
                                triggerAction.Data = correctedData;
                                actionNud.Value = correctedData;
                                break;
                            default:
                                break;
                        }
                        break;
                }
                if (!tooltipShown)
                {
                    HideToolTip(toolTip1, actionValueComboBox, false);
                }
            }
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            if (Trigger.CheckForChanges(triggers, backupTriggers))
            {
                DialogResult dr = MessageBox.Show(this, "Warning! There are changes in the triggers. This function works best if the triggers match the state of the currently edited map. Are you sure you want to continue?", "Triggers check", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.No)
                {
                    return;
                }
            }
            string[] errors = plugin.CheckTriggers(triggers, true, false, false, out _, false, out _)?.ToArray();
            if (errors == null || errors.Length == 0)
            {
                MessageBox.Show(this, "No issues were encountered.", "Triggers check", MessageBoxButtons.OK);
                return;
            }
            using (ErrorMessageBox emb = new ErrorMessageBox())
            {
                emb.Title = "Triggers check";
                emb.Message = "The following issues were encountered:";
                emb.Errors = errors;
                emb.UseWordWrap = true;
                emb.ShowDialog();
            }
        }

        private string GetActionValueLabel(ComboBox cbbVal, ArgType type)
        {
            if (cbbVal == null || !cbbVal.Visible || cbbVal.Items.Count == 0 || cbbVal.SelectedIndex == -1 || cbbVal.SelectedItem == null)
            {
                return null;
            }
            string selected = cbbVal.SelectedItem.ToString();
            switch (type)
            {
                case ArgType.TeamType:
                    return GetTeamLabel(selected);
                case ArgType.Trigger:
                    return RefreshTriggerLabel(selected);
            }
            return null;
        }

        private string GetTeamLabel(string teamtypeName)
        {
            TeamType teamtype = plugin.Map.TeamTypes.FirstOrDefault(t => t.Name == teamtypeName);
            if (teamtype != null)
            {
                return teamtype.GetSummaryLabel(true);
            }
            return null;
        }

        private string RefreshTriggerLabel(string triggerName)
        {
            Trigger trigger = triggers.FirstOrDefault(t => t.Name.Equals(triggerName, StringComparison.OrdinalIgnoreCase));
            return trigger == null ? null : plugin.TriggerSummary(trigger, true, false);
        }

        private bool ShowToolTip(ToolTip toolTip, Control target, string message)
        {
            if (target == null || message == null || !target.ClientRectangle.Contains(target.PointToClient(Cursor.Position)))
            {
                HideToolTip(toolTip, target, true);
                return false;
            }
            Point resPoint = target.PointToScreen(new Point(0, target.Height));
            MethodInfo m = toolTip.GetType().GetMethod("SetTool",
                       BindingFlags.Instance | BindingFlags.NonPublic);
            m.Invoke(toolTip, new object[] { target, message, 2, resPoint });
            tooltipShownOn = target;
            return true;
        }

        private void HideToolTip(ToolTip toolTip, Control target, bool hideIfNotcurrent)
        {
            try
            {
                if (hideIfNotcurrent && tooltipShownOn != target && tooltipShownOn != null)
                {
                    toolTip.Hide(tooltipShownOn);
                }
                if (target != null)
                {
                    toolTip.Hide(target);
                }
            }
            catch { /* ignore */ }
            tooltipShownOn = null;
        }
    }
}
