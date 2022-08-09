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
using System.Linq;
using System.Reflection;

namespace MobiusEditor.TiberianDawn
{
    public static class TeamMissionTypes
    {

        public static readonly TeamMission AttackBase = new TeamMission(0, "Attack Base", TeamMissionArgType.Time);
        public static readonly TeamMission AttackUnits = new TeamMission(1, "Attack Units", TeamMissionArgType.Time);
        public static readonly TeamMission AttackCivilians = new TeamMission(2, "Attack Civil.", TeamMissionArgType.Time);
        public static readonly TeamMission Rampage = new TeamMission(3, "Rampage", TeamMissionArgType.Time);
        public static readonly TeamMission DefendBase = new TeamMission(4, "Defend Base", TeamMissionArgType.Time);
        public static readonly TeamMission Move = new TeamMission(5, "Move", TeamMissionArgType.Waypoint);
        public static readonly TeamMission MoveToCell = new TeamMission(6, "Move to Cell", TeamMissionArgType.MapCell);
        public static readonly TeamMission Retreat = new TeamMission(7, "Retreat", TeamMissionArgType.Time);
        public static readonly TeamMission Guard = new TeamMission(8, "Guard", TeamMissionArgType.Time);
        public static readonly TeamMission Loop = new TeamMission(9, "Loop", TeamMissionArgType.OrderNumber);
        public static readonly TeamMission AttackTarcom = new TeamMission(10, "Attack Tarcom", TeamMissionArgType.Tarcom);
        public static readonly TeamMission Unload = new TeamMission(11, "Unload", TeamMissionArgType.Waypoint);

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
