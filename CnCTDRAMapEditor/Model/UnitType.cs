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
        /// <summary>No flags set.</summary>
        None            = 0,
        /// <summary>Is a fixed-wing airplane. This treats it as 16-frame rotation, and affects the default orders for placing it on the map.</summary>
        IsFixedWing     = 1 << 0,
        /// <summary>Has a turret drawn on the unit.</summary>
        HasTurret       = 1 << 1,
        /// <summary>Needs to render two turrets.</summary>
        HasDoubleTurret = 1 << 2,
        /// <summary>Can attack units. This affects the default orders for placing it on the map.</summary>
        IsArmed         = 1 << 3,
        /// <summary>Can harvest resources. This affects the default orders for placing it on the map.</summary>
        IsHarvester     = 1 << 4,
        /// <summary>Does not change its colors to that of its owning house..</summary>
        NoRemap         = 1 << 5,
        /// <summary>Is a unit that is filtered out of the lists if expansion units are disabled.</summary>
        IsExpansionUnit = 1 << 6,
        /// <summary>Can show a mobile gap area-of-effect radius indicator.</summary>
        IsGapGenerator  = 1 << 7,
        /// <summary>Can show a radar jamming area-of-effect radius indicator.</summary>
        IsJammer        = 1 << 8,
    }

    public static class UnitTypeIDMask
    {
        public const sbyte Aircraft   = 1 << 5;
        public const sbyte Vessel     = 1 << 6;
    }

    public class UnitType : ICellOverlapper, ICellOccupier, ITechnoType
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
        public bool[,] OpaqueMask => new bool[1, 1] { { true } };
        public bool[,] OccupyMask => new bool[1, 1] { { true } };
        public string OwnerHouse { get; private set; }
        public bool IsGroundUnit => !this.IsAircraft && !this.IsVessel;
        public bool IsAircraft => (this.ID & UnitTypeIDMask.Aircraft) != 0;
        public bool IsVessel => (this.ID & UnitTypeIDMask.Vessel) != 0;
        public bool HasTurret => (this.Flag & UnitTypeFlag.HasTurret) == UnitTypeFlag.HasTurret;
        public bool HasDoubleTurret => (this.Flag & UnitTypeFlag.HasDoubleTurret) == UnitTypeFlag.HasDoubleTurret;
        public bool IsFixedWing => (this.Flag & UnitTypeFlag.IsFixedWing) == UnitTypeFlag.IsFixedWing;
        public bool IsArmed => (this.Flag & UnitTypeFlag.IsArmed) == UnitTypeFlag.IsArmed;
        public bool IsHarvester => (this.Flag & UnitTypeFlag.IsHarvester) == UnitTypeFlag.IsHarvester;
        public bool IsExpansionOnly => (this.Flag & UnitTypeFlag.IsExpansionUnit) == UnitTypeFlag.IsExpansionUnit;
        public bool CanRemap => (this.Flag & UnitTypeFlag.NoRemap) != UnitTypeFlag.NoRemap;
        private string nameId;

        public Bitmap Thumbnail { get; set; }

        public UnitType(sbyte id, string name, string textId, string ownerHouse, string turret, string turret2, int turrOffset, int turrY, UnitTypeFlag flags) //bool hasTurret, bool isFixedWing, bool isArmed, bool isHarvester)
        {
            this.ID = id;
            this.Name = name;
            this.nameId = textId;
            this.OwnerHouse = ownerHouse;
            bool hasTurret = ((flags & UnitTypeFlag.HasTurret) == UnitTypeFlag.HasTurret);
            this.Turret = hasTurret ? turret : null;
            this.SecondTurret = hasTurret && ((flags & UnitTypeFlag.HasDoubleTurret) == UnitTypeFlag.HasDoubleTurret) ? turret2 : null;
            this.TurretOffset = turrOffset;
            this.TurretY = turrY;
            this.Flag = flags;
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
                return ReferenceEquals(this, obj) || string.Equals(this.Name, unit.Name, StringComparison.OrdinalIgnoreCase) && this.ID == unit.ID;
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
            return this.Name;
        }

        public void InitDisplayName()
        {
            this.DisplayName = !String.IsNullOrEmpty(this.nameId) && !String.IsNullOrEmpty(Globals.TheGameTextManager[this.nameId])
                ? Globals.TheGameTextManager[this.nameId] + " (" + this.Name.ToUpperInvariant() + ")"
                : this.Name.ToUpperInvariant();
        }

        public void Init(GameType gameType, HouseType house, DirectionType direction)
        {
            InitDisplayName();
            Bitmap oldImage = this.Thumbnail;
            Unit mockUnit = new Unit()
            {
                Type = this,
                House = house,
                Strength = 256,
                Direction = direction
            };
            // Renderer draws a border of a full cell around the unit. In practice this is excessive,
            // so for a nicer preview we use only half a cell around.
            Bitmap unitThumbnail = new Bitmap(Globals.PreviewTileWidth * 2, Globals.PreviewTileHeight * 2);
            unitThumbnail.SetResolution(96, 96);
            using (Bitmap bigThumbnail = new Bitmap(Globals.PreviewTileWidth * 3, Globals.PreviewTileHeight * 3))
            {
                bigThumbnail.SetResolution(96, 96);
                using (Graphics g = Graphics.FromImage(bigThumbnail))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    RenderInfo render = MapRenderer.RenderUnit(gameType, new Point(1, 1), Globals.PreviewTileSize, mockUnit);
                    if (render.RenderedObject != null)
                    {
                        render.RenderAction(g);
                    }
                }
                using (Graphics g2 = Graphics.FromImage(unitThumbnail))
                {
                    g2.DrawImage(bigThumbnail, new Point(-Globals.PreviewTileWidth / 2, -Globals.PreviewTileHeight / 2));
                }
            }
            this.Thumbnail = unitThumbnail;
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
