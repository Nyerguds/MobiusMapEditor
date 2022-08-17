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
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Tools;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class TriggersDialog : Form
    {
        private readonly IGamePlugin plugin;
        private readonly int maxTriggers;

        private readonly List<Trigger> backupTriggers;
        private readonly List<Trigger> triggers;
        public List<Trigger> Triggers => triggers;

        private ListViewItem SelectedItem => (triggersListView.SelectedItems.Count > 0) ? triggersListView.SelectedItems[0] : null;

        private Trigger SelectedTrigger => SelectedItem?.Tag as Trigger;

        public TriggersDialog(IGamePlugin plugin, int maxTriggers)
        {
            this.plugin = plugin;
            this.maxTriggers = maxTriggers;

            InitializeComponent();

            switch (plugin.GameType)
            {
                case GameType.TiberianDawn:
                    existenceLabel.Text = "Loop";
                    event1Label.Text = "Event";
                    action1Label.Text = "Action";
                    typeLabel.Visible = typeComboBox.Visible = false;
                    event2Label.Visible = event2ComboBox.Visible = event2Flp.Visible = false;
                    action2Label.Visible = action2ComboBox.Visible = action2Flp.Visible = false;
                    btnCheck.Visible = true;
                    break;
                case GameType.RedAlert:
                    teamLabel.Visible = teamComboBox.Visible = false;
                    btnCheck.Visible = false;
                    break;
            }

            triggers = new List<Trigger>(plugin.Map.Triggers.Select(t => t.Clone()));
            backupTriggers = new List<Trigger>(plugin.Map.Triggers.Select(t => t.Clone()));
            int nrOfTriggers = Math.Min(maxTriggers, triggers.Count);
            btnAdd.Enabled = nrOfTriggers < maxTriggers;
            triggersListView.BeginUpdate();
            {
                for (int i = 0; i < nrOfTriggers; ++i)
                {
                    Trigger trigger = triggers[i];
                    var item = new ListViewItem(trigger.Name)
                    {
                        Tag = trigger
                    };
                    triggersListView.Items.Add(item).ToolTipText = trigger.Name;
                }
            }
            triggersListView.EndUpdate();

            string[] existenceNames = Enum.GetNames(typeof(TriggerPersistentType));
            switch (plugin.GameType)
            {
                case GameType.TiberianDawn:
                    existenceNames = new string[] { "No", "And", "Or" };
                    break;
                case GameType.RedAlert:
                    existenceNames = new string[] { "Temporary", "Semi-Constant", "Constant" };
                    break;
            }

            string[] typeNames = new string[]
            {
                "E => A1 [+ A2]",
                "E1 && E2 => A1 [+ A2]",
                "E1 || E2 => A1 [+ A2]",
                "E1 => A1; E2 => A2",
            };

            houseComboBox.DataSource = House.None.Yield().Concat(plugin.Map.Houses.Select(t => t.Type.Name)).ToArray();
            existenceComboBox.DataSource = Enum.GetValues(typeof(TriggerPersistentType)).Cast<int>()
                .Select(v => new { Name = existenceNames[v], Value = (TriggerPersistentType)v })
                .ToArray();
            typeComboBox.DataSource = Enum.GetValues(typeof(TriggerMultiStyleType)).Cast<int>()
                .Select(v => new { Name = typeNames[v], Value = (TriggerMultiStyleType)v })
                .ToArray();
            event1ComboBox.DataSource = plugin.Map.EventTypes.Where(t => !string.IsNullOrEmpty(t)).ToArray();
            event2ComboBox.DataSource = plugin.Map.EventTypes.Where(t => !string.IsNullOrEmpty(t)).ToArray();
            action1ComboBox.DataSource = plugin.Map.ActionTypes.Where(t => !string.IsNullOrEmpty(t)).ToArray();
            action2ComboBox.DataSource = plugin.Map.ActionTypes.Where(t => !string.IsNullOrEmpty(t)).ToArray();
            teamComboBox.DataSource = TeamType.None.Yield().Concat(plugin.Map.TeamTypes.Select(t => t.Name)).ToArray();

            triggersTableLayoutPanel.Visible = false;
        }

        private void triggersListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            houseComboBox.DataBindings.Clear();
            existenceComboBox.DataBindings.Clear();
            typeComboBox.DataBindings.Clear();
            // no longer managed by data bindings; too many weird race conditions.
            //event1ComboBox.DataBindings.Clear();
            //event2ComboBox.DataBindings.Clear();
            //action1ComboBox.DataBindings.Clear();
            //action2ComboBox.DataBindings.Clear();
            teamComboBox.DataBindings.Clear();

            if (SelectedTrigger != null)
            {
                houseComboBox.DataBindings.Add("SelectedItem", SelectedTrigger, "House");
                existenceComboBox.DataBindings.Add("SelectedValue", SelectedTrigger, "PersistentType");
                // Set event 1
                TriggerEvent evt1 = SelectedTrigger.Event1.Clone();
                event1ComboBox.SelectedItem = SelectedTrigger.Event1.EventType;
                UpdateTriggerEventControls(SelectedTrigger.Event1, event1Nud, event1ValueComboBox, evt1);
                // Set action 1
                TriggerAction act1 = SelectedTrigger.Action1.Clone();
                action1ComboBox.SelectedItem = SelectedTrigger.Action1.ActionType;
                UpdateTriggerActionControls(SelectedTrigger.Action1, action1Nud, action1ValueComboBox, act1);
                switch (plugin.GameType)
                {
                    case GameType.TiberianDawn:
                        teamComboBox.DataBindings.Add("SelectedItem", SelectedTrigger.Action1, "Team");
                        break;
                    case GameType.RedAlert:
                        typeComboBox.DataBindings.Add("SelectedValue", SelectedTrigger, "EventControl");
                        // Set event 2
                        TriggerEvent evt2 = SelectedTrigger.Event2.Clone();
                        event2ComboBox.SelectedItem = SelectedTrigger.Event2.EventType;
                        UpdateTriggerEventControls(SelectedTrigger.Event2, event2Nud, event2ValueComboBox, evt2);
                        // Set action 2
                        TriggerAction act2 = SelectedTrigger.Action2.Clone();
                        action2ComboBox.SelectedItem = SelectedTrigger.Action2?.ActionType;
                        UpdateTriggerActionControls(SelectedTrigger.Action2, action2Nud, action2ValueComboBox, act2);                        
                        break;
                }

                triggersTableLayoutPanel.Visible = true;
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
                addTriggerToolStripMenuItem.Visible = true;
                addTriggerToolStripMenuItem.Enabled = triggersListView.Items.Count < maxTriggers;
                renameTriggerToolStripMenuItem.Visible = itemExists;
                removeTriggerToolStripMenuItem.Visible = itemExists;
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
        }

        private void TriggersDialog_KeyDown(Object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.A | Keys.Control))
            {
                AddTrigger();
            }
        }

        private void Event1Nud_ValueChanged(object sender, EventArgs e)
        {
            Trigger trig = SelectedTrigger;
            if (trig != null && trig.Event1 != null)
            {
                trig.Event1.Data = (long)event1Nud.Value;
            }
        }

        private void Event2Nud_ValueChanged(object sender, EventArgs e)
        {
            Trigger trig = SelectedTrigger;
            if (trig != null && trig.Event2 != null)
            {
                trig.Event2.Data = (long)event2Nud.Value;
            }
        }

        private void Action1Nud_ValueChanged(object sender, EventArgs e)
        {
            Trigger trig = SelectedTrigger;
            if (trig != null && trig.Action1 != null)
            {
                trig.Action1.Data = (long)action1Nud.Value;
            }
        }

        private void Action2Nud_ValueChanged(object sender, EventArgs e)
        {
            Trigger trig = SelectedTrigger;
            if (trig != null && trig.Action2 != null)
            {
                trig.Action2.Data = (long)action2Nud.Value;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddTrigger();
        }
                
        private void TriggersDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If user pressed ok, nevermind,just go on.
            if (this.DialogResult == DialogResult.OK)
                return;
            bool hasChanges = Trigger.CheckForChanges(triggers, backupTriggers);
            if (hasChanges)
            {
                DialogResult dr =  MessageBox.Show("Triggers have been changed! Are you sure you want to cancel?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                    return;
                this.DialogResult = DialogResult.None;
                e.Cancel = true;
            }
        }

        private void addTriggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddTrigger();
        }

        
        private void renameTriggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedItem != null)
                SelectedItem.BeginEdit();
        }

        private void removeTriggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveTrigger();
        }

        private void AddTrigger()
        {
            if (triggersListView.Items.Count >= maxTriggers)
                return;
            string name = GeneralUtils.MakeNew4CharName(triggers.Select(t => t.Name), "????", Trigger.None);
            var trigger = new Trigger { Name = name, House = plugin.Map.HouseTypes.First().Name };
            var item = new ListViewItem(trigger.Name)
            {
                Tag = trigger
            };
            triggers.Add(trigger);
            triggersListView.Items.Add(item).ToolTipText = trigger.Name;
            btnAdd.Enabled = triggers.Count < maxTriggers;
            item.Selected = true;
            item.BeginEdit();
        }

        private void RemoveTrigger()
        {
            ListViewItem selected = SelectedItem;
            int index = triggersListView.SelectedIndices.Count == 0 ? -1 : triggersListView.SelectedIndices[0];
            if (selected != null)
            {
                triggers.Remove(selected.Tag as Trigger);
                triggersListView.Items.Remove(selected);
            }
            if (triggersListView.Items.Count == index)
                index--;
            if (index >= 0 && triggersListView.Items.Count > index)
                triggersListView.Items[index].Selected = true;
            btnAdd.Enabled = triggers.Count < maxTriggers;
        }

        private void triggersListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            int maxLength = int.MaxValue;
            switch (plugin.GameType)
            {
                case GameType.TiberianDawn:
                    maxLength = 4;
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
                MessageBox.Show(string.Format("Trigger name is longer than {0} characters.", maxLength), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (Trigger.None.Equals(curName, StringComparison.InvariantCultureIgnoreCase))
            {
                e.CancelEdit = true;
                MessageBox.Show(string.Format("Trigger name 'None' is reserved and cannot be used."), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!INIHelpers.IsValidKey(curName))
            {
                e.CancelEdit = true;
                MessageBox.Show(string.Format("Trigger name '{0}' contains illegal characters. This format only supports simple ASCII, and cannot contain '=', '[' or ']'.", curName), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (triggers.Where(t => (t != SelectedTrigger) && t.Name.Equals(curName, StringComparison.InvariantCultureIgnoreCase)).Any())
            {
                e.CancelEdit = true;
                MessageBox.Show(string.Format("Trigger with name '{0}' already exists.", curName.ToUpperInvariant()), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                SelectedTrigger.Name = curName;
                triggersListView.Items[e.Item].ToolTipText = SelectedTrigger.Name;
            }
        }

        private void typeComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (plugin.GameType == GameType.RedAlert)
            {
                var eventType = (TriggerMultiStyleType)typeComboBox.SelectedValue;
                event2Label.Visible = event2ComboBox.Visible = event2Flp.Visible = eventType != TriggerMultiStyleType.Only;
            }
        }

        private void Event1ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedTrigger != null)
            {
                SelectedTrigger.Event1.EventType = event1ComboBox.SelectedItem.ToString();
            }
            UpdateTriggerEventControls(SelectedTrigger?.Event1, event1Nud, event1ValueComboBox, null);
        }

        private void Action1ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedTrigger != null)
            {
                SelectedTrigger.Action1.ActionType = action1ComboBox.SelectedItem.ToString();
            }
            UpdateTriggerActionControls(SelectedTrigger?.Action1, action1Nud, action1ValueComboBox, null);
        }

        private void Event2ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedTrigger != null)
            {
                SelectedTrigger.Event2.EventType = event2ComboBox.SelectedItem.ToString();
            }
            UpdateTriggerEventControls(SelectedTrigger?.Event2, event2Nud, event2ValueComboBox, null);
        }

        private void Action2ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedTrigger != null)
            {
                SelectedTrigger.Action2.ActionType = action2ComboBox.SelectedItem.ToString();
            }
            UpdateTriggerActionControls(SelectedTrigger?.Action2, action2Nud, action2ValueComboBox, null);
        }

        private void UpdateTriggerEventControls(TriggerEvent triggerEvent, NumericUpDown eventNud, ComboBox eventValueComboBox, TriggerEvent triggerEventData)
        {
            eventNud.Visible = false;
            eventNud.DataBindings.Clear();
            eventValueComboBox.Visible = false;
            eventValueComboBox.DataBindings.Clear();
            eventValueComboBox.DataSource = null;
            eventValueComboBox.DisplayMember = null;
            eventValueComboBox.ValueMember = null;
            if (triggerEvent != null)
            {
                if (triggerEventData == null)
                {
                    triggerEvent.Data = 0;
                    triggerEvent.Team = null;
                }
                else
                {
                    triggerEvent.FillDataFrom(triggerEventData);
                }
                switch (plugin.GameType)
                {
                    case GameType.TiberianDawn:
                        switch (triggerEvent.EventType)
                        {
                            case TiberianDawn.EventTypes.EVENT_TIME:
                            case TiberianDawn.EventTypes.EVENT_CREDITS:
                            case TiberianDawn.EventTypes.EVENT_NUNITS_DESTROYED:
                            case TiberianDawn.EventTypes.EVENT_NBUILDINGS_DESTROYED:
                                eventNud.Visible = true;
                                eventNud.Value = triggerEventData != null ? triggerEventData.Data : 0;
                                break;
                            case TiberianDawn.EventTypes.EVENT_BUILD:
                                eventValueComboBox.Visible = true;
                                eventValueComboBox.DisplayMember = "Name";
                                eventValueComboBox.ValueMember = "Value";
                                eventValueComboBox.DataSource = plugin.Map.BuildingTypes.Select(t => new { Name = t.DisplayName, Value = (long)t.ID }).ToArray();
                                eventValueComboBox.DataBindings.Add("SelectedValue", triggerEvent, "Data");
                                if (triggerEventData == null)
                                {
                                    eventValueComboBox.SelectedIndex = 0;
                                }
                                else
                                {
                                    eventValueComboBox.SelectedValue = triggerEventData.Data;
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    case GameType.RedAlert:
                        switch (triggerEvent.EventType)
                        {
                            case RedAlert.EventTypes.TEVENT_LEAVES_MAP:
                                eventValueComboBox.Visible = true;
                                eventValueComboBox.DataSource = plugin.Map.TeamTypes.Count == 0 ? new[] { TeamType.None } : plugin.Map.TeamTypes.Select(t => t.Name).ToArray();
                                eventValueComboBox.DataBindings.Add("SelectedItem", triggerEvent, "Team");
                                if (triggerEventData == null)
                                {
                                    eventValueComboBox.SelectedIndex = 0;
                                }
                                else
                                {
                                    eventValueComboBox.SelectedItem = triggerEventData.Team;
                                }
                                break;
                            case RedAlert.EventTypes.TEVENT_PLAYER_ENTERED:
                            case RedAlert.EventTypes.TEVENT_CROSS_HORIZONTAL:
                            case RedAlert.EventTypes.TEVENT_CROSS_VERTICAL:
                            case RedAlert.EventTypes.TEVENT_ENTERS_ZONE:
                            case RedAlert.EventTypes.TEVENT_LOW_POWER:
                            case RedAlert.EventTypes.TEVENT_SPIED:
                            case RedAlert.EventTypes.TEVENT_THIEVED:
                            case RedAlert.EventTypes.TEVENT_HOUSE_DISCOVERED:
                            case RedAlert.EventTypes.TEVENT_BUILDINGS_DESTROYED:
                            case RedAlert.EventTypes.TEVENT_UNITS_DESTROYED:
                            case RedAlert.EventTypes.TEVENT_ALL_DESTROYED:
                                eventValueComboBox.Visible = true;
                                eventValueComboBox.DisplayMember = "Name";
                                eventValueComboBox.ValueMember = "Value";
                                eventValueComboBox.DataSource = new { Name = House.None, Value = (long)-1 }.Yield().Concat(plugin.Map.Houses.Select(t => new { t.Type.Name, Value = (long)t.Type.ID })).ToArray();
                                eventValueComboBox.DataBindings.Add("SelectedValue", triggerEvent, "Data");
                                if (triggerEventData == null)
                                {
                                    eventValueComboBox.SelectedIndex = 0;
                                }
                                else
                                {
                                    eventValueComboBox.SelectedValue = triggerEventData.Data;
                                }
                                break;
                            case RedAlert.EventTypes.TEVENT_BUILDING_EXISTS:
                            case RedAlert.EventTypes.TEVENT_BUILD:
                                eventValueComboBox.Visible = true;
                                eventValueComboBox.DisplayMember = "Name";
                                eventValueComboBox.ValueMember = "Value";
                                eventValueComboBox.DataSource = plugin.Map.BuildingTypes.Select(t => new { Name = t.DisplayName, Value = (long)t.ID }).ToArray();
                                eventValueComboBox.DataBindings.Add("SelectedValue", triggerEvent, "Data");
                                if (triggerEventData == null)
                                {
                                    eventValueComboBox.SelectedIndex = 0;
                                }
                                else
                                {
                                    eventValueComboBox.SelectedValue = triggerEventData.Data;
                                }
                                break;
                            case RedAlert.EventTypes.TEVENT_BUILD_UNIT:
                                eventValueComboBox.Visible = true;
                                eventValueComboBox.DisplayMember = "Name";
                                eventValueComboBox.ValueMember = "Value";
                                eventValueComboBox.DataSource = plugin.Map.UnitTypes.Where(t => t.IsUnit).Select(t => new { Name = t.DisplayName, Value = (long)t.ID }).ToArray();
                                eventValueComboBox.DataBindings.Add("SelectedValue", triggerEvent, "Data");
                                if (triggerEventData == null)
                                {
                                    eventValueComboBox.SelectedIndex = 0;
                                }
                                else
                                {
                                    eventValueComboBox.SelectedValue = triggerEventData.Data;
                                }
                                break;
                            case RedAlert.EventTypes.TEVENT_BUILD_INFANTRY:
                                eventValueComboBox.Visible = true;
                                eventValueComboBox.DisplayMember = "Name";
                                eventValueComboBox.ValueMember = "Value";
                                eventValueComboBox.DataSource = plugin.Map.InfantryTypes.Select(t => new { Name = t.DisplayName, Value = (long)t.ID }).ToArray();
                                eventValueComboBox.DataBindings.Add("SelectedValue", triggerEvent, "Data");
                                if (triggerEventData == null)
                                {
                                    eventValueComboBox.SelectedIndex = 0;
                                }
                                else
                                {
                                    eventValueComboBox.SelectedValue = triggerEventData.Data;
                                }
                                break;
                            case RedAlert.EventTypes.TEVENT_BUILD_AIRCRAFT:
                                eventValueComboBox.Visible = true;
                                eventValueComboBox.DisplayMember = "Name";
                                eventValueComboBox.ValueMember = "Value";
                                eventValueComboBox.DataSource = plugin.Map.TeamTechnoTypes.Where(t => (t is UnitType) && ((UnitType)t).IsAircraft).Select(t => new { Name = t.DisplayName, Value = (long)t.ID }).ToArray();
                                eventValueComboBox.DataBindings.Add("SelectedValue", triggerEvent, "Data");
                                if (triggerEventData == null)
                                {
                                    eventValueComboBox.SelectedIndex = 0;
                                }
                                else
                                {
                                    eventValueComboBox.SelectedValue = triggerEventData.Data;
                                }
                                break;
                            case RedAlert.EventTypes.TEVENT_NUNITS_DESTROYED:
                            case RedAlert.EventTypes.TEVENT_NBUILDINGS_DESTROYED:
                            case RedAlert.EventTypes.TEVENT_CREDITS:
                            case RedAlert.EventTypes.TEVENT_TIME:
                            case RedAlert.EventTypes.TEVENT_GLOBAL_SET:
                            case RedAlert.EventTypes.TEVENT_GLOBAL_CLEAR:
                                eventNud.Visible = true;
                                if (triggerEventData == null)
                                {
                                    eventNud.Value = 0;
                                }
                                else
                                {
                                    eventNud.Value = triggerEventData.Data;
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                }
            }
        }

        private void UpdateTriggerActionControls(TriggerAction triggerAction, NumericUpDown actionNud, ComboBox actionValueComboBox, TriggerAction triggerActionData)
        {
            actionNud.Visible = false;
            actionNud.DataBindings.Clear();
            actionNud.Minimum = long.MinValue;
            actionNud.Maximum = long.MaxValue;
            actionValueComboBox.Visible = false;
            actionValueComboBox.DataBindings.Clear();
            actionValueComboBox.DataSource = null;
            actionValueComboBox.DisplayMember = null;
            actionValueComboBox.ValueMember = null;
            if (triggerAction != null)
            {
                if (triggerActionData == null)
                {
                    triggerAction.Data = 0;
                    triggerAction.Trigger = Trigger.None;
                    triggerAction.Team = TeamType.None;
                }
                else
                {
                    triggerAction.FillDataFrom(triggerActionData);
                }
                switch (plugin.GameType)
                {
                    case GameType.RedAlert:
                        switch (triggerAction.ActionType)
                        {
                            case RedAlert.ActionTypes.TACTION_CREATE_TEAM:
                            case RedAlert.ActionTypes.TACTION_DESTROY_TEAM:
                            case RedAlert.ActionTypes.TACTION_REINFORCEMENTS:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DataSource = plugin.Map.TeamTypes.Count == 0 ? new[] { TeamType.None } : plugin.Map.TeamTypes.Select(t => t.Name).ToArray();
                                actionValueComboBox.DataBindings.Add("SelectedItem", triggerAction, "Team");
                                if (triggerActionData == null)
                                {
                                    if (actionValueComboBox.Items.Count > 0)
                                    {
                                        actionValueComboBox.SelectedIndex = 0;
                                    }
                                }
                                else
                                {
                                    actionValueComboBox.SelectedItem = triggerActionData.Team;
                                }
                                break;
                            case RedAlert.ActionTypes.TACTION_WIN:
                            case RedAlert.ActionTypes.TACTION_LOSE:
                            case RedAlert.ActionTypes.TACTION_BEGIN_PRODUCTION:
                            case RedAlert.ActionTypes.TACTION_FIRE_SALE:
                            case RedAlert.ActionTypes.TACTION_AUTOCREATE:
                            case RedAlert.ActionTypes.TACTION_ALL_HUNT:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DisplayMember = "Name";
                                actionValueComboBox.ValueMember = "Value";
                                actionValueComboBox.DataSource = new { Name = House.None, Value = (long)-1 }.Yield().Concat(
                                    plugin.Map.Houses.Select(t => new { t.Type.Name, Value = (long)t.Type.ID })).ToArray();
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Data");
                                if (triggerActionData == null)
                                {
                                    actionValueComboBox.SelectedIndex = 0;
                                }
                                else
                                {
                                    actionValueComboBox.SelectedValue = triggerActionData.Data;
                                }
                                break;
                            case RedAlert.ActionTypes.TACTION_FORCE_TRIGGER:
                            case RedAlert.ActionTypes.TACTION_DESTROY_TRIGGER:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DataSource = triggers.Select(t => t.Name).ToArray();
                                actionValueComboBox.DataBindings.Add("SelectedItem", triggerAction, "Trigger");
                                if (triggerActionData == null)
                                {
                                    actionValueComboBox.SelectedIndex = 0;
                                }
                                else
                                {
                                    actionValueComboBox.SelectedItem = triggerActionData.Trigger;
                                }
                                break;
                            case RedAlert.ActionTypes.TACTION_DZ:
                            case RedAlert.ActionTypes.TACTION_REVEAL_SOME:
                            case RedAlert.ActionTypes.TACTION_REVEAL_ZONE:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DisplayMember = "Name";
                                actionValueComboBox.ValueMember = "Value";
                                actionValueComboBox.DataSource = new { Name = Waypoint.None, Value = (long)-1 }.Yield().Concat(
                                    plugin.Map.Waypoints.Select((t, i) => new { t.Name, Value = (long)i })).ToArray();
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Data");
                                if (triggerActionData == null)
                                {
                                    actionValueComboBox.SelectedIndex = 0;
                                }
                                else
                                {
                                    actionValueComboBox.SelectedValue = triggerActionData.Data;
                                }
                                break;
                            case RedAlert.ActionTypes.TACTION_1_SPECIAL:
                            case RedAlert.ActionTypes.TACTION_FULL_SPECIAL:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DisplayMember = "Name";
                                actionValueComboBox.ValueMember = "Value";
                                var superData = new { Name = "None", Value = (long)-1 }.Yield().Concat(
                                    RedAlert.ActionDataTypes.SuperTypes.Select((t, i) => new { Name = t, Value = (long)i }))
                                    .OrderBy(t => t.Name).ToArray();
                                actionValueComboBox.DataSource = superData;
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Data");
                                if (triggerActionData == null)
                                {
                                    actionValueComboBox.SelectedIndex = 0;
                                }
                                else
                                {
                                    actionValueComboBox.SelectedValue = triggerActionData.Data;
                                }
                                /*/
                                actionValueComboBox.DataSource = Enum.GetValues(typeof(RedAlert.ActionDataTypes.SpecialWeaponType)).Cast<int>()
                                    .Select(v => new { Name = Enum.GetName(typeof(RedAlert.ActionDataTypes.SpecialWeaponType), v), Value = (long)v })
                                    .ToArray();
                                //*/
                                break;
                            case RedAlert.ActionTypes.TACTION_PLAY_MUSIC:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DisplayMember = "Name";
                                actionValueComboBox.ValueMember = "Value";
                                var musData = plugin.Map.ThemeTypes.Select((t, i) => new { Name = t, Value = (long)i - 1 }).OrderBy(t => t.Name).ToList();
                                var musDefItem = musData.Where(t => t.Value == -1).FirstOrDefault();
                                if (musDefItem != null)
                                {
                                    musData.Remove(musDefItem);
                                    musData.Insert(0, musDefItem);
                                }
                                actionValueComboBox.DataSource = musData;
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Data");
                                if (triggerActionData == null)
                                {
                                    actionValueComboBox.SelectedIndex = 0;
                                }
                                else
                                {
                                    actionValueComboBox.SelectedValue = triggerActionData.Data;
                                }
                                /*/
                                actionValueComboBox.DataSource = Enum.GetValues(typeof(RedAlert.ActionDataTypes.ThemeType)).Cast<int>()
                                    .Select(v => new { Name = Enum.GetName(typeof(RedAlert.ActionDataTypes.ThemeType), v), Value = (long)v })
                                    .ToArray();
                                //*/
                                break;
                            case RedAlert.ActionTypes.TACTION_PLAY_MOVIE:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DisplayMember = "Name";
                                actionValueComboBox.ValueMember = "Value";
                                var movData = plugin.Map.MovieTypes.Select((t, i) => new { Name = t, Value = (long)i - 1 }).OrderBy(t => t.Name, new ExplorerComparer()).ToList();
                                var movDefItem = movData.Where(t => t.Value == -1).FirstOrDefault();
                                if (movDefItem != null)
                                {
                                    movData.Remove(movDefItem);
                                    movData.Insert(0, movDefItem);
                                }
                                actionValueComboBox.DataSource = movData;
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Data");
                                if (triggerActionData == null)
                                {
                                    actionValueComboBox.SelectedIndex = 0;
                                }
                                else
                                {
                                    actionValueComboBox.SelectedValue = triggerActionData.Data;
                                }
                                /*/
                                actionValueComboBox.DataSource = Enum.GetValues(typeof(RedAlert.ActionDataTypes.VQType)).Cast<int>()
                                    .Select(v => new { Name = Enum.GetName(typeof(RedAlert.ActionDataTypes.VQType), v), Value = (long)v })
                                    .ToArray();
                                //*/
                                break;
                            case RedAlert.ActionTypes.TACTION_PLAY_SOUND:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DisplayMember = "Name";
                                actionValueComboBox.ValueMember = "Value";
                                var vocData = new { Name = "None", Value = (long)-1 }.Yield().Concat(
                                    RedAlert.ActionDataTypes.VocDesc.Select((t, i) => new { Name = t, Value = (long)i })
                                    .Where(t => !String.Equals(t.Name, "x", StringComparison.InvariantCultureIgnoreCase))).ToArray();
                                /*/
                                var vocData = new { Name = "None", Value = (long)-1 }.Yield().Concat(
                                    RedAlert.ActionDataTypes.VocNames.Select((t, i) => new { Name = t, Value = (long)i })
                                    .Where(t => !String.Equals(t.Name, "x", StringComparison.InvariantCultureIgnoreCase))
                                    .OrderBy(t => t.Name, new ExplorerComparer())).ToArray();
                                //*/
                                actionValueComboBox.DataSource = vocData;
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Data");
                                if (triggerActionData == null)
                                {
                                    actionValueComboBox.SelectedIndex = 0;
                                }
                                else
                                {
                                    actionValueComboBox.SelectedValue = triggerActionData.Data;
                                }
                                /*/
                                actionValueComboBox.DataSource = Enum.GetValues(typeof(RedAlert.ActionDataTypes.VocType)).Cast<int>()
                                    .Select(v => new { Name = Enum.GetName(typeof(RedAlert.ActionDataTypes.VocType), v), Value = (long)v })
                                    .ToArray();
                                //*/
                                break;
                            case RedAlert.ActionTypes.TACTION_PLAY_SPEECH:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DisplayMember = "Name";
                                actionValueComboBox.ValueMember = "Value";
                                var voxData = new { Name = "None", Value = (long)-1 }.Yield().Concat(
                                    RedAlert.ActionDataTypes.VoxDesc.Select((t, i) => new { Name = t, Value = (long)i })
                                    .Where(t => !String.Equals(t.Name, "none", StringComparison.InvariantCultureIgnoreCase))).ToArray();
                                /*/
                                var voxData = new { Name = "None", Value = (long)-1 }.Yield().Concat(
                                    RedAlert.ActionDataTypes.VocNames.Select((t, i) => new { Name = t, Value = (long)i })
                                    .Where(t => !String.Equals(t.Name, "none", StringComparison.InvariantCultureIgnoreCase))
                                    .OrderBy(t => t.Name, new ExplorerComparer())).ToArray();
                                //*/
                                actionValueComboBox.DataSource = voxData;
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Data");
                                if (triggerActionData == null)
                                {
                                    actionValueComboBox.SelectedIndex = 0;
                                }
                                else
                                {
                                    actionValueComboBox.SelectedValue = triggerActionData.Data;
                                }
                                /*/
                                actionValueComboBox.DataSource = Enum.GetValues(typeof(RedAlert.ActionDataTypes.VoxType)).Cast<int>()
                                    .Select(v => new { Name = Enum.GetName(typeof(RedAlert.ActionDataTypes.VoxType), v), Value = (long)v })
                                    .ToArray();
                                //*/
                                break;
                            case RedAlert.ActionTypes.TACTION_PREFERRED_TARGET:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DisplayMember = "Name";
                                actionValueComboBox.ValueMember = "Value";
                                var quarryData = RedAlert.TeamMissionTypes.Attack.DropdownOptions.Select(t => new { Name = t.Item2, Value = (long)t.Item1 }).ToArray();
                                actionValueComboBox.DataSource = quarryData;
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Data");
                                if (triggerActionData == null)
                                {
                                    actionValueComboBox.SelectedIndex = 0;
                                }
                                else
                                {
                                    actionValueComboBox.SelectedValue = triggerActionData.Data;
                                }
                                /*/
                                actionValueComboBox.DataSource = Enum.GetValues(typeof(RedAlert.ActionDataTypes.QuarryType)).Cast<int>()
                                    .Select(v => new { Name = Enum.GetName(typeof(RedAlert.ActionDataTypes.QuarryType), v), Value = (long)v })
                                    .ToArray();
                                //*/
                                break;
                            case RedAlert.ActionTypes.TACTION_BASE_BUILDING:
                                actionValueComboBox.Visible = true;
                                actionValueComboBox.DisplayMember = "Name";
                                actionValueComboBox.ValueMember = "Value";
                                var trueFalseData = new long[] { 0, 1 }.Select(b => new { Name = b == 0 ? "On" : "Off", Value = b }).ToArray();
                                actionValueComboBox.DataSource = trueFalseData;
                                actionValueComboBox.DataBindings.Add("SelectedValue", triggerAction, "Data");
                                if (triggerActionData == null)
                                {
                                    actionValueComboBox.SelectedIndex = 0;
                                }
                                else
                                {
                                    if (triggerActionData.Data > 1 || triggerActionData.Data < 0)
                                        triggerActionData.Data = 0;
                                    actionValueComboBox.SelectedValue = triggerActionData.Data;
                                }
                                break;
                            case RedAlert.ActionTypes.TACTION_TEXT_TRIGGER:
                                actionNud.Visible = true;
                                actionNud.Minimum = 1;
                                actionNud.Maximum = 209;
                                triggerAction.Data = 1;
                                if (triggerActionData == null)
                                {
                                    actionNud.Value = 1;
                                }
                                else
                                {
                                    if (triggerActionData.Data > 209 || triggerActionData.Data < 1)
                                        triggerActionData.Data = 1;
                                    actionNud.Value = triggerActionData.Data;
                                }
                                break;
                            case RedAlert.ActionTypes.TACTION_ADD_TIMER:
                            case RedAlert.ActionTypes.TACTION_SUB_TIMER:
                            case RedAlert.ActionTypes.TACTION_SET_TIMER:
                            case RedAlert.ActionTypes.TACTION_SET_GLOBAL:
                            case RedAlert.ActionTypes.TACTION_CLEAR_GLOBAL:
                                actionNud.Visible = true;
                                if (triggerActionData == null)
                                {
                                    actionNud.Value = 0;
                                }
                                else
                                {
                                    actionNud.Value = triggerActionData.Data;
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                }
            }
        }

        private void btnCheck_Click(Object sender, EventArgs e)
        {
            if (Trigger.CheckForChanges(triggers, backupTriggers))
            {
                DialogResult dr = MessageBox.Show("Warning! There are changes in the triggers. This function works best if the triggers match the state of the currently edited map. Are you sure you want to continue?", "Triggers check", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.No)
                {
                    return;
                }
            }
            string errors = String.Empty;
            if (plugin.GameType == GameType.TiberianDawn)
            {
                errors = CheckTiberianDawnTriggers();
            }
            else if (plugin.GameType == GameType.RedAlert)
            {
                errors = CheckRedAlertTriggers();
            }
            if (String.IsNullOrEmpty(errors))
            {
                errors = "No issues were encountered.";
            }
            MessageBox.Show(errors, "Triggers check", MessageBoxButtons.OK);
        }

        private string CheckTiberianDawnTriggers()
        {
            // Might need to migrate this to the plugin.
            HouseType player = plugin.Map.HouseTypes.Where(t => t.Equals(plugin.Map.BasicSection.Player)).FirstOrDefault() ?? plugin.Map.HouseTypes.First();
            
            List<string> errors = new List<string>();
            List<string> curErrors = new List<string>();
            List<ITechno> mapTechnos = plugin.Map.GetAllTechnos().ToList();
            foreach (Trigger trigger in this.triggers)
            {
                curErrors.Clear();
                string trigName = trigger.Name;
                string event1 = trigger.Event1.EventType;
                string action1 = trigger.Action1.ActionType;
                bool noOwner = House.None.EqualsOrDefaultIgnoreCase(trigger.House, House.None);
                bool isPlayer = !noOwner && player.Equals(trigger.House);
                //bool playerIsNonstandard = !player.Equals(TiberianDawn.HouseTypes.Good) && !player.Equals(TiberianDawn.HouseTypes.Bad);
                //bool isGoodguy = String.Equals(trigger.House, TiberianDawn.HouseTypes.Good.Name, StringComparison.InvariantCultureIgnoreCase);
                //bool isBadguy = String.Equals(trigger.House, TiberianDawn.HouseTypes.Bad.Name, StringComparison.InvariantCultureIgnoreCase);
                bool isLinked = mapTechnos.Any(tech => String.Equals(trigName, tech.Trigger, StringComparison.InvariantCultureIgnoreCase));
                //bool isLinkedToStructs = mapTechnos.Any(tech => tech is Building && String.Equals(trigName, tech.Trigger, StringComparison.InvariantCultureIgnoreCase));
                //bool isLinkedToUnits = mapTechnos.Any(tech => (tech is Unit || tech is Infantry) && String.Equals(trigName, tech.Trigger, StringComparison.InvariantCultureIgnoreCase));
                //bool isLinkedToTrees = mapTechnos.Any(tech => (tech is Terrain) && String.Equals(trigName, tech.Trigger, StringComparison.InvariantCultureIgnoreCase));
                bool isCellTrig = plugin.Map.CellTriggers.Any(c => c.Value.Trigger == trigName);
                bool hasTeam = !String.IsNullOrEmpty(trigger.Action1.Team) && trigger.Action1.Team != TeamType.None;
                bool isAnd = trigger.PersistentType == TriggerPersistentType.SemiPersistent;
                //bool isOr = trigger.PersistentType == TriggerPersistentType.Persistent;
                
                // Event checks
                if (event1 == TiberianDawn.EventTypes.EVENT_PLAYER_ENTERED && noOwner)
                {
                    curErrors.Add("A \"Player Enters\" trigger without a House set will cause a game crash. If this is only used for detecting capturing, the House will not be checked, but it must still be set.");
                }
                if (event1 == TiberianDawn.EventTypes.EVENT_ATTACKED && !noOwner)
                {
                    if (isLinked)
                    {
                        curErrors.Add("\"Attacked\" triggers with a House set will trigger when that House is attacked. To make a trigger for checking if objects are attacked, leave the House empty.");
                    }
                    else if (!isPlayer)
                    {
                        curErrors.Add("\"Attacked\" triggers with a House set will trigger when that House is attacked. However, this logic only works for the player's House.");
                    }
                }
                if (event1 == TiberianDawn.EventTypes.EVENT_ANY && action1 != TiberianDawn.ActionTypes.ACTION_WINLOSE)
                {
                    curErrors.Add("The \"Any\" event will trigger on literally anything that can happen to a linked object. It should normally only be used with the \"Cap=Win/Des=Lose\" action.");
                }
                if (event1 == TiberianDawn.EventTypes.EVENT_NBUILDINGS_DESTROYED && trigger.Event1.Data == 0)
                {
                    curErrors.Add("The amount of buildings that needs to be destroyed is 0.");
                }
                if (event1 == TiberianDawn.EventTypes.EVENT_NUNITS_DESTROYED && trigger.Event1.Data == 0)
                {
                    curErrors.Add("The amount of units that needs to be destroyed is 0.");
                }
                // Action checks
                if (action1 == TiberianDawn.ActionTypes.ACTION_AIRSTRIKE && event1 == TiberianDawn.EventTypes.EVENT_PLAYER_ENTERED && !isPlayer && isCellTrig)
                {
                    curErrors.Add("This will give the Airstrike to the House that activates the Celltrigger. This will grant the AI house linked to it periodic airstrikes that will only target structure.");
                }
                if (action1 == TiberianDawn.ActionTypes.ACTION_WINLOSE && event1 == TiberianDawn.EventTypes.EVENT_ANY && isAnd)
                {
                    curErrors.Add("\"Any\" → \"Cap=Win/Des=Lose\" triggers don't function with existence status \"And\".");
                }
                if (action1 == TiberianDawn.ActionTypes.ACTION_ALLOWWIN && !isPlayer)
                {
                    curErrors.Add("Each \"Allow Win\" trigger increases the \"win blockage\" on a House, which prevents it from winning until they are all cleared. However, since this is put on the House specified in the trigger, \"Allow Win\" triggers nly work with the player's House.");
                }
                if (action1 == TiberianDawn.ActionTypes.ACTION_BEGIN_PRODUCTION)
                {
                    if (trigger.Event1.EventType != TiberianDawn.EventTypes.EVENT_PLAYER_ENTERED && noOwner)
                    {
                        curErrors.Add("The House set in a \"Production\" trigger determines the House that starts production, except in case of a celltrigger. Having no House will crash the game.");
                    }
                    //else if (trigger.Event1.EventType == TiberianDawn.EventTypes.EVENT_PLAYER_ENTERED && isCellTrig && playerIsNonstandard)
                    //{
                    //    curErrors.Add("For a celltrigger, the House that starts production is always be the 'classic opposing House' of the player's House.");
                    //}
                }
                if (action1 == TiberianDawn.ActionTypes.ACTION_REINFORCEMENTS && !hasTeam)
                {
                    curErrors.Add("There is no team set to reinforce.");
                }
                if (action1 == TiberianDawn.ActionTypes.ACTION_DESTROY_TEAM && !hasTeam)
                {
                    curErrors.Add("There is no team set to disband.");
                }
                if (action1 == TiberianDawn.ActionTypes.ACTION_DESTROY_XXXX && !this.triggers.Any(t => "XXXX".Equals(t.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    curErrors.Add("There is no trigger called 'XXXX' to destroy.");
                }
                if (action1 == TiberianDawn.ActionTypes.ACTION_DESTROY_YYYY && !this.triggers.Any(t => "YYYY".Equals(t.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    curErrors.Add("There is no trigger called 'YYYY' to destroy.");
                }
                if (action1 == TiberianDawn.ActionTypes.ACTION_DESTROY_ZZZZ && !this.triggers.Any(t => "ZZZZ".Equals(t.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    curErrors.Add("There is no trigger called 'ZZZZ' to destroy.");
                }
                if (curErrors.Count > 0)
                {
                    curErrors.Insert(0, trigName.ToUpper() + ":");
                    errors.AddRange(curErrors);
                    errors.Add("\n");
                }
            }
            if (errors.Count == 0)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder("The following issues were encountered:\n\n");
            foreach (string item in errors)
            {
                sb.Append(item).Append('\n');
            }
            sb.TrimEnd('\n');
            return sb.ToString();
        }

        private string CheckRedAlertTriggers()
        {
            return null;
            // TODO
        }

    }
}
