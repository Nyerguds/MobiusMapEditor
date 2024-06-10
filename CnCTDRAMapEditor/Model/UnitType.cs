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
using System;
using System.Diagnostics;
using System.Drawing;

namespace MobiusEditor.Model
{
    [Flags]
    public enum UnitTypeFlag
    {
        /// <summary>No flags set.</summary>
        None            /**/ = 0,
        /// <summary>Is a fixed-wing airplane. This affects the default orders for placing it on the map.</summary>
        IsFixedWing     /**/ = 1 << 0,
        /// <summary>Has a turret drawn on the unit.</summary>
        HasTurret       /**/ = 1 << 1,
        /// <summary>Needs to render two turrets.</summary>
        HasDoubleTurret /**/ = 1 << 2,
        /// <summary>Can attack units. This affects the default orders for placing it on the map.</summary>
        IsArmed         /**/ = 1 << 3,
        /// <summary>Can harvest resources. This affects the default orders for placing it on the map.</summary>
        IsHarvester     /**/ = 1 << 4,
        /// <summary>Does not change its colors to that of its owning house..</summary>
        NoRemap         /**/ = 1 << 5,
        /// <summary>Is a unit that is filtered out of the lists if expansion units are disabled.</summary>
        IsExpansionUnit /**/ = 1 << 6,
        /// <summary>Can show a mobile gap area-of-effect radius indicator.</summary>
        IsGapGenerator  /**/ = 1 << 7,
        /// <summary>Can show a radar jamming area-of-effect radius indicator.</summary>
        IsJammer        /**/ = 1 << 8,
    }

    [Flags]
    public enum FrameUsage
    {
        None                /**/ = 0,
        /// <summary>Specifies that this rotation is the full 32 frames. Generally used for ground units, helicopters, turrets, and TD aircraft.</summary>
        Frames32Full        /**/ = 1 << 1,
        /// <summary>Specifies that this rotation is simplified to 16 frames. Generally used for RA boats/aircraft.</summary>
        Frames16Simple      /**/ = 1 << 2,
        /// <summary>Specifies that this rotation is 16 frames, but saved as 8-frame because it is symmetrical and thus the second half of the frames is the same.</summary>
        Frames16Symmetrical /**/ = 1 << 3,
        /// <summary>Specifies that this rotation is cardinal drections only; 8 frames. Generally used for walkers.</summary>
        Frames08Cardinal    /**/ = 1 << 4,
        /// <summary>Specifies that this unit or turret only shows a single frame.</summary>
        Frames01Single      /**/ = 1 << 5,
        /// <summary>Specifies that the unit has special damaged states for 50% and 25% (TD Gunboat)</summary>
        DamageStates        /**/ = 1 << 6,
        /// <summary>Modifier for body frames to determine that there are extra body frames before the turret; adds 4 for air/sea units, and 6 for vehicles.</summary>
        HasUnloadFrames     /**/ = 1 << 7,
        /// <summary>Specifies that this rotation is a rotor (turret only)</summary>
        Rotor               /**/ = 1 << 8,
        /// <summary>Specifies that this turret is on a flatbed on the back of the vehicle and should use special positioning logic.</summary>
        OnFlatBed           /**/ = 1 << 9,

        FrameUsages = Frames01Single | Frames08Cardinal | Frames16Simple | Frames16Symmetrical | Frames32Full,
        FrameModifiers = DamageStates | HasUnloadFrames | Rotor | OnFlatBed
    }

    public class VehicleType : UnitType
    {
        public override bool IsGroundUnit => true;
        public override bool IsAircraft => false;
        public override bool IsVessel => false;
        public override bool IsFixedWing => false;

        public VehicleType(int id, string name, string textId, string ownerHouse, FrameUsage bodyFrameUsage, FrameUsage turrFrameUsage, string turret, string turret2, int turrOffset, int turrY, UnitTypeFlag flags)
            : base(id, name, textId, ownerHouse, bodyFrameUsage, turrFrameUsage, turret, turret2, turrOffset, turrY, flags)
        { }

        public VehicleType(int id, string name, string textId, string ownerHouse, FrameUsage bodyFrameUsage, FrameUsage turrFrameUsage, UnitTypeFlag flags)
            : base(id, name, textId, ownerHouse, bodyFrameUsage, turrFrameUsage, null, null, 0, 0, flags)
        { }

        public VehicleType(int id, string name, string textId, string ownerHouse, FrameUsage bodyFrameUsage, UnitTypeFlag flags)
            : base(id, name, textId, ownerHouse, bodyFrameUsage, FrameUsage.None, null, null, 0, 0, flags)
        { }

        public VehicleType(int id, string name, string textId, string ownerHouse, FrameUsage bodyFrameUsage)
            : base(id, name, textId, ownerHouse, bodyFrameUsage, FrameUsage.None, null, null, 0, 0, UnitTypeFlag.None)
        { }

        public VehicleType(int id, string name, string textId, string ownerHouse, FrameUsage bodyFrameUsage, FrameUsage turrFrameUsage, int turrOffset, int turretY, UnitTypeFlag flags)
            : base(id, name, textId, ownerHouse, bodyFrameUsage, turrFrameUsage, null, null, turrOffset, turretY, flags)
        { }

    }

