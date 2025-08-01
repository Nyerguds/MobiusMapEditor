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

        static EventTypes()
        {
            Types =
                (from field in typeof(EventTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
                 where field.IsLiteral && !field.IsInitOnly && typeof(string).IsAssignableFrom(field.FieldType)
                 select field.GetValue(null) as string).ToArray();
            Dictionary<string, string> typesInfo = new Dictionary<string, string>()
            {
                { EVENT_NONE, "" },
                { EVENT_PLAYER_ENTERED, "Cell entered, or building captured." },
                { EVENT_DISCOVERED, "Map object is discovered by the player." },
                { EVENT_ATTACKED, "Map object is attacked by anybody.\nIf a House is set, it acts as \"any building of this House is attacked\",\nbut this system only works for the player House." },
                { EVENT_DESTROYED, "Map object is destroyed.\nThis event should never have a House set." },
                { EVENT_ANY, "Any action at all." },
                { EVENT_HOUSE_DISCOVERED, "Anything from this House is discovered by the player." },
                { EVENT_UNITS_DESTROYED, "All units of this House are destroyed." },
                { EVENT_BUILDINGS_DESTROYED, "All buildings of this House are destroyed." },
                { EVENT_ALL_DESTROYED, "Everything owned by this House is destroyed." },
                { EVENT_CREDITS, "This House reached the set amount of credits\nCash only; harvested tiberium does not count." },
                { EVENT_TIME, "Time in 1/10th min." },
                { EVENT_NBUILDINGS_DESTROYED, "Amount of buildings of this House destroyed." },
                { EVENT_NUNITS_DESTROYED, "Amount of units of this House destroyed." },
                { EVENT_NOFACTORIES, "No vehicle or infantry production facilities remain for this House." },
                { EVENT_EVAC_CIVILIAN, "Civilian is evacuated by Chinook helicopter." },
                { EVENT_BUILD, "This building was constructed by the player." },
            };
            TypesInfo = new ReadOnlyDictionary<string, string>(typesInfo);
        }

        public static IEnumerable<string> GetTypes()
        {
            return Types;
        }

    }
}
