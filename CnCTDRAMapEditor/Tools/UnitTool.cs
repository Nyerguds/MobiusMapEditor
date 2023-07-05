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
    public class UnitTool : ViewTool
    {
        /// <summary>
        /// Layers that are not painted by the PostRenderMap function on ViewTool level because they are handled
        /// at a specific point in the PostRenderMap override by the implementing tool.
        /// </summary>
        protected override MapLayerFlag ManuallyHandledLayers => MapLayerFlag.TechnoTriggers | MapLayerFlag.EffectRadius | MapLayerFlag.OverlapOutlines;

        private readonly TypeListBox unitTypesBox;
        private readonly MapPanel unitTypeMapPanel;
        private readonly ObjectProperties objectProperties;

        private Map previewMap;
        protected override Map RenderMap => previewMap;

        private bool placementMode;

        protected override Boolean InPlacementMode
        {
            get { return placementMode || startedDragging; }
        }

        private readonly Unit mockUnit;

        private Unit selectedUnit;
        private Point? selectedUnitLocation;
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
                        mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(navigationWidget.MouseCell, new Size(1, 1)), 1, 1));
                    }
                    selectedUnitType = value;
                    unitTypesBox.SelectedValue = selectedUnitType;
                    if (placementMode && (selectedUnitType != null))
                    {
                        mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(navigationWidget.MouseCell, new Size(1, 1)), 1, 1));
                    }
                    mockUnit.Type = selectedUnitType;
                    RefreshPreviewPanel();
                }
            }
        }

        public UnitTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, TypeListBox unitTypesBox, MapPanel unitTypeMapPanel, ObjectProperties objectProperties, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> url)
            : base(mapPanel, layers, statusLbl, plugin, url)
        {
            previewMap = map;
            List<UnitType> unitTypes = plugin.Map.UnitTypes.OrderBy(t => t.ID).ToList();
            UnitType unitType = unitTypes.First();
            mockUnit = new Unit()
            {
                Type = unitType,
                House = map.Houses.First().Type,
                Strength = 256,
                Direction = map.UnitDirectionTypes.Where(d => d.Equals(FacingType.North)).First(),
                Mission = map.GetDefaultMission(unitType)
            };
            mockUnit.PropertyChanged += MockUnit_PropertyChanged;
            this.unitTypesBox = unitTypesBox;
            this.unitTypesBox.Types = unitTypes;
            this.unitTypesBox.SelectedIndexChanged += UnitTypeComboBox_SelectedIndexChanged;
            this.unitTypeMapPanel = unitTypeMapPanel;
            this.unitTypeMapPanel.BackColor = Color.White;
            this.unitTypeMapPanel.MaxZoom = 1;
            this.unitTypeMapPanel.SmoothScale = Globals.PreviewSmoothScale;
            this.objectProperties = objectProperties;
            this.objectProperties.Object = mockUnit;
            SelectedUnitType = mockUnit.Type;
        }

        protected override void UpdateExpansionUnits()
        {
            int selectedIndex = unitTypesBox.SelectedIndex;
            UnitType selected = unitTypesBox.SelectedValue as UnitType;
            unitTypesBox.SelectedIndexChanged -= UnitTypeComboBox_SelectedIndexChanged;
            List<UnitType> updatedTypes = plugin.Map.UnitTypes.OrderBy(t => t.ID).ToList();
            if (!updatedTypes.Contains(selected))
            {
                // Find nearest existing.
                selected = null;
                List<UnitType> oldTypes = this.unitTypesBox.Types.Cast<UnitType>().ToList();
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
            unitTypesBox.Types = updatedTypes;
            unitTypesBox.SelectedIndexChanged += UnitTypeComboBox_SelectedIndexChanged;
            unitTypesBox.SelectedValue = selected;
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
                    selectedUnitLocation = null;
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
            bool origDirtyState = plugin.Dirty;
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
                    ev.Plugin.Dirty = origDirtyState;
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
                    ev.Plugin.Dirty = true;
                }
            }
            url.Track(undoAction, redoAction);
        }

        private void MockUnit_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshPreviewPanel();
        }

        private void SelectedUnit_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            mapPanel.Invalidate(map, sender as Unit);
        }

        private void UnitTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedUnitType = unitTypesBox.SelectedValue as UnitType;
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
            if (selectedUnit != null && selectedUnitLocation.HasValue)
            {
                AddMoveUndoTracking(selectedUnit, selectedUnitLocation.Value);
                selectedUnit = null;
                selectedUnitLocation = null;
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
            bool origDirtyState = plugin.Dirty;
            plugin.Dirty = true;
            void undoAction(UndoRedoEventArgs ev)
            {
                ev.MapPanel.Invalidate(ev.Map, toMove);
                ev.Map.Technos.Remove(toMove);
                ev.Map.Technos.Add(startLocation, toMove);
                ev.MapPanel.Invalidate(ev.Map, toMove);
                if (ev.Plugin != null)
                {
                    ev.Plugin.Dirty = origDirtyState;
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
                    ev.Plugin.Dirty = true;
                }
            }
            url.Track(undoAction, redoAction);
        }

        private void MouseoverWidget_MouseCellChanged(object sender, MouseCellChangedEventArgs e)
        {
            if (placementMode)
            {
                if (SelectedUnitType != null)
                {
                    mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(e.OldCell, new Size(1, 1)), 1, 1));
                    mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(e.NewCell, new Size(1, 1)), 1, 1));
                }
            }
            else if (selectedUnit != null)
            {
                if (!startedDragging && selectedUnitLocation.HasValue && selectedUnitLocation.Value != e.NewCell)
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
            selectedUnitLocation = null;
            startedDragging = false;
            var unit = mockUnit.Clone();
            if (map.Technos.Add(location, unit))
            {
                bool origDirtyState = plugin.Dirty;
                plugin.Dirty = true;
                mapPanel.Invalidate(map, unit);
                void undoAction(UndoRedoEventArgs e)
                {
                    e.MapPanel.Invalidate(e.Map, unit);
                    e.Map.Technos.Remove(unit);
                    if (e.Plugin != null)
                    {
                        e.Plugin.Dirty = origDirtyState;
                    }
                }
                void redoAction(UndoRedoEventArgs e)
                {
                    e.Map.Technos.Add(location, unit);
                    e.MapPanel.Invalidate(e.Map, unit);
                    if (e.Plugin != null)
                    {
                        e.Plugin.Dirty = true;
                    }
                }
                url.Track(undoAction, redoAction);
            }
        }

        private void RemoveUnit(Point location)
        {
            if (map.Technos[location] is Unit unit)
            {
                mapPanel.Invalidate(map, unit);
                map.Technos.Remove(unit);
                bool origDirtyState = plugin.Dirty;
                plugin.Dirty = true;
                void undoAction(UndoRedoEventArgs e)
                {
                    e.Map.Technos.Add(location, unit);
                    e.MapPanel.Invalidate(e.Map, unit);
                    if (e.Plugin != null)
                    {
                        e.Plugin.Dirty = origDirtyState;
                    }
                }
                void redoAction(UndoRedoEventArgs e)
                {
                    e.MapPanel.Invalidate(e.Map, unit);
                    e.Map.Technos.Remove(unit);
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
            int maxVal = unitTypesBox.Items.Count - 1;
            int curVal = unitTypesBox.SelectedIndex;
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
                unitTypesBox.SelectedIndex = newVal;
                if (placementMode)
                {
                    mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(navigationWidget.MouseCell, new Size(1, 1)), 1, 1));
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
            if (SelectedUnitType != null)
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
            if (SelectedUnitType != null)
            {
                mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(navigationWidget.MouseCell, new Size(1, 1)), 1, 1));
            }
            UpdateStatus();
        }

        private void PickUnit(Point location)
        {
            if (map.Metrics.GetCell(location, out int cell))
            {
                if (map.Technos[cell] is Unit unit)
                {
                    SelectedUnitType = unit.Type;
                    mockUnit.House = unit.House;
                    mockUnit.Strength = unit.Strength;
                    mockUnit.Direction = unit.Direction;
                    mockUnit.Mission = unit.Mission;
                    mockUnit.Trigger = unit.Trigger;
                }
            }
        }

        private void SelectUnit(Point location)
        {
            selectedUnit = null;
            selectedUnitLocation = null;
            startedDragging = false;
            if (map.Metrics.GetCell(location, out int cell))
            {
                Unit selected = map.Technos[cell] as Unit;
                Point? selectedLocation = selected != null ? map.Technos[selected] : null;
                selectedUnit = selected;
                selectedUnitLocation = selectedLocation;
            }
            mapPanel.Invalidate();
            UpdateStatus();
        }

        protected override void RefreshPreviewPanel()
        {
            var oldImage = unitTypeMapPanel.MapImage;
            if (mockUnit.Type != null)
            {
                var unitPreview = new Bitmap(Globals.PreviewTileWidth * 3, Globals.PreviewTileWidth * 3);
                unitPreview.SetResolution(96, 96);
                using (var g = Graphics.FromImage(unitPreview))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    MapRenderer.RenderUnit(plugin.GameType, new Point(1, 1), Globals.PreviewTileSize, mockUnit).Item2(g);
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

        private void UpdateStatus()
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
            if (SelectedUnitType == null)
            {
                return;
            }
            var location = navigationWidget.MouseCell;
            var unit = mockUnit.Clone();
            unit.Tint = Color.FromArgb(128, Color.White);
            unit.IsPreview = true;
            previewMap.Technos.Add(location, unit);
        }

        protected override void PostRenderMap(Graphics graphics, Rectangle visibleCells)
        {
            base.PostRenderMap(graphics, visibleCells);
            // Since we manually handle GapRadius painting, need to do the Buildings ones too.
            if ((Layers & (MapLayerFlag.Buildings | MapLayerFlag.EffectRadius)) == (MapLayerFlag.Buildings | MapLayerFlag.EffectRadius))
            {
                MapRenderer.RenderAllBuildingEffectRadiuses(graphics, previewMap, visibleCells, Globals.MapTileSize, map.GapRadius, null);
            }
            this. HandlePaintOutlines(graphics, previewMap, visibleCells, Globals.MapTileSize, Globals.MapTileScale, this.Layers);
            // For bounds, add one more cell to get all borders showing.
            Rectangle boundRenderCells = visibleCells;
            boundRenderCells.Inflate(1, 1);
            boundRenderCells.Intersect(map.Metrics.Bounds);
            MapRenderer.RenderAllBoundsFromPoint(graphics, boundRenderCells, Globals.MapTileSize, previewMap.Technos.OfType<Unit>());
            if ((Layers & MapLayerFlag.TechnoTriggers) == MapLayerFlag.TechnoTriggers)
            {
                MapRenderer.RenderAllTechnoTriggers(graphics, previewMap, visibleCells, Globals.MapTileSize, Layers);
            }
            Unit selected = null;
            Point? loc = null;
            if (selectedUnit != null && selectedUnitLocation.HasValue)
            {
                selected = selectedUnit;
                loc = selectedUnitLocation.Value;
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
            if ((Layers & MapLayerFlag.EffectRadius) == MapLayerFlag.EffectRadius)
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
            this.mapPanel.MouseDown += MapPanel_MouseDown;
            this.mapPanel.MouseUp += MapPanel_MouseUp;
            this.mapPanel.MouseDoubleClick += MapPanel_MouseDoubleClick;
            this.mapPanel.MouseMove += MapPanel_MouseMove;
            this.mapPanel.MouseLeave += MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown += UnitTool_KeyDown;
            (this.mapPanel as Control).KeyUp += UnitTool_KeyUp;
            navigationWidget.BoundsMouseCellChanged += MouseoverWidget_MouseCellChanged;
            UpdateStatus();
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
            this.mapPanel.MouseDown -= MapPanel_MouseDown;
            this.mapPanel.MouseUp -= MapPanel_MouseUp;
            this.mapPanel.MouseDoubleClick -= MapPanel_MouseDoubleClick;
            this.mapPanel.MouseMove -= MapPanel_MouseMove;
            this.mapPanel.MouseLeave -= MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown -= UnitTool_KeyDown;
            (this.mapPanel as Control).KeyUp -= UnitTool_KeyUp;
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
                    unitTypesBox.SelectedIndexChanged -= UnitTypeComboBox_SelectedIndexChanged;
                    Deactivate();
                }
                disposedValue = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
