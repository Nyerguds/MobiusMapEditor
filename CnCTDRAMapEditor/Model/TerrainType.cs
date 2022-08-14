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
    public class TerrainType : ITechnoType, ICellOverlapper, ICellOccupier, IBrowsableType
    {
        public sbyte ID { get; private set; }

        public string Name { get; private set; }

        public string DisplayName => Name;

        public Rectangle OverlapBounds => new Rectangle(Point.Empty, Size);

        public bool[,] OccupyMask { get; private set; }

        public Size Size => new Size(OccupyMask.GetLength(1), OccupyMask.GetLength(0));

        public TheaterType[] Theaters { get; private set; }

        public int DisplayIcon { get; private set; }

        public TemplateTypeFlag TemplateType { get; private set; }

        public bool IsArmed => false;

        public bool IsHarvester => false;

        public Size GetRenderSize(Size cellSize)
        {
            return new Size(Size.Width * cellSize.Width, Size.Height * cellSize.Height);
        }

        public Image Thumbnail { get; set; }

        public TerrainType(sbyte id, string name, TheaterType[] theaters, bool[,] occupyMask, int displayIcon, TemplateTypeFlag templateType)
        {
            ID = id;
            Name = name;
            Theaters = theaters;
            OccupyMask = occupyMask;
            DisplayIcon = displayIcon;
            TemplateType = templateType;
        }

        public TerrainType(sbyte id, string name, TheaterType[] theaters, bool[,] occupyMask, int displayIcon)
            : this(id, name, theaters, occupyMask, displayIcon, TemplateTypeFlag.None)
        {
        }

        public TerrainType(sbyte id, string name, TheaterType[] theaters, bool[,] occupyMask, TemplateTypeFlag templateType)
            : this(id, name, theaters, occupyMask, 0, templateType)
        {
        }

        public TerrainType(sbyte id, string name, TheaterType[] theaters, bool[,] occupyMask)
            : this(id, name, theaters, occupyMask, 0, TemplateTypeFlag.None)
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
            return Name;
        }

        public void Init(TheaterType theater)
        {
            var oldImage = Thumbnail;
            string tileName = Name;
            if ((TemplateType & TemplateTypeFlag.OreMine) != TemplateTypeFlag.None)
            {
                tileName = "OREMINE";
            }

            if (Globals.TheTilesetManager.GetTileData(theater.Tilesets, tileName, DisplayIcon, out Tile tile))
            {
                var tileSize = Globals.PreviewTileSize;
                var renderSize = new Size(tileSize.Width * Size.Width, tileSize.Height * Size.Height);
                Rectangle overlayBounds = MapRenderer.RenderBounds(tile.Image.Size, Size, tileSize);
                Bitmap th = new Bitmap(renderSize.Width, renderSize.Height);
                using (Graphics g = Graphics.FromImage(th))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    g.DrawImage(tile.Image, overlayBounds);
                }
                Thumbnail = th;
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
    }
}
