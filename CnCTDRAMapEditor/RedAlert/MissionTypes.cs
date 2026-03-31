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

namespace MobiusEditor.RedAlert
{
    public static class MissionTypes
    {
        public const string MISSION_SLEEP = "Sleep";
        public const string MISSION_ATTACK = "Attack";
        public const string MISSION_MOVE = "Move";
        public const string MISSION_QMOVE = "QMove";
        public const string MISSION_RETREAT = "Retreat";
        public const string MISSION_STICKY = "Sticky";
        public const string MISSION_GUARD = "Guard";
        public const string MISSION_ENTER = "Enter";
        public const string MISSION_CAPTURE = "Capture";
        public const string MISSION_HARVEST = "Harvest";
        public const string MISSION_AREAGUARD = "Area Guard";
        public const string MISSION_RETURN = "Return";
        public const string MISSION_STOP = "Stop";
        public const string MISSION_AMBUSH = "Ambush";
        public const string MISSION_HUNT = "Hunt";
        public const string MISSION_UNLOAD = "Unload";
        public const string MISSION_SABOTAGE = "Sabotage";
        public const string MISSION_CONSTRUCTION = "Construction";
        public const string MISSION_SELLING = "Selling";
        public const string MISSION_REPAIR = "Repair";
        public const string MISSION_RESCUE = "Rescue";
        public const string MISSION_MISSILE = "Missile";
        public const string MISSION_HARMLESS = "Harmless";

        private static readonly (string, bool)[] AllTypes = new (string, bool)[]
        {
            // Nyerguds upgrade: filter out types that are irrelevant for preplaced units.
            // Note that TeamTypes use a separate list, defined in the TeamMissionTypes class.
            (MISSION_SLEEP, true),
            (MISSION_ATTACK, false),
            (MISSION_MOVE, false),
            (MISSION_QMOVE, false),
            (MISSION_RETREAT, false),
            (MISSION_STICKY, true),
            (MISSION_GUARD, true),
            (MISSION_ENTER, false),
            (MISSION_CAPTURE, false),
            (MISSION_HARVEST, true),
            (MISSION_AREAGUARD, true),
            (MISSION_RETURN, true),
            (MISSION_STOP, true),
            (MISSION_AMBUSH, true),
            (MISSION_HUNT, true),
            (MISSION_UNLOAD, true),
            (MISSION_SABOTAGE, false),
            (MISSION_CONSTRUCTION, false),
            (MISSION_SELLING, false),
            (MISSION_REPAIR, false),
            (MISSION_RESCUE, false),
            (MISSION_MISSILE, false),
            (MISSION_HARMLESS, true),
        };

        private static readonly string[] UsableTypes;
        private static readonly string[] UnassignableTypes;

        static MissionTypes()
        {
            UsableTypes = AllTypes.Where(itm => itm.Item2).Select(itm => itm.Item1).ToArray();
            UnassignableTypes = AllTypes.Where(itm => !itm.Item2).Select(itm => itm.Item1).ToArray();
        }

        public static IEnumerable<string> GetTypes()
        {
            return UsableTypes.ToArray();
        }

        public static IEnumerable<string> GetUnassignableTypes()
        {
            return UnassignableTypes.ToArray();
        }
    }
}
