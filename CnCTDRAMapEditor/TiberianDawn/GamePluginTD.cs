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
using TGASharpLib;

namespace MobiusEditor.TiberianDawn
{
    public class GamePluginTD : IGamePlugin
    {
        protected const int multiStartPoints = 8;
        protected const int totalNumberedPoints = 25;

        private bool isLoading = false;
        protected bool isMegaMap = false;

        protected static readonly Regex singlePlayRegex = new Regex("^SC[A-LN-Z]\\d{2}\\d?[EWX][A-EL]$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected static readonly Regex movieRegex = new Regex(@"^(?:.*?\\)*(.*?)\.BK2$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        protected static readonly Regex baseKeyRegex = new Regex("^\\d{3}$", RegexOptions.Compiled);
        private readonly GameInfoTibDawn gameTypeInfo = new GameInfoTibDawn();

        protected static readonly IEnumerable<ITechnoType> fullTechnoTypes;

        protected const string MovieEmpty = "x";
        
        protected static readonly string DisabledObjExplSole = String.Format(IniParseConstants.ConsultManual, IniParseConstants.SettingNoOwnedObjSole);
        protected static readonly string DisabledObjSoleOne = IniParseConstants.ParseHandleSoleObjects + " " + IniParseConstants.EntrySkipped + " " + DisabledObjExplSole;
        protected static readonly string DisabledObjSoleMul = IniParseConstants.ParseHandleSoleObjects + " " + IniParseConstants.EntriesSkipped + " " + DisabledObjExplSole;
        protected readonly IEnumerable<string> movieTypes;

        protected readonly string[] HousesOnN64 = new[] { HouseTypes.None.Name, HouseTypes.Good.Name, HouseTypes.Bad.Name, HouseTypes.Neutral.Name};
        protected readonly string[] UnitsNotOnN64 = new[] { UnitTypes.Visceroid.Name, UnitTypes.SSM.Name, UnitTypes.Tric.Name, UnitTypes.Trex.Name, UnitTypes.Rapt.Name, UnitTypes.Steg.Name };
        protected readonly string[] BuildingsNotOnN64 = new[] { BuildingTypes.Tanker.Name };

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

        protected static readonly IEnumerable<string> movieTypesRemarksOld = new string[]
        {
            "BODYBAGS",
            "REFINT",
            "REFINERY",
            "SIZZLE",
            "SIZZLE2",
            "TRAILER",
            "TRTKIL_D",
        };


        protected IEnumerable<string> movieTypesRemarksNew = new string[0];

        protected const string themeEmpty = "No Theme";

        protected static readonly IEnumerable<string> themeTypes = new string[]
        {
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

        public static IEnumerable<string> Movies => movieTypesTD;
        public static IEnumerable<string> Themes => themeTypes;

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
            set
            {
                isDirty = value;
                feedBackHandler?.UpdateStatus();
            }
        }

        bool isEmpty;
        public bool Empty
        {
            get { return isEmpty; }
            set
            {
                isEmpty = value;
                feedBackHandler?.UpdateStatus();
            }
        }

        public FileType LoadedFileType { get; private set; }

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

        public virtual IEnumerable<string> SetExtraIniText(string extraIniText, out bool footPrintsChanged, out HashSet<Point> refreshPoints)
        {
            return SetExtraIniText(extraIniText, false, out footPrintsChanged, out refreshPoints);
        }

        public IEnumerable<string> TestSetExtraIniText(string extraIniText, bool isSolo, bool expansionEnabled, out bool footPrintsChanged)
        {
            return SetExtraIniText(extraIniText, true, out footPrintsChanged, out _);
        }

        public IEnumerable<string> SetExtraIniText(string extraIniText, bool forFootprintTest, out bool footPrintsChanged, out HashSet<Point> refreshPoints)
        {
            footPrintsChanged = false;
            INI extraTextIni = new INI();
            try
            {
                extraTextIni.Parse(extraIniText ?? String.Empty);
            }
            catch
            {
                refreshPoints = null;
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
            extraTextIni.Sections.Remove("MapPack");
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
                CleanBaseSection(extraTextIni, baseSec);
            }
            extraTextIni.Sections.Remove("Waypoints");
            extraTextIni.Sections.Remove("CellTriggers");
            foreach (Model.House house in Map.Houses)
            {
                INITools.ClearDataFrom(extraTextIni, house.Type.Name, (House)house);
            }
            extraSections = extraTextIni.Sections.Count == 0 ? null : extraTextIni.Sections;
            if (!Globals.ExpandTdScripting)
            {
                // Perhaps support the v1.06 bibs-disabling option in the future? Would need an entire bib-changing logic like RA has though.
            }
            refreshPoints = forFootprintTest ? null : new HashSet<Point>();
            ResetMissionRules(extraTextIni, forFootprintTest, out footPrintsChanged, refreshPoints);
            return null;
        }

        public static bool CheckForMegamap(INI iniContents)
        {
            return INITools.CheckForIniInfo(iniContents, "Map", "Version", "1");
        }

        public static bool CheckForEmbeddedMap(INI iniContents)
        {
            return INITools.CheckForIniInfo(iniContents, "MapPack");
        }

        public static bool CheckNormalMapFormat(byte[] binContents, Size mapSize, int maxTypeVal, int maxTypeValN64, out bool isN64)
        {
            isN64 = false;
            if (binContents == null)
            {
                return false;
            }
            int dataLen = binContents.Length;
            int mapWidth = mapSize.Width;
            int mapHeight = mapSize.Height;
            int mapLen = mapWidth * mapHeight;
            if (dataLen != mapLen * 2)
            {
                return false;
            }
            if (maxTypeValN64 > 0)
            {
                int normalType = 0;
                int n64Type = 0;
                for (int i = 0; i < dataLen; i += 2)
                {
                    short val = (short)(binContents[i] | (binContents[i + 1] << 8));
                    if (val == -1)
                    {
                        n64Type++;
                    }
                    else if (val == 0xFF)
                    {
                        normalType++;
                    }
                    else if (binContents[i] == 0 && binContents[i + 1] < 0x10)
                    {
                        // XCC Editor doesn't save using the "-1" shortcut but actually saves clear terrain per cell.
                        normalType++;
                    }
                }
                isN64 = n64Type > 0 && normalType == 0;
                if (isN64)
                {
                    for (int i = 0; i < dataLen; i += 2)
                    {
                        ushort val = (ushort)((binContents[i] << 8) + binContents[i + 1]);
                        if (val != 0xFFFF && val > maxTypeValN64)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            for (int i = 0; i < dataLen; i += 2)
            {
                byte typeValue = binContents[i];
                if (typeValue != 0xFF && typeValue > maxTypeVal)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CheckMegaMapFormat(byte[] binContents, Size mapSize, int maxTypeVal)
        {
            int dataLen = binContents.Length;
            if (dataLen % 4 != 0)
            {
                // Always divisible by four.
                return false;
            }
            int lastCell = -1;
            int mapWidth = mapSize.Width;
            int mapHeight = mapSize.Height;
            int mapLen = mapWidth * mapHeight;
            int readPos = 0;
            while (readPos < dataLen)
            {
                int cell = binContents[readPos] | (binContents[readPos+1] << 8);
                if (cell == lastCell)
                {
                    return false;
                }
                else if (cell < lastCell)
                {
                    return false;
                }
                if (cell > mapLen)
                {
                    return false;
                }
                byte typeValue = binContents[readPos+2];
                // byte iconValue = binContents[readPos+3];
                if (typeValue != 0xFF && typeValue > maxTypeVal)
                {
                    return false;
                }
                readPos += 4;
            }
            return true;
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
                string baseVariant = colGoodGuy?.Variant ?? "BASE_TEAM";
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
            LoadedFileType = FileType.None;
            // Readonly, so I'm splitting this off
            HashSet<string> movies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            // todo list these and hardcode them so the editor knows them even in pure classic mode.
            string moviesMegPath = Path.Combine(Globals.TheArchiveManager.LoadRoot, "MOVIES_TD.MEG");
            if (File.Exists(moviesMegPath))
            {
                using (Megafile megafile = new Megafile(moviesMegPath))
                {
                    foreach (string filename in megafile)
                    {
                        Match m = movieRegex.Match(filename);
                        if (m.Success)
                        {
                            movies.Add(m.Groups[1].ToString());
                        }
                    }
                }
            }
            int moviesFromMeg = movies.Count;
            movieTypesRemarksNew = movies.Where(mv => !movieTypesTD.Contains(mv) && !movieTypesRemarksOld.Contains(mv)).ToArray();

            // In case this isn't the remaster, just add all known videos.
            if (moviesFromMeg == 0)
            {
                movies.UnionWith(movieTypesTD);
            }
            else
            {
                movies.UnionWith(movieTypesRemarksOld);
            }
            List<string> finalMovies = movies.ToList();
            if (moviesFromMeg > 0 || !Globals.UseClassicFiles)
            {
                for (int i = 0; i < finalMovies.Count; ++i)
                {
                    finalMovies[i] = AddVideoRemarks(finalMovies[i]);
                }
            }
            finalMovies.Sort(new ExplorerComparer());
            finalMovies.Insert(0, MovieEmpty);
            movieTypes = finalMovies.ToArray();
        }

        public GamePluginTD(bool megaMap)
            : this(true, megaMap)
        {
        }

        public GamePluginTD(bool mapImage, bool megaMap)
            : this()
        {
            this.isMegaMap = megaMap;
            IEnumerable<Waypoint> playerWaypoints = Enumerable.Range(0, multiStartPoints).Select(i => new Waypoint(String.Format("P{0}", i), Waypoint.GetFlagForMpId(i)));
            IEnumerable<Waypoint> generalWaypoints = Enumerable.Range(multiStartPoints, totalNumberedPoints - multiStartPoints).Select(i => new Waypoint(i.ToString()));
            Waypoint[] specialWaypoints = new Waypoint[] { new Waypoint("Flare", "Flr.", WaypointFlag.Flare), new Waypoint("Home", WaypointFlag.Home), new Waypoint("Reinf.", "Rnf.", WaypointFlag.Reinforce) };
            Waypoint[] waypoints = playerWaypoints.Concat(generalWaypoints).Concat(specialWaypoints).ToArray();
            BasicSection basicSection = new BasicSection();
            basicSection.SetDefault();
            IEnumerable<HouseType> houseTypes = HouseTypes.GetTypes();
            basicSection.Player = houseTypes.Where(h => !h.IsSpecial).First().Name;
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
            Size mapSize = !megaMap ? gameTypeInfo.MapSize : gameTypeInfo.MapSizeMega;
            Map = new Map(basicSection, null, mapSize, typeof(House), houseTypes,
                null, TheaterTypes.GetTypes(), TemplateTypes.GetTypes(),
                TerrainTypes.GetTypes(), OverlayTypes.GetTypes(), SmudgeTypes.GetTypes(Globals.ConvertCraters),
                EventTypes.GetTypes(), cellEventTypes, unitEventTypes, structureEventTypes, terrainEventTypes,
                ActionTypes.GetTypes(), cellActionTypes, unitActionTypes, structureActionTypes, terrainActionTypes,
                MissionTypes.GetTypes(), MissionTypes.GetUnassignableTypes(), MissionTypes.MISSION_GUARD, MissionTypes.MISSION_STOP, MissionTypes.MISSION_HARVEST,
                MissionTypes.MISSION_UNLOAD, DirectionTypes.GetMainTypes(), DirectionTypes.GetAllTypes(), InfantryTypes.GetTypes(),
                UnitTypes.GetTypes(Globals.DisableAirUnits), BuildingTypes.GetTypes(false), TeamMissionTypes.GetTypes(),
                fullTechnoTypes, waypoints, movieTypes, MovieEmpty, themeEmpty.Yield().Concat(themeTypes), themeEmpty,
                4, 0, 0, Constants.DefaultResourceValue, 0);
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
            try
            {
                isLoading = true;
                LoadedFileType = FileType.INI;
                Map.Theater = Map.TheaterTypes.Where(t => t.Equals(theater)).FirstOrDefault() ?? Map.TheaterTypes.FirstOrDefault() ?? TheaterTypes.Desert;
                Map.TopLeft = new Point(1, 1);
                Map.Size = Map.Metrics.Size - new Size(2, 2);
                Map.BasicSection.Name = Constants.EmptyMapName;
                UpdateBasePlayerHouse();
                Empty = true;
            }
            finally
            {
                Empty = true;
                isLoading = false;
            }
        }

        public virtual IEnumerable<string> Load(string loadPath, string iniPath, byte[] iniContent, string binPath, byte[] binContent, ref FileType fileType)
        {
            return Load(loadPath, iniPath, iniContent, binPath, binContent, ref fileType, false);
        }

        protected List<string> Load(string loadPath, string iniPath, byte[] iniContent, string binPath, byte[] binContent, ref FileType fileType, bool forSole)
        {
            try
            {
                isLoading = true;
                INI ini = new INI();
                List<string> errors = new List<string>();
                bool modified = false;
                bool tryCheckSingle = false;
                bool checkN64 = !forSole && (fileType == FileType.I64 || fileType == FileType.B64 || (binPath != null && binPath.EndsWith(".map", StringComparison.OrdinalIgnoreCase)));
                ParseIniContent(ini, iniContent, forSole);
                tryCheckSingle = !forSole
                    && singlePlayRegex.IsMatch(Path.GetFileNameWithoutExtension(iniPath))
                    && !INITools.CheckForIniInfo(ini, "Basic", "SoloMission");
                errors.AddRange(LoadINI(ini, tryCheckSingle, fileType == FileType.MIX, ref modified));
                if (binContent != null)
                {
                    // if a bin file is present, prefer the external file.
                    ini.Sections.Remove("MapPack");
                    ReadMap(binContent, Path.GetFileName(binPath), errors, ref modified, checkN64, out bool isN64Map);
                    if ((fileType == FileType.INI || fileType == FileType.BIN) && isN64Map)
                    {
                        fileType = FileType.I64;
                    }
                }
                else if (INITools.CheckForIniInfo(ini, "MapPack"))
                {
                    ReadMapFromIni(ini, errors, ref modified);
                    fileType = FileType.MPR;
                }
                else
                {
                    string ext = fileType == FileType.I64 ? "map" : "bin";
                    errors.Add(String.Format("No .{0} file found for file '{1}'. Using empty map.", ext, Path.GetFileName(loadPath)));
                    Map.Templates.Clear();
                    fileType = FileType.INI;
                }
                LoadedFileType = fileType;
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

        private void ReadMap(byte[] fileContents, string filename, List<string> errors, ref bool modified, bool checkN64, out bool isN64Map)
        {
            int binLen = Map.Metrics.Width * Map.Metrics.Height * 2;
            isN64Map = false;
            List<string> err = new List<string>();
            bool mod = modified;
            CellGrid<Template> templates = ReadBinData(fileContents, filename, Map.Metrics.Size, err, ref mod);            
            if (checkN64 && fileContents.Length == binLen)
            {
                int normalType = 0;
                int n64Type = 0;
                for (int i = 0; i < binLen; i += 2)
                {
                    short val = (short)(fileContents[i] | (fileContents[i + 1] << 8));
                    if (val == -1)
                    {
                        n64Type++;
                    }
                    else if (val == 0xFF)
                    {
                        normalType++;
                    }
                    else if (fileContents[i] == 0 && fileContents[i + 1] < 0x10)
                    {
                        // XCC Editor doesn't save using the "-1" shortcut but actually saves clear terrain per cell.
                        normalType++;
                    }
                }
                if (normalType == 0 && n64Type > 0)
                {
                    List<string> errN64 = new List<string>();
                    bool modN64 = modified;
                    CellGrid<Template> templatesN64 = ReadN64MapData(fileContents, filename, errN64, ref modN64);
                    // Went better than identifying PC format; use this one.
                    if (templatesN64 != null && errN64.Count < err.Count)
                    {
                        isN64Map = true;
                        templates = templatesN64;
                        err = errN64;
                        mod = modN64;
                    }
                }
            }
            errors.AddRange(err);
            modified = mod;
            Map.Templates.Clear();
            for (int y = 0; y < Map.Metrics.Height; ++y)
            {
                for (int x = 0; x < Map.Metrics.Width; ++x)
                {
                    Map.Templates[y, x] = templates[y, x];
                }
            }
        }

        private void ReadMapFromIni(INI ini, List<string> errors, ref bool modified)
        {
            CellGrid<Template> templates = new CellGrid<Template>(Map.Metrics);
            IEnumerable<string> err = LoadMapPack(ini, templates, ref modified);
            errors.AddRange(err);
            Map.Templates.Clear();
            for (int y = 0; y < Map.Metrics.Height; ++y)
            {
                for (int x = 0; x < Map.Metrics.Width; ++x)
                {
                    Map.Templates[y, x] = templates[y, x];
                }
            }
        }

        private CellGrid<Template> ReadBinData(byte[] mapData, string filename, Size mapSize, List<string> errors, ref bool modified)
        {
            CellGrid<Template> templates = new CellGrid<Template>(new CellMetrics(mapSize));
            using (MemoryStream ms = new MemoryStream(mapData))
            using (BinaryReader binReader = new BinaryReader(ms, Encoding.UTF8, true))
            {
                long mapLen = mapData.Length;
                if (!isMegaMap && mapLen == 0x2000)
                {
                    errors.AddRange(LoadBinaryClassic(binReader, templates, ref modified));
                }
                else if (isMegaMap && mapLen % 4 == 0)
                {
                    errors.AddRange(LoadBinaryMega(binReader, templates, ref modified));
                }
                else
                {
                    errors.Add(String.Format("'{0}' does not have the correct size for a {1} .bin file.", filename, GameInfo.Name));
                    modified = true;
                }
            }
            return templates;
        }

        private CellGrid<Template> ReadN64MapData(byte[] mapData, string filename, List<string> errors, ref bool modified)
        {
            CellGrid<Template> templates = new CellGrid<Template>(Map.Metrics);
            long mapLen = mapData.Length;
            const int binLen = 0x2000;
            if (mapLen != binLen)
            {
                errors.Add(String.Format("'{0}' does not have the correct size for a Nintendo 64 {1} .map file.", filename, GameInfo.Name));
                modified = true;
                return null;
            }
            bool isDesert = TheaterTypes.Desert.Name.Equals(Map.Theater?.Name, StringComparison.OrdinalIgnoreCase);
            Dictionary<int, ushort> mapping = isDesert ? N64MapConverter.DESERT_MAPPING : N64MapConverter.TEMPERATE_MAPPING;
            string mappingName = isDesert ? TheaterTypes.Desert.Name : TheaterTypes.Temperate.Name;
            byte[] buffer = new byte[binLen];
            const ushort defVal = 0xFF00;
            for (int i = 0; i < binLen; i += 2)
            {
                byte val1 = mapData[i];
                byte val2 = mapData[i + 1];
                // Nintendo 64 big-endian combining.
                int n64Val = val1 << 8 | val2;
                ushort pcVal;
                if (n64Val == 0xFFFF)
                {
                    pcVal = defVal;
                }
                else
                {
                    if (!mapping.TryGetValue(n64Val, out pcVal))
                    {
                        errors.Add(String.Format("No mapping found for value {0} in Nintendo 64 mapping table for {1} Theater.", n64Val.ToString("X4"), mappingName));
                        pcVal = defVal;
                    }
                }
                buffer[i] = (byte)((pcVal >> 8) & 0xFF);
                buffer[i + 1] = (byte)(pcVal & 0xFF);
            }
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader binReader = new BinaryReader(ms, Encoding.UTF8, true))
            {
                errors.AddRange(LoadBinaryClassic(binReader, templates, ref modified));
            }
            return templates;
        }

        private string AddVideoRemarks(string videoName)
        {
            if (MovieEmpty.Equals(videoName))
                return videoName;
            string newName = GeneralUtils.AddRemarks(videoName, MovieEmpty, true, movieTypesRemarksOld, IniParseConstants.MovieRemarkOld, out bool changed);
            if (!changed)
            {
                newName = GeneralUtils.AddRemarks(videoName, MovieEmpty, true, movieTypesRemarksNew, IniParseConstants.MovieRemarkNew, false);
            }
            return newName;
        }

        private void ParseIniContent(INI ini, byte[] iniBytes, bool forSole)
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
            if (iniTextUtf8 == null)
            {
                return;
            }
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
            if (briefSectionUtf8 == null || briefSectionDos == null)
            {
                return;
            }
            if (briefSectionUtf8.Keys.Contains("Text"))
            {
                // TD briefings do not handle semicolons as comment, so if a comment exists, restore it.
                string comment = briefSectionUtf8.GetComment("Text");
                string briefing = briefSectionUtf8.Keys["Text"];
                if (comment != null)
                {
                    briefing += comment;
                }
                briefSectionDos.Keys["Text"] = briefing;
                return;
            }
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
            OverlayType road2 = Map.OverlayTypes.FirstOrDefault(ov => (ov.Flags & OverlayTypeFlag.RoadSpecial) != OverlayTypeFlag.None && ov.ForceTileNr == 1 && ov.GraphicsSource != ov.Name);
            if (road2 != null)
            {
                string[] iniTextArr = iniText.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
                Dictionary<string, int> foundAmounts = new Dictionary<string, int>();
                Dictionary<string, string> cellTypes = new Dictionary<string, string>();
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
                        string cellNumber = match.Groups[1].Value;
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
                    string cellNumber = match.Groups[1].Value;
                    string type = cellTypes.TryGetValue(cellNumber, out type) ? type : null;
                    // Do not allow actual road2 type cells in the ini.
                    if (type != null && !type.Equals(road2name, StringComparison.InvariantCultureIgnoreCase)
                        && foundAmounts.TryGetValue(cellNumber, out int amount))
                    {
                        if (amount == 1)
                        {
                            newIniText.Add(currLine);
                        }
                        else if (amount > 1)
                        {
                            if (type.Equals(roadname, StringComparison.InvariantCultureIgnoreCase))
                            {
                                // Add second line as prefixed.
                                newIniText.Add(cellNumber + roadname);
                                newIniText.Add("0" + cellNumber + roadname);
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

        protected virtual List<string> LoadINI(INI ini, bool tryCheckSoloMission, bool fromMix, ref bool modified)
        {
            return LoadINI(ini, tryCheckSoloMission, fromMix, false, ref modified);
        }

        protected List<string> LoadINI(INI ini, bool tryCheckSoloMission, bool fromMix, bool forSole, ref bool modified)
        {
            List<string> errors = new List<string>();
            Map.BeginUpdate();
            // Digest. Seems to exist in some console maps.
            ini.Sections.Remove("Digest");
            BasicSection basic = (BasicSection)Map.BasicSection;
            HouseType player = this.LoadIniBasic(ini, basic);
            UpdateBuildingRules(ini, this.Map, false, null);
            this.LoadIniMap(ini, errors, ref modified);
            bool skipSoleStuff = forSole && Globals.NoOwnedObjectsInSole;
            LoadIniBriefing(ini, errors, ref modified);
            LoadIniSteam(ini, errors, ref modified);
            List<TeamType> teamTypes = LoadIniTeamTypes(ini, errors, ref modified);
            Map.TeamTypes.AddRange(teamTypes);
            List<Trigger> triggers = LoadIniTriggers(ini, errors, ref modified);
            LoadIniSmudge(ini, errors, ref modified);
            // Sort
            ExplorerComparer comparer = new ExplorerComparer();
            triggers.Sort((x, y) => comparer.Compare(x.Name, y.Name));
            Dictionary<string, string> caseTrigs = Trigger.None.Yield().Concat(triggers.Select(t => t.Name)).ToDictionary(t => t, StringComparer.OrdinalIgnoreCase);
            HashSet<string> checkUnitTrigs = Trigger.None.Yield().Concat(Map.FilterUnitTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            LoadIniInfantry(ini, skipSoleStuff, caseTrigs, checkUnitTrigs, errors, ref modified);
            LoadIniUnits(ini, skipSoleStuff, caseTrigs, checkUnitTrigs, errors, ref modified);
            LoadIniAircraft(ini, skipSoleStuff, errors, ref modified);
            HashSet<string> checkStrcTrigs = Trigger.None.Yield().Concat(Map.FilterStructureTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            bool wallWarningAdded = false;
            LoadIniStructures(ini, skipSoleStuff, caseTrigs, checkStrcTrigs, errors, ref modified, ref wallWarningAdded);
            LoadIniBase(ini, skipSoleStuff, errors, ref modified, ref wallWarningAdded);
            HashSet<string> checkTerrTrigs = Trigger.None.Yield().Concat(Map.FilterTerrainTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            LoadIniTerrain(ini, caseTrigs, checkTerrTrigs, errors, ref modified);
            LoadIniOverlay(ini, errors, ref modified);
            LoadIniWaypoints(ini, errors, ref modified);
            HashSet<string> checkCellTrigs = Map.FilterCellTriggers(triggers).Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            this.LoadIniCellTriggers(ini, caseTrigs, checkCellTrigs, errors, ref modified);
            LoadIniHouses(ini, errors, ref modified);
            UpdateBasePlayerHouse();
            ClearUnusedTriggerArguments(triggers);
            errors.AddRange(CheckTriggers(triggers, true, true, false, out _, true, out bool trigsFixed));
            if (trigsFixed)
            {
                modified = true;
            }
            extraSections = ini.Sections.Clone();
            // Won't trigger the notifications.
            Map.Triggers.Clear();
            Map.Triggers.AddRange(triggers);
            Map.TeamTypes.Sort((x, y) => comparer.Compare(x.Name, y.Name));
            if (!forSole)
            {
                errors.AddRange(UpdateHouseRules(ini, Map, null));
                CheckSwitchToSolo(tryCheckSoloMission, fromMix, errors);
            }
            Map.EndUpdate();
            return errors;
        }

        private HouseType LoadIniBasic(INI ini, BasicSection basic)
        {
            INISection basicSection = INITools.ParseAndLeaveRemainder(ini, "Basic", Map.BasicSection, new MapContext(Map, false));
            if (basicSection != null)
            {
                basic.Intro = AddVideoRemarks(basic.Intro);
                basic.Brief = AddVideoRemarks(basic.Brief);
                basic.Action = AddVideoRemarks(basic.Action);
                basic.Win = AddVideoRemarks(basic.Win);
                basic.Win2 = AddVideoRemarks(basic.Win2);
                basic.Win3 = AddVideoRemarks(basic.Win3);
                basic.Win4 = AddVideoRemarks(basic.Win4);
                basic.Lose = AddVideoRemarks(basic.Lose);
            }
            string plName = Map.BasicSection.Player;
            HouseType player = Map.HouseTypes.Where(t => t.Equals(plName)).FirstOrDefault() ?? Map.HouseTypes.First();
            plName = player.Name;
            Map.BasicSection.Player = plName;
            return player;
        }

        private void LoadIniMap(INI ini, List<string> errors, ref bool modified)
        {
            // Map info
            string theaterStr = ini["Map"]?.TryGetValue("Theater") ?? String.Empty;
            // Specifically disable this to give accurate feedback on parse errors in the map size information.
            Map.MapSection.AutoFixSize = false;
            // This sets the Theater
            INISection mapSection = INITools.ParseAndLeaveRemainder(ini, "Map", Map.MapSection, new MapContext(Map, false));
            if (!this.Map.TheaterTypes.Any(thr => String.Equals(thr.Name, theaterStr, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add(String.Format("Theater \"{0}\" could not be found. Defaulting to \"{1}\".", theaterStr, Map.Theater));
                modified = true;
            }
            // Also clear megamap indicator.
            if (mapSection.Remove("Version") && mapSection.Keys.Count == 0)
            {
                ini.Sections.Remove(mapSection.Name);
            }
            Map.MapSection.FixBounds(errors);
            Map.MapSection.AutoFixSize = true;
        }

        private void LoadIniBriefing(INI ini, List<string> errors, ref bool modified)
        {
            INISection briefingSection = ini.Sections["Briefing"];
            if (briefingSection == null)
            {
                return;
            }
            if (briefingSection.Keys.Contains("Text"))
            {
                // Remastered briefing
                Map.BriefingSection.Briefing = briefingSection["Text"].Replace("@", Environment.NewLine);
            }
            else
            {
                bool parseAtBreaks = Globals.EnableTdClassicMultiLine;
                // If enabled, '@' type multiLine has priority, since it's an 'official' method (though from RA).
                bool parseHashBreaks = Globals.EnableTd106LineBreaks && !parseAtBreaks;
                // Classic briefing, with v1.06 line break support.
                StringBuilder briefLines = new StringBuilder();
                int line = 1;
                string lineStr;
                bool addSpace = false;
                while (briefingSection.Keys.Contains(lineStr = line.ToString()))
                {
                    string briefLine = briefingSection[lineStr].Trim();
                    // C&C95 v1.06 line break format. Unlike RA's '@' system, this only works at the end of the line.
                    // The @ system is checked here too so ending on an @ doesn't add an extra space.
                    int breakLen = (parseHashBreaks && briefLine.EndsWith("##")) ? 2 :
                        (parseAtBreaks && briefLine.EndsWith("@")) ? 1: 0;
                    if (breakLen > 0)
                    {
                        briefLine = briefLine.Substring(0, briefLine.Length - breakLen);
                    }
                    if (addSpace)
                    {
                        briefLines.Append(" ");
                    }
                    briefLines.Append(briefLine.TrimEnd());
                    if (breakLen > 0)
                    {
                        briefLines.AppendLine();
                    }
                    addSpace = breakLen == 0;
                    line++;
                }
                if (parseAtBreaks)
                {
                    briefLines = briefLines.Replace("@", Environment.NewLine);
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

        private void LoadIniSteam(INI ini, List<string> errors, ref bool modified)
        {
            // Steam info
            INISection steamSection = ini.Sections.Extract("Steam");
            if (steamSection != null)
            {
                // Ignore any errors in this.
                INI.ParseSection(new MapContext(Map, false), steamSection, Map.SteamSection, true);
            }
        }

        private List<TeamType> LoadIniTeamTypes(INI ini, List<string> errors, ref bool modified)
        {
            INISection teamTypesSection = ini.Sections.Extract("TeamTypes");
            List<TeamType> teamTypes = new List<TeamType>();
            if (teamTypesSection == null || teamTypesSection.Count == 0)
            {
                return teamTypes;
            }
            string curType = "Team Type";
            // Make case insensitive dictionary of teamtype missions.
            Dictionary<string, TeamMission> teamMissionTypes = Enumerable.ToDictionary(TeamMissionTypes.GetTypes(), t => t.Mission, StringComparer.OrdinalIgnoreCase);
            int teamNameLenMax = GameInfo.MaxTeamNameLength;
            foreach (KeyValuePair<string, string> kvp in teamTypesSection)
            {
                try
                {
                    if (kvp.Key.Length > teamNameLenMax)
                    {
                        errors.Add(String.Format(IniParseConstants.KeyLengthWarning,
                            curType, kvp.Key, 8));
                    }
                    TeamType teamType = new TeamType { Name = kvp.Key };
                    string[] tokens = kvp.Value.Split(',');
                    string houseStr = tokens[(int)TeamTypeOptions.House];
                    teamType.House = Map.HouseTypes.Where(t => t.Equals(houseStr)).FirstOrDefault();
                    if (teamType.House == null)
                    {
                        HouseType defHouse = Map.HouseTypes.First();
                        errors.Add(String.Format("Team Type '{0}' references unknown house '{1}'; reverting to '{2}'.",
                            kvp.Key, houseStr, defHouse.Name));
                        modified = true;
                        teamType.House = defHouse;
                    }
                    teamType.IsRoundAbout = Int32.Parse(tokens[(int)TeamTypeOptions.IsRoundAbout]) != 0;
                    teamType.IsLearning = Int32.Parse(tokens[(int)TeamTypeOptions.IsLearning]) != 0;
                    teamType.IsSuicide = Int32.Parse(tokens[(int)TeamTypeOptions.IsSuicide]) != 0;
                    teamType.IsAutocreate = Int32.Parse(tokens[(int)TeamTypeOptions.IsAutocreate]) != 0;
                    teamType.IsMercenary = Int32.Parse(tokens[(int)TeamTypeOptions.IsMercenary]) != 0;
                    teamType.RecruitPriority = Int32.Parse(tokens[(int)TeamTypeOptions.RecruitPriority]);
                    teamType.MaxAllowed = Byte.Parse(tokens[(int)TeamTypeOptions.MaxAllowed]);
                    teamType.InitNum = Byte.Parse(tokens[(int)TeamTypeOptions.InitNum]);
                    teamType.Fear = Byte.Parse(tokens[(int)TeamTypeOptions.Fear]);
                    int numClasses = Int32.Parse(tokens[(int)TeamTypeOptions.Classes]);
                    int classesIndex = (int)TeamTypeOptions.Classes + 1;
                    int classesIndexEnd = classesIndex + numClasses;
                    int classesMax = Math.Min(GameInfo.MaxTeamClasses, numClasses);
                    int classesIndexMax = classesIndex + classesMax;
                    for (int i = classesIndex; i < classesIndexMax; ++i)
                    {
                        string[] classTokens = tokens[i].Split(':');
                        if (classTokens.Length != 2)
                        {
                            errors.Add(String.Format("Team Type '{0}' has wrong number of tokens for class index {1} (has {2}, expecting 2); class ignored.",
                                kvp.Key, i, classTokens.Length));
                            modified = true;
                            continue;
                        }
                        ITechnoType type = fullTechnoTypes.Where(t => t.Name.Equals(classTokens[0], StringComparison.InvariantCultureIgnoreCase))
                            .FirstOrDefault();

                        if (!Byte.TryParse(classTokens[1], out byte count))
                        {
                            count = 1;
                        }
                        if (type == null)
                        {
                            errors.Add(String.Format("Team Type '{0}', class index {1}, references unknown class '{2}'; class ignored.",
                                kvp.Key, i, classTokens[0]));
                            modified = true;
                            continue;
                        }
                        teamType.Classes.Add(new TeamTypeClass { Type = type, Count = count });
                    }
                    if (numClasses > GameInfo.MaxTeamClasses)
                    {
                        errors.Add(String.Format("Team Type '{0}' has more classes than the game can handle (has {1}, maximum is {2}).",
                            kvp.Key, numClasses, GameInfo.MaxTeamClasses));
                        modified = true;
                    }
                    int numMissions = Int32.Parse(tokens[classesIndexEnd]);
                    int missionsIndex = classesIndexEnd + 1;
                    int missionsIndexEnd = missionsIndex + numMissions;
                    int missionsMax = Math.Min(GameInfo.MaxTeamMissions, numMissions);
                    int missionsIndexMax = missionsIndex + missionsMax;
                    for (int i = missionsIndex; i < missionsIndexMax; ++i)
                    {
                        string[] missionTokens = tokens[i].Split(':');
                        if (missionTokens.Length != 2)
                        {
                            errors.Add(String.Format("Team Type '{0}' has wrong number of tokens for orders index {1} (has {2}, expecting 2); order ignored.",
                                kvp.Key, i, missionTokens.Length));
                            modified = true;
                            continue;
                        }
                        // fix mission case sensitivity issues.
                        teamMissionTypes.TryGetValue(missionTokens[0], out TeamMission mission);
                        if (mission == null)
                        {
                            errors.Add(String.Format("Team Type '{0}', orders index {1}, references unknown orders '{2}'; order ignored.",
                                kvp.Key, i, missionTokens[0]));
                            modified = true;
                            continue;
                        }
                        string argError = null;
                        string argStr = missionTokens[1];
                        if (!Int32.TryParse(argStr, out int arg))
                        {
                            argError = String.Format("Team Type '{0}', orders index {1} ('{2}'), has a non-numeric value '{3}'; reverting to 0.",
                                kvp.Key, i, mission.Mission, argStr);
                        }
                        else if (mission.ArgType == TeamMissionArgType.Time && arg < 0)
                        {
                            argError = String.Format("Team Type '{0}', orders index {1} ('{2}'), has a bad value '{3}' for a Time argument; reverting to 0.",
                                kvp.Key, i, mission.Mission, argStr);
                        }
                        else if (mission.ArgType == TeamMissionArgType.Waypoint && (arg < -1 || arg > Map.Waypoints.Length))
                        {
                            argError = String.Format("Team Type '{0}', orders index {1} ('{2}'), has a bad value '{3}' for a Waypoint argument: reverting to 0.",
                                kvp.Key, i, mission.Mission, argStr);
                        }
                        else if (mission.ArgType == TeamMissionArgType.OptionsList && (arg < 0 || arg > mission.DropdownOptions.Max(vl => vl.Value))) // Not actually used in TD.
                        {
                            argError = String.Format("Team Type '{0}', orders index {1} ('{2}'), has a bad value '{3}' for the available options; reverting to 0.",
                                kvp.Key, i, mission.Mission, argStr);
                        }
                        else if (mission.ArgType == TeamMissionArgType.MapCell && (arg < 0 || arg >= Map.Metrics.Length))
                        {
                            argError = String.Format("Team Type '{0}', orders index {1} ('{2}'), has a bad value '{3}' for a Cell argument; reverting to 0.",
                                kvp.Key, i, mission.Mission, argStr);
                        }
                        else if (mission.ArgType == TeamMissionArgType.MissionNumber && (arg < 0 || arg > missionsMax)) // Not actually used in TD.
                        {
                            argError = String.Format("Team Type '{0}', orders index {1} ('{2}'), has a bad value '{3}' for an orders index argument; reverting to 0.",
                                kvp.Key, i, mission.Mission, argStr);
                        }
                        else if (mission.ArgType == TeamMissionArgType.Tarcom && arg < 0)
                        {
                            argError = String.Format("Team Type '{0}', orders index {1} ('{2}'), has a bad value '{3}' for a Tarcom argument; reverting to 0.",
                                kvp.Key, i, mission.Mission, argStr);
                        }
                        if (argError != null)
                        {
                            errors.Add(argError);
                            modified = true;
                            arg = 0;
                        }
                        teamType.Missions.Add(new TeamTypeMission { Mission = mission, Argument = arg });
                    }
                    if (numMissions > GameInfo.MaxTeamMissions)
                    {
                        errors.Add(String.Format("Team Type '{0}' has more orders than the game can handle (has {1}, maximum is {2}).",
                            kvp.Key, numMissions, GameInfo.MaxTeamMissions));
                        modified = true;
                    }
                    int reinforceIndex = missionsIndexEnd;
                    if (tokens.Length > reinforceIndex)
                    {
                        teamType.IsReinforcable = Int32.Parse(tokens[reinforceIndex]) != 0;
                    }
                    int prebuiltIndex = missionsIndexEnd + 1;
                    if (tokens.Length > prebuiltIndex)
                    {
                        teamType.IsPrebuilt = Int32.Parse(tokens[prebuiltIndex]) != 0;
                    }
                    teamTypes.Add(teamType);
                }
                catch (Exception ex)
                {
                    errors.Add(String.Format("Team Type '{0}' has errors and can't be parsed: {1}.", kvp.Key, ex.Message));
                    modified = true;
                }
            }
            return teamTypes;
        }

        private List<Trigger> LoadIniTriggers(INI ini, List<string> errors, ref bool modified)
        {
            INISection triggersSection = ini.Sections.Extract("Triggers");
            List<Trigger> triggers = new List<Trigger>();
            if (triggersSection == null)
            {
                return triggers;
            }
            string curType = "Trigger";
            int trigLoopMax = (int)Enum.GetValues(typeof(TriggerPersistentType)).Cast<TriggerPersistentType>().Max();
            string trigLoopDef = "'0' (" + Trigger.PersistenceNamesShort.ToList()[0] + ")";
            int trigNameLenMax = GameInfo.MaxTriggerNameLength;
            foreach (KeyValuePair<string, string> kvp in triggersSection)
            {
                try
                {
                    if (kvp.Key.Length > trigNameLenMax)
                    {
                        errors.Add(String.Format(IniParseConstants.KeyLengthWarning,
                            curType, kvp.Key, 4));
                    }
                    string[] tokens = kvp.Value.Split(',');
                    if (tokens.Length < 5)
                    {
                        errors.Add(String.Format(IniParseConstants.ParseTokensBadNr,
                            curType, kvp.Key, kvp.Value, tokens.Length, "5 or 6"));
                        modified = true;
                        continue;
                    }
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
                            errors.Add(String.Format(IniParseConstants.ParseTypeUnknownDef,
                                curType, kvp.Key, "Event", tokens[0], EventTypes.EVENT_NONE));
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
                            errors.Add(String.Format(IniParseConstants.ParseTypeUnknownDef,
                                curType, kvp.Key, "Action", tokens[1], EventTypes.EVENT_NONE));
                            modified = true;
                        }
                    }
                    trigger.Action1.ActionType = actionType;
                    if (!Int32.TryParse(tokens[2], out int data))
                    {
                        errors.Add(String.Format(IniParseConstants.ParseDataBad,
                            curType, kvp.Key, "Data", tokens[2], 0));
                        data = 0;
                        modified = true;
                    }
                    trigger.Event1.Data = data;

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
                            errors.Add(String.Format(IniParseConstants.ParseTypeUnknownDef,
                                curType, kvp.Key, "House", tokens[3], Model.House.None));
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
                            modified = true;
                        }
                    }
                    trigger.Action1.Team = team;
                    trigger.PersistentType = TriggerPersistentType.Volatile;
                    if (tokens.Length > 5)
                    {
                        if (!Int32.TryParse(tokens[5], out int trigPersist))
                        {
                            errors.Add(String.Format(IniParseConstants.ParseDataBad,
                                curType, kvp.Key, "loop type", tokens[5], trigLoopDef));
                            trigPersist = 0;
                            modified = true;
                        }
                        else if (trigPersist < 0 || trigPersist > trigLoopMax)
                        {
                            errors.Add(String.Format(IniParseConstants.ParseTypeUnknownDef,
                                curType, kvp.Key, "loop type", tokens[5], trigLoopDef));
                            trigPersist = 0;
                            modified = true;
                        }
                        trigger.PersistentType = (TriggerPersistentType)trigPersist;
                    }
                    triggers.Add(trigger);
                }
                catch (Exception ex)
                {
                    errors.Add(String.Format("Trigger '{0}' has errors and can't be parsed: {1}.", kvp.Key, ex.Message));
                    modified = true;
                }
            }
            return triggers;
        }

        private void LoadIniSmudge(INI ini, List<string> errors, ref bool modified)
        {
            INISection smudgeSection = ini.Sections.Extract("Smudge");
            if (smudgeSection == null)
            {
                return;
            }
            bool craterWarningAdded = false;
            const string curType = "Smudge";
            foreach (KeyValuePair<string, string> kvp in smudgeSection)
            {
                if (!Int32.TryParse(kvp.Key, out int cell))
                {
                    errors.Add(String.Format(IniParseConstants.ParseCellKeyBad,
                        curType, kvp.Key, kvp.Value));
                    modified = true;
                    continue;
                }
                if (!Map.Metrics.Contains(cell))
                {
                    errors.Add(String.Format(IniParseConstants.ParseCellKeyIllegal,
                        curType, kvp.Key, cell));
                    modified = true;
                    continue;
                }
                string[] tokens = kvp.Value.Split(',');
                if (tokens.Length != 3)
                {
                    errors.Add(String.Format(IniParseConstants.ParseTokensBadNr,
                        curType, kvp.Key, tokens[0], tokens.Length, 3));
                    modified = true;
                    continue;
                }
                // Craters other than cr1 don't work right in the game. Replace them by stage-0 cr1.
                bool badCrater = Globals.ConvertCraters && SmudgeTypes.BadCraters.IsMatch(tokens[0]);
                SmudgeType smudgeType = badCrater ? SmudgeTypes.Crater1 : Map.SmudgeTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault();
                if (smudgeType == null)
                {
                    errors.Add(String.Format(IniParseConstants.ParseTypeUnknownCell,
                        curType, kvp.Key, cell, tokens[0]));
                    modified = true;
                    continue;
                }
                string name = smudgeType.Name.ToUpperInvariant();
                if (badCrater)
                {
                    errors.Add(String.Format(IniParseConstants.ParseHandleCrater,
                        curType, kvp.Key, tokens[0].ToUpperInvariant(), cell, name));
                    if (!craterWarningAdded)
                    {
                        errors.Add(String.Format(IniParseConstants.ConsultManual, IniParseConstants.SettingBadCraters));
                        craterWarningAdded = true;
                    }
                    modified = true;
                }
                if (Globals.FilterTheaterObjects && !smudgeType.ExistsInTheater)
                {
                    errors.Add(String.Format(IniParseConstants.ParseTheaterBadCell,
                        curType, kvp.Key, curType, name, cell));
                    modified = true;
                    continue;
                }
                int icon = 0;
                if (smudgeType.Icons > 1 && Int32.TryParse(tokens[2], out icon))
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
        }

        private void LoadIniInfantry(INI ini, bool skipSoleStuff, Dictionary<string, string> caseTrigs, HashSet<string> checkUnitTrigs, List<string> errors, ref bool modified)
        {
            INISection infantrySection = ini.Sections.Extract("Infantry");
            int amount = infantrySection?.Count ?? 0;
            if (amount == 0)
            {
                return;
            }
            if (skipSoleStuff)
            {
                errors.Add(String.Format(amount == 1 ? DisabledObjSoleOne : DisabledObjSoleMul, amount, "[Infantry]"));
                modified = true;
                return;
            }
            List<string> warnings = new List<string>();
            string curType = Map.InfantryTypes.First().TypeName;
            foreach (KeyValuePair<string, string> kvp in infantrySection)
            {
                // Parse warnings. Only add these to the errors list if the item is not skipped entirely with a fatal error.
                warnings.Clear();
                string[] tokens = kvp.Value.Split(',');
                if (tokens.Length != 8)
                {
                    errors.Add(String.Format(IniParseConstants.ParseTokensBadNr,
                        curType, kvp.Key, kvp.Value, tokens.Length, 8));
                    modified = true;
                    continue;
                }
                InfantryType infantryType = Map.InfantryTypes.Where(t => t.Equals(tokens[1])).FirstOrDefault();
                if (infantryType == null)
                {
                    errors.Add(String.Format(IniParseConstants.ParseTypeUnknown,
                        curType, kvp.Key, curType, tokens[1]));
                    modified = true;
                    continue;
                }
                string name = infantryType.Name.ToUpper();
                if (!Int32.TryParse(tokens[2], out int strength))
                {
                    errors.Add(String.Format(IniParseConstants.ParseStrengthBad,
                        curType, kvp.Key, name, tokens[2]));
                    modified = true;
                    continue;
                }
                if (!Int32.TryParse(tokens[3], out int cell))
                {
                    errors.Add(String.Format(IniParseConstants.ParseCellBad,
                        curType, kvp.Key, name, tokens[3]));
                    modified = true;
                    continue;
                }
                if (!Map.Metrics.Contains(cell))
                {
                    errors.Add(String.Format(IniParseConstants.ParseCellIllegal,
                        curType, kvp.Key, name, cell));
                    modified = true;
                    continue;
                }
                if (!Int32.TryParse(tokens[4], out int stoppingPos))
                {
                    errors.Add(String.Format(IniParseConstants.ParseSubPosBad,
                        curType, kvp.Key, name, cell, tokens[4]));
                    modified = true;
                    continue;
                }
                if (stoppingPos < 0 || stoppingPos >= Globals.NumInfantryStops)
                {
                    errors.Add(String.Format(IniParseConstants.ParseSubPosIllegal,
                        curType, kvp.Key, name, cell, stoppingPos));
                    modified = true;
                    continue;
                }
                string cellpos = String.Format(IniParseConstants.ParseInfantryCellSubPos, cell, stoppingPos);
                if (strength < 0 || strength > 256)
                {
                    int newStrength = strength.Restrict(0, 256);
                    warnings.Add(String.Format(IniParseConstants.ParseStrengthIllegal,
                        curType, kvp.Key, name, cellpos, stoppingPos, strength, newStrength));
                    strength = newStrength;
                    modified = true;
                }
                if (!Int32.TryParse(tokens[6], out int dirValue))
                {
                    warnings.Add(String.Format(IniParseConstants.ParseDirectionBad,
                        curType, kvp.Key, name, cellpos, stoppingPos, tokens[6]));
                    modified = true;
                    dirValue = 0;
                }
                DirectionType dirType = DirectionType.FindClosestDirectionType(dirValue, Map.UnitDirectionTypes);
                if (dirType.ID != dirValue)
                {
                    warnings.Add(
                        String.Format(IniParseConstants.ParseDirectionIllegal + IniParseConstants.ParseDirectionUnknown,
                        curType, kvp.Key, name, cellpos, stoppingPos, dirValue) + 
                        String.Format(IniParseConstants.ParseDirectionClosest,
                        dirType.ID, dirType.Name));
                    modified = true;
                }
                ICellOccupier occupier = Map.Technos[cell];
                InfantryGroup infantryGroup = occupier as InfantryGroup;
                // Nothing blocking: make new InfantryGroup
                if (infantryGroup == null && occupier == null)
                {
                    infantryGroup = new InfantryGroup();
                    Map.Technos.Add(cell, infantryGroup);
                }
                // No InfantryGroup found or created, so something was blocking it. Figure out what.
                if (infantryGroup == null)
                {
                    string blocker;
                    if (occupier is Terrain terrain)
                    {
                        // Should always find this.
                        (Point location, Terrain occupier) occ = Map.Technos.OfType<Terrain>().FirstOrDefault(po => po.Occupier == terrain);
                        Map.Metrics.GetCell(occ.location, out int placement);
                        blocker = String.Format(IniParseConstants.ParseBlockerMulticellArg,
                            terrain.Type.TypeName, terrain.Type.Name.ToUpperInvariant(), placement);
                    }
                    else if (occupier is ITechno tech)
                    {
                        blocker = tech.TechnoType.TypeName + " " + tech.TechnoType.Name.ToUpperInvariant();
                    }
                    else
                    {
                        blocker = IniParseConstants.ParseBlockerUnknown;
                    }
                    errors.Add(String.Format(IniParseConstants.ParseBlocker,
                            curType, kvp.Key, name, cellpos, blocker));
                    modified = true;
                    continue;
                }
                Infantry occupant = infantryGroup.Infantry[stoppingPos];
                if (occupant != null)
                {
                    errors.Add(String.Format(IniParseConstants.ParseBlockerInfGroup,
                        curType, kvp.Key, name, cell, occupant.Type.Name.ToUpperInvariant(), stoppingPos));
                    modified = true;
                    continue;
                }
                if (!caseTrigs.ContainsKey(tokens[7]))
                {
                    warnings.Add(String.Format(IniParseConstants.ParseTriggerUnknownObj,
                        curType, kvp.Key, name, cellpos, tokens[7]));
                    modified = true;
                    tokens[7] = Trigger.None;
                }
                else if (!checkUnitTrigs.Contains(tokens[7]))
                {
                    warnings.Add(String.Format(IniParseConstants.ParseTriggerIllegalObj,
                        curType, kvp.Key, name, cellpos, caseTrigs[tokens[7]]));
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
                    Direction = dirType,
                    Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5])).FirstOrDefault(),
                    Trigger = tokens[7]
                };
                infantryGroup.Infantry[stoppingPos] = inf;
                if (inf.House == null)
                {
                    HouseType defHouse = Map.HouseTypes.First();
                    warnings.Add(String.Format(IniParseConstants.ParseHouseUnknownObj,
                        curType, kvp.Key, name, cellpos, tokens[0], defHouse.Name));
                    modified = true;
                    inf.House = defHouse;
                }
                if (inf.Mission == null)
                {
                    string badOrder = Map.MissionTypesBad.Where(t => t.Equals(tokens[5], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    string deforders = Map.GetDefaultMission(infantryType);
                    string message = badOrder != null ? IniParseConstants.ParseOrdersUnsupported : IniParseConstants.ParseOrdersUnknown;
                    errors.Add(String.Format(message, curType, kvp.Key, name, cellpos, badOrder ?? tokens[5], deforders));
                    modified = true;
                    inf.Mission = deforders;
                }
                errors.AddRange(warnings);
            }
        }

        private void LoadIniUnits(INI ini, bool skipSoleStuff, Dictionary<string, string> caseTrigs, HashSet<string> checkUnitTrigs, List<string> errors, ref bool modified)
        {
            INISection unitsSection = ini.Sections.Extract("Units");
            int amount = unitsSection?.Count ?? 0;
            if (amount == 0)
            {
                return;
            }
            if (skipSoleStuff)
            {
                errors.Add(String.Format(amount == 1 ? DisabledObjSoleOne : DisabledObjSoleMul, amount, "[Units]"));
                modified = true;
                return;
            }
            List<string> warnings = new List<string>();
            List<VehicleType> units = Map.AllUnitTypes.OfType<VehicleType>().ToList();
            string curType = VehicleType.SubTypeName;
            foreach (KeyValuePair<string, string> kvp in unitsSection)
            {
                // Parse warnings. Only add these to the errors list if the item is not skipped entirely with a fatal error.
                warnings.Clear();
                string[] tokens = kvp.Value.Split(',');
                if (tokens.Length != 7)
                {
                    errors.Add(String.Format(IniParseConstants.ParseTokensBadNr,
                        curType, kvp.Key, kvp.Value, tokens.Length, 7));
                    modified = true;
                    continue;
                }
                UnitType unitType = units.Where(t => t.Equals(tokens[1])).FirstOrDefault();
                if (unitType == null)
                {
                    errors.Add(String.Format(IniParseConstants.ParseTypeUnknown,
                        curType, kvp.Key, curType, tokens[1]));
                    modified = true;
                    continue;
                }
                string name = unitType.Name.ToUpperInvariant();
                if (!Int32.TryParse(tokens[2], out int strength))
                {
                    errors.Add(String.Format(IniParseConstants.ParseStrengthBad,
                        curType, kvp.Key, name, tokens[2]));
                    modified = true;
                    continue;
                }
                if (!Int32.TryParse(tokens[3], out int cell))
                {
                    errors.Add(String.Format(IniParseConstants.ParseCellBad,
                        curType, kvp.Key, name, tokens[3]));
                    modified = true;
                    continue;
                }
                if (!Map.Metrics.Contains(cell))
                {
                    errors.Add(String.Format(IniParseConstants.ParseCellIllegal,
                        curType, kvp.Key, name, cell));
                    modified = true;
                    continue;
                }
                if (strength < 0 || strength > 256)
                {
                    int newStrength = strength.Restrict(0, 256);
                    warnings.Add(String.Format(IniParseConstants.ParseStrengthIllegal,
                        curType, kvp.Key, name, cell, strength, newStrength));
                    strength = newStrength;
                    modified = true;
                }
                if (!Int32.TryParse(tokens[4], out int dirValue))
                {
                    warnings.Add(String.Format(IniParseConstants.ParseDirectionBad,
                        curType, kvp.Key, name, cell, tokens[4]));
                    modified = true;
                    dirValue = 0;
                }
                DirectionType dirType = DirectionType.FindClosestDirectionType(dirValue, Map.UnitDirectionTypes);
                if (dirType.ID != dirValue)
                {
                    warnings.Add(
                        String.Format(IniParseConstants.ParseDirectionIllegal + IniParseConstants.ParseDirectionUnknown,
                        curType, kvp.Key, name, cell, dirValue) +
                        String.Format(IniParseConstants.ParseDirectionClosest,
                        dirType.ID, dirType.Name));                        
                    modified = true;
                }
                Unit newUnit = new Unit()
                {
                    Type = unitType,
                    House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                    Strength = strength,
                    Direction = dirType,
                    Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault(),
                };
                if (newUnit.House == null)
                {
                    HouseType defHouse = Map.HouseTypes.First();
                    warnings.Add(String.Format(IniParseConstants.ParseHouseUnknownObj,
                        curType, kvp.Key, name, cell, tokens[0], defHouse.Name));
                    modified = true;
                    newUnit.House = defHouse;
                }
                if (newUnit.Mission == null)
                {
                    string badOrder = Map.MissionTypesBad.Where(t => t.Equals(tokens[5], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    string deforders = Map.GetDefaultMission(unitType);
                    if (newUnit.Type.Equals(UnitTypes.MCV) && badOrder == MissionTypes.MISSION_RESCUE)
                    {
                        // "Unload unpacks the MCV. "Hunt" looks for a nearby target and unpacks the MCV.
                        // "Rescue" equates to "Hunt" in the code, but clutters the list, so let's correct it to "Hunt".
                        deforders = MissionTypes.MISSION_HUNT;
                    }
                    string message = badOrder != null ? IniParseConstants.ParseOrdersUnsupported : IniParseConstants.ParseOrdersUnknown;
                    warnings.Add(String.Format(message, curType, kvp.Key, name, cell, badOrder ?? tokens[5], deforders));
                    newUnit.Mission = deforders;
                }
                if (!Map.Technos.Add(cell, newUnit))
                {
                    ICellOccupier occupier = Map.Technos[cell];
                    string blocker;
                    if (occupier is Terrain terrain)
                    {
                        // Should always find this.
                        (Point location, Terrain occupier) occ = Map.Technos.OfType<Terrain>().FirstOrDefault(po => po.Occupier == terrain);
                        Map.Metrics.GetCell(occ.location, out int placement);
                        blocker = String.Format(IniParseConstants.ParseBlockerMulticellArg,
                            terrain.Type.TypeName, terrain.Type.Name.ToUpperInvariant(), placement);
                    }
                    else if (occupier is InfantryGroup ig)
                    {
                        Infantry[] infList = ig.Infantry.Where(i => i != null).ToArray();
                        string infNames = String.Join(", ", infList.Select(i => i.Type.Name.ToUpperInvariant()).ToArray());
                        if (infList.Length > 1)
                        {
                            infNames = "(" + infNames + ")";
                        }
                        // Can never be empty.
                        blocker = infList[0].TechnoType.TypeName + " " + infNames;
                    }
                    else if (occupier is ITechno tech)
                    {
                        blocker = tech.TechnoType.TypeName + " " + tech.TechnoType.Name.ToUpperInvariant();
                    }
                    else
                    {
                        blocker = IniParseConstants.ParseBlockerUnknown;
                    }
                    errors.Add(String.Format(IniParseConstants.ParseBlocker,
                            curType, kvp.Key, name, cell, blocker));
                    modified = true;
                    continue;
                }
                if (!caseTrigs.ContainsKey(tokens[6]))
                {
                    warnings.Add(String.Format(IniParseConstants.ParseTriggerUnknownObj,
                        curType, kvp.Key, name, cell, tokens[6]));
                    modified = true;
                    newUnit.Trigger = Trigger.None;
                }
                else if (!checkUnitTrigs.Contains(tokens[6]))
                {
                    warnings.Add(String.Format(IniParseConstants.ParseTriggerIllegalObj,
                        curType, kvp.Key, name, cell, caseTrigs[tokens[6]]));
                    modified = true;
                    newUnit.Trigger = Trigger.None;
                }
                else
                {
                    // Adapt to same case
                    newUnit.Trigger = caseTrigs[tokens[6]];
                }
                errors.AddRange(warnings);
            }
        }

        private void LoadIniAircraft(INI ini, bool skipSoleStuff, List<string> errors, ref bool modified)
        {
            // Classic game does not support this, so I'm leaving this out by default.
            // It is always extracted, so it doesn't end up with the "extra sections"
            INISection aircraftSection = ini.Sections.Extract("Aircraft");
            int amount = aircraftSection?.Count ?? 0;
            if (amount == 0)
            {
                return;
            }
            string curType = AircraftType.SubTypeName;
            if (Globals.DisableAirUnits || skipSoleStuff)
            {
                errors.Add(
                    (skipSoleStuff ? IniParseConstants.ParseHandleSoleObjects : String.Format(IniParseConstants.SectionDisabled, curType)) + " " +
                    String.Format(amount == 1 ? IniParseConstants.EntrySkipped : IniParseConstants.EntriesSkipped, amount, "[Aircraft]") + " " +
                    String.Format(IniParseConstants.ConsultManual, skipSoleStuff ? IniParseConstants.SettingNoOwnedObjSole : IniParseConstants.SettingNoAirUnits));
                modified = true;
                return;
            }
            List<string> warnings = new List<string>();
            List<AircraftType> aircraft = Map.AllUnitTypes.OfType<AircraftType>().ToList();
            foreach (KeyValuePair<string, string> kvp in aircraftSection)
            {
                // Parse warnings. Only add these to the errors list if the item is not skipped entirely with a fatal error.
                warnings.Clear();
                string[] tokens = kvp.Value.Split(',');
                if (tokens.Length != 6)
                {
                    errors.Add(String.Format(IniParseConstants.ParseTokensBadNr,
                        curType, kvp.Key, kvp.Value, tokens.Length, 6));
                    modified = true;
                    continue;
                }
                UnitType aircraftType = aircraft.Where(t => t.Equals(tokens[1])).FirstOrDefault();
                if (aircraftType == null)
                {
                    errors.Add(String.Format(IniParseConstants.ParseTypeUnknown,
                        curType, kvp.Key, curType, tokens[1]));
                    modified = true;
                    continue;
                }
                string name = aircraftType.Name.ToUpperInvariant();
                if (!Int32.TryParse(tokens[2], out int strength))
                {
                    errors.Add(String.Format(IniParseConstants.ParseStrengthBad,
                        curType, kvp.Key, name, tokens[2]));
                    modified = true;
                    continue;
                }
                if (!Int32.TryParse(tokens[3], out int cell))
                {
                    errors.Add(String.Format(IniParseConstants.ParseCellBad,
                        curType, kvp.Key, name, tokens[3]));
                    modified = true;
                    continue;
                }
                if (!Map.Metrics.Contains(cell))
                {
                    errors.Add(String.Format(IniParseConstants.ParseCellIllegal,
                        curType, kvp.Key, name, cell));
                    modified = true;
                    continue;
                }
                if (strength < 0 || strength > 256)
                {
                    int newStrength = strength.Restrict(0, 256);
                    warnings.Add(String.Format(IniParseConstants.ParseStrengthIllegal,
                        curType, kvp.Key, name, cell, strength, newStrength));
                    strength = newStrength;
                    modified = true;
                }
                if (!Int32.TryParse(tokens[4], out int dirValue))
                {
                    warnings.Add(String.Format(IniParseConstants.ParseDirectionBad,
                        curType, kvp.Key, name, cell, tokens[4]));
                    modified = true;
                    dirValue = 0;
                }
                DirectionType dirType = DirectionType.FindClosestDirectionType(dirValue, Map.UnitDirectionTypes);
                if (dirType.ID != dirValue)
                {
                    warnings.Add(
                        String.Format(IniParseConstants.ParseDirectionIllegal + IniParseConstants.ParseDirectionUnknown,
                        curType, kvp.Key, name, cell, dirValue) +
                        String.Format(IniParseConstants.ParseDirectionClosest,
                        dirType.ID, dirType.Name));
                    modified = true;
                }
                Unit newAir = new Unit()
                {
                    Type = aircraftType,
                    House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                    Strength = strength,
                    Direction = dirType,
                    Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5])).FirstOrDefault()
                };
                if (newAir.House == null)
                {
                    HouseType defHouse = Map.HouseTypes.First();
                    warnings.Add(String.Format(IniParseConstants.ParseHouseUnknownObj,
                        curType, kvp.Key, name, cell, tokens[0], defHouse.Name));
                    modified = true;
                    newAir.House = defHouse;
                }
                if (newAir.Mission == null)
                {
                    string badOrder = Map.MissionTypesBad.Where(t => t.Equals(tokens[5], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    string deforders = Map.GetDefaultMission(aircraftType);
                    string message = badOrder != null ? IniParseConstants.ParseOrdersUnsupported : IniParseConstants.ParseOrdersUnknown;
                    warnings.Add(String.Format(message, curType, kvp.Key, name, cell, badOrder ?? tokens[5], deforders));
                    newAir.Mission = deforders;
                }
                if (!Map.Technos.Add(cell, newAir))
                {
                    ICellOccupier occupier = Map.Technos[cell];
                    string blocker;
                    if (occupier is Terrain terrain)
                    {
                        // Should always find this.
                        (Point location, Terrain occupier) occ = Map.Technos.OfType<Terrain>().FirstOrDefault(po => po.Occupier == terrain);
                        Map.Metrics.GetCell(occ.location, out int placement);
                        blocker = String.Format(IniParseConstants.ParseBlockerMulticellArg,
                            terrain.Type.TypeName, terrain.Type.Name.ToUpperInvariant(), placement);
                    }
                    else if (occupier is InfantryGroup ig)
                    {
                        Infantry[] infList = ig.Infantry.Where(i => i != null).ToArray();
                        string infNames = String.Join(", ", infList.Select(i => i.Type.Name.ToUpperInvariant()).ToArray());
                        if (infList.Length > 1)
                        {
                            infNames = "(" + infNames + ")";
                        }
                        // Can never be empty.
                        blocker = infList[0].TechnoType.TypeName + " " + infNames;
                    }
                    else if (occupier is ITechno tech)
                    {
                        blocker = tech.TechnoType.TypeName + " " + tech.TechnoType.Name.ToUpperInvariant();
                    }
                    else
                    {
                        blocker = IniParseConstants.ParseBlockerUnknown;
                    }
                    errors.Add(String.Format(IniParseConstants.ParseBlocker,
                        curType, kvp.Key, name, cell, blocker));
                    modified = true;
                    continue;
                }
                errors.AddRange(warnings);
            }
        }

        private void LoadIniStructures(INI ini, bool skipSoleStuff, Dictionary<string, string> caseTrigs, HashSet<string> checkStrcTrigs, List<string> errors, ref bool modified, ref bool wallWarningAdded)
        {
            INISection structuresSection = ini.Sections.Extract("Structures");
            int amount = structuresSection?.Count ?? 0;
            if (amount == 0)
            {
                return;
            }
            if (skipSoleStuff)
            {
                errors.Add(String.Format(amount == 1 ? DisabledObjSoleOne : DisabledObjSoleMul, amount, "[Structures]"));
                modified = true;
                return;
            }
            List<string> warnings = new List<string>();
            string curType = Map.AllBuildingTypes.First().TypeName;
            foreach (KeyValuePair<string, string> kvp in structuresSection)
            {
                // Parse warnings. Only add these to the errors list if the item is not skipped entirely with a fatal error.
                warnings.Clear();
                string[] tokens = kvp.Value.Split(',');
                if (tokens.Length != 6)
                {
                    errors.Add(String.Format(IniParseConstants.ParseTokensBadNr,
                        curType, kvp.Key, kvp.Value, tokens.Length, 6));
                    modified = true;
                    continue;
                }
                BuildingType buildingType = Map.AllBuildingTypes.Where(t => t.Equals(tokens[1])).FirstOrDefault();
                if (buildingType == null)
                {
                    errors.Add(String.Format(IniParseConstants.ParseTypeUnknown,
                        curType, kvp.Key, curType, tokens[1]));
                    modified = true;
                    continue;
                }
                string name = buildingType.Name.ToUpperInvariant();
                if (Globals.FilterTheaterObjects && buildingType.IsTheaterDependent && !buildingType.ExistsInTheater)
                {
                    errors.Add(String.Format(IniParseConstants.ParseTheaterBad,
                        curType, kvp.Key, curType, name));
                    modified = true;
                    continue;
                }
                if (!Int32.TryParse(tokens[2], out int strength))
                {
                    errors.Add(String.Format(IniParseConstants.ParseStrengthBad,
                        curType, kvp.Key, name, tokens[2]));
                    modified = true;
                    continue;
                }
                if (!Int32.TryParse(tokens[3], out int cell))
                {
                    errors.Add(String.Format(IniParseConstants.ParseCellBad,
                        curType, kvp.Key, name, tokens[3]));
                    modified = true;
                    continue;
                }
                if (!Map.Metrics.Contains(cell))
                {
                    errors.Add(String.Format(IniParseConstants.ParseCellIllegal,
                        curType, kvp.Key, name, cell));
                    modified = true;
                    continue;
                }
                if (strength < 0 || strength > 256)
                {
                    int newStrength = strength.Restrict(0, 256);
                    warnings.Add(String.Format(IniParseConstants.ParseStrengthIllegal,
                        curType, kvp.Key, name, cell, strength, newStrength));
                    strength = newStrength;
                    modified = true;
                }
                // Do this here, before House or Trigger, since those get ignored if it's a wall type.
                if (buildingType.IsWall && !Globals.AllowWallBuildings)
                {
                    OverlayType wall = Map.OverlayTypes.Where(t => t.Equals(buildingType.Name)).FirstOrDefault();
                    if (wall != null)
                    {
                        errors.Add(String.Format(IniParseConstants.ParseHandleWallStruct,
                            curType, kvp.Key, curType, name, cell, curType));
                        if (!wallWarningAdded)
                        {
                            errors.Add(String.Format(IniParseConstants.ConsultManual, IniParseConstants.SettingNoWallBuildings));
                            wallWarningAdded = true;
                        }
                        Map.Overlay[cell] = new Overlay() { Type = wall, Icon = 0 };
                        modified = true;
                        continue;
                    }
                }
                if (!Int32.TryParse(tokens[4], out int dirValue))
                {
                    warnings.Add(String.Format(IniParseConstants.ParseDirectionBad,
                        curType, kvp.Key, name, cell, tokens[4]));
                    modified = true;
                    dirValue = 0;
                }
                DirectionType dirType = DirectionType.FindClosestDirectionType(dirValue, Map.BuildingDirectionTypes);
                if (dirType.ID != dirValue)
                {
                    string correction = buildingType.HasTurret ?
                        String.Format(IniParseConstants.ParseDirectionClosest, dirType.ID, dirType.Name) :
                        String.Format(IniParseConstants.ParseDirectionNotSupported, curType, name);
                    warnings.Add(String.Format(IniParseConstants.ParseDirectionIllegal + IniParseConstants.ParseDirectionUnknown,
                            curType, kvp.Key, name, cell, dirValue, correction));
                    modified = true;
                }
                else if (!buildingType.HasTurret && dirType.ID != 0)
                {
                    string correction = String.Format(IniParseConstants.ParseDirectionNotSupported, curType, name);
                    warnings.Add(String.Format(IniParseConstants.ParseDirectionIllegal,
                            curType, kvp.Key, name, cell, correction));
                    modified = true;
                }
                Building newBld = new Building()
                    {
                        Type = buildingType,
                        House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                        Strength = strength,
                        Direction = dirType,
                    };
                if (newBld.House == null)
                {
                    HouseType defHouse = Map.HouseTypes.First();
                    warnings.Add(String.Format(IniParseConstants.ParseHouseUnknownObj,
                        curType, kvp.Key, name, cell, tokens[0], defHouse.Name));
                    modified = true;
                    newBld.House = defHouse;
                }
                if (!Map.Buildings.CanAdd(cell, newBld))
                {
                    Map.CheckBuildingBlockingCell(curType, kvp.Key, buildingType, cell, errors, ref modified);
                    continue;
                }
                Map.Buildings.Add(cell, newBld);
                if (!caseTrigs.ContainsKey(tokens[5]))
                {
                    warnings.Add(String.Format(IniParseConstants.ParseTriggerUnknownObj,
                        curType, kvp.Key, name, cell, tokens[5]));
                    modified = true;
                    newBld.Trigger = Trigger.None;
                }
                else if (!checkStrcTrigs.Contains(tokens[5]))
                {
                    warnings.Add(String.Format(IniParseConstants.ParseTriggerIllegalObj,
                        curType, kvp.Key, name, cell, caseTrigs[tokens[5]]));
                    modified = true;
                    newBld.Trigger = Trigger.None;
                }
                else
                {
                    // Adapt to same case
                    newBld.Trigger = caseTrigs[tokens[5]];
                }
                errors.AddRange(warnings);
            }
        }

        private void LoadIniBase(INI ini, bool skipSoleStuff, List<string> errors, ref bool modified, ref bool wallWarningAdded)
        {
            INISection baseSection = ini.Sections["Base"];
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
            const string curType = "Base building";
            string baseCountStr = baseSection.TryGetValue("Count");
            baseSection.Remove("Count");
            if (!Int32.TryParse(baseCountStr, out int baseCount))
            {
                if (skipSoleStuff)
                {
                    // Ignore error. Just indicate it's skipped.
                    errors.Add(IniParseConstants.ParseHandleSoleObjects + " [Base] section is skipped. " + DisabledObjExplSole);
                }
                else
                {
                    errors.Add(String.Format(IniParseConstants.ParseBaseCountBad,
                        curType, baseCountStr));
                }
                modified = true;
                CleanBaseSection(ini, baseSection);
                return;
            }
            if (skipSoleStuff && baseCount > 0)
            {
                errors.Add(String.Format(baseCount == 1 ? DisabledObjSoleOne : DisabledObjSoleMul, baseCount, "[Structures]"));
                modified = true;
                CleanBaseSection(ini, baseSection);
                return;
            }
            int curPriorityVal = 0;
            List<BuildingType> buildings = Map.AllBuildingTypes.ToList();
            string strType = buildings.First().TypeName;
            for (int i = 0; i < baseCount; ++i)
            {
                // This type has no parse warnings. Everything is fatal enough to skip the entry.
                string key = i.ToString("D3");
                string value = baseSection.TryGetValue(key);
                if (value == null)
                {
                    errors.Add(String.Format(IniParseConstants.ParseBaseEntryMissing,
                        curType, key));
                    continue;
                }
                baseSection.Remove(key);
                string[] tokens = value.Split(',');
                if (tokens.Length != 2)
                {
                    errors.Add(String.Format(IniParseConstants.ParseTokensBadNr,
                        curType, key, value, tokens.Length, 2));
                    modified = true;
                    continue;
                }
                BuildingType buildingType = Map.AllBuildingTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault();
                bool foundCoord = Int32.TryParse(tokens[1], out int coord);
                Point location = new Point((coord >> 8) & 0x7F, (coord >> 24) & 0x7F);
                bool canPlace = Map.Metrics.GetCell(location, out int cell);

                if (buildingType == null)
                {
                    if (foundCoord && canPlace)
                    {
                        errors.Add(String.Format(IniParseConstants.ParseTypeUnknownCell,
                            curType, key, cell, strType, tokens[0]));
                    }
                    else
                    {
                        errors.Add(String.Format(IniParseConstants.ParseTypeUnknown,
                            curType, key, strType, tokens[0]));
                    }
                    modified = true;
                    continue;
                }
                string name = buildingType.Name.ToUpperInvariant();
                if (buildingType.IsWall && !Globals.AllowWallBuildings)
                {
                    if (foundCoord && canPlace)
                    {
                        errors.Add(String.Format(IniParseConstants.ParseHandleWallSkipCell,
                            curType, key, curType, name, cell));
                    }
                    else
                    {
                        errors.Add(String.Format(IniParseConstants.ParseHandleWallSkip,
                            curType, key, curType, name));
                    }
                    if (!wallWarningAdded)
                    {
                        errors.Add(String.Format(IniParseConstants.ConsultManual, IniParseConstants.SettingNoWallBuildings));
                        wallWarningAdded = true;
                    }
                    modified = true;
                    continue;
                }
                if (Globals.FilterTheaterObjects && buildingType.IsTheaterDependent && !buildingType.ExistsInTheater)
                {
                    if (foundCoord && canPlace)
                    {
                        errors.Add(String.Format(IniParseConstants.ParseTheaterBadCell,
                            curType, key, strType, name, cell));
                    }
                    else
                    {
                        errors.Add(String.Format(IniParseConstants.ParseTheaterBad,
                            curType, key, strType, name));
                    }
                    modified = true;
                    continue;
                }
                if (!foundCoord)
                {
                    errors.Add(String.Format(IniParseConstants.ParseCoordsBad,
                        curType, key, name, tokens[1]));
                    modified = true;
                    continue;
                }
                if (!canPlace)
                {
                    errors.Add(String.Format(IniParseConstants.ParseCoordsIllegal,
                        curType, key, name, location.X, location.Y));
                    continue;
                }
                if (Map.Buildings.OfType<Building>().Where(x => x.Location == location && x.Occupier.Type.ID == buildingType.ID).FirstOrDefault().Occupier is Building building)
                {
                    // Building found: set priority and continue.
                    if (building.BasePriority == -1)
                    {
                        building.BasePriority = curPriorityVal++;
                    }
                    else
                    {
                        errors.Add(String.Format(IniParseConstants.ParseBaseDuplicate,
                            curType, key, strType, name, cell));
                    }
                    continue;
                }
                // Building not found: add as new with IsPrebuilt set to false.
                Building toRebuild = new Building()
                {
                    Type = buildingType,
                    House = HouseTypes.None,
                    Strength = 256,
                    Direction = Map.BuildingDirectionTypes.FirstOrDefault(),
                    BasePriority = curPriorityVal++,
                    IsPrebuilt = false
                };
                if (!Map.Buildings.CanAdd(location, toRebuild))
                {
                    Map.CheckBuildingBlockingCell(curType, key, buildingType, cell, errors, ref modified);
                    continue;
                }
                Map.Buildings.Add(location, toRebuild);
            }
            // All base sections removed; remainder are exceeding count.
            foreach (KeyValuePair<string, string> kvp in baseSection)
            {
                if (baseKeyRegex.IsMatch(kvp.Key))
                {
                    errors.Add(String.Format(IniParseConstants.ParseBaseCountExceeded,
                        curType, kvp.Key, baseCount));
                    modified = true;
                }
                // non-matches will be ignored as potential modded content.
            }
            CleanBaseSection(ini, baseSection);
        }

        protected void CleanBaseSection(INI ini, INISection baseSection)
        {
            // Clean out and leave; might contain addon keys.
            baseSection.Remove("Count");
            baseSection.RemoveWhere(k => baseKeyRegex.IsMatch(k));
            if (baseSection.Count == 0)
            {
                ini.Sections.Remove(baseSection.Name);
            }
        }

        private void LoadIniTerrain(INI ini, Dictionary<string, string> caseTrigs, HashSet<string> checkTerrTrigs, List<string> errors, ref bool modified)
        {
            INISection terrainSection = ini.Sections.Extract("Terrain");
            if (terrainSection == null)
            {
                return;
            }
            List<string> warnings = new List<string>();
            string curType = Map.TerrainTypes.First().TypeName;
            foreach (KeyValuePair<string, string> kvp in terrainSection)
            {
                // Parse warnings. Only add these to the errors list if the item is not skipped entirely with a fatal error.
                warnings.Clear();
                if (!Int32.TryParse(kvp.Key, out int cell))
                {
                    errors.Add(String.Format(IniParseConstants.ParseCellKeyBad,
                        curType, kvp.Key, kvp.Value));
                    modified = true;
                    continue;
                }
                if (!Map.Metrics.Contains(cell))
                {
                    errors.Add(String.Format(IniParseConstants.ParseCellKeyIllegal,
                        curType, kvp.Key, cell));
                    modified = true;
                    continue;
                }
                string[] tokens = kvp.Value.Split(',');
                TerrainType terrainType = Map.TerrainTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault();
                if (terrainType == null)
                {
                    errors.Add(String.Format(IniParseConstants.ParseTypeUnknownCell,
                        curType, kvp.Key, cell,tokens[0]));
                    modified = true;
                    continue;
                }
                string name = terrainType.Name.ToUpperInvariant();
                if (Globals.FilterTheaterObjects && !terrainType.ExistsInTheater)
                {
                    errors.Add(String.Format(IniParseConstants.ParseTheaterBad,
                        curType, kvp.Key, curType, name));
                    modified = true;
                    continue;
                }
                Terrain newTerr = new Terrain
                {
                    Type = terrainType
                };
                if (!Map.Technos.Add(cell, newTerr))
                {
                    ICellOccupier occupier = Map.FindBlockingObject(cell, terrainType, false, out int blockingCell, out int placementCell);
                    string reportCell = blockingCell == -1 ? "<unknown>" : blockingCell.ToString();
                    string blocker;
                    if (occupier is Terrain terrain)
                    {
                        blocker = String.Format(IniParseConstants.ParseBlockerMulticellArg,
                            terrain.Type.TypeName, terrain.Type.Name.ToUpperInvariant(), placementCell);
                    }
                    else if (occupier is InfantryGroup ig)
                    {
                        Infantry[] infList = ig.Infantry.Where(i => i != null).ToArray();
                        string infNames = String.Join(", ", infList.Select(i => i.Type.Name.ToUpperInvariant()).ToArray());
                        if (infList.Length > 1)
                        {
                            infNames = "(" + infNames + ")";
                        }
                        blocker = infList[0].TechnoType.TypeName + " " + infNames;
                    }
                    else if (occupier is ITechno tech)
                    {
                        blocker = tech.TechnoType.TypeName + " " + tech.TechnoType.Name.ToUpperInvariant();
                    }
                    else
                    {
                        blocker = IniParseConstants.ParseBlockerUnknown;
                    }
                    errors.Add(String.Format(IniParseConstants.ParseBlockerMulticell,
                        curType, kvp.Key, name, cell, blocker, reportCell));
                    modified = true;
                    continue;
                }
                // Optional trigger
                if (tokens.Length > 1)
                {
                    if (!caseTrigs.ContainsKey(tokens[1]))
                    {
                        warnings.Add(String.Format(IniParseConstants.ParseTriggerUnknownObj,
                            curType, kvp.Key, name, cell, tokens[1]));
                        modified = true;
                        newTerr.Trigger = Trigger.None;
                    }
                    else if (!checkTerrTrigs.Contains(tokens[1]))
                    {
                        warnings.Add(String.Format(IniParseConstants.ParseTriggerIllegalObj,
                            curType, kvp.Key, name, cell, caseTrigs[tokens[1]]));
                        modified = true;
                        newTerr.Trigger = Trigger.None;
                    }
                    else
                    {
                        // Adapt to same case
                        newTerr.Trigger = caseTrigs[tokens[1]];
                    }
                }
                errors.AddRange(warnings);
            }
        }

        private void LoadIniOverlay(INI ini, List<string> errors, ref bool modified)
        {
            INISection overlaySection = ini.Sections.Extract("Overlay");
            if (overlaySection == null)
            {
                return;
            }
            // Get special road type.
            OverlayType road2 = Map.OverlayTypes.FirstOrDefault(ov => (ov.Flags & OverlayTypeFlag.RoadSpecial) != OverlayTypeFlag.None && ov.ForceTileNr == 1 && ov.GraphicsSource != ov.Name);
            OverlayType road = road2 == null ? null : Map.OverlayTypes.FirstOrDefault(ov => ov.Equals(road2.GraphicsSource));
            IEnumerable<OverlayType> allTypes = OverlayTypes.GetAllTypes();
            int lastLine = Map.Metrics.Height - 1;
            List<int> addedConcrete = new List<int>();
            bool squishWarningAdded = false;
            const string curType = "Overlay";
            foreach (KeyValuePair<string, string> kvp in overlaySection)
            {
                if (!Int32.TryParse(kvp.Key, out int cell))
                {
                    errors.Add(String.Format(IniParseConstants.ParseCellKeyBad,
                        curType, kvp.Key, kvp.Value));
                    modified = true;
                    continue;
                }
                if (!Map.Metrics.GetLocation(cell, out Point point))
                {
                    errors.Add(String.Format(IniParseConstants.ParseCellKeyIllegal,
                        curType, kvp.Key, kvp.Value));
                    modified = true;
                    continue;
                }
                OverlayType overlayType = Map.OverlayTypes.Where(t => t.Equals(kvp.Value)).FirstOrDefault();
                if (overlayType == null)
                {
                        errors.Add(String.Format(IniParseConstants.ParseTypeUnknown,
                            curType, kvp.Key, curType, kvp.Value));
                        modified = true;
                }
                string name = overlayType.Name.ToUpperInvariant();
                if (Globals.DisableSquishMark && (overlayType.Flags & OverlayTypeFlag.Gross) != OverlayTypeFlag.None)
                {
                    errors.Add(String.Format(IniParseConstants.TypeSkipped,
                        curType, name));
                    if (!squishWarningAdded)
                    {
                        errors.Add(String.Format(IniParseConstants.ConsultManual, IniParseConstants.SettingNoSquishMark));
                        squishWarningAdded = true;
                    }
                    modified = true;
                }
                if (!Map.Metrics.Contains(cell))
                {
                    errors.Add(String.Format(IniParseConstants.ParseCellIllegal,
                        curType, kvp.Key, name, cell));
                    modified = true;
                    continue;
                }
                if (point.Y == 0 || point.Y == lastLine)
                {
                    errors.Add(String.Format(IniParseConstants.ParseOverlayTopBottom,
                        curType, kvp.Key, name, kvp.Value));
                    modified = true;
                    continue;
                }
                if (overlayType.IsConcrete && Globals.FixConcretePavement)
                {
                    addedConcrete.Add(cell);
                }
                if (Globals.FilterTheaterObjects && !overlayType.ExistsInTheater)
                {
                    errors.Add(String.Format(IniParseConstants.ParseTheaterBadCell,
                        curType, kvp.Value, curType, name, cell));
                    modified = true;
                    continue;
                }
                if ((overlayType.IsWall || overlayType.IsSolid) && Map.Buildings.ObjectAt(cell, out ICellOccupier occupier))
                {
                    string blocker;
                    if (occupier is ITechno tech)
                    {
                        blocker = tech.TechnoType.TypeName + " " + tech.TechnoType.Name.ToUpperInvariant();
                    }
                    else if (occupier is Overlay ovl)
                    {
                        blocker = IniParseConstants.OverlayTypeDescription(ovl.Type);
                    }
                    else
                    {
                        // Should never happen since overlay and buildings are a separate layer now.
                        blocker = IniParseConstants.ParseBlockerUnknown;
                    }
                    errors.Add(String.Format(IniParseConstants.ParseBlocker,
                        curType, kvp.Key, IniParseConstants.OverlayTypeDescription(overlayType), cell, blocker));
                    modified = true;
                    continue;
                }
                // Found second cell of ROAD overlay; make it ROAD2.
                // This can be done by prefixing cell numbers with 0.
                Overlay overlayOcc = Map.Overlay[cell];
                if (overlayOcc != null
                    && overlayType == road
                    && overlayOcc.Type == road)
                {
                    overlayType = road2;
                    overlayOcc = null;
                }
                // Besides the ROAD upgrade, do not allow overlay to override previous entries.
                if (overlayOcc == null)
                {
                    Map.Overlay[cell] = new Overlay { Type = overlayType, Icon = 0 };
                }
            }
            if (Globals.FixConcretePavement)
            {
                List<string> additionalConcCells = new List<string>();
                foreach (int conc in addedConcrete)
                {
                    OverlayType currentOverlay = Map.Overlay[conc]?.Type;
                    bool isOdd = (conc % 2) != 0;
                    int extracell = -1;
                    bool hasAdjacent = Map.Metrics.Adjacent(conc, isOdd ? FacingType.West : FacingType.East, out extracell);
                    // This is before any concrete fixing is done, so no need to check for ignorable concrete cells.
                    OverlayType adjacentOverlay = Map.Overlay[extracell]?.Type;
                    if (currentOverlay != null && hasAdjacent && adjacentOverlay == null)
                    {
                        Map.Overlay[extracell] = new Overlay { Type = currentOverlay, Icon = 0 };
                        additionalConcCells.Add(extracell.ToString());
                    }
                }
                if (additionalConcCells.Count > 0)
                {
                    modified = true;
                    errors.Add(String.Format(IniParseConstants.ParseHandleConcPairs, String.Join(", ", additionalConcCells.ToArray())));
                }
            }
        }

        private void LoadIniWaypoints(INI ini, List<string> errors, ref bool modified)
        {
            INISection waypointsSection = ini.Sections.Extract("Waypoints");
            if (waypointsSection == null || waypointsSection.Count == 0)
            {
                return;
            }
            string curType = "Waypoint";
            foreach (KeyValuePair<string, string> kvp in waypointsSection)
            {
                if (!Int32.TryParse(kvp.Key, out int waypoint))
                {
                    errors.Add(String.Format(IniParseConstants.ParseIntKeyBad,
                        curType, kvp.Key));
                    modified = true;
                    continue;
                }
                if (waypoint != 0 && kvp.Key.StartsWith("0"))
                {
                    errors.Add(String.Format(IniParseConstants.ParseIntKeyPadded,
                        curType, kvp.Key));
                    modified = true;
                    continue;
                }
                string wpNr = "#" + waypoint;
                if (!Int32.TryParse(kvp.Value, out int cell))
                {
                    errors.Add(String.Format(IniParseConstants.ParseCellBad,
                        curType, kvp.Key, wpNr, kvp.Value));
                    modified = true;
                    continue;
                }
                // Waypoint range. don't bother reporting empty entries.
                if ((waypoint < 0 || waypoint >= Map.Waypoints.Length))
                {
                    if (cell != -1)
                    {
                        errors.Add(String.Format(IniParseConstants.ParseIntKeyRange,
                            curType, kvp.Key, 0, Map.Waypoints.Length - 1));
                        modified = true;
                    }
                    continue;
                }
                if (!Map.Metrics.Contains(cell))
                {
                    Map.Waypoints[waypoint].Cell = null;
                    // Skip empty entries without error.
                    if (cell != -1)
                    {
                        errors.Add(String.Format(IniParseConstants.ParseCellIllegal,
                            curType, kvp.Key, wpNr, cell));
                        modified = true;
                    }
                    continue;
                }
                Map.Waypoints[waypoint].Cell = cell;
            }
        }

        private void LoadIniCellTriggers(INI ini, Dictionary<string, string> caseTrigs, HashSet<string> checkCellTrigs, List<string> errors, ref bool modified)
        {
            INISection cellTriggersSection = ini.Sections.Extract("CellTriggers");
            if (cellTriggersSection == null || cellTriggersSection.Count == 0)
            {
                return;
            }
            string curType = "Cell trigger";
            foreach (KeyValuePair<string, string> kvp in cellTriggersSection)
            {
                if (!Int32.TryParse(kvp.Key, out int cell))
                {
                    errors.Add(String.Format(IniParseConstants.ParseCellKeyBad,
                        curType, kvp.Key, kvp.Value));
                    modified = true;
                    continue;
                }
                if (!Map.Metrics.Contains(cell))
                {
                    errors.Add(String.Format(IniParseConstants.ParseCellKeyIllegal,
                        curType, kvp.Key, cell));
                    modified = true;
                    continue;
                }
                if (!caseTrigs.ContainsKey(kvp.Value))
                {
                    errors.Add(String.Format(IniParseConstants.ParseTriggerUnknown,
                        curType, kvp.Key, kvp.Value));
                    modified = true;
                    continue;
                }
                if (!checkCellTrigs.Contains(kvp.Value))
                {
                    errors.Add(String.Format(IniParseConstants.ParseTriggerIllegal,
                        curType, kvp.Key, kvp.Value));
                    modified = true;
                    continue;
                }
                Map.CellTriggers[cell] = new CellTrigger(caseTrigs[kvp.Value]);
            }
        }

        private void LoadIniHouses(INI ini, List<string> errors, ref bool modified)
        {
            Dictionary<string, string> correctedEdges = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string edge in Globals.MapEdges)
                correctedEdges.Add(edge, edge);
            string defaultEdge = Globals.MapEdges.FirstOrDefault() ?? String.Empty;
            foreach (Model.House house in Map.Houses)
            {
                if (house.Type.IsSpecial)
                {
                    continue;
                }
                House gameHouse = (House)house;
                ParseHouseSection(ini, gameHouse, correctedEdges, defaultEdge, errors, ref modified);
            }
        }

        private void ParseHouseSection(INI ini, House house, Dictionary<string, string> correctedEdges, string defaultEdge, List<string> errors, ref bool modified)
        {
            List<(string, string)> newErrors = new List<(string, string)>();
            INISection houseSection = INITools.ParseAndLeaveRemainder(ini, house.Type.Name, house, new MapContext(Map, false), newErrors);
            string curType = "House";
            if (newErrors.Count > 0)
            {
                modified = true;
                foreach ((string key, string err) in newErrors)
                {
                    errors.Add(String.Format(IniParseConstants.SectionPropError,
                        curType, house.Type.Name, key, err));
                }
            }
            if (!correctedEdges.ContainsKey(house.Edge))
            {
                errors.Add(String.Format(IniParseConstants.ParseEdgeIllegal,
                    curType, house.Type.Name, house.Edge, defaultEdge));
                house.Edge = defaultEdge;
                modified = true;
            }
            else
            {
                house.Edge = correctedEdges[house.Edge];
            }
            house.Enabled = houseSection != null;
        }

        private void CheckSwitchToSolo(bool tryCheckSoloMission, bool dontReportSwitch, List<string> errors)
        {
            bool switchedToSolo = false;
            if (tryCheckSoloMission && !Map.BasicSection.SoloMission)
            {
                List<Trigger> triggers = Map.Triggers;
                // In research mode, always assume the filename alone is sufficient.
                switchedToSolo = Globals.ResearchMode
                    || (triggers.Any(t => t.Action1.ActionType == ActionTypes.ACTION_WIN) && triggers.Any(t => t.Action1.ActionType == ActionTypes.ACTION_LOSE))
                    || triggers.Any(t => t.Event1.EventType == EventTypes.EVENT_ANY && t.Action1.ActionType == ActionTypes.ACTION_WINLOSE);
            }
            if (switchedToSolo)
            {
                Map.BasicSection.SoloMission = true;
                if ((!dontReportSwitch && Globals.ReportMissionDetection) || errors.Count > 0)
                {
                    errors.Insert(0, IniParseConstants.SoloMissionDetected);
                }
            }
        }

        protected IEnumerable<string> LoadMapPack(INI ini, CellGrid<Template> target, ref bool modified)
        {
            List<string> errors = new List<string>();
            target.Clear();
            TemplateType[] templateTypes = Map.GetMapTemplateTypes();
            INISection mapPackSection = ini.Sections.Extract("MapPack");
            if (mapPackSection == null)
            {
                errors.Add("Section \"[MapPack]\" not found!");
                return errors;
            }
            Map.Templates.Clear();
            const int typeSize = 2;
            const int iconSize = 1;
            byte[] data = INITools.DecompressLCWSection(mapPackSection, Map.Metrics, typeSize + iconSize, errors, ref modified);
            if (data == null)
            {
                return errors;
            }
            int width = Map.Metrics.Width;
            int height = Map.Metrics.Height;
            int indexType = 0;
            int indexIcon = width * height * typeSize;
            int cell = 0;
            for (int y = 0; y < width; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    int typeValue = data[indexType] + (data[indexType + 1] << 8);
                    if (typeValue == 0xFFFF)
                    {
                        typeValue = 0xFF;
                    }
                    indexType += typeSize;
                    int iconValue = data[indexIcon];
                    indexIcon += iconSize;
                    TemplateType templateType = ChecKTemplateType(templateTypes, typeValue, iconValue, cell, x, y, errors, ref modified);
                    target[y, x] = (templateType != null) ? new Template { Type = templateType, Icon = iconValue } : null;
                    cell++;
                }
            }
            return errors;
        }

        protected IEnumerable<string> LoadBinaryClassic(BinaryReader reader, CellGrid<Template> target, ref bool modified)
        {
            List<string> errors = new List<string>();
            target.Clear();
            TemplateType[] templateTypes = Map.GetMapTemplateTypes();
            int width = target.Metrics.Width;
            int height = target.Metrics.Width;
            int cell = 0;
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    byte typeValue = reader.ReadByte();
                    byte iconValue = reader.ReadByte();
                    TemplateType templateType = ChecKTemplateType(templateTypes, typeValue, iconValue, cell, x, y, errors, ref modified);
                    target[y, x] = (templateType != null) ? new Template { Type = templateType, Icon = iconValue } : null;
                    cell++;
                }
            }
            return errors;
        }

        protected IEnumerable<string> LoadBinaryMega(BinaryReader reader, CellGrid<Template> target, ref bool modified)
        {
            List<string> errors = new List<string>();
            target.Clear();
            TemplateType[] templateTypes = Map.GetMapTemplateTypes();
            long dataLen = reader.BaseStream.Length;
            int mapLen = target.Metrics.Length;
            int mapWidth = target.Metrics.Width;
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
                TemplateType templateType = ChecKTemplateType(templateTypes, typeValue, iconValue, cell, x, y, errors, ref modified);
                target[y,x] = (templateType != null) ? new Template { Type = templateType, Icon = iconValue } : null;
            }
            return errors;
        }

        protected TemplateType ChecKTemplateType(TemplateType[] templateTypes, int typeValue, int iconValue, int cell, int x, int y, List<string> errors, ref bool modified)
        {
            // Ignore clear terrain
            if (typeValue == 0xFF)
            {
                return null;
            }
            // Prevent loading of illegal tiles.
            TemplateType templateType = typeValue >= templateTypes.Length ? null : templateTypes[typeValue];
            if (templateType == null)
            {
                errors.Add(String.Format("Unknown template value {0:X2} at cell {1} [{2},{3}]; clearing.", typeValue, cell, x, y));
                modified = true;
                return null;
            }
            if (templateType.Flags.HasFlag(TemplateTypeFlag.Clear) || templateType.Flags.HasFlag(TemplateTypeFlag.Group))
            {
                // No explicitly set Clear terrain allowed. Also no explicitly set versions allowed of the "group" dummy entries.
                templateType = null;
            }
            else if (!templateType.ExistsInTheater && Globals.FilterTheaterObjects)
            {
                errors.Add(String.Format("Template '{0}' at cell {1} [{2},{3}] is not available in the set theater; clearing.", templateType.Name.ToUpper(), cell, x, y));
                modified = true;
                templateType = null;
            }
            else if (iconValue >= templateType.NumIcons)
            {
                errors.Add(String.Format("Template '{0}' at cell {1} [{2},{3}] has an icon set ({4}) that is outside its icons range; clearing.", templateType.Name.ToUpper(), cell, x, y, iconValue));
                modified = true;
                templateType = null;
            }
            else if (!templateType.IsRandom && templateType.IconMask != null && !templateType.IconMask[iconValue / templateType.IconWidth, iconValue % templateType.IconWidth])
            {
                errors.Add(String.Format("Template '{0}' at cell {1} [{2},{3}] has an icon set ({4}) that is not part of its placeable cells; clearing.", templateType.Name.ToUpper(), cell, x, y, iconValue));
                modified = true;
                templateType = null;
            }
            return templateType;
        }

        /// <summary>
        /// Applies any rules in the given extra ini content to the plugin.
        /// </summary>
        /// <param name="extraIniText">Ini content that remains after parsing an ini file. If null, only a rules reset is performed.</param>
        /// <param name="forFootprintTest">Don't apply changes, just test the result for <paramref name="footPrintsChanged"/></param>
        /// <param name="footPrintsChanged">Returns true if any building footprints were changed as a result of the ini rule changes.</param>
        /// <returns>Any errors in parsing the <paramref name="extraIniText"/> contents.</returns>
        protected virtual List<string> ResetMissionRules(INI extraIniText, bool forFootprintTest, out bool footPrintsChanged, HashSet<Point> refreshPoints)
        {
            List<string> errors = new List<string>();
            Dictionary<string, bool> bibBackups = Map.BuildingTypes.ToDictionary(b => b.Name, b => b.HasBib, StringComparer.OrdinalIgnoreCase);
            errors.AddRange(UpdateBuildingRules(extraIniText, Map, forFootprintTest, refreshPoints));
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
            if (!forFootprintTest)
            {
                errors.AddRange(UpdateHouseRules(extraIniText, Map, refreshPoints));
            }
            Map.NotifyRulesChanges(refreshPoints);
            return errors;
        }

        private static IEnumerable<string> UpdateBuildingRules(INI ini, Map map, bool forFootPrintTest, HashSet<Point> refreshPoints)
        {
            const string CapturableKey= "Capturable";
            bool disableAllBibs = false;
            INISection basicSection = ini.Sections["Basic"];
            if (basicSection != null)
            {
                string noBibs = basicSection.TryGetValue("NoBibs");
                disableAllBibs = YesNoBooleanTypeConverter.Parse(noBibs);
            }                
            List<string> errors = new List<string>();
            Dictionary<string, BuildingType> originals = BuildingTypes.GetTypes(true).ToDictionary(b => b.Name, StringComparer.OrdinalIgnoreCase);
            List<(Point Location, Building Occupier)> buildings = map.Buildings.OfType<Building>()
                 .OrderBy(pb => pb.Location.Y * map.Metrics.Width + pb.Location.X).ToList();
            // Remove all buildings
            if (!forFootPrintTest)
            {
                foreach ((Point p, Building b) in buildings)
                {
                    refreshPoints?.UnionWith(OccupierSet.GetOccupyPoints(p, b));
                    map.Buildings.Remove(b);
                }
            }
            // Potentially add new bibs that obstruct stuff
            foreach (BuildingType bType in map.BuildingTypes)
            {
                // No rules to read for walls.
                if (bType.IsWall || !originals.TryGetValue(bType.Name, out BuildingType orig))
                {
                    continue;
                }
                bType.HasBib = !disableAllBibs && orig.HasBib;
                if (!forFootPrintTest)
                {
                    bType.Capturable = orig.Capturable;
                    INISection bldSettings = ini[bType.Name];
                    if (bldSettings == null)
                    {
                        continue;
                    }
                    BuildingSection bld = new BuildingSection();
                    try
                    {
                        List<(string, string)> parseErrors = INI.ParseSection(new MapContext(map, false), bldSettings, bld, true);
                        foreach ((string iniKey, string error) in parseErrors.Where(b => CapturableKey.Equals(b.Item1, StringComparison.InvariantCulture)))
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
                    if (bldSettings.Keys.Contains(CapturableKey))
                    {
                        bType.Capturable = bld.Capturable;
                    }
                }
            }
            if (forFootPrintTest)
            {
                return errors;
            }
            // Try re-adding the buildings.
            foreach ((Point p, Building b) in buildings)
            {
                refreshPoints?.UnionWith(OccupierSet.GetOccupyPoints(p, b));
                map.Buildings.Add(p, b);
            }
            return errors;
        }

        private IEnumerable<string> UpdateHouseRules(INI ini, Map map, HashSet<Point> refreshPoints)
        {
            List<string> errors = new List<string>();
            // Not going to bother implementing this in remastered mode.
            if (!Globals.UseClassicFiles)
            {
                return errors;
            }
            List<(Point Location, Building Occupier)> buildings = map.Buildings.OfType<Building>()
                 .OrderBy(pb => pb.Location.Y * map.Metrics.Width + pb.Location.X).ToList();
            List<(Point Location, Unit Occupier)> units = map.Technos.OfType<Unit>()
                 .OrderBy(pb => pb.Location.Y * map.Metrics.Width + pb.Location.X).ToList();
            List<(Point Location, InfantryGroup Occupier)> inf = map.Technos.OfType<InfantryGroup>()
                 .OrderBy(pb => pb.Location.Y * map.Metrics.Width + pb.Location.X).ToList();

            foreach (House house in Map.Houses.Cast<House>())
            {
                INISection basicSection = ini.Sections[house.Type.Name];
                House.PlayerColorType primarySchemeOrig = house.PrimaryScheme;
                House.PlayerColorType secondarySchemeOrig = house.SecondaryScheme;
                if (basicSection == null)
                {
                    house.PrimaryScheme = null;
                    house.SecondaryScheme = null;
                }
                else
                {
                    string primScheme = basicSection.TryGetValue("ColorScheme");
                    house.PrimaryScheme = House.PlayerColorType.GetPlayerColor(primScheme);
                    if (!String.IsNullOrWhiteSpace(primScheme) && house.PrimaryScheme == null && !"None".Equals(primScheme, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add(String.Format("Unknown value \"{0}\"for ColorScheme on House {1}", primScheme.Trim(), house.Type.Name));
                    }
                    string secScheme = basicSection.TryGetValue("SecondaryScheme");
                    if ("None".Equals(secScheme, StringComparison.OrdinalIgnoreCase))
                    {
                        house.SecondaryScheme = new House.PlayerColorType();
                    }
                    else
                    {
                        house.SecondaryScheme = House.PlayerColorType.GetPlayerColor(secScheme);
                        if (!String.IsNullOrWhiteSpace(secScheme) && house.SecondaryScheme == null)
                        {
                            errors.Add(String.Format("Unknown value \"{0}\"for SecondaryScheme on House {1}", secScheme.Trim(), house.Type.Name));
                        }
                    }
                }
                if (refreshPoints != null && (house.PrimaryScheme != primarySchemeOrig || house.SecondaryScheme != secondarySchemeOrig))
                {
                    foreach ((Point Location, Building Occupier) in buildings.Where(b => b.Occupier.House == house.Type))
                    {
                        refreshPoints.UnionWith(OccupierSet.GetOccupyPoints(Location, Occupier));
                    }
                    foreach ((Point Location, Unit Occupier) in units.Where(u => u.Occupier.House == house.Type))
                    {
                        refreshPoints.UnionWith(OccupierSet.GetOccupyPoints(Location, Occupier));
                    }
                    foreach ((Point Location, InfantryGroup Occupier) in inf.Where(b => b.Occupier.Infantry.Any(i => i != null && i.House == house.Type)))
                    {
                        refreshPoints.UnionWith(OccupierSet.GetOccupyPoints(Location, Occupier));
                    }
                }
            }
            return errors;
        }

        public virtual long Save(string path, FileType fileType)
        {
            return Save(path, fileType, null, false, false);
        }

        public virtual long Save(string path, FileType fileType, Bitmap customPreview, bool dontResavePreview, bool forSteam)
        {
            return Save(path, fileType, false, customPreview, dontResavePreview, forSteam);
        }

        public long Save(string path, FileType fileType, bool forSole, Bitmap customPreview, bool dontResavePreview, bool forSteam)
        {
            string errors = Validate(fileType, LoadedFileType, false, false, forSole);
            if (errors != null)
            {
                return 0;
            }
            bool isN64 = fileType == FileType.I64 || fileType == FileType.B64;
            string binExtension = isN64 ? ".map" : ".bin";
            string iniPath = fileType == FileType.INI || fileType == FileType.MPR || fileType == FileType.I64 ? path : Path.ChangeExtension(path, ".ini");
            string binPath = fileType == FileType.BIN || fileType == FileType.B64 ? path : Path.ChangeExtension(path, binExtension);
            Encoding dos437 = Encoding.GetEncoding(437);
            Encoding utf8 = new UTF8Encoding(false, false);
            byte[] linebreak = utf8.GetBytes("\r\n");
            INI ini = new INI();
            List<(string section, string key)> utf8Components = new List<(string section, string key)>();
            utf8Components.AddRange(new[] { ("Steam", null), ("Briefing", "Text"), ("Basic", "Author") });
            if (!Globals.UseClassicFiles || !Globals.ClassicEncodesNameAsCp437 || fileType == FileType.PGM)
            {
                utf8Components.Add(("Basic", "Name"));
            }
            long retVal = 0;
            switch (fileType)
            {
                case FileType.INI:
                case FileType.BIN:
                case FileType.MPR:
                case FileType.I64:
                case FileType.B64:
                    SaveINI(ini, fileType, path, forSteam);
                    using (FileStream iniStream = new FileStream(iniPath, FileMode.Create))
                    using (BinaryWriter iniWriter = new BinaryWriter(iniStream))
                    {
                        // Use '\n' in proprocessing for simplicity. WriteMultiEncoding will use full line breaks.
                        string iniText = ini.ToString("\n");
                        //string iniText = forSole ? ini.ToString("\n") : FixRoad2Save(ini, "\n");
                        GeneralUtils.WriteMultiEncoding(iniText.Split('\n'), iniWriter, dos437, utf8, utf8Components.ToArray(), linebreak);
                        retVal = iniStream.Position;
                    }
                    // MPR = experimental "embedded map" like RA has. If enabled, don't write external .bin file.
                    if (fileType != FileType.MPR)
                    {
                        using (FileStream binStream = new FileStream(binPath, FileMode.Create))
                        using (BinaryWriter binWriter = new BinaryWriter(binStream))
                        {
                            if (!isMegaMap)
                            {
                                if (!isN64)
                                {
                                    SaveBinaryClassic(binWriter);
                                }
                                else
                                {
                                    SaveBinaryClassicN64(binWriter, false);
                                }
                            }
                            else
                            {
                                if (isN64)
                                {
                                    throw new NotSupportedException("Megamaps cannot be saved in Nintendo 64 format.");
                                }
                                SaveBinaryMega(binWriter);
                            }
                        }
                    }
                    // None of this junk for Sole Survivor.
                    if (!forSole && !Map.BasicSection.SoloMission && (!Globals.UseClassicFiles || !Globals.ClassicProducesNoMetaFiles) && !forSteam)
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
                                    SaveMapPreview(tgaStream);
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
                case FileType.PGM:
                    SaveINI(ini, fileType, path, true);
                    using (MemoryStream iniStream = new MemoryStream())
                    using (MemoryStream binStream = new MemoryStream())
                    using (MemoryStream tgaStream = new MemoryStream())
                    using (MemoryStream jsonStream = new MemoryStream())
                    using (BinaryWriter iniWriter = new BinaryWriter(iniStream))
                    using (BinaryWriter binWriter = new BinaryWriter(binStream))
                    using (JsonTextWriter jsonWriter = new JsonTextWriter(new StreamWriter(jsonStream)))
                    using (MegafileBuilder megafileBuilder = new MegafileBuilder(String.Empty, path))
                    {
                        string iniText = ini.ToString("\n");
                        //string iniText = forSole ? ini.ToString("\n") : FixRoad2Save(ini, "\n");
                        GeneralUtils.WriteMultiEncoding(iniText.Split('\n'), iniWriter, dos437, utf8, utf8Components.ToArray(), linebreak);
                        iniWriter.Flush();
                        retVal = iniStream.Position;
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
                            SaveMapPreview(tgaStream);
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
            return retVal;
        }

        private void SaveSteamInfoIni(INI ini)
        {
            INISection generalSection = new INISection("General");
            generalSection["AuthorId"] = Map.SteamSection.Author;
            generalSection["Author"] = Map.BasicSection.Author;
            generalSection["ContentType"] = "0";
            generalSection["Flags"] = String.Empty;
            ini.Sections.Add(generalSection);

            INISection titleSection = new INISection("Title");
            titleSection["0"] = Map.BasicSection.Name;
            ini.Sections.Add(titleSection);

            INISection descriptionSection = new INISection("Description");
            string description = RedAlert.GamePluginRA.PreprocessBriefingText(Map.SteamSection.Description);
            List<string> finalLines = RedAlert.GamePluginRA.WriteClassicBriefing(description, Constants.BriefLineCutoffClassic);
            for (int i = 0; i < finalLines.Count; ++i)
            {
                descriptionSection[(i + 1).ToString()] = finalLines[i];
            }
            ini.Sections.Add(descriptionSection);
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
            OverlayType road2 = Map.OverlayTypes.FirstOrDefault(ov => (ov.Flags & OverlayTypeFlag.RoadSpecial) != OverlayTypeFlag.None && ov.ForceTileNr > 0 && ov.GraphicsSource != ov.Name);
            if (road2 != null)
            {
                string roadLine = "=" + road2.GraphicsSource.ToUpperInvariant() + lineEnd;
                Regex roadDetect = new Regex("^\\s*(\\d+)\\s*=\\s*" + road2.Name + "\\s*$", RegexOptions.IgnoreCase);
                StringBuilder output = new StringBuilder();
                string[] iniString = iniString1.Split('\n');
                // Quick and dirty ini parser to find the correct ini section.
                bool inOverlay = false;
                for (int i = 0; i < iniString.Length; ++i)
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

        protected virtual void SaveINI(INI ini, FileType fileType, string fileName, bool forSteam)
        {
            if (extraSections != null)
            {
                ini.Sections.AddRange(extraSections.Clone());
            }
            SaveIniBasic(ini, fileName);
            SaveIniMap(ini);
            if (!forSteam)
            {
                SaveIniSteam(ini);
            }
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
            SaveIniTerrain(ini);
            if (fileType == FileType.MPR)
            {
                SaveMapPack(ini);
            }
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
                for (int i = 0; i < name.Length; ++i)
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
            Map.MapSection.FixBounds(null);
            INISection mapSection = INITools.FillAndReAdd(ini, "Map", Map.MapSection, new MapContext(Map, false), true);
            if (isMegaMap)
            {
                mapSection["Version"] = "1";
            }
            INI.WriteSection(new MapContext(Map, false), mapSection, Map.MapSection);
            return mapSection;
        }

        protected INISection SaveIniSteam(INI ini)
        {
            if (Map.SteamSection.PublishedFileId == 0)
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
            if (String.IsNullOrEmpty(Map.BriefingSection.Briefing))
            {
                if (oldSection != null)
                {
                    ini.Sections.Add(oldSection);
                    return oldSection;
                }
                return null;
            }
            INISection briefingSection = ini.Sections.Add("Briefing");
            string briefText = PreprocessBriefingText(Map.BriefingSection.Briefing);
            if (Globals.WriteRemasterBriefing)
            {
                // Remastered TD supports line breaks as @ characters.
                briefingSection["Text"] = briefText.Replace('\n', '@');
            }
            // If both disabled, default to classic one
            if (Globals.WriteClassicBriefing || (!Globals.WriteRemasterBriefing && !Globals.WriteClassicBriefing))
            {
                if (briefText.Length > Constants.MaxBriefLengthClassic)
                {
                    briefText = briefText.Substring(0, Constants.MaxBriefLengthClassic);
                }
                List<string> finalLines;
                string[] lines = briefText.Split('\n', true);
                // If 1.06 line breaks are disabled, remove all line breaks from the briefing.
                if (!Globals.EnableTd106LineBreaks)
                {
                    for (int i = 0; i > lines.Length; ++i)
                    {
                        lines[i] = lines[i].Trim();
                    }
                    lines = new string[] { String.Join(" ", lines) };
                }
                if (!Globals.EnableTdClassicMultiLine)
                {
                    // Logic for either 1.06 line breaks. With line breaks stripped out,
                    // this is just the line split logic for normal briefing writing.
                    // 1.06 line breaks are a bit odd, in that the ## to trigger
                    // the break must be put on the end of the line.
                    finalLines = new List<string>();
                    int last = lines.Length - 1;
                    for (int i = 0; i < lines.Length; ++i)
                    {
                        string line = lines[i].Trim();
                        if (i != last)
                        {
                            // This logic never triggers for non-1.06 briefings since everything is put on one line.
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
                }
                else
                {
                    string briefTextAt = briefText.Replace("\n", "@");
                    finalLines = RedAlert.GamePluginRA.WriteClassicBriefing(briefTextAt, Constants.BriefLineCutoffClassic);
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

        private string PreprocessBriefingText(string briefText)
        {
            // Remove tabs, trim off spaces and line breaks.
            briefText = (briefText ?? String.Empty).Replace('\t', ' ').Trim('\r', '\n', ' ').Replace("\r\n", "\n").Replace("\r", "\n");
            // Remove duplicate spaces.
            briefText = Regex.Replace(briefText, " +", " ");
            // Remove spaces around line breaks.
            briefText = Regex.Replace(briefText, " *\\n *", "\n");
            return briefText;
        }

        private INISection SaveMapPack(INI ini)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                for (int y = 0; y < Map.Metrics.Height; ++y)
                {
                    for (int x = 0; x < Map.Metrics.Width; ++x)
                    {
                        Template template = Map.Templates[y, x];
                        if (template != null && (template.Type.Flags & TemplateTypeFlag.Clear) == TemplateTypeFlag.None)
                        {
                            writer.Write((ushort)template.Type.ID);
                        }
                        else
                        {
                            writer.Write(UInt16.MaxValue);
                        }
                    }
                }
                for (int y = 0; y < Map.Metrics.Height; ++y)
                {
                    for (int x = 0; x < Map.Metrics.Width; ++x)
                    {
                        Template template = Map.Templates[y, x];
                        if (template != null && (template.Type.Flags & TemplateTypeFlag.Clear) == TemplateTypeFlag.None)
                        {
                            writer.Write((byte)template.Icon);
                        }
                        else
                        {
                            writer.Write((byte)0);
                        }
                    }
                }
                ini.Sections.Remove("MapPack");
                return INITools.CompressLCWSection(ini.Sections.Add("MapPack"), stream.ToArray());
            }
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
                    .Select(c => String.Format("{0}:{1}", c.Type.Name.ToUpperInvariant(), c.Count))
                    .ToArray();
                string[] missions = teamType.Missions
                    .Select(m => String.Format("{0}:{1}", m.Mission.Mission, m.Argument))
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
                    String.Join(",", classes),
                    missions.Length.ToString(),
                    String.Join(",", missions),
                    teamType.IsReinforcable ? "1" : "0",
                    teamType.IsPrebuilt ? "1" : "0"
                };
                teamTypesSection[teamType.Name] = String.Join(",", tokens.Where(t => !String.IsNullOrEmpty(t)));
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
                if (String.IsNullOrEmpty(trigger.Name))
                {
                    continue;
                }
                List<string> tokens = new List<string>
                {
                    trigger.Event1.EventType,
                    trigger.Action1.ActionType,
                    trigger.Event1.Data.ToString(),
                    String.IsNullOrEmpty(trigger.House) ? House.None : trigger.House,
                    String.IsNullOrEmpty(trigger.Action1.Team) ? TeamType.None : trigger.Action1.Team,
                    ((int)trigger.PersistentType).ToString()
                };
                triggersSection[trigger.Name] = String.Join(",", tokens);
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
                CleanBaseSection(ini, baseSectionOld);
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
                    baseSection[key] = String.Format("{0},{1}",
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
                    infantrySection[key] = String.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
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
                structuresSection[key] = String.Format("{0},{1},{2},{3},{4},{5}",
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
                unitsSection[key] = String.Format("{0},{1},{2},{3},{4},{5},{6}",
                    unit.House.Name,
                    unit.Type.Name.ToUpperInvariant(),
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
                aircraftSection[key] = String.Format("{0},{1},{2},{3},{4},{5}",
                    aircraft.House.Name,
                    aircraft.Type.Name.ToUpperInvariant(),
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
            foreach (Model.House house in Map.Houses.Where(h => !h.Type.IsSpecial).OrderBy(h => h.Type.ID))
            {
                House gameHouse = (House)house;
                bool enabled = house.Enabled;
                string name = gameHouse.Type.Name;
                INISection houseSection = INITools.FillAndReAdd(ini, name, gameHouse, new MapContext(Map, false), enabled);
                // Current house is not in its own alliances list. Fix that.
                if (houseSection != null && !gameHouse.Allies.Contains(gameHouse.Type.ID))
                {
                    HashSet<string> allies = (houseSection.TryGetValue("Allies") ?? String.Empty)
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
                OverlayType ovlt = overlay.Type;
                bool isRoad2 = ovlt == OverlayTypes.Road2;
                if (isRoad2)
                {
                    ovlt = OverlayTypes.Road;
                }
                if (Map.IsIgnorableConcrete(overlay))
                {
                    continue;
                }
                string overlayName = ovlt.Name.ToUpperInvariant();
                if ((ovlt.Flags & OverlayTypeFlag.TiberiumOrGold) == OverlayTypeFlag.TiberiumOrGold)
                    overlayName = "TI" + rd.Next(1, 13);
                overlaySection[cell.ToString()] = overlayName;
                // Add second cell of ROAD.
                if (isRoad2)
                {
                    overlaySection["0" + cell.ToString()] = overlayName;
                }
            }
            return overlaySection;
        }

        protected INISection SaveIniSmudge(INI ini)
        {
            INISection smudgeSection = ini.Sections.Add("Smudge");
            // Flatten multi-cell bibs
            Dictionary<int, Smudge> resolvedSmudge = new Dictionary<int, Smudge>();
            foreach (var (cell, smudge) in Map.Smudge.Where(item => !item.Value.IsAutoBib).OrderBy(s => s.Cell))
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
                smudgeSection[cell.ToString()] = String.Format("{0},{1},{2}", smudge.Type.Name.ToUpperInvariant(), cell, Math.Min(smudge.Type.Icons - 1, smudge.Icon));
            }
            return smudgeSection;
        }

        protected INISection SaveIniTerrain(INI ini)
        {
            INISection terrainSection = ini.Sections.Add("Terrain");
            Dictionary<int, int> cellDupes = new Dictionary<int, int>();
            foreach (var (location, terrain) in Map.Technos.OfType<Terrain>().OrderBy(t => Map.Metrics.GetCell(t.Location)))
            {
                if (Map.Metrics.GetCell(location, out int cell))
                {
                    string prefix = String.Empty;
                    if (cellDupes.TryGetValue(cell, out int amount))
                    {
                        prefix = new string(Enumerable.Repeat('0', amount).ToArray());
                    }
                    cellDupes[cell] = amount + 1;
                    terrainSection[prefix + cell.ToString()] = String.Format("{0},{1}", terrain.Type.Name.ToUpperInvariant(), terrain.Trigger);
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
                    if (template != null && (template.Type.Flags & TemplateTypeFlag.Clear) == 0)
                    {
                        writer.Write((byte)template.Type.ID);
                        writer.Write((byte)template.Icon);
                    }
                    else
                    {
                        writer.Write(Byte.MaxValue);
                        writer.Write((byte)0);
                    }
                }
            }
        }
        protected List<string> SaveBinaryClassicN64(BinaryWriter writer, bool check)
        {
            Dictionary<ushort, List<int>> notFound = check ? new Dictionary<ushort, List<int>>() : null;
            bool isDesert = TheaterTypes.Desert.Name.Equals(Map.Theater?.Name, StringComparison.OrdinalIgnoreCase);
            Dictionary<int, ushort> mapping = isDesert ? N64MapConverter.DESERT_MAPPING_REVERSED : N64MapConverter.TEMPERATE_MAPPING_REVERSED;
            string mappingName = isDesert ? TheaterTypes.Desert.Name : TheaterTypes.Temperate.Name;
            for (int y = 0; y < Map.Metrics.Height; ++y)
            {
                for (int x = 0; x < Map.Metrics.Width; ++x)
                {
                    Template template = Map.Templates[y, x];
                    if (template != null && (template.Type.Flags & TemplateTypeFlag.Clear) == 0)
                    {
                        byte template1 = (byte)template.Type.ID;
                        byte template2 = (byte)template.Icon;
                        if (mapping.TryGetValue(template1 << 8 | template2, out ushort writeVal))
                        {
                            writer.Write((byte)((writeVal >> 8) & 0xFF));
                            writer.Write((byte)(writeVal & 0xFF));
                            continue;
                        }
                        else if (check)
                        {
                            if (!notFound.TryGetValue(template.Type.ID, out List<int> cells))
                            {
                                cells = new List<int>();
                                notFound.Add(template.Type.ID, cells);
                            }
                            cells.Add(Map.Metrics.GetCell(new Point(x, y)).Value);
                        }
                    }
                    // Fallback: write clear terrain.
                    writer.Write(Byte.MaxValue);
                    writer.Write(Byte.MaxValue);
                }
            }
            if (!check)
            {
                return null;
            }
            List<string> errors = new List<string>();
            bool multiCellItems = false;
            if (notFound.Keys.Count > 0)
            {
                foreach (TemplateType tt in this.Map.TemplateTypes)
                {
                    if (notFound.TryGetValue(tt.ID, out List<int> cells))
                    {
                        bool multiple = cells.Count > 1;
                        errors.Add(String.Format("Template type \"{0}\" is not available in the Nintendo 64 map type. Found cell{1}: {2}",
                            tt.Name, multiple ? "s" : String.Empty, String.Join(", ", cells.Select(c => c.ToString()).ToArray())));
                        if (multiple)
                        {
                            multiCellItems = true;
                        }
                    }
                }
            }
            if (errors.Count > 0)
            {
                // This is not added as extra item, but pasted onto the last warning.
                bool plural = notFound.Keys.Count > 1 || multiCellItems;
                int last = errors.Count - 1;
                errors[last] = errors[last] + String.Format("\nIf the map is saved to this type, {0} will be replaced with clear terrain.",
                    plural ? "these cells" : "this cell");
            }
            return errors;
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
                    if (template == null || (template.Type.Flags & TemplateTypeFlag.Clear) != 0)
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

        protected void SaveMapPreview(Stream stream)
        {
            Map.GenerateMapPreview(this).Save(stream);
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
            // Writing the Home for singleplay maps is probably useless, but it's better than the player start points.
            WaypointFlag waypointType = Map.BasicSection.SoloMission ? WaypointFlag.Home : WaypointFlag.PlayerStart;
            foreach (Waypoint waypoint in Map.Waypoints.Where(w => w.Flags.HasFlag(waypointType)
                && w.Cell.HasValue && Map.Metrics.GetLocation(w.Cell.Value, out Point p) && Map.Bounds.Contains(p)))
            {
                writer.WriteValue(waypoint.Cell.Value);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public virtual string Validate(FileType saveType, FileType oldType, bool forResave, bool forWarnings)
        {
            return Validate(saveType, oldType, forResave, forWarnings, false);
        }

        protected string Validate(FileType saveType, FileType oldType, bool forResave, bool forWarnings, bool forSole)
        {
            GameInfo gi = GameInfo;
            bool isN64 = saveType == FileType.I64 || saveType == FileType.B64;
            bool wasN64 = oldType == FileType.I64 || oldType == FileType.B64;
            if (forWarnings)
            {
                return ValidateForWarnings(saveType, forResave, isN64, wasN64, forSole);
            }
            List<string> errors = new List<string>();
            if (isN64 && isMegaMap)
            {
                errors.Add("Megamaps are not supported by the Nintendo 64 map format.");
            }
            int numStartPoints = Map.Waypoints.Count(w => w.Flags.HasFlag(WaypointFlag.PlayerStart) && w.Cell.HasValue
                && Map.Metrics.GetLocation(w.Cell.Value, out Point pt) && Map.Bounds.Contains(pt));
            int numBadPoints = Map.Waypoints.Count(w => w.Flags.HasFlag(WaypointFlag.PlayerStart) && w.Cell.HasValue
                && Map.Metrics.GetLocation(w.Cell.Value, out Point pt) && !Map.Bounds.Contains(pt));
            if (Globals.EnforceObjectMaximums)
            {
                int numAircraft = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsAircraft).Count();
                int numBuildings = Map.Buildings.OfType<Building>().Where(x => x.Occupier.IsPrebuilt).Count();
                int numInfantry = Map.Technos.OfType<InfantryGroup>().Sum(item => item.Occupier.Infantry.Count(i => i != null));
                int numTerrain = Map.Technos.OfType<Terrain>().Count();
                int numUnits = Map.Technos.OfType<Unit>().Where(u => u.Occupier.Type.IsGroundUnit).Count();
                bool noSoleSkip = !forSole || !Globals.NoOwnedObjectsInSole;
                if (!Globals.DisableAirUnits && numAircraft > gi.MaxAircraft && noSoleSkip)
                {
                    errors.Add(String.Format("Maximum number of aircraft exceeded ({0} > {1})", numAircraft, gi.MaxAircraft));
                }
                if (numBuildings > gi.MaxBuildings && noSoleSkip)
                {
                    errors.Add(String.Format("Maximum number of structures exceeded ({0} > {1})", numBuildings, gi.MaxBuildings));
                }
                if (numInfantry > gi.MaxInfantry && noSoleSkip)
                {
                    errors.Add(String.Format("Maximum number of infantry exceeded ({0} > {1})", numInfantry, gi.MaxInfantry));
                }
                if (numTerrain > gi.MaxTerrain)
                {
                    errors.Add(String.Format("Maximum number of terrain objects exceeded ({0} > {1})", numTerrain, gi.MaxTerrain));
                }
                if (numUnits > gi.MaxUnits && noSoleSkip)
                {
                    errors.Add(String.Format("Maximum number of units exceeded ({0} > {1})", numUnits, gi.MaxUnits));
                }
            }
            // Ignore all further checks for Sole Survivor
            if (!forSole)
            {
                if (Globals.EnforceObjectMaximums)
                {
                    if (Map.TeamTypes.Count > gi.MaxTeams)
                    {
                        errors.Add(String.Format("Maximum number of team types exceeded ({0} > {1})", Map.TeamTypes.Count, gi.MaxTeams));
                    }
                    if (Map.Triggers.Count > gi.MaxTriggers)
                    {
                        errors.Add(String.Format("Maximum number of triggers exceeded ({0} > {1})", Map.Triggers.Count, gi.MaxTriggers));
                    }
                }
                if (!Map.BasicSection.SoloMission)
                {
                    if (numStartPoints < 2)
                    {
                        errors.Add("Skirmish/Multiplayer maps need at least 2 waypoints for player starting locations.");
                    }
                    if (numBadPoints > 0)
                    {
                        errors.Add("Skirmish/Multiplayer maps should not have player start waypoints placed outside the map bound.");
                    }
                }
                else
                {
                    Waypoint homeWaypoint = Map.Waypoints.Where(w => w.Flags.HasFlag(WaypointFlag.Home)).FirstOrDefault();
                    if ((!homeWaypoint.Cell.HasValue || !Map.Metrics.GetLocation(homeWaypoint.Cell.Value, out Point p) || !Map.Bounds.Contains(p)))
                    {
                        errors.Add("Single-player maps need the Home waypoint to be placed, inside the map bounds.");
                    }
                }
                IEnumerable<string> triggerErr = CheckTriggers(this.Map.Triggers, true, true, true, out bool triggerErrFatal, false, out _);
                if (triggerErrFatal)
                {
                    errors.AddRange(triggerErr);
                }
            }
            if (errors.Count > 0)
            {
                return "Error(s) during map validation:\n* "
                    + String.Join("\n* ", errors.ToArray());
            }
            return null;
        }

        private string ValidateForWarnings(FileType saveType, bool forResave, bool isN64, bool wasN64, bool forSole)
        {
            // Check if map has name
            List<string> errors = new List<string>();
            if ((saveType == FileType.None || forResave) && this.GameInfo.MapNameIsEmpty(this.Map.BasicSection.Name))
            {
                errors.Add("Map name is empty. If you continue, the filename will be filled in as map name.");
            }
            if (!forSole)
            {
                if (isN64)
                {
                    if (isMegaMap)
                    {
                        // Fatal error. Don't bother asking anything for the warning; it'll go to the save attempt and get the fatal error.
                        return null;
                    }
                    if (!wasN64)
                    {
                        // Always add a warning when saving to Nintendo 64 map format for the first time.
                        errors.Add("You are saving to the Nintendo 64 map format. This type is only meant to be inserted into a Command & Conquer Nintendo 64 ROM, and cannot be handled correctly by the PC game.");
                    }
                    int startErr = errors.Count;
                    string[][] plurArgsCount = new[] { new[] { String.Empty, "s" } };
                    string[][] plurArgsTypes = new[] { new[] { " a", String.Empty }, new[] { String.Empty, "s" }, new[] { "does", "do" } };
                    // Check houses
                    bool usedPlural = false;
                    List<string> unitTypes = UnitTypes.GetTypes(false).Select(h => h.Name).ToList();
                    string[] airTypes = UnitTypes.GetTypes(false).Where(ut => ut.IsAircraft).Select(h => h.Name).ToArray();
                    CheckAllowed(errors, "Found {4} usage{0} of{1} House{2} that {3} not exist in the Nintendo 64 version: {5}.", plurArgsCount, plurArgsTypes, ref usedPlural,
                        Map.GetAllTechnos().Where(t => t.TechnoType.Ownable).Select(t => t.House.Name)
                        .Concat(Map.Buildings.OfType<Building>().Select(b => b.Occupier.House.Name))
                        .Concat(Map.Triggers.Select(t => t.House)).Concat(Map.TeamTypes.Select(t => t.House.Name)),
                        HouseTypes.GetTypes().Select(h => h.Name), false, true, HousesOnN64);
                    // Check units
                    CheckAllowed(errors, "Found {4} unit{0} of{1} type{2} that {3} not exist in the Nintendo 64 version: {5}.", plurArgsCount, plurArgsTypes, ref usedPlural,
                        Map.Technos.OfType<Unit>().Select(i => i.Occupier.Type.Name),
                        unitTypes, true, false, UnitsNotOnN64);
                    // Check units
                    CheckAllowed(errors, "Aircraft cannot be loaded from ini in the Nintendo 64 version. Found {3} aircraft of type{1}: {4}.", null, plurArgsTypes, ref usedPlural,
                        Map.Technos.OfType<Unit>().Select(i => i.Occupier.Type.Name),
                        unitTypes, true, false, airTypes);
                    // Check buildings. Not actually sure if walls as buildings work on C&C64...
                    CheckAllowed(errors, "Found {4} building{0} of{1} type{2} that {3} not exist in the Nintendo 64 version: {5}.", plurArgsCount, plurArgsTypes, ref usedPlural,
                        Map.Buildings.OfType<Building>().Select(i => i.Occupier.Type.Name),
                        BuildingTypes.GetTypes(true).Select(h => h.Name), true, false, BuildingsNotOnN64);
                    // always report as plural if errors on multiple types were found.
                    if (errors.Count > 1)
                    {
                        usedPlural = true;
                    }
                    if (errors.Count > 0)
                    {
                        int last = errors.Count - 1;
                        errors[last] = errors[last] + String.Format("\n{0} will not be removed from the map, but will not work on the Nintendo 64 version.",
                            usedPlural ? "These" : "This");
                    }
                    // Check map
                    List<string> n64MapErrs;
                    using (MemoryStream binStream = new MemoryStream())
                    using (BinaryWriter binWriter = new BinaryWriter(binStream))
                    {
                        n64MapErrs = SaveBinaryClassicN64(binWriter, true);
                    }
                    errors.AddRange(n64MapErrs);
                }
            }
            if (errors.Count > 0)
            {
                return "Warnings:\n* " + String.Join("\n* ", errors.ToArray());
            }
            return null;
        }

        /// <summary>
        /// Checks if certain objects are allowed in the map to save.
        /// </summary>
        /// <param name="errors">List of errors to add to.</param>
        /// <param name="format">
        /// Format string. Arguments inside this will first number through the contents of <paramref name="plurArgsCount"/>, then through those of <paramref name="plurArgsType"/>,
        /// then two more for the amount of found disallowed objects, and the listing of the found disallowed object types.
        /// </param>
        /// <param name="plurArgsCount">Array of string[2]; one for each argument that should be plural if the amount of found disallowed objects is plural. Each string[2] contains the singular and plural form of the argument.</param>
        /// <param name="plurArgsType">Array of string[2]; one for each argument that should be plural if the amount of found disallowed object types is plural. Each string[2] contains the singular and plural form of the argument.</param>
        /// <param name="foundMultiple">Will be set to true if the amount of found objects in this check was greater than 1.</param>
        /// <param name="checkItems">List of string items to check for illegal types.</param>
        /// <param name="allTypes">All types that can appear in the enumerable. This is used for the order. Any items not in this list are also counted as bad, but added in order of appearance.</param>
        /// <param name="uppercaseNames">True if the type names should be reported as upper case.</param>
        /// <param name="allowFiltered">True if the <paramref name="filteredTypes"/> array are the allowed objects, false if it specifies the disallowed ones.</param>
        /// <param name="filteredTypes">Objects to filter on to check what types are allowed.</param>
        private void CheckAllowed(List<string> errors, string format, string[][] plurArgsCount, string[][] plurArgsType, ref bool foundMultiple, IEnumerable<string> checkItems,
            IEnumerable<string> allTypes, bool uppercaseNames, bool allowFiltered, params string[] filteredTypes)
        {
            HashSet<string> filteredItems = new HashSet<string>(filteredTypes, StringComparer.OrdinalIgnoreCase);
            HashSet<string> foundBadItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> allowedTypes = new HashSet<string>(allTypes, StringComparer.OrdinalIgnoreCase);
            List<string> extraBad = new List<string>();
            int badCount = 0;
            foreach (string item in checkItems)
            {
                bool notAllowed = filteredItems.Contains(item);
                if (allowFiltered)
                {
                    notAllowed = !notAllowed;
                }
                if (notAllowed)
                {
                    badCount++;
                    if (!foundBadItems.Contains(item))
                    {
                        foundBadItems.Add(item);
                        if (!allowedTypes.Contains(item))
                        {
                            extraBad.Add(item);
                        }
                    }
                }
            }
            if (badCount > 1)
            {
                foundMultiple = true;
            }
            string[] badItems = allTypes.Where(hname => foundBadItems.Contains(hname)).Concat(extraBad).ToArray();
            if (badItems.Length > 0)
            {
                if (uppercaseNames)
                {
                    badItems = badItems.Select(item => item.ToUpperInvariant()).ToArray();
                }
                bool pluralCount = badCount > 1;
                bool pluralTypes = badItems.Length > 1;
                int plurLenCount = plurArgsCount == null ? 0 : plurArgsCount.Length;
                int plurLenTypes = plurArgsType == null ? 0 : plurArgsType.Length;
                int plurals = plurLenCount + plurLenTypes;
                object[] args = new object[plurals + 2];
                args[plurals] = badCount;
                args[plurals + 1] = String.Join(", ", badItems);
                for (int i = 0; i < plurLenCount; ++i)
                {
                    args[i] = plurArgsCount[i][pluralCount ? 1 : 0];
                }
                for (int i = 0; i < plurLenTypes; ++i)
                {
                    args[plurLenCount + i] = plurArgsType[i][pluralTypes ? 1 : 0];
                }
                errors.Add(String.Format(format, args));
            }
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
                int startPoints = Map.Waypoints.Count(w => w.Cell.HasValue && w.Flags.HasFlag(WaypointFlag.PlayerStart));
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
                        usedWaypoints.Add(Enumerable.Range(0, Map.Waypoints.Length).Where(i => Map.Waypoints[i].Flags.HasFlag(WaypointFlag.Flare)).First());
                }
                WaypointFlag toIgnore = WaypointFlag.Home | WaypointFlag.Reinforce;
                string unusedWaypointsStr = String.Join(", ", setWaypoints.OrderBy(w => w)
                    .Where(w => (Map.Waypoints[w].Flags & toIgnore) == WaypointFlag.None
                                && !usedWaypoints.Contains(w)).Select(w => Map.Waypoints[w].Name).ToArray());
                string unsetUsedWaypointsStr = String.Join(", ", usedWaypoints.OrderBy(w => w).Where(w => !setWaypoints.Contains(w)).Select(w => Map.Waypoints[w].Name).ToArray());
                string evalEmpty(string str)
                {
                    return String.IsNullOrEmpty(str) ? "-" : str;
                };
                info.Add(String.Empty);
                info.Add("Scripting remarks:");
                info.Add(String.Format("Unused team types: {0}", evalEmpty(unusedTeamsStr)));
                info.Add(String.Format("Placed waypoints not used in teams or triggers: {0}", evalEmpty(unusedWaypointsStr)));
                info.Add(String.Format("Empty waypoints used in teams or triggers: {0}", evalEmpty(unsetUsedWaypointsStr)));
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
                !b.Occupier.House.IsSpecial && b.Occupier.Type.IsFactory))
            {
                housesWithCY.Add(bld.House.Name);
            }
            // TODO Add Globals.ExpandTdScripting check to some of this.
            HashSet<string> teamDeployOrders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                TeamMissionTypes.Unload.Mission,
            };
            foreach (TeamType team in Map.TeamTypes)
            {
                bool hasMcv = team.Classes.Select(cl => cl.Type).OfType<UnitType>().Any(b =>
                "mcv".Equals(b.Name, StringComparison.InvariantCultureIgnoreCase));
                if (hasMcv && team.Missions.Any(m => teamDeployOrders.Contains(m.Mission.Mission)))
                {
                    housesWithCY.Add(team.House.Name);
                }
            }
            HashSet<string> deployOrders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                MissionTypes.MISSION_UNLOAD,
                MissionTypes.MISSION_HUNT,
                MissionTypes.MISSION_TIMED_HUNT,
                MissionTypes.MISSION_AMBUSH,
                MissionTypes.MISSION_RESCUE
            };
            foreach ((_, Unit unit) in Map.Technos.OfType<Unit>().Where(b =>
                "mcv".Equals(b.Occupier.Type.Name, StringComparison.InvariantCultureIgnoreCase)
                && deployOrders.Contains(b.Occupier.Mission)))
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
                            foreach ((int ctCell, CellTrigger ctValue) in Map.CellTriggers)
                            {
                                if (trig.Equals(ctValue.Trigger))
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
                for (int i = 0; i < length; ++i)
                {
                    Waypoint waypoint = waypoints[i];
                    if (waypoint != null && waypoint.Cell.HasValue && waypoint.Flags.HasFlag(WaypointFlag.Flare))
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
                if (!Globals.ExpandTdScripting)
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
                bool isAll = trigger.PersistentType == TriggerPersistentType.SemiPersistent;
                bool isEach = trigger.PersistentType == TriggerPersistentType.Persistent;
                bool isDestroyableX = "xxxx".Equals(trigName, StringComparison.OrdinalIgnoreCase);
                bool isDestroyableY = "yyyy".Equals(trigName, StringComparison.OrdinalIgnoreCase);
                bool isDestroyableZ = "zzzz".Equals(trigName, StringComparison.OrdinalIgnoreCase);
                bool isDestroyableU = Globals.ExpandTdScripting && "uuuu".Equals(trigName, StringComparison.OrdinalIgnoreCase);
                bool isDestroyableV = Globals.ExpandTdScripting && "vvvv".Equals(trigName, StringComparison.OrdinalIgnoreCase);
                bool isDestroyableW = Globals.ExpandTdScripting && "wwww".Equals(trigName, StringComparison.OrdinalIgnoreCase);
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
                if (!fatalOnly && event1 == EventTypes.EVENT_DESTROYED && !noOwner)
                {
                    if (isAll)
                    {
                        curErrors.Add(prefix + "A \"Destroyed\" trigger with a House set and repeat status \"When all triggered\" will never work, since a reference to the trigger will be added to the House Triggers list for that House, and since a House can't be \"destroyed\", nothing can ever trigger that instance, making it impossible to clear all objectives required for the trigger to fire.");
                    }
                    else
                    {
                        curErrors.Add(prefix + "A \"Destroyed\" trigger should never have a House set. Setting a House will uselessly add a reference to the trigger to the House Triggers list for that House, and since a House can't be \"destroyed\", nothing can ever trigger that instance.");
                    }
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
                if (!fatalOnly && action1 == ActionTypes.ACTION_WINLOSE && event1 == EventTypes.EVENT_ANY && isAll)
                {
                    curErrors.Add(prefix + "\"Any\" → \"Cap=Win/Des=Lose\" triggers don't function with existence status \"When all triggered\".");
                }
                if (!fatalOnly && action1 == ActionTypes.ACTION_ALLOWWIN && !isPlayer)
                {
                    curErrors.Add(prefix + "Each \"Allow Win\" trigger increases the \"win blockage\" on the House specified in the trigger, which prevents that house from winning until they are all cleared. However, since only the player can be blocked from winning, such triggers only work when they are linked to the player's House.");
                }
                if (!fatalOnly && action1 == ActionTypes.ACTION_ALLOWWIN && isEach && !isDestroyable)
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
                            curErrors.Add(prefix + ""Team types with air units can't be created using \"Create Team\".");
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
                if (Globals.ExpandTdScripting)
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

        public string TriggerSummary(Trigger trigger, bool withLineBreaks, bool includeTriggerName)
        {
            if (trigger == null)
            {
                return null;
            }
            string trigFormat = (includeTriggerName ? "{4}: " : String.Empty)
                + (!withLineBreaks ? "{0}, {1}, {2} → {3}" : "{0}, {1},\n{2} → {3}");
            string evt = trigger.Event1.EventType ?? TriggerEvent.None;
            bool isDataEvent = evt == EventTypes.EVENT_CREDITS
                            || evt == EventTypes.EVENT_TIME
                            || evt == EventTypes.EVENT_NBUILDINGS_DESTROYED
                            || evt == EventTypes.EVENT_NUNITS_DESTROYED
                            || evt == EventTypes.EVENT_BUILD;
            if (isDataEvent)
            {
                string data = trigger.Event1.Data.ToString();
                if (evt == EventTypes.EVENT_BUILD)
                {
                    BuildingType bt = Map.BuildingTypes.FirstOrDefault(b => b.ID == trigger.Event1.Data);
                    if (bt != null)
                        data = bt.Name;
                }
                evt = String.Format(GameInfo.TRIG_ARG_FORMAT, evt, data);
            }
            string act = trigger.Action1.ActionType ?? TriggerAction.None;
            bool isTeamAction = act == ActionTypes.ACTION_CREATE_TEAM
                             || act == ActionTypes.ACTION_DESTROY_TEAM
                             || act == ActionTypes.ACTION_REINFORCEMENTS;
            if (isTeamAction)
            {
                act = String.Format(GameInfo.TRIG_ARG_FORMAT, act, trigger.Action1.Team ?? TeamType.None);
            }
            string persistence = GameInfo.PERSISTENCE_NAMES[(int)trigger.PersistentType];
            return String.Format(trigFormat, trigger.House, persistence, evt, act, trigger.Name);
        }

        public string TriggerEventInfo(List<Trigger> triggers, string eventName)
        {
            if (eventName == null)
            {
                return null;
            }
            List<string> info = new List<string>();
            if (EventTypes.TypesInfo.TryGetValue(eventName, out string trigInfoTd))
            {
                info.Add(trigInfoTd);
            }
            if (EventTypes.TypesDescription.TryGetValue(eventName, out string trigDescrTd))
            {
                info.Add(trigDescrTd);
            }
            return String.Join("\n", info.ToArray());
        }

        public string TriggerActionInfo(List<Trigger> triggers, string actionName)
        {
            if (actionName == null)
            {
                return null;
            }
            List<string> info = new List<string>();
            if (ActionTypes.TypesInfo.TryGetValue(actionName, out string trigInfoTd))
            {
                info.Add(trigInfoTd);
            }
            if (ActionTypes.TypesDescription.TryGetValue(actionName, out string trigDescrTd))
            {
                info.Add(trigDescrTd);
            }
            string delTrig = null;
            bool isFlare = false;
            switch (actionName)
            {
                case ActionTypes.ACTION_DESTROY_XXXX: delTrig = "XXXX"; break;
                case ActionTypes.ACTION_DESTROY_YYYY: delTrig = "YYYY"; break;
                case ActionTypes.ACTION_DESTROY_ZZZZ: delTrig = "ZZZZ"; break;
                case ActionTypes.ACTION_DESTROY_UUUU: delTrig = "UUUU"; break;
                case ActionTypes.ACTION_DESTROY_VVVV: delTrig = "VVVV"; break;
                case ActionTypes.ACTION_DESTROY_WWWW: delTrig = "WWWW"; break;
                case ActionTypes.ACTION_DZ: isFlare = true; break;
            }
            if (delTrig != null)
            {
                Trigger toDestr = triggers.FirstOrDefault(tr => delTrig.Equals(tr.Name, StringComparison.OrdinalIgnoreCase));
                if (toDestr == null)
                {
                    info.Add(delTrig + ": not found");
                }
                else
                {
                    info.Add(TriggerSummary(toDestr, true, true));
                    // Check special case on removing "Allow Win" trigger.
                    string targetAct = toDestr.Action1?.ActionType;
                    if (ActionTypes.ACTION_ALLOWWIN.Equals(targetAct)
                        && ActionTypes.TypesDescription.TryGetValue(targetAct, out string actDescrTd))
                    {
                        info.Add(actDescrTd);
                    }
                }
            }
            else if (isFlare)
            {
                string wp = "Waypoint 25";
                Waypoint z = Map.Waypoints.FirstOrDefault(w => w.Flags.HasFlag(WaypointFlag.Flare));
                if (z == null)
                {
                    // Should never happen.
                    info.Add(wp + " not found!");
                }
                else if (!z.Point.HasValue)
                {
                    info.Add(wp + " is not set.");
                }
                else
                {
                    Point p = z.Point.Value;
                    info.Add(String.Format("{0}: [{1},{2}] (cell {3})", wp, p.X, p.Y, z.Cell.Value));
                }
            }
            return String.Join("\n", info.ToArray());
        }

        public virtual ITeamColor[] GetFlagColors()
        {
            string[] flagColorNames = new string[] {
                "MULTI2",
                "MULTI5",
                "MULTI4",
                "MULTI6",
                "MULTI1",
                "MULTI3",
                "MULTI7",
                "MULTI8",
            };
            ITeamColor[] flagColors = new ITeamColor[flagColorNames.Length];
            for (int i = 0; i < flagColorNames.Length; ++i)
            {
                flagColors[i] = Globals.TheTeamColorManager[flagColorNames[i]];
            }
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

        public bool? IsBuildingCapturable(Building building, out string info)
        {
            info = null;
            if (building.Type.IsWall)
            {
                info = "Walls are technically Overlay types. They can never be captured.";
                return false;
            }
            bool? capturable = building.Type.Capturable;
            BuildingType bt = BuildingTypes.GetTypes(false).FirstOrDefault(b => String.Equals(building.Type.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            List<string> infoList = new List<string>();
            bool capturableClassic = building.Type.Capturable;
            bool capturableRemaster = bt.Capturable;
            bool capturabilitySetClassic = building.Type.Capturable != bt.Capturable;
            bool capturabilitySetRemaster = false;
            if (!Trigger.IsEmpty(building.Trigger) && (!bt.Capturable || !building.Type.Capturable))
            {
                Trigger trig = this.Map.Triggers.FirstOrDefault(t => String.Equals(t.Name, building.Trigger, StringComparison.OrdinalIgnoreCase));
                if (trig != null && trig.Action1.ActionType == ActionTypes.ACTION_WINLOSE)
                {
                    capturable = true;
                    capturableRemaster = true;
                    capturabilitySetRemaster = true;
                    infoList.Add("• This building is made capturable by trigger with\n" +
                                 "   action \"" + ActionTypes.ACTION_WINLOSE + "\" (Remaster only)");
                }
            }
            if (building.Type.Capturable != bt.Capturable)
            {
                // Check if it's due to ini tweaks by checking if base object is capturable.
                infoList.Add(String.Format("• This building type is made {0}capturable due to\n" +
                                           "   rules tweak in the map file (C&C95 v1.06 only)", building.Type.Capturable ? String.Empty : "un"));
            }
            if (infoList.Count > 0)
            {
                info = String.Join("\n", infoList.ToArray());
            }
            if ((capturabilitySetClassic && capturabilitySetRemaster) && capturableClassic != capturableRemaster)
            {
                capturable = null;
            }
            return capturable;
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
            string curr = Map.BasicSection.Player;
            string opposing = HouseTypes.GetClassicOpposingPlayer(curr);
            HouseType basePlayer = Map.HouseNone?.Type
                ?? Map.HouseTypes.Where(t => String.Equals(t.Name, opposing, StringComparison.OrdinalIgnoreCase)).FirstOrDefault()
                ?? Map.HouseTypes.Where(t => !String.Equals(t.Name, curr, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
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
                    Map.ClearEvents();
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
