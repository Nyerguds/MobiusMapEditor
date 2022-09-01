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
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace MobiusEditor.RedAlert
{
    public static class BuildingTypes
    {
        public static readonly BuildingType AdvancedTech = new BuildingType(0, "atek", "TEXT_STRUCTURE_RA_ATEK", 0, 200, new bool[2, 2] { { true, true }, { true, true } }, "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType IronCurtain = new BuildingType(1, "iron", "TEXT_STRUCTURE_RA_IRON", 0, 200, new bool[2, 2] { { false, false }, { true, true } },"USSR");
        public static readonly BuildingType Weapon = new BuildingType(2, "weap", "TEXT_STRUCTURE_RA_WEAP", 0, 30, new bool[2, 3] { { true, true, true }, { true, true, true } }, "Greece", "weap2", BuildingTypeFlag.Bib);
        public static readonly BuildingType Chronosphere = new BuildingType(3, "pdox", "TEXT_STRUCTURE_RA_PDOX", 0, 200, new bool[2, 2] { { true, true }, { true, true } }, "Greece");
        public static readonly BuildingType Pillbox = new BuildingType(4, "pbox", "TEXT_STRUCTURE_RA_PBOX", 0, 15, new bool[1, 1] { { true } }, "Greece");
        public static readonly BuildingType CamoPillbox = new BuildingType(5, "hbox", "TEXT_STRUCTURE_RA_HBOX", 0, 15, new bool[1, 1] { { true } }, "Greece");
        public static readonly BuildingType Command = new BuildingType(6, "dome", "TEXT_STRUCTURE_RA_DOME", 0, 40, new bool[2, 2] { { true, true }, { true, true } }, "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType GapGenerator = new BuildingType(7, "gap", "TEXT_STRUCTURE_RA_GAP", 60, 0, new bool[2, 1] { { false }, { true } }, "Greece");
        public static readonly BuildingType Turret = new BuildingType(8, "gun", "TEXT_STRUCTURE_RA_GUN", 0, 40, new bool[1, 1] { { true } }, "Greece", BuildingTypeFlag.Turret);
        public static readonly BuildingType AAGun = new BuildingType(9, "agun", "TEXT_STRUCTURE_RA_AGUN", 0, 50, new bool[2, 1] { { false }, { true } }, "Greece", BuildingTypeFlag.Turret);
        public static readonly BuildingType FlameTurret = new BuildingType(10, "ftur", "TEXT_STRUCTURE_RA_FTUR", 0, 20, new bool[1, 1] { { true } }, "USSR");
        public static readonly BuildingType Const = new BuildingType(11, "fact", "TEXT_STRUCTURE_RA_FACT", 0, 0, new bool[3, 3] { { true, true, true }, { true, true, true }, { true, true, true } }, "Greece", BuildingTypeFlag.Bib | BuildingTypeFlag.Factory);
        public static readonly BuildingType Refinery = new BuildingType(12, "proc", "TEXT_STRUCTURE_RA_PROC", 0, 30, new bool[3, 3] { { false, true, false }, { true, true, true }, { true, false, false } }, "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType Storage = new BuildingType(13, "silo", "TEXT_STRUCTURE_RA_SILO", 0, 10, new bool[1, 1] { { true } }, "Greece");
        public static readonly BuildingType Helipad = new BuildingType(14, "hpad", "TEXT_STRUCTURE_RA_HPAD", 0, 10, new bool[2, 2] { { true, true }, { true, true } }, "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType SAM = new BuildingType(15, "sam", "TEXT_STRUCTURE_RA_SAM", 0, 20, new bool[1, 2] { { true, true } }, "USSR", BuildingTypeFlag.Turret);
        public static readonly BuildingType AirStrip = new BuildingType(16, "afld", "TEXT_STRUCTURE_RA_AFLD", 0, 30, new bool[2, 3] { { true, true, true }, { true, true, true } }, "USSR");
        public static readonly BuildingType Power = new BuildingType(17, "powr", "TEXT_STRUCTURE_RA_POWR", 100, 0, new bool[2, 2] { { true, true }, { true, true } }, "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType AdvancedPower = new BuildingType(18, "apwr", "TEXT_STRUCTURE_RA_APWR", 200, 0, new bool[3, 3] { { false, false, false }, { true, true, true }, { true, true, true } }, "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType SovietTech = new BuildingType(19, "stek", "TEXT_STRUCTURE_RA_STEK", 0, 100, new bool[3, 3] { { false, false, false }, { true, true, true }, { true, true, true } }, "USSR", BuildingTypeFlag.Bib);
        public static readonly BuildingType Hospital = new BuildingType(20, "hosp", "TEXT_STRUCTURE_RA_HOSP", 0, 20, new bool[2, 2] { { true, true }, { true, true } }, "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType Barracks = new BuildingType(21, "barr", "TEXT_STRUCTURE_RA_BARR", 0, 20, new bool[2, 2] { { true, true }, { true, true } }, "USSR", BuildingTypeFlag.Bib);
        public static readonly BuildingType Tent = new BuildingType(22, "tent", "TEXT_STRUCTURE_RA_TENT", 0, 20, new bool[2, 2] { { true, true }, { true, true } }, "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType Kennel = new BuildingType(23, "kenn", "TEXT_STRUCTURE_RA_KENN", 0, 10, new bool[1, 1] { { true } }, "USSR");
        public static readonly BuildingType Repair = new BuildingType(24, "fix", "TEXT_STRUCTURE_RA_FIX", 0, 30, new bool[3, 3] { { false, true, false }, { true, true, true }, { false, true, false } }, "Greece");
        public static readonly BuildingType BioLab = new BuildingType(25, "bio", "TEXT_STRUCTURE_RA_BIO", 0, 40, new bool[2, 2] { { true, true }, { true, true } }, "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType Mission = new BuildingType(26, "miss", "TEXT_STRUCTURE_RA_MISS", 0, 0, new bool[2, 3] { { true, true, true }, { true, true, true } }, "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType ShipYard = new BuildingType(27, "syrd", "TEXT_STRUCTURE_RA_SYRD", 0, 30, new bool[3, 3] { { true, true, true }, { true, true, true }, { true, true, true } }, "Greece");
        public static readonly BuildingType SubPen = new BuildingType(28, "spen", "TEXT_STRUCTURE_RA_SPEN", 0, 30, new bool[3, 3] { { true, true, true }, { true, true, true }, { true, true, true } }, "USSR");
        public static readonly BuildingType MissileSilo = new BuildingType(29, "mslo", "TEXT_STRUCTURE_RA_MSLO", 0, 100, new bool[1, 2] { { true, true } }, "Greece", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType ForwardCom = new BuildingType(30, "fcom", "TEXT_STRUCTURE_RA_FCOM", 0, 200, new bool[2, 2] { { false, false }, { true, true } }, "USSR", BuildingTypeFlag.Bib);
        public static readonly BuildingType Tesla = new BuildingType(31, "tsla", "TEXT_STRUCTURE_RA_TSLA", 0, 150, new bool[2, 1] { { false }, { true } }, "USSR");
        public static readonly BuildingType FakeWeapon = new BuildingType(32, "weap", "TEXT_STRUCTURE_RA_WEAF", 0, 2, new bool[2, 3] { { true, true, true }, { true, true, true } }, "Greece", "weap2", BuildingTypeFlag.Bib | BuildingTypeFlag.Fake);
        public static readonly BuildingType FakeConst = new BuildingType(33, "fact", "TEXT_STRUCTURE_RA_FACF", 0, 2, new bool[3, 3] { { true, true, true }, { true, true, true }, { true, true, true } }, "Greece", BuildingTypeFlag.Bib | BuildingTypeFlag.Fake);
        public static readonly BuildingType FakeShipYard = new BuildingType(34, "syrd", "TEXT_STRUCTURE_RA_SYRF", 0, 2, new bool[3, 3] { { true, true, true }, { true, true, true }, { true, true, true } }, "Greece", BuildingTypeFlag.Fake);
        public static readonly BuildingType FakeSubPen = new BuildingType(35, "spen", "TEXT_STRUCTURE_RA_SPEF", 0, 2, new bool[3, 3] { { true, true, true }, { true, true, true }, { true, true, true } }, "USSR", BuildingTypeFlag.Fake);
        public static readonly BuildingType FakeCommand = new BuildingType(36, "dome", "TEXT_STRUCTURE_RA_DOMF", 0, 2, new bool[2, 2] { { true, true }, { true, true } }, "Greece", BuildingTypeFlag.Bib | BuildingTypeFlag.Fake);
        public static readonly BuildingType AVMine = new BuildingType(43, "minv", "TEXT_STRUCTURE_RA_MINV", 0, 0, new bool[1, 1] { { true } }, "Greece", BuildingTypeFlag.SingleFrame);
        public static readonly BuildingType APMine = new BuildingType(44, "minp", "TEXT_STRUCTURE_RA_MINP", 0, 0, new bool[1, 1] { { true } }, "Greece", BuildingTypeFlag.SingleFrame);
        public static readonly BuildingType V01 = new BuildingType(45, "v01", "TEXT_STRUCTURE_TITLE_CIV1", 0, 0, new bool[2, 2] { { false, false }, { true, true } }, "Neutral", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType V02 = new BuildingType(46, "v02", "TEXT_STRUCTURE_TITLE_CIV2", 0, 0, new bool[2, 2] { { false, false }, { true, true } }, "Neutral", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType V03 = new BuildingType(47, "v03", "TEXT_STRUCTURE_TITLE_CIV3", 0, 0, new bool[2, 2] { { false, true }, { true, true } }, "Neutral", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType V04 = new BuildingType(48, "v04", "TEXT_STRUCTURE_TITLE_CIV4", 0, 0, new bool[2, 2] { { false, false }, { true, true } }, "Neutral", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType V05 = new BuildingType(49, "v05", "TEXT_STRUCTURE_TITLE_CIV5", 0, 0, new bool[1, 2] { { true, true } }, "Neutral", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType V06 = new BuildingType(50, "v06", "TEXT_STRUCTURE_TITLE_CIV6", 0, 0, new bool[1, 2] { { true, true } }, "Neutral", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType V07 = new BuildingType(51, "v07", "TEXT_STRUCTURE_TITLE_CIV7", 0, 0, new bool[1, 2] { { true, true } }, "Neutral", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType V08 = new BuildingType(52, "v08", "TEXT_STRUCTURE_TITLE_CIV8", 0, 0, new bool[1, 1] { { true } }, "Neutral", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType V09 = new BuildingType(53, "v09", "TEXT_STRUCTURE_TITLE_CIV9", 0, 0, new bool[1, 1] { { true } }, "Neutral", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType V10 = new BuildingType(54, "v10", "TEXT_STRUCTURE_TITLE_CIV10", 0, 0, new bool[1, 1] { { true } }, "Neutral", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType V11 = new BuildingType(55, "v11", "TEXT_STRUCTURE_TITLE_CIV11", 0, 0, new bool[1, 1] { { true } }, "Neutral", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType V12 = new BuildingType(56, "v12", "TEXT_STRUCTURE_TITLE_CIV12", 0, 0, new bool[1, 1] { { true } }, "Neutral", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType V13 = new BuildingType(57, "v13", "TEXT_STRUCTURE_TITLE_CIV12", 0, 0, new bool[1, 1] { { true } }, "Neutral", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType V14 = new BuildingType(58, "v14", "TEXT_STRUCTURE_TITLE_CIV13", 0, 0, new bool[1, 1] { { true } }, "Neutral", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType V15 = new BuildingType(59, "v15", "TEXT_STRUCTURE_TITLE_CIV14", 0, 0, new bool[1, 1] { { true } }, "Neutral", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType V16 = new BuildingType(60, "v16", "TEXT_STRUCTURE_TITLE_CIV15", 0, 0, new bool[1, 1] { { true } }, "Neutral", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType V17 = new BuildingType(61, "v17", "TEXT_STRUCTURE_TITLE_CIV16", 0, 0, new bool[1, 1] { { true } }, "Neutral", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType V18 = new BuildingType(62, "v18", "TEXT_STRUCTURE_TITLE_CIV17", 0, 0, new bool[1, 1] { { true } }, "Neutral", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType V19 = new BuildingType(63, "v19", "Oil Pump", 0, 0, new bool[1, 1] { { true } }, "Neutral", new[] { TheaterTypes.Temperate, TheaterTypes.Snow });
        public static readonly BuildingType Barrel = new BuildingType(82, "barl", "TEXT_STRUCTURE_RA_BARL", 0, 0, new bool[1, 1] { { true } }, "Neutral");
        public static readonly BuildingType Barrel3 = new BuildingType(83, "brl3", "TEXT_STRUCTURE_RA_BRL3", 0, 0, new bool[1, 1] { { true } }, "Neutral");
        public static readonly BuildingType Queen = new BuildingType(84, "quee", "TEXT_STRUCTURE_RA_QUEE", 0, 0, new bool[1, 2] { { true, true } }, "Special");
        public static readonly BuildingType Larva1 = new BuildingType(85, "lar1", "TEXT_STRUCTURE_RA_LAR1", 0, 0, new bool[1, 1] { { true } }, "Special");
        public static readonly BuildingType Larva2 = new BuildingType(86, "lar2", "TEXT_STRUCTURE_RA_LAR2", 0, 0, new bool[1, 1] { { true } }, "Special");

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

    public class RaBuildingIniSection
    {
        [DefaultValue(0)]
        public int Power { get; set; }

        [TypeConverter(typeof(OneZeroBooleanTypeConverter))]
        [DefaultValue(false)]
        public bool Bib { get; set; }
    }
}
