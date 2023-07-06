﻿//
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
using MobiusEditor.Utility;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace MobiusEditor.RedAlert
{
    public static class TemplateTypes
    {
        // Equivalents groups; predefined for easy identification.
        private static readonly string[] ShoresWaterSouth = new[] { "sh04", "sh05", "sh06", "sh07" };
        private static readonly string[] ShoresWaterNorth = new[] { "sh25", "sh26", "sh27", "sh28" };
        private static readonly string[] CliffsSouth = new[] { "s03", "s04", "s05" };
        private static readonly string[] CliffsWest = new[] { "s10", "s11", "s12" };
        private static readonly string[] CliffsNorth = new[] { "s17", "s18", "s19" };
        private static readonly string[] CliffsEast = new[] { "s24", "s25", "s26" };
        private static readonly string[] RoadNorthSouth = new[] { "d07", "d08" };
        private static readonly string[] RoadEastWest4 = new[] { "d09", "d10" };
        private static readonly string[] RoadEastWest2 = new[] { "d11", "d12" };
        private static readonly string[] RiverNorthSouth2 = new[] { "rv06", "rv07" };
        private static readonly string[] WaterCliffsSouth = new[] { "wc03", "wc04", "wc05" };
        private static readonly string[] WaterCliffsWest = new[] { "wc10", "wc11", "wc12" };
        private static readonly string[] WaterCliffsNorth = new[] { "wc17", "wc18", "wc19" };
        private static readonly string[] WaterCliffsEast = new[] { "wc24", "wc25", "wc26" };
        private static readonly string[] WallCornerNorthWestInt = new[] { "wall0023", "wall0025" };
        private static readonly string[] WallCornerNorthWestExt = new[] { "wall0024", "wall0026" };
        private static readonly string[] WallCornerNorthEastInt = new[] { "wall0027", "wall0029" };
        private static readonly string[] WallCornerNorthEastExt = new[] { "wall0028", "wall0030" };
        private static readonly string[] WallCornerSouthWestInt = new[] { "wall0031", "wall0033" };
        private static readonly string[] WallCornerSouthWestExt = new[] { "wall0032", "wall0034" };
        private static readonly string[] WallCornerSouthEastInt = new[] { "wall0035", "wall0037" };
        private static readonly string[] WallCornerSouthEastExt = new[] { "wall0036", "wall0038" };
        private static readonly string[] WallTSouthInt = new[] { "wall0043", "wall0045" };
        private static readonly string[] WallTSouthExt = new[] { "wall0044", "wall0046" };

        public static readonly TemplateType Clear = new TemplateType(0, "clear1", 1, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow, TheaterTypes.Interior }, null, TemplateTypeFlag.Clear);
        public static readonly TemplateType Water = new TemplateType(1, "w1", 1, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Water2 = new TemplateType(2, "w2", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Shore01 = new TemplateType(3, "sh01", 4, 5, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "0001 0111 1111 1100 1000");
        public static readonly TemplateType Shore02 = new TemplateType(4, "sh02", 5, 5, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "00001 00011 11111 11110 11100");
        public static readonly TemplateType Shore03 = new TemplateType(5, "sh03", 3, 5, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "001 011 111 110 100");
        public static readonly TemplateType Shore04 = new TemplateType(6, "sh04", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, ShoresWaterSouth);
        public static readonly TemplateType Shore05 = new TemplateType(7, "sh05", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, ShoresWaterSouth);
        public static readonly TemplateType Shore06 = new TemplateType(8, "sh06", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, ShoresWaterSouth);
        public static readonly TemplateType Shore07 = new TemplateType(9, "sh07", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, ShoresWaterSouth);
        public static readonly TemplateType Shore08 = new TemplateType(10, "sh08", 1, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Shore09 = new TemplateType(11, "sh09", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Shore10 = new TemplateType(12, "sh10", 5, 6, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "10000 11000 11110 00111 00111 00011");
        public static readonly TemplateType Shore11 = new TemplateType(13, "sh11", 4, 5, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "1100 1110 1111 0011 0011");
        public static readonly TemplateType Shore12 = new TemplateType(14, "sh12", 3, 5, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "110 111 111 011 001");
        public static readonly TemplateType Shore13 = new TemplateType(15, "sh13", 6, 5, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "111000 011100 011110 011111 000111");
        public static readonly TemplateType Shore14 = new TemplateType(16, "sh14", 4, 4, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "1110 1110 1110 0111");
        public static readonly TemplateType Shore15 = new TemplateType(17, "sh15", 5, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11110 01110 00111");
        public static readonly TemplateType Shore16 = new TemplateType(18, "sh16", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Shore17 = new TemplateType(19, "sh17", 2, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Shore18 = new TemplateType(20, "sh18", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Shore19 = new TemplateType(21, "sh19", 4, 5, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "0111 0111 1110 1110 1110");
        public static readonly TemplateType Shore20 = new TemplateType(22, "sh20", 5, 4, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "00111 11110 11100 11100");
        public static readonly TemplateType Shore21 = new TemplateType(23, "sh21", 5, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "01111 11110 11100");
        public static readonly TemplateType Shore22 = new TemplateType(24, "sh22", 6, 5, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "100000 110000 111111 011111 000111");
        public static readonly TemplateType Shore23 = new TemplateType(25, "sh23", 5, 5, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11100 11111 01111 00111 00111");
        public static readonly TemplateType Shore24 = new TemplateType(26, "sh24", 3, 4, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "110 111 011 001");
        public static readonly TemplateType Shore25 = new TemplateType(27, "sh25", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, ShoresWaterNorth);
        public static readonly TemplateType Shore26 = new TemplateType(28, "sh26", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, ShoresWaterNorth);
        public static readonly TemplateType Shore27 = new TemplateType(29, "sh27", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, ShoresWaterNorth);
        public static readonly TemplateType Shore28 = new TemplateType(30, "sh28", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "011 111 111", ShoresWaterNorth);
        public static readonly TemplateType Shore29 = new TemplateType(31, "sh29", 1, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Shore30 = new TemplateType(32, "sh30", 3, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Shore31 = new TemplateType(33, "sh31", 6, 5, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "000111 001111 111110 111110 111110");
        public static readonly TemplateType Shore32 = new TemplateType(34, "sh32", 4, 4, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "0011 1111 1110 1100");
        public static readonly TemplateType Shore33 = new TemplateType(35, "sh33", 3, 4, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "011 111 110 110");
        public static readonly TemplateType Shore34 = new TemplateType(36, "sh34", 6, 5, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "000111 011110 011100 011100 111000");
        public static readonly TemplateType Shore35 = new TemplateType(37, "sh35", 4, 4, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "0011 0111 0111 1110");
        public static readonly TemplateType Shore36 = new TemplateType(38, "sh36", 4, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "0111 1110 1110");
        public static readonly TemplateType Shore37 = new TemplateType(39, "sh37", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "011 111 111");
        public static readonly TemplateType Shore38 = new TemplateType(40, "sh38", 2, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Shore39 = new TemplateType(41, "sh39", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Shore40 = new TemplateType(42, "sh40", 5, 5, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11100 01100 01110 01111 00111");
        public static readonly TemplateType Shore41 = new TemplateType(43, "sh41", 4, 4, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "1100 1110 1111 0111");
        public static readonly TemplateType Shore42 = new TemplateType(44, "sh42", 4, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "1110 0111 0111");
        public static readonly TemplateType Shore43 = new TemplateType(45, "sh43", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Shore44 = new TemplateType(46, "sh44", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Shore45 = new TemplateType(47, "sh45", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Shore46 = new TemplateType(48, "sh46", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Shore47 = new TemplateType(49, "sh47", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "100 110 111");
        public static readonly TemplateType Shore48 = new TemplateType(50, "sh48", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Shore49 = new TemplateType(51, "sh49", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Shore50 = new TemplateType(52, "sh50", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Shore51 = new TemplateType(53, "sh51", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "111 011 011");
        public static readonly TemplateType Shore52 = new TemplateType(54, "sh52", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "011 111 011");
        public static readonly TemplateType Shore53 = new TemplateType(55, "sh53", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "110 111 111");
        public static readonly TemplateType Shore54 = new TemplateType(56, "sh54", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "111 110 100");
        public static readonly TemplateType Shore55 = new TemplateType(57, "sh55", 1, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Shore56 = new TemplateType(58, "sh56", 2, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff01 = new TemplateType(59, "wc01", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff02 = new TemplateType(60, "wc02", 2, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff03 = new TemplateType(61, "wc03", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, WaterCliffsSouth);
        public static readonly TemplateType ShoreCliff04 = new TemplateType(62, "wc04", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, WaterCliffsSouth);
        public static readonly TemplateType ShoreCliff05 = new TemplateType(63, "wc05", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, WaterCliffsSouth);
        public static readonly TemplateType ShoreCliff06 = new TemplateType(64, "wc06", 2, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11 11 10");
        public static readonly TemplateType ShoreCliff07 = new TemplateType(65, "wc07", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff08 = new TemplateType(66, "wc08", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff09 = new TemplateType(67, "wc09", 3, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff10 = new TemplateType(68, "wc10", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, WaterCliffsWest);
        public static readonly TemplateType ShoreCliff11 = new TemplateType(69, "wc11", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, WaterCliffsWest);
        public static readonly TemplateType ShoreCliff12 = new TemplateType(70, "wc12", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, WaterCliffsWest);
        public static readonly TemplateType ShoreCliff13 = new TemplateType(71, "wc13", 3, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff14 = new TemplateType(72, "wc14", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff15 = new TemplateType(73, "wc15", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff16 = new TemplateType(74, "wc16", 2, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "10 11 01");
        public static readonly TemplateType ShoreCliff17 = new TemplateType(75, "wc17", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, WaterCliffsNorth);
        public static readonly TemplateType ShoreCliff18 = new TemplateType(76, "wc18", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, WaterCliffsNorth);
        public static readonly TemplateType ShoreCliff19 = new TemplateType(77, "wc19", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, WaterCliffsNorth);
        public static readonly TemplateType ShoreCliff20 = new TemplateType(78, "wc20", 2, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "01 11 11");
        public static readonly TemplateType ShoreCliff21 = new TemplateType(79, "wc21", 1, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff22 = new TemplateType(80, "wc22", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff23 = new TemplateType(81, "wc23", 3, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff24 = new TemplateType(82, "wc24", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, WaterCliffsEast);
        public static readonly TemplateType ShoreCliff25 = new TemplateType(83, "wc25", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, WaterCliffsEast);
        public static readonly TemplateType ShoreCliff26 = new TemplateType(84, "wc26", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, WaterCliffsEast);
        public static readonly TemplateType ShoreCliff27 = new TemplateType(85, "wc27", 3, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff28 = new TemplateType(86, "wc28", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11 01");
        public static readonly TemplateType ShoreCliff29 = new TemplateType(87, "wc29", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff30 = new TemplateType(88, "wc30", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff31 = new TemplateType(89, "wc31", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff32 = new TemplateType(90, "wc32", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11 10");
        public static readonly TemplateType ShoreCliff33 = new TemplateType(91, "wc33", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff34 = new TemplateType(92, "wc34", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff35 = new TemplateType(93, "wc35", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff36 = new TemplateType(94, "wc36", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff37 = new TemplateType(95, "wc37", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType ShoreCliff38 = new TemplateType(96, "wc38", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Boulder1 = new TemplateType(97, "b1", 1, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Boulder2 = new TemplateType(98, "b2", 2, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Boulder3 = new TemplateType(99, "b3", 3, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        //public static readonly TemplateType Boulder4 = new TemplateType(100, "b4", 0, 0, new [] { TheaterTypes.Temperate, TheaterTypes.Snow }, null); // Files don't actually exist.
        //public static readonly TemplateType Boulder5 = new TemplateType(101, "b5", 0, 0, new [] { TheaterTypes.Temperate, TheaterTypes.Snow }, null); // Files don't actually exist.
        //public static readonly TemplateType Boulder6 = new TemplateType(102, "b6", 0, 0, new [] { TheaterTypes.Temperate, TheaterTypes.Snow }, null); // Files don't actually exist.
        public static readonly TemplateType Patch01 = new TemplateType(103, "p01", 1, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Patch02 = new TemplateType(104, "p02", 1, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Patch03 = new TemplateType(105, "p03", 1, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Patch04 = new TemplateType(106, "p04", 1, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Patch07 = new TemplateType(107, "p07", 4, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Patch08 = new TemplateType(108, "p08", 3, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Patch13 = new TemplateType(109, "p13", 3, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Patch14 = new TemplateType(110, "p14", 2, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        //public static readonly TemplateType Patch15 = new TemplateType(111, "p15", 0, 0, new [] { TheaterTypes.Temperate, TheaterTypes.Snow }, null); // Files don't actually exist. Leftover of TD Snow.
        public static readonly TemplateType River01 = new TemplateType(112, "rv01", 5, 4, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11000 11111 11111 01100");
        public static readonly TemplateType River02 = new TemplateType(113, "rv02", 5, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11111 11111 11100");
        public static readonly TemplateType River03 = new TemplateType(114, "rv03", 4, 4, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "1100 1111 1111 0111");
        public static readonly TemplateType River04 = new TemplateType(115, "rv04", 4, 4, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "0011 0111 1111 1110");
        public static readonly TemplateType River05 = new TemplateType(116, "rv05", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType River06 = new TemplateType(117, "rv06", 3, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, RiverNorthSouth2);
        public static readonly TemplateType River07 = new TemplateType(118, "rv07", 3, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, RiverNorthSouth2);
        public static readonly TemplateType River08 = new TemplateType(119, "rv08", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType River09 = new TemplateType(120, "rv09", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType River10 = new TemplateType(121, "rv10", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType River11 = new TemplateType(122, "rv11", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType River12 = new TemplateType(123, "rv12", 3, 4, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType River13 = new TemplateType(124, "rv13", 4, 4, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "0011 1111 1111 1111");
        public static readonly TemplateType Falls1 = new TemplateType(125, "falls1", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Falls1a = new TemplateType(126, "falls1a", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Falls2 = new TemplateType(127, "falls2", 3, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Falls2a = new TemplateType(128, "falls2a", 3, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Ford1 = new TemplateType(129, "ford1", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Ford2 = new TemplateType(130, "ford2", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Bridge1 = new TemplateType(131, "bridge1", 5, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "01111 11111 00100");
        public static readonly TemplateType Bridge1d = new TemplateType(132, "bridge1d", 5, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "01111 11111 00100");
        public static readonly TemplateType Bridge2 = new TemplateType(133, "bridge2", 5, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11110 11111");
        public static readonly TemplateType Bridge2d = new TemplateType(134, "bridge2d", 5, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11110 11111");
        public static readonly TemplateType Slope01 = new TemplateType(135, "s01", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Slope02 = new TemplateType(136, "s02", 2, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Slope03 = new TemplateType(137, "s03", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, CliffsSouth);
        public static readonly TemplateType Slope04 = new TemplateType(138, "s04", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, CliffsSouth);
        public static readonly TemplateType Slope05 = new TemplateType(139, "s05", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, CliffsSouth);
        public static readonly TemplateType Slope06 = new TemplateType(140, "s06", 2, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11 11 10");
        public static readonly TemplateType Slope07 = new TemplateType(141, "s07", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Slope08 = new TemplateType(142, "s08", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Slope09 = new TemplateType(143, "s09", 3, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Slope10 = new TemplateType(144, "s10", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, CliffsWest);
        public static readonly TemplateType Slope11 = new TemplateType(145, "s11", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, CliffsWest);
        public static readonly TemplateType Slope12 = new TemplateType(146, "s12", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, CliffsWest);
        public static readonly TemplateType Slope13 = new TemplateType(147, "s13", 3, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Slope14 = new TemplateType(148, "s14", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11 10");
        public static readonly TemplateType Slope15 = new TemplateType(149, "s15", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Slope16 = new TemplateType(150, "s16", 2, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "10 11 01");
        public static readonly TemplateType Slope17 = new TemplateType(151, "s17", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, CliffsNorth);
        public static readonly TemplateType Slope18 = new TemplateType(152, "s18", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, CliffsNorth);
        public static readonly TemplateType Slope19 = new TemplateType(153, "s19", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, CliffsNorth);
        public static readonly TemplateType Slope20 = new TemplateType(154, "s20", 2, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "01 11 11");
        public static readonly TemplateType Slope21 = new TemplateType(155, "s21", 1, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Slope22 = new TemplateType(156, "s22", 2, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Slope23 = new TemplateType(157, "s23", 3, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Slope24 = new TemplateType(158, "s24", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, CliffsEast);
        public static readonly TemplateType Slope25 = new TemplateType(159, "s25", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, CliffsEast);
        public static readonly TemplateType Slope26 = new TemplateType(160, "s26", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, null, CliffsEast);
        public static readonly TemplateType Slope27 = new TemplateType(161, "s27", 3, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Slope28 = new TemplateType(162, "s28", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11 01");
        public static readonly TemplateType Slope29 = new TemplateType(163, "s29", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Slope30 = new TemplateType(164, "s30", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Slope31 = new TemplateType(165, "s31", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Slope32 = new TemplateType(166, "s32", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11 10");
        public static readonly TemplateType Slope33 = new TemplateType(167, "s33", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Slope34 = new TemplateType(168, "s34", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Slope35 = new TemplateType(169, "s35", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Slope36 = new TemplateType(170, "s36", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Slope37 = new TemplateType(171, "s37", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Slope38 = new TemplateType(172, "s38", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Road01 = new TemplateType(173, "d01", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "01 11");
        public static readonly TemplateType Road02 = new TemplateType(174, "d02", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Road03 = new TemplateType(175, "d03", 1, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Road04 = new TemplateType(176, "d04", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "01 11");
        public static readonly TemplateType Road05 = new TemplateType(177, "d05", 3, 4, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "011 110 110 110");
        public static readonly TemplateType Road06 = new TemplateType(178, "d06", 2, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "10 11 11");
        public static readonly TemplateType Road07 = new TemplateType(179, "d07", 3, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "111 011", RoadNorthSouth);
        public static readonly TemplateType Road08 = new TemplateType(180, "d08", 3, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "010 111", RoadNorthSouth);
        public static readonly TemplateType Road09 = new TemplateType(181, "d09", 4, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "1111 1111 0011", RoadEastWest4);
        public static readonly TemplateType Road10 = new TemplateType(182, "d10", 4, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "1100 1111", Point.Empty, RoadEastWest4);
        public static readonly TemplateType Road11 = new TemplateType(183, "d11", 2, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "01 11 10", RoadEastWest2);
        public static readonly TemplateType Road12 = new TemplateType(184, "d12", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "10 11", Point.Empty, RoadEastWest2);
        public static readonly TemplateType Road13 = new TemplateType(185, "d13", 4, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "1110 1111 0011");
        public static readonly TemplateType Road14 = new TemplateType(186, "d14", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "011 111 111");
        public static readonly TemplateType Road15 = new TemplateType(187, "d15", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "111 111 110");
        public static readonly TemplateType Road16 = new TemplateType(188, "d16", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Road17 = new TemplateType(189, "d17", 3, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Road18 = new TemplateType(190, "d18", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Road19 = new TemplateType(191, "d19", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Road20 = new TemplateType(192, "d20", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "110 111 111");
        public static readonly TemplateType Road21 = new TemplateType(193, "d21", 3, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Road22 = new TemplateType(194, "d22", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "010 111 111");
        public static readonly TemplateType Road23 = new TemplateType(195, "d23", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "011 111 110");
        public static readonly TemplateType Road24 = new TemplateType(196, "d24", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "110 111 011");
        public static readonly TemplateType Road25 = new TemplateType(197, "d25", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "110 111 011");
        public static readonly TemplateType Road26 = new TemplateType(198, "d26", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "01 10");
        public static readonly TemplateType Road27 = new TemplateType(199, "d27", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "01 10");
        public static readonly TemplateType Road28 = new TemplateType(200, "d28", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11 10");
        public static readonly TemplateType Road29 = new TemplateType(201, "d29", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11 10");
        public static readonly TemplateType Road30 = new TemplateType(202, "d30", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11 10");
        public static readonly TemplateType Road31 = new TemplateType(203, "d31", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "01 11");
        public static readonly TemplateType Road32 = new TemplateType(204, "d32", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "01 11");
        public static readonly TemplateType Road33 = new TemplateType(205, "d33", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "01 11");
        public static readonly TemplateType Road34 = new TemplateType(206, "d34", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "011 111 110");
        public static readonly TemplateType Road35 = new TemplateType(207, "d35", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "011 111 110");
        public static readonly TemplateType Road36 = new TemplateType(208, "d36", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "10 01");
        public static readonly TemplateType Road37 = new TemplateType(209, "d37", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "10 01");
        public static readonly TemplateType Road38 = new TemplateType(210, "d38", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11 01");
        public static readonly TemplateType Road39 = new TemplateType(211, "d39", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11 01");
        public static readonly TemplateType Road40 = new TemplateType(212, "d40", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11 01");
        public static readonly TemplateType Road41 = new TemplateType(213, "d41", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "10 11");
        public static readonly TemplateType Road42 = new TemplateType(214, "d42", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "10 11");
        public static readonly TemplateType Road43 = new TemplateType(215, "d43", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "10 11");
        public static readonly TemplateType Rough01 = new TemplateType(216, "rf01", 1, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Rough02 = new TemplateType(217, "rf02", 1, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Rough03 = new TemplateType(218, "rf03", 1, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Rough04 = new TemplateType(219, "rf04", 1, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Rough05 = new TemplateType(220, "rf05", 1, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Rough06 = new TemplateType(221, "rf06", 1, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Rough07 = new TemplateType(222, "rf07", 1, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Rough08 = new TemplateType(223, "rf08", 1, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Rough09 = new TemplateType(224, "rf09", 1, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Rough10 = new TemplateType(225, "rf10", 2, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Rough11 = new TemplateType(226, "rf11", 2, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Road44 = new TemplateType(227, "d44", 1, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Road45 = new TemplateType(228, "d45", 1, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType River14 = new TemplateType(229, "rv14", 1, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType River15 = new TemplateType(230, "rv15", 2, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType RiverCliff01 = new TemplateType(231, "rc01", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType RiverCliff02 = new TemplateType(232, "rc02", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType RiverCliff03 = new TemplateType(233, "rc03", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType RiverCliff04 = new TemplateType(234, "rc04", 2, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Bridge1a = new TemplateType(235, "br1a", 4, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "0110 1111 0111");
        public static readonly TemplateType Bridge1b = new TemplateType(236, "br1b", 4, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Bridge1c = new TemplateType(237, "br1c", 4, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Bridge2a = new TemplateType(238, "br2a", 5, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "01100 11111 01110");
        public static readonly TemplateType Bridge2b = new TemplateType(239, "br2b", 5, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Bridge2c = new TemplateType(240, "br2c", 5, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Bridge3a = new TemplateType(241, "br3a", 4, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Bridge3b = new TemplateType(242, "br3b", 4, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Bridge3c = new TemplateType(243, "br3c", 4, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Bridge3d = new TemplateType(244, "br3d", 4, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Bridge3e = new TemplateType(245, "br3e", 4, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Bridge3f = new TemplateType(246, "br3f", 4, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType F01 = new TemplateType(247, "f01", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType F02 = new TemplateType(248, "f02", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType F03 = new TemplateType(249, "f03", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType F04 = new TemplateType(250, "f04", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType F05 = new TemplateType(251, "f05", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType F06 = new TemplateType(252, "f06", 3, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null);
        public static readonly TemplateType Arro0001 = new TemplateType(253, "arro0001", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Arro0002 = new TemplateType(254, "arro0002", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Arro0003 = new TemplateType(255, "arro0003", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Arro0004 = new TemplateType(256, "arro0004", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Arro0005 = new TemplateType(257, "arro0005", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Arro0006 = new TemplateType(258, "arro0006", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Arro0007 = new TemplateType(259, "arro0007", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Arro0008 = new TemplateType(260, "arro0008", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Arro0009 = new TemplateType(261, "arro0009", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Arro0010 = new TemplateType(262, "arro0010", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Arro0011 = new TemplateType(263, "arro0011", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Arro0012 = new TemplateType(264, "arro0012", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Arro0013 = new TemplateType(265, "arro0013", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Arro0014 = new TemplateType(266, "arro0014", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Arro0015 = new TemplateType(267, "arro0015", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Flor0001 = new TemplateType(268, "flor0001", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Flor0002 = new TemplateType(269, "flor0002", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Flor0003 = new TemplateType(270, "flor0003", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Flor0004 = new TemplateType(271, "flor0004", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Flor0005 = new TemplateType(272, "flor0005", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Flor0006 = new TemplateType(273, "flor0006", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Flor0007 = new TemplateType(274, "flor0007", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Gflr0001 = new TemplateType(275, "gflr0001", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Gflr0002 = new TemplateType(276, "gflr0002", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Gflr0003 = new TemplateType(277, "gflr0003", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Gflr0004 = new TemplateType(278, "gflr0004", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Gflr0005 = new TemplateType(279, "gflr0005", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Gstr0001 = new TemplateType(280, "gstr0001", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Gstr0002 = new TemplateType(281, "gstr0002", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Gstr0003 = new TemplateType(282, "gstr0003", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Gstr0004 = new TemplateType(283, "gstr0004", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Gstr0005 = new TemplateType(284, "gstr0005", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Gstr0006 = new TemplateType(285, "gstr0006", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Gstr0007 = new TemplateType(286, "gstr0007", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Gstr0008 = new TemplateType(287, "gstr0008", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Gstr0009 = new TemplateType(288, "gstr0009", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Gstr0010 = new TemplateType(289, "gstr0010", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Gstr0011 = new TemplateType(290, "gstr0011", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0001 = new TemplateType(291, "lwal0001", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Lwal0002 = new TemplateType(292, "lwal0002", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Lwal0003 = new TemplateType(293, "lwal0003", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Lwal0004 = new TemplateType(294, "lwal0004", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Lwal0005 = new TemplateType(295, "lwal0005", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Lwal0006 = new TemplateType(296, "lwal0006", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0007 = new TemplateType(297, "lwal0007", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0008 = new TemplateType(298, "lwal0008", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0009 = new TemplateType(299, "lwal0009", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0010 = new TemplateType(300, "lwal0010", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0011 = new TemplateType(301, "lwal0011", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0012 = new TemplateType(302, "lwal0012", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0013 = new TemplateType(303, "lwal0013", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0014 = new TemplateType(304, "lwal0014", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0015 = new TemplateType(305, "lwal0015", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0016 = new TemplateType(306, "lwal0016", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0017 = new TemplateType(307, "lwal0017", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0018 = new TemplateType(308, "lwal0018", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0019 = new TemplateType(309, "lwal0019", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0020 = new TemplateType(310, "lwal0020", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0021 = new TemplateType(311, "lwal0021", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0022 = new TemplateType(312, "lwal0022", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0023 = new TemplateType(313, "lwal0023", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0024 = new TemplateType(314, "lwal0024", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0025 = new TemplateType(315, "lwal0025", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0026 = new TemplateType(316, "lwal0026", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Lwal0027 = new TemplateType(317, "lwal0027", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Strp0001 = new TemplateType(318, "strp0001", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Strp0002 = new TemplateType(319, "strp0002", new[] { TheaterTypes.Interior }, false, null);
        public static readonly TemplateType Strp0003 = new TemplateType(320, "strp0003", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Strp0004 = new TemplateType(321, "strp0004", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Strp0005 = new TemplateType(322, "strp0005", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Strp0006 = new TemplateType(323, "strp0006", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Strp0007 = new TemplateType(324, "strp0007", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Strp0008 = new TemplateType(325, "strp0008", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Strp0009 = new TemplateType(326, "strp0009", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Strp0010 = new TemplateType(327, "strp0010", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Strp0011 = new TemplateType(328, "strp0011", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Wall0001 = new TemplateType(329, "wall0001", 1, 1, new[] { TheaterTypes.Interior }, null);
        // These 1x1 tiles get combined into three 'tile groups', making them not show up as individual tiles.
        // This reduces clutter and offers automatic randomisability to the user.
        public static readonly TemplateType Wall0002 = new TemplateType(330, "wall0002", new[] { TheaterTypes.Interior }, "wallgroup1");
        public static readonly TemplateType Wall0003 = new TemplateType(331, "wall0003", new[] { TheaterTypes.Interior }, "wallgroup2");
        public static readonly TemplateType Wall0004 = new TemplateType(332, "wall0004", new[] { TheaterTypes.Interior }, "wallgroup1");
        public static readonly TemplateType Wall0005 = new TemplateType(333, "wall0005", new[] { TheaterTypes.Interior }, "wallgroup2");
        public static readonly TemplateType Wall0006 = new TemplateType(334, "wall0006", new[] { TheaterTypes.Interior }, "wallgroup1");
        public static readonly TemplateType Wall0007 = new TemplateType(335, "wall0007", new[] { TheaterTypes.Interior }, "wallgroup2");
        public static readonly TemplateType Wall0008 = new TemplateType(336, "wall0008", new[] { TheaterTypes.Interior }, "wallgroup1");
        public static readonly TemplateType Wall0009 = new TemplateType(337, "wall0009", new[] { TheaterTypes.Interior }, "wallgroup2");
        public static readonly TemplateType Wall0010 = new TemplateType(338, "wall0010", new[] { TheaterTypes.Interior }, "wallgroup1");
        public static readonly TemplateType Wall0011 = new TemplateType(339, "wall0011", new[] { TheaterTypes.Interior }, "wallgroup2");
        public static readonly TemplateType Wall0012 = new TemplateType(340, "wall0012", new[] { TheaterTypes.Interior }, "wallgroup3");
        public static readonly TemplateType Wall0013 = new TemplateType(341, "wall0013", new[] { TheaterTypes.Interior }, "wallgroup3");
        public static readonly TemplateType Wall0014 = new TemplateType(342, "wall0014", new[] { TheaterTypes.Interior }, "wallgroup3");
        public static readonly TemplateType Wall0015 = new TemplateType(343, "wall0015", new[] { TheaterTypes.Interior }, "wallgroup3");
        public static readonly TemplateType Wall0016 = new TemplateType(344, "wall0016", new[] { TheaterTypes.Interior }, "wallgroup3");
        public static readonly TemplateType Wall0017 = new TemplateType(345, "wall0017", new[] { TheaterTypes.Interior }, "wallgroup3");
        public static readonly TemplateType Wall0018 = new TemplateType(346, "wall0018", new[] { TheaterTypes.Interior }, "wallgroup3");
        public static readonly TemplateType Wall0019 = new TemplateType(347, "wall0019", new[] { TheaterTypes.Interior }, "wallgroup1");
        public static readonly TemplateType Wall0020 = new TemplateType(348, "wall0020", new[] { TheaterTypes.Interior }, "wallgroup1");
        public static readonly TemplateType Wall0021 = new TemplateType(349, "wall0021", new[] { TheaterTypes.Interior }, "wallgroup3");
        public static readonly TemplateType Wall0022 = new TemplateType(350, "wall0022", new[] { TheaterTypes.Interior }, "wallgroup3");
        // These combine a bunch of separate 1x1 walls into one randomisable group.
        public static readonly TemplateType WallsLite = new TemplateType(0xFFFC, "wallgroup1", new[] { TheaterTypes.Interior }, true, new[] { "wall0002", "wall0004", "wall0006", "wall0008", "wall0010", "wall0019", "wall0020" });
        public static readonly TemplateType WallsDark = new TemplateType(0xFFFD, "wallgroup2", new[] { TheaterTypes.Interior }, true, new[] { "wall0003", "wall0005", "wall0007", "wall0009", "wall0011" });
        public static readonly TemplateType WallsFlat = new TemplateType(0xFFFE, "wallgroup3", new[] { TheaterTypes.Interior }, true, new[] { "wall0012", "wall0013", "wall0014", "wall0015", "wall0016", "wall0017", "wall0018", "wall0021", "wall0022" });
        public static readonly TemplateType Wall0023 = new TemplateType(351, "wall0023", 2, 2, new[] { TheaterTypes.Interior }, null, "11 10", WallCornerNorthWestInt);
        public static readonly TemplateType Wall0024 = new TemplateType(352, "wall0024", 2, 2, new[] { TheaterTypes.Interior }, null, "11 10", WallCornerNorthWestExt);
        public static readonly TemplateType Wall0025 = new TemplateType(353, "wall0025", 2, 2, new[] { TheaterTypes.Interior }, null, "11 10", WallCornerNorthWestInt);
        public static readonly TemplateType Wall0026 = new TemplateType(354, "wall0026", 2, 2, new[] { TheaterTypes.Interior }, null, "11 10", WallCornerNorthWestExt);
        public static readonly TemplateType Wall0027 = new TemplateType(355, "wall0027", 2, 2, new[] { TheaterTypes.Interior }, null, "11 01", WallCornerNorthEastInt);
        public static readonly TemplateType Wall0028 = new TemplateType(356, "wall0028", 2, 2, new[] { TheaterTypes.Interior }, null, "11 01", WallCornerNorthEastExt);
        public static readonly TemplateType Wall0029 = new TemplateType(357, "wall0029", 2, 2, new[] { TheaterTypes.Interior }, null, "11 01", WallCornerNorthEastInt);
        public static readonly TemplateType Wall0030 = new TemplateType(358, "wall0030", 2, 2, new[] { TheaterTypes.Interior }, null, "11 01", WallCornerNorthEastExt);
        public static readonly TemplateType Wall0031 = new TemplateType(359, "wall0031", 2, 2, new[] { TheaterTypes.Interior }, null, "10 11", WallCornerSouthWestInt);
        public static readonly TemplateType Wall0032 = new TemplateType(360, "wall0032", 2, 2, new[] { TheaterTypes.Interior }, null, "10 11", WallCornerSouthWestExt);
        public static readonly TemplateType Wall0033 = new TemplateType(361, "wall0033", 2, 2, new[] { TheaterTypes.Interior }, null, "10 11", WallCornerSouthWestInt);
        public static readonly TemplateType Wall0034 = new TemplateType(362, "wall0034", 2, 2, new[] { TheaterTypes.Interior }, null, "10 11", WallCornerSouthWestExt);
        public static readonly TemplateType Wall0035 = new TemplateType(363, "wall0035", 2, 2, new[] { TheaterTypes.Interior }, null, "01 11", WallCornerSouthEastInt);
        public static readonly TemplateType Wall0036 = new TemplateType(364, "wall0036", 2, 2, new[] { TheaterTypes.Interior }, null, "01 11", WallCornerSouthEastExt);
        public static readonly TemplateType Wall0037 = new TemplateType(365, "wall0037", 2, 2, new[] { TheaterTypes.Interior }, null, "01 11", WallCornerSouthEastInt);
        public static readonly TemplateType Wall0038 = new TemplateType(366, "wall0038", 2, 2, new[] { TheaterTypes.Interior }, null, "01 11", WallCornerSouthEastExt);
        public static readonly TemplateType Wall0039 = new TemplateType(367, "wall0039", 2, 3, new[] { TheaterTypes.Interior }, null, "10 11 10");
        public static readonly TemplateType Wall0040 = new TemplateType(368, "wall0040", 2, 3, new[] { TheaterTypes.Interior }, null, "10 11 10");
        public static readonly TemplateType Wall0041 = new TemplateType(369, "wall0041", 2, 3, new[] { TheaterTypes.Interior }, null, "01 11 01");
        public static readonly TemplateType Wall0042 = new TemplateType(370, "wall0042", 2, 3, new[] { TheaterTypes.Interior }, null, "01 11 01");
        public static readonly TemplateType Wall0043 = new TemplateType(371, "wall0043", 3, 2, new[] { TheaterTypes.Interior }, null, "010 111", WallTSouthInt);
        public static readonly TemplateType Wall0044 = new TemplateType(372, "wall0044", 3, 2, new[] { TheaterTypes.Interior }, null, "010 111", WallTSouthExt);
        public static readonly TemplateType Wall0045 = new TemplateType(373, "wall0045", 3, 2, new[] { TheaterTypes.Interior }, null, "010 111", WallTSouthInt);
        public static readonly TemplateType Wall0046 = new TemplateType(374, "wall0046", 3, 2, new[] { TheaterTypes.Interior }, null, "010 111", WallTSouthExt);
        public static readonly TemplateType Wall0047 = new TemplateType(375, "wall0047", 3, 2, new[] { TheaterTypes.Interior }, null, "111 010");
        public static readonly TemplateType Wall0048 = new TemplateType(376, "wall0048", 3, 2, new[] { TheaterTypes.Interior }, null, "111 010");
        public static readonly TemplateType Wall0049 = new TemplateType(377, "wall0049", 3, 3, new[] { TheaterTypes.Interior }, null, "010 111 010");
        public static readonly TemplateType Bridge1h = new TemplateType(378, "bridge1h", 5, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, new[] {"01111 11111 00111", "01111 11111 00100"});
        public static readonly TemplateType Bridge2h = new TemplateType(379, "bridge2h", 5, 2, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11110 11111");
        public static readonly TemplateType Bridge1ax = new TemplateType(380, "br1x", 5, 3, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "10011 00001 00001");
        public static readonly TemplateType Bridge2ax = new TemplateType(381, "br2x", 5, 1, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "10001");
        public static readonly TemplateType Bridge1x = new TemplateType(382, "bridge1x", 5, 4, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "00111 10000 00000 11011");
        public static readonly TemplateType Bridge2x = new TemplateType(383, "bridge2x", 5, 5, new[] { TheaterTypes.Temperate, TheaterTypes.Snow }, null, "11111 00001 00000 11111 00011");
        public static readonly TemplateType Xtra0001 = new TemplateType(384, "xtra0001", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Xtra0002 = new TemplateType(385, "xtra0002", 2, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Xtra0003 = new TemplateType(386, "xtra0003", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Xtra0004 = new TemplateType(387, "xtra0004", 1, 2, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Xtra0005 = new TemplateType(388, "xtra0005", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Xtra0006 = new TemplateType(389, "xtra0006", 2, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Xtra0007 = new TemplateType(390, "xtra0007", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Xtra0008 = new TemplateType(391, "xtra0008", 1, 2, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Xtra0009 = new TemplateType(392, "xtra0009", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Xtra0010 = new TemplateType(393, "xtra0010", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Xtra0011 = new TemplateType(394, "xtra0011", 1, 1, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Xtra0012 = new TemplateType(395, "xtra0012", 1, 2, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Xtra0013 = new TemplateType(396, "xtra0013", 1, 2, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Xtra0014 = new TemplateType(397, "xtra0014", 3, 2, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Xtra0015 = new TemplateType(398, "xtra0015", 3, 2, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType Xtra0016 = new TemplateType(399, "xtra0016", 2, 4, new[] { TheaterTypes.Interior }, null);
        public static readonly TemplateType AntHill = new TemplateType(400, "hill01", 4, 3, new[] { TheaterTypes.Temperate }, null);

        private static TemplateType[] Types;

        static TemplateTypes()
        {
            Types =
                (from field in typeof(TemplateTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
                 where field.IsInitOnly && typeof(TemplateType).IsAssignableFrom(field.FieldType)
                 select field.GetValue(null) as TemplateType).ToArray();
        }

        public static IEnumerable<TemplateType> GetTypes()
        {
            return Types;
        }
    }

    public class RaLandIniSection
    {
        public RaLandIniSection(int footSpeed, int trackSpeed, int wheelSpeed, int floatSpeed, bool buildable)
        {
            this.Foot = footSpeed;
            this.Track = trackSpeed;
            this.Wheel = wheelSpeed;
            this.Float = floatSpeed;
            this.Buildable = buildable;
        }

        [DefaultValue(0)]
        [TypeConverter(typeof(PercentageTypeConverter))]
        public int Foot { get; set; }

        [DefaultValue(0)]
        [TypeConverter(typeof(PercentageTypeConverter))]
        public int Track { get; set; }

        [DefaultValue(0)]
        [TypeConverter(typeof(PercentageTypeConverter))]
        public int Wheel { get; set; }

        [DefaultValue(0)]
        [TypeConverter(typeof(PercentageTypeConverter))]
        public int Float { get; set; }

        [TypeConverter(typeof(OneZeroBooleanTypeConverter))]
        [DefaultValue(0)]
        public bool Buildable { get; set; }
    }
}
