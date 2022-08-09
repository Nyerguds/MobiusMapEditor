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
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class TriggersDialog : Form
    {
        private readonly IGamePlugin plugin;
        private readonly int maxTriggers;

        private readonly List<Trigger> backupTriggers;
        private readonly List<Trigger> triggers;
        public IEnumerable<Trigger> Triggers => triggers;

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
                    break;
                case GameType.RedAlert:
                    teamLabel.Visible = teamComboBox.Visible = false;
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

            houseComboBox.DataSource = "None".Yield().Concat(plugin.Map.Houses.Select(t => t.Type.Name)).ToArray();
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
            teamComboBox.DataSource = "None".Yield().Concat(plugin.Map.TeamTypes.Select(t => t.Name)).ToArray();

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
                TriggerEvent evt1 = SelectedTrigger.Event1.Clone();
                event1ComboBox.SelectedItem = SelectedTrigger.Event1?.EventType;
                UpdateTriggerEventControls(SelectedTrigger.Event1, event1Nud, event1ValueComboBox, evt1);
                TriggerAction act1 = SelectedTrigger.Action1.Clone();
                action1ComboBox.SelectedItem = SelectedTrigger.Action1?.ActionType;
                UpdateTriggerActionControls(SelectedTrigger.Action1, action1Nud, action1ValueComboBox, act1);

                switch (plugin.GameType)
                {
                    case GameType.TiberianDawn:
                        teamComboBox.DataBindings.Add("SelectedItem", SelectedTrigger.Action1, "Team");
                        break;
                    case GameType.RedAlert:
                        typeComboBox.DataBindings.Add("SelectedValue", SelectedTrigger, "EventControl");
                        TriggerEvent evt2 = SelectedTrigger.Event2.Clone();
                        event2ComboBox.SelectedItem = SelectedTrigger.Event2.EventType;
                        UpdateTriggerEventControls(SelectedTrigger.Event2, event2Nud, event2ValueComboBox, evt2);
                        event2ComboBox.SelectedItem = SelectedTrigger.Event2?.EventType;
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
        private void Event1Nud_ValueEntered(object sender, ValueEnteredEventArgs e)
        {
            Trigger trig = SelectedTrigger;
            if (trig != null && trig.Event1 != null)
            {
                trig.Event1.Data = (long)event1Nud.EnteredValue;
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

        private void Event2Nud_ValueEntered(object sender, ValueEnteredEventArgs e)
        {
            Trigger trig = SelectedTrigger;
            if (trig != null && trig.Event2 != null)
            {
                trig.Event2.Data = (long)event2Nud.EnteredValue;
            }
        }

        private void Action1Nud_ValueEntered(object sender, ValueEnteredEventArgs e)
        {
            Trigger trig = SelectedTrigger;
            if (trig != null && trig.Action1 != null)
            {
                trig.Action1.Data = (long)action1Nud.EnteredValue;
            }
        }

        private void Action2Nud_ValueEntered(object sender, ValueEnteredEventArgs e)
        {
            Trigger trig = SelectedTrigger;
            if (trig != null && trig.Action2 != null)
            {
                trig.Action2.Data = (long)action2Nud.EnteredValue;
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
            bool hasChanges = triggers.Count != backupTriggers.Count;
            if (!hasChanges)
            {
                foreach (Trigger trig in triggers)
                {
                    Trigger oldTrig = backupTriggers.Find(t => t.Name.Equals(trig.Name));
                    if (oldTrig == null)
                    {
                        hasChanges = true;
                        break;
                    }
                    hasChanges = !trig.EqualsOther(oldTrig);
                    if (hasChanges)
                        break;
                }
            }
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
            string name = GeneralUtils.MakeNew4CharName(triggers.Select(t => t.Name), "????", "none");
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
            else if ("None".Equals(curName, StringComparison.InvariantCultureIgnoreCase))
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
                                eventValueComboBox.DataSource = plugin.Map.TeamTypes.Count == 0 ? new[] { "None" } : plugin.Map.TeamTypes.Select(t => t.Name).ToArray();
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
                                eventValueComboBox.DataSource = new { Name = "None", Value = (long)-1 }.Yield().Concat(plugin.Map.Houses.Select(t => new { t.Type.Name, Value = (long)t.Type.ID })).ToArray();
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
                                eventValueComboBox.DataSource = plugin.Map.UnitTypes.Where(t => t.IsAircraft).Select(t => new { Name = t.DisplayName, Value = (long)t.ID }).ToArray();
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
                    triggerAction.Trigger = null;
                    triggerAction.Team = null;  
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
                                actionValueComboBox.DataSource = plugin.Map.TeamTypes.Count == 0 ? new[] { "None" } : plugin.Map.TeamTypes.Select(t => t.Name).ToArray();
                                actionValueComboBox.DataBindings.Add("SelectedItem", triggerAction, "Team");
                                if (triggerActionData == null)
                                {
                                    if (actionValueComboBox.Items.Count > 0)
                                        actionValueComboBox.SelectedIndex = 0;
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
                                actionValueComboBox.DataSource = new { Name = "None", Value = (long)-1 }.Yield().Concat(
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
                                actionValueComboBox.DataSource = new { Name = "None", Value = (long)-1 }.Yield().Concat(
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
                                    RedAlert.ActionDataTypes.VocTypes.Select((t, i) => new { Name = t, Value = (long)i })
                                    .Where(t => t.Name != "x").OrderBy(t => t.Name, new ExplorerComparer())).ToArray();
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
                                    RedAlert.ActionDataTypes.VoxTypes.Select((t, i) => new { Name = t, Value = (long)i })
                                    .Where(t => t.Name != "none").OrderBy(t => t.Name, new ExplorerComparer())).ToArray();
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
                                var trueFalseData = new long[] { 0, 1 }.Select(b => new { Name = b == 0 ? "Stop" : "Start", Value = b }).ToArray();
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
                                actionNud.DataBindings.Add("Value", triggerAction, "Data");
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
                                actionNud.DataBindings.Add("Value", triggerAction, "Data");
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
    }
}
