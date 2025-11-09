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
using System;
using MobiusEditor.Utility;

namespace MobiusEditor.TiberianDawn
{
    public class House : Model.House
    {
        [NonSerializedINIKey]
        public PlayerColorType PrimaryScheme { get; set; }
        [NonSerializedINIKey]
        public PlayerColorType SecondaryScheme { get; set; }
        [NonSerializedINIKey]
        public PlayerColorType RadarScheme { get; set; }

        [NonSerializedINIKey]
        public override string UnitTeamColor => PrimaryScheme?.UnitColor ?? Type.UnitTeamColor;

        [NonSerializedINIKey]
        public override string BuildingTeamColor
        {
            get
            {
                if (SecondaryScheme != null)
                {
                    return SecondaryScheme.IsNone ? PrimaryScheme?.UnitColor ?? Type.UnitTeamColor : SecondaryScheme.UnitColor;
                }
                else
                {
                    return PrimaryScheme?.BuildingColor ?? PrimaryScheme?.UnitColor ?? Type.BuildingTeamColor ?? Type.UnitTeamColor;
                }
            }
        }

        [NonSerializedINIKey]
        public override string OutlineColor => RadarScheme?.UnitColor ?? PrimaryScheme?.RadarColor ?? Type.OutlineColor;

        public House(Model.HouseType type)
            : base(type)

        {
        }

        public class PlayerColorType
        {
            public string IniName { get; private set; }
            public string UnitColor { get; private set; }
            public string BuildingColor { get; private set; }
            public string RadarColor { get; private set; }
            public bool IsNone { get; private set; }

            public PlayerColorType(string iniName, string remapTable, string remapTableBuildings, string radarColor)
            {
                IsNone = false;
                IniName = iniName;
                UnitColor = remapTable;
                BuildingColor = remapTableBuildings;
                RadarColor = radarColor;
            }

            public PlayerColorType()
            {
                IsNone = true;
            }

            public static PlayerColorType GetPlayerColor(string name)
            {
                if(String.IsNullOrEmpty(name))
                {
                    return null;
                }
                for (int i = 0; i < PlayerColors.Length; ++i)
                {
                    if (name.Equals(PlayerColors[i].IniName, StringComparison.OrdinalIgnoreCase))
                    {
                        return PlayerColors[i];
                    }
                }
                return null;
            }

            public static PlayerColorType[] PlayerColors =
            {
                new PlayerColorType("Yellow",     "MULTI1",     null,            "MULTI1"), // REMAP_YELLOW
	            new PlayerColorType("Red",        "MULTI3",     null,            "MULTI3"), // REMAP_RED
	            new PlayerColorType("BlueGreen",  "MULTI2",     null,            "MULTI2"), // REMAP_BLUEGREEN
	            new PlayerColorType("Orange",     "MULTI5",     null,            "MULTI5"), // REMAP_ORANGE
	            new PlayerColorType("Green",      "MULTI4",     null,            "MULTI4"), // REMAP_GREEN
	            new PlayerColorType("Blue",       "MULTI6",     null,            "MULTI6"), // REMAP_BLUE
	            // Singleplayer House colors
                new PlayerColorType("GDI",        "GOOD",       null,            "GOOD"), // REMAP_GDI
	            new PlayerColorType("Nod",        "BAD_UNIT",   "BAD_STRUCTURE", "BAD_STRUCTURE"), // REMAP_NOD
	            new PlayerColorType("Neutral",    "GOOD",       null,            "BAD_UNIT"), // REMAP_NEUTRAL
	            new PlayerColorType("Jurassic",   "GOOD",       null,            "BAD_STRUCTURE"), // REMAP_JP
	            // New remap colors
                new PlayerColorType("DarkGrey",   "DARKGREY",   null,            "DARKGREY"), // REMAP_DARK_GREY
	            new PlayerColorType("Brown",      "MULTI7",     null,            "MULTI7"), // REMAP_BROWN
                new PlayerColorType("Burgundy",   "MULTI8",     null,            "MULTI8"), // REMAP_BURGUNDY
	            new PlayerColorType("Fire",       "FIRE",       null,            "FIRE"), // REMAP_FIRE
                new PlayerColorType("Pink",       "PINK",       null,            "PINK"), // REMAP_PINK
	            new PlayerColorType("WarmSilver", "WARMSILVER", null,            "WARMSILVER"), // REMAP_WARM_SILVER
	            // Alternate names for some of the colors
	            new PlayerColorType("Aqua",       "MULTI2",     null,            "MULTI2"), // REMAP_AQUA
	            new PlayerColorType("Grey",       "MULTI6",     null,            "MULTI6"), // REMAP_GREY
	            new PlayerColorType("Gray",       "MULTI6",     null,            "MULTI6"), // REMAP_GRAY
	            new PlayerColorType("Teal",       "MULTI2",     null,            "MULTI2"), // REMAP_TEAL
	            new PlayerColorType("DarkGray",   "DARKGREY",   null,            "DARKGREY"), // REMAP_DARK_GRAY
            };
        }
    }
}
