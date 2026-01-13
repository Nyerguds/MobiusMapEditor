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
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace MobiusEditor
{
    public static class Globals
    {
        static Globals()
        {
            // General options
            SetEnabledGames(Properties.Settings.Default.EnabledGames);
            //LazyInitSteam: used directly, since it's only used once on startup.
            EditorLanguage = Properties.Settings.Default.EditorLanguage;
            EnableDpiAwareness = Properties.Settings.Default.EnableDpiAwareness;
            CheckUpdatesOnStartup = Properties.Settings.Default.CheckUpdatesOnStartup;

            // Classic files
            UseClassicFiles = Properties.Settings.Default.UseClassicFiles;
            //ClassicPathTD/RA/SS: used by the GameInfo classes directly
            ClassicNoRemasterLogic = Properties.Settings.Default.ClassicNoRemasterLogic;
            ClassicProducesNoMetaFiles = Properties.Settings.Default.ClassicProducesNoMetaFiles;
            ClassicEncodesNameAsCp437 = Properties.Settings.Default.ClassicEncodesNameAsCp437;
            //ModsToLoadTD/RA/SS: used by the GameInfo classes directly
            MixContentInfoFile = Properties.Settings.Default.MixContentInfoFile;

            // Defaults
            BoundsObstructFill = Properties.Settings.Default.DefaultBoundsObstructFill;
            TileDragProtect = Properties.Settings.Default.DefaultTileDragProtect;
            TileDragRandomize = Properties.Settings.Default.DefaultTileDragRandomize;
            ShowPlacementGrid = Properties.Settings.Default.DefaultShowPlacementGrid;
            CratesOnTop = Properties.Settings.Default.DefaultCratesOnTop;
            //ExportTileScale / ExportTileScaleClassic: auto-selected in ExportTileScale property.
            ExportMultiMapsInBounds = Properties.Settings.Default.DefaultExportMultiInBounds;
            OutlineAllCrates = Properties.Settings.Default.DefaultOutlineAllCrates;

            // Fine tuning
            ZoomToBoundsOnLoad = Properties.Settings.Default.ZoomToBoundsOnLoad;
            RememberToolData = Properties.Settings.Default.RememberToolData;
            AllowDeleteRoutePoints = Properties.Settings.Default.AllowDeleteRoutePoints;
            //MapScale / MapScaleClassic: auto-selected in MapScale property.
            //PreviewScale / PreviewScaleClassic: auto-selected in PreviewScale property.
            //ObjectToolItemSizeMultiplier/TemplateToolTextureSizeMultiplier/MaxMapTileTextureSize: used in controls directly.
            UndoRedoStackSize = Properties.Settings.Default.UndoRedoStackSize;
            MinimumClampSize = Properties.Settings.Default.MinimumClampSize;

            // Colors, hashing, outlines and transparency
            MapBackColor = Color.FromArgb(255, Properties.Settings.Default.MapBackColor);
            MapGridColor = Properties.Settings.Default.MapGridColor;

            HashColorLandClear = Properties.Settings.Default.HashColorLandClear;
            HashColorLandBeach = Properties.Settings.Default.HashColorLandBeach;
            HashColorLandRock = Properties.Settings.Default.HashColorLandRock;
            HashColorLandRoad = Properties.Settings.Default.HashColorLandRoad;
            HashColorLandWater = Properties.Settings.Default.HashColorLandWater;
            HashColorLandRiver = Properties.Settings.Default.HashColorLandRiver;
            HashColorLandRough = Properties.Settings.Default.HashColorLandRough;

            HashColorTechnoPart = Properties.Settings.Default.HashColorTechnoPart;
            HashColorTechnoFull = Properties.Settings.Default.HashColorTechnoFull;

            OutlineColorCrateWood = Properties.Settings.Default.OutlineColorCrateWood;
            OutlineColorCrateSteel = Properties.Settings.Default.OutlineColorCrateSteel;
            OutlineColorTerrain = Properties.Settings.Default.OutlineColorTerrain;
            OutlineColorSolidOverlay = Properties.Settings.Default.OutlineColorSolidOverlay;
            OutlineColorWall = Properties.Settings.Default.OutlineColorWall;

            IgnoreShadowOverlap = Properties.Settings.Default.IgnoreShadowOverlap;

            PreviewAlphaFloat = Properties.Settings.Default.PreviewAlpha;
            UnbuiltAlphaFloat = Properties.Settings.Default.UnbuiltAlpha;

            // Behavior tweaks
            ReportMissionDetection = Properties.Settings.Default.ReportMissionDetection;
            EnforceObjectMaximums = Properties.Settings.Default.EnforceObjectMaximums;
            EnforceTriggerTypes = Properties.Settings.Default.EnforceTriggerTypes;
            ConvertRaObsoleteClear = Properties.Settings.Default.ConvertRaObsoleteClear;
            BlockingBibs = Properties.Settings.Default.BlockingBibs;
            DisableAirUnits = Properties.Settings.Default.DisableAirUnits;
            ConvertCraters = Properties.Settings.Default.ConvertCraters;
            DisableSquishMark = Properties.Settings.Default.DisableSquishMark;
            FilterTheaterObjects = Properties.Settings.Default.FilterTheaterObjects;
            WriteClassicBriefing = Properties.Settings.Default.WriteClassicBriefing;
            WriteRemasterBriefing = Properties.Settings.Default.WriteRemasterBriefing;
            ApplyHarvestBug = Properties.Settings.Default.ApplyHarvestBug;
            AllowWallBuildings = !Properties.Settings.Default.OverlayWallsOnly;
            NoOwnedObjectsInSole = Properties.Settings.Default.NoOwnedObjectsInSole;

            // Mod / 3rd party tweak support
            EnableTd106LineBreaks = Properties.Settings.Default.EnableTd106LineBreaks;
            ExpandTdScripting = !Properties.Settings.Default.ExpandTdScripting;
            // Experimental; not enabled for now.
            EnableTdRpmFormat = false;
            EnableTdClassicMultiLine = false;

            // Graphical tweaks
            FixClassicEinstein = Properties.Settings.Default.FixClassicEinstein;
            AllowImageModsInMaps = Properties.Settings.Default.AllowImageModsInMaps;
            LandedHelisTd = Properties.Settings.Default.TdHelisSpawnOnGround;
            LandedHelisRa = Properties.Settings.Default.RaHelisSpawnOnGround;
            FixConcretePavement = Properties.Settings.Default.FixConcretePavement;
            TdWideBridges = Properties.Settings.Default.TdWideBridges;
            AdjustSoleTeleports = Properties.Settings.Default.DrawSoleTeleports;
        }

        private static void SetEnabledGames(string eg)
        {
            string[] enabledGames = (eg ?? String.Empty)
                            .Split(',', ';').Select(g => g.Trim()).ToArray();
            EnabledGames = enabledGames.ToHashSet(StringComparer.OrdinalIgnoreCase);
            EnabledGamesOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int order = 0;
            foreach (string game in enabledGames)
            {
                if (!EnabledGamesOrder.ContainsKey(game))
                {
                    EnabledGamesOrder[game] = order++;
                }
            }
        }

        // Harcoded values
        public const string TilesetsXMLPath = @"DATA\XML\TILESETS.XML";
        public const string TexturesPath = @"DATA\ART\TEXTURES\SRGB";
        public const string MegafilePath = @"DATA";
        public const string GameTextFilenameFormat = @"DATA\TEXT\MASTERTEXTFILE_{0}.LOC";

        public static int OriginalTileWidth { get { return UseClassicFiles ? 24 : 128; } }
        public static int OriginalTileHeight { get { return UseClassicFiles ? 24 : 128; } }
        public static Size OriginalTileSize => new Size(OriginalTileWidth, OriginalTileHeight);

        public const int PixelWidth = 24;
        public const int PixelHeight = 24;
        public static Size PixelSize => new Size(PixelWidth, PixelHeight);

        public static int ZOrderFlying = 24;
        public static int ZOrderTop = 20;
        public static int ZOrderBelowTop = 19;
        public static int ZOrderHigher = 18;
        public static int ZOrderHigh = 15;
        public static int ZOrderDefault = 10;
        public static int ZOrderPaved = 5;
        public static int ZOrderFlat = 2;
        public static int ZOrderOverlay = 1;
        public static int ZOrderFloor = 0;

        // Global settings
        public static HashSet<string> EnabledGames { get; set; }
        public static Dictionary<string, int> EnabledGamesOrder { get; set; }
        public static string EditorLanguage { get; set; }
        public static bool EnableDpiAwareness { get; set; }
        public static bool CheckUpdatesOnStartup { get; set; }

        public static bool UseClassicFiles { get; set; }
        public static bool ClassicNoRemasterLogic { get; set; }
        public static bool ClassicProducesNoMetaFiles { get; private set; }
        public static bool ClassicEncodesNameAsCp437 { get; private set; }
        public static string MixContentInfoFile { get; set; }

        public static bool BoundsObstructFill { get; set; }
        public static bool TileDragProtect { get; set; }
        public static bool TileDragRandomize { get; set; }
        public static bool ShowPlacementGrid { get; set; }
        public static bool CratesOnTop { get; set; }
        private static double ExportTileScaleRaw => UseClassicFiles ? Properties.Settings.Default.DefaultExportScaleClassic : Properties.Settings.Default.DefaultExportScale;
        public static double ExportTileScale => Math.Max(MinScale, Math.Abs(ExportTileScaleRaw));
        public static bool ExportSmoothScale => ExportTileScaleRaw < 0;
        public static bool ExportMultiMapsInBounds { get; set; }
        public static bool OutlineAllCrates { get; set; }

        public static bool ZoomToBoundsOnLoad { get; private set; }
        public static bool RememberToolData { get; private set; }
        public static bool AllowDeleteRoutePoints { get; private set; }

        private static double MinScale => 1.0 / Math.Min(OriginalTileWidth, OriginalTileHeight);
        private static double MapTileScaleRaw => UseClassicFiles ? Properties.Settings.Default.MapScaleClassic : Properties.Settings.Default.MapScale;
        public static double MapTileScale => Math.Max(MinScale, Math.Abs(MapTileScaleRaw));
        public static bool MapSmoothScale => MapTileScaleRaw < 0;
        public static int MapTileWidth => Math.Max(1, (int)Math.Round(OriginalTileWidth * MapTileScale));
        public static int MapTileHeight => Math.Max(1, (int)Math.Round(OriginalTileHeight * MapTileScale));
        public static Size MapTileSize => new Size(MapTileWidth, MapTileHeight);

        private static double PreviewTileScaleRaw => UseClassicFiles ? Properties.Settings.Default.PreviewScaleClassic : Properties.Settings.Default.PreviewScale;
        public static double PreviewTileScale => Math.Max(MinScale, Math.Abs(PreviewTileScaleRaw));
        public static bool PreviewSmoothScale => (PreviewTileScaleRaw) < 0;
        public static int PreviewTileWidth => Math.Max(1, (int)Math.Round(OriginalTileWidth * PreviewTileScale));
        public static int PreviewTileHeight => (int)Math.Round(OriginalTileHeight * PreviewTileScale);
        public static Size PreviewTileSize => new Size(PreviewTileWidth, PreviewTileHeight);

        public static int UndoRedoStackSize { get; private set; }
        public static Size MinimumClampSize { get; private set; }

        public static Color MapBackColor { get; private set; }
        public static Color MapGridColor { get; private set; }

        public static Color HashColorLandClear { get; private set; }
        public static Color HashColorLandBeach { get; private set; }
        public static Color HashColorLandRock { get; private set; }
        public static Color HashColorLandRoad { get; private set; }
        public static Color HashColorLandWater { get; private set; }
        public static Color HashColorLandRiver { get; private set; }
        public static Color HashColorLandRough { get; private set; }

        public static Color HashColorTechnoPart { get; private set; }
        public static Color HashColorTechnoFull { get; private set; }

        public static Color OutlineColorCrateWood { get; private set; }
        public static Color OutlineColorCrateSteel { get; private set; }
        public static Color OutlineColorTerrain { get; private set; }
        public static Color OutlineColorSolidOverlay { get; private set; }
        public static Color OutlineColorWall { get; private set; }

        public static bool IgnoreShadowOverlap { get; private set; }

        public static float PreviewAlphaFloat { get; private set; }
        public static int PreviewAlphaInt => ((int)(PreviewAlphaFloat * 256)).Restrict(0, 255);
        public static float UnbuiltAlphaFloat { get; private set; }
        public static int UnbuiltAlphaInt => ((int)(UnbuiltAlphaFloat * 256)).Restrict(0, 255);

        public static bool ReportMissionDetection { get; private set; }
        public static bool EnforceObjectMaximums { get; private set; }
        public static bool EnforceTriggerTypes { get; private set; }
        public static bool ConvertRaObsoleteClear { get; private set; }
        public static bool BlockingBibs { get; private set; }
        public static bool DisableAirUnits { get; private set; }
        public static bool ConvertCraters { get; private set; }
        public static bool DisableSquishMark { get; private set; }
        public static bool FilterTheaterObjects { get; private set; }
        public static bool WriteClassicBriefing { get; private set; }
        public static bool WriteRemasterBriefing { get; private set; }
        public static bool ApplyHarvestBug { get; private set; }
        public static bool AllowWallBuildings { get; private set; }
        public static bool NoOwnedObjectsInSole { get; private set; }

        // Mod / 3rd party tweak support
        public static bool EnableTd106LineBreaks { get; private set; }
        public static bool ExpandTdScripting { get; private set; }
        // Experimental; not enabled for now.
        /// <summary>Embed the map inside the ini file for TD. This is just an experiment, and not normally enabled.</summary>
        public static bool EnableTdRpmFormat { get; private set; }
        /// <summary>Use RA style '@' line breaks in the classic briefing. Like the embedded map, this is experimental and not normally enabled. If enabled, it takes priority over the 1.06 line breaks logic.</summary>
        public static bool EnableTdClassicMultiLine { get; private set; }

        public static bool FixClassicEinstein { get; private set; }
        public static bool AllowImageModsInMaps { get; private set; }
        public static bool FixConcretePavement { get; private set; }
        public static bool TdWideBridges { get; private set; }
        public static bool LandedHelisTd { get; private set; }
        public static bool LandedHelisRa { get; private set; }
        public static bool AdjustSoleTeleports { get; private set; }

        public static readonly Size MapPreviewSize = new Size(512, 512);
        public static readonly Size WorkshopPreviewSize = new Size(512, 512);

        public static readonly string[] MapEdges = new string[] { "North", "East", "South", "West" };
        public const int NumInfantryStops = 5;

        public const int MaxTeamClasses = 5;
        public const int MaxTeamMissions = 20;

        public const long MaxMapSize = 0x20000;

        public static IArchiveManager TheArchiveManager;
        public static ITilesetManager TheTilesetManager;
        public static ITeamColorManager TheTeamColorManager;
        public static IGameTextManager TheGameTextManager;
        public static ShapeCacheManager TheShapeCacheManager;

        public static readonly string RootSaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"CnCRemastered\Local_Custom_Maps");
        public static readonly string ModDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"CnCRemastered\Mods");

        // for encrypted mix files.
        public static readonly string PublicKey = "AihRvNoIbTn85FZRYNZRcT+i6KpU+maCsEqr3Q5q+LDB5tH7Tz2qQ38V";
        public static readonly string PrivateKey = "AigKVje8mROcR8QixnxUEF5b29Curkq01DNDWCdOG99XBqH79OaCiTCB";

#if DEVELOPER
        public static class Developer
        {
            public static bool ShowOverlapCells = false;
        }
#endif
    }
}
