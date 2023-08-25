using System.Linq;
using System.Windows.Forms;
using MobiusEditor.Controls;
using MobiusEditor.Event;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;

namespace MobiusEditor.Tools.Dialogs
{
    public class OverlayToolDialog : GenericToolDialog<OverlaysTool>
    {
        public OverlayToolDialog(Form parentForm)
            : base(parentForm)
        {
            Text = "Overlay";
        }

        protected override void InitializeInternal(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel, ToolTip mouseToolTip,
            IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> undoRedoList)
        {
            GenericTypeListBox.Types = plugin.Map.OverlayTypes.
                Where(t => t.IsOverlay && (!Globals.FilterTheaterObjects || t.ExistsInTheater)).
                OrderBy(t => t.ID);

            Tool = new OverlaysTool(mapPanel, activeLayers, toolStatusLabel,
                GenericTypeListBox, GenericTypeMapPanel, plugin, undoRedoList);
        }
    }
}
