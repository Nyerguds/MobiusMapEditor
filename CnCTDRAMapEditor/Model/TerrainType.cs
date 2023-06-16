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
using MobiusEditor.Render;
using MobiusEditor.Utility;
using System;
using System.Drawing;

namespace MobiusEditor.Model
{
    public class TerrainType : ITechnoType, ICellOverlapper, ICellOccupier
    {
        public sbyte ID { get; private set; }
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public Rectangle OverlapBounds => new Rectangle(Point.Empty, Size);
        public bool[,] OpaqueMask { get; private set; }
        public bool[,] OccupyMask { get; private set; }
        public Size Size => new Size(OccupyMask.GetLength(1), OccupyMask.GetLength(0));
        public TheaterType[] Theaters { get; private set; }
        public int DisplayIcon { get; private set; }
        public TemplateTypeFlag TemplateType { get; private set; }
        public String GraphicsSource { get; private set; }
        public bool IsArmed => false;
        public bool IsAircraft => false;
        public bool IsFixedWing => false;
        public bool IsHarvester => false;
        private string nameId;

        public Size GetRenderSize(Size cellSize)
        {
            return new Size(Size.Width * cellSize.Width, Size.Height * cellSize.Height);
        }

        public Bitmap Thumbnail { get; set; }

        /// <summary>
        /// Creates a new TerrainType object.
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="name">Name</param>
        /// <param name="textId">Text ID to look up in the game text manager.</param>
        /// <param name="theaters">Theaters in which this appears. Null means all.</param>
        /// <param name="width">Width of the terrain object, in cells.</param>
        /// <param name="height">Height of the terrain object, in cells.</param>
        /// <param name="occupyMask">String indicating the occupied cells. Spaces are ignored, '0' means unoccupied, any other value means occupied. A null value indicates it is fully occupied.</param>
        /// <param name="graphicsSource">Override for the graphics source. If null, the name is used.</param>
        /// <param name="displayIcon">Override for the frame to display. Normally 0.</param>
        /// <param name="templateType">Terrain type this should be placed down on. Currently unused.</param>
        public TerrainType(sbyte id, string name, string textId, TheaterType[] theaters, int width, int height, string occupyMask, String graphicsSource, int displayIcon, TemplateTypeFlag templateType)
        {
            this.ID = id;
            this.Name = name;
            this.nameId = textId;
            this.Theaters = theaters;
            this.OccupyMask = GeneralUtils.GetMaskFromString(width, height, occupyMask);
            this.DisplayIcon = displayIcon;
            this.GraphicsSource = graphicsSource == null ? name : graphicsSource;
            this.TemplateType = templateType;
        }

        /// <summary>
        /// Creates a new TerrainType object.
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="name">Name</param>
        /// <param name="textId">Text ID to look up in the game text manager.</param>
        /// <param name="theaters">Theaters in which this appears. Null means all.</param>
        /// <param name="width">Width of the terrain object, in cells.</param>
        /// <param name="height">Height of the terrain object, in cells.</param>
        /// <param name="occupyMask">String indicating the occupied cells. Spaces are ignored, '0' means unoccupied, any other value means occupied. A null value indicates it is fully occupied.</param>
        /// <param name="graphicsSource">Override for the graphics source. If null, the name is used.</param>
        /// <param name="displayIcon">Override for the frame to display. Normally 0.</param>
        public TerrainType(sbyte id, string name, string textId, TheaterType[] theaters, int width, int height, string occupyMask, String graphicsSource, int displayIcon)
            : this(id, name, textId, theaters, width, height, occupyMask, graphicsSource, displayIcon, TemplateTypeFlag.None)
        {
        }

        /// <summary>
        /// Creates a new TerrainType object.
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="name">Name</param>
        /// <param name="textId">Text ID to look up in the game text manager.</param>
        /// <param name="theaters">Theaters in which this appears. Null means all.</param>
        /// <param name="width">Width of the terrain object, in cells.</param>
        /// <param name="height">Height of the terrain object, in cells.</param>
        /// <param name="occupyMask">String indicating the occupied cells. Spaces are ignored, '0' means unoccupied, any other value means occupied. A null value indicates it is fully occupied.</param>
        /// <param name="graphicsSource">Override for the graphics source. If null, the name is used.</param>
        public TerrainType(sbyte id, string name, string textId, TheaterType[] theaters, int width, int height, string occupyMask, String graphicsSource)
            : this(id, name, textId, theaters, width, height, occupyMask, graphicsSource, 0, TemplateTypeFlag.None)
        {
        }

