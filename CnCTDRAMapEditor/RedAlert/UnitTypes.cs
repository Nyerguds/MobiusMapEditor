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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MobiusEditor.Model;

namespace MobiusEditor.RedAlert
{
    public static class UnitTypes
    {
        public static readonly UnitType HTank = new VehicleType(0, "4tnk", "TEXT_UNIT_RA_4TNK", "USSR", FrameUsage.Frames32Full, FrameUsage.Frames32Full, UnitTypeFlag.Armed | UnitTypeFlag.Turret);
        public static readonly UnitType MTank = new VehicleType(1, "3tnk", "TEXT_UNIT_RA_3TNK", "USSR", FrameUsage.Frames32Full, FrameUsage.Frames32Full, UnitTypeFlag.Armed | UnitTypeFlag.Turret);
        public static readonly UnitType MTank2 = new VehicleType(2, "2tnk", "TEXT_UNIT_RA_2TNK", "Greece", FrameUsage.Frames32Full, FrameUsage.Frames32Full, UnitTypeFlag.Armed | UnitTypeFlag.Turret);
        public static readonly UnitType LTank = new VehicleType(3, "1tnk", "TEXT_UNIT_RA_1TNK", "Greece", FrameUsage.Frames32Full, FrameUsage.Frames32Full, UnitTypeFlag.Armed | UnitTypeFlag.Turret);
        public static readonly UnitType APC = new VehicleType(4, "apc", "TEXT_UNIT_RA_APC", "Greece", FrameUsage.Frames32Full | FrameUsage.HasUnloadFrames, UnitTypeFlag.Armed);
        public static readonly UnitType MineLayer = new VehicleType(5, "mnly", "TEXT_UNIT_RA_MNLY", "Greece", FrameUsage.Frames32Full);
        public static readonly UnitType Jeep = new VehicleType(6, "jeep", "TEXT_UNIT_RA_JEEP", "Greece", FrameUsage.Frames32Full, FrameUsage.Frames32Full, 0, -4, UnitTypeFlag.Armed | UnitTypeFlag.Turret);
        public static readonly UnitType Harvester = new VehicleType(7, "harv", "TEXT_UNIT_RA_HARV", "Greece", FrameUsage.Frames32Full, UnitTypeFlag.Harvester);
        public static readonly UnitType Arty = new VehicleType(8, "arty", "TEXT_UNIT_RA_ARTY", "Greece", FrameUsage.Frames32Full, UnitTypeFlag.Armed);
        public static readonly UnitType MRJammer = new VehicleType(9, "mrj", "TEXT_UNIT_RA_MRJ", "Greece", FrameUsage.Frames32Full, FrameUsage.Frames32Full, 0, -5, UnitTypeFlag.Turret | UnitTypeFlag.Jammer);
        public static readonly UnitType MGG = new VehicleType(10, "mgg", "TEXT_UNIT_RA_MGG", "Greece", FrameUsage.Frames32Full, FrameUsage.Frames16Symmetrical | FrameUsage.OnFlatBed, 0, 0, UnitTypeFlag.Turret | UnitTypeFlag.GapGenerator);
        public static readonly UnitType MCV = new VehicleType(11, "mcv", "TEXT_UNIT_RA_MCV", "Greece", FrameUsage.Frames32Full);
        public static readonly UnitType V2Launcher = new VehicleType(12, "v2rl", "TEXT_UNIT_RA_V2RL", "USSR", FrameUsage.Frames32Full, UnitTypeFlag.Armed);
        public static readonly UnitType ConvoyTruck = new VehicleType(13, "truk", "TEXT_UNIT_RA_TRUK", "Greece", FrameUsage.Frames32Full);
        public static readonly UnitType Ant1 = new VehicleType(14, "ant1", "TEXT_UNIT_RA_ANT1", "USSR", FrameUsage.Frames08Cardinal, UnitTypeFlag.Armed | UnitTypeFlag.NoRules);
        public static readonly UnitType Ant2 = new VehicleType(15, "ant2", "TEXT_UNIT_RA_ANT2", "Ukraine", FrameUsage.Frames08Cardinal, UnitTypeFlag.Armed | UnitTypeFlag.NoRules);
        public static readonly UnitType Ant3 = new VehicleType(16, "ant3", "TEXT_UNIT_RA_ANT3", "Germany", FrameUsage.Frames08Cardinal, UnitTypeFlag.Armed | UnitTypeFlag.NoRules);
        public static readonly UnitType Chrono = new VehicleType(17, "ctnk", "TEXT_UNIT_RA_CTNK", "Greece", FrameUsage.Frames32Full, UnitTypeFlag.Armed | UnitTypeFlag.ExpansionOnly);
        public static readonly UnitType Tesla = new VehicleType(18, "ttnk", "TEXT_UNIT_RA_TTNK", "USSR", FrameUsage.Frames32Full, FrameUsage.Frames01Single, UnitTypeFlag.Armed | UnitTypeFlag.Turret | UnitTypeFlag.ExpansionOnly | UnitTypeFlag.Jammer);
        public static readonly UnitType MAD = new VehicleType(19, "qtnk", "TEXT_UNIT_RA_QTNK", "USSR", FrameUsage.Frames32Full, UnitTypeFlag.Armed | UnitTypeFlag.ExpansionOnly);
        public static readonly UnitType DemoTruck = new VehicleType(20, "dtrk", "TEXT_UNIT_RA_DTRK", "USSR", FrameUsage.Frames32Full, UnitTypeFlag.Armed | UnitTypeFlag.ExpansionOnly);
        public static readonly UnitType Phase = new VehicleType(21, "stnk", "TEXT_UNIT_RA_STNK", "Greece", FrameUsage.Frames32Full | FrameUsage.HasUnloadFrames, FrameUsage.Frames32Full, UnitTypeFlag.Armed | UnitTypeFlag.Turret | UnitTypeFlag.ExpansionOnly);

