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
    public class SmudgeTool : ViewTool
    {
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

        protected override Boolean InPlacementMode
        {
            get { return placementMode; }
        }

        private readonly Smudge mockSmudge;

        private Smudge selectedSmudge;
        private Point selectedSmudgePoint;

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
                        mapPanel.Invalidate(map, new Rectangle(navigationWidget.MouseCell, selectedSmudgeType.Size));
                    }
                    selectedSmudgeType = value;
                    smudgeTypeListBox.SelectedValue = selectedSmudgeType;
                    if (placementMode && (selectedSmudgeType != null))
                    {
                        mapPanel.Invalidate(map, new Rectangle(navigationWidget.MouseCell, selectedSmudgeType.Size));
                    }
                    mockSmudge.Icon = Math.Min(selectedSmudgeType.Icons - 1, mockSmudge.Icon);
                    mockSmudge.Type = selectedSmudgeType;
                    RefreshPreviewPanel();
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
            SelectedSmudgeType = smudgeTypeListBox.Types.First() as SmudgeType;
        }

        private void MapPanel_MouseLeave(object sender, EventArgs e)
        {
            ExitPlacementMode();
        }

        private void MapPanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys != Keys.None || e.Button != MouseButtons.Left)
            {
                return;
            }
            Point mousePoint = navigationWidget.MouseCell;
            if (map.Metrics.GetCell(navigationWidget.MouseCell, out int cell))
            {
                if (map.Smudge[cell] is Smudge smudge && !smudge.Type.IsAutoBib)
                {
                    selectedSmudge = smudge;
                    selectedSmudgePoint = smudge.GetPlacementOrigin(mousePoint);
                    Smudge preEdit = smudge.Clone();
                    selectedSmudgeProperties?.Close();
                    selectedSmudgeProperties = new SmudgePropertiesPopup(plugin, smudge);
                    selectedSmudgeProperties.Closed += (cs, ce) =>
                    {
                        navigationWidget.Refresh();
                        AddPropertiesUndoRedo(smudge, preEdit);
                    };
                    smudge.PropertyChanged += SelectedSmudge_PropertyChanged;
                    selectedSmudgeProperties.Show(mapPanel, mapPanel.PointToClient(Control.MousePosition));
                    UpdateStatus();
                }
            }
        }

        private void AddPropertiesUndoRedo(Smudge smudge, Smudge preEdit)
        {
            // smudge = smudge in its final edited form. Clone for preservation
            Smudge redoSmudge = smudge.Clone();
            Smudge undoSmudge = preEdit;
            if (redoSmudge.Equals(undoSmudge))
            {
                return;
            }
            bool origDirtyState = plugin.Dirty;
            plugin.Dirty = true;
            void undoAction(UndoRedoEventArgs ev)
            {
                smudge.CloneDataFrom(undoSmudge);
                ev.MapPanel.Invalidate(ev.Map, smudge);
                if (ev.Plugin != null)
                {
                    ev.Plugin.Dirty = origDirtyState;
                }
            }
            void redoAction(UndoRedoEventArgs ev)
            {
                smudge.CloneDataFrom(redoSmudge);
                ev.MapPanel.Invalidate(ev.Map, smudge);
                if (ev.Plugin != null)
                {
                    ev.Plugin.Dirty = true;
                }
            }
            url.Track(undoAction, redoAction);
        }

        private void MockSmudge_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshPreviewPanel();
        }

        private void SelectedSmudge_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Smudge smudge = sender as Smudge;
            if (smudge != null && !smudge.Type.IsAutoBib && ReferenceEquals(smudge, selectedSmudge))
            {
                mapPanel.Invalidate(map, new Rectangle(selectedSmudgePoint, smudge.Type.Size));
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
                SmudgeType selected = SelectedSmudgeType;
                if (selected != null)
                {
                    mapPanel.Invalidate(map, new Rectangle(e.OldCell, selected.Size));
                    mapPanel.Invalidate(map, new Rectangle(e.NewCell, selected.Size));
                }
            }
            else if ((e.MouseButtons == MouseButtons.Left) || (e.MouseButtons == MouseButtons.Right))
            {
                PickSmudge(e.NewCell);
            }
        }

        private void AddSmudge(Point location)
        {
            SmudgeType selected = SelectedSmudgeType;
            if (selected == null)
            {
                return;
            }
            Dictionary<Point, Smudge> undomap = new Dictionary<Point, Smudge>();
            Dictionary<Point, Smudge> redomap = new Dictionary<Point, Smudge>();
            bool multiCell = selected.IsMultiCell;
            int icon = 0;
            Size size = selected.Size;
            Smudge oldSmudge = map.Smudge[location];
            // Find smudges whose 0,0 point will be overwritten by this placement.
            foreach (Point p in FindSmudgesForPoint(location))
            {
                Smudge existingBib = map.Smudge[p];
                if (existingBib != null && existingBib.Type.IsAutoBib)
                {
                    continue;
                }
                undomap[p] = map.Smudge[p];
                redomap[p] = null;
                map.Smudge[p] = null;
            }
            RestoreNearbySmudge(map, undomap.Keys, redomap);
            Point placeLocation = location;
            var basicSmudge = mockSmudge.Clone();
            for (int y = 0; y < size.Height; ++y)
            {
                for (int x = 0; x < size.Width; ++x)
                {
                    placeLocation.X = location.X + x;
                    Smudge existingBib = map.Smudge[placeLocation];
                    if (existingBib != null && existingBib.Type.IsAutoBib)
                    {
                        icon++;
                        continue;
                    }
                    var smudge = basicSmudge.Clone();
                    if (multiCell)
                    {
                        smudge.Icon = icon++;
                    }
                    // Prevent it from overwriting already-cleared smudge
                    if (!undomap.ContainsKey(placeLocation))
                    {
                        undomap[placeLocation] = map.Smudge[placeLocation];
                    }
                    redomap[placeLocation] = smudge;
                    map.Smudge[placeLocation] = smudge;
                }
                placeLocation.Y++;
            }
            mapPanel.Invalidate(map, undomap.Keys);
            mapPanel.Invalidate(map, redomap.Keys);
            bool origDirtyState = plugin.Dirty;
            plugin.Dirty = true;
            void undoAction(UndoRedoEventArgs e)
            {
                e.MapPanel.Invalidate(e.Map, undomap.Keys);
                foreach (Point p in undomap.Keys)
                {
                    e.Map.Smudge[p] = undomap[p];
                }
                if (e.Plugin != null)
                {
                    e.Plugin.Dirty = origDirtyState;
                }
            }
            void redoAction(UndoRedoEventArgs e)
            {
                foreach (Point p in redomap.Keys)
                {
                    e.Map.Smudge[p] = redomap[p];
                }
                e.MapPanel.Invalidate(e.Map, redomap.Keys); if (e.Plugin != null)
                {
                    e.Plugin.Dirty = true;
                }
            }
            url.Track(undoAction, redoAction);
        }

        private void RemoveSmudge(Point location)
        {
            SmudgeType selected = SelectedSmudgeType;
            Rectangle toClear = new Rectangle(location, selected == null ? new Size(1, 1) : selected.Size);
            Dictionary<Point, Smudge> undomap = new Dictionary<Point, Smudge>();
            Dictionary<Point, Smudge> redomap = new Dictionary<Point, Smudge>();
            // Clear points
            RemoveSmudgePoints(toClear, undomap, redomap);
            // Restore nearby bibs
            RestoreNearbySmudge(map, undomap.Keys, redomap);
            mapPanel.Invalidate(map, undomap.Keys);
            mapPanel.Invalidate(map, redomap.Keys);
            bool origDirtyState = plugin.Dirty;
            plugin.Dirty = true;
            // TODO
            void undoAction(UndoRedoEventArgs e)
            {
                foreach (Point p in undomap.Keys)
                {
                    e.Map.Smudge[p] = undomap[p];
                }
                e.MapPanel.Invalidate(e.Map, undomap.Keys);
                if (e.Plugin != null)
                {
                    e.Plugin.Dirty = origDirtyState;
                }
            }
            void redoAction(UndoRedoEventArgs e)
            {
                e.MapPanel.Invalidate(e.Map, redomap.Keys);
                foreach (Point p in redomap.Keys)
                {
                    e.Map.Smudge[p] = redomap[p];
                }
                if (e.Plugin != null)
                {
                    e.Plugin.Dirty = true;
                }
            }
            url.Track(undoAction, redoAction);
        }

        private void RemoveSmudgePoints(Rectangle toClear, Dictionary<Point, Smudge> undomap, Dictionary<Point, Smudge> redomap)
        {
            foreach (Point loc in toClear.Points())
            {
                SmudgeType smudgeType;
                if (!(map.Smudge[loc] is Smudge smudge) || (smudgeType = smudge.Type).IsAutoBib)
                {
                    continue;
                }
                Point baseLocation = smudge.GetPlacementOrigin(loc);
                int curIcon = 0;
                Point removeLocation = baseLocation;
                Size size = smudgeType.Size;
                bool isMultiCell = smudgeType.IsMultiCell;
                for (int y = 0; y < size.Height; ++y)
                {
                    for (int x = 0; x < size.Width; ++x)
                    {
                        removeLocation.X = baseLocation.X + x;
                        var foundSmudge = map.Smudge[removeLocation];
                        if (isMultiCell && foundSmudge != null && (!smudgeType.Equals(foundSmudge.Type) || foundSmudge.Icon != curIcon))
                        {
                            curIcon++;
                            continue;
                        }
                        curIcon++;
                        undomap[removeLocation] = foundSmudge;
                        redomap[removeLocation] = null;
                        map.Smudge[removeLocation] = null;
                    }
                    removeLocation.Y++;
                }
            }
        }

        public static void RestoreNearbySmudge(Map map, IEnumerable<Point> points, Dictionary<Point, Smudge> redomap)
        {
            if (points == null)
            {
                return;
            }
            // Maximum size that can be occupied by multi-cell bibs.
            int maxW = map.SmudgeTypes.Where(sm => sm.IsMultiCell && !sm.IsAutoBib).Max(sm => sm.Size.Width);
            int maxH = map.SmudgeTypes.Where(sm => sm.IsMultiCell && !sm.IsAutoBib).Max(sm => sm.Size.Height);
            foreach (Point loc in points.OrderBy(p => p.X).ThenByDescending(p => p.Y))
            {
                // scan smudges from bottom to top, right to left, to find if any cell from a nearby smudge should occupy this location.
                Rectangle scanRect = new Rectangle(loc.X - maxW + 1, loc.Y - maxH + 1, maxW * 2 - 1, maxH * 2 - 1);
                foreach (Point p in scanRect.Points().Where(p => map.Metrics.Bounds.Contains(p)).OrderByDescending(p => p.X).ThenByDescending(p => p.Y))
                {
                    Smudge toFix = map.Smudge[p];
                    SmudgeType toFixType = toFix?.Type;
                    if (toFixType == null || toFixType.IsAutoBib || !toFixType.IsMultiCell)
                    {
                        continue;
                    }
                    Point fixBase = toFix.GetPlacementOrigin(p);
                    if (new Rectangle(fixBase, toFixType.Size).Contains(loc))
                    {
                        Smudge fixSmudge = toFix.Clone();
                        fixSmudge.Icon = (loc.Y - fixBase.Y) * toFixType.Size.Width + loc.X - fixBase.X;
                        if (redomap != null)
                        {
                            redomap[loc] = fixSmudge;
                        }
                        map.Smudge[loc] = fixSmudge;
                        break;
                    }
                }
            }
        }

        private List<Point> FindSmudgesForPoint(Point loc)
        {
            // Maximum size that can be occupied by multi-cell bibs.
            int maxW = map.SmudgeTypes.Where(sm => sm.IsMultiCell && !sm.IsAutoBib).Max(sm => sm.Size.Width);
            int maxH = map.SmudgeTypes.Where(sm => sm.IsMultiCell && !sm.IsAutoBib).Max(sm => sm.Size.Height);
                // scan smudges from top to bottom to find if any cell from a nearby smudge should occupy this location.
            Rectangle scanRect = new Rectangle(loc.X, loc.Y, maxW, maxH);
            List<Point> found = new List<Point>();
            foreach (Point p in scanRect.Points())
            {
                Smudge toCheck = map.Smudge[p];
                SmudgeType toFindType = toCheck?.Type;
                if (toFindType == null)
                {
                    continue;
                }
                Point checkBase = toCheck.GetPlacementOrigin(p);
                if (loc == checkBase)
                {
                    found.Add(p);
                }
            }
            return found;
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
            SmudgeType selected = SelectedSmudgeType;
            if (selected != null)
            {
                mapPanel.Invalidate(map, new Rectangle(navigationWidget.MouseCell, selected.Size));
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
            SmudgeType selected = SelectedSmudgeType;
            if (selected != null)
            {
                mapPanel.Invalidate(map, new Rectangle(navigationWidget.MouseCell, selected.Size));
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
                    SmudgeType picked = smudge.Type;
                    // convert building bib back to usable bib
                    if (picked.IsAutoBib)
                    {
                        SmudgeType sm = map.SmudgeTypes.FirstOrDefault(s => !s.IsAutoBib && s.Name == picked.Name);
                        if (sm != null)
                        {
                            mockSmudge.Icon = 0;
                            SelectedSmudgeType = sm;
                        }
                    }
                    else
                    {
                        mockSmudge.Icon = 0;
                        SelectedSmudgeType = picked;
                        mockSmudge.Icon = Math.Min(smudge.Type.Icons - 1, picked.IsMultiCell ? 0 : smudge.Icon);
                    }
                }
            }
        }

        protected override void RefreshPreviewPanel()
        {
            var oldImage = smudgeTypeMapPanel.MapImage;
            SmudgeType mockType = mockSmudge?.Type;
            if (mockType != null)
            {
                Size mockSize = mockType.Size;
                CellMetrics mockMetrics = new CellMetrics(mockSize);
                var smudgePreview = new Bitmap(Globals.PreviewTileWidth * mockSize.Width, Globals.PreviewTileWidth * mockSize.Height);
                using (var g = Graphics.FromImage(smudgePreview))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    // Needs to be done here manually since "one" multi-cell smudge is really just a collection of smudge cells.
                    List<(int, Smudge)> smudgeList = new List<(int, Smudge)>();
                    int icons = mockSize.Width * mockSize.Height;
                    for (int i = 0; i < icons; ++i)
                    {
                        Smudge smudge = new Smudge()
                        {
                            Type = mockType,
                            Icon = mockType.IsMultiCell ? i : mockSmudge.Icon
                        };
                        smudgeList.Add((i, smudge));
                        mockMetrics.GetLocation(i, out Point p);
                        var render = MapRenderer.Render(map.Theater, p, Globals.PreviewTileSize, Globals.PreviewTileScale, smudge);
                        if (!render.Item1.IsEmpty)
                        {
                            render.Item2(g);
                        }
                    }
                    MapRenderer.RenderAllBoundsFromCell(g, Globals.PreviewTileSize, smudgeList, mockMetrics);
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
            smudgeTypeMapPanel.Invalidate();
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
            previewMap = map.Clone(true);
            if (!placementMode)
            {
                return;
            }
            navigationWidget.MouseoverSize = Size.Empty;
            var location = navigationWidget.MouseCell;
            SmudgeType selected = SelectedSmudgeType;
            if (selected == null || !previewMap.Metrics.GetCell(location, out _))
            {
                return;
            }
            int icon = 0;
            Size size = selected.Size;
            bool multiCell = selected.IsMultiCell;
            Point placeLocation = location;
            var basicSmudge = mockSmudge.Clone();
            for (int y = 0; y < size.Height; ++y)
            {
                for (int x = 0; x < size.Width; ++x)
                {
                    placeLocation.X = location.X + x;
                    var mock = basicSmudge.Clone();
                    mock.Tint = Color.FromArgb(128, Color.White);
                    if (multiCell)
                    {
                        mock.Icon = icon++;
                    }
                    Smudge oldSmudge = previewMap.Smudge[placeLocation];
                    if (oldSmudge != null && oldSmudge.Type.IsAutoBib)
                    {
                        if (x == 0 && y == 0)
                        {
                            navigationWidget.MouseoverSize = new Size(1, 1);
                        }
                    }
                    else
                    {
                        previewMap.Smudge[placeLocation] = mock;
                    }
                }
                placeLocation.Y++;
            }
        }

        protected override void PostRenderMap(Graphics graphics)
        {
            base.PostRenderMap(graphics);
            MapRenderer.RenderAllBoundsFromCell(graphics, Globals.MapTileSize, previewMap.Smudge.Where(x => !x.Value.Type.IsAutoBib), previewMap.Metrics);
        }

        public override void Activate()
        {
            base.Activate();
            this.Deactivate(true);
            this.mapPanel.MouseDown += MapPanel_MouseDown;
            this.mapPanel.MouseMove += MapPanel_MouseMove;
            this.mapPanel.MouseDoubleClick += MapPanel_MouseDoubleClick;
            this.mapPanel.MouseLeave += MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown += SmudgeTool_KeyDown;
            (this.mapPanel as Control).KeyUp += SmudgeTool_KeyUp;
            navigationWidget.MouseCellChanged += MouseoverWidget_MouseCellChanged;
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
                ExitPlacementMode();
                base.Deactivate();
            }
            this.mapPanel.MouseDown -= MapPanel_MouseDown;
            this.mapPanel.MouseMove -= MapPanel_MouseMove;
            this.mapPanel.MouseDoubleClick -= MapPanel_MouseDoubleClick;
            this.mapPanel.MouseLeave -= MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown -= SmudgeTool_KeyDown;
            (this.mapPanel as Control).KeyUp -= SmudgeTool_KeyUp;
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
                    Deactivate();
                    smudgeTypeListBox.SelectedIndexChanged -= SmudgeTypeComboBox_SelectedIndexChanged;
                }
                disposedValue = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
