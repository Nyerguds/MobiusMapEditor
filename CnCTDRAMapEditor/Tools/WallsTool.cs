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
    public class WallsTool : ViewTool
    {
        /// <summary>
        /// Layers that are not painted by the PostRenderMap function on ViewTool level because they are handled
        /// at a specific point in the PostRenderMap override by the implementing tool.
        /// </summary>
        protected override MapLayerFlag ManuallyHandledLayers => MapLayerFlag.None;

        private readonly TypeListBox wallTypeListBox;
        private readonly MapPanel wallTypeMapPanel;

        private readonly Dictionary<int, Overlay> undoOverlays = new Dictionary<int, Overlay>();
        private readonly Dictionary<int, Overlay> redoOverlays = new Dictionary<int, Overlay>();

        private Map previewMap;
        protected override Map RenderMap => previewMap;

        public override bool IsBusy { get { return undoOverlays.Count > 0; } }

        public override Object CurrentObject
        {
            get { return selectedWallType; }
            set
            {
                if (value is OverlayType ovt)
                {
                    SelectedWallType = ovt;
                }
            }
        }

        private bool placementMode;

        protected override Boolean InPlacementMode
        {
            get { return placementMode; }
        }

        private OverlayType selectedWallType;
        private OverlayType SelectedWallType
        {
            get => selectedWallType;
            set
            {
                if (selectedWallType != value)
                {
                    if (placementMode && (selectedWallType != null))
                    {
                        mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(navigationWidget.MouseCell, new Size(1, 1)), 1, 1));
                    }
                    selectedWallType = value;
                    wallTypeListBox.SelectedValue = selectedWallType;
                    RefreshPreviewPanel();
                }
            }
        }

        public WallsTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, TypeListBox wallTypeListBox, MapPanel wallTypeMapPanel,
            IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> url)
            : base(mapPanel, layers, statusLbl, plugin, url)
        {
            previewMap = map;
            this.wallTypeListBox = wallTypeListBox;
            this.wallTypeListBox.SelectedIndexChanged += WallTypeListBox_SelectedIndexChanged;
            this.wallTypeMapPanel = wallTypeMapPanel;
            this.wallTypeMapPanel.BackColor = Color.White;
            this.wallTypeMapPanel.MaxZoom = 1;
            this.wallTypeMapPanel.SmoothScale = Globals.PreviewSmoothScale;
            SelectedWallType = this.wallTypeListBox.Types.First() as OverlayType;
        }

        private void WallTypeListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedWallType = wallTypeListBox.SelectedValue as OverlayType;
        }

        private void WallTool_KeyDown(object sender, KeyEventArgs e)
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

        private void WallTool_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                ExitPlacementMode();
            }
        }

        private void MapPanel_MouseLeave(object sender, EventArgs e)
        {
            ExitPlacementMode();
        }

        private void MapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            Point mouseCell = navigationWidget.MouseCell;
            if (placementMode)
            {
                if (e.Button == MouseButtons.Left)
                {
                    // Overlay is not allowed on first and last row. See OverlayClass::Read_INI
                    if (mouseCell.Y > 0 && mouseCell.Y < map.Metrics.Height - 1)
                    {
                        AddWall(mouseCell);
                    }
                }
                else if (e.Button == MouseButtons.Right)
                {
                    RemoveWall(mouseCell);
                }
            }
            else if ((e.Button == MouseButtons.Left) || (e.Button == MouseButtons.Right))
            {
                PickWall(mouseCell);
            }
        }

        private void MapPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if ((undoOverlays.Count > 0) || (redoOverlays.Count > 0))
            {
                CommitChange();
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
            Point mouseCell = e.NewCell;
            if (placementMode)
            {
                if (Control.MouseButtons == MouseButtons.Left)
                {
                    if (mouseCell.Y > 0 && mouseCell.Y < map.Metrics.Height - 1)
                    {
                        AddWall(mouseCell);
                    }
                }
                else if (Control.MouseButtons == MouseButtons.Right)
                {
                    RemoveWall(mouseCell);
                }
                if (SelectedWallType != null)
                {
                    mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(e.OldCell, new Size(1, 1)), 1, 1));
                    mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(mouseCell, new Size(1, 1)), 1, 1));
                }
            }
            else if (e.MouseButtons == MouseButtons.Right)
            {
                PickWall(mouseCell);
            }
        }

        private void AddWall(Point location)
        {
            OverlayType selected = SelectedWallType;
            // Can't place overlay on top and bottom row. See OverlayClass::Read_INI
            if (selected == null || location.Y == 0 || location.Y == map.Metrics.Height - 1)
            {
                return;
            }
            if (map.Metrics.GetCell(location, out int cell))
            {
                var overlay = new Overlay { Type = SelectedWallType, Icon = 0 };
                if (map.Technos.CanAdd(cell, overlay) && map.Buildings.CanAdd(cell, overlay))
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

        private void RemoveWall(Point location)
        {
            if (map.Metrics.GetCell(location, out int cell))
            {
                var overlay = map.Overlay[cell];
                if (overlay?.Type.IsWall ?? false)
                {
                    if (!undoOverlays.ContainsKey(cell))
                    {
                        undoOverlays[cell] = overlay;
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
            url.Track(undoAction, redoAction, ToolType.Wall);
        }

        private void CheckSelectShortcuts(KeyEventArgs e)
        {
            int maxVal = wallTypeListBox.Items.Count - 1;
            int curVal = wallTypeListBox.SelectedIndex;
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
                wallTypeListBox.SelectedIndex = newVal;
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
            if (SelectedWallType != null)
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
            if (SelectedWallType != null)
            {
                mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(navigationWidget.MouseCell, new Size(1, 1)), 1, 1));
            }
            UpdateStatus();
        }

        private void PickWall(Point location)
        {
            if (map.Metrics.GetCell(location, out int cell))
            {
                var overlay = map.Overlay[cell];
                if ((overlay != null) && overlay.Type.IsWall)
                {
                    SelectedWallType = overlay.Type;
                }
            }
        }

        protected override void RefreshPreviewPanel()
        {
            wallTypeMapPanel.MapImage = SelectedWallType?.Thumbnail;
        }

        public override void UpdateStatus()
        {
            if (placementMode)
            {
                statusLbl.Text = "Left-Click drag to add walls, Right-Click drag to remove walls";
            }
            else
            {
                statusLbl.Text = "Shift to enter placement mode, Left-Click or Right-Click to pick wall";
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
            var location = navigationWidget.MouseCell;
            OverlayType selected = this.SelectedWallType;
            if (selected == null || !previewMap.Metrics.GetCell(location, out int cell))
            {
                return;
            }
            if (location.Y == 0 || location.Y == map.Metrics.Height - 1)
            {
                navigationWidget.MouseoverSize = new Size(1, 1);
            }
            var overlay = new Overlay { Type = selected, Icon = 0, Tint = Color.FromArgb(128, Color.FromArgb(128, selected.Tint)) };
            if (previewMap.Technos.CanAdd(cell, overlay) && previewMap.Buildings.CanAdd(cell, overlay))
            {
                previewMap.Overlay[cell] = overlay;
                mapPanel.Invalidate(previewMap, Rectangle.Inflate(new Rectangle(location, new Size(1, 1)), 1, 1));
            }
        }

        protected override void PostRenderMap(Graphics graphics, Rectangle visibleCells)
        {
            base.PostRenderMap(graphics, visibleCells);
            // For bounds, add one more cell to get all borders showing.
            Rectangle boundRenderCells = visibleCells;
            boundRenderCells.Inflate(1, 1);
            boundRenderCells.Intersect(map.Metrics.Bounds);
            int secondRow = map.Metrics.Width;
            int lastRow = map.Metrics.Length - map.Metrics.Width;
            MapRenderer.RenderAllBoundsFromCell(graphics, boundRenderCells, Globals.MapTileSize,
                previewMap.Overlay.Where(x => x.Value.Type.IsWall && x.Cell >= secondRow && x.Cell < lastRow), previewMap.Metrics);
        }

        public override void Activate()
        {
            base.Activate();
            this.Deactivate(true);
            this.mapPanel.MouseDown += MapPanel_MouseDown;
            this.mapPanel.MouseUp += MapPanel_MouseUp;
            this.mapPanel.MouseMove += MapPanel_MouseMove;
            this.mapPanel.MouseLeave += MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown += WallTool_KeyDown;
            (this.mapPanel as Control).KeyUp += WallTool_KeyUp;
            this.navigationWidget.MouseCellChanged += MouseoverWidget_MouseCellChanged;
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
            (this.mapPanel as Control).KeyDown -= WallTool_KeyDown;
            (this.mapPanel as Control).KeyUp -= WallTool_KeyUp;
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
                    wallTypeListBox.SelectedIndexChanged -= WallTypeListBox_SelectedIndexChanged;
                    Deactivate();
                }
                disposedValue = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
