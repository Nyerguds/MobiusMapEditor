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
    [Flags]
    public enum BuildingTypeFlag
    {
        /// <summary>No flags set.</summary>
        None             /**/ = 0,
        /// <summary>Produces structures.</summary>
        Factory          /**/ = 1 << 0,
        /// <summary>Has a bib attached.</summary>
        Bib              /**/ = 1 << 1,
        /// <summary>Is a fake building.</summary>
        Fake             /**/ = 1 << 2,
        /// <summary>Has a rotating turret, and accepts a Facing value in the ini file.</summary>
        Turret           /**/ = 1 << 3,
        /// <summary>Only has a single frame of graphics.</summary>
        SingleFrame      /**/ = 1 << 4,
        /// <summary>Does not adjust to house colors.</summary>
        NoRemap          /**/ = 1 << 5,
        /// <summary>Can show a gap area-of-effect radius indicator.</summary>
        GapGenerator     /**/ = 1 << 7,
        /// <summary>Do not show this building in the lists if its graphics were not found in the currently loaded theater.</summary>
        TheaterDependent /**/ = 1 << 8,
        /// <summary>This building type s a wall. Only show if placing walls as buildings is enabled.</summary>
        Wall             /**/ = 1 << 9,
    }

    [DebuggerDisplay("{Name}")]
    public class BuildingType : ICellOverlapper, ICellOccupier, ITechnoType
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public string DisplayNameWithTheaterInfo
        {
            get
            {
                if (!this.IsTheaterDependent || this.ExistsInTheater)
                {
                    return this.DisplayName;
                }
                else
                {
                    return this.DisplayName + " (different theater)";
                }
            }
        }

        public BuildingTypeFlag Flag { get; private set; }
        public String GraphicsSource { get; private set; }
        public int FrameOffset { get; private set; }
        public int PowerUsage { get; set; }
        public int PowerProduction { get; set; }
        public int Storage { get; set; }
        public bool Capturable { get; set; }
        public Rectangle OverlapBounds => new Rectangle(Point.Empty, OccupyMask.GetDimensions());
        public bool[,][] OverlapMask { get; private set; }
        public bool[,][] ContentMask { get; private set; }
        public bool[,] OccupyMask { get; private set; }

        /// <summary>Actual footprint of the building, without bibs involved.</summary>
        public bool[,] BaseOccupyMask { get; private set; }
        public Size Size => BaseOccupyMask.GetDimensions();
        public bool HasBib
        {
            get { return this.Flag.HasFlag(BuildingTypeFlag.Bib); }
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
        public bool IsTheaterDependent => this.Flag.HasFlag(BuildingTypeFlag.TheaterDependent);
        public string FactoryOverlay { get; private set; }
        public Bitmap Thumbnail { get; set; }
        public bool ExistsInTheater { get; set; }
        public bool IsArmed => false; // Not actually true, but irrelevant for practical purposes; their Mission is not set in the ini file.
        public bool IsAircraft => false;
        public bool IsFixedWing => false;
        public bool IsHarvester => false;
        public bool IsExpansionOnly => false;

        public bool IsFake => this.Flag.HasFlag(BuildingTypeFlag.Fake);
        public bool HasTurret => this.Flag.HasFlag(BuildingTypeFlag.Turret);
        public bool IsSingleFrame => this.Flag.HasFlag(BuildingTypeFlag.SingleFrame);
        public bool CanRemap => !this.Flag.HasFlag(BuildingTypeFlag.NoRemap);
        public bool IsWall => this.Flag.HasFlag(BuildingTypeFlag.Wall);
        /// <summary>
        /// Value for Z-sorting; can be used to make buildings specifically show as "flatter" than others so pieces sticking out at the top don't overlap objects on these cells.
        /// </summary>
        public int ZOrder { get; private set; }
        private string nameId;

        public BuildingType(int id, string name, string textId, int powerProd, int powerUse, int storage, bool capturable, int width, int height, string occupyMask, string ownerHouse, string factoryOverlay, int frameOffset, String graphicsSource, BuildingTypeFlag flag, int zOrder)
        {
            this.ID = id;
            this.Flag = flag;
            this.FrameOffset = frameOffset;
            this.Name = name;
            this.GraphicsSource = graphicsSource ?? name;
            this.nameId = textId;
            this.PowerProduction = powerProd;
            this.PowerUsage = powerUse;
            this.Storage = storage;
            this.Capturable = capturable;
            this.BaseOccupyMask = GeneralUtils.GetMaskFromString(width, height, occupyMask, '0', ' ');
            this.OwnerHouse = ownerHouse;
            this.FactoryOverlay = factoryOverlay;
            this.ZOrder = zOrder;
            // Check on width and disable if needed. This also calls RecalculateBibs.
            this.HasBib = this.HasBib;
        }

        public BuildingType(int id, string name, string textId, int powerProd, int powerUse, int storage, bool capturable, int width, int height, string occupyMask, string ownerHouse, string factoryOverlay, int frameOffset, String graphicsSource, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, storage, capturable, width, height, occupyMask, ownerHouse, factoryOverlay, frameOffset, graphicsSource, flag, Globals.ZOrderDefault)
        {
        }

        public BuildingType(int id, string name, string textId, int powerProd, int powerUse, bool capturable, int width, int height, string occupyMask, string ownerHouse, String graphicsSource, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, 0, capturable, width, height, occupyMask, ownerHouse, null, 0, graphicsSource, flag)
        {
        }

        public BuildingType(int id, string name, string textId, int powerProd, int powerUse, bool capturable, int width, int height, string occupyMask, string ownerHouse, int zOrder)
            : this(id, name, textId, powerProd, powerUse, 0, capturable, width, height, occupyMask, ownerHouse, null, 0, null, BuildingTypeFlag.None, zOrder)
        {
        }

        public BuildingType(int id, string name, string textId, int powerProd, int powerUse, bool capturable, int width, int height, string occupyMask, string ownerHouse, BuildingTypeFlag flag, int zOrder)
            : this(id, name, textId, powerProd, powerUse, 0, capturable, width, height, occupyMask, ownerHouse, null, 0, null, flag, zOrder)
        {
        }

        public BuildingType(int id, string name, string textId, int powerProd, int powerUse, bool capturable, int width, int height, string occupyMask, string ownerHouse, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, 0, capturable, width, height, occupyMask, ownerHouse, null, 0, null, flag)
        {
        }

        public BuildingType(int id, string name, string textId, int powerProd, int powerUse, int storage, bool capturable, int width, int height, string occupyMask, string ownerHouse)
            : this(id, name, textId, powerProd, powerUse, storage, capturable, width, height, occupyMask, ownerHouse, null, 0, null, BuildingTypeFlag.None)
        {
        }

        public BuildingType(int id, string name, string textId, int powerProd, int powerUse, bool capturable, int width, int height, string occupyMask, string ownerHouse)
            : this(id, name, textId, powerProd, powerUse, 0, capturable, width, height, occupyMask, ownerHouse, null, 0, null, BuildingTypeFlag.None)
        {
        }

        public BuildingType(int id, string name, string textId, int powerProd, int powerUse, bool capturable, int width, int height, string occupyMask, string ownerHouse, int frameOffset, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, 0, capturable, width, height, occupyMask, ownerHouse, null, frameOffset, null, flag)
        {
        }

        public BuildingType(int id, string name, string textId, int powerProd, int powerUse, bool capturable, int width, int height, string occupyMask, string ownerHouse, string factoryOverlay, string graphicsSource, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, 0, capturable, width, height, occupyMask, ownerHouse, factoryOverlay, 0, graphicsSource, flag)
        {
        }

        public BuildingType(int id, string name, string textId, int powerProd, int powerUse, int storage, bool capturable, int width, int height, string occupyMask, string ownerHouse, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, storage, capturable, width, height, occupyMask, ownerHouse, null, 0, null, flag)
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
            string occupyMask = GeneralUtils.GetStringFromMask(this.BaseOccupyMask, '1', '0', ' ');
            BuildingType newBld = new BuildingType(this.ID, this.Name, this.nameId, this.PowerProduction, this.PowerUsage, this.Storage, this.Capturable, baseMaskX, baseMaskY, occupyMask, this.OwnerHouse, this.FactoryOverlay, this.FrameOffset, this.GraphicsSource, this.Flag, this.ZOrder);
            return newBld;
        }

        public override bool Equals(object obj)
        {
            if (obj is BuildingType)
            {
                return this == obj;
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

        public void Init(GameInfo gameInfo, HouseType house, DirectionType direction)
        {
            this.InitDisplayName();
            this.ExistsInTheater = Globals.TheTilesetManager.TileExists(this.GraphicsSource);
            Bitmap oldImage = this.Thumbnail;
            Building mockBuilding = new Building()
            {
                Type = this,
                House = house,
                Strength = 256,
                Direction = direction
            };
            RenderInfo render = MapRenderer.RenderBuilding(gameInfo, null, Point.Empty, Globals.PreviewTileSize, Globals.PreviewTileScale, mockBuilding, false);
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
                        Size previewSize = mockBuilding.OccupyMask.GetDimensions();
                        Rectangle visibleBounds = new Rectangle(Point.Empty, previewSize);
                        MapRenderer.RenderAllFakeBuildingLabels(g, gameInfo, (Point.Empty, mockBuilding).Yield(), visibleBounds, Globals.PreviewTileSize);
                    }
                }
                this.Thumbnail = th;
                this.OverlapMask = GeneralUtils.MakeOpaqueMask(th, this.Size, 25, 10, 20, 0x80, false);
                this.ContentMask = GeneralUtils.MakeOpaqueMask(th, this.Size, 25, 10, 20, Globals.UseClassicFiles ? 0x80 : 0x40, !Globals.UseClassicFiles);
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
            this.ExistsInTheater = false;
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
