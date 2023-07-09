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

        protected override void InitializeInternal(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel, ToolTip mouseToolTip,
            IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> undoRedoList)
        {
            GenericTypeListBox.Types = plugin.Map.OverlayTypes.Where(t => t.IsWall).OrderBy(t => t.ID);
            Tool = new WallsTool(mapPanel, activeLayers, toolStatusLabel,
                GenericTypeListBox, GenericTypeMapPanel, plugin, undoRedoList);
        }
    }
}
