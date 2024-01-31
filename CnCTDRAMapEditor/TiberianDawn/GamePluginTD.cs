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
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TGASharpLib;

namespace MobiusEditor.TiberianDawn
{
    public class GamePluginTD : IGamePlugin
    {
        protected bool isMegaMap = false;

        protected static readonly Regex SinglePlayRegex = new Regex("^SC[A-LN-Z]\\d{2}\\d?[EWX][A-EL]$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected static readonly Regex MovieRegex = new Regex(@"^(?:.*?\\)*(.*?)\.BK2$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly GameInfoTibDawn gameTypeInfo = new GameInfoTibDawn();

        protected static readonly IEnumerable<ITechnoType> fullTechnoTypes;

        protected const string movieEmpty = "x";
        protected readonly IEnumerable<string> movieTypes;

        protected static readonly IEnumerable<string> movieTypesTD = new string[]
        {
            "AIRSTRK",
            "AKIRA",
            "BANNER",
            "BANR_NOD",
            "BCANYON",
            "BKGROUND",
            "BLACKOUT",
            "BODYBAGS",
            "BOMBAWAY",
            "BOMBFLEE",
            "BURDET1",
            "BURDET2",
            "CC2TEASE",
            "CONSYARD",
            "DESFLEES",
            "DESKILL",
            "DESOLAT",
            "DESSWEEP",
            "DINO",
            "FLAG",
            "FLYY",
            "FORESTKL",
            "GAMEOVER",
            "GDI1",
            "GDI2",
            "GDI3",
            "GDI4A",
            "GDI4B",
            "GDI5",
            "GDI6",
            "GDI7",
            "GDI8A",
            "GDI8B",
            "GDI9",
            "GDI10",
            "GDI11",
            "GDI12",
            "GDI13",
            "GDI14",
            "GDI15",
            "GDI3LOSE",
            "GDIEND1",
            "GDIEND2",
            "GDIFINA",
            "GDIFINB",
            "GDILOSE",
            "GENERIC",
            "GUNBOAT",
            "HELLVALY",
            "INFERNO",
            "INSITES",
            "INTRO2",
            "IONTEST",
            "KANEPRE",
            "LANDING",
            "LOGO",
            "NAPALM",
            "NITEJUMP",
            "NOD1",
            "NOD2",
            "NOD3",
            "NOD4A",
            "NOD4B",
            "NOD5",
            "NOD6",
            "NOD7A",
            "NOD7B",
            "NOD8",
            "NOD9",
            "NOD10A",
            "NOD10B",
            "NOD11",
            "NOD12",
            "NOD13",
            "NOD1PRE",
            "NODEND1",
            "NODEND2",
            "NODEND3",
            "NODEND4",
            "NODFINAL",
            "NODFLEES",
            "NODLOSE",
            "NODSWEEP",
            "NUKE",
            "OBEL",
            "PARATROP",
            "PINTLE",
            "PLANECRA",
            "PODIUM",
            "REFINT",
            "REFINERY",
            "RETRO",
            "SABOTAGE",
            "SAMDIE",
            "SAMSITE",
            "SEIGE",
            "SETHPRE",
            "SIZZLE",
            "SIZZLE2",
            "SPYCRASH",
            "STEALTH",
            "SUNDIAL",
            "TANKGO",
            "TANKKILL",
            "TBRINFO1",
            "TBRINFO2",
            "TBRINFO3",
            "TIBERFX",
            "TRAILER",
            "TRTKIL_D",
            "TURTKILL",
            "VISOR",
        };

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

        protected const string themeEmpty = "No Theme";

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

        public virtual GameInfo GameInfo => gameTypeInfo;
        public virtual HouseType ActiveHouse { get; set; }
        public virtual bool IsMegaMap => isMegaMap;
        public virtual Map Map { get; protected set; }
        public virtual Image MapImage { get; protected set; }

        protected IFeedBackHandler feedBackHandler;
        public virtual IFeedBackHandler FeedBackHandler
        {
            get { return feedBackHandler; }
            set { feedBackHandler = value; }
        }

        bool isDirty;
        public virtual bool Dirty
        {
            get { return isDirty; }
            set { isDirty = value; feedBackHandler?.UpdateStatus(); }
        }

        protected INISectionCollection extraSections;
        public virtual string GetExtraIniText()
        {
            INI ini = new INI();
            if (extraSections != null)
            {
                ini.Sections.AddRange(extraSections);
            }
            return ini.ToString();
        }

        public virtual IEnumerable<string> SetExtraIniText(String extraIniText, out bool footPrintsChanged)
        {
            return SetExtraIniText(extraIniText, false, out footPrintsChanged);
        }

        public IEnumerable<string> TestSetExtraIniText(String extraIniText, bool isSolo, bool expansionEnabled, out bool footPrintsChanged)
        {
            // No such thing in TD as rules that change footprints, unless I add support for the 1.06 mission option.
            footPrintsChanged = false;
            return null;
        }

        public IEnumerable<string> SetExtraIniText(String extraIniText, bool forFootprintTest, out bool footPrintsChanged)
        {
            footPrintsChanged = false;
            INI extraTextIni = new INI();
            try
            {
                extraTextIni.Parse(extraIniText ?? String.Empty);
            }
            catch
            {
                return null;
            }
            // Remove any sections known and handled / disallowed by the editor.
            INITools.ClearDataFrom(extraTextIni, "Basic", (BasicSection)Map.BasicSection);
            INITools.ClearDataFrom(extraTextIni, "Map", Map.MapSection);
            if (extraTextIni.Sections["Briefing"] is INISection briefSec)
            {
                briefSec.Remove("Text");
                briefSec.RemoveWhere(k => Regex.IsMatch(k, "^\\d+$"));
                if (briefSec.Count == 0)
                {
                    extraTextIni.Sections.Remove(briefSec.Name);
                }
            }
            extraTextIni.Sections.Remove("Steam");
            extraTextIni.Sections.Remove("TeamTypes");
            extraTextIni.Sections.Remove("Triggers");
            extraTextIni.Sections.Remove("Terrain");
            extraTextIni.Sections.Remove("Overlay");
            extraTextIni.Sections.Remove("Smudge");
            extraTextIni.Sections.Remove("Infantry");
            extraTextIni.Sections.Remove("Units");
            extraTextIni.Sections.Remove("Aircraft");
            extraTextIni.Sections.Remove("Structures");
            // Digest. Seems to exist in some console maps.
            extraTextIni.Sections.Remove("Digest");
            if (extraTextIni.Sections["Base"] is INISection baseSec)
            {
                baseSec.Remove("Count");
                baseSec.RemoveWhere(k => Regex.IsMatch(k, "^\\d{3}$"));
                if (baseSec.Count == 0)
                {
                    extraTextIni.Sections.Remove(baseSec.Name);
                }
            }
            extraTextIni.Sections.Remove("Waypoints");
            extraTextIni.Sections.Remove("CellTriggers");
            foreach (Model.House house in Map.Houses)
            {
                INITools.ClearDataFrom(extraTextIni, house.Type.Name, (House)house);
            }
            extraSections = extraTextIni.Sections.Count == 0 ? null : extraTextIni.Sections;
            if (!Globals.Ignore106Scripting)
            {
                // Perhaps support the v1.06 bibs-disabling option in the future? Would need an entire bib-changing logic like RA has though.
            }
            return null;
        }

        public static bool CheckForMegamap(INI iniContents)
        {
            return INITools.CheckForIniInfo(iniContents, "Map", "Version", "1");
        }

        public IEnumerable<string> Initialize()
        {
            Globals.TheTeamColorManager.Load(@"DATA\XML\CNCTDTEAMCOLORS.XML");
            AddTeamColorsTD(Globals.TheTeamColorManager);
            return new List<string>();
        }

        public static void AddTeamColorsTD(ITeamColorManager teamColorManager)
        {
            // Only applicable for Remastered colors since I can't control those.
            if (teamColorManager is TeamColorManager tcm)
            {
                TeamColor colGoodGuy = tcm.GetItem("GOOD");
                TeamColor colBadUnits = tcm.GetItem("BAD_UNIT");
                string baseVariant = colGoodGuy?.Variant?? "BASE_TEAM";
                if (colGoodGuy != null)
                {
                    // Neutral
                    TeamColor teamColorSNeutral = new TeamColor(tcm);
                    teamColorSNeutral.Load(colGoodGuy, "NEUTRAL");
                    tcm.AddTeamColor(teamColorSNeutral);
                    // Special
                    TeamColor teamColorSpecial = new TeamColor(tcm);
                    teamColorSpecial.Load(colGoodGuy, "SPECIAL");
                    tcm.AddTeamColor(teamColorSpecial);
                }
                // Black for unowned.
                TeamColor teamColorNone = new TeamColor(tcm);
                teamColorNone.Load("NONE", baseVariant,
                    Color.FromArgb(66, 255, 0), Color.FromArgb(0, 255, 56), 0,
                    new Vector3(0.30f, -1.00f, 0.00f), new Vector3(0f, 1f, 1f), new Vector2(0.0f, 0.1f),
                    new Vector3(0, 1, 1), new Vector2(0, 1), Color.FromArgb(61, 61, 59));
                tcm.AddTeamColor(teamColorNone);
                if (colBadUnits != null)
                {
                    // Extra color for flag 7: metallic blue.
                    TeamColor teamColorSeven = new TeamColor(tcm);
                    teamColorSeven.Load(colBadUnits, "MULTI7");
                    tcm.AddTeamColor(teamColorSeven);
                }
                // Extra color for flag 8: copy of RA's purple.
                TeamColor teamColorEight = new TeamColor(tcm);
                teamColorEight.Load("MULTI8", baseVariant,
                    Color.FromArgb(66, 255, 0), Color.FromArgb(0, 255, 56), 0,
                    new Vector3(0.410f, 0.300f, 0.000f), new Vector3(0f, 1f, 1f), new Vector2(0.0f, 1.0f),
                    new Vector3(0, 1, 1), new Vector2(0, 1), Color.FromArgb(77, 13, 255));
                tcm.AddTeamColor(teamColorEight);
            }
        }

        static GamePluginTD()
        {
            fullTechnoTypes = InfantryTypes.GetTypes().Cast<ITechnoType>().Concat(UnitTypes.GetTypes(false).Cast<ITechnoType>());
        }

        protected GamePluginTD()
        {
            // Readonly, so I'm splitting this off
            HashSet<string> movies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            String moviesMegPath = Path.Combine(Globals.TheArchiveManager.LoadRoot, "MOVIES_TD.MEG");
            if (File.Exists(moviesMegPath))
            {
                using (Megafile megafile = new Megafile(moviesMegPath))
                {
                    foreach (string filename in megafile)
                    {
                        Match m = MovieRegex.Match(filename);
                        if (m.Success)
                        {
                            movies.Add(m.Groups[1].ToString());
                        }
                    }
                }
            }
            // Preparation for decoupling from remaster files.
            if (movies.Count == 0)
            {
                movies.UnionWith(movieTypesTD);
            }
            foreach (string mov in movieTypesAdditional)
            {
                string movName = GeneralUtils.TrimRemarks(mov, true, ';', '(');
                if (movies.FirstOrDefault(m => m.Equals(movName, StringComparison.OrdinalIgnoreCase)) == null)
                {
                    movies.Add(mov);
                }
            }
            List<string> finalMovies = movies.ToList();
            finalMovies.Sort(new ExplorerComparer());
            finalMovies.Insert(0, movieEmpty);
            movieTypes = finalMovies.ToArray();
        }

        public GamePluginTD(bool megaMap)
            : this(true, megaMap)
        {
        }

        public GamePluginTD(bool mapImage, bool megaMap)
            :this()
        {
            this.isMegaMap = megaMap;
            IEnumerable<Waypoint> playerWaypoints = Enumerable.Range(0, Constants.MultiStartPoints).Select(i => new Waypoint(string.Format("P{0}", i), Waypoint.GetFlagForMpId(i)));
            IEnumerable<Waypoint> generalWaypoints = Enumerable.Range(Constants.MultiStartPoints, 25 - Constants.MultiStartPoints).Select(i => new Waypoint(i.ToString()));
            Waypoint[] specialWaypoints = new Waypoint[] { new Waypoint("Flare", WaypointFlag.Flare), new Waypoint("Home", WaypointFlag.Home), new Waypoint("Reinf.", WaypointFlag.Reinforce) };
            Waypoint[] waypoints = playerWaypoints.Concat(generalWaypoints).Concat(specialWaypoints).ToArray();
            BasicSection basicSection = new BasicSection();
            basicSection.SetDefault();
            IEnumerable<HouseType> houseTypes = HouseTypes.GetTypes();
            basicSection.Player = houseTypes.Where(h => (h.Flags & HouseTypeFlag.Special) == HouseTypeFlag.None).First().Name;
            basicSection.BasePlayer = HouseTypes.None.Name;
            string[] cellEventTypes = new[]
            {
                EventTypes.EVENT_PLAYER_ENTERED,
                EventTypes.EVENT_ANY,
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
            Size mapSize = !megaMap ? Constants.MaxSize : Constants.MaxSizeMega;
            Map = new Map(basicSection, null, mapSize, typeof(House), houseTypes,
                null, TheaterTypes.GetTypes(), TemplateTypes.GetTypes(),
                TerrainTypes.GetTypes(), OverlayTypes.GetTypes(), SmudgeTypes.GetTypes(Globals.ConvertCraters),
                EventTypes.GetTypes(), cellEventTypes, unitEventTypes, structureEventTypes, terrainEventTypes,
                ActionTypes.GetTypes(), cellActionTypes, unitActionTypes, structureActionTypes, terrainActionTypes,
                MissionTypes.GetTypes(), MissionTypes.MISSION_GUARD, MissionTypes.MISSION_STOP, MissionTypes.MISSION_HARVEST,
                MissionTypes.MISSION_UNLOAD, DirectionTypes.GetMainTypes(), DirectionTypes.GetAllTypes(), InfantryTypes.GetTypes(),
                UnitTypes.GetTypes(Globals.DisableAirUnits), BuildingTypes.GetTypes(), TeamMissionTypes.GetTypes(),
                fullTechnoTypes, waypoints, 4, 0, 0, movieTypes, movieEmpty, themeTypes, themeEmpty, Constants.TiberiumValue, 0);
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

        public virtual void New(string theater)
        {
            Map.Theater = Map.TheaterTypes.Where(t => t.Equals(theater)).FirstOrDefault() ?? Map.TheaterTypes.FirstOrDefault() ?? TheaterTypes.Desert;
            Map.TopLeft = new Point(1, 1);
            Map.Size = Map.Metrics.Size - new Size(2, 2);
            Map.BasicSection.Name = Constants.EmptyMapName;
            UpdateBasePlayerHouse();
        }

        public virtual IEnumerable<string> Load(string path, FileType fileType)
        {
            return Load(path, fileType, false);
        }

        protected List<string> Load(string path, FileType fileType, bool forSole)
        {
            INI ini = new INI();
            List<string> errors = new List<string>();
            bool modified = false;
            bool forceSingle = false;
            Byte[] iniBytes;
            switch (fileType)
            {
                case FileType.INI:
                case FileType.BIN:
                    string iniPath = fileType == FileType.INI ? path : Path.ChangeExtension(path, ".ini");
                    string binPath = fileType == FileType.BIN ? path : Path.ChangeExtension(path, ".bin");
                    if (!File.Exists(iniPath))
                    {
                        // Should never happen; this gets filtered out in the game type detection.
                        throw new ApplicationException("Cannot find an ini file to load for " + Path.GetFileName(path) + ".");
                    }
                    iniBytes = File.ReadAllBytes(iniPath);
                    ParseIniContent(ini, iniBytes, forSole);
                    forceSingle = !forSole && SinglePlayRegex.IsMatch(Path.GetFileNameWithoutExtension(path));
                    errors.AddRange(LoadINI(ini, forceSingle, ref modified));
                    if (!File.Exists(binPath))
                    {
                        errors.Add(String.Format("No .bin file found for file '{0}'. Using empty map.", Path.GetFileName(path)));
                        modified = true;
                        Map.Templates.Clear();
                    }
                    else
                    {
                        using (BinaryReader binReader = new BinaryReader(new FileStream(binPath, FileMode.Open, FileAccess.Read)))
                        {
                            long mapLen = binReader.BaseStream.Length;
                            if ((!isMegaMap && mapLen == 0x2000) || (isMegaMap && mapLen % 4 == 0))
                            {
                                errors.AddRange(!isMegaMap ? LoadBinaryClassic(binReader, ref modified) : LoadBinaryMega(binReader, ref modified));
                            }
                            else
                            {
                                errors.Add(String.Format("'{0}' does not have the correct size for a " + this.GameInfo.Name + " .bin file.", Path.GetFileName(binPath)));
                                modified = true;
                                Map.Templates.Clear();
                            }
                        }
                    }
                    break;
                case FileType.MEG:
                case FileType.PGM:
                    using (Megafile megafile = new Megafile(path))
                    {
                        string iniFile = megafile.Where(p => Path.GetExtension(p).ToLower() == ".ini").FirstOrDefault();
                        string binFile = megafile.Where(p => Path.GetExtension(p).ToLower() == ".bin").FirstOrDefault();
                        if (iniFile == null || binFile == null)
                        {
                            throw new ApplicationException("Cannot find the necessary files inside the " + Path.GetFileName(path) + " archive.");
                        }
                        using (Stream iniStream = megafile.OpenFile(iniFile))
                        using (Stream binStream = megafile.OpenFile(binFile))
                        using (BinaryReader binReader = new BinaryReader(binStream))
                        {
                            iniBytes = iniStream.ReadAllBytes();
                            ParseIniContent(ini, iniBytes, forSole);
                            errors.AddRange(LoadINI(ini, false, ref modified));
                            long mapLen = binReader.BaseStream.Length;
                            if ((!isMegaMap && mapLen == 0x2000) || (isMegaMap && mapLen % 4 == 0))
                            {
                                errors.AddRange(!isMegaMap ? LoadBinaryClassic(binReader, ref modified) : LoadBinaryMega(binReader, ref modified));
                            }
                            else
                            {
                                errors.Add(String.Format("'{0}' does not have the correct size for a " + this.GameInfo.Name + " .bin file.", Path.GetFileName(binFile)));
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

        private void ParseIniContent(INI ini, Byte[] iniBytes, Boolean forSole)
        {
            Encoding encDOS = Encoding.GetEncoding(437);
            string iniText = encDOS.GetString(iniBytes);
            Encoding encUtf8 = new UTF8Encoding(false, false);
            string iniTextUtf8 = encUtf8.GetString(iniBytes);
            // Sole Survivor does not have 2-stage ROAD; ROAD acts as teleporter in Sole.
            if (!forSole)
            {
                iniText = FixRoad2Load(iniText);
            }
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
                // Remastered one-line "Text" briefing from [Briefing] section.
                INISection briefSectionUtf8 = utf8Ini.Sections["Briefing"];
                INISection briefSectionDos = ini.Sections["Briefing"];
                // Use UTF-8 briefing if present. Restore content behind semicolon cut off as 'comment'.
                if (briefSectionUtf8 != null && briefSectionDos != null)
                {
                    if (briefSectionUtf8.Keys.Contains("Text"))
                    {
                        string comment = briefSectionUtf8.GetComment("Text");
                        String briefing = briefSectionUtf8.Keys["Text"];
                        if (comment != null)
                        {
                            briefing = briefing + comment;
                        }
                        briefSectionDos.Keys["Text"] = briefing;
                    }
                    else
                    {
                        int line = 1;
                        string lineStr = line.ToString();
                        while (briefSectionDos.Contains(lineStr))
                        {
                            string comment = briefSectionDos.GetComment(lineStr);
                            if (comment != null)
                            {
                                briefSectionDos[lineStr] = briefSectionDos[lineStr] + comment;
                            }
                            line++;
                            lineStr = line.ToString();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This detects and transforms double lines of the ROAD type in the Overlay section in the ini
        /// to single lines of the dummy type used to represent the second state of ROAD in the editor.
        /// </summary>
        /// <param name="iniReader">Stream reader to read from.</param>
        /// <returns>The ini file as string, with all double ROAD overlay lines replaced by the dummy Road2 type.</returns>
        protected string FixRoad2Load(string iniText)
        {
            // ROAD's second state can only be accessed by applying ROAD overlay to the same cell twice.
            // This can be achieved by saving its Overlay line twice in the ini file. However, this is
            // technically against the format's specs, since the game requests a list of all keys and
            // then finds the FIRST entry for each key. This means the contents of the second line never
            // get read, but those of the first are simply applied twice. For ROAD, however, this is
            // exactly what we want to achieve to unlock its second state, so the bug doesn't matter.
            OverlayType[] fixTypes = Map.OverlayTypes.Where(ov => (ov.Flag & OverlayTypeFlag.RoadSpecial) != OverlayTypeFlag.None && ov.ForceTileNr == 1 && ov.GraphicsSource != ov.Name).ToArray();
            foreach (OverlayType road2 in fixTypes)
            {
                string[] iniTextArr = iniText.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
                Dictionary<int, int> foundAmounts = new Dictionary<int, int>();
                Dictionary<int, string> cellTypes = new Dictionary<int, string>();
                Regex overlayRegex = new Regex("^\\s*(\\d+)\\s*=\\s*([a-zA-Z0-9]+)\\s*$", RegexOptions.IgnoreCase);
                string roadname = road2.GraphicsSource;
                string road2name = road2.Name.ToUpper();
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
                if (foundAmounts.All(k => k.Value == 1) && !cellTypes.Values.Contains(road2name, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
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
                iniText = sb.ToString();
            }
            return iniText;
        }

        protected virtual List<string> LoadINI(INI ini, bool forceSoloMission, ref bool modified)
        {
            return LoadINI(ini, forceSoloMission, false, ref modified);
        }

        protected List<string> LoadINI(INI ini, bool forceSoloMission, bool forSole, ref bool modified)
        {
            List<string> errors = new List<string>();
            Map.BeginUpdate();
            BasicSection basic = (BasicSection)Map.BasicSection;
            INISection basicSection = INITools.ParseAndLeaveRemainder(ini, "Basic", Map.BasicSection, new MapContext(Map, false));
            if (basicSection != null)
            {
                char[] cutfrom = { ';', '(' };
                string[] toAddRem = movieTypesAdditional.Select(vid => GeneralUtils.TrimRemarks(vid, true, cutfrom)).ToArray();
                const string remark = " (Classic only)";
                basic.Intro = GeneralUtils.AddRemarks(basic.Intro, movieEmpty, true, toAddRem, remark);
                basic.Brief = GeneralUtils.AddRemarks(basic.Brief, movieEmpty, true, toAddRem, remark);
                basic.Action = GeneralUtils.AddRemarks(basic.Action, movieEmpty, true, toAddRem, remark);
                basic.Win = GeneralUtils.AddRemarks(basic.Win, movieEmpty, true, toAddRem, remark);
                basic.Win2 = GeneralUtils.AddRemarks(basic.Win2, movieEmpty, true, toAddRem, remark);
                basic.Win3 = GeneralUtils.AddRemarks(basic.Win3, movieEmpty, true, toAddRem, remark);
                basic.Win4 = GeneralUtils.AddRemarks(basic.Win4, movieEmpty, true, toAddRem, remark);
                basic.Lose = GeneralUtils.AddRemarks(basic.Lose, movieEmpty, true, toAddRem, remark);
            }
            Map.BasicSection.Player = Map.HouseTypes.Where(t => t.Equals(Map.BasicSection.Player)).FirstOrDefault()?.Name ?? Map.HouseTypes.First().Name;
            // Digest. Seems to exist in some console maps.
            ini.Sections.Remove("Digest");
            // Map info
            string theaterStr = ini["Map"]?.TryGetValue("Theater") ?? String.Empty;
            INISection mapSection = INITools.ParseAndLeaveRemainder(ini, "Map", Map.MapSection, new MapContext(Map, false));
            if (!this.Map.TheaterTypes.Any(thr => String.Equals(thr.Name, theaterStr, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add(String.Format("Theater \"{0}\" could not be found. Defaulting to \"{1}\".", theaterStr, Map.Theater));
                modified = true;
            }
            // Also clear megamap indicator.
            if (mapSection.Remove("Version") && mapSection.Keys.Count == 0)
                ini.Sections.Remove(mapSection.Name);
            Map.MapSection.FixBounds();
#if DEBUG
            //MessageBox.Show("Graphics loaded");
#endif
            string th = Map.Theater.Name;
            bool skipSoleStuff = forSole && Globals.NoOwnedObjectsInSole;
            INISection briefingSection = ini.Sections["Briefing"];
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
                    string lineStr;
                    bool addSpace = false;
                    while (briefingSection.Keys.Contains(lineStr = line.ToString()))
                    {
                        string briefLine = briefingSection[lineStr].Trim();
                        // C&C95 v1.06 line break format. Unlike RA's '@' system, this only works at the end of the line.
                        bool hasBreak = briefLine.EndsWith("##");
                        if (hasBreak)
                        {
                            briefLine = briefLine.Substring(0, briefLine.Length - 2);
                        }
                        if (addSpace)
                        {
                            briefLines.Append(" ");
                        }
                        briefLines.Append(briefLine.TrimEnd());
                        if (hasBreak)
                        {
                            briefLines.AppendLine();
                        }
                        addSpace = !hasBreak;
                        line++;
                    }
                    Map.BriefingSection.Briefing = briefLines.ToString();
                }
                briefingSection.Remove("Text");
                briefingSection.RemoveWhere(k => Regex.IsMatch(k, "^\\d+$"));
                if (briefingSection.Keys.Count == 0)
                {
                    ini.Sections.Remove(briefingSection.Name);
                }
            }
            INISection steamSection = ini.Sections.Extract("Steam");
            if (steamSection != null)
            {
                // Ignore any errors in this.
                INI.ParseSection(new MapContext(Map, false), steamSection, Map.SteamSection, true);
            }
            INISection teamTypesSection = ini.Sections.Extract("TeamTypes");
            // Make case insensitive dictionary of teamtype missions.
            Dictionary<string, TeamMission> teamMissionTypes = Enumerable.ToDictionary(TeamMissionTypes.GetTypes(), t => t.Mission, StringComparer.OrdinalIgnoreCase);
            if (teamTypesSection != null)
            {
                foreach (KeyValuePair<string, string> kvp in teamTypesSection)
                {
                    try
                    {
                        if (kvp.Key.Length > 8)
                        {
                            errors.Add(string.Format("TeamType '{0}' has a name that is longer than 8 characters. This will not be corrected by the loading process, but should be addressed, since it can make the teams fail to read correctly, and might even crash the game.", kvp.Key));
                        }
                        TeamType teamType = new TeamType { Name = kvp.Key };
                        List<string> tokens = kvp.Value.Split(',').ToList();
                        teamType.House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault();
                        if (teamType.House == null)
                        {
                            HouseType defHouse = Map.HouseTypes.First();
                            errors.Add(string.Format("Teamtype '{0}' references unknown house '{1}'; clearing to '{2}'.", kvp.Key, tokens[0], defHouse.Name));
                            modified = true;
                            teamType.House = defHouse;
                        }
                        tokens.RemoveAt(0);
                        teamType.IsRoundAbout = int.Parse(tokens[0]) != 0; tokens.RemoveAt(0);
                        teamType.IsLearning = int.Parse(tokens[0]) != 0; tokens.RemoveAt(0);
                        teamType.IsSuicide = int.Parse(tokens[0]) != 0; tokens.RemoveAt(0);
                        teamType.IsAutocreate = int.Parse(tokens[0]) != 0; tokens.RemoveAt(0);
                        teamType.IsMercenary = int.Parse(tokens[0]) != 0; tokens.RemoveAt(0);
                        teamType.RecruitPriority = int.Parse(tokens[0]); tokens.RemoveAt(0);
                        teamType.MaxAllowed = byte.Parse(tokens[0]); tokens.RemoveAt(0);
                        teamType.InitNum = byte.Parse(tokens[0]); tokens.RemoveAt(0);
                        teamType.Fear = byte.Parse(tokens[0]); tokens.RemoveAt(0);
                        int numClasses = int.Parse(tokens[0]); tokens.RemoveAt(0);
                        for (int i = 0; i < Math.Min(Globals.MaxTeamClasses, numClasses); ++i)
                        {
                            string[] classTokens = tokens[0].Split(':'); tokens.RemoveAt(0);
                            if (classTokens.Length == 2)
                            {
                                ITechnoType type = fullTechnoTypes.Where(t => t.Name.Equals(classTokens[0], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                                byte count;
                                if (!byte.TryParse(classTokens[1], out count))
                                    count = 1;
                                if (type != null)
                                {
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
                        int numMissions = int.Parse(tokens[0]); tokens.RemoveAt(0);
                        for (int i = 0; i < Math.Min(Globals.MaxTeamMissions, numMissions); ++i)
                        {
                            string[] missionTokens = tokens[0].Split(':'); tokens.RemoveAt(0);
                            if (missionTokens.Length == 2)
                            {
                                // fix mission case sensitivity issues.
                                TeamMission mission;
                                teamMissionTypes.TryGetValue(missionTokens[0], out mission);
                                if (mission != null)
                                {
                                    // I'll allow any value for Attack Tarcom; you never know.
                                    if (!Int32.TryParse(missionTokens[1], out int arg) || (mission != TeamMissionTypes.AttackTarcom && arg < 0))
                                    {
                                        errors.Add(string.Format("Team '{0}', orders index {1} ('{2}') has an incorrect value '{3}'. Reverting to 0.", kvp.Key, i, mission.Mission, missionTokens[1]));
                                        modified = true;
                                        arg = 0;
                                    }
                                    teamType.Missions.Add(new TeamTypeMission { Mission = mission, Argument = arg });
                                }
                                else
                                {
                                    errors.Add(string.Format("Team '{0}' references unknown orders '{1}'. Orders ignored.", kvp.Key, missionTokens[0]));
                                    modified = true;
                                }
                            }
                            else
                            {
                                errors.Add(string.Format("Team '{0}' has wrong number of tokens for orders index {1} (expecting 2).", kvp.Key, i));
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
                        errors.Add(string.Format("Teamtype '{0}' has errors and can't be parsed: {1}.", kvp.Key, ex.Message));
                        modified = true;
                    }
                }
            }
            INISection triggersSection = ini.Sections.Extract("Triggers");
            List<Trigger> triggers = new List<Trigger>();
            if (triggersSection != null)
            {
                foreach (KeyValuePair<string, string> kvp in triggersSection)
                {
                    try
                    {
                        if (kvp.Key.Length > 4)
                        {
                            errors.Add(string.Format("Trigger '{0}' has a name that is longer than 4 characters. This will not be corrected by the loading process, but should be addressed, since it can make the triggers fail to link correctly to objects and cell triggers, and might even crash the game.", kvp.Key));
                        }
                        string[] tokens = kvp.Value.Split(',');
                        if (tokens.Length >= 5)
                        {
                            Trigger trigger = new Trigger { Name = kvp.Key };
                            string eventType = tokens[0];
                            if (EventTypes.EVENT_NONE.Equals(eventType, StringComparison.OrdinalIgnoreCase))
                            {
                                eventType = EventTypes.EVENT_NONE;
                            }
                            else
                            {
                                eventType = EventTypes.GetTypes().FirstOrDefault(evt => evt.Equals(eventType, StringComparison.OrdinalIgnoreCase)) ?? EventTypes.EVENT_NONE;
                                if (EventTypes.EVENT_NONE.Equals(eventType, StringComparison.OrdinalIgnoreCase))
                                {
                                    errors.Add(string.Format("Trigger '{0}' references unknown event '{1}'. Reverted to 'None'.", kvp.Key, tokens[0]));
                                    modified = true;
                                }
                            }
                            trigger.Event1.EventType = eventType;
                            string actionType = tokens[1];
                            if (ActionTypes.ACTION_NONE.Equals(actionType, StringComparison.OrdinalIgnoreCase))
                            {
                                actionType = ActionTypes.ACTION_NONE;
                            }
                            else
                            {
                                actionType = ActionTypes.GetTypes().FirstOrDefault(act => act.Equals(actionType, StringComparison.OrdinalIgnoreCase)) ?? ActionTypes.ACTION_NONE;
                                if (ActionTypes.ACTION_NONE.Equals(actionType, StringComparison.OrdinalIgnoreCase))
                                {
                                    errors.Add(string.Format("Trigger '{0}' references unknown action '{1}'. Reverted to 'None'.", kvp.Key, tokens[4]));
                                    modified = true;
                                }
                            }
                            trigger.Action1.ActionType = actionType;
                            trigger.Event1.Data = long.Parse(tokens[2]);
                            string house = tokens[3];
                            if (Model.House.IsEmpty(house))
                            {
                                house = Model.House.None;
                            }
                            else
                            {
                                house = Map.HouseTypes.FirstOrDefault(t => t.Name.Equals(house, StringComparison.OrdinalIgnoreCase))?.Name ?? Model.House.None;
                                if (Model.House.IsEmpty(house))
                                {
                                    errors.Add(string.Format("Trigger '{0}' references unknown House '{1}'; clearing to 'None'.", kvp.Key, tokens[4]));
                                    modified = true;
                                }
                            }
                            trigger.House = house;
                            string team = tokens[4];
                            if (TeamType.IsEmpty(tokens[4]))
                            {
                                team = TeamType.None;
                            }
                            else
                            {
                                team = Map.TeamTypes.FirstOrDefault(tt => tt.Name.Equals(tokens[4], StringComparison.OrdinalIgnoreCase))?.Name ?? TeamType.None;
                                if (TeamType.IsEmpty(tokens[4]))
                                {
                                    errors.Add(string.Format("Trigger '{0}' references unknown teamtype '{1}'. Reverted to 'None'.", kvp.Key, tokens[4]));
                                    modified = true;
                                }
                            }
                            trigger.Action1.Team = team;
                            trigger.PersistentType = TriggerPersistentType.Volatile;
                            if (tokens.Length >= 6)
                            {
                                trigger.PersistentType = (TriggerPersistentType)int.Parse(tokens[5]);
                            }
                            triggers.Add(trigger);
                        }
                        else
                        {
                            errors.Add(string.Format("Trigger '{0}' has too few tokens (expecting at least 5).", kvp.Key));
                            modified = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add(string.Format("Trigger '{0}' has errors and can't be parsed: {1}.", kvp.Key, ex.Message));
                        modified = true;
                    }
                }
            }
            // Sort
            ExplorerComparer comparer = new ExplorerComparer();
            triggers.Sort((x, y) => comparer.Compare(x.Name, y.Name));
            Dictionary<string, string> caseTrigs = Trigger.None.Yield().Concat(triggers.Select(t => t.Name)).ToDictionary(t => t, StringComparer.OrdinalIgnoreCase);
            HashSet<string> checkCellTrigs = Map.FilterCellTriggers(triggers).Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            HashSet<string> checkUnitTrigs = Trigger.None.Yield().Concat(Map.FilterUnitTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            HashSet<string> checkStrcTrigs = Trigger.None.Yield().Concat(Map.FilterStructureTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            HashSet<string> checkTerrTrigs = Trigger.None.Yield().Concat(Map.FilterTerrainTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            INISection smudgeSection = ini.Sections.Extract("Smudge");
            if (smudgeSection != null)
            {
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
            INISection infantrySection = ini.Sections.Extract("Infantry");
            if (infantrySection != null)
            {
                foreach (KeyValuePair<string, string> kvp in infantrySection)
                {
                    if (skipSoleStuff)
                    {
                        int amount = infantrySection.Count();
                        bool isOne = amount == 1;
                        errors.Add(string.Format("Owned objects in Sole Survivor are disabled. {0} [Infantry] {1} skipped.", amount, isOne ? "entry was" : "entries were"));
                        modified = true;
                        break;
                    }
                    string[] tokens = kvp.Value.Split(',');
                    if (tokens.Length == 8)
                    {
                        InfantryType infantryType = Map.InfantryTypes.Where(t => t.Equals(tokens[1])).FirstOrDefault();
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
                                        errors.Add(string.Format("Infantry '{0}' on cell {1}, sub-position {2} links to unknown trigger '{3}'; clearing trigger.", infantryType.Name, cell, stoppingPos, tokens[7]));
                                        modified = true;
                                        tokens[7] = Trigger.None;
                                    }
                                    else if (!checkUnitTrigs.Contains(tokens[7]))
                                    {
                                        errors.Add(string.Format("Infantry '{0}' on cell {1}, sub-position {2} links to trigger '{3}' which does not contain an event applicable to infantry; clearing trigger.", infantryType.Name, cell, stoppingPos, tokens[7]));
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
                                        HouseType defHouse = Map.HouseTypes.First();
                                        errors.Add(string.Format("Infantry '{0}' on cell {1}, sub-position {2} references unknown house '{3}'; clearing to '{4}'.", inf.Type.Name, cell, stoppingPos, tokens[0], defHouse.Name));
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
            INISection unitsSection = ini.Sections.Extract("Units");
            if (unitsSection != null)
            {
                foreach (KeyValuePair<string, string> kvp in unitsSection)
                {
                    if (skipSoleStuff)
                    {
                        int amount = unitsSection.Count();
                        bool isOne = amount == 1;
                        errors.Add(string.Format("Owned objects in Sole Survivor are disabled. {0} [Units] {1} skipped.", amount, isOne ? "entry was" : "entries were"));
                        modified = true;
                        break;
                    }
                    string[] tokens = kvp.Value.Split(',');
                    if (tokens.Length == 7)
                    {
                        UnitType unitType = Map.UnitTypes.Where(t => t.IsGroundUnit && t.Equals(tokens[1])).FirstOrDefault();
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
                            Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault() ?? Map.GetDefaultMission(unitType),
                        };
                        if (newUnit.House == null)
                        {
                            HouseType defHouse = Map.HouseTypes.First();
                            errors.Add(string.Format("Unit '{0}' on cell {1} references unknown house '{2}'; clearing to '{3}'.", newUnit.Type.Name, cell, tokens[0], defHouse.Name));
                            modified = true;
                            newUnit.House = defHouse;
                        }
                        // "Rescue" and "Unload" both make the MCV deploy, but "Rescue" looks very strange in the editor, so we keep only one of them and convert the other.
                        if (MissionTypes.MISSION_RESCUE.Equals(tokens[5], StringComparison.InvariantCultureIgnoreCase) && newUnit.Type.Equals(UnitTypes.MCV))
                        {
                            newUnit.Mission = MissionTypes.MISSION_UNLOAD;
                        }
                        if (Map.Technos.Add(cell, newUnit))
                        {
                            if (!caseTrigs.ContainsKey(tokens[6]))
                            {
                                errors.Add(string.Format("Unit '{0}' on cell {1} links to unknown trigger '{2}'; clearing trigger.", unitType.Name, cell, tokens[6]));
                                modified = true;
                                newUnit.Trigger = Trigger.None;
                            }
                            else if (!checkUnitTrigs.Contains(tokens[6]))
                            {
                                errors.Add(string.Format("Unit '{0}' on cell {1} links to trigger '{2}' which does not contain an event applicable to units; clearing trigger.", unitType.Name, cell, tokens[6]));
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
            // Classic game does not support this, so I'm leaving this out by default.
            // It is always extracted, so it doesn't end up with the "extra sections"
            INISection aircraftSection = ini.Sections.Extract("Aircraft");
            if (aircraftSection != null)
            {
                foreach (KeyValuePair<string, string> kvp in aircraftSection)
                {
                    if (Globals.DisableAirUnits || skipSoleStuff)
                    {
                        int amount = aircraftSection.Count();
                        bool isOne = amount == 1;
                        string disabledObj = skipSoleStuff ? "Owned objects in Sole Survivor" : "Aircraft";
                        errors.Add(string.Format("{0} are disabled. {1} [Aircraft] {2} skipped.", disabledObj, amount, isOne ? "entry was" : "entries were"));
                        modified = true;
                        break;
                    }
                    string[] tokens = kvp.Value.Split(',');
                    if (tokens.Length == 6)
                    {
                        UnitType aircraftType = Map.UnitTypes.Where(t => t.IsAircraft && t.Equals(tokens[1])).FirstOrDefault();
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
                        if (newUnit.House == null)
                        {
                            HouseType defHouse = Map.HouseTypes.First();
                            errors.Add(string.Format("Aircraft '{0}' on cell {1} references unknown house '{2}'; clearing to '{3}'.", newUnit.Type.Name, cell, tokens[0], defHouse.Name));
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
            INISection structuresSection = ini.Sections.Extract("Structures");
            if (structuresSection != null)
            {
                foreach (KeyValuePair<string, string> kvp in structuresSection)
                {
                    if (skipSoleStuff)
                    {
                        int amount = structuresSection.Count();
                        bool isOne = amount == 1;
                        errors.Add(string.Format("Owned objects in Sole Survivor are disabled. {0} [Structures] {1} skipped.", amount, isOne ? "entry was" : "entries were"));
                        modified = true;
                        break;
                    }
                    string[] tokens = kvp.Value.Split(',');
                    if (tokens.Length == 6)
                    {
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
                        Building newBld = new Building()
                        {
                            Type = buildingType,
                            House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                            Strength = strength,
                            Direction = DirectionType.GetDirectionType(dirValue, Map.BuildingDirectionTypes),
                        };
                        if (newBld.House == null)
                        {
                            HouseType defHouse = Map.HouseTypes.First();
                            errors.Add(string.Format("Structure '{0}' on cell {1} references unknown house '{2}'; clearing to '{3}'.", buildingType.Name, cell, tokens[0], defHouse.Name));
                            modified = true;
                            newBld.House = defHouse;
                        }
                        if (Map.Buildings.CanAdd(cell, newBld) && Map.Technos.CanAdd(cell, newBld, newBld.Type.BaseOccupyMask))
                        {
                            Map.Buildings.Add(cell, newBld);
                            if (!caseTrigs.ContainsKey(tokens[5]))
                            {
                                errors.Add(string.Format("Structure '{0}' on cell {1} links to unknown trigger '{2}'; clearing trigger.", buildingType.Name, cell, tokens[5]));
                                modified = true;
                                newBld.Trigger = Trigger.None;
                            }
                            else if (!checkStrcTrigs.Contains(tokens[5]))
                            {
                                errors.Add(string.Format("Structure '{0}' on cell {1} links to trigger '{2}' which does not contain an event applicable to structures; clearing trigger.", buildingType.Name, cell, tokens[5]));
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
                            Map.CheckBuildingBlockingCell(cell, buildingType, errors, ref modified);
                        }
                    }
                    else
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
                    }
                }
            }
            INISection baseSection = ini.Sections["Base"];
            string baseCountStr = baseSection != null ? baseSection.TryGetValue("Count") : null;
            if (baseSection != null)
            {
                baseSection.Remove("Count");
                if (!Int32.TryParse(baseCountStr, out int baseCount))
                {
                    if (skipSoleStuff)
                    {
                        // Ignore error. Just indicate it's skipped.
                        errors.Add(string.Format("Owned objects in Sole Survivor are disabled. [Base] section is skipped."));
                    }
                    else
                    {
                        errors.Add(string.Format("Base count '{0}' is not a valid integer.", baseCountStr));
                    }
                    modified = true;
                }
                else if (skipSoleStuff && baseCount > 0)
                {
                    errors.Add(string.Format("Owned objects in Sole Survivor are disabled. [Base] section is skipped."));
                    modified = true;
                }
                else
                {
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
                        int coord;
                        if (!int.TryParse(tokens[1], out coord))
                        {
                            errors.Add(string.Format("Coordinates for base rebuild entry '{0}', structure '{1}' cannot be parsed; value: '{2}'; skipping.", key, buildingType.Name, tokens[1]));
                            modified = true;
                            continue;
                        }
                        Point location = new Point((coord >> 8) & 0x7F, (coord >> 24) & 0x7F);
                        if (Map.Buildings.OfType<Building>().Where(x => x.Location == location && x.Occupier.Type.ID == buildingType.ID).FirstOrDefault().Occupier is Building building)
                        {
                            building.BasePriority = curPriorityVal;
                        }
                        else
                        {
                            if (!Map.Metrics.GetCell(location, out int cell))
                            {
                                errors.Add(string.Format("Base rebuild entry '{0}', structure '{1}' cannot be placed at cell '{2}'; skipping.", key, buildingType.Name, cell));
                            }
                            Building toRebuild = new Building()
                            {
                                Type = buildingType,
                                House = HouseTypes.None,
                                Strength = 256,
                                Direction = DirectionTypes.North,
                                BasePriority = curPriorityVal,
                                IsPrebuilt = false
                            };
                            if (!Map.Buildings.CanAdd(location, toRebuild) || !Map.Technos.CanAdd(location, toRebuild, toRebuild.Type.BaseOccupyMask))
                            {
                                Map.CheckBuildingBlockingCell(cell, buildingType, errors, ref modified, key);
                            }
                            else
                            {
                                Map.Buildings.Add(location, toRebuild);
                            }
                        }
                        curPriorityVal++;
                    }
                    foreach (KeyValuePair<string, string> kvp in baseSection)
                    {
                        errors.Add(string.Format("Invalid base rebuild priority entry '{0}={1}'.", kvp.Key, kvp.Value));
                        modified = true;
                    }
                }
                // Clean out and leave; might contain addon keys.
                baseSection.Remove("Count");
                baseSection.RemoveWhere(k => Regex.IsMatch(k, "^\\d{3}$"));
                if (baseSection.Count == 0)
                {
                    ini.Sections.Remove(baseSection.Name);
                }
            }
            INISection terrainSection = ini.Sections.Extract("Terrain");
            if (terrainSection != null)
            {
                foreach (KeyValuePair<string, string> kvp in terrainSection)
                {
                    int cell;
                    if (!int.TryParse(kvp.Key, out cell))
                    {
                        errors.Add(string.Format("Cell for terrain cannot be parsed. Key: '{0}', value: '{1}'; skipping.", kvp.Key, kvp.Value));
                        modified = true;
                        continue;
                    }
                    string[] tokens = kvp.Value.Split(',');
                    TerrainType terrainType = Map.TerrainTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault();
                    if (terrainType != null)
                    {
                        if (Globals.FilterTheaterObjects && terrainType.Theaters != null && !terrainType.Theaters.Contains(th))
                        {
                            errors.Add(string.Format("Terrain '{0}' is not available in the set theater; skipping.", terrainType.Name));
                            modified = true;
                            continue;
                        }
                        Terrain newTerr = new Terrain
                        {
                            Type = terrainType
                        };
                        if (Map.Technos.Add(cell, newTerr))
                        {
                            if (tokens.Length > 1)
                            {
                                if (!caseTrigs.ContainsKey(tokens[1]))
                                {
                                    errors.Add(string.Format("Terrain '{0}' on cell {1} links to unknown trigger '{2}'; clearing trigger.", terrainType.Name, cell, tokens[1]));
                                    modified = true;
                                    newTerr.Trigger = Trigger.None;
                                }
                                else if (!checkTerrTrigs.Contains(tokens[1]))
                                {
                                    errors.Add(string.Format("Terrain '{0}' on cell {1} links to trigger '{2}' which does not contain an event applicable to terrain; clearing trigger.", terrainType.Name, cell, tokens[1]));
                                    modified = true;
                                    newTerr.Trigger = Trigger.None;
                                }
                                else
                                {
                                    // Adapt to same case
                                    newTerr.Trigger = caseTrigs[tokens[1]];
                                }
                            }
                        }
                        else
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
                                    errors.Add(string.Format("Terrain '{0}' placed on cell {1} overlaps unknown techno at cell {2}; skipping.", terrainType.Name, cell, reportCell));
                                    modified = true;
                                }
                                else
                                {
                                    errors.Add(string.Format("Terrain '{0}' placed on cell {1} crosses outside the map bounds; skipping.", terrainType.Name, cell));
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
            }
            INISection overlaySection = ini.Sections.Extract("Overlay");
            if (overlaySection != null)
            {
                int lastLine = Map.Metrics.Height - 1;
                foreach (KeyValuePair<string, string> kvp in overlaySection)
                {
                    int cell;
                    if (!int.TryParse(kvp.Key, out cell))
                    {
                        errors.Add(string.Format("Cell for overlay cannot be parsed. Key: '{0}', value: '{1}'; skipping.", kvp.Key, kvp.Value));
                        modified = true;
                        continue;
                    }
                    if (!Map.Metrics.GetLocation(cell, out Point point))
                    {
                        errors.Add(string.Format("Cell for overlay is not inside the map bounds. Key: '{0}', value: '{1}'; skipping.", kvp.Key, kvp.Value));
                        modified = true;
                        continue;
                    }
                    if (point.Y == 0 || point.Y == lastLine)
                    {
                        errors.Add(string.Format("Overlay can not be placed on the first and or last lines of the map. Key: '{0}', value: '{1}'; skipping.", kvp.Key, kvp.Value));
                        modified = true;
                        continue;
                    }
                    OverlayType overlayType = Map.OverlayTypes.Where(t => t.Equals(kvp.Value)).FirstOrDefault();
                    if (overlayType != null)
                    {
                        if (Globals.FilterTheaterObjects && !overlayType.ExistsInTheater)
                        {
                            errors.Add(string.Format("Overlay '{0}' is not available in the set theater; skipping.", overlayType.Name));
                            modified = true;
                            continue;
                        }
                        Map.Overlay[cell] = new Overlay { Type = overlayType, Icon = 0 };
                    }
                    else
                    {
                        errors.Add(string.Format("Overlay '{0}' references unknown overlay.", kvp.Value));
                        modified = true;
                    }
                }
            }
            INISection waypointsSection = ini.Sections.Extract("Waypoints");
            if (waypointsSection != null)
            {
                foreach (KeyValuePair<string, string> kvp in waypointsSection)
                {
                    if (int.TryParse(kvp.Key, out int waypoint))
                    {
                        if (int.TryParse(kvp.Value, out int cell))
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
                            errors.Add(string.Format("Waypoint {0} has invalid cell '{1}' (expecting integer).", waypoint, kvp.Value));
                            modified = true;
                        }
                    }
                    else
                    {
                        errors.Add(string.Format("Invalid waypoint '{0}' (expecting integer).", kvp.Key));
                        modified = true;
                    }
                }
            }
            INISection cellTriggersSection = ini.Sections.Extract("CellTriggers");
            if (cellTriggersSection != null)
            {
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
                            errors.Add(string.Format("Cell trigger {0} is outside map bounds; skipping.", cell));
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
            Dictionary<string, string> correctedEdges = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string edge in Globals.MapEdges)
                correctedEdges.Add(edge, edge);
            string defaultEdge = Globals.MapEdges.FirstOrDefault() ?? string.Empty;
            foreach (Model.House house in Map.Houses)
            {
                if ((house.Type.Flags & HouseTypeFlag.Special) != HouseTypeFlag.None)
                {
                    continue;
                }
                House gameHouse = (House)house;
                INISection houseSection = INITools.ParseAndLeaveRemainder(ini, gameHouse.Type.Name, gameHouse, new MapContext(Map, false));
                gameHouse.Enabled = houseSection != null;
            }
            UpdateBasePlayerHouse();
            ClearUnusedTriggerArguments(triggers);
            bool wasFixed;
            errors.AddRange(CheckTriggers(triggers, true, true, false, out _, true, out wasFixed));
            if (wasFixed)
            {
                modified = true;
            }
            // Won't trigger the notifications.
            Map.Triggers.Clear();
            Map.Triggers.AddRange(triggers);
            Map.TeamTypes.Sort((x, y) => comparer.Compare(x.Name, y.Name));
            extraSections = ini.Sections.Clone();
            bool switchedToSolo = !forSole && forceSoloMission && !basic.SoloMission
                && ((triggers.Any(t => t.Action1.ActionType == ActionTypes.ACTION_WIN) && triggers.Any(t => t.Action1.ActionType == ActionTypes.ACTION_LOSE))
                    || triggers.Any(t => t.Event1.EventType == EventTypes.EVENT_ANY && t.Action1.ActionType == ActionTypes.ACTION_WINLOSE));
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

        protected IEnumerable<string> LoadBinaryClassic(BinaryReader reader, ref bool modified)
        {
            List<string> errors = new List<string>();
            Map.Templates.Clear();
            TemplateType[] templateTypes = GetTemplateTypesAsArray();
            for (int y = 0; y < Map.Metrics.Height; ++y)
            {
                for (int x = 0; x < Map.Metrics.Width; ++x)
                {
                    byte typeValue = reader.ReadByte();
                    byte iconValue = reader.ReadByte();
                    TemplateType templateType = ChecKTemplateType(templateTypes, typeValue, iconValue, x, y, errors, ref modified);
                    Map.Templates[y, x] = (templateType != null) ? new Template { Type = templateType, Icon = iconValue } : null;
                }
            }
            return errors;
        }

        protected IEnumerable<string> LoadBinaryMega(BinaryReader reader, ref bool modified)
        {
            List<string> errors = new List<string>();
            Map.Templates.Clear();
            TemplateType[] templateTypes = GetTemplateTypesAsArray();
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
                TemplateType templateType = ChecKTemplateType(templateTypes, typeValue, iconValue, x, y, errors, ref modified);
                Map.Templates[y,x] = (templateType != null) ? new Template { Type = templateType, Icon = iconValue } : null;
            }
            return errors;
        }

        protected TemplateType[] GetTemplateTypesAsArray()
        {
            TemplateType[] templateTypes = new TemplateType[0x100];
            foreach (TemplateType tt in Map.TemplateTypes)
            {
                templateTypes[tt.ID] = tt;
            }
            return templateTypes;
        }

        protected TemplateType ChecKTemplateType(TemplateType[] templateTypes, int typeValue, int iconValue, int x, int y, List<string> errors, ref bool modified)
        {
            TemplateType templateType = templateTypes[typeValue];
            // Prevent loading of illegal tiles.
            if (templateType != null)
            {
                if ((templateType.Flag & TemplateTypeFlag.Clear) != TemplateTypeFlag.None || (templateType.Flag & TemplateTypeFlag.Group) == TemplateTypeFlag.Group)
                {
                    // No explicitly set Clear terrain allowed. Also no explicitly set versions allowed of the "group" dummy entries.
                    templateType = null;
                }
                else if (!templateType.ExistsInTheater && Globals.FilterTheaterObjects)
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
                else if (!templateType.IsRandom && templateType.IconMask != null && !templateType.IconMask[iconValue / templateType.IconWidth, iconValue % templateType.IconWidth])
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

        public virtual bool Save(string path, FileType fileType)
        {
            return Save(path, fileType, null, false);
        }

        public virtual bool Save(string path, FileType fileType, Bitmap customPreview, bool dontResavePreview)
        {
            return Save(path, fileType, false, customPreview, dontResavePreview);
        }

        public bool Save(string path, FileType fileType, bool forSole, Bitmap customPreview, bool dontResavePreview)
        {
            string errors = Validate(false, forSole);
            if (errors != null)
            {
                MessageBox.Show(errors, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            string iniPath = fileType == FileType.INI ? path : Path.ChangeExtension(path, ".ini");
            string binPath = fileType == FileType.BIN ? path : Path.ChangeExtension(path, ".bin");
            Encoding dos437 = Encoding.GetEncoding(437);
            Encoding utf8 = new UTF8Encoding(false, false);
            byte[] linebreak = utf8.GetBytes("\r\n");
            INI ini = new INI();
            switch (fileType)
            {
                case FileType.INI:
                case FileType.BIN:
                    SaveINI(ini, fileType, path);
                    using (FileStream iniStream = new FileStream(iniPath, FileMode.Create))
                    using (BinaryWriter iniWriter = new BinaryWriter(iniStream))
                    {
                        // Use '\n' in proprocessing for simplicity. WriteMultiEncoding will use full line breaks.
                        string iniText = forSole ? ini.ToString("\n") : FixRoad2Save(ini, "\n");
                        GeneralUtils.WriteMultiEncoding(iniText.Split('\n'), iniWriter, dos437, utf8, new[] { ("Steam", null), ("Briefing", "Text"), ("Basic", "Name"), ("Basic", "Author") }, linebreak);
                    }
                    using (FileStream binStream = new FileStream(binPath, FileMode.Create))
                    using (BinaryWriter binWriter = new BinaryWriter(binStream))
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
                    if (!forSole && !Map.BasicSection.SoloMission && (!Globals.UseClassicFiles || !Globals.ClassicProducesNoMetaFiles))
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
                    using (MemoryStream iniStream = new MemoryStream())
                    using (MemoryStream binStream = new MemoryStream())
                    using (MemoryStream tgaStream = new MemoryStream())
                    using (MemoryStream jsonStream = new MemoryStream())
                    using (BinaryWriter iniWriter = new BinaryWriter(iniStream))
                    using (BinaryWriter binWriter = new BinaryWriter(binStream))
                    using (JsonTextWriter jsonWriter = new JsonTextWriter(new StreamWriter(jsonStream)))
                    using (MegafileBuilder megafileBuilder = new MegafileBuilder(String.Empty, path))
                    {
                        string iniText = forSole ? ini.ToString("\n") : FixRoad2Save(ini, "\n");
                        GeneralUtils.WriteMultiEncoding(iniText.Split('\n'), iniWriter, dos437, utf8, new[] { ("Steam", null), ("Briefing", "Text"), ("Basic", "Name"), ("Basic", "Author") }, linebreak);
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
                        string iniFile = Path.ChangeExtension(Path.GetFileName(path), ".ini").ToUpper();
                        string binFile = Path.ChangeExtension(Path.GetFileName(path), ".bin").ToUpper();
                        string tgaFile = Path.ChangeExtension(Path.GetFileName(path), ".tga").ToUpper();
                        string jsonFile = Path.ChangeExtension(Path.GetFileName(path), ".json").ToUpper();
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
        /// <param name="lineEnd">Line end.</param>
        protected string FixRoad2Save(INI ini, string lineEnd)
        {
            // ROAD's second state can only be accessed by applying ROAD overlay to the same cell twice.
            // This can be achieved by saving its Overlay line twice in the ini file. However, this is
            // technically against the format's specs, since the game requests a list of all keys and
            // then finds the FIRST entry for each key. This means the contents of the second line never
            // get read, but those of the first are simply applied twice. For ROAD, however, this is
            // exactly what we want to achieve to unlock its second state, so the bug doesn't matter.
            string iniString1 = ini.ToString("\n");
            OverlayType[] fixTypes = Map.OverlayTypes.Where(ov => (ov.Flag & OverlayTypeFlag.RoadSpecial) != OverlayTypeFlag.None && ov.ForceTileNr > 0 && ov.GraphicsSource != ov.Name).ToArray();
            foreach (OverlayType road2 in fixTypes)
            {
                string roadLine = "=" + road2.GraphicsSource.ToUpperInvariant() + lineEnd;
                Regex roadDetect = new Regex("^\\s*(\\d+)\\s*=\\s*" + road2.Name + "\\s*$", RegexOptions.IgnoreCase);
                StringBuilder output = new StringBuilder();
                string[] iniString = iniString1.Split('\n');
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
                        output.Append(newRoad);
                        output.Append(newRoad);
                    }
                    else
                    {
                        output.Append(currLine);
                        output.Append(lineEnd);
                    }
                }
                iniString1 = output.ToString();
            }
            return iniString1;
        }

        protected virtual void SaveINI(INI ini, FileType fileType, string fileName)
        {
            if (extraSections != null)
            {
                ini.Sections.AddRange(extraSections.Clone());
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
            BasicSection basic = (BasicSection)Map.BasicSection;
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
            INISection basicSection = INITools.FillAndReAdd(ini, "Basic", (BasicSection)Map.BasicSection, new MapContext(Map, false), true);
            return basicSection;
        }

        protected INISection SaveIniMap(INI ini)
        {
            Map.MapSection.FixBounds();
            INISection mapSection = INITools.FillAndReAdd(ini, "Map", Map.MapSection, new MapContext(Map, false), true);
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
            string briefText = (Map.BriefingSection.Briefing ?? String.Empty).Replace('\t', ' ').Trim('\r', '\n', ' ').Replace("\r\n", "\n").Replace("\r", "\n");
            // Remove duplicate spaces,
            briefText = Regex.Replace(briefText, " +", " ");
            // Remove spaces around line breaks. (Test this!)
            briefText = Regex.Replace(briefText, " *\\n *", "\n");
            if (briefText.Length == 0)
            {
                return null;
            }
            briefingSection["Text"] = briefText.Replace("\n", "@");
            if (Globals.WriteClassicBriefing)
            {
                if (briefText.Length > Constants.MaxBriefLengthClassic)
                {
                    briefText = briefText.Substring(0, Constants.MaxBriefLengthClassic);
                }
                string[] lines = briefText.Split('\n');
                List<string> finalLines = new List<string>();
                int last = lines.Length - 1;
                for (int i = 0; i < lines.Length; ++i)
                {
                    string line = lines[i].Trim();
                    if (i != last)
                    {
                        line += "##";
                    }
                    if (line.Length <= Constants.BriefLineCutoffClassic)
                    {
                        finalLines.Add(line);
                        continue;
                    }
                    string[] splitLine = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    int wordIndex = 0;
                    while (wordIndex < splitLine.Length)
                    {
                        StringBuilder sb = new StringBuilder();
                        // Always allow initial word
                        int nextLength = 0;
                        while (nextLength < Constants.BriefLineCutoffClassic && wordIndex < splitLine.Length)
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
            if (oldSection != null)
            {
                foreach (KeyValuePair<String, String> kvp in oldSection)
                {
                    if (!briefingSection.Contains(kvp.Key))
                    {
                        briefingSection[kvp.Key] = kvp.Value;
                    }
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
            INISection cellTriggersSection = ini.Sections.Add("CellTriggers");
            foreach (var (cell, cellTrigger) in Map.CellTriggers.OrderBy(t => t.Cell))
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
            INISection teamTypesSection = ini.Sections.Add("TeamTypes");
            foreach (TeamType teamType in Map.TeamTypes.OrderBy(t => t.Name.ToUpperInvariant()))
            {
                string[] classes = teamType.Classes
                    .Select(c => string.Format("{0}:{1}", c.Type.Name.ToUpperInvariant(), c.Count))
                    .ToArray();
                string[] missions = teamType.Missions
                    .Select(m => string.Format("{0}:{1}", m.Mission.Mission, m.Argument))
                    .ToArray();
                List<string> tokens = new List<string>
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
            INISection triggersSection = ini.Sections.Add("Triggers");
            foreach (Trigger trigger in Map.Triggers.OrderBy(t => t.Name.ToUpperInvariant()))
            {
                if (string.IsNullOrEmpty(trigger.Name))
                {
                    continue;
                }
                List<string> tokens = new List<string>
                {
                    trigger.Event1.EventType,
                    trigger.Action1.ActionType,
                    trigger.Event1.Data.ToString(),
                    string.IsNullOrEmpty(trigger.House) ? House.None : trigger.House,
                    string.IsNullOrEmpty(trigger.Action1.Team) ? TeamType.None : trigger.Action1.Team,
                    ((int)trigger.PersistentType).ToString()
                };
                triggersSection[trigger.Name] = string.Join(",", tokens);
            }
            return triggersSection;
        }

        protected INISection SaveIniWaypoints(INI ini)
        {
            INISection waypointsSection = ini.Sections.Add("Waypoints");
            for (int i = Map.Waypoints.Length - 1; i >= 0; --i)
            {
                Waypoint waypoint = Map.Waypoints[i];
                waypointsSection[i.ToString()] = waypoint.Cell.GetValueOrDefault(-1).ToString();
            }
            return waypointsSection;
        }

        protected INISection SaveIniBase(INI ini, bool dummy)
        {
            INISection baseSectionOld = ini.Sections.Extract("Base");
            if (baseSectionOld != null)
            {
                baseSectionOld.Remove("Count");
                baseSectionOld.RemoveWhere(k => Regex.IsMatch(k, "^\\d{3}$"));
            }
            INISection baseSection = ini.Sections.Add("Base");
            if (dummy)
            {
                baseSection["Count"] = "0";
            }
            else
            {
                var baseBuildings = Map.Buildings.OfType<Building>().Where(x => x.Occupier.BasePriority >= 0).OrderByDescending(x => x.Occupier.BasePriority).ToArray();
                int baseIndex = baseBuildings.Length - 1;
                foreach (var (location, building) in baseBuildings)
                {
                    string key = baseIndex.ToString("D3");
                    baseIndex--;
                    baseSection[key] = string.Format("{0},{1}",
                        building.Type.Name.ToUpperInvariant(),
                        ((location.Y & 0x7F) << 24) | ((location.X & 0x7F) << 8)
                    );
                }
                baseSection["Count"] = baseBuildings.Length.ToString();
            }
            if (baseSectionOld != null)
            {
                foreach (KeyValuePair<string, string> kvp in baseSectionOld)
                {
                    baseSection[kvp.Key] = kvp.Value;
                }
            }
            return baseSection;
        }

        protected INISection SaveIniInfantry(INI ini)
        {
            INISection infantrySection = ini.Sections.Add("Infantry");
            int infantryIndex = 0;
            foreach (var (location, infantryGroup) in Map.Technos.OfType<InfantryGroup>().OrderBy(i => Map.Metrics.GetCell(i.Location)))
            {
                for (int i = 0; i < infantryGroup.Infantry.Length; ++i)
                {
                    Infantry infantry = infantryGroup.Infantry[i];
                    if (infantry == null)
                    {
                        continue;
                    }
                    if (!Map.Metrics.GetCell(location, out int cell))
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
                        string.IsNullOrEmpty(infantry.Mission) ? "Guard" : infantry.Mission,
                        infantry.Direction.ID,
                        infantry.Trigger
                    );
                }
            }
            return infantrySection;
        }

        protected INISection SaveIniStructures(INI ini)
        {
            INISection structuresSection = ini.Sections.Add("Structures");
            int structureIndex = 0;
            foreach (var (location, building) in Map.Buildings.OfType<Building>().Where(b => b.Occupier.IsPrebuilt).OrderBy(b => Map.Metrics.GetCell(b.Location)))
            {
                if (!Map.Metrics.GetCell(location, out int cell))
                {
                    continue;
                }
                string key = structureIndex.ToString("D3");
                structureIndex++;
                structuresSection[key] = string.Format("{0},{1},{2},{3},{4},{5}",
                    building.House.Name,
                    building.Type.Name.ToUpperInvariant(),
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
            INISection unitsSection = ini.Sections.Add("Units");
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
                    string.IsNullOrEmpty(unit.Mission) ? "Guard" : unit.Mission,
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
            INISection aircraftSection = ini.Sections.Add("Aircraft");
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
                    string.IsNullOrEmpty(aircraft.Mission) ? "Guard" : aircraft.Mission
                );
            }
            return aircraftSection;
        }

        protected IEnumerable<INISection> SaveIniHouses(INI ini)
        {
            List<INISection> houseSections = new List<INISection>();
            foreach (Model.House house in Map.Houses.Where(h => (h.Type.Flags & HouseTypeFlag.Special) == HouseTypeFlag.None).OrderBy(h => h.Type.ID))
            {
                House gameHouse = (House)house;
                bool enabled = house.Enabled;
                string name = gameHouse.Type.Name;
                INISection houseSection = INITools.FillAndReAdd(ini, name, gameHouse, new MapContext(Map, false), enabled);
                // Current house is not in its own alliances list. Fix that.
                if (houseSection != null && !gameHouse.Allies.Contains(gameHouse.Type.ID))
                {
                    HashSet<String> allies = (houseSection.TryGetValue("Allies") ?? String.Empty)
                        .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
                    if (!allies.Contains(name))
                    {
                        allies.Add(name);
                        List<string> alliesBuild = new List<string>();
                        foreach (HouseType houseAll in Map.HouseTypesIncludingSpecials.Where(h => allies.Contains(h.Name)))
                        {
                            alliesBuild.Add(houseAll.Name);
                        }
                        houseSection["Allies"] = String.Join(",", alliesBuild.ToArray());
                    }
                    houseSections.Add(houseSection);
                }
            }
            return houseSections;
        }

        protected INISection SaveIniOverlay(INI ini)
        {
            INISection overlaySection = ini.Sections.Add("Overlay");
            Regex tiberium = new Regex("TI([0-9]|(1[0-2]))", RegexOptions.IgnoreCase);
            Random rd = new Random();
            foreach ((int cell, Overlay overlay) in Map.Overlay.OrderBy(o => o.Cell))
            {
                if (Map.IsIgnorableOverlay(overlay))
                {
                    continue;
                }
                string overlayName = overlay.Type.Name;
                if (tiberium.IsMatch(overlayName))
                    overlayName = "TI" + rd.Next(1, 13);
                overlaySection[cell.ToString()] = overlayName.ToUpperInvariant();
            }
            return overlaySection;
        }

        protected INISection SaveIniSmudge(INI ini)
        {
            INISection smudgeSection = ini.Sections.Add("Smudge");
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
            return smudgeSection;
        }

        protected INISection SaveINITerrain(INI ini)
        {
            INISection terrainSection = ini.Sections.Add("Terrain");
            foreach (var (location, terrain) in Map.Technos.OfType<Terrain>().OrderBy(t => Map.Metrics.GetCell(t.Location)))
            {
                if (Map.Metrics.GetCell(location, out int cell))
                {
                    terrainSection[cell.ToString()] = string.Format("{0},{1}", terrain.Type.Name.ToUpperInvariant(), terrain.Trigger);
                }
            }
            return terrainSection;
        }

        protected void SaveBinaryClassic(BinaryWriter writer)
        {
            for (int y = 0; y < Map.Metrics.Height; ++y)
            {
                for (int x = 0; x < Map.Metrics.Width; ++x)
                {
                    Template template = Map.Templates[y, x];
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
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    Template template = Map.Templates[y, x];
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
            Map.GenerateMapPreview(this, renderAll).Save(stream);
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

        public virtual string Validate(bool forWarnings)
        {
            return Validate(forWarnings, false);
        }

        protected string Validate(bool forWarnings, bool forSole)
        {
            if (forWarnings)
            {
                // Check if map has name
                if (this.GameInfo.MapNameIsEmpty(this.Map.BasicSection.Name))
                {
                    return "Map name is empty. If you continue, the filename will be filled in as map name.";
                }
                return null;
            }
            StringBuilder sb = new StringBuilder("Error(s) during map validation:");
            bool ok = true;
            int numAircraft = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsAircraft).Count();
            int numBuildings = Map.Buildings.OfType<Building>().Where(x => x.Occupier.IsPrebuilt).Count();
            int numInfantry = Map.Technos.OfType<InfantryGroup>().Sum(item => item.Occupier.Infantry.Count(i => i != null));
            int numTerrain = Map.Technos.OfType<Terrain>().Count();
            int numUnits = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsGroundUnit).Count();
            int numStartPoints = Map.Waypoints.Count(w => (w.Flag & WaypointFlag.PlayerStart) == WaypointFlag.PlayerStart && w.Cell.HasValue
                && Map.Metrics.GetLocation(w.Cell.Value, out Point pt) && Map.Bounds.Contains(pt));
            int numBadPoints = Map.Waypoints.Count(w => (w.Flag & WaypointFlag.PlayerStart) == WaypointFlag.PlayerStart && w.Cell.HasValue
                && Map.Metrics.GetLocation(w.Cell.Value, out Point pt) && !Map.Bounds.Contains(pt));
            bool classicSole = forSole && Globals.RestrictSoleLimits;
            int maxAir = classicSole ? Constants.MaxAircraftClassic : Constants.MaxAircraft;
            int maxBld = classicSole ? Constants.MaxBuildingsClassic : Constants.MaxBuildings;
            int maxInf = classicSole ? Constants.MaxInfantryClassic : Constants.MaxInfantry;
            int maxTer = classicSole ? Constants.MaxTerrainClassic : Constants.MaxTerrain;
            int maxUni = classicSole ? Constants.MaxUnitsClassic : Constants.MaxUnits;
            bool noSoleSkip = !forSole || !Globals.NoOwnedObjectsInSole;
            if (!Globals.DisableAirUnits && numAircraft > maxAir && noSoleSkip && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of aircraft exceeded ({0} > {1})", numAircraft, maxAir));
                ok = false;
            }
            if (numBuildings > maxBld && noSoleSkip && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of structures exceeded ({0} > {1})", numBuildings, maxBld));
                ok = false;
            }
            if (numInfantry > maxInf && noSoleSkip && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of infantry exceeded ({0} > {1})", numInfantry, maxInf));
                ok = false;
            }
            if (numTerrain > maxTer && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of terrain objects exceeded ({0} > {1})", numTerrain, maxTer));
                ok = false;
            }
            if (numUnits > maxUni && noSoleSkip && Globals.EnforceObjectMaximums)
            {
                sb.Append(string.Format("\nMaximum number of units exceeded ({0} > {1})", numUnits, maxUni));
                ok = false;
            }
            // Ignore all further checks for Sole Survivor
            if (forSole)
            {
                return ok ? null : sb.ToString();
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
            IEnumerable<string> triggerErr = CheckTriggers(this.Map.Triggers, true, true, true, out fatal, false, out _);
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

        public virtual IEnumerable<string> AssessMapItems()
        {
            ExplorerComparer cmp = new ExplorerComparer();
            List<string> info = new List<string>();
            int numAircraft = Globals.DisableAirUnits ? 0 : Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsAircraft).Count();
            int numBuildings = Map.Buildings.OfType<Building>().Where(x => x.Occupier.IsPrebuilt).Count();
            int numInfantry = Map.Technos.OfType<InfantryGroup>().Sum(item => item.Occupier.Infantry.Count(i => i != null));
            int numTerrain = Map.Technos.OfType<Terrain>().Count();
            int numUnits = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsGroundUnit).Count();
            info.Add("Objects overview:");
            const string maximums = "Number of {0}: {1}. Maximum: {2}. Classic maximum: {3}.";
            if (!Globals.DisableAirUnits)
            {
                info.Add(String.Format(maximums, "aircraft", numAircraft, Constants.MaxAircraft, Constants.MaxAircraftClassic));
            }
            info.Add(String.Format(maximums, "structures", numBuildings, Constants.MaxBuildings, Constants.MaxBuildingsClassic));
            info.Add(String.Format(maximums, "infantry", numInfantry, Constants.MaxInfantry, Constants.MaxInfantryClassic));
            info.Add(String.Format(maximums, "terrain objects", numTerrain, Constants.MaxTerrain, Constants.MaxTerrainClassic));
            info.Add(String.Format(maximums, "units", numUnits, Constants.MaxUnits, Constants.MaxUnitsClassic));
            info.Add(String.Format(maximums, "team types", Map.TeamTypes.Count, Constants.MaxTeams, Constants.MaxTeamsClassic));
            info.Add(String.Format(maximums, "triggers", Map.Triggers.Count, Constants.MaxTriggers, Constants.MaxTriggersClassic));
            if (!Map.BasicSection.SoloMission)
            {
                info.Add(String.Empty);
                info.Add("Multiplayer info:");
                int startPoints = Map.Waypoints.Count(w => w.Cell.HasValue && (w.Flag & WaypointFlag.PlayerStart) == WaypointFlag.PlayerStart);
                info.Add(String.Format("Number of set starting points: {0}.", startPoints));
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
                            if (tr.Action1.Team == teamName &&
                                (tr.Action1.ActionType == ActionTypes.ACTION_CREATE_TEAM
                               || tr.Action1.ActionType == ActionTypes.ACTION_DESTROY_TEAM
                               || tr.Action1.ActionType == ActionTypes.ACTION_REINFORCEMENTS))
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
                string[] unusedTeams = Map.TeamTypes.Select(tm => tm.Name).Where(tn => !usedTeams.Contains(tn)).ToArray();
                Array.Sort(unusedTeams, cmp);
                string unusedTeamsStr = String.Join(", ", unusedTeams);
                foreach (Trigger tr in Map.Triggers)
                {
                    if (tr.Action1.ActionType == ActionTypes.ACTION_DZ)
                        usedWaypoints.Add(Enumerable.Range(0, Map.Waypoints.Length).Where(i => (Map.Waypoints[i].Flag & WaypointFlag.Flare) == WaypointFlag.Flare).First());
                }
                WaypointFlag toIgnore = WaypointFlag.Home | WaypointFlag.Reinforce;
                string unusedWaypointsStr = String.Join(", ", setWaypoints.OrderBy(w => w)
                    .Where(w => (Map.Waypoints[w].Flag & toIgnore) == WaypointFlag.None
                                && !usedWaypoints.Contains(w)).Select(w => Map.Waypoints[w].Name).ToArray());
                string unsetUsedWaypointsStr = String.Join(", ", usedWaypoints.OrderBy(w => w).Where(w => !setWaypoints.Contains(w)).Select(w => Map.Waypoints[w].Name).ToArray());
                string evalEmpty(string str)
                {
                    return String.IsNullOrEmpty(str) ? "-" : str;
                };
                info.Add(String.Empty);
                info.Add("Scripting remarks:");
                info.Add(string.Format("Unused team types: {0}", evalEmpty(unusedTeamsStr)));
                info.Add(string.Format("Placed waypoints not used in teams or triggers: {0}", evalEmpty(unusedWaypointsStr)));
                info.Add(string.Format("Empty waypoints used in teams or triggers: {0}", evalEmpty(unsetUsedWaypointsStr)));
            }
            return info;
        }

        private void ClearUnusedTriggerArguments(List<Trigger> triggers)
        {
            foreach (Trigger tr in triggers)
            {
                ClearUnusedEventArgs(tr.Event1);
                ClearUnusedActionArgs(tr.Action1);
            }
        }

        private void ClearUnusedEventArgs(TriggerEvent ev)
        {
            switch (ev.EventType)
            {
                case EventTypes.EVENT_NONE:
                case EventTypes.EVENT_PLAYER_ENTERED:
                case EventTypes.EVENT_DISCOVERED:
                case EventTypes.EVENT_ATTACKED:
                case EventTypes.EVENT_DESTROYED:
                case EventTypes.EVENT_ANY:
                case EventTypes.EVENT_HOUSE_DISCOVERED:
                case EventTypes.EVENT_UNITS_DESTROYED:
                case EventTypes.EVENT_BUILDINGS_DESTROYED:
                case EventTypes.EVENT_ALL_DESTROYED:
                case EventTypes.EVENT_NOFACTORIES:
                case EventTypes.EVENT_EVAC_CIVILIAN:
                    ev.Data = 0;
                    break;
                case EventTypes.EVENT_CREDITS:
                case EventTypes.EVENT_TIME:
                case EventTypes.EVENT_NBUILDINGS_DESTROYED:
                case EventTypes.EVENT_NUNITS_DESTROYED:
                case EventTypes.EVENT_BUILD:
                    break;
            }
        }
        private void ClearUnusedActionArgs(TriggerAction ac)
        {
            switch (ac.ActionType)
            {
                case ActionTypes.ACTION_NONE:
                case ActionTypes.ACTION_WIN:
                case ActionTypes.ACTION_LOSE:
                case ActionTypes.ACTION_BEGIN_PRODUCTION:
                case ActionTypes.ACTION_ALL_HUNT:
                case ActionTypes.ACTION_DZ:
                case ActionTypes.ACTION_AIRSTRIKE:
                case ActionTypes.ACTION_NUKE:
                case ActionTypes.ACTION_ION:
                case ActionTypes.ACTION_DESTROY_XXXX:
                case ActionTypes.ACTION_DESTROY_YYYY:
                case ActionTypes.ACTION_DESTROY_ZZZZ:
                case ActionTypes.ACTION_DESTROY_UUUU:
                case ActionTypes.ACTION_DESTROY_VVVV:
                case ActionTypes.ACTION_DESTROY_WWWW:
                case ActionTypes.ACTION_AUTOCREATE:
                case ActionTypes.ACTION_WINLOSE:
                case ActionTypes.ACTION_ALLOWWIN:
                    ac.Team = TeamType.None;
                    break;
                case ActionTypes.ACTION_CREATE_TEAM:
                case ActionTypes.ACTION_DESTROY_TEAM:
                case ActionTypes.ACTION_REINFORCEMENTS:
                    break;
            }
        }

        public virtual HashSet<string> GetHousesWithProduction()
        {
            HashSet<string> housesWithProd = new HashSet<string>();
            // Tiberian Dawn logic: find AIs with construction yard and Production trigger.
            HashSet<string> housesWithCY = new HashSet<string>();
            foreach ((_, Building bld) in Map.Buildings.OfType<Building>().Where(b => b.Occupier.IsPrebuilt &&
                (b.Occupier.House.Flags & HouseTypeFlag.Special) != HouseTypeFlag.None && (b.Occupier.Type.Flag & BuildingTypeFlag.Factory) == BuildingTypeFlag.Factory))
            {
                housesWithCY.Add(bld.House.Name);
            }
            foreach ((_, Unit unit) in Map.Technos.OfType<Unit>().Where(b =>
                "mcv".Equals(b.Occupier.Type.Name, StringComparison.InvariantCultureIgnoreCase)
                && "Unload".Equals(b.Occupier.Mission, StringComparison.InvariantCultureIgnoreCase)))
            {
                housesWithCY.Add(unit.House.Name);
            }
            string cellTriggerHouse = HouseTypes.GetClassicOpposingPlayer(Map.BasicSection.Player);
            foreach (Trigger trig in Map.Triggers)
            {
                string triggerHouse = trig.House;
                if (trig.Action1.ActionType == ActionTypes.ACTION_BEGIN_PRODUCTION)
                {
                    if (trig.Event1.EventType == EventTypes.EVENT_PLAYER_ENTERED)
                    {
                        // Either a cell trigger, or a building to capture. Scan for celltriggers.
                        if (housesWithCY.Contains(cellTriggerHouse))
                        {
                            foreach (var item in Map.CellTriggers)
                            {
                                if (trig.Equals(item.Value.Trigger))
                                {
                                    housesWithProd.Add(cellTriggerHouse);
                                    break;
                                }
                            }
                        }
                        // Scan for attached buildings to capture.
                        if (housesWithCY.Contains(triggerHouse))
                        {
                            foreach ((_, Building bld) in Map.Buildings.OfType<Building>())
                            {
                                if (trig.Equals(bld.Trigger))
                                {
                                    housesWithProd.Add(triggerHouse);
                                    break;
                                }
                            }
                        }
                    }
                    else if (housesWithCY.Contains(triggerHouse))
                    {
                        housesWithProd.Add(trig.House);
                    }
                }
            }
            return housesWithProd;
        }

        public virtual int[] GetRevealRadiusForWaypoints(bool forLargeReveal)
        {
            Waypoint[] waypoints = Map.Waypoints;
            int length = waypoints.Length;
            int[] flareRadius = new int[length];
            if (!forLargeReveal)
            {
                for (int i = 0; i < length; i++)
                {
                    Waypoint waypoint = waypoints[i];
                    if (waypoint != null && waypoint.Cell.HasValue && (waypoint.Flag & WaypointFlag.Flare) != WaypointFlag.None)
                    {
                        flareRadius[i] = Map.DropZoneRadius;
                    }
                }
            }
            return flareRadius;
        }

        public virtual IEnumerable<string> CheckTriggers(IEnumerable<Trigger> triggers, bool includeExternalData, bool prefixNames, bool fatalOnly, out bool fatal, bool fix, out bool wasFixed)
        {
            fatal = false;
            wasFixed = false;
            List<string> errors = new List<string>();
            List<string> curErrors = new List<string>();
            List<ITechno> mapTechnos = Map.GetAllTechnos().ToList();
            HouseType player = Map.HouseTypes.Where(t => t.Equals(Map.BasicSection.Player)).FirstOrDefault() ?? Map.HouseTypes.First();
            bool delXExists = false;
            bool delYExists = false;
            bool delZExists = false;
            bool delUExists = false;
            bool delVExists = false;
            bool delWExists = false;
            bool xxxxExists = false;
            bool yyyyExists = false;
            bool zzzzExists = false;
            bool uuuuExists = false;
            bool vvvvExists = false;
            bool wwwwExists = false;
            foreach (Trigger trigger in triggers)
            {
                string actionType = trigger.Action1.ActionType;
                if (String.Equals(trigger.Name, "XXXX", StringComparison.OrdinalIgnoreCase)) xxxxExists = true;
                if (String.Equals(trigger.Name, "YYYY", StringComparison.OrdinalIgnoreCase)) yyyyExists = true;
                if (String.Equals(trigger.Name, "ZZZZ", StringComparison.OrdinalIgnoreCase)) zzzzExists = true;
                if (actionType == ActionTypes.ACTION_DESTROY_XXXX) delXExists = true;
                if (actionType == ActionTypes.ACTION_DESTROY_YYYY) delYExists = true;
                if (actionType == ActionTypes.ACTION_DESTROY_ZZZZ) delZExists = true;
                if (!Globals.Ignore106Scripting)
                {
                    if (String.Equals(trigger.Name, "UUUU", StringComparison.OrdinalIgnoreCase)) uuuuExists = true;
                    if (String.Equals(trigger.Name, "VVVV", StringComparison.OrdinalIgnoreCase)) vvvvExists = true;
                    if (String.Equals(trigger.Name, "WWWW", StringComparison.OrdinalIgnoreCase)) wwwwExists = true;
                    if (actionType == ActionTypes.ACTION_DESTROY_UUUU) delUExists = true;
                    if (actionType == ActionTypes.ACTION_DESTROY_VVVV) delVExists = true;
                    if (actionType == ActionTypes.ACTION_DESTROY_WWWW) delWExists = true;
                }
            }
            foreach (Trigger trigger in triggers)
            {
                string team = trigger.Action1.Team;
                string trigName = trigger.Name;
                string prefix = prefixNames ? "Trigger \"" + trigName + "\": " : String.Empty;
                string event1 = trigger.Event1.EventType;
                string action1 = trigger.Action1.ActionType;
                bool noOwner = Model.House.IsEmpty(trigger.House);
                bool isPlayer = !noOwner && player.Equals(trigger.House);
                //bool playerIsNonstandard = !player.Equals(HouseTypes.Good) && !player.Equals(HouseTypes.Bad);
                //bool isGoodguy = String.Equals(trigger.House, HouseTypes.Good.Name, StringComparison.OrdinalIgnoreCase);
                //bool isBadguy = String.Equals(trigger.House, HouseTypes.Bad.Name, StringComparison.OrdinalIgnoreCase);
                bool isLinked = mapTechnos.Any(tech => String.Equals(trigName, tech.Trigger, StringComparison.OrdinalIgnoreCase));
                //bool isLinkedToStructs = mapTechnos.Any(tech => tech is Building && String.Equals(trigName, tech.Trigger, StringComparison.OrdinalIgnoreCase));
                //bool isLinkedToUnits = mapTechnos.Any(tech => (tech is Unit || tech is Infantry) && String.Equals(trigName, tech.Trigger, StringComparison.OrdinalIgnoreCase));
                //bool isLinkedToTrees = mapTechnos.Any(tech => (tech is Terrain) && String.Equals(trigName, tech.Trigger, StringComparison.OrdinalIgnoreCase));
                bool isCellTrig = Map.CellTriggers.Any(c => trigName.Equals(c.Value.Trigger, StringComparison.OrdinalIgnoreCase));
                bool hasTeam = !TeamType.IsEmpty(trigger.Action1.Team);
                bool isAnd = trigger.PersistentType == TriggerPersistentType.SemiPersistent;
                bool isLoop = trigger.PersistentType == TriggerPersistentType.Persistent;
                bool isDestroyableX = "xxxx".Equals(trigName, StringComparison.OrdinalIgnoreCase);
                bool isDestroyableY = "yyyy".Equals(trigName, StringComparison.OrdinalIgnoreCase);
                bool isDestroyableZ = "zzzz".Equals(trigName, StringComparison.OrdinalIgnoreCase);
                bool isDestroyableU = !Globals.Ignore106Scripting && "uuuu".Equals(trigName, StringComparison.OrdinalIgnoreCase);
                bool isDestroyableV = !Globals.Ignore106Scripting && "vvvv".Equals(trigName, StringComparison.OrdinalIgnoreCase);
                bool isDestroyableW = !Globals.Ignore106Scripting && "wwww".Equals(trigName, StringComparison.OrdinalIgnoreCase);
                bool isDestroyable = (isDestroyableX && delXExists) || (isDestroyableY && delYExists) || (isDestroyableZ && delZExists)
                     || (isDestroyableU && delUExists) || (isDestroyableV && delVExists) || (isDestroyableW && delWExists);

                // If this is null but hasTeam is true, something went wrong internally. Should never happen.
                TeamType teamObj = !hasTeam ? null : Map.TeamTypes.Where(tt => tt.Name == team).FirstOrDefault();

                if (event1 == EventTypes.EVENT_PLAYER_ENTERED && Model.House.IsEmpty(trigger.House))
                {
                    curErrors.Add(prefix + (fatalOnly ? String.Empty : "[FATAL] - ") + "A \"Player Enters\" trigger without a House set will cause a game crash.");
                    fatal = true;
                }
                if (!fatalOnly && event1 == EventTypes.EVENT_ATTACKED && !noOwner)
                {
                    if (isLinked && includeExternalData)
                    {
                        curErrors.Add(prefix + "\"Attacked\" triggers with a House set will trigger when that House is attacked. To make a trigger for checking if objects are attacked, leave the House empty.");
                    }
                    else if (!isPlayer)
                    {
                        curErrors.Add(prefix + "\"Attacked\" triggers with a House set will trigger when that House is attacked. However, this logic only works for the player's House.");
                    }
                }
                if (!fatalOnly && event1 == EventTypes.EVENT_DESTROYED && !noOwner && isAnd)
                {
                    curErrors.Add(prefix + "A \"Destroyed\" trigger with a House set and repeat status \"AND\" will never work, since a reference to the trigger will be added to the House Triggers list for that House, and there is nothing that can ever trigger that instance, making it impossible to clear all objectives required for the trigger to fire.");
                }
                if (!fatalOnly && event1 == EventTypes.EVENT_ANY && action1 != ActionTypes.ACTION_WINLOSE)
                {
                    curErrors.Add(prefix + "The \"Any\" event will trigger on literally anything that can happen to a linked object. It should normally only be used with the \"Cap=Win/Des=Lose\" action.");
                }
                if (!fatalOnly && event1 == EventTypes.EVENT_NBUILDINGS_DESTROYED && trigger.Event1.Data == 0)
                {
                    curErrors.Add(prefix + "The amount of buildings that needs to be destroyed is 0.");
                }
                if (!fatalOnly && event1 == EventTypes.EVENT_NUNITS_DESTROYED && trigger.Event1.Data == 0)
                {
                    curErrors.Add(prefix + "The amount of units that needs to be destroyed is 0.");
                }
                if (!fatalOnly && event1 == EventTypes.EVENT_BUILD && (trigger.Event1.Data < 0 || trigger.Event1.Data > Map.BuildingTypes.Max(bld => bld.ID)))
                {
                    string error = prefix + "Illegal building id \"" + trigger.Event1.Data + "\" for \"Built It\" event.";
                    if (fix)
                    {
                        trigger.Event1.Data = 0;
                        wasFixed = true;
                        error += " Fixed to \"0\" (" + (Map.BuildingTypes.FirstOrDefault(bld => bld.ID == 0)?.Name ?? String.Empty) + ").";
                    }
                    curErrors.Add(error);
                }

                // Action checks
                if (!fatalOnly && action1 == ActionTypes.ACTION_AIRSTRIKE && event1 == EventTypes.EVENT_PLAYER_ENTERED && !isPlayer && isCellTrig)
                {
                    curErrors.Add(prefix + "This will give the Airstrike to the House that activates the Celltrigger. This will grant the AI house linked to it periodic airstrikes that will only target structure.");
                }
                if (!fatalOnly && action1 == ActionTypes.ACTION_WINLOSE && event1 == EventTypes.EVENT_ANY && isAnd)
                {
                    curErrors.Add(prefix + "\"Any\" → \"Cap=Win/Des=Lose\" triggers don't function with existence status \"And\".");
                }
                if (!fatalOnly && action1 == ActionTypes.ACTION_ALLOWWIN && !isPlayer)
                {
                    curErrors.Add(prefix + "Each \"Allow Win\" trigger increases the \"win blockage\" on the House specified in the trigger, which prevents that house from winning until they are all cleared. However, since only the player can be blocked from winning, such triggers only work when they are linked to the player's House.");
                }
                if (!fatalOnly && action1 == ActionTypes.ACTION_ALLOWWIN && isLoop && !isDestroyable)
                {
                    curErrors.Add(prefix + "Each \"Allow Win\" trigger increases the \"win blockage\" on the House specified in the trigger, which prevents that house from winning until they are all cleared. The blockage is only cleared when the trigger is removed, which only happens either when it can no longer trigger, or when it is explicitly removed by a \"Destroy Trigger\" action. Since this trigger is set to execute \"on each triggering\", it will loop indefinitely and will never be removed.");
                }
                if (action1 == ActionTypes.ACTION_BEGIN_PRODUCTION)
                {
                    if ((event1 != EventTypes.EVENT_PLAYER_ENTERED && noOwner)
                        || (includeExternalData && event1 == EventTypes.EVENT_PLAYER_ENTERED && isLinked && !isCellTrig))
                    {
                        curErrors.Add(prefix + (fatalOnly ? String.Empty : "[FATAL] - ") + "The House set in a \"Production\" trigger determines the House that starts production, except in case of a celltrigger. Having no House will crash the game.");
                        fatal = true;
                    }
                    //else if (includeExternalData && trigger.Event1.EventType == EventTypes.EVENT_PLAYER_ENTERED && isCellTrig && playerIsNonstandard)
                    //{
                    //    curErrors.Add(prefix + "For a celltrigger, the House that starts production is always be the 'classic opposing House' of the player's House.");
                    //}
                }
                if (!hasTeam)
                {
                    switch (action1)
                    {
                        case ActionTypes.ACTION_REINFORCEMENTS:
                            curErrors.Add(prefix + (fatalOnly ? String.Empty : "[FATAL] - ") + "There is no team set to reinforce.");
                            fatal = true;
                            break;
                        case ActionTypes.ACTION_CREATE_TEAM:
                            curErrors.Add(prefix + (fatalOnly ? String.Empty : "[FATAL] - ") + "There is no team set to create.");
                            fatal = true;
                            break;
                        case ActionTypes.ACTION_DESTROY_TEAM:
                            curErrors.Add(prefix + (fatalOnly ? String.Empty : "[FATAL] - ") + "There is no team set to disband.");
                            fatal = true;
                            break;
                    }
                }
                /*/
                // Pending inclusion. Need more research. In general, though, this is probably wrong.
                if (!fatalOnly && hasTeam && action1 == ActionTypes.ACTION_CREATE_TEAM)
                {
                    foreach (TeamTypeClass cl in teamObj.Classes)
                    {
                        if (cl.Type.IsAircraft)
                        {
                            curErrors.Add(prefix + "Teams with air units can't be created using \"Create Team\".");
                            break;
                        }
                    }
                }
                //*/
                if (!fatalOnly && action1 == ActionTypes.ACTION_DESTROY_XXXX && !xxxxExists)
                {
                    curErrors.Add(prefix + "There is no trigger called 'XXXX' to destroy.");
                }
                if (!fatalOnly && action1 == ActionTypes.ACTION_DESTROY_YYYY && !yyyyExists)
                {
                    curErrors.Add(prefix + "There is no trigger called 'YYYY' to destroy.");
                }
                if (!fatalOnly && action1 == ActionTypes.ACTION_DESTROY_ZZZZ && !zzzzExists)
                {
                    curErrors.Add(prefix + "There is no trigger called 'ZZZZ' to destroy.");
                }
                if (!Globals.Ignore106Scripting)
                {
                    if (!fatalOnly && action1 == ActionTypes.ACTION_DESTROY_UUUU && !uuuuExists)
                    {
                        curErrors.Add(prefix + "There is no trigger called 'UUUU' to destroy.");
                    }
                    if (!fatalOnly && action1 == ActionTypes.ACTION_DESTROY_VVVV && !vvvvExists)
                    {
                        curErrors.Add(prefix + "There is no trigger called 'VVVV' to destroy.");
                    }
                    if (!fatalOnly && action1 == ActionTypes.ACTION_DESTROY_WWWW && !wwwwExists)
                    {
                        curErrors.Add(prefix + "There is no trigger called 'WWWW' to destroy.");
                    }
                }
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

        public virtual ITeamColor[] GetFlagColors()
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
            // Metallic light blue
            flagColors[6] = Globals.TheTeamColorManager["MULTI7"];
            // RA Purple
            flagColors[7] = Globals.TheTeamColorManager["MULTI8"];
            return flagColors;
        }

        public virtual bool IsLandUnitPassable(LandType landType)
        {
            switch (landType)
            {
                case LandType.Clear:
                case LandType.Beach:
                case LandType.Road:
                case LandType.Rough:
                    return true;
                case LandType.Rock:
                case LandType.Water:
                case LandType.River:
                    return false;
            }
            return false;
        }

        public virtual bool IsBoatPassable(LandType landType)
        {
            switch (landType)
            {
                case LandType.Water:
                case LandType.River:
                    return true;
                case LandType.Clear:
                case LandType.Road:
                case LandType.Rock:
                case LandType.Beach:
                case LandType.Rough:
                    return false;
            }
            return false;
        }

        public virtual bool IsBuildable(LandType landType)
        {
            switch (landType)
            {
                case LandType.Clear:
                case LandType.Road:
                    return true;
                case LandType.Beach:
                case LandType.Rock:
                case LandType.Water:
                case LandType.River:
                case LandType.Rough:
                    return false;
            }
            return false;
        }

        protected void BasicSection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Player":
                    UpdateBasePlayerHouse();
                    break;
                case "SoloMission":
                    Map.UpdateWaypoints();
                    break;
            }
        }

        protected void MapSection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Theater":
                    Map.InitTheater(GameInfo);
                    break;
            }
        }

        protected void UpdateBasePlayerHouse()
        {
            HouseType basePlayer = Map.HouseNone?.Type ?? Map.HouseTypes.FirstOrDefault();
            if (basePlayer == null)
            {
                return;
            }
            // Unused in TD, but whatever.
            Map.BasicSection.BasePlayer = basePlayer.Name;
            // Not really needed now BasePlayer House is always "None", but whatever.
            foreach (var (_, building) in Map.Buildings.OfType<Building>())
            {
                if (!building.IsPrebuilt)
                {
                    building.House = basePlayer;
                }
            }
        }

        #region IDisposable Support
        protected bool disposedValue = false;

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
