//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
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

        public TilesetManagerClassic(IArchiveManager archiveManager)
        {
            this.archiveManager = archiveManager;
        }

        public void Reset(TheaterType theater)
        {
            foreach (Dictionary<Int32, ShapeFrameData> tiles in this.tileData.Values)
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
            this.theater = theater;
            this.currentlyLoadedPalette = TeamRemapManager.GetPaletteForTheater(this.archiveManager, theater);
        }

        public Boolean GetTeamColorTileData(String name, Int32 shape, ITeamColor teamColor, out Tile tile, Boolean generateFallback, Boolean onlyIfDefined, string remapGraphicsSource, byte[] remapTable, bool clearCachedVersion)
        {
            tile = null;
            String teamColorName = teamColor == null ? String.Empty : (teamColor.Name ?? String.Empty);
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
            if (shapeFile != null)
            {
                tile = this.RemapShapeFile(shapeFile, shape, teamColor, generateFallback, out shapeFrame);
            }
            else
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
                // Remaps the tile, and takes care of caching it and possibly generating dummies.
                tile = this.RemapShapeFile(shapeFile, shape, teamColor, generateFallback, out shapeFrame);
            }
            // shapeFrame is ALWAYS filled in if tile isn't null;
            return tile != null && !shapeFrame.IsDummy;
        }

        public Boolean GetTeamColorTileData(String name, Int32 shape, ITeamColor teamColor, out Tile tile, Boolean generateFallback, Boolean onlyIfDefined, string remapGraphicsSource, byte[] remapTable)
        {
            return GetTeamColorTileData(name, shape, teamColor, out tile, generateFallback, onlyIfDefined, remapGraphicsSource, remapTable, false);
        }

        public Boolean GetTeamColorTileData(String name, Int32 shape, ITeamColor teamColor, out Tile tile, Boolean generateFallback, Boolean onlyIfDefined)
        {
            return GetTeamColorTileData(name, shape, teamColor, out tile, generateFallback, onlyIfDefined, null, null, false);
        }

        public Boolean GetTileData(String name, Int32 shape, out Tile tile, Boolean generateFallback, Boolean onlyIfDefined)
        {
            return GetTeamColorTileData(name, shape, null, out tile, generateFallback, onlyIfDefined, null, null, false);
        }

        public Boolean GetTeamColorTileData(String name, Int32 shape, ITeamColor teamColor, out Tile tile)
        {
            return GetTeamColorTileData(name, shape, teamColor, out tile, false, false, null, null, false);
        }

        public Boolean GetTileData(String name, Int32 shape, out Tile tile)
        {
            return GetTeamColorTileData(name, shape, null, out tile, false, false, null, null, false);
        }

        public int GetTileDataLength(string name)
        {
            if (name == null)
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
            return shapes.Max(kv => kv.Key) + 1;
        }

        private Dictionary<int, ShapeFrameData> GetShapeFile(String name)
        {
            bool isShpExt = false;
            Byte[] fileContents = null;
            // If it has an extension, force it.
            if (Path.HasExtension(name))
            {
                fileContents = GetFileContents(name);
                // Immediately abort; classic file system does not support double extensions.
                if (fileContents == null)
                {
                    return null;
                }
            }
            // Try theater extension, then ".shp".
            if (fileContents == null)
            {
                fileContents = GetFileContents(name + "." + theater.ClassicExtension);
            }
            if (fileContents == null)
            {
                isShpExt = true;
                fileContents = GetFileContents(name + ".shp");
            }
            if (fileContents == null)
            {
                return null;
            }
            int[] widths = null;
            int[] heights = null;
            Byte[][] shpData = null;
            try
            {
                // TD/RA SHP file
                int width;
                int height;
                shpData = ClassicSpriteLoader.GetCcShpData(fileContents, out width, out height);
                if (shpData != null)
                {
                    int len = shpData.Length;
                    widths = Enumerable.Repeat(width, len).ToArray();
                    heights = Enumerable.Repeat(height, len).ToArray();
                }
            }
            catch (ArgumentException) { /* ignore */ }
            // Don't try to load as tmp if the filename is .shp
            if (shpData == null && !isShpExt)
            {
                try
                {
                    // TD map template tileset
                    shpData = ClassicSpriteLoader.GetCcTmpData(fileContents, out widths, out heights);
                }
                catch (ArgumentException) { /* ignore */ }
            }
            if (shpData == null && !isShpExt)
            {
                try
                {
                    // RA map template tileset
                    shpData = ClassicSpriteLoader.GetRaTmpData(fileContents, out widths, out heights, out _, out bool[] tilesUseList, out _, out _);
                    for (int i = 0; i < tilesUseList.Length; ++i)
                    {
                        if (i >= tilesUseList.Length || !tilesUseList[i])
                        {
                            shpData[i] = null;
                        }
                    }
                }
                catch (ArgumentException) { /* ignore */ }
            }
            // Only try to read Dune II SHP if it's a .shp file.
            if (shpData == null && isShpExt)
            {
                try
                {
                    // Dune II SHP
                    shpData = ClassicSpriteLoader.GetD2ShpData(fileContents, out widths, out heights);
                }
                catch (ArgumentException) { /* ignore */ }
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

        private Tile RemapShapeFile(Dictionary<Int32, ShapeFrameData> shapeFile, Int32 shape, ITeamColor teamColor, bool generateFallback, out ShapeFrameData shapeFrame)
        {
            String teamColorName = teamColor == null ? String.Empty : teamColor.Name;
            if (!shapeFile.TryGetValue(shape, out shapeFrame))
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
            }
            Color[] pal = currentlyLoadedPalette;
            if (shapeFrame.IsDummy)
            {
                // Make gray colour semitransparent on dummy graphics.
                pal = new Color[currentlyLoadedPalette.Length];
                Array.Copy(currentlyLoadedPalette, 0, pal, 0, pal.Length);
                // EGA palette grayscale colours.
                pal[12] = Color.FromArgb(0x80, pal[12]);
                pal[13] = Color.FromArgb(0x80, pal[13]);
                pal[14] = Color.FromArgb(0x80, pal[14]);
                pal[15] = Color.FromArgb(0x80, pal[15]);
            }
            Bitmap bm = ImageUtils.BuildImage(data, width, height, width, PixelFormat.Format8bppIndexed, pal, null);
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

        private Byte[] GetFileContents(String name)
        {
            Byte[] fileContents = null;
            using (Stream stream = archiveManager.OpenFile(name))
            {
                if (stream != null)
                {
                    using (BinaryReader sr = new BinaryReader(stream))
                    {
                        fileContents = sr.ReadAllBytes();
                    }
                }
            }
            return fileContents;
        }


    }
}
