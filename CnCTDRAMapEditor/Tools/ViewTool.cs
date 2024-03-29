﻿//
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
        public IGamePlugin Plugin => plugin;
        public abstract Object CurrentObject { get; set; }
        protected bool isActive = false;
        public bool IsActive { get { return isActive; } }
        public event EventHandler RequestMouseInfoRefresh;

        protected void RefreshMainWindowMouseInfo()
        {
            RequestMouseInfoRefresh?.Invoke(this, new EventArgs());
        }

        public abstract void UpdateStatus();

        public virtual bool IsBusy { get { return false; } }

        protected readonly Map map;

        protected readonly MapPanel mapPanel;
        protected readonly ToolStripStatusLabel statusLbl;
        protected readonly UndoRedoList<UndoRedoEventArgs, ToolType> url;
        protected readonly NavigationWidget navigationWidget;

        public NavigationWidget NavigationWidget => navigationWidget;

        protected virtual Map RenderMap => map;

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
                    layers = value;
                    Invalidate(modifiedBits);
                    RefreshPreviewPanel();
                }
            }
        }

        public ViewTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> url)
        {
            this.layers = layers;
            this.plugin = plugin;
            this.url = url;
            this.mapPanel = mapPanel;
            this.statusLbl = statusLbl;
            map = plugin.Map;
            map.BasicSection.PropertyChanged += BasicSection_PropertyChanged;
            navigationWidget = new NavigationWidget(mapPanel, map.Metrics, Globals.MapTileSize, true);
        }

        protected void Invalidate(MapLayerFlag modifiedBits)
        {
            bool refreshIndicatorsOnly = (modifiedBits & ~MapLayerFlag.Indicators) == 0;
            if (refreshIndicatorsOnly)
            {
                mapPanel.Invalidate();
            }
            else if ((modifiedBits & MapLayerFlag.Template) != MapLayerFlag.None)
            {
                // Full repaint. Normally only done on initial load.
                mapPanel.Invalidate(RenderMap);
            }
            else
            {
                // Filter the objects of the layers that have been toggled.
                HashSet<Point> points = new HashSet<Point>();
                CellMetrics metr = map.Metrics;
                if ((modifiedBits & MapLayerFlag.Technos) == MapLayerFlag.Technos)
                {
                    // All of them; don't filter.
                    foreach (var (Location, Overlapper) in RenderMap.Overlappers)
                    {
                        AddOverlapperPoints(Overlapper, Location, points);
                    }
                }
                else
                {
                    // Specific filters per type.
                    if ((modifiedBits & MapLayerFlag.Terrain) != MapLayerFlag.None)
                    {
                        AddOverlapperTypePoints<Terrain>(points);
                    }
                    if ((modifiedBits & MapLayerFlag.Infantry) != MapLayerFlag.None)
                    {
                        AddOverlapperTypePoints<InfantryGroup>(points);
                    }
                    if ((modifiedBits & MapLayerFlag.Units) != MapLayerFlag.None)
                    {
                        AddOverlapperTypePoints<Unit>(points);
                    }
                    if ((modifiedBits & MapLayerFlag.Buildings) != MapLayerFlag.None)
                    {
                        AddOverlapperTypePoints<Building>(points);
                    }
                }
                if ((modifiedBits & MapLayerFlag.OverlayAll) != MapLayerFlag.None)
                {
                    foreach (var (Cell, _) in RenderMap.Overlay)
                    {
                        if (metr.GetLocation(Cell, out Point pt))
                        {
                            points.Add(pt);
                        }
                    }
                }
                if ((modifiedBits & MapLayerFlag.Smudge) != MapLayerFlag.None)
                {
                    // Multi-cell smudges are placed per cell, so no need to filter out bibs and check all their cells separately.
                    foreach (var (Cell, _) in RenderMap.Smudge)
                    {
                        if (metr.GetLocation(Cell, out Point pt))
                        {
                            points.Add(pt);
                        }
                    }
                }
                if ((modifiedBits & MapLayerFlag.Waypoints) != MapLayerFlag.None)
                {
                    foreach (Waypoint wp in RenderMap.Waypoints)
                    {
                        if (wp.Cell.HasValue && metr.GetLocation(wp.Cell.Value, out Point pt))
                        {
                            points.Add(pt);
                        }
                    }
                }
                mapPanel.Invalidate(RenderMap, points);
            }
        }

        private void AddOverlapperTypePoints<T>(HashSet<Point> points) where T: ICellOverlapper
        {
            foreach (var (Location, Overlapper) in RenderMap.Overlappers.OfType<T>())
            {
                AddOverlapperPoints(Overlapper, Location, points);
            }
        }

        private void AddOverlapperPoints(ICellOverlapper overlapper, Point location, HashSet<Point> points)
        {
            points.UnionWith(new Rectangle(
                location.X + overlapper.OverlapBounds.X,
                location.Y + overlapper.OverlapBounds.Y,
                overlapper.OverlapBounds.Width,
                overlapper.OverlapBounds.Height).Points());
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
                        MapPropertiesChanged();
                    }
                    break;
                case "ExpansionEnabled":
                    UpdateExpansionUnits();
                    break;
            }
        }

        protected virtual void MapPropertiesChanged()
        {
            // Can be overridden by tool windows.
        }

        protected virtual void RefreshPreviewPanel()
        {
            // Can be overridden by tool windows.
        }

        protected virtual void UpdateExpansionUnits()
        {
            // Can be overridden by sub-tools to update their object lists.
        }

        private void Map_RulesChanged(Object sender, MapRefreshEventArgs e)
        {
            // Bibs may have changed. Refresh all buildings.
            mapPanel.Invalidate(map, e.Points);
            // Checks if there is any smudge to restore in the given points.
            SmudgeTool.RestoreNearbySmudge(map, e.Points, null);
            RefreshPreviewPanel();
        }

        private void Map_MapContentsChanged(Object sender, MapRefreshEventArgs e)
        {
            // General event meant to refresh map.
            mapPanel.Invalidate(map, e.Points);
        }

        private void MapPanel_PreRender(object sender, RenderEventArgs e)
        {
            if (e.Points != null && e.Points.Count == 0)
            {
                return;
            }
            PreRenderMap();
            using (var g = Graphics.FromImage(mapPanel.MapImage))
            {
                MapRenderer.SetRenderSettings(g, Globals.MapSmoothScale);
                MapRenderer.Render(plugin.GameInfo, RenderMap, g, e.Points?.Where(p => map.Metrics.Contains(p)).ToHashSet(), Layers);
            }
        }

        private void MapPanel_PostRender(object sender, RenderEventArgs e)
        {
            Rectangle refreshBounds = NavigationWidget.VisibleBounds;
            PostRenderMap(e.Graphics, refreshBounds);
            navigationWidget.Render(e.Graphics);
        }

        protected virtual void PreRenderMap() { }

        protected abstract bool InPlacementMode { get; }

        protected virtual void PostRenderMap(Graphics graphics, Rectangle visibleCells)
        {
            PostRenderMap(graphics, this.plugin, this.map, Globals.MapTileScale, Layers, ManuallyHandledLayers, this.InPlacementMode, visibleCells);
        }

        public static void PostRenderMap(Graphics graphics, IGamePlugin plugin, Map map, double tileScale, MapLayerFlag layersToRender, MapLayerFlag manuallyHandledLayers, bool inPlacementMode, Rectangle visibleCells)
        {
            // tileScale should always be given so it results in an exact integer tile size. Math.Round was added to account for .999 situations in the floats.
            Size tileSize = new Size(Math.Max(1, (int)Math.Round(Globals.OriginalTileWidth * tileScale)), Math.Max(1, (int)Math.Round(Globals.OriginalTileHeight * tileScale)));
            // For bounds, add one more cell to get all borders showing.
            Rectangle boundRenderCells = visibleCells;
            boundRenderCells.Inflate(1, 1);
            boundRenderCells.Intersect(map.Metrics.Bounds);
            // Only render these if they are not in the priority layers, and not handled manually.
            // The functions themselves will take care of checking whether they are in the active layers to render.
            if ((layersToRender & MapLayerFlag.LandTypes) == MapLayerFlag.LandTypes
                && (manuallyHandledLayers & MapLayerFlag.LandTypes) == MapLayerFlag.None)
            {
                MapRenderer.RenderLandTypes(graphics, plugin, map.Templates, tileSize, visibleCells, false);
            }
            if ((Globals.ShowPlacementGrid && inPlacementMode) ||
                (layersToRender & MapLayerFlag.MapGrid) == MapLayerFlag.MapGrid
                && (manuallyHandledLayers & MapLayerFlag.MapGrid) == MapLayerFlag.None)
            {
                MapRenderer.RenderMapGrid(graphics, visibleCells, map.Bounds, (layersToRender & MapLayerFlag.Boundaries) == MapLayerFlag.Boundaries, tileSize, Globals.MapGridColor);
            }
            if ((layersToRender & MapLayerFlag.MapSymmetry) == MapLayerFlag.MapSymmetry
                && (manuallyHandledLayers & MapLayerFlag.MapSymmetry) == MapLayerFlag.None)
            {
                MapRenderer.RenderMapSymmetry(graphics, map.Bounds, tileSize, Color.Cyan);
            }
            if ((layersToRender & MapLayerFlag.Boundaries) == MapLayerFlag.Boundaries
                && (manuallyHandledLayers & MapLayerFlag.Boundaries) == MapLayerFlag.None)
            {
                MapRenderer.RenderMapBoundaries(graphics, map, visibleCells, tileSize);
            }
            bool autoHandleOutlines = (manuallyHandledLayers & MapLayerFlag.OverlapOutlines) == MapLayerFlag.None;
            bool renderOverlay = (layersToRender & MapLayerFlag.Overlay) == MapLayerFlag.Overlay;
            if ((layersToRender & MapLayerFlag.OverlapOutlines) == MapLayerFlag.OverlapOutlines && autoHandleOutlines)
            {
                if ((layersToRender & MapLayerFlag.Infantry) == MapLayerFlag.Infantry && plugin.GameInfo.SupportsMapLayer(MapLayerFlag.Infantry))
                {
                    MapRenderer.RenderAllInfantryOutlines(graphics, map, visibleCells, tileSize, true);
                }
                if ((layersToRender & MapLayerFlag.Units) == MapLayerFlag.Units && plugin.GameInfo.SupportsMapLayer(MapLayerFlag.Units))
                {
                    MapRenderer.RenderAllVehicleOutlines(graphics, plugin.GameInfo, map, visibleCells, tileSize, true);
                }
                if (renderOverlay && !Globals.OutlineAllCrates && plugin.Map.CrateOverlaysAvailable)
                {
                    MapRenderer.RenderAllCrateOutlines(graphics, plugin.GameInfo, map, visibleCells, tileSize, tileScale, !Globals.OutlineAllCrates);
                }
            }
            // Special case: while it's not handled by OverlapOutlines, tools indicating that they handle the OverlapOutlines
            // manually will also paint this, so of all the outlines, it's drawn last.
            if (renderOverlay  && autoHandleOutlines && Globals.OutlineAllCrates)
            {
                MapRenderer.RenderAllCrateOutlines(graphics, plugin.GameInfo, map, visibleCells, tileSize, tileScale, false);
            }
            if ((layersToRender & MapLayerFlag.CellTriggers) == MapLayerFlag.CellTriggers
                && (manuallyHandledLayers & MapLayerFlag.CellTriggers) == MapLayerFlag.None)
            {
                MapRenderer.RenderCellTriggersSoft(graphics, map, visibleCells, tileSize);
            }
            if ((layersToRender & (MapLayerFlag.Waypoints | MapLayerFlag.FootballArea)) == (MapLayerFlag.Waypoints | MapLayerFlag.FootballArea)
                && (manuallyHandledLayers & MapLayerFlag.WaypointsIndic) == MapLayerFlag.None && plugin.GameInfo.SupportsMapLayer(MapLayerFlag.FootballArea))
            {
                MapRenderer.RenderAllFootballAreas(graphics, map, visibleCells, tileSize, tileScale, plugin.GameInfo);
                MapRenderer.RenderFootballAreaFlags(graphics, plugin.GameInfo, map, visibleCells, tileSize);
            }
            if ((layersToRender & (MapLayerFlag.Buildings | MapLayerFlag.EffectRadius)) == (MapLayerFlag.Buildings | MapLayerFlag.EffectRadius)
                && (manuallyHandledLayers & MapLayerFlag.EffectRadius) == MapLayerFlag.None && plugin.GameInfo.SupportsMapLayer(MapLayerFlag.EffectRadius))
            {
                MapRenderer.RenderAllBuildingEffectRadiuses(graphics, map, visibleCells, tileSize, map.GapRadius, null);
            }
            if ((layersToRender & (MapLayerFlag.Units | MapLayerFlag.EffectRadius)) == (MapLayerFlag.Units | MapLayerFlag.EffectRadius)
                && (manuallyHandledLayers & MapLayerFlag.EffectRadius) == MapLayerFlag.None && plugin.GameInfo.SupportsMapLayer(MapLayerFlag.EffectRadius))
            {
                MapRenderer.RenderAllUnitEffectRadiuses(graphics, map, visibleCells, tileSize, map.RadarJamRadius, null);
            }
            if ((layersToRender & (MapLayerFlag.Waypoints | MapLayerFlag.WaypointRadius)) == (MapLayerFlag.Waypoints | MapLayerFlag.WaypointRadius)
                && (manuallyHandledLayers & MapLayerFlag.WaypointRadius) == MapLayerFlag.None)
            {
                MapRenderer.RenderAllWayPointRevealRadiuses(graphics, plugin, map, boundRenderCells, tileSize, null);
            }
            if ((layersToRender & (MapLayerFlag.Waypoints | MapLayerFlag.WaypointsIndic)) == (MapLayerFlag.Waypoints | MapLayerFlag.WaypointsIndic)
                && (manuallyHandledLayers & MapLayerFlag.WaypointsIndic) == MapLayerFlag.None)
            {
                MapRenderer.RenderWayPointIndicators(graphics, map, visibleCells, tileSize, Color.LightGreen, false, true);
            }
            if ((layersToRender & (MapLayerFlag.Buildings | MapLayerFlag.BuildingFakes)) == (MapLayerFlag.Buildings | MapLayerFlag.BuildingFakes)
                && (manuallyHandledLayers & MapLayerFlag.BuildingFakes) == MapLayerFlag.None && plugin.GameInfo.SupportsMapLayer(MapLayerFlag.BuildingFakes))
            {
                MapRenderer.RenderAllFakeBuildingLabels(graphics, map, visibleCells, tileSize);
            }
            if ((layersToRender & (MapLayerFlag.Buildings | MapLayerFlag.BuildingRebuild)) == (MapLayerFlag.Buildings | MapLayerFlag.BuildingRebuild)
                && (manuallyHandledLayers & MapLayerFlag.BuildingRebuild) == MapLayerFlag.None && plugin.GameInfo.SupportsMapLayer(MapLayerFlag.BuildingRebuild))
            {
                MapRenderer.RenderAllRebuildPriorityLabels(graphics, map, visibleCells, tileSize);
            }
            if ((layersToRender & MapLayerFlag.TechnoTriggers) == MapLayerFlag.TechnoTriggers
                && (manuallyHandledLayers & MapLayerFlag.TechnoTriggers) == MapLayerFlag.None)
            {
                MapRenderer.RenderAllTechnoTriggers(graphics, map, visibleCells, tileSize, layersToRender);
            }
        }

        protected void HandlePaintOutlines(Graphics graphics, Map map, Rectangle visibleCells, Size tileSize, double tileScale, MapLayerFlag layers)
        {
            bool renderAllCrateOutlines = Globals.OutlineAllCrates;
            bool renderOverlay = (layers & MapLayerFlag.Overlay) == MapLayerFlag.Overlay;
            if ((layers & MapLayerFlag.OverlapOutlines) == MapLayerFlag.OverlapOutlines)
            {
                if ((layers & MapLayerFlag.Infantry) == MapLayerFlag.Infantry)
                {
                    MapRenderer.RenderAllInfantryOutlines(graphics, map, visibleCells, tileSize, true);
                }
                if ((layers & MapLayerFlag.Units) == MapLayerFlag.Units)
                {
                    MapRenderer.RenderAllVehicleOutlines(graphics, plugin.GameInfo, map, visibleCells, tileSize, true);
                }
                if (renderOverlay && !renderAllCrateOutlines)
                {
                    MapRenderer.RenderAllCrateOutlines(graphics, plugin.GameInfo, map, visibleCells, tileSize, tileScale, true);
                }
            }
            // Special case: while it's not handled by the OverlapOutlines flag being enabled, tools indicating
            // that they handle the OverlapOutlines flag manually will also takr care of painting this, so of
            // all the outlines, it's drawn last.
            if (renderOverlay && renderAllCrateOutlines)
            {
                MapRenderer.RenderAllCrateOutlines(graphics, plugin.GameInfo, map, visibleCells, tileSize, tileScale, false);
            }
        }

        /// <summary>
        /// Called when the tool is made the active tool.
        /// Remember to call this base method when overriding
        /// in derived classes.
        /// </summary>
        public virtual void Activate()
        {
            this.navigationWidget.Activate();
            this.mapPanel.PreRender += MapPanel_PreRender;
            this.mapPanel.PostRender += MapPanel_PostRender;
            this.map.RulesChanged += this.Map_RulesChanged;
            this.map.MapContentsChanged += this.Map_MapContentsChanged;
            isActive = true;
        }

        /// <summary>
        /// Called when the tool is made inactive
        /// (for example, the user selects a different tool).
        /// Remember to call this base method when overriding
        /// in derived classes.
        /// </summary>
        public virtual void Deactivate()
        {
            isActive = false;
            this.navigationWidget.Deactivate();
            this.mapPanel.PreRender -= MapPanel_PreRender;
            this.mapPanel.PostRender -= MapPanel_PostRender;
            this.map.RulesChanged -= this.Map_RulesChanged;
            this.map.MapContentsChanged -= this.Map_MapContentsChanged;
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
