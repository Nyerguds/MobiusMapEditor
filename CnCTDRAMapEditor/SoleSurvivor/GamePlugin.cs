using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MobiusEditor.SoleSurvivor
{
    public class GamePlugin : TiberianDawn.GamePlugin
    {

        protected const int cratePoints = 4;
        protected const int teamStartPoints = 4;
        public override GameType GameType => GameType.SoleSurvivor;
        public override bool IsMegaMap => true;

        public GamePlugin(IFeedBackHandler feedBackHandler)
            : this(true, feedBackHandler)
        {
        }

        public static bool CheckForSSmap(string path, FileType fileType)
        {
            return CheckForMegamap(path, fileType) && CheckForIniInfo(path, fileType, "Crates", null, null);
        }

        public CratesSection CratesSection { get; private set; }

        public GamePlugin(bool mapImage, IFeedBackHandler feedBackHandler)
            : base()
        {
            this.isMegaMap = true;
            this.feedBackHandler = feedBackHandler;
            var crateWaypoints = Enumerable.Range(0, cratePoints).Select(i => new Waypoint(string.Format("CR{0}", i), WaypointFlag.CrateSpawn));
            var teamWaypoints = Enumerable.Range(cratePoints, teamStartPoints).Select(i => new Waypoint(string.Format("TM{0}", i - cratePoints), Waypoint.GetFlagForMpId(i - cratePoints)));
            var generalWaypoints = Enumerable.Range(cratePoints + teamStartPoints, 25 - cratePoints - teamStartPoints).Select(i => new Waypoint(i.ToString()));
            var specialWaypoints = new Waypoint[] { new Waypoint("Flare", WaypointFlag.Flare), new Waypoint("Home", WaypointFlag.Home), new Waypoint("Reinf.", WaypointFlag.Reinforce) };
            Waypoint[] waypoints = crateWaypoints.Concat(teamWaypoints).Concat(generalWaypoints).Concat(specialWaypoints).ToArray();
            var basicSection = new BasicSection();
            basicSection.SetDefault();
            var houseTypes = HouseTypes.GetTypes();
            basicSection.Player = HouseTypes.Admin.Name;
            // Irrelevant for SS. Rebuilding options will be disabled in the editor.
            basicSection.BasePlayer = HouseTypes.GetBasePlayer(basicSection.Player);
            CratesSection = new CratesSection();
            CratesSection.SetDefault();

            // I guess we leave these to the TD defaults.
            string[] cellEventTypes = new[]
            {
                TiberianDawn.EventTypes.EVENT_PLAYER_ENTERED,
                TiberianDawn.EventTypes.EVENT_NONE
            };
            string[] unitEventTypes =
            {
                TiberianDawn.EventTypes.EVENT_DISCOVERED,
                TiberianDawn.EventTypes.EVENT_ATTACKED,
                TiberianDawn.EventTypes.EVENT_DESTROYED,
                TiberianDawn.EventTypes.EVENT_ANY,
                TiberianDawn.EventTypes.EVENT_NONE
            };
            string[] structureEventTypes = (new[] { TiberianDawn.EventTypes.EVENT_PLAYER_ENTERED }).Concat(unitEventTypes).ToArray();
            string[] terrainEventTypes =
            {
                TiberianDawn.EventTypes.EVENT_ATTACKED,
                TiberianDawn.EventTypes.EVENT_ANY,
                TiberianDawn.EventTypes.EVENT_NONE
            };
            string[] cellActionTypes = { };
            string[] unitActionTypes = { };
            string[] structureActionTypes = { };
            string[] terrainActionTypes = { };

            IEnumerable<BuildingType> buildings = TiberianDawn.BuildingTypes.GetTypes();
            foreach (BuildingType bld in buildings)
            {
                // Power is irrelevant in SS.
                bld.PowerUsage = 0;
                bld.PowerProduction = 0;
            }

            TeamColor[] flagColors = new TeamColor[8];
            foreach (HouseType house in houseTypes)
            {
                int mpId = Waypoint.GetMpIdFromFlag(house.MultiplayIdentifier);
                if (mpId == -1)
                {
                    continue;
                }
                flagColors[mpId] = Globals.TheTeamColorManager[house.UnitTeamColor];
            }
            // Purple
            flagColors[6] = new TeamColor(Globals.TheTeamColorManager, flagColors[0], "MULTI7", new Vector3(0.410f, 0.100f, 0.000f));
            // Pink
            flagColors[7] = new TeamColor(Globals.TheTeamColorManager, flagColors[0], "MULTI8", new Vector3(0.618f, -0.100f, 0.000f));

            Map = new Map(basicSection, null, TiberianDawn.Constants.MaxSizeMega, typeof(House), houseTypes,
                flagColors, TiberianDawn.TheaterTypes.GetTypes(), TiberianDawn.TemplateTypes.GetTypes(),
                TiberianDawn.TerrainTypes.GetTypes(), OverlayTypes.GetTypes(), TiberianDawn.SmudgeTypes.GetTypes(Globals.ConvertCraters),
                TiberianDawn.EventTypes.GetTypes(), cellEventTypes, unitEventTypes, structureEventTypes, terrainEventTypes,
                TiberianDawn.ActionTypes.GetTypes(), cellActionTypes, unitActionTypes, structureActionTypes, terrainActionTypes,
                TiberianDawn.MissionTypes.GetTypes(), TiberianDawn.DirectionTypes.GetTypes(), TiberianDawn.InfantryTypes.GetTypes(), TiberianDawn.UnitTypes.GetTypes(true),
                new BuildingType[0], TiberianDawn.TeamMissionTypes.GetTypes(), fullTechnoTypes, waypoints, movieTypes, themeTypes)
            {
                TiberiumOrGoldValue = 25
            };
            Map.MapSection.PropertyChanged += MapSection_PropertyChanged;
            // Clean up this mess.
            foreach (House house in Map.Houses)
            {
                if (house.Type.ID > HouseTypes.Multi1.ID)
                {
                    house.Enabled = false;
                }
            }
            if (mapImage)
            {
                MapImage = new Bitmap(Map.Metrics.Width * Globals.MapTileWidth, Map.Metrics.Height * Globals.MapTileHeight);
            }
        }

    }
}
