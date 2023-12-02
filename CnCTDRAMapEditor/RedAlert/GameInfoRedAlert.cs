using System;
using System.Collections.Generic;
using System.Linq;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;

namespace MobiusEditor.RedAlert
{
    public class GameInfoRedAlert : GameInfo
    {
        public override GameType GameType => GameType.RedAlert;
        public override string Name => "Red Alert";
        public override string ModFolder => "Red_Alert";
        public override string ModIdentifier => "RA";
        public override string ModsToLoad => Properties.Settings.Default.ModsToLoadRA;
        public override string ModsToLoadSetting => "ModsToLoadRA";
        public override string WorkshopTypeId => "RA";
        public override string ClassicFolder => Properties.Settings.Default.ClassicPathRA;
        public override string ClassicFolderRemaster => "CNCDATA\\RED_ALERT\\AFTERMATH";
        public override string ClassicFolderDefault => "Classic\\RA\\";
        public override string ClassicFolderSetting => "ClassicPathRA";
        public override string ClassicStringsFile => "conquer.eng";
        public override TheaterType[] AllTheaters => TheaterTypes.GetAllTypes().ToArray();
        public override TheaterType[] AvailableTheaters => TheaterTypes.GetTypes().ToArray();
        public override bool MegamapSupport => true;
        public override bool MegamapOptional => false;
        public override bool MegamapDefault => true;
        public override bool MegamapOfficial => true;
        public override bool HasSinglePlayer => true;
        public override int MaxTriggers => Constants.MaxTriggers;
        public override int MaxTeams => Constants.MaxTeams;
        public override int HitPointsGreenMinimum => 128;
        public override int HitPointsYellowMinimum => 64;
        public override IGamePlugin CreatePlugin(Boolean mapImage, Boolean megaMap) => new GamePluginRA(mapImage);

        public override void InitializePlugin(IGamePlugin plugin)
        {
            Byte[] rulesFile = Globals.TheArchiveManager.ReadFileClassic("rules.ini");
            Byte[] rulesUpdFile = Globals.TheArchiveManager.ReadFileClassic("aftrmath.ini");
            Byte[] rulesMpFile = Globals.TheArchiveManager.ReadFileClassic("mplayer.ini");
            // This returns errors in original rules files. Ignore for now.
            if (plugin is GamePluginRA raPlugin)
            {
                raPlugin.ReadRules(rulesFile);
                raPlugin.ReadExpandRules(rulesUpdFile);
                raPlugin.ReadMultiRules(rulesMpFile);
            }
            // Only one will be found.
            Globals.TheTeamColorManager.Load(@"DATA\XML\CNCRATEAMCOLORS.XML");
            Globals.TheTeamColorManager.Load("palette.cps");
            AddTeamColorsRA(Globals.TheTeamColorManager);
        }

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
            mfm.LoadArchives(GameType.RedAlert, "sc*.mix", true);
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
            List<string> loadedFiles = mfm.ToList();
            const string prefix = "RA: ";
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
            return mlf != MapLayerFlag.FootballArea;
        }

        private static void AddTeamColorsRA(ITeamColorManager teamColorManager)
        {
            if (teamColorManager is TeamColorManager tcm)
            {
                // Remaster additions / tweaks
                // "Neutral" in RA colors seems broken; makes stuff black, so remove it.
                tcm.RemoveTeamColor("NEUTRAL");
                // Special. Technically color "JP" exists for this, but it's wrong.
                TeamColor teamColorSpecial = new TeamColor(tcm);
                teamColorSpecial.Load(tcm.GetItem("SPAIN"), "SPECIAL");
                tcm.AddTeamColor(teamColorSpecial);
            }
        }
    }
}
