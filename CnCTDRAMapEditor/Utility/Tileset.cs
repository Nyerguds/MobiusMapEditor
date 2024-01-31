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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Xml;

namespace MobiusEditor.Utility
{
    public class Tileset
    {
        private static string DummyFormatTga = "DATA\\ART\\TEXTURES\\SRGB\\FALLBACK_DUMMY\\{0}_{1:D4}.tga";

        private class TileData
        {
            public int FPS { get; set; }
            public string[] Frames { get; set; }
            public bool IsDummy { get; set; }

            public Dictionary<string, Tile[]> TeamColorTiles { get; } = new Dictionary<string, Tile[]>();
        }

        private readonly Dictionary<string, Dictionary<int, TileData>> tiles = new Dictionary<string, Dictionary<int, TileData>>(StringComparer.OrdinalIgnoreCase);

        private readonly TextureManager textureManager;

        private static readonly Bitmap transparentTileImage;

        static Tileset()
        {
            transparentTileImage = new Bitmap(Globals.OriginalTileWidth, Globals.OriginalTileHeight);
            transparentTileImage.SetResolution(96, 96);
            transparentTileImage.MakeTransparent();
        }

        public Tileset(TextureManager textureManager)
        {
            this.textureManager = textureManager;
        }

        public void Reset()
        {
            foreach (var item in tiles)
            {
                foreach (var tileItem in item.Value)
                {
                    // TextureManager returns clones, so these need to be cleaned up explicitly.
                    foreach (KeyValuePair<string, Tile[]> tileinfo in tileItem.Value.TeamColorTiles)
                    {
                        if (tileinfo.Value != null) {
                            foreach (Tile tile in tileinfo.Value)
                            {
                                // clean up bitmap
                                tile.Dispose();
                            }
                        }
                    }
                    tileItem.Value.TeamColorTiles.Clear();
                }
            }
        }

        public void Load(string xml, string texturesPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            var rootPath = Path.Combine(texturesPath, xmlDoc.SelectSingleNode("TilesetTypeClass/RootTexturePath").InnerText);
            foreach (XmlNode tileNode in xmlDoc.SelectNodes("TilesetTypeClass/Tiles/Tile"))
            {
                TileData tileData = new TileData();

                var name = tileNode.SelectSingleNode("Key/Name").InnerText;
                var shape = int.Parse(tileNode.SelectSingleNode("Key/Shape").InnerText);
                var fpsNode = tileNode.SelectSingleNode("Value/AnimationData/FPS");
                tileData.FPS = (fpsNode != null) ? int.Parse(fpsNode.InnerText) : 0;
                var frameNodes = tileNode.SelectNodes("Value/Frames/Frame");
                tileData.Frames = new string[Math.Min(1, frameNodes.Count)];
                for (var i = 0; i < tileData.Frames.Length; ++i)
                {
                    string filename = null;
                    if (!string.IsNullOrEmpty(frameNodes[i].InnerText))
                    {
                        filename = Path.Combine(rootPath, frameNodes[i].InnerText);
                    }
                    tileData.Frames[i] = filename;
                }
                if (!tiles.TryGetValue(name, out Dictionary<int, TileData> shapes))
                {
                    shapes = new Dictionary<int, TileData>();
                    tiles[name] = shapes;
                }
                shapes[shape] = tileData;
            }
        }

        public bool GetTileData(string name, int shape, ITeamColor teamColor, out int fps, out Tile[] tiles, out bool isDummy, bool generateFallback)
        {
            fps = 0;
            tiles = null;
            isDummy = false;
            if (name == null)
            {
                return false;
            }
            Dictionary<int, TileData> shapes;
            if (!this.tiles.TryGetValue(name, out shapes) && !generateFallback)
            {
                return false;
            }
            name = name.ToUpperInvariant();
            if (shapes == null && generateFallback)
            {
                if (shape < 0)
                {
                    shape = 0;
                }
                string dummy = String.Format(DummyFormatTga, name, shape);
                shapes = new Dictionary<int, TileData>();
                TileData dummyData = new TileData();
                dummyData.Frames = new string[] { dummy };
                dummyData.IsDummy = true;
                shapes.Add(shape, dummyData);
                // Add it, so it's present for the next lookup.
                this.tiles[name] = shapes;
                isDummy = true;
            }
            if (shape < 0)
            {
                shape = Math.Max(0, shapes.Max(kv => kv.Key) + shape + 1);
            }
            if (!shapes.TryGetValue(shape, out TileData tileData))
            {
                if (generateFallback)
                {
                    // Shape was found, but specific frame was not. Add it.
                    string dummy = String.Format(DummyFormatTga, name, shape);
                    tileData = new TileData();
                    tileData.Frames = new string[] { dummy };
                    tileData.IsDummy = true;
                    shapes.Add(shape, tileData);
                    isDummy = true;
                }
                else
                {
                    return false;
                }
            }
            var key = teamColor?.Name ?? string.Empty;
            bool needsdummy = false;
            if (!tileData.TeamColorTiles.TryGetValue(key, out Tile[] tileDataTiles) || (needsdummy = generateFallback && tileDataTiles.Length == 0 || tileDataTiles.Any(t => t.Image == null)))
            {
                tileDataTiles = new Tile[tileData.Frames.Length];
                tileData.TeamColorTiles[key] = tileDataTiles;

                for (int i = 0; i < tileDataTiles.Length; ++i)
                {
                    var filename = tileData.Frames[i];
                    if (!string.IsNullOrEmpty(filename))
                    {
                        (Bitmap bitmap, Rectangle opaqueBounds) = textureManager.GetTexture(filename, teamColor, isDummy || needsdummy);
                        tileDataTiles[i] = new Tile(bitmap, opaqueBounds);
                    }
                    else
                    {
                        tileDataTiles[i] = new Tile(transparentTileImage);
                    }
                }
            }
            fps = tileData.FPS;
            tiles = tileDataTiles;
            isDummy = tileData.IsDummy;
            return true;
        }

        public int GetTileDataLength(string name)
        {
            if (name == null)
            {
                return -1;
            }
            if (!this.tiles.TryGetValue(name, out Dictionary<int, TileData> shapes) || shapes.Where(kv => !kv.Value.IsDummy).Count() == 0)
            {
                return -1;
            }
            return shapes.Where(kv => !kv.Value.IsDummy).Max(kv => kv.Key) + 1;
        }

    }
}
