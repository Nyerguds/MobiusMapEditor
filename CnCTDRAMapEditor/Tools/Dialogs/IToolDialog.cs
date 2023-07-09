using MobiusEditor.Controls;
using MobiusEditor.Event;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using System;
using System.Windows.Forms;

namespace MobiusEditor.Tools.Dialogs
{
    public interface IToolDialog : IDisposable
    {
        ITool GetTool();
        void SetTool(ITool value);
        void Initialize(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel, ToolTip mouseToolTip,
            IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> undoRedoList);
    }
}
