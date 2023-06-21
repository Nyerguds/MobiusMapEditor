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
using MobiusEditor.Interface;
using MobiusEditor.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MobiusEditor.Utility
{
    class TeamRemapManager : ITeamColorManager
    {
        // TD remap
        public static readonly TeamRemap RemapTdGood = new TeamRemap("GOOD", 176, 180, 176, new byte[] { 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191 });
        public static readonly TeamRemap RemapTdBadRed = new TeamRemap("BAD_STRUCTURE", 127, 123, 176, new byte[] { 127, 126, 125, 124, 122, 46, 120, 47, 125, 124, 123, 122, 42, 121, 120, 120 });
        public static readonly TeamRemap RemapTdBadGray = new TeamRemap("BAD_UNIT", 127, 123, 176, new byte[] { 161, 200, 201, 202, 204, 205, 206, 12, 201, 202, 203, 204, 205, 115, 198, 114 });
        // The color name identifiers in the remaster are all out of wack because they synced them with the RA ones.
        public static readonly TeamRemap RemapTdTealBlue = new TeamRemap("MULTI2", 2, 135, 176, new byte[] { 2, 119, 118, 135, 136, 138, 112, 12, 118, 135, 136, 137, 138, 139, 114, 112 });
        public static readonly TeamRemap RemapTdOrange = new TeamRemap("MULTI5", 24, 26, 176, new byte[] { 24, 25, 26, 27, 29, 31, 46, 47, 26, 27, 28, 29, 30, 31, 43, 47 });
        public static readonly TeamRemap RemapTdGreen = new TeamRemap("MULTI4", 167, 159, 176, new byte[] { 5, 165, 166, 167, 159, 142, 140, 199, 166, 167, 157, 3, 159, 143, 142, 141 });
        public static readonly TeamRemap RemapTdLtBlue = new TeamRemap("MULTI6", 201, 203, 176, new byte[] { 161, 200, 201, 202, 204, 205, 206, 12, 201, 202, 203, 204, 205, 115, 198, 114 });
        public static readonly TeamRemap RemapTdYellow = new TeamRemap("MULTI1", 5, 157, 176, new byte[] { 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191 });
        public static readonly TeamRemap RemapTdRed = new TeamRemap("MULTI3", 127, 123, 176, new byte[] { 127, 126, 125, 124, 122, 46, 120, 47, 125, 124, 123, 122, 42, 121, 120, 120 });
        // Extra colours added for flags. With thanks to Kilkakon.
        public static readonly TeamRemap RemapTdBrown = new TeamRemap("MULTI7", 146, 209, 176, new byte[] { 146, 152, 209, 151, 173, 150, 173, 183, 146, 152, 209, 151, 173, 150, 173, 183 });
        public static readonly TeamRemap RemapTdBurg = new TeamRemap("MULTI8", 214, 213, 176, new byte[] { 132, 133, 134, 213, 214, 121, 120, 12, 133, 134, 213, 214, 121, 174, 120, 199 });
        // 'Too fleshy' according to Chad1233, so I took Burgundy instead.
        //public static readonly TeamRemap RemapTdPink = new TeamRemap("MULTI8", 217, 218, 176, new byte[] { 17, 17, 217, 218, 209, 213, 174, 120, 217, 217, 218, 209, 213, 214, 214, 174 });
        // For unowned buildings on the rebuild list.
        public static readonly TeamRemap RemapTdBlack = new TeamRemap("NONE", 199, 199, 176, new byte[] { 195, 196, 196, 13, 169, 198, 199, 112, 196, 13, 13, 169, 154, 198, 198, 199 });

        private static readonly Dictionary<string, TeamRemap> RemapsTd;

        static TeamRemapManager()
        {
            RemapsTd = (from field in typeof(TeamRemapManager).GetFields(BindingFlags.Static | BindingFlags.Public)
                        where field.IsInitOnly && typeof(TeamRemap).IsAssignableFrom(field.FieldType) && field.Name.StartsWith("RemapTd")
                        select field.GetValue(null) as TeamRemap).ToDictionary(trm => trm.Name);
        }

        private Dictionary<string, TeamRemap> remapsRa = new Dictionary<string, TeamRemap>();
        private GameType currentlyLoadedGameType;
        private Color[] currentlyLoadedPalette;
        private byte currentRemapBaseIndex = 0;
        private readonly IArchiveManager mixfileManager;
        private readonly string[] remapsColorsRa =
            {
                "GOLD",
                "LTBLUE",
                "RED",
                "GREEN",
                "ORANGE",
	            "GREY",
                "BLUE",
                "BROWN",
                //"TYPE",
                //"REALLY_BLUE",
                //"DIALOG_BLUE",
        };

        private readonly Dictionary<string, string[]> remapUseRa = new Dictionary<string, string[]> {
            { "GOLD", new string[]{ "SPAIN", "NEUTRAL", "SPECIAL" , "MULTI1" } },
            { "LTBLUE", new string[]{ "GREECE", "GOOD", "MULTI2" } },
            { "RED", new string[]{ "USSR", "BAD", "MULTI3" } },
            { "GREEN", new string[]{ "ENGLAND", "MULTI4" } },
            { "ORANGE", new string[]{ "UKRAINE", "MULTI5" } },
            { "GREY", new string[]{ "GERMANY", "MULTI6" } },
            { "BLUE", new string[]{ "FRANCE", "MULTI7" } },
            { "BROWN", new string[]{ "TURKEY", "MULTI8" } },
            //{ "TYPE", new string[]{ } },
            //{ "REALLY_BLUE", new string[]{ } },
            //{ "DIALOG_BLUE", new string[]{ } },
        };

        public ITeamColor this[string key] => this.GetforCurrentGame(key);
        public Color GetBaseColor(string key)
        {
            if (this.currentlyLoadedPalette == null)
            {
                // Standard yellow. Identical in TD and RA, so give this as hardcoded default.
                return Color.FromArgb(246, 214, 121);
            }
            TeamRemap tc = this.GetforCurrentGame(key);
            if (tc == null)
            {
                return this.currentlyLoadedPalette[this.currentRemapBaseIndex];
            }
            Byte[] b = new byte[1] { this.currentRemapBaseIndex };
            tc.ApplyToImage(b, 1, 1, 1, 1, null);
            return this.currentlyLoadedPalette[b[0]];
            
        }

        public Color RemapBaseColor => this.GetBaseColor(null);

        private TeamRemap GetforCurrentGame(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            Dictionary<string, TeamRemap> currentRemaps;
            switch (this.currentlyLoadedGameType)
            {
                case GameType.TiberianDawn:
                case GameType.SoleSurvivor:
                    currentRemaps = RemapsTd;
                    break;
                case GameType.RedAlert:
                    currentRemaps = this.remapsRa;
                    break;
                default:
                    return null;
            }
            return currentRemaps.ContainsKey(key) ? currentRemaps[key] : null;
        }

        public void Load(string path)
        {
            if (this.currentlyLoadedGameType != GameType.RedAlert)
            {
                return;
            }
            byte[] cpsData;
            using (Stream palettecps = this.mixfileManager.OpenFile(path))
            {
                if (palettecps == null)
                {
                    // Not found; ignore and do nothing.
                    return;
                }
                try
                {
                    Byte[] cpsFileBytes;
                    using (BinaryReader sr = new BinaryReader(palettecps))
                    {
                        cpsFileBytes = GeneralUtils.ReadAllBytes(sr);
                    }
                    cpsData = ClassicSpriteLoader.GetCpsData(cpsFileBytes, out _);
                }
                catch (ArgumentException)
                {
                    // Not a valid CPS file; ignore and do nothing.
                    return;
                }
            }
            // CPS file found and decoded successfully; re-initialise RA remap data.
            this.remapsRa.Clear();
            int height = Math.Min(200, this.remapsColorsRa.Length);
            Dictionary<string, TeamRemap> raRemapColors = new Dictionary<string, TeamRemap>();
            byte[] remapSource = new byte[16];
            Array.Copy(cpsData, 0, remapSource, 0, 16);
            // Taking brightest colour here, not unit/structure colour.
            bool baseRemapSet = false;
            for (int y = 0; y < height; ++y)
            {
                int ptr = 320 * y;
                String name = this.remapsColorsRa[y];
                Byte[] remap = new byte[16];
                Array.Copy(cpsData, ptr, remap, 0, 16);
                // Apparently the same in RA?
                byte unitRadarColor = cpsData[ptr + 6];
                byte buildingRadarColor = cpsData[ptr + 6];
                if (!baseRemapSet)
                {
                    currentRemapBaseIndex = buildingRadarColor;
                    baseRemapSet = true;
                }
                TeamRemap col = new TeamRemap(name, unitRadarColor, buildingRadarColor, remapSource, remap);
                raRemapColors.Add(name, col);
            }
            foreach (String col in this.remapsColorsRa)
            {
                string[] usedRemaps;
                TeamRemap remapColor;
                if (this.remapUseRa.TryGetValue(col, out usedRemaps) && raRemapColors.TryGetValue(col, out remapColor))
                {
                    for (int i = 0; i < usedRemaps.Length; ++i)
                    {
                        String actualName = usedRemaps[i];
                        TeamRemap actualCol = new TeamRemap(usedRemaps[i], remapColor);
                        this.remapsRa.Add(actualName, actualCol);
                    }
                }
            }
        }

        public TeamRemapManager(IArchiveManager fileManager)
        {
            this.mixfileManager = fileManager;
        }

        public void Reset(GameType gameType, TheaterType theater)
        {
            // Need to be re-fetched from palette.cps after the reset.
            this.remapsRa.Clear();
            this.currentlyLoadedGameType = gameType;
            this.currentlyLoadedPalette = GetPaletteForTheater(this.mixfileManager, theater);
            switch (this.currentlyLoadedGameType)
            {
                case GameType.TiberianDawn:
                case GameType.SoleSurvivor:
                    this.currentRemapBaseIndex = RemapTdGood.BuildingRadarColor;
                    break;
                case GameType.RedAlert:
                    this.currentRemapBaseIndex = 86;
                    break;
                default:
                    this.currentRemapBaseIndex = 0;
                    break;
            }
        }

        public static Color[] GetPaletteForTheater(IArchiveManager archiveManager, TheaterType theater)
        {
            Color[] colors;
            if (theater == null)
            {
                return Enumerable.Range(0, 0x100).Select(i => Color.FromArgb(i, i, i)).ToArray();
            }
            // file manager should already be reset to read from the correct game at this point.
            using (Stream palette = archiveManager.OpenFile(theater.ClassicTileset + ".pal"))
            {
                if (palette == null)
                {
                    // Grayscale palette; looks awful but still allows distinguishing stuff.
                    colors = Enumerable.Range(0, 0x100).Select(i => Color.FromArgb(i, i, i)).ToArray();
                }
                else
                {
                    byte[] pal;
                    using (BinaryReader sr = new BinaryReader(palette))
                    {
                        pal = GeneralUtils.ReadAllBytes(sr);
                    }
                    colors = ClassicSpriteLoader.LoadSixBitPalette(pal, 0, 0x100);
                }
            }
            // Set background transparent
            colors[0] = Color.FromArgb(0x00, colors[0]);
            // Set shadow color to semitransparent black. I'm not gonna mess around with classic fading table remapping for this.
            colors[4] = Color.FromArgb(0x80, Color.Black);
            return colors;
        }
    }
}
