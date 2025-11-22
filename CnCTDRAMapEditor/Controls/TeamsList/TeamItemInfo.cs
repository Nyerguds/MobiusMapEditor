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

namespace MobiusEditor.Controls
{
    public class TeamItemInfo : CustomControlInfo<TeamItemControl, TeamTypeClass, char, int>
    {
        ITechnoType[] technos;

        public TeamItemInfo(string name, IEnumerable<TeamTypeClass> properties, IEnumerable<ITechnoType> technos)
        {
            Name = name;
            Properties = properties.ToArray();
            this.technos = technos.ToArray();
        }

        public override TeamItemControl MakeControl(TeamTypeClass property, IListedControlController<TeamTypeClass, char, int> controller)
        {
            return new TeamItemControl(property, controller, technos);
        }

        public override void UpdateControl(TeamTypeClass property, IListedControlController<TeamTypeClass, char, int> controller, TeamItemControl control)
        {
            control.SetInfo(property, controller, technos);
        }

        public override TeamItemControl GetControlByProperty(TeamTypeClass property, IEnumerable<TeamItemControl> controls)
        {
            return controls.FirstOrDefault(ctrl => ReferenceEquals(ctrl.Info, property));
        }

        public override void HideControlTooltips(TeamItemControl control)
        {
            // Nothing to do; no tooltips on these.
        }
    }
}
