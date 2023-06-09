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
using System.Collections.Generic;

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

        private static readonly string[] Types = new string[]
        {
            // Nyerguds upgrade: Removed irrelevant types for preplaced units.
            // Note that TeamTypes use a separate list, defined in the TeamMissionTypes class.
            MISSION_SLEEP,
            //MISSION_ATTACK,
            //MISSION_MOVE,
            //MISSION_QMOVE,
            //MISSION_RETREAT,
            MISSION_STICKY,
            MISSION_GUARD,
            //MISSION_ENTER,
            //MISSION_CAPTURE,
            MISSION_HARVEST,
            MISSION_AREAGUARD,
            MISSION_RETURN,
            MISSION_STOP,
            MISSION_AMBUSH,
            MISSION_HUNT,
            MISSION_UNLOAD,
            //MISSION_SABOTAGE,
            //MISSION_CONSTRUCTION,
            //MISSION_SELLING,
            //MISSION_REPAIR,
            //MISSION_RESCUE,
            //MISSION_MISSILE,
            MISSION_HARMLESS,
        };

        public static IEnumerable<string> GetTypes()
        {
            return Types;
        }
    }
}
