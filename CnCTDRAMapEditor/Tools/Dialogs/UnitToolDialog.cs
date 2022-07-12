using System.Linq;
using System.Windows.Forms;
using MobiusEditor.Controls;
using MobiusEditor.Event;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;

namespace MobiusEditor.Tools.Dialogs
{
    public class UnitToolDialog : ObjectToolDialog<UnitTool>
    {
        public UnitToolDialog(Form parentForm, IGamePlugin plugin)
            : base(parentForm, plugin)
        {
            Text = "Units";
        }

        public override void Initialize(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel, ToolTip mouseToolTip, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> undoRedoList)
        {
            ObjectTypeListBox.Types = plugin.Map.UnitTypes.Where(t => !t.IsFixedWing).OrderBy(t => t.ID);
            Tool = new UnitTool(mapPanel, activeLayers, toolStatusLabel, ObjectTypeListBox,
                ObjectTypeMapPanel, ObjectProperties, plugin, undoRedoList);
        }
    }
}
