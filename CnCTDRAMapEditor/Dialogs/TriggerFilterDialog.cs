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

        public TriggerFilterDialog(IGamePlugin plugin, String persistenceLabel, string[] persistenceNames, string[] eventControlNames)
        {
            this.persistenceNames = persistenceNames;
            this.eventControlNames = eventControlNames;
            this.plugin = plugin;
            this.isRA = this.plugin.GameType == GameType.RedAlert;
            InitializeComponent();
            this.chkPersistenceType.Text = persistenceLabel;
        }

        private void SetFilterInfo(TriggerFilter filter)
        {
            ResetOptions();
            chkHouse.Checked = filter.FilterHouse;
            if (chkHouse.Checked)
            {
                cmbHouse.SelectedIndex = ListItem.GetIndexInDropdownByLabel<long>(filter.House, cmbHouse);
            }
            chkPersistenceType.Checked = filter.FilterPersistenceType;
            if (chkPersistenceType.Checked)
            {
                cmbPersistenceType.SelectedIndex = ListItem.GetIndexInDropdown(filter.PersistenceType, cmbPersistenceType);
            }
            if (isRA)
            {
                chkEventControl.Checked = filter.FilterEventControl;
                if (chkEventControl.Checked)
                {
                    cmbEventControl.SelectedIndex = ListItem.GetIndexInDropdown(filter.EventControl, cmbEventControl);
                }
            }
            chkEventType.Checked = filter.FilterEventType;
            if (chkEventType.Checked)
            {
                cmbEventType.SelectedItem = filter.EventType;
            }
            chkActionType.Checked = filter.FilterActionType;
            if (chkActionType.Checked)
            {
                cmbActionType.SelectedItem = filter.ActionType;
            }
            chkTeamType.Checked = filter.FilterTeamType;
            if (chkTeamType.Checked)
            {
                cmbTeamType.SelectedItem = filter.TeamType;
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
            chkWaypoint.Checked = false;
            chkGlobal.Checked = false;
        }

        private void btnReset_Click(Object sender, EventArgs e)
        {
            ResetOptions();
        }

        private void ChkHouse_CheckedChanged(Object sender, EventArgs e)
        {
            cmbHouse.DataSource = null;
            cmbHouse.Enabled = chkHouse.Checked;
            if (chkHouse.Checked)
            {
                cmbHouse.DataSource = new ListItem<long>(-1, House.None).Yield().Concat(
                                        this.plugin.Map.Houses.Select(t => new ListItem<long>(t.Type.ID, t.Type.Name))).ToArray();
            }
        }

        private void ChkPersistenceType_CheckedChanged(Object sender, EventArgs e)
        {
            cmbPersistenceType.DataSource = null;
            cmbPersistenceType.Enabled = chkPersistenceType.Checked;
            if (chkPersistenceType.Checked)
            {
                cmbPersistenceType.DataSource = Enum.GetValues(typeof(TriggerPersistentType)).Cast<TriggerPersistentType>()
                    .Select(v => new ListItem<TriggerPersistentType>(v, persistenceNames[(int)v])).ToArray();
            }
        }

        private void ChkEventControl_CheckedChanged(Object sender, EventArgs e)
        {
            cmbEventControl.DataSource = null;
            cmbEventControl.Enabled = isRA && chkEventControl.Checked;
            if (isRA && chkEventControl.Checked)
            {
                cmbEventControl.DataSource = Enum.GetValues(typeof(TriggerMultiStyleType)).Cast<TriggerMultiStyleType>()
                    .Select(v => new ListItem<TriggerMultiStyleType>(v, eventControlNames[(int)v])).ToArray();
            }
        }

        private void ChkEventType_CheckedChanged(Object sender, EventArgs e)
        {
            cmbEventType.DataSource = null;
            cmbEventType.Enabled = chkEventType.Checked;
            if (chkEventType.Checked)
            {
                cmbEventType.DataSource = plugin.Map.EventTypes.Where(t => !string.IsNullOrEmpty(t)).ToArray();
            }
        }

        private void ChkActionType_CheckedChanged(Object sender, EventArgs e)
        {
            cmbActionType.DataSource = null;
            cmbActionType.Enabled = chkActionType.Checked;
            if (chkActionType.Checked)
            {
                cmbActionType.DataSource = plugin.Map.ActionTypes.Where(t => !string.IsNullOrEmpty(t)).ToArray();
            }
        }

        private void ChkTeamType_CheckedChanged(Object sender, EventArgs e)
        {
            cmbTeamType.DataSource = null;
            cmbTeamType.Enabled = chkTeamType.Checked;
            if (chkTeamType.Checked)
            {
                cmbTeamType.DataSource = TeamType.None.Yield().Concat(plugin.Map.TeamTypes.Select(t => t.Name)).ToArray();
            }
        }

        private void ChkWaypoint_CheckedChanged(Object sender, EventArgs e)
        {
            cmbWaypoint.DataSource = null;
            if (isRA)
            {
                cmbWaypoint.Enabled = chkWaypoint.Checked;
                if (chkWaypoint.Checked)
                {
                    cmbWaypoint.DataSource = Waypoint.None.Yield().Concat(plugin.Map.Waypoints.Select(w => w.ToString())).ToArray();
                }
            }
        }

        private void ChkGlobal_CheckedChanged(Object sender, EventArgs e)
        {
            if (isRA)
            {
                nudGlobal.Enabled = chkGlobal.Checked;
                nudGlobal.Value = chkGlobal.Checked ? filter.Global : 0;
            }
        }

        private void btnOk_Click(Object sender, EventArgs e)
        {
            filter = new TriggerFilter(plugin);
            filter.FilterHouse = chkHouse.Checked;
            filter.House = filter.FilterHouse ? (cmbHouse.SelectedItem as ListItem<long>).Label : null;
            filter.FilterPersistenceType = chkPersistenceType.Checked;
            filter.PersistenceType = filter.FilterPersistenceType ? (TriggerPersistentType)cmbPersistenceType.SelectedValue : TriggerPersistentType.Volatile;
            filter.FilterEventControl = chkEventControl.Checked;
            filter.EventControl = filter.FilterEventControl ? (TriggerMultiStyleType)cmbEventControl.SelectedValue : TriggerMultiStyleType.Only;
            filter.FilterEventType = chkEventType.Checked;
            filter.EventType = filter.FilterEventType ? (String)cmbEventType.SelectedItem : TriggerEvent.None;
            filter.FilterActionType = chkActionType.Checked;
            filter.ActionType = filter.FilterActionType ? (String)cmbActionType.SelectedItem : TriggerAction.None;
            filter.FilterTeamType = chkTeamType.Checked;
            filter.TeamType = filter.FilterTeamType ? (String)cmbTeamType.SelectedItem : TeamType.None;
            filter.FilterWaypoint = chkWaypoint.Checked;
            filter.Waypoint = filter.FilterWaypoint ? cmbWaypoint.SelectedIndex - 1 : -1;
            filter.FilterGlobal = chkGlobal.Checked;
            filter.Global = filter.FilterGlobal ? nudGlobal.IntValue : 0;
        }

        private void TriggerFilterDialog_Load(Object sender, EventArgs e)
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
