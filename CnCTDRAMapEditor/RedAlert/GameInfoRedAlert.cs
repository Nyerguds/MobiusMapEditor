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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;

namespace MobiusEditor.RedAlert
{
    public class GameInfoRedAlert : GameInfo
    {
        public override GameType GameType => GameType.RedAlert;
        public override string Name => "Red Alert";
        public override string ShortName => "RA";
        public override string IniName => "RedAlert";
        public override string DefaultSaveDirectory => Path.Combine(Globals.RootSaveDirectory, "Red_Alert");
        public override string SaveFilter => "Red Alert files (*.mpr;*.ini)|*.mpr;*.ini";
        public override string OpenFilter => "Red Alert files (*.mpr;*.ini)|*.mpr;*.ini";
        public override string DefaultExtension => ".mpr";
        public override string DefaultExtensionFromMix => ".ini";
        public override string DefaultExtensionFromPgm => DefaultExtension;
        public override Dictionary<FileType, string[]> ExtensionsForTypes => 
            new Dictionary<FileType, string[]>() { { FileType.INI, new string[] { ".ini", ".mpr" } }, { FileType.PGM, new string[] { ".pgm", ".meg" } } };
        public override string ModFolder => Path.Combine(Globals.ModDirectory, "Red_Alert");
        public override string ModIdentifier => "RA";
        public override string ModsToLoad => Properties.Settings.Default.ModsToLoadRA;
        public override string ModsToLoadSetting => "ModsToLoadRA";
        public override string WorkshopTypeId => "RA";
        public override string[] RemasterMegFiles => new string[] {"CONFIG.MEG", "TEXTURES_COMMON_SRGB.MEG", "TEXTURES_SRGB.MEG", "TEXTURES_RA_SRGB.MEG" };
        public override string ClassicFolder => Properties.Settings.Default.ClassicPathRA;
        public override string ClassicFolderRemaster => "CNCDATA\\RED_ALERT";
        public override string ClassicFolderRemasterData => ClassicFolderRemaster + "\\AFTERMATH";
        public override string ClassicFolderDefault => "Classic\\RA\\";
        public override string ClassicFolderSetting => "ClassicPathRA";
        public override string ClassicStringsFile => "conquer.eng";
        public override Size MapSize => Constants.MaxSize;
        public override Size MapSizeMega => Constants.MaxSize;
        public override TheaterType[] AllTheaters => TheaterTypes.GetAllTypes().ToArray();
        public override TheaterType[] AvailableTheaters => TheaterTypes.GetTypes().ToArray();
        public override bool MegamapIsSupported => true;
        public override bool MegamapIsOptional => false;
        public override bool MegamapIsDefault => true;
        public override bool MegamapIsOfficial => true;
        public override bool HasSinglePlayer => true;
        public override bool CanUseNewMixFormat => true;
        public override long MaxDataSize => Globals.MaxMapSize;
        public override int MaxTriggers => Constants.MaxTriggers;
        public override int MaxTeams => Constants.MaxTeams;
        public override int HitPointsGreenMinimum => 128;
        public override int HitPointsYellowMinimum => 64;
        public override OverlayTypeFlag OverlayIconType => OverlayTypeFlag.Crate;

        public override IGamePlugin CreatePlugin(Boolean mapImage, Boolean megaMap) => new GamePluginRA(mapImage);

