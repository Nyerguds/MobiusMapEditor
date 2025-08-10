//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// The Command & Conquer Map Editor and corresponding source code is distributed
// in the hope that it will be useful, but with permitted additional restrictions
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT
// distributed with this program. You should have received a copy of the
// GNU General Public License along with permitted additional restrictions
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using MobiusEditor.Event;
using MobiusEditor.Interface;
using MobiusEditor.Render;
using MobiusEditor.Tools;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TGASharpLib;

namespace MobiusEditor.Model
{
    // Keep this list synchronised with Map.MapLayerNames
    [Flags]
    public enum MapLayerFlag: int
    {
        None            /**/ = 0,
        // Map objects
        Template        /**/ = 1 << 00,
        Terrain         /**/ = 1 << 01,
        Infantry        /**/ = 1 << 02,
        Units           /**/ = 1 << 03,
        Buildings       /**/ = 1 << 04,
        Overlay         /**/ = 1 << 05,
        Walls           /**/ = 1 << 06,
        Resources       /**/ = 1 << 07,
        Smudge          /**/ = 1 << 08,
        Waypoints       /**/ = 1 << 09,
        FootballArea    /**/ = 1 << 10,
        // Indicators
        Boundaries      /**/ = 1 << 11,
        WaypointsIndic  /**/ = 1 << 12,
        CellTriggers    /**/ = 1 << 13,
        TechnoTriggers  /**/ = 1 << 14,
        BuildingRebuild /**/ = 1 << 15,
        BuildingFakes   /**/ = 1 << 16,
        OverlapOutlines /**/ = 1 << 17,
        // Extra Indicators
        MapSymmetry     /**/ = 1 << 18,
        MapGrid         /**/ = 1 << 19,
        LandTypes       /**/ = 1 << 20,
        TechnoOccupancy /**/ = 1 << 21,
        WaypointRadius  /**/ = 1 << 22,
        CrateOutlines   /**/ = 1 << 23,
        EffectRadius    /**/ = 1 << 24,
        HomeAreaBox     /**/ = 1 << 25,

        Technos = Terrain | Infantry | Units | Buildings,
        OverlayAll = Resources | Walls | Overlay,
        /// <summary>Listing of layers that are hard-painted onto the map image.</summary>
        MapLayers = Template | Terrain | Resources | Walls | Overlay | Smudge | Infantry | Units | Buildings | Waypoints | FootballArea,
        /// <summary>Listing of layers that don't need a full map repaint.</summary>
        Indicators = Boundaries | WaypointsIndic | CellTriggers | TechnoTriggers | BuildingRebuild | BuildingFakes | OverlapOutlines
            | MapSymmetry | MapGrid | LandTypes | TechnoOccupancy | WaypointRadius | CrateOutlines | EffectRadius | HomeAreaBox,
        All = Int32.MaxValue
    }

    public class TriggersChangedEventArgs : EventArgs
    {

    }

    public class MapContext : ITypeDescriptorContext
    {
        public IContainer Container { get; private set; }

        public object Instance { get; private set; }

        public PropertyDescriptor PropertyDescriptor { get; private set; }

        public Map Map => Instance as Map;

        public readonly bool FractionalPercentages;

        public MapContext(Map map, bool fractionalPercentages)
        {
            Instance = map;
            FractionalPercentages = fractionalPercentages;
        }

        public object GetService(Type serviceType) => null;

        public void OnComponentChanged() { }

        public bool OnComponentChanging() => true;
    }

    public class Map : ICloneable
    {
        /// <summary>
        /// Enum specifying how filled a cell of concrete is. A full cell consist of a full triangle pointing to the side,
        /// with a half-triangle at the top and a half-triangle at the bottom filling the remaining space.
        /// </summary>
        [Flags]
        private enum ConcFill
        {
            None   /**/ = 0,
            Center /**/ = 1 << 0,
            Top    /**/ = 1 << 1,
            Bottom /**/ = 1 << 2,
        }

        /// <summary>
        /// Enum for adjacent positions around a concrete cell to refresh, representing the positions as bits.
        /// Note that this uses "side", and not left or right, in order to unify the logic for even and odd cells.
        /// </summary>
        [Flags]
        private enum ConcAdj
        {
            None       /**/ = 0,
            Top        /**/ = 1 << 0,
            TopSide    /**/ = 1 << 1,
            Side       /**/ = 1 << 2,
            BottomSide /**/ = 1 << 3,
            Bottom     /**/ = 1 << 4,
        }

        // Seed itself is no longer random. Fixed seed gives consistency on resaves.
        private const int randomSeed = 1621259415;

        // Keep this list synchronised with the MapLayerFlag enum
        public static string[] MapLayerNames = {
            // Map layers
            /* Template        */ "Map templates",
            /* Terrain         */ "Terrain",
            /* Infantry        */ "Infantry",
            /* Units           */ "Units",
            /* Buildings       */ "Buildings",
            /* Overlay         */ "Overlay",
            /* Walls           */ "Walls",
            /* Resources       */ "Resources",
            /* Smudge          */ "Smudge",
            /* Waypoints       */ "Waypoints",
            /* FootballArea    */ "Football goal areas",
            // Indicators
            /* Boundaries      */ "Map boundaries",
            /* WaypointsIndic  */ "Waypoint labels",
            /* CellTriggers    */ "Cell triggers",
            /* TechnoTriggers  */ "Object triggers",
            /* BuildingRebuild */ "Building rebuild priorities",
            /* BuildingFakes   */ "Building 'fake' labels",
            /* OverlapOutlines */ "Outlines on overlapped objects",
            // Extra indicators
            /* MapSymmetry     */ "Map symmetry",
            /* MapGrid         */ "Map grid",
            /* LandTypes       */ "Map land types hashing",
            /* TechnoOccupancy */ "Object occupancy hashing",
            /* WaypointRadius  */ "Waypoint reveal radiuses",
            /* CrateOutlines   */ "Crate outlines",
            /* EffectRadius    */ "Jam / gap radiuses",
            /* HomeAreaBox     */ "Home waypoint start view",
        };

        private static readonly int[] TiberiumStages = new int[] { 0, 1, 3, 4, 6, 7, 8, 10, 11 };
        private static readonly int[] GemStages = new int[] { 0, 0, 0, 1, 1, 1, 2, 2, 2 };
        /// <summary>Facings to check for adjacent cells around even-cell concrete. The order in this array matches the bits order in the ConcAdj enum.</summary>
        private static readonly FacingType[] ConcreteCheckEven = { FacingType.North, FacingType.NorthWest, FacingType.West, FacingType.SouthWest, FacingType.South };
        /// <summary>Facings to check for adjacent cells around odd-cell concrete. The order in this array matches the bits order in the ConcAdj enum.</summary>
        private static readonly FacingType[] ConcreteCheckOdd = { FacingType.North, FacingType.NorthEast, FacingType.East, FacingType.SouthEast, FacingType.South };

        /// <summary>
        /// Represents the order of the different fill states inside the CONC sprite file.
        /// Each state has two icons; one for odd and one for even cells. Note that for some
        /// reason, the first state has these switched on the sprite; it has the even graphics first.
        /// </summary>
        private static readonly ConcFill[] IconFillStates = {
            /* 0 */ ConcFill.Center,
            /* 1 */ ConcFill.Center | ConcFill.Bottom | ConcFill.Top,
            /* 2 */ ConcFill.Top,
            /* 3 */ ConcFill.Bottom,
            /* 4 */ ConcFill.Center | ConcFill.Bottom,
            /* 5 */ ConcFill.Center | ConcFill.Top,
            /* 6 */ ConcFill.Bottom | ConcFill.Top,
        };

        private static readonly Dictionary<ConcFill, int> concreteStateToIcon = IconFillStates.Select((value, index) => new { value, index })
            .Append(new { value = ConcFill.None, index = 0 }).ToDictionary(pair => pair.value, pair => pair.index);

        private static readonly Regex TileInfoSplitRegex = new Regex("^([^:]+):(\\d+)$", RegexOptions.Compiled);

        private int updateCount = 0;
        private bool updating = false;
        private IDictionary<MapLayerFlag, ISet<Point>> invalidateLayers = new Dictionary<MapLayerFlag, ISet<Point>>();
        private bool invalidateOverlappers;

        public event EventHandler<MapRefreshEventArgs> RulesChanged;
        public void NotifyRulesChanges(ISet<Point> refreshPoints)
        {
            if (RulesChanged != null)
            {
                RulesChanged(this, new MapRefreshEventArgs(refreshPoints));
            }
        }

        public event EventHandler<MapRefreshEventArgs> MapContentsChanged;
        public void NotifyMapContentsChanged(ISet<Point> refreshPoints)
        {
            if (MapContentsChanged != null)
            {
                MapContentsChanged(this, new MapRefreshEventArgs(refreshPoints));
            }
        }

        public bool ConcreteOverlaysAvailable { get; private set; }
        public bool CrateOverlaysAvailable { get; private set; }
        public bool FlareWaypointAvailable { get; private set; }
        public bool ExpansionUnitsAvailable { get; private set; }

        public readonly BasicSection BasicSection;

        public readonly MapSection MapSection;

        public readonly BriefingSection BriefingSection = new BriefingSection();

        public readonly SteamSection SteamSection = new SteamSection();

        public TheaterType Theater { get => MapSection.Theater; set => MapSection.Theater = value; }

        public Point TopLeft
        {
            get => new Point(MapSection.X, MapSection.Y);
            set { MapSection.X = value.X; MapSection.Y = value.Y; }
        }

        public Size Size
        {
            get => new Size(MapSection.Width, MapSection.Height);
            set { MapSection.Width = value.Width; MapSection.Height = value.Height; }
        }

        public Rectangle Bounds
        {
            get => MapSection.Bounds;
            set { MapSection.X = value.Left; MapSection.Y = value.Top; MapSection.Width = value.Width; MapSection.Height = value.Height; }
        }

        public bool ForPreview { get; private set; }

        public readonly Type HouseType;

        public readonly HouseType[] HouseTypes;

        public readonly HouseType[] HouseTypesIncludingSpecials;

        public ITeamColor[] FlagColors { get; set; }

        public readonly List<TheaterType> TheaterTypes;

        public readonly List<TemplateType> TemplateTypes;

        /// <summary>Returns the template types that can actually exist in map data, excluding group dummies.</summary>
        /// <returns>An array of template types where each object is at the index of its own ID.</returns>
        public TemplateType[] GetMapTemplateTypes()
        {
            List<TemplateType> valids = TemplateTypes.Where(tt => !tt.IsGroup).ToList();
            if (valids.Count == 0)
            {
                return new TemplateType[0];
            }
            int size = valids.Max(tt => tt.ID) + 1;
            TemplateType[] templateTypes = new TemplateType[size];
            foreach (TemplateType tt in valids)
            {
                templateTypes[tt.ID] = tt;
            }
            return templateTypes;
        }

        public HashSet<LandType> UsedLandTypes = new HashSet<LandType>();

        public readonly List<TerrainType> TerrainTypes;

        public readonly List<OverlayType> OverlayTypes;

        public readonly List<SmudgeType> SmudgeTypes;

        public readonly string[] EventTypes;
        public readonly HashSet<string> CellEventTypes;
        public readonly HashSet<string> UnitEventTypes;
        public readonly HashSet<string> BuildingEventTypes;
        public readonly HashSet<string> TerrainEventTypes;

        public readonly string[] ActionTypes;
        public readonly HashSet<string> CellActionTypes;
        public readonly HashSet<string> UnitActionTypes;
        public readonly HashSet<string> BuildingActionTypes;
        public readonly HashSet<string> TerrainActionTypes;

        public readonly string[] MissionTypes;

        private const string defaultMission = "Guard";
        private readonly string inputMissionArmed;
        private readonly string inputMissionUnarmed;
        private readonly string inputMissionAircraft;
        private readonly string inputMissionHarvest;

        public readonly string DefaultMissionArmed;
        public readonly string DefaultMissionUnarmed;
        public readonly string DefaultMissionAircraft;
        public readonly string DefaultMissionHarvest;

        public readonly List<DirectionType> BuildingDirectionTypes;

        public readonly List<DirectionType> UnitDirectionTypes;

        public readonly List<InfantryType> AllInfantryTypes;
        public List<InfantryType> InfantryTypes
        {
            get
            {
                if (BasicSection == null || !BasicSection.ExpansionEnabled)
                {
                    return AllInfantryTypes.Where(inf => !inf.IsExpansionOnly).ToList();
                }
                return AllInfantryTypes.ToList();
            }
        }

        public readonly List<UnitType> AllUnitTypes;
        public List<UnitType> UnitTypes
        {
            get
            {
                if (BasicSection == null || !BasicSection.ExpansionEnabled)
                {
                    return AllUnitTypes.Where(un => !un.IsExpansionOnly).ToList();
                }
                return AllUnitTypes.ToList();
            }
        }

        public readonly List<BuildingType> BuildingTypes;

        public readonly List<ITechnoType> AllTeamTechnoTypes;
        public List<ITechnoType> TeamTechnoTypes
        {
            get
            {
                if (BasicSection == null || !BasicSection.ExpansionEnabled)
                {
                    return AllTeamTechnoTypes.Where(tc => !tc.IsExpansionOnly).ToList();
                }
                return AllTeamTechnoTypes.ToList();
            }
        }

        public readonly TeamMission[] TeamMissionTypes;

        public readonly CellMetrics Metrics;

        public readonly CellGrid<Template> Templates;

        public readonly CellGrid<Overlay> Overlay;

        public readonly CellGrid<Smudge> Smudge;

        public readonly OccupierSet<ICellOccupier> Technos;

        public readonly OccupierSet<ICellOccupier> Buildings;

        public readonly OverlapperSet<ICellOverlapper> Overlappers;

        public readonly Waypoint[] Waypoints;

        public event EventHandler<EventArgs> WaypointsUpdated;

        public void NotifyWaypointsUpdate()
        {
            if (WaypointsUpdated != null)
                WaypointsUpdated(this, new EventArgs());
        }

        public int DropZoneRadius { get; set; }

        public int GapRadius { get; set; }

        public int RadarJamRadius { get; set; }

        public readonly CellGrid<CellTrigger> CellTriggers;

