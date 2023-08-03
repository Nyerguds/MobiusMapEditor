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
using System.Linq;

namespace MobiusEditor.Model
{

    [Flags]
    public enum BuildingTypeFlag
    {
        /// <summary>No flags set.</summary>
        None           = 0,
        /// <summary>Produces structures.</summary>
        Factory        = (1 << 0),
        /// <summary>Has a bib attached.</summary>
        Bib            = (1 << 1),
        /// <summary>Is a fake building.</summary>
        Fake           = (1 << 2),
        /// <summary>Has a rotating turret, and accepts a Facing value in the ini file.</summary>
        Turret         = (1 << 3),
        /// <summary>Only has a single frame of graphics.</summary>
        SingleFrame    = (1 << 4),
        /// <summary>Does not adjust to house colors.</summary>
        NoRemap        = (1 << 5),
        /// <summary>Can show a gap area-of-effect radius indicator.</summary>
        IsGapGenerator = (1 << 7),
    }

    public class BuildingType : ICellOverlapper, ICellOccupier, ITechnoType
    {
        public sbyte ID { get; private set; }
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public string DisplayNameWithTheaterInfo
        {
            get
            {
                if (this.Theaters == null)
                {
                    return this.DisplayName;
                }
                if (this.Theaters.Length == 0)
                {
                    return this.DisplayName + " (Not in any theaters)";
                }
                return this.DisplayName + " (" + String.Join(",", this.Theaters.Select(th => th.Name).ToArray()) +")";
            }
        }

        public BuildingTypeFlag Flag { get; private set; }
        public String GraphicsSource { get; private set; }
        public int FrameOFfset { get; private set; }
        public int PowerUsage { get; set; }
        public int PowerProduction { get; set; }
        public int Storage { get; set; }
        public Rectangle OverlapBounds => new Rectangle(Point.Empty, new Size(this.OccupyMask.GetLength(1), this.OccupyMask.GetLength(0)));
        public bool[,] OpaqueMask { get; private set; }
        public bool[,] OccupyMask { get; private set; }

        /// <summary>Actual footprint of the building, without bibs involved.</summary>
        public bool[,] BaseOccupyMask { get; private set; }
        public Rectangle BaseOccupyBounds => new Rectangle(Point.Empty, new Size(this.BaseOccupyMask.GetLength(1), this.BaseOccupyMask.GetLength(0)));
        public Size Size { get; private set; }
        public bool HasBib
        {
            get { return (this.Flag & BuildingTypeFlag.Bib) == BuildingTypeFlag.Bib; }
            set
            {
                // Bibs are only supported for widths 2 to 4
                if (value && this.Size.Width >= 2 && this.Size.Width <= 4)
                {
                    this.Flag |= BuildingTypeFlag.Bib;
                }
                else
                {
                    this.Flag &= ~BuildingTypeFlag.Bib;
                }
                this.RecalculateBibs();
            }
        }

        public string OwnerHouse { get; private set; }
        public TheaterType[] Theaters { get; private set; }
        public string FactoryOverlay { get; private set; }
        public Bitmap Thumbnail { get; set; }
        public bool IsArmed => false; // Not actually true, but irrelevant for practical purposes; their Mission is not set in the ini file.
        public bool IsAircraft => false;
        public bool IsFixedWing => false;
        public bool IsHarvester => false;
        public bool IsExpansionOnly => false;

