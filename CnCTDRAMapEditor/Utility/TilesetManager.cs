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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace MobiusEditor.Utility
{
    public class TilesetManager
    {
        private readonly Dictionary<string, Tileset> tilesets = new Dictionary<string, Tileset>();

        private readonly MegafileManager megafileManager;
        private string expandModPath = null;

        public TilesetManager(MegafileManager megafileManager, TextureManager textureManager, string xmlPath, string texturesPath, string expandModPath)
        {
            this.megafileManager = megafileManager;
            this.expandModPath = expandModPath;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(megafileManager.Open(xmlPath));

            foreach (XmlNode fileNode in xmlDoc.SelectNodes("TilesetFiles/File"))
            {

                string xmlFile = Path.Combine(Path.GetDirectoryName(xmlPath), fileNode.InnerText);
                XmlDocument fileXmlDoc = new XmlDocument();
                string modXmlPath = expandModPath == null ? null : Path.Combine(expandModPath, xmlFile);
                if (modXmlPath != null && File.Exists(modXmlPath))
                {
                    fileXmlDoc.Load(modXmlPath);
                }
                else
                {
                    fileXmlDoc.Load(megafileManager.Open(xmlFile));
                }
                foreach (XmlNode tilesetNode in fileXmlDoc.SelectNodes("Tilesets/TilesetTypeClass"))
                {
                    var tileset = new Tileset(textureManager);
                    tileset.Load(tilesetNode.OuterXml, texturesPath);

                    tilesets[tilesetNode.Attributes["name"].Value] = tileset;
                }
            }
        }

        public void Reset()
        {
            foreach (var item in tilesets)
            {
                item.Value.Reset();
            }
        }

        public bool GetTeamColorTileData(IEnumerable<string> searchTilesets, string name, int shape, TeamColor teamColor, out int fps, out Tile[] tiles, bool generateFallback)
        {
            fps = 0;
            tiles = null;
            Tileset first = null;
            foreach (var tileset in tilesets.Join(searchTilesets, x => x.Key, y => y, (x, y) => x.Value))
            {
                if (generateFallback && first == null)
                {
                    first = tileset;
                }
                if (tileset.GetTileData(name, shape, teamColor, out fps, out tiles, false))
                {
                    return true;
                }
            }
            if (generateFallback && first != null && first.GetTileData(name, shape, teamColor, out fps, out tiles, generateFallback))
            {
                return true;
            }
            return false;
        }

        public bool GetTeamColorTileData(IEnumerable<string> searchTilesets, string name, int shape, TeamColor teamColor, out int fps, out Tile tile, bool generateFallback)
        {
            tile = null;
            if (!GetTeamColorTileData(searchTilesets, name, shape, teamColor, out fps, out Tile[] tiles, generateFallback))
            {
                return false;
            }
            tile = tiles[0];
            return true;
        }

        public bool GetTeamColorTileData(IEnumerable<string> searchTilesets, string name, int shape, TeamColor teamColor, out Tile[] tiles, bool generateFallback)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, teamColor, out int fps, out tiles, generateFallback);
        }

        public bool GetTeamColorTileData(IEnumerable<string> searchTilesets, string name, int shape, TeamColor teamColor, out Tile[] tiles)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, teamColor, out int fps, out tiles, false);
        }

        public bool GetTeamColorTileData(IEnumerable<string> searchTilesets, string name, int shape, TeamColor teamColor, out Tile tile, bool generateFallback)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, teamColor, out int fps, out tile, generateFallback);
        }

        public bool GetTeamColorTileData(IEnumerable<string> searchTilesets, string name, int shape, TeamColor teamColor, out Tile tile)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, teamColor, out int fps, out tile, false);
        }

        public bool GetTileData(IEnumerable<string> searchTilesets, string name, int shape, out int fps, out Tile[] tiles, bool generateFallback)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, null, out fps, out tiles, generateFallback);
        }

        public bool GetTileData(IEnumerable<string> searchTilesets, string name, int shape, out int fps, out Tile[] tiles)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, null, out fps, out tiles, false);
        }

        public bool GetTileData(IEnumerable<string> searchTilesets, string name, int shape, out int fps, out Tile tile, bool generateFallback)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, null, out fps, out tile, generateFallback);
        }

        public bool GetTileData(IEnumerable<string> searchTilesets, string name, int shape, out int fps, out Tile tile)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, null, out fps, out tile, false);
        }

        public bool GetTileData(IEnumerable<string> searchTilesets, string name, int shape, out Tile[] tiles, bool generateFallback)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, null, out tiles, generateFallback);
        }

        public bool GetTileData(IEnumerable<string> searchTilesets, string name, int shape, out Tile[] tiles)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, null, out tiles, false);
        }

        public bool GetTileData(IEnumerable<string> searchTilesets, string name, int shape, out Tile tile, bool generateFallback)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, null, out tile, generateFallback);
        }

        public bool GetTileData(IEnumerable<string> searchTilesets, string name, int shape, out Tile tile)
        {
            return GetTeamColorTileData(searchTilesets, name, shape, null, out tile, false);
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
