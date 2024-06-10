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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MobiusEditor.Model;

namespace MobiusEditor.TiberianDawn
{
    public static class UnitTypes
    {
        public static readonly UnitType HTank = new VehicleType(0, "htnk", "TEXT_UNIT_TITLE_GDI_MAMMOTH_TANK", "Goodguy", FrameUsage.Frames32Full, FrameUsage.Frames32Full, UnitTypeFlag.IsArmed | UnitTypeFlag.HasTurret);
        public static readonly UnitType MTank = new VehicleType(1, "mtnk", "TEXT_UNIT_TITLE_GDI_MED_TANK", "Goodguy", FrameUsage.Frames32Full, FrameUsage.Frames32Full, UnitTypeFlag.IsArmed | UnitTypeFlag.HasTurret);
        public static readonly UnitType LTank = new VehicleType(2, "ltnk", "TEXT_UNIT_TITLE_NOD_LIGHT_TANK", "Badguy", FrameUsage.Frames32Full, FrameUsage.Frames32Full, UnitTypeFlag.IsArmed | UnitTypeFlag.HasTurret);
        public static readonly UnitType STank = new VehicleType(3, "stnk", "TEXT_UNIT_TITLE_NOD_STEALTH_TANK", "Badguy", FrameUsage.Frames32Full, UnitTypeFlag.IsArmed);
        public static readonly UnitType FTank = new VehicleType(4, "ftnk", "TEXT_UNIT_TITLE_NOD_FLAME_TANK", "Badguy", FrameUsage.Frames32Full, UnitTypeFlag.IsArmed);
        public static readonly UnitType Visceroid = new VehicleType(5, "vice", "TEXT_UNIT_TITLE_VICE", "Special", FrameUsage.Frames01Single, UnitTypeFlag.IsArmed);
        public static readonly UnitType APC = new VehicleType(6, "apc", "TEXT_UNIT_TITLE_GDI_APC", "Goodguy", FrameUsage.Frames32Full | FrameUsage.HasUnloadFrames, UnitTypeFlag.IsArmed);
        public static readonly UnitType MLRS = new VehicleType(7, "msam", "TEXT_UNIT_TITLE_GDI_MRLS", "Goodguy", FrameUsage.Frames32Full, FrameUsage.Frames32Full | FrameUsage.OnFlatBed, 0, 0, UnitTypeFlag.IsArmed | UnitTypeFlag.HasTurret);
        public static readonly UnitType Jeep = new VehicleType(8, "jeep", "TEXT_UNIT_TITLE_GDI_HUMVEE", "Goodguy", FrameUsage.Frames32Full, FrameUsage.Frames32Full, 0, -4, UnitTypeFlag.IsArmed | UnitTypeFlag.HasTurret);
        public static readonly UnitType Buggy = new VehicleType(9, "bggy", "TEXT_UNIT_TITLE_NOD_NOD_BUGGY", "Badguy", FrameUsage.Frames32Full, FrameUsage.Frames32Full, 0, -4, UnitTypeFlag.IsArmed | UnitTypeFlag.HasTurret);
        public static readonly UnitType Harvester = new VehicleType(10, "harv", "TEXT_UNIT_TITLE_GDI_HARVESTER", "Goodguy", FrameUsage.Frames32Full, UnitTypeFlag.IsHarvester);
        public static readonly UnitType Arty = new VehicleType(11, "arty", "TEXT_UNIT_TITLE_NOD_ARTILLERY", "Badguy", FrameUsage.Frames32Full, UnitTypeFlag.IsArmed);
        public static readonly UnitType SSM = new VehicleType(12, "mlrs", "TEXT_UNIT_TITLE_GDI_MLRS", "Badguy", FrameUsage.Frames32Full, FrameUsage.Frames32Full | FrameUsage.OnFlatBed, 0, 0, UnitTypeFlag.IsArmed | UnitTypeFlag.HasTurret);
        public static readonly UnitType Hover = new VehicleType(13, "lst", "TEXT_UNIT_TITLE_LST", "Goodguy", FrameUsage.Frames01Single);
        public static readonly UnitType MHQ = new VehicleType(14, "mhq", "TEXT_UNIT_TITLE_MHQ", "Goodguy", FrameUsage.Frames32Full, FrameUsage.Frames32Full, 0, -4, UnitTypeFlag.HasTurret);
        public static readonly UnitType GunBoat = new VehicleType(15, "boat", "TEXT_UNIT_TITLE_WAKE", "Goodguy", FrameUsage.Frames32Full | FrameUsage.DamageStates, FrameUsage.Frames32Full, UnitTypeFlag.IsArmed);
        public static readonly UnitType MCV = new VehicleType(16, "mcv", "TEXT_UNIT_TITLE_GDI_MCV", "Goodguy", FrameUsage.Frames32Full);
        public static readonly UnitType Bike = new VehicleType(17, "bike", "TEXT_UNIT_TITLE_NOD_RECON_BIKE", "Badguy", FrameUsage.Frames32Full, UnitTypeFlag.IsArmed);
        public static readonly UnitType Tric = new VehicleType(18, "tric", "TEXT_UNIT_TITLE_TRIC", "Special", FrameUsage.Frames08Cardinal, UnitTypeFlag.IsArmed | UnitTypeFlag.NoRemap);
        public static readonly UnitType Trex = new VehicleType(19, "trex", "TEXT_UNIT_TITLE_TREX", "Special", FrameUsage.Frames08Cardinal, UnitTypeFlag.IsArmed | UnitTypeFlag.NoRemap);
        public static readonly UnitType Rapt = new VehicleType(20, "rapt", "TEXT_UNIT_TITLE_RAPT", "Special", FrameUsage.Frames08Cardinal, UnitTypeFlag.IsArmed | UnitTypeFlag.NoRemap);
        public static readonly UnitType Steg = new VehicleType(21, "steg", "TEXT_UNIT_TITLE_STEG", "Special", FrameUsage.Frames08Cardinal, UnitTypeFlag.IsArmed | UnitTypeFlag.NoRemap);

        public static readonly UnitType Tran = new AircraftType(0, "tran", "TEXT_UNIT_TITLE_GDI_TRANSPORT", "Goodguy", FrameUsage.Frames32Full | FrameUsage.HasUnloadFrames, FrameUsage.Rotor, "LROTOR", "RROTOR", 1, -2, UnitTypeFlag.HasTurret | UnitTypeFlag.HasDoubleTurret);
        public static readonly UnitType A10 = new AircraftType(1, "a10", "TEXT_UNIT_TITLE_A10", "Goodguy", FrameUsage.Frames32Full, UnitTypeFlag.IsArmed | UnitTypeFlag.IsFixedWing);
        public static readonly UnitType Heli = new AircraftType(2, "heli", "TEXT_UNIT_TITLE_NOD_HELICOPTER", "Badguy", FrameUsage.Frames32Full, FrameUsage.Rotor, "LROTOR", null, 0, -2, UnitTypeFlag.IsArmed | UnitTypeFlag.HasTurret);
        public static readonly UnitType C17 = new AircraftType(3, "c17", "TEXT_UNIT_TITLE_C17", "Badguy", FrameUsage.Frames32Full, UnitTypeFlag.IsFixedWing);
        public static readonly UnitType Orca = new AircraftType(4, "orca", "TEXT_UNIT_TITLE_GDI_ORCA", "Goodguy", FrameUsage.Frames32Full, UnitTypeFlag.IsArmed);

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
