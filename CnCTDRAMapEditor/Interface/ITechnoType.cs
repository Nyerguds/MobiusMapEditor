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
    public interface ITechnoType: IBrowsableType
    {
        /// <summary>Object ID</summary>
        int ID { get; }
        /// <summary>Object ini name</summary>
        string Name { get; }
        /// <summary>True if the object can have a House that owns it.</summary>
        bool Ownable { get; }
        /// <summary>True if this object has a weapon. This affects the default orders for placing it on the map.</summary>
        bool IsArmed { get; }
        /// <summary>True if this object is an aircraft, and is normally not placeable on the map.</summary>
        bool IsAircraft { get; }
        /// <summary>True if this object is a fixed-wing aircraft. This treats it as 16-frame rotation, and affects the default orders for placing it on the map.</summary>
        bool IsFixedWing { get; }
        /// <summary>True if this object can harvest resources. This affects the default orders for placing it on the map.</summary>
        bool IsHarvester { get; }
        /// <summary>True if this TechnoType is only available when enabling the game's expansion pack.</summary>
        bool IsExpansionOnly { get; }
        /// <summary>True if this techno type adapts to its house colors.</summary>
        bool CanRemap { get; }
        /// <summary>True if graphics for this object could be loaded on init.</summary>
        bool GraphicsFound { get; }
    }
}
