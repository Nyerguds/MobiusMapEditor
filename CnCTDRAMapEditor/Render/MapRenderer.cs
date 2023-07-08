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
using System.Drawing.Text;
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

        public static void Render(GameType gameType, Map map, Graphics graphics, ISet<Point> locations, MapLayerFlag layers, double tileScale)
        {
            // tileScale should always be given so it results in an exact integer tile size. Math.Round was added to account for .999 situations in the floats.
            Size tileSize = new Size(Math.Max(1, (int)Math.Round(Globals.OriginalTileWidth * tileScale)), Math.Max(1, (int)Math.Round(Globals.OriginalTileHeight * tileScale)));
            //Size tileSize = new Size(Math.Max(1, (int)(Globals.OriginalTileWidth * tileScale)), Math.Max(1, (int)(Globals.OriginalTileHeight * tileScale)));
            TheaterType theater = map.Theater;
            // paint position, paint action, true if flat.
            List<(Rectangle, Action<Graphics>, Boolean)> overlappingRenderList = new List<(Rectangle, Action<Graphics>, bool)>();
            Func<IEnumerable<Point>> renderLocations = null;
            if (locations != null)
            {
                renderLocations = () => locations.OrderBy(p => p.Y * map.Metrics.Width + p.X);
            }
            else
            {
                IEnumerable<Point> allCells()
                {
                    for (Int32 y = 0; y < map.Metrics.Height; ++y)
                    {
                        for (Int32 x = 0; x < map.Metrics.Width; ++x)
                        {
                            yield return new Point(x, y);
                        }
                    }
                }
                renderLocations = allCells;
            }
            var backupCompositingQuality = graphics.CompositingQuality;
            var backupInterpolationMode = graphics.InterpolationMode;
            var backupSmoothingMode = graphics.SmoothingMode;
            var backupPixelOffsetMode = graphics.PixelOffsetMode;

            // Check if double tile painting is useful.
            SetRenderSettings(graphics, false);
            bool isHard = backupCompositingQuality == graphics.CompositingQuality &&
                          backupInterpolationMode == graphics.InterpolationMode &&
                          backupSmoothingMode == graphics.SmoothingMode &&
                          backupPixelOffsetMode == graphics.PixelOffsetMode;
            graphics.CompositingQuality = backupCompositingQuality;
            graphics.InterpolationMode = backupInterpolationMode;
            graphics.SmoothingMode = backupSmoothingMode;
            graphics.PixelOffsetMode = backupPixelOffsetMode;
            if ((layers & MapLayerFlag.Template) != MapLayerFlag.None)
            {
                TemplateType clear = map.TemplateTypes.Where(t => t.Flag == TemplateTypeFlag.Clear).FirstOrDefault();
                foreach (Point topLeft in renderLocations())
                {
                    Template template = map.Templates[topLeft];
                    TemplateType ttype = template?.Type ?? clear;
                    String name = ttype.Name;
                    Int32 icon = template?.Icon ?? ((topLeft.X & 0x03) | ((topLeft.Y) & 0x03) << 2);
                    // This should never happen; group tiles should never be placed down on the map.
                    if ((ttype.Flag & TemplateTypeFlag.Group) == TemplateTypeFlag.Group)
                    {
                        name = ttype.GroupTiles[icon];
                        icon = 0;
                    }
                    // If something is actually placed on the map, show it, even if it has no graphics.
                    bool success = Globals.TheTilesetManager.GetTileData(name, icon, out Tile tile, true, false);
                    if (tile != null)
                    {
                        Rectangle renderBounds = new Rectangle(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height, tileSize.Width, tileSize.Height);
                        if (tile.Image != null)
                        {
                            using (Bitmap tileImg = tile.Image.RemoveAlpha())
                            {
                                // Double tile painting drastically reduces edge artifacts when using smooth scaling
                                if (!isHard)
                                {
                                    // Hard mode
                                    SetRenderSettings(graphics, false);
                                    graphics.DrawImage(tileImg, renderBounds);
                                    // Original smooth mode
                                    graphics.CompositingQuality = backupCompositingQuality;
                                    graphics.InterpolationMode = backupInterpolationMode;
                                    graphics.SmoothingMode = backupSmoothingMode;
                                    graphics.PixelOffsetMode = backupPixelOffsetMode;
                                }
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
                foreach (Point topLeft in renderLocations())
                {
                    Smudge smudge = map.Smudge[topLeft];
                    // Don't render bibs in theaters which don't contain them.
                    if (smudge != null && smudge.Type.IsAutoBib && (smudge.Type.Theaters == null || smudge.Type.Theaters.Contains(theater)))
                    {
                        RenderSmudge(theater, topLeft, tileSize, tileScale, smudge).Item2(graphics);
                    }
                }
            }
            if ((layers & MapLayerFlag.Smudge) != MapLayerFlag.None)
            {
                foreach (Point topLeft in renderLocations())
                {
                    Smudge smudge = map.Smudge[topLeft];
                    if (smudge != null && !smudge.Type.IsAutoBib)
                    {
                        RenderSmudge(theater, topLeft, tileSize, tileScale, smudge).Item2(graphics);
                    }
                }
            }

            if ((layers & MapLayerFlag.OverlayAll) != MapLayerFlag.None)
            {
                foreach (Point location in renderLocations())
                {
                    Overlay overlay = map.Overlay[location];
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
                        RenderOverlay(gameType, location, tileSize, tileScale, overlay).Item2(graphics);
                    }
                }
            }
            if ((layers & MapLayerFlag.Buildings) != MapLayerFlag.None)
            {
                foreach ((Point topLeft, Building building) in map.Buildings.OfType<Building>())
                {
                    if ((locations != null) && !locations.Contains(topLeft))
                    {
                        continue;
                    }
                    overlappingRenderList.Add(RenderBuilding(gameType, topLeft, tileSize, tileScale, building));
                }
            }
            if ((layers & MapLayerFlag.Infantry) != MapLayerFlag.None)
            {
                foreach ((Point topLeft, InfantryGroup infantryGroup) in map.Technos.OfType<InfantryGroup>())
                {
                    if ((locations != null) && !locations.Contains(topLeft))
                    {
                        continue;
                    }
                    for (int i = 0; i < infantryGroup.Infantry.Length; ++i)
                    {
                        Infantry infantry = infantryGroup.Infantry[i];
                        if (infantry == null)
                        {
                            continue;
                        }
                        overlappingRenderList.Add(RenderInfantry(topLeft, tileSize, infantry, (InfantryStoppingType)i));
                    }
                }
            }
            if ((layers & MapLayerFlag.Units) != MapLayerFlag.None)
            {
                foreach ((Point topLeft, Unit unit) in map.Technos.OfType<Unit>())
                {
                    if ((locations != null) && !locations.Contains(topLeft))
                    {
                        continue;
                    }
                    overlappingRenderList.Add(RenderUnit(gameType, topLeft, tileSize, unit));
                }
            }
            // Paint flat items (like the repair bay)
            //.OrderBy(x => x.Item1.Bottom) --> .OrderBy(x => x.Item1.CenterPoint().Y)
            foreach ((Rectangle location, Action<Graphics> renderer, Boolean flat) in overlappingRenderList.Where(x => !x.Item1.IsEmpty && x.Item3).OrderBy(x => x.Item1.CenterPoint().Y))
            {
                renderer(graphics);
            }
            // Paint all the rest
            foreach ((Rectangle location, Action<Graphics> renderer, Boolean flat) in overlappingRenderList.Where(x => !x.Item1.IsEmpty && !x.Item3).OrderBy(x => x.Item1.CenterPoint().Y))
            {
                renderer(graphics);
            }
            overlappingRenderList.Clear();
            if ((layers & MapLayerFlag.Terrain) != MapLayerFlag.None)
            {
                foreach ((Point topLeft, Terrain terrain) in map.Technos.OfType<Terrain>())
                {
                    if ((locations != null) && !locations.Contains(topLeft))
                    {
                        continue;
                    }
                    overlappingRenderList.Add(RenderTerrain(gameType, theater, topLeft, tileSize, tileScale, terrain));
                }
                foreach ((Rectangle location, Action<Graphics> renderer, Boolean flat) in overlappingRenderList.Where(x => !x.Item1.IsEmpty && !x.Item3).OrderBy(x => x.Item1.CenterPoint().Y))
                {
                    renderer(graphics);
                }
            }
            if (Globals.CratesOnTop && (layers & MapLayerFlag.Overlay) != MapLayerFlag.None)
            {
                foreach (Point topLeft in renderLocations())
                {
                    Overlay overlay = map.Overlay[topLeft];
                    if (overlay == null || !overlay.Type.IsCrate)
                    {
                        continue;
                    }
                    RenderOverlay(gameType, topLeft, tileSize, tileScale, overlay).Item2(graphics);
                }
            }

            if ((layers & MapLayerFlag.Waypoints) != MapLayerFlag.None)
            {
                // todo avoid overlapping waypoints of the same type?
                HashSet<int> handledPoints = new HashSet<int>();
                ITeamColor[] flagColors = map.FlagColors;
                foreach (Waypoint waypoint in map.Waypoints)
                {
                    if (!waypoint.Point.HasValue || (locations != null && !locations.Contains(waypoint.Point.Value)))
                    {
                        continue;
                    }
                    RenderWaypoint(gameType, map.BasicSection.SoloMission, tileSize, flagColors, waypoint).Item2(graphics);
                }
            }
        }

        public static void Render(GameType gameType, Map map, Graphics graphics, ISet<Point> locations, MapLayerFlag layers)
        {
            Render(gameType, map, graphics, locations, layers, Globals.MapTileScale);
        }

        public static (Rectangle, Action<Graphics>) RenderSmudge(TheaterType theater, Point topLeft, Size tileSize, double tileScale, Smudge smudge)
        {
            if (Globals.FilterTheaterObjects && smudge.Type.Theaters != null && !smudge.Type.Theaters.Contains(theater))
            {
                Debug.Print(string.Format("Smudge {0} ({1}) not available in this theater.", smudge.Type.Name, smudge.Icon));
                return (Rectangle.Empty, (g) => { });
            }
            Color tint = smudge.Tint;
            ImageAttributes imageAttributes = new ImageAttributes();
            if (tint != Color.White)
            {
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
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
            bool success = Globals.TheTilesetManager.GetTileData(smudge.Type.Name, smudge.Icon, out Tile tile, true, false);
            if (tile != null && tile.Image != null)
            {
                Rectangle smudgeBounds = RenderBounds(tile.Image.Size, new Size(1, 1), tileScale);
                smudgeBounds.X += topLeft.X * tileSize.Width;
                smudgeBounds.Y += topLeft.Y * tileSize.Width;
                if (!success)
                {
                    smudgeBounds.X += (int)Math.Round(smudgeBounds.Width * 0.1f);
                    smudgeBounds.Y += (int)Math.Round(smudgeBounds.Height * 0.1f);
                    smudgeBounds.Width = (int)Math.Round(smudgeBounds.Width * 0.8f);
                    smudgeBounds.Height = (int)Math.Round(smudgeBounds.Height * 0.8f);
                }
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

        public static (Rectangle, Action<Graphics>) RenderOverlay(GameType gameType, Point topLeft, Size tileSize, double tileScale, Overlay overlay)
        {
            OverlayType ovtype = overlay.Type;
            string name = ovtype.GraphicsSource;
            int icon = ovtype.IsConcrete || ovtype.IsResource || ovtype.IsWall || ovtype.ForceTileNr == -1 ? overlay.Icon : ovtype.ForceTileNr;
            bool isTeleport = gameType == GameType.SoleSurvivor && ovtype == SoleSurvivor.OverlayTypes.Teleport && Globals.AdjustSoleTeleports;
            bool success = Globals.TheTilesetManager.GetTileData(name, icon, out Tile tile, true, false);
            if (tile != null && tile.Image != null)
            {
                int actualTopLeftX = topLeft.X * tileSize.Width;
                int actualTopLeftY = topLeft.Y * tileSize.Height;
                Rectangle overlayBounds = RenderBounds(tile.Image.Size, new Size(1, 1), tileScale);
                overlayBounds.X += actualTopLeftX;
                overlayBounds.Y += actualTopLeftY;
                Color tint = overlay.Tint;
                // unused atm
                Single brightness = 1.0f;
                void render(Graphics g)
                {
                    ImageAttributes imageAttributes = new ImageAttributes();
                    if (tint != Color.White || brightness != 1.0f)
                    {
                        ColorMatrix colorMatrix = new ColorMatrix(new float[][]
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
            bool succeeded = Globals.TheTilesetManager.GetTileData(tileName, terrain.Type.DisplayIcon, out Tile tile, true, false);
            if (!succeeded && !String.Equals(terrain.Type.GraphicsSource, terrain.Type.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!String.Equals(terrain.Type.GraphicsSource, terrain.Type.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    succeeded = Globals.TheTilesetManager.GetTileData(terrain.Type.Name, terrain.Type.DisplayIcon, out tile, true, false);
                }
            }
            Color tint = terrain.Tint;
            ImageAttributes imageAttributes = new ImageAttributes();
            if (tint != Color.White)
            {
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] {tint.R / 255.0f, 0, 0, 0, 0},
                    new float[] {0, tint.G / 255.0f, 0, 0, 0},
                    new float[] {0, 0, tint.B / 255.0f, 0, 0},
                    new float[] {0, 0, 0, tint.A / 255.0f, 0},
                    new float[] {0, 0, 0, 0, 1},
                });
                imageAttributes.SetColorMatrix(colorMatrix);
            }
            Size terrTSize = terrain.Type.Size;
            Size tileISize = tile.Image.Size;
            Point location = new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height);
            Size maxSize = new Size(terrain.Type.Size.Width * tileSize.Width, terrain.Type.Size.Height * tileSize.Height);
            Rectangle paintBounds;
            if (!succeeded)
            {
                // Stretch dummy graphics over the whole size.
                paintBounds = new Rectangle(0, 0, (int)Math.Round(terrTSize.Width * tileISize.Width * tileScale), (int)Math.Round(terrTSize.Height * tileISize.Height * tileScale));
            }
            else
            {
                paintBounds = RenderBounds(tileISize, terrTSize, tileScale);
            }
            paintBounds.X += location.X;
            paintBounds.Y += location.Y;
            Rectangle terrainBounds = new Rectangle(location, maxSize);
            void render(Graphics g)
            {
                g.DrawImage(tile.Image, paintBounds, 0, 0, tile.Image.Width, tile.Image.Height, GraphicsUnit.Pixel, imageAttributes);
            }
            return (terrainBounds, render, false);
        }

        public static (Rectangle, Action<Graphics>, bool) RenderBuilding(GameType gameType, Point topLeft, Size tileSize, double tileScale, Building building)
        {
            Color tint = building.Tint;
            Int32 icon = building.Type.FrameOFfset;
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
                maxIcon = Globals.TheTilesetManager.GetTileDataLength(building.Type.GraphicsSource);
                //hasCollapseFrame = (gameType == GameType.TiberianDawn || gameType == GameType.SoleSurvivor) && maxIcon > 1 && maxIcon % 2 == 1;
                hasCollapseFrame = maxIcon > 1 && maxIcon % 2 == 1;
                damageIconOffs = (maxIcon + (hasCollapseFrame ? 0 : 1)) / 2;
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
            ITeamColor teamColor = building.Type.CanRemap ? Globals.TheTeamColorManager[building.House?.BuildingTeamColor] : null;
            bool succeeded = Globals.TheTilesetManager.GetTeamColorTileData(building.Type.GraphicsSource, icon, teamColor, out Tile tile, true, false);
            if (tile == null || tile.Image == null)
            {
                Debug.Print(string.Format("Building {0} ({1}) not found", building.Type.Name, icon));
                return (Rectangle.Empty, (g) => { }, false);
            }
            Point location = new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height);
            Size maxSize = new Size(building.Type.Size.Width * tileSize.Width, building.Type.Size.Height * tileSize.Height);

            Size bldTSize = building.Type.Size;
            Size tileISize = tile.Image.Size;
            Rectangle paintBounds;
            if (!succeeded)
            {
                // Stretch dummy graphics over the whole size.
                paintBounds = new Rectangle(0, 0, (int)Math.Round(bldTSize.Width * tileISize.Width * tileScale), (int)Math.Round(bldTSize.Height * tileISize.Height * tileScale));
            }
            else
            {
                paintBounds = RenderBounds(tileISize, bldTSize, tileScale);
            }
            Rectangle buildingBounds = new Rectangle(location, maxSize);
            Tile factoryOverlayTile = null;
            // Draw no factory overlay over the collapse frame.
            if (building.Type.FactoryOverlay != null && (building.Strength > 1 || !hasCollapseFrame))
            {
                int overlayIcon = 0;
                if (building.Strength <= healthyMin)
                {
                    int maxOverlayIcon = Globals.TheTilesetManager.GetTileDataLength(building.Type.FactoryOverlay);
                    overlayIcon = maxOverlayIcon / 2;
                }
                Globals.TheTilesetManager.GetTeamColorTileData(building.Type.FactoryOverlay, overlayIcon, Globals.TheTeamColorManager[building.House?.BuildingTeamColor], out factoryOverlayTile);
            }
            void render(Graphics g)
            {
                ImageAttributes imageAttributes = new ImageAttributes();
                if (tint != Color.White)
                {
                    ColorMatrix colorMatrix = new ColorMatrix(new float[][]
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
                            factoryG.CopyRenderSettingsFrom(g);
                            Size renderSize = tileISize;
                            if (!succeeded)
                            {
                                renderSize.Width = building.Type.Size.Width * tileSize.Width;
                                renderSize.Height = building.Type.Size.Height * tileSize.Height;
                            }
                            Rectangle factBounds = RenderBounds(renderSize, building.Type.Size, tileScale);
                            Rectangle ovrlBounds = RenderBounds(factoryOverlayTile.Image.Size, building.Type.Size, tileScale);
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

        public static (Rectangle, Action<Graphics>, bool) RenderInfantry(Point topLeft, Size tileSize, Infantry infantry, InfantryStoppingType infantryStoppingType)
        {
            Int32 icon = HumanShape[Facing32[infantry.Direction.ID]];
            ITeamColor teamColor = infantry.Type.CanRemap ? Globals.TheTeamColorManager[infantry.House?.UnitTeamColor] : null;
            Tile tile = null;
            // RA classic infantry remap support.
            bool success;
            if (infantry.Type.ClassicGraphicsSource != null && Globals.TheTilesetManager is TilesetManagerClassic tsmc)
            {
                success = tsmc.GetTeamColorTileData(infantry.Type.Name, icon, teamColor, out tile, true, false, infantry.Type.ClassicGraphicsSource, infantry.Type.ClassicGraphicsRemap);
            }
            else
            {
                success = Globals.TheTilesetManager.GetTeamColorTileData(infantry.Type.Name, icon, teamColor, out tile, true, false);
            }
            if (tile != null && tile.Image != null)
            {
                Size imSize = tile.Image.Size;
                // These values are experimental, from comparing map editor screenshots to game screenshots. -Nyer
                Point baseLocation = new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height);
                Point offset = Point.Empty;
                Rectangle renderBounds;
                Rectangle virtualBounds;
                if (success)
                {
                    baseLocation += new Size(tileSize.Width / 2, tileSize.Height / 2);
                    int infantryCorrectX = tileSize.Width / -12;
                    int infantryCorrectY = tileSize.Height / 6;
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
                    virtualBounds = new Rectangle(
                        new Point(baseLocation.X - (tile.OpaqueBounds.Width / 2), baseLocation.Y - tile.OpaqueBounds.Height),
                        tile.OpaqueBounds.Size
                    );
                    Size renderSize = new Size(imSize.Width * tileSize.Width / Globals.OriginalTileWidth, imSize.Height * tileSize.Height / Globals.OriginalTileHeight);
                    renderBounds = new Rectangle(baseLocation - new Size(renderSize.Width / 2, renderSize.Height / 2), renderSize);
                }
                else
                {
                    if (infantryStoppingType == InfantryStoppingType.UpperRight || infantryStoppingType == InfantryStoppingType.LowerRight)
                        offset.X += (tileSize.Width * 2 / 3);
                    if (infantryStoppingType == InfantryStoppingType.LowerLeft || infantryStoppingType == InfantryStoppingType.LowerRight)
                        offset.Y += (tileSize.Height / 2);
                    if (infantryStoppingType == InfantryStoppingType.Center)
                    {
                        offset.X += (tileSize.Height / 3);
                        offset.Y += (tileSize.Height / 4);
                    }
                    baseLocation.Offset(offset);
                    Size renderSize = new Size(imSize.Width * tileSize.Width / Globals.OriginalTileWidth, imSize.Height * tileSize.Height / Globals.OriginalTileHeight);
                    renderSize.Width /= 3;
                    renderSize.Height /= 2;
                    renderBounds = new Rectangle(baseLocation, renderSize);
                    virtualBounds = renderBounds;
                }
                Color tint = infantry.Tint;
                void render(Graphics g)
                {
                    ImageAttributes imageAttributes = new ImageAttributes();
                    if (tint != Color.White)
                    {
                        ColorMatrix colorMatrix = new ColorMatrix(new float[][]
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

        public static (Rectangle, Action<Graphics>, bool) RenderUnit(GameType gameType, Point topLeft, Size tileSize, Unit unit)
        {
            int icon = -1;
            if (gameType == GameType.TiberianDawn || gameType == GameType.SoleSurvivor)
            {
                if ((unit.Type == TiberianDawn.UnitTypes.Tric) ||
                         (unit.Type == TiberianDawn.UnitTypes.Trex) ||
                         (unit.Type == TiberianDawn.UnitTypes.Rapt) ||
                         (unit.Type == TiberianDawn.UnitTypes.Steg))
                {
                    Int32 facing = ((unit.Direction.ID + 0x10) & 0xFF) >> 5;
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
            ITeamColor teamColor = null;
            if (unit.House != null && unit.Type.CanRemap)
            {
                String teamColorName;
                if (!unit.House.OverrideTeamColors.TryGetValue(unit.Type.Name, out teamColorName))
                {
                    teamColorName = unit.House?.UnitTeamColor;
                }
                teamColor = Globals.TheTeamColorManager[teamColorName];
            }
            Globals.TheTilesetManager.GetTeamColorTileData(unit.Type.Name, icon, teamColor, out Tile tile, true, false);
            if (tile == null || tile.Image == null)
            {
                Debug.Print(string.Format("Unit {0} ({1}) not found", unit.Type.Name, icon));
                return (Rectangle.Empty, (g) => { }, false);
            }
            Size imSize = tile.Image.Size;
            Point location =
                new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height) +
                new Size(tileSize.Width / 2, tileSize.Height / 2);
            Size renderSize = new Size(imSize.Width * tileSize.Width / Globals.OriginalTileWidth, imSize.Height * tileSize.Height / Globals.OriginalTileHeight);
            Rectangle renderRect = new Rectangle(new Point(0, 0), renderSize);
            Rectangle renderBounds = new Rectangle(location - new Size(renderSize.Width / 2, renderSize.Height / 2), renderSize);
            Tile turretTile = null;
            Tile turret2Tile = null;
            if (unit.Type.HasTurret)
            {
                string turretName = unit.Type.Turret ?? unit.Type.Name;
                string turret2Name = unit.Type.HasDoubleTurret ? unit.Type.SecondTurret ?? unit.Type.Turret ?? unit.Type.Name : null;
                int turretIcon = unit.Type.Name.Equals(turretName, StringComparison.OrdinalIgnoreCase) ? icon + 32 : icon;
                int turret2Icon = unit.Type.Name.Equals(turret2Name, StringComparison.OrdinalIgnoreCase) ? icon + 32 : icon;
                // Special frame handling
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
                        // turret is an animation rather than a rotation; always take the first frame.
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
                    Globals.TheTilesetManager.GetTeamColorTileData(turretName, turretIcon, teamColor, out turretTile, false, false);
                if (turret2Name != null)
                    Globals.TheTilesetManager.GetTeamColorTileData(turret2Name, turret2Icon, teamColor, out turret2Tile, false, false);
            }
            Color tint = unit.Tint;
            void render(Graphics g)
            {
                ImageAttributes imageAttributes = new ImageAttributes();
                if (tint != Color.White)
                {
                    ColorMatrix colorMatrix = new ColorMatrix(new float[][]
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
                        unitG.CopyRenderSettingsFrom(g);
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

                            void RenderTurret(Graphics ug, Tile turrTile, Point turrAdjust, Size tSize)
                            {
                                Size turretSize = turrTile.Image.Size;
                                Size turretRenderSize = new Size(turretSize.Width * tSize.Width / Globals.OriginalTileWidth, turretSize.Height * tSize.Height / Globals.OriginalTileHeight);
                                Rectangle turrBounds = new Rectangle(center - new Size(turretRenderSize.Width / 2, turretRenderSize.Height / 2), turretRenderSize);
                                turrBounds.Offset(
                                    turrAdjust.X * tSize.Width / Globals.PixelWidth,
                                    turrAdjust.Y * tSize.Height / Globals.PixelHeight
                                );
                                ug.DrawImage(turrTile.Image, turrBounds, 0, 0, turrTile.Image.Width, turrTile.Image.Height, GraphicsUnit.Pixel);
                            }

                            if (turretTile != null && turretTile.Image != null)
                            {
                                RenderTurret(unitG, turretTile, turretAdjust, tileSize);
                            }
                            if (unit.Type.HasDoubleTurret && turret2Tile != null && turret2Tile.Image != null)
                            {
                                RenderTurret(unitG, turret2Tile, turret2Adjust, tileSize);
                            }
                        }
                    }
                    g.DrawImage(unitBm, renderBounds, 0,0, renderBounds.Width, renderBounds.Height, GraphicsUnit.Pixel, imageAttributes);
                }
            }
            return (renderBounds, render, false);
        }

        public static (Rectangle, Action<Graphics>) RenderWaypoint(GameType gameType, bool soloMission, Size tileSize, ITeamColor[] flagColors, Waypoint waypoint)
        {
            // Opacity is normally 0.5 for non-flag waypoint indicators, but is variable because the post-render
            // actions of the waypoints tool will paint a fully opaque version over the currently selected waypoint.
            //int mpId = Waypoint.GetMpIdFromFlag(waypoint.Flag);
            //float defaultOpacity = !soloMission && mpId >= 0 && mpId < flagColors.Length ? 1.0f : 0.5f;
            return RenderWaypoint(gameType, soloMission, tileSize, flagColors, waypoint, 0.5f);
        }

        public static (Rectangle, Action<Graphics>) RenderWaypoint(GameType gameType, bool soloMission, Size tileSize, ITeamColor[] flagColors, Waypoint waypoint, float transparencyModifier)
        {
            if (!waypoint.Point.HasValue)
            {
                return (Rectangle.Empty, (g) => { });
            }
            Point point = waypoint.Point.Value;
            bool isDefaultIcon = true;
            string tileGraphics = "beacon";
            int icon = 0;
            ITeamColor teamColor = null;
            double sizeMultiplier = 1;
            Color tint = waypoint.Tint;
            float brightness = 1.0f;
            int mpId = Waypoint.GetMpIdFromFlag(waypoint.Flag);
            bool gotTile = false;
            Tile tile;
            if (!soloMission && mpId >= 0 && mpId < flagColors.Length)
            {
                isDefaultIcon = false;
                tileGraphics = "flagfly";
                icon = 0;
                teamColor = flagColors[mpId];
                // Always paint flags as opaque.
                transparencyModifier = 1.0f;
                gotTile = Globals.TheTilesetManager.GetTeamColorTileData(tileGraphics, icon, teamColor, out tile);
            }
            else if (gameType == GameType.SoleSurvivor && (waypoint.Flag & WaypointFlag.CrateSpawn) == WaypointFlag.CrateSpawn)
            {
                isDefaultIcon = false;
                tileGraphics = "scrate";
                icon = 0;
                sizeMultiplier = 2;
                //tint = Color.FromArgb(waypoint.Tint.A, Color.Green);
                //brightness = 1.5f;
                gotTile = Globals.TheTilesetManager.GetTileData(tileGraphics, icon, out tile);
            }
            else
            {
                gotTile = Globals.TheTilesetManager.GetTileData(tileGraphics, icon, out tile);
            }
            if (!gotTile && isDefaultIcon)
            {
                // Beacon only exists in remastered graphics. Get fallback.
                tileGraphics = "trans.icn";
                icon = 3;
                gotTile = Globals.TheTilesetManager.GetTeamColorTileData(tileGraphics, icon, teamColor, out tile);
            }
            if (!gotTile)
            {
                Debug.Print(string.Format("Waypoint graphics {0} ({1}) not found", tileGraphics, icon));
                return (Rectangle.Empty, (g) => { });
            }
            Point location = new Point(point.X * tileSize.Width, point.Y * tileSize.Height);
            Size renderSize = new Size(tile.Image.Width * tileSize.Width / Globals.OriginalTileWidth, tile.Image.Height * tileSize.Height / Globals.OriginalTileHeight);
            renderSize.Width = (int)Math.Round(renderSize.Width * sizeMultiplier);
            renderSize.Height = (int)Math.Round(renderSize.Height * sizeMultiplier);
            Rectangle renderBounds = new Rectangle(location, renderSize);
            Rectangle imgBounds = new Rectangle(Point.Empty, tile.Image.Size);
            bool isClipping = renderSize.Width > tileSize.Width || renderSize.Height > tileSize.Height;
            if (tileSize.Width > renderSize.Width)
            {
                // Pad. This rounds upwards because bottom and left are generally shadows.
                renderBounds.X += (int)Math.Round((tileSize.Width - renderSize.Width) / 2.0, MidpointRounding.AwayFromZero);
            }
            else if (tileSize.Width < renderSize.Width)
            {
                // Crop
                renderBounds.Width = tileSize.Width;
                imgBounds.Width = (int)Math.Round(tileSize.Width / sizeMultiplier);
                imgBounds.X = (tile.Image.Width - imgBounds.Width) / 2;
            }
            if (tileSize.Height > renderSize.Height)
            {
                // Pad. This rounds upwards because bottom and left are generally shadows.
                renderBounds.Y += (int)Math.Round((tileSize.Height - renderSize.Height) / 2.0, MidpointRounding.AwayFromZero);
            }
            else if (tileSize.Height < renderSize.Height)
            {
                // Crop
                renderBounds.Height = tileSize.Height;
                imgBounds.Height = (int)Math.Round(tileSize.Height / sizeMultiplier);
                imgBounds.Y = (tile.Image.Height - imgBounds.Height) / 2;
            }
            void render(Graphics g)
            {
                ImageAttributes imageAttributes = new ImageAttributes();
                // Waypoints get drawn as semitransparent, so always execute this.
                if (tint != Color.White || brightness != 1.0 || transparencyModifier != 1.0)
                {
                    ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                    {
                            new float[] {tint.R * brightness / 255.0f, 0, 0, 0, 0},
                            new float[] {0, tint.G * brightness / 255.0f, 0, 0, 0},
                            new float[] {0, 0, tint.B * brightness / 255.0f, 0, 0},
                            new float[] {0, 0, 0, (tint.A * transparencyModifier) / 255.0f, 0},
                            new float[] {0, 0, 0, 0, 1},
                    });
                    imageAttributes.SetColorMatrix(colorMatrix);
                }
                g.DrawImage(tile.Image, renderBounds, imgBounds.X, imgBounds.Y, imgBounds.Width, imgBounds.Height, GraphicsUnit.Pixel, imageAttributes);
            }
            return (renderBounds, render);
        }

        public static void RenderAllBoundsFromCell<T>(Graphics graphics, Rectangle visibleCells, Size tileSize, IEnumerable<(int, T)> renderList, CellMetrics metrics)
        {
            RenderAllBoundsFromCell(graphics, visibleCells, tileSize, renderList, metrics, Color.Green);
        }

        public static void RenderAllBoundsFromCell<T>(Graphics graphics, Rectangle visibleCells, Size tileSize, IEnumerable<(int, T)> renderList, CellMetrics metrics, Color boundsColor)
        {
            RenderAllBoundsFromCell(graphics, visibleCells, tileSize, renderList.Select(tp => tp.Item1), metrics, boundsColor);
        }

        public static void RenderAllBoundsFromCell(Graphics graphics, Rectangle visibleCells, Size tileSize, IEnumerable<int> renderList, CellMetrics metrics, Color boundsColor)
        {
            using (Pen boundsPen = new Pen(boundsColor, Math.Max(1, tileSize.Width / 16.0f)))
            {
                foreach (Int32 cell in renderList)
                {
                    if (metrics.GetLocation(cell, out Point topLeft) && visibleCells.Contains(topLeft))
                    {
                        Rectangle bounds = new Rectangle(new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height), tileSize);
                        graphics.DrawRectangle(boundsPen, bounds);
                    }
                }
            }
        }

        public static void RenderAllBoundsFromPoint<T>(Graphics graphics, Rectangle visibleCells, Size tileSize, IEnumerable<(Point, T)> renderList)
        {
            RenderAllBoundsFromPoint(graphics, visibleCells, tileSize, renderList.Select(tp => tp.Item1), Color.Green);
        }

        public static void RenderAllBoundsFromPoint(Graphics graphics, Rectangle visibleCells, Size tileSize, IEnumerable<Point> renderList, Color boundsColor)
        {
            using (Pen boundsPen = new Pen(boundsColor, Math.Max(1, tileSize.Width / 16.0f)))
            {
                foreach (Point topLeft in renderList.Where(pt => visibleCells.Contains(pt)))
                {
                    Rectangle bounds = new Rectangle(new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height), tileSize);
                    graphics.DrawRectangle(boundsPen, bounds);
                }
            }
        }

        public static void RenderAllBoundsFromPoint<T>(Graphics graphics, Rectangle visibleCells, Size tileSize, IEnumerable<(Point, T)> renderList, Color boundsColor)
        {
            using (Pen boundsPen = new Pen(boundsColor, Math.Max(1, tileSize.Width / 16.0f)))
            {
                foreach ((Point topLeft, T _) in renderList.Where(pt => visibleCells.Contains(pt.Item1)))
                {
                    Rectangle bounds = new Rectangle(new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height), tileSize);
                    graphics.DrawRectangle(boundsPen, bounds);
                }
            }
        }

        public static void RenderAllOccupierBounds<T>(Graphics graphics, Rectangle visibleCells, Size tileSize, IEnumerable<(Point, T)> occupiers) where T : ICellOccupier, ICellOverlapper
        {
            RenderAllOccupierBounds(graphics, visibleCells, tileSize, occupiers, Color.Green, Color.Red);
        }

        public static void RenderAllOccupierBounds<T>(Graphics graphics, Rectangle visibleCells, Size tileSize, IEnumerable<(Point, T)> occupiers, Color boundsColor, Color OccupierColor) where T: ICellOccupier, ICellOverlapper
        {
            float boundsPenSize = Math.Max(1, tileSize.Width / 16.0f);
            float occupyPenSize = Math.Max(0.5f, tileSize.Width / 32.0f);
            if (occupyPenSize == boundsPenSize)
            {
                boundsPenSize += 2;
            }
            using (Pen boundsPen = new Pen(boundsColor, boundsPenSize))
            using (Pen occupyPen = new Pen(OccupierColor, occupyPenSize))
            {
                foreach ((Point topLeft, T occupier) in occupiers)
                {
                    Rectangle typeBounds = occupier.OverlapBounds;
                    Rectangle bounds = new Rectangle(
                        new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height),
                        new Size(typeBounds.Width * tileSize.Width, typeBounds.Height * tileSize.Height)
                    );
                    graphics.DrawRectangle(boundsPen, bounds);
                }
                foreach ((Point topLeft, T occupier) in occupiers)
                {
                    bool[,] occupyMask = occupier is Building bl ? bl.Type.BaseOccupyMask : occupier.OccupyMask;

                    for (Int32 y = 0; y < occupyMask.GetLength(0); ++y)
                    {
                        for (Int32 x = 0; x < occupyMask.GetLength(1); ++x)
                        {
                            if (occupyMask[y, x])
                            {
                                Rectangle occupyCellBounds = new Rectangle(new Point(topLeft.X + x, topLeft.Y + y), new Size(1, 1));
                                Rectangle occupyBounds = new Rectangle(
                                    new Point((topLeft.X + x) * tileSize.Width, (topLeft.Y + y) * tileSize.Height), tileSize);
                                if (visibleCells.Contains(occupyCellBounds))
                                {
                                    graphics.DrawRectangle(occupyPen, occupyBounds);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void RenderAllCrateOutlines(Graphics g, Map map, Rectangle visibleCells, Size tileSize, double tileScale, bool onlyIfBehindObjects)
        {
            // Optimised to only get the paint area once per crate type.
            // Sadly can't easily be cached because everything in this class is static.
            Dictionary<string, RegionData> paintAreas = new Dictionary<string, RegionData>();
            float outlineThickness = 0.05f;
            byte alphaThreshold = (byte)(Globals.UseClassicFiles ? 0x80 : 0x40);
            //double lumThreshold = 0.01d;
            foreach ((Int32 cell, Overlay overlay) in map.Overlay)
            {
                OverlayType ovlt = overlay.Type;
                Size cellSize = new Size(1, 1);
                if ((ovlt.Flag & OverlayTypeFlag.Crate) == OverlayTypeFlag.None
                    || !map.Metrics.GetLocation(cell, out Point location))
                {
                    continue;
                }
                if (!visibleCells.Contains(location) || (onlyIfBehindObjects && !IsOverlapped(map, location, false)))
                {
                    continue;
                }
                Color outlineCol = Color.FromArgb(0xA0, Color.Red);
                if ((ovlt.Flag & OverlayTypeFlag.Crate) == OverlayTypeFlag.WoodCrate) outlineCol = Color.FromArgb(0xA0, 0xFF, 0xC0, 0x40);
                if ((ovlt.Flag & OverlayTypeFlag.Crate) == OverlayTypeFlag.SteelCrate) outlineCol = Color.FromArgb(0xA0, Color.White);

                RegionData paintAreaRel;
                if (!paintAreas.TryGetValue(ovlt.Name, out paintAreaRel))
                {
                    // Clone with full opacity
                    Overlay toRender = overlay;
                    if (overlay.Tint.A != 255)
                    {
                        toRender = new Overlay()
                        {
                            Type = overlay.Type,
                            Icon = overlay.Icon,
                            Tint = Color.FromArgb(255, overlay.Tint),
                        };
                    }
                    using (Bitmap bm = new Bitmap(tileSize.Width, tileSize.Height, PixelFormat.Format32bppArgb))
                    {
                        using (Graphics ig = Graphics.FromImage(bm))
                        {
                            RenderOverlay(GameType.None, Point.Empty, tileSize, tileScale, toRender).Item2(ig);
                        }
                        paintAreaRel = ImageUtils.GetOutline(tileSize, bm, outlineThickness, alphaThreshold, Globals.UseClassicFiles);
                        paintAreas[ovlt.Name] = paintAreaRel;
                    }
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

        public static void RenderAllInfantryOutlines(Graphics g, Map map, Rectangle visibleCells, Size tileSize, bool onlyIfBehindObjects)
        {
            // Optimised to only get the paint area once per crate type.
            // Sadly can't easily be cached because everything in this class is static.
            Dictionary<string, RegionData> paintAreas = new Dictionary<string, RegionData>();
            float outlineThickness = 0.05f;
            byte alphaThreshold = (byte)(Globals.UseClassicFiles ? 0x80 : 0x40);
            //double lumThreshold = 0.01d;
            visibleCells.Inflate(1, 1);
            foreach (var (location, infantryGroup) in map.Technos.OfType<InfantryGroup>().OrderBy(i => map.Metrics.GetCell(i.Location)))
            {
                Size cellSize = new Size(1, 1);
                if (!visibleCells.Contains(location))
                {
                    continue;
                }
                if (onlyIfBehindObjects && !IsOverlapped(map, location, true))
                {
                    continue;
                }
                for (Int32 i = 0; i < infantryGroup.Infantry.Length; ++i)
                {
                    Infantry infantry = infantryGroup.Infantry[i];
                    if (infantry == null)
                    {
                        continue;
                    }
                    Color outlineCol = Color.FromArgb(0xA0, Globals.TheTeamColorManager.GetBaseColor(infantry.House?.UnitTeamColor));
                    RegionData paintAreaRel;
                    String id = infantry.Type.Name + '_' + i + '_' + infantry.Direction.ID;
                    if (!paintAreas.TryGetValue(id, out paintAreaRel))
                    {
                        // Clone with full opacity
                        if (infantry.Tint.A != 255)
                        {
                            infantry = infantry.Clone();
                            infantry.Tint = Color.FromArgb(255, infantry.Tint);
                        }
                        using (Bitmap bm = new Bitmap(tileSize.Width * 3, tileSize.Height * 3, PixelFormat.Format32bppArgb))
                        {
                            using (Graphics ig = Graphics.FromImage(bm))
                            {
                                RenderInfantry(new Point(1, 1), tileSize, infantry, (InfantryStoppingType)i).Item2(ig);
                            }
                            paintAreaRel = ImageUtils.GetOutline(tileSize, bm, outlineThickness, alphaThreshold, Globals.UseClassicFiles);
                            paintAreas[id] = paintAreaRel;
                        }
                    }
                    // Rendered in a 3x3 cell frame, so subtract one.
                    int actualTopLeftX = (location.X - 1) * tileSize.Width;
                    int actualTopLeftY = (location.Y - 1) * tileSize.Height;
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
        }

        public static void RenderAllVehicleOutlines(Graphics g, GameType gameType, Map map, Rectangle visibleCells, Size tileSize, bool onlyIfBehindObjects)
        {
            // Optimised to only get the paint area once per crate type.
            // Sadly can't easily be cached because everything in this class is static.
            Dictionary<string, RegionData> paintAreas = new Dictionary<string, RegionData>();
            float outlineThickness = 0.05f;
            byte alphaThreshold = (byte)(Globals.UseClassicFiles ? 0x80 : 0x40);
            //double lumThreshold = 0.01d;
            visibleCells.Inflate(1, 1);
            foreach (var (location, unit) in map.Technos.OfType<Unit>().OrderBy(i => map.Metrics.GetCell(i.Location)))
            {
                Size cellSize = new Size(1, 1);
                if (!visibleCells.Contains(location))
                {
                    continue;
                }
                if (onlyIfBehindObjects && !IsOverlapped(map, location, true))
                {
                    continue;
                }
                Color outlineCol = Color.FromArgb(0xA0, Globals.TheTeamColorManager.GetBaseColor(unit.House?.UnitTeamColor));
                RegionData paintAreaRel;
                String id = unit.Type.Name + '_' + unit.Direction.ID;
                if (!paintAreas.TryGetValue(id, out paintAreaRel))
                {
                    // Clone with full opacity
                    Unit toRender = unit;
                    if (unit.Tint.A != 255)
                    {
                        toRender = unit.Clone();
                        unit.Tint = Color.FromArgb(255, unit.Tint);
                    }
                    using (Bitmap bm = new Bitmap(tileSize.Width * 3, tileSize.Height * 3, PixelFormat.Format32bppArgb))
                    {
                        using (Graphics ig = Graphics.FromImage(bm))
                        {
                            RenderUnit(gameType, new Point(1, 1), tileSize, toRender).Item2(ig);
                        }
                        paintAreaRel = ImageUtils.GetOutline(tileSize, bm, outlineThickness, alphaThreshold, Globals.UseClassicFiles);
                        paintAreas[id] = paintAreaRel;
                    }

                }
                // Rendered in a 3x3 cell frame, so subtract one.
                int actualTopLeftX = (location.X - 1) * tileSize.Width;
                int actualTopLeftY = (location.Y - 1) * tileSize.Height;
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

        private static Boolean IsOverlapped(Map map,  Point location, bool ignoreUnits)
        {
            ICellOccupier techno = map.Technos[location];
            // Single-cell occupier. Always pass.
            if (!ignoreUnits && (techno is Unit || techno is InfantryGroup))
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
                List<Point> pts = boundsRect.Points().OrderBy(p => p.Y * map.Metrics.Width + p.X).ToList();
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

        public static void RenderAllFootballAreas(Graphics graphics, Map map, Rectangle visibleCells, Size tileSize, double tileScale, GameType gameType)
        {
            if (gameType != GameType.SoleSurvivor)
            {
                return;
            }
            HashSet<Point> footballPoints = new HashSet<Point>();
            Rectangle renderArea = map.Metrics.Bounds;
            renderArea.Intersect(visibleCells);
            foreach (Waypoint waypoint in map.Waypoints)
            {
                if (!waypoint.Point.HasValue || Waypoint.GetMpIdFromFlag(waypoint.Flag) == -1)
                {
                    continue;
                }
                Point[] roadPoints = new Rectangle(waypoint.Point.Value.X - 1, waypoint.Point.Value.Y - 1, 4, 3).Points().ToArray();
                foreach (Point p in roadPoints.Where(p => renderArea.Contains(p)))
                {
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
                RenderOverlay(gameType, p, tileSize, tileScale, footballTerrain).Item2(graphics);
            }
        }

        public static void RenderFootballAreaFlags(Graphics graphics, GameType gameType, Map map, Rectangle visibleCells, Size tileSize)
        {
            if (gameType != GameType.SoleSurvivor)
            {
                return;
            }
            // Re-render flags on top of football areas.
            List<Waypoint> footballWayPoints = new List<Waypoint>();
            foreach (Waypoint waypoint in map.Waypoints)
            {
                if (waypoint.Point.HasValue && Waypoint.GetMpIdFromFlag(waypoint.Flag) >= 0 && visibleCells.Contains(waypoint.Point.Value))
                {
                    footballWayPoints.Add(waypoint);
                }
            }
            ITeamColor[] flagColors = map.FlagColors;
            foreach (Waypoint wp in footballWayPoints)
            {
                RenderWaypoint(gameType, false, tileSize, flagColors, wp).Item2(graphics);
            }
        }

        public static void RenderAllFakeBuildingLabels(Graphics graphics, Map map, Rectangle visibleCells, Size tileSize)
        {
            foreach ((Point topLeft, Building building) in map.Buildings.OfType<Building>())
            {
                Rectangle buildingBounds = new Rectangle(topLeft, building.Type.Size);
                if (visibleCells.IntersectsWith(buildingBounds))
                {
                    RenderFakeBuildingLabel(graphics, building, topLeft, tileSize, false);
                }
            }
        }

        public static void RenderFakeBuildingLabel(Graphics graphics, Building building, Point topLeft, Size tileSize, Boolean forPreview)
        {
            if (!building.Type.IsFake)
            {
                return;
            }
            StringFormat stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            Size maxSize = building.Type.Size;
            Rectangle buildingBounds = new Rectangle(
                new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height),
                new Size(maxSize.Width * tileSize.Width, maxSize.Height * tileSize.Height)
            );
            string fakeText = Globals.TheGameTextManager["TEXT_UI_FAKE"];
            double tileScaleHor = tileSize.Width / 128.0;
            using (SolidBrush fakeBackgroundBrush = new SolidBrush(Color.FromArgb((forPreview ? 128 : 256) * 2 / 3, Color.Black)))
            using (SolidBrush fakeTextBrush = new SolidBrush(Color.FromArgb(forPreview ? building.Tint.A : 255, Color.White)))
            {
                using (Font font = graphics.GetAdjustedFont(fakeText, SystemFonts.DefaultFont, buildingBounds.Width, buildingBounds.Height,
                    Math.Max(1, (int)Math.Round(12 * tileScaleHor)), Math.Max(1, (int)Math.Round(24 * tileScaleHor)), stringFormat, true))
                {
                    SizeF textBounds = graphics.MeasureString(fakeText, font, buildingBounds.Width, stringFormat);
                    RectangleF backgroundBounds = new RectangleF(buildingBounds.Location, textBounds);
                    graphics.FillRectangle(fakeBackgroundBrush, backgroundBounds);
                    graphics.DrawString(fakeText, font, fakeTextBrush, backgroundBounds, stringFormat);
                }
            }
        }

        public static void RenderAllRebuildPriorityLabels(Graphics graphics, Map map, Rectangle visibleCells, Size tileSize)
        {
            foreach ((Point topLeft, Building building) in map.Buildings.OfType<Building>())
            {
                Rectangle buildingBounds = new Rectangle(topLeft, building.Type.Size);
                if (visibleCells.IntersectsWith(buildingBounds))
                {
                    RenderRebuildPriorityLabel(graphics, building, topLeft, tileSize, false);
                }
            }
        }

        public static void RenderRebuildPriorityLabel(Graphics graphics, Building building, Point topLeft, Size tileSize, Boolean forPreview)
        {
            StringFormat stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            Size maxSize = building.Type.Size;
            Rectangle buildingBounds = new Rectangle(
                new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height),
                new Size(maxSize.Width * tileSize.Width, maxSize.Height * tileSize.Height)
            );
            if (building.BasePriority >= 0)
            {
                double tileScaleHor = tileSize.Width / 128.0;
                string priText = building.BasePriority.ToString();
                using (SolidBrush baseBackgroundBrush = new SolidBrush(Color.FromArgb((forPreview ? 128 : 256) * 2 / 3, Color.Black)))
                using (SolidBrush baseTextBrush = new SolidBrush(Color.FromArgb(forPreview ? 128 : 255, Color.Red)))
                {
                    using (Font font = graphics.GetAdjustedFont(priText, SystemFonts.DefaultFont, buildingBounds.Width, buildingBounds.Height,
                        Math.Max(1, (int)Math.Round(12 * tileScaleHor)), Math.Max(1, (int)Math.Round(24 * tileScaleHor)), stringFormat, true))
                    {
                        SizeF textBounds = graphics.MeasureString(priText, font, buildingBounds.Width, stringFormat);
                        RectangleF backgroundBounds = new RectangleF(buildingBounds.Location, textBounds);
                        backgroundBounds.Offset((buildingBounds.Width - textBounds.Width) / 2.0f, buildingBounds.Height - textBounds.Height);
                        graphics.FillRectangle(baseBackgroundBrush, backgroundBounds);
                        graphics.DrawString(priText, font, baseTextBrush, backgroundBounds, stringFormat);
                    }
                }
            }
        }

        public static void RenderAllTechnoTriggers(Graphics graphics, Map map, Rectangle visibleCells, Size tileSize, MapLayerFlag layersToRender)
        {
            RenderAllTechnoTriggers(graphics, map, visibleCells, tileSize, layersToRender, Color.LimeGreen, null, false);
        }

        public static void RenderAllTechnoTriggers(Graphics graphics, Map map, Rectangle visibleCells, Size tileSize, MapLayerFlag layersToRender, Color color, string toPick, bool excludePick)
        {
            double tileScaleHor = tileSize.Width / 128.0;
            float borderSize = Math.Max(0.5f, tileSize.Width / 60.0f);
            foreach ((Point topLeft, ICellOccupier techno) in map.Technos)
            {
                Point location = new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height);
                (string trigger, Rectangle bounds, int alpha)[] triggers = null;
                if (techno is Terrain terrain && !Trigger.IsEmpty(terrain.Trigger))
                {
                    if ((layersToRender & MapLayerFlag.Terrain) == MapLayerFlag.Terrain)
                    {
                        if (visibleCells.IntersectsWith(new Rectangle(topLeft, terrain.Type.Size)))
                        {
                            Size size = new Size(terrain.Type.Size.Width * tileSize.Width, terrain.Type.Size.Height * tileSize.Height);
                            triggers = new (string, Rectangle, int)[] { (terrain.Trigger, new Rectangle(location, size), terrain.Tint.A) };
                        }
                    }
                }
                else if (techno is Building building && !Trigger.IsEmpty(building.Trigger))
                {
                    if ((layersToRender & MapLayerFlag.Buildings) == MapLayerFlag.Buildings)
                    {
                        if (visibleCells.IntersectsWith(new Rectangle(topLeft, building.Type.Size)))
                        {
                            Size size = new Size(building.Type.Size.Width * tileSize.Width, building.Type.Size.Height * tileSize.Height);
                            triggers = new (string, Rectangle, int)[] { (building.Trigger, new Rectangle(location, size), building.Tint.A) };
                        }
                    }
                }
                else if (techno is Unit unit && !Trigger.IsEmpty(unit.Trigger))
                {
                    if ((layersToRender & MapLayerFlag.Units) == MapLayerFlag.Units)
                    {
                        if (visibleCells.Contains(topLeft))
                        {
                            triggers = new (string, Rectangle, int)[] { (unit.Trigger, new Rectangle(location, tileSize), unit.Tint.A) };
                        }
                    }
                }
                else if (techno is InfantryGroup infantryGroup)
                {
                    if ((layersToRender & MapLayerFlag.Infantry) == MapLayerFlag.Infantry)
                    {
                        if (!visibleCells.Contains(topLeft))
                        {
                            continue;
                        }
                        List<(string, Rectangle, int)> infantryTriggers = new List<(string, Rectangle, int)>();
                        for (Int32 i = 0; i < infantryGroup.Infantry.Length; ++i)
                        {
                            Infantry infantry = infantryGroup.Infantry[i];
                            if (infantry == null || Trigger.IsEmpty(infantry.Trigger))
                            {
                                continue;
                            }
                            Size size = tileSize;
                            Size offset = Size.Empty;
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
                            Rectangle bounds = new Rectangle(location + offset, size);
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
                    foreach ((String trigger, Rectangle bounds, Int32 alpha) in triggers.Where(x => toPick == null
                    || (excludePick && !x.trigger.Equals(toPick, StringComparison.OrdinalIgnoreCase))
                     || (!excludePick && x.trigger.Equals(toPick, StringComparison.OrdinalIgnoreCase))))
                    {
                        Color alphaColor = Color.FromArgb(alpha, color);
                        using (SolidBrush technoTriggerBackgroundBrush = new SolidBrush(Color.FromArgb(96 * alpha / 256, Color.Black)))
                        using (SolidBrush technoTriggerBrush = new SolidBrush(alphaColor))
                        using (Pen technoTriggerPen = new Pen(alphaColor, borderSize))
                        using (Font font = graphics.GetAdjustedFont(trigger, SystemFonts.DefaultFont, bounds.Width, bounds.Height,
                            Math.Max(1, (int)Math.Round(12 * tileScaleHor)), Math.Max(1, (int)Math.Round(24 * tileScaleHor)), stringFormat, true))
                        {
                            SizeF textBounds = graphics.MeasureString(trigger, font, bounds.Width, stringFormat);
                            RectangleF backgroundBounds = new RectangleF(bounds.Location, textBounds);
                            backgroundBounds.Offset((bounds.Width - textBounds.Width) / 2.0f, (bounds.Height - textBounds.Height) / 2.0f);
                            graphics.FillRectangle(technoTriggerBackgroundBrush, backgroundBounds);
                            graphics.DrawRectangle(technoTriggerPen, Rectangle.Round(backgroundBounds));
                            graphics.DrawString(trigger, font, technoTriggerBrush, bounds, stringFormat);
                        }
                    }
                }
            }
        }

        public static void RenderWayPointIndicators(Graphics graphics, Map map, Rectangle visibleCells, Size tileSize, Color textColor, bool forPreview, bool excludeSpecified, params Waypoint[] specified)
        {
            HashSet<Waypoint> specifiedWaypoints = specified.ToHashSet();

            Waypoint[] toPaint = excludeSpecified ? map.Waypoints : specified;
            foreach (Waypoint waypoint in toPaint)
            {
                if (waypoint.Cell.HasValue && map.Metrics.GetLocation(waypoint.Cell.Value, out Point point) && visibleCells.Contains(point))
                {
                    if (excludeSpecified && specifiedWaypoints.Contains(waypoint))
                    {
                        continue;
                    }
                    RenderWayPointIndicator(graphics, waypoint, point, tileSize, textColor, forPreview);
                }
            }
        }

        public static void RenderWayPointIndicator(Graphics graphics, Waypoint waypoint, Point topLeft, Size tileSize, Color textColor, bool forPreview)
        {
            StringFormat stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            Rectangle paintBounds = new Rectangle(new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height), tileSize);
            // Adjust calcuations to tile size. The below adjustments are done assuming the tile is 128 wide.
            double tileScaleHor = tileSize.Width / 128.0;
            string wpText = waypoint.Name;
            using (SolidBrush baseBackgroundBrush = new SolidBrush(Color.FromArgb(forPreview ? 64 : 128, Color.Black)))
            using (SolidBrush baseTextBrush = new SolidBrush(Color.FromArgb(forPreview ? 128 : 255, textColor)))
            {
                using (Font font = graphics.GetAdjustedFont(wpText, SystemFonts.DefaultFont, paintBounds.Width, paintBounds.Height,
                    Math.Max(1, (int)Math.Round(12 * tileScaleHor)), Math.Max(1, (int)Math.Round(55 * tileScaleHor)), stringFormat, true))
                {
                    SizeF textBounds = graphics.MeasureString(wpText, font, paintBounds.Width, stringFormat);
                    RectangleF backgroundBounds = new RectangleF(paintBounds.Location, textBounds);
                    backgroundBounds.Offset((paintBounds.Width - textBounds.Width) / 2.0f, (paintBounds.Height - textBounds.Height) / 2.0f);
                    graphics.FillRectangle(baseBackgroundBrush, backgroundBounds);
                    graphics.DrawString(wpText, font, baseTextBrush, backgroundBounds, stringFormat);
                }
            }
        }

        public static void RenderAllBuildingEffectRadiuses(Graphics graphics, Map map, Rectangle visibleCells, Size tileSize, int effectRadius, Building selected)
        {
            foreach ((Point topLeft, Building building) in map.Buildings.OfType<Building>()
                .Where(b => (b.Occupier.Type.Flag & BuildingTypeFlag.IsGapGenerator) != BuildingTypeFlag.None))
            {
                RenderBuildingEffectRadius(graphics, visibleCells, tileSize, effectRadius, building, topLeft, selected);
            }
        }

        public static void RenderBuildingEffectRadius(Graphics graphics, Rectangle visibleCells, Size tileSize, int effectRadius, Building building, Point topLeft, Building selected)
        {
            if ((building.Type.Flag & BuildingTypeFlag.IsGapGenerator) != BuildingTypeFlag.IsGapGenerator)
            {
                return;
            }
            ITeamColor tc = Globals.TheTeamColorManager[building.House?.BuildingTeamColor];
            Color circleColor = Globals.TheTeamColorManager.GetBaseColor(tc?.Name);
            bool[,] cells = building.Type.BaseOccupyMask;
            int maskY = cells.GetLength(0);
            int maskX = cells.GetLength(1);
            Rectangle circleCellBounds = GeneralUtils.GetBoxFromCenterCell(topLeft, maskX, maskY, effectRadius, effectRadius, new Size(1,1), out _);
            Rectangle circleBounds = GeneralUtils.GetBoxFromCenterCell(topLeft, maskX, maskY, effectRadius, effectRadius, tileSize, out Point center);
            if (visibleCells.IntersectsWith(circleCellBounds))
            {
                Color alphacorr = Color.FromArgb(selected == building ? 255 : (building.Tint.A * 128 / 256), circleColor);
                RenderCircleDiagonals(graphics, tileSize, alphacorr, effectRadius, effectRadius, center);
                DrawDashesCircle(graphics, circleBounds, tileSize, alphacorr, true, -1.25f, 2.5f);
            }
        }

        public static void RenderAllUnitEffectRadiuses(Graphics graphics, Map map, Rectangle visibleCells, Size tileSize, int jamRadius, Unit selected)
        {
            foreach ((Point topLeft, Unit unit) in map.Technos.OfType<Unit>()
                .Where(b => (b.Occupier.Type.Flag & (UnitTypeFlag.IsGapGenerator | UnitTypeFlag.IsJammer)) != UnitTypeFlag.None))
            {
                RenderUnitEffectRadius(graphics, tileSize, jamRadius, unit, topLeft, visibleCells, selected);
            }
        }

        public static void RenderUnitEffectRadius(Graphics graphics, Size tileSize, int jamRadius, Unit unit, Point cell, Rectangle visibleCells, Unit selected)
        {
            bool isJammer = (unit.Type.Flag & UnitTypeFlag.IsJammer) == UnitTypeFlag.IsJammer;
            bool isGapGen = (unit.Type.Flag & UnitTypeFlag.IsGapGenerator) == UnitTypeFlag.IsGapGenerator;
            if (!isJammer && !isGapGen)
            {
                return;
            }
            ITeamColor tc = Globals.TheTeamColorManager[unit.House?.BuildingTeamColor];
            Color circleColor = Globals.TheTeamColorManager.GetBaseColor(tc?.Name);
            Color alphacorr = Color.FromArgb(unit == selected ? 255 : (unit.Tint.A * 128 / 256), circleColor);
            if (isJammer)
            {
                // uses map's Gap Generator range.
                Rectangle circleCellBounds = GeneralUtils.GetBoxFromCenterCell(cell, 1, 1, jamRadius, jamRadius, new Size(1, 1), out _);
                Rectangle circleBounds = GeneralUtils.GetBoxFromCenterCell(cell, 1, 1, jamRadius, jamRadius, tileSize, out Point center);
                if (visibleCells.IntersectsWith(circleCellBounds))
                {
                    RenderCircleDiagonals(graphics, tileSize, alphacorr, jamRadius, jamRadius, center);
                    DrawDashesCircle(graphics, circleBounds, tileSize, alphacorr, true, -1.25f, 2.5f);
                }
            }
            if (isGapGen)
            {
                // uses specific 5x7 circle around the unit cell
                int radiusX = 2;
                int radiusY = 3;
                Rectangle circleCellBounds = GeneralUtils.GetBoxFromCenterCell(cell, 1, 1, radiusX, radiusY, new Size(1, 1), out _);
                Rectangle circleBounds = GeneralUtils.GetBoxFromCenterCell(cell, 1, 1, radiusX, radiusY, tileSize, out Point center);
                if (visibleCells.IntersectsWith(circleCellBounds))
                {
                    RenderCircleDiagonals(graphics, tileSize, alphacorr, radiusX, radiusY, center);
                    DrawDashesCircle(graphics, circleBounds, tileSize, alphacorr, true, -1.25f, 2.5f);
                }
            }
        }

        private static void RenderCircleDiagonals(Graphics graphics, Size tileSize, Color paintColor, double radiusX, double radiusY, Point center)
        {

            float penSize = Math.Max(1.0f, tileSize.Width / 16.0f);
            using (Pen linePen = new Pen(paintColor, penSize))
            {
                linePen.DashPattern = new float[] { 1.0F, 4.0F, 6.0F, 4.0F };
                linePen.DashCap = DashCap.Round;
                int diamX = (int)Math.Round((radiusX * 2 + 1) * tileSize.Width);
                int radX = diamX / 2;
                int diamY = (int)Math.Round((radiusY * 2 + 1) * tileSize.Height);
                int radY = diamY / 2;
                double sinDistance = Math.Sin(Math.PI * 45 / 180.0);
                int sinX = (int)Math.Round(radX * sinDistance);
                int sinY = (int)Math.Round(radY * sinDistance);
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

        public static void RenderAllWayPointRevealRadiuses(Graphics graphics, IGamePlugin plugin, Map map, Rectangle visibleCells, Size tileSize, Waypoint selectedItem)
        {
            RenderAllWayPointRevealRadiuses(graphics, plugin, map, visibleCells, tileSize, selectedItem, false);
        }

        public static void RenderAllWayPointRevealRadiuses(Graphics graphics, IGamePlugin plugin, Map map, Rectangle visibleCells, Size tileSize, Waypoint selectedItem, bool onlySelected)
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
                        RenderWayPointRevealRadius(graphics, map.Metrics, visibleCells, tileSize, drawColor, isSelected, false, wpReveal1[i], cur);
                    }
                    if (wpReveal2[i] != 0)
                    {
                        RenderWayPointRevealRadius(graphics, map.Metrics, visibleCells, tileSize, drawColor, isSelected, false, wpReveal2[i], cur);
                    }
                }
            }
        }

        public static void RenderWayPointRevealRadius(Graphics graphics, CellMetrics metrics, Rectangle visibleCells, Size tileSize, Color circleColor, bool isSelected, bool forPreview, double revealRadius, Waypoint waypoint)
        {
            if (waypoint.Cell.HasValue && metrics.GetLocation(waypoint.Cell.Value, out Point cellPoint))
            {
                double diam = revealRadius * 2 + 1;
                Rectangle circleCellBounds = new Rectangle(
                    (int)Math.Round(cellPoint.X - revealRadius),
                    (int)Math.Round(cellPoint.Y - revealRadius),
                    (int)Math.Round(diam),
                    (int)Math.Round(diam));
                Rectangle circleBounds = new Rectangle(
                    (int)Math.Round(cellPoint.X * tileSize.Width - revealRadius * tileSize.Width),
                    (int)Math.Round(cellPoint.Y * tileSize.Width - revealRadius * tileSize.Height),
                    (int)Math.Round(diam * tileSize.Width),
                    (int)Math.Round(diam * tileSize.Height));
                if (visibleCells.IntersectsWith(circleCellBounds))
                {
                    DrawDashesCircle(graphics, circleBounds, tileSize, Color.FromArgb(isSelected && !forPreview ? 255 : 128, circleColor), isSelected, 1.25f, 2.5f);
                }
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

        public static void RenderCellTriggersSoft(Graphics graphics, Map map, Rectangle visibleCells, Size tileSize, params String[] specifiedToExclude)
        {
            RenderCellTriggers(graphics, map, visibleCells, tileSize, Color.Black, Color.White, Color.White, 0.75f, false, true, specifiedToExclude);
        }

        public static void RenderCellTriggersHard(Graphics graphics, Map map, Rectangle visibleCells, Size tileSize, params String[] specifiedToExclude)
        {
            RenderCellTriggers(graphics, map, visibleCells, tileSize, Color.Black, Color.White, Color.White, 1, false, true, specifiedToExclude);
        }

        public static void RenderCellTriggersSelected(Graphics graphics, Map map, Rectangle visibleCells, Size tileSize, params String[] specifiedToDraw)
        {
            RenderCellTriggers(graphics, map, visibleCells, tileSize, Color.Black, Color.Yellow, Color.Yellow, 1, true, false, specifiedToDraw);
        }

        public static void RenderCellTriggers(Graphics graphics, Map map, Rectangle visibleCells, Size tileSize, Color fillColor, Color borderColor, Color textColor, double alphaAdjust, bool thickborder, bool excludeSpecified, params String[] specified)
        {
            // For bounds, add one more cell to get all borders showing.
            Rectangle boundRenderCells = visibleCells;
            boundRenderCells.Inflate(1, 1);
            boundRenderCells.Intersect(map.Metrics.Bounds);
            HashSet<string> specifiedSet = new HashSet<string>(specified, StringComparer.OrdinalIgnoreCase);
            List<(Point p, CellTrigger cellTrigger)> toRender = new List<(Point p, CellTrigger cellTrigger)>();
            HashSet<string> toRenderSet = new HashSet<string>();
            List<(Point p, CellTrigger cellTrigger)> boundsToDraw = new List<(Point p, CellTrigger cellTrigger)>();
            foreach ((Int32 cell, CellTrigger cellTrigger) in map.CellTriggers.OrderBy(c => c.Cell))
            {
                Int32 x = cell % map.Metrics.Width;
                Int32 y = cell / map.Metrics.Width;
                if (!boundRenderCells.Contains(x, y))
                {
                    continue;
                }
                bool contains = specifiedSet.Contains(cellTrigger.Trigger);
                if (contains && excludeSpecified || !contains && !excludeSpecified)
                {
                    continue;
                }
                bool isPreview = cellTrigger.Tint.A != 255;
                Point p = new Point(x, y);
                if (visibleCells.Contains(x, y))
                {
                    toRender.Add((p, cellTrigger));
                    toRenderSet.Add(cellTrigger.Trigger + "=" + (isPreview ? 'P' : 'N'));
                }
                boundsToDraw.Add((p, cellTrigger));
            }
            if (boundsToDraw.Count == 0)
            {
                return;
            }
            Color ApplyAlpha(Color col, int baseAlpha, double alphaMul)
            {
                return Color.FromArgb(Math.Max(0, Math.Min(0xFF, (int)Math.Round(baseAlpha * alphaMul, MidpointRounding.AwayFromZero))), col);
            };
            // Actual balance is fixed; border is 1, text is 1/2, background is 3/8. The original alpha inside the given colours is ignored.
            fillColor = ApplyAlpha(fillColor, 0x60, alphaAdjust);
            borderColor = ApplyAlpha(borderColor, 0xFF, alphaAdjust);
            textColor = ApplyAlpha(textColor, 0x80, alphaAdjust);
            Color previewFillColor = ApplyAlpha(fillColor, 0x60, alphaAdjust / 2);
            Color previewBorderColor = ApplyAlpha(borderColor, 0xFF, alphaAdjust / 2);
            Color previewTextColor = ApplyAlpha(textColor, 0x80, alphaAdjust / 2);

            Dictionary<string, Bitmap> renders = new Dictionary<string, Bitmap>();
            try
            {
                int sizeW = 128;
                int sizeH = 128;
                double tileScaleHor = sizeW / 128.0;
                Rectangle tileBounds = new Rectangle(0, 0, sizeW - 1, sizeH - 1);
                using (SolidBrush prevCellTriggersBackgroundBrush = new SolidBrush(previewFillColor))
                using (SolidBrush prevCellTriggersBrush = new SolidBrush(previewTextColor))
                using (SolidBrush cellTriggersBackgroundBrush = new SolidBrush(fillColor))
                using (SolidBrush cellTriggersBrush = new SolidBrush(textColor))
                {
                    foreach (string trigger in toRenderSet)
                    {
                        string[] trigPart = trigger.Split('=');
                        if (trigPart.Length != 2)
                        {
                            continue;
                        }
                        string text = trigPart[0];
                        bool isPreview = trigPart[1] == "P";
                        Color textCol = isPreview ? previewTextColor : textColor;

                        Bitmap bm = new Bitmap(sizeW, sizeH);
                        using (Graphics ctg = Graphics.FromImage(bm))
                        {
                            SetRenderSettings(ctg, true);
                            Rectangle textBounds = new Rectangle(Point.Empty, tileBounds.Size);
                            StringFormat stringFormat = new StringFormat
                            {
                                Alignment = StringAlignment.Center,
                                LineAlignment = StringAlignment.Center
                            };
                            //*/
                            ctg.FillRectangle(isPreview ? prevCellTriggersBackgroundBrush : cellTriggersBackgroundBrush, textBounds);
                            using (Bitmap textBm = new Bitmap(sizeW, sizeH))
                            {
                                using (Graphics textGr = Graphics.FromImage(textBm))
                                using (Font font = ctg.GetAdjustedFont(text, SystemFonts.DefaultFont, textBounds.Width, textBounds.Height,
                                    Math.Max(1, (int)Math.Round(24 * tileScaleHor)), Math.Max(1, (int)Math.Round(48 * tileScaleHor)), stringFormat, true))
                                {
                                    // If not set, the text will not use alpha
                                    textGr.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
                                    textGr.DrawString(text, font, isPreview ? prevCellTriggersBrush : cellTriggersBrush, textBounds, stringFormat);
                                }
                                // Clear background under text to make it more transparent. There are probably more elegant ways to do this, but this works.
                                RegionData textInline = ImageUtils.GetOutline(new Size(sizeW, sizeH), textBm, 0.00f, (byte)Math.Max(0, textCol.A - 1), true);
                                using (Region clearArea = new Region(textInline))
                                using (Brush clear = new SolidBrush(Color.Transparent))
                                {
                                    ctg.CompositingMode = CompositingMode.SourceCopy;
                                    ctg.FillRegion(clear, clearArea);
                                    ctg.CompositingMode = CompositingMode.SourceOver;
                                }
                                ctg.DrawImage(textBm, new Rectangle(0, 0, sizeW, sizeH), 0, 0, sizeW, sizeH, GraphicsUnit.Pixel);
                            }
                        }
                        renders.Add(trigger, bm);
                    }
                }

                var backupCompositingQuality = graphics.CompositingQuality;
                var backupInterpolationMode = graphics.InterpolationMode;
                var backupSmoothingMode = graphics.SmoothingMode;
                var backupPixelOffsetMode = graphics.PixelOffsetMode;
                SetRenderSettings(graphics, true);
                foreach ((Point p, CellTrigger cellTrigger) in toRender)
                {
                    bool isPreview = cellTrigger.Tint.A != 255;
                    string requestName = cellTrigger.Trigger + "=" + (isPreview ? 'P' : 'N');
                    if (renders.TryGetValue(requestName, out Bitmap ctBm))
                    {
                        Rectangle renderBounds = new Rectangle(p.X * tileSize.Width, p.Y * tileSize.Height, tileSize.Width, tileSize.Height);
                        graphics.DrawImage(ctBm, renderBounds, tileBounds, GraphicsUnit.Pixel);
                    }
                }
                graphics.CompositingQuality = backupCompositingQuality;
                graphics.InterpolationMode = backupInterpolationMode;
                graphics.SmoothingMode = backupSmoothingMode;
                graphics.PixelOffsetMode = backupPixelOffsetMode;

            }
            finally
            {
                Bitmap[] bms = renders.Values.ToArray();
                for (int i = 0; i < bms.Length; i++)
                {
                    try { bms[i].Dispose(); }
                    catch { /* ignore */ }
                }
            }
            float borderSize = Math.Max(0.5f, tileSize.Width / 60.0f);
            float thickBorderSize = Math.Max(1f, tileSize.Width / 20.0f);
            using (Pen prevBorderPen = new Pen(previewBorderColor, thickborder ? thickBorderSize : borderSize))
            using (Pen borderPen = new Pen(borderColor, thickborder ? thickBorderSize : borderSize))
            {
                foreach ((Point p, CellTrigger cellTrigger) in boundsToDraw)
                {
                    bool isPreview = cellTrigger.Tint.A != 255;
                    Rectangle bounds = new Rectangle(new Point(p.X * tileSize.Width, p.Y * tileSize.Height), tileSize);
                    graphics.DrawRectangle(isPreview ? prevBorderPen : borderPen, bounds);
                }
            }
        }

        public static void RenderMapBoundaries(Graphics graphics, Map map, Rectangle visibleCells, Size tileSize)
        {
            RenderMapBoundaries(graphics, map.Bounds, visibleCells, tileSize, Color.Cyan);
        }

        public static void RenderMapBoundaries(Graphics graphics, Rectangle bounds, Rectangle visibleCells, Size tileSize, Color color)
        {
            // Inflate so you'd see the indicator if it's at the edge.
            visibleCells.Inflate(1, 1);
            Rectangle cropped = bounds;
            cropped.Intersect(visibleCells);
            // If these two are identical, that means all map borders are at least one cell outside the visible cells area.
            if (visibleCells == cropped)
            {
                return;
            }
            Rectangle boundsRect = Rectangle.FromLTRB(
                bounds.Left * tileSize.Width,
                bounds.Top * tileSize.Height,
                bounds.Right * tileSize.Width,
                bounds.Bottom * tileSize.Height
            );
            using (Pen boundsPen = new Pen(color, Math.Max(1f, tileSize.Width / 8.0f)))
            {
                graphics.DrawRectangle(boundsPen, boundsRect);
            }
        }

        public static void RenderMapSymmetry(Graphics graphics, Rectangle bounds, Size tileSize, Color color)
        {
            Rectangle boundsRect = Rectangle.FromLTRB(
                bounds.Left * tileSize.Width,
                bounds.Top * tileSize.Height,
                bounds.Right * tileSize.Width,
                bounds.Bottom * tileSize.Height
            );
            using (Pen boundsPen = new Pen(color, Math.Max(1f, tileSize.Width / 8.0f)))
            {
                graphics.DrawLine(boundsPen, new Point(boundsRect.X, boundsRect.Y), new Point(boundsRect.Right, boundsRect.Bottom));
                graphics.DrawLine(boundsPen, new Point(boundsRect.Right, boundsRect.Y), new Point(boundsRect.X, boundsRect.Bottom));

                int halfX = boundsRect.X + boundsRect.Width / 2;
                int halfY = boundsRect.Y + boundsRect.Height / 2;
                graphics.DrawLine(boundsPen, new Point(halfX, boundsRect.Y), new Point(halfX, boundsRect.Bottom));
                graphics.DrawLine(boundsPen, new Point(boundsRect.X, halfY), new Point(boundsRect.Right, halfY));
            }
        }

        public static void RenderMapGrid(Graphics graphics, Rectangle renderBounds, Rectangle mapBounds, bool mapBoundsRendered, Size tileSize, Color color)
        {
            Rectangle boundvisibleCells = mapBounds;
            boundvisibleCells.Intersect(renderBounds);
            int startY = boundvisibleCells.Y;
            int startX = boundvisibleCells.X;
            int endRight = boundvisibleCells.Right;
            int endBottom = boundvisibleCells.Bottom;
            if (mapBoundsRendered)
            {
                if (boundvisibleCells.Y == mapBounds.Y)
                {
                    startY++;
                }
                if (boundvisibleCells.Bottom == mapBounds.Bottom)
                {
                    endBottom--;
                }
                if (boundvisibleCells.X == mapBounds.X)
                {
                    startX++;
                }
                if (boundvisibleCells.Right == mapBounds.Right)
                {
                    endRight--;
                }
            }
            using (Pen gridPen = new Pen(color, Math.Max(1f, tileSize.Width / 16.0f)))
            {
                int leftBound = boundvisibleCells.X * tileSize.Width;
                int rightBound = boundvisibleCells.Right * tileSize.Width;
                for (int y = startY; y <= endBottom; ++y)
                {
                    int ymul = y * tileSize.Height;
                    graphics.DrawLine(gridPen, new Point(leftBound, ymul), new Point(rightBound, ymul));
                }
                //*/
                int topBound = boundvisibleCells.Y * tileSize.Height;
                int bottomBound = boundvisibleCells.Bottom * tileSize.Height;
                for (int x = startX; x <= endRight; ++x)
                {
                    int xmul = x * tileSize.Height;
                    graphics.DrawLine(gridPen, new Point(xmul, topBound), new Point(xmul, bottomBound));
                }
                //*/
            }
        }

        public static void RenderLandTypes(Graphics graphics, IGamePlugin plugin, CellGrid<Template> templates, Size tileSize, Rectangle visibleCells, bool ignoreClear)
        {
            // Check which cells need to be marked.
            List<(int, int)> cellsVehImpassable = new List<(int, int)>();
            List<(int, int)> cellsUnbuildable = new List<(int, int)>();
            List<(int, int)> cellsBoatMovable = new List<(int, int)>();
            List<(int, int)> cellsRiver = new List<(int, int)>();
            // Possibly fetch the terrain type for clear terrain on this theater?
            TemplateType clear = plugin.Map.TemplateTypes.Where(t => (t.Flag & TemplateTypeFlag.Clear) == TemplateTypeFlag.Clear).FirstOrDefault();
            LandType clearLand = clear.LandTypes.Length > 0 ? clear.LandTypes[0] : LandType.Clear;
            // Caching this in advance for all types.
            LandType[] landTypes = (LandType[])Enum.GetValues(typeof(LandType));
            bool[][] passable = new bool[landTypes.Length][];
            for (int i = 0; i < landTypes.Length; i++)
            {
                LandType landType = landTypes[i];
                passable[i] = new bool[3];
                passable[i][0] = plugin.IsLandUnitPassable(landType);
                passable[i][1] = plugin.IsBoatPassable(landType);
                passable[i][2] = plugin.IsBuildable(landType);
            }
            // The actual check.
            for (int y = visibleCells.Y; y < visibleCells.Bottom; ++y)
            {
                for (int x = visibleCells.X; x < visibleCells.Right; ++x)
                {
                    Template template = templates[y, x];
                    LandType land = LandType.None;
                    if (template == null)
                    {
                        if (!ignoreClear)
                        {
                            land = clearLand;
                        }
                    }
                    else
                    {
                        LandType[] types = template.Type.LandTypes;
                        int icon = (template.Type.Flag & (TemplateTypeFlag.Clear | TemplateTypeFlag.RandomCell)) != TemplateTypeFlag.None ? 0 : template.Icon;
                        land = icon < types.Length ? types[icon] : LandType.Clear;
                    }
                    // Exclude uninitialised terrain
                    if (land != LandType.None)
                    {
                        bool isLandUnitPassable = passable[(int)land][0];
                        bool isBoatPassable = passable[(int)land][1];
                        bool isBuildable = passable[(int)land][2];
                        if (isLandUnitPassable)
                        {
                            if (!isBuildable)
                            {
                                cellsUnbuildable.Add((x, y));
                            }
                        }
                        else
                        {
                            if (isBoatPassable)
                            {
                                cellsBoatMovable.Add((x, y));
                            }
                            else
                            {
                                if (land == LandType.River || land == LandType.Water)
                                {
                                    // Special case; impassable water.
                                    cellsRiver.Add((x, y));
                                }
                                else
                                {
                                    cellsVehImpassable.Add((x, y));
                                }
                            }
                        }
                    }
                }
            }
            // On to the painting part.
            Bitmap bmImp = null;
            bool disposeBmImp = false;
            Bitmap bmUnb = null;
            bool disposeBmUnb = false;
            Bitmap bmWtr = null;
            bool disposeBmWtr = false;
            Bitmap bmRiv = null;
            int tileWidth = tileSize.Width;
            int tileHeight = tileSize.Height;
            float lineSize = tileWidth / 16.0f;
            int lineOffsetW = tileWidth / 4;
            int lineOffsetH = tileHeight / 4;
            Tile tile;
            if (Globals.TheTilesetManager.GetTileData("trans.icn", 2, out tile) && tile != null) bmImp = tile.Image; // red
            if (Globals.TheTilesetManager.GetTileData("trans.icn", 1, out tile) && tile != null) bmUnb = tile.Image; // yellow
            if (Globals.TheTilesetManager.GetTileData("trans.icn", 0, out tile) && tile != null) bmWtr = tile.Image; // white
            try
            {
                var colorMatrix = new ColorMatrix();
                colorMatrix.Matrix33 = 0.50f;
                var imageAttributes = new ImageAttributes();
                imageAttributes.SetColorMatrix(
                    colorMatrix,
                    ColorMatrixFlag.Default,
                    ColorAdjustType.Bitmap);
                // If the graphics could not be loaded from the clasic files, generate them.
                if (bmImp == null && cellsVehImpassable.Count > 0)
                {
                    disposeBmImp = true;
                    bmImp = GenerateLinesBitmap(tileWidth, tileHeight, Color.FromArgb(170, 0,0), lineSize, lineOffsetW, lineOffsetH, graphics);
                }
                if (bmUnb == null && cellsUnbuildable.Count > 0)
                {
                    disposeBmUnb = true;
                    bmUnb = GenerateLinesBitmap(tileWidth, tileHeight, Color.FromArgb(255, 255, 85), lineSize, lineOffsetW, lineOffsetH, graphics);
                }
                if (bmWtr == null)
                {
                    if (cellsBoatMovable.Count > 0)
                    {
                        disposeBmWtr = true;
                        bmWtr = GenerateLinesBitmap(tileSize.Width, tileSize.Height, Color.FromArgb(255, 255, 255), lineSize, lineOffsetW, lineOffsetH, graphics);
                    }
                    if (cellsRiver.Count > 0)
                    {
                        bmRiv = GenerateLinesBitmap(tileSize.Width, tileSize.Height, Color.FromArgb(0, 0, 255), lineSize, lineOffsetW, lineOffsetH, graphics);
                    }
                }
                else if (cellsRiver.Count > 0)
                {
                    bmRiv = new Bitmap(bmWtr);
                    RegionData lines = ImageUtils.GetOutline(bmWtr.Size, bmWtr, 0.00f, 0x80, true);
                    using (Graphics bgr = Graphics.FromImage(bmRiv))
                    using (Region blueArea = new Region(lines))
                    using (Brush blueBrush = new SolidBrush(Color.FromArgb(0, 0, 255)))
                    {
                        bgr.FillRegion(blueBrush, blueArea);
                    }
                }
                // Finally, paint the actual cells.
                foreach ((int x, int y) in cellsVehImpassable)
                {
                    graphics.DrawImage(bmImp, new Rectangle(tileWidth * x, tileHeight * y, tileWidth, tileHeight), 0, 0, bmImp.Width, bmImp.Height, GraphicsUnit.Pixel, imageAttributes);
                }
                foreach ((int x, int y) in cellsUnbuildable)
                {
                    graphics.DrawImage(bmUnb, new Rectangle(tileWidth * x, tileHeight * y, tileWidth, tileHeight), 0, 0, bmUnb.Width, bmUnb.Height, GraphicsUnit.Pixel, imageAttributes);
                }
                foreach ((int x, int y) in cellsBoatMovable)
                {
                    graphics.DrawImage(bmWtr, new Rectangle(tileWidth * x, tileHeight * y, tileWidth, tileHeight), 0, 0, bmWtr.Width, bmWtr.Height, GraphicsUnit.Pixel, imageAttributes);
                }
                foreach ((int x, int y) in cellsRiver)
                {
                    graphics.DrawImage(bmRiv, new Rectangle(tileWidth * x, tileHeight * y, tileWidth, tileHeight), 0, 0, bmRiv.Width, bmRiv.Height, GraphicsUnit.Pixel, imageAttributes);
                }
            }
            finally
            {
                if (disposeBmImp && bmImp != null) try { bmImp.Dispose(); } catch { /* ignore */ }
                if (disposeBmUnb && bmUnb != null) try { bmUnb.Dispose(); } catch { /* ignore */ }
                if (disposeBmWtr && bmWtr != null) try { bmWtr.Dispose(); } catch { /* ignore */ }
                if (bmRiv != null) try { bmRiv.Dispose(); } catch { /* ignore */ }
            }
        }

        private static Bitmap GenerateLinesBitmap(Int32 width, Int32 height, Color color, float lineSize, Int32 lineOffsetW, Int32 lineOffsetH, Graphics g)
        {
            Bitmap bitmap = new Bitmap(width, height);
            int tripleWidth = width * 3;
            int tripleHeight = height * 3;
            bool hardLines = g != null && g.InterpolationMode == InterpolationMode.NearestNeighbor;
            int nrOfLines = 1;
            if (hardLines)
            {
                nrOfLines = Math.Max(1, (int)Math.Round(lineSize));
            }
            using (Bitmap img = new Bitmap(tripleWidth, tripleHeight))
            {
                using (Graphics gr = Graphics.FromImage(img))
                using (SolidBrush sb = new SolidBrush(color))
                using (Pen p = new Pen(sb, hardLines ? 1 : lineSize))
                {
                    if (g != null)
                    {
                        CopyRenderSettingsFrom(gr, g);
                    }
                    int offsetX = lineOffsetW;
                    int offsetY = lineOffsetH;
                    int hexWidth = width * 6;
                    int hexHeight = height * 6;
                    while (offsetX <= hexWidth && offsetY <= hexHeight)
                    {
                        // Paint lines
                        for (int i = 0; i < nrOfLines; i++)
                        {
                            gr.DrawLine(p, offsetX + i, 0, 0, offsetY + i);
                        }
                        offsetX += lineOffsetW;
                        offsetY += lineOffsetH;
                    }
                }
                using (Graphics gr = Graphics.FromImage(bitmap))
                {
                    if (g != null)
                    {
                        CopyRenderSettingsFrom(gr, g);
                    }
                    gr.DrawImage(img, new Rectangle(0, 0, width, height), width, height, width, height, GraphicsUnit.Pixel);
                    // Paint lines image on final image.
                }
            }
            return bitmap;
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

        public static void CopyRenderSettingsFrom(this Graphics target, Graphics source)
        {
            target.CompositingQuality = source.CompositingQuality;
            target.InterpolationMode = source.InterpolationMode;
            target.SmoothingMode = source.SmoothingMode;
            target.PixelOffsetMode = source.PixelOffsetMode;
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
            return new Rectangle((int)Math.Round(locX * scaleFactorX), (int)Math.Round(locY * scaleFactorY),
                Math.Max(1, (int)Math.Round(newSize.Width * scaleFactorX)), Math.Max(1, (int)Math.Round(newSize.Height * scaleFactorY)));
        }
    }
}
