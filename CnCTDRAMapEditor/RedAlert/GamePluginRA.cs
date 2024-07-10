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
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TGASharpLib;

namespace MobiusEditor.RedAlert
{
    class GamePluginRA : IGamePlugin
    {
        protected const int multiStartPoints = 8;
        protected const int totalNumberedPoints = 98;

        private readonly IEnumerable<string> movieTypes;
        private bool isLoading = false;

        private static readonly Regex singlePlayRegex = new Regex("^SC[A-LN-Z]\\d{2}[EWX][A-EL]$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected static readonly Regex baseKeyRegex = new Regex("^\\d{3}$", RegexOptions.Compiled);
        private readonly GameInfoRedAlert gameTypeInfo = new GameInfoRedAlert();

        private const string movieEmpty = "<none>";
        private const string remarkOld = " (Classic only)";
        private const string remarkNew = " (Remaster only)";

        private static readonly IEnumerable<string> movieTypesRemarksOld = new string[]
        {
            "SHIPYARD", // MISSING
            "ENGLISH",  // High res. Intro
            "SIZZLE",   // MISSING
            "SIZZLE2",  // MISSING
            "TRAILER"   // MISSING
        };

        private static readonly IEnumerable<string> movieTypesRemarksNew = new string[]
        {
            "RETALIATION_ALLIED1",
            "RETALIATION_ALLIED2",
            "RETALIATION_ALLIED3",
            "RETALIATION_ALLIED4",
            "RETALIATION_ALLIED5",
            "RETALIATION_ALLIED6",
            "RETALIATION_ALLIED7",
            "RETALIATION_ALLIED8",
            "RETALIATION_ALLIED9",
            "RETALIATION_ALLIED10",
            "RETALIATION_SOVIET1",
            "RETALIATION_SOVIET2",
            "RETALIATION_SOVIET3",
            "RETALIATION_SOVIET4",
            "RETALIATION_SOVIET5",
            "RETALIATION_SOVIET6",
            "RETALIATION_SOVIET7",
            "RETALIATION_SOVIET8",
            "RETALIATION_SOVIET9",
            "RETALIATION_SOVIET10",
            "RETALIATION_WINA",
            "RETALIATION_WINS",
            "RETALIATION_ANTS"
        };

        private static readonly IEnumerable<string> movieTypesRa = new string[]
        {
            "AAGUN",
            "MIG",
            "SFROZEN",
            "AIRFIELD",
            "BATTLE",
            "BMAP",
            "BOMBRUN",
            "DPTHCHRG",
            "GRVESTNE",
            "MONTPASS",
            "MTNKFACT",
            "CRONTEST",
            "OILDRUM",
            "ALLIEND",
            "RADRRAID",
            "SHIPYARD", // MISSING
            "SHORBOMB",
            "SITDUCK",
            "SLNTSRVC",
            "SNOWBASE",
            "EXECUTE",
            "REDINTRO", // low res.
            "NUKESTOK",
            "V2ROCKET",
            "SEARCH",
            "BINOC",
            "ELEVATOR",
            "FROZEN",
            "MCV",
            "SHIPSINK",
            "SOVMCV",
            "TRINITY",
            "ALLYMORF",
            "APCESCPE",
            "BRDGTILT",
            "CRONFAIL",
            "STRAFE",
            "DESTROYR",
            "DOUBLE",
            "FLARE",
            "SNSTRAFE",
            "LANDING",
            "ONTHPRWL",
            "OVERRUN",
            "SNOWBOMB",
            "SOVCEMET",
            "TAKE_OFF",
            "TESLA",
            "SOVIET8",
            "SPOTTER",
            "ALLY1",
            "ALLY2",
            "ALLY4",
            "SOVFINAL",
            "ASSESS",
            "SOVIET10",
            "DUD",
            "MCV_LAND",
            "MCVBRDGE",
            "PERISCOP",
            "SHORBOM1",
            "SHORBOM2",
            "SOVBATL",
            "SOVTSTAR",
            "AFTRMATH",
            "SOVIET11",
            "MASASSLT",
            "ENGLISH",  // High res. Intro
            "SOVIET1",
            "SOVIET2",
            "SOVIET3",
            "SOVIET4",
            "SOVIET5",
            "SOVIET6",
            "SOVIET7",
            "PROLOG",
            "AVERTED",
            "COUNTDWN",
            "MOVINGIN",
            "ALLY10",
            "ALLY12",
            "ALLY5",
            "ALLY6",
            "ALLY8",
            "TANYA1",
            "TANYA2",
            "ALLY10B",
            "ALLY11",
            "ALLY14",
            "ALLY9",
            "SPY",
            "TOOFAR",
            "SOVIET12",
            "SOVIET13",
            "SOVIET9",
            "BEACHEAD",
            "SOVIET14",
            "SIZZLE",   // MISSING
            "SIZZLE2",  // MISSING
            // "TRAILER",  // MISSING
            "ANTEND",
            "ANTINTRO",
            //2019/11/12 JAS - Added for Retaliation movies
            "RETALIATION_ALLIED1",
            "RETALIATION_ALLIED2",
            "RETALIATION_ALLIED3",
            "RETALIATION_ALLIED4",
            "RETALIATION_ALLIED5",
            "RETALIATION_ALLIED6",
            "RETALIATION_ALLIED7",
            "RETALIATION_ALLIED8",
            "RETALIATION_ALLIED9",
            "RETALIATION_ALLIED10",
            "RETALIATION_SOVIET1",
            "RETALIATION_SOVIET2",
            "RETALIATION_SOVIET3",
            "RETALIATION_SOVIET4",
            "RETALIATION_SOVIET5",
            "RETALIATION_SOVIET6",
            "RETALIATION_SOVIET7",
            "RETALIATION_SOVIET8",
            "RETALIATION_SOVIET9",
            "RETALIATION_SOVIET10",
            "RETALIATION_WINA",
            "RETALIATION_WINS",
            "RETALIATION_ANTS"
        };

        private const string themeEmpty = "No Theme";

        private static readonly IEnumerable<string> themeTypes = new string[]
        {
            "BIGF226M",
            "CRUS226M",
            "FAC1226M",
            "FAC2226M",
            "HELL226M",
            "RUN1226M",
            "SMSH226M",
            "TREN226M",
            "WORK226M",
            "AWAIT",
            "DENSE_R",
            "FOGGER1A",
            "MUD1A",
            "RADIO2",
            "ROLLOUT",
            "SNAKE",
            "TERMINAT",
            "TWIN",
            "VECTOR1A",
            "MAP",
            "SCORE",
            "INTRO",
            "CREDITS",
            "2ND_HAND",
            "ARAZOID",
            "BACKSTAB",
            "CHAOS2",
            "SHUT_IT",
            "TWINMIX1",
            "UNDER3",
            "VR2",
            "BOG",
            "FLOAT_V2",
            "GLOOM",
            "GRNDWIRE",
            "RPT",
            "SEARCH",
            "TRACTION",
            "WASTELND"
        };

        public static IEnumerable<string> Movies => movieTypesRa;
        public static IEnumerable<string> MoviesClassic => movieTypesRa.Where(mv => !movieTypesRemarksNew.Contains(mv));
        public static IEnumerable<string> Themes => themeTypes;

        private static readonly IEnumerable<ITechnoType> fullTechnoTypes;

        public GameInfo GameInfo => gameTypeInfo;
        public bool IsMegaMap => true;
        public HouseType ActiveHouse { get; set; }
        public Map Map { get; }
        public Image MapImage { get; private set; }

        private IFeedBackHandler feedBackHandler;
        public IFeedBackHandler FeedBackHandler
        {
            get { return feedBackHandler; }
            set { feedBackHandler = value; }
        }

        bool isDirty;
        public bool Dirty
        {
            get { return isDirty; }
            set
            {
                isDirty = value;
                feedBackHandler?.UpdateStatus();
            }
        }

        private INISectionCollection extraSections;
        public string GetExtraIniText()
        {
            INI extraTextIni = new INI();
            if (extraSections != null)
            {
                extraTextIni.Sections.AddRange(extraSections);
            }
            return extraTextIni.ToString();
        }

        public IEnumerable<string> SetExtraIniText(string extraIniText, out bool footPrintsChanged)
        {
            return SetExtraIniText(extraIniText, this.Map.BasicSection.SoloMission, this.Map.BasicSection.ExpansionEnabled, false, out footPrintsChanged);
        }

        public IEnumerable<string> TestSetExtraIniText(string extraIniText, bool isSolo, bool expansionEnabled, out bool footPrintsChanged)
        {
            return SetExtraIniText(extraIniText, isSolo, expansionEnabled, true, out footPrintsChanged);
        }

        public List<string> SetExtraIniText(string extraIniText, bool isSolo, bool expansionEnabled, bool forFootprintTest, out bool footPrintsChanged)
        {
            INI extraTextIni = new INI();
            try
            {
                extraTextIni.Parse(extraIniText ?? String.Empty);
            }
            catch
            {
                footPrintsChanged = false;
                return null;
            }
            List<string> errors = ResetMissionRules(extraTextIni, isSolo, expansionEnabled, forFootprintTest, out footPrintsChanged);
            if (!forFootprintTest)
            {
                extraSections = extraTextIni.Sections.Count == 0 ? null : extraTextIni.Sections;
            }
            return errors;
        }

        private void SetMissionRules(INI iniText, bool isSolo, bool expansionEnabled, List<string> errors, ref bool modified)
        {
            if (this.rulesIni != null)
            {
                UpdateRules(rulesIni, this.Map, false);
            }
            if (expansionEnabled)
            {
                if (this.aftermathRulesIni != null)
                {
                    UpdateRules(aftermathRulesIni, this.Map, false);
                }
                if (this.aftermathMpRulesIni != null && !isSolo)
                {
                    UpdateRules(aftermathMpRulesIni, this.Map, false);
                }
            }
            List<string> newErrors = UpdateRules(iniText, this.Map, false);
            if (newErrors.Count > 0)
            {
                modified = true;
                errors.AddRange(newErrors);
            }
        }

        /// <summary>
        /// Trims the given extra ini content to just unmanaged information,
        /// resets the plugin's rules to their defaults, and then applies any
        /// rules in the given extra ini content to the plugin.
        /// </summary>
        /// <param name="extraIniText">Ini content that remains after parsing an ini file. If null, only a rules reset is performed.</param>
        /// <param name="footPrintsChanged">Returns true if any building footprints were changed as a result of the ini rule changes.</param>
        /// <returns>Any errors in parsing the <paramref name="extraIniText"/> contents.</returns>
        private List<string> ResetMissionRules(INI extraIniText)
        {
            return ResetMissionRules(extraIniText, this.Map.BasicSection.SoloMission, this.Map.BasicSection.ExpansionEnabled, false, out _);
        }

        /// <summary>
        /// Trims the given extra ini content to just unmanaged information,
        /// resets the plugin's rules to their defaults, and then applies any
        /// rules in the given extra ini content to the plugin.
        /// </summary>
        /// <param name="extraIniText">Ini content that remains after parsing an ini file. If null, only a rules reset is performed.</param>
        /// <param name="isSolo">True if this operation should consider this as singleplayer mission.</param>
        /// <param name="expansionEnabled">True if this operation should consider expansions to be enabled.</param>
        /// <param name="forFootprintTest">Don't apply changes, just test the result for <paramref name="footPrintsChanged"/></param>
        /// <param name="footPrintsChanged">Returns true if any building footprints were changed as a result of the ini rule changes.</param>
        /// <returns>Any errors in parsing the <paramref name="extraIniText"/> contents.</returns>
        private List<string> ResetMissionRules(INI extraIniText, bool isSolo, bool expansionEnabled, bool forFootprintTest, out bool footPrintsChanged)
        {
            if (extraIniText != null && !forFootprintTest)
            {
                CleanIniContents(extraIniText);
            }
            Dictionary<string, bool> bibBackups = Map.BuildingTypes.ToDictionary(b => b.Name, b => b.HasBib, StringComparer.OrdinalIgnoreCase);
            if (this.rulesIni != null)
            {
                UpdateRules(rulesIni, this.Map, forFootprintTest);
            }
            if (expansionEnabled)
            {
                if (this.aftermathRulesIni != null)
                {
                    UpdateRules(aftermathRulesIni, this.Map, forFootprintTest);
                }
                if (this.aftermathMpRulesIni != null && !isSolo)
                {
                    UpdateRules(aftermathMpRulesIni, this.Map, forFootprintTest);
                }
            }
            List<string> errors = null;
            if (extraIniText != null)
            {
                errors = UpdateRules(extraIniText, this.Map, forFootprintTest);
            }
            footPrintsChanged = false;
            foreach (BuildingType bType in Map.BuildingTypes)
            {
                if (bibBackups.TryGetValue(bType.Name, out bool bTypeHadBib))
                {
                    bool bibChanged = bType.HasBib != bTypeHadBib;
                    footPrintsChanged |= bibChanged;
                    if (forFootprintTest && bibChanged)
                    {
                        // Restore old value. Test mode will make sure nothing on the map changed.
                        bType.HasBib = bTypeHadBib;
                    }
                }
            }
            return errors;
        }

        private void CleanIniContents(INI extraIniText)
        {
            // Strip "NewUnitsEnabled" from the Aftermath section.
            INISection amSection = extraIniText.Sections["Aftermath"];
            if (amSection != null)
            {
                amSection.Remove("NewUnitsEnabled");
            }
            // Remove any sections known and handled / disallowed by the editor.
            extraIniText.Sections.Remove("Digest");
            INITools.ClearDataFrom(extraIniText, "Basic", (BasicSection)Map.BasicSection);
            INITools.ClearDataFrom(extraIniText, "Map", Map.MapSection);
            extraIniText.Sections.Remove("Steam");
            extraIniText.Sections.Remove("TeamTypes");
            extraIniText.Sections.Remove("Trigs");
            extraIniText.Sections.Remove("MapPack");
            extraIniText.Sections.Remove("Terrain");
            extraIniText.Sections.Remove("OverlayPack");
            extraIniText.Sections.Remove("Smudge");
            extraIniText.Sections.Remove("Units");
            extraIniText.Sections.Remove("Aircraft");
            extraIniText.Sections.Remove("Ships");
            extraIniText.Sections.Remove("Infantry");
            extraIniText.Sections.Remove("Structures");
            if (extraIniText.Sections["Base"] is INISection baseSec)
            {
                CleanBaseSection(extraIniText, baseSec);
            }
            extraIniText.Sections.Remove("Waypoints");
            extraIniText.Sections.Remove("CellTriggers");
            if (extraIniText.Sections["Briefing"] is INISection briefSec)
            {
                briefSec.Remove("Text");
                briefSec.RemoveWhere(k => Regex.IsMatch(k, "^\\d+$"));
                if (briefSec.Count == 0)
                {
                    extraIniText.Sections.Remove(briefSec.Name);
                }
            }
            foreach (House house in Map.Houses)
            {
                INITools.ClearDataFrom(extraIniText, house.Type.Name, house);
            }
        }

        private INI rulesIni;
        private INI aftermathRulesIni;
        private INI aftermathMpRulesIni;

        // Any time a new plugin is made it starts with these defaults. They are then further adapted by the rule reads.
        private readonly LandIniSection LandClear = new LandIniSection(90, 80, 60, 00, true);
        private readonly LandIniSection LandRough = new LandIniSection(80, 70, 40, 00, false);
        private readonly LandIniSection LandRoad = new LandIniSection(100, 100, 100, 00, true);
        private readonly LandIniSection LandWater = new LandIniSection(00, 00, 00, 100, false);
        private readonly LandIniSection LandRock = new LandIniSection(00, 00, 00, 00, false);
        private readonly LandIniSection LandBeach = new LandIniSection(80, 70, 40, 00, false);
        private readonly LandIniSection LandRiver = new LandIniSection(00, 00, 00, 00, false);

        public static bool CheckForRAMap(INI contents)
        {
            return INITools.CheckForIniInfo(contents, "MapPack");
        }

        static GamePluginRA()
        {
            fullTechnoTypes = InfantryTypes.GetTypes().Cast<ITechnoType>().Concat(UnitTypes.GetTypes(false).Cast<ITechnoType>());
        }

        public IEnumerable<string> Initialize()
        {
            List<string> errors = new List<string>();
            // This returns errors in original rules files. Ignore for now.
            this.rulesIni = ReadRulesFile(Globals.TheArchiveManager.ReadFileClassic("rules.ini"));
            errors.AddRange(UpdateRules(rulesIni, this.Map, false));
            this.aftermathRulesIni = ReadRulesFile(Globals.TheArchiveManager.ReadFileClassic("aftrmath.ini"));
            if (this.Map.BasicSection.ExpansionEnabled && this.aftermathRulesIni != null)
            {
                errors.AddRange(UpdateRules(aftermathRulesIni, this.Map, false));
            }
            this.aftermathMpRulesIni = ReadRulesFile(Globals.TheArchiveManager.ReadFileClassic("mplayer.ini"));
            if (this.aftermathMpRulesIni != null && this.Map.BasicSection.ExpansionEnabled && !this.Map.BasicSection.SoloMission)
            {
                errors.AddRange(UpdateRules(aftermathMpRulesIni, this.Map, false));
            }
            // Only one will be found.
            Globals.TheTeamColorManager.Load(@"DATA\XML\CNCRATEAMCOLORS.XML");
            Globals.TheTeamColorManager.Load("palette.cps");
            AddTeamColorsRA(Globals.TheTeamColorManager);
            return errors;
        }

        private static void AddTeamColorsRA(ITeamColorManager teamColorManager)
        {
            // Only applicable for Remastered colors since I can't control those.
            if (teamColorManager is TeamColorManager tcm)
            {
                // "Neutral" in RA colors seems broken; makes stuff black, so remove it.
                tcm.RemoveTeamColor("NEUTRAL");
                TeamColor colSpain = tcm.GetItem("SPAIN");
                if (colSpain != null)
                {
                    // Special. Technically color "JP" exists for this, but it's wrong. Clone Spain instead.
                    TeamColor teamColorSpecial = new TeamColor(tcm);
                    teamColorSpecial.Load(colSpain, "SPECIAL");
                    tcm.AddTeamColor(teamColorSpecial);
                }
            }
        }

        public GamePluginRA()
            : this(true)
        {
        }

        public GamePluginRA(bool mapImage)
        {
            IEnumerable<Waypoint> playerWaypoints = Enumerable.Range(0, multiStartPoints).Select(i => new Waypoint(string.Format("P{0}", i), Waypoint.GetFlagForMpId(i)));
            IEnumerable<Waypoint> generalWaypoints = Enumerable.Range(multiStartPoints, totalNumberedPoints - multiStartPoints).Select(i => new Waypoint(i.ToString()));
            Waypoint[] specialWaypoints = new Waypoint[] { new Waypoint("Home", WaypointFlag.Home), new Waypoint("Reinf.", "Rnf.", WaypointFlag.Reinforce), new Waypoint("Special", "Spc.", WaypointFlag.Special) };
            IEnumerable<Waypoint> waypoints = playerWaypoints.Concat(generalWaypoints).Concat(specialWaypoints);
            // Do not load these from the .meg archive; RA movies list is 100% fixed.
            List<string> movies = new List<string>(movieTypesRa);
            for (int i = 0; i < movies.Count; ++i)
            {
                string vidName = GeneralUtils.AddRemarks(movies[i], movieEmpty, true, movieTypesRemarksOld, remarkOld, out bool changed);
                // Only add one remark.
                if (!changed)
                {
                    vidName = GeneralUtils.AddRemarks(movies[i], movieEmpty, true, movieTypesRemarksNew, remarkNew);
                }
                movies[i] = vidName;
            }
            movies.Insert(0, movieEmpty);
            movieTypes = movies.ToArray();
            BasicSection basicSection = new BasicSection();
            basicSection.SetDefault();
            IEnumerable<HouseType> houseTypes = HouseTypes.GetTypes();
            basicSection.Player = houseTypes.Where(ht => !ht.Flags.HasFlag(HouseTypeFlag.Special)).First().Name;
            basicSection.BasePlayer = HouseTypes.GetClassicOpposingPlayer(basicSection.Player);
            string[] cellEventTypes =
            {
                EventTypes.TEVENT_CROSS_HORIZONTAL,
                EventTypes.TEVENT_CROSS_VERTICAL,
                EventTypes.TEVENT_ENTERS_ZONE,
                EventTypes.TEVENT_PLAYER_ENTERED,
                EventTypes.TEVENT_DISCOVERED,
                EventTypes.TEVENT_NONE
            };
            string[] unitEventTypes =
            {
                EventTypes.TEVENT_DISCOVERED,
                EventTypes.TEVENT_DESTROYED,
                EventTypes.TEVENT_ATTACKED,
                EventTypes.TEVENT_ANY,
                EventTypes.TEVENT_NONE
            };
            string[] structureEventTypes = unitEventTypes.Concat(new[]
            {
                EventTypes.TEVENT_PLAYER_ENTERED,
                EventTypes.TEVENT_SPIED,
            }).ToArray();
            string[] terrainEventTypes = { };
            string[] cellActionTypes = { ActionTypes.TACTION_DESTROY_OBJECT };
            string[] unitActionTypes = { ActionTypes.TACTION_DESTROY_OBJECT };
            string[] structureActionTypes = { ActionTypes.TACTION_DESTROY_OBJECT };
            string[] terrainActionTypes = { };

            // Remap classic Einstein DOS graphics to no longer look like Mobius.
            InfantryTypes.Einstein.ClassicGraphicsRemap = Globals.FixClassicEinstein ? InfantryClassicRemap.RemapEinstein : null;

            Map = new Map(basicSection, null, Constants.MaxSize, typeof(House), houseTypes, null,
                TheaterTypes.GetTypes(), TemplateTypes.GetTypes(),
                TerrainTypes.GetTypes(), OverlayTypes.GetTypes(), SmudgeTypes.GetTypes(Globals.ConvertCraters),
                EventTypes.GetTypes(), cellEventTypes, unitEventTypes, structureEventTypes, terrainEventTypes,
                ActionTypes.GetTypes(), cellActionTypes, unitActionTypes, structureActionTypes, terrainActionTypes,
                MissionTypes.GetTypes(), MissionTypes.MISSION_GUARD, MissionTypes.MISSION_STOP, MissionTypes.MISSION_HARVEST,
                MissionTypes.MISSION_UNLOAD, DirectionTypes.GetMainTypes(), DirectionTypes.GetAllTypes(), InfantryTypes.GetTypes(),
                UnitTypes.GetTypes(Globals.DisableAirUnits), BuildingTypes.GetTypes(), TeamMissionTypes.GetTypes(),
                fullTechnoTypes, waypoints, movieTypes, movieEmpty, themeEmpty.Yield().Concat(themeTypes), themeEmpty,
                Constants.DefaultDropZoneRadius, Constants.DefaultGapRadius, Constants.DefaultJamRadius, Constants.DefaultGoldValue, Constants.DefaultGemValue);
            Map.BasicSection.PropertyChanged += BasicSection_PropertyChanged;
            Map.MapSection.PropertyChanged += MapSection_PropertyChanged;
            if (mapImage)
            {
                Bitmap mapImg = new Bitmap(Map.Metrics.Width * Globals.MapTileWidth, Map.Metrics.Height * Globals.MapTileHeight);
                mapImg.SetResolution(96, 96);
                mapImg.RemoveAlphaOnCurrent();
                MapImage = mapImg;
            }
        }

        public void New(string theater)
        {
            try
            {
                isLoading = true;
                Map.Theater = Map.TheaterTypes.Where(t => t.Equals(theater)).FirstOrDefault() ?? Map.TheaterTypes.FirstOrDefault() ?? TheaterTypes.Temperate;
                Map.TopLeft = new Point(1, 1);
                Map.Size = Map.Metrics.Size - new Size(2, 2);
                Map.BasicSection.Player = Map.HouseTypes.FirstOrDefault()?.Name;
                Map.BasicSection.Name = Constants.EmptyMapName;
                UpdateBasePlayerHouse();
                // Initialises rules.
                ResetMissionRules(null);
            }
            finally
            {
                isLoading = false;
            }
        }

        public IEnumerable<string> Load(string path, FileType fileType)
        {
            bool modified = false;
            try
            {
                isLoading = true;
                List<string> errors = new List<string>();
                bool tryCheckSingle = false;
                byte[] iniBytes;
                INI ini = new INI();
                switch (fileType)
                {
                    case FileType.INI:
                    case FileType.BIN:
                        {
                            iniBytes = File.ReadAllBytes(path);
                            ParseIniContent(ini, iniBytes, errors);
                            tryCheckSingle = singlePlayRegex.IsMatch(Path.GetFileNameWithoutExtension(path));
                            errors.AddRange(LoadINI(ini, tryCheckSingle, false, ref modified));
                        }
                        break;
                    case FileType.MEG:
                    case FileType.PGM:
                        {
                            using (Megafile megafile = new Megafile(path))
                            {
                                string mprFile = megafile.Where(p => Path.GetExtension(p).ToLower() == ".mpr").FirstOrDefault();
                                if (mprFile == null)
                                {
                                    throw new ApplicationException("Cannot find the necessary file inside the " + Path.GetFileName(path) + " archive.");
                                }
                                using (Stream iniStream = megafile.OpenFile(mprFile))
                                {
                                    iniBytes = iniStream.ReadAllBytes();
                                    ParseIniContent(ini, iniBytes, errors);
                                }
                                // Don't try to check for singleplay mission: if in a meg archive, it should
                                // be a Remaster file, so SoloMission will be set if it's singleplay.
                                errors.AddRange(LoadINI(ini, false, false, ref modified));
                            }
                        }
                        break;
                    case FileType.MIX:
                        iniBytes = MixPath.ReadFile(path, FileType.INI, out MixEntry iniFile);
                        ParseIniContent(ini, iniBytes, errors);
                        tryCheckSingle = iniFile.Name == null || singlePlayRegex.IsMatch(Path.GetFileNameWithoutExtension(iniFile.Name));
                        errors.AddRange(LoadINI(ini, tryCheckSingle, true, ref modified));
                        break;
                    default:
                        throw new NotSupportedException("Unsupported filetype.");
                }
                if (modified)
                {
                    this.Dirty = true;
                }
                return errors;
            }
            finally
            {
                isLoading = false;
            }
        }

        private void ParseIniContent(INI ini, byte[] iniBytes, List<string> errors)
        {
            bool fixedSemicolon = false;
            Encoding encDOS = Encoding.GetEncoding(437);
            string iniText = encDOS.GetString(iniBytes);
            Encoding encUtf8 = new UTF8Encoding(false, false);
            string iniTextUtf8 = encUtf8.GetString(iniBytes);
            ini.Parse(iniText);
            // Specific support for DOS-437 file but with some specific sections in UTF-8.
            if (iniTextUtf8 != null)
            {
                INI utf8Ini = new INI();
                utf8Ini.Parse(iniTextUtf8);
                // Steam section
                INISection steamSectionUtf8 = utf8Ini.Sections["Steam"];
                if (steamSectionUtf8 != null)
                {
                    if (!ini.Sections.Replace(steamSectionUtf8))
                    {
                        ini.Sections.Add(steamSectionUtf8);
                    }
                }
                // Name and author from Basic section
                INISection basicSectionUtf8 = utf8Ini.Sections["Basic"];
                INISection basicSectionDos = ini.Sections["Basic"];
                if (basicSectionUtf8 != null && basicSectionDos != null)
                {
                    if (basicSectionUtf8.Keys.Contains("Name") && !basicSectionUtf8.Keys["Name"].Contains('\uFFFD'))
                    {
                        basicSectionDos.Keys["Name"] = basicSectionUtf8.Keys["Name"];
                    }
                    if (basicSectionUtf8.Keys.Contains("Author") && !basicSectionUtf8.Keys["Author"].Contains('\uFFFD'))
                    {
                        basicSectionDos.Keys["Author"] = basicSectionUtf8.Keys["Author"];
                    }
                }
                // Remastered one-line "Text" briefing from [Briefing] section
                INISection briefSectionUtf8 = utf8Ini.Sections["Briefing"];
                INISection briefSectionDos = ini.Sections["Briefing"];
                // Use UTF-8 briefing if present. Restore content behind semicolon cut off as 'comment'.
                if (briefSectionUtf8 != null && briefSectionDos != null && briefSectionUtf8.Keys.Contains("Text"))
                {
                    string comment = briefSectionUtf8.GetComment("Text");
                    string briefing = briefSectionUtf8.Keys["Text"];
                    if (comment != null)
                    {
                        briefing += comment;
                    }
                    briefSectionDos.Keys["Text"] = briefing;
                }
                else if (briefSectionDos != null)
                {
                    int line = 1;
                    string lineStr = line.ToString();
                    while (briefSectionDos.Contains(lineStr))
                    {
                        string comment = briefSectionDos.GetComment(lineStr);
                        if (comment != null)
                        {
                            briefSectionDos[lineStr] = briefSectionDos[lineStr] + comment;
                            fixedSemicolon = true;
                        }
                        line++;
                        lineStr = line.ToString();
                    }
                }
            }
            if (fixedSemicolon)
            {
                errors.Add("Classic briefing contains semicolons. These are not supported by the classic game and will cut off lines when used.");
            }
        }

        private IEnumerable<string> LoadINI(INI ini, bool tryCheckSoloMission, bool fromMix, ref bool modified)
        {
            List<string> errors = new List<string>();
            Map.BeginUpdate();
            // Fetch some rules.ini information
            errors.AddRange(UpdateBuildingRules(ini, this.Map, false));
            // Just gonna remove this; I assume it'll be invalid after a re-save anyway.
            ini.Sections.Extract("Digest");
            HouseType player = this.LoadBasic(ini);
            bool expansionEnabled = LoadAftermath(ini);
            LoadMapInfo(ini, errors, ref modified);
            LoadSteamInfo(ini);
            List<TeamType> teamTypes = this.LoadTeamTypes(ini, errors, ref modified);
            List<Trigger> triggers = this.LoadTriggers(ini, errors, ref modified);
            // Rules should be applied in advance to correctly set bibs.
            bool isSolo = CheckSwitchToSolo(tryCheckSoloMission, fromMix, triggers, Map.BasicSection.SoloMission, player, errors);
            SetMissionRules(ini, isSolo, expansionEnabled, errors, ref modified);
            Dictionary<string, string> caseTrigs = Trigger.None.Yield().Concat(triggers.Select(t => t.Name)).ToDictionary(t => t, StringComparer.OrdinalIgnoreCase);
            LoadMapPack(ini, errors, ref modified);
            LoadSmudge(ini, errors, ref modified);
            HashSet<string> checkUnitTrigs = Trigger.None.Yield().Concat(Map.FilterUnitTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            LoadUnits(ini, caseTrigs, checkUnitTrigs, errors, ref modified);
            LoadAircraft(ini, errors, ref modified);
            LoadShips(ini, caseTrigs, checkUnitTrigs, errors, ref modified);
            LoadInfantry(ini, caseTrigs, checkUnitTrigs, errors, ref modified);
            HashSet<string> checkStrcTrigs = Trigger.None.Yield().Concat(Map.FilterStructureTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            LoadStructures(ini, caseTrigs, checkStrcTrigs, errors, ref modified);
            LoadBase(ini, errors, ref modified);
            // Terrain objects in RA have no triggers.
            //HashSet<string> checkTerrTrigs = Trigger.None.Yield().Concat(Map.FilterTerrainTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            LoadTerrain(ini, errors, ref modified);
            LoadOverlay(ini, errors, ref modified);
            LoadWaypoints(ini, errors, ref modified);
            HashSet<string> checkCellTrigs = Map.FilterCellTriggers(triggers).Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            LoadCellTriggers(ini, caseTrigs, checkCellTrigs, errors, ref modified);
            LoadBriefing(ini, errors, ref modified);
            LoadHouses(ini, errors, ref modified);
            LinkTriggersAndTeams(triggers, teamTypes, checkUnitTrigs, errors, ref modified);
            // Now they are linked, triggers and tamtypes can be sorted.
            ExplorerComparer comparer = new ExplorerComparer();
            // Apply teamtypes.
            Map.TeamTypes.Clear();
            Map.TeamTypes.AddRange(teamTypes.OrderBy(t => t.Name, comparer));
            UpdateBasePlayerHouse();
            triggers.Sort((x, y) => comparer.Compare(x.Name, y.Name));
            ClearUnusedTriggerArguments(triggers);
            CheckTriggersGlobals(triggers, teamTypes, errors, ref modified, player);
            // Apply triggers in a way that won't trigger the notifications.
            Map.Triggers.Clear();
            Map.Triggers.AddRange(triggers);
            CleanIniContents(ini);
            // Store remaining extra sections.
            extraSections = ini.Sections.Count == 0 ? null : ini.Sections;
            Map.EndUpdate();
            return errors;
        }

        private HouseType LoadBasic(INI ini)
        {
            // Basic info
            BasicSection basic = (BasicSection)Map.BasicSection;
            INISection basicSection = INITools.ParseAndLeaveRemainder(ini, "Basic", basic, new MapContext(Map, true));
            if (basicSection != null)
            {
                List<string> movies = new List<string>(movieTypesRa);
                for (int i = 0; i < movies.Count; ++i)
                {
                    string vidName = GeneralUtils.AddRemarks(movies[i], movieEmpty, true, movieTypesRemarksOld, remarkOld);
                    movies[i] = GeneralUtils.AddRemarks(vidName, movieEmpty, true, movieTypesRemarksNew, remarkNew);
                }
                movies.Insert(0, movieEmpty);
                basic.Intro = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Intro, movieEmpty, true, movieTypesRemarksOld, remarkOld), movieEmpty, true, movieTypesRemarksNew, remarkNew);
                basic.Intro = GeneralUtils.FilterToExisting(basic.Intro, movieEmpty, true, movies);
                basic.Brief = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Brief, movieEmpty, true, movieTypesRemarksOld, remarkOld), movieEmpty, true, movieTypesRemarksNew, remarkNew);
                basic.Brief = GeneralUtils.FilterToExisting(basic.Brief, movieEmpty, true, movies);
                basic.Action = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Action, movieEmpty, true, movieTypesRemarksOld, remarkOld), movieEmpty, true, movieTypesRemarksNew, remarkNew);
                basic.Action = GeneralUtils.FilterToExisting(basic.Action, movieEmpty, true, movies);
                basic.Win = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Win, movieEmpty, true, movieTypesRemarksOld, remarkOld), movieEmpty, true, movieTypesRemarksNew, remarkNew);
                basic.Win = GeneralUtils.FilterToExisting(basic.Win, movieEmpty, true, movies);
                basic.Win2 = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Win2, movieEmpty, true, movieTypesRemarksOld, remarkOld), movieEmpty, true, movieTypesRemarksNew, remarkNew);
                basic.Win2 = GeneralUtils.FilterToExisting(basic.Win2, movieEmpty, true, movies);
                basic.Win3 = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Win3, movieEmpty, true, movieTypesRemarksOld, remarkOld), movieEmpty, true, movieTypesRemarksNew, remarkNew);
                basic.Win3 = GeneralUtils.FilterToExisting(basic.Win3, movieEmpty, true, movies);
                basic.Win4 = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Win4, movieEmpty, true, movieTypesRemarksOld, remarkOld), movieEmpty, true, movieTypesRemarksNew, remarkNew);
                basic.Win4 = GeneralUtils.FilterToExisting(basic.Win4, movieEmpty, true, movies);
                basic.Lose = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Lose, movieEmpty, true, movieTypesRemarksOld, remarkOld), movieEmpty, true, movieTypesRemarksNew, remarkNew);
                basic.Lose = GeneralUtils.FilterToExisting(basic.Lose, movieEmpty, true, movies);
            }
            string plName = Map.BasicSection.Player;
            HouseType player = Map.HouseTypes.Where(t => t.Equals(plName)).FirstOrDefault() ?? Map.HouseTypes.First();
            plName = player.Name;
            Map.BasicSection.Player = plName;
            Map.BasicSection.BasePlayer = HouseTypes.GetClassicOpposingPlayer(plName);
            return player;
        }

        private bool LoadAftermath(INI ini)
        {
            bool aftermathEnabled = false;
            // Don't remove from extra sections.
            INISection aftermathSection = ini.Sections["Aftermath"];
            if (aftermathSection != null)
            {
                string amEnabled = aftermathSection.TryGetValue("NewUnitsEnabled");
                aftermathEnabled = YesNoBooleanTypeConverter.Parse(amEnabled);
                aftermathSection.Remove("NewUnitsEnabled");
                // Remove if empty.
                if (aftermathSection.Empty)
                    ini.Sections.Remove(aftermathSection.Name);
            }
            // Needs to be enabled in advance; it determines which units are valid to have placed on the map.
            Map.BasicSection.ExpansionEnabled = aftermathEnabled;
            return aftermathEnabled;
        }

        private void LoadMapInfo(INI ini, List<string> errors, ref bool modified)
        {
            // Map info
            string theaterStr = ini["Map"]?.TryGetValue("Theater") ?? String.Empty;
            INISection mapSection = INITools.ParseAndLeaveRemainder(ini, "Map", Map.MapSection, new MapContext(Map, true));
            if (!this.Map.TheaterTypes.Any(thr => String.Equals(thr.Name, theaterStr, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add(String.Format("Theater \"{0}\" could not be found. Defaulting to \"{1}\".", theaterStr, Map.Theater));
                modified = true;
            }
            Map.MapSection.FixBounds();
        }

        private void LoadSteamInfo(INI ini)
        {
            // Steam info
            INISection steamSection = ini.Sections.Extract("Steam");
            if (steamSection != null)
            {
                // Ignore any errors in this.
                INI.ParseSection(new MapContext(Map, true), steamSection, Map.SteamSection, true);
            }
        }

        private T indexToType<T>(IList<T> list, string index, bool defnull)
        {
            return (int.TryParse(index, out int result) && (result >= 0) && (result < list.Count)) ? list[result] : (defnull ? default(T) : list.First());
        }

        private List<TeamType> LoadTeamTypes(INI ini, List<string> errors, ref bool modified)
        {
            INISection teamTypesSection = ini.Sections.Extract("TeamTypes");
            List<TeamType> teamTypes = new List<TeamType>();
            if (teamTypesSection == null || teamTypesSection.Count == 0)
            {
                return teamTypes;
            }
            foreach (KeyValuePair<string, string> kvp in teamTypesSection)
            {
                try
                {
                    if (kvp.Key.Length > 8)
                    {
                        errors.Add(string.Format("TeamType '{0}' has a name that is longer than 8 characters. This will not be corrected by the loading process, but should be addressed, since it can make the teams fail to read correctly, and might even crash the game.", kvp.Key));
                    }
                    TeamType teamType = new TeamType { Name = kvp.Key };
                    string[] tokens = kvp.Value.Split(',');
                    string houseStr = tokens[(int)TeamTypeOptions.House];
                    teamType.House = Map.HouseTypes.Where(t => t.Equals(sbyte.Parse(houseStr))).FirstOrDefault();
                    if (teamType.House == null)
                    {
                        HouseType defHouse = Map.HouseTypes.First();
                        errors.Add(string.Format("Team '{0}' has unknown house ID '{1}'; clearing to '{2}'.", kvp.Key, houseStr, defHouse.Name));
                        modified = true;
                        teamType.House = defHouse;
                    }
                    int flags = int.Parse(tokens[(int)TeamTypeOptions.Flags]);
                    teamType.IsRoundAbout = (flags & 0x01) != 0;
                    teamType.IsSuicide = (flags & 0x02) != 0;
                    teamType.IsAutocreate = (flags & 0x04) != 0;
                    teamType.IsPrebuilt = (flags & 0x08) != 0;
                    teamType.IsReinforcable = (flags & 0x10) != 0;
                    teamType.RecruitPriority = int.Parse(tokens[(int)TeamTypeOptions.RecruitPriority]);
                    teamType.InitNum = byte.Parse(tokens[(int)TeamTypeOptions.InitNum]);
                    teamType.MaxAllowed = byte.Parse(tokens[(int)TeamTypeOptions.MaxAllowed]);
                    teamType.Origin = int.Parse(tokens[(int)TeamTypeOptions.Origin]);
                    teamType.Trigger = tokens[(int)TeamTypeOptions.Trigger];
                    int numClasses = int.Parse(tokens[(int)TeamTypeOptions.Classes]);
                    int classesIndex = (int)TeamTypeOptions.Classes + 1;
                    int classesIndexEnd = classesIndex + numClasses;
                    int classesMax = Math.Min(Globals.MaxTeamClasses, numClasses);
                    int classesIndexMax = classesIndex + classesMax;
                    for (int i = classesIndex; i < classesIndexMax; ++i)
                    {
                        string[] classTokens = tokens[i].Split(':');
                        if (classTokens.Length == 2)
                        {
                            ITechnoType type = fullTechnoTypes.Where(t => t.Equals(classTokens[0])).FirstOrDefault();
                            byte count = byte.Parse(classTokens[1]);
                            if (type != null)
                            {
                                if (!Map.BasicSection.ExpansionEnabled && type.IsExpansionOnly)
                                {
                                    errors.Add(string.Format("Team '{0}' contains expansion unit '{1}', but expansion units are not enabled; enabling expansion units.", kvp.Key, type.Name));
                                    Map.BasicSection.ExpansionEnabled = true;
                                    modified = true;
                                }
                                teamType.Classes.Add(new TeamTypeClass { Type = type, Count = count });
                            }
                            else
                            {
                                errors.Add(string.Format("Team '{0}' references unknown class '{1}'.", kvp.Key, classTokens[0]));
                                modified = true;
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Team '{0}' has wrong number of tokens for class index {1} (expecting 2).", kvp.Key, i));
                            modified = true;
                        }
                    }
                    if (numClasses > Globals.MaxTeamClasses)
                    {
                        errors.Add(string.Format("Team '{0}' has more classes than the game can handle (has {1}, maximum is {2}).", kvp.Key, numClasses, Globals.MaxTeamClasses));
                        modified = true;
                    }
                    int numMissions = int.Parse(tokens[classesIndexEnd]);
                    int missionsIndex = classesIndexEnd + 1;
                    int missionsIndexEnd = missionsIndex + numMissions;
                    int missionsMax = Math.Min(Globals.MaxTeamMissions, numMissions);
                    int missionsIndexMax = missionsIndex + missionsMax;
                    for (int i = missionsIndex; i < missionsIndexMax; ++i)
                    {
                        string[] missionTokens = tokens[i].Split(':');
                        if (missionTokens.Length == 2)
                        {
                            TeamMission mission = indexToType(Map.TeamMissionTypes, missionTokens[0], true);
                            if (mission != null)
                            {
                                string argError = null;
                                string argStr = missionTokens[1];
                                if (!Int32.TryParse(argStr, out int arg))
                                {
                                    argError = string.Format("Team '{0}', orders index {1} ('{2}') has a non-numeric value '{3}'. Reverting to 0.", kvp.Key, i, mission.Mission, argStr);
                                }
                                else if (mission.ArgType == TeamMissionArgType.Time && arg < 0)
                                {
                                    argError = string.Format("Team '{0}', orders index {1} ('{2}') has a bad value '{3}' for a Time argument. Reverting to 0.", kvp.Key, i, mission.Mission, argStr);
                                }
                                else if (mission.ArgType == TeamMissionArgType.Waypoint && (arg < -1 || arg > Map.Waypoints.Length))
                                {
                                    argError = string.Format("Team '{0}', orders index {1} ('{2}') has a bad value '{3}' for a Waypoint argument. Reverting to 0.", kvp.Key, i, mission.Mission, argStr);
                                }
                                else if (mission.ArgType == TeamMissionArgType.OptionsList && (arg < 0 || arg > mission.DropdownOptions.Max(vl => vl.Value)))
                                {
                                    argError = string.Format("Team '{0}', orders index {1} ('{2}') has a bad value '{3}' for the available options. Reverting to 0.", kvp.Key, i, mission.Mission, argStr);
                                }
                                else if (mission.ArgType == TeamMissionArgType.MapCell && (arg < 0 || arg >= Map.Metrics.Length))
                                {
                                    argError = string.Format("Team '{0}', orders index {1} ('{2}') has a bad value '{3}' for a Cell argument. Reverting to 0.", kvp.Key, i, mission.Mission, argStr);
                                }
                                else if (mission.ArgType == TeamMissionArgType.MissionNumber && (arg < 0 || arg > missionsMax))
                                {
                                    argError = string.Format("Team '{0}', orders index {1} ('{2}') has a bad value '{3}' for an orders index argument. Reverting to 0.", kvp.Key, i, mission.Mission, argStr);
                                }
                                else if (mission.ArgType == TeamMissionArgType.Tarcom && arg < 0)
                                {
                                    argError = string.Format("Team '{0}', orders index {1} ('{2}') has a bad value '{3}' for a Tarcom argument. Reverting to 0.", kvp.Key, i, mission.Mission, argStr);
                                }
                                // Note: globals are deliberately NOT checked here; the CheckTriggersGlobals logic takes care of checking that.
                                if (argError != null)
                                {
                                    errors.Add(argError);
                                    modified = true;
                                    arg = 0;
                                }
                                teamType.Missions.Add(new TeamTypeMission { Mission = mission, Argument = arg });
                            }
                            else
                            {
                                errors.Add(string.Format("Team '{0}' references unknown orders id '{1}'.", kvp.Key, missionTokens[0]));
                                modified = true;
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Team '{0}' has wrong number of tokens for orders index {1} (expecting 2).", kvp.Key, i));
                            modified = true;
                        }
                    }
                    if (numMissions > Globals.MaxTeamMissions)
                    {
                        errors.Add(string.Format("Team '{0}' has more orders than the game can handle (has {1}, maximum is {2}).", kvp.Key, numMissions, Globals.MaxTeamMissions));
                        modified = true;
                    }
                    teamTypes.Add(teamType);
                }
                catch (Exception ex)
                {
                    errors.Add(string.Format("Teamtype '{0}' has errors and can't be parsed: {1}.", kvp.Key, ex.Message));
                    modified = true;
                }
            }
            return teamTypes;
        }

        private List<Trigger> LoadTriggers(INI ini, List<string> errors, ref bool modified)
        {
            INISection triggersSection = ini.Sections.Extract("Trigs");
            List<Trigger> triggers = new List<Trigger>();
            if (triggersSection == null)
            {
                return triggers;
            }
            void fixEvent(TriggerEvent e, string triggerName, int evtNr)
            {
                switch (e.EventType)
                {
                    case EventTypes.TEVENT_THIEVED:
                    case EventTypes.TEVENT_PLAYER_ENTERED:
                    case EventTypes.TEVENT_CROSS_HORIZONTAL:
                    case EventTypes.TEVENT_CROSS_VERTICAL:
                    case EventTypes.TEVENT_ENTERS_ZONE:
                    case EventTypes.TEVENT_HOUSE_DISCOVERED:
                    case EventTypes.TEVENT_BUILDINGS_DESTROYED:
                    case EventTypes.TEVENT_UNITS_DESTROYED:
                    case EventTypes.TEVENT_ALL_DESTROYED:
                    case EventTypes.TEVENT_LOW_POWER:
                    case EventTypes.TEVENT_BUILDING_EXISTS:
                    case EventTypes.TEVENT_BUILD:
                    case EventTypes.TEVENT_BUILD_UNIT:
                    case EventTypes.TEVENT_BUILD_INFANTRY:
                        if (e.Data != -1)
                        {
                            e.Data &= 0xFF;
                        }
                        break;
                    case EventTypes.TEVENT_BUILD_AIRCRAFT:
                        if (e.Data != -1)
                        {
                            e.Data &= 0xFF;
                            // original editor did this weird thing of upshifting unit IDs instead of defining specific classes for aircraft and vessels.
                            const int AircraftMask = 1 << 5;
                            if ((e.Data & AircraftMask) == AircraftMask)
                            {
                                int fixedData = (int)e.Data & ~AircraftMask;
                                AircraftType heliType = this.Map.AllTeamTechnoTypes.OfType<AircraftType>().FirstOrDefault(u => u.ID == fixedData);
                                if (heliType == null)
                                {
                                    heliType = this.Map.AllTeamTechnoTypes.OfType<AircraftType>().FirstOrDefault();
                                    fixedData = heliType.ID;
                                }
                                errors.Add(string.Format("Trigger '{0}', Event {1} (\"{2}\") has bad value '{3}' set for the Aircraft id. This is most likely caused by older versions of this editor. Fixing id to '{4}' ({5}).",
                                    triggerName, evtNr, e.EventType.TrimEnd('.'), e.Data, fixedData, heliType.Name));
                                e.Data = fixedData;
                            }
                        }
                        break;
                }
            };
            void fixAction(TriggerAction a)
            {
                switch (a.ActionType)
                {
                    case ActionTypes.TACTION_1_SPECIAL:
                    case ActionTypes.TACTION_FULL_SPECIAL:
                    case ActionTypes.TACTION_FIRE_SALE:
                    case ActionTypes.TACTION_WIN:
                    case ActionTypes.TACTION_LOSE:
                    case ActionTypes.TACTION_ALL_HUNT:
                    case ActionTypes.TACTION_BEGIN_PRODUCTION:
                    case ActionTypes.TACTION_AUTOCREATE:
                    case ActionTypes.TACTION_BASE_BUILDING:
                    case ActionTypes.TACTION_CREATE_TEAM:
                    case ActionTypes.TACTION_DESTROY_TEAM:
                    case ActionTypes.TACTION_REINFORCEMENTS:
                    case ActionTypes.TACTION_FORCE_TRIGGER:
                    case ActionTypes.TACTION_DESTROY_TRIGGER:
                    case ActionTypes.TACTION_DZ:
                    case ActionTypes.TACTION_REVEAL_SOME:
                    case ActionTypes.TACTION_REVEAL_ZONE:
                    case ActionTypes.TACTION_PLAY_MUSIC:
                    case ActionTypes.TACTION_PLAY_MOVIE:
                    case ActionTypes.TACTION_PLAY_SOUND:
                    case ActionTypes.TACTION_PLAY_SPEECH:
                    case ActionTypes.TACTION_PREFERRED_TARGET:
                        if (a.Data != -1)
                        {
                            a.Data &= 0xFF;
                        }
                        break;
                }
            };
            foreach (KeyValuePair<string, string> kvp in triggersSection)
            {
                try
                {
                    string[] tokens = kvp.Value.Split(',');
                    if (tokens.Length == 18)
                    {
                        if (kvp.Key.Length > 4)
                        {
                            errors.Add(string.Format("Trigger '{0}' has a name that is longer than 4 characters. This will not be corrected by the loading process, but should be addressed, since it can make the triggers fail to link correctly to objects and cell triggers, and might even crash the game.", kvp.Key));
                        }
                        Trigger trigger = new Trigger { Name = kvp.Key };
                        trigger.PersistentType = (TriggerPersistentType)int.Parse(tokens[0]);
                        int houseId = sbyte.Parse(tokens[1]);
                        trigger.House = houseId == -1 ? House.None : Map.HouseTypes.Where(t => t.Equals(houseId)).FirstOrDefault()?.Name;
                        if (trigger.House == null)
                        {
                            errors.Add(string.Format("Trigger '{0}' has unknown house ID '{1}'; clearing to '{2}'.", kvp.Key, tokens[1], House.None));
                            modified = true;
                            trigger.House = House.None;
                        }
                        trigger.EventControl = (TriggerMultiStyleType)int.Parse(tokens[2]);
                        trigger.Event1.EventType = indexToType(Map.EventTypes, tokens[4], false);
                        trigger.Event1.Team = tokens[5];
                        trigger.Event1.Data = long.Parse(tokens[6]);
                        trigger.Event2.EventType = indexToType(Map.EventTypes, tokens[7], false);
                        trigger.Event2.Team = tokens[8];
                        trigger.Event2.Data = long.Parse(tokens[9]);
                        trigger.Action1.ActionType = indexToType(Map.ActionTypes, tokens[10], false);
                        trigger.Action1.Team = tokens[11];
                        trigger.Action1.Trigger = tokens[12];
                        trigger.Action1.Data = long.Parse(tokens[13]);
                        trigger.Action2.ActionType = indexToType(Map.ActionTypes, tokens[14], false);
                        trigger.Action2.Team = tokens[15];
                        trigger.Action2.Trigger = tokens[16];
                        trigger.Action2.Data = long.Parse(tokens[17]);
                        // Fix up data caused by union usage in the legacy game
                        fixEvent(trigger.Event1, kvp.Key, 1);
                        fixEvent(trigger.Event2, kvp.Key, 2);
                        fixAction(trigger.Action1);
                        fixAction(trigger.Action2);
                        triggers.Add(trigger);
                    }
                    else
                    {
                        errors.Add(string.Format("Trigger '{0}' has too few tokens (expecting 18).", kvp.Key));
                        modified = true;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(string.Format("Trigger '{0}' has curErrors and can't be parsed: {1}.", kvp.Key, ex.Message));
                    modified = true;
                }
            }
            return triggers;
        }

        private void LoadMapPack(INI ini, List<string> errors, ref bool modified)
        {
            INISection mapPackSection = ini.Sections.Extract("MapPack");
            if (mapPackSection == null)
            {
                return;
            }
            Map.Templates.Clear();
            byte[] data = DecompressLCWSection(mapPackSection, 3, errors, ref modified);
            if (data == null)
            {
                return;
            }
            int width = Map.Metrics.Width;
            int height = Map.Metrics.Height;
            // Dump into array, so no lookups are needed.
            TemplateType[] templateTypes = new TemplateType[0x10000];
            foreach (TemplateType tt in Map.TemplateTypes)
            {
                templateTypes[tt.ID] = tt;
            }
            // Amount of tile 255 detected outside map bounds.
            int oldClearCount = 0;
            int oldClearOutside = 0;
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                for (int y = 0; y < height; ++y)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        ushort typeValue = reader.ReadUInt16();
                        TemplateType templateType = templateTypes[typeValue];
                        if (templateType == null && typeValue != 0xFFFF)
                        {
                            errors.Add(String.Format("Unknown template value {0:X4} at cell [{1},{2}]; clearing.", typeValue, x, y));
                            modified = true;
                        }
                        else if (templateType != null)
                        {
                            if (templateType.Flag.HasFlag(TemplateTypeFlag.Clear) || templateType.Flag.HasFlag(TemplateTypeFlag.Group))
                            {
                                // No explicitly set Clear terrain allowed. Also no explicitly set versions allowed of the "group" dummy entries.
                                templateType = null;
                            }
                            else if (!templateType.ExistsInTheater)
                            {
                                if (typeValue == 255)
                                {
                                    if (Globals.ConvertRaObsoleteClear)
                                    {
                                        oldClearCount++;
                                    }
                                }
                                else if (Globals.FilterTheaterObjects)
                                {
                                    errors.Add(String.Format("Template '{0}' at cell [{1},{2}] is not available in the set theater; clearing.", templateType.Name.ToUpper(), x, y));
                                    modified = true;
                                    templateType = null;
                                }
                            }
                            else if (Globals.ConvertRaObsoleteClear && typeValue == 255)
                            {
                                // If this point is reached, 255 is allowed, meaning we're in Interior theater.
                                // Count the amount of tiles outside the map bounds, for the 80% check.
                                oldClearCount++;
                                if (!Map.Bounds.Contains(x, y))
                                {
                                    oldClearOutside++;
                                }
                            }
                        }
                        Map.Templates[y, x] = (templateType != null) ? new Template { Type = templateType } : null;
                    }
                }
                for (int y = 0; y < height; ++y)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        byte iconValue = reader.ReadByte();
                        Template template = Map.Templates[y, x];
                        // Prevent loading of illegal tiles. Do not give errors on clear terrain if it's going to be cleared anyway.
                        if (template != null && (template.Type.ID != 255 || !Globals.ConvertRaObsoleteClear))
                        {
                            TemplateType templateType = template.Type;
                            bool tileOk = false;
                            if (iconValue >= templateType.NumIcons)
                            {
                                errors.Add(String.Format("Template '{0}' at cell [{1},{2}] has an icon set ({3}) that is outside its icons range; clearing.", templateType.Name.ToUpper(), x, y, iconValue));
                                modified = true;
                            }
                            else if (!templateType.IsRandom && templateType.IconMask != null && !templateType.IconMask[iconValue / templateType.IconWidth, iconValue % templateType.IconWidth])
                            {
                                // Attempt to automatically correct known errors like the bridges
                                if (FixCorruptTiles(template, iconValue, out byte newIcon, out string type))
                                {
                                    tileOk = true;
                                    errors.Add(String.Format("Template '{0}' at cell [{1},{2}], icon {3} is an illegal {4} tile; fixing.", templateType.Name.ToUpper(), x, y, iconValue, type));
                                    // Only adapt this after adding the old icon value in the message.
                                    iconValue = newIcon;
                                }
                                else
                                {
                                    errors.Add(String.Format("Template '{0}' at cell [{1},{2}] has an icon set ({3}) that is not part of its placeable cells; clearing.", templateType.Name.ToUpper(), x, y, iconValue));
                                }
                                modified = true;
                            }
                            else if (templateType != TemplateTypes.Clear)
                            {
                                tileOk = true;
                            }
                            if (!tileOk)
                            {
                                Map.Templates[y, x] = null;
                            }
                            else
                            {
                                template.Icon = iconValue;
                            }
                        }
                    }
                }
            }
            // On theaters where tile 255 is an existing tile, test if more than 80% of the area outside the map is tile 255.
            bool tileFFValidForTheater = templateTypes[0xFF]?.ExistsInTheater ?? false;
            if (oldClearCount > 0 && (!tileFFValidForTheater || oldClearOutside > (width * height - Map.Bounds.Width * Map.Bounds.Height) * 8 / 10))
            {
                TemplateType clear = Map.TemplateTypes.Where(tt => tt.Flag.HasFlag(TemplateTypeFlag.Clear)).FirstOrDefault();
                bool clearIsPassable = clear == null || !clear.ExistsInTheater || this.IsFullyLandUnitPassable(clear.LandTypes[0]);
                // This is an old map. Clear any 255 tile.
                // If clear terrain is not passable, detect passable areas that touch the outside border, and add spawnable areas there. Remove all the rest.
                TemplateType clearFallBack = null;
                HashSet<Point> border = null;
                if (!clearIsPassable)
                {
                    // Don't bother doing this if clear terrain is passable; then this data is never used.
                    clearFallBack = Map.TemplateTypes.Where(tt => tt.ExistsInTheater && tt.IconWidth == 1 && tt.IconHeight == 1 && tt.LandTypes[0] == LandType.Clear).FirstOrDefault();
                    Rectangle mapBounds = Map.Bounds;
                    Rectangle mapBorderBounds = mapBounds;
                    Rectangle mapFullBounds = Map.Metrics.Bounds;
                    mapBorderBounds.Inflate(3, 3);
                    border = mapBorderBounds.Points().Where(p => mapFullBounds.Contains(p) && !mapBounds.Contains(p)).ToHashSet();
                    // Eval function
                    bool isImpassableCell(Template cell)
                    {
                        return (!clearIsPassable && (cell == null || cell.Type.ID == 255)) || (cell != null && !this.IsFullyLandUnitPassable(cell.Type.LandTypes[cell.Icon]));
                    }
                    // Corners.
                    if (isImpassableCell(Map.Templates[mapBounds.Top, mapBounds.Left]))
                    {
                        border.RemoveWhere(p => p.Y < mapBounds.Top && p.X < mapBounds.Left);
                    }
                    if (isImpassableCell(Map.Templates[mapBounds.Top, mapBounds.Right - 1]))
                    {
                        border.RemoveWhere(p => p.Y < mapBounds.Top && p.X >= mapBounds.Right);
                    }
                    if (isImpassableCell(Map.Templates[mapBounds.Bottom - 1, mapBounds.Left]))
                    {
                        border.RemoveWhere(p => p.Y >= mapBounds.Bottom && p.X < mapBounds.Left);
                    }
                    if (isImpassableCell(Map.Templates[mapBounds.Bottom - 1, mapBounds.Right - 1]))
                    {
                        border.RemoveWhere(p => p.Y >= mapBounds.Bottom && p.X >= mapBounds.Right);
                    }
                    foreach (int xCell in Enumerable.Range(mapBounds.Left, mapBounds.Width))
                    {
                        Template top = Map.Templates[mapBounds.Top, xCell];
                        if (isImpassableCell(Map.Templates[mapBounds.Top, xCell]))
                        {
                            border.RemoveWhere(p => p.Y < mapBounds.Top && p.X == xCell);
                        }
                        Template bottom = Map.Templates[mapBounds.Bottom - 1, xCell];
                        if ((bottom == null && !clearIsPassable) || bottom.Type.ID == 255 || !this.IsFullyLandUnitPassable(bottom.Type.LandTypes[bottom.Icon]))
                        {
                            border.RemoveWhere(p => p.Y >= mapBounds.Bottom && p.X == xCell);
                        }
                    }
                    foreach (int yCell in Enumerable.Range(mapBounds.Top, mapBounds.Height))
                    {
                        if (isImpassableCell(Map.Templates[yCell, mapBounds.Left]))
                        {
                            border.RemoveWhere(p => p.X < mapBounds.Left && p.Y == yCell);
                        }
                        if (isImpassableCell(Map.Templates[yCell, mapBounds.Right - 1]))
                        {
                            border.RemoveWhere(p => p.X >= mapBounds.Right && p.Y == yCell);
                        }
                    }
                }
                for (int y = 0; y < height; ++y)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        Template cell = Map.Templates[y, x];
                        if (cell != null && cell.Type.ID == 255)
                        {
                            if (clearIsPassable || !border.Contains(new Point(x, y)) || clearFallBack == null)
                            {
                                Map.Templates[y, x] = null;
                                modified = true;
                            }
                            else
                            {
                                // Retain passable cells on any potential entrance points of the map.
                                Map.Templates[y, x] = new Template() { Type = clearFallBack, Icon = 0 };
                                modified = true;
                            }
                        }
                    }
                }
                string obsError = "Use of obsolete version of 'Clear' terrain detected; clearing.";
                if (!clearIsPassable && border != null && border.Count() > 0)
                    obsError += " Generating passable areas for possible scripted reinforcements.";
                errors.Add(obsError);
            }
