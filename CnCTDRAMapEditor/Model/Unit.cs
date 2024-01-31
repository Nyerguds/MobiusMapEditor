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
using System.Drawing;
using System.Runtime.CompilerServices;

namespace MobiusEditor.Model
{
    public class Unit : ITechno, ICellOverlapper, ICellOccupier, INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private UnitType type;
        public UnitType Type { get => type; set => SetField(ref type, value); }
        public ITechnoType TechnoType => type;

        public Rectangle OverlapBounds => Type.OverlapBounds;
        public bool[,] OpaqueMask => Type.OpaqueMask;

        public bool[,] OccupyMask => Type.OccupyMask;

        private HouseType house;
        public HouseType House { get => house; set => SetField(ref house, value); }

        private int strength = 256;
        public int Strength { get => strength; set => SetField(ref strength, value); }

        private DirectionType direction;
        public DirectionType Direction { get => direction; set => SetField(ref direction, value); }

        private string mission;
        public string Mission { get => mission; set => SetField(ref mission, value); }

        private string trigger = Model.Trigger.None;
        public string Trigger { get => trigger; set => SetField(ref trigger, value); }

        public Color Tint { get; set; } = Color.White;
        public bool IsPreview { get; set; }

        public override string ToString()
        {
            return Type?.Name ?? "Unknown";
        }

        public Unit Clone()
        {
            Unit clone = new Unit();
            clone.CloneDataFrom(this);
            return clone;
        }

        public void CloneDataFrom(Unit other)
        {
            Type = other.Type;
            House = other.House;
            Strength = other.Strength;
            Direction = other.Direction;
            Mission = other.Mission;
            Trigger = other.Trigger;
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

        public Boolean DataEquals(Unit other)
        {
            return this.Type == other.Type &&
                this.House == other.House &&
                this.Strength == other.Strength &&
                this.Direction == other.Direction &&
                this.Mission == other.Mission &&
                this.Trigger == other.Trigger;
        }
    }
}
