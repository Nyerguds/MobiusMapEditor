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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace MobiusEditor.Model
{
    [DebuggerDisplay("{Type}: {Trigger}")]
    public class Building : ITechno, ICellOverlapper, ICellOccupier, INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private BuildingType type;
        public BuildingType Type { get => type; set => SetField(ref type, value); }
        public ITechnoType TechnoType => type;

        public Rectangle OverlapBounds => Type.OverlapBounds;
        public bool[,] OpaqueMask => Type.OpaqueMask;
        public bool[,] OccupyMask => Type.OccupyMask;
        public bool[,] BaseOccupyMask => Type.BaseOccupyMask;
        public int ZOrder => Type.ZOrder;
        public int DrawOrderCache { get; set; }
        public Size Size => type.Size;

        private HouseType house;
        public HouseType House { get => house; set => SetField(ref house, value); }

        private int strength = 256;
        public int Strength { get => strength; set => SetField(ref strength, value); }

        private DirectionType direction;
        public DirectionType Direction { get => direction; set => SetField(ref direction, value); }

        private string trigger = Model.Trigger.None;
        public string Trigger { get => trigger; set => SetField(ref trigger, value); }

        private int basePriority = -1;
        public int BasePriority { get => basePriority; set => SetField(ref basePriority, value); }

        private bool isPrebuilt = true;
        public bool IsPrebuilt { get => isPrebuilt; set => SetField(ref isPrebuilt, value); }

        private bool sellable;
        public bool Sellable { get => sellable; set => SetField(ref sellable, value); }

        private bool rebuild;
        public bool Rebuild { get => rebuild; set => SetField(ref rebuild, value); }

        public ISet<int> BibCells { get; private set; } = new HashSet<int>();

        public bool IsPreview { get; set; }

        public override string ToString()
        {
            return Type?.Name ?? "Unknown";
        }

        public Building Clone()
        {
            Building clone = new Building();
            clone.CloneDataFrom(this);
            return clone;
        }

        public void CloneDataFrom(Building other)
        {
            Type = other.Type;
            House = other.House;
            Strength = other.Strength;
            Direction = other.Direction;
            Trigger = other.Trigger;
            BasePriority = other.BasePriority;
            IsPrebuilt = other.IsPrebuilt;
            Sellable = other.Sellable;
            Rebuild = other.Rebuild;
            DrawOrderCache = other.DrawOrderCache;
        }

        public Dictionary<Point, Smudge> GetBib(Point location, List<SmudgeType> smudgeTypes)
        {
            if (!Type.HasBib)
            {
                return null;
            }
            SmudgeType bibType = SmudgeType.GetBib(smudgeTypes, Type.Size.Width);
            // Theaters don't actually matter; invisible bibs in Interior are still seen as bibs.
            if (bibType == null)// || bibType.Theaters != null && !bibType.Theaters.Contains(theater))
            {
                return null;
            }
            Dictionary<Point, Smudge> bibCells = new Dictionary<Point, Smudge>();
            int icon = 0;
            for (var y = 0; y < bibType.Size.Height; ++y)
            {
                for (var x = 0; x < bibType.Size.Width; ++x, ++icon)
                {
                    Point loc = new Point(location.X + x, location.Y + Type.Size.Height + y - 1);
                    Smudge bibCell = new Smudge(bibType, icon, this);
                    bibCells[loc] = bibCell;
                }
            }
            return bibCells;
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        object ICloneable.Clone()
        {
            return Clone();
        }

        public Boolean DataEquals(Building other)
        {
            return this.Type == other.Type &&
                this.House == other.House &&
                this.Strength == other.Strength &&
                this.Direction == other.Direction &&
                this.Trigger == other.Trigger &&
                this.BasePriority == other.BasePriority &&
                this.IsPrebuilt == other.IsPrebuilt &&
                this.Sellable == other.Sellable &&
                this.Rebuild == other.Rebuild;
        }
    }
}
