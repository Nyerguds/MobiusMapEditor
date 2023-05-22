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

    [Flags]
    public enum BuildingTypeFlag
    {
        None           = 0,
        Factory        = (1 << 0),
        Bib            = (1 << 1),
        Fake           = (1 << 2),
        Turret         = (1 << 3),
        SingleFrame    = (1 << 4),
        NoRemap        = (1 << 5),
        Flat           = (1 << 6),
        IsGapGenerator = (1 << 7),
    }

    public class BuildingType : ICellOverlapper, ICellOccupier, ITechnoType
    {
        public sbyte ID { get; private set; }

        public string Name { get; private set; }

        public string DisplayName { get; private set; }

        public BuildingTypeFlag Flag { get; private set; }

        public String GraphicsSource { get; private set; }

        public int FrameOFfset { get; private set; }

        public int PowerUsage { get; set; }

        public int PowerProduction { get; set; }

        public int Storage { get; set; }

        public Rectangle OverlapBounds => new Rectangle(Point.Empty, new Size(OccupyMask.GetLength(1), OccupyMask.GetLength(0)));
        public bool[,] OpaqueMask { get; private set; }

        public bool[,] OccupyMask { get; private set; }

        /// <summary>Actual footprint of the building, without bibs involved.</summary>
        public bool[,] BaseOccupyMask { get; private set; }

        public Size Size { get; private set; }

        public bool HasBib
        {
            get { return (Flag & BuildingTypeFlag.Bib) == BuildingTypeFlag.Bib; }
            set
            {
                if (value)
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
        public TheaterType[] Theaters { get; private set; }
        public string FactoryOverlay { get; private set; }
        public Bitmap Thumbnail { get; set; }
        public bool IsArmed => false; // Not actually true, but irrelevant for practical purposes; their Mission is not set in the ini file.
        public bool IsHarvester => false;
        public bool IsAircraft => false;
        public bool IsFixedWing => false;

        public bool IsFake => (Flag & BuildingTypeFlag.Fake) == BuildingTypeFlag.Fake;
        public bool HasTurret => (Flag & BuildingTypeFlag.Turret) == BuildingTypeFlag.Turret;
        public bool IsSingleFrame => (Flag & BuildingTypeFlag.SingleFrame) == BuildingTypeFlag.SingleFrame;
        public bool CanRemap => (Flag & BuildingTypeFlag.NoRemap) != BuildingTypeFlag.NoRemap;
        /// <summary>
        /// Indicates buildings that have pieces sticking out at the top that should not overlap the objects on these cells.
        /// </summary>
        public bool IsFlat => (Flag & BuildingTypeFlag.Flat) == BuildingTypeFlag.Flat;

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int storage, int width, int height, string occupyMask, string ownerHouse, TheaterType[] theaters, string factoryOverlay, int frameOffset, String graphicsSource, BuildingTypeFlag flag)
        {
            this.ID = id;
            this.Flag = flag;
            this.FrameOFfset = frameOffset;
            this.Name = name;
            this.GraphicsSource = graphicsSource ?? name;
            this.DisplayName = !String.IsNullOrEmpty(textId) && !String.IsNullOrEmpty(Globals.TheGameTextManager[textId])
                ? Globals.TheGameTextManager[textId] + " (" + Name.ToUpperInvariant() + ")"
                : name.ToUpperInvariant();
            this.PowerProduction = powerProd;
            this.PowerUsage = powerUse;
            this.Storage = storage;
            this.BaseOccupyMask = GeneralUtils.GetMaskFromString(width, height, occupyMask);
            this.Size = new Size(width, height);
            this.OwnerHouse = ownerHouse;
            this.Theaters = theaters;
            this.FactoryOverlay = factoryOverlay;
            this.RecalculateBibs();
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int width, int height, string occupyMask, string ownerHouse, TheaterType[] theaters, String graphicsSource, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, 0, width, height, occupyMask, ownerHouse, theaters, null, 0, graphicsSource, flag)
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
            int maskY = BaseOccupyMask.GetLength(0);
            int maskX = BaseOccupyMask.GetLength(1);
            if (HasBib)
            {
                OccupyMask = new bool[maskY + 1, maskX];
                for (var y = 0; y < maskY; ++y)
                {
                    for (var x = 0; x < maskX; ++x)
                    {
                        OccupyMask[y, x] = BaseOccupyMask[y, x];
                    }
                }
                if (Globals.BlockingBibs)
                {
                    for (var x = 0; x < maskX; ++x)
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
            int baseMaskY = this.BaseOccupyMask.GetLength(0);
            int baseMaskX = this.BaseOccupyMask.GetLength(1);
            string occupyMask = GeneralUtils.GetStringFromMask(this.BaseOccupyMask);
            TheaterType[] theaters = null;
            if (Theaters != null)
            {
                int thLen = Theaters.Length;
                theaters = new TheaterType[thLen];
                Array.Copy(Theaters, theaters, thLen);
            }
            // Don't do lookup of the UI name. We don't have the original lookup ID at this point, so don't bother, and just restore the name afterwards.
            BuildingType newBld = new BuildingType(ID, Name, null, PowerProduction, PowerUsage, Storage, baseMaskX, baseMaskY, occupyMask, OwnerHouse, theaters, FactoryOverlay, FrameOFfset, GraphicsSource, Flag);
            newBld.DisplayName = DisplayName;
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

        public void Init(GameType gameType, TheaterType theater, HouseType house, DirectionType direction)
        {
            var oldImage = Thumbnail;
            var mockBuilding = new Building()
            {
                Type = this,
                House = house,
                Strength = 256,
                Direction = direction
            };
            var render = MapRenderer.RenderBuilding(gameType, theater, Point.Empty, Globals.PreviewTileSize, Globals.PreviewTileScale, mockBuilding);
            if (!render.Item1.IsEmpty)
            {
                var th = new Bitmap(render.Item1.Width, render.Item1.Height);
                th.SetResolution(96, 96);
                using (var g = Graphics.FromImage(th))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    render.Item2(g);
                    if (IsFake)
                    {
                        MapRenderer.RenderFakeBuildingLabel(g, mockBuilding, Point.Empty, Globals.PreviewTileSize, Globals.PreviewTileScale, false);
                    }
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
    }
}
