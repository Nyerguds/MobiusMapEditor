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

namespace MobiusEditor.TiberianDawn
{
    public class GamePlugin : IGamePlugin
    {
        private static readonly Regex MovieRegex = new Regex(@"^(.*?\\)*(.*?)\.BK2$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly IEnumerable<ITechnoType> technoTypes;

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

        public bool Dirty { get; set; }

        private INISectionCollection extraSections;

        static GamePlugin()
        {
            technoTypes = InfantryTypes.GetTypes().Cast<ITechnoType>().Concat(UnitTypes.GetTypes(false).Cast<ITechnoType>());
        }

        public GamePlugin(bool mapImage)
        {
            var playerWaypoints = Enumerable.Range(0, 6).Select(i => new Waypoint(string.Format("P{0}", i), WaypointFlag.PlayerStart));
            var generalWaypoints = Enumerable.Range(6, 19).Select(i => new Waypoint(i.ToString()));
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
                        movies.Add(m.Groups[m.Groups.Count - 1].ToString());
                    }
                }
            }
            movies.AddRange(movieTypesAdditional);
            movies.Sort();
            movies = movies.Distinct().ToList();
            movies.Insert(0, "x");
            movieTypes = movies.ToArray();

            var basicSection = new BasicSection();
            basicSection.SetDefault();

            var houseTypes = HouseTypes.GetTypes();
            basicSection.Player = houseTypes.First().Name;

            Map = new Map(basicSection, null, Constants.MaxSize, typeof(House),
                houseTypes, TheaterTypes.GetTypes(), TemplateTypes.GetTypes(), TerrainTypes.GetTypes(),
                OverlayTypes.GetTypes(), SmudgeTypes.GetTypes(), EventTypes.GetTypes(), ActionTypes.GetTypes(),
                MissionTypes.GetTypes(), DirectionTypes.GetTypes(), InfantryTypes.GetTypes(), UnitTypes.GetTypes(true),
                BuildingTypes.GetTypes(), TeamMissionTypes.GetTypes(), TeamMissionTypes.GetTypesInfo(), technoTypes,
                waypoints, movieTypes, themeTypes )
            {
                TiberiumOrGoldValue = 25
            };

            Map.BasicSection.PropertyChanged += BasicSection_PropertyChanged;
            Map.MapSection.PropertyChanged += MapSection_PropertyChanged;

            if (mapImage)
            {
                MapImage = new Bitmap(Map.Metrics.Width * Globals.TileWidth, Map.Metrics.Height * Globals.TileHeight);
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

            Dirty = true;
        }

