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
using MobiusEditor.Controls;
using MobiusEditor.Event;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Render;
using MobiusEditor.Utility;
using MobiusEditor.Widgets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Tools
{
    public abstract class ViewTool : ITool
    {
        protected readonly IGamePlugin plugin;
        protected readonly Map map;

        protected readonly MapPanel mapPanel;
        protected readonly ToolStripStatusLabel statusLbl;
        protected readonly UndoRedoList<UndoRedoEventArgs> url;
        protected readonly NavigationWidget navigationWidget;

        protected virtual Map RenderMap => map;

        /// <summary> Layers that are important to this tool and need to be drawn last in the PostRenderMap process.</summary>
        protected abstract MapLayerFlag PriorityLayers { get; }
        /// <summary>
        /// Layers that are not painted by the PostRenderMap function on ViewTool level because they are handled
        /// at a specific point in the PostRenderMap override by the implementing tool.
        /// </summary>
        protected abstract MapLayerFlag ManuallyHandledLayers { get; }

        private MapLayerFlag layers;
        public MapLayerFlag Layers
        {
            get => layers;
            set
            {
                if (layers != value)
                {
                    MapLayerFlag modifiedBits = layers ^ value;
                    bool refreshIndicatorsOnly = (modifiedBits & ~MapLayerFlag.Indicators) == 0;
                    layers = value;
                    Invalidate(refreshIndicatorsOnly);
                    RefreshPreviewPanel();
                }
            }
        }

        public ViewTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> url)
        {
            this.layers = layers;
            this.plugin = plugin;
            this.url = url;
            this.mapPanel = mapPanel;
            this.statusLbl = statusLbl;
            map = plugin.Map;
            map.BasicSection.PropertyChanged += BasicSection_PropertyChanged;
            navigationWidget = new NavigationWidget(mapPanel, map.Metrics, Globals.MapTileSize);
        }

        protected void Invalidate(bool refreshIndicatorsOnly)
        {
            if (refreshIndicatorsOnly)
            {
                mapPanel.Invalidate();
            }
            else
            {
                // Full repaint.
                mapPanel.Invalidate(RenderMap);
            }
        }

        private void BasicSection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "BasePlayer":
                    {
                        foreach (var baseBuilding in map.Buildings.OfType<Building>().Select(x => x.Occupier).Where(x => x.BasePriority >= 0))
                        {
                            mapPanel.Invalidate(map, baseBuilding);
                        }
                    }
                    break;
                case "ExpansionEnabled":
                    UpdateExpansionUnits();
                    RemoveExpansionUnits();
                    break;
            }
        }

        protected virtual void UpdateExpansionUnits()
        {
            // Can be overridden by sub-tools to update their object lists.
        }

        private void RemoveExpansionUnits()
        {
            if (map.BasicSection.ExpansionEnabled)
            {
                // Expansion is enabled. Nothing to do.
                return;
            }
            // Technos on map
            List<(Point, ICellOccupier)> toDelete = new List<(Point, ICellOccupier)>();
            foreach ((Point p, ICellOccupier occup) in map.Technos)
            {
                if (occup is Unit un)
                {
                    if (un.Type.IsExpansionUnit)
                    {
                        toDelete.Add((p, occup));
                    }
                }
                else if (occup is InfantryGroup ifg)
                {
                    if (ifg.Infantry.Any(inf => inf != null && inf.Type.IsExpansionUnit))
                    {
                        toDelete.Add((p, occup));
                    }
                }
            }
            foreach ((Point point, ICellOccupier occup) in toDelete)
            {
                if (occup is Unit un)
                {
                    mapPanel.Invalidate(map, un);
                    map.Technos.Remove(occup);
                }
                else if (occup is InfantryGroup infantryGroup)
                {
                    Infantry[] inf = infantryGroup.Infantry;
                    for (int i = 0; i < inf.Length; ++i)
                    {
                        if (inf[i] != null && (inf[i].Type.Flag & UnitTypeFlag.IsExpansionUnit) == UnitTypeFlag.IsExpansionUnit)
                        {
                            inf[i] = null;
                        }
                    }
                    bool delGroup = inf.All(i => i == null);
                    mapPanel.Invalidate(map, infantryGroup);
                    if (delGroup)
                    {
                        map.Technos.Remove(infantryGroup);
                    }
                }
            }
            // Teamtypes
            foreach (TeamType teamtype in map.TeamTypes)
            {
                List<TeamTypeClass> toRemove = new List<TeamTypeClass>();
                foreach (TeamTypeClass ttclass in teamtype.Classes)
                {
                    if ((ttclass.Type is UnitType ut && ut.IsExpansionUnit) || (ttclass.Type is InfantryType it && it.IsExpansionUnit))
                    {
                        toRemove.Add(ttclass);
                    }
                }
                foreach (TeamTypeClass ttclass in toRemove)
                {
                    teamtype.Classes.Remove(ttclass);
                }
            }
        }

        protected abstract void RefreshPreviewPanel();

        private void Map_RulesChanged(Object sender, EventArgs e)
        {
            // Bibs may have changed. Refresh all buildings.
            foreach ((Point p, Building bld) in map.Buildings.OfType<Building>())
            {
                Dictionary<Point, Smudge> bibPoints = bld.GetBib(p, map.SmudgeTypes, true);
                if (bibPoints != null)
                {
                    mapPanel.Invalidate(map, bibPoints.Keys);
                    SmudgeTool.RestoreNearbySmudge(map, bibPoints.Keys, null);
                }
            }
            RefreshPreviewPanel();
        }

        private void MapPanel_PreRender(object sender, RenderEventArgs e)
        {
            if ((e.Cells != null) && (e.Cells.Count == 0))
            {
                return;
            }
            PreRenderMap();
            using (var g = Graphics.FromImage(mapPanel.MapImage))
            {
                MapRenderer.SetRenderSettings(g, Globals.MapSmoothScale);
                MapRenderer.Render(plugin.GameType, RenderMap, g, e.Cells?.Where(p => map.Metrics.Contains(p)).ToHashSet(), Layers);
            }
        }

        private void MapPanel_PostRender(object sender, RenderEventArgs e)
        {
            PostRenderMap(e.Graphics);
            navigationWidget.Render(e.Graphics);
        }

        protected virtual void PreRenderMap() { }

        protected virtual void PostRenderMap(Graphics graphics)
        {
            PostRenderMap(graphics, this.map, Globals.MapTileScale, PriorityLayers, ManuallyHandledLayers, Layers);
        }

        public static void PostRenderMap(Graphics graphics, Map map, double tileScale,  MapLayerFlag priorityLayers, MapLayerFlag manuallyHandledLayers, MapLayerFlag layersToRender)
        {
            Size tileSize = new Size(Math.Max(1, (int)(Globals.OriginalTileWidth * tileScale)), Math.Max(1, (int)(Globals.OriginalTileHeight * tileScale)));

            // Only render these if they are not in the priority layers, and not handled manually.
            // The functions themselves will take care of checking whether they are in the active layers to render.
            if ((priorityLayers & MapLayerFlag.Boundaries) == MapLayerFlag.None
                && (manuallyHandledLayers & MapLayerFlag.Boundaries) == MapLayerFlag.None)
            {
                RenderMapBoundaries(graphics, layersToRender, map, tileSize);
            }
            if ((priorityLayers & MapLayerFlag.CellTriggers) == MapLayerFlag.None
                && (manuallyHandledLayers & MapLayerFlag.CellTriggers) == MapLayerFlag.None)
            {
                RenderCellTriggers(graphics, map, tileSize, tileScale, layersToRender);
            }
            if ((priorityLayers & MapLayerFlag.Waypoints) == MapLayerFlag.None
                && (manuallyHandledLayers & MapLayerFlag.Waypoints) == MapLayerFlag.None)
            {
                RenderWayPoints(graphics, map, tileSize, tileScale, layersToRender);
            }
            if ((priorityLayers & MapLayerFlag.BuildingRebuild) == MapLayerFlag.None
                && (manuallyHandledLayers & MapLayerFlag.BuildingRebuild) == MapLayerFlag.None)
            {
                RenderAllBuildingLabels(graphics, map, tileSize, tileScale, layersToRender);
            }
            if ((priorityLayers & MapLayerFlag.TechnoTriggers) == MapLayerFlag.None
                && (manuallyHandledLayers & MapLayerFlag.TechnoTriggers) == MapLayerFlag.None)
            {
                RenderTechnoTriggers(graphics, map, tileSize, tileScale, layersToRender);
            }
            // Priority layers are only drawn at the end, so they get painted over all others.
            if ((priorityLayers & MapLayerFlag.Boundaries) != MapLayerFlag.None)
            {
                RenderMapBoundaries(graphics, layersToRender, map, tileSize);
            }
            if ((priorityLayers & MapLayerFlag.CellTriggers) != MapLayerFlag.None)
            {
                RenderCellTriggers(graphics, map, tileSize, tileScale, layersToRender);
            }
            if ((priorityLayers & MapLayerFlag.Waypoints) != MapLayerFlag.None)
            {
                RenderWayPoints(graphics, map, tileSize, tileScale, layersToRender);
            }
            if ((priorityLayers & MapLayerFlag.BuildingRebuild) != MapLayerFlag.None)
            {
                RenderAllBuildingLabels(graphics, map, tileSize, tileScale, layersToRender);
            }
            if ((priorityLayers & MapLayerFlag.TechnoTriggers) != MapLayerFlag.None)
            {
                RenderTechnoTriggers(graphics, map, tileSize, tileScale, layersToRender);
            }
        }

        public static void RenderAllBuildingLabels(Graphics graphics, Map map, Size tileSize, double tileScale, MapLayerFlag layersToRender)
        {
            if ((layersToRender & MapLayerFlag.Buildings) == MapLayerFlag.None
                || ((layersToRender & MapLayerFlag.BuildingRebuild) == MapLayerFlag.None && (layersToRender & MapLayerFlag.BuildingFakes) == MapLayerFlag.None))
            {
                return;
            }
            foreach (var (topLeft, building) in map.Buildings.OfType<Building>())
            {
                RenderBuildingLabels(graphics, building, topLeft, tileSize, tileScale, layersToRender, false);
            }
        }

        public static void RenderBuildingLabels(Graphics graphics, Building building, Point topLeft, Size tileSize, double tileScale, MapLayerFlag layersToRender, Boolean forPreview)
        {
            if ((layersToRender & MapLayerFlag.Buildings) == MapLayerFlag.None)
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
            if (building.Type.IsFake && (layersToRender & MapLayerFlag.BuildingFakes) == MapLayerFlag.BuildingFakes)
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
            if (building.BasePriority >= 0 && (layersToRender & MapLayerFlag.BuildingRebuild) == MapLayerFlag.BuildingRebuild)
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
                        backgroundBounds.Offset((buildingBounds.Width - textBounds.Width) / 2.0f, buildingBounds.Height -  textBounds.Height);
                        graphics.FillRectangle(baseBackgroundBrush, backgroundBounds);
                        graphics.DrawString(priText, font, baseTextBrush, backgroundBounds, stringFormat);
                    }
                }
            }
        }

        protected static void RenderWayPoints(Graphics graphics, Map map, Size tileSize, double tileScale, MapLayerFlag layersToRender, params Waypoint[] specifiedToExclude)
        {
            RenderWayPoints(graphics, map, tileSize, tileScale, layersToRender, Color.Black, Color.DarkOrange, Color.DarkOrange, false, true, specifiedToExclude);
        }

        protected static void RenderWayPoints(Graphics graphics, Map map, Size tileSize, double tileScale, MapLayerFlag layersToRender, Color fillColor, Color borderColor, Color textColor, bool thickborder, bool excludeSpecified, params Waypoint[] specified)
        {
            if ((layersToRender & MapLayerFlag.Waypoints) == MapLayerFlag.None)
            {
                return;
            }
            float borderSize = Math.Max(0.5f, tileSize.Width / 60.0f);
            float thickBorderSize = Math.Max(1f, tileSize.Width / 20.0f);
            using (var waypointBackgroundBrush = new SolidBrush(Color.FromArgb(96, fillColor)))
            using (var waypointBrush = new SolidBrush(textColor))
            using (var waypointPen = new Pen(Color.FromArgb(128, borderColor), thickborder ? thickBorderSize : borderSize))
            {
                Waypoint[] toPaint = excludeSpecified ? map.Waypoints : specified;
                foreach (var waypoint in toPaint)
                {
                    if (waypoint.Cell.HasValue)
                    {
                        if (excludeSpecified && specified.Contains(waypoint))
                        {
                            continue;
                        }
                        var x = waypoint.Cell.Value % map.Metrics.Width;
                        var y = waypoint.Cell.Value / map.Metrics.Width;
                        var location = new Point(x * tileSize.Width, y * tileSize.Height);
                        var textBounds = new Rectangle(location, tileSize);
                        graphics.FillRectangle(waypointBackgroundBrush, textBounds);
                        graphics.DrawRectangle(waypointPen, textBounds);
                        StringFormat stringFormat = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        var text = waypoint.Name.ToString();
                        using (var font = graphics.GetAdjustedFont(text, SystemFonts.DefaultFont, textBounds.Width,
                            Math.Max(1, (int)(24 * tileScale)), Math.Max(1, (int)(48 * tileScale)), true))
                        {
                            graphics.DrawString(text.ToString(), font, waypointBrush, textBounds, stringFormat);
                        }
                    }
                }
            }
        }

        protected static void RenderTechnoTriggers(Graphics graphics, Map map, Size tileSize, double tileScale, MapLayerFlag layersToRender)
        {
            if ((layersToRender & MapLayerFlag.TechnoTriggers) == MapLayerFlag.None)
            {
                return;
            }
            float borderSize = Math.Max(0.5f, tileSize.Width / 60.0f);
            using (var technoTriggerBackgroundBrush = new SolidBrush(Color.FromArgb(96, Color.Black)))
            using (var technoTriggerBrush = new SolidBrush(Color.LimeGreen))
            using (var technoTriggerPen = new Pen(Color.LimeGreen, borderSize))
            {
                foreach (var (cell, techno) in map.Technos)
                {
                    var location = new Point(cell.X * tileSize.Width, cell.Y * tileSize.Height);
                    (string trigger, Rectangle bounds)[] triggers = null;
                    if (techno is Terrain terrain)
                    {
                        if ((layersToRender & MapLayerFlag.Terrain) == MapLayerFlag.Terrain)
                        {
                            triggers = new (string, Rectangle)[] { (terrain.Trigger, new Rectangle(location, terrain.Type.GetRenderSize(tileSize))) };
                        }
                    }
                    else if (techno is Building building)
                    {
                        if ((layersToRender & MapLayerFlag.Buildings) == MapLayerFlag.Buildings)
                        {
                            var size = new Size(building.Type.Size.Width * tileSize.Width, building.Type.Size.Height * tileSize.Height);
                            triggers = new (string, Rectangle)[] { (building.Trigger, new Rectangle(location, size)) };
                        }
                    }
                    else if (techno is Unit unit)
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
                                if (infantry == null)
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
                        foreach (var (trigger, bounds) in triggers.Where(x => x.trigger != null && !x.trigger.Equals("None", StringComparison.OrdinalIgnoreCase)))
                        {
                            using (var font = graphics.GetAdjustedFont(trigger, SystemFonts.DefaultFont, bounds.Width,
                                Math.Max(1,(int)(12 * tileScale)), Math.Max(1, (int)(24 * tileScale)), true))
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

        protected static void RenderCellTriggers(Graphics graphics, Map map, Size tileSize, double tileScale, MapLayerFlag layersToRender, params String[] specifiedToExclude)
        {
            RenderCellTriggers(graphics, map, tileSize, tileScale, layersToRender, Color.Black, Color.White, Color.White, false, true, specifiedToExclude);
        }

        protected static void RenderCellTriggers(Graphics graphics, Map map, Size tileSize, double tileScale, MapLayerFlag layersToRender, Color fillColor, Color borderColor, Color textColor, bool thickborder, bool excludeSpecified, params String[] specified)
        {
            if ((layersToRender & MapLayerFlag.CellTriggers) == MapLayerFlag.None)
            {
                return;
            }
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

        protected static void RenderMapBoundaries(Graphics graphics, MapLayerFlag Layers, Map map, Size tileSize)
        {
            RenderMapBoundaries(graphics, Layers, map, map.Bounds, tileSize, Color.Cyan);
        }

        protected static void RenderMapBoundaries(Graphics graphics, MapLayerFlag Layers, Map map, Rectangle bounds, Size tileSize, Color color)
        {
            if ((Layers & MapLayerFlag.Boundaries) == MapLayerFlag.None)
            {
                return;
            }
            var boundsRect = Rectangle.FromLTRB(
                bounds.Left * tileSize.Width,
                bounds.Top * tileSize.Height,
                bounds.Right * tileSize.Width,
                bounds.Bottom * tileSize.Height
            );
            using (var boundsPen = new Pen(color, Math.Max(1f, tileSize.Width / 8.0f)))
                graphics.DrawRectangle(boundsPen, boundsRect);
        }

        /// <summary>
        /// Called when the tool is made the active tool.
        /// Remember to call this base method when overriding
        /// in derived classes.
        /// </summary>
        public virtual void Activate()
        {
            navigationWidget.Activate();
            this.mapPanel.PreRender += MapPanel_PreRender;
            this.mapPanel.PostRender += MapPanel_PostRender;
            this.map.RulesChanged += this.Map_RulesChanged;
        }

        /// <summary>
        /// Called when the tool is made inactive
        /// (for example, the user selects a different tool).
        /// Remember to call this base method when overriding
        /// in derived classes.
        /// </summary>
        public virtual void Deactivate()
        {
            navigationWidget.Deactivate();
            mapPanel.PreRender -= MapPanel_PreRender;
            mapPanel.PostRender -= MapPanel_PostRender;
            this.map.RulesChanged -= this.Map_RulesChanged;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Deactivate();
                    navigationWidget.Dispose();
                    map.BasicSection.PropertyChanged -= BasicSection_PropertyChanged;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
