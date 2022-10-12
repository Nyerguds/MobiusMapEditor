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
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TGASharpLib;

namespace MobiusEditor.TiberianDawn
{
    public class GamePlugin : IGamePlugin
    {
        protected bool isMegaMap = false;

        protected  const int multiStartPoints = 8;
        protected  static readonly Regex SinglePlayRegex = new Regex("^SC[A-LN-Z]\\d{2}[EWX][A-EL]$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected static readonly Regex MovieRegex = new Regex(@"^(?:.*?\\)*(.*?)\.BK2$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected static readonly IEnumerable<ITechnoType> fullTechnoTypes;

        protected const string defVidVal = "x";
        protected readonly IEnumerable<string> movieTypes;

        protected static readonly IEnumerable<string> movieTypesAdditional = new string[]
        {
            "BODYBAGS (Classic only)",
            "REFINT (Classic only)",
            "REFINERY (Classic only)",
            "SIZZLE (Classic only)",
            "SIZZLE2 (Classic only)",
            "TRAILER (Classic only)",
            "TRTKIL_D (Classic only)",
        };

        protected static readonly IEnumerable<string> themeTypes = new string[]
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
        public virtual string Name => "Tiberian Dawn";

        public virtual GameType GameType => GameType.TiberianDawn;

        public virtual bool IsMegaMap => isMegaMap;

        public Map Map { get; protected set; }

        public Image MapImage { get; protected set; }

        protected IFeedBackHandler feedBackHandler;
        public IFeedBackHandler FeedBackHandler
        {
            get { return feedBackHandler; }
            set { feedBackHandler = value; }
        }

        bool isDirty;
        public bool Dirty
        {
            get { return isDirty; }
            set { isDirty = value; feedBackHandler?.UpdateStatus(); }
        }

        protected INISectionCollection extraSections;
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
            set {
                INI ini = new INI();
                try
                {
                    ini.Parse(value);
                }
                catch
                {
                    return;
                }
                // Remove any sections known and handled / disallowed by the editor.
                ini.Sections.Remove("Basic");
                ini.Sections.Remove("Map");
                ini.Sections.Remove("Briefing");
                ini.Sections.Remove("Steam");
                ini.Sections.Remove("TeamTypes");
                ini.Sections.Remove("Triggers");
                ini.Sections.Remove("Terrain");
                ini.Sections.Remove("Overlay");
                ini.Sections.Remove("Smudge");
                ini.Sections.Remove("Infantry");
                ini.Sections.Remove("Units");
                ini.Sections.Remove("Aircraft");
                ini.Sections.Remove("Structures");
                ini.Sections.Remove("Base");
                ini.Sections.Remove("Waypoints");
                ini.Sections.Remove("CellTriggers");
                foreach (var house in Map.Houses)
                {
                    ini.Sections.Remove(house.Type.Name);
                }
                extraSections = ini.Sections.Count == 0 ? null : ini.Sections;
            }
        }

        public static bool CheckForMegamap(INI iniContents)
        {
            return GeneralUtils.CheckForIniInfo(iniContents, "Map", "Version", "1");
        }

        static GamePlugin()
        {
            fullTechnoTypes = InfantryTypes.GetTypes().Cast<ITechnoType>().Concat(UnitTypes.GetTypes(false).Cast<ITechnoType>());
        }

        protected GamePlugin()
        {
            // Readonly, so I'm splitting this off
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
            movies.Insert(0, defVidVal);
            movieTypes = movies.ToArray();
        }

        public GamePlugin(bool megaMap)
            : this(true, megaMap)
        {
        }

