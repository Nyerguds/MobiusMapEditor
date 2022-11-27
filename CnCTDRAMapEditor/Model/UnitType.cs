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
    public enum UnitTypeFlag
    {
        None            = 0,
        IsFixedWing     = 1 << 0,
        HasTurret       = 1 << 1,
        HasDoubleTurret = 1 << 2,
        IsArmed         = 1 << 3,
        IsHarvester     = 1 << 4,
        IsExpansionUnit = 1 << 5,
    }

    public static class UnitTypeIDMask
    {
        public const sbyte Aircraft   = 1 << 5;
        public const sbyte Vessel     = 1 << 6;
    }

    public class UnitType : ICellOverlapper, ICellOccupier, ITechnoType, IBrowsableType
    {
        public sbyte ID { get; private set; }
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public string Turret { get; private set; }
        public string SecondTurret { get; private set; }
        public int TurretOffset { get; private set; }
        public int TurretY { get; private set; }
        public UnitTypeFlag Flag { get; private set; }
        public Rectangle OverlapBounds => new Rectangle(-1, -1, 3, 3);
        public bool[,] OccupyMask => new bool[1, 1] { { true } };
        public string OwnerHouse { get; private set; }
        public bool IsGroundUnit => !IsAircraft && !IsVessel;
        public bool IsAircraft => (ID & UnitTypeIDMask.Aircraft) != 0;
        public bool IsVessel => (ID & UnitTypeIDMask.Vessel) != 0;
        public bool HasTurret => (Flag & UnitTypeFlag.HasTurret) == UnitTypeFlag.HasTurret;
        public bool HasDoubleTurret => (Flag & UnitTypeFlag.HasDoubleTurret) == UnitTypeFlag.HasDoubleTurret;
        public bool IsFixedWing => (Flag & UnitTypeFlag.IsFixedWing) == UnitTypeFlag.IsFixedWing;
        public bool IsArmed => (Flag & UnitTypeFlag.IsArmed) == UnitTypeFlag.IsArmed;
        public bool IsHarvester => (Flag & UnitTypeFlag.IsHarvester) == UnitTypeFlag.IsHarvester;
        public bool IsExpansionUnit => (Flag & UnitTypeFlag.IsExpansionUnit) == UnitTypeFlag.IsExpansionUnit;
        private Size _RenderSize;

        public Size GetRenderSize(Size cellSize)
        {
            //RenderSize = new Size(tile.Image.Width / Globals.MapTileScale, tile.Image.Height / Globals.MapTileScale);
            return new Size(_RenderSize.Width * cellSize.Width / Globals.OriginalTileWidth, _RenderSize.Height * cellSize.Height / Globals.OriginalTileHeight);
        }

        public Bitmap Thumbnail { get; set; }

        public UnitType(sbyte id, string name, string textId, string ownerHouse, string turret, string turret2, int turrOffset, int turrY, UnitTypeFlag flags) //bool hasTurret, bool isFixedWing, bool isArmed, bool isHarvester)
        {
            ID = id;
            Name = name;
            DisplayName = Globals.TheGameTextManager[textId] + " (" + Name.ToUpperInvariant() + ")";
            OwnerHouse = ownerHouse;
            bool hasTurret = ((flags & UnitTypeFlag.HasTurret) == UnitTypeFlag.HasTurret);
            Turret = hasTurret ? turret : null;
            SecondTurret = hasTurret && ((flags & UnitTypeFlag.HasDoubleTurret) == UnitTypeFlag.HasDoubleTurret) ? turret2 : null;
            TurretOffset = turrOffset;
            TurretY = turrY;
            Flag = flags;
        }

        public UnitType(sbyte id, string name, string textId, string ownerHouse, int turrOffset, int turrY, UnitTypeFlag flags) //bool hasTurret, bool isFixedWing, bool isArmed, bool isHarvester)
             : this(id, name, textId, ownerHouse, null, null, turrOffset, turrY, flags)
        {
        }

        public UnitType(sbyte id, string name, string textId, string ownerHouse, UnitTypeFlag flags) //bool hasTurret, bool isFixedWing, bool isArmed, bool isHarvester)
             : this(id, name, textId, ownerHouse, null, null, 0, 0, flags)
        {
        }

        public UnitType(sbyte id, string name, string textId, string ownerHouse)
            : this(id, name, textId, ownerHouse, null, null, 0, 0, UnitTypeFlag.None)
        {
        }

        public UnitType(sbyte id, string name, string textId)
            : this(id, name, textId, null, null, null, 0, 0, UnitTypeFlag.None)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is UnitType unit)
            {
                return ReferenceEquals(this, obj) || string.Equals(Name, unit.Name, StringComparison.OrdinalIgnoreCase) && ID == unit.ID;
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
            if (Globals.TheTilesetManager.GetTileData(theater.Tilesets, Name, 0, out Tile tile))
            {
                _RenderSize = tile.Image.Size;
            }
            var mockUnit = new Unit()
            {
                Type = this,
                House = house,
                Strength = 256,
                Direction = direction
            };
            var unitThumbnail = new Bitmap(Globals.PreviewTileWidth * 2, Globals.PreviewTileHeight * 2);
            using (Bitmap bigThumbnail = new Bitmap(Globals.PreviewTileWidth * 3, Globals.PreviewTileHeight * 3))
            {
                using (var g = Graphics.FromImage(bigThumbnail))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    MapRenderer.Render(gameType, theater, new Point(1, 1), Globals.PreviewTileSize, mockUnit).Item2(g);
                }
                using (var g2 = Graphics.FromImage(unitThumbnail))
                {
                    g2.DrawImage(bigThumbnail, new Point(-Globals.PreviewTileWidth / 2, -Globals.PreviewTileHeight / 2));
                }
            }
            Thumbnail = unitThumbnail;
            if (oldImage != null)
            {
                try { oldImage.Dispose(); }
                catch { /* ignore */ }
            }
        }
    }
}
