using System.Linq;
using System.Windows.Forms;
using MobiusEditor.Controls;
using MobiusEditor.Event;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;

namespace MobiusEditor.Tools.Dialogs
{
    public partial class SmudgeToolDialog : ToolDialog<SmudgeTool>
    {
        public SmudgeToolDialog(Form parentForm)
            : base(parentForm)
        {
            InitializeComponent();
        }

        public SmudgeToolDialog(Form parentForm, IGamePlugin plugin)
            : this(parentForm)
        {
            smudgeProperties.Initialize(plugin, true);
        }

        protected override void InitializeInternal(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel, ToolTip mouseToolTip,
            IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> undoRedoList)
        {
            smudgeTypeListBox.Types = plugin.Map.SmudgeTypes
                .Where(t => !t.IsAutoBib && (!Globals.FilterTheaterObjects || t.Theaters == null || t.Theaters.Contains(plugin.Map.Theater)))
                .OrderBy(t => t.ID);
            Tool = new SmudgeTool(mapPanel, activeLayers, toolStatusLabel, smudgeTypeListBox, smudgeTypeMapPanel, smudgeProperties, plugin, undoRedoList);
        }
    }
}
