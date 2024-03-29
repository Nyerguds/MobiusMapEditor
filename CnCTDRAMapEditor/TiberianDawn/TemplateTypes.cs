﻿//
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
using MobiusEditor.Model;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace MobiusEditor.TiberianDawn
{
    public static class TemplateTypes
    {
        // Local shortcuts for init
        private static readonly string ThDes = TheaterTypes.Desert.Name;
        private static readonly string ThTem = TheaterTypes.Temperate.Name;
        private static readonly string ThWin = TheaterTypes.Winter.Name;
        private static readonly string ThSno = TheaterTypes.Snow.Name;

        // Equivalents groups; predefined for easy identification.
        private static readonly string[] ShoresWaterSouthTem = new[] { "sh1", "sh2", "sh5", "sh6" };
        private static readonly string[] ShoresWaterNorthTem = new[] { "sh12", "sh13", "sh14", "sh15" };
        private static readonly string[] ShoresWaterSouthDes3 = new[] { "sh19", "sh21" };
        private static readonly string[] ShoresWaterSouthDes4 = new[] { "sh20", "sh63" };
        private static readonly string[] ShoresWaterNorthDes = new[] { "sh25", "sh26", "sh28" };
        private static readonly string[] CliffsSouth = new[] { "s03", "s04", "s05" };
        private static readonly string[] CliffsWest = new[] { "s10", "s11", "s12" };
        private static readonly string[] CliffsNorth = new[] { "s17", "s18", "s19" };
        private static readonly string[] CliffsEast = new[] { "s24", "s25", "s26" };
        private static readonly string[] RoadNorthSouth = new[] { "d07", "d08" };
        private static readonly string[] RoadEastWest4 = new[] { "d09", "d10" };
        private static readonly string[] RoadEastWest2 = new[] { "d11", "d12" };
        private static readonly string[] RiverNorthSouth2 = new[] { "rv06", "rv07" };
        private static readonly string[] RiverEastWest4 = new[] { "rv14", "rv15" };
        private static readonly string[] RiverNorthSouth4 = new[] { "rv18", "rv19" };

        public static readonly TemplateType Clear = new TemplateType(0, "clear1", 1, 1, "C", TemplateTypeFlag.Clear);
        public static readonly TemplateType Water = new TemplateType(1, "w1", 1, 1, "W", TemplateTypeFlag.DefaultFill);
        public static readonly TemplateType Water2 = new TemplateType(2, "w2", 2, 2, "WW WW");
        public static readonly TemplateType Shore1 = new TemplateType(3, "sh1", 3, 3, "BBB BBB WWW", null, ShoresWaterSouthTem);
        public static readonly TemplateType Shore2 = new TemplateType(4, "sh2", 3, 3, "BBB III III", null, ShoresWaterSouthTem);
        public static readonly TemplateType Shore3 = new TemplateType(5, "sh3", 1, 1, "I");
        public static readonly TemplateType Shore4 = new TemplateType(6, "sh4", 2, 1, "II");
        public static readonly TemplateType Shore5 = new TemplateType(7, "sh5", 3, 3, "BBB BBB WWW", null, ShoresWaterSouthTem);
        public static readonly TemplateType Shore11 = new TemplateType(8, "sh11", 3, 3, "BWW BWW WWW");
        public static readonly TemplateType Shore12 = new TemplateType(9, "sh12", 3, 3, "WWW WWB WWB", null, ShoresWaterNorthTem);
        public static readonly TemplateType Shore13 = new TemplateType(10, "sh13", 3, 3, "WWW WWW BWW", null, ShoresWaterNorthTem);
        public static readonly TemplateType Shore14 = new TemplateType(11, "sh14", 3, 3, "III III BBI", null, ShoresWaterNorthTem);
        public static readonly TemplateType Shore15 = new TemplateType(12, "sh15", 3, 3, "XII III BIB", "011 111 111", ShoresWaterNorthTem);
        public static readonly TemplateType Slope1 = new TemplateType(13, "s01", 2, 2, "II CI");
        public static readonly TemplateType Slope2 = new TemplateType(14, "s02", 2, 3, "IC II II");
        public static readonly TemplateType Slope3 = new TemplateType(15, "s03", 2, 2, "II II", null, CliffsSouth);
        public static readonly TemplateType Slope4 = new TemplateType(16, "s04", 2, 2, "II II", null, CliffsSouth);
        public static readonly TemplateType Slope5 = new TemplateType(17, "s05", 2, 2, "II II", null, CliffsSouth);
        public static readonly TemplateType Slope6 = new TemplateType(18, "s06", 2, 3, "CI II IX", "11 11 10");
        public static readonly TemplateType Slope7 = new TemplateType(19, "s07", 2, 2, "II II");
        public static readonly TemplateType Slope8 = new TemplateType(20, "s08", 2, 2, "IC II");
        public static readonly TemplateType Slope9 = new TemplateType(21, "s09", 3, 2, "III CII");
        public static readonly TemplateType Slope10 = new TemplateType(22, "s10", 2, 2, "II II", null, CliffsWest);
        public static readonly TemplateType Slope11 = new TemplateType(23, "s11", 2, 2, "II II", null, CliffsWest);
        public static readonly TemplateType Slope12 = new TemplateType(24, "s12", 2, 2, "II II", null, CliffsWest);
        public static readonly TemplateType Slope13 = new TemplateType(25, "s13", 3, 2, "III IIC");
        public static readonly TemplateType Slope14 = new TemplateType(26, "s14", 2, 2, "IC CX", "11 10");
        public static readonly TemplateType Slope15 = new TemplateType(27, "s15", 2, 2, "IC II");
        public static readonly TemplateType Slope16 = new TemplateType(28, "s16", 2, 3, "IX II XI", "10 11 01");
        public static readonly TemplateType Slope17 = new TemplateType(29, "s17", 2, 2, "II II", null, CliffsNorth);
        public static readonly TemplateType Slope18 = new TemplateType(30, "s18", 2, 2, "II II", null, CliffsNorth);
        public static readonly TemplateType Slope19 = new TemplateType(31, "s19", 2, 2, "II II", null, CliffsNorth);
        public static readonly TemplateType Slope20 = new TemplateType(32, "s20", 2, 3, "XI II IC", "01 11 11");
        public static readonly TemplateType Slope21 = new TemplateType(33, "s21", 1, 2, "I I");
        public static readonly TemplateType Slope22 = new TemplateType(34, "s22", 2, 1, "II");
        public static readonly TemplateType Slope23 = new TemplateType(35, "s23", 3, 2, "III IIC");
        public static readonly TemplateType Slope24 = new TemplateType(36, "s24", 2, 2, "II II", null, CliffsEast);
        public static readonly TemplateType Slope25 = new TemplateType(37, "s25", 2, 2, "II II", null, CliffsEast);
        public static readonly TemplateType Slope26 = new TemplateType(38, "s26", 2, 2, "II II", null, CliffsEast);
        public static readonly TemplateType Slope27 = new TemplateType(39, "s27", 3, 2, "IIC CII");
        public static readonly TemplateType Slope28 = new TemplateType(40, "s28", 2, 2, "II XI", "11 01");
        public static readonly TemplateType Slope29 = new TemplateType(41, "s29", 2, 2, "II II");
        public static readonly TemplateType Slope30 = new TemplateType(42, "s30", 2, 2, "II II");
        public static readonly TemplateType Slope31 = new TemplateType(43, "s31", 2, 2, "II II");
        public static readonly TemplateType Slope32 = new TemplateType(44, "s32", 2, 2, "II IX", "11 10");
        public static readonly TemplateType Slope33 = new TemplateType(45, "s33", 2, 2, "II II");
        public static readonly TemplateType Slope34 = new TemplateType(46, "s34", 2, 2, "II II");
        public static readonly TemplateType Slope35 = new TemplateType(47, "s35", 2, 2, "II II");
        public static readonly TemplateType Slope36 = new TemplateType(48, "s36", 2, 2, "II II");
        public static readonly TemplateType Slope37 = new TemplateType(49, "s37", 2, 2, "II II");
        public static readonly TemplateType Slope38 = new TemplateType(50, "s38", 2, 2, "II II");
        public static readonly TemplateType Shore32 = new TemplateType(51, "sh32", 3, 3, "WCC CCC CCC");
        public static readonly TemplateType Shore33 = new TemplateType(52, "sh33", 3, 3, "CCW CCC CCC");
        public static readonly TemplateType Shore20 = new TemplateType(53, "sh20", 4, 1, "IIII", null, ShoresWaterSouthDes4);
        public static readonly TemplateType Shore21 = new TemplateType(54, "sh21", 3, 1, "III", null, new Point(0, 1), ShoresWaterSouthDes3);
        public static readonly TemplateType Shore22 = new TemplateType(55, "sh22", 6, 2, "XIIIXX IIIII", "011100 111111");
        public static readonly TemplateType Shore23 = new TemplateType(56, "sh23", 2, 2, "IC II");
        public static readonly TemplateType Brush1 = new TemplateType(57, "br1", 1, 1, "I");
        public static readonly TemplateType Brush2 = new TemplateType(58, "br2", 1, 1, "I");
        public static readonly TemplateType Brush3 = new TemplateType(59, "br3", 1, 1, "I");
        public static readonly TemplateType Brush4 = new TemplateType(60, "br4", 1, 1, "I");
        public static readonly TemplateType Brush5 = new TemplateType(61, "br5", 1, 1, "I");
        public static readonly TemplateType Brush6 = new TemplateType(62, "br6", 2, 2, "II II");
        public static readonly TemplateType Brush7 = new TemplateType(63, "br7", 2, 2, "II II");
        public static readonly TemplateType Brush8 = new TemplateType(64, "br8", 3, 2, "III III");
        public static readonly TemplateType Brush9 = new TemplateType(65, "br9", 3, 2, "III III");
        public static readonly TemplateType Brush10 = new TemplateType(66, "br10", 2, 1, "II");
        public static readonly TemplateType Patch1 = new TemplateType(67, "p01", 1, 1, "C");
        public static readonly TemplateType Patch2 = new TemplateType(68, "p02", 1, 1, "I");
        public static readonly TemplateType Patch3 = new TemplateType(69, "p03", 1, 1, "C");
        public static readonly TemplateType Patch4 = new TemplateType(70, "p04", 1, 1, "I");
        public static readonly TemplateType Patch5 = new TemplateType(71, "p05", 2, 2, "CC CC");
        public static readonly TemplateType Patch6 = new TemplateType(72, "p06", 6, 4, "CCCCXX XCCCXX XXCCCC XXCCCC", "111100 011100 001111 001111");
        public static readonly TemplateType Patch7 = new TemplateType(73, "p07", 4, 2, "CCCC CCCC", "1111 1111", new Dictionary<string, string>() { { ThDes, "1111 1110" } }); // Different shape in Desert.
        public static readonly TemplateType Patch8 = new TemplateType(74, "p08", 3, 2, "CCC CCC"); // Has Desert set in-game too, but no .des file exists.
        public static readonly TemplateType Shore16 = new TemplateType(75, "sh16", 3, 2, "III III");
        public static readonly TemplateType Shore17 = new TemplateType(76, "sh17", 2, 2, "WW WW");
        public static readonly TemplateType Shore18 = new TemplateType(77, "sh18", 2, 2, "WW WW");
        public static readonly TemplateType Shore19 = new TemplateType(78, "sh19", 3, 2, "XII III", "011 111", ShoresWaterSouthDes3);
        public static readonly TemplateType Patch13 = new TemplateType(79, "p13", 3, 2, "CCC CCC");
        public static readonly TemplateType Patch14 = new TemplateType(80, "p14", 2, 1, "CC");
        public static readonly TemplateType Patch15 = new TemplateType(81, "p15", 4, 2, "CCCC CCXX", "1111 1100"); // Has Temperate set in-game too, but no .tem file exists.
        public static readonly TemplateType Boulder1 = new TemplateType(82, "b1", 1, 1, "I");
        public static readonly TemplateType Boulder2 = new TemplateType(83, "b2", 2, 1, "II");
        public static readonly TemplateType Boulder3 = new TemplateType(84, "b3", 3, 1, "III");
        // These three tiles are set to Temperate in the source code, but only have graphics in Desert. However, they are remastered, and work in-game, but only in Remastered mode.
        // Since the 1.06c patch unlocks all templates for all theaters automatically, this means only the Remastered Classic Mode can't use them, so I'm enabling them in the editor.
        public static readonly TemplateType Boulder4 = new TemplateType(85, "b4", 1, 1, "I");
        public static readonly TemplateType Boulder5 = new TemplateType(86, "b5", 1, 1, "I");
        public static readonly TemplateType Boulder6 = new TemplateType(87, "b6", 1, 1, "I");
        public static readonly TemplateType Shore6 = new TemplateType(88, "sh6", 3, 3, "BBB BBB WWW", null, ShoresWaterSouthTem);
        public static readonly TemplateType Shore7 = new TemplateType(89, "sh7", 2, 2, "BW WW");
        public static readonly TemplateType Shore8 = new TemplateType(90, "sh8", 3, 3, "BBB BBB BBW");
        public static readonly TemplateType Shore9 = new TemplateType(91, "sh9", 3, 3, "BBB BBB WBB");
        public static readonly TemplateType Shore10 = new TemplateType(92, "sh10", 2, 2, "WB WW");
        public static readonly TemplateType Road1 = new TemplateType(93, "d01", 2, 2, "XC CC", "01 11");
        public static readonly TemplateType Road2 = new TemplateType(94, "d02", 2, 2, "CC CC", "11 11");
        public static readonly TemplateType Road3 = new TemplateType(95, "d03", 1, 2, "CC", "1 1");
        public static readonly TemplateType Road4 = new TemplateType(96, "d04", 2, 2, "XC CC", "01 11");
        public static readonly TemplateType Road5 = new TemplateType(97, "d05", 3, 4, "XCC CCX CCX CCX", "011 110 110 110");
        public static readonly TemplateType Road6 = new TemplateType(98, "d06", 2, 3, "CX CC CC", "10 11 11");
        public static readonly TemplateType Road7 = new TemplateType(99, "d07", 3, 2, "CCC XCC", "111 011", RoadNorthSouth);
        public static readonly TemplateType Road8 = new TemplateType(100, "d08", 3, 2, "XCXC CCC", "010 111", RoadNorthSouth);
        public static readonly TemplateType Road9 = new TemplateType(101, "d09", 4, 3, "CCCC CCCC XXCC", "1111 1111 0011", RoadEastWest4);
        public static readonly TemplateType Road10 = new TemplateType(102, "d10", 4, 2, "CCXX CCCC", "1100 1111", Point.Empty, RoadEastWest4);
        public static readonly TemplateType Road11 = new TemplateType(103, "d11", 2, 3, "XC CC CX", "01 11 10", RoadEastWest2);
        public static readonly TemplateType Road12 = new TemplateType(104, "d12", 2, 2, "CX CC", "10 11", Point.Empty, RoadEastWest2);
        public static readonly TemplateType Road13 = new TemplateType(105, "d13", 4, 3, "CCCX CCCC XXCC", "1110 1111 0011");
        public static readonly TemplateType Road14 = new TemplateType(106, "d14", 3, 3, "XCC CCC CCC", "011 111 111");
        public static readonly TemplateType Road15 = new TemplateType(107, "d15", 3, 3, "CCC CCC CCX", "111 111 110");
        public static readonly TemplateType Road16 = new TemplateType(108, "d16", 3, 3, "CCC CCC CCC");
        public static readonly TemplateType Road17 = new TemplateType(109, "d17", 3, 2, "CCC CCC");
        public static readonly TemplateType Road18 = new TemplateType(110, "d18", 3, 3, "CCC CCC CCC");
        public static readonly TemplateType Road19 = new TemplateType(111, "d19", 3, 3, "CCC CCC CCC");
        public static readonly TemplateType Road20 = new TemplateType(112, "d20", 3, 3, "CCX CCC CCC", "110 111 111");
        public static readonly TemplateType Road21 = new TemplateType(113, "d21", 3, 2, "CCC CCC");
        public static readonly TemplateType Road22 = new TemplateType(114, "d22", 3, 3, "XCX CCC CCC", "010 111 111");
        public static readonly TemplateType Road23 = new TemplateType(115, "d23", 3, 3, "XCC CCC CCX", "011 111 110");
        public static readonly TemplateType Road24 = new TemplateType(116, "d24", 3, 3, "CCX CCC XCC", "110 111 011");
        public static readonly TemplateType Road25 = new TemplateType(117, "d25", 3, 3, "CCX CCC XCC", "110 111 011");
        public static readonly TemplateType Road26 = new TemplateType(118, "d26", 2, 2, "XC CX", "01 10");
        public static readonly TemplateType Road27 = new TemplateType(119, "d27", 2, 2, "CX XC", "01 10");
        public static readonly TemplateType Road28 = new TemplateType(120, "d28", 2, 2, "CC CX", "11 10");
        public static readonly TemplateType Road29 = new TemplateType(121, "d29", 2, 2, "CC CX", "11 10");
        public static readonly TemplateType Road30 = new TemplateType(122, "d30", 2, 2, "CC CX", "11 10");
        public static readonly TemplateType Road31 = new TemplateType(123, "d31", 2, 2, "XC CC", "01 11");
        public static readonly TemplateType Road32 = new TemplateType(124, "d32", 2, 2, "XC CC", "01 11");
        public static readonly TemplateType Road33 = new TemplateType(125, "d33", 2, 2, "XC CC", "01 11");
        public static readonly TemplateType Road34 = new TemplateType(126, "d34", 3, 3, "XCC CCC CCX", "011 111 110");
        public static readonly TemplateType Road35 = new TemplateType(127, "d35", 3, 3, "XCC CCC CCX", "011 111 110");
        public static readonly TemplateType Road36 = new TemplateType(128, "d36", 2, 2, "CX XC", "10 01");
        public static readonly TemplateType Road37 = new TemplateType(129, "d37", 2, 2, "CX XC", "10 01");
        public static readonly TemplateType Road38 = new TemplateType(130, "d38", 2, 2, "CC XC", "11 01");
        public static readonly TemplateType Road39 = new TemplateType(131, "d39", 2, 2, "CC XC", "11 01");
        public static readonly TemplateType Road40 = new TemplateType(132, "d40", 2, 2, "CC XC", "11 01");
        public static readonly TemplateType Road41 = new TemplateType(133, "d41", 2, 2, "CX CC", "10 11");
        public static readonly TemplateType Road42 = new TemplateType(134, "d42", 2, 2, "CX CC", "10 11");
        public static readonly TemplateType Road43 = new TemplateType(135, "d43", 2, 2, "CX CC", "10 11");
        public static readonly TemplateType River1 = new TemplateType(136, "rv01", 5, 4, "WWXXX WWWWW WWWWW WWWXX", "11000 11111 11111 01100");
        public static readonly TemplateType River2 = new TemplateType(137, "rv02", 5, 3, "WWWWW WWWWW WWWXX", "11111 11111 11100");
        public static readonly TemplateType River3 = new TemplateType(138, "rv03", 4, 4, "WWXX WWwC WWWW XWWW", "1100 1111 1111 0111");
        public static readonly TemplateType River4 = new TemplateType(139, "rv04", 4, 4, "XXWW XWWW WWWW WWWX", "0011 0111 1111 1110");
        public static readonly TemplateType River5 = new TemplateType(140, "rv05", 3, 3, "WWW WWW WWW");
        public static readonly TemplateType River6 = new TemplateType(141, "rv06", 3, 2, "WWW WWW", null, RiverNorthSouth2);
        public static readonly TemplateType River7 = new TemplateType(142, "rv07", 3, 2, "WWW WWW", null, RiverNorthSouth2);
        public static readonly TemplateType River8 = new TemplateType(143, "rv08", 2, 2, "WW WW");
        public static readonly TemplateType River9 = new TemplateType(144, "rv09", 2, 2, "WW WW");
        public static readonly TemplateType River10 = new TemplateType(145, "rv10", 2, 2, "WW WW");
        public static readonly TemplateType River11 = new TemplateType(146, "rv11", 2, 2, "WW WW");
        public static readonly TemplateType River12 = new TemplateType(147, "rv12", 3, 4, "WWW WWW WWW WWW");
        public static readonly TemplateType River13 = new TemplateType(148, "rv13", 4, 4, "XXWW WWWW WWWW, WWWW", "0011 1111 1111, 1111");
        public static readonly TemplateType River14 = new TemplateType(149, "rv14", 4, 3, "WWWW WWWW WWWW", null, RiverEastWest4);
        public static readonly TemplateType River15 = new TemplateType(150, "rv15", 4, 3, "WWWW WWWW WWWW", null, RiverEastWest4);
        public static readonly TemplateType River16 = new TemplateType(151, "rv16", 6, 4, "XXWWWW XWWWWW WWWWWW WWWWXX", "001111 011111 111111 111100");
        public static readonly TemplateType River17 = new TemplateType(152, "rv17", 6, 5, "WWWWXX WWWWWX WWWWWW WWWWWW XXXXWW", "111100 111110 111111 111111 000011");
        public static readonly TemplateType River18 = new TemplateType(153, "rv18", 4, 4, "WWWW WWWX WWWX WWWX", "1111 1110 1110 1110", new Point(1, 0), RiverNorthSouth4);
        public static readonly TemplateType River19 = new TemplateType(154, "rv19", 4, 4, "XWWW XWWW WWWW WWWW", "0111 0111 1111 1111", Point.Empty, RiverNorthSouth4);
        public static readonly TemplateType River20 = new TemplateType(155, "rv20", 6, 8, "XXWWWW XWWWWX WWWWWX WWWWWX WWWWWX WWWWXX WWWWXX WWWWXX", "001111 011110 111110 111110 111110 111100 111100 111100");
        public static readonly TemplateType River21 = new TemplateType(156, "rv21", 5, 8, "WWWWX WWWWX XWWWX XWWWW XWWWW XXWWW XXWWW XXXWW", "11110 11110 01110 01111 01111 00111 00111 00011");
        public static readonly TemplateType River22 = new TemplateType(157, "rv22", 3, 3, "XWW WWW WWW", "011 111 111");
        public static readonly TemplateType River23 = new TemplateType(158, "rv23", 3, 3, "WWW WWW WWW");
        public static readonly TemplateType River24 = new TemplateType(159, "rv24", 3, 3, "WWW WWW XWW", "111 111 011");
        public static readonly TemplateType River25 = new TemplateType(160, "rv25", 3, 3, "WWW WWW WWW");
        public static readonly TemplateType Ford1 = new TemplateType(161, "ford1", 3, 3, "WWC CCC WWC");
        public static readonly TemplateType Ford2 = new TemplateType(162, "ford2", 3, 3, "CCC WCW WCC");
        public static readonly TemplateType Falls1 = new TemplateType(163, "falls1", 3, 3, "CWW WWW WWW");
        public static readonly TemplateType Falls2 = new TemplateType(164, "falls2", 3, 2, "WWW WWW");
        public static readonly TemplateType Bridge1 = new TemplateType(165, "bridge1", 4, 4, "XXCC WWCW WCWW CCWW", "0011 1111 1111 1111");
        public static readonly TemplateType Bridge1d = new TemplateType(166, "bridge1d", 4, 4, "XXCC WWWW WWWW CCWW", "0011 1111 1111 1111");
        public static readonly TemplateType Bridge2 = new TemplateType(167, "bridge2", 5, 5, "CCWXW WCWWW WWCWW WWWCC XXXCC", "11101 11111 11111 11111 00011");
        public static readonly TemplateType Bridge2d = new TemplateType(168, "bridge2d", 5, 5, "CCWXW WWWWW WWWWW WWWCC XXXCC", "11101 11111 11111 11111 00011");
        public static readonly TemplateType Bridge3 = new TemplateType(169, "bridge3", 6, 5, "XXXCCX CWWCWC WWCWWW WCWWWW CCXXXX", "000110 111111 111111 111111 110000");
        public static readonly TemplateType Bridge3d = new TemplateType(170, "bridge3d", 6, 5, "XXXCCX CWWWWC WWWWWW WCWWWW WCXXXX", "000110 111111 111111 111111 110000");
        public static readonly TemplateType Bridge4 = new TemplateType(171, "bridge4", 6, 4, "XCWWWW WWCWWW WWWCWW WWWWCX", "011111 111111 111111 111110");
        public static readonly TemplateType Bridge4d = new TemplateType(172, "bridge4d", 6, 4, "XCWWWW WWWWWW WWWWWW WWWWCX", "011111 111111 111111 111110");
        public static readonly TemplateType Shore24 = new TemplateType(173, "sh24", 3, 3, "III IIC III");
        public static readonly TemplateType Shore25 = new TemplateType(174, "sh25", 3, 2, "III CII", null, ShoresWaterNorthDes);
        public static readonly TemplateType Shore26 = new TemplateType(175, "sh26", 3, 2, "III XII", "111 011", ShoresWaterNorthDes);
        public static readonly TemplateType Shore27 = new TemplateType(176, "sh27", 4, 1, "III III");
        public static readonly TemplateType Shore28 = new TemplateType(177, "sh28", 3, 1, "III", (string)null, Point.Empty, ShoresWaterNorthDes);
        public static readonly TemplateType Shore29 = new TemplateType(178, "sh29", 6, 2, "IIIIII XCIIXX", "111111 011100");
        public static readonly TemplateType Shore30 = new TemplateType(179, "sh30", 2, 2, "II IX", "11 10");
        public static readonly TemplateType Shore31 = new TemplateType(180, "sh31", 3, 3, "III III III");
        public static readonly TemplateType Patch16 = new TemplateType(181, "p16", 2, 2, "CC CC");
        public static readonly TemplateType Patch17 = new TemplateType(182, "p17", 4, 2, "XXCC CCCC", "0011 1111");
        public static readonly TemplateType Patch18 = new TemplateType(183, "p18", 4, 3, "CCXX CCCC XXCC", "1100 1111 0011");
        public static readonly TemplateType Patch19 = new TemplateType(184, "p19", 4, 3, "XXCC CCCC CCXX", "0011 1111 1100");
        public static readonly TemplateType Patch20 = new TemplateType(185, "p20", 4, 3, "CCCC XXCC XXCC", "1111 0011 0011");
        public static readonly TemplateType Shore34 = new TemplateType(186, "sh34", 3, 3, "BBW BBW BBW");
        public static readonly TemplateType Shore35 = new TemplateType(187, "sh35", 3, 3, "WBB WBB WBB");
        public static readonly TemplateType Shore36 = new TemplateType(188, "sh36", 1, 1, "C");
        public static readonly TemplateType Shore37 = new TemplateType(189, "sh37", 1, 1, "C");
        public static readonly TemplateType Shore38 = new TemplateType(190, "sh38", 1, 1, "C");
        public static readonly TemplateType Shore39 = new TemplateType(191, "sh39", 1, 1, "C");
        public static readonly TemplateType Shore40 = new TemplateType(192, "sh40", 3, 3, "CWW WWW WWW");
        public static readonly TemplateType Shore41 = new TemplateType(193, "sh41", 3, 3, "WWC WWC WCC");
        public static readonly TemplateType Shore42 = new TemplateType(194, "sh42", 1, 2, "W W");
        public static readonly TemplateType Shore43 = new TemplateType(195, "sh43", 1, 3, "W W W");
        public static readonly TemplateType Shore44 = new TemplateType(196, "sh44", 1, 3, "W W W");
        public static readonly TemplateType Shore45 = new TemplateType(197, "sh45", 1, 2, "W W");
        public static readonly TemplateType Shore46 = new TemplateType(198, "sh46", 3, 3, "CCW CWW WWW");
        public static readonly TemplateType Shore47 = new TemplateType(199, "sh47", 3, 3, "WWX WWX WWW", "110 110 111");
        public static readonly TemplateType Shore48 = new TemplateType(200, "sh48", 3, 3, "WXX WXX WWW", "100 100 111");
        public static readonly TemplateType Shore49 = new TemplateType(201, "sh49", 3, 3, "XWW WWW WWW", "011 111 111");
        public static readonly TemplateType Shore50 = new TemplateType(202, "sh50", 4, 3, "XWWW WWWX WWXX", "0111 1110 1100");
        public static readonly TemplateType Shore51 = new TemplateType(203, "sh51", 4, 3, "WW00 WWW0 WWWW", "1100 1110 1111");
        public static readonly TemplateType Shore52 = new TemplateType(204, "sh52", 4, 3, "WWWW XXWW XXWW", "1111 0011 0011");
        public static readonly TemplateType Shore53 = new TemplateType(205, "sh53", 4, 3, "CCCW CWWW WWWW");
        public static readonly TemplateType Shore54 = new TemplateType(206, "sh54", 3, 2, "WWW WWW");
        public static readonly TemplateType Shore55 = new TemplateType(207, "sh55", 3, 2, "WWW WWW");
        public static readonly TemplateType Shore56 = new TemplateType(208, "sh56", 3, 2, "XWW WWW", "011 111");
        public static readonly TemplateType Shore57 = new TemplateType(209, "sh57", 3, 2, "WXX WWW", "100 111");
        public static readonly TemplateType Shore58 = new TemplateType(210, "sh58", 2, 3, "WX WW WW", "10 11 11");
        public static readonly TemplateType Shore59 = new TemplateType(211, "sh59", 2, 3, "CW WW WW");
        public static readonly TemplateType Shore60 = new TemplateType(212, "sh60", 2, 3, "WW WC WC");
        public static readonly TemplateType Shore61 = new TemplateType(213, "sh61", 2, 3, "WC WW XW", "11 11 01");
        public static readonly TemplateType Shore62 = new TemplateType(214, "sh62", 6, 1, "WWWWWW");
        public static readonly TemplateType Shore63 = new TemplateType(215, "sh63", 4, 1, "WWWW", null, ShoresWaterSouthDes4);
        // Test. Don't use; the game has Temperate graphics for these but no IDs.
        //public static readonly TemplateType SRoad1 = new TemplateType(216, "sr1", 1, 1, "C");
        //public static readonly TemplateType SRoad2 = new TemplateType(217, "sr2", 2, 1, "CC");

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
}
