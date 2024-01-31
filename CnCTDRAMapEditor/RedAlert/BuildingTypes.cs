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
using MobiusEditor.Utility;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace MobiusEditor.RedAlert
{
    public static class BuildingTypes
    {
        public static readonly BuildingType AdvancedTech = new BuildingType(0, "atek", "TEXT_STRUCTURE_RA_ATEK", 0, 200, 2, 2, null, "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType IronCurtain = new BuildingType(1, "iron", "TEXT_STRUCTURE_RA_IRON", 0, 200, 2, 2, "00 11", "USSR");
        public static readonly BuildingType Weapon = new BuildingType(2, "weap", "TEXT_STRUCTURE_RA_WEAP", 0, 30, 3, 2, null, "Greece", "weap2", null, BuildingTypeFlag.Bib);
        public static readonly BuildingType Chronosphere = new BuildingType(3, "pdox", "TEXT_STRUCTURE_RA_PDOX", 0, 200, 2, 2, null, "Greece");
        public static readonly BuildingType Pillbox = new BuildingType(4, "pbox", "TEXT_STRUCTURE_RA_PBOX", 0, 15, 1, 1, null, "Greece");
        public static readonly BuildingType CamoPillbox = new BuildingType(5, "hbox", "TEXT_STRUCTURE_RA_HBOX", 0, 15, 1, 1, null, "Greece");
        public static readonly BuildingType Command = new BuildingType(6, "dome", "TEXT_STRUCTURE_RA_DOME", 0, 40, 2, 2, null, "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType GapGenerator = new BuildingType(7, "gap", "TEXT_STRUCTURE_RA_GAP", 0, 60, 1, 2, "0 1", "Greece", 13, BuildingTypeFlag.GapGenerator);
        public static readonly BuildingType Turret = new BuildingType(8, "gun", "TEXT_STRUCTURE_RA_GUN", 0, 40, 1, 1, null, "Greece", BuildingTypeFlag.Turret);
        public static readonly BuildingType AAGun = new BuildingType(9, "agun", "TEXT_STRUCTURE_RA_AGUN", 0, 50, 1, 2, "0 1", "Greece", BuildingTypeFlag.Turret);
        public static readonly BuildingType FlameTurret = new BuildingType(10, "ftur", "TEXT_STRUCTURE_RA_FTUR", 0, 20, 1, 1, null, "USSR");
        public static readonly BuildingType Const = new BuildingType(11, "fact", "TEXT_STRUCTURE_RA_FACT", 0, 0, 3, 3, null, "Greece", BuildingTypeFlag.Bib | BuildingTypeFlag.Factory);
        public static readonly BuildingType Refinery = new BuildingType(12, "proc", "TEXT_STRUCTURE_RA_PROC", 0, 30, 2000, 3, 3, "010 111 100", "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType Storage = new BuildingType(13, "silo", "TEXT_STRUCTURE_RA_SILO", 0, 10, 1500, 1, 1, null, "Greece");
        public static readonly BuildingType Helipad = new BuildingType(14, "hpad", "TEXT_STRUCTURE_RA_HPAD", 0, 10, 2, 2, null, "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType SAM = new BuildingType(15, "sam", "TEXT_STRUCTURE_RA_SAM", 0, 20, 2, 1, null, "USSR", BuildingTypeFlag.Turret);
        public static readonly BuildingType AirStrip = new BuildingType(16, "afld", "TEXT_STRUCTURE_RA_AFLD", 0, 30, 3, 2, null, "USSR");
        public static readonly BuildingType Power = new BuildingType(17, "powr", "TEXT_STRUCTURE_RA_POWR", 100, 0, 2, 2, null, "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType AdvancedPower = new BuildingType(18, "apwr", "TEXT_STRUCTURE_RA_APWR", 200, 0, 3, 3, "000 111 111", "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType SovietTech = new BuildingType(19, "stek", "TEXT_STRUCTURE_RA_STEK", 0, 100, 3, 3, "000 111 111", "USSR", BuildingTypeFlag.Bib);
        public static readonly BuildingType Hospital = new BuildingType(20, "hosp", "TEXT_STRUCTURE_RA_HOSP", 0, 20, 2, 2, null, "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType Barracks = new BuildingType(21, "barr", "TEXT_STRUCTURE_RA_BARR", 0, 20, 2, 2, null, "USSR", BuildingTypeFlag.Bib);
        public static readonly BuildingType Tent = new BuildingType(22, "tent", "TEXT_STRUCTURE_RA_TENT", 0, 20, 2, 2, null, "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType Kennel = new BuildingType(23, "kenn", "TEXT_STRUCTURE_RA_KENN", 0, 10, 1, 1, null, "USSR");
        public static readonly BuildingType Repair = new BuildingType(24, "fix", "TEXT_STRUCTURE_RA_FIX", 0, 30, 3, 3, "010 111 010", "Greece", BuildingTypeFlag.None, BuildingType.ZOrderPaved);
        public static readonly BuildingType BioLab = new BuildingType(25, "bio", "TEXT_STRUCTURE_RA_BIO", 0, 40, 2, 2, null, "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType Mission = new BuildingType(26, "miss", "TEXT_STRUCTURE_RA_MISS", 0, 0, 3, 2, null, "Greece", BuildingTypeFlag.Bib);
        public static readonly BuildingType ShipYard = new BuildingType(27, "syrd", "TEXT_STRUCTURE_RA_SYRD", 0, 30, 3, 3, null, "Greece");
        public static readonly BuildingType SubPen = new BuildingType(28, "spen", "TEXT_STRUCTURE_RA_SPEN", 0, 30, 3, 3, null, "USSR");
        public static readonly BuildingType MissileSilo = new BuildingType(29, "mslo", "TEXT_STRUCTURE_RA_MSLO", 0, 100, 2, 1, null, "Greece");
        public static readonly BuildingType ForwardCom = new BuildingType(30, "fcom", "TEXT_STRUCTURE_RA_FCOM", 0, 200, 2, 2, "00 11", "USSR", BuildingTypeFlag.Bib);
        public static readonly BuildingType Tesla = new BuildingType(31, "tsla", "TEXT_STRUCTURE_RA_TSLA", 0, 150, 1, 2, "0 1", "USSR");
        // Fake buildings
        public static readonly BuildingType FakeWeapon = new BuildingType(32, "weaf", "TEXT_STRUCTURE_RA_WEAF", 0, 2, 3, 2, null, "Greece", "weap2", "weap", BuildingTypeFlag.Bib | BuildingTypeFlag.Fake);
        public static readonly BuildingType FakeConst = new BuildingType(33, "facf", "TEXT_STRUCTURE_RA_FACF", 0, 2, 3, 3, null, "Greece", "fact", BuildingTypeFlag.Bib | BuildingTypeFlag.Fake);
        public static readonly BuildingType FakeShipYard = new BuildingType(34, "syrf", "TEXT_STRUCTURE_RA_SYRF", 0, 2, 3, 3, null, "Greece", "syrd", BuildingTypeFlag.Fake);
        public static readonly BuildingType FakeSubPen = new BuildingType(35, "spef", "TEXT_STRUCTURE_RA_SPEF", 0, 2, 3, 3, null, "USSR", "spen", BuildingTypeFlag.Fake);
        public static readonly BuildingType FakeCommand = new BuildingType(36, "domf", "TEXT_STRUCTURE_RA_DOMF", 0, 2, 2, 2, null, "Greece", "dome", BuildingTypeFlag.Bib | BuildingTypeFlag.Fake);
        // Added "walls-as-buildings"
        public static readonly BuildingType Sandbag = new BuildingType(37, "sbag", "TEXT_STRUCTURE_RA_SBAG", 0, 0, 1, 1, null, "Greece", BuildingTypeFlag.Wall | BuildingTypeFlag.NoRemap);
        public static readonly BuildingType Cyclone = new BuildingType(38, "cycl", "TEXT_STRUCTURE_RA_CYCL", 0, 0, 1, 1, null, "Greece", BuildingTypeFlag.Wall | BuildingTypeFlag.NoRemap);
        public static readonly BuildingType Brick = new BuildingType(39, "brik", "TEXT_STRUCTURE_RA_BRIK", 0, 0, 1, 1, null, "Greece", BuildingTypeFlag.Wall | BuildingTypeFlag.NoRemap);
        public static readonly BuildingType Barbwire = new BuildingType(40, "barb", "TEXT_STRUCTURE_RA_BARB", 0, 0, 1, 1, null, "Greece", BuildingTypeFlag.Wall | BuildingTypeFlag.NoRemap);
        public static readonly BuildingType Wood = new BuildingType(41, "wood", "TEXT_STRUCTURE_RA_WOOD", 0, 0, 1, 1, null, "Greece", BuildingTypeFlag.Wall | BuildingTypeFlag.NoRemap);
        public static readonly BuildingType Fence = new BuildingType(42, "fenc", "TEXT_STRUCTURE_RA_FENC", 0, 0, 1, 1, null, "Greece", BuildingTypeFlag.Wall | BuildingTypeFlag.NoRemap);
        // Mines
        public static readonly BuildingType AVMine = new BuildingType(43, "minv", "TEXT_STRUCTURE_RA_MINV", 0, 0, 1, 1, null, "Greece", BuildingTypeFlag.SingleFrame);
        public static readonly BuildingType APMine = new BuildingType(44, "minp", "TEXT_STRUCTURE_RA_MINP", 0, 0, 1, 1, null, "Greece", BuildingTypeFlag.SingleFrame);
        // Civilian buildings
        public static readonly BuildingType V01 = new BuildingType(45, "v01", "TEXT_STRUCTURE_TITLE_CIV1", 0, 0, 2, 2, "00 11", "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V02 = new BuildingType(46, "v02", "TEXT_STRUCTURE_TITLE_CIV2", 0, 0, 2, 2, "00 11", "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V03 = new BuildingType(47, "v03", "TEXT_STRUCTURE_TITLE_CIV3", 0, 0, 2, 2, "01 11", "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V04 = new BuildingType(48, "v04", "TEXT_STRUCTURE_TITLE_CIV4", 0, 0, 2, 2, "00 11", "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V05 = new BuildingType(49, "v05", "TEXT_STRUCTURE_TITLE_CIV5", 0, 0, 2, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V06 = new BuildingType(50, "v06", "TEXT_STRUCTURE_TITLE_CIV6", 0, 0, 2, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V07 = new BuildingType(51, "v07", "TEXT_STRUCTURE_TITLE_CIV7", 0, 0, 2, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V08 = new BuildingType(52, "v08", "TEXT_STRUCTURE_TITLE_CIV8", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V09 = new BuildingType(53, "v09", "TEXT_STRUCTURE_TITLE_CIV9", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V10 = new BuildingType(54, "v10", "TEXT_STRUCTURE_TITLE_CIV10", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V11 = new BuildingType(55, "v11", "TEXT_STRUCTURE_TITLE_CIV11", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        // Added additional string ID for string "Haystacks" (plural). Remaster just copies "TEXT_STRUCTURE_TITLE_CIV12".
        public static readonly BuildingType V12 = new BuildingType(56, "v12", "TEXT_STRUCTURE_TITLE_CIV12B", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V13 = new BuildingType(57, "v13", "TEXT_STRUCTURE_TITLE_CIV12", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V14 = new BuildingType(58, "v14", "TEXT_STRUCTURE_TITLE_CIV13", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V15 = new BuildingType(59, "v15", "TEXT_STRUCTURE_TITLE_CIV14", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V16 = new BuildingType(60, "v16", "TEXT_STRUCTURE_TITLE_CIV15", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V17 = new BuildingType(61, "v17", "TEXT_STRUCTURE_TITLE_CIV16", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V18 = new BuildingType(62, "v18", "TEXT_STRUCTURE_TITLE_CIV17", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        // Added additional string ID for string "Oil Pump"; was missing entirely.
        public static readonly BuildingType V19 = new BuildingType(63, "v19", "TEXT_STRUCTURE_TITLE_OIL_PUMP", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap);
        // Desert civilian buildings. Will never show up unless the option to filter theater-illegals is disabled.
        public static readonly BuildingType V20 = new BuildingType(64, "v20", "TEXT_STRUCTURE_TITLE_CIV18", 0, 0, 2, 2, "00 11", "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V21 = new BuildingType(65, "v21", "TEXT_STRUCTURE_TITLE_CIV19", 0, 0, 2, 2, "11 01", "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V22 = new BuildingType(66, "v22", "TEXT_STRUCTURE_TITLE_CIV20", 0, 0, 2, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V23 = new BuildingType(67, "v23", "TEXT_STRUCTURE_TITLE_CIV21", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V24 = new BuildingType(68, "v24", "TEXT_STRUCTURE_TITLE_CIV22", 0, 0, 2, 2, "00 11", "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        // Second "Church" string. Can be useful to have this different for classic mods. Remaster just copies "TEXT_STRUCTURE_TITLE_CIV1".
        public static readonly BuildingType V25 = new BuildingType(69, "v25", "TEXT_STRUCTURE_TITLE_CIV1B", 0, 0, 2, 2, "01 11", "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V26 = new BuildingType(70, "v26", "TEXT_STRUCTURE_TITLE_CIV23", 0, 0, 2, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V27 = new BuildingType(71, "v27", "TEXT_STRUCTURE_TITLE_CIV24", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V28 = new BuildingType(72, "v28", "TEXT_STRUCTURE_TITLE_CIV25", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V29 = new BuildingType(73, "v29", "TEXT_STRUCTURE_TITLE_CIV26", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V30 = new BuildingType(74, "v30", "TEXT_STRUCTURE_TITLE_CIV27", 0, 0, 2, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V31 = new BuildingType(75, "v31", "TEXT_STRUCTURE_TITLE_CIV28", 0, 0, 2, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V32 = new BuildingType(76, "v32", "TEXT_STRUCTURE_TITLE_CIV29", 0, 0, 2, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V33 = new BuildingType(77, "v33", "TEXT_STRUCTURE_TITLE_CIV30", 0, 0, 2, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V34 = new BuildingType(78, "v34", "TEXT_STRUCTURE_TITLE_CIV31", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V35 = new BuildingType(79, "v35", "TEXT_STRUCTURE_TITLE_CIV32", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V36 = new BuildingType(80, "v36", "TEXT_STRUCTURE_TITLE_CIV33", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent);
        public static readonly BuildingType V37 = new BuildingType(81, "v37", "TEXT_STRUCTURE_TITLE_CIV34", 0, 0, 4, 2, "0111 0111", "Neutral", BuildingTypeFlag.NoRemap | BuildingTypeFlag.TheaterDependent, BuildingType.ZOrderFlat);

        public static readonly BuildingType Barrel = new BuildingType(82, "barl", "TEXT_STRUCTURE_RA_BARL", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap);
        public static readonly BuildingType Barrel3 = new BuildingType(83, "brl3", "TEXT_STRUCTURE_RA_BRL3", 0, 0, 1, 1, null, "Neutral", BuildingTypeFlag.NoRemap);
        public static readonly BuildingType Queen = new BuildingType(84, "quee", "TEXT_STRUCTURE_RA_QUEE", 0, 0, 2, 1, null, "USSR");
        public static readonly BuildingType Larva1 = new BuildingType(85, "lar1", "TEXT_STRUCTURE_RA_LAR1", 0, 0, 1, 1, null, "USSR", BuildingTypeFlag.NoRemap);
        public static readonly BuildingType Larva2 = new BuildingType(86, "lar2", "TEXT_STRUCTURE_RA_LAR2", 0, 0, 1, 1, null, "USSR", BuildingTypeFlag.NoRemap);

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
            return Types.Where(b => Globals.AllowWallBuildings || !b.IsWall).Select(b => b.Clone());
        }
    }

    public class RaBuildingIniSection
    {
        [DefaultValue(0)]
        public int Power { get; set; }

        [DefaultValue(0)]
        public int Storage { get; set; }

        [TypeConverter(typeof(OneZeroBooleanTypeConverter))]
        [DefaultValue(0)]
        public bool Bib { get; set; }
    }
}
