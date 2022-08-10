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
using MobiusEditor.Utility;
using System;
using System.Drawing;

namespace MobiusEditor.Model
{
    [Flags]
    public enum OverlayTypeFlag
    {
        // Nyerguds upgrade: Added decoration type.
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

        public Image Thumbnail { get; set; }

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

         // No reason not to allow placing decorations and flag pedestal.
        public bool IsPlaceable => (Flag & (OverlayTypeFlag.Crate | OverlayTypeFlag.Decoration | OverlayTypeFlag.Flag | OverlayTypeFlag.Concrete)) != OverlayTypeFlag.None;

        public OverlayType(sbyte id, string name, string textId, TheaterType[] theaters, OverlayTypeFlag flag, String graphicsLoadOverride, int forceTileNr)
        {
            ID = id;
            Name = name;
            GraphicsSource = graphicsLoadOverride == null ? name : graphicsLoadOverride;
            ForceTileNr = forceTileNr;
            DisplayName = Globals.TheGameTextManager[textId] + " (" + GraphicsSource.ToUpperInvariant() + ")";
            Theaters = theaters;
            Flag = flag;
        }

        public OverlayType(sbyte id, string name, string textId, TheaterType[] theaters, OverlayTypeFlag flag)
            :this(id, name, textId, theaters, flag, null, -1)
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

        public OverlayType(sbyte id, string name, string textId)
            : this(id, name, textId, null, OverlayTypeFlag.None)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is OverlayType)
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
            int tilenr = ForceTileNr == -1 ? 0 : ForceTileNr;
            if (Globals.TheTilesetManager.GetTileData(theater.Tilesets, GraphicsSource, tilenr, out Tile tile, (Flag & OverlayTypeFlag.Decoration) != 0))
            {
                var tileSize = new Size(Globals.OriginalTileWidth, Globals.OriginalTileHeight);
                var size = tile.Image.Width < Globals.OriginalTileWidth && tile.Image.Height < Globals.OriginalTileHeight ?
                    new Size(tile.Image.Width, tile.Image.Height) : tileSize;
                var location = new Point(tileSize.Width / 2 - size.Width / 2, tileSize.Height / 2 - size.Height / 2);
                var overlayBounds = new Rectangle(location, size);
                Bitmap th = new Bitmap(tileSize.Width, tileSize.Height);
                using (Graphics graphics = Graphics.FromImage(th))
                {
                    graphics.DrawImage(tile.Image, overlayBounds);
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
