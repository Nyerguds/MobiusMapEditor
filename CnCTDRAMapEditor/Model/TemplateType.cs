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
using System.IO;
using System.Linq;

namespace MobiusEditor.Model
{
    [Flags]
    public enum TemplateTypeFlag
    {
        /// <summary>No flags set.</summary>
        None           = 0,
        /// <summary>Used to filter out default terrain.</summary>
        Clear          = (1 << 0),
        Water          = (1 << 1),
        /// <summary>This tileset is 1x1, and any additional tiles it contains are treated as randomisable alternate tiles, not as parts of a larger shape.</summary>
        RandomCell     = (1 << 2),
        /// <summary>
        /// This is a virtual tileset group, used to group together loose equivalent 1x1 tiles as randomisable. It should never actually be placed down on the map.
        /// GroupTiles will contain all tiles that are part of this group.
        /// </summary>
        Group          = RandomCell | (1 << 3),
        /// <summary>
        /// This tile is a 1x1 tile that has a bunch of equivalent alternates, and because of that, it is grouped in a virtual tileset group as randomisable.
        /// GroupTiles will contain the group it belongs to, so picking it from the map can easily select the group it belongs to.
        /// </summary>
        IsGrouped      = (1 << 4),
        /// <summary>
        /// This tileset has equivalent tilesets, and when drag-placing this, it will switch to randomly placing the alternates as well.
        /// GroupTiles contains all equivalents that can be used.
        /// </summary>
        HasEquivalents = (1 << 5),
    }

    public enum LandType
    {
        Clear,     // "Clear" terrain.
        Road,      // Road terrain.
        Water,     // Water.
        Rock,      // Impassable rock.
        Wall,      // Wall (blocks movement).
        Tiberium,  // Tiberium field.
        Beach,     // Beach terrain.
        Rough,     // Rocky terrain.
        River,     // Rocky riverbed.
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
        public Dictionary<string, bool[]> MaskOverrides { get; private set; }
        private LandType[] landOverrides;
        public LandType[] LandTypes { get; private set; }
        private bool extraTilesFoundOn1x1;
        public bool IsRandom => (this.extraTilesFoundOn1x1 || (this.Flag & TemplateTypeFlag.RandomCell) != TemplateTypeFlag.None)
                                    && this.IconWidth == 1 && this.IconHeight == 1;

        /// <summary>
        /// On template types with the 'Group' flag, this needs to contains the list of all the tiles that are part of the group.
        /// On template types with the 'IsGrouped' flag, it must be filled in with a single item containing the name of the group template they belong to.
        /// On templates with the 'HasEquivalents' flag, this lists all the equivalent tiles.
        /// </summary>
        public string[] GroupTiles { get; private set; }

