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
using System.Drawing;
using System.IO;

namespace MobiusEditor
{
    public static class Globals
    {
        static Globals()
        {
            // Startup options
            UseClassicFiles = Properties.Settings.Default.UseClassicFiles;
            EditorLanguage = Properties.Settings.Default.EditorLanguage;
            EnableDpiAwareness = Properties.Settings.Default.EnableDpiAwareness;
            CheckUpdatesOnStartup = Properties.Settings.Default.CheckUpdatesOnStartup;
            ClassicNoRemasterLogic = Properties.Settings.Default.ClassicNoRemasterLogic;
            MixContentInfoFile = Properties.Settings.Default.MixContentInfoFile;
            // Defaults
            BoundsObstructFill = Properties.Settings.Default.DefaultBoundsObstructFill;
            TileDragProtect = Properties.Settings.Default.DefaultTileDragProtect;
            TileDragRandomize = Properties.Settings.Default.DefaultTileDragRandomize;
            ShowPlacementGrid = Properties.Settings.Default.DefaultShowPlacementGrid;
            OutlineAllCrates = Properties.Settings.Default.DefaultOutlineAllCrates;
            CratesOnTop = Properties.Settings.Default.DefaultCratesOnTop;
            // Fine tuning
            ZoomToBoundsOnLoad = Properties.Settings.Default.ZoomToBoundsOnLoad;
            RememberToolData = Properties.Settings.Default.RememberToolData;

            MapBackColor = Color.FromArgb(255, Properties.Settings.Default.MapBackColor);
            MapGridColor = Properties.Settings.Default.MapGridColor;

            HashColorTechnoPart = Properties.Settings.Default.HashColorTechnoPart;
            HashColorTechnoFull = Properties.Settings.Default.HashColorTechnoFull;
            HashColorLandClear = Properties.Settings.Default.HashColorLandClear;
            HashColorLandBeach = Properties.Settings.Default.HashColorLandBeach;
            HashColorLandRock = Properties.Settings.Default.HashColorLandRock;
            HashColorLandRoad = Properties.Settings.Default.HashColorLandRoad;
            HashColorLandWater = Properties.Settings.Default.HashColorLandWater;
            HashColorLandRiver = Properties.Settings.Default.HashColorLandRiver;
            HashColorLandRough = Properties.Settings.Default.HashColorLandRough;

            OutlineColorCrateWood = Properties.Settings.Default.OutlineColorCrateWood;
            OutlineColorCrateSteel = Properties.Settings.Default.OutlineColorCrateSteel;
            OutlineColorTerrain = Properties.Settings.Default.OutlineColorTerrain;
            OutlineColorSolidOverlay = Properties.Settings.Default.OutlineColorSolidOverlay;
            OutlineColorWall = Properties.Settings.Default.OutlineColorWall;
            PreviewAlphaFloat = Properties.Settings.Default.PreviewAlpha;
            UnbuiltAlphaFloat = Properties.Settings.Default.UnbuiltAlpha;
            IgnoreShadowOverlap = Properties.Settings.Default.IgnoreShadowOverlap;

            UndoRedoStackSize = Properties.Settings.Default.UndoRedoStackSize;
            MinimumClampSize = Properties.Settings.Default.MinimumClampSize;
            // Behavior tweaks
            ReportMissionDetection = Properties.Settings.Default.ReportMissionDetection;
            AllowWallBuildings = !Properties.Settings.Default.OverlayWallsOnly;
            EnforceObjectMaximums = Properties.Settings.Default.EnforceObjectMaximums;
            EnforceTriggerTypes = Properties.Settings.Default.EnforceTriggerTypes;
            Ignore106Scripting = Properties.Settings.Default.Ignore106Scripting;
            ClassicProducesNoMetaFiles = Properties.Settings.Default.ClassicProducesNoMetaFiles;
            ClassicEncodesNameAsCp437 = Properties.Settings.Default.ClassicEncodesNameAsCp437;
            ConvertRaObsoleteClear = Properties.Settings.Default.ConvertRaObsoleteClear;
            BlockingBibs = Properties.Settings.Default.BlockingBibs;
            DisableAirUnits = Properties.Settings.Default.DisableAirUnits;
            ConvertCraters = Properties.Settings.Default.ConvertCraters;
            DisableSquishMark = Properties.Settings.Default.DisableSquishMark;
            FilterTheaterObjects = Properties.Settings.Default.FilterTheaterObjects;
            WriteClassicBriefing = Properties.Settings.Default.WriteClassicBriefing;
            ApplyHarvestBug = Properties.Settings.Default.ApplyHarvestBug;
            FixClassicEinstein = Properties.Settings.Default.FixClassicEinstein;
            FixConcretePavement = Properties.Settings.Default.FixConcretePavement;
            NoOwnedObjectsInSole = Properties.Settings.Default.NoOwnedObjectsInSole;
            AdjustSoleTeleports = Properties.Settings.Default.DrawSoleTeleports;
            RestrictSoleLimits = Properties.Settings.Default.RestrictSoleLimits;
        }

        public const string TilesetsXMLPath = @"DATA\XML\TILESETS.XML";
        public const string TexturesPath = @"DATA\ART\TEXTURES\SRGB";
        public const string MegafilePath = @"DATA";
        public const string GameTextFilenameFormat = @"DATA\TEXT\MASTERTEXTFILE_{0}.LOC";

        public static int OriginalTileWidth { get { return UseClassicFiles ? 24 : 128; } }
        public static int OriginalTileHeight { get { return UseClassicFiles ? 24 : 128; } }
        public static Size OriginalTileSize => new Size(OriginalTileWidth, OriginalTileHeight);

