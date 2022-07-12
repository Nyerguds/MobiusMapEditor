using System.Linq;
using System.Windows.Forms;
using MobiusEditor.Controls;
using MobiusEditor.Event;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;

namespace MobiusEditor.Tools.Dialogs
{
    class WallsToolDialog : GenericToolDialog<WallsTool>
    {
        public WallsToolDialog(Form parentForm)
            : base(parentForm)
        {
            Text = "Walls";
        }

        public override void Initialize(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel, ToolTip mouseToolTip, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> undoRedoList)
        {
            GenericTypeListBox.Types = plugin.Map.OverlayTypes.Where(t => t.IsWall).OrderBy(t => t.ID);
            Tool = new WallsTool(mapPanel, activeLayers, toolStatusLabel,
                GenericTypeListBox, GenericTypeMapPanel, plugin, undoRedoList);
        }
    }
}
