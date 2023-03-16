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
            navigationWidget = new NavigationWidget(mapPanel, map.Metrics, Globals.MapTileSize, true);
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
                MapRenderer.Render(plugin.GameType, RenderMap, g, e.Points?.Where(p => map.Metrics.Contains(p)).ToHashSet(), Layers);
            }
        }

        private void MapPanel_PostRender(object sender, RenderEventArgs e)
        {
            PostRenderMap(e.Graphics);
            navigationWidget.Render(e.Graphics);
        }

        protected virtual void PreRenderMap() { }

        protected abstract bool InPlacementMode { get; }

        protected virtual void PostRenderMap(Graphics graphics)
        {
            PostRenderMap(graphics, this.plugin, this.map, Globals.MapTileScale, Layers, ManuallyHandledLayers, this.InPlacementMode);
        }

        public static void PostRenderMap(Graphics graphics, IGamePlugin plugin, Map map, double tileScale, MapLayerFlag layersToRender, MapLayerFlag manuallyHandledLayers, bool inPlacementMode)
        {
            Size tileSize = new Size(Math.Max(1, (int)(Globals.OriginalTileWidth * tileScale)), Math.Max(1, (int)(Globals.OriginalTileHeight * tileScale)));

            // Only render these if they are not in the priority layers, and not handled manually.
            // The functions themselves will take care of checking whether they are in the active layers to render.
            if ((Globals.ShowPlacementGrid && inPlacementMode) ||
                (layersToRender & MapLayerFlag.MapGrid) == MapLayerFlag.MapGrid
                && (manuallyHandledLayers & MapLayerFlag.MapGrid) == MapLayerFlag.None)
            {
                MapRenderer.RenderMapGrid(graphics, map.Bounds, tileSize, Globals.MapGridColor);
            }
            if ((layersToRender & MapLayerFlag.MapSymmetry) == MapLayerFlag.MapSymmetry
                && (manuallyHandledLayers & MapLayerFlag.MapSymmetry) == MapLayerFlag.None)
            {
                MapRenderer.RenderMapBoundaries(graphics, map.Bounds, tileSize, Color.Cyan, true);
            }
            if ((layersToRender & MapLayerFlag.Boundaries) == MapLayerFlag.Boundaries
                && (manuallyHandledLayers & MapLayerFlag.Boundaries) == MapLayerFlag.None)
            {
                MapRenderer.RenderMapBoundaries(graphics, map, tileSize);
            }
            if ((layersToRender & MapLayerFlag.CellTriggers) == MapLayerFlag.CellTriggers
                && (manuallyHandledLayers & MapLayerFlag.CellTriggers) == MapLayerFlag.None)
            {
                MapRenderer.RenderCellTriggers(graphics, map, tileSize, tileScale);
            }
            if ((layersToRender & (MapLayerFlag.Waypoints | MapLayerFlag.FootballArea)) == (MapLayerFlag.Waypoints | MapLayerFlag.FootballArea)
                && (manuallyHandledLayers & MapLayerFlag.WaypointsIndic) == MapLayerFlag.None && plugin.GameType == GameType.SoleSurvivor)
            {
                MapRenderer.RenderAllFootballAreas(graphics, map, tileSize, tileScale, plugin.GameType);
                MapRenderer.RenderFootballAreaFlags(graphics, plugin.GameType, map, tileSize);
            }
            if ((layersToRender & (MapLayerFlag.Buildings | MapLayerFlag.EffectRadius)) == (MapLayerFlag.Buildings | MapLayerFlag.EffectRadius)
                && (manuallyHandledLayers & MapLayerFlag.EffectRadius) == MapLayerFlag.None)
            {
                MapRenderer.RenderAllBuildingEffectRadiuses(graphics, map, Globals.MapTileSize, map.GapRadius);
            }
            if ((layersToRender & (MapLayerFlag.Units | MapLayerFlag.EffectRadius)) == (MapLayerFlag.Units | MapLayerFlag.EffectRadius)
                && (manuallyHandledLayers & MapLayerFlag.EffectRadius) == MapLayerFlag.None)
            {
                MapRenderer.RenderAllUnitEffectRadiuses(graphics, map, Globals.MapTileSize, map.RadarJamRadius);
            }
            if ((layersToRender & (MapLayerFlag.Waypoints | MapLayerFlag.WaypointRadius)) == (MapLayerFlag.Waypoints | MapLayerFlag.WaypointRadius)
                && (manuallyHandledLayers & MapLayerFlag.WaypointRadius) == MapLayerFlag.None)
            {
                MapRenderer.RenderAllWayPointRevealRadiuses(graphics, plugin, map, Globals.MapTileSize, null);
            }
            if ((layersToRender & (MapLayerFlag.Waypoints | MapLayerFlag.WaypointsIndic)) == (MapLayerFlag.Waypoints | MapLayerFlag.WaypointsIndic)
                && (manuallyHandledLayers & MapLayerFlag.WaypointsIndic) == MapLayerFlag.None)
            {
                MapRenderer.RenderWayPointIndicators(graphics, map, tileSize, tileScale, Color.LightGreen, false, true);
            }
            if ((layersToRender & (MapLayerFlag.Buildings | MapLayerFlag.BuildingFakes)) == (MapLayerFlag.Buildings | MapLayerFlag.BuildingFakes)
                && (manuallyHandledLayers & MapLayerFlag.BuildingFakes) == MapLayerFlag.None)
            {
                MapRenderer.RenderAllFakeBuildingLabels(graphics, map, tileSize, tileScale);
            }
            if ((layersToRender & (MapLayerFlag.Buildings | MapLayerFlag.BuildingRebuild)) == (MapLayerFlag.Buildings | MapLayerFlag.BuildingRebuild)
                && (manuallyHandledLayers & MapLayerFlag.BuildingRebuild) == MapLayerFlag.None)
            {
                MapRenderer.RenderAllRebuildPriorityLabels(graphics, map, tileSize, tileScale);
            }
            if ((layersToRender & MapLayerFlag.TechnoTriggers) == MapLayerFlag.TechnoTriggers
                && (manuallyHandledLayers & MapLayerFlag.TechnoTriggers) == MapLayerFlag.None)
            {
                MapRenderer.RenderAllTechnoTriggers(graphics, map, tileSize, tileScale, layersToRender);
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
        }

        /// <summary>
        /// Called when the tool is made inactive
        /// (for example, the user selects a different tool).
        /// Remember to call this base method when overriding
        /// in derived classes.
        /// </summary>
        public virtual void Deactivate()
        {
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
