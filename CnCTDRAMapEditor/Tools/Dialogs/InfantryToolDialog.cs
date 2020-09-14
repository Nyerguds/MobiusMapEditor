using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public InfantryToolDialog(IGamePlugin plugin) : base(plugin)
        {
            Text = "Infantry";
        }

        public override void Initialize(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel, ToolTip mouseToolTip, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> undoRedoList)
        {
            ObjectTypeListBox.Types = plugin.Map.InfantryTypes.OrderBy(t => t.Name);
            Tool = new InfantryTool(mapPanel, activeLayers, toolStatusLabel, ObjectTypeListBox,
                ObjectTypeMapPanel, ObjectProperties, plugin, undoRedoList);
        }
    }
}
