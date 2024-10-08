﻿//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                     Version 2, December 2004
//
//  Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
//  Everyone is permitted to copy and distribute verbatim or modified
//  copies of this license document, and changing it is allowed as long
//  as the name is changed.
//
//             DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//    TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//   0. You just DO WHAT THE FUCK YOU WANT TO.

// When enabled, this shows all failed load attempts that occur in the GetShapeFile function.
//#define WriteFileLoadDebug

using MobiusEditor.Interface;
using MobiusEditor.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace MobiusEditor.Utility
{
    /// <summary>
    /// TilesetManagerClassic: contains a list of Shapes, stored as Dictionary by Shape name.
    /// Shape: Dictionary of frame numbers and the TileData for each frame number
    /// ShapeFrameData: Contains the width, height and raw unremapped frame data.
    ///        Also contains a dictionary with the actually-cached Tiles using the team color name as key.
    /// Tile: The cached remapped image, plus its transparent bounds.
    /// </summary>
    public class TilesetManagerClassic: ITilesetManager
    {
        // TileSet: Dictionary<string,shape>
        // shape: Dictionary<int,ShapeData> // full SHP file
        // ShapeData: filename + mapping of team colors to remapped Tiles

        private class ShapeFrameData
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public byte[] FrameData { get; set; }
            public bool IsDummy { get; set; }
            public Dictionary<string, Tile> TeamColorTiles { get; } = new Dictionary<string, Tile>();
        }

        private Dictionary<string, Dictionary<int, ShapeFrameData>> tileData = new Dictionary<string, Dictionary<int, ShapeFrameData>>();
        private IArchiveManager archiveManager;
        private TheaterType theater;
        private Color[] currentlyLoadedPalette;
        private Color[] currentlyLoadedPaletteBare;

        public Color[] CurrentlyLoadedPalette => currentlyLoadedPalette.ToArray();
        public Color[] CurrentlyLoadedPaletteBare => currentlyLoadedPaletteBare.ToArray();

        public TilesetManagerClassic(IArchiveManager archiveManager)
        {
            this.archiveManager = archiveManager;
            this.Reset(GameType.None, null);
        }

        public void Reset(GameType gameType, TheaterType theater)
        {
            foreach (Dictionary<int, ShapeFrameData> tiles in this.tileData.Values)
            {
                foreach (ShapeFrameData shapeFrame in tiles.Values)
                {
                    foreach (Tile tile in shapeFrame.TeamColorTiles.Values)
                    {
                        tile.Dispose();
                    }
                    shapeFrame.TeamColorTiles.Clear();
                }
                tiles.Clear();
            }
            this.tileData.Clear();
            this.theater = null;
            if (gameType != GameType.None && theater != null && (this.archiveManager.CurrentGameType != gameType || this.archiveManager.CurrentTheater != theater))
            {
                throw new InvalidOperationException("The archive manager is not reset to the given game; cannot load the correct files.");
            }
            this.theater = theater;
            Color[] pal = TeamRemapManager.GetPaletteForTheater(this.archiveManager, theater);
            int palLength = 0x100;
            this.currentlyLoadedPaletteBare = new Color[0x100];
            this.currentlyLoadedPalette = new Color[0x100];
            for (int i = 0; i < pal.Length && i < palLength; ++i)
            {
                this.currentlyLoadedPalette[i] = pal[i];
                this.currentlyLoadedPaletteBare[i] = pal[i];
            }
            ApplySpecialColors(this.currentlyLoadedPalette, true, true);
            ApplySpecialColors(this.currentlyLoadedPaletteBare, true, false);
        }

        protected void ApplySpecialColors(Color[] colors, bool adjustTrans, bool adjustShadow)
        {
            if (adjustTrans)
            {
                // Set background transparent
                colors[0] = Color.FromArgb(0x00, colors[0]);
            }
            if (adjustShadow)
            {
                // Set shadow color to semitransparent black. Classic fading table remapping is impossible for this since the editor's main bitmap is high color.
                colors[4] = Color.FromArgb(0x80, Color.Black);
            }
        }

        /// <summary>
        /// Finds the nearest color on the palette matching the given color.
        /// </summary>
        /// <param name="color">Color to find</param>
        /// <param name="includeShadowFilterColor">Use the palette where the shadow filter color is not adjusted to semitransparent black.</param>
        /// <returns>The index on the palette of the color that most closely matches the requested color.</returns>
        public int GetClosestColorIndex(Color color, bool includeShadowFilterColor)
        {
            return GetClosestColorIndex(color, includeShadowFilterColor, out _);
        }

        /// <summary>
        /// Finds the nearest color on the palette matching the given color.
        /// </summary>
        /// <param name="color">Color to find</param>
        /// <param name="includeShadowFilterColor">Use the palette where the shadow filter color is not adjusted to semitransparent black.</param>
        /// <param name="actualColor">Output parameter giving the actual retrieved color.</param>
        /// <returns>The index on the palette of the color that most closely matches the requested color.</returns>
        public int GetClosestColorIndex(Color color, bool includeShadowFilterColor, out Color actualColor)
        {
            if (currentlyLoadedPalette == null)
            {
                actualColor = Color.Transparent;
                return 0;
            }
            Color[] palette = includeShadowFilterColor ? this.currentlyLoadedPaletteBare : this.currentlyLoadedPalette;
            int index = ImageUtils.GetClosestPaletteIndexMatch(color, palette, 0.Yield());
            actualColor = palette[index];
            return index;
        }

        public bool GetTeamColorTileData(string name, int shape, ITeamColor teamColor, out Tile tile, bool generateFallback, bool onlyIfDefined, bool withShadow, string remapGraphicsSource, byte[] remapTable, bool clearCachedVersion)
        {
            tile = null;
            string teamColorName = teamColor == null ? String.Empty : (teamColor.Name ?? String.Empty);
            Dictionary<int, ShapeFrameData> shapeFile;
            ShapeFrameData shapeFrame;
            bool cached = tileData.TryGetValue(name, out shapeFile);
            // Deliberately fetch clean version
            if (cached && clearCachedVersion)
            {
                // Dispose all images.
                foreach (ShapeFrameData shpFrame in shapeFile.Values)
                {
                    foreach (Tile tl in shpFrame.TeamColorTiles.Values)
                    {
                        tl.Dispose();
                    }
                    shpFrame.TeamColorTiles.Clear();
                }
                shapeFile.Clear();
                shapeFile = null;
                tileData.Remove(name);
                cached = false;
            }

            if (cached
                && shapeFile.TryGetValue(shape, out shapeFrame)
                && shapeFrame.TeamColorTiles.TryGetValue(teamColorName, out tile))
            {
                return !shapeFrame.IsDummy;
            }
            if (shapeFile == null)
            {
                // If there's a remap graphics source, prefer that.
                shapeFile = this.GetShapeFile(remapGraphicsSource ?? name);
                if (shapeFile == null)
                {
                    if (!generateFallback)
                    {
                        return false;
                    }
                    shapeFile = new Dictionary<int, ShapeFrameData>();
                }
                tileData[name] = shapeFile;
                // System to fix RA's remapped infantry. Since everything is cached, this only works if the very
                // first call to fetch these graphics is guaranteed to pass along the graphics source and remap info.
                // To ensure correct behaviour, "name" should be given a unique string that doesn't match any real graphics.
                if (remapTable != null && remapTable.Length >= 0x100)
                {
                    foreach (int key in shapeFile.Keys)
                    {
                        ShapeFrameData sfd = shapeFile[key];
                        Byte[] frameGfx = sfd.FrameData;
                        for (int i = 0; i < frameGfx.Length; ++i)
                        {
                            frameGfx[i] = remapTable[frameGfx[i]];
                        }
                    }
                }
            }
            // Remaps the tile, and takes care of caching it and possibly generating dummies.
            tile = this.RemapShapeFile(shapeFile, shape, teamColor, generateFallback, withShadow, out shapeFrame);
            // shapeFrame is ALWAYS filled in if tile isn't null;
            return tile != null && !shapeFrame.IsDummy;
        }

        public bool GetTeamColorTileData(string name, int shape, ITeamColor teamColor, out Tile tile, bool generateFallback, bool onlyIfDefined, string remapGraphicsSource, byte[] remapTable)
        {
            return GetTeamColorTileData(name, shape, teamColor, out tile, generateFallback, onlyIfDefined, true, remapGraphicsSource, remapTable, false);
        }

        public bool GetTeamColorTileData(string name, int shape, ITeamColor teamColor, out Tile tile, bool generateFallback, bool onlyIfDefined)
        {
            return GetTeamColorTileData(name, shape, teamColor, out tile, generateFallback, onlyIfDefined, true, null, null, false);
        }

        public bool GetTileData(string name, int shape, out Tile tile, bool generateFallback, bool onlyIfDefined)
        {
            return GetTeamColorTileData(name, shape, null, out tile, generateFallback, onlyIfDefined, true, null, null, false);
        }

        public bool GetTeamColorTileData(string name, int shape, ITeamColor teamColor, out Tile tile)
        {
            return GetTeamColorTileData(name, shape, teamColor, out tile, false, false, true, null, null, false);
        }

        public bool GetTeamColorTileData(string name, int shape, ITeamColor teamColor, bool ignoreShadow, out Tile tile)
        {
            return GetTeamColorTileData(name, shape, teamColor, out tile, false, false, !ignoreShadow, null, null, false);
        }

        public bool GetTileData(string name, int shape, out Tile tile)
        {
            return GetTeamColorTileData(name, shape, null, out tile, false, false, true, null, null, false);
        }

        public int GetTileDataLength(string name)
        {
            if (name == null || this.theater == null)
            {
                return -1;
            }
            if (!this.tileData.TryGetValue(name, out Dictionary<int, ShapeFrameData> shapes))
            {
                // If it's not cached yet, fetch without caching. This avoids issues with the special remap system.
                // These ShapeFrameData objects don't need to be disposed since they can't contain Bitmap objects yet.
                shapes = GetShapeFile(name);
                if (shapes == null)
                {
                    return -1;
                }
            }
            if (shapes.Where(kv => !kv.Value.IsDummy).Count() == 0)
            {
                return -1;
            }
            return shapes.Where(kv => !kv.Value.IsDummy).Max(kv => kv.Key) + 1;
        }

        public bool TileExists(string name)
        {
            return GetTileDataLength(name) > 0;
        }

        public Bitmap GetTile(string remasterSprite, int remastericon, string classicSprite, int classicicon, ITeamColor teamColor)
        {
            bool found = GetTeamColorTileData(classicSprite, classicicon, teamColor, out Tile tile);
            return found && tile != null && tile.Image != null ? new Bitmap(tile.Image) : null;
        }

        public Bitmap GetTexture(string remasterTexturePath, string classicSprite, int classicicon, bool ignoreClassicShadow)
        {
            bool found = GetTeamColorTileData(classicSprite, classicicon, null, ignoreClassicShadow, out Tile tile);
            return found && tile != null && tile.Image != null ? new Bitmap(tile.Image) : null;
        }

        private Dictionary<int, ShapeFrameData> GetShapeFile(string name)
        {
            bool isShpExt = false;
            bool isFntExt = false;
            Byte[] fileContents = null;
            // If it has an extension, force it.
            if (Path.HasExtension(name))
            {
                fileContents = archiveManager.ReadFile(name);
                // Immediately abort; classic file system does not support double extensions.
                if (fileContents == null)
                {
                    return null;
                }
                string ext = Path.GetExtension(name);
                isShpExt = ".shp".Equals(ext, StringComparison.OrdinalIgnoreCase);
                isFntExt = ".fnt".Equals(ext, StringComparison.OrdinalIgnoreCase);

            }
            // Try theater extension, then ".shp", then ".fnt".
            if (fileContents == null)
            {
                fileContents = archiveManager.ReadFile(name + "." + theater.ClassicExtension);
            }
            if (fileContents == null)
            {
                fileContents = archiveManager.ReadFile(name + ".shp");
                isShpExt = fileContents != null;
            }
            if (fileContents == null)
            {
                fileContents = archiveManager.ReadFile(name + ".fnt");
                isFntExt = fileContents != null;
            }
            if (fileContents == null)
            {
                return null;
            }
#if DEBUG && WriteFileLoadDebug
            bool throwWhenParsing = true;
#else
            bool throwWhenParsing = false;
#endif
            int[] widths = null;
            int[] heights = null;
            Byte[][] shpData = null;
            if (!isFntExt)
            {
                try
                {
                    // TD/RA SHP file
                    int width;
                    int height;
                    shpData = ClassicSpriteLoader.GetCcShpData(fileContents, out width, out height, throwWhenParsing);
                    if (shpData != null)
                    {
                        int len = shpData.Length;
                        widths = Enumerable.Repeat(width, len).ToArray();
                        heights = Enumerable.Repeat(height, len).ToArray();
                    }
                }
                catch
#if DEBUG && WriteFileLoadDebug
                    (FileTypeLoadException e)
#endif
                {
                    /* ignore */
#if DEBUG && WriteFileLoadDebug
                    System.Diagnostics.Debug.WriteLine("Failed to load file {0} as {1}: {2}", name, e.AttemptedLoadedType, e.Message);
#endif
                }
            }
            // Don't try to load as tmp if the filename is .shp
            if (shpData == null && !isShpExt && !isFntExt)
            {
                try
                {
                    // TD map template tileset
                    shpData = ClassicSpriteLoader.GetCcTmpData(fileContents, out widths, out heights, throwWhenParsing);
                }
                catch
#if DEBUG && WriteFileLoadDebug
                    (FileTypeLoadException e)
#endif
                {
                    /* ignore */
#if DEBUG && WriteFileLoadDebug
                    System.Diagnostics.Debug.WriteLine("Failed to load file {0} as {1}: {2}", name, e.AttemptedLoadedType, e.Message);
#endif
                }

            }
            if (shpData == null && !isShpExt && !isFntExt)
            {
                try
                {
                    // RA map template tileset
                    shpData = ClassicSpriteLoader.GetRaTmpData(fileContents, out widths, out heights, out _, out bool[] tilesUseList, out _, out _, throwWhenParsing);
                    if (shpData != null)
                    {
                        for (int i = 0; i < tilesUseList.Length; ++i)
                        {
                            if (i >= tilesUseList.Length || !tilesUseList[i])
                            {
                                shpData[i] = null;
                            }
                        }
                    }
                }
                catch
#if DEBUG && WriteFileLoadDebug
                    (FileTypeLoadException e)
#endif
                {
                    /* ignore */
#if DEBUG && WriteFileLoadDebug
                    System.Diagnostics.Debug.WriteLine("Failed to load file {0} as {1}: {2}", name, e.AttemptedLoadedType, e.Message);
#endif
                }
            }
            // Only try to read Dune II SHP if it's a .shp file.
            if (shpData == null && isShpExt)
            {
                try
                {
                    // Dune II SHP
                    shpData = ClassicSpriteLoader.GetD2ShpData(fileContents, out widths, out heights, throwWhenParsing);
                }
                catch
#if DEBUG && WriteFileLoadDebug
                    (FileTypeLoadException e)
#endif
                {
                    /* ignore */
#if DEBUG && WriteFileLoadDebug
                    System.Diagnostics.Debug.WriteLine("Failed to load file {0} as {1}: {2}", name, e.AttemptedLoadedType, e.Message);
#endif
                }
            }
            // Only try to read font file if it's either explicitly requested as .fnt file, or found as .fnt file.
            if (shpData == null && isFntExt)
            {
                try
                {
                    // Font file
                    int height;
                    shpData = ClassicSpriteLoader.GetCCFontData(fileContents, out widths, out height, throwWhenParsing);
                    if (shpData != null)
                    {
                        int len = shpData.Length;
                        heights = Enumerable.Repeat(height, len).ToArray();
                    }
                }
                catch
#if DEBUG && WriteFileLoadDebug
                    (FileTypeLoadException e)
#endif
                {
                    /* ignore */
#if DEBUG && WriteFileLoadDebug
                    System.Diagnostics.Debug.WriteLine("Failed to load file {0} as {1}: {2}", name, e.AttemptedLoadedType, e.Message);
#endif
                }
            }
            if (shpData == null || shpData.Length == 0)
            {
                return null;
            }
            // Finally, we got our frames; get the data.
            Dictionary<int, ShapeFrameData> shapeFile = new Dictionary<int, ShapeFrameData>();
            int frames = shpData.Length;
            for (int i = 0; i < frames; i++)
            {
                byte[] frameData = shpData[i];
                int width = widths[i];
                int height = heights[i];
                // Allows excluding frames from template tilesets
                if (frameData == null || width == 0 || height == 0)
                {
                    continue;
                }
                ShapeFrameData frData = new ShapeFrameData();
                frData.FrameData = frameData;
                frData.Width = width;
                frData.Height = height;
                // Don't add any remapping yet; it'll be done automatically afterwards anyway. Since the data
                // is saved as bytes, it's perfectly possible that no non-remapped version is cached as image.
                shapeFile.Add(i, frData);
            }
            return shapeFile;
        }

        private Tile RemapShapeFile(Dictionary<int, ShapeFrameData> shapeFile, int shape, ITeamColor teamColor, bool generateFallback, bool withShadow, out ShapeFrameData shapeFrame)
        {
            string teamColorName = teamColor == null ? String.Empty : teamColor.Name;
            if (!withShadow)
            {
                teamColorName += " no-shadow";
            }
            if (!shapeFile.TryGetValue(shape, out shapeFrame)
                || shapeFrame.FrameData == null || shapeFrame.FrameData.Length == 0
                || shapeFrame.Width == 0 || shapeFrame.Height == 0)
            {
                if (!generateFallback)
                {
                    return null;
                }
                shapeFrame = GenerateDummy();
                shapeFile[shape] = shapeFrame;
            }
            Tile tile;
            if (shapeFrame.TeamColorTiles.TryGetValue(teamColorName, out tile))
            {
                return tile;
            }
            int width = shapeFrame.Width;
            int height = shapeFrame.Height;
            byte[] data = shapeFrame.FrameData;
            Rectangle opaqueBounds = ImageUtils.CalculateOpaqueBounds8bpp(data, width, height, width, 0);
            if (teamColor != null && !String.IsNullOrEmpty(teamColorName) && !shapeFrame.IsDummy)
            {
                // Finally, the actual remapping!
                byte[] dataRemap = new byte[data.Length];
                Array.Copy(data, 0, dataRemap, 0, data.Length);
                teamColor.ApplyToImage(dataRemap, width, height, 1, width, opaqueBounds);
                data = dataRemap;
                // If opaque bounds might have changed due to remapping, recalculate bounds.
                if (teamColor.RemapTable != null && teamColor.RemapTable.Length > 1)
                {
                    int maxIndex = Math.Min(teamColor.RemapTable.Length, data.Max());
                    // To include index "1", we need 1 more item after skipping 0. So maxIndex needs to be used without adjustment.
                    if (teamColor.RemapTable.Skip(1).Take(maxIndex).Any(i => i == 0))
                    {
                        opaqueBounds = ImageUtils.CalculateOpaqueBounds8bpp(data, width, height, width, 0);
                    }
                }
            }
            Color[] pal = withShadow ? currentlyLoadedPalette : currentlyLoadedPaletteBare;
            if (shapeFrame.IsDummy)
            {
                // Make gray colors semitransparent on dummy graphics.
                pal = new Color[currentlyLoadedPalette.Length];
                Array.Copy(currentlyLoadedPalette, 0, pal, 0, pal.Length);
                // EGA palette grayscale colors.
                pal[12] = Color.FromArgb(0x80, pal[12]);
                pal[13] = Color.FromArgb(0x80, pal[13]);
                pal[14] = Color.FromArgb(0x80, pal[14]);
                pal[15] = Color.FromArgb(0x80, pal[15]);
            }
            Bitmap bm;
            using (Bitmap bm8 = ImageUtils.BuildImage(data, width, height, width, PixelFormat.Format8bppIndexed, pal, null))
            {
                // Convert to 32-bit, to avoid weird artifacts when using ColorMatrix on it.
                bm = new Bitmap(bm8);
            }
            tile = new Tile(bm, opaqueBounds);
            shapeFrame.TeamColorTiles.Add(teamColorName, tile);
            return tile;
        }

        private ShapeFrameData GenerateDummy()
        {
            // Dummy generation
            int width = 24;
            int height = 24;
            int length = width * height;
            byte[] dummyData = Enumerable.Repeat<Byte>(14, length).ToArray();
            // Nevermind the internal border if it's too small.
            if (width > 12 || height > 12)
            {
                int offsGrX = width / 12;
                int offsGrY = height / 12;
                int smallGrW = width - offsGrX * 2;
                int smallGrH = height - offsGrY * 2;
                int smallGrLen = smallGrW * smallGrH;
                byte[] dataSmallGr = Enumerable.Repeat<Byte>(13, smallGrLen).ToArray();
                ImageUtils.PasteOn8bpp(dummyData, width, height, width, dataSmallGr, smallGrW, smallGrH, smallGrW, new Rectangle(offsGrX, offsGrY, smallGrW, smallGrH), null, true);
            }
            int offsWhX =  width / 6;
            int offsWhY =  height / 6;
            int smallWhW = width - offsWhX * 2;
            int smallWhH = height - offsWhY * 2;
            int smallWhLen = smallWhW * smallWhH;
            byte[] dataSmallWh = Enumerable.Repeat<Byte>(15, smallWhLen).ToArray();
            ImageUtils.PasteOn8bpp(dummyData, width, height, width, dataSmallWh, smallWhW, smallWhH, smallWhW, new Rectangle(offsWhX, offsWhY, smallWhW, smallWhH), null, true);
            // Fill frame daya object.
            ShapeFrameData frameData = new ShapeFrameData();
            frameData.Width = width;
            frameData.Height = height;
            frameData.FrameData = dummyData;
            frameData.IsDummy = true;
            return frameData;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Reset(GameType.None, null);
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
