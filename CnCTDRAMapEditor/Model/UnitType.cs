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
        FixedWing     /**/ = 1 << 0,
        /// <summary>Has a turret drawn on the unit.</summary>
        Turret       /**/ = 1 << 1,
        /// <summary>Needs to render two turrets. Requires a <see cref="UnitType.TurretOffset"/> greater than zero, which will be applied to the second turret with 180° added to the rotated position.</summary>
        DoubleTurret /**/ = 1 << 2,
        /// <summary>Can attack units. This affects the default orders for placing it on the map.</summary>
        Armed         /**/ = 1 << 3,
        /// <summary>Can harvest resources. This affects the default orders for placing it on the map.</summary>
        Harvester     /**/ = 1 << 4,
        /// <summary>Does not change its colors to that of its owning house.</summary>
        NoRemap         /**/ = 1 << 5,
        /// <summary>Uses the buildings remap of the owning House.</summary>
        BuildingRemap   /**/ = 1 << 6,
        /// <summary>Is a unit that is filtered out of the lists if expansion units are disabled.</summary>
        ExpansionOnly /**/ = 1 << 7,
        /// <summary>Can show a mobile gap area-of-effect radius indicator.</summary>
        GapGenerator  /**/ = 1 << 8,
        /// <summary>Can show a radar jamming area-of-effect radius indicator.</summary>
        Jammer        /**/ = 1 << 9,
        /// <summary>This type typically has no rules present in the rules file, and needs specific checks on that.</summary>
        NoRules         /**/ = 1 << 10,
    }

    [Flags]
    public enum FrameUsage
    {
        None                /**/ = 0,
        /// <summary>Specifies that this rotation is the full 32 frames. Generally used for ground units, helicopters, turrets, and TD aircraft.</summary>
        Frames32Full        /**/ = 1 << 1,
        /// <summary>Specifies that this rotation is simplified to 16 frames. Generally used for RA boats/aircraft.</summary>
        Frames16Simple      /**/ = 1 << 2,
        /// <summary>Specifies that this rotation is 16 frames, but saved as 8-frame because it is front-to-back symmetrical and thus the second half of the frames is the same.</summary>
        Frames16Symmetrical /**/ = 1 << 3,
        /// <summary>Specifies that this rotation is cardinal and intercardinal directions only; 8 frames. Generally used for walkers.</summary>
        Frames08Cardinal    /**/ = 1 << 4,
        /// <summary>Specifies that this unit or turret only shows a single frame.</summary>
        Frames01Single      /**/ = 1 << 5,
        /// <summary>Specifies that the unit has special damaged states for 50% and 25% (TD Gunboat)</summary>
        DamageStates        /**/ = 1 << 6,
        /// <summary>Modifier for body frames to determine that there are extra body frames before the turret; adds 4 for air/sea units, and 6 for vehicles.</summary>
        HasUnloadFrames     /**/ = 1 << 7,
        /// <summary>Specifies that this rotation is a rotor (turret only)</summary>
        Rotor               /**/ = 1 << 8,
        /// <summary>Specifies that this turret is on a flatbed on the back of the vehicle and should use special positioning logic. This is a special case that replaces <see cref="UnitType.TurretOffset"/>.</summary>
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
        protected readonly Rectangle overlapFlying = new Rectangle(-1, -2, 3, 4);
        // Flying aircraft are treated as overlapping the entire cell. Since they rotate, no detail analysis is done.
        protected readonly bool[,][] overlapMaskFlying = new bool[2, 1][] { { new bool[] { true, true, true, true, true } }, { new bool[5] } };
        protected readonly Point overlapMaskOffsetFlying = new Point(0, -1);

        /// <summary>If enabled, this aircraft will be rendered as flying.</summary>
        public override bool IsGroundUnit => false;
        public override bool IsAircraft => true;
        public override bool IsFlying { get; set; }
        public override bool IsVessel => false;
        public override bool IsFixedWing => Flags.HasFlag(UnitTypeFlag.FixedWing);
        public override int ZOrder => IsFlying ? Globals.ZOrderFlying : base.ZOrder;
        public override Rectangle OverlapBounds => IsFlying ? overlapFlying : base.OverlapBounds;
        public override bool[,][] OverlapMask => IsFlying ? overlapMaskFlying : base.OverlapMask;
        public override Point OverlapMaskOffset => IsFlying ? overlapMaskOffsetFlying : base.OverlapMaskOffset;

        public AircraftType(int id, string name, string textId, string ownerHouse, FrameUsage bodyFrameUsage, FrameUsage turrFrameUsage, string turret, string turret2, int turrOffset, int turretY, UnitTypeFlag flags)
            : base(id, name, textId, ownerHouse, bodyFrameUsage, turrFrameUsage, turret, turret2, turrOffset, turretY, flags)
        {
            IsFlying = Flags.HasFlag(UnitTypeFlag.FixedWing);
        }

        public AircraftType(int id, string name, string textId, string ownerHouse, FrameUsage bodyFrameUsage, UnitTypeFlag flags)
            : this(id, name, textId, ownerHouse, bodyFrameUsage, FrameUsage.None, null, null, 0, 0, flags)
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
        public bool Ownable => true;
        public string DisplayName { get; private set; }
        public string NameOverride { get; set; }
        public string Turret { get; private set; }
        public string SecondTurret { get; private set; }
        public int TurretOffset { get; private set; }
        public int TurretY { get; private set; }
        public UnitTypeFlag Flags { get; private set; }
        public FrameUsage BodyFrameUsage { get; private set; }
        public FrameUsage TurretFrameUsage { get; private set; }
        public virtual Rectangle OverlapBounds => new Rectangle(-1, -1, 3, 3);
        // Units are big enough to be visible even when partially overlapped, so they only count as overlapped if their center is overlapped.
        public virtual bool[,][] OverlapMask => new bool[1, 1][] { { new bool[] { true, false, false, false, false } } };
        public virtual Point OverlapMaskOffset => Point.Empty;
        public virtual bool[,][] ContentMask => OverlapMask;
        public virtual Point ContentMaskOffset => OverlapMaskOffset;
        public virtual bool[,] OccupyMask => new bool[1, 1] { { true } };
        public virtual bool[,] BaseOccupyMask => new bool[1, 1] { { true } };
        public virtual int ZOrder => Globals.ZOrderDefault;
        public string OwnerHouse { get; private set; }
        public abstract bool IsGroundUnit { get; }
        public abstract bool IsAircraft { get; }
        public virtual bool IsFlying { get { return false; } set { } }
        public abstract bool IsVessel { get; }
        public abstract bool IsFixedWing { get; }
        /// <summary>Has a turret drawn on the unit.</summary>
        public bool HasTurret => Flags.HasFlag(UnitTypeFlag.Turret);
        /// <summary>Needs to render two turrets. Requires a <see cref="UnitType.TurretOffset"/> greater than zero, which will be applied to the second turret with 180° added to the rotated position.</summary>
        public bool HasDoubleTurret => Flags.HasFlag(UnitTypeFlag.DoubleTurret);
        public bool IsArmed => Flags.HasFlag(UnitTypeFlag.Armed);
        public bool IsHarvester => Flags.HasFlag(UnitTypeFlag.Harvester);
        public bool CanRemap => !Flags.HasFlag(UnitTypeFlag.NoRemap);
        /// <summary>Uses the buildings remap of the owning House.</summary>
        public bool BuildingRemap => Flags.HasFlag(UnitTypeFlag.BuildingRemap);
        public string ImageOverride { get; set; }
        public bool IsExpansionOnly => Flags.HasFlag(UnitTypeFlag.ExpansionOnly);
        /// <summary>Can show a mobile gap area-of-effect radius indicator.</summary>
        public bool IsGapGenerator => Flags.HasFlag(UnitTypeFlag.GapGenerator);
        /// <summary>Can show a radar jamming area-of-effect radius indicator.</summary>
        public bool IsJammer => Flags.HasFlag(UnitTypeFlag.Jammer);
        /// <summary>This type typically has no rules present in the rules file, and needs specific checks on that.</summary>
        public bool HasNoRules => Flags.HasFlag(UnitTypeFlag.NoRules);

        public bool GraphicsFound { get; private set; }
        private string nameId;

        public Bitmap Thumbnail { get; set; }

        public UnitType(int id, string name, string textId, string ownerHouse, FrameUsage bodyFrameUsage, FrameUsage turrFrameUsage, string turret, string turret2, int turrOffset, int turretY, UnitTypeFlag flags)
        {
            ID = id;
            Name = name;
            nameId = textId;
            OwnerHouse = ownerHouse;
            bool hasTurret = flags.HasFlag(UnitTypeFlag.Turret);
            Turret = hasTurret ? turret : null;
            SecondTurret = hasTurret && flags.HasFlag(UnitTypeFlag.DoubleTurret) ? turret2 : null;
            TurretOffset = turrOffset;
            TurretY = turretY;
            Flags = flags;
            BodyFrameUsage = bodyFrameUsage;
            TurretFrameUsage = turrFrameUsage;
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

        public void InitDisplayName()
        {
            string str = null;
            bool hasString = !String.IsNullOrEmpty(nameId) && !String.IsNullOrEmpty(str = Globals.TheGameTextManager[nameId]);
            bool hasOverride = !String.IsNullOrEmpty(NameOverride);
            if (!hasString && !hasOverride)
            {
                DisplayName = Name.ToUpperInvariant();
                return;
            }
            DisplayName = (hasOverride ? NameOverride : str)
                + " (" + Name.ToUpperInvariant() + ")";
        }

        public void Init(GameInfo gameInfo, HouseType house, DirectionType direction)
        {
            InitDisplayName();
            Bitmap oldImage = Thumbnail;
            Unit mockUnit = new Unit()
            {
                Type = this,
                House = house,
                Strength = 256,
                Direction = direction
            };
            if (IsAircraft)
            {
                // Initialise this from GameInfo.
                IsFlying = IsFixedWing || !gameInfo.LandedHelis;
            }
            // Renderer draws a border of a full cell around the unit. In practice this is excessive,
            // so for a nicer preview we use only half a cell around.
            Bitmap unitThumbnail = new Bitmap(Globals.PreviewTileWidth * 2, Globals.PreviewTileHeight * 2);
            unitThumbnail.SetResolution(96, 96);
            bool flying = IsAircraft && IsFlying;
            // Temporarily disable this for the thumbnail.
            if (flying)
            {
                IsFlying = false;
            }
            using (Bitmap bigThumbnail = new Bitmap(Globals.PreviewTileWidth * 3, Globals.PreviewTileHeight * 3))
            {
                bigThumbnail.SetResolution(96, 96);
                using (Graphics g = Graphics.FromImage(bigThumbnail))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    
                    RenderInfo render = MapRenderer.RenderUnit(gameInfo, null, new Point(1, 1), Globals.PreviewTileSize, mockUnit, false);
                    if (render.RenderedObject != null)
                    {
                        render.RenderAction(g);
                    }
                    GraphicsFound = !render.IsDummy;
                }
                using (Graphics g2 = Graphics.FromImage(unitThumbnail))
                {
                    g2.DrawImage(bigThumbnail, new Point(-Globals.PreviewTileWidth / 2, -Globals.PreviewTileHeight / 2));
                }
            }
            if (flying)
            {
                IsFlying = true;
            }
            Thumbnail = unitThumbnail;
            if (oldImage != null)
            {
                try { oldImage.Dispose(); }
                catch { /* ignore */ }
            }
        }
        public void Reset()
        {
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
