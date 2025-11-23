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
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text.RegularExpressions;

namespace MobiusEditor.TiberianDawn
{
    public class GameInfoTibDawn : GameInfo
    {
        private static readonly int HighestTdMapVal = TemplateTypes.GetTypes().Max(t => (int)t.ID);

        public override GameType GameType => GameType.TiberianDawn;
        public override string Name => "Tiberian Dawn";
        public override string ShortName => "TD";
        public override string IniName => "TiberianDawn";
        public override string SteamId => "1213210";
        public override bool PublishedMapsUseMirrorServer => true;
        public override string SteamGameName => "Command & Conquer: Remastered";
        public override string SteamGameNameShort => "C&C:Rem";
        public override string SteamFileExtensionSolo => ".PGM";
        public override string SteamFileExtensionMulti => ".PGM";
        public override FileType SteamFileType => FileType.PGM;
        public override string[] SteamDefaultTags => new string[] { "TD" };
        public override string[] SteamSoloTags => new string[] { "singleplayer" };
        public override string[] SteamMultiTags => new string[] { "multiplayer" };
        public override string[] SteamSoloExtraTags => new string[] { };
        public override string[] SteamMultiExtraTags => new string[] { "FFA", "1v1", "2v2" };
        public override string DefaultSaveDirectory => Path.Combine(Globals.RootSaveDirectory, "Tiberian_Dawn");
        public override FileTypeInfo[] SupportedFileTypes => new FileTypeInfo[] {
            // BIN and B64 are added separately so they get accepted as valid resave types, but their data is never actually used.
            new FileTypeInfo(FileType.INI, "Tiberian Dawn map (ini+bin)", new string[] { "ini", "bin" }, new string[] { "ini", "bin" },
                new FileType[] { FileType.INI, FileType.BIN }, new FileType[] { FileType.INI, FileType.BIN }),
            new FileTypeInfo(FileType.BIN, "Tiberian Dawn map (ini+bin)", FileTypeFlags.HideFromList, new string[] { "bin", "ini" }, new string[] { "bin", "ini" },
                new FileType[] { FileType.BIN, FileType.INI }, new FileType[] { FileType.BIN, FileType.INI }),
            // Experimental; TD map but saved in single file as RA. Disabled for now.
            new FileTypeInfo(FileType.MPR, "Tiberian Dawn map (compact)", FileTypeFlags.ExpandedType, new string[] { "ini" }, new string[] { "mpr" }),
            new FileTypeInfo(FileType.I64, "Tiberian Dawn N64 map (ini+map)", new string[] { "ini", "map" }, new string[] { "ini", "map" },
                new FileType[] { FileType.I64, FileType.B64 }, new FileType[] { FileType.I64, FileType.B64 }),
            new FileTypeInfo(FileType.B64, "Tiberian Dawn N64 map (ini+map)", FileTypeFlags.HideFromList, new string[] { "map", "ini" }, new string[] { "map", "ini" },
                new FileType[] { FileType.B64, FileType.I64 }, new FileType[] { FileType.B64, FileType.I64 }),
            new FileTypeInfo(FileType.PGM, "Tiberian Dawn map PGM", FileTypeFlags.InternalUse, new string[] { "pgm" }, new string[] { "pgm" })
        };
        public override FileType DefaultSaveType => FileType.INI;
        public override FileType DefaultSaveTypeFromMix => FileType.INI;
        public override FileType DefaultSaveTypeFromPgm => FileType.INI;
        public override string ModFolder => Path.Combine(Globals.ModDirectory, "Tiberian_Dawn");
        public override string ModIdentifier => "TD";
        public override string ModsToLoad => Properties.Settings.Default.ModsToLoadTD;
        public override string ModsToLoadSetting => "ModsToLoadTD";
        public override string[] RemasterMegFiles => new string[] { "CONFIG.MEG", "TEXTURES_COMMON_SRGB.MEG", "TEXTURES_SRGB.MEG", "TEXTURES_TD_SRGB.MEG" };
        public override string ClassicFolder => Properties.Settings.Default.ClassicPathTD;
        public override string ClassicFolderRemaster => "CNCDATA\\TIBERIAN_DAWN";
        public override string ClassicFolderRemasterData => ClassicFolderRemaster + "\\CD1";
        public override string ClassicFolderDefault => "Classic\\TD";
        public override string ClassicFolderSetting => "ClassicPathTD";
        public override string ClassicStringsFile => "conquer.eng";
        public override Size MapSize => Constants.MaxSize;
        public override Size MapSizeMega => Constants.MaxSizeMega;
        public override TheaterType[] AllTheaters => TheaterTypes.GetAllTypes().ToArray();
        public override TheaterType[] AvailableTheaters => TheaterTypes.GetTypes().ToArray();
        public override bool MegamapIsSupported => true;
        public override bool MegamapIsOptional => true;
        public override bool MegamapIsDefault => false;
        public override bool MegamapIsOfficial => false;
        public override bool HasSinglePlayer => true;
        public override bool CanUseNewMixFormat => false;
        public override long MaxDataSize => Globals.MaxMapSize;
        public override int MaxAircraft => Constants.MaxAircraft;
        public override int MaxVessels => 0;
        public override int MaxBuildings => Constants.MaxBuildings;
        public override int MaxInfantry => Constants.MaxInfantry;
        public override int MaxTerrain => Constants.MaxTerrain;
        public override int MaxUnits => Constants.MaxUnits;
        public override int MaxTriggers => Constants.MaxTriggers;
        public override int MaxTriggerNameLength => Constants.MaxTriggerNameLength;
        public override int MaxTeams => Constants.MaxTeams;
        public override int MaxTeamNameLength => Constants.MaxTeamNameLength;
        public override int MaxTeamClasses => Globals.MaxTeamClasses;
        public override int MaxTeamMissions => Globals.MaxTeamMissions;
        public override int HitPointsGreenMinimum => 127;
        public override int HitPointsYellowMinimum => 63;
        public override bool LandedHelis => Globals.LandedHelisTd;
        public override Size ViewportSizeSmall => new Size(240, 192);
        public override Size ViewportSidebarSmall => new Size(80, 192);
        public override Point ViewportOffsetSmall => new Point(0, 0);
        public override Size ViewportSizeLarge => new Size(480, 384);
        public override Size ViewportSidebarLarge => new Size(160, 384);
        public override Point ViewportOffsetLarge => new Point(0, 0);
        public override OverlayTypeFlag OverlayIconType => OverlayTypeFlag.Crate;
        public override Bitmap WorkshopPreviewGeneric => Properties.Resources.UI_CustomMissionPreviewDefault;
        public override Bitmap WorkshopPreviewGenericGame => Properties.Resources.TD_Head;

