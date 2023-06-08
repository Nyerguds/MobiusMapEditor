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

namespace MobiusEditor.RedAlert
{
    public static class HouseTypes
    {
        public static readonly HouseType Spain = new HouseType(0, "Spain", "SPAIN");
        public static readonly HouseType Greece = new HouseType(1, "Greece", "GREECE");
        public static readonly HouseType USSR = new HouseType(2, "USSR", "USSR");
        public static readonly HouseType England = new HouseType(3, "England", "ENGLAND");
        public static readonly HouseType Ukraine = new HouseType(4, "Ukraine", "UKRAINE");
        public static readonly HouseType Germany = new HouseType(5, "Germany", "GERMANY");
        public static readonly HouseType France = new HouseType(6, "France", "FRANCE");
        public static readonly HouseType Turkey = new HouseType(7, "Turkey", "TURKEY");
        public static readonly HouseType Good = new HouseType(8, "GoodGuy", "GOOD");
        public static readonly HouseType Bad = new HouseType(9, "BadGuy", "BAD");
        public static readonly HouseType Neutral = new HouseType(10, "Neutral", "NEUTRAL");
        public static readonly HouseType Special = new HouseType(11, "Special", "SPECIAL");
        public static readonly HouseType Multi1 = new HouseType(12, "Multi1", WaypointFlag.PlayerStart1, "MULTI1"); // yellow
        public static readonly HouseType Multi2 = new HouseType(13, "Multi2", WaypointFlag.PlayerStart2, "MULTI2"); // teal
        public static readonly HouseType Multi3 = new HouseType(14, "Multi3", WaypointFlag.PlayerStart3, "MULTI3"); // red
        public static readonly HouseType Multi4 = new HouseType(15, "Multi4", WaypointFlag.PlayerStart4, "MULTI4"); // green
        public static readonly HouseType Multi5 = new HouseType(16, "Multi5", WaypointFlag.PlayerStart5, "MULTI5"); // orange
        public static readonly HouseType Multi6 = new HouseType(17, "Multi6", WaypointFlag.PlayerStart6, "MULTI7"); // purple; fixed to match actual game. 
        public static readonly HouseType Multi7 = new HouseType(18, "Multi7", WaypointFlag.PlayerStart7, "MULTI6"); // blue; fixed to match actual game. 
        public static readonly HouseType Multi8 = new HouseType(19, "Multi8", WaypointFlag.PlayerStart8, "MULTI8"); // pink

        private static readonly HouseType[] Types;

        static HouseTypes()
        {
            Types =
                (from field in typeof(HouseTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
                 where field.IsInitOnly && typeof(HouseType).IsAssignableFrom(field.FieldType)
                 select field.GetValue(null) as HouseType).ToArray();
        }

        public static IEnumerable<HouseType> GetTypes()
        {
            return Types;
        }

        public static string GetBasePlayer(string player)
        {
            return GetClassicOpposingPlayer(player);
        }

        public static string GetClassicOpposingPlayer(string player)
        {
            return (USSR.Equals(player) || Ukraine.Equals(player) || Bad.Equals(player)) ? Greece.Name : USSR.Name;
        }
    }
}
