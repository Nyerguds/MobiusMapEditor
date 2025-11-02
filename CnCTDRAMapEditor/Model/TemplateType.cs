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
using MobiusEditor.Render;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace MobiusEditor.Model
{
    [Flags]
    public enum TemplateTypeFlag
    {
        /// <summary>No flags set.</summary>
        None           /**/ = 0,
        /// <summary>Used to filter out default terrain.</summary>
        Clear          /**/ = 1 << 0,
        /// <summary>Default fill tile to use. Can be defined multiple times to ensure each theater contains a valid one; when requested, the lowest-id one will be used.</summary>
        DefaultFill    /**/ = 1 << 1,
        /// <summary>This tileset is 1x1, and any additional tiles it contains are treated as randomisable alternate tiles, not as parts of a larger shape.</summary>
        RandomCell     /**/ = 1 << 2,
        /// <summary>
        /// This is a virtual tileset group, used to group together loose equivalent 1x1 tiles as randomisable. It should never actually be placed down on the map.
        /// TemplateType.GroupTiles will contain all tiles that are part of this group.
        /// </summary>
        Group          /**/ = RandomCell | (1 << 3),
        /// <summary>
        /// This tile is a 1x1 tile that has a bunch of equivalent alternates, and because of that, it is grouped in a virtual tileset group as randomisable.
        /// TemplateType.GroupTiles will contain the group it belongs to, so picking it from the map can easily select the containing group instead.
        /// </summary>
        IsGrouped      /**/ = 1 << 4,
        /// <summary>
        /// This tileset has equivalent tilesets, and when drag-placing this, it will switch to randomly placing the alternates as well.
        /// TemplateType.GroupTiles contains all equivalents that can be used.
        /// </summary>
        HasEquivalents /**/ = 1 << 5,
    }

    public enum LandType
    {
        None = 0,   // Uninitialised.
        Clear,      // "Clear" terrain.
        Road,       // Road terrain.
        Water,      // Water.
        Rock,       // Impassable rock.
        //Wall,     // Wall (blocks movement).
        //Tiberium, // Tiberium field.
        Beach,      // Beach terrain.
        Rough,      // Rocky terrain.
        River,      // Rocky riverbed.
    }

    [DebuggerDisplay("{Name}")]
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
        public bool Initialised { get; set; }
        public bool ExistsInTheater { get; set; }
        public TemplateTypeFlag Flags { get; private set; }
        public Dictionary<string, bool[,]> MaskOverrides { get; private set; }
        private LandType[] landsDefault;
        public LandType[] LandTypes { get; private set; }
        private bool extraTilesFoundOn1x1;
        public bool IsRandom => (this.extraTilesFoundOn1x1 || (this.Flags & TemplateTypeFlag.RandomCell) != TemplateTypeFlag.None)
                                    && this.IconWidth == 1 && this.IconHeight == 1;
        public bool IsGroup => (this.Flags & TemplateTypeFlag.Group) == TemplateTypeFlag.Group;
        public bool IsGrouped => (this.Flags & TemplateTypeFlag.IsGrouped) == TemplateTypeFlag.IsGrouped;

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
        /// Creates a TemplateType object. Not public, since the real constructor should always be the one with the default land type mapping.
        /// </summary>
        /// <param name="id">Numeric id in the game map data.</param>
        /// <param name="name">Name of the associated graphics.</param>
        /// <param name="iconWidth">Width in cells.</param>
        /// <param name="iconHeight">Height in cells.</param>
        /// <param name="landsDefault">Defaults for the terrain types for each cell. See <see cref="LandTypesMapping"/> for the characters to use.</param>
        /// <param name="flags">Indicates special terrain types.</param>
        /// <param name="maskOverrides">Overrides the shape for different theaters. An empty string is used as default.</param>
        private TemplateType(ushort id, string name, int iconWidth, int iconHeight, string landsDefault, TemplateTypeFlag flags, Dictionary<string, bool[,]> maskOverrides)
        {
            ID = id;
            Name = name;
            IconWidth = iconWidth;
            IconHeight = iconHeight;
            NumIcons = IconWidth * IconHeight;
            ThumbnailIconWidth = IconWidth;
            ThumbnailIconHeight = IconHeight;
            Flags = flags;
            MaskOverrides = new Dictionary<string, bool[,]>(StringComparer.OrdinalIgnoreCase);
            if (maskOverrides != null)
            {
                foreach (KeyValuePair<string, bool[,]> kvp in maskOverrides)
                {
                    MaskOverrides[kvp.Key] = kvp.Value;
                }
            }

            if (landsDefault != null)
            {
                this.landsDefault = landsDefault == null ? null : GetLandTypesFromString(landsDefault);
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
        /// <param name="landsDefault">Defaults for the terrain types for each cell. See <see cref="LandTypesMapping"/> for the characters to use.</param>
        /// <param name="flags">Indicates special terrain types.</param>
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, string landsDefault, TemplateTypeFlag flags)
            : this(id, name, iconWidth, iconHeight, landsDefault, flags, null)
        {
        }

        /// <summary>
        /// Creates a TemplateType object.
        /// </summary>
        /// <param name="id">Numeric id in the game map data.</param>
        /// <param name="name">Name of the associated graphics.</param>
        /// <param name="iconWidth">Width in cells.</param>
        /// <param name="iconHeight">Height in cells.</param>
        /// <param name="landsDefault">Defaults for the terrain types for each cell. See <see cref="LandTypesMapping"/> for the characters to use.</param>
        /// <param name="flags">Indicates special terrain types.</param>
        /// <param name="maskDefault">Default tile usage mask to use for any theaters not overridden by <paramref name="maskOverrides"/>. Indices with '0' are removed from the tiles. Spaces are ignored and can be added for visual separation.</param>
        /// <param name="maskOverrides">Mask override per theater, for tiles that contain differing numbers of tiles in different theaters.</param>
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, string landsDefault, TemplateTypeFlag flags, string maskDefault, Dictionary<string, string> maskOverrides)
            : this(id, name, iconWidth, iconHeight, landsDefault, flags, null)
        {
            bool isRandom = NumIcons == 1 && (flags & TemplateTypeFlag.RandomCell) != TemplateTypeFlag.None;
            // Mask override for tiles that contain too many graphics in the Remaster. Indices with '0' are removed from the tiles.
            // Spaces are ignored and can be added for visual separation.
            if (maskDefault != null)
            {
                MaskOverrides[String.Empty] = GeneralUtils.GetMaskFromString(iconWidth, iconHeight, maskDefault, '0', ' ');
            }
            if (maskOverrides != null && maskOverrides.Keys.Count > 0 && !isRandom)
            {
                foreach (KeyValuePair<string, string> kvp in maskOverrides)
                {
                    bool[,] mask = null;
                    if (!String.IsNullOrEmpty(kvp.Value))
                    {
                        mask = GeneralUtils.GetMaskFromString(iconWidth, iconHeight, kvp.Value, '0', ' ');
                    }
                    if (mask != null)
                    {
                        MaskOverrides[kvp.Key] = mask;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a TemplateType object as 1x1 that is part of a group-entry.
        /// </summary>
        /// <param name="id">Numeric id in the game map data.</param>
        /// <param name="name">Name of the associated graphics.</param>
        /// <param name="landsDefault">Defaults for the terrain types for each cell. See <see cref="LandTypesMapping"/> for the characters to use.</param>
        /// <param name="groupName">Name of the group entry it belongs to.</param>
        public TemplateType(ushort id, string name, string landsDefault, string groupName)
            : this(id, name, 1, 1, landsDefault, TemplateTypeFlag.IsGrouped, null)
        {
            GroupTiles = new string[] { groupName };
        }

        /// <summary>
        /// Creates a TemplateType object as 1x1 containing multiple tiles.
        /// </summary>
        /// <param name="id">Numeric id in the game map data.</param>
        /// <param name="name">Name of the associated graphics.</param>
        /// <param name="landType">Land type to apply to all cells of this template type.</param>
        public TemplateType(ushort id, string name, char landType)
            : this(id, name, 1, 1, landType.ToString(), TemplateTypeFlag.RandomCell, null)
        {
        }

        /// <summary>
        /// Creates a TemplateType object as 1x1 containing multiple tiles. If specified, can be used as "group" of other 1x1 tiles.
        /// </summary>
        /// <param name="id">Numeric id in the game map data.</param>
        /// <param name="name">Name of the associated graphics.</param>
        /// <param name="landsDefault">Defaults for the terrain types for each cell. See <see cref="LandTypesMapping"/> for the characters to use.</param>
        /// <param name="asGroup">True to create this tile as group for containing other 1x1 types. Needs 'containedTiles' to be filled in.</param>
        /// <param name="containedTiles">The tiles contained in this group entry.</param>
        public TemplateType(ushort id, string name, string landsDefault, bool asGroup, string[] containedTiles)
            : this(id, name, 1, 1, landsDefault, asGroup ? TemplateTypeFlag.Group : TemplateTypeFlag.RandomCell, null)
        {
            if (asGroup)
            {
                GroupTiles = containedTiles.ToArray();
            }
        }

        /// <summary>
        /// Creates a TemplateType object that has a list of equivalent tiles.
        /// </summary>
        /// <param name="id">Numeric id in the game map data.</param>
        /// <param name="name">Name of the associated graphics.</param>
        /// <param name="iconWidth">Width in cells.</param>
        /// <param name="iconHeight">Height in cells.</param>
        /// <param name="landsDefault">Defaults for the terrain types for each cell. See <see cref="LandTypesMapping"/> for the characters to use.</param>
        /// <param name="maskDefault">Tile usage mask. Indices with '0' are removed from the tiles. Spaces are ignored and can be added for visual separation.</param>
        /// <param name="equivalentTiles">Equivalent tiles that can be placed down when drag-placing multiple of this.</param>
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, string landsDefault, string maskDefault, string[] equivalentTiles)
            : this(id, name, iconWidth, iconHeight, landsDefault, maskDefault, Point.Empty, equivalentTiles)
        {
        }

        /// <summary>
        /// Creates a TemplateType object that has a list of equivalent tiles.
        /// </summary>
        /// <param name="id">Numeric id in the game map data.</param>
        /// <param name="name">Name of the associated graphics.</param>
        /// <param name="iconWidth">Width in cells.</param>
        /// <param name="iconHeight">Height in cells.</param>
        /// <param name="landsDefault">Defaults for the terrain types for each cell. See <see cref="LandTypesMapping"/> for the characters to use.</param>
        /// <param name="maskDefault">Tile usage mask. Indices with '0' are removed from the tiles. Spaces are ignored and can be added for visual separation.</param>
        /// <param name="equivalentOffset">Thez position this tileset should be placed on when used as equivalent.</param>
        /// <param name="equivalentTiles">Equivalent tiles that can be placed down when drag-placing multiple of this.</param>
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, string landsDefault, string maskDefault, Point equivalentOffset, string[] equivalentTiles)
            : this(id, name, iconWidth, iconHeight, landsDefault, TemplateTypeFlag.None, maskDefault, null)
        {
            // prevent list with only the item itself from being accepted
            List<string> equivs = equivalentTiles == null ? null : equivalentTiles.Distinct(StringComparer.OrdinalIgnoreCase).Where(t => t != null && !t.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();
            if (equivs != null && equivs.Count > 0)
            {
                equivs.Add(name);
                this.EquivalentOffset = equivalentOffset;
                this.Flags = TemplateTypeFlag.HasEquivalents;
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
        /// <param name="landsDefault">Defaults for the terrain types for each cell. See <see cref="LandTypesMapping"/> for the characters to use.</param>
        /// <param name="maskDefault">Default tile usage mask to use for any theaters not overridden by <paramref name="maskOverrides"/>. Indices with '0' are removed from the tiles. Spaces are ignored and can be added for visual separation.</param>
        /// <param name="maskOverrides">Mask override per theater, for tiles that contain differing numbers of tiles in different theaters.</param>
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, string landsDefault, string maskDefault, Dictionary<string, string> maskOverrides)
            : this(id, name, iconWidth, iconHeight, landsDefault, TemplateTypeFlag.None, maskDefault, maskOverrides)
        {
        }

        /// <summary>
        /// Creates a TemplateType object.
        /// </summary>
        /// <param name="id">Numeric id in the game map data.</param>
        /// <param name="name">Name of the associated graphics.</param>
        /// <param name="iconWidth">Width in cells.</param>
        /// <param name="iconHeight">Height in cells.</param>
        /// <param name="landsDefault">Defaults for the terrain types for each cell. See <see cref="LandTypesMapping"/> for the characters to use.</param>
        /// <param name="maskDefault">Default tile usage mask. Indices with '0' are removed from the tiles. Spaces are ignored and can be added for visual separation.</param>
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, string landsDefault, string maskDefault)
            : this(id, name, iconWidth, iconHeight, landsDefault, TemplateTypeFlag.None, maskDefault, null)
        {
        }

        /// <summary>
        /// Creates a TemplateType object.
        /// </summary>
        /// <param name="id">Numeric id in the game map data.</param>
        /// <param name="name">Name of the associated graphics.</param>
        /// <param name="iconWidth">Width in cells.</param>
        /// <param name="iconHeight">Height in cells.</param>
        /// <param name="landsDefault">Defaults for the terrain types for each cell. See <see cref="LandTypesMapping"/> for the characters to use.</param>
        public TemplateType(ushort id, string name, int iconWidth, int iconHeight, string landsDefault)
            : this(id, name, iconWidth, iconHeight, landsDefault, TemplateTypeFlag.None)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is TemplateType tmp)
            {
                return ReferenceEquals(this, obj) || (tmp.ID == this.ID && String.Equals(tmp.Name, this.Name, StringComparison.InvariantCultureIgnoreCase));
            }
            else if (obj is sbyte sbid)
            {
                return this.ID == sbid;
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

        public void AddIni(StringBuilder sb)
        {
            if (sb.Length > 0)
            {
                sb.AppendLine();
            }
            sb.Append("[").Append(Name).AppendLine("]");
            sb.Append("X=").Append(this.IconWidth).AppendLine();
            sb.Append("Y=").Append(this.IconHeight).AppendLine();
            string landTypesStr = GetLandTypesString('_', ' ');
            sb.Append("Terrain=").AppendLine(landTypesStr);
            if (MaskOverrides.Count > 0)
            {
                List<string> overrides = new List<string>();
                foreach (KeyValuePair<string, bool[,]> ovr in MaskOverrides)
                {
                    string mask = GetMaskString(ovr.Value, '1', '0', ' ');
                    if (String.IsNullOrEmpty(ovr.Key))
                    {
                        sb.Append("Mask=").AppendLine(mask);
                    }
                    else
                    {
                        string ovrr = ovr.Key + ':' + mask;
                        overrides.Add(ovrr);
                    }
                }
                if (overrides.Count > 0)
                {
                    sb.Append("MaskOverrides=").AppendLine(String.Join(",", overrides.ToArray()));
                }
            }
            //sb.Append("PrimaryType=").AppendLine(LandType.Clear.ToString());
        }

        public LandType GetLandType(int icon)
        {
            if ((Flags & (TemplateTypeFlag.Clear | TemplateTypeFlag.RandomCell)) != TemplateTypeFlag.None)
                icon = 0;
            return icon >= 0 && this.LandTypes != null && icon < this.LandTypes.Length ? this.LandTypes[icon] : LandType.Clear;
        }

        public string GetLandTypesString(char fillerChar, char separatorChar)
        {
            if ((this.Flags & (TemplateTypeFlag.Clear | TemplateTypeFlag.RandomCell)) != TemplateTypeFlag.None)
            {
                LandStringsMapping.TryGetValue(this.GetLandType(0), out char lt);
                return lt.ToString();
            }
            int arrLen = IconWidth * IconHeight;
            bool hasSeparator = separatorChar != '\0';
            if (hasSeparator)
            {
                arrLen += IconHeight - 1;
            }
            char[] maskArr = new char[arrLen];
            int icon = 0;
            int index = 0;
            for (int y = 0; y < IconHeight; ++y)
            {
                if (y > 0 && hasSeparator)
                {
                    maskArr[index++] = separatorChar;
                }
                for (int x = 0; x < IconWidth; ++x)
                {
                    if (!IconMask[y, x])
                    {
                        maskArr[index++] = fillerChar;
                    }
                    else
                    {
                        LandStringsMapping.TryGetValue(this.GetLandType(icon), out char lt);
                        maskArr[index++] = lt;
                    }
                    icon++;
                }
            }
            return new String(maskArr);
        }

        public static string GetMaskString(bool[,] mask, char filled, char clear, char separator)
        {
            int yMax = mask.GetLength(0);
            int xMax = mask.GetLength(1);
            int arrLen = xMax * yMax;
            bool hasSeparator = separator != '\0';
            if (hasSeparator)
            {
                arrLen += yMax - 1;
            }
            char[] maskArr = new char[arrLen];
            int index = 0;
            for (int y = 0; y < yMax; ++y)
            {
                if (y > 0 && hasSeparator)
                {
                    maskArr[index++] = separator;
                }
                for (int x = 0; x < xMax; ++x)
                {
                    maskArr[index++] = x < xMax && y < yMax && mask[y, x] ? filled : clear;
                }
            }
            return new string(maskArr);
        }

        public void InitDisplayName()
        {
            // Do nothing. Templates have no real UI names.
        }

        public void Init(GameInfo gameInfo, TheaterType theater, bool onlyIfFound)
        {
            Init(gameInfo, theater, false, onlyIfFound);
        }

        public void Init(GameInfo gameInfo, TheaterType theater, bool forceDummy, bool onlyIfFound)
        {
            this.ExistsInTheater = false;
            this.Initialised = false;
            bool isGroup = Flags.HasFlag(TemplateTypeFlag.Group);
            bool[,] mask = null;
            LandType[] landTypes = null;
            bool isRandom = Flags.HasFlag(TemplateTypeFlag.RandomCell);
            bool maskInit = false;
            bool fileInit = false;
            Bitmap oldImage = Thumbnail;
            GameType gameType = gameInfo.GameType;
            if (gameType == GameType.TiberianDawn || gameType == GameType.SoleSurvivor)
            {
                // Possibly read mask init from TD tiles rather than use hardcoded data? Not done atm.
                // maskInit = this.InitFromClassicFileTd(theater, out mask);
            }
            else if (gameType == GameType.RedAlert)
            {
                // This changes IconHeight, IconWidth and NumIcons.
                fileInit = this.InitFromClassicFileRa(theater, out mask, out landTypes);
                maskInit = fileInit;
            }
            if (!maskInit)
            {
                mask = new bool[IconHeight, IconWidth];
            }
            this.extraTilesFoundOn1x1 = false;
            int tilesLen = Globals.TheTilesetManager.GetTileDataLength(Name);
            // This allows mods to add 'random' tiles to existing 1x1 tiles. Check excludes 'Clear' terrain and items already defined as random.
            if (IconWidth == 1 & IconHeight == 1 && !Flags.HasFlag(TemplateTypeFlag.Clear) && (!isRandom || fileInit) && !isGroup)
            {
                if (tilesLen > 1)
                {
                    this.extraTilesFoundOn1x1 = true;
                    isRandom = true;
                }
            }
            Size tileSize = new Size(Globals.PreviewTileWidth, Globals.PreviewTileHeight);
            int loopWidth = IconWidth;
            int loopHeight = IconHeight;
            int numIcons = IconWidth * IconHeight;
            if (isRandom)
            {
                numIcons = Math.Max(1, isGroup ? GroupTiles.Length : tilesLen);
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
            Bitmap th = new Bitmap(loopWidth * tileSize.Width, loopHeight * tileSize.Height);
            th.SetResolution(96, 96);
            bool found = mask[0, 0];
            bool anyFound = false;
            using (Graphics g = Graphics.FromImage(th))
            {
                MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                g.Clear(Color.Transparent);
                int icon = 0;
                // If the requested tile is 100% definitely inside the bounds of what is supposed to have graphics, allow it to fetch dummy graphics.
                // Always allow dummy graphics for the first cell of a 1x1-random template.
                bool tryDummy = !onlyIfFound || forceDummy || (isRandom && IconWidth == 1 && IconHeight == 1) || NumIcons == 1;
                // Try to get mask; first specific per theater, then the general all-theaters mask.
                // The dictionary should only contain either a general one or one per theater, so the fetch order doesn't really matter.
                bool[,] maskOv = null;
                if (!maskInit && !isRandom && MaskOverrides.Keys.Count > 0 && !MaskOverrides.TryGetValue(theater.Name, out maskOv))
                    MaskOverrides.TryGetValue(String.Empty, out maskOv);
                for (int y = 0; y < loopHeight; ++y)
                {
                    for (int x = 0; x < loopWidth; ++x, ++icon)
                    {
                        bool fetchDummy = tryDummy;
                        if (icon >= NumIcons)
                        {
                            break;
                        }
                        if (maskOv != null)
                        {
                            if (!maskOv[y, x])
                                continue;
                            else // If forced, always fetch the graphics.
                                fetchDummy = true;
                        }
                        int iconToFetch = icon;
                        string nameToFetch = Name;
                        if (isGroup)
                        {
                            nameToFetch = GroupTiles[icon];
                            iconToFetch = 0;
                            fetchDummy = forceDummy;
                        }
                        if (maskInit && !isRandom && (loopWidth * loopHeight) > 1)
                        {
                            if (!mask[y, x])
                            {
                                // Don't bother.
                                continue;
                            }
                            found = true;
                        }
                        // Fetch dummy if definitely in bounds, first cell of a random one, or dummy is forced.
                        bool success = Globals.TheTilesetManager.GetTileData(nameToFetch, iconToFetch, out Tile tile, fetchDummy, false);
                        anyFound |= success;
                        if (tile != null && tile.Image != null)
                        {
                            using (Bitmap tileImg = tile.Image.RemoveAlpha())
                            {
                                g.DrawImage(tileImg, x * tileSize.Width, y * tileSize.Height, tileSize.Width, tileSize.Height);
                            }
                            if (!isRandom && !maskInit)
                            {
                                found = mask[y, x] = true;
                            }
                        }
                    }
                }
            }
            if (!found)
            {
                this.Initialised = false;
                this.ExistsInTheater = false;
                try { th.Dispose(); }
                catch { /* ignore */ }
            }
            else
            {
                this.Initialised = true;
                // If it 'exists' in the remastered tileset xml, it's supposed to exist,
                // so mark it as existing regardless of whether the graphics were found.
                this.ExistsInTheater = anyFound || tilesLen > 0;
            }
            Thumbnail = found ? th : null;
            IconMask = mask;
            if (oldImage != null)
            {
                try { oldImage.Dispose(); }
                catch { /* ignore */ }
            }
            if (!fileInit)
            {
                landTypes = landsDefault != null ? landsDefault.ToArray() : null;
            }
            if (landTypes == null && !isGroup)
            {
                int typeIcons = isRandom ? 1 : isGroup ? 0 : numIcons;
                landTypes = Enumerable.Repeat(LandType.Clear, typeIcons).ToArray();
            }
            // Randoms initialise to 1, then expand and copy that to all tiles.
            if (!isGroup && isRandom && landTypes.Length >= 1)
            {
                LandType rntp = landTypes[0];
                landTypes = Enumerable.Repeat(rntp, numIcons).ToArray();
            }
            LandTypes = landTypes;
        }

        private bool InitFromClassicFileRa(TheaterType theater, out bool[,] usageMask, out LandType[] landTypes)
        {
            usageMask = null;
            landTypes = null;
            if (Flags.HasFlag(TemplateTypeFlag.Group))
            {
                return false;
            }
            byte[] fileData = Globals.TheArchiveManager.ReadFileClassic(this.Name + "." + theater.ClassicExtension);
            if (fileData == null)
            {
                return false;
            }
            byte[][] data = ClassicSpriteLoader.GetRaTmpData(fileData, out _, out _, out byte[] landTypeInfo, out bool[] usage, out int width, out int height, false);
            if (data == null)
            {
                return false;
            }
            // This actually modifies the global static objects.
            this.IconHeight = Math.Max(1, height);
            this.IconWidth = Math.Max(1, width);
            this.NumIcons = this.IconWidth * this.IconHeight;
            landTypes = new LandType[NumIcons];
            int max = Math.Min(IconWidth * IconHeight, landTypeInfo.Length);
            for (int icon = 0; icon < max; ++icon)
            {
                byte val = landTypeInfo[icon];
                LandType land = val > tileTypeFromFile.Length ? LandType.Clear : tileTypeFromFile[landTypeInfo[icon]];
                landTypes[icon] = land;
            }
            usageMask = new bool[IconHeight, IconWidth];
            if (NumIcons > 1)
            {
                int frame = 0;
                for (int y = 0; y < IconHeight; ++y)
                {
                    for (int x = 0; x < IconWidth; ++x)
                    {
                        if (frame < max)
                        {
                            usageMask[y, x] = usage[frame++];
                        }
                    }
                }
            }
            else if (NumIcons == 1)
            {
                usageMask[0, 0] = true;
            }
            return true;
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

        public IEnumerable<Point> GetIconPoints(Point offset)
        {
            for (int y = 0; y < IconHeight; ++y)
            {
                for (int x = 0; x < IconWidth; ++x)
                {
                    if (IconMask[y, x])
                    {
                        yield return new Point(offset.X + x, offset.Y + y);
                    }
                }
            }
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
            bool isRandom = (Flags & TemplateTypeFlag.RandomCell) != TemplateTypeFlag.None;
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
            this.Initialised = false;
            this.ExistsInTheater = false;
            if (oldImage != null)
            {
                try { oldImage.Dispose(); }
                catch { /* ignore */ }
            }
        }

        /// <summary>
        /// The mapping to convert the tile type values in RA Templates to Land Types. According to the description in the
        /// RA code, these are simply a set of extra colors set in the original art files as metadata, used to indicate
        /// the types. They seem to match the standard Westwood EGA palette that all the pre-TS 8-bit palettes start with.
        /// </summary>
        public static readonly LandType[] tileTypeFromFile = new[]
        {
            LandType.Clear, /* Black      */ // Unused / Clear on 1x1 with alternates
            LandType.Clear, /* Purple     */ // ???
            LandType.Clear, /* Teal       */ // ???
            LandType.Clear, /* Green      */ // Clear
            LandType.Clear, /* Lime       */ // ???
            LandType.Clear, /* Yellow     */ // ???
            LandType.Beach, /* Light Red  */ // Beach
            LandType.Clear, /* Brown      */ // ???
            LandType.Rock,  /* Red        */ // Rock
            LandType.Road,  /* Cyan       */ // Road
            LandType.Water, /* Light Blue */ // Water
            LandType.River, /* Dark Blue  */ // River
            LandType.Clear, /* Black      */ // ???
            LandType.Clear, /* Dark gray  */ // ???
            LandType.Rough, /* Light gray */ // Rough
            LandType.Clear, /* White      */ // ???
        };

        public static readonly Dictionary<char, LandType> LandTypesMapping = new Dictionary<char, LandType>
        {
            { 'X', LandType.Clear }, // Filler tile, or [Clear] terrain on 1x1 sets with multiple tiles.
            { 'C', LandType.Clear }, // [Clear] Normal clear terrain.
            { 'B', LandType.Beach }, // [Beach] Sandy beach. Can't be built on.
            { 'I', LandType.Rock }, // [Rock]  Impassable terrain.
            { 'R', LandType.Road }, // [Road]  Units move faster on this terrain.
            { 'W', LandType.Water }, // [Water] Ships can travel over this.
            { 'V', LandType.River }, // [River] Ships normally can't travel over this.
            { 'H', LandType.Rough }, // [Rough] Rough terrain. Can't be built on
        };

        public static readonly Dictionary<LandType, char> LandStringsMapping = new Dictionary<LandType, char>
        {
            {LandType.Clear, 'C' }, // [Clear] Normal clear terrain.
            {LandType.Beach, 'B' }, // [Beach] Sandy beach. Can't be built on.
            {LandType.Rock, 'I' },  // [Rock]  Impassable terrain.
            {LandType.Road, 'R' },  // [Road]  Units move faster on this terrain.
            {LandType.Water, 'W' }, // [Water] Ships can travel over this.
            {LandType.River, 'V' }, // [River] Ships normally can't travel over this.
            {LandType.Rough, 'H' }, // [Rough] Rough terrain. Can't be built on
        };

        public static LandType[] GetLandTypesFromString(string types)
        {
            types = types.Replace(" ", String.Empty);
            LandType[] arr = new LandType[types.Length];
            char[] array = types.ToUpperInvariant().ToCharArray();
            for (int i = 0; i < array.Length; ++i)
            {
                arr[i] = LandTypesMapping.TryGetValue(array[i], out LandType t) ? t : LandType.Clear;
            }
            return arr;
        }
    }
}