        public bool IsFake => (this.Flag & BuildingTypeFlag.Fake) == BuildingTypeFlag.Fake;
        public bool HasTurret => (this.Flag & BuildingTypeFlag.Turret) == BuildingTypeFlag.Turret;
        public bool IsSingleFrame => (this.Flag & BuildingTypeFlag.SingleFrame) == BuildingTypeFlag.SingleFrame;
        public bool CanRemap => (this.Flag & BuildingTypeFlag.NoRemap) != BuildingTypeFlag.NoRemap;
        /// <summary>
        /// Indicates buildings that have pieces sticking out at the top that should not overlap the objects on these cells.
        /// </summary>
        public int ZOrder { get; private set; }
        private string nameId;

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int storage, int width, int height, string occupyMask, string ownerHouse, TheaterType[] theaters, string factoryOverlay, int frameOffset, String graphicsSource, BuildingTypeFlag flag, int zOrder)
        {
            this.ID = id;
            this.Flag = flag;
            this.FrameOFfset = frameOffset;
            this.Name = name;
            this.GraphicsSource = graphicsSource ?? name;
            this.nameId = textId;
            this.PowerProduction = powerProd;
            this.PowerUsage = powerUse;
            this.Storage = storage;
            this.BaseOccupyMask = GeneralUtils.GetMaskFromString(width, height, occupyMask);
            this.Size = new Size(width, height);
            this.OwnerHouse = ownerHouse;
            this.Theaters = theaters;
            this.FactoryOverlay = factoryOverlay;
            this.ZOrder = zOrder;
            // Check on width and disable if needed. This also calls RecalculateBibs.
            this.HasBib = this.HasBib;
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int storage, int width, int height, string occupyMask, string ownerHouse, TheaterType[] theaters, string factoryOverlay, int frameOffset, String graphicsSource, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, storage, width, height, occupyMask, ownerHouse, theaters, factoryOverlay, frameOffset, graphicsSource, flag, 10)
        {
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int width, int height, string occupyMask, string ownerHouse, TheaterType[] theaters, String graphicsSource, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, 0, width, height, occupyMask, ownerHouse, theaters, null, 0, graphicsSource, flag)
        {
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int width, int height, string occupyMask, string ownerHouse, TheaterType[] theaters, int zOrder)
            : this(id, name, textId, powerProd, powerUse, 0, width, height, occupyMask, ownerHouse, theaters, null, 0, null, BuildingTypeFlag.None, zOrder)
        {
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int width, int height, string occupyMask, string ownerHouse, TheaterType[] theaters, BuildingTypeFlag flag, int zOrder)
            : this(id, name, textId, powerProd, powerUse, 0, width, height, occupyMask, ownerHouse, theaters, null, 0, null, flag, zOrder)
        {
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int width, int height, string occupyMask, string ownerHouse, TheaterType[] theaters, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, 0, width, height, occupyMask, ownerHouse, theaters, null, 0, null, flag)
        {
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int storage, int width, int height, string occupyMask, string ownerHouse)
            : this(id, name, textId, powerProd, powerUse, storage, width, height, occupyMask, ownerHouse, null, null, 0, null, BuildingTypeFlag.None)
        {
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int width, int height, string occupyMask, string ownerHouse)
            : this(id, name, textId, powerProd, powerUse, 0, width, height, occupyMask, ownerHouse, null, null, 0, null, BuildingTypeFlag.None)
        {
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int width, int height, string occupyMask, string ownerHouse, int frameOffset, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, 0, width, height, occupyMask, ownerHouse, null, null, frameOffset, null, flag)
        {
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int width, int height, string occupyMask, string ownerHouse, string factoryOverlay, string graphicsSource, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, 0, width, height, occupyMask, ownerHouse, null, factoryOverlay, 0, graphicsSource, flag)
        {
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int width, int height, string occupyMask, string ownerHouse, string graphicsSource, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, 0, width, height, occupyMask, ownerHouse, null, null, 0, graphicsSource, flag)
        {
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int storage, int width, int height, string occupyMask, string ownerHouse, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, storage, width, height, occupyMask, ownerHouse, null, null, 0, null, flag)
        {
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int width, int height, string occupyMask, string ownerHouse, int zOrder)
            : this(id, name, textId, powerProd, powerUse, 0, width, height, occupyMask, ownerHouse, null, null, 0, null, BuildingTypeFlag.None, zOrder)
        {
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int width, int height, string occupyMask, string ownerHouse, BuildingTypeFlag flag, int zOrder)
            : this(id, name, textId, powerProd, powerUse, 0, width, height, occupyMask, ownerHouse, null, null, 0, null, flag, zOrder)
        {
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int width, int height, string occupyMask, string ownerHouse, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, 0, width, height, occupyMask, ownerHouse, null, null, 0, null, flag)
        {
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int width, int height, string occupyMask, string ownerHouse, TheaterType[] theaters)
            : this(id, name, textId, powerProd, powerUse, 0, width, height, occupyMask, ownerHouse, theaters, null, 0, null, BuildingTypeFlag.None)
        {
        }

        private void RecalculateBibs()
        {
            int maskY = this.BaseOccupyMask.GetLength(0);
            int maskX = this.BaseOccupyMask.GetLength(1);
            if (this.HasBib)
            {
                this.OccupyMask = new bool[maskY + 1, maskX];
                for (int y = 0; y < maskY; ++y)
                {
                    for (int x = 0; x < maskX; ++x)
                    {
                        this.OccupyMask[y, x] = this.BaseOccupyMask[y, x];
                    }
                }
                if (Globals.BlockingBibs)
                {
                    for (int x = 0; x < maskX; ++x)
                    {
                        this.OccupyMask[maskY, x] = true;
                        this.OccupyMask[maskY - 1, x] = true;
                    }
                }
            }
            else
            {
                this.OccupyMask = this.BaseOccupyMask;
            }
        }

        public BuildingType Clone()
        {
            // Get original dimensions, and mask in string form.
            int baseMaskY = this.BaseOccupyMask.GetLength(0);
            int baseMaskX = this.BaseOccupyMask.GetLength(1);
            string occupyMask = GeneralUtils.GetStringFromMask(this.BaseOccupyMask);
            TheaterType[] theaters = null;
            if (this.Theaters != null)
            {
                int thLen = this.Theaters.Length;
                theaters = new TheaterType[thLen];
                Array.Copy(this.Theaters, theaters, thLen);
            }
            BuildingType newBld = new BuildingType(this.ID, this.Name, this.nameId, this.PowerProduction, this.PowerUsage, this.Storage, baseMaskX, baseMaskY, occupyMask, this.OwnerHouse, theaters, this.FactoryOverlay, this.FrameOFfset, this.GraphicsSource, this.Flag, this.ZOrder);
            return newBld;
        }

        public override bool Equals(object obj)
        {
            if (obj is BuildingType)
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

        public void Init(GameType gameType, HouseType house, DirectionType direction)
        {
            this.InitDisplayName();
            Bitmap oldImage = this.Thumbnail;
            Building mockBuilding = new Building()
            {
                Type = this,
                House = house,
                Strength = 256,
                Direction = direction
            };
            RenderInfo render = MapRenderer.RenderBuilding(gameType, Point.Empty, Globals.PreviewTileSize, Globals.PreviewTileScale, mockBuilding);
            if (render.RenderedObject != null)
            {
                Bitmap th = new Bitmap(this.Size.Width * Globals.PreviewTileSize.Width, this.Size.Height * Globals.PreviewTileSize.Height);
                th.SetResolution(96, 96);
                using (Graphics g = Graphics.FromImage(th))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    render.RenderAction(g);
                    if (this.IsFake)
                    {
                        MapRenderer.RenderFakeBuildingLabel(g, mockBuilding, Point.Empty, Globals.PreviewTileSize, false);
                    }
                }
                this.Thumbnail = th;
                this.OpaqueMask = GeneralUtils.FindOpaqueCells(th, this.Size, 10, 25, 0x80);
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
