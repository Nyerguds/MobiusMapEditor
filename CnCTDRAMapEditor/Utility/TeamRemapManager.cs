using MobiusEditor.Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MobiusEditor.Utility
{
    class TeamRemapManager : ITeamColorManager
    {
        // TD remap
        public static readonly TeamRemap RemapTdGoodGuy = new TeamRemap("GOOD", 176, 180, 176, new byte[] { 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191 });
        public static readonly TeamRemap RemapTdBadGuyRed = new TeamRemap("BAD_STRUCTURE", 127, 123, 176, new byte[] { 127, 126, 125, 124, 122, 46, 120, 47, 125, 124, 123, 122, 42, 121, 120, 120 });
        public static readonly TeamRemap RemapTdBadGuyGray = new TeamRemap("BAD_UNIT", 127, 123, 176, new byte[] { 161, 200, 201, 202, 204, 205, 206, 12, 201, 202, 203, 204, 205, 115, 198, 114 });
        // The color name identifiers in the remaster are all out of wack.
        public static readonly TeamRemap RemapTdBlue = new TeamRemap("MULTI2", 2, 135, 176, new byte[] {2, 119, 118, 135, 136, 138, 112, 12, 118, 135, 136, 137, 138, 139, 114, 112});
        public static readonly TeamRemap RemapTdOrange = new TeamRemap("MULTI5", 24, 26, 176, new byte[] { 24, 25, 26, 27, 29, 31, 46, 47, 26, 27, 28, 29, 30, 31, 43, 47 });
        public static readonly TeamRemap RemapTdGreen = new TeamRemap("MULTI4", 167, 159, 176, new byte[] { 5, 165, 166, 167, 159, 142, 140, 199, 166, 167, 157, 3, 159, 143, 142, 141 });
        public static readonly TeamRemap RemapTdLtBlue = new TeamRemap("MULTI6", 201, 203, 176, new byte[] { 161, 200, 201, 202, 204, 205, 206, 12, 201, 202, 203, 204, 205, 115, 198, 114 });
        public static readonly TeamRemap RemapTdYellow = new TeamRemap("MULTI1", 5, 157, 176, new byte[] { 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191 });
        public static readonly TeamRemap RemapTdRed = new TeamRemap("MULTI3", 127, 123, 176, new byte[] { 127, 126, 125, 124, 122, 46, 120, 47, 125, 124, 123, 122, 42, 121, 120, 120 });
        // Added extra colours for flags
        public static readonly TeamRemap RemapTdMul7 = new TeamRemap("MULTI7", 146, 209, 176, new byte[] { 146, 152, 209, 151, 173, 150, 173, 183, 146, 152, 209, 151, 173, 150, 173, 183 }); // Brown
        public static readonly TeamRemap RemapTdMul8 = new TeamRemap("MULTI8", 214, 213, 176, new byte[] { 132, 133, 134, 213, 214, 121, 120, 12, 133, 134, 213, 214, 121, 174, 120, 199 }); // Burgundy
        public static readonly TeamRemap RemapTdNone = new TeamRemap("NONE", 199, 199, 176, new byte[] { 14, 195, 196, 13, 169, 198, 199, 112, 14, 195, 196, 13, 169, 198, 199, 112 }); // Black


        private static readonly Dictionary<string, TeamRemap> RemapsTd;

        public const Byte MaxValueSixBit = 63;
        private static readonly Byte[] ConvertToEightBit = new Byte[64];

        static TeamRemapManager()
        {
            RemapsTd = (from field in typeof(ITeamColorManager).GetFields(BindingFlags.Static | BindingFlags.Public)
                        where field.IsInitOnly && typeof(TeamRemap).IsAssignableFrom(field.FieldType) && field.Name.StartsWith("RemapTd")
                        select field.GetValue(null) as TeamRemap).ToDictionary(trm => trm.Name);

            // Build easy lookup tables for this, so no calculations are ever needed for this later.
            for (Int32 i = 0; i < 64; ++i)
                ConvertToEightBit[i] = (Byte)Math.Round(i * 255.0 / 63.0, MidpointRounding.ToEven);
        }

        private Dictionary<string, TeamRemap> remapsRa = new Dictionary<string, TeamRemap>();
        private GameType currentlyLoadedGameType;
        private Color[] currentlyLoadedPalette;
        private readonly IArchiveManager mixfileManager;


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
            // Check if CPS, if so, fill in "this.remapsRa"
        }

        public TeamRemapManager(IArchiveManager fileManager)
        {
            this.mixfileManager = fileManager;
        }

        public void Reset(GameType gameType, string theater)
        {
            currentlyLoadedGameType = gameType;
            currentlyLoadedPalette = new Color[0x100];
            // file manager should already be reset to read from the correct game at this point.
            using (Stream palette = mixfileManager.OpenFile(theater + ".pal"))
            {
                if (palette != null)
                {
                    byte[] pal;
                    using (BinaryReader sr = new BinaryReader(palette))
                    {
                        pal = GeneralUtils.ReadAllBytes(sr);
                    }
                    int len = Math.Min(pal.Length / 3, 0x100);
                    int offs = 0;
                    for (int i = 0; i < len; ++i)
                    {
                        byte r = ConvertToEightBit[pal[offs + 0] & 0x3F];
                        byte g = ConvertToEightBit[pal[offs + 1] & 0x3F];
                        byte b = ConvertToEightBit[pal[offs + 2] & 0x3F];
                        currentlyLoadedPalette[i] = Color.FromArgb(r, g, b);
                            offs += 3;
                    }
                }
            }
            if (gameType == GameType.TiberianDawn || gameType == GameType.SoleSurvivor)
            {

            }
        }

        public void setTheater()
        {

        }
    }
}
