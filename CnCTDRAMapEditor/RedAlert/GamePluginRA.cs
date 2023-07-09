//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.

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
        private const int maxBriefLengthClassic = 1022;
        private const int briefLineCutoffClassic = 74;
        private const int multiStartPoints = 8;

        private const int DefaultGoldValue = 25;
        private const int DefaultGemValue = 50;
        private const int DefaultDropZoneRadius = 4;
        private const int DefaultGapRadius = 10;
        private const int DefaultJamRadius = 15;
        private readonly IEnumerable<string> movieTypes;
        private bool isLoading = false;

        private static readonly Regex SinglePlayRegex = new Regex("^SC[A-LN-Z]\\d{2}[EWX][A-EL]$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private const string emptyMapName = "<none>";
        private const string movieEmpty = "<none>";
        private const string RemarkOld = " (Classic only)";

        private static readonly IEnumerable<string> movieTypesRemarksOld = new string[]
        {
            "SHIPYARD", // MISSING
            "ENGLISH",  // High res. Intro
            "SIZZLE",   //MISSING
            "SIZZLE2",  //MISSING
        };

        private const string RemarkNew = " (Remaster only)";

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
            "SIZZLE",   //MISSING
            "SIZZLE2",  //MISSING
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
            "No Theme",
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

        private static readonly IEnumerable<ITechnoType> fullTechnoTypes;

        public string Name => "Red Alert";

        public string DefaultExtension => ".mpr";

        public GameType GameType => GameType.RedAlert;

        public HouseType ActiveHouse { get; set; }

        public bool IsMegaMap => true;

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

        public IEnumerable<string> SetExtraIniText(String extraIniText, out bool footPrintsChanged)
        {
            return SetExtraIniText(extraIniText, this.Map.BasicSection.SoloMission, this.Map.BasicSection.ExpansionEnabled, false, out footPrintsChanged);
        }

        public IEnumerable<string> TestSetExtraIniText(String extraIniText, bool isSolo, bool expansionEnabled, out bool footPrintsChanged)
        {
            return SetExtraIniText(extraIniText, isSolo, expansionEnabled, true, out footPrintsChanged);
        }

        public IEnumerable<string> SetExtraIniText(String extraIniText, bool isSolo, bool expansionEnabled, bool forFootprintTest, out bool footPrintsChanged)
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
            IEnumerable<string> errors = ResetRules(extraTextIni, isSolo, expansionEnabled, forFootprintTest, out footPrintsChanged);
            if (!forFootprintTest)
            {
                extraSections = extraTextIni.Sections.Count == 0 ? null : extraTextIni.Sections;
            }
            return errors;
        }

        /// <summary>
        /// Trims the given extra ini content to just unmanaged information,
        /// resets the plugin's rules to their defaults, and then applies any
        /// rules in the given extra ini content to the plugin.
        /// </summary>
        /// <param name="extraTextIni">Ini content that remains after parsing an ini file. If null, only a rules reset is performed.</param>
        /// <param name="footPrintsChanged">Returns true if any building footprints were changed as a result of the ini rule changes.</param>
        /// <returns>Any errors in parsing the <paramref name="extraTextIni"/> contents.</returns>
        private IEnumerable<string> ResetRules(INI extraTextIni)
        {
            return ResetRules(extraTextIni, this.Map.BasicSection.SoloMission, this.Map.BasicSection.ExpansionEnabled, false, out _);
        }

        /// <summary>
        /// Trims the given extra ini content to just unmanaged information,
        /// resets the plugin's rules to their defaults, and then applies any
        /// rules in the given extra ini content to the plugin.
        /// </summary>
        /// <param name="extraTextIni">Ini content that remains after parsing an ini file. If null, only a rules reset is performed.</param>
        /// <param name="isSolo">True if this operation should consider this as singleplayer mission.</param>
        /// <param name="expansionEnabled">True if this operation should consider expansions to be enabled.</param>
        /// <param name="forFootprintTest">Don't apply changes, just test the result for <paramref name="footPrintsChanged"/></param>
        /// <param name="footPrintsChanged">Returns true if any building footprints were changed as a result of the ini rule changes.</param>
        /// <returns>Any errors in parsing the <paramref name="extraTextIni"/> contents.</returns>
        private IEnumerable<string> ResetRules(INI extraTextIni, bool isSolo, bool expansionEnabled, bool forFootprintTest, out bool footPrintsChanged)
        {
            if (extraTextIni != null && !forFootprintTest)
            {
                // Strip "NewUnitsEnabled" from the Aftermath section.
                INISection amSection = extraTextIni.Sections["Aftermath"];
                if (amSection != null)
                {
                    amSection.Keys.Remove("NewUnitsEnabled");
                }
                // Remove any sections known and handled / disallowed by the editor.
                extraTextIni.Sections.Remove("Digest");
                INITools.ClearDataFrom(extraTextIni, "Basic", (BasicSection)Map.BasicSection);
                INITools.ClearDataFrom(extraTextIni, "Map", Map.MapSection);
                extraTextIni.Sections.Remove("Steam");
                extraTextIni.Sections.Remove("TeamTypes");
                extraTextIni.Sections.Remove("Trigs");
                extraTextIni.Sections.Remove("MapPack");
                extraTextIni.Sections.Remove("Terrain");
                extraTextIni.Sections.Remove("OverlayPack");
                extraTextIni.Sections.Remove("Smudge");
                extraTextIni.Sections.Remove("Units");
                extraTextIni.Sections.Remove("Aircraft");
                extraTextIni.Sections.Remove("Ships");
                extraTextIni.Sections.Remove("Infantry");
                extraTextIni.Sections.Remove("Structures");
                extraTextIni.Sections.Remove("Base");
                extraTextIni.Sections.Remove("Waypoints");
                extraTextIni.Sections.Remove("CellTriggers");
                extraTextIni.Sections.Remove("Briefing");
                foreach (House house in Map.Houses)
                {
                    INITools.ClearDataFrom(extraTextIni, house.Type.Name, house);
                }
            }
            Dictionary<string, bool> bibBackups = Map.BuildingTypes.ToDictionary(b => b.Name, b => b.HasBib, StringComparer.OrdinalIgnoreCase);
            if (this.rulesIni != null)
            {
                UpdateRules(rulesIni, this.Map, forFootprintTest);
            }
            if (this.aftermathRulesIni != null && expansionEnabled)
            {
                UpdateRules(aftermathRulesIni, this.Map, forFootprintTest);
            }
            if (this.multiplayRulesIni != null && !isSolo)
            {
                UpdateRules(multiplayRulesIni, this.Map, forFootprintTest);
            }
            IEnumerable<string> errors = null;
            if (extraTextIni != null)
            {
                errors = UpdateRules(extraTextIni, this.Map, forFootprintTest);
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

        private INI rulesIni;
        private INI aftermathRulesIni;
        private INI multiplayRulesIni;

        // Any time a new plugin is made it starts with these defaults. They are then further adapted by the rule reads.
        private readonly RaLandIniSection LandClear = new RaLandIniSection(90, 80, 60, 00, true);
        private readonly RaLandIniSection LandRough = new RaLandIniSection(80, 70, 40, 00, false);
        private readonly RaLandIniSection LandRoad = new RaLandIniSection(100, 100, 100, 00, true);
        private readonly RaLandIniSection LandWater = new RaLandIniSection(00, 00, 00, 100, false);
        private readonly RaLandIniSection LandRock = new RaLandIniSection(00, 00, 00, 00, false);
        private readonly RaLandIniSection LandBeach = new RaLandIniSection(80, 70, 40, 00, false);
        private readonly RaLandIniSection LandRiver = new RaLandIniSection(00, 00, 00, 00, false);

        public static bool CheckForRAMap(INI contents)
        {
            return INITools.CheckForIniInfo(contents, "MapPack");
        }

        static GamePluginRA()
        {
            fullTechnoTypes = InfantryTypes.GetTypes().Cast<ITechnoType>().Concat(UnitTypes.GetTypes(false).Cast<ITechnoType>());
        }

        public GamePluginRA()
            : this(true)
        {
        }

        public GamePluginRA(bool mapImage)
        {
            IEnumerable<Waypoint> playerWaypoints = Enumerable.Range(0, multiStartPoints).Select(i => new Waypoint(string.Format("P{0}", i), Waypoint.GetFlagForMpId(i)));
            IEnumerable<Waypoint> generalWaypoints = Enumerable.Range(multiStartPoints, 98 - multiStartPoints).Select(i => new Waypoint(i.ToString()));
            Waypoint[] specialWaypoints = new Waypoint[] { new Waypoint("Home", WaypointFlag.Home), new Waypoint("Reinf.", WaypointFlag.Reinforce), new Waypoint("Special", WaypointFlag.Special) };
            IEnumerable<Waypoint> waypoints = playerWaypoints.Concat(generalWaypoints).Concat(specialWaypoints);
            List<string> movies = new List<string>(movieTypesRa);
            for (int i = 0; i < movies.Count; ++i)
            {
                string vidName = GeneralUtils.AddRemarks(movies[i], movieEmpty, true, movieTypesRemarksOld, RemarkOld);
                movies[i] = GeneralUtils.AddRemarks(vidName, movieEmpty, true, movieTypesRemarksNew, RemarkNew);
            }
            movies.Insert(0, movieEmpty);
            movieTypes = movies.ToArray();
            BasicSection basicSection = new BasicSection();
            basicSection.SetDefault();
            IEnumerable<HouseType> houseTypes = HouseTypes.GetTypes();
            basicSection.Player = houseTypes.First().Name;
            basicSection.BasePlayer = HouseTypes.GetBasePlayer(basicSection.Player);
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


            Map = new Map(basicSection, null, Constants.MaxSize, typeof(House), houseTypes, null,
                TheaterTypes.GetTypes(), TemplateTypes.GetTypes(),
                TerrainTypes.GetTypes(), OverlayTypes.GetTypes(), SmudgeTypes.GetTypes(Globals.ConvertCraters),
                EventTypes.GetTypes(), cellEventTypes, unitEventTypes, structureEventTypes, terrainEventTypes,
                ActionTypes.GetTypes(), cellActionTypes, unitActionTypes, structureActionTypes, terrainActionTypes,
                MissionTypes.GetTypes(), MissionTypes.MISSION_GUARD, MissionTypes.MISSION_STOP, MissionTypes.MISSION_HARVEST,
                MissionTypes.MISSION_UNLOAD, DirectionTypes.GetMainTypes(), DirectionTypes.GetAllTypes(), InfantryTypes.GetTypes(),
                UnitTypes.GetTypes(Globals.DisableAirUnits), BuildingTypes.GetTypes(), TeamMissionTypes.GetTypes(),
                fullTechnoTypes, waypoints, DefaultDropZoneRadius, DefaultGapRadius, DefaultJamRadius, movieTypes, movieEmpty, themeTypes, themeEmpty,
                DefaultGoldValue, DefaultGemValue);
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
                Map.BasicSection.Name = emptyMapName;
                UpdateBasePlayerHouse();
                // Initialises rules.
                ResetRules(null);
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
                bool forceSingle = false;
                Byte[] iniBytes;
                switch (fileType)
                {
                    case FileType.INI:
                    case FileType.BIN:
                        {
                            INI ini = new INI();
                            iniBytes = File.ReadAllBytes(path);
                            ParseIniContent(ini, iniBytes);
                            forceSingle = SinglePlayRegex.IsMatch(Path.GetFileNameWithoutExtension(path));
                            errors.AddRange(LoadINI(ini, forceSingle, ref modified));
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
                                INI ini = new INI();
                                using (Stream iniStream = megafile.OpenFile(mprFile))
                                {
                                    iniBytes = iniStream.ReadAllBytes();
                                    ParseIniContent(ini, iniBytes);
                                }
                                errors.AddRange(LoadINI(ini, false, ref modified));
                            }
                        }
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

        private void ParseIniContent(INI ini, Byte[] iniBytes)
        {
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
                if (briefSectionUtf8 != null && briefSectionDos != null && briefSectionUtf8.Keys.Contains("Text"))
                {
                    briefSectionDos.Keys["Text"] = briefSectionUtf8.Keys["Text"];
                }
            }
        }

        private IEnumerable<string> LoadINI(INI ini, bool forceSoloMission, ref bool modified)
        {
            List<string> errors = new List<string>();
            Map.BeginUpdate();
            // Fetch some rules.ini information
            errors.AddRange(UpdateBuildingRules(ini, this.Map, false));
            // Just gonna remove this; I assume it'll be invalid after a re-save anyway.
            ini.Sections.Extract("Digest");
            // Basic info
            BasicSection basic = (BasicSection)Map.BasicSection;
            INISection basicSection = INITools.ParseAndLeaveRemainder(ini, "Basic", basic, new MapContext(Map, true));
            if (basicSection != null)
            {
                List<string> movies = new List<string>(movieTypesRa);
                for (int i = 0; i < movies.Count; ++i)
                {
                    string vidName = GeneralUtils.AddRemarks(movies[i], movieEmpty, true, movieTypesRemarksOld, RemarkOld);
                    movies[i] = GeneralUtils.AddRemarks(vidName, movieEmpty, true, movieTypesRemarksNew, RemarkNew);
                }
                movies.Insert(0, movieEmpty);
                basic.Intro = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Intro, movieEmpty, true, movieTypesRemarksOld, RemarkOld), movieEmpty, true, movieTypesRemarksNew, RemarkNew);
                basic.Intro = GeneralUtils.FilterToExisting(basic.Intro, movieEmpty, true, movies);
                basic.Brief = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Brief, movieEmpty, true, movieTypesRemarksOld, RemarkOld), movieEmpty, true, movieTypesRemarksNew, RemarkNew);
                basic.Brief = GeneralUtils.FilterToExisting(basic.Brief, movieEmpty, true, movies);
                basic.Action = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Action, movieEmpty, true, movieTypesRemarksOld, RemarkOld), movieEmpty, true, movieTypesRemarksNew, RemarkNew);
                basic.Action = GeneralUtils.FilterToExisting(basic.Action, movieEmpty, true, movies);
                basic.Win = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Win, movieEmpty, true, movieTypesRemarksOld, RemarkOld), movieEmpty, true, movieTypesRemarksNew, RemarkNew);
                basic.Win = GeneralUtils.FilterToExisting(basic.Win, movieEmpty, true, movies);
                basic.Win2 = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Win2, movieEmpty, true, movieTypesRemarksOld, RemarkOld), movieEmpty, true, movieTypesRemarksNew, RemarkNew);
                basic.Win2 = GeneralUtils.FilterToExisting(basic.Win2, movieEmpty, true, movies);
                basic.Win3 = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Win3, movieEmpty, true, movieTypesRemarksOld, RemarkOld), movieEmpty, true, movieTypesRemarksNew, RemarkNew);
                basic.Win3 = GeneralUtils.FilterToExisting(basic.Win3, movieEmpty, true, movies);
                basic.Win4 = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Win4, movieEmpty, true, movieTypesRemarksOld, RemarkOld), movieEmpty, true, movieTypesRemarksNew, RemarkNew);
                basic.Win4 = GeneralUtils.FilterToExisting(basic.Win4, movieEmpty, true, movies);
                basic.Lose = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Lose, movieEmpty, true, movieTypesRemarksOld, RemarkOld), movieEmpty, true, movieTypesRemarksNew, RemarkNew);
                basic.Lose = GeneralUtils.FilterToExisting(basic.Lose, movieEmpty, true, movies);
            }
            string plName = Map.BasicSection.Player;
            HouseType player = Map.HouseTypes.Where(t => t.Equals(plName)).FirstOrDefault() ?? Map.HouseTypes.First();
            plName = player.Name;
            Map.BasicSection.Player = plName;
            Map.BasicSection.BasePlayer = HouseTypes.GetBasePlayer(plName);
            bool aftermathEnabled = false;
            // Don't remove from extra sections.
            INISection aftermathSection = ini.Sections["Aftermath"];
            if (aftermathSection != null)
            {
                string amEnabled = aftermathSection.TryGetValue("NewUnitsEnabled");
                aftermathEnabled = amEnabled != null && Int32.TryParse(amEnabled, out int val) && val == 1;
                aftermathSection.Keys.Remove("NewUnitsEnabled");
                // Remove if empty.
                if (aftermathSection.Empty)
                    ini.Sections.Remove(aftermathSection.Name);
            }
            // Needs to be enabled in advance; it determines which units are valid to have placed on the map.
            Map.BasicSection.ExpansionEnabled = aftermathEnabled;
            // Map info
            INISection mapSection = INITools.ParseAndLeaveRemainder(ini, "Map", Map.MapSection, new MapContext(Map, true));
            Map.MapSection.FixBounds();
