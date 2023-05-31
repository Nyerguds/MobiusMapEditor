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
        /// <summary>No flags set.</summary>
        None            = 0,
        /// <summary>Is a basic resource overlay.</summary>
        TiberiumOrGold  = (1 << 0),
        /// <summary>Is a high value resource overlay.</summary>
        Gems            = (1 << 1),
        /// <summary>Is a wall.</summary>
        Wall            = (1 << 2),
        /// <summary>Is a wooden crate. This affects the color of the outline it gets.</summary>
        WoodCrate       = (1 << 3),
        /// <summary>Is a steel crate. This affects the color of the outline it gets.</summary>
        SteelCrate      = (1 << 4),
        /// <summary>Is the flag placement indicator.</summary>
        Flag            = (1 << 5),
        /// <summary>Is a pavement type.</summary>
        Pavement        = (1 << 6),
        /// <summary>Needs to use the special concrete pavement connection logic.</summary>
        Concrete        = (1 << 7),
        /// <summary>Is a solid object that obstructs placement.</summary>
        Solid           = (1 << 8),
        /// <summary>Is a crate.</summary>
        Crate           = WoodCrate | SteelCrate,
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

        public bool IsPavement => (Flag & OverlayTypeFlag.Pavement) != OverlayTypeFlag.None;

        public bool IsConcrete => (Flag & OverlayTypeFlag.Concrete) != OverlayTypeFlag.None;

        public bool IsCrate => (Flag & OverlayTypeFlag.Crate) != OverlayTypeFlag.None;

        public bool IsFlag => (Flag & OverlayTypeFlag.Flag) != OverlayTypeFlag.None;

        public Color Tint { get; set; } = Color.White;

        /// <summary>
        /// Defines that it is placeable under the "overlay" category (and not resource or wall)
        /// </summary>
        public bool IsOverlay => (Flag & (OverlayTypeFlag.Wall | OverlayTypeFlag.TiberiumOrGold | OverlayTypeFlag.Gems)) == OverlayTypeFlag.None;

        public OverlayType(sbyte id, string name, string textId, TheaterType[] theaters, OverlayTypeFlag flag, String graphicsSource, int forceTileNr, Color tint)
        {
            this.ID = id;
            this.Name = name;
            this.GraphicsSource = graphicsSource == null ? name : graphicsSource;
            this.ForceTileNr = forceTileNr;
            // Shows graphics source and not real internal name to mask different internal name for ROAD #2.
            this.DisplayName = !String.IsNullOrEmpty(textId) && !String.IsNullOrEmpty(Globals.TheGameTextManager[textId])
                ? Globals.TheGameTextManager[textId] + " (" + GraphicsSource.ToUpperInvariant() + ")"
                : name.ToUpperInvariant();
            this.Theaters = theaters;
            this.Flag = flag;
            this.Tint = tint;
        }

        public OverlayType(sbyte id, string name, string textId, TheaterType[] theaters, OverlayTypeFlag flag, String graphicsSource, int forceTileNr)
            : this(id, name, textId, theaters, flag, graphicsSource, forceTileNr, Color.White)
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

        public OverlayType(sbyte id, string name, string textId, OverlayTypeFlag flag, int forceTileNr)
            : this(id, name, textId, null, flag, null, forceTileNr, Color.White)
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

        public void Init(GameType gameType, TheaterType theater)
        {
            var oldImage = Thumbnail;
            var tileSize = Globals.PreviewTileSize;
            Bitmap th = new Bitmap(tileSize.Width, tileSize.Height);
            using (Graphics g = Graphics.FromImage(th))
            {
                int tilenr = ForceTileNr == -1 ? 0 : ForceTileNr;
                Overlay mockOverlay = new Overlay()
                {
                    Type = this,
                    Icon = tilenr,
                };
                MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                MapRenderer.RenderOverlay(gameType, theater, Point.Empty, Globals.PreviewTileSize, Globals.PreviewTileScale, mockOverlay).Item2(g);
            }
            Thumbnail = th;
            if (oldImage != null)
            {
                try { oldImage.Dispose(); }
                catch { /* ignore */ }
            }
        }
    }
}
