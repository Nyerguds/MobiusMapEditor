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

        public Boolean GetTeamColorTileData(String name, Int32 shape, ITeamColor teamColor, out Tile tile, Boolean generateFallback, Boolean onlyIfDefined)
        {
            tile = null;
            String teamColorName = teamColor == null ? String.Empty : (teamColor.Name ?? String.Empty);
            Dictionary<int, ShapeFrameData> shapeFile;
            ShapeFrameData shapeFrame;
            if (tileData.TryGetValue(name, out shapeFile)
                && shapeFile.TryGetValue(shape, out shapeFrame)
                && shapeFrame.TeamColorTiles.TryGetValue(teamColorName, out tile))
            {
                return true;
            }
            if (shapeFile != null)
            {
                tile = this.RemapShapeFile(shapeFile, shape, teamColor, generateFallback);
            }
            else
            {
                shapeFile = this.GetShapeFile(name);
                if (shapeFile == null)
                {
                    if (!generateFallback)
                    {
                        return false;
                    }
                    shapeFile = new Dictionary<int, ShapeFrameData>();
                }
                tileData[name] = shapeFile;
                // Remaps the tile, and takes care of caching it and possibly generating dummies.
                tile = this.RemapShapeFile(shapeFile, shape, teamColor, generateFallback);
            }
            return tile != null;
        }

        public Boolean GetTeamColorTileData(String name, Int32 shape, ITeamColor teamColor, out Tile tile)
        {
            return GetTeamColorTileData(name, shape, teamColor, out tile, false, false);
        }

        public Boolean GetTileData(String name, Int32 shape, out Tile tile, Boolean generateFallback, Boolean onlyIfDefined)
        {
            return GetTeamColorTileData(name, shape, null, out tile, false, false);
        }

        public Boolean GetTileData(String name, Int32 shape, out Tile tile)
        {
            return GetTeamColorTileData(name, shape, null, out tile, false, false);
        }

        public int GetTileDataLength(string name)
        {
            if (name == null)
            {
                return -1;
            }
            if (!this.tileData.TryGetValue(name, out Dictionary<int, ShapeFrameData> shapes))
            {
                return -1;
            }
            return shapes.Max(kv => kv.Key) + 1;
        }

        private Dictionary<int, ShapeFrameData> GetShapeFile(String name)
        {
            bool isShpExt = false;
            // Try theater extension, then ".shp".
            Byte[] fileContents = GetFileContents(name + "." + theater.ClassicExtension);
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
                    int width;
                    int height;
                    shpData = ClassicSpriteLoader.GetRaTmpData(fileContents, out width, out height);
                    if (shpData != null)
                    {
                        int len = shpData.Length;
                        widths = Enumerable.Repeat(width, len).ToArray();
                        heights = Enumerable.Repeat(height, len).ToArray();
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

        private Tile RemapShapeFile(Dictionary<Int32, ShapeFrameData> shapeFile, Int32 shape, ITeamColor teamColor, bool generateFallback)
        {
            String teamColorName = teamColor == null ? String.Empty : teamColor.Name;
            if (!shapeFile.TryGetValue(shape, out ShapeFrameData frameData))
            {
                if (!generateFallback)
                {
                    return null;
                }
                // Make average-sized dummy.
                int minWidth = shapeFile.Values.Where(v => v.Width != 0).Min(v => v.Width);
                int maxWidth = shapeFile.Values.Where(v => v.Width != 0).Max(v => v.Width);
                int minHeight = shapeFile.Values.Where(v => v.Height != 0).Min(v => v.Height);
                int maxHeight = shapeFile.Values.Where(v => v.Height != 0).Max(v => v.Height);
                int dummyWidth = minWidth + (maxWidth - minWidth) / 2;
                int dummyHeight = minHeight + (maxHeight - minHeight) / 2;
                frameData = GenerateDummy(dummyWidth, dummyHeight);
                shapeFile[shape] = frameData;
            }
            Tile tile;
            if (frameData.TeamColorTiles.TryGetValue(teamColorName, out tile))
            {
                return tile;
            }
            int width = frameData.Width;
            int height = frameData.Height;
            byte[] data = frameData.FrameData;
            Rectangle opaqueBounds = ImageUtils.CalculateOpaqueBounds8bpp(data, width, height, width, 0);
            if (teamColor != null && !String.IsNullOrEmpty(teamColorName) && !frameData.IsDummy)
            {
                // Finally, the actual remapping!
                teamColor.ApplyToImage(data, width, height, 1, width, opaqueBounds);
            }
            Bitmap bm = ImageUtils.BuildImage(data, width, height, width, PixelFormat.Format8bppIndexed, currentlyLoadedPalette, null);
            tile = new Tile(bm, opaqueBounds);
            frameData.TeamColorTiles.Add(teamColorName, tile);
            return tile;
        }

        private ShapeFrameData GenerateDummy(int width, int height)
        {
            // Dummy generation
            int length = width * height;
            byte[] dummyData = Enumerable.Repeat<Byte>(14, length).ToArray();
            if (width > 2 && height > 3)
            {
                int offsX = Math.Max(width / 6, 1);
                int offsY = Math.Max(height / 6, 1);
                int smallW = Math.Max(width - offsX * 2, 1);
                int smallH = Math.Max(height - offsY * 2, 1);
                int smallLen = smallW * smallH;
                byte[] dataSmall = Enumerable.Repeat<Byte>(15, smallLen).ToArray();
                ImageUtils.PasteOn8bpp(dummyData, width, height, width, dataSmall, smallW, smallH, smallW, new Rectangle(offsX, offsY, smallW, smallH), null, true);
            }
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
