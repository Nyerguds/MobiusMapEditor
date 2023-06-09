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
using MobiusEditor.Interface;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace MobiusEditor.Utility
{
    public class TilesetManager
    {
        private readonly Dictionary<string, Tileset> tilesets = new Dictionary<string, Tileset>();

        private readonly IArchiveManager megafileManager;
        private readonly TextureManager textureManager;
        private readonly string xmlPath;
        private readonly string texturesPath;

        public TilesetManager(IArchiveManager megafileManager, TextureManager textureManager, string xmlPath, string texturesPath)
        {
            this.megafileManager = megafileManager;
            this.textureManager = textureManager;
            this.xmlPath = xmlPath;
            this.texturesPath = texturesPath;
            LoadXmlfiles();
        }

        private void LoadXmlfiles()
        {
            tilesets.Clear();
            XmlDocument xmlDoc = null;
            using (Stream xmlStream = megafileManager.OpenFile(xmlPath))
            {
                if (xmlStream != null)
                {
                    xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlStream);
                }
            }
            if (xmlDoc != null)
            {
                foreach (XmlNode fileNode in xmlDoc.SelectNodes("TilesetFiles/File"))
                {
                    string xmlFile = Path.Combine(Path.GetDirectoryName(xmlPath), fileNode.InnerText);
                    XmlDocument fileXmlDoc = null;
                    using (Stream xmlStream = megafileManager.OpenFile(xmlFile))
                    {
                        if (xmlStream != null)
                        {
                            fileXmlDoc = new XmlDocument();
                            fileXmlDoc.Load(xmlStream);
                        }
                    }
                    if (fileXmlDoc != null)
                    {
                        foreach (XmlNode tilesetNode in fileXmlDoc.SelectNodes("Tilesets/TilesetTypeClass"))
                        {
                            var tileset = new Tileset(textureManager);
                            tileset.Load(tilesetNode.OuterXml, texturesPath);
                            tilesets[tilesetNode.Attributes["name"].Value] = tileset;
                        }
                    }
                }
            }
        }

        public void Reset()
        {
            foreach (var item in tilesets)
            {
                item.Value.Reset();
            }
            LoadXmlfiles();
        }

        public bool GetTeamColorTileData(IEnumerable<string> searchTilesets, string name, int shape, ITeamColor teamColor, out int fps, out Tile[] tiles, bool generateFallback, bool onlyIfDefined)
        {
            fps = 0;
            tiles = null;
            Tileset first = null;
            // Tilesets are now searched in the given order, allowing accurate defining of main tilesets and fallback tilesets.
            //foreach (var tileset in tilesets.Join(searchTilesets, x => x.Key, y => y, (x, y) => x.Value))
            foreach (string searchTileset in searchTilesets)
            {
                if (!tilesets.ContainsKey(searchTileset))
                {
                    continue;
                }
                Tileset tileset = tilesets[searchTileset];
                if (generateFallback && first == null)
                {
                    first = tileset;
                }
                if (tileset.GetTileData(name, shape, teamColor, out fps, out tiles, false))
                {
                    if (generateFallback && tiles.Any(t => t.Image == null))
                    {
                        // Tile found, but contains no data. Re-fetch with dummy generation.
                        if (tileset.GetTileData(name, shape, teamColor, out fps, out tiles, true))
                        {
                            return true;
                        }
                        continue;
                    }
                    return true;
                }
            }
            // If the tile is not defined at all, and onlyifdefined is not enabled, make a dummy entry anyway.
            if (!onlyIfDefined && generateFallback && first != null && first.GetTileData(name, shape, teamColor, out fps, out tiles, true))
            {
                return true;
            }
            return false;
        }

        public bool GetTeamColorTileData(IEnumerable<string> searchTilesets, string name, int shape, ITeamColor teamColor, out int fps, out Tile tile, bool generateFallback, bool onlyIfDefined)
        {
            tile = null;
            if (!GetTeamColorTileData(searchTilesets, name, shape, teamColor, out fps, out Tile[] tiles, generateFallback, onlyIfDefined))
            {
                return false;
            }
            tile = tiles[0];
            return true;
        }

        public bool GetTeamColorTileData(IEnumerable<string> searchTilesets, string name, int shape, ITeamColor teamColor, out Tile[] tiles, bool generateFallback, bool onlyIfDefined)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, teamColor, out int fps, out tiles, generateFallback, onlyIfDefined);
        }

        public bool GetTeamColorTileData(IEnumerable<string> searchTilesets, string name, int shape, ITeamColor teamColor, out Tile[] tiles)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, teamColor, out int fps, out tiles, false, false);
        }

        public bool GetTeamColorTileData(IEnumerable<string> searchTilesets, string name, int shape, ITeamColor teamColor, out Tile tile, bool generateFallback, bool onlyIfDefined)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, teamColor, out int fps, out tile, generateFallback, onlyIfDefined);
        }

        public bool GetTeamColorTileData(IEnumerable<string> searchTilesets, string name, int shape, ITeamColor teamColor, out Tile tile)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, teamColor, out int fps, out tile, false, false);
        }

        public bool GetTileData(IEnumerable<string> searchTilesets, string name, int shape, out int fps, out Tile[] tiles, bool generateFallback, bool onlyIfDefined)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, null, out fps, out tiles, generateFallback, onlyIfDefined);
        }

        public bool GetTileData(IEnumerable<string> searchTilesets, string name, int shape, out int fps, out Tile[] tiles)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, null, out fps, out tiles, false, false);
        }

        public bool GetTileData(IEnumerable<string> searchTilesets, string name, int shape, out int fps, out Tile tile, bool generateFallback, bool onlyIfDefined)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, null, out fps, out tile, generateFallback, onlyIfDefined);
        }

        public bool GetTileData(IEnumerable<string> searchTilesets, string name, int shape, out int fps, out Tile tile)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, null, out fps, out tile, false, false);
        }

        public bool GetTileData(IEnumerable<string> searchTilesets, string name, int shape, out Tile[] tiles, bool generateFallback, bool onlyIfDefined)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, null, out tiles, generateFallback, onlyIfDefined);
        }

        public bool GetTileData(IEnumerable<string> searchTilesets, string name, int shape, out Tile[] tiles)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, null, out tiles, false, false);
        }

        public bool GetTileData(IEnumerable<string> searchTilesets, string name, int shape, out Tile tile, bool generateFallback, bool onlyIfDefined)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, null, out tile, generateFallback, onlyIfDefined);
        }

        public bool GetTileData(IEnumerable<string> searchTilesets, string name, int shape, out Tile tile)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, null, out tile, false, false);
        }

        public int GetTileDataLength(IEnumerable<string> searchTilesets, string name)
        {
            foreach (var tileset in tilesets.Join(searchTilesets, x => x.Key, y => y, (x, y) => x.Value))
            {
                int frames = tileset.GetTileDataLength(name);
                if (frames != -1)
                {
                    return frames;
                }
            }
            return -1;
        }
    }
}
