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
using MobiusEditor.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MobiusEditor.TiberianDawn
{
    public static class TheaterTypes
    {
        private static readonly IEnumerable<string> commonTilesets = new string[] { "TD_Units", "TD_Structures", "TD_VFX", "Common_VFX" };

        public static readonly TheaterType Desert = new TheaterType(0, "Desert", "desert", "des", "TD_Terrain_Desert", commonTilesets);
        public static readonly TheaterType Jungle = new TheaterType(1, "Jungle", "jungle", "jun", true, "TD_Terrain_Jungle", commonTilesets);
        public static readonly TheaterType Temperate = new TheaterType(2, "Temperate", "temperat", "tem", "TD_Terrain_Temperate", commonTilesets);
        // Winter seems to fall back on Temperate for the Haystack graphics.
        public static readonly TheaterType Winter = new TheaterType(3, "Winter", "winter", "win", "TD_Terrain_Winter", commonTilesets.Concat("TD_Terrain_Temperate".Yield()));
        public static readonly TheaterType Snow = new TheaterType(4, "Snow", "snow", "sno", true, "TD_Terrain_Snow", commonTilesets);
        public static readonly TheaterType Caribbean = new TheaterType(5, "Caribbean", "caribbea", "car", true, "TD_Terrain_Caribbean", commonTilesets);

        private static TheaterType[] Types;

        static TheaterTypes()
        {
            Types =
                (from field in typeof(TheaterTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
                 where field.IsInitOnly && typeof(TheaterType).IsAssignableFrom(field.FieldType)
                 select field.GetValue(null) as TheaterType).OrderBy(th => th.ID).ToArray();
        }

        public static IEnumerable<TheaterType> GetAllTypes()
        {
            return Types;
        }

        public static IEnumerable<TheaterType> GetTypes()
        {
            if (Globals.UseClassicFiles)
            {
                return Types.Where(t => t.IsClassicMixFound);
            }
            else
            {
                return Types.Where(t => t.IsRemasterTilesetFound);
            }
        }
    }
}
