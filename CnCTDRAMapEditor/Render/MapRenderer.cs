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
using MobiusEditor.Model;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace MobiusEditor.Render
{
    public static class MapRenderer
    {
        private static readonly int[] Facing16 = new int[256]
        {
            0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,
            2,2,2,2,2,2,2,2,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,4,4,4,4,4,4,4,4,
            4,4,4,4,4,4,4,4,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,6,6,6,6,6,6,6,6,
            6,6,6,6,6,6,6,6,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,8,8,8,8,8,8,8,8,
            8,8,8,8,8,8,8,8,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,10,10,10,10,10,10,10,10,
            10,10,10,10,10,10,10,10,11,11,11,11,11,11,11,11,11,11,11,11,11,11,11,11,12,12,12,12,12,12,12,12,
            12,12,12,12,12,12,12,12,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,14,14,14,14,14,14,14,14,
            14,14,14,14,14,14,14,14,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,0,0,0,0,0,0,0,0
        };

        private static readonly int[] Facing32 = new int[256]
        {
            0,0,0,0,0,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,3,3,3,3,3,3,3,3,3,3,
            3,4,4,4,4,4,4,5,5,5,5,5,5,5,6,6,6,6,6,6,6,7,7,7,7,7,7,7,8,8,8,8,
            8,8,8,9,9,9,9,9,9,9,10,10,10,10,10,10,10,11,11,11,11,11,11,11,12,12,12,12,12,12,12,12,
            13,13,13,13,13,13,13,13,14,14,14,14,14,14,14,14,14,15,15,15,15,15,15,15,15,15,16,16,16,16,16,16,
            16,16,16,16,16,17,17,17,17,17,17,17,17,17,18,18,18,18,18,18,18,18,18,19,19,19,19,19,19,19,19,19,
            19,20,20,20,20,20,20,21,21,21,21,21,21,21,22,22,22,22,22,22,22,23,23,23,23,23,23,23,24,24,24,24,
            24,24,24,25,25,25,25,25,25,25,26,26,26,26,26,26,26,27,27,27,27,27,27,27,28,28,28,28,28,28,28,28,
            29,29,29,29,29,29,29,29,30,30,30,30,30,30,30,30,30,31,31,31,31,31,31,31,31,31,0,0,0,0,0,0
        };

        private static readonly int[] HumanShape = new int[32]
        {
            0,0,7,7,7,7,6,6,6,6,5,5,5,5,5,4,4,4,3,3,3,3,2,2,2,2,1,1,1,1,1,0
        };

        private static readonly int[] BodyShape = new int[32]
        {
            0,31,30,29,28,27,26,25,24,23,22,21,20,19,18,17,16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1
        };

        /// <summary>
        /// Cosine table. Technically signed bytes , but stored as 00-FF for simplicity.
        /// </summary>
        private static byte[] CosTable = {
            0x00, 0x03, 0x06, 0x09, 0x0c, 0x0f, 0x12, 0x15, 0x18, 0x1b, 0x1e, 0x21, 0x24, 0x27, 0x2a, 0x2d,
            0x30, 0x33, 0x36, 0x39, 0x3b, 0x3e, 0x41, 0x43, 0x46, 0x49, 0x4b, 0x4e, 0x50, 0x52, 0x55, 0x57,
            0x59, 0x5b, 0x5e, 0x60, 0x62, 0x64, 0x65, 0x67, 0x69, 0x6b, 0x6c, 0x6e, 0x6f, 0x71, 0x72, 0x74,
            0x75, 0x76, 0x77, 0x78, 0x79, 0x7a, 0x7b, 0x7b, 0x7c, 0x7d, 0x7d, 0x7e, 0x7e, 0x7e, 0x7e, 0x7e,

            0x7f, 0x7e, 0x7e, 0x7e, 0x7e, 0x7e, 0x7d, 0x7d, 0x7c, 0x7b, 0x7b, 0x7a, 0x79, 0x78, 0x77, 0x76,
            0x75, 0x74, 0x72, 0x71, 0x70, 0x6e, 0x6c, 0x6b, 0x69, 0x67, 0x66, 0x64, 0x62, 0x60, 0x5e, 0x5b,
            0x59, 0x57, 0x55, 0x52, 0x50, 0x4e, 0x4b, 0x49, 0x46, 0x43, 0x41, 0x3e, 0x3b, 0x39, 0x36, 0x33,
            0x30, 0x2d, 0x2a, 0x27, 0x24, 0x21, 0x1e, 0x1b, 0x18, 0x15, 0x12, 0x0f, 0x0c, 0x09, 0x06, 0x03,

            0x00, 0xfd, 0xfa, 0xf7, 0xf4, 0xf1, 0xee, 0xeb, 0xe8, 0xe5, 0xe2, 0xdf, 0xdc, 0xd9, 0xd6, 0xd3,
            0xd0, 0xcd, 0xca, 0xc7, 0xc5, 0xc2, 0xbf, 0xbd, 0xba, 0xb7, 0xb5, 0xb2, 0xb0, 0xae, 0xab, 0xa9,
            0xa7, 0xa5, 0xa2, 0xa0, 0x9e, 0x9c, 0x9a, 0x99, 0x97, 0x95, 0x94, 0x92, 0x91, 0x8f, 0x8e, 0x8c,
            0x8b, 0x8a, 0x89, 0x88, 0x87, 0x86, 0x85, 0x85, 0x84, 0x83, 0x83, 0x82, 0x82, 0x82, 0x82, 0x82,

            0x82, 0x82, 0x82, 0x82, 0x82, 0x82, 0x83, 0x83, 0x84, 0x85, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8a,
            0x8b, 0x8c, 0x8e, 0x8f, 0x90, 0x92, 0x94, 0x95, 0x97, 0x99, 0x9a, 0x9c, 0x9e, 0xa0, 0xa2, 0xa5,
            0xa7, 0xa9, 0xab, 0xae, 0xb0, 0xb2, 0xb5, 0xb7, 0xba, 0xbd, 0xbf, 0xc2, 0xc5, 0xc7, 0xca, 0xcd,
            0xd0, 0xd3, 0xd6, 0xd9, 0xdc, 0xdf, 0xe2, 0xe5, 0xe8, 0xeb, 0xee, 0xf1, 0xf4, 0xf7, 0xfa, 0xfd,
        };

        /// <summary>
        /// Sine table. Technically signed bytes , but stored as 00-FF for simplicity.
        /// </summary>
        private static byte[] SinTable = {
            0x7f, 0x7e, 0x7e, 0x7e, 0x7e, 0x7e, 0x7d, 0x7d, 0x7c, 0x7b, 0x7b, 0x7a, 0x79, 0x78, 0x77, 0x76,
            0x75, 0x74, 0x72, 0x71, 0x70, 0x6e, 0x6c, 0x6b, 0x69, 0x67, 0x66, 0x64, 0x62, 0x60, 0x5e, 0x5b,
            0x59, 0x57, 0x55, 0x52, 0x50, 0x4e, 0x4b, 0x49, 0x46, 0x43, 0x41, 0x3e, 0x3b, 0x39, 0x36, 0x33,
            0x30, 0x2d, 0x2a, 0x27, 0x24, 0x21, 0x1e, 0x1b, 0x18, 0x15, 0x12, 0x0f, 0x0c, 0x09, 0x06, 0x03,

            0x00, 0xfd, 0xfa, 0xf7, 0xf4, 0xf1, 0xee, 0xeb, 0xe8, 0xe5, 0xe2, 0xdf, 0xdc, 0xd9, 0xd6, 0xd3,
            0xd0, 0xcd, 0xca, 0xc7, 0xc5, 0xc2, 0xbf, 0xbd, 0xba, 0xb7, 0xb5, 0xb2, 0xb0, 0xae, 0xab, 0xa9,
            0xa7, 0xa5, 0xa2, 0xa0, 0x9e, 0x9c, 0x9a, 0x99, 0x97, 0x95, 0x94, 0x92, 0x91, 0x8f, 0x8e, 0x8c,
            0x8b, 0x8a, 0x89, 0x88, 0x87, 0x86, 0x85, 0x85, 0x84, 0x83, 0x83, 0x82, 0x82, 0x82, 0x82, 0x82,

            0x82, 0x82, 0x82, 0x82, 0x82, 0x82, 0x83, 0x83, 0x84, 0x85, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8a,
            0x8b, 0x8c, 0x8e, 0x8f, 0x90, 0x92, 0x94, 0x95, 0x97, 0x99, 0x9a, 0x9c, 0x9e, 0xa0, 0xa2, 0xa5,
            0xa7, 0xa9, 0xab, 0xae, 0xb0, 0xb2, 0xb5, 0xb7, 0xba, 0xbd, 0xbf, 0xc2, 0xc5, 0xc7, 0xca, 0xcd,
            0xd0, 0xd3, 0xd6, 0xd9, 0xdc, 0xdf, 0xe2, 0xe5, 0xe8, 0xeb, 0xee, 0xf1, 0xf4, 0xf7, 0xfa, 0xfd,

            0x00, 0x03, 0x06, 0x09, 0x0c, 0x0f, 0x12, 0x15, 0x18, 0x1b, 0x1e, 0x21, 0x24, 0x27, 0x2a, 0x2d,
            0x30, 0x33, 0x36, 0x39, 0x3b, 0x3e, 0x41, 0x43, 0x46, 0x49, 0x4b, 0x4e, 0x50, 0x52, 0x55, 0x57,
            0x59, 0x5b, 0x5e, 0x60, 0x62, 0x64, 0x65, 0x67, 0x69, 0x6b, 0x6c, 0x6e, 0x6f, 0x71, 0x72, 0x74,
            0x75, 0x76, 0x77, 0x78, 0x79, 0x7a, 0x7b, 0x7b, 0x7c, 0x7d, 0x7d, 0x7e, 0x7e, 0x7e, 0x7e, 0x7e,
        };

        private static void MovePoint(ref int x, ref int y, byte dir, int distance, int perspectiveDivider)
        {
            x += ((sbyte)CosTable[dir] * distance) >> 7;
            y += -(((sbyte)SinTable[dir] * distance / perspectiveDivider) >> 7);
        }

        private static readonly short[] HeliDistanceAdjust = { 8, 9, 10, 9, 8, 9, 10, 9 };

        private static readonly Point[] BackTurretAdjust = new Point[]
        {
            new Point(1, 2),    // N
            new Point(-1, 1),
            new Point(-2, 0),
            new Point(-3, 0),
            new Point(-3, 1),   // NW
            new Point(-4, -1),
            new Point(-4, -1),
            new Point(-5, -2),
            new Point(-5, -3),  // W
            new Point(-5, -3),
            new Point(-3, -3),
            new Point(-3, -4),
            new Point(-3, -4),  // SW
            new Point(-3, -5),
            new Point(-2, -5),
            new Point(-1, -5),
            new Point(0, -5),   // S
            new Point(1, -6),
            new Point(2, -5),
            new Point(3, -5),
            new Point(4, -5),   // SE
            new Point(6, -4),
            new Point(6, -3),
            new Point(6, -3),
            new Point(6, -3),   // E
            new Point(5, -1),
            new Point(5, -1),
            new Point(4, 0),
            new Point(3, 0),    // NE
            new Point(2, 0),
            new Point(2, 1),
            new Point(1, 2)
        };

        private static readonly int randomSeed;

        static MapRenderer()
        {
            randomSeed = Guid.NewGuid().GetHashCode();
        }

        public static void Render(GameType gameType, Map map, Graphics graphics, ISet<Point> locations, MapLayerFlag layers, double tileScale)
        {
            var tileSize = new Size(Math.Max(1, (int)(Globals.OriginalTileWidth * tileScale)), Math.Max(1, (int)(Globals.OriginalTileHeight * tileScale)));
            var tiberiumOrGoldTypes = map.OverlayTypes.Where(t => t.IsTiberiumOrGold).Select(t => t).ToArray();
            var gemTypes = map.OverlayTypes.Where(t => t.IsGem).ToArray();
            // paint position, paint action, true if flat.
            var overlappingRenderList = new List<(Rectangle, Action<Graphics>, bool)>();
            Func<IEnumerable<Point>> renderLocations = null;
            if (locations != null)
            {
                renderLocations = () => locations;
            }
            else
            {
                IEnumerable<Point> allCells()
                {
                    for (var y = 0; y < map.Metrics.Height; ++y)
                    {
                        for (var x = 0; x < map.Metrics.Width; ++x)
                        {
                            yield return new Point(x, y);
                        }
                    }
                }
                renderLocations = allCells;
            }
            if ((layers & MapLayerFlag.Template) != MapLayerFlag.None)
            {
                TemplateType clear = map.TemplateTypes.Where(t => t.Flag == TemplateTypeFlag.Clear).FirstOrDefault();
                foreach (var topLeft in renderLocations())
                {
                    map.Metrics.GetCell(topLeft, out int cell);
                    var template = map.Templates[topLeft];
                    TemplateType ttype = template?.Type ?? clear;
                    var name = ttype.Name;
                    var icon = template?.Icon ?? ((cell & 0x03) | ((cell >> 4) & 0x0C));
                    if ((ttype.Flag & TemplateTypeFlag.Group) == TemplateTypeFlag.Group)
                    {
                        name = ttype.GroupTiles[icon];
                        icon = 0;
                    }
                    // If it's actually placed on the map, show it, even if it has no graphics.
                    if (Globals.TheTilesetManager.GetTileData(map.Theater.Tilesets, name, icon, out Tile tile, true, false))
                    {
                        var renderBounds = new Rectangle(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height, tileSize.Width, tileSize.Height);
                        if(tile.Image != null)
                        {
                            //graphics.DrawImage(tile.Image, renderBounds);
                            using (Bitmap tileImg = tile.Image.RemoveAlpha())
                            {
                                graphics.DrawImage(tileImg, renderBounds);
                            }
                        }
                    }
                    else
                    {
                        Debug.Print(string.Format("Template {0} ({1}) not found", name, icon));
                    }
                }
            }
            // Attached bibs are counted under Buildings, not Smudge.
            if ((layers & MapLayerFlag.Buildings) != MapLayerFlag.None)
            {
                foreach (var topLeft in renderLocations())
                {
                    var smudge = map.Smudge[topLeft];
                    if (smudge != null && smudge.Type.IsAutoBib)
                    {
                        RenderSmudge(map.Theater, topLeft, tileSize, tileScale, smudge).Item2(graphics);
                    }
                }
            }
            if ((layers & MapLayerFlag.Smudge) != MapLayerFlag.None)
            {
                foreach (var topLeft in renderLocations())
                {
                    var smudge = map.Smudge[topLeft];
                    if (smudge != null && !smudge.Type.IsAutoBib)
                    {
                        RenderSmudge(map.Theater, topLeft, tileSize, tileScale, smudge).Item2(graphics);
                    }
                }
            }

            if ((layers & MapLayerFlag.OverlayAll) != MapLayerFlag.None)
            {
                foreach (var location in renderLocations())
                {
                    var overlay = map.Overlay[location];
                    if (overlay == null)
                    {
                        continue;
                    }
                    if (Globals.CratesOnTop && overlay.Type.IsCrate && (layers & MapLayerFlag.Overlay) != MapLayerFlag.None)
                    {
                        // if "CratesOnTop" logic is active, crates are skipped here and painted afterwards.
                        continue;
                    }
                    bool paintAsWall = overlay.Type.IsWall && (layers & MapLayerFlag.Walls) != MapLayerFlag.None;
                    bool paintAsResource = overlay.Type.IsResource && (layers & MapLayerFlag.Resources) != MapLayerFlag.None;
                    bool paintAsOverlay = overlay.Type.IsOverlay && (layers & MapLayerFlag.Overlay) != MapLayerFlag.None;
                    if (paintAsWall || paintAsResource || paintAsOverlay)
                    {
                        RenderOverlay(gameType, map.Theater, tiberiumOrGoldTypes, gemTypes, location, tileSize, tileScale, overlay).Item2(graphics);
                    }
                }
            }

            if ((layers & MapLayerFlag.Terrain) != MapLayerFlag.None)
            {
                foreach (var (topLeft, terrain) in map.Technos.OfType<Terrain>())
                {
                    if ((locations != null) && !locations.Contains(topLeft))
                    {
                        continue;
                    }
                    overlappingRenderList.Add(RenderTerrain(gameType, map.Theater, topLeft, tileSize, tileScale, terrain));
                }
            }
            if ((layers & MapLayerFlag.Buildings) != MapLayerFlag.None)
            {
                foreach (var (topLeft, building) in map.Buildings.OfType<Building>())
                {
                    if ((locations != null) && !locations.Contains(topLeft))
                    {
                        continue;
                    }
                    overlappingRenderList.Add(RenderBuilding(gameType, map.Theater, topLeft, tileSize, tileScale, building));
                }
            }
            if ((layers & MapLayerFlag.Infantry) != MapLayerFlag.None)
            {
                foreach (var (topLeft, infantryGroup) in map.Technos.OfType<InfantryGroup>())
                {
                    if ((locations != null) && !locations.Contains(topLeft))
                    {
                        continue;
                    }
                    for (int i = 0; i < infantryGroup.Infantry.Length; ++i)
                    {
                        var infantry = infantryGroup.Infantry[i];
                        if (infantry == null)
                        {
                            continue;
                        }
                        overlappingRenderList.Add(RenderInfantry(map.Theater, topLeft, tileSize, infantry, (InfantryStoppingType)i));
                    }
                }
            }
            if ((layers & MapLayerFlag.Units) != MapLayerFlag.None)
            {
                foreach (var (topLeft, unit) in map.Technos.OfType<Unit>())
                {
                    if ((locations != null) && !locations.Contains(topLeft))
                    {
                        continue;
                    }
                    overlappingRenderList.Add(RenderUnit(gameType, map.Theater, topLeft, tileSize, unit));
                }
            }
            // Paint flat items (like the repair bay)
            //.OrderBy(x => x.Item1.Bottom) --> .OrderBy(x => x.Item1.CenterPoint().Y)
            foreach (var (location, renderer, flat) in overlappingRenderList.Where(x => !x.Item1.IsEmpty && x.Item3).OrderBy(x => x.Item1.CenterPoint().Y))
            {
                renderer(graphics);
            }
            // Paint all the rest
            foreach (var (location, renderer, flat) in overlappingRenderList.Where(x => !x.Item1.IsEmpty && !x.Item3).OrderBy(x => x.Item1.CenterPoint().Y))
            {
                renderer(graphics);
            }
            if (Globals.CratesOnTop && (layers & MapLayerFlag.Overlay) != MapLayerFlag.None)
            {
                foreach (var topLeft in renderLocations())
                {
                    var overlay = map.Overlay[topLeft];
                    if (overlay == null || !overlay.Type.IsCrate)
                    {
                        continue;
                    }
                    RenderOverlay(gameType, map.Theater, tiberiumOrGoldTypes, gemTypes, topLeft, tileSize, tileScale, overlay).Item2(graphics);
                }
            }

            if ((layers & MapLayerFlag.Waypoints) != MapLayerFlag.None)
            {
                // todo avoid overlapping waypoints of the same type?
                HashSet<int> handledPoints = new HashSet<int>();
                ITeamColor[] flagColors = map.FlagColors.ToArray();
                foreach (Waypoint waypoint in map.Waypoints)
                {
                    if (!waypoint.Point.HasValue || (locations != null && !locations.Contains(waypoint.Point.Value)))
                    {
                        continue;
                    }
                    RenderWaypoint(gameType, map.BasicSection.SoloMission, map.Theater, tileSize, flagColors, waypoint).Item2(graphics);
                }
            }
        }

        public static void Render(GameType gameType, Map map, Graphics graphics, ISet<Point> locations, MapLayerFlag layers)
        {
            Render(gameType, map, graphics, locations, layers, Globals.MapTileScale);
        }

        public static (Rectangle, Action<Graphics>) RenderSmudge(TheaterType theater, Point topLeft, Size tileSize, double tileScale, Smudge smudge)
        {
            var tint = smudge.Tint;
            var imageAttributes = new ImageAttributes();
            if (tint != Color.White)
            {
                var colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] {tint.R / 255.0f, 0, 0, 0, 0},
                    new float[] {0, tint.G / 255.0f, 0, 0, 0},
                    new float[] {0, 0, tint.B / 255.0f, 0, 0},
                    new float[] {0, 0, 0, tint.A / 255.0f, 0},
                    new float[] {0, 0, 0, 0, 1},
                }
                );
                imageAttributes.SetColorMatrix(colorMatrix);
            }
            if (Globals.TheTilesetManager.GetTileData(theater.Tilesets, smudge.Type.Name, smudge.Icon, out Tile tile))
            {
                Rectangle smudgeBounds = RenderBounds(tile.Image.Size, new Size(1, 1), tileScale);
                smudgeBounds.X += topLeft.X * tileSize.Width;
                smudgeBounds.Y += topLeft.Y * tileSize.Width;
                void render(Graphics g)
                {
                    g.DrawImage(tile.Image, smudgeBounds, 0, 0, tile.Image.Width, tile.Image.Height, GraphicsUnit.Pixel, imageAttributes);
                }
                return (smudgeBounds, render);
            }
            else
            {
                Debug.Print(string.Format("Smudge {0} ({1}) not found", smudge.Type.Name, smudge.Icon));
                return (Rectangle.Empty, (g) => { });
            }
        }

        public static (Rectangle, Action<Graphics>) RenderOverlay(GameType gameType, TheaterType theater, OverlayType[] tiberiumOrGoldTypes, OverlayType[] gemTypes, Point topLeft, Size tileSize, double tileScale, Overlay overlay)
        {
            string name;
            OverlayType ovtype = overlay.Type;
            if (ovtype.IsGem && gemTypes != null)
            {
                name = gemTypes[new Random(randomSeed ^ topLeft.GetHashCode()).Next(gemTypes.Length)].Name;
            }
            else if (ovtype.IsTiberiumOrGold && tiberiumOrGoldTypes != null)
            {
                name = tiberiumOrGoldTypes[new Random(randomSeed ^ topLeft.GetHashCode()).Next(tiberiumOrGoldTypes.Length)].Name;
            }
            else
            {
                name = ovtype.GraphicsSource;
            }
            int icon = ovtype.IsConcrete || ovtype.IsResource || ovtype.IsWall || ovtype.ForceTileNr == -1 ? overlay.Icon : ovtype.ForceTileNr;
            bool isTeleport = gameType == GameType.SoleSurvivor && ovtype == SoleSurvivor.OverlayTypes.Teleport && Globals.AdjustSoleTeleports;
            // For Decoration types, generate dummy if not found.
            if (Globals.TheTilesetManager.GetTileData(theater.Tilesets, name, icon, out Tile tile, (ovtype.Flag & OverlayTypeFlag.Pavement) != 0, false))
            {
                int actualTopLeftX = topLeft.X * tileSize.Width;
                int actualTopLeftY = topLeft.Y * tileSize.Height;
                Rectangle overlayBounds = RenderBounds(tile.Image.Size, new Size(1, 1), tileScale);
                overlayBounds.X += actualTopLeftX;
                overlayBounds.Y += actualTopLeftY;
                var tint = overlay.Tint;
                // unused atm
                var brightness = 1.0f;
                void render(Graphics g)
                {
                    var imageAttributes = new ImageAttributes();
                    if (tint != Color.White || brightness != 1.0f)
                    {
                        var colorMatrix = new ColorMatrix(new float[][]
                        {
                            new float[] {tint.R * brightness / 255.0f, 0, 0, 0, 0},
                            new float[] {0, tint.G * brightness / 255.0f, 0, 0, 0},
                            new float[] {0, 0, tint.B * brightness / 255.0f, 0, 0},
                            new float[] {0, 0, 0, tint.A / 255.0f, 0},
                            new float[] {0, 0, 0, 0, 1},
                        }
                        );
                        imageAttributes.SetColorMatrix(colorMatrix);
                    }
                    g.DrawImage(tile.Image, overlayBounds, 0, 0, tile.Image.Width, tile.Image.Height, GraphicsUnit.Pixel, imageAttributes);
                    if (isTeleport)
                    {
                        // Transform ROAD tile into the teleport from SS.
                        int blackBorderX = Math.Max(1, tileSize.Width / 24);
                        int blackBorderY = Math.Max(1, tileSize.Height / 24);
                        int blueWidth = tileSize.Width - blackBorderX * 2;
                        int blueHeight = tileSize.Height - blackBorderY * 2;
                        int blackWidth = tileSize.Width - blackBorderX * 4;
                        int blackHeight = tileSize.Height - blackBorderY * 4;
                        using (SolidBrush blue = new SolidBrush(Color.FromArgb(92, 164, 200)))
                        using (SolidBrush black = new SolidBrush(Color.Black))
                        {
                            g.FillRectangle(blue, actualTopLeftX + blackBorderX, actualTopLeftY + blackBorderY, blueWidth, blueHeight);
                            g.FillRectangle(black, actualTopLeftX + blackBorderX * 2, actualTopLeftY + blackBorderY * 2, blackWidth, blackHeight);
                        }
                    }
                }
                return (overlayBounds, render);
            }
            else
            {
                Debug.Print(string.Format("Overlay {0} ({1}) not found", name, icon));
                return (Rectangle.Empty, (g) => { });
            }
        }

        public static (Rectangle, Action<Graphics>, bool) RenderTerrain(GameType gameType, TheaterType theater, Point topLeft, Size tileSize, double tileScale, Terrain terrain)
        {
            string tileName = terrain.Type.GraphicsSource;
            if (!Globals.TheTilesetManager.GetTileData(theater.Tilesets, tileName, terrain.Type.DisplayIcon, out Tile tile))
            {
                Debug.Print(string.Format("Terrain {0} ({1}) not found", tileName, terrain.Type.DisplayIcon));
                return (Rectangle.Empty, (g) => { }, false);
            }
            var tint = terrain.Tint;
            var imageAttributes = new ImageAttributes();
            if (tint != Color.White)
            {
                var colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] {tint.R / 255.0f, 0, 0, 0, 0},
                    new float[] {0, tint.G / 255.0f, 0, 0, 0},
                    new float[] {0, 0, tint.B / 255.0f, 0, 0},
                    new float[] {0, 0, 0, tint.A / 255.0f, 0},
                    new float[] {0, 0, 0, 0, 1},
                });
                imageAttributes.SetColorMatrix(colorMatrix);
            }
            var location = new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height);
            var maxSize = new Size(terrain.Type.Size.Width * tileSize.Width, terrain.Type.Size.Height * tileSize.Height);
            Rectangle paintBounds = RenderBounds(tile.Image.Size, terrain.Type.Size, tileScale);
            paintBounds.X += location.X;
            paintBounds.Y += location.Y;
            var terrainBounds = new Rectangle(location, maxSize);
            void render(Graphics g)
            {
                g.DrawImage(tile.Image, paintBounds, 0, 0, tile.Image.Width, tile.Image.Height, GraphicsUnit.Pixel, imageAttributes);
            }
            return (terrainBounds, render, false);
        }

        public static (Rectangle, Action<Graphics>, bool) RenderBuilding(GameType gameType, TheaterType theater, Point topLeft, Size tileSize, double tileScale, Building building)
        {
            var tint = building.Tint;
            var icon = building.Type.FrameOFfset;
            int maxIcon = 0;
            int damageIconOffs = 0;
            int collapseIcon = 0;
            // In TD, damage is when BELOW the threshold. In RA, it's ON the threshold.
            int healthyMin = gameType == GameType.RedAlert ? 128 : 127;
            bool isDamaged = building.Strength <= healthyMin;
            bool hasCollapseFrame = false;
            // Only fetch if damaged. BuildingType.IsSingleFrame is an override for the RA mines. Everything else works with one simple logic.
            if (isDamaged && !building.Type.IsSingleFrame)
            {
                maxIcon = Globals.TheTilesetManager.GetTileDataLength(theater.Tilesets, building.Type.GraphicsSource);
                hasCollapseFrame = (gameType == GameType.TiberianDawn || gameType == GameType.SoleSurvivor) && maxIcon > 1 && maxIcon % 2 == 1;
                damageIconOffs = maxIcon / 2;
                collapseIcon = maxIcon - 1;
            }
            if (building.Type.HasTurret)
            {
                icon += BodyShape[Facing32[building.Direction.ID]];
                if (isDamaged)
                {
                    icon += damageIconOffs;
                }
            }
            else
            {
                if (building.Strength <= 1 && hasCollapseFrame)
                {
                    icon = collapseIcon;
                }
                else if (isDamaged)
                {
                    icon += damageIconOffs;
                }
            }
            ITeamColor tc = building.Type.CanRemap ? Globals.TheTeamColorManager[building.House.BuildingTeamColor] : null;
            if (Globals.TheTilesetManager.GetTeamColorTileData(theater.Tilesets, building.Type.GraphicsSource, icon, tc, out Tile tile))
            {
                var location = new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height);
                var maxSize = new Size(building.Type.Size.Width * tileSize.Width, building.Type.Size.Height * tileSize.Height);
                var paintBounds = RenderBounds(tile.Image.Size, building.Type.Size, tileScale);
                var buildingBounds = new Rectangle(location, maxSize);
                Tile factoryOverlayTile = null;
                // Draw no factory overlay over the collapse frame.
                if (building.Type.FactoryOverlay != null && (building.Strength > 1 || !hasCollapseFrame))
                {
                    int overlayIcon = 0;
                    if (building.Strength <= healthyMin)
                    {
                        int maxOverlayIcon = Globals.TheTilesetManager.GetTileDataLength(theater.Tilesets, building.Type.FactoryOverlay);
                        overlayIcon = maxOverlayIcon / 2;
                    }
                    Globals.TheTilesetManager.GetTeamColorTileData(theater.Tilesets, building.Type.FactoryOverlay, overlayIcon, Globals.TheTeamColorManager[building.House.BuildingTeamColor], out factoryOverlayTile);
                }
                void render(Graphics g)
                {
                    var imageAttributes = new ImageAttributes();
                    if (tint != Color.White)
                    {
                        var colorMatrix = new ColorMatrix(new float[][]
                        {
                            new float[] {tint.R / 255.0f, 0, 0, 0, 0},
                            new float[] {0, tint.G / 255.0f, 0, 0, 0},
                            new float[] {0, 0, tint.B / 255.0f, 0, 0},
                            new float[] {0, 0, 0, tint.A / 255.0f, 0},
                            new float[] {0, 0, 0, 0, 1},
                        }
                        );
                        imageAttributes.SetColorMatrix(colorMatrix);
                    }
                    if (factoryOverlayTile != null)
                    {
                        // Avoid overlay showing as semiitransparent.
                        using (Bitmap factory = new Bitmap(maxSize.Width, maxSize.Height))
                        {
                            factory.SetResolution(96, 96);
                            using (Graphics factoryG = Graphics.FromImage(factory))
                            {
                                var factBounds = RenderBounds(tile.Image.Size, building.Type.Size, tileScale);
                                var ovrlBounds = RenderBounds(factoryOverlayTile.Image.Size, building.Type.Size, tileScale);
                                factoryG.DrawImage(tile.Image, factBounds, 0, 0, tile.Image.Width, tile.Image.Height, GraphicsUnit.Pixel);
                                factoryG.DrawImage(factoryOverlayTile.Image, ovrlBounds, 0, 0, factoryOverlayTile.Image.Width, factoryOverlayTile.Image.Height, GraphicsUnit.Pixel);
                            }
                            g.DrawImage(factory, buildingBounds, 0, 0, factory.Width, factory.Height, GraphicsUnit.Pixel, imageAttributes);
                        }
                    }
                    else
                    {
                        paintBounds.X += location.X;
                        paintBounds.Y += location.Y;
                        g.DrawImage(tile.Image, paintBounds, 0, 0, tile.Image.Width, tile.Image.Height, GraphicsUnit.Pixel, imageAttributes);
                    }
                }
                // "is flat" is for buildings that have pieces sticking out at the top that should not overlap objects on these cells.
                return (buildingBounds, render, building.Type.IsFlat);
            }
            else
            {
                Debug.Print(string.Format("Building {0} ({1}) not found", building.Type.Name, icon));
                return (Rectangle.Empty, (g) => { }, false);
            }
        }

        public static (Rectangle, Action<Graphics>, bool) RenderInfantry(TheaterType theater, Point topLeft, Size tileSize, Infantry infantry, InfantryStoppingType infantryStoppingType)
        {
            var icon = HumanShape[Facing32[infantry.Direction.ID]];
            string teamColor = infantry.House?.UnitTeamColor;
            if (Globals.TheTilesetManager.GetTeamColorTileData(theater.Tilesets, infantry.Type.Name, icon, Globals.TheTeamColorManager[teamColor], out Tile tile))
            {
                // These values are experimental, from comparing map editor screenshots to game screenshots. -Nyer
                int infantryCorrectX = tileSize.Width / -12;
                int infantryCorrectY = tileSize.Height / 6;
                var baseLocation = new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height)
                    + new Size(tileSize.Width / 2, tileSize.Height / 2);
                var offset = Point.Empty;
                switch (infantryStoppingType)
                {
                    case InfantryStoppingType.UpperLeft:
                        offset.X = -tileSize.Width / 4 + infantryCorrectX;
                        offset.Y = -tileSize.Height / 4 + infantryCorrectY;
                        break;
                    case InfantryStoppingType.UpperRight:
                        offset.X = tileSize.Width / 4 + infantryCorrectX;
                        offset.Y = -tileSize.Height / 4 + infantryCorrectY;
                        break;
                    case InfantryStoppingType.LowerLeft:
                        offset.X = -tileSize.Width / 4 + infantryCorrectX;
                        offset.Y = tileSize.Height / 4 + infantryCorrectY;
                        break;
                    case InfantryStoppingType.LowerRight:
                        offset.X = tileSize.Width / 4 + infantryCorrectX;
                        offset.Y = tileSize.Height / 4 + infantryCorrectY;
                        break;
                    case InfantryStoppingType.Center:
                        offset.X = infantryCorrectX;
                        offset.Y = infantryCorrectY;
                        break;
                }
                baseLocation.Offset(offset);
                var virtualBounds = new Rectangle(
                    new Point(baseLocation.X - (tile.OpaqueBounds.Width / 2), baseLocation.Y - tile.OpaqueBounds.Height),
                    tile.OpaqueBounds.Size
                );
                var renderSize = infantry.Type.GetRenderSize(tileSize);
                var renderBounds = new Rectangle(baseLocation - new Size(renderSize.Width / 2, renderSize.Height / 2), renderSize);

                var tint = infantry.Tint;
                void render(Graphics g)
                {
                    var imageAttributes = new ImageAttributes();
                    if (tint != Color.White)
                    {
                        var colorMatrix = new ColorMatrix(new float[][]
                        {
                            new float[] {tint.R / 255.0f, 0, 0, 0, 0},
                            new float[] {0, tint.G / 255.0f, 0, 0, 0},
                            new float[] {0, 0, tint.B / 255.0f, 0, 0},
                            new float[] {0, 0, 0, tint.A / 255.0f, 0},
                            new float[] {0, 0, 0, 0, 1},
                        }
                        );
                        imageAttributes.SetColorMatrix(colorMatrix);
                    }
                    g.DrawImage(tile.Image, renderBounds, 0, 0, tile.Image.Width, tile.Image.Height, GraphicsUnit.Pixel, imageAttributes);
                }
                return (virtualBounds, render, false);
            }
            else
            {
                Debug.Print(string.Format("Infantry {0} ({1}) not found", infantry.Type.Name, icon));
                return (Rectangle.Empty, (g) => { }, false);
            }
        }

        public static (Rectangle, Action<Graphics>, bool) RenderUnit(GameType gameType, TheaterType theater, Point topLeft, Size tileSize, Unit unit)
        {
            int icon = -1;
            if (gameType == GameType.TiberianDawn || gameType == GameType.SoleSurvivor)
            {
                if ((unit.Type == TiberianDawn.UnitTypes.Tric) ||
                         (unit.Type == TiberianDawn.UnitTypes.Trex) ||
                         (unit.Type == TiberianDawn.UnitTypes.Rapt) ||
                         (unit.Type == TiberianDawn.UnitTypes.Steg))
                {
                    var facing = ((unit.Direction.ID + 0x10) & 0xFF) >> 5;
                    icon = BodyShape[facing + ((facing > 0) ? 24 : 0)];
                }
                else if ((unit.Type == TiberianDawn.UnitTypes.Hover) ||
                         (unit.Type == TiberianDawn.UnitTypes.Visceroid))
                {
                    icon = 0;
                }
                else if (unit.Type == TiberianDawn.UnitTypes.GunBoat)
                {
                    icon = BodyShape[Facing32[unit.Direction.ID]];
                    // East facing is not actually possible to set in missions. This is just the turret facing.
                    // In TD, damage is when BELOW the threshold. In RA, it's ON the threshold.
                    if (unit.Strength < 128)
                        icon += 32;
                    if (unit.Strength < 64)
                        icon += 32;
                }
                
            }
            else if (gameType == GameType.RedAlert)
            {
                if (unit.Type.IsAircraft && unit.Type.IsFixedWing)
                {
                    icon = BodyShape[Facing16[unit.Direction.ID] * 2] / 2;
                }
                else if (unit.Type.IsVessel)
                {
                    if ((unit.Type == RedAlert.UnitTypes.Transport) ||
                        (unit.Type == RedAlert.UnitTypes.Carrier))
                    {
                        icon = 0;
                    }
                    else
                    {
                        icon = BodyShape[Facing16[unit.Direction.ID] * 2] / 2;
                    }
                }
                else if ((unit.Type == RedAlert.UnitTypes.Ant1) ||
                        (unit.Type == RedAlert.UnitTypes.Ant2) ||
                        (unit.Type == RedAlert.UnitTypes.Ant3))
                {
                    icon = ((BodyShape[Facing32[unit.Direction.ID]] + 2) / 4) & 0x07;
                }
            }
            // Default behaviour for 32-frame units.
            if (icon == -1)
            {
                icon = BodyShape[Facing32[unit.Direction.ID]];
            }
            string teamColor = null;
            if (unit.House != null)
            {
                if (!unit.House.OverrideTeamColors.TryGetValue(unit.Type.Name, out teamColor))
                {
                    teamColor = unit.House.UnitTeamColor;
                }
            }
            if (!Globals.TheTilesetManager.GetTeamColorTileData(theater.Tilesets, unit.Type.Name, icon, Globals.TheTeamColorManager[teamColor], out Tile tile))
            {
                Debug.Print(string.Format("Unit {0} ({1}) not found", unit.Type.Name, icon));
                return (Rectangle.Empty, (g) => { }, false);
            }
            var location =
                new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height) +
                new Size(tileSize.Width / 2, tileSize.Height / 2);
            var renderSize = unit.Type.GetRenderSize(tileSize);
            var renderRect = new Rectangle(new Point(0, 0), renderSize);
            var renderBounds = new Rectangle(location - new Size(renderSize.Width / 2, renderSize.Height / 2), renderSize);
            Tile turretTile = null;
            Tile turret2Tile = null;
            if (unit.Type.HasTurret)
            {
                string turretName = unit.Type.Turret ?? unit.Type.Name;
                string turret2Name = unit.Type.HasDoubleTurret ? unit.Type.SecondTurret ?? unit.Type.Turret ?? unit.Type.Name : null;
                int turretIcon = unit.Type.Name.Equals(turretName, StringComparison.OrdinalIgnoreCase) ? icon + 32 : icon;
                int turret2Icon = unit.Type.Name.Equals(turret2Name, StringComparison.OrdinalIgnoreCase) ? icon + 32 : icon;
                if (gameType == GameType.RedAlert)
                {
                    if (unit.Type == RedAlert.UnitTypes.Phase)
                    {
                        // Compensate for unload frames.
                        turretIcon += 6;
                    }
                    else if (unit.Type == RedAlert.UnitTypes.MGG)
                    {
                        // 16-frame rotation, but saved as 8 frames because the other 8 are identical.
                        turretIcon = 32 + ((icon / 2) & 7);
                    }
                    else if (unit.Type == RedAlert.UnitTypes.Tesla)
                    {
                        // Fixed turret frame.
                        turretIcon = 32;
                    }
                }
                if (unit.Type.IsVessel)
                {
                    // Ships have 32-frame turrets on a 16-frame body.
                    turretIcon = BodyShape[Facing32[unit.Direction.ID]];
                    turret2Icon = BodyShape[Facing32[unit.Direction.ID]];
                }
                else if (unit.Type.IsAircraft)
                {
                    int getRotorIcon(string turrName, int dir, int turrIcon)
                    {
                        // Bit-wise ToUpper() for ASCII ;)
                        if (!String.IsNullOrEmpty(turrName) && (turrName[0] & 0xDF) == 'L')
                        {
                            turrIcon = (dir >> 5) % 2 == 1 ? 9 : 5;
                        }
                        if (!String.IsNullOrEmpty(turrName) && (turrName[0] & 0xDF) == 'R')
                        {
                            turrIcon = (dir >> 5) % 2 == 1 ? 8 : 4;
                        }
                        return turrIcon;
                    }
                    turretIcon = getRotorIcon(turretName, unit.Direction.ID, turretIcon);
                    turret2Icon = getRotorIcon(turret2Name, unit.Direction.ID, turret2Icon);
                }
                if (turretName != null)
                    Globals.TheTilesetManager.GetTeamColorTileData(theater.Tilesets, turretName, turretIcon, Globals.TheTeamColorManager[teamColor], out turretTile);
                if (turret2Name != null)
                    Globals.TheTilesetManager.GetTeamColorTileData(theater.Tilesets, turret2Name, turret2Icon, Globals.TheTeamColorManager[teamColor], out turret2Tile);
            }
            var tint = unit.Tint;
            void render(Graphics g)
            {
                var imageAttributes = new ImageAttributes();
                if (tint != Color.White)
                {
                    var colorMatrix = new ColorMatrix(new float[][]
                    {
                            new float[] {tint.R / 255.0f, 0, 0, 0, 0},
                            new float[] {0, tint.G / 255.0f, 0, 0, 0},
                            new float[] {0, 0, tint.B / 255.0f, 0, 0},
                            new float[] {0, 0, 0, tint.A / 255.0f, 0},
                            new float[] {0, 0, 0, 0, 1},
                    }
                    );
                    imageAttributes.SetColorMatrix(colorMatrix);
                }
                // Combine body and turret to one image, then paint it. This is done because it might be semitransparent.
                using (Bitmap unitBm = new Bitmap(renderBounds.Width, renderBounds.Height))
                {
                    unitBm.SetResolution(96, 96);
                    using (Graphics unitG = Graphics.FromImage(unitBm))
                    {
                        if (tile != null) {
                            unitG.DrawImage(tile.Image, renderRect, 0, 0, tile.Image.Width, tile.Image.Height, GraphicsUnit.Pixel);
                        }
                        if (unit.Type.HasTurret)
                        {
                            Point turretAdjust = Point.Empty;
                            Point turret2Adjust = Point.Empty;

                            if (unit.Type.TurretOffset == Int32.MaxValue)
                            {
                                // Special case: MaxValue indicates the turret oFfset is determined by the TurretAdjustOffset table.
                                turretAdjust = BackTurretAdjust[Facing32[unit.Direction.ID]];
                                if (unit.Type.HasDoubleTurret)
                                {
                                    // Never actually used for 2 turrets.
                                    turret2Adjust = BackTurretAdjust[Facing32[(byte)((unit.Direction.ID + DirectionTypes.South.ID) & 0xFF)]];
                                }
                            }
                            else if (unit.Type.TurretOffset != 0)
                            {
                                // Used by ships and by the transport helicopter.
                                int distance = unit.Type.TurretOffset;
                                int face = (unit.Direction.ID >> 5) & 7;
                                if (unit.Type.IsAircraft)
                                {
                                    // Stretch distance is given by a table.
                                    distance *= HeliDistanceAdjust[face];
                                }

                                int x = 0;
                                int y = 0;
                                // For vessels, perspective stretch is simply done as '/ 2'.
                                int perspectiveDivide = unit.Type.IsVessel ? 2 : 1;
                                MovePoint(ref x, ref y, unit.Direction.ID, distance, perspectiveDivide);
                                turretAdjust.X = x;
                                turretAdjust.Y = y;
                                if (unit.Type.HasDoubleTurret)
                                {
                                    x = 0;
                                    y = 0;
                                    MovePoint(ref x, ref y, (byte)((unit.Direction.ID + DirectionTypes.South.ID) & 0xFF), distance, perspectiveDivide);
                                    turret2Adjust.X = x;
                                    turret2Adjust.Y = y;
                                }
                            }
                            // Adjust Y-offset.
                            turretAdjust.Y += unit.Type.TurretY;
                            if (unit.Type.HasDoubleTurret)
                            {
                                turret2Adjust.Y += unit.Type.TurretY;
                            }                            
                            Point center = new Point(renderBounds.Width / 2, renderBounds.Height / 2);
                            if (turretTile != null) {
                                Size turretSize = turretTile.Image.Size;
                                var turretRenderSize = new Size(turretSize.Width * tileSize.Width / Globals.OriginalTileWidth, turretSize.Height * tileSize.Height / Globals.OriginalTileHeight);
                                var turretBounds = new Rectangle(center - new Size(turretRenderSize.Width / 2, turretRenderSize.Height / 2), turretRenderSize);
                                turretBounds.Offset(
                                    turretAdjust.X * tileSize.Width / Globals.PixelWidth,
                                    turretAdjust.Y * tileSize.Height / Globals.PixelHeight
                                );
                                unitG.DrawImage(turretTile.Image, turretBounds, 0, 0, turretTile.Image.Width, turretTile.Image.Height, GraphicsUnit.Pixel); 
                            }
                            if (unit.Type.HasDoubleTurret && turret2Tile != null)
                            {
                                Size turret2Size = turret2Tile.Image.Size;
                                var turret2RenderSize = new Size(turret2Size.Width * tileSize.Width / Globals.OriginalTileWidth, turret2Size.Height * tileSize.Height / Globals.OriginalTileHeight);
                                var turret2Bounds = new Rectangle(center - new Size(turret2RenderSize.Width / 2, turret2RenderSize.Height / 2), turret2RenderSize);
                                turret2Bounds.Offset(
                                    turret2Adjust.X * tileSize.Width / Globals.PixelWidth,
                                    turret2Adjust.Y * tileSize.Height / Globals.PixelHeight
                                );
                                unitG.DrawImage(turret2Tile.Image, turret2Bounds, 0, 0, turret2Tile.Image.Width, turret2Tile.Image.Height, GraphicsUnit.Pixel);
                            }
                        }
                    }
                    g.DrawImage(unitBm, renderBounds, 0,0, renderBounds.Width, renderBounds.Height, GraphicsUnit.Pixel, imageAttributes);
                }
            }
            return (renderBounds, render, false);
        }

        public static (Rectangle, Action<Graphics>) RenderWaypoint(GameType gameType, bool soloMission, TheaterType theater, Size tileSize, ITeamColor[] flagColors, Waypoint waypoint)
        {
            // Opacity is normally 0.5 for non-flag waypoint indicators, but is variable because the post-render
            // actions of the waypoints tool will paint a fully opaque version over the currently selected waypoint.
            //int mpId = Waypoint.GetMpIdFromFlag(waypoint.Flag);
            //float defaultOpacity = !soloMission && mpId >= 0 && mpId < flagColors.Length ? 1.0f : 0.5f;
            return Render(gameType, soloMission, theater, tileSize, flagColors, waypoint, 0.5f);
        }

        public static (Rectangle, Action<Graphics>) Render(GameType gameType, bool soloMission, TheaterType theater, Size tileSize, ITeamColor[] flagColors, Waypoint waypoint, float transparencyModifier)
        {
            if (!waypoint.Point.HasValue)
            {
                return (Rectangle.Empty, (g) => { });
            }
            Point point = waypoint.Point.Value;
            string tileGraphics = "beacon";
            ITeamColor teamColor = null;
            Color tint = waypoint.Tint;
            float brightness = 1.0f;
            int mpId = Waypoint.GetMpIdFromFlag(waypoint.Flag);
            int icon = 0;
            bool defaultIcon = true;
            if (!soloMission && mpId >= 0 && mpId < flagColors.Length)
            {
                defaultIcon = false;
                tileGraphics = "flagfly";
                // Always paint flags as opaque.
                transparencyModifier = 1.0f;
                teamColor = flagColors[mpId];
                icon = 0;
            }
            if (gameType == GameType.SoleSurvivor && (waypoint.Flag & WaypointFlag.CrateSpawn) == WaypointFlag.CrateSpawn)
            {
                defaultIcon = false;
                tileGraphics = "scrate";
                icon = 0;
                //tint = Color.FromArgb(waypoint.Tint.A, Color.Green);
                //brightness = 1.5f;
            }
            bool gotIcon = Globals.TheTilesetManager.GetTeamColorTileData(theater.Tilesets, tileGraphics, icon, teamColor, out Tile tile, true, true);
            if (!gotIcon && defaultIcon)
            {
                // Beacon only exists in remastered graphics. Get fallback.
                tileGraphics = "armor";
                icon = 6;
                gotIcon = Globals.TheTilesetManager.GetTeamColorTileData(theater.Tilesets, tileGraphics, icon, teamColor, out tile, true, true);
            }
            if (!gotIcon)
            {
                Debug.Print(string.Format("Waypoint graphics {0} ({1}) not found", tileGraphics, icon));
                return (Rectangle.Empty, (g) => { });
            }
            var location = new Point(point.X * tileSize.Width, point.Y * tileSize.Height);
            var renderSize = tileSize;
            //Rectangle renderBounds = RenderBounds(tile.Image.Size, new Size(1, 1), tileSize);
            Rectangle renderBounds = GeneralUtils.GetBoundingBoxCenter(tile.OpaqueBounds.Width, tile.OpaqueBounds.Height, tileSize.Width, tileSize.Height);
            renderBounds.X += location.X;
            renderBounds.Y += location.Y;
            void render(Graphics g)
            {
                var imageAttributes = new ImageAttributes();
                // Waypoints get drawn as semitransparent, so always execute this.
                if (tint != Color.White || brightness != 1.0 || transparencyModifier != 1.0)
                {
                    var colorMatrix = new ColorMatrix(new float[][]
                    {
                            new float[] {tint.R * brightness / 255.0f, 0, 0, 0, 0},
                            new float[] {0, tint.G * brightness / 255.0f, 0, 0, 0},
                            new float[] {0, 0, tint.B * brightness / 255.0f, 0, 0},
                            new float[] {0, 0, 0, (tint.A * transparencyModifier) / 255.0f, 0},
                            new float[] {0, 0, 0, 0, 1},
                    });
                    imageAttributes.SetColorMatrix(colorMatrix);
                }
                g.DrawImage(tile.Image, renderBounds, 0, 0, tile.OpaqueBounds.Width, tile.OpaqueBounds.Height, GraphicsUnit.Pixel, imageAttributes);
            }
            return (renderBounds, render);
        }

        public static void RenderAllBoundsFromCell<T>(Graphics graphics, Size tileSize, IEnumerable<(int, T)> renderList, CellMetrics metrics)
        {
            RenderAllBoundsFromCell(graphics, tileSize, renderList, metrics, Color.Green);
        }

        public static void RenderAllBoundsFromCell<T>(Graphics graphics, Size tileSize, IEnumerable<(int, T)> renderList, CellMetrics metrics, Color boundsColor)
        {
            RenderAllBoundsFromCell(graphics, tileSize, renderList.Select(tp => tp.Item1), metrics, boundsColor);
        }

        public static void RenderAllBoundsFromCell(Graphics graphics, Size tileSize, IEnumerable<int> renderList, CellMetrics metrics, Color boundsColor)
        {
            using (var boundsPen = new Pen(boundsColor, Math.Max(1, tileSize.Width / 16.0f)))
            {
                foreach (var cell in renderList)
                {
                    metrics.GetLocation(cell, out Point topLeft);
                    var bounds = new Rectangle(new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height), tileSize);
                    graphics.DrawRectangle(boundsPen, bounds);
                }
            }
        }

        public static void RenderAllBoundsFromPoint<T>(Graphics graphics, Size tileSize, IEnumerable<(Point, T)> renderList)
        {
            RenderAllBoundsFromPoint(graphics, tileSize, renderList.Select(tp => tp.Item1), Color.Green);
        }

        public static void RenderAllBoundsFromPoint(Graphics graphics, Size tileSize, IEnumerable<Point> renderList, Color boundsColor)
        {
            using (var boundsPen = new Pen(boundsColor, Math.Max(1, tileSize.Width / 16.0f)))
            {
                foreach (var topLeft in renderList)
                {
                    var bounds = new Rectangle(new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height), tileSize);
                    graphics.DrawRectangle(boundsPen, bounds);
                }
            }
        }

        public static void RenderAllBoundsFromPoint<T>(Graphics graphics, Size tileSize, IEnumerable<(Point, T)> renderList, Color boundsColor)
        {
            using (var boundsPen = new Pen(boundsColor, Math.Max(1, tileSize.Width / 16.0f)))
            {
                foreach (var (topLeft, _) in renderList)
                {
                    var bounds = new Rectangle(new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height), tileSize);
                    graphics.DrawRectangle(boundsPen, bounds);
                }
            }
        }

        public static void RenderAllOccupierBounds<T>(Graphics graphics, Size tileSize, IEnumerable<(Point, T)> occupiers) where T : ICellOccupier, ICellOverlapper
        {
            RenderAllOccupierBounds(graphics, tileSize, occupiers, Color.Green, Color.Red);
        }

        public static void RenderAllOccupierBounds<T>(Graphics graphics, Size tileSize, IEnumerable<(Point, T)> occupiers, Color boundsColor, Color OccupierColor) where T: ICellOccupier, ICellOverlapper
        {
            float boundsPenSize = Math.Max(1, tileSize.Width / 16.0f);
            float occupyPenSize = Math.Max(0.5f, tileSize.Width / 32.0f);
            if (occupyPenSize == boundsPenSize)
            {
                boundsPenSize += 2;
            }
            using (var boundsPen = new Pen(boundsColor, boundsPenSize))
            using (var occupyPen = new Pen(OccupierColor, occupyPenSize))
            {
                foreach (var (topLeft, occupier) in occupiers)
                {
                    Rectangle typeBounds = occupier.OverlapBounds;
                    var bounds = new Rectangle(
                        new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height),
                        new Size(typeBounds.Width * tileSize.Width, typeBounds.Height * tileSize.Height)
                    );
                    graphics.DrawRectangle(boundsPen, bounds);
                }
                foreach (var (topLeft, occupier) in occupiers)
                {
                    bool[,] occupyMask = occupier is Building bl ? bl.Type.BaseOccupyMask : occupier.OccupyMask;

                    for (var y = 0; y < occupyMask.GetLength(0); ++y)
                    {
                        for (var x = 0; x < occupyMask.GetLength(1); ++x)
                        {
                            if (occupyMask[y, x])
                            {
                                var occupyBounds = new Rectangle(
                                    new Point((topLeft.X + x) * tileSize.Width, (topLeft.Y + y) * tileSize.Height), tileSize);
                                graphics.DrawRectangle(occupyPen, occupyBounds);
                            }
                        }
                    }
                }
            }
        }

        public static void RenderAllCrateOutlines(Graphics g, Map map, Size tileSize, double tileScale, bool onlyIfBehindObjects)
        {
            // Optimised to only get the paint area once per crate type.
            // Sadly can't easily be cached because everything in this class is static.
            Dictionary<string, RegionData> paintAreas = new Dictionary<string, RegionData>();
            float outlineThickness = 0.05f;
            byte alphaThreshold = 0x40;
            //double lumThreshold = 0.01d;
            foreach (var (cell, overlay) in map.Overlay)
            {
                OverlayType ovlt = overlay.Type;
                Size cellSize = new Size(1, 1);
                if ((ovlt.Flag & OverlayTypeFlag.Crate) == OverlayTypeFlag.None
                    || !map.Metrics.GetLocation(cell, out Point location))
                {
                    continue;
                }
                if (onlyIfBehindObjects && !IsOverlapped(map, location))
                {
                    continue;
                }
                Color outlineCol = Color.FromArgb(0xA0, Color.Red);
                if ((ovlt.Flag & OverlayTypeFlag.Crate) == OverlayTypeFlag.WoodCrate) outlineCol = Color.FromArgb(0xA0, 0xFF, 0xC0, 0x40);
                if ((ovlt.Flag & OverlayTypeFlag.Crate) == OverlayTypeFlag.SteelCrate) outlineCol = Color.FromArgb(0xA0, Color.White);

                RegionData paintAreaRel;
                if (!paintAreas.TryGetValue(ovlt.Name, out paintAreaRel))
                {
                    paintAreaRel = GetOverlayOutline(map.Theater, location, tileSize, tileScale, overlay, outlineThickness, alphaThreshold, true);
                    paintAreas[ovlt.Name] = paintAreaRel;
                }
                int actualTopLeftX = location.X * tileSize.Width;
                int actualTopLeftY = location.Y * tileSize.Height;
                if (paintAreaRel != null)
                {
                    using (Region paintArea = new Region(paintAreaRel))
                    using (Brush brush = new SolidBrush(outlineCol))
                    {
                        paintArea.Translate(actualTopLeftX, actualTopLeftY);
                        g.FillRegion(brush, paintArea);
                    }
                }
            }
        }

        private static Boolean IsOverlapped(Map map,  Point location)
        {
            ICellOccupier techno = map.Technos[location];
            // Single-cell occupier. Always pass.
            if (techno is Unit || techno is InfantryGroup)
            {
                return true;
            }
            // Logic for multi-cell occupiers; buildings and terrain.
            // Return true if either an occupied cell, or overlayed by graphics deemed opaque.
            ISet<ICellOverlapper> technos = map.Overlappers.OverlappersAt(location);
            if (technos.Count == 0)
            {
                return false;
            }
            foreach (ICellOverlapper ovl in technos)
            {
                Building bld = ovl as Building;
                if (bld == null && !(ovl is Terrain))
                {
                    continue;
                }
                ICellOccupier occ = ovl as ICellOccupier;
                bool[,] opaqueMask = ovl.OpaqueMask;
                int maskY = opaqueMask.GetLength(0);
                int maskX = opaqueMask.GetLength(1);
                Point? pt = map.Technos[occ];
                if (!pt.HasValue)
                {
                    continue;
                }
                // Get list of points, find current point in the list.
                Rectangle boundsRect = new Rectangle(pt.Value, new Size(maskX, maskY));
                List<Point> pts = boundsRect.Points().OrderBy(p => p.Y).ThenBy(p => p.X).ToList();
                int index = pts.IndexOf(location);
                if (index == -1)
                {
                    continue;
                }
                // For buildings, need to specifically take the mask without bib attached.
                bool[,] occupyMask = bld != null ? bld.Type.BaseOccupyMask : occ.OccupyMask;
                // Trick to convert 2-dimensional arrays to linear format.
                bool[] occupyArr = occupyMask.Cast<bool>().ToArray();
                bool[] opaqueArr = opaqueMask.Cast<bool>().ToArray();
                // If either part of the occupied cells, or obscured from view by graphics, return true.
                if (occupyArr[index] || opaqueArr[index])
                {
                    return true;
                }
            }
            return false;
        }

        public static RegionData GetOverlayOutline(TheaterType theater, Point topLeft, Size tileSize, double tileScale, Overlay overlay, float outline, byte alphaThreshold, bool relative)
        {
            OverlayType ovtype = overlay.Type;
            string name = ovtype.GraphicsSource;
            int icon = ovtype.IsConcrete || ovtype.IsResource || ovtype.IsWall || ovtype.ForceTileNr == -1 ? overlay.Icon : ovtype.ForceTileNr;
            // For Decoration types, generate dummy if not found.
            if (!Globals.TheTilesetManager.GetTileData(theater.Tilesets, name, icon, out Tile tile, (ovtype.Flag & OverlayTypeFlag.Pavement) != 0, false))
            {
                return null;
            }
            int actualTopLeftX = relative ? 0 : topLeft.X * tileSize.Width;
            int actualTopLeftY = relative ? 0 : topLeft.Y * tileSize.Height;
            Rectangle relOverlayBounds = RenderBounds(tile.Image.Size, new Size(1, 1), tileScale);
            Rectangle overlayBounds = new Rectangle(relative ? 0 : actualTopLeftX, relative ? 0 : actualTopLeftY, tileSize.Width, tileSize.Height);
            Size maxSize = new Size(Globals.OriginalTileWidth, Globals.OriginalTileHeight);
            int actualOutlineX = (int)Math.Max(1, outline * tileSize.Width);
            int actualOutlineY = (int)Math.Max(1, outline * tileSize.Height);
            using (Bitmap bm = new Bitmap(tileSize.Width, tileSize.Height, PixelFormat.Format32bppArgb))
            {
                bm.SetResolution(96, 96);
                using (Graphics g2 = Graphics.FromImage(bm))
                {
                    g2.DrawImage(tile.Image, relOverlayBounds, 0, 0, tile.Image.Width, tile.Image.Height, GraphicsUnit.Pixel);
                }
                Byte[] imgData = ImageUtils.GetImageData(bm, out int stride, PixelFormat.Format32bppArgb, true);

                bool isOpaqueAndNotBlack(byte[] mapdata, int yVal, int xVal)
                {
                    int address = yVal * stride + xVal * 4;
                    // Check alpha
                    if (mapdata[address + 3] < alphaThreshold)
                    {
                        return false;
                    }
                    // Check brightness to exclude shadow
                    byte red = mapdata[address + 2];
                    byte grn = mapdata[address + 1];
                    byte blu = mapdata[address + 0];
                    // Integer method.
                    int redBalanced = red * red * 2126;
                    int grnBalanced = grn * grn * 7152;
                    int bluBalanced = blu * blu * 0722;
                    int lum = (redBalanced + grnBalanced + bluBalanced) / 255 / 255;
                    // The integer division will automatically reduce anything near-black
                    // to zero, so actually checking against a threshold is unnecessary.
                    return lum > 0; // lum > lumThresholdSq * 1000

                    // Floating point method
                    //double redF = red / 255.0;
                    //double grnF = grn / 255.0;
                    //double bluF = blu / 255.0;
                    //double lum = 0.2126d * redF * redF + 0.7152d * grnF * grnF + 0.0722d * bluF * bluF;
                    //return lum >= lumThresholdSq;
                };

                //Func<byte[], int, int, bool> isOpaque_ = (mapdata, yVal, xVal) => mapdata[yVal * stride + xVal * 4 + 3] >= alphaThreshold;
                List<List<Point>> blobs = BlobDetection.FindBlobs(imgData, tileSize.Width, tileSize.Height, isOpaqueAndNotBlack, true, false);
                List<Point> allblobs = new List<Point>();
                foreach (List<Point> blob in blobs)
                {
                    foreach (Point p in blob)
                    {
                        allblobs.Add(p);
                    }
                }
                HashSet<Point> drawPoints = new HashSet<Point>();
                HashSet<Point> removePoints = new HashSet<Point>();
                foreach (Point p in allblobs)
                {
                    Rectangle rect = new Rectangle(p.X + actualTopLeftX, p.Y + actualTopLeftY, 1, 1);
                    removePoints.UnionWith(rect.Points());
                    rect.Inflate(actualOutlineX, actualOutlineY);
                    rect.Intersect(overlayBounds);
                    if (!rect.IsEmpty)
                    {
                        drawPoints.UnionWith(rect.Points());
                    }
                }
                foreach (Point p in removePoints)
                {
                    drawPoints.Remove(p);
                }
                RegionData rData;
                using (Region r = new Region())
                {
                    r.MakeEmpty();
                    Size pixelSize = new Size(1, 1);
                    foreach (Point p in drawPoints)
                    {
                        r.Union(new Rectangle(p, pixelSize));
                    }
                    rData = r.GetRegionData();
                }
                return rData;
            }
        }

        public static void RenderAllFootballAreas(Graphics graphics, Map map, Size tileSize, double tileScale, GameType gameType)
        {
            // probably wouldn't work anyway; SS "road" would not be initialised.
            if (gameType != GameType.SoleSurvivor)
            {
                return;
            }
            HashSet<Point> footballPoints = new HashSet<Point>();
            foreach (Waypoint waypoint in map.Waypoints)
            {
                if (!waypoint.Point.HasValue || Waypoint.GetMpIdFromFlag(waypoint.Flag) == -1)
                {
                    continue;
                }
                Point[] roadPoints = new Rectangle(waypoint.Point.Value.X - 1, waypoint.Point.Value.Y - 1, 4, 3).Points().ToArray();
                foreach (Point p in roadPoints)
                {
                    //if (p == waypoint.Point.Value)
                    //{
                    //    continue;
                    //}
                    footballPoints.Add(p);
                }
            }
            foreach (Point p in footballPoints.OrderBy(p => p.Y * map.Metrics.Width + p.X))
            {
                Overlay footballTerrain = new Overlay()
                {
                    Type = SoleSurvivor.OverlayTypes.Road,
                    Tint = Color.FromArgb(128, Color.White)
                };
                RenderOverlay(gameType, map.Theater, null, null, p, tileSize, tileScale, footballTerrain).Item2(graphics);
            }
        }

        public static void RenderFootballAreaFlags(Graphics graphics, GameType gameType, Map map, Size tileSize)
        {
            if (gameType != GameType.SoleSurvivor)
            {
                return;
            }
            // Re-render flags on top of football areas.
            List<Waypoint> footballWayPoints = new List<Waypoint>();
            foreach (Waypoint waypoint in map.Waypoints)
            {
                if (waypoint.Point.HasValue && Waypoint.GetMpIdFromFlag(waypoint.Flag) >= 0)
                {
                    footballWayPoints.Add(waypoint);
                }
            }
            ITeamColor[] flagColors = map.FlagColors.ToArray();
            foreach (Waypoint wp in footballWayPoints)
            {
                RenderWaypoint(gameType, false, map.Theater, tileSize, flagColors, wp).Item2(graphics);
            }
        }
        

        public static void RenderAllFakeBuildingLabels(Graphics graphics, Map map, Size tileSize, double tileScale)
        {
            foreach (var (topLeft, building) in map.Buildings.OfType<Building>())
            {
                RenderFakeBuildingLabel(graphics, building, topLeft, tileSize, tileScale, false);
            }
        }

        public static void RenderFakeBuildingLabel(Graphics graphics, Building building, Point topLeft, Size tileSize, double tileScale, Boolean forPreview)
        {
            if (!building.Type.IsFake)
            {
                return;
            }
            var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            var maxSize = building.Type.Size;
            var buildingBounds = new Rectangle(
                new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height),
                new Size(maxSize.Width * tileSize.Width, maxSize.Height * tileSize.Height)
            );
            string fakeText = Globals.TheGameTextManager["TEXT_UI_FAKE"];
            using (var fakeBackgroundBrush = new SolidBrush(Color.FromArgb((forPreview ? 128 : 256) * 2 / 3, Color.Black)))
            using (var fakeTextBrush = new SolidBrush(Color.FromArgb(forPreview ? building.Tint.A : 255, Color.White)))
            {
                using (var font = graphics.GetAdjustedFont(fakeText, SystemFonts.DefaultFont, buildingBounds.Width, buildingBounds.Height,
                    Math.Max(1, (int)(12 * tileScale)), Math.Max(1, (int)(24 * tileScale)), stringFormat, true))
                {
                    var textBounds = graphics.MeasureString(fakeText, font, buildingBounds.Width, stringFormat);
                    var backgroundBounds = new RectangleF(buildingBounds.Location, textBounds);
                    graphics.FillRectangle(fakeBackgroundBrush, backgroundBounds);
                    graphics.DrawString(fakeText, font, fakeTextBrush, backgroundBounds, stringFormat);
                }
            }
        }

        public static void RenderAllRebuildPriorityLabels(Graphics graphics, Map map, Size tileSize, double tileScale)
        {
            foreach (var (topLeft, building) in map.Buildings.OfType<Building>())
            {
                RenderRebuildPriorityLabel(graphics, building, topLeft, tileSize, tileScale, false);
            }
        }

        public static void RenderRebuildPriorityLabel(Graphics graphics, Building building, Point topLeft, Size tileSize, double tileScale, Boolean forPreview)
        {
            var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            var maxSize = building.Type.Size;
            var buildingBounds = new Rectangle(
                new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height),
                new Size(maxSize.Width * tileSize.Width, maxSize.Height * tileSize.Height)
            );
            if (building.BasePriority >= 0)
            {
                string priText = building.BasePriority.ToString();
                using (var baseBackgroundBrush = new SolidBrush(Color.FromArgb((forPreview ? 128 : 256) * 2 / 3, Color.Black)))
                using (var baseTextBrush = new SolidBrush(Color.FromArgb(forPreview ? 128 : 255, Color.Red)))
                {
                    using (var font = graphics.GetAdjustedFont(priText, SystemFonts.DefaultFont, buildingBounds.Width, buildingBounds.Height,
                        Math.Max(1, (int)(12 * tileScale)), Math.Max(1, (int)(24 * tileScale)), stringFormat, true))
                    {
                        var textBounds = graphics.MeasureString(priText, font, buildingBounds.Width, stringFormat);
                        var backgroundBounds = new RectangleF(buildingBounds.Location, textBounds);
                        backgroundBounds.Offset((buildingBounds.Width - textBounds.Width) / 2.0f, buildingBounds.Height - textBounds.Height);
                        graphics.FillRectangle(baseBackgroundBrush, backgroundBounds);
                        graphics.DrawString(priText, font, baseTextBrush, backgroundBounds, stringFormat);
                    }
                }
            }
        }

        public static void RenderAllTechnoTriggers(Graphics graphics, Map map, Size tileSize, double tileScale, MapLayerFlag layersToRender)
        {
            RenderAllTechnoTriggers(graphics, map, tileSize, tileScale, layersToRender, Color.LimeGreen, null, false);
        }

        public static void RenderAllTechnoTriggers(Graphics graphics, Map map, Size tileSize, double tileScale, MapLayerFlag layersToRender, Color color, string toPick, bool excludePick)
        {
            float borderSize = Math.Max(0.5f, tileSize.Width / 60.0f);
                foreach (var (cell, techno) in map.Technos)
                {
                    var location = new Point(cell.X * tileSize.Width, cell.Y * tileSize.Height);
                    (string trigger, Rectangle bounds, int alpha)[] triggers = null;
                    if (techno is Terrain terrain && !Trigger.IsEmpty(terrain.Trigger))
                    {
                        if ((layersToRender & MapLayerFlag.Terrain) == MapLayerFlag.Terrain)
                        {
                            triggers = new (string, Rectangle, int)[] { (terrain.Trigger, new Rectangle(location, terrain.Type.GetRenderSize(tileSize)), terrain.Tint.A) };
                        }
                    }
                    else if (techno is Building building && !Trigger.IsEmpty(building.Trigger))
                    {
                        if ((layersToRender & MapLayerFlag.Buildings) == MapLayerFlag.Buildings)
                        {
                            var size = new Size(building.Type.Size.Width * tileSize.Width, building.Type.Size.Height * tileSize.Height);
                            triggers = new (string, Rectangle, int)[] { (building.Trigger, new Rectangle(location, size), building.Tint.A) };
                        }
                    }
                    else if (techno is Unit unit && !Trigger.IsEmpty(unit.Trigger))
                    {
                        if ((layersToRender & MapLayerFlag.Units) == MapLayerFlag.Units)
                        {
                            triggers = new (string, Rectangle, int)[] { (unit.Trigger, new Rectangle(location, tileSize), unit.Tint.A) };
                        }
                    }
                    else if (techno is InfantryGroup infantryGroup)
                    {
                        if ((layersToRender & MapLayerFlag.Infantry) == MapLayerFlag.Infantry)
                        {
                            List<(string, Rectangle, int)> infantryTriggers = new List<(string, Rectangle, int)>();
                            for (var i = 0; i < infantryGroup.Infantry.Length; ++i)
                            {
                                var infantry = infantryGroup.Infantry[i];
                                if (infantry == null ||  Trigger.IsEmpty(infantry.Trigger))
                                {
                                    continue;
                                }
                                var size = tileSize;
                                var offset = Size.Empty;
                                switch ((InfantryStoppingType)i)
                                {
                                    case InfantryStoppingType.UpperLeft:
                                        offset.Width = -size.Width / 4;
                                        offset.Height = -size.Height / 4;
                                        break;
                                    case InfantryStoppingType.UpperRight:
                                        offset.Width = size.Width / 4;
                                        offset.Height = -size.Height / 4;
                                        break;
                                    case InfantryStoppingType.LowerLeft:
                                        offset.Width = -size.Width / 4;
                                        offset.Height = size.Height / 4;
                                        break;
                                    case InfantryStoppingType.LowerRight:
                                        offset.Width = size.Width / 4;
                                        offset.Height = size.Height / 4;
                                        break;
                                }
                                var bounds = new Rectangle(location + offset, size);
                                infantryTriggers.Add((infantry.Trigger, bounds, infantry.Tint.A));
                            }
                            triggers = infantryTriggers.ToArray();
                        }
                    }
                if (triggers != null)
                {
                    StringFormat stringFormat = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    foreach (var (trigger, bounds, alpha) in triggers.Where(x => toPick == null
                    || (excludePick && !x.trigger.Equals(toPick, StringComparison.OrdinalIgnoreCase))
                     || (!excludePick && x.trigger.Equals(toPick, StringComparison.OrdinalIgnoreCase))))
                    {
                        Color alphaColor = Color.FromArgb(alpha, color);
                        using (var technoTriggerBackgroundBrush = new SolidBrush(Color.FromArgb(96 * alpha / 256, Color.Black)))
                        using (var technoTriggerBrush = new SolidBrush(alphaColor))
                        using (var technoTriggerPen = new Pen(alphaColor, borderSize))
                        using (var font = graphics.GetAdjustedFont(trigger, SystemFonts.DefaultFont, bounds.Width, bounds.Height,
                            Math.Max(1, (int)(12 * tileScale)), Math.Max(1, (int)(24 * tileScale)), stringFormat, true))
                        {
                            var textBounds = graphics.MeasureString(trigger, font, bounds.Width, stringFormat);
                            var backgroundBounds = new RectangleF(bounds.Location, textBounds);
                            backgroundBounds.Offset((bounds.Width - textBounds.Width) / 2.0f, (bounds.Height - textBounds.Height) / 2.0f);
                            graphics.FillRectangle(technoTriggerBackgroundBrush, backgroundBounds);
                            graphics.DrawRectangle(technoTriggerPen, Rectangle.Round(backgroundBounds));
                            graphics.DrawString(trigger, font, technoTriggerBrush, bounds, stringFormat);
                        }
                    }
                }
            }
        }

        public static void RenderWayPointIndicators(Graphics graphics, Map map, Size tileSize, double tileScale, Color textColor, bool forPreview, bool excludeSpecified, params Waypoint[] specified)
        {
            HashSet<Waypoint> specifiedWaypoints = specified.ToHashSet();

            Waypoint[] toPaint = excludeSpecified ? map.Waypoints : specified;
            foreach (var waypoint in toPaint)
            {
                if (waypoint.Cell.HasValue && map.Metrics.GetLocation(waypoint.Cell.Value, out Point point))
                {
                    if (excludeSpecified && specifiedWaypoints.Contains(waypoint))
                    {
                        continue;
                    }
                    RenderWayPointIndicator(graphics, waypoint, point, tileSize, tileScale, textColor, forPreview);
                }
            }
        }

        public static void RenderWayPointIndicator(Graphics graphics, Waypoint waypoint, Point topLeft, Size tileSize, double tileScale, Color textColor, bool forPreview)
        {
            var stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            var paintBounds = new Rectangle(new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height), tileSize);
            string wpText = waypoint.Name;
            using (var baseBackgroundBrush = new SolidBrush(Color.FromArgb(forPreview ? 64 : 128, Color.Black)))
            using (var baseTextBrush = new SolidBrush(Color.FromArgb(forPreview ? 128 : 255, textColor)))
            {
                using (var font = graphics.GetAdjustedFont(wpText, SystemFonts.DefaultFont, paintBounds.Width, paintBounds.Height,
                    Math.Max(1, (int)(12 * tileScale)), Math.Max(1, (int)(55 * tileScale)), stringFormat, true))
                {
                    var textBounds = graphics.MeasureString(wpText, font, paintBounds.Width, stringFormat);
                    var backgroundBounds = new RectangleF(paintBounds.Location, textBounds);
                    backgroundBounds.Offset((paintBounds.Width - textBounds.Width) / 2.0f, (paintBounds.Height - textBounds.Height) / 2.0f);
                    graphics.FillRectangle(baseBackgroundBrush, backgroundBounds);
                    graphics.DrawString(wpText, font, baseTextBrush, backgroundBounds, stringFormat);
                }
            }
        }

        public static void RenderAllBuildingEffectRadiuses(Graphics graphics, Map map, Size tileSize, int effectRadius)
        {
            foreach (var (topLeft, building) in map.Buildings.OfType<Building>()
                .Where(b => (b.Occupier.Type.Flag & BuildingTypeFlag.IsGapGenerator) != BuildingTypeFlag.None))
            {
                RenderBuildingEffectRadius(graphics, tileSize, effectRadius, building, topLeft);
            }
        }

        public static void RenderBuildingEffectRadius(Graphics graphics, Size tileSize, int effectRadius, Building building, Point topLeft)
        {
            if ((building.Type.Flag & BuildingTypeFlag.IsGapGenerator) != BuildingTypeFlag.IsGapGenerator)
            {
                return;
            }
            ITeamColor tc = building.Type.CanRemap ? Globals.TheTeamColorManager[building.House.BuildingTeamColor] : null;
            Color circleColor = tc?.BaseColor ?? Globals.TheTeamColorManager.RemapBaseColor;
            bool[,] cells = building.Type.BaseOccupyMask;
            int maskY = cells.GetLength(0);
            int maskX = cells.GetLength(1);
            Rectangle circleBounds = GeneralUtils.GetBoxFromCenterCell(topLeft, maskX, maskY, effectRadius, effectRadius, tileSize, out Point center);
            Color alphacorr = Color.FromArgb(building.Tint.A * 128 / 256, circleColor);
            RenderCircleDiagonals(graphics, tileSize, alphacorr, effectRadius, effectRadius, center);
            DrawDashesCircle(graphics, circleBounds, tileSize, alphacorr, true, -1.25f, 2.5f);
        }

        public static void RenderAllUnitEffectRadiuses(Graphics graphics, Map map, Size tileSize, int jamRadius)
        {
            foreach (var (topLeft, unit) in map.Technos.OfType<Unit>()
                .Where(b => (b.Occupier.Type.Flag & (UnitTypeFlag.IsGapGenerator | UnitTypeFlag.IsJammer)) != UnitTypeFlag.None))
            {
                RenderUnitEffectRadius(graphics, tileSize, jamRadius, unit, topLeft);
            }
        }

        public static void RenderUnitEffectRadius(Graphics graphics, Size tileSize, int jamRadius, Unit unit, Point cell)
        {
            bool isJammer = (unit.Type.Flag & UnitTypeFlag.IsJammer) == UnitTypeFlag.IsJammer;
            bool isGapGen = (unit.Type.Flag & UnitTypeFlag.IsGapGenerator) == UnitTypeFlag.IsGapGenerator;
            if (!isJammer && !isGapGen)
            {
                return;
            }
            String teamColor;
            if (!unit.House.OverrideTeamColors.TryGetValue(unit.Type.Name, out teamColor))
            {
                teamColor = unit.House.UnitTeamColor;
            }
            ITeamColor tc = Globals.TheTeamColorManager[teamColor];
            Color circleColor = tc?.BaseColor ?? Globals.TheTeamColorManager.RemapBaseColor;
            Color alphacorr = Color.FromArgb(unit.Tint.A * 128 / 256, circleColor);
            if (isJammer)
            {
                // uses map's Gap Generator range.
                Rectangle circleBounds = GeneralUtils.GetBoxFromCenterCell(cell, 1, 1, jamRadius, jamRadius, tileSize, out Point center);
                RenderCircleDiagonals(graphics, tileSize, alphacorr, jamRadius, jamRadius, center);
                DrawDashesCircle(graphics, circleBounds, tileSize, alphacorr, true, -1.25f, 2.5f);
            }
            if (isGapGen)
            {
                // uses specific 5x7 circle around the unit cell
                int radiusX = 2;
                int radiusY = 3;
                Rectangle circleBounds = GeneralUtils.GetBoxFromCenterCell(cell, 1, 1, radiusX, radiusY, tileSize, out Point center);
                RenderCircleDiagonals(graphics, tileSize, alphacorr, radiusX, radiusY, center);
                DrawDashesCircle(graphics, circleBounds, tileSize, alphacorr, true, -1.25f, 2.5f);
            }
        }

        private static void RenderCircleDiagonals(Graphics graphics, Size tileSize, Color paintColor, double radiusX, double radiusY, Point center)
        {

            float penSize = Math.Max(1.0f, tileSize.Width / 16.0f);
            using (Pen linePen = new Pen(paintColor, penSize))
            {
                linePen.DashPattern = new float[] { 1.0F, 4.0F, 6.0F, 4.0F };
                linePen.DashCap = DashCap.Round;
                int diamX = (int)((radiusX * 2 + 1) * tileSize.Width);
                int radX = diamX / 2;
                int diamY = (int)((radiusY * 2 + 1) * tileSize.Height);
                int radY = diamY / 2;
                double sinDistance = Math.Sin(Math.PI * 45 / 180.0);
                int sinX = (int)(radX * sinDistance);
                int sinY = (int)(radY * sinDistance);
                graphics.DrawLine(linePen, center, new Point(center.X, center.Y - radY));
                graphics.DrawLine(linePen, center, new Point(center.X - sinX, center.Y + sinY));
                graphics.DrawLine(linePen, center, new Point(center.X + radX, center.Y));
                graphics.DrawLine(linePen, center, new Point(center.X + sinX, center.Y + sinX));
                graphics.DrawLine(linePen, center, new Point(center.X, center.Y + radY));
                graphics.DrawLine(linePen, center, new Point(center.X + sinX, center.Y - sinY));
                graphics.DrawLine(linePen, center, new Point(center.X - radX, center.Y));
                graphics.DrawLine(linePen, center, new Point(center.X - sinX, center.Y - sinY));
            }
        }

        public static void RenderAllWayPointRevealRadiuses(Graphics graphics, IGamePlugin plugin, Map map, Size tileSize, Waypoint selectedItem)
        {
            RenderAllWayPointRevealRadiuses(graphics, plugin, map, tileSize, selectedItem, false);
        }

        public static void RenderAllWayPointRevealRadiuses(Graphics graphics, IGamePlugin plugin, Map map, Size tileSize, Waypoint selectedItem, bool onlySelected)
        {
            int[] wpReveal1 = plugin.GetRevealRadiusForWaypoints(map, false);
            int[] wpReveal2 = plugin.GetRevealRadiusForWaypoints(map, true);
            Waypoint[] allWaypoints = map.Waypoints;
            for (int i = 0; i < allWaypoints.Length; i++)
            {
                Waypoint cur = allWaypoints[i];
                bool isSelected = selectedItem != null && selectedItem == cur;
                if (onlySelected && !isSelected)
                {
                    continue;
                }
                Point? p = cur?.Point;
                if (p.HasValue)
                {
                    Color drawColor = isSelected ? Color.Yellow : Color.Orange;
                    if (wpReveal1[i] != 0)
                    {
                        RenderWayPointRevealRadius(graphics, map.Metrics, tileSize, drawColor, isSelected, false, wpReveal1[i], cur);
                    }
                    if (wpReveal2[i] != 0)
                    {
                        RenderWayPointRevealRadius(graphics, map.Metrics, tileSize, drawColor, isSelected, false, wpReveal2[i], cur);
                    }
                }
            }
        }

        public static void RenderWayPointRevealRadius(Graphics graphics, CellMetrics metrics, Size tileSize, Color circleColor, bool thickborder, bool forPreview, double revealRadius, Waypoint waypoint)
        {
            if (waypoint.Cell.HasValue && metrics.GetLocation(waypoint.Cell.Value, out Point cellPoint))
            {
                double diam = revealRadius * 2 + 1;
                Rectangle circleBounds = new Rectangle(
                    (int)(cellPoint.X * tileSize.Width - revealRadius * tileSize.Width),
                    (int)(cellPoint.Y * tileSize.Width - revealRadius * tileSize.Height),
                    (int)(diam * tileSize.Width),
                    (int)(diam * tileSize.Height));
                DrawDashesCircle(graphics, circleBounds, tileSize, Color.FromArgb(forPreview ? 64 : 128, circleColor), thickborder, 1.25f, 2.5f);
            }
        }

        public static void DrawCircle(Graphics graphics, Rectangle circleBounds, Size tileSize, Color circleColor, bool thickborder)
        {
            float penSize = Math.Max(1f, tileSize.Width / (thickborder ? 16.0f : 32.0f));
            using (Pen circlePen = new Pen(circleColor, penSize))
            {
                graphics.DrawEllipse(circlePen, circleBounds);
            }
        }

        public static void DrawDashesCircle(Graphics graphics, Rectangle circleBounds, Size tileSize, Color circleColor, bool thickborder, float startAngle, float drawAngle)
        {
            float penSize = Math.Max(1f, tileSize.Width / (thickborder ? 16.0f : 32.0f));
            using (Pen circlePen = new Pen(circleColor, penSize))
            {
                drawAngle = Math.Abs(drawAngle);
                float endPoint = 360f + startAngle - drawAngle;
                for (float i = startAngle; i <= endPoint; i += drawAngle * 2)
                {
                    graphics.DrawArc(circlePen, circleBounds, i, drawAngle);
                }
            }
        }

        public static void RenderCellTriggersSoft(Graphics graphics, Map map, Size tileSize, double tileScale, params String[] specifiedToExclude)
        {
            RenderCellTriggers(graphics, map, tileSize, tileScale, Color.Black, Color.White, Color.White, 0.75f, false, true, specifiedToExclude);
        }

        public static void RenderCellTriggersHard(Graphics graphics, Map map, Size tileSize, double tileScale, params String[] specifiedToExclude)
        {
            RenderCellTriggers(graphics, map, tileSize, tileScale, Color.Black, Color.White, Color.White, 1, false, true, specifiedToExclude);
        }

        public static void RenderCellTriggersSelected(Graphics graphics, Map map, Size tileSize, double tileScale, params String[] specifiedToDraw)
        {
            RenderCellTriggers(graphics, map, tileSize, tileScale, Color.Black, Color.Yellow, Color.Yellow, 1, true, false, specifiedToDraw);
        }

        public static void RenderCellTriggers(Graphics graphics, Map map, Size tileSize, double tileScale, Color fillColor, Color borderColor, Color textColor, double alphaAdjust, bool thickborder, bool excludeSpecified, params String[] specified)
        {
            Color ApplyAlpha(Color col, int baseAlpha, double alphaMul)
            {
                return Color.FromArgb(Math.Max(0, Math.Min(0xFF, (int)Math.Round(baseAlpha * alphaMul, MidpointRounding.AwayFromZero))), col);
            };
            // Actual balance is fixed; border is 1, text is 1/2, background is 3/8. The original alpha inside the given colours is ignored.
            fillColor = ApplyAlpha(fillColor, 0x60, alphaAdjust);
            borderColor = ApplyAlpha(borderColor, 0x100, alphaAdjust);
            textColor = ApplyAlpha(textColor, 0x80, alphaAdjust);
            Color previewFillColor = ApplyAlpha(fillColor, 0x60, alphaAdjust / 2);
            Color previewBorderColor = ApplyAlpha(borderColor, 0x100, alphaAdjust / 2);
            Color previewTextColor = ApplyAlpha(textColor, 0x80, alphaAdjust / 2);

            float borderSize = Math.Max(0.5f, tileSize.Width / 60.0f);
            float thickBorderSize = Math.Max(1f, tileSize.Width / 20.0f);
            HashSet<String> specifiedSet = new HashSet<String>(specified, StringComparer.OrdinalIgnoreCase);

            using (SolidBrush prevCellTriggersBackgroundBrush = new SolidBrush(previewFillColor))
            using (Pen prevCellTriggerPen = new Pen(previewBorderColor, thickborder ? thickBorderSize : borderSize))
            using (SolidBrush prevCellTriggersBrush = new SolidBrush(previewTextColor))
            using (SolidBrush cellTriggersBackgroundBrush = new SolidBrush(fillColor))
            using (Pen cellTriggerPen = new Pen(borderColor, thickborder ? thickBorderSize : borderSize))
            using (SolidBrush cellTriggersBrush = new SolidBrush(textColor))
            {
                foreach (var (cell, cellTrigger) in map.CellTriggers)
                {
                    bool contains = specifiedSet.Contains(cellTrigger.Trigger);
                    if (contains && excludeSpecified || !contains && !excludeSpecified)
                    {
                        continue;
                    }
                    var x = cell % map.Metrics.Width;
                    var y = cell / map.Metrics.Width;
                    var location = new Point(x * tileSize.Width, y * tileSize.Height);
                    var textBounds = new Rectangle(location, tileSize);
                    bool isPreview = cellTrigger.Tint.A != 255;
                    graphics.FillRectangle(isPreview ? prevCellTriggersBackgroundBrush : cellTriggersBackgroundBrush, textBounds);
                    graphics.DrawRectangle(isPreview ? prevCellTriggerPen : cellTriggerPen, textBounds);
                    StringFormat stringFormat = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    var text = cellTrigger.Trigger;
                    using (var font = graphics.GetAdjustedFont(text, SystemFonts.DefaultFont, textBounds.Width, textBounds.Height,
                        Math.Max(1, (int)(24 * tileScale)), Math.Max(1, (int)(48 * tileScale)), stringFormat, true))
                    {
                        graphics.DrawString(text.ToString(), font, isPreview ? prevCellTriggersBrush : cellTriggersBrush, textBounds, stringFormat);
                    }
                }
            }
        }

        public static void RenderMapBoundaries(Graphics graphics, Map map, Size tileSize)
        {
            RenderMapBoundaries(graphics, map.Bounds, tileSize, Color.Cyan, false);
        }

        public static void RenderMapBoundaries(Graphics graphics, Rectangle bounds, Size tileSize, Color color, bool symmetryLines)
        {
            var boundsRect = Rectangle.FromLTRB(
                bounds.Left * tileSize.Width,
                bounds.Top * tileSize.Height,
                bounds.Right * tileSize.Width,
                bounds.Bottom * tileSize.Height
            );
            using (var boundsPen = new Pen(color, Math.Max(1f, tileSize.Width / 8.0f)))
            {
                if (!symmetryLines)
                {
                    graphics.DrawRectangle(boundsPen, boundsRect);
                }
                else
                {
                    graphics.DrawLine(boundsPen, new Point(boundsRect.X, boundsRect.Y), new Point(boundsRect.Right, boundsRect.Bottom));
                    graphics.DrawLine(boundsPen, new Point(boundsRect.Right, boundsRect.Y), new Point(boundsRect.X, boundsRect.Bottom));

                    int halfX = boundsRect.X + boundsRect.Width / 2;
                    int halfY = boundsRect.Y + boundsRect.Height / 2;
                    graphics.DrawLine(boundsPen, new Point(halfX, boundsRect.Y), new Point(halfX, boundsRect.Bottom));
                    graphics.DrawLine(boundsPen, new Point(boundsRect.X, halfY), new Point(boundsRect.Right, halfY));
                }
            }
        }

        public static void RenderMapGrid(Graphics graphics, Rectangle bounds, Size tileSize, Color color)
        {
            using (var gridPen = new Pen(color, Math.Max(1f, tileSize.Width / 16.0f)))
            {
                int leftBound = bounds.Left * tileSize.Width;
                int rightBound = bounds.Right * tileSize.Width;
                for (int y = bounds.Top + 1; y < bounds.Bottom; ++y)
                {
                    int ymul = y * tileSize.Height;
                    graphics.DrawLine(gridPen, new Point(leftBound, ymul), new Point(rightBound, ymul));
                }
                int topBound = bounds.Top * tileSize.Height;
                int bottomBound = bounds.Bottom * tileSize.Height;
                for (int x = bounds.Left + 1; x < bounds.Right; ++x)
                {
                    int xmul = x * tileSize.Height;
                    graphics.DrawLine(gridPen, new Point(xmul, topBound), new Point(xmul, bottomBound));
                }
            }
        }

        public static void SetRenderSettings(Graphics g, Boolean smooth)
        {
            if (smooth)
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            }
            else
            {
                g.CompositingQuality = CompositingQuality.AssumeLinear;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.Half;
            }
        }

        public static Rectangle RenderBounds(Size imageSize, Size cellDimensions, Size cellSize)
        {
            double scaleFactorX = cellSize.Width / (double)Globals.OriginalTileWidth;
            double scaleFactorY = cellSize.Height / (double)Globals.OriginalTileHeight;
            return RenderBounds(imageSize, cellDimensions, scaleFactorX, scaleFactorY);
        }

        public static Rectangle RenderBounds(Size imageSize, Size cellDimensions, double scaleFactor)
        {
            return RenderBounds(imageSize, cellDimensions, scaleFactor, scaleFactor);
        }

        public static Rectangle RenderBounds(Size imageSize, Size cellDimensions, double scaleFactorX, double scaleFactorY)
        {
            Size maxSize = new Size(cellDimensions.Width * Globals.OriginalTileWidth, cellDimensions.Height * Globals.OriginalTileHeight);
            // If graphics are too large, scale them down using the largest dimension
            Size newSize = new Size(imageSize.Width, imageSize.Height);
            if ((imageSize.Width >= imageSize.Height) && (imageSize.Width > maxSize.Width))
            {
                newSize.Height = imageSize.Height * maxSize.Width / imageSize.Width;
                newSize.Width = maxSize.Width;
            }
            else if ((imageSize.Height >= imageSize.Width) && (imageSize.Height > maxSize.Height))
            {
                newSize.Width = imageSize.Width * maxSize.Height / imageSize.Height;
                newSize.Height = maxSize.Height;
            }
            // center graphics inside bounding box
            int locX = (maxSize.Width - newSize.Width) / 2;
            int locY = (maxSize.Height - newSize.Height) / 2;
            return new Rectangle((int)(locX * scaleFactorX), (int)(locY * scaleFactorY),
                Math.Max(1, (int)(newSize.Width * scaleFactorX)), Math.Max(1, (int)(newSize.Height * scaleFactorY)));
        }
    }
}