        public GamePlugin(bool mapImage, bool megaMap)
            :this()
        {
            this.isMegaMap = megaMap;
            var playerWaypoints = Enumerable.Range(0, multiStartPoints).Select(i => new Waypoint(string.Format("P{0}", i), Waypoint.GetFlagForMpId(i)));
            var generalWaypoints = Enumerable.Range(multiStartPoints, 25 - multiStartPoints).Select(i => new Waypoint(i.ToString()));
            var specialWaypoints = new Waypoint[] { new Waypoint("Flare", WaypointFlag.Flare), new Waypoint("Home", WaypointFlag.Home), new Waypoint("Reinf.", WaypointFlag.Reinforce) };
            Waypoint[] waypoints = playerWaypoints.Concat(generalWaypoints).Concat(specialWaypoints).ToArray();
            var basicSection = new BasicSection();
            basicSection.SetDefault();
            var houseTypes = HouseTypes.GetTypes();
            basicSection.Player = houseTypes.Where(h => h.ID > -1).First().Name;
            basicSection.BasePlayer = HouseTypes.GetBasePlayer(basicSection.Player);
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
            // Metallic light blue
            flagColors[6] = Globals.TheTeamColorManager["BAD_UNIT"];
            // Purple
            flagColors[7] = new TeamColor(Globals.TheTeamColorManager, flagColors[0], "MULTI8", new Vector3(0.410f, 0.100f, 0.000f));


            Size mapSize = !megaMap ? Constants.MaxSize : Constants.MaxSizeMega;
            Map = new Map(basicSection, null, mapSize, typeof(House), houseTypes,
                flagColors, TheaterTypes.GetTypes(), TemplateTypes.GetTypes(),
                TerrainTypes.GetTypes(), OverlayTypes.GetTypes(), SmudgeTypes.GetTypes(Globals.ConvertCraters),
                EventTypes.GetTypes(), cellEventTypes, unitEventTypes, structureEventTypes, terrainEventTypes,
                ActionTypes.GetTypes(), cellActionTypes, unitActionTypes, structureActionTypes, terrainActionTypes,
                MissionTypes.GetTypes(), DirectionTypes.GetMainTypes(), DirectionTypes.GetAllTypes(), InfantryTypes.GetTypes(), UnitTypes.GetTypes(Globals.DisableAirUnits),
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
        }

        public virtual IEnumerable<string> Load(string path, FileType fileType)
        {
            return Load(path, fileType, false);
        }

        protected List<string> Load(string path, FileType fileType, bool forSole)
        {
            var ini = new INI();
            var errors = new List<string>();
            bool modified = false;
            bool forceSingle = false;
            switch (fileType)
            {
                case FileType.INI:
                case FileType.BIN:
                    var iniPath = fileType == FileType.INI ? path : Path.ChangeExtension(path, ".ini");
                    var binPath = fileType == FileType.BIN ? path : Path.ChangeExtension(path, ".bin");
                    if (!File.Exists(iniPath))
                    {
                        // Should never happen; this gets filtered out in the game type detection.
                        throw new ApplicationException("Cannot find an ini file to load for " + Path.GetFileName(path) + ".");
                    }
                    using (var iniReader = new StreamReader(iniPath))
                    {
                        string iniText = iniReader.ReadToEnd();
                        if (!forSole)
                        {
                            iniText = FixRoad2Load(iniText);
                        }
                        ini.Parse(iniText);
                        forceSingle = !forSole && SinglePlayRegex.IsMatch(Path.GetFileNameWithoutExtension(path));
                        errors.AddRange(LoadINI(ini, forceSingle, ref modified));
                    }
                    if (!File.Exists(binPath))
                    {
                        errors.Add(String.Format("No .bin file found for file '{0}'. Using empty map.", Path.GetFileName(path)));
                        modified = true;
                        Map.Templates.Clear();
                    }
                    else
                    {
                        using (var binReader = new BinaryReader(new FileStream(binPath, FileMode.Open, FileAccess.Read)))
                        {
                            long mapLen = binReader.BaseStream.Length;
                            if ((!isMegaMap && mapLen == 0x2000) || (isMegaMap && mapLen % 4 == 0))
                            {
                                errors.AddRange(!isMegaMap ? LoadBinaryClassic(binReader, ref modified) : LoadBinaryMega(binReader, ref modified));
                            }
                            else
                            {
                                errors.Add(String.Format("'{0}' does not have the correct size for a " + this.Name + " .bin file.", Path.GetFileName(binPath)));
                                modified = true;
                                Map.Templates.Clear();
                            }
                        }
                    }
                    break;
                case FileType.MEG:
                case FileType.PGM:
                    using (var megafile = new Megafile(path))
                    {
                        var iniFile = megafile.Where(p => Path.GetExtension(p).ToLower() == ".ini").FirstOrDefault();
                        var binFile = megafile.Where(p => Path.GetExtension(p).ToLower() == ".bin").FirstOrDefault();
                        if (iniFile == null || binFile == null)
                        {
                            throw new ApplicationException("Cannot find the necessary files inside the " + Path.GetFileName(path) + " archive.");
                        }
                        using (var iniReader = new StreamReader(megafile.Open(iniFile)))
                        using (var binReader = new BinaryReader(megafile.Open(binFile)))
                        {
                            string iniText = iniReader.ReadToEnd();
                            if (!forSole)
                            {
                                iniText = FixRoad2Load(iniText);
                            }
                            ini.Parse(iniText);
                            errors.AddRange(LoadINI(ini, false, ref modified));
                            long mapLen = binReader.BaseStream.Length;
                            if ((!isMegaMap && mapLen == 0x2000) || (isMegaMap && mapLen % 4 == 0))
                            {
                                errors.AddRange(!isMegaMap ? LoadBinaryClassic(binReader, ref modified) : LoadBinaryMega(binReader, ref modified));
                            }
                            else
                            {
                                errors.Add(String.Format("'{0}' does not have the correct size for a " + this.Name + " .bin file.", Path.GetFileName(binFile)));
                                modified = true;
                                Map.Templates.Clear();
                            }
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

        /// <summary>
        /// This detects and transforms double lines of the ROAD type in the Overlay section in the ini
        /// to single lines of the dummy type used to represent the second state of ROAD in the editor.
        /// </summary>
        /// <param name="iniReader">Stream reader to read from.</param>
        /// <returns>The ini file as string, with all double ROAD overlay lines replaced by the dummy Road2 type.</returns>
        protected string FixRoad2Load(String iniText)
        {
            // ROAD's second state can only be accessed by applying ROAD overlay to the same cell twice.
            // This can be achieved by saving its Overlay line twice in the ini file. However, this is
            // technically against the format's specs, since the game requests a list of all keys and
            // then finds the FIRST entry for each key. This means the contents of the second line never
            // get read, but those of the first are simply applied twice. For ROAD, however, this is
            // exactly what we want to achieve to unlock its second state, so the bug doesn't matter.

            string[] iniTextArr = iniText.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            Dictionary<int, int> foundAmounts = new Dictionary<int, int>();
            Dictionary<int, string> cellTypes = new Dictionary<int, string>();
            string roadname = OverlayTypes.Road.Name;
            Regex overlayRegex = new Regex("^\\s*(\\d+)\\s*=\\s*([a-zA-Z0-9]+)\\s*$", RegexOptions.IgnoreCase);
            string road2name = OverlayTypes.Road2.Name.ToUpper();
            string road2dummy = "=" + road2name;
            // Quick and dirty ini parser to find the correct ini section.
            bool inOverlay = false;
            for (int i = 0; i < iniTextArr.Length; ++i)
            {
                string currLine = iniTextArr[i].Trim();
                if (currLine.StartsWith("["))
                {
                    if (inOverlay)
                    {
                        // We were in Overlay, and passed into the next section. Abort completely.
                        break;
                    }
                    inOverlay = "[Overlay]".Equals(currLine, StringComparison.InvariantCultureIgnoreCase);
                    continue;
                }
                if (!inOverlay)
                {
                    continue;
                }
                Match match = overlayRegex.Match(currLine);
                if (match.Success)
                {
                    int cellNumber = Int32.Parse(match.Groups[1].Value);
                    foundAmounts.TryGetValue(cellNumber, out int cur);
                    foundAmounts[cellNumber] = cur + 1;
                    // Only add first detected type, just like the game would.
                    if (cur == 0)
                    {
                        cellTypes[cellNumber] = match.Groups[2].Value;
                    }
                }
            }
            // Only process the ini if any of the detected lines have a found amount of more than one. If references to literal ROAD2 are found,
            // also process the ini so they can be removed; we do not want those to be accepted as valid type by the editor.
            if (foundAmounts.All(k => k.Value == 1) && !cellTypes.Values.Contains(OverlayTypes.Road2.Name, StringComparer.InvariantCultureIgnoreCase)) 
            {
                return iniText;
            }
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
                    // stuff outside Overlay, empty lines, etc. Store and continue.
                    newIniText.Add(currLine);
                    continue;
                }
                int cellNumber = Int32.Parse(match.Groups[1].Value);
                string type = cellTypes.TryGetValue(cellNumber, out type) ? type : null;
                int amount;
                // Do not allow actual road2 type cells in the ini.
                if (type != null && !type.Equals(road2name, StringComparison.InvariantCultureIgnoreCase)
                    && foundAmounts.TryGetValue(cellNumber, out amount))
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
                        foundAmounts[cellNumber] = -1;
                    }
                    // Else, write nothing. This will happen to the entries put to -1, and is used to remove the duplicates.
                }
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < newIniText.Count; ++i)
            {
                sb.Append(newIniText[i]).Append("\r\n");
            }
            return sb.ToString();
        }

        protected virtual List<string> LoadINI(INI ini, bool forceSoloMission, ref bool modified)
        {
            return LoadINI(ini, forceSoloMission, false, ref modified);
        }

        protected List<string> LoadINI(INI ini, bool forceSoloMission, bool forSole, ref bool modified)
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
                basic.Intro = GeneralUtils.AddRemarks(basic.Intro, defVidVal, true, toAddRem, remark);
                basic.Brief = GeneralUtils.AddRemarks(basic.Brief, defVidVal, true, toAddRem, remark);
                basic.Action = GeneralUtils.AddRemarks(basic.Action, defVidVal, true, toAddRem, remark);
                basic.Win = GeneralUtils.AddRemarks(basic.Win, defVidVal, true, toAddRem, remark);
                basic.Win2 = GeneralUtils.AddRemarks(basic.Win2, defVidVal, true, toAddRem, remark);
                basic.Win3 = GeneralUtils.AddRemarks(basic.Win3, defVidVal, true, toAddRem, remark);
                basic.Win4 = GeneralUtils.AddRemarks(basic.Win4, defVidVal, true, toAddRem, remark);
                basic.Lose = GeneralUtils.AddRemarks(basic.Lose, defVidVal, true, toAddRem, remark);
            }
            Map.BasicSection.Player = Map.HouseTypes.Where(t => t.Equals(Map.BasicSection.Player)).FirstOrDefault()?.Name ?? Map.HouseTypes.First().Name;
            var mapSection = ini.Sections.Extract("Map");
            if (mapSection != null)
            {
                INI.ParseSection(new MapContext(Map, false), mapSection, Map.MapSection);
            }
            Map.MapSection.FixBounds();
            //MessageBox.Show("graphics loaded.");
            var briefingSection = ini.Sections.Extract("Briefing");
            if (briefingSection != null)
            {
                if (briefingSection.Keys.Contains("Text"))
                {
                    Map.BriefingSection.Briefing = briefingSection["Text"].Replace("@", Environment.NewLine);
                }
                else
                {
                    StringBuilder briefLines = new StringBuilder();
                    int line = 1;
                    String lineStr;
                    bool addSpace = false;
                    while (briefingSection.Keys.Contains(lineStr = line.ToString()))
                    {
                        String briefLine = briefingSection[lineStr].Trim();
                        // C&C95 v1.06 line break format.
                        bool hasBreak = briefLine.EndsWith("##");
                        if (hasBreak)
                        {
                            briefLine = briefLine.Substring(0, briefLine.Length - 2);
                        }
                        if (addSpace)
                        {
                            briefLines.Append(" ");
                        }
                        briefLines.Append(briefLine);
                        if (hasBreak)
                        {
                            briefLines.AppendLine();
                        }
                        addSpace = !hasBreak;
                        line++;
                    }
                    Map.BriefingSection.Briefing = briefLines.ToString();
                    //Map.BriefingSection.Briefing = string.Join(" ", briefingSection.Keys.Select(k => k.Value)).Replace("@", Environment.NewLine);
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
                        if (Key.Length > 8)
                        {
                            errors.Add(string.Format("TeamType '{0}' has a name that is longer than 8 characters. This will not be corrected by the loading process, but should be addressed, since it can make the teams fail to read correctly, and might even crash the game.", Key));
                            modified = true;
                        }
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
                                    modified = true;
                                }
                            }
                            else
                            {
                                errors.Add(string.Format("Team '{0}' has wrong number of tokens for mission index {1} (expecting 2).", Key, i));
                                modified = true;
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
                    catch (Exception ex)
                    {
                        errors.Add(string.Format("Teamtype '{0}' has errors and can't be parsed: {1}.", Key, ex.Message));
                        modified = true;
                    }
                }
            }
            var triggersSection = ini.Sections.Extract("Triggers");
            List<Trigger> triggers = new List<Trigger>();
            if (triggersSection != null)
            {
                foreach (var (Key, Value) in triggersSection)
                {
                    try
                    {
                        if (Key.Length > 4)
                        {
                            errors.Add(string.Format("Trigger '{0}' has a name that is longer than 4 characters. This will not be corrected by the loading process, but should be addressed, since it can make the triggers fail to read correctly and link to objects and cell triggers, and might even crash the game.", Key));
                            modified = true;
                        }
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
                            triggers.Add(trigger);
                        }
                        else
                        {
                            errors.Add(string.Format("Trigger '{0}' has too few tokens (expecting at least 5).", Key));
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
            // Sort
            var comparer = new ExplorerComparer();
            triggers.Sort((x, y) => comparer.Compare(x.Name, y.Name));
            HashSet<string> checkTrigs = Trigger.None.Yield().Concat(triggers.Select(t => t.Name)).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            HashSet<string> checkCellTrigs = Map.FilterCellTriggers(triggers).Select(t => t.Name).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            HashSet<string> checkUnitTrigs = Trigger.None.Yield().Concat(Map.FilterUnitTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            HashSet<string> checkStrcTrigs = Trigger.None.Yield().Concat(Map.FilterStructureTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            HashSet<string> checkTerrTrigs = Trigger.None.Yield().Concat(Map.FilterTerrainTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
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
                    var tokens = Value.Split(',');
                    if (tokens.Length == 2)
                    {
                        var terrainType = Map.TerrainTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault();
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
                                Trigger = tokens[1]
                            };
                            if (Map.Technos.Add(cell, newTerr))
                            {
                                if (!checkTrigs.Contains(newTerr.Trigger))
                                {
                                    errors.Add(string.Format("Terrain '{0}' links to unknown trigger '{1}'; clearing trigger.", terrainType.Name, tokens[1]));
                                    modified = true;
                                    newTerr.Trigger = Trigger.None;
                                }
                                else if (!checkTerrTrigs.Contains(newTerr.Trigger))
                                {
                                    errors.Add(string.Format("Terrain '{0}' links to trigger '{1}' which does not contain an event applicable to terrain; clearing trigger.", terrainType.Name, tokens[1]));
                                    modified = true;
                                    newTerr.Trigger = Trigger.None;
                                }
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
                                        errors.Add(string.Format("Terrain '{0}' placed on cell {1} overlaps unknown techno in cell {2}; skipping.", terrainType.Name, cell, reportCell));
                                        modified = true;
                                    }
                                    else
                                    {
                                        errors.Add(string.Format("Terrain '{0}' placed on cell {1} overlaps unknown techno; skipping.", terrainType.Name, cell));
                                        modified = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Terrain '{0}' references unknown terrain.", tokens[0]));
                            modified = true;
                        }
                    }
                    else
                    {
                        errors.Add(string.Format("Terrain '{0}' has wrong number of tokens (expecting 2).", Key));
                        modified = true;
                    }
                }
            }
            var overlaySection = ini.Sections.Extract("Overlay");
            if (overlaySection != null)
            {
                foreach (var (Key, Value) in overlaySection)
                {
                    int cell;
                    if (!int.TryParse(Key, out cell))
                    {
                        errors.Add(string.Format("Cell for overlay cannot be parsed. Key: '{0}', value: '{1}'; skipping.", Key, Value));
                        modified = true;
                        continue;
                    }
                    if (!Map.Metrics.Contains(cell))
                    {
                        errors.Add(string.Format("Cell for overlay is not inside the map bounds. Key: '{0}', value: '{1}'; skipping.", Key, Value));
                        modified = true;
                        continue;

                    }
                    var overlayType = Map.OverlayTypes.Where(t => t.Equals(Value)).FirstOrDefault();
                    if (overlayType != null)
                    {
                        if (Globals.FilterTheaterObjects && overlayType.Theaters != null && !overlayType.Theaters.Contains(Map.Theater))
                        {
                            errors.Add(string.Format("Overlay '{0}' is not available in the set theater; skipping.", overlayType.Name));
                            modified = true;
                            continue;
                        }
                        Map.Overlay[cell] = new Overlay { Type = overlayType, Icon = 0 };
                    }
                    else
                    {
                        errors.Add(string.Format("Overlay '{0}' references unknown overlay.", Value));
                        modified = true;
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
                                        errors.Add(string.Format("Infantry '{0}' links to trigger '{1}' which does not contain an event applicable to infantry; clearing trigger.", infantryType.Name, tokens[7]));
                                        modified = true;
                                        tokens[7] = Trigger.None;
                                    }
                                    infantryGroup.Infantry[stoppingPos] = new Infantry(infantryGroup)
                                    {
                                        Type = infantryType,
                                        House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                                        Strength = strength,
                                        Direction = DirectionType.GetDirectionType(dirValue, Map.UnitDirectionTypes),
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
            var unitsSection = ini.Sections.Extract("Units");
            if (unitsSection != null)
            {
                foreach (var (Key, Value) in unitsSection)
                {
                    var tokens = Value.Split(',');
                    if (tokens.Length == 7)
                    {
                        var unitType = Map.UnitTypes.Where(t => t.IsUnit && t.Equals(tokens[1])).FirstOrDefault();
                        if (unitType == null)
                        {
                            errors.Add(string.Format("Unit '{0}' references unknown unit.", tokens[1]));
                            modified = true;
                            continue;
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
                            Trigger = tokens[6]
                        };
                        // "Rescue" and "Unload" both make the MCV deploy, but "Rescue" looks very strange in the editor, so we keep only one of them and convert the other.
                        if (MissionTypes.MISSION_RESCUE.Equals(tokens[5], StringComparison.InvariantCultureIgnoreCase) && newUnit.Type.Equals(UnitTypes.MCV))
                        {
                            newUnit.Mission = MissionTypes.MISSION_UNLOAD;
                        }
                        if (Map.Technos.Add(cell, newUnit))
                        {
                            if (!checkTrigs.Contains(tokens[6]))
                            {
                                errors.Add(string.Format("Unit '{0}' links to unknown trigger '{1}'; clearing trigger.", unitType.Name, newUnit.Trigger));
                                modified = true;
                                newUnit.Trigger = Trigger.None;
                            }
                            else if (!checkUnitTrigs.Contains(tokens[6]))
                            {
                                errors.Add(string.Format("Unit '{0}' links to trigger '{1}' which does not contain an event applicable to units; clearing trigger.", unitType.Name, newUnit.Trigger));
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
                        Unit newUnit = new Unit()
                        {
                            Type = aircraftType,
                            House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                            Strength = strength,
                            Direction = DirectionType.GetDirectionType(dirValue, Map.UnitDirectionTypes),
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
            var structuresSection = ini.Sections.Extract("Structures");
            if (structuresSection != null && !forSole)
            {
                foreach (var (Key, Value) in structuresSection)
                {
                    var tokens = Value.Split(',');
                    if (tokens.Length == 6)
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
                        Building newBld = new Building()
                        {
                            Type = buildingType,
                            House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                            Strength = strength,
                            Direction = DirectionType.GetDirectionType(dirValue, Map.BuildingDirectionTypes),
                            Trigger = tokens[5]
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
                                errors.Add(string.Format("Structure '{0}' links to trigger '{1}' which does not contain an event applicable to structures; clearing trigger.", buildingType.Name, tokens[5]));
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
            if (baseSection != null && !forSole)
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
                                if (Globals.FilterTheaterObjects && buildingType.Theaters != null && !buildingType.Theaters.Contains(Map.Theater))
                                {
                                    errors.Add(string.Format("Base rebuild entry {0} references structure '{1}' which is not available in the set theater; skipping.", priority, buildingType.Name));
                                    modified = true;
                                    continue;
                                }
                                int coord;
                                if (!int.TryParse(tokens[1], out coord))
                                {
                                    errors.Add(string.Format("Coordinates for base rebuild entry '{0}' cannot be parsed; value: '{1}'; skipping.", buildingType.Name, tokens[1]));
                                    modified = true;
                                    continue;
                                }
                                // Preparations for megamap support.
                                Point location = new Point((coord >> 8) & 0x7F, (coord >> 24) & 0x7F);
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
                                errors.Add(string.Format("Base rebuild entry {0} references unknown structure '{1}'.", priority, tokens[0]));
                                modified = true;
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
                            if (checkTrigs.Contains(Value))
                            {
                                if (checkCellTrigs.Contains(Value))
                                {
                                    Map.CellTriggers[cell] = new CellTrigger(Value);
                                }
                                else
                                {
                                    errors.Add(string.Format("Cell trigger {0} links to trigger '{1}' which does not contain a placeable event; skipping.", cell, Value));
                                    modified = true;
                                }
                            }
                            else
                            {
                                errors.Add(string.Format("Cell trigger {0} links to unknown trigger '{1}'; skipping.", cell, Value));
                                modified = true;
                            }
                        }
                        else
                        {
                            errors.Add(string.Format("Cell trigger {0} is outside map bounds; skipping.", cell));
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
            // Won't trigger the automatic cleanup and notifications.
            Map.Triggers.Clear();
            Map.Triggers.AddRange(triggers);
            Map.TeamTypes.Sort((x, y) => comparer.Compare(x.Name, y.Name));
            extraSections = ini.Sections;
            bool switchedToSolo = !forSole && forceSoloMission && !Map.BasicSection.SoloMission
                && ((triggers.Any(t => t.Action1.ActionType == ActionTypes.ACTION_WIN) && triggers.Any(t => t.Action1.ActionType == ActionTypes.ACTION_LOSE))
                    || triggers.Any(t => t.Event1.EventType == EventTypes.EVENT_ANY && t.Action1.ActionType == ActionTypes.ACTION_WINLOSE));
            if (switchedToSolo)
            {
                errors.Insert(0, "Filename detected as classic single player mission format, and win and lose trigger detected. Applying \"SoloMission\" flag.");
                Map.BasicSection.SoloMission = true;
            }
            Map.EndUpdate();
            return errors;
        }

        protected IEnumerable<string> LoadBinaryClassic(BinaryReader reader, ref bool modified)
        {
            var errors = new List<string>();
            Map.Templates.Clear();
            for (var y = 0; y < Map.Metrics.Height; ++y)
            {
                for (var x = 0; x < Map.Metrics.Width; ++x)
                {
                    var typeValue = reader.ReadByte();
                    var iconValue = reader.ReadByte();
                    TemplateType templateType = ChecKTemplateType(typeValue, iconValue, x, y, errors, ref modified);
                    Map.Templates[y, x] = (templateType != null) ? new Template { Type = templateType, Icon = iconValue } : null;
                }
            }
            return errors;
        }

        protected IEnumerable<string> LoadBinaryMega(BinaryReader reader, ref bool modified)
        {
            var errors = new List<string>();
            Map.Templates.Clear();
            long dataLen = reader.BaseStream.Length;
            int mapLen = Map.Metrics.Length;
            int mapWidth = Map.Metrics.Width;
            int lastCell = -1;
            while (reader.BaseStream.Position < dataLen)
            {
                byte cellLow = reader.ReadByte();
                byte cellHi = reader.ReadByte();
                int cell = (cellHi << 8) | cellLow;
                if (cell == lastCell)
                {
                    errors.Add(String.Format("Map contains duplicate cell numbers.", cell));
                }
                else if (cell < lastCell)
                {
                    errors.Add(String.Format("Map cell numbers are not in sequential order.", cell));
                }
                if (cell > mapLen)
                {
                    errors.Add(String.Format("Map contains cell number '{0}' which is too large for a TD MegaMap.", cell));
                    modified = true;
                    // Just abort I guess?
                    break;
                }
                int y = cell / mapWidth;
                int x = cell % mapWidth;
                byte typeValue = reader.ReadByte();
                byte iconValue = reader.ReadByte();
                TemplateType templateType = ChecKTemplateType(typeValue, iconValue, x, y, errors, ref modified);
                Map.Templates[y,x] = (templateType != null) ? new Template { Type = templateType, Icon = iconValue } : null;
            }
            return errors;
        }

        protected TemplateType ChecKTemplateType(int typeValue, int iconValue, int x, int y, List<string> errors, ref bool modified)
        {
            TemplateType templateType = Map.TemplateTypes.Where(t => t.Equals(typeValue)).FirstOrDefault();
            // Prevent loading of illegal tiles.
            if (templateType != null)
            {
                bool isRandom = (templateType.Flag & TemplateTypeFlag.RandomCell) != TemplateTypeFlag.None;
                if ((templateType.Flag & TemplateTypeFlag.Clear) != TemplateTypeFlag.None || (templateType.Flag & TemplateTypeFlag.Group) == TemplateTypeFlag.Group)
                {
                    // No explicitly set Clear terrain allowed. Also no explicitly set versions allowed of the "group" dummy entries.
                    templateType = null;
                }
                else if (templateType.Theaters != null && !templateType.Theaters.Contains(Map.Theater))
                {
                    errors.Add(String.Format("Template '{0}' at cell [{1},{2}] is not available in the set theater; clearing.", templateType.Name.ToUpper(), x, y));
                    modified = true;
                    templateType = null;
                }
                else if (iconValue >= templateType.NumIcons)
                {
                    errors.Add(String.Format("Template '{0}' at cell [{1},{2}] has an icon set ({3}) that is outside its icons range; clearing.", templateType.Name.ToUpper(), x, y, iconValue));
                    modified = true;
                    templateType = null;
                }
                else if (!isRandom && templateType.IconMask != null && !templateType.IconMask[iconValue / templateType.IconWidth, iconValue % templateType.IconWidth])
                {
                    errors.Add(String.Format("Template '{0}' at cell [{1},{2}] has an icon set ({3}) that is not part of its placeable cells; clearing.", templateType.Name.ToUpper(), x, y, iconValue));
                    modified = true;
                    templateType = null;
                }
            }
            else if (typeValue != 0xFF)
            {
                errors.Add(String.Format("Unknown template value {0:X2} at cell [{1},{2}]; clearing.", typeValue, x, y));
                modified = true;
            }
            return templateType;
        }

        public bool Save(string path, FileType fileType)
        {
            return Save(path, fileType, null);
        }

        public virtual bool Save(string path, FileType fileType, Bitmap customPreview)
        {
            return Save(path, fileType, false, customPreview);
        }

        public bool Save(string path, FileType fileType, bool forSole, Bitmap customPreview)
        {
            String errors = Validate(forSole);
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
                    SaveINI(ini, fileType, path);
                    using (var iniWriter = new StreamWriter(iniPath))
                    {
                        if (forSole)
                        {
                            iniWriter.Write(ini.ToString());
                        }
                        else
                        {
                            FixRoad2Save(ini, iniWriter);
                        }
                    }
                    using (var binStream = new FileStream(binPath, FileMode.Create))
                    using (var binWriter = new BinaryWriter(binStream))
                    {
                        if (!isMegaMap)
                        {
                            SaveBinaryClassic(binWriter);
                        }
                        else
                        {
                            SaveBinaryMega(binWriter);
                        }
                    }
                    // None of this junk for Sole Survivor.
                    if (!forSole && (!Map.BasicSection.SoloMission || !Properties.Settings.Default.NoMetaFilesForSinglePlay))
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
                    SaveINI(ini, fileType, path);
                    using (var iniStream = new MemoryStream())
                    using (var binStream = new MemoryStream())
                    using (var tgaStream = new MemoryStream())
                    using (var jsonStream = new MemoryStream())
                    using (var iniWriter = new StreamWriter(iniStream))
                    using (var binWriter = new BinaryWriter(binStream))
                    using (var jsonWriter = new JsonTextWriter(new StreamWriter(jsonStream)))
                    using (var megafileBuilder = new MegafileBuilder(String.Empty, path))
                    {
                        if (forSole)
                        {
                            iniWriter.Write(ini.ToString());
                        }
                        else
                        {
                            FixRoad2Save(ini, iniWriter);
                        }
                        iniWriter.Flush();
                        iniStream.Position = 0;
                        if (!isMegaMap)
                        {
                            SaveBinaryClassic(binWriter);
                        }
                        else
                        {
                            SaveBinaryMega(binWriter);
                        }
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

        /// <summary>
        /// This detects Overlay lines of the dummy type used to represent the second state of ROAD,
        /// and replaces them with double lines of the ROAD type so the game will apply them correctly.
        /// </summary>
        /// <param name="ini">The generated ini file</param>
        /// <param name="iniWriter">The stream writer to write the text to.</param>
        protected void FixRoad2Save(INI ini, StreamWriter iniWriter)
        {
            // ROAD's second state can only be accessed by applying ROAD overlay to the same cell twice.
            // This can be achieved by saving its Overlay line twice in the ini file. However, this is
            // technically against the format's specs, since the game requests a list of all keys and
            // then finds the FIRST entry for each key. This means the contents of the second line never
            // get read, but those of the first are simply applied twice. For ROAD, however, this is
            // exactly what we want to achieve to unlock its second state, so the bug doesn't matter.

            string roadLine = "=" + OverlayTypes.Road.Name.ToUpperInvariant() + "\r\n";
            Regex roadDetect = new Regex("^\\s*(\\d+)\\s*=\\s*" + OverlayTypes.Road2.Name + "\\s*$", RegexOptions.IgnoreCase);
            string[] iniString = ini.ToString().Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            // Quick and dirty ini parser to find the correct ini section.
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
                    // Write twice to achieve second state. (Already contains line break.)
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

        protected virtual void SaveINI(INI ini, FileType fileType, string fileName)
        {
            if (extraSections != null)
            {
                ini.Sections.AddRange(extraSections);
            }
            SaveIniBasic(ini, fileName);
            SaveIniMap(ini);
            SaveIniSteam(ini, fileType);
            SaveIniBriefing(ini);
            SaveIniCellTriggers(ini, false);
            SaveIniTeamTypes(ini, false);
            SaveIniTriggers(ini, false);
            SaveIniWaypoints(ini);
            SaveIniBase(ini, false);
            SaveIniInfantry(ini);
            SaveIniStructures(ini);
            SaveIniUnits(ini);
            SaveIniAircraft(ini);
            SaveIniHouses(ini);
            SaveIniOverlay(ini);
            SaveIniSmudge(ini);
            SaveINITerrain(ini);
        }

        protected INISection SaveIniBasic(INI ini, string fileName)
        {
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
            INISection basicSection = ini.Sections.Add("Basic");
            INI.WriteSection(new MapContext(Map, false), basicSection, Map.BasicSection);
            return basicSection;
        }

        protected INISection SaveIniMap(INI ini)
        {
            Map.MapSection.FixBounds();
            INISection mapSection = ini.Sections.Add("Map");
            if (isMegaMap)
            {
                mapSection["Version"] = "1";
            }
            INI.WriteSection(new MapContext(Map, false), mapSection, Map.MapSection);
            return mapSection;
        }

        protected INISection SaveIniSteam(INI ini, FileType fileType)
        {
            if (fileType == FileType.PGM)
            {
                return null;
            }
            INISection steamSection = ini.Sections.Add("Steam");
            INI.WriteSection(new MapContext(Map, false), steamSection, Map.SteamSection);
            return steamSection;
        }

        protected INISection SaveIniBriefing(INI ini)
        {
            if (string.IsNullOrEmpty(Map.BriefingSection.Briefing))
            {
                return null;
            }
            var briefingSection = ini.Sections.Add("Briefing");
            String briefText = Map.BriefingSection.Briefing.Replace('\t', ' ').Trim('\r', '\n', ' ').Replace("\r\n", "\n").Replace("\r", "\n");
            if (string.IsNullOrEmpty(briefText))
            {
                return null;
            }
            briefingSection["Text"] = briefText.Replace("\n", "@");
            if (Globals.WriteClassicBriefing)
            {
                const int classicLineCutoff = 74;
                String[] lines = briefText.Split('\n');
                List<String> finalLines = new List<string>();
                int last = lines.Length - 1;
                for (int i = 0; i < lines.Length; ++i)
                {
                    String line = lines[i].Trim();
                    if (i != last)
                    {
                        line += "##";
                    }
                    if (line.Length <= classicLineCutoff)
                    {
                        finalLines.Add(line);
                        continue;
                    }
                    String[] splitLine = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int wordIndex = 0;
                    while (wordIndex < splitLine.Length)
                    {
                        StringBuilder sb = new StringBuilder();
                        // Always allow initial word
                        int nextLength = 0;
                        while (nextLength < classicLineCutoff && wordIndex < splitLine.Length)
                        {
                            if (sb.Length > 0)
                                sb.Append(' ');
                            sb.Append(splitLine[wordIndex++]);
                            nextLength = wordIndex >= splitLine.Length ? 0 : (sb.Length + 1 + splitLine[wordIndex].Length);
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

        protected INISection SaveIniCellTriggers(INI ini, bool omitEmpty)
        {
            if (omitEmpty && Map.CellTriggers.Count() == 0)
            {
                return null;
            }
            var cellTriggersSection = ini.Sections.Add("CellTriggers");
            foreach (var (cell, cellTrigger) in Map.CellTriggers)
            {
                cellTriggersSection[cell.ToString()] = cellTrigger.Trigger;
            }
            return cellTriggersSection;
        }

        protected INISection SaveIniTeamTypes(INI ini, bool omitEmpty)
        {
            if (omitEmpty && Map.TeamTypes.Count == 0)
            {
                return null;
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
            return teamTypesSection;
        }

        protected INISection SaveIniTriggers(INI ini, bool omitEmpty)
        {
            if (omitEmpty && Map.Triggers.Count == 0)
            {
                return null;
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
            return triggersSection;
        }

        protected INISection SaveIniWaypoints(INI ini)
        {
            var waypointsSection = ini.Sections.Add("Waypoints");
            for (var i = 0; i < Map.Waypoints.Length; ++i)
            {
                var waypoint = Map.Waypoints[i];
                if (waypoint.Cell.HasValue)
                {
                    waypointsSection[i.ToString()] = waypoint.Cell.Value.ToString();
                }
            }
            return waypointsSection;
        }

        protected INISection SaveIniBase(INI ini, bool dummy)
        {
            var baseSection = ini.Sections.Add("Base");
            if (!dummy)
            {
                var baseBuildings = Map.Buildings.OfType<Building>().Where(x => x.Occupier.BasePriority >= 0).OrderByDescending(x => x.Occupier.BasePriority).ToArray();
                var baseIndex = baseBuildings.Length - 1;
                foreach (var (location, building) in baseBuildings)
                {
                    var key = baseIndex.ToString("D3");
                    baseIndex--;

                    baseSection[key] = string.Format("{0},{1}",
                        building.Type.Name.ToUpper(),
                        ((location.Y & 0x7F) << 24) | ((location.X & 0x7F) << 8)
                    );
                }
                baseSection["Count"] = baseBuildings.Length.ToString();
            }
            else
            {
                baseSection["Count"] = "0";
            }
            return baseSection;
        }

        protected INISection SaveIniInfantry(INI ini)
        {
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
            return infantrySection;
        }

        protected INISection SaveIniStructures(INI ini)
        {
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
            return structuresSection;
        }

        protected INISection SaveIniUnits(INI ini)
        {
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
            return unitsSection;
        }

        protected INISection SaveIniAircraft(INI ini)
        {
            // Classic game does not support this, so it's disabled by default.
            if (Globals.DisableAirUnits)
            {
                return null;
            }
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
            return aircraftSection;
        }

        protected IEnumerable<INISection> SaveIniHouses(INI ini)
        {
            List<INISection> houseSections = new List<INISection>();
            foreach (var house in Map.Houses)
            {
                if ((house.Type.ID < 0) || !house.Enabled)
                {
                    continue;
                }
                INISection houseSection = ini.Sections.Add(house.Type.Name);
                INI.WriteSection(new MapContext(Map, false), houseSection, house);
                houseSections.Add(houseSection);
            }
            return houseSections;
        }

        protected INISection SaveIniOverlay(INI ini)
        {
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
            return overlaySection;
        }

        protected INISection SaveIniSmudge(INI ini)
        {
            var smudgeSection = ini.Sections.Add("Smudge");
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
            return smudgeSection;
        }

        protected INISection SaveINITerrain(INI ini)
        {
            var terrainSection = ini.Sections.Add("Terrain");
            foreach (var (location, terrain) in Map.Technos.OfType<Terrain>())
            {
                Map.Metrics.GetCell(location, out int cell);
                terrainSection[cell.ToString()] = string.Format("{0},{1}", terrain.Type.Name, terrain.Trigger);
            }
            return terrainSection;
        }

        protected void SaveBinaryClassic(BinaryWriter writer)
        {
            for (var y = 0; y < Map.Metrics.Height; ++y)
            {
                for (var x = 0; x < Map.Metrics.Width; ++x)
                {
                    var template = Map.Templates[y, x];
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

        protected void SaveBinaryMega(BinaryWriter writer)
        {
            int height = Map.Metrics.Height;
            int width = Map.Metrics.Width;
            for (var y = 0; y < height; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    var template = Map.Templates[y, x];
                    if (template == null || (template.Type.Flag & TemplateTypeFlag.Clear) != 0)
                    {
                        continue;
                    }
                    int cell = y * width + x;
                    writer.Write((byte)(cell & 0xFF));
                    writer.Write((byte)((cell >> 8) & 0xFF));
                    writer.Write((byte)template.Type.ID);
                    writer.Write((byte)template.Icon);
                }
            }
        }

        protected void SaveMapPreview(Stream stream, Boolean renderAll)
        {
            Map.GenerateMapPreview(renderAll ? this.GameType : GameType.None, renderAll).Save(stream);
        }

        protected void SaveJSON(JsonTextWriter writer)
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

        public String Validate()
        {
            return Validate(false);
        }

        protected String Validate(bool forSS)
        {
            StringBuilder sb = new StringBuilder("Error(s) during map validation:");
            bool ok = true;
            int numAircraft = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsAircraft).Count();
            int numBuildings = Map.Buildings.OfType<Building>().Where(x => x.Occupier.IsPrebuilt).Count();
            int numInfantry = Map.Technos.OfType<InfantryGroup>().Sum(item => item.Occupier.Infantry.Count(i => i != null));
            int numTerrain = Map.Technos.OfType<Terrain>().Count();
            int numUnits = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsUnit).Count();
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
            if (!forSS)
            {
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
            }
            return ok ? null : sb.ToString();
        }

        protected void BasicSection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Player":
                    UpdateBasePlayerHouse();
                    break;
                case "SoloMission":
                    UpdateWaypoints();
                    break;
            }
        }

        protected void MapSection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Theater":
                    Map.InitTheater(GameType);
                    break;
            }
        }

        protected void UpdateBasePlayerHouse()
        {
            Map.BasicSection.BasePlayer = HouseTypes.GetBasePlayer(Map.BasicSection.Player);
            var basePlayer = Map.HouseTypesIncludingNone.Where(h => h.Equals(Map.BasicSection.BasePlayer)).FirstOrDefault() ?? Map.HouseTypes.First();
            // Not really needed now BasePlayer House is always "None", but whatever.
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

        #region IDisposable Support
        protected bool disposedValue = false;

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
