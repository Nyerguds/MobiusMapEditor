using System.Linq;
using System.Windows.Forms;
using MobiusEditor.Controls;
using MobiusEditor.Event;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;

namespace MobiusEditor.Tools.Dialogs
{
    public class BuildingToolDialog : ObjectToolDialog<BuildingTool>
    {
        public BuildingToolDialog(IGamePlugin plugin) : base(plugin)
        {
            Text = "Structures";
        }

        public override void Initialize(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel, ToolTip mouseToolTip, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> undoRedoList)
        {
            ObjectTypeListBox.Types = plugin.Map.BuildingTypes
                .Where(t => (t.Theaters == null) || t.Theaters.Contains(plugin.Map.Theater))
                .OrderBy(t => t.IsFake)
                .ThenBy(t => t.Name);
            Tool = new BuildingTool(mapPanel, activeLayers, toolStatusLabel, ObjectTypeListBox,
                ObjectTypeMapPanel, ObjectProperties, plugin, undoRedoList);
        }
    }
}
