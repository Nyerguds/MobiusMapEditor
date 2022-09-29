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
    public static class BuildingTypes
    {
        public static readonly BuildingType Weapon = new BuildingType(0, "weap", "TEXT_STRUCTURE_TITLE_GDI_WEAPONS_FACTORY", 0, 30, new bool[3, 3] { { false, false, false }, { true, true, true }, { true, true, true } }, "Goodguy", "weap2", BuildingTypeFlag.Bib);
        public static readonly BuildingType GTower = new BuildingType(1, "gtwr", "TEXT_STRUCTURE_TITLE_GDI_GUARD_TOWER", 0, 10, new bool[1, 1] { { true } }, "Goodguy");
        public static readonly BuildingType ATower = new BuildingType(2, "atwr", "TEXT_STRUCTURE_TITLE_GDI_ADV_GUARD_TOWER", 0, 20, new bool[2, 1] { { false }, { true } }, "Goodguy");
        public static readonly BuildingType Obelisk = new BuildingType(3, "obli", "TEXT_STRUCTURE_TITLE_NOD_OBELISK", 0, 150, new bool[2, 1] { { false }, { true } }, "Badguy");
        public static readonly BuildingType Command = new BuildingType(4, "hq", "TEXT_STRUCTURE_TITLE_GDI_COMM_CENTER", 0, 40, new bool[2, 2] { { true, false }, { true, true } }, "Goodguy", BuildingTypeFlag.Bib);
        public static readonly BuildingType Turret = new BuildingType(5, "gun", "TEXT_STRUCTURE_TITLE_NOD_TURRET", 0, 20, new bool[1, 1] { { true } }, "Badguy", BuildingTypeFlag.Turret);
        public static readonly BuildingType Const = new BuildingType(6, "fact", "TEXT_STRUCTURE_RA_FACT", 30, 15, new bool[2, 3] { { true, true, true }, { true, true, true } }, "Goodguy", BuildingTypeFlag.Bib | BuildingTypeFlag.Factory);
        public static readonly BuildingType Refinery = new BuildingType(7, "proc", "TEXT_STRUCTURE_TITLE_GDI_REFINERY", 10, 40, 1000, new bool[3, 3] { { false, true, false }, { true, true, true }, { false, false, false } }, "Goodguy", BuildingTypeFlag.Bib);
        public static readonly BuildingType Storage = new BuildingType(8, "silo", "TEXT_STRUCTURE_TITLE_GDI_SILO", 0, 10, 1500, new bool[1, 2] { { true, true } }, "Goodguy", BuildingTypeFlag.Bib);
        public static readonly BuildingType Helipad = new BuildingType(9, "hpad", "TEXT_STRUCTURE_TITLE_GDI_HELIPAD", 0, 10, new bool[2, 2] { { true, true }, { true, true } }, "Goodguy", BuildingTypeFlag.Bib);
        public static readonly BuildingType SAM = new BuildingType(10, "sam", "TEXT_STRUCTURE_TITLE_NOD_SAM_SITE", 0, 20, new bool[1, 2] { { true, true } }, "Badguy");
        public static readonly BuildingType AirStrip = new BuildingType(11, "afld", "TEXT_STRUCTURE_TITLE_NOD_AIRFIELD", 0, 30, new bool[2, 4] { { true, true, true, true }, { true, true, true, true } }, "Badguy", BuildingTypeFlag.Bib);
        public static readonly BuildingType Power = new BuildingType(12, "nuke", "TEXT_STRUCTURE_TITLE_GDI_POWER_PLANT", 100, 0, new bool[2, 2] { { true, false }, { true, true } }, "Goodguy", BuildingTypeFlag.Bib);
        public static readonly BuildingType AdvancedPower = new BuildingType(13, "nuk2", "TEXT_STRUCTURE_TITLE_GDI_ADV_POWER_PLANT", 200, 0, new bool[2, 2] { { true, false }, { true, true } }, "Goodguy", BuildingTypeFlag.Bib);
        public static readonly BuildingType Hospital = new BuildingType(14, "hosp", "TEXT_UNIT_TITLE_HOSP", 0, 20, 100, new bool[2, 2] { { true, true }, { true, true } }, "Goodguy", BuildingTypeFlag.Bib);
        public static readonly BuildingType Barracks = new BuildingType(15, "pyle", "TEXT_STRUCTURE_TITLE_GDI_BARRACKS", 0, 20, new bool[2, 2] { { true, true }, { false, false } }, "Goodguy", BuildingTypeFlag.Bib);
        public static readonly BuildingType Tanker = new BuildingType(16, "arco", "Oil Tanker", 0, 0, new bool[1, 2] { { true, true } }, "Neutral");
        public static readonly BuildingType Repair = new BuildingType(17, "fix", "TEXT_STRUCTURE_TITLE_GDI_REPAIR_FACILITY", 0, 30, new bool[3, 3] { { false, true, false }, { true, true, true }, { false, true, false} }, "Goodguy", BuildingTypeFlag.Bib);
        public static readonly BuildingType BioLab = new BuildingType(18, "bio", "TEXT_UNIT_TITLE_BIO", 0, 40, 100, new bool[2, 2] { { true, true }, { true, true } }, "Badguy", BuildingTypeFlag.Bib);
        public static readonly BuildingType Hand = new BuildingType(19, "hand", "TEXT_STRUCTURE_TITLE_NOD_HAND_OF_NOD", 0, 20, new bool[3, 2] { { false, false }, { true, true }, { false, true } }, "Badguy", BuildingTypeFlag.Bib);
        public static readonly BuildingType Temple = new BuildingType(20, "tmpl", "TEXT_STRUCTURE_TITLE_NOD_TEMPLE_OF_NOD", 0, 150, new bool[3, 3] { { false, false, false }, { true, true, true }, { true, true, true } }, "Badguy", BuildingTypeFlag.Bib);
        public static readonly BuildingType Eye = new BuildingType(21, "eye", "TEXT_STRUCTURE_TITLE_GDI_ADV_COMM_CENTER", 0, 200, new bool[2, 2] { { true, false }, { true, true } }, "Goodguy", BuildingTypeFlag.Bib);
        public static readonly BuildingType Mission = new BuildingType(22, "miss", "TEXT_STRUCTURE_TITLE_CIV35", 0, 0, new bool[2, 3] { { true, true, true }, { true, true, true } }, "Goodguy", BuildingTypeFlag.Bib);
        public static readonly BuildingType V01 = new BuildingType(23, "v01", "TEXT_STRUCTURE_TITLE_CIV1", 0, 0, new bool[2, 2] { { false, false }, { true, true } }, "Neutral", new [] { TheaterTypes.Temperate, TheaterTypes.Winter });
        public static readonly BuildingType V02 = new BuildingType(24, "v02", "TEXT_STRUCTURE_TITLE_CIV2", 0, 0, new bool[2, 2] { { false, false }, { true, true } }, "Neutral", new [] { TheaterTypes.Temperate, TheaterTypes.Winter });
        public static readonly BuildingType V03 = new BuildingType(25, "v03", "TEXT_STRUCTURE_TITLE_CIV3", 0, 0, new bool[2, 2] { { false, true }, { true, true } }, "Neutral", new [] { TheaterTypes.Temperate, TheaterTypes.Winter });
        public static readonly BuildingType V04 = new BuildingType(26, "v04", "TEXT_STRUCTURE_TITLE_CIV4", 0, 0, new bool[2, 2] { { false, false }, { true, true } }, "Neutral", new [] { TheaterTypes.Temperate, TheaterTypes.Winter });
        public static readonly BuildingType V05 = new BuildingType(27, "v05", "TEXT_STRUCTURE_TITLE_CIV5", 0, 0, new bool[1, 2] { { true, true } }, "Neutral", new [] { TheaterTypes.Temperate, TheaterTypes.Winter });
        public static readonly BuildingType V06 = new BuildingType(28, "v06", "TEXT_STRUCTURE_TITLE_CIV6", 0, 0, new bool[1, 2] { { true, true } }, "Neutral", new [] { TheaterTypes.Temperate, TheaterTypes.Winter });
        public static readonly BuildingType V07 = new BuildingType(29, "v07", "TEXT_STRUCTURE_TITLE_CIV7", 0, 0, new bool[1, 2] { { true, true } }, "Neutral", new [] { TheaterTypes.Temperate, TheaterTypes.Winter });
        public static readonly BuildingType V08 = new BuildingType(30, "v08", "TEXT_STRUCTURE_TITLE_CIV8", 0, 0, new bool[1, 1] { { true } }, "Neutral", new [] { TheaterTypes.Temperate, TheaterTypes.Winter });
        public static readonly BuildingType V09 = new BuildingType(31, "v09", "TEXT_STRUCTURE_TITLE_CIV9", 0, 0, new bool[1, 1] { { true } }, "Neutral", new [] { TheaterTypes.Temperate, TheaterTypes.Winter });
        public static readonly BuildingType V10 = new BuildingType(32, "v10", "TEXT_STRUCTURE_TITLE_CIV10", 0, 0, new bool[1, 1] { { true } }, "Neutral", new [] { TheaterTypes.Temperate, TheaterTypes.Winter });
        public static readonly BuildingType V11 = new BuildingType(33, "v11", "TEXT_STRUCTURE_TITLE_CIV11", 0, 0, new bool[1, 1] { { true } }, "Neutral", new [] { TheaterTypes.Temperate, TheaterTypes.Winter });
        public static readonly BuildingType V12 = new BuildingType(34, "v12", "TEXT_STRUCTURE_TITLE_CIV12", 0, 0, new bool[1, 1] { { true } }, "Neutral", new [] { TheaterTypes.Temperate, TheaterTypes.Winter });
        public static readonly BuildingType V13 = new BuildingType(35, "v13", "TEXT_STRUCTURE_TITLE_CIV12", 0, 0, new bool[1, 1] { { true } }, "Neutral", new [] { TheaterTypes.Temperate, TheaterTypes.Winter });
        public static readonly BuildingType V14 = new BuildingType(36, "v14", "TEXT_STRUCTURE_TITLE_CIV13", 0, 0, new bool[1, 1] { { true } }, "Neutral", new [] { TheaterTypes.Temperate, TheaterTypes.Winter });
        public static readonly BuildingType V15 = new BuildingType(37, "v15", "TEXT_STRUCTURE_TITLE_CIV14", 0, 0, new bool[1, 1] { { true } }, "Neutral", new [] { TheaterTypes.Temperate, TheaterTypes.Winter });
        public static readonly BuildingType V16 = new BuildingType(38, "v16", "TEXT_STRUCTURE_TITLE_CIV15", 0, 0, new bool[1, 1] { { true } }, "Neutral", new [] { TheaterTypes.Temperate, TheaterTypes.Winter });
        public static readonly BuildingType V17 = new BuildingType(39, "v17", "TEXT_STRUCTURE_TITLE_CIV16", 0, 0, new bool[1, 1] { { true } }, "Neutral", new [] { TheaterTypes.Temperate, TheaterTypes.Winter });
        public static readonly BuildingType V18 = new BuildingType(40, "v18", "TEXT_STRUCTURE_TITLE_CIV17", 0, 0, new bool[1, 1] { { true } }, "Neutral", new [] { TheaterTypes.Temperate, TheaterTypes.Winter });
        public static readonly BuildingType V19 = new BuildingType(41, "v19", "Oil Pump", 0, 0, new bool[1, 1] { { true } }, "Neutral");
        public static readonly BuildingType V20 = new BuildingType(42, "v20", "TEXT_STRUCTURE_TITLE_CIV18", 0, 0, new bool[2, 2] { { false, false }, { true, true } }, "Neutral", new [] { TheaterTypes.Desert });
        public static readonly BuildingType V21 = new BuildingType(43, "v21", "TEXT_STRUCTURE_TITLE_CIV19", 0, 0, new bool[2, 2] { { true, true }, { false, true } }, "Neutral", new [] { TheaterTypes.Desert });
        public static readonly BuildingType V22 = new BuildingType(44, "v22", "TEXT_STRUCTURE_TITLE_CIV20", 0, 0, new bool[1, 2] { { true, true } }, "Neutral", new [] { TheaterTypes.Desert });
        public static readonly BuildingType V23 = new BuildingType(45, "v23", "TEXT_STRUCTURE_TITLE_CIV21", 0, 0, new bool[1, 1] { { true } }, "Neutral", new [] { TheaterTypes.Desert });
        public static readonly BuildingType V24 = new BuildingType(46, "v24", "TEXT_STRUCTURE_TITLE_CIV22", 0, 0, new bool[2, 2] { { false, false }, { true, true } }, "Neutral", new [] { TheaterTypes.Desert });
        public static readonly BuildingType V25 = new BuildingType(47, "v25", "TEXT_STRUCTURE_TITLE_CIV1", 0, 0, new bool[2, 2] { { false, true }, { true, true } }, "Neutral", new [] { TheaterTypes.Desert });
        public static readonly BuildingType V26 = new BuildingType(48, "v26", "TEXT_STRUCTURE_TITLE_CIV23", 0, 0, new bool[1, 2] { { true, true } }, "Neutral", new [] { TheaterTypes.Desert });
        public static readonly BuildingType V27 = new BuildingType(49, "v27", "TEXT_STRUCTURE_TITLE_CIV24", 0, 0, new bool[1, 1] { { true } }, "Neutral", new [] { TheaterTypes.Desert });
        public static readonly BuildingType V28 = new BuildingType(50, "v28", "TEXT_STRUCTURE_TITLE_CIV25", 0, 0, new bool[1, 1] { { true } }, "Neutral", new [] { TheaterTypes.Desert });
        public static readonly BuildingType V29 = new BuildingType(51, "v29", "TEXT_STRUCTURE_TITLE_CIV26", 0, 0, new bool[1, 1] { { true } }, "Neutral", new [] { TheaterTypes.Desert });
        public static readonly BuildingType V30 = new BuildingType(52, "v30", "TEXT_STRUCTURE_TITLE_CIV27", 0, 0, new bool[1, 2] { { true, true } }, "Neutral", new [] { TheaterTypes.Desert });
        public static readonly BuildingType V31 = new BuildingType(53, "v31", "TEXT_STRUCTURE_TITLE_CIV28", 0, 0, new bool[1, 2] { { true, true } }, "Neutral", new [] { TheaterTypes.Desert });
        public static readonly BuildingType V32 = new BuildingType(54, "v32", "TEXT_STRUCTURE_TITLE_CIV29", 0, 0, new bool[1, 2] { { true, true } }, "Neutral", new [] { TheaterTypes.Desert });
        public static readonly BuildingType V33 = new BuildingType(55, "v33", "TEXT_STRUCTURE_TITLE_CIV30", 0, 0, new bool[1, 2] { { true, true } }, "Neutral", new [] { TheaterTypes.Desert });
        public static readonly BuildingType V34 = new BuildingType(56, "v34", "TEXT_STRUCTURE_TITLE_CIV31", 0, 0, new bool[1, 1] { { true } }, "Neutral", new [] { TheaterTypes.Desert });
        public static readonly BuildingType V35 = new BuildingType(57, "v35", "TEXT_STRUCTURE_TITLE_CIV32", 0, 0, new bool[1, 1] { { true } }, "Neutral", new [] { TheaterTypes.Desert });
        public static readonly BuildingType V36 = new BuildingType(58, "v36", "TEXT_STRUCTURE_TITLE_CIV33", 0, 0, new bool[1, 1] { { true } }, "Neutral", new [] { TheaterTypes.Desert });
        public static readonly BuildingType V37 = new BuildingType(59, "v37", "TEXT_STRUCTURE_TITLE_CIV34", 0, 0, new bool[2, 4] { { false, true, true, true }, { false, true, true, true } }, "Neutral", new [] { TheaterTypes.Desert });

        private static readonly BuildingType[] Types;

        static BuildingTypes()
        {
            Types =
                (from field in typeof(BuildingTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
                 where field.IsInitOnly && typeof(BuildingType).IsAssignableFrom(field.FieldType)
                 select field.GetValue(null) as BuildingType).ToArray();
        }

        public static IEnumerable<BuildingType> GetTypes()
        {
            // Always preserve originals
            return Types.Select(b => b.Clone());
        }
    }
}
