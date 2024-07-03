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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace MobiusEditor.Model
{
    public enum FacingType
    {
        None      /**/ = -1,
        North     /**/ = 0,
        NorthEast /**/ = 1,
        East      /**/ = 2,
        SouthEast /**/ = 3,
        South     /**/ = 4,
        SouthWest /**/ = 5,
        West      /**/ = 6,
        NorthWest /**/ = 7
    }

    public class CellChangedEventArgs<T> : EventArgs
    {
        public readonly int Cell;

        public readonly Point Location;

        public readonly T OldValue;

        public readonly T Value;

        public CellChangedEventArgs(CellMetrics metrics, int cell, T oldValue, T value)
        {
            Cell = cell;
            metrics.GetLocation(cell, out Location);
            OldValue = oldValue;
            Value = value;
        }

        public CellChangedEventArgs(CellMetrics metrics, Point location, T oldValue, T value)
        {
            Location = location;
            metrics.GetCell(location, out Cell);
            OldValue = oldValue;
            Value = value;
        }
    }

    public class CellGrid<T> : IEnumerable<(int Cell, T Value)>, IEnumerable
    {
        private readonly CellMetrics metrics;
        public CellMetrics Metrics => metrics;

        private readonly T[,] cells;

        public T this[int y, int x]
        {
            get => metrics.Contains(x, y) ? cells[y, x] : default(T);
            set
            {
                if (metrics.Contains(x, y) && !EqualityComparer<T>.Default.Equals(cells[y, x], value))
                {
                    var lastValue = cells[y, x];
                    cells[y, x] = value;
                    OnCellChanged(new CellChangedEventArgs<T>(metrics, new Point(x, y), lastValue, cells[y, x]));
                }
            }
        }

        public T this[Point location] { get => this[location.Y, location.X]; set => this[location.Y, location.X] = value; }

        public T this[int cell] { get => this[cell / metrics.Width, cell % metrics.Width]; set => this[cell / metrics.Width, cell % metrics.Width] = value; }

        public Size Size => metrics.Size;

        public int Length => metrics.Length;

        public event EventHandler<CellChangedEventArgs<T>> CellChanged;
        public event EventHandler<EventArgs> Cleared;

        public CellGrid(CellMetrics metrics)
        {
            this.metrics = metrics;
            cells = new T[metrics.Height, metrics.Width];
        }

        public void Clear()
        {
            Array.Clear(cells, 0, cells.Length);
            OnCleared();
        }

        public int CellNumberOf(T obj)
        {
            for (int y = 0; y < metrics.Height; ++y)
            {
                for (int x = 0; x < metrics.Width; ++x)
                {
                    if (cells[y, x].Equals(obj))
                        return y * metrics.Width + x;
                }
            }
            return -1;
        }


        public T Adjacent(Point location, FacingType facing)
        {
            return metrics.Adjacent(location, facing, out Point adjacent) ? this[adjacent] : default;
        }

        public T Adjacent(int cell, FacingType facing)
        {
            if (!metrics.GetLocation(cell, out Point location))
            {
                return default;
            }
            return metrics.Adjacent(location, facing, out Point adjacent) ? this[adjacent] : default;
        }

        public bool CopyTo(CellGrid<T> other)
        {
            if (metrics.Length != other.metrics.Length)
            {
                return false;
            }

            for (var i = 0; i < metrics.Length; ++i)
            {
                other[i] = this[i];
            }

            return true;
        }

        protected virtual void OnCellChanged(CellChangedEventArgs<T> e)
        {
            CellChanged?.Invoke(this, e);
        }

        protected virtual void OnCleared()
        {
            Cleared?.Invoke(this, new EventArgs());
        }

        public IEnumerable<(int Cell, T Value)> IntersectsWithCells(ISet<int> cells)
        {
            foreach (var i in cells)
            {
                if (metrics.Contains(i))
                {
                    var cell = this[i];
                    if (cell != null)
                    {
                        yield return (i, cell);
                    }
                }
            }
        }

        public IEnumerable<(int Cell, T Value)> IntersectsWithCells(ISet<Point> locations)
        {
            foreach (var location in locations)
            {
                if (metrics.Contains(location))
                {
                    var cell = this[location];
                    if (cell != null)
                    {
                        metrics.GetCell(location, out int i);
                        yield return (i, cell);
                    }
                }
            }
        }
        public IEnumerable<(Point Point, T Value)> IntersectsWithPoints(ISet<int> cells)
        {
            foreach (var i in cells)
            {
                if (metrics.GetLocation(i, out Point location))
                {
                    var cell = this[i];
                    if (cell != null)
                    {
                        yield return (location, cell);
                    }
                }
            }
        }

        public IEnumerable<(Point Point, T Value)> IntersectsWithPoints(ISet<Point> locations)
        {
            foreach (var location in locations)
            {
                if (metrics.Contains(location))
                {
                    var cell = this[location];
                    if (cell != null)
                    {
                        yield return (location, cell);
                    }
                }
            }
        }

        public IEnumerator<(int Cell, T Value)> GetEnumerator()
        {
            for (int i = 0; i < metrics.Length; ++i)
            {
                T cell = this[i];
                if (cell != null)
                {
                    yield return (i, cell);
                }
            }
        }

        public IEnumerable<(Point Point, T Value)> FromPoints()
        {
            for (var i = 0; i < metrics.Length; ++i)
            {
                T cellVal = this[i];
                if (cellVal != null && metrics.GetLocation(i, out Point location))
                {
                    yield return (location, cellVal);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
