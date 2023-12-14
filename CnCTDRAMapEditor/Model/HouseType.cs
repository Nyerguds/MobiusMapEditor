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
using System;
using System.Collections.Generic;
using System.Linq;

namespace MobiusEditor.Model
{
    [Flags]
    public enum HouseTypeFlag
    {
        /// <summary>No flags set.</summary>
        None          /**/ = 0,
        /// <summary>Is a special House, not used for normal lists.</summary>
        Special       /**/ = 1 << 1,
        /// <summary>Is used for alliances.</summary>
        ForAlliances  /**/ = 1 << 2,
        /// <summary>Special empty House, used if the plugin does not assign any House to unbuilt structures.</summary>
        BaseHouse    /**/ = 1 << 3,
    }

    public class HouseType
    {
        public int ID { get; private set; }

        public string Name { get; private set; }

        public string UnitTeamColor { get; private set; }

        public string BuildingTeamColor { get; private set; }

        public WaypointFlag MultiplayIdentifier { get; private set; }

        public HouseTypeFlag Flags { get; private set; }

        public IDictionary<string, string> OverrideTeamColors { get; private set; }

        public HouseType(int id, string name, WaypointFlag multiplayIdentifier, HouseTypeFlag flags, string unitTeamColor, string buildingTeamColor, params (string type, string teamColor)[] overrideTeamColors)
        {
            this.ID = id;
            this.Name = name;
            this.MultiplayIdentifier = multiplayIdentifier;
            this.Flags = flags;
            // Alliances not supported for house IDs larger than 32.
            if (id >= 32) Flags &= ~HouseTypeFlag.ForAlliances;
            this.UnitTeamColor = unitTeamColor;
            this.BuildingTeamColor = buildingTeamColor;
            this.OverrideTeamColors = overrideTeamColors.ToDictionary(x => x.type, x => x.teamColor);
        }

        public HouseType(int id, string name, string unitTeamColor, string buildingTeamColor, params (string type, string teamColor)[] overrideTeamColors)
            :this(id, name,  WaypointFlag.None, HouseTypeFlag.ForAlliances, unitTeamColor, buildingTeamColor, overrideTeamColors)
        {
        }

        public HouseType(int id, string name, HouseTypeFlag flags, string teamColor)
            : this(id, name, WaypointFlag.None, flags, teamColor, teamColor)
        {
        }

        public HouseType(int id, string name, string teamColor)
            : this(id, name, WaypointFlag.None, HouseTypeFlag.ForAlliances, teamColor, teamColor)
        {
        }
        public HouseType(int id, string name)
            : this(id, name, WaypointFlag.None, HouseTypeFlag.ForAlliances, null, null)
        {
        }

        public HouseType(int id, string name, WaypointFlag multiplayIdentifier, string teamColor)
            : this(id, name, multiplayIdentifier, HouseTypeFlag.ForAlliances, teamColor, teamColor)
        {
        }

        public HouseType(int id, string name, WaypointFlag multiplayIdentifier, HouseTypeFlag flags, string teamColor)
            : this(id, name, multiplayIdentifier, flags, teamColor, teamColor)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is HouseType house)
            {
                return ReferenceEquals(this, obj) || (house.Name == Name && house.ID == ID);
            }
            else if (obj is sbyte sb)
            {
                return this.ID == sb;
            }
            else if (obj is byte b)
            {
                return this.ID == b;
            }
            else if (obj is int i)
            {
                return this.ID == i;
            }
            else if (obj is string)
            {
                return string.Equals(Name, obj as string, StringComparison.OrdinalIgnoreCase);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
