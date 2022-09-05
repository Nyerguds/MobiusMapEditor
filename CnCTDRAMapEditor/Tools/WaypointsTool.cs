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
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Tools
{
    public class WaypointsTool : ViewTool
    {
        /// <summary> Layers that are important to this tool and need to be drawn last in the PostRenderMap process.</summary>
        protected override MapLayerFlag PriorityLayers => MapLayerFlag.None;
        /// <summary>
        /// Layers that are not painted by the PostRenderMap function on ViewTool level because they are handled
        /// at a specific point in the PostRenderMap override by the implementing tool.
        /// </summary>
        protected override MapLayerFlag ManuallyHandledLayers => MapLayerFlag.Waypoints;

        private readonly ComboBox waypointCombo;
        private readonly Button jumpToButton;


        private (Waypoint waypoint, int? cell)? undoWaypoint;
        private (Waypoint waypoint, int? cell)? redoWaypoint;

        private bool placementMode;

        public WaypointsTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, ComboBox waypointCombo, Button jumpToButton,
            IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> url)
            : base(mapPanel, layers, statusLbl, plugin, url)
        {
            this.jumpToButton = jumpToButton;
            this.jumpToButton.Click += JumpToButton_Click;
            this.waypointCombo = waypointCombo;
            this.waypointCombo.DisplayMember = "";
            this.waypointCombo.DataSource = plugin.Map.Waypoints.ToArray();
            this.waypointCombo.SelectedIndexChanged += this.WaypointCombo_SelectedIndexChanged;
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
            else if ((e.Button == MouseButtons.Left) || (e.Button == MouseButtons.Right))
            {
                PickWaypoint(navigationWidget.MouseCell, e.Button == MouseButtons.Right);
            }
        }

        private void WaypointsTool_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                EnterPlacementMode();
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
            if (map.Metrics.GetCell(location, out int cell))
            {
                int selected = waypointCombo.SelectedIndex;
                Waypoint[] wp = map.Waypoints;
                var waypoint = wp[selected];
                if (waypoint.Cell != cell)
                {
                    if (undoWaypoint == null)
                    {
                        undoWaypoint = (waypoint, waypoint.Cell);
                    }
                    else if (undoWaypoint.Value.cell == cell)
                    {
                        undoWaypoint = null;
                    }
                    waypoint.Cell = cell;
                    redoWaypoint = (waypoint, waypoint.Cell);
                    CommitChange();
                    mapPanel.Invalidate();
                    waypointCombo.DataSource = null;
                    waypointCombo.Items.Clear();
                    waypointCombo.DataSource = wp.ToArray();
                    waypointCombo.SelectedIndex = selected;
                    plugin.Dirty = true;
                }
            }
        }

        private void RemoveWaypoint(Point location)
        {
            if (map.Metrics.GetCell(location, out int cell))
            {
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
                    if (undoWaypoint == null)
                    {
                        undoWaypoint = (waypoint, waypoint.Cell);
                    }
                    waypoint.Cell = null;
                    redoWaypoint = (waypoint, null);
                    CommitChange();
                    mapPanel.Invalidate();
                    waypointCombo.DataSource = null;
                    waypointCombo.Items.Clear();
                    waypointCombo.DataSource = wp.ToArray();
                    waypointCombo.SelectedIndex = waypointIndex;
                    
                    plugin.Dirty = true;
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
            UpdateStatus();
        }

        private void ExitPlacementMode()
        {
            if (!placementMode)
            {
                return;
            }
            placementMode = false;
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

        private void CommitChange()
        {
            var undoWaypoint2 = undoWaypoint;
            void undoAction(UndoRedoEventArgs e)
            {
                undoWaypoint2.Value.waypoint.Cell = undoWaypoint2.Value.cell;
                mapPanel.Invalidate();
            }
            var redoWaypoint2 = redoWaypoint;
            void redoAction(UndoRedoEventArgs e)
            {
                redoWaypoint2.Value.waypoint.Cell = redoWaypoint2.Value.cell;
                mapPanel.Invalidate();
            }
            undoWaypoint = null;
            redoWaypoint = null;
            url.Track(undoAction, redoAction);
        }

        private void WaypointCombo_SelectedIndexChanged(Object sender, EventArgs e)
        {
            jumpToButton.Enabled = waypointCombo.SelectedItem is Waypoint wp && wp.Cell.HasValue;
            mapPanel.Invalidate();
        }

        private void JumpToButton_Click(Object sender, EventArgs e)
        {
            if (waypointCombo.SelectedItem is Waypoint wp)
            {
                int cell = wp.Cell.GetValueOrDefault(-1);
                if (cell != -1)
                {
                    Point cellPoint;
                    if (RenderMap.Metrics.GetLocation(cell, out cellPoint))
                    {
                        int scaleFull = Math.Min(mapPanel.ClientRectangle.Width, mapPanel.ClientRectangle.Height);
                        bool isWidth = scaleFull == mapPanel.ClientRectangle.Width;
                        double mapSize = isWidth ? map.Metrics.Width : map.Metrics.Height;
                        // pixels per tile at zoom level 1.
                        double basicTileSize = scaleFull / mapSize;
                        // Convert cell position to actual position on image.
                        int cellX = (int)Math.Round(basicTileSize * mapPanel.Zoom * (cellPoint.X + 0.5));
                        int cellY = (int)Math.Round(basicTileSize * mapPanel.Zoom * (cellPoint.Y + 0.5));
                        // Get location to use to center the waypoint on the screen.
                        int x = cellX - mapPanel.ClientRectangle.Width / 2;
                        int y = cellY - mapPanel.ClientRectangle.Height / 2;
                        mapPanel.AutoScrollPosition = new Point(x,y);
                    }
                }
            }
        }

        protected override void PostRenderMap(Graphics graphics)
        {
            base.PostRenderMap(graphics);
            Waypoint selected = waypointCombo.SelectedItem as Waypoint;
            Waypoint[] selectedRange = selected != null ? new [] { selected } : new Waypoint[] { };
            RenderWayPoints(graphics, map, Globals.MapTileSize, Globals.MapTileScale, Layers, selectedRange);
            if (selected != null)
            {
                RenderWayPoints(graphics, map, Globals.MapTileSize, Globals.MapTileScale, Layers, Color.Black, Color.Yellow, Color.Yellow, true, false, selectedRange);
            }
        }

        private void UpdateStatus()
        {
            if (placementMode)
            {
                statusLbl.Text = "Left-Click to set cell waypoint, Right-Click to clear cell waypoint";
            }
            else
            {
                statusLbl.Text = "Shift to enter placement mode, Left-Click or Right-Click to pick cell waypoint";
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
            (this.mapPanel as Control).KeyDown += WaypointsTool_KeyDown;
            (this.mapPanel as Control).KeyUp += WaypointsTool_KeyUp;
            this.mapPanel.MouseDown += MapPanel_MouseDown;
            this.mapPanel.MouseMove += MapPanel_MouseMove;
            this.mapPanel.MouseLeave += MapPanel_MouseLeave;
            this.map.WaypointsUpdated += this.Map_WaypointsUpdated;
        }

        public override void Deactivate()
        {
            ExitPlacementMode();
            base.Deactivate();
            this.map.WaypointsUpdated -= this.Map_WaypointsUpdated;
            this.mapPanel.MouseDown -= MapPanel_MouseDown;
            this.mapPanel.MouseMove -= MapPanel_MouseMove;
            this.mapPanel.MouseLeave -= MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown -= WaypointsTool_KeyDown;
            (this.mapPanel as Control).KeyUp -= WaypointsTool_KeyUp;
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
