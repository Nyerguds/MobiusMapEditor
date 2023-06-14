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
        public sbyte ID { get; private set; }

        public string Name { get; private set; }

        public string DisplayName { get; private set; }

        public TheaterType[] Theaters { get; private set; }

        public Size Size { get; set; }
        public int Icons { get; set; }

        public SmudgeTypeFlag Flag { get; private set; }
        public bool IsAutoBib => (Flag & (SmudgeTypeFlag.Bib1 | SmudgeTypeFlag.Bib2 | SmudgeTypeFlag.Bib3)) != SmudgeTypeFlag.None;
        public bool IsMultiCell => Icons == 1 && (Size.Width > 0 || Size.Height > 0);

        public Bitmap Thumbnail { get; set; }

        public SmudgeType(sbyte id, string name, string textId)
            : this(id, name, textId, null, new Size(1, 1), 1, SmudgeTypeFlag.None)
        {
        }

        public SmudgeType(sbyte id, string name, string textId, TheaterType[] theaters)
            : this(id, name, textId, theaters, new Size(1, 1), 1, SmudgeTypeFlag.None)
        {
        }

        public SmudgeType(sbyte id, string name, string textId, int icons)
            : this(id, name, textId, null, new Size(1, 1), icons, SmudgeTypeFlag.None)
        {
        }

        public SmudgeType(sbyte id, string name, string textId, TheaterType[] theaters, int icons)
            : this(id, name, textId, theaters, new Size(1, 1), icons, SmudgeTypeFlag.None)
        {
        }

        public SmudgeType(sbyte id, string name, string textId, TheaterType[] theaters, Size size, SmudgeTypeFlag flag)
            : this(id, name, textId, theaters, size, 1, flag)
        {
        }

        public SmudgeType(sbyte id, string name, string textId, Size size, SmudgeTypeFlag flag)
            : this(id, name, textId, null, size, 1, flag)
        {
        }

        public SmudgeType(sbyte id, string name, string textId, Size size)
            : this(id, name, textId, null, size, SmudgeTypeFlag.None)
        {
        }

        public SmudgeType(sbyte id, string name, string textId, TheaterType[] theaters, Size size)
            : this(id, name, textId, theaters, size, SmudgeTypeFlag.None)
        {
        }

        public SmudgeType(sbyte id, string name, string textId, Size size, int icons, SmudgeTypeFlag flag)
            : this(id, name, textId, null, size, icons, flag)
        {
        }

        public SmudgeType(sbyte id, string name, string textId, TheaterType[] theaters, Size size, int icons, SmudgeTypeFlag flag)
        {
            this.ID = id;
            this.Name = name;
            this.DisplayName = !String.IsNullOrEmpty(textId) && !String.IsNullOrEmpty(Globals.TheGameTextManager[textId])
                ? Globals.TheGameTextManager[textId] + " (" + Name.ToUpperInvariant() + ")"
                : name.ToUpperInvariant();
            this.Size = size;
            this.Icons = icons;
            this.Flag = flag;
            this.Theaters = theaters;
        }

        public override bool Equals(object obj)
        {
            if (obj is SmudgeType sm)
            {
                return ReferenceEquals(this, sm) || (ID == sm.ID && Name == sm.Name && Flag == sm.Flag && Size == sm.Size && Icons == sm.Icons);
            }
            else if (obj is sbyte)
            {
                return ID == (sbyte)obj;
            }
            else if (obj is string)
            {
                return string.Equals(Name, obj as string, StringComparison.OrdinalIgnoreCase);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        public void Init()
        {
            var oldImage = Thumbnail;
            var tileSize = Globals.PreviewTileSize;
            Bitmap th = new Bitmap(tileSize.Width * Size.Width, tileSize.Height * Size.Height);
            th.SetResolution(96, 96);
            bool found = false;
            using (Graphics g = Graphics.FromImage(th))
            {
                MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                int icon = 0;
                for (int y = 0; y < Size.Height; y++)
                {
                    for (int x = 0; x < Size.Width; x++)
                    {
                        if (Globals.TheTilesetManager.GetTileData(Name, icon++, out Tile tile))
                        {
                            found = true;
                            Rectangle overlayBounds = MapRenderer.RenderBounds(tile.Image.Size, new Size(1, 1), Globals.PreviewTileScale);
                            overlayBounds.X += tileSize.Width * x;
                            overlayBounds.Y += tileSize.Height * y;
                            g.DrawImage(tile.Image, overlayBounds);
                        }
                    }
                }
            }
            if (found)
            {
                Thumbnail = th;
            }
            else
            {
                th.Dispose();
                Thumbnail = null;
            }
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
