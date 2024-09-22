//
// Copyright 2020 Rami Pasanen
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
