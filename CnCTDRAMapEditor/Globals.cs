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
            MapTileScale = Math.Min(1, Math.Max(0.05f, Math.Abs(Properties.Settings.Default.MapScale)));
            MapSmoothScale = Properties.Settings.Default.MapScale < 0;
            PreviewTileScale = Math.Min(1, Math.Max(0.05f, Math.Abs(Properties.Settings.Default.PreviewScale)));
            PreviewSmoothScale = Properties.Settings.Default.PreviewScale < 0;
            ExportTileScale = Math.Min(1, Math.Abs(Properties.Settings.Default.DefaultExportScale));
            ExportSmoothScale = Properties.Settings.Default.DefaultExportScale < 0;
            UndoRedoStackSize = Properties.Settings.Default.UndoRedoStackSize;
            DisableAirUnits = Properties.Settings.Default.DisableAirUnits;
            ConvertCraters = Properties.Settings.Default.ConvertCraters;
            BlockingBibs = Properties.Settings.Default.BlockingBibs;
            ConvertRaObsoleteClear = Properties.Settings.Default.ConvertRaObsoleteClear;
            BoundsObstructFill = Properties.Settings.Default.BoundsObstructFill;
            FilterTheaterObjects = Properties.Settings.Default.FilterTheaterObjects;
            NoOwnedObjectsInSole = Properties.Settings.Default.NoOwnedObjectsInSole;
        }

        public const string TilesetsXMLPath = @"DATA\XML\TILESETS.XML";
        public const string TexturesPath = @"DATA\ART\TEXTURES\SRGB";
        public const string MegafilePath = @"DATA";
        public const string GameTextFilenameFormat = @"DATA\TEXT\MASTERTEXTFILE_{0}.LOC";

        public const int OriginalTileWidth = 128;
        public const int OriginalTileHeight = 128;
        public static readonly Size OriginalTileSize = new Size(OriginalTileWidth, OriginalTileHeight);

        public static double MapTileScale { get; set; }
        public static bool MapSmoothScale { get; set; }
        public static int MapTileWidth => Math.Max(1, (int)(OriginalTileWidth * MapTileScale));
        public static int MapTileHeight => Math.Max(1, (int)(OriginalTileHeight * MapTileScale));
        public static Size MapTileSize => new Size(MapTileWidth, MapTileHeight);

        public static double PreviewTileScale { get; set; }
        public static bool PreviewSmoothScale { get; set; }
        public static int PreviewTileWidth => Math.Max(1, (int)(OriginalTileWidth * PreviewTileScale));
        public static int PreviewTileHeight => (int)(OriginalTileHeight * PreviewTileScale);
        public static Size PreviewTileSize => new Size(PreviewTileWidth, PreviewTileHeight);

        public static double ExportTileScale { get; set; }
        public static bool ExportSmoothScale { get; set; }
        public static bool DisableAirUnits;
        public static bool ConvertCraters;
        public static bool BlockingBibs;
        public static bool ConvertRaObsoleteClear;
        public static bool BoundsObstructFill;
        public static bool FilterTheaterObjects;
        public static bool NoOwnedObjectsInSole;

        public static int UndoRedoStackSize = 50;

        public const int PixelWidth = 24;
        public const int PixelHeight = 24;

        public static readonly Size MapPreviewSize = new Size(512, 512);
        public static readonly Size WorkshopPreviewSize = new Size(512, 512);

        public static readonly string[] Edges = new string[] { "North", "East", "South", "West" };
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

#if DEVELOPER
        public static class Developer
        {
            public static bool ShowOverlapCells = false;
        }
#endif
    }
}