        public IEnumerable<string> Load(string path, FileType fileType)
        {
            var errors = new List<string>();
            switch (fileType)
            {
                case FileType.INI:
                case FileType.BIN:
                    {
                        var iniPath = Path.ChangeExtension(path, ".ini");
                        var binPath = Path.ChangeExtension(path, ".bin");
                        var ini = new INI();
                        using (var iniReader = new StreamReader(iniPath))
                        using (var binReader = new BinaryReader(new FileStream(binPath, FileMode.Open, FileAccess.Read)))
                        {
                            ini.Parse(iniReader);
                            errors.AddRange(LoadINI(ini));
                            LoadBinary(binReader);
                        }
                    }
                    break;
                case FileType.MEG:
                case FileType.PGM:
                    {
                        using (var megafile = new Megafile(path))
                        {
                            var iniFile = megafile.Where(p => Path.GetExtension(p).ToLower() == ".ini").FirstOrDefault();
                            var binFile = megafile.Where(p => Path.GetExtension(p).ToLower() == ".bin").FirstOrDefault();
                            if ((iniFile != null) && (binFile != null))
                            {
                                var ini = new INI();
                                using (var iniReader = new StreamReader(megafile.Open(iniFile)))
                                using (var binReader = new BinaryReader(megafile.Open(binFile)))
                                {
                                    ini.Parse(iniReader);
                                    errors.AddRange(LoadINI(ini));
                                    LoadBinary(binReader);
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

        private IEnumerable<string> LoadINI(INI ini)
        {
            var errors = new List<string>();

            Map.BeginUpdate();

            var basicSection = ini.Sections.Extract("Basic");
            if (basicSection != null)
            {
                INI.ParseSection(new MapContext(Map, false), basicSection, Map.BasicSection);
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
            Dictionary<string, string> teamMissionTypes = Enumerable.ToDictionary(TeamMissionTypes.GetTypes(), t => t, StringComparer.OrdinalIgnoreCase);
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
                                var type = technoTypes.Where(t => t.Equals(classTokens[0])).FirstOrDefault();
                                byte count;
                                if (!byte.TryParse(classTokens[1], out count))
                                    count = 1;
                                if (type != null)
                                {
                                    teamType.Classes.Add(new TeamTypeClass { Type = type, Count = count });
                                }
                                else
                                {
                                    errors.Add(string.Format("Team '{0}' references unknown class '{1}'", Key, classTokens[0]));
                                }
                            }
                            else
                            {
                                errors.Add(string.Format("Team '{0}' has wrong number of tokens for class index {1} (expecting 2)", Key, i));
                            }
                        }

                        var numMissions = int.Parse(tokens[0]); tokens.RemoveAt(0);
                        for (int i = 0; i < Math.Min(Globals.MaxTeamMissions, numMissions); ++i)
                        {
                            var missionTokens = tokens[0].Split(':'); tokens.RemoveAt(0);
                            if (missionTokens.Length == 2)
                            {
                                string mission;
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
                                    errors.Add(string.Format("Team '{0}' references unknown class '{1}'", Key, missionTokens[0]));
                                }
                            }
                            else
                            {
                                errors.Add(string.Format("Team '{0}' has wrong number of tokens for mission index {1} (expecting 2)", Key, i));
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
                    catch (ArgumentOutOfRangeException) { }
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
                        errors.Add(string.Format("Trigger '{0}' has too few tokens (expecting at least 5)", Key));
                    }
                }
            }

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
                            if (!Map.Technos.Add(cell, new Terrain
                                {
                                    Type = terrainType,
                                    Icon = terrainType.IsTransformable ? 22 : 0,
                                    Trigger = tokens[1]
                                }))
                            {
                                var techno = Map.Technos[cell];
                                if (techno is Building building)
                                {
                                    errors.Add(string.Format("Terrain '{0}' overlaps structure '{1}' in cell {2}, skipping", tokens[0], building.Type.Name, cell));
                                }
                                else if (techno is Overlay overlay)
                                {
                                    errors.Add(string.Format("Terrain '{0}' overlaps overlay '{1}' in cell {2}, skipping", tokens[0], overlay.Type.Name, cell));
                                }
                                else if (techno is Terrain terrain)
                                {
                                    errors.Add(string.Format("Terrain '{0}' overlaps terrain '{1}' in cell {2}, skipping", tokens[0], terrain.Type.Name, cell));
                                }
                                else if (techno is InfantryGroup infantry)
                                {
                                    errors.Add(string.Format("Terrain '{0}' overlaps infantry in cell {1}, skipping", tokens[0], cell));
                                }
                                else if (techno is Unit unit)
                                {
                                    errors.Add(string.Format("Terrain '{0}' overlaps unit '{1}' in cell {2}, skipping", tokens[0], unit.Type.Name, cell));
                                }
                                else
                                {
                                    errors.Add(string.Format("Terrain '{0}' overlaps unknown techno in cell {1}, skipping", tokens[0], cell));
                                }
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Terrain '{0}' references unknown terrain", tokens[0]));
                        }
                    }
                    else
                    {
                        errors.Add(string.Format("Terrain '{0}' has wrong number of tokens (expecting 2)", Key));
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
                        errors.Add(string.Format("Overlay '{0}' references unknown overlay", Value));
                    }
                }
            }

            var smudgeSection = ini.Sections.Extract("Smudge");
            if (smudgeSection != null)
            {
                foreach (var (Key, Value) in smudgeSection)
                {
                    var cell = int.Parse(Key);
                    var tokens = Value.Split(',');
                    if (tokens.Length == 3)
                    {
                        var smudgeType = Map.SmudgeTypes.Where(t => t.Equals(tokens[0]) && (t.Flag & SmudgeTypeFlag.Bib) == 0).FirstOrDefault();
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
                            errors.Add(string.Format("Smudge '{0}' references unknown smudge", tokens[0]));
                        }
                    }
                }
            }

