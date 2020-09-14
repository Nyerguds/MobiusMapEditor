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
    public class SmudgeToolDialog : GenericToolDialog<SmudgeTool>
    {
        public SmudgeToolDialog()
        {
            Text = "Smudge";
        }

        public override void Initialize(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel,
            ToolTip mouseToolTip, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> undoRedoList)
        {
            GenericTypeListBox.Types = plugin.Map.SmudgeTypes.Where(t => (t.Flag & SmudgeTypeFlag.Bib) == SmudgeTypeFlag.None).OrderBy(t => t.Name);
            Tool = new SmudgeTool(mapPanel, activeLayers, toolStatusLabel, GenericTypeListBox, GenericTypeMapPanel, plugin, undoRedoList);
        }
    }
}
