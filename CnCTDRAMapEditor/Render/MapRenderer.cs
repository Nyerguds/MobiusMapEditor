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
using System.Text;

namespace MobiusEditor.Render
{
    public class RenderInfo
    {
        // paint position, paint action, true if flat, sub-position.

        // This works in 'classic pixels'; 1/24th of a cell.
        public Point RenderBasePoint;
        public Action<Graphics> RenderAction { get; private set; }
        public int ZOrder { get; private set; }
        public ITechno RenderedObject { get; private set; }
        public bool IsRendered { get; set; }

        public RenderInfo(Point renderPosition, Action<Graphics> paintAction, int zOrder, ITechno paintedObject)
        {
            this.RenderBasePoint = renderPosition;
            this.RenderAction = paintAction;
            this.ZOrder = zOrder;
            this.RenderedObject = paintedObject;
            this.IsRendered = false;
        }

        public RenderInfo(Point renderPosition, Action<Graphics> paintAction, ITechno paintedObject)
            :this(renderPosition, paintAction, Globals.ZOrderDefault, paintedObject)
        {
        }
    }

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
        /// Cosine table. Technically signed bytes, but stored as 00-FF for simplicity.
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
        /// Sine table. Technically signed bytes, but stored as 00-FF for simplicity.
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

        public static void Render(GameInfo gameInfo, Map map, Graphics graphics, ISet<Point> locations, MapLayerFlag layers, double tileScale, ShapeCacheManager cacheManager)
        {
            bool disposeCacheManager = false;
            if (cacheManager == null)
            {
                cacheManager = new ShapeCacheManager();
                disposeCacheManager = true;
            }
            // tileScale should always be given so it results in an exact integer tile size. Math.Round was added to account for .999 situations in the floats.
            Size tileSize = new Size(Math.Max(1, (int)Math.Round(Globals.OriginalTileWidth * tileScale)), Math.Max(1, (int)Math.Round(Globals.OriginalTileHeight * tileScale)));
            //Size tileSize = new Size(Math.Max(1, (int)(Globals.OriginalTileWidth * tileScale)), Math.Max(1, (int)(Globals.OriginalTileHeight * tileScale)));
            TheaterType theater = map.Theater;
            // paint position, paint action, true if flat, sub-position.
            List<RenderInfo> overlappingRenderList = new List<RenderInfo>();
            Func<IEnumerable<Point>> renderLocations = null;
            if (locations != null)
            {
                renderLocations = () => locations.OrderBy(p => p.Y * map.Metrics.Width + p.X);
            }
            else
            {
                IEnumerable<Point> allCells()
                {
                    for (int y = 0; y < map.Metrics.Height; ++y)
                    {
                        for (int x = 0; x < map.Metrics.Width; ++x)
                        {
                            yield return new Point(x, y);
                        }
                    }
                }
                renderLocations = allCells;
            }
            CompositingQuality backupCompositingQuality = graphics.CompositingQuality;
            InterpolationMode backupInterpolationMode = graphics.InterpolationMode;
            SmoothingMode backupSmoothingMode = graphics.SmoothingMode;
            PixelOffsetMode backupPixelOffsetMode = graphics.PixelOffsetMode;
            // Check if high-quality tile resizing is useful.
            SetRenderSettings(graphics, false);
            bool isSmooth = backupCompositingQuality != graphics.CompositingQuality ||
                            backupInterpolationMode != graphics.InterpolationMode ||
                            backupSmoothingMode != graphics.SmoothingMode ||
                            backupPixelOffsetMode != graphics.PixelOffsetMode;
            // No need to restore the settings; the high quality tile resizing makes all tiles the
            // required size, and if isSmooth is false, the settings were already on pixel resize.
            if ((layers & MapLayerFlag.Template) != MapLayerFlag.None)
            {
                TemplateType clear = map.TemplateTypes.Where(t => t.Flag == TemplateTypeFlag.Clear).FirstOrDefault();
                foreach (Point topLeft in renderLocations())
                {
                    Template template = map.Templates[topLeft];
                    TemplateType ttype = template?.Type ?? clear;
                    string name = ttype.Name;
                    // For clear terrain, calculate icon from 0-15 using map position.
                    int icon = template?.Icon ?? ((topLeft.X & 0x03) | ((topLeft.Y) & 0x03) << 2);
                    // If something is actually placed on the map, show it, even if it has no graphics.
                    string tileName = "template_" + name + "_" + icon.ToString("D4") + "_" + tileSize.Width + "x" + tileSize.Height + (isSmooth ? "_smooth" : String.Empty);
                    Bitmap tileImg = cacheManager.GetImage(tileName);
                    Rectangle renderBounds = new Rectangle(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height, tileSize.Width, tileSize.Height);
                    if (tileImg == null)
                    {
                        bool success = Globals.TheTilesetManager.GetTileData(name, icon, out Tile tile, true, false);
                        if (tile != null && tile.Image != null)
                        {
                            Bitmap tileImage = tile.Image;
                            if (!isSmooth || (tileSize.Width == tileImage.Width && tileSize.Height == tileImage.Height))
                            {
                                tileImg = tileImage.RemoveAlpha();
                            }
                            else
                            {
                                // Results in a new image that is scaled to the correct size, without edge artifacts.
                                Bitmap scaledImage = tileImage.HighQualityScale(tileSize.Width, tileSize.Height,
                                        backupCompositingQuality, backupInterpolationMode, backupSmoothingMode, backupPixelOffsetMode);
                                scaledImage.RemoveAlphaOnCurrent();
                                tileImg = scaledImage;
                            }
                            cacheManager.AddImage(tileName, tileImg);
                            
                        }
                    }
                    if (tileImg != null)
                    {
                        graphics.DrawImage(tileImg, renderBounds);
                    }
                    else
                    {
                        Debug.Print(string.Format("Template {0} ({1}) could not be rendered.", name, icon));
                    }
                }
            }
            // Since high-quality scaling is now done on the tiles themselves, the actual map tile painting is done
            // with pixel interpolation mode because it is faster, and the tiles are already correctly sized anyway.
            // So now, restore the actual requested settings.
            graphics.CompositingQuality = backupCompositingQuality;
            graphics.InterpolationMode = backupInterpolationMode;
            graphics.SmoothingMode = backupSmoothingMode;
            graphics.PixelOffsetMode = backupPixelOffsetMode;
            // Attached bibs are counted under Buildings, not Smudge.
            if ((layers & MapLayerFlag.Buildings) != MapLayerFlag.None)
            {
                foreach (Point topLeft in renderLocations())
                {
                    Smudge smudge = map.Smudge[topLeft];
                    // Don't render bibs in theaters which don't contain them.
                    if (smudge != null && smudge.Type.IsAutoBib && (!Globals.FilterTheaterObjects || smudge.Type.ExistsInTheater))
                    {
                        RenderSmudge(topLeft, tileSize, tileScale, smudge, isSmooth, cacheManager).Item2(graphics);
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
                        RenderSmudge(topLeft, tileSize, tileScale, smudge, isSmooth, cacheManager).Item2(graphics);
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
                        RenderOverlay(gameInfo, location, map.Bounds, tileSize, tileScale, overlay, false).Item2(graphics);
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
                    overlappingRenderList.Add(RenderBuilding(gameInfo, map, topLeft, tileSize, tileScale, building, false));
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
                    foreach (InfantryStoppingType ist in InfantryGroup.RenderOrder)
                    {
                        Infantry infantry = infantryGroup.Infantry[(int)ist];
                        if (infantry == null)
                        {
                            continue;
                        }
                        overlappingRenderList.Add(RenderInfantry(topLeft, tileSize, infantry, ist, false));
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
                    overlappingRenderList.Add(RenderUnit(gameInfo, topLeft, tileSize, unit, false));
                }
            }
            if ((layers & MapLayerFlag.Terrain) != MapLayerFlag.None)
            {
                foreach ((Point topLeft, Terrain terrain) in map.Technos.OfType<Terrain>())
                {
                    if ((locations != null) && !locations.Contains(topLeft))
                    {
                        continue;
                    }
                    overlappingRenderList.Add(RenderTerrain(topLeft, tileSize, tileScale, terrain, false));
                }
            }
            // Paint all the rest
            List<RenderInfo> validRenders = overlappingRenderList.Where(obj => obj.RenderedObject != null).ToList();
            int paintOrder = 0;
            foreach (RenderInfo info in validRenders.OrderBy(obj => obj.ZOrder).ThenBy(obj => obj.RenderBasePoint.Y).ThenByDescending(obj => obj.RenderBasePoint.X))
            {
                info.RenderAction(graphics);
                info.RenderedObject.DrawOrderCache = paintOrder++;
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
                    RenderOverlay(gameInfo, topLeft, map.Bounds, tileSize, tileScale, overlay, false).Item2(graphics);
                }
            }

            if ((layers & MapLayerFlag.Waypoints) != MapLayerFlag.None)
            {
                // todo avoid overlapping waypoints of the same type?
                Dictionary<int, int> flagOverlapPoints = new Dictionary<int, int>();
                HashSet<int> handledPoints = new HashSet<int>();
                ITeamColor[] flagColors = map.FlagColors;
                bool soloMission = map.BasicSection.SoloMission;
                bool previewIsFlag = map.Waypoints.Where(w => w.IsPreview && Waypoint.GetMpIdFromFlag(w.Flag) != -1).Any();
                int lastFlag = -1;
                float wpAlpha = 0.5f;
                if (!soloMission)
                {
                    int firstFlag = -1;
                    for (int i = 0; i < map.Waypoints.Length; i++)
                    {
                        Waypoint waypoint = map.Waypoints[i];
                        if (waypoint.IsPreview)
                        {
                            continue;
                        }
                        int mpId = Waypoint.GetMpIdFromFlag(map.Waypoints[i].Flag);
                        if (mpId != -1)
                        {
                            if (firstFlag == -1)
                            {
                                firstFlag = i;
                            }
                            lastFlag = i;
                        }
                    }
                    // This logic is kind of dirty; it relies on all flag points being in consecutive order. But without that, the preview logic doesn't work.
                    for (int i = 0; i < firstFlag; i++)
                    {
                        Waypoint waypoint = map.Waypoints[i];
                        if (!waypoint.Point.HasValue || (locations != null && !locations.Contains(waypoint.Point.Value))
                            || !map.Metrics.GetCell(waypoint.Point.Value, out int cell) || handledPoints.Contains(cell))
                        {
                            continue;
                        }
                        handledPoints.Add(cell);
                        RenderWaypoint(gameInfo, soloMission, tileSize, flagColors, waypoint, wpAlpha, 0).Item2(graphics);
                    }
                    RenderWaypointFlags(graphics, gameInfo, map, map.Metrics.Bounds, tileSize);
                }
                for (int i = lastFlag + 1; i < map.Waypoints.Length; i++)
                {
                    Waypoint waypoint = map.Waypoints[i];
                    if (!waypoint.Point.HasValue || (locations != null && !locations.Contains(waypoint.Point.Value))
                        || !map.Metrics.GetCell(waypoint.Point.Value, out int cell) || handledPoints.Contains(cell))
                    {
                        continue;
                    }
                    handledPoints.Add(cell);
                    RenderWaypoint(gameInfo, soloMission, tileSize, flagColors, waypoint, wpAlpha, 0).Item2(graphics);
                }
            }
            if (disposeCacheManager)
            {
                cacheManager.Reset();
            }
        }

        public static void Render(GameInfo gameInfo, Map map, Graphics graphics, ISet<Point> locations, MapLayerFlag layers)
        {
            Render(gameInfo, map, graphics, locations, layers, Globals.MapTileScale, Globals.TheShapeCacheManager);
        }

        public static (Rectangle, Action<Graphics>) RenderSmudge(Point topLeft, Size tileSize, double tileScale, Smudge smudge, bool isSmoothRendering, ShapeCacheManager cacheManager)
        {
            if (Globals.FilterTheaterObjects && !smudge.Type.ExistsInTheater)
            {
                Debug.Print(string.Format("Smudge {0} ({1}) not available in this theater.", smudge.Type.Name, smudge.Icon));
                return (Rectangle.Empty, (g) => { });
            }
            float alphaFactor = 1.0f;
            Building bld = smudge.AttachedTo;
            if (bld != null)
            {
                if (bld.IsPreview)
                {
                    alphaFactor = Globals.PreviewAlphaFloat;
                }
                if (!bld.IsPrebuilt)
                {
                    alphaFactor *= Globals.UnbuiltAlphaFloat;
                }
            }
            else if (smudge.IsPreview)
            {
                alphaFactor = Globals.PreviewAlphaFloat;
            }
            alphaFactor = alphaFactor.Restrict(0, 1);
            string tileName = "smudge_" + smudge.Type.Name + "_" + smudge.Icon.ToString("D4") + "_" + tileSize.Width + "x" + tileSize.Height + (isSmoothRendering ? "_smooth" : String.Empty);
            Bitmap tileImg = cacheManager.GetImage(tileName);
            if (tileImg == null)
            {
                bool success = Globals.TheTilesetManager.GetTileData(smudge.Type.Name, smudge.Icon, out Tile tile, true, false);
                if (tile != null && tile.Image != null)
                {
                    Bitmap tileImage = tile.Image;
                    // Check if high-quality tile resizing is useful.
                    if (!isSmoothRendering || (tileSize.Width == tileImage.Width && tileSize.Height == tileImage.Height))
                    {
                        tileImg = new Bitmap(tileSize.Width, tileSize.Height);
                        Rectangle smudgeBounds = RenderBounds(tile.Image.Size, new Size(1, 1), tileScale);
                        using (Graphics g = Graphics.FromImage(tileImg))
                        {
                            SetRenderSettings(g, isSmoothRendering);
                            g.DrawImage(tileImage, smudgeBounds, new Rectangle(0, 0, tileImage.Width, tileImage.Height), GraphicsUnit.Pixel);
                        }
                    }
                    else
                    {
                        // Results in a new image that is scaled to the correct size, without edge artifacts.
                        tileImg = tileImage.HighQualityScale(tileSize.Width, tileSize.Height);
                    }
                    cacheManager.AddImage(tileName, tileImg);
                    
                }
            }
            if (tileImg != null)
            {
                Rectangle smudgeBounds = new Rectangle(
                    (tileSize.Width - tileImg.Width) / 2,
                    (tileSize.Height - tileImg.Height) / 2,
                    tileImg.Width, tileImg.Height);
                smudgeBounds.X += topLeft.X * tileSize.Width;
                smudgeBounds.Y += topLeft.Y * tileSize.Height;
                void render(Graphics g)
                {
                    using (ImageAttributes imageAttributes = new ImageAttributes())
                    {
                        imageAttributes.SetColorMatrix(GetColorMatrix(Color.White, 1.0f, alphaFactor));
                        g.DrawImage(tileImg, smudgeBounds, 0, 0, tileImg.Width, tileImg.Height, GraphicsUnit.Pixel, imageAttributes);
                    }
                }
                return (smudgeBounds, render);
            }
            else
            {
                Debug.Print(string.Format("Smudge {0} ({1}) not found", smudge.Type.Name, smudge.Icon));
                return (Rectangle.Empty, (g) => { });
            }
        }

        public static (Rectangle, Action<Graphics>) RenderOverlay(GameInfo gameInfo, Point topLeft, Rectangle? mapBounds, Size tileSize, double tileScale, Overlay overlay, bool fullOpaque)
        {
            OverlayType ovtype = overlay.Type;
            string name = ovtype.GraphicsSource;
            int icon = ovtype.IsConcrete || ovtype.IsResource || ovtype.IsWall || ovtype.ForceTileNr == -1 ? overlay.Icon : ovtype.ForceTileNr;
            bool isTeleport = gameInfo != null && gameInfo.GameType == GameType.SoleSurvivor && ovtype == SoleSurvivor.OverlayTypes.Teleport && Globals.AdjustSoleTeleports;
            bool success = Globals.TheTilesetManager.GetTileData(name, icon, out Tile tile, true, false);
            if (tile != null && tile.Image != null)
            {
                int actualTopLeftX = topLeft.X * tileSize.Width;
                int actualTopLeftY = topLeft.Y * tileSize.Height;
                Rectangle overlayBounds = RenderBounds(tile.Image.Size, new Size(1, 1), tileScale);
                overlayBounds.X += actualTopLeftX;
                overlayBounds.Y += actualTopLeftY;
                float alphaFactor = 1.0f;
                if (!fullOpaque && overlay.IsPreview)
                {
                    alphaFactor = Globals.PreviewAlphaFloat;
                }
                Color tint = Color.White;
                if (overlay.Type.IsResource && mapBounds.HasValue && !mapBounds.Value.Contains(topLeft))
                {
                    tint = Color.FromArgb(0xFF, 0x80, 0x80);
                    // Technically the multiplication isn't needed; resources have no preview state in the editor.
                    if (!fullOpaque)
                    {
                        alphaFactor *= 0.7f;
                    }
                }
                alphaFactor = alphaFactor.Restrict(0, 1);
                void render(Graphics g)
                {
                    using (ImageAttributes imageAttributes = new ImageAttributes())
                    {
                        imageAttributes.SetColorMatrix(GetColorMatrix(tint, 1.0f, alphaFactor));
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
                }
                return (overlayBounds, render);
            }
            else
            {
                Debug.Print(string.Format("Overlay {0} ({1}) not found", name, icon));
                return (Rectangle.Empty, (g) => { });
            }
        }

        public static RenderInfo RenderTerrain(Point topLeft, Size tileSize, double tileScale, Terrain terrain, bool fullOpaque)
        {
            TerrainType type = terrain.Type;
            string tileName = type.GraphicsSource;
            bool succeeded = Globals.TheTilesetManager.GetTileData(tileName, type.DisplayIcon, out Tile tile, true, false);
            if (!succeeded && !string.Equals(type.GraphicsSource, type.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                succeeded = Globals.TheTilesetManager.GetTileData(type.Name, type.DisplayIcon, out tile, true, false);
            }
            float alphaFactor = 1.0f;
            if (!fullOpaque)
            {
                if (terrain.IsPreview)
                {
                    alphaFactor *= Globals.PreviewAlphaFloat;
                }
                alphaFactor = alphaFactor.Restrict(0, 1);
            }
            Size terrTSize = type.Size;
            Size tileISize = tile.Image.Size;
            Point location = new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height);
            Size maxSize = new Size(terrTSize.Width * tileSize.Width, terrTSize.Height * tileSize.Height);
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
            void render(Graphics g)
            {
                using (ImageAttributes imageAttributes = new ImageAttributes())
                {
                    imageAttributes.SetColorMatrix(GetColorMatrix(Color.White, 1.0f, alphaFactor));
                    g.DrawImage(tile.Image, paintBounds, 0, 0, tile.Image.Width, tile.Image.Height, GraphicsUnit.Pixel, imageAttributes);
                }
            }
            Point centerPoint = GetTerrainRenderPoint(terrain);
            Point usedCenter = new Point(topLeft.X * Globals.PixelWidth + centerPoint.X, topLeft.Y * Globals.PixelHeight + centerPoint.Y);
            return new RenderInfo(usedCenter, render, terrain);
        }

        public static RenderInfo RenderBuilding(GameInfo gameInfo, Map map, Point topLeft, Size tileSize, double tileScale, Building building, bool fullOpaque)
        {
            float alphaFactor = 1.0f;
            if (!fullOpaque)
            {
                if (building.IsPreview)
                {
                    alphaFactor *= Globals.PreviewAlphaFloat;
                }
                if (!building.IsPrebuilt)
                {
                    alphaFactor *= Globals.UnbuiltAlphaFloat;
                }
                alphaFactor = alphaFactor.Restrict(0, 1);
            }
            int icon = building.Type.FrameOffset;
            int maxIcon = 0;
            int damageIconOffs = 0;
            int collapseIcon = 0;
            // In TD, damage is when BELOW the threshold. In RA, it's ON the threshold.
            int healthyMin = gameInfo.HitPointsGreenMinimum;
            bool isDamaged = building.Strength <= healthyMin;
            bool hasCollapseFrame = false;
            // Only fetch if damaged. BuildingType.IsSingleFrame is an override for the RA mines. Everything else works with one simple logic.
            if (isDamaged && !building.Type.IsSingleFrame && !building.Type.IsWall)
            {
                maxIcon = Globals.TheTilesetManager.GetTileDataLength(building.Type.GraphicsSource);
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
            else if (building.Type.IsWall)
            {
                icon += GetBuildingOverlayIcon(map, topLeft, building);
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
            Point location = new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height);
            Size maxSize = new Size(building.Type.Size.Width * tileSize.Width, building.Type.Size.Height * tileSize.Height);

            Size bldTSize = building.Type.Size;
            Size tileISize = tile.Image.Size;
            Rectangle paintBounds;
            if (!succeeded)
            {
                // Stretch dummy graphics over the whole size.
                paintBounds = new Rectangle(0, 0, maxSize.Width, maxSize.Height);
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
                using (ImageAttributes imageAttributes = new ImageAttributes())
                {
                    imageAttributes.SetColorMatrix(GetColorMatrix(Color.White, 1.0f, alphaFactor));
                    if (factoryOverlayTile != null)
                    {
                        // Avoid factory overlay showing as semitransparent.
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
            }
            Point centerPoint = GetBuildingRenderPoint(building);
            Point usedCenter = new Point(topLeft.X * Globals.PixelWidth + centerPoint.X, topLeft.Y * Globals.PixelHeight + centerPoint.Y);
            // "Z-Order" is for sorting buildings as floor level (0), flat on the floor (5), or sticking out of the floor (default; 10).
            // It determines whether pieces on unoccupied cells should overlap objects on these cells or be drawn below them.
            return new RenderInfo(usedCenter, render, building.Type.ZOrder, building);
        }

        private static int GetBuildingOverlayIcon(Map map, Point topLeft, Building building)
        {
            if (!building.Type.IsWall || map == null)
            {
                return 0;
            }
            BuildingType bt = building.Type;

            bool hasNorthWall = (map.Metrics.Adjacent(topLeft, FacingType.North, out Point north) ? map.Buildings[north] as Building : null)?.Type == bt;
            bool hasEastWall = (map.Metrics.Adjacent(topLeft, FacingType.East, out Point east) ? map.Buildings[east] as Building : null)?.Type == bt;
            bool hasSouthWall = (map.Metrics.Adjacent(topLeft, FacingType.South, out Point south) ? map.Buildings[south] as Building : null)?.Type == bt;
            bool hasWestWall = (map.Metrics.Adjacent(topLeft, FacingType.West, out Point west) ? map.Buildings[west] as Building : null)?.Type == bt;

            string btName = bt.Name;
            hasNorthWall |= map.Overlay.Adjacent(topLeft, FacingType.North)?.Type.Name == btName;
            hasEastWall |= map.Overlay.Adjacent(topLeft, FacingType.East)?.Type.Name == btName;
            hasSouthWall |= map.Overlay.Adjacent(topLeft, FacingType.South)?.Type.Name == btName;
            hasWestWall |= map.Overlay.Adjacent(topLeft, FacingType.West)?.Type.Name == btName;
            int icon = 0;
            if (hasNorthWall)
            {
                icon |= 1;
            }
            if (hasEastWall)
            {
                icon |= 2;
            }
            if (hasSouthWall)
            {
                icon |= 4;
            }
            if (hasWestWall)
            {
                icon |= 8;
            }
            return icon;
        }

        public static RenderInfo RenderInfantry(Point topLeft, Size tileSize, Infantry infantry, InfantryStoppingType infantryStoppingType, bool fullOpaque)
        {
            int icon = HumanShape[Facing32[infantry.Direction.ID]];
            ITeamColor teamColor = infantry.Type.CanRemap ? Globals.TheTeamColorManager[infantry.House?.UnitTeamColor] : null;
            Tile tile = null;
            // InfantryType.Init() should have taken care of RA's classic civilian remap mess at this point, and remapped all cached source graphics.
            bool success = Globals.TheTilesetManager.GetTeamColorTileData(infantry.Type.GraphicsSource, icon, teamColor, out tile, true, false);
            if (tile == null || tile.Image == null)
            {
                Debug.Print(string.Format("Infantry {0} graphics ({1}, frame {2}) not found", infantry.Type.Name, infantry.Type.GraphicsSource, icon));
                return new RenderInfo(Point.Empty, (g) => { }, infantry);
            }
            Size imSize = tile.Image.Size;
            Point origLocation = new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height);
            Point renderLocation = origLocation;
            Point offset;
            // Offset is calculated as "pixels" in the old 24-pixel cells.
            Size renderSize;
            if (success)
            {
                // Actual graphics: get tweaked positions accounting for the shape of the infantry and the fact the theoretical positions are at their feet.
                offset = GetInfantryRenderPoint(infantryStoppingType);
                Point offsetActual = new Point(offset.X * tileSize.Width / Globals.PixelWidth, offset.Y * tileSize.Height / Globals.PixelHeight);
                renderLocation.Offset(offsetActual);
                renderSize = new Size(imSize.Width * tileSize.Width / Globals.OriginalTileWidth, imSize.Height * tileSize.Height / Globals.OriginalTileHeight);
            }
            else
            {
                // Dummy graphics: use theoretical positions.
                offset = InfantryGroup.RenderPosition(infantryStoppingType, false);
                Point offsetActual = new Point(offset.X * tileSize.Width / Globals.PixelWidth, offset.Y * tileSize.Height / Globals.PixelHeight);
                renderLocation.Offset(offsetActual);
                renderSize = new Size(imSize.Width * tileSize.Width / Globals.OriginalTileWidth, imSize.Height * tileSize.Height / Globals.OriginalTileHeight);
                renderSize.Width /= 3;
                renderSize.Height /= 2;
            }
            Rectangle renderBounds = new Rectangle(renderLocation - new Size(renderSize.Width / 2, renderSize.Height / 2), renderSize);
            float alphaFactor = 1.0f;
            if (!fullOpaque)
            {
                if (infantry.IsPreview)
                {
                    alphaFactor *= Globals.PreviewAlphaFloat;
                }
                alphaFactor = alphaFactor.Restrict(0, 1);
            }
            void render(Graphics g)
            {
                using (ImageAttributes imageAttributes = new ImageAttributes())
                {
                    imageAttributes.SetColorMatrix(GetColorMatrix(Color.White, 1.0f, alphaFactor));
                    g.DrawImage(tile.Image, renderBounds, 0, 0, tile.Image.Width, tile.Image.Height, GraphicsUnit.Pixel, imageAttributes);
                    // Test code to visualise original 5-point die face location (green), and corrected infantry base point (feet) location (red).
                    /*/
                    Size pixel = new Size(tileSize.Width / Globals.PixelWidth, tileSize.Height / Globals.PixelHeight);
                    using (SolidBrush sb = new SolidBrush(Color.Red))
                    {
                        g.FillRectangle(sb, new Rectangle(renderLocation, pixel));
                    }
                    using (SolidBrush sb = new SolidBrush(Color.LimeGreen))
                    {
                        g.FillRectangle(sb, new Rectangle(new Point(
                            origLocation.X + offsetBare.X * tileSize.Width / Globals.PixelWidth,
                            origLocation.Y + (offsetBare.Y * tileSize.Height / Globals.PixelHeight)), pixel));
                    }
                    //*/
                }
            }
            // Render position is the feet point, adjusted to 24-pixel cell location.
            return new RenderInfo(new Point(topLeft.X * Globals.PixelWidth + offset.X, topLeft.Y * Globals.PixelHeight + offset.Y), render, infantry);
        }

        public static RenderInfo RenderUnit(GameInfo gameInfo, Point topLeft, Size tileSize, Unit unit, bool fullOpaque)
        {
            int icon = 0;
            int bodyFrames = 0;
            // In TD, damage is when BELOW the threshold. In RA, it's ON the threshold.
            int healthyMin = gameInfo.HitPointsGreenMinimum;
            int damagedMin = gameInfo.HitPointsYellowMinimum;
            FrameUsage frameUsage = unit.Type.BodyFrameUsage;
            if (frameUsage.HasFlag(FrameUsage.Frames01Single))
            {
                icon = 0;
                // Not actually determined, but whatever. Single frame units generally have no turret.
                bodyFrames = 1;
            }
            else if (frameUsage.HasFlag(FrameUsage.Frames08Cardinal))
            {
                icon = ((BodyShape[Facing32[unit.Direction.ID]] + 2) / 4) & 0x07;
                bodyFrames = 8;
            }
            else if (frameUsage.HasFlag(FrameUsage.Frames16Simple))
            {
                icon = BodyShape[Facing16[unit.Direction.ID] * 2] / 2;
                bodyFrames = 16;
            }
            else if (frameUsage.HasFlag(FrameUsage.Frames16Symmetrical))
            {
                // Special case for 16-frame rotation saved as 8-frame because it is symmetrical and thus the second half of the frames is the same.
                icon = (BodyShape[Facing32[unit.Direction.ID]] / 2) & 0x07;
                bodyFrames = 8;
            }
            else if (frameUsage.HasFlag(FrameUsage.Frames32Full) || !frameUsage.HasAnyFlags(FrameUsage.FrameUsages))
            {
                icon = BodyShape[Facing32[unit.Direction.ID]];
                bodyFrames = 32;
            }
            // Special logic for TD gunboat's damaged states.
            // East facing is not actually possible to set in missions. This is just the turret facing.
            if (frameUsage.HasFlag(FrameUsage.DamageStates))
            {
                if (unit.Strength <= healthyMin)
                    icon += bodyFrames;
                if (unit.Strength <= damagedMin)
                    icon += bodyFrames;
                // Skip three-step damaged frames. In practice this will just go to the east-facing ones though.
                bodyFrames *= 3;
            }
            // Special logic for APC-types with unload frames.
            if ((frameUsage & FrameUsage.HasUnloadFrames) != FrameUsage.None)
            {
                if (unit.Type.IsAircraft)
                {
                    // Transport heli unload has 4 frames
                    bodyFrames += 4;
                }
                else if (unit.Type.IsVessel)
                {
                    // Boat unload has 4 frames
                    bodyFrames += 4;
                }
                else
                {
                    // APC unload has 6 frames.
                    bodyFrames += 6;
                }
            }
            // Get House color.
            ITeamColor teamColor = null;
            if (unit.House != null && unit.Type.CanRemap)
            {
                string teamColorName;
                if (!unit.House.OverrideTeamColors.TryGetValue(unit.Type.Name, out teamColorName))
                {
                    teamColorName = unit.House?.UnitTeamColor;
                }
                teamColor = Globals.TheTeamColorManager[teamColorName];
            }
            // Get body frame
            Globals.TheTilesetManager.GetTeamColorTileData(unit.Type.Name, icon, teamColor, out Tile tile, true, false);
            if (tile == null || tile.Image == null)
            {
                Debug.Print(string.Format("Unit {0} ({1}) not found", unit.Type.Name, icon));
                return new RenderInfo(Point.Empty, (g) => { }, null);
            }
            Size imSize = tile.Image.Size;
            Point location =
                new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height) +
                new Size(tileSize.Width / 2, tileSize.Height / 2);
            Size renderSize = new Size(imSize.Width * tileSize.Width / Globals.OriginalTileWidth, imSize.Height * tileSize.Height / Globals.OriginalTileHeight);
            Rectangle renderRect = new Rectangle(new Point(0, 0), renderSize);
            Rectangle renderBounds = new Rectangle(location - new Size(renderSize.Width / 2, renderSize.Height / 2), renderSize);
            // Turret handling
            Tile turretTile = null;
            Tile turret2Tile = null;
            Point turretAdjust = Point.Empty;
            Point turret2Adjust = Point.Empty;
            if (unit.Type.HasTurret)
            {
                FrameUsage turrUsage = unit.Type.TurretFrameUsage;
                string turretName = unit.Type.Turret ?? unit.Type.Name;
                string turret2Name = unit.Type.HasDoubleTurret ? unit.Type.SecondTurret ?? unit.Type.Turret ?? unit.Type.Name : null;
                int turret1Icon = 0;
                int turret2Icon = 0;
                if ((turrUsage & FrameUsage.Frames01Single) != FrameUsage.None)
                {
                    turret1Icon = 0;
                    turret2Icon = 0;
                }
                else if ((turrUsage & FrameUsage.Frames08Cardinal) != FrameUsage.None)
                {
                    // Never used for a turret, but whatever.
                    turret1Icon = ((BodyShape[Facing32[unit.Direction.ID]] + 2) / 4) & 0x07;
                    turret2Icon = turret1Icon;
                }
                else if ((turrUsage & FrameUsage.Frames16Simple) != FrameUsage.None)
                {
                    turret1Icon = BodyShape[Facing16[unit.Direction.ID] * 2] / 2;
                    turret2Icon = turret1Icon;
                }
                else if ((turrUsage & FrameUsage.Frames16Symmetrical) != FrameUsage.None)
                {
                    // Special case for 16-frame rotation saved as 8-frame because it is symmetrical and thus the second half of the frames is the same (MGG)
                    turret1Icon = (BodyShape[Facing32[unit.Direction.ID]] / 2) & 7;
                    turret2Icon = turret1Icon;
                }
                else if ((turrUsage & FrameUsage.Frames32Full) != FrameUsage.None)
                {
                    turret1Icon = BodyShape[Facing32[unit.Direction.ID]];
                    turret2Icon = turret1Icon;
                }
                else if ((turrUsage & FrameUsage.Rotor) != FrameUsage.None)
                {
                    turret1Icon = (unit.Direction.ID >> 5) % 2 == 1 ? 9 : 5;
                    turret2Icon = (unit.Direction.ID >> 5) % 2 == 1 ? 8 : 4;
                }
                // If same as body name, add body frames.
                turret1Icon = unit.Type.Name.Equals(turretName, StringComparison.OrdinalIgnoreCase) ? bodyFrames + turret1Icon : turret1Icon;
                turret2Icon = unit.Type.Name.Equals(turret2Name, StringComparison.OrdinalIgnoreCase) ? bodyFrames + turret2Icon : turret2Icon;
                if (turretName != null)
                    Globals.TheTilesetManager.GetTeamColorTileData(turretName, turret1Icon, teamColor, out turretTile, false, false);
                if (turret2Name != null)
                    Globals.TheTilesetManager.GetTeamColorTileData(turret2Name, turret2Icon, teamColor, out turret2Tile, false, false);
                // Flatbed is a special case; if it is used, TurretOffset is ignored.
                if (turrUsage.HasFlag(FrameUsage.OnFlatBed))
                {
                    // OnFlatBed indicates the turret oFfset is determined by the BackTurretAdjust table.
                    turretAdjust = BackTurretAdjust[Facing32[unit.Direction.ID]];
                    // Never actually used for 2 turrets. Put second turret in the front?
                    turret2Adjust = BackTurretAdjust[Facing32[(byte)((unit.Direction.ID + DirectionTypes.South.ID) & 0xFF)]];
                }
                else if (unit.Type.TurretOffset != 0)
                {
                    // Used by ships and by the transport helicopter.
                    int distance = unit.Type.TurretOffset;
                    int face = (unit.Direction.ID >> 5) & 7;
                    if (turrUsage.HasFlag(FrameUsage.Rotor))
                    {
                        // Rotor stretch distance is given by a table.
                        distance *= HeliDistanceAdjust[face];
                    }
                    int x = 0;
                    int y = 0;
                    // For vessels, perspective stretch is simply done as '/ 2'.
                    int perspectiveDivide = unit.Type.IsVessel ? 2 : 1;
                    MovePoint(ref x, ref y, unit.Direction.ID, distance, perspectiveDivide);
                    turretAdjust.X += x;
                    turretAdjust.Y += y;
                    if (unit.Type.HasDoubleTurret)
                    {
                        x = 0;
                        y = 0;
                        MovePoint(ref x, ref y, (byte)((unit.Direction.ID + DirectionTypes.South.ID) & 0xFF), distance, perspectiveDivide);
                        turret2Adjust.X += x;
                        turret2Adjust.Y += y;
                    }
                }
                // Adjust Y-offset.
                turretAdjust.Y += unit.Type.TurretY;
                turret2Adjust.Y += unit.Type.TurretY;
            }
            float alphaFactor = 1.0f;
            if (!fullOpaque)
            {
                if (unit.IsPreview)
                {
                    alphaFactor *= Globals.PreviewAlphaFloat;
                }
                alphaFactor = alphaFactor.Restrict(0, 1);
            }
            void render(Graphics g)
            {
                using (ImageAttributes imageAttributes = new ImageAttributes())
                {
                    imageAttributes.SetColorMatrix(GetColorMatrix(Color.White, 1.0f, alphaFactor));
                    // Combine body and turret to one image, then paint it. This is done because it might be semitransparent.
                    using (Bitmap unitBm = new Bitmap(renderBounds.Width, renderBounds.Height))
                    {
                        unitBm.SetResolution(96, 96);
                        using (Graphics unitG = Graphics.FromImage(unitBm))
                        {
                            unitG.CopyRenderSettingsFrom(g);
                            if (tile != null)
                            {
                                unitG.DrawImage(tile.Image, renderRect, 0, 0, tile.Image.Width, tile.Image.Height, GraphicsUnit.Pixel);
                            }
                            if (unit.Type.HasTurret)
                            {
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
                        g.DrawImage(unitBm, renderBounds, 0, 0, renderBounds.Width, renderBounds.Height, GraphicsUnit.Pixel, imageAttributes);
                    }
                }
            }
            Point centerPoint = GetVehicleRenderPoint();
            Point usedCenter = new Point(topLeft.X * Globals.PixelWidth + centerPoint.X, topLeft.Y * Globals.PixelHeight + centerPoint.Y);
            return new RenderInfo(usedCenter, render, unit);
        }

        public static (Rectangle, Action<Graphics>) RenderWaypoint(GameInfo gameInfo, bool soloMission, Size tileSize, ITeamColor[] flagColors, Waypoint waypoint, float alphaFactor, int offset)
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
            if (waypoint.IsPreview)
            {
                alphaFactor *= Globals.PreviewAlphaFloat;
            }
            alphaFactor = alphaFactor.Restrict(0, 1);
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
                //transparencyModifier = 1.0f;
                gotTile = Globals.TheTilesetManager.GetTeamColorTileData(tileGraphics, icon, teamColor, out tile);
            }
            else if (gameInfo.GameType == GameType.SoleSurvivor && waypoint.Flag.HasFlag(WaypointFlag.CrateSpawn))
            {
                isDefaultIcon = false;
                tileGraphics = "scrate";
                icon = 0;
                sizeMultiplier = 2;
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
                gotTile = Globals.TheTilesetManager.GetTileData(tileGraphics, icon, out tile);
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
            // Apply offset
            int actualOffsetX = offset * tileSize.Width / Globals.PixelWidth;
            int actualOffsetY = offset * tileSize.Height / Globals.PixelHeight;
            renderBounds.X += actualOffsetX;
            renderBounds.Y += actualOffsetY;
            renderBounds.Width = Math.Max(0, renderBounds.Width - actualOffsetX);
            renderBounds.Height = Math.Max(0, renderBounds.Height - actualOffsetY);
            // Optional: crop the image. If not, it scales, which also looks okay
            //int imageOffsetX = (int)(tile.Image.Width * sizeMultiplier * offset / Globals.PixelWidth);
            //int imageOffsetY = (int)(tile.Image.Height * sizeMultiplier * offset / Globals.PixelWidth);
            //imgBounds.Width = Math.Max(0, imgBounds.Width - imageOffsetX);
            //imgBounds.Height = Math.Max(0, imgBounds.Height - imageOffsetY);

            void render(Graphics g)
            {
                using (ImageAttributes imageAttributes = new ImageAttributes())
                {
                    imageAttributes.SetColorMatrix(GetColorMatrix(Color.White, 1.0f, alphaFactor));
                    if (renderBounds.Width > 0 && renderBounds.Height > 0 && imgBounds.Width > 0 && imgBounds.Height > 0)
                    {
                        g.DrawImage(tile.Image, renderBounds, imgBounds.X, imgBounds.Y, imgBounds.Width, imgBounds.Height, GraphicsUnit.Pixel, imageAttributes);
                    }
                }
            }
            return (renderBounds, render);
        }

        private static Point GetRenderPoint(Object obj, InfantryStoppingType? ist)
        {
            if (obj is Building building)
            {
                return GetBuildingRenderPoint(building);
            }
            else if (obj is Terrain terrain)
            {
                return GetTerrainRenderPoint(terrain);
            }
            else if (obj is InfantryGroup && ist.HasValue)
            {
                return GetInfantryRenderPoint(ist.Value);
            }
            else
            {
                return GetVehicleRenderPoint();
            }
        }

        private static Point GetVehicleRenderPoint()
        {
            return new Point(Globals.PixelWidth / 2, Globals.PixelHeight / 2);
        }

        private static Point GetInfantryRenderPoint(InfantryStoppingType ist)
        {
            return InfantryGroup.RenderPosition(ist, true);
        }

        private static Point GetTerrainRenderPoint(Terrain terrain)
        {
            return terrain.Type.CenterPoint;
        }

        private static Point GetBuildingRenderPoint(Building building)
        {
            return GeneralUtils.GetOccupiedCenter(building.Type.BaseOccupyMask, new Size(Globals.PixelWidth, Globals.PixelHeight));
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
                foreach (int cell in renderList)
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

        public static void RenderAllOccupierBoundsGreen<T>(Graphics graphics, Rectangle visibleCells, Size tileSize, IEnumerable<(Point, T)> occupiers) where T : ICellOccupier, ICellOverlapper
        {
            RenderAllOccupierBounds(graphics, visibleCells, tileSize, occupiers, Color.Green, Color.Transparent);
        }

        public static void RenderAllOccupierCellsRed<T>(Graphics graphics, Rectangle visibleCells, Size tileSize, IEnumerable<(Point, T)> occupiers) where T : ICellOccupier, ICellOverlapper
        {
            RenderAllOccupierBounds(graphics, visibleCells, tileSize, occupiers, Color.Transparent, Color.Red);
        }

        public static void RenderAllOccupierBounds<T>(Graphics graphics, Rectangle visibleCells, Size tileSize, IEnumerable<(Point, T)> occupiers) where T : ICellOccupier, ICellOverlapper
        {
            RenderAllOccupierBounds(graphics, visibleCells, tileSize, occupiers, Color.Green, Color.Red);
        }

        public static void RenderAllOccupierBounds<T>(Graphics graphics, Rectangle visibleCells, Size tileSize, IEnumerable<(Point, T)> occupiers, Color boundsColor, Color occupierColor) where T : ICellOccupier, ICellOverlapper
        {
            float boundsPenSize = Math.Max(1, tileSize.Width / 16.0f);
            float occupyPenSize = Math.Max(0.5f, tileSize.Width / 32.0f);
            if (occupyPenSize == boundsPenSize)
            {
                boundsPenSize += 2;
            }
            if (boundsColor.A != 0)
            {
                using (Pen boundsPen = new Pen(boundsColor, boundsPenSize))
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
                }
            }
            if (occupierColor.A != 0)
            {
                using (Pen occupyPen = new Pen(occupierColor, occupyPenSize))
                {
                    foreach ((Point topLeft, T occupier) in occupiers)
                    {
                        bool[,] occupyMask = occupier is Building bl ? bl.Type.BaseOccupyMask : occupier.OccupyMask;

                        for (int y = 0; y < occupyMask.GetLength(0); ++y)
                        {
                            for (int x = 0; x < occupyMask.GetLength(1); ++x)
                            {
                                if (!occupyMask[y, x])
                                {
                                    continue;
                                }
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

        public static void RenderAllCrateOutlines(Graphics g, GameInfo gameInfo, Map map, Rectangle visibleCells, Size tileSize, double tileScale, bool onlyIfBehindObjects)
        {
            RenderAllOverlayOutlines(g, gameInfo, map, visibleCells, tileSize, tileScale, OverlayTypeFlag.WoodCrate, onlyIfBehindObjects, Globals.OutlineColorCrateWood);
            RenderAllOverlayOutlines(g, gameInfo, map, visibleCells, tileSize, tileScale, OverlayTypeFlag.SteelCrate, onlyIfBehindObjects, Globals.OutlineColorCrateSteel);
        }
        
        public static void RenderAllSolidOverlayOutlines(Graphics g, GameInfo gameInfo, Map map, Rectangle visibleCells, Size tileSize, double tileScale, bool onlyIfBehindObjects)
        {
            RenderAllOverlayOutlines(g, gameInfo, map, visibleCells, tileSize, tileScale, OverlayTypeFlag.Solid, onlyIfBehindObjects, Globals.OutlineColorSolidOverlay);
            RenderAllOverlayOutlines(g, gameInfo, map, visibleCells, tileSize, tileScale, OverlayTypeFlag.Wall, onlyIfBehindObjects, Globals.OutlineColorWall);
        }

        public static void RenderAllOverlayOutlines(Graphics g, GameInfo gameInfo, Map map, Rectangle visibleCells, Size tileSize, double tileScale, OverlayTypeFlag types,
            bool onlyIfBehindObjects, Color outlineColor)
        {
            if (outlineColor.A == 0)
            {
                return;
            }
            Dictionary<Point, Overlay> includedPoints = new Dictionary<Point, Overlay>();
            List<int> includedCells = new List<int>();
            float outlineThickness = 0.05f;
            byte alphaThreshold = (byte)(Globals.UseClassicFiles ? 0x80 : 0x40);
            //double lumThreshold = 0.01d;
            // Get all included points in an initial sweep so they're all available in the second processing step.
            foreach ((int cell, Overlay overlay) in map.Overlay)
            {
                OverlayType ovlt = overlay.Type;
                if (!ovlt.Flag.HasAnyFlags(types) || !map.Metrics.GetLocation(cell, out Point location))
                {
                    continue;
                }
                // Solid overlay should never exist on cells with units.
                bool unitsAlwaysOverlap = !overlay.Type.IsSolid && !overlay.Type.IsWall;
                if (!visibleCells.Contains(location) || (onlyIfBehindObjects && !IsOverlapped(map, location, unitsAlwaysOverlap, null, null, location, -1)))
                {
                    continue;
                }
                includedCells.Add(cell);
                includedPoints.Add(location, overlay);
            }
            // Now we have all cells to process, we can ensure the outlines for neighbouring wall cells are combined.
            foreach (int cell in includedCells)
            {
                if (!map.Overlay.Metrics.GetLocation(cell, out Point p) || !includedPoints.TryGetValue(p, out Overlay overlay))
                {
                    continue;
                }
                OverlayType ovlt = overlay.Type;
                Size cellSize = new Size(1, 1);
                Color outlineCol = Color.FromArgb(0xA0, outlineColor);
                Overlay tstOvl;
                // If this is a wall, exclude edge cells on sides that are the same type of wall, so they connect properly.
                bool includeAbove = !ovlt.IsWall || !includedPoints.TryGetValue(p.OffsetPoint(0, -1), out tstOvl) || tstOvl.Type.ID != ovlt.ID;
                bool includeRight = !ovlt.IsWall || !includedPoints.TryGetValue(p.OffsetPoint(1, 0), out tstOvl) || tstOvl.Type.ID != ovlt.ID;
                bool includeBelow = !ovlt.IsWall || !includedPoints.TryGetValue(p.OffsetPoint(0, 1), out tstOvl) || tstOvl.Type.ID != ovlt.ID;
                bool includeLefty = !ovlt.IsWall || !includedPoints.TryGetValue(p.OffsetPoint(-1, 0), out tstOvl) || tstOvl.Type.ID != ovlt.ID;
                int aroundMask = (includeAbove ? 1 : 0) | (includeRight ? 2 : 0) | (includeBelow ? 4 : 0) | (includeLefty ? 8 : 0);
                string ovlId = "outline_ovl_" + overlay.Type.Name + "_" + overlay.Icon + "_" + aroundMask + "_" + tileSize.Width + "x" + tileSize.Height;
                RegionData paintAreaRel = Globals.TheShapeCacheManager.GetShape(ovlId);
                if (paintAreaRel == null)
                {
                    using (Bitmap bm = new Bitmap(tileSize.Width * 3, tileSize.Height * 3, PixelFormat.Format32bppArgb))
                    {
                        using (Graphics ig = Graphics.FromImage(bm))
                        {
                            RenderOverlay(gameInfo, new Point(1, 1), null, tileSize, tileScale, overlay, true).Item2(ig);
                        }
                        paintAreaRel = ImageUtils.GetOutline(tileSize, bm, outlineThickness, alphaThreshold, Globals.UseClassicFiles);
                        // Wall connecting: if any side cells should be excluded so they connect to neighbouring cells,
                        // intersect the region with another region containing only the desired side cells.
                        if (ovlt.IsWall && aroundMask != (1 | 2 | 4 | 8))
                        {
                            using (Region outline = new Region(paintAreaRel))
                            {
                                using (Region rgn = new Region(new Rectangle(new Point(tileSize.Width, tileSize.Height), tileSize)))
                                {
                                    // Adds side cells that aren't cut off.
                                    if (includeAbove) rgn.Union(new Rectangle(new Point(tileSize.Width/**/, /**/              0), tileSize));
                                    if (includeRight) rgn.Union(new Rectangle(new Point(tileSize.Width * 2, tileSize.Height/**/), tileSize));
                                    if (includeBelow) rgn.Union(new Rectangle(new Point(tileSize.Width/**/, tileSize.Height * 2), tileSize));
                                    if (includeLefty) rgn.Union(new Rectangle(new Point(/**/             0, tileSize.Height/**/), tileSize));
                                    // Not sure if needed; adds corner cells as well if neighbouring cells are added.
                                    if (includeAbove && includeRight) rgn.Union(new Rectangle(new Point(tileSize.Width * 2, /**/              0), tileSize));
                                    if (includeRight && includeBelow) rgn.Union(new Rectangle(new Point(tileSize.Width * 2, tileSize.Height * 2), tileSize));
                                    if (includeBelow && includeLefty) rgn.Union(new Rectangle(new Point(/**/             0, tileSize.Height * 2), tileSize));
                                    if (includeLefty && includeAbove) rgn.Union(new Rectangle(new Point(/**/             0, /**/              0), tileSize));
                                    outline.Intersect(rgn);
                                    paintAreaRel = outline.GetRegionData();
                                }
                            }
                        }
                        Globals.TheShapeCacheManager.AddShape(ovlId, paintAreaRel);
                    }
                }
                int actualTopLeftX = tileSize.Width * (p.X -1);
                int actualTopLeftY = tileSize.Height * (p.Y -1);
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
                foreach (InfantryStoppingType ist in InfantryGroup.RenderOrder)
                {
                    Infantry infantry = infantryGroup.Infantry[(int)ist];
                    if (infantry == null)
                    {
                        continue;
                    }
                    if (onlyIfBehindObjects)
                    {
                        if (!IsOverlapped(map, location, false, ist, infantryGroup, location, infantryGroup.DrawOrderCache))
                        {
                            continue;
                        }
                    }
                    Color outlineCol = Color.FromArgb(0x80, Globals.TheTeamColorManager.GetBaseColor(infantry.House?.UnitTeamColor));
                    string infId = "outline_inf_" + infantry.Type.Name + '_' + ((int)ist) + '_' + infantry.Direction.ID + "_" + tileSize.Width + "x" + tileSize.Height;
                    RegionData paintAreaRel = Globals.TheShapeCacheManager.GetShape(infId);
                    if (paintAreaRel == null)
                    {
                        using (Bitmap bm = new Bitmap(tileSize.Width * 3, tileSize.Height * 3, PixelFormat.Format32bppArgb))
                        {
                            using (Graphics ig = Graphics.FromImage(bm))
                            {
                                RenderInfantry(new Point(1, 1), tileSize, infantry, ist, true).RenderAction(ig);
                            }
                            paintAreaRel = ImageUtils.GetOutline(tileSize, bm, outlineThickness, alphaThreshold, Globals.UseClassicFiles);
                            Globals.TheShapeCacheManager.AddShape(infId, paintAreaRel);
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

        public static void RenderAllUnitOutlines(Graphics g, GameInfo gameInfo, Map map, Rectangle visibleCells, Size tileSize, bool onlyIfBehindObjects)
        {
            RenderAllObjectOutlines(g, gameInfo, map, map.Technos.OfType<Unit>(), visibleCells, tileSize, true,
                (h) => h.UnitTeamColor, (gr, p, unt) => RenderUnit(gameInfo, new Point(1, 1), tileSize, unt, true).RenderAction(gr), Color.Black);
        }

        public static void RenderAllBuildingOutlines(Graphics g, GameInfo gameInfo, Map map, Rectangle visibleCells, Size tileSize, double tileScale, bool onlyIfBehindObjects)
        {
            RenderAllObjectOutlines(g, gameInfo, map, map.Buildings.OfType<Building>(), visibleCells, tileSize, true,
                (h) => h.BuildingTeamColor, (gr, p, bld) => RenderBuilding(gameInfo, null, p, tileSize, tileScale, bld, true).RenderAction(gr), Color.Black);
        }

        public static void RenderAllTerrainOutlines(Graphics g, GameInfo gameInfo, Map map, Rectangle visibleCells, Size tileSize, double tileScale, bool onlyIfBehindObjects)
        {
            RenderAllObjectOutlines(g, gameInfo, map, map.Technos.OfType<Terrain>(), visibleCells, tileSize, true,
                null, (gr, p, trn) => RenderTerrain(p, tileSize, tileScale, trn, true).RenderAction(gr), Globals.OutlineColorTerrain);
        }

        /// <summary>
        /// Unified function for rendering outlines for techno objects. Infantry are still separate, because of the whole InfantryGroup mess.
        /// </summary>
        /// <typeparam name="T">Techno that's being handled.</typeparam>
        /// <param name="g">Graphics object to paint on.</param>
        /// <param name="gameInfo">GameInfo object for this game.</param>
        /// <param name="map">Map, to check metrics and object overlaps.</param>
        /// <param name="occupiers">The list of occupiers to render outlines for.</param>
        /// <param name="visibleCells">Visible area on the screen in which these outlines should be rendered.</param>
        /// <param name="tileSize">Size of the map tiles.</param>
        /// <param name="onlyIfBehindObjects">True to only render objects that are overlapped by something.</param>
        /// <param name="colorPick">Function to get the preferred team color name from a House.</param>
        /// <param name="RenderAction">Action to render an object of the handled type at a specific point on a graphics object.</param>
        /// <param name="fallbackColor">Fallback colour in case no House or <paramref name="colorPick"/> function are available.</param>
        private static void RenderAllObjectOutlines<T>(Graphics g, GameInfo gameInfo, Map map, IEnumerable<(Point Location, T Occupier)> occupiers,
            Rectangle visibleCells, Size tileSize, bool onlyIfBehindObjects, Func<HouseType, string> colorPick, Action<Graphics, Point, T> RenderAction, Color fallbackColor)
            where T: ITechno, ICellOverlapper, ICellOccupier, ICloneable
        {
            float outlineThickness = 0.05f;
            byte alphaThreshold = (byte)(Globals.UseClassicFiles ? 0x80 : 0x40);
            //double lumThreshold = 0.01d;
            visibleCells.Inflate(1, 1);
            foreach ((Point objLocation, T placedObj) in occupiers.OrderBy(i => map.Metrics.GetCell(i.Location)))
            {
                // This is a visibility check; check cells that are deemed "visible".
                bool[,] opaqueMask = placedObj.OpaqueMask;
                bool[,] occupyMask = placedObj.OccupyMask;
                int paintOrder = placedObj.DrawOrderCache;
                int maskY = opaqueMask == null ? 0 : opaqueMask.GetLength(0);
                int maskX = opaqueMask == null ? 0 : opaqueMask.GetLength(1);
                // If not in currently viewed area, ignore.
                Rectangle objBounds = new Rectangle(objLocation, placedObj.OccupyMask.GetDimensions());
                if (!visibleCells.IntersectsWith(objBounds))
                {
                    continue;
                }
                if (onlyIfBehindObjects)
                {
                    // Select actual map points for all visible points in opaqueMask
                    bool allOpaque = true;
                    Point[] opaquePoints = Enumerable.Range(0, maskY).SelectMany(nrY => Enumerable.Range(0, maskX).Select(nrX => new Point(nrX, nrY)))
                        .Where(pt => opaqueMask[pt.Y, pt.X]).Select(pt => new Point(objLocation.X + pt.X, objLocation.Y + pt.Y)).ToArray();
                    foreach (Point opaquePoint in opaquePoints)
                    {
                        if (!IsOverlapped(map, opaquePoint, false, null, placedObj, objLocation, paintOrder))
                        {
                            allOpaque = false;
                            break;
                        }
                    }
                    if (!allOpaque)
                    {
                        continue;
                    }
                }
                Size cellSize = new Size(maskX + 2, maskY + 2);
                Color houseCol = fallbackColor;
                if (placedObj.House != null && colorPick != null)
                {
                    houseCol = Color.FromArgb(0x80, Globals.TheTeamColorManager.GetBaseColor(colorPick(placedObj.House)));
                }
                string id = "outline_" + typeof(T).Name + "_" + placedObj.TechnoType.Name + '_' + (placedObj.Direction == null ? 0 : placedObj.Direction.ID).ToString() + "_" + tileSize.Width + "x" + tileSize.Height;
                RegionData paintAreaRel = Globals.TheShapeCacheManager.GetShape(id);
                if (paintAreaRel == null)
                {
                    // Clone without preview flag.
                    T toRender = placedObj;
                    if (placedObj.IsPreview)
                    {
                        toRender = (T)placedObj.Clone();
                        placedObj.IsPreview = false;
                        if (placedObj is Building bld)
                        {
                            bld.BasePriority = 0;
                            bld.IsPrebuilt = true;
                        }
                    }
                    using (Bitmap bm = new Bitmap(tileSize.Width * cellSize.Width, tileSize.Height * cellSize.Width, PixelFormat.Format32bppArgb))
                    {
                        using (Graphics ig = Graphics.FromImage(bm))
                        {
                            RenderAction(ig, new Point(1, 1), toRender);
                        }
                        paintAreaRel = ImageUtils.GetOutline(tileSize, bm, outlineThickness, alphaThreshold, Globals.UseClassicFiles);
                        Globals.TheShapeCacheManager.AddShape(id, paintAreaRel);
                    }
                }
                int paintPosTopLeftX = (objLocation.X - 1) * tileSize.Width;
                int paintPosTopLeftY = (objLocation.Y - 1) * tileSize.Height;
                if (paintAreaRel != null)
                {
                    using (Region paintArea = new Region(paintAreaRel))
                    using (Brush brush = new SolidBrush(houseCol))
                    {
                        paintArea.Translate(paintPosTopLeftX, paintPosTopLeftY);
                        g.FillRegion(brush, paintArea);
                    }
                }
            }
        }

        /// <summary>
        /// Check if an object is considered overlapped by something on the map.
        /// </summary>
        /// <param name="map">Map to check on.</param>
        /// <param name="location">Location to check for overlap.</param>
        /// <param name="unitsAlwaysOverlap">True to immediately return true if the cell is occupied by units.</param>
        /// <param name="ist">When filled in, the overlapper is treated as infantry on that location.</param>
        /// <param name="objectToCheck">Object for which overlap is being checked. This object is automatically ignored in the objects it loops over to check for overlaps.</param>
        /// <param name="objectLocation">Object location on the map.</param>
        /// <returns>true if the cell is considered filled enough to overlap things.</returns>
        private static bool IsOverlapped(Map map, Point location, bool unitsAlwaysOverlap, InfantryStoppingType? ist,
            ICellOverlapper objectToCheck, Point objectLocation, int objectPaintOrder)
        {
            ICellOccupier techno = map.Technos[location];
            // Single-cell occupier. Always pass.
            if (unitsAlwaysOverlap && (techno is Unit || techno is InfantryGroup) && techno != objectToCheck)
            {
                return true;
            }
            Point centerPoint = GetRenderPoint(objectToCheck, ist);
            // Logic for multi-cell occupiers; buildings and terrain.
            // Return true if either an occupied cell, or overlayed by graphics deemed opaque.
            ICellOverlapper[] technos = map.Overlappers.OverlappersAt(location).Where(ov => !ReferenceEquals(ov,objectToCheck)).ToArray();
            if (technos.Length == 0)
            {
                return false;
            }
            foreach (ICellOverlapper ovl in technos)
            {
                ICellOccupier occ = ovl as ICellOccupier;
                if (occ == null)
                {
                    continue;
                }
                bool[,] opaqueMask = ovl.OpaqueMask;
                int maskY = opaqueMask == null ? 0 : opaqueMask.GetLength(0);
                int maskX = opaqueMask == null ? 0 : opaqueMask.GetLength(1);
                Point? pt = map.Technos[occ] ?? map.Buildings[occ];
                if (!pt.HasValue)
                {
                    continue;
                }
                // Object we're comparing with was drawn before the current one, so it can't possible overlap it.
                // This caching allows extremely easy checks on overlap without much processing.
                if (ovl is ITechno paintedObj && paintedObj.DrawOrderCache < objectPaintOrder)
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
                // Trick to convert 2-dimensional arrays to linear format.
                bool[] opaqueArr = opaqueMask.Cast<bool>().ToArray();
                if (opaqueArr[index])
                {
                    // If obscured from view by graphics, return true.
                    return true;
                }
            }
            return false;
        }

        public static void RenderAllFootballAreas(Graphics graphics, Map map, Rectangle visibleCells, Size tileSize, double tileScale, GameInfo gameInfo)
        {
            if (!gameInfo.SupportsMapLayer(MapLayerFlag.FootballArea))
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
                    IsPreview = true,
                };
                RenderOverlay(gameInfo, p, null, tileSize, tileScale, footballTerrain, false).Item2(graphics);
            }
        }

        public static void RenderWaypointFlags(Graphics graphics, GameInfo gameInfo, Map map, Rectangle visibleCells, Size tileSize)
        {
            // Re-render flags on top of football areas.
            List<Waypoint> flagWayPoints = new List<Waypoint>();
            Dictionary<int, int> flagOverlapMpCheck = new Dictionary<int, int>();
            Dictionary<Waypoint, int> flagOffsets = new Dictionary<Waypoint, int>();
            // Get all waypoints. Ignore the preview if it is on the same cell as the same actual waypoint.
            // Preview waypoint is always only one, and added at the end, so a sequential run always works.
            foreach (Waypoint waypoint in map.Waypoints)
            {
                int mpId = Waypoint.GetMpIdFromFlag(waypoint.Flag);
                if (waypoint.Point.HasValue && mpId >= 0 && visibleCells.Contains(waypoint.Point.Value)
                    && map.Metrics.GetCell(waypoint.Point.Value, out int cell))
                {
                    bool alreadyExists = flagOverlapMpCheck.TryGetValue(mpId, out int mpCell) && cell == mpCell;
                    if (!alreadyExists)
                    {
                        flagWayPoints.Add(waypoint);
                        flagOverlapMpCheck[mpId] = cell;
                    }
                }
            }
            // Create offsets if multiple flags are on the same cell.
            flagWayPoints = flagWayPoints.OrderBy(w => Waypoint.GetMpIdFromFlag(w.Flag)).ToList();
            Dictionary<int, int> flagOverlapPoints = new Dictionary<int, int>();
            foreach (Waypoint waypoint in flagWayPoints)
            {
                if (map.Metrics.GetCell(waypoint.Point.Value, out int cell))
                {
                    if (!flagOverlapPoints.TryGetValue(cell, out int amount))
                    {
                        flagOverlapPoints.Add(cell, 1);
                    }
                    else
                    {
                        flagOverlapPoints[cell] = amount + 1;
                    }
                    flagOffsets[waypoint] = amount * 2;
                }
            }
            // Paint the flags.
            ITeamColor[] flagColors = map.FlagColors;
            foreach (Waypoint wp in flagWayPoints)
            {
                flagOffsets.TryGetValue(wp, out int offset);
                RenderWaypoint(gameInfo, false, tileSize, flagColors, wp, wp.IsPreview ? Globals.PreviewAlphaFloat : 1.0f, offset).Item2(graphics);
            }
        }

        public static void RenderAllFakeBuildingLabels(Graphics graphics, GameInfo gameInfo, Map map, Rectangle visibleCells, Size tileSize)
        {
            RenderAllFakeBuildingLabels(graphics, gameInfo, map.Buildings.OfType<Building>(), visibleCells, tileSize);
        }

        public static void RenderAllFakeBuildingLabels(Graphics graphics, GameInfo gameInfo, IEnumerable<(Point topLeft, Building building)> buildings, Rectangle visibleCells, Size tileSize)
        {
            Color textColor = Color.White;
            Color backPaintColor = Color.Black;
            StringFormat stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            string classicFont = null;
            bool cropClassicFont = false;
            TeamRemap remapClassicFont = null;
            if (Globals.TheTilesetManager is TilesetManagerClassic tsmc && Globals.TheTeamColorManager is TeamRemapManager trm)
            {
                classicFont = gameInfo.GetClassicFontInfo(ClassicFont.CellTriggers, tsmc, trm, textColor, out cropClassicFont, out remapClassicFont);
            }
            string fakeText = Globals.TheGameTextManager["TEXT_UI_FAKE"];
            double tileScaleHor = tileSize.Width / 128.0;

            using (SolidBrush fakeBackgroundBrushPrev = new SolidBrush(Color.FromArgb(128 * 2 / 3, backPaintColor)))
            using (SolidBrush fakeBackgroundBrush = new SolidBrush(Color.FromArgb(256 * 2 / 3, backPaintColor)))
            using (ImageAttributes imageAttributes = new ImageAttributes())
            {
                foreach ((Point topLeft, Building building) in buildings)
                {
                    if (!building.Type.IsFake)
                    {
                        continue;
                    }
                    Rectangle buildingCellBounds = new Rectangle(topLeft, building.Type.Size);
                    if (!visibleCells.IntersectsWith(buildingCellBounds))
                    {
                        continue;
                    }
                    bool forPreview = building.IsPreview;
                    Size maxSize = building.Type.Size;
                    Rectangle buildingBounds = new Rectangle(
                        new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height),
                        new Size(maxSize.Width * tileSize.Width, maxSize.Height * tileSize.Height)
                    );
                    if (classicFont == null)
                    {
                        using (SolidBrush fakeTextBrush = new SolidBrush(Color.FromArgb(forPreview ? Globals.PreviewAlphaInt : 255, textColor)))
                        {
                            using (Font font = graphics.GetAdjustedFont(fakeText, SystemFonts.DefaultFont, buildingBounds.Width, buildingBounds.Height,
                                Math.Max(1, (int)Math.Round(12 * tileScaleHor)), Math.Max(1, (int)Math.Round(24 * tileScaleHor)), stringFormat, true))
                            {
                                SizeF textBounds = graphics.MeasureString(fakeText, font, buildingBounds.Width, stringFormat);
                                RectangleF backgroundBounds = new RectangleF(buildingBounds.Location, textBounds);
                                graphics.FillRectangle(forPreview ? fakeBackgroundBrushPrev : fakeBackgroundBrush, backgroundBounds);
                                graphics.DrawString(fakeText, font, fakeTextBrush, backgroundBounds, stringFormat);
                            }
                        }
                    }
                    else
                    {
                        Rectangle buildingRenderBounds = new Rectangle(
                            new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height),
                            new Size(maxSize.Width * tileSize.Width, maxSize.Height * tileSize.Height)
                        );
                        string fkId = "fake_classic_" + maxSize.Width + "x" + maxSize.Height;
                        Bitmap fkBm = Globals.TheShapeCacheManager.GetImage(fkId);
                        if (fkBm == null)
                        {
                            Rectangle buildingBoundsClassic = new Rectangle(Point.Empty, new Size(maxSize.Width * Globals.OriginalTileWidth, maxSize.Height * Globals.OriginalTileHeight));
                            fkBm = new Bitmap(buildingBoundsClassic.Width, buildingBoundsClassic.Height);
                            using (Graphics bmgr = Graphics.FromImage(fkBm))
                            {
                                int[] indices = Encoding.ASCII.GetBytes(fakeText).Select(x => (int)x).ToArray();
                                using (Bitmap txt = RenderTextFromSprite(classicFont, remapClassicFont, Size.Empty, indices, false, cropClassicFont))
                                {
                                    int frameWidth = Math.Min(txt.Width + 2, buildingBoundsClassic.Width);
                                    int frameHeight = Math.Min(txt.Height + 2, buildingBoundsClassic.Height);
                                    Rectangle backgroundBounds = new Rectangle(Point.Empty, new Size(frameWidth, frameHeight));
                                    Rectangle frameRect = new Rectangle(0, 0, frameWidth, frameHeight);
                                    Rectangle textRect = new Rectangle(1, 1, txt.Width, txt.Height);
                                    bmgr.FillRectangle(fakeBackgroundBrush, frameRect);
                                    bmgr.DrawImage(txt, textRect, 0, 0, txt.Width, txt.Height, GraphicsUnit.Pixel);
                                }
                            }
                            Globals.TheShapeCacheManager.AddImage(fkId, fkBm);
                        }
                        imageAttributes.SetColorMatrix(GetColorMatrix(Color.White, 1.0f, building.IsPreview ? 0.5f : 1.0f));
                        graphics.DrawImage(fkBm, buildingRenderBounds, 0, 0, fkBm.Width, fkBm.Height, GraphicsUnit.Pixel, imageAttributes);
                        
                    }
                }
            }
        }

        public static void RenderAllRebuildPriorityLabels(Graphics graphics, GameInfo gameInfo, IEnumerable<(Point topLeft, Building building)> buildings, Rectangle visibleCells, Size tileSize, double tilescale)
        {
            Color textColor = Color.Red;
            Color backPaintColor = Color.Black;
            StringFormat stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            string classicFont = null;
            bool cropClassicFont = false;
            TeamRemap remapClassicFont = null;
            if (Globals.TheTilesetManager is TilesetManagerClassic tsmc && Globals.TheTeamColorManager is TeamRemapManager trm)
            {
                classicFont = gameInfo.GetClassicFontInfo(ClassicFont.CellTriggers, tsmc, trm, textColor, out cropClassicFont, out remapClassicFont);
            }
            foreach ((Point topLeft, Building building) in buildings)
            {
                if (building.BasePriority < 0 || !visibleCells.IntersectsWith(new Rectangle(topLeft, building.Type.Size)))
                {
                    continue;
                }
                Size maxSize = building.Type.Size;
                Rectangle buildingRenderBounds = new Rectangle(
                    new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height),
                    new Size(maxSize.Width * tileSize.Width, maxSize.Height * tileSize.Height)
                );
                double tileScaleHor = tileSize.Width / 128.0;
                string priText = building.BasePriority.ToString();
                using (SolidBrush baseBackgroundBrush = new SolidBrush(Color.FromArgb(256 * 2 / 3, backPaintColor)))
                using (ImageAttributes imageAttributes = new ImageAttributes())
                {
                    imageAttributes.SetColorMatrix(GetColorMatrix(Color.White, 1.0f, building.IsPreview ? 0.5f : 1.0f));
                    if (classicFont == null)
                    {
                        using (SolidBrush baseTextBrush = new SolidBrush(Color.FromArgb(255, textColor)))
                        using (Font font = graphics.GetAdjustedFont(priText, SystemFonts.DefaultFont, buildingRenderBounds.Width, buildingRenderBounds.Height,
                            Math.Max(1, (int)Math.Round(12 * tileScaleHor)), Math.Max(1, (int)Math.Round(24 * tileScaleHor)), stringFormat, true))
                        {
                            SizeF textBounds = graphics.MeasureString(priText, font, buildingRenderBounds.Width, stringFormat);
                            RectangleF backgroundBounds = new RectangleF(new PointF(buildingRenderBounds.X, buildingRenderBounds.Y), textBounds);
                            backgroundBounds.Offset((buildingRenderBounds.Width - textBounds.Width) / 2.0f, buildingRenderBounds.Height - textBounds.Height);
                            graphics.FillRectangle(baseBackgroundBrush, backgroundBounds);
                            graphics.DrawString(priText, font, baseTextBrush, backgroundBounds, stringFormat);
                        }
                    }
                    else
                    {
                        string priId = "priority_classic_" + maxSize.Width + "x" + maxSize.Height + "_" + priText;
                        Bitmap priBm = Globals.TheShapeCacheManager.GetImage(priId);
                        if (priBm == null)
                        {
                            Rectangle buildingBounds = new Rectangle(Point.Empty, new Size(maxSize.Width * Globals.OriginalTileWidth, maxSize.Height * Globals.OriginalTileHeight));
                            priBm = new Bitmap(buildingBounds.Width, buildingBounds.Height);
                            using (Graphics bmgr = Graphics.FromImage(priBm))
                            {
                                int[] indices = Encoding.ASCII.GetBytes(priText).Select(x => (int)x).ToArray();
                                using (Bitmap txt = RenderTextFromSprite(classicFont, remapClassicFont, Size.Empty, indices, false, cropClassicFont))
                                {
                                    int textOffsetX = (buildingBounds.Width - txt.Width) / 2;
                                    int textOffsetY = (buildingBounds.Height - txt.Height - 1);
                                    int frameOffsetX = Math.Max(textOffsetX, -tileSize.Width) - 1;
                                    int frameOffsetY = Math.Max(textOffsetY, -tileSize.Height) - 1;
                                    int frameWidth = Math.Min(txt.Width + 2, buildingBounds.Width);
                                    int frameHeight = Math.Min(txt.Height + 2, buildingBounds.Height);
                                    Rectangle frameRect = new Rectangle(frameOffsetX, frameOffsetY, frameWidth, frameHeight);
                                    Rectangle textRect = new Rectangle(textOffsetX, textOffsetY, txt.Width, txt.Height);
                                    bmgr.FillRectangle(baseBackgroundBrush, frameRect);
                                    bmgr.DrawImage(txt, textRect, 0, 0, txt.Width, txt.Height, GraphicsUnit.Pixel);
                                }
                            }
                            Globals.TheShapeCacheManager.AddImage(priId, priBm);
                        }
                        imageAttributes.SetColorMatrix(GetColorMatrix(Color.White, 1.0f, building.IsPreview ? 0.5f : 1.0f));
                        graphics.DrawImage(priBm, buildingRenderBounds, 0, 0, priBm.Width, priBm.Height, GraphicsUnit.Pixel, imageAttributes);
                        
                    }
                }
            }
        }

        public static void RenderAllTechnoTriggers(Graphics graphics, GameInfo gameInfo, Map map, Rectangle visibleCells, Size tileSize, MapLayerFlag layersToRender)
        {
            RenderAllTechnoTriggers(graphics, gameInfo, map.Technos, visibleCells, tileSize, layersToRender, Color.LimeGreen, null, false);
        }

        public static void RenderAllTechnoTriggers(Graphics graphics, GameInfo gameInfo, OccupierSet<ICellOccupier> mapTechnos, Rectangle visibleCells, Size tileSize, MapLayerFlag layersToRender, Color color, string toPick, bool excludePick)
        {
            string classicFont = null;
            bool cropClassicFont = false;
            string classicFontInf = null;
            bool cropClassicFontInf = false;
            TeamRemap remapClassicFont = null;
            TeamRemap remapClassicFontInf = null;
            if (Globals.TheTilesetManager is TilesetManagerClassic tsmc && Globals.TheTeamColorManager is TeamRemapManager trm)
            {
                classicFont = gameInfo.GetClassicFontInfo(ClassicFont.TechnoTriggers, tsmc, trm, color, out cropClassicFont, out remapClassicFont);
                classicFontInf = gameInfo.GetClassicFontInfo(ClassicFont.InfantryTriggers, tsmc, trm, color, out cropClassicFontInf, out remapClassicFontInf);
            }
            double tileScaleHor = tileSize.Width / 128.0;
            float borderSize = Math.Max(0.5f, tileSize.Width / 60.0f);
            foreach ((Point topLeft, ICellOccupier techno) in mapTechnos)
            {
                Point location = new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height);
                (string trigger, Rectangle bounds, int alpha)[] triggers = null;
                if (techno is Terrain terrain && !Trigger.IsEmpty(terrain.Trigger))
                {
                    if (layersToRender.HasFlag(MapLayerFlag.Terrain))
                    {
                        if (visibleCells.IntersectsWith(new Rectangle(topLeft, terrain.Type.Size)))
                        {
                            Size size = new Size(terrain.Type.Size.Width * tileSize.Width, terrain.Type.Size.Height * tileSize.Height);
                            triggers = new (string, Rectangle, int)[] { (terrain.Trigger, new Rectangle(location, size), terrain.IsPreview ? Globals.PreviewAlphaInt : 255) };
                        }
                    }
                }
                else if (techno is Building building && !Trigger.IsEmpty(building.Trigger))
                {
                    if (layersToRender.HasFlag(MapLayerFlag.Buildings))
                    {
                        if (visibleCells.IntersectsWith(new Rectangle(topLeft, building.Type.Size)))
                        {
                            Size size = new Size(building.Type.Size.Width * tileSize.Width, building.Type.Size.Height * tileSize.Height);
                            triggers = new (string, Rectangle, int)[] { (building.Trigger, new Rectangle(location, size), building.IsPreview ? Globals.PreviewAlphaInt : 255) };
                        }
                    }
                }
                else if (techno is Unit unit && !Trigger.IsEmpty(unit.Trigger))
                {
                    if (layersToRender.HasFlag(MapLayerFlag.Units))
                    {
                        if (visibleCells.Contains(topLeft))
                        {
                            triggers = new (string, Rectangle, int)[] { (unit.Trigger, new Rectangle(location, tileSize), unit.IsPreview ? Globals.PreviewAlphaInt : 255) };
                        }
                    }
                }
                else if (techno is InfantryGroup infantryGroup)
                {
                    if (layersToRender.HasFlag(MapLayerFlag.Infantry))
                    {
                        if (!visibleCells.Contains(topLeft))
                        {
                            continue;
                        }
                        List<(string, Rectangle, int)> infantryTriggers = new List<(string, Rectangle, int)>();
                        for (int i = 0; i < infantryGroup.Infantry.Length; ++i)
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
                            infantryTriggers.Add((infantry.Trigger, bounds, infantry.IsPreview ? Globals.PreviewAlphaInt : 255));
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
                    foreach ((string trigger, Rectangle bounds, int alpha) in triggers.Where(x => toPick == null
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

        public static void RenderWayPointIndicators(Graphics graphics, Map map, GameInfo gameInfo, Rectangle visibleCells, Size tileSize, Color textColor, bool forPreview, bool excludeSpecified, params Waypoint[] specified)
        {
            HashSet<Waypoint> specifiedWaypoints = specified.ToHashSet();

            Waypoint[] toPaint = excludeSpecified ? map.Waypoints : specified;
            string classicFontShort = null;
            bool cropClassicFontShort = false;
            TeamRemap remapClassicFontShort = null;
            string classicFontLong = null;
            bool cropClassicFontLong = false;
            TeamRemap remapClassicFontLong = null;

            if (Globals.TheTilesetManager is TilesetManagerClassic tsmc && Globals.TheTeamColorManager is TeamRemapManager trm)
            {
                classicFontShort = gameInfo.GetClassicFontInfo(ClassicFont.Waypoints, tsmc, trm, textColor, out cropClassicFontShort, out remapClassicFontShort);
                classicFontLong = gameInfo.GetClassicFontInfo(ClassicFont.WaypointsLong, tsmc, trm, textColor, out cropClassicFontLong, out remapClassicFontLong);
            }
            foreach (Waypoint waypoint in toPaint)
            {
                if ((excludeSpecified && specifiedWaypoints.Contains(waypoint)) || !waypoint.Cell.HasValue
                    || !map.Metrics.GetLocation(waypoint.Cell.Value, out Point topLeft) || !visibleCells.Contains(topLeft))
                {
                    continue;
                }
                Rectangle paintBounds = new Rectangle(new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height), tileSize);
                string wpText = waypoint.Name;
                bool isLong = wpText.Length > 3;
                string classicFont = isLong ? classicFontLong : classicFontShort;
                bool cropClassicFont = isLong ? cropClassicFontLong : cropClassicFontShort;
                TeamRemap remapClassicFont = isLong ? remapClassicFontLong : remapClassicFontShort;
                if (classicFont != null && isLong)
                {
                    wpText = waypoint.ShortName;
                }
                Color backPaintColor = Color.FromArgb(128, Color.Black);
                // Adjust calcuations to tile size. The below adjustments are done assuming the tile is 128 wide.
                double tileScaleHor = tileSize.Width / 128.0;
                using (SolidBrush baseBackgroundBrush = new SolidBrush(backPaintColor))
                using (ImageAttributes imageAttributes = new ImageAttributes())
                {
                    imageAttributes.SetColorMatrix(GetColorMatrix(Color.White, 1.0f, forPreview ? 0.5f : 1.0f));
                    if (classicFont == null)
                    {
                        StringFormat stringFormat = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        using (SolidBrush baseTextBrush = new SolidBrush(Color.FromArgb(forPreview ? 128 : 255, textColor)))
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
                    else
                    {
                        string wpId = "waypoint_" + wpText + "_" + classicFont + "_" + remapClassicFont.Name;
                        Bitmap wpBm = Globals.TheShapeCacheManager.GetImage(wpId);
                        if (wpBm == null)
                        {
                            wpBm = new Bitmap(tileSize.Width, tileSize.Height);
                            int[] indices = Encoding.ASCII.GetBytes(wpText).Select(x => (int)x).ToArray();
                            using (Graphics bmgr = Graphics.FromImage(wpBm))
                            using (Bitmap txt = RenderTextFromSprite(classicFont, remapClassicFont, Size.Empty, indices, false, cropClassicFont))
                            {
                                int textOffsetX = (tileSize.Width - txt.Width) / 2;
                                int textOffsetY = (tileSize.Height - txt.Height) / 2;
                                int frameOffsetX = Math.Max(textOffsetX, -tileSize.Width) - 1;
                                int frameOffsetY = Math.Max(textOffsetY, -tileSize.Height) - 1;
                                int frameWidth = Math.Min(txt.Width + 2, tileSize.Width * 3);
                                int frameHeight = Math.Min(txt.Height + 2, tileSize.Height * 3);
                                Rectangle frameRect = new Rectangle(frameOffsetX, frameOffsetY, frameWidth, frameHeight);
                                Rectangle textRect = new Rectangle(textOffsetX, textOffsetY, txt.Width, txt.Height);
                                bmgr.FillRectangle(baseBackgroundBrush, frameRect);
                                bmgr.DrawImage(txt, textRect, 0, 0, txt.Width, txt.Height, GraphicsUnit.Pixel);
                            }
                        }
                        Globals.TheShapeCacheManager.AddImage(wpId, wpBm);
                        Rectangle paintRect = new Rectangle(paintBounds.Location.X, paintBounds.Location.Y, wpBm.Width, wpBm.Height);
                        graphics.DrawImage(wpBm, paintRect, 0, 0, wpBm.Width, wpBm.Height, GraphicsUnit.Pixel, imageAttributes);
                        
                    }
                }
            }
        }

        public static void RenderAllBuildingEffectRadiuses(Graphics graphics, Map map, Rectangle visibleCells, Size tileSize, int effectRadius, Building selected)
        {
            foreach ((Point topLeft, Building building) in map.Buildings.OfType<Building>()
                .Where(b => (b.Occupier.Type.Flag & BuildingTypeFlag.GapGenerator) != BuildingTypeFlag.None))
            {
                RenderBuildingEffectRadius(graphics, visibleCells, tileSize, effectRadius, building, topLeft, selected);
            }
        }

        public static void RenderBuildingEffectRadius(Graphics graphics, Rectangle visibleCells, Size tileSize, int effectRadius, Building building, Point topLeft, Building selected)
        {
            if ((building.Type.Flag & BuildingTypeFlag.GapGenerator) != BuildingTypeFlag.GapGenerator)
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
                float alphaFactor = 1.0f;
                if (building.IsPreview)
                {
                    alphaFactor *= Globals.PreviewAlphaFloat;
                }
                if (!building.IsPrebuilt)
                {
                    alphaFactor *= Globals.UnbuiltAlphaFloat;
                }
                int alphaFactorInt = (int)Math.Round(alphaFactor * 256).Restrict(0, 255);
                Color alphacorr = Color.FromArgb(alphaFactorInt, circleColor);
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
            bool isJammer = unit.Type.Flag.HasFlag(UnitTypeFlag.IsJammer);
            bool isGapGen = unit.Type.Flag.HasFlag(UnitTypeFlag.IsGapGenerator);
            if (!isJammer && !isGapGen)
            {
                return;
            }
            ITeamColor tc = Globals.TheTeamColorManager[unit.House?.BuildingTeamColor];
            Color circleColor = Globals.TheTeamColorManager.GetBaseColor(tc?.Name);
            float alphaFactor = 1.0f;
            if (unit.IsPreview)
            {
                alphaFactor *= Globals.PreviewAlphaFloat;
            }
            int alphaFactorInt = (int)Math.Round(alphaFactor * 256).Restrict(0, 255);
            Color alphacorr = Color.FromArgb(alphaFactorInt, circleColor);
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
            int[] wpReveal1 = plugin.GetRevealRadiusForWaypoints(false);
            int[] wpReveal2 = plugin.GetRevealRadiusForWaypoints(true);
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

        public static void RenderCellTriggersSoft(Graphics graphics, GameInfo gameInfo, Map map, Rectangle visibleCells, Size tileSize, params string[] specifiedToExclude)
        {
            RenderCellTriggers(graphics, gameInfo, map, visibleCells, tileSize, Color.Black, Color.Silver, Color.White, 0.75f, false, true, specifiedToExclude);
        }

        public static void RenderCellTriggersHard(Graphics graphics, GameInfo gameInfo, Map map, Rectangle visibleCells, Size tileSize, params string[] specifiedToExclude)
        {
            RenderCellTriggers(graphics, gameInfo, map, visibleCells, tileSize, Color.Black, Color.Silver, Color.White, 1, false, true, specifiedToExclude);
        }

        public static void RenderCellTriggersSelected(Graphics graphics, GameInfo gameInfo, Map map, Rectangle visibleCells, Size tileSize, params string[] specifiedToDraw)
        {
            RenderCellTriggers(graphics, gameInfo, map, visibleCells, tileSize, Color.Black, Color.FromArgb(0xFF, 0xC0, 0xC0, 0x00), Color.Yellow, 1, true, false, specifiedToDraw);
        }

        public static void RenderCellTriggers(Graphics graphics, GameInfo gameInfo, Map map, Rectangle visibleCells, Size tileSize, Color fillColor, Color borderColor, Color textColor, double alphaAdjust, bool thickborder, bool excludeSpecified, params string[] specified)
        {
            string classicFont = null;
            bool cropClassicFont = false;
            TeamRemap remapClassicFont = null;
            if (Globals.TheTilesetManager is TilesetManagerClassic tsmc && Globals.TheTeamColorManager is TeamRemapManager trm)
            {
                classicFont = gameInfo.GetClassicFontInfo(ClassicFont.CellTriggers, tsmc, trm, textColor, out cropClassicFont, out remapClassicFont);
            }
            // For bounds, add one more cell to get all borders showing.
            Rectangle boundRenderCells = visibleCells;
            boundRenderCells.Inflate(1, 1);
            boundRenderCells.Intersect(map.Metrics.Bounds);
            HashSet<string> specifiedSet = new HashSet<string>(specified, StringComparer.OrdinalIgnoreCase);
            List<(Point p, CellTrigger cellTrigger)> toRender = new List<(Point p, CellTrigger cellTrigger)>();
            HashSet<string> toRenderSet = new HashSet<string>();
            List<(Point p, CellTrigger cellTrigger)> boundsToDraw = new List<(Point p, CellTrigger cellTrigger)>();
            foreach ((int cell, CellTrigger cellTrigger) in map.CellTriggers.OrderBy(c => c.Cell))
            {
                int x = cell % map.Metrics.Width;
                int y = cell / map.Metrics.Width;
                if (!boundRenderCells.Contains(x, y))
                {
                    continue;
                }
                bool contains = specifiedSet.Contains(cellTrigger.Trigger);
                if (contains && excludeSpecified || !contains && !excludeSpecified)
                {
                    continue;
                }
                // Allow better alpha control by detecting previews but not using that alpha.
                bool isPreview = cellTrigger.IsPreview;
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
            int sizeW = tileSize.Width;
            int sizeH = tileSize.Height;
            // Actual balance is fixed; border is 1, text is 1/2, background is 3/8. The original alpha inside the given colors is ignored.
            // Should probably rewrite this to paint text as opaque on 6/8 alpha background, then paint that as 50% alpha and add solid border,
            // and then paint that with the final adjusted alpha factor.
            fillColor = ApplyAlpha(fillColor, 0x60, alphaAdjust);
            borderColor = ApplyAlpha(borderColor, 0xFF, alphaAdjust);
            textColor = ApplyAlpha(textColor, 0x80, alphaAdjust);
            // for classic fonts
            Color adjustColor = ApplyAlpha(Color.White, 0x80, alphaAdjust);
            // Preview versions
            Color previewFillColor = ApplyAlpha(fillColor, 0x60, alphaAdjust / 2);
            Color previewBorderColor = ApplyAlpha(borderColor, 0xFF, alphaAdjust / 2);
            Color previewTextColor = ApplyAlpha(textColor, 0x80, alphaAdjust / 2);
            // for classic fonts
            Color previewAdjustColor = ApplyAlpha(Color.White, 0x80, alphaAdjust / 2);

            // Render each trigger once, and just paint the rendered image multiple times.
            Dictionary<string, Bitmap> backRenders = new Dictionary<string, Bitmap>();
            Dictionary<string, Bitmap> renders = new Dictionary<string, Bitmap>();
            double tileScaleHor = sizeW / 128.0;
            Rectangle tileBounds = new Rectangle(0, 0, sizeW, sizeH);
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
                    Rectangle textBounds = new Rectangle(Point.Empty, tileBounds.Size);
                    string trId = "trigger_" + trigger + "_" + ((uint)(isPreview ? previewTextColor : textColor).ToArgb()).ToString("X4");
                    string bgId = "trig_bg_" + trigger + "_" + ((uint)(isPreview ? previewFillColor : fillColor).ToArgb()).ToString("X4");

                    Bitmap trigbm = Globals.TheShapeCacheManager.GetImage(trId);
                    if (trigbm == null)
                    {
                        trigbm = new Bitmap(sizeW, sizeH);
                        using (Graphics trigctg = Graphics.FromImage(trigbm))
                        {
                            SetRenderSettings(trigctg, classicFont == null);
                            StringFormat stringFormat = new StringFormat
                            {
                                Alignment = StringAlignment.Center,
                                LineAlignment = StringAlignment.Center
                            };
                            using (Bitmap textBm = new Bitmap(sizeW, sizeH))
                            {
                                using (Graphics textGr = Graphics.FromImage(textBm))
                                {
                                    if (classicFont == null)
                                    {
                                        using (Font font = trigctg.GetAdjustedFont(text, SystemFonts.DefaultFont, textBounds.Width, textBounds.Height,
                                            Math.Max(1, (int)Math.Round(24 * tileScaleHor)), Math.Max(1, (int)Math.Round(48 * tileScaleHor)), stringFormat, true))
                                        {
                                            SetRenderSettings(trigctg, true);
                                            // If not set, the text will have ugly black fades at the edges.
                                            textGr.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
                                            textGr.DrawString(text, font, isPreview ? prevCellTriggersBrush : cellTriggersBrush, textBounds, stringFormat);
                                        }
                                    }
                                    else
                                    {
                                        int[] indices = Encoding.ASCII.GetBytes(text).Select(x => (int)x).ToArray();
                                        using (ImageAttributes imageAttributes = new ImageAttributes())
                                        {
                                            imageAttributes.SetColorMatrix(GetColorMatrix(isPreview ? previewAdjustColor : adjustColor, 1.0f, 1.0f));
                                            using (Bitmap txt = RenderTextFromSprite(classicFont, remapClassicFont, tileBounds.Size, indices, false, cropClassicFont))
                                            {
                                                Rectangle paintBounds = new Rectangle(Point.Empty, tileSize);
                                                textGr.DrawImage(txt, textBounds, 0, 0, tileBounds.Width, tileBounds.Height, GraphicsUnit.Pixel, imageAttributes);
                                            }
                                        }
                                    }
                                }
                                trigctg.DrawImage(textBm, 0, 0);
                            }
                        }
                        Globals.TheShapeCacheManager.AddImage(trId, trigbm);
                    }
                    Bitmap fillbm = Globals.TheShapeCacheManager.GetImage(bgId);
                    if (fillbm == null)
                    {
                        fillbm = new Bitmap(sizeW, sizeH);
                        using (Graphics fillctg = Graphics.FromImage(fillbm))
                        {
                            SetRenderSettings(fillctg, classicFont == null);
                            fillctg.FillRectangle(isPreview ? prevCellTriggersBackgroundBrush : cellTriggersBackgroundBrush, textBounds);
                            // Clear background under text to make it more transparent. There are probably more elegant ways to do this, but this works.
                            RegionData textInline = ImageUtils.GetOutline(new Size(sizeW, sizeH), trigbm, 0.00f, (byte)Math.Max(0, textCol.A - 1), false);
                            using (Region clearArea = new Region(textInline))
                            using (Brush clear = new SolidBrush(Color.Transparent))
                            {
                                fillctg.CompositingMode = CompositingMode.SourceCopy;
                                fillctg.FillRegion(clear, clearArea);
                                fillctg.CompositingMode = CompositingMode.SourceOver;
                            }
                        }
                        Globals.TheShapeCacheManager.AddImage(bgId, fillbm);
                    }
                    backRenders.Add(trigger, fillbm);
                    renders.Add(trigger, trigbm);
                }
            }

            var backupCompositingQuality = graphics.CompositingQuality;
            var backupInterpolationMode = graphics.InterpolationMode;
            var backupSmoothingMode = graphics.SmoothingMode;
            var backupPixelOffsetMode = graphics.PixelOffsetMode;
            SetRenderSettings(graphics, classicFont == null);

            foreach ((Point p, CellTrigger cellTrigger) in toRender)
            {
                bool isPreview = cellTrigger.IsPreview;
                string requestName = cellTrigger.Trigger + "=" + (isPreview ? 'P' : 'N');
                if (backRenders.TryGetValue(requestName, out Bitmap fillCtBm))
                {
                    graphics.DrawImage(fillCtBm, p.X * tileSize.Width, p.Y * tileSize.Height);
                }
            }

            float borderSize = Math.Max(0.5f, tileSize.Width / 60.0f);
            float thickBorderSize = Math.Max(1f, tileSize.Width / 20.0f);
            using (Pen prevBorderPen = new Pen(previewBorderColor, thickborder ? thickBorderSize : borderSize))
            using (Pen borderPen = new Pen(borderColor, thickborder ? thickBorderSize : borderSize))
            {
                foreach ((Point p, CellTrigger cellTrigger) in boundsToDraw)
                {
                    bool isPreview = cellTrigger.IsPreview;
                    Rectangle bounds = new Rectangle(new Point(p.X * tileSize.Width, p.Y * tileSize.Height), tileSize);
                    graphics.DrawRectangle(isPreview ? prevBorderPen : borderPen, bounds);
                }
            }
            foreach ((Point p, CellTrigger cellTrigger) in toRender)
            {
                bool isPreview = cellTrigger.IsPreview;
                string requestName = cellTrigger.Trigger + "=" + (isPreview ? 'P' : 'N');
                if (renders.TryGetValue(requestName, out Bitmap ctBm))
                {
                    graphics.DrawImage(ctBm, p.X * tileSize.Width, p.Y * tileSize.Height);
                }
            }
            graphics.CompositingQuality = backupCompositingQuality;
            graphics.InterpolationMode = backupInterpolationMode;
            graphics.SmoothingMode = backupSmoothingMode;
            graphics.PixelOffsetMode = backupPixelOffsetMode;
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

        /// <summary>
        /// Paints the passability grid onto the map.
        /// </summary>
        /// <param name="graphics">Graphics to paint on.</param>
        /// <param name="plugin">Game plugin</param>
        /// <param name="templates">The map data itself</param>
        /// <param name="technos">If given, draws a green grid on the locations of the technos in the given set.</param>
        /// <param name="tileSize">Tile size</param>
        /// <param name="visibleCells">If given, only cells in the given area are marked.</param>
        /// <param name="ignoreCells">Cells to completely ignore during the drawing operation.</param>
        /// <param name="forPreview">Indicates this is painted for placement preview purposes, meaning colours with their alpha set to 0 are restored and also handled.</param>
        /// <param name="soft">True to paint the hashing with only 25% alpha instead of the usual 50%.</param>
        public static void RenderHashAreas(Graphics graphics, IGamePlugin plugin, CellGrid<Template> templates, OccupierSet<ICellOccupier> technos, Size tileSize, Rectangle visibleCells, HashSet<Point> ignoreCells, bool forPreview, bool soft)
        {
            // Check which cells need to be marked.
            LandType clearLand = LandType.Clear;
            // Fetch the terrain type for clear terrain on this theater.
            IEnumerable<Point> points = visibleCells.Points();
            TemplateType clear = plugin.Map.TemplateTypes.Where(t => t.Flag.HasFlag(TemplateTypeFlag.Clear)).FirstOrDefault();
            clearLand = clear.LandTypes.Length > 0 ? clear.LandTypes[0] : LandType.Clear;
            HashSet<LandType> usedLandTypes = plugin.Map.UsedLandTypes;
            if (technos != null && technos.Count() == 0)
            {
                technos = null;
            }
            if (templates != null && templates.Length == 0)
            {
                templates = null;
            }
            if (technos == null && templates == null)
            {
                return;
            }
            // Caching this in advance for all types.
            LandType[] landTypes = (LandType[])Enum.GetValues(typeof(LandType));
            int tileWidth = tileSize.Width;
            int tileHeight = tileSize.Height;
            float lineSize = tileWidth / 16.0f;
            int lineOffsetW = tileWidth / 4;
            int lineOffsetH = tileHeight / 4;
            Dictionary<LandType, Color> landColorsMapping = GetLandColorsMapping();
            // If the classic sprite exist, use that and recolour it. Since it's fetched
            // with extension, this will simply not return anything in Remastered mode.
            Globals.TheTilesetManager.GetTileData("trans.icn", 0, out Tile tile);
            // -1 and 0 are used for respectively partially-filled infantry cells, and fully filled techno cells.
            // But including them in the loop is a lot simpler than extracting the loop process into a function.
            for (int i = -1; i < landTypes.Length; i++)
            {
                LandType landType = LandType.None;
                Color curCol;
                // Techno indication hijacks LandType.None just because it's in this loop.
                bool forTechnos = i <= 0 && technos != null;
                bool forTechnosPart = forTechnos && i < 0;
                bool forTechnosFull = forTechnos && i == 0;
                if (forTechnosPart)
                {
                    curCol = Globals.HashColorTechnoPart;
                }
                else if (forTechnosFull)
                {
                    curCol = Globals.HashColorTechnoFull;
                }
                else
                {
                    if (i <= 0 || templates == null)
                    {
                        continue;
                    }
                    landType = landTypes[i];
                    if (!usedLandTypes.Contains(landType) || !landColorsMapping.TryGetValue(landType, out curCol))
                    {
                        continue;
                    }
                }
                // Unless it's for a placement preview, terrain types with a colour that is fully transparent are completely skipped.
                if (curCol.A == 0 && !forPreview)
                {
                    continue;
                }
                curCol = Color.FromArgb(255, curCol);
                string hashId = "hashing_" + tileWidth + "x" + tileHeight + "_" + ((uint)curCol.ToArgb()).ToString("X4");
                Bitmap hashBmp = Globals.TheShapeCacheManager.GetImage(hashId);
                if (hashBmp == null)
                {
                    hashBmp = GenerateLinesBitmap(tile, tileWidth, tileHeight, curCol, lineSize, lineOffsetW, lineOffsetH, graphics);
                    Globals.TheShapeCacheManager.AddImage(hashId, hashBmp);
                }
                using (ImageAttributes imageAttributes = new ImageAttributes())
                {
                    imageAttributes.SetColorMatrix(GetColorMatrix(Color.White, 1.0f, soft ? 0.25f : 0.50f));
                    for (int y = visibleCells.Y; y < visibleCells.Bottom; ++y)
                    {
                        for (int x = visibleCells.X; x < visibleCells.Right; ++x)
                        {
                            if (ignoreCells != null && ignoreCells.Contains(new Point(x, y)))
                            {
                                continue;
                            }
                            bool renderTerrainType = true;
                            if (technos != null)
                            {
                                ICellOccupier techno = technos[y, x];
                                // Skip if it's the techno-loop and there's no techno,
                                // or if it's not the techno-loop and there is a techno (to avoid overlap).
                                if ((techno != null && !forTechnos) || (techno == null && forTechnos))
                                {
                                    continue;
                                }
                                if (forTechnos && techno != null)
                                {
                                    renderTerrainType = false;
                                    bool incomplete = techno is InfantryGroup ifg && ifg.Infantry.Any(inf => inf == null);
                                    if (incomplete && forTechnosFull || !incomplete && forTechnosPart)
                                    {
                                        continue;
                                    }
                                }
                            }
                            if (renderTerrainType)
                            {
                                Template template = templates[y, x];
                                LandType land = LandType.None;
                                land = template == null ? clearLand : template.Type.GetLandType(template.Icon);
                                // Only handle currently looped one
                                if (land != landType)
                                {
                                    continue;
                                }
                            }
                            graphics.DrawImage(hashBmp, new Rectangle(tileWidth * x, tileHeight * y, tileWidth, tileHeight),
                                0, 0, hashBmp.Width, hashBmp.Height, GraphicsUnit.Pixel, imageAttributes);
                        }
                    }
                }
            }
        }

        private static Dictionary<LandType, Color> GetLandColorsMapping()
        {
            return new Dictionary<LandType, Color>
            {
                {LandType.Clear, Globals.HashColorLandClear }, // [Clear] Normal clear terrain.
                {LandType.Beach, Globals.HashColorLandBeach }, // [Beach] Sandy beach. Can't be built on.
                {LandType.Rock,  Globals.HashColorLandRock }, //  [Rock]  Impassable terrain.
                {LandType.Road,  Globals.HashColorLandRoad }, //  [Road]  Units move faster on this terrain.
                {LandType.Water, Globals.HashColorLandWater }, // [Water] Ships can travel over this.
                {LandType.River, Globals.HashColorLandRiver }, // [River] Ships normally can't travel over this.
                {LandType.Rough, Globals.HashColorLandRough }, // [Rough] Rough terrain. Can't be built on
            };
        }

        /// <summary>
        /// Generates a cell filled with diagonal line hashing. If a tile is given, and it matches the requested size,
        /// it used to generate the result by recolouring all its non-transparent pixels with the requested color.
        /// </summary>
        /// <param name="tile">Tile to use graphics from, if available.</param>
        /// <param name="width">Width of the resulting image.</param>
        /// <param name="height">Height of the resulting image.</param>
        /// <param name="color">Color of the resulting image.</param>
        /// <param name="lineSize">Thickness of the lines to paint.</param>
        /// <param name="lineOffsetW">Horizontal offset between each line.</param>
        /// <param name="lineOffsetH">Vertical offset between each line.</param>
        /// <param name="g">Graphics object to copy render settings from.</param>
        /// <returns>The image with diagonal lines, in the given color.</returns>
        private static Bitmap GenerateLinesBitmap(Tile tile, int width, int height, Color color, float lineSize, int lineOffsetW, int lineOffsetH, Graphics g)
        {
            if (tile != null && tile.Image != null)
            {
                byte colR = color.R;
                byte colG = color.G;
                byte colB = color.B;
                byte[] imgData = ImageUtils.GetImageData(tile.Image, out int stride, PixelFormat.Format32bppArgb, true);
                // Replace colors, retain alpha.
                for (int i = 0; i < imgData.Length; i += 4)
                {
                    // Only replace non-transparent pixels
                    if (imgData[i + 3] == 0)
                    {
                        continue;
                    }
                    // ARGB = [BB GG RR AA]
                    imgData[i + 0] = colB;
                    imgData[i + 1] = colG;
                    imgData[i + 2] = colR;
                }
                if (tile.Image.Width == width && tile.Image.Height == height)
                {
                    return ImageUtils.BuildImage(imgData, width, height, stride, PixelFormat.Format32bppArgb, null, null);
                }
                else
                {
                    using (Bitmap tmp = ImageUtils.BuildImage(imgData, tile.Image.Width, tile.Image.Height, stride, PixelFormat.Format32bppArgb, null, null))
                    {
                        Bitmap bm = new Bitmap(width, height);
                        using (Graphics bmg = Graphics.FromImage(bm))
                        {
                            // Pixel upscale for classic graphics
                            SetRenderSettings(bmg, false);
                            bmg.DrawImage(tmp, new Rectangle(0, 0, width, height), 0, 0, tmp.Width, tmp.Height, GraphicsUnit.Pixel);
                        }
                        return bm;
                    }
                }
            }
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

        public static void SetRenderSettings(Graphics g, bool smooth)
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

        private static Bitmap RenderTextFromSprite(string fontsprite, TeamRemap remap, Size bounds, int[] shapes, bool wrap, bool cropSpacingToOne)
        {
            int nrOfChars = shapes.Length;
            if (nrOfChars == 0)
            {
                return null;
            }
            Tile[] tiles = new Tile[nrOfChars];
            int[] offsets = new int[nrOfChars];
            int[] widths = new int[nrOfChars];
            int lineLength = wrap ? 0 : bounds.Width;
            int minTop = -1;
            int maxHeight = bounds.Height;
            int lineSize = 0;
            // Make sure the width of the target image is at least as large as the
            // largest character, to avoid endless loops on line breaks.
            for (int i = 0; i < nrOfChars; ++i)
            {
                int charIndex = shapes[i];
                if (Globals.TheTilesetManager.GetTeamColorTileData(fontsprite, charIndex, remap, true, out Tile character))
                {
                    if (character.Image == null)
                    {
                        continue;
                    }
                    tiles[i] = character;
                    // If cropping is requested, crop character.
                    int charWidth;
                    if (cropSpacingToOne)
                    {
                        Rectangle tileBounds = character.OpaqueBounds;
                        if (tileBounds.Width != 0)
                        {
                            offsets[i] = tileBounds.X;
                            charWidth = tileBounds.Width + 1;
                        }
                        else
                        {
                            charWidth = character.Image.Width;
                        }
                        minTop = minTop == -1 ? tileBounds.Top : Math.Min(minTop, tileBounds.Top);
                        maxHeight = Math.Max(maxHeight, tileBounds.Bottom);
                    }
                    else
                    {
                        charWidth = character.Image.Width;
                        maxHeight = Math.Max(maxHeight, character.Image.Height);
                    }
                    if (i == nrOfChars - 1)
                    {
                        charWidth -= cropSpacingToOne ? 1 : (character.Image.Width - character.OpaqueBounds.Right);
                    }
                    if (wrap)
                    {
                        lineLength = Math.Max(lineLength, charWidth);
                    }
                    else
                    {
                        lineLength += charWidth;
                    }
                    widths[i] = charWidth;
                    lineSize = Math.Max(lineSize, character.Image.Height);
                }
            }
            minTop = Math.Max(minTop, 0);
            int lineHeight = 0;
            int usedWidth = 0;
            int curWidth = 0;
            if (lineLength == 0 || maxHeight - minTop == 0)
            {
                return new Bitmap(2, 2);
            }
            Bitmap bitmap = new Bitmap(lineLength, maxHeight - minTop, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                for (int i = 0; i < nrOfChars; ++i)
                {
                    Tile curChar = tiles[i];
                    if (curChar == null)
                    {
                        continue;
                    }
                    usedWidth = Math.Max(usedWidth, curWidth);
                    if (curWidth >= lineLength)
                    {
                        lineHeight += lineSize;
                        if (lineHeight >= maxHeight)
                        {
                            break;
                        }
                        usedWidth = Math.Max(usedWidth, curWidth);
                        curWidth = 0;
                    }
                    if (curChar.Image == null)
                    {
                        continue;
                    }
                    int nextWidth = curWidth + widths[i];
                    if (nextWidth > lineLength)
                    {
                        lineHeight += lineSize;
                        if (lineHeight >= maxHeight)
                        {
                            break;
                        }
                        curWidth = 0;
                    }
                    else
                    {
                        usedWidth = Math.Max(usedWidth, nextWidth);
                    }
                    g.DrawImage(curChar.Image, curWidth - offsets[i], lineHeight - minTop);
                    curWidth = nextWidth;
                }
            }
            lineHeight += lineSize;
            // Chop off if it exceeds bounds.
            if (!bounds.IsEmpty && (lineLength > bounds.Width || maxHeight > bounds.Height || lineHeight < bounds.Height || usedWidth < bounds.Width))
            {
                Bitmap curBm = bitmap;
                Bitmap newBm = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
                int height = Math.Max((bounds.Height - lineHeight) / 2, 0);
                int width = Math.Max((bounds.Width - usedWidth) / 2, 0);
                using (Graphics g = Graphics.FromImage(newBm)) {
                    g.DrawImage(curBm, width, height);
                }
                try { curBm.Dispose(); }
                catch { /* ignore */ }
                bitmap = newBm;
            }
            return bitmap;
        }

        private static ColorMatrix GetColorMatrix(Color tint, float brightnessModifier, float alphaModifier)
        {
            return new ColorMatrix(new float[][]
            {
                new float[] {tint.R * brightnessModifier / 255.0f, 0, 0, 0, 0},
                new float[] {0, tint.G * brightnessModifier / 255.0f, 0, 0, 0},
                new float[] {0, 0, tint.B * brightnessModifier / 255.0f, 0, 0},
                new float[] {0, 0, 0, tint.A * alphaModifier / 255.0f, 0},
                new float[] {0, 0, 0, 0, 1},
            });
        }
    }
}
