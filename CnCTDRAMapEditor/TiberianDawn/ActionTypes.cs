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
    public static class ActionTypes
    {
        public const string ACTION_NONE             /**/ = "None";
        public const string ACTION_WIN              /**/ = "Win";
        public const string ACTION_LOSE             /**/ = "Lose";
        public const string ACTION_BEGIN_PRODUCTION /**/ = "Production";
        public const string ACTION_CREATE_TEAM      /**/ = "Create Team";
        public const string ACTION_DESTROY_TEAM     /**/ = "Dstry Teams";
        public const string ACTION_ALL_HUNT         /**/ = "All to Hunt";
        public const string ACTION_REINFORCEMENTS   /**/ = "Reinforce.";
        public const string ACTION_DZ               /**/ = "DZ at 'Z'";
        public const string ACTION_AIRSTRIKE        /**/ = "Airstrike";
        public const string ACTION_NUKE             /**/ = "Nuclear Missile";
        public const string ACTION_ION              /**/ = "Ion Cannon";
        public const string ACTION_DESTROY_XXXX     /**/ = "Dstry Trig 'XXXX'";
        public const string ACTION_DESTROY_YYYY     /**/ = "Dstry Trig 'YYYY'";
        public const string ACTION_DESTROY_ZZZZ     /**/ = "Dstry Trig 'ZZZZ'";
        public const string ACTION_DESTROY_UUUU     /**/ = "Dstry Trig 'UUUU'";
        public const string ACTION_DESTROY_VVVV     /**/ = "Dstry Trig 'VVVV'";
        public const string ACTION_DESTROY_WWWW     /**/ = "Dstry Trig 'WWWW'";
        public const string ACTION_AUTOCREATE       /**/ = "Autocreate";
        public const string ACTION_WINLOSE          /**/ = "Cap=Win/Des=Lose";
        public const string ACTION_ALLOWWIN         /**/ = "Allow Win";

        private static readonly string[] Types;
        public static ReadOnlyDictionary<string, string> TypesInfo { get; }
        public static ReadOnlyDictionary<string, string> TypesDescription { get; }

        static ActionTypes()
        {
            List<string> types=
                (from field in typeof(ActionTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
                 where field.IsLiteral && !field.IsInitOnly && typeof(string).IsAssignableFrom(field.FieldType)
                 select field.GetValue(null) as string).ToList();
            if (!Globals.EnableTd106Scripting)
            {
                types.Remove(ACTION_DESTROY_UUUU);
                types.Remove(ACTION_DESTROY_VVVV);
                types.Remove(ACTION_DESTROY_WWWW);
            }
            Types = types.ToArray();
            Dictionary<string, string> typesInfo = new Dictionary<string, string>()
            {
                // Max length indicator
                //  "The initial Action description can have this length" +
                //  "\nFurther info on the Action has a line break in front" +
                { ACTION_NONE,
                    "Nothing." },
                { ACTION_WIN,
                    "The player is flagged to win." },
                { ACTION_LOSE,
                    "The Player loses." },
                { ACTION_BEGIN_PRODUCTION,
                    "The affected House starts making units and buildings."  },
                { ACTION_CREATE_TEAM,
                    "A Team of the linked Teamtype is created." },
                { ACTION_DESTROY_TEAM,
                    "Disband all Teams of the linked Teamtype." },
                { ACTION_ALL_HUNT,
                    "All non-player owned ground units go to 'Hunt' mode." },
                { ACTION_REINFORCEMENTS,
                    "A Team of the linked Teamtype arrives on the map." },
                { ACTION_DZ,
                    "Drop Zone at waypoint 'Z'." },
                { ACTION_AIRSTRIKE,
                    "Gives permanent Airstrike ability." },
                { ACTION_NUKE,
                    "Enables a one-time nuclear strike for BadGuy, and" +
                    "\nimmediately charges it. Requires a Temple to fire." },
                { ACTION_ION,
                    "Enables a one-time Ion Cannon strike for GoodGuy," +
                    "\nand immediately charges it." },
                { ACTION_DESTROY_XXXX,
                    "Destroys the Trigger named \"XXXX\"." },
                { ACTION_DESTROY_YYYY,
                    "Destroys the Trigger named \"YYYY\"." },
                { ACTION_DESTROY_ZZZZ,
                    "Destroys the Trigger named \"ZZZZ\"." },
                { ACTION_DESTROY_UUUU,
                    "Destroys the Trigger named \"UUUU\"." },
                { ACTION_DESTROY_VVVV,
                    "Destroys the Trigger named \"VVVV\"." },
                { ACTION_DESTROY_WWWW,
                    "Destroys the Trigger named \"WWWW\"." },
                { ACTION_AUTOCREATE,
                    "Randomly start creating \"Autocreate\" Teamtypes." },
                { ACTION_WINLOSE,
                    "If the object is destroyed, the player loses." +
                    "\n If it is captured by any House, the player wins." },
                { ACTION_ALLOWWIN,
                    "Only allow the House to win after this is triggered." +
                    "\nHas no effect if a non-player House is set." },
            };
            TypesInfo = new ReadOnlyDictionary<string, string>(typesInfo);
            Dictionary<string, string> typesDescription = new Dictionary<string, string>()
            {
                // Max length indicator
                //  "The initial Action description can have this length" +
                //  "\nFurther info on the Action has a line break in front" +
                { ACTION_NONE,
                    "No action will occur." },
                { ACTION_WIN,
                    "They will win after any \"Allow Win\" blockages are" +
                    "\ncleared." },
                { ACTION_LOSE, null },
                { ACTION_BEGIN_PRODUCTION,
                    "If the source of the action is a cell, this affects" +
                    "\nthe \"classic opposing House\" (if House is GoodGuy," +
                    "\nthen it affects BadGuy, if not, GoodGuy). Otherwise," +
                    "\nthe House set in the trigger is affected." },
                { ACTION_CREATE_TEAM,
                    "This does not build units. Teams are created from" +
                    "\nunits available on the map. Whether units from a" +
                    "\nteam are built depends on the Teamtype's settings." },
                { ACTION_DESTROY_TEAM,
                    "This does not destroy any units. It just frees up" +
                    "\nthe units in these Teams to go do other stuff." },
                { ACTION_ALL_HUNT,
                    "This disbands all teams these units might be in." },
                { ACTION_REINFORCEMENTS,
                    "The point at which the units appear depends on the" +
                    "\nEdge set on the Teamtype's House, and any blocked" +
                    "\narea along that edge just outside the usable map." },
                { ACTION_DZ,
                    "An area with a radius of 4 cells around Waypoint 25" +
                    "\nis revealed to the Player, and a flare is shown. The" +
                    "\nflare is not shown when placed on a building." },
                { ACTION_AIRSTRIKE,
                    "Normally this is given to the player, but from cell," +
                    "\nit instead goes to the House that triggered it." },
                { ACTION_NUKE,
                    "Human players only get a nuke once, unless they" +
                    "\ncollect extra steel crates to unlock more. Each nuke" +
                    "\ncosts three steel crates to unlock." },
                { ACTION_ION,
                    "Computer Houses can fire this without owning an" +
                    "\nAdvanced Communications Center. Human players" +
                    "\nneed to own one for this Action to function correctly." },
                { ACTION_DESTROY_XXXX,
                    "Removes the trigger from the game entirely, ensuring" +
                    "\nit will never activate." },
                { ACTION_DESTROY_YYYY,
                    "Removes the trigger from the game entirely, ensuring" +
                    "\nit will never activate." },
                { ACTION_DESTROY_ZZZZ,
                    "Removes the trigger from the game entirely, ensuring" +
                    "\nit will never activate." },
                { ACTION_DESTROY_UUUU,
                    "Removes the trigger from the game entirely, ensuring" +
                    "\nit will never activate." },
                { ACTION_DESTROY_VVVV,
                    "Removes the trigger from the game entirely, ensuring" +
                    "\nit will never activate." },
                { ACTION_DESTROY_WWWW,
                    "Removes the trigger from the game entirely, ensuring" +
                    "\nit will never activate." },
                { ACTION_AUTOCREATE,
                    "From a celltrigger, this activates on all Houses." +
                    "\nFrom a map object, this affects the object's House." +
                    "\nOtherwise, the House set in the trigger is affected." },
                { ACTION_WINLOSE,
                    "Normally used with the \"Any\" Event." },
                { ACTION_ALLOWWIN,
                    "Destroying this trigger using a \"Dstry Trig\" Action" +
                    "\nhas the same effect as activating it; it will remove" +
                    "\nthe Win blockage and allow winning." },
            };
            TypesDescription = new ReadOnlyDictionary<string, string>(typesDescription);
        }

        public static IEnumerable<string> GetTypes()
        {
            return Types;
        }
    }
}
