//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.

// The Command & Conquer Map Editor and corresponding source code is distributed
// in the hope that it will be useful, but with permitted additional restrictions
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT
// distributed with this program. You should have received a copy of the
// GNU General Public License along with permitted additional restrictions
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using MobiusEditor.Model;
using MobiusEditor.Widgets;
using System;
using System.Windows.Forms;

namespace MobiusEditor.Interface
{
    public interface ITool : IDisposable
    {
        /// <summary>Game plugin set in the Tool</summary>
        IGamePlugin Plugin { get; }
        /// <summary>Navigation widget set in the tool.</summary>
        NavigationWidget NavigationWidget { get; }

        /// <summary>Indicates that the tool is in the middle of an operation, and undo/redo actions are not allowed at that moment.</summary>
        bool IsBusy { get; }
        /// <summary>Gets whether this tool is currently activated.</summary>
        bool IsActive { get; }

        /// <summary>Allows getting/setting the current layers to paint.</summary>
        MapLayerFlag Layers { get; set; }
        /// <summary>Allows extracting/setting the state of the current selection and mock object in the Tool.</summary>
        Object CurrentObject { get; set; }
        /// <summary>Allows the Tool to force the main window to refresh its mouse cell info without a real mouse move event occurring.</summary>
        event EventHandler RequestMouseInfoRefresh;

        /// <summary>Activate the Tool</summary>
        void Activate();
        /// <summary>Deactivate the Tool</summary>
        void Deactivate();
        /// <summary>Update the status label.</summary>
        void UpdateStatus();
    }
}
