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
using MobiusEditor.Widgets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MobiusEditor.Tools
{
    public class TemplateTool : ViewTool
    {
        private const int DesignResWidth = 3840;
        private const int DesignTextureWidth = 256;

        private static readonly Regex CategoryRegex = new Regex(@"^([a-z]*)", RegexOptions.Compiled);

        private readonly ListView templateTypeListView;
        private readonly MapPanel templateTypeMapPanel;
        private readonly ToolTip mouseTooltip;

        private readonly Dictionary<int, Template> undoTemplates = new Dictionary<int, Template>();
        private readonly Dictionary<int, Template> redoTemplates = new Dictionary<int, Template>();

        private Map previewMap;
        protected override Map RenderMap => previewMap;

        private bool placementMode;

        private bool boundsMode;
        private Rectangle dragBounds;
        private int dragEdge = -1;

        private TemplateType selectedTemplateType;
        private TemplateType SelectedTemplateType
        {
            get => selectedTemplateType;
            set
            {
                if (selectedTemplateType != value)
                {
                    if (placementMode && (selectedTemplateType != null))
                    {
                        for (var y = 0; y < selectedTemplateType.IconHeight; ++y)
                        {
                            for (var x = 0; x < selectedTemplateType.IconWidth; ++x)
                            {
                                mapPanel.Invalidate(map, new Point(navigationWidget.MouseCell.X + x, navigationWidget.MouseCell.Y + y));
                            }
                        }
                    }
                    selectedTemplateType = value;
                    templateTypeListView.BeginUpdate();
                    templateTypeListView.SelectedIndexChanged -= TemplateTypeListView_SelectedIndexChanged;
                    foreach (ListViewItem item in templateTypeListView.Items)
                    {
                        item.Selected = item.Tag == selectedTemplateType;
                    }
                    if (templateTypeListView.SelectedIndices.Count > 0)
                    {
                        templateTypeListView.EnsureVisible(templateTypeListView.SelectedIndices[0]);
                    }
                    templateTypeListView.SelectedIndexChanged += TemplateTypeListView_SelectedIndexChanged;
                    templateTypeListView.EndUpdate();

                    if (placementMode && (selectedTemplateType != null))
                    {
                        for (var y = 0; y < selectedTemplateType.IconHeight; ++y)
                        {
                            for (var x = 0; x < selectedTemplateType.IconWidth; ++x)
                            {
                                mapPanel.Invalidate(map, new Point(navigationWidget.MouseCell.X + x, navigationWidget.MouseCell.Y + y));
                            }
                        }
                    }
                    RefreshMapPanel();
                }
            }
        }

        private Point? selectedIcon;
        private Point? SelectedIcon
        {
            get => selectedIcon;
            set
            {
                if (selectedIcon != value)
                {
                    selectedIcon = value;
                    templateTypeMapPanel.Invalidate();
                    if (placementMode && (SelectedTemplateType != null))
                    {
                        for (var y = 0; y < SelectedTemplateType.IconHeight; ++y)
                        {
                            for (var x = 0; x < SelectedTemplateType.IconWidth; ++x)
                            {
                                mapPanel.Invalidate(map, new Point(navigationWidget.MouseCell.X + x, navigationWidget.MouseCell.Y + y));
                            }
                        }
                    }
                }
            }
        }

        private NavigationWidget templateTypeNavigationWidget;

        public TemplateTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, ListView templateTypeListView, MapPanel templateTypeMapPanel, ToolTip mouseTooltip, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> url)
            : base(mapPanel, layers, statusLbl, plugin, url)
        {
            previewMap = map;
            manuallyHandledLayers = MapLayerFlag.Boundaries;
            this.templateTypeListView = templateTypeListView;
            this.templateTypeListView.SelectedIndexChanged -= TemplateTypeListView_SelectedIndexChanged;
            string templateCategory(TemplateType template)
            {
                var m = CategoryRegex.Match(template.Name);
                return m.Success ? m.Groups[1].Value : string.Empty;
            }
            var templateTypes = plugin.Map.TemplateTypes
                .Where(t =>
                    (t.Thumbnail != null) &&
                    t.Theaters.Contains(plugin.Map.Theater) &&
                    ((t.Flag & TemplateTypeFlag.Clear) == TemplateTypeFlag.None))
                .GroupBy(t => templateCategory(t)).OrderBy(g => g.Key);
            var templateTypeImages = templateTypes.SelectMany(g => g).Select(t => t.Thumbnail);
            var clear = plugin.Map.TemplateTypes.Where(t => (t.Flag & TemplateTypeFlag.Clear) == TemplateTypeFlag.Clear).FirstOrDefault();
            Screen screen = Screen.FromHandle(mapPanel.Handle) ?? Screen.PrimaryScreen;
            int maxSize = Properties.Settings.Default.MaxMapTileTextureSize;
            if (maxSize == 0)
            {
                double ratio = DesignResWidth / (double)screen.Bounds.Width;
                maxSize = (int)((DesignTextureWidth / ratio) * Properties.Settings.Default.TemplateToolTextureSizeMultiplier);
            }
            var maxWidth = Math.Min(templateTypeImages.Max(t => t.Width), maxSize);
            var maxHeight = Math.Min(templateTypeImages.Max(t => t.Height), maxSize);
            var imageList = new ImageList();
            imageList.Images.Add(clear.Thumbnail);
            imageList.Images.AddRange(templateTypeImages.ToArray());
            imageList.ImageSize = new Size(maxWidth, maxHeight);
            imageList.ColorDepth = ColorDepth.Depth24Bit;
            this.templateTypeListView.BeginUpdate();
            this.templateTypeListView.LargeImageList = imageList;
            // Fixed constantly growing items list.
            if (this.templateTypeListView.Groups.Count > 0)
                this.templateTypeListView.Groups.Clear();
            if (this.templateTypeListView.Items.Count > 0)
                this.templateTypeListView.Items.Clear();
            var imageIndex = 0;
            var group = new ListViewGroup(clear.DisplayName);
            this.templateTypeListView.Groups.Add(group);
            var item = new ListViewItem(clear.DisplayName, imageIndex++)
            {
                Group = group,
                Tag = clear
            };
            this.templateTypeListView.Items.Add(item);
            foreach (var templateTypeGroup in templateTypes)
            {
                group = new ListViewGroup(templateTypeGroup.Key);
                this.templateTypeListView.Groups.Add(group);
                foreach (var templateType in templateTypeGroup)
                {
                    item = new ListViewItem(templateType.DisplayName, imageIndex++)
                    {
                        Group = group,
                        Tag = templateType
                    };
                    this.templateTypeListView.Items.Add(item);
                }
            }
            this.templateTypeListView.EndUpdate();
            this.templateTypeListView.SelectedIndexChanged += TemplateTypeListView_SelectedIndexChanged;
            this.templateTypeMapPanel = templateTypeMapPanel;
            this.templateTypeMapPanel.MouseDown += TemplateTypeMapPanel_MouseDown;
            this.templateTypeMapPanel.PostRender += TemplateTypeMapPanel_PostRender;
            this.templateTypeMapPanel.BackColor = Color.Black;
            this.templateTypeMapPanel.MaxZoom = 1;
            this.templateTypeMapPanel.SmoothScale = Globals.PreviewSmoothScale;
            this.mouseTooltip = mouseTooltip;
            SelectedTemplateType = templateTypes.First().First();
        }

        private void Url_Redone(object sender, EventArgs e)
        {
            if (boundsMode && (map.Bounds != dragBounds))
            {
                dragBounds = map.Bounds;
                dragEdge = -1;
                UpdateTooltip();
                mapPanel.Invalidate();
            }
        }

        private void Url_Undone(object sender, EventArgs e)
        {
            if (boundsMode && (map.Bounds != dragBounds))
            {
                dragBounds = map.Bounds;
                dragEdge = -1;
                UpdateTooltip();
                mapPanel.Invalidate();
            }
        }

        private void TemplateTypeMapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if ((SelectedTemplateType == null) || ((SelectedTemplateType.IconWidth * SelectedTemplateType.IconHeight) == 1))
            {
                SelectedIcon = null;
            }
            else
            {
                if (e.Button == MouseButtons.Left)
                {
                    var templateTypeMouseCell = templateTypeNavigationWidget.MouseCell;
                    if ((templateTypeMouseCell.X >= 0) && (templateTypeMouseCell.X < SelectedTemplateType.IconWidth))
                    {
                        if ((templateTypeMouseCell.Y >= 0) && (templateTypeMouseCell.Y < SelectedTemplateType.IconHeight))
                        {
                            if (SelectedTemplateType.IconMask[templateTypeMouseCell.X, templateTypeMouseCell.Y])
                            {
                                SelectedIcon = templateTypeMouseCell;
                            }
                        }
                    }
                }
                else if (e.Button == MouseButtons.Right)
                {
                    SelectedIcon = null;
                }
            }
        }

        private void TemplateTypeMapPanel_PostRender(object sender, RenderEventArgs e)
        {
            e.Graphics.Transform = new Matrix();
            if (SelectedTemplateType != null)
            {
                if (SelectedIcon.HasValue)
                {
                    int panelWidth = templateTypeMapPanel.ClientSize.Width;
                    int panelHeight = templateTypeMapPanel.ClientSize.Height;
                    int iconWidth = SelectedTemplateType.IconWidth;
                    int iconHeight = SelectedTemplateType.IconHeight;
                    int maxIconRect = Math.Max(iconWidth, iconHeight);
                    int scaleX = panelWidth / maxIconRect;
                    int scaleY = panelHeight / maxIconRect;
                    int scale = Math.Min(scaleX, scaleY);
                    int leftoverX = panelWidth - (scale * maxIconRect);
                    int leftoverY = panelHeight - (scale * maxIconRect);
                    int padX = leftoverX / 2;
                    int padY = leftoverY / 2;
                    using (var selectedIconPen = new Pen(Color.Yellow, Math.Max(1, scale/16))) {
                        var cellSize = new Size(scale, scale);
                        var rect = new Rectangle(new Point(padX + SelectedIcon.Value.X * cellSize.Width, padY + SelectedIcon.Value.Y * cellSize.Height), cellSize);
                        e.Graphics.DrawRectangle(selectedIconPen, rect);
                    }
                }
                var sizeStringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                var text = string.Format("{0} ({1}x{2})", SelectedTemplateType.DisplayName, SelectedTemplateType.IconWidth, SelectedTemplateType.IconHeight);
                var textSize = e.Graphics.MeasureString(text, SystemFonts.CaptionFont) + new SizeF(6.0f, 6.0f);
                var textBounds = new RectangleF(new PointF(0, 0), textSize);
                using (var sizeBackgroundBrush = new SolidBrush(Color.FromArgb(128, Color.Black)))
                using (var sizeTextBrush = new SolidBrush(Color.White))
                {
                    e.Graphics.FillRectangle(sizeBackgroundBrush, textBounds);
                    e.Graphics.DrawString(text, SystemFonts.CaptionFont, sizeTextBrush, textBounds, sizeStringFormat);
                }
            }
        }

        private void TemplateTool_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                if (boundsMode)
                    ExitAllModes();
                else
                    EnterPlacementMode();
            }
            else if (e.KeyCode == Keys.ControlKey)
            {
                if (placementMode)
                    ExitAllModes();
                else
                    EnterBoundsMode();
            }
            else
            {
                ExitAllModes();
            }
        }

        private void TemplateTool_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.ShiftKey) || (e.KeyCode == Keys.ControlKey))
            {
                ExitAllModes();
            }
        }

        private void TemplateTypeListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedTemplateType = templateTypeListView == null ? null : (templateTypeListView.SelectedItems.Count > 0) ? (templateTypeListView.SelectedItems[0].Tag as TemplateType) : null;
            SelectedIcon = null;
        }

        private void MapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (boundsMode)
            {
                dragEdge = DetectDragEdge();

                UpdateStatus();
            }
            else if (placementMode)
            {
                HandlePlace(e.Button);
            }
            else if ((e.Button == MouseButtons.Left) || (e.Button == MouseButtons.Right))
            {
                PickTemplate(navigationWidget.MouseCell, e.Button == MouseButtons.Left);
            }
        }

        private void HandlePlace(MouseButtons button)
        {
            if (button == MouseButtons.Left)
            {
                if ((selectedTemplateType.Flag & TemplateTypeFlag.Clear) != 0)
                    RemoveTemplate(navigationWidget.MouseCell);
                else
                    SetTemplate(navigationWidget.MouseCell);
            }
            else if (button == MouseButtons.Right)
            {
                RemoveTemplate(navigationWidget.MouseCell);
            }
        }

        private void MapPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (boundsMode)
            {
                if (dragBounds != map.Bounds)
                {
                    var oldBounds = map.Bounds;
                    void undoAction(UndoRedoEventArgs ure)
                    {
                        ure.Map.Bounds = oldBounds;
                        ure.MapPanel.Invalidate();
                    }
                    void redoAction(UndoRedoEventArgs ure)
                    {
                        ure.Map.Bounds = dragBounds;
                        ure.MapPanel.Invalidate();
                    }
                    map.Bounds = dragBounds;
                    url.Track(undoAction, redoAction);
                    mapPanel.Invalidate();
                }
                dragEdge = -1;
                UpdateStatus();
            }
            else
            {
                if ((undoTemplates.Count > 0) || (redoTemplates.Count > 0))
                {
                    CommitChange();
                }
            }
        }

        private void MapPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!placementMode && (Control.ModifierKeys == Keys.Shift))
            {
                EnterPlacementMode();
            }
            else if (!boundsMode && (Control.ModifierKeys == Keys.Control))
            {
                EnterBoundsMode();
            }
            else if ((placementMode || boundsMode) && (Control.ModifierKeys == Keys.None))
            {
                ExitAllModes();
            }
            var cursor = Cursors.Default;
            if (boundsMode)
            {
                switch ((dragEdge >= 0) ? dragEdge : DetectDragEdge())
                {
                    case 0:
                    case 4:
                        cursor = Cursors.SizeNS;
                        break;
                    case 2:
                    case 6:
                        cursor = Cursors.SizeWE;
                        break;
                    case 1:
                    case 5:
                        cursor = Cursors.SizeNESW;
                        break;
                    case 3:
                    case 7:
                        cursor = Cursors.SizeNWSE;
                        break;
                }
            }
            Cursor.Current = cursor;
            UpdateTooltip();
        }

        private void MouseoverWidget_MouseCellChanged(object sender, MouseCellChangedEventArgs e)
        {
            if (dragEdge >= 0)
            {
                var endDrag = navigationWidget.MouseCell;
                map.Metrics.Clip(ref endDrag, new Size(1, 1), Size.Empty);
                switch (dragEdge)
                {
                    case 0:
                    case 1:
                    case 7:
                        if (endDrag.Y < dragBounds.Bottom)
                        {
                            dragBounds.Height = dragBounds.Bottom - endDrag.Y;
                            dragBounds.Y = endDrag.Y;
                        }
                        break;
                }
                switch (dragEdge)
                {
                    case 5:
                    case 6:
                    case 7:
                        if (endDrag.X < dragBounds.Right)
                        {
                            dragBounds.Width = dragBounds.Right - endDrag.X;
                            dragBounds.X = endDrag.X;
                        }
                        break;
                }
                switch (dragEdge)
                {
                    case 3:
                    case 4:
                    case 5:
                        if (endDrag.Y > dragBounds.Top)
                        {
                            dragBounds.Height = endDrag.Y - dragBounds.Top;
                        }
                        break;
                }
                switch (dragEdge)
                {
                    case 1:
                    case 2:
                    case 3:
                        if (endDrag.X > dragBounds.Left)
                        {
                            dragBounds.Width = endDrag.X - dragBounds.Left;
                        }
                        break;
                }
                mapPanel.Invalidate(map);
            }
            else if (placementMode)
            {
                HandlePlace(Control.MouseButtons);
                if (SelectedTemplateType != null)
                {
                    foreach (var location in new Point[] { e.OldCell, e.NewCell })
                    {
                        if (SelectedIcon.HasValue)
                        {
                            mapPanel.Invalidate(map, new Point(location.X, location.Y));
                        }
                        else
                        {
                            for (var y = 0; y < SelectedTemplateType.IconHeight; ++y)
                            {
                                for (var x = 0; x < SelectedTemplateType.IconWidth; ++x)
                                {
                                    if (!SelectedTemplateType.IconMask[x, y])
                                    {
                                        continue;
                                    }
                                    mapPanel.Invalidate(map, new Point(location.X + x, location.Y + y));
                                }
                            }
                        }
                    }
                }
            }
            else if((Control.MouseButtons == MouseButtons.Left) || (Control.MouseButtons == MouseButtons.Right))
            {
                PickTemplate(navigationWidget.MouseCell, Control.MouseButtons == MouseButtons.Left);
            }
        }

        private void RefreshMapPanel()
        {
            if (templateTypeNavigationWidget != null)
            {
                templateTypeNavigationWidget.Dispose();
                templateTypeNavigationWidget = null;
            }
            TemplateType selected = SelectedTemplateType;
            if (selected != null)
            {
                templateTypeMapPanel.MapImage = selected.Thumbnail;
                var templateTypeMetrics = new CellMetrics(selected.IconWidth, selected.IconHeight);
                templateTypeNavigationWidget = new NavigationWidget(templateTypeMapPanel, templateTypeMetrics, Globals.OriginalTileSize);
                templateTypeNavigationWidget.MouseoverSize = Size.Empty;
                templateTypeNavigationWidget.Activate();
            }
            else
            {
                templateTypeMapPanel.MapImage = null;
            }
        }

        private void SetTemplate(Point location)
        {
            if (SelectedTemplateType != null)
            {
                if (SelectedIcon.HasValue)
                {
                    if (map.Metrics.GetCell(location, out int cell))
                    {
                        if (!undoTemplates.ContainsKey(cell))
                        {
                            undoTemplates[cell] = map.Templates[location];
                        }
                        var icon = (SelectedIcon.Value.Y * SelectedTemplateType.IconWidth) + SelectedIcon.Value.X;
                        var template = new Template { Type = SelectedTemplateType, Icon = icon };
                        map.Templates[cell] = template;
                        redoTemplates[cell] = template;
                        mapPanel.Invalidate(map, cell);
                        plugin.Dirty = true;
                    }
                }
                else
                {
                    for (int y = 0, icon = 0; y < SelectedTemplateType.IconHeight; ++y)
                    {
                        for (var x = 0; x < SelectedTemplateType.IconWidth; ++x, ++icon)
                        {
                            if (!SelectedTemplateType.IconMask[x, y])
                            {
                                continue;
                            }
                            var subLocation = new Point(location.X + x, location.Y + y);
                            if (map.Metrics.GetCell(subLocation, out int cell))
                            {
                                if (!undoTemplates.ContainsKey(cell))
                                {
                                    undoTemplates[cell] = map.Templates[subLocation];
                                }
                            }
                        }
                    }
                    for (int y = 0, icon = 0; y < SelectedTemplateType.IconHeight; ++y)
                    {
                        for (var x = 0; x < SelectedTemplateType.IconWidth; ++x, ++icon)
                        {
                            if (!SelectedTemplateType.IconMask[x, y])
                            {
                                continue;
                            }
                            var subLocation = new Point(location.X + x, location.Y + y);
                            if (map.Metrics.GetCell(subLocation, out int cell))
                            {
                                var template = new Template { Type = SelectedTemplateType, Icon = icon };
                                map.Templates[cell] = template;
                                redoTemplates[cell] = template;
                                mapPanel.Invalidate(map, cell);
                                plugin.Dirty = true;
                            }
                        }
                    }
                }
            }
        }

        private void RemoveTemplate(Point location)
        {
            if (SelectedTemplateType != null)
            {
                if (SelectedIcon.HasValue)
                {
                    if (map.Metrics.GetCell(location, out int cell))
                    {
                        if (!undoTemplates.ContainsKey(cell))
                        {
                            undoTemplates[cell] = map.Templates[location];
                        }
                        map.Templates[cell] = null;
                        redoTemplates[cell] = null;
                        mapPanel.Invalidate(map, cell);
                        plugin.Dirty = true;
                    }
                }
                else
                {
                    for (int y = 0, icon = 0; y < SelectedTemplateType.IconHeight; ++y)
                    {
                        for (var x = 0; x < SelectedTemplateType.IconWidth; ++x, ++icon)
                        {
                            if (!SelectedTemplateType.IconMask[x, y])
                            {
                                continue;
                            }
                            var subLocation = new Point(location.X + x, location.Y + y);
                            if (map.Metrics.GetCell(subLocation, out int cell))
                            {
                                if (!undoTemplates.ContainsKey(cell))
                                {
                                    undoTemplates[cell] = map.Templates[subLocation];
                                }
                            }
                        }
                    }
                    for (int y = 0, icon = 0; y < SelectedTemplateType.IconHeight; ++y)
                    {
                        for (var x = 0; x < SelectedTemplateType.IconWidth; ++x, ++icon)
                        {
                            if (!SelectedTemplateType.IconMask[x, y])
                            {
                                continue;
                            }
                            var subLocation = new Point(location.X + x, location.Y + y);
                            if (map.Metrics.GetCell(subLocation, out int cell))
                            {
                                map.Templates[cell] = null;
                                redoTemplates[cell] = null;
                                mapPanel.Invalidate(map, cell);
                                plugin.Dirty = true;
                            }
                        }
                    }
                }
            }
        }

        private void EnterPlacementMode()
        {
            if (placementMode || boundsMode)
            {
                return;
            }
            placementMode = true;
            navigationWidget.MouseoverSize = Size.Empty;
            if (SelectedTemplateType != null)
            {
                for (var y = 0; y < SelectedTemplateType.IconHeight; ++y)
                {
                    for (var x = 0; x < SelectedTemplateType.IconWidth; ++x)
                    {
                        mapPanel.Invalidate(map, new Point(navigationWidget.MouseCell.X + x, navigationWidget.MouseCell.Y + y));
                    }
                }
            }
            UpdateStatus();
        }

        private void EnterBoundsMode()
        {
            if (boundsMode || placementMode)
            {
                return;
            }
            boundsMode = true;
            dragBounds = map.Bounds;
            navigationWidget.MouseoverSize = Size.Empty;
            if (SelectedTemplateType != null)
            {
                for (var y = 0; y < SelectedTemplateType.IconHeight; ++y)
                {
                    for (var x = 0; x < SelectedTemplateType.IconWidth; ++x)
                    {
                        mapPanel.Invalidate(map, new Point(navigationWidget.MouseCell.X + x, navigationWidget.MouseCell.Y + y));
                    }
                }
            }
            UpdateTooltip();
            UpdateStatus();
        }

        private void ExitAllModes()
        {
            if (!placementMode && !boundsMode)
            {
                return;
            }
            boundsMode = false;
            dragEdge = -1;
            dragBounds = Rectangle.Empty;
            placementMode = false;
            navigationWidget.MouseoverSize = new Size(1, 1);
            if (SelectedTemplateType != null)
            {
                for (var y = 0; y < SelectedTemplateType.IconHeight; ++y)
                {
                    for (var x = 0; x < SelectedTemplateType.IconWidth; ++x)
                    {
                        mapPanel.Invalidate(map, new Point(navigationWidget.MouseCell.X + x, navigationWidget.MouseCell.Y + y));
                    }
                }
            }
            UpdateTooltip();
            UpdateStatus();
        }

        private void UpdateTooltip()
        {
            if (boundsMode)
            {
                var tooltip = string.Format("X = {0}\nY = {1}\nWidth = {2}\nHeight = {3}", dragBounds.Left, dragBounds.Top, dragBounds.Width, dragBounds.Height);
                var textSize = TextRenderer.MeasureText(tooltip, SystemFonts.CaptionFont);
                var tooltipSize = new Size(textSize.Width + 6, textSize.Height + 6);

                var tooltipPosition = mapPanel.PointToClient(Control.MousePosition);
                switch (dragEdge)
                {
                    case -1:
                    case 0:
                    case 1:
                    case 7:
                        tooltipPosition.Y -= tooltipSize.Height;
                        break;
                }
                switch (dragEdge)
                {
                    case -1:
                    case 5:
                    case 6:
                    case 7:
                        tooltipPosition.X -= tooltipSize.Width;
                        break;
                }
                var screenPosition = mapPanel.PointToScreen(tooltipPosition);
                var screen = Screen.FromControl(mapPanel);
                screenPosition.X = Math.Max(0, Math.Min(screen.WorkingArea.Width - tooltipSize.Width, screenPosition.X));
                screenPosition.Y = Math.Max(0, Math.Min(screen.WorkingArea.Height - tooltipSize.Height, screenPosition.Y));
                tooltipPosition = mapPanel.PointToClient(screenPosition);
                mouseTooltip.Show(tooltip, mapPanel, tooltipPosition.X, tooltipPosition.Y);
            }
            else
            {
                mouseTooltip.Hide(mapPanel);
            }
        }

        private void PickTemplate(Point location, bool wholeTemplate)
        {
            if (map.Metrics.GetCell(location, out int cell))
            {
                var template = map.Templates[cell];
                if (template != null)
                {
                    SelectedTemplateType = template.Type;
                }
                else
                {
                    SelectedTemplateType = map.TemplateTypes.Where(t => t.Equals("clear1")).FirstOrDefault();
                }
                if (!wholeTemplate && ((SelectedTemplateType.IconWidth * SelectedTemplateType.IconHeight) > 1))
                {
                    var icon = template?.Icon ?? 0;
                    SelectedIcon = new Point(icon % SelectedTemplateType.IconWidth, icon / SelectedTemplateType.IconWidth);
                }
                else
                {
                    SelectedIcon = null;
                }
            }
        }

        private int DetectDragEdge()
        {
            var mouseCell = navigationWidget.MouseCell;
            var mousePixel = navigationWidget.MouseSubPixel;
            var topEdge =
                ((mouseCell.Y == dragBounds.Top) && (mousePixel.Y <= (Globals.PixelHeight / 4))) ||
                ((mouseCell.Y == dragBounds.Top - 1) && (mousePixel.Y >= (3 * Globals.PixelHeight / 4)));
            var bottomEdge =
                ((mouseCell.Y == dragBounds.Bottom) && (mousePixel.Y <= (Globals.PixelHeight / 4))) ||
                ((mouseCell.Y == dragBounds.Bottom - 1) && (mousePixel.Y >= (3 * Globals.PixelHeight / 4)));
            var leftEdge =
                 ((mouseCell.X == dragBounds.Left) && (mousePixel.X <= (Globals.PixelWidth / 4))) ||
                 ((mouseCell.X == dragBounds.Left - 1) && (mousePixel.X >= (3 * Globals.PixelWidth / 4)));
            var rightEdge =
                ((mouseCell.X == dragBounds.Right) && (mousePixel.X <= (Globals.PixelHeight / 4))) ||
                ((mouseCell.X == dragBounds.Right - 1) && (mousePixel.X >= (3 * Globals.PixelHeight / 4)));
            if (topEdge)
            {
                if (rightEdge)
                {
                    return 1;
                }
                else if (leftEdge)
                {
                    return 7;
                }
                else
                {
                    return 0;
                }
            }
            else if (bottomEdge)
            {
                if (rightEdge)
                {
                    return 3;
                }
                else if (leftEdge)
                {
                    return 5;
                }
                else
                {
                    return 4;
                }
            }
            else if (rightEdge)
            {
                return 2;
            }
            else if (leftEdge)
            {
                return 6;
            }
            else
            {
                return -1;
            }
        }

        private void CommitChange()
        {
            var undoTemplates2 = new Dictionary<int, Template>(undoTemplates);
            void undoAction(UndoRedoEventArgs e)
            {
                foreach (var kv in undoTemplates2)
                {
                    e.Map.Templates[kv.Key] = kv.Value;
                }
                e.MapPanel.Invalidate(e.Map, undoTemplates2.Keys);
            }
            var redoTemplates2 = new Dictionary<int, Template>(redoTemplates);
            void redoAction(UndoRedoEventArgs e)
            {
                foreach (var kv in redoTemplates2)
                {
                    e.Map.Templates[kv.Key] = kv.Value;
                }
                e.MapPanel.Invalidate(e.Map, redoTemplates2.Keys);
            }
            undoTemplates.Clear();
            redoTemplates.Clear();
            url.Track(undoAction, redoAction);
        }

        private void UpdateStatus()
        {
            if (placementMode)
            {
                statusLbl.Text = "Left-Click to place template, Right-Click to clear template";
            }
            else if (boundsMode)
            {
                if (dragEdge >= 0)
                {
                    statusLbl.Text = "Release left button to end dragging map bounds edge";
                }
                else
                {
                    statusLbl.Text = "Left-Click a map bounds edge to start dragging";
                }
            }
            else
            {
                statusLbl.Text = "Shift to enter placement mode, Ctrl to enter map bounds mode, Left-Click to pick whole template, Right-Click to pick individual template tile";
            }
        }

        protected override void PreRenderMap()
        {
            base.PreRenderMap();
            previewMap = map.Clone();
            if (placementMode)
            {
                var location = navigationWidget.MouseCell;
                if (SelectedTemplateType != null)
                {
                    if (SelectedIcon.HasValue)
                    {
                        if (previewMap.Metrics.GetCell(location, out int cell))
                        {
                            var icon = (SelectedIcon.Value.Y * SelectedTemplateType.IconWidth) + SelectedIcon.Value.X;
                            previewMap.Templates[cell] = new Template { Type = SelectedTemplateType, Icon = icon };
                        }
                    }
                    else
                    {
                        int icon = 0;
                        for (var y = 0; y < SelectedTemplateType.IconHeight; ++y)
                        {
                            for (var x = 0; x < SelectedTemplateType.IconWidth; ++x, ++icon)
                            {
                                if (!SelectedTemplateType.IconMask[x, y])
                                {
                                    continue;
                                }
                                var subLocation = new Point(location.X + x, location.Y + y);
                                if (previewMap.Metrics.GetCell(subLocation, out int cell))
                                {
                                    previewMap.Templates[cell] = new Template { Type = SelectedTemplateType, Icon = icon };
                                }
                            }
                        }
                    }
                }
            }
        }

        protected override void PostRenderMap(Graphics graphics)
        {
            base.PostRenderMap(graphics);
            if (boundsMode)
            {
                var bounds = Rectangle.FromLTRB(
                    dragBounds.Left * Globals.MapTileWidth,
                    dragBounds.Top * Globals.MapTileHeight,
                    dragBounds.Right * Globals.MapTileWidth,
                    dragBounds.Bottom * Globals.MapTileHeight
                );
                using (var boundsPen = new Pen(Color.Red, 8.0f))
                {
                    graphics.DrawRectangle(boundsPen, bounds);
                }
            }
            else
            {
                RenderMapBoundaries(graphics);
                if (placementMode)
                {
                    var location = navigationWidget.MouseCell;
                    if (SelectedTemplateType != null)
                    {
                        var previewBounds = new Rectangle(
                            location.X * Globals.MapTileWidth,
                            location.Y * Globals.MapTileHeight,
                            (SelectedIcon.HasValue ? 1 : SelectedTemplateType.IconWidth) * Globals.MapTileWidth,
                            (SelectedIcon.HasValue ? 1 : SelectedTemplateType.IconHeight) * Globals.MapTileHeight
                        );
                        using (var previewPen = new Pen(Color.Green, 4.0f))
                        {
                            graphics.DrawRectangle(previewPen, previewBounds);
                        }
                    }
                }
            }
        }

        public override void Activate()
        {
            base.Activate();
            this.mapPanel.MouseDown += MapPanel_MouseDown;
            this.mapPanel.MouseUp += MapPanel_MouseUp;
            this.mapPanel.MouseMove += MapPanel_MouseMove;
            (this.mapPanel as Control).KeyDown += TemplateTool_KeyDown;
            (this.mapPanel as Control).KeyUp += TemplateTool_KeyUp;
            navigationWidget.MouseCellChanged += MouseoverWidget_MouseCellChanged;
            templateTypeNavigationWidget?.Activate();
            url.Undone += Url_Undone;
            url.Redone += Url_Redone;
            UpdateStatus();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            mapPanel.MouseDown -= MapPanel_MouseDown;
            mapPanel.MouseUp -= MapPanel_MouseUp;
            mapPanel.MouseMove -= MapPanel_MouseMove;
            (mapPanel as Control).KeyDown -= TemplateTool_KeyDown;
            (mapPanel as Control).KeyUp -= TemplateTool_KeyUp;
            navigationWidget.MouseCellChanged -= MouseoverWidget_MouseCellChanged;
            templateTypeNavigationWidget?.Deactivate();
            url.Undone -= Url_Undone;
            url.Redone -= Url_Redone;
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
                    templateTypeListView.SelectedIndexChanged -= TemplateTypeListView_SelectedIndexChanged;
                    templateTypeNavigationWidget?.Dispose();
                    templateTypeMapPanel.MouseDown -= TemplateTypeMapPanel_MouseDown;
                    templateTypeMapPanel.PostRender -= TemplateTypeMapPanel_PostRender;
                }
                disposedValue = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }

}
