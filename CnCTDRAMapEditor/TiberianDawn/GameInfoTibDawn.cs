using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;

namespace MobiusEditor.TiberianDawn
{
    public class GameInfoTibDawn : GameInfo
    {
        public override GameType GameType => GameType.TiberianDawn;
        public override string Name => "Tiberian Dawn";
        public override string ModFolder => "Tiberian_Dawn";
        public override string ModIdentifier => "TD";
        public override string ModsToLoad => Properties.Settings.Default.ModsToLoadTD;
        public override string ModsToLoadSetting => "ModsToLoadTD";
        public override string WorkshopTypeId => "TD";
        public override string ClassicFolder => Properties.Settings.Default.ClassicPathTD;
        public override string ClassicFolderRemaster => "CNCDATA\\TIBERIAN_DAWN\\CD1";
        public override string ClassicFolderDefault => "Classic\\TD\\";
        public override string ClassicFolderSetting => "ClassicPathTD";
        public override string ClassicStringsFile => "conquer.eng";
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
        public override IGamePlugin CreatePlugin(Boolean mapImage, Boolean megaMap) => new GamePluginTD(mapImage, megaMap);

        public override void InitializePlugin(IGamePlugin plugin)
        {
            Globals.TheTeamColorManager.Load(@"DATA\XML\CNCTDTEAMCOLORS.XML");
            AddTeamColorsTD(Globals.TheTeamColorManager);
        }

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
            return mlf != MapLayerFlag.BuildingFakes
                && mlf != MapLayerFlag.EffectRadius
                && mlf != MapLayerFlag.FootballArea;
        }


        public static void AddTeamColorsTD(ITeamColorManager teamColorManager)
        {
            // Only applicable for Remastered colors since I can't control those.
            if (teamColorManager is TeamColorManager tcm)
            {
                // Remaster additions / tweaks
                // Neutral
                TeamColor teamColorSNeutral = new TeamColor(tcm);
                teamColorSNeutral.Load(tcm.GetItem("GOOD"), "NEUTRAL");
                tcm.AddTeamColor(teamColorSNeutral);
                // Special
                TeamColor teamColorSpecial = new TeamColor(tcm);
                teamColorSpecial.Load(tcm.GetItem("GOOD"), "SPECIAL");
                tcm.AddTeamColor(teamColorSpecial);
                // Black for unowned.
                TeamColor teamColorNone = new TeamColor(tcm);
                teamColorNone.Load("NONE", "BASE_TEAM",
                    Color.FromArgb(66, 255, 0), Color.FromArgb(0, 255, 56), 0,
                    new Vector3(0.30f, -1.00f, 0.00f), new Vector3(0f, 1f, 1f), new Vector2(0.0f, 0.1f),
                    new Vector3(0, 1, 1), new Vector2(0, 1), Color.FromArgb(61, 61, 59));
                tcm.AddTeamColor(teamColorNone);
                // Extra color for flag 7: metallic blue.
                TeamColor teamColorSeven = new TeamColor(tcm);
                teamColorSeven.Load(tcm.GetItem("BAD_UNIT"), "MULTI7");
                tcm.AddTeamColor(teamColorSeven);
                // Extra color for flag 8: copy of RA's purple.
                TeamColor teamColorEight = new TeamColor(tcm);
                teamColorEight.Load("MULTI8", "BASE_TEAM",
                    Color.FromArgb(66, 255, 0), Color.FromArgb(0, 255, 56), 0,
                    new Vector3(0.410f, 0.300f, 0.000f), new Vector3(0f, 1f, 1f), new Vector2(0.0f, 1.0f),
                    new Vector3(0, 1, 1), new Vector2(0, 1), Color.FromArgb(77, 13, 255));
                tcm.AddTeamColor(teamColorEight);
            }
        }
    }
}
