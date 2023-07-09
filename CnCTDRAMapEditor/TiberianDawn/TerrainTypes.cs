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
    public static class TerrainTypes
    {
        public static readonly TerrainType Tree1 = new TerrainType(0, "t01", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate }, 2, 2, "00 10");
        public static readonly TerrainType Tree2 = new TerrainType(1, "t02", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate }, 2, 2, "00 10");
        public static readonly TerrainType Tree3 = new TerrainType(2, "t03", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate }, 2, 2, "00 10");
        public static readonly TerrainType Tree4 = new TerrainType(3, "t04", "TEXT_PROP_TITLE_CACTUS", new TheaterType[] { TheaterTypes.Desert }, 1, 1, null);
        public static readonly TerrainType Tree5 = new TerrainType(4, "t05", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate }, 2, 2, "00 10");
        public static readonly TerrainType Tree6 = new TerrainType(5, "t06", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate }, 2, 2, "00 10");
        public static readonly TerrainType Tree7 = new TerrainType(6, "t07", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate }, 2, 2, "00 10");
        public static readonly TerrainType Tree8 = new TerrainType(7, "t08", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate, TheaterTypes.Desert }, 2, 1, "10");
        public static readonly TerrainType Tree9 = new TerrainType(8, "t09", "TEXT_PROP_TITLE_CACTUS", new TheaterType[] { TheaterTypes.Desert }, 2, 1, "10");
        public static readonly TerrainType Tree10 = new TerrainType(9, "t10", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate }, 2, 2, "00 11");
        public static readonly TerrainType Tree11 = new TerrainType(10, "t11", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate }, 2, 2, "00 11");
        public static readonly TerrainType Tree12 = new TerrainType(11, "t12", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate }, 2, 2, "00 10");
        public static readonly TerrainType Tree13 = new TerrainType(12, "t13", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate }, 2, 2, "00 10");
        public static readonly TerrainType Tree14 = new TerrainType(13, "t14", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate }, 2, 2, "00 11");
        public static readonly TerrainType Tree15 = new TerrainType(14, "t15", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate }, 2, 2, "00 11");
        public static readonly TerrainType Tree16 = new TerrainType(15, "t16", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate }, 2, 2, "00 10");
        public static readonly TerrainType Tree17 = new TerrainType(16, "t17", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate }, 2, 2, "00 10");
        public static readonly TerrainType Tree18 = new TerrainType(17, "t18", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Desert }, 3, 2, "000 010");
        public static readonly TerrainType Split1 = new TerrainType(18, "split2", "TEXT_PROP_TITLE_BLOSSOM_TREE", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Winter }, 2, 2, "00 10", 30);
        public static readonly TerrainType Split2 = new TerrainType(19, "split3", "TEXT_PROP_TITLE_BLOSSOM_TREE", new TheaterType[] { TheaterTypes.Temperate, TheaterTypes.Winter, TheaterTypes.Desert }, 2, 2, "00 10", 30);
        public static readonly TerrainType Clump1 = new TerrainType(20, "tc01", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate }, 3, 2, "000 110");
        public static readonly TerrainType Clump2 = new TerrainType(21, "tc02", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate }, 3, 2, "010 110");
        public static readonly TerrainType Clump3 = new TerrainType(22, "tc03", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate }, 3, 2, "110 110");
        public static readonly TerrainType Clump4 = new TerrainType(23, "tc04", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate }, 4, 3, "0000 1110 1000");
        public static readonly TerrainType Clump5 = new TerrainType(24, "tc05", "TEXT_PROP_TITLE_TREE", new TheaterType[] { TheaterTypes.Winter, TheaterTypes.Temperate }, 4, 3, "0010 1110 0110");
        public static readonly TerrainType Rock1 = new TerrainType(25, "rock1", "TEXT_PROP_TITLE_ROCK", new TheaterType[] { TheaterTypes.Desert }, 2, 2, "00 11");
        public static readonly TerrainType Rock2 = new TerrainType(26, "rock2", "TEXT_PROP_TITLE_ROCK", new TheaterType[] { TheaterTypes.Desert }, 3, 1, "110");
        public static readonly TerrainType Rock3 = new TerrainType(27, "rock3", "TEXT_PROP_TITLE_ROCK", new TheaterType[] { TheaterTypes.Desert }, 3, 2, "000 110");
        public static readonly TerrainType Rock4 = new TerrainType(28, "rock4", "TEXT_PROP_TITLE_ROCK", new TheaterType[] { TheaterTypes.Desert }, 2, 1, "10");
        public static readonly TerrainType Rock5 = new TerrainType(29, "rock5", "TEXT_PROP_TITLE_ROCK", new TheaterType[] { TheaterTypes.Desert }, 2, 1, "10");
        public static readonly TerrainType Rock6 = new TerrainType(30, "rock6", "TEXT_PROP_TITLE_ROCK", new TheaterType[] { TheaterTypes.Desert }, 3, 2, "000 111");
        public static readonly TerrainType Rock7 = new TerrainType(31, "rock7", "TEXT_PROP_TITLE_ROCK", new TheaterType[] { TheaterTypes.Desert }, 5, 1, "11110");

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
