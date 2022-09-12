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
using MobiusEditor.Tools;
using System;
using System.Drawing;

namespace MobiusEditor.Model
{

    [Flags]
    public enum BuildingTypeFlag
    {
        None        = 0,
        Factory     = (1 << 0),
        Bib         = (1 << 1),
        Fake        = (1 << 2),
        Turret      = (1 << 3),
        SingleFrame = (1 << 4),
        NoRemap     = (1 << 5),
    }

    public class BuildingType : ICellOverlapper, ICellOccupier, ITechnoType, IBrowsableType
    {
        public sbyte ID { get; private set; }

        public string Name { get; private set; }

        public string DisplayName { get; private set; }

        public string Tilename { get; private set; }

        public BuildingTypeFlag Flag { get; private set; }

        public int PowerUsage { get; set; }

        public int PowerProduction { get; set; }

        public int Storage { get; set; }

        public Rectangle OverlapBounds => new Rectangle(Point.Empty, new Size(OccupyMask.GetLength(1), OccupyMask.GetLength(0)));

        public bool[,] OccupyMask { get; private set; }

        // Actual footprint of the building, without bibs involved.
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

        public bool IsFake => (Flag & BuildingTypeFlag.Fake) == BuildingTypeFlag.Fake;
        public bool HasTurret => (Flag & BuildingTypeFlag.Turret) == BuildingTypeFlag.Turret;
        public bool IsSingleFrame => (Flag & BuildingTypeFlag.SingleFrame) == BuildingTypeFlag.SingleFrame;
        public bool CanRemap => (Flag & BuildingTypeFlag.NoRemap) != BuildingTypeFlag.NoRemap;

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int storage, bool[,] occupyMask, string ownerHouse, TheaterType[] theaters, string factoryOverlay, BuildingTypeFlag flag)
        {
            ID = id;
            Flag = flag;
            Name = IsFake ? (name.Substring(0, name.Length - 1) + "f") : name;
            DisplayName = (IsFake ? "Fake " : String.Empty) + Globals.TheGameTextManager[textId] + " (" + Name.ToUpperInvariant() + ")";
            Tilename = name;
            PowerProduction = powerProd;
            PowerUsage = powerUse;
            Storage = storage;
            BaseOccupyMask = occupyMask;
            int maskY = occupyMask.GetLength(0);
            int maskX = occupyMask.GetLength(1);
            Size = new Size(maskX, maskY);
            OwnerHouse = ownerHouse;
            Theaters = theaters;
            FactoryOverlay = factoryOverlay;
            RecalculateBibs();
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, bool[,] occupyMask, string ownerHouse, TheaterType[] theaters, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, 0, occupyMask, ownerHouse, theaters, null, flag)
        {
        }
        
        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int storage, bool[,] occupyMask, string ownerHouse)
            : this(id, name, textId, powerProd, powerUse, storage, occupyMask, ownerHouse, null, null, BuildingTypeFlag.None)
        {
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, bool[,] occupyMask, string ownerHouse)
            : this(id, name, textId, powerProd, powerUse, 0, occupyMask, ownerHouse, null, null, BuildingTypeFlag.None)
        {
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, bool[,] occupyMask, string ownerHouse, string factoryOverlay, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, 0, occupyMask, ownerHouse, null, factoryOverlay, flag)
        {
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, int storage, bool[,] occupyMask, string ownerHouse, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, storage, occupyMask, ownerHouse, null, null, flag)
        {
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, bool[,] occupyMask, string ownerHouse, BuildingTypeFlag flag)
            : this(id, name, textId, powerProd, powerUse, 0, occupyMask, ownerHouse, null, null, flag)
        {
        }

        public BuildingType(sbyte id, string name, string textId, int powerProd, int powerUse, bool[,] occupyMask, string ownerHouse, TheaterType[] theaters)
            : this(id, name, textId, powerProd, powerUse, 0, occupyMask, ownerHouse, theaters, null, BuildingTypeFlag.None)
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
            int baseMaskY = BaseOccupyMask.GetLength(0);
            int baseMaskX = BaseOccupyMask.GetLength(1);
            bool[,] bOccupyMask = new bool[baseMaskY, baseMaskX];
            for (var y = 0; y < baseMaskY; ++y)
            {
                for (var x = 0; x < baseMaskX; ++x)
                {
                    bOccupyMask[y, x] = BaseOccupyMask[y, x];
                }
            }
            TheaterType[] theaters = null;
            if (Theaters != null)
            {
                int thLen = Theaters.Length;
                theaters = new TheaterType[thLen];
                Array.Copy(Theaters, theaters, thLen);
            }
            BuildingType newBld = new BuildingType(ID, Tilename, DisplayName, PowerProduction, PowerUsage, Storage, bOccupyMask, OwnerHouse, theaters, FactoryOverlay, Flag);
            // Fix this
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
            return Name;
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

            var render = MapRenderer.Render(gameType, theater, Point.Empty, Globals.PreviewTileSize, Globals.PreviewTileScale, mockBuilding);
            if (!render.Item1.IsEmpty)
            {
                var buildingPreview = new Bitmap(render.Item1.Width, render.Item1.Height);
                using (var g = Graphics.FromImage(buildingPreview))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    render.Item2(g);
                    if (IsFake)
                    {
                        ViewTool.RenderBuildingLabels(g, mockBuilding, Point.Empty, Globals.PreviewTileSize, Globals.PreviewTileScale, MapLayerFlag.BuildingFakes, false);
                    }
                }
                Thumbnail = buildingPreview;
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
