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
using MobiusEditor.Model;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MobiusEditor.RedAlert
{
    public static class TheaterTypes
    {
        private static readonly IEnumerable<string> commonTilesets = new string[] { "RA_Units", "RA_Structures", "RA_VFX", "Common_VFX" };

        public static readonly TheaterType Temperate = new TheaterType(0, "Temperate", "temperat", "tem", 1, "RA_Terrain_Temperate", commonTilesets);
        public static readonly TheaterType Snow = new TheaterType(1, "Snow", "snow", "sno", 6, "RA_Terrain_Snow", commonTilesets);
        public static readonly TheaterType Interior = new TheaterType(2, "Interior", "interior", "int", 0, "RA_Terrain_Interior", commonTilesets);
        // CnCNet theaters
        public static readonly TheaterType Winter = new TheaterType(3, "Winter", "winter", "win", 1, true, "RA_Terrain_Winter", commonTilesets);
        public static readonly TheaterType Desert = new TheaterType(4, "Desert", "desert", "des", 4, true, "RA_Terrain_Desert", commonTilesets);
        public static readonly TheaterType Jungle = new TheaterType(5, "Jungle", "jungle", "jun", 3, true, "RA_Terrain_Jungle", commonTilesets);
        public static readonly TheaterType Barren = new TheaterType(6, "Barren", "barren", "bar", 1, true, "RA_Terrain_Barren", commonTilesets);
        public static readonly TheaterType Cave = new TheaterType(7, "Cave", "cave", "cav", 1, true, "RA_Terrain_Cave", commonTilesets);

        private static TheaterType[] Types;

        static TheaterTypes()
        {
            Types =
                (from field in typeof(TheaterTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
                 where field.IsInitOnly && typeof(TheaterType).IsAssignableFrom(field.FieldType)
                 select field.GetValue(null) as TheaterType).OrderBy(t => t.ID).ToArray();
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
