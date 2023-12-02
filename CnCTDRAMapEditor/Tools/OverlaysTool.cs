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
        protected override MapLayerFlag ManuallyHandledLayers => MapLayerFlag.OverlapOutlines;

        private readonly TypeListBox overlayTypeListBox;
        private readonly MapPanel overlayTypeMapPanel;

        private readonly Dictionary<int, Overlay> undoOverlays = new Dictionary<int, Overlay>();
        private readonly Dictionary<int, Overlay> redoOverlays = new Dictionary<int, Overlay>();

        public override bool IsBusy { get { return undoOverlays.Count > 0; } }

        private Map previewMap;
        protected override Map RenderMap => previewMap;

        public override Object CurrentObject
        {
            get { return selectedOverlayType; }
            set
            {
                if (value is OverlayType ovt)
                {
                    OverlayType ot = overlayTypeListBox.Types.Where(o => o is OverlayType ovlt && ovlt.ID == ovt.ID
                        && String.Equals(ovlt.Name, ovt.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() as OverlayType;
                    if (ot != null)
                    {
                        SelectedOverlayType = ot;
                    }
                }
            }
        }

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
                    overlayTypeListBox.SelectedValue = selectedOverlayType;
                    RefreshPreviewPanel();
                }
            }
        }

        public OverlaysTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, TypeListBox overlayTypeListBox, MapPanel overlayTypeMapPanel,
            IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> url)
            : base(mapPanel, layers, statusLbl, plugin, url)
        {
            previewMap = map;
            this.overlayTypeListBox = overlayTypeListBox;
            this.overlayTypeListBox.SelectedIndexChanged += OverlayTypeListBox_SelectedIndexChanged;
            this.overlayTypeMapPanel = overlayTypeMapPanel;
            this.overlayTypeMapPanel.BackColor = Color.White;
            this.overlayTypeMapPanel.MaxZoom = 1;
            this.overlayTypeMapPanel.SmoothScale = Globals.PreviewSmoothScale;
            SelectedOverlayType = this.overlayTypeListBox.Types.First() as OverlayType;
        }

        private void OverlayTypeListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedOverlayType = overlayTypeListBox.SelectedValue as OverlayType;
        }

        private void OverlaysTool_KeyDown(object sender, KeyEventArgs e)
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
            Point mouseCell = e.NewCell;
            if (placementMode)
            {
                if (Control.MouseButtons == MouseButtons.Left)
                {
                    AddOverlay(mouseCell);
                }
                else if (Control.MouseButtons == MouseButtons.Right)
                {
                    RemoveOverlay(mouseCell);
                }
                if (SelectedOverlayType != null)
                {
                    // For Concrete
                    mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(e.OldCell, new Size(1, 1)), 1, 1));
                    mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(mouseCell, new Size(1, 1)), 1, 1));
                }
            }
            else if ((e.MouseButtons == MouseButtons.Left) || (e.MouseButtons == MouseButtons.Right))
            {
                PickOverlay(mouseCell);
            }
        }

        private void AddOverlay(Point location)
        {
            OverlayType selected = SelectedOverlayType;
            // Can't place overlay on top and bottom row. See OverlayClass::Read_INI
            if (selected == null || location.Y == 0 || location.Y == map.Metrics.Height - 1)
            {
                return;
            }
            if (map.Metrics.GetCell(location, out int cell))
            {
                Overlay overlay = new Overlay { Type = selected, Icon = 0 };
                Overlay cur = map.Overlay[location];
                if (cur == null || Map.IsIgnorableOverlay(cur))
                {
                    if (!undoOverlays.ContainsKey(cell))
                    {
                        undoOverlays[cell] = map.Overlay[cell];
                    }
                    map.Overlay[cell] = overlay;
                    Rectangle refreshArea = Rectangle.Inflate(new Rectangle(location, new Size(1, 1)), 1, 1);
                    map.UpdateConcreteOverlays(refreshArea.Points().ToHashSet());
                    redoOverlays[cell] = overlay;
                    mapPanel.Invalidate(map, refreshArea);
                }
            }
        }

        private void RemoveOverlay(Point location)
        {
            if (map.Metrics.GetCell(location, out int cell))
            {
                Overlay overlay = map.Overlay[cell];
                if (overlay != null && overlay.Type.IsOverlay && !Map.IsIgnorableOverlay(overlay))
                {
                    if (!undoOverlays.ContainsKey(cell))
                    {
                        undoOverlays[cell] = map.Overlay[cell];
                    }
                    map.Overlay[cell] = null;
                    Rectangle refreshArea = Rectangle.Inflate(new Rectangle(location, new Size(1, 1)), 1, 1);
                    map.UpdateConcreteOverlays(refreshArea.Points().ToHashSet());
                    redoOverlays[cell] = null;
                    mapPanel.Invalidate(map, refreshArea);
                }
            }
        }
        private void CommitChange()
        {
            bool origDirtyState = plugin.Dirty;
            plugin.Dirty = true;
            Dictionary<Int32, Overlay> undoOverlays2 = new Dictionary<int, Overlay>(undoOverlays);
            void undoAction(UndoRedoEventArgs e)
            {
                foreach (KeyValuePair<Int32, Overlay> kv in undoOverlays2)
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
            Dictionary<Int32, Overlay> redoOverlays2 = new Dictionary<int, Overlay>(redoOverlays);
            void redoAction(UndoRedoEventArgs e)
            {
                foreach (KeyValuePair<Int32, Overlay> kv in redoOverlays2)
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
            url.Track(undoAction, redoAction, ToolType.Overlay);
        }

        private void CheckSelectShortcuts(KeyEventArgs e)
        {
            int maxVal = overlayTypeListBox.Items.Count - 1;
            int curVal = overlayTypeListBox.SelectedIndex;
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
                overlayTypeListBox.SelectedIndex = newVal;
                if (placementMode)
                {
                    mapPanel.Invalidate(map, navigationWidget.MouseCell);
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
            navigationWidget.PenColor = Color.Red;
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
            navigationWidget.PenColor = Color.Yellow;
            if (SelectedOverlayType != null)
            {
                mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(navigationWidget.MouseCell, new Size(1, 1)), 1, 1));
            }
            UpdateStatus();
        }

        private void PickOverlay(Point location)
        {
            Overlay overlay = map.Overlay[location];
            if (overlay != null && overlay.Type.IsOverlay && !Map.IsIgnorableOverlay(overlay))
            {
                SelectedOverlayType = overlay.Type;
            }
        }

        protected override void RefreshPreviewPanel()
        {
            Image oldImage = overlayTypeMapPanel.MapImage;
            OverlayType overlayType = selectedOverlayType;
            if (overlayType != null)
            {
                Bitmap overlayPreview = new Bitmap(Globals.PreviewTileWidth, Globals.PreviewTileHeight);
                overlayPreview.SetResolution(96, 96);
                Overlay mockOverlay = new Overlay()
                {
                    Type = overlayType,
                    Icon = 0
                };
                (Rectangle, Action<Graphics>) render = MapRenderer.RenderOverlay(plugin.GameInfo, new Point(0,0), Globals.PreviewTileSize, Globals.PreviewTileScale, mockOverlay);
                if (!render.Item1.IsEmpty)
                {
                    using (Graphics g = Graphics.FromImage(overlayPreview))
                    {
                        MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                        render.Item2(g);
                    }
                }
                else
                {
                    // This should never happen; the map renderer renders dummy graphics for overlay now.
                    using (Graphics g = Graphics.FromImage(overlayPreview))
                    {
                        MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                        List<(int, Overlay)> overlayList = new List<(int, Overlay)>();
                        overlayList.Add((0, new Overlay() { Type = overlayType, Icon = 0 }));
                        MapRenderer.RenderAllBoundsFromCell(g, new Rectangle(0, 0, 1, 1), Globals.PreviewTileSize, overlayList, new CellMetrics(new Size(1, 1)));
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

        public override void UpdateStatus()
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
            if (!placementMode)
            {
                return;
            }
            navigationWidget.MouseoverSize = Size.Empty;
            Point location = navigationWidget.MouseCell;
            OverlayType selected = this.SelectedOverlayType;
            if (selected == null || !previewMap.Metrics.GetCell(location, out int cell))
            {
                return;
            }
            if (location.Y == 0 || location.Y == map.Metrics.Height - 1)
            {
                navigationWidget.MouseoverSize = new Size(1, 1);
            }
            Overlay onCell = previewMap.Overlay[cell];
            if (onCell == null || Map.IsIgnorableOverlay(onCell))
            {
                previewMap.Overlay[cell] = new Overlay { Type = SelectedOverlayType, Icon = 0, Tint = Color.FromArgb(128, SelectedOverlayType.Tint) };
                Rectangle refreshArea = Rectangle.Inflate(new Rectangle(location, new Size(1, 1)), 1, 1);
                previewMap.UpdateConcreteOverlays(refreshArea.Points().ToHashSet());
                mapPanel.Invalidate(previewMap, refreshArea);
            }
        }

        protected override void PostRenderMap(Graphics graphics, Rectangle visibleCells)
        {
            base.PostRenderMap(graphics, visibleCells);
            // For bounds, add one more cell to get all borders showing.
            Rectangle boundRenderCells = visibleCells;
            boundRenderCells.Inflate(1, 1);
            boundRenderCells.Intersect(map.Metrics.Bounds);
            this.HandlePaintOutlines(graphics, previewMap, boundRenderCells, Globals.MapTileSize, Globals.MapTileScale, this.Layers);
            int secondRowStartCell = map.Metrics.Width;
            int lastRowStartCell = map.Metrics.Length - map.Metrics.Width;
            MapRenderer.RenderAllBoundsFromCell(graphics, boundRenderCells, Globals.MapTileSize,
                previewMap.Overlay.Where(x => x.Value.Type.IsOverlay && x.Cell >= secondRowStartCell && x.Cell < lastRowStartCell && !Map.IsIgnorableOverlay(x.Value)), previewMap.Metrics);
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
            this.navigationWidget.BoundsMouseCellChanged += MouseoverWidget_MouseCellChanged;
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
            this.mapPanel.MouseDown -= MapPanel_MouseDown;
            this.mapPanel.MouseUp -= MapPanel_MouseUp;
            this.mapPanel.MouseMove -= MapPanel_MouseMove;
            this.mapPanel.MouseLeave -= MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown -= OverlaysTool_KeyDown;
            (this.mapPanel as Control).KeyUp -= OverlaysTool_KeyUp;
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
                    Deactivate();
                    overlayTypeListBox.SelectedIndexChanged -= OverlayTypeListBox_SelectedIndexChanged;
                }
                disposedValue = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