#if DEBUG
            //MessageBox.Show("Graphics loaded");
#endif
            // Steam info
            INISection steamSection = ini.Sections.Extract("Steam");
            if (steamSection != null)
            {
                // Ignore any errors in this.
                INI.ParseSection(new MapContext(Map, true), steamSection, Map.SteamSection, true);
            }
            T indexToType<T>(IList<T> list, string index, bool defnull)
            {
                return (int.TryParse(index, out int result) && (result >= 0) && (result < list.Count)) ? list[result] : (defnull ? default(T) : list.First());
            }
            INISection teamTypesSection = ini.Sections.Extract("TeamTypes");
            List<TeamType> teamTypes = new List<TeamType>();
            if (teamTypesSection != null)
            {
                foreach (var (Key, Value) in teamTypesSection)
                {
                    try
                    {
                        if (Key.Length > 8)
                        {
                            errors.Add(string.Format("TeamType '{0}' has a name that is longer than 8 characters. This will not be corrected by the loading process, but should be addressed, since it can make the teams fail to read correctly, and might even crash the game.", Key));
                        }
                        TeamType teamType = new TeamType { Name = Key };
                        List<string> tokens = Value.Split(',').ToList();
                        teamType.House = Map.HouseTypes.Where(t => t.Equals(sbyte.Parse(tokens[0]))).FirstOrDefault();
                        if (teamType.House == null)
                        {
                            HouseType defHouse = Map.HouseTypes.First();
                            errors.Add(string.Format("Team '{0}' has unknown house ID '{1}'; clearing to '{2}'.", Key, tokens[0], defHouse.Name));
                            modified = true;
                            teamType.House = defHouse;
                        }
                        tokens.RemoveAt(0);
                        int flags = int.Parse(tokens[0]); tokens.RemoveAt(0);
                        teamType.IsRoundAbout = (flags & 0x01) != 0;
                        teamType.IsSuicide = (flags & 0x02) != 0;
                        teamType.IsAutocreate = (flags & 0x04) != 0;
                        teamType.IsPrebuilt = (flags & 0x08) != 0;
                        teamType.IsReinforcable = (flags & 0x10) != 0;
                        teamType.RecruitPriority = int.Parse(tokens[0]); tokens.RemoveAt(0);
                        teamType.InitNum = byte.Parse(tokens[0]); tokens.RemoveAt(0);
                        teamType.MaxAllowed = byte.Parse(tokens[0]); tokens.RemoveAt(0);
                        teamType.Origin = int.Parse(tokens[0]); tokens.RemoveAt(0);
                        teamType.Trigger = tokens[0]; tokens.RemoveAt(0);
                        int numClasses = int.Parse(tokens[0]); tokens.RemoveAt(0);
                        for (int i = 0; i < Math.Min(Globals.MaxTeamClasses, numClasses); ++i)
                        {
                            string[] classTokens = tokens[0].Split(':'); tokens.RemoveAt(0);
                            if (classTokens.Length == 2)
                            {
                                ITechnoType type = fullTechnoTypes.Where(t => t.Equals(classTokens[0])).FirstOrDefault();
                                byte count = byte.Parse(classTokens[1]);
                                if (type != null)
                                {
                                    if (!aftermathEnabled && type.IsExpansionOnly)
                                    {
                                        errors.Add(string.Format("Team '{0}' contains expansion unit '{1}', but expansion units are not enabled; enabling expansion units.", Key, type.Name));
                                        Map.BasicSection.ExpansionEnabled = aftermathEnabled = true;
                                        modified = true;
                                    }
                                    teamType.Classes.Add(new TeamTypeClass { Type = type, Count = count });
                                }
                                else
                                {
                                    errors.Add(string.Format("Team '{0}' references unknown class '{1}'.", Key, classTokens[0]));
                                    modified = true;
                                }
                            }
                            else
                            {
                                errors.Add(string.Format("Team '{0}' has wrong number of tokens for class index {1} (expecting 2).", Key, i));
                                modified = true;
                            }
                        }
                        int numMissions = int.Parse(tokens[0]); tokens.RemoveAt(0);
                        for (int i = 0; i < Math.Min(Globals.MaxTeamMissions, numMissions); ++i)
                        {
                            string[] missionTokens = tokens[0].Split(':'); tokens.RemoveAt(0);
                            if (missionTokens.Length == 2)
                            {
                                TeamMission mission = indexToType(Map.TeamMissionTypes, missionTokens[0], true);
                                if (mission != null)
                                {
                                    if (Int32.TryParse(missionTokens[1], out int arg))
                                    {
                                        teamType.Missions.Add(new TeamTypeMission { Mission = mission, Argument = arg });
                                    }
                                    else
                                    {
                                        errors.Add(string.Format("Team '{0}', orders index {1} ('{2}') has an incorrect value '{3}'.", Key, i, mission, missionTokens[1]));
                                        modified = true;
                                    }
                                }
                                else
                                {
                                    errors.Add(string.Format("Team '{0}' references unknown orders id '{1}'.", Key, missionTokens[0]));
                                    modified = true;
                                }
                            }
                            else
                            {
                                errors.Add(string.Format("Team '{0}' has wrong number of tokens for orders index {1} (expecting 2).", Key, i));
                                modified = true;
                            }
                        }
                        teamTypes.Add(teamType);
                    }
                    catch (Exception ex)
                    {
                        errors.Add(string.Format("Teamtype '{0}' has errors and can't be parsed: {1}.", Key, ex.Message));
                        modified = true;
                    }
                }
            }
            INISection triggersSection = ini.Sections.Extract("Trigs");
            List<Trigger> triggers = new List<Trigger>();
            if (triggersSection != null)
            {
                void fixEvent(TriggerEvent e)
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
                        case EventTypes.TEVENT_BUILD_AIRCRAFT:
                            if (e.Data != -1)
                            {
                                e.Data &= 0xFF;
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
                foreach (var (Key, Value) in triggersSection)
                {
                    try
                    {
                        string[] tokens = Value.Split(',');
                        if (tokens.Length == 18)
                        {
                            if (Key.Length > 4)
                            {
                                errors.Add(string.Format("Trigger '{0}' has a name that is longer than 4 characters. This will not be corrected by the loading process, but should be addressed, since it can make the triggers fail to link correctly to objects and cell triggers, and might even crash the game.", Key));
                            }
                            Trigger trigger = new Trigger { Name = Key };
                            trigger.PersistentType = (TriggerPersistentType)int.Parse(tokens[0]);
                            trigger.House = Map.HouseTypes.Where(t => t.Equals(sbyte.Parse(tokens[1]))).FirstOrDefault()?.Name;
                            if (trigger.House == null)
                            {
                                errors.Add(string.Format("Trigger '{0}' has unknown house ID '{1}'; clearing to '{2}'.", Key, tokens[0], House.None));
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
                            fixEvent(trigger.Event1);
                            fixEvent(trigger.Event2);
                            fixAction(trigger.Action1);
                            fixAction(trigger.Action2);
                            triggers.Add(trigger);
                        }
                        else
                        {
                            errors.Add(string.Format("Trigger '{0}' has too few tokens (expecting 18).", Key));
                            modified = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add(string.Format("Trigger '{0}' has curErrors and can't be parsed: {1}.", Key, ex.Message));
                        modified = true;
                    }
                }
            }
            //MessageBox.Show("at triggers");
            Dictionary<string, string> caseTrigs = Trigger.None.Yield().Concat(triggers.Select(t => t.Name)).ToDictionary(t => t, StringComparer.OrdinalIgnoreCase);
            HashSet<string> checkCellTrigs = Map.FilterCellTriggers(triggers).Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            HashSet<string> checkUnitTrigs = Trigger.None.Yield().Concat(Map.FilterUnitTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            HashSet<string> checkStrcTrigs = Trigger.None.Yield().Concat(Map.FilterStructureTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            // Terrain objects in RA have no triggers
            //HashSet<string> checkTerrTrigs = Trigger.None.Yield().Concat(Map.FilterTerrainTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            //MessageBox.Show("MapPack");
            INISection mapPackSection = ini.Sections.Extract("MapPack");
            if (mapPackSection != null)
            {
                Map.Templates.Clear();
                byte[] data = DecompressLCWSection(mapPackSection, 3, errors);
                if (data != null)
                {
                    int width = Map.Metrics.Width;
                    int height = Map.Metrics.Height;
                    using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
                    {
                        bool clearedOldClear = false;
                        // Amount of tile 255 detected outside map bounds.
                        int oldClearCount = 0;
                        for (int y = 0; y < height; ++y)
                        {
                            for (int x = 0; x < width; ++x)
                            {
                                ushort typeValue = reader.ReadUInt16();
                                TemplateType templateType = Map.TemplateTypes.Where(t => t.Equals(typeValue)).FirstOrDefault();
                                if (templateType == null && typeValue != 0xFFFF)
                                {
                                    errors.Add(String.Format("Unknown template value {0:X4} at cell [{1},{2}]; clearing.", typeValue, x, y));
                                    modified = true;
                                }
                                else if (templateType != null)
                                {
                                    if ((templateType.Flag & TemplateTypeFlag.Clear) != TemplateTypeFlag.None || (templateType.Flag & TemplateTypeFlag.Group) == TemplateTypeFlag.Group)
                                    {
                                        // No explicitly set Clear terrain allowed. Also no explicitly set versions allowed of the "group" dummy entries.
                                        templateType = null;
                                    }
                                    else if (templateType.Theaters != null && !templateType.Theaters.Contains(Map.Theater))
                                    {
                                        if (typeValue == 255)
                                        {
                                            if (Globals.ConvertRaObsoleteClear)
                                            {
                                                if (!clearedOldClear)
                                                {
                                                    errors.Add(String.Format("Use of obsolete version of 'Clear' terrain detected; clearing."));
                                                    // Not marking as modified for this.
                                                    //modified = true;
                                                    clearedOldClear = true;
                                                }
                                                templateType = null;
                                            }
                                        }
                                        else
                                        {
                                            errors.Add(String.Format("Template '{0}' at cell [{1},{2}] is not available in the set theater; clearing.", templateType.Name.ToUpper(), x, y));
                                            modified = true;
                                            templateType = null;
                                        }
                                    }
                                    if (Globals.ConvertRaObsoleteClear)
                                    {
                                        if (typeValue == 255 && !clearedOldClear)
                                        {
                                            // This means 255 is allowed, and we're in Interior theater.
                                            if (!Map.Bounds.Contains(x, y))
                                            {
                                                // Only count tiles outside the map borders, for the 80% check.
                                                oldClearCount++;
                                            }
                                        }
                                    }
                                }
                                Map.Templates[y, x] = (templateType != null) ? new Template { Type = templateType } : null;
                            }
                        }
                        if (oldClearCount > 0)
                        {
                            int outsideMapSize = width * height - Map.Bounds.Width * Map.Bounds.Height;
                            // Test if more than 80% of the area outside the map is tile 255.
                            if (oldClearCount > outsideMapSize * 8 / 10)
                            {
                                // This is an old map. Clear any 255 tile.
                                errors.Add(String.Format("Use of obsolete version of 'Clear' terrain detected; clearing."));
                                // Not marking as modified for this.
                                //modified = true;
                                for (int y = 0; y < height; ++y)
                                {
                                    for (int x = 0; x < width; ++x)
                                    {
                                        Template cell = Map.Templates[y, x];
                                        if (cell != null && cell.Type.ID == 255)
                                        {
                                            Map.Templates[y, x] = null;
                                        }
                                    }
                                }
                            }
                        }
                        for (int y = 0; y < height; ++y)
                        {
                            for (int x = 0; x < width; ++x)
                            {
                                Byte iconValue = reader.ReadByte();
                                Template template = Map.Templates[y, x];
                                // Prevent loading of illegal tiles.
                                if (template != null)
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
                }
            }
            INISection smudgeSection = ini.Sections.Extract("Smudge");
            if (smudgeSection != null)
            {
                foreach (var (Key, Value) in smudgeSection)
                {
                    int cell;
                    if (!int.TryParse(Key, out cell))
                    {
                        errors.Add(string.Format("Cell for Smudge cannot be parsed. Key: '{0}', value: '{1}'; skipping.", Key, Value));
                        modified = true;
                        continue;
                    }
                    string[] tokens = Value.Split(',');
                    if (tokens.Length == 3)
                    {
                        // Craters other than cr1 don't work right in the game. Replace them by stage-0 cr1.
                        bool badCrater = Globals.ConvertCraters && SmudgeTypes.BadCraters.IsMatch(tokens[0]);
                        SmudgeType smudgeType = badCrater ? SmudgeTypes.Crater1 : Map.SmudgeTypes.Where(t => t.Equals(tokens[0]) && !t.IsAutoBib).FirstOrDefault();
                        if (smudgeType != null)
                        {
                            if (Globals.FilterTheaterObjects && smudgeType.Theaters != null && !smudgeType.Theaters.Contains(Map.Theater))
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
                        errors.Add(string.Format("Smudge on cell '{0}' has wrong number of tokens (expecting 3).", Key));
                        modified = true;
                    }
                }
            }
            INISection unitsSection = ini.Sections.Extract("Units");
            if (unitsSection != null)
            {
                foreach (var (Key, Value) in unitsSection)
                {
                    string[] tokens = Value.Split(',');
                    if (tokens.Length == 7)
                    {
                        UnitType unitType = Map.AllUnitTypes.Where(t => t.IsGroundUnit && t.Equals(tokens[1])).FirstOrDefault();
                        if (unitType == null)
                        {
                            errors.Add(string.Format("Unit '{0}' references unknown unit.", tokens[1]));
                            modified = true;
                            continue;
                        }
                        if (!aftermathEnabled && unitType.IsExpansionOnly)
                        {
                            errors.Add(string.Format("Expansion unit '{0}' encountered, but expansion units are not enabled; enabling expansion units.", unitType.Name));
                            modified = true;
                            Map.BasicSection.ExpansionEnabled = aftermathEnabled = true;
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
                            errors.Add(string.Format("Unit entry '{0}' has wrong number of tokens (expecting 7).", Key));
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
            // Classic game does not support this, so I'm leaving this out by default.
            // It is always extracted, so it doesn't end up with the "extra sections"
            INISection aircraftSection = ini.Sections.Extract("Aircraft");
            if (!Globals.DisableAirUnits && aircraftSection != null)
            {
                foreach (var (Key, Value) in aircraftSection)
                {
                    string[] tokens = Value.Split(',');
                    if (tokens.Length == 6)
                    {
                        UnitType aircraftType = Map.AllUnitTypes.Where(t => t.IsAircraft && t.Equals(tokens[1])).FirstOrDefault();
                        if (aircraftType == null)
                        {
                            errors.Add(string.Format("Aircraft '{0}' references unknown aircraft.", tokens[1]));
                            modified = true;
                            continue;
                        }
                        if (!aftermathEnabled && aircraftType.IsExpansionOnly)
                        {
                            errors.Add(string.Format("Expansion aircraft '{0}' encountered, but expansion units are not enabled; enabling expansion units.", aircraftType.Name));
                            modified = true;
                            Map.BasicSection.ExpansionEnabled = aftermathEnabled = true;
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
                            errors.Add(string.Format("Aircraft entry '{0}' has wrong number of tokens (expecting 6).", Key));
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
            INISection shipsSection = ini.Sections.Extract("Ships");
            if (shipsSection != null)
            {
                foreach (var (Key, Value) in shipsSection)
                {
                    string[] tokens = Value.Split(',');
                    if (tokens.Length == 7)
                    {
                        UnitType vesselType = Map.AllUnitTypes.Where(t => t.IsVessel && t.Equals(tokens[1])).FirstOrDefault();
                        if (vesselType == null)
                        {
                            errors.Add(string.Format("Ship '{0}' references unknown ship.", tokens[1]));
                            modified = true;
                            continue;
                        }
                        if (!aftermathEnabled && vesselType.IsExpansionOnly)
                        {
                            errors.Add(string.Format("Expansion ship '{0}' encountered, but expansion units are not enabled; enabling expansion units.", vesselType.Name));
                            modified = true;
                            Map.BasicSection.ExpansionEnabled = aftermathEnabled = true;
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
                            errors.Add(string.Format("Ship entry '{0}' has wrong number of tokens (expecting 7).", Key));
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
            INISection infantrySection = ini.Sections.Extract("Infantry");
            if (infantrySection != null)
            {
                foreach (var (Key, Value) in infantrySection)
                {
                    string[] tokens = Value.Split(',');
                    if (tokens.Length == 8)
                    {
                        InfantryType infantryType = Map.AllInfantryTypes.Where(t => t.Equals(tokens[1])).FirstOrDefault();
                        if (infantryType == null)
                        {
                            errors.Add(string.Format("Infantry '{0}' references unknown infantry.", tokens[1]));
                            modified = true;
                            continue;
                        }
                        if (!aftermathEnabled && infantryType.IsExpansionOnly)
                        {
                            errors.Add(string.Format("Expansion infantry unit '{0}' encountered, but expansion units are not enabled; enabling expansion units.", infantryType.Name));
                            modified = true;
                            Map.BasicSection.ExpansionEnabled = aftermathEnabled = true;
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
                            errors.Add(string.Format("Infantry entry '{0}' has wrong number of tokens (expecting 8).", Key));
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
            INISection structuresSection = ini.Sections.Extract("Structures");
            if (structuresSection != null)
            {
                foreach (var (Key, Value) in structuresSection)
                {
                    string[] tokens = Value.Split(',');
                    if (tokens.Length > 5)
                    {
                        BuildingType buildingType = Map.BuildingTypes.Where(t => t.Equals(tokens[1])).FirstOrDefault();
                        if (buildingType == null)
                        {
                            errors.Add(string.Format("Structure '{0}' references unknown structure.", tokens[1]));
                            modified = true;
                            continue;
                        }
                        if (Globals.FilterTheaterObjects && buildingType.Theaters != null && !buildingType.Theaters.Contains(Map.Theater))
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
                        if (Map.Buildings.Add(cell, newBld))
                        {
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
                        else
                        {
                            ICellOccupier techno = Map.FindBlockingObject(cell, buildingType, out int blockingCell);
                            string reportCell = blockingCell == -1 ? "<unknown>" : blockingCell.ToString();
                            if (techno is Building building)
                            {
                                bool onBib = false;
                                if (building.Type.HasBib)
                                {
                                    Point newPoint = new Point(cell % Map.Metrics.Width, cell / Map.Metrics.Width);
                                    // Not in main area; must be on bib.
                                    onBib = !OccupierSet<Building>.GetOccupyPoints(Map.Buildings[building].Value, building.Type.BaseOccupyMask).Contains(newPoint);
                                }
                                if (onBib)
                                {
                                    errors.Add(string.Format("Structure '{0}' placed on cell {1} overlaps bib of structure '{2}' in cell {3}; skipping.", buildingType.Name, cell, building.Type.Name, reportCell));
                                    modified = true;
                                }
                                else
                                {
                                    errors.Add(string.Format("Structure '{0}' placed on cell {1} overlaps structure '{2}' in cell {3}; skipping.", buildingType.Name, cell, building.Type.Name, reportCell));
                                    modified = true;
                                }
                            }
                            else if (techno is Overlay overlay)
                            {
                                errors.Add(string.Format("Structure '{0}' placed on cell {1} overlaps overlay '{2}' in cell {3}; skipping.", buildingType.Name, cell, overlay.Type.Name, reportCell));
                                modified = true;
                            }
                            else if (techno is Terrain terrain)
                            {
                                errors.Add(string.Format("Structure '{0}' placed on cell {1} overlaps terrain '{2}' in cell {3}; skipping.", buildingType.Name, cell, terrain.Type.Name, reportCell));
                                modified = true;
                            }
                            else if (techno is InfantryGroup infantry)
                            {
                                errors.Add(string.Format("Structure '{0}' placed on cell {1} overlaps infantry in cell {2}; skipping.", buildingType.Name, cell, reportCell));
                                modified = true;
                            }
                            else if (techno is Unit unit)
                            {
                                errors.Add(string.Format("Structure '{0}' placed on cell {1} overlaps unit '{2}' in cell {3}; skipping.", buildingType.Name, cell, unit.Type.Name, reportCell));
                                modified = true;
                            }
                            else
                            {
                                if (blockingCell != -1)
                                {
                                    errors.Add(string.Format("Structure '{0}' placed on cell {1} overlaps unknown techno in cell {2}; skipping.", buildingType.Name, cell, blockingCell));
                                    modified = true;
                                }
                                else
                                {
                                    errors.Add(string.Format("Structure '{0}' placed on cell {1} crosses outside the map bounds; skipping.", buildingType.Name, cell));
                                    modified = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (tokens.Length < 2)
                        {
                            errors.Add(string.Format("Structure entry '{0}' has wrong number of tokens (expecting 6).", Key));
                            modified = true;
                        }
                        else
                        {
                            errors.Add(string.Format("Structure '{0}' has wrong number of tokens (expecting 6).", tokens[1]));
                            modified = true;
                        }
                    }
                }
            }
            INISection baseSection = ini.Sections.Extract("Base");
            string baseCountStr = baseSection != null ? baseSection.TryGetValue("Count") : null;
            string basePlayerStr = baseSection != null ? baseSection.TryGetValue("Player") : null;
            HouseType basePlayer = Map.HouseTypes.First();
            if (basePlayerStr != null)
            {
                HouseType basePlayerLookup = Map.HouseTypes.Where(t => t.Equals(basePlayerStr)).FirstOrDefault();
                if (basePlayerLookup != null)
                {
                    basePlayer = basePlayerLookup;
                }
            }
            Map.BasicSection.BasePlayer = basePlayer.Name;
            if (baseSection != null)
            {
                if (!Int32.TryParse(baseCountStr, out int baseCount))
                {
                    errors.Add(string.Format("Base count '{0}' is not a valid integer.", baseCountStr));
                    modified = true;
                }
                else
                {
                    baseSection.Keys.Remove("Count");
                    baseSection.Keys.Remove("Player");
                    int curPriorityVal = 0;
                    for (int i = 0; i < baseCount; i++)
                    {
                        string key = i.ToString("D3");
                        string value = baseSection.TryGetValue(key);
                        if (value == null)
                        {
                            continue;
                        }
                        baseSection.Keys.Remove(key);
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
                        if (Globals.FilterTheaterObjects && buildingType.Theaters != null && !buildingType.Theaters.Contains(Map.Theater))
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
                        if (Map.Buildings.OfType<Building>().Where(x => x.Location == location).FirstOrDefault().Occupier is Building building)
                        {
                            building.BasePriority = curPriorityVal;
                        }
                        else
                        {
                            Map.Buildings.Add(cell, new Building()
                            {
                                Type = buildingType,
                                House = basePlayer,
                                Strength = 256,
                                Direction = DirectionTypes.North,
                                BasePriority = curPriorityVal,
                                IsPrebuilt = false
                            });
                        }
                        curPriorityVal++;
                    }
                    foreach (var (Key, Value) in baseSection)
                    {
                        errors.Add(string.Format("Invalid base rebuild priority entry '{0}={1}'.", Key, Value));
                        modified = true;
                    }
                }
            }
            INISection terrainSection = ini.Sections.Extract("Terrain");
            if (terrainSection != null)
            {
                foreach (var (Key, Value) in terrainSection)
                {
                    int cell;
                    if (!int.TryParse(Key, out cell))
                    {
                        errors.Add(string.Format("Cell for terrain cannot be parsed. Key: '{0}', value: '{1}'; skipping.", Key, Value));
                        modified = true;
                        continue;
                    }
                    string name = Value.Split(',')[0];
                    TerrainType terrainType = Map.TerrainTypes.Where(t => t.Equals(name)).FirstOrDefault();
                    if (terrainType != null)
                    {
                        if (Globals.FilterTheaterObjects && terrainType.Theaters != null && !terrainType.Theaters.Contains(Map.Theater))
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
                        if (Map.Technos.Add(cell, newTerr))
                        {
                            //if (!checkTrigs.Contains(newTerr.Trigger))
                            //{
                            //    errors.Add(string.Format("Terrain '{0}' on cell {1} links to unknown trigger '{2}'; clearing trigger..", terrainType, cell, newTerr.Trigger));
                            //    modified = true;
                            //    newTerr.Trigger = Trigger.None;
                            //}
                            //else if (!checkTerrTrigs.Contains(Value))
                            //{
                            //    errors.Add(string.Format("Terrain '{0}' on cell {1} links to trigger '{2}' which does not contain an event or action applicable to terrain; clearing trigger.", terrainType, cell, newTerr.Trigger));
                            //    modified = true;
                            //    newTerr.Trigger = Trigger.None;
                            //}
                        }
                        else
                        {
                            ICellOccupier techno = Map.FindBlockingObject(cell, terrainType, out int blockingCell);
                            string reportCell = blockingCell == -1 ? "<unknown>" : blockingCell.ToString();
                            if (techno is Building building)
                            {
                                errors.Add(string.Format("Terrain '{0}' on cell {1} overlaps structure '{2}' in cell {3}; skipping.", terrainType.Name, cell, building.Type.Name, reportCell));
                                modified = true;
                            }
                            else if (techno is Overlay overlay)
                            {
                                errors.Add(string.Format("Terrain '{0}' on cell {1} overlaps overlay '{2}' in cell {3}; skipping.", terrainType.Name, cell, overlay.Type.Name, reportCell));
                                modified = true;
                            }
                            else if (techno is Terrain terrain)
                            {
                                errors.Add(string.Format("Terrain '{0}' on cell {1} overlaps terrain '{2}' in cell {3}; skipping.", terrainType.Name, cell, terrain.Type.Name, reportCell));
                                modified = true;
                            }
                            else if (techno is InfantryGroup infantry)
                            {
                                errors.Add(string.Format("Terrain '{0}' on cell {1} overlaps infantry in cell {2}; skipping.", terrainType.Name, cell, reportCell));
                                modified = true;
                            }
                            else if (techno is Unit unit)
                            {
                                errors.Add(string.Format("Terrain '{0}' on cell {1} overlaps unit '{2}' in cell {3}; skipping.", terrainType.Name, cell, unit.Type.Name, reportCell));
                                modified = true;
                            }
                            else
                            {
                                if (blockingCell != -1)
                                {
                                    errors.Add(string.Format("Terrain '{0}' placed on cell {1} overlaps unknown techno in cell {2}; skipping.", terrainType.Name, cell, blockingCell));
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
            INISection overlayPackSection = ini.Sections.Extract("OverlayPack");
            if (overlayPackSection != null)
            {
                Map.Overlay.Clear();
                byte[] data = DecompressLCWSection(overlayPackSection, 1, errors);
                if (data != null)
                {
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
                        if (overlayType != null)
                        {
                            if (i < secondRow || i >= lastRow)
                            {
                                errors.Add(string.Format("Overlay can not be placed on the first and last lines of the map. Cell: '{0}', Type: '{1}'; skipping.", i, overlayType.Name));
                                modified = true;
                                continue;
                            }
                            if (Globals.FilterTheaterObjects && overlayType.Theaters != null && !overlayType.Theaters.Contains(Map.Theater))
                            {
                                errors.Add(string.Format("Overlay '{0}' is not available in the set theater; skipping.", overlayType.Name));
                                modified = true;
                                continue;
                            }
                            Map.Overlay[i] = new Overlay { Type = overlayType, Icon = 0 };
                        }
                        else
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
                        }
                    }
                }
            }
            INISection waypointsSection = ini.Sections.Extract("Waypoints");
            if (waypointsSection != null)
            {
                foreach (var (Key, Value) in waypointsSection)
                {
                    if (int.TryParse(Key, out int waypoint))
                    {
                        if (int.TryParse(Value, out int cell))
                        {
                            if ((waypoint >= 0) && (waypoint < Map.Waypoints.Length))
                            {
                                if (Map.Metrics.Contains(cell))
                                {
                                    Map.Waypoints[waypoint].Cell = cell;
                                }
                                else
                                {
                                    Map.Waypoints[waypoint].Cell = null;
                                    if (cell != -1)
                                    {
                                        errors.Add(string.Format("Waypoint {0} cell value {1} out of range (expecting between {2} and {3}).", waypoint, cell, 0, Map.Metrics.Length - 1));
                                        modified = true;
                                    }
                                }
                            }
                            else if (cell != -1)
                            {
                                errors.Add(string.Format("Waypoint {0} out of range (expecting between {1} and {2}).", waypoint, 0, Map.Waypoints.Length - 1));
                                modified = true;
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Waypoint {0} has invalid cell '{1}' (expecting integer).", waypoint, Value));
                            modified = true;
                        }
                    }
                    else
                    {
                        errors.Add(string.Format("Invalid waypoint '{0}' (expecting integer).", Key));
                        modified = true;
                    }
                }
            }
            INISection cellTriggersSection = ini.Sections.Extract("CellTriggers");
            if (cellTriggersSection != null)
            {
                foreach (var (Key, Value) in cellTriggersSection)
                {
                    if (int.TryParse(Key, out int cell))
                    {
                        if (Map.Metrics.Contains(cell))
                        {
                            if (!caseTrigs.ContainsKey(Value))
                            {
                                errors.Add(string.Format("Cell trigger {0} links to unknown trigger '{1}'; skipping.", cell, Value));
                                modified = true;
                            }
                            else if (!checkCellTrigs.Contains(Value))
                            {
                                errors.Add(string.Format("Cell trigger {0} links to trigger '{1}' which does not contain a placeable event; skipping.", cell, Value));
                                modified = true;
                            }
                            else
                            {
                                Map.CellTriggers[cell] = new CellTrigger(caseTrigs[Value]);
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
                        errors.Add(string.Format("Invalid cell trigger '{0}' (expecting integer).", Key));
                        modified = true;
                    }
                }
            }
            INISection briefingSection = ini.Sections.Extract("Briefing");
            if (briefingSection != null)
            {
                if (briefingSection.Keys.Contains("Text"))
                {
                    Map.BriefingSection.Briefing = briefingSection["Text"].Replace("@", Environment.NewLine);
                }
                else
                {
                    Map.BriefingSection.Briefing = string.Join(" ", briefingSection.Keys.Select(k => k.Value)).Replace("@", Environment.NewLine);
                }
            }
            foreach (House house in Map.Houses)
            {
                if (house.Type.ID < 0)
                {
                    continue;
                }
                House gameHouse = (House)house;
                INISection houseSection = INITools.ParseAndLeaveRemainder(ini, gameHouse.Type.Name, gameHouse, new MapContext(Map, true));
                house.Enabled = houseSection != null;
            }
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
            // Sort
            ExplorerComparer comparer = new ExplorerComparer();
            Map.TeamTypes.Clear();
            Map.TeamTypes.AddRange(teamTypes.OrderBy(t => t.Name, comparer));
            UpdateBasePlayerHouse();
            triggers.Sort((x, y) => comparer.Compare(x.Name, y.Name));
#if DEBUG
            //MessageBox.Show("at triggers check");
#endif
            // Keep track of corrected globals.
            List<int> availableGlobals;
            Dictionary<long, int> fixedGlobals = new Dictionary<long, int>();
            HashSet<int> teamGlobals = GetTeamGlobals(teamTypes);
            ClearUnusedTriggerArguments(triggers);
            bool wasFixed;
            errors.AddRange(CheckTriggers(triggers, true, true, false, out _, true, out wasFixed, teamGlobals, out availableGlobals, ref fixedGlobals));
            if (wasFixed)
            {
                modified = true;
            }
            errors.AddRange(FixTeamTypeGlobals(Map.TeamTypes, availableGlobals, fixedGlobals, out wasFixed));
            if (wasFixed)
            {
                modified = true;
            }
            // Won't trigger the notifications.
            Map.Triggers.Clear();
            Map.Triggers.AddRange(triggers);
            // init rules stuff
            errors.AddRange(this.ResetRules(ini));
            extraSections = ini.Sections.Count == 0 ? null : ini.Sections;
            bool switchedToSolo = false;
            if (forceSoloMission && !basic.SoloMission)
            {
                int playerId = player.ID;
                bool hasWinTrigger =
                    Map.Triggers.Any(t => t.Action1.ActionType == ActionTypes.TACTION_WIN && t.Action1.Data == playerId) ||
                    Map.Triggers.Any(t => t.Action1.ActionType == ActionTypes.TACTION_LOSE && t.Action1.Data != playerId) ||
                    Map.Triggers.Any(t => t.Action2.ActionType == ActionTypes.TACTION_WIN && t.Action2.Data == playerId) ||
                    Map.Triggers.Any(t => t.Action2.ActionType == ActionTypes.TACTION_LOSE && t.Action2.Data != playerId);
                bool hasLoseTrigger =
                    Map.Triggers.Any(t => t.Action1.ActionType == ActionTypes.TACTION_LOSE && t.Action1.Data == playerId) ||
                    Map.Triggers.Any(t => t.Action1.ActionType == ActionTypes.TACTION_WIN && t.Action1.Data != playerId) ||
                    Map.Triggers.Any(t => t.Action2.ActionType == ActionTypes.TACTION_LOSE && t.Action2.Data == playerId) ||
                    Map.Triggers.Any(t => t.Action2.ActionType == ActionTypes.TACTION_WIN && t.Action2.Data != playerId);
                switchedToSolo = hasWinTrigger && hasLoseTrigger;
            }
            if (switchedToSolo)
            {
                Map.BasicSection.SoloMission = true;
                if (Globals.ReportMissionDetection || errors.Count > 0)
                {
                    errors.Insert(0, "Filename detected as classic single player mission format, and win and lose trigger detected. Applying \"SoloMission\" flag.");
                }
            }
            Map.EndUpdate();
            return errors;
        }

        public IEnumerable<string> ReadRules(Byte[] rulesFile)
        {
            this.rulesIni = ReadRulesFile(rulesFile);
            if (this.rulesIni != null)
            {
                return UpdateRules(rulesIni, this.Map, false);
            }
            return null;
        }

        public IEnumerable<string> ReadExpandRules(Byte[] rulesFile)
        {
            this.aftermathRulesIni = ReadRulesFile(rulesFile);
            if (this.aftermathRulesIni != null && this.Map.BasicSection.ExpansionEnabled)
            {
                return UpdateRules(aftermathRulesIni, this.Map, false);
            }
            return null;
        }

        public IEnumerable<string> ReadMultiRules(Byte[] rulesFile)
        {
            this.multiplayRulesIni = ReadRulesFile(rulesFile);
            if (this.multiplayRulesIni != null && !this.Map.BasicSection.SoloMission)
            {
                return UpdateRules(multiplayRulesIni, this.Map, false);
            }
            return null;
        }

        private INI ReadRulesFile(Byte[] rulesFile)
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

        private Boolean FixCorruptTiles(Template template, byte iconValue, out byte newIconValue, out string type)
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
        private IEnumerable<string> UpdateRules(INI ini, Map map, bool forFootprintTest)
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

        private IEnumerable<string> UpdateLandTypeRules(INI ini, Map map)
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

        private void ReadLandType(INI ini, Map map, string landType, RaLandIniSection landRules, List<string> errors)
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
            map.TiberiumOrGoldValue = goldVal ?? DefaultGoldValue;
            int? gemVal = GetIntRulesValue(ini, "General", "GemValue", false, errors);
            map.GemValue = gemVal ?? DefaultGemValue;
            int? radius = GetIntRulesValue(ini, "General", "DropZoneRadius", false, errors);
            map.DropZoneRadius = radius ?? DefaultDropZoneRadius;
            int? gapRadius = GetIntRulesValue(ini, "General", "GapRadius", false, errors);
            map.GapRadius = gapRadius ?? DefaultGapRadius;
            int? jamRadius = GetIntRulesValue(ini, "General", "RadarJamRadius", false, errors);
            map.RadarJamRadius = jamRadius ?? DefaultJamRadius;
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
                    refreshPoints.UnionWith(OccupierSet<Building>.GetOccupyPoints(p, b.OccupyMask));
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
                refreshPoints.UnionWith(OccupierSet<Building>.GetOccupyPoints(p, b.OccupyMask));
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
                IEnumerable<Point> buildingPoints = OccupierSet<Building>.GetOccupyPoints(p, b.OccupyMask);
                // Clear any walls that may now end up on the bib.
                if (Globals.BlockingBibs)
                {
                    foreach (Point bldPoint in buildingPoints)
                    {
                        Overlay ovl = map.Overlay[bldPoint];
                        if (ovl != null && ovl.Type.IsWall)
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
                    if (!Map.BasicSection.SoloMission || !Globals.NoMetaFilesForSinglePlay)
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
            INISection aftermathSection = null;
            List<INISection> addedExtra = new List<INISection>();
            if (extraSections != null)
            {
                foreach (INISection section in extraSections)
                {
                    if ("Aftermath".Equals(section.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        aftermathSection = section;
                    }
                    else
                    {
                        addedExtra.Add(section);
                    }
                }
            }
            BasicSection basic = (BasicSection)Map.BasicSection;
            // Make new Aftermath section
            INISection newAftermathSection = new INISection("Aftermath");
            newAftermathSection["NewUnitsEnabled"] = basic.ExpansionEnabled ? "1" : "0";
            if (aftermathSection != null)
            {
                // If old section is present, remove NewUnitsEnabled value from it, and copy the remainder into the new one.
                aftermathSection.Keys.Remove("NewUnitsEnabled");
                foreach ((string key, string value) in aftermathSection)
                {
                    newAftermathSection[key] = value;
                }
            }
            // Add Aftermath section
            ini.Sections.Add(newAftermathSection);
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
                for (Int32 i = 0; i < name.Length; i++)
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
                    teamType.House.ID.ToString(),
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
            foreach (House house in Map.Houses)
            {
                if (house.Type.ID < 0)
                {
                    continue;
                }
                House gameHouse = (House)house;
                bool enabled = house.Enabled;
                INITools.FillAndReAdd(ini, gameHouse.Type.Name, gameHouse, new MapContext(Map, false), enabled);
            }
            ini.Sections.Remove("Briefing");
            if (!string.IsNullOrEmpty(Map.BriefingSection.Briefing))
            {
                SaveIniBriefing(ini);
            }
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    for (int y = 0; y < Map.Metrics.Height; ++y)
                    {
                        for (int x = 0; x < Map.Metrics.Width; ++x)
                        {
                            Template template = Map.Templates[y, x];
                            if (template != null && (template.Type.Flag & TemplateTypeFlag.Clear) == 0)
                            {
                                writer.Write(template.Type.ID);
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
                ini.Sections.Remove("MapPack");
                CompressLCWSection(ini.Sections.Add("MapPack"), stream.ToArray());
            }
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    for (int i = 0; i < Map.Metrics.Length; ++i)
                    {
                        Overlay overlay = Map.Overlay[i];
                        if (overlay != null)
                        {
                            writer.Write(overlay.Type.ID);
                        }
                        else
                        {
                            writer.Write(byte.MaxValue);
                        }
                    }
                }
                ini.Sections.Remove("OverlayPack");
                CompressLCWSection(ini.Sections.Add("OverlayPack"), stream.ToArray());
            }
        }

        protected INISection SaveIniBriefing(INI ini)
        {
            if (string.IsNullOrEmpty(Map.BriefingSection.Briefing))
            {
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
                if (briefText.Length > maxBriefLengthClassic)
                {
                    briefText = briefText.Substring(0, maxBriefLengthClassic);
                }
                List<string> finalLines = new List<string>();
                string line = briefText;
                if (line.Length <= briefLineCutoffClassic)
                {
                    finalLines.Add(line);
                }
                else
                {
                    string[] splitLine = Regex.Split(line, "([ @])");
                    //.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int wordIndex = 0;
                    while (wordIndex < splitLine.Length)
                    {
                        StringBuilder sb = new StringBuilder();
                        // Always allow initial word
                        int nextLength = 0;
                        bool isBreak = false;
                        while (nextLength < briefLineCutoffClassic && wordIndex < splitLine.Length)
                        {
                            String cur = splitLine[wordIndex];
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
                        finalLines.Add(sb.ToString());
                    }
                }
                for (int i = 0; i < finalLines.Count; ++i)
                {
                    briefingSection[(i + 1).ToString()] = finalLines[i];
                }
            }
            return briefingSection;
        }

        private void SaveMapPreview(Stream stream, Boolean renderAll)
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
                foreach (Waypoint waypoint in Map.Waypoints.Where(w => (w.Flag & WaypointFlag.PlayerStart) == WaypointFlag.PlayerStart
                    && w.Cell.HasValue && Map.Metrics.GetLocation(w.Cell.Value, out Point p) && Map.Bounds.Contains(p)))
                {
                    writer.WriteValue(waypoint.Cell.Value);
                }
            }
            else
            {
                // Probably useless, but better than the player start points.
                foreach (Waypoint waypoint in Map.Waypoints.Where(w => (w.Flag & WaypointFlag.Home) == WaypointFlag.Home
                    && w.Cell.HasValue && Map.Metrics.GetLocation(w.Cell.Value, out Point p) && Map.Bounds.Contains(p)))
                {
                    writer.WriteValue(waypoint.Cell.Value);
                }
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public string Validate(Boolean forWarnings)
        {
            StringBuilder sb = new StringBuilder();
            if (forWarnings)
            {
                // Check if map has name
                if (this.MapNameIsEmpty(this.Map.BasicSection.Name))
                {
                    sb.Append("Map name is empty. If you continue, the filename will be filled in as map name.\n");
                }
                // Check if the map or any of the scripting references ants, and if so, if their rules are filled in.
                UnitType[] antUs = { UnitTypes.Ant1, UnitTypes.Ant2, UnitTypes.Ant3 };
                BuildingType[] antBRaw = { BuildingTypes.Queen, BuildingTypes.Larva1, BuildingTypes.Larva2 };
                // Buildings get cloned so equal function doesn't work on the bare blueprints.
                BuildingType[] antBs = this.Map.BuildingTypes.Where(bt => antBRaw.Any(loc => loc.ID == bt.ID)).ToArray();

                List<BuildingType> usedAntBldTypes = Map.Buildings.OfType<Building>().Where(x => antBs.Contains(x.Occupier.Type)).Select(lb => lb.Occupier.Type).Distinct().ToList();
                List<UnitType> usedAntUnitTypes = Map.Technos.OfType<Unit>().Where(u => antUs.Contains(u.Occupier.Type)).Select(lb => lb.Occupier.Type).Distinct().ToList();
                List<UnitType> usedAntsInTeams = Map.TeamTypes.SelectMany(t => t.Classes).Where(cl => antUs.Contains(cl.Type)).Select(cl => cl.Type).OfType<UnitType>().Distinct().ToList();
                usedAntUnitTypes.AddRange(usedAntsInTeams);
                // Nothing found.
                if (usedAntBldTypes.Count == 0 && usedAntUnitTypes.Count == 0)
                {
                    return sb.ToString();
                }
                bool hasQueen = usedAntBldTypes.Any(bld => bld.ID == BuildingTypes.Queen.ID);
                List<String> types = new List<string>();
                foreach (UnitType unit in usedAntUnitTypes)
                {
                    if (extraSections == null || !extraSections.Contains(unit.Name))
                        types.Add(unit.Name.ToUpperInvariant());
                }
                foreach (BuildingType bld in usedAntBldTypes)
                {
                    if (extraSections == null || !extraSections.Contains(bld.Name))
                        types.Add(bld.Name.ToUpperInvariant());
                }
                if (types.Count == 0)
                {
                    return sb.ToString();
                }
                sb.Append("The following ant units and structures were found on the map or in the scripting, but have no ini rules set to properly define their stats:\n\n");
                sb.Append(String.Join(", ", types.ToArray()));
                string stats = usedAntUnitTypes.Count == 0 ? (hasQueen ? "strength or weapon" : "strength") : "strength, weapon or movement speed";
                sb.Append("\n\nWithout ini definitions, these things will have no ").Append(stats)
                    .Append(", and will malfunction in the game. The definitions can be set in Settings → Map Settings → INI Rules & Tweaks.");
                return sb.ToString();
            }
            sb.Append("Error(s) during map validation:\n");
            bool ok = true;
            int numAircraft = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsAircraft).Count();
            int numBuildings = Map.Buildings.OfType<Building>().Where(x => x.Occupier.IsPrebuilt).Count();
            int numInfantry = Map.Technos.OfType<InfantryGroup>().Sum(item => item.Occupier.Infantry.Count(i => i != null));
            int numTerrain = Map.Technos.OfType<Terrain>().Count();
            int numUnits = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsGroundUnit).Count();
            int numVessels = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsVessel).Count();
            int numStartPoints = Map.Waypoints.Count(w => (w.Flag & WaypointFlag.PlayerStart) == WaypointFlag.PlayerStart && w.Cell.HasValue
                && Map.Metrics.GetLocation(w.Cell.Value, out Point pt) && Map.Bounds.Contains(pt));
            int numBadPoints = Map.Waypoints.Count(w => (w.Flag & WaypointFlag.PlayerStart) == WaypointFlag.PlayerStart && w.Cell.HasValue
                && Map.Metrics.GetLocation(w.Cell.Value, out Point pt) && !Map.Bounds.Contains(pt));
            if (!Globals.DisableAirUnits && numAircraft > Constants.MaxAircraft && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of aircraft exceeded ({0} > {1})", numAircraft, Constants.MaxAircraft));
                ok = false;
            }
            if (numBuildings > Constants.MaxBuildings && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of structures exceeded ({0} > {1})", numBuildings, Constants.MaxBuildings));
                ok = false;
            }
            if (numInfantry > Constants.MaxInfantry && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of infantry exceeded ({0} > {1})", numInfantry, Constants.MaxInfantry));
                ok = false;
            }
            if (numTerrain > Constants.MaxTerrain && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of terrain objects exceeded ({0} > {1})", numTerrain, Constants.MaxTerrain));
                ok = false;
            }
            if (numUnits > Constants.MaxUnits && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of units exceeded ({0} > {1})", numUnits, Constants.MaxUnits));
                ok = false;
            }
            if (numVessels > Constants.MaxVessels && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of ships exceeded ({0} > {1})", numVessels, Constants.MaxVessels));
                ok = false;
            }
            if (Map.TeamTypes.Count > Constants.MaxTeams && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of team types exceeded ({0} > {1})", Map.TeamTypes.Count, Constants.MaxTeams));
                ok = false;
            }
            if (Map.Triggers.Count > Constants.MaxTriggers && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of triggers exceeded ({0} > {1})", Map.Triggers.Count, Constants.MaxTriggers));
                ok = false;
            }
            if (!Map.BasicSection.SoloMission)
            {
                if (numStartPoints < 2)
                {
                    sb.Append("\nSkirmish/Multiplayer maps need at least 2 waypoints for player starting locations.");
                    ok = false;
                }
                if (numBadPoints > 0)
                {
                    sb.Append("\nSkirmish/Multiplayer maps should not have player start waypoints placed outside the map bound.");
                    ok = false;
                }
            }
            Waypoint homeWaypoint = Map.Waypoints.Where(w => (w.Flag & WaypointFlag.Home) == WaypointFlag.Home).FirstOrDefault();
            if (Map.BasicSection.SoloMission && (!homeWaypoint.Cell.HasValue || !Map.Metrics.GetLocation(homeWaypoint.Cell.Value, out Point p) || !Map.Bounds.Contains(p)))
            {
                sb.Append("\nSingle-player maps need the Home waypoint to be placed, inside the map bounds.");
                ok = false;
            }
            bool fatal;
            IEnumerable<string> triggerErr = CheckTriggers(this.Map.Triggers, true, true, true, out fatal, false, out bool _);
            if (fatal)
            {
                foreach (string err in triggerErr)
                {
                    sb.Append("\n").Append(err);
                }
                ok = false;
            }
            return ok ? null : sb.ToString().Trim('\n');
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
                int startPoints = Map.Waypoints.Count(w => w.Cell.HasValue && (w.Flag & WaypointFlag.PlayerStart) == WaypointFlag.PlayerStart);
                info.Add(string.Format("Number of set starting points: {0}.", startPoints));
            }
            const bool assessScripting = true;
            if (assessScripting)
            {
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
                        if (tm.Mission.ArgType == TeamMissionArgType.Waypoint)
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
            }
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
                    if (tr.Action1.ActionType == ActionTypes.TACTION_DZ || tr.Action1.ActionType == ActionTypes.TACTION_REVEAL_SOME || tr.Action1.ActionType == ActionTypes.TACTION_REVEAL_ZONE)
                        usedWaypoints.Add((int)tr.Action1.Data);
                    if (tr.Action2.ActionType == ActionTypes.TACTION_DZ || tr.Action2.ActionType == ActionTypes.TACTION_REVEAL_SOME || tr.Action2.ActionType == ActionTypes.TACTION_REVEAL_ZONE)
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
                Map.BasicSection.BasePlayer = HouseTypes.GetBasePlayer(Map.BasicSection.Player);
            }
            HouseType rebuildHouse = Map.HouseTypesIncludingNone.Where(h => h.Name == Map.BasicSection.BasePlayer).FirstOrDefault();
            housesWithProd.Add(rebuildHouse.Name);
            return housesWithProd;
        }

        public int[] GetRevealRadiusForWaypoints(Map map, bool forLargeReveal)
        {
            Waypoint[] waypoints = map.Waypoints;
            int length = waypoints.Length;
            int[] flareRadius = new int[length];
            for (int i = 0; i < length; i++)
            {
                string actionType = forLargeReveal ? ActionTypes.TACTION_REVEAL_SOME : ActionTypes.TACTION_DZ;
                foreach (Trigger trigger in map.Triggers)
                {
                    if ((actionType.Equals(trigger.Action1.ActionType, StringComparison.OrdinalIgnoreCase)
                        && trigger.Action1.Data == i)
                        || (actionType.Equals(trigger.Action2.ActionType, StringComparison.OrdinalIgnoreCase)
                        && trigger.Action2.Data == i))
                    {
                        flareRadius[i] = forLargeReveal ? map.GapRadius : map.DropZoneRadius;
                    }
                }
            }
            return flareRadius;
        }

        public IEnumerable<string> CheckTriggers(IEnumerable<Trigger> triggers, bool includeExternalData, bool prefixNames, bool fatalOnly, out bool fatal, bool fix, out bool wasFixed)
        {
            Dictionary<long, int> fixedGlobals = new Dictionary<long, int>();
            return CheckTriggers(triggers, includeExternalData, prefixNames, fatalOnly, out fatal, fix, out wasFixed, null, out _, ref fixedGlobals);
        }

        public IEnumerable<string> CheckTriggers(IEnumerable<Trigger> triggers, bool includeExternalData, bool prefixNames, bool fatalOnly, out bool fatal, bool fix, out bool wasFixed, HashSet<int> teamGlobals, out List<int> availableGlobals, ref Dictionary<long, int> fixedGlobals)
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
                string event1 = trigger.Event1.EventType;
                string event2 = trigger.Event2.EventType;
                string action1 = trigger.Action1.ActionType;
                string action2 = trigger.Action2.ActionType;
                // Not sure which ones are truly fatal.
                // Events
                CheckEventHouse(prefix, trigger.Event1, curErrors, 1, ref fatal, fatalOnly, fix, ref wasFixed);
                CheckEventHouse(prefix, trigger.Event2, curErrors, 2, ref fatal, fatalOnly, fix, ref wasFixed);
                // globals checks are only for ini read, really.
                CheckEventGlobals(prefix, trigger.Event1, curErrors, 1, ref fatal, fatalOnly, fix, ref wasFixed, availableGlobals, fixedGlobals);
                CheckEventGlobals(prefix, trigger.Event2, curErrors, 2, ref fatal, fatalOnly, fix, ref wasFixed, availableGlobals, fixedGlobals);
                CheckEventTeam(prefix, trigger.Event1, curErrors, 1, ref fatal, fatalOnly);
                CheckEventTeam(prefix, trigger.Event2, curErrors, 2, ref fatal, fatalOnly);
                // Actions
                CheckActionHouse(prefix, trigger.Action1, curErrors, 1, ref fatal, fatalOnly, fix, ref wasFixed);
                CheckActionHouse(prefix, trigger.Action2, curErrors, 2, ref fatal, fatalOnly, fix, ref wasFixed);
                CheckActionText(prefix, trigger.Action1, curErrors, 1, ref fatal, fatalOnly, fix, ref wasFixed);
                CheckActionText(prefix, trigger.Action2, curErrors, 2, ref fatal, fatalOnly, fix, ref wasFixed);
                // globals checks are only for ini read, really.
                CheckActionGlobals(prefix, trigger.Action1, curErrors, 1, ref fatal, fatalOnly, fix, ref wasFixed, availableGlobals, fixedGlobals);
                CheckActionGlobals(prefix, trigger.Action2, curErrors, 2, ref fatal, fatalOnly, fix, ref wasFixed, availableGlobals, fixedGlobals);
                CheckActionTeam(prefix, trigger.Action1, curErrors, 1, ref fatal, fatalOnly);
                CheckActionTeam(prefix, trigger.Action2, curErrors, 2, ref fatal, fatalOnly);
                CheckActionTrigger(prefix, trigger.Action1, curErrors, 1, ref fatal, fatalOnly);
                CheckActionTrigger(prefix, trigger.Action2, curErrors, 2, ref fatal, fatalOnly);
                // Waypoints: also only relevant on ini read.
                CheckActionWaypoint(prefix, trigger.Action1, curErrors, 1, ref fatal, fatalOnly, fix, ref wasFixed);
                CheckActionWaypoint(prefix, trigger.Action2, curErrors, 2, ref fatal, fatalOnly, fix, ref wasFixed);
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
                for (Int32 i = 0; i < team.Missions.Count; i++)
                {
                    TeamTypeMission ttm = team.Missions[i];
                    if (ttm.Mission.Mission == TeamMissionTypes.SetGlobal.Mission && (ttm.Argument < 0 || ttm.Argument > 29))
                    {
                        string error = String.Format("Team \"{0}\" Order {1} has an illegal global value \"{2}\": Globals only go from 0 to 29.",
                            team.Name, i + 1, ttm.Argument);
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
                        ttm.Argument = fixedVal == -1 ? ttm.Argument.Restrict(0, 29) : fixedVal;
                        wasFixed = true;
                        error += fixedVal == -1 ? (" Fixed to \"" + ttm.Argument + "\".") : (" Fixed to available global \"" + fixedVal + "\".");
                        errors.Add(error);
                    }
                }
            }
            return errors;
        }

        private void CheckEventHouse(string prefix, TriggerEvent evnt, List<string> errors, Int32 nr, ref bool fatal, bool fatalOnly, bool fix, ref bool wasFixed)
        {
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

        private void CheckEventGlobals(string prefix, TriggerEvent evnt, List<string> errors, Int32 nr, ref bool fatal, bool fatalOnly, bool fix, ref bool wasFixed, List<int> availableGlobals, Dictionary<long,int> fixedGlobals)
        {
            if (fatalOnly)
            {
                return;
            }
            switch (evnt.EventType)
            {
                case EventTypes.TEVENT_GLOBAL_SET:
                case EventTypes.TEVENT_GLOBAL_CLEAR:
                    if (evnt.Data < 0 || evnt.Data > 29)
                    {
                        string error = prefix + "Event " + nr + " has an illegal global value \"" + evnt.Data + "\": Globals only go from 0 to 29.";
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
                            evnt.Data = fixedVal == -1 ? evnt.Data.Restrict(0, 29) : fixedVal;
                            wasFixed = true;
                            error += fixedVal == -1 ? (" Fixed to \"" + evnt.Data + "\".") : (" Fixed to available global \"" + evnt.Data + "\".");
                        }
                        errors.Add(error);
                    }
                    break;
            }
        }

        private void CheckEventTeam(string prefix, TriggerEvent evnt, List<string> errors, Int32 nr, ref bool fatal, bool fatalOnly)
        {
            if (!TeamType.IsEmpty(evnt.Team) || fatalOnly)
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

        private void CheckActionHouse(string prefix, TriggerAction action, List<string> errors, Int32 nr, ref bool fatal, bool fatalOnly, bool fix, ref bool wasFixed)
        {
            if (fatalOnly)
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
                            errors.Add(prefix + "Action " + nr + ": \"" + actn.TrimEnd('.') + "\" on a House other than the player will always make you lose.");
                            break;
                        case ActionTypes.TACTION_LOSE:
                            errors.Add(prefix + "Action " + nr + ": \"" + actn.TrimEnd('.') + "\" on a House other than the player will always make you win.");
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
                            errors.Add(prefix + "Action " + nr + ": \"" + actn.TrimEnd('.') + "\" is set to the player's House.");
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

        private void CheckActionText(string prefix, TriggerAction action, List<string> errors, Int32 nr, ref bool fatal, bool fatalOnly, bool fix, ref bool wasFixed)
        {
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

        private void CheckActionGlobals(string prefix, TriggerAction action, List<string> errors, Int32 nr, ref bool fatal, bool fatalOnly, bool fix, ref bool wasFixed, List<int> availableGlobals, Dictionary<long, int> globalFixes)
        {
            if (fatalOnly)
            {
                return;
            }
            switch (action.ActionType)
            {
                case ActionTypes.TACTION_SET_GLOBAL:
                case ActionTypes.TACTION_CLEAR_GLOBAL:
                    if (action.Data < 0 || action.Data > 29)
                    {
                        string error = prefix + "Action " + nr + " has an illegal global value \""+ action.Data + "\": Globals only go from 0 to 29.";
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
                            action.Data = fixedVal == -1 ? action.Data.Restrict(0, 29) : fixedVal;
                            wasFixed = true;
                            error += fixedVal == -1 ? (" Fixed to \"" + action.Data + "\".") : (" Fixed to available global \"" + action.Data + "\".");
                        }
                        errors.Add(error);
                    }
                    break;
            }
        }

        private void CheckActionTeam(string prefix, TriggerAction action, List<string> errors, int nr, ref bool fatal, bool fatalOnly)
        {
            if (!TeamType.IsEmpty(action.Team) || fatalOnly)
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

        private void CheckActionTrigger(string prefix, TriggerAction action, List<string> errors, Int32 nr, ref bool fatal, bool fatalOnly)
        {
            if (!Trigger.IsEmpty(action.Trigger) || fatalOnly)
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

        private void CheckActionWaypoint(string prefix, TriggerAction act, List<string> errors, Int32 nr, ref bool fatal, bool fatalOnly, bool fix, ref bool wasFixed)
        {
            int maxId = Map.Waypoints.Length - 1;
            long wayPoint = act.Data;
            if ((wayPoint >= -1 && wayPoint <= maxId) || fatalOnly)
            {
                return;
            }
            switch (act.ActionType)
            {
                case ActionTypes.TACTION_DZ:
                case ActionTypes.TACTION_REVEAL_SOME:
                case ActionTypes.TACTION_REVEAL_ZONE:
                    if (wayPoint < -1 || wayPoint > maxId)
                    {
                        string error = prefix + "Action " + nr + ": \"" + act.ActionType.TrimEnd('.') + "\" has an illegal waypoint value \"" + act.Data + "\".";
                        if (fix)
                        {
                            act.Data = -1;
                            wasFixed = true;
                            error += " Fixing to \"-1\" (None).";
                        }
                        errors.Add(error);
                    }
                    break;
            }
        }

        public bool MapNameIsEmpty(string name)
        {
            return String.IsNullOrEmpty(name) || emptyMapName.Equals(name, StringComparison.OrdinalIgnoreCase);
        }

        public string EvaluateBriefing(string briefing)
        {
            bool briefLenOvfl = false;
            bool briefLenSplitOvfl = false;
            const int cutoff = 40;
            string briefText = (briefing ?? String.Empty).Replace('\t', ' ').Trim('\r', '\n', ' ').Replace("\r\n", "\n").Replace("\r", "\n");
            int lines = briefText.Count(c => c == '\n') + 1;
            briefLenOvfl = lines > 25;
            if (!briefLenOvfl)
            {
                // split in lines of 40; that's more or less the average line length in the brief screen.
                List<string> txtLines = new List<string>();
                string[] briefLines = briefText.Split('\n');
                for (int i = 0; i < briefLines.Length; ++i)
                {
                    string line = briefLines[i].Trim();
                    if (line.Length <= cutoff)
                    {
                        txtLines.Add(line);
                        continue;
                    }
                    string[] splitLine = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int wordIndex = 0;
                    while (wordIndex < splitLine.Length)
                    {
                        StringBuilder sb = new StringBuilder();
                        // Always allow initial word
                        int nextLength = 0;
                        while (nextLength < cutoff && wordIndex < splitLine.Length)
                        {
                            if (sb.Length > 0)
                                sb.Append(' ');
                            sb.Append(splitLine[wordIndex++]);
                            nextLength = wordIndex >= splitLine.Length ? 0 : (sb.Length + 1 + splitLine[wordIndex].Length);
                        }
                        txtLines.Add(sb.ToString());
                    }
                }
                briefLenSplitOvfl = txtLines.Count > 25;
            }
            const string warn25Lines = "Red Alert's briefing screen in the Remaster can only show 25 lines of briefing text. ";
            string message = null;
            if (briefLenOvfl)
            {
                message = warn25Lines + "Your current briefing exceeds that.";
            }
            else if (briefLenSplitOvfl)
            {
                message = warn25Lines + "The lines average to about 40 characters per line, and when split that way, your current briefing exceeds that, meaning it will most likely not display correctly in-game.";
            }
            if (Globals.WriteClassicBriefing && briefText.Length > maxBriefLengthClassic)
            {
                if (message == null)
                {
                    message = String.Empty;
                }
                else
                {
                    message += '\n';
                }
                message += "Classic Red Alert briefings cannot exceed " + maxBriefLengthClassic + " characters. This includes line breaks.\n\nThis will not affect the mission when playing in the Remaster, but the briefing will be truncated when playing in the original game.";
            }
            return message;
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


        private RaLandIniSection GetLandInfo(LandType landType)
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

        public bool IsLandUnitPassable(LandType landType)
        {
            RaLandIniSection landInfo = GetLandInfo(landType);
            return landInfo != null && (landInfo.Foot > 0 || landInfo.Wheel > 0 || landInfo.Track > 0);
        }

        public bool IsBoatPassable(LandType landType)
        {
            RaLandIniSection landInfo = GetLandInfo(landType);
            return landInfo != null && landInfo.Float > 0;
        }

        public bool IsBuildable(LandType landType)
        {
            RaLandIniSection landInfo = GetLandInfo(landType);
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
                    UpdateWaypoints();
                    break;
            }
        }

        private void MapSection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Theater":
                    Map.InitTheater(GameType);
                    break;
            }
        }

        private void UpdateBasePlayerHouse()
        {
            if (Map.BasicSection.BasePlayer == null)
            {
                Map.BasicSection.BasePlayer = HouseTypes.GetBasePlayer(Map.BasicSection.Player);
                // Updating BasePlayer will trigger PropertyChanged and re-call this function, so no need to continue here.
                return;
            }
            HouseType basePlayer = Map.HouseTypesIncludingNone.Where(h => h.Equals(Map.BasicSection.BasePlayer)).FirstOrDefault() ?? Map.HouseTypes.First();
            foreach (var (_, building) in Map.Buildings.OfType<Building>())
            {
                if (!building.IsPrebuilt)
                {
                    building.House = basePlayer;
                }
            }
        }

        protected void UpdateWaypoints()
        {
            bool isSolo = Map.BasicSection.SoloMission;
            HashSet<Point> updated = new HashSet<Point>();
            for (Int32 i = 0; i < Map.Waypoints.Length; ++i)
            {
                Waypoint waypoint = Map.Waypoints[i];
                if ((waypoint.Flag & WaypointFlag.PlayerStart) == WaypointFlag.PlayerStart)
                {
                    Map.Waypoints[i].Name = isSolo ? i.ToString() : string.Format("P{0}", i);
                    if (waypoint.Point.HasValue)
                    {
                        updated.Add(waypoint.Point.Value);
                    }
                }
            }
            Map.NotifyWaypointsUpdate();
            Map.NotifyMapContentsChanged(updated);
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

        private byte[] DecompressLCWSection(INISection section, int bytesPerCell, List<string> errors)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var (_, value) in section)
            {
                sb.Append(value);
            }
            byte[] compressedBytes;
            try
            {
                compressedBytes = Convert.FromBase64String(sb.ToString());
            }
            catch(FormatException)
            {
                errors.Add("Failed to unpack [" + section.Name + "] from Base64.");
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
                    return decompressedBytes;
                }
                if (writePtr + decompressed > decompressedBytes.Length)
                {
                    errors.Add("Failed to decompress [" + section.Name + "]: data exceeds map size.");
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