    public class AircraftType : UnitType
    {
        public override bool IsGroundUnit => false;
        public override bool IsAircraft => true;
        public override bool IsVessel => false;
        public override bool IsFixedWing => this.Flag.HasFlag(UnitTypeFlag.IsFixedWing);

        public AircraftType(int id, string name, string textId, string ownerHouse, FrameUsage bodyFrameUsage, FrameUsage turrFrameUsage, string turret, string turret2, int turrOffset, int turretY, UnitTypeFlag flags)
            : base(id, name, textId, ownerHouse, bodyFrameUsage, turrFrameUsage, turret, turret2, turrOffset, turretY, flags)
        { }

        public AircraftType(int id, string name, string textId, string ownerHouse, FrameUsage bodyFrameUsage, UnitTypeFlag flags)
            : base(id, name, textId, ownerHouse, bodyFrameUsage, FrameUsage.None, null, null, 0, 0, flags)
        { }
    }

    public class VesselType : UnitType
    {
        public override bool IsGroundUnit => false;
        public override bool IsAircraft => false;
        public override bool IsVessel => true;
        public override bool IsFixedWing => false;

        public VesselType(int id, string name, string textId, string ownerHouse, FrameUsage bodyFrameUsage, FrameUsage turrFrameUsage, string turret, string turret2, int turrOffset, int turretY, UnitTypeFlag flags)
            : base(id, name, textId, ownerHouse, bodyFrameUsage, turrFrameUsage, turret, turret2, turrOffset, turretY, flags)
        { }

        public VesselType(int id, string name, string textId, string ownerHouse, FrameUsage bodyFrameUsage, FrameUsage turrFrameUsage, UnitTypeFlag flags)
         : base(id, name, textId, ownerHouse, bodyFrameUsage, turrFrameUsage, null, null, 0, 0, flags)
        {
        }

        public VesselType(int id, string name, string textId, string ownerHouse, FrameUsage bodyFrameUsage)
            : base(id, name, textId, ownerHouse, bodyFrameUsage, FrameUsage.None, null, null, 0, 0, UnitTypeFlag.None)
        {
        }

        public VesselType(int id, string name, string textId, string ownerHouse, FrameUsage bodyFrameUsage, UnitTypeFlag flags)
         : base(id, name, textId, ownerHouse, bodyFrameUsage, FrameUsage.None, null, null, 0, 0, flags)
        {
        }
    }

    [DebuggerDisplay("{Name}")]
    public abstract class UnitType : ICellOverlapper, ICellOccupier, ITechnoType
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public string Turret { get; private set; }
        public string SecondTurret { get; private set; }
        public int TurretOffset { get; private set; }
        public int TurretY { get; private set; }
        public UnitTypeFlag Flag { get; private set; }
        public FrameUsage BodyFrameUsage { get; private set; }
        public FrameUsage TurretFrameUsage { get; private set; }
        public Rectangle OverlapBounds => new Rectangle(-1, -1, 3, 3);
        public bool[,] OpaqueMask => new bool[1, 1] { { true } };
        public bool[,] OccupyMask => new bool[1, 1] { { true } };
        public string OwnerHouse { get; private set; }
        public abstract bool IsGroundUnit { get; }
        public abstract bool IsAircraft { get; }
        public abstract bool IsVessel { get; }
        public abstract bool IsFixedWing { get; }
        public bool HasTurret => this.Flag.HasFlag(UnitTypeFlag.HasTurret);
        public bool HasDoubleTurret => this.Flag.HasFlag(UnitTypeFlag.HasDoubleTurret);
        public bool IsArmed => this.Flag.HasFlag(UnitTypeFlag.IsArmed);
        public bool IsHarvester => this.Flag.HasFlag(UnitTypeFlag.IsHarvester);
        public bool IsExpansionOnly => this.Flag.HasFlag(UnitTypeFlag.IsExpansionUnit);
        public bool CanRemap => !this.Flag.HasFlag(UnitTypeFlag.NoRemap);
        private string nameId;

        public Bitmap Thumbnail { get; set; }

        public UnitType(int id, string name, string textId, string ownerHouse, FrameUsage bodyFrameUsage, FrameUsage turrFrameUsage, string turret, string turret2, int turrOffset, int turretY, UnitTypeFlag flags)
        {
            this.ID = id;
            this.Name = name;
            this.nameId = textId;
            this.OwnerHouse = ownerHouse;
            bool hasTurret = flags.HasFlag(UnitTypeFlag.HasTurret);
            this.Turret = hasTurret ? turret : null;
            this.SecondTurret = hasTurret && flags.HasFlag(UnitTypeFlag.HasDoubleTurret) ? turret2 : null;
            this.TurretOffset = turrOffset;
            this.TurretY = turretY;
            this.Flag = flags;
            this.BodyFrameUsage = bodyFrameUsage;
            this.TurretFrameUsage = turrFrameUsage;
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

        public void Init(GameInfo gameInfo, HouseType house, DirectionType direction)
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
                    RenderInfo render = MapRenderer.RenderUnit(gameInfo, new Point(1, 1), Globals.PreviewTileSize, mockUnit);
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
