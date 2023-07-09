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
        public BuildingToolDialog(Form parentForm, IGamePlugin plugin)
            : base(parentForm, plugin)
        {
            Text = "Structures";
        }

        protected override void InitializeInternal(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel, ToolTip mouseToolTip,
            IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> undoRedoList)
        {
            ObjectTypeListBox.Types = plugin.Map.BuildingTypes
                .Where(t => !Globals.FilterTheaterObjects || t.Theaters == null || t.Theaters.Contains(plugin.Map.Theater))
                .OrderBy(t => t.ID);
            Tool = new BuildingTool(mapPanel, activeLayers, toolStatusLabel, ObjectTypeListBox,
                ObjectTypeMapPanel, ObjectProperties, plugin, undoRedoList);
        }
    }
}
