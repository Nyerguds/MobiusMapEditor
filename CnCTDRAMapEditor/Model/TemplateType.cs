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
using System.Linq;

namespace MobiusEditor.Model
{
    [Flags]
    public enum TemplateTypeFlag
    {
        None           = 0,
        Clear          = (1 << 0),
        Water          = (1 << 1),
        RandomCell     = (1 << 2),
        Group          = RandomCell | (1 << 3),
        IsGrouped      = (1 << 4),
        HasEquivalents = (1 << 5),
    }

    public class TemplateType : IBrowsableType
    {
        public ushort ID { get; private set; }

        public string Name { get; private set; }

        public string DisplayName => Name;

        public int IconWidth { get; private set; }

        public int IconHeight { get; private set; }

        public Size IconSize => new Size(IconWidth, IconHeight);

        public int ThumbnailIconWidth { get; private set; }

        public int ThumbnailIconHeight { get; private set; }

        public Size ThumbnailSize => new Size(IconWidth, IconHeight);

        public int NumIcons { get; private set; }

        public bool[,] IconMask { get; set; }

        public Bitmap Thumbnail { get; set; }

        public TheaterType[] Theaters { get; private set; }

        public TemplateTypeFlag Flag { get; private set; }

        public Dictionary<string, bool[,]> MaskOverrides { get; private set; }

        /// <summary>
        /// On template types with the 'Group' flag, this needs to contains the list of all the tiles that are part of the group.
        /// On template types with the 'IsGrouped' flag, it must be filled in with a single item containing the name of the group template they belong to.
        /// On templates with the 'HasEquivalents' flag, this lists all the equivalent tiles.
        /// </summary>
        public string[] GroupTiles { get; private set; }

        /// <summary>
        /// If 'HasEquivalents' flag is active, the equivalents might not all be the same dimensions.
        /// This gives the full bounds this tile should act like when used as equivalent.
        /// </summary>
        public Size EquivalentBounds { get; private set; } = Size.Empty;

