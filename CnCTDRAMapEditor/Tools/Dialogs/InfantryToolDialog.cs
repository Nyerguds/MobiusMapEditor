using System.Linq;
using System.Windows.Forms;
using MobiusEditor.Controls;
using MobiusEditor.Event;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;

namespace MobiusEditor.Tools.Dialogs
{
    public class InfantryToolDialog : ObjectToolDialog<InfantryTool>
    {
        public InfantryToolDialog(Form parentForm, IGamePlugin plugin)
            : base(parentForm, plugin)
        {
            Text = "Infantry";
        }

        protected override void InitializeInternal(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel, ToolTip mouseToolTip, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> undoRedoList)
        {
            Tool = new InfantryTool(mapPanel, activeLayers, toolStatusLabel, ObjectTypeListBox,
                ObjectTypeMapPanel, ObjectProperties, plugin, undoRedoList);
        }
    }
}
