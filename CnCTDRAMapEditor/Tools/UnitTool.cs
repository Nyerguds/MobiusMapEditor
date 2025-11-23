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
    public class UnitTool : ViewTool
    {
        /// <summary>
        /// Layers that are not painted by the PostRenderMap function on ViewTool level because they are handled
        /// at a specific point in the PostRenderMap override by the implementing tool.
        /// </summary>
        protected override MapLayerFlag ManuallyHandledLayers => MapLayerFlag.TechnoTriggers | MapLayerFlag.EffectRadius | MapLayerFlag.OverlapOutlines;

        private readonly TypeListBox unitTypeListBox;
        private readonly MapPanel unitTypeMapPanel;
        private readonly ObjectProperties objectProperties;

        private Map previewMap;
        protected override Map RenderMap => previewMap;

        public override bool IsBusy { get { return startedDragging; } }

        public override object CurrentObject
        {
            get { return mockUnit; }
            set
            {
                if (value is Unit un)
                {
                    UnitType ut = this.unitTypeListBox.Types.Where(u => u is UnitType unt && unt.ID == un.Type.ID
                        && String.Equals(unt.Name, un.Type.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() as UnitType;
                    if (ut != null)
                    {
                        SelectedUnitType = ut;
                    }
                    un.Type = SelectedUnitType;
                    mockUnit.CloneDataFrom(un);
                    RefreshPreviewPanel();
                }
            }
        }

        private bool placementMode;

        protected override bool InPlacementMode
        {
            get { return placementMode || startedDragging; }
        }

        private readonly Unit mockUnit;

        private Unit selectedUnit;
        private Point? selectedUnitStartLocation;
        private bool startedDragging;

        private ObjectPropertiesPopup selectedObjectProperties;

        private UnitType selectedUnitType;
        private UnitType SelectedUnitType
        {
            get => selectedUnitType;
            set
            {
                if (selectedUnitType != value)
                {
                    if (placementMode && (selectedUnitType != null))
                    {
                        Rectangle bounds = selectedUnitType.OverlapBounds;
                        Point mouseLoc = new Point(navigationWidget.MouseCell.X + bounds.Location.X, navigationWidget.MouseCell.Y + bounds.Location.Y);
                        mapPanel.Invalidate(map, new Rectangle(mouseLoc, bounds.Size));
                    }
                    selectedUnitType = value;
                    unitTypeListBox.SelectedValue = selectedUnitType;
                    if (placementMode && (selectedUnitType != null))
                    {
                        Rectangle bounds = selectedUnitType.OverlapBounds;
                        Point mouseLoc = new Point(navigationWidget.MouseCell.X + bounds.Location.X, navigationWidget.MouseCell.Y + bounds.Location.Y);
                        mapPanel.Invalidate(map, new Rectangle(mouseLoc, bounds.Size));
                    }
                    mockUnit.Type = selectedUnitType;
                }
            }
        }

        public UnitTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, TypeListBox unitTypeListBox, MapPanel unitTypeMapPanel,
            ObjectProperties objectProperties, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> url)
            : base(mapPanel, layers, statusLbl, plugin, url)
        {
            previewMap = map;
            // Leaving these in order of definition.
            List<UnitType> unitTypes = plugin.Map.UnitTypes.ToList();
            UnitType unitType = unitTypes.First();
            mockUnit = new Unit()
            {
                Type = unitType,
                House = map.Houses.First().Type,
                Strength = 256,
                Direction = map.UnitDirectionTypes.Where(d => d.Equals(FacingType.North)).First(),
                Mission = map.GetDefaultMission(unitType)
            };
            this.unitTypeListBox = unitTypeListBox;
            this.unitTypeListBox.Types = unitTypes;
            this.unitTypeListBox.SelectedIndexChanged += UnitTypeListBox_SelectedIndexChanged;
            this.unitTypeMapPanel = unitTypeMapPanel;
            this.unitTypeMapPanel.BackColor = Color.White;
            this.unitTypeMapPanel.MaxZoom = 1;
            this.unitTypeMapPanel.SmoothScale = Globals.PreviewSmoothScale;
            this.objectProperties = objectProperties;
            this.objectProperties.Object = mockUnit;
            SelectedUnitType = mockUnit.Type;
            RefreshPreviewPanel();
        }

        protected override void UpdateExpansionUnits()
        {
            int selectedIndex = unitTypeListBox.SelectedIndex;
            UnitType selected = unitTypeListBox.SelectedValue as UnitType;
            unitTypeListBox.SelectedIndexChanged -= UnitTypeListBox_SelectedIndexChanged;
            List<UnitType> updatedTypes = plugin.Map.UnitTypes.ToList();
            if (!updatedTypes.Contains(selected))
            {
                // Find nearest existing.
                selected = null;
                List<UnitType> oldTypes = this.unitTypeListBox.Types.Cast<UnitType>().ToList();
                for (int i = selectedIndex; i >= 0; --i)
                {
                    if (updatedTypes.Contains(oldTypes[i]))
                    {
                        selected = oldTypes[i];
                        break;
                    }
                }
                if (selected == null)
                {
                    for (int i = selectedIndex; i < oldTypes.Count; ++i)
                    {
                        if (updatedTypes.Contains(oldTypes[i]))
                        {
                            selected = oldTypes[i];
                            break;
                        }
                    }
                }
            }
            unitTypeListBox.Types = updatedTypes;
            unitTypeListBox.SelectedIndexChanged += UnitTypeListBox_SelectedIndexChanged;
            unitTypeListBox.SelectedValue = selected;
        }

        private void MapPanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys != Keys.None || e.Button != MouseButtons.Left)
            {
                return;
            }
            if (map.Metrics.GetCell(navigationWidget.MouseCell, out int cell))
            {
                if (map.Technos[cell] is Unit unit)
                {
                    selectedUnit = null;
                    selectedUnitStartLocation = null;
                    startedDragging = false;
                    mapPanel.Invalidate();
                    Unit preEdit = unit.Clone();
                    selectedObjectProperties?.Close();
                    selectedObjectProperties = new ObjectPropertiesPopup(objectProperties.Plugin, unit);
                    selectedObjectProperties.Closed += (cs, ce) =>
                    {
                        selectedObjectProperties = null;
                        navigationWidget.Refresh();
                        AddPropertiesUndoRedo(unit, preEdit);
                        unit.PropertyChanged -= SelectedUnit_PropertyChanged;
                    };
                    unit.PropertyChanged += SelectedUnit_PropertyChanged;
                    selectedObjectProperties.Show(mapPanel, mapPanel.PointToClient(Control.MousePosition));
                    UpdateStatus();
                }
            }
        }

        private void AddPropertiesUndoRedo(Unit unit, Unit preEdit)
        {
            // unit = unit in its final edited form. Clone for preservation
            Unit redoUnit = unit.Clone();
            Unit undoUnit = preEdit;
            if (redoUnit.DataEquals(undoUnit))
            {
                return;
            }
            bool origEmptyState = plugin.Empty;
            plugin.Dirty = true;
            void undoAction(UndoRedoEventArgs ev)
            {
                unit.CloneDataFrom(undoUnit);
                if (unit.Trigger == null || (!Trigger.IsEmpty(unit.Trigger)
                    && !ev.Map.FilterUnitTriggers().Any(tr => tr.Name.Equals(unit.Trigger, StringComparison.InvariantCultureIgnoreCase))))
                {
                    unit.Trigger = Trigger.None;
                }
                ev.MapPanel.Invalidate(ev.Map, unit);
                if (ev.Plugin != null)
                {
                    ev.Plugin.Empty = origEmptyState;
                    ev.Plugin.Dirty = !ev.NewStateIsClean;
                }
            }
            void redoAction(UndoRedoEventArgs ev)
            {
                unit.CloneDataFrom(redoUnit);
                if (unit.Trigger == null || (!Trigger.IsEmpty(unit.Trigger)
                    && !ev.Map.FilterUnitTriggers().Any(tr => tr.Name.Equals(unit.Trigger, StringComparison.InvariantCultureIgnoreCase))))
                {
                    unit.Trigger = Trigger.None;
                }
                ev.MapPanel.Invalidate(ev.Map, unit);
                if (ev.Plugin != null)
                {
                    // Redo can never restore the "empty" state, but CAN be the point at which a save was done.
                    ev.Plugin.Empty = false;
                    ev.Plugin.Dirty = !ev.NewStateIsClean;
                }
            }
            url.Track(undoAction, redoAction, ToolType.Unit);
        }

        private void MockUnit_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "House")
            {
                plugin.ActiveHouse = mockUnit.House;
            }
            RefreshPreviewPanel();
        }

        private void SelectedUnit_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            mapPanel.Invalidate(map, sender as Unit);
        }

        private void UnitTypeListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedUnitType = unitTypeListBox.SelectedValue as UnitType;
            mockUnit.Mission = map.GetDefaultMission(SelectedUnitType, mockUnit.Mission);
        }

        private void UnitTool_KeyDown(object sender, KeyEventArgs e)
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

        private void UnitTool_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                ExitPlacementMode();
            }
        }

        private void MapPanel_MouseLeave(object sender, EventArgs e)
        {
            ExitPlacementMode();
            MapPanel_MouseUp(sender, new MouseEventArgs(MouseButtons.None, 0, 0, 0, 0));
        }

        private void MapPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta == 0 || !Control.ModifierKeys.HasFlag(Keys.Control))
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

        private void MapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (placementMode)
            {
                if (e.Button == MouseButtons.Left)
                {
                    AddUnit(navigationWidget.MouseCell);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    RemoveUnit(navigationWidget.MouseCell);
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                SelectUnit(navigationWidget.MouseCell);
            }
            else if (e.Button == MouseButtons.Right)
            {
                PickUnit(navigationWidget.MouseCell);
            }
        }

        private void MapPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (selectedUnit != null && selectedUnitStartLocation.HasValue)
            {
                AddMoveUndoTracking(selectedUnit, selectedUnitStartLocation.Value);
                selectedUnit = null;
                selectedUnitStartLocation = null;
                startedDragging = false;
                mapPanel.Invalidate();
                UpdateStatus();
            }
        }

        private void AddMoveUndoTracking(Unit toMove, Point startLocation)
        {
            Point? finalLocation = map.Technos[toMove];
            if (!finalLocation.HasValue || finalLocation.Value == startLocation)
            {
                return;
            }
            Point endLocation = finalLocation.Value;
            bool origEmptyState = plugin.Empty;
            plugin.Dirty = true;
            void undoAction(UndoRedoEventArgs ev)
            {
                ev.MapPanel.Invalidate(ev.Map, toMove);
                ev.Map.Technos.Remove(toMove);
                ev.Map.Technos.Add(startLocation, toMove);
                ev.MapPanel.Invalidate(ev.Map, toMove);
                if (ev.Plugin != null)
                {
                    ev.Plugin.Empty = origEmptyState;
                    ev.Plugin.Dirty = !ev.NewStateIsClean;
                }
            }
            void redoAction(UndoRedoEventArgs ev)
            {
                ev.MapPanel.Invalidate(ev.Map, toMove);
                ev.Map.Technos.Remove(toMove);
                ev.Map.Technos.Add(endLocation, toMove);
                ev.MapPanel.Invalidate(ev.Map, toMove);
                if (ev.Plugin != null)
                {
                    // Redo can never restore the "empty" state, but CAN be the point at which a save was done.
                    ev.Plugin.Empty = false;
                    ev.Plugin.Dirty = !ev.NewStateIsClean;
                }
            }
            url.Track(undoAction, redoAction, ToolType.Unit);
        }

        private void MouseoverWidget_MouseCellChanged(object sender, MouseCellChangedEventArgs e)
        {
            if (placementMode)
            {
                if (SelectedUnitType != null)
                {
                    Rectangle bounds = SelectedUnitType.OverlapBounds;
                    Point oldLoc = new Point(e.OldCell.X + bounds.Location.X, e.OldCell.Y + bounds.Location.Y);
                    Point newLoc = new Point(e.NewCell.X + bounds.Location.X, e.NewCell.Y + bounds.Location.Y);
                    mapPanel.Invalidate(map, new Rectangle(oldLoc, bounds.Size));
                    mapPanel.Invalidate(map, new Rectangle(newLoc, bounds.Size));
                }
            }
            else if (selectedUnit != null)
            {
                if (!startedDragging && selectedUnitStartLocation.HasValue && selectedUnitStartLocation.Value != e.NewCell)
                {
                    startedDragging = true;
                }
                Unit toMove = selectedUnit;
                var oldLocation = map.Technos[toMove].Value;
                mapPanel.Invalidate(map, toMove);
                map.Technos.Remove(toMove);
                if (map.Technos.Add(e.NewCell, toMove))
                {
                    mapPanel.Invalidate(map, toMove);
                }
                else
                {
                    map.Technos.Add(oldLocation, toMove);
                }
            }
            else if (e.MouseButtons == MouseButtons.Right)
            {
                PickUnit(e.NewCell);
            }
        }

        private void AddUnit(Point location)
        {
            if (!map.Metrics.Contains(location))
            {
                return;
            }
            if (SelectedUnitType == null)
            {
                return;
            }
            selectedUnit = null;
            selectedUnitStartLocation = null;
            startedDragging = false;
            var unit = mockUnit.Clone();
            if (map.Technos.Add(location, unit))
            {
                bool origEmptyState = plugin.Empty;
                plugin.Dirty = true;
                mapPanel.Invalidate(map, unit);
                void undoAction(UndoRedoEventArgs ev)
                {
                    ev.MapPanel.Invalidate(ev.Map, unit);
                    ev.Map.Technos.Remove(unit);
                    if (ev.Plugin != null)
                    {
                        ev.Plugin.Empty = origEmptyState;
                        ev.Plugin.Dirty = !ev.NewStateIsClean;
                    }
                }
                void redoAction(UndoRedoEventArgs ev)
                {
                    ev.Map.Technos.Add(location, unit);
                    ev.MapPanel.Invalidate(ev.Map, unit);
                    if (ev.Plugin != null)
                    {
                        // Redo can never restore the "empty" state, but CAN be the point at which a save was done.
                        ev.Plugin.Empty = false;
                        ev.Plugin.Dirty = !ev.NewStateIsClean;
                    }
                }
                url.Track(undoAction, redoAction, ToolType.Unit);
            }
        }

        private void RemoveUnit(Point location)
        {
            if (map.Technos[location] is Unit unit)
            {
                mapPanel.Invalidate(map, unit);
                map.Technos.Remove(unit);
                bool origEmptyState = plugin.Empty;
                plugin.Dirty = true;
                void undoAction(UndoRedoEventArgs ev)
                {
                    ev.Map.Technos.Add(location, unit);
                    ev.MapPanel.Invalidate(ev.Map, unit);
                    if (ev.Plugin != null)
                    {
                        ev.Plugin.Empty = origEmptyState;
                        ev.Plugin.Dirty = !ev.NewStateIsClean;
                    }
                }
                void redoAction(UndoRedoEventArgs ev)
                {
                    ev.MapPanel.Invalidate(ev.Map, unit);
                    ev.Map.Technos.Remove(unit);
                    if (ev.Plugin != null)
                    {
                        // Redo can never restore the "empty" state, but CAN be the point at which a save was done.
                        ev.Plugin.Empty = false;
                        ev.Plugin.Dirty = !ev.NewStateIsClean;
                    }
                }
                url.Track(undoAction, redoAction, ToolType.Unit);
            }
        }

        private void CheckSelectShortcuts(KeyEventArgs e)
        {
            int maxVal = unitTypeListBox.Items.Count - 1;
            int curVal = unitTypeListBox.SelectedIndex;
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
                unitTypeListBox.SelectedIndex = newVal;
                // If in placement mode, the SelectedUnitType property change takes care of the refresh.
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
            if (SelectedUnitType != null)
            {
                Rectangle bounds = SelectedUnitType.OverlapBounds;
                Point mouseLoc = new Point(navigationWidget.MouseCell.X + bounds.Location.X, navigationWidget.MouseCell.Y + bounds.Location.Y);
                mapPanel.Invalidate(map, new Rectangle(mouseLoc, bounds.Size));
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
            if (SelectedUnitType != null)
            {
                Rectangle bounds = SelectedUnitType.OverlapBounds;
                Point mouseLoc = new Point(navigationWidget.MouseCell.X + bounds.Location.X, navigationWidget.MouseCell.Y + bounds.Location.Y);
                mapPanel.Invalidate(map, new Rectangle(mouseLoc, bounds.Size));
            }
            UpdateStatus();
        }

        private void PickUnit(Point location)
        {
            if (map.Technos[location] is Unit unit)
            {
                SelectedUnitType = unit.Type;
                mockUnit.House = unit.House;
                mockUnit.Strength = unit.Strength;
                mockUnit.Direction = unit.Direction;
                mockUnit.Mission = unit.Mission;
                mockUnit.Trigger = unit.Trigger;
            }
        }

        private void SelectUnit(Point location)
        {
            selectedUnit = null;
            selectedUnitStartLocation = null;
            startedDragging = false;
            if (map.Metrics.GetCell(location, out int cell))
            {
                Unit selected = map.Technos[cell] as Unit;
                Point? selectedLocation = selected != null ? map.Technos[selected] : null;
                selectedUnit = selected;
                selectedUnitStartLocation = selectedLocation;
            }
            mapPanel.Invalidate();
            UpdateStatus();
        }

        protected override void RefreshPreviewPanel()
        {
            Image oldImage = unitTypeMapPanel.MapImage;
            if (mockUnit.Type != null)
            {
                bool flying = mockUnit.Type.IsAircraft && mockUnit.Type.IsFlying;
                Rectangle renderRect = mockUnit.Type.OverlapBounds.AdjustToScale(Globals.PreviewTileSize);
                Point renderPoint = new Point(1, mockUnit.Type.OverlapBounds.Height - 2);
                Bitmap unitPreview = new Bitmap(renderRect.Width, renderRect.Height);
                unitPreview.SetResolution(96, 96);
                using (var g = Graphics.FromImage(unitPreview))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    RenderInfo render = MapRenderer.RenderUnit(plugin.GameInfo, map, renderPoint, Globals.PreviewTileSize, mockUnit, false);
                    if (render.RenderedObject != null)
                    {
                        render.RenderAction(g);
                    }
                    if (Layers.HasFlag(MapLayerFlag.TechnoTriggers))
                    {
                        CellMetrics tm = new CellMetrics(3, 3);
                        OccupierSet<ICellOccupier> technoSet = new OccupierSet<ICellOccupier>(tm);
                        technoSet.Add(4, mockUnit);
                        MapRenderer.RenderAllTechnoTriggers(g, plugin.GameInfo, technoSet, null, tm.Bounds, Globals.PreviewTileSize, Layers, Color.LimeGreen, null, false);
                    }
                }
                unitTypeMapPanel.MapImage = unitPreview;
            }
            else
            {
                unitTypeMapPanel.MapImage = null;
            }
            if (oldImage != null)
            {
                try { oldImage.Dispose(); }
                catch { /* ignore */ }
            }
        }

        public override void UpdateStatus()
        {
            if (placementMode)
            {
                statusLbl.Text = "Left-Click to place unit, Right-Click to remove unit";
            }
            else if (selectedUnit != null)
            {
                statusLbl.Text = "Drag mouse to move unit";
            }
            else
            {
                statusLbl.Text = "Shift to enter placement mode, Left-Click drag to move unit, Double-Click to update unit properties, Right-Click to pick unit";
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
            if (SelectedUnitType == null)
            {
                return;
            }
            var location = navigationWidget.MouseCell;
            var unit = mockUnit.Clone();
            unit.IsPreview = true;
            bool placeable = previewMap.Technos.Add(location, unit);
            if (!placeable)
            {
                navigationWidget.MouseoverSize = new Size(1, 1);
            }
        }

        protected override void PostRenderMap(Graphics graphics, Rectangle visibleCells)
        {
            base.PostRenderMap(graphics, visibleCells);
            // Since we manually handle GapRadius painting, need to do the Buildings ones too.
            if (Layers.HasFlag(MapLayerFlag.Buildings | MapLayerFlag.EffectRadius))
            {
                MapRenderer.RenderAllBuildingEffectRadiuses(graphics, previewMap, visibleCells, Globals.MapTileSize, map.GapRadius, null);
            }
            this.HandlePaintOutlines(graphics, previewMap, visibleCells, Globals.MapTileSize, Globals.MapTileScale, this.Layers);
            // For bounds, add one more cell to get all borders showing.
            Rectangle boundRenderCells = visibleCells;
            boundRenderCells.Inflate(1, 1);
            boundRenderCells.Intersect(map.Metrics.Bounds);
            MapRenderer.RenderAllBoundsFromPoint(graphics, boundRenderCells, Globals.MapTileSize, previewMap.Technos.OfType<Unit>());
            if (Layers.HasFlag(MapLayerFlag.TechnoTriggers))
            {
                MapRenderer.RenderAllTechnoTriggers(graphics, plugin.GameInfo, previewMap, visibleCells, Globals.MapTileSize, Layers);
            }
            Unit selected = null;
            Point? loc = null;
            if (selectedUnit != null && selectedUnitStartLocation.HasValue)
            {
                selected = selectedUnit;
                loc = map.Technos[selectedUnit];
            }
            else if (placementMode)
            {
                (Point p, Unit u) = previewMap.Technos.OfType<Unit>().Where(t => t.Occupier.IsPreview).FirstOrDefault();
                if (u != null)
                {
                    selected = u;
                    loc = p;
                }
            }
            else if (selectedObjectProperties?.ObjectProperties?.Object is Unit un)
            {
                loc = map.Technos[un];
                if (loc.HasValue)
                {
                    selected = un;
                }
            }
            if (Layers.HasFlag(MapLayerFlag.EffectRadius))
            {
                MapRenderer.RenderAllUnitEffectRadiuses(graphics, previewMap, visibleCells, Globals.MapTileSize, map.RadarJamRadius, selected);
            }
            else if (selected != null && loc.HasValue)
            {
                MapRenderer.RenderUnitEffectRadius(graphics, Globals.MapTileSize, map.RadarJamRadius, selected, loc.Value, visibleCells, selected);
            }
        }

        public override void Activate()
        {
            base.Activate();
            Deactivate(true);
            mockUnit.PropertyChanged += MockUnit_PropertyChanged;
            mapPanel.MouseDown += MapPanel_MouseDown;
            mapPanel.MouseUp += MapPanel_MouseUp;
            mapPanel.MouseDoubleClick += MapPanel_MouseDoubleClick;
            mapPanel.MouseMove += MapPanel_MouseMove;
            mapPanel.MouseLeave += MapPanel_MouseLeave;
            mapPanel.MouseWheel += MapPanel_MouseWheel;
            mapPanel.LostFocus += MapPanel_MouseLeave;
            mapPanel.SuspendMouseZoomKeys = Keys.Control;
            (mapPanel as Control).KeyDown += UnitTool_KeyDown;
            (mapPanel as Control).KeyUp += UnitTool_KeyUp;
            navigationWidget.BoundsMouseCellChanged += MouseoverWidget_MouseCellChanged;
            UpdateStatus();
            RefreshPreviewPanel();
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
            mockUnit.PropertyChanged -= MockUnit_PropertyChanged;
            mapPanel.MouseDown -= MapPanel_MouseDown;
            mapPanel.MouseUp -= MapPanel_MouseUp;
            mapPanel.MouseDoubleClick -= MapPanel_MouseDoubleClick;
            mapPanel.MouseMove -= MapPanel_MouseMove;
            mapPanel.MouseLeave -= MapPanel_MouseLeave;
            mapPanel.MouseWheel -= MapPanel_MouseWheel;
            mapPanel.LostFocus -= MapPanel_MouseLeave;
            mapPanel.SuspendMouseZoomKeys = Keys.None;
            (mapPanel as Control).KeyDown -= UnitTool_KeyDown;
            (mapPanel as Control).KeyUp -= UnitTool_KeyUp;
            navigationWidget.BoundsMouseCellChanged -= MouseoverWidget_MouseCellChanged;
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
                    unitTypeListBox.SelectedIndexChanged -= UnitTypeListBox_SelectedIndexChanged;
                    Deactivate();
                }
                disposedValue = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
