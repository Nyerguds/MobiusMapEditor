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
    public class TerrainTool : ViewTool
    {
        /// <summary>
        /// Layers that are not painted by the PostRenderMap function on ViewTool level because they are handled
        /// at a specific point in the PostRenderMap override by the implementing tool.
        /// </summary>
        protected override MapLayerFlag ManuallyHandledLayers => MapLayerFlag.TechnoTriggers;

        private readonly TypeListBox terrainTypeListBox;
        private readonly MapPanel terrainTypeMapPanel;
        private readonly TerrainProperties terrainProperties;

        private Map previewMap;
        protected override Map RenderMap => previewMap;

        public override bool IsBusy { get { return startedDragging; } }

        public override Object CurrentObject
        {
            get { return mockTerrain; }
            set
            {
                if (value is Terrain ter)
                {
                    TerrainType tt = terrainTypeListBox.Types.Where(t => t is TerrainType trt && trt.ID == ter.Type.ID
                        && String.Equals(trt.Name, ter.Type.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() as TerrainType;
                    if (tt != null)
                    {
                        SelectedTerrainType = tt;
                    }
                    ter.Type = SelectedTerrainType;
                    mockTerrain.CloneDataFrom(ter);
                    RefreshPreviewPanel();
                }
            }
        }

        private bool placementMode;

        protected override Boolean InPlacementMode
        {
            get { return placementMode || startedDragging; }
        }

        private readonly Terrain mockTerrain;

        private Terrain selectedTerrain;
        private Point? selectedTerrainStartLocation;
        private Point selectedTerrainPivot;
        private bool startedDragging;

        private TerrainPropertiesPopup selectedTerrainProperties;

        private TerrainType selectedTerrainType;
        private TerrainType SelectedTerrainType
        {
            get => selectedTerrainType;
            set
            {
                if (selectedTerrainType != value)
                {
                    if (placementMode && (selectedTerrainType != null))
                    {
                        mapPanel.Invalidate(map, new Rectangle(navigationWidget.MouseCell, selectedTerrainType.OverlapBounds.Size));
                    }
                    selectedTerrainType = value;
                    terrainTypeListBox.SelectedValue = selectedTerrainType;
                    if (placementMode && (selectedTerrainType != null))
                    {
                        mapPanel.Invalidate(map, new Rectangle(navigationWidget.MouseCell, selectedTerrainType.OverlapBounds.Size));
                    }
                    mockTerrain.Type = selectedTerrainType;
                }
            }
        }

        public TerrainTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, TypeListBox terrainTypeListBox, MapPanel terrainTypeMapPanel,
            TerrainProperties terrainProperties, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> url)
            : base(mapPanel, layers, statusLbl, plugin, url)
        {
            previewMap = map;
            mockTerrain = new Terrain();
            this.terrainTypeListBox = terrainTypeListBox;
            this.terrainTypeListBox.SelectedIndexChanged += TerrainTypeListBox_SelectedIndexChanged;
            this.terrainTypeMapPanel = terrainTypeMapPanel;
            this.terrainTypeMapPanel.BackColor = Color.White;
            this.terrainTypeMapPanel.MaxZoom = 1;
            this.terrainTypeMapPanel.SmoothScale = Globals.PreviewSmoothScale;
            this.terrainProperties = terrainProperties;
            this.terrainProperties.Terrain = mockTerrain;
            this.terrainProperties.Visible = plugin.Map.TerrainEventTypes.Count > 0;
            SelectedTerrainType = terrainTypeListBox.Types.First() as TerrainType;
            RefreshPreviewPanel();
        }

        private void MapPanel_MouseLeave(object sender, EventArgs e)
        {
            ExitPlacementMode();
        }

        private void MapPanel_MouseWheel(Object sender, MouseEventArgs e)
        {
            if (e.Delta == 0 || (Control.ModifierKeys & Keys.Control) == Keys.None)
            {
                return;
            }
            KeyEventArgs keyArgs = new KeyEventArgs(e.Delta > 0 ? Keys.PageUp : Keys.PageDown);
            CheckSelectShortcuts(keyArgs);
        }

        private void MapPanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys != Keys.None || e.Button != MouseButtons.Left)
            {
                return;
            }
            if (map.Metrics.GetCell(navigationWidget.MouseCell, out int cell))
            {
                if (map.Technos[cell] is Terrain terrain)
                {
                    selectedTerrain = null;
                    selectedTerrainStartLocation = null;
                    selectedTerrainPivot = Point.Empty;
                    startedDragging = false;
                    mapPanel.Invalidate();
                    selectedTerrainProperties?.Close();
                    // only TD supports triggers ("Attacked" type) on terrain types.
                    if (plugin.Map.TerrainActionTypes.Count > 0 || plugin.Map.TerrainEventTypes.Count > 0)
                    {
                        Terrain preEdit = terrain.Clone();
                        selectedTerrainProperties = new TerrainPropertiesPopup(terrainProperties.Plugin, terrain);
                        selectedTerrainProperties.Closed += (cs, ce) =>
                        {
                            navigationWidget.Refresh();
                            AddPropertiesUndoRedo(terrain, preEdit);
                        };
                        selectedTerrainProperties.Show(mapPanel, mapPanel.PointToClient(Control.MousePosition));
                    }
                    UpdateStatus();
                }
            }
        }

        private void AddPropertiesUndoRedo(Terrain terrain, Terrain preEdit)
        {
            // terrain = terrain in its final edited form. Clone for preservation
            Terrain redoTerr = terrain.Clone();
            Terrain undoTerr = preEdit;
            if (redoTerr.Equals(undoTerr))
            {
                return;
            }
            bool origDirtyState = plugin.Dirty;
            plugin.Dirty = true;
            void undoAction(UndoRedoEventArgs ev)
            {
                terrain.CloneDataFrom(undoTerr);
                if (terrain.Trigger == null || (!Trigger.IsEmpty(terrain.Trigger)
                    && !ev.Map.FilterTerrainTriggers().Any(tr => tr.Name.Equals(terrain.Trigger, StringComparison.InvariantCultureIgnoreCase))))
                {
                    terrain.Trigger = Trigger.None;
                }
                ev.MapPanel.Invalidate(ev.Map, terrain);
                if (ev.Plugin != null)
                {
                    ev.Plugin.Dirty = origDirtyState;
                }
            }
            void redoAction(UndoRedoEventArgs ev)
            {
                terrain.CloneDataFrom(redoTerr);
                if (terrain.Trigger == null || (!Trigger.IsEmpty(terrain.Trigger)
                    && !ev.Map.FilterTerrainTriggers().Any(tr => tr.Name.Equals(terrain.Trigger, StringComparison.InvariantCultureIgnoreCase))))
                {
                    terrain.Trigger = Trigger.None;
                }
                ev.MapPanel.Invalidate(ev.Map, terrain);
                if (ev.Plugin != null)
                {
                    ev.Plugin.Dirty = true;
                }
            }
            url.Track(undoAction, redoAction, ToolType.Terrain);
        }

        private void MockTerrain_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshPreviewPanel();
        }

        private void TerrainTypeListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedTerrainType = terrainTypeListBox.SelectedValue as TerrainType;
            RefreshPreviewPanel();
        }

        private void TerrainTool_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                EnterPlacementMode();
            }
            else
            {
                CheckSelectShortcuts(e);
            }
        }

        private void TerrainTool_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                ExitPlacementMode();
            }
        }

        private void MapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (placementMode)
            {
                if (e.Button == MouseButtons.Left)
                {
                    AddTerrain(navigationWidget.MouseCell);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    RemoveTerrain(navigationWidget.MouseCell);
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                SelectTerrain(navigationWidget.MouseCell);
            }
            else if (e.Button == MouseButtons.Right)
            {
                PickTerrain(navigationWidget.MouseCell);
            }
        }

        private void MapPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (selectedTerrain != null && selectedTerrainStartLocation.HasValue)
            {
                AddMoveUndoTracking(selectedTerrain, selectedTerrainStartLocation.Value);
                selectedTerrain = null;
                selectedTerrainStartLocation = null;
                selectedTerrainPivot = Point.Empty;
                startedDragging = false;
                mapPanel.Invalidate();
                UpdateStatus();
            }
        }

        private void AddMoveUndoTracking(Terrain toMove, Point startLocation)
        {
            Point? finalLocation = map.Technos[toMove];
            if (finalLocation.HasValue && finalLocation.Value != startLocation)
            {
                Point endLocation = finalLocation.Value;
                bool origDirtyState = plugin.Dirty;
                plugin.Dirty = true;
                void undoAction(UndoRedoEventArgs ev)
                {
                    ev.MapPanel.Invalidate(ev.Map, toMove);
                    ev.Map.Technos.Remove(toMove);
                    ev.Map.Technos.Add(startLocation, toMove);
                    ev.MapPanel.Invalidate(ev.Map, toMove);
                    if (ev.Plugin != null)
                    {
                        ev.Plugin.Dirty = origDirtyState;
                    }
                }
                void redoAction(UndoRedoEventArgs ev)
                {
                    ev.MapPanel.Invalidate(ev.Map, toMove);
                    ev.Map.Technos.Remove(toMove);
                    ev.Map.Technos.Add(endLocation, toMove);
                    ev.MapPanel.Invalidate(ev.Map, toMove);
                    if (ev.Plugin != null)
                    {
                        ev.Plugin.Dirty = true;
                    }
                }
                url.Track(undoAction, redoAction, ToolType.Terrain);
            }
        }

        private void MapPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!placementMode && (Control.ModifierKeys == Keys.Shift))
            {
                EnterPlacementMode();
            }
            else if (placementMode && (Control.ModifierKeys == Keys.None))
            {
                ExitPlacementMode();
            }
        }

        private void MouseoverWidget_MouseCellChanged(object sender, MouseCellChangedEventArgs e)
        {
            if (placementMode)
            {
                if (SelectedTerrainType != null)
                {
                    mapPanel.Invalidate(map, new Rectangle(e.OldCell, SelectedTerrainType.OverlapBounds.Size));
                    mapPanel.Invalidate(map, new Rectangle(e.NewCell, SelectedTerrainType.OverlapBounds.Size));
                }
            }
            else if (selectedTerrain != null)
            {
                if (!startedDragging && selectedTerrainStartLocation.HasValue
                    && new Point(selectedTerrainStartLocation.Value.X + selectedTerrainPivot.X, selectedTerrainStartLocation.Value.Y + selectedTerrainPivot.Y) != e.NewCell)
                {
                    startedDragging = true;
                }
                Terrain toMove = selectedTerrain;
                var oldLocation = map.Technos[toMove].Value;
                var newLocation = new Point(Math.Max(0, e.NewCell.X - selectedTerrainPivot.X), Math.Max(0, e.NewCell.Y - selectedTerrainPivot.Y));
                mapPanel.Invalidate(map, toMove);
                map.Technos.Remove(toMove);
                if (map.Technos.Add(newLocation, toMove))
                {
                    mapPanel.Invalidate(map, toMove);
                }
                else
                {
                    map.Technos.Add(oldLocation, toMove);
                }
            }
            else if (e.MouseButtons == MouseButtons.Right)
            {
                PickTerrain(e.NewCell);
            }
        }

        private void AddTerrain(Point location)
        {
            if (!map.Metrics.Contains(location))
            {
                return;
            }
            if (SelectedTerrainType == null)
            {
                return;
            }
            selectedTerrain = null;
            selectedTerrainStartLocation = null;
            selectedTerrainPivot = Point.Empty;
            startedDragging = false;
            var terrain = mockTerrain.Clone();
            if (map.Technos.Add(location, terrain))
            {
                bool origDirtyState = plugin.Dirty;
                plugin.Dirty = true;
                mapPanel.Invalidate(map, terrain);
                void undoAction(UndoRedoEventArgs e)
                {
                    e.MapPanel.Invalidate(e.Map, location);
                    e.Map.Technos.Remove(terrain);
                    if (e.Plugin != null)
                    {
                        e.Plugin.Dirty = origDirtyState;
                    }
                }
                void redoAction(UndoRedoEventArgs e)
                {
                    e.Map.Technos.Add(location, terrain);
                    e.MapPanel.Invalidate(e.Map, location);
                    if (e.Plugin != null)
                    {
                        e.Plugin.Dirty = true;
                    }
                }
                url.Track(undoAction, redoAction, ToolType.Terrain);
            }
        }

        private void RemoveTerrain(Point location)
        {
            if (map.Technos[location] is Terrain terrain)
            {
                Point actualLocation = map.Technos[terrain].Value;
                mapPanel.Invalidate(map, terrain);
                map.Technos.Remove(location);
                bool origDirtyState = plugin.Dirty;
                plugin.Dirty = true;
                void undoAction(UndoRedoEventArgs e)
                {
                    e.Map.Technos.Add(actualLocation, terrain);
                    e.MapPanel.Invalidate(e.Map, terrain);
                    if (e.Plugin != null)
                    {
                        e.Plugin.Dirty = origDirtyState;
                    }
                }
                void redoAction(UndoRedoEventArgs e)
                {
                    e.MapPanel.Invalidate(e.Map, terrain);
                    e.Map.Technos.Remove(terrain);
                    if (e.Plugin != null)
                    {
                        e.Plugin.Dirty = true;
                    }
                }
                url.Track(undoAction, redoAction, ToolType.Terrain);
            }
        }

        private void CheckSelectShortcuts(KeyEventArgs e)
        {
            int maxVal = terrainTypeListBox.Items.Count - 1;
            int curVal = terrainTypeListBox.SelectedIndex;
            int newVal;
            switch (e.KeyCode)
            {
                case Keys.Home:
                    newVal = 0;
                    break;
                case Keys.End:
                    newVal = maxVal;
                    break;
                case Keys.PageDown:
                    newVal = Math.Min(curVal + 1, maxVal);
                    break;
                case Keys.PageUp:
                    newVal = Math.Max(curVal - 1, 0);
                    break;
                default:
                    return;
            }
            if (curVal != newVal)
            {
                terrainTypeListBox.SelectedIndex = newVal;
                TerrainType selected = SelectedTerrainType;
                if (placementMode && selected != null)
                {
                    mapPanel.Invalidate(map, new Rectangle(navigationWidget.MouseCell, selected.OverlapBounds.Size));
                }
            }
        }

        private void EnterPlacementMode()
        {
            if (placementMode)
            {
                return;
            }
            placementMode = true;
            navigationWidget.MouseoverSize = Size.Empty;
            if (SelectedTerrainType != null)
            {
                mapPanel.Invalidate(map, new Rectangle(navigationWidget.MouseCell, selectedTerrainType.OverlapBounds.Size));
            }
            UpdateStatus();
        }

        private void ExitPlacementMode()
        {
            if (!placementMode)
            {
                return;
            }
            placementMode = false;
            navigationWidget.MouseoverSize = new Size(1, 1);
            if (SelectedTerrainType != null)
            {
                mapPanel.Invalidate(map, new Rectangle(navigationWidget.MouseCell, selectedTerrainType.OverlapBounds.Size));
            }
            UpdateStatus();
        }

        private void PickTerrain(Point location)
        {
            if (map.Technos[location] is Terrain terrain)
            {
                SelectedTerrainType = terrain.Type;
                mockTerrain.Trigger = terrain.Trigger;
            }
        }

        private void SelectTerrain(Point location)
        {
            selectedTerrain = null;
            selectedTerrainStartLocation = null;
            selectedTerrainPivot = Point.Empty;
            startedDragging = false;
            if (map.Metrics.GetCell(location, out int cell) && map.Technos[cell] is Terrain selected && selected != null)
            {
                Point? selectedLocation = map.Technos[selected];
                Point selectedPivot = location - (Size)selectedLocation;
                selectedTerrain = selected;
                selectedTerrainStartLocation = selectedLocation;
                selectedTerrainPivot = selectedPivot;
            }
            mapPanel.Invalidate();
            UpdateStatus();
        }

        protected override void RefreshPreviewPanel()
        {
            var oldImage = terrainTypeMapPanel.MapImage;
            if (mockTerrain.Type != null)
            {
                Size previewSize = mockTerrain.Type.OverlapBounds.Size;
                var terrainPreview = new Bitmap(previewSize.Width * Globals.PreviewTileWidth, previewSize.Height * Globals.PreviewTileHeight);
                terrainPreview.SetResolution(96, 96);
                using (var g = Graphics.FromImage(terrainPreview))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    RenderInfo render = MapRenderer.RenderTerrain(new Point(0, 0), Globals.PreviewTileSize, Globals.PreviewTileScale, mockTerrain);
                    if (render.RenderedObject != null)
                    {
                        render.RenderAction(g);
                    }
                    List<(Point p, Terrain ter)> terrainList = new List<(Point p, Terrain ter)>();
                    terrainList.Add((new Point(0, 0), mockTerrain));
                    MapRenderer.RenderAllOccupierBounds(g, new Rectangle(Point.Empty, previewSize), Globals.PreviewTileSize, terrainList);
                    if ((Layers & MapLayerFlag.TechnoTriggers) == MapLayerFlag.TechnoTriggers)
                    {
                        CellMetrics tm = new CellMetrics(mockTerrain.Type.OverlapBounds.Size);
                        OccupierSet<ICellOccupier> technoSet = new OccupierSet<ICellOccupier>(tm);
                        technoSet.Add(0, mockTerrain);
                        MapRenderer.RenderAllTechnoTriggers(g, technoSet, tm.Bounds, Globals.PreviewTileSize, Layers, Color.LimeGreen, null, false);
                    }
                }
                terrainTypeMapPanel.MapImage = terrainPreview;
            }
            else
            {
                terrainTypeMapPanel.MapImage = null;
            }
            if (oldImage != null)
            {
                try { oldImage.Dispose(); }
                catch { /* ignore */ }
            }
            terrainTypeMapPanel.Invalidate();
        }

        public override void UpdateStatus()
        {
            if (placementMode)
            {
                statusLbl.Text = "Left-Click to place terrain, Right-Click to remove terrain";
            }
            else if (selectedTerrain != null)
            {
                statusLbl.Text = "Drag mouse to move terrain";
            }
            else
            {
                bool hasTerrainTriggers = plugin.Map.TerrainActionTypes.Count > 0 || plugin.Map.TerrainEventTypes.Count > 0;
                statusLbl.Text = "Shift to enter placement mode, Left-Click drag to move terrain, "
                    + (hasTerrainTriggers ? "Double-Click to update terrain properties, " : String.Empty)
                    + "Right-Click to pick terrain";
            }
        }

        protected override void PreRenderMap()
        {
            base.PreRenderMap();
            previewMap = map.Clone(true);
            if (!placementMode)
            {
                return;
            }
            if (SelectedTerrainType == null)
            {
                return;
            }
            navigationWidget.MouseoverSize = Size.Empty;
            Point location = navigationWidget.MouseCell;
            if (previewMap.Metrics.Contains(location))
            {
                var terrain = mockTerrain.Clone();
                terrain.Tint = Color.FromArgb(128, Color.White);
                terrain.IsPreview = true;
                //previewMap.Technos.Add(location, terrain);
                if (previewMap.Technos.CanAdd(location, terrain, terrain.Type.OccupyMask) && previewMap.Technos.Add(location, terrain))
                {
                    mapPanel.Invalidate(previewMap, terrain);
                }
            }
        }

        protected override void PostRenderMap(Graphics graphics, Rectangle visibleCells)
        {
            base.PostRenderMap(graphics, visibleCells);
            // For bounds, add one more cell to get all borders showing.
            Rectangle boundRenderCells = visibleCells;
            boundRenderCells.Inflate(1, 1);
            boundRenderCells.Intersect(map.Metrics.Bounds);
            Point location = navigationWidget.MouseCell;
            // Render green outline for any terrain object already placed on the real map.
            MapRenderer.RenderAllOccupierBoundsGreen(graphics, boundRenderCells, Globals.MapTileSize, map.Technos.OfType<Terrain>());
            IEnumerable<Point> highlightPoints = null;
            IEnumerable<(Point, Terrain)> place = null;
            TerrainType selectedType = SelectedTerrainType;
            // Determine overlap cells, and whether to render the current place preview outline.
            if (placementMode && selectedType != null)
            {
                List<Point> occupyPoints = OccupierSet.GetOccupyPoints(location, selectedType.OccupyMask).ToList();
                Boolean isCurrent = map.Technos.OfType<Terrain>().Any(lo => lo.Location == location && lo.Occupier.Type == selectedType);
                // If there is already a terrain object of this exact type placed on the current location, don't render anything extra.
                if (!isCurrent)
                {
                    // List overlapped points to indicate obstructions.
                    highlightPoints = occupyPoints.Where(p => map.Technos[p] != null);
                    // Store info on where to render backup preview outline.
                    place = (location, mockTerrain).Yield();
                }
            }
            // Render green outline of current terrain object.
            if (place != null)
            {
                MapRenderer.RenderAllOccupierBoundsGreen(graphics, boundRenderCells, Globals.MapTileSize, place);
            }
            // Render thin red obstructed cells of all other terrain objects.
            MapRenderer.RenderAllOccupierCellsRed(graphics, boundRenderCells, Globals.MapTileSize, map.Technos.OfType<Terrain>());
            // Render thin blue obstructed cells of preview terrain object.
            if (place != null)
            {
                MapRenderer.RenderAllOccupierBounds(graphics, boundRenderCells, Globals.MapTileSize, place, Color.Transparent, Color.Blue);
            }
            // Render thick red over obstructed cells.
            if (highlightPoints != null)
            {
                MapRenderer.RenderAllBoundsFromPoint(graphics, boundRenderCells, Globals.MapTileSize, highlightPoints, Color.Red);
            }
            if ((Layers & MapLayerFlag.TechnoTriggers) == MapLayerFlag.TechnoTriggers)
            {
                MapRenderer.RenderAllTechnoTriggers(graphics, previewMap, visibleCells, Globals.MapTileSize, Layers);
            }
        }

        public override void Activate()
        {
            base.Activate();
            this.Deactivate(true);
            this.mockTerrain.PropertyChanged += MockTerrain_PropertyChanged;
            this.mapPanel.MouseDown += MapPanel_MouseDown;
            this.mapPanel.MouseMove += MapPanel_MouseMove;
            this.mapPanel.MouseUp += MapPanel_MouseUp;
            this.mapPanel.MouseDoubleClick += MapPanel_MouseDoubleClick;
            this.mapPanel.MouseLeave += MapPanel_MouseLeave;
            this.mapPanel.MouseWheel += MapPanel_MouseWheel;
            this.mapPanel.SuspendMouseZoomKeys = Keys.Control;
            (this.mapPanel as Control).KeyDown += TerrainTool_KeyDown;
            (this.mapPanel as Control).KeyUp += TerrainTool_KeyUp;
            this.navigationWidget.BoundsMouseCellChanged += MouseoverWidget_MouseCellChanged;
            this.navigationWidget.MouseoverSize = new Size(1, 1);
            this.navigationWidget.PenColor = Color.Yellow;
            this.UpdateStatus();
            this.RefreshPreviewPanel();
        }

        public override void Deactivate()
        {
            this.Deactivate(false);
        }

        public void Deactivate(bool forActivate)
        {
            if (!forActivate)
            {
                this.ExitPlacementMode();
                base.Deactivate();
            }
            this.mockTerrain.PropertyChanged -= MockTerrain_PropertyChanged;
            this.mapPanel.MouseDown -= MapPanel_MouseDown;
            this.mapPanel.MouseMove -= MapPanel_MouseMove;
            this.mapPanel.MouseUp -= MapPanel_MouseUp;
            this.mapPanel.MouseDoubleClick -= MapPanel_MouseDoubleClick;
            this.mapPanel.MouseLeave -= MapPanel_MouseLeave;
            this.mapPanel.MouseWheel -= MapPanel_MouseWheel;
            this.mapPanel.SuspendMouseZoomKeys = Keys.None;
            (this.mapPanel as Control).KeyDown -= TerrainTool_KeyDown;
            (this.mapPanel as Control).KeyUp -= TerrainTool_KeyUp;
            this.navigationWidget.BoundsMouseCellChanged -= MouseoverWidget_MouseCellChanged;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    selectedTerrainProperties?.Close();
                    selectedTerrainProperties = null;
                    terrainTypeListBox.SelectedIndexChanged -= TerrainTypeListBox_SelectedIndexChanged;
                    Deactivate();
                }
                disposedValue = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