        public event EventHandler<EventArgs> TriggersUpdated;

        public void NotifyTriggersUpdate()
        {
            if (TriggersUpdated != null)
                TriggersUpdated(this, new EventArgs());
        }

        private List<Trigger> triggers;
        public List<Trigger> Triggers
        {
            get { return triggers; }
            set
            {
                triggers = value;
                // Only an actual replacing of the list will call these, but they can be called manually after an update.
                // A bit more manual than the whole ObservableCollection system, but a lot less cumbersome.
                NotifyTriggersUpdate();
            }
        }

        public readonly List<TeamType> TeamTypes;

        public House[] Houses;
        public House[] HousesIncludingSpecials;
        public House[] HousesForAlliances;
        public House HouseNone;

        public string MovieEmpty;
        public readonly List<string> MovieTypes;
        public readonly string ThemeEmpty;
        public readonly List<string> ThemeTypes;

        ///<summary>The value for the basic resource on the map.</summary>
        public int TiberiumOrGoldValue { get; set; }
        /// <summary>The value for the high-value resource on the map.</summary>
        public int GemValue { get; set; }

        /// <summary>Gets the total amount of resources on the map inside the map border.</summary>
        public int ResourcesOnMap
        {
            get
            {
                return GetTotalResources(true);
            }
        }

        /// <summary>
        /// Initialises a new Map object.
        /// </summary>
        /// <param name="basicSection">ini [Basic] section in the specific object type for this game.</param>
        /// <param name="theater">Theater of the map.</param>
        /// <param name="cellSize">Size of the map, in cells.</param>
        /// <param name="houseType">Type to use for creating House objects from houseTypes.</param>
        /// <param name="houseTypes">The list of house types.</param>
        /// <param name="flagColors">The list of team colors to use for multiplayer start location indicators.</param>
        /// <param name="theaterTypes">The list of all theaters supported by this game.</param>
        /// <param name="templateTypes">The list of all template types supported by this game.</param>
        /// <param name="terrainTypes">The list of all terrain types supported by this game.</param>
        /// <param name="overlayTypes">The list of all overlay types supported by this game.</param>
        /// <param name="smudgeTypes">The list of all smudge types supported by this game.</param>
        /// <param name="eventTypes">The list of all event types supported by this game.</param>
        /// <param name="cellEventTypes">The list of all event types applicable to cells in this game.</param>
        /// <param name="unitEventTypes">The list of all event types applicable to units in this game.</param>
        /// <param name="buildingEventTypes">The list of all event types applicable to buildings in this game.</param>
        /// <param name="terrainEventTypes">The list of all event types applicable to terrain in this game.</param>
        /// <param name="actionTypes">The list of all action types supported by this game.</param>
        /// <param name="cellActionTypes">The list of all action types applicable to cells in this game.</param>
        /// <param name="unitActionTypes">The list of all action types applicable to units in this game.</param>
        /// <param name="buildingActionTypes">The list of all action types applicable to buildings in this game.</param>
        /// <param name="terrainActionTypes">The list of all action types applicable to terrain in this game.</param>
        /// <param name="missionTypes">The list of all mission types (orders) supported by this game.</param>
        /// <param name="armedMission">The default mission for armed units.</param>
        /// <param name="unarmedMission">The default mission for unarmed units.</param>
        /// <param name="harvestMission">The default mission for harvesting units.</param>
        /// <param name="aircraftMission">The default mission for air units.</param>
        /// <param name="unitDirectionTypes">The list of all direction types applicable to units.</param>
        /// <param name="buildingDirectionTypes">The list of all direction types applicable to structures.</param>
        /// <param name="infantryTypes">The list of all infantry types.</param>
        /// <param name="unitTypes">The list of all unit types.</param>
        /// <param name="buildingTypes">The list of all building types.</param>
        /// <param name="teamMissionTypes">The list of all mission types (orders) usable by teams.</param>
        /// <param name="teamTechnoTypes">The list of all techno types usable in teams.</param>
        /// <param name="waypoints">The list of waypoints.</param>
        /// <param name="dropZoneRadius">The radius that is revealed around a dropzone waypoint.</param>
        /// <param name="gapRadius">The radius that is affected by a gap generator.</param>
        /// <param name="jamRadius">The radius that is affected by a radar jammer.</param>
        /// <param name="movieTypes">The list of all movies usable by this game.</param>
        /// <param name="emptyMovie">The name to use for detecting and saving an empty movie entry.</param>
        /// <param name="themeTypes">The list of all music usable by this game.</param>
        /// <param name="emptyTheme">The name to use for detecting and saving an empty music entry.</param>
        /// <param name="tiberiumOrGoldValue">The value for the basic resource on the map.</param>
        /// <param name="gemValue">The value for the high-value resource on the map.</param>
        public Map(BasicSection basicSection, TheaterType theater, Size cellSize, Type houseType, IEnumerable<HouseType> houseTypes,
            ITeamColor[] flagColors, IEnumerable<TheaterType> theaterTypes, IEnumerable<TemplateType> templateTypes,
            IEnumerable<TerrainType> terrainTypes, IEnumerable<OverlayType> overlayTypes, IEnumerable<SmudgeType> smudgeTypes,
            IEnumerable<string> eventTypes, IEnumerable<string> cellEventTypes, IEnumerable<string> unitEventTypes, IEnumerable<string> buildingEventTypes, IEnumerable<string> terrainEventTypes,
            IEnumerable<string> actionTypes, IEnumerable<string> cellActionTypes, IEnumerable<string> unitActionTypes, IEnumerable<string> buildingActionTypes, IEnumerable<string> terrainActionTypes,
            IEnumerable<string> missionTypes, string armedMission, string unarmedMission, string harvestMission, string aircraftMission,
            IEnumerable<DirectionType> unitDirectionTypes, IEnumerable<DirectionType> buildingDirectionTypes,
            IEnumerable<InfantryType> infantryTypes, IEnumerable<UnitType> unitTypes, IEnumerable<BuildingType> buildingTypes,
            IEnumerable<TeamMission> teamMissionTypes, IEnumerable<ITechnoType> teamTechnoTypes, IEnumerable<Waypoint> waypoints,
            IEnumerable<string> movieTypes, string emptyMovie, IEnumerable<string> themeTypes, string emptyTheme,
            int dropZoneRadius, int gapRadius, int jamRadius, int tiberiumOrGoldValue, int gemValue)
        {
            MapSection = new MapSection(cellSize);
            BasicSection = basicSection;
            HouseType = houseType;
            HouseType[] allHouseTypes = houseTypes.ToArray();
            HouseTypesIncludingSpecials = houseTypes.ToArray();
            HouseTypes = allHouseTypes.Where(h => !h.IsSpecial).ToArray();
            FlagColors = flagColors == null ? new ITeamColor[8] : flagColors;
            TheaterTypes = new List<TheaterType>(theaterTypes);
            TemplateTypes = new List<TemplateType>(templateTypes);
            TerrainTypes = new List<TerrainType>(terrainTypes);
            OverlayTypes = new List<OverlayType>(overlayTypes);
            SmudgeTypes = new List<SmudgeType>(smudgeTypes);
            EventTypes = eventTypes.ToArray();
            CellEventTypes = cellEventTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            UnitEventTypes = unitEventTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            BuildingEventTypes = buildingEventTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            TerrainEventTypes = terrainEventTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            CellActionTypes = cellActionTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            UnitActionTypes = unitActionTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            BuildingActionTypes = buildingActionTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            TerrainActionTypes = terrainActionTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);