        /// <summary>
        /// Creates a new TerrainType object.
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="name">Name</param>
        /// <param name="textId">Text ID to look up in the game text manager.</param>
        /// <param name="theaters">Theaters in which this appears. Null means all.</param>
        /// <param name="width">Width of the terrain object, in cells.</param>
        /// <param name="height">Height of the terrain object, in cells.</param>
        /// <param name="occupyMask">String indicating the occupied cells. Spaces are ignored, '0' means unoccupied, any other value means occupied. A null value indicates it is fully occupied.</param>
        /// <param name="displayIcon">Override for the frame to display. Normally 0.</param>
        public TerrainType(sbyte id, string name, string textId, TheaterType[] theaters, int width, int height, string occupyMask, int displayIcon)
            : this(id, name, textId, theaters, width, height, occupyMask, null, displayIcon, TemplateTypeFlag.None)
        {
        }

        /// <summary>
        /// Creates a new TerrainType object.
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="name">Name</param>
        /// <param name="textId">Text ID to look up in the game text manager.</param>
        /// <param name="theaters">Theaters in which this appears. Null means all.</param>
        /// <param name="width">Width of the terrain object, in cells.</param>
        /// <param name="height">Height of the terrain object, in cells.</param>
        /// <param name="occupyMask">String indicating the occupied cells. Spaces are ignored, '0' means unoccupied, any other value means occupied. A null value indicates it is fully occupied.</param>
        /// <param name="templateType">Terrain type this should be placed down on. Currently unused.</param>
        public TerrainType(sbyte id, string name, string textId, TheaterType[] theaters, int width, int height, string occupyMask, TemplateTypeFlag templateType)
            : this(id, name, textId, theaters, width, height, occupyMask, null, 0, templateType)
        {
        }

        /// <summary>
        /// Creates a new TerrainType object.
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="name">Name</param>
        /// <param name="textId">Text ID to look up in the game text manager.</param>
        /// <param name="theaters">Theaters in which this appears. Null means all.</param>
        /// <param name="width">Width of the terrain object, in cells.</param>
        /// <param name="height">Height of the terrain object, in cells.</param>
        /// <param name="occupyMask">String indicating the occupied cells. Spaces are ignored, '0' means unoccupied, any other value means occupied. A null value indicates it is fully occupied.</param>
        public TerrainType(sbyte id, string name, string textId, TheaterType[] theaters, int width, int height, string occupyMask)
            : this(id, name, textId, theaters, width, height, occupyMask, null, 0, TemplateTypeFlag.None)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is TerrainType)
            {
                return this == obj;
            }
            else if (obj is sbyte)
            {
                return ID == (sbyte)obj;
            }
            else if (obj is string)
            {
                return string.Equals(Name, obj as string, StringComparison.OrdinalIgnoreCase);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return (Name ?? String.Empty).ToUpperInvariant();
        }

        public void Init()
        {
            this.DisplayName = !String.IsNullOrEmpty(nameId) && !String.IsNullOrEmpty(Globals.TheGameTextManager[nameId])
                ? Globals.TheGameTextManager[nameId] + " (" + Name.ToUpperInvariant() + ")"
                : Name.ToUpperInvariant();
            var oldImage = Thumbnail;
            string tileName = GraphicsSource;
            if (Globals.TheTilesetManager.GetTileData(tileName, DisplayIcon, out Tile tile))
            {
                var tileSize = Globals.PreviewTileSize;
                var renderSize = new Size(tileSize.Width * Size.Width, tileSize.Height * Size.Height);
                Rectangle overlayBounds = MapRenderer.RenderBounds(tile.Image.Size, Size, Globals.PreviewTileScale);
                Bitmap th = new Bitmap(renderSize.Width, renderSize.Height);
                th.SetResolution(96, 96);
                using (Graphics g = Graphics.FromImage(th))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    g.DrawImage(tile.Image, overlayBounds);
                }
                Thumbnail = th;
                OpaqueMask = GeneralUtils.FindOpaqueCells(th, Size, 10, 25, 0x80);
            }
            else
            {
                Thumbnail = null;
            }
            if (oldImage != null)
            {
                try { oldImage.Dispose(); }
                catch { /* ignore */ }
            }
        }
        public void Reset()
        {
            Bitmap oldImage = this.Thumbnail;
            this.Thumbnail = null;
            if (oldImage != null)
            {
                try { oldImage.Dispose(); }
                catch { /* ignore */ }
            }
        }
    }
}
