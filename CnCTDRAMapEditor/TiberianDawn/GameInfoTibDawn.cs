using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;

namespace MobiusEditor.TiberianDawn
{
    public class GameInfoTibDawn : GameInfo
    {
        public override GameType GameType => GameType.TiberianDawn;
        public override string Name => "Tiberian Dawn";
        public override string DefaultSaveDirectory => Path.Combine(Globals.RootSaveDirectory, "Tiberian_Dawn");
        public override string OpenFilter => Constants.FileFilter;
        public override string SaveFilter => Constants.FileFilter;
        public override string DefaultExtension => ".ini";
        public override string ModFolder => Path.Combine(Globals.ModDirectory, "Tiberian_Dawn");
        public override string ModIdentifier => "TD";
        public override string ModsToLoad => Properties.Settings.Default.ModsToLoadTD;
        public override string ModsToLoadSetting => "ModsToLoadTD";
        public override string WorkshopTypeId => "TD";
        public override string ClassicFolder => Properties.Settings.Default.ClassicPathTD;
        public override string ClassicFolderRemaster => "CNCDATA\\TIBERIAN_DAWN\\CD1";
        public override string ClassicFolderDefault => "Classic\\TD\\";
        public override string ClassicFolderSetting => "ClassicPathTD";
        public override string ClassicStringsFile => "conquer.eng";
        public override string ClassicFontTriggers => "scorefnt.fnt";
        public override TheaterType[] AllTheaters => TheaterTypes.GetAllTypes().ToArray();
        public override TheaterType[] AvailableTheaters => TheaterTypes.GetTypes().ToArray();
        public override bool MegamapSupport => true;
        public override bool MegamapOptional => true;
        public override bool MegamapDefault => false;
        public override bool MegamapOfficial => false;
        public override bool HasSinglePlayer => true;
        public override int MaxTriggers => Constants.MaxTriggers;
        public override int MaxTeams => Constants.MaxTeams;
        public override int HitPointsGreenMinimum => 127;
        public override int HitPointsYellowMinimum => 63;
        public override OverlayTypeFlag OverlayIconType => OverlayTypeFlag.Crate;
        public override IGamePlugin CreatePlugin(Boolean mapImage, Boolean megaMap) => new GamePluginTD(mapImage, megaMap);

        public override void InitClassicFiles(MixfileManager mfm, List<string> loadErrors, List<string> fileLoadErrors, bool forRemaster)
        {
            mfm.Reset(GameType.None, null);
            // Contains cursors / strings file
            mfm.LoadArchive(GameType.TiberianDawn, "local.mix", false);
            mfm.LoadArchive(GameType.TiberianDawn, "cclocal.mix", false);
            // Mod addons
            mfm.LoadArchives(GameType.TiberianDawn, "sc*.mix", false);
            mfm.LoadArchive(GameType.TiberianDawn, "conquer.mix", false);
            // Theaters
            foreach (TheaterType tdTheater in AllTheaters)
            {
                StartupLoader.LoadClassicTheater(mfm, GameType.TiberianDawn, tdTheater, false);
            }
            // Check files.
            mfm.Reset(GameType.TiberianDawn, null);
            List<string> loadedFiles = mfm.ToList();
            const string prefix = "TD: ";
            if (!forRemaster)
            {
                StartupLoader.TestMixExists(loadedFiles, loadErrors, prefix, "local.mix", "cclocal.mix");
                StartupLoader.TestMixExists(loadedFiles, loadErrors, prefix, "conquer.mix");
            }
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
            return GetTile("beacon", 0, "mouse", 12);
        }

        public override Bitmap GetCellTriggerIcon()
        {
            return GetTile("mine", 3, "mine.shp", 3);
        }

