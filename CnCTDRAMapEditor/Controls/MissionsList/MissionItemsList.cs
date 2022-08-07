using MobiusEditor.Controls.ControlsList;
using MobiusEditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiusEditor.Controls
{
    public class MissionItemsList : ControlsList<MissionItemControl, TeamTypeMission>
    {
        protected override void FocusItem(MissionItemControl control)
        {
            control.FocusValue();
        }
    }
}
