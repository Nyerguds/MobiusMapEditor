using MobiusEditor.Controls.ControlsList;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public partial class MissionItemControl : UserControl
    {
        public TeamTypeMission Info { get; set; }
        private Boolean m_Loading;
        private ListedControlController<TeamTypeMission> m_Controller;
        private TeamMission defaultMission;
        private TeamMissionArgType currentType = TeamMissionArgType.None;
        private DropDownItem<int>[] waypoints;

        public MissionItemControl()
            :this(null, null, null, null)
        {
            
        }

        public MissionItemControl(TeamTypeMission info, ListedControlController<TeamTypeMission> controller, IEnumerable<TeamMission> missions, IEnumerable<DropDownItem<int>> waypoints)
        {
            InitializeComponent();
            this.m_Controller = controller;
            TeamMission[] missionsArr = missions.ToArray();
            this.defaultMission = missionsArr.FirstOrDefault();
            this.cmbMission.DataSource = missionsArr;
            this.cmbMission.DisplayMember = "Mission";
            this.waypoints = waypoints.ToArray();
            if (info != null)
                UpdateInfo(info);
        }

        public void UpdateInfo(TeamTypeMission info)
        {
            try
            {
                m_Loading = true;
                this.Info = info;
                TeamMission mission = info != null ? info.Mission : defaultMission;
                int value = info != null ? info.Argument : 0;
                this.cmbMission.Text = mission.Mission;
                UpdateValueControl(mission, value);
            }
            finally
            {
                m_Loading = false;
            }
        }

        public void FocusValue()
        {
            this.cmbMission.Select();
        }

        private void UpdateValueControl(TeamMission mission, int value)
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
                    selectIndex = DropDownItem<int>.GetIndexInList(value, waypoints);
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
                    selectIndex = DropDownItem<int>.GetIndexInList(value, items);
                    if (selectIndex == -1 && items.Length > 0)
                    {
                        selectIndex = 0;
                    }
                    cmbValue.SelectedIndex = selectIndex;
                    break;
                default:
                    // Number, time, tarcom
                    // Might split this up for tooltips later.
                    this.numValue.Visible = true;
                    this.cmbValue.Visible = false;
                    break;
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
                int value = -1;
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
            this.Info.Argument = (Byte)this.numValue.Value;
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
