using MobiusEditor.Controls.ControlsList;
using MobiusEditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiusEditor.Controls
{
    public class TeamItemsList : ControlsList<TeamItemControl, TeamTypeClass>
    {
        protected override void FocusItem(TeamItemControl control)
        {
            control.FocusValue();
        }
    }
}
