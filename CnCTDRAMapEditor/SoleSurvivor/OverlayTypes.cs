//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                     Version 2, December 2004
//
//  Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
//  Everyone is permitted to copy and distribute verbatim or modified
//  copies of this license document, and changing it is allowed as long
//  as the name is changed.
//
//             DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//    TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//   0. You just DO WHAT THE FUCK YOU WANT TO.
using MobiusEditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MobiusEditor.SoleSurvivor
{
    public static class OverlayTypes
    {
        public static readonly OverlayType Concrete = new OverlayType(0, "conc", "TEXT_OVERLAY_CONCRETE_PAVEMENT", OverlayTypeFlag.Pavement | OverlayTypeFlag.Concrete);
        public static readonly OverlayType Sandbag = new OverlayType(1, "sbag", "TEXT_STRUCTURE_TITLE_GDI_SANDBAGS", OverlayTypeFlag.Wall);
        public static readonly OverlayType Cyclone = new OverlayType(2, "cycl", "TEXT_STRUCTURE_TITLE_GDI_CHAIN_LINK", OverlayTypeFlag.Wall);
        public static readonly OverlayType Brick = new OverlayType(3, "brik", "TEXT_STRUCTURE_TITLE_GDI_CONCRETE", OverlayTypeFlag.Wall);
        public static readonly OverlayType Barbwire = new OverlayType(4, "barb", "TEXT_STRUCTURE_RA_BARB", OverlayTypeFlag.Wall);
        public static readonly OverlayType Wood = new OverlayType(5, "wood", "TEXT_STRUCTURE_TD_WOOD", OverlayTypeFlag.Wall);
        public static readonly OverlayType Tiberium1 = new OverlayType(6, "ti1", "TEXT_OVERLAY_TIBERIUM", OverlayTypeFlag.TiberiumOrGold, 11);
        public static readonly OverlayType Tiberium2 = new OverlayType(7, "ti2", "TEXT_OVERLAY_TIBERIUM", OverlayTypeFlag.TiberiumOrGold, 11);
        public static readonly OverlayType Tiberium3 = new OverlayType(8, "ti3", "TEXT_OVERLAY_TIBERIUM", OverlayTypeFlag.TiberiumOrGold, 11);
        public static readonly OverlayType Tiberium4 = new OverlayType(9, "ti4", "TEXT_OVERLAY_TIBERIUM", OverlayTypeFlag.TiberiumOrGold, 11);
        public static readonly OverlayType Tiberium5 = new OverlayType(10, "ti5", "TEXT_OVERLAY_TIBERIUM", OverlayTypeFlag.TiberiumOrGold, 11);
        public static readonly OverlayType Tiberium6 = new OverlayType(11, "ti6", "TEXT_OVERLAY_TIBERIUM", OverlayTypeFlag.TiberiumOrGold, 11);
        public static readonly OverlayType Tiberium7 = new OverlayType(12, "ti7", "TEXT_OVERLAY_TIBERIUM", OverlayTypeFlag.TiberiumOrGold, 11);
        public static readonly OverlayType Tiberium8 = new OverlayType(13, "ti8", "TEXT_OVERLAY_TIBERIUM", OverlayTypeFlag.TiberiumOrGold, 11);
        public static readonly OverlayType Tiberium9 = new OverlayType(14, "ti9", "TEXT_OVERLAY_TIBERIUM", OverlayTypeFlag.TiberiumOrGold, 11);
        public static readonly OverlayType Tiberium10 = new OverlayType(15, "ti10", "TEXT_OVERLAY_TIBERIUM", OverlayTypeFlag.TiberiumOrGold, 11);
        public static readonly OverlayType Tiberium11 = new OverlayType(16, "ti11", "TEXT_OVERLAY_TIBERIUM", OverlayTypeFlag.TiberiumOrGold, 11);
        public static readonly OverlayType Tiberium12 = new OverlayType(17, "ti12", "TEXT_OVERLAY_TIBERIUM", OverlayTypeFlag.TiberiumOrGold, 11);
        public static readonly OverlayType Teleport = new OverlayType(18, "road", "TEXT_OVERLAY_TELEPORTER", 1);
        // Unusable in SS from ini since it has the same name as ROAD. Added here and forced to frame 1 because it is used in football fields.
        public static readonly OverlayType Road = new OverlayType(19, "road", "TEXT_OVERLAY_CONCRETE_ROAD", 1);
        // Not available to place down sadly: even the ini read for it in the game code only succeeds if 'IsGross' is enabled.
        //public static readonly OverlayType Squishy = new OverlayType(20, "SQUISH", OverlayTypeFlag.Decoration);
        public static readonly OverlayType V12 = new OverlayType(21, "v12", "TEXT_STRUCTURE_TITLE_CIV12B", OverlayTypeFlag.Solid);
        public static readonly OverlayType V13 = new OverlayType(22, "v13", "TEXT_STRUCTURE_TITLE_CIV12", OverlayTypeFlag.Solid);
        public static readonly OverlayType V14 = new OverlayType(23, "v14", "TEXT_STRUCTURE_TITLE_CIV13", OverlayTypeFlag.Solid);
        public static readonly OverlayType V15 = new OverlayType(24, "v15", "TEXT_STRUCTURE_TITLE_CIV14", OverlayTypeFlag.Solid);
        public static readonly OverlayType V16 = new OverlayType(25, "v16", "TEXT_STRUCTURE_TITLE_CIV15", OverlayTypeFlag.Solid);
        public static readonly OverlayType V17 = new OverlayType(26, "v17", "TEXT_STRUCTURE_TITLE_CIV16", OverlayTypeFlag.Solid);
        public static readonly OverlayType V18 = new OverlayType(27, "v18", "TEXT_STRUCTURE_TITLE_CIV17", OverlayTypeFlag.Solid);
        public static readonly OverlayType FlagSpot = new OverlayType(28, "fpls", "TEXT_CF_ONHOVER_SPOT", OverlayTypeFlag.Flag | OverlayTypeFlag.Pavement);
        // Not placeable in SS.
        //public static readonly OverlayType WoodCrate = new OverlayType(29, "wcrate", "Wooden Crate", OverlayTypeFlag.Crate);
        //public static readonly OverlayType SteelCrate = new OverlayType(30, "scrate", "Steel Crate", OverlayTypeFlag.Crate);
        //public static readonly OverlayType ArmageddonCrate = new OverlayType(31, "acrate", "Armageddon Crate", null, OverlayTypeFlag.Crate, "scrate", -1, Color.Red);
        //public static readonly OverlayType HealCrate = new OverlayType(32, "hcrate", "Heal Crate", null, OverlayTypeFlag.Crate, "scrate", -1, Color.Green);

        private static OverlayType[] Types;

        static OverlayTypes()
        {
            // ROAD is filtered out of this, but is available to the program for painting the football fields.
            Types =
                (from field in typeof(OverlayTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
                 where field.IsInitOnly && typeof(OverlayType).IsAssignableFrom(field.FieldType)
                 select field.GetValue(null) as OverlayType).Where(t => t.ID != 19).OrderBy(t => t.ID).ToArray();
        }

        public static IEnumerable<OverlayType> GetTypes()
        {
            return Types;
        }
    }
}
