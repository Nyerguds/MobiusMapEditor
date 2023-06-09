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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MobiusEditor.Model;

namespace MobiusEditor.TiberianDawn
{
    public static class UnitTypes
    {
        public static readonly UnitType HTank = new UnitType(0, "htnk", "TEXT_UNIT_TITLE_GDI_MAMMOTH_TANK", "Goodguy", UnitTypeFlag.IsArmed | UnitTypeFlag.HasTurret);
        public static readonly UnitType MTank = new UnitType(1, "mtnk", "TEXT_UNIT_TITLE_GDI_MED_TANK", "Goodguy", UnitTypeFlag.IsArmed | UnitTypeFlag.HasTurret);
        public static readonly UnitType LTank = new UnitType(2, "ltnk", "TEXT_UNIT_TITLE_NOD_LIGHT_TANK", "Badguy", UnitTypeFlag.IsArmed | UnitTypeFlag.HasTurret);
        public static readonly UnitType STank = new UnitType(3, "stnk", "TEXT_UNIT_TITLE_NOD_STEALTH_TANK", "Badguy", UnitTypeFlag.IsArmed);
        public static readonly UnitType FTank = new UnitType(4, "ftnk", "TEXT_UNIT_TITLE_NOD_FLAME_TANK", "Badguy", UnitTypeFlag.IsArmed);
        public static readonly UnitType Visceroid = new UnitType(5, "vice", "TEXT_UNIT_TITLE_VICE", "Special", UnitTypeFlag.IsArmed);
        public static readonly UnitType APC = new UnitType(6, "apc", "TEXT_UNIT_TITLE_GDI_APC", "Goodguy", UnitTypeFlag.IsArmed);
        public static readonly UnitType MLRS = new UnitType(7, "msam", "TEXT_UNIT_TITLE_GDI_MRLS", "Goodguy", Int32.MaxValue, 0, UnitTypeFlag.IsArmed | UnitTypeFlag.HasTurret);
        public static readonly UnitType Jeep = new UnitType(8, "jeep", "TEXT_UNIT_TITLE_GDI_HUMVEE", "Goodguy",0, -4, UnitTypeFlag.IsArmed | UnitTypeFlag.HasTurret);
        public static readonly UnitType Buggy = new UnitType(9, "bggy", "TEXT_UNIT_TITLE_NOD_NOD_BUGGY", "Badguy", 0, -4, UnitTypeFlag.IsArmed | UnitTypeFlag.HasTurret);
        public static readonly UnitType Harvester = new UnitType(10, "harv", "TEXT_UNIT_TITLE_GDI_HARVESTER", "Goodguy", UnitTypeFlag.IsHarvester);
        public static readonly UnitType Arty = new UnitType(11, "arty", "TEXT_UNIT_TITLE_NOD_ARTILLERY", "Badguy", UnitTypeFlag.IsArmed);
        public static readonly UnitType SSM = new UnitType(12, "mlrs", "TEXT_UNIT_TITLE_GDI_MLRS", "Badguy", Int32.MaxValue, 0, UnitTypeFlag.IsArmed | UnitTypeFlag.HasTurret);
        public static readonly UnitType Hover = new UnitType(13, "lst", "TEXT_UNIT_TITLE_LST", "Goodguy");
        public static readonly UnitType MHQ = new UnitType(14, "mhq", "TEXT_UNIT_TITLE_MHQ", "Goodguy", 0, -4, UnitTypeFlag.HasTurret);
        public static readonly UnitType GunBoat = new UnitType(15, "boat", "TEXT_UNIT_TITLE_WAKE", "Goodguy", UnitTypeFlag.IsArmed);
        public static readonly UnitType MCV = new UnitType(16, "mcv", "TEXT_UNIT_TITLE_GDI_MCV", "Goodguy");
        public static readonly UnitType Bike = new UnitType(17, "bike", "TEXT_UNIT_TITLE_NOD_RECON_BIKE", "Badguy", UnitTypeFlag.IsArmed);
        public static readonly UnitType Tric = new UnitType(18, "tric", "TEXT_UNIT_TITLE_TRIC", "Special", UnitTypeFlag.IsArmed);
        public static readonly UnitType Trex = new UnitType(19, "trex", "TEXT_UNIT_TITLE_TREX", "Special", UnitTypeFlag.IsArmed);
        public static readonly UnitType Rapt = new UnitType(20, "rapt", "TEXT_UNIT_TITLE_RAPT", "Special", UnitTypeFlag.IsArmed);
        public static readonly UnitType Steg = new UnitType(21, "steg", "TEXT_UNIT_TITLE_STEG", "Special", UnitTypeFlag.IsArmed);

        public static readonly UnitType Tran = new UnitType(0 | UnitTypeIDMask.Aircraft, "tran", "TEXT_UNIT_TITLE_GDI_TRANSPORT", "Goodguy", "LROTOR", "RROTOR", 1, -2, UnitTypeFlag.HasTurret | UnitTypeFlag.HasDoubleTurret);
        public static readonly UnitType A10 = new UnitType(1 | UnitTypeIDMask.Aircraft, "a10", "TEXT_UNIT_TITLE_A10", "Goodguy", UnitTypeFlag.IsArmed | UnitTypeFlag.IsFixedWing);
        public static readonly UnitType Heli = new UnitType(2 | UnitTypeIDMask.Aircraft, "heli", "TEXT_UNIT_TITLE_NOD_HELICOPTER", "Badguy", "LROTOR", null, 0, -2, UnitTypeFlag.IsArmed | UnitTypeFlag.HasTurret);
        public static readonly UnitType C17 = new UnitType(3 | UnitTypeIDMask.Aircraft, "c17", "TEXT_UNIT_TITLE_C17", "Badguy", UnitTypeFlag.IsFixedWing);
        public static readonly UnitType Orca = new UnitType(4 | UnitTypeIDMask.Aircraft, "orca", "TEXT_UNIT_TITLE_GDI_ORCA", "Goodguy", UnitTypeFlag.IsArmed);

        private static readonly UnitType[] Types;

        static UnitTypes()
        {
            Types =
                (from field in typeof(UnitTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
                 where field.IsInitOnly && typeof(UnitType).IsAssignableFrom(field.FieldType)
                 select field.GetValue(null) as UnitType).ToArray();
        }

        public static IEnumerable<UnitType> GetTypes(Boolean withoutAircraft)
        {
            // only return placeable units; you can't place down aircraft in C&C
            if (withoutAircraft)
                return Types.Where(t => !t.IsAircraft);
            return Types;
        }
    }
}
