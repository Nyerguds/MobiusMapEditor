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

namespace MobiusEditor.RedAlert
{
    public static class TeamMissionTypes
    {
        private static readonly TeamMission[] Types = new TeamMission[]
        {
            new TeamMission("Attack a Type...", TeamMissionArgType.OptionsList, (1, "Anything"), (2, "Buildings"), (3, "Harvesters"), (4, "Infantry"), (5, "Vehicles"), (6, "Ships"), (7, "Factories"), (8, "Base Defences"), (9, "Base Threats"), (10, "Power Facilities"), (11, "Fake Buildings")),
            new TeamMission("Attack a Waypoint...", TeamMissionArgType.Waypoint),
            new TeamMission("Change Formation...", TeamMissionArgType.OptionsList, (1, "Tight (surround weaker units)"), (2, "Spread Out"), (3, "Wedge North"), (4, "Wedge East"), (5, "Wedge South"), (6, "Wedge West"), (7, "Column N/S"), (8, "Line E/W")),
            new TeamMission("Move to Waypoint...", TeamMissionArgType.Waypoint),
            new TeamMission("Move to Cell...", TeamMissionArgType.Number),
            new TeamMission("Guard Area...", TeamMissionArgType.Waypoint),
            new TeamMission("Jump to Order #...", TeamMissionArgType.Number),
            new TeamMission("Attack Tarcom...", TeamMissionArgType.Tarcom),
            new TeamMission("Unload", TeamMissionArgType.None),
            new TeamMission("Deploy", TeamMissionArgType.None),
            new TeamMission("Follow Friendlies", TeamMissionArgType.None),
            new TeamMission("Do This...", TeamMissionArgType.OptionsList, (0, "Sleep"), (1, "Attack"), (2, "Move"), (3, "Qmove"), (4, "Retreat"), (5, "Guard"), (6, "Sticky"), (7, "Enter"), (8, "Capture"), (9, "Harvest"), (10, "Area Guard"), (11, "Return"), (12, "Stop"), (13, "Ambush"), (14, "Hunt"), (15, "Unload"), (16, "Sabotage"), (20, "Rescue"), (22, "Harmless")),
            new TeamMission("Set Global...", TeamMissionArgType.Number),
            new TeamMission("Invulnerable", TeamMissionArgType.None),
            new TeamMission("Load onto Transport", TeamMissionArgType.None),
            new TeamMission("Spy on Building...", TeamMissionArgType.Waypoint),
            new TeamMission("Patrol to...", TeamMissionArgType.Waypoint),
        };

        public static IEnumerable<TeamMission> GetTypes()
        {
            return Types;
        }

        private static readonly string[] TypesInfo = new string[]
        {
            "Type to attack",
            "Waypoint number",
            "Formation",
            "Waypoint number",
            "Cell number",
            "Time in 1/10th min",
            "Orders line to jump to",
            "Tarcom identifier",
            "",
            "",
            "",
            "Action",
            "Global number to set.",
            "Time in 1/10th min",
            "",
            "Waypoint number",
            "Waypoint number",
        };

        public static IEnumerable<string> GetTypesInfo()
        {
            return TypesInfo;
        }
    }
}
