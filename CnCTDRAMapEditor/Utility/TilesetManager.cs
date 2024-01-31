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
using MobiusEditor.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace MobiusEditor.Utility
{
    /// <summary>
    /// TilesetManager: contains a dictionary of tilesets mapped by name. The Theater decides which of them will be searched in.
    /// Tileset: thematically grouped collection of Shapes, stored as Dictionary by Shape name
    /// Shape: Dictionary of frame numbers and the TileData for each frame number
    /// TileData: Contains the animation rate and a collection of filenames for all alternate frames of this tile.
    ///        Also contains a dictionary with the actually-cached Tile arrays using the team color name as key.
    /// Tile[]: The cached remapped images, one for each filename in TileData, plus their transparent bounds.
    ///        Normally just one item, except if there are alternates of the same frame (e.g. water animations).
    /// </summary>
    public class TilesetManager: ITilesetManager, IDisposable
    {
        private readonly Dictionary<string, Tileset> tilesets = new Dictionary<string, Tileset>();

        private readonly IArchiveManager archiveManager;
        private readonly TextureManager textureManager;
        private readonly string xmlPath;
        private readonly string texturesPath;
        private TheaterType theater;

        public TextureManager TextureManager { get { return textureManager; } }

        public TilesetManager(IArchiveManager megafileManager, string xmlPath, string texturesPath)
        {
            this.archiveManager = megafileManager;
            this.textureManager = new TextureManager(megafileManager);
            this.xmlPath = xmlPath;
            this.texturesPath = texturesPath;
            LoadXmlfiles();
        }

        public bool TilesetExists(string tilesetName)
        {
            return tilesets.ContainsKey(tilesetName);
        }

        private void LoadXmlfiles()
        {
            HashSet<String> allowedTileSets = this.theater == null ? null : theater.Tilesets.ToHashSet();
            this.textureManager.Reset();
            foreach (var item in tilesets)
            {
                item.Value.Reset();
            }
            tilesets.Clear();
            XmlDocument xmlDoc = null;
            using (Stream xmlStream = archiveManager.OpenFile(xmlPath))
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
                    using (Stream xmlStream = archiveManager.OpenFile(xmlFile))
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
                            string name = tilesetNode.Attributes["name"].Value;
                            if (allowedTileSets != null && allowedTileSets.Contains(name))
                            {
                                continue;
                            }
                            var tileset = new Tileset(textureManager);
                            tileset.Load(tilesetNode.OuterXml, texturesPath);
                            tilesets[name] = tileset;
                        }
                    }
                }
            }
        }

        public void Reset(GameType gameType, TheaterType theater)
        {
            LoadXmlfiles();
            this.theater = theater;
        }

        private bool GetTeamColorTileData(string name, int shape, ITeamColor teamColor, out int fps, out Tile[] tiles, bool generateFallback, bool onlyIfDefined)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException("TilesetManager");
            }
            fps = 0;
            tiles = null;
            Tileset first = null;
            bool isDummy = false;
            // Tilesets are now searched in the given order, allowing accurate defining of main tilesets and fallback tilesets.
            //foreach (var tileset in tilesets.Join(searchTilesets, x => x.Key, y => y, (x, y) => x.Value))
            if (this.theater == null)
            {
                return false;
            }
            IEnumerable<string> searchTilesets = this.theater.Tilesets;
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
                if (tileset.GetTileData(name, shape, teamColor, out fps, out tiles, out isDummy, false))
                {
                    if (generateFallback && tiles.Any(t => t.Image == null))
                    {
                        // Tile not found, or contains no data. Re-fetch with dummy generation.
                        if (tileset.GetTileData(name, shape, teamColor, out fps, out tiles, out isDummy, true))
                        {
                            // Signal in return value that dummy was generated.
                            return false;
                        }
                        continue;
                    }
                    return !isDummy;
                }
            }
            // If the tile is not defined at all, and onlyifdefined is not enabled, make a dummy entry anyway.
            if (generateFallback && !onlyIfDefined && first != null && first.GetTileData(name, shape, teamColor, out fps, out tiles, out isDummy, true))
            {
                // Signal in return value that dummy was generated.
                return false;
            }
            return false;
        }

        private bool GetTeamColorTileData(string name, int shape, ITeamColor teamColor, out int fps, out Tile tile, bool generateFallback, bool onlyIfDefined)
        {
            bool success = GetTeamColorTileData(name, shape, teamColor, out fps, out Tile[] tiles, generateFallback, onlyIfDefined);
            tile = tiles == null ? null : tiles[0];
            return success;
        }

        public bool GetTeamColorTileData(string name, int shape, ITeamColor teamColor, out Tile tile, bool generateFallback, bool onlyIfDefined)
        {
            return GetTeamColorTileData(name, shape, teamColor, out int fps, out tile, generateFallback, onlyIfDefined);
        }

        public bool GetTeamColorTileData(string name, int shape, ITeamColor teamColor, out Tile tile)
        {
            return GetTeamColorTileData(name, shape, teamColor, out int fps, out tile, false, false);
        }

        public bool GetTileData(string name, int shape, out Tile tile, bool generateFallback, bool onlyIfDefined)
        {
            return GetTeamColorTileData(name, shape, null, out tile, generateFallback, onlyIfDefined);
        }

        public bool GetTileData(string name, int shape, out Tile tile)
        {
            return GetTeamColorTileData(name, shape, null, out tile, false, false);
        }

        public int GetTileDataLength(string name)
        {
            if (name == null ||  this.theater == null)
            {
                return -1;
            }
            IEnumerable<string> searchTilesets = this.theater.Tilesets;
            foreach (Tileset tileset in tilesets.Join(searchTilesets, x => x.Key, y => y, (x, y) => x.Value))
            {
                int frames = tileset.GetTileDataLength(name);
                if (frames != -1)
                {
                    return frames;
                }
            }
            return -1;
        }

        public bool TileExists(string name)
        {
            return GetTileDataLength(name) > 0;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    TextureManager.Dispose();
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
