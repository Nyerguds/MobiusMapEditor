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
    public class ResourcesTool : ViewTool
    {
        /// <summary>
        /// Layers that are not painted by the PostRenderMap function on ViewTool level because they are handled
        /// at a specific point in the PostRenderMap override by the implementing tool.
        /// </summary>
        protected override MapLayerFlag ManuallyHandledLayers => MapLayerFlag.None;

        private readonly Label boundsResourcesLbl;
        private readonly NumericUpDown brushSizeNud;
        private readonly CheckBox gemsCheckBox;

        private bool placementMode;
        private bool additivePlacement;

        protected override Boolean InPlacementMode
        {
            get { return placementMode || (Control.ModifierKeys & Keys.Shift) == Keys.Shift; }
        }

        private readonly Dictionary<int, Overlay> undoOverlays = new Dictionary<int, Overlay>();
        private readonly Dictionary<int, Overlay> redoOverlays = new Dictionary<int, Overlay>();

        public ResourcesTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, Label boundsResourcesLbl,
            NumericUpDown brushSizeNud, CheckBox gemsCheckBox, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> url)
            : base(mapPanel, layers, statusLbl, plugin, url)
        {
            this.boundsResourcesLbl = boundsResourcesLbl;
            this.brushSizeNud = brushSizeNud;
            this.gemsCheckBox = gemsCheckBox;
            this.brushSizeNud.ValueChanged += BrushSizeNud_ValueChanged;
            navigationWidget.MouseoverSize = new Size((int)brushSizeNud.Value, (int)brushSizeNud.Value);
            Update();
        }

        private void Url_UndoRedo(object sender, EventArgs e)
        {
            Update();
        }

        private void BrushSizeNud_ValueChanged(object sender, EventArgs e)
        {
            int actualValue = (int)brushSizeNud.Value | 1;
            if (brushSizeNud.Value != actualValue)
            {
                // Will re-trigger this, and then go to the other case.
                brushSizeNud.Value = actualValue;
            }
            else
            {
                navigationWidget.MouseoverSize = new Size((int)brushSizeNud.Value, (int)brushSizeNud.Value);
                CheckRedPenEdges();
            }
        }


        private void ResourcesTool_KeyUpDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey && sender is MapPanel mp)
            {
                mp.Invalidate();
            }
        }

        private void ResourceTool_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.PageDown)
            {
                brushSizeNud.DownButton();
                mapPanel.Invalidate();
            }
            else if (e.KeyCode == Keys.PageUp)
            {
                brushSizeNud.UpButton();
                mapPanel.Invalidate();
            }
            else
            {
                ResourcesTool_KeyUpDown(sender, e);
            }
        }

        private void MapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (!placementMode)
                {
                    EnterPlacementMode(true);
                    AddResource(navigationWidget.MouseCell);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (!placementMode)
                {
                    EnterPlacementMode(false);
                    RemoveResource(navigationWidget.MouseCell);
                }
            }
        }

        private void MapPanel_MouseLeave(object sender, EventArgs e)
        {
            ExitPlacementMode();
        }

        private void MapPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (placementMode)
            {
                if (((e.Button == MouseButtons.Left) && additivePlacement) ||
                    ((e.Button == MouseButtons.Right) && !additivePlacement))
                {
                    ExitPlacementMode();
                }
            }
            if ((undoOverlays.Count > 0) || (redoOverlays.Count > 0))
            {
                CommitChange();
            }
        }

        private void MouseoverWidget_MouseCellChanged(object sender, MouseCellChangedEventArgs e)
        {
            CheckRedPenEdges();
            if (placementMode)
            {
                if (additivePlacement)
                {
                    AddResource(e.NewCell);
                }
                else
                {
                    RemoveResource(e.NewCell);
                }
            }
        }

        private void AddResource(Point location)
        {
            OverlayType resourceType = gemsCheckBox.Checked ?
                map.OverlayTypes.Where(t => t.IsGem).FirstOrDefault() :
                map.OverlayTypes.Where(t => t.IsTiberiumOrGold).FirstOrDefault();
            if (resourceType == null)
            {
                return;
            }
            Rectangle rectangle = new Rectangle(location, new Size(1, 1));
            rectangle.Inflate(navigationWidget.MouseoverSize.Width / 2, navigationWidget.MouseoverSize.Height / 2);
            foreach (Point subLocation in rectangle.Points())
            {
                // Can't place overlay on top and bottom row. See OverlayClass::Read_INI
                if (subLocation.Y == 0 || subLocation.Y == map.Metrics.Height - 1)
                {
                    continue;
                }
                if (map.Metrics.GetCell(subLocation, out int cell))
                {
                    if (map.Overlay[cell] == null)
                    {
                        if (!undoOverlays.ContainsKey(cell))
                        {
                            undoOverlays[cell] = map.Overlay[cell];
                        }
                        Overlay overlay = new Overlay { Type = resourceType, Icon = 0 };
                        map.Overlay[cell] = overlay;
                        redoOverlays[cell] = overlay;
                    }
                }
            }
            rectangle.Inflate(1, 1);
            mapPanel.Invalidate(map, rectangle);
            Update();
        }

        private void RemoveResource(Point location)
        {
            Rectangle rectangle = new Rectangle(location, new Size(1, 1));
            rectangle.Inflate(navigationWidget.MouseoverSize.Width / 2, navigationWidget.MouseoverSize.Height / 2);
            foreach (Point subLocation in rectangle.Points())
            {
                if (map.Metrics.GetCell(subLocation, out int cell))
                {
                    if (map.Overlay[cell]?.Type.IsResource ?? false)
                    {
                        if (!undoOverlays.ContainsKey(cell))
                        {
                            undoOverlays[cell] = map.Overlay[cell];
                        }
                        map.Overlay[cell] = null;
                        redoOverlays[cell] = null;
                    }
                }
            }
            rectangle.Inflate(1, 1);
            mapPanel.Invalidate(map, rectangle);
            Update();
        }

        private void CheckRedPenEdges()
        {
            // Only check if size is 1x1; otherwise it'll be marked by PostRenderMap.
            if (navigationWidget.MouseoverSize.Width == 1 && navigationWidget.MouseoverSize.Height == 1
                && (navigationWidget.MouseCell.Y == 0 || navigationWidget.MouseCell.Y == map.Metrics.Height - 1))
            {
                navigationWidget.PenColor = Color.Red;
            }
            else
            {
                navigationWidget.PenColor = Color.Yellow;
            }
        }

        private void EnterPlacementMode(bool additive)
        {
            if (placementMode)
            {
                return;
            }
            placementMode = true;
            additivePlacement = additive;
            UpdateStatus();
        }

        private void ExitPlacementMode()
        {
            if (!placementMode)
            {
                return;
            }
            placementMode = false;
            mapPanel.Invalidate();
            UpdateStatus();
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
                    Rectangle rectangle = new Rectangle(location, new Size(1, 1));
                    rectangle.Inflate(1, 1);
                    return rectangle;
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
                    Rectangle rectangle = new Rectangle(location, new Size(1, 1));
                    rectangle.Inflate(1, 1);
                    return rectangle;
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

        private void Update()
        {
            boundsResourcesLbl.Text = map.ResourcesOnMap.ToString();
            if (map.OverlayTypes.Any(t => t.IsGem))
            {
                gemsCheckBox.Visible = true;
            }
            else
            {
                gemsCheckBox.Visible = false;
                gemsCheckBox.Checked = false;
            }
        }

        private void UpdateStatus()
        {
            if (placementMode)
            {
                if (additivePlacement)
                {
                    statusLbl.Text = "Drag mouse to add resources";
                }
                else
                {
                    statusLbl.Text = "Drag mouse to remove resources";
                }
            }
            else
            {
                statusLbl.Text = "Left-Click drag to add resources, Right-Click drag to remove resources";
            }
        }

        protected override void PostRenderMap(Graphics graphics, Rectangle visibleCells)
        {
            base.PostRenderMap(graphics, visibleCells);
            // For bounds, add one more cell to get all borders showing.
            Rectangle boundRenderCells = visibleCells;
            boundRenderCells.Inflate(1, 1);
            boundRenderCells.Intersect(map.Metrics.Bounds);
            List<Point> redCells = new List<Point>();
            Rectangle rectangle = new Rectangle(navigationWidget.MouseCell, new Size(1, 1));
            rectangle.Inflate(navigationWidget.MouseoverSize.Width / 2, navigationWidget.MouseoverSize.Height / 2);
            int lastRow = map.Metrics.Height - 1;
            foreach (Point subLocation in rectangle.Points())
            {
                // Can't place overlay on top and bottom row. See OverlayClass::Read_INI
                if ((subLocation.Y == 0 || subLocation.Y == lastRow) && boundRenderCells.Contains(subLocation))
                {
                    redCells.Add(subLocation);
                }
            }
            (Point, Overlay) inBounds(Map myMap, Overlay ovl, int cell, bool ifInBounds)
            {
                if (ovl.Type.IsResource && myMap.Metrics.GetLocation(cell, out Point location) && boundRenderCells.Contains(location))
                {
                    bool isInBounds = myMap.Bounds.Contains(location);
                    return ((isInBounds && ifInBounds) || (!isInBounds && !ifInBounds)) ? (location, ovl) : (location, null);
                }
                return (Point.Empty, null);
            }
            Rectangle mapBounds = map.Bounds;
            MapRenderer.RenderAllBoundsFromPoint(graphics, boundRenderCells, Globals.MapTileSize, map.Overlay
                .Select(x => inBounds(map, x.Value, x.Cell, true)).Where(p => p.Item2 != null));
            MapRenderer.RenderAllBoundsFromPoint(graphics, boundRenderCells, Globals.MapTileSize, map.Overlay
                .Select(x => inBounds(map, x.Value, x.Cell, false)).Where(p => p.Item2 != null), Color.FromArgb(0x80, 0xFF, 0x40, 0x40));
            MapRenderer.RenderAllBoundsFromPoint(graphics, boundRenderCells, Globals.MapTileSize, redCells, Color.Red);
        }

        public override void Activate()
        {
            base.Activate();
            this.Deactivate(true);
            this.mapPanel.MouseDown += MapPanel_MouseDown;
            this.mapPanel.MouseUp += MapPanel_MouseUp;
            this.mapPanel.MouseLeave += MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown += ResourceTool_KeyDown;
            (this.mapPanel as Control).KeyUp += ResourcesTool_KeyUpDown;
            this.navigationWidget.MouseCellChanged += MouseoverWidget_MouseCellChanged;
            this.url.Undone += Url_UndoRedo;
            this.url.Redone += Url_UndoRedo;
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
                ExitPlacementMode();
                base.Deactivate();
            }
            this.mapPanel.MouseDown -= MapPanel_MouseDown;
            this.mapPanel.MouseUp -= MapPanel_MouseUp;
            this.mapPanel.MouseLeave -= MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown -= ResourceTool_KeyDown;
            (this.mapPanel as Control).KeyUp -= ResourcesTool_KeyUpDown;
            this.navigationWidget.MouseCellChanged -= MouseoverWidget_MouseCellChanged;
            this.url.Undone -= Url_UndoRedo;
            this.url.Redone -= Url_UndoRedo;
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
                    brushSizeNud.ValueChanged -= BrushSizeNud_ValueChanged;
                }
                disposedValue = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