        public override Bitmap GetSelectIcon()
        {
            // Remaster: Chronosphere cursor from TEXTURES_SRGB.MEG
            // Alt: @"DATA\ART\TEXTURES\SRGB\ICON_IONCANNON_15.DDS
            // Classic: Ion Cannon cursor
            return GetTexture(@"DATA\ART\TEXTURES\SRGB\ICON_SELECT_GREEN_04.DDS", "mouse", 118);
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

        public override TeamRemap GetClassicFontTriggerRemap(TilesetManagerClassic tsmc, Color textColor)
        {
            return GetClassicFontRemapSimple(ClassicFontTriggers, tsmc, textColor);
        }

        public override IEnumerable<string> GetGameFiles()
        {
            const string iniExt = ".ini";
            const string binExt = ".bin";
            const string cpsExt = ".cps";
            char[] sides = { 'g', 'b' };
            string[] suffixes = { "ea", "eb", "ec", "ed", "ee", "wa", "wb", "wc", "wd", "we", "xa", "xb", "xc", "xd", "xe" };
            string mainSuffix = suffixes[0];
            // Campaign and expansion missions
            for (int c = 0; c < sides.Length; ++c)
            {
                char campaign = sides[c];
                // Main campaigns, and v1.06 minicampaigns
                for (int i = 1; i < 100; ++i)
                {
                    for (int s = 0; s < suffixes.Length; ++s)
                    {
                        string missionName = GetMissionName(campaign, i, suffixes[s]);
                        yield return missionName + iniExt;
                        yield return missionName + binExt;
                        yield return missionName + cpsExt;
                    }
                }
                // Expansion missions (no minicampaigns on these; too much collision)
                for (int i = 100; i < 900; ++i)
                {
                    string missionName = GetMissionName(campaign, i, mainSuffix);
                    yield return missionName + iniExt;
                    yield return missionName + binExt;
                    yield return missionName + cpsExt;
                }
                //  v1.06 minicampaigns on 900 and beyond.
                for (int i = 900; i < 1000; ++i)
                {
                    for (int s = 0; s < suffixes.Length; ++s)
                    {
                        string missionName = GetMissionName(campaign, i, suffixes[s]);
                        yield return missionName + iniExt;
                        yield return missionName + binExt;
                        yield return missionName + cpsExt;
                    }
                }
            }
            for (int i = 1; i < 20; ++i)
            {
                string missionName = GetMissionName('j', i, mainSuffix);
                yield return missionName + iniExt;
                yield return missionName + binExt;
                yield return missionName + cpsExt;
            }
            for (int i = 0; i < 1000; ++i)
            {
                string missionName = GetMissionName('m', i, mainSuffix);
                yield return missionName + iniExt;
                yield return missionName + binExt;
            }
            
            // Graphics used in the editor

            const string shpExt = ".shp";
            string[] theaterExts = TheaterTypes.GetAllTypes().Where(th => !th.IsModTheater).Select(tt => "." + tt.ClassicExtension.Trim('.')).ToArray();
            string[] extraThExts = TheaterTypes.GetAllTypes().Where(th => th.IsModTheater).Select(tt => "." + tt.ClassicExtension.Trim('.')).ToArray();
            
            // Templates
            foreach (TemplateType tmp in TemplateTypes.GetTypes())
            {
                string name = tmp.Name;
                for (int i = 0; i < theaterExts.Length; ++i)
                {
                    yield return name + theaterExts[i];
                }
            }
            foreach (TemplateType tmp in TemplateTypes.GetTypes())
            {
                string name = tmp.Name;
                for (int i = 0; i < extraThExts.Length; ++i)
                {
                    yield return name + extraThExts[i];
                }
            }
            // Buildings, with icons and build-up animations
            foreach (BuildingType bt in BuildingTypes.GetTypes())
            {
                string name = bt.Name;
                yield return name + shpExt;
                yield return name + "make" + shpExt;
                yield return name + "icon" + shpExt;
                for (int i = 0; i < theaterExts.Length; ++i)
                {
                    string thExt = theaterExts[i];
                    yield return name + thExt;
                    yield return name + "make" + thExt;
                    yield return name + "icnh" + thExt;
                }
            }
            foreach (BuildingType bt in BuildingTypes.GetTypes())
            {
                string name = bt.Name;
                for (int i = 0; i < extraThExts.Length; ++i)
                {
                    string thExt = extraThExts[i];
                    yield return name + thExt;
                    yield return name + "make" + thExt;
                    yield return name + "icnh" + thExt;
                }
            }
            // Smudge
            foreach (SmudgeType sm in SmudgeTypes.GetTypes(false))
            {
                string name = sm.Name;
                for (int i = 0; i < theaterExts.Length; ++i)
                {
                    yield return name + theaterExts[i];
                }
            }
            foreach (SmudgeType sm in SmudgeTypes.GetTypes(false))
            {
                string name = sm.Name;
                for (int i = 0; i < extraThExts.Length; ++i)
                {
                    yield return name + extraThExts[i];
                }
            }
            // Terrain
            foreach (TerrainType tr in TerrainTypes.GetTypes())
            {
                string name = tr.Name;
                for (int i = 0; i < theaterExts.Length; ++i)
                {
                    yield return name + theaterExts[i];
                }
            }
            foreach (TerrainType tr in TerrainTypes.GetTypes())
            {
                string name = tr.Name;
                for (int i = 0; i < extraThExts.Length; ++i)
                {
                    yield return name + extraThExts[i];
                }
            }
            // Infantry
            foreach (InfantryType it in InfantryTypes.GetTypes())
            {
                string name = it.Name;
                yield return name + shpExt;
                yield return name + "icon" + shpExt;
                yield return name + "rot" + shpExt;
                for (int i = 0; i < theaterExts.Length; ++i)
                {
                    yield return name + "icnh" + theaterExts[i];
                }
            }
            foreach (InfantryType it in InfantryTypes.GetTypes())
            {
                string name = it.Name;
                for (int i = 0; i < extraThExts.Length; ++i)
                {
                    yield return name + "icnh" + extraThExts[i];
                }
            }
            // Units
            foreach (UnitType un in UnitTypes.GetTypes(false))
            {
                string name = un.Name;
                yield return name + shpExt;
                yield return name + "icon" + shpExt;
                for (int i = 0; i < theaterExts.Length; ++i)
                {
                    yield return name + "icnh" + theaterExts[i];
                }
            }
            foreach (UnitType un in UnitTypes.GetTypes(false))
            {
                string name = un.Name;
                for (int i = 0; i < extraThExts.Length; ++i)
                {
                    yield return name + "icnh" + extraThExts[i];
                }
            }
            // overlay
            foreach (OverlayType ov in OverlayTypes.GetTypes())
            {
                yield return ov.Name + shpExt;
            }
        }
    }
}
