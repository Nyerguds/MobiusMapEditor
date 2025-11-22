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
        private IListedControlController<TeamTypeMission, char, int> m_Controller;
        private TeamMission[] missionsArr;
        private TeamMission defaultMission;
        private TeamMissionArgType currentType = TeamMissionArgType.None;
        private ListItem<int>[] waypoints;
        private int mapSize;
        private ToolTip tooltip;

        public MissionItemControl()
            :this(null, null, null, null, 0, null)
        {
        }

        public MissionItemControl(TeamTypeMission info, IListedControlController<TeamTypeMission, char, int> controller,
            IEnumerable<TeamMission> missions, IEnumerable<ListItem<int>> waypoints, int mapSize, ToolTip tooltip)
        {
            InitializeComponent();
            SetInfo(info, controller, missions, waypoints, mapSize, tooltip);
        }

        public void SetInfo(TeamTypeMission info, IListedControlController<TeamTypeMission, char, int> controller,
            IEnumerable<TeamMission> missions, IEnumerable<ListItem<int>> waypoints, int mapSize, ToolTip tooltip)
        {
            TeamTypeMission old = Info;
            bool updatedMissions = false;
            bool updatedWaypoints = false;
            bool updatedMapSize = false;
            try
            {
                m_Loading = true;
                Info = null;
                m_Controller = controller;
                TeamMission[] tmpMissArr = missions.ToArray();
                if (!ArrayUtils.ArraysAreEqual(missionsArr, tmpMissArr))
                {
                    updatedMissions = true;
                    missionsArr = tmpMissArr;
                    defaultMission = missionsArr.FirstOrDefault();
                    cmbMission.DisplayMember = null;
                    cmbMission.DataSource = missionsArr;
                    cmbMission.DisplayMember = "Mission";
                }
                ListItem<int>[] tmpWpArr = waypoints.ToArray();
                if (!ArrayUtils.ArraysAreEqual(this.waypoints, tmpWpArr))
                {
                    updatedWaypoints = true;
                    this.waypoints = waypoints.ToArray();
                }
                updatedMapSize = this.mapSize != mapSize;
                this.mapSize = mapSize;
                this.tooltip = tooltip;
            }
            finally
            {
                m_Loading = false;
            }
            if (info != null)
            {
                if (!updatedMissions && !updatedWaypoints && !updatedMapSize &&
                    old != null && info.Mission == old.Mission && info.Argument == old.Argument)
                {
                    Info = info;
                }
                else
                {
                    UpdateInfo(info);
                }
                // When resetting an item, always clear all tooltips.
                HideAllToolTips();
            }
        }

        public void UpdateInfo(TeamTypeMission info)
        {
            try
            {
                m_Loading = true;
                Info = info;
                TeamMission mission = info != null ? info.Mission : defaultMission;
                int value = info != null ? info.Argument : 0;
                cmbMission.Text = mission.Mission;
                int newVal = UpdateValueControl(mission, value);
                if (Info != null)
                {
                    Info.Argument = newVal;
                }
            }
            finally
            {
                m_Loading = false;
            }
        }

        public void FocusValue()
        {
            cmbMission.Select();
        }

        public void FocusButton()
        {
            btnOptions.Select();
        }

        public void HideAllToolTips()
        {
            Control_MouseLeave(cmbMission, new EventArgs());
            Control_MouseLeave(numValue, new EventArgs());
            Control_MouseLeave(cmbValue, new EventArgs());
        }

        private int UpdateValueControl(TeamMission mission, int value)
        {
            if (tooltip != null)
            {
                tooltip.SetToolTip(cmbMission, mission.Tooltip);
                tooltip.SetToolTip(numValue, null);
                tooltip.SetToolTip(cmbValue, null);
            }
            int newValue = value;
            switch (mission.ArgType)
            {
                case TeamMissionArgType.None:
                default:
                    numValue.Visible = false;
                    cmbValue.Visible = false;
                    newValue = 0;
                    break;
                case TeamMissionArgType.Number:
                    newValue = SetUpNumValue(0, Int32.MaxValue, 1, value, tooltip, "Number");
                    break;
                case TeamMissionArgType.Time:
                    newValue = SetUpNumValue(0, Int32.MaxValue, 10, value, tooltip, "Time in 1/10th min");
                    break;
                case TeamMissionArgType.Waypoint:
                    newValue = SetUpCmbValue(waypoints, value, tooltip, "Waypoint");
                    break;
                case TeamMissionArgType.OptionsList:
                    ListItem<int>[] items = mission.DropdownOptions.Select(ddo => ListItem.Create(ddo.Value, ddo.Label)).ToArray();
                    newValue = SetUpCmbValue(items, value, tooltip, null);
                    break;
                case TeamMissionArgType.MapCell:
                    newValue = SetUpNumValue(0, mapSize - 1, 1, value, tooltip, "Map cell");
                    break;
                case TeamMissionArgType.MissionNumber:
                    newValue = SetUpNumValue(0, Int32.MaxValue, 1, value, tooltip, "0-based index in this orders list");
                    break;
                case TeamMissionArgType.GlobalNumber:
                    newValue = SetUpNumValue(0, 29, 1, value, tooltip, "Global to set");
                    break;
                case TeamMissionArgType.Tarcom:
                    newValue = SetUpNumValue(0, Int32.MaxValue, 1, value, tooltip, "Tarcom");
                    break;
            }
            currentType = mission.ArgType;
            return newValue;
        }

        private int SetUpNumValue(int min, int max, int mouseWheelIncrement, int curValue, ToolTip tooltip, string tooltipText)
        {
            cmbValue.Visible = false;
            numValue.Visible = true;
            numValue.Value = numValue.Minimum;
            numValue.Minimum = min;
            numValue.Maximum = max;
            numValue.MouseWheelIncrement = mouseWheelIncrement;
            int constrainedVal = (int)numValue.Constrain(curValue);
            numValue.Value = constrainedVal;
            if (tooltip != null)
            {
                tooltip.SetToolTip(numValue, tooltipText);
                tooltip.SetToolTip(cmbValue, null);
            }
            return constrainedVal;
        }

        private int SetUpCmbValue(ListItem<int>[] items, int value, ToolTip tooltip, string tooltipText)
        {
            numValue.Visible = false;
            cmbValue.Visible = true;
            cmbValue.DataSource = items;
            int selectIndex = ListItem.GetIndexInList(value, items);
            if (selectIndex == -1 && items.Length > 0)
            {
                selectIndex = 0;
            }
            cmbValue.SelectedIndex = selectIndex;
            if (tooltip != null)
            {
                tooltip.SetToolTip(cmbValue, tooltipText);
                tooltip.SetToolTip(numValue, null);
            }
            return selectIndex;
        }

        private void CmbMission_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_Loading || Info == null)
            {
                return;
            }
            if (cmbMission.SelectedItem is TeamMission mission)
            {
                Info.Mission = mission;
                int value = -1;
                if (mission.ArgType == currentType && (currentType == TeamMissionArgType.Time || currentType == TeamMissionArgType.Waypoint))
                {
                    value = Info.Argument;
                }
                int newVal = UpdateValueControl(mission, value);
                Info.Argument = newVal;
            }
        }

        private void NumValue_ValueChanged(object sender, EventArgs e)
        {
            if (m_Loading || Info == null || !numValue.Visible)
            {
                return;
            }
            Info.Argument = (int)numValue.Value;
            m_Controller?.UpdateControlInfo(Info, 'E');
        }

        private void CmbValue_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_Loading || Info == null || !cmbValue.Visible)
            {
                return;
            }
            if (!(cmbValue.SelectedItem is ListItem<int> item))
            {
                return;
            }
            Info.Argument = (byte)item.Value;
            // E = Edit (has no real effect)
            m_Controller?.UpdateControlInfo(Info, 'E');
        }

        private void BtnOptions_Click(object sender, EventArgs e)
        {
            Control src = sender as Control;
            if (m_Loading || Info == null || src == null)
            {
                return;
            }
            if (m_Controller != null)
            {
                // Hide irrelevant options.
                // I = request Index of current item.
                int index = m_Controller.UpdateControlInfo(Info, 'I');
                tsmiMoveUp.Visible = index > 0;
                // L = request Length of whole list.
                int length = m_Controller.UpdateControlInfo(Info, 'L');
                tsmiMoveDown.Visible = index < length - 1;
                // M = request Maximum length allowed in list.
                int max = m_Controller.UpdateControlInfo(Info, 'M');
                Boolean maxNotReached = length < max;
                tsmiDuplicate.Visible = maxNotReached;
                tsmiInsert.Visible = maxNotReached;
            }
            else
            {
                tsmiMoveUp.Visible = true;
                tsmiMoveDown.Visible = true;
                tsmiDuplicate.Visible = true;
                tsmiInsert.Visible = true;
            }
            cmsOptions.Show(src, 0, src.Height);
        }

        private void TsmiDelete_Click(object sender, EventArgs e)
        {
            if (m_Loading || Info == null) return;
            // R = Remove current item.
            m_Controller?.UpdateControlInfo(Info, 'R');
        }

        private void TsmiMoveUp_Click(object sender, EventArgs e)
        {
            if (m_Loading || Info == null) return;
            // U = move current item Up.
            m_Controller?.UpdateControlInfo(Info, 'U');
        }

        private void TsmiMoveDown_Click(object sender, EventArgs e)
        {
            if (m_Loading || Info == null) return;
            // D = move current item Down.
            m_Controller?.UpdateControlInfo(Info, 'D');
        }

        private void TsmiDuplicate_Click(object sender, EventArgs e)
        {
            if (m_Loading || Info == null) return;
            // C = Clone current item.
            m_Controller?.UpdateControlInfo(Info, 'C');
        }

        private void TsmiInsert_Click(object sender, EventArgs e)
        {
            if (m_Loading || Info == null) return;
            // A = Add new item on current item's spot, pushing the current item down.
            m_Controller?.UpdateControlInfo(Info, 'A');
        }

        private void Control_MouseLeave(object sender, EventArgs e)
        {
            // Not sure why the toolitops sometimes linger when moving the mouse
            // onto the equivalent control of another item, but this fixes it.
            if (tooltip == null || !(sender is Control ctrl)) return;
            if (tooltip.GetToolTip(ctrl) != null)
            {
                tooltip.Hide(ctrl);
            }
        }

    }
}
