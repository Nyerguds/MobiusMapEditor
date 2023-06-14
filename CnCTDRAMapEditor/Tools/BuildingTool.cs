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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Tools
{
    public class BuildingTool : ViewTool
    {
        private readonly TypeListBox buildingTypesBox;
        private readonly MapPanel buildingTypeMapPanel;
        private readonly ObjectProperties objectProperties;

        private Map previewMap;
        protected override Map RenderMap => previewMap;

        /// <summary>
        /// Layers that are not painted by the PostRenderMap function on ViewTool level because they are handled
        /// at a specific point in the PostRenderMap override by the implementing tool.
        /// </summary>
        protected override MapLayerFlag ManuallyHandledLayers => MapLayerFlag.TechnoTriggers | MapLayerFlag.BuildingRebuild | MapLayerFlag.BuildingFakes | MapLayerFlag.EffectRadius;

        private bool placementMode;

        protected override Boolean InPlacementMode
        {
            get { return placementMode || startedDragging; }
        }

        private readonly Building mockBuilding;

        private Building selectedBuilding;
        private Point? selectedBuildingLocation;
        private Dictionary<Point, Smudge> selectedBuildingEatenSmudge;
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
                        mapPanel.Invalidate(map, new Rectangle(navigationWidget.MouseCell, selectedBuildingType.OverlapBounds.Size));
                    }
                    selectedBuildingType = value;
                    buildingTypesBox.SelectedValue = selectedBuildingType;
                    if (placementMode && (selectedBuildingType != null))
                    {
                        mapPanel.Invalidate(map, new Rectangle(navigationWidget.MouseCell, selectedBuildingType.OverlapBounds.Size));
                    }
                    mockBuilding.Type = selectedBuildingType;
                    RefreshPreviewPanel();
                }
            }
        }

        public BuildingTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, TypeListBox buildingTypesBox, MapPanel buildingTypeMapPanel, ObjectProperties objectProperties, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> url)
            : base(mapPanel, layers, statusLbl, plugin, url)
        {
            previewMap = map;
            mockBuilding = new Building()
            {
                Type = buildingTypesBox.Types.First() as BuildingType,
                House = map.Houses.First().Type,
                Strength = 256,
                Direction = map.BuildingDirectionTypes.Where(d => d.Equals(FacingType.North)).First()
            };
            mockBuilding.PropertyChanged += MockBuilding_PropertyChanged;
            this.buildingTypesBox = buildingTypesBox;
            this.buildingTypesBox.SelectedIndexChanged += BuildingTypeComboBox_SelectedIndexChanged;
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
                    selectedBuildingLocation = null;
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
                ev.MapPanel.Invalidate(ev.Map, building);
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
                ev.MapPanel.Invalidate(ev.Map, building);
                if (ev.Plugin != null)
                {
                    ev.Plugin.Dirty = true;
                }
            }
            url.Track(undoAction, redoAction);
        }

        private void MockBuilding_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Type" && (mockBuilding.Type == null || !mockBuilding.Type.HasTurret))
            {
                mockBuilding.Direction = map.BuildingDirectionTypes.Where(d => d.Equals(FacingType.North)).First();
            }
            RefreshPreviewPanel();
        }

        private void SelectedBuilding_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            mapPanel.Invalidate(map, sender as Building);
        }

        private void BuildingTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedBuildingType = buildingTypesBox.SelectedValue as BuildingType;
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
            if (selectedBuilding != null && selectedBuildingLocation.HasValue)
            {
                AddMoveUndoTracking(selectedBuilding, selectedBuildingLocation.Value);
                selectedBuildingEatenSmudge = null;
                selectedBuilding = null;
                selectedBuildingLocation = null;
                selectedBuildingPivot = Point.Empty;
                startedDragging = false;
                mapPanel.Invalidate();
                UpdateStatus();
            }
        }

        private void AddMoveUndoTracking(Building toMove, Point startLocation)
        {
            Point? finalLocation = map.Buildings[toMove];
            Dictionary<Point, Smudge> eaten = selectedBuildingEatenSmudge.ToDictionary(p => p.Key, p => p.Value);
            if (!finalLocation.HasValue || finalLocation.Value == selectedBuildingLocation)
            {
                return;
            }
            bool origDirtyState = plugin.Dirty;
            plugin.Dirty = true;
            Point endLocation = finalLocation.Value;
            void undoAction(UndoRedoEventArgs ev)
            {
                ev.MapPanel.Invalidate(ev.Map, toMove);
                ev.Map.Buildings.Remove(toMove);
                if (eaten != null)
                {
                    foreach (Point p in eaten.Keys)
                    {
                        Smudge oldSmudge = ev.Map.Smudge[p];
                        if (oldSmudge == null || !oldSmudge.Type.IsAutoBib)
                        {
                            ev.Map.Smudge[p] = eaten[p];
                            // DO NOT REMOVE THE POINTS FROM "eaten": the undo might be done again in the future.
                        }
                    }
                }
                ev.Map.Buildings.Add(startLocation, toMove);
                ev.MapPanel.Invalidate(ev.Map, toMove);
                if (ev.Plugin != null)
                {
                    ev.Plugin.Dirty = origDirtyState;
                }
            }
            void redoAction(UndoRedoEventArgs ev)
            {
                ev.MapPanel.Invalidate(ev.Map, toMove);
                ev.Map.Buildings.Remove(toMove);
                if (eaten != null)
                {
                    foreach (Point p in eaten.Keys)
                    {
                        Smudge oldSmudge = ev.Map.Smudge[p];
                        if (oldSmudge == null || !oldSmudge.Type.IsAutoBib)
                        {
                            ev.Map.Smudge[p] = eaten[p];
                            // DO NOT REMOVE THE POINTS FROM "eaten": the undo might be done again in the future.
                        }
                    }
                }
                ev.Map.Buildings.Add(endLocation, toMove);
                ev.MapPanel.Invalidate(ev.Map, toMove);
                if (ev.Plugin != null)
                {
                    ev.Plugin.Dirty = true;
                }
            }
            url.Track(undoAction, redoAction);
        }

        private void MouseoverWidget_MouseCellChanged(object sender, MouseCellChangedEventArgs e)
        {
            if (placementMode)
            {
                if (SelectedBuildingType != null)
                {
                    mapPanel.Invalidate(map, new Rectangle(e.OldCell, SelectedBuildingType.OverlapBounds.Size));
                    mapPanel.Invalidate(map, new Rectangle(e.NewCell, SelectedBuildingType.OverlapBounds.Size));
                }
            }
            else if (selectedBuilding != null)
            {
                if (!startedDragging && selectedBuildingLocation.HasValue
                    && new Point(selectedBuildingLocation.Value.X + selectedBuildingPivot.X, selectedBuildingLocation.Value.Y + selectedBuildingPivot.Y) != e.NewCell)
                {
                    startedDragging = true;
                }
                Building toMove = selectedBuilding;
                var oldLocation = map.Buildings[toMove].Value;
                var newLocation = new Point(Math.Max(0, e.NewCell.X - selectedBuildingPivot.X), Math.Max(0, e.NewCell.Y - selectedBuildingPivot.Y));
                mapPanel.Invalidate(map, toMove);
                Point[] oldBibPoints = null;
                Point[] newBibPoints = null;
                Point[] allBibPoints = null;
                Dictionary<Point, Smudge> refreshList = new Dictionary<Point, Smudge>();
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
                        Smudge oldSmudge = map.Smudge[newBibPoint];
                        if (oldSmudge != null && !oldSmudge.Type.IsAutoBib && !selectedBuildingEatenSmudge.ContainsKey(newBibPoint)) {
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
                    mapPanel.Invalidate(map, toMove);
                }
                else
                {
                    map.Buildings.Add(oldLocation, toMove);
                }
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
            selectedBuildingLocation = null;
            selectedBuildingPivot = Point.Empty;
            startedDragging = false;
            var building = mockBuilding.Clone();
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
            if (map.Buildings.Add(location, building))
            {
                Building[] baseBuildings = null;
                int[] buildingPrioritiesOld = null;
                Building[] baseBuildingsOrdered = null;
                if (building.BasePriority >= 0)
                {
                    baseBuildings = map.Buildings.OfType<Building>().Select(x => x.Occupier).Where(x => x.BasePriority >= 0).ToArray();
                    buildingPrioritiesOld = new int[baseBuildings.Length];
                    for (Int32 i = 0; i < baseBuildings.Length; ++i)
                    {
                        Building baseBuilding = baseBuildings[i];
                        buildingPrioritiesOld[i] = baseBuilding.BasePriority;
                        if ((building != baseBuilding) && (baseBuilding.BasePriority >= building.BasePriority))
                        {
                            baseBuilding.BasePriority++;
                        }
                    }
                    baseBuildingsOrdered = baseBuildings.OrderBy(x => x.BasePriority).ToArray();
                    for (var i = 0; i < baseBuildingsOrdered.Length; ++i)
                    {
                        baseBuildingsOrdered[i].BasePriority = i;
                    }
                    foreach (var baseBuilding in baseBuildings)
                    {
                        mapPanel.Invalidate(map, baseBuilding);
                    }
                }
                mapPanel.Invalidate(map, building);
                bool origDirtyState = plugin.Dirty;
                plugin.Dirty = true;
                void undoAction(UndoRedoEventArgs e)
                {
                    e.MapPanel.Invalidate(e.Map, building);
                    e.Map.Buildings.Remove(building);
                    if (eatenSmudge != null)
                    {
                        foreach (Point p in eatenSmudge.Keys)
                        {
                            Smudge oldSmudge = e.Map.Smudge[p];
                            if (oldSmudge == null || !oldSmudge.Type.IsAutoBib)
                            {
                                e.Map.Smudge[p] = eatenSmudge[p];
                                // DO NOT REMOVE THE POINTS FROM "eatenSmudge": the undo might be done again in the future.
                            }
                        }
                    }
                    if (baseBuildings != null && buildingPrioritiesOld != null && baseBuildings.Length == buildingPrioritiesOld.Length)
                    {
                        for (Int32 i = 0; i < baseBuildings.Length; ++i)
                        {
                            Building bld = baseBuildings[i];
                            if (building != bld)
                            {
                                bld.BasePriority = buildingPrioritiesOld[i];
                                e.MapPanel.Invalidate(map, bld);
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
                    if (baseBuildingsOrdered != null)
                    {
                        for (Int32 i = 0; i < baseBuildingsOrdered.Length; ++i)
                        {
                            Building bld = baseBuildingsOrdered[i];
                            bld.BasePriority = i;
                            e.MapPanel.Invalidate(e.Map, bld);
                        }
                    }
                    e.MapPanel.Invalidate(e.Map, building);
                    if (e.Plugin != null)
                    {
                        e.Plugin.Dirty = true;
                    }
                }
                url.Track(undoAction, redoAction);
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
                mapPanel.Invalidate(map, building);
                map.Buildings.Remove(building);
                SmudgeTool.RestoreNearbySmudge(map, bibPoints, null);
                if (building.BasePriority >= 0)
                {
                    baseBuildings = map.Buildings.OfType<Building>().Select(x => x.Occupier).Where(x => x.BasePriority >= 0).OrderBy(x => x.BasePriority).ToArray();
                    buildingPrioritiesOld = new int[baseBuildings.Length];
                    for (var i = 0; i < baseBuildings.Length; ++i)
                    {
                        buildingPrioritiesOld[i] = baseBuildings[i].BasePriority;
                        baseBuildings[i].BasePriority = i;
                    }
                    foreach (var baseBuilding in map.Buildings.OfType<Building>().Select(x => x.Occupier).Where(x => x.BasePriority >= 0))
                    {
                        mapPanel.Invalidate(map, baseBuilding);
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
                            e.MapPanel.Invalidate(e.Map, bld);
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
                    e.MapPanel.Invalidate(e.Map, building);
                    e.Map.Buildings.Remove(building);
                    SmudgeTool.RestoreNearbySmudge(map, bibPoints, null);
                    if (baseBuildings != null)
                    {
                        for (Int32 i = 0; i < baseBuildings.Length; ++i)
                        {
                            Building bld = baseBuildings[i];
                            bld.BasePriority = i;
                            e.MapPanel.Invalidate(map, bld);
                        }
                    }
                    if (e.Plugin != null)
                    {
                        e.Plugin.Dirty = true;
                    }
                }
                url.Track(undoAction, redoAction);
            }
        }

        private void CheckSelectShortcuts(KeyEventArgs e)
        {
            int maxVal = buildingTypesBox.Items.Count - 1;
            int curVal = buildingTypesBox.SelectedIndex;
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
                buildingTypesBox.SelectedIndex = newVal;
                BuildingType selected = SelectedBuildingType;
                if (placementMode && selected != null)
                {
                    mapPanel.Invalidate(map, new Rectangle(navigationWidget.MouseCell, selected.OverlapBounds.Size));
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
            if (SelectedBuildingType != null)
            {
                mapPanel.Invalidate(map, new Rectangle(navigationWidget.MouseCell, SelectedBuildingType.OverlapBounds.Size));
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
            if (SelectedBuildingType != null)
            {
                mapPanel.Invalidate(map, new Rectangle(navigationWidget.MouseCell, SelectedBuildingType.OverlapBounds.Size));
            }
            UpdateStatus();
        }

        private void PickBuilding(Point location)
        {
            if (map.Metrics.GetCell(location, out int cell))
            {
                if (map.Buildings[cell] is Building building)
                {
                    SelectedBuildingType = building.Type;
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
        }

        private void SelectBuilding(Point location)
        {
            selectedBuildingEatenSmudge = null;
            selectedBuilding = null;
            selectedBuildingLocation = null;
            selectedBuildingPivot = Point.Empty;
            startedDragging = false;
            if (map.Metrics.GetCell(location, out int cell))
            {
                Building selected = map.Buildings[cell] as Building;
                Point? selectedLocation = selected != null ? map.Buildings[selected] : null;
                Point selectedPivot = selected != null ? location - (Size)selectedLocation : Point.Empty;
                selectedBuildingEatenSmudge = new Dictionary<Point, Smudge>();
                selectedBuilding = selected;
                selectedBuildingLocation = selectedLocation;
                selectedBuildingPivot = selectedPivot;
            }
            mapPanel.Invalidate();
            UpdateStatus();
        }

        protected override void MapPropertiesChanged()
        {
            HouseType baseHouse;
            if (!mockBuilding.IsPrebuilt && !mockBuilding.House.Equals(baseHouse = map.GetBaseHouse(plugin.GameType)))
            {
                mockBuilding.House = baseHouse;
                // Doesn't need a refresh call; that's handled by MockBuilding_PropertyChanged
            }
        }

        protected override void RefreshPreviewPanel()
        {
            var oldImage = buildingTypeMapPanel.MapImage;
            if (mockBuilding.Type != null)
            {
                Dictionary <Point, Smudge> bibCells = mockBuilding.GetBib(new Point(0, 0), map.SmudgeTypes);
                List<(Rectangle, Action<Graphics>)> bibRender = new List<(Rectangle, Action<Graphics>)>();
                if (bibCells != null)
                {
                    foreach (var point in bibCells.Keys)
                    {
                        var bibCellRender = MapRenderer.RenderSmudge(map.Theater, point, Globals.PreviewTileSize, Globals.PreviewTileScale, bibCells[point]);
                        bibRender.Add(bibCellRender);
                    }
                }
                var renderBuilding = MapRenderer.RenderBuilding(plugin.GameType, new Point(0, 0), Globals.PreviewTileSize, Globals.PreviewTileScale, mockBuilding);
                Size previewSize = mockBuilding.OverlapBounds.Size;
                var buildingPreview = new Bitmap(previewSize.Width * Globals.PreviewTileWidth, previewSize.Height * Globals.PreviewTileHeight);
                buildingPreview.SetResolution(96, 96);
                using (var g = Graphics.FromImage(buildingPreview))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    foreach (var bib in bibRender)
                    {
                        if (!bib.Item1.IsEmpty)
                        {
                            bib.Item2(g);
                        }
                    }
                    if (!renderBuilding.Item1.IsEmpty)
                    {
                        renderBuilding.Item2(g);
                    }
                    List<(Point p, Building ter)> buildingList = new List<(Point p, Building ter)>();
                    buildingList.Add((new Point(0, 0), mockBuilding));
                    MapRenderer.RenderAllOccupierBounds(g, Globals.PreviewTileSize, buildingList);
                    if ((Layers & MapLayerFlag.BuildingFakes) == MapLayerFlag.BuildingFakes)
                    {
                        MapRenderer.RenderFakeBuildingLabel(g, mockBuilding, new Point(0, 0), Globals.PreviewTileSize, Globals.PreviewTileScale, false);
                    }
                    if ((Layers & MapLayerFlag.BuildingRebuild) == MapLayerFlag.BuildingRebuild)
                    {
                        MapRenderer.RenderRebuildPriorityLabel(g, mockBuilding, new Point(0, 0), Globals.PreviewTileSize, Globals.PreviewTileScale, false);
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

        private void UpdateStatus()
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
            var location = navigationWidget.MouseCell;
            var building = mockBuilding.Clone();
            building.Tint = Color.FromArgb(128, Color.White);
            if (previewMap.Technos.CanAdd(location, building, building.Type.BaseOccupyMask) && previewMap.Buildings.Add(location, building))
            {
                mapPanel.Invalidate(previewMap, building);
            }
        }

        protected override void PostRenderMap(Graphics graphics)
        {
            base.PostRenderMap(graphics);
            // Since we manually handle GapRadius painting, need to do the Units ones too.
            if ((Layers & (MapLayerFlag.Units | MapLayerFlag.EffectRadius)) == (MapLayerFlag.Units | MapLayerFlag.EffectRadius))
            {
                MapRenderer.RenderAllUnitEffectRadiuses(graphics, previewMap, Globals.MapTileSize, map.RadarJamRadius);
            }
            MapRenderer.RenderAllOccupierBounds(graphics, Globals.MapTileSize, previewMap.Buildings.OfType<Building>());
            if ((Layers & MapLayerFlag.Buildings) == MapLayerFlag.Buildings)
            {
                if ((Layers & MapLayerFlag.EffectRadius) == MapLayerFlag.EffectRadius)
                {
                    MapRenderer.RenderAllBuildingEffectRadiuses(graphics, previewMap, Globals.MapTileSize, map.GapRadius);
                }
                else if (placementMode)
                {
                    (Point p, Building b) = previewMap.Technos.OfType<Building>().Where(t => t.Occupier.Tint.A != 255).FirstOrDefault();
                    if (b != null)
                    {
                        MapRenderer.RenderBuildingEffectRadius(graphics, Globals.MapTileSize, map.GapRadius, b, p);
                    }
                }
                else if (selectedBuilding != null && selectedBuildingLocation.HasValue)
                {
                    Point? loc = map.Buildings[selectedBuilding];
                    if (loc.HasValue)
                    {
                        MapRenderer.RenderBuildingEffectRadius(graphics, Globals.MapTileSize, map.GapRadius, selectedBuilding, loc.Value);
                    }
                }
                else if (selectedObjectProperties?.ObjectProperties?.Object is Building bl)
                {
                    Point? loc = map.Buildings[bl];
                    if (loc.HasValue)
                    {
                        MapRenderer.RenderBuildingEffectRadius(graphics, Globals.MapTileSize, map.GapRadius, bl, loc.Value);
                    }
                }
                if ((Layers & MapLayerFlag.BuildingFakes) == MapLayerFlag.BuildingFakes)
                {
                    MapRenderer.RenderAllFakeBuildingLabels(graphics, previewMap, Globals.MapTileSize, Globals.MapTileScale);
                }
                if ((Layers & MapLayerFlag.BuildingRebuild) == MapLayerFlag.BuildingRebuild)
                {
                    MapRenderer.RenderAllRebuildPriorityLabels(graphics, previewMap, Globals.MapTileSize, Globals.MapTileScale);
                }
                if ((Layers & MapLayerFlag.TechnoTriggers) == MapLayerFlag.TechnoTriggers)
                {
                    MapRenderer.RenderAllTechnoTriggers(graphics, previewMap, Globals.MapTileSize, Globals.MapTileScale, Layers);
                }
            }
        }

        public override void Activate()
        {
            base.Activate();
            this.Deactivate(true);
            this.mapPanel.MouseDown += MapPanel_MouseDown;
            this.mapPanel.MouseUp += MapPanel_MouseUp;
            this.mapPanel.MouseDoubleClick += MapPanel_MouseDoubleClick;
            this.mapPanel.MouseMove += MapPanel_MouseMove;
            this.mapPanel.MouseLeave += MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown += BuildingTool_KeyDown;
            (this.mapPanel as Control).KeyUp += BuildingTool_KeyUp;
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
            this.mapPanel.MouseDoubleClick -= MapPanel_MouseDoubleClick;
            this.mapPanel.MouseMove -= MapPanel_MouseMove;
            this.mapPanel.MouseLeave -= MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown -= BuildingTool_KeyDown;
            (this.mapPanel as Control).KeyUp -= BuildingTool_KeyUp;
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
                    selectedObjectProperties?.Close();
                    selectedObjectProperties = null;
                    buildingTypesBox.SelectedIndexChanged -= BuildingTypeComboBox_SelectedIndexChanged;
                    Deactivate();
                }
                disposedValue = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
