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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Tools
{
    public class BuildingTool : ViewTool
    {
        private readonly TypeListBox buildingTypeListBox;
        private readonly MapPanel buildingTypeMapPanel;
        private readonly ObjectProperties objectProperties;

        private Map previewMap;
        protected override Map RenderMap => previewMap;

        public override bool IsBusy { get { return startedDragging; } }

        public override Object CurrentObject
        {
            get { return mockBuilding; }
            set
            {
                if (value is Building bld)
                {
                    BuildingType bt = buildingTypeListBox.Types.Where(b => b is BuildingType blt && blt.ID == bld.Type.ID
                        && String.Equals(blt.Name, bld.Type.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() as BuildingType;
                    if (bt != null)
                    {
                        SelectedBuildingType= bt;
                    }
                    bld.Type = SelectedBuildingType;
                    mockBuilding.CloneDataFrom(bld);
                    RefreshPreviewPanel();
                }
            }
        }

        /// <summary>
        /// Layers that are not painted by the PostRenderMap function on ViewTool level because they are handled
        /// at a specific point in the PostRenderMap override by the implementing tool.
        /// </summary>
        protected override MapLayerFlag ManuallyHandledLayers => MapLayerFlag.TechnoTriggers | MapLayerFlag.BuildingRebuild | MapLayerFlag.BuildingFakes | MapLayerFlag.EffectRadius | MapLayerFlag.OverlapOutlines;

        private bool placementMode;

        protected override Boolean InPlacementMode
        {
            get { return placementMode || startedDragging; }
        }

        private readonly Building mockBuilding;

        private Building selectedBuilding;
        private Point? selectedBuildingStartLocation;
        private Dictionary<Point, Smudge> selectedBuildingEatenSmudge;
        private Dictionary<Point, Overlay> selectedBuildingEatenOverlay;
        private Point selectedBuildingPivot;
        private bool startedDragging;

        private ObjectPropertiesPopup selectedObjectProperties;

        private BuildingType selectedBuildingType;
        private BuildingType SelectedBuildingType
        {
            get => selectedBuildingType;
            set
            {
                if (selectedBuildingType != value)
                {
                    if (placementMode && (selectedBuildingType != null))
                    {
                        Rectangle toRefresh = new Rectangle(navigationWidget.MouseCell, selectedBuildingType.OverlapBounds.Size);
                        if (selectedBuildingType.IsWall) toRefresh.Inflate(1, 1);
                        mapPanel.Invalidate(map, toRefresh);
                    }
                    selectedBuildingType = value;
                    buildingTypeListBox.SelectedValue = selectedBuildingType;
                    if (placementMode && (selectedBuildingType != null))
                    {
                        Rectangle toRefresh = new Rectangle(navigationWidget.MouseCell, selectedBuildingType.OverlapBounds.Size);
                        if (selectedBuildingType.IsWall) toRefresh.Inflate(1, 1);
                        mapPanel.Invalidate(map, toRefresh);
                    }
                    mockBuilding.Type = selectedBuildingType;
                    RefreshPreviewPanel();
                }
            }
        }

        public BuildingTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, TypeListBox buildingTypeListBox, MapPanel buildingTypeMapPanel,
            ObjectProperties objectProperties, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> url)
            : base(mapPanel, layers, statusLbl, plugin, url)
        {
            previewMap = map;
            mockBuilding = new Building()
            {
                Type = buildingTypeListBox.Types.First() as BuildingType,
                House = map.Houses.First().Type,
                Strength = 256,
                Direction = map.BuildingDirectionTypes.Where(d => d.Equals(FacingType.North)).First()
            };
            this.buildingTypeListBox = buildingTypeListBox;
            this.buildingTypeListBox.SelectedIndexChanged += BuildingTypeListBox_SelectedIndexChanged;
            this.buildingTypeMapPanel = buildingTypeMapPanel;
            this.buildingTypeMapPanel.BackColor = Color.White;
            this.buildingTypeMapPanel.MaxZoom = 1;
            this.buildingTypeMapPanel.SmoothScale = Globals.PreviewSmoothScale;
            this.objectProperties = objectProperties;
            this.objectProperties.Object = mockBuilding;
            SelectedBuildingType = mockBuilding.Type;
        }

        private void MapPanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys != Keys.None || e.Button != MouseButtons.Left)
            {
                return;
            }
            if (map.Metrics.GetCell(navigationWidget.MouseCell, out int cell))
            {
                if (map.Buildings[cell] is Building building)
                {
                    selectedBuilding = null;
                    selectedBuildingStartLocation = null;
                    selectedBuildingPivot = Point.Empty;
                    startedDragging = false;
                    mapPanel.Invalidate();
                    Building preEdit = building.Clone();
                    selectedObjectProperties?.Close();
                    selectedObjectProperties = new ObjectPropertiesPopup(objectProperties.Plugin, building);
                    selectedObjectProperties.Closed += (cs, ce) =>
                    {
                        selectedObjectProperties = null;
                        navigationWidget.Refresh();
                        AddPropertiesUndoRedo(building, preEdit);
                    };
                    building.PropertyChanged += SelectedBuilding_PropertyChanged;
                    selectedObjectProperties.Show(mapPanel, mapPanel.PointToClient(Control.MousePosition));
                    UpdateStatus();
                }
            }
        }

        private void AddPropertiesUndoRedo(Building building, Building preEdit)
        {
            // building = building in its final edited form. Clone for preservation
            Building redoBuilding = building.Clone();
            Building undoBuilding = preEdit;
            AdjustBuildPriorities(building, preEdit, out Building[] baseBuildings, out int[] buildingPrioritiesOld, out int[] buildingPrioritiesNew);
            if (redoBuilding.DataEquals(undoBuilding))
            {
                return;
            }
            bool origDirtyState = plugin.Dirty;
            plugin.Dirty = true;
            void undoAction(UndoRedoEventArgs ev)
            {
                building.CloneDataFrom(undoBuilding);
                if (building.Trigger == null || (!Trigger.IsEmpty(building.Trigger)
                    && !ev.Map.FilterStructureTriggers().Any(tr => tr.Name.Equals(building.Trigger, StringComparison.InvariantCultureIgnoreCase))))
                {
                    building.Trigger = Trigger.None;
                }
                InvalidateBuildingArea(ev.MapPanel, ev.Map, building);
                if (baseBuildings != null && buildingPrioritiesOld != null && baseBuildings.Length == buildingPrioritiesOld.Length)
                {
                    for (Int32 i = 0; i < baseBuildings.Length; ++i)
                    {
                        Building bld = baseBuildings[i];
                        bld.BasePriority = buildingPrioritiesOld[i];
                        InvalidateBuildingArea(ev.MapPanel, ev.Map, bld);
                    }
                }
                if (ev.Plugin != null)
                {
                    ev.Plugin.Dirty = origDirtyState;
                }
            }
            void redoAction(UndoRedoEventArgs ev)
            {
                building.CloneDataFrom(redoBuilding);
                if (building.Trigger == null || (!Trigger.IsEmpty(building.Trigger)
                    && !ev.Map.FilterStructureTriggers().Any(tr => tr.Name.Equals(building.Trigger, StringComparison.InvariantCultureIgnoreCase))))
                {
                    building.Trigger = Trigger.None;
                }
                InvalidateBuildingArea(ev.MapPanel, ev.Map, building);
                if (baseBuildings != null && buildingPrioritiesNew != null && baseBuildings.Length == buildingPrioritiesNew.Length)
                {
                    for (Int32 i = 0; i < baseBuildings.Length; ++i)
                    {
                        Building bld = baseBuildings[i];
                        bld.BasePriority = buildingPrioritiesNew[i];
                        InvalidateBuildingArea(ev.MapPanel, ev.Map, bld);
                    }
                }
                if (ev.Plugin != null)
                {
                    ev.Plugin.Dirty = true;
                }
            }
            url.Track(undoAction, redoAction, ToolType.Building);
        }

        private void MockBuilding_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Type" && (mockBuilding.Type == null || !mockBuilding.Type.HasTurret))
            {
                mockBuilding.Direction = map.BuildingDirectionTypes.Where(d => d.Equals(FacingType.North)).First();
            }
            if (e.PropertyName == "House")
            {
                // Fix for House "None" set on unbuilt buildings.
                HouseType newHouse = mockBuilding.House;
                if (newHouse == null || newHouse.Flags.HasFlag(HouseTypeFlag.BaseHouse | HouseTypeFlag.Special))
                {
                    string opposing = plugin.GameInfo.GetClassicOpposingPlayer(plugin.Map.BasicSection.Player);
                    newHouse = plugin.Map.Houses.Where(h => h.Type.Equals(opposing)).FirstOrDefault()?.Type ?? plugin.Map.Houses.First().Type;
                }
                plugin.ActiveHouse = newHouse;
            }
            RefreshPreviewPanel();
        }

        private void SelectedBuilding_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            InvalidateBuildingArea(mapPanel, map, sender as Building);
        }

        private void BuildingTypeListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedBuildingType = buildingTypeListBox.SelectedValue as BuildingType;
        }

        private void BuildingTool_KeyDown(object sender, KeyEventArgs e)
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

        private void BuildingTool_KeyUp(object sender, KeyEventArgs e)
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

        private void MapPanel_MouseWheel(Object sender, MouseEventArgs e)
        {
            if (e.Delta == 0 || (Control.ModifierKeys & Keys.Control) == Keys.None)
            {
                return;
            }
            KeyEventArgs keyArgs= new KeyEventArgs(e.Delta > 0 ? Keys.PageUp : Keys.PageDown);
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

        private void MapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (placementMode)
            {
                if (e.Button == MouseButtons.Left)
                {
                    AddBuilding(navigationWidget.MouseCell);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    RemoveBuilding(navigationWidget.MouseCell);
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                SelectBuilding(navigationWidget.MouseCell);
            }
            else if (e.Button == MouseButtons.Right)
            {
                PickBuilding(navigationWidget.MouseCell);
            }
        }

        private void MapPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (selectedBuilding != null && selectedBuildingStartLocation.HasValue)
            {
                AddMoveUndoTracking(selectedBuilding, selectedBuildingStartLocation.Value);
                selectedBuildingEatenSmudge = null;
                selectedBuildingEatenOverlay = null;
                selectedBuilding = null;
                selectedBuildingStartLocation = null;
                selectedBuildingPivot = Point.Empty;
                startedDragging = false;
                mapPanel.Invalidate();
                UpdateStatus();
            }
        }

        private static void InvalidateBuildingArea(MapPanel mapPanel, Map map, Building building)
        {
            if (building.Type.IsWall)
            {
                Rectangle? current = map.Overlappers[building];
                if (current.HasValue)
                {
                    Rectangle refreshArea = Rectangle.Inflate(current.Value, 1, 1);
                    //map.UpdateWallOverlays(refreshArea.Points().ToHashSet());
                    mapPanel.Invalidate(map, refreshArea);
                }
            }
            else
            {
                mapPanel.Invalidate(map, building);
            }
        }

        private void AddMoveUndoTracking(Building toMove, Point startLocation)
        {
            Point? finalLocation = map.Buildings[toMove];
            Dictionary<Point, Smudge> eatenSm = selectedBuildingEatenSmudge?.ToDictionary(p => p.Key, p => p.Value);
            Dictionary<Point, Overlay> eatenOv = selectedBuildingEatenOverlay?.ToDictionary(p => p.Key, p => p.Value);
            if (!finalLocation.HasValue || finalLocation.Value == selectedBuildingStartLocation)
            {
                return;
            }
            bool origDirtyState = plugin.Dirty;
            plugin.Dirty = true;
            Point endLocation = finalLocation.Value;
            void undoAction(UndoRedoEventArgs ev)
            {
                InvalidateBuildingArea(ev.MapPanel, ev.Map, toMove);
                ev.Map.Buildings.Remove(toMove);
                if (eatenSm != null)
                {
                    foreach (Point p in eatenSm.Keys)
                    {
                        Smudge oldSmudge = ev.Map.Smudge[p];
                        if (oldSmudge == null || !oldSmudge.Type.IsAutoBib)
                        {
                            ev.Map.Smudge[p] = eatenSm[p];
                            // DO NOT REMOVE THE POINTS FROM "eaten": the undo might be done again in the future.
                        }
                    }
                }
                if (eatenOv != null)
                {
                    foreach (Point p in eatenOv.Keys)
                    {
                        Overlay oldOverlay = ev.Map.Overlay[p];
                        if (oldOverlay == null || Map.IsIgnorableOverlay(oldOverlay))
                        {
                            ev.Map.Overlay[p] = eatenOv[p];
                            // DO NOT REMOVE THE POINTS FROM "eaten": the undo might be done again in the future.
                        }
                    }
                }
                ev.Map.Buildings.Add(startLocation, toMove);
                InvalidateBuildingArea(ev.MapPanel, ev.Map, toMove);
                //ev.MapPanel.Invalidate(ev.Map, toMove);
                if (ev.Plugin != null)
                {
                    ev.Plugin.Dirty = origDirtyState;
                }
            }
            void redoAction(UndoRedoEventArgs ev)
            {
                InvalidateBuildingArea(ev.MapPanel, ev.Map, toMove);
                ev.Map.Buildings.Remove(toMove);
                if (eatenSm != null)
                {
                    foreach (Point p in eatenSm.Keys)
                    {
                        Smudge oldSmudge = ev.Map.Smudge[p];
                        if (oldSmudge == null || !oldSmudge.Type.IsAutoBib)
                        {
                            ev.Map.Smudge[p] = null;
                            // DO NOT REMOVE THE POINTS FROM "eaten": the undo might be done again in the future.
                        }
                    }
                }
                if (eatenOv != null)
                {
                    foreach (Point p in eatenOv.Keys)
                    {
                        Overlay oldOverlay = ev.Map.Overlay[p];
                        if (oldOverlay == null || Map.IsIgnorableOverlay(oldOverlay))
                        {
                            ev.Map.Overlay[p] = null;
                            // DO NOT REMOVE THE POINTS FROM "eaten": the undo might be done again in the future.
                        }
                    }
                }
                ev.Map.Buildings.Add(endLocation, toMove);
                InvalidateBuildingArea(ev.MapPanel, ev.Map, toMove);
                if (ev.Plugin != null)
                {
                    ev.Plugin.Dirty = true;
                }
            }
            url.Track(undoAction, redoAction, ToolType.Building);
        }

        private void MouseoverWidget_MouseCellChanged(object sender, MouseCellChangedEventArgs e)
        {
            if (placementMode)
            {
                if (SelectedBuildingType != null)
                {
                    Rectangle oldRect = new Rectangle(e.OldCell, SelectedBuildingType.OverlapBounds.Size);
                    Rectangle newRect = new Rectangle(e.NewCell, SelectedBuildingType.OverlapBounds.Size);
                    if (SelectedBuildingType.IsWall)
                    {
                        oldRect.Inflate(1, 1);
                        newRect.Inflate(1, 1);
                    }
                    HashSet<Point> points = oldRect.Points().ToHashSet();
                    points.UnionWith(newRect.Points());
                    mapPanel.Invalidate(map, points);
                }
            }
            else if (selectedBuilding != null)
            {
                if (!startedDragging && selectedBuildingStartLocation.HasValue
                    && new Point(selectedBuildingStartLocation.Value.X + selectedBuildingPivot.X, selectedBuildingStartLocation.Value.Y + selectedBuildingPivot.Y) != e.NewCell)
                {
                    startedDragging = true;
                }
                Building toMove = selectedBuilding;
                Point oldLocation = map.Buildings[toMove].Value;
                Point newLocation = new Point(Math.Max(0, e.NewCell.X - selectedBuildingPivot.X), Math.Max(0, e.NewCell.Y - selectedBuildingPivot.Y));
                InvalidateBuildingArea(mapPanel, map, toMove);
                Point[] oldBibPoints = null;
                Point[] newBibPoints = null;
                Point[] allBibPoints = null;
                Dictionary<Point, Smudge> refreshList = new Dictionary<Point, Smudge>();
                if (toMove.Type.IsWall)
                {
                    if (selectedBuildingEatenOverlay == null)
                    {
                        selectedBuildingEatenOverlay = new Dictionary<Point, Overlay>();
                    }
                    if (selectedBuildingEatenOverlay.ContainsKey(e.OldCell))
                    {
                        Overlay eatenOvl = selectedBuildingEatenOverlay[e.OldCell];
                        map.Overlay[e.OldCell] = eatenOvl;
                        selectedBuildingEatenOverlay.Remove(e.OldCell);
                    }
                    Overlay toEatOvl = map.Overlay[e.NewCell];
                    // Do not allow eating solid overlays
                    if (toEatOvl != null && !toEatOvl.Type.IsWall && !toEatOvl.Type.IsSolid)
                    {
                        selectedBuildingEatenOverlay.Add(e.NewCell, toEatOvl);
                        map.Overlay[e.NewCell] = null;
                    }
                }
                if (toMove.Type.HasBib)
                {
                    oldBibPoints = map.Smudge.IntersectsWithCells(toMove.BibCells).Where(x => x.Value.Type.IsAutoBib)
                        .Select(b => map.Metrics.GetLocation(b.Cell, out Point p) ? p : new Point(-1, -1)).Where(p => p.X >= 0 && p.Y >= 0).ToArray();
                    Dictionary<Point, Smudge> newBib = toMove.GetBib(newLocation, map.SmudgeTypes);
                    newBibPoints = newBib.Keys.ToArray();
                    allBibPoints = oldBibPoints.Union(newBibPoints).ToArray();
                    if (selectedBuildingEatenSmudge == null)
                    {
                        selectedBuildingEatenSmudge = new Dictionary<Point, Smudge>();
                    }
                    foreach (Point newBibPoint in newBibPoints)
                    {
                        if (!map.Metrics.Contains(newBibPoint))
                        {
                            continue;
                        }
                        Smudge oldSmudge = map.Smudge[newBibPoint];
                        if (oldSmudge != null && !oldSmudge.Type.IsAutoBib && !selectedBuildingEatenSmudge.ContainsKey(newBibPoint))
                        {
                            selectedBuildingEatenSmudge.Add(newBibPoint, oldSmudge);
                        }
                    }
                }
                map.Buildings.Remove(toMove);
                // All bib points are restored initially, so the RestoreNearbySmudge logic can do its magic on them.
                if (allBibPoints != null && selectedBuildingEatenSmudge != null)
                {
                    foreach (Point p in allBibPoints)
                    {
                        if (selectedBuildingEatenSmudge.TryGetValue(p, out Smudge toRestore))
                        {
                            Smudge oldSmudge = map.Smudge[p];
                            if (oldSmudge == null || !oldSmudge.Type.IsAutoBib)
                            {
                                map.Smudge[p] = toRestore;
                                // Remove any points that are restored and no longer relevant to remember.
                                if (!newBibPoints.Contains(p))
                                {
                                    selectedBuildingEatenSmudge.Remove(p);
                                }
                            }
                        }
                    }
                }
                if (allBibPoints != null)
                {
                    SmudgeTool.RestoreNearbySmudge(map, allBibPoints, refreshList);
                }
                if (map.Technos.CanAdd(newLocation, toMove, toMove.Type.BaseOccupyMask) && map.Buildings.Add(newLocation, toMove))
                {
                    InvalidateBuildingArea(mapPanel, map, toMove);
                }
                else
                {
                    map.Buildings.Add(oldLocation, toMove);
                    InvalidateBuildingArea(mapPanel, map, toMove);
                }
                // smudge refresh
                mapPanel.Invalidate(map, refreshList.Keys);
            }
            else if (e.MouseButtons == MouseButtons.Right)
            {
                PickBuilding(e.NewCell);
            }
        }

        private void AddBuilding(Point location)
        {
            if (!map.Metrics.Contains(location))
            {
                return;
            }
            if (SelectedBuildingType == null)
            {
                return;
            }
            selectedBuilding = null;
            selectedBuildingStartLocation = null;
            selectedBuildingPivot = Point.Empty;
            startedDragging = false;
            Building building = mockBuilding.Clone();
            if (!map.Technos.CanAdd(location, building, building.Type.BaseOccupyMask))
            {
                return;
            }
            Dictionary<Point, Smudge> newBib = building.GetBib(location, map.SmudgeTypes);
            Dictionary<Point, Smudge> eatenSmudge = null;
            if (newBib != null)
            {
                Point[] newBibPoints = newBib.Keys.ToArray();
                eatenSmudge = new Dictionary<Point, Smudge>();
                foreach (Point newBibPoint in newBibPoints)
                {
                    Smudge oldSmudge = map.Smudge[newBibPoint];
                    if (oldSmudge != null && !oldSmudge.Type.IsAutoBib && !eatenSmudge.ContainsKey(newBibPoint))
                    {
                        eatenSmudge.Add(newBibPoint, oldSmudge);
                    }
                }
            }
            Overlay eatenOverlay = building.Type.IsWall ? map.Overlay[location] : null;
            if (map.Buildings.Add(location, building))
            {
                AdjustBuildPriorities(building, null, out Building[] baseBuildings, out int[] buildingPrioritiesOld, out int[] buildingPrioritiesNew);
                if (eatenOverlay != null)
                {
                    map.Overlay[location] = null;
                }
                InvalidateBuildingArea(mapPanel, map, building);
                bool origDirtyState = plugin.Dirty;
                plugin.Dirty = true;
                void undoAction(UndoRedoEventArgs e)
                {
                    InvalidateBuildingArea(e.MapPanel, e.Map, building);
                    e.Map.Buildings.Remove(building);
                    if (eatenOverlay != null)
                    {
                        map.Overlay[location] = eatenOverlay;
                    }
                    if (eatenSmudge != null)
                    {
                        foreach (Point p in eatenSmudge.Keys)
                        {
                            Smudge oldSmudge = e.Map.Smudge[p];
                            if (oldSmudge == null || !oldSmudge.Type.IsAutoBib)
                            {
                                // DO NOT REMOVE THE POINTS FROM "eatenSmudge": the undo might be done again in the future.
                                e.Map.Smudge[p] = eatenSmudge[p];
                            }
                        }
                    }
                    if (baseBuildings != null && buildingPrioritiesOld != null && baseBuildings.Length == buildingPrioritiesOld.Length)
                    {
                        for (Int32 i = 0; i < baseBuildings.Length; ++i)
                        {
                            Building bld = baseBuildings[i];
                            // current building was removed by undo, so don't bother adjusting it.
                            if (building != bld)
                            {
                                bld.BasePriority = buildingPrioritiesOld[i];
                                InvalidateBuildingArea(e.MapPanel, e.Map, bld);
                            }
                        }
                    }
                    if (e.Plugin != null)
                    {
                        e.Plugin.Dirty = origDirtyState;
                    }
                }
                void redoAction(UndoRedoEventArgs e)
                {
                    e.Map.Buildings.Add(location, building);
                    if (eatenOverlay != null)
                    {
                        map.Overlay[location] = null;
                    }
                    if (baseBuildings != null && buildingPrioritiesNew != null && baseBuildings.Length == buildingPrioritiesNew.Length)
                    {
                        for (Int32 i = 0; i < baseBuildings.Length; ++i)
                        {
                            Building bld = baseBuildings[i];
                            bld.BasePriority = buildingPrioritiesNew[i];
                            InvalidateBuildingArea(e.MapPanel, e.Map, bld);
                        }
                    }
                    InvalidateBuildingArea(e.MapPanel, e.Map, building);
                    if (e.Plugin != null)
                    {
                        e.Plugin.Dirty = true;
                    }
                }
                url.Track(undoAction, redoAction, ToolType.Building);
            }
        }

        private void AdjustBuildPriorities(Building building, Building bldPreEdit, out Building[] baseBuildings, out int[] buildingPrioritiesOld, out int[] buildingPrioritiesNew)
        {
            buildingPrioritiesOld = null;
            buildingPrioritiesNew = null;
            baseBuildings = null;
            if (building.BasePriority >= 0)
            {
                baseBuildings = map.Buildings.OfType<Building>().Where(b => b.Occupier.BasePriority >= 0).OrderBy(b => map.Metrics.GetCell(b.Location)).Select(b => b.Occupier).ToArray();
                int newPr = Math.Max(0, Math.Min(baseBuildings.Length - 1, building.BasePriority));
                buildingPrioritiesOld = new int[baseBuildings.Length];
                buildingPrioritiesNew = new int[baseBuildings.Length];
                // Save old values.
                for (Int32 i = 0; i < baseBuildings.Length; ++i)
                {
                    Building baseBuilding = baseBuildings[i];
                    if (building == baseBuilding && bldPreEdit != null)
                    {
                        baseBuilding = bldPreEdit;
                    }
                    buildingPrioritiesOld[i] = baseBuilding.BasePriority;
                }
                // Re-sort, removing inconsistencies
                List<Building> sortedBuildings = baseBuildings.OrderBy(b => b.BasePriority).ToList();
                // To ensure the new priority is correct, remove it from the list and re-insert it at the correct location.
                sortedBuildings.Remove(building);
                sortedBuildings.Insert(newPr, building);
                for (Int32 i = 0; i < sortedBuildings.Count; ++i)
                {
                    Building baseBuilding = sortedBuildings[i];
                    baseBuilding.BasePriority = i;
                }
                for (Int32 i = 0; i < baseBuildings.Length; ++i)
                {
                    Building baseBuilding = baseBuildings[i];
                    buildingPrioritiesNew[i] = baseBuilding.BasePriority;
                }
                foreach (Building baseBuilding in sortedBuildings)
                {
                    mapPanel.Invalidate(map, baseBuilding);
                }
            }
        }

        private void RemoveBuilding(Point location)
        {
            if (map.Buildings[location] is Building building)
            {
                Point actualPoint = map.Buildings[building].Value;
                Building[] baseBuildings = null;
                int[] buildingPrioritiesOld = null;

                Point[] bibPoints = building.GetBib(actualPoint, map.SmudgeTypes)?.Keys?.ToArray();
                InvalidateBuildingArea(mapPanel, map, building);
                map.Buildings.Remove(building);
                SmudgeTool.RestoreNearbySmudge(map, bibPoints, null);
                if (building.BasePriority >= 0)
                {
                    baseBuildings = map.Buildings.OfType<Building>().Select(x => x.Occupier).Where(x => x.BasePriority >= 0).OrderBy(x => x.BasePriority).ToArray();
                    buildingPrioritiesOld = new int[baseBuildings.Length];
                    for (int i = 0; i < baseBuildings.Length; ++i)
                    {
                        buildingPrioritiesOld[i] = baseBuildings[i].BasePriority;
                        baseBuildings[i].BasePriority = i;
                    }
                    foreach (Building baseBuilding in baseBuildings)
                    {
                        InvalidateBuildingArea(mapPanel, map, baseBuilding);
                    }
                }
                bool origDirtyState = plugin.Dirty;
                plugin.Dirty = true;
                void undoAction(UndoRedoEventArgs e)
                {
                    e.Map.Buildings.Add(actualPoint, building);
                    if (baseBuildings != null && buildingPrioritiesOld != null && baseBuildings.Length == buildingPrioritiesOld.Length)
                    {
                        for (Int32 i = 0; i < baseBuildings.Length; ++i)
                        {
                            Building bld = baseBuildings[i];
                            bld.BasePriority = buildingPrioritiesOld[i];
                            InvalidateBuildingArea(e.MapPanel, e.Map, bld);
                        }
                    }
                    e.MapPanel.Invalidate(e.Map, building);
                    if (e.Plugin != null)
                    {
                        e.Plugin.Dirty = origDirtyState;
                    }
                }
                void redoAction(UndoRedoEventArgs e)
                {
                    InvalidateBuildingArea(e.MapPanel, e.Map, building);
                    e.Map.Buildings.Remove(building);
                    SmudgeTool.RestoreNearbySmudge(e.Map, bibPoints, null);
                    if (baseBuildings != null)
                    {
                        for (Int32 i = 0; i < baseBuildings.Length; ++i)
                        {
                            Building bld = baseBuildings[i];
                            bld.BasePriority = i;
                            InvalidateBuildingArea(e.MapPanel, e.Map, bld);
                        }
                    }
                    if (e.Plugin != null)
                    {
                        e.Plugin.Dirty = true;
                    }
                }
                url.Track(undoAction, redoAction, ToolType.Building);
            }
        }

        private void CheckSelectShortcuts(KeyEventArgs e)
        {
            int maxVal = buildingTypeListBox.Items.Count - 1;
            int curVal = buildingTypeListBox.SelectedIndex;
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
                BuildingType selectedOld = SelectedBuildingType;
                buildingTypeListBox.SelectedIndex = newVal;
                BuildingType selectedNew = SelectedBuildingType;
                if (placementMode)
                {
                    HashSet<Point> refreshArea = new HashSet<Point>();
                    if (selectedNew != null)
                    {
                        Rectangle refresh = new Rectangle(navigationWidget.MouseCell, selectedNew.OverlapBounds.Size);
                        if (selectedNew.IsWall)
                        {
                            refresh.Inflate(1, 1);
                        }
                        refreshArea.Union(new Rectangle(navigationWidget.MouseCell, selectedNew.OverlapBounds.Size).Points());
                    }
                    if (selectedOld != null)
                    {
                        Rectangle refresh = new Rectangle(navigationWidget.MouseCell, selectedOld.OverlapBounds.Size);
                        if (selectedOld.IsWall)
                        {
                            refresh.Inflate(1, 1);
                        }
                        refreshArea.Union(new Rectangle(navigationWidget.MouseCell, selectedNew.OverlapBounds.Size).Points());
                    }
                    mapPanel.Invalidate(map, refreshArea);
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
            BuildingType selected = SelectedBuildingType;
            if (selected != null)
            {
                Rectangle refresh = new Rectangle(navigationWidget.MouseCell, selected.OverlapBounds.Size);
                if (selected.IsWall) refresh.Inflate(1, 1);
                mapPanel.Invalidate(map, refresh);
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
            BuildingType selected = SelectedBuildingType;
            if (selected != null)
            {
                Rectangle refresh = new Rectangle(navigationWidget.MouseCell, selected.OverlapBounds.Size);
                if (selected.IsWall) refresh.Inflate(1, 1);
                mapPanel.Invalidate(map, refresh);
            }
            UpdateStatus();
        }

        private void PickBuilding(Point location)
        {
            BuildingType newPicked = null;
            Building building = map.Buildings[location] as Building;
            if (building != null)
            {
                newPicked = building.Type;
            }
            else if (Globals.AllowWallBuildings && map.Overlay[location] is Overlay overlay && overlay.Type.IsWall)
            {
                string wType = overlay.Type.Name;
                newPicked = map.BuildingTypes.FirstOrDefault(bl => bl.IsWall && String.Equals(bl.Name, wType, StringComparison.OrdinalIgnoreCase));
            }
            if (newPicked != null)
            {
                SelectedBuildingType = newPicked;
            }
            if (building != null)
            {
                mockBuilding.Strength = building.Strength;
                mockBuilding.Direction = building.Direction;
                mockBuilding.Trigger = building.Trigger;
                mockBuilding.BasePriority = building.BasePriority;
                mockBuilding.IsPrebuilt = building.IsPrebuilt;
                mockBuilding.House = building.House;
                mockBuilding.Sellable = building.Sellable;
                mockBuilding.Rebuild = building.Rebuild;
            }
        }

        private void SelectBuilding(Point location)
        {
            selectedBuildingEatenSmudge = null;
            selectedBuilding = null;
            selectedBuildingStartLocation = null;
            selectedBuildingPivot = Point.Empty;
            startedDragging = false;
            if (map.Metrics.GetCell(location, out int cell))
            {
                Building selected = map.Buildings[cell] as Building;
                Point? selectedLocation = selected != null ? map.Buildings[selected] : null;
                Point selectedPivot = selected != null ? location - (Size)selectedLocation : Point.Empty;
                selectedBuildingEatenSmudge = new Dictionary<Point, Smudge>();
                selectedBuilding = selected;
                selectedBuildingStartLocation = selectedLocation;
                selectedBuildingPivot = selectedPivot;
            }
            mapPanel.Invalidate();
            UpdateStatus();
        }

        protected override void MapPropertiesChanged()
        {
            HouseType baseHouse;
            if (!mockBuilding.IsPrebuilt && !mockBuilding.House.Equals(baseHouse = map.GetBaseHouse(plugin.GameInfo)))
            {
                mockBuilding.House = baseHouse;
                // Doesn't need a refresh call; that's handled by MockBuilding_PropertyChanged
            }
        }

        protected override void RefreshPreviewPanel()
        {
            Image oldImage = buildingTypeMapPanel.MapImage;
            if (mockBuilding.Type != null)
            {
                Dictionary <Point, Smudge> bibCells = mockBuilding.GetBib(new Point(0, 0), map.SmudgeTypes);
                List<(Rectangle, Action<Graphics>)> bibRender = new List<(Rectangle, Action<Graphics>)>();
                if (bibCells != null)
                {
                    foreach (Point point in bibCells.Keys)
                    {
                        (Rectangle, Action<Graphics>) bibCellRender = MapRenderer.RenderSmudge(point, Globals.PreviewTileSize, Globals.PreviewTileScale, bibCells[point]);
                        bibRender.Add(bibCellRender);
                    }
                }
                RenderInfo render = MapRenderer.RenderBuilding(plugin.GameInfo, null, new Point(0, 0), Globals.PreviewTileSize, Globals.PreviewTileScale, mockBuilding);
                Size previewSize = mockBuilding.OccupyMask.GetDimensions();
                Bitmap buildingPreview = new Bitmap(previewSize.Width * Globals.PreviewTileWidth, previewSize.Height * Globals.PreviewTileHeight);
                buildingPreview.SetResolution(96, 96);
                using (Graphics g = Graphics.FromImage(buildingPreview))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    foreach ((Rectangle, Action<Graphics>) bib in bibRender)
                    {
                        if (!bib.Item1.IsEmpty)
                        {
                            bib.Item2(g);
                        }
                    }
                    if (render.RenderedObject != null)
                    {
                        render.RenderAction(g);
                    }
                    List<(Point p, Building ter)> buildingList = new List<(Point p, Building ter)>
                        { (new Point(0, 0), mockBuilding) };
                    Rectangle visibleBounds = new Rectangle(Point.Empty, previewSize);
                    MapRenderer.RenderAllOccupierBounds(g, visibleBounds, Globals.PreviewTileSize, buildingList);
                    if (Layers.HasFlag(MapLayerFlag.BuildingFakes))
                    {
                        MapRenderer.RenderFakeBuildingLabel(g, mockBuilding, new Point(0, 0), Globals.PreviewTileSize, false);
                    }
                    if (Layers.HasFlag(MapLayerFlag.BuildingRebuild))
                    {
                        MapRenderer.RenderAllRebuildPriorityLabels(g, plugin.GameInfo, buildingList, visibleBounds, Globals.PreviewTileSize, Globals.PreviewTileScale);
                    }
                    if (Layers.HasFlag(MapLayerFlag.TechnoTriggers))
                    {
                        CellMetrics tm = new CellMetrics(mockBuilding.Type.OverlapBounds.Size);
                        OccupierSet<ICellOccupier> technoSet = new OccupierSet<ICellOccupier>(tm);
                        technoSet.Add(0, mockBuilding);
                        MapRenderer.RenderAllTechnoTriggers(g, plugin.GameInfo, technoSet, tm.Bounds, Globals.PreviewTileSize, Layers, Color.LimeGreen, null, false);
                    }
                }
                buildingTypeMapPanel.MapImage = buildingPreview;
            }
            else
            {
                buildingTypeMapPanel.MapImage = null;
            }
            if (oldImage != null)
            {
                try { oldImage.Dispose(); }
                catch { /* ignore */ }
            }
            buildingTypeMapPanel.Invalidate();
        }

        public override void UpdateStatus()
        {
            if (placementMode)
            {
                statusLbl.Text = "Left-Click to place building, Right-Click to remove building";
            }
            else if (selectedBuilding != null)
            {
                statusLbl.Text = "Drag mouse to move building";
            }
            else
            {
                statusLbl.Text = "Shift to enter placement mode, Left-Click drag to move building, Double-Click to update building properties, Right-Click to pick building";
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
            if (SelectedBuildingType == null)
            {
                return;
            }
            Point location = navigationWidget.MouseCell;
            Building building = mockBuilding.Clone();
            building.Tint = Color.FromArgb(128, Color.White);
            building.IsPreview = true;
            if (previewMap.Technos.CanAdd(location, building, building.Type.BaseOccupyMask) && previewMap.Buildings.Add(location, building))
            {
                if (building.Type.IsWall)
                {
                    previewMap.Overlay[location] = null;
                }
                InvalidateBuildingArea(mapPanel, previewMap, building);
            }
        }

        protected override void PostRenderMap(Graphics graphics, Rectangle visibleCells)
        {
            base.PostRenderMap(graphics, visibleCells);
            // For bounds, add one more cell to get all borders showing.
            Rectangle boundRenderCells = visibleCells;
            boundRenderCells.Inflate(1, 1);
            boundRenderCells.Intersect(map.Metrics.Bounds);
            // Since we manually handle GapRadius painting, need to do the Units ones too.
            if ((Layers & (MapLayerFlag.Units | MapLayerFlag.EffectRadius)) == (MapLayerFlag.Units | MapLayerFlag.EffectRadius))
            {
                MapRenderer.RenderAllUnitEffectRadiuses(graphics, previewMap, boundRenderCells, Globals.MapTileSize, map.RadarJamRadius, null);
            }
            Point location = navigationWidget.MouseCell;
            // Render green outline for any building already placed on the real map.
            MapRenderer.RenderAllOccupierBoundsGreen(graphics, boundRenderCells, Globals.MapTileSize, map.Buildings.OfType<Building>());
            IEnumerable<Point> highlightPoints = null;
            IEnumerable<(Point, Building)> place = null;
            BuildingType selectedType = SelectedBuildingType;
            // Determine overlap cells, and whether to render the current place preview outline.
            if (placementMode && selectedType != null)
            {
                List<Point> occupyPoints = OccupierSet.GetOccupyPoints(location, selectedType.BaseOccupyMask).ToList();
                Boolean isCurrent = map.Technos.OfType<Building>().Any(lo => lo.Location == location && lo.Occupier.Type == selectedType);
                // If there is already a building of this exact type placed on the current location, don't render anything extra.
                if (!isCurrent)
                {
                    // List overlapped points to indicate obstructions.
                    highlightPoints = occupyPoints.Where(p => map.Technos[p] != null || (Globals.BlockingBibs && (map.Smudge[p]?.Type.IsAutoBib ?? false)));
                    // Store info on where to render backup preview outline.
                    place = (location, mockBuilding).Yield();
                }
            }
            // Render green outline of current building.
            if (place != null)
            {
                MapRenderer.RenderAllOccupierBoundsGreen(graphics, boundRenderCells, Globals.MapTileSize, place);
            }
            // Render thin red obstructed cells of all other buildings.
            MapRenderer.RenderAllOccupierCellsRed(graphics, boundRenderCells, Globals.MapTileSize, map.Technos.OfType<Building>());
            // Render thin blue obstructed cells of preview building.
            if (place != null)
            {
                MapRenderer.RenderAllOccupierBounds(graphics, boundRenderCells, Globals.MapTileSize, place, Color.Transparent, Color.Blue);
            }
            // Render thick red over obstructed cells.
            if (highlightPoints != null)
            {
                MapRenderer.RenderAllBoundsFromPoint(graphics, boundRenderCells, Globals.MapTileSize, highlightPoints, Color.Red);
            }
            Building selected = null;
            Point? loc = null;
            if (selectedBuilding != null && selectedBuildingStartLocation.HasValue)
            {
                selected = selectedBuildingStartLocation.HasValue ? selectedBuilding : null;
                loc = map.Buildings[selectedBuilding];
            }
            else if (placementMode)
            {
                (Point p, Building b) = previewMap.Technos.OfType<Building>().Where(t => t.Occupier.IsPreview).FirstOrDefault();
                if (b != null)
                {
                    selected = b;
                    loc = p;
                }
            }
            else if (selectedObjectProperties?.ObjectProperties?.Object is Building bl)
            {
                loc = map.Buildings[bl];
                if (loc.HasValue)
                {
                    selected = bl;
                }
            }
            if (Layers.HasFlag(MapLayerFlag.EffectRadius))
            {
                MapRenderer.RenderAllBuildingEffectRadiuses(graphics, previewMap, boundRenderCells, Globals.MapTileSize, map.GapRadius, selected);
            }
            else if (selected != null)
            {
                MapRenderer.RenderBuildingEffectRadius(graphics, boundRenderCells, Globals.MapTileSize, map.GapRadius, selected, loc.Value, selected);
            }
            this.HandlePaintOutlines(graphics, previewMap, visibleCells, Globals.MapTileSize, Globals.MapTileScale, this.Layers);
            if (Layers.HasFlag(MapLayerFlag.BuildingFakes))
            {
                MapRenderer.RenderAllFakeBuildingLabels(graphics, previewMap, visibleCells, Globals.MapTileSize);
            }
            if (Layers.HasFlag(MapLayerFlag.BuildingRebuild))
            {
                MapRenderer.RenderAllRebuildPriorityLabels(graphics, plugin.GameInfo, previewMap.Buildings.OfType<Building>(), visibleCells, Globals.MapTileSize, Globals.MapTileScale);
            }
            if (Layers.HasFlag(MapLayerFlag.TechnoTriggers))
            {
                MapRenderer.RenderAllTechnoTriggers(graphics, plugin.GameInfo, previewMap, visibleCells, Globals.MapTileSize, Layers);
            }
        }

        public override void Activate()
        {
            base.Activate();
            this.Deactivate(true);
            this.mockBuilding.PropertyChanged += MockBuilding_PropertyChanged;
            this.mapPanel.MouseDown += MapPanel_MouseDown;
            this.mapPanel.MouseUp += MapPanel_MouseUp;
            this.mapPanel.MouseDoubleClick += MapPanel_MouseDoubleClick;
            this.mapPanel.MouseMove += MapPanel_MouseMove;
            this.mapPanel.MouseLeave += MapPanel_MouseLeave;
            this.mapPanel.MouseWheel += MapPanel_MouseWheel;
            this.mapPanel.SuspendMouseZoomKeys = Keys.Control;
            (this.mapPanel as Control).KeyDown += BuildingTool_KeyDown;
            (this.mapPanel as Control).KeyUp += BuildingTool_KeyUp;
            this.navigationWidget.BoundsMouseCellChanged += MouseoverWidget_MouseCellChanged;
            this.navigationWidget.MouseoverSize = new Size(1, 1);
            this.navigationWidget.PenColor = Color.Yellow;
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
            this.mockBuilding.PropertyChanged -= MockBuilding_PropertyChanged;
            this.mapPanel.MouseDown -= MapPanel_MouseDown;
            this.mapPanel.MouseUp -= MapPanel_MouseUp;
            this.mapPanel.MouseDoubleClick -= MapPanel_MouseDoubleClick;
            this.mapPanel.MouseMove -= MapPanel_MouseMove;
            this.mapPanel.MouseLeave -= MapPanel_MouseLeave;
            this.mapPanel.MouseWheel -= MapPanel_MouseWheel;
            this.mapPanel.SuspendMouseZoomKeys = Keys.None;
            (this.mapPanel as Control).KeyDown -= BuildingTool_KeyDown;
            (this.mapPanel as Control).KeyUp -= BuildingTool_KeyUp;
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
                    selectedObjectProperties?.Close();
                    selectedObjectProperties = null;
                    buildingTypeListBox.SelectedIndexChanged -= BuildingTypeListBox_SelectedIndexChanged;
                    Deactivate();
                }
                disposedValue = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
