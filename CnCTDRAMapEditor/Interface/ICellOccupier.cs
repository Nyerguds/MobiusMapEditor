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

namespace MobiusEditor.Interface
{
    public interface ICellOccupier
    {
        /// <summary>Footprint of this object, determining where no other objects can be placed.</summary>
        /// <remarks>Given the way two-dimensional arrays work, the first value is the Y coordinate, and the second is the X coordinate</remarks>
        bool[,] OccupyMask { get; }
        /// <summary>Footprint of this object, without bibs attached.</summary>
        /// <remarks>Given the way two-dimensional arrays work, the first value is the Y coordinate, and the second is the X coordinate</remarks>
        bool[,] BaseOccupyMask { get; }
    }
}
