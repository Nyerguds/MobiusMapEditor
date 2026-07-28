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
using MobiusEditor.Interface;
using MobiusEditor.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    internal class MissionItemInfo : CustomControlInfo<MissionItemControl, TeamTypeMission, char, int>
    {
        readonly IGamePlugin plugin;
        readonly ToolTip tooltip;

        public MissionItemInfo(string name, IEnumerable<TeamTypeMission> properties, IGamePlugin plugin, ToolTip tooltip)
        {
            Name = name;
            Properties = properties.ToArray();
            this.plugin = plugin;
            this.tooltip = tooltip;
        }

        public override MissionItemControl MakeControl(TeamTypeMission property, IListedControlController<TeamTypeMission, char, int> controller, int index)
        {
            return new MissionItemControl(property, controller, plugin, tooltip, index);
        }

        public override void UpdateControl(TeamTypeMission property, IListedControlController<TeamTypeMission, char, int> controller, MissionItemControl control, int index)
        {
            control.SetInfo(property, controller, plugin, tooltip, index);
        }

        public override MissionItemControl GetControlByProperty(TeamTypeMission property, IEnumerable<MissionItemControl> controls)
        {
            return controls.FirstOrDefault(ctrl => ReferenceEquals(ctrl.Info, property));
        }

        public override void HideControlTooltips(MissionItemControl control)
        {
            control.HideAllToolTips();
        }
    }
}
