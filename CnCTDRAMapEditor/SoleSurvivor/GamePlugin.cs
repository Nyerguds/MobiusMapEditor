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
        protected const int teamStartPoints = 8;
        public override String Name => "Sole Survivor";
        public override GameType GameType => GameType.SoleSurvivor;
        public override bool IsMegaMap => true;

        public static bool CheckForSSmap(INI iniContents)
        {
            return GeneralUtils.CheckForIniInfo(iniContents, "Crates", null, null);
        }

        protected CratesSection cratesSection;
        public CratesSection CratesSection => cratesSection;

        public GamePlugin()
            : this(true)
        {
        }

        public GamePlugin(bool mapImage)
            : base()
        {
            this.isMegaMap = true;
            var crateWaypoints = Enumerable.Range(0, cratePoints).Select(i => new Waypoint(string.Format("CR{0}", i), WaypointFlag.CrateSpawn));
            var teamWaypoints = Enumerable.Range(cratePoints, teamStartPoints).Select(i => new Waypoint(string.Format("TM{0}", i - cratePoints), Waypoint.GetFlagForMpId(i - cratePoints)));
            var generalWaypoints = Enumerable.Range(cratePoints + teamStartPoints, 25 - cratePoints - teamStartPoints).Select(i => new Waypoint(i.ToString()));
            var specialWaypoints = new Waypoint[] { new Waypoint("Flare", WaypointFlag.Flare), new Waypoint("Home", WaypointFlag.Home), new Waypoint("Reinf.", WaypointFlag.Reinforce) };
            Waypoint[] waypoints = crateWaypoints.Concat(teamWaypoints).Concat(generalWaypoints).Concat(specialWaypoints).ToArray();
            var basicSection = new TiberianDawn.BasicSection();
            basicSection.SetDefault();
            var houseTypes = HouseTypes.GetTypes().ToList();
            basicSection.Player = HouseTypes.Admin.Name;
            // Irrelevant for SS. Rebuilding options will be disabled in the editor.
            basicSection.BasePlayer = HouseTypes.GetBasePlayer(basicSection.Player);
            cratesSection = new CratesSection();
            cratesSection.SetDefault();

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
            BuildingType[] buildings = Globals.NoOwnedObjectsInSole ? new BuildingType[0] : TiberianDawn.BuildingTypes.GetTypes().ToArray();
            UnitType[] units = Globals.NoOwnedObjectsInSole ? new UnitType[0] : TiberianDawn.UnitTypes.GetTypes(Globals.DisableAirUnits).ToArray();
            InfantryType[] infantry = Globals.NoOwnedObjectsInSole ? new InfantryType[0] : TiberianDawn.InfantryTypes.GetTypes().ToArray();
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
            // Multi7: the dark blue that's unused in SS because Multi4 uses BAD_UNITS instead.
            flagColors[6] = Globals.TheTeamColorManager["MULTI2"];
            // Multi8: RA Purple
            flagColors[7] = new TeamColor(Globals.TheTeamColorManager, flagColors[0], "MULTI8", new Vector3(0.410f, 0.100f, 0.000f));
            Map = new Map(basicSection, null, TiberianDawn.Constants.MaxSizeMega, typeof(House), houseTypes,
                flagColors, TiberianDawn.TheaterTypes.GetTypes(), TiberianDawn.TemplateTypes.GetTypes(),
                TiberianDawn.TerrainTypes.GetTypes(), OverlayTypes.GetTypes(), TiberianDawn.SmudgeTypes.GetTypes(Globals.ConvertCraters),
                TiberianDawn.EventTypes.GetTypes(), cellEventTypes, unitEventTypes, structureEventTypes, terrainEventTypes,
                TiberianDawn.ActionTypes.GetTypes(), cellActionTypes, unitActionTypes, structureActionTypes, terrainActionTypes,
                TiberianDawn.MissionTypes.GetTypes(), DirectionTypes.GetMainTypes(), DirectionTypes.GetAllTypes(), infantry, units,
                buildings, TiberianDawn.TeamMissionTypes.GetTypes(), fullTechnoTypes, waypoints, movieTypes, themeTypes)
            {
                TiberiumOrGoldValue = 25
            };
            Map.MapSection.PropertyChanged += MapSection_PropertyChanged;
            // Clean up this mess.
            foreach (House house in Map.Houses)
            {
                if (house.Type.ID >= HouseTypes.Multi1.ID)
                {
                    house.Enabled = false;
                }
            }
            if (mapImage)
            {
                MapImage = new Bitmap(Map.Metrics.Width * Globals.MapTileWidth, Map.Metrics.Height * Globals.MapTileHeight);
            }
        }

        public override IEnumerable<string> Load(string path, FileType fileType)
        {
            return Load(path, fileType, true);
        }

        protected override List<string> LoadINI(INI ini, bool forceSoloMission, ref bool modified)
        {
            List<string> errors = LoadINI(ini, forceSoloMission, true, ref modified);
            var cratesIniSection = extraSections.Extract("Crates");
            if (cratesIniSection != null)
            {
                INI.ParseSection(new MapContext(Map, false), cratesIniSection, this.cratesSection);
            }
            return errors;
        }

        public override bool Save(string path, FileType fileType, Bitmap customPreview)
        {
            return Save(path, fileType, true, customPreview);
        }

        protected override void SaveINI(INI ini, FileType fileType, string fileName)
        {
            INISection waypointBackup = null;
            INISection overlayBackup = null;
            if (extraSections != null)
            {
                // Commonly found in official maps as backups of beta versions of the map.
                // If found, place them under their respective original sections.
                waypointBackup = extraSections.Extract("OldWaypoints");
                overlayBackup = extraSections.Extract("OldOverlay");
                ini.Sections.AddRange(extraSections);
            }
            INISection basicSection = SaveIniBasic(ini, fileName);
            // Not used for SS; not Remaster, and no single play in it.
            basicSection.Keys.Remove("SoloMission");
            SaveIniMap(ini);
            INISection cratesIniSection = ini.Sections.Add("Crates");
            SaveIniWaypoints(ini);
            if (waypointBackup != null)
            {
                ini.Sections.Add(waypointBackup);
            }
            INI.WriteSection(new MapContext(Map, false), cratesIniSection, this.cratesSection);
            SaveIniCellTriggers(ini, true);
            SaveIniTeamTypes(ini, true);
            SaveIniTriggers(ini, true);
            SaveIniHouses(ini);
            //SaveIniBriefing(ini);
            SaveIniUnits(ini);
            SaveIniAircraft(ini);
            SaveIniBase(ini, true);
            SaveIniInfantry(ini);
            SaveIniStructures(ini);
            SaveINITerrain(ini);
            SaveIniOverlay(ini);
            if (overlayBackup != null)
            {
                ini.Sections.Add(overlayBackup);
            }
            SaveIniSmudge(ini);
        }

        public override HashSet<string> GetHousesWithProduction()
        {
            // Not applicable. Return empty set.
            return new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
