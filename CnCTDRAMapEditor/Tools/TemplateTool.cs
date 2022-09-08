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
        /// <summary> Layers that are important to this tool and need to be drawn last in the PostRenderMap process.</summary>
        protected override MapLayerFlag PriorityLayers => MapLayerFlag.None;
        /// <summary>
        /// Layers that are not painted by the PostRenderMap function on ViewTool level because they are handled
        /// at a specific point in the PostRenderMap override by the implementing tool.
        /// </summary>
        protected override MapLayerFlag ManuallyHandledLayers => MapLayerFlag.Boundaries;

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
        private FacingType dragEdge = FacingType.None;
        private Random random;

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
                    RefreshPreviewMapPanel();
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
                    TemplateType selected = SelectedTemplateType;
                    if (placementMode && (selected != null))
                    {
                        for (var y = 0; y < selected.IconHeight; ++y)
                        {
                            for (var x = 0; x < selected.IconWidth; ++x)
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
            this.random = new Random();
            this.templateTypeListView = templateTypeListView;
            this.templateTypeListView.SelectedIndexChanged -= TemplateTypeListView_SelectedIndexChanged;
            string templateCategory(TemplateType template)
            {
                var m = CategoryRegex.Match(template.Name);
                return m.Success ? m.Groups[1].Value : string.Empty;
            }
            ExplorerComparer expl = new ExplorerComparer();
            var templateTypes = plugin.Map.TemplateTypes
                .Where(t => t.Thumbnail != null
                    && t.Theaters.Contains(plugin.Map.Theater)
                    && (t.Flag & TemplateTypeFlag.Clear) == TemplateTypeFlag.None
                    && (t.Flag & TemplateTypeFlag.IsGrouped) == TemplateTypeFlag.None)
                .OrderBy(t => t.Name, expl)
                .GroupBy(t => templateCategory(t)).OrderBy(g => g.Key, expl);
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
            imageList.Images.AddRange(templateTypeImages.Select(im => im.FitToBoundingBox(maxWidth, maxHeight)).ToArray());
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
                dragEdge = FacingType.None;
                UpdateTooltip();
                mapPanel.Invalidate();
            }
        }

        private void Url_Undone(object sender, EventArgs e)
        {
            if (boundsMode && (map.Bounds != dragBounds))
            {
                dragBounds = map.Bounds;
                dragEdge = FacingType.None;
                UpdateTooltip();
                mapPanel.Invalidate();
            }
        }

        private void TemplateTypeMapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            // This code picks the single icon from the preview pane.
            TemplateType selected = SelectedTemplateType;
            bool isRandom = selected != null && (selected.Flag & TemplateTypeFlag.RandomCell) != TemplateTypeFlag.None;
            if (e.Button == MouseButtons.Right || (selected == null) || (e.Button == MouseButtons.Left && selected.NumIcons == 1))
            {
                SelectedIcon = null;
                return;
            }
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            var templateTypeMouseCell = templateTypeNavigationWidget.MouseCell;
            int width = selected.ThumbnailWidth;
            int height = selected.ThumbnailHeight;
            int x = templateTypeMouseCell.X;
            int y = templateTypeMouseCell.Y;
            if ((x >= 0) && (x < width))
            {
                if ((y >= 0) && (y < height))
                {
                    if (isRandom)
                    {
                        if (y * width + x < selected.NumIcons)
                        {
                            SelectedIcon = templateTypeMouseCell;
                        }
                    }
                    else
                    {
                        if (selected.IconMask == null || selected.IconMask[x, y])
                        {
                            SelectedIcon = templateTypeMouseCell;
                        }
                    }
                }
            }
        }

        private void TemplateTypeMapPanel_PostRender(object sender, RenderEventArgs e)
        {
            e.Graphics.Transform = new Matrix();
            TemplateType selected = SelectedTemplateType;
            if (selected == null)
            {
                return;
            }
            bool isRandom = (selected.Flag & TemplateTypeFlag.RandomCell) != TemplateTypeFlag.None && selected.NumIcons > 1;
            if (SelectedIcon.HasValue || isRandom)
            {
                int width = selected.ThumbnailWidth;
                int height = selected.ThumbnailHeight;
                int panelWidth = templateTypeMapPanel.ClientSize.Width;
                int panelHeight = templateTypeMapPanel.ClientSize.Height;
                int iconWidth = width;
                int iconHeight = height;
                int scaleX = panelWidth / iconWidth;
                int scaleY = panelHeight / iconHeight;
                int scale = Math.Min(scaleX, scaleY);
                int leftoverX = panelWidth - (scale * iconWidth);
                int leftoverY = panelHeight - (scale * iconHeight);
                int padX = leftoverX / 2;
                int padY = leftoverY / 2;
                if (isRandom)
                {
                    int icon = 0;
                    using (var selectedIconPen = new Pen(Color.LightSkyBlue, Math.Max(1, scale / 16)))
                    {
                        var cellSize = new Size(scale, scale);
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                if (icon >= selected.NumIcons)
                                {
                                    break;
                                }
                                var rect = new Rectangle(new Point(padX + x * cellSize.Width, padY + y * cellSize.Height), cellSize);
                                e.Graphics.DrawRectangle(selectedIconPen, rect);
                                icon++;
                            }
                        }
                    }
                }
                if (SelectedIcon.HasValue)
                {
                    using (var selectedIconPen = new Pen(Color.Yellow, Math.Max(1, scale / 16)))
                    {
                        var cellSize = new Size(scale, scale);
                        var rect = new Rectangle(new Point(padX + SelectedIcon.Value.X * cellSize.Width, padY + SelectedIcon.Value.Y * cellSize.Height), cellSize);
                        e.Graphics.DrawRectangle(selectedIconPen, rect);
                    }
                }
            }
            var sizeStringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            var text = string.Format("{0} ({1}x{2})", selected.DisplayName, selected.IconWidth, selected.IconHeight);
            var textSize = e.Graphics.MeasureString(text, SystemFonts.CaptionFont) + new SizeF(6.0f, 6.0f);
            var textBounds = new RectangleF(new PointF(0, 0), textSize);
            using (var sizeBackgroundBrush = new SolidBrush(Color.FromArgb(128, Color.Black)))
            using (var sizeTextBrush = new SolidBrush(Color.White))
            {
                e.Graphics.FillRectangle(sizeBackgroundBrush, textBounds);
                e.Graphics.DrawString(text, SystemFonts.CaptionFont, sizeTextBrush, textBounds, sizeStringFormat);
            }
        }

        private void TemplateTool_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                if (boundsMode)
                {
                    ExitAllModes();
                }
                else
                {
                    EnterPlacementMode();
                }
            }
            else if (e.KeyCode == Keys.ControlKey)
            {
                if (placementMode)
                {
                    ExitAllModes();
                }
                else
                {
                    EnterBoundsMode();
                }
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
                CommitEdgeDrag();
                CommitTileChanges();
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
            TemplateType selected = SelectedTemplateType;
            if (button == MouseButtons.Left)
            {
                if (selected == null || (selected.Flag & TemplateTypeFlag.Clear) != 0)
                {
                    RemoveTemplate(navigationWidget.MouseCell);
                }
                else
                {
                    SetTemplate(navigationWidget.MouseCell);
                }
            }
            else if (button == MouseButtons.Right)
            {
                RemoveTemplate(navigationWidget.MouseCell);
            }
        }

        private void MapPanel_MouseUp(object sender, MouseEventArgs e)
        {
            CommitEdgeDrag();
            CommitTileChanges();
        }

        private void MapPanel_MouseLeave(object sender, EventArgs e)
        {
            ExitAllModes();
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
                switch (dragEdge != FacingType.None ? dragEdge : DetectDragEdge())
                {
                    case FacingType.North:
                    case FacingType.South:
                        cursor = Cursors.SizeNS;
                        break;
                    case FacingType.East:
                    case FacingType.West:
                        cursor = Cursors.SizeWE;
                        break;
                    case FacingType.NorthEast:
                    case FacingType.SouthWest:
                        cursor = Cursors.SizeNESW;
                        break;
                    case FacingType.NorthWest:
                    case FacingType.SouthEast:
                        cursor = Cursors.SizeNWSE;
                        break;
                }
                // NavigationWidget manages all mouse cursor changes, so tools don't conflict with each other.
                if (navigationWidget.CurrentCursor != cursor)
                {
                    navigationWidget.CurrentCursor = cursor;
                    UpdateTooltip();
                }
            }
            if (!boundsMode)
            {
                mouseTooltip.Hide(mapPanel);
            }
        }

        private void MouseoverWidget_ClosestMouseCellBorderChanged(object sender, MouseCellChangedEventArgs e)
        {
            if (dragEdge == FacingType.None)
            {
                return;
            }
            var endDrag = e.NewCell;
            map.Metrics.Clip(ref endDrag, new Size(1, 1), Size.Empty);
            switch (dragEdge)
            {
                case FacingType.North:
                case FacingType.NorthEast:
                case FacingType.NorthWest:
                    if (endDrag.Y < dragBounds.Bottom)
                    {
                        dragBounds.Height = Math.Max(1, dragBounds.Bottom - endDrag.Y);
                        dragBounds.Y = endDrag.Y;
                    }
                    break;
            }
            switch (dragEdge)
            {
                case FacingType.SouthWest:
                case FacingType.West:
                case FacingType.NorthWest:
                    if (endDrag.X < dragBounds.Right)
                    {
                        dragBounds.Width = Math.Max(1, dragBounds.Right - endDrag.X);
                        dragBounds.X = endDrag.X;
                    }
                    break;
            }
            switch (dragEdge)
            {
                case FacingType.SouthEast:
                case FacingType.South:
                case FacingType.SouthWest:
                    if (endDrag.Y > dragBounds.Top)
                    {
                        dragBounds.Height = Math.Max(1, endDrag.Y - dragBounds.Top);
                    }
                    break;
            }
            switch (dragEdge)
            {
                case FacingType.NorthEast:
                case FacingType.East:
                case FacingType.SouthEast:
                    if (endDrag.X > dragBounds.Left)
                    {
                        dragBounds.Width = Math.Max(1, endDrag.X - dragBounds.Left);
                    }
                    break;
            }
            // Doesn't need a map re-render, just a repaint with the post-render steps. So give it 0 cells.
            mapPanel.Invalidate();
            UpdateTooltip();
        }

        private void MouseoverWidget_MouseCellChanged(object sender, MouseCellChangedEventArgs e)
        {
            if (dragEdge != FacingType.None)
            {
                return;
            }
            mouseTooltip.Hide(mapPanel);
            if (!placementMode)
            {
                if ((Control.MouseButtons == MouseButtons.Left) || (Control.MouseButtons == MouseButtons.Right))
                {
                    PickTemplate(navigationWidget.MouseCell, Control.MouseButtons == MouseButtons.Left);
                }
                return;
            }
            HandlePlace(Control.MouseButtons);
            TemplateType selected = SelectedTemplateType;
            if (selected == null)
            {
                return;
            }
            foreach (var location in new Point[] { e.OldCell, e.NewCell })
            {
                if (SelectedIcon.HasValue)
                {
                    mapPanel.Invalidate(map, new Point(location.X, location.Y));
                }
                else
                {
                    for (var y = 0; y < selected.IconHeight; ++y)
                    {
                        for (var x = 0; x < selected.IconWidth; ++x)
                        {
                            if (selected.IconMask != null && !selected.IconMask[x, y])
                            {
                                continue;
                            }
                            mapPanel.Invalidate(map, new Point(location.X + x, location.Y + y));
                        }
                    }
                }
            }
        }

        private void RefreshPreviewMapPanel()
        {
            if (templateTypeNavigationWidget != null)
            {
                templateTypeNavigationWidget.Dispose();
                templateTypeNavigationWidget = null;
            }
            TemplateType selected = SelectedTemplateType;
            if (selected != null)
            {
                if (selected.Thumbnail == null)
                {
                    // Special case: tile is not initialised. Initialise with forced dummy generation.
                    // This is really only for the tile FF "research mode" on RA maps.
                    selected.Init(plugin.Map.Theater, true);
                }
                templateTypeMapPanel.MapImage = selected.Thumbnail;
                var templateTypeMetrics = new CellMetrics(selected.ThumbnailWidth, selected.ThumbnailHeight);
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
            TemplateType selected = SelectedTemplateType;
            if (selected == null)
            {
                return;
            }
            bool isGroup = (selected.Flag & TemplateTypeFlag.Group) == TemplateTypeFlag.Group;
            if (SelectedIcon.HasValue)
            {
                if (map.Metrics.GetCell(location, out int cell))
                {
                    if (!undoTemplates.ContainsKey(cell))
                    {
                        undoTemplates[cell] = map.Templates[location];
                    }
                    var icon = (SelectedIcon.Value.Y * selected.ThumbnailWidth) + SelectedIcon.Value.X;
                    TemplateType placeType = selected;
                    if (isGroup)
                    {
                        placeType = map.TemplateTypes.Where(t => t.Name == selected.GroupTiles[icon]).FirstOrDefault();
                        icon = 0;
                    }
                    var template = new Template { Type = placeType, Icon = icon };
                    map.Templates[cell] = template;
                    redoTemplates[cell] = template;
                    mapPanel.Invalidate(map, cell);
                    plugin.Dirty = true;
                }
            }
            else
            {
                for (int y = 0, icon = 0; y < selected.IconHeight; ++y)
                {
                    for (var x = 0; x < selected.IconWidth; ++x, ++icon)
                    {
                        if (selected.IconMask != null && !selected.IconMask[x, y])
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
                bool isRandom = (selected.Flag & TemplateTypeFlag.RandomCell) != TemplateTypeFlag.None && selected.IconWidth == 1 && selected.IconHeight == 1;
                for (int y = 0, icon = 0; y < selected.IconHeight; ++y)
                {
                    for (var x = 0; x < selected.IconWidth; ++x, ++icon)
                    {
                        if (selected.IconMask != null && !selected.IconMask[x, y])
                        {
                            continue;
                        }
                        var subLocation = new Point(location.X + x, location.Y + y);
                        if (map.Metrics.GetCell(subLocation, out int cell))
                        {
                            int placeIcon;
                            TemplateType placeType = selected;
                            if (isGroup)
                            {
                                int randomType = random.Next(0, selected.NumIcons);
                                placeType = map.TemplateTypes.Where(t => t.Name == selected.GroupTiles[randomType]).FirstOrDefault();
                                placeIcon = 0;
                            }
                            else if (isRandom)
                            {
                                placeIcon = random.Next(0, selected.NumIcons);
                            }
                            else
                            {
                                placeIcon = icon;
                            }
                            var template = new Template { Type = placeType, Icon = placeIcon };
                            map.Templates[cell] = template;
                            redoTemplates[cell] = template;
                            mapPanel.Invalidate(map, cell);
                            plugin.Dirty = true;
                        }
                    }
                }
            }
        }

        private void RemoveTemplate(Point location)
        {
            TemplateType selected = SelectedTemplateType;
            if (selected == null || SelectedIcon.HasValue)
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
                for (int y = 0, icon = 0; y < selected.IconHeight; ++y)
                {
                    for (var x = 0; x < selected.IconWidth; ++x, ++icon)
                    {
                        if (selected.IconMask != null && !selected.IconMask[x, y])
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
                for (int y = 0, icon = 0; y < selected.IconHeight; ++y)
                {
                    for (var x = 0; x < selected.IconWidth; ++x, ++icon)
                    {
                        if (selected.IconMask != null && !selected.IconMask[x, y])
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

        private void EnterPlacementMode()
        {
            if (placementMode || boundsMode)
            {
                return;
            }
            placementMode = true;
            navigationWidget.MouseoverSize = Size.Empty;
            InvalidateCurrentArea();
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
            InvalidateCurrentArea();
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
            navigationWidget.CurrentCursor = Cursors.Default;
            dragEdge = FacingType.None;
            dragBounds = Rectangle.Empty;
            placementMode = false;
            navigationWidget.MouseoverSize = new Size(1, 1);
            InvalidateCurrentArea();
            UpdateTooltip();
            UpdateStatus();
        }

        private void InvalidateCurrentArea()
        {
            TemplateType selected = SelectedTemplateType;
            if (selected == null)
            {
                return;
            }
            for (var y = 0; y < selected.IconHeight; ++y)
            {
                for (var x = 0; x < selected.IconWidth; ++x)
                {
                    mapPanel.Invalidate(map, new Point(navigationWidget.MouseCell.X + x, navigationWidget.MouseCell.Y + y));
                }
            }
        }

        private void UpdateTooltip()
        {
            FacingType showEdge = dragEdge != FacingType.None ? dragEdge : DetectDragEdge();
            if (boundsMode && showEdge != FacingType.None)
            {
                var tooltip = string.Format("X = {0}\nY = {1}\nWidth = {2}\nHeight = {3}", dragBounds.Left, dragBounds.Top, dragBounds.Width, dragBounds.Height);
                var textSize = TextRenderer.MeasureText(tooltip, SystemFonts.CaptionFont);
                var tooltipSize = new Size(textSize.Width + 6, textSize.Height + 6);

                Point tooltipPosition = Control.MousePosition;
                PointF zoomedCell = navigationWidget.ZoomedCellSize;
                // Corrects to nearest border; should match NavigationWidget.ClosestMouseCellBorder
                if (navigationWidget.ClosestMouseCellBorder.Y > navigationWidget.MouseCell.Y)
                {
                    tooltipPosition.Y += (int)((Globals.PixelWidth - navigationWidget.MouseSubPixel.Y) * zoomedCell.Y / Globals.PixelWidth);
                }
                else
                {
                    tooltipPosition.Y -= (int)(navigationWidget.MouseSubPixel.Y * zoomedCell.Y / Globals.PixelWidth);
                }
                if (navigationWidget.ClosestMouseCellBorder.X > navigationWidget.MouseCell.X)
                {
                    tooltipPosition.X += (int)((Globals.PixelWidth - navigationWidget.MouseSubPixel.X) * zoomedCell.X / Globals.PixelWidth);
                }
                else
                {
                    tooltipPosition.X -= (int)(navigationWidget.MouseSubPixel.X * zoomedCell.X / Globals.PixelWidth);
                }
                switch (showEdge)
                {
                    case FacingType.North:
                    case FacingType.NorthEast:
                    case FacingType.NorthWest:
                        tooltipPosition.Y += (int)zoomedCell.Y;
                        break;
                    case FacingType.South:
                    case FacingType.SouthEast:
                    case FacingType.SouthWest:
                        tooltipPosition.Y -= (int)zoomedCell.Y;
                        break;
                }
                switch (showEdge)
                {
                    case FacingType.SouthWest:
                    case FacingType.West:
                    case FacingType.NorthWest:
                        tooltipPosition.X += (int)zoomedCell.X;
                        break;
                    case FacingType.SouthEast:
                    case FacingType.East:
                    case FacingType.NorthEast:
                        tooltipPosition.X -= (int)zoomedCell.X;
                        break;
                }
                //*/
                switch (showEdge)
                {
                    case FacingType.South:
                    case FacingType.SouthEast:
                    case FacingType.SouthWest:
                        tooltipPosition.Y -= tooltipSize.Height;
                        break;
                }
                switch (showEdge)
                {
                    case FacingType.SouthEast:
                    case FacingType.East:
                    case FacingType.NorthEast:
                        tooltipPosition.X -= tooltipSize.Width;
                        break;
                }
                var screen = Screen.FromControl(mapPanel);
                tooltipPosition.X = Math.Max(screen.WorkingArea.X, Math.Min(screen.WorkingArea.X + screen.WorkingArea.Width - tooltipSize.Width, tooltipPosition.X));
                tooltipPosition.Y = Math.Max(screen.WorkingArea.Y, Math.Min(screen.WorkingArea.Y + screen.WorkingArea.Height - tooltipSize.Height, tooltipPosition.Y));
                tooltipPosition = mapPanel.PointToClient(tooltipPosition);
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
                bool groupOwned = false;
                if (template != null)
                {
                    if ((template.Type.Flag & TemplateTypeFlag.IsGrouped) != TemplateTypeFlag.None && template.Type.GroupTiles.Length == 1)
                    {
                        groupOwned = true;
                        string owningType = template.Type.GroupTiles[0];
                        SelectedTemplateType = map.TemplateTypes.Where(t => t.Name.Equals(owningType, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    }
                    else
                    {
                        SelectedTemplateType = template.Type;
                    }
                }
                else
                {
                    SelectedTemplateType = map.TemplateTypes.Where(t => t.Name.Equals("clear1", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                }
                TemplateType selected = SelectedTemplateType;
                bool isRandom = (selected.Flag & TemplateTypeFlag.RandomCell) != TemplateTypeFlag.None && selected.NumIcons > 1;
                if (!wholeTemplate && ((selected.IconWidth * selected.IconHeight) > 1 || isRandom))
                {
                    int icon;
                    if (groupOwned)
                    {
                        string origType = template.Type.Name;
                        icon = Enumerable.Range(0, selected.GroupTiles.Length).FirstOrDefault(i => origType.Equals(selected.GroupTiles[i], StringComparison.InvariantCultureIgnoreCase));
                    }
                    else
                    {
                        icon = template?.Icon ?? 0;
                    }
                    int width = selected.ThumbnailWidth;
                    SelectedIcon = new Point(icon % width, icon / width);
                }
                else
                {
                    SelectedIcon = null;
                }
            }
        }

        private FacingType DetectDragEdge()
        {
            var mouseCell = navigationWidget.ClosestMouseCellBorder;
            var realMouseCell = navigationWidget.MouseCell;
            var mousePixelDistanceY = navigationWidget.MouseSubPixel.Y;
            if (mousePixelDistanceY > Globals.PixelHeight / 2)
            {
                mousePixelDistanceY = Globals.PixelHeight - mousePixelDistanceY;
            }
            var mousePixelDistanceX = navigationWidget.MouseSubPixel.X;
            if (mousePixelDistanceX > Globals.PixelWidth / 2)
            {
                mousePixelDistanceX = Globals.PixelWidth - mousePixelDistanceX;
            }
            int nearEnough = Globals.PixelHeight / 4;
            int nearEnoughTopLeft = Globals.PixelHeight - nearEnough;
            bool inBoundsX = (realMouseCell.X >= dragBounds.X || realMouseCell.X == dragBounds.X - 1 && navigationWidget.MouseSubPixel.X >= nearEnoughTopLeft)
                              && (realMouseCell.X < dragBounds.X + dragBounds.Width || realMouseCell.X == dragBounds.X + dragBounds.Width && navigationWidget.MouseSubPixel.X <= nearEnough);
            bool inBoundsY = (realMouseCell.Y >= dragBounds.Y || realMouseCell.Y == dragBounds.Y - 1 && navigationWidget.MouseSubPixel.Y >= nearEnoughTopLeft)
                              && (realMouseCell.Y < dragBounds.Y + dragBounds.Height || realMouseCell.Y == dragBounds.Y + dragBounds.Height && navigationWidget.MouseSubPixel.Y <= nearEnough);
            bool topEdge = mouseCell.Y == dragBounds.Top && mousePixelDistanceY <= nearEnough && inBoundsX;
            bool bottomEdge = mouseCell.Y == dragBounds.Bottom && mousePixelDistanceY <= nearEnough && inBoundsX;
            bool leftEdge = mouseCell.X == dragBounds.Left && mousePixelDistanceX <= nearEnough && inBoundsY;
            bool rightEdge = mouseCell.X == dragBounds.Right && mousePixelDistanceX <= nearEnough && inBoundsY;
            if (topEdge)
            {
                if (rightEdge)
                {
                    return FacingType.NorthEast;
                }
                else if (leftEdge)
                {
                    return FacingType.NorthWest;
                }
                else
                {
                    return FacingType.North;
                }
            }
            else if (bottomEdge)
            {
                if (rightEdge)
                {
                    return FacingType.SouthEast;
                }
                else if (leftEdge)
                {
                    return FacingType.SouthWest;
                }
                else
                {
                    return FacingType.South;
                }
            }
            else if (rightEdge)
            {
                return FacingType.East;
            }
            else if (leftEdge)
            {
                return FacingType.West;
            }
            else
            {
                return FacingType.None;
            }
        }

        private void CommitEdgeDrag()
        {
            if (!boundsMode)
            {
                return;
            }
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
                plugin.Dirty = true;
                url.Track(undoAction, redoAction);
                mapPanel.Invalidate();
            }
            dragEdge = FacingType.None;
            UpdateStatus();
        }

        private void CommitTileChanges()
        {
            if (!placementMode || (undoTemplates.Count == 0) || (redoTemplates.Count == 0))
            {
                return;
            }
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
            if (!placementMode)
            {
                return;
            }
            TemplateType selected = SelectedTemplateType;
            if (selected == null)
            {
                return;
            }
            var location = navigationWidget.MouseCell;
            if (SelectedIcon.HasValue)
            {
                if (previewMap.Metrics.GetCell(location, out int cell))
                {
                    var icon = (SelectedIcon.Value.Y * selected.ThumbnailWidth) + SelectedIcon.Value.X;
                    previewMap.Templates[cell] = new Template { Type = selected, Icon = icon };
                }
            }
            else
            {
                int icon = 0;
                for (var y = 0; y < selected.IconHeight; ++y)
                {
                    for (var x = 0; x < selected.IconWidth; ++x, ++icon)
                    {
                        if (selected.IconMask != null && !selected.IconMask[x, y])
                        {
                            continue;
                        }
                        var subLocation = new Point(location.X + x, location.Y + y);
                        if (previewMap.Metrics.GetCell(subLocation, out int cell))
                        {
                            previewMap.Templates[cell] = new Template { Type = selected, Icon = icon };
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
                RenderMapBoundaries(graphics, Layers, map, dragBounds, Globals.MapTileSize, Color.Red);
            }
            else
            {
                RenderMapBoundaries(graphics, Layers, map, Globals.MapTileSize);
                if (placementMode)
                {
                    var location = navigationWidget.MouseCell;
                    TemplateType selected = SelectedTemplateType;
                    if (selected == null)
                    {
                        return;
                    }
                    var previewBounds = new Rectangle(
                        location.X * Globals.MapTileWidth,
                        location.Y * Globals.MapTileHeight,
                        (SelectedIcon.HasValue ? 1 : selected.IconWidth) * Globals.MapTileWidth,
                        (SelectedIcon.HasValue ? 1 : selected.IconHeight) * Globals.MapTileHeight
                    );
                    using (var previewPen = new Pen(Color.Green, Math.Max(1, Globals.MapTileSize.Width / 16.0f)))
                    {
                        graphics.DrawRectangle(previewPen, previewBounds);
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
            this.mapPanel.MouseLeave += MapPanel_MouseLeave;
            (this.mapPanel as Control).KeyDown += TemplateTool_KeyDown;
            (this.mapPanel as Control).KeyUp += TemplateTool_KeyUp;
            navigationWidget.MouseCellChanged += MouseoverWidget_MouseCellChanged;
            navigationWidget.ClosestMouseCellBorderChanged += MouseoverWidget_ClosestMouseCellBorderChanged;
            templateTypeNavigationWidget?.Activate();
            url.Undone += Url_Undone;
            url.Redone += Url_Redone;
            UpdateStatus();
        }

        public override void Deactivate()
        {
            ExitAllModes();
            base.Deactivate();
            mapPanel.MouseDown -= MapPanel_MouseDown;
            mapPanel.MouseUp -= MapPanel_MouseUp;
            mapPanel.MouseMove -= MapPanel_MouseMove;
            mapPanel.MouseLeave -= MapPanel_MouseLeave;
            (mapPanel as Control).KeyDown -= TemplateTool_KeyDown;
            (mapPanel as Control).KeyUp -= TemplateTool_KeyUp;
            navigationWidget.CurrentCursor = Cursors.Default;
            navigationWidget.ClosestMouseCellBorderChanged -= MouseoverWidget_ClosestMouseCellBorderChanged;
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
                    this.templateTypeListView.SelectedIndexChanged -= TemplateTypeListView_SelectedIndexChanged;
                    this.templateTypeNavigationWidget?.Dispose();
                    this.templateTypeMapPanel.MouseDown -= TemplateTypeMapPanel_MouseDown;
                    this.templateTypeMapPanel.PostRender -= TemplateTypeMapPanel_PostRender;
                    foreach (Image img in this.templateTypeListView.LargeImageList.Images)
                    {
                        try
                        {
                            img.Dispose();
                        }
                        catch
                        {
                            // Ignore.
                        }
                    }
                }
                disposedValue = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }

}
