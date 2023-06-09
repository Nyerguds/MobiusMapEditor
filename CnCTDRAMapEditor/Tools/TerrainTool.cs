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

        private bool placementMode;

        protected override Boolean InPlacementMode
        {
            get { return placementMode || startedDragging; }
        }

        private readonly Terrain mockTerrain;

        private Terrain selectedTerrain;
        private Point? selectedTerrainLocation;
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
                    RefreshPreviewPanel();
                }
            }
        }

        public TerrainTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, TypeListBox terrainTypeComboBox, MapPanel terrainTypeMapPanel, TerrainProperties terrainProperties, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> url)
            : base(mapPanel, layers, statusLbl, plugin, url)
        {
            previewMap = map;
            mockTerrain = new Terrain();
            mockTerrain.PropertyChanged += MockTerrain_PropertyChanged;
            this.terrainTypeListBox = terrainTypeComboBox;
            this.terrainTypeListBox.SelectedIndexChanged += TerrainTypeCombo_SelectedIndexChanged;
            this.terrainTypeMapPanel = terrainTypeMapPanel;
            this.terrainTypeMapPanel.BackColor = Color.White;
            this.terrainTypeMapPanel.MaxZoom = 1;
            this.terrainTypeMapPanel.SmoothScale = Globals.PreviewSmoothScale;
            this.terrainProperties = terrainProperties;
            this.terrainProperties.Terrain = mockTerrain;
            this.terrainProperties.Visible = plugin.Map.TerrainEventTypes.Count > 0;
            SelectedTerrainType = terrainTypeComboBox.Types.First() as TerrainType;
        }

        private void MapPanel_MouseLeave(object sender, EventArgs e)
        {
            ExitPlacementMode();
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
                    selectedTerrainLocation = null;
                    selectedTerrainPivot = Point.Empty;
                    startedDragging = false;
                    mapPanel.Invalidate();
                    selectedTerrainProperties?.Close();
                    // only TD supports triggers ("Attacked" type) on terrain types.
                    if (plugin.GameType == GameType.TiberianDawn)
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
            url.Track(undoAction, redoAction);
        }

        private void MockTerrain_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshPreviewPanel();
        }

        private void TerrainTypeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedTerrainType = terrainTypeListBox.SelectedValue as TerrainType;
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
            if (selectedTerrain != null && selectedTerrainLocation.HasValue)
            {
                AddMoveUndoTracking(selectedTerrain, selectedTerrainLocation.Value);
                selectedTerrain = null;
                selectedTerrainLocation = null;
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
                url.Track(undoAction, redoAction);
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
                if (!startedDragging && selectedTerrainLocation.HasValue
                    && new Point(selectedTerrainLocation.Value.X + selectedTerrainPivot.X, selectedTerrainLocation.Value.Y + selectedTerrainPivot.Y) != e.NewCell)
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
            selectedTerrainLocation = null;
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
                url.Track(undoAction, redoAction);
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
                url.Track(undoAction, redoAction);
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
            if (map.Metrics.GetCell(location, out int cell))
            {
                if (map.Technos[cell] is Terrain terrain)
                {
                    SelectedTerrainType = terrain.Type;
                    mockTerrain.Trigger = terrain.Trigger;
                }
            }
        }

        private void SelectTerrain(Point location)
        {
            selectedTerrain = null;
            selectedTerrainLocation = null;
            selectedTerrainPivot = Point.Empty;
            startedDragging = false;
            if (map.Metrics.GetCell(location, out int cell) && map.Technos[cell] is Terrain selected && selected != null)
            {
                Point? selectedLocation = map.Technos[selected];
                Point selectedPivot = location - (Size)selectedLocation;
                selectedTerrain = selected;
                selectedTerrainLocation = selectedLocation;
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
                    var render = MapRenderer.RenderTerrain(plugin.GameType, map.Theater, new Point(0, 0), Globals.PreviewTileSize, Globals.PreviewTileScale, mockTerrain);
                    if (!render.Item1.IsEmpty)
                    {
                        render.Item2(g);
                    }
                    List<(Point p, Terrain ter)> terrainList = new List<(Point p, Terrain ter)>();
                    terrainList.Add((new Point(0, 0), mockTerrain));
                    MapRenderer.RenderAllOccupierBounds(g, Globals.PreviewTileSize, terrainList);
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

        private void UpdateStatus()
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
                statusLbl.Text = "Shift to enter placement mode, Left-Click drag to move terrain, "
                    + (plugin.GameType == GameType.TiberianDawn ? "Double-Click to update terrain properties, " : String.Empty)
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
            var location = navigationWidget.MouseCell;
            if (previewMap.Metrics.Contains(location))
            {
                var terrain = mockTerrain.Clone();
                terrain.Tint = Color.FromArgb(128, Color.White);
                //previewMap.Technos.Add(location, terrain);
                if (previewMap.Technos.CanAdd(location, terrain, terrain.Type.OccupyMask) && previewMap.Buildings.Add(location, terrain))
                {
                    mapPanel.Invalidate(previewMap, terrain);
                }
            }
        }

        protected override void PostRenderMap(Graphics graphics)
        {
            base.PostRenderMap(graphics);
            MapRenderer.RenderAllOccupierBounds(graphics, Globals.MapTileSize, previewMap.Technos.OfType<Terrain>());
            if ((Layers & MapLayerFlag.TechnoTriggers) == MapLayerFlag.TechnoTriggers)
            {
                MapRenderer.RenderAllTechnoTriggers(graphics, previewMap, Globals.MapTileSize, Globals.MapTileScale, Layers);
            }
        }

        public override void Activate()
        {
            base.Activate();
            this.Deactivate(true);
            this.mapPanel.MouseDown += MapPanel_MouseDown;
            this.mapPanel.MouseMove += MapPanel_MouseMove;
            this.mapPanel.MouseUp += MapPanel_MouseUp;
            this.mapPanel.MouseDoubleClick += MapPanel_MouseDoubleClick;
            this.mapPanel.MouseLeave += MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown += TerrainTool_KeyDown;
            (this.mapPanel as Control).KeyUp += TerrainTool_KeyUp;
            this.UpdateStatus();
            this.navigationWidget.MouseCellChanged += MouseoverWidget_MouseCellChanged;
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
            this.mapPanel.MouseDown -= MapPanel_MouseDown;
            this.mapPanel.MouseMove -= MapPanel_MouseMove;
            this.mapPanel.MouseUp -= MapPanel_MouseUp;
            this.mapPanel.MouseDoubleClick -= MapPanel_MouseDoubleClick;
            this.mapPanel.MouseLeave -= MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown -= TerrainTool_KeyDown;
            (this.mapPanel as Control).KeyUp -= TerrainTool_KeyUp;
            this.navigationWidget.MouseCellChanged -= MouseoverWidget_MouseCellChanged;
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
                    terrainTypeListBox.SelectedIndexChanged -= TerrainTypeCombo_SelectedIndexChanged;
                    Deactivate();
                }
                disposedValue = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
