﻿//
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

namespace MobiusEditor.TiberianDawn
{
    public class GamePlugin : IGamePlugin
    {
        private static readonly Regex SinglePlayRegex = new Regex("^SC[A-LN-Z]\\d{2}[EWX][A-EL]$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex MovieRegex = new Regex(@"^(?:.*?\\)*(.*?)\.BK2$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly IEnumerable<ITechnoType> fullTechnoTypes;

        private readonly IEnumerable<string> movieTypes;

        private static readonly IEnumerable<string> movieTypesAdditional = new string[]
        {
            "BODYBAGS (Classic only)",
            "REFINT (Classic only)",
            "REFINERY (Classic only)",
            "SIZZLE (Classic only)",
            "SIZZLE2 (Classic only)",
            "TRAILER (Classic only)",
            "TRTKIL_D (Classic only)",
        };

        private static readonly IEnumerable<string> themeTypes = new string[]
        {
            "No Theme",
            "AIRSTRIK",
            "80MX226M",
            "CHRG226M",
            "CREP226M",
            "DRIL226M",
            "DRON226M",
            "FIST226M",
            "RECN226M",
            "VOIC226M",
            "HEAVYG",
            "J1",
            "JDI_V2",
            "RADIO",
            "RAIN",
            "AOI",
            "CCTHANG",
            "DIE",
            "FWP",
            "IND",
            "IND2",
            "JUSTDOIT",
            "LINEFIRE",
            "MARCH",
            "TARGET",
            "NOMERCY",
            "OTP",
            "PRP",
            "ROUT",
            "HEART",
            "STOPTHEM",
            "TROUBLE",
            "WARFARE",
            "BEFEARED",
            "I_AM",
            "WIN1",
            "MAP1",
            "VALKYRIE",
            "NOD_WIN1",
            "NOD_MAP1",
            "OUTTAKES"
        };

        public GameType GameType => GameType.TiberianDawn;

        public Map Map { get; }

        public Image MapImage { get; private set; }

        IFeedBackHandler feedBackHandler;

        bool isDirty;
        public bool Dirty
        {
            get { return isDirty; }
            set { isDirty = value; feedBackHandler?.UpdateStatus(); }
        }

        private INISectionCollection extraSections;

        static GamePlugin()
        {
            fullTechnoTypes = InfantryTypes.GetTypes().Cast<ITechnoType>().Concat(UnitTypes.GetTypes(false).Cast<ITechnoType>());
        }

        public GamePlugin(IFeedBackHandler feedBackHandler)
            : this(true, feedBackHandler)
        {
        }

        public GamePlugin(bool mapImage, IFeedBackHandler feedBackHandler)
        {
            this.feedBackHandler = feedBackHandler;
            const int mplayers = 6;
            var playerWaypoints = Enumerable.Range(0, mplayers).Select(i => new Waypoint(string.Format("P{0}", i), WaypointFlag.PlayerStart));
            var generalWaypoints = Enumerable.Range(mplayers, 25 - mplayers).Select(i => new Waypoint(i.ToString()));
            var specialWaypoints = new Waypoint[] { new Waypoint("Flare"), new Waypoint("Home"), new Waypoint("Reinf.") };
            var waypoints = playerWaypoints.Concat(generalWaypoints).Concat(specialWaypoints);
            var movies = new List<string>();
            using (var megafile = new Megafile(Path.Combine(Globals.MegafilePath, "MOVIES_TD.MEG")))
            {
                foreach (var filename in megafile)
                {
                    var m = MovieRegex.Match(filename);
                    if (m.Success)
                    {
                        movies.Add(m.Groups[1].ToString());
                    }
                }
            }
            movies.AddRange(movieTypesAdditional);
            movies = movies.Distinct().ToList();
            movies.Sort(new ExplorerComparer());
            movies.Insert(0, "x");
            movieTypes = movies.ToArray();
            var basicSection = new BasicSection();
            basicSection.SetDefault();
            var houseTypes = HouseTypes.GetTypes();
            basicSection.Player = houseTypes.First().Name;

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

            Map = new Map(basicSection, null, Constants.MaxSize, typeof(House),
                houseTypes, TheaterTypes.GetTypes(), TemplateTypes.GetTypes(),
                TerrainTypes.GetTypes(), OverlayTypes.GetTypes(), SmudgeTypes.GetTypes(),
                EventTypes.GetTypes(), cellEventTypes, unitEventTypes, structureEventTypes, terrainEventTypes,
                ActionTypes.GetTypes(), cellActionTypes, unitActionTypes, structureActionTypes, terrainActionTypes,
                MissionTypes.GetTypes(), DirectionTypes.GetTypes(), InfantryTypes.GetTypes(), UnitTypes.GetTypes(true),
                BuildingTypes.GetTypes(), TeamMissionTypes.GetTypes(), fullTechnoTypes, waypoints, movieTypes, themeTypes)
            {
                TiberiumOrGoldValue = 25
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
            Map.Theater = Map.TheaterTypes.Where(t => t.Equals(theater)).FirstOrDefault() ?? TheaterTypes.Temperate;
            Map.TopLeft = new Point(1, 1);
            Map.Size = Map.Metrics.Size - new Size(2, 2);
            UpdateBasePlayerHouse();
            //Dirty = true;
        }

        public IEnumerable<string> Load(string path, FileType fileType)
        {
            var ini = new INI();
            var errors = new List<string>();
            var iniPath = fileType == FileType.INI ? path : Path.ChangeExtension(path, ".ini");
            var binPath = fileType == FileType.BIN ? path : Path.ChangeExtension(path, ".bin");
            switch (fileType)
            {
                case FileType.INI:
                case FileType.BIN:
                    using (var iniReader = new StreamReader(iniPath))
                    using (var binReader = new BinaryReader(new FileStream(binPath, FileMode.Open, FileAccess.Read)))
                    {
                        string iniText = FixRoad2Load(iniReader);
                        ini.Parse(iniText);
                        bool forceSingle = SinglePlayRegex.IsMatch(Path.GetFileNameWithoutExtension(path));
                        errors.AddRange(LoadINI(ini, forceSingle));
                        if (binReader.BaseStream.Length != 0x2000)
                        {
                            errors.Add(String.Format("'{0}' does not have the correct size for a Tiberian Dawn .bin file.", Path.GetFileName(binPath)));
                            Map.Templates.Clear();
                        }
                        else
                        {
                            errors.AddRange(LoadBinary(binReader));
                        }
                    }
                    break;
                case FileType.MEG:
                case FileType.PGM:
                    using (var megafile = new Megafile(path))
                    {
                        var iniFile = megafile.Where(p => Path.GetExtension(p).ToLower() == ".ini").FirstOrDefault();
                        var binFile = megafile.Where(p => Path.GetExtension(p).ToLower() == ".bin").FirstOrDefault();
                        if ((iniFile != null) && (binFile != null))
                        {
                            using (var iniReader = new StreamReader(megafile.Open(iniFile)))
                            using (var binReader = new BinaryReader(megafile.Open(binFile)))
                            {
                                ini.Parse(iniReader);
                                errors.AddRange(LoadINI(ini, false));
                                if (binReader.BaseStream.Length != 0x2000)
                                {
                                    errors.Add(String.Format("'{0}' does not have the correct size for a Tiberian Dawn .bin file.", Path.GetFileName(binFile)));
                                    Map.Templates.Clear();
                                }
                                else
                                {
                                    errors.AddRange(LoadBinary(binReader));
                                }
                            }
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
            return errors;
        }

        private string FixRoad2Load(StreamReader iniReader)
        {
            string iniText = iniReader.ReadToEnd();
            string[] iniTextArr = iniText.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            Dictionary<int, int> dupeDetect = new Dictionary<int, int>();
            Dictionary<int, string> cellTypes = new Dictionary<int, string>();
            string roadname = OverlayTypes.Road.Name;
            Regex overlayRegex = new Regex("^\\s*(\\d+)\\s*=\\s*([a-zA-Z0-9]+)\\s*$", RegexOptions.IgnoreCase);
            string road2name = OverlayTypes.Road2.Name.ToUpper();
            string road2dummy = "=" + road2name;
            bool inOverlay = false;
            for (int i = 0; i < iniTextArr.Length; ++i)
            {
                string currLine = iniTextArr[i].Trim();
                if (currLine.StartsWith("["))
                {
                    inOverlay = "[Overlay]".Equals(currLine, StringComparison.InvariantCultureIgnoreCase);
                    continue;
                }
                if (inOverlay)
                {
                    Match match = overlayRegex.Match(currLine);
                    if (match.Success)
                    {
                        int cellNumber = Int32.Parse(match.Groups[1].Value);
                        int cur = dupeDetect.TryGetValue(cellNumber, out int curVal) ? curVal : 0;
                        dupeDetect[cellNumber] = cur + 1;
                        // Only add first detected type, just like the game would.
                        if (cur == 0)
                            cellTypes[cellNumber] = match.Groups[2].Value;
                    }
                }
            }
            // If any of the detected lines have a count of more than one, process the ini.
            // If references to literal ROAD2 are found, also remove them. We do not want it to be accepted as valid type by the editor.
            if (dupeDetect.Any(k => k.Value > 1) || cellTypes.Values.Contains(OverlayTypes.Road2.Name, StringComparer.InvariantCultureIgnoreCase))
            {
                inOverlay = false;
                List<string> newIniText = new List<string>();
                for (int i = 0; i < iniTextArr.Length; ++i)
                {
                    string currLine = iniTextArr[i].Trim();
                    if (currLine.StartsWith("["))
                    {
                        inOverlay = "[Overlay]".Equals(currLine, StringComparison.InvariantCultureIgnoreCase);
                        // No point in detecting anything else off this line, so immediately store and continue.
                        newIniText.Add(currLine);
                        continue;
                    }
                    Match match;
                    if (!inOverlay || !(match = overlayRegex.Match(iniTextArr[i])).Success)
                    {
                        // stuff outside Overlay, and empty lines / comment lines
                        newIniText.Add(currLine);
                    }
                    else
                    {
                        int cellNumber = Int32.Parse(match.Groups[1].Value);
                        string type = cellTypes.TryGetValue(cellNumber, out type) ? type : null;
                        int amount;
                        // Do not allow actual road2 type cells in the ini.
                        if (type != null && !type.Equals(road2name, StringComparison.InvariantCultureIgnoreCase)
                            && dupeDetect.TryGetValue(cellNumber, out amount))
                        {
                            if (amount == 1)
                            {
                                newIniText.Add(currLine);
                            }
                            else if (amount > 1)
                            {
                                if (type.Equals(roadname, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    // Add road2
                                    newIniText.Add(cellNumber + road2dummy);
                                }
                                else
                                {
                                    // Some other cell with duped overlay? Just put it in once as it should be.
                                    newIniText.Add(currLine);
                                }
                                // Ensures TryGetValue succeeds, but nothing is written for any following matches.
                                dupeDetect[cellNumber] = -1;
                            }
                            // Else, write nothing. This will happen to the entries put to -1.
                        }
                    }
                }
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < newIniText.Count; ++i)
                {
                    sb.Append(newIniText[i]).Append("\r\n");
                }
                iniText = sb.ToString();
            }
            return iniText;
        }

        private IEnumerable<string> LoadINI(INI ini, bool forceSoloMission)
        {
            var errors = new List<string>();
            Map.BeginUpdate();
            var basicSection = ini.Sections.Extract("Basic");
            if (basicSection != null)
            {
                INI.ParseSection(new MapContext(Map, false), basicSection, Map.BasicSection);
                char[] cutfrom = { ';', '(' };
                string[] toAddRem = movieTypesAdditional.Select(vid => GeneralUtils.TrimRemarks(vid, true, cutfrom)).ToArray();
                Model.BasicSection basic = Map.BasicSection;
                const string remark = " (Classic only)";
                basic.Intro = GeneralUtils.AddRemarks(basic.Intro, "x", true, toAddRem, remark);
                basic.Brief = GeneralUtils.AddRemarks(basic.Brief, "x", true, toAddRem, remark);
                basic.Action = GeneralUtils.AddRemarks(basic.Action, "x", true, toAddRem, remark);
                basic.Win = GeneralUtils.AddRemarks(basic.Win, "x", true, toAddRem, remark);
                basic.Win2 = GeneralUtils.AddRemarks(basic.Win2, "x", true, toAddRem, remark);
                basic.Win3 = GeneralUtils.AddRemarks(basic.Win3, "x", true, toAddRem, remark);
                basic.Win4 = GeneralUtils.AddRemarks(basic.Win4, "x", true, toAddRem, remark);
                basic.Lose = GeneralUtils.AddRemarks(basic.Lose, "x", true, toAddRem, remark);
            }
            Map.BasicSection.Player = Map.HouseTypes.Where(t => t.Equals(Map.BasicSection.Player)).FirstOrDefault()?.Name ?? Map.HouseTypes.First().Name;
            var mapSection = ini.Sections.Extract("Map");
            if (mapSection != null)
            {
                INI.ParseSection(new MapContext(Map, false), mapSection, Map.MapSection);
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
            var steamSection = ini.Sections.Extract("Steam");
            if (steamSection != null)
            {
                INI.ParseSection(new MapContext(Map, false), steamSection, Map.SteamSection);
            }
            var teamTypesSection = ini.Sections.Extract("TeamTypes");
            // Make case insensitive dictionary of teamtype missions.
            Dictionary<string, TeamMission> teamMissionTypes = Enumerable.ToDictionary(TeamMissionTypes.GetTypes(), t => t.Mission, StringComparer.OrdinalIgnoreCase);
            if (teamTypesSection != null)
            {
                foreach (var (Key, Value) in teamTypesSection)
                {
                    try
                    {
                        var teamType = new TeamType { Name = Key };

                        var tokens = Value.Split(',').ToList();
                        teamType.House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(); tokens.RemoveAt(0);
                        teamType.IsRoundAbout = int.Parse(tokens[0]) != 0; tokens.RemoveAt(0);
                        teamType.IsLearning = int.Parse(tokens[0]) != 0; tokens.RemoveAt(0);
                        teamType.IsSuicide = int.Parse(tokens[0]) != 0; tokens.RemoveAt(0);
                        teamType.IsAutocreate = int.Parse(tokens[0]) != 0; tokens.RemoveAt(0);
                        teamType.IsMercenary = int.Parse(tokens[0]) != 0; tokens.RemoveAt(0);
                        teamType.RecruitPriority = int.Parse(tokens[0]); tokens.RemoveAt(0);
                        teamType.MaxAllowed = byte.Parse(tokens[0]); tokens.RemoveAt(0);
                        teamType.InitNum = byte.Parse(tokens[0]); tokens.RemoveAt(0);
                        teamType.Fear = byte.Parse(tokens[0]); tokens.RemoveAt(0);
                        var numClasses = int.Parse(tokens[0]); tokens.RemoveAt(0);
                        for (int i = 0; i < Math.Min(Globals.MaxTeamClasses, numClasses); ++i)
                        {
                            var classTokens = tokens[0].Split(':'); tokens.RemoveAt(0);
                            if (classTokens.Length == 2)
                            {
                                var type = fullTechnoTypes.Where(t => t.Name.Equals(classTokens[0], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                                byte count;
                                if (!byte.TryParse(classTokens[1], out count))
                                    count = 1;
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
                                TeamMission mission;
                                // fix mission case sensitivity issues.
                                teamMissionTypes.TryGetValue(missionTokens[0], out mission);
                                byte count;
                                byte.TryParse(missionTokens[1], out count);
                                if (mission != null)
                                {
                                    teamType.Missions.Add(new TeamTypeMission { Mission = mission, Argument = count });
                                }
                                else
                                {
                                    errors.Add(string.Format("Team '{0}' references unknown class '{1}'.", Key, missionTokens[0]));
                                }
                            }
                            else
                            {
                                errors.Add(string.Format("Team '{0}' has wrong number of tokens for mission index {1} (expecting 2).", Key, i));
                            }
                        }
                        if (tokens.Count > 0)
                        {
                            teamType.IsReinforcable = int.Parse(tokens[0]) != 0; tokens.RemoveAt(0);
                        }
                        if (tokens.Count > 0)
                        {
                            teamType.IsPrebuilt = int.Parse(tokens[0]) != 0; tokens.RemoveAt(0);
                        }
                        Map.TeamTypes.Add(teamType);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        errors.Add(string.Format("Teamtype '{0}' has errors and can't be parsed.", Key));
                    }
                }
            }
            var triggersSection = ini.Sections.Extract("Triggers");
            if (triggersSection != null)
            {
                foreach (var (Key, Value) in triggersSection)
                {
                    var tokens = Value.Split(',');
                    if (tokens.Length >= 5)
                    {
                        var trigger = new Trigger { Name = Key };

                        trigger.Event1.EventType = tokens[0];
                        trigger.Event1.Data = long.Parse(tokens[2]);
                        trigger.Action1.ActionType = tokens[1];
                        trigger.House = Map.HouseTypes.Where(t => t.Equals(tokens[3])).FirstOrDefault()?.Name ?? "None";
                        if (String.IsNullOrEmpty(tokens[4]))
                            tokens[4] = TeamType.None;
                        trigger.Action1.Team = tokens[4];
                        trigger.PersistentType = TriggerPersistentType.Volatile;
                        if (tokens.Length >= 6)
                        {
                            trigger.PersistentType = (TriggerPersistentType)int.Parse(tokens[5]);
                        }
                        Map.Triggers.Add(trigger);
                    }
                    else
                    {
                        errors.Add(string.Format("Trigger '{0}' has too few tokens (expecting at least 5).", Key));
                    }
                }
            }
            HashSet<string> checkTrigs = Trigger.None.Yield().Concat(Map.Triggers.Select(t => t.Name)).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            HashSet<string> checkCellTrigs = Map.FilterCellTriggers().Select(t => t.Name).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            HashSet<string> checkUnitTrigs = Trigger.None.Yield().Concat(Map.FilterUnitTriggers().Select(t => t.Name)).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            HashSet<string> checkStrcTrigs = Trigger.None.Yield().Concat(Map.FilterStructureTriggers().Select(t => t.Name)).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            HashSet<string> checkTerrTrigs = Trigger.None.Yield().Concat(Map.FilterTerrainTriggers().Select(t => t.Name)).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            var terrainSection = ini.Sections.Extract("Terrain");
            if (terrainSection != null)
            {
                foreach (var (Key, Value) in terrainSection)
                {
                    var cell = int.Parse(Key);
                    var tokens = Value.Split(',');
                    if (tokens.Length == 2)
                    {
                        var terrainType = Map.TerrainTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault();
                        if (terrainType != null)
                        {
                            Terrain newTerr = new Terrain
                            {
                                Type = terrainType,
                                Icon = terrainType.DisplayIcon,
                                Trigger = tokens[1]
                            };
                            if (Map.Technos.Add(cell, newTerr))
                            {
                                if (!checkTrigs.Contains(newTerr.Trigger))
                                {
                                    errors.Add(string.Format("Terrain '{0}' links to unknown trigger '{1}'; clearing trigger.", terrainType, tokens[1]));
                                    newTerr.Trigger = Trigger.None;
                                }
                                else if (!checkTerrTrigs.Contains(newTerr.Trigger))
                                {
                                    errors.Add(string.Format("Terrain '{0}' links to trigger '{1}' which does not contain an event applicable to terrain; clearing trigger.", terrainType, tokens[1]));
                                    newTerr.Trigger = Trigger.None;
                                }
                            }
                            else
                            {
                                var techno = Map.FindBlockingObject(cell, terrainType, out int blockingCell);
                                int reportCell = blockingCell == -1 ? cell : blockingCell;
                                if (techno is Building building)
                                {
                                    errors.Add(string.Format("Terrain '{0}' overlaps structure '{1}' in cell {2}; skipping.", tokens[0], building.Type.Name, blockingCell));
                                }
                                else if (techno is Overlay overlay)
                                {
                                    errors.Add(string.Format("Terrain '{0}' overlaps overlay '{1}' in cell {2}; skipping.", tokens[0], overlay.Type.Name, blockingCell));
                                }
                                else if (techno is Terrain terrain)
                                {
                                    errors.Add(string.Format("Terrain '{0}' overlaps terrain '{1}' in cell {2}; skipping.", tokens[0], terrain.Type.Name, blockingCell));
                                }
                                else if (techno is InfantryGroup infantry)
                                {
                                    errors.Add(string.Format("Terrain '{0}' overlaps infantry in cell {1}; skipping.", tokens[0], blockingCell));
                                }
                                else if (techno is Unit unit)
                                {
                                    errors.Add(string.Format("Terrain '{0}' overlaps unit '{1}' in cell {2}; skipping.", tokens[0], unit.Type.Name, blockingCell));
                                }
                                else
                                {
                                    errors.Add(string.Format("Terrain '{0}' overlaps unknown techno in cell {1}; skipping.", tokens[0], blockingCell));
                                }
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Terrain '{0}' references unknown terrain.", tokens[0]));
                        }
                    }
                    else
                    {
                        errors.Add(string.Format("Terrain '{0}' has wrong number of tokens (expecting 2).", Key));
                    }
                }
            }
            var overlaySection = ini.Sections.Extract("Overlay");
            if (overlaySection != null)
            {
                foreach (var (Key, Value) in overlaySection)
                {
                    var cell = int.Parse(Key);
                    var overlayType = Map.OverlayTypes.Where(t => t.Equals(Value)).FirstOrDefault();
                    if (overlayType != null)
                    {
                        Map.Overlay[cell] = new Overlay { Type = overlayType, Icon = 0 };
                    }
                    else
                    {
                        errors.Add(string.Format("Overlay '{0}' references unknown overlay.", Value));
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
                                            errors.Add(string.Format("Infantry '{0}' links to unknown trigger '{1}'; clearing trigger.", infantryType.Name, tokens[7]));
                                            tokens[7] = Trigger.None;
                                        }
                                        else if (!checkUnitTrigs.Contains(tokens[7]))
                                        {
                                            errors.Add(string.Format("Infantry '{0}' links to trigger '{1}' which does not contain an event applicable to infantry; clearing trigger.", infantryType.Name, tokens[7]));
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
                                    errors.Add(string.Format("Unit '{0}' links to unknown trigger '{1}'; clearing trigger.", unitType.Name, newUnit.Trigger));
                                    newUnit.Trigger = Trigger.None;
                                }
                                else if (!checkUnitTrigs.Contains(tokens[6]))
                                {
                                    errors.Add(string.Format("Unit '{0}' links to trigger '{1}' which does not contain an event applicable to units; clearing trigger.", unitType.Name, newUnit.Trigger));
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
            var structuresSection = ini.Sections.Extract("Structures");
            if (structuresSection != null)
            {
                foreach (var (Key, Value) in structuresSection)
                {
                    var tokens = Value.Split(',');
                    if (tokens.Length == 6)
                    {
                        var buildingType = Map.BuildingTypes.Where(t => t.Equals(tokens[1])).FirstOrDefault();
                        if (buildingType != null)
                        {
                            var direction = (byte)((int.Parse(tokens[4]) + 0x08) & ~0x0F);
                            var cell = int.Parse(tokens[3]);
                            Building newBld = new Building()
                            {
                                Type = buildingType,
                                House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                                Strength = int.Parse(tokens[2]),
                                Direction = Map.DirectionTypes.Where(d => d.Equals(direction)).FirstOrDefault(),
                                Trigger = tokens[5]
                            };
                            if (Map.Buildings.Add(cell, newBld))
                            {
                                if (!checkTrigs.Contains(tokens[5]))
                                {
                                    errors.Add(string.Format("Structure '{0}' links to unknown trigger '{1}'; clearing trigger.", buildingType.Name, tokens[5]));
                                    newBld.Trigger = Trigger.None;
                                }
                                else if (!checkStrcTrigs.Contains(tokens[5]))
                                {
                                    errors.Add(string.Format("Structure '{0}' links to trigger '{1}' which does not contain an event applicable to structures; clearing trigger.", buildingType.Name, tokens[5]));
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
                    if (int.TryParse(Key, out int priority))
                    {
                        var tokens = Value.Split(',');
                        if (tokens.Length == 2)
                        {
                            var buildingType = Map.BuildingTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault();
                            if (buildingType != null)
                            {
                                var coord = int.Parse(tokens[1]);
                                var location = new Point((coord >> 8) & 0x3F, (coord >> 24) & 0x3F);
                                if (Map.Buildings.OfType<Building>().Where(x => x.Location == location).FirstOrDefault().Occupier is Building building)
                                {
                                    building.BasePriority = priority;
                                }
                                else
                                {
                                    Map.Buildings.Add(location, new Building()
                                    {
                                        Type = buildingType,
                                        House = HouseTypes.None,
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
                            errors.Add(string.Format("Cell trigger {0} is outside map bounds; skipping.", cell));
                        }
                    }
                    else
                    {
                        errors.Add(string.Format("Invalid cell trigger '{0}' (expecting integer).", Key));
                    }
                }
            }
            Dictionary<string, string> correctedEdges = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var edge in Globals.Edges)
                correctedEdges.Add(edge, edge);
            string defaultEdge = Globals.Edges.FirstOrDefault() ?? string.Empty;
            foreach (var house in Map.Houses)
            {
                if (house.Type.ID < 0)
                {
                    continue;
                }
                var houseSection = ini.Sections.Extract(house.Type.Name);
                if (houseSection != null)
                {
                    INI.ParseSection(new MapContext(Map, false), houseSection, house);
                    house.Enabled = true;
                    string correctedEdge;
                    if (!correctedEdges.TryGetValue(house.Edge, out correctedEdge))
                        correctedEdge = defaultEdge;
                    house.Edge = correctedEdge;
                }
                else
                {
                    house.Enabled = false;
                }
            }
            UpdateBasePlayerHouse();
            // Sort
            var comparer = new ExplorerComparer();
            List<Trigger> reorderedTriggers = Map.Triggers.OrderBy(t => t.Name, comparer).ToList();
            Map.Triggers.ReplaceRange(reorderedTriggers);
            Map.TeamTypes.Sort((x, y) => comparer.Compare(x.Name, y.Name));
            extraSections = ini.Sections;
            bool switchedToSolo = forceSoloMission && !Map.BasicSection.SoloMission
                && reorderedTriggers.Any(t => t.Action1.ActionType == ActionTypes.ACTION_WIN)
                && reorderedTriggers.Any(t => t.Action1.ActionType == ActionTypes.ACTION_LOSE);
            if (switchedToSolo)
            {
                errors.Insert(0, "Filename detected as classic single player mission format, and win and lose trigger detected. Applying \"SoloMission\" flag.");
                Map.BasicSection.SoloMission = true;
            }
            Map.EndUpdate();
            return errors;
        }

        private IEnumerable<string> LoadBinary(BinaryReader reader)
        {
            var errors = new List<string>();
            Map.Templates.Clear();
            for (var y = 0; y < Map.Metrics.Height; ++y)
            {
                for (var x = 0; x < Map.Metrics.Width; ++x)
                {
                    var typeValue = reader.ReadByte();
                    var iconValue = reader.ReadByte();
                    var templateType = Map.TemplateTypes.Where(t => t.Equals(typeValue)).FirstOrDefault();
                    // Prevent loading of illegal tiles.
                    if (templateType != null)
                    {
                        bool isRandom = (templateType.Flag & TemplateTypeFlag.RandomCell) != TemplateTypeFlag.None;
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
                        else if (iconValue >= templateType.NumIcons)
                        {
                            errors.Add(String.Format("Template '{0}' at cell [{1},{2}] has an icon set ({3}) that is outside its icons range; clearing.", templateType.Name.ToUpper(), x, y, iconValue));
                            templateType = null;
                        }
                        else if (!isRandom && !templateType.IconMask[iconValue % templateType.IconWidth, iconValue / templateType.IconWidth])
                        {
                            errors.Add(String.Format("Template '{0}' at cell [{1},{2}] has an icon set ({3}) that is not part of its placeable cells; clearing.", templateType.Name.ToUpper(), x, y, iconValue));
                            templateType = null;
                        }
                    }
                    else if (typeValue != 0xFF)
                    {
                        errors.Add(String.Format("Unknown template value {0:X2} at cell [{1},{2}]; clearing.", typeValue, x, y));
                    }
                    Map.Templates[x, y] = (templateType != null) ? new Template { Type = templateType, Icon = iconValue } : null;
                }
            }
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
            var iniPath = fileType == FileType.INI ? path : Path.ChangeExtension(path, ".ini");
            var binPath = fileType == FileType.BIN ? path : Path.ChangeExtension(path, ".bin");
            var ini = new INI();
            switch (fileType)
            {
                case FileType.INI:
                case FileType.BIN:
                    SaveINI(ini, fileType);
                    using (var iniWriter = new StreamWriter(iniPath))
                    {
                        //iniWriter.Write(ini.ToString());
                        FixRoad2Save(ini, iniWriter);
                    }
                    using (var binStream = new FileStream(binPath, FileMode.Create))
                    using (var binWriter = new BinaryWriter(binStream))
                    {
                        SaveBinary(binWriter);
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
                    SaveINI(ini, fileType);
                    using (var iniStream = new MemoryStream())
                    using (var binStream = new MemoryStream())
                    using (var tgaStream = new MemoryStream())
                    using (var jsonStream = new MemoryStream())
                    using (var iniWriter = new StreamWriter(iniStream))
                    using (var binWriter = new BinaryWriter(binStream))
                    using (var jsonWriter = new JsonTextWriter(new StreamWriter(jsonStream)))
                    using (var megafileBuilder = new MegafileBuilder(@"", path))
                    {
                        FixRoad2Save(ini, iniWriter);
                        iniWriter.Flush();
                        iniStream.Position = 0;
                        SaveBinary(binWriter);
                        binWriter.Flush();
                        binStream.Position = 0;
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
                        var iniFile = Path.ChangeExtension(Path.GetFileName(path), ".ini").ToUpper();
                        var binFile = Path.ChangeExtension(Path.GetFileName(path), ".bin").ToUpper();
                        var tgaFile = Path.ChangeExtension(Path.GetFileName(path), ".tga").ToUpper();
                        var jsonFile = Path.ChangeExtension(Path.GetFileName(path), ".json").ToUpper();
                        megafileBuilder.AddFile(iniFile, iniStream);
                        megafileBuilder.AddFile(binFile, binStream);
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

        private void FixRoad2Save(INI ini, StreamWriter iniWriter)
        {
            // Special code to make the second state of ROAD cells work.
            string roadLine = "=" + OverlayTypes.Road.Name.ToUpperInvariant() + "\r\n";
            Regex roadDetect = new Regex("^\\s*(\\d+)\\s*=\\s*" + OverlayTypes.Road2.Name + "\\s*$", RegexOptions.IgnoreCase);
            string[] iniString = ini.ToString().Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            bool inOverlay = false;
            for (int i = 0; i < iniString.Length; i++)
            {
                string currLine = iniString[i].Trim();
                if (currLine.StartsWith("["))
                {
                    inOverlay = "[Overlay]".Equals(currLine, StringComparison.InvariantCultureIgnoreCase);
                }
                Match match;
                if (inOverlay && (match = roadDetect.Match(currLine)).Success)
                {
                    string newRoad = match.Groups[1].Value + roadLine;
                    // Write twice to achieve second state.
                    iniWriter.Write(newRoad);
                    iniWriter.Write(newRoad);
                }
                else
                {
                    iniWriter.Write(currLine);
                    iniWriter.Write("\r\n");
                }
            }
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
            ini.Sections.Remove("Briefing");
            if (!string.IsNullOrEmpty(Map.BriefingSection.Briefing))
            {
                var briefingSection = ini.Sections.Add("Briefing");
                briefingSection["Text"] = Map.BriefingSection.Briefing.Replace(Environment.NewLine, "@");
            }
            var cellTriggersSection = ini.Sections.Add("CellTriggers");
            foreach (var (cell, cellTrigger) in Map.CellTriggers)
            {
                cellTriggersSection[cell.ToString()] = cellTrigger.Trigger;
            }
            var teamTypesSection = ini.Sections.Add("TeamTypes");
            foreach (var teamType in Map.TeamTypes)
            {
                var classes = teamType.Classes
                    .Select(c => string.Format("{0}:{1}", c.Type.Name.ToUpperInvariant(), c.Count))
                    .ToArray();
                var missions = teamType.Missions
                    .Select(m => string.Format("{0}:{1}", m.Mission.Mission, m.Argument))
                    .ToArray();
                var tokens = new List<string>
                {
                    teamType.House.Name,
                    teamType.IsRoundAbout ? "1" : "0",
                    teamType.IsLearning ? "1" : "0",
                    teamType.IsSuicide ? "1" : "0",
                    teamType.IsAutocreate ? "1" : "0",
                    teamType.IsMercenary ? "1" : "0",
                    teamType.RecruitPriority.ToString(),
                    teamType.MaxAllowed.ToString(),
                    teamType.InitNum.ToString(),
                    teamType.Fear.ToString(),
                    classes.Length.ToString(),
                    string.Join(",", classes),
                    missions.Length.ToString(),
                    string.Join(",", missions),
                    teamType.IsReinforcable ? "1" : "0",
                    teamType.IsPrebuilt ? "1" : "0"
                };
                teamTypesSection[teamType.Name] = string.Join(",", tokens.Where(t => !string.IsNullOrEmpty(t)));
            }

            var triggersSection = ini.Sections.Add("Triggers");
            foreach (var trigger in Map.Triggers)
            {
                if (string.IsNullOrEmpty(trigger.Name))
                {
                    continue;
                }
                var tokens = new List<string>
                {
                    trigger.Event1.EventType,
                    trigger.Action1.ActionType,
                    trigger.Event1.Data.ToString(),
                    String.IsNullOrEmpty(trigger.House) ? House.None : trigger.House,
                    String.IsNullOrEmpty(trigger.Action1.Team) ? TeamType.None : trigger.Action1.Team,
                    ((int)trigger.PersistentType).ToString()
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
            var baseSection = ini.Sections.Add("Base");
            var baseBuildings = Map.Buildings.OfType<Building>().Where(x => x.Occupier.BasePriority >= 0).OrderByDescending(x => x.Occupier.BasePriority).ToArray();
            var baseIndex = baseBuildings.Length - 1;
            foreach (var (location, building) in baseBuildings)
            {
                var key = baseIndex.ToString("D3");
                baseIndex--;

                baseSection[key] = string.Format("{0},{1}",
                    building.Type.Name.ToUpper(),
                    ((location.Y & 0x3F) << 24) | ((location.X & 0x3F) << 8)
                );
            }
            baseSection["Count"] = baseBuildings.Length.ToString();

            var infantrySection = ini.Sections.Add("Infantry");
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
            var structuresSection = ini.Sections.Add("Structures");
            var structureIndex = 0;
            foreach (var (location, building) in Map.Buildings.OfType<Building>().Where(x => x.Occupier.IsPrebuilt))
            {
                var key = structureIndex.ToString("D3");
                structureIndex++;

                Map.Metrics.GetCell(location, out int cell);
                structuresSection[key] = string.Format("{0},{1},{2},{3},{4},{5}",
                    building.House.Name,
                    building.Type.Name,
                    building.Strength,
                    cell,
                    building.Direction.ID,
                    building.Trigger
                );
            }
            var unitsSection = ini.Sections.Add("Units");
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
            // Classic game does not support this, so I'm leaving this out. It's buggy anyway.
            /*/
            var aircraftSection = ini.Sections.Add("Aircraft");
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
            //*/
            foreach (var house in Map.Houses)
            {
                if ((house.Type.ID < 0) || !house.Enabled)
                {
                    continue;
                }
                INI.WriteSection(new MapContext(Map, false), ini.Sections.Add(house.Type.Name), house);
            }
            var overlaySection = ini.Sections.Add("Overlay");
            Regex tiberium = new Regex("TI([0-9]|(1[0-2]))", RegexOptions.IgnoreCase);
            Random rd = new Random();
            foreach (var (cell, overlay) in Map.Overlay)
            {
                string overlayName = overlay.Type.Name;
                if (tiberium.IsMatch(overlayName))
                    overlayName = "TI" + rd.Next(1, 13);
                overlaySection[cell.ToString()] = overlayName.ToUpperInvariant();
            }
            var smudgeSection = ini.Sections.Add("Smudge");
            foreach (var (cell, smudge) in Map.Smudge.Where(item => (item.Value.Type.Flag & SmudgeTypeFlag.Bib) == SmudgeTypeFlag.None))
            {
                smudgeSection[cell.ToString()] = string.Format("{0},{1},{2}", smudge.Type.Name.ToUpper(), cell, smudge.Icon);
            }
            var terrainSection = ini.Sections.Add("Terrain");
            foreach (var (location, terrain) in Map.Technos.OfType<Terrain>())
            {
                Map.Metrics.GetCell(location, out int cell);
                terrainSection[cell.ToString()] = string.Format("{0},{1}", terrain.Type.Name, terrain.Trigger);
            }
        }

        private void SaveBinary(BinaryWriter writer)
        {
            for (var y = 0; y < Map.Metrics.Height; ++y)
            {
                for (var x = 0; x < Map.Metrics.Width; ++x)
                {
                    var template = Map.Templates[x, y];
                    if (template != null && (template.Type.Flag & TemplateTypeFlag.Clear) == 0)
                    {
                        writer.Write((byte)template.Type.ID);
                        writer.Write((byte)template.Icon);
                    }
                    else
                    {
                        writer.Write(byte.MaxValue);
                        writer.Write((byte)0);
                    }
                }
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

        public String Validate()
        {
            StringBuilder sb = new StringBuilder("Error(s) during map validation:");
            bool ok = true;
            int numAircraft = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsAircraft).Count();
            int numBuildings = Map.Buildings.OfType<Building>().Where(x => x.Occupier.IsPrebuilt).Count();
            int numInfantry = Map.Technos.OfType<InfantryGroup>().Sum(item => item.Occupier.Infantry.Count(i => i != null));
            int numTerrain = Map.Technos.OfType<Terrain>().Count();
            int numUnits = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsUnit).Count();
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
                case "Player":
                    {
                        UpdateBasePlayerHouse();
                    }
                    break;
            }
        }

        private void MapSection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Theater":
                    {
                        Map.InitTheater(GameType);
                    }
                    break;
            }
        }

        private void UpdateBasePlayerHouse()
        {
            Map.BasicSection.BasePlayer = HouseTypes.GetBasePlayer(Map.BasicSection.Player);
            var basePlayer = Map.HouseTypesIncludingNone.Where(h => h.Equals(Map.BasicSection.BasePlayer)).FirstOrDefault() ?? Map.HouseTypes.First();
            foreach (var (_, building) in Map.Buildings.OfType<Building>())
            {
                if (!building.IsPrebuilt)
                {
                    building.House = basePlayer;
                }
            }
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
