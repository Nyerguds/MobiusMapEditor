//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                     Version 2, December 2004
//
//  Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
//  Everyone is permitted to copy and distribute verbatim or modified
//  copies of this license document, and changing it is allowed as long
//  as the name is changed.
//
//             DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//    TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//   0. You just DO WHAT THE FUCK YOU WANT TO.
using MobiusEditor.Model;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MobiusEditor.SoleSurvivor
{
    public static class HouseTypes
    {
        // Legacy
        public static readonly HouseType Good      /**/ = new HouseType(0, "GoodGuy", "TEXT_GLOBAL_DEFENSE_INITIATIVE", HouseTypeFlag.ForAlliances, "BAD_STRUCTURE");
        public static readonly HouseType Bad       /**/ = new HouseType(1, "BadGuy", "TEXT_BROTHERHOOD_OF_NOD", HouseTypeFlag.ForAlliances, "BAD_STRUCTURE");
        public static readonly HouseType Neutral   /**/ = new HouseType(2, "Neutral", "TEXT_UNIT_TITLE_CIVILIAN", HouseTypeFlag.ForAlliances, "GOOD");
        public static readonly HouseType Special   /**/ = new HouseType(3, "Special", "TEXT_FACTION_NAME_FACTION_JURASSIC", HouseTypeFlag.ForAlliances, "BAD_UNIT");
        // Special for SS
        public static readonly HouseType Admin     /**/ = new HouseType(4, "Admin", HouseTypeFlag.ForAlliances, "BAD_UNIT");
        public static readonly HouseType Spectator /**/ = new HouseType(5, "Spectator", HouseTypeFlag.ForAlliances, "MULTI6");
        // Teams for football/CTF games
        public static readonly HouseType Team1     /**/ = new HouseType(6, "Team 1", HouseTypeFlag.ForAlliances, "MULTI6"); // Teal
        public static readonly HouseType Team2     /**/ = new HouseType(7, "Team 2", HouseTypeFlag.ForAlliances, "MULTI5"); // Orange
        public static readonly HouseType Team3     /**/ = new HouseType(8, "Team 3", HouseTypeFlag.ForAlliances, "MULTI4"); // Green
        public static readonly HouseType Team4     /**/ = new HouseType(9, "Team 4", HouseTypeFlag.ForAlliances, "BAD_UNIT"); // Gray
        // Guess I'll add these too? Kind of useless though...
        public static readonly HouseType Multi1    /**/ = new HouseType(10, "Multi1", Waypoint.GetFlagForMpId(0), HouseTypeFlag.ForAlliances, "MULTI2"); // Blue (originally teal)
        public static readonly HouseType Multi2    /**/ = new HouseType(11, "Multi2", Waypoint.GetFlagForMpId(1), HouseTypeFlag.ForAlliances, "MULTI5"); // Orange
        public static readonly HouseType Multi3    /**/ = new HouseType(12, "Multi3", Waypoint.GetFlagForMpId(2), HouseTypeFlag.ForAlliances, "MULTI4"); // Green
        public static readonly HouseType Multi4    /**/ = new HouseType(13, "Multi4", Waypoint.GetFlagForMpId(3), HouseTypeFlag.ForAlliances, "MULTI6"); // Teal (originally gray)
        public static readonly HouseType Multi5    /**/ = new HouseType(14, "Multi5", Waypoint.GetFlagForMpId(4), HouseTypeFlag.None, "MULTI1"); // Yellow
        public static readonly HouseType Multi6    /**/ = new HouseType(15, "Multi6", Waypoint.GetFlagForMpId(5), HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi7    /**/ = new HouseType(16, "Multi7", Waypoint.GetFlagForMpId(6), HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi8    /**/ = new HouseType(17, "Multi8", Waypoint.GetFlagForMpId(7), HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi9    /**/ = new HouseType(18, "Multi9", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi10   /**/ = new HouseType(19, "Multi10", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi11   /**/ = new HouseType(20, "Multi11", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi12   /**/ = new HouseType(21, "Multi12", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi13   /**/ = new HouseType(22, "Multi13", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi14   /**/ = new HouseType(23, "Multi14", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi15   /**/ = new HouseType(24, "Multi15", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi16   /**/ = new HouseType(25, "Multi16", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi17   /**/ = new HouseType(26, "Multi17", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi18   /**/ = new HouseType(27, "Multi18", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi19   /**/ = new HouseType(28, "Multi19", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi20   /**/ = new HouseType(29, "Multi20", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi21   /**/ = new HouseType(30, "Multi21", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi22   /**/ = new HouseType(31, "Multi22", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi23   /**/ = new HouseType(32, "Multi23", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi24   /**/ = new HouseType(33, "Multi24", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi25   /**/ = new HouseType(34, "Multi25", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi26   /**/ = new HouseType(35, "Multi26", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi27   /**/ = new HouseType(36, "Multi27", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi28   /**/ = new HouseType(37, "Multi28", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi29   /**/ = new HouseType(38, "Multi29", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi30   /**/ = new HouseType(39, "Multi30", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi31   /**/ = new HouseType(40, "Multi31", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi32   /**/ = new HouseType(41, "Multi32", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi33   /**/ = new HouseType(42, "Multi33", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi34   /**/ = new HouseType(43, "Multi34", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi35   /**/ = new HouseType(44, "Multi35", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi36   /**/ = new HouseType(45, "Multi36", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi37   /**/ = new HouseType(46, "Multi37", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi38   /**/ = new HouseType(47, "Multi38", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi39   /**/ = new HouseType(48, "Multi39", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi40   /**/ = new HouseType(49, "Multi40", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi41   /**/ = new HouseType(50, "Multi41", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi42   /**/ = new HouseType(51, "Multi42", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi43   /**/ = new HouseType(52, "Multi43", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi44   /**/ = new HouseType(53, "Multi44", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi45   /**/ = new HouseType(54, "Multi45", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi46   /**/ = new HouseType(55, "Multi46", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi47   /**/ = new HouseType(56, "Multi47", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi48   /**/ = new HouseType(57, "Multi48", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi49   /**/ = new HouseType(58, "Multi49", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi50   /**/ = new HouseType(59, "Multi50", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi51   /**/ = new HouseType(60, "Multi51", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi52   /**/ = new HouseType(61, "Multi52", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi53   /**/ = new HouseType(62, "Multi53", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi54   /**/ = new HouseType(63, "Multi54", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi55   /**/ = new HouseType(64, "Multi55", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi56   /**/ = new HouseType(65, "Multi56", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi57   /**/ = new HouseType(66, "Multi57", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi58   /**/ = new HouseType(67, "Multi58", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi59   /**/ = new HouseType(68, "Multi59", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi60   /**/ = new HouseType(69, "Multi60", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi61   /**/ = new HouseType(70, "Multi61", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi62   /**/ = new HouseType(71, "Multi62", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi63   /**/ = new HouseType(72, "Multi63", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi64   /**/ = new HouseType(73, "Multi64", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi65   /**/ = new HouseType(74, "Multi65", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi66   /**/ = new HouseType(75, "Multi66", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi67   /**/ = new HouseType(76, "Multi67", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi68   /**/ = new HouseType(77, "Multi68", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi69   /**/ = new HouseType(78, "Multi69", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi70   /**/ = new HouseType(79, "Multi70", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi71   /**/ = new HouseType(80, "Multi71", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi72   /**/ = new HouseType(81, "Multi72", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi73   /**/ = new HouseType(82, "Multi73", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi74   /**/ = new HouseType(83, "Multi74", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi75   /**/ = new HouseType(84, "Multi75", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi76   /**/ = new HouseType(85, "Multi76", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi77   /**/ = new HouseType(86, "Multi77", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi78   /**/ = new HouseType(87, "Multi78", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi79   /**/ = new HouseType(88, "Multi79", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi80   /**/ = new HouseType(89, "Multi80", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi81   /**/ = new HouseType(90, "Multi81", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi82   /**/ = new HouseType(91, "Multi82", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi83   /**/ = new HouseType(92, "Multi83", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi84   /**/ = new HouseType(93, "Multi84", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi85   /**/ = new HouseType(94, "Multi85", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi86   /**/ = new HouseType(95, "Multi86", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi87   /**/ = new HouseType(96, "Multi87", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi88   /**/ = new HouseType(97, "Multi88", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi89   /**/ = new HouseType(98, "Multi89", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi90   /**/ = new HouseType(99, "Multi90", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi91   /**/ = new HouseType(100, "Multi91", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi92   /**/ = new HouseType(101, "Multi92", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi93   /**/ = new HouseType(102, "Multi93", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi94   /**/ = new HouseType(103, "Multi94", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi95   /**/ = new HouseType(104, "Multi95", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi96   /**/ = new HouseType(105, "Multi96", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi97   /**/ = new HouseType(106, "Multi97", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi98   /**/ = new HouseType(107, "Multi98", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi99   /**/ = new HouseType(108, "Multi99", HouseTypeFlag.None, "MULTI3"); // Red
        public static readonly HouseType Multi100  /**/ = new HouseType(109, "Multi100", HouseTypeFlag.None, "MULTI3"); // Red

        private static readonly HouseType[] Types;

        static HouseTypes()
        {
            Types =
                (from field in typeof(HouseTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
                 where field.IsInitOnly && typeof(HouseType).IsAssignableFrom(field.FieldType)
                 select field.GetValue(null) as HouseType).OrderBy(h => h.ID).ToArray();
        }

        public static IEnumerable<HouseType> GetTypes()
        {
            return Types;
        }

        public static string GetClassicOpposingPlayer(string player)
        {
            return Good.Equals(player) ? Bad.Name : Good.Name;
        }
    }
}
