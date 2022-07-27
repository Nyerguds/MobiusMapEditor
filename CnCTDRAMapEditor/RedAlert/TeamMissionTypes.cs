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
    public static class TeamMissionTypes
    {
        private static readonly string[] Types = new string[]
        {
            "Attack a Type...",
            "Attack a Waypoint...",
            "Change Formation...",
            "Move to Waypoint...",
            "Move to Cell...",
            "Guard Area...",
            "Jump to Order #...",
            "Attack Tarcom...",
            "Unload",
            "Deploy",
            "Follow Friendlies",
            "Do This...",
            "Set Global...",
            "Invulnerable...",
            "Load onto Transport",
            "Spy on Building...",
            "Patrol to..."
        };

        public static IEnumerable<string> GetTypes()
        {
            return Types;
        }

        private static readonly string[] TypesInfo = new string[]
        {
            " 1 = Anything\n 2 = Buildings\n 3 = Harvesters\n 4 = Infantry\n 5 = Vehicles\n 6 = Ships\n 7 = Factories\n 8 = Base Defences\n 9 = Base Threats\n10 = Power Facilities\n11 = Fake Buildings",
            "Waypoint number",
            "1 = Tight (surround weaker units)\n2 = Spread Out\n3 = Wedge North\n4 = Wedge East\n5 = Wedge South\n6 = Wedge West\n7 = Column N/S\n8 = Line E/W",
            "Waypoint number",
            "Cell number",
            "Time in 1/10th min",
            "Orders line to jump to",
            "Tarcom identifier",
            "",
            "",
            "",
            " 0 = Sleep\n 1 = Attack\n 2 = Move\n 3 = Qmove\n 4 = Retreat\n 5 = Guard\n 6 = Sticky\n 7 = Enter\n 8 = Capture\n 9 = Harvest\n10 = Area Guard\n11 = Return\n12 = Stop\n13 = Ambush\n14 = Hunt\n15 = Unload\n16 = Sabotage\n17 = Construction (buildings only)\n18 = Selling (buildings only)\n19 = Repair (buildings only)\n20 = Rescue\n21 = Missile (buildings only)\n22 = Harmless",
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
