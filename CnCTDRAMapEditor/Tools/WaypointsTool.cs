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
    public class WaypointsTool : ViewTool
    {
        /// <summary>
        /// Layers that are not painted by the PostRenderMap function on ViewTool level because they are handled
        /// at a specific point in the PostRenderMap override by the implementing tool.
        /// </summary>
        protected override MapLayerFlag ManuallyHandledLayers => MapLayerFlag.WaypointsIndic | MapLayerFlag.TechnoTriggers | MapLayerFlag.WaypointRadius;

        private readonly ComboBox waypointCombo;
        private readonly Button jumpToButton;

        private Map previewMap;
        protected override Map RenderMap => previewMap;

        private bool placementMode;

        protected override Boolean InPlacementMode
        {
            get { return placementMode; }
        }

        public WaypointsTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, ComboBox waypointCombo, Button jumpToButton,
            IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> url)
            : base(mapPanel, layers, statusLbl, plugin, url)
        {
            previewMap = map;
            this.jumpToButton = jumpToButton;
            this.jumpToButton.Click += JumpToButton_Click;
            this.waypointCombo = waypointCombo;
            this.waypointCombo.DisplayMember = "";
            this.waypointCombo.DataSource = plugin.Map.Waypoints.ToArray();
            this.waypointCombo.SelectedIndexChanged += this.WaypointCombo_SelectedIndexChanged;
            navigationWidget.MouseCellChanged += MouseoverWidget_MouseCellChanged;
            this.WaypointCombo_SelectedIndexChanged(null, null);
            UpdateStatus();
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
            if (placementMode)
            {
                int selected = waypointCombo.SelectedIndex;
                if (selected != -1)
                {
                    Waypoint[] wp = map.Waypoints;
                    var waypoint = wp[selected];
                    if (waypoint.Cell.HasValue)
                    {
                        mapPanel.Invalidate(map, waypoint.Cell.Value);
                    }
                }
                mapPanel.Invalidate(map, e.OldCell);
                mapPanel.Invalidate(map, e.NewCell);
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
            var waypoint = wp[selected];
            if (waypoint.Cell == cell)
            {
                return;
            }
            int? oldCell = waypoint.Cell;
            waypoint.Cell = cell;
            CommitChange(waypoint, oldCell, cell);
            waypointCombo.DataSource = null;
            waypointCombo.Items.Clear();
            waypointCombo.DataSource = wp.ToArray();
            waypointCombo.SelectedIndex = selected;
            if (oldCell.HasValue)
            {
                mapPanel.Invalidate(map, oldCell.Value);
            }
            mapPanel.Invalidate(map, location);
        }

        private void RemoveWaypoint(Point location)
        {
            if (!map.Metrics.GetCell(location, out int cell))
            {
                return;
            }
            Waypoint waypoint;
            Waypoint[] wp = map.Waypoints;
            int waypointIndex;
            if (waypointCombo.SelectedItem is Waypoint selwp && selwp.Cell == cell)
            {
                waypoint = selwp;
                waypointIndex = waypointCombo.SelectedIndex;
            }
            else
            {
                waypointIndex = Enumerable.Range(0, wp.Length).Where(i => wp[i].Cell == cell).FirstOrDefault();
                // why doesn't "FirstOrDefault" allow GIVING a default? Bah.
                if (waypointIndex == 0 && wp[0].Cell != cell)
                    waypointIndex = -1;
                waypoint = waypointIndex == -1 ? null : wp[waypointIndex];
            }
            if (waypoint != null)
            {
                int? oldCell = waypoint.Cell;
                waypoint.Cell = null;
                CommitChange(waypoint, oldCell, null);
                mapPanel.Invalidate(map, location);
                waypointCombo.DataSource = null;
                waypointCombo.Items.Clear();
                waypointCombo.DataSource = wp.ToArray();
                waypointCombo.SelectedIndex = waypointIndex;
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
                        newVal = Enumerable.Range(0, map.Waypoints.Length).Cast<int?>().Where(i => (map.Waypoints[i.Value].Flag & WaypointFlag.Flare) == WaypointFlag.Flare).FirstOrDefault();
                        break;
                    case Keys.H:
                        newVal = Enumerable.Range(0, map.Waypoints.Length).Cast<int?>().Where(i => (map.Waypoints[i.Value].Flag & WaypointFlag.Home) == WaypointFlag.Home).FirstOrDefault();
                        break;
                    case Keys.R:
                        newVal = Enumerable.Range(0, map.Waypoints.Length).Cast<int?>().Where(i => (map.Waypoints[i.Value].Flag & WaypointFlag.Reinforce) == WaypointFlag.Reinforce).FirstOrDefault();
                        break;
                    case Keys.S:
                        newVal = Enumerable.Range(0, map.Waypoints.Length).Cast<int?>().Where(i => (map.Waypoints[i.Value].Flag & WaypointFlag.Special) == WaypointFlag.Special).FirstOrDefault();
                        break;
                }
            }
            else
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
                if (placementMode)
                {
                    int selected = waypointCombo.SelectedIndex;
                    if (selected != -1)
                    {
                        Waypoint[] wp = map.Waypoints;
                        var waypoint = wp[selected];
                        if (waypoint.Cell.HasValue)
                        {
                            mapPanel.Invalidate(map, waypoint.Cell.Value);
                        }
                    }
                }
                waypointCombo.SelectedIndex = newVal.Value;
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
            int selected = waypointCombo.SelectedIndex;
            if (selected != -1)
            {
                Waypoint[] wp = map.Waypoints;
                var waypoint = wp[selected];
                if (waypoint.Cell.HasValue)
                {
                    mapPanel.Invalidate(map, waypoint.Cell.Value);
                }
            }
            mapPanel.Invalidate(map, navigationWidget.MouseCell);
            UpdateStatus();
        }

        private void ExitPlacementMode()
        {
            if (!placementMode)
            {
                return;
            }
            placementMode = false;
            int selected = waypointCombo.SelectedIndex;
            if (selected != -1)
            {
                Waypoint[] wp = map.Waypoints;
                var waypoint = wp[selected];
                if (waypoint.Cell.HasValue)
                {
                    mapPanel.Invalidate(map, waypoint.Cell.Value);
                }
            }
            mapPanel.Invalidate(map, navigationWidget.MouseCell);
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
                    waypointCombo.SelectedIndex = foundIndices[0];
                    return;
                }
                int nextIndex = backwards ? curIndex - 1 : curIndex + 1;
                if (nextIndex < 0)
                    nextIndex = foundIndices.Count - 1;
                else if (nextIndex >= foundIndices.Count)
                    nextIndex = 0;
                waypointCombo.SelectedIndex = foundIndices[nextIndex];
            }
        }

        private void CommitChange(Waypoint waypoint, int? oldCell, int? newCell)
        {
            bool origDirtyState = plugin.Dirty;
            plugin.Dirty = true;
            void undoAction(UndoRedoEventArgs e)
            {
                waypoint.Cell = oldCell;
                if (newCell.HasValue)
                    e.MapPanel.Invalidate(e.Map, newCell.Value);
                if (oldCell.HasValue)
                    e.MapPanel.Invalidate(e.Map, oldCell.Value);
                if (e.Plugin != null)
                {
                    e.Plugin.Dirty = origDirtyState;
                }
            }
            void redoAction(UndoRedoEventArgs e)
            {
                waypoint.Cell = newCell;
                if (newCell.HasValue)
                    e.MapPanel.Invalidate(e.Map, newCell.Value);
                if (oldCell.HasValue)
                    e.MapPanel.Invalidate(e.Map, oldCell.Value);
                if (e.Plugin != null)
                {
                    e.Plugin.Dirty = true;
                }
            }
            url.Track(undoAction, redoAction);
        }

        private void WaypointCombo_SelectedIndexChanged(Object sender, EventArgs e)
        {
            Waypoint selected = waypointCombo.SelectedItem as Waypoint;
            jumpToButton.Enabled = selected != null && selected.Cell.HasValue;
            if (selected != null && selected.Cell.HasValue)
            {
                mapPanel.Invalidate(RenderMap, selected.Cell.Value);
            }
            mapPanel.Invalidate(RenderMap, navigationWidget.MouseCell);
        }

        private void JumpToButton_Click(Object sender, EventArgs e)
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
        }

        protected override void PreRenderMap()
        {
            base.PreRenderMap();
            previewMap = map.Clone(true);
            if (!placementMode)
            {
                return;
            }
            // Add placement mode dummy in extra slot provided for this purpose on cloned maps.
            int selectedIndex = waypointCombo.SelectedIndex;
            if (selectedIndex == -1)
            {
                return;
            }
            Waypoint orig = previewMap.Waypoints[selectedIndex];
            var location = navigationWidget.MouseCell;
            if(!previewMap.Metrics.GetCell(location, out int cell))
            {
                return;
            }
            if (previewMap.Waypoints.Length == map.Waypoints.Length + 1)
            {
                selectedIndex = map.Waypoints.Length;
                previewMap.Waypoints[selectedIndex] = new Waypoint(orig.Name, orig.Flag, orig.Metrics);
            }
            previewMap.Waypoints[selectedIndex].Cell = cell;
            previewMap.Waypoints[selectedIndex].Tint = Color.FromArgb(128, Color.White);
        }

        protected override void PostRenderMap(Graphics graphics)
        {
            base.PostRenderMap(graphics);
            int selectedIndex = waypointCombo.SelectedIndex;
            Waypoint selected = waypointCombo.SelectedItem as Waypoint;
            Waypoint dummySelected = null;
            Waypoint[] selectedRange = selected != null ? new [] { selected } : new Waypoint[] { };
            if (plugin.GameType == GameType.SoleSurvivor && (Layers & MapLayerFlag.FootballArea) == MapLayerFlag.FootballArea)
            {
                MapRenderer.RenderAllFootballAreas(graphics, map, Globals.MapTileSize, Globals.MapTileScale, plugin.GameType);
                MapRenderer.RenderFootballAreaFlags(graphics, plugin.GameType, map, Globals.MapTileSize);
            }
            // If the selected waypoint is not a flag, re-render it as opaque.
            if (selected != null && (plugin.Map.BasicSection.SoloMission || (selected.Flag & WaypointFlag.PlayerStart) != WaypointFlag.PlayerStart))
            {
                MapRenderer.Render(plugin.GameType, true, map.Theater, Globals.MapTileSize, map.FlagColors.ToArray(), selected, 1.0f).Item2(graphics);
            }
            // Render those here to they are put over the opaque redraw of the current waypoint.
            MapRenderer.RenderAllTechnoTriggers(graphics, plugin.Map, Globals.MapTileSize, Globals.MapTileScale, Layers);
            MapRenderer.RenderAllBoundsFromCell(graphics, Globals.MapTileSize,
                map.Waypoints.Where(wp => wp != selected && wp.Cell.HasValue).Select(wp => wp.Cell.Value), map.Metrics, Color.Orange);
            // For TD, always render reveal waypoint.
            bool renderAll = plugin.GameType != GameType.RedAlert || (Layers & MapLayerFlag.WaypointRadius) == MapLayerFlag.WaypointRadius;
            MapRenderer.RenderAllWayPointRevealRadiuses(graphics, plugin, map, Globals.MapTileSize, selected, !renderAll);
            MapRenderer.RenderWayPointIndicators(graphics, map, Globals.MapTileSize, Globals.MapTileScale, Color.LightGreen, false, true, selectedRange);
            if (selected != null)
            {
                if (placementMode && selectedIndex >= 0)
                {
                    selected = previewMap.Waypoints[selectedIndex];
                    dummySelected = previewMap.Waypoints.Length == map.Waypoints.Length + 1 ? previewMap.Waypoints[map.Waypoints.Length] : null;
                }
                if (selected.Cell.HasValue)
                {
                    MapRenderer.RenderAllBoundsFromCell(graphics, Globals.MapTileSize, new int[] { selected.Cell.Value }, map.Metrics, Color.Yellow);
                    MapRenderer.RenderWayPointIndicators(graphics, map, Globals.MapTileSize, Globals.MapTileScale, Color.Yellow, false, false, selectedRange);
                }
                if (dummySelected != null && (selected == null || selected.Cell != dummySelected.Cell))
                {
                    MapRenderer.RenderWayPointIndicators(graphics, map, Globals.MapTileSize, Globals.MapTileScale, Color.Yellow, true, false, new[] { dummySelected });
                    // Need to do this manually since it's an extra waypoint not normally on the list, and it uses the radius data of the original waypoint to place.
                    int[] wpReveal1 = plugin.GetRevealRadiusForWaypoints(map, false);
                    int[] wpReveal2 = plugin.GetRevealRadiusForWaypoints(map, true);
                    if (wpReveal1[selectedIndex] != 0)
                    {
                        MapRenderer.RenderWayPointRevealRadius(graphics, map.Metrics, Globals.MapTileSize, Color.Yellow, true, true, wpReveal1[selectedIndex], dummySelected);
                    }
                    if (wpReveal2[selectedIndex] != 0)
                    {
                        MapRenderer.RenderWayPointRevealRadius(graphics, map.Metrics, Globals.MapTileSize, Color.Yellow, true, true, wpReveal2[selectedIndex], dummySelected);
                    }
                }
            }
        }

        private void UpdateStatus()
        {
            WaypointFlag flag = WaypointFlag.None;
            Waypoint[] wps = map.Waypoints;
            int wplen = map.Waypoints.Length;
            for (int i = 0; i < wplen; ++i) {
                flag |= wps[i].Flag;
            }
            List<String> specialKeys = new List<string>();
            if ((flag & WaypointFlag.Flare) != WaypointFlag.None)
            {
                specialKeys.Add("F");
            }
            if ((flag & WaypointFlag.Home) != WaypointFlag.None)
            {
                specialKeys.Add("H");
            }
            if ((flag & WaypointFlag.Reinforce) != WaypointFlag.None)
            {
                specialKeys.Add("R");
            }
            if ((flag & WaypointFlag.Special) != WaypointFlag.None)
            {
                specialKeys.Add("S");
            }

            if (placementMode)
            {
                statusLbl.Text = "Left-Click to set cell waypoint, Right-Click to clear cell waypoint, " + String.Join("/", specialKeys) + " to select a special waypoint";
            }
            else
            {
                statusLbl.Text = "Shift to enter placement mode, Left-Click or Right-Click to pick cell waypoint, Shift + " + String.Join("/", specialKeys) + " to select a special waypoint";
            }
        }

        private void Map_WaypointsUpdated(Object sender, EventArgs e)
        {
            this.waypointCombo.SelectedIndexChanged -= this.WaypointCombo_SelectedIndexChanged;
            int selected = waypointCombo.SelectedIndex;
            mapPanel.Invalidate();
            waypointCombo.DataSource = null;
            waypointCombo.Items.Clear();
            waypointCombo.DataSource = map.Waypoints.ToArray();
            waypointCombo.SelectedIndex = selected;
            this.waypointCombo.SelectedIndexChanged += this.WaypointCombo_SelectedIndexChanged;
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
            this.map.WaypointsUpdated += this.Map_WaypointsUpdated;
            navigationWidget.MouseCellChanged += MouseoverWidget_MouseCellChanged;
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
            (this.mapPanel as Control).KeyDown -= WaypointsTool_KeyDown;
            (this.mapPanel as Control).KeyUp -= WaypointsTool_KeyUp;
            navigationWidget.MouseCellChanged -= MouseoverWidget_MouseCellChanged;
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
