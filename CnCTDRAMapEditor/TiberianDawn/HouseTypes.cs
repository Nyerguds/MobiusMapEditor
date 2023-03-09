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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MobiusEditor.TiberianDawn
{
    public static class HouseTypes
    {
        /// <summary>Special House added for buildings without owner; those in the rebuild list that are not on the map.</summary>
        public static readonly HouseType None = new HouseType(-1, House.None, "NONE");
        // Normal Houses
        public static readonly HouseType Good = new HouseType(0, "GoodGuy", "GOOD");
        public static readonly HouseType Bad = new HouseType(1, "BadGuy", "BAD_UNIT", "BAD_STRUCTURE", ("harv", "BAD_STRUCTURE"), ("mcv", "BAD_STRUCTURE"));
        // Added actual recoloring
        public static readonly HouseType Neutral = new HouseType(2, "Neutral", "GOOD");
        public static readonly HouseType Special = new HouseType(3, "Special", "GOOD");
        // Fixed to match actual game. Seems they messed up the naming of the colors in the xml files by taking the color definitions from the C&C
        // game code in order, arbitrarily naming those "Multi1" to "Multi6", and then correctly applying those obviously wrongly named colors to
        // the multi-Houses in the Remastered game. The editor code logically assumed they were named after their House, and thus got it all wrong.
        public static readonly HouseType Multi1 = new HouseType(4, "Multi1", WaypointFlag.PlayerStart1, "MULTI2"); // Blue (originally teal)
        public static readonly HouseType Multi2 = new HouseType(5, "Multi2", WaypointFlag.PlayerStart2, "MULTI5"); // Orange
        public static readonly HouseType Multi3 = new HouseType(6, "Multi3", WaypointFlag.PlayerStart3, "MULTI4"); // Green
        public static readonly HouseType Multi4 = new HouseType(7, "Multi4", WaypointFlag.PlayerStart4, "MULTI6"); // Teal (originally gray)
        public static readonly HouseType Multi5 = new HouseType(8, "Multi5", WaypointFlag.PlayerStart5, "MULTI1"); // Yellow
        public static readonly HouseType Multi6 = new HouseType(9, "Multi6", WaypointFlag.PlayerStart6, "MULTI3"); // Red


        private static readonly HouseType[] Types;

        static HouseTypes()
        {
            Types =
                (from field in typeof(HouseTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
                 where field.IsInitOnly && typeof(HouseType).IsAssignableFrom(field.FieldType)
                 select field.GetValue(null) as HouseType).OrderBy(h => h.ID).ToArray();
        }

        public static IEnumerable<HouseType> GetTypes()
        {
            return Types;
        }

        public static string GetBasePlayer(string player)
        {
            return None.Name;
            //return GetClassicOpposingPlayer(player);
        }

        public static string GetClassicOpposingPlayer(string player)
        {
            return Good.Equals(player) ? Bad.Name : Good.Name;
        }
    }
}
