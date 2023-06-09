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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace MobiusEditor.Model
{
    [System.Diagnostics.DebuggerDisplay("{Type.Name}")]
    public class Smudge: ICellOverlapper, INotifyPropertyChanged, ICloneable, IEquatable<Smudge>
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private SmudgeType type;
        public SmudgeType Type { get => type; set => SetField(ref type, value); }

        private int icon;
        public int Icon { get => icon; set => SetField(ref icon, value); }

        public Color Tint { get; set; } = Color.White;

        public Rectangle OverlapBounds => new Rectangle(Point.Empty, Type.Size);
        public bool[,] OpaqueMask => new bool[1, 1] { { true } };

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

        public override string ToString()
        {
            return Type?.Name ?? "Unknown";
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        protected Smudge()
        {
        }

        public Smudge(SmudgeType type)
        {
            this.type = type;
        }

        public Smudge(SmudgeType type, int icon)
        {
            this.type = type;
            this.icon = icon;
        }

        public Smudge(SmudgeType type, int icon, Color tint)
        {
            this.type = type;
            this.icon = icon;
            this.Tint = tint;
        }

        public Smudge Clone()
        {
            return new Smudge(this.Type, this.Icon, this.Tint);
        }

        public void CloneDataFrom(Smudge other)
        {
            Type = other.Type;
            Icon = other.Icon;
            Tint = other.Tint;
        }

        public Boolean Equals(Smudge other)
        {
            return this.Type == other.Type && this.Icon == other.Icon;
        }

        /// <summary>
        /// Calculates the point at which the smudge's origin point is
        /// placed, if the current icon is placed on the given point.
        /// </summary>
        /// <param name="cell">Point of the current smudge object.</param>
        /// <returns>The point where icon 0 of this smudge is located.</returns>
        public Point GetPlacementOrigin(Point point)
        {
            if (!this.Type.IsMultiCell)
                return point;
            int x = this.Icon % this.Type.Size.Width;
            int y = this.Icon / this.Type.Size.Width;
            return new Point(point.X - x, point.Y - y);
        }

        /// <summary>
        /// Calculates the cell at which the smudge's origin point is
        /// placed, if the current icon is placed on the given cell.
        /// </summary>
        /// <param name="cell">Cell of the current smudge object.</param>
        /// <param name="metrics">Cell metrics used to calculate cell numbers.</param>
        /// <returns>The cell where icon 0 of this smudge is located.</returns>
        public Int32 GetPlacementOrigin(int cell, CellMetrics metrics)
        {
            if (!this.Type.IsMultiCell)
                return cell;
            int x = this.Icon % this.Type.Size.Width;
            int y = this.Icon / this.Type.Size.Width;
            return cell - x - metrics.Width * y;
        }
    }
}
