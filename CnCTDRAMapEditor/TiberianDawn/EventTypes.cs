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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace MobiusEditor.TiberianDawn
{
    public static class EventTypes
    {
        public const string EVENT_NONE                 /**/ = "None";
        public const string EVENT_PLAYER_ENTERED       /**/ = "Player Enters";
        public const string EVENT_DISCOVERED           /**/ = "Discovered";
        public const string EVENT_ATTACKED             /**/ = "Attacked";
        public const string EVENT_DESTROYED            /**/ = "Destroyed";
        public const string EVENT_ANY                  /**/ = "Any";
        public const string EVENT_HOUSE_DISCOVERED     /**/ = "House Discov.";
        public const string EVENT_UNITS_DESTROYED      /**/ = "Units Destr.";
        public const string EVENT_BUILDINGS_DESTROYED  /**/ = "Bldgs Destr.";
        public const string EVENT_ALL_DESTROYED        /**/ = "All Destr.";
        public const string EVENT_CREDITS              /**/ = "Credits";
        public const string EVENT_TIME                 /**/ = "Time";
        public const string EVENT_NBUILDINGS_DESTROYED /**/ = "# Bldgs Dstr.";
        public const string EVENT_NUNITS_DESTROYED     /**/ = "# Units Dstr.";
        public const string EVENT_NOFACTORIES          /**/ = "No Factories";
        public const string EVENT_EVAC_CIVILIAN        /**/ = "Civ. Evac.";
        public const string EVENT_BUILD                /**/ = "Built It";

        private static readonly string[] Types;
        public static ReadOnlyDictionary<string, string> TypesInfo { get; }
        public static ReadOnlyDictionary<string, string> TypesDescription { get; }

        static EventTypes()
        {
            Types =
                (from field in typeof(EventTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
                 where field.IsLiteral && !field.IsInitOnly && typeof(string).IsAssignableFrom(field.FieldType)
                 select field.GetValue(null) as string).ToArray();
            Dictionary<string, string> typesInfo = new Dictionary<string, string>()
            {
                // Max length indicator
                //  "The initial Event description can have this length." +
                //  "\nFurther info on the Event has a line break in front." +
                { EVENT_NONE,
                    "Nothing. This event will never fire." },
                { EVENT_PLAYER_ENTERED,
                    "Celltrigger entered, or map object captured." },
                { EVENT_DISCOVERED,
                    "The linked map object is discovered by the player." },
                { EVENT_ATTACKED,
                    "Map object is attacked by anybody." },
                { EVENT_DESTROYED,
                    "Map object is destroyed." },
                { EVENT_ANY,
                    "Any action at all." },
                { EVENT_HOUSE_DISCOVERED,
                    "Anything from this House is discovered by the player." },
                { EVENT_UNITS_DESTROYED,
                    "All units of this House are destroyed." },
                { EVENT_BUILDINGS_DESTROYED,
                    "All buildings of this House are destroyed." },
                { EVENT_ALL_DESTROYED,
                    "Everything owned by this House is destroyed." },
                { EVENT_CREDITS,
                    "This House reached the set amount of credits." },
                { EVENT_TIME,
                    "Time in 1/10th min." },
                { EVENT_NBUILDINGS_DESTROYED,
                    "Amount of buildings of this House destroyed." },
                { EVENT_NUNITS_DESTROYED,
                    "Amount of units of this House destroyed." },
                { EVENT_NOFACTORIES,
                    "No vehicle or infantry production facilities" +
                    "\nremain for this House." },
                { EVENT_EVAC_CIVILIAN,
                    "Civilian is evacuated by Chinook helicopter." },
                { EVENT_BUILD,
                    "This building type was constructed by the player." },
            };
            TypesInfo = new ReadOnlyDictionary<string, string>(typesInfo);
            Dictionary<string, string> typesDescription = new Dictionary<string, string>()
            {
                // Max length indicator
                //  "The initial Event description can have this length." +
                //  "\nFurther info on the Event has a line break in front." +
                { EVENT_NONE, null },
                { EVENT_PLAYER_ENTERED,
                    "Cells only trigger when the entering unit belongs to" +
                    "\nthe trigger's House. Capturing triggers on any House." },
                { EVENT_DISCOVERED,
                    "This cannot be used on player owned objects. To" +
                    "\ndetect reactivating a shrouded base, use \"Built It\"." },
                { EVENT_ATTACKED,
                    "If a House is set, it acts as \"any building of this" +
                    "\nHouse is attacked\", hovever, this system only works" +
                    "\nfor the player House." },
                { EVENT_DESTROYED,
                    "This event should never have a House set." },
                { EVENT_ANY,
                    "Normally only used for Action \"Cap=Win/Des=Lose\"" },
                { EVENT_HOUSE_DISCOVERED, null },
                { EVENT_UNITS_DESTROYED,
                    "This check excludes Gunboats, Transport Helicopters," +
                    "\ndelivery planes, and airstrike planes." },
                { EVENT_BUILDINGS_DESTROYED, null },
                { EVENT_ALL_DESTROYED,
                    "This check excludes Gunboats, Transport Helicopters," +
                    "\ndelivery planes, and airstrike planes." },
                { EVENT_CREDITS,
                    "Cash only; harvested tiberium does not count." },
                { EVENT_TIME,
                    "This is obviously influenced by the set game speed." +
                    "\nIt corresponds to Moderate game speed on the Remaster," +
                    "\nor one tick below half on the original game's slider." },
                { EVENT_NBUILDINGS_DESTROYED, null },
                { EVENT_NUNITS_DESTROYED, null },
                { EVENT_NOFACTORIES, null },
                { EVENT_EVAC_CIVILIAN, null },
                { EVENT_BUILD,
                    "Capturing a building of this type does not count." +
                    "\nDiscovering a player-owned one in the shroud does." },
            };
            TypesDescription = new ReadOnlyDictionary<string, string>(typesDescription);
        }

        public static IEnumerable<string> GetTypes()
        {
            return Types;
        }

    }
}
