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
using MobiusEditor.Controls.ControlsList;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public partial class MissionItemControl : UserControl
    {
        public TeamTypeMission Info { get; set; }
        private bool m_Loading;
        private ListedControlController<TeamTypeMission> m_Controller;
        private TeamMission defaultMission;
        private TeamMissionArgType currentType = TeamMissionArgType.None;
        private ListItem<int>[] waypoints;
        private int mapSize;
        private ToolTip tooltip;

        public MissionItemControl()
            :this(null, null, null, null, 0, null)
        {
        }

        public MissionItemControl(TeamTypeMission info, ListedControlController<TeamTypeMission> controller, IEnumerable<TeamMission> missions, IEnumerable<ListItem<int>> waypoints, int mapSize, ToolTip tooltip)
        {
            InitializeComponent();
            SetInfo(info, controller, missions, waypoints, mapSize, tooltip);
        }

        public void SetInfo(TeamTypeMission info, ListedControlController<TeamTypeMission> controller, IEnumerable<TeamMission> missions, IEnumerable<ListItem<int>> waypoints, int mapSize, ToolTip tooltip)
        {
            try
            {
                this.m_Loading = true;
                this.Info = null;
                this.m_Controller = controller;
                TeamMission[] missionsArr = missions.ToArray();
                this.defaultMission = missionsArr.FirstOrDefault();
                this.cmbMission.DisplayMember = null;
                this.cmbMission.DataSource = null;
                this.cmbMission.Items.Clear();
                this.cmbMission.DataSource = missionsArr;
                this.cmbMission.DisplayMember = "Mission";
                this.waypoints = waypoints.ToArray();
                this.mapSize = mapSize;
                this.tooltip = tooltip;
            }
            finally
            {
                this.m_Loading = false;
            }
            if (info != null)
            {
                UpdateInfo(info);
            }
        }

        public void UpdateInfo(TeamTypeMission info)
        {
            try
            {
                this.m_Loading = true;
                this.Info = info;
                TeamMission mission = info != null ? info.Mission : defaultMission;
                int value = info != null ? info.Argument : 0;
                this.cmbMission.Text = mission.Mission;
                int newVal = UpdateValueControl(mission, value);
                if (this.Info != null)
                {
                    Info.Argument = newVal;
                }
            }
            finally
            {
                this.m_Loading = false;
            }
        }

        public void FocusValue()
        {
            this.cmbMission.Select();
        }

        private int UpdateValueControl(TeamMission mission, int value)
        {
            if (tooltip != null)
            {
                tooltip.SetToolTip(cmbMission, mission.Tooltip);
                tooltip.SetToolTip(this.numValue, null);
                tooltip.SetToolTip(this.cmbValue, null);
            }
            int newValue = value;
            switch (mission.ArgType)
            {
                case TeamMissionArgType.None:
                default:
                    this.numValue.Visible = false;
                    this.cmbValue.Visible = false;
                    newValue = 0;
                    break;
                case TeamMissionArgType.Number:
                    newValue = SetUpNumValue(0, Int32.MaxValue, 1, value, this.tooltip, "Number");
                    break;
                case TeamMissionArgType.Time:
                    newValue = SetUpNumValue(0, Int32.MaxValue, 10, value, this.tooltip, "Time in 1/10th min");
                    break;
                case TeamMissionArgType.Waypoint:
                    newValue = SetUpCmbValue(waypoints, value, tooltip, "Waypoint");
                    break;
                case TeamMissionArgType.OptionsList:
                    ListItem<int>[] items = mission.DropdownOptions.Select(ddo => new ListItem<int>(ddo.Value, ddo.Label)).ToArray();
                    newValue = SetUpCmbValue(items, value, tooltip, null);
                    break;
                case TeamMissionArgType.MapCell:
                    newValue = SetUpNumValue(0, mapSize - 1, 1, value, this.tooltip, "Map cell");
                    break;
                case TeamMissionArgType.MissionNumber:
                    newValue = SetUpNumValue(0, Int32.MaxValue, 1, value, this.tooltip, "0-based index in this orders list");
                    break;
                case TeamMissionArgType.GlobalNumber:
                    newValue = SetUpNumValue(0, 29, 1, value, this.tooltip, "Global to set");
                    break;
                case TeamMissionArgType.Tarcom:
                    newValue = SetUpNumValue(0, Int32.MaxValue, 1, value, this.tooltip, "Tarcom");
                    break;
            }
            currentType = mission.ArgType;
            return newValue;
        }

        private int SetUpNumValue(int min, int max, int mouseWheelIncrement, int curValue, ToolTip tooltip, string tooltipText)
        {
            this.cmbValue.Visible = false;
            this.numValue.Visible = true;
            this.numValue.Value = this.numValue.Minimum;
            this.numValue.Minimum = min;
            this.numValue.Maximum = max;
            this.numValue.MouseWheelIncrement = mouseWheelIncrement;
            int constrainedVal = (int)numValue.Constrain(curValue);
            this.numValue.Value = constrainedVal;
            if (tooltip != null)
            {
                tooltip.SetToolTip(this.numValue, tooltipText);
                tooltip.SetToolTip(this.cmbValue, null);
            }
            return constrainedVal;
        }

        private int SetUpCmbValue(ListItem<int>[] items, int value, ToolTip tooltip, string tooltipText)
        {
            this.numValue.Visible = false;
            this.cmbValue.Visible = true;
            this.cmbValue.DataSource = items;
            int selectIndex = ListItem.GetIndexInList(value, items);
            if (selectIndex == -1 && items.Length > 0)
            {
                selectIndex = 0;
            }
            this.cmbValue.SelectedIndex = selectIndex;
            if (tooltip != null)
            {
                tooltip.SetToolTip(this.cmbValue, tooltipText);
                tooltip.SetToolTip(this.numValue, null);
            }
            return selectIndex;
        }

        private void CmbMission_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.m_Loading || this.Info == null)
            {
                return;
            }
            TeamMission mission = this.cmbMission.SelectedItem as TeamMission;
            if (mission != null)
            {
                this.Info.Mission = mission;
                int value = -1;
                if (mission.ArgType == currentType && (currentType == TeamMissionArgType.Time || currentType == TeamMissionArgType.Waypoint))
                {
                    value = this.Info.Argument;
                }
                int newVal = UpdateValueControl(mission, value);
                this.Info.Argument = newVal;
            }
        }

        private void NumValue_ValueChanged(object sender, EventArgs e)
        {
            if (this.m_Loading || this.Info == null || !this.numValue.Visible)
            {
                return;
            }
            this.Info.Argument = (int)this.numValue.Value;
            if (this.m_Controller != null)
                this.m_Controller.UpdateControlInfo(this.Info);
        }

        private void CmbValue_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.m_Loading || this.Info == null || !this.cmbValue.Visible)
            {
                return;
            }
            ListItem<int> item = this.cmbValue.SelectedItem as ListItem<int>;
            if (item == null)
            {
                return;
            }
            this.Info.Argument = (byte)item.Value;
            if (this.m_Controller != null)
                this.m_Controller.UpdateControlInfo(this.Info);
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (this.m_Loading || this.Info == null)
            {
                return;
            }
            // Setting type to null is the signal to delete.
            this.Info.Mission = null;
            if (this.m_Controller != null)
                this.m_Controller.UpdateControlInfo(this.Info);
        }
    }
}
