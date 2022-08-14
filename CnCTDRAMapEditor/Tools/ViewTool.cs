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
                    layers = value;
                    Invalidate();
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

        protected void Invalidate()
        {
            mapPanel.Invalidate(RenderMap);
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
            }
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
            // Only render these if they are not in the priority layers, and not handled manually.
            // The functions themselves will take care of checking whether they are in the active layers to render.
            if ((PriorityLayers & MapLayerFlag.Boundaries) == MapLayerFlag.None
                && (ManuallyHandledLayers & MapLayerFlag.Boundaries) == MapLayerFlag.None)
            {
                RenderMapBoundaries(graphics);
            }
            if ((PriorityLayers & MapLayerFlag.CellTriggers) == MapLayerFlag.None
                && (ManuallyHandledLayers & MapLayerFlag.CellTriggers) == MapLayerFlag.None)
            {
                RenderCellTriggers(graphics);
            }
            if ((PriorityLayers & MapLayerFlag.Waypoints) == MapLayerFlag.None
                && (ManuallyHandledLayers & MapLayerFlag.Waypoints) == MapLayerFlag.None)
            {
                RenderWayPoints(graphics);
            }
            if ((PriorityLayers & MapLayerFlag.TechnoTriggers) == MapLayerFlag.None
                && (ManuallyHandledLayers & MapLayerFlag.TechnoTriggers) == MapLayerFlag.None)
            {
                RenderTechnoTriggers(graphics);
            }
            // Priority layers are only drawn at the end, so they get painted over all others.
            if ((PriorityLayers & MapLayerFlag.Boundaries) != MapLayerFlag.None)
            {
                RenderMapBoundaries(graphics);
            }
            if ((PriorityLayers & MapLayerFlag.CellTriggers) != MapLayerFlag.None)
            {
                RenderCellTriggers(graphics);
            }
            if ((PriorityLayers & MapLayerFlag.Waypoints) != MapLayerFlag.None)
            {
                RenderWayPoints(graphics);
            }
            if ((PriorityLayers & MapLayerFlag.TechnoTriggers) != MapLayerFlag.None)
            {
                RenderTechnoTriggers(graphics);
            }
        }

        protected void RenderWayPoints(Graphics graphics, params Waypoint[] specifiedToExclude)
        {
            RenderWayPoints(graphics, Color.Black, Color.DarkOrange, Color.DarkOrange, false, true, specifiedToExclude);
        }

        protected void RenderWayPoints(Graphics graphics, Color fillColor, Color borderColor, Color textColor, bool thickborder, bool excludeSpecified, params Waypoint[] specified)
        {
            if ((Layers & MapLayerFlag.Waypoints) == MapLayerFlag.None)
            {
                return;
            }
            using (var waypointBackgroundBrush = new SolidBrush(Color.FromArgb(96, fillColor)))
            using (var waypointBrush = new SolidBrush(textColor))
            using (var waypointPen = new Pen(Color.FromArgb(128, borderColor), thickborder ? 3f : 1f))
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
                        var location = new Point(x * Globals.MapTileWidth, y * Globals.MapTileHeight);
                        var textBounds = new Rectangle(location, Globals.MapTileSize);
                        graphics.FillRectangle(waypointBackgroundBrush, textBounds);
                        graphics.DrawRectangle(waypointPen, textBounds);
                        StringFormat stringFormat = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        var text = waypoint.Name.ToString();
                        var font = graphics.GetAdjustedFont(text, SystemFonts.DefaultFont, textBounds.Width, 24 / Globals.MapTileScale, 48 / Globals.MapTileScale, true);
                        graphics.DrawString(text.ToString(), font, waypointBrush, textBounds, stringFormat);
                    }
                }
            }
        }

        protected void RenderTechnoTriggers(Graphics graphics)
        {
            if ((Layers & MapLayerFlag.TechnoTriggers) == MapLayerFlag.None)
            {
                return;
            }
            using (var technoTriggerBackgroundBrush = new SolidBrush(Color.FromArgb(96, Color.Black)))
            using (var technoTriggerBrush = new SolidBrush(Color.LimeGreen))
            using (var technoTriggerPen = new Pen(Color.LimeGreen))
            {
                foreach (var (cell, techno) in map.Technos)
                {
                    var location = new Point(cell.X * Globals.MapTileWidth, cell.Y * Globals.MapTileHeight);
                    (string trigger, Rectangle bounds)[] triggers = null;
                    if (techno is Terrain terrain)
                    {
                        triggers = new (string, Rectangle)[] { (terrain.Trigger, new Rectangle(location, terrain.Type.GetRenderSize(Globals.MapTileSize))) };
                    }
                    else if (techno is Building building)
                    {
                        var size = new Size(building.Type.Size.Width * Globals.MapTileWidth, building.Type.Size.Height * Globals.MapTileHeight);
                        triggers = new (string, Rectangle)[] { (building.Trigger, new Rectangle(location, size)) };
                    }
                    else if (techno is Unit unit)
                    {
                        triggers = new (string, Rectangle)[] { (unit.Trigger, new Rectangle(location, Globals.MapTileSize)) };
                    }
                    else if (techno is InfantryGroup infantryGroup)
                    {
                        List<(string, Rectangle)> infantryTriggers = new List<(string, Rectangle)>();
                        for (var i = 0; i < infantryGroup.Infantry.Length; ++i)
                        {
                            var infantry = infantryGroup.Infantry[i];
                            if (infantry == null)
                            {
                                continue;
                            }
                            var size = Globals.MapTileSize;
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
                    if (triggers != null)
                    {
                        StringFormat stringFormat = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        foreach (var (trigger, bounds) in triggers.Where(x => x.trigger != null && !x.trigger.Equals("None", StringComparison.OrdinalIgnoreCase)))
                        {
                            var font = graphics.GetAdjustedFont(trigger, SystemFonts.DefaultFont, bounds.Width, 12 / Globals.MapTileScale, 24 / Globals.MapTileScale, true);
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

        protected void RenderCellTriggers(Graphics graphics, params String[] specifiedToExclude)
        {
            RenderCellTriggers(graphics, Color.Black, Color.White, Color.White, false, true, specifiedToExclude);
        }

        protected void RenderCellTriggers(Graphics graphics, Color fillColor, Color borderColor, Color textColor, bool thickborder, bool excludeSpecified, params String[] specified)
        {
            if ((Layers & MapLayerFlag.CellTriggers) == MapLayerFlag.None)
            {
                return;
            }
            HashSet<String> specifiedSet = new HashSet<String>(specified, StringComparer.InvariantCultureIgnoreCase);
            using (var cellTriggersBackgroundBrush = new SolidBrush(Color.FromArgb(96, fillColor)))
            using (var cellTriggersBrush = new SolidBrush(Color.FromArgb(128, textColor)))
            using (var cellTriggerPen = new Pen(borderColor, thickborder ? 3f : 1f))
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
                    var location = new Point(x * Globals.MapTileWidth, y * Globals.MapTileHeight);
                    var textBounds = new Rectangle(location, Globals.MapTileSize);
                    graphics.FillRectangle(cellTriggersBackgroundBrush, textBounds);
                    graphics.DrawRectangle(cellTriggerPen, textBounds);
                    StringFormat stringFormat = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    var text = cellTrigger.Trigger;
                    var font = graphics.GetAdjustedFont(text, SystemFonts.DefaultFont, textBounds.Width, 24 / Globals.MapTileScale, 48 / Globals.MapTileScale, true);
                    graphics.DrawString(text.ToString(), font, cellTriggersBrush, textBounds, stringFormat);
                }
            }
        }

        protected void RenderMapBoundaries(Graphics graphics)
        {
            if ((Layers & MapLayerFlag.Boundaries) == MapLayerFlag.None)
            {
                return;
            }
            var bounds = Rectangle.FromLTRB(
                map.Bounds.Left * Globals.MapTileWidth,
                map.Bounds.Top * Globals.MapTileHeight,
                map.Bounds.Right * Globals.MapTileWidth,
                map.Bounds.Bottom * Globals.MapTileHeight
            );
            using (var boundsPen = new Pen(Color.Cyan, 8.0f))
                graphics.DrawRectangle(boundsPen, bounds);
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
