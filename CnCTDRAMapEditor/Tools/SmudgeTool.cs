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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Tools
{
    public class SmudgeTool : ViewTool
    {
        /// <summary> Layers that are important to this tool and need to be drawn last in the PostRenderMap process.</summary>
        protected override MapLayerFlag PriorityLayers => MapLayerFlag.None;
        /// <summary>
        /// Layers that are not painted by the PostRenderMap function on ViewTool level because they are handled
        /// at a specific point in the PostRenderMap override by the implementing tool.
        /// </summary>
        protected override MapLayerFlag ManuallyHandledLayers => MapLayerFlag.None;

        private readonly TypeListBox smudgeTypeListBox;
        private readonly MapPanel smudgeTypeMapPanel;
        private readonly SmudgeProperties smudgeProperties;

        private Map previewMap;
        protected override Map RenderMap => previewMap;

        private bool placementMode;

        private readonly Smudge mockSmudge;

        private Smudge selectedSmudge;
        private int selectedSmudgeCell;
        private SmudgePropertiesPopup selectedSmudgeProperties;

        private SmudgeType selectedSmudgeType;
        private SmudgeType SelectedSmudgeType
        {
            get => selectedSmudgeType;
            set
            {
                if (selectedSmudgeType != value)
                {
                    if (placementMode && (selectedSmudgeType != null))
                    {
                        mapPanel.Invalidate(map, navigationWidget.MouseCell);
                    }
                    selectedSmudgeType = value;
                    smudgeTypeListBox.SelectedValue = selectedSmudgeType;
                    if (placementMode && (selectedSmudgeType != null))
                    {
                        mapPanel.Invalidate(map, navigationWidget.MouseCell);
                    }
                    mockSmudge.Icon = 0;
                    mockSmudge.Type = selectedSmudgeType;
                    RefreshMapPanel();
                }
            }
        }

        public SmudgeTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, TypeListBox smudgeTypeListBox, MapPanel smudgeTypeMapPanel, SmudgeProperties smudgeProperties, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> url)
            : base(mapPanel, layers, statusLbl, plugin, url)
        {
            previewMap = map;
            mockSmudge = new Smudge()
            {
                Type = smudgeTypeListBox.Types.First() as SmudgeType,
                Icon = 0
            };
            mockSmudge.PropertyChanged += MockSmudge_PropertyChanged;
            this.smudgeTypeListBox = smudgeTypeListBox;
            this.smudgeTypeListBox.SelectedIndexChanged += SmudgeTypeComboBox_SelectedIndexChanged;
            this.smudgeTypeMapPanel = smudgeTypeMapPanel;
            this.smudgeTypeMapPanel.BackColor = Color.White;
            this.smudgeTypeMapPanel.MaxZoom = 1;
            this.smudgeTypeMapPanel.SmoothScale = Globals.PreviewSmoothScale;
            this.smudgeProperties = smudgeProperties;
            this.smudgeProperties.Smudge = mockSmudge;
            navigationWidget.MouseCellChanged += MouseoverWidget_MouseCellChanged;
            SelectedSmudgeType = smudgeTypeListBox.Types.First() as SmudgeType;
        }

        private void MapPanel_MouseLeave(object sender, EventArgs e)
        {
            ExitPlacementMode();
        }

        private void MapPanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys != Keys.None)
            {
                return;
            }
            if (map.Metrics.GetCell(navigationWidget.MouseCell, out int cell))
            {
                if (map.Smudge[cell] is Smudge smudge && (smudge.Type.Flag & SmudgeTypeFlag.Bib) == 0)
                {
                    selectedSmudge = smudge;
                    selectedSmudgeCell = cell;
                    selectedSmudgeProperties?.Close();
                    selectedSmudgeProperties = new SmudgePropertiesPopup(plugin, smudge);
                    selectedSmudgeProperties.Closed += (cs, ce) =>
                    {
                        navigationWidget.Refresh();
                    };
                    smudge.PropertyChanged += SelectedSmudge_PropertyChanged;
                    selectedSmudgeProperties.Show(mapPanel, mapPanel.PointToClient(Control.MousePosition));
                    UpdateStatus();
                }
            }
        }

        private void MockSmudge_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshMapPanel();
        }

        private void SelectedSmudge_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Smudge smudge = sender as Smudge;
            if (smudge != null && ((smudge.Type.Flag & SmudgeTypeFlag.Bib) == SmudgeTypeFlag.None) && ReferenceEquals(smudge, selectedSmudge))
            {
                mapPanel.Invalidate(map, selectedSmudgeCell);
            }
        }

        private void SmudgeTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedSmudgeType = smudgeTypeListBox.SelectedValue as SmudgeType;
        }

        private void SmudgeTool_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                EnterPlacementMode();
            }
        }

        private void SmudgeTool_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
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
                    AddSmudge(navigationWidget.MouseCell);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    RemoveSmudge(navigationWidget.MouseCell);
                }
            }
            else if ((e.Button == MouseButtons.Left) || (e.Button == MouseButtons.Right))
            {
                PickSmudge(navigationWidget.MouseCell);
            }
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

        private void MouseoverWidget_MouseCellChanged(object sender, MouseCellChangedEventArgs e)
        {
            if (placementMode)
            {
                if (SelectedSmudgeType != null)
                {
                    mapPanel.Invalidate(map, e.OldCell);
                    mapPanel.Invalidate(map, e.NewCell);
                }
            }
        }

        private void AddSmudge(Point location)
        {
            if (map.Smudge[location] == null)
            {
                if (SelectedSmudgeType != null)
                {
                    var smudge = mockSmudge.Clone();
                    map.Smudge[location] = smudge;
                    mapPanel.Invalidate(map, location);
                    void undoAction(UndoRedoEventArgs e)
                    {
                        e.MapPanel.Invalidate(e.Map, location);
                        e.Map.Smudge[location] = null;
                    }
                    void redoAction(UndoRedoEventArgs e)
                    {
                        e.Map.Smudge[location] = smudge;
                        e.MapPanel.Invalidate(e.Map, location);
                    }
                    url.Track(undoAction, redoAction);
                    plugin.Dirty = true;
                }
            }
        }

        private void RemoveSmudge(Point location)
        {
            if ((map.Smudge[location] is Smudge smudge) && ((smudge.Type.Flag & SmudgeTypeFlag.Bib) == SmudgeTypeFlag.None))
            {
                map.Smudge[location] = null;
                mapPanel.Invalidate(map, location);
                void undoAction(UndoRedoEventArgs e)
                {
                    e.Map.Smudge[location] = smudge;
                    e.MapPanel.Invalidate(e.Map, location);
                }
                void redoAction(UndoRedoEventArgs e)
                {
                    e.MapPanel.Invalidate(e.Map, location);
                    e.Map.Smudge[location] = null;
                }
                url.Track(undoAction, redoAction);
                plugin.Dirty = true;
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
            if (SelectedSmudgeType != null)
            {
                mapPanel.Invalidate(map, navigationWidget.MouseCell);
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
            if (SelectedSmudgeType != null)
            {
                mapPanel.Invalidate(map, navigationWidget.MouseCell);
            }
            UpdateStatus();
        }

        private void PickSmudge(Point location)
        {
            if (map.Metrics.GetCell(location, out int cell))
            {
                var smudge = map.Smudge[cell];
                if (smudge != null)
                {
                    // convert building bib back to usable bib
                    if ((smudge.Type.Flag & SmudgeTypeFlag.Bib) != 0)
                    {
                        SmudgeType sm = map.SmudgeTypes.FirstOrDefault(s => (s.Flag & SmudgeTypeFlag.Bib) == 0 && s.Name == smudge.Type.Name);
                        if (sm != null)
                        {
                            SelectedSmudgeType = sm;
                        }
                    }
                    else
                    {
                        SelectedSmudgeType = smudge.Type;
                        mockSmudge.Icon = smudge.Icon;
                    }
                }
            }
        }

        private void RefreshMapPanel()
        {
            var oldImage = smudgeTypeMapPanel.MapImage;
            if (mockSmudge.Type != null)
            {
                var smudgePreview = new Bitmap(Globals.PreviewTileWidth, Globals.PreviewTileWidth);
                using (var g = Graphics.FromImage(smudgePreview))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    MapRenderer.Render(map.Theater, new Point(0, 0), Globals.PreviewTileSize, Globals.PreviewTileScale, mockSmudge).Item2(g);
                }
                smudgeTypeMapPanel.MapImage = smudgePreview;
            }
            else
            {
                smudgeTypeMapPanel.MapImage = null;
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
                statusLbl.Text = "Left-Click to place smudge, Right-Click to remove smudge";
            }
            else
            {
                statusLbl.Text = "Shift to enter placement mode, Left-Click or Right-Click to pick smudge. Double-Click to update smudge properties.";
            }
        }

        protected override void PreRenderMap()
        {
            base.PreRenderMap();
            previewMap = map.Clone();
            if (placementMode)
            {
                var location = navigationWidget.MouseCell;
                if (SelectedSmudgeType != null)
                {
                    if (previewMap.Metrics.GetCell(location, out int cell))
                    {
                        if (previewMap.Smudge[cell] == null)
                        {
                            Smudge mock = mockSmudge.Clone();
                            mock.Tint = Color.FromArgb(128, Color.White);
                            previewMap.Smudge[cell] = mock;
                        }
                    }
                }
            }
        }

        protected override void PostRenderMap(Graphics graphics)
        {
            base.PostRenderMap(graphics);
            using (var smudgePen = new Pen(Color.Green, Math.Max(1, Globals.MapTileSize.Width / 16.0f)))
            {
                foreach (var (cell, smudge) in previewMap.Smudge.Where(x => (x.Value.Type.Flag & SmudgeTypeFlag.Bib) == SmudgeTypeFlag.None))
                {
                    previewMap.Metrics.GetLocation(cell, out Point topLeft);
                    var bounds = new Rectangle(new Point(topLeft.X * Globals.MapTileWidth, topLeft.Y * Globals.MapTileHeight), Globals.MapTileSize);
                    graphics.DrawRectangle(smudgePen, bounds);
                }
            }
        }

        public override void Activate()
        {
            base.Activate();
            this.mapPanel.MouseDown += MapPanel_MouseDown;
            this.mapPanel.MouseMove += MapPanel_MouseMove;
            this.mapPanel.MouseDoubleClick += MapPanel_MouseDoubleClick;
            this.mapPanel.MouseLeave += MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown += SmudgeTool_KeyDown;
            (this.mapPanel as Control).KeyUp += SmudgeTool_KeyUp;
            UpdateStatus();
        }

        public override void Deactivate()
        {
            ExitPlacementMode();
            base.Deactivate();
            this.mapPanel.MouseDown -= MapPanel_MouseDown;
            this.mapPanel.MouseMove -= MapPanel_MouseMove;
            this.mapPanel.MouseDoubleClick -= MapPanel_MouseDoubleClick;
            this.mapPanel.MouseLeave -= MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown -= SmudgeTool_KeyDown;
            (this.mapPanel as Control).KeyUp -= SmudgeTool_KeyUp;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Deactivate();
                    smudgeTypeListBox.SelectedIndexChanged -= SmudgeTypeComboBox_SelectedIndexChanged;
                    navigationWidget.MouseCellChanged -= MouseoverWidget_MouseCellChanged;
                }
                disposedValue = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