        public override FileType IdentifyMap(INI iniContents, byte[] binContents, bool contentWasSwapped, bool acceptBin, out bool isMegaMap, out string theater)
        {
            isMegaMap = false;
            theater = null;
            bool iniMatch = IsCnCIni(iniContents) &&
                !RedAlert.GamePluginRA.CheckForRAMap(iniContents) &&
                !SoleSurvivor.GamePluginSS.CheckForSSmap(iniContents);
            if (!iniMatch)
            {
                return FileType.None;
            }
            theater = GetTheater(iniContents);
            bool ismpr = GamePluginTD.CheckForEmbeddedMap(iniContents);
            if (ismpr && contentWasSwapped)
            {
                // extra loaded ini file is irrelevant to actual loaded file.
                return FileType.None;
            }
            isMegaMap = GamePluginTD.CheckForMegamap(iniContents);
            int maxTemplate = TemplateTypes.GetTypes().Max(tp => tp.ID);
            bool isDesert = TheaterTypes.Desert.Name.Equals(theater, StringComparison.OrdinalIgnoreCase);
            Dictionary<int, ushort> n64Mapping = isDesert ? N64MapConverter.DESERT_MAPPING : N64MapConverter.TEMPERATE_MAPPING;
            int maxTemplateN64 = isMegaMap ? -1 : n64Mapping.Keys.Where(k => k != -1 && k != 0xFFFF).Max();
            bool isN64 = false;
            bool normalMapFormatOk = !isMegaMap && !ismpr && GamePluginTD.CheckNormalMapFormat(binContents, MapSize, maxTemplate, maxTemplateN64, out isN64);
            if (contentWasSwapped && !acceptBin
                && (isMegaMap && !GamePluginTD.CheckMegaMapFormat(binContents, MapSizeMega, maxTemplate)
                 || (!isMegaMap && !normalMapFormatOk)))
            {
                // Primary read file is just some unsupported file that happens to have the same
                // name as a valid ini file in the same folder. Reject the original loaded file.
                return FileType.None;
            }
            return ismpr ? FileType.MPR : contentWasSwapped ? (isN64 ? FileType.B64 : FileType.BIN) : (isN64 ? FileType.I64 : FileType.INI);
        }

