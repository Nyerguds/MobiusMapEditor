using MobiusEditor.Controls.ControlsList;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    internal class MissionItemInfo : CustomControlInfo<MissionItemControl, TeamTypeMission>
    {
        TeamMission[] missions;
        ListItem<int>[] waypoints;
        int mapSize;
        ToolTip tooltip;

        public MissionItemInfo(String name, IEnumerable<TeamTypeMission> properties, IEnumerable<TeamMission> missions, IEnumerable<ListItem<int>> waypoints, int mapSize, ToolTip tooltip)
        {
            Name = name;
            Properties = properties.ToArray();
            this.missions = missions.ToArray();
            this.waypoints = waypoints.ToArray();
            this.mapSize = mapSize;
            this.tooltip = tooltip;
        }

        public override MissionItemControl GetControlByProperty(TeamTypeMission property, IEnumerable<MissionItemControl> controls)
        {
            return controls.FirstOrDefault(ctrl => ReferenceEquals(ctrl.Info, property));
        }

        public override MissionItemControl MakeControl(TeamTypeMission property, ListedControlController<TeamTypeMission> controller)
        {
            return new MissionItemControl(property, controller, missions, waypoints, mapSize, tooltip);
        }
        public override void UpdateControl(TeamTypeMission property, ListedControlController<TeamTypeMission> controller, MissionItemControl control)
        {
            control.SetInfo(property, controller, missions, waypoints, mapSize, tooltip);
        }
    }
}
