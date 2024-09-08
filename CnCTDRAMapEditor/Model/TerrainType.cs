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
using MobiusEditor.Render;
using MobiusEditor.Utility;
using System;
using System.Diagnostics;
using System.Drawing;

namespace MobiusEditor.Model
{
    [DebuggerDisplay("{Name}")]
    public class TerrainType : ITechnoType, ICellOverlapper, ICellOccupier
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public Rectangle OverlapBounds => new Rectangle(Point.Empty, this.Size);
        public bool[,][] OpaqueMask { get; private set; }
        public bool[,] OccupyMask { get; private set; }
        public Size Size => this.OccupyMask.GetDimensions();
        public bool[,] BaseOccupyMask => OccupyMask;
        public int ZOrder => Globals.ZOrderDefault;
        public bool ExistsInTheater { get; private set; }
        public int DisplayIcon { get; private set; }
        public LandType PlacementLand { get; private set; }
        public String GraphicsSource { get; private set; }
        public bool IsArmed => false;
        public bool IsAircraft => false;
        public bool IsFixedWing => false;
        public bool IsHarvester => false;
        public bool IsExpansionOnly => false;
        public bool CanRemap => false;
        public Point CenterPoint { get; private set; }
        private string nameId;

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
        /// <param name="placementLand">Land type this should be placed down on. Currently unused.</param>
        public TerrainType(int id, string name, string textId, int width, int height, Point centerPoint, string occupyMask, string graphicsSource, int displayIcon, LandType placementLand)
        {
            this.ID = id;
            this.Name = name;
            this.nameId = textId;
            this.OccupyMask = GeneralUtils.GetMaskFromString(width, height, occupyMask, '0', ' ');
            if (centerPoint == Point.Empty)
            {
                centerPoint = GeneralUtils.GetOccupiedCenter(OccupyMask, new Size(Globals.PixelWidth, Globals.PixelHeight));
            }
            this.CenterPoint = centerPoint;
            this.GraphicsSource = graphicsSource == null ? name : graphicsSource;
            this.DisplayIcon = displayIcon;
            this.PlacementLand = placementLand;
        }

        /// <summary>
        /// Creates a new TerrainType object.
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="name">Name</param>
        /// <param name="textId">Text ID to look up in the game text manager.</param>
        /// <param name="width">Width of the terrain object, in cells.</param>
        /// <param name="height">Height of the terrain object, in cells.</param>
        /// <param name="occupyMask">String indicating the occupied cells. Spaces are ignored, '0' means unoccupied, any other value means occupied. A null value indicates it is fully occupied.</param>
        /// <param name="graphicsSource">Override for the graphics source. If null, the name is used.</param>
        /// <param name="displayIcon">Override for the frame to display. Normally 0.</param>
        public TerrainType(int id, string name, string textId, int width, int height, Point centerPoint, string occupyMask, String graphicsSource, int displayIcon)
            : this(id, name, textId, width, height, centerPoint, occupyMask, graphicsSource, displayIcon, LandType.Clear)
        {
        }

        /// <summary>
        /// Creates a new TerrainType object.
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="name">Name</param>
        /// <param name="textId">Text ID to look up in the game text manager.</param>
        /// <param name="width">Width of the terrain object, in cells.</param>
        /// <param name="height">Height of the terrain object, in cells.</param>
        /// <param name="occupyMask">String indicating the occupied cells. Spaces are ignored, '0' means unoccupied, any other value means occupied. A null value indicates it is fully occupied.</param>
        /// <param name="graphicsSource">Override for the graphics source. If null, the name is used.</param>
        public TerrainType(int id, string name, string textId, int width, int height, Point centerPoint, string occupyMask, String graphicsSource)
            : this(id, name, textId, width, height, centerPoint, occupyMask, graphicsSource, 0, LandType.Clear)
        {
        }

