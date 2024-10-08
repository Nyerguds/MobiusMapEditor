﻿//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
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
using System.Linq;
using System.Reflection;

namespace MobiusEditor.Utility
{
    public class TeamRemapManager : ITeamColorManager, ITeamColorManager<TeamRemap>
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
        // Extra colors added for flags. With thanks to Kilkakon.
        public static readonly TeamRemap RemapTdBrown = new TeamRemap("MULTI7", 146, 209, 176, new byte[] { 146, 152, 209, 151, 173, 150, 173, 183, 146, 152, 209, 151, 173, 150, 173, 183 });
        public static readonly TeamRemap RemapTdBurgundy = new TeamRemap("MULTI8", 214, 213, 176, new byte[] { 132, 133, 134, 213, 214, 121, 120, 12, 133, 134, 213, 214, 121, 174, 120, 199 });
        // 'Too fleshy' according to Chad1233, so I took Burgundy instead.
        //public static readonly TeamRemap RemapTdPink = new TeamRemap("MULTI8", 217, 218, 176, new byte[] { 17, 17, 217, 218, 209, 213, 174, 120, 217, 217, 218, 209, 213, 214, 214, 174 });
        // For unowned buildings on the rebuild list.
        public static readonly TeamRemap RemapTdBlack = new TeamRemap("NONE", 199, 199, 176, new byte[] { 195, 196, 196, 13, 169, 198, 199, 112, 196, 13, 13, 169, 154, 198, 198, 199 });

        private static byte RA_BASE_INDEX = 3;

        private static readonly Dictionary<string, TeamRemap> RemapsTd;

        static TeamRemapManager()
        {
            RemapsTd = (from field in typeof(TeamRemapManager).GetFields(BindingFlags.Static | BindingFlags.Public)
                        where field.IsInitOnly && typeof(TeamRemap).IsAssignableFrom(field.FieldType) && field.Name.StartsWith("RemapTd")
                        select field.GetValue(null) as TeamRemap).ToDictionary(trm => trm.Name);
        }

        private Dictionary<string, TeamRemap> remapsRa = new Dictionary<string, TeamRemap>();
        private Dictionary<string, TeamRemap> currentRemaps = new Dictionary<string, TeamRemap>();
        private GameType currentlyLoadedGameType;
        private Color[] currentlyLoadedPalette;
        private byte currentRemapBaseIndex = 0;
        private readonly IArchiveManager mixfileManager;

        /// <summary>List of remaps found inside the palette.cps file</summary>
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

        /// <summary>Maps Red Alert's colors loaded from palette.cps to the actual remaster team color names.</summary>
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

        public ITeamColor this[string key] => GetItem(key);
        public Color GetBaseColor(string key)
        {
            if (this.currentlyLoadedPalette == null)
            {
                // Standard yellow. Identical in TD and RA, so give this as hardcoded default.
                return Color.FromArgb(246, 214, 121);
            }
            TeamRemap tc = GetItem(key);
            if (tc == null)
            {
                return this.currentlyLoadedPalette[this.currentRemapBaseIndex];
            }
            Byte[] b = new byte[1] { this.currentRemapBaseIndex };
            tc.ApplyToImage(b, 1, 1, 1, 1, null);
            return this.currentlyLoadedPalette[b[0]];
        }

        public Color RemapBaseColor => this.GetBaseColor(null);