            var infantrySections = ini.Sections.Extract("Infantry");
            if (infantrySections != null)
            {
                foreach (var (_, Value) in infantrySections)
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
                                        infantryGroup.Infantry[stoppingPos] = new Infantry(infantryGroup)
                                        {
                                            Type = infantryType,
                                            House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                                            Strength = int.Parse(tokens[2]),
                                            Direction = Map.DirectionTypes.Where(d => d.Equals(direction)).FirstOrDefault(),
                                            Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5])).FirstOrDefault(),
                                            Trigger = tokens[7]
                                        };
                                    }
                                    else
                                    {
                                        errors.Add(string.Format("Infantry '{0}' overlaps another infantry at position {1} in cell {2}, skipping", tokens[1], stoppingPos, cell));
                                    }
                                }
                                else
                                {
                                    errors.Add(string.Format("Infantry '{0}' has invalid position {1} in cell {2}, skipping", tokens[1], stoppingPos, cell));
                                }
                            }
                            else
                            {
                                var techno = Map.Technos[cell];
                                if (techno is Building building)
                                {
                                    errors.Add(string.Format("Infantry '{0}' overlaps structure '{1}' in cell {2}, skipping", tokens[1], building.Type.Name, cell));
                                }
                                else if (techno is Overlay overlay)
                                {
                                    errors.Add(string.Format("Infantry '{0}' overlaps overlay '{1}' in cell {2}, skipping", tokens[1], overlay.Type.Name, cell));
                                }
                                else if (techno is Terrain terrain)
                                {
                                    errors.Add(string.Format("Infantry '{0}' overlaps terrain '{1}' in cell {2}, skipping", tokens[1], terrain.Type.Name, cell));
                                }
                                else if (techno is Unit unit)
                                {
                                    errors.Add(string.Format("Infantry '{0}' overlaps unit '{1}' in cell {2}, skipping", tokens[1], unit.Type.Name, cell));
                                }
                                else
                                {
                                    errors.Add(string.Format("Infantry '{0}' overlaps unknown techno in cell {1}, skipping", tokens[1], cell));
                                }
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Infantry '{0}' references unknown infantry", tokens[1]));
                        }
                    }
                    else
                    {
                        errors.Add(string.Format("Infantry '{0}' has wrong number of tokens (expecting 8)", tokens[1]));
                    }
                }
            }

            var unitsSections = ini.Sections.Extract("Units");
            if (unitsSections != null)
            {
                foreach (var (_, Value) in unitsSections)
                {
                    var tokens = Value.Split(',');
                    if (tokens.Length == 7)
                    {
                        var unitType = Map.UnitTypes.Where(t => t.IsUnit && t.Equals(tokens[1])).FirstOrDefault();
                        if (unitType != null)
                        {
                            var direction = (byte)((int.Parse(tokens[4]) + 0x08) & ~0x0F);

                            var cell = int.Parse(tokens[3]);
                            if (!Map.Technos.Add(cell, new Unit()
                                {
                                    Type = unitType,
                                    House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                                    Strength = int.Parse(tokens[2]),
                                    Direction = Map.DirectionTypes.Where(d => d.Equals(direction)).FirstOrDefault(),
                                    Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5])).FirstOrDefault(),
                                    Trigger = tokens[6]
                                }))
                            {
                                var techno = Map.Technos[cell];
                                if (techno is Building building)
                                {
                                    errors.Add(string.Format("Unit '{0}' overlaps structure '{1}' in cell {2}, skipping", tokens[1], building.Type.Name, cell));
                                }
                                else if (techno is Overlay overlay)
                                {
                                    errors.Add(string.Format("Unit '{0}' overlaps overlay '{1}' in cell {2}, skipping", tokens[1], overlay.Type.Name, cell));
                                }
                                else if (techno is Terrain terrain)
                                {
                                    errors.Add(string.Format("Unit '{0}' overlaps terrain '{1}' in cell {2}, skipping", tokens[1], terrain.Type.Name, cell));
                                }
                                else if (techno is InfantryGroup infantry)
                                {
                                    errors.Add(string.Format("Unit '{0}' overlaps infantry in cell {1}, skipping", tokens[1], cell));
                                }
                                else if (techno is Unit unit)
                                {
                                    errors.Add(string.Format("Unit '{0}' overlaps unit '{1}' in cell {2}, skipping", tokens[1], unit.Type.Name, cell));
                                }
                                else
                                {
                                    errors.Add(string.Format("Unit '{0}' overlaps unknown techno in cell {1}, skipping", tokens[1], cell));
                                }
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Unit '{0}' references unknown unit", tokens[1]));
                        }
                    }
                    else
                    {
                        errors.Add(string.Format("Unit '{0}' has wrong number of tokens (expecting 7)", tokens[1]));
                    }
                }
            }

            var aircraftSections = ini.Sections.Extract("Aircraft");
            if (aircraftSections != null)
            {
                foreach (var (_, Value) in aircraftSections)
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
                                    Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5])).FirstOrDefault()
                                }))
                            {
                                var techno = Map.Technos[cell];
                                if (techno is Building building)
                                {
                                    errors.Add(string.Format("Aircraft '{0}' overlaps structure '{1}' in cell {2}, skipping", tokens[1], building.Type.Name, cell));
                                }
                                else if (techno is Overlay overlay)
                                {
                                    errors.Add(string.Format("Aircraft '{0}' overlaps overlay '{1}' in cell {2}, skipping", tokens[1], overlay.Type.Name, cell));
                                }
                                else if (techno is Terrain terrain)
                                {
                                    errors.Add(string.Format("Aircraft '{0}' overlaps terrain '{1}' in cell {2}, skipping", tokens[1], terrain.Type.Name, cell));
                                }
                                else if (techno is InfantryGroup infantry)
                                {
                                    errors.Add(string.Format("Aircraft '{0}' overlaps infantry in cell {1}, skipping", tokens[1], cell));
                                }
                                else if (techno is Unit unit)
                                {
                                    errors.Add(string.Format("Aircraft '{0}' overlaps unit '{1}' in cell {2}, skipping", tokens[1], unit.Type.Name, cell));
                                }
                                else
                                {
                                    errors.Add(string.Format("Aircraft '{0}' overlaps unknown techno in cell {1}, skipping", tokens[1], cell));
                                }
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Aircraft '{0}' references unknown aircraft", tokens[1]));
                        }
                    }
                    else
                    {
                        errors.Add(string.Format("Aircraft '{0}' has wrong number of tokens (expecting 6)", tokens[1]));
                    }
                }
            }

            var structuresSection = ini.Sections.Extract("Structures");
            if (structuresSection != null)
            {
                foreach (var (_, Value) in structuresSection)
                {
                    var tokens = Value.Split(',');
                    if (tokens.Length == 6)
                    {
                        var buildingType = Map.BuildingTypes.Where(t => t.Equals(tokens[1])).FirstOrDefault();
                        if (buildingType != null)
                        {
                            var direction = (byte)((int.Parse(tokens[4]) + 0x08) & ~0x0F);

                            var cell = int.Parse(tokens[3]);
                            if (!Map.Buildings.Add(cell, new Building()
                            {
                                Type = buildingType,
                                House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                                Strength = int.Parse(tokens[2]),
                                Direction = Map.DirectionTypes.Where(d => d.Equals(direction)).FirstOrDefault(),
                                Trigger = tokens[5]
                                }))
                            {
                                var techno = Map.Technos[cell];
                                if (techno is Building building)
                                {
                                    errors.Add(string.Format("Structure '{0}' overlaps structure '{1}' in cell {2}, skipping", tokens[1], building.Type.Name, cell));
                                }
                                else if (techno is Overlay overlay)
                                {
                                    errors.Add(string.Format("Structure '{0}' overlaps overlay '{1}' in cell {2}, skipping", tokens[1], overlay.Type.Name, cell));
                                }
                                else if (techno is Terrain terrain)
                                {
                                    errors.Add(string.Format("Structure '{0}' overlaps terrain '{1}' in cell {2}, skipping", tokens[1], terrain.Type.Name, cell));
                                }
                                else if (techno is InfantryGroup infantry)
                                {
                                    errors.Add(string.Format("Structure '{0}' overlaps infantry in cell {1}, skipping", tokens[1], cell));
                                }
                                else if (techno is Unit unit)
                                {
                                    errors.Add(string.Format("Structure '{0}' overlaps unit '{1}' in cell {2}, skipping", tokens[1], unit.Type.Name, cell));
                                }
                                else
                                {
                                    errors.Add(string.Format("Structure '{0}' overlaps unknown techno in cell {1}, skipping", tokens[1], cell));
                                }
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Structure '{0}' references unknown structure", tokens[1]));
                        }
                    }
                    else
                    {
                        errors.Add(string.Format("Structure '{0}' has wrong number of tokens (expecting 6)", tokens[1]));
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
                                        Strength = 256,
                                        Direction = DirectionTypes.North,
                                        BasePriority = priority,
                                        IsPrebuilt = false
                                    });
                                }
                            }
                            else
                            {
                                errors.Add(string.Format("Base priority {0} references unknown structure '{1}'", priority, tokens[0]));
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Base priority {0} has wrong number of tokens (expecting 2)", priority));
                        }
                    }
                    else if (!Key.Equals("Count", StringComparison.CurrentCultureIgnoreCase))
                    {
                        errors.Add(string.Format("Invalid base priority '{0}' (expecting integer)", Key));
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
                                        errors.Add(string.Format("Waypoint {0} cell value {1} out of range (expecting between {2} and {3})", waypoint, cell, 0, Map.Metrics.Length - 1));
                                    }
                                }
                            }
                            else if (cell != -1)
                            {
                                errors.Add(string.Format("Waypoint {0} out of range (expecting between {1} and {2})", waypoint, 0, Map.Waypoints.Length - 1));
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Waypoint {0} has invalid cell '{1}' (expecting integer)", waypoint, Value));
                        }
                    }
                    else
                    {
                        errors.Add(string.Format("Invalid waypoint '{0}' (expecting integer)", Key));
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
                            Map.CellTriggers[cell] = new CellTrigger
                            {
                                Trigger = Value
                            };
                        }
                        else
                        {
                            errors.Add(string.Format("Cell trigger {0} outside map bounds", cell));
                        }
                    }
                    else
                    {
                        errors.Add(string.Format("Invalid cell trigger '{0}' (expecting integer)", Key));
                    }
                }
            }

            Dictionary<string, string> correctedEdges = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var edge in House.GetEdges())
                correctedEdges.Add(edge, edge);
            String defaultEdge = House.GetEdges().FirstOrDefault() ?? String.Empty;
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

            extraSections = ini.Sections;

            Map.EndUpdate();

            return errors;
        }

        private void LoadBinary(BinaryReader reader)
        {
            Map.Templates.Clear();

            for (var y = 0; y < Map.Metrics.Height; ++y)
            {
                for (var x = 0; x < Map.Metrics.Width; ++x)
                {
                    var typeValue = reader.ReadByte();
                    var iconValue = reader.ReadByte();
                    var templateType = Map.TemplateTypes.Where(t => t.Equals(typeValue)).FirstOrDefault();
                    // Prevent loading of illegal tiles.
                    if (templateType != null
                        && (!templateType.Theaters.Contains(Map.Theater)
                            || templateType == TemplateTypes.Clear || iconValue >= templateType.NumIcons
                            || !templateType.IconMask[iconValue % templateType.IconWidth, iconValue / templateType.IconWidth]))
                    {
                        templateType = null;
                    }
                    Map.Templates[x, y] = (templateType != null) ? new Template { Type = templateType, Icon = iconValue } : null;
                }
            }
        }

        public bool Save(string path, FileType fileType)
        {
            if (!Validate())
            {
                return false;
            }

            switch (fileType)
            {
                case FileType.INI:
                case FileType.BIN:
                    {
                        var iniPath = Path.ChangeExtension(path, ".ini");
                        var binPath = Path.ChangeExtension(path, ".bin");
                        var tgaPath = Path.ChangeExtension(path, ".tga");
                        var jsonPath = Path.ChangeExtension(path, ".json");

                        var ini = new INI();
                        SaveINI(ini, fileType);
                        using (var iniWriter = new StreamWriter(iniPath))
                        {
                            iniWriter.Write(ini.ToString());
                        }

                        using (var binStream = new FileStream(binPath, FileMode.Create))
                        using (var binWriter = new BinaryWriter(binStream))
                        {
                            SaveBinary(binWriter);
                        }

                        using (var tgaStream = new FileStream(tgaPath, FileMode.Create))
                        {
                            SaveMapPreview(tgaStream);
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
                        var ini = new INI();
                        SaveINI(ini, fileType);
                        using (var iniStream = new MemoryStream())
                        using (var binStream = new MemoryStream())
                        using (var tgaStream = new MemoryStream())
                        using (var jsonStream = new MemoryStream())
                        using (var binWriter = new BinaryWriter(binStream))
                        using (var jsonWriter = new JsonTextWriter(new StreamWriter(jsonStream)))
                        using (var megafileBuilder = new MegafileBuilder(@"", path))
                        {
                            var iniWriter = new StreamWriter(iniStream);
                            iniWriter.Write(ini.ToString());
                            iniWriter.Flush();
                            iniStream.Position = 0;

                            SaveBinary(binWriter);
                            binWriter.Flush();
                            binStream.Position = 0;

                            SaveMapPreview(tgaStream);
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
                    }
                    break;
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
            BasicSection basic = Map.BasicSection as BasicSection;
            if (basic != null)
            {
                char[] cutfrom = { ';', '(' };
                basic.Intro = INIHelpers.TrimRemarks(basic.Intro, true, cutfrom);
                basic.Brief = INIHelpers.TrimRemarks(basic.Brief, true, cutfrom);
                basic.Action = INIHelpers.TrimRemarks(basic.Action, true, cutfrom);
                basic.Win = INIHelpers.TrimRemarks(basic.Win, true, cutfrom);
                //basic.Win2 = INIHelpers.TrimRemarks(basic.Win2, true, cutfrom);
                //basic.Win3 = INIHelpers.TrimRemarks(basic.Win3, true, cutfrom);
                //basic.Win4 =INIHelpers.TrimRemarks(basic.Win4, true, cutfrom);
                basic.Lose = INIHelpers.TrimRemarks(basic.Lose, true, cutfrom);
            }

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
            foreach(var teamType in Map.TeamTypes)
            {
                var classes = teamType.Classes
                    .Select(c => string.Format("{0}:{1}", c.Type.Name.ToLower(), c.Count))
                    .ToArray();
                var missions = teamType.Missions
                    .Select(m => string.Format("{0}:{1}", m.Mission, m.Argument))
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
            // Game does not actually support reading this.
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
                String overlayName = overlay.Type.Name;
                if (tiberium.IsMatch(overlayName))
                    overlayName = "TI" + rd.Next(1, 13);
                overlaySection[cell.ToString()] = overlayName.ToUpperInvariant();
            }

            var smudgeSection = ini.Sections.Add("Smudge");
            foreach (var (cell, smudge) in Map.Smudge.Where(item => (item.Value.Type.Flag & SmudgeTypeFlag.Bib) == SmudgeTypeFlag.None))
            {
                smudgeSection[cell.ToString()] = string.Format("{0},{1},{2}", smudge.Type.Name, cell, smudge.Icon);
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

        private void SaveMapPreview(Stream stream)
        {
            Map.GenerateMapPreview().Save(stream);
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

        private bool Validate()
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

            if (!ok)
            {
                MessageBox.Show(sb.ToString(), "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return ok;
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

            var basePlayer = Map.HouseTypes.Where(h => h.Equals(Map.BasicSection.BasePlayer)).FirstOrDefault() ?? Map.HouseTypes.First();
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
