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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Tools
{
    public class OverlaysTool : ViewTool
    {
        /// <summary>
        /// Layers that are not painted by the PostRenderMap function on ViewTool level because they are handled
        /// at a specific point in the PostRenderMap override by the implementing tool.
        /// </summary>
        protected override MapLayerFlag ManuallyHandledLayers => MapLayerFlag.CrateOutlines;

        private readonly TypeListBox overlayTypeComboBox;
        private readonly MapPanel overlayTypeMapPanel;

        private readonly Dictionary<int, Overlay> undoOverlays = new Dictionary<int, Overlay>();
        private readonly Dictionary<int, Overlay> redoOverlays = new Dictionary<int, Overlay>();

        private Map previewMap;
        protected override Map RenderMap => previewMap;

        private bool placementMode;

        protected override Boolean InPlacementMode
        {
            get { return placementMode; }
        }

        private OverlayType selectedOverlayType;
        private OverlayType SelectedOverlayType
        {
            get => selectedOverlayType;
            set
            {
                if (selectedOverlayType != value)
                {
                    if (placementMode && (selectedOverlayType != null))
                    {
                        mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(navigationWidget.MouseCell, new Size(1, 1)), 1, 1));
                    }
                    selectedOverlayType = value;
                    overlayTypeComboBox.SelectedValue = selectedOverlayType;
                    RefreshPreviewPanel();
                }
            }
        }

        public OverlaysTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, TypeListBox overlayTypeComboBox, MapPanel overlayTypeMapPanel, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> url)
            : base(mapPanel, layers, statusLbl, plugin, url)
        {
            previewMap = map;
            this.overlayTypeComboBox = overlayTypeComboBox;
            this.overlayTypeComboBox.SelectedIndexChanged += OverlayTypeComboBox_SelectedIndexChanged;
            this.overlayTypeMapPanel = overlayTypeMapPanel;
            this.overlayTypeMapPanel.BackColor = Color.White;
            this.overlayTypeMapPanel.MaxZoom = 1;
            this.overlayTypeMapPanel.SmoothScale = Globals.PreviewSmoothScale;
            SelectedOverlayType = this.overlayTypeComboBox.Types.First() as OverlayType;
        }

        private void OverlayTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedOverlayType = overlayTypeComboBox.SelectedValue as OverlayType;
        }

        private void OverlaysTool_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                EnterPlacementMode();
            }
        }

        private void OverlaysTool_KeyUp(object sender, KeyEventArgs e)
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
                    AddOverlay(navigationWidget.MouseCell);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    RemoveOverlay(navigationWidget.MouseCell);
                }
            }
            else if ((e.Button == MouseButtons.Left) || (e.Button == MouseButtons.Right))
            {
                PickOverlay(navigationWidget.MouseCell);
            }
        }

        private void MapPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if ((undoOverlays.Count > 0) || (redoOverlays.Count > 0))
            {
                CommitChange();
            }
        }

        private void MapPanel_MouseLeave(object sender, EventArgs e)
        {
            ExitPlacementMode();
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
                if (Control.MouseButtons == MouseButtons.Left)
                {
                    AddOverlay(e.NewCell);
                }
                else if (Control.MouseButtons == MouseButtons.Right)
                {
                    RemoveOverlay(e.NewCell);
                }
                if (SelectedOverlayType != null)
                {
                    // For Concrete
                    mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(e.OldCell, new Size(1, 1)), 1, 1));
                    mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(e.NewCell, new Size(1, 1)), 1, 1));
                }
            }
            else if ((e.MouseButtons == MouseButtons.Left) || (e.MouseButtons == MouseButtons.Right))
            {
                PickOverlay(e.NewCell);
            }
        }

        private void AddOverlay(Point location)
        {
            OverlayType selected = SelectedOverlayType;
            // Can't place overlay on top and bottom row, for some odd reason.
            if (selected == null || location.Y == 0 || location.Y == map.Metrics.Height - 1)
            {
                return;
            }
            if (map.Metrics.GetCell(location, out int cell))
            {
                var overlay = new Overlay { Type = selected, Icon = 0 };
                if (map.Overlay[location] == null)
                {
                    if (!undoOverlays.ContainsKey(cell))
                    {
                        undoOverlays[cell] = map.Overlay[cell];
                    }
                    map.Overlay[cell] = overlay;
                    redoOverlays[cell] = overlay;
                    mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(location, new Size(1, 1)), 1, 1));
                }
            }
        }

        private void RemoveOverlay(Point location)
        {
            if (map.Metrics.GetCell(location, out int cell))
            {
                var overlay = map.Overlay[cell];
                if (overlay?.Type.IsOverlay ?? false)
                {
                    if (!undoOverlays.ContainsKey(cell))
                    {
                        undoOverlays[cell] = map.Overlay[cell];
                    }
                    map.Overlay[cell] = null;
                    redoOverlays[cell] = null;
                    mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(location, new Size(1, 1)), 1, 1));
                }
            }
        }
        private void CommitChange()
        {
            bool origDirtyState = plugin.Dirty;
            plugin.Dirty = true;
            var undoOverlays2 = new Dictionary<int, Overlay>(undoOverlays);
            void undoAction(UndoRedoEventArgs e)
            {
                foreach (var kv in undoOverlays2)
                {
                    e.Map.Overlay[kv.Key] = kv.Value;
                }
                e.MapPanel.Invalidate(e.Map, undoOverlays2.Keys.Select(k =>
                {
                    e.Map.Metrics.GetLocation(k, out Point location);
                    return Rectangle.Inflate(new Rectangle(location, new Size(1, 1)), 1, 1);
                }));
                if (e.Plugin != null)
                {
                    e.Plugin.Dirty = origDirtyState;
                }
            }
            var redoOverlays2 = new Dictionary<int, Overlay>(redoOverlays);
            void redoAction(UndoRedoEventArgs e)
            {
                foreach (var kv in redoOverlays2)
                {
                    e.Map.Overlay[kv.Key] = kv.Value;
                }
                e.MapPanel.Invalidate(e.Map, redoOverlays2.Keys.Select(k =>
                {
                    e.Map.Metrics.GetLocation(k, out Point location);
                    return Rectangle.Inflate(new Rectangle(location, new Size(1, 1)), 1, 1);
                }));
                if (e.Plugin != null)
                {
                    e.Plugin.Dirty = true;
                }
            }
            undoOverlays.Clear();
            redoOverlays.Clear();
            url.Track(undoAction, redoAction);
        }

        private void EnterPlacementMode()
        {
            if (placementMode)
            {
                return;
            }
            placementMode = true;
            navigationWidget.MouseoverSize = Size.Empty;
            if (SelectedOverlayType != null)
            {
                mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(navigationWidget.MouseCell, new Size(1, 1)), 1, 1));
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
            if (SelectedOverlayType != null)
            {
                mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(navigationWidget.MouseCell, new Size(1, 1)), 1, 1));
            }
            UpdateStatus();
        }

        private void PickOverlay(Point location)
        {
            if (map.Metrics.GetCell(location, out int cell))
            {
                var overlay = map.Overlay[cell];
                // Nyerguds fix: this should use the same filter as the list fill! Crashed on resources.
                if ((overlay != null) && overlay.Type.IsOverlay)
                {
                    SelectedOverlayType = overlay.Type;
                }
            }
        }

        protected override void RefreshPreviewPanel()
        {
            var oldImage = overlayTypeMapPanel.MapImage;
            if (SelectedOverlayType != null)
            {
                var overlayPreview = new Bitmap(Globals.PreviewTileWidth, Globals.PreviewTileHeight);
                Overlay mockOverlay = new Overlay()
                {
                    Type = SelectedOverlayType,
                    Icon = 0
                };
                var render = MapRenderer.Render(plugin.GameType, map.Theater, new OverlayType[0], new OverlayType[0], new Point(0,0), Globals.PreviewTileSize, Globals.PreviewTileScale, mockOverlay);
                if (!render.Item1.IsEmpty)
                {
                    using (var g = Graphics.FromImage(overlayPreview))
                    {
                        MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                        render.Item2(g);
                    }
                }
                else
                {
                    // This should never happen; the map renderer renders dummy graphics for overlay now.
                    using (var g = Graphics.FromImage(overlayPreview))
                    {
                        MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                        List<(int, Overlay)> overlayList = new List<(int, Overlay)>();
                        overlayList.Add((0, new Overlay() { Type = SelectedOverlayType, Icon = 0 }));
                        MapRenderer.RenderAllBoundsFromCell(g, Globals.PreviewTileSize, overlayList, new CellMetrics(new Size(1, 1)));
                    }
                }
                overlayTypeMapPanel.MapImage = overlayPreview;
            }
            else
            {
                overlayTypeMapPanel.MapImage = null;
            }
            if (oldImage != null)
            {
                try { oldImage.Dispose(); }
                catch { /* ignore */ }
            }
            overlayTypeMapPanel.Invalidate();
        }

        private void UpdateStatus()
        {
            if (placementMode)
            {
                statusLbl.Text = "Left-Click to place overlay, Right-Click to remove overlay";
            }
            else
            {
                statusLbl.Text = "Shift to enter placement mode, Left-Click or Right-Click to pick overlay";
            }
        }

        protected override void PreRenderMap()
        {
            base.PreRenderMap();
            previewMap = map.Clone(true);
            if (placementMode)
            {
                var location = navigationWidget.MouseCell;
                if (SelectedOverlayType != null)
                {
                    if (previewMap.Metrics.GetCell(location, out int cell))
                    {
                        if (previewMap.Overlay[cell] == null)
                        {
                            previewMap.Overlay[cell] = new Overlay { Type = SelectedOverlayType, Icon = 0, Tint = Color.FromArgb(128, SelectedOverlayType.Tint) };
                            mapPanel.Invalidate(previewMap, Rectangle.Inflate(new Rectangle(location, new Size(1, 1)), 1, 1));
                        }
                    }
                }
            }
        }

        protected override void PostRenderMap(Graphics graphics)
        {
            base.PostRenderMap(graphics);
            if ((Layers & MapLayerFlag.CrateOutlines) == MapLayerFlag.CrateOutlines)
            {
                MapRenderer.RenderAllCrateOutlines(graphics, previewMap, Globals.MapTileSize, Globals.MapTileScale, false);
            }
            MapRenderer.RenderAllBoundsFromCell(graphics, Globals.MapTileSize, previewMap.Overlay.Where(x => x.Value.Type.IsOverlay), previewMap.Metrics);
        }

        public override void Activate()
        {
            base.Activate();
            this.Deactivate(true);
            this.mapPanel.MouseDown += MapPanel_MouseDown;
            this.mapPanel.MouseUp += MapPanel_MouseUp;
            this.mapPanel.MouseMove += MapPanel_MouseMove;
            this.mapPanel.MouseLeave += MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown += OverlaysTool_KeyDown;
            (this.mapPanel as Control).KeyUp += OverlaysTool_KeyUp;
            this.navigationWidget.MouseCellChanged += MouseoverWidget_MouseCellChanged;
            this.UpdateStatus();
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
            this.mapPanel.MouseUp -= MapPanel_MouseUp;
            this.mapPanel.MouseMove -= MapPanel_MouseMove;
            this.mapPanel.MouseLeave -= MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown -= OverlaysTool_KeyDown;
            (this.mapPanel as Control).KeyUp -= OverlaysTool_KeyUp;
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
                    Deactivate();
                    overlayTypeComboBox.SelectedIndexChanged -= OverlayTypeComboBox_SelectedIndexChanged;
                }
                disposedValue = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
