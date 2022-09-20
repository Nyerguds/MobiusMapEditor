using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiusEditor.TiberianDawn
{
    public class GamePluginSS : GamePlugin
    {

        protected const int cratePoints = 4;
        protected const int teamStartPoints = 4;

        public GamePluginSS(IFeedBackHandler feedBackHandler)
            : this(true, feedBackHandler)
        {
        }

        public static bool CheckForSSmap(String path, FileType fileType)
        {
            return CheckForMegamap(path, fileType) && CheckForIniInfo(path, fileType, "Crates", null, null);
        }

        public GamePluginSS(Boolean mapImage, IFeedBackHandler feedBackHandler)
            : base()
        {
            this.isMegaMap = true;
            this.feedBackHandler = feedBackHandler;
            var crateWaypoints = Enumerable.Range(0, cratePoints).Select(i => new Waypoint(string.Format("CR{0}", i), WaypointFlag.CrateSpawn));
            var teamWaypoints = Enumerable.Range(cratePoints, 25 - cratePoints).Select(i => new Waypoint(string.Format("TM{0}", i - cratePoints), WaypointFlag.PlayerStart));
            var generalWaypoints = Enumerable.Range(cratePoints + teamStartPoints, 25 - cratePoints - teamStartPoints).Select(i => new Waypoint(i.ToString()));
            var specialWaypoints = new Waypoint[] { new Waypoint("Flare", WaypointFlag.Flare), new Waypoint("Home", WaypointFlag.Home), new Waypoint("Reinf.", WaypointFlag.Reinforce) };
            Waypoint[] waypoints = crateWaypoints.Concat(teamWaypoints).Concat(generalWaypoints).Concat(specialWaypoints).ToArray();
            var basicSection = new BasicSection();
            basicSection.SetDefault();
            var houseTypes = HouseTypesSS.GetTypes();
            basicSection.Player = HouseTypesSS.Admin.Name;
            // Irrelevant for SS. Rebuilding options will be disabled in the editor.
            basicSection.BasePlayer = HouseTypes.GetBasePlayer(basicSection.Player);
            // I guess we leave these to the TD defaults.
            string[] cellEventTypes = new[]
            {
                EventTypes.EVENT_PLAYER_ENTERED,
                EventTypes.EVENT_NONE
            };
            string[] unitEventTypes =
            {
                EventTypes.EVENT_DISCOVERED,
                EventTypes.EVENT_ATTACKED,
                EventTypes.EVENT_DESTROYED,
                EventTypes.EVENT_ANY,
                EventTypes.EVENT_NONE
            };
            string[] structureEventTypes = (new[] { EventTypes.EVENT_PLAYER_ENTERED }).Concat(unitEventTypes).ToArray();
            string[] terrainEventTypes =
            {
                EventTypes.EVENT_ATTACKED,
                EventTypes.EVENT_ANY,
                EventTypes.EVENT_NONE
            };
            string[] cellActionTypes = { };
            string[] unitActionTypes = { };
            string[] structureActionTypes = { };
            string[] terrainActionTypes = { };
            Map = new Map(basicSection, null, Constants.MaxSizeMega, typeof(House),
                houseTypes, TheaterTypes.GetTypes(), TemplateTypes.GetTypes(),
                TerrainTypes.GetTypes(), OverlayTypes.GetTypes(), SmudgeTypes.GetTypes(Globals.ConvertCraters),
                EventTypes.GetTypes(), cellEventTypes, unitEventTypes, structureEventTypes, terrainEventTypes,
                ActionTypes.GetTypes(), cellActionTypes, unitActionTypes, structureActionTypes, terrainActionTypes,
                MissionTypes.GetTypes(), DirectionTypes.GetTypes(), InfantryTypes.GetTypes(), UnitTypes.GetTypes(Globals.DisableAirUnits),
                BuildingTypes.GetTypes(), TeamMissionTypes.GetTypes(), fullTechnoTypes, waypoints, movieTypes, themeTypes)
            {
                TiberiumOrGoldValue = 25
            };
            Map.MapSection.PropertyChanged += MapSection_PropertyChanged;
            // Clean up this mess.
            foreach (House house in Map.Houses)
            {
                if (house.Type.ID > HouseTypesSS.Multi1.ID)
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
