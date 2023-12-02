using System;
using System.Collections.Generic;
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
        public override String ModFolder => "Tiberian_Dawn";
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

        public override void InitializePlugin(IGamePlugin plugin)
        {
            Globals.TheTeamColorManager.Load(@"DATA\XML\CNCTDTEAMCOLORS.XML");
            GameInfoTibDawn.AddTeamColorsTD(Globals.TheTeamColorManager);
        }

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

        public override string GetClassicOpposingPlayer(string player) => HouseTypes.GetTypes().FirstOrDefault()?.Name;

        public override bool SupportsMapLayer(MapLayerFlag mlf)
        {
            return mlf != MapLayerFlag.BuildingFakes
                && mlf != MapLayerFlag.EffectRadius
                && (!Globals.NoOwnedObjectsInSole ||
                   (mlf != MapLayerFlag.Buildings
                 && mlf != MapLayerFlag.Units
                 && mlf != MapLayerFlag.Infantry
                 && mlf != MapLayerFlag.BuildingRebuild));
        }
    }
}
