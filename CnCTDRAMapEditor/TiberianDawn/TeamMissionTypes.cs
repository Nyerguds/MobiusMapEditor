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
using System.Collections.Generic;

namespace MobiusEditor.TiberianDawn
{
    public static class TeamMissionTypes
    {
        private static readonly TeamMission[] Types = new TeamMission[]
        {
            new TeamMission("Attack Base", TeamMissionArgType.Time),
            new TeamMission("Attack Units", TeamMissionArgType.Time),
            new TeamMission("Attack Civil.", TeamMissionArgType.Time),
            new TeamMission("Rampage", TeamMissionArgType.Time),
            new TeamMission("Defend Base", TeamMissionArgType.Time),
            new TeamMission("Move", TeamMissionArgType.Waypoint),
            new TeamMission("Move to Cell", TeamMissionArgType.Number),
            new TeamMission("Retreat", TeamMissionArgType.Time),
            new TeamMission("Guard", TeamMissionArgType.Time),
            new TeamMission("Loop", TeamMissionArgType.Number),
            new TeamMission("Attack Tarcom", TeamMissionArgType.Tarcom),
            new TeamMission("Unload", TeamMissionArgType.Waypoint),
        };

        public static IEnumerable<TeamMission> GetTypes()
        {
            return Types;
        }

        private static readonly string[] TypesInfo = new string[]
        {
            "Time in 1/10th min",
            "Time in 1/10th min",
            "Time in 1/10th min",
            "Time in 1/10th min",
            "Time in 1/10th min",
            "Waypoint number",
            "Cell number",
            "",
            "Time in 1/10th min",
            "Amount of actions to trim off the Missions loop",
            "Tarcom identifier",
            "Waypoint at which to unload."
        };

        public static IEnumerable<string> GetTypesInfo()
        {
            return TypesInfo;
        }
    }
}