        public static readonly UnitType Tran = new AircraftType(0, "tran", "TEXT_UNIT_RA_TRAN", "USSR", FrameUsage.Frames32Full | FrameUsage.HasUnloadFrames, FrameUsage.Rotor, "lrotor", "rrotor", 1, -2, UnitTypeFlag.Turret | UnitTypeFlag.DoubleTurret);
        public static readonly UnitType Badr = new AircraftType(1, "badr", "TEXT_UNIT_RA_BADR", "USSR", FrameUsage.Frames16Simple, UnitTypeFlag.FixedWing);
        public static readonly UnitType U2 = new AircraftType(2, "u2", "TEXT_UNIT_RA_U2", "USSR", FrameUsage.Frames16Simple, UnitTypeFlag.FixedWing);
        public static readonly UnitType Mig = new AircraftType(3, "mig", "TEXT_UNIT_RA_MIG", "USSR", FrameUsage.Frames16Simple, UnitTypeFlag.FixedWing | UnitTypeFlag.Armed);
        public static readonly UnitType Yak = new AircraftType(4, "yak", "TEXT_UNIT_RA_YAK", "USSR", FrameUsage.Frames16Simple, UnitTypeFlag.FixedWing | UnitTypeFlag.Armed);
        public static readonly UnitType Heli = new AircraftType(5, "heli", "TEXT_UNIT_RA_HELI", "Greece", FrameUsage.Frames32Full, FrameUsage.Rotor, "lrotor", null, 0, -2, UnitTypeFlag.Armed | UnitTypeFlag.Turret);
        public static readonly UnitType Hind = new AircraftType(6, "hind", "TEXT_UNIT_RA_HIND", "USSR", FrameUsage.Frames32Full, FrameUsage.Rotor, "lrotor", null, 0, -2, UnitTypeFlag.Armed | UnitTypeFlag.Turret);

        public static readonly UnitType Submarine = new VesselType(0, "ss", "TEXT_UNIT_RA_SS", "USSR", FrameUsage.Frames16Simple, UnitTypeFlag.Armed);
        public static readonly UnitType Destroyer = new VesselType(1, "dd", "TEXT_UNIT_RA_DD", "Greece", FrameUsage.Frames16Simple, FrameUsage.Frames32Full, "ssam", null, -8, -4, UnitTypeFlag.Armed | UnitTypeFlag.Turret);
        public static readonly UnitType Cruiser = new VesselType(2, "ca", "TEXT_UNIT_RA_CA", "Greece", FrameUsage.Frames16Simple, FrameUsage.Frames32Full, "turr", "turr", 22, -4, UnitTypeFlag.Armed | UnitTypeFlag.Turret | UnitTypeFlag.DoubleTurret);
        public static readonly UnitType Transport = new VesselType(3, "lst", "TEXT_UNIT_RA_LST", "Greece", FrameUsage.Frames01Single | FrameUsage.HasUnloadFrames);
        public static readonly UnitType PTBoat = new VesselType(4, "pt", "TEXT_UNIT_RA_PT", "Greece", FrameUsage.Frames16Simple, FrameUsage.Frames32Full, "mgun", null, 14, 1, UnitTypeFlag.Armed | UnitTypeFlag.Turret);
        public static readonly UnitType MissileSubmarine = new VesselType(5, "msub", "TEXT_UNIT_RA_MSUB", "USSR", FrameUsage.Frames16Simple, UnitTypeFlag.Armed | UnitTypeFlag.ExpansionOnly);
        public static readonly UnitType Carrier = new VesselType(6, "carr", "TEXT_UNIT_RA_CARR", "Greece", FrameUsage.Frames01Single, UnitTypeFlag.ExpansionOnly);

        private static readonly UnitType[] Types;

        static UnitTypes()
        {
            Types =
                (from field in typeof(UnitTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
                 where field.IsInitOnly && typeof(UnitType).IsAssignableFrom(field.FieldType)
                 select field.GetValue(null) as UnitType).ToArray();
        }

        public static IEnumerable<UnitType> GetTypes(bool withoutAircraft)
        {
            // only return placeable units, not aircraft.
            if (withoutAircraft)
                return Types.Where(t => !t.IsAircraft);
            return Types;
        }
    }
}
