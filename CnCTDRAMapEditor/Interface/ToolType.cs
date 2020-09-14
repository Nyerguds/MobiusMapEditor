using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiusEditor.Interface
{
    [Flags]
    public enum ToolType
    {
        None = 0,
        Map = 1 << 0,
        Smudge = 1 << 1,
        Overlay = 1 << 2,
        Terrain = 1 << 3,
        Infantry = 1 << 4,
        Unit = 1 << 5,
        Building = 1 << 6,
        Resources = 1 << 7,
        Wall = 1 << 8,
        Waypoint = 1 << 9,
        CellTrigger = 1 << 10
    }
}
