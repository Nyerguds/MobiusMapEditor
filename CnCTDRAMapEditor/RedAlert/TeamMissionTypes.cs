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
using MobiusEditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MobiusEditor.RedAlert
{
    public static class TeamMissionTypes
    {
        public static readonly TeamMission Attack = new TeamMission(0, "Attack a Type...", TeamMissionArgType.OptionsList, (0, "Nothing"), (1, "Anything"), (2, "Buildings"), (3, "Harvesters"), (4, "Infantry"), (5, "Vehicles"), (6, "Ships"), (7, "Factories"), (8, "Base Defences"), (9, "Base Threats"), (10, "Power Facilities"), (11, "Fake Buildings"));
        public static readonly TeamMission AttackWaypt = new TeamMission(1, "Attack a Waypoint...", TeamMissionArgType.Waypoint);
        public static readonly TeamMission Formation = new TeamMission(2, "Change Formation...", TeamMissionArgType.OptionsList, (0, "None"), (1, "Tight (surround weaker units)"), (2, "Spread Out"), (3, "Wedge North"), (4, "Wedge East"), (5, "Wedge South"), (6, "Wedge West"), (7, "Column N/S"), (8, "Line E/W"));
        public static readonly TeamMission Move = new TeamMission(3, "Move to Waypoint...", TeamMissionArgType.Waypoint);
        public static readonly TeamMission Movecell = new TeamMission(4, "Move to Cell...", TeamMissionArgType.MapCell);
        public static readonly TeamMission Guard = new TeamMission(5, "Guard Area...", TeamMissionArgType.Waypoint);
        public static readonly TeamMission Loop = new TeamMission(6, "Jump to Order #...", TeamMissionArgType.OrderNumber);
        public static readonly TeamMission AttackTarcom = new TeamMission(7, "Attack Tarcom...", TeamMissionArgType.Tarcom);
        public static readonly TeamMission Unload = new TeamMission(8, "Unload", TeamMissionArgType.None);
        public static readonly TeamMission Deploy = new TeamMission(9, "Deploy", TeamMissionArgType.None);
        public static readonly TeamMission HoundDog = new TeamMission(10, "Follow Friendlies", TeamMissionArgType.None);
        public static readonly TeamMission Do = new TeamMission(11, "Do This...", TeamMissionArgType.OptionsList, (0, "Sleep"), (1, "Attack"), (2, "Move"), (3, "Qmove"), (4, "Retreat"), (5, "Guard"), (6, "Sticky"), (7, "Enter"), (8, "Capture"), (9, "Harvest"), (10, "Area Guard"), (11, "Return"), (12, "Stop"), (13, "Ambush"), (14, "Hunt"), (15, "Unload"), (16, "Sabotage"), (20, "Rescue"), (22, "Harmless"));
        public static readonly TeamMission SetGlobal = new TeamMission(12, "Set Global...", TeamMissionArgType.GlobalNumber);
        public static readonly TeamMission Invulnerable = new TeamMission(13, "Invulnerable", TeamMissionArgType.None);
        public static readonly TeamMission Load = new TeamMission(14, "Load onto Transport", TeamMissionArgType.None);
        public static readonly TeamMission Spy = new TeamMission(15, "Spy on Building...", TeamMissionArgType.Waypoint);
        public static readonly TeamMission Patrol = new TeamMission(16, "Patrol to...", TeamMissionArgType.Waypoint);

        private static readonly TeamMission[] Types;

        static TeamMissionTypes()
        {
            Types =
                (from field in typeof(TeamMissionTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
                 where field.IsInitOnly && typeof(TeamMission).IsAssignableFrom(field.FieldType)
                 select field.GetValue(null) as TeamMission).ToArray();
        }

        public static IEnumerable<TeamMission> GetTypes()
        {
            return Types;
        }
    }
}
