using MobiusEditor.Controls.ControlsList;
using MobiusEditor.Model;
using MobiusEditor.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiusEditor.Controls
{
    internal class MissionItemInfo : CustomControlInfo<MissionItemControl, TeamTypeMission>
    {
        TeamMission[] missions;
        DropDownItem<int>[] waypoints;

        public MissionItemInfo(String name, IEnumerable<TeamTypeMission> properties, IEnumerable<TeamMission> missions, IEnumerable<DropDownItem<int>> waypoints)
        {
            Name = name;
            Properties = properties.ToArray();
            this.missions = missions.ToArray();
            this.waypoints = waypoints.ToArray();
        }

        public override MissionItemControl GetControlByProperty(TeamTypeMission property, IEnumerable<MissionItemControl> controls)
        {
            return controls.FirstOrDefault(ctrl => ReferenceEquals(ctrl.Info, property));
        }

        public override MissionItemControl MakeControl(TeamTypeMission property, ListedControlController<TeamTypeMission> controller)
        {
            return new MissionItemControl(property, controller, missions, waypoints);
        }
    }
}