        /// <summary>
        /// Creates a new TerrainType object.
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="name">Name</param>
        /// <param name="textId">Text ID to look up in the game text manager.</param>
        /// <param name="width">Width of the terrain object, in cells.</param>
        /// <param name="height">Height of the terrain object, in cells.</param>
        /// <param name="occupyMask">String indicating the occupied cells. Spaces are ignored, '0' means unoccupied, any other value means occupied. A null value indicates it is fully occupied.</param>
        /// <param name="displayIcon">Override for the frame to display. Normally 0.</param>
        public TerrainType(int id, string name, string textId, int width, int height, Point centerPoint, string occupyMask, int displayIcon)
            : this(id, name, textId, width, height, centerPoint, occupyMask, null, displayIcon, LandType.Clear)
        {
        }

        /// <summary>
        /// Creates a new TerrainType object.
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="name">Name</param>
        /// <param name="textId">Text ID to look up in the game text manager.</param>
        /// <param name="width">Width of the terrain object, in cells.</param>
        /// <param name="height">Height of the terrain object, in cells.</param>
        /// <param name="occupyMask">String indicating the occupied cells. Spaces are ignored, '0' means unoccupied, any other value means occupied. A null value indicates it is fully occupied.</param>
        /// <param name="placementLand">Land type this should be placed down on. Currently unused.</param>
        public TerrainType(int id, string name, string textId, int width, int height, Point centerPoint, string occupyMask, LandType placementLand)
            : this(id, name, textId, width, height, centerPoint, occupyMask, null, 0, placementLand)
        {
        }

        /// <summary>
        /// Creates a new TerrainType object.
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="name">Name</param>
        /// <param name="textId">Text ID to look up in the game text manager.</param>
        /// <param name="width">Width of the terrain object, in cells.</param>
        /// <param name="height">Height of the terrain object, in cells.</param>
        /// <param name="occupyMask">String indicating the occupied cells. Spaces are ignored, '0' means unoccupied, any other value means occupied. A null value indicates it is fully occupied.</param>
        public TerrainType(int id, string name, string textId, int width, int height, Point centerPoint, string occupyMask)
            : this(id, name, textId, width, height, centerPoint, occupyMask, null, 0, LandType.Clear)
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
                return this.ID == (sbyte)obj;
            }
            else if (obj is string)
            {
                return string.Equals(this.Name, obj as string, StringComparison.OrdinalIgnoreCase);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        public override string ToString()
        {
            return (this.Name ?? String.Empty).ToUpperInvariant();
        }

        public void InitDisplayName()
        {
            this.DisplayName = !String.IsNullOrEmpty(this.nameId) && !String.IsNullOrEmpty(Globals.TheGameTextManager[this.nameId])
                ? Globals.TheGameTextManager[this.nameId] + " (" + this.Name.ToUpperInvariant() + ")"
                : this.Name.ToUpperInvariant();
        }

        public void Init()
        {
            this.InitDisplayName();
            this.ExistsInTheater = Globals.TheTilesetManager.TileExists(this.GraphicsSource);
            if (!this.ExistsInTheater && !String.Equals(this.GraphicsSource, this.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                this.ExistsInTheater = Globals.TheTilesetManager.TileExists(this.Name);
            }
            Bitmap oldImage = this.Thumbnail;
            Terrain mockTerrain = new Terrain()
            {
                Type = this,
            };
            RenderInfo render = MapRenderer.RenderTerrain(Point.Empty, Globals.PreviewTileSize, Globals.PreviewTileScale, mockTerrain, false);
            if (render.RenderedObject != null)
            {
                Bitmap th = new Bitmap(this.Size.Width * Globals.PreviewTileSize.Width, this.Size.Height * Globals.PreviewTileSize.Height);
                th.SetResolution(96, 96);
                using (Graphics g = Graphics.FromImage(th))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    render.RenderAction(g);
                }
                this.Thumbnail = th;
                this.OpaqueMask = GeneralUtils.MakeOpaqueMask(th, this.Size, 25, 10, 20, 0x80, false);
            }
            else
            {
                this.Thumbnail = null;
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
