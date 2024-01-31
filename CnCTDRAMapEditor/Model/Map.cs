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

        Boundaries      /**/ = 1 << 10,
        MapSymmetry     /**/ = 1 << 11,
        MapGrid         /**/ = 1 << 12,
        WaypointsIndic  /**/ = 1 << 13,
        FootballArea    /**/ = 1 << 14,
        CellTriggers    /**/ = 1 << 15,
        TechnoTriggers  /**/ = 1 << 16,
        BuildingRebuild /**/ = 1 << 17,
        BuildingFakes   /**/ = 1 << 18,
        EffectRadius    /**/ = 1 << 19,
        WaypointRadius  /**/ = 1 << 20,
        OverlapOutlines /**/ = 1 << 21,
        LandTypes       /**/ = 1 << 22,

        OverlayAll = Resources | Walls | Overlay,
        Technos = Terrain | Infantry | Units | Buildings,
        /// <summary>Listing of layers that are hard-painted onto the map image.</summary>
        MapLayers = Template | Terrain | Resources | Walls | Overlay | Smudge | Infantry | Units | Buildings | Waypoints,
        /// <summary>Listing of layers that don't need a full map repaint.</summary>
        Indicators = Boundaries | MapSymmetry | MapGrid | WaypointsIndic | FootballArea | CellTriggers
            | TechnoTriggers | BuildingRebuild | BuildingFakes | EffectRadius | WaypointRadius
            | OverlapOutlines | LandTypes,
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

        public Map Map => this.Instance as Map;

        public readonly bool FractionalPercentages;

        public MapContext(Map map, bool fractionalPercentages)
        {
            this.Instance = map;
            this.FractionalPercentages = fractionalPercentages;
        }

        public object GetService(Type serviceType) => null;

        public void OnComponentChanged() { }

        public bool OnComponentChanging() => true;
    }

    public class Map : ICloneable
    {
        /// <summary>
        /// Enum specifying how filled a cell of concrete is.
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

        private static readonly int randomSeed;
        private static Dictionary<ConcFill, int> concreteStateToIcon = new Dictionary<ConcFill, int>();

        static Map()
        {
            randomSeed = Guid.NewGuid().GetHashCode();
            concreteStateToIcon = iconFillStates.Select((value, index) => new { value, index })
                      .ToDictionary(pair => pair.value, pair => pair.index);
            // Add default, since it does not appear in the cellStates.
            concreteStateToIcon.Add(ConcFill.None, 0);
        }

        // Keep this list synchronised with the MapLayerFlag enum
        public static String[] MapLayerNames = {
            // Map layers
            "Map templates",
            "Terrain",
            "Infantry",
            "Units",
            "Buildings",
            "Overlay",
            "Walls",
            "Resources",
            "Smudge",
            "Waypoints",
            // Indicators
            "Map boundaries",
            "Map symmetry",
            "Map grid",
            "Waypoint labels",
            "Football goal areas",
            "Cell triggers",
            "Object triggers",
            "Building rebuild priorities",
            "Building 'fake' labels",
            "Jam / gap radiuses",
            "Waypoint reveal radiuses",
            "Object outlines",
            "Land types"
        };

        private static int[] tiberiumStages = new int[] { 0, 1, 3, 4, 6, 7, 8, 10, 11 };
        private static int[] gemStages = new int[] { 0, 0, 0, 1, 1, 1, 2, 2, 2 };
        /// <summary>Facings to check for adjacent cells around even-cell concrete. The order in this array matches the bits order in the ConcAdj enum.</summary>
        private static FacingType[] concreteCheckEven = { FacingType.North, FacingType.NorthWest, FacingType.West, FacingType.SouthWest, FacingType.South };
        /// <summary>Facings to check for adjacent cells around odd-cell concrete. The order in this array matches the bits order in the ConcAdj enum.</summary>
        private static FacingType[] concreteCheckOdd = { FacingType.North, FacingType.NorthEast, FacingType.East, FacingType.SouthEast, FacingType.South };

        /// <summary>
        /// Represents the order of the different fill states inside the CONC sprite file.
        /// Each state has two icons; one for odd and one for even cells. Note that for some
        /// reason, the first state has these switched on the sprite; it has the even graphics first.
        /// </summary>
        private static ConcFill[] iconFillStates = {
            /* 0 */ ConcFill.Center,
            /* 1 */ ConcFill.Center | ConcFill.Bottom | ConcFill.Top,
            /* 2 */ ConcFill.Top,
            /* 3 */ ConcFill.Bottom,
            /* 4 */ ConcFill.Center | ConcFill.Bottom,
            /* 5 */ ConcFill.Center | ConcFill.Top,
            /* 6 */ ConcFill.Bottom | ConcFill.Top,
        };

        private static Regex tileInfoSplitRegex = new Regex("^([^:]+):(\\d+)$", RegexOptions.Compiled);

        private int updateCount = 0;
        private bool updating = false;
        private IDictionary<MapLayerFlag, ISet<Point>> invalidateLayers = new Dictionary<MapLayerFlag, ISet<Point>>();
        private bool invalidateOverlappers;

        public event EventHandler<MapRefreshEventArgs> RulesChanged;
        public void NotifyRulesChanges(ISet<Point> refreshPoints)
        {
            if (RulesChanged != null)
            {
                this.RulesChanged(this, new MapRefreshEventArgs(refreshPoints));
            }
        }

        public event EventHandler<MapRefreshEventArgs> MapContentsChanged;
        public void NotifyMapContentsChanged(ISet<Point> refreshPoints)
        {
            if (MapContentsChanged != null)
            {
                this.MapContentsChanged(this, new MapRefreshEventArgs(refreshPoints));
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

        public TheaterType Theater { get => this.MapSection.Theater; set => this.MapSection.Theater = value; }

        public Point TopLeft
        {
            get => new Point(this.MapSection.X, this.MapSection.Y);
            set { this.MapSection.X = value.X; this.MapSection.Y = value.Y; }
        }

        public Size Size
        {
            get => new Size(this.MapSection.Width, this.MapSection.Height);
            set { this.MapSection.Width = value.Width; this.MapSection.Height = value.Height; }
        }

        public Rectangle Bounds
        {
            get => this.MapSection.Bounds;
            set { this.MapSection.X = value.Left; this.MapSection.Y = value.Top; this.MapSection.Width = value.Width; this.MapSection.Height = value.Height; }
        }

        public bool ForPreview { get; private set; }

        public readonly Type HouseType;

        public readonly HouseType[] HouseTypes;

        public readonly HouseType[] HouseTypesIncludingSpecials;

        public ITeamColor[] FlagColors { get; set; }

        public readonly List<TheaterType> TheaterTypes;

        public readonly List<TemplateType> TemplateTypes;

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
                if (this.BasicSection == null || !this.BasicSection.ExpansionEnabled)
                {
                    return this.AllInfantryTypes.Where(inf => !inf.IsExpansionOnly).ToList();
                }
                return this.AllInfantryTypes.ToList();
            }
        }

        public readonly List<UnitType> AllUnitTypes;
        public List<UnitType> UnitTypes
        {
            get
            {
                if (this.BasicSection == null || !this.BasicSection.ExpansionEnabled)
                {
                    return this.AllUnitTypes.Where(un => !un.IsExpansionOnly).ToList();
                }
                return this.AllUnitTypes.ToList();
            }
        }

        public readonly List<BuildingType> BuildingTypes;

        public readonly List<ITechnoType> AllTeamTechnoTypes;
        public List<ITechnoType> TeamTechnoTypes
        {
            get
            {
                if (this.BasicSection == null || !this.BasicSection.ExpansionEnabled)
                {
                    return this.AllTeamTechnoTypes.Where(tc => !tc.IsExpansionOnly).ToList();
                }
                return this.AllTeamTechnoTypes.ToList();
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
            get { return this.triggers; }
            set
            {
                this.triggers = value;
                // Only an actual replacing of the list will call these, but they can be called manually after an update.
                // A bit more manual than the whole ObservableCollection system, but a lot less cumbersome.
                this.NotifyTriggersUpdate();
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
                return this.GetTotalResources(true);
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
            IEnumerable<DirectionType> unitDirectionTypes, IEnumerable<DirectionType> buildingDirectionTypes, IEnumerable<InfantryType> infantryTypes,
            IEnumerable<UnitType> unitTypes, IEnumerable<BuildingType> buildingTypes, IEnumerable<TeamMission> teamMissionTypes, IEnumerable<ITechnoType> teamTechnoTypes,
            IEnumerable<Waypoint> waypoints, int dropZoneRadius, int gapRadius, int jamRadius, IEnumerable<string> movieTypes, string emptyMovie, IEnumerable<string> themeTypes, string emptyTheme,
            int tiberiumOrGoldValue, int gemValue)
        {
            this.MapSection = new MapSection(cellSize);
            this.BasicSection = basicSection;
            this.HouseType = houseType;
            this.HouseTypesIncludingSpecials = houseTypes.ToArray();
            this.HouseTypes = this.HouseTypesIncludingSpecials.Where(h => (h.Flags & HouseTypeFlag.Special) == HouseTypeFlag.None).ToArray();
            this.FlagColors = flagColors == null ? new ITeamColor[8] : flagColors;
            this.TheaterTypes = new List<TheaterType>(theaterTypes);
            this.TemplateTypes = new List<TemplateType>(templateTypes);
            this.TerrainTypes = new List<TerrainType>(terrainTypes);
            this.OverlayTypes = new List<OverlayType>(overlayTypes);
            this.SmudgeTypes = new List<SmudgeType>(smudgeTypes);
            this.EventTypes = eventTypes.ToArray();
            this.CellEventTypes = cellEventTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            this.UnitEventTypes = unitEventTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            this.BuildingEventTypes = buildingEventTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            this.TerrainEventTypes = terrainEventTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            this.CellActionTypes = cellActionTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            this.UnitActionTypes = unitActionTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            this.BuildingActionTypes = buildingActionTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);
            this.TerrainActionTypes = terrainActionTypes.ToHashSet(StringComparer.OrdinalIgnoreCase);

            this.ActionTypes = actionTypes.ToArray();
            this.MissionTypes = missionTypes.ToArray();
            string defMission = this.MissionTypes.Where(m => m.Equals(defaultMission, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() ?? this.MissionTypes.First();
            // Unfiltered originals, to ensure this remains correct when cloning.
            this.inputMissionArmed = armedMission;
            this.inputMissionUnarmed = unarmedMission;
            this.inputMissionAircraft = harvestMission;
            this.inputMissionHarvest = aircraftMission;

            this.DefaultMissionArmed = this.MissionTypes.Where(m => m.Equals(armedMission, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() ?? defMission;
            this.DefaultMissionUnarmed = this.MissionTypes.Where(m => m.Equals(unarmedMission, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() ?? defMission;
            // Reverts to "Stop" if there are no resources (RA indoor)
            this.DefaultMissionHarvest = this.OverlayTypes.Any(ov => ov.IsResource) ? this.MissionTypes.Where(m => m.Equals(harvestMission, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() ?? this.DefaultMissionUnarmed : this.DefaultMissionUnarmed;
            // Only "Unload" will make them stay on the spot as expected.
            this.DefaultMissionAircraft = this.MissionTypes.Where(m => m.Equals(aircraftMission, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() ?? defMission;
            this.UnitDirectionTypes = new List<DirectionType>(unitDirectionTypes);
            this.BuildingDirectionTypes = new List<DirectionType>(buildingDirectionTypes);
            this.AllInfantryTypes = new List<InfantryType>(infantryTypes);
            this.AllUnitTypes = new List<UnitType>(unitTypes);
            this.BuildingTypes = new List<BuildingType>(buildingTypes);
            this.TeamMissionTypes = teamMissionTypes.ToArray();
            this.AllTeamTechnoTypes = new List<ITechnoType>(teamTechnoTypes);
            this.MovieEmpty = emptyMovie;
            this.MovieTypes = new List<string>(movieTypes);
            this.ThemeEmpty = emptyTheme;
            this.ThemeTypes = new List<string>(themeTypes);
            this.TiberiumOrGoldValue = tiberiumOrGoldValue;
            this.GemValue = gemValue;
            this.Metrics = new CellMetrics(cellSize);
            this.Templates = new CellGrid<Template>(this.Metrics);
            this.Overlay = new CellGrid<Overlay>(this.Metrics);
            this.Smudge = new CellGrid<Smudge>(this.Metrics);
            this.Technos = new OccupierSet<ICellOccupier>(this.Metrics);
            this.Buildings = new OccupierSet<ICellOccupier>(this.Metrics);
            this.Overlappers = new OverlapperSet<ICellOverlapper>(this.Metrics);
            this.triggers = new List<Trigger>();
            this.TeamTypes = new List<TeamType>();
            this.HousesIncludingSpecials = this.HouseTypesIncludingSpecials.Select(t => { House h = (House)Activator.CreateInstance(this.HouseType, t); h.SetDefault(); return h; }).ToArray();
            this.Houses = this.HousesIncludingSpecials.Where(h => (h.Type.Flags & (HouseTypeFlag.Special)) == HouseTypeFlag.None).ToArray();
            // Build houses list for allies. Special houses not shown in the normal houses lists (e.g. 'Allies' and 'Soviet') are put first.
            List<House> housesAlly = this.HousesIncludingSpecials.Where(h => (h.Type.Flags & (HouseTypeFlag.ForAlliances)) == HouseTypeFlag.ForAlliances).ToList();
            List<House> housesAllySpecial = housesAlly.Where(h => (h.Type.Flags & (HouseTypeFlag.Special)) == HouseTypeFlag.Special).OrderBy(h => h.Type.ID).ToList();
            List<House> housesAllyNormal = housesAlly.Where(h => (h.Type.Flags & (HouseTypeFlag.Special)) == HouseTypeFlag.None).OrderBy(h => h.Type.ID).ToList();
            this.HousesForAlliances = housesAllySpecial.Concat(housesAllyNormal).ToArray();
            this.HouseNone = this.HousesIncludingSpecials.Where(h => (h.Type.Flags & (HouseTypeFlag.BaseHouse)) == HouseTypeFlag.BaseHouse).FirstOrDefault();
            Waypoint[] wp = waypoints.ToArray();
            this.Waypoints = new Waypoint[wp.Length];
            for (int i = 0; i < wp.Length; ++i)
            {
                // Deep clone with current metrics, to allow showing waypoints as cell coordinates.
                this.Waypoints[i] = new Waypoint(wp[i].Name, wp[i].Flag, this.Metrics, wp[i].Cell);
            }
            this.DropZoneRadius = dropZoneRadius;
            this.GapRadius = gapRadius;
            this.RadarJamRadius = jamRadius;
            this.CellTriggers = new CellGrid<CellTrigger>(this.Metrics);

            // Optimisation: checks on what is inside the given data, used to prevent unnecessary logic from executing.
            this.ConcreteOverlaysAvailable = this.OverlayTypes.Any(ovl => ovl.IsConcrete);
            this.CrateOverlaysAvailable = this.OverlayTypes.Any(ovl => ovl.IsCrate);
            this.FlareWaypointAvailable = this.Waypoints.Any(wpt => (wpt.Flag & WaypointFlag.Flare) != WaypointFlag.None);
            this.ExpansionUnitsAvailable = BuildingTypes.Any(tt => tt.IsExpansionOnly)
                || AllInfantryTypes.Any(tt => tt.IsExpansionOnly)
                || TerrainTypes.Any(tt => tt.IsExpansionOnly)
                || AllUnitTypes.Any(tt => tt.IsExpansionOnly);

            this.MapSection.SetDefault();
            this.BriefingSection.SetDefault();
            this.SteamSection.SetDefault();
            this.Templates.Clear();
            this.Overlay.Clear();
            this.Smudge.Clear();
            this.Technos.Clear();
            this.Overlappers.Clear();
            this.CellTriggers.Clear();

            this.TopLeft = new Point(1, 1);
            this.Size = this.Metrics.Size - new Size(2, 2);
            this.Theater = theater;

            this.Overlay.CellChanged += this.Overlay_CellChanged;
            this.Technos.OccupierAdded += this.Technos_OccupierAdded;
            this.Technos.OccupierRemoved += this.Technos_OccupierRemoved;
            this.Buildings.OccupierAdded += this.Buildings_OccupierAdded;
            this.Buildings.OccupierRemoved += this.Buildings_OccupierRemoved;
        }

        public void BeginUpdate()
        {
            this.updateCount++;
        }

        public void EndUpdate()
        {
            if (--this.updateCount == 0)
            {
                this.Update();
            }
        }

        public void InitTheater(GameInfo gameInfo)
        {
            try
            {
                foreach (TemplateType templateType in this.TemplateTypes)
                {
                    templateType.Init(gameInfo, this.Theater, Globals.FilterTheaterObjects);
                }
                foreach (SmudgeType smudgeType in this.SmudgeTypes)
                {
                    smudgeType.Init(this.Theater);
                }
                foreach (OverlayType overlayType in this.OverlayTypes)
                {
                    overlayType.Init(gameInfo, this.Theater);
                }
                string th = this.Theater.Name;
                foreach (TerrainType terrainType in this.TerrainTypes.Where(itm => !Globals.FilterTheaterObjects || itm.Theaters == null || itm.Theaters.Contains(th)))
                {
                    terrainType.Init();
                    terrainType.InitDisplayName();
                }
                // Ignore expansion status for these; they can still be enabled later.
                DirectionType infDir = this.UnitDirectionTypes.Where(d => d.Facing == FacingType.South).First();
                foreach (InfantryType infantryType in this.AllInfantryTypes)
                {
                    infantryType.Init(this.HouseTypesIncludingSpecials.Where(h => h.Equals(infantryType.OwnerHouse)).FirstOrDefault(), infDir);
                }
                DirectionType unitDir = this.UnitDirectionTypes.Where(d => d.Facing == FacingType.SouthWest).First();
                foreach (UnitType unitType in this.AllUnitTypes)
                {
                    unitType.Init(gameInfo, this.HouseTypesIncludingSpecials.Where(h => h.Equals(unitType.OwnerHouse)).FirstOrDefault(), unitDir);
                }
                // Required for initialising air unit names for teamtypes if DisableAirUnits is true.
                foreach (ITechnoType techno in this.AllTeamTechnoTypes)
                {
                    techno.InitDisplayName();
                }
                DirectionType bldDir = this.UnitDirectionTypes.Where(d => d.Facing == FacingType.North).First();
                // No restriction. All get attempted and dummies are all filled in.
                foreach (BuildingType buildingType in this.BuildingTypes)
                {
                    buildingType.Init(gameInfo, this.HouseTypesIncludingSpecials.Where(h => h.Equals(buildingType.OwnerHouse)).FirstOrDefault(), bldDir);
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
            this.updating = true;
            if (this.invalidateLayers.TryGetValue(MapLayerFlag.Resources, out ISet<Point> locations))
            {
                this.UpdateResourceOverlays(locations, true);
            }
            if (this.invalidateLayers.TryGetValue(MapLayerFlag.Walls, out locations))
            {
                this.UpdateWallOverlays(locations);
            }
            if (Globals.AllowWallBuildings && this.invalidateLayers.TryGetValue(MapLayerFlag.Buildings, out locations))
            {
                this.UpdateWallOverlays(locations);
            }
            if (this.invalidateLayers.TryGetValue(MapLayerFlag.Overlay, out locations))
            {
                this.UpdateConcreteOverlays(locations);
            }
            if (this.invalidateOverlappers)
            {
                this.Overlappers.Clear();
                foreach ((Point location, ICellOccupier techno) in this.Technos)
                {
                    if (techno is ICellOverlapper)
                    {
                        this.Overlappers.Add(location, techno as ICellOverlapper);
                    }
                }
            }
            this.invalidateLayers.Clear();
            this.invalidateOverlappers = false;
            this.updating = false;
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
            foreach ((Int32 cell, Overlay value) in this.Overlay)
            {
                Point point;
                if (!value.Type.IsResource || !this.Metrics.GetLocation(cell, out point))
                {
                    continue;
                }
                if (inBounds && !this.Bounds.Contains(point))
                {
                    continue;
                }
                int adj = 0;
                foreach (FacingType facing in CellMetrics.AdjacentFacings)
                {
                    Overlay ovl;
                    if (this.Metrics.Adjacent(point, facing, out Point adjPoint)
                        && (ovl = this.Overlay[adjPoint]) != null && ovl.Type.IsResource)
                    {
                        if (inBounds && !this.Bounds.Contains(adjPoint))
                        {
                            continue;
                        }
                        adj++;
                    }
                }
                int thickness = value.Type.IsGem ? gemStages[adj] : tiberiumStages[adj];
                // Harvesting has a bug where the final stage returns a value of 0 since it uses the 0-based icon index.
                // Harvesting one gem stage fills one bail, plus 3 extra bails. Last stage is 0 (due to that bug), but still gets the extra bails.
                if (Globals.ApplyHarvestBug)
                {
                    totalResources += value.Type.IsGem ? thickness * this.GemValue + this.GemValue * 3 : thickness * this.TiberiumOrGoldValue;
                }
                else
                {
                    // Fixed logic, in case it is repaired in the code.
                    totalResources += (thickness + 1) * (value.Type.IsGem ? this.GemValue * 4 : this.TiberiumOrGoldValue);
                }
            }
            return totalResources;
        }

        /// <summary>
        /// Update resource overlay to the desired density and randomised type.
        /// </summary>
        /// <param name="locations">Set of Locations on which changes occurred.</param>
        /// <param name="reduceOutOfBounds">True if resources out of bounds are reduced to minimum size and marked to be tinted red.</param>
        /// <remarks> This function is separate from GetTotalResources because it only updates the specified areas.</remarks>
        public void UpdateResourceOverlays(ISet<Point> locations, bool reduceOutOfBounds)
        {
            Rectangle checkBounds = reduceOutOfBounds ? this.Bounds : this.Metrics.Bounds;
            OverlayType[] tiberiumOrGoldTypes = this.OverlayTypes.Where(t => t.IsTiberiumOrGold).ToArray();
            if (tiberiumOrGoldTypes.Length == 0) tiberiumOrGoldTypes = null;
            OverlayType[] gemTypes = this.OverlayTypes.Where(t => t.IsGem).ToArray();
            if (gemTypes.Length == 0) gemTypes = null;
            foreach ((Point location, Overlay overlay) in this.Overlay.IntersectsWithPoints(locations).Where(o => o.Value.Type.IsResource))
            {
                int count = 0;
                bool inBounds = checkBounds.Contains(location);
                if (inBounds)
                {
                    foreach (FacingType facing in CellMetrics.AdjacentFacings)
                    {
                        if (this.Metrics.Adjacent(location, facing, out Point adjacent) && checkBounds.Contains(adjacent))
                        {
                            Overlay adjacentOverlay = this.Overlay[adjacent];
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
                overlay.Tint = inBounds ? Color.White : Color.FromArgb(0x80, 0xFF, 0x80, 0x80);
                overlay.Icon = overlay.Type.IsGem ? gemStages[count] : tiberiumStages[count];
            }
        }

        public void UpdateWallOverlays(ISet<Point> locations)
        {
            foreach ((Point location, Overlay overlay) in this.Overlay.IntersectsWithPoints(locations).Where(o => o.Value.Type.IsWall))
            {
                OverlayType ovt = overlay.Type;
                bool hasNorthWall = this.Overlay.Adjacent(location, FacingType.North)?.Type == ovt;
                bool hasEastWall = this.Overlay.Adjacent(location, FacingType.East)?.Type == ovt;
                bool hasSouthWall = this.Overlay.Adjacent(location, FacingType.South)?.Type == ovt;
                bool hasWestWall = this.Overlay.Adjacent(location, FacingType.West)?.Type == ovt;
                if (Globals.AllowWallBuildings)
                {
                    String ovtName = overlay.Type.Name;
                    hasNorthWall |= (this.Metrics.Adjacent(location, FacingType.North, out Point north) ? this.Buildings[north] as Building : null)?.Type.Name == ovtName;
                    hasEastWall |= (this.Metrics.Adjacent(location, FacingType.East, out Point east) ? this.Buildings[east] as Building : null)?.Type.Name == ovtName;
                    hasSouthWall |= (this.Metrics.Adjacent(location, FacingType.South, out Point south) ? this.Buildings[south] as Building : null)?.Type.Name == ovtName;
                    hasWestWall |= (this.Metrics.Adjacent(location, FacingType.West, out Point west) ? this.Buildings[west] as Building : null)?.Type.Name == ovtName;
                }
                int icon = (hasNorthWall ? 1 : 0) | (hasEastWall ? 2 : 0) | (hasSouthWall ? 4 : 0) | (hasWestWall ? 8 : 0);
                overlay.Icon = icon;
            }
        }

        public static bool IsIgnorableOverlay(Overlay overlay)
        {
            if (overlay == null)
                return true;
            if (overlay.Type.IsConcrete && (GetConcState(overlay) & ConcFill.Center) == ConcFill.None)
            {
                // Filler cell. Ignore.
                return true;
            }
            return false;
        }

        public void UpdateConcreteOverlays(ISet<Point> locations)
        {
            if (!this.ConcreteOverlaysAvailable)
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
                //UpdateConcreteOverlays_ORIG(locations);
            }
        }

        private void UpdateConcreteOverlaysCorrect(ISet<Point> locations, bool forExtraCells)
        {
            // Add the points around extra cells
            HashSet<Point> updateLocations = new HashSet<Point>(locations);
            HashSet<Point> newExtraCellsToAdd = new HashSet<Point>();
            foreach ((Point pt, Overlay overlay) in this.Overlay.IntersectsWithPoints(locations).Where(o => o.Value.Type.IsConcrete))
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
                foreach ((Point pt, Overlay overlay) in this.Overlay.IntersectsWithPoints(loopList).Where(o => o.Value.Type.IsConcrete))
                {
                    if (!IsIgnorableOverlay(overlay))
                    {
                        continue;
                    }
                    FacingType[] adjCells = pt.X % 2 == 1 ? concreteCheckOdd : concreteCheckEven;
                    for (int i = 0; i < adjCells.Length; i++)
                    {
                        if (!this.Metrics.Adjacent(pt, adjCells[i], out Point adjacent))
                        {
                            continue;
                        }
                        Overlay adj = this.Overlay[adjacent];
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
            foreach ((Int32 cell, Overlay overlay) in this.Overlay.IntersectsWithCells(updateLocations).Where(o => o.Value.Type.IsConcrete))
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
                FacingType[] adjCells = isodd ? concreteCheckOdd : concreteCheckEven;
                ConcAdj mask = ConcAdj.None;
                int[] cells = new int[adjCells.Length];
                for (int i = 0; i < adjCells.Length; i++)
                {
                    Overlay neighbor = this.Overlay.Adjacent(cell, adjCells[i]);
                    cells[i] = -1;
                    if (this.Metrics.Adjacent(cell, adjCells[i], out int adjacent))
                        cells[i] = adjacent;
                    if (neighbor?.Type == overlay.Type)
                    {
                        if ((GetConcState(neighbor) & ConcFill.Center) != ConcFill.None)
                        {
                            mask |= (ConcAdj)(1 << i);
                        }
                    }
                }
                // Unified logic so the operation becomes identical for the even and odd cells.
                // This still isn't a 100% match with the game, but that's because the version in-game is a buggy mess.
                bool top = (mask & ConcAdj.Top) != ConcAdj.None;
                bool topSide = (mask & ConcAdj.TopSide) != ConcAdj.None;
                bool side = (mask & ConcAdj.Side) != ConcAdj.None;
                bool bottomSide = (mask & ConcAdj.BottomSide) != ConcAdj.None;
                bool bottom = (mask & ConcAdj.Bottom) != ConcAdj.None;

                // Logic to fill the main cell. Standard for a placed cell is to fill the center.
                ConcFill fillState = ConcFill.Center;
#if false
                // OLD LOGIC: Does not fill triangles between two vertical cells
                // If two out of three of top, side and top-side are filled, fill the top.
                if ((side && (topSide || top)) || (topSide && (side || top)))
                    fillState |= ConcFill.Top;
                // If two out of three of bottom, side and bottom-side are filled, fill the bottom.
                if ((side && (bottomSide || bottom)) || (bottomSide && (side || bottom)))
                    fillState |= ConcFill.Bottom;

                // Logic to fill in edge cells. See what the currently evaluated cell will add to it.
                int cellTop = cells[0];
                int cellSide = cells[2];
                int cellBottom = cells[4];
                // If top cell is clear, and current cell has its top filled, then side and top-side are added, so add bottom connection piece in top cell.
                ConcFill fillStateTop = ConcFill.None;
                if (!top && (fillState & ConcFill.Top) != 0)
                    fillStateTop |= ConcFill.Bottom;
                // If bottom cell is clear, and current cell has its bottom filled, then side and bottom-side are added, so add top connection piece in bottom cell.
                ConcFill fillStateBottom = ConcFill.None;
                if (!bottom && (fillState & ConcFill.Bottom) != 0)
                    fillStateBottom |= ConcFill.Top;
                // join side aside-top
                // If side cell is clear, and current cell has its top filled, then top and top-side are added, so add top connection piece in side cell.
                ConcFill fillStateSide = ConcFill.None;
                if (!side && (fillState & ConcFill.Top) != 0)
                    fillStateSide |= ConcFill.Top;
                // If side cell is clear, and current cell has its bottom filled, then bottom and bottom-side are added, so add bottom connection piece in side cell.
                if (!side && (fillState & ConcFill.Bottom) != 0)
                    fillStateSide |= ConcFill.Bottom;
#else
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
#endif
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
            foreach (Int32 cell in addCells)
            {
                Point? pt = Metrics.GetLocation(cell);
                toRemove.Remove(cell);
                // Only allow updating the actual given points.
                if (forExtraCells && pt != null && !locations.Contains(pt.Value))
                    continue;
                OverlayType toMake = ovlTypes[cell];
                ConcFill addState = addedCells[cell];
                Overlay ovl = this.Overlay[cell];
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
                    this.Overlay[cell] = ovl;
                }
                if (!forExtraCells && pt.HasValue)
                {
                    addedPoints.Add(pt.Value);
                }
            }
            foreach (Int32 cell in toRemove)
            {
                this.Overlay[cell] = null;
            }
            if (!forExtraCells && addedPoints.Count > 0)
            {
                UpdateConcreteOverlaysCorrect(addedPoints, true);
            }
        }

        private void UpdateConcreteOverlaysGame(ISet<Point> locations)
        {
            foreach ((Int32 cell, Overlay overlay) in this.Overlay.IntersectsWithCells(locations).Where(o => o.Value.Type.IsConcrete))
            {
                bool isodd = (cell & 1) == 1;
                // Cells to check around the current cell. In order: top, top side, side, bottom side, bottom
                FacingType[] adjCells = isodd ? concreteCheckOdd : concreteCheckEven;
                ConcAdj mask = 0;
                for (int i = 0; i < adjCells.Length; i++)
                {
                    Overlay neighbor = this.Overlay.Adjacent(cell, adjCells[i]);
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
            if (val < 0 || val > iconFillStates.Length)
            {
                return ConcFill.None;
            }
            return iconFillStates[val];
        }

        private static int GetConcIcon(ConcFill fillState, bool isOdd)
        {
            int odd = isOdd ? 1 : 0;
            concreteStateToIcon.TryGetValue(fillState, out int val);
            // For some reason the odd and even icons for state 0 are swapped compared to all the others, so
            // an extra check has to be added for that. Otherwise just "(state * 2) + 1 - odd" would suffice.
            return val == 0 ? odd : (val * 2) + 1 - odd;
        }

        private enum ConcreteEnum
        {
            C_NONE         /**/ = -1,
            C_LEFT         /**/ = 0,
            C_RIGHT        /**/ = 1,
            C_RIGHT_UPDOWN /**/ = 2,
            C_LEFT_UPDOWN  /**/ = 3,
            C_UP_RIGHT     /**/ = 4,
            C_UP_LEFT      /**/ = 5,
            C_DOWN_RIGHT   /**/ = 6,
            C_DOWN_LEFT    /**/ = 7,
            C_RIGHT_DOWN   /**/ = 8,
            C_LEFT_DOWN    /**/ = 9,
            C_RIGHT_UP     /**/ = 10,
            C_LEFT_UP      /**/ = 11,
            C_UPDOWN_RIGHT /**/ = 12,
            C_UPDOWN_LEFT  /**/ = 13
        }

        private void UpdateConcreteOverlays_ORIG(ISet<Point> locations)
        {
            foreach ((Int32 cell, Overlay overlay) in this.Overlay.IntersectsWithCells(locations).Where(o => o.Value.Type.IsConcrete))
            {
                // Original logic as it is in the game code. Still doesn't match reality, probably due to bugs in the logic to add side cells.
                FacingType[] odd = { FacingType.North, FacingType.NorthEast, FacingType.East, FacingType.SouthEast, FacingType.South };
                FacingType[] even = { FacingType.North, FacingType.South, FacingType.SouthWest, FacingType.West, FacingType.NorthWest };
                int isodd = cell & 1;
                FacingType[] cells = isodd != 0 ? odd : even;
                int index = 0;
                for (int i = 0; i < cells.Length; i++)
                {
                    Overlay neighbor = this.Overlay.Adjacent(cell, cells[i]);
                    if (neighbor != null && neighbor.Type == overlay.Type)
                    {
                        int ic = overlay.Icon;
                        if (ic < 4 || (ic > 7 && ic < 12))
                        {
                            index |= (1 << i);
                        }
                    }
                }
                const int OF_N = 0x01;
                const int OF_NE = 0x02;
                const int OF_E = 0x04;
                const int OF_SE = 0x08;
                const int OF_S = 0x10;
                const int EF_N = 0x01;
                const int EF_NW = 0x10;
                const int EF_W = 0x08;
                const int EF_SW = 0x04;
                const int EF_S = 0x02;
                ConcreteEnum icon = 0;
                if (isodd != 0)
                {
                    switch (index)
                    {
                        case OF_NE:
                        case OF_N | OF_NE:
                        case OF_E | OF_N:
                        case OF_E | OF_NE:
                        case OF_N | OF_NE | OF_E:
                        case OF_S | OF_N | OF_NE:
                            icon = ConcreteEnum.C_RIGHT_UP;      // right - up
                            break;
                        case OF_SE:
                        case OF_E | OF_SE:
                        case OF_S | OF_SE:
                        case OF_S | OF_E:
                        case OF_S | OF_SE | OF_E:
                        case OF_S | OF_SE | OF_N:
                            icon = ConcreteEnum.C_RIGHT_DOWN;        // right - down
                            break;
                        case OF_SE | OF_NE:
                        case OF_SE | OF_NE | OF_N:
                        case OF_SE | OF_NE | OF_S:
                        case OF_SE | OF_NE | OF_S | OF_N:
                        case OF_SE | OF_E | OF_N:
                        case OF_SE | OF_E | OF_NE | OF_N:
                        case OF_S | OF_E | OF_N:
                        case OF_S | OF_E | OF_NE:
                        case OF_S | OF_E | OF_NE | OF_N:
                        case OF_S | OF_SE | OF_E | OF_N:
                        case OF_S | OF_SE | OF_E | OF_NE | OF_N:
                        case OF_S | OF_SE | OF_E | OF_NE:
                            icon = ConcreteEnum.C_RIGHT_UPDOWN;      // right - up - down
                            break;
                        default:
                            icon = ConcreteEnum.C_RIGHT;     // right
                            break;
                    }
                }
                else
                {
                    switch (index)
                    {
                        case EF_NW:
                        case EF_NW | EF_N:
                        case EF_W | EF_N:
                        case EF_NW | EF_W | EF_N:
                        case EF_NW | EF_W:
                        case EF_NW | EF_S | EF_N:
                            icon = ConcreteEnum.C_LEFT_UP;       // left - up
                            break;
                        case EF_SW:
                        case EF_SW | EF_S:
                        case EF_W | EF_S:
                        case EF_W | EF_SW | EF_S:
                        case EF_W | EF_SW:
                        case EF_SW | EF_S | EF_N:
                            icon = ConcreteEnum.C_LEFT_DOWN;     // left - down
                            break;
                        case EF_NW | EF_SW:
                        case EF_NW | EF_SW | EF_N:
                        case EF_NW | EF_SW | EF_S:
                        case EF_NW | EF_SW | EF_S | EF_N:
                        case EF_W | EF_S | EF_N:
                        case EF_W | EF_SW | EF_N:
                        case EF_W | EF_SW | EF_S | EF_N:
                        case EF_NW | EF_W | EF_S:
                        case EF_NW | EF_W | EF_S | EF_N:
                        case EF_NW | EF_W | EF_SW | EF_S | EF_N:
                        case EF_NW | EF_W | EF_SW | EF_N:
                        case EF_NW | EF_W | EF_SW | EF_S:
                            icon = ConcreteEnum.C_LEFT_UPDOWN;       // left - up - down
                            break;
                        default:
                            icon = ConcreteEnum.C_LEFT;      // left
                            break;
                    }
                }
                overlay.Icon = (int)icon;
            }
        }

        public void SetMapTemplatesRaw(byte[] data, int width, int height, Dictionary<int, string> types, string fillType)
        {
            int maxY = Math.Min(this.Metrics.Height, height);
            int maxX = Math.Min(this.Metrics.Width, width);
            Dictionary<int, TemplateType> replaceTypes = new Dictionary<int, TemplateType>();
            Dictionary<int, int> replaceIcons = new Dictionary<int, int>();
            int fillIcon;
            TemplateType fillTile;
            this.SplitTileInfo(fillType, out fillTile, out fillIcon, "fillType", false);
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
                this.SplitTileInfo(tileType, out tile, out tileIcon, "types", false);
                replaceTypes[kvp.Key] = tile;
                if (tile != null)
                {
                    if ((tile.Flag & TemplateTypeFlag.Group) == TemplateTypeFlag.Group)
                    {
                        tile = tileIcon >= tile.GroupTiles.Length ?
                            null : this.TemplateTypes.Where(t => t.Name == tile.GroupTiles[tileIcon]).FirstOrDefault();
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
            this.Templates.Clear();
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
                            this.Templates[y, x] = curr == null ? null : new Template { Type = curr, Icon = icon };
                        }
                    }
                    else if (fillTile != null)
                    {
                        this.Templates[y, x] = new Template { Type = fillTile, Icon = fillIcon };
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
            Match m = tileInfoSplitRegex.Match(tileType);
            if (m.Success)
            {
                tileType = m.Groups[1].Value;
                tileIcon = Int32.Parse(m.Groups[2].Value);
            }
            tile = this.TemplateTypes.Where(t => String.Equals(tileType, t.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            string th = this.Theater.Name;
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
            if (!this.Metrics.GetCell(location, out int cell))
            {
                return String.Format("X = {0}, Y = {1}, No cell", location.X, location.Y);
            }
            bool inBounds = this.Bounds.Contains(location);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("X = {0}, Y = {1}, Cell = {2}", location.X, location.Y, cell);
            Template template = this.Templates[cell];
            TemplateType templateType = template?.Type;
            if (templateType != null)
            {
                sb.AppendFormat(", Template = {0} ({1}) ({2})", templateType.DisplayName, template.Icon, template.Type.GetLandType(template.Icon).ToString());
            }
            Smudge smudge = this.Smudge[cell];
            SmudgeType smudgeType = smudge?.Type;
            if (smudgeType != null)
            {
                sb.AppendFormat(", Smudge = {0}{1}", smudgeType.DisplayName, smudgeType.IsAutoBib ? " (Attached)" : String.Empty);
            }
            Overlay overlay = this.Overlay[cell];
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
            Terrain terrain = this.Technos[location] as Terrain;
            TerrainType terrainType = terrain?.Type;
            if (terrainType != null)
            {
                sb.AppendFormat(", Terrain = {0}", terrainType.DisplayName);
            }
            if (this.Technos[location] is InfantryGroup infantryGroup)
            {
                InfantryStoppingType i = InfantryGroup.ClosestStoppingTypes(subPixel).First();
                Infantry inf = infantryGroup.Infantry[(int)i];
                if (inf != null)
                {
                    sb.AppendFormat(", Infantry = {0} ({1})", inf.Type.DisplayName, InfantryGroup.GetStoppingTypeName(i));
                }
            }
            Unit unit = this.Technos[location] as Unit;
            UnitType unitType = unit?.Type;
            if (unitType != null)
            {
                sb.AppendFormat(", Unit = {0}", unitType.DisplayName);
            }
            Building building = this.Buildings[location] as Building;
            BuildingType buildingType = building?.Type;
            if (buildingType != null)
            {
                sb.AppendFormat(", Building = {0}", buildingType.DisplayName);
            }
            return sb.ToString();
        }

        public HouseType GetBaseHouse(GameInfo gameInfo)
        {
            if (this.HouseNone != null)
            {
                return this.HouseNone.Type;
            }
            String oppos = gameInfo.GetClassicOpposingPlayer(this.BasicSection.Player);
            return this.HouseTypes.Where(h => h.Equals(this.BasicSection.BasePlayer)).FirstOrDefault()
                ?? this.HouseTypes.Where(h => h.Equals(oppos)).FirstOrDefault()
                ?? this.HouseTypes.First();

        }

        private void RemoveBibs(Building building)
        {
            Int32[] bibCells = this.Smudge.IntersectsWithCells(building.BibCells).Where(x => x.Value.Type.IsAutoBib).Select(x => x.Cell).ToArray();
            foreach (Int32 cell in bibCells)
            {
                this.Smudge[cell] = null;
            }
            building.BibCells.Clear();
        }

        private void AddBibs(Point location, Building building)
        {
            Dictionary<Point, Smudge> bibPoints = building.GetBib(location, this.SmudgeTypes);
            if (bibPoints == null)
            {
                return;
            }
            foreach (Point p in bibPoints.Keys)
            {
                if (this.Metrics.GetCell(p, out int subCell))
                {
                    this.Smudge[subCell] = bibPoints[p];
                    building.BibCells.Add(subCell);
                }
            }
        }

        public Map Clone(bool forPreview)
        {
            Waypoint[] wpPreview = new Waypoint[this.Waypoints.Length + (forPreview ? 1 : 0)];
            Array.Copy(this.Waypoints, wpPreview, this.Waypoints.Length);
            if (forPreview)
            {
                wpPreview[this.Waypoints.Length] = new Waypoint("", null);
            }
            // This is a shallow clone; the map is new, but the placed contents all still reference the original objects.
            // These shallow copies are used for map preview during editing, where dummy objects can be added without any issue.
            Map map = new Map(this.BasicSection, this.Theater, this.Metrics.Size, this.HouseType, this.HouseTypesIncludingSpecials,
                this.FlagColors, this.TheaterTypes, this.TemplateTypes, this.TerrainTypes, this.OverlayTypes, this.SmudgeTypes,
                this.EventTypes, this.CellEventTypes, this.UnitEventTypes, this.BuildingEventTypes, this.TerrainEventTypes,
                this.ActionTypes, this.CellActionTypes, this.UnitActionTypes, this.BuildingActionTypes, this.TerrainActionTypes,
                this.MissionTypes, this.inputMissionArmed, this.inputMissionUnarmed, this.inputMissionHarvest, this.inputMissionAircraft,
                this.UnitDirectionTypes, this.BuildingDirectionTypes, this.AllInfantryTypes, this.AllUnitTypes, this.BuildingTypes, this.TeamMissionTypes,
                this.AllTeamTechnoTypes, wpPreview, this.DropZoneRadius, this.GapRadius, this.RadarJamRadius, this.MovieTypes, this.MovieEmpty, this.ThemeTypes, this.ThemeEmpty,
                this.TiberiumOrGoldValue, this.GemValue)
            {
                TopLeft = TopLeft,
                Size = Size,
                // Allows functions to check whether they are being applied on the real map or the preview map.
                ForPreview = forPreview
            };
            map.BeginUpdate();
            this.MapSection.CopyTo(map.MapSection);
            this.BriefingSection.CopyTo(map.BriefingSection);
            // Ignore processing-only "VisibilityAsEnum".
            this.SteamSection.CopyTo(map.SteamSection, typeof(NonSerializedINIKeyAttribute));
            Array.Copy(this.Houses, map.Houses, map.Houses.Length);
            map.Triggers.AddRange(this.Triggers);
            this.Templates.CopyTo(map.Templates);
            this.Overlay.CopyTo(map.Overlay);
            this.Smudge.CopyTo(map.Smudge);
            this.CellTriggers.CopyTo(map.CellTriggers);
            foreach ((Point location, ICellOccupier occupier) in this.Technos)
            {
                if (occupier is InfantryGroup infantryGroup)
                {
                    // This creates an InfantryGroup not linked to its infantry, but it is necessary
                    // to ensure that the real InfantryGroups are not polluted with dummy objects.
                    InfantryGroup newInfantryGroup = new InfantryGroup();
                    Array.Copy(infantryGroup.Infantry, newInfantryGroup.Infantry, newInfantryGroup.Infantry.Length);
                    map.Technos.Add(location, newInfantryGroup);
                }
                else if (!(occupier is Building))
                {
                    map.Technos.Add(location, occupier);
                }
            }
            foreach ((Point location, ICellOccupier building) in this.Buildings)
            {
                // Silly side effect: this fixes any building bibs.
                map.Buildings.Add(location, building);
            }
            map.TeamTypes.AddRange(this.TeamTypes);
            map.EndUpdate();
            return map;
        }

        public IEnumerable<Trigger> FilterCellTriggers()
        {
            return this.FilterCellTriggers(this.Triggers);
        }

        public IEnumerable<Trigger> FilterCellTriggers(IEnumerable<Trigger> triggers)
        {
            foreach (Trigger trigger in FilterTriggersByEvent(this.CellEventTypes, triggers).Concat(this.FilterTriggersByAction(this.CellActionTypes, triggers).Distinct()))
            {
                yield return trigger;
            }
        }

        public IEnumerable<Trigger> FilterUnitTriggers()
        {
            return this.FilterUnitTriggers(this.Triggers);
        }

        public IEnumerable<Trigger> FilterUnitTriggers(IEnumerable<Trigger> triggers)
        {
            foreach (Trigger trigger in FilterTriggersByEvent(this.UnitEventTypes, triggers).Concat(this.FilterTriggersByAction(this.UnitActionTypes, triggers).Distinct()))
            {
                yield return trigger;
            }
        }

        public IEnumerable<Trigger> FilterStructureTriggers()
        {
            return this.FilterStructureTriggers(this.Triggers);
        }

        public IEnumerable<Trigger> FilterStructureTriggers(IEnumerable<Trigger> triggers)
        {
            foreach (Trigger trigger in FilterTriggersByEvent(this.BuildingEventTypes, triggers).Concat(this.FilterTriggersByAction(this.BuildingActionTypes, triggers).Distinct()))
            {
                yield return trigger;
            }
        }

        public IEnumerable<Trigger> FilterTerrainTriggers()
        {
            return this.FilterTerrainTriggers(this.Triggers);
        }

        public IEnumerable<Trigger> FilterTerrainTriggers(IEnumerable<Trigger> triggers)
        {
            foreach (Trigger trigger in FilterTriggersByEvent(this.TerrainEventTypes, triggers).Concat(this.FilterTriggersByAction(this.TerrainActionTypes, triggers).Distinct()))
            {
                yield return trigger;
            }
        }

        public static IEnumerable<Trigger> FilterTriggersByEvent(HashSet<String> allowedEventTypes, IEnumerable<Trigger> triggers)
        {
            foreach (Trigger trig in triggers)
            {
                if (trig.Event1 != null && allowedEventTypes.Contains(trig.Event1.EventType)
                    || ((trig.EventControl == TriggerMultiStyleType.Or || trig.EventControl == TriggerMultiStyleType.And)
                        && trig.Event2 != null && allowedEventTypes.Contains(trig.Event2.EventType)))
                {
                    yield return trig;
                }
            }
        }

        public static String MakeAllowedTriggersToolTip(string[] filteredEvents, string[] filteredActions)
        {
            return MakeAllowedTriggersToolTip(filteredEvents, null, filteredActions, null);
        }

        public static String MakeAllowedTriggersToolTip(string[] filteredEvents, string[] filteredActions, Trigger trigger)
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

        public static String MakeAllowedTriggersToolTip(string[] filteredEvents, String[] indicatedEvents, string[] filteredActions, string[] indicatedActions)
        {
            if (indicatedEvents == null)
            {
                indicatedEvents = new string[0];
            }
            if (indicatedActions == null)
            {
                indicatedActions = new string[0];
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

        public IEnumerable<Trigger> FilterTriggersByAction(HashSet<String> allowedActionTypes, IEnumerable<Trigger> triggers)
        {
            foreach (Trigger trig in triggers)
            {
                if (trig.Action1 != null && allowedActionTypes.Contains(trig.Action1.ActionType)
                    || (trig.Action2 != null && allowedActionTypes.Contains(trig.Action2.ActionType)))
                {
                    yield return trig;
                }
            }
        }

        public IEnumerable<ITechno> GetAllTechnos()
        {
            foreach ((Point location, ICellOccupier occupier) in this.Technos)
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
            return this.GetDefaultMission(techno, this.DefaultMissionArmed);
        }

        public string GetDefaultMission(ITechnoType techno, String currentMission)
        {
            if (techno.IsHarvester)
            {
                return this.DefaultMissionHarvest;
            }
            if (techno.IsAircraft && !techno.IsFixedWing)
            {
                // Ground-landable aircraft. Default order should be 'Unload' to make it land on the spot it spawned on.
                return this.DefaultMissionAircraft;
            }
            if (!techno.IsArmed)
            {
                return this.DefaultMissionUnarmed;
            }
            // Automatically switch from other default missions to the general 'Guard' one, but don't change custom-picked mission like 'Hunt4.
            if (currentMission == this.DefaultMissionHarvest || currentMission == this.DefaultMissionAircraft || currentMission == this.DefaultMissionUnarmed)
            {
                return this.DefaultMissionArmed;
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
            if (this.Metrics.GetLocation(cell, out Point p))
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
                    for (Int32 y = 0; y < ylen; ++y)
                    {
                        for (Int32 x = 0; x < xlen; ++x)
                        {
                            if (y < ylenMask && x < xlenMask && mask[y, x])
                            {
                                if (!this.Metrics.GetCell(new Point(p.X + x, p.Y + y), out int targetCell))
                                {
                                    blockingCell = -1;
                                    placementCell = -1;
                                    return null;
                                }
                                ICellOccupier techno = this.Technos[targetCell];
                                ICellOccupier b = this.Buildings[targetCell];
                                if (techno != null || b != null)
                                {
                                    blockingCell = targetCell;
                                    Point? blockingOrigin = techno != null ? this.Technos[techno] : this.Buildings[b];
                                    placementCell = this.Metrics.GetCell(blockingOrigin.Value).Value;
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
                for (Int32 y = 0; y < ylenOcMask; ++y)
                {
                    for (Int32 x = 0; x < xlenOcMask; ++x)
                    {
                        if (occupyMask[y, x])
                        {
                            if (!this.Metrics.GetCell(new Point(p.X + x, p.Y + y), out int targetCell))
                            {
                                blockingCell = -1;
                                placementCell = -1;
                                return null;
                            }
                            ICellOccupier techno = this.Technos[targetCell];
                            ICellOccupier b = this.Buildings[targetCell];
                            if (techno != null || b != null)
                            {
                                blockingCell = targetCell;
                                Point? blockingOrigin = null;
                                if (b != null)
                                {
                                    blockingOrigin = this.Buildings[b];
                                    onBib = true;
                                }
                                else if (!bibIgnoreCells.Contains(new Point(x, y)))
                                {
                                    // For checking non-building technos on a building's area, ignore the unoccupied bib cells.
                                    blockingOrigin = this.Technos[techno];
                                }
                                if (blockingOrigin.HasValue)
                                {
                                    placementCell = this.Metrics.GetCell(blockingOrigin.Value).Value;
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
            String reportString;
            if (rebuildIndex != null)
            {
                String bibRemark = isbib ? "Bib area of b" : "B";
                reportString = String.Format("{0}ase rebuild entry '{1}', structure '{2}' on cell '{3}'", bibRemark, rebuildIndex, buildingType.Name, cell);
            }
            else
            {
                String bibRemark = isbib ? "Bib area of s" : "S";
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
                    errors.Add(string.Format("{0} overlaps bib area of structure '{1}' placed on cell {2} at cell {3}; skipping.", reportString, building.Type.Name, placementcell, reportCell));
                    modified = true;
                }
                else
                {
                    errors.Add(string.Format("{0} overlaps structure '{1}' placed on cell {2} at cell {3}; skipping.", reportString, building.Type.Name, placementcell, reportCell));
                    modified = true;
                }
            }
            else if (techno is Overlay overlay)
            {
                errors.Add(string.Format("{0} overlaps overlay '{1}' on cell {2}; skipping.", reportString, overlay.Type.Name, reportCell));
                modified = true;
            }
            else if (techno is Terrain terrain)
            {
                errors.Add(string.Format("{0} overlaps terrain '{1}' placed on cell {2} at cell {3}; skipping.", reportString, terrain.Type.Name, placementcell, reportCell));
                modified = true;
            }
            else if (techno is InfantryGroup infantry)
            {
                Infantry inf = infantry.Infantry.FirstOrDefault(u => u != null);
                string infInfo = inf == null ? string.Empty : string.Format(" '{0}'", inf.Type.Name);
                errors.Add(string.Format("{0} overlaps infantry '{1}' on cell {2}; skipping.", reportString, infInfo, reportCell));
                modified = true;
            }
            else if (techno is Unit unit)
            {
                errors.Add(string.Format("{0} overlaps unit '{1}' on cell {2}; skipping.", reportString, unit.Type.Name, reportCell));
                modified = true;
            }
            else
            {
                if (blockingCell != -1)
                {
                    errors.Add(string.Format("{0} overlaps unknown techno on cell {1}; skipping.", reportString, blockingCell));
                    modified = true;
                }
                else
                {
                    errors.Add(string.Format("{0} crosses outside the map bounds; skipping.", reportString));
                    modified = true;
                }
            }
        }


        public TGA GeneratePreview(Size previewSize, IGamePlugin plugin, bool renderAll, bool sharpen)
        {
            MapLayerFlag toRender = MapLayerFlag.Template | (renderAll ? MapLayerFlag.OverlayAll | MapLayerFlag.Smudge | MapLayerFlag.Technos : MapLayerFlag.Resources);
            int?[] backupWps = null;
            if (!this.BasicSection.SoloMission)
            {
                toRender = toRender | MapLayerFlag.Waypoints;
                // Since there's no way to tell the map renderer to only render flag waypoints, we backup and
                // clear all other waypoints before the preview generation, and restore them again afterwards.
                backupWps = new int?[this.Waypoints.Length];
                for (int i = 0; i < this.Waypoints.Length; ++i)
                {
                    // Clear waypoint if not player start.
                    if (this.Waypoints[i].Cell.HasValue && (this.Waypoints[i].Flag & WaypointFlag.PlayerStart) == 0)
                    {
                        backupWps[i] = Waypoints[i].Cell;
                        this.Waypoints[i].Cell = null;
                    }
                }
            }
            try
            {
                return this.GeneratePreview(previewSize, plugin, toRender, true, true, true, sharpen);
            }
            finally
            {
                // Restore waypoints.
                if (backupWps != null)
                {
                    for (int i = 0; i < this.Waypoints.Length; ++i)
                    {
                        if (backupWps[i].HasValue)
                        {
                            this.Waypoints[i].Cell = backupWps[i];
                        }
                    }
                }
            }
        }

        public TGA GeneratePreview(Size previewSize, IGamePlugin plugin, MapLayerFlag toRender, bool clearBackgrround, bool smooth, bool crop, bool sharpen)
        {
            HashSet<Point> locations = this.Metrics.Bounds.Points().ToHashSet();
            Rectangle boundsToUse = crop ? this.Bounds : new Rectangle(Point.Empty, this.Metrics.Size);
            Size originalTileSize = Globals.OriginalTileSize;
            Size renderTileSize = originalTileSize;
            //Size renderTileSize = new Size((int)Math.Round(originalTileSize.Width * tileScale), (int)Math.Round(originalTileSize.Height * tileScale));
            Rectangle mapBounds = new Rectangle(boundsToUse.Left * renderTileSize.Width, boundsToUse.Top * renderTileSize.Height,
                    boundsToUse.Width * renderTileSize.Width, boundsToUse.Height * renderTileSize.Height);
            Single previewScale = Math.Min(previewSize.Width / (float)mapBounds.Width, previewSize.Height / (float)mapBounds.Height);
            Size scaledSize = new Size((int)Math.Round(previewSize.Width / previewScale), (int)Math.Round(previewSize.Height / previewScale));

            using (Bitmap fullBitmap = new Bitmap(this.Metrics.Width * originalTileSize.Width, this.Metrics.Height * originalTileSize.Height))
            using (Bitmap croppedBitmap = new Bitmap(previewSize.Width, previewSize.Height))
            {
                using (Graphics g = Graphics.FromImage(fullBitmap))
                {
                    MapRenderer.SetRenderSettings(g, smooth);
                    MapRenderer.Render(plugin.GameInfo, this, g, locations, toRender, 1);
                    if ((toRender & MapLayerFlag.Indicators) != 0)
                    {
                        ViewTool.PostRenderMap(g, plugin, this, 1, toRender, MapLayerFlag.None, false, plugin.Map.Metrics.Bounds);
                    }
                }
                using (Graphics g = Graphics.FromImage(croppedBitmap))
                {
                    MapRenderer.SetRenderSettings(g, smooth);
                    Matrix transform = new Matrix();
                    transform.Scale(previewScale, previewScale);
                    transform.Translate((scaledSize.Width - mapBounds.Width) / 2, (scaledSize.Height - mapBounds.Height) / 2);
                    g.Transform = transform;
                    if (clearBackgrround)
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

        public TGA GenerateMapPreview(IGamePlugin plugin, bool renderAll)
        {
            return this.GeneratePreview(Globals.MapPreviewSize, plugin, renderAll, false);
        }

        public TGA GenerateWorkshopPreview(IGamePlugin plugin, bool renderAll)
        {
            return this.GeneratePreview(Globals.WorkshopPreviewSize, plugin, renderAll, true);
        }

        object ICloneable.Clone()
        {
            return this.Clone(false);
        }

        private void Overlay_CellChanged(object sender, CellChangedEventArgs<Overlay> e)
        {
            if (e.OldValue != null && (e.OldValue.Type.IsWall || e.OldValue.Type.IsSolid))
            {
                this.Buildings.Remove(e.OldValue);
            }
            if (e.Value != null && (e.Value.Type.IsWall || e.Value.Type.IsSolid))
            {
                this.Buildings.Add(e.Location, e.Value);
            }
            if (e.Value?.Type.IsConcrete ?? false)
            {
                //this.UpdateConcreteOverlays(Rectangle.Inflate(new Rectangle(e.Location, new Size(1, 1)), 1, 1).Points().ToHashSet());
            }
            if (this.updating)
            {
                return;
            }
            foreach (Overlay overlay in new Overlay[] { e.OldValue, e.Value })
            {
                if (overlay == null)
                {
                    continue;
                }
                MapLayerFlag layer;
                if (overlay.Type.IsResource)
                {
                    layer = MapLayerFlag.Resources;
                }
                else if (overlay.Type.IsWall)
                {
                    layer = MapLayerFlag.Walls;
                }
                else if (overlay.Type.IsConcrete)
                {
                    layer = MapLayerFlag.Overlay;
                }
                else
                {
                    continue;
                }
                if (!this.invalidateLayers.TryGetValue(layer, out ISet<Point> locations))
                {
                    locations = new HashSet<Point>();
                    this.invalidateLayers[layer] = locations;
                }
                locations.UnionWith(Rectangle.Inflate(new Rectangle(e.Location, new Size(1, 1)), 1, 1).Points());
            }
            if (this.updateCount == 0)
            {
                this.Update();
            }
        }

        private void Technos_OccupierAdded(object sender, OccupierAddedEventArgs<ICellOccupier> e)
        {
            if (e.Occupier is ICellOverlapper overlapper)
            {
                if (this.updateCount == 0)
                {
                    this.Overlappers.Add(e.Location, overlapper);
                }
                else
                {
                    this.invalidateOverlappers = true;
                }
            }
        }

        private void Technos_OccupierRemoved(object sender, OccupierRemovedEventArgs<ICellOccupier> e)
        {
            if (e.Occupier is ICellOverlapper overlapper)
            {
                if (this.updateCount == 0)
                {
                    this.Overlappers.Remove(overlapper);
                }
                else
                {
                    this.invalidateOverlappers = true;
                }
            }
        }

        private void Buildings_OccupierAdded(object sender, OccupierAddedEventArgs<ICellOccupier> e)
        {
            if (e.Occupier is Building building)
            {
                this.Technos.Add(e.Location, e.Occupier, building.Type.BaseOccupyMask);
                this.AddBibs(e.Location, building);
                if (building.Type.IsWall)
                {
                    Rectangle toRefresh = new Rectangle(e.Location, building.Type.OverlapBounds.Size);
                    toRefresh.Inflate(1, 1);
                    this.UpdateWallOverlays(toRefresh.Points().ToHashSet());
                }
            }
            else
            {
                this.Technos.Add(e.Location, e.Occupier);
            }
        }

        private void Buildings_OccupierRemoved(object sender, OccupierRemovedEventArgs<ICellOccupier> e)
        {
            if (e.Occupier is Building building)
            {
                this.RemoveBibs(building);
                if (building.Type.IsWall)
                {
                    Rectangle toRefresh = new Rectangle(e.Location, building.Type.OverlapBounds.Size);
                    toRefresh.Inflate(1, 1);
                    this.UpdateWallOverlays(toRefresh.Points().ToHashSet());
                }
            }
            this.Technos.Remove(e.Occupier);
        }

        public void UpdateWaypoints()
        {
            bool isSolo = this.BasicSection.SoloMission;
            HashSet<Point> updated = new HashSet<Point>();
            for (Int32 i = 0; i < this.Waypoints.Length; ++i)
            {
                Waypoint waypoint = this.Waypoints[i];
                if ((waypoint.Flag & WaypointFlag.PlayerStart) == WaypointFlag.PlayerStart)
                {
                    this.Waypoints[i].Name = isSolo ? i.ToString() : string.Format("P{0}", i);
                    if (waypoint.Point.HasValue)
                    {
                        updated.Add(waypoint.Point.Value);
                    }
                }
            }
            this.NotifyWaypointsUpdate();
            this.NotifyMapContentsChanged(updated);
        }

        public bool RemoveExpansionUnits()
        {
            HashSet<Point> refreshPoints = new HashSet<Point>();
            bool changed = false;
            if (this.BasicSection.ExpansionEnabled)
            {
                // Expansion is enabled. Nothing to do.
                return false;
            }
            // Technos on map
            List<(Point, ICellOccupier)> toDelete = new List<(Point, ICellOccupier)>();
            foreach ((Point p, ICellOccupier occup) in this.Technos)
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
                    Rectangle? refreshArea = this.Overlappers[un];
                    if (refreshArea.HasValue)
                    {
                        refreshPoints.UnionWith(refreshArea.Value.Points());
                    }
                    //mapPanel.Invalidate(map, un);
                    this.Technos.Remove(occup);
                    changed = true;
                }
                else if (occup is InfantryGroup infantryGroup)
                {
                    Infantry[] inf = infantryGroup.Infantry;
                    for (int i = 0; i < inf.Length; ++i)
                    {
                        if (inf[i] != null && (inf[i].Type.Flag & UnitTypeFlag.IsExpansionUnit) == UnitTypeFlag.IsExpansionUnit)
                        {
                            inf[i] = null;
                            changed = true;
                        }
                    }
                    bool delGroup = inf.All(i => i == null);
                    Rectangle? refreshArea = this.Overlappers[infantryGroup];
                    if (refreshArea.HasValue)
                    {
                        refreshPoints.UnionWith(refreshArea.Value.Points());
                    }
                    //mapPanel.Invalidate(map, infantryGroup);
                    if (delGroup)
                    {
                        this.Technos.Remove(infantryGroup);
                    }
                }
            }
            // Teamtypes
            foreach (TeamType teamtype in this.TeamTypes)
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
            this.NotifyMapContentsChanged(refreshPoints);
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
        public void ApplyTriggerChanges(List<(String Name1, String Name2)> renameActions, out Dictionary<object, string> undoList, out Dictionary<object, string> redoList, out Dictionary<CellTrigger, int> cellTriggerLocations, List<Trigger> newTriggers)
        {
            undoList = new Dictionary<object, string>();
            redoList = new Dictionary<object, string>();
            cellTriggerLocations = new Dictionary<CellTrigger, int>();
            foreach ((String name1, String name2) in renameActions)
            {
                if (Trigger.IsEmpty(name1))
                {
                    continue;
                }
                foreach ((Point location, Building building) in this.Buildings.OfType<Building>().Where(x => x.Occupier.IsPrebuilt))
                {
                    if (String.Equals(building.Trigger, name1, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!undoList.ContainsKey(building))
                            undoList[building] = building.Trigger;
                        redoList[building] = name2;
                        building.Trigger = name2;
                    }
                }
                foreach (ITechno techno in this.GetAllTechnos())
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
                foreach (TeamType team in this.TeamTypes)
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
                foreach ((int cell, CellTrigger value) in this.CellTriggers)
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
            this.CleanUpTriggers(newTriggers, undoList, redoList, cellTriggerLocations);
        }

        private void CleanUpTriggers(List<Trigger> triggers, Dictionary<object, string> undoList, Dictionary<object, string> redoList, Dictionary<CellTrigger, int> cellTriggerLocations)
        {
            // Clean techno types
            HashSet<string> availableTriggers = triggers.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            HashSet<string> availableUnitTriggers = this.FilterUnitTriggers(triggers).Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            HashSet<string> availableBuildingTriggers = this.FilterStructureTriggers(triggers).Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            HashSet<string> availableTerrainTriggers = this.FilterTerrainTriggers(triggers).Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (ITechno techno in this.GetAllTechnos())
            {
                if (techno is Infantry infantry)
                {
                    this.CheckTechnoTrigger(infantry, availableUnitTriggers, undoList, redoList);
                }
                else if (techno is Unit unit)
                {
                    this.CheckTechnoTrigger(unit, availableUnitTriggers, undoList, redoList);
                }
                else if (techno is Building building)
                {
                    this.CheckTechnoTrigger(building, availableBuildingTriggers, undoList, redoList);
                }
                else if (techno is Terrain terrain)
                {
                    this.CheckTechnoTrigger(terrain, availableTerrainTriggers, undoList, redoList);
                }
            }
            // Clean teamtypes
            foreach (TeamType team in this.TeamTypes)
            {
                String trig = team.Trigger;
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
            this.CleanUpCellTriggers(triggers, undoList, redoList, cellTriggerLocations);
        }

        private void CheckTechnoTrigger(ITechno techno, HashSet<String> availableTriggers, Dictionary<object, string> undoList, Dictionary<object, string> redoList)
        {
            String trig = techno.Trigger;
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
            HashSet<string> placeableTrigs = this.FilterCellTriggers(triggers).Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            List<int> cellsToClear = new List<int>();
            foreach ((int cell, CellTrigger value) in this.CellTriggers)
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
                this.CellTriggers[cellsToClear[i]] = null;
            }
        }

        public void ApplyTeamTypeRenames(List<(String Name1, String Name2)> renameActions)
        {
            foreach ((String name1, String name2) in renameActions)
            {
                if (TeamType.IsEmpty(name1))
                {
                    continue;
                }
                foreach (Trigger trigger in this.triggers)
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
            Dictionary<String, int[]> powerWithUnbuilt = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
            Dictionary<String, int[]> powerWithoutUnbuilt = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
            foreach (HouseType house in this.HouseTypes)
            {
                if (housesWithProd.Contains(house.Name))
                {
                    powerWithUnbuilt[house.Name] = new int[3];
                }
                powerWithoutUnbuilt[house.Name] = new int[3];
            }
            HashSet<String> hasDamagedPowerPlants = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach ((_, Building bld) in this.Buildings.OfType<Building>())
            {
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
                        if (powerWithUnbuilt.TryGetValue(house, out housePwr))
                        {
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
            foreach (HouseType house in this.HouseTypes)
            {
                if (housesWithProd.Contains(house.Name))
                {
                    prodHouses.Add(house.Name);
                }
            }
            info.Add("Production-capable Houses: " + (prodHouses.Count == 0 ? "None" : String.Join(", ", prodHouses.ToArray())));
            foreach (HouseType house in this.HouseTypes)
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
                    bool hasUnbuilt = powerWithUnbuilt.TryGetValue(house.Name, out housePwrAll);
                    if (hasUnbuilt)
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
                    if (hasDamaged && !hasUnbuilt) houseInfo.Append("Has damaged power plants. ");
                    houseInfo.Append("Produces ").Append(houseProdBuilt);
                    if (hasDamaged)
                        houseInfo.Append(" currently; ").Append(houseProdBuiltHealthy).Append(" at full strength");
                    houseInfo.Append(", uses ").Append(houseUsageBuilt).Append(".");
                    info.Add(houseInfo.ToString());
                }
            }
            return info;
        }

        public IEnumerable<string> AssessStorage(HashSet<string> housesWithProd)
        {
            Dictionary<String, int> storageWithUnbuilt = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            Dictionary<String, int> storageWithoutUnbuilt = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (HouseType house in this.HouseTypes)
            {
                if (housesWithProd.Contains(house.Name))
                {
                    storageWithUnbuilt[house.Name] = 0;
                }
                storageWithoutUnbuilt[house.Name] = 0;
            }
            foreach ((_, Building bld) in this.Buildings.OfType<Building>())
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
            foreach (HouseType house in this.HouseTypes)
            {
                if (housesWithProd.Contains(house.Name))
                {
                    prodHouses.Add(house.Name);
                }
            }
            info.Add("Production-capable Houses: " + (prodHouses.Count == 0 ? "None" : String.Join(", ", prodHouses.ToArray())));
            foreach (HouseType house in this.HouseTypes)
            {
                int houseStorageBuilt;
                if (storageWithUnbuilt.TryGetValue(house.Name, out int houseStorageAll))
                {
                    storageWithoutUnbuilt.TryGetValue(house.Name, out houseStorageBuilt);

                    String houseInfo = String.Format("{0}: Storage capacity: {1}. (Without unbuilt: {2})",
                        house.Name, houseStorageAll, houseStorageBuilt);
                    info.Add(houseInfo);
                }
                else if (storageWithoutUnbuilt.TryGetValue(house.Name, out houseStorageBuilt))
                {
                    String houseInfo = String.Format("{0}: Storage capacity: {1}.",
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
            foreach (ITechnoType technoType in this.AllTeamTechnoTypes)
            {
                // units, boats, aircraft, infantry
                technoType.Reset();
            }
            // probably not needed since it's in the team techno types.
            foreach (UnitType unitType in this.AllUnitTypes)
            {
                unitType.Reset();
            }
            // probably not needed since it's in the team techno types.
            foreach (InfantryType infantryType in this.AllInfantryTypes)
            {
                infantryType.Reset();
            }
            foreach (BuildingType buildingType in this.BuildingTypes)
            {
                buildingType.Reset();
            }
            foreach (TemplateType template in this.TemplateTypes)
            {
                template.Reset();
            }
            foreach (TerrainType terrainType in this.TerrainTypes)
            {
                terrainType.Reset();
            }
            foreach (OverlayType overlayType in this.OverlayTypes)
            {
                overlayType.Reset();
            }
            foreach (SmudgeType smudgeType in this.SmudgeTypes)
            {
                smudgeType.Reset();
            }
        }
    }
}
