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
using System.Drawing;

namespace MobiusEditor.Model
{
    [Flags]
    public enum WaypointFlag
    {
        None = 0,
        Flare        /**/ = 1 << 0,
        Home         /**/ = 1 << 1,
        Reinforce    /**/ = 1 << 2,
        Special      /**/ = 1 << 3,
        CrateSpawn   /**/ = 1 << 4,
        PlayerStart  /**/ = 1 << 5,
        // Never referenced, but used internally by the flags system. These eight must be reserved on the bits directly following PlayerStart.
        PlayerStart1 /**/ = PlayerStart | 1 << 6,
        PlayerStart2 /**/ = PlayerStart | 1 << 7,
        PlayerStart3 /**/ = PlayerStart | 1 << 8,
        PlayerStart4 /**/ = PlayerStart | 1 << 9,
        PlayerStart5 /**/ = PlayerStart | 1 << 10,
        PlayerStart6 /**/ = PlayerStart | 1 << 11,
        PlayerStart7 /**/ = PlayerStart | 1 << 12,
        PlayerStart8 /**/ = PlayerStart | 1 << 13,
    }

    public class Waypoint : INamedType
    {
        public static WaypointFlag GetFlagForMpId(int mpId)
        {
            if (mpId < 0)
            {
                return WaypointFlag.None;
            }
            return WaypointFlag.PlayerStart | (WaypointFlag)((int)WaypointFlag.PlayerStart << (mpId + 1));
        }

        public static int GetMpIdFromFlag(WaypointFlag flag)
        {
            if ((flag & WaypointFlag.PlayerStart) != WaypointFlag.PlayerStart)
            {
                return -1;
            }
            int pls = (int)WaypointFlag.PlayerStart;
            int flagId = ((int)flag & ~pls) / (pls << 1);
            if (flagId == 0)
            {
                return -1;
            }
            int mpId = 0;
            // Find which multiplayer house number it has.
            while (flagId > 1)
            {
                flagId >>= 1;
                mpId++;
            }
            return mpId;
        }

        public static readonly string None = "None";

        public CellMetrics Metrics { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }

        public WaypointFlag Flag { get; set; }

        public int? Cell { get; set; }
        public Point? Point => !Cell.HasValue || Cell == -1 || Metrics == null ? null : (Point?)new Point(Cell.Value % Metrics.Width, Cell.Value / Metrics.Width);

        public bool IsPreview { get; set; }

        public Waypoint(string name, string shortName, WaypointFlag flag, CellMetrics metrics, int? cell)
        {
            Metrics = metrics;
            Name = name;
            ShortName = shortName;
            Flag = flag;
            Cell = cell;
        }

        public Waypoint(string name, WaypointFlag flag, CellMetrics metrics, int? cell)
            : this (name, name, flag, metrics, cell)
        {
        }

        public Waypoint(string name, string shortName, WaypointFlag flag, CellMetrics metrics)
            : this(name, shortName, flag, metrics, null)
        {
        }

        public Waypoint(string name, WaypointFlag flag, CellMetrics metrics)
            : this(name, name, flag, metrics, null)
        {
        }

        public Waypoint(string name, CellMetrics metrics)
            : this(name, name, WaypointFlag.None, metrics, null)
        {
        }

        public Waypoint(string name, string shortName, WaypointFlag flag)
            : this(name, shortName, flag, null, null)
        {
        }

        public Waypoint(string name, WaypointFlag flag)
            : this(name, name, flag, null, null)
        {
        }

        public Waypoint(string name)
            : this(name, name, WaypointFlag.None, null, null)
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
