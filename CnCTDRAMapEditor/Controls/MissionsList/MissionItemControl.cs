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
        private DropDownItem<int>[] waypoints;
        private int mapSize;
        private ToolTip tooltip;

        public MissionItemControl()
            :this(null, null, null, null, 0, null)
        {
            
        }

        public MissionItemControl(TeamTypeMission info, ListedControlController<TeamTypeMission> controller, IEnumerable<TeamMission> missions, IEnumerable<DropDownItem<int>> waypoints, int mapSize, ToolTip tooltip)
        {
            InitializeComponent();
            SetInfo(info, controller, missions, waypoints, mapSize, tooltip);
        }

        public void SetInfo(TeamTypeMission info, ListedControlController<TeamTypeMission> controller, IEnumerable<TeamMission> missions, IEnumerable<DropDownItem<int>> waypoints, int mapSize, ToolTip tooltip) 
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
                UpdateInfo(info);
        }

        public void UpdateInfo(TeamTypeMission info)
        {
            try
            {
                this.m_Loading = true;
                this.Info = info;
                TeamMission mission = info != null ? info.Mission : defaultMission;
                long value = info != null ? info.Argument : 0;
                this.cmbMission.Text = mission.Mission;
                UpdateValueControl(mission, value);
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

        private void UpdateValueControl(TeamMission mission, long value)
        {
            int selectIndex;
            switch (mission.ArgType)
            {
                case TeamMissionArgType.None:
                    this.numValue.Visible = false;
                    this.cmbValue.Visible = false;
                    if (this.Info != null)
                        this.Info.Argument = 0;
                    break;
                case TeamMissionArgType.Waypoint:
                    this.numValue.Visible = false;
                    this.cmbValue.Visible = true;
                    this.cmbValue.DataSource = waypoints;
                    //this.cmbValue.Tool
                    selectIndex = DropDownItem<int>.GetIndexInList((int)value, waypoints);
                    if (selectIndex == -1 && waypoints.Length > 0)
                    {
                        selectIndex = 0;
                    }
                    cmbValue.SelectedIndex = selectIndex;
                    break;
                case TeamMissionArgType.OptionsList:
                    this.numValue.Visible = false;
                    this.cmbValue.Visible = true;
                    DropDownItem<int>[] items = mission.DropdownOptions.Select(tup => new DropDownItem<int>(tup.Item1, tup.Item2)).ToArray();
                    this.cmbValue.DataSource = items;
                    selectIndex = DropDownItem<int>.GetIndexInList((int)value, items);
                    if (selectIndex == -1 && items.Length > 0)
                    {
                        selectIndex = 0;
                    }
                    cmbValue.SelectedIndex = selectIndex;
                    break;
                case TeamMissionArgType.MapCell:
                    this.numValue.Value = this.numValue.Minimum;
                    this.numValue.Minimum = 0;
                    this.numValue.Maximum = mapSize - 1;
                    this.numValue.Visible = true;
                    this.numValue.Value = numValue.Constrain(value);
                    this.cmbValue.Visible = false;
                    break;
                default:
                    // Number, time, global, tarcom
                    // Might split this up for tooltips later.
                    this.numValue.Value = this.numValue.Minimum;
                    this.numValue.Minimum = 0;
                    this.numValue.Maximum = Int32.MaxValue;
                    this.numValue.Visible = true;
                    this.numValue.Value = numValue.Constrain(value);
                    this.cmbValue.Visible = false;
                    break;
            }
            if (tooltip != null)
            {

                tooltip.SetToolTip(cmbValue, null);
                tooltip.SetToolTip(numValue, null);
                switch (mission.ArgType)
                {
                    case TeamMissionArgType.None:
                        break;
                    case TeamMissionArgType.Number:
                        tooltip.SetToolTip(numValue, "number");
                        break;
                    case TeamMissionArgType.Time:
                        tooltip.SetToolTip(numValue, "Time in 1/10th min");
                        break;
                    case TeamMissionArgType.Waypoint:
                        tooltip.SetToolTip(cmbValue, "Waypoint");
                        break;
                    case TeamMissionArgType.OptionsList:
                        break;
                    case TeamMissionArgType.MapCell:
                        tooltip.SetToolTip(numValue, "Map cell");
                        break;
                    case TeamMissionArgType.OrderNumber:
                        tooltip.SetToolTip(numValue, "0-based index in this orders list");
                        break;
                    case TeamMissionArgType.GlobalNumber:
                        tooltip.SetToolTip(cmbValue, "Global to set"); break;
                    case TeamMissionArgType.Tarcom:
                        tooltip.SetToolTip(numValue, "Tarcom");
                        break;
                }
            }
            currentType = mission.ArgType;
        }

        private void cmbMission_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if (this.m_Loading || this.Info == null)
            {
                return;
            }
            TeamMission mission = this.cmbMission.SelectedItem as TeamMission;
            if (mission != null)
            {
                this.Info.Mission = mission;
                long value = -1;
                if (mission.ArgType == currentType && (currentType == TeamMissionArgType.Time || currentType == TeamMissionArgType.Waypoint))
                {
                    value = Info.Argument;
                }
                UpdateValueControl(mission, value);
            }
        }

        private void numAmount_ValueChanged(Object sender, EventArgs e)
        {
            if (this.m_Loading || this.Info == null || !this.numValue.Visible)
            {
                return;
            }
            this.Info.Argument = (Int64)this.numValue.Value;
            if (this.m_Controller != null)
                this.m_Controller.UpdateControlInfo(this.Info);
        }

        private void cmbValue_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if (this.m_Loading || this.Info == null || !this.cmbValue.Visible)
            {
                return;
            }
            DropDownItem<int> item = this.cmbValue.SelectedItem as DropDownItem<int>;
            if (item == null)
            {
                return;
            }
            this.Info.Argument = (Byte)item.Value;
            if (this.m_Controller != null)
                this.m_Controller.UpdateControlInfo(this.Info);
        }

        private void btnRemove_Click(Object sender, EventArgs e)
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
