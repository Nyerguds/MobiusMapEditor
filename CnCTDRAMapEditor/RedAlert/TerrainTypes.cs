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
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace MobiusEditor.RedAlert
{
    public class TerrainTypes
    {
        public static readonly TerrainType Tree1Class = new TerrainType(0, "t01", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 2, 2, new Point(11, 41), "00 10");
        public static readonly TerrainType Tree2Class = new TerrainType(1, "t02", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 2, 2, new Point(11, 44), "00 10");
        public static readonly TerrainType Tree3Class = new TerrainType(2, "t03", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 2, 2, new Point(12, 45), "00 10");
        public static readonly TerrainType Tree5Class = new TerrainType(3, "t05", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 2, 2, new Point(15, 41), "00 10");
        public static readonly TerrainType Tree6Class = new TerrainType(4, "t06", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 2, 2, new Point(16, 37), "00 10");
        public static readonly TerrainType Tree7Class = new TerrainType(5, "t07", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 2, 2, new Point(15, 41), "00 10");
        public static readonly TerrainType Tree8Class = new TerrainType(6, "t08", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 2, 1, new Point(14, 22), "10");
        public static readonly TerrainType Tree10Class = new TerrainType(7, "t10", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 2, 2, new Point(25, 43), "00 11");
        public static readonly TerrainType Tree11Class = new TerrainType(8, "t11", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 2, 2, new Point(23, 44), "00 11");
        public static readonly TerrainType Tree12Class = new TerrainType(9, "t12", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 2, 2, new Point(14, 36), "00 10");
        public static readonly TerrainType Tree13Class = new TerrainType(10, "t13", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 2, 2, new Point(19, 40), "00 10");
        public static readonly TerrainType Tree14Class = new TerrainType(11, "t14", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 2, 2, new Point(19, 40), "00 11");
        public static readonly TerrainType Tree15Class = new TerrainType(12, "t15", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 2, 2, new Point(19, 40), "00 11");
        public static readonly TerrainType Tree16Class = new TerrainType(13, "t16", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 2, 2, new Point(13, 36), "00 10");
        public static readonly TerrainType Tree17Class = new TerrainType(14, "t17", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 2, 2, new Point(18, 44), "00 10");
        public static readonly TerrainType Clump1Class = new TerrainType(15, "tc01", "TEXT_PROP_TITLE_TREES", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 3, 2, new Point(28, 41), "000 110");
        public static readonly TerrainType Clump2Class = new TerrainType(16, "tc02", "TEXT_PROP_TITLE_TREES", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 3, 2, new Point(38, 41), "010 110");
        public static readonly TerrainType Clump3Class = new TerrainType(17, "tc03", "TEXT_PROP_TITLE_TREES", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 3, 2, new Point(33, 35), "110 110");
        public static readonly TerrainType Clump4Class = new TerrainType(18, "tc04", "TEXT_PROP_TITLE_TREES", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 4, 3, new Point(44, 49), "0000 1110 1000");
        public static readonly TerrainType Clump5Class = new TerrainType(19, "tc05", "TEXT_PROP_TITLE_TREES", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 4, 3, new Point(49, 58), "0010 1110 0110");
        public static readonly TerrainType Ice01 = new TerrainType(20, "ice01", "TEXT_PROP_TITLE_ICE", new TheaterType[] { TheaterTypes.Snow }, 2, 2, new Point(24, 24), null, LandType.Water);
        public static readonly TerrainType Ice02 = new TerrainType(21, "ice02", "TEXT_PROP_TITLE_ICE", new TheaterType[] { TheaterTypes.Snow }, 1, 2, new Point(12, 24), null, LandType.Water);
        public static readonly TerrainType Ice03 = new TerrainType(22, "ice03", "TEXT_PROP_TITLE_ICE", new TheaterType[] { TheaterTypes.Snow }, 2, 1, new Point(24, 12), null, LandType.Water);
        public static readonly TerrainType Ice04 = new TerrainType(23, "ice04", "TEXT_PROP_TITLE_ICE", new TheaterType[] { TheaterTypes.Snow }, 1, 1, new Point(12, 12), null, LandType.Water);
        public static readonly TerrainType Ice05 = new TerrainType(24, "ice05", "TEXT_PROP_TITLE_ICE", new TheaterType[] { TheaterTypes.Snow }, 1, 1, new Point(12, 12), null, LandType.Water);
        public static readonly TerrainType Boxes01 = new TerrainType(25, "boxes01", "TEXT_PROP_TITLE_INT_BOXES", new TheaterType[] { TheaterTypes.Interior }, 1, 1, new Point(12, 24), null);
        public static readonly TerrainType Boxes02 = new TerrainType(26, "boxes02", "TEXT_PROP_TITLE_INT_BOXES", new TheaterType[] { TheaterTypes.Interior }, 1, 1, new Point(12, 24), null);
        public static readonly TerrainType Boxes03 = new TerrainType(27, "boxes03", "TEXT_PROP_TITLE_INT_BOXES", new TheaterType[] { TheaterTypes.Interior }, 1, 1, new Point(12, 24), null);
        public static readonly TerrainType Boxes04 = new TerrainType(28, "boxes04", "TEXT_PROP_TITLE_INT_BOXES", new TheaterType[] { TheaterTypes.Interior }, 1, 1, new Point(12, 24), null);
        public static readonly TerrainType Boxes05 = new TerrainType(29, "boxes05", "TEXT_PROP_TITLE_INT_BOXES", new TheaterType[] { TheaterTypes.Interior }, 1, 1, new Point(12, 24), null);
        public static readonly TerrainType Boxes06 = new TerrainType(30, "boxes06", "TEXT_PROP_TITLE_INT_BOXES", new TheaterType[] { TheaterTypes.Interior }, 1, 1, new Point(12, 24), null);
        public static readonly TerrainType Boxes07 = new TerrainType(31, "boxes07", "TEXT_PROP_TITLE_INT_BOXES", new TheaterType[] { TheaterTypes.Interior }, 1, 1, new Point(12, 24), null);
        public static readonly TerrainType Boxes08 = new TerrainType(32, "boxes08", "TEXT_PROP_TITLE_INT_BOXES", new TheaterType[] { TheaterTypes.Interior }, 1, 1, new Point(12, 24), null);
        public static readonly TerrainType Boxes09 = new TerrainType(33, "boxes09", "TEXT_PROP_TITLE_INT_BOXES", new TheaterType[] { TheaterTypes.Interior }, 1, 1, new Point(12, 24), null);
        public static readonly TerrainType Mine = new TerrainType(34, "mine", "TEXT_PROP_TITLE_OREMINE", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Snow }, 1, 1, new Point(12, 24), null, "OREMINE");

        private static TerrainType[] Types;

        static TerrainTypes()
        {
            Types =
                (from field in typeof(TerrainTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
                 where field.IsInitOnly && typeof(TerrainType).IsAssignableFrom(field.FieldType)
                 select field.GetValue(null) as TerrainType).ToArray();
        }

        public static IEnumerable<TerrainType> GetTypes()
        {
            return Types;
        }
    }
}
