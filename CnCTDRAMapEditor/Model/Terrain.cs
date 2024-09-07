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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using MobiusEditor.Interface;

namespace MobiusEditor.Model
{
    [DebuggerDisplay("{Type}: {Trigger}")]
    public class Terrain : ITechno, ICellOverlapper, ICellOccupier, INotifyPropertyChanged, ICloneable, IEquatable<Terrain>
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private TerrainType type;
        public TerrainType Type { get => type; set => SetField(ref type, value); }
        public ITechnoType TechnoType => type;

        public Rectangle OverlapBounds => Type.OverlapBounds;
        public bool[,][] OpaqueMask => Type.OpaqueMask;

        public bool[,] OccupyMask => Type.OccupyMask;
        public bool[,] BaseOccupyMask => Type.OccupyMask;
        public int ZOrder => Type.ZOrder;
        public int DrawOrderCache { get; set; }
        public int DrawFrameCache { get; set; }

        private string trigger = Model.Trigger.None;
        public string Trigger { get => trigger; set => SetField(ref trigger, value); }

        public DirectionType Direction { get { return null; } set { } }

        // Terrain has no House; ignore this.
        public HouseType House
        {
            get { return null; }
            set { }
        }

        private int strength = 256;
        public int Strength { get => strength; set => SetField(ref strength, value); }

        public bool IsPreview { get; set; }

        public override string ToString()
        {
            return Type?.Name ?? "Unknown";
        }

        public Terrain Clone()
        {
            Terrain clone = new Terrain();
            clone.CloneDataFrom(this);
            return clone;
        }

        public void CloneDataFrom(Terrain other)
        {
            Type = other.Type;
            Strength = other.Strength;
            Trigger = other.Trigger;
            DrawOrderCache = other.DrawOrderCache;
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

        public Boolean Equals(Terrain other)
        {
            return this.Type == other.Type &&
                this.Strength == other.Strength &&
                this.Trigger == other.Trigger;
        }
    }
}
