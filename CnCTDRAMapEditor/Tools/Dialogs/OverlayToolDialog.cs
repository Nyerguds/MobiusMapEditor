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
using System.Linq;
using System.Windows.Forms;
using MobiusEditor.Controls;
using MobiusEditor.Event;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;

namespace MobiusEditor.Tools.Dialogs
{
    public class OverlayToolDialog : GenericToolDialog<OverlaysTool>
    {
        public OverlayToolDialog(Form parentForm)
            : base(parentForm)
        {
            Text = "Overlay";
        }

        protected override void InitializeInternal(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel, ToolTip mouseToolTip,
            IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> undoRedoList)
        {
            GenericTypeListBox.Types = plugin.Map.OverlayTypes.
                Where(t => t.IsOverlay && (!Globals.FilterTheaterObjects || t.ExistsInTheater));

            Tool = new OverlaysTool(mapPanel, activeLayers, toolStatusLabel,
                GenericTypeListBox, GenericTypeMapPanel, plugin, undoRedoList);
        }
    }
}