#if DEBUG && false
            // Test code for visualising old edits. Swaps out old clear with actual clear, and actual clear with the unused tile "B4".
            // Add B4 to the tileset to show the changed areas more clearly.
            if (!Globals.ConvertRaObsoleteClear)
            {
                TemplateType obs = TemplateTypes.Boulder4;
                for (int y = 0; y < height; ++y)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        if (Map.Templates[y, x] == null)
                        {
                            Map.Templates[y, x] = new Template() { Type = obs, Icon = 0 };
                        }
                        else if (Map.Templates[y, x].Type.ID == 255)
                        {
                            Map.Templates[y, x] = null;
                        }
                    }
                }
            }
#endif
        }

        private void LoadSmudge(INI ini, List<string> errors, ref bool modified)
        {
            INISection smudgeSection = ini.Sections.Extract("Smudge");
            if (smudgeSection == null)
            {
                return;
            }
            foreach (KeyValuePair<string, string> kvp in smudgeSection)
            {
                int cell;
                if (!int.TryParse(kvp.Key, out cell))
                {
                    errors.Add(string.Format("Cell for Smudge cannot be parsed. Key: '{0}', value: '{1}'; skipping.", kvp.Key, kvp.Value));
                    modified = true;
                    continue;
                }
                string[] tokens = kvp.Value.Split(',');
                if (tokens.Length == 3)
                {
                    // Craters other than cr1 don't work right in the game. Replace them by stage-0 cr1.
                    bool badCrater = Globals.ConvertCraters && SmudgeTypes.BadCraters.IsMatch(tokens[0]);
                    SmudgeType smudgeType = badCrater ? SmudgeTypes.Crater1 : Map.SmudgeTypes.Where(t => t.Equals(tokens[0]) && !t.IsAutoBib).FirstOrDefault();
                    if (smudgeType != null)
                    {
                        if (Globals.FilterTheaterObjects && !smudgeType.ExistsInTheater)
                        {
                            errors.Add(string.Format("Smudge '{0}' is not available in the set theater; skipping.", smudgeType.Name));
                            modified = true;
                            continue;
                        }
                        if (badCrater)
                        {
                            errors.Add(string.Format("Smudge '{0}' does not function correctly in maps. Correcting to '{1}'.", tokens[0], smudgeType.Name));
                            modified = true;
                        }
                        int icon = 0;
                        if (smudgeType.Icons > 1 && int.TryParse(tokens[2], out icon))
                            icon = Math.Max(0, Math.Min(smudgeType.Icons - 1, icon));
                        bool multiCell = smudgeType.IsMultiCell;
                        if (Map.Metrics.GetLocation(cell, out Point location))
                        {
                            int placeIcon = 0;
                            Size size = smudgeType.Size;
                            Point placeLocation = location;
                            for (int y = 0; y < size.Height; ++y)
                            {
                                for (int x = 0; x < size.Width; ++x)
                                {
                                    placeLocation.X = location.X + x;
                                    Map.Smudge[placeLocation] = new Smudge(smudgeType, multiCell ? placeIcon++ : icon);
                                }
                                placeLocation.Y++;
                            }
                        }
                    }
                    else
                    {
                        errors.Add(string.Format("Smudge '{0}' references unknown smudge.", tokens[0]));
                        modified = true;
                    }
                }
                else
                {
                    errors.Add(string.Format("Smudge on cell '{0}' has wrong number of tokens (expecting 3).", kvp.Key));
                    modified = true;
                }
            }
        }

        private void LoadUnits(INI ini, Dictionary<string, string> caseTrigs, HashSet<string> checkUnitTrigs, List<string> errors, ref bool modified)
        {
            INISection unitsSection = ini.Sections.Extract("Units");
            if (unitsSection == null)
            {
                return;
            }
            foreach (KeyValuePair<string, string> kvp in unitsSection)
            {
                string[] tokens = kvp.Value.Split(',');
                if (tokens.Length == 7)
                {
                    UnitType unitType = Map.AllUnitTypes.Where(t => t.IsGroundUnit && t.Equals(tokens[1])).FirstOrDefault();
                    if (unitType == null)
                    {
                        errors.Add(string.Format("Unit '{0}' references unknown unit.", tokens[1]));
                        modified = true;
                        continue;
                    }
                    if (!Map.BasicSection.ExpansionEnabled && unitType.IsExpansionOnly)
                    {
                        errors.Add(string.Format("Expansion unit '{0}' encountered, but expansion units are not enabled; enabling expansion units.", unitType.Name));
                        modified = true;
                        Map.BasicSection.ExpansionEnabled = Map.BasicSection.ExpansionEnabled = true;
                    }
                    int strength;
                    if (!int.TryParse(tokens[2], out strength))
                    {
                        errors.Add(string.Format("Strength for unit '{0}' cannot be parsed; value: '{1}'; skipping.", unitType.Name, tokens[2]));
                        modified = true;
                        continue;
                    }
                    int cell;
                    if (!int.TryParse(tokens[3], out cell))
                    {
                        errors.Add(string.Format("Cell for unit '{0}' cannot be parsed; value: '{1}'; skipping.", unitType.Name, tokens[3]));
                        modified = true;
                        continue;
                    }
                    int dirValue;
                    if (!int.TryParse(tokens[4], out dirValue))
                    {
                        errors.Add(string.Format("Direction for unit '{0}' cannot be parsed; value: '{1}'; skipping.", unitType.Name, tokens[4]));
                        modified = true;
                        continue;
                    }
                    Unit newUnit = new Unit()
                    {
                        Type = unitType,
                        House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                        Strength = strength,
                        Direction = DirectionType.GetDirectionType(dirValue, Map.UnitDirectionTypes),
                        Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5])).FirstOrDefault() ?? Map.GetDefaultMission(unitType),
                    };
                    if (newUnit.House == null)
                    {
                        HouseType defHouse;
                        if ("ITALY".Equals(tokens[0], StringComparison.OrdinalIgnoreCase))
                        {
                            defHouse = HouseTypes.Ukraine;
                            errors.Add(string.Format("Unit '{0}' on cell {1} has obsolete house '{2}'; substituting with '{3}'.", newUnit.Type.Name, cell, tokens[0], defHouse.Name));
                        }
                        else
                        {
                            defHouse = Map.HouseTypes.First();
                            errors.Add(string.Format("Unit '{0}' on cell {1} references unknown house '{2}'; clearing to '{3}'.", newUnit.Type.Name, cell, tokens[0], defHouse.Name));
                        }
                        modified = true;
                        newUnit.House = defHouse;
                    }
                    if (Map.Technos.Add(cell, newUnit))
                    {
                        if (!caseTrigs.ContainsKey(tokens[6]))
                        {
                            errors.Add(string.Format("Unit '{0}' on cell {1} links to unknown trigger '{1}'; clearing trigger.", unitType.Name, cell, tokens[6]));
                            modified = true;
                            newUnit.Trigger = Trigger.None;
                        }
                        else if (!checkUnitTrigs.Contains(tokens[6]))
                        {
                            errors.Add(string.Format("Unit '{0}' on cell {1} links to trigger '{1}' which does not contain an event or action applicable to units; clearing trigger.", unitType.Name, cell, tokens[6]));
                            modified = true;
                            newUnit.Trigger = Trigger.None;
                        }
                        else
                        {
                            // Adapt to same case
                            newUnit.Trigger = caseTrigs[tokens[6]];
                        }
                    }
                    else
                    {
                        ICellOccupier techno = Map.Technos[cell];
                        if (techno is Building building)
                        {
                            errors.Add(string.Format("Unit '{0}' overlaps structure '{1}' in cell {2}; skipping.", unitType.Name, building.Type.Name, cell));
                            modified = true;
                        }
                        else if (techno is Overlay overlay)
                        {
                            errors.Add(string.Format("Unit '{0}' overlaps overlay '{1}' in cell {2}; skipping.", unitType.Name, overlay.Type.Name, cell));
                            modified = true;
                        }
                        else if (techno is Terrain terrain)
                        {
                            errors.Add(string.Format("Unit '{0}' overlaps terrain '{1}' in cell {2}; skipping.", unitType.Name, terrain.Type.Name, cell));
                            modified = true;
                        }
                        else if (techno is InfantryGroup infantry)
                        {
                            errors.Add(string.Format("Unit '{0}' overlaps infantry in cell {1}; skipping.", unitType.Name, cell));
                            modified = true;
                        }
                        else if (techno is Unit unit)
                        {
                            errors.Add(string.Format("Unit '{0}' overlaps unit '{1}' in cell {2}; skipping.", unitType.Name, unit.Type.Name, cell));
                            modified = true;
                        }
                        else
                        {
                            errors.Add(string.Format("Unit '{0}' overlaps unknown techno in cell {1}; skipping.", unitType.Name, cell));
                            modified = true;
                        }
                    }
                }
                else
                {
                    if (tokens.Length < 2)
                    {
                        errors.Add(string.Format("Unit entry '{0}' has wrong number of tokens (expecting 7).", kvp.Key));
                        modified = true;
                    }
                    else
                    {
                        errors.Add(string.Format("Unit '{0}' has wrong number of tokens (expecting 7).", tokens[1]));
                        modified = true;
                    }
                }
            }
        }

        private void LoadAircraft(INI ini, List<string> errors, ref bool modified)
        {
            // Classic game does not support this, so I'm leaving this out by default.
            // It is always extracted, so it doesn't end up with the "extra sections"
            INISection aircraftSection = ini.Sections.Extract("Aircraft");
            int amount = aircraftSection == null ? 0 : aircraftSection.Count();
            if (amount == 0)
            {
                return;
            }
            if (Globals.DisableAirUnits)
            {
                bool isOne = amount == 1;
                errors.Add(string.Format("Aircraft are disabled. {0} [Aircraft] {1} skipped. If you don't know why, please consult the manual's explanation of the \"DisableAirUnits\" setting.", amount, isOne ? "entry was" : "entries were"));
                modified = true;
                return;
            }
            foreach (KeyValuePair<string, string> kvp in aircraftSection)
            {
                string[] tokens = kvp.Value.Split(',');
                if (tokens.Length == 6)
                {
                    UnitType aircraftType = Map.AllUnitTypes.Where(t => t.IsAircraft && t.Equals(tokens[1])).FirstOrDefault();
                    if (aircraftType == null)
                    {
                        errors.Add(string.Format("Aircraft '{0}' references unknown aircraft.", tokens[1]));
                        modified = true;
                        continue;
                    }
                    if (!Map.BasicSection.ExpansionEnabled && aircraftType.IsExpansionOnly)
                    {
                        errors.Add(string.Format("Expansion aircraft '{0}' encountered, but expansion units are not enabled; enabling expansion units.", aircraftType.Name));
                        modified = true;
                        Map.BasicSection.ExpansionEnabled = true;
                    }
                    int strength;
                    if (!int.TryParse(tokens[2], out strength))
                    {
                        errors.Add(string.Format("Strength for aircraft '{0}' cannot be parsed; value: '{1}'; skipping.", aircraftType.Name, tokens[2]));
                        modified = true;
                        continue;
                    }
                    int cell;
                    if (!int.TryParse(tokens[3], out cell))
                    {
                        errors.Add(string.Format("Cell for aircraft '{0}' cannot be parsed; value: '{1}'; skipping.", aircraftType.Name, tokens[3]));
                        modified = true;
                        continue;
                    }
                    int dirValue;
                    if (!int.TryParse(tokens[4], out dirValue))
                    {
                        errors.Add(string.Format("Direction for aircraft '{0}' cannot be parsed; value: '{1}'; skipping.", aircraftType.Name, tokens[4]));
                        modified = true;
                        continue;
                    }
                    Unit newUnit = new Unit()
                    {
                        Type = aircraftType,
                        House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                        Strength = strength,
                        Direction = DirectionType.GetDirectionType(dirValue, Map.UnitDirectionTypes),
                        Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5])).FirstOrDefault() ?? Map.GetDefaultMission(aircraftType)
                    };
                    if (newUnit.House == null)
                    {
                        HouseType defHouse;
                        if ("ITALY".Equals(tokens[0], StringComparison.OrdinalIgnoreCase))
                        {
                            defHouse = HouseTypes.Ukraine;
                            errors.Add(string.Format("Aircraft '{0}' on cell {1} has obsolete house '{2}'; substituting with '{3}'.", newUnit.Type.Name, cell, tokens[0], defHouse.Name));
                        }
                        else
                        {
                            defHouse = Map.HouseTypes.First();
                            errors.Add(string.Format("Aircraft '{0}' on cell {1} references unknown house '{2}'; clearing to '{3}'.", newUnit.Type.Name, cell, tokens[0], defHouse.Name));
                        }
                        modified = true;
                        newUnit.House = defHouse;
                    }
                    if (!Map.Technos.Add(cell, newUnit))
                    {
                        ICellOccupier techno = Map.Technos[cell];
                        if (techno is Building building)
                        {
                            errors.Add(string.Format("Aircraft '{0}' overlaps structure '{1}' in cell {2}; skipping.", aircraftType.Name, building.Type.Name, cell));
                            modified = true;
                        }
                        else if (techno is Overlay overlay)
                        {
                            errors.Add(string.Format("Aircraft '{0}' overlaps overlay '{1}' in cell {2}; skipping.", aircraftType.Name, overlay.Type.Name, cell));
                            modified = true;
                        }
                        else if (techno is Terrain terrain)
                        {
                            errors.Add(string.Format("Aircraft '{0}' overlaps terrain '{1}' in cell {2}; skipping.", aircraftType.Name, terrain.Type.Name, cell));
                            modified = true;
                        }
                        else if (techno is InfantryGroup infantry)
                        {
                            errors.Add(string.Format("Aircraft '{0}' overlaps infantry in cell {1}; skipping.", aircraftType.Name, cell));
                            modified = true;
                        }
                        else if (techno is Unit unit)
                        {
                            errors.Add(string.Format("Aircraft '{0}' overlaps unit '{1}' in cell {2}; skipping.", aircraftType.Name, unit.Type.Name, cell));
                            modified = true;
                        }
                        else
                        {
                            errors.Add(string.Format("Aircraft '{0}' overlaps unknown techno in cell {1}; skipping.", aircraftType.Name, cell));
                            modified = true;
                        }
                    }
                }
                else
                {
                    if (tokens.Length < 2)
                    {
                        errors.Add(string.Format("Aircraft entry '{0}' has wrong number of tokens (expecting 6).", kvp.Key));
                        modified = true;
                    }
                    else
                    {
                        errors.Add(string.Format("Aircraft '{0}' has wrong number of tokens (expecting 6).", tokens[1]));
                        modified = true;
                    }
                }
            }
        }

        private void LoadShips(INI ini, Dictionary<string, string> caseTrigs, HashSet<string> checkUnitTrigs, List<string> errors, ref bool modified)
        {
            INISection shipsSection = ini.Sections.Extract("Ships");
            if (shipsSection == null)
            {
                return;
            }
            foreach (KeyValuePair<string, string> kvp in shipsSection)
            {
                string[] tokens = kvp.Value.Split(',');
                if (tokens.Length == 7)
                {
                    UnitType vesselType = Map.AllUnitTypes.Where(t => t.IsVessel && t.Equals(tokens[1])).FirstOrDefault();
                    if (vesselType == null)
                    {
                        errors.Add(string.Format("Ship '{0}' references unknown ship.", tokens[1]));
                        modified = true;
                        continue;
                    }
                    if (!Map.BasicSection.ExpansionEnabled && vesselType.IsExpansionOnly)
                    {
                        errors.Add(string.Format("Expansion ship '{0}' encountered, but expansion units are not enabled; enabling expansion units.", vesselType.Name));
                        modified = true;
                        Map.BasicSection.ExpansionEnabled = true;
                    }
                    int strength;
                    if (!int.TryParse(tokens[2], out strength))
                    {
                        errors.Add(string.Format("Strength for ship '{0}' cannot be parsed; value: '{1}'; skipping.", vesselType.Name, tokens[2]));
                        modified = true;
                        continue;
                    }
                    int cell;
                    if (!int.TryParse(tokens[3], out cell))
                    {
                        errors.Add(string.Format("Cell for ship '{0}' cannot be parsed; value: '{1}'; skipping.", vesselType.Name, tokens[3]));
                        modified = true;
                        continue;
                    }
                    int dirValue;
                    if (!int.TryParse(tokens[4], out dirValue))
                    {
                        errors.Add(string.Format("Direction for ship '{0}' cannot be parsed; value: '{1}'; skipping.", vesselType.Name, tokens[4]));
                        modified = true;
                        continue;
                    }
                    Unit newShip = new Unit()
                    {
                        Type = vesselType,
                        House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                        Strength = strength,
                        Direction = DirectionType.GetDirectionType(dirValue, Map.UnitDirectionTypes),
                        Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5])).FirstOrDefault() ?? Map.GetDefaultMission(vesselType),
                    };
                    if (newShip.House == null)
                    {
                        HouseType defHouse;
                        if ("ITALY".Equals(tokens[0], StringComparison.OrdinalIgnoreCase))
                        {
                            defHouse = HouseTypes.Ukraine;
                            errors.Add(string.Format("Ship '{0}' on cell {1} has obsolete house '{2}'; substituting with '{3}'.", newShip.Type.Name, cell, tokens[0], defHouse.Name));
                        }
                        else
                        {
                            defHouse = Map.HouseTypes.First();
                            errors.Add(string.Format("Ship '{0}' on cell {1} references unknown house '{2}'; clearing to '{3}'.", newShip.Type.Name, cell, tokens[0], defHouse.Name));
                        }
                        modified = true;
                        newShip.House = defHouse;
                    }
                    if (Map.Technos.Add(cell, newShip))
                    {
                        if (!caseTrigs.ContainsKey(tokens[6]))
                        {
                            errors.Add(string.Format("Ship '{0}' on cell {1} links to unknown trigger '{2}'; clearing trigger.", vesselType.Name, cell, tokens[6]));
                            modified = true;
                            newShip.Trigger = Trigger.None;
                        }
                        else if (!checkUnitTrigs.Contains(tokens[6]))
                        {
                            errors.Add(string.Format("Ship '{0}' on cell {1} links to trigger '{2}' which does not contain an event or action applicable to ships; clearing trigger.", vesselType.Name, cell, tokens[6]));
                            modified = true;
                            newShip.Trigger = Trigger.None;
                        }
                        else
                        {
                            // Adapt to same case
                            newShip.Trigger = caseTrigs[tokens[6]];
                        }
                    }
                    else
                    {
                        ICellOccupier techno = Map.Technos[cell];
                        if (techno is Building building)
                        {
                            errors.Add(string.Format("Ship '{0}' overlaps structure '{1}' in cell {2}; skipping.", vesselType.Name, building.Type.Name, cell));
                            modified = true;
                        }
                        else if (techno is Overlay overlay)
                        {
                            errors.Add(string.Format("Ship '{0}' overlaps overlay '{1}' in cell {2}; skipping.", vesselType.Name, overlay.Type.Name, cell));
                            modified = true;
                        }
                        else if (techno is Terrain terrain)
                        {
                            errors.Add(string.Format("Ship '{0}' overlaps terrain '{1}' in cell {2}; skipping.", vesselType.Name, terrain.Type.Name, cell));
                            modified = true;
                        }
                        else if (techno is InfantryGroup infantry)
                        {
                            errors.Add(string.Format("Ship '{0}' overlaps infantry in cell {1}; skipping.", vesselType.Name, cell));
                            modified = true;
                        }
                        else if (techno is Unit unit)
                        {
                            errors.Add(string.Format("Ship '{0}' overlaps unit '{1}' in cell {2}; skipping.", vesselType.Name, unit.Type.Name, cell));
                            modified = true;
                        }
                        else
                        {
                            errors.Add(string.Format("Ship '{0}' overlaps unknown techno in cell {1}; skipping.", vesselType.Name, cell));
                            modified = true;
                        }
                    }
                }
                else
                {
                    if (tokens.Length < 2)
                    {
                        errors.Add(string.Format("Ship entry '{0}' has wrong number of tokens (expecting 7).", kvp.Key));
                        modified = true;
                    }
                    else
                    {
                        errors.Add(string.Format("Ship '{0}' has wrong number of tokens (expecting 7).", tokens[1]));
                        modified = true;
                    }
                }
            }
        }

        private void LoadInfantry(INI ini, Dictionary<string, string> caseTrigs, HashSet<string> checkUnitTrigs, List<string> errors, ref bool modified)
        {
            INISection infantrySection = ini.Sections.Extract("Infantry");
            if (infantrySection == null)
            {
                return;
            }
            foreach (KeyValuePair<string, string> kvp in infantrySection)
            {
                string[] tokens = kvp.Value.Split(',');
                if (tokens.Length == 8)
                {
                    InfantryType infantryType = Map.AllInfantryTypes.Where(t => t.Equals(tokens[1])).FirstOrDefault();
                    if (infantryType == null)
                    {
                        errors.Add(string.Format("Infantry '{0}' references unknown infantry.", tokens[1]));
                        modified = true;
                        continue;
                    }
                    if (!Map.BasicSection.ExpansionEnabled && infantryType.IsExpansionOnly)
                    {
                        errors.Add(string.Format("Expansion infantry unit '{0}' encountered, but expansion units are not enabled; enabling expansion units.", infantryType.Name));
                        modified = true;
                        Map.BasicSection.ExpansionEnabled = true;
                    }
                    int strength;
                    if (!int.TryParse(tokens[2], out strength))
                    {
                        errors.Add(string.Format("Strength for infantry '{0}' cannot be parsed; value: '{1}'; skipping.", infantryType.Name, tokens[2]));
                        modified = true;
                        continue;
                    }
                    int cell;
                    if (!int.TryParse(tokens[3], out cell))
                    {
                        errors.Add(string.Format("Cell for infantry '{0}' cannot be parsed; value: '{1}'; skipping.", infantryType.Name, tokens[3]));
                        modified = true;
                        continue;
                    }
                    InfantryGroup infantryGroup = Map.Technos[cell] as InfantryGroup;
                    if ((infantryGroup == null) && (Map.Technos[cell] == null))
                    {
                        infantryGroup = new InfantryGroup();
                        Map.Technos.Add(cell, infantryGroup);
                    }
                    if (infantryGroup != null)
                    {
                        int stoppingPos;
                        if (!int.TryParse(tokens[4], out stoppingPos))
                        {
                            errors.Add(string.Format("Sub-position for infantry '{0}' on cell {1} cannot be parsed; value: '{2}'; skipping.", infantryType.Name, cell, tokens[4]));
                            modified = true;
                            continue;
                        }
                        if (stoppingPos < Globals.NumInfantryStops)
                        {
                            int dirValue;
                            if (!int.TryParse(tokens[6], out dirValue))
                            {
                                errors.Add(string.Format("Direction for infantry '{0}' on cell {1}, sub-position {2} cannot be parsed; value: '{3}'; skipping.", infantryType.Name, cell, stoppingPos, tokens[6]));
                                modified = true;
                                continue;
                            }
                            if (infantryGroup.Infantry[stoppingPos] == null)
                            {
                                if (!caseTrigs.ContainsKey(tokens[7]))
                                {
                                    errors.Add(string.Format("Infantry '{0}' on cell {1}, sub-position {2}  links to unknown trigger '{3}'; clearing trigger.", infantryType.Name, cell, stoppingPos, tokens[7]));
                                    modified = true;
                                    tokens[7] = Trigger.None;
                                }
                                else if (!checkUnitTrigs.Contains(tokens[7]))
                                {
                                    errors.Add(string.Format("Infantry '{0}' on cell {1}, sub-position {2}  links to trigger '{3}' which does not contain an event or action applicable to infantry; clearing trigger.", infantryType.Name, cell, stoppingPos, tokens[7]));
                                    modified = true;
                                    tokens[7] = Trigger.None;
                                }
                                else
                                {
                                    // Adapt to same case
                                    tokens[7] = caseTrigs[tokens[7]];
                                }
                                Infantry inf = new Infantry(infantryGroup)
                                {
                                    Type = infantryType,
                                    House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                                    Strength = strength,
                                    Direction = DirectionType.GetDirectionType(dirValue, Map.UnitDirectionTypes),
                                    Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5])).FirstOrDefault() ?? Map.GetDefaultMission(infantryType),
                                    Trigger = tokens[7]
                                };
                                infantryGroup.Infantry[stoppingPos] = inf;
                                if (inf.House == null)
                                {
                                    HouseType defHouse;
                                    if ("ITALY".Equals(tokens[0], StringComparison.OrdinalIgnoreCase))
                                    {
                                        defHouse = HouseTypes.Ukraine;
                                        errors.Add(string.Format("Infantry '{0}' on cell {1}, sub-position {2} has obsolete house '{3}'; substituting with '{4}'.", inf.Type.Name, cell, stoppingPos, tokens[0], defHouse.Name));
                                    }
                                    else
                                    {
                                        defHouse = Map.HouseTypes.First();
                                        errors.Add(string.Format("Infantry '{0}' on cell {1}, sub-position {2} references unknown house '{3}'; clearing to '{4}'.", inf.Type.Name, cell, stoppingPos, tokens[0], defHouse.Name));
                                    }
                                    modified = true;
                                    inf.House = defHouse;
                                }
                            }
                            else
                            {
                                errors.Add(string.Format("Infantry '{0}' overlaps another infantry at position {1} in cell {2}; skipping.", infantryType.Name, stoppingPos, cell));
                                modified = true;
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Infantry '{0}' has invalid position {1} in cell {2}; skipping.", infantryType.Name, stoppingPos, cell));
                            modified = true;
                        }
                    }
                    else
                    {
                        ICellOccupier techno = Map.Technos[cell];
                        if (techno is Building building)
                        {
                            errors.Add(string.Format("Infantry '{0}' overlaps structure '{1}' in cell {2}; skipping.", infantryType.Name, building.Type.Name, cell));
                            modified = true;
                        }
                        else if (techno is Overlay overlay)
                        {
                            errors.Add(string.Format("Infantry '{0}' overlaps overlay '{1}' in cell {2}; skipping.", infantryType.Name, overlay.Type.Name, cell));
                            modified = true;
                        }
                        else if (techno is Terrain terrain)
                        {
                            errors.Add(string.Format("Infantry '{0}' overlaps terrain '{1}' in cell {2}; skipping.", infantryType.Name, terrain.Type.Name, cell));
                            modified = true;
                        }
                        else if (techno is Unit unit)
                        {
                            errors.Add(string.Format("Infantry '{0}' overlaps unit '{1}' in cell {2}; skipping.", infantryType.Name, unit.Type.Name, cell));
                            modified = true;
                        }
                        else
                        {
                            errors.Add(string.Format("Infantry '{0}' overlaps unknown techno in cell {1}; skipping.", infantryType.Name, cell));
                            modified = true;
                        }
                    }
                }
                else
                {
                    if (tokens.Length < 2)
                    {
                        errors.Add(string.Format("Infantry entry '{0}' has wrong number of tokens (expecting 8).", kvp.Key));
                        modified = true;
                    }
                    else
                    {
                        errors.Add(string.Format("Infantry '{0}' has wrong number of tokens (expecting 8).", tokens[1]));
                        modified = true;
                    }
                }
            }
        }

        private void LoadStructures(INI ini, Dictionary<string, string> caseTrigs, HashSet<string> checkStrcTrigs, List<string> errors, ref bool modified)
        {
            INISection structuresSection = ini.Sections.Extract("Structures");
            if (structuresSection == null)
            {
                return;
            }
            foreach (KeyValuePair<string, string> kvp in structuresSection)
            {
                string[] tokens = kvp.Value.Split(',');
                if (tokens.Length <= 5)
                {
                    if (tokens.Length < 2)
                    {
                        errors.Add(string.Format("Structure entry '{0}' has wrong number of tokens (expecting 6).", kvp.Key));
                        modified = true;
                    }
                    else
                    {
                        errors.Add(string.Format("Structure '{0}' has wrong number of tokens (expecting 6).", tokens[1]));
                        modified = true;
                    }
                    continue;
                }
                BuildingType buildingType = Map.BuildingTypes.Where(t => t.Equals(tokens[1])).FirstOrDefault();
                if (buildingType == null)
                {
                    errors.Add(string.Format("Structure '{0}' references unknown structure.", tokens[1]));
                    modified = true;
                    continue;
                }
                if (Globals.FilterTheaterObjects && buildingType.IsTheaterDependent && !buildingType.ExistsInTheater)
                {
                    errors.Add(string.Format("Structure '{0}' is not available in the set theater; skipping.", buildingType.Name));
                    modified = true;
                    continue;
                }
                int strength;
                if (!int.TryParse(tokens[2], out strength))
                {
                    errors.Add(string.Format("Strength for structure '{0}' cannot be parsed; value: '{1}'; skipping.", buildingType.Name, tokens[2]));
                    modified = true;
                    continue;
                }
                int cell;
                if (!int.TryParse(tokens[3], out cell))
                {
                    errors.Add(string.Format("Cell for structure '{0}' cannot be parsed; value: '{1}'; skipping.", buildingType.Name, tokens[3]));
                    modified = true;
                    continue;
                }
                int dirValue;
                if (!int.TryParse(tokens[4], out dirValue))
                {
                    errors.Add(string.Format("Direction for structure '{0}' cannot be parsed; value: '{1}'; skipping.", buildingType.Name, tokens[4]));
                    modified = true;
                    continue;
                }
                bool sellable = (tokens.Length > 6) && int.TryParse(tokens[6], out int sell) && sell != 0;
                bool rebuild = (tokens.Length > 7) && int.TryParse(tokens[7], out int rebld) && rebld != 0;
                Building newBld = new Building()
                {
                    Type = buildingType,
                    House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                    Strength = strength,
                    Direction = DirectionType.GetDirectionType(dirValue, Map.BuildingDirectionTypes),
                    Sellable = sellable,
                    Rebuild = rebuild
                };
                if (newBld.House == null)
                {
                    HouseType defHouse;
                    if ("ITALY".Equals(tokens[0], StringComparison.OrdinalIgnoreCase))
                    {
                        defHouse = RedAlert.HouseTypes.Ukraine;
                        errors.Add(string.Format("Structure '{0}' on cell {1} has obsolete house '{2}'; substituting with '{3}'.", buildingType.Name, cell, tokens[0], defHouse.Name));
                    }
                    else
                    {
                        defHouse = Map.HouseTypes.First();
                        errors.Add(string.Format("Structure '{0}' on cell {1} references unknown house '{2}'; clearing to '{3}'.", buildingType.Name, cell, tokens[0], defHouse.Name));
                    }
                    modified = true;
                    newBld.House = defHouse;
                }
                if (!Map.Buildings.CanAdd(cell, newBld) || !Map.Technos.CanAdd(cell, newBld, newBld.Type.BaseOccupyMask))
                {
                    Map.CheckBuildingBlockingCell(cell, buildingType, errors, ref modified);
                    continue;
                }
                Map.Buildings.Add(cell, newBld);
                if (!caseTrigs.ContainsKey(tokens[5]))
                {
                    errors.Add(string.Format("Structure '{0}' on cell {1} links to unknown trigger '{2}'; clearing trigger.", buildingType.Name, cell, tokens[5]));
                    modified = true;
                    newBld.Trigger = Trigger.None;
                }
                else if (!checkStrcTrigs.Contains(tokens[5]))
                {
                    errors.Add(string.Format("Structure '{0}' on cell {1} links to trigger '{2}' which does not contain an event or action applicable to structures; clearing trigger.", buildingType.Name, cell, tokens[5]));
                    modified = true;
                    newBld.Trigger = Trigger.None;
                }
                else
                {
                    // Adapt to same case
                    newBld.Trigger = caseTrigs[tokens[5]];
                }
            }
        }

        private void LoadBase(INI ini, List<string> errors, ref bool modified)
        {
            INISection baseSection = ini.Sections["Base"];
            string baseCountStr = null;
            HouseType basePlayer = Map.HouseTypes.First();
            if (baseSection != null)
            {
                baseCountStr = baseSection.TryGetValue("Count");
                baseSection.Remove("Count");
                string basePlayerStr = baseSection.TryGetValue("Player");
                baseSection.Remove("Player");
                if (basePlayerStr != null)
                {
                    HouseType basePlayerLookup = Map.HouseTypes.Where(t => t.Equals(basePlayerStr)).FirstOrDefault();
                    if (basePlayerLookup != null)
                    {
                        basePlayer = basePlayerLookup;
                    }
                }
            }
            Map.BasicSection.BasePlayer = basePlayer.Name;
            // if it's just an empty [Base] header with nothing below, ignore.
            if (baseSection == null)
            {
                return;
            }
            if (baseSection.Keys.Count == 0)
            {
                CleanBaseSection(ini, baseSection);
                return;
            }
            if (!Int32.TryParse(baseCountStr, out int baseCount))
            {
                errors.Add(string.Format("Base count '{0}' is not a valid integer.", baseCountStr));
                modified = true;
                CleanBaseSection(ini, baseSection);
                return;
            }
            int curPriorityVal = 0;
            for (int i = 0; i < baseCount; i++)
            {
                string key = i.ToString("D3");
                string value = baseSection.TryGetValue(key);
                if (value == null)
                {
                    continue;
                }
                baseSection.Remove(key);
                string[] tokens = value.Split(',');
                if (tokens.Length != 2)
                {
                    errors.Add(string.Format("Base rebuild entry {0} has wrong number of tokens (expecting 2).", key));
                    modified = true;
                    continue;
                }
                BuildingType buildingType = Map.BuildingTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault();
                if (buildingType == null)
                {
                    errors.Add(string.Format("Base rebuild entry {0} references unknown structure '{1}'.", key, tokens[0]));
                    modified = true;
                    continue;
                }
                if (Globals.FilterTheaterObjects && buildingType.IsTheaterDependent && !buildingType.ExistsInTheater)
                {
                    errors.Add(string.Format("Base rebuild entry {0} references structure '{1}' which is not available in the set theater; skipping.", key, buildingType.Name));
                    modified = true;
                    continue;
                }
                int cell;
                if (!int.TryParse(tokens[1], out cell))
                {
                    errors.Add(string.Format("Cell for base rebuild entry '{0}', structure '{1}' cannot be parsed; value: '{2}'; skipping.", key, buildingType.Name, tokens[1]));
                    modified = true;
                    continue;
                }
                Map.Metrics.GetLocation(cell, out Point location);
                if (Map.Buildings.OfType<Building>().Where(x => x.Location == location && x.Occupier.Type.ID == buildingType.ID).FirstOrDefault().Occupier is Building building)
                {
                    if (building.BasePriority == -1)
                    {
                        building.BasePriority = curPriorityVal++;
                    }
                    else
                    {
                        errors.Add(string.Format("Duplicate base rebuild entry for structure '{0}' on cell '{1}'; skipping.", buildingType.Name, cell));
                    }
                    continue;
                }
                Map.Buildings.Add(cell, new Building()
                {
                    Type = buildingType,
                    House = basePlayer,
                    Strength = 256,
                    Direction = DirectionTypes.North,
                    BasePriority = curPriorityVal++,
                    IsPrebuilt = false
                });
            }
            foreach (KeyValuePair<string, string> kvp in baseSection)
            {
                if (baseKeyRegex.IsMatch(kvp.Key))
                {
                    errors.Add(string.Format("Base rebuild priority entry with key '{0}' exceeds count; skipping.", kvp.Key));
                    modified = true;
                }
                // non-matches will be ignored as potential modded content.
            }
            CleanBaseSection(ini, baseSection);
        }

        protected void CleanBaseSection(INI ini, INISection baseSection)
        {
            // Clean out and leave; might contain addon keys.
            baseSection.Remove("Player");
            baseSection.Remove("Count");
            baseSection.RemoveWhere(k => baseKeyRegex.IsMatch(k));
            if (baseSection.Count == 0)
            {
                ini.Sections.Remove(baseSection.Name);
            }
        }

        private void LoadTerrain(INI ini, List<string> errors, ref bool modified)
        {
            string th = Map.Theater.Name;
            INISection terrainSection = ini.Sections.Extract("Terrain");
            if (terrainSection == null)
            {
                return;
            }
            foreach (KeyValuePair<string, string> kvp in terrainSection)
            {
                int cell;
                if (!int.TryParse(kvp.Key, out cell))
                {
                    errors.Add(string.Format("Cell for terrain cannot be parsed. Key: '{0}', value: '{1}'; skipping.", kvp.Key, kvp.Value));
                    modified = true;
                    continue;
                }
                string name = kvp.Value.Split(',')[0];
                TerrainType terrainType = Map.TerrainTypes.Where(t => t.Equals(name)).FirstOrDefault();
                if (terrainType != null)
                {
                    if (Globals.FilterTheaterObjects && !terrainType.ExistsInTheater)
                    {
                        errors.Add(string.Format("Terrain '{0}' is not available in the set theater; skipping.", terrainType.Name));
                        modified = true;
                        continue;
                    }
                    Terrain newTerr = new Terrain
                    {
                        Type = terrainType,
                        Trigger = Trigger.None
                    };
                    if (!Map.Technos.Add(cell, newTerr))
                    {
                        ICellOccupier techno = Map.FindBlockingObject(cell, terrainType, out int blockingCell, out int placementCell);
                        string reportCell = blockingCell == -1 ? "<unknown>" : blockingCell.ToString();
                        if (techno is Building building)
                        {
                            errors.Add(string.Format("Terrain '{0}' on cell {1} overlaps structure '{2}' placed on cell {3} at cell {4}; skipping.", terrainType.Name, cell, building.Type.Name, placementCell, reportCell));
                            modified = true;
                        }
                        else if (techno is Overlay overlay)
                        {
                            errors.Add(string.Format("Terrain '{0}' on cell {1} overlaps overlay '{2}' at cell {3}; skipping.", terrainType.Name, cell, overlay.Type.Name, reportCell));
                            modified = true;
                        }
                        else if (techno is Terrain terrain)
                        {
                            errors.Add(string.Format("Terrain '{0}' on cell {1} overlaps terrain '{2}' placed on cell {3} at cell {4}; skipping.", terrainType.Name, cell, terrain.Type.Name, placementCell, reportCell));
                            modified = true;
                        }
                        else if (techno is InfantryGroup infantry)
                        {
                            errors.Add(string.Format("Terrain '{0}' on cell {1} overlaps infantry at cell {2}; skipping.", terrainType.Name, cell, reportCell));
                            modified = true;
                        }
                        else if (techno is Unit unit)
                        {
                            errors.Add(string.Format("Terrain '{0}' on cell {1} overlaps unit '{2}' at cell {3}; skipping.", terrainType.Name, cell, unit.Type.Name, reportCell));
                            modified = true;
                        }
                        else
                        {
                            if (blockingCell != -1)
                            {
                                errors.Add(string.Format("Terrain '{0}' placed on cell {1} overlaps unknown techno at cell {2}; skipping.", terrainType.Name, cell, blockingCell));
                                modified = true;
                            }
                            else
                            {
                                errors.Add(string.Format("Terrain '{0}' placed on cell {1} crosses outside the map bounds; skipping.", terrainType.Name, cell));
                            }
                        }
                    }
                }
                else
                {
                    errors.Add(string.Format("Terrain '{0}' references unknown terrain.", name));
                    modified = true;
                }
            }
        }

        private void LoadOverlay(INI ini, List<string> errors, ref bool modified)
        {
            INISection overlayPackSection = ini.Sections.Extract("OverlayPack");
            if (overlayPackSection == null)
            {
                return;
            }
            Map.Overlay.Clear();
            byte[] data = DecompressLCWSection(overlayPackSection, 1, errors, ref modified);
            if (data == null)
            {
                return;
            }
            int secondRow = Map.Metrics.Width;
            int lastRow = Map.Metrics.Length - Map.Metrics.Width;
            for (int i = 0; i < Map.Metrics.Length; ++i)
            {
                byte overlayId = data[i];
                // Technically signed, so filter out negative values. This makes it skip over empty entries without error, since they should be FF.
                if ((overlayId & 0x80) != 0)
                {
                    continue;
                }
                OverlayType overlayType = Map.OverlayTypes.Where(t => t.Equals(overlayId)).FirstOrDefault();
                if (overlayType == null)
                {
                    if (i < secondRow || i >= lastRow)
                    {
                        errors.Add(string.Format("Overlay can not be placed on the first and last lines of the map. Cell: '{0}', Id: '{1}' (unknown); skipping.", i, overlayId));
                        modified = true;
                    }
                    else
                    {
                        errors.Add(string.Format("Overlay ID {0} references unknown overlay.", overlayId));
                        modified = true;
                    }
                    continue;
                }
                if (i < secondRow || i >= lastRow)
                {
                    errors.Add(string.Format("Overlay can not be placed on the first and last lines of the map. Cell: '{0}', Type: '{1}'; skipping.", i, overlayType.Name));
                    modified = true;
                    continue;
                }
                if (Globals.FilterTheaterObjects && !overlayType.ExistsInTheater)
                {
                    errors.Add(string.Format("Overlay '{0}' is not available in the set theater; skipping.", overlayType.Name));
                    modified = true;
                    continue;
                }
                ICellOccupier techno = Map.Technos[i];
                if ((overlayType.IsWall || overlayType.IsSolid) && techno != null)
                {
                    string desc = overlayType.IsWall ? "Wall" : "Solid overlay";
                    if (techno is Building building)
                    {
                        errors.Add(string.Format("{0} '{1}' overlaps structure '{2}' at cell {3}; skipping.", desc, overlayType.Name, building.Type.Name, i));
                        modified = true;
                    }
                    else if (techno is Terrain terrain)
                    {
                        errors.Add(string.Format("{0} '{1}' overlaps terrain '{2}' at cell {3}; skipping.", desc, overlayType.Name, terrain.Type.Name, i));
                        modified = true;
                    }
                    else if (techno is Unit unit)
                    {
                        errors.Add(string.Format("{0} '{1}' overlaps unit '{2}' at cell {3}; skipping.", desc, overlayType.Name, unit.Type.Name, i));
                        modified = true;
                    }
                    else if (techno is InfantryGroup igrp)
                    {
                        errors.Add(string.Format("{0} '{1}' overlaps infantry at cell {2}; skipping.", desc, overlayType.Name, i));
                        modified = true;
                    }
                    else
                    {
                        errors.Add(string.Format("{0} '{1}' overlaps unknown techno in cell {2}; skipping.", desc, overlayType.Name, i));
                        modified = true;
                    }
                    continue;
                }
                Map.Overlay[i] = new Overlay { Type = overlayType, Icon = 0 };
            }
        }

        private void LoadWaypoints(INI ini, List<string> errors, ref bool modified)
        {
            INISection waypointsSection = ini.Sections.Extract("Waypoints");
            if (waypointsSection == null || waypointsSection.Count == 0)
            {
                return;
            }
            foreach (KeyValuePair<string, string> kvp in waypointsSection)
            {
                if (!int.TryParse(kvp.Key, out int waypoint))
                {
                    errors.Add(string.Format("Invalid waypoint '{0}' (expecting integer).", kvp.Key));
                    modified = true;
                    continue;
                }
                if (waypoint != 0 && kvp.Key.StartsWith("0"))
                {
                    errors.Add(string.Format("Waypoint '{0}' is zero-padded and will never be read by the game. Skipping.", kvp.Key));
                    continue;
                }
                if (!int.TryParse(kvp.Value, out int cell))
                {
                    errors.Add(string.Format("Waypoint {0} has invalid cell '{1}' (expecting integer).", waypoint, kvp.Value));
                    modified = true;
                    continue;
                }
                if (waypoint < 0 || waypoint >= Map.Waypoints.Length)
                {
                    // don't bother reporting illegal-but-empty entries.
                    if (cell != -1)
                    {
                        errors.Add(string.Format("Waypoint {0} out of range (expecting between {1} and {2}).", waypoint, 0, Map.Waypoints.Length - 1));
                        modified = true;
                    }
                }
                else if (!Map.Metrics.Contains(cell))
                {
                    Map.Waypoints[waypoint].Cell = null;
                    // don't bother reporting illegal-but-empty entries.
                    if (cell != -1)
                    {
                        errors.Add(string.Format("Waypoint {0} cell value {1} out of range (expecting between {2} and {3}).", waypoint, cell, 0, Map.Metrics.Length - 1));
                        modified = true;
                    }
                }
                else
                {
                    Map.Waypoints[waypoint].Cell = cell;
                }
            }
        }

        private void LoadCellTriggers(INI ini, Dictionary<string, string> caseTrigs, HashSet<string> checkCellTrigs, List<string> errors, ref bool modified)
        {
            INISection cellTriggersSection = ini.Sections.Extract("CellTriggers");
            if (cellTriggersSection == null)
            {
                return;
            }
            foreach (KeyValuePair<string, string> kvp in cellTriggersSection)
            {
                if (int.TryParse(kvp.Key, out int cell))
                {
                    if (Map.Metrics.Contains(cell))
                    {
                        if (!caseTrigs.ContainsKey(kvp.Value))
                        {
                            errors.Add(string.Format("Cell trigger {0} links to unknown trigger '{1}'; skipping.", cell, kvp.Value));
                            modified = true;
                        }
                        else if (!checkCellTrigs.Contains(kvp.Value))
                        {
                            errors.Add(string.Format("Cell trigger {0} links to trigger '{1}' which does not contain a placeable event; skipping.", cell, kvp.Value));
                            modified = true;
                        }
                        else
                        {
                            Map.CellTriggers[cell] = new CellTrigger(caseTrigs[kvp.Value]);
                        }
                    }
                    else
                    {
                        errors.Add(string.Format("Cell trigger {0} outside map bounds; skipping.", cell));
                        modified = true;
                    }
                }
                else
                {
                    errors.Add(string.Format("Invalid cell trigger '{0}' (expecting integer).", kvp.Key));
                    modified = true;
                }
            }
        }

        private void LoadBriefing(INI ini, List<string> errors, ref bool modified)
        {
            INISection briefingSection = ini.Sections["Briefing"];
            if (briefingSection == null)
            {
                return;
            }
            // Remove all spaces before and after the line breaks.
            Regex breaksWithTrim = new Regex("[ \t]*@[ \t]*");
            if (briefingSection.Keys.Contains("Text"))
            {
                Map.BriefingSection.Briefing = breaksWithTrim.Replace(briefingSection["Text"], Environment.NewLine);
            }
            else
            {
                List<string> briefLines = new List<string>();
                int line = 1;
                string lineStr;
                // Only take consecutive numbers; if one is missing, abort.
                while (briefingSection.Keys.Contains(lineStr = line.ToString()))
                {
                    briefLines.Add(briefingSection[lineStr].Trim());
                    line++;
                }
                Map.BriefingSection.Briefing = breaksWithTrim.Replace(String.Join(" ", briefLines), Environment.NewLine);
            }
            briefingSection.Remove("Text");
            briefingSection.RemoveWhere(k => Regex.IsMatch(k, "^\\d+$"));
            if (briefingSection.Keys.Count == 0)
            {
                ini.Sections.Remove("Briefing");
            }
        }

        private void LoadHouses(INI ini, List<string> errors, ref bool modified)
        {
            Dictionary<string, string> correctedEdges = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string edge in Globals.MapEdges)
                correctedEdges.Add(edge, edge);
            string defaultEdge = Globals.MapEdges.FirstOrDefault() ?? string.Empty;
            foreach (Model.House house in Map.Houses)
            {
                if (house.Type.Flags.HasFlag(HouseTypeFlag.Special))
                {
                    continue;
                }
                House gameHouse = (House)house;
                ParseHouseSection(ini, gameHouse, gameHouse.Type.Name, correctedEdges, defaultEdge, errors, ref modified);
            }
            House ukr = Map.Houses.Where(h => h.Type == HouseTypes.Ukraine).FirstOrDefault() as House;
            const string italySectionName = "ITALY";
            if (ukr != null && !ukr.Enabled && ini.Sections.Contains(italySectionName))
            {
                List<string> itaErrors = new List<string>();
                bool itaMod = false;
                INISection houseSection = ParseHouseSection(ini, ukr, italySectionName, correctedEdges, defaultEdge, itaErrors, ref itaMod);
                string secName = houseSection.Name;
                // Will only succeed if anything remained in the house section.
                ini.Sections.Rename(italySectionName, ukr.Type.Name);
                errors.Add(string.Format("Obsolete house section '{0}' found, and its modern counterpart '{1}' is not present. Interpreting section as '{1}'.", secName, ukr.Type.Name));
                if (itaErrors.Count > 0)
                {
                    // add these after the detection message.
                    errors.AddRange(itaErrors);
                    if (itaMod)
                    {
                        modified = true;
                    }
                }
            }
        }

        private INISection ParseHouseSection(INI ini, House house, string section, Dictionary<string, string> correctedEdges, string defaultEdge, List<string> errors, ref bool modified)
        {
            List<(string, string)> newErrors = new List<(string, string)>();
            INISection houseSection = INITools.ParseAndLeaveRemainder(ini, section, house, new MapContext(Map, true), newErrors);
            if (newErrors.Count > 0)
            {
                modified = true;
                foreach ((string key, string err) in newErrors)
                {
                    errors.Add(String.Format("Error parsing key {0} of house {1}: {2}", key, house.Type.Name, err));
                }
            }
            if (!correctedEdges.ContainsKey(house.Edge))
            {
                errors.Add(String.Format("House {0} has an unknown edge value '{1}'; reverting to {2}", house.Type.Name, house.Edge, defaultEdge));
                house.Edge = defaultEdge;
            }
            else
            {
                house.Edge = correctedEdges[house.Edge];
            }
            house.Enabled = houseSection != null;
            return houseSection;
        }

        private void LinkTriggersAndTeams(List<Trigger> triggers, List<TeamType> teamTypes, HashSet<string> checkUnitTrigs, List<string> errors, ref bool modified)
        {
            string indexToName<T>(IList<T> list, string index, string defaultValue) where T : INamedType
            {
                return (int.TryParse(index, out int result) && (result >= 0) && (result < list.Count)) ? list[result].Name : defaultValue;
            }
            foreach (TeamType teamType in teamTypes)
            {
                string trigName = indexToName(triggers, teamType.Trigger, Trigger.None);
                if (!checkUnitTrigs.Contains(trigName))
                {
                    errors.Add(string.Format("Team '{0}' links to trigger '{1}' which does not contain an event or action applicable to units; clearing trigger.", teamType.Name, trigName));
                    modified = true;
                    teamType.Trigger = Trigger.None;
                }
                else
                {
                    teamType.Trigger = trigName;
                }
            }
            foreach (Trigger trigger in triggers)
            {
                trigger.Event1.Team = indexToName(teamTypes, trigger.Event1.Team, TeamType.None);
                trigger.Event2.Team = indexToName(teamTypes, trigger.Event2.Team, TeamType.None);
                trigger.Action1.Team = indexToName(teamTypes, trigger.Action1.Team, TeamType.None);
                trigger.Action1.Trigger = indexToName(triggers, trigger.Action1.Trigger, Trigger.None);
                trigger.Action2.Team = indexToName(teamTypes, trigger.Action2.Team, TeamType.None);
                trigger.Action2.Trigger = indexToName(triggers, trigger.Action2.Trigger, Trigger.None);
            }
        }

        /// <summary>
        /// Checks the globals used in triggers and teamtypes, and if any out-of-range values are used,
        /// attempts to find unused ones, and substitutes all usages of these illegal values by unused ones.
        /// </summary>
        /// <param name="triggers">List of all read triggers</param>
        /// <param name="teamTypes">List of all read team types</param>
        /// <param name="errors">List to add errors to.</param>
        /// <param name="modified">Returns true if any fixes were made.</param>
        private void CheckTriggersGlobals(List<Trigger> triggers, List<TeamType> teamTypes, List<string> errors, ref bool modified, HouseType defaultHouse)
        {
            // Keep track of corrected globals.
            List<int> availableGlobals;
            Dictionary<long, int> fixedGlobals = new Dictionary<long, int>();
            HashSet<int> teamGlobals = GetTeamGlobals(teamTypes);
            bool wasFixed;
            errors.AddRange(CheckTriggers(triggers, true, true, false, out _, true, out wasFixed, teamGlobals, out availableGlobals, ref fixedGlobals, defaultHouse));
            if (wasFixed)
            {
                modified = true;
            }
            errors.AddRange(FixTeamTypeGlobals(Map.TeamTypes, availableGlobals, fixedGlobals, out wasFixed));
            if (wasFixed)
            {
                modified = true;
            }
        }

        private bool CheckSwitchToSolo(bool tryCheckSoloMission, bool dontReportSwitch, List<Trigger> triggers, bool wasSolo, HouseType player, List<string> errors)
        {
            bool switchedToSolo = false;
            if (tryCheckSoloMission && !wasSolo)
            {
                int playerId = player.ID;
                bool hasWinTrigger =
                    triggers.Any(t => t.Action1.ActionType == ActionTypes.TACTION_WIN && t.Action1.Data == playerId) ||
                    triggers.Any(t => t.Action1.ActionType == ActionTypes.TACTION_LOSE && t.Action1.Data != playerId) ||
                    triggers.Any(t => t.Action2.ActionType == ActionTypes.TACTION_WIN && t.Action2.Data == playerId) ||
                    triggers.Any(t => t.Action2.ActionType == ActionTypes.TACTION_LOSE && t.Action2.Data != playerId);
                bool hasLoseTrigger =
                    triggers.Any(t => t.Action1.ActionType == ActionTypes.TACTION_LOSE && t.Action1.Data == playerId) ||
                    triggers.Any(t => t.Action1.ActionType == ActionTypes.TACTION_WIN && t.Action1.Data != playerId) ||
                    triggers.Any(t => t.Action2.ActionType == ActionTypes.TACTION_LOSE && t.Action2.Data == playerId) ||
                    triggers.Any(t => t.Action2.ActionType == ActionTypes.TACTION_WIN && t.Action2.Data != playerId);
                switchedToSolo = hasWinTrigger && hasLoseTrigger;
            }
            bool isSolo = wasSolo || switchedToSolo;
            if (switchedToSolo)
            {
                Map.BasicSection.SoloMission = true;
                if (errors != null && ((!dontReportSwitch && Globals.ReportMissionDetection) || errors.Count > 0))
                {
                    errors.Insert(0, "Filename detected as classic single player mission format, and win and lose trigger detected. Applying \"SoloMission\" flag.");
                }
            }
            return isSolo;
        }

        private INI ReadRulesFile(byte[] rulesFile)
        {
            if (rulesFile == null)
            {
                return null;
            }
            Encoding encDOS = Encoding.GetEncoding(437);
            string iniText = encDOS.GetString(rulesFile);
            INI ini = new INI();
            ini.Parse(iniText);
            return ini;
        }

        private bool FixCorruptTiles(Template template, byte iconValue, out byte newIconValue, out string type)
        {
            TemplateType templateType = template.Type;
            bool isFixed = false;
            type = null;
            newIconValue = iconValue;
            const string bridgeType = "bridge";
            const string shoreType = "shore";
            if (templateType == TemplateTypes.Bridge1x)
            {
                type = bridgeType;
                templateType = TemplateTypes.Bridge1;
                // Shift up one line
                newIconValue -= 5;
                isFixed = true;
            }
            else if (templateType == TemplateTypes.Bridge1)
            {
                type = bridgeType;
                templateType = TemplateTypes.Bridge1x;
                // Shift down one line
                newIconValue += 5;
                isFixed = true;
            }
            else if (templateType == TemplateTypes.Bridge2x)
            {
                type = bridgeType;
                templateType = TemplateTypes.Bridge2;
                // Shift up one line
                newIconValue -= 5;
                isFixed = true;
            }
            else if (templateType == TemplateTypes.Bridge2)
            {
                type = bridgeType;
                templateType = TemplateTypes.Bridge2x;
                // Shift down one line
                newIconValue += 5;
                isFixed = true;
            }
            else if (templateType == TemplateTypes.Bridge1a)
            {
                type = bridgeType;
                templateType = TemplateTypes.Bridge1ax;
                isFixed = true;
            }
            else if (templateType == TemplateTypes.Bridge1ax)
            {
                type = bridgeType;
                templateType = TemplateTypes.Bridge1a;
                // Align to new width
                if (newIconValue >= 5)
                    newIconValue--;
                if (newIconValue >= 10)
                    newIconValue--;
                isFixed = true;
            }
            else if (templateType == TemplateTypes.Bridge2a)
            {
                type = bridgeType;
                templateType = TemplateTypes.Bridge2ax;
                // Shift up two lines
                newIconValue -= 10;
                isFixed = true;
            }
            else if (templateType == TemplateTypes.Bridge2ax)
            {
                type = bridgeType;
                templateType = TemplateTypes.Bridge2a;
                // Shift down two lines
                newIconValue += 10;
                isFixed = true;
            }
            else if (templateType == TemplateTypes.ShoreCliff32)
            {
                type = shoreType;
                // Reset to water
                templateType = TemplateTypes.Water;
                newIconValue = 0;
                isFixed = true;
            }
            else if (templateType == TemplateTypes.Shore42)
            {
                type = shoreType;
                // Reset to water
                templateType = TemplateTypes.Water;
                newIconValue = 0;
                isFixed = true;
            }
            template.Type = templateType;
            return isFixed && newIconValue >= 0 && newIconValue < templateType.NumIcons
                && (templateType.IconMask == null || templateType.IconMask[newIconValue / templateType.IconWidth, newIconValue % templateType.IconWidth]);
        }

        /// <summary>
        /// Update rules according to the information in the given ini file.
        /// </summary>
        /// <param name="ini">ini file</param>
        /// <param name="map">Current map; used for ini parsing.</param>
        /// <param name="forFootprintTest">Run in test mode, where bibs are changed but nothing is actually updated on the map.</param>
        /// <param name="footPrintsChanged">Returns true of the rule changes modified any building footprint sizes.</param>
        /// <returns>Any errors returned by the parsing process.</returns>
        private List<string> UpdateRules(INI ini, Map map, bool forFootprintTest)
        {
            List<string> errors = new List<string>();
            if (ini == null)
            {
                errors.Add("Rules file is null!");
            }
            else
            {
                if (!forFootprintTest)
                {
                    errors.AddRange(this.UpdateLandTypeRules(ini, map));
                    errors.AddRange(this.UpdateGeneralRules(ini, map));
                }
                errors.AddRange(UpdateBuildingRules(ini, map, forFootprintTest));
            }
            return errors;
        }

        private List<string> UpdateLandTypeRules(INI ini, Map map)
        {
            List<string> errors = new List<string>();
            this.ReadLandType(ini, map, "Clear", LandClear, errors);
            this.ReadLandType(ini, map, "Rough", LandRough, errors);
            this.ReadLandType(ini, map, "Road", LandRoad, errors);
            this.ReadLandType(ini, map, "Water", LandWater, errors);
            this.ReadLandType(ini, map, "Rock", LandRock, errors);
            this.ReadLandType(ini, map, "Beach", LandBeach, errors);
            this.ReadLandType(ini, map, "River", LandRiver, errors);
            return errors;
        }

        private void ReadLandType(INI ini, Map map, string landType, LandIniSection landRules, List<string> errors)
        {
            if (ini == null || landRules == null)
            {
                return;
            }
            INISection landIni = ini[landType];
            if (landIni == null)
            {
                return;
            }
            try
            {
                List<(string, string)> parseErrors = INI.ParseSection(new MapContext(map, false), landIni, landRules, true);
                if (errors != null)
                {
                    foreach ((string iniKey, string error) in parseErrors)
                    {
                        errors.Add("Custom rules error on [" + landType + "]: " + error.TrimEnd('.') + ". Value for \"" + iniKey + "\" is ignored.");
                    }
                }
            }
            catch (Exception e)
            {
                if (errors != null)
                {
                    // Normally won't happen with the aforementioned system.
                    errors.Add("Custom rules error on [" + landType + "]: " + e.Message.TrimEnd('.') + ". Rule updates for [" + landType + "] are ignored.");
                }
            }
        }

        private IEnumerable<string> UpdateGeneralRules(INI ini, Map map)
        {
            List<string> errors = new List<string>();
            int? goldVal = GetIntRulesValue(ini, "General", "GoldValue", false, errors);
            map.TiberiumOrGoldValue = goldVal ?? Constants.DefaultGoldValue;
            int? gemVal = GetIntRulesValue(ini, "General", "GemValue", false, errors);
            map.GemValue = gemVal ?? Constants.DefaultGemValue;
            int? radius = GetIntRulesValue(ini, "General", "DropZoneRadius", false, errors);
            map.DropZoneRadius = radius ?? Constants.DefaultDropZoneRadius;
            int? gapRadius = GetIntRulesValue(ini, "General", "GapRadius", false, errors);
            map.GapRadius = gapRadius ?? Constants.DefaultGapRadius;
            int? jamRadius = GetIntRulesValue(ini, "General", "RadarJamRadius", false, errors);
            map.RadarJamRadius = jamRadius ?? Constants.DefaultJamRadius;
            return errors;
        }

        private int? GetIntRulesValue(INI ini, string sec, string key, bool percentage, List<string> errors)
        {
            INISection section = ini.Sections[sec];
            if (section == null)
            {
                return null;
            }
            string valStr = section.TryGetValue(key);
            string valStrOrig = valStr;
            if (valStr != null)
            {
                valStr = valStr.Trim();
                if (percentage)
                {
                    valStr = valStr.TrimEnd('%', ' ', '\t');
                }
                try
                {
                    return Int32.Parse(valStr);
                }
                catch
                {
                    errors.Add(String.Format("Bad value \"{0}\" for \"{1}\" rule in section [{2}]. Needs an integer number{3}.",
                        valStrOrig, key, sec, percentage ? " percentage" : String.Empty));
                }
            }
            return null;
        }

        private static IEnumerable<string> UpdateBuildingRules(INI ini, Map map, bool forFootPrintTest)
        {
            List<string> errors = new List<string>();
            Dictionary<string, BuildingType> originals = BuildingTypes.GetTypes().ToDictionary(b => b.Name, StringComparer.OrdinalIgnoreCase);
            HashSet<Point> refreshPoints = new HashSet<Point>();
            List<(Point Location, Building Occupier)> buildings = map.Buildings.OfType<Building>()
                 .OrderBy(pb => pb.Location.Y * map.Metrics.Width + pb.Location.X).ToList();
            // Remove all buildings
            if (!forFootPrintTest)
            {
                foreach ((Point p, Building b) in buildings)
                {
                    refreshPoints.UnionWith(OccupierSet.GetOccupyPoints(p, b));
                    map.Buildings.Remove(b);
                }
            }
            // Potentially add new bibs that obstruct stuff
            foreach (BuildingType bType in map.BuildingTypes)
            {
                if (!originals.TryGetValue(bType.Name, out BuildingType orig))
                {
                    continue;
                }
                INISection bldSettings = ini[bType.Name];
                // Reset
                if (!forFootPrintTest)
                {
                    bType.PowerUsage = orig.PowerUsage;
                    bType.PowerProduction = orig.PowerProduction;
                    bType.Storage = orig.Storage;
                }
                if (bldSettings == null)
                {
                    if (!forFootPrintTest)
                    {
                        refreshPoints.UnionWith(ChangeBib(map, buildings, bType, orig.HasBib));
                    }
                    else if (bType.HasBib != orig.HasBib)
                    {
                        bType.HasBib = orig.HasBib;
                    }
                    continue;
                }
                RaBuildingIniSection bld = new RaBuildingIniSection();
                try
                {
                    List<(string,string)> parseErrors = INI.ParseSection(new MapContext(map, true), bldSettings, bld, true);
                    foreach ((string iniKey, string error) in parseErrors)
                    {
                        errors.Add("Custom rules error on [" + bType.Name + "]: " + error.TrimEnd('.') + ". Value for \"" + iniKey + "\" is ignored.");
                    }
                }
                catch (Exception e)
                {
                    // Normally won't happen with the aforementioned system.
                    errors.Add("Custom rules error on [" + bType.Name + "]: " + e.Message.TrimEnd('.') + ". Rule updates for [" + bType.Name + "] are ignored.");
                    continue;
                }
                if (!forFootPrintTest)
                {
                    if (bldSettings.Keys.Contains("Power"))
                    {
                        bType.PowerUsage = bld.Power >= 0 ? 0 : -bld.Power;
                        bType.PowerProduction = bld.Power <= 0 ? 0 : bld.Power;
                    }
                    if (bldSettings.Keys.Contains("Storage"))
                    {
                        bType.Storage = bld.Storage;
                    }
                }
                bool hasBib = bldSettings.Keys.Contains("Bib") ? bld.Bib : orig.HasBib;
                if (!forFootPrintTest)
                {
                    refreshPoints.UnionWith(ChangeBib(map, buildings, bType, hasBib));
                }
                else if (bType.HasBib != hasBib)
                {
                    bType.HasBib = hasBib;
                }
            }
            if (forFootPrintTest)
            {
                return errors;
            }
            // Try re-adding the buildings.
            foreach ((Point p, Building b) in buildings)
            {
                refreshPoints.UnionWith(OccupierSet.GetOccupyPoints(p, b));
                map.Buildings.Add(p, b);
            }
            map.NotifyRulesChanges(refreshPoints);
            return errors;
        }

        /// <summary>
        /// Bibs need a special refresh logic because they need to remove walls.
        /// </summary>
        /// <param name="map">Map</param>
        /// //<param name="buildings">List of buildings, since the original in map got wiped.</param>
        /// <param name="bType">Building type</param>
        /// <param name="hasBib">true if the new setting says the building will have a bib.</param>
        /// <returns>The points of and directly around any removed walls.</returns>
        private static IEnumerable<Point> ChangeBib(Map map, IEnumerable<(Point Location, Building Occupier)> buildings, BuildingType bType, bool hasBib)
        {
            HashSet<Point> changed = new HashSet<Point>();
            if (bType.HasBib == hasBib)
            {
                return changed;
            }
            List<(Point p, Building b)> foundBuildings = buildings.Where(lo => bType.ID == lo.Occupier.Type.ID)
                .OrderBy(lo => lo.Location.Y * map.Metrics.Width + lo.Location.X).ToList();
            bType.HasBib = hasBib;
            foreach ((Point p, Building b) in foundBuildings)
            {
                IEnumerable<Point> buildingPoints = OccupierSet.GetOccupyPoints(p, b);
                // Clear any walls that may now end up on the bib.
                if (Globals.BlockingBibs)
                {
                    foreach (Point bldPoint in buildingPoints)
                    {
                        Overlay ovl = map.Overlay[bldPoint];
                        if (ovl != null && (ovl.Type.IsWall || ovl.Type.IsSolid))
                        {
                            Rectangle toRefresh = new Rectangle(bldPoint, new Size(1, 1));
                            toRefresh.Inflate(1, 1);
                            map.Overlay[bldPoint] = null;
                            changed.UnionWith(toRefresh.Points());
                        }
                    }
                }
            }
            return changed;
        }

        public bool Save(string path, FileType fileType)
        {
            return Save(path, fileType, null, false);
        }

        public bool Save(string path, FileType fileType, Bitmap customPreview, bool dontResavePreview)
        {
            string errors = Validate(false);
            if (!String.IsNullOrWhiteSpace(errors))
            {
                MessageBox.Show(errors, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            Encoding dos437 = Encoding.GetEncoding(437);
            Encoding utf8 = new UTF8Encoding(false, false);
            byte[] linebreak = utf8.GetBytes("\r\n");
            INI ini = new INI();
            switch (fileType)
            {
                case FileType.INI:
                case FileType.BIN:
                    SaveINI(ini, fileType, path);
                    using (FileStream mprStream = new FileStream(path, FileMode.Create))
                    using (BinaryWriter mprWriter = new BinaryWriter(mprStream))
                    {
                        string iniText = ini.ToString("\n");
                        // Possibly scan extra ini content for all units/structs/etc with "Name" fields and save them as UTF-8 too? Not sure how the Remaster handles these.
                        GeneralUtils.WriteMultiEncoding(iniText.Split('\n'), mprWriter, dos437, utf8, new[] { ("Steam", null), ("Briefing", "Text"), ("Basic", "Name"), ("Basic", "Author") }, linebreak);
                    }
                    if (!Map.BasicSection.SoloMission && (!Globals.UseClassicFiles || !Globals.ClassicProducesNoMetaFiles))
                    {
                        string tgaPath = Path.ChangeExtension(path, ".tga");
                        string jsonPath = Path.ChangeExtension(path, ".json");
                        if (!File.Exists(tgaPath) || !dontResavePreview)
                        {
                            using (FileStream tgaStream = new FileStream(tgaPath, FileMode.Create))
                            {
                                if (customPreview != null)
                                {
                                    TGA.FromBitmap(customPreview).Save(tgaStream);
                                }
                                else
                                {
                                    SaveMapPreview(tgaStream, true);
                                }
                            }
                        }
                        using (FileStream jsonStream = new FileStream(jsonPath, FileMode.Create))
                        using (JsonTextWriter jsonWriter = new JsonTextWriter(new StreamWriter(jsonStream)))
                        {
                            SaveJSON(jsonWriter);
                        }
                    }
                    break;
                case FileType.MEG:
                case FileType.PGM:
                    SaveINI(ini, fileType, path);
                    using (MemoryStream mprStream = new MemoryStream())
                    using (MemoryStream tgaStream = new MemoryStream())
                    using (MemoryStream jsonStream = new MemoryStream())
                    using (BinaryWriter mprWriter = new BinaryWriter(mprStream))
                    using (JsonTextWriter jsonWriter = new JsonTextWriter(new StreamWriter(jsonStream)))
                    using (MegafileBuilder megafileBuilder = new MegafileBuilder(String.Empty, path))
                    {
                        string iniText = ini.ToString("\n");
                        GeneralUtils.WriteMultiEncoding(iniText.Split('\n'), mprWriter, dos437, utf8, new[] { ("Steam", null), ("Briefing", "Text"), ("Basic", "Name"), ("Basic", "Author") }, linebreak);
                        mprStream.Position = 0;
                        if (customPreview != null)
                        {
                            TGA.FromBitmap(customPreview).Save(tgaStream);
                        }
                        else
                        {
                            SaveMapPreview(tgaStream, true);
                        }
                        tgaStream.Position = 0;
                        SaveJSON(jsonWriter);
                        jsonWriter.Flush();
                        jsonStream.Position = 0;
                        string mprFile = Path.ChangeExtension(Path.GetFileName(path), ".mpr").ToUpper();
                        string tgaFile = Path.ChangeExtension(Path.GetFileName(path), ".tga").ToUpper();
                        string jsonFile = Path.ChangeExtension(Path.GetFileName(path), ".json").ToUpper();
                        megafileBuilder.AddFile(mprFile, mprStream);
                        megafileBuilder.AddFile(tgaFile, tgaStream);
                        megafileBuilder.AddFile(jsonFile, jsonStream);
                        megafileBuilder.Write();
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
            return true;
        }

        private void SaveINI(INI ini, FileType fileType, string fileName)
        {
            INISection oldAftermathSection = null;
            List<INISection> addedExtra = new List<INISection>();
            if (extraSections != null)
            {
                foreach (INISection section in extraSections)
                {
                    if ("Aftermath".Equals(section.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        oldAftermathSection = section.Clone();
                    }
                    else
                    {
                        addedExtra.Add(section.Clone());
                    }
                }
            }
            BasicSection basic = (BasicSection)Map.BasicSection;
            // Make new Aftermath section
            INISection newAftermathSection = new INISection("Aftermath");
            newAftermathSection["NewUnitsEnabled"] = basic.ExpansionEnabled ? "1" : "0";
            if (oldAftermathSection != null)
            {
                // If old section is present, remove NewUnitsEnabled value from it, and copy the remainder into the new one.
                oldAftermathSection.Remove("NewUnitsEnabled");
                foreach (KeyValuePair<string, string> kvp in oldAftermathSection)
                {
                    newAftermathSection[kvp.Key] = kvp.Value;
                }
            }
            // Add Aftermath section, either if it's enabled or if any custom info was added into the section besides the Enabled status.
            if (basic.ExpansionEnabled || newAftermathSection.Keys.Count > 1)
            {
                ini.Sections.Add(newAftermathSection);
            }
            // Add any other rules / unmanaged sections.
            ini.Sections.AddRange(addedExtra);
            // Clean up video names
            char[] cutfrom = { ';', '(' };
            basic.Intro = GeneralUtils.TrimRemarks(basic.Intro, true, cutfrom);
            basic.Brief = GeneralUtils.TrimRemarks(basic.Brief, true, cutfrom);
            basic.Action = GeneralUtils.TrimRemarks(basic.Action, true, cutfrom);
            basic.Win = GeneralUtils.TrimRemarks(basic.Win, true, cutfrom);
            basic.Win2 = GeneralUtils.TrimRemarks(basic.Win2, true, cutfrom);
            basic.Win3 = GeneralUtils.TrimRemarks(basic.Win3, true, cutfrom);
            basic.Win4 = GeneralUtils.TrimRemarks(basic.Win4, true, cutfrom);
            basic.Lose = GeneralUtils.TrimRemarks(basic.Lose, true, cutfrom);
            if (String.IsNullOrWhiteSpace(basic.Name))
            {
                string[] name = Path.GetFileNameWithoutExtension(fileName).Split(new[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < name.Length; i++)
                {
                    string word = name[i];
                    // Very very rough APA title casing :)
                    if (word.Length > 3)
                    {
                        name[i] = word[0].ToString().ToUpperInvariant() + word.Substring(1).ToLowerInvariant();
                    }
                }
                basic.Name = String.Join(" ", name);
            }
            INITools.FillAndReAdd(ini, "Basic", basic, new MapContext(Map, false), true);
            Map.MapSection.FixBounds();
            INITools.FillAndReAdd(ini, "Map", Map.MapSection, new MapContext(Map, false), true);
            if (fileType != FileType.PGM)
            {
                INI.WriteSection(new MapContext(Map, false), ini.Sections.Add("Steam"), Map.SteamSection);
            }
            INISection smudgeSection = ini.Sections.Add("SMUDGE");
            // Flatten multi-cell bibs
            Dictionary<int, Smudge> resolvedSmudge = new Dictionary<int, Smudge>();
            foreach (var (cell, smudge) in Map.Smudge.Where(item => !item.Value.Type.IsAutoBib).OrderBy(s => s.Cell))
            {
                int actualCell = smudge.GetPlacementOrigin(cell, this.Map.Metrics);
                if (!resolvedSmudge.ContainsKey(actualCell))
                {
                    resolvedSmudge[actualCell] = smudge;
                }
            }
            foreach (int cell in resolvedSmudge.Keys.OrderBy(c => c))
            {
                Smudge smudge = resolvedSmudge[cell];
                smudgeSection[cell.ToString()] = string.Format("{0},{1},{2}", smudge.Type.Name.ToUpperInvariant(), cell, Math.Min(smudge.Type.Icons - 1, smudge.Icon));
            }
            INISection terrainSection = ini.Sections.Add("TERRAIN");
            foreach (var (location, terrain) in Map.Technos.OfType<Terrain>().OrderBy(t => Map.Metrics.GetCell(t.Location)))
            {
                if (Map.Metrics.GetCell(location, out int cell))
                {
                    terrainSection[cell.ToString()] = terrain.Type.Name.ToUpperInvariant();
                }
            }
            INISection cellTriggersSection = ini.Sections.Add("CellTriggers");
            foreach (var (cell, cellTrigger) in Map.CellTriggers.OrderBy(t => t.Cell))
            {
                cellTriggersSection[cell.ToString()] = cellTrigger.Trigger;
            }
            int nameToIndex<T>(IList<T> list, string name)
            {
                int index = list.TakeWhile(x => !x.Equals(name)).Count();
                return (index < list.Count) ? index : -1;
            }
            string nameToIndexString<T>(IList<T> list, string name) => nameToIndex(list, name).ToString();
            INISection teamTypesSection = ini.Sections.Add("TeamTypes");
            foreach (TeamType teamType in Map.TeamTypes)
            {
                string[] classes = teamType.Classes
                    .Select(c => string.Format("{0}:{1}", c.Type.Name.ToUpperInvariant(), c.Count))
                    .ToArray();
                string[] missions = teamType.Missions
                    .Select(m => string.Format("{0}:{1}", m.Mission.ID, m.Argument))
                    .ToArray();
                int flags = 0;
                if (teamType.IsRoundAbout) flags |= 0x01;
                if (teamType.IsSuicide) flags |= 0x02;
                if (teamType.IsAutocreate) flags |= 0x04;
                if (teamType.IsPrebuilt) flags |= 0x08;
                if (teamType.IsReinforcable) flags |= 0x10;
                List<string> tokens = new List<string>
                {
                    teamType.House?.ID.ToString() ?? "-1",
                    flags.ToString(),
                    teamType.RecruitPriority.ToString(),
                    teamType.InitNum.ToString(),
                    teamType.MaxAllowed.ToString(),
                    teamType.Origin.ToString(),
                    nameToIndexString(Map.Triggers, teamType.Trigger),
                    classes.Length.ToString(),
                    string.Join(",", classes),
                    missions.Length.ToString(),
                    string.Join(",", missions)
                };
                teamTypesSection[teamType.Name] = string.Join(",", tokens.Where(t => !string.IsNullOrEmpty(t)));
            }
            INISection infantrySection = ini.Sections.Add("INFANTRY");
            int infantryIndex = 0;
            foreach (var (location, infantryGroup) in Map.Technos.OfType<InfantryGroup>().OrderBy(i => Map.Metrics.GetCell(i.Location)))
            {
                for (int i = 0; i < infantryGroup.Infantry.Length; ++i)
                {
                    Infantry infantry = infantryGroup.Infantry[i];
                    if (infantry == null || !Map.Metrics.GetCell(location, out int cell))
                    {
                        continue;
                    }
                    string key = infantryIndex.ToString("D3");
                    infantryIndex++;
                    infantrySection[key] = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                        infantry.House.Name,
                        infantry.Type.Name.ToUpperInvariant(),
                        infantry.Strength,
                        cell,
                        i,
                        String.IsNullOrEmpty(infantry.Mission) ? "Guard" : infantry.Mission,
                        infantry.Direction.ID,
                        infantry.Trigger
                    );
                }
            }
            INISection structuresSection = ini.Sections.Add("STRUCTURES");
            int structureIndex = 0;
            foreach (var (location, building) in Map.Buildings.OfType<Building>().Where(x => x.Occupier.IsPrebuilt).OrderBy(b => Map.Metrics.GetCell(b.Location)))
            {
                if (!Map.Metrics.GetCell(location, out int cell))
                {
                    continue;
                }
                string key = structureIndex.ToString("D3");
                structureIndex++;
                structuresSection[key] = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                    building.House.Name,
                    building.Type.Name.ToUpperInvariant(),
                    building.Strength,
                    cell,
                    building.Direction.ID,
                    building.Trigger,
                    building.Sellable ? 1 : 0,
                    building.Rebuild ? 1 : 0
                );
            }
            INISection baseSectionOld = ini.Sections.Extract("Base");
            if (baseSectionOld != null)
            {
                CleanBaseSection(ini, baseSectionOld);
            }
            INISection baseSection = ini.Sections.Add("Base");
            var baseBuildings = Map.Buildings.OfType<Building>().Where(x => x.Occupier.BasePriority >= 0).OrderBy(x => x.Occupier.BasePriority).ToArray();
            baseSection["Player"] = Map.BasicSection.BasePlayer;
            baseSection["Count"] = baseBuildings.Length.ToString();
            int baseIndex = 0;
            foreach (var (location, building) in baseBuildings)
            {
                if (!Map.Metrics.GetCell(location, out int cell))
                {
                    continue;
                }
                string key = baseIndex.ToString("D3");
                baseIndex++;
                baseSection[key] = string.Format("{0},{1}",
                    building.Type.Name.ToUpperInvariant(),
                    cell
                );
            }
            if (baseSectionOld != null)
            {
                foreach (KeyValuePair<string, string> kvp in baseSectionOld)
                {
                    baseSection[kvp.Key] = kvp.Value;
                }
            }
            INISection unitsSection = ini.Sections.Add("UNITS");
            int unitIndex = 0;
            foreach (var (location, unit) in Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsGroundUnit).OrderBy(u => Map.Metrics.GetCell(u.Location)))
            {
                if (!Map.Metrics.GetCell(location, out int cell))
                {
                    continue;
                }
                string key = unitIndex.ToString("D3");
                unitIndex++;
                unitsSection[key] = string.Format("{0},{1},{2},{3},{4},{5},{6}",
                    unit.House.Name,
                    unit.Type.Name.ToUpperInvariant(),
                    unit.Strength,
                    cell,
                    unit.Direction.ID,
                    String.IsNullOrEmpty(unit.Mission) ? "Guard" : unit.Mission,
                    unit.Trigger
                );
            }
            // Classic game does not support this, so it's disabled by default.
            if (!Globals.DisableAirUnits)
            {
                INISection aircraftSection = ini.Sections.Add("AIRCRAFT");
                int aircraftIndex = 0;
                foreach (var (location, aircraft) in Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsAircraft).OrderBy(u => Map.Metrics.GetCell(u.Location)))
                {
                    if (!Map.Metrics.GetCell(location, out int cell))
                    {
                        continue;
                    }
                    string key = aircraftIndex.ToString("D3");
                    aircraftIndex++;
                    aircraftSection[key] = string.Format("{0},{1},{2},{3},{4},{5}",
                        aircraft.House.Name,
                        aircraft.Type.Name.ToUpperInvariant(),
                        aircraft.Strength,
                        cell,
                        aircraft.Direction.ID,
                        String.IsNullOrEmpty(aircraft.Mission) ? "Guard" : aircraft.Mission
                    );
                }
            }
            INISection shipsSection = ini.Sections.Add("SHIPS");
            int shipsIndex = 0;
            foreach (var (location, ship) in Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsVessel).OrderBy(u => Map.Metrics.GetCell(u.Location)))
            {
                if (!Map.Metrics.GetCell(location, out int cell))
                {
                    continue;
                }
                string key = shipsIndex.ToString("D3");
                shipsIndex++;
                shipsSection[key] = string.Format("{0},{1},{2},{3},{4},{5},{6}",
                    ship.House.Name,
                    ship.Type.Name.ToUpperInvariant(),
                    ship.Strength,
                    cell,
                    ship.Direction.ID,
                    String.IsNullOrEmpty(ship.Mission) ? "Guard" : ship.Mission,
                    ship.Trigger
                );
            }
            INISection triggersSection = ini.Sections.Add("Trigs");
            foreach (var trigger in Map.Triggers)
            {
                if (string.IsNullOrEmpty(trigger.Name))
                {
                    continue;
                }

                int action2TypeIndex = nameToIndex(Map.ActionTypes, trigger.Action2.ActionType);
                TriggerMultiStyleType actionControl = (action2TypeIndex > 0) ? TriggerMultiStyleType.And : TriggerMultiStyleType.Only;

                List<string> tokens = new List<string>
                {
                    ((int)trigger.PersistentType).ToString(),
                    !string.IsNullOrEmpty(trigger.House) ? (Map.HouseTypes.Where(h => h.Equals(trigger.House)).FirstOrDefault()?.ID.ToString() ?? "-1") : "-1",
                    ((int)trigger.EventControl).ToString(),
                    ((int)actionControl).ToString(),
                    nameToIndexString(Map.EventTypes, trigger.Event1.EventType),
                    nameToIndexString(Map.TeamTypes, trigger.Event1.Team),
                    trigger.Event1.Data.ToString(),
                    trigger.EventControl == TriggerMultiStyleType.Only ? "0" : nameToIndexString(Map.EventTypes, trigger.Event2.EventType),
                    trigger.EventControl == TriggerMultiStyleType.Only ? "0" : nameToIndexString(Map.TeamTypes, trigger.Event2.Team),
                    trigger.EventControl == TriggerMultiStyleType.Only ? "0" : trigger.Event2.Data.ToString(),
                    nameToIndexString(Map.ActionTypes, trigger.Action1.ActionType),
                    nameToIndexString(Map.TeamTypes, trigger.Action1.Team),
                    nameToIndexString(Map.Triggers, trigger.Action1.Trigger),
                    trigger.Action1.Data.ToString(),
                    action2TypeIndex.ToString(),
                    nameToIndexString(Map.TeamTypes, trigger.Action2.Team),
                    nameToIndexString(Map.Triggers, trigger.Action2.Trigger),
                    trigger.Action2.Data.ToString()
                };

                triggersSection[trigger.Name] = string.Join(",", tokens);
            }
            INISection waypointsSection = ini.Sections.Add("Waypoints");
            for (int i = 0; i < Map.Waypoints.Length; ++i)
            {
                Waypoint waypoint = Map.Waypoints[i];
                if (waypoint.Cell.HasValue)
                {
                    waypointsSection[i.ToString()] = waypoint.Cell.Value.ToString();
                }
            }
            foreach (Model.House house in Map.Houses.Where(h => !h.Type.Flags.HasFlag(HouseTypeFlag.Special)).OrderBy(h => h.Type.ID))
            {
                House gameHouse = (House)house;
                bool enabled = house.Enabled;
                string name = gameHouse.Type.Name;
                INISection houseSection = INITools.FillAndReAdd(ini, gameHouse.Type.Name, gameHouse, new MapContext(Map, false), enabled);
                // Current house is not in its own alliances list. Fix that.
                if (houseSection != null && !gameHouse.Allies.Contains(gameHouse.Type.ID))
                {
                    HashSet<string> allies = (houseSection.TryGetValue("Allies") ?? string.Empty)
                        .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
                    if (!allies.Contains(name))
                    {
                        allies.Add(name);
                        List<string> alliesBuild = new List<string>();
                        foreach (Model.House houseAll in Map.HousesForAlliances.Where(h => allies.Contains(h.Type.Name)))
                        {
                            alliesBuild.Add(houseAll.Type.Name);
                        }
                        houseSection["Allies"] = String.Join(",", alliesBuild.ToArray());
                    }
                }
            }
            SaveIniBriefing(ini);
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
                {
                    for (int y = 0; y < Map.Metrics.Height; ++y)
                    {
                        for (int x = 0; x < Map.Metrics.Width; ++x)
                        {
                            Template template = Map.Templates[y, x];
                            if (template != null && (template.Type.Flag & TemplateTypeFlag.Clear) == 0)
                            {
                                writer.Write((ushort)template.Type.ID);
                            }
                            else
                            {
                                writer.Write(ushort.MaxValue);
                            }
                        }
                    }
                    for (int y = 0; y < Map.Metrics.Height; ++y)
                    {
                        for (int x = 0; x < Map.Metrics.Width; ++x)
                        {
                            Template template = Map.Templates[y, x];
                            if (template != null && (template.Type.Flag & TemplateTypeFlag.Clear) == 0)
                            {
                                writer.Write((byte)template.Icon);
                            }
                            else
                            {
                                writer.Write((byte)0);
                            }
                        }
                    }
                }
                stream.Flush();
                ini.Sections.Remove("MapPack");
                CompressLCWSection(ini.Sections.Add("MapPack"), stream.ToArray());
            }
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
                {
                    for (int i = 0; i < Map.Metrics.Length; ++i)
                    {
                        Overlay overlay = Map.Overlay[i];
                        if (overlay != null)
                        {
                            writer.Write((byte)overlay.Type.ID);
                        }
                        else
                        {
                            writer.Write(byte.MaxValue);
                        }
                    }
                }
                stream.Flush();
                ini.Sections.Remove("OverlayPack");
                CompressLCWSection(ini.Sections.Add("OverlayPack"), stream.ToArray());
            }
        }

        protected INISection SaveIniBriefing(INI ini)
        {
            INISection oldSection = ini.Sections.Extract("Briefing");
            if (oldSection != null)
            {
                oldSection.Remove("Text");
                oldSection.RemoveWhere(k => Regex.IsMatch(k, "^\\d+$"));
            }
            if (string.IsNullOrEmpty(Map.BriefingSection.Briefing))
            {
                if (oldSection != null)
                {
                    ini.Sections.Add(oldSection);
                    return oldSection;
                }
                return null;
            }
            INISection briefingSection = ini.Sections.Add("Briefing");
            string briefText = Map.BriefingSection.Briefing.Replace('\t', ' ').Trim('\r', '\n', ' ').Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "@");
            // Remove duplicate spaces
            briefText = Regex.Replace(briefText, " +", " ");
            if (string.IsNullOrEmpty(briefText))
            {
                return null;
            }
            briefingSection["Text"] = briefText;
            if (Globals.WriteClassicBriefing)
            {
                if (briefText.Length > Constants.MaxBriefLengthClassic)
                {
                    briefText = briefText.Substring(0, Constants.MaxBriefLengthClassic);
                }
                List<string> finalLines = new List<string>();
                string line = briefText;
                if (line.Length <= Constants.BriefLineCutoffClassic)
                {
                    finalLines.Add(line);
                }
                else
                {
                    string[] splitLine = Regex.Split(line, "([ @])");
                    int wordIndex = 0;
                    while (wordIndex < splitLine.Length)
                    {
                        StringBuilder sb = new StringBuilder();
                        // Always allow initial word
                        int nextLength = 0;
                        bool isBreak = false;
                        while (nextLength < Constants.BriefLineCutoffClassic && wordIndex < splitLine.Length)
                        {
                            string cur = splitLine[wordIndex];
                            bool wasBreak = isBreak;
                            isBreak = cur == "@";
                            if (cur == " " || cur.Length == 0)
                            {
                                wordIndex++;
                                continue;
                            }
                            if (sb.Length > 0 && !isBreak && !wasBreak)
                                sb.Append(' ');
                            sb.Append(cur);
                            wordIndex++;
                            // Skip spaces and empty entries.
                            while (wordIndex < splitLine.Length && (splitLine[wordIndex].Length == 0 || splitLine[wordIndex] == " "))
                                wordIndex++;
                            if (wordIndex < splitLine.Length)
                            {
                                // Next
                                cur = splitLine[wordIndex];
                                nextLength = sb.Length + (cur == "@" || isBreak ? 0 : 1) + cur.Length;
                            }
                        }
                        // Classic briefings cannot contain semicolons.
                        finalLines.Add(sb.Replace(';', ':').ToString());
                    }
                }
                for (int i = 0; i < finalLines.Count; ++i)
                {
                    briefingSection[(i + 1).ToString()] = finalLines[i];
                }
            }
            if (oldSection != null)
            {
                foreach (KeyValuePair<string, string> kvp in oldSection)
                {
                    if (!briefingSection.Contains(kvp.Key))
                    {
                        briefingSection[kvp.Key] = kvp.Value;
                    }
                }
            }
            return briefingSection;
        }

        private void SaveMapPreview(Stream stream, bool renderAll)
        {
            Map.GenerateMapPreview(this, renderAll).Save(stream);
        }

        private void SaveJSON(JsonTextWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("MapTileX");
            writer.WriteValue(Map.MapSection.X);
            writer.WritePropertyName("MapTileY");
            writer.WriteValue(Map.MapSection.Y);
            writer.WritePropertyName("MapTileWidth");
            writer.WriteValue(Map.MapSection.Width);
            writer.WritePropertyName("MapTileHeight");
            writer.WriteValue(Map.MapSection.Height);
            writer.WritePropertyName("Theater");
            writer.WriteValue(Map.MapSection.Theater.Name.ToUpper());
            writer.WritePropertyName("Waypoints");
            writer.WriteStartArray();
            if (!Map.BasicSection.SoloMission)
            {
                foreach (Waypoint waypoint in Map.Waypoints.Where(w => w.Flag.HasFlag(WaypointFlag.PlayerStart)
                    && w.Cell.HasValue && Map.Metrics.GetLocation(w.Cell.Value, out Point p) && Map.Bounds.Contains(p)))
                {
                    writer.WriteValue(waypoint.Cell.Value);
                }
            }
            else
            {
                // Probably useless, but better than the player start points.
                foreach (Waypoint waypoint in Map.Waypoints.Where(w => w.Flag.HasFlag(WaypointFlag.Home)
                    && w.Cell.HasValue && Map.Metrics.GetLocation(w.Cell.Value, out Point p) && Map.Bounds.Contains(p)))
                {
                    writer.WriteValue(waypoint.Cell.Value);
                }
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public string Validate(bool forWarnings)
        {
            if (forWarnings)
            {
                return ValidateForWarnings();
            }
            StringBuilder sb = new StringBuilder();
            int numAircraft = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsAircraft).Count();
            int numBuildings = Map.Buildings.OfType<Building>().Where(x => x.Occupier.IsPrebuilt).Count();
            int numInfantry = Map.Technos.OfType<InfantryGroup>().Sum(item => item.Occupier.Infantry.Count(i => i != null));
            int numTerrain = Map.Technos.OfType<Terrain>().Count();
            int numUnits = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsGroundUnit).Count();
            int numVessels = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsVessel).Count();
            int numStartPoints = Map.Waypoints.Count(w => w.Flag.HasFlag(WaypointFlag.PlayerStart) && w.Cell.HasValue
                && Map.Metrics.GetLocation(w.Cell.Value, out Point pt) && Map.Bounds.Contains(pt));
            int numBadPoints = Map.Waypoints.Count(w => w.Flag.HasFlag(WaypointFlag.PlayerStart) && w.Cell.HasValue
                && Map.Metrics.GetLocation(w.Cell.Value, out Point pt) && !Map.Bounds.Contains(pt));
            if (!Globals.DisableAirUnits && numAircraft > Constants.MaxAircraft && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of aircraft exceeded ({0} > {1})", numAircraft, Constants.MaxAircraft));
            }
            if (numBuildings > Constants.MaxBuildings && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of structures exceeded ({0} > {1})", numBuildings, Constants.MaxBuildings));
            }
            if (numInfantry > Constants.MaxInfantry && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of infantry exceeded ({0} > {1})", numInfantry, Constants.MaxInfantry));
            }
            if (numTerrain > Constants.MaxTerrain && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of terrain objects exceeded ({0} > {1})", numTerrain, Constants.MaxTerrain));
            }
            if (numUnits > Constants.MaxUnits && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of units exceeded ({0} > {1})", numUnits, Constants.MaxUnits));
            }
            if (numVessels > Constants.MaxVessels && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of ships exceeded ({0} > {1})", numVessels, Constants.MaxVessels));
            }
            if (Map.TeamTypes.Count > Constants.MaxTeams && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of team types exceeded ({0} > {1})", Map.TeamTypes.Count, Constants.MaxTeams));
            }
            if (Map.Triggers.Count > Constants.MaxTriggers && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of triggers exceeded ({0} > {1})", Map.Triggers.Count, Constants.MaxTriggers));
            }
            if (!Map.BasicSection.SoloMission)
            {
                if (numStartPoints < 2)
                {
                    sb.Append("\nSkirmish/Multiplayer maps need at least 2 waypoints for player starting locations.");
                }
                if (numBadPoints > 0)
                {
                    sb.Append("\nSkirmish/Multiplayer maps should not have player start waypoints placed outside the map bound.");
                }
            }
            Waypoint homeWaypoint = Map.Waypoints.Where(w => w.Flag.HasFlag(WaypointFlag.Home)).FirstOrDefault();
            if (Map.BasicSection.SoloMission && (!homeWaypoint.Cell.HasValue || !Map.Metrics.GetLocation(homeWaypoint.Cell.Value, out Point p) || !Map.Bounds.Contains(p)))
            {
                sb.Append("\nSingle-player maps need the Home waypoint to be placed, inside the map bounds.");
            }
            bool fatal;
            IEnumerable<string> triggerErr = CheckTriggers(this.Map.Triggers, true, true, true, out fatal, false, out bool _);
            if (fatal)
            {
                foreach (string err in triggerErr)
                {
                    sb.Append("\n").Append(err);
                }
            }
            if (sb.Length > 0)
            {
                sb.Insert(0, "Error(s) during map validation:\n").TrimEnd('\n');
                return sb.ToString();
            }
            return null;
        }

        private string ValidateForWarnings()
        {
            StringBuilder sb = new StringBuilder();
            // Check if map has name
            if (this.GameInfo.MapNameIsEmpty(this.Map.BasicSection.Name))
            {
                sb.Append("Map name is empty. If you continue, the filename will be filled in as map name.\n");
            }
            UnitType[] antUnitTypes = { UnitTypes.Ant1, UnitTypes.Ant2, UnitTypes.Ant3 };
            BuildingType[] antBuildingTypes = { BuildingTypes.Queen, BuildingTypes.Larva1, BuildingTypes.Larva2 };
            string[] antWeapons = { "Mandible" };
            CheckMissingRules(sb, "ant-related", antUnitTypes, null, antBuildingTypes, antWeapons);
            /*/
            // Seems all AM units have default rules in aftrmath.ini, so no real point in checking this.
            if (Map.BasicSection.ExpansionEnabled)
            {
                UnitType[] expUnitTypes = Map.UnitTypes.Where(u => u.IsExpansionOnly).ToArray();
                InfantryType[] expInfTypes = Map.InfantryTypes.Where(u => u.IsExpansionOnly).ToArray();
                BuildingType[] expBuildingTypes = { };
                string[] expWeapons = { "AirAssault", "PortaTesla", "TTankZap", "GoodWrench", "SubSCUD", "APTusk", "Democharge" };
                // Cah't check warheads: it requires having a list of all weapons (or checking literally every section for "Warhead=" I guess).
                //string[] expWarheads = { "Mechanical" };
                CheckMissingRules(sb, "Aftermath", expUnitTypes, expInfTypes, expBuildingTypes, expWeapons);
            }
            //*/
            return sb.ToString();
        }

        /// <summary>
        /// Checks if the map or any of the scripting has references to the given types, and if so, if their rules are filled in.
        /// </summary>
        /// <param name="sb">StringBuilder to throw the analysis into in case types are missing.</param>
        /// <param name="context">The types of units/structures/etc being checked. Can be left empty.</param>
        /// <param name="checkUnitTypes">Unit types to check.</param>
        /// <param name="checkInfantryTypes">Infantry types to check.</param>
        /// <param name="checkBuildingTypes">Building types to check.</param>
        /// <param name="checkWeaponTypes">Weapon types to check.</param>
        private void CheckMissingRules(StringBuilder sb, string context, UnitType[] checkUnitTypes, InfantryType[] checkInfantryTypes, BuildingType[] checkBuildingTypes, string[] checkWeaponTypes)
        {
            context = (context ?? String.Empty).Trim();
            if (context.Length == 0)
            {
                context = null;
            }
            // Check used objects on map: units
            List<UnitType> usedUnits = new List<UnitType>();
            if (checkUnitTypes != null && checkUnitTypes.Length > 0)
            {
                // Resolve potential clones to their local equivalents.
                UnitType[] testUnitTypes = this.Map.UnitTypes.Where(ut => checkUnitTypes.Any(ct => ct.ID == ut.ID)).ToArray();
                Dictionary<int, UnitType> testUnitsById = testUnitTypes.ToDictionary(ct => ct.ID);
                // Find on map
                List<UnitType> mapUsedUnits = Map.Technos.OfType<Unit>().Where(u => testUnitTypes.Contains(u.Occupier.Type)).Select(lb => lb.Occupier.Type).ToList();
                // Find in teams
                List<UnitType> teamUsedUnits = Map.TeamTypes.SelectMany(t => t.Classes).Where(cl => testUnitTypes.Contains(cl.Type)).Select(cl => cl.Type).OfType<UnitType>().ToList();
                mapUsedUnits.AddRange(teamUsedUnits);
                // Find in triggers
                List<UnitType> trigUsedUnits1 = Map.Triggers
                    .Where(tr => tr.Event1.EventType == EventTypes.TEVENT_BUILD_UNIT && testUnitsById.ContainsKey((int)tr.Event1.Data))
                    .Select(tr => testUnitsById[(int)tr.Event1.Data]).ToList();
                mapUsedUnits.AddRange(trigUsedUnits1);
                List<UnitType> trigUsedUnits2 = Map.Triggers
                    .Where(tr => tr.UsesEvent2 && tr.Event2.EventType == EventTypes.TEVENT_BUILD_UNIT && testUnitsById.ContainsKey((int)tr.Event2.Data))
                    .Select(tr => testUnitsById[(int)tr.Event2.Data]).ToList();
                mapUsedUnits.AddRange(trigUsedUnits2);
                // Match back to original list
                usedUnits.AddRange(testUnitTypes.Where(u => mapUsedUnits.Contains(u)));
            }
            // Check used objects on map: infantry
            List<InfantryType> usedInfantry = new List<InfantryType>();
            if (checkInfantryTypes != null && checkInfantryTypes.Length > 0)
            {
                // Resolve potential clones to their local equivalents.
                InfantryType[] testInfantryTypes = this.Map.InfantryTypes.Where(it => checkInfantryTypes.Any(ct => ct.ID == it.ID)).ToArray();
                Dictionary<int, InfantryType> testInfantryById = testInfantryTypes.ToDictionary(ct => ct.ID);
                // Find on map
                List<InfantryType> mapUsedInfantry = Map.Technos.OfType<InfantryGroup>().SelectMany(ig => ig.Occupier.Infantry).Where(i => i != null && testInfantryTypes.Contains(i.Type)).Select(i => i.Type).ToList();
                // Find in teams
                List<InfantryType> teamUsedInfantry = Map.TeamTypes.SelectMany(t => t.Classes).Where(cl => testInfantryTypes.Contains(cl.Type)).Select(cl => cl.Type).OfType<InfantryType>().ToList();
                mapUsedInfantry.AddRange(teamUsedInfantry);
                // Find in triggers
                List<InfantryType> trigUsedInfantry1 = Map.Triggers
                    .Where(tr => tr.Event1.EventType == EventTypes.TEVENT_BUILD_INFANTRY && testInfantryById.ContainsKey((int)tr.Event1.Data))
                    .Select(tr => testInfantryById[(int)tr.Event1.Data]).ToList();
                mapUsedInfantry.AddRange(trigUsedInfantry1);
                List<InfantryType> trigUsedInfantry2 = Map.Triggers
                    .Where(tr => tr.UsesEvent2 && tr.Event2.EventType == EventTypes.TEVENT_BUILD_INFANTRY && testInfantryById.ContainsKey((int)tr.Event2.Data))
                    .Select(tr => testInfantryById[(int)tr.Event2.Data]).ToList();
                mapUsedInfantry.AddRange(trigUsedInfantry2);
                // Match back to original list
                usedInfantry.AddRange(testInfantryTypes.Where(u => mapUsedInfantry.Contains(u)));
            }

            // Check used objects on map: buildings
            List<BuildingType> usedBuildings = new List<BuildingType>();
            if (checkBuildingTypes != null && checkBuildingTypes.Length > 0)
            {
                // Resolve potential clones to their local equivalents.
                BuildingType[] testBuildingTypes = this.Map.BuildingTypes.Where(bt => checkBuildingTypes.Any(ct => ct.ID == bt.ID)).ToArray();
                Dictionary<int, BuildingType> testBuildingsById = testBuildingTypes.ToDictionary(ct => ct.ID);
                // Find on map
                List<BuildingType> mapUsedBuildings = Map.Buildings.OfType<Building>().Where(x => testBuildingTypes.Contains(x.Occupier.Type)).Select(lb => lb.Occupier.Type).ToList();
                // Find in triggers
                List<BuildingType> trigUsedBuildings1 = Map.Triggers
                    .Where(tr => (tr.Event1.EventType == EventTypes.TEVENT_BUILDING_EXISTS || tr.Event1.EventType == EventTypes.TEVENT_BUILD)
                            && testBuildingsById.ContainsKey((int)tr.Event1.Data)).Select(tr => testBuildingsById[(int)tr.Event1.Data]).ToList();
                mapUsedBuildings.AddRange(trigUsedBuildings1);
                List<BuildingType> trigUsedBuildings2 = Map.Triggers
                    .Where(tr => tr.UsesEvent2 && (tr.Event2.EventType == EventTypes.TEVENT_BUILDING_EXISTS || tr.Event2.EventType == EventTypes.TEVENT_BUILD)
                            && testBuildingsById.ContainsKey((int)tr.Event2.Data)).Select(tr => testBuildingsById[(int)tr.Event2.Data]).ToList();
                mapUsedBuildings.AddRange(trigUsedBuildings2);
                // Match back to original list
                usedBuildings.AddRange(testBuildingTypes.Where(b => mapUsedBuildings.Contains(b)));
            }
            // Determine in which rules to check.
            List<INISectionCollection> checkSections = new List<INISectionCollection>();
            if (rulesIni != null)
                checkSections.Add(rulesIni.Sections);
            if (Map.BasicSection.ExpansionEnabled && aftermathRulesIni != null)
                checkSections.Add(aftermathRulesIni.Sections);
            if (Map.BasicSection.ExpansionEnabled && !Map.BasicSection.SoloMission && aftermathMpRulesIni != null)
                checkSections.Add(aftermathMpRulesIni.Sections);
            if (extraSections != null)
                checkSections.Add(extraSections);
            // weapon checks.
            List<string> missingWeapons = CheckMissingWeaponRules(checkWeaponTypes, checkSections);
            // None of the checked items is used on the map, and no used weapons were identified as missing rules.
            if (usedBuildings.Count == 0 && usedUnits.Count == 0 && usedInfantry.Count == 0 && missingWeapons.Count == 0)
            {
                return;
            }
            // Build final list of missing objects and object types.
            List<string> missingTypes = new List<string>();
            List<string> missingObjTypes = new List<string>();
            const string unitStr = "unit";
            bool unitsMissing = false;
            foreach (UnitType unit in usedUnits)
            {
                if (!INISectionCollection.AnyIniSectionContains(unit.Name, checkSections))
                {
                    missingObjTypes.Add(unit.Name.ToUpperInvariant() + " (" + unitStr + ")");
                    unitsMissing = true;
                }
            }
            if (unitsMissing) missingTypes.Add(unitStr);
            const string infStr = "infantry";
            bool infantryMissing = false;
            foreach (InfantryType inf in usedInfantry)
            {
                if (!INISectionCollection.AnyIniSectionContains(inf.Name, checkSections))
                {
                    missingObjTypes.Add(inf.Name.ToUpperInvariant() + " (" + infStr + ")");
                    infantryMissing = true;
                }
            }
            if (infantryMissing) missingTypes.Add(infStr);
            const string bldStr = "building";
            bool buildingsMissing = false;
            foreach (BuildingType bld in usedBuildings)
            {
                if (!INISectionCollection.AnyIniSectionContains(bld.Name, checkSections))
                {
                    missingObjTypes.Add(bld.Name.ToUpperInvariant() + " (" + bldStr + ")");
                    buildingsMissing = true;
                }
            }
            if (buildingsMissing) missingTypes.Add(bldStr);
            const string weaponStr = "weapon";
            foreach (string weapon in missingWeapons)
            {
                missingObjTypes.Add(weapon + " (" + weaponStr + ")");
            }
            if (missingWeapons.Count > 0) missingTypes.Add(weaponStr);
            if (missingObjTypes.Count == 0)
            {
                return;
            }
            bool plural = missingObjTypes.Count > 1;
            // sb.Append(null) will abort immediately, so it's more efficient than using 'String.Empty'.
            sb.Append("The following ");
            sb.Append(context != null ? (context + " ") : null);
            sb.Append(string.Join("/", missingTypes.ToArray()));
            sb.Append(" type").Append(plural ? "s are" : " is").Append(" used on the map");
            sb.Append(unitsMissing || infantryMissing || buildingsMissing ? " or in the scripting" : null);
            sb.Append(", but ").Append(plural ? "have" : "has").Append(" no ini rules set to properly define ").Append(plural ? "their" : "its").Append(" stats:\n- ");
            sb.Append(string.Join("\n- ", missingObjTypes.ToArray()));
            sb.Append("\nWithout ini definition").Append(plural ? "s, these objects" : ", this object").Append(" will have no ");
            sb.Append(unitsMissing || infantryMissing ? "strength, weapon or movement speed" : buildingsMissing ? "strength or weapon" : "weapon");
            sb.Append(" stats, and will malfunction in the game. The definitions can be set in Settings → Map Settings → INI Rules & Tweaks.");
        }

        private List<string> CheckMissingWeaponRules(string[] checkWeaponTypes, List<INISectionCollection> checkSections)
        {
            // weapon checks.
            List<string> missingWeapons = new List<string>();
            const string primaryIniName = "Primary";
            const string secondaryIniName = "Secondary";
            // Make list of all technos
            List<ITechnoType> allTypes = new List<ITechnoType>();
            allTypes.AddRange(Map.UnitTypes);
            allTypes.AddRange(Map.InfantryTypes);
            allTypes.AddRange(Map.BuildingTypes);
            // Loop over all weapons to check.
            foreach (string weaponType in checkWeaponTypes)
            {
                if (INISectionCollection.AnyIniSectionContains(weaponType, checkSections))
                {
                    // Rules section exist, so there is no problem.
                    continue;
                }
                // Rules section does not exist; check if any units/structures/infantry use the weapon.
                bool weaponUsed = false;
                foreach (ITechnoType techno in allTypes)
                {
                    string sectionName = techno.Name;
                    foreach (INISectionCollection inicoll in checkSections)
                    {
                        if (inicoll.Contains(sectionName))
                        {
                            INISection section = inicoll[sectionName];
                            string primaryWeapon = section.TryGetValue(primaryIniName);
                            string secondaryWeapon = section.TryGetValue(secondaryIniName);
                            // Check if the weapon under investigation is used in this section.
                            // Since we confirmed it has no rules, it's a problem if it's used.
                            if (weaponType == primaryWeapon || weaponType == secondaryWeapon)
                            {
                                missingWeapons.Add(weaponType);
                                weaponUsed = true;
                                break;
                            }
                        }
                    }
                    if (weaponUsed)
                    {
                        break;
                    }
                }
            }
            return missingWeapons.Distinct().ToList();
        }

        public IEnumerable<string> AssessMapItems()
        {
            ExplorerComparer cmp = new ExplorerComparer();
            List<string> info = new List<string>();
            int numAircraft = Globals.DisableAirUnits ? 0 : Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsAircraft).Count();
            int numBuildings = Map.Buildings.OfType<Building>().Where(x => x.Occupier.IsPrebuilt).Count();
            int numInfantry = Map.Technos.OfType<InfantryGroup>().Sum(item => item.Occupier.Infantry.Count(i => i != null));
            int numTerrain = Map.Technos.OfType<Terrain>().Count();
            int numUnits = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsGroundUnit).Count();
            int numVessels = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsVessel).Count();
            info.Add("Objects overview:");

            const string maximums = "Number of {0}: {1}. Maximum: {2}.";
            if (!Globals.DisableAirUnits)
            {
                info.Add(string.Format(maximums, "aircraft", numAircraft, Constants.MaxAircraft));
            }
            info.Add(string.Format(maximums, "structures", numBuildings, Constants.MaxBuildings));
            info.Add(string.Format(maximums, "infantry", numInfantry, Constants.MaxInfantry));
            info.Add(string.Format(maximums, "terrain objects", numTerrain, Constants.MaxTerrain));
            info.Add(string.Format(maximums, "units", numUnits, Constants.MaxUnits));
            info.Add(string.Format(maximums, "ships", numVessels, Constants.MaxVessels));
            info.Add(string.Format(maximums, "team types", Map.TeamTypes.Count, Constants.MaxTeams));
            info.Add(string.Format(maximums, "triggers", Map.Triggers.Count, Constants.MaxTriggers));
            if (!Map.BasicSection.SoloMission)
            {
                info.Add(String.Empty);
                info.Add("Multiplayer info:");
                int startPoints = Map.Waypoints.Count(w => w.Cell.HasValue && w.Flag.HasFlag(WaypointFlag.PlayerStart));
                info.Add(string.Format("Number of set starting points: {0}.", startPoints));
            }
            HashSet<int> usedWaypoints = new HashSet<int>();
            HashSet<int> setWaypoints = Enumerable.Range(0, Map.Waypoints.Length).Where(i => Map.Waypoints[i].Cell.HasValue).ToHashSet();
            HashSet<string> usedTeams = new HashSet<string>();
            foreach (TeamType tt in Map.TeamTypes)
            {
                string teamName = tt.Name;
                if (tt.IsAutocreate)
                {
                    usedTeams.Add(teamName);
                }
                else
                {
                    foreach (Trigger tr in Map.Triggers)
                    {
                        // "else if" is just to not check all; if it's on one then it's already added anyway.
                        if ((tr.Event1.Team == teamName &&
                                tr.Event1.EventType == EventTypes.TEVENT_LEAVES_MAP) ||
                            (tr.Event2.Team == teamName && tr.EventControl != TriggerMultiStyleType.Only &&
                                tr.Event2.EventType == EventTypes.TEVENT_LEAVES_MAP) ||
                            (tr.Action1.Team == teamName && (
                                tr.Action1.ActionType == ActionTypes.TACTION_CREATE_TEAM ||
                                tr.Action1.ActionType == ActionTypes.TACTION_DESTROY_TEAM ||
                                tr.Action1.ActionType == ActionTypes.TACTION_REINFORCEMENTS)) ||
                            (tr.Action2.Team == teamName && (
                                tr.Action2.ActionType == ActionTypes.TACTION_CREATE_TEAM ||
                                tr.Action2.ActionType == ActionTypes.TACTION_DESTROY_TEAM ||
                                tr.Action2.ActionType == ActionTypes.TACTION_REINFORCEMENTS)))
                        {
                            usedTeams.Add(teamName);
                        }
                    }
                }
                if (tt.Origin != -1)
                {
                    usedWaypoints.Add(tt.Origin);
                }
                foreach (TeamTypeMission tm in tt.Missions)
                {
                    if (tm.Mission.ArgType == TeamMissionArgType.Waypoint && tm.Argument != -1)
                    {
                        usedWaypoints.Add((int)tm.Argument);
                    }
                }
            }
            List<string> unusedTeams = Map.TeamTypes.Select(tm => tm.Name).Where(tn => !usedTeams.Contains(tn)).ToList();
            // Paratrooper Infantry team. This is a special team and thus never 'unused'.
            unusedTeams.Remove("@PINF");
            unusedTeams.Sort();
            string unusedTeamsStr = String.Join(", ", unusedTeams.ToArray());
            HashSet<int> checkedGlobals = new HashSet<int>();
            HashSet<int> alteredGlobals = new HashSet<int>();
            GetUsagesInTriggers(Map.Triggers, checkedGlobals, alteredGlobals, usedWaypoints);
            alteredGlobals.UnionWith(GetTeamGlobals(Map.TeamTypes));
            string usedGlobalsStr = String.Join(", ", checkedGlobals.Union(alteredGlobals).OrderBy(g => g).Select(g => g.ToString()).ToArray());
            string chGlobalsNotEdStr = String.Join(", ", checkedGlobals.Where(g => !alteredGlobals.Contains(g)).OrderBy(g => g).Select(g => g.ToString()).ToArray());
            string edGlobalsNotChStr = String.Join(", ", alteredGlobals.Where(g => !checkedGlobals.Contains(g)).OrderBy(g => g).Select(g => g.ToString()).ToArray());
            WaypointFlag toIgnore = WaypointFlag.Home | WaypointFlag.Reinforce | WaypointFlag.Special;
            string unusedWaypointsStr = String.Join(", ", setWaypoints.OrderBy(w => w)
                .Where(w => (Map.Waypoints[w].Flag & toIgnore) == WaypointFlag.None
                            && !usedWaypoints.Contains(w)).Select(w => Map.Waypoints[w].Name).ToArray());
            // Prevent illegal data in triggers/teams from crashing the function by adding a range check.
            int maxWp = Map.Waypoints.Length;
            // This does not filter out the "Special" waypoint, but that's okay. Anyone using (that will know that is to be expected.
            string unsetUsedWaypointsStr = String.Join(", ", usedWaypoints.OrderBy(w => w).Where(w => !setWaypoints.Contains(w))
                .Select(w => w >= maxWp ? w.ToString() : Map.Waypoints[w].Name).ToArray());
            string evalEmpty(string str)
            {
                return String.IsNullOrEmpty(str) ? "-" : str;
            };
            info.Add(String.Empty);
            info.Add("Scripting remarks:");
            info.Add(String.Format("Unused team types: {0}", evalEmpty(unusedTeamsStr)));
            info.Add(String.Format("Globals used: {0}", evalEmpty(usedGlobalsStr)));
            info.Add(String.Format("Globals altered but never checked: {0}", evalEmpty(edGlobalsNotChStr)));
            info.Add(String.Format("Globals checked but never altered: {0}", evalEmpty(chGlobalsNotEdStr)));
            info.Add(String.Format("Placed waypoints not used in teams or triggers: {0}", evalEmpty(unusedWaypointsStr)));
            info.Add(String.Format("Empty waypoints used in teams or triggers: {0}", evalEmpty(unsetUsedWaypointsStr)));
            return info;
        }

        private void GetUsagesInTriggers(IEnumerable<Trigger> triggers, HashSet<int> checkedGlobals, HashSet<int> alteredGlobals, HashSet<int> usedWaypoints)
        {
            foreach (Trigger tr in triggers)
            {
                if (checkedGlobals != null)
                {
                    if (tr.Event1.EventType == EventTypes.TEVENT_GLOBAL_CLEAR || tr.Event1.EventType == EventTypes.TEVENT_GLOBAL_SET)
                        checkedGlobals.Add((int)tr.Event1.Data);
                    if (tr.EventControl != TriggerMultiStyleType.Only && (tr.Event2.EventType == EventTypes.TEVENT_GLOBAL_CLEAR || tr.Event2.EventType == EventTypes.TEVENT_GLOBAL_SET))
                        checkedGlobals.Add((int)tr.Event2.Data);
                }
                if (alteredGlobals != null)
                {
                    if (tr.Action1.ActionType == ActionTypes.TACTION_CLEAR_GLOBAL || tr.Action1.ActionType == ActionTypes.TACTION_SET_GLOBAL)
                        alteredGlobals.Add((int)tr.Action1.Data);
                    if (tr.Action2.ActionType == ActionTypes.TACTION_CLEAR_GLOBAL || tr.Action2.ActionType == ActionTypes.TACTION_SET_GLOBAL)
                        alteredGlobals.Add((int)tr.Action2.Data);
                }
                if (usedWaypoints != null)
                {
                    if ((tr.Action1.ActionType == ActionTypes.TACTION_DZ || tr.Action1.ActionType == ActionTypes.TACTION_REVEAL_SOME || tr.Action1.ActionType == ActionTypes.TACTION_REVEAL_ZONE) && tr.Action1.Data != -1)
                        usedWaypoints.Add((int)tr.Action1.Data);
                    if ((tr.Action2.ActionType == ActionTypes.TACTION_DZ || tr.Action2.ActionType == ActionTypes.TACTION_REVEAL_SOME || tr.Action2.ActionType == ActionTypes.TACTION_REVEAL_ZONE) && tr.Action2.Data != -1)
                        usedWaypoints.Add((int)tr.Action2.Data);
                }
            }
        }

        private void ClearUnusedTriggerArguments(List<Trigger> triggers)
        {
            foreach (Trigger tr in triggers)
            {
                ClearUnusedEventArgs(tr.Event1, false);
                ClearUnusedActionArgs(tr.Action1);
                ClearUnusedEventArgs(tr.Event2, tr.EventControl == TriggerMultiStyleType.Only);
                ClearUnusedActionArgs(tr.Action2);
            }
        }

        private void ClearUnusedEventArgs(TriggerEvent ev, bool isUnused)
        {
            if (isUnused)
            {
                ev.EventType = TriggerEvent.None;
                ev.Data = 0;
                ev.Team = TeamType.None;
            }
            switch (ev.EventType)
            {
                case EventTypes.TEVENT_NONE:
                case EventTypes.TEVENT_SPIED:
                case EventTypes.TEVENT_DISCOVERED:
                case EventTypes.TEVENT_ATTACKED:
                case EventTypes.TEVENT_DESTROYED:
                case EventTypes.TEVENT_ANY:
                case EventTypes.TEVENT_MISSION_TIMER_EXPIRED:
                case EventTypes.TEVENT_NOFACTORIES:
                case EventTypes.TEVENT_EVAC_CIVILIAN:
                case EventTypes.TEVENT_FAKES_DESTROYED:
                case EventTypes.TEVENT_ALL_BRIDGES_DESTROYED:
                    // Neither team nor data
                    ev.Data = 0;
                    ev.Team = TeamType.None;
                    break;
                case EventTypes.TEVENT_LEAVES_MAP:
                    // Only team
                    ev.Data = 0;
                    break;
                case EventTypes.TEVENT_PLAYER_ENTERED:
                case EventTypes.TEVENT_CROSS_HORIZONTAL:
                case EventTypes.TEVENT_CROSS_VERTICAL:
                case EventTypes.TEVENT_ENTERS_ZONE:
                case EventTypes.TEVENT_LOW_POWER:
                case EventTypes.TEVENT_THIEVED:
                case EventTypes.TEVENT_HOUSE_DISCOVERED:
                case EventTypes.TEVENT_BUILDINGS_DESTROYED:
                case EventTypes.TEVENT_UNITS_DESTROYED:
                case EventTypes.TEVENT_ALL_DESTROYED:
                case EventTypes.TEVENT_BUILDING_EXISTS:
                case EventTypes.TEVENT_BUILD:
                case EventTypes.TEVENT_BUILD_UNIT:
                case EventTypes.TEVENT_BUILD_INFANTRY:
                case EventTypes.TEVENT_BUILD_AIRCRAFT:
                case EventTypes.TEVENT_NUNITS_DESTROYED:
                case EventTypes.TEVENT_NBUILDINGS_DESTROYED:
                case EventTypes.TEVENT_CREDITS:
                case EventTypes.TEVENT_TIME:
                case EventTypes.TEVENT_GLOBAL_SET:
                case EventTypes.TEVENT_GLOBAL_CLEAR:
                    // Only data
                    ev.Team = TeamType.None;
                    break;
            }
        }

        private void ClearUnusedActionArgs(TriggerAction ac)
        {
            switch (ac.ActionType)
            {
                case ActionTypes.TACTION_NONE:
                case ActionTypes.TACTION_WINLOSE:
                case ActionTypes.TACTION_ALLOWWIN:
                case ActionTypes.TACTION_REVEAL_ALL:
                case ActionTypes.TACTION_START_TIMER:
                case ActionTypes.TACTION_STOP_TIMER:
                case ActionTypes.TACTION_CREEP_SHADOW:
                case ActionTypes.TACTION_DESTROY_OBJECT:
                case ActionTypes.TACTION_LAUNCH_NUKES:
                    ac.Trigger = Trigger.None;
                    ac.Team = TeamType.None;
                    ac.Data = 0;
                    break;
                case ActionTypes.TACTION_WIN:
                case ActionTypes.TACTION_LOSE:
                case ActionTypes.TACTION_BEGIN_PRODUCTION:
                case ActionTypes.TACTION_ALL_HUNT:
                case ActionTypes.TACTION_FIRE_SALE:
                case ActionTypes.TACTION_DZ:
                case ActionTypes.TACTION_PLAY_MOVIE:
                case ActionTypes.TACTION_TEXT_TRIGGER:
                case ActionTypes.TACTION_REVEAL_SOME:
                case ActionTypes.TACTION_REVEAL_ZONE:
                case ActionTypes.TACTION_PLAY_SOUND:
                case ActionTypes.TACTION_PLAY_MUSIC:
                case ActionTypes.TACTION_PLAY_SPEECH:
                case ActionTypes.TACTION_ADD_TIMER:
                case ActionTypes.TACTION_SUB_TIMER:
                case ActionTypes.TACTION_SET_TIMER:
                case ActionTypes.TACTION_SET_GLOBAL:
                case ActionTypes.TACTION_CLEAR_GLOBAL:
                case ActionTypes.TACTION_BASE_BUILDING:
                case ActionTypes.TACTION_1_SPECIAL:
                case ActionTypes.TACTION_FULL_SPECIAL:
                case ActionTypes.TACTION_PREFERRED_TARGET:
                case ActionTypes.TACTION_AUTOCREATE:
                    ac.Trigger = Trigger.None;
                    ac.Team = TeamType.None;
                    break;
                case ActionTypes.TACTION_CREATE_TEAM:
                case ActionTypes.TACTION_DESTROY_TEAM:
                case ActionTypes.TACTION_REINFORCEMENTS:
                    ac.Trigger = Trigger.None;
                    ac.Data = 0;
                    break;
                case ActionTypes.TACTION_DESTROY_TRIGGER:
                case ActionTypes.TACTION_FORCE_TRIGGER:
                    ac.Team = TeamType.None;
                    ac.Data = 0;
                    break;
            }
        }

        public HashSet<string> GetHousesWithProduction()
        {
            HashSet<string> housesWithProd = new HashSet<string>();
            if (Map.BasicSection.BasePlayer == null)
            {
                Map.BasicSection.BasePlayer = HouseTypes.GetClassicOpposingPlayer(Map.BasicSection.Player);
            }
            HouseType rebuildHouse = Map.HouseTypes.Where(h => h.Name == Map.BasicSection.BasePlayer).FirstOrDefault();
            housesWithProd.Add(rebuildHouse.Name);
            return housesWithProd;
        }

        public int[] GetRevealRadiusForWaypoints(bool forLargeReveal)
        {
            Waypoint[] waypoints = Map.Waypoints;
            int length = waypoints.Length;
            int[] flareRadius = new int[length];
            for (int i = 0; i < length; i++)
            {
                string actionType = forLargeReveal ? ActionTypes.TACTION_REVEAL_SOME : ActionTypes.TACTION_DZ;
                foreach (Trigger trigger in Map.Triggers)
                {
                    if ((actionType.Equals(trigger.Action1.ActionType, StringComparison.OrdinalIgnoreCase)
                        && trigger.Action1.Data == i)
                        || (actionType.Equals(trigger.Action2.ActionType, StringComparison.OrdinalIgnoreCase)
                        && trigger.Action2.Data == i))
                    {
                        flareRadius[i] = forLargeReveal ? Map.GapRadius : Map.DropZoneRadius;
                    }
                }
            }
            return flareRadius;
        }

        public IEnumerable<string> CheckTriggers(IEnumerable<Trigger> triggers, bool includeExternalData, bool prefixNames, bool fatalOnly, out bool fatal, bool fix, out bool wasFixed)
        {
            Dictionary<long, int> fixedGlobals = new Dictionary<long, int>();
            string setHouse = this.Map.BasicSection.Player;
            HouseType house = string.IsNullOrEmpty(setHouse) ? null : Map.HouseTypes.Where(h => h.Equals(setHouse)).FirstOrDefault();
            return CheckTriggers(triggers, includeExternalData, prefixNames, fatalOnly, out fatal, fix, out wasFixed, null, out _, ref fixedGlobals, house);
        }

        public IEnumerable<string> CheckTriggers(IEnumerable<Trigger> triggers, bool includeExternalData, bool prefixNames, bool fatalOnly, out bool fatal, bool fix,
            out bool wasFixed, HashSet<int> teamGlobals, out List<int> availableGlobals, ref Dictionary<long, int> fixedGlobals, HouseType defaultHouse)
        {
            fatal = false;
            wasFixed = false;
            List<string> curErrors = new List<string>();
            List<string> errors = new List<string>();
            HashSet<int> checkedGlobals = new HashSet<int>();
            HashSet<int> alteredGlobals = new HashSet<int>();
            GetUsagesInTriggers(triggers, checkedGlobals, alteredGlobals, null);
            availableGlobals = !fix ? new List<int>() : Enumerable.Range(0, 30).Where(n => !checkedGlobals.Contains(n) && !alteredGlobals.Contains(n)).ToList();
            if (teamGlobals != null)
            {
                availableGlobals = availableGlobals.Where(n => !teamGlobals.Contains(n)).ToList();
            }
            foreach (Trigger trigger in triggers)
            {
                string trigName = trigger.Name;
                string prefix = prefixNames ? "Trigger \"" + trigName + "\": " : String.Empty;
                // House "None" is fatal on a lot of events.
                CheckTriggerHouse(prefix, trigger, curErrors, ref fatal, fatalOnly, fix, ref wasFixed, defaultHouse);
                // Not sure which ones are truly fatal.
                // Events
                CheckEventHouse(prefix, false, trigger.Event1, curErrors, 1, ref fatal, fatalOnly, fix, ref wasFixed);
                CheckEventHouse(prefix, trigger.UsesEvent2, trigger.Event2, curErrors, 2, ref fatal, fatalOnly, fix, ref wasFixed);
                // globals checks are only for ini read, really.
                CheckEventGlobals(prefix, false, trigger.Event1, curErrors, 1, ref fatal, fatalOnly, fix, ref wasFixed, availableGlobals, fixedGlobals);
                CheckEventGlobals(prefix, trigger.UsesEvent2, trigger.Event2, curErrors, 2, ref fatal, fatalOnly, fix, ref wasFixed, availableGlobals, fixedGlobals);
                CheckEventTeam(prefix, false, trigger.Event1, curErrors, 1, ref fatal, fatalOnly);
                CheckEventTeam(prefix, trigger.UsesEvent2, trigger.Event2, curErrors, 2, ref fatal, fatalOnly);
                // Actions
                CheckActionHouse(prefix, false, trigger.Action1, curErrors, 1, ref fatal, fatalOnly, fix, ref wasFixed);
                CheckActionHouse(prefix, trigger.UsesEvent2, trigger.Action2, curErrors, 2, ref fatal, fatalOnly, fix, ref wasFixed);
                CheckActionText(prefix, false, trigger.Action1, curErrors, 1, ref fatal, fatalOnly, fix, ref wasFixed);
                CheckActionText(prefix, trigger.UsesEvent2, trigger.Action2, curErrors, 2, ref fatal, fatalOnly, fix, ref wasFixed);
                // globals checks are only for ini read, really.
                CheckActionGlobals(prefix, false, trigger.Action1, curErrors, 1, ref fatal, fatalOnly, fix, ref wasFixed, availableGlobals, fixedGlobals);
                CheckActionGlobals(prefix, trigger.UsesEvent2, trigger.Action2, curErrors, 2, ref fatal, fatalOnly, fix, ref wasFixed, availableGlobals, fixedGlobals);
                CheckActionTeam(prefix, false, trigger.Action1, curErrors, 1, ref fatal, fatalOnly);
                CheckActionTeam(prefix, trigger.UsesEvent2, trigger.Action2, curErrors, 2, ref fatal, fatalOnly);
                CheckActionTrigger(prefix, false, trigger.Action1, curErrors, 1, ref fatal, fatalOnly);
                CheckActionTrigger(prefix, trigger.UsesEvent2, trigger.Action2, curErrors, 2, ref fatal, fatalOnly);
                // Waypoints: also only relevant on ini read.
                CheckActionWaypoint(prefix, false, trigger.Action1, curErrors, 1, ref fatal, fatalOnly, fix, ref wasFixed);
                CheckActionWaypoint(prefix, trigger.UsesEvent2, trigger.Action2, curErrors, 2, ref fatal, fatalOnly, fix, ref wasFixed);
                // Specific checks go here:
                // -celltrigger "Entered By" somehow cannot trigger fire sale? Investigate.
                if (curErrors.Count > 0)
                {
                    if (prefixNames)
                    {
                        errors.AddRange(curErrors);
                    }
                    else
                    {
                        errors.Add(trigName + ":");
                        errors.AddRange(curErrors.Select(er => "-" + er));
                        errors.Add(String.Empty);
                    }
                    curErrors.Clear();
                }
            }
            return errors;
        }

        private HashSet<int> GetTeamGlobals(List<TeamType> teamTypes)
        {
            HashSet<int> usedGlobals = new HashSet<int>();
            foreach (TeamType team in teamTypes)
            {
                foreach (TeamTypeMission ttm in team.Missions)
                {
                    if (ttm.Mission.Mission == TeamMissionTypes.SetGlobal.Mission)
                    {
                        usedGlobals.Add((int)(ttm.Argument));
                    }
                }
            }
            return usedGlobals;
        }

        private IEnumerable<string> FixTeamTypeGlobals(IEnumerable<TeamType> teamTypes, List<int> availableGlobals, Dictionary<long, int> fixedGlobals, out bool wasFixed)
        {
            List<string> errors = new List<string>();
            wasFixed = false;
            foreach (TeamType team in teamTypes)
            {
                for (int i = 0; i < team.Missions.Count; i++)
                {
                    TeamTypeMission ttm = team.Missions[i];
                    if (ttm.Mission.Mission == TeamMissionTypes.SetGlobal.Mission && (ttm.Argument < 0 || ttm.Argument > Constants.HighestGlobal))
                    {
                        string error = String.Format("Team \"{0}\" Order {1} has an illegal global value \"{2}\": Globals only go from 0 to {3}.",
                            team.Name, i + 1, ttm.Argument, Constants.HighestGlobal);
                        int fixedVal = -1;
                        if (fixedGlobals != null && fixedGlobals.TryGetValue(ttm.Argument, out int fixVal))
                        {
                            fixedVal = fixVal;
                        }
                        else if (availableGlobals != null && availableGlobals.Count > 0)
                        {
                            fixedVal = availableGlobals[0];
                            availableGlobals.RemoveAt(0);
                            fixedGlobals.Add(ttm.Argument, fixedVal);
                        }
                        ttm.Argument = fixedVal == -1 ? ttm.Argument.Restrict(0, Constants.HighestGlobal) : fixedVal;
                        wasFixed = true;
                        error += fixedVal == -1 ? (" Fixed to \"" + ttm.Argument + "\".") : (" Fixed to available global \"" + fixedVal + "\".");
                        errors.Add(error);
                    }
                }
            }
            return errors;
        }

        private void CheckTriggerHouse(string prefix, Trigger trigger, List<string> errors, ref bool fatal, bool fatalOnly,
            bool fix, ref bool wasFixed, HouseType defaultHouse)
        {
            int house = !string.IsNullOrEmpty(trigger.House) ? (Map.HouseTypes.Where(h => h.Equals(trigger.House)).FirstOrDefault()?.ID ?? -1) : -1;
            string fixHouse = defaultHouse?.Name ?? House.None;
            if (house != -1)
            {
                return;
            }
            string error;
            TriggerEvent[] events = new TriggerEvent[] { trigger.Event1, trigger.Event2 };
            List<int> fatalEvts = new List<int>();
            List<int> warningEvts = new List<int>();
            for (int i = 0; i < 2; i++)
            {
                if (i == 1 && !trigger.UsesEvent2)
                {
                    break;
                }
                TriggerEvent evnt = events[i];
                switch (evnt.EventType)
                {
                    case EventTypes.TEVENT_LEAVES_MAP:
                        warningEvts.Add(i);
                        break;
                    case EventTypes.TEVENT_THIEVED:
                    case EventTypes.TEVENT_HOUSE_DISCOVERED:
                    case EventTypes.TEVENT_ANY:
                    case EventTypes.TEVENT_UNITS_DESTROYED:
                    case EventTypes.TEVENT_BUILDINGS_DESTROYED:
                    case EventTypes.TEVENT_ALL_DESTROYED:
                    case EventTypes.TEVENT_CREDITS:
                    case EventTypes.TEVENT_NBUILDINGS_DESTROYED:
                    case EventTypes.TEVENT_NUNITS_DESTROYED:
                    case EventTypes.TEVENT_NOFACTORIES:
                    case EventTypes.TEVENT_EVAC_CIVILIAN:
                    case EventTypes.TEVENT_BUILD:
                    case EventTypes.TEVENT_BUILD_UNIT:
                    case EventTypes.TEVENT_BUILD_INFANTRY:
                    case EventTypes.TEVENT_BUILD_AIRCRAFT:
                    case EventTypes.TEVENT_LOW_POWER:
                    case EventTypes.TEVENT_BUILDING_EXISTS:
                        fatalEvts.Add(i);
                        break;
                }
            }
            if (fatalEvts.Count > 0)
            {
                fatal = true;
                string eventInfo = String.Join(" and ", fatalEvts.Select(ev => String.Format("Event {0} is \"{1}\"", ev + 1, events[ev].EventType.TrimEnd('.'))));
                string eventDesc =
                error = String.Format("{0}House is set to {1}, but {2}. {3} event{4} require{4} a House to be set, or the trigger will cause a game crash on mission load.",
                    prefix, House.None, eventInfo, fatalEvts.Count > 1 ? "Both" : "This", fatalEvts.Count > 1 ? "s" : String.Empty, fatalEvts.Count == 1 ? "s" : String.Empty);
                if (fix && fixHouse != House.None)
                {
                    trigger.House = fixHouse;
                    wasFixed = true;
                    error += " Fixed to \"" + fixHouse + "\" (Player house).";
                }
                errors.Add(error);
            }
            if (!fatalOnly && warningEvts.Count > 0)
            {
                string eventInfo = String.Join(" and ", warningEvts.Select(ev => String.Format("Event {0} is \"{1}\"", ev + 1, events[ev].EventType.TrimEnd('.'))));
                string eventDesc =
                error = String.Format("{0}House is set to {1}, but {2}. {3} event{4} require{5} a House to be set, or the trigger will immediately fire at the start of the mission.",
                    prefix, House.None, eventInfo, warningEvts.Count > 1 ? "Both" : "This", warningEvts.Count > 1 ? "s" : String.Empty, warningEvts.Count == 1 ? "s" : String.Empty);
                if (fix && fixHouse != House.None)
                {
                    trigger.House = fixHouse;
                    wasFixed = true;
                    error += " Fixed to \"" + fixHouse + "\" (Player house).";
                }
                errors.Add(error);
            }
        }

        private void CheckEventHouse(string prefix, bool skipCheck, TriggerEvent evnt, List<string> errors, int nr, ref bool fatal, bool fatalOnly, bool fix, ref bool wasFixed)
        {
            if (skipCheck)
            {
                return;
            }
            int maxId = Map.Houses.Max(h => h.Type.ID);
            long house = evnt.Data;
            if ((house >= 0 && house <= maxId) || fatalOnly)
            {
                return;
            }
            switch (evnt.EventType)
            {
                // celltrigger events
                case EventTypes.TEVENT_PLAYER_ENTERED:
                case EventTypes.TEVENT_CROSS_HORIZONTAL:
                case EventTypes.TEVENT_CROSS_VERTICAL:
                case EventTypes.TEVENT_ENTERS_ZONE:
                // trigger house event, with arg as cause
                case EventTypes.TEVENT_THIEVED:
                // arg house event
                case EventTypes.TEVENT_LOW_POWER: // not fatal
                case EventTypes.TEVENT_HOUSE_DISCOVERED:
                case EventTypes.TEVENT_BUILDINGS_DESTROYED:
                case EventTypes.TEVENT_UNITS_DESTROYED:
                case EventTypes.TEVENT_ALL_DESTROYED:
                    string error;
                    if (house < -1 || house > maxId)
                    {
                        error = prefix + "Event " + nr + ": \"" + evnt.EventType.TrimEnd('.') + "\" has an illegal house id \"" + evnt.Data + "\".";
                        if (fix)
                        {
                            evnt.Data = -1; // 'fix' to -1, so it at least has a valid UI value.
                            wasFixed = true;
                            error += " Fixed to \"-1\" (" + House.None + ").";
                        }
                    }
                    else
                    {
                        // case "-1" is all that remains here. Never fix this case; it's user responsibility.
                        error = prefix + "Event " + nr + ": \"" + evnt.EventType.TrimEnd('.') + "\" requires a house to be set.";
                    }
                    errors.Add(error);
                    break;
            }
        }

        private void CheckEventGlobals(string prefix, bool skipCheck, TriggerEvent evnt, List<string> errors, int nr, ref bool fatal, bool fatalOnly, bool fix, ref bool wasFixed, List<int> availableGlobals, Dictionary<long,int> fixedGlobals)
        {
            if (skipCheck || fatalOnly)
            {
                return;
            }
            switch (evnt.EventType)
            {
                case EventTypes.TEVENT_GLOBAL_SET:
                case EventTypes.TEVENT_GLOBAL_CLEAR:
                    if (evnt.Data < 0 || evnt.Data > Constants.HighestGlobal)
                    {
                        string error = String.Format("{0}Event \"{1}\" has an illegal global value \"{2}\": Globals only go from 0 to {3}.",
                            prefix, nr, evnt.Data, Constants.HighestGlobal);
                        if (fix)
                        {
                            int fixedVal = -1;
                            if (fixedGlobals.TryGetValue(evnt.Data, out int fixVal))
                            {
                                fixedVal = fixVal;
                            }
                            else if (availableGlobals.Count > 0)
                            {
                                fixedVal = availableGlobals[0];
                                availableGlobals.RemoveAt(0);
                                fixedGlobals.Add(evnt.Data, fixedVal);
                            }
                            evnt.Data = fixedVal == -1 ? evnt.Data.Restrict(0, Constants.HighestGlobal) : fixedVal;
                            wasFixed = true;
                            error += fixedVal == -1 ? (" Fixed to \"" + evnt.Data + "\".") : (" Fixed to available global \"" + evnt.Data + "\".");
                        }
                        errors.Add(error);
                    }
                    break;
            }
        }

        private void CheckEventTeam(string prefix, bool skipCheck, TriggerEvent evnt, List<string> errors, int nr, ref bool fatal, bool fatalOnly)
        {
            if (skipCheck || !TeamType.IsEmpty(evnt.Team) || fatalOnly)
            {
                return;
            }
            switch (evnt.EventType)
            {
                case EventTypes.TEVENT_LEAVES_MAP:
                    errors.Add(prefix + "Event " + nr + ": There is no team set to leave the map.");
                    break;
            }
        }

        private void CheckActionHouse(string prefix, bool skipCheck, TriggerAction action, List<string> errors, int nr, ref bool fatal, bool fatalOnly, bool fix, ref bool wasFixed)
        {
            if (skipCheck || fatalOnly)
            {
                return;
            }
            int maxId = Map.Houses.Max(h => h.Type.ID);
            long house = action.Data;
            string actn = action.ActionType;
            if (house > -1 && house <= maxId)
            {
                // In range
                string houseName = Map.Houses[house].Type.Name;
                bool houseIsPlayer = houseName.Equals(Map.BasicSection.Player, StringComparison.OrdinalIgnoreCase);
                if (!houseIsPlayer)
                {
                    switch (actn)
                    {
                        case ActionTypes.TACTION_WIN:
                            errors.Add(String.Format("{0}Action {1}: \"{2}\" on a House other than the player will always make you lose. Use a normal \"{3}\" trigger intead.",
                                prefix, nr, actn.TrimEnd('.'), ActionTypes.TACTION_LOSE.TrimEnd('.')));
                            break;
                        case ActionTypes.TACTION_LOSE:
                            errors.Add(String.Format("{0}Action {1}: \"{2}\" on a House other than the player will always make you win. Use a normal \"{3}\" trigger intead.",
                                prefix, nr, actn.TrimEnd('.'), ActionTypes.TACTION_WIN.TrimEnd('.')));
                            break;
                    }
                }
                else
                {
                    switch (actn)
                    {
                        case ActionTypes.TACTION_BEGIN_PRODUCTION:
                        case ActionTypes.TACTION_FIRE_SALE:
                        case ActionTypes.TACTION_AUTOCREATE:
                        case ActionTypes.TACTION_ALL_HUNT:
                            errors.Add(prefix + "Action " + nr + ": \"" + actn.TrimEnd('.') + "\" is set to the player's House. These are AI actions.");
                            break;
                    }
                }
                return;
            }
            switch (actn)
            {
                case ActionTypes.TACTION_WIN:
                case ActionTypes.TACTION_LOSE:
                case ActionTypes.TACTION_BEGIN_PRODUCTION:
                case ActionTypes.TACTION_FIRE_SALE:
                case ActionTypes.TACTION_AUTOCREATE:
                case ActionTypes.TACTION_ALL_HUNT:
                    string error;
                    if (house < -1 || house > maxId)
                    {
                        error = prefix + "Action " + nr + ": \"" + actn.TrimEnd('.') + "\" has an illegal house id \"" + action.Data + "\".";
                        if (fix)
                        {
                            action.Data = -1; // 'fix' to -1, so it at least has a valid UI value.
                            wasFixed = true;
                            error += " Fixed to \"" + action.Data + "\" (" + House.None + ").";
                        }
                    }
                    else
                    {
                        // case "-1" is all that remains here. Never fix this case; it's user responsibility.
                        error = prefix + "Action " + nr + ": \"" + actn.TrimEnd('.') + "\" requires a house to be set.";
                    }
                    errors.Add(error);
                    break;
            }
        }

        private void CheckActionText(string prefix, bool skipCheck, TriggerAction action, List<string> errors, int nr, ref bool fatal, bool fatalOnly, bool fix, ref bool wasFixed)
        {
            if (skipCheck)
            {
                return;
            }
            switch (action.ActionType)
            {
                case ActionTypes.TACTION_TEXT_TRIGGER:
                    if (action.Data < 1 || action.Data > 209)
                    {
                        string error = prefix + (fatalOnly ? String.Empty : "[FATAL] - ") + "Action " + nr + "has an illegal value \"" + action.Data + "\": \"Text triggers only go from 1 to 209.";
                        fatal = true;
                        if (fix)
                        {
                            action.Data = action.Data.Restrict(1, 209);
                            wasFixed = true;
                            error += " Fixed to \"" + action.Data + "\".";
                        }
                        errors.Add(error);
                    }
                    break;
            }
        }

        private void CheckActionGlobals(string prefix, bool skipCheck, TriggerAction action, List<string> errors, int nr, ref bool fatal, bool fatalOnly, bool fix, ref bool wasFixed, List<int> availableGlobals, Dictionary<long, int> globalFixes)
        {
            if (skipCheck || fatalOnly)
            {
                return;
            }
            switch (action.ActionType)
            {
                case ActionTypes.TACTION_SET_GLOBAL:
                case ActionTypes.TACTION_CLEAR_GLOBAL:
                    if (action.Data < 0 || action.Data > Constants.HighestGlobal)
                    {
                        string error = String.Format("{0}Action \"{1}\" has an illegal global value \"{2}\": Globals only go from 0 to {3}.",
                            prefix, nr, action.Data, Constants.HighestGlobal);
                        if (fix)
                        {
                            int fixedVal = -1;
                            if (globalFixes.TryGetValue(action.Data, out int fixVal))
                            {
                                fixedVal = fixVal;
                            }
                            else if (availableGlobals.Count > 0)
                            {
                                fixedVal = availableGlobals[0];
                                availableGlobals.RemoveAt(0);
                                globalFixes.Add(action.Data, fixedVal);
                            }
                            action.Data = fixedVal == -1 ? action.Data.Restrict(0, Constants.HighestGlobal) : fixedVal;
                            wasFixed = true;
                            error += fixedVal == -1 ? (" Fixed to \"" + action.Data + "\".") : (" Fixed to available global \"" + action.Data + "\".");
                        }
                        errors.Add(error);
                    }
                    break;
            }
        }

        private void CheckActionTeam(string prefix, bool skipCheck, TriggerAction action, List<string> errors, int nr, ref bool fatal, bool fatalOnly)
        {
            if (skipCheck || !TeamType.IsEmpty(action.Team) || fatalOnly)
            {
                return;
            }
            switch (action.ActionType)
            {
                case ActionTypes.TACTION_REINFORCEMENTS:
                    errors.Add(prefix + "Action " + nr + ": There is no team type set to reinforce.");
                    break;
                case ActionTypes.TACTION_CREATE_TEAM:
                    errors.Add(prefix + "Action " + nr + ": There is no team type set to create.");
                    break;
                case ActionTypes.TACTION_DESTROY_TEAM:
                    errors.Add(prefix + "Action " + nr + ": There is no team type set to disband.");
                    break;
            }
        }

        private void CheckActionTrigger(string prefix, bool skipCheck, TriggerAction action, List<string> errors, int nr, ref bool fatal, bool fatalOnly)
        {
            if (skipCheck || !Trigger.IsEmpty(action.Trigger) || fatalOnly)
            {
                return;
            }
            switch (action.ActionType)
            {
                case ActionTypes.TACTION_FORCE_TRIGGER:
                    errors.Add(prefix + "Action " + nr + ": There is no trigger set to force.");
                    break;
                case ActionTypes.TACTION_DESTROY_TRIGGER:
                    errors.Add(prefix + "Action " + nr + ": There is no trigger set to destroy.");
                    break;
            }
        }

        private void CheckActionWaypoint(string prefix, bool skipCheck, TriggerAction act, List<string> errors, int nr, ref bool fatal, bool fatalOnly, bool fix, ref bool wasFixed)
        {
            if (skipCheck)
            {
                return;
            }
            int maxId = Map.Waypoints.Length - 1;
            long wayPoint = act.Data;
            if ((wayPoint >= 0 && wayPoint <= maxId) || fatalOnly)
            {
                return;
            }
            switch (act.ActionType)
            {
                case ActionTypes.TACTION_DZ:
                case ActionTypes.TACTION_REVEAL_SOME:
                case ActionTypes.TACTION_REVEAL_ZONE:
                    if (wayPoint < 0 || wayPoint > maxId)
                    {
                        string error = prefix + "Action " + nr + ": \"" + act.ActionType.TrimEnd('.') + "\" ";
                        if (act.Data == -1)
                        {
                            // Special case: warn, don't fix.
                            error += "has an empty waypoint.";
                        }
                        else
                        {
                            error += "has an illegal waypoint value \"" + act.Data + "\".";
                            if (fix)
                            {
                                act.Data = -1;
                                wasFixed = true;
                                error += " Fixing to \"-1\" (None).";
                            }
                        }
                        errors.Add(error);
                    }
                    break;
            }
        }

        public string TriggerSummary(Trigger trigger, bool withLineBreaks)
        {
            string[][] eventControlStrings =
            {
                    new[] { "{0} → {2}" ,"{0}\n  → {2}" },
                    new[] { "{0} AND {1} → {2}",  "{0} AND {1}\n  → {2}"},
                    new[] { "{0} OR {1} → {2}",  "{0} OR {1}\n  → {2}"},
                    new[] { "{0} → {2}; {1} → {3}",  "{0} → {2};\n{1} → {3}" },
            };
            // name, house, repeat status, event control
            string trigFormat = !withLineBreaks ? "{0}: {1}, {2}, {3}" : "{0}: {1}, {2},\n{3}";
            string evtControlFormat = eventControlStrings[(int)trigger.EventControl][!withLineBreaks ? 0 : 1];
            string persistence = GameInfo.PERSISTENCE_NAMES[(int)trigger.PersistentType];
            string evt1 = GetEventString(trigger.Event1);
            string evt2 = GetEventString(trigger.Event2);
            string act1 = GetActionString(trigger.Action1);
            string act2 = GetActionString(trigger.Action2);
            if (trigger.EventControl != TriggerMultiStyleType.Linked
                && !TriggerAction.None.Equals(act2, StringComparison.OrdinalIgnoreCase))
            {
                act1 = act1 + " + " + act2;
            }
            string evtControl = String.Format(evtControlFormat, evt1, evt2, act1, act2);
            return String.Format(trigFormat, trigger.Name, trigger.House, persistence, evtControl);
        }

        private string GetEventString(TriggerEvent evt)
        {
            string eventStr = (evt.EventType ?? TriggerEvent.None).TrimEnd('.');
            string eventArg = null;
            switch (evt.EventType)
            {
                case EventTypes.TEVENT_LEAVES_MAP:
                    eventArg = evt.Team ?? TeamType.None;
                    break;
                case EventTypes.TEVENT_PLAYER_ENTERED:
                case EventTypes.TEVENT_CROSS_HORIZONTAL:
                case EventTypes.TEVENT_CROSS_VERTICAL:
                case EventTypes.TEVENT_ENTERS_ZONE:
                case EventTypes.TEVENT_LOW_POWER:
                case EventTypes.TEVENT_THIEVED:
                case EventTypes.TEVENT_HOUSE_DISCOVERED:
                case EventTypes.TEVENT_BUILDINGS_DESTROYED:
                case EventTypes.TEVENT_UNITS_DESTROYED:
                case EventTypes.TEVENT_ALL_DESTROYED:
                    Model.House house = Map.Houses.FirstOrDefault(h => h.Type.ID == evt.Data);
                    eventArg = house?.Type?.Name ?? House.None;
                    break;
                case EventTypes.TEVENT_BUILDING_EXISTS:
                case EventTypes.TEVENT_BUILD:
                    BuildingType bt = Map.BuildingTypes.FirstOrDefault(b => b.ID == evt.Data);
                    eventArg = bt?.Name ?? evt.Data.ToString();
                    break;
                case EventTypes.TEVENT_BUILD_UNIT:
                    UnitType ut = Map.UnitTypes.FirstOrDefault(u => u.IsGroundUnit && u.ID == evt.Data);
                    eventArg = ut?.Name ?? evt.Data.ToString();
                    break;
                case EventTypes.TEVENT_BUILD_INFANTRY:
                    InfantryType it = Map.InfantryTypes.FirstOrDefault(i => i.ID == evt.Data);
                    eventArg = it?.Name ?? evt.Data.ToString();
                    break;
                case EventTypes.TEVENT_BUILD_AIRCRAFT:
                    UnitType at = Map.TeamTechnoTypes.OfType<UnitType>().FirstOrDefault(a => a.IsAircraft && a.ID == evt.Data);
                    eventArg = at?.Name ?? evt.Data.ToString();
                    break;
                case EventTypes.TEVENT_NUNITS_DESTROYED:
                case EventTypes.TEVENT_NBUILDINGS_DESTROYED:
                case EventTypes.TEVENT_CREDITS:
                case EventTypes.TEVENT_TIME:
                case EventTypes.TEVENT_GLOBAL_SET:
                case EventTypes.TEVENT_GLOBAL_CLEAR:
                    eventArg = evt.Data.ToString();
                    break;
            }
            return eventArg == null ? eventStr : String.Format(GameInfo.TRIG_ARG_FORMAT, eventStr, eventArg);
        }

        private string GetActionString(TriggerAction act)
        {
            string actionStr = (act.ActionType ?? TriggerAction.None).TrimEnd('.');
            string actionArg = null;
            switch (act.ActionType)
            {
                case ActionTypes.TACTION_CREATE_TEAM:
                case ActionTypes.TACTION_DESTROY_TEAM:
                case ActionTypes.TACTION_REINFORCEMENTS:
                    actionArg = act.Team ?? TeamType.None;
                    break;
                case ActionTypes.TACTION_WIN:
                case ActionTypes.TACTION_LOSE:
                case ActionTypes.TACTION_BEGIN_PRODUCTION:
                case ActionTypes.TACTION_FIRE_SALE:
                case ActionTypes.TACTION_AUTOCREATE:
                case ActionTypes.TACTION_ALL_HUNT:
                    Model.House house = Map.Houses.FirstOrDefault(h => h.Type.ID == act.Data);
                    actionArg = house?.Type?.Name ?? House.None;
                    break;
                case ActionTypes.TACTION_FORCE_TRIGGER:
                case ActionTypes.TACTION_DESTROY_TRIGGER:
                    return String.Format(GameInfo.TRIG_ARG_FORMAT, act, act.Trigger ?? Trigger.None);
                case ActionTypes.TACTION_DZ:
                case ActionTypes.TACTION_REVEAL_SOME:
                case ActionTypes.TACTION_REVEAL_ZONE:
                    Waypoint z = act.Data >= 0 && act.Data < Map.Waypoints.Length ? Map.Waypoints[act.Data] : null;
                    string wpSummary = act.Data.ToString();
                    if (z == null)
                        wpSummary += " [Bad value]";
                    else if (!z.Point.HasValue)
                        wpSummary += " [Not set]";
                    else
                    {
                        Point p = z.Point.Value;
                        wpSummary = String.Format("{0} [{1},{2}] (cell {3})", wpSummary, p.X, p.Y, z.Cell.Value);
                    }
                    actionArg = wpSummary;
                    break;
                case ActionTypes.TACTION_1_SPECIAL:
                case ActionTypes.TACTION_FULL_SPECIAL:
                    actionArg = act.Data >= 0 && act.Data < ActionDataTypes.SuperTypes.Length ? ActionDataTypes.SuperTypes[act.Data] : "??";
                    break;
                case ActionTypes.TACTION_PLAY_MUSIC:
                    actionArg = act.Data >= 0 && act.Data < Map.ThemeTypes.Count ? Map.ThemeTypes[(int)act.Data] : "??";
                    break;
                case ActionTypes.TACTION_PLAY_MOVIE:
                    actionArg = act.Data >= 0 && act.Data < Map.MovieTypes.Count ? Map.MovieTypes[(int)act.Data] : "??";
                    break;
                case ActionTypes.TACTION_PLAY_SOUND:
                    actionArg = act.Data >= 0 && act.Data < ActionDataTypes.VocNames.Length ? ActionDataTypes.VocNames[(int)act.Data] : "??";
                    break;
                case ActionTypes.TACTION_PLAY_SPEECH:
                    actionArg = act.Data >= 0 && act.Data < ActionDataTypes.VoxNames.Length ? ActionDataTypes.VoxNames[(int)act.Data] : "??";
                    break;
                case ActionTypes.TACTION_PREFERRED_TARGET:
                    int count = TeamMissionTypes.Attack.DropdownOptions.Count(vl => vl.Value == act.Data);
                    actionArg = count == 0 ? "??" : TeamMissionTypes.Attack.DropdownOptions.First(t => t.Value == act.Data).Label;
                    break;
                case ActionTypes.TACTION_BASE_BUILDING:
                    actionArg = act.Data == 0 ? "Off" : act.Data == 1 ? "On" : "??";
                    break;
                case ActionTypes.TACTION_TEXT_TRIGGER:
                    actionArg = act.Data.ToString("000") + " " + (act.Data >= 1 && act.Data <= ActionDataTypes.TextDesc.Length ? ActionDataTypes.TextDesc[(int)act.Data - 1] : "(??)");
                    break;
                case ActionTypes.TACTION_ADD_TIMER:
                case ActionTypes.TACTION_SUB_TIMER:
                case ActionTypes.TACTION_SET_TIMER:
                case ActionTypes.TACTION_SET_GLOBAL:
                case ActionTypes.TACTION_CLEAR_GLOBAL:
                    actionArg = act.Data.ToString();
                    break;
            }
            return actionArg == null ? actionStr : String.Format(GameInfo.TRIG_ARG_FORMAT, actionStr, actionArg);
        }

        public ITeamColor[] GetFlagColors()
        {
            ITeamColor[] flagColors = new ITeamColor[8];
            foreach (HouseType house in Map.HouseTypes)
            {
                int mpId = Waypoint.GetMpIdFromFlag(house.MultiplayIdentifier);
                if (mpId == -1)
                {
                    continue;
                }
                flagColors[mpId] = Globals.TheTeamColorManager[house.UnitTeamColor];
            }
            return flagColors;
        }


        private LandIniSection GetLandInfo(LandType landType)
        {
            switch (landType)
            {
                case LandType.Clear: return this.LandClear;
                case LandType.Beach: return this.LandBeach;
                case LandType.Road: return this.LandRoad;
                case LandType.Rough: return this.LandRough;
                case LandType.Rock: return this.LandRock;
                case LandType.River: return this.LandRiver;
                case LandType.Water: return this.LandWater;
            }
            return null;
        }

        private bool IsFullyLandUnitPassable(LandType landType)
        {
            LandIniSection landInfo = GetLandInfo(landType);
            return landInfo != null && (landInfo.Foot > 0 && landInfo.Wheel > 0 && landInfo.Track > 0);
        }

        public bool IsLandUnitPassable(LandType landType)
        {
            LandIniSection landInfo = GetLandInfo(landType);
            return landInfo != null && (landInfo.Foot > 0 || landInfo.Wheel > 0 || landInfo.Track > 0);
        }

        public bool IsBoatPassable(LandType landType)
        {
            LandIniSection landInfo = GetLandInfo(landType);
            return landInfo != null && landInfo.Float > 0;
        }

        public bool IsBuildable(LandType landType)
        {
            LandIniSection landInfo = GetLandInfo(landType);
            return landInfo != null && landInfo.Buildable;
        }

        private void BasicSection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "BasePlayer":
                    UpdateBasePlayerHouse();
                    break;
                case "ExpansionEnabled":
                    bool changed = Map.RemoveExpansionUnits();
                    if (!isLoading && changed)
                    {
                        Dirty = true;
                    }
                    break;
                case "SoloMission":
                    Map.UpdateWaypoints();
                    break;
            }
        }

        private void MapSection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Theater":
                    Map.InitTheater(GameInfo);
                    break;
            }
        }

        private void UpdateBasePlayerHouse()
        {
            if (Map.BasicSection.BasePlayer == null)
            {
                Map.BasicSection.BasePlayer = HouseTypes.GetClassicOpposingPlayer(Map.BasicSection.Player);
                // Updating BasePlayer will trigger PropertyChanged and re-call this function, so no need to continue here.
                return;
            }
            HouseType basePlayer = Map.HouseTypesIncludingSpecials.Where(h => h.Equals(Map.BasicSection.BasePlayer)).FirstOrDefault() ?? Map.HouseTypes.First();
            foreach (var (_, building) in Map.Buildings.OfType<Building>())
            {
                if (!building.IsPrebuilt)
                {
                    building.House = basePlayer;
                }
            }
        }

        private void CompressLCWSection(INISection section, byte[] decompressedBytes)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                foreach (byte[] decompressedChunk in decompressedBytes.Split(8192))
                {
                    byte[] compressedChunk = WWCompression.LcwCompress(decompressedChunk);
                    writer.Write((ushort)compressedChunk.Length);
                    writer.Write((ushort)decompressedChunk.Length);
                    writer.Write(compressedChunk);
                }
                writer.Flush();
                stream.Position = 0;
                string[] values = Convert.ToBase64String(stream.ToArray()).Split(70).ToArray();
                for (int i = 0; i < values.Length; ++i)
                {
                    section[(i + 1).ToString()] = values[i];
                }
            }
        }

        private byte[] DecompressLCWSection(INISection section, int bytesPerCell, List<string> errors, ref bool modified)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> kvp in section)
            {
                sb.Append(kvp.Value);
            }
            byte[] compressedBytes;
            try
            {
                compressedBytes = Convert.FromBase64String(sb.ToString());
            }
            catch (FormatException)
            {
                errors.Add("Failed to unpack [" + section.Name + "] from Base64.");
                modified = true;
                return null;
            }
            int readPtr = 0;
            int writePtr = 0;
            byte[] decompressedBytes = new byte[Map.Metrics.Width * Map.Metrics.Height * bytesPerCell];
            while ((readPtr + 4) <= compressedBytes.Length)
            {
                uint uLength;
                using (BinaryReader reader = new BinaryReader(new MemoryStream(compressedBytes, readPtr, 4)))
                {
                    uLength = reader.ReadUInt32();
                }
                int length = (int)(uLength & 0x0000FFFF);
                readPtr += 4;
                byte[] dest = new byte[8192];
                int readPtr2 = readPtr;
                int decompressed;
                try
                {
                    decompressed = WWCompression.LcwDecompress(compressedBytes, ref readPtr2, dest, 0);
                }
                catch
                {
                    errors.Add("Error decompressing ["+ section.Name + "].");
                    modified = true;
                    return decompressedBytes;
                }
                if (writePtr + decompressed > decompressedBytes.Length)
                {
                    errors.Add("Failed to decompress [" + section.Name + "]: data exceeds map size.");
                    modified = true;
                    return decompressedBytes;
                }
                Array.Copy(dest, 0, decompressedBytes, writePtr, decompressed);
                readPtr += length;
                writePtr += decompressed;
            }
            return decompressedBytes;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try { MapImage?.Dispose(); }
                    catch { /* ignore */ }
                    // Dispose of cached images in type objects. This is non-destructive; the type objects themselves don't actually get disposed.
                    Map.ResetCachedGraphics();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
