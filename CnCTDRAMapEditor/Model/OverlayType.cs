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
using System.Drawing.Imaging;

namespace MobiusEditor.Model
{
    [Flags]
    public enum OverlayTypeFlag
    {
        // Nyerguds upgrade: Added decoration and concrete types.
        None            = 0,
        TiberiumOrGold  = (1 << 0),
        Gems            = (1 << 1),
        Wall            = (1 << 2),
        Crate           = (1 << 3),
        Flag            = (1 << 4),
        Decoration      = (1 << 5),
        Concrete        = (1 << 6),
    }

    public class OverlayType : ICellOccupier, IBrowsableType
    {

        public sbyte ID { get; private set; }

        public string Name { get; private set; }

        public string DisplayName { get; private set; }

        public TheaterType[] Theaters { get; private set; }

        public OverlayTypeFlag Flag { get; private set; }

        public Bitmap Thumbnail { get; set; }

        public String GraphicsSource { get; private set; }

        public int ForceTileNr { get; private set; }

        public bool[,] OccupyMask => new bool[1, 1] { { true } };

        public bool IsResource => (Flag & (OverlayTypeFlag.TiberiumOrGold | OverlayTypeFlag.Gems)) != OverlayTypeFlag.None;

        public bool IsTiberiumOrGold => (Flag & OverlayTypeFlag.TiberiumOrGold) != OverlayTypeFlag.None;

        public bool IsGem => (Flag & OverlayTypeFlag.Gems) != OverlayTypeFlag.None;

        public bool IsWall => (Flag & OverlayTypeFlag.Wall) != OverlayTypeFlag.None;

        public bool IsDecoration => (Flag & OverlayTypeFlag.Decoration) != OverlayTypeFlag.None;

        public bool IsConcrete => (Flag & OverlayTypeFlag.Concrete) != OverlayTypeFlag.None;

        public bool IsCrate => (Flag & OverlayTypeFlag.Crate) != OverlayTypeFlag.None;

        public bool IsFlag => (Flag & OverlayTypeFlag.Flag) != OverlayTypeFlag.None;

        public Color Tint { get; set; } = Color.White;

        // No reason not to allow placing decorations and flag pedestal.
        public bool IsPlaceable => (Flag & (OverlayTypeFlag.Crate | OverlayTypeFlag.Decoration | OverlayTypeFlag.Flag | OverlayTypeFlag.Concrete)) != OverlayTypeFlag.None;

        public OverlayType(sbyte id, string name, string textId, TheaterType[] theaters, OverlayTypeFlag flag, String graphicsSource, int forceTileNr, Color tint)
        {
            ID = id;
            Name = name;
            GraphicsSource = graphicsSource == null ? name : graphicsSource;
            ForceTileNr = forceTileNr;
            DisplayName = Globals.TheGameTextManager[textId] + " (" + GraphicsSource.ToUpperInvariant() + ")";
            Theaters = theaters;
            Flag = flag;
            Tint = tint;
        }

        public OverlayType(sbyte id, string name, string textId, TheaterType[] theaters, OverlayTypeFlag flag, String graphicsSource, int forceTileNr)
            : this(id, name, textId, theaters, flag, graphicsSource, forceTileNr, Color.White)
        {
        }

        public OverlayType(sbyte id, string name, string textId, TheaterType[] theaters, OverlayTypeFlag flag, String graphicsSource)
            :this(id, name, textId, theaters, flag, graphicsSource, -1, Color.White)
        {
        }

        public OverlayType(sbyte id, string name, string textId, TheaterType[] theaters, OverlayTypeFlag flag, int forceTileNr)
            :this(id, name, textId, theaters, flag, null, forceTileNr, Color.White)
        {
        }

        public OverlayType(sbyte id, string name, string textId, TheaterType[] theaters, OverlayTypeFlag flag)
            : this(id, name, textId, theaters, flag, null, -1, Color.White)
        {
        }

        public OverlayType(sbyte id, string name, string textId, OverlayTypeFlag flag)
            : this(id, name, textId, null, flag)
        {
        }

        public OverlayType(sbyte id, string name, string textId, TheaterType[] theaters)
            : this(id, name, textId, theaters, OverlayTypeFlag.Decoration)
        {
        }

        public OverlayType(sbyte id, string name, OverlayTypeFlag flag)
            : this(id, name, name, null, flag)
        {
        }

        public OverlayType(sbyte id, string name, OverlayTypeFlag flag, int forceTileNr)
            : this(id, name, name, null, flag, null, forceTileNr, Color.White)
        {
        }

        public OverlayType(sbyte id, string name, string textId)
            : this(id, name, textId, null, OverlayTypeFlag.None)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is OverlayType ovl)
            {
                return ReferenceEquals(this, obj) || (this.ID == ovl.ID && string.Equals(this.Name, ovl.Name, StringComparison.OrdinalIgnoreCase));
            }
            else if (obj is sbyte sb)
            {
                return this.ID == sb;
            }
            else if (obj is byte b)
            {
                return this.ID == b;
            }
            else if (obj is int i)
            {
                return this.ID == i;
            }
            else if (obj is string str)
            {
                return string.Equals(this.Name, str, StringComparison.OrdinalIgnoreCase);
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
            int tilenr = ForceTileNr == -1 ? 0 : ForceTileNr;
            if (Globals.TheTilesetManager.GetTileData(theater.Tilesets, GraphicsSource, tilenr, out Tile tile, (Flag & OverlayTypeFlag.Decoration) != 0, false))
            {
                var tileSize = Globals.PreviewTileSize;
                Rectangle overlayBounds = MapRenderer.RenderBounds(tile.Image.Size, new Size(1, 1), Globals.PreviewTileScale);
                Bitmap th = new Bitmap(tileSize.Width, tileSize.Height);
                using (Graphics g = Graphics.FromImage(th))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    var imageAttributes = new ImageAttributes();
                    if (Tint != Color.White)
                    {
                        var colorMatrix = new ColorMatrix(new float[][]
                        {
                            new float[] { Tint.R / 255.0f, 0, 0, 0, 0 },
                            new float[] { 0, Tint.G / 255.0f, 0, 0, 0 },
                            new float[] { 0, 0, Tint.B / 255.0f, 0, 0 },
                            new float[] { 0, 0, 0, 1, 0 },
                            new float[] { 0, 0, 0, 0, 1 },
                        });
                        imageAttributes.SetColorMatrix(colorMatrix);
                    }
                    g.DrawImage(tile.Image, overlayBounds, 0, 0, tile.Image.Width, tile.Image.Height, GraphicsUnit.Pixel, imageAttributes);
                }
                Thumbnail = th;
            }
            else
            {
                Thumbnail = SystemIcons.Error.ToBitmap();
            }
            if (oldImage != null)
            {
                try { oldImage.Dispose(); }
                catch { /* ignore */ }
            }
        }
    }
}