            ActionTypes = actionTypes.ToArray();
            MissionTypes = missionTypes.ToArray();
            string defMission = MissionTypes.Where(m => m.Equals(defaultMission, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() ?? MissionTypes.First();
            // Unfiltered originals, to ensure this remains correct when cloning.
            inputMissionArmed = armedMission;
            inputMissionUnarmed = unarmedMission;
            inputMissionAircraft = harvestMission;
            inputMissionHarvest = aircraftMission;

            DefaultMissionArmed = MissionTypes.Where(m => m.Equals(armedMission, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() ?? defMission;
            DefaultMissionUnarmed = MissionTypes.Where(m => m.Equals(unarmedMission, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() ?? defMission;
            // Reverts to "Stop" if there are no resources (RA indoor)
            DefaultMissionHarvest = OverlayTypes.Any(ov => ov.IsResource) ? MissionTypes.Where(m => m.Equals(harvestMission, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() ?? DefaultMissionUnarmed : DefaultMissionUnarmed;
            // Only "Unload" will make them stay on the spot as expected.
            DefaultMissionAircraft = MissionTypes.Where(m => m.Equals(aircraftMission, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() ?? defMission;
            UnitDirectionTypes = new List<DirectionType>(unitDirectionTypes);
            BuildingDirectionTypes = new List<DirectionType>(buildingDirectionTypes);
            AllInfantryTypes = new List<InfantryType>(infantryTypes);
            AllUnitTypes = new List<UnitType>(unitTypes);
            BuildingTypes = new List<BuildingType>(buildingTypes);
            TeamMissionTypes = teamMissionTypes.ToArray();
            AllTeamTechnoTypes = new List<ITechnoType>(teamTechnoTypes);
            MovieEmpty = emptyMovie;
            MovieTypes = new List<string>(movieTypes);
            ThemeEmpty = emptyTheme;
            ThemeTypes = new List<string>(themeTypes);
            TiberiumOrGoldValue = tiberiumOrGoldValue;
            GemValue = gemValue;
            Metrics = new CellMetrics(cellSize);
            Templates = new CellGrid<Template>(Metrics);
            Overlay = new CellGrid<Overlay>(Metrics);
            Smudge = new CellGrid<Smudge>(Metrics);
            Technos = new OccupierSet<ICellOccupier>(Metrics);
            Buildings = new OccupierSet<ICellOccupier>(Metrics);
            Overlappers = new OverlapperSet<ICellOverlapper>(Metrics);
            triggers = new List<Trigger>();
            TeamTypes = new List<TeamType>();
            House[] allHouses = allHouseTypes.Select(t => { House h = (House)Activator.CreateInstance(HouseType, t); h.SetDefault(); return h; }).ToArray();
            HousesIncludingSpecials = allHouses;
            Houses = allHouses.Where(h => !h.Type.IsSpecial).ToArray();
            // Build houses list for allies. Special houses not shown in the normal houses lists (e.g. 'Allies' and 'Soviet') are put first.
            List<House> housesAlly = allHouses.Where(h => h.Type.IsForAlliances).ToList();
            List<House> housesAllySpecial = housesAlly.Where(h => h.Type.IsSpecial).OrderBy(h => h.Type.ID).ToList();
            List<House> housesAllyNormal = housesAlly.Where(h => !h.Type.IsSpecial).OrderBy(h => h.Type.ID).ToList();
            // put special types at the start.
            HousesForAlliances = housesAllySpecial.Concat(housesAllyNormal).ToArray();
            HouseNone = allHouses.Where(h => h.Type.IsSpecial && h.Type.IsBaseHouse).FirstOrDefault();
            Waypoint[] wp = waypoints.ToArray();
            Waypoints = new Waypoint[wp.Length];
            for (int i = 0; i < wp.Length; ++i)
            {
                // Deep clone with current metrics, to allow showing waypoints as cell coordinates.
                Waypoints[i] = new Waypoint(wp[i].Name, wp[i].ShortName, wp[i].Flags, Metrics, wp[i].Cell);
            }
            DropZoneRadius = dropZoneRadius;
            GapRadius = gapRadius;
            RadarJamRadius = jamRadius;
            CellTriggers = new CellGrid<CellTrigger>(Metrics);

            // Optimisation: checks on what is inside the given data, used to prevent unnecessary logic from executing.
            ConcreteOverlaysAvailable = OverlayTypes.Any(ovl => ovl.IsConcrete);
            CrateOverlaysAvailable = OverlayTypes.Any(ovl => ovl.IsCrate);
            FlareWaypointAvailable = Waypoints.Any(wpt => wpt.Flags.HasFlag(WaypointFlag.Flare));
            ExpansionUnitsAvailable = BuildingTypes.Any(tt => tt.IsExpansionOnly)
                || AllInfantryTypes.Any(tt => tt.IsExpansionOnly)
                || TerrainTypes.Any(tt => tt.IsExpansionOnly)
                || AllUnitTypes.Any(tt => tt.IsExpansionOnly);

            MapSection.SetDefault();
            BriefingSection.SetDefault();
            SteamSection.SetDefault();
            Templates.Clear();
            Overlay.Clear();
            Smudge.Clear();
            Technos.Clear();
            Overlappers.Clear();
            CellTriggers.Clear();

            TopLeft = new Point(1, 1);
            Size = Metrics.Size - new Size(2, 2);
            Theater = theater;

            Overlay.CellChanged += Overlay_CellChanged;
            Technos.OccupierAdded += Technos_OccupierAdded;
            Technos.OccupierRemoved += Technos_OccupierRemoved;
            Buildings.OccupierAdded += Buildings_OccupierAdded;
            Buildings.OccupierRemoved += Buildings_OccupierRemoved;
        }

        public void BeginUpdate()
        {
            updateCount++;
        }

        public void EndUpdate()
        {
            if (--updateCount == 0)
            {
                Update();
            }
        }

        public void InitTheater(GameInfo gameInfo)
        {
            try
            {
                foreach (TemplateType templateType in TemplateTypes)
                {
                    templateType.Init(gameInfo, Theater, Globals.FilterTheaterObjects);
                }
                UsedLandTypes = TemplateTypes
                    .Where(tmp => tmp.ExistsInTheater)
                    .SelectMany(tmp => tmp.LandTypes ?? new LandType[0])
                    .Distinct()
                    .Where(lt => lt != LandType.None)
                    .ToHashSet();
                foreach (SmudgeType smudgeType in SmudgeTypes)
                {
                    smudgeType.Init(Theater);
                }
                foreach (OverlayType overlayType in OverlayTypes)
                {
                    overlayType.Init(gameInfo);
                }
                string th = Theater.Name;
                foreach (TerrainType terrainType in TerrainTypes)
                {
                    terrainType.Init();
                }
                // Ignore expansion status for these; they can still be enabled later.
                DirectionType infDir = UnitDirectionTypes.Where(d => d.Facing == FacingType.South).First();
                foreach (InfantryType infantryType in AllInfantryTypes)
                {
                    infantryType.Init(HouseTypesIncludingSpecials.Where(h => h.Equals(infantryType.OwnerHouse)).FirstOrDefault(), infDir);
                }
                DirectionType unitDir = UnitDirectionTypes.Where(d => d.Facing == FacingType.SouthWest).First();
                foreach (UnitType unitType in AllUnitTypes)
                {
                    unitType.Init(gameInfo, HouseTypesIncludingSpecials.Where(h => h.Equals(unitType.OwnerHouse)).FirstOrDefault(), unitDir);
                }
                // Required for initialising air unit names for teamtypes if DisableAirUnits is true.
                foreach (ITechnoType techno in AllTeamTechnoTypes)
                {
                    techno.InitDisplayName();
                }
                DirectionType bldDir = UnitDirectionTypes.Where(d => d.Facing == FacingType.North).First();
                // No restriction. All get attempted and dummies are all filled in.
                foreach (BuildingType buildingType in BuildingTypes)
                {
                    buildingType.Init(gameInfo, HouseTypesIncludingSpecials.Where(h => h.Equals(buildingType.OwnerHouse)).FirstOrDefault(), bldDir);
                }
            }
            catch (Exception ex)
            {
                if (!(ex is ThreadAbortException))
                {
                    string message = ex.Message;
                    if (ex is ArgumentException argex)
                    {
                        message = GeneralUtils.RecoverArgExceptionMessage(argex, false);
                    }
                    System.Windows.Forms.MessageBox.Show("An error occurred while initialising the map data for the current theater.\n\n" + ex.Message + "\n" + ex.StackTrace, "Whoops!");
                }
            }
        }

        private void Update()
        {
            updating = true;
            if (invalidateLayers.TryGetValue(MapLayerFlag.Resources, out ISet<Point> locations))
            {
                UpdateResourceOverlays(locations, true);
            }
            if (invalidateLayers.TryGetValue(MapLayerFlag.Walls, out locations))
            {
                // Not sure if needed; Buildings_OccupierAdded and Buildings_OccupierRemoved take care of this, since
                // adding a wall overlay triggers adding it to the buildings too.
                UpdateWallOverlays(locations);
            }
            if (invalidateLayers.TryGetValue(MapLayerFlag.Overlay, out locations))
            {
                UpdateConcreteOverlays(locations);
            }
            if (invalidateOverlappers)
            {
                Overlappers.Clear();
                foreach ((Point location, ICellOccupier techno) in Technos)
                {
                    if (techno is ICellOverlapper)
                    {
                        Overlappers.Add(location, techno as ICellOverlapper);
                    }
                }
                foreach ((Point location, ICellOccupier bld) in Buildings)
                {
                    if (bld is ICellOverlapper)
                    {
                        Overlappers.Add(location, bld as ICellOverlapper);
                    }
                }
            }
            invalidateLayers.Clear();
            invalidateOverlappers = false;
            updating = false;
        }

        /// <summary>
        /// Returns the combined value of all resources placed on the map. Can be specified to only calculate inside the set map bounds.
        /// This method does not rely on the set icon, but recalculates the density of all resource cells.
        /// </summary>
        /// <param name="inBounds">Specifically calculate only resources inside the set map bounds.</param>
        /// <returns>The combined value of all resources on the map.</returns>
        private int GetTotalResources(bool inBounds)
        {
            int totalResources = 0;
            foreach ((int cell, Overlay value) in Overlay)
            {
                Point point;
                if (!value.Type.IsResource || !Metrics.GetLocation(cell, out point))
                {
                    continue;
                }
                if (inBounds && !Bounds.Contains(point))
                {
                    continue;
                }
                int adj = 0;
                foreach (FacingType facing in CellMetrics.AdjacentFacings)
                {
                    Overlay ovl;
                    if (Metrics.Adjacent(point, facing, out Point adjPoint)
                        && (ovl = Overlay[adjPoint]) != null && ovl.Type.IsResource)
                    {
                        if (inBounds && !Bounds.Contains(adjPoint))
                        {
                            continue;
                        }
                        adj++;
                    }
                }
                int thickness = value.Type.IsGem ? GemStages[adj] : TiberiumStages[adj];
                // Harvesting has a bug where the final stage returns a value of 0 since it uses the 0-based icon index.
                // Harvesting one gem stage fills one bail, plus 3 extra bails. Last stage is 0 (due to that bug), but still gets the extra bails.
                if (Globals.ApplyHarvestBug)
                {
                    totalResources += value.Type.IsGem ? thickness * GemValue + GemValue * 3 : thickness * TiberiumOrGoldValue;
                }
                else
                {
                    // Fixed logic, in case it is repaired in the code.
                    totalResources += (thickness + 1) * (value.Type.IsGem ? GemValue * 4 : TiberiumOrGoldValue);
                }
            }
            return totalResources;
        }

        /// <summary>
        /// Update resource overlay to the desired density and randomised type.
        /// </summary>
        /// <param name="locations">Set of Locations on which changes occurred.</param>
        /// <param name="reduceOutOfBounds">True if resources out of bounds are reduced to minimum size to indicate they are not valid.</param>
        /// <remarks> This function is separate from GetTotalResources because it only updates the specified areas.</remarks>
        public void UpdateResourceOverlays(ISet<Point> locations, bool reduceOutOfBounds)
        {
            Rectangle checkBounds = reduceOutOfBounds ? Bounds : Metrics.Bounds;
            OverlayType[] tiberiumOrGoldTypes = OverlayTypes.Where(t => t.IsTiberiumOrGold).ToArray();
            if (tiberiumOrGoldTypes.Length == 0) tiberiumOrGoldTypes = null;
            OverlayType[] gemTypes = OverlayTypes.Where(t => t.IsGem).ToArray();
            if (gemTypes.Length == 0) gemTypes = null;
            foreach ((Point location, Overlay overlay) in Overlay.IntersectsWithPoints(locations).Where(o => o.Value.Type.IsResource))
            {
                int count = 0;
                if (checkBounds.Contains(location))
                {
                    foreach (FacingType facing in CellMetrics.AdjacentFacings)
                    {
                        if (Metrics.Adjacent(location, facing, out Point adjacent) && checkBounds.Contains(adjacent))
                        {
                            Overlay adjacentOverlay = Overlay[adjacent];
                            if (adjacentOverlay?.Type.IsResource ?? false)
                            {
                                count++;
                            }
                        }
                    }
                }
                OverlayType ovType = overlay.Type;
                if (ovType.IsGem && gemTypes != null)
                {
                    overlay.Type = gemTypes[new Random(randomSeed ^ location.GetHashCode()).Next(gemTypes.Length)];
                }
                else if (ovType.IsTiberiumOrGold && tiberiumOrGoldTypes != null)
                {
                    overlay.Type = tiberiumOrGoldTypes[new Random(randomSeed ^ location.GetHashCode()).Next(tiberiumOrGoldTypes.Length)];
                }
                overlay.Icon = overlay.Type.IsGem ? GemStages[count] : TiberiumStages[count];
            }
        }

        public void UpdateWallOverlays(ISet<Point> locations)
        {
            foreach ((Point location, Overlay overlay) in Overlay.IntersectsWithPoints(locations).Where(o => o.Value.Type.IsWall))
            {
                OverlayType ovt = overlay.Type;
                bool hasNorthWall = Overlay.Adjacent(location, FacingType.North)?.Type == ovt;
                bool hasEastWall = Overlay.Adjacent(location, FacingType.East)?.Type == ovt;
                bool hasSouthWall = Overlay.Adjacent(location, FacingType.South)?.Type == ovt;
                bool hasWestWall = Overlay.Adjacent(location, FacingType.West)?.Type == ovt;
                if (Globals.AllowWallBuildings)
                {
                    string ovtName = overlay.Type.Name;
                    hasNorthWall |= (Metrics.Adjacent(location, FacingType.North, out Point north) ? Buildings[north] as Building : null)?.Type.Name == ovtName;
                    hasEastWall |= (Metrics.Adjacent(location, FacingType.East, out Point east) ? Buildings[east] as Building : null)?.Type.Name == ovtName;
                    hasSouthWall |= (Metrics.Adjacent(location, FacingType.South, out Point south) ? Buildings[south] as Building : null)?.Type.Name == ovtName;
                    hasWestWall |= (Metrics.Adjacent(location, FacingType.West, out Point west) ? Buildings[west] as Building : null)?.Type.Name == ovtName;
                }
                int icon = (hasNorthWall ? 1 : 0) | (hasEastWall ? 2 : 0) | (hasSouthWall ? 4 : 0) | (hasWestWall ? 8 : 0);
                overlay.Icon = icon;
            }
        }

        public static bool IsIgnorableOverlay(Overlay overlay)
        {
            if (overlay == null)
                return true;
            if (overlay.Type.IsConcrete && !GetConcState(overlay).HasFlag(ConcFill.Center))
            {
                // Filler cell. Ignore.
                return true;
            }
            return false;
        }

        public void UpdateConcreteOverlays(ISet<Point> locations)
        {
            if (!ConcreteOverlaysAvailable)
            {
                return;
            }
            if (Globals.FixConcretePavement)
            {
                UpdateConcreteOverlaysCorrect(locations, false);
            }
            else
            {
                UpdateConcreteOverlaysGame(locations);
            }
        }

        private void UpdateConcreteOverlaysCorrect(ISet<Point> locations, bool forExtraCells)
        {
            // Add the points around extra cells
            HashSet<Point> updateLocations = new HashSet<Point>(locations);
            HashSet<Point> newExtraCellsToAdd = new HashSet<Point>();
            foreach ((Point pt, Overlay overlay) in Overlay.IntersectsWithPoints(locations).Where(o => o.Value.Type.IsConcrete))
            {
                if (IsIgnorableOverlay(overlay))
                {
                    newExtraCellsToAdd.Add(pt);
                }
            }
            HashSet<Point> allExtraCells = new HashSet<Point>();
            // Add all locations that are adjacent to extra cells in a way that can affect them.
            while (newExtraCellsToAdd.Count > 0)
            {
                HashSet<Point> loopList = new HashSet<Point>(newExtraCellsToAdd);
                newExtraCellsToAdd.Clear();
                foreach ((Point pt, Overlay overlay) in Overlay.IntersectsWithPoints(loopList).Where(o => o.Value.Type.IsConcrete))
                {
                    if (!IsIgnorableOverlay(overlay))
                    {
                        continue;
                    }
                    FacingType[] adjCells = pt.X % 2 == 1 ? ConcreteCheckOdd : ConcreteCheckEven;
                    for (int i = 0; i < adjCells.Length; ++i)
                    {
                        if (!Metrics.Adjacent(pt, adjCells[i], out Point adjacent))
                        {
                            continue;
                        }
                        Overlay adj = Overlay[adjacent];
                        if (adj != null && adj.Type.IsConcrete)
                        {
                            updateLocations.Add(adjacent);
                            if (IsIgnorableOverlay(overlay) && !allExtraCells.Contains(pt))
                            {
                                newExtraCellsToAdd.Add(pt);
                                allExtraCells.Add(pt);
                            }
                        }
                    }
                }
            }
            Dictionary<int, ConcFill> addedCells = new Dictionary<int, ConcFill>();
            Dictionary<int, OverlayType> ovlTypes = new Dictionary<int, OverlayType>();
            HashSet<int> toRemove = new HashSet<int>();
            foreach ((int cell, Overlay overlay) in Overlay.IntersectsWithCells(updateLocations).Where(o => o.Value.Type.IsConcrete))
            {
                if (IsIgnorableOverlay(overlay))
                {
                    if (!forExtraCells)
                    {
                        toRemove.Add(cell);
                    }
                    continue;
                }
                // Cells to check around the current cell. In order: top, top-aside, aside, bottom-aside, bottom
                bool isodd = cell % 2 == 1;
                FacingType[] adjCells = isodd ? ConcreteCheckOdd : ConcreteCheckEven;
                ConcAdj mask = ConcAdj.None;
                int[] cells = new int[adjCells.Length];
                for (int i = 0; i < adjCells.Length; ++i)
                {
                    Overlay neighbor = Overlay.Adjacent(cell, adjCells[i]);
                    cells[i] = -1;
                    if (Metrics.Adjacent(cell, adjCells[i], out int adjacent))
                        cells[i] = adjacent;
                    if (neighbor?.Type == overlay.Type)
                    {
                        if (GetConcState(neighbor).HasFlag(ConcFill.Center))
                        {
                            mask |= (ConcAdj)(1 << i);
                        }
                    }
                }
                // Unified logic so the operation becomes identical for the even and odd cells.
                bool top = mask.HasFlag(ConcAdj.Top);
                bool topSide = mask.HasFlag(ConcAdj.TopSide);
                bool side = mask.HasFlag(ConcAdj.Side);
                bool bottomSide = mask.HasFlag(ConcAdj.BottomSide);
                bool bottom = mask.HasFlag(ConcAdj.Bottom);
                // Logic to fill the main cell. Standard for a placed cell is to fill the center.
                ConcFill fillState = ConcFill.Center;
                // NEW LOGIC: Fills triangle between two vertical cells
                // If cell at top, connect. If not, still connect if the two in front are filled.
                if (top || (topSide && side))
                {
                    fillState |= ConcFill.Top;
                }
                // If cell at bottom, connect. If not, still connect if the two in front are filled.
                if (bottom || (bottomSide && side))
                {
                    fillState |= ConcFill.Bottom;
                }
                // Logic to fill in edge cells. See what the currently evaluated cell will add to it.
                int cellTop = cells[0];
                int cellSide = cells[2];
                int cellBottom = cells[4];
                ConcFill fillStateTop = ConcFill.None;
                if (!top && (fillState & ConcFill.Top) != 0)
                {
                    fillStateTop |= ConcFill.Bottom;
                }
                ConcFill fillStateBottom = ConcFill.None;
                if (!bottom && (fillState & ConcFill.Bottom) != 0)
                {
                    fillStateBottom |= ConcFill.Top;
                }
                ConcFill fillStateSide = ConcFill.None;
                if (!side)
                {
                    if (topSide && top)
                        fillStateSide |= ConcFill.Top;
                    if (bottomSide && bottom)
                        fillStateSide |= ConcFill.Bottom;
                }
                // Only update if this is not for side cells.
                if (!forExtraCells)
                {
                    overlay.Icon = GetConcIcon(fillState, isodd);
                }
                // add concrete to fill up corners, completely fixing the way it should look.
                if (fillStateTop != ConcFill.None && cellTop != -1)
                {
                    ConcFill current = addedCells.ContainsKey(cellTop) ? addedCells[cellTop] : ConcFill.None;
                    addedCells[cellTop] = current | fillStateTop;
                    ovlTypes[cellTop] = overlay.Type;
                }
                if (fillStateBottom != ConcFill.None && cellBottom != -1)
                {
                    ConcFill current = addedCells.ContainsKey(cellBottom) ? addedCells[cellBottom] : ConcFill.None;
                    addedCells[cellBottom] = current | fillStateBottom;
                    ovlTypes[cellBottom] = overlay.Type;
                }
                if (fillStateSide != ConcFill.None && cellSide != -1)
                {
                    ConcFill current = addedCells.ContainsKey(cellSide) ? addedCells[cellSide] : ConcFill.None;
                    addedCells[cellSide] = current | fillStateSide;
                    ovlTypes[cellSide] = overlay.Type;
                }
            }
            List<int> addCells = addedCells.Keys.ToList();
            addCells.Sort();
            HashSet<Point> addedPoints = new HashSet<Point>();
            foreach (int cell in addCells)
            {
                Point? pt = Metrics.GetLocation(cell);
                toRemove.Remove(cell);
                // Only allow updating the actual given points.
                if (forExtraCells && pt != null && !locations.Contains(pt.Value))
                    continue;
                OverlayType toMake = ovlTypes[cell];
                ConcFill addState = addedCells[cell];
                Overlay ovl = Overlay[cell];
                bool isNew = ovl == null;
                if (isNew)
                {
                    ovl = new Overlay();
                    ovl.Type = toMake;
                }
                else
                {
                    if (ovl.Type != toMake || !IsIgnorableOverlay(ovl))
                    {
                        continue;
                    }
                }
                ovl.Icon = GetConcIcon(addState, cell % 2 == 1);
                if (isNew)
                {
                    Overlay[cell] = ovl;
                }
                if (!forExtraCells && pt.HasValue)
                {
                    addedPoints.Add(pt.Value);
                }
            }
            foreach (int cell in toRemove)
            {
                Overlay[cell] = null;
            }
            if (!forExtraCells && addedPoints.Count > 0)
            {
                UpdateConcreteOverlaysCorrect(addedPoints, true);
            }
        }

        private void UpdateConcreteOverlaysGame(ISet<Point> locations)
        {
            foreach ((int cell, Overlay overlay) in Overlay.IntersectsWithCells(locations).Where(o => o.Value.Type.IsConcrete))
            {
                bool isodd = (cell & 1) == 1;
                // Cells to check around the current cell. In order: top, top side, side, bottom side, bottom
                FacingType[] adjCells = isodd ? ConcreteCheckOdd : ConcreteCheckEven;
                ConcAdj mask = 0;
                for (int i = 0; i < adjCells.Length; ++i)
                {
                    Overlay neighbor = Overlay.Adjacent(cell, adjCells[i]);
                    if (neighbor != null && neighbor.Type.IsConcrete && !IsIgnorableOverlay(overlay))
                    {
                        mask |= (ConcAdj)(1 << i);
                    }
                }
                ConcFill fillState = ConcFill.Center;
                // Not trying to find any logic in this; it's a mess. I just copied all these cases from screenshots.
                if (isodd)
                {
                    switch (mask)
                    {
                        case (ConcAdj.Top | ConcAdj.TopSide | ConcAdj.Side | ConcAdj.BottomSide | ConcAdj.Bottom):
                        case (ConcAdj.TopSide | ConcAdj.Side | ConcAdj.BottomSide | ConcAdj.Bottom):
                        case (ConcAdj.TopSide | ConcAdj.Side | ConcAdj.Bottom):
                        case (ConcAdj.Top | ConcAdj.TopSide | ConcAdj.Side | ConcAdj.Bottom):
                        case (ConcAdj.Top | ConcAdj.Side | ConcAdj.BottomSide | ConcAdj.Bottom):
                        case (ConcAdj.Top | ConcAdj.Side | ConcAdj.Bottom):
                            fillState |= ConcFill.Top | ConcFill.Bottom;
                            break;
                        case (ConcAdj.Top | ConcAdj.TopSide | ConcAdj.Side | ConcAdj.BottomSide):
                        case (ConcAdj.Top | ConcAdj.TopSide | ConcAdj.BottomSide | ConcAdj.Bottom):
                        case (ConcAdj.TopSide | ConcAdj.Side | ConcAdj.BottomSide):
                        case (ConcAdj.Top | ConcAdj.TopSide | ConcAdj.Side):
                        case (ConcAdj.Top | ConcAdj.Side | ConcAdj.BottomSide):
                        case (ConcAdj.TopSide | ConcAdj.BottomSide):
                        case (ConcAdj.Top | ConcAdj.TopSide | ConcAdj.Bottom):
                        case (ConcAdj.Top | ConcAdj.TopSide | ConcAdj.BottomSide):
                        case (ConcAdj.Top | ConcAdj.TopSide):
                        case (ConcAdj.TopSide | ConcAdj.Side):
                        case (ConcAdj.TopSide):
                        case (ConcAdj.Top | ConcAdj.Side):
                            fillState |= ConcFill.Top;
                            break;
                        case (ConcAdj.Side | ConcAdj.BottomSide | ConcAdj.Bottom):
                        case (ConcAdj.Side | ConcAdj.Bottom):
                            fillState |= ConcFill.Bottom;
                            break;
                    }
                }
                else
                {
                    switch (mask)
                    {
                        case (ConcAdj.Top | ConcAdj.TopSide | ConcAdj.Side | ConcAdj.BottomSide | ConcAdj.Bottom):
                        case (ConcAdj.TopSide | ConcAdj.Side | ConcAdj.BottomSide | ConcAdj.Bottom):
                        case (ConcAdj.Top | ConcAdj.TopSide | ConcAdj.BottomSide | ConcAdj.Bottom):
                        case (ConcAdj.TopSide | ConcAdj.Side | ConcAdj.Bottom):
                        case (ConcAdj.TopSide | ConcAdj.BottomSide | ConcAdj.Bottom):
                        case (ConcAdj.Top | ConcAdj.TopSide | ConcAdj.Side | ConcAdj.Bottom):
                        case (ConcAdj.Top | ConcAdj.Side | ConcAdj.BottomSide | ConcAdj.Bottom):
                        case (ConcAdj.Top | ConcAdj.Side | ConcAdj.Bottom):
                            fillState |= ConcFill.Top | ConcFill.Bottom;
                            break;
                        case (ConcAdj.Top | ConcAdj.TopSide | ConcAdj.Side | ConcAdj.BottomSide):
                        case (ConcAdj.TopSide | ConcAdj.Side | ConcAdj.BottomSide):
                        case (ConcAdj.Top | ConcAdj.Side | ConcAdj.BottomSide):
                        case (ConcAdj.Top | ConcAdj.TopSide | ConcAdj.Side):
                        case (ConcAdj.TopSide | ConcAdj.Side):
                        case (ConcAdj.TopSide | ConcAdj.BottomSide):
                        case (ConcAdj.Top | ConcAdj.TopSide | ConcAdj.Bottom):
                        case (ConcAdj.Top | ConcAdj.TopSide | ConcAdj.BottomSide):
                        case (ConcAdj.Top | ConcAdj.TopSide):
                        case (ConcAdj.Top | ConcAdj.Side):
                        case (ConcAdj.TopSide):
                            fillState |= ConcFill.Top;
                            break;
                        case (ConcAdj.Side | ConcAdj.BottomSide | ConcAdj.Bottom):
                        case (ConcAdj.Top | ConcAdj.BottomSide | ConcAdj.Bottom):
                        case (ConcAdj.Side | ConcAdj.Bottom):
                        case (ConcAdj.BottomSide | ConcAdj.Bottom):
                            fillState |= ConcFill.Bottom;
                            break;
                    }
                }
                overlay.Icon = GetConcIcon(fillState, isodd);
            }
        }

        private static ConcFill GetConcState(Overlay conc)
        {
            if (!conc.Type.IsConcrete)
            {
                return ConcFill.None;
            }
            int val = conc.Icon / 2;
            if (val < 0 || val > IconFillStates.Length)
            {
                return ConcFill.None;
            }
            return IconFillStates[val];
        }

        private static int GetConcIcon(ConcFill fillState, bool isOdd)
        {
            int odd = isOdd ? 1 : 0;
            concreteStateToIcon.TryGetValue(fillState, out int val);
            // For some reason the odd and even icons for state 0 are swapped compared to all the others, so
            // an extra check has to be added for that. Otherwise just "(state * 2) + 1 - odd" would suffice.
            return val == 0 ? odd : (val * 2) + 1 - odd;
        }

        public void SetMapTemplatesRaw(byte[] data, int width, int height, Dictionary<int, string> types, string fillType)
        {
            int maxY = Math.Min(Metrics.Height, height);
            int maxX = Math.Min(Metrics.Width, width);
            Dictionary<int, TemplateType> replaceTypes = new Dictionary<int, TemplateType>();
            Dictionary<int, int> replaceIcons = new Dictionary<int, int>();
            int fillIcon;
            TemplateType fillTile;
            SplitTileInfo(fillType, out fillTile, out fillIcon, "fillType", false);
            if (fillTile != null)
            {
                Point? fillPoint = fillTile.GetIconPoint(fillIcon);
                if (!fillPoint.HasValue || !fillTile.IsValidIcon(fillPoint.Value))
                {
                    fillIcon = fillTile.GetIconIndex(fillTile.GetFirstValidIcon());
                }
            }
            foreach (KeyValuePair<int, string> kvp in types)
            {
                string tileType = kvp.Value;
                int tileIcon;
                TemplateType tile;
                SplitTileInfo(tileType, out tile, out tileIcon, "types", false);
                replaceTypes[kvp.Key] = tile;
                if (tile != null)
                {
                    if (tile.Flags.HasFlag(TemplateTypeFlag.Group))
                    {
                        tile = tileIcon >= tile.GroupTiles.Length ?
                            null : TemplateTypes.Where(t => t.Name == tile.GroupTiles[tileIcon]).FirstOrDefault();
                        replaceTypes[kvp.Key] = tile;
                        tileIcon = 0;
                    }
                    else
                    {
                        Point? tilePoint = tile.GetIconPoint(tileIcon);
                        if (!tilePoint.HasValue || !tile.IsValidIcon(tilePoint.Value))
                        {
                            tileIcon = fillTile.GetIconIndex(fillTile.GetFirstValidIcon());
                        }
                    }
                    replaceIcons[kvp.Key] = tileIcon;
                }
            }
            Templates.Clear();
            int lineOffset = 0;
            int stride = width * 4;
            for (int y = 0; y < maxY; ++y)
            {
                int offset = lineOffset;
                for (int x = 0; x < maxX; ++x)
                {
                    // ARGB = [BB GG RR AA]
                    int col = 0xFF << 24 | data[offset + 2] << 16 | data[offset + 1] << 8 | data[offset];
                    TemplateType curr;
                    if (replaceTypes.TryGetValue(col, out curr))
                    {
                        // If clear terrain, don't bother doing anything; this started with a map clear.
                        if (curr != null)
                        {
                            int icon = replaceIcons.ContainsKey(col) ? replaceIcons[col] : 0;
                            Templates[y, x] = curr == null ? null : new Template { Type = curr, Icon = icon };
                        }
                    }
                    else if (fillTile != null)
                    {
                        Templates[y, x] = new Template { Type = fillTile, Icon = fillIcon };
                    }
                    offset += 4;
                }
                lineOffset += stride;
            }
        }

        public void SplitTileInfo(string tileType, out TemplateType tile, out int tileIcon, string context, bool safe)
        {
            tileIcon = 0;
            tile = null;
            if (tileType == null)
            {
                return;
            }
            Match m = TileInfoSplitRegex.Match(tileType);
            if (m.Success)
            {
                tileType = m.Groups[1].Value;
                tileIcon = Int32.Parse(m.Groups[2].Value);
            }
            tile = TemplateTypes.Where(t => String.Equals(tileType, t.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            string th = Theater.Name;
            if (tile == null)
            {
                if (!safe)
                {
                    throw new ArgumentException(String.Format("Cannot find tile type '{0}'!", tileType), context);
                }
            }
            else if (!tile.ExistsInTheater)
            {
                if (!safe)
                {
                    throw new ArgumentException(String.Format("Tile type '{0}' does not exist in theater {1}.", tileType, th), context);
                }
                else
                {
                    tile = null;
                }
            }
        }

        public string GetCellDescription(Point location, Point subPixel)
        {
            if (!Metrics.GetCell(location, out int cell))
            {
                return String.Format("X = {0}, Y = {1}, No cell", location.X, location.Y);
            }
            bool inBounds = Bounds.Contains(location);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("X = {0}, Y = {1}, Cell = {2}", location.X, location.Y, cell);
            Template template = Templates[cell];
            TemplateType templateType = template?.Type;
            if (templateType != null)
            {
                sb.AppendFormat(", Template = {0} ({1}) ({2})", templateType.DisplayName, template.Icon, template.Type.GetLandType(template.Icon).ToString());
            }
            Smudge smudge = Smudge[cell];
            SmudgeType smudgeType = smudge?.Type;
            if (smudgeType != null)
            {
                sb.AppendFormat(", Smudge = {0}{1}", smudgeType.DisplayName, smudge.AttachedTo != null ? " (Attached)" : String.Empty);
            }
            Overlay overlay = Overlay[cell];
            OverlayType overlayType = overlay?.Type;
            if (overlayType != null)
            {
                if (overlayType.IsResource)
                {
                    sb.AppendFormat(", Resource = {0} Stage {1}", overlayType.DisplayName, inBounds ? overlay.Icon.ToString() : "-");
                }
                else
                {
                    sb.AppendFormat(", Overlay = {0}", overlayType.DisplayName);
                    if (IsIgnorableOverlay(overlay))
                    {
                        sb.Append(" (corner filler)");
                    }
                }
            }
            Terrain terrain = Technos[location] as Terrain;
            TerrainType terrainType = terrain?.Type;
            if (terrainType != null)
            {
                sb.AppendFormat(", Terrain = {0}", terrainType.DisplayName);
            }
            if (Technos[location] is InfantryGroup infantryGroup)
            {
                InfantryStoppingType i = InfantryGroup.ClosestStoppingTypes(subPixel).First();
                Infantry inf = infantryGroup.Infantry[(int)i];
                if (inf != null)
                {
                    sb.AppendFormat(", Infantry = {0} ({1})", inf.Type.DisplayName, InfantryGroup.GetStoppingTypeName(i));
                }
            }
            Unit unit = Technos[location] as Unit;
            UnitType unitType = unit?.Type;
            if (unitType != null)
            {
                sb.AppendFormat(", Unit = {0}", unitType.DisplayName);
            }
            Building building = Buildings[location] as Building;
            BuildingType buildingType = building?.Type;
            if (buildingType != null)
            {
                sb.AppendFormat(", Building = {0}", buildingType.DisplayName);
            }
            return sb.ToString();
        }

        public HouseType GetBaseHouse(GameInfo gameInfo)
        {
            if (HouseNone != null)
            {
                return HouseNone.Type;
            }
            string oppos = gameInfo.GetClassicOpposingPlayer(BasicSection.Player);
            return HouseTypes.Where(h => h.Equals(BasicSection.BasePlayer)).FirstOrDefault()
                ?? HouseTypes.Where(h => h.Equals(oppos)).FirstOrDefault()
                ?? HouseTypes.First();

        }

        private void RemoveBibs(Building building)
        {
            int[] bibCells = Smudge.IntersectsWithCells(building.BibCells).Where(x => x.Value.AttachedTo == building).Select(x => x.Cell).ToArray();
            foreach (int cell in bibCells)
            {
                Smudge[cell] = null;
            }
            building.BibCells.Clear();
        }

        private void AddBibs(Point location, Building building)
        {
            Dictionary<Point, Smudge> bibPoints = building.GetBib(location, SmudgeTypes);
            if (bibPoints == null)
            {
                return;
            }
            foreach (Point p in bibPoints.Keys)
            {
                if (Metrics.GetCell(p, out int subCell))
                {
                    Smudge[subCell] = bibPoints[p];
                    building.BibCells.Add(subCell);
                }
            }
        }

        /// <summary>
        /// Transforms the given teamtype orders list into a list of points forming a route on the map.
        /// The additional <paramref name="validWayPoints"/> outputs which points on the map correspond to valid waypoints.
        /// If a valid point exists in the output list, but it is null in <paramref name="validWayPoints"/>, this point is
        /// the first valid location found after an order to loop back to an earlier order.
        /// </summary>
        /// <param name="teamType">The teamtype for which to retrieve the route.</param>
        /// <param name="validWayPoints">A list matching the output list, in which a positive number indicates a waypoint number,
        /// -1 indicates a raw cell number, and null is not a real location but a loop order.</param>
        /// <returns>A list of map cells the route goes through. Null entries are either empty waypoints, or not orders that can be transformed into a location.</returns>
        public List<int?> GetTeamTypeRoute(string teamType, out List<int?> validWayPoints)
        {
            
            if (teamType == null)
            {
                validWayPoints = null;
                return null;
            }
            TeamType team = TeamTypes.FirstOrDefault(t => teamType.Equals(t.Name, StringComparison.OrdinalIgnoreCase));
            if (team == null)
            {
                validWayPoints = null;
                return null;
            }
            return GetTeamTypeRoute(team.Missions, out validWayPoints);
        }

        /// <summary>
        /// Transforms the given teamtype orders list into a list of points forming a route on the map.
        /// The additional <paramref name="validWayPoints"/> outputs which points on the map correspond to valid waypoints.
        /// If a valid point exists in the output list, but it is null in <paramref name="validWayPoints"/>, this point is
        /// the first valid location found after an order to loop back to an earlier order.
        /// </summary>
        /// <param name="orders">The list of teamtype orders.</param>
        /// <param name="validWayPoints">A list matching the output list, in which a positive number indicates a waypoint number,
        /// -1 indicates a raw cell number, and null is not a real location but a loop order.</param>
        /// <returns>A list of map cells the route goes through. Null entries are either empty waypoints, or not orders that can be transformed into a location.</returns>
        public List<int?> GetTeamTypeRoute(List<TeamTypeMission> orders, out List<int?> validWayPoints)
        {
            List<int?> points = new List<int?>();
            validWayPoints = new List<int?>();
            if (orders == null)
            {
                return points;
            }
            Waypoint[] wps = Waypoints;
            bool killLoop = false;
            for (int i = 0; i < orders.Count; ++i)
            {
                int? cell = null;
                int? validWp = null;
                TeamTypeMission ttm = orders[i];
                TeamMissionArgType tmArgTp = ttm.Mission.ArgType;
                int tmArg = ttm.Argument;
                switch (tmArgTp)
                {
                    case TeamMissionArgType.Waypoint:
                        validWp = tmArg;
                        if (tmArg >= 0 && tmArg < wps.Length) cell = wps[tmArg].Cell;
                        break;
                    case TeamMissionArgType.MapCell:
                        validWp = -1;
                        if (tmArg >= 0 && tmArg < Metrics.Length) cell = tmArg;
                        break;
                    case TeamMissionArgType.MissionNumber:
                        int endPoint = tmArg > i ? orders.Count : i;
                        // looping back invalidates any following instructions.
                        killLoop = tmArg <= i;
                        if (!killLoop)
                        {
                            // if jumping ahead, skip items in loop.
                            i = tmArg - 1;
                        }
                        bool foundLoopPoint = false;
                        for (int j = tmArg; j < endPoint; ++j)
                        {
                            TeamTypeMission ttm2 = orders[j];
                            TeamMissionArgType tmArgTp2 = ttm2.Mission.ArgType;
                            int tmArg2 = ttm2.Argument;
                            switch (tmArgTp2)
                            {
                                case TeamMissionArgType.Waypoint:
                                    foundLoopPoint = true;
                                    if (tmArg2 >= 0 && tmArg2 < wps.Length) cell = wps[tmArg2].Cell;
                                    break;
                                case TeamMissionArgType.MapCell:
                                    foundLoopPoint = true;
                                    if (tmArg2 >= 0 && tmArg2 < Metrics.Length) cell = tmArg2;
                                    break;
                                case TeamMissionArgType.MissionNumber:
                                    foundLoopPoint = true;
                                    break;
                            }
                            if (foundLoopPoint)
                            {
                                break;
                            }
                        }
                        break;
                }
                points.Add(cell);
                validWayPoints.Add(validWp);
                if (killLoop)
                {
                    break;
                }
            }
            return points;
        }

        public Map Clone(bool forPreview)
        {
            Waypoint[] wpPreview = new Waypoint[Waypoints.Length + (forPreview ? 1 : 0)];
            Array.Copy(Waypoints, wpPreview, Waypoints.Length);
            if (forPreview)
            {
                wpPreview[Waypoints.Length] = new Waypoint("", null);
            }
            // This is a shallow clone; the map is new, but the placed contents all still reference the original objects.
            // These shallow copies are used for map preview during editing, where dummy objects can be added without any issue.
            Map map = new Map(BasicSection, Theater, Metrics.Size, HouseType, HouseTypesIncludingSpecials,
                FlagColors, TheaterTypes, TemplateTypes, TerrainTypes, OverlayTypes, SmudgeTypes,
                EventTypes, CellEventTypes, UnitEventTypes, BuildingEventTypes, TerrainEventTypes,
                ActionTypes, CellActionTypes, UnitActionTypes, BuildingActionTypes, TerrainActionTypes,
                MissionTypes, inputMissionArmed, inputMissionUnarmed, inputMissionHarvest, inputMissionAircraft,
                UnitDirectionTypes, BuildingDirectionTypes, AllInfantryTypes, AllUnitTypes, BuildingTypes, TeamMissionTypes,
                AllTeamTechnoTypes, wpPreview, MovieTypes, MovieEmpty, ThemeTypes, ThemeEmpty,
                DropZoneRadius, GapRadius, RadarJamRadius, TiberiumOrGoldValue, GemValue)
            {
                UsedLandTypes = UsedLandTypes,
                TopLeft = TopLeft,
                Size = Size,
                // Allows functions to check whether they are being applied on the real map or the preview map.
                ForPreview = forPreview
            };
            map.BeginUpdate();
            MapSection.CopyTo(map.MapSection);
            BriefingSection.CopyTo(map.BriefingSection);
            // Ignore processing-only "VisibilityAsEnum".
            SteamSection.CopyTo(map.SteamSection, typeof(NonSerializedINIKeyAttribute));
            Array.Copy(Houses, map.Houses, map.Houses.Length);
            map.Triggers.AddRange(Triggers);
            Templates.CopyTo(map.Templates);
            Overlay.CopyTo(map.Overlay);
            Smudge.CopyTo(map.Smudge);
            CellTriggers.CopyTo(map.CellTriggers);
            foreach ((Point location, ICellOccupier occupier) in Technos)
            {
                if (occupier is InfantryGroup infantryGroup)
                {
                    // This creates an InfantryGroup not linked to its infantry, but it is necessary
                    // to ensure that the real InfantryGroups are not polluted with dummy objects.
                    InfantryGroup newInfantryGroup = new InfantryGroup();
                    Array.Copy(infantryGroup.Infantry, newInfantryGroup.Infantry, newInfantryGroup.Infantry.Length);
                    map.Technos.Add(location, newInfantryGroup);
                }
                else
                {
                    map.Technos.Add(location, occupier);
                }
            }
            foreach ((Point location, ICellOccupier building) in Buildings)
            {
                // Silly side effect: this fixes any building bibs.
                map.Buildings.Add(location, building);
            }
            map.TeamTypes.AddRange(TeamTypes);
            // Global update of all things that need updating like wall connections and resource density and such.
            map.EndUpdate();
            return map;
        }

        public IEnumerable<Trigger> FilterCellTriggers()
        {
            return FilterCellTriggers(Triggers);
        }

        public IEnumerable<Trigger> FilterCellTriggers(IEnumerable<Trigger> triggers)
        {
            foreach (Trigger trigger in FilterTriggersByEvent(CellEventTypes, triggers).Concat(FilterTriggersByAction(CellActionTypes, triggers).Distinct()))
            {
                yield return trigger;
            }
        }

        public IEnumerable<Trigger> FilterUnitTriggers()
        {
            return FilterUnitTriggers(Triggers);
        }

        public IEnumerable<Trigger> FilterUnitTriggers(IEnumerable<Trigger> triggers)
        {
            foreach (Trigger trigger in FilterTriggersByEvent(UnitEventTypes, triggers).Concat(FilterTriggersByAction(UnitActionTypes, triggers).Distinct()))
            {
                yield return trigger;
            }
        }

        public IEnumerable<Trigger> FilterStructureTriggers()
        {
            return FilterStructureTriggers(Triggers);
        }

        public IEnumerable<Trigger> FilterStructureTriggers(IEnumerable<Trigger> triggers)
        {
            foreach (Trigger trigger in FilterTriggersByEvent(BuildingEventTypes, triggers).Concat(FilterTriggersByAction(BuildingActionTypes, triggers).Distinct()))
            {
                yield return trigger;
            }
        }

        public IEnumerable<Trigger> FilterTerrainTriggers()
        {
            return FilterTerrainTriggers(Triggers);
        }

        public IEnumerable<Trigger> FilterTerrainTriggers(IEnumerable<Trigger> triggers)
        {
            foreach (Trigger trigger in FilterTriggersByEvent(TerrainEventTypes, triggers).Concat(FilterTriggersByAction(TerrainActionTypes, triggers).Distinct()))
            {
                yield return trigger;
            }
        }

        public static IEnumerable<Trigger> FilterTriggersByEvent(HashSet<string> allowedEventTypes, IEnumerable<Trigger> triggers)
        {
            foreach (Trigger trig in triggers)
            {
                if (!Globals.EnforceTriggerTypes || (trig.Event1 != null && allowedEventTypes.Contains(trig.Event1.EventType))
                    || ((trig.EventControl == TriggerMultiStyleType.Or || trig.EventControl == TriggerMultiStyleType.And)
                        && trig.Event2 != null && allowedEventTypes.Contains(trig.Event2.EventType)))
                {
                    yield return trig;
                }
            }
        }

        public static string MakeAllowedTriggersToolTip(string[] filteredEvents, string[] filteredActions)
        {
            return MakeAllowedTriggersToolTip(filteredEvents, null, filteredActions, null);
        }

        public static string MakeAllowedTriggersToolTip(string[] filteredEvents, string[] filteredActions, Trigger trigger)
        {
            List<string> indicatedEvents = new List<string>();
            List<string> indicatedActions = new List<string>();
            if (trigger != null)
            {
                indicatedEvents.Add(trigger.Event1.EventType);
                if (trigger.UsesEvent2 && !TriggerEvent.IsEmpty(trigger.Event2.EventType))
                {
                    indicatedEvents.Add(trigger.Event2.EventType);
                }
                indicatedActions.Add(trigger.Action1.ActionType);
                if (trigger.EventControl == TriggerMultiStyleType.Linked || !TriggerEvent.IsEmpty(trigger.Action2.ActionType))
                {
                    indicatedActions.Add(trigger.Action2.ActionType);
                }
            }
            return MakeAllowedTriggersToolTip(filteredEvents, indicatedEvents.ToArray(), filteredActions, indicatedActions.ToArray());
        }

        public static string MakeAllowedTriggersToolTip(string[] filteredEvents, string[] indicatedEvents, string[] filteredActions, string[] indicatedActions)
        {
            if (indicatedEvents == null)
            {
                indicatedEvents = new string[0];
            }
            if (indicatedActions == null)
            {
                indicatedActions = new string[0];
            }
            if (!Globals.EnforceTriggerTypes)
            {
                return "Trigger restricting has been disabled.";
            }
            StringBuilder tooltip = new StringBuilder();
            bool hasEvents = filteredEvents != null && filteredEvents.Length > 0;
            bool hasActions = filteredActions != null && filteredActions.Length > 0;
            if (hasEvents)
            {
                tooltip.Append("Usable trigger events:");
                foreach (string evt in filteredEvents)
                {
                    string showEvt = evt.TrimEnd('.');
                    if (indicatedEvents.Contains(evt))
                    {
                        tooltip.Append("\n> [").Append(showEvt.ToUpperInvariant()).Append("]");
                    }
                    else
                    {
                        tooltip.Append("\n\u2022 ").Append(showEvt);
                    }
                }
            }
            if (hasActions)
            {
                if (hasEvents)
                {
                    tooltip.Append("\n");
                }
                tooltip.Append("Usable trigger actions:");
                foreach (string act in filteredActions)
                {
                    string showAct = act.TrimEnd('.');
                    if (indicatedActions.Contains(act))
                    {
                        tooltip.Append("\n> [").Append(showAct.ToUpperInvariant()).Append("]");
                    }
                    else
                    {
                        tooltip.Append("\n\u2022 ").Append(showAct);
                    }
                }
            }
            return hasEvents || hasActions ? tooltip.ToString() : null;
        }

        public IEnumerable<Trigger> FilterTriggersByAction(HashSet<string> allowedActionTypes, IEnumerable<Trigger> triggers)
        {
            foreach (Trigger trig in triggers)
            {
                if (!Globals.EnforceTriggerTypes || (trig.Action1 != null && allowedActionTypes.Contains(trig.Action1.ActionType))
                    || (trig.Action2 != null && allowedActionTypes.Contains(trig.Action2.ActionType)))
                {
                    yield return trig;
                }
            }
        }

        public IEnumerable<ITechno> GetAllTechnos()
        {
            foreach ((Point location, ICellOccupier occupier) in Technos)
            {
                if (occupier is InfantryGroup infantryGroup)
                {
                    foreach (Infantry inf in infantryGroup.Infantry)
                    {
                        if (inf != null)
                        {
                            yield return inf;
                        }
                    }
                }
                else if (occupier is ITechno techno)
                {
                    yield return techno;
                }
            }
        }

        public string GetDefaultMission(ITechnoType techno)
        {
            return GetDefaultMission(techno, DefaultMissionArmed);
        }

        public string GetDefaultMission(ITechnoType techno, string currentMission)
        {
            if (techno.IsHarvester)
            {
                return DefaultMissionHarvest;
            }
            if (techno.IsAircraft && !techno.IsFixedWing)
            {
                // Ground-landable aircraft. Default order should be 'Unload' to make it land on the spot it spawned on.
                return DefaultMissionAircraft;
            }
            if (!techno.IsArmed)
            {
                return DefaultMissionUnarmed;
            }
            // Automatically switch from other default missions to the general 'Guard' one, but don't change custom-picked mission like 'Hunt4.
            if (currentMission == DefaultMissionHarvest || currentMission == DefaultMissionAircraft || currentMission == DefaultMissionUnarmed)
            {
                return DefaultMissionArmed;
            }
            return currentMission;
        }

        public ICellOccupier FindBlockingObject(int cell, ICellOccupier obj, out int blockingCell, out int placementCell)
        {
            return FindBlockingObject(cell, obj, out blockingCell, out placementCell, out _);
        }

        public ICellOccupier FindBlockingObject(int cell, ICellOccupier obj, out int blockingCell, out int placementCell, out bool onBib)
        {
            onBib = false;
            blockingCell = -1;
            placementCell = -1;
            HashSet<Point> bibIgnoreCells = new HashSet<Point>();
            if (Metrics.GetLocation(cell, out Point p))
            {
                bool[,] occupyMask = obj.OccupyMask;
                int ylenOcMask = occupyMask.GetLength(0);
                int xlenOcMask = occupyMask.GetLength(1);
                // First check building bounds without bib.
                if (obj is BuildingType bld)
                {
                    bool[,] mask = bld.BaseOccupyMask;
                    int ylenMask = mask.GetLength(0);
                    int xlenMask = mask.GetLength(1);
                    int ylen = Math.Max(ylenMask, ylenOcMask);
                    int xlen = Math.Max(xlenMask, xlenOcMask);
                    for (int y = 0; y < ylen; ++y)
                    {
                        for (int x = 0; x < xlen; ++x)
                        {
                            if (y < ylenMask && x < xlenMask && mask[y, x])
                            {
                                if (!Metrics.GetCell(new Point(p.X + x, p.Y + y), out int targetCell))
                                {
                                    blockingCell = -1;
                                    placementCell = -1;
                                    return null;
                                }
                                ICellOccupier techno = Technos[targetCell];
                                ICellOccupier b = Buildings[targetCell];
                                if (techno != null || b != null)
                                {
                                    blockingCell = targetCell;
                                    Point? blockingOrigin = techno != null ? Technos[techno] : Buildings[b];
                                    placementCell = Metrics.GetCell(blockingOrigin.Value).Value;
                                    onBib = false;
                                    return techno ?? b;
                                }
                            }
                            else if (y < ylenOcMask && x < xlenOcMask && occupyMask[y, x])
                            {
                                // On bib, not on main space.
                                bibIgnoreCells.Add(new Point(x, y));
                            }
                        }
                    }
                    // If this is a building, and bibs don't block, that's all we need to check for this.
                    if (!Globals.BlockingBibs)
                    {
                        return null;
                    }
                }
                // Check all other types, and building bibs.
                for (int y = 0; y < ylenOcMask; ++y)
                {
                    for (int x = 0; x < xlenOcMask; ++x)
                    {
                        if (occupyMask[y, x])
                        {
                            if (!Metrics.GetCell(new Point(p.X + x, p.Y + y), out int targetCell))
                            {
                                blockingCell = -1;
                                placementCell = -1;
                                return null;
                            }
                            ICellOccupier techno = Technos[targetCell];
                            ICellOccupier b = Buildings[targetCell];
                            if (techno != null || b != null)
                            {
                                blockingCell = targetCell;
                                Point? blockingOrigin = null;
                                if (b != null)
                                {
                                    blockingOrigin = Buildings[b];
                                    onBib = true;
                                }
                                else if (!bibIgnoreCells.Contains(new Point(x, y)))
                                {
                                    // For checking non-building technos on a building's area, ignore the unoccupied bib cells.
                                    blockingOrigin = Technos[techno];
                                }
                                if (blockingOrigin.HasValue)
                                {
                                    placementCell = Metrics.GetCell(blockingOrigin.Value).Value;
                                    return techno ?? b;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public void CheckBuildingBlockingCell(int cell, BuildingType buildingType, List<string> errors, ref bool modified)
        {
            CheckBuildingBlockingCell(cell, buildingType, errors, ref modified, null);
        }

        public void CheckBuildingBlockingCell(int cell, BuildingType buildingType, List<string> errors, ref bool modified, string rebuildIndex)
        {
            ICellOccupier techno = FindBlockingObject(cell, buildingType, out int blockingCell, out int placementcell, out bool isbib);
            string reportString;
            if (rebuildIndex != null)
            {
                string bibRemark = isbib ? "Bib area of b" : "B";
                reportString = String.Format("{0}ase rebuild entry '{1}', structure '{2}' on cell '{3}'", bibRemark, rebuildIndex, buildingType.Name, cell);
            }
            else
            {
                string bibRemark = isbib ? "Bib area of s" : "S";
                reportString = String.Format("{0}tructure '{1}' placed on cell {2}", bibRemark, buildingType.Name, cell);
            }
            string reportCell = blockingCell == -1 ? "<unknown>" : blockingCell.ToString();
            if (techno is Building building)
            {
                bool onBib = false;
                if (building.Type.HasBib)
                {
                    onBib = true;
                    Point newPoint = new Point(cell % Metrics.Width, cell / Metrics.Width);
                    // All cells required for this building
                    HashSet<Point> errBuildingPoints = OccupierSet.GetOccupyPoints(newPoint, buildingType).ToHashSet();
                    foreach (Point p in OccupierSet.GetOccupyPoints(Buildings[building].Value, building.Type.BaseOccupyMask))
                    {
                        if (errBuildingPoints.Contains(p))
                        {
                            // In main area, so not on bib.
                            onBib = false;
                            break;
                        }
                    }
                }
                if (onBib)
                {
                    errors.Add(String.Format("{0} overlaps bib area of structure '{1}' placed on cell {2} at cell {3}; skipping.", reportString, building.Type.Name, placementcell, reportCell));
                    modified = true;
                }
                else
                {
                    errors.Add(String.Format("{0} overlaps structure '{1}' placed on cell {2} at cell {3}; skipping.", reportString, building.Type.Name, placementcell, reportCell));
                    modified = true;
                }
            }
            else if (techno is Overlay overlay)
            {
                errors.Add(String.Format("{0} overlaps overlay '{1}' on cell {2}; skipping.", reportString, overlay.Type.Name, reportCell));
                modified = true;
            }
            else if (techno is Terrain terrain)
            {
                errors.Add(String.Format("{0} overlaps terrain '{1}' placed on cell {2} at cell {3}; skipping.", reportString, terrain.Type.Name, placementcell, reportCell));
                modified = true;
            }
            else if (techno is InfantryGroup infantry)
            {
                Infantry inf = infantry.Infantry.FirstOrDefault(u => u != null);
                string infInfo = inf == null ? String.Empty : String.Format(" '{0}'", inf.Type.Name);
                errors.Add(String.Format("{0} overlaps infantry '{1}' on cell {2}; skipping.", reportString, infInfo, reportCell));
                modified = true;
            }
            else if (techno is Unit unit)
            {
                errors.Add(String.Format("{0} overlaps unit '{1}' on cell {2}; skipping.", reportString, unit.Type.Name, reportCell));
                modified = true;
            }
            else
            {
                if (blockingCell != -1)
                {
                    errors.Add(String.Format("{0} overlaps unknown techno on cell {1}; skipping.", reportString, blockingCell));
                    modified = true;
                }
                else
                {
                    errors.Add(String.Format("{0} crosses outside the map bounds; skipping.", reportString));
                    modified = true;
                }
            }
        }

        public TGA GenerateMapPreview(IGamePlugin plugin)
        {
            return GeneratePreview(Globals.MapPreviewSize, plugin, Bounds, true, false);
        }

        public TGA GenerateWorkshopPreview(IGamePlugin plugin, Rectangle boundsToUse)
        {
            return GeneratePreview(Globals.WorkshopPreviewSize, plugin, boundsToUse, true, false);
        }

        public TGA GeneratePreview(Size previewSize, IGamePlugin plugin, Rectangle boundsToUse, bool highlightFlags, bool sharpen)
        {
            MapLayerFlag toRender = MapLayerFlag.Template | MapLayerFlag.OverlayAll | MapLayerFlag.Smudge | MapLayerFlag.Technos;
            bool smooth = boundsToUse.Width * Globals.OriginalTileWidth > previewSize.Width;
            return GeneratePreview(previewSize, plugin, toRender, boundsToUse, true, highlightFlags, smooth, sharpen);
        }

        public TGA GeneratePreview(Size previewSize, IGamePlugin plugin, MapLayerFlag toRender, Rectangle boundsToUse, bool clearBackground, bool highlightFlags, bool smooth, bool sharpen)
        {
            HashSet<Point> locations = Metrics.Bounds.Points().ToHashSet();
            Size originalTileSize = Globals.OriginalTileSize;
            float tileScale = Math.Min((float)previewSize.Width / boundsToUse.Width / originalTileSize.Width, (float)previewSize.Height / boundsToUse.Height / originalTileSize.Height);
            float scaledWidth = originalTileSize.Width * tileScale;
            float scaledHeight = originalTileSize.Height * tileScale;
            // Adjust to nearest higher int
            int scaleWidth = scaledWidth == (int)scaledWidth ? (int)scaledWidth : (int)(scaledWidth + 1);
            int scaleHeight = scaledHeight == (int)scaledHeight ? (int)scaledHeight : (int)(scaledHeight + 1);
            Size renderTileSize = new Size(scaleWidth, scaleHeight);
            // Recalculate scale
            tileScale = scaleWidth / (float)originalTileSize.Width;
            //float tileScale = 1;
            //Size renderTileSize = originalTileSize;
            Rectangle mapBounds = new Rectangle(boundsToUse.Left * renderTileSize.Width, boundsToUse.Top * renderTileSize.Height,
                    boundsToUse.Width * renderTileSize.Width, boundsToUse.Height * renderTileSize.Height);
            float previewScale = Math.Min(previewSize.Width / (float)mapBounds.Width, previewSize.Height / (float)mapBounds.Height);
            Size scaledSize = new Size((int)Math.Round(previewSize.Width / previewScale), (int)Math.Round(previewSize.Height / previewScale));

            using (ShapeCacheManager shapeCache = new ShapeCacheManager())
            using (Bitmap fullBitmap = new Bitmap(Metrics.Width * renderTileSize.Width, Metrics.Height * renderTileSize.Height))
            using (Bitmap croppedBitmap = new Bitmap(previewSize.Width, previewSize.Height))
            {
                using (Graphics g = Graphics.FromImage(fullBitmap))
                {
                    MapRenderer.SetRenderSettings(g, !Globals.UseClassicFiles);
                    MapRenderer.Render(plugin.GameInfo, this, g, locations, toRender, tileScale, highlightFlags, shapeCache);
                    if (toRender.HasAnyFlags(MapLayerFlag.Indicators))
                    {
                        ViewTool.RenderIndicators(g, plugin, this, tileScale, toRender, MapLayerFlag.None, false, plugin.Map.Metrics.Bounds);
                    }
                }
                using (Graphics g = Graphics.FromImage(croppedBitmap))
                {
                    // Smoothen if sized down.
                    MapRenderer.SetRenderSettings(g, smooth);
                    Matrix transform = new Matrix();
                    transform.Scale(previewScale, previewScale);
                    transform.Translate((scaledSize.Width - mapBounds.Width) / 2, (scaledSize.Height - mapBounds.Height) / 2);
                    g.Transform = transform;
                    if (clearBackground)
                    {
                        g.Clear(Color.Black);
                    }
                    g.DrawImage(fullBitmap, new Rectangle(0, 0, mapBounds.Width, mapBounds.Height), mapBounds, GraphicsUnit.Pixel);
                }
                if (sharpen)
                {
                    using (Bitmap sharpenedImage = croppedBitmap.Sharpen(1.0f))
                    {
                        return TGA.FromBitmap(sharpenedImage);
                    }
                }
                else
                {
                    return TGA.FromBitmap(croppedBitmap);
                }
            }
        }

        object ICloneable.Clone()
        {
            return Clone(false);
        }

        private void Overlay_CellChanged(object sender, CellChangedEventArgs<Overlay> e)
        {
            if (e.OldValue != null && (e.OldValue.Type.IsWall || e.OldValue.Type.IsSolid))
            {
                Buildings.Remove(e.OldValue);
            }
            if (e.Value != null && (e.Value.Type.IsWall || e.Value.Type.IsSolid))
            {
                Buildings.Add(e.Location, e.Value);
            }
            if (updating)
            {
                return;
            }
            foreach (Overlay overlay in new Overlay[] { e.OldValue, e.Value })
            {
                if (overlay == null)
                {
                    continue;
                }
                List<MapLayerFlag> layers = new List<MapLayerFlag>();
                if (overlay.Type.IsResource)
                {
                    layers.Add(MapLayerFlag.Resources);
                    layers.Add(MapLayerFlag.Overlay);
                }
                else if (overlay.Type.IsWall)
                {
                    layers.Add(MapLayerFlag.Walls);
                    layers.Add(MapLayerFlag.Overlay);
                }
                else if (overlay.Type.IsConcrete)
                {
                    layers.Add(MapLayerFlag.Overlay);
                }
                foreach (MapLayerFlag layer in layers)
                {
                    if (!invalidateLayers.TryGetValue(layer, out ISet<Point> locations))
                    {
                        locations = new HashSet<Point>();
                        invalidateLayers[layer] = locations;
                    }
                    locations.UnionWith(Rectangle.Inflate(new Rectangle(e.Location, new Size(1, 1)), 1, 1).Points());
                }
            }
            if (updateCount == 0)
            {
                Update();
            }
        }

        private void Technos_OccupierAdded(object sender, OccupierAddedEventArgs<ICellOccupier> e)
        {
            if (e.Occupier is ICellOverlapper overlapper)
            {
                if (updateCount == 0)
                {
                    Overlappers.Add(e.Location, overlapper);
                }
                else
                {
                    invalidateOverlappers = true;
                }
            }
        }

        private void Technos_OccupierRemoved(object sender, OccupierRemovedEventArgs<ICellOccupier> e)
        {
            if (e.Occupier is ICellOverlapper overlapper)
            {
                if (updateCount == 0)
                {
                    Overlappers.Remove(overlapper);
                }
                else
                {
                    invalidateOverlappers = true;
                }
            }
        }

        private void Buildings_OccupierAdded(object sender, OccupierAddedEventArgs<ICellOccupier> e)
        {
            if (e.Occupier is ICellOverlapper overlapper)
            {
                //Debug.WriteLine("update count is " + this.updateCount + "; " + (this.updateCount > 0 ? "not " : String.Empty) + "adding building " + overlapper.ToString() + " to " + (this.ForPreview ? "preview " : String.Empty) + "map.");
                if (updateCount == 0)
                {
                    Overlappers.Add(e.Location, overlapper);
                }
                else
                {
                    invalidateOverlappers = true;
                }
            }
            if (e.Occupier is Building building)
            {
                AddBibs(e.Location, building);
                if (building.Type.IsWall)
                {
                    Rectangle toRefresh = new Rectangle(e.Location, building.Type.OverlapBounds.Size);
                    toRefresh.Inflate(1, 1);
                    UpdateWallOverlays(toRefresh.Points().ToHashSet());
                }
            }
        }

        private void Buildings_OccupierRemoved(object sender, OccupierRemovedEventArgs<ICellOccupier> e)
        {
            if (e.Occupier is Building building)
            {
                RemoveBibs(building);
                if (building.Type.IsWall)
                {
                    Rectangle toRefresh = new Rectangle(e.Location, building.Type.OverlapBounds.Size);
                    toRefresh.Inflate(1, 1);
                    UpdateWallOverlays(toRefresh.Points().ToHashSet());
                }
            }
            if (e.Occupier is ICellOverlapper overlapper)
            {
                if (updateCount == 0)
                {
                    Overlappers.Remove(overlapper);
                }
                else
                {
                    invalidateOverlappers = true;
                }
            }
        }

        public void UpdateWaypoints()
        {
            bool isSolo = BasicSection.SoloMission;
            HashSet<Point> updated = new HashSet<Point>();
            for (int i = 0; i < Waypoints.Length; ++i)
            {
                Waypoint waypoint = Waypoints[i];
                if (waypoint.Flags.HasFlag(WaypointFlag.PlayerStart))
                {
                    string newName = isSolo ? i.ToString() : String.Format("P{0}", i);
                    Waypoints[i].Name = newName;
                    Waypoints[i].ShortName = newName;
                    if (waypoint.Point.HasValue)
                    {
                        updated.Add(waypoint.Point.Value);
                    }
                }
            }
            NotifyWaypointsUpdate();
            NotifyMapContentsChanged(updated);
        }

        public bool RemoveExpansionUnits()
        {
            HashSet<Point> refreshPoints = new HashSet<Point>();
            bool changed = false;
            if (BasicSection.ExpansionEnabled)
            {
                // Expansion is enabled. Nothing to do.
                return false;
            }
            // Technos on map
            List<(Point, ICellOccupier)> toDelete = new List<(Point, ICellOccupier)>();
            foreach ((Point p, ICellOccupier occup) in Technos)
            {
                if (occup is Unit un)
                {
                    if (un.Type.IsExpansionOnly)
                    {
                        toDelete.Add((p, occup));
                    }
                }
                else if (occup is InfantryGroup ifg)
                {
                    if (ifg.Infantry.Any(inf => inf != null && inf.Type.IsExpansionOnly))
                    {
                        toDelete.Add((p, occup));
                    }
                }
            }
            foreach ((Point point, ICellOccupier occup) in toDelete)
            {
                if (occup is Unit un)
                {
                    Rectangle? refreshArea = Overlappers[un];
                    if (refreshArea.HasValue)
                    {
                        refreshPoints.UnionWith(refreshArea.Value.Points());
                    }
                    //mapPanel.Invalidate(map, un);
                    Technos.Remove(occup);
                    changed = true;
                }
                else if (occup is InfantryGroup infantryGroup)
                {
                    Infantry[] inf = infantryGroup.Infantry;
                    for (int i = 0; i < inf.Length; ++i)
                    {
                        if (inf[i] != null && inf[i].Type.IsExpansionOnly)
                        {
                            inf[i] = null;
                            changed = true;
                        }
                    }
                    bool delGroup = inf.All(i => i == null);
                    Rectangle? refreshArea = Overlappers[infantryGroup];
                    if (refreshArea.HasValue)
                    {
                        refreshPoints.UnionWith(refreshArea.Value.Points());
                    }
                    //mapPanel.Invalidate(map, infantryGroup);
                    if (delGroup)
                    {
                        Technos.Remove(infantryGroup);
                    }
                }
            }
            // Teamtypes
            foreach (TeamType teamtype in TeamTypes)
            {
                List<TeamTypeClass> toRemove = new List<TeamTypeClass>();
                foreach (TeamTypeClass ttclass in teamtype.Classes)
                {
                    if (ttclass.Type.IsExpansionOnly)
                    {
                        toRemove.Add(ttclass);
                    }
                }
                foreach (TeamTypeClass ttclass in toRemove)
                {
                    teamtype.Classes.Remove(ttclass);
                    changed = true;
                }
            }
            NotifyMapContentsChanged(refreshPoints);
            return changed;
        }

        /// <summary>
        /// Applies trigger renames, and does the checks to see whether all attached objects are still valid for the trigger type they are connected to.
        /// The undo and redo lists can be used to track these actions, with the celtrigger location keeping track of cell triggers that might get removed by this.
        /// </summary>
        /// <param name="renameActions">All rename actions</param>
        /// <param name="undoList">Undo list, linking objects to their original trigger value</param>
        /// <param name="redoList">Redo list, linking objects to their final trigger value</param>
        /// <param name="cellTriggerLocations">Locations for all modified cell triggers</param>
        /// <param name="newTriggers">Triggers list to use to check for trigger links to objects.</param>
        public void ApplyTriggerNameChanges(List<(string Name1, string Name2)> renameActions, out Dictionary<object, string> undoList, out Dictionary<object, string> redoList, out Dictionary<CellTrigger, int> cellTriggerLocations, List<Trigger> newTriggers)
        {
            undoList = new Dictionary<object, string>();
            redoList = new Dictionary<object, string>();
            cellTriggerLocations = new Dictionary<CellTrigger, int>();
            foreach ((string name1, string name2) in renameActions)
            {
                if (Trigger.IsEmpty(name1))
                {
                    continue;
                }
                foreach ((Point location, Building building) in Buildings.OfType<Building>().Where(x => x.Occupier.IsPrebuilt))
                {
                    if (String.Equals(building.Trigger, name1, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!undoList.ContainsKey(building))
                            undoList[building] = building.Trigger;
                        redoList[building] = name2;
                        building.Trigger = name2;
                    }
                }
                foreach (ITechno techno in GetAllTechnos())
                {
                    if (String.Equals(techno.Trigger, name1, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!undoList.ContainsKey(techno))
                        {
                            undoList[techno] = techno.Trigger;
                        }
                        redoList[techno] = name2;
                        techno.Trigger = name2;
                    }
                }
                foreach (TeamType team in TeamTypes)
                {
                    if (String.Equals(team.Trigger, name1, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!undoList.ContainsKey(team))
                        {
                            undoList[team] = team.Trigger;
                        }
                        redoList[team] = name2;
                        team.Trigger = name2;
                    }
                }
                foreach ((int cell, CellTrigger value) in CellTriggers)
                {
                    if (String.Equals(value.Trigger, name1, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!undoList.ContainsKey(value))
                        {
                            undoList[value] = value.Trigger;
                        }
                        redoList[value] = name2;
                        // if name2 is "None", the post-trigger-assignment sweep will clean this up.
                        value.Trigger = name2;
                        if (!cellTriggerLocations.ContainsKey(value))
                        {
                            cellTriggerLocations[value] = cell;
                        }
                    }
                }
            }
            CleanUpTriggers(newTriggers, undoList, redoList, cellTriggerLocations);
        }

        private void CleanUpTriggers(List<Trigger> triggers, Dictionary<object, string> undoList, Dictionary<object, string> redoList, Dictionary<CellTrigger, int> cellTriggerLocations)
        {
            // Clean techno types
            HashSet<string> availableTriggers = triggers.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            HashSet<string> availableUnitTriggers = FilterUnitTriggers(triggers).Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            HashSet<string> availableBuildingTriggers = FilterStructureTriggers(triggers).Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            HashSet<string> availableTerrainTriggers = FilterTerrainTriggers(triggers).Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (ITechno techno in GetAllTechnos())
            {
                if (techno is Infantry infantry)
                {
                    CheckTechnoTrigger(infantry, availableUnitTriggers, undoList, redoList);
                }
                else if (techno is Unit unit)
                {
                    CheckTechnoTrigger(unit, availableUnitTriggers, undoList, redoList);
                }
                else if (techno is Building building)
                {
                    CheckTechnoTrigger(building, availableBuildingTriggers, undoList, redoList);
                }
                else if (techno is Terrain terrain)
                {
                    CheckTechnoTrigger(terrain, availableTerrainTriggers, undoList, redoList);
                }
            }
            // Clean teamtypes
            foreach (TeamType team in TeamTypes)
            {
                string trig = team.Trigger;
                if (!Trigger.IsEmpty(trig) && !availableUnitTriggers.Contains(trig))
                {
                    if (undoList != null && !undoList.ContainsKey(team))
                    {
                        undoList.Add(team, trig);
                    }
                    if (redoList != null)
                    {
                        redoList[team] = Trigger.None;
                    }
                    team.Trigger = Trigger.None;
                }
            }
            // Clean triggers. Not covered in undo/redo actions since it is applied on the new list directly.
            foreach (Trigger trig in triggers)
            {
                if (trig == null)
                {
                    continue;
                }
                if (trig.Action1.Trigger != Trigger.None && !availableTriggers.Contains(trig.Action1.Trigger))
                {
                    trig.Action1.Trigger = Trigger.None;
                }
                if (trig.Action2.Trigger != Trigger.None && !availableTriggers.Contains(trig.Action2.Trigger))
                {
                    trig.Action2.Trigger = Trigger.None;
                }
            }
            CleanUpCellTriggers(triggers, undoList, redoList, cellTriggerLocations);
        }

        private void CheckTechnoTrigger(ITechno techno, HashSet<string> availableTriggers, Dictionary<object, string> undoList, Dictionary<object, string> redoList)
        {
            string trig = techno.Trigger;
            if (!Trigger.IsEmpty(trig) && !availableTriggers.Contains(trig))
            {
                if (undoList != null && !undoList.ContainsKey(techno))
                {
                    undoList.Add(techno, trig);
                }
                if (redoList != null)
                {
                    redoList[techno] = Trigger.None;
                }
                techno.Trigger = Trigger.None;
            }
        }

        private void CleanUpCellTriggers(List<Trigger> triggers, Dictionary<object, string> undoList, Dictionary<object, string> redoList, Dictionary<CellTrigger, int> cellTriggerLocations)
        {
            HashSet<string> placeableTrigs = FilterCellTriggers(triggers).Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            List<int> cellsToClear = new List<int>();
            foreach ((int cell, CellTrigger value) in CellTriggers)
            {
                if (Trigger.IsEmpty(value.Trigger) || !placeableTrigs.Contains(value.Trigger))
                {
                    if (undoList != null && !undoList.ContainsKey(value))
                    {
                        undoList.Add(value, value.Trigger);
                    }
                    if (redoList != null)
                    {
                        redoList[value] = Trigger.None;
                    }
                    cellTriggerLocations[value] = cell;
                    cellsToClear.Add(cell);
                }
            }
            for (int i = 0; i < cellsToClear.Count; ++i)
            {
                CellTriggers[cellsToClear[i]] = null;
            }
        }

        public void ApplyTeamTypeRenames(List<(string Name1, string Name2)> renameActions)
        {
            foreach ((string name1, string name2) in renameActions)
            {
                if (TeamType.IsEmpty(name1))
                {
                    continue;
                }
                foreach (Trigger trigger in triggers)
                {
                    if (String.Equals(trigger.Event1.Team, name1, StringComparison.OrdinalIgnoreCase))
                    {
                        trigger.Event1.Team = name2;
                    }
                    if (String.Equals(trigger.Event2.Team, name1, StringComparison.OrdinalIgnoreCase))
                    {
                        trigger.Event2.Team = name2;
                    }
                    if (String.Equals(trigger.Action1.Team, name1, StringComparison.OrdinalIgnoreCase))
                    {
                        trigger.Action1.Team = name2;
                    }
                    if (String.Equals(trigger.Action2.Team, name1, StringComparison.OrdinalIgnoreCase))
                    {
                        trigger.Action2.Team = name2;
                    }
                }
            }
        }

        public IEnumerable<string> AssessPower(HashSet<string> housesWithProd)
        {
            Dictionary<string, int[]> powerWithUnbuilt = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, int[]> powerWithoutUnbuilt = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
            foreach (HouseType house in HouseTypes)
            {
                if (housesWithProd.Contains(house.Name))
                {
                    powerWithUnbuilt[house.Name] = new int[3];
                }
                powerWithoutUnbuilt[house.Name] = new int[3];
            }
            HashSet<string> hasDamagedPowerPlants = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> hasUnbuiltStructures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach ((_, Building bld) in Buildings.OfType<Building>())
            {
                string bldHouse = bld.House.Name;
                int bldUsage = bld.Type.PowerUsage;
                int bldProdHealthy = bld.Type.PowerProduction;
                int bldProdCur = bld.Type.PowerProduction;
                if (bld.IsPrebuilt)
                {
                    if (bld.Strength < 256 && bldProdCur > 0)
                    {
                        hasDamagedPowerPlants.Add(bld.House.Name);
                    }
                    bldProdCur = bldProdCur * bld.Strength / 256;
                }
                int[] housePwr;
                // These should all belong to the "rebuild house" due to internal property change listeners; no need to explicitly check.
                if (!bld.IsPrebuilt)
                {
                    foreach (string house in housesWithProd)
                    {
                        if ((HouseNone != null || bld.House.Name == house) && powerWithUnbuilt.TryGetValue(house, out housePwr))
                        {
                            if (!hasUnbuiltStructures.Contains(house))
                            {
                                hasUnbuiltStructures.Add(house);
                            }
                            housePwr[0] += bldUsage;
                            housePwr[1] += bldProdHealthy;
                            housePwr[2] += bldProdHealthy;
                        }
                    }
                }
                else if (powerWithUnbuilt.TryGetValue(bld.House.Name, out housePwr))
                {
                    housePwr[0] += bldUsage;
                    housePwr[1] += bldProdCur;
                    housePwr[2] += bldProdHealthy;
                }
                if (bld.IsPrebuilt && powerWithoutUnbuilt.TryGetValue(bld.House.Name, out housePwr))
                {
                    housePwr[0] += bldUsage;
                    housePwr[1] += bldProdCur;
                    housePwr[2] += bldProdHealthy;
                }
            }
            List<string> info = new List<string>();
            List<string> prodHouses = new List<string>();
            foreach (HouseType house in HouseTypes)
            {
                if (housesWithProd.Contains(house.Name))
                {
                    prodHouses.Add(house.Name);
                }
            }
            info.Add("Production-capable Houses: " + (prodHouses.Count == 0 ? "None" : String.Join(", ", prodHouses.ToArray())));
            foreach (HouseType house in HouseTypes)
            {
                int[] housePwrAll;
                int[] housePwrBuilt;
                bool hasDamaged = hasDamagedPowerPlants.Contains(house.Name);
                if (powerWithoutUnbuilt.TryGetValue(house.Name, out housePwrBuilt))
                {
                    StringBuilder houseInfo = new StringBuilder();
                    int houseUsageBuilt = housePwrBuilt[0]; // PowerUsage;
                    int houseProdBuilt = housePwrBuilt[1]; // PowerProduction at actual strength;
                    int houseProdBuiltHealthy = housePwrBuilt[2]; // PowerProduction when healthy;
                    houseInfo.Append(house.Name).Append(": ");
                    bool canRebuild = powerWithUnbuilt.TryGetValue(house.Name, out housePwrAll);
                    bool hasUnbuilt = hasUnbuiltStructures.Contains(house.Name);
                    bool listUnbuilt = canRebuild && hasUnbuilt;
                    if (listUnbuilt)
                    {
                        houseInfo.Append("With unbuilt: ");
                        int houseUsage = housePwrAll[0]; // PowerUsage;
                        int houseProd = housePwrAll[1]; // PowerProduction;
                        int houseProdHealthy = housePwrAll[2]; // PowerProduction when healthy;
                        houseInfo.Append(houseProd < houseUsage ? "[NOT OK]" : "OK").Append(" - ");
                        if (hasDamaged) houseInfo.Append("Has damaged power plants. ");
                        houseInfo.Append("Produces ").Append(houseProd);
                        if (hasDamaged)
                            houseInfo.Append(" currently; ").Append(houseProdHealthy).Append(" at full strength");
                        houseInfo.Append(", uses ").Append(houseUsage).Append(".");
                        houseInfo.Append(" Without unbuilt: ");
                    }
                    houseInfo.Append(houseProdBuilt < houseUsageBuilt ? "[NOT OK]" : "OK").Append(" - ");
                    if (hasDamaged && !listUnbuilt)
                    {
                        houseInfo.Append("Has damaged power plants. ");
                    }
                    houseInfo.Append("Produces ").Append(houseProdBuilt);
                    if (hasDamaged)
                    {
                        houseInfo.Append(" currently; ").Append(houseProdBuiltHealthy).Append(" at full strength");
                    }
                    houseInfo.Append(", uses ").Append(houseUsageBuilt).Append(".");
                    if (canRebuild && !hasUnbuilt)
                    {
                        houseInfo.Append(" Has no unbuilt.");
                    }
                    info.Add(houseInfo.ToString());
                }
            }
            return info;
        }

        public IEnumerable<string> AssessStorage(HashSet<string> housesWithProd)
        {
            Dictionary<string, int> storageWithUnbuilt = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, int> storageWithoutUnbuilt = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (HouseType house in HouseTypes)
            {
                if (housesWithProd.Contains(house.Name))
                {
                    storageWithUnbuilt[house.Name] = 0;
                }
                storageWithoutUnbuilt[house.Name] = 0;
            }
            foreach ((_, Building bld) in Buildings.OfType<Building>())
            {
                int bldStorage = bld.Type.Storage;
                if (!bld.IsPrebuilt)
                {
                    foreach (string house in housesWithProd)
                    {
                        if (storageWithUnbuilt.ContainsKey(house))
                        {
                            storageWithUnbuilt[house] += bldStorage;
                        }
                    }
                }
                else if (storageWithUnbuilt.ContainsKey(bld.House.Name))
                {
                    storageWithUnbuilt[bld.House.Name] += bldStorage;
                }
                if (bld.IsPrebuilt && storageWithoutUnbuilt.ContainsKey(bld.House.Name))
                {
                    storageWithoutUnbuilt[bld.House.Name] += bldStorage;
                }
            }
            List<string> info = new List<string>();
            List<string> prodHouses = new List<string>();
            foreach (HouseType house in HouseTypes)
            {
                if (housesWithProd.Contains(house.Name))
                {
                    prodHouses.Add(house.Name);
                }
            }
            info.Add("Production-capable Houses: " + (prodHouses.Count == 0 ? "None" : String.Join(", ", prodHouses.ToArray())));
            foreach (HouseType house in HouseTypes)
            {
                int houseStorageBuilt;
                if (storageWithUnbuilt.TryGetValue(house.Name, out int houseStorageAll))
                {
                    storageWithoutUnbuilt.TryGetValue(house.Name, out houseStorageBuilt);

                    string houseInfo = String.Format("{0}: Storage capacity: {1}. (Without unbuilt: {2})",
                        house.Name, houseStorageAll, houseStorageBuilt);
                    info.Add(houseInfo);
                }
                else if (storageWithoutUnbuilt.TryGetValue(house.Name, out houseStorageBuilt))
                {
                    string houseInfo = String.Format("{0}: Storage capacity: {1}.",
                        house.Name, houseStorageBuilt);
                    info.Add(houseInfo);
                }
            }
            return info;
        }

        public void ResetCachedGraphics()
        {
            // Dispose of cached images. This is non-destructive; the type objects themselves
            // don't actually get disposed. Their thumbnail simply gets disposed and cleared.
            foreach (ITechnoType technoType in AllTeamTechnoTypes)
            {
                // units, boats, aircraft, infantry
                technoType.Reset();
            }
            // probably not needed since it's in the team techno types.
            foreach (UnitType unitType in AllUnitTypes)
            {
                unitType.Reset();
            }
            // probably not needed since it's in the team techno types.
            foreach (InfantryType infantryType in AllInfantryTypes)
            {
                infantryType.Reset();
            }
            foreach (BuildingType buildingType in BuildingTypes)
            {
                buildingType.Reset();
            }
            foreach (TemplateType template in TemplateTypes)
            {
                template.Reset();
            }
            foreach (TerrainType terrainType in TerrainTypes)
            {
                terrainType.Reset();
            }
            foreach (OverlayType overlayType in OverlayTypes)
            {
                overlayType.Reset();
            }
            foreach (SmudgeType smudgeType in SmudgeTypes)
            {
                smudgeType.Reset();
            }
        }

        public void ClearEvents()
        {
            WaypointsUpdated = null;
            TriggersUpdated = null;
            RulesChanged = null;
            MapContentsChanged = null;
        }

        /// <summary>Gets a view of the initial viewport at the start of a mission</summary>
        /// <param name="homeWpIsCenter">True if the "Home" waypoint indicates the center of the viewport, not the top left corner.</param>
        /// <param name="dos">Get the closeup DOS 13x8 cells viewport, rather than the win95 26x16 one.</param>
        /// <param name="square">Select a square inside the standard rectangular viewport.</param>
        /// <param name="focusPlayerUnits">If <paramref name="square"/> is true, move the square's horizontal position inside the full viewport to highlight where player units are.</param>
        /// <returns>A rectangle representing the starting position of a mission.</returns>
        public Rectangle GetSoloViewport(bool homeWpIsCenter, bool dos, bool square, bool focusPlayerUnits)
        {
            Waypoint startPoint = Waypoints.FirstOrDefault(wp => wp.Flags.HasFlag(WaypointFlag.Home));
            return GetSoloViewport(homeWpIsCenter, startPoint, dos, square, focusPlayerUnits);
        }

        /// <summary>Gets a view of the initial viewport at the start of a mission</summary>
        /// <param name="homeWpIsCenter">True if the "Home" waypoint indicates the center of the viewport, not the top left corner.</param>
        /// <param name="startPoint">Waypoint to render the box from.</param>
        /// <param name="dos">Get the closeup DOS 13x8 cells viewport, rather than the win95 26x16 one.</param>
        /// <param name="square">Select a square inside the standard rectangular viewport.</param>
        /// <param name="focusPlayerUnits">If <paramref name="square"/> is true, move the square's horizontal position inside the full viewport to highlight where player units are.</param>
        /// <returns>A rectangle representing the starting position of a mission.</returns>
        public Rectangle GetSoloViewport(bool homeWpIsCenter, Waypoint startPoint, bool dos, bool square, bool focusPlayerUnits)
        {
            int width = dos ? 13 : 26;
            int height = dos ? 8 : 16;
            Point start = startPoint?.Point ?? Bounds.Location;
            Rectangle viewportRect = new Rectangle(start, new Size(width, height));
            if (startPoint != null && homeWpIsCenter)
            {
                // Do -1 on both to 'remove' center cell itself. This means that in case of even numbers, the
                // top and left edges will be rounded down in their distance in cells away from the center cell.
                viewportRect.Offset((width - 1) / (-2), (height - 1) / (-2));
            }
            if (!square)
            {
                viewportRect = GeneralUtils.ConstrainToBounds(viewportRect, Bounds);
                viewportRect = GeneralUtils.ConstrainToBounds(viewportRect, Metrics.Bounds);
                return viewportRect;
            }
            // Just to not short-circuit my brain...
            int squareWidth = height;
            List<Point> points = null;
            if (focusPlayerUnits)
            {
                HouseType player = HouseTypes.Where(t => t.Equals(BasicSection.Player)).FirstOrDefault() ?? HouseTypes.First();
                points = Technos
                    .Where(t => viewportRect.Contains(t.Location)
                    && ((t.Occupier as Unit)?.House == player || ((t.Occupier as InfantryGroup)?.Infantry.Any(i => i != null && i.House == player) ?? false)))
                    .Select(t => t.Location)
                    .Concat(Buildings.OfType<Building>().Where(b => b.Occupier.House == player)
                        .SelectMany(b => OccupierSet.GetOccupyPoints(b.Location, b.Occupier.BaseOccupyMask).Where(p => viewportRect.Contains(p))))
                    .Where(p => Bounds.Contains(p)).Distinct().ToList();
            }
            Rectangle returnRect = viewportRect;
            returnRect.Width = squareWidth;
            if (points == null || points.Count == 0)
            {
                int shift = (width - squareWidth) / 2;
                returnRect.Offset(shift, 0);
            }
            else
            {
                int minX = points.Min(p => p.X);
                int maxX = points.Max(p => p.X) + 1;
                int newCenter = minX + (maxX - minX) / 2;
                int newX = newCenter - squareWidth / 2;
                returnRect.X = newX;
            }
            // New rect can shift to left or right, but not outside the full viewport bounds.
            returnRect = GeneralUtils.ConstrainToBounds(returnRect, viewportRect);
            returnRect = GeneralUtils.ConstrainToBounds(returnRect, Bounds);
            returnRect = GeneralUtils.ConstrainToBounds(returnRect, Metrics.Bounds);
            return returnRect;
        }
    }
}
