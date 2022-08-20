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
using System.Drawing;
using System.Drawing.Imaging;

namespace MobiusEditor.Model
{
    [Flags]
    public enum TemplateTypeFlag
    {
        None       = 0,
        Clear      = (1 << 0),
        Water      = (1 << 1),
        RandomCell = (1 << 2),
    }

    public class TemplateType : IBrowsableType
    {
        public ushort ID { get; private set; }

        public string Name { get; private set; }

        public string DisplayName => Name;

        public int IconWidth { get; private set; }

        public int IconHeight { get; private set; }

        public Size IconSize => new Size(IconWidth, IconHeight);

        public int ThumbnailWidth { get; private set; }

        public int ThumbnailHeight { get; private set; }

        public Size ThumbnailSize => new Size(IconWidth, IconHeight);

        public int NumIcons { get; private set; }

        public bool[,] IconMask { get; set; }

        public Image Thumbnail { get; set; }

        public TheaterType[] Theaters { get; private set; }

        public TemplateTypeFlag Flag { get; private set; }

        public Dictionary<string, bool[,]> MaskOverrides { get; private set; }

        /// <summary>
        /// Creates a TemplateType object.
        /// </summary>
        /// <param name="id">Numeric id in the game map data.</param>
        /// <param name="name">Name of the associated graphics.</param>
        /// <param name="iconWidth">Width in cells.</param>
        /// <param name="iconHeight">Height in cells.</param>
        /// <param name="theaters">Theaters that contain this tile.</param>
        /// <param name="flag">Indicates special terrain types.</param>
        /// <param name="maskOverrides">Mask override for tiles that contain too many graphics in the Remaster. Indices with '0' are removed from the tiles. Spaces are ignored and can be added for visual separation.</param>
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, TheaterType[] theaters, TemplateTypeFlag flag, params String[] maskOverrides)
        {
            ID = id;
            Name = name;
            IconWidth = iconWidth;
            IconHeight = iconHeight;
            NumIcons = IconWidth * IconHeight;
            ThumbnailWidth = IconWidth;
            ThumbnailHeight = IconHeight;
            Theaters = theaters;
            Flag = flag;
            MaskOverrides = new Dictionary<string, bool[,]>(StringComparer.InvariantCultureIgnoreCase);
            // Mask override for tiles that contain too many graphics in the Remaster. Indices with '0' are removed from the tiles.
            // Spaces are ignored and can be added for visual separation.
            if (maskOverrides.Length > 0)
            {
                bool forAll = theaters == null || maskOverrides.Length != theaters.Length;
                for (Int32 i = 0; i < maskOverrides.Length; i++)
                {
                    string maskOverride = maskOverrides[i];
                    string theater = forAll ? String.Empty : theaters[i].Name;
                    bool[, ] mask = null;
                    if (!String.IsNullOrEmpty(maskOverride))
                    {
                        mask = new bool[iconWidth, iconHeight];
                        int icon = 0;
                        for (var y = 0; y < IconHeight; ++y)
                        {
                            for (var x = 0; x < IconWidth; ++x, ++icon)
                            {
                                while (icon < maskOverride.Length && maskOverride[icon] == ' ')
                                {
                                    icon++;
                                }
                                mask[x, y] = icon < maskOverride.Length && maskOverride[icon] != '0';
                            }
                        }
                    }
                    if (mask != null)
                    {
                        MaskOverrides[theater] = mask;
                    }
                    if (forAll)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a TemplateType object.
        /// </summary>
        /// <param name="id">Numeric id in the game map data.</param>
        /// <param name="name">Name of the associated graphics.</param>
        /// <param name="iconWidth">Width in cells.</param>
        /// <param name="iconHeight">Height in cells.</param>
        /// <param name="theaters">Theaters that contain this tile.</param>
        /// <param name="maskOverride">Mask override for tiles that contain too many graphics in the Remaster. Indices with '0' are removed from the tiles. Spaces are ignored and can be added for visual separation.</param>
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, TheaterType[] theaters, params String[] maskOverride)
            : this(id, name, iconWidth, iconHeight, theaters, TemplateTypeFlag.None, maskOverride)
        {
        }

        /// <summary>
        /// Creates a TemplateType object.
        /// </summary>
        /// <param name="id">Numeric id in the game map data.</param>
        /// <param name="name">Name of the associated graphics.</param>
        /// <param name="iconWidth">Width in cells.</param>
        /// <param name="iconHeight">Height in cells.</param>
        /// <param name="theaters">Theaters that contain this tile.</param>
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, TheaterType[] theaters)
            : this(id, name, iconWidth, iconHeight, theaters, TemplateTypeFlag.None)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is TemplateType)
            {
                return this == obj;
            }
            else if (obj is byte)
            {
                return ID == (byte)obj;
            }
            else if (obj is ushort)
            {
                return ID == (ushort)obj;
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
            // This allows mods to add 'random' tiles to existing 1x1 tiles. Check excludes 'Clear' terrain and items already defined as random.
            if (IconWidth == 1 & IconHeight == 1 && (Flag & TemplateTypeFlag.Clear) == TemplateTypeFlag.None && (Flag & TemplateTypeFlag.RandomCell) == TemplateTypeFlag.None)
            {
                if (Globals.TheTilesetManager.GetTileDataLength(theater.Tilesets, Name) > 1)
                {
                    Flag |= TemplateTypeFlag.RandomCell;
                }
            }
            var oldImage = Thumbnail;
            var size = new Size(Globals.PreviewTileWidth, Globals.PreviewTileHeight);
            var mask = new bool[IconWidth, IconHeight];
            int loopWidth = IconWidth;
            int loopHeight = IconHeight;
            bool isRandom = (Flag & TemplateTypeFlag.RandomCell) != TemplateTypeFlag.None;
            if (isRandom)
            {
                int numIcons = Globals.TheTilesetManager.GetTileDataLength(theater.Tilesets, Name);
                if (numIcons >= 1)
                {
                    // Try to fit it into a shape as square as possible.
                    NumIcons = numIcons;
                    Double sqrt = Math.Sqrt(numIcons);
                    loopWidth = (sqrt - Math.Floor(sqrt)) < 0.0001  ? (int)sqrt : (int)(sqrt + 1);
                    loopHeight = numIcons / loopWidth + (numIcons % loopWidth == 0 ? 0 : 1);
                    mask[0, 0] = true;
                }
            }
            // To not have to redo the calculation on random times.
            ThumbnailWidth = loopWidth;
            ThumbnailHeight = loopHeight;
            var thumbnail = new Bitmap(loopWidth * size.Width, loopHeight * size.Height);
            bool found = mask[0, 0];
            using (var g = Graphics.FromImage(thumbnail))
            {
                MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                g.Clear(Color.Transparent);
                int icon = 0;
                // If the requested tile is 100% definitely inside the bounds of what is supposed to have graphics, allow it to fetch dummy graphics.
                bool tryDummy = !isRandom && IconWidth == 1 || IconHeight == 1;
                // Try to get mask; first specific per theater, then the general all-theaters mask.
                // The dictionary should only contain either a general one or one per theater, so the fetch order doesn't really matter.
                bool[,] maskOv = null;
                if (!isRandom && MaskOverrides.Keys.Count > 0 && !MaskOverrides.TryGetValue(theater.Name, out maskOv))
                    MaskOverrides.TryGetValue(String.Empty, out maskOv);
                for (var y = 0; y < loopHeight; ++y)
                {
                    for (var x = 0; x < loopWidth; ++x, ++icon)
                    {
                        if (maskOv != null && !maskOv[x, y])
                        {
                            continue;
                        }
                        if (Globals.TheTilesetManager.GetTileData(theater.Tilesets, Name, icon, out Tile tile, tryDummy, true))
                        {
                            if (tile.Image != null)
                            {
                                using (Bitmap tileImg = tile.Image.RemoveAlpha())
                                {
                                    g.DrawImage(tileImg, x * size.Width, y * size.Height, size.Width, size.Height);
                                }
                                if (!isRandom)
                                {
                                    found = mask[x, y] = true;
                                }
                            }
                        }
                    }
                }
            }
            if (!found)
            {
                try { thumbnail.Dispose(); }
                catch { /* ignore */ }
            }
            Thumbnail = found ? thumbnail : null;
            IconMask = mask;
            if (oldImage != null)
            {
                try { oldImage.Dispose(); }
                catch { /* ignore */ }
            }
        }
    }
}
