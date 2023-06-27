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
    public class InfantryTool : ViewTool
    {
        private readonly TypeListBox infantryTypesBox;
        private readonly MapPanel infantryTypeMapPanel;
        private readonly ObjectProperties objectProperties;

        private Map previewMap;
        protected override Map RenderMap => previewMap;

        /// <summary>
        /// Layers that are not painted by the PostRenderMap function on ViewTool level because they are handled
        /// at a specific point in the PostRenderMap override by the implementing tool.
        /// </summary>
        protected override MapLayerFlag ManuallyHandledLayers => MapLayerFlag.TechnoTriggers | MapLayerFlag.OverlapOutlines;

        private bool placementMode;

        protected override Boolean InPlacementMode
        {
            get { return placementMode || startedDragging; }
        }

        private readonly Infantry mockInfantry;

        private Infantry selectedInfantry;
        private Point? selectedInfantryLocation;
        private int selectedInfantryStop = -1;
        private bool startedDragging;
        private ObjectPropertiesPopup selectedObjectProperties;

        private InfantryType selectedInfantryType;
        private InfantryType SelectedInfantryType
        {
            get => selectedInfantryType;
            set
            {
                if (selectedInfantryType != value)
                {
                    if (placementMode && (selectedInfantryType != null))
                    {
                        mapPanel.Invalidate(map, navigationWidget.MouseCell);
                    }

                    selectedInfantryType = value;
                    infantryTypesBox.SelectedValue = selectedInfantryType;

                    if (placementMode && (selectedInfantryType != null))
                    {
                        mapPanel.Invalidate(map, navigationWidget.MouseCell);
                    }

                    mockInfantry.Type = selectedInfantryType;

                    RefreshPreviewPanel();
                }
            }
        }

        public InfantryTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, TypeListBox infantryTypesBox, MapPanel infantryTypeMapPanel, ObjectProperties objectProperties, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> url)
            : base(mapPanel, layers, statusLbl, plugin, url)
        {
            previewMap = map;
            List<InfantryType> infTypes = plugin.Map.InfantryTypes.OrderBy(t => t.ID).ToList();
            InfantryType infType = infTypes.First();
            mockInfantry = new Infantry(null)
            {
                Type = infType,
                House = map.Houses.First().Type,
                Strength = 256,
                Direction = map.UnitDirectionTypes.Where(d => d.Equals(FacingType.South)).First(),
                Mission = map.GetDefaultMission(infType)
            };
            mockInfantry.PropertyChanged += MockInfantry_PropertyChanged;
            this.infantryTypesBox = infantryTypesBox;
            this.infantryTypesBox.Types = infTypes;
            this.infantryTypesBox.SelectedIndexChanged += InfantryTypeComboBox_SelectedIndexChanged;
            this.infantryTypeMapPanel = infantryTypeMapPanel;
            this.infantryTypeMapPanel.BackColor = Color.White;
            this.infantryTypeMapPanel.MaxZoom = 1;
            this.infantryTypeMapPanel.SmoothScale = Globals.PreviewSmoothScale;
            this.objectProperties = objectProperties;
            this.objectProperties.Object = mockInfantry;
            SelectedInfantryType = mockInfantry.Type;
        }

        protected override void UpdateExpansionUnits()
        {
            int selectedIndex = infantryTypesBox.SelectedIndex;
            InfantryType selected = infantryTypesBox.SelectedValue as InfantryType;
            this.infantryTypesBox.SelectedIndexChanged -= InfantryTypeComboBox_SelectedIndexChanged;
            List<InfantryType> updatedTypes = plugin.Map.InfantryTypes.OrderBy(t => t.ID).ToList();
            if (!updatedTypes.Contains(selected))
            {
                // Find nearest existing.
                selected = null;
                List<InfantryType> oldTypes = this.infantryTypesBox.Types.Cast<InfantryType>().ToList();
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
            this.infantryTypesBox.Types = updatedTypes;
            this.infantryTypesBox.SelectedIndexChanged += InfantryTypeComboBox_SelectedIndexChanged;
            infantryTypesBox.SelectedValue = selected;
        }

        private void MapPanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys != Keys.None || e.Button != MouseButtons.Left)
            {
                return;
            }
            if (map.Metrics.GetCell(navigationWidget.MouseCell, out int cell))
            {
                if (map.Technos[cell] is InfantryGroup infantryGroup)
                {
                    var i = InfantryGroup.ClosestStoppingTypes(navigationWidget.MouseSubPixel).Cast<int>().First();
                    if (infantryGroup.Infantry[i] is Infantry infantry)
                    {
                        selectedInfantry = null;
                        selectedInfantryLocation = null;
                        selectedInfantryStop = -1;
                        startedDragging = false;
                        mapPanel.Invalidate();
                        Infantry preEdit = infantry.Clone();
                        selectedObjectProperties?.Close();
                        selectedObjectProperties = new ObjectPropertiesPopup(objectProperties.Plugin, infantry);
                        selectedObjectProperties.Closed += (cs, ce) =>
                        {
                            selectedObjectProperties = null;
                            navigationWidget.Refresh();
                            AddPropertiesUndoRedo(infantry, preEdit);
                        };
                        infantry.PropertyChanged += SelectedInfantry_PropertyChanged;
                        selectedObjectProperties.Show(mapPanel, mapPanel.PointToClient(Control.MousePosition));
                        UpdateStatus();
                    }
                }
            }
        }

        private void AddPropertiesUndoRedo(Infantry infantry, Infantry preEdit)
        {
            // infantry = infantry in its final edited form. Clone for preservation
            Infantry redoInf = infantry.Clone();
            Infantry undoInf = preEdit;
            if (redoInf.DataEquals(undoInf))
            {
                return;
            }
            bool origDirtyState = plugin.Dirty;
            plugin.Dirty = true;
            void undoAction(UndoRedoEventArgs ev)
            {
                infantry.CloneDataFrom(undoInf);
                if (infantry.Trigger == null || (!Trigger.IsEmpty(infantry.Trigger)
                    && !ev.Map.FilterUnitTriggers().Any(tr => tr.Name.Equals(infantry.Trigger, StringComparison.InvariantCultureIgnoreCase))))
                {
                    infantry.Trigger = Trigger.None;
                }
                ev.MapPanel.Invalidate(ev.Map, infantry.InfantryGroup);
                if (ev.Plugin != null)
                {
                    ev.Plugin.Dirty = origDirtyState;
                }
            }
            void redoAction(UndoRedoEventArgs ev)
            {
                infantry.CloneDataFrom(redoInf);
                if (infantry.Trigger == null || (!Trigger.IsEmpty(infantry.Trigger)
                    && !ev.Map.FilterUnitTriggers().Any(tr => tr.Name.Equals(infantry.Trigger, StringComparison.InvariantCultureIgnoreCase))))
                {
                    infantry.Trigger = Trigger.None;
                }
                ev.MapPanel.Invalidate(ev.Map, infantry.InfantryGroup);
                if (ev.Plugin != null)
                {
                    ev.Plugin.Dirty = true;
                }
            }
            url.Track(undoAction, redoAction);
        }

        private void MockInfantry_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshPreviewPanel();
        }

        private void SelectedInfantry_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            mapPanel.Invalidate(map, (sender as Infantry).InfantryGroup);
        }

        private void InfantryTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedInfantryType = infantryTypesBox.SelectedValue as InfantryType;
            mockInfantry.Mission = map.GetDefaultMission(SelectedInfantryType, mockInfantry.Mission);
        }

        private void InfantryTool_KeyDown(object sender, KeyEventArgs e)
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

        private void InfantryTool_KeyUp(object sender, KeyEventArgs e)
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
            if (placementMode)
            {
                mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(navigationWidget.MouseCell, new Size(1, 1)), 1, 1));
            }
            else if (selectedInfantry != null)
            {
                Point curCell = navigationWidget.MouseCell;
                Point oldLocation = map.Technos[selectedInfantry.InfantryGroup].Value;
                if (!startedDragging && selectedInfantryLocation.HasValue && selectedInfantryLocation.Value != curCell)
                {
                    startedDragging = true;
                }
                int oldStop = selectedInfantry.InfantryGroup.GetLocation(selectedInfantry);
                InfantryGroup infantryGroup = null;
                var techno = map.Technos[curCell];
                if (techno == null)
                {
                    infantryGroup = new InfantryGroup();
                    map.Technos.Add(curCell, infantryGroup);
                }
                else if (techno is InfantryGroup)
                {
                    infantryGroup = techno as InfantryGroup;
                }
                if (infantryGroup != null)
                {
                    foreach (var i in InfantryGroup.ClosestStoppingTypes(navigationWidget.MouseSubPixel).Cast<int>())
                    {
                        if (infantryGroup.Infantry[i] == null)
                        {
                            selectedInfantry.InfantryGroup.Infantry[oldStop] = null;
                            infantryGroup.Infantry[i] = selectedInfantry;

                            if (infantryGroup != selectedInfantry.InfantryGroup)
                            {
                                mapPanel.Invalidate(map, selectedInfantry.InfantryGroup);
                                if (selectedInfantry.InfantryGroup.Infantry.All(x => x == null))
                                {
                                    map.Technos.Remove(selectedInfantry.InfantryGroup);
                                }
                            }
                            selectedInfantry.InfantryGroup = infantryGroup;
                            mapPanel.Invalidate(map, infantryGroup);
                        }
                        if (infantryGroup == selectedInfantry.InfantryGroup)
                        {
                            if (!startedDragging && selectedInfantryStop != i)
                            {
                                startedDragging = true;
                            }
                            break;
                        }
                    }
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                PickInfantry(navigationWidget.MouseCell);
            }
        }

        private void MapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (placementMode)
            {
                if (e.Button == MouseButtons.Left)
                {
                    AddInfantry(navigationWidget.MouseCell);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    RemoveInfantry(navigationWidget.MouseCell);
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                SelectInfantry(navigationWidget.MouseCell);
            }
            else if (e.Button == MouseButtons.Right)
            {
                PickInfantry(navigationWidget.MouseCell);
            }
        }

        private void MapPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (selectedInfantry != null && selectedInfantryLocation.HasValue && selectedInfantryStop != -1)
            {
                AddMoveUndoTracking(selectedInfantry, selectedInfantryLocation.Value, selectedInfantryStop);
                selectedInfantry = null;
                selectedInfantryLocation = null;
                selectedInfantryStop = -1;
                startedDragging = false;
                mapPanel.Invalidate();
                UpdateStatus();
            }
        }

        private void AddMoveUndoTracking(Infantry toMove, Point startLocation, int startStop)
        {
            Point? finalLocation = map.Technos[toMove.InfantryGroup].Value;
            int finalStop = selectedInfantry.InfantryGroup.GetLocation(selectedInfantry);
            if (!finalLocation.HasValue || finalStop == -1 || (finalLocation.Value == startLocation && finalStop == startStop))
            {
                return;
            }
            bool origDirtyState = plugin.Dirty;
            plugin.Dirty = true;
            Point endLocation = finalLocation.Value;
            void undoAction(UndoRedoEventArgs ev)
            {
                InfantryGroup startGroup = ev.Map.Technos[startLocation] as InfantryGroup;
                InfantryGroup finalGroup = ev.Map.Technos[endLocation] as InfantryGroup;
                if (startGroup == finalGroup)
                {
                    ev.MapPanel.Invalidate(ev.Map, startGroup);
                    startGroup.Infantry[startStop] = toMove;
                    startGroup.Infantry[finalStop] = null;
                    toMove.InfantryGroup = startGroup;
                }
                else
                {
                    if (startGroup == null)
                    {
                        startGroup = new InfantryGroup();
                        ev.Map.Technos.Add(startLocation, startGroup);
                    }
                    startGroup.Infantry[startStop] = toMove;
                    toMove.InfantryGroup = startGroup;
                    ev.MapPanel.Invalidate(ev.Map, startGroup);
                    finalGroup.Infantry[finalStop] = null;
                    ev.MapPanel.Invalidate(ev.Map, finalGroup);
                    if (finalGroup.Infantry.All(x => x == null))
                    {
                        ev.Map.Technos.Remove(finalGroup);
                    }
                }
                if (ev.Plugin != null)
                {
                    ev.Plugin.Dirty = origDirtyState;
                }
            }
            void redoAction(UndoRedoEventArgs ev)
            {
                InfantryGroup startGroup = ev.Map.Technos[startLocation] as InfantryGroup;
                InfantryGroup finalGroup = ev.Map.Technos[endLocation] as InfantryGroup;
                if (finalGroup == startGroup)
                {
                    ev.MapPanel.Invalidate(ev.Map, finalGroup);
                    finalGroup.Infantry[startStop] = null;
                    finalGroup.Infantry[finalStop] = toMove;
                    toMove.InfantryGroup = finalGroup;
                }
                else
                {
                    if (finalGroup == null)
                    {
                        finalGroup = new InfantryGroup();
                        ev.Map.Technos.Add(endLocation, finalGroup);
                    }
                    finalGroup.Infantry[startStop] = toMove;
                    toMove.InfantryGroup = finalGroup;
                    ev.MapPanel.Invalidate(ev.Map, finalGroup);
                    startGroup.Infantry[finalStop] = null;
                    ev.MapPanel.Invalidate(ev.Map, startGroup);
                    if (startGroup.Infantry.All(x => x == null))
                    {
                        ev.Map.Technos.Remove(startGroup);
                    }
                }
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
                if (SelectedInfantryType != null)
                {
                    mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(e.OldCell, new Size(1, 1)), 1, 1));
                    mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(e.NewCell, new Size(1, 1)), 1, 1));
                }
            }
        }

        private void AddInfantry(Point location)
        {
            if (!map.Metrics.GetCell(location, out int cell))
            {
                return;
            }
            if (SelectedInfantryType == null)
            {
                return;
            }
            selectedInfantry = null;
            selectedInfantryLocation = null;
            selectedInfantryStop = -1;
            startedDragging = false;
            InfantryGroup infantryGroup = null;
            var techno = map.Technos[cell];
            if (techno == null)
            {
                infantryGroup = new InfantryGroup();
                map.Technos.Add(cell, infantryGroup);
            }
            else if (techno is InfantryGroup)
            {
                infantryGroup = techno as InfantryGroup;
            }
            if (infantryGroup != null)
            {
                foreach (var placeStop in InfantryGroup.ClosestStoppingTypes(navigationWidget.MouseSubPixel).Cast<int>())
                {
                    if (infantryGroup.Infantry[placeStop] != null)
                    {
                        continue;
                    }
                    var infantry = mockInfantry.Clone();
                    infantryGroup.Infantry[placeStop] = infantry;
                    infantry.InfantryGroup = infantryGroup;
                    mapPanel.Invalidate(map, infantryGroup);
                    bool origDirtyState = plugin.Dirty;
                    plugin.Dirty = true;
                    void undoAction(UndoRedoEventArgs ev)
                    {
                        InfantryGroup placeGroup = ev.Map.Technos[cell] as InfantryGroup;
                        if (placeGroup != null && placeGroup.Infantry[placeStop] != null)
                        {
                            placeGroup.Infantry[placeStop] = null;
                            mapPanel.Invalidate(map, placeGroup);
                            if (placeGroup.Infantry.All(x => x == null))
                            {
                                ev.Map.Technos.Remove(placeGroup);
                            }
                        }
                        if (ev.Plugin != null)
                        {
                            ev.Plugin.Dirty = origDirtyState;
                        }
                    }
                    void redoAction(UndoRedoEventArgs ev)
                    {
                        ICellOccupier occupier = ev.Map.Technos[cell];
                        InfantryGroup placeGroup = occupier as InfantryGroup;
                        if (occupier == null)
                        {
                            placeGroup = new InfantryGroup();
                            ev.Map.Technos.Add(cell, placeGroup);
                        }
                        if (placeGroup != null)
                        {
                            ev.MapPanel.Invalidate(ev.Map, placeGroup);
                            placeGroup.Infantry[placeStop] = infantry;
                            infantry.InfantryGroup = placeGroup;
                        }
                        if (ev.Plugin != null)
                        {
                            ev.Plugin.Dirty = true;
                        }
                    }
                    url.Track(undoAction, redoAction);
                    break;
                }
            }
        }

        private void RemoveInfantry(Point location)
        {
            if (!map.Metrics.GetCell(location, out int cell))
            {
                return;
            }
            if (!(map.Technos[cell] is InfantryGroup infantryGroup))
            {
                return;
            }
            foreach (var placeStop in InfantryGroup.ClosestStoppingTypes(navigationWidget.MouseSubPixel).Cast<int>())
            {
                Infantry delInf = infantryGroup.Infantry[placeStop];
                if (delInf == null)
                {
                    continue;
                }
                infantryGroup.Infantry[placeStop] = null;
                mapPanel.Invalidate(map, infantryGroup);
                if (infantryGroup.Infantry.All(x => x == null))
                {
                    map.Technos.Remove(infantryGroup);
                }
                bool origDirtyState = plugin.Dirty;
                plugin.Dirty = true;
                void undoAction(UndoRedoEventArgs ev)
                {
                    ICellOccupier occupier = ev.Map.Technos[cell];
                    InfantryGroup placeGroup = occupier as InfantryGroup;
                    if (occupier == null)
                    {
                        placeGroup = new InfantryGroup();
                        ev.Map.Technos.Add(cell, placeGroup);
                    }
                    if (placeGroup != null)
                    {
                        ev.MapPanel.Invalidate(ev.Map, placeGroup);
                        placeGroup.Infantry[placeStop] = delInf;
                        delInf.InfantryGroup = placeGroup;
                    }
                    if (ev.Plugin != null)
                    {
                        ev.Plugin.Dirty = origDirtyState;
                    }
                }
                void redoAction(UndoRedoEventArgs ev)
                {
                    InfantryGroup placeGroup = ev.Map.Technos[cell] as InfantryGroup;
                    if (placeGroup != null && placeGroup.Infantry[placeStop] != null)
                    {
                        placeGroup.Infantry[placeStop] = null;
                        mapPanel.Invalidate(map, placeGroup);
                        if (placeGroup.Infantry.All(x => x == null))
                        {
                            ev.Map.Technos.Remove(placeGroup);
                        }
                    }
                    if (ev.Plugin != null)
                    {
                        ev.Plugin.Dirty = true;
                    }
                }
                url.Track(undoAction, redoAction);
                break;
            }
        }

        private void CheckSelectShortcuts(KeyEventArgs e)
        {
            int maxVal = infantryTypesBox.Items.Count - 1;
            int curVal = infantryTypesBox.SelectedIndex;
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
                infantryTypesBox.SelectedIndex = newVal;
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
            if (SelectedInfantryType != null)
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
            if (SelectedInfantryType != null)
            {
                mapPanel.Invalidate(map, Rectangle.Inflate(new Rectangle(navigationWidget.MouseCell, new Size(1, 1)), 1, 1));
            }

            UpdateStatus();
        }

        private void PickInfantry(Point location)
        {
            if (map.Metrics.GetCell(location, out int cell))
            {
                if (map.Technos[cell] is InfantryGroup infantryGroup)
                {
                    var i = InfantryGroup.ClosestStoppingTypes(navigationWidget.MouseSubPixel).Cast<int>().First();
                    if (infantryGroup.Infantry[i] is Infantry infantry)
                    {
                        SelectedInfantryType = infantry.Type;
                        mockInfantry.House = infantry.House;
                        mockInfantry.Strength = infantry.Strength;
                        mockInfantry.Direction = infantry.Direction;
                        mockInfantry.Mission = infantry.Mission;
                        mockInfantry.Trigger = infantry.Trigger;
                    }
                }
            }
        }

        private void SelectInfantry(Point location)
        {
            selectedInfantry = null;
            selectedInfantryLocation = null;
            selectedInfantryStop = -1;
            if (map.Metrics.GetCell(location, out int cell))
            {
                if (map.Technos[cell] is InfantryGroup infantryGroup)
                {
                    var i = InfantryGroup.ClosestStoppingTypes(navigationWidget.MouseSubPixel).Cast<int>().First();
                    if (infantryGroup.Infantry[i] is Infantry infantry)
                    {
                        selectedInfantry = infantry;
                        selectedInfantryLocation = location;
                        selectedInfantryStop = i;
                    }
                }
            }
            mapPanel.Invalidate();
            UpdateStatus();
        }

        protected override void RefreshPreviewPanel()
        {
            var oldImage = infantryTypeMapPanel.MapImage;
            if (mockInfantry.Type != null)
            {
                var infantryPreview = new Bitmap(Globals.PreviewTileWidth, Globals.PreviewTileHeight);
                infantryPreview.SetResolution(96, 96);
                using (var g = Graphics.FromImage(infantryPreview))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    MapRenderer.RenderInfantry(Point.Empty, Globals.PreviewTileSize, mockInfantry, InfantryStoppingType.Center).Item2(g);
                }
                infantryTypeMapPanel.MapImage = infantryPreview;
            }
            else
            {
                infantryTypeMapPanel.MapImage = null;
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
                statusLbl.Text = "Left-Click to place infantry, Right-Click to remove infantry";
            }
            else if (selectedInfantry != null)
            {
                statusLbl.Text = "Drag mouse to move infantry";
            }
            else
            {
                statusLbl.Text = "Shift to enter placement mode, Left-Click drag to move infantry, Double-Click to update infantry properties, Right-Click to pick infantry";
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
            var location = navigationWidget.MouseCell;
            if (SelectedInfantryType == null)
            {
                return;
            }
            if (!previewMap.Metrics.GetCell(location, out int cell))
            {
                return;
            }
            InfantryGroup infantryGroup;
            var techno = previewMap.Technos[cell];
            if (techno == null)
            {
                infantryGroup = new InfantryGroup();
                previewMap.Technos.Add(cell, infantryGroup);
            }
            else
            {
                infantryGroup = techno as InfantryGroup;
            }
            if (infantryGroup != null)
            {
                foreach (var i in InfantryGroup.ClosestStoppingTypes(navigationWidget.MouseSubPixel).Cast<int>())
                {
                    if (infantryGroup.Infantry[i] == null)
                    {
                        var infantry = mockInfantry.Clone();
                        infantry.Tint = Color.FromArgb(128, Color.White);
                        infantryGroup.Infantry[i] = infantry;
                        break;
                    }
                }
            }
        }

        protected override void PostRenderMap(Graphics graphics, Rectangle visibleCells)
        {
            base.PostRenderMap(graphics, visibleCells);
            // For bounds, add one more cell to get all borders showing.
            Rectangle boundRenderCells = visibleCells;
            boundRenderCells.Inflate(1, 1);
            boundRenderCells.Intersect(map.Metrics.Bounds);
            MapRenderer.RenderAllBoundsFromPoint(graphics, boundRenderCells, Globals.MapTileSize, previewMap.Technos.OfType<InfantryGroup>());
            this.HandlePaintOutlines(graphics, previewMap, visibleCells, Globals.MapTileSize, Globals.MapTileScale, this.Layers);
            if ((Layers & (MapLayerFlag.Infantry | MapLayerFlag.TechnoTriggers)) == (MapLayerFlag.Infantry | MapLayerFlag.TechnoTriggers))
            {
                MapRenderer.RenderAllTechnoTriggers(graphics, previewMap, visibleCells, Globals.MapTileSize, Layers);
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
            (this.mapPanel as Control).KeyDown += InfantryTool_KeyDown;
            (this.mapPanel as Control).KeyUp += InfantryTool_KeyUp;
            this.navigationWidget.BoundsMouseCellChanged += MouseoverWidget_MouseCellChanged;
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
            (this.mapPanel as Control).KeyDown -= InfantryTool_KeyDown;
            (this.mapPanel as Control).KeyUp -= InfantryTool_KeyUp;
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
                    infantryTypesBox.SelectedIndexChanged -= InfantryTypeComboBox_SelectedIndexChanged;
                    Deactivate();
                }
                disposedValue = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
