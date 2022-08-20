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
        private readonly IEnumerable<string> movieTypes;

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

        public GameType GameType => GameType.RedAlert;

        public Map Map { get; }

        public Image MapImage { get; private set; }

        public bool Dirty { get; set; }

        private INISectionCollection extraSections;

        static GamePlugin()
        {
            fullTechnoTypes = InfantryTypes.GetTypes().Cast<ITechnoType>().Concat(UnitTypes.GetTypes(false).Cast<ITechnoType>());
        }

        public GamePlugin(bool mapImage)
        {
            var playerWaypoints = Enumerable.Range(0, 8).Select(i => new Waypoint(string.Format("P{0}", i), WaypointFlag.PlayerStart));
            var generalWaypoints = Enumerable.Range(8, 90).Select(i => new Waypoint(i.ToString()));
            var specialWaypoints = new Waypoint[] { new Waypoint("Home"), new Waypoint("Reinf."), new Waypoint("Special") };
            var waypoints = playerWaypoints.Concat(generalWaypoints).Concat(specialWaypoints);
            var movies = new List<string>(movieTypesRa);
            for (int i = 0; i < movies.Count; ++i)
            {
                string vidName = GeneralUtils.AddRemarks(movies[i], "x", true, movieTypesRemarksOld, RemarkOld);
                movies[i] = GeneralUtils.AddRemarks(vidName, "x", true, movieTypesRemarksNew, RemarkNew);
            }
            movies.Insert(0, "x");
            movieTypes = movies.ToArray();
            var basicSection = new BasicSection();
            basicSection.SetDefault();
            var houseTypes = HouseTypes.GetTypes();
            basicSection.Player = houseTypes.First().Name;
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
            string[] structureEventTypes = unitEventTypes.Concat( new []
            {
                EventTypes.TEVENT_PLAYER_ENTERED,
                EventTypes.TEVENT_SPIED,
            }).ToArray();
            string[] terrainEventTypes = { };
            string[] cellActionTypes = { ActionTypes.TACTION_DESTROY_OBJECT };
            string[] unitActionTypes = { };
            string[] structureActionTypes = { ActionTypes.TACTION_DESTROY_OBJECT };
            string[] terrainActionTypes = { };

            Map = new Map(basicSection, null, Constants.MaxSize, typeof(House),
                houseTypes, TheaterTypes.GetTypes(), TemplateTypes.GetTypes(),
                TerrainTypes.GetTypes(), OverlayTypes.GetTypes(), SmudgeTypes.GetTypes(),
                EventTypes.GetTypes(), cellEventTypes, unitEventTypes, structureEventTypes, terrainEventTypes,
                ActionTypes.GetTypes(), cellActionTypes, unitActionTypes, structureActionTypes, terrainActionTypes,
                MissionTypes.GetTypes(), DirectionTypes.GetTypes(), InfantryTypes.GetTypes(), UnitTypes.GetTypes(true),
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

        public GamePlugin()
            : this(true)
        {
        }

        public void New(string theater)
        {
            Map.Theater = Map.TheaterTypes.Where(t => t.Equals(theater)).FirstOrDefault() ?? TheaterTypes.Temperate;
            Map.TopLeft = new Point(1, 1);
            Map.Size = Map.Metrics.Size - new Size(2, 2);
            UpdateBasePlayerHouse();
            //Dirty = true;
        }

        public IEnumerable<string> Load(string path, FileType fileType)
        {
            var errors = new List<string>();
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
                        errors.AddRange(LoadINI(ini));
                    }
                    break;
                case FileType.MEG:
                case FileType.PGM:
                    {
                        using (var megafile = new Megafile(path))
                        {
                            var mprFile = megafile.Where(p => Path.GetExtension(p).ToLower() == ".mpr").FirstOrDefault();
                            if (mprFile != null)
                            {
                                var ini = new INI();
                                using (var reader = new StreamReader(megafile.Open(mprFile)))
                                {
                                    ini.Parse(reader);
                                }
                                errors.AddRange(LoadINI(ini));
                            }
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
            return errors;
        }

        private IEnumerable<string> LoadINI(INI ini)
        {
            var errors = new List<string>();
            Map.BeginUpdate();
            var basicSection = ini.Sections.Extract("Basic");
            if (basicSection != null)
            {
                INI.ParseSection(new MapContext(Map, true), basicSection, Map.BasicSection);
                Model.BasicSection basic = Map.BasicSection;
                basic.Intro = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Intro, "x", true, movieTypesRemarksOld, RemarkOld), "x", true, movieTypesRemarksNew, RemarkNew);
                basic.Intro = GeneralUtils.FilterToExisting(basic.Intro, "x", true, movieTypesRa);
                basic.Brief = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Brief, "x", true, movieTypesRemarksOld, RemarkOld), "x", true, movieTypesRemarksNew, RemarkNew);
                basic.Brief = GeneralUtils.FilterToExisting(basic.Brief, "x", true, movieTypesRa);
                basic.Action = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Action, "x", true, movieTypesRemarksOld, RemarkOld), "x", true, movieTypesRemarksNew, RemarkNew);
                basic.Action = GeneralUtils.FilterToExisting(basic.Action, "x", true, movieTypesRa);
                basic.Win = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Win, "x", true, movieTypesRemarksOld, RemarkOld), "x", true, movieTypesRemarksNew, RemarkNew);
                basic.Win = GeneralUtils.FilterToExisting(basic.Win, "x", true, movieTypesRa);
                basic.Win2 = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Win2, "x", true, movieTypesRemarksOld, RemarkOld), "x", true, movieTypesRemarksNew, RemarkNew);
                basic.Win2 = GeneralUtils.FilterToExisting(basic.Win2, "x", true, movieTypesRa);
                basic.Win3 = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Win3, "x", true, movieTypesRemarksOld, RemarkOld), "x", true, movieTypesRemarksNew, RemarkNew);
                basic.Win3 = GeneralUtils.FilterToExisting(basic.Win3, "x", true, movieTypesRa);
                basic.Win4 = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Win4, "x", true, movieTypesRemarksOld, RemarkOld), "x", true, movieTypesRemarksNew, RemarkNew);
                basic.Win4 = GeneralUtils.FilterToExisting(basic.Win4, "x", true, movieTypesRa);
                basic.Lose = GeneralUtils.AddRemarks(GeneralUtils.AddRemarks(basic.Lose, "x", true, movieTypesRemarksOld, RemarkOld), "x", true, movieTypesRemarksNew, RemarkNew);
                basic.Lose = GeneralUtils.FilterToExisting(basic.Lose, "x", true, movieTypesRa);
            }
            Map.BasicSection.Player = Map.HouseTypes.Where(t => t.Equals(Map.BasicSection.Player)).FirstOrDefault()?.Name ?? Map.HouseTypes.First().Name;
            Map.BasicSection.BasePlayer = HouseTypes.GetBasePlayer(Map.BasicSection.Player);
            var mapSection = ini.Sections.Extract("Map");
            if (mapSection != null)
            {
                INI.ParseSection(new MapContext(Map, true), mapSection, Map.MapSection);
            }
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
            if (teamTypesSection != null)
            {
                foreach (var (Key, Value) in teamTypesSection)
                {
                    try
                    {
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
                                var type = fullTechnoTypes.Where(t => t.Equals(classTokens[0])).FirstOrDefault();
                                var count = byte.Parse(classTokens[1]);
                                if (type != null)
                                {
                                    teamType.Classes.Add(new TeamTypeClass { Type = type, Count = count });
                                }
                                else
                                {
                                    errors.Add(string.Format("Team '{0}' references unknown class '{1}'.", Key, classTokens[0]));
                                }
                            }
                            else
                            {
                                errors.Add(string.Format("Team '{0}' has wrong number of tokens for class index {1} (expecting 2).", Key, i));
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
                            }
                        }
                        Map.TeamTypes.Add(teamType);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        errors.Add(string.Format("Teamtype '{0}' has errors and can't be parsed.", Key));
                    }
                }
            }
            var triggersSection = ini.Sections.Extract("Trigs");
            if (triggersSection != null)
            {
                foreach (var (Key, Value) in triggersSection)
                {
                    var tokens = Value.Split(',');
                    if (tokens.Length == 18)
                    {
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
                        Map.Triggers.Add(trigger);
                    }
                    else
                    {
                        errors.Add(string.Format("Trigger '{0}' has too few tokens (expecting 18).", Key));
                    }
                }
            }
            HashSet<string> checkTrigs = Trigger.None.Yield().Concat(Map.Triggers.Select(t => t.Name)).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            HashSet<string> checkCellTrigs = Map.FilterCellTriggers().Select(t => t.Name).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            HashSet<string> checkUnitTrigs = Trigger.None.Yield().Concat(Map.FilterUnitTriggers().Select(t => t.Name)).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            HashSet<string> checkStrcTrigs = Trigger.None.Yield().Concat(Map.FilterStructureTriggers().Select(t => t.Name)).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            // Terrain objects in RA have no triggers
            //HashSet<string> checkTerrTrigs = Trigger.None.Yield().Concat(Map.FilterTerrainTriggers().Select(t => t.Name)).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            var mapPackSection = ini.Sections.Extract("MapPack");
            if (mapPackSection != null)
            {
                Map.Templates.Clear();
                var data = DecompressLCWSection(mapPackSection, 3, errors);
                if (data != null)
                {
                    using (var reader = new BinaryReader(new MemoryStream(data)))
                    {
                        for (var y = 0; y < Map.Metrics.Height; ++y)
                        {
                            for (var x = 0; x < Map.Metrics.Width; ++x)
                            {
                                var typeValue = reader.ReadUInt16();
                                var templateType = Map.TemplateTypes.Where(t => t.Equals(typeValue)).FirstOrDefault();
                                if (templateType == null && typeValue != 0xFFFF)
                                {
                                    errors.Add(String.Format("Unknown template value {0:X2} at cell [{1},{2}]; clearing.", typeValue, x, y));
                                }
                                else if (templateType != null)
                                {
                                    if ((templateType.Flag & TemplateTypeFlag.Clear) != TemplateTypeFlag.None || (templateType.Flag & TemplateTypeFlag.Group) == TemplateTypeFlag.Group)
                                    {
                                        // No explicitly set Clear terrain allowed. Also no explicitly set versions allowed of the "group" dummy entries.
                                        templateType = null;
                                    }
                                    else if (!templateType.Theaters.Contains(Map.Theater))
                                    {
                                        errors.Add(String.Format("Template '{0}' at cell [{1},{2}] is not available in the set theater; clearing.", templateType.Name.ToUpper(), x, y));
                                        templateType = null;
                                    }
                                }
                                Map.Templates[x, y] = (templateType != null) ? new Template { Type = templateType } : null;
                            }
                        }
                        for (var y = 0; y < Map.Metrics.Height; ++y)
                        {
                            for (var x = 0; x < Map.Metrics.Width; ++x)
                            {
                                var iconValue = reader.ReadByte();
                                var template = Map.Templates[x, y];
                                // Prevent loading of illegal tiles.
                                if (template != null)
                                {
                                    var templateType = template.Type;
                                    bool isRandom = (templateType.Flag & TemplateTypeFlag.RandomCell) != TemplateTypeFlag.None;
                                    bool tileOk = false;
                                    if (iconValue >= templateType.NumIcons)
                                    {
                                        errors.Add(String.Format("Template '{0}' at cell [{1},{2}] has an icon set ({3}) that is outside its icons range; clearing.", templateType.Name.ToUpper(), x, y, iconValue));
                                    }
                                    else if (!isRandom && !templateType.IconMask[iconValue % templateType.IconWidth, iconValue / templateType.IconWidth])
                                    {
                                        errors.Add(String.Format("Template '{0}' at cell [{1},{2}] has an icon set ({3}) that is not part of its placeable cells; clearing.", templateType.Name.ToUpper(), x, y, iconValue));
                                    }
                                    else if (templateType != TemplateTypes.Clear)
                                    {
                                        tileOk = true;
                                    }
                                    if (!tileOk)
                                    {
                                        Map.Templates[x, y] = null;
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
                    var cell = int.Parse(Key);
                    var name = Value.Split(',')[0];
                    var terrainType = Map.TerrainTypes.Where(t => t.Equals(name)).FirstOrDefault();
                    if (terrainType != null)
                    {
                        Terrain newTerr = new Terrain
                        {
                            Type = terrainType,
                            Icon = terrainType.DisplayIcon,
                            Trigger = Trigger.None
                        };
                        if (Map.Technos.Add(cell, newTerr))
                        {
                            //if (!checkTrigs.Contains(newTerr.Trigger))
                            //{
                            //    errors.Add(string.Format("Terrain '{0}' links to unknown trigger '{1}'; clearing trigger..", terrainType, newTerr.Trigger));
                            //    newTerr.Trigger = Trigger.None;
                            //}
                            //else if (!checkTerrTrigs.Contains(Value))
                            //{
                            //    errors.Add(string.Format("Terrain '{0}' links to trigger '{1}' which does not contain an event applicable to terrain; clearing trigger.", terrainType, newTerr.Trigger));
                            //    newTerr.Trigger = Trigger.None;
                            //}
                        }
                        else
                        {
                            var techno = Map.FindBlockingObject(cell, terrainType, out int blockingCell);
                            int reportCell = blockingCell == -1 ? cell : blockingCell;
                            if (techno is Building building)
                            {
                                errors.Add(string.Format("Terrain '{0}' overlaps structure '{1}' in cell {2}; skipping.", name, building.Type.Name, reportCell));
                            }
                            else if (techno is Overlay overlay)
                            {
                                errors.Add(string.Format("Terrain '{0}' overlaps overlay '{1}' in cell {2}; skipping.", name, overlay.Type.Name, reportCell));
                            }
                            else if (techno is Terrain terrain)
                            {
                                errors.Add(string.Format("Terrain '{0}' overlaps terrain object '{1}' in cell {2}; skipping.", name, terrain.Type.Name, reportCell));
                            }
                            else if (techno is InfantryGroup infantry)
                            {
                                errors.Add(string.Format("Terrain '{0}' overlaps infantry in cell {1}; skipping.", name, reportCell));
                            }
                            else if (techno is Unit unit)
                            {
                                errors.Add(string.Format("Terrain '{0}' overlaps unit '{1}' in cell {2}; skipping.", name, unit.Type.Name, reportCell));
                            }
                            else
                            {
                                errors.Add(string.Format("Terrain '{0}' overlaps unknown techno in cell {1}; skipping.", name, reportCell));
                            }
                        }
                    }
                    else
                    {
                        errors.Add(string.Format("Terrain '{0}' references unknown terrain.", name));
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
                                Map.Overlay[i] = new Overlay { Type = overlayType, Icon = 0 };
                            }
                            else
                            {
                                errors.Add(string.Format("Overlay ID {0} references unknown overlay.", overlayId));
                            }
                        }
                    }
                }
            }
            var smudgeSection = ini.Sections.Extract("Smudge");
            // Craters other than cr1 don't work right in the game. Replace them by stage-0 cr1.
            Regex craterRegex = new Regex("^CR[2-6]$", RegexOptions.IgnoreCase);
            if (smudgeSection != null)
            {
                foreach (var (Key, Value) in smudgeSection)
                {
                    var cell = int.Parse(Key);
                    var tokens = Value.Split(',');
                    if (tokens.Length == 3)
                    {
                        bool badCrater = craterRegex.IsMatch(tokens[0]);
                        var smudgeType = badCrater ? SmudgeTypes.Crater1 : Map.SmudgeTypes.Where(t => t.Equals(tokens[0]) && (t.Flag & SmudgeTypeFlag.Bib) == 0).FirstOrDefault();
                        if (smudgeType != null)
                        {
                            int icon = 0;
                            if (smudgeType.Icons > 1 && int.TryParse(tokens[2], out icon))
                                icon = Math.Max(0, Math.Min(smudgeType.Icons - 1, icon));
                            Map.Smudge[cell] = new Smudge
                            {
                                Type = smudgeType,
                                Icon = icon
                            };
                        }
                        else
                        {
                            errors.Add(string.Format("Smudge '{0}' references unknown smudge.", tokens[0]));
                        }
                    }
                    else
                    {
                        errors.Add(string.Format("Smudge on cell '{0}' has wrong number of tokens (expecting 3).", Key));
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
                        var unitType = Map.UnitTypes.Where(t => t.IsUnit && t.Equals(tokens[1])).FirstOrDefault();
                        if (unitType != null)
                        {
                            var direction = (byte)((int.Parse(tokens[4]) + 0x08) & ~0x0F);
                            var cell = int.Parse(tokens[3]);
                            Unit newUnit = new Unit()
                            {
                                Type = unitType,
                                House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                                Strength = int.Parse(tokens[2]),
                                Direction = Map.DirectionTypes.Where(d => d.Equals(direction)).FirstOrDefault(),
                                Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5])).FirstOrDefault() ?? Map.GetDefaultMission(unitType),
                                Trigger = tokens[6]
                            };
                            if (Map.Technos.Add(cell, newUnit))
                            {
                                if (!checkTrigs.Contains(tokens[6]))
                                {
                                    errors.Add(string.Format("Unit '{0}' links to unknown trigger '{1}'; clearing trigger.", tokens[1], tokens[6]));
                                    newUnit.Trigger = Trigger.None;
                                }
                                else if (!checkUnitTrigs.Contains(tokens[6]))
                                {
                                    errors.Add(string.Format("Unit '{0}' links to trigger '{1}' which does not contain an event applicable to units; clearing trigger.", tokens[1], tokens[6]));
                                    newUnit.Trigger = Trigger.None;
                                }
                            }
                            else
                            {
                                var techno = Map.Technos[cell];
                                if (techno is Building building)
                                {
                                    errors.Add(string.Format("Unit '{0}' overlaps structure '{1}' in cell {2}; skipping.", tokens[1], building.Type.Name, cell));
                                }
                                else if (techno is Overlay overlay)
                                {
                                    errors.Add(string.Format("Unit '{0}' overlaps overlay '{1}' in cell {2}; skipping.", tokens[1], overlay.Type.Name, cell));
                                }
                                else if (techno is Terrain terrain)
                                {
                                    errors.Add(string.Format("Unit '{0}' overlaps terrain '{1}' in cell {2}; skipping.", tokens[1], terrain.Type.Name, cell));
                                }
                                else if (techno is InfantryGroup infantry)
                                {
                                    errors.Add(string.Format("Unit '{0}' overlaps infantry in cell {1}; skipping.", tokens[1], cell));
                                }
                                else if (techno is Unit unit)
                                {
                                    errors.Add(string.Format("Unit '{0}' overlaps unit '{1}' in cell {2}; skipping.", tokens[1], unit.Type.Name, cell));
                                }
                                else
                                {
                                    errors.Add(string.Format("Unit '{0}' overlaps unknown techno in cell {1}; skipping.", tokens[1], cell));
                                }
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Unit '{0}' references unknown unit.", tokens[1]));
                        }
                    }
                    else
                    {
                        if (tokens.Length < 2)
                        {
                            errors.Add(string.Format("Unit entry '{0}' has wrong number of tokens (expecting 7).", Key));
                        }
                        else
                        {
                            errors.Add(string.Format("Unit '{0}' has wrong number of tokens (expecting 7).", tokens[1]));
                        }
                    }
                }
            }
            // Classic game does not support this, so I'm leaving this out. It's buggy anyway.
            // Extracting it so it doesn't end up with the "extra sections"
            var aircraftSection = ini.Sections.Extract("Aircraft");
            /*/
            if (aircraftSection != null)
            {
                foreach (var (Key, Value) in aircraftSection)
                {
                    var tokens = Value.Split(',');
                    if (tokens.Length == 6)
                    {
                        var aircraftType = Map.UnitTypes.Where(t => t.IsAircraft && t.Equals(tokens[1])).FirstOrDefault();
                        if (aircraftType != null)
                        {
                            var direction = (byte)((int.Parse(tokens[4]) + 0x08) & ~0x0F);
                            var cell = int.Parse(tokens[3]);
                            if (!Map.Technos.Add(cell, new Unit()
                                {
                                    Type = aircraftType,
                                    House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                                    Strength = int.Parse(tokens[2]),
                                    Direction = Map.DirectionTypes.Where(d => d.Equals(direction)).FirstOrDefault(),
                                    Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5])).FirstOrDefault() ?? Map.GetDefaultMission(aircraftType)
                                }))
                            {
                                var techno = Map.Technos[cell];
                                if (techno is Building building)
                                {
                                    errors.Add(string.Format("Aircraft '{0}' overlaps structure '{1}' in cell {2}; skipping.", tokens[1], building.Type.Name, cell));
                                }
                                else if (techno is Overlay overlay)
                                {
                                    errors.Add(string.Format("Aircraft '{0}' overlaps overlay '{1}' in cell {2}; skipping.", tokens[1], overlay.Type.Name, cell));
                                }
                                else if (techno is Terrain terrain)
                                {
                                    errors.Add(string.Format("Aircraft '{0}' overlaps terrain '{1}' in cell {2}; skipping.", tokens[1], terrain.Type.Name, cell));
                                }
                                else if (techno is InfantryGroup infantry)
                                {
                                    errors.Add(string.Format("Aircraft '{0}' overlaps infantry in cell {1}; skipping.", tokens[1], cell));
                                }
                                else if (techno is Unit unit)
                                {
                                    errors.Add(string.Format("Aircraft '{0}' overlaps unit '{1}' in cell {2}; skipping.", tokens[1], unit.Type.Name, cell));
                                }
                                else
                                {
                                    errors.Add(string.Format("Aircraft '{0}' overlaps unknown techno in cell {1}; skipping.", tokens[1], cell));
                                }
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Aircraft '{0}' references unknown aircraft.", tokens[1]));
                        }
                    }
                    else
                    {
                        if (tokens.Length < 2)
                        {
                            errors.Add(string.Format("Aircraft entry '{0}' has wrong number of tokens (expecting 6).", Key));
                        }
                        else
                        {
                            errors.Add(string.Format("Aircraft '{0}' has wrong number of tokens (expecting 6).", tokens[1]));
                        }
                    }
                }
            }
            //*/
            var shipsSection = ini.Sections.Extract("Ships");
            if (shipsSection != null)
            {
                foreach (var (Key, Value) in shipsSection)
                {
                    var tokens = Value.Split(',');
                    if (tokens.Length == 7)
                    {
                        var vesselType = Map.UnitTypes.Where(t => t.IsVessel && t.Equals(tokens[1])).FirstOrDefault();
                        if (vesselType != null)
                        {
                            var direction = (byte)((int.Parse(tokens[4]) + 0x08) & ~0x0F);
                            var cell = int.Parse(tokens[3]);
                            Unit newShip = new Unit()
                            {
                                Type = vesselType,
                                House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                                Strength = int.Parse(tokens[2]),
                                Direction = Map.DirectionTypes.Where(d => d.Equals(direction)).FirstOrDefault(),
                                Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5])).FirstOrDefault() ?? Map.GetDefaultMission(vesselType),
                                Trigger = tokens[6]
                            };
                            if (Map.Technos.Add(cell, newShip))
                            {
                                if (!checkTrigs.Contains(tokens[6]))
                                {
                                    errors.Add(string.Format("Ship '{0}' links to unknown trigger '{1}'; clearing trigger.", tokens[1], tokens[6]));
                                    newShip.Trigger = Trigger.None;
                                }
                                else if (!checkUnitTrigs.Contains(tokens[6]))
                                {
                                    errors.Add(string.Format("Ship '{0}' links to trigger '{1}' which does not contain an event applicable to ships; clearing trigger.", tokens[1], tokens[6]));
                                    newShip.Trigger = Trigger.None;
                                }
                            }
                            else
                            {
                                var techno = Map.Technos[cell];
                                if (techno is Building building)
                                {
                                    errors.Add(string.Format("Ship '{0}' overlaps structure '{1}' in cell {2}; skipping.", tokens[1], building.Type.Name, cell));
                                }
                                else if (techno is Overlay overlay)
                                {
                                    errors.Add(string.Format("Ship '{0}' overlaps overlay '{1}' in cell {2}; skipping.", tokens[1], overlay.Type.Name, cell));
                                }
                                else if (techno is Terrain terrain)
                                {
                                    errors.Add(string.Format("Ship '{0}' overlaps terrain '{1}' in cell {2}; skipping.", tokens[1], terrain.Type.Name, cell));
                                }
                                else if (techno is InfantryGroup infantry)
                                {
                                    errors.Add(string.Format("Ship '{0}' overlaps infantry in cell {1}; skipping.", tokens[1], cell));
                                }
                                else if (techno is Unit unit)
                                {
                                    errors.Add(string.Format("Ship '{0}' overlaps unit '{1}' in cell {2}; skipping.", tokens[1], unit.Type.Name, cell));
                                }
                                else
                                {
                                    errors.Add(string.Format("Ship '{0}' overlaps unknown techno in cell {1}; skipping.", tokens[1], cell));
                                }
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Ship '{0}' references unknown ship.", tokens[1]));
                        }
                    }
                    else
                    {
                        if (tokens.Length < 2)
                        {
                            errors.Add(string.Format("Ship entry '{0}' has wrong number of tokens (expecting 7).", Key));
                        }
                        else
                        {
                            errors.Add(string.Format("Ship '{0}' has wrong number of tokens (expecting 7).", tokens[1]));
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
                        if (infantryType != null)
                        {
                            var cell = int.Parse(tokens[3]);
                            var infantryGroup = Map.Technos[cell] as InfantryGroup;
                            if ((infantryGroup == null) && (Map.Technos[cell] == null))
                            {
                                infantryGroup = new InfantryGroup();
                                Map.Technos.Add(cell, infantryGroup);
                            }
                            if (infantryGroup != null)
                            {
                                var stoppingPos = int.Parse(tokens[4]);
                                if (stoppingPos < Globals.NumInfantryStops)
                                {
                                    var direction = (byte)((int.Parse(tokens[6]) + 0x08) & ~0x0F);
                                    if (infantryGroup.Infantry[stoppingPos] == null)
                                    {
                                        if (!checkTrigs.Contains(tokens[7]))
                                        {
                                            errors.Add(string.Format("Infantry '{0}' links to unknown trigger '{1}'; clearing trigger.", infantryType, tokens[7]));
                                            tokens[7] = Trigger.None;
                                        }
                                        else if (!checkUnitTrigs.Contains(tokens[7]))
                                        {
                                            errors.Add(string.Format("Infantry '{0}' links to trigger '{1}' which does not contain an event applicable to infantry; clearing trigger.", infantryType, tokens[7]));
                                            tokens[7] = Trigger.None;
                                        }
                                        infantryGroup.Infantry[stoppingPos] = new Infantry(infantryGroup)
                                        {
                                            Type = infantryType,
                                            House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                                            Strength = int.Parse(tokens[2]),
                                            Direction = Map.DirectionTypes.Where(d => d.Equals(direction)).FirstOrDefault(),
                                            Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5])).FirstOrDefault() ?? Map.GetDefaultMission(infantryType),
                                            Trigger = tokens[7]
                                        };
                                    }
                                    else
                                    {
                                        errors.Add(string.Format("Infantry '{0}' overlaps another infantry at position {1} in cell {2}; skipping.", tokens[1], stoppingPos, cell));
                                    }
                                }
                                else
                                {
                                    errors.Add(string.Format("Infantry '{0}' has invalid position {1} in cell {2}; skipping.", tokens[1], stoppingPos, cell));
                                }
                            }
                            else
                            {
                                var techno = Map.Technos[cell];
                                if (techno is Building building)
                                {
                                    errors.Add(string.Format("Infantry '{0}' overlaps structure '{1}' in cell {2}; skipping.", tokens[1], building.Type.Name, cell));
                                }
                                else if (techno is Overlay overlay)
                                {
                                    errors.Add(string.Format("Infantry '{0}' overlaps overlay '{1}' in cell {2}; skipping.", tokens[1], overlay.Type.Name, cell));
                                }
                                else if (techno is Terrain terrain)
                                {
                                    errors.Add(string.Format("Infantry '{0}' overlaps terrain '{1}' in cell {2}; skipping.", tokens[1], terrain.Type.Name, cell));
                                }
                                else if (techno is Unit unit)
                                {
                                    errors.Add(string.Format("Infantry '{0}' overlaps unit '{1}' in cell {2}; skipping.", tokens[1], unit.Type.Name, cell));
                                }
                                else
                                {
                                    errors.Add(string.Format("Infantry '{0}' overlaps unknown techno in cell {1}; skipping.", tokens[1], cell));
                                }
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Infantry '{0}' references unknown infantry.", tokens[1]));
                        }
                    }
                    else
                    {
                        if (tokens.Length < 2)
                        {
                            errors.Add(string.Format("Infantry entry '{0}' has wrong number of tokens (expecting 8).", Key));
                        }
                        else
                        {
                            errors.Add(string.Format("Infantry '{0}' has wrong number of tokens (expecting 8).", tokens[1]));
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
                    if (tokens.Length >= 6)
                    {
                        var buildingType = Map.BuildingTypes.Where(t => t.Equals(tokens[1])).FirstOrDefault();
                        if (buildingType != null)
                        {
                            var direction = (byte)((int.Parse(tokens[4]) + 0x08) & ~0x0F);
                            bool sellable = (tokens.Length >= 7) && (int.Parse(tokens[6]) != 0);
                            bool rebuild = (tokens.Length >= 8) && (int.Parse(tokens[7]) != 0);
                            var cell = int.Parse(tokens[3]);
                            Building newBld = new Building()
                            {
                                Type = buildingType,
                                House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                                Strength = int.Parse(tokens[2]),
                                Direction = Map.DirectionTypes.Where(d => d.Equals(direction)).FirstOrDefault(),
                                Trigger = tokens[5],
                                Sellable = sellable,
                                Rebuild = rebuild
                            };
                            if (Map.Buildings.Add(cell, newBld))
                            {
                                if (!checkTrigs.Contains(tokens[5]))
                                {
                                    errors.Add(string.Format("Structure '{0}' links to unknown trigger '{1}'; clearing trigger.", tokens[1], tokens[5]));
                                    newBld.Trigger = Trigger.None;
                                }
                                else if (!checkStrcTrigs.Contains(tokens[5]))
                                {
                                    errors.Add(string.Format("Structure '{0}' links to trigger '{1}' which does not contain an event applicable to structures; clearing trigger.", tokens[1], tokens[5]));
                                    newBld.Trigger = Trigger.None;
                                }
                            }
                            else
                            {
                                var techno = Map.FindBlockingObject(cell, buildingType, out int blockingCell);
                                int reportCell = blockingCell == -1 ? cell : blockingCell;
                                if (techno is Building building)
                                {
                                    errors.Add(string.Format("Structure '{0}' overlaps structure '{1}' in cell {2}; skipping.", tokens[1], building.Type.Name, reportCell));
                                }
                                else if (techno is Overlay overlay)
                                {
                                    errors.Add(string.Format("Structure '{0}' overlaps overlay '{1}' in cell {2}; skipping.", tokens[1], overlay.Type.Name, reportCell));
                                }
                                else if (techno is Terrain terrain)
                                {
                                    errors.Add(string.Format("Structure '{0}' overlaps terrain '{1}' in cell {2}; skipping.", tokens[1], terrain.Type.Name, reportCell));
                                }
                                else if (techno is InfantryGroup infantry)
                                {
                                    errors.Add(string.Format("Structure '{0}' overlaps infantry in cell {1}; skipping.", tokens[1], reportCell));
                                }
                                else if (techno is Unit unit)
                                {
                                    errors.Add(string.Format("Structure '{0}' overlaps unit '{1}' in cell {2}; skipping.", tokens[1], unit.Type.Name, reportCell));
                                }
                                else
                                {
                                    errors.Add(string.Format("Structure '{0}' overlaps unknown techno in cell {1}; skipping.", tokens[1], reportCell));
                                }
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Structure '{0}' references unknown structure.", tokens[1]));
                        }
                    }
                    else
                    {
                        if (tokens.Length < 2)
                        {
                            errors.Add(string.Format("Structure entry '{0}' has wrong number of tokens (expecting 6).", Key));
                        }
                        else
                        {
                            errors.Add(string.Format("Structure '{0}' has wrong number of tokens (expecting 6).", tokens[1]));
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
                            if (buildingType != null)
                            {
                                var cell = int.Parse(tokens[1]);
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
                                errors.Add(string.Format("Base priority {0} references unknown structure '{1}'.", priority, tokens[0]));
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Base priority {0} has wrong number of tokens (expecting 2).", priority));
                        }
                    }
                    else if (!Key.Equals("Count", StringComparison.CurrentCultureIgnoreCase))
                    {
                        errors.Add(string.Format("Invalid base priority '{0}' (expecting integer).", Key));
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
                                    }
                                }
                            }
                            else if (cell != -1)
                            {
                                errors.Add(string.Format("Waypoint {0} out of range (expecting between {1} and {2}).", waypoint, 0, Map.Waypoints.Length - 1));
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Waypoint {0} has invalid cell '{1}' (expecting integer).", waypoint, Value));
                        }
                    }
                    else
                    {
                        errors.Add(string.Format("Invalid waypoint '{0}' (expecting integer).", Key));
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
                            if (checkTrigs.Contains(Value))
                            {
                                if (checkCellTrigs.Contains(Value))
                                {
                                    Map.CellTriggers[cell] = new CellTrigger
                                    {
                                        Trigger = Value
                                    };
                                }
                                else
                                {
                                    errors.Add(string.Format("Cell trigger {0} links to trigger '{1}' which does not contain a placeable event; skipping.", cell, Value));
                                }
                            }
                            else
                            {
                                errors.Add(string.Format("Cell trigger {0} links to unknown trigger '{1}'; skipping.", cell, Value));
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Cell trigger {0} outside map bounds; skipping.", cell));
                        }
                    }
                    else
                    {
                        errors.Add(string.Format("Invalid cell trigger '{0}' (expecting integer).", Key));
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
            foreach (var teamType in Map.TeamTypes)
            {
                teamType.Trigger = indexToName(Map.Triggers, teamType.Trigger, Trigger.None);
            }
            foreach (var trigger in Map.Triggers)
            {
                trigger.Event1.Team = indexToName(Map.TeamTypes, trigger.Event1.Team, TeamType.None);
                trigger.Event2.Team = indexToName(Map.TeamTypes, trigger.Event2.Team, TeamType.None);
                trigger.Action1.Team = indexToName(Map.TeamTypes, trigger.Action1.Team, TeamType.None);
                trigger.Action1.Trigger = indexToName(Map.Triggers, trigger.Action1.Trigger, Trigger.None);
                trigger.Action2.Team = indexToName(Map.TeamTypes, trigger.Action2.Team, TeamType.None);
                trigger.Action2.Trigger = indexToName(Map.Triggers, trigger.Action2.Trigger, Trigger.None);
            }
            UpdateBasePlayerHouse();
            // Sort
            List<Trigger> reordered = Map.Triggers.OrderBy(t => t.Name, new ExplorerComparer()).ToList();
            Map.Triggers.ReplaceRange(reordered);
            extraSections = ini.Sections;
            Map.EndUpdate();
            return errors;
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
                    SaveINI(ini, fileType);
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
                    SaveINI(ini, fileType);
                    using (var iniStream = new MemoryStream())
                    using (var tgaStream = new MemoryStream())
                    using (var jsonStream = new MemoryStream())
                    using (var iniWriter = new StreamWriter(iniStream))
                    using (var jsonWriter = new JsonTextWriter(new StreamWriter(jsonStream)))
                    using (var megafileBuilder = new MegafileBuilder(@"", path))
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

        private void SaveINI(INI ini, FileType fileType)
        {
            if (extraSections != null)
            {
                ini.Sections.AddRange(extraSections);
            }
            Model.BasicSection basic = Map.BasicSection;
            char[] cutfrom = { ';', '(' };
            basic.Intro = GeneralUtils.TrimRemarks(basic.Intro, true, cutfrom);
            basic.Brief = GeneralUtils.TrimRemarks(basic.Brief, true, cutfrom);
            basic.Action = GeneralUtils.TrimRemarks(basic.Action, true, cutfrom);
            basic.Win = GeneralUtils.TrimRemarks(basic.Win, true, cutfrom);
            basic.Win2 = GeneralUtils.TrimRemarks(basic.Win2, true, cutfrom);
            basic.Win3 = GeneralUtils.TrimRemarks(basic.Win3, true, cutfrom);
            basic.Win4 = GeneralUtils.TrimRemarks(basic.Win4, true, cutfrom);
            basic.Lose = GeneralUtils.TrimRemarks(basic.Lose, true, cutfrom);            
            INI.WriteSection(new MapContext(Map, false), ini.Sections.Add("Basic"), Map.BasicSection);
            INI.WriteSection(new MapContext(Map, false), ini.Sections.Add("Map"), Map.MapSection);
            if (fileType != FileType.PGM)
            {
                INI.WriteSection(new MapContext(Map, false), ini.Sections.Add("Steam"), Map.SteamSection);
            }
            ini["Basic"]["NewINIFormat"] = "3";
            var smudgeSection = ini.Sections.Add("SMUDGE");
            foreach (var (cell, smudge) in Map.Smudge.Where(item => (item.Value.Type.Flag & SmudgeTypeFlag.Bib) == SmudgeTypeFlag.None))
            {
                smudgeSection[cell.ToString()] = string.Format("{0},{1},{2}", smudge.Type.Name.ToUpper(), cell, smudge.Icon);
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
                        infantry.Mission,
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
                    unit.Mission,
                    unit.Trigger
                );
            }
            // Classic game does not support this, so I'm leaving this out.
            /*/
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
                    aircraft.Mission
                );
            }
            //*/
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
                    ship.Mission,
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
                            var template = Map.Templates[x, y];
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
                            var template = Map.Templates[x, y];
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
            foreach (var waypoint in Map.Waypoints.Where(w => (w.Flag == WaypointFlag.PlayerStart) && w.Cell.HasValue))
            {
                writer.WriteValue(waypoint.Cell.Value);
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
            int numWaypoints = Map.Waypoints.Count(w => w.Cell.HasValue);
            if (numAircraft > Constants.MaxAircraft)
            {
                sb.Append(Environment.NewLine + string.Format("Maximum number of aircraft exceeded ({0} > {1})", numAircraft, Constants.MaxAircraft));
                ok = false;
            }
            if (numBuildings > Constants.MaxBuildings)
            {
                sb.Append(Environment.NewLine + string.Format("Maximum number of structures exceeded ({0} > {1})", numBuildings, Constants.MaxBuildings));
                ok = false;
            }
            if (numInfantry > Constants.MaxInfantry)
            {
                sb.Append(Environment.NewLine + string.Format("Maximum number of infantry exceeded ({0} > {1})", numInfantry, Constants.MaxInfantry));
                ok = false;
            }
            if (numTerrain > Constants.MaxTerrain)
            {
                sb.Append(Environment.NewLine + string.Format("Maximum number of terrain objects exceeded ({0} > {1})", numTerrain, Constants.MaxTerrain));
                ok = false;
            }
            if (numUnits > Constants.MaxUnits)
            {
                sb.Append(Environment.NewLine + string.Format("Maximum number of units exceeded ({0} > {1})", numUnits, Constants.MaxUnits));
                ok = false;
            }
            if (numVessels > Constants.MaxVessels)
            {
                sb.Append(Environment.NewLine + string.Format("Maximum number of ships exceeded ({0} > {1})", numVessels, Constants.MaxVessels));
                ok = false;
            }
            if (Map.TeamTypes.Count > Constants.MaxTeams)
            {
                sb.Append(Environment.NewLine + string.Format("Maximum number of team types exceeded ({0} > {1})", Map.TeamTypes.Count, Constants.MaxTeams));
                ok = false;
            }
            if (Map.Triggers.Count > Constants.MaxTriggers)
            {
                sb.Append(Environment.NewLine + string.Format("Maximum number of triggers exceeded ({0} > {1})", Map.Triggers.Count, Constants.MaxTriggers));
                ok = false;
            }
            if (!Map.BasicSection.SoloMission && (numWaypoints < 2))
            {
                sb.Append(Environment.NewLine + "Skirmish/Multiplayer maps need at least 2 waypoints for player starting locations.");
                ok = false;
            }
            var homeWaypoint = Map.Waypoints.Where(w => w.Equals("Home")).FirstOrDefault();
            if (Map.BasicSection.SoloMission && !homeWaypoint.Cell.HasValue)
            {
                sb.Append(Environment.NewLine + string.Format("Single-player maps need the Home waypoint to be placed.", Map.Triggers.Count, Constants.MaxTriggers));
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
            var basePlayer = Map.HouseTypes.Where(h => h.Equals(Map.BasicSection.BasePlayer)).FirstOrDefault() ?? Map.HouseTypes.First();
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
