//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                     Version 2, December 2004
//
//  Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
//  Everyone is permitted to copy and distribute verbatim or modified
//  copies of this license document, and changing it is allowed as long
//  as the name is changed.
//
//             DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//    TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//   0. You just DO WHAT THE FUCK YOU WANT TO.
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class TriggerFilterDialog : Form
    {
        private string[] persistenceNames;
        private string[] eventControlNames;
        private string[] currentTrigs;
        private IGamePlugin plugin;
        private bool isRA;
        private TriggerFilter filter;
        public TriggerFilter Filter
        {
            get
            {
                return filter;
            }
            set
            {
                filter = value;
                SetFilterInfo(filter);
            }
        }

        public TriggerFilterDialog(IGamePlugin plugin, string persistenceLabel, string[] persistenceNames, string[] eventControlNames, string[] currentTrigs)
        {
            if (Enum.GetValues(typeof(TriggerPersistentType)).Length != persistenceNames.Length)
            {
                throw new ArgumentException("Incorrect amount of persistence names!", "persistenceNames");
            }
            if (Enum.GetValues(typeof(TriggerMultiStyleType)).Length != eventControlNames.Length)
            {
                throw new ArgumentException("Incorrect amount of event control names!", "eventControlNames");
            }
            this.persistenceNames = persistenceNames;
            this.eventControlNames = eventControlNames;
            this.currentTrigs = (currentTrigs ?? new string[0]).Where(st => !String.IsNullOrWhiteSpace(st)).ToArray();
            this.plugin = plugin;
            this.isRA = this.plugin.GameInfo.GameType == GameType.RedAlert;
            InitializeComponent();
            this.chkPersistenceType.Text = persistenceLabel;
        }

        private void SetFilterInfo(TriggerFilter filter)
        {
            ResetOptions();
            chkHouse.Checked = filter.FilterHouse;
            if (chkHouse.Checked)
            {
                cmbHouse.SelectedIndex = ListItem.GetIndexInComboBox(filter.House, cmbHouse);
            }
            chkPersistenceType.Checked = filter.FilterPersistenceType;
            if (chkPersistenceType.Checked)
            {
                cmbPersistenceType.SelectedIndex = ListItem.GetIndexInComboBox(filter.PersistenceType, cmbPersistenceType);
            }
            if (isRA)
            {
                chkEventControl.Checked = filter.FilterEventControl;
                if (chkEventControl.Checked)
                {
                    cmbEventControl.SelectedIndex = ListItem.GetIndexInComboBox(filter.EventControl, cmbEventControl);
                }
            }
            chkEventType.Checked = filter.FilterEventType;
            if (chkEventType.Checked)
            {
                cmbEventType.SelectedIndex = ListItem.GetIndexInComboBox(filter.EventType, cmbEventType);
            }
            chkActionType.Checked = filter.FilterActionType;
            if (chkActionType.Checked)
            {
                cmbActionType.SelectedIndex = ListItem.GetIndexInComboBox(filter.ActionType, cmbActionType);
            }
            chkTeamType.Checked = filter.FilterTeamType;
            if (chkTeamType.Checked)
            {
                cmbTeamType.SelectedIndex = ListItem.GetIndexInComboBox(filter.TeamType, cmbTeamType);
            }
            string correctCaseName = currentTrigs.FirstOrDefault(tr => String.Equals(tr, filter.Trigger, StringComparison.Ordinal));
            chkTrigger.Checked = filter.FilterTrigger && correctCaseName != null;
            if (chkTrigger.Checked)
            {
                cmbTrigger.SelectedIndex = ListItem.GetIndexInComboBox(correctCaseName, cmbTrigger);
            }
            if (isRA)
            {
                chkWaypoint.Checked = filter.FilterWaypoint;
                if (chkWaypoint.Checked)
                {
                    cmbWaypoint.SelectedIndex = (filter.Waypoint + 1).Restrict(0, plugin.Map.Waypoints.Length);
                }
                chkGlobal.Checked = filter.FilterGlobal;
                if (chkGlobal.Checked)
                {
                    nudGlobal.Value = filter.Global.Restrict(0, 29);
                }
            }
        }
        private void ResetOptions()
        {
            chkHouse.Checked = false;
            chkPersistenceType.Checked = false;
            chkEventControl.Checked = false;
            chkEventType.Checked = false;
            chkActionType.Checked = false;
            chkTeamType.Checked = false;
            chkTrigger.Checked = false;
            chkWaypoint.Checked = false;
            chkGlobal.Checked = false;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ResetOptions();
        }

        private void ChkHouse_CheckedChanged(object sender, EventArgs e)
        {
            cmbHouse.DataSource = null;
            cmbHouse.ValueMember = null;
            cmbHouse.DisplayMember = null;
            cmbHouse.Enabled = chkHouse.Checked;
            if (chkHouse.Checked)
            {
                cmbHouse.ValueMember = "Value";
                cmbHouse.DisplayMember = "Label";
                cmbHouse.DataSource = ListItem.Create(House.None).Yield().Concat(
                                        this.plugin.Map.Houses.Select(t => ListItem.Create(t.Type.Name))).ToArray();
            }
        }

        private void ChkPersistenceType_CheckedChanged(object sender, EventArgs e)
        {
            cmbPersistenceType.DataSource = null;
            cmbPersistenceType.ValueMember = null;
            cmbPersistenceType.DisplayMember = null;
            cmbPersistenceType.Enabled = chkPersistenceType.Checked;
            if (chkPersistenceType.Checked)
            {
                cmbPersistenceType.ValueMember = "Value";
                cmbPersistenceType.DisplayMember = "Label";
                cmbPersistenceType.DataSource = Enum.GetValues(typeof(TriggerPersistentType)).Cast<TriggerPersistentType>()
                    .Select(v => ListItem.Create(v, persistenceNames[(int)v])).ToArray();
            }
        }

        private void ChkEventControl_CheckedChanged(object sender, EventArgs e)
        {
            cmbEventControl.DataSource = null;
            cmbEventControl.ValueMember = null;
            cmbEventControl.DisplayMember = null;
            cmbEventControl.Enabled = isRA && chkEventControl.Checked;
            if (isRA && chkEventControl.Checked)
            {
                cmbEventControl.ValueMember = "Value";
                cmbEventControl.DisplayMember = "Label";
                cmbEventControl.DataSource = Enum.GetValues(typeof(TriggerMultiStyleType)).Cast<TriggerMultiStyleType>()
                    .Select(v => ListItem.Create(v, eventControlNames[(int)v])).ToArray();
            }
        }

        private void ChkEventType_CheckedChanged(object sender, EventArgs e)
        {
            cmbEventType.DataSource = null;
            cmbEventType.ValueMember = null;
            cmbEventType.DisplayMember = null;
            cmbEventType.Enabled = chkEventType.Checked;
            if (chkEventType.Checked)
            {
                cmbEventType.ValueMember= "Value";
                cmbEventType.DisplayMember = "Label";
                cmbEventType.DataSource = plugin.Map.EventTypes.Where(t => !string.IsNullOrEmpty(t)).Select(et => ListItem.Create(et)).ToArray();
            }
        }

        private void ChkActionType_CheckedChanged(object sender, EventArgs e)
        {
            cmbActionType.DataSource = null;
            cmbActionType.ValueMember = null;
            cmbActionType.DisplayMember = null;
            cmbActionType.Enabled = chkActionType.Checked;
            if (chkActionType.Checked)
            {
                cmbActionType.ValueMember= "Value";
                cmbActionType.DisplayMember = "Label";
                cmbActionType.DataSource = plugin.Map.ActionTypes.Where(t => !string.IsNullOrEmpty(t)).Select(at => ListItem.Create(at)).ToArray();
            }
        }

        private void ChkTeamType_CheckedChanged(object sender, EventArgs e)
        {
            cmbTeamType.DataSource = null;
            cmbTeamType.ValueMember = null;
            cmbTeamType.DisplayMember = null;
            cmbTeamType.Enabled = chkTeamType.Checked;
            if (chkTeamType.Checked)
            {
                cmbTeamType.ValueMember= "Value";
                cmbTeamType.DisplayMember = "Label";
                cmbTeamType.DataSource = ListItem.Create(TeamType.None).Yield().Concat(plugin.Map.TeamTypes.Select(t => ListItem.Create(t.Name))).ToArray();
            }
        }

        private void ChkTrigger_CheckedChanged(object sender, EventArgs e)
        {
            cmbTrigger.DataSource = null;
            cmbTrigger.ValueMember = null;
            cmbTrigger.DisplayMember = null;
            cmbTrigger.Enabled = chkTrigger.Checked;
            if (chkTrigger.Checked)
            {
                cmbTrigger.ValueMember= "Value";
                cmbTrigger.DisplayMember = "Label";
                cmbTrigger.DataSource = ListItem.Create(Trigger.None).Yield().Concat(this.currentTrigs.Select(tr => ListItem.Create(tr))).ToArray();
            }
        }

        private void ChkWaypoint_CheckedChanged(object sender, EventArgs e)
        {
            cmbWaypoint.DataSource = null;
            cmbWaypoint.ValueMember = null;
            cmbWaypoint.DisplayMember = null;
            if (isRA)
            {
                cmbWaypoint.Enabled = chkWaypoint.Checked;
                if (chkWaypoint.Checked)
                {
                    Waypoint[] wp = plugin.Map.Waypoints;
                    cmbWaypoint.ValueMember = "Value";
                    cmbWaypoint.DisplayMember = "Label";
                    cmbWaypoint.DataSource = ListItem.Create(-1, Waypoint.None).Yield().Concat(wp.Select((w, i) => ListItem.Create(i, w.ToString()))).ToArray();
                }
            }
        }

        private void ChkGlobal_CheckedChanged(object sender, EventArgs e)
        {
            if (isRA)
            {
                nudGlobal.Enabled = chkGlobal.Checked;
                nudGlobal.Value = chkGlobal.Checked ? filter.Global : 0;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            filter = new TriggerFilter(plugin);
            filter.FilterHouse = chkHouse.Checked;
            filter.House = filter.FilterHouse ? (string)cmbHouse.SelectedValue: null;
            filter.FilterPersistenceType = chkPersistenceType.Checked;
            filter.PersistenceType = filter.FilterPersistenceType ? (TriggerPersistentType)cmbPersistenceType.SelectedValue : TriggerPersistentType.Volatile;
            filter.FilterEventControl = chkEventControl.Checked;
            filter.EventControl = filter.FilterEventControl ? (TriggerMultiStyleType)cmbEventControl.SelectedValue : TriggerMultiStyleType.Only;
            filter.FilterEventType = chkEventType.Checked;
            filter.EventType = filter.FilterEventType ? (string)cmbEventType.SelectedValue : TriggerEvent.None;
            filter.FilterActionType = chkActionType.Checked;
            filter.ActionType = filter.FilterActionType ? (string)cmbActionType.SelectedValue : TriggerAction.None;
            filter.FilterTeamType = chkTeamType.Checked;
            filter.TeamType = filter.FilterTeamType ? (string)cmbTeamType.SelectedValue : TeamType.None;
            filter.FilterWaypoint = chkWaypoint.Checked;
            filter.Waypoint = filter.FilterWaypoint ? (int)cmbWaypoint.SelectedValue : -1;
            filter.FilterGlobal = chkGlobal.Checked;
            filter.Global = filter.FilterGlobal ? nudGlobal.IntValue : 0;
            filter.FilterTrigger= chkTrigger.Checked;
            filter.Trigger = filter.FilterTrigger ? (string)cmbTrigger.SelectedValue : Trigger.None;
        }

        private void TriggerFilterDialog_Load(object sender, EventArgs e)
        {
            int origTableHeight = triggersTableLayoutPanel.Height;
            chkEventControl.Visible = this.isRA;
            cmbEventControl.Visible = this.isRA;
            chkWaypoint.Visible = this.isRA;
            cmbWaypoint.Visible = this.isRA;
            chkGlobal.Visible = this.isRA;
            nudGlobal.Visible = this.isRA;
            int newTableHeight = triggersTableLayoutPanel.PreferredSize.Height;
            int diff = origTableHeight - newTableHeight;
            this.Size = new Size(this.Width, this.Height - diff);
            if (this.StartPosition == FormStartPosition.CenterParent || this.StartPosition == FormStartPosition.CenterScreen)
            {
                this.Location = new Point(this.Location.X, this.Location.Y + diff / 2);
            }
        }
    }
}
