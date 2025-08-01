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

        public MissionItemInfo(string name, IEnumerable<TeamTypeMission> properties, IEnumerable<TeamMission> missions, IEnumerable<ListItem<int>> waypoints, int mapSize, ToolTip tooltip)
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
