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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Tools
{
    public class WaypointsTool : ViewTool
    {
        /// <summary>
        /// Layers that are not painted by the PostRenderMap function on ViewTool level because they are handled
        /// at a specific point in the PostRenderMap override by the implementing tool.
        /// </summary>
        protected override MapLayerFlag ManuallyHandledLayers => MapLayerFlag.WaypointsIndic | MapLayerFlag.TechnoTriggers |
            MapLayerFlag.WaypointRadius | MapLayerFlag.HomeAreaBox;

        private readonly ComboBox waypointCombo;
        private readonly Button jumpToButton;

        private Map previewMap;
        protected override Map RenderMap => previewMap;

        private int lastSelectedIndex;
        // Simply uses the index.
        public override object CurrentObject
        {
            get { return lastSelectedIndex; }
            set
            {
                if (value is int index && index >= 0 && waypointCombo.Items.Count > index)
                {
                    lastSelectedIndex = index;
                    waypointCombo.SelectedIndex = index;
                }
            }
        }

        private bool placementMode;

        protected override bool InPlacementMode
        {
            get { return placementMode; }
        }

        public WaypointsTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, ComboBox waypointCombo, Button jumpToButton,
            IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> url)
            : base(mapPanel, layers, statusLbl, plugin, url)
        {
            previewMap = map;
            this.jumpToButton = jumpToButton;
            this.jumpToButton.Click += JumpToButton_Click;
            this.waypointCombo = waypointCombo;
            this.waypointCombo.DataSource = plugin.Map.Waypoints.ToArray();
            this.waypointCombo.SelectedIndexChanged += this.WaypointCombo_SelectedIndexChanged;
            navigationWidget.MouseCellChanged += MouseoverWidget_MouseCellChanged;
            UpdateStatus();
        }

        private void Url_UndoRedoDone(object sender, UndoRedoEventArgs ev)
        {
            // Only update this stuff if the undo/redo event was actually a waypoint change.
            if (!ev.Source.HasFlag(ToolType.Waypoint))
            {
                return;
            }
            int selectedIndex = waypointCombo.SelectedIndex;
            this.waypointCombo.SelectedIndexChanged -= this.WaypointCombo_SelectedIndexChanged;
            this.waypointCombo.DataSource = null;
            this.waypointCombo.Items.Clear();
            Waypoint[] wp = map.Waypoints.ToArray();
            this.waypointCombo.DataSource = wp;
            this.waypointCombo.SelectedIndexChanged += this.WaypointCombo_SelectedIndexChanged;
            Waypoint selected = null;
            if (selectedIndex >= 0 && selectedIndex < wp.Length)
            {
                lastSelectedIndex = selectedIndex;
                try
                {
                    waypointCombo.SelectedIndex = selectedIndex;
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Not sure if this can still happen but if it does... ignore it.
                }
                selected = wp[selectedIndex];
            }
            this.jumpToButton.Enabled = selected != null && selected.Cell.HasValue;
        }

        private void MapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (placementMode)
            {
                if (e.Button == MouseButtons.Left)
                {
                    SetWaypoint(navigationWidget.MouseCell);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    RemoveWaypoint(navigationWidget.MouseCell);
                }
            }
            else if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                PickWaypoint(navigationWidget.MouseCell, e.Button == MouseButtons.Right);
            }
        }

        private void MouseoverWidget_MouseCellChanged(object sender, MouseCellChangedEventArgs e)
        {
            Waypoint waypoint = null;
            if (placementMode)
            {
                int selected = waypointCombo.SelectedIndex;
                if (selected != -1)
                {
                    Waypoint[] wp = map.Waypoints;
                    waypoint = wp[selected];
                    if (waypoint.Cell.HasValue)
                    {
                        mapPanel.Invalidate(map, GetRefreshCells(waypoint, waypoint.Point.Value));
                    }
                }
                mapPanel.Invalidate(map, GetRefreshCells(waypoint, e.OldCell));
                mapPanel.Invalidate(map, GetRefreshCells(waypoint, e.NewCell));
            }
            else if (e.MouseButtons == MouseButtons.Left || e.MouseButtons == MouseButtons.Right)
            {
                PickWaypoint(e.NewCell, e.MouseButtons == MouseButtons.Right);
            }
        }

        private void WaypointsTool_KeyDown(object sender, KeyEventArgs e)
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

        private void WaypointsTool_KeyUp(object sender, KeyEventArgs e)
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

        private void MapPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta == 0 || (Control.ModifierKeys & Keys.Control) == Keys.None)
            {
                return;
            }
            KeyEventArgs keyArgs = new KeyEventArgs(e.Delta > 0 ? Keys.PageUp : Keys.PageDown);
            CheckSelectShortcuts(keyArgs);
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

        private void SetWaypoint(Point location)
        {
            if (!map.Metrics.GetCell(location, out int cell))
            {
                return;
            }
            int selected = waypointCombo.SelectedIndex;
            if (selected == -1)
            {
                return;
            }
            Waypoint[] wp = map.Waypoints;
            Waypoint waypoint = wp[selected];
            if (waypoint.Cell == cell)
            {
                return;
            }
            int? oldCell = waypoint.Cell;
            Point? oldPoint = waypoint.Point;
            waypoint.Cell = cell;
            CommitChange(waypoint, oldCell, cell);
            this.waypointCombo.SelectedIndexChanged -= this.WaypointCombo_SelectedIndexChanged;
            waypointCombo.DataSource = null;
            waypointCombo.Items.Clear();
            waypointCombo.DataSource = wp.ToArray();
            this.waypointCombo.SelectedIndexChanged += this.WaypointCombo_SelectedIndexChanged;
            waypointCombo.SelectedIndex = selected;
            if (oldPoint.HasValue)
            {
                mapPanel.Invalidate(map, GetRefreshCells(waypoint, oldPoint.Value));
            }
            mapPanel.Invalidate(map, GetRefreshCells(waypoint, location));
        }

        private void RemoveWaypoint(Point location)
        {
            if (!map.Metrics.GetCell(location, out int cell))
            {
                return;
            }
            Waypoint waypoint;
            Waypoint[] wp = map.Waypoints;
            int origIndex = waypointCombo.SelectedIndex;
            int waypointIndex;
            if (waypointCombo.SelectedItem is Waypoint selwp && selwp.Cell == cell)
            {
                waypoint = selwp;
                waypointIndex = waypointCombo.SelectedIndex;
            }
            else
            {
                waypointIndex = Enumerable.Range(0, wp.Length).Where(i => wp[i].Cell == cell).DefaultIfEmpty(-1).Last();
                waypoint = waypointIndex == -1 ? null : wp[waypointIndex];
            }
            if (waypoint != null)
            {
                int? oldCell = waypoint.Cell;
                waypoint.Cell = null;
                CommitChange(waypoint, oldCell, null);
                mapPanel.Invalidate(map, GetRefreshCells(waypoint, location));
                waypointCombo.DataSource = null;
                waypointCombo.Items.Clear();
                waypointCombo.DataSource = wp.ToArray();
                waypointCombo.SelectedIndex = origIndex;
            }
        }

        private void CheckSelectShortcuts(KeyEventArgs e)
        {
            int maxVal = waypointCombo.Items.Count - 1;
            int curVal = waypointCombo.SelectedIndex;
            int? newVal = null;
            if (Control.ModifierKeys == Keys.Shift)
            {
                switch (e.KeyCode)
                {
                    case Keys.F:
                        newVal = Enumerable.Range(0, map.Waypoints.Length).Cast<int?>().Where(i => (map.Waypoints[i.Value].Flags & WaypointFlag.Flare) == WaypointFlag.Flare).FirstOrDefault();
                        break;
                    case Keys.H:
                        newVal = Enumerable.Range(0, map.Waypoints.Length).Cast<int?>().Where(i => (map.Waypoints[i.Value].Flags & WaypointFlag.Home) == WaypointFlag.Home).FirstOrDefault();
                        break;
                    case Keys.R:
                        newVal = Enumerable.Range(0, map.Waypoints.Length).Cast<int?>().Where(i => (map.Waypoints[i.Value].Flags & WaypointFlag.Reinforce) == WaypointFlag.Reinforce).FirstOrDefault();
                        break;
                    case Keys.S:
                        newVal = Enumerable.Range(0, map.Waypoints.Length).Cast<int?>().Where(i => (map.Waypoints[i.Value].Flags & WaypointFlag.Special) == WaypointFlag.Special).FirstOrDefault();
                        break;
                }
            }
            // Allow these to work while shift is pressed.
            if (!newVal.HasValue)
            {
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
                    case Keys.Enter:
                        JumpToWaypoint();
                        break;
                }
            }
            if (newVal.HasValue && curVal != newVal.Value)
            {
                Waypoint oldWp = map.Waypoints[curVal];
                Waypoint newWp = map.Waypoints[newVal.Value];
                lastSelectedIndex = newVal.Value;
                waypointCombo.SelectedIndex = newVal.Value;
                if (placementMode)
                {
                    mapPanel.Invalidate(map, GetRefreshCells(oldWp, navigationWidget.MouseCell));
                    mapPanel.Invalidate(map, GetRefreshCells(newWp, navigationWidget.MouseCell));
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
            int selected = waypointCombo.SelectedIndex;
            Waypoint cur = selected != -1 ? map.Waypoints[selected] : null;
            mapPanel.Invalidate(map, GetRefreshCells(cur, navigationWidget.MouseCell));
            UpdateStatus();
        }

        private Rectangle GetRefreshCells(Waypoint waypoint, Point toRefresh)
        {
            if (Layers.HasFlag(MapLayerFlag.FootballArea) && waypoint != null && waypoint.Flags.HasFlag(WaypointFlag.FootballField))
            {
                return new Rectangle(toRefresh.X - 1, toRefresh.Y - 1, 4, 3);
            }
            else
            {
                return new Rectangle(toRefresh, new Size(1, 1));
            }
        }

        private void ExitPlacementMode()
        {
            if (!placementMode)
            {
                return;
            }
            navigationWidget.MouseoverSize = new Size(1, 1);
            navigationWidget.PenColor = Color.Yellow;
            placementMode = false;
            int selected = waypointCombo.SelectedIndex;
            Waypoint cur = selected != -1 ? map.Waypoints[selected] : null;
            mapPanel.Invalidate(map, GetRefreshCells(cur, navigationWidget.MouseCell));
            UpdateStatus();
        }

        private void PickWaypoint(Point location, bool backwards)
        {
            if (map.Metrics.GetCell(location, out int cell))
            {
                Waypoint[] wp = map.Waypoints;
                List<int> foundIndices = new List<int>();
                int curSelIndex = waypointCombo.SelectedIndex;
                int curIndex = -1;
                for (var i = 0; i < wp.Length; ++i)
                {
                    if (wp[i].Cell == cell)
                    {
                        foundIndices.Add(i);
                        if (i == curSelIndex)
                            curIndex = foundIndices.Count - 1;
                    }
                }
                if (foundIndices.Count == 0)
                {
                    // should be impossible but whatever.
                    return;
                }
                if (foundIndices.Count == 1 || curIndex == -1)
                {
                    lastSelectedIndex = foundIndices[0];
                    waypointCombo.SelectedIndex = foundIndices[0];
                    return;
                }
                int nextIndex = backwards ? curIndex - 1 : curIndex + 1;
                if (nextIndex < 0)
                    nextIndex = foundIndices.Count - 1;
                else if (nextIndex >= foundIndices.Count)
                    nextIndex = 0;
                lastSelectedIndex = foundIndices[nextIndex];
                waypointCombo.SelectedIndex = foundIndices[nextIndex];
            }
        }

        private void CommitChange(Waypoint waypoint, int? oldCell, int? newCell)
        {
            bool origEmptyState = plugin.Empty;
            plugin.Dirty = true;
            void undoAction(UndoRedoEventArgs ev)
            {
                waypoint.Cell = oldCell;
                if (newCell.HasValue)
                {
                    Point newPoint = ev.Map.Metrics.GetLocation(newCell.Value).Value;
                    ev.MapPanel.Invalidate(ev.Map, GetRefreshCells(waypoint, newPoint));
                }
                if (oldCell.HasValue)
                {
                    Point oldPoint = ev.Map.Metrics.GetLocation(oldCell.Value).Value;
                    ev.MapPanel.Invalidate(ev.Map, GetRefreshCells(waypoint, oldPoint));
                }
                if (ev.Plugin != null)
                {
                    ev.Plugin.Empty = origEmptyState;
                    ev.Plugin.Dirty = !ev.NewStateIsClean;
                }
            }
            void redoAction(UndoRedoEventArgs ev)
            {
                waypoint.Cell = newCell;
                if (newCell.HasValue)
                {
                    Point newPoint = ev.Map.Metrics.GetLocation(newCell.Value).Value;
                    ev.MapPanel.Invalidate(ev.Map, GetRefreshCells(waypoint, newPoint));
                }
                if (oldCell.HasValue)
                {
                    Point oldPoint = ev.Map.Metrics.GetLocation(oldCell.Value).Value;
                    ev.MapPanel.Invalidate(ev.Map, GetRefreshCells(waypoint, oldPoint));
                }
                if (ev.Plugin != null)
                {
                    // Redo can never restore the "empty" state, but CAN be the point at which a save was done.
                    ev.Plugin.Empty = false;
                    ev.Plugin.Dirty = !ev.NewStateIsClean;
                }
            }
            Waypoint selected = waypointCombo.SelectedItem as Waypoint;
            jumpToButton.Enabled = selected != null && selected.Cell.HasValue;
            url.Track(undoAction, redoAction, ToolType.Waypoint);
        }

        private void WaypointCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            Waypoint selected = waypointCombo.SelectedItem as Waypoint;
            jumpToButton.Enabled = selected != null && selected.Cell.HasValue;
            if (selected != null && selected.Cell.HasValue)
            {
                lastSelectedIndex = waypointCombo.SelectedIndex;
                mapPanel.Invalidate(RenderMap, GetRefreshCells(selected, selected.Point.Value));
            }
            mapPanel.Invalidate(RenderMap, GetRefreshCells(selected, navigationWidget.MouseCell));
        }

        private void JumpToButton_Click(object sender, EventArgs e)
        {
            JumpToWaypoint();
        }

        protected void JumpToWaypoint()
        {
            if (!(waypointCombo.SelectedItem is Waypoint wp))
            {
                return;
            }
            int cell = wp.Cell.GetValueOrDefault(-1);
            if (cell == -1)
            {
                return;
            }
            Point cellPoint;
            if (!RenderMap.Metrics.GetLocation(cell, out cellPoint))
            {
                return;
            }
            mapPanel.JumpToPosition(map.Metrics, cellPoint, false);
            navigationWidget.Refresh();
            this.UpdateStatus();
            this.RefreshMainWindowMouseInfo();
        }

        protected override void PreRenderMap()
        {
            base.PreRenderMap();
            previewMap = map.Clone(true);
            if (!placementMode)
            {
                return;
            }
            int selectedIndex = waypointCombo.SelectedIndex;
            if (selectedIndex == -1)
            {
                return;
            }
            Waypoint orig = previewMap.Waypoints[selectedIndex];
            var location = navigationWidget.MouseCell;
            if (!previewMap.Metrics.GetCell(location, out int cell))
            {
                return;
            }
            // Add placement mode dummy in extra slot provided for this purpose on cloned maps. This if-check should always pass.
            if (previewMap.Waypoints.Length == map.Waypoints.Length + 1)
            {
                selectedIndex = map.Waypoints.Length;
                previewMap.Waypoints[selectedIndex] = new Waypoint(orig.Name, orig.ShortName, orig.Flags, orig.Metrics);
            }
            previewMap.Waypoints[selectedIndex].Cell = cell;
            previewMap.Waypoints[selectedIndex].IsPreview = true;
        }

        protected override void PostRenderMap(Graphics graphics, Rectangle visibleCells)
        {
            base.PostRenderMap(graphics, visibleCells);
            // For bounds, add one more cell to get all borders showing.
            Rectangle boundRenderCells = visibleCells;
            boundRenderCells.Inflate(1, 1);
            boundRenderCells.Intersect(map.Metrics.Bounds);
            GameInfo gameInfo = plugin.GameInfo;
            int selectedIndex = waypointCombo.SelectedIndex;
            Waypoint selected = waypointCombo.SelectedItem as Waypoint;
            Waypoint dummySelected = null;
            Waypoint[] selectedRange = selected != null ? new [] { selected } : new Waypoint[] { };
            // Render those here so they are put over the opaque redraw of the current waypoint.
            if (Layers.HasFlag(MapLayerFlag.TechnoTriggers))
            {
                MapRenderer.RenderAllTechnoTriggers(graphics, gameInfo, plugin.Map, visibleCells, Globals.MapTileSize, Layers);
            }
            if (selected != null)
            {
                if (placementMode && selectedIndex >= 0)
                {
                    selected = previewMap.Waypoints[selectedIndex];
                    dummySelected = previewMap.Waypoints.Length == map.Waypoints.Length + 1 ? previewMap.Waypoints[map.Waypoints.Length] : null;
                }
            }
            // Render Home waypoint box.
            if (plugin.Map.BasicSection.SoloMission && (Layers.HasFlag(MapLayerFlag.HomeAreaBox) || selected.Flags.HasFlag(WaypointFlag.Home)))
            {
                Waypoint home = previewMap.Waypoints.FirstOrDefault(w => w.Flags.HasFlag(WaypointFlag.Home) && !w.IsPreview);
                bool renderDummy = dummySelected != null && dummySelected.Cell.HasValue && dummySelected.Flags.HasFlag(WaypointFlag.Home)
                    && (home == null || !home.Cell.HasValue || dummySelected.Cell.Value != home.Cell.Value);
                Color color = selected.Flags.HasFlag(WaypointFlag.Home) && !renderDummy ? Color.Yellow : Color.Orange;
                if (home.Cell.HasValue)
                {
                    MapRenderer.RenderHomeWayPointBox(graphics, plugin, map, boundRenderCells, Globals.MapTileSize, home, color);
                }
                if (renderDummy)
                {
                    MapRenderer.RenderHomeWayPointBox(graphics, plugin, previewMap, boundRenderCells, Globals.MapTileSize, dummySelected, Color.Yellow);
                }
            }
            MapRenderer.RenderAllBoundsFromCell(graphics, boundRenderCells, Globals.MapTileSize,
                map.Waypoints.Where(wp => wp != selected && wp.Cell.HasValue).Select(wp => wp.Cell.Value), map.Metrics, Color.Orange);
            // If the plugin has a dedicated "flare" waypoint, then it is hardcoded and the only one, and should always be rendered.
            bool renderAll = plugin.Map.FlareWaypointAvailable || Layers.HasFlag(MapLayerFlag.WaypointRadius);
            MapRenderer.RenderAllWayPointRevealRadiuses(graphics, plugin, map, boundRenderCells, Globals.MapTileSize, selected, !renderAll);
            MapRenderer.RenderWayPointIndicators(graphics, map, gameInfo, visibleCells, Globals.MapTileSize, Color.LightGreen, false, true, selectedRange);
            if (selected != null)
            {
                if (selected.Cell.HasValue)
                {
                    MapRenderer.RenderAllBoundsFromCell(graphics, boundRenderCells, Globals.MapTileSize, new int[] { selected.Cell.Value }, map.Metrics, Color.Yellow);
                    MapRenderer.RenderWayPointIndicators(graphics, map, gameInfo, visibleCells, Globals.MapTileSize, Color.Yellow, false, false, selectedRange);
                }
                if (dummySelected != null && (selected == null || selected.Cell != dummySelected.Cell))
                {
                    MapRenderer.RenderWayPointIndicators(graphics, map, gameInfo, visibleCells, Globals.MapTileSize, Color.Yellow, true, false, new[] { dummySelected });
                    // Need to do this manually since it's an extra waypoint not normally on the list, and it uses the radius data of the original waypoint to place.
                    int[] wpReveal1 = plugin.GetRevealRadiusForWaypoints(false);
                    int[] wpReveal2 = plugin.GetRevealRadiusForWaypoints(true);
                    if (wpReveal1[selectedIndex] != 0)
                    {
                        MapRenderer.RenderWayPointRevealRadius(graphics, map.Metrics, boundRenderCells, Globals.MapTileSize, Color.Yellow, true, true, wpReveal1[selectedIndex], dummySelected);
                    }
                    if (wpReveal2[selectedIndex] != 0)
                    {
                        MapRenderer.RenderWayPointRevealRadius(graphics, map.Metrics, boundRenderCells, Globals.MapTileSize, Color.Yellow, true, true, wpReveal2[selectedIndex], dummySelected);
                    }
                }
            }
        }

        public override void UpdateStatus()
        {
            WaypointFlag flags = WaypointFlag.None;
            Waypoint[] wps = map.Waypoints;
            int wplen = map.Waypoints.Length;
            // Collect all existing flags in the list.
            for (int i = 0; i < wplen; ++i) {
                flags |= wps[i].Flags;
            }
            List<string> specialKeys = new List<string>();
            if (flags.HasFlag(WaypointFlag.Flare))
            {
                specialKeys.Add("F");
            }
            if (flags.HasFlag(WaypointFlag.Home))
            {
                specialKeys.Add("H");
            }
            if (flags.HasFlag(WaypointFlag.Reinforce))
            {
                specialKeys.Add("R");
            }
            if (flags.HasFlag(WaypointFlag.Special))
            {
                specialKeys.Add("S");
            }
            string special = String.Join("/", specialKeys);
            if (placementMode)
            {
                statusLbl.Text = "Left-Click to set cell waypoint, Right-Click to clear cell waypoint, " + special + " to select a special waypoint";
            }
            else
            {
                statusLbl.Text = "Shift to enter placement mode, Left-Click or Right-Click to pick cell waypoint, Shift + " + special + " to select a special waypoint, Enter to jump to cell waypoint location";
            }
        }

        private void Map_WaypointsUpdated(object sender, EventArgs e)
        {
            this.waypointCombo.SelectedIndexChanged -= this.WaypointCombo_SelectedIndexChanged;
            int selected = waypointCombo.SelectedIndex;
            mapPanel.Invalidate();
            waypointCombo.DataSource = null;
            waypointCombo.Items.Clear();
            waypointCombo.DataSource = map.Waypoints.ToArray();
            waypointCombo.SelectedIndex = selected;
            this.waypointCombo.SelectedIndexChanged += this.WaypointCombo_SelectedIndexChanged;
            WaypointCombo_SelectedIndexChanged(null, null);
        }

        public override void Activate()
        {
            base.Activate();
            Deactivate(true);
            (this.mapPanel as Control).KeyDown += WaypointsTool_KeyDown;
            (this.mapPanel as Control).KeyUp += WaypointsTool_KeyUp;
            this.mapPanel.MouseDown += MapPanel_MouseDown;
            this.mapPanel.MouseMove += MapPanel_MouseMove;
            this.mapPanel.MouseLeave += MapPanel_MouseLeave;
            this.mapPanel.MouseWheel += MapPanel_MouseWheel;
            this.mapPanel.SuspendMouseZoomKeys = Keys.Control;
            this.map.WaypointsUpdated += this.Map_WaypointsUpdated;
            this.navigationWidget.MouseCellChanged += MouseoverWidget_MouseCellChanged;
            this.url.Undone += Url_UndoRedoDone;
            this.url.Redone += Url_UndoRedoDone;
            this.UpdateStatus();
        }

        public override void Deactivate()
        {
            Deactivate(false);
        }

        public void Deactivate(bool forActivate)
        {
            if (!forActivate)
            {
                ExitPlacementMode();
                base.Deactivate();
            }
            this.map.WaypointsUpdated -= this.Map_WaypointsUpdated;
            this.mapPanel.MouseDown -= MapPanel_MouseDown;
            this.mapPanel.MouseMove -= MapPanel_MouseMove;
            this.mapPanel.MouseLeave -= MapPanel_MouseLeave;
            this.mapPanel.MouseWheel -= MapPanel_MouseWheel;
            this.mapPanel.SuspendMouseZoomKeys = Keys.None;
            (this.mapPanel as Control).KeyDown -= WaypointsTool_KeyDown;
            (this.mapPanel as Control).KeyUp -= WaypointsTool_KeyUp;
            this.navigationWidget.MouseCellChanged -= MouseoverWidget_MouseCellChanged;
            this.url.Undone -= Url_UndoRedoDone;
            this.url.Redone -= Url_UndoRedoDone;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.jumpToButton.Click -= JumpToButton_Click;
                    this.waypointCombo.SelectedIndexChanged -= this.WaypointCombo_SelectedIndexChanged;
                    Deactivate();
                }
                disposedValue = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
