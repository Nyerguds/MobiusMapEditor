using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.TiberianDawn;
using MobiusEditor.Utility;

namespace MobiusEditor.SoleSurvivor
{
    public class GameInfoSole : GameInfo
    {
        public override GameType GameType => GameType.SoleSurvivor;
        public override String Name => "Sole Survivor";
        public override string DefaultSaveDirectory => Path.Combine(Globals.RootSaveDirectory, "Tiberian_Dawn");
        public override string OpenFilter => Constants.FileFilter;
        public override string SaveFilter => Constants.FileFilter;
        public override string DefaultExtension => ".ini";
        public override string ModFolder => Path.Combine(Globals.ModDirectory, "Tiberian_Dawn");
        public override String ModIdentifier => "TD";
        public override String ModsToLoad => Properties.Settings.Default.ModsToLoadTD;
        public override String ModsToLoadSetting => "ModsToLoadTD";
        public override string WorkshopTypeId => null;
        public override String ClassicFolder => Properties.Settings.Default.ClassicPathSS;
        public override string ClassicFolderRemaster => "CNCDATA\\TIBERIAN_DAWN\\CD1";
        public override String ClassicFolderDefault => "Classic\\SS\\";
        public override String ClassicFolderSetting => "ClassicPathSS";
        public override String ClassicStringsFile => "conquer.eng";
        public override TheaterType[] AllTheaters => TheaterTypes.GetAllTypes().ToArray();
        public override TheaterType[] AvailableTheaters => TheaterTypes.GetTypes().ToArray();
        public override bool MegamapSupport => true;
        public override bool MegamapOptional => true;
        public override bool MegamapDefault => true;
        public override bool MegamapOfficial => true;
        public override bool HasSinglePlayer => false;
        public override int MaxTriggers => Constants.MaxTriggers;
        public override int MaxTeams => Constants.MaxTeams;
        public override int HitPointsGreenMinimum => 127;
        public override int HitPointsYellowMinimum => 63;
        public override IGamePlugin CreatePlugin(Boolean mapImage, Boolean megaMap) => new GamePluginSS(mapImage, megaMap);

        public override void InitClassicFiles(MixfileManager mfm, List<string> loadErrors, List<string> fileLoadErrors, bool forRemaster)
        {
            mfm.Reset(GameType.None, null);
            // Contains cursors / strings file
            mfm.LoadArchive(GameType.SoleSurvivor, "local.mix", false);
            mfm.LoadArchive(GameType.SoleSurvivor, "cclocal.mix", false);
            // Mod addons
            mfm.LoadArchives(GameType.SoleSurvivor, "sc*.mix", false);
            mfm.LoadArchive(GameType.SoleSurvivor, "conquer.mix", false);
            // Theaters
            foreach (TheaterType ssTheater in AllTheaters)
            {
                StartupLoader.LoadClassicTheater(mfm, GameType.SoleSurvivor, ssTheater, false);
            }
            // Check files
            mfm.Reset(GameType.SoleSurvivor, null);
            List<string> loadedFiles = mfm.ToList();
            const string prefix = "SS: ";
            // Check required files.
            if (!forRemaster)
            {
                StartupLoader.TestMixExists(loadedFiles, loadErrors, prefix, "local.mix", "cclocal.mix");
                StartupLoader.TestMixExists(loadedFiles, loadErrors, prefix, "conquer.mix");
            }
            foreach (TheaterType ssTheater in AllTheaters)
            {
                StartupLoader.TestMixExists(loadedFiles, loadErrors, prefix, ssTheater, !ssTheater.IsModTheater);
            }
            if (!forRemaster)
            {
                StartupLoader.TestFileExists(mfm, fileLoadErrors, prefix, "conquer.eng");
            }
        }

        public override string GetClassicOpposingPlayer(string player) => HouseTypes.GetClassicOpposingPlayer(player);

        public override bool SupportsMapLayer(MapLayerFlag mlf)
        {
            MapLayerFlag badLayers = MapLayerFlag.BuildingFakes | MapLayerFlag.EffectRadius;
            if (Globals.NoOwnedObjectsInSole) {
                badLayers |= MapLayerFlag.Buildings | MapLayerFlag.Units | MapLayerFlag.Infantry | MapLayerFlag.BuildingRebuild;
            }
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
            return null;
        }

        public override bool MapNameIsEmpty(string name)
        {
            return String.IsNullOrEmpty(name) || Constants.EmptyMapName.Equals(name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
