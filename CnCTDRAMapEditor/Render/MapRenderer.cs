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
using System.Numerics;

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

        private static readonly Point[] TurretAdjust = new Point[]
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
            var overlappingRenderList = new List<(Rectangle, Action<Graphics>)>();
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
                        Render(map.Theater, topLeft, tileSize, tileScale, smudge).Item2(graphics);
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
                        Render(map.Theater, topLeft, tileSize, tileScale, smudge).Item2(graphics);
                    }
                }
            }

            if ((layers & MapLayerFlag.OverlayAll) != MapLayerFlag.None)
            {
                foreach (var topLeft in renderLocations())
                {
                    var overlay = map.Overlay[topLeft];
                    if (overlay == null)
                    {
                        continue;
                    }
                    if ((overlay.Type.IsResource && ((layers & MapLayerFlag.Resources) != MapLayerFlag.None)) ||
                        (overlay.Type.IsWall && ((layers & MapLayerFlag.Walls) != MapLayerFlag.None)) ||
                        ((layers & MapLayerFlag.Overlay) != MapLayerFlag.None))
                    {
                        Render(gameType, map.Theater, tiberiumOrGoldTypes, gemTypes, topLeft, tileSize, tileScale, overlay).Item2(graphics);
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
                    overlappingRenderList.Add(Render(gameType, map.Theater, topLeft, tileSize, tileScale, terrain));
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
                    overlappingRenderList.Add(Render(gameType, map.Theater, topLeft, tileSize, tileScale, building));
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
                        overlappingRenderList.Add(Render(map.Theater, topLeft, tileSize, infantry, (InfantryStoppingType)i));
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
                    overlappingRenderList.Add(Render(gameType, map.Theater, topLeft, tileSize, unit));
                }
            }
            foreach (var (location, renderer) in overlappingRenderList.Where(x => !x.Item1.IsEmpty).OrderBy(x => x.Item1.Bottom))
            {
                renderer(graphics);
            }
            if ((layers & MapLayerFlag.Waypoints) != MapLayerFlag.None)
            {
                // todo avoid overlapping waypoints of the same type?
                HashSet<int> handledPoints = new HashSet<int>();
                TeamColor[] flagColors = map.FlagColors.ToArray();
                foreach (Waypoint waypoint in map.Waypoints)
                {
                    if (!waypoint.Point.HasValue || (locations != null && !locations.Contains(waypoint.Point.Value)))
                    {
                        continue;
                    }
                    Render(gameType, map.BasicSection.SoloMission, map.Theater, tileSize, flagColors, waypoint).Item2(graphics);
                }
            }
        }

        public static void Render(GameType gameType, Map map, Graphics graphics, ISet<Point> locations, MapLayerFlag layers)
        {
            Render(gameType, map, graphics, locations, layers, Globals.MapTileScale);
        }

        public static (Rectangle, Action<Graphics>) Render(TheaterType theater, Point topLeft, Size tileSize, double tileScale, Smudge smudge)
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

        public static (Rectangle, Action<Graphics>) Render(GameType gameType, TheaterType theater, OverlayType[] tiberiumOrGoldTypes, OverlayType[] gemTypes, Point topLeft, Size tileSize, double tileScale, Overlay overlay)
        {
            string name;
            if (overlay.Type.IsGem && gemTypes != null)
            {
                name = gemTypes[new Random(randomSeed ^ topLeft.GetHashCode()).Next(gemTypes.Length)].Name;
            }
            else if (overlay.Type.IsTiberiumOrGold && tiberiumOrGoldTypes != null)
            {
                name = tiberiumOrGoldTypes[new Random(randomSeed ^ topLeft.GetHashCode()).Next(tiberiumOrGoldTypes.Length)].Name;
            }
            else
            {
                name = overlay.Type.GraphicsSource;
            }
            int icon;
            if (overlay.Type.IsConcrete || overlay.Type.IsResource || overlay.Type.IsWall)
            {
                icon = overlay.Icon;
            }
            else
            {
                icon = overlay.Type.ForceTileNr == -1 ? overlay.Icon : overlay.Type.ForceTileNr;
            }
            bool isTeleport = gameType == GameType.SoleSurvivor && overlay.Type == SoleSurvivor.OverlayTypes.Teleport && Globals.AdjustSSTeleports;
            // For Decoration types, generate dummy if not found.
            if (Globals.TheTilesetManager.GetTileData(theater.Tilesets, name, icon, out Tile tile, (overlay.Type.Flag & OverlayTypeFlag.Decoration) != 0, false))
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

        public static (Rectangle, Action<Graphics>) Render(GameType gameType, TheaterType theater, Point topLeft, Size tileSize, double tileScale, Terrain terrain)
        {
            string tileName = terrain.Type.GraphicsSource;
            if (!Globals.TheTilesetManager.GetTileData(theater.Tilesets, tileName, terrain.Type.DisplayIcon, out Tile tile))
            {
                Debug.Print(string.Format("Terrain {0} ({1}) not found", tileName, terrain.Type.DisplayIcon));
                return (Rectangle.Empty, (g) => { });
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
            return (terrainBounds, render);
        }

        public static (Rectangle, Action<Graphics>) Render(GameType gameType, TheaterType theater, Point topLeft, Size tileSize, double tileScale, Building building)
        {
            var tint = building.Tint;
            var icon = 0;
            int maxIcon = 0;
            int damageIcon = 0;
            int collapseIcon = 0;
            bool hasCollapseFrame = false;
            // In TD, damage is when BELOW the threshold. In RA, it's ON the threshold.
            int healthyMin = gameType == GameType.RedAlert ? 128 : 127;
            // Only fetch if damaged. BuildingType.IsSingleFrame is an override for the RA mines. Everything else works with one simple logic.
            if (building.Strength <= healthyMin && !building.Type.IsSingleFrame)
            {
                maxIcon = Globals.TheTilesetManager.GetTileDataLength(theater.Tilesets, building.Type.Tilename);
                hasCollapseFrame = (gameType == GameType.TiberianDawn || gameType == GameType.SoleSurvivor) && maxIcon > 1 && maxIcon % 2 == 1;
                damageIcon = maxIcon / 2;
                collapseIcon = hasCollapseFrame ? maxIcon - 1 : damageIcon;
            }
            if (building.Type.HasTurret)
            {
                icon = BodyShape[Facing32[building.Direction.ID]];
                if (building.Strength <= healthyMin)
                {
                    icon += damageIcon;
                }
            }
            else
            {
                if (building.Strength <= 1)
                {
                    icon = collapseIcon;
                }
                else if (building.Strength <= healthyMin)
                {
                    icon = damageIcon;
                }
            }
            TeamColor tc = building.Type.CanRemap ? Globals.TheTeamColorManager[building.House.BuildingTeamColor] : null;
            if (Globals.TheTilesetManager.GetTeamColorTileData(theater.Tilesets, building.Type.Tilename, icon, tc, out Tile tile))
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
                return (buildingBounds, render);
            }
            else
            {
                Debug.Print(string.Format("Building {0} ({1}) not found", building.Type.Name, icon));
                return (Rectangle.Empty, (g) => { });
            }
        }

        public static (Rectangle, Action<Graphics>) Render(TheaterType theater, Point topLeft, Size tileSize, Infantry infantry, InfantryStoppingType infantryStoppingType)
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
                return (virtualBounds, render);
            }
            else
            {
                Debug.Print(string.Format("Infantry {0} ({1}) not found", infantry.Type.Name, icon));
                return (Rectangle.Empty, (g) => { });
            }
        }

        public static (Rectangle, Action<Graphics>) Render(GameType gameType, TheaterType theater, Point topLeft, Size tileSize, Unit unit)
        {
            int icon = 0;
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
                else
                {
                    icon = BodyShape[Facing32[unit.Direction.ID]];
                    if (unit.Type == TiberianDawn.UnitTypes.GunBoat)
                    {
                        // East facing is not actually possible to set in missions. This is just the turret facing.
                        // In TD, damage is when BELOW the threshold. In RA, it's ON the threshold.
                        if (unit.Strength < 128)
                            icon += 32;
                        if (unit.Strength < 64)
                            icon += 32;
                    }
                }
            }
            else if (gameType == GameType.RedAlert)
            {
                if (unit.Type.IsAircraft)
                {
                    if ((unit.Type == RedAlert.UnitTypes.Tran) ||
                        (unit.Type == RedAlert.UnitTypes.Heli) ||
                        (unit.Type == RedAlert.UnitTypes.Hind))
                    {
                        icon = BodyShape[Facing32[unit.Direction.ID]];
                    }
                    else
                    {
                        icon = BodyShape[Facing16[unit.Direction.ID] * 2] / 2;
                    }
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
                        icon = BodyShape[Facing16[unit.Direction.ID] * 2] >> 1;
                    }
                }
                else
                {
                    if ((unit.Type == RedAlert.UnitTypes.Ant1) ||
                        (unit.Type == RedAlert.UnitTypes.Ant2) ||
                        (unit.Type == RedAlert.UnitTypes.Ant3))
                    {
                        icon = ((BodyShape[Facing32[unit.Direction.ID]] + 2) / 4) & 0x07;
                    }
                    else
                    {
                        icon = BodyShape[Facing32[unit.Direction.ID]];
                    }
                }
            }

            string teamColor = null;
            if (unit.House != null)
            {
                if (!unit.House.OverrideTeamColors.TryGetValue(unit.Type.Name, out teamColor))
                {
                    teamColor = unit.House.UnitTeamColor;
                }
            }

            if (Globals.TheTilesetManager.GetTeamColorTileData(theater.Tilesets, unit.Type.Name, icon, Globals.TheTeamColorManager[teamColor], out Tile tile))
            {
                var location =
                    new Point(topLeft.X * tileSize.Width, topLeft.Y * tileSize.Height) +
                    new Size(tileSize.Width / 2, tileSize.Height / 2);
                var renderSize = unit.Type.GetRenderSize(tileSize);
                var renderBounds = new Rectangle(location - new Size(renderSize.Width / 2, renderSize.Height / 2), renderSize);
                Tile turretTile = null;
                if (unit.Type.HasTurret)
                {
                    var turretName = unit.Type.Name;
                    var turretIcon = icon + 32;
                    if (gameType == GameType.RedAlert)
                    {
                        if (unit.Type == RedAlert.UnitTypes.Phase)
                        {
                            turretIcon += 6;
                        }
                        else if (unit.Type == RedAlert.UnitTypes.MGG)
                        {
                            int mggIcon = icon / 2;
                            if (mggIcon >= 8)
                                mggIcon -= 8;
                            turretIcon = 32 + mggIcon;
                        }
                        else if (unit.Type == RedAlert.UnitTypes.Tesla)
                        {
                            turretIcon = 32;
                        }
                    }
                    if (unit.Type.IsVessel)
                    {
                        // Might implement this properly later.
                        turretName = null;
#if false && TODO
                        if (unit.Type == RedAlert.UnitTypes.Cruiser)
                        {
                            turretName = "TURR";
                        }
                        else if (unit.Type == RedAlert.UnitTypes.Destroyer)
                        {
                            turretName = "SSAM";
                        }
                        else if (unit.Type == RedAlert.UnitTypes.PTBoat)
                        {
                            turretName = "MGUN";
                        }
#endif
                        turretIcon = BodyShape[Facing32[unit.Direction.ID]];
                    }
                    if (turretName != null)
                        Globals.TheTilesetManager.GetTeamColorTileData(theater.Tilesets, turretName, turretIcon, Globals.TheTeamColorManager[teamColor], out turretTile);
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
                    g.DrawImage(tile.Image, renderBounds, 0, 0, tile.Image.Width, tile.Image.Height, GraphicsUnit.Pixel, imageAttributes);
                    if (turretTile != null)
                    {
                        Point turretAdjust = Point.Empty;
                        if (gameType == GameType.RedAlert)
                        {
                            if (unit.Type.IsVessel)
                            {
                                // TODO
                            }
                            else if (unit.Type == RedAlert.UnitTypes.Jeep)
                            {
                                turretAdjust.Y = -4;
                            }
                            else if (unit.Type == RedAlert.UnitTypes.MGG)
                            {
                                turretAdjust = TurretAdjust[Facing32[unit.Direction.ID]];
                            }
                            else if (unit.Type == RedAlert.UnitTypes.MRJammer)
                            {
                                turretAdjust.Y = -5;
                            }
                        }
                        else if (gameType == GameType.TiberianDawn || gameType == GameType.SoleSurvivor)
                        {
                            if (unit.Type == TiberianDawn.UnitTypes.Jeep ||
                                unit.Type == TiberianDawn.UnitTypes.Buggy ||
                                unit.Type == TiberianDawn.UnitTypes.MHQ)
                            {
                                turretAdjust.Y = -4;
                            }
                            else if (unit.Type == TiberianDawn.UnitTypes.SSM ||
                                     unit.Type == TiberianDawn.UnitTypes.MLRS)
                            {
                                turretAdjust = TurretAdjust[Facing32[unit.Direction.ID]];
                            }
                        }
                        var turretBounds = renderBounds;
                        turretBounds.Offset(
                            turretAdjust.X * tileSize.Width / Globals.PixelWidth,
                            turretAdjust.Y * tileSize.Height / Globals.PixelHeight
                        );
                        g.DrawImage(turretTile.Image, turretBounds, 0, 0, tile.Image.Width, tile.Image.Height, GraphicsUnit.Pixel, imageAttributes);
                    }
                }
                return (renderBounds, render);
            }
            else
            {
                Debug.Print(string.Format("Unit {0} ({1}) not found", unit.Type.Name, icon));
                return (Rectangle.Empty, (g) => { });
            }
        }

        public static (Rectangle, Action<Graphics>) Render(GameType gameType, bool soloMission, TheaterType theater, Size tileSize, TeamColor[] flagColors, Waypoint waypoint)
        {
            // Opacity is normally 0.5 for non-flag waypoint indicators, but is variable because the post-render
            // actions of the waypoints tool will paint a fully opaque version over the currently selected waypoint.
            //int mpId = Waypoint.GetMpIdFromFlag(waypoint.Flag);
            //float defaultOpacity = !soloMission && mpId >= 0 && mpId < flagColors.Length ? 1.0f : 0.5f;
            return Render(gameType, soloMission, theater, tileSize, flagColors, waypoint, 0.5f);
        }

        public static (Rectangle, Action<Graphics>) Render(GameType gameType, bool soloMission, TheaterType theater, Size tileSize, TeamColor[] flagColors, Waypoint waypoint, float transparencyModifier)
        {
            if (!waypoint.Point.HasValue)
            {
                return (Rectangle.Empty, (g) => { });
            }
            Point point = waypoint.Point.Value;
            string tileGraphics = "beacon";
            TeamColor teamColor = null;
            Color tint = waypoint.Tint;
            float brightness = 1.0f;
            int mpId = Waypoint.GetMpIdFromFlag(waypoint.Flag);
            if (!soloMission && mpId >= 0 && mpId < flagColors.Length)
            {
                tileGraphics = "flagfly";
                // Always paint flags as opaque.
                transparencyModifier = 1.0f;
                teamColor = flagColors[mpId];
            }
            if (gameType == GameType.SoleSurvivor && (waypoint.Flag & WaypointFlag.CrateSpawn) == WaypointFlag.CrateSpawn)
            {
                tileGraphics = "scrate";
                //tint = Color.FromArgb(waypoint.Tint.A, Color.Green);
                //brightness = 1.5f;
            }
            int icon = 0;
            if (Globals.TheTilesetManager.GetTeamColorTileData(theater.Tilesets, tileGraphics, icon, teamColor, out Tile tile, true, true))
            {
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
            else
            {
                Debug.Print(string.Format("Waypoint graphics {0} ({1}) not found", tileGraphics, icon));
                return (Rectangle.Empty, (g) => { });
            }
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
                Render(gameType, map.Theater, null, null, p, tileSize, tileScale, footballTerrain).Item2(graphics);
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
            TeamColor[] flagColors = map.FlagColors.ToArray();
            foreach (Waypoint wp in footballWayPoints)
            {
                Render(gameType, false, map.Theater, tileSize, flagColors, wp).Item2(graphics);
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
            if (building.Type.IsFake)
            {
                string fakeText = Globals.TheGameTextManager["TEXT_UI_FAKE"];
                using (var fakeBackgroundBrush = new SolidBrush(Color.FromArgb((forPreview ? 128 : 256) * 2 / 3, Color.Black)))
                using (var fakeTextBrush = new SolidBrush(Color.FromArgb(forPreview ? building.Tint.A : 255, Color.White)))
                {
                    using (var font = graphics.GetAdjustedFont(fakeText, SystemFonts.DefaultFont, buildingBounds.Width,
                        Math.Max(1, (int)(12 * tileScale)), Math.Max(1, (int)(24 * tileScale)), true))
                    {
                        var textBounds = graphics.MeasureString(fakeText, font, buildingBounds.Width, stringFormat);
                        var backgroundBounds = new RectangleF(buildingBounds.Location, textBounds);
                        graphics.FillRectangle(fakeBackgroundBrush, backgroundBounds);
                        graphics.DrawString(fakeText, font, fakeTextBrush, backgroundBounds, stringFormat);
                    }
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
                    using (var font = graphics.GetAdjustedFont(priText, SystemFonts.DefaultFont, buildingBounds.Width,
                        Math.Max(1, (int)(12 * tileScale)), Math.Max(1, (int)(24 * tileScale)), true))
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
            using (var technoTriggerBackgroundBrush = new SolidBrush(Color.FromArgb(96, Color.Black)))
            using (var technoTriggerBrush = new SolidBrush(color))
            using (var technoTriggerPen = new Pen(color, borderSize))
            {
                foreach (var (cell, techno) in map.Technos)
                {
                    var location = new Point(cell.X * tileSize.Width, cell.Y * tileSize.Height);
                    (string trigger, Rectangle bounds)[] triggers = null;
                    if (techno is Terrain terrain && !Trigger.IsEmpty(terrain.Trigger))
                    {
                        if ((layersToRender & MapLayerFlag.Terrain) == MapLayerFlag.Terrain)
                        {
                            triggers = new (string, Rectangle)[] { (terrain.Trigger, new Rectangle(location, terrain.Type.GetRenderSize(tileSize))) };
                        }
                    }
                    else if (techno is Building building && !Trigger.IsEmpty(building.Trigger))
                    {
                        if ((layersToRender & MapLayerFlag.Buildings) == MapLayerFlag.Buildings)
                        {
                            var size = new Size(building.Type.Size.Width * tileSize.Width, building.Type.Size.Height * tileSize.Height);
                            triggers = new (string, Rectangle)[] { (building.Trigger, new Rectangle(location, size)) };
                        }
                    }
                    else if (techno is Unit unit && !Trigger.IsEmpty(unit.Trigger))
                    {
                        if ((layersToRender & MapLayerFlag.Units) == MapLayerFlag.Units)
                        {
                            triggers = new (string, Rectangle)[] { (unit.Trigger, new Rectangle(location, tileSize)) };
                        }
                    }
                    else if (techno is InfantryGroup infantryGroup)
                    {
                        if ((layersToRender & MapLayerFlag.Infantry) == MapLayerFlag.Infantry)
                        {
                            List<(string, Rectangle)> infantryTriggers = new List<(string, Rectangle)>();
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
                                infantryTriggers.Add((infantry.Trigger, bounds));
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
                        foreach (var (trigger, bounds) in triggers.Where(x => toPick == null
                        || (excludePick && !x.trigger.Equals(toPick, StringComparison.OrdinalIgnoreCase))
                         || (!excludePick && x.trigger.Equals(toPick, StringComparison.OrdinalIgnoreCase))))
                        {
                            using (var font = graphics.GetAdjustedFont(trigger, SystemFonts.DefaultFont, bounds.Width,
                                Math.Max(1, (int)(12 * tileScale)), Math.Max(1, (int)(24 * tileScale)), true))
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
                using (var font = graphics.GetAdjustedFont(wpText, SystemFonts.DefaultFont, paintBounds.Width,
                    Math.Max(1, (int)(12 * tileScale)), Math.Max(1, (int)(30 * tileScale)), true))
                {
                    var textBounds = graphics.MeasureString(wpText, font, paintBounds.Width, stringFormat);
                    var backgroundBounds = new RectangleF(paintBounds.Location, textBounds);
                    backgroundBounds.Offset((paintBounds.Width - textBounds.Width) / 2.0f, paintBounds.Height - textBounds.Height);
                    graphics.FillRectangle(baseBackgroundBrush, backgroundBounds);
                    graphics.DrawString(wpText, font, baseTextBrush, backgroundBounds, stringFormat);
                }
            }
        }

        public static void RenderCellTriggers(Graphics graphics, Map map, Size tileSize, double tileScale, params String[] specifiedToExclude)
        {
            RenderCellTriggers(graphics, map, tileSize, tileScale, Color.Black, Color.White, Color.White, false, true, specifiedToExclude);
        }

        public static void RenderCellTriggers(Graphics graphics, Map map, Size tileSize, double tileScale, Color fillColor, Color borderColor, Color textColor, bool thickborder, bool excludeSpecified, params String[] specified)
        {
            float borderSize = Math.Max(0.5f, tileSize.Width / 60.0f);
            float thickBorderSize = Math.Max(1f, tileSize.Width / 20.0f);
            HashSet<String> specifiedSet = new HashSet<String>(specified, StringComparer.InvariantCultureIgnoreCase);
            using (var cellTriggersBackgroundBrush = new SolidBrush(Color.FromArgb(96, fillColor)))
            using (var cellTriggersBrush = new SolidBrush(Color.FromArgb(128, textColor)))
            using (var cellTriggerPen = new Pen(borderColor, thickborder ? thickBorderSize : borderSize))
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
                    graphics.FillRectangle(cellTriggersBackgroundBrush, textBounds);
                    graphics.DrawRectangle(cellTriggerPen, textBounds);
                    StringFormat stringFormat = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    var text = cellTrigger.Trigger;
                    using (var font = graphics.GetAdjustedFont(text, SystemFonts.DefaultFont, textBounds.Width,
                        Math.Max(1, (int)(24 * tileScale)), Math.Max(1, (int)(48 * tileScale)), true))
                    {
                        graphics.DrawString(text.ToString(), font, cellTriggersBrush, textBounds, stringFormat);
                    }
                }
            }
        }

        public static void RenderMapBoundaries(Graphics graphics, Map map, Size tileSize)
        {
            RenderMapBoundaries(graphics, map.Bounds, tileSize, Color.Cyan, false);
        }

        public static void RenderMapBoundaries(Graphics graphics, Rectangle bounds, Size tileSize, Color color, bool diagonals)
        {
            var boundsRect = Rectangle.FromLTRB(
                bounds.Left * tileSize.Width,
                bounds.Top * tileSize.Height,
                bounds.Right * tileSize.Width,
                bounds.Bottom * tileSize.Height
            );
            using (var boundsPen = new Pen(color, Math.Max(1f, tileSize.Width / 8.0f)))
            {
                graphics.DrawRectangle(boundsPen, boundsRect);
                if (diagonals)
                {
                    graphics.DrawLine(boundsPen, new Point(boundsRect.X, boundsRect.Y), new Point(boundsRect.Right, boundsRect.Bottom));
                    graphics.DrawLine(boundsPen, new Point(boundsRect.Right, boundsRect.Y), new Point(boundsRect.X, boundsRect.Bottom));
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