        public const int PixelWidth = 24;
        public const int PixelHeight = 24;

        public static int ZOrderDefault = 10;
        public static int ZOrderPaved = 5;
        public static int ZOrderFlat = 1;
        public static int ZOrderFloor = 0;

        public static bool UseClassicFiles { get; set; }
        public static string EditorLanguage { get; set; }
        public static bool EnableDpiAwareness { get; set; }
        public static bool CheckUpdatesOnStartup { get; set; }
        public static bool ClassicNoRemasterLogic { get; set; }
        public static string MixContentInfoFile { get; set; }

        public static bool BoundsObstructFill { get; set; }
        public static bool TileDragProtect { get; set; }
        public static bool TileDragRandomize { get; set; }
        public static bool ShowPlacementGrid { get; set; }
        public static bool OutlineAllCrates { get; set; }
        public static bool CratesOnTop { get; set; }

        public static double ExportTileScale
        {
            get
            {
                double defExpScale = UseClassicFiles ? Properties.Settings.Default.DefaultExportScaleClassic : Properties.Settings.Default.DefaultExportScale;
                return Math.Max(GetMinScale(), Math.Abs(defExpScale));
            }
        }

        public static bool ExportSmoothScale
        {
            get
            {
                return (UseClassicFiles ? Properties.Settings.Default.DefaultExportScaleClassic : Properties.Settings.Default.DefaultExportScale) < 0;
            }
        }

        public static bool ZoomToBoundsOnLoad { get; private set; }
        public static bool RememberToolData { get; private set; }
        public static Color MapBackColor { get; private set; }
        public static Color MapGridColor { get; private set; }

        public static Color HashColorTechnoPart { get; private set; }
        public static Color HashColorTechnoFull { get; private set; }
        public static Color HashColorLandClear { get; private set; }
        public static Color HashColorLandBeach { get; private set; }
        public static Color HashColorLandRock { get; private set; }
        public static Color HashColorLandRoad { get; private set; }
        public static Color HashColorLandWater { get; private set; }
        public static Color HashColorLandRiver { get; private set; }
        public static Color HashColorLandRough { get; private set; }

        public static Color OutlineColorCrateWood { get; private set; }
        public static Color OutlineColorCrateSteel { get; private set; }
        public static Color OutlineColorTerrain { get; private set; }
        public static Color OutlineColorSolidOverlay { get; private set; }
        public static Color OutlineColorWall { get; private set; }

        public static float PreviewAlphaFloat { get; private set; }
        public static int PreviewAlphaInt => ((int)(PreviewAlphaFloat * 256)).Restrict(0, 255);
        public static float UnbuiltAlphaFloat { get; private set; }
        public static int UnbuiltAlphaInt => ((int)(UnbuiltAlphaFloat * 256)).Restrict(0, 255);

        public static bool IgnoreShadowOverlap { get; private set; }

        private static double GetMinScale(){ return 1.0 / Math.Min(OriginalTileWidth, OriginalTileHeight); }
        public static double MapTileScale => Math.Max(GetMinScale(), Math.Abs(UseClassicFiles ? Properties.Settings.Default.MapScaleClassic : Properties.Settings.Default.MapScale));
        public static bool MapSmoothScale => (UseClassicFiles ? Properties.Settings.Default.MapScaleClassic : Properties.Settings.Default.MapScale) < 0;
        public static int MapTileWidth => Math.Max(1, (int)Math.Round(OriginalTileWidth * MapTileScale));
        public static int MapTileHeight => Math.Max(1, (int)Math.Round(OriginalTileHeight * MapTileScale));
        public static Size MapTileSize => new Size(MapTileWidth, MapTileHeight);

        public static double PreviewTileScale =>  Math.Max(GetMinScale(), Math.Abs(UseClassicFiles ? Properties.Settings.Default.PreviewScaleClassic : Properties.Settings.Default.PreviewScale));
        public static bool PreviewSmoothScale => (UseClassicFiles ? Properties.Settings.Default.PreviewScaleClassic : Properties.Settings.Default.PreviewScale) < 0;
        public static int PreviewTileWidth => Math.Max(1, (int)Math.Round(OriginalTileWidth * PreviewTileScale));
        public static int PreviewTileHeight => (int)Math.Round(OriginalTileHeight * PreviewTileScale);
        public static Size PreviewTileSize => new Size(PreviewTileWidth, PreviewTileHeight);

        public static int UndoRedoStackSize { get; private set; }
        public static Size MinimumClampSize { get; private set; }

        public static bool ReportMissionDetection { get; private set; }
        public static bool AllowWallBuildings { get; private set; }
        public static bool EnforceObjectMaximums { get; private set; }
        public static bool EnforceTriggerTypes { get; private set; }
        public static bool Ignore106Scripting { get; private set; }
        public static bool ClassicProducesNoMetaFiles { get; private set; }
        public static bool ConvertRaObsoleteClear { get; private set; }
        public static bool BlockingBibs { get; private set; }
        public static bool DisableAirUnits { get; private set; }
        public static bool ConvertCraters { get; private set; }
        public static bool FilterTheaterObjects { get; private set; }
        public static bool WriteClassicBriefing { get; private set; }
        public static bool ClassicEncodesNameAsCp437 { get; private set; }
        public static bool ApplyHarvestBug { get; private set; }
        public static bool NoOwnedObjectsInSole { get; private set; }
        public static bool FixClassicEinstein { get; private set; }
        public static bool FixConcretePavement { get; private set; }
        public static bool DisableSquishMark { get; private set; }
        public static bool AdjustSoleTeleports { get; private set; }
        public static bool RestrictSoleLimits { get; private set; }

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