        public override void InitClassicFiles(MixfileManager mfm, List<string> loadErrors, List<string> fileLoadErrors, bool forRemaster)
        {
            mfm.Reset(GameType.None, null);
            // Aftermath expand file. Contains latest strings file and the expansion vehicle graphics.
            mfm.LoadArchive(GameType.RedAlert, "expand2.mix", false, false, false, true);
            // Counterstrike expand file. All graphics from expand are also in expand2.mix,
            // but it could be used in modding to override different files. Not considered vital.
            mfm.LoadArchive(GameType.RedAlert, "expand.mix", false, false, false, true);
            // Container archives.
            mfm.LoadArchive(GameType.RedAlert, "redalert.mix", false, true, false, true);
            mfm.LoadArchive(GameType.RedAlert, "main.mix", false, true, false, true);
            // Needed for theater palettes and the remap settings in palette.cps
            mfm.LoadArchive(GameType.RedAlert, "local.mix", false, false, true, true);
            // Mod addons. Loaded with a special function.
            mfm.LoadArchives(GameType.RedAlert, "sc*.mix", true, "scores.mix");
            // Not normally needed, but in the beta this contains palette.cps.
            mfm.LoadArchive(GameType.RedAlert, "general.mix", false, false, true, true);
            // Main graphics archive
            mfm.LoadArchive(GameType.RedAlert, "conquer.mix", false, false, true, true);
            // Infantry
            mfm.LoadArchive(GameType.RedAlert, "lores.mix", false, false, true, true);
            // Expansion infantry
            mfm.LoadArchive(GameType.RedAlert, "lores1.mix", false, false, true, true);
            // Theaters
            foreach (TheaterType raTheater in AllTheaters)
            {
                StartupLoader.LoadClassicTheater(mfm, GameType.RedAlert, raTheater, true);
            }
            // Check files
            mfm.Reset(GameType.RedAlert, null);
            HashSet<string> loadedFiles = mfm.Select(s => Path.GetFileName(s)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            string prefix = ShortName + ": ";
            // Allow loading without expansion files.
            //TestMixExists(loadedFiles, loadErrors, prefix, "expand2.mix");
            StartupLoader.TestMixExists(loadedFiles, loadErrors, prefix, "local.mix");
            if (!forRemaster)
            {
                StartupLoader.TestMixExists(loadedFiles, loadErrors, prefix, "conquer.mix");
                StartupLoader.TestMixExists(loadedFiles, loadErrors, prefix, "lores.mix");
                // Allow loading without expansion files.
                //TestMixExists(loadedFiles, loadErrors, prefix, "lores1.mix");
            }
            // Required theaters
            foreach (TheaterType raTheater in AllTheaters)
            {
                StartupLoader.TestMixExists(loadedFiles, loadErrors, prefix, raTheater, !raTheater.IsModTheater);
            }
            if (!forRemaster)
            {
                StartupLoader.TestFileExists(mfm, fileLoadErrors, prefix, "palette.cps");
                StartupLoader.TestFileExists(mfm, fileLoadErrors, prefix, "conquer.eng");
            }
            StartupLoader.TestFileExists(mfm, fileLoadErrors, prefix, "rules.ini");
            // Allow loading without expansion files.
            //TestFileExists(mfm, loadErrors,prefix, "aftrmath.ini");
            //TestFileExists(mfm, loadErrors,prefix, "mplayer.ini");
        }

        public override string GetClassicOpposingPlayer(string player) => HouseTypes.GetClassicOpposingPlayer(player);

        public override bool SupportsMapLayer(MapLayerFlag mlf)
        {
            return mlf == (mlf & ~MapLayerFlag.FootballArea);
        }

        public override Bitmap GetWaypointIcon()
        {
            return Globals.TheTilesetManager.GetTile("beacon", 0, "mouse", 15, null);
        }

        public override Bitmap GetCellTriggerIcon()
        {
            return Globals.TheTilesetManager.GetTile("mine", 3, "mine.shp", 3, null);
        }

        public override Bitmap GetSelectIcon()
        {
            // Remaster: Chronosphere cursor from TEXTURES_SRGB.MEG
            // Alt: @"DATA\ART\TEXTURES\SRGB\ICON_IONCANNON_15.DDS
            // Classic: Chronosphere cursor
            return Globals.TheTilesetManager.GetTexture(@"DATA\ART\TEXTURES\SRGB\ICON_SELECT_GREEN_04.DDS", "mouse", 101, true);
        }

        public override Bitmap GetCaptureIcon()
        {
            // Remaster: Chronosphere cursor from TEXTURES_SRGB.MEG
            // Alt: @"DATA\ART\TEXTURES\SRGB\ICON_IONCANNON_15.DDS
            // Classic: Chronosphere cursor
            return Globals.TheTilesetManager.GetTexture(@"DATA\ART\TEXTURES\SRGB\ICON_MOUNT_UNIT_X2_02.DDS", "mouse", 115, true);
        }

        public override string EvaluateBriefing(string briefing)
        {
            string briefText = (briefing ?? String.Empty).Replace('\t', ' ').Trim('\r', '\n', ' ').Replace("\r\n", "\n").Replace("\r", "\n");
            StringBuilder message = new StringBuilder();
            if (!Globals.UseClassicFiles || !Globals.ClassicNoRemasterLogic)
            {
                // The actual line length cutoff depends on the user resolution and which characters are used, but this is a decent indication.
                const int cutoff = 40;
                int lines = briefText.Count(c => c == '\n') + 1;
                bool briefLenOvfl = lines > 25;
                bool briefLenSplitOvfl = false;
                // If it's already over 25 lines because of the line breaks, don't bother doing the length split logic; it'll be bad anyway.
                if (!briefLenOvfl)
                {
                    // split in lines of 40; that's more or less the average line length in the brief screen.
                    List<string> txtLines = new List<string>();
                    string[] briefLines = briefText.Split('\n');
                    for (int i = 0; i < briefLines.Length; ++i)
                    {
                        string line = briefLines[i].Trim();
                        if (line.Length <= cutoff)
                        {
                            txtLines.Add(line);
                            continue;
                        }
                        string[] splitLine = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        int wordIndex = 0;
                        while (wordIndex < splitLine.Length)
                        {
                            StringBuilder sb = new StringBuilder();
                            // Always allow initial word
                            int nextLength = 0;
                            while (nextLength < cutoff && wordIndex < splitLine.Length)
                            {
                                if (sb.Length > 0)
                                    sb.Append(' ');
                                sb.Append(splitLine[wordIndex++]);
                                nextLength = wordIndex >= splitLine.Length ? 0 : (sb.Length + 1 + splitLine[wordIndex].Length);
                            }
                            txtLines.Add(sb.ToString());
                        }
                    }
                    briefLenSplitOvfl = txtLines.Count > 25;
                }
                const string warn25Lines = "Red Alert's briefing screen in the Remaster can only show 25 lines of briefing text. ";
                if (briefLenOvfl)
                {
                    message.Append(warn25Lines).Append("Your current briefing exceeds that.");
                }
                else if (briefLenSplitOvfl)
                {
                    message.Append(warn25Lines)
                        .Append("The lines average to about 40 characters per line, and when split that way, your current briefing exceeds that, ")
                        .Append("meaning it will most likely not display correctly in-game.");
                }
            }
            if (Globals.WriteClassicBriefing)
            {
                if (briefText.Length > Constants.MaxBriefLengthClassic)
                {
                    if (message.Length > 0)
                    {
                        message.Append("\n\n");
                    }
                    message.Append("Classic Red Alert briefings cannot exceed ").Append(Constants.MaxBriefLengthClassic).Append(" characters. This includes line breaks.")
                        .Append(" This will not affect the mission when playing in the Remaster, but the briefing will be truncated when playing in the original game.");
                }
                if (briefText.Contains(";"))
                {
                    if (message.Length > 0)
                    {
                        message.Append("\n\n");
                    }
                    message.Append("Classic Red Alert briefings cannot contain semicolon characters, since they are the ini format's notation ")
                        .Append("for marking all following text on the line as comment. This will not affect the mission when playing in the Remaster, ")
                        .Append("but if kept, they will be replaced by colons in the classic briefing to prevent this issue.");
                }
            }
            return message.Length == 0 ? null : message.ToString();
        }

        public override bool MapNameIsEmpty(string name)
        {
            return String.IsNullOrEmpty(name) || Constants.EmptyMapName.Equals(name, StringComparison.OrdinalIgnoreCase);
        }

        public override string GetClassicFontInfo(ClassicFont font, TilesetManagerClassic tsmc, TeamRemapManager trm, Color textColor, out bool crop, out TeamRemap remap, out Dictionary<byte, Color> remapAdjust)
        {
            crop = false;
            remap = null;
            remapAdjust = null;
            string fontName = null;
            switch (font)
            {
                case ClassicFont.Waypoints:
                    crop = true;
                    fontName = "8point.fnt";
                    remap = GetClassicFontRemapSimple(fontName, tsmc, trm, textColor, out remapAdjust, 2, 3);
                    break;
                case ClassicFont.WaypointsLong:
                    crop = true;
                    fontName = "editfnt.fnt";
                    remap = GetClassicFontRemapSimple(fontName, tsmc, trm, textColor, out remapAdjust, 2, 3);
                    break;
                case ClassicFont.CellTriggers:
                    crop = true;
                    fontName = "scorefnt.fnt";
                    remap = GetClassicFontRemapSimple(fontName, tsmc, trm, textColor, out remapAdjust);
                    break;
                case ClassicFont.RebuildPriority:
                    crop = true;
                    fontName = "scorefnt.fnt";
                    remap = GetClassicFontRemapSimple(fontName, tsmc, trm, textColor, out remapAdjust);
                    break;
                case ClassicFont.TechnoTriggers:
                    crop = true;
                    fontName = "editfnt.fnt";
                    remap = GetClassicFontRemapSimple(fontName, tsmc, trm, textColor, out remapAdjust, 2, 3);
                    break;
                case ClassicFont.TechnoTriggersSmall:
                    crop = true;
                    fontName = "5pntthin.fnt";
                    if (!tsmc.TileExists(fontName))
                    {
                        fontName = "3point.fnt";
                    }
                    remap = GetClassicFontRemapSimple(fontName, tsmc, trm, textColor, out remapAdjust);
                    break;
                case ClassicFont.FakeLabels:
                    crop = true;
                    fontName = "editfnt.fnt";
                    remap = GetClassicFontRemapSimple(fontName, tsmc, trm, textColor, out remapAdjust, 2, 3);
                    break;
            }
            if (!tsmc.TileExists(fontName))
            {
                fontName = null;
            }
            return fontName;
        }
    }
}
