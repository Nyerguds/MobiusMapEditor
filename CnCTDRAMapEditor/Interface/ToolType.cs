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
using System;

namespace MobiusEditor.Interface
{
    [Flags]
    public enum ToolType
    {
        None        /**/ = 0,
        Map         /**/ = 1 <<  0,
        Smudge      /**/ = 1 <<  1,
        Overlay     /**/ = 1 <<  2,
        Terrain     /**/ = 1 <<  3,
        Infantry    /**/ = 1 <<  4,
        Unit        /**/ = 1 <<  5,
        Building    /**/ = 1 <<  6,
        Resources   /**/ = 1 <<  7,
        Wall        /**/ = 1 <<  8,
        Waypoint    /**/ = 1 <<  9,
        CellTrigger /**/ = 1 << 10,
        Select      /**/ = 1 << 11
    }
}
