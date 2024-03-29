﻿using System.Linq;
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

        protected override void InitializeInternal(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel, ToolTip mouseToolTip,
            IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> undoRedoList)
        {
            Tool = new UnitTool(mapPanel, activeLayers, toolStatusLabel, ObjectTypeListBox,
                ObjectTypeMapPanel, ObjectProperties, plugin, undoRedoList);
        }
    }
}