        public override IGamePlugin CreatePlugin(bool mapImage, bool megaMap) => new GamePluginTD(mapImage, megaMap);

        public override void InitClassicFiles(MixfileManager mfm, List<string> loadErrors, List<string> fileLoadErrors, bool forRemaster)
        {
            // This function is used by Sole Survivor too, so it references the local GameType and ShortName.
            string prefix = ShortName + ": ";
            mfm.Reset(GameType.None, null);
            // Contains cursors / strings file. Prefer Win95 version over DOS one.
            mfm.LoadArchive(GameType, "cclocal.mix", false);
            mfm.LoadArchive(GameType, "local.mix", false);
            // Mod addons
            mfm.LoadArchives(GameType, "sc*.mix", false, "scores.mix");
            mfm.LoadArchive(GameType, "conquer.mix", false);
            // Theaters
            foreach (TheaterType tdTheater in AllTheaters)
            {
                StartupLoader.LoadClassicTheater(mfm, GameType, tdTheater, false);
            }
            // Check files.
            mfm.Reset(GameType, null);
            HashSet<string> loadedFiles = mfm.Select(s => Path.GetFileName(s)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            // Check required files.
            if (!forRemaster)
            {
                StartupLoader.TestMixExists(loadedFiles, loadErrors, prefix, "local.mix", "cclocal.mix");
                StartupLoader.TestMixExists(loadedFiles, loadErrors, prefix, "conquer.mix");
            }
            // Theaters
            foreach (TheaterType tdTheater in AllTheaters)
            {
                StartupLoader.TestMixExists(loadedFiles, loadErrors, prefix, tdTheater, !tdTheater.IsModTheater);
            }
            if (!forRemaster)
            {
                StartupLoader.TestFileExists(mfm, fileLoadErrors, prefix, "conquer.eng");
            }
        }

        public override string GetClassicOpposingPlayer(string player) => HouseTypes.GetClassicOpposingPlayer(player);

        public override bool SupportsMapLayer(MapLayerFlag mlf)
        {
            MapLayerFlag badLayers = MapLayerFlag.BuildingFakes | MapLayerFlag.EffectRadius | MapLayerFlag.FootballArea;
            return mlf == (mlf & ~badLayers);
        }

        public override Bitmap GetWaypointIcon()
        {
            return Globals.TheTilesetManager.GetTexture(@"DATA\ART\TEXTURES\SRGB\ICON_SELECT_FRIENDLY_X2_00.DDS", "mouse", 12, true);
        }

        public override Bitmap GetCellTriggerIcon()
        {
            return Globals.TheTilesetManager.GetTile("mine", 3, "mine.shp", 3, null);
        }

        public override Bitmap GetSelectIcon()
        {
            // Remaster: Chronosphere cursor from TEXTURES_SRGB.MEG
            // Alt: @"DATA\ART\TEXTURES\SRGB\ICON_IONCANNON_15.DDS"
            // Classic: Ion Cannon cursor
            return Globals.TheTilesetManager.GetTexture(@"DATA\ART\TEXTURES\SRGB\ICON_SELECT_GREEN_04.DDS", "mouse", 118, true);
        }

        public override Bitmap GetCaptureIcon()
        {
            return Globals.TheTilesetManager.GetTexture(@"DATA\ART\TEXTURES\SRGB\ICON_MOUNT_UNIT_X2_02.DDS", "mouse", 121, true);
        }

        public override string EvaluateBriefing(string briefing)
        {
            if (!Globals.WriteClassicBriefing)
            {
                return null;
            }
            string briefText = (briefing ?? String.Empty).Replace('\t', ' ').Trim('\r', '\n', ' ').Replace("\r\n", "\n").Replace("\r", "\n");
            // Remove duplicate spaces
            briefText = Regex.Replace(briefText, " +", " ");
            if (briefText.Length > Constants.MaxBriefLengthClassic)
            {
                return "Classic Tiberian Dawn briefings cannot exceed " + Constants.MaxBriefLengthClassic + " characters. This includes line breaks.\n\nThis will not affect the mission when playing in the Remaster, but the briefing will be truncated when playing in the original game.";
            }
            return null;
        }

        public override bool MapNameIsEmpty(string name)
        {
            return String.IsNullOrEmpty(name) || Constants.EmptyMapName.Equals(name, StringComparison.OrdinalIgnoreCase);
        }

        public override string GetClassicFontInfo(ClassicFont font, TilesetManagerClassic tsmc, Color textColor, out bool crop, out Color[] palette)
        {
            crop = false;
            palette = null;
            string fontName = null;
            int[] toClear;
            switch (font)
            {
                case ClassicFont.Waypoints:
                    crop = true;
                    fontName = "8point.fnt";
                    palette = GetClassicFontPalette(textColor, 2, 3);
                    break;
                case ClassicFont.WaypointsLong: // The DOS 6point.fnt would be ideal for this, but they replaced it with a much larger one in C&C95.
                    crop = true;
                    fontName = "6ptdos.fnt";
                    toClear = new int[] { 2, 3 };
                    if (!tsmc.TileExists(fontName))
                    {
                        fontName = "scorefnt.fnt";
                        toClear = new int[0];
                    }
                    palette = GetClassicFontPalette(textColor, toClear);
                    break;
                case ClassicFont.CellTriggers:
                    crop = true;
                    fontName = "scorefnt.fnt";
                    palette = GetClassicFontPalette(textColor);
                    break;
                case ClassicFont.RebuildPriority:
                    crop = true;
                    fontName = "scorefnt.fnt";
                    palette = GetClassicFontPalette(textColor);
                    break;
                case ClassicFont.TechnoTriggers:
                    crop = true;
                    fontName = "6ptdos.fnt";
                    toClear = new int[] { 2, 3 };
                    if (!tsmc.TileExists(fontName))
                    {
                        fontName = "scorefnt.fnt";
                        toClear = new int[0];
                    }
                    palette = GetClassicFontPalette(textColor, toClear);
                    break;
                case ClassicFont.TechnoTriggersSmall:
                    crop = true;
                    fontName = "5pntthin.fnt";
                    if (!tsmc.TileExists(fontName))
                    {
                        fontName = "3point.fnt";
                    }
                    palette = GetClassicFontPalette(textColor);
                    break;
                case ClassicFont.FakeLabels:
                    break;
            }
            if (!tsmc.TileExists(fontName))
            {
                fontName = null;
            }
            return fontName;
        }

        public override Tile GetClassicFakeLabel(TilesetManagerClassic tsm)
        {
            return null;
        }

        public override string GetSteamWorkshopFileName(IGamePlugin plugin)
        {
            return "MAPDATA";
        }
    }
}
