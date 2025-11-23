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
using System.Diagnostics;
using System.Drawing;

namespace MobiusEditor.Model
{
    [DebuggerDisplay("{Type}: {Icon}")]
    public class Overlay : ICellOccupier, ICellOverlapper
    {
        public OverlayType Type { get; set; }

        public int Icon { get; set; }

        public Size OverlapSize => new Size(1, 1);

        public bool[,] OccupyMask => Type.OccupyMask;
        public bool[,] BaseOccupyMask => Type.OccupyMask;
        public Rectangle OverlapBounds => Type.OverlapBounds;
        public bool[,][] OverlapMask => Type.OverlapMask;
        public Point OverlapMaskOffset => Type.OverlapMaskOffset;
        public bool[,][] ContentMask => Type.ContentMask;
        public Point ContentMaskOffset => Type.ContentMaskOffset;
        public int ZOrder => Type.ZOrder;
        /// <summary>This flag is set after the paint operation to indicate whether this object is overlapped.</summary>
        public bool IsOverlapped { get; set; }

        public bool IsPreview { get; set; }

        public override string ToString()
        {
            return Type?.Name ?? "Unknown";
        }
    }
}
