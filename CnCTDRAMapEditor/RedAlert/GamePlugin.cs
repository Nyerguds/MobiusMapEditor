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
    class GamePlugin : IGamePlugin
    {
        private const int multiStartPoints = 8;
        private readonly IEnumerable<string> movieTypes;
        private bool isLoading = false;

        private static readonly Regex SinglePlayRegex = new Regex("^SC[A-LN-Z]\\d{2}[EWX][A-EL]$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private const string defVidVal = "<none>";
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

        public String Name => "Red Alert";

        public GameType GameType => GameType.RedAlert;

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
        public String ExtraIniText
        {
            get
            {
                INI ini = new INI();
                if (extraSections != null)
                {
                    ini.Sections.AddRange(extraSections);
                }
                return ini.ToString();
            }
            set
            {
                INI ini = new INI();
                try
                {
                    ini.Parse(value);
                }
                catch
                {
                    return;
                }
                // Strip "NewUnitsEnabled" from the Aftermath section.
                INISection amSection = ini.Sections["Aftermath"];
                if (amSection != null)
                {
                    amSection.Keys.Remove("NewUnitsEnabled");
                }
                // Remove any sections known and handled / disallowed by the editor.
                ini.Sections.Remove("Digest");
                ini.Sections.Remove("Basic");
                ini.Sections.Remove("Map");
                ini.Sections.Remove("Steam");
                ini.Sections.Remove("TeamTypes");
                ini.Sections.Remove("Trigs");
                ini.Sections.Remove("MapPack");
                ini.Sections.Remove("Terrain");
                ini.Sections.Remove("OverlayPack");
                ini.Sections.Remove("Smudge");
                ini.Sections.Remove("Units");
                ini.Sections.Remove("Aircraft");
                ini.Sections.Remove("Ships");
                ini.Sections.Remove("Infantry");
                ini.Sections.Remove("Structures");
                ini.Sections.Remove("Base");
                ini.Sections.Remove("Waypoints");
                ini.Sections.Remove("CellTriggers");
                ini.Sections.Remove("Briefing");
                foreach (var house in Map.Houses)
                {
                    ini.Sections.Remove(house.Type.Name);
                }
                extraSections = ini.Sections.Count == 0 ? null : ini.Sections;
                UpdateBuildingRules(ini, this.Map);
            }
        }
        public static bool CheckForRAMap(INI contents)
        {
            return GeneralUtils.CheckForIniInfo(contents, "MapPack", null, null);
        }

        static GamePlugin()
        {
            fullTechnoTypes = InfantryTypes.GetTypes().Cast<ITechnoType>().Concat(UnitTypes.GetTypes(false).Cast<ITechnoType>());
        }

        public GamePlugin()
            : this(true)
        {
        }

        public GamePlugin(bool mapImage)
        {
            var playerWaypoints = Enumerable.Range(0, multiStartPoints).Select(i => new Waypoint(string.Format("P{0}", i), Waypoint.GetFlagForMpId(i)));
            var generalWaypoints = Enumerable.Range(multiStartPoints, 98 - multiStartPoints).Select(i => new Waypoint(i.ToString()));
            var specialWaypoints = new Waypoint[] { new Waypoint("Home", WaypointFlag.Home), new Waypoint("Reinf.", WaypointFlag.Reinforce), new Waypoint("Special", WaypointFlag.Special) };
            var waypoints = playerWaypoints.Concat(generalWaypoints).Concat(specialWaypoints);
            var movies = new List<string>(movieTypesRa);
            for (int i = 0; i < movies.Count; ++i)
            {
                string vidName = GeneralUtils.AddRemarks(movies[i], defVidVal, true, movieTypesRemarksOld, RemarkOld);
                movies[i] = GeneralUtils.AddRemarks(vidName, defVidVal, true, movieTypesRemarksNew, RemarkNew);
            }
            movies.Insert(0, defVidVal);
            movieTypes = movies.ToArray();
            var basicSection = new BasicSection();
            basicSection.SetDefault();
            var houseTypes = HouseTypes.GetTypes();
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

            Map = new Map(basicSection, null, Constants.MaxSize, typeof(House), houseTypes, flagColors,
                TheaterTypes.GetTypes(), TemplateTypes.GetTypes(),
                TerrainTypes.GetTypes(), OverlayTypes.GetTypes(), SmudgeTypes.GetTypes(Globals.ConvertCraters),
                EventTypes.GetTypes(), cellEventTypes, unitEventTypes, structureEventTypes, terrainEventTypes,
                ActionTypes.GetTypes(), cellActionTypes, unitActionTypes, structureActionTypes, terrainActionTypes,
                MissionTypes.GetTypes(), DirectionTypes.GetTypes(), InfantryTypes.GetTypes(), UnitTypes.GetTypes(Globals.DisableAirUnits),
                BuildingTypes.GetTypes(), TeamMissionTypes.GetTypes(), fullTechnoTypes, waypoints, movieTypes, themeTypes)
            {
                TiberiumOrGoldValue = 35,
                GemValue = 110
            };
            Map.BasicSection.PropertyChanged += BasicSection_PropertyChanged;
            Map.MapSection.PropertyChanged += MapSection_PropertyChanged;
            if (mapImage)
            {
                MapImage = new Bitmap(Map.Metrics.Width * Globals.MapTileWidth, Map.Metrics.Height * Globals.MapTileHeight);
            }
        }

        public void New(string theater)
        {
            try
            {
                isLoading = true;
                Map.Theater = Map.TheaterTypes.Where(t => t.Equals(theater)).FirstOrDefault() ?? TheaterTypes.Temperate;
                Map.TopLeft = new Point(1, 1);
                Map.Size = Map.Metrics.Size - new Size(2, 2);
                Map.BasicSection.Player = Map.HouseTypes.FirstOrDefault()?.Name;
                UpdateBasePlayerHouse();
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
                var errors = new List<string>();
                
                bool forceSingle = false;
                switch (fileType)
                {
                    case FileType.INI:
                    case FileType.BIN:
                        {
                            var ini = new INI();
                            using (var reader = new StreamReader(path))
                            {
                                ini.Parse(reader);
                            }
                            forceSingle = SinglePlayRegex.IsMatch(Path.GetFileNameWithoutExtension(path));
                            errors.AddRange(LoadINI(ini, forceSingle, ref modified));
                        }
                        break;
                    case FileType.MEG:
                    case FileType.PGM:
                        {
                            using (var megafile = new Megafile(path))
                            {
                                var mprFile = megafile.Where(p => Path.GetExtension(p).ToLower() == ".mpr").FirstOrDefault();
                                if (mprFile == null)
                                {
                                    throw new ApplicationException("Cannot find the necessary file inside the " + Path.GetFileName(path) + " archive.");
                                }
                                var ini = new INI();
                                using (var reader = new StreamReader(megafile.Open(mprFile)))
                                {
                                    ini.Parse(reader);
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

        private IEnumerable<string> LoadINI(INI ini, bool forceSoloMission, ref bool modified)
        {
            var errors = new List<string>();
            Map.BeginUpdate();
            // Fetch some rules.ini information
            UpdateBuildingRules(ini, this.Map);
            // Just gonna remove this; I assume it'll be invalid after a re-save anyway.
            ini.Sections.Extract("Digest");
            // Basic info
            var basicSection = ini.Sections.Extract("Basic");
            if (basicSection != null)
            {
                INI.ParseSection(new MapContext(Map, true), basicSection, Map.BasicSection);
                Model.BasicSection basic = Map.BasicSection;
                basic.Intro = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Intro, defVidVal, true, movieTypesRemarksOld, RemarkOld), defVidVal, true, movieTypesRemarksNew, RemarkNew);
                basic.Intro = GeneralUtils.FilterToExisting(basic.Intro, defVidVal, true, movieTypesRa);
                basic.Brief = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Brief, defVidVal, true, movieTypesRemarksOld, RemarkOld), defVidVal, true, movieTypesRemarksNew, RemarkNew);
                basic.Brief = GeneralUtils.FilterToExisting(basic.Brief, defVidVal, true, movieTypesRa);
                basic.Action = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Action, defVidVal, true, movieTypesRemarksOld, RemarkOld), defVidVal, true, movieTypesRemarksNew, RemarkNew);
                basic.Action = GeneralUtils.FilterToExisting(basic.Action, defVidVal, true, movieTypesRa);
                basic.Win = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Win, defVidVal, true, movieTypesRemarksOld, RemarkOld), defVidVal, true, movieTypesRemarksNew, RemarkNew);
                basic.Win = GeneralUtils.FilterToExisting(basic.Win, defVidVal, true, movieTypesRa);
                basic.Win2 = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Win2, defVidVal, true, movieTypesRemarksOld, RemarkOld), defVidVal, true, movieTypesRemarksNew, RemarkNew);
                basic.Win2 = GeneralUtils.FilterToExisting(basic.Win2, defVidVal, true, movieTypesRa);
                basic.Win3 = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Win3, defVidVal, true, movieTypesRemarksOld, RemarkOld), defVidVal, true, movieTypesRemarksNew, RemarkNew);
                basic.Win3 = GeneralUtils.FilterToExisting(basic.Win3, defVidVal, true, movieTypesRa);
                basic.Win4 = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Win4, defVidVal, true, movieTypesRemarksOld, RemarkOld), defVidVal, true, movieTypesRemarksNew, RemarkNew);
                basic.Win4 = GeneralUtils.FilterToExisting(basic.Win4, defVidVal, true, movieTypesRa);
                basic.Lose = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Lose, defVidVal, true, movieTypesRemarksOld, RemarkOld), defVidVal, true, movieTypesRemarksNew, RemarkNew);
                basic.Lose = GeneralUtils.FilterToExisting(basic.Lose, defVidVal, true, movieTypesRa);
            }
            Map.BasicSection.Player = Map.HouseTypes.Where(t => t.Equals(Map.BasicSection.Player)).FirstOrDefault()?.Name ?? Map.HouseTypes.First().Name;
            Map.BasicSection.BasePlayer = HouseTypes.GetBasePlayer(Map.BasicSection.Player);
            bool aftermathEnabled = false;
            // Don't remove from extra sections.
            INISection aftermathSection = ini.Sections["Aftermath"];
            if (aftermathSection != null)
            {
                var amEnabled = aftermathSection["NewUnitsEnabled"];
                aftermathEnabled = amEnabled != null && Int32.TryParse(amEnabled, out int val) && val == 1;
                aftermathSection.Keys.Remove("NewUnitsEnabled");
                // Remove if empty.
                if (aftermathSection.Empty)
                    ini.Sections.Remove(aftermathSection.Name);
            }
            // Needs to be enabled in advance; it determines which units are valid to have placed on the map.
            Map.BasicSection.ExpansionEnabled = aftermathEnabled;
            // Map info
            var mapSection = ini.Sections.Extract("Map");
            if (mapSection != null)
            {
                INI.ParseSection(new MapContext(Map, true), mapSection, Map.MapSection);
            }
            Map.MapSection.FixBounds();
            // Steam info
            var steamSection = ini.Sections.Extract("Steam");
            if (steamSection != null)
            {
                INI.ParseSection(new MapContext(Map, true), steamSection, Map.SteamSection);
            }
            T indexToType<T>(IList<T> list, string index)
            {
                return (int.TryParse(index, out int result) && (result >= 0) && (result < list.Count)) ? list[result] : list.First();
            }
            var teamTypesSection = ini.Sections.Extract("TeamTypes");
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
                        var teamType = new TeamType { Name = Key };
                        var tokens = Value.Split(',').ToList();
                        teamType.House = Map.HouseTypes.Where(t => t.Equals(sbyte.Parse(tokens[0]))).FirstOrDefault(); tokens.RemoveAt(0);
                        var flags = int.Parse(tokens[0]); tokens.RemoveAt(0);
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
                        var numClasses = int.Parse(tokens[0]); tokens.RemoveAt(0);
                        for (int i = 0; i < Math.Min(Globals.MaxTeamClasses, numClasses); ++i)
                        {
                            var classTokens = tokens[0].Split(':'); tokens.RemoveAt(0);
                            if (classTokens.Length == 2)
                            {
                                ITechnoType type = fullTechnoTypes.Where(t => t.Equals(classTokens[0])).FirstOrDefault();
                                var count = byte.Parse(classTokens[1]);
                                if (type != null)
                                {
                                    if (!aftermathEnabled && ((type is UnitType un && un.IsExpansionUnit) || (type is InfantryType it && it.IsExpansionUnit)))
                                    {
                                        errors.Add(string.Format("Team '{0}' contains expansion unit '{0}', but expansion units not enabled; enabling expansion units.", Key, type.Name));
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
                        var numMissions = int.Parse(tokens[0]); tokens.RemoveAt(0);
                        for (int i = 0; i < Math.Min(Globals.MaxTeamMissions, numMissions); ++i)
                        {
                            var missionTokens = tokens[0].Split(':'); tokens.RemoveAt(0);
                            if (missionTokens.Length == 2)
                            {
                                teamType.Missions.Add(new TeamTypeMission { Mission = indexToType(Map.TeamMissionTypes, missionTokens[0]), Argument = int.Parse(missionTokens[1]) });
                            }
                            else
                            {
                                errors.Add(string.Format("Team '{0}' has wrong number of tokens for mission index {1} (expecting 2).", Key, i));
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
            var triggersSection = ini.Sections.Extract("Trigs");
            List<Trigger> triggers = new List<Trigger>();
            if (triggersSection != null)
            {
                foreach (var (Key, Value) in triggersSection)
                {
                    try
                    {
                        var tokens = Value.Split(',');
                        if (tokens.Length == 18)
                        {
                            if (Key.Length > 4)
                            {
                                errors.Add(string.Format("Trigger '{0}' has a name that is longer than 4 characters. This will not be corrected by the loading process, but should be addressed, since it can make the triggers fail to read correctly and link to objects and cell triggers, and might even crash the game.", Key));
                            }
                            var trigger = new Trigger { Name = Key };
                            trigger.PersistentType = (TriggerPersistentType)int.Parse(tokens[0]);
                            trigger.House = Map.HouseTypes.Where(t => t.Equals(sbyte.Parse(tokens[1]))).FirstOrDefault()?.Name ?? "None";
                            trigger.EventControl = (TriggerMultiStyleType)int.Parse(tokens[2]);
                            trigger.Event1.EventType = indexToType(Map.EventTypes, tokens[4]);
                            trigger.Event1.Team = tokens[5];
                            trigger.Event1.Data = long.Parse(tokens[6]);
                            trigger.Event2.EventType = indexToType(Map.EventTypes, tokens[7]);
                            trigger.Event2.Team = tokens[8];
                            trigger.Event2.Data = long.Parse(tokens[9]);
                            trigger.Action1.ActionType = indexToType(Map.ActionTypes, tokens[10]);
                            trigger.Action1.Team = tokens[11];
                            trigger.Action1.Trigger = tokens[12];
                            trigger.Action1.Data = long.Parse(tokens[13]);
                            trigger.Action2.ActionType = indexToType(Map.ActionTypes, tokens[14]);
                            trigger.Action2.Team = tokens[15];
                            trigger.Action2.Trigger = tokens[16];
                            trigger.Action2.Data = long.Parse(tokens[17]);
                            // Fix up data caused by union usage in the legacy game
                            Action<TriggerEvent> fixEvent = (TriggerEvent e) =>
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
                                        e.Data &= 0xFF;
                                        break;
                                    default:
                                        break;
                                }
                            };
                            Action<TriggerAction> fixAction = (TriggerAction a) =>
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
                                        a.Data &= 0xFF;
                                        break;
                                    case ActionTypes.TACTION_TEXT_TRIGGER:
                                        a.Data = Math.Max(1, Math.Min(209, a.Data));
                                        break;
                                    default:
                                        break;
                                }
                            };
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
                        errors.Add(string.Format("Trigger '{0}' has errors and can't be parsed: {1}.", Key, ex.Message));
                        modified = true;
                    }
                }
            }
            HashSet<string> checkTrigs = Trigger.None.Yield().Concat(triggers.Select(t => t.Name)).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            HashSet<string> checkCellTrigs = Map.FilterCellTriggers(triggers).Select(t => t.Name).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            HashSet<string> checkUnitTrigs = Trigger.None.Yield().Concat(Map.FilterUnitTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            HashSet<string> checkStrcTrigs = Trigger.None.Yield().Concat(Map.FilterStructureTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            // Terrain objects in RA have no triggers
            //HashSet<string> checkTerrTrigs = Trigger.None.Yield().Concat(Map.FilterTerrainTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            //MessageBox.Show("MapPack");
            var mapPackSection = ini.Sections.Extract("MapPack");
            if (mapPackSection != null)
            {
                Map.Templates.Clear();
                var data = DecompressLCWSection(mapPackSection, 3, errors);
                if (data != null)
                {
                    int width = Map.Metrics.Width;
                    int height = Map.Metrics.Height;
                    using (var reader = new BinaryReader(new MemoryStream(data)))
                    {
                        bool clearedOldClear = false;
                        // Amount of tile 255 detected outside map bounds.
                        int oldClearCount = 0;
                        for (var y = 0; y < height; ++y)
                        {
                            for (var x = 0; x < width; ++x)
                            {
                                var typeValue = reader.ReadUInt16();
                                var templateType = Map.TemplateTypes.Where(t => t.Equals(typeValue)).FirstOrDefault();
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
                                                    modified = true;
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
                                modified = true;
                                for (var y = 0; y < height; ++y)
                                {
                                    for (var x = 0; x < width; ++x)
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
                        for (var y = 0; y < height; ++y)
                        {
                            for (var x = 0; x < width; ++x)
                            {
                                var iconValue = reader.ReadByte();
                                var template = Map.Templates[y, x];
                                // Prevent loading of illegal tiles.
                                if (template != null)
                                {
                                    var templateType = template.Type;
                                    bool isRandom = (templateType.Flag & TemplateTypeFlag.RandomCell) != TemplateTypeFlag.None;
                                    bool tileOk = false;
                                    if (iconValue >= templateType.NumIcons)
                                    {
                                        errors.Add(String.Format("Template '{0}' at cell [{1},{2}] has an icon set ({3}) that is outside its icons range; clearing.", templateType.Name.ToUpper(), x, y, iconValue));
                                        modified = true;
                                    }
                                    else if (!isRandom && templateType.IconMask != null && !templateType.IconMask[iconValue / templateType.IconWidth, iconValue % templateType.IconWidth])
                                    {
                                        errors.Add(String.Format("Template '{0}' at cell [{1},{2}] has an icon set ({3}) that is not part of its placeable cells; clearing.", templateType.Name.ToUpper(), x, y, iconValue));
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
            var terrainSection = ini.Sections.Extract("Terrain");
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
                    var name = Value.Split(',')[0];
                    var terrainType = Map.TerrainTypes.Where(t => t.Equals(name)).FirstOrDefault();
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
                            //    errors.Add(string.Format("Terrain '{0}' links to unknown trigger '{1}'; clearing trigger..", terrainType, newTerr.Trigger));
                            //    modified = true;
                            //    newTerr.Trigger = Trigger.None;
                            //}
                            //else if (!checkTerrTrigs.Contains(Value))
                            //{
                            //    errors.Add(string.Format("Terrain '{0}' links to trigger '{1}' which does not contain an event or action applicable to terrain; clearing trigger.", terrainType, newTerr.Trigger));
                            //    modified = true;
                            //    newTerr.Trigger = Trigger.None;
                            //}
                        }
                        else
                        {
                            var techno = Map.FindBlockingObject(cell, terrainType, out int blockingCell);
                            string reportCell = blockingCell == -1 ? "<unknown>" : cell.ToString();
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
                                    errors.Add(string.Format("Terrain '{0}' placed on cell {1} overlaps unknown techno; skipping.", terrainType.Name, cell));
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
            var overlayPackSection = ini.Sections.Extract("OverlayPack");
            if (overlayPackSection != null)
            {
                Map.Overlay.Clear();
                var data = DecompressLCWSection(overlayPackSection, 1, errors);
                if (data != null)
                {
                    for (var i = 0; i < Map.Metrics.Length; ++i)
                    {
                        var overlayId = data[i];
                        // Technically signed, so filter out negative values.
                        if ((overlayId & 0x80) == 0)
                        {
                            var overlayType = Map.OverlayTypes.Where(t => t.Equals(overlayId)).FirstOrDefault();
                            if (overlayType != null)
                            {
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
                                errors.Add(string.Format("Overlay ID {0} references unknown overlay.", overlayId));
                                modified = true;
                            }
                        }
                    }
                }
            }
            var smudgeSection = ini.Sections.Extract("Smudge");
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
                    var tokens = Value.Split(',');
                    if (tokens.Length == 3)
                    {
                        // Craters other than cr1 don't work right in the game. Replace them by stage-0 cr1.
                        bool badCrater = Globals.ConvertCraters && SmudgeTypes.GetBadCraterRegex().IsMatch(tokens[0]);
                        var smudgeType = badCrater ? SmudgeTypes.Crater1 : Map.SmudgeTypes.Where(t => t.Equals(tokens[0]) && !t.IsAutoBib).FirstOrDefault();
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
                                        Map.Smudge[placeLocation] = new Smudge
                                        {
                                            Type = smudgeType,
                                            Icon = multiCell ? placeIcon++ : icon
                                        };
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
            var unitsSection = ini.Sections.Extract("Units");
            if (unitsSection != null)
            {
                foreach (var (Key, Value) in unitsSection)
                {
                    var tokens = Value.Split(',');
                    if (tokens.Length == 7)
                    {
                        var unitType = Map.AllUnitTypes.Where(t => t.IsUnit && t.Equals(tokens[1])).FirstOrDefault();
                        if (unitType == null)
                        {
                            errors.Add(string.Format("Unit '{0}' references unknown unit.", tokens[1]));
                            modified = true;
                            continue;
                        }
                        if (!aftermathEnabled && unitType.IsExpansionUnit)
                        {
                            errors.Add(string.Format("Expansion unit '{0}' encountered, but expansion units not enabled; enabling expansion units.", unitType.Name));
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
                        var direction = (byte)((dirValue + 0x08) & ~0x0F);
                        Unit newUnit = new Unit()
                        {
                            Type = unitType,
                            House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                            Strength = strength,
                            Direction = Map.DirectionTypes.Where(d => d.Equals(direction)).FirstOrDefault(),
                            Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5])).FirstOrDefault() ?? Map.GetDefaultMission(unitType),
                            Trigger = tokens[6]
                        };
                        if (Map.Technos.Add(cell, newUnit))
                        {
                            if (!checkTrigs.Contains(tokens[6]))
                            {
                                errors.Add(string.Format("Unit '{0}' links to unknown trigger '{1}'; clearing trigger.", unitType.Name, tokens[6]));
                                modified = true;
                                newUnit.Trigger = Trigger.None;
                            }
                            else if (!checkUnitTrigs.Contains(tokens[6]))
                            {
                                errors.Add(string.Format("Unit '{0}' links to trigger '{1}' which does not contain an event or action applicable to units; clearing trigger.", unitType.Name, tokens[6]));
                                modified = true;
                                newUnit.Trigger = Trigger.None;
                            }
                        }
                        else
                        {
                            var techno = Map.Technos[cell];
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
            var aircraftSection = ini.Sections.Extract("Aircraft");
            if (!Globals.DisableAirUnits && aircraftSection != null)
            {
                foreach (var (Key, Value) in aircraftSection)
                {
                    var tokens = Value.Split(',');
                    if (tokens.Length == 6)
                    {
                        var aircraftType = Map.UnitTypes.Where(t => t.IsAircraft && t.Equals(tokens[1])).FirstOrDefault();
                        if (aircraftType == null)
                        {
                            errors.Add(string.Format("Aircraft '{0}' references unknown aircraft.", tokens[1]));
                            modified = true;
                            continue;
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
                        var direction = (byte)((dirValue + 0x08) & ~0x0F);
                        Unit newUnit = new Unit()
                        {
                            Type = aircraftType,
                            House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                            Strength = strength,
                            Direction = Map.DirectionTypes.Where(d => d.Equals(direction)).FirstOrDefault(),
                            Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5])).FirstOrDefault() ?? Map.GetDefaultMission(aircraftType)
                        };
                        if (!Map.Technos.Add(cell, newUnit))
                        {
                            var techno = Map.Technos[cell];
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
            var shipsSection = ini.Sections.Extract("Ships");
            if (shipsSection != null)
            {
                foreach (var (Key, Value) in shipsSection)
                {
                    var tokens = Value.Split(',');
                    if (tokens.Length == 7)
                    {
                        var vesselType = Map.UnitTypes.Where(t => t.IsVessel && t.Equals(tokens[1])).FirstOrDefault();
                        if (vesselType == null)
                        {
                            errors.Add(string.Format("Ship '{0}' references unknown ship.", tokens[1]));
                            modified = true;
                            continue;
                        }
                        int strength;
                        if (!int.TryParse(tokens[2], out strength))
                        {
                            errors.Add(string.Format("Strength for aircraft '{0}' cannot be parsed; value: '{1}'; skipping.", vesselType.Name, tokens[2]));
                            modified = true;
                            continue;
                        }
                        int cell;
                        if (!int.TryParse(tokens[3], out cell))
                        {
                            errors.Add(string.Format("Cell for aircraft '{0}' cannot be parsed; value: '{1}'; skipping.", vesselType.Name, tokens[3]));
                            modified = true;
                            continue;
                        }
                        int dirValue;
                        if (!int.TryParse(tokens[4], out dirValue))
                        {
                            errors.Add(string.Format("Direction for aircraft '{0}' cannot be parsed; value: '{1}'; skipping.", vesselType.Name, tokens[4]));
                            modified = true;
                            continue;
                        }
                        var direction = (byte)((dirValue + 0x08) & ~0x0F);
                        Unit newShip = new Unit()
                        {
                            Type = vesselType,
                            House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                            Strength = strength,
                            Direction = Map.DirectionTypes.Where(d => d.Equals(direction)).FirstOrDefault(),
                            Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5])).FirstOrDefault() ?? Map.GetDefaultMission(vesselType),
                            Trigger = tokens[6]
                        };
                        if (Map.Technos.Add(cell, newShip))
                        {
                            if (!checkTrigs.Contains(tokens[6]))
                            {
                                errors.Add(string.Format("Ship '{0}' links to unknown trigger '{1}'; clearing trigger.", vesselType.Name, tokens[6]));
                                modified = true;
                                newShip.Trigger = Trigger.None;
                            }
                            else if (!checkUnitTrigs.Contains(tokens[6]))
                            {
                                errors.Add(string.Format("Ship '{0}' links to trigger '{1}' which does not contain an event or action applicable to ships; clearing trigger.", vesselType.Name, tokens[6]));
                                modified = true;
                                newShip.Trigger = Trigger.None;
                            }
                        }
                        else
                        {
                            var techno = Map.Technos[cell];
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
            var infantrySection = ini.Sections.Extract("Infantry");
            if (infantrySection != null)
            {
                foreach (var (Key, Value) in infantrySection)
                {
                    var tokens = Value.Split(',');
                    if (tokens.Length == 8)
                    {
                        var infantryType = Map.InfantryTypes.Where(t => t.Equals(tokens[1])).FirstOrDefault();
                        if (infantryType == null)
                        {
                            errors.Add(string.Format("Infantry '{0}' references unknown infantry.", tokens[1]));
                            modified = true;
                            continue;
                        }
                        if (!aftermathEnabled && infantryType.IsExpansionUnit)
                        {
                            errors.Add(string.Format("Expansion infantry '{0}' encountered, but expansion units not enabled; enabling expansion units.", infantryType.Name));
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
                        var infantryGroup = Map.Technos[cell] as InfantryGroup;
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
                                errors.Add(string.Format("Sub-position for infantry '{0}' cannot be parsed; value: '{1}'; skipping.", infantryType.Name, tokens[4]));
                                modified = true;
                                continue;
                            }
                            if (stoppingPos < Globals.NumInfantryStops)
                            {
                                int dirValue;
                                if (!int.TryParse(tokens[6], out dirValue))
                                {
                                    errors.Add(string.Format("Direction for infantry '{0}' cannot be parsed; value: '{1}'; skipping.", infantryType.Name, tokens[6]));
                                    modified = true;
                                    continue;
                                }
                                var direction = (byte)((dirValue + 0x08) & ~0x0F);
                                if (infantryGroup.Infantry[stoppingPos] == null)
                                {
                                    if (!checkTrigs.Contains(tokens[7]))
                                    {
                                        errors.Add(string.Format("Infantry '{0}' links to unknown trigger '{1}'; clearing trigger.", infantryType.Name, tokens[7]));
                                        modified = true;
                                        tokens[7] = Trigger.None;
                                    }
                                    else if (!checkUnitTrigs.Contains(tokens[7]))
                                    {
                                        errors.Add(string.Format("Infantry '{0}' links to trigger '{1}' which does not contain an event or action applicable to infantry; clearing trigger.", infantryType.Name, tokens[7]));
                                        modified = true;
                                        tokens[7] = Trigger.None;
                                    }
                                    infantryGroup.Infantry[stoppingPos] = new Infantry(infantryGroup)
                                    {
                                        Type = infantryType,
                                        House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                                        Strength = strength,
                                        Direction = Map.DirectionTypes.Where(d => d.Equals(direction)).FirstOrDefault(),
                                        Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5])).FirstOrDefault() ?? Map.GetDefaultMission(infantryType),
                                        Trigger = tokens[7]
                                    };
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
                            var techno = Map.Technos[cell];
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
            var structuresSection = ini.Sections.Extract("Structures");
            if (structuresSection != null)
            {
                foreach (var (Key, Value) in structuresSection)
                {
                    var tokens = Value.Split(',');
                    if (tokens.Length > 5)
                    {
                        var buildingType = Map.BuildingTypes.Where(t => t.Equals(tokens[1])).FirstOrDefault();
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
                        var direction = (byte)((dirValue + 0x08) & ~0x0F);
                        bool sellable = (tokens.Length > 6) && int.TryParse(tokens[6], out int sell) && sell != 0;
                        bool rebuild = (tokens.Length > 7) && int.TryParse(tokens[7], out int rebld) && rebld != 0;
                        Building newBld = new Building()
                        {
                            Type = buildingType,
                            House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                            Strength = strength,
                            Direction = Map.DirectionTypes.Where(d => d.Equals(direction)).FirstOrDefault(),
                            Trigger = tokens[5],
                            Sellable = sellable,
                            Rebuild = rebuild
                        };
                        if (Map.Buildings.Add(cell, newBld))
                        {
                            if (!checkTrigs.Contains(tokens[5]))
                            {
                                errors.Add(string.Format("Structure '{0}' links to unknown trigger '{1}'; clearing trigger.", buildingType.Name, tokens[5]));
                                modified = true;
                                newBld.Trigger = Trigger.None;
                            }
                            else if (!checkStrcTrigs.Contains(tokens[5]))
                            {
                                errors.Add(string.Format("Structure '{0}' links to trigger '{1}' which does not contain an event or action applicable to structures; clearing trigger.", buildingType.Name, tokens[5]));
                                modified = true;
                                newBld.Trigger = Trigger.None;
                            }
                        }
                        else
                        {
                            var techno = Map.FindBlockingObject(cell, buildingType, out int blockingCell);
                            string reportCell = blockingCell == -1 ? "<unknown>" : cell.ToString();
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
                                    errors.Add(string.Format("Structure '{0}' placed on cell {1} overlaps unknown techno; skipping.", buildingType.Name, cell));
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
            var baseSection = ini.Sections.Extract("Base");
            if (baseSection != null)
            {
                foreach (var (Key, Value) in baseSection)
                {
                    if (Key.Equals("Player", StringComparison.OrdinalIgnoreCase))
                    {
                        Map.BasicSection.BasePlayer = Map.HouseTypes.Where(t => t.Equals(Value)).FirstOrDefault()?.Name ?? Map.HouseTypes.First().Name;
                    }
                    else if (int.TryParse(Key, out int priority))
                    {
                        var tokens = Value.Split(',');
                        if (tokens.Length == 2)
                        {
                            var buildingType = Map.BuildingTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault();
                            if (buildingType == null)
                            {
                                errors.Add(string.Format("Base rebuild entry {0} references unknown structure '{1}'.", priority, tokens[0]));
                                modified = true;
                                continue;
                            }
                            if (Globals.FilterTheaterObjects && buildingType.Theaters != null && !buildingType.Theaters.Contains(Map.Theater))
                            {
                                errors.Add(string.Format("Base rebuild entry {0} references structure '{1}' which is not available in the set theater; skipping.", priority, buildingType.Name));
                                modified = true;
                                continue;
                            }
                            int cell;
                            if (!int.TryParse(tokens[1], out cell))
                            {
                                errors.Add(string.Format("Cell for base rebuild entry '{0}' cannot be parsed; value: '{1}'; skipping.", buildingType.Name, tokens[1]));
                                modified = true;
                                continue;
                            }
                            Map.Metrics.GetLocation(cell, out Point location);
                            if (Map.Buildings.OfType<Building>().Where(x => x.Location == location).FirstOrDefault().Occupier is Building building)
                            {
                                building.BasePriority = priority;
                            }
                            else
                            {
                                Map.Buildings.Add(cell, new Building()
                                {
                                    Type = buildingType,
                                    Strength = 256,
                                    Direction = DirectionTypes.North,
                                    BasePriority = priority,
                                    IsPrebuilt = false
                                });
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Base rebuild entry {0} has wrong number of tokens (expecting 2).", priority));
                            modified = true;
                        }
                    }
                    else if (!Key.Equals("Count", StringComparison.CurrentCultureIgnoreCase))
                    {
                        errors.Add(string.Format("Invalid base rebuild priority '{0}' (expecting integer).", Key));
                        modified = true;
                    }
                }
            }
            var waypointsSection = ini.Sections.Extract("Waypoints");
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
            var cellTriggersSection = ini.Sections.Extract("CellTriggers");
            if (cellTriggersSection != null)
            {
                foreach (var (Key, Value) in cellTriggersSection)
                {
                    if (int.TryParse(Key, out int cell))
                    {
                        if (Map.Metrics.Contains(cell))
                        {
                            if (!checkTrigs.Contains(Value))
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
                                Map.CellTriggers[cell] = new CellTrigger(Value);
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
            var briefingSection = ini.Sections.Extract("Briefing");
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
            foreach (var house in Map.Houses)
            {
                if (house.Type.ID < 0)
                {
                    continue;
                }
                var houseSection = ini.Sections.Extract(house.Type.Name);
                if (houseSection != null)
                {
                    INI.ParseSection(new MapContext(Map, true), houseSection, house);
                    house.Enabled = true;
                }
                else
                {
                    house.Enabled = false;
                }
            }
            string indexToName<T>(IList<T> list, string index, string defaultValue) where T : INamedType
            {
                return (int.TryParse(index, out int result) && (result >= 0) && (result < list.Count)) ? list[result].Name : defaultValue;
            }
            foreach (var teamType in teamTypes)
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
            foreach (var trigger in triggers)
            {
                trigger.Event1.Team = indexToName(teamTypes, trigger.Event1.Team, TeamType.None);
                trigger.Event2.Team = indexToName(teamTypes, trigger.Event2.Team, TeamType.None);
                trigger.Action1.Team = indexToName(teamTypes, trigger.Action1.Team, TeamType.None);
                trigger.Action1.Trigger = indexToName(triggers, trigger.Action1.Trigger, Trigger.None);
                trigger.Action2.Team = indexToName(teamTypes, trigger.Action2.Team, TeamType.None);
                trigger.Action2.Trigger = indexToName(triggers, trigger.Action2.Trigger, Trigger.None);
            }
            // Sort
            var comparer = new ExplorerComparer();
            Map.TeamTypes.Clear();
            Map.TeamTypes.AddRange(teamTypes.OrderBy(t => t.Name, comparer));
            UpdateBasePlayerHouse();
            // Won't trigger the automatic cleanup and notifications.
            Map.Triggers.Clear();
            Map.Triggers.AddRange(triggers.OrderBy(t => t.Name, comparer));
            extraSections = ini.Sections;
            bool switchedToSolo = forceSoloMission && !Map.BasicSection.SoloMission
                && (Map.Triggers.Any(t => t.Action1.ActionType == ActionTypes.TACTION_WIN) || Map.Triggers.Any(t => t.Action2.ActionType == ActionTypes.TACTION_WIN))
                && (Map.Triggers.Any(t => t.Action1.ActionType == ActionTypes.TACTION_LOSE) || Map.Triggers.Any(t => t.Action2.ActionType == ActionTypes.TACTION_LOSE));
            if (switchedToSolo)
            {
                errors.Insert(0, "Filename detected as classic single player mission format, and win and lose trigger detected. Applying \"SoloMission\" flag.");
                Map.BasicSection.SoloMission = true;
            }
            Map.EndUpdate();
            return errors;
        }

        private void UpdateBuildingRules(INI ini, Map map)
        {
            Dictionary<string, BuildingType> originals = BuildingTypes.GetTypes().ToDictionary(b => b.Name, StringComparer.InvariantCultureIgnoreCase);
            HashSet<Point> refreshPoints = new HashSet<Point>();
            List<(Point Location, Building Occupier)> buildings = map.Buildings.OfType<Building>()
                 .OrderBy(pb => pb.Location.Y * map.Metrics.Width + pb.Location.X).ToList();
            // Remove all buildings
            foreach ((Point p, Building b) in buildings)
            {
                refreshPoints.UnionWith(OccupierSet<Building>.GetOccupyPoints(p, b.OccupyMask));
                map.Buildings.Remove(b);
            }
            // Potentially add new bibs that obstruct stuff
            foreach (BuildingType bType in map.BuildingTypes)
            {
                if (!originals.TryGetValue(bType.Name, out BuildingType orig))
                {
                    continue;
                }
                var bldSettings = ini[bType.Name];
                if (bldSettings == null)
                {
                    // Reset
                    bType.PowerUsage = orig.PowerUsage;
                    bType.PowerProduction = orig.PowerProduction;
                    bType.Storage = orig.Storage;
                    refreshPoints.UnionWith(ChangeBib(map, buildings, bType, orig.HasBib));
                    continue;
                }
                RaBuildingIniSection bld = new RaBuildingIniSection();
                INI.ParseSection(new MapContext(map, true), bldSettings, bld);
                if (bldSettings.Keys.Contains("Power"))
                {
                    bType.PowerUsage = bld.Power >= 0 ? 0 : -bld.Power;
                    bType.PowerProduction = bld.Power <= 0 ? 0 : bld.Power;
                }
                else
                {
                    bType.PowerUsage = orig.PowerUsage;
                    bType.PowerProduction = orig.PowerProduction;
                }
                if (bldSettings.Keys.Contains("Storage"))
                {
                    bType.Storage = bld.Storage;
                }
                else
                {
                    bType.Storage = orig.Storage;
                }
                bool hasBib;
                if (bldSettings.Keys.Contains("Bib"))
                {
                    hasBib = bld.Bib;
                }
                else
                {
                    hasBib = orig.HasBib;
                }
                refreshPoints.UnionWith(ChangeBib(map, buildings, bType, hasBib));
            }
            // Try re-adding the buildings.
            foreach ((Point p, Building b) in buildings)
            {
                refreshPoints.UnionWith(OccupierSet<Building>.GetOccupyPoints(p, b.OccupyMask));
                map.Buildings.Add(p, b);
            }
            map.NotifyRulesChanges(refreshPoints);
        }

        /// <summary>
        /// Bibs need a special refresh logic because they need to remove walls.
        /// </summary>
        /// <param name="map">Map</param>
        /// //<param name="buildings">List of buildings, since the original in map got wiped.</param>
        /// <param name="bType">Building type</param>
        /// <param name="hasBib">true if the new setting says the building will have a bib.</param>
        /// <returns>The points of and directly around any removed walls.</returns>
        private IEnumerable<Point> ChangeBib(Map map, IEnumerable<(Point Location, Building Occupier)> buildings, BuildingType bType, Boolean hasBib)
        {
            HashSet<Point> changed = new HashSet<Point>();
            if (bType.HasBib == hasBib)
            {
                return changed;
            }
            List<(Point p, Building b)> foundBuildings = buildings.Where(lo => bType.Name.Equals(lo.Occupier.Type.Name, StringComparison.InvariantCultureIgnoreCase))
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
            return Save(path, fileType, null);
        }

        public bool Save(string path, FileType fileType, Bitmap customPreview)
        {
            String errors = Validate();
            if (errors != null)
            {
                MessageBox.Show(errors, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            var ini = new INI();
            switch (fileType)
            {
                case FileType.INI:
                case FileType.BIN:
                    SaveINI(ini, fileType, path);
                    using (var mprWriter = new StreamWriter(path))
                    {
                        mprWriter.Write(ini.ToString());
                    }
                    if (!Map.BasicSection.SoloMission || !Properties.Settings.Default.NoMetaFilesForSinglePlay)
                    {
                        var tgaPath = Path.ChangeExtension(path, ".tga");
                        var jsonPath = Path.ChangeExtension(path, ".json");
                        using (var tgaStream = new FileStream(tgaPath, FileMode.Create))
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
                        using (var jsonStream = new FileStream(jsonPath, FileMode.Create))
                        using (var jsonWriter = new JsonTextWriter(new StreamWriter(jsonStream)))
                        {
                            SaveJSON(jsonWriter);
                        }
                    }
                    break;
                case FileType.MEG:
                case FileType.PGM:
                    {
                        SaveINI(ini, fileType, path);
                        using (var iniStream = new MemoryStream())
                        using (var tgaStream = new MemoryStream())
                        using (var jsonStream = new MemoryStream())
                        using (var iniWriter = new StreamWriter(iniStream))
                        using (var jsonWriter = new JsonTextWriter(new StreamWriter(jsonStream)))
                        using (var megafileBuilder = new MegafileBuilder(String.Empty, path))
                        {
                            iniWriter.Write(ini.ToString());
                            iniWriter.Flush();
                            iniStream.Position = 0;
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
                            var mprFile = Path.ChangeExtension(Path.GetFileName(path), ".mpr").ToUpper();
                            var tgaFile = Path.ChangeExtension(Path.GetFileName(path), ".tga").ToUpper();
                            var jsonFile = Path.ChangeExtension(Path.GetFileName(path), ".json").ToUpper();
                            megafileBuilder.AddFile(mprFile, iniStream);
                            megafileBuilder.AddFile(tgaFile, tgaStream);
                            megafileBuilder.AddFile(jsonFile, jsonStream);
                            megafileBuilder.Write();
                        }
                        break;
                    }
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
            Model.BasicSection basic = Map.BasicSection;
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
                    String word = name[i];
                    // Very very rough APA title casing :)
                    if (word.Length > 3)
                    {
                        name[i] = word[0].ToString().ToUpper() + word.Substring(1).ToLowerInvariant();
                    }
                }
                basic.Name = String.Join(" ", name);
            }
            INI.WriteSection(new MapContext(Map, false), ini.Sections.Add("Basic"), Map.BasicSection);
            Map.MapSection.FixBounds();
            INI.WriteSection(new MapContext(Map, false), ini.Sections.Add("Map"), Map.MapSection);
            if (fileType != FileType.PGM)
            {
                INI.WriteSection(new MapContext(Map, false), ini.Sections.Add("Steam"), Map.SteamSection);
            }
            ini["Basic"]["NewINIFormat"] = "3";
            var smudgeSection = ini.Sections.Add("SMUDGE");
            // Flatten multi-cell bibs
            Dictionary<int, Smudge> resolvedSmudge = new Dictionary<int, Smudge>();
            foreach (var (cell, smudge) in Map.Smudge.Where(item => !item.Value.Type.IsAutoBib))
            {
                int actualCell = SmudgeType.GetCellFromIcon(smudge, cell, this.Map.Metrics);
                if (!resolvedSmudge.ContainsKey(actualCell))
                {
                    resolvedSmudge[actualCell] = smudge;
                }
            }
            foreach (int cell in resolvedSmudge.Keys.OrderBy(c => c))
            {
                Smudge smudge = resolvedSmudge[cell];
                smudgeSection[cell.ToString()] = string.Format("{0},{1},{2}", smudge.Type.Name.ToUpper(), cell, Math.Min(smudge.Type.Icons - 1, smudge.Icon));
            }
            var terrainSection = ini.Sections.Add("TERRAIN");
            foreach (var (location, terrain) in Map.Technos.OfType<Terrain>())
            {
                Map.Metrics.GetCell(location, out int cell);
                terrainSection[cell.ToString()] = terrain.Type.Name.ToUpper();
            }
            var cellTriggersSection = ini.Sections.Add("CellTriggers");
            foreach (var (cell, cellTrigger) in Map.CellTriggers)
            {
                cellTriggersSection[cell.ToString()] = cellTrigger.Trigger;
            }
            int nameToIndex<T>(IList<T> list, string name)
            {
                var index = list.TakeWhile(x => !x.Equals(name)).Count();
                return (index < list.Count) ? index : -1;
            }
            string nameToIndexString<T>(IList<T> list, string name) => nameToIndex(list, name).ToString();
            var teamTypesSection = ini.Sections.Add("TeamTypes");
            foreach (var teamType in Map.TeamTypes)
            {
                var classes = teamType.Classes
                    .Select(c => string.Format("{0}:{1}", c.Type.Name.ToUpper(), c.Count))
                    .ToArray();
                var missions = teamType.Missions
                    .Select(m => string.Format("{0}:{1}", m.Mission.ID, m.Argument))
                    .ToArray();
                int flags = 0;
                if (teamType.IsRoundAbout) flags |= 0x01;
                if (teamType.IsSuicide) flags |= 0x02;
                if (teamType.IsAutocreate) flags |= 0x04;
                if (teamType.IsPrebuilt) flags |= 0x08;
                if (teamType.IsReinforcable) flags |= 0x10;
                var tokens = new List<string>
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
            var infantrySection = ini.Sections.Add("INFANTRY");
            var infantryIndex = 0;
            foreach (var (location, infantryGroup) in Map.Technos.OfType<InfantryGroup>())
            {
                for (var i = 0; i < infantryGroup.Infantry.Length; ++i)
                {
                    var infantry = infantryGroup.Infantry[i];
                    if (infantry == null)
                    {
                        continue;
                    }
                    var key = infantryIndex.ToString("D3");
                    infantryIndex++;

                    Map.Metrics.GetCell(location, out int cell);
                    infantrySection[key] = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                        infantry.House.Name,
                        infantry.Type.Name,
                        infantry.Strength,
                        cell,
                        i,
                        String.IsNullOrEmpty(infantry.Mission) ? "Guard" : infantry.Mission,
                        infantry.Direction.ID,
                        infantry.Trigger
                    );
                }
            }
            var structuresSection = ini.Sections.Add("STRUCTURES");
            var structureIndex = 0;
            foreach (var (location, building) in Map.Buildings.OfType<Building>().Where(x => x.Occupier.IsPrebuilt))
            {
                var key = structureIndex.ToString("D3");
                structureIndex++;

                Map.Metrics.GetCell(location, out int cell);
                structuresSection[key] = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                    building.House.Name,
                    building.Type.Name,
                    building.Strength,
                    cell,
                    building.Direction.ID,
                    building.Trigger,
                    building.Sellable ? 1 : 0,
                    building.Rebuild ? 1 : 0
                );
            }
            var baseSection = ini.Sections.Add("Base");
            var baseBuildings = Map.Buildings.OfType<Building>().Where(x => x.Occupier.BasePriority >= 0).OrderBy(x => x.Occupier.BasePriority).ToArray();
            baseSection["Player"] = Map.BasicSection.BasePlayer;
            baseSection["Count"] = baseBuildings.Length.ToString();
            var baseIndex = 0;
            foreach (var (location, building) in baseBuildings)
            {
                var key = baseIndex.ToString("D3");
                baseIndex++;

                Map.Metrics.GetCell(location, out int cell);
                baseSection[key] = string.Format("{0},{1}",
                    building.Type.Name.ToUpper(),
                    cell
                );
            }
            var unitsSection = ini.Sections.Add("UNITS");
            var unitIndex = 0;
            foreach (var (location, unit) in Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsUnit))
            {
                var key = unitIndex.ToString("D3");
                unitIndex++;

                Map.Metrics.GetCell(location, out int cell);
                unitsSection[key] = string.Format("{0},{1},{2},{3},{4},{5},{6}",
                    unit.House.Name,
                    unit.Type.Name,
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
                var aircraftSection = ini.Sections.Add("AIRCRAFT");
                var aircraftIndex = 0;
                foreach (var (location, aircraft) in Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsAircraft))
                {
                    var key = aircraftIndex.ToString("D3");
                    aircraftIndex++;

                    Map.Metrics.GetCell(location, out int cell);
                    aircraftSection[key] = string.Format("{0},{1},{2},{3},{4},{5}",
                        aircraft.House.Name,
                        aircraft.Type.Name,
                        aircraft.Strength,
                        cell,
                        aircraft.Direction.ID,
                        String.IsNullOrEmpty(aircraft.Mission) ? "Guard" : aircraft.Mission
                    );
                }
            }
            var shipsSection = ini.Sections.Add("SHIPS");
            var shipsIndex = 0;
            foreach (var (location, ship) in Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsVessel))
            {
                var key = shipsIndex.ToString("D3");
                shipsIndex++;

                Map.Metrics.GetCell(location, out int cell);
                shipsSection[key] = string.Format("{0},{1},{2},{3},{4},{5},{6}",
                    ship.House.Name,
                    ship.Type.Name,
                    ship.Strength,
                    cell,
                    ship.Direction.ID,
                    String.IsNullOrEmpty(ship.Mission) ? "Guard" : ship.Mission,
                    ship.Trigger
                );
            }

            var triggersSection = ini.Sections.Add("Trigs");
            foreach (var trigger in Map.Triggers)
            {
                if (string.IsNullOrEmpty(trigger.Name))
                {
                    continue;
                }

                var action2TypeIndex = nameToIndex(Map.ActionTypes, trigger.Action2.ActionType);
                var actionControl = (action2TypeIndex > 0) ? TriggerMultiStyleType.And : TriggerMultiStyleType.Only;

                var tokens = new List<string>
                {
                    ((int)trigger.PersistentType).ToString(),
                    !string.IsNullOrEmpty(trigger.House) ? (Map.HouseTypes.Where(h => h.Equals(trigger.House)).FirstOrDefault()?.ID.ToString() ?? "-1") : "-1",
                    ((int)trigger.EventControl).ToString(),
                    ((int)actionControl).ToString(),
                    nameToIndexString(Map.EventTypes, trigger.Event1.EventType),
                    nameToIndexString(Map.TeamTypes, trigger.Event1.Team),
                    trigger.Event1.Data.ToString(),
                    nameToIndexString(Map.EventTypes, trigger.Event2.EventType),
                    nameToIndexString(Map.TeamTypes, trigger.Event2.Team),
                    trigger.Event2.Data.ToString(),
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

            var waypointsSection = ini.Sections.Add("Waypoints");
            for (var i = 0; i < Map.Waypoints.Length; ++i)
            {
                var waypoint = Map.Waypoints[i];
                if (waypoint.Cell.HasValue)
                {
                    waypointsSection[i.ToString()] = waypoint.Cell.Value.ToString();
                }
            }

            foreach (var house in Map.Houses)
            {
                if ((house.Type.ID < 0) || !house.Enabled)
                {
                    continue;
                }
                INI.WriteSection(new MapContext(Map, true), ini.Sections.Add(house.Type.Name), house);
            }

            ini.Sections.Remove("Briefing");
            if (!string.IsNullOrEmpty(Map.BriefingSection.Briefing))
            {
                var briefingSection = ini.Sections.Add("Briefing");
                briefingSection["Text"] = Map.BriefingSection.Briefing.Replace(Environment.NewLine, "@");
            }

            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    for (var y = 0; y < Map.Metrics.Height; ++y)
                    {
                        for (var x = 0; x < Map.Metrics.Width; ++x)
                        {
                            var template = Map.Templates[y, x];
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

                    for (var y = 0; y < Map.Metrics.Height; ++y)
                    {
                        for (var x = 0; x < Map.Metrics.Width; ++x)
                        {
                            var template = Map.Templates[y, x];
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

            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    for (var i = 0; i < Map.Metrics.Length; ++i)
                    {
                        var overlay = Map.Overlay[i];
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

        private void SaveMapPreview(Stream stream, Boolean renderAll)
        {
            Map.GenerateMapPreview(renderAll ? this.GameType : GameType.None, renderAll).Save(stream);
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
                foreach (var waypoint in Map.Waypoints.Where(w => (w.Flag & WaypointFlag.PlayerStart) == WaypointFlag.PlayerStart && w.Cell.HasValue))
                {
                    writer.WriteValue(waypoint.Cell.Value);
                }
            }
            else
            {
                // Probably useless, but better than the player start points.
                foreach (var waypoint in Map.Waypoints.Where(w => (w.Flag & WaypointFlag.Home) == WaypointFlag.Home && w.Cell.HasValue))
                {
                    writer.WriteValue(waypoint.Cell.Value);
                }
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public string Validate()
        {
            StringBuilder sb = new StringBuilder("Error(s) during map validation:");
            bool ok = true;
            int numAircraft = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsAircraft).Count();
            int numBuildings = Map.Buildings.OfType<Building>().Where(x => x.Occupier.IsPrebuilt).Count();
            int numInfantry = Map.Technos.OfType<InfantryGroup>().Sum(item => item.Occupier.Infantry.Count(i => i != null));
            int numTerrain = Map.Technos.OfType<Terrain>().Count();
            int numUnits = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsUnit).Count();
            int numVessels = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsVessel).Count();
            int numWaypoints = Map.Waypoints.Count(w => (w.Flag & WaypointFlag.PlayerStart) == WaypointFlag.PlayerStart && w.Cell.HasValue);
            if (numAircraft > Constants.MaxAircraft)
            {
                sb.AppendLine().Append(string.Format("Maximum number of aircraft exceeded ({0} > {1})", numAircraft, Constants.MaxAircraft));
                ok = false;
            }
            if (numBuildings > Constants.MaxBuildings)
            {
                sb.AppendLine().Append(string.Format("Maximum number of structures exceeded ({0} > {1})", numBuildings, Constants.MaxBuildings));
                ok = false;
            }
            if (numInfantry > Constants.MaxInfantry)
            {
                sb.AppendLine().Append(string.Format("Maximum number of infantry exceeded ({0} > {1})", numInfantry, Constants.MaxInfantry));
                ok = false;
            }
            if (numTerrain > Constants.MaxTerrain)
            {
                sb.AppendLine().Append(string.Format("Maximum number of terrain objects exceeded ({0} > {1})", numTerrain, Constants.MaxTerrain));
                ok = false;
            }
            if (numUnits > Constants.MaxUnits)
            {
                sb.AppendLine().Append(string.Format("Maximum number of units exceeded ({0} > {1})", numUnits, Constants.MaxUnits));
                ok = false;
            }
            if (numVessels > Constants.MaxVessels)
            {
                sb.AppendLine().Append(string.Format("Maximum number of ships exceeded ({0} > {1})", numVessels, Constants.MaxVessels));
                ok = false;
            }
            if (Map.TeamTypes.Count > Constants.MaxTeams)
            {
                sb.AppendLine().Append(string.Format("Maximum number of team types exceeded ({0} > {1})", Map.TeamTypes.Count, Constants.MaxTeams));
                ok = false;
            }
            if (Map.Triggers.Count > Constants.MaxTriggers)
            {
                sb.AppendLine().Append(string.Format("Maximum number of triggers exceeded ({0} > {1})", Map.Triggers.Count, Constants.MaxTriggers));
                ok = false;
            }
            if (!Map.BasicSection.SoloMission && (numWaypoints < 2))
            {
                sb.AppendLine().Append("Skirmish/Multiplayer maps need at least 2 waypoints for player starting locations.");
                ok = false;
            }
            var homeWaypoint = Map.Waypoints.Where(w => (w.Flag & WaypointFlag.Home) == WaypointFlag.Home).FirstOrDefault();
            if (Map.BasicSection.SoloMission && !homeWaypoint.Cell.HasValue)
            {
                sb.AppendLine().Append("Single-player maps need the Home waypoint to be placed.");
                ok = false;
            }
            return ok ? null : sb.ToString();
        }

        private void BasicSection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "BasePlayer":
                    UpdateBasePlayerHouse();
                    break;
                case "ExpansionEnabled":
                    // Not here. See ViewTool.cs for the Aftermath units clearing.
                    //UpdateExpansionUnits();
                    if (!isLoading)
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
            var basePlayer = Map.HouseTypesIncludingNone.Where(h => h.Equals(Map.BasicSection.BasePlayer)).FirstOrDefault() ?? Map.HouseTypes.First();
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
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                foreach (var decompressedChunk in decompressedBytes.Split(8192))
                {
                    var compressedChunk = WWCompression.LcwCompress(decompressedChunk);
                    writer.Write((ushort)compressedChunk.Length);
                    writer.Write((ushort)decompressedChunk.Length);
                    writer.Write(compressedChunk);
                }
                writer.Flush();
                stream.Position = 0;
                var values = Convert.ToBase64String(stream.ToArray()).Split(70).ToArray();
                for (var i = 0; i < values.Length; ++i)
                {
                    section[(i + 1).ToString()] = values[i];
                }
            }
        }

        private byte[] DecompressLCWSection(INISection section, int bytesPerCell, List<string> errors)
        {
            var sb = new StringBuilder();
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
            var decompressedBytes = new byte[Map.Metrics.Width * Map.Metrics.Height * bytesPerCell];
            while ((readPtr + 4) <= compressedBytes.Length)
            {
                uint uLength;
                using (var reader = new BinaryReader(new MemoryStream(compressedBytes, readPtr, 4)))
                {
                    uLength = reader.ReadUInt32();
                }
                var length = (int)(uLength & 0x0000FFFF);
                readPtr += 4;
                var dest = new byte[8192];
                var readPtr2 = readPtr;
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
                    MapImage?.Dispose();
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
