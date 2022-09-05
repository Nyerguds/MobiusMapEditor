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
using System.Drawing;

namespace MobiusEditor.Model
{
    [Flags]
    public enum WaypointFlag
    {
        None = 0,
        PlayerStart = 1 << 0,
        Flare       = 1 << 1,
        Home        = 1 << 2,
        Reinforce   = 1 << 3,
        Special     = 1 << 4
    }

    public class Waypoint : INamedType
    {
        public static readonly string None = "None";

        public CellMetrics Metrics { get; set; }
        public string Name { get; set; }

        public WaypointFlag Flag { get; set; }

        public int? Cell { get; set; }
        public Point Point { get; set; }

        public Waypoint(string name, WaypointFlag flag, CellMetrics metrics)
        {
            this.Metrics = metrics;
            Name = name;
            Flag = flag;
        }

        public Waypoint(string name, CellMetrics metrics)
            : this(name, WaypointFlag.None, metrics)
        {
        }
        public Waypoint(string name, WaypointFlag flag)
            : this(name, flag, null)
        {
        }
        public Waypoint(string name)
            : this(name, WaypointFlag.None, null)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is Waypoint wp)
            {
                return ReferenceEquals(this, wp) || string.Equals(this.Name, wp.Name, StringComparison.OrdinalIgnoreCase) && this.Cell == wp.Cell;
            }
            else if (obj is string str)
            {
                return string.Equals(Name, str, StringComparison.OrdinalIgnoreCase);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            string location = "-";
            if (Cell.HasValue) {
                if (Metrics == null)
                {
                    location = Cell.Value.ToString();
                }
                else if (Metrics.GetLocation(Cell.Value, out Point loc))
                {
                    location = String.Format("{0},{1}", loc.X, loc.Y);
                }
            }
            return String.Format("{0} [{1}]", Name, location);
        }
    }
}