        /// <summary>
        /// If 'HasEquivalents' flag is active, the equivalents might not all be the same dimensions.
        /// This gives the offset this tile should be placed on when used as equivalent.
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
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, TheaterType[] theaters, string landOverride, TemplateTypeFlag flag)
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
            MaskOverrides = new Dictionary<string, bool[]>(StringComparer.OrdinalIgnoreCase);
            if (landOverride != null)
            {
                this.landOverrides = landOverride == null ? null : GetLandTypesFromString(landOverride);
            }
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
        /// <param name="maskOverrides">Mask override per theater, for tiles that contain differing numbers of tiles in different theaters, or contain too many graphics in the Remaster. Indices with '0' are removed from the tiles. Spaces are ignored and can be added for visual separation.</param>
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, TheaterType[] theaters, string landOverride, TemplateTypeFlag flag, string[] maskOverrides)
            : this(id, name, iconWidth, iconHeight, theaters, landOverride, flag)
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
                    bool[] mask = null;
                    if (!String.IsNullOrEmpty(maskOverride))
                    {
                        mask = GeneralUtils.GetMaskFromString(iconWidth * iconHeight, maskOverride, '0', ' ');
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
            : this(id, name, 1, 1, theaters, null, TemplateTypeFlag.IsGrouped)
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
            : this(id, name, 1, 1, theaters, null, asGroup ? TemplateTypeFlag.Group : TemplateTypeFlag.RandomCell)
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
        /// <param name="maskOverride">Mask override for tiles that contain too many graphics in the Remaster. Indices with '0' are removed from the tiles. Spaces are ignored and can be added for visual separation.</param>
        /// <param name="equivalentOffset">Thez position this tileset should be placed on when used as equivalent.</param>
        /// <param name="equivalentTiles">Equivalent tiles that can be placed down when drag-placing multiple of this.</param>
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, TheaterType[] theaters, string landOverride, string maskOverride, Point equivalentOffset, string[] equivalentTiles)
            : this (id, name, iconWidth, iconHeight, theaters, landOverride, maskOverride == null ? null : new[] { maskOverride }, equivalentOffset, equivalentTiles)
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
        /// <param name="maskOverrides">Mask override for tiles that contain too many graphics in the Remaster. Indices with '0' are removed from the tiles. Spaces are ignored and can be added for visual separation.</param>
        /// <param name="equivalentOffset">Thez position this tileset should be placed on when used as equivalent.</param>
        /// <param name="equivalentTiles">Equivalent tiles that can be placed down when drag-placing multiple of this.</param>
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, TheaterType[] theaters, string landOverride, string[] maskOverrides, Point equivalentOffset, string[] equivalentTiles)
            : this(id, name, iconWidth, iconHeight, theaters, landOverride, TemplateTypeFlag.None, maskOverrides)
        {
            // prevent list with only the item itself from being accepted
            List<string> equivs = equivalentTiles == null ? null : equivalentTiles.Distinct(StringComparer.OrdinalIgnoreCase).Where(t => t != null && !t.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();
            if (equivs != null && equivs.Count > 0)
            {
                equivs.Add(name);
                this.EquivalentOffset = equivalentOffset;
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
        /// <param name="maskOverride">Mask override for tiles that contain too many graphics in the Remaster. Indices with '0' are removed from the tiles. Spaces are ignored and can be added for visual separation.</param>
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, TheaterType[] theaters, string landOverride, string maskOverride, string[] equivalentTiles)
            : this(id, name, iconWidth, iconHeight, theaters, landOverride, maskOverride == null ? null : new[] { maskOverride }, Point.Empty, equivalentTiles)
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
        /// <param name="maskOverrides">Mask override per theater, for tiles that contain differing numbers of tiles in different theaters, or contain too many graphics in the Remaster. Indices with '0' are removed from the tiles. Spaces are ignored and can be added for visual separation.</param>
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, TheaterType[] theaters, string landOverride, string[] maskOverrides)
            : this(id, name, iconWidth, iconHeight, theaters, landOverride, TemplateTypeFlag.None, maskOverrides)
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
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, TheaterType[] theaters, string landOverride, string maskOverride)
            : this(id, name, iconWidth, iconHeight, theaters, landOverride, TemplateTypeFlag.None, maskOverride == null ? null : new string[] { maskOverride })
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
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, TheaterType[] theaters, string landOverride)
            : this(id, name, iconWidth, iconHeight, theaters, landOverride, TemplateTypeFlag.None)
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
            this.extraTilesFoundOn1x1 = false;
            bool isRandom = (Flag & TemplateTypeFlag.RandomCell) != TemplateTypeFlag.None;
            bool isGroup = (Flag & TemplateTypeFlag.Group) == TemplateTypeFlag.Group;
            // This allows mods to add 'random' tiles to existing 1x1 tiles. Check excludes 'Clear' terrain and items already defined as random.
            if (IconWidth == 1 & IconHeight == 1 && (Flag & TemplateTypeFlag.Clear) == TemplateTypeFlag.None && !isRandom && !isGroup)
            {
                if (Globals.TheTilesetManager.GetTileDataLength(Name) > 1)
                {
                    this.extraTilesFoundOn1x1 = true;
                    isRandom = true;
                }
            }
            Bitmap oldImage = Thumbnail;
            Size size = new Size(Globals.PreviewTileWidth, Globals.PreviewTileHeight);
            Boolean[,] mask = new bool[IconHeight, IconWidth];
            int loopWidth = IconWidth;
            int loopHeight = IconHeight;
            Int32 numIcons = IconWidth * IconHeight;
            if (isRandom)
            {
                numIcons = Math.Max(1, isGroup ? GroupTiles.Length : Globals.TheTilesetManager.GetTileDataLength(Name));
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
            Bitmap th = new Bitmap(loopWidth * size.Width, loopHeight * size.Height);
            th.SetResolution(96, 96);
            bool found = mask[0, 0];
            using (Graphics g = Graphics.FromImage(th))
            {
                MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                g.Clear(Color.Transparent);
                int icon = 0;
                // If the requested tile is 100% definitely inside the bounds of what is supposed to have graphics, allow it to fetch dummy graphics.
                // Always allow dummy graphics for the first cell of a 1x1-random template.
                bool tryDummy = forceDummy || (!isRandom && (IconWidth == 1 || IconHeight == 1)) || NumIcons == 1;
                // Try to get mask; first specific per theater, then the general all-theaters mask.
                // The dictionary should only contain either a general one or one per theater, so the fetch order doesn't really matter.
                bool[] maskOv = null;
                if (!isRandom && MaskOverrides.Keys.Count > 0 && !MaskOverrides.TryGetValue(theater.Name, out maskOv))
                    MaskOverrides.TryGetValue(String.Empty, out maskOv);
                for (Int32 y = 0; y < loopHeight; ++y)
                {
                    for (Int32 x = 0; x < loopWidth; ++x, ++icon)
                    {
                        if (icon >= NumIcons)
                        {
                            break;
                        }
                        if (maskOv != null)
                        {
                            if (!maskOv[icon])
                                continue;
                            else // If forced, always fetch the graphics.
                                tryDummy = true;
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
                        bool success = Globals.TheTilesetManager.GetTileData(nameToFetch, iconToFetch, out Tile tile, tryDummy, false);
                        if (tile != null && tile.Image != null)
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
            if (!found)
            {
                try { th.Dispose(); }
                catch { /* ignore */ }
            }
            Thumbnail = found ? th : null;
            IconMask = mask;
            if (oldImage != null)
            {
                try { oldImage.Dispose(); }
                catch { /* ignore */ }
            }
            LandType[] landTypes = landOverrides != null ? landOverrides.ToArray() : null;
            int typeIcons = isRandom ? 1 : isGroup ? 0 : numIcons;
            if (!isGroup && this.landOverrides == null)
            {
                using (Stream terrainTypeInfo = Globals.TheArchiveManager.OpenFileClassic(this.Name + "." + theater.ClassicExtension))
                {
                    if (terrainTypeInfo != null)
                    {
                        byte[] fileData;
                        using (BinaryReader br = new BinaryReader(terrainTypeInfo))
                        {
                            fileData = br.ReadAllBytes();
                        }
                        landTypes = Enumerable.Repeat(LandType.Clear, typeIcons).ToArray();
                        try
                        {
                            // TODO: Dimensions are currently not loaded from the classic files yet.
                            ClassicSpriteLoader.GetRaTmpData(fileData, out _, out _, out byte[] landTypeInfo, out _, out _, out _);
                            landTypes = new LandType[typeIcons];
                            int max = Math.Min(typeIcons, landTypeInfo.Length);
                            for (int icon = 0; icon < max; ++icon)
                            {
                                byte val = landTypeInfo[icon];
                                LandType land = val > tileTypeFromFile.Length ? LandType.Clear : tileTypeFromFile[landTypeInfo[icon]];
                                landTypes[icon] = land;
                            }
                        }
                        catch (ArgumentException ex) { /* Not able to parse; fall back to all-clear. */ }
                    }
                }
                if (isRandom && landTypes.Length >= 1)
                {
                    LandType rntp = landTypes[0];
                    landTypes = Enumerable.Repeat(rntp, numIcons).ToArray();
                }
            }
            if (landTypes == null && !isGroup)
            {
                landTypes = Enumerable.Repeat(LandType.Clear, typeIcons).ToArray();
            }
            LandTypes = landTypes;
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
            if (point.X < 0 || point.Y < 0)
            {
                return false;
            }
            if (point.X >= ThumbnailIconWidth || point.X >= ThumbnailIconWidth)
            {
                return false;
            }
            return this.IsRandom || IconMask[point.Y, point.X];
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

        private static readonly LandType[] tileTypeFromFile = new[]
        {
            LandType.Clear,         // Unused / 1x1-multiple
            LandType.Clear,         // ???
            LandType.Clear,         // ???
            LandType.Clear,         // Clear
            LandType.Clear,         // ???
            LandType.Clear,         // ???
            LandType.Beach,         // Beach
            LandType.Clear,         // ???
            LandType.Rock,          // Rock
            LandType.Road,          // Road
            LandType.Water,         // Water
            LandType.River,         // River
            LandType.Clear,         // ???
            LandType.Clear,         // ???
            LandType.Rough,         // Rough
            LandType.Clear,         // ???
        };

        private static readonly Dictionary<char, LandType> LandTypesMapping = new Dictionary<char, LandType>
        {
            { 'X', LandType.Clear }, // Filler tile, or [Clear] terrain on 1x1 sets with multiple tiles.
            { 'C', LandType.Clear }, // [Clear] Normal clear terrain.
            { 'B', LandType.Beach }, // [Beach] Sandy beach. Can''t be built on.
            { 'I', LandType.Rock }, // [Rock]  Impassable terrain.
            { 'R', LandType.Road }, // [Road]  Units move faster on this terrain.
            { 'W', LandType.Water }, // [Water] Ships can travel over this.
            { 'V', LandType.River }, // [River] Ships normally can''t travel over this.
            { 'H', LandType.Rough }, // [Rough] Rough terrain. Can''t be built on
        };

        private static LandType[] GetLandTypesFromString(string types)
        {
            types = types.Replace(" ", String.Empty);
            LandType[] arr = new LandType[types.Length];
            Char[] array = types.ToUpperInvariant().ToCharArray();
            for (Int32 i = 0; i < array.Length; ++i)
            {
                arr[i] = LandTypesMapping.TryGetValue(array[i], out LandType t) ? t : LandType.Clear;
            }
            return arr;
        }
    }
}
