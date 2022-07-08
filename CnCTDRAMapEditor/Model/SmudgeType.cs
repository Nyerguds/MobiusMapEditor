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
using MobiusEditor.Utility;
using System;
using System.Drawing;

namespace MobiusEditor.Model
{
    [Flags]
    public enum SmudgeTypeFlag
    {
        None = 0,
        // Only used for the bibs automatically added under buildings.
        Bib = 1 << 3,
        Bib1 = 0 | Bib,
        Bib2 = 1 | Bib,
        Bib3 = 2 | Bib,
    }

    public class SmudgeType : IBrowsableType
    {
        public sbyte ID { get; private set; }

        public string Name { get; private set; }

        public string DisplayName => Name;

        public Size Size { get; set; }
        public int Icons{ get; set; }

        public SmudgeTypeFlag Flag { get; private set; }

        public Size RenderSize { get; set; }

        public Image Thumbnail { get; set; }

        public SmudgeType(sbyte id, string name)
            :this(id, name, new Size(1, 1), 1, SmudgeTypeFlag.None)
        {
        }
        public SmudgeType(sbyte id, string name, int icons)
            :this(id, name, new Size(1, 1), icons, SmudgeTypeFlag.None)
        {
        }


        public SmudgeType(sbyte id, string name, Size size, SmudgeTypeFlag flag)
            : this(id, name, size, 1, flag)
        {
        }
        
        public SmudgeType(sbyte id, string name, Size size)
            : this(id, name, size, SmudgeTypeFlag.None)
        {
        }

        public SmudgeType(sbyte id, string name, SmudgeTypeFlag type)
            : this(id, name, new Size(1, 1), type)
        {
        }

        public SmudgeType(sbyte id, string name, Size size, int icons, SmudgeTypeFlag flag)
        {
            ID = id;
            Name = name;
            Size = size;
            Icons = icons;
            Flag = flag;
        }


        public override bool Equals(object obj)
        {
            if (obj is SmudgeType)
            {
                return this == obj;
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

        public void Init(TheaterType theater)
        {
            var oldImage = Thumbnail;
            if (Globals.TheTilesetManager.GetTileData(theater.Tilesets, Name, 0, out Tile tile))
            {
                if ((tile.Image.Width * Globals.TileHeight) > (tile.Image.Height * Globals.TileWidth))
                {
                    RenderSize = new Size(
                        tile.Image.Width * Globals.TileWidth / tile.Image.Width,
                        tile.Image.Height * Globals.TileWidth / tile.Image.Width
                    );
                }
                else
                {
                    RenderSize = new Size(
                        tile.Image.Width * Globals.TileHeight / tile.Image.Height,
                        tile.Image.Height * Globals.TileHeight / tile.Image.Height
                    );
                }
                Thumbnail = new Bitmap(tile.Image, tile.Image.Width, tile.Image.Height);
            }
            else
            {
                Thumbnail = null;
            }
            if (oldImage != null)
            {
                try { oldImage.Dispose(); }
                catch { /* ignore */ }
            }
        }
    }
}