        public void Load(string path)
        {
            currentRemaps.Clear();
            if (this.currentlyLoadedGameType != GameType.RedAlert)
            {
                currentRemaps.MergeWith(RemapsTd);
                return;
            }
            byte[] cpsFileBytes = this.mixfileManager.ReadFile(path);
            if (cpsFileBytes == null)
            {
                // Not found; ignore and do nothing. Don't reset the current remaps unless a valid cps file is actually
                // found, since the remap manager will also be requested to attempt to read the remaster team colors xml file.
                return;
            }
            byte[] cpsData = ClassicSpriteLoader.GetCpsData(cpsFileBytes, out _, false);
            if (cpsData == null)
            {
                // Not a valid CPS file; ignore and do nothing.
                return;
            }
            // CPS file found and decoded successfully; re-initialise RA remap data.
            this.remapsRa.Clear();
            int height = Math.Min(200, this.remapsColorsRa.Length);
            Dictionary<string, TeamRemap> raRemapColors = new Dictionary<string, TeamRemap>();
            byte[] remapSource = new byte[16];
            Array.Copy(cpsData, 0, remapSource, 0, 16);
            currentRemapBaseIndex = remapSource[RA_BASE_INDEX];
            for (int y = 0; y < height; ++y)
            {
                int ptr = 320 * y;
                string name = remapsColorsRa[y];
                byte[] remap = new byte[16];
                Array.Copy(cpsData, ptr, remap, 0, 16);
                // Apparently the same for units and buildings in RA.
                byte radarColor = cpsData[ptr + 6];
                TeamRemap col = new TeamRemap(name, radarColor, radarColor, remapSource, remap);
                raRemapColors.Add(name, col);
            }
            // Assign read remaps to actual remaster-named team colors.
            foreach (string col in remapsColorsRa)
            {
                string[] usedRemaps;
                TeamRemap remapColor;
                if (remapUseRa.TryGetValue(col, out usedRemaps) && raRemapColors.TryGetValue(col, out remapColor))
                {
                    for (int i = 0; i < usedRemaps.Length; ++i)
                    {
                        string actualName = usedRemaps[i];
                        TeamRemap actualCol = new TeamRemap(usedRemaps[i], remapColor);
                        this.remapsRa.Add(actualName, actualCol);
                    }
                }
            }
            currentRemaps.MergeWith(this.remapsRa);
        }

        public TeamRemapManager(IArchiveManager fileManager)
        {
            this.mixfileManager = fileManager;
            this.Reset(GameType.None, null);
        }

        public void Reset(GameType gameType, TheaterType theater)
        {
            this.currentlyLoadedGameType = gameType;
            this.currentRemaps.Clear();
            this.currentlyLoadedPalette = GetPaletteForTheater(this.mixfileManager, theater);
            this.currentRemapBaseIndex = 0;
            switch (this.currentlyLoadedGameType)
            {
                case GameType.TiberianDawn:
                case GameType.SoleSurvivor:
                    this.currentRemapBaseIndex = RemapTdGood.UnitRadarColor;
                    this.currentRemaps.MergeWith(RemapsTd);
                    break;
                case GameType.RedAlert:
                    this.currentRemapBaseIndex = (byte)(80 + RA_BASE_INDEX);
                    this.currentRemaps.MergeWith(remapsRa);
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

            byte[] pal = archiveManager.ReadFile(theater.ClassicTileset + ".pal");
            if (pal == null)
            {
                // Grayscale palette; looks awful but still allows distinguishing stuff.
                colors = Enumerable.Range(0, 0x100).Select(i => Color.FromArgb(i, i, i)).ToArray();
            }
            else
            {
                colors = ClassicSpriteLoader.LoadSixBitPalette(pal, 0, 0x100);
            }
            return colors;
        }

        public TeamRemap GetItem(string key) => !string.IsNullOrEmpty(key) && currentRemaps.ContainsKey(key) ? currentRemaps[key] : null;

        public void RemoveTeamColor(string col)
        {
            if (col != null && currentRemaps.ContainsKey(col))
            {
                currentRemaps.Remove(col);
            }
        }

        public void AddTeamColor(TeamRemap col)
        {
            if (col != null && col.Name != null)
            {
                currentRemaps[col.Name] = col;
            }
        }

        public void RemoveTeamColor(TeamRemap col)
        {
            if (col != null && col.Name != null && currentRemaps.ContainsKey(col.Name))
            {
                currentRemaps.Remove(col.Name);
            }
        }
    }
}
