using MobiusEditor.Controls.ControlsList;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiusEditor.Controls
{
    public class TeamItemInfo : CustomControlInfo<TeamItemControl, TeamTypeClass>
    {
        ITechnoType[] technos;

        public TeamItemInfo(String name, IEnumerable<TeamTypeClass> properties, IEnumerable<ITechnoType> technos)
        {
            Name = name;
            Properties = properties.ToArray();
            this.technos = technos.ToArray();
        }

        public override TeamItemControl GetControlByProperty(TeamTypeClass property, IEnumerable<TeamItemControl> controls)
        {
            return controls.FirstOrDefault(ctrl => ReferenceEquals(ctrl.Info, property));
        }

        public override TeamItemControl MakeControl(TeamTypeClass property, ListedControlController<TeamTypeClass> controller)
        {
            return new TeamItemControl(property, controller, technos);
        }
    }
}
