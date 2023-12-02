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
using MobiusEditor.Render;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace MobiusEditor.Model
{
    [Flags]
    public enum SmudgeTypeFlag
    {
        None = 0,
        /// <summary>4-wide bib indicator. Only used for the bibs automatically added under buildings.</summary>
        Bib1 = 1 << 0,
        /// <summary>3-wide bib indicator. Only used for the bibs automatically added under buildings.</summary>
        Bib2 = 1 << 1,
        /// <summary>2-wide bib indicator. Only used for the bibs automatically added under buildings.</summary>
        Bib3 = 1 << 2,
    }

    [DebuggerDisplay("{Name}")]
    public class SmudgeType : IBrowsableType
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public bool ExistsInTheater { get; private set; }
        public Size Size { get; set; }
        public int Icons { get; set; }
        public SmudgeTypeFlag Flag { get; private set; }
        public bool IsAutoBib => (this.Flag & (SmudgeTypeFlag.Bib1 | SmudgeTypeFlag.Bib2 | SmudgeTypeFlag.Bib3)) != SmudgeTypeFlag.None;
        public bool IsMultiCell => this.Icons == 1 && (this.Size.Width > 0 || this.Size.Height > 0);
        public Bitmap Thumbnail { get; set; }
        private string nameId;


        public SmudgeType(int id, string name, string textId, Size size, int icons, SmudgeTypeFlag flag)
        {
            this.ID = id;
            this.Name = name;
            this.nameId = textId;
            this.Size = size;
            this.Icons = icons;
            this.Flag = flag;
        }

        public SmudgeType(int id, string name, string textId)
            : this(id, name, textId, new Size(1, 1), 1, SmudgeTypeFlag.None)
        {
        }

        public SmudgeType(int id, string name, string textId, int icons)
            : this(id, name, textId, new Size(1, 1), icons, SmudgeTypeFlag.None)
        {
        }

        public SmudgeType(int id, string name, string textId, Size size, SmudgeTypeFlag flag)
            : this(id, name, textId, size, 1, flag)
        {
        }

        public SmudgeType(int id, string name, string textId, Size size)
            : this(id, name, textId, size, SmudgeTypeFlag.None)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is SmudgeType sm)
            {
                return ReferenceEquals(this, sm) || (this.ID == sm.ID && this.Name == sm.Name && this.Flag == sm.Flag && this.Size == sm.Size && this.Icons == sm.Icons);
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

        public void Init(TheaterType theater)
        {
            InitDisplayName();
            // Only evaluate classic tiles, to prevent Interior theater from showing smudge entries.
            this.ExistsInTheater = Globals.TheArchiveManager.ClassicFileExists(this.Name + "." + theater.ClassicExtension);
            var tileSize = Globals.PreviewTileSize;
            Bitmap oldImage = this.Thumbnail;
            Bitmap th = new Bitmap(this.Size.Width * Globals.PreviewTileSize.Width, this.Size.Height * Globals.PreviewTileSize.Height);
            th.SetResolution(96, 96);
            int icon = 0;
            using (Graphics g = Graphics.FromImage(th))
            {
                MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                for (int y = 0; y < this.Size.Height; y++)
                {
                    for (int x = 0; x < this.Size.Width; x++)
                    {
                        Smudge mockSmudge = new Smudge(this) { Icon = icon++ };
                        MapRenderer.RenderSmudge(new Point(x, y), Globals.PreviewTileSize, Globals.PreviewTileScale, mockSmudge).Item2(g);
                    }
                }
            }
            this.Thumbnail = th;
            if (oldImage != null)
            {
                try { oldImage.Dispose(); }
                catch { /* ignore */ }
            }
        }

        public static SmudgeType GetBib(IEnumerable<SmudgeType> SmudgeTypes, int width)
        {
            switch (width)
            {
                case 2:
                    return SmudgeTypes.Where(t => t.Flag == SmudgeTypeFlag.Bib3).FirstOrDefault();
                case 3:
                    return SmudgeTypes.Where(t => t.Flag == SmudgeTypeFlag.Bib2).FirstOrDefault();
                case 4:
                    return SmudgeTypes.Where(t => t.Flag == SmudgeTypeFlag.Bib1).FirstOrDefault();
                default:
                    return null;
            }
        }
        public void Reset()
        {
            this.ExistsInTheater = false;
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
