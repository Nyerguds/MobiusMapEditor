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
using MobiusEditor.RedAlert;
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

        protected const string movieEmpty = "x";
        protected const string remarkOld = " (Classic only)";
        protected const string remarkNew = " (Remaster only)";
        protected const string disabledObjSole = "Owned objects in Sole Survivor are disabled.";
        protected const string settingNoOwnedObjSole = "NoOwnedObjectsInSole";
        protected const string entrySkipped = "{0} {1} entry was skipped.";
        protected const string entriesSkipped = "{0} {1} entries were skipped.";
        protected const string consultManual = "If you don't know why, please consult the manual's explanation of the \"{0}\" setting.";
        protected static readonly string disabledObjExplSole = String.Format(consultManual, settingNoOwnedObjSole);
        protected static readonly string disabledObjSoleOne = disabledObjSole + " " + entrySkipped + " " + disabledObjExplSole;
        protected static readonly string disabledObjSoleMul = disabledObjSole + " " + entriesSkipped + " " + disabledObjExplSole;
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
            if (!Globals.EnableTd106Scripting)
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
            finalMovies.Insert(0, movieEmpty);
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
                UnitTypes.GetTypes(Globals.DisableAirUnits), BuildingTypes.GetTypes(), TeamMissionTypes.GetTypes(),
                fullTechnoTypes, waypoints, movieTypes, movieEmpty, themeEmpty.Yield().Concat(themeTypes), themeEmpty,
                4, 0, 0, Constants.TiberiumValue, 0);
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
                }
                else
                {
                    errors.Add(String.Format("No ." + (fileType == FileType.I64 ? "map" : "bin") + " file found for file '{0}'. Using empty map.", Path.GetFileName(loadPath)));
                    Map.Templates.Clear();
                    fileType = FileType.INI;
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
                    errors.Add(String.Format("'{0}' does not have the correct size for a " + this.GameInfo.Name + " .bin file.", filename));
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
                errors.Add(String.Format("'{0}' does not have the correct size for a N64 " + this.GameInfo.Name + " .map file.", filename));
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
                // N64 big-endian combining.
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
                        errors.Add(String.Format("No mapping found for value {0} in N64 mapping table for {1} Theater.", n64Val.ToString("X4"), mappingName));
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
            if (movieEmpty.Equals(videoName))
                return videoName;
            string newName = GeneralUtils.AddRemarks(videoName, movieEmpty, true, movieTypesRemarksOld, remarkOld, out bool changed);
            if (!changed)
            {
                newName = GeneralUtils.AddRemarks(videoName, movieEmpty, true, movieTypesRemarksNew, remarkNew, false);
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
                string comment = briefSectionUtf8.GetComment("Text");
                string briefing = briefSectionUtf8.Keys["Text"];
                if (comment != null)
                {
                    briefing = briefing + comment;
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
            OverlayType[] fixTypes = Map.OverlayTypes.Where(ov => (ov.Flags & OverlayTypeFlag.RoadSpecial) != OverlayTypeFlag.None && ov.ForceTileNr == 1 && ov.GraphicsSource != ov.Name).ToArray();
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
            HouseType player = this.LoadBasic(ini, basic);
            UpdateBuildingRules(ini, this.Map, false, null);
            this.LoadMapInfo(ini, errors, ref modified);
#if DEBUG
            //MessageBox.Show("Graphics loaded");
#endif
            bool skipSoleStuff = forSole && Globals.NoOwnedObjectsInSole;
            LoadBriefing(ini, errors, ref modified);
            LoadSteamInfo(ini, errors, ref modified);
            List<TeamType> teamTypes = LoadTeamTypes(ini, errors, ref modified);
            Map.TeamTypes.AddRange(teamTypes);
            List<Trigger> triggers = LoadTriggers(ini, errors, ref modified);
            LoadSmudge(ini, errors, ref modified);
            // Sort
            ExplorerComparer comparer = new ExplorerComparer();
            triggers.Sort((x, y) => comparer.Compare(x.Name, y.Name));
            Dictionary<string, string> caseTrigs = Trigger.None.Yield().Concat(triggers.Select(t => t.Name)).ToDictionary(t => t, StringComparer.OrdinalIgnoreCase);
            HashSet<string> checkUnitTrigs = Trigger.None.Yield().Concat(Map.FilterUnitTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            LoadInfantry(ini, skipSoleStuff, caseTrigs, checkUnitTrigs, errors, ref modified);
            LoadUnits(ini, skipSoleStuff, caseTrigs, checkUnitTrigs, errors, ref modified);
            LoadAircraft(ini, skipSoleStuff, errors, ref modified);
            HashSet<string> checkStrcTrigs = Trigger.None.Yield().Concat(Map.FilterStructureTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            LoadStructures(ini, skipSoleStuff, caseTrigs, checkStrcTrigs, errors, ref modified);
            LoadBase(ini, skipSoleStuff, errors, ref modified);
            HashSet<string> checkTerrTrigs = Trigger.None.Yield().Concat(Map.FilterTerrainTriggers(triggers).Select(t => t.Name)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            LoadTerrain(ini, caseTrigs, checkTerrTrigs, errors, ref modified);
            LoadOverlay(ini, errors, ref modified);
            LoadWaypoints(ini, errors, ref modified);
            HashSet<string> checkCellTrigs = Map.FilterCellTriggers(triggers).Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            this.LoadCellTriggers(ini, caseTrigs, checkCellTrigs, errors, ref modified);
            LoadHouses(ini, errors, ref modified);
            UpdateBasePlayerHouse();
            ClearUnusedTriggerArguments(triggers);
            bool wasFixed;
            errors.AddRange(CheckTriggers(triggers, true, true, false, out _, true, out wasFixed));
            if (wasFixed)
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

        private HouseType LoadBasic(INI ini, BasicSection basic)
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

        private void LoadMapInfo(INI ini, List<string> errors, ref bool modified)
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

        private void LoadBriefing(INI ini, List<string> errors, ref bool modified)
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

        private void LoadSteamInfo(INI ini, List<string> errors, ref bool modified)
        {
            // Steam info
            INISection steamSection = ini.Sections.Extract("Steam");
            if (steamSection != null)
            {
                // Ignore any errors in this.
                INI.ParseSection(new MapContext(Map, false), steamSection, Map.SteamSection, true);
            }
        }

        private List<TeamType> LoadTeamTypes(INI ini, List<string> errors, ref bool modified)
        {
            INISection teamTypesSection = ini.Sections.Extract("TeamTypes");
            List<TeamType> teamTypes = new List<TeamType>();
            if (teamTypesSection == null || teamTypesSection.Count == 0)
            {
                return teamTypes;
            }
            // Make case insensitive dictionary of teamtype missions.
            Dictionary<string, TeamMission> teamMissionTypes = Enumerable.ToDictionary(TeamMissionTypes.GetTypes(), t => t.Mission, StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string> kvp in teamTypesSection)
            {
                try
                {
                    if (kvp.Key.Length > 8)
                    {
                        errors.Add(String.Format("TeamType '{0}' has a name that is longer than 8 characters. This will not be corrected by the loading process, but should be addressed, since it can make the teams fail to read correctly, and might even crash the game.", kvp.Key));
                    }
                    TeamType teamType = new TeamType { Name = kvp.Key };
                    string[] tokens = kvp.Value.Split(',');
                    string houseStr = tokens[(int)TeamTypeOptions.House];
                    teamType.House = Map.HouseTypes.Where(t => t.Equals(houseStr)).FirstOrDefault();
                    if (teamType.House == null)
                    {
                        HouseType defHouse = Map.HouseTypes.First();
                        errors.Add(String.Format("Teamtype '{0}' references unknown house '{1}'; clearing to '{2}'.", kvp.Key, houseStr, defHouse.Name));
                        modified = true;
                        teamType.House = defHouse;
                    }
                    teamType.IsRoundAbout = int.Parse(tokens[(int)TeamTypeOptions.IsRoundAbout]) != 0;
                    teamType.IsLearning = int.Parse(tokens[(int)TeamTypeOptions.IsLearning]) != 0;
                    teamType.IsSuicide = int.Parse(tokens[(int)TeamTypeOptions.IsSuicide]) != 0;
                    teamType.IsAutocreate = int.Parse(tokens[(int)TeamTypeOptions.IsAutocreate]) != 0;
                    teamType.IsMercenary = int.Parse(tokens[(int)TeamTypeOptions.IsMercenary]) != 0;
                    teamType.RecruitPriority = int.Parse(tokens[(int)TeamTypeOptions.RecruitPriority]);
                    teamType.MaxAllowed = byte.Parse(tokens[(int)TeamTypeOptions.MaxAllowed]);
                    teamType.InitNum = byte.Parse(tokens[(int)TeamTypeOptions.InitNum]);
                    teamType.Fear = byte.Parse(tokens[(int)TeamTypeOptions.Fear]);
                    int numClasses = int.Parse(tokens[(int)TeamTypeOptions.Classes]);
                    int classesIndex = (int)TeamTypeOptions.Classes + 1;
                    int classesIndexEnd = classesIndex + numClasses;
                    int classesMax = Math.Min(GameInfo.MaxTeamClasses, numClasses);
                    int classesIndexMax = classesIndex + classesMax;
                    for (int i = classesIndex; i < classesIndexMax; ++i)
                    {
                        string[] classTokens = tokens[i].Split(':');
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
                                errors.Add(String.Format("Team '{0}' references unknown class '{1}'; class ignored.", kvp.Key, classTokens[0]));
                                modified = true;
                            }
                        }
                        else
                        {
                            errors.Add(String.Format("Team '{0}' has wrong number of tokens for class index {1} (has {2}, expecting 2).", kvp.Key, i, classTokens.Length));
                            modified = true;
                        }
                    }
                    if (numClasses > GameInfo.MaxTeamClasses)
                    {
                        errors.Add(String.Format("Team '{0}' has more classes than the game can handle (has {1}, maximum is {2}).", kvp.Key, numClasses, GameInfo.MaxTeamClasses));
                        modified = true;
                    }
                    int numMissions = int.Parse(tokens[classesIndexEnd]);
                    int missionsIndex = classesIndexEnd + 1;
                    int missionsIndexEnd = missionsIndex + numMissions;
                    int missionsMax = Math.Min(GameInfo.MaxTeamMissions, numMissions);
                    int missionsIndexMax = missionsIndex + missionsMax;
                    for (int i = missionsIndex; i < missionsIndexMax; ++i)
                    {
                        string[] missionTokens = tokens[i].Split(':');
                        if (missionTokens.Length != 2)
                        {
                            errors.Add(String.Format("Team '{0}' has wrong number of tokens for orders index {1} (has {2}, expecting 2).", kvp.Key, i, missionTokens.Length));
                            modified = true;
                            continue;
                        }
                        // fix mission case sensitivity issues.
                        TeamMission mission;
                        teamMissionTypes.TryGetValue(missionTokens[0], out mission);
                        if (mission == null)
                        {
                            errors.Add(String.Format("Team '{0}' references unknown orders '{1}'; orders ignored.", kvp.Key, missionTokens[0]));
                            modified = true;
                            continue;
                        }
                        string argError = null;
                        string argStr = missionTokens[1];
                        if (!Int32.TryParse(argStr, out int arg))
                        {
                            argError = String.Format("Team '{0}', orders index {1} ('{2}') has a non-numeric value '{3}'; reverting to 0.", kvp.Key, i, mission.Mission, argStr);
                        }
                        else if (mission.ArgType == TeamMissionArgType.Time && arg < 0)
                        {
                            argError = String.Format("Team '{0}', orders index {1} ('{2}') has a bad value '{3}' for a Time argument; reverting to 0.", kvp.Key, i, mission.Mission, argStr);
                        }
                        else if (mission.ArgType == TeamMissionArgType.Waypoint && (arg < -1 || arg > Map.Waypoints.Length))
                        {
                            argError = String.Format("Team '{0}', orders index {1} ('{2}') has a bad value '{3}' for a Waypoint argument: reverting to 0.", kvp.Key, i, mission.Mission, argStr);
                        }
                        else if (mission.ArgType == TeamMissionArgType.OptionsList && (arg < 0 || arg > mission.DropdownOptions.Max(vl => vl.Value))) // Not actually used in TD.
                        {
                            argError = String.Format("Team '{0}', orders index {1} ('{2}') has a bad value '{3}' for the available options; reverting to 0.", kvp.Key, i, mission.Mission, argStr);
                        }
                        else if (mission.ArgType == TeamMissionArgType.MapCell && (arg < 0 || arg >= Map.Metrics.Length))
                        {
                            argError = String.Format("Team '{0}', orders index {1} ('{2}') has a bad value '{3}' for a Cell argument; reverting to 0.", kvp.Key, i, mission.Mission, argStr);
                        }
                        else if (mission.ArgType == TeamMissionArgType.MissionNumber && (arg < 0 || arg > missionsMax))
                        {
                            argError = String.Format("Team '{0}', orders index {1} ('{2}') has a bad value '{3}' for an orders index argument; reverting to 0.", kvp.Key, i, mission.Mission, argStr);
                        }
                        else if (mission.ArgType == TeamMissionArgType.Tarcom && arg < 0)
                        {
                            argError = String.Format("Team '{0}', orders index {1} ('{2}') has a bad value '{3}' for a Tarcom argument; reverting to 0.", kvp.Key, i, mission.Mission, argStr);
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
                        errors.Add(String.Format("Team '{0}' has more orders than the game can handle (has {1}, maximum is {2}).", kvp.Key, numMissions, GameInfo.MaxTeamMissions));
                        modified = true;
                    }
                    int reinforceIndex = missionsIndexEnd;
                    if (tokens.Length > reinforceIndex)
                    {
                        teamType.IsReinforcable = int.Parse(tokens[reinforceIndex]) != 0;
                    }
                    int prebuiltIndex = missionsIndexEnd + 1;
                    if (tokens.Length > prebuiltIndex)
                    {
                        teamType.IsPrebuilt = int.Parse(tokens[prebuiltIndex]) != 0;
                    }
                    teamTypes.Add(teamType);
                }
                catch (Exception ex)
                {
                    errors.Add(String.Format("Teamtype '{0}' has errors and can't be parsed: {1}.", kvp.Key, ex.Message));
                    modified = true;
                }
            }
            return teamTypes;
        }

        private List<Trigger> LoadTriggers(INI ini, List<string> errors, ref bool modified)
        {
            INISection triggersSection = ini.Sections.Extract("Triggers");
            List<Trigger> triggers = new List<Trigger>();
            if (triggersSection == null)
            {
                return triggers;
            }
            foreach (KeyValuePair<string, string> kvp in triggersSection)
            {
                try
                {
                    if (kvp.Key.Length > 4)
                    {
                        errors.Add(String.Format("Trigger '{0}' has a name that is longer than 4 characters. This will not be corrected by the loading process, but should be addressed, since it can make the triggers fail to link correctly to objects and cell triggers, and might even crash the game.", kvp.Key));
                    }
                    string[] tokens = kvp.Value.Split(',');
                    if (tokens.Length < 5)
                    {
                        errors.Add(String.Format("Trigger '{0}' has too few tokens (expecting at least 5).", kvp.Key));
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
                            errors.Add(String.Format("Trigger '{0}' references unknown event '{1}'; reverting to 'None'.", kvp.Key, tokens[0]));
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
                            errors.Add(String.Format("Trigger '{0}' references unknown action '{1}'; reverting to 'None'.", kvp.Key, tokens[4]));
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
                            errors.Add(String.Format("Trigger '{0}' references unknown House '{1}'; clearing to 'None'.", kvp.Key, tokens[4]));
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
                            errors.Add(String.Format("Trigger '{0}' references unknown teamtype '{1}'; reverting to 'None'.", kvp.Key, tokens[4]));
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
                catch (Exception ex)
                {
                    errors.Add(String.Format("Trigger '{0}' has errors and can't be parsed: {1}.", kvp.Key, ex.Message));
                    modified = true;
                }
            }
            return triggers;
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
                if (!Int32.TryParse(kvp.Key, out cell))
                {
                    errors.Add(String.Format("Cell for Smudge cannot be parsed. Key: '{0}', value: '{1}'; skipping.", kvp.Key, kvp.Value));
                    modified = true;
                    continue;
                }
                string[] tokens = kvp.Value.Split(',');
                if (tokens.Length != 3)
                {
                    if (tokens.Length > 0 && tokens[0].Length > 0)
                    {
                        errors.Add(String.Format("Smudge '{0}' on cell {1} has wrong number of tokens (has {2}, expecting 3); skipping.", tokens[0], kvp.Key, tokens.Length));
                    }
                    else
                    {
                        errors.Add(String.Format("Smudge on cell {0} has wrong number of tokens (has {1}, expecting 3); skipping.", kvp.Key, tokens.Length));
                    }
                    modified = true;
                    continue;
                }
                // Craters other than cr1 don't work right in the game. Replace them by stage-0 cr1.
                bool badCrater = Globals.ConvertCraters && SmudgeTypes.BadCraters.IsMatch(tokens[0]);
                SmudgeType smudgeType = badCrater ? SmudgeTypes.Crater1 : Map.SmudgeTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault();
                if (smudgeType == null)
                {
                    errors.Add(String.Format("Smudge '{0}' on cell {1} references unknown smudge.", tokens[0], cell));
                    modified = true;
                    continue;
                }
                if (Globals.FilterTheaterObjects && !smudgeType.ExistsInTheater)
                {
                    errors.Add(String.Format("Smudge '{0}' on cell {1} is not available in the set theater; skipping.", smudgeType.Name, cell));
                    modified = true;
                    continue;
                }
                if (badCrater)
                {
                    errors.Add(String.Format("Smudge '{0}' (cell {1}) does not function correctly in maps. Correcting to '{2}'.", tokens[0], cell, smudgeType.Name));
                    modified = true;
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

        private void LoadInfantry(INI ini, bool skipSoleStuff, Dictionary<string, string> caseTrigs, HashSet<string> checkUnitTrigs, List<string> errors, ref bool modified)
        {
            INISection infantrySection = ini.Sections.Extract("Infantry");
            int amount = infantrySection?.Count ?? 0;
            if (amount == 0)
            {
                return;
            }
            if (skipSoleStuff)
            {
                errors.Add(String.Format(amount == 1 ? disabledObjSoleOne : disabledObjSoleMul, amount, "[Infantry]"));
                modified = true;
                return;
            }
            foreach (KeyValuePair<string, string> kvp in infantrySection)
            {
                string[] tokens = kvp.Value.Split(',');
                if (tokens.Length != 8)
                {
                    if (tokens.Length < 2)
                    {
                        errors.Add(String.Format("Infantry entry '{0}' has wrong number of tokens (has {1}, expecting 8).", kvp.Key, tokens.Length));
                    }
                    else
                    {
                        errors.Add(String.Format("Infantry entry '{0}', infantry '{1}', has wrong number of tokens (has {2}, expecting 8).", kvp.Key, tokens[1], tokens.Length));
                    }
                    modified = true;
                    continue;
                }
                InfantryType infantryType = Map.InfantryTypes.Where(t => t.Equals(tokens[1])).FirstOrDefault();
                if (infantryType == null)
                {
                    errors.Add(String.Format("Infantry '{0}' references unknown infantry; skipping.", tokens[1]));
                    modified = true;
                    continue;
                }
                int strength;
                if (!Int32.TryParse(tokens[2], out strength))
                {
                    errors.Add(String.Format("Strength for infantry '{0}', value '{1}', cannot be parsed; skipping.", infantryType.Name, tokens[2]));
                    modified = true;
                    continue;
                }
                int cell;
                if (!Int32.TryParse(tokens[3], out cell))
                {
                    errors.Add(String.Format("Cell for infantry '{0}', value '{1}', cannot be parsed; skipping.", infantryType.Name, tokens[3]));
                    modified = true;
                    continue;
                }
                ICellOccupier techno = Map.Technos[cell];
                InfantryGroup infantryGroup = techno as InfantryGroup;
                if (infantryGroup == null && techno == null)
                {
                    infantryGroup = new InfantryGroup();
                    Map.Technos.Add(cell, infantryGroup);
                }
                if (infantryGroup == null)
                {
                    if (techno is Building building)
                    {
                        errors.Add(String.Format("Infantry '{0}' overlaps structure '{1}' in cell {2}; skipping.", infantryType.Name, building.Type.Name, cell));
                    }
                    else if (techno is Overlay overlay)
                    {
                        errors.Add(String.Format("Infantry '{0}' overlaps overlay '{1}' in cell {2}; skipping.", infantryType.Name, overlay.Type.Name, cell));
                    }
                    else if (techno is Terrain terrain)
                    {
                        errors.Add(String.Format("Infantry '{0}' overlaps terrain '{1}' in cell {2}; skipping.", infantryType.Name, terrain.Type.Name, cell));
                    }
                    else if (techno is Unit unit)
                    {
                        errors.Add(String.Format("Infantry '{0}' overlaps unit '{1}' in cell {2}; skipping.", infantryType.Name, unit.Type.Name, cell));
                    }
                    else
                    {
                        errors.Add(String.Format("Infantry '{0}' overlaps unknown techno in cell {1}; skipping.", infantryType.Name, cell));
                    }
                    modified = true;
                    continue;
                }
                int stoppingPos;
                if (!Int32.TryParse(tokens[4], out stoppingPos))
                {
                    errors.Add(String.Format("Sub-position for infantry '{0}' on cell {1}, value '{2}', cannot be parsed; skipping.",
                        infantryType.Name, cell, tokens[4]));
                    modified = true;
                    continue;
                }
                if (stoppingPos < 0 || stoppingPos >= Globals.NumInfantryStops)
                {
                    errors.Add(String.Format("Sub-position for infantry '{0}' on cell {1} has illegal value {2}; skipping.",
                        infantryType.Name, cell, stoppingPos));
                    modified = true;
                    break;
                }
                if (strength < 1 || strength > 256)
                {
                    int newStrength = strength.Restrict(1, 256);
                    errors.Add(String.Format("Strength for infantry '{0}' on cell {1}, sub-position {2} has illegal value {3}; corrected to {4}.",
                        infantryType.Name, cell, stoppingPos, strength, newStrength));
                    strength = newStrength;
                    modified = true;
                }
                int dirValue;
                if (!Int32.TryParse(tokens[6], out dirValue))
                {
                    errors.Add(String.Format("Direction for infantry '{0}' on cell {1}, sub-position {2}, value '{3}', cannot be parsed; clearing to 0.",
                        infantryType.Name, cell, stoppingPos, tokens[6]));
                    modified = true;
                    dirValue = 0;
                }
                DirectionType dirType = DirectionType.FindClosestDirectionType(dirValue, Map.UnitDirectionTypes);
                if (dirType.ID != dirValue)
                {
                    errors.Add(String.Format("Direction for infantry '{0}' on cell {1}, sub-position {2}, value {3}, cannot be matched to a known value. Taking closest match value {4} ({5}).",
                        infantryType.Name, cell, stoppingPos, dirValue, dirType.ID, dirType.Name));
                    modified = true;
                }
                if (infantryGroup.Infantry[stoppingPos] != null)
                {
                    errors.Add(String.Format("Infantry '{0}' overlaps another infantry at sub-position {1} in cell {2}; skipping.",
                        infantryType.Name, stoppingPos, cell));
                    modified = true;
                    continue;
                }
                if (!caseTrigs.ContainsKey(tokens[7]))
                {
                    errors.Add(String.Format("Infantry '{0}' on cell {1}, sub-position {2} links to unknown trigger '{3}'; clearing trigger.",
                        infantryType.Name, cell, stoppingPos, tokens[7]));
                    modified = true;
                    tokens[7] = Trigger.None;
                }
                else if (!checkUnitTrigs.Contains(tokens[7]))
                {
                    errors.Add(String.Format("Infantry '{0}' on cell {1}, sub-position {2} links to trigger '{3}' which does not contain an event applicable to infantry; clearing trigger.",
                        infantryType.Name, cell, stoppingPos, tokens[7]));
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
                    errors.Add(String.Format("Infantry '{0}' on cell {1}, sub-position {2} references unknown house '{3}'; clearing to '{4}'.",
                        inf.Type.Name, cell, stoppingPos, tokens[0], defHouse.Name));
                    modified = true;
                    inf.House = defHouse;
                }
                if (inf.Mission == null)
                {
                    string badOrder = Map.MissionTypesBad.Where(t => t.Equals(tokens[5], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    string deforders = Map.GetDefaultMission(infantryType);
                    string reason = badOrder != null ? "unsupported" : "unknown";
                    errors.Add(String.Format("Infantry '{0}' on cell {1}, sub-position {2} references {3} orders '{4}'; changing to '{5}'.",
                        inf.Type.Name, cell, stoppingPos, reason, badOrder ?? tokens[5], deforders));
                    inf.Mission = deforders;
                }
            }
        }

        private void LoadUnits(INI ini, bool skipSoleStuff, Dictionary<string, string> caseTrigs, HashSet<string> checkUnitTrigs, List<string> errors, ref bool modified)
        {
            INISection unitsSection = ini.Sections.Extract("Units");
            int amount = unitsSection?.Count ?? 0;
            if (amount == 0)
            {
                return;
            }
            if (skipSoleStuff)
            {
                errors.Add(String.Format(amount == 1 ? disabledObjSoleOne : disabledObjSoleMul, amount, "[Units]"));
                modified = true;
                return;
            }
            foreach (KeyValuePair<string, string> kvp in unitsSection)
            {
                string[] tokens = kvp.Value.Split(',');
                if (tokens.Length != 7)
                {
                    if (tokens.Length < 2)
                    {
                        errors.Add(String.Format("Unit entry '{0}' has wrong number of tokens (has {1}, expecting 7).", kvp.Key, tokens.Length));
                    }
                    else
                    {
                        errors.Add(String.Format("Unit entry '{0}', unit '{1}', has wrong number of tokens (has {2}, expecting 7).", kvp.Key, tokens[1], tokens.Length));
                    }
                    modified = true;
                    continue;
                }
                UnitType unitType = Map.UnitTypes.Where(t => t.IsGroundUnit && t.Equals(tokens[1])).FirstOrDefault();
                if (unitType == null)
                {
                    errors.Add(String.Format("Unit '{0}' references unknown unit.", tokens[1]));
                    modified = true;
                    continue;
                }
                int strength;
                if (!Int32.TryParse(tokens[2], out strength))
                {
                    errors.Add(String.Format("Strength for unit '{0}', value '{1}', cannot be parsed; skipping.", unitType.Name, tokens[2]));
                    modified = true;
                    continue;
                }
                int cell;
                if (!Int32.TryParse(tokens[3], out cell))
                {
                    errors.Add(String.Format("Cell for unit '{0}', value '{1}', cannot be parsed; skipping.", unitType.Name, tokens[3]));
                    modified = true;
                    continue;
                }
                if (strength < 1 || strength > 256)
                {
                    int newStrength = strength.Restrict(1, 256);
                    errors.Add(String.Format("Strength for unit '{0}' on cell {1} has illegal value {2}; corrected to {3}.", unitType.Name, cell, strength, newStrength));
                    strength = newStrength;
                    modified = true;
                }
                int dirValue;
                if (!Int32.TryParse(tokens[4], out dirValue))
                {
                    errors.Add(String.Format("Direction for unit '{0}' on cell {1}, value '{2}', cannot be parsed; clearing to 0.", unitType.Name, cell, tokens[4]));
                    modified = true;
                    dirValue = 0;
                }
                DirectionType dirType = DirectionType.FindClosestDirectionType(dirValue, Map.UnitDirectionTypes);
                if (dirType.ID != dirValue)
                {
                    errors.Add(String.Format("Direction for unit '{0}' on cell {1}, value {2}, cannot be matched to a known value. Taking closest match value {3} ({4}).",
                        unitType.Name, cell, dirValue, dirType.ID, dirType.Name));
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
                    errors.Add(String.Format("Unit '{0}' on cell {1} references unknown house '{2}'; clearing to '{3}'.",
                        newUnit.Type.Name, cell, tokens[0], defHouse.Name));
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
                        // "Rescue" equates to "Hunt" in the code, bot clutters the list, so let's correct it to "Hunt".
                        deforders = MissionTypes.MISSION_HUNT;
                    }
                    string reason = badOrder != null ? "unsupported" : "unknown";
                    errors.Add(String.Format("Unit '{0}' on cell {1} references {2} orders '{3}'; changing to '{4}'.",
                        newUnit.Type.Name, cell, reason, badOrder ?? tokens[5], deforders));
                    newUnit.Mission = deforders;
                }
                if (!Map.Technos.Add(cell, newUnit))
                {
                    ICellOccupier techno = Map.Technos[cell];
                    if (techno is Building building)
                    {
                        errors.Add(String.Format("Unit '{0}' overlaps structure '{1}' in cell {2}; skipping.", unitType.Name, building.Type.Name, cell));
                    }
                    else if (techno is Overlay overlay)
                    {
                        errors.Add(String.Format("Unit '{0}' overlaps overlay '{1}' in cell {2}; skipping.", unitType.Name, overlay.Type.Name, cell));
                    }
                    else if (techno is Terrain terrain)
                    {
                        errors.Add(String.Format("Unit '{0}' overlaps terrain '{1}' in cell {2}; skipping.", unitType.Name, terrain.Type.Name, cell));
                    }
                    else if (techno is InfantryGroup infantry)
                    {
                        errors.Add(String.Format("Unit '{0}' overlaps infantry in cell {1}; skipping.", unitType.Name, cell));
                    }
                    else if (techno is Unit unit)
                    {
                        errors.Add(String.Format("Unit '{0}' overlaps unit '{1}' in cell {2}; skipping.", unitType.Name, unit.Type.Name, cell));
                    }
                    else
                    {
                        errors.Add(String.Format("Unit '{0}' overlaps unknown techno in cell {1}; skipping.", unitType.Name, cell));
                    }
                    modified = true;
                    continue;
                }
                if (!caseTrigs.ContainsKey(tokens[6]))
                {
                    errors.Add(String.Format("Unit '{0}' on cell {1} links to unknown trigger '{2}'; clearing trigger.",
                        unitType.Name, cell, tokens[6]));
                    modified = true;
                    newUnit.Trigger = Trigger.None;
                }
                else if (!checkUnitTrigs.Contains(tokens[6]))
                {
                    errors.Add(String.Format("Unit '{0}' on cell {1} links to trigger '{2}' which does not contain an event applicable to units; clearing trigger.",
                        unitType.Name, cell, tokens[6]));
                    modified = true;
                    newUnit.Trigger = Trigger.None;
                }
                else
                {
                    // Adapt to same case
                    newUnit.Trigger = caseTrigs[tokens[6]];
                }
            }
        }

        private void LoadAircraft(INI ini, bool skipSoleStuff, List<string> errors, ref bool modified)
        {
            // Classic game does not support this, so I'm leaving this out by default.
            // It is always extracted, so it doesn't end up with the "extra sections"
            INISection aircraftSection = ini.Sections.Extract("Aircraft");
            int amount = aircraftSection?.Count ?? 0;
            if (amount == 0)
            {
                return;
            }
            if (Globals.DisableAirUnits || skipSoleStuff)
            {
                bool isOne = amount == 1;
                string disabledObj = skipSoleStuff ? "Owned objects in Sole Survivor" : "Aircraft";
                string disabledObjExpl = String.Format(consultManual, skipSoleStuff ? settingNoOwnedObjSole : "DisableAirUnits");
                errors.Add(String.Format("{0} are disabled. {0} {2}",
                    String.Format(isOne ? entrySkipped : entriesSkipped, amount, "[Aircraft]"), disabledObjExpl));
                modified = true;
                return;
            }
            foreach (KeyValuePair<string, string> kvp in aircraftSection)
            {
                string[] tokens = kvp.Value.Split(',');
                if (tokens.Length != 6)
                {
                    if (tokens.Length < 2)
                    {
                        errors.Add(String.Format("Aircraft entry '{0}' has wrong number of tokens (has {1}, expecting 6).", kvp.Key, tokens.Length));
                    }
                    else
                    {
                        errors.Add(String.Format("Aircraft entry '{0}', aircraft '{1}', has wrong number of tokens (has {2}, expecting 6).", kvp.Key, tokens[1], tokens.Length));
                    }
                    modified = true;
                    continue;
                }
                UnitType aircraftType = Map.UnitTypes.Where(t => t.IsAircraft && t.Equals(tokens[1])).FirstOrDefault();
                if (aircraftType == null)
                {
                    errors.Add(String.Format("Aircraft '{0}' references unknown aircraft.", tokens[1]));
                    modified = true;
                    continue;
                }
                int strength;
                if (!Int32.TryParse(tokens[2], out strength))
                {
                    errors.Add(String.Format("Strength for aircraft '{0}', value '{1}', cannot be parsed; skipping.", aircraftType.Name, tokens[2]));
                    modified = true;
                    continue;
                }
                int cell;
                if (!Int32.TryParse(tokens[3], out cell))
                {
                    errors.Add(String.Format("Cell for aircraft '{0}', value '{1}', cannot be parsed; skipping.", aircraftType.Name, tokens[3]));
                    modified = true;
                    continue;
                }
                if (strength < 1 || strength > 256)
                {
                    int newStrength = strength.Restrict(1, 256);
                    errors.Add(String.Format("Strength for aircraft '{0}' on cell {1} has illegal value {2}; corrected to {3}.",
                        aircraftType.Name, cell, strength, newStrength));
                    strength = newStrength;
                    modified = true;
                }
                int dirValue;
                if (!Int32.TryParse(tokens[4], out dirValue))
                {
                    errors.Add(String.Format("Direction for aircraft '{0}' on cell {1}, value '{2}', cannot be parsed; clearing to 0.",
                        aircraftType.Name, cell, tokens[4]));
                    modified = true;
                    dirValue = 0;
                }
                DirectionType dirType = DirectionType.FindClosestDirectionType(dirValue, Map.UnitDirectionTypes);
                if (dirType.ID != dirValue)
                {
                    errors.Add(String.Format("Direction for aircraft '{0}' on cell {1}, value {2}, cannot be matched to a known value. Taking closest match value {3} ({4}).",
                        aircraftType.Name, cell, dirValue, dirType.ID, dirType.Name));
                    modified = true;
                }
                Unit newAir = new Unit()
                {
                    Type = aircraftType,
                    House = Map.HouseTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault(),
                    Strength = strength,
                    Direction = dirType,
                    Mission = Map.MissionTypes.Where(t => t.Equals(tokens[5])).FirstOrDefault() ?? Map.GetDefaultMission(aircraftType)
                };
                if (newAir.House == null)
                {
                    HouseType defHouse = Map.HouseTypes.First();
                    errors.Add(String.Format("Aircraft '{0}' on cell {1} references unknown house '{2}'; clearing to '{3}'.",
                        newAir.Type.Name, cell, tokens[0], defHouse.Name));
                    modified = true;
                    newAir.House = defHouse;
                }
                if (newAir.Mission == null)
                {
                    string badOrder = Map.MissionTypesBad.Where(t => t.Equals(tokens[5], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    string deforders = Map.GetDefaultMission(aircraftType);
                    string reason = badOrder != null ? "unsupported" : "unknown";
                    errors.Add(String.Format("Aircraft '{0}' on cell {1} references {2} orders '{3}'; changing to '{4}'.",
                        newAir.Type.Name, cell, reason, badOrder ?? tokens[5], deforders));
                    newAir.Mission = deforders;
                }
                if (!Map.Technos.Add(cell, newAir))
                {
                    ICellOccupier techno = Map.Technos[cell];
                    if (techno is Building building)
                    {
                        errors.Add(String.Format("Aircraft '{0}' overlaps structure '{1}' in cell {2}; skipping.", aircraftType.Name, building.Type.Name, cell));
                    }
                    else if (techno is Overlay overlay)
                    {
                        errors.Add(String.Format("Aircraft '{0}' overlaps overlay '{1}' in cell {2}; skipping.", aircraftType.Name, overlay.Type.Name, cell));
                    }
                    else if (techno is Terrain terrain)
                    {
                        errors.Add(String.Format("Aircraft '{0}' overlaps terrain '{1}' in cell {2}; skipping.", aircraftType.Name, terrain.Type.Name, cell));
                    }
                    else if (techno is InfantryGroup infantry)
                    {
                        errors.Add(String.Format("Aircraft '{0}' overlaps infantry in cell {1}; skipping.", aircraftType.Name, cell));
                    }
                    else if (techno is Unit unit)
                    {
                        errors.Add(String.Format("Aircraft '{0}' overlaps unit '{1}' in cell {2}; skipping.", aircraftType.Name, unit.Type.Name, cell));
                    }
                    else
                    {
                        errors.Add(String.Format("Aircraft '{0}' overlaps unknown techno in cell {1}; skipping.", aircraftType.Name, cell));
                    }
                    modified = true;
                }
            }
        }

        private void LoadStructures(INI ini, bool skipSoleStuff, Dictionary<string, string> caseTrigs, HashSet<string> checkStrcTrigs, List<string> errors, ref bool modified)
        {
            INISection structuresSection = ini.Sections.Extract("Structures");
            int amount = structuresSection?.Count ?? 0;
            if (amount == 0)
            {
                return;
            }
            if (skipSoleStuff)
            {
                errors.Add(String.Format(amount == 1 ? disabledObjSoleOne : disabledObjSoleMul, amount, "[Structures]"));
                modified = true;
                return;
            }
            foreach (KeyValuePair<string, string> kvp in structuresSection)
            {
                string[] tokens = kvp.Value.Split(',');
                if (tokens.Length != 6)
                {
                    if (tokens.Length < 2)
                    {
                        errors.Add(String.Format("Structure entry '{0}' has wrong number of tokens (has {1}, expecting 6).", kvp.Key, tokens.Length));
                        modified = true;
                    }
                    else
                    {
                        errors.Add(String.Format("Structure entry {0}, structure '{1}', has wrong number of tokens (has {2}, expecting 6).", kvp.Key, tokens[1], tokens.Length));
                        modified = true;
                    }
                    continue;
                }
                BuildingType buildingType = Map.BuildingTypes.Where(t => t.Equals(tokens[1])).FirstOrDefault();
                if (buildingType == null)
                {
                    errors.Add(String.Format("Structure '{0}' references unknown structure.", tokens[1]));
                    modified = true;
                    continue;
                }
                if (Globals.FilterTheaterObjects && buildingType.IsTheaterDependent && !buildingType.ExistsInTheater)
                {
                    errors.Add(String.Format("Structure '{0}' is not available in the set theater; skipping.", buildingType.Name));
                    modified = true;
                    continue;
                }
                int strength;
                if (!Int32.TryParse(tokens[2], out strength))
                {
                    errors.Add(String.Format("Strength for structure '{0}', value '{1}', cannot be parsed; skipping.", buildingType.Name, tokens[2]));
                    modified = true;
                    continue;
                }
                int cell;
                if (!Int32.TryParse(tokens[3], out cell))
                {
                    errors.Add(String.Format("Cell for structure '{0}', value '{1}', cannot be parsed; skipping.",
                        buildingType.Name, tokens[3]));
                    modified = true;
                    continue;
                }
                if (strength < 1 || strength > 256)
                {
                    int newStrength = strength.Restrict(1, 256);
                    errors.Add(String.Format("Strength for structure '{0}' on cell {1} has illegal value {2}; corrected to {3}.",
                        buildingType.Name, cell, strength, newStrength));
                    strength = newStrength;
                    modified = true;
                }
                // Do this here, before House or Trigger, since those get ignored if it's a wall type.
                if (buildingType.IsWall && !Globals.AllowWallBuildings)
                {
                    OverlayType wall = Map.OverlayTypes.Where(t => t.Equals(kvp.Value)).FirstOrDefault();
                    if (wall != null)
                    {
                        errors.Add(String.Format("Structure '{0}' on cell {1} is a wall type. It will be treated as wall, not as building.",
                            buildingType.Name, cell));
                        Map.Overlay[cell] = new Overlay() { Type = wall, Icon = 0 };
                        modified = true;
                        continue;
                    }
                }
                int dirValue;
                if (!Int32.TryParse(tokens[4], out dirValue))
                {
                    errors.Add(String.Format("Direction for structure '{0}' on cell {1}, value '{2}', cannot be parsed; clearing to 0.",
                        buildingType.Name, cell, tokens[4]));
                    modified = true;
                    dirValue = 0;
                }
                DirectionType dirType = DirectionType.FindClosestDirectionType(dirValue, Map.BuildingDirectionTypes);
                if (dirType.ID != dirValue)
                {
                    errors.Add(String.Format("Direction for structure '{0}' on cell {1}, value {2}, cannot be matched to a known value. Taking closest match value {3} ({4}).",
                        buildingType.Name, cell, dirValue, dirType.ID, dirType.Name));
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
                    errors.Add(String.Format("Structure '{0}' on cell {1} references unknown house '{2}'; clearing to '{3}'.",
                        buildingType.Name, cell, tokens[0], defHouse.Name));
                    modified = true;
                    newBld.House = defHouse;
                }
                if (!Map.Buildings.CanAdd(cell, newBld)) // || !Map.Technos.CanAdd(cell, newBld, newBld.Type.BaseOccupyMask))
                {
                    Map.CheckBuildingBlockingCell(cell, buildingType, errors, ref modified);
                    continue;
                }
                Map.Buildings.Add(cell, newBld);
                if (!caseTrigs.ContainsKey(tokens[5]))
                {
                    errors.Add(String.Format("Structure '{0}' on cell {1} links to unknown trigger '{2}'; clearing trigger.",
                        buildingType.Name, cell, tokens[5]));
                    modified = true;
                    newBld.Trigger = Trigger.None;
                }
                else if (!checkStrcTrigs.Contains(tokens[5]))
                {
                    errors.Add(String.Format("Structure '{0}' on cell {1} links to trigger '{2}' which does not contain an event applicable to structures; clearing trigger.",
                        buildingType.Name, cell, tokens[5]));
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

        private void LoadBase(INI ini, bool skipSoleStuff, List<string> errors, ref bool modified)
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
            string baseCountStr = baseSection.TryGetValue("Count");
            baseSection.Remove("Count");
            if (!Int32.TryParse(baseCountStr, out int baseCount))
            {
                if (skipSoleStuff)
                {
                    // Ignore error. Just indicate it's skipped.
                    errors.Add("Owned objects in Sole Survivor are disabled. [Base] section is skipped. " + disabledObjExplSole);
                }
                else
                {
                    errors.Add(String.Format("Base count '{0}' is not a valid integer.", baseCountStr));
                }
                modified = true;
                CleanBaseSection(ini, baseSection);
                return;
            }
            if (skipSoleStuff && baseCount > 0)
            {
                errors.Add(String.Format(baseCount == 1 ? disabledObjSoleOne : disabledObjSoleMul, baseCount, "[Structures]"));
                modified = true;
                CleanBaseSection(ini, baseSection);
                return;
            }
            int curPriorityVal = 0;
            for (int i = 0; i < baseCount; ++i)
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
                    if (tokens.Length > 0 && tokens[0].Length > 0)
                    {
                        errors.Add(String.Format("Base rebuild entry {0}, structure {1}, has wrong number of tokens (has {2}, expecting 2).", key, tokens[0], tokens.Length));
                    }
                    else
                    {
                        errors.Add(String.Format("Base rebuild entry {0} has wrong number of tokens (has {1}, expecting 2).", key, tokens.Length));
                    }
                    modified = true;
                    continue;
                }
                BuildingType buildingType = Map.BuildingTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault();
                if (buildingType == null)
                {
                    errors.Add(String.Format("Base rebuild entry {0} references unknown structure '{1}'.", key, tokens[0]));
                    modified = true;
                    continue;
                }
                if (Globals.FilterTheaterObjects && buildingType.IsTheaterDependent && !buildingType.ExistsInTheater)
                {
                    errors.Add(String.Format("Base rebuild entry {0} references structure '{1}' which is not available in the set theater; skipping.", key, buildingType.Name));
                    modified = true;
                    continue;
                }
                int coord;
                if (!Int32.TryParse(tokens[1], out coord))
                {
                    errors.Add(String.Format("Coordinates for base rebuild entry '{0}', structure '{1}', value '{2}', cannot be parsed; skipping.", key, buildingType.Name, tokens[1]));
                    modified = true;
                    continue;
                }
                Point location = new Point((coord >> 8) & 0x7F, (coord >> 24) & 0x7F);
                bool canPlace = Map.Metrics.GetCell(location, out int cell);
                if (Map.Buildings.OfType<Building>().Where(x => x.Location == location && x.Occupier.Type.ID == buildingType.ID).FirstOrDefault().Occupier is Building building)
                {
                    if (building.BasePriority == -1)
                    {
                        building.BasePriority = curPriorityVal++;
                    }
                    else
                    {
                        errors.Add(String.Format("Base rebuild entry '{0}' is a duplicate entry for structure '{0}' on cell '{1}'; skipping.", buildingType.Name, cell));
                    }
                    continue;
                }
                if (!canPlace)
                {
                    errors.Add(String.Format("Base rebuild entry '{0}', structure '{1}' cannot be placed at cell '{2}'; skipping.", key, buildingType.Name, cell));
                }
                Building toRebuild = new Building()
                {
                    Type = buildingType,
                    House = HouseTypes.None,
                    Strength = 256,
                    Direction = Map.BuildingDirectionTypes.FirstOrDefault(),
                    BasePriority = curPriorityVal++,
                    IsPrebuilt = false
                };
                if (!Map.Buildings.CanAdd(location, toRebuild)) // || !Map.Technos.CanAdd(location, toRebuild, toRebuild.Type.BaseOccupyMask))
                {
                    Map.CheckBuildingBlockingCell(cell, buildingType, errors, ref modified, key);
                    continue;
                }
                Map.Buildings.Add(location, toRebuild);
            }
            foreach (KeyValuePair<string, string> kvp in baseSection)
            {
                if (baseKeyRegex.IsMatch(kvp.Key))
                {
                    errors.Add(String.Format("Base rebuild priority entry with key '{0}' exceeds count; skipping.", kvp.Key));
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

        private void LoadTerrain(INI ini, Dictionary<string, string> caseTrigs, HashSet<string> checkTerrTrigs, List<string> errors, ref bool modified)
        {
            INISection terrainSection = ini.Sections.Extract("Terrain");
            if (terrainSection == null)
            {
                return;
            }
            foreach (KeyValuePair<string, string> kvp in terrainSection)
            {
                int cell;
                if (!Int32.TryParse(kvp.Key, out cell))
                {
                    errors.Add(String.Format("Cell for terrain cannot be parsed. Key: '{0}', value: '{1}'; skipping.", kvp.Key, kvp.Value));
                    modified = true;
                    continue;
                }
                string[] tokens = kvp.Value.Split(',');
                TerrainType terrainType = Map.TerrainTypes.Where(t => t.Equals(tokens[0])).FirstOrDefault();
                if (terrainType == null)
                {
                    errors.Add(String.Format("Terrain '{0}' references unknown terrain.", tokens[0]));
                    modified = true;
                    continue;
                }
                if (Globals.FilterTheaterObjects && !terrainType.ExistsInTheater)
                {
                    errors.Add(String.Format("Terrain '{0}' is not available in the set theater; skipping.", terrainType.Name));
                    modified = true;
                    continue;
                }
                Terrain newTerr = new Terrain
                {
                    Type = terrainType
                };
                if (!Map.Technos.Add(cell, newTerr))
                {
                    ICellOccupier techno = Map.FindBlockingObject(cell, terrainType, false, out int blockingCell, out int placementCell);
                    string reportCell = blockingCell == -1 ? "<unknown>" : blockingCell.ToString();
                    if (techno is Terrain terrain)
                    {
                        errors.Add(String.Format("Terrain '{0}' on cell {1} overlaps terrain '{2}' placed on cell {3} at cell {4}; skipping.", terrainType.Name, cell, terrain.Type.Name, placementCell, reportCell));
                        modified = true;
                    }
                    else if (techno is InfantryGroup infantry)
                    {
                        errors.Add(String.Format("Terrain '{0}' on cell {1} overlaps infantry at cell {2}; skipping.", terrainType.Name, cell, reportCell));
                        modified = true;
                    }
                    else if (techno is Unit unit)
                    {
                        errors.Add(String.Format("Terrain '{0}' on cell {1} overlaps unit '{2}' at cell {3}; skipping.", terrainType.Name, cell, unit.Type.Name, reportCell));
                        modified = true;
                    }
                    else
                    {
                        if (blockingCell != -1)
                        {
                            errors.Add(String.Format("Terrain '{0}' placed on cell {1} overlaps unknown techno at cell {2}; skipping.", terrainType.Name, cell, reportCell));
                            modified = true;
                        }
                        else
                        {
                            errors.Add(String.Format("Terrain '{0}' placed on cell {1} crosses outside the map bounds; skipping.", terrainType.Name, cell));
                            modified = true;
                        }
                    }
                }
                // Optional trigger
                if (tokens.Length > 1)
                {
                    if (!caseTrigs.ContainsKey(tokens[1]))
                    {
                        errors.Add(String.Format("Terrain '{0}' on cell {1} links to unknown trigger '{2}'; clearing trigger.", terrainType.Name, cell, tokens[1]));
                        modified = true;
                        newTerr.Trigger = Trigger.None;
                    }
                    else if (!checkTerrTrigs.Contains(tokens[1]))
                    {
                        errors.Add(String.Format("Terrain '{0}' on cell {1} links to trigger '{2}' which does not contain an event applicable to terrain; clearing trigger.", terrainType.Name, cell, tokens[1]));
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
        }

        private void LoadOverlay(INI ini, List<string> errors, ref bool modified)
        {
            INISection overlaySection = ini.Sections.Extract("Overlay");
            if (overlaySection == null)
            {
                return;
            }
            int lastLine = Map.Metrics.Height - 1;
            foreach (KeyValuePair<string, string> kvp in overlaySection)
            {
                int cell;
                if (!Int32.TryParse(kvp.Key, out cell))
                {
                    errors.Add(String.Format("Cell for overlay cannot be parsed. Key: '{0}', value: '{1}'; skipping.", kvp.Key, kvp.Value));
                    modified = true;
                    continue;
                }
                if (!Map.Metrics.GetLocation(cell, out Point point))
                {
                    errors.Add(String.Format("Cell for overlay is not inside the map bounds. Key: '{0}', value: '{1}'; skipping.", kvp.Key, kvp.Value));
                    modified = true;
                    continue;
                }
                if (point.Y == 0 || point.Y == lastLine)
                {
                    errors.Add(String.Format("Overlay can not be placed on the first and or last lines of the map. Key: '{0}', value: '{1}'; skipping.", kvp.Key, kvp.Value));
                    modified = true;
                    continue;
                }
                OverlayType overlayType = Map.OverlayTypes.Where(t => t.Equals(kvp.Value)).FirstOrDefault();
                if (overlayType == null)
                {
                    if (OverlayTypes.Squishy.Equals(kvp.Value))
                    {
                        string disabledObjExpl = String.Format(consultManual, "DisableSquishMark");
                        errors.Add(String.Format("Overlay '{0}' is disabled in the editor. {1}", OverlayTypes.Squishy.Name, disabledObjExpl));
                        modified = true;
                    }
                    else
                    {
                        errors.Add(String.Format("Overlay '{0}' references unknown overlay.", kvp.Value));
                        modified = true;
                    }
                    continue;
                }
                if (Globals.FilterTheaterObjects && !overlayType.ExistsInTheater)
                {
                    errors.Add(String.Format("Overlay '{0}' is not available in the set theater; skipping.", overlayType.Name));
                    modified = true;
                    continue;
                }
                if ((overlayType.IsWall || overlayType.IsSolid) && Map.Buildings.ObjectAt(cell, out ICellOccupier techno))
                {
                    string ovldesc = overlayType.IsWall ? "Wall" : "Solid overlay";
                    if (techno is Building building)
                    {
                        errors.Add(String.Format("{0} '{1}' overlaps structure '{2}' at cell {3}; skipping.", ovldesc, overlayType.Name, building.Type.Name, cell));
                    }
                    else if (techno is Overlay ovl)
                    {
                        errors.Add(String.Format("{0} '{1}' overlaps overlay '{2}' at cell {3}; skipping.", ovldesc, overlayType.Name, ovl.Type.Name, cell));
                    }
                    else
                    {
                        // Should never happen since buildings are a separate layer now.
                        errors.Add(String.Format("{0} '{1}' overlaps unknown techno in cell {2}; skipping.", ovldesc, overlayType.Name, cell));
                    }
                    modified = true;
                    continue;
                }
                Map.Overlay[cell] = new Overlay { Type = overlayType, Icon = 0 };
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
                if (!Int32.TryParse(kvp.Key, out int waypoint))
                {
                    errors.Add(String.Format("Invalid waypoint '{0}' (expecting integer).", kvp.Key));
                    modified = true;
                    continue;
                }
                if (waypoint != 0 && kvp.Key.StartsWith("0"))
                {
                    errors.Add(String.Format("Waypoint '{0}' is zero-padded and will never be read by the game. Skipping.", kvp.Key));
                    continue;
                }
                if (!Int32.TryParse(kvp.Value, out int cell))
                {
                    errors.Add(String.Format("Waypoint {0} has invalid cell '{1}' (expecting integer).", waypoint, kvp.Value));
                    modified = true;
                    continue;
                }
                if (waypoint < 0 || waypoint >= Map.Waypoints.Length)
                {
                    // don't bother reporting illegal-but-empty entries.
                    if (cell != -1)
                    {
                        errors.Add(String.Format("Waypoint {0} out of range (expecting between {1} and {2}).", waypoint, 0, Map.Waypoints.Length - 1));
                        modified = true;
                    }
                    continue;
                }
                if (!Map.Metrics.Contains(cell))
                {
                    Map.Waypoints[waypoint].Cell = null;
                    // don't bother reporting illegal-but-empty entries.
                    if (cell != -1)
                    {
                        errors.Add(String.Format("Waypoint {0} cell value {1} out of range (expecting between {2} and {3}).", waypoint, cell, 0, Map.Metrics.Length - 1));
                        modified = true;
                    }
                    continue;
                }
                Map.Waypoints[waypoint].Cell = cell;
            }
        }

        private void LoadCellTriggers(INI ini, Dictionary<string, string> caseTrigs, HashSet<string> checkCellTrigs, List<string> errors, ref bool modified)
        {
            INISection cellTriggersSection = ini.Sections.Extract("CellTriggers");
            if (cellTriggersSection == null || cellTriggersSection.Count == 0)
            {
                return;
            }
            foreach (KeyValuePair<string, string> kvp in cellTriggersSection)
            {
                if (!Int32.TryParse(kvp.Key, out int cell))
                {
                    errors.Add(String.Format("Invalid cell trigger '{0}' (expecting integer).", kvp.Key));
                    modified = true;
                    continue;
                }
                if (!Map.Metrics.Contains(cell))
                {
                    errors.Add(String.Format("Cell trigger {0} is outside map bounds; skipping.", cell));
                    modified = true;
                    continue;
                }
                if (!caseTrigs.ContainsKey(kvp.Value))
                {
                    errors.Add(String.Format("Cell trigger {0} links to unknown trigger '{1}'; skipping.", cell, kvp.Value));
                    modified = true;
                    continue;
                }
                if (!checkCellTrigs.Contains(kvp.Value))
                {
                    errors.Add(String.Format("Cell trigger {0} links to trigger '{1}' which does not contain a placeable event; skipping.", cell, kvp.Value));
                    modified = true;
                    continue;
                }
                Map.CellTriggers[cell] = new CellTrigger(caseTrigs[kvp.Value]);
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
        }

        private void CheckSwitchToSolo(bool tryCheckSoloMission, bool dontReportSwitch, List<string> errors)
        {
            bool switchedToSolo = false;
            if (tryCheckSoloMission && !Map.BasicSection.SoloMission)
            {
                List<Trigger> triggers = Map.Triggers;
                switchedToSolo =
                    (triggers.Any(t => t.Action1.ActionType == ActionTypes.ACTION_WIN) && triggers.Any(t => t.Action1.ActionType == ActionTypes.ACTION_LOSE))
                    || triggers.Any(t => t.Event1.EventType == EventTypes.EVENT_ANY && t.Action1.ActionType == ActionTypes.ACTION_WINLOSE);
            }
            if (switchedToSolo)
            {
                Map.BasicSection.SoloMission = true;
                if ((!dontReportSwitch && Globals.ReportMissionDetection) || errors.Count > 0)
                {
                    errors.Insert(0, "Filename detected as classic single player mission format, and win and lose trigger detected. Applying \"SoloMission\" flag.");
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
            Dictionary<string, BuildingType> originals = BuildingTypes.GetTypes().ToDictionary(b => b.Name, StringComparer.OrdinalIgnoreCase);
            List<(Point Location, Building Occupier)> buildings = map.Buildings.OfType<Building>()
                 .OrderBy(pb => pb.Location.Y * map.Metrics.Width + pb.Location.X).ToList();
            // Remove all buildings
            if (!forFootPrintTest)
            {
                foreach ((Point p, Building b) in buildings)
                {
                    if (refreshPoints != null)
                    {
                        refreshPoints.UnionWith(OccupierSet.GetOccupyPoints(p, b));
                    }
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
                bType.HasBib = disableAllBibs ? false : orig.HasBib;
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
                if (refreshPoints != null)
                {
                    refreshPoints.UnionWith(OccupierSet.GetOccupyPoints(p, b));
                }
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

            foreach (House house in Map.Houses)
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
            string errors = Validate(fileType, false, forSole);
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
                        string iniText = forSole ? ini.ToString("\n") : FixRoad2Save(ini, "\n");
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
                                    throw new NotSupportedException("Megamaps cannot be saved in N64 format.");
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
                        string iniText = forSole ? ini.ToString("\n") : FixRoad2Save(ini, "\n");
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
            return retVal;
        }

        private void SaveSteamInfoIni(INI ini)
        {
            INISection generalSection = new INISection("General");
            generalSection["AuthorId"] = Map.SteamSection.Author;
            generalSection["Author"] = Map.BasicSection.Author;
            generalSection["ContentType"] = "0";
            generalSection["Flags"] = string.Empty;
            ini.Sections.Add(generalSection);

            INISection titleSection = new INISection("Title");
            titleSection["0"] = Map.BasicSection.Name;
            ini.Sections.Add(titleSection);

            INISection descriptionSection = new INISection("Description");
            string description = GamePluginRA.PreprocessBriefingText(Map.SteamSection.Description);
            List<string> finalLines = GamePluginRA.WriteClassicBriefing(description, Constants.BriefLineCutoffClassic);
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
            OverlayType[] fixTypes = Map.OverlayTypes.Where(ov => (ov.Flags & OverlayTypeFlag.RoadSpecial) != OverlayTypeFlag.None && ov.ForceTileNr > 0 && ov.GraphicsSource != ov.Name).ToArray();
            foreach (OverlayType road2 in fixTypes)
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
                    finalLines = GamePluginRA.WriteClassicBriefing(briefTextAt, Constants.BriefLineCutoffClassic);
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
                            writer.Write(ushort.MaxValue);
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
                aircraftSection[key] = String.Format("{0},{1},{2},{3},{4},{5}",
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
            foreach (var (location, terrain) in Map.Technos.OfType<Terrain>().OrderBy(t => Map.Metrics.GetCell(t.Location)))
            {
                if (Map.Metrics.GetCell(location, out int cell))
                {
                    terrainSection[cell.ToString()] = String.Format("{0},{1}", terrain.Type.Name.ToUpperInvariant(), terrain.Trigger);
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
                        writer.Write(byte.MaxValue);
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
                            List<int> cells;
                            if (!notFound.TryGetValue(template.Type.ID, out cells))
                            {
                                cells = new List<int>();
                                notFound.Add(template.Type.ID, cells);
                            }
                            cells.Add(Map.Metrics.GetCell(new Point(x, y)).Value);
                        }
                    }
                    // Fallback: write clear terrain.
                    writer.Write(byte.MaxValue);
                    writer.Write(byte.MaxValue);
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
                        errors.Add(String.Format("Template type \"{0}\" is not available in the N64 map type. Found cell{1}: {2}",
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
                string lastITem = errors[errors.Count - 1];
                errors[errors.Count - 1] = lastITem + "\n" + String.Format("If the map is saved to this type, {0} will be replaced with clear terrain.",
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

        protected void SaveMapPreview(Stream stream, bool renderAll)
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

        public virtual string Validate(FileType saveType, bool forResave, bool forWarnings)
        {
            return Validate(saveType, forResave, forWarnings, false);
        }

        protected string Validate(FileType saveType, bool forResave, bool forWarnings, bool forSole)
        {
            GameInfo gi = GameInfo;
            bool isN64 = saveType == FileType.I64 || saveType == FileType.B64;
            if (forWarnings)
            {
                return ValidateForWarnings(saveType, forResave, isN64, forSole);
            }
            List<string> errors = new List<string>();
            if (isN64 && isMegaMap)
            {
                errors.Add("Megamaps are not supported by the N64 map format.");
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

        private string ValidateForWarnings(FileType saveType, bool forResave, bool isN64, bool forSole)
        {
            // Check if map has name
            List<string> errors = new List<string>();
            if ((saveType == FileType.None || forResave) && this.GameInfo.MapNameIsEmpty(this.Map.BasicSection.Name))
            {
                errors.Add("Map name is empty. If you continue, the filename will be filled in as map name.");
            }
            if (isN64)
            {
                if (isMegaMap)
                {
                    // Fatal error. Don't bother asking anything for the warning; it'll go to the save attempt and get the fatal error.
                    return null;
                }
                string[][] plurArgs = new[] { new[] { string.Empty, "s" }, new[] { "does", "do" } };
                // Check houses
                CheckAllowed(errors, "Found usage of House{0} that {1} not exist in the N64 version: {2}.", plurArgs,
                    Map.GetAllTechnos().Where(t => t.TechnoType.Ownable).Select(t => t.House.Name)
                    .Concat(Map.Buildings.OfType<Building>().Select(b => b.Occupier.House.Name))
                    .Concat(Map.Triggers.Select(t => t.House)).Concat(Map.TeamTypes.Select(t => t.House.Name)).Distinct(),
                    HouseTypes.GetTypes().Select(h => h.Name), false, true, HouseTypes.None.Name, HouseTypes.Good.Name,
                    HouseTypes.Bad.Name, HouseTypes.Neutral.Name);
                // Check units
                CheckAllowed(errors, "Found unit type{0} that {1} not exist in the N64 version: {2}.", plurArgs,
                    Map.Technos.OfType<Unit>().Select(i => i.Occupier.Type.Name).Distinct(),
                    UnitTypes.GetTypes(false).Select(h => h.Name), true, false, UnitTypes.Visceroid.Name,
                    UnitTypes.SSM.Name, UnitTypes.Tric.Name, UnitTypes.Trex.Name, UnitTypes.Rapt.Name, UnitTypes.Steg.Name);
                // Check buildings
                CheckAllowed(errors, "Found building type{0} that {1} not exist in the N64 version: {2}.", plurArgs,
                    Map.Buildings.OfType<Building>().Select(i => i.Occupier.Type.Name).Distinct(),
                    BuildingTypes.GetTypes().Select(h => h.Name), true, false, BuildingTypes.Tanker.Name);
                // Check map
                List<string> n64MapErrs;
                using (MemoryStream binStream = new MemoryStream())
                using (BinaryWriter binWriter = new BinaryWriter(binStream))
                {
                    n64MapErrs = SaveBinaryClassicN64(binWriter, true);
                }
                errors.AddRange(n64MapErrs);
            }
            if (errors.Count > 0)
            {
                return "Warnings:\n* " + String.Join("\n* ", errors.ToArray());
            }
            return null;
        }

        private void CheckAllowed(List<string> errors, string format, string[][] plurArgs, IEnumerable<string> enumerable,
            IEnumerable<string> allTypes, bool uppercaseNames, bool allowFiltered, params string[] filtered)
        {
            HashSet<string> filteredItems = new HashSet<string>(filtered, StringComparer.OrdinalIgnoreCase);
            HashSet<string> foundBadItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string item in enumerable)
            {
                bool notAllowed = filteredItems.Contains(item);
                if (allowFiltered)
                {
                    notAllowed = !notAllowed;
                }
                if (notAllowed && !foundBadItems.Contains(item))
                {
                    foundBadItems.Add(item);
                }
            }
            if (foundBadItems.Count > 0)
            {
                string[] badItems = allTypes.Where(hname => foundBadItems.Contains(hname)).ToArray();
                if (uppercaseNames)
                {
                    badItems = badItems.Select(item => item.ToUpperInvariant()).ToArray();
                }
                bool plural = foundBadItems.Count > 1;
                object[] args = new object[plurArgs.Length + 1];
                args[plurArgs.Length] = String.Join(", ", badItems);
                for (int i = 0; i < plurArgs.Length; ++i) {
                    args[i] = plurArgs[i][plural ? 1 : 0];
                }
                errors.Add(String.Format(format,args));
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
                if (!Globals.EnableTd106Scripting)
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
                bool isDestroyableU = Globals.EnableTd106Scripting && "uuuu".Equals(trigName, StringComparison.OrdinalIgnoreCase);
                bool isDestroyableV = Globals.EnableTd106Scripting && "vvvv".Equals(trigName, StringComparison.OrdinalIgnoreCase);
                bool isDestroyableW = Globals.EnableTd106Scripting && "wwww".Equals(trigName, StringComparison.OrdinalIgnoreCase);
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
                if (Globals.EnableTd106Scripting)
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

        public String TriggerEventInfo(List<Trigger> triggers, string eventName)
        {
            if (eventName == null)
            {
                return null;
            }
            List<String> info = new List<string>();
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

        public String TriggerActionInfo(List<Trigger> triggers, string actionName)
        {
            if (actionName == null)
            {
                return null;
            }
            List<String> info = new List<string>();
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
                    String targetAct = toDestr.Action1?.ActionType;
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
                string flag = flagColorNames[i];
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
            bool? capturable = building.Type.Capturable;
            // TODO add checks on triggers and 1.06 rule tweaks.
            BuildingType bt = BuildingTypes.GetTypes().FirstOrDefault(b => String.Equals(building.Type.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            List<string> infoList = new List<string>();
            info = null;
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
