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
    [DebuggerDisplay("{Name}")]
    public class InfantryType : ITechnoType
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public string GraphicsSource { get; set; }
        public string DisplayName { get; private set; }
        public string OwnerHouse { get; private set; }
        public UnitTypeFlag Flag { get; private set; }
        public bool IsArmed => (this.Flag & UnitTypeFlag.IsArmed) == UnitTypeFlag.IsArmed;
        public bool IsAircraft => false;
        public bool IsFixedWing => false;
        public bool IsExpansionOnly => (this.Flag & UnitTypeFlag.IsExpansionUnit) == UnitTypeFlag.IsExpansionUnit;
        public bool IsHarvester => false;
        public bool CanRemap => (this.Flag & UnitTypeFlag.NoRemap) != UnitTypeFlag.NoRemap;
        public string ClassicGraphicsSource { get; set; }
        public Byte[] ClassicGraphicsRemap { get; set; }

        public Bitmap Thumbnail { get; set; }
        private string nameId;

        public InfantryType(int id, string name, string textId, string ownerHouse, string remappedFrom, byte[] remapTable, UnitTypeFlag flags)
        {
            this.ID = id;
            this.Name = name;
            this.GraphicsSource = name;
            this.nameId = textId;
            this.OwnerHouse = ownerHouse;
            this.Flag = flags;
            this.ClassicGraphicsSource = remappedFrom;
            this.ClassicGraphicsRemap = remapTable;
        }

        public InfantryType(int id, string name, string textId, string ownerHouse, UnitTypeFlag flags)
            : this(id, name, textId, ownerHouse, null, null, flags)
        {
        }
        public InfantryType(int id, string name, string textId, string ownerHouse)
        : this(id, name, textId, ownerHouse, null, null, UnitTypeFlag.None)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is InfantryType)
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
            return this.Name;
        }


        public void InitDisplayName()
        {
            this.DisplayName = !String.IsNullOrEmpty(this.nameId) && !String.IsNullOrEmpty(Globals.TheGameTextManager[this.nameId])
                ? Globals.TheGameTextManager[this.nameId] + " (" + this.Name.ToUpperInvariant() + ")"
                : this.Name.ToUpperInvariant();
        }

        public void Init(HouseType house, DirectionType direction)
        {
            this.InitDisplayName();
            Bitmap oldImage = this.Thumbnail;
            // Initialisation for the special RA civilian remapping logic.
            if (((this.ClassicGraphicsSource != null && !String.Equals(this.Name, this.ClassicGraphicsSource, StringComparison.OrdinalIgnoreCase))
                || this.ClassicGraphicsRemap != null) && Globals.TheTilesetManager is TilesetManagerClassic tsmc)
            {
                string actualSprite = this.ClassicGraphicsSource ?? this.Name;
                // Use special override that 100% makes sure previously-cached versions are cleared, so previous accidental fetches do not corrupt
                this.GraphicsSource = actualSprite + " (override for infantry "+ this.Name + ")";
                tsmc.GetTeamColorTileData(this.GraphicsSource, 0, null, out _, true, false, true, actualSprite, this.ClassicGraphicsRemap, true);
            }
            Infantry mockInfantry = new Infantry(null)
            {
                Type = this,
                House = house,
                Strength = 256,
                Direction = direction
            };
            Bitmap infantryThumbnail = new Bitmap(Globals.PreviewTileWidth, Globals.PreviewTileHeight);
            using (Graphics g = Graphics.FromImage(infantryThumbnail))
            {
                MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                RenderInfo render = MapRenderer.RenderInfantry(Point.Empty, Globals.PreviewTileSize, mockInfantry, InfantryStoppingType.Center);
                if (render.RenderedObject != null)
                {
                    render.RenderAction(g);
                }
            }
            this.Thumbnail = infantryThumbnail;
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
