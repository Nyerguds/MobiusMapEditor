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
        // The color name identifiers in the remaster are all out of wack.
        public static readonly TeamRemap RemapTdBlue = new TeamRemap("MULTI2", 2, 135, 176, new byte[] { 2, 119, 118, 135, 136, 138, 112, 12, 118, 135, 136, 137, 138, 139, 114, 112 });
        public static readonly TeamRemap RemapTdOrange = new TeamRemap("MULTI5", 24, 26, 176, new byte[] { 24, 25, 26, 27, 29, 31, 46, 47, 26, 27, 28, 29, 30, 31, 43, 47 });
        public static readonly TeamRemap RemapTdGreen = new TeamRemap("MULTI4", 167, 159, 176, new byte[] { 5, 165, 166, 167, 159, 142, 140, 199, 166, 167, 157, 3, 159, 143, 142, 141 });
        public static readonly TeamRemap RemapTdLtBlue = new TeamRemap("MULTI6", 201, 203, 176, new byte[] { 161, 200, 201, 202, 204, 205, 206, 12, 201, 202, 203, 204, 205, 115, 198, 114 });
        public static readonly TeamRemap RemapTdYellow = new TeamRemap("MULTI1", 5, 157, 176, new byte[] { 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191 });
        public static readonly TeamRemap RemapTdRed = new TeamRemap("MULTI3", 127, 123, 176, new byte[] { 127, 126, 125, 124, 122, 46, 120, 47, 125, 124, 123, 122, 42, 121, 120, 120 });
        // Added extra colours for flags
        public static readonly TeamRemap RemapTdBrown = new TeamRemap("MULTI7", 146, 209, 176, new byte[] { 146, 152, 209, 151, 173, 150, 173, 183, 146, 152, 209, 151, 173, 150, 173, 183 }); // Brown
        //public static readonly TeamRemap RemapTdBurg = new TeamRemap("Burgudy", 214, 213, 176, new byte[] { 132, 133, 134, 213, 214, 121, 120, 12, 133, 134, 213, 214, 121, 174, 120, 199 }); // Burgundy
        public static readonly TeamRemap RemapTdPink = new TeamRemap("MULTI8", 217, 218, 176, new byte[] { 17, 17, 217, 218, 209, 213, 174, 120, 217, 217, 218, 209, 213, 214, 214, 174 }); // Pink
        public static readonly TeamRemap RemapTdBlack = new TeamRemap("NONE", 199, 199, 176, new byte[] { 14, 195, 196, 13, 169, 198, 199, 112, 14, 195, 196, 13, 169, 198, 199, 112 }); // Black

        private static readonly Dictionary<string, TeamRemap> RemapsTd;

        static TeamRemapManager()
        {
            RemapsTd = (from field in typeof(ITeamColorManager).GetFields(BindingFlags.Static | BindingFlags.Public)
                        where field.IsInitOnly && typeof(TeamRemap).IsAssignableFrom(field.FieldType) && field.Name.StartsWith("RemapTd")
                        select field.GetValue(null) as TeamRemap).ToDictionary(trm => trm.Name);
        }

        private Dictionary<string, TeamRemap> remapsRa = new Dictionary<string, TeamRemap>();
        private GameType currentlyLoadedGameType;
        private Color[] currentlyLoadedPalette;
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

        public ITeamColor this[string key] => GetforCurrentGame(key);
        public Color GetBaseColor(string key)
        {
            TeamRemap tc = GetforCurrentGame(key);
            if (tc != null)
            {
                return currentlyLoadedPalette[tc.UnitRadarColor];
            }
            return RemapBaseColor;
        }

        private TeamRemap GetforCurrentGame(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            Dictionary<string, TeamRemap> currentRemaps;
            switch (currentlyLoadedGameType)
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

        private Color remapBaseColor = Color.Black;
        public Color RemapBaseColor => remapBaseColor;

        public void Load(string path)
        {
            if (currentlyLoadedGameType != GameType.RedAlert)
            {
                return;
            }
            byte[] cpsData;
            Color[] palette;
            using (Stream palettecps = mixfileManager.OpenFile(path))
            {
                if (palettecps == null)
                {
                    return;
                }
                try
                {
                    Byte[] cpsFileBytes;
                    using (BinaryReader sr = new BinaryReader(palettecps))
                    {
                        cpsFileBytes = GeneralUtils.ReadAllBytes(sr);
                    }
                    cpsData = ClassicSpriteLoader.GetCpsData(cpsFileBytes, 0, out palette);
                }
                catch (ArgumentException ex)
                {
                    return;
                }
            }
            // Data found; re-initialise RA remaps.
            this.remapsRa.Clear();
            int height = Math.Min(200, remapsColorsRa.Length);
            Dictionary<string, TeamRemap> raRemapColors = new Dictionary<string, TeamRemap>();
            byte[] remapSource = new byte[16];
            Array.Copy(cpsData, 0, remapSource, 0, 16);
            for (int y = 0; y < height; ++y)
            {
                int ptr = 320 * y;
                String name = remapsColorsRa[y];
                Byte[] remap = new byte[16];
                Array.Copy(cpsData, ptr, remap, 0, 16);
                // Apparently the same in RA?
                byte unitRadarColor = cpsData[ptr + 6];
                byte buildingRadarColor = cpsData[ptr + 6];
                TeamRemap col = new TeamRemap(name, unitRadarColor, buildingRadarColor, remapSource, remap);
                raRemapColors.Add(name, col);
            }
            foreach (String col in remapsColorsRa)
            {
                string[] usedRemaps;
                TeamRemap remapColor;
                if (remapUseRa.TryGetValue(col, out usedRemaps) && raRemapColors.TryGetValue(col, out remapColor))
                {
                    for (int i = 0; i < usedRemaps.Length; ++i)
                    {
                        String actualName = usedRemaps[i];
                        TeamRemap actualCol = new TeamRemap(usedRemaps[i], remapColor);
                        remapsRa.Add(actualName, actualCol);
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
            this.remapsRa.Clear();
            currentlyLoadedGameType = gameType;
            // file manager should already be reset to read from the correct game at this point.
            using (Stream palette = mixfileManager.OpenFile(theater.ClassicTileset + ".pal"))
            {
                if (palette == null)
                {
                    // Grayscale palette; looks awful but still allows distinguishing stuff.
                    currentlyLoadedPalette = Enumerable.Range(0, 0x100).Select(i => Color.FromArgb(i, i, i)).ToArray();
                }
                else
                {
                    byte[] pal;
                    using (BinaryReader sr = new BinaryReader(palette))
                    {
                        pal = GeneralUtils.ReadAllBytes(sr);
                    }
                    currentlyLoadedPalette = ClassicSpriteLoader.ReadSixBitPaletteAsEightBit(pal, 0, 0x100);
                }
            }
            // Set background transparent
            currentlyLoadedPalette[0] = Color.FromArgb(0x00, currentlyLoadedPalette[0]);
            // Set shadow color to semitransparent black. I'm not gonna mess around with classic fading table remapping for this.
            currentlyLoadedPalette[4] = Color.FromArgb(0x80, Color.Black);
        }
    }
}
