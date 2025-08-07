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
using System.Text;
using System.Text.RegularExpressions;
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

        public bool RoutesMode => routesMode;
        private bool routesMode = false;
        private readonly Button modeSwitchButton;
        private readonly ComboBox waypointsCombo;
        private readonly Button jumpToButton;
        private readonly ComboBox routesCombo;
        private readonly Label currentRoutePointLabel;
        private readonly Button editTeamButton;
        private readonly Button jumpToRouteButton;
        private const string JMP_FIRST = "Jump to start";
        private const string JMP_NEXT = "Jump to next";
        private const string CURRENT = "Selected: ";

        private Map previewMap;
        protected override Map RenderMap => previewMap;

        private int lastSelectedIndexWp;
        private int lastSelectedIndexRt;
        // Simply uses the index.
        public override object CurrentObject
        {
            get
            {
                string data = routesMode ? ("R:" + lastSelectedIndexRt + ":" + currentPathPos) : "W:" + lastSelectedIndexWp;
                return data;
            }
            set
            {
                bool parseOk = false;
                if (value is string data)
                {
                    //index && index >= 0 && waypointsCombo.Items.Count > index;
                    Match m = Regex.Match(data, "(R|W):(\\d+)(?::(-?\\d+))?");
                    if (m.Success)
                    {
                        parseOk = true;
                        routesMode = m.Groups[1].Value == "R";
                        int index = Int32.Parse(m.Groups[2].Value);
                        if (routesMode)
                        {
                            routesCombo.SelectedIndex = index;
                            lastSelectedIndexRt = index;
                            currentPathPos = m.Groups[3].Length == 0 ? -1 : Int32.Parse(m.Groups[3].Value);
                            SelectPathWaypoint(false, false);
                        }
                        else
                        {
                            lastSelectedIndexWp = index;
                            waypointsCombo.SelectedIndex = index;
                            currentPath = null;
                            currentPathPos = -1;
                        }
                    }
                }
                if(!parseOk)
                {
                    routesMode = false;
                    lastSelectedIndexWp = 0;
                    lastSelectedIndexRt = 0;
                    currentPath = null;
                    currentPathPos = -1;
                    waypointsCombo.SelectedIndex = 0;
                }
                OnModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool placementMode;

        protected override bool InPlacementMode
        {
            get { return placementMode; }
        }
        
        public String TeamTypeToolTip { get; private set; }
        private List<Waypoint> currentPath = null;
        private int currentPathPos = -1;
        public event EventHandler OnTeamTypeChanged;
        public event EventHandler OnModeChanged;
        public event EventHandler DoTeamTypeEdit;

        public WaypointsTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, ComboBox waypointsCombo, Button jumpToButton,
            ComboBox routesCombo, Button jumpToRouteButton, Label currentRoutePointLabel, Button editTeamButton, Button modeSwitchButton, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> url)
            : base(mapPanel, layers, statusLbl, plugin, url)
        {
            previewMap = map;
            this.jumpToButton = jumpToButton;
            this.jumpToButton.Click += JumpToButton_Click;
            this.modeSwitchButton = modeSwitchButton;
            this.modeSwitchButton.Click += ModeSwitchButton_Click;
            this.waypointsCombo = waypointsCombo;
            this.waypointsCombo.DataSource = plugin.Map.Waypoints.ToArray();
            this.waypointsCombo.SelectedIndexChanged += this.WaypointsCombo_SelectedIndexChanged;
            this.routesCombo = routesCombo;
            this.routesCombo.DataSource = TeamType.None.Yield().Concat(plugin.Map.TeamTypes.Select(t => t.Name)).ToArray();
            this.routesCombo.SelectedIndexChanged += this.RoutesCombo_SelectedIndexChanged;
            this.currentRoutePointLabel = currentRoutePointLabel;
            this.editTeamButton = editTeamButton;
            this.editTeamButton.Click += EditTeamButton_Click;
            this.jumpToRouteButton = jumpToRouteButton;
            this.jumpToRouteButton.Text = JMP_FIRST;
            this.jumpToRouteButton.Click += JumpToRouteButton_Click;
            UpdateStatus();
        }

        private void EditTeamButton_Click(object sender, EventArgs e)
        {
            if (routesMode)
            {
                DoTeamTypeEdit?.Invoke(routesCombo, EventArgs.Empty);
            }
        }

        public void TeamTypeEditDone()
        {
            // refresh comboboxes
            UndoRedoEventArgs ev = new UndoRedoEventArgs(mapPanel, map, plugin);
            ev.Source = ToolType.Waypoint;
            Url_UndoRedoDone(null, ev);
        }

        private void Url_UndoRedoDone(object sender, UndoRedoEventArgs ev)
        {
            // Only update this stuff if the undo/redo event was actually a waypoint change.
            if (!ev.Source.HasFlag(ToolType.Waypoint))
            {
                return;
            }
            int waypointIndex = waypointsCombo.SelectedIndex;
            this.waypointsCombo.SelectedIndexChanged -= this.WaypointsCombo_SelectedIndexChanged;
            this.waypointsCombo.DataSource = null;
            this.waypointsCombo.Items.Clear();
            Waypoint[] wp = map.Waypoints.ToArray();
            this.waypointsCombo.DataSource = wp;
            this.waypointsCombo.SelectedIndexChanged += this.WaypointsCombo_SelectedIndexChanged;
            
            void SelectWaypoint(Map map, int selectedIndex)
            {
                Waypoint selected = null;
                if (selectedIndex < 0 || selectedIndex >= map.Waypoints.Length)
                {
                    selectedIndex = 0;
                }
                lastSelectedIndexWp = selectedIndex;
                waypointsCombo.SelectedIndex = selectedIndex;
                selected = map.Waypoints[selectedIndex];
                this.jumpToButton.Enabled = selected != null && selected.Cell.HasValue;
            }
            void SelectRoute(string[] routeItems, string toSelect)
            {
                lastSelectedIndexRt = Enumerable.Range(0, routesCombo.Items.Count)
                    .FirstOrDefault(i => String.Equals(routeItems[i], toSelect, StringComparison.OrdinalIgnoreCase));
                routesCombo.SelectedIndex = lastSelectedIndexRt;
            }
            // routes
            string selectedTeam = routesCombo.SelectedItem as string;
            routesCombo.SelectedIndex = -1;
            routesCombo.DataSource = null;
            string[] items = TeamType.None.Yield().Concat(plugin.Map.TeamTypes.Select(t => t.Name)).ToArray();
            routesCombo.DataSource = items;
            if (routesMode)
            {
                SelectWaypoint(map, waypointIndex);
                SelectRoute(items, selectedTeam);
            }
            else
            {
                SelectRoute(items, selectedTeam);
                SelectWaypoint(map, waypointIndex);
            }
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
                int selected = waypointsCombo.SelectedIndex;
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
            int selected = waypointsCombo.SelectedIndex;
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
            this.waypointsCombo.SelectedIndexChanged -= this.WaypointsCombo_SelectedIndexChanged;
            waypointsCombo.DataSource = null;
            waypointsCombo.Items.Clear();
            waypointsCombo.DataSource = wp.ToArray();
            this.waypointsCombo.SelectedIndexChanged += this.WaypointsCombo_SelectedIndexChanged;
            waypointsCombo.SelectedIndex = selected;
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
            int origIndex = waypointsCombo.SelectedIndex;
            int waypointIndex;
            if (waypointsCombo.SelectedItem is Waypoint selwp && selwp.Cell == cell)
            {
                waypoint = selwp;
                waypointIndex = waypointsCombo.SelectedIndex;
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
                waypointsCombo.DataSource = null;
                waypointsCombo.Items.Clear();
                waypointsCombo.DataSource = wp.ToArray();
                waypointsCombo.SelectedIndex = origIndex;
            }
        }

        private void CheckSelectShortcuts(KeyEventArgs e)
        {
            if (!routesMode)
            {
                CheckShortcutsWaypoints(e);
            }
            else
            {
                CheckShortcutsRoutes(e);
            }
        }

        private void CheckShortcutsWaypoints(KeyEventArgs e)
        {
            int maxVal = waypointsCombo.Items.Count - 1;
            int curVal = waypointsCombo.SelectedIndex;
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
                    case Keys.M:
                        ModeSwitchButton_Click(null, EventArgs.Empty);
                        return;
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
                        JumpToSelectedWaypoint();
                        break;
                }
            }
            if (newVal.HasValue && curVal != newVal.Value)
            {
                Waypoint oldWp = map.Waypoints[curVal];
                Waypoint newWp = map.Waypoints[newVal.Value];
                lastSelectedIndexWp = newVal.Value;
                waypointsCombo.SelectedIndex = newVal.Value;
                if (placementMode)
                {
                    mapPanel.Invalidate(map, GetRefreshCells(oldWp, navigationWidget.MouseCell));
                    mapPanel.Invalidate(map, GetRefreshCells(newWp, navigationWidget.MouseCell));
                }
            }
        }

        private void CheckShortcutsRoutes(KeyEventArgs e)
        {
            int maxVal = routesCombo.Items.Count - 1;
            int curVal = routesCombo.SelectedIndex;
            int? newVal = null;
            if (Control.ModifierKeys == Keys.Shift && e.KeyCode == Keys.M)
            {
                ModeSwitchButton_Click(null, EventArgs.Empty);
                return;
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
                        SelectPathWaypoint(true, true);
                        break;
                }
            }
            if (newVal.HasValue && curVal != newVal.Value)
            {
                List<Waypoint> oldPath = currentPath;
                Waypoint oldWp = null;
                if (oldPath != null && oldPath.Count > 0)
                {
                    int oldPathPoint = currentPathPos;
                    if (oldPathPoint < 0 | oldPathPoint >= oldPath.Count)
                    {
                        oldPathPoint = -1;
                        SelectPathWaypoint(false, false);
                        oldPathPoint = 0;
                    }
                    oldWp = oldPath[oldPathPoint];
                }
                lastSelectedIndexRt = newVal.Value;
                routesCombo.SelectedIndex = newVal.Value;
                List<Waypoint> newPath = currentPath;
                Waypoint newWp = null;
                if (newPath != null && newPath.Count > 0)
                {
                    newWp = newPath[0];
                }

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
            // in route mode, only allow moving hte points of the current route.
            if (routesMode && (currentPath == null || currentPath.Count == 0 || currentPath.All(wp => wp.Flags.HasFlag(WaypointFlag.Temporary))))
            {
                return;
            }
            placementMode = true;
            int selected = waypointsCombo.SelectedIndex;
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
            int selected = waypointsCombo.SelectedIndex;
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
                int curSelIndex = waypointsCombo.SelectedIndex;
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
                    // The clicked cell was not a waypoint.
                    return;
                }
                int newSelectedIndex;
                if (foundIndices.Count == 1 || curIndex == -1)
                {
                    newSelectedIndex = foundIndices[0];
                }
                else
                {
                    int nextIndex = backwards ? curIndex - 1 : curIndex + 1;
                    if (nextIndex < 0)
                        nextIndex = foundIndices.Count - 1;
                    else if (nextIndex >= foundIndices.Count)
                        nextIndex = 0;
                    newSelectedIndex = foundIndices[nextIndex];
                }
                lastSelectedIndexWp = newSelectedIndex;
                if (!routesMode)
                {
                    waypointsCombo.SelectedIndex = newSelectedIndex;
                }
                Waypoint selected = map.Waypoints[newSelectedIndex];
                if (routesMode && currentPath != null && currentPath.Count > 0)
                {
                    int? index = Enumerable.Range(0, currentPath.Count).Cast<int?>().FirstOrDefault(i => currentPath[(int)i] == selected);
                    currentPathPos = index.GetValueOrDefault(-1);
                    SelectPathWaypoint(false, false);
                }
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
            Waypoint selected = waypointsCombo.SelectedItem as Waypoint;
            jumpToButton.Enabled = selected != null && selected.Cell.HasValue;
            url.Track(undoAction, redoAction, ToolType.Waypoint);
        }

        private void WaypointsCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            Waypoint selected = waypointsCombo.SelectedItem as Waypoint;
            jumpToButton.Enabled = selected != null && selected.Cell.HasValue;
            if (selected != null && selected.Cell.HasValue)
            {
                lastSelectedIndexWp = waypointsCombo.SelectedIndex;
                mapPanel.Invalidate(RenderMap, GetRefreshCells(selected, selected.Point.Value));
            }
            mapPanel.Invalidate(RenderMap, GetRefreshCells(selected, navigationWidget.MouseCell));
        }

        private void RoutesCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = routesCombo.SelectedItem as string;
            bool hasTeam = selected != null && !TeamType.None.Equals(selected, StringComparison.OrdinalIgnoreCase);
            if (hasTeam)
            {
                currentPath = plugin.Map.GetTeamTypeRoute(selected);
                currentPathPos = -1;
            }
            else
            {
                currentPath = null;
                currentPathPos = -1;
            }
            editTeamButton.Enabled = hasTeam;
            SelectPathWaypoint(false, false);
            TeamType teamtype = selected == null ? null : plugin.Map.TeamTypes.FirstOrDefault(tt => tt.Name.Equals(selected, StringComparison.OrdinalIgnoreCase));
            TeamTypeToolTip = teamtype?.GetSummaryLabel(true);
            OnTeamTypeChanged?.Invoke(this, new EventArgs());
            mapPanel.Invalidate();
        }

        private void JumpToButton_Click(object sender, EventArgs e)
        {
            JumpToSelectedWaypoint();
        }

        private void JumpToRouteButton_Click(object sender, EventArgs e)
        {
            SelectPathWaypoint(true, true);
        }

        private void ModeSwitchButton_Click(object sender, EventArgs e)
        {
            routesMode = !routesMode;
            Waypoint selected = waypointsCombo.SelectedIndex == -1 ? null : map.Waypoints[waypointsCombo.SelectedIndex];
            if (!routesMode)
            {
                currentPath = null;
                currentPathPos = -1;
            }
            else
            {
                RoutesCombo_SelectedIndexChanged(this, new EventArgs());
                if (currentPath != null && currentPath.Count > 0 && selected != null)
                {
                    int? index = Enumerable.Range(0, currentPath.Count).Cast<int?>().FirstOrDefault(i => currentPath[(int)i] == selected);
                    currentPathPos = index.GetValueOrDefault(-1);
                }
                else
                {
                    currentPathPos = -1;
                    ExitPlacementMode();
                }
                SelectPathWaypoint(false, false);
            }
            mapPanel.Invalidate(map, GetRefreshCells(selected, navigationWidget.MouseCell));
            OnModeChanged?.Invoke(this, new EventArgs());
        }

        protected void JumpToSelectedWaypoint()
        {
            if (!(waypointsCombo.SelectedItem is Waypoint wp))
            {
                return;
            }
            JumpToWaypoint(wp);
        }

        protected void SelectPathWaypoint(bool increment, bool jumpTo)
        {
            if (currentPath == null || currentPath.Count == 0)
            {
                currentPathPos = -1;
                AdjustRouteButton();
                return;
            }
            int currentPathPoint = currentPathPos;
            if (increment)
            {
                currentPathPoint++;
            }
            if (currentPathPoint < 0 | currentPathPoint >= currentPath.Count)
            {
                currentPathPoint = 0;
            }
            Waypoint wp = currentPath[currentPathPoint];
            if (!wp.Flags.HasFlag(WaypointFlag.Temporary))
            {
                waypointsCombo.SelectedItem = wp;
            }
            if (jumpTo)
            {
                JumpToWaypoint(wp);
            }
            if (increment)
            {
                currentPathPos = currentPathPoint >= currentPath.Count ? -1 : currentPathPoint;
            }
            AdjustRouteButton();
        }

        protected void AdjustRouteButton()
        {
            if (currentPath == null || currentPath.Count == 0)
            {
                jumpToRouteButton.Text = JMP_NEXT;
                jumpToRouteButton.Enabled = false;
                return;
            }
            jumpToRouteButton.Enabled = true;
            bool hasNext = currentPathPos > -1 && currentPathPos + 1 < currentPath.Count;
            bool noCurr = waypointsCombo.SelectedIndex == -1;
            Waypoint[] wps = map.Waypoints;
            Waypoint wpNext = hasNext ? currentPath[currentPathPos + 1] : currentPath[0];
            string waypoint = wpNext.Flags.HasFlag(WaypointFlag.Temporary) ? ("#" + wpNext.Cell) :
                (Enumerable.Range(0, wps.Length).Cast<int?>().FirstOrDefault(i => wps[(int)i] == wpNext)?.ToString() ?? "?");
            jumpToRouteButton.Text = (hasNext ? JMP_NEXT : JMP_FIRST) + " (" + waypoint + ")";
            currentRoutePointLabel.Text = CURRENT + (noCurr ? "-" : waypointsCombo.SelectedIndex.ToString());
        }

        protected void JumpToWaypoint(Waypoint wp)
        {
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
            int selectedIndex = waypointsCombo.SelectedIndex;
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
            int selectedIndex = waypointsCombo.SelectedIndex;
            Waypoint selected = waypointsCombo.SelectedItem as Waypoint;
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
            Waypoint[] suppressRange = selectedRange;
            Waypoint[] redRangeBounds = new Waypoint[0];
            Waypoint[] redRangeIndic = new Waypoint[0];
            Waypoint[] tracePath = currentPath == null ? null : currentPath.Where(wp => wp.Cell.HasValue).ToArray();
            if (tracePath != null && tracePath.Length > 0)
            {
                MapRenderer.RenderWaypointsPath(graphics, map, visibleCells, Globals.MapTileSize, tracePath);
                suppressRange = selectedRange.Concat(tracePath).ToArray();
                redRangeBounds = tracePath.Where(wp => !selectedRange.Contains(wp)).ToArray();
                redRangeIndic = tracePath.ToArray();
                selectedRange = selectedRange.Where(wp => !tracePath.Contains(wp)).ToArray();
            }
            MapRenderer.RenderAllBoundsFromCell(graphics, boundRenderCells, Globals.MapTileSize,
                map.Waypoints.Where(wp => !suppressRange.Contains(wp) && wp.Cell.HasValue).Select(wp => wp.Cell.Value), map.Metrics, Color.Orange);
            if (redRangeBounds.Length > 0)
            {
                MapRenderer.RenderAllBoundsFromCell(graphics, boundRenderCells, Globals.MapTileSize, redRangeBounds.Select(wp => wp.Cell.Value), map.Metrics, Color.Red);
            }
            // If the plugin has a dedicated "flare" waypoint, then it is hardcoded and the only one, and should always be rendered.
            bool renderAll = plugin.Map.FlareWaypointAvailable || Layers.HasFlag(MapLayerFlag.WaypointRadius);
            MapRenderer.RenderAllWayPointRevealRadiuses(graphics, plugin, map, boundRenderCells, Globals.MapTileSize, selected, !renderAll);
            MapRenderer.RenderWayPointIndicators(graphics, map, gameInfo, visibleCells, Globals.MapTileSize, Color.LightGreen, false, true, suppressRange);
            if (redRangeIndic.Length > 0)
            {
                MapRenderer.RenderWayPointIndicators(graphics, map, gameInfo, visibleCells, Globals.MapTileSize, Color.Red, false, false, redRangeIndic);
            }
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
            StringBuilder text = new StringBuilder();
            if (placementMode)
            {
                text.Append("Left-Click to set cell waypoint, Right-Click to clear cell waypoint, M to change to ")
                    .Append(routesMode ? "waypoints" : "routes").Append(" mode");
                if (!routesMode)
                {
                    text.Append(", ").Append(special).Append(" to select a special waypoint");
                }
                
            }
            else
            {
                text.Append("Shift to enter placement mode, Left-Click or Right-Click to pick cell waypoint, Shift + M to change to ")
                    .Append(routesMode ? "waypoints" : "routes").Append(" mode");
                if (!routesMode)
                {
                    text.Append(", Shift + ").Append(special).Append(" to select a special waypoint");
                }
                text.Append(", Enter to jump to ").Append(routesMode ? "next route" : "cell").Append(" waypoint location");
            }
            statusLbl.Text = text.ToString();
        }

        private void Map_WaypointsUpdated(object sender, EventArgs e)
        {
            this.waypointsCombo.SelectedIndexChanged -= this.WaypointsCombo_SelectedIndexChanged;
            int selected = waypointsCombo.SelectedIndex;
            mapPanel.Invalidate();
            waypointsCombo.DataSource = null;
            waypointsCombo.Items.Clear();
            waypointsCombo.DataSource = map.Waypoints.ToArray();
            waypointsCombo.SelectedIndex = selected;
            this.waypointsCombo.SelectedIndexChanged += this.WaypointsCombo_SelectedIndexChanged;
            WaypointsCombo_SelectedIndexChanged(null, null);
        }

        public override void Activate()
        {
            base.Activate();
            Deactivate(true);
            (mapPanel as Control).KeyDown += WaypointsTool_KeyDown;
            (mapPanel as Control).KeyUp += WaypointsTool_KeyUp;
            mapPanel.MouseDown += MapPanel_MouseDown;
            mapPanel.MouseMove += MapPanel_MouseMove;
            mapPanel.MouseLeave += MapPanel_MouseLeave;
            mapPanel.MouseWheel += MapPanel_MouseWheel;
            mapPanel.SuspendMouseZoomKeys = Keys.Control;
            map.WaypointsUpdated += this.Map_WaypointsUpdated;
            navigationWidget.MouseCellChanged += MouseoverWidget_MouseCellChanged;
            url.Undone += Url_UndoRedoDone;
            url.Redone += Url_UndoRedoDone;
            UpdateStatus();
            OnModeChanged?.Invoke(this, EventArgs.Empty);
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
            url.Undone -= Url_UndoRedoDone;
            url.Redone -= Url_UndoRedoDone;
            map.WaypointsUpdated -= this.Map_WaypointsUpdated;
            mapPanel.MouseDown -= MapPanel_MouseDown;
            mapPanel.MouseMove -= MapPanel_MouseMove;
            mapPanel.MouseLeave -= MapPanel_MouseLeave;
            mapPanel.MouseWheel -= MapPanel_MouseWheel;
            mapPanel.SuspendMouseZoomKeys = Keys.None;
            (mapPanel as Control).KeyDown -= WaypointsTool_KeyDown;
            (mapPanel as Control).KeyUp -= WaypointsTool_KeyUp;
            navigationWidget.MouseCellChanged -= MouseoverWidget_MouseCellChanged;
            OnModeChanged?.Invoke(this, EventArgs.Empty);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    jumpToButton.Click -= JumpToButton_Click;
                    waypointsCombo.SelectedIndexChanged -= this.WaypointsCombo_SelectedIndexChanged;
                    modeSwitchButton.Click += ModeSwitchButton_Click;
                    routesCombo.SelectedIndexChanged += this.RoutesCombo_SelectedIndexChanged;
                    editTeamButton.Click -= EditTeamButton_Click;
                    jumpToRouteButton.Click -= JumpToRouteButton_Click;
                    OnTeamTypeChanged = null;
                    OnModeChanged = null;
                    Deactivate();
                }
                disposedValue = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
