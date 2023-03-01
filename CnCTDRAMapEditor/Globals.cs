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
            double minScale = 1.0 / Math.Min(OriginalTileWidth, OriginalTileHeight);

            BoundsObstructFill = Properties.Settings.Default.DefaultBoundsObstructFill;
            TileDragProtect = Properties.Settings.Default.DefaultTileDragProtect;
            TileDragRandomize = Properties.Settings.Default.DefaultTileDragRandomize;
            ShowPlacementGrid = Properties.Settings.Default.DefaultShowPlacementGrid;
            CratesOnTop = Properties.Settings.Default.DefaultCratesOnTop;
            ShowMapGrid = Properties.Settings.Default.DefaultShowMapGrid;

            ExportTileScale = Math.Min(1, Math.Max(minScale, Math.Abs(Properties.Settings.Default.DefaultExportScale)));
            ExportSmoothScale = Properties.Settings.Default.DefaultExportScale < 0;
            MapGridColor = Properties.Settings.Default.MapGridColor;
            MapBackColor = Color.FromArgb(255, Properties.Settings.Default.MapBackColor);

            MapTileScale = Math.Min(1, Math.Max(minScale, Math.Abs(Properties.Settings.Default.MapScale)));
            MapSmoothScale = Properties.Settings.Default.MapScale < 0;
            PreviewTileScale = Math.Min(1, Math.Max(minScale, Math.Abs(Properties.Settings.Default.PreviewScale)));
            PreviewSmoothScale = Properties.Settings.Default.PreviewScale < 0;
            UndoRedoStackSize = Properties.Settings.Default.UndoRedoStackSize;
            MinimumClampSize = Properties.Settings.Default.MinimumClampSize;
            NoMetaFilesForSinglePlay = Properties.Settings.Default.NoMetaFilesForSinglePlay;
            ConvertRaObsoleteClear = Properties.Settings.Default.ConvertRaObsoleteClear;
            BlockingBibs = Properties.Settings.Default.BlockingBibs;
            DisableAirUnits = Properties.Settings.Default.DisableAirUnits;
            ConvertCraters = Properties.Settings.Default.ConvertCraters;
            FilterTheaterObjects = Properties.Settings.Default.FilterTheaterObjects;
            WriteClassicBriefing = Properties.Settings.Default.WriteClassicBriefing;
            ApplyHarvestBug = Properties.Settings.Default.ApplyHarvestBug;
            NoOwnedObjectsInSole = Properties.Settings.Default.NoOwnedObjectsInSole;
            AdjustSoleTeleports = Properties.Settings.Default.DrawSoleTeleports;
            ExpandSoleLimits = !Properties.Settings.Default.RestrictSoleLimits;
        }

        public const string TilesetsXMLPath = @"DATA\XML\TILESETS.XML";
        public const string TexturesPath = @"DATA\ART\TEXTURES\SRGB";
        public const string MegafilePath = @"DATA";
        public const string GameTextFilenameFormat = @"DATA\TEXT\MASTERTEXTFILE_{0}.LOC";

        public const int OriginalTileWidth = 128;
        public const int OriginalTileHeight = 128;
        public static readonly Size OriginalTileSize = new Size(OriginalTileWidth, OriginalTileHeight);

        public const int PixelWidth = 24;
        public const int PixelHeight = 24;

        public static bool BoundsObstructFill { get; set; }
        public static bool TileDragProtect { get; set; }
        public static bool TileDragRandomize { get; set; }
        public static bool ShowPlacementGrid { get; set; }
        public static bool CratesOnTop { get; set; }
        public static bool ShowMapGrid { get; set; }

        public static double ExportTileScale { get; private set; }
        public static bool ExportSmoothScale { get; private set; }
        public static Color MapGridColor { get; private set; }
        public static Color MapBackColor { get; private set; }

        public static double MapTileScale { get; private set; }
        public static bool MapSmoothScale { get; private set; }
        public static int MapTileWidth => Math.Max(1, (int)(OriginalTileWidth * MapTileScale));
        public static int MapTileHeight => Math.Max(1, (int)(OriginalTileHeight * MapTileScale));
        public static Size MapTileSize => new Size(MapTileWidth, MapTileHeight);

        public static double PreviewTileScale { get; private set; }
        public static bool PreviewSmoothScale { get; private set; }
        public static int PreviewTileWidth => Math.Max(1, (int)(OriginalTileWidth * PreviewTileScale));
        public static int PreviewTileHeight => (int)(OriginalTileHeight * PreviewTileScale);
        public static Size PreviewTileSize => new Size(PreviewTileWidth, PreviewTileHeight);

        public static int UndoRedoStackSize { get; private set; }
        public static Size MinimumClampSize { get; private set; }
        public static bool NoMetaFilesForSinglePlay { get; private set; }
        public static bool ConvertRaObsoleteClear { get; private set; }
        public static bool BlockingBibs { get; private set; }
        public static bool DisableAirUnits { get; private set; }
        public static bool ConvertCraters { get; private set; }
        public static bool FilterTheaterObjects { get; private set; }
        public static bool WriteClassicBriefing { get; private set; }
        public static bool ApplyHarvestBug { get; private set; }
        public static bool NoOwnedObjectsInSole { get; private set; }
        public static bool AdjustSoleTeleports { get; private set; }
        public static bool ExpandSoleLimits { get; private set; }

        public static readonly Size MapPreviewSize = new Size(512, 512);
        public static readonly Size WorkshopPreviewSize = new Size(512, 512);

        public static readonly string[] MapEdges = new string[] { "North", "East", "South", "West" };
        public const int NumInfantryStops = 5;

        public const int MaxTeamClasses = 5;
        public const int MaxTeamMissions = 20;

        public const long MaxMapSize = 0x20000;

        public static MegafileManager TheMegafileManager;
        public static TextureManager TheTextureManager;
        public static TilesetManager TheTilesetManager;
        public static TeamColorManager TheTeamColorManager;
        public static GameTextManager TheGameTextManager;

        public static readonly string RootSaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"CnCRemastered\Local_Custom_Maps");

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
