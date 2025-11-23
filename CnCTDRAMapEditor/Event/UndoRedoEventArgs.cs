//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// The Command & Conquer Map Editor and corresponding source code is distributed
// in the hope that it will be useful, but with permitted additional restrictions
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT
// distributed with this program. You should have received a copy of the
// GNU General Public License along with permitted additional restrictions
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using MobiusEditor.Controls;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using System;

namespace MobiusEditor.Event
{
    public class UndoRedoEventArgs : EventArgs, IUndoRedoEventArgs<ToolType>
    {
        public MapPanel MapPanel { get; private set; }

        public IGamePlugin Plugin { get; private set; }

        public bool Cancelled { get; set; }

        public bool NewStateIsClean { get; set; }

        public Map Map { get; private set; }

        public ToolType Source { get; set; }

        public UndoRedoEventArgs(MapPanel mapPanel, IGamePlugin plugin)
            : this(mapPanel, plugin?.Map, plugin)
        {
        }

        public UndoRedoEventArgs(MapPanel mapPanel, Map map, IGamePlugin plugin)
        {
            MapPanel = mapPanel;
            Plugin = plugin;
            Map = map;
            Cancelled = false;
            Source = ToolType.None;
        }
    }
}
