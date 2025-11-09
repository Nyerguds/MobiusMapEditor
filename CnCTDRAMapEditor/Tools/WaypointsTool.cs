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
        private String lastSelectedTeam;

        public override object CurrentObject
        {
            get
            {
                string data = routesMode ? ("R=" + (lastSelectedTeam ?? TeamType.None) + "=" + currentPathPos) : "W=" + lastSelectedIndexWp;
                return data;
            }
            set
            {
                bool parseOk = false;
                if (value is string data)
                {
                    //index && index >= 0 && waypointsCombo.Items.Count > index;
                    Match m = Regex.Match(data, "(R|W)=([^=]+)(?==(-?\\d+))?");
                    if (m.Success)
                    {
                        routesMode = m.Groups[1].Value == "R";
                        int index;
                        if (routesMode)
                        {
                            string team = m.Groups[2].Value;
                            index = ListItem.GetIndexInComboBox(team, routesCombo);
                            if (index != -1)
                            {
                                parseOk = true;
                                lastSelectedTeam = team;
                                routesCombo.SelectedIndex = index;
                                currentPathPos = m.Groups[3].Length == 0 ? -1 : Int32.Parse(m.Groups[3].Value);
                                SelectPathWaypoint(false, false);
                            }
                        }
                        else
                        {
                            if (Int32.TryParse(m.Groups[2].Value, out index))
                            {
                                parseOk = true;
                                lastSelectedIndexWp = index;
                                waypointsCombo.SelectedIndex = index;
                                lastSelectedTeam = TeamType.None;
                                currentPathWaypoints = null;
                                currentPathCells = null;
                                currentPathPos = -1;
                            }
                        }
                    }
                }
                if (!parseOk)
                {
                    routesMode = false;
                    lastSelectedIndexWp = 0;
                    lastSelectedTeam = TeamType.None;
                    currentPathWaypoints = null;
                    currentPathCells = null;
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
        private List<int?> currentPathCells = null;
        private List<int?> currentPathWaypoints = null;
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
            this.waypointsCombo.ValueMember = "Value";
            this.waypointsCombo.DisplayMember = "Label";
            this.waypointsCombo.DataSource = plugin.Map.Waypoints.Select(wp => ListItem.Create(wp)).ToArray();
            this.waypointsCombo.SelectedIndexChanged += this.WaypointsCombo_SelectedIndexChanged;
            this.routesCombo = routesCombo;
            this.routesCombo.ValueMember = "Value";
            this.routesCombo.DisplayMember = "Label";
            this.routesCombo.DataSource = TeamType.None.Yield().Concat(plugin.Map.TeamTypes.Select(t => t.Name)).Select(t => ListItem.Create(t)).ToArray();
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
            waypointsCombo.SelectedIndexChanged -= WaypointsCombo_SelectedIndexChanged;
            waypointsCombo.DataSource = map.Waypoints.Select(wp => ListItem.Create(wp)).ToArray();
            waypointsCombo.SelectedIndexChanged += WaypointsCombo_SelectedIndexChanged;
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
                jumpToButton.Enabled = selected != null && selected.Cell.HasValue;
            }
            void SelectRoute(string[] routeItems, string toSelect)
            {
                int selectIndex = Enumerable.Range(0, routesCombo.Items.Count)
                    .FirstOrDefault(i => String.Equals(routeItems[i], toSelect, StringComparison.OrdinalIgnoreCase));
                if (selectIndex != -1)
                {
                    lastSelectedTeam = toSelect;
                    routesCombo.SelectedIndex = selectIndex;
                }
            }
            // routes
            string selectedTeam = ListItem.GetValueFromComboBox<string>(routesCombo);
            string[] items = TeamType.None.Yield().Concat(plugin.Map.TeamTypes.Select(t => t.Name)).ToArray();
            routesCombo.DataSource = items.Select(t => ListItem.Create(t)).ToArray();
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
            Waypoint[] waypoints = map.Waypoints;
            Waypoint waypoint = waypoints[selected];
            if (waypoint.Cell.HasValue && waypoint.Cell.Value == cell)
            {
                return;
            }
            int? oldCell = waypoint.Cell;
            Point? oldPoint = waypoint.Point;
            waypoint.Cell = cell;
            CommitChange(waypoint, oldCell, cell);
            waypointsCombo.SelectedIndexChanged -= WaypointsCombo_SelectedIndexChanged;
            waypointsCombo.DataSource = waypoints.Select(wp => ListItem.Create(wp)).ToArray();
            waypointsCombo.SelectedIndex = selected;
            waypointsCombo.SelectedIndexChanged += WaypointsCombo_SelectedIndexChanged;
            WaypointsCombo_SelectedIndexChanged(waypointsCombo, null);
            if (routesMode)
            {
                // refresh path contents without resetting current path position.
                // This is necessary to adjust the cells of loop orders, since they don't link to the actual waypoint object.
                string teamname = ListItem.GetValueFromComboBox<string>(routesCombo);
                TeamType team = TeamType.GetTeamType(teamname, plugin.Map);
                currentPathCells = map.GetTeamTypeRoute(teamname, out currentPathWaypoints);
            }
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
            if (routesMode && !Globals.AllowDeleteRoutePoints)
            {
                return;
            }
            Waypoint waypoint;
            Waypoint[] waypoints = map.Waypoints;
            int origIndex = waypointsCombo.SelectedIndex;
            int waypointIndex;
            Waypoint selwp = ListItem.GetItemFromComboBox<Waypoint>(waypointsCombo)?.Value;
            if (selwp != null && selwp.Cell.HasValue && selwp.Cell.Value == cell)
            {
                waypoint = selwp;
                waypointIndex = waypointsCombo.SelectedIndex;
            }
            else
            {
                waypointIndex = Enumerable.Range(0, waypoints.Length).Where(i => waypoints[i].Cell.HasValue && waypoints[i].Cell.Value == cell).DefaultIfEmpty(-1).Last();
                waypoint = waypointIndex == -1 ? null : waypoints[waypointIndex];
            }
            if (waypoint != null)
            {
                int? oldCell = waypoint.Cell;
                waypoint.Cell = null;
                CommitChange(waypoint, oldCell, null);
                mapPanel.Invalidate(map, GetRefreshCells(waypoint, location));
                waypointsCombo.SelectedIndexChanged -= WaypointsCombo_SelectedIndexChanged;
                waypointsCombo.DataSource = waypoints.Select(wp => ListItem.Create(wp)).ToArray();
                waypointsCombo.SelectedIndex = origIndex;
                waypointsCombo.SelectedIndexChanged += WaypointsCombo_SelectedIndexChanged;
                WaypointsCombo_SelectedIndexChanged(waypointsCombo, null);
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
            bool isShift = Control.ModifierKeys == Keys.Shift;
            bool wasPlacementMode = placementMode;
            if (newVal.HasValue && curVal != newVal.Value)
            {
                int? oldWpi = null;
                if (isShift || wasPlacementMode)
                {
                    int pathPos = currentPathPos;
                    if (pathPos == -1)
                    {
                        pathPos = GetNextPathPoint(pathPos);
                    }
                    oldWpi = pathPos == -1 ? null : currentPathWaypoints[pathPos];
                }
                lastSelectedTeam = ListItem.GetItemFromComboBoxAtIndex<String>(routesCombo, newVal.Value)?.Value ?? TeamType.None;
                routesCombo.SelectedIndex = newVal.Value;
                // Adjust placement mode to newly selected teamtype.
                if (currentPathWaypoints == null || currentPathWaypoints.All(wp => !wp.HasValue || wp.Value == -1))
                {
                    if (wasPlacementMode)
                    {
                        ExitPlacementMode();
                    }
                }
                else if (isShift && !wasPlacementMode)
                {
                    EnterPlacementMode();
                }
                if (placementMode)
                {
                    int pathPos = currentPathPos;
                    if (pathPos == -1)
                    {
                        pathPos = GetNextPathPoint(pathPos);
                    }
                    int? newWpi = pathPos == -1 ? null : currentPathWaypoints[pathPos];
                    Waypoint[] mwp = map.Waypoints;
                    Waypoint oldWp = !oldWpi.HasValue || oldWpi.Value < 0 || oldWpi.Value >= mwp.Length ? null : mwp[oldWpi.Value];
                    mapPanel.Invalidate(map, GetRefreshCells(oldWp, navigationWidget.MouseCell));
                    Waypoint newWp = !newWpi.HasValue || newWpi.Value < 0 || newWpi.Value >= mwp.Length ? null : mwp[newWpi.Value];
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
            // in route mode, only allow moving the points of the current route.
            if (routesMode && (currentPathWaypoints == null || currentPathWaypoints.All(wp => !wp.HasValue || wp.Value == -1)))
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
                int si = waypointsCombo.SelectedIndex;
                int curIndex = -1;
                for (var i = 0; i < wp.Length; ++i)
                {
                    if (wp[i].Cell == cell)
                    {
                        foundIndices.Add(i);
                        if (i == si)
                            curIndex = foundIndices.Count - 1;
                    }
                }
                if (foundIndices.Count == 0)
                {
                    // The clicked cell was not a waypoint.
                    return;
                }
                int nsi;
                if (foundIndices.Count == 1 || curIndex == -1)
                {
                    nsi = foundIndices[0];
                }
                else
                {
                    int nextIndex = backwards ? curIndex - 1 : curIndex + 1;
                    if (nextIndex < 0)
                        nextIndex = foundIndices.Count - 1;
                    else if (nextIndex >= foundIndices.Count)
                        nextIndex = 0;
                    nsi = foundIndices[nextIndex];
                }
                // in routesMode, SelectPathWaypoint already does this, so no need to do it twice.
                if (!routesMode)
                {
                    waypointsCombo.SelectedIndex = nsi;
                    lastSelectedIndexWp = nsi;
                }
                Waypoint selected = map.Waypoints[nsi];
                if (routesMode && currentPathCells != null && currentPathCells.Count > 0)
                {
                    int? index = si == -1 ? null : Enumerable.Range(0, currentPathWaypoints.Count).Cast<int?>().FirstOrDefault(i => currentPathWaypoints[(int)i] == nsi);
                    if (index.HasValue)
                    {
                        currentPathPos = index.Value;
                        SelectPathWaypoint(false, false);
                    }
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
            Waypoint selected = ListItem.GetValueFromComboBox<Waypoint>(waypointsCombo);
            jumpToButton.Enabled = selected != null && selected.Cell.HasValue;
            url.Track(undoAction, redoAction, ToolType.Waypoint);
        }

        private void WaypointsCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            Waypoint selected = ListItem.GetValueFromComboBox<Waypoint>(waypointsCombo);
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
            string selected = ListItem.GetValueFromComboBox<string>(routesCombo);
            currentPathCells = map.GetTeamTypeRoute(selected, out currentPathWaypoints);
            currentPathPos = -1;
            editTeamButton.Enabled = currentPathCells != null;
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
            int si = waypointsCombo.SelectedIndex;
            Waypoint sel = si == -1 ? null : map.Waypoints[si];
            if (!routesMode)
            {
                currentPathWaypoints = null;
                currentPathCells = null;
                currentPathPos = -1;
            }
            else
            {
                RoutesCombo_SelectedIndexChanged(this, new EventArgs());
                if (currentPathCells != null && currentPathCells.Count > 0 && sel != null)
                {
                    int? index = si == -1 ? null : Enumerable.Range(0, currentPathWaypoints.Count).Cast<int?>().FirstOrDefault(i => currentPathWaypoints[(int)i] == si);
                    currentPathPos = index.GetValueOrDefault(-1);
                }
                else
                {
                    currentPathPos = -1;
                    ExitPlacementMode();
                }
                SelectPathWaypoint(false, false);
            }
            mapPanel.Invalidate(map, GetRefreshCells(sel, navigationWidget.MouseCell));
            OnModeChanged?.Invoke(this, new EventArgs());
        }

        protected void JumpToSelectedWaypoint()
        {
            Waypoint wp = ListItem.GetValueFromComboBox<Waypoint>(waypointsCombo);
            if (wp == null)
            {
                return;
            }
            JumpToWaypoint(wp);
        }

        protected void SelectPathWaypoint(bool increment, bool jumpTo)
        {
            // no points found. Abort.
            if (currentPathCells == null || currentPathCells.Count == 0)
            {
                currentPathPos = -1;
                AdjustRouteButton();
                return;
            }
            int cpp = increment ? GetNextPathPoint(currentPathPos) : currentPathPos;

            if (cpp == -1)
            {
                if (!increment)
                {
                    // This is an inital pre-jump state. Find if a valid point exists.
                    cpp = GetNextPathPoint(cpp);
                }
                if (cpp == -1)
                {
                    // GetNextPathPoint found nothing valid. Abort.
                    currentPathPos = -1;
                    AdjustRouteButton();
                    return;
                }
            }
            // cpp should be at a valid value now.
            int? waypoint = currentPathWaypoints[cpp];
            int? cell = currentPathCells[cpp];
            Waypoint wp = null;
            if (waypoint.HasValue)
            {
                // could be -1 for a "move to cell"; in that case nothing happens.
                int wpval = waypoint.Value;
                if (wpval >= 0 && wpval < map.Waypoints.Length)
                {
                    wp = map.Waypoints[wpval];
                }                    
            }
            // if a valid waypoint was found, select it.
            if (wp != null)
            {
                waypointsCombo.SelectedIndex = waypoint.Value;
            }
            int selectedWp = waypointsCombo.SelectedIndex;
            // if no waypoint was found, and the current selected waypoint is nowhere in the route,
            // select the first valid route waypoint instead.
            if (wp == null && (selectedWp == -1 || !currentPathWaypoints.Any(wpi => wpi == selectedWp)))
            {
                int validWp = GetNextPathPoint(-1);
                int firstValid = validWp;
                while (validWp != -1 && !currentPathWaypoints[validWp].HasValue || currentPathWaypoints[validWp].Value == -1)
                {
                    validWp = GetNextPathPoint(validWp);
                    // avoid looping back to the front.
                    if (firstValid == validWp)
                    {
                        break;
                    }
                }
                if (validWp != -1 && currentPathWaypoints[validWp].HasValue && currentPathWaypoints[validWp].Value != -1)
                {
                    waypointsCombo.SelectedIndex = currentPathWaypoints[validWp].Value;
                }
            }
            if (jumpTo)
            {
                if (wp != null)
                {
                    JumpToWaypoint(wp);
                }
                else if (cell.HasValue)
                {
                    JumpToCell(cell.Value);
                }
            }
            if (increment)
            {
                currentPathPos = cpp;
            }
            AdjustRouteButton();
        }

        private int GetNextPathPoint(int currentPathPos)
        {
            // remove illegal values
            int pathlen = currentPathWaypoints?.Count ?? 0;
            int cpp = Math.Max(0, currentPathPos + 1);
            // skip non-waypoint orders.
            while (cpp < pathlen && currentPathWaypoints[cpp] == null)
            {
                cpp++;
            }
            // exceed: loop to start, find first valid again
            if (cpp >= pathlen)
            {
                cpp = 0;
                // skip non-waypoint orders.
                while (cpp < pathlen && currentPathWaypoints[cpp] == null)
                {
                    cpp++;
                }
            }
            // the only case in which this returns -1 is if there are no valid points to continue to.
            if (cpp >= pathlen)
            {
                cpp = -1;
            }
            return cpp;
        }

        protected void AdjustRouteButton()
        {
            if (currentPathCells == null || currentPathCells.Count == 0 || currentPathCells.All(c => !c.HasValue))
            {
                jumpToRouteButton.Text = JMP_FIRST;
                jumpToRouteButton.Enabled = false;
                return;
            }
            
            int routeNext = GetNextPathPoint(currentPathPos);
            jumpToRouteButton.Enabled = routeNext != -1;
            // true if the next point is not a loop back to the start
            bool hasNext = currentPathPos != -1 && routeNext > currentPathPos;
            Waypoint[] wps = map.Waypoints;
            // contains a valid waypoint, or -1 for a cell number.
            int? wpNext = routeNext == -1 ? null : currentPathWaypoints[routeNext];
            string waypoint;
            if (wpNext == -1)
            {
                int? cell = currentPathCells[routeNext];
                waypoint = cell.HasValue ? ("#" + cell.Value) : "?";
            }
            else if (wpNext == null)
            {
                waypoint = "?";
            }
            else
            {
                waypoint = wpNext.ToString();
            }
            jumpToRouteButton.Text = (hasNext ? JMP_NEXT : JMP_FIRST) + " (" + waypoint+")";
            bool noCurr = waypointsCombo.SelectedIndex == -1;
            currentRoutePointLabel.Text = CURRENT + (noCurr ? "-" : waypointsCombo.SelectedIndex.ToString());
        }

        protected void JumpToWaypoint(Waypoint wp)
        {
            JumpToCell(wp?.Cell);
        }

        protected void JumpToCell(int? cell)
        {
            if (!cell.HasValue)
            {
                return;
            }
            if (!RenderMap.Metrics.GetLocation(cell.Value, out Point cellPoint))
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
            Waypoint selected = ListItem.GetValueFromComboBox<Waypoint>(waypointsCombo);
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
            if (plugin.Map.BasicSection.SoloMission && (Layers.HasFlag(MapLayerFlag.HomeAreaBox) || (selected != null && selected.Flags.HasFlag(WaypointFlag.Home))))
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
            if (routesMode && currentPathCells != null && currentPathWaypoints != null && currentPathCells.Any(wp => wp.HasValue))
            {
                Waypoint[] wps = plugin.Map.Waypoints;
                MapRenderer.RenderWaypointsPath(graphics, map, visibleCells, Globals.MapTileSize, currentPathCells, currentPathWaypoints);
                List<Waypoint> tracePath = new List<Waypoint>();
                for (int i = 0; i < currentPathWaypoints.Count; ++i)
                {
                    int? cpw = currentPathWaypoints[i];
                    int? cpc = currentPathCells[i];
                    if (!cpw.HasValue || cpw.Value > wps.Length)
                    {
                        continue;
                    }
                    if (cpw.Value >= 0)
                    {
                        tracePath.Add(wps[cpw.Value]);
                    }
                    else if (cpw.Value == -1 && cpc.HasValue)
                    {
                        tracePath.Add(new Waypoint("##", map.Metrics, cpc));
                    }  
                }
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
            mapPanel.Invalidate();
            int selected = waypointsCombo.SelectedIndex;
            waypointsCombo.SelectedIndexChanged -= WaypointsCombo_SelectedIndexChanged;
            waypointsCombo.DataSource = map.Waypoints.ToArray();
            waypointsCombo.SelectedIndex = selected;
            waypointsCombo.SelectedIndexChanged += WaypointsCombo_SelectedIndexChanged;
            WaypointsCombo_SelectedIndexChanged(waypointsCombo, null);
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
                    modeSwitchButton.Click -= ModeSwitchButton_Click;
                    routesCombo.SelectedIndexChanged -= this.RoutesCombo_SelectedIndexChanged;
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