        /// <summary>
        /// If 'HasEquivalents' flag is active, the equivalents might not all be the same dimensions.
        /// This gives the offset inside the EquivalentBounds this tile should be placed on when used as equivalent.
        /// </summary>
        public Point EquivalentOffset { get; private set; } = Point.Empty;

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
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, TheaterType[] theaters, TemplateTypeFlag flag)
        {
            ID = id;
            Name = name;
            IconWidth = iconWidth;
            IconHeight = iconHeight;
            NumIcons = IconWidth * IconHeight;
            ThumbnailIconWidth = IconWidth;
            ThumbnailIconHeight = IconHeight;
            Theaters = theaters;
            Flag = flag;
            MaskOverrides = new Dictionary<string, bool[,]>(StringComparer.OrdinalIgnoreCase);
            GroupTiles = new string[0];
        }

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
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, TheaterType[] theaters, TemplateTypeFlag flag, string[] maskOverrides)
            : this(id, name, iconWidth, iconHeight, theaters, flag)
        {
            bool isRandom = NumIcons == 1 && (flag & TemplateTypeFlag.RandomCell) != TemplateTypeFlag.None;
            // Mask override for tiles that contain too many graphics in the Remaster. Indices with '0' are removed from the tiles.
            // Spaces are ignored and can be added for visual separation.
            if (maskOverrides != null && maskOverrides.Length > 0 && !isRandom)
            {
                bool forAll = theaters == null || maskOverrides.Length != theaters.Length;
                for (int i = 0; i < maskOverrides.Length; i++)
                {
                    string maskOverride = maskOverrides[i];
                    string theater = forAll ? String.Empty : theaters[i].Name;
                    bool[,] mask = null;
                    if (!String.IsNullOrEmpty(maskOverride))
                    {
                        mask = new bool[iconHeight, iconWidth];
                        int charIndex = 0;
                        for (int y = 0; y < IconHeight; ++y)
                        {
                            for (int x = 0; x < IconWidth; ++x, ++charIndex)
                            {
                                // The format allows whitespace for clarity. Skip without consequence.
                                while (charIndex < maskOverride.Length && maskOverride[charIndex] == ' ')
                                {
                                    charIndex++;
                                }
                                mask[y, x] = charIndex < maskOverride.Length && maskOverride[charIndex] != '0';
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
        /// Creates a TemplateType object as 1x1 that is part of a group-entry.
        /// </summary>
        /// <param name="id">Numeric id in the game map data.</param>
        /// <param name="name">Name of the associated graphics.</param>
        /// <param name="theaters">Theaters that contain this tile.</param>
        /// <param name="groupName">Name of the group entry it belongs to.</param>
        public TemplateType(ushort id, string name, TheaterType[] theaters, string groupName)
            : this(id, name, 1, 1, theaters, TemplateTypeFlag.IsGrouped)
        {
            GroupTiles = new string[] { groupName };
        }

        /// <summary>
        /// Creates a TemplateType object as 1x1 containing multiple tiles. If specified, can be used as "group" of other 1x1 tiles.
        /// </summary>
        /// <param name="id">Numeric id in the game map data.</param>
        /// <param name="name">Name of the associated graphics.</param>
        /// <param name="theaters">Theaters that contain this tile.</param>
        /// <param name="asGroup">True to create this tile as group for containing other 1x1 types. Needs 'containedTiles' to be filled in.</param>
        /// <param name="containedTiles">The tiles contained in this group entry.</param>
        public TemplateType(ushort id, string name, TheaterType[] theaters, bool asGroup, string[] containedTiles)
            : this(id, name, 1, 1, theaters, asGroup ? TemplateTypeFlag.Group : TemplateTypeFlag.RandomCell)
        {
            if (asGroup)
            {
                GroupTiles = containedTiles.ToArray();
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
        /// <param name="flag">Indicates special terrain types.</param>
        /// <param name="maskOverrides">Mask override for tiles that contain too many graphics in the Remaster. Indices with '0' are removed from the tiles. Spaces are ignored and can be added for visual separation.</param>
        /// <param name="equivalentBounds"></param>
        /// <param name="equivalentOffset"></param>
        /// <param name="equivalentTiles"></param>
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, TheaterType[] theaters, string maskOverrides, Size equivalentBounds, Point equivalentOffset, string[] equivalentTiles)
            : this(id, name, iconWidth, iconHeight, theaters, maskOverrides == null ? null : new[] { maskOverrides }, equivalentBounds, equivalentOffset, equivalentTiles)
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
        /// <param name="flag">Indicates special terrain types.</param>
        /// <param name="maskOverrides">Mask override for tiles that contain too many graphics in the Remaster. Indices with '0' are removed from the tiles. Spaces are ignored and can be added for visual separation.</param>
        /// <param name="equivalentBounds"></param>
        /// <param name="equivalentOffset"></param>
        /// <param name="equivalentTiles"></param>
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, TheaterType[] theaters, string[] maskOverrides, Size equivalentBounds, Point equivalentOffset, string[] equivalentTiles)
            : this(id, name, iconWidth, iconHeight, theaters, TemplateTypeFlag.None, maskOverrides)
        {
            this.EquivalentBounds = equivalentBounds == Size.Empty ? new Size(iconWidth, iconHeight) : equivalentBounds;
            this.EquivalentOffset = equivalentOffset;
            List<String> equivs = equivalentTiles == null ? null : equivalentTiles.Distinct(StringComparer.OrdinalIgnoreCase).Where(t => t != null && !t.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();
            if (equivs != null && equivs.Count > 0)
            {
                equivs.Add(name);
                this.Flag = TemplateTypeFlag.HasEquivalents;
            }
            this.GroupTiles = equivs == null ? new string[0] : equivs.ToArray();
        }

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
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, TheaterType[] theaters, String maskOverrides, String[] equivalentTiles)
            : this(id, name, iconWidth, iconHeight, theaters, maskOverrides == null ? null : new[] { maskOverrides }, Size.Empty, Point.Empty, equivalentTiles)
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
        /// <param name="maskOverride">Mask override for tiles that contain too many graphics in the Remaster. Indices with '0' are removed from the tiles. Spaces are ignored and can be added for visual separation.</param>
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, TheaterType[] theaters, String[] maskOverride)
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
        /// <param name="maskOverride">Mask override for tiles that contain too many graphics in the Remaster. Indices with '0' are removed from the tiles. Spaces are ignored and can be added for visual separation.</param>
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, TheaterType[] theaters, String maskOverride)
            : this(id, name, iconWidth, iconHeight, theaters, TemplateTypeFlag.None, maskOverride == null ? null : new string[] { maskOverride })
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
            if (obj is TemplateType tmp)
            {
                return ReferenceEquals(this, obj) || (tmp.ID == this.ID && String.Equals(tmp.Name, this.Name, StringComparison.InvariantCultureIgnoreCase));
            }
            else if (obj is byte bid)
            {
                return ID == bid;
            }
            else if (obj is ushort sid)
            {
                return ID == sid;
            }
            else if (obj is int iid)
            {
                return ID == iid;
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
            Init(theater, false);
        }
        
        public void Init(TheaterType theater, bool forceDummy)
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
            var mask = new bool[IconHeight, IconWidth];
            int loopWidth = ThumbnailIconWidth;
            int loopHeight = IconHeight;
            bool isRandom = (Flag & TemplateTypeFlag.RandomCell) != TemplateTypeFlag.None;
            bool isGroup = (Flag & TemplateTypeFlag.Group) == TemplateTypeFlag.Group;
            if (isRandom)
            {
                Int32 numIcons;
                if (isGroup)
                {
                    numIcons = GroupTiles.Length;
                }
                else
                {
                    numIcons = Globals.TheTilesetManager.GetTileDataLength(theater.Tilesets, Name);
                }
                numIcons = Math.Max(1, numIcons);
                NumIcons = numIcons;
                mask[0, 0] = true;
                if (numIcons > 1)
                {
                    // Try to fit it into a shape as square as possible.
                    Double sqrt = Math.Sqrt(numIcons);
                    loopWidth = (sqrt - Math.Floor(sqrt)) < 0.0001 ? (int)sqrt : (int)(sqrt + 1);
                    loopHeight = numIcons / loopWidth + (numIcons % loopWidth == 0 ? 0 : 1);
                }
            }
            // To avoid having to redo the calculations on random tiles.
            ThumbnailIconWidth = loopWidth;
            ThumbnailIconHeight = loopHeight;
            var thumbnail = new Bitmap(loopWidth * size.Width, loopHeight * size.Height);
            bool found = mask[0, 0];
            using (var g = Graphics.FromImage(thumbnail))
            {
                MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                g.Clear(Color.Transparent);
                int icon = 0;
                // If the requested tile is 100% definitely inside the bounds of what is supposed to have graphics, allow it to fetch dummy graphics.
                // Always allow dummy graphics for the first cell of a 1x1-random template.
                bool tryDummy = forceDummy || (!isRandom && (IconWidth == 1 || IconHeight == 1)) || NumIcons == 1;
                // Try to get mask; first specific per theater, then the general all-theaters mask.
                // The dictionary should only contain either a general one or one per theater, so the fetch order doesn't really matter.
                bool[,] maskOv = null;
                if (!isRandom && MaskOverrides.Keys.Count > 0 && !MaskOverrides.TryGetValue(theater.Name, out maskOv))
                    MaskOverrides.TryGetValue(String.Empty, out maskOv);
                for (var y = 0; y < loopHeight; ++y)
                {
                    for (var x = 0; x < loopWidth; ++x, ++icon)
                    {
                        if (icon >= NumIcons)
                        {
                            break;
                        }
                        if (maskOv != null && !maskOv[y, x])
                        {
                            continue;
                        }
                        int iconToFetch = icon;
                        string nameToFetch = Name;
                        if (isGroup)
                        {
                            nameToFetch = GroupTiles[icon];
                            iconToFetch = 0;
                            tryDummy = forceDummy;
                        }
                        // Fetch dummy if definitely in bounds, first cell of a random one, or dummy is forced.
                        if (Globals.TheTilesetManager.GetTileData(theater.Tilesets, nameToFetch, iconToFetch, out Tile tile, tryDummy, forceDummy || !isRandom || (x == 0 && y == 0)))
                        {
                            if (tile.Image != null)
                            {
                                using (Bitmap tileImg = tile.Image.RemoveAlpha())
                                {
                                    g.DrawImage(tileImg, x * size.Width, y * size.Height, size.Width, size.Height);
                                }
                                if (!isRandom)
                                {
                                    found = mask[y, x] = true;
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

        public int GetIconIndex(Point point)
        {
            if (!IsValidIcon(point))
            {
                return -1;
            }
            return point.Y * ThumbnailIconWidth + point.X;
        }

        public Point? GetIconPoint(int icon)
        {
            if (icon < 0 || icon > NumIcons)
            {
                return null;
            }
            return new Point(icon % ThumbnailIconWidth, icon / ThumbnailIconWidth);
        }

        public bool IsValidIcon(Point point)
        {
            bool isRandom = (Flag & TemplateTypeFlag.RandomCell) != TemplateTypeFlag.None;
            if (point.X < 0 || point.Y < 0)
            {
                return false;
            }
            if (point.X >= ThumbnailIconWidth || point.X >= ThumbnailIconWidth)
            {
                return false;
            }
            return isRandom || IconMask[point.Y, point.X];
        }

        public Point GetFirstValidIcon()
        {
            bool isRandom = (Flag & TemplateTypeFlag.RandomCell) != TemplateTypeFlag.None;
            for (int y = 0; y < ThumbnailIconHeight; ++y)
            {
                for (int x = 0; x < ThumbnailIconWidth; ++x)
                {
                    if (isRandom || IconMask[y, x])
                    {
                        return new Point(x, y);
                    }
                }
            }
            // Should never happen.
            return new Point(0, 0);
        }
    }
}
