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
        /// <summary>This building type is a wall. Only show if placing walls as buildings is enabled.</summary>
        Wall             /**/ = 1 << 9,
        /// <summary>This type typically has no rules present in the rules file, and needs specific checks on that.</summary>
        NoRules          /**/ = 1 << 10,
    }

    [DebuggerDisplay("{Name}")]
    public class BuildingType : ICellOverlapper, ICellOccupier, ITechnoType
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public bool Ownable => true;
        public string DisplayName { get; private set; }
        public string DisplayNameWithTheaterInfo
        {
            get
            {
                if (!IsTheaterDependent || ExistsInTheater)
                {
                    return DisplayName;
                }
                else
                {
                    return DisplayName + " (different theater)";
                }
            }
        }

        public BuildingTypeFlag Flag { get; private set; }
        public string GraphicsSource { get; private set; }
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
            get { return Flag.HasFlag(BuildingTypeFlag.Bib); }
            set
            {
                // Bibs are only supported for widths 2 to 4
                if (value && Size.Width >= 2 && Size.Width <= 4)
                {
                    Flag |= BuildingTypeFlag.Bib;
                }
                else
                {
                    Flag &= ~BuildingTypeFlag.Bib;
                }
                RecalculateBibs();
            }
        }

        public string OwnerHouse { get; private set; }
        public bool IsTheaterDependent => Flag.HasFlag(BuildingTypeFlag.TheaterDependent);
        public string FactoryOverlay { get; private set; }
        public Bitmap Thumbnail { get; set; }
        public bool ExistsInTheater { get; set; }
        public bool IsArmed => false; // Not actually true, but irrelevant for practical purposes; their Mission is not set in the ini file.
        public bool IsAircraft => false;
        public bool IsFixedWing => false;
        public bool IsHarvester => false;
        public bool IsExpansionOnly => false;

        public bool IsFake => Flag.HasFlag(BuildingTypeFlag.Fake);
        public bool HasTurret => Flag.HasFlag(BuildingTypeFlag.Turret);
        public bool IsSingleFrame => Flag.HasFlag(BuildingTypeFlag.SingleFrame);
        public bool CanRemap => !Flag.HasFlag(BuildingTypeFlag.NoRemap);
        public bool IsWall => Flag.HasFlag(BuildingTypeFlag.Wall);
        public bool GraphicsFound { get; private set; }
        /// <summary>
        /// Value for Z-sorting; can be used to make buildings specifically show as "flatter" than others so pieces sticking out at the top don't overlap objects on these cells.
        /// </summary>
        public int ZOrder { get; private set; }
        private string nameId;

        public BuildingType(int id, string name, string textId, int powerProd, int powerUse, int storage, bool capturable, int width, int height, string occupyMask, string ownerHouse, string factoryOverlay, int frameOffset, string graphicsSource, BuildingTypeFlag flag, int zOrder)
        {
            ID = id;
            Flag = flag;
            FrameOffset = frameOffset;
            Name = name;
            GraphicsSource = graphicsSource ?? name;
            nameId = textId;
            PowerProduction = powerProd;
            PowerUsage = powerUse;
            Storage = storage;
            Capturable = capturable;
            BaseOccupyMask = GeneralUtils.GetMaskFromString(width, height, occupyMask, '0', ' ');
            OwnerHouse = ownerHouse;
            FactoryOverlay = factoryOverlay;
            ZOrder = zOrder;
            // Check on width and disable if needed. This also calls RecalculateBibs.
            HasBib = HasBib;
        }

        public BuildingType(int id, string name, string textId, int powerProd, int powerUse, int storage, bool capturable, int width, int height, string occupyMask, string ownerHouse, string factoryOverlay, int frameOffset, string graphicsSource, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, storage, capturable, width, height, occupyMask, ownerHouse, factoryOverlay, frameOffset, graphicsSource, flag, Globals.ZOrderDefault)
        {
        }

        public BuildingType(int id, string name, string textId, int powerProd, int powerUse, bool capturable, int width, int height, string occupyMask, string ownerHouse, string graphicsSource, BuildingTypeFlag flag)
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
            int maskY = BaseOccupyMask.GetLength(0);
            int maskX = BaseOccupyMask.GetLength(1);
            if (HasBib)
            {
                OccupyMask = new bool[maskY + 1, maskX];
                for (int y = 0; y < maskY; ++y)
                {
                    for (int x = 0; x < maskX; ++x)
                    {
                        OccupyMask[y, x] = BaseOccupyMask[y, x];
                    }
                }
                if (Globals.BlockingBibs)
                {
                    for (int x = 0; x < maskX; ++x)
                    {
                        OccupyMask[maskY, x] = true;
                        OccupyMask[maskY - 1, x] = true;
                    }
                }
            }
            else
            {
                OccupyMask = BaseOccupyMask;
            }
        }

        public BuildingType Clone()
        {
            // Get original dimensions, and mask in string form.
            int baseMaskY = BaseOccupyMask.GetLength(0);
            int baseMaskX = BaseOccupyMask.GetLength(1);
            string occupyMask = GeneralUtils.GetStringFromMask(BaseOccupyMask, '1', '0', ' ');
            BuildingType newBld = new BuildingType(ID, Name, nameId, PowerProduction, PowerUsage, Storage, Capturable, baseMaskX, baseMaskY, occupyMask, OwnerHouse, FactoryOverlay, FrameOffset, GraphicsSource, Flag, ZOrder);
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
                return ID == sb;
            }
            else if (obj is byte b)
            {
                return ID == b;
            }
            else if (obj is int i)
            {
                return ID == i;
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

        public void InitDisplayName()
        {
            DisplayName = !String.IsNullOrEmpty(nameId) && !String.IsNullOrEmpty(Globals.TheGameTextManager[nameId])
                ? Globals.TheGameTextManager[nameId] + " (" + Name.ToUpperInvariant() + ")"
                : Name.ToUpperInvariant();
        }

        public void Init(GameInfo gameInfo, HouseType house, DirectionType direction)
        {
            InitDisplayName();
            ExistsInTheater = Globals.TheTilesetManager.TileExists(GraphicsSource);
            Bitmap oldImage = Thumbnail;
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
                Bitmap th = new Bitmap(Size.Width * Globals.PreviewTileSize.Width, Size.Height * Globals.PreviewTileSize.Height);
                th.SetResolution(96, 96);
                using (Graphics g = Graphics.FromImage(th))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    render.RenderAction(g);
                    if (IsFake)
                    {
                        Size previewSize = mockBuilding.OccupyMask.GetDimensions();
                        Rectangle visibleBounds = new Rectangle(Point.Empty, previewSize);
                        MapRenderer.RenderAllFakeBuildingLabels(g, gameInfo, (Point.Empty, mockBuilding).Yield(), visibleBounds, Globals.PreviewTileSize);
                    }
                    GraphicsFound = !render.IsDummy;
                }
                Thumbnail = th;
                // calculate the areas of this that can overlap other objects (include shadow)
                OverlapMask = GeneralUtils.MakeOpaqueMask(th, Size, 25, 10, 20, 0x10, false);
                // calculate the areas of this that need to be overlapped to consider this covered (exclude shadow)
                ContentMask = GeneralUtils.MakeOpaqueMask(th, Size, 25, 10, 20, 0xE0, !Globals.UseClassicFiles);
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
            ExistsInTheater = false;
            Bitmap oldImage = Thumbnail;
            Thumbnail = null;
            if (oldImage != null)
            {
                try { oldImage.Dispose(); }
                catch { /* ignore */ }
            }
        }
    }
}
