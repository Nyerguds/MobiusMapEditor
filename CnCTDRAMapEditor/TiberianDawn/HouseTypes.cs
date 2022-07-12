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
        public static readonly HouseType Good = new HouseType(0, "GoodGuy", "GOOD");
        public static readonly HouseType Bad = new HouseType(1, "BadGuy", "BAD_UNIT", "BAD_STRUCTURE", ("harv", "BAD_STRUCTURE"), ("mcv", "BAD_STRUCTURE"));
        // Added actual recoloring
        public static readonly HouseType Neutral = new HouseType(2, "Neutral", "GOOD");
        public static readonly HouseType Special = new HouseType(3, "Special", "GOOD");
        // fixed to match actual game. Seems they messed up the naming of these in the xml files, and then applied that bizarre mixup to the Houses correctly
        // in the Remastered game, leaving the editor, which requested the obviously-correct name for each Multi-House color, to show it all wrong.
        public static readonly HouseType Multi1 = new HouseType(4, "Multi1", "MULTI2");
        public static readonly HouseType Multi2 = new HouseType(5, "Multi2", "MULTI5");
        public static readonly HouseType Multi3 = new HouseType(6, "Multi3", "MULTI4");
        public static readonly HouseType Multi4 = new HouseType(7, "Multi4", "MULTI6");
        public static readonly HouseType Multi5 = new HouseType(8, "Multi5", "MULTI1");
        public static readonly HouseType Multi6 = new HouseType(9, "Multi6", "MULTI3");

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
            return Bad.Equals(player) ? Good.Name : Bad.Name;
        }
    }
}
