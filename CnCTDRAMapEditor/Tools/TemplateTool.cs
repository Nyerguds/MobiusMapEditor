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
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MobiusEditor.Tools
{
    public class TemplateTool : ViewTool
    {
        /// <summary>
        /// Layers that are not painted by the PostRenderMap function on ViewTool level because they are handled
        /// at a specific point in the PostRenderMap override by the implementing tool.
        /// </summary>
        protected override MapLayerFlag ManuallyHandledLayers
        {
            get
            {
                MapLayerFlag handled = MapLayerFlag.Boundaries | MapLayerFlag.LandTypes | MapLayerFlag.TechnoOccupancy | MapLayerFlag.HomeAreaBox;
                if (boundsMode)
                {
                    handled |= MapLayerFlag.MapSymmetry;
                }
                return handled;
            }
        }

        private const int DesignResWidth = 3840;
        private const int DesignTextureWidth = 256;

        private static readonly Regex CategoryRegex = new Regex(@"^([a-z]*)", RegexOptions.Compiled);

        private readonly ListView templateTypeListView;
        private readonly MapPanel templateTypeMapPanel;
        private readonly ToolTip mouseTooltip;

        private readonly Dictionary<int, Template> undoTemplates = new Dictionary<int, Template>();
        private readonly Dictionary<int, Template> redoTemplates = new Dictionary<int, Template>();
        private string lastPlaced;

        private Map previewMap;
        protected override Map RenderMap => previewMap;

        public override bool IsBusy { get { return undoTemplates.Count > 0; } }

        // Uses a dummy Template object with the type and selected cell, with -1 if no specific cell is selected.
        public override object CurrentObject
        {
            get
            {
                Template template = new Template();
                template.Type = this.selectedTemplateType;
                Point? sel = SelectedIcon;
                if (!sel.HasValue)
                {
                    template.Icon = -1;
                }
                else
                {
                    template.Icon = (sel.Value.Y * template.Type.ThumbnailIconWidth) + sel.Value.X;
                }
                return template;
            }
            set
            {
                if (value is Template tem)
                {
                    TemplateType tt = null;
                    foreach (ListViewItem item in templateTypeListView.Items)
                    {
                        if (item.Tag is TemplateType tmt && tmt.ID == tem.Type.ID && String.Equals(tmt.Name, tem.Type.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            tt = tmt;
                            break;
                        }
                    }
                    if (tt != null)
                    {
                        SelectedTemplateType = tt;
                    }
                    if (tem.Icon < 0 || tt == null || tem.Icon >= tt.IconWidth * tt.IconHeight)
                    {
                        SelectedIcon = null;
                    }
                    else
                    {
                        Point p = new Point(tem.Icon % tem.Type.ThumbnailIconWidth, tem.Icon / tem.Type.ThumbnailIconWidth);
                        SelectedIcon = tt.IconMask[p.Y, p.X] ? p : (Point?)null;
                    }
                }
            }
        }

        private bool placementMode;

        protected override bool InPlacementMode
        {
            get { return placementMode || fillMode; }
        }

        private bool boundsMode;
        private Rectangle dragBounds;
        private FacingType dragEdge = FacingType.None;
        private Point? dragStartPoint = null;
        private Rectangle? dragStartBounds = null;
        private Random random;

        private bool fillMode;

        private TemplateType selectedTemplateType;
        private TemplateType SelectedTemplateType
        {
            get => selectedTemplateType;
            set
            {
                bool wasNull = value == null;
                if (wasNull)
                {
                    // Get clear terrain from list entries.
                    foreach (ListViewItem item in templateTypeListView.Items)
                    {
                        if (item.Tag is TemplateType tt && tt.Flags == TemplateTypeFlag.Clear)
                        {
                            value = tt;
                            break;
                        }
                    }
                }
                if (selectedTemplateType != value)
                {
                    if ((placementMode || fillMode) && (selectedTemplateType != null))
                    {
                        for (int y = 0; y < selectedTemplateType.IconHeight; ++y)
                        {
                            for (int x = 0; x < selectedTemplateType.IconWidth; ++x)
                            {
                                mapPanel.Invalidate(map, new Point(navigationWidget.MouseCell.X + x, navigationWidget.MouseCell.Y + y));
                            }
                        }
                    }
                    selectedTemplateType = value;
                    templateTypeListView.BeginUpdate();
                    templateTypeListView.SelectedIndexChanged -= TemplateTypeListView_SelectedIndexChanged;
                    if (!wasNull)
                    {
                        foreach (ListViewItem item in templateTypeListView.Items)
                        {
                            item.Selected = item.Tag == selectedTemplateType;
                        }
                        if (templateTypeListView.SelectedIndices.Count > 0)
                        {
                            templateTypeListView.EnsureVisible(templateTypeListView.SelectedIndices[0]);
                        }
                    }
                    templateTypeListView.SelectedIndexChanged += TemplateTypeListView_SelectedIndexChanged;
                    templateTypeListView.EndUpdate();
                    if ((placementMode || fillMode) && (selectedTemplateType != null))
                    {
                        for (int y = 0; y < selectedTemplateType.IconHeight; ++y)
                        {
                            for (int x = 0; x < selectedTemplateType.IconWidth; ++x)
                            {
                                mapPanel.Invalidate(map, new Point(navigationWidget.MouseCell.X + x, navigationWidget.MouseCell.Y + y));
                            }
                        }
                    }
                    RefreshPreviewPanel();
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
                    if ((placementMode || fillMode) && (selected != null))
                    {
                        for (int y = 0; y < selected.IconHeight; ++y)
                        {
                            for (int x = 0; x < selected.IconWidth; ++x)
                            {
                                mapPanel.Invalidate(map, new Point(navigationWidget.MouseCell.X + x, navigationWidget.MouseCell.Y + y));
                            }
                        }
                    }
                }
            }
        }

        private NavigationWidget templateTypeNavigationWidget;

        public TemplateTool(MapPanel mapPanel, MapLayerFlag layers, ToolStripStatusLabel statusLbl, ListView templateTypeListView, MapPanel templateTypeMapPanel, ToolTip mouseTooltip,
            IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> url)
            : base(mapPanel, layers, statusLbl, plugin, url)
        {
            previewMap = map;
            // Used for placing random tiles.
            this.random = new Random();
            this.templateTypeListView = templateTypeListView;
            this.templateTypeListView.SelectedIndexChanged -= TemplateTypeListView_SelectedIndexChanged;
            string templateCategory(TemplateType template)
            {
                Match m = CategoryRegex.Match(template.Name);
                return m.Success ? m.Groups[1].Value : string.Empty;
            }
            TheaterType theater = plugin.Map.Theater;
            TemplateType clear = plugin.Map.TemplateTypes.Where(t => t.Flags.HasFlag(TemplateTypeFlag.Clear)).FirstOrDefault();
            if (clear.Thumbnail == null || !clear.Initialised)
            {
                // Clear should ALWAYS be initialised and available, even if missing.
                clear.Init(plugin.GameInfo, plugin.Map.Theater, true, false);
            }
            ExplorerComparer expl = new ExplorerComparer();
            // Special case: tiles that are not initialised, but are present on the map. Initialise with forced dummy generation.
            // This is really only for the tile FF "research mode" on RA maps, or if FilterTheaterObjects is disabled.
            foreach (TemplateType templateType in plugin.Map.Templates.Select(ct => ct.Value?.Type).Distinct().Where(t => t != null && !t.Initialised))
            {
                templateType.Init(plugin.GameInfo, plugin.Map.Theater, true, false);
            }
            var templateTypes = plugin.Map.TemplateTypes
                .Where(t => t.Thumbnail != null && (!Globals.FilterTheaterObjects || t.ExistsInTheater)
                    && !t.Flags.HasFlag(TemplateTypeFlag.Clear)
                    && !t.Flags.HasFlag(TemplateTypeFlag.IsGrouped))
                .OrderBy(t => t.Name, expl)
                .GroupBy(t => templateCategory(t)).OrderBy(g => g.Key, expl);
            List<Bitmap> templateTypeImages = new List<Bitmap>();
            templateTypeImages.Add(clear.Thumbnail);
            templateTypeImages.AddRange(templateTypes.SelectMany(g => g).Select(t => t.Thumbnail));
            Screen screen = Screen.FromHandle(mapPanel.Handle) ?? Screen.PrimaryScreen;
            int maxSize = Properties.Settings.Default.MaxMapTileTextureSize;
            if (maxSize == 0)
            {
                double ratio = DesignResWidth / (double)screen.Bounds.Width;
                maxSize = (int)Math.Round((DesignTextureWidth / ratio) * Properties.Settings.Default.TemplateToolTextureSizeMultiplier);
            }
            int maxWidth = Math.Min(templateTypeImages.Max(t => t.Width), maxSize);
            int maxHeight = Math.Min(templateTypeImages.Max(t => t.Height), maxSize);
            ImageList imageList = new ImageList();
            imageList.Images.AddRange(templateTypeImages.Select(im => im.FitToBoundingBox(maxWidth, maxHeight)).ToArray());
            imageList.ImageSize = new Size(maxWidth, maxHeight);
            imageList.ColorDepth = ColorDepth.Depth24Bit;
            this.templateTypeListView.BeginUpdate();
            this.templateTypeListView.LargeImageList = imageList;
            this.templateTypeListView.View = View.LargeIcon;
            const int padding = 12;
            ListViewItem_SetPadding(this.templateTypeListView, padding, padding);
            // Fixed constantly growing items list.
            if (this.templateTypeListView.Groups.Count > 0)
                this.templateTypeListView.Groups.Clear();
            if (this.templateTypeListView.Items.Count > 0)
                this.templateTypeListView.Items.Clear();
            int imageIndex = 0;
            ListViewGroup groupClear = new ListViewGroup(clear.DisplayName);
            this.templateTypeListView.Groups.Add(groupClear);
            ListViewItem itemClear = new ListViewItem(clear.DisplayName, imageIndex++)
            {
                Group = groupClear,
                Tag = clear
            };
            this.templateTypeListView.Items.Add(itemClear);
            foreach (var templateTypeGroup in templateTypes)
            {
                ListViewGroup group = new ListViewGroup(templateTypeGroup.Key);
                this.templateTypeListView.Groups.Add(group);
                foreach (TemplateType templateType in templateTypeGroup)
                {
                    ListViewItem item = new ListViewItem(templateType.DisplayName, imageIndex++)
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
            this.templateTypeMapPanel.MouseMove += TemplateTypeMapPanel_MouseMove;
            this.templateTypeMapPanel.PostRender += TemplateTypeMapPanel_PostRender;
            this.templateTypeMapPanel.BackColor = Color.Black;
            this.templateTypeMapPanel.MaxZoom = 1;
            this.templateTypeMapPanel.SmoothScale = Globals.PreviewSmoothScale;
            this.mouseTooltip = mouseTooltip;
            // Select first actually-initialised non-clear tile.
            SelectedTemplateType = templateTypes.FirstOrDefault(gr => gr.Any(t => t.ExistsInTheater))?.FirstOrDefault(t => t.ExistsInTheater);
        }

        public void ListViewItem_SetPadding(ListView listview, int horizontalPadding, int verticalPadding)
        {
            if (listview != null && listview.View == View.LargeIcon && listview.LargeImageList != null)
            {
                Size imgSize = listview.LargeImageList.ImageSize;
                ListViewItem_SetSpacing(listview, imgSize.Width + horizontalPadding, imgSize.Height + 4 + listview.Font.Height + verticalPadding);
            }
        }

        public void ListViewItem_SetSpacing(ListView listview, int itemWidth, int itemHeight)
        {
            const int LVM_FIRST = 0x1000;
            const int LVM_SETICONSPACING = LVM_FIRST + 53;
            uint arg = (uint)(itemWidth & 0xFFFF) | (uint)((itemHeight & 0xFFFF) << 16);
            GeneralUtils.SendMessage(listview.Handle, LVM_SETICONSPACING, IntPtr.Zero, (IntPtr)arg);
        }

        private void Url_UndoRedoDone(object sender, UndoRedoEventArgs ev)
        {
            // Only update this stuff if the undo/redo event was actually a map change.
            if (!ev.Source.HasFlag(ToolType.Map))
            {
                return;
            }
            // Fixes the fact bounds are not applied when pressing ctrl+z / ctrl+y due to the fact the ctrl key is held down
            if (boundsMode && (map.Bounds != dragBounds))
            {
                HashSet<Point> updCells = GetResourceUpdateCells(dragBounds, map.Bounds);
                dragBounds = map.Bounds;
                dragEdge = FacingType.None;
                dragStartPoint = null;
                dragStartBounds = null;
                UpdateTooltip();
                mapPanel.Invalidate(map, updCells);
            }
        }

        /// <summary>
        /// Determines which cells of resources will need graphical updates with the system
        /// to reduce resources to their minimum size outside the map bounds.
        /// </summary>
        /// <param name="oldBounds">Map bounds before the resize.</param>
        /// <param name="newBounds">Map bounds after the resize.</param>
        /// <returns></returns>
        private HashSet<Point> GetResourceUpdateCells(Rectangle oldBounds, Rectangle newBounds)
        {
            Rectangle intersect = oldBounds;
            intersect.Intersect(newBounds);
            // Update 1-wide border inside the area common between the two rectangles.
            // Should technically only be the cells at the sides that border the new and old areas,
            // but we'll leave the rest to not complicate the algorithm here too much.
            HashSet<Point> cellsToUpdate = intersect.BorderCells(1).ToHashSet();
            // Update entire old area outside common rectangle.
            cellsToUpdate.UnionWith(oldBounds.Points().Where(p => !intersect.Contains(p)));
            // Update entire new area outside common rectangle.
            cellsToUpdate.UnionWith(newBounds.Points().Where(p => !intersect.Contains(p)));
            HashSet<Point> resourceCellsToUpdate = new HashSet<Point>();
            foreach ((Point location, Overlay overlay) in map.Overlay.IntersectsWithPoints(cellsToUpdate).Where(o => o.Value.Type.IsResource))
            {
                resourceCellsToUpdate.Add(location);
            }
            return resourceCellsToUpdate;
        }

        private void TemplateTypeMapPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TemplateTypeMapPanel_MouseDown(sender, e);
            }
        }

        private void TemplateTypeMapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            // This code picks the single icon from the preview pane.
            TemplateType selected = SelectedTemplateType;
            if (e.Button == MouseButtons.Right || (selected == null) || (e.Button == MouseButtons.Left && selected.NumIcons == 1))
            {
                SelectedIcon = null;
                return;
            }
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            Point templateTypeMouseCell = templateTypeNavigationWidget.MouseCell;
            // Generated thumbnail may differ from internal IconHeight and IconWidth on groups and randomisable 1x1s,
            // so use the specifically-saved icon sizes of the generated thumbnail.
            int width = selected.ThumbnailIconWidth;
            int height = selected.ThumbnailIconHeight;
            int x = templateTypeMouseCell.X;
            int y = templateTypeMouseCell.Y;
            if ((x >= 0) && (x < width))
            {
                if ((y >= 0) && (y < height))
                {
                    if (selected.IsRandom)
                    {
                        if (y * width + x < selected.NumIcons)
                        {
                            SelectedIcon = templateTypeMouseCell;
                        }
                    }
                    else
                    {
                        if (selected.IconMask == null || selected.IconMask[y, x])
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
            bool isRandom = selected.IsRandom && selected.NumIcons > 1;
            if (SelectedIcon.HasValue || isRandom)
            {
                int width = selected.ThumbnailIconWidth;
                int height = selected.ThumbnailIconHeight;
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
                    using (Pen selectedIconPen = new Pen(Color.LightSkyBlue, Math.Max(1, scale / 16)))
                    {
                        Size cellSize = new Size(scale, scale);
                        for (int y = 0; y < height; ++y)
                        {
                            for (int x = 0; x < width; ++x)
                            {
                                if (icon >= selected.NumIcons)
                                {
                                    break;
                                }
                                Rectangle rect = new Rectangle(new Point(padX + x * cellSize.Width, padY + y * cellSize.Height), cellSize);
                                e.Graphics.DrawRectangle(selectedIconPen, rect);
                                icon++;
                            }
                        }
                    }
                }
                if (SelectedIcon.HasValue)
                {
                    using (Pen selectedIconPen = new Pen(Color.Yellow, Math.Max(1, scale / 16)))
                    {
                        Size cellSize = new Size(scale, scale);
                        Rectangle rect = new Rectangle(new Point(padX + SelectedIcon.Value.X * cellSize.Width, padY + SelectedIcon.Value.Y * cellSize.Height), cellSize);
                        e.Graphics.DrawRectangle(selectedIconPen, rect);
                    }
                }
            }
            StringFormat sizeStringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            string text = String.Format("{0} ({1}x{2})", selected.DisplayName, selected.IconWidth, selected.IconHeight);
            SizeF textSize = e.Graphics.MeasureString(text, SystemFonts.CaptionFont) + new SizeF(6.0f, 6.0f);
            RectangleF textBounds = new RectangleF(new PointF(0, 0), textSize);
            using (SolidBrush sizeBackgroundBrush = new SolidBrush(Color.FromArgb(128, Color.Black)))
            using (SolidBrush sizeTextBrush = new SolidBrush(Color.White))
            {
                e.Graphics.FillRectangle(sizeBackgroundBrush, textBounds);
                e.Graphics.DrawString(text, SystemFonts.CaptionFont, sizeTextBrush, textBounds, sizeStringFormat);
            }
        }

        private void TemplateTool_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                if (Control.ModifierKeys == (Keys.Control | Keys.Shift))
                {
                    EnterFillMode();
                }
                else if (Control.ModifierKeys == Keys.Shift)
                {
                    EnterPlacementMode();
                }
                else if (boundsMode)
                {
                    ExitAllModes();
                }
            }
            else if (e.KeyCode == Keys.ControlKey)
            {
                if (Control.ModifierKeys == (Keys.Control | Keys.Shift))
                {
                    EnterFillMode();
                }
                else if (Control.ModifierKeys == Keys.Control)
                {
                    EnterBoundsMode();
                }
                else if (placementMode)
                {
                    ExitAllModes();
                }
            }
            else
            {
                CheckSelectShortcuts(e);
            }
        }

        private void TemplateTool_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.ShiftKey) || (e.KeyCode == Keys.ControlKey))
            {
                CommitEdgeDrag();
                CommitTileChanges(false);
                ExitAllModes();
                if (Control.ModifierKeys == Keys.Control)
                {
                    EnterBoundsMode();
                }
                else if (Control.ModifierKeys == Keys.Shift)
                {
                    EnterPlacementMode();
                }
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
                if (e.Button == MouseButtons.Left)
                {
                    dragEdge = DetectDragEdge(false);
                    Point startPoint = navigationWidget.MouseCell;
                    dragStartPoint = map.Bounds.Contains(startPoint) ? (Point?)startPoint : null;
                    dragStartBounds = dragBounds;
                    UpdateStatus();
                }
            }
            else if (placementMode)
            {
                HandlePlace(e.Button);
            }
            else if (fillMode)
            {
                HandleFill(e.Button);
            }
            else if ((e.Button == MouseButtons.Left) || (e.Button == MouseButtons.Right))
            {
                PickTemplate(navigationWidget.ActualMouseCell, e.Button == MouseButtons.Left);
            }
        }

        private void HandlePlace(MouseButtons button)
        {
            TemplateType selected = SelectedTemplateType;
            if (button == MouseButtons.Left)
            {
                if (selected == null || selected.Flags.HasFlag(TemplateTypeFlag.Clear))
                {
                    RemoveTemplate(navigationWidget.ActualMouseCell);
                }
                else
                {
                    SetTemplate(navigationWidget.ActualMouseCell, false);
                }
            }
            else if (button == MouseButtons.Right)
            {
                RemoveTemplate(navigationWidget.ActualMouseCell);
            }
        }

        private void HandleFill(MouseButtons button)
        {
            TemplateType selected = SelectedTemplateType;
            if (selected == null)
            {
                return;
            }
            Point? icon = SelectedIcon;
            bool clear = button == MouseButtons.Right;
            bool place = button == MouseButtons.Left;
            // Since is this now ordered logically as [y,x] it might be a good idea to flatten it and do icon lookups.
            bool[,] mask = selected.IconMask;
            bool is1x1 = icon.HasValue || (selected.IconWidth == 1 && selected.IconHeight == 1);
            if ((!place && !clear) || (place && clear))
            {
                return;
            }
            Point currentCell = navigationWidget.MouseCell;
            // Determine which types to include in the fill detection
            HashSet<TemplateType> templatesToFind = new HashSet<TemplateType>();
            if (place || is1x1)
            {
                TemplateType toFind = map.Templates[currentCell]?.Type;
                if (toFind != null && toFind.Flags.HasFlag(TemplateTypeFlag.IsGrouped) && toFind.GroupTiles.Length == 1)
                {
                    string owningType = toFind.GroupTiles[0];
                    TemplateType group = map.TemplateTypes.Where(t => t.Name.Equals(owningType, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    templatesToFind.UnionWith(map.TemplateTypes.Where(t => group.GroupTiles.Contains(t.Name, StringComparer.OrdinalIgnoreCase)));
                }
                else
                {
                    templatesToFind.Add(toFind);
                }
            }
            else if (clear)
            {
                // For Clear, it'll evaluate all cell types under the full selected tile, and clear all adjacent tiles of all those types.
                List<TemplateType> typesToFind = new List<TemplateType>();
                for (int y = 0; y < selected.IconHeight; ++y)
                {
                    for (int x = 0; x < selected.IconWidth; ++x)
                    {
                        Point toCheck = new Point(x + currentCell.X, y + currentCell.Y);
                        if (mask[y, x] && map.Metrics.GetCell(toCheck, out int cell))
                        {
                            typesToFind.Add(map.Templates[cell]?.Type);
                        }
                    }
                }
                // Detect any group types inside it and get the full list.
                foreach (TemplateType tp in typesToFind)
                {
                    if (tp != null && tp.Flags.HasFlag(TemplateTypeFlag.IsGrouped) && tp.GroupTiles.Length == 1)
                    {
                        string owningType = tp.GroupTiles[0];
                        TemplateType group = map.TemplateTypes.Where(t => t.Name.Equals(owningType, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                        templatesToFind.UnionWith(map.TemplateTypes.Where(t => group.GroupTiles.Contains(t.Name, StringComparer.OrdinalIgnoreCase)));
                    }
                    else
                    {
                        templatesToFind.Add(tp);
                    }
                }
            }
            // Transform map data to simpler array.
            int mapWidth = map.Metrics.Width;
            int mapHeight = map.Metrics.Height;
            Template[] scanData = new Template[mapHeight * mapWidth];
            foreach ((int cell, Template t) in map.Templates)
            {
                scanData[cell] = t;
            }
            // Determine which cells to perform the blob search on.
            List<Point> detectPoints = new List<Point>();
            if (place || is1x1)
            {
                // Placement mode: only fill from the actual mouse point.
                detectPoints.Add(currentCell);
            }
            else
            {
                // Clear mode: fill with all cells in the grid that are in the icon mask.
                for (int y = 0; y < selected.IconHeight; ++y)
                {
                    for (int x = 0; x < selected.IconWidth; ++x)
                    {
                        Point toAdd = new Point(x + currentCell.X, y + currentCell.Y);
                        if (mask[y, x] && map.Metrics.Contains(toAdd))
                        {
                            detectPoints.Add(toAdd);
                        }
                    }
                }
            }
            // Actual flood fill part.
            Rectangle bounds = map.Bounds;
            int insideBounds = detectPoints.Count(dp => bounds.Contains(dp));
            // Null means some points are outside, some are inside. It disables the whole bounds logic.
            bool? inBounds = insideBounds > 0 && insideBounds < detectPoints.Count ? null : (bool?)(insideBounds != 0);
            bool validCell(Template[] mapData, int yVal, int xVal)
            {
                if (Globals.BoundsObstructFill && inBounds.HasValue)
                {
                    bool inB = inBounds.Value;
                    bool pointInBounds = bounds.Contains(xVal, yVal);
                    if ((inB && !pointInBounds) || (!inB && pointInBounds))
                    {
                        return false;
                    }
                }
                return templatesToFind.Contains(mapData[mapWidth * yVal + xVal]?.Type);
            }
            HashSet<Point> fillPoints = new HashSet<Point>();
            foreach (Point p in detectPoints)
            {
                if (!fillPoints.Contains(p))
                {
                    List<Point> blobPoints = BlobDetection.MakeBlobForPoint(p.X, p.Y, scanData, mapWidth, mapHeight,
                        validCell, false, false, null, out _);
                    // Should never be null; all points return at least themselves.
                    if (blobPoints != null)
                    {
                        fillPoints.UnionWith(blobPoints);
                    }
                }
            }
            undoTemplates.Clear();
            redoTemplates.Clear();
            // Prevent it from placing down actual clear terrain.
            bool isClear = clear || selected.Flags.HasFlag(TemplateTypeFlag.Clear);
            if (isClear || icon.HasValue || is1x1)
            {
                foreach (Point fill in fillPoints)
                {
                    if (map.Metrics.GetCell(fill, out int cell))
                    {
                        if (isClear)
                        {
                            undoTemplates[cell] = map.Templates[fill];
                            map.Templates[cell] = null;
                            redoTemplates[cell] = null;
                        }
                        else
                        {
                            SetTemplate(fill, true);
                        }
                    }
                }
                mapPanel.Invalidate(map, fillPoints);
            }
            else if (place)
            {
                int height = selected.IconHeight;
                int width = selected.IconWidth;
                // get offset
                int originX = currentCell.X % width;
                int originY = currentCell.Y % height;
                List<Point> refreshList = new List<Point>();
                foreach (Point p in fillPoints)
                {
                    int diffX = p.X % width - originX;
                    if (diffX < 0)
                        diffX += width;
                    int diffY = p.Y % height - originY;
                    if (diffY < 0)
                        diffY += height;
                    int paintIcon = diffY * width + diffX;
                    if (!mask[diffY, diffX])
                    {
                        continue;
                    }
                    refreshList.Add(p);
                    Template tmp = new Template()
                    {
                        Type = selected,
                        Icon = paintIcon,
                    };
                    if (map.Metrics.GetCell(p, out int cell))
                    {
                        undoTemplates[cell] = map.Templates[cell];
                        map.Templates[cell] = tmp;
                        redoTemplates[cell] = tmp;
                    }
                }
                // Exclude nonexistent tiles that didn't get replaced from the refresh.
                mapPanel.Invalidate(map, refreshList);
            }
            CommitTileChanges(true);
        }

        public static string RandomizeTiles(IGamePlugin plugin, MapPanel mapPanel, UndoRedoList<UndoRedoEventArgs, ToolType> url)
        {
            Random rnd = new Random();
            Dictionary<int, Template> undoTemplates = new Dictionary<int, Template>();
            Dictionary<int, Template> redoTemplates = new Dictionary<int, Template>();
            Map map = plugin.Map;
            if (map.TemplateTypes.Where(t => t.ExistsInTheater && t.IsRandom).Count() == 0)
            {
                return "This map's theater does not contain randomizable tiles.";
            }
            int mapLength = map.Metrics.Length;
            int mapWidth = map.Metrics.Width;
            for (int i = 0; i < mapLength; ++i)
            {
                Point location = new Point(i / mapWidth, i % mapWidth);
                // For grouped tiles, PickTemplate will return the Group, not the individual tile. So RandomCell is enabled on the result.
                TemplateType cur = PickTemplate(map, location, true, out _);
                if (cur == null || !cur.IsRandom)
                {
                    continue;
                }
                SetTemplate(map, cur, location, null, undoTemplates, redoTemplates, rnd, true);
            }
            mapPanel.Invalidate(map, redoTemplates.Keys);
            int count = undoTemplates.Count;
            // This clears the undo/redo lists, so refresh and count needs to be taken first.
            CommitTileChanges(url, undoTemplates, redoTemplates, plugin);
            return String.Format("{0} cells replaced.", count);
        }

        private void MapPanel_MouseUp(object sender, MouseEventArgs e)
        {
            CommitEdgeDrag();
            CommitTileChanges(false);
        }

        private void MapPanel_MouseLeave(object sender, EventArgs e)
        {
            ExitAllModes();
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
            else if (!boundsMode && (Control.ModifierKeys == Keys.Control))
            {
                EnterBoundsMode();
            }
            else if (!fillMode && (Control.ModifierKeys == (Keys.Control | Keys.Shift)))
            {
                EnterFillMode();
            }
            else if ((placementMode || boundsMode) && (Control.ModifierKeys == Keys.None))
            {
                ExitAllModes();
            }
            CheckBoundsCursor();
            if (!boundsMode)
            {
                mouseTooltip.Hide(mapPanel);
            }
        }

        private void CheckBoundsCursor()
        {
            Cursor cursor = Cursors.Default;
            if (boundsMode)
            {
                switch (dragEdge != FacingType.None ? dragEdge : DetectDragEdge(dragStartPoint.HasValue))
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
                    case FacingType.None:
                        if (map.Bounds.Contains(navigationWidget.MouseCell) || (dragStartPoint.HasValue && dragStartBounds.HasValue))
                        {
                            cursor = Cursors.SizeAll;
                        }
                        break;
                }
                // NavigationWidget manages all mouse cursor changes, so tools don't conflict with each other.
                if (navigationWidget.CurrentCursor != cursor && navigationWidget.MouseInBounds)
                {
                    navigationWidget.CurrentCursor = cursor;
                    UpdateTooltip();
                }
            }
        }

        private void MouseoverWidget_ClosestMouseCellBorderChanged(object sender, MouseCellChangedEventArgs e)
        {
            if (dragEdge == FacingType.None || e.MouseButtons != MouseButtons.Left)
            {
                return;
            }
            Point endDrag = e.NewCell;
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
            Point mouseCell = e.NewCell;
            if (boundsMode)
            {
                if (dragEdge == FacingType.None && dragStartPoint.HasValue && dragStartBounds.HasValue)
                {
                    int xDiff = dragStartPoint.Value.X - mouseCell.X;
                    int yDiff = dragStartPoint.Value.Y - mouseCell.Y;
                    int xMax = map.Metrics.Width - dragStartBounds.Value.Width - 1;
                    int yMax = map.Metrics.Height - dragStartBounds.Value.Height - 1;
                    dragBounds.X = Math.Max(1, Math.Min(dragStartBounds.Value.X - xDiff, xMax));
                    dragBounds.Y = Math.Max(1, Math.Min(dragStartBounds.Value.Y - yDiff, yMax));
                    UpdateTooltip();
                }
                return;
            }
            mouseTooltip.Hide(mapPanel);
            if (!placementMode && !fillMode)
            {
                if ((e.MouseButtons == MouseButtons.Left) || (e.MouseButtons == MouseButtons.Right))
                {
                    PickTemplate(mouseCell, e.MouseButtons == MouseButtons.Left);
                }
                return;
            }
            // If mouse button is pressed, place.
            // Fill mode is not handled in drag-click. It is a single-click operation.
            if (placementMode)
            {
                HandlePlace(e.MouseButtons);
            }
            // Handle refresh necessary for normal mouse movement of preview template.
            TemplateType selected = SelectedTemplateType;
            if (selected == null)
            {
                return;
            }
            foreach (Point location in new Point[] { e.OldCell, e.NewCell })
            {
                if (SelectedIcon.HasValue)
                {
                    mapPanel.Invalidate(map, new Point(location.X, location.Y));
                }
                else
                {
                    for (int y = 0; y < selected.IconHeight; ++y)
                    {
                        for (int x = 0; x < selected.IconWidth; ++x)
                        {
                            if (selected.IconMask != null && !selected.IconMask[y, x])
                            {
                                continue;
                            }
                            mapPanel.Invalidate(map, new Point(location.X + x, location.Y + y));
                        }
                    }
                }
            }
        }

        protected override void RefreshPreviewPanel()
        {
            if (templateTypeNavigationWidget != null)
            {
                templateTypeNavigationWidget.Dispose();
                templateTypeNavigationWidget = null;
            }
            TemplateType selected = SelectedTemplateType;
            Image oldImage = templateTypeMapPanel.MapImage;
            bool renderGrid = this.Layers.HasFlag(MapLayerFlag.LandTypes);
            if (selected != null)
            {
                // Special case: tiles that are not initialised, but are present on the map. Initialise with forced dummy generation.
                // This is really only for the tile FF "research mode" on RA maps, or if FilterTheaterObjects is disabled.
                if (selected.Thumbnail == null || !selected.Initialised)
                {
                    selected.Init(plugin.GameInfo, plugin.Map.Theater, true, false);
                }
                CellMetrics templateTypeMetrics = new CellMetrics(selected.ThumbnailIconWidth, selected.ThumbnailIconHeight);
                CellGrid<Template> templates = null;
                HashSet<Point> ignoredPoints = new HashSet<Point>();
                if (renderGrid)
                {
                    // Fill "dummy map" to apply land types grid to it.
                    templates = new CellGrid<Template>(templateTypeMetrics);
                    int cell = 0;
                    bool isRandom = selected.IsRandom;
                    for (int y = 0; y < selected.ThumbnailIconHeight; ++y)
                    {
                        for (int x = 0; x < selected.ThumbnailIconWidth; ++x)
                        {
                            if ((!isRandom && selected.IconMask[y, x]) || (isRandom && cell < selected.NumIcons))
                            {
                                SetTemplate(map.TemplateTypes, templates, selected, new Point(x, y), new Point(x, y), null, null, this.random, false);
                            }
                            else
                            {
                                ignoredPoints.Add(new Point(x, y));
                            }
                            cell++;
                        }
                    }
                }
                templateTypeNavigationWidget = new NavigationWidget(templateTypeMapPanel, templateTypeMetrics, Globals.PreviewTileSize, false);
                templateTypeNavigationWidget.MouseoverSize = Size.Empty;
                templateTypeNavigationWidget.Activate();
                Bitmap templatePreview = new Bitmap(selected.ThumbnailIconWidth * Globals.PreviewTileWidth, selected.ThumbnailIconHeight * Globals.PreviewTileHeight);
                templatePreview.SetResolution(96, 96);
                using (Graphics g = Graphics.FromImage(templatePreview))
                {
                    MapRenderer.SetRenderSettings(g, Globals.PreviewSmoothScale);
                    g.DrawImage(selected.Thumbnail, new Rectangle(Point.Empty, selected.Thumbnail.Size), 0, 0, templatePreview.Width, templatePreview.Height, GraphicsUnit.Pixel);
                    if (templates != null)
                    {
                        MapRenderer.RenderHashAreas(g, plugin, templates, null, null, Globals.PreviewTileSize, templateTypeMetrics.Bounds, ignoredPoints, true, false);
                    }
                }
                // paint selected.Thumbnail;
                templateTypeMapPanel.MapImage = templatePreview;
            }
            else
            {
                templateTypeMapPanel.MapImage = null;
            }
            if (oldImage != null)
            {
                try { oldImage.Dispose(); }
                catch { /* ignore */ }
            }
            templateTypeMapPanel.Invalidate();
        }

        private void SetTemplate(Point location, bool skipInvalidate)
        {
            // If dragging a multi-tile template, only place a new one if nothing overlaps with previously-placed tiles from the same drag operation.
            TemplateType selected = SelectedTemplateType;
            bool[,] selectedMask = selected.IconMask;
            if (SelectedIcon == null && Globals.TileDragProtect && !selected.IsRandom)
            {
                for (int y = 0, icon = 0; y < selected.IconHeight; ++y)
                {
                    for (int x = 0; x < selected.IconWidth; ++x, ++icon)
                    {
                        if (selectedMask != null && !selectedMask[y, x])
                        {
                            continue;
                        }
                        Point subLocation = new Point(location.X + x, location.Y + y);
                        if (map.Metrics.GetCell(subLocation, out int cell) && redoTemplates.ContainsKey(cell))
                        {
                            return;
                        }
                    }
                }
                // Consider placing an alternate if something is already placed, and the selected tile has alternates.
                if (Globals.TileDragRandomize && redoTemplates.Count > 0
                    && selected.Flags.HasFlag(TemplateTypeFlag.HasEquivalents)
                    && selected.GroupTiles != null && selected.GroupTiles.Length > 0)
                {
                    // Remove last-placed from possible placed tiles so each new placed one is unique.
                    List<string> alts = selected.GroupTiles.Where(x => x != null && !x.Equals(lastPlaced, StringComparison.OrdinalIgnoreCase)).ToList();
                    // If there is only one alternate, just randomise between both, to avoid getting a repeating alternating pattern.
                    if (alts.Count == 1)
                    {
                        alts.Add(selected.Name);
                    }
                    int entry = random.Next(0, alts.Count);
                    string tile = alts[entry];
                    TemplateType newtile = this.map.TemplateTypes.FirstOrDefault(tt => tile.Equals(tt.Name, StringComparison.OrdinalIgnoreCase));
                    if (newtile != null)
                    {
                        // Adjust to offsets.
                        location = new Point(location.X - selected.EquivalentOffset.X + newtile.EquivalentOffset.X,
                                            location.Y - selected.EquivalentOffset.Y + newtile.EquivalentOffset.Y);
                        selected = newtile;
                    }
                }
            }
            Dictionary<int, Template> addedRedoTemplates = new Dictionary<int, Template>();
            SetTemplate(map, selected, location, SelectedIcon, undoTemplates, addedRedoTemplates, random, false);
            lastPlaced = selected.Name;
            // Merge with main redoTemplates list.
            addedRedoTemplates.ToList().ForEach(kv => redoTemplates[kv.Key] = kv.Value);
            if (!skipInvalidate)
            {
                mapPanel.Invalidate(map, addedRedoTemplates.Keys);
            }
        }

        public static void SetTemplate(Map map, TemplateType selected, Point location, Point? selectedIcon,
            Dictionary<int, Template> undoTemplates, Dictionary<int, Template> redoTemplates, Random randomiser, bool diversify)
        {
            SetTemplate(map.TemplateTypes, map.Templates, selected, location, selectedIcon, undoTemplates, redoTemplates, randomiser, diversify);
        }

        public static void SetTemplate(List<TemplateType> templateTypes, CellGrid<Template> templates, TemplateType selected, Point location, Point? selectedIcon,
            Dictionary<int, Template> undoTemplates, Dictionary<int, Template> redoTemplates, Random randomiser, bool diversify)
        {
            if (selected == null)
            {
                return;
            }
            bool isGroup = selected.Flags.HasFlag(TemplateTypeFlag.Group);
            if (selectedIcon.HasValue)
            {
                if (templates.Metrics.GetCell(location, out int cell))
                {
                    if (undoTemplates != null && !undoTemplates.ContainsKey(cell))
                    {
                        undoTemplates[cell] = templates[location];
                    }
                    int icon = (selectedIcon.Value.Y * selected.ThumbnailIconWidth) + selectedIcon.Value.X;
                    TemplateType placeType = selected;
                    if (isGroup)
                    {
                        placeType = templateTypes.Where(t => t.Name == selected.GroupTiles[icon]).FirstOrDefault();
                        icon = 0;
                    }
                    Template placeTemplate = new Template { Type = placeType, Icon = icon };
                    templates[cell] = placeTemplate;
                    if (redoTemplates != null)
                    {
                        redoTemplates[cell] = placeTemplate;
                    }
                }
            }
            else
            {
                for (int y = 0, icon = 0; y < selected.IconHeight; ++y)
                {
                    for (int x = 0; x < selected.IconWidth; ++x, ++icon)
                    {
                        if (selected.IconMask != null && !selected.IconMask[y, x])
                        {
                            continue;
                        }
                        Point subLocation = new Point(location.X + x, location.Y + y);
                        if (templates.Metrics.GetCell(subLocation, out int cell))
                        {
                            if (undoTemplates != null && !undoTemplates.ContainsKey(cell))
                            {
                                undoTemplates[cell] = templates[subLocation];
                            }
                        }
                    }
                }
                bool isRandom = selected.IsRandom && selected.IconWidth == 1 && selected.IconHeight == 1;
                for (int y = 0, icon = 0; y < selected.IconHeight; ++y)
                {
                    for (int x = 0; x < selected.IconWidth; ++x, ++icon)
                    {
                        if (selected.IconMask != null && !selected.IconMask[y, x])
                        {
                            continue;
                        }
                        Point subLocation = new Point(location.X + x, location.Y + y);
                        if (templates.Metrics.GetCell(subLocation, out int cell))
                        {
                            int placeIcon;
                            TemplateType placeType = selected;
                            if (isGroup)
                            {
                                // diversify: when filling random tiles, check that the tiles above and to the left are different.
                                // This does not consider all 4 sides since they are randomised sequentially per cell number.
                                List<int> used = new List<int>();
                                if (diversify && selected.NumIcons > 4)
                                {
                                    Template adjNorth = templates.Adjacent(location, FacingType.North);
                                    if (adjNorth != null && adjNorth.Type.Flags.HasFlag(TemplateTypeFlag.IsGrouped)
                                        && adjNorth.Type.GroupTiles[0] == selected.Name)
                                    {
                                        used.Add(adjNorth.Type.ID);
                                    }
                                    Template adjWest = templates.Adjacent(location, FacingType.West);
                                    if (adjWest != null && adjWest.Type.Flags.HasFlag(TemplateTypeFlag.IsGrouped)
                                        && adjWest.Type.GroupTiles[0] == selected.Name)
                                    {
                                        used.Add(adjWest.Type.ID);
                                    }
                                }
                                int randomType = randomiser.Next(0, selected.NumIcons);
                                placeType = templateTypes.Where(t => t.Name == selected.GroupTiles[randomType]).FirstOrDefault();
                                placeIcon = 0;
                                while (used.Contains(placeType.ID))
                                {
                                    randomType = randomiser.Next(0, selected.NumIcons);
                                    placeType = templateTypes.Where(t => t.Name == selected.GroupTiles[randomType]).FirstOrDefault();
                                }
                            }
                            else if (isRandom)
                            {
                                // diversify: when filling random tiles, check that the tiles above and to the left are different.
                                // This does not consider all 4 sides since they are randomised sequentially per cell number.
                                List<int> used = new List<int>();
                                if (diversify && selected.NumIcons > 4)
                                {
                                    Template adjNorth = templates.Adjacent(location, FacingType.North);
                                    if (adjNorth != null && adjNorth.Type.ID == selected.ID)
                                    {
                                        used.Add(adjNorth.Icon);
                                    }
                                    Template adjWest = templates.Adjacent(location, FacingType.West);
                                    if (adjWest != null && adjWest.Type.ID == selected.ID)
                                    {
                                        used.Add(adjWest.Icon);
                                    }
                                }
                                placeIcon = randomiser.Next(0, selected.NumIcons);
                                while (used.Contains(placeIcon))
                                {
                                    placeIcon = randomiser.Next(0, selected.NumIcons);
                                }
                            }
                            else
                            {
                                placeIcon = icon;
                            }
                            Template template = new Template { Type = placeType, Icon = placeIcon };
                            templates[cell] = template;
                            if (redoTemplates != null)
                            {
                                redoTemplates[cell] = template;
                            }
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
                }
            }
            else
            {
                for (int y = 0, icon = 0; y < selected.IconHeight; ++y)
                {
                    for (int x = 0; x < selected.IconWidth; ++x, ++icon)
                    {
                        if (selected.IconMask != null && !selected.IconMask[y, x])
                        {
                            continue;
                        }
                        Point subLocation = new Point(location.X + x, location.Y + y);
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
                    for (int x = 0; x < selected.IconWidth; ++x, ++icon)
                    {
                        if (selected.IconMask != null && !selected.IconMask[y, x])
                        {
                            continue;
                        }
                        Point subLocation = new Point(location.X + x, location.Y + y);
                        if (map.Metrics.GetCell(subLocation, out int cell))
                        {
                            map.Templates[cell] = null;
                            redoTemplates[cell] = null;
                            mapPanel.Invalidate(map, cell);
                        }
                    }
                }
            }
        }

        private void CheckSelectShortcuts(KeyEventArgs e)
        {
            int maxVal = templateTypeListView.Items.Count - 1;
            int curVal = templateTypeListView.SelectedIndices.Count == 0 ? -1 : templateTypeListView.SelectedIndices[0];
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
                if (placementMode && SelectedTemplateType != null)
                {
                    InvalidateCurrentArea();
                }
                templateTypeListView.Items[newVal].Selected = true;
                templateTypeListView.Select();
                if (newVal == 0)
                {
                    // selecting "null" (Clear) doesn't automatically jump to the top, to prevent a misclick
                    // in the list from messing up the user's scroll position. So we need to do it manually.
                    templateTypeListView.EnsureVisible(templateTypeListView.SelectedIndices[0]);
                }
                TemplateType selected = SelectedTemplateType;
                if (placementMode && selected != null)
                {
                    InvalidateCurrentArea();
                }
            }
        }

        private void EnterFillMode()
        {
            if (fillMode)
            {
                return;
            }
            boundsMode = false;
            navigationWidget.CurrentCursor = Cursors.Default;
            dragEdge = FacingType.None;
            dragBounds = Rectangle.Empty;
            dragStartPoint = null;
            dragStartBounds = null;
            placementMode = false;
            fillMode = true;
            navigationWidget.MouseoverSize = Size.Empty;
            InvalidateCurrentArea();
            mapPanel.Invalidate();
            UpdateStatus();
        }

        private void EnterPlacementMode()
        {
            if (placementMode)
            {
                return;
            }
            boundsMode = false;
            navigationWidget.CurrentCursor = Cursors.Default;
            dragEdge = FacingType.None;
            dragBounds = Rectangle.Empty;
            dragStartPoint = null;
            dragStartBounds = null;
            fillMode = false;
            placementMode = true;
            navigationWidget.MouseoverSize = Size.Empty;
            InvalidateCurrentArea();
            mapPanel.Invalidate();
            UpdateStatus();
        }

        private void EnterBoundsMode()
        {
            if (boundsMode)
            {
                return;
            }
            fillMode = false;
            placementMode = false;
            boundsMode = true;
            dragBounds = map.Bounds;
            dragStartPoint = null;
            dragStartBounds = null;
            navigationWidget.MouseoverSize = Size.Empty;
            InvalidateCurrentArea();
            mapPanel.Invalidate();
            UpdateTooltip();
            UpdateStatus();
            CheckBoundsCursor();
        }

        private void ExitAllModes()
        {
            if (!placementMode && !boundsMode && !fillMode)
            {
                return;
            }
            boundsMode = false;
            navigationWidget.CurrentCursor = Cursors.Default;
            dragEdge = FacingType.None;
            dragBounds = Rectangle.Empty;
            dragStartPoint = null;
            dragStartBounds = null;
            placementMode = false;
            fillMode = false;
            navigationWidget.MouseoverSize = new Size(1, 1);
            InvalidateCurrentArea();
            mapPanel.Invalidate();
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
            for (int y = 0; y < selected.IconHeight; ++y)
            {
                for (int x = 0; x < selected.IconWidth; ++x)
                {
                    mapPanel.Invalidate(map, new Point(navigationWidget.MouseCell.X + x, navigationWidget.MouseCell.Y + y));
                }
            }
        }

        private void UpdateTooltip()
        {
            FacingType showEdge = dragEdge != FacingType.None ? dragEdge : DetectDragEdge(dragStartPoint.HasValue);
            if (boundsMode && (showEdge != FacingType.None || (dragStartPoint.HasValue && dragStartBounds.HasValue)))
            {
                string tooltip = String.Format("X = {0}\nY = {1}\nWidth = {2}\nHeight = {3}", dragBounds.Left, dragBounds.Top, dragBounds.Width, dragBounds.Height);
                Size textSize = TextRenderer.MeasureText(tooltip, SystemFonts.CaptionFont);
                Size tooltipSize = new Size(textSize.Width + 6, textSize.Height + 6);
                Point mouseCell = navigationWidget.MouseCell;
                Point tooltipPosition = Control.MousePosition;
                SizeF zoomedCell = navigationWidget.ZoomedCellSize;
                // Corrects to nearest border; should match NavigationWidget.ClosestMouseCellBorder
                if (showEdge != FacingType.None)
                {
                    if (navigationWidget.ClosestMouseCellBorder.Y > mouseCell.Y)
                    {
                        tooltipPosition.Y += (int)Math.Round((Globals.PixelWidth - navigationWidget.MouseSubPixel.Y) * zoomedCell.Height / Globals.PixelWidth);
                    }
                    else
                    {
                        tooltipPosition.Y -= (int)Math.Round(navigationWidget.MouseSubPixel.Y * zoomedCell.Height / Globals.PixelWidth);
                    }
                    if (navigationWidget.ClosestMouseCellBorder.X > mouseCell.X)
                    {
                        tooltipPosition.X += (int)Math.Round((Globals.PixelWidth - navigationWidget.MouseSubPixel.X) * zoomedCell.Width / Globals.PixelWidth);
                    }
                    else
                    {
                        tooltipPosition.X -= (int)Math.Round(navigationWidget.MouseSubPixel.X * zoomedCell.Width / Globals.PixelWidth);
                    }
                    switch (showEdge)
                    {
                        case FacingType.North:
                        case FacingType.NorthEast:
                        case FacingType.NorthWest:
                            tooltipPosition.Y += (int)zoomedCell.Height;
                            break;
                        case FacingType.South:
                        case FacingType.SouthEast:
                        case FacingType.SouthWest:
                            tooltipPosition.Y -= (int)zoomedCell.Height;
                            break;
                    }
                    switch (showEdge)
                    {
                        case FacingType.SouthWest:
                        case FacingType.West:
                        case FacingType.NorthWest:
                            tooltipPosition.X += (int)zoomedCell.Width;
                            break;
                        case FacingType.SouthEast:
                        case FacingType.East:
                        case FacingType.NorthEast:
                            tooltipPosition.X -= (int)zoomedCell.Width;
                            break;
                    }
                    switch (showEdge)
                    {
                        case FacingType.South:
                        case FacingType.SouthEast:
                        case FacingType.SouthWest:
                            tooltipPosition.Y -= tooltipSize.Height;
                            break;
                    }
                }
                else if (dragStartPoint.HasValue && dragStartBounds.HasValue)
                {
                    // Always towards the center, one cell away from the mouse.
                    Point center = dragBounds.CenterPoint();
                    if (mouseCell.X < center.X)
                        tooltipPosition.X += (int)zoomedCell.Width;
                    else
                        tooltipPosition.X -= (int)zoomedCell.Width + tooltipSize.Width;
                    if (mouseCell.Y < center.Y)
                        tooltipPosition.Y += (int)zoomedCell.Height;
                    else
                        tooltipPosition.Y -= (int)zoomedCell.Height + tooltipSize.Height;
                }
                switch (showEdge)
                {
                    case FacingType.SouthEast:
                    case FacingType.East:
                    case FacingType.NorthEast:
                        tooltipPosition.X -= tooltipSize.Width;
                        break;
                }
                Screen screen = Screen.FromControl(mapPanel);
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
            SelectedTemplateType = PickTemplate(map, location, wholeTemplate, out Point? selectedIcon);
            SelectedIcon = selectedIcon;
        }

        public static TemplateType PickTemplate(Map map, Point location, bool wholeTemplate, out Point? selectedIcon)
        {
            selectedIcon = null;
            if (!map.Templates.Metrics.Contains(location))
            {
                return null;
            }
            Template template = map.Templates[location];
            TemplateType picked = null;
            bool groupOwned = false;
            if (template != null)
            {
                if (template.Type.Flags.HasFlag(TemplateTypeFlag.IsGrouped) && template.Type.GroupTiles.Length == 1)
                {
                    groupOwned = true;
                    string owningType = template.Type.GroupTiles[0];
                    picked = map.TemplateTypes.Where(t => t.Name.Equals(owningType, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                }
                else
                {
                    picked = template.Type;
                }
            }
            else
            {
                picked = map.TemplateTypes.Where(t => t.Name.Equals("clear1", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }
            TemplateType selected = picked;
            bool isRandom = selected.IsRandom && selected.NumIcons > 1;
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
                int width = selected.ThumbnailIconWidth;
                selectedIcon = new Point(icon % width, icon / width);
            }
            else
            {
                selectedIcon = null;
            }
            return picked;
        }

        private FacingType DetectDragEdge(bool isDragging)
        {
            Point mouseCell = navigationWidget.ClosestMouseCellBorder;
            Point realMouseCell = navigationWidget.MouseCell;
            int mousePixelDistanceY = navigationWidget.MouseSubPixel.Y;
            if (mousePixelDistanceY > Globals.PixelHeight / 2)
            {
                mousePixelDistanceY = Globals.PixelHeight - mousePixelDistanceY;
            }
            int mousePixelDistanceX = navigationWidget.MouseSubPixel.X;
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
            if (!isDragging && (!inBoundsX || !inBoundsY))
                return FacingType.None;
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
            HashSet<Point> updCells = null;
            if (dragBounds != map.Bounds)
            {
                Rectangle oldBounds = map.Bounds;
                Rectangle newBounds = dragBounds;
                updCells = GetResourceUpdateCells(oldBounds, newBounds);
                if (updCells.Count == 0)
                {
                    updCells = null;
                }
                bool origEmptyState = plugin.Empty;
                plugin.Dirty = true;
                void undoAction(UndoRedoEventArgs ev)
                {
                    ev.Map.Bounds = oldBounds;
                    if (updCells == null)
                    {
                        ev.MapPanel.Invalidate();
                    }
                    else
                    {
                        // Tools that paint from a cloned map update this automatically, but not all tools use a cloned map, and undo/redo
                        // actions can happen after switching to a different tool. Also better to just have it correct in the real map.
                        ev.Map.UpdateResourceOverlays(updCells, true);
                        ev.MapPanel.Invalidate(ev.Map, updCells);
                    }
                    if (ev.Plugin != null)
                    {
                        ev.Plugin.Empty = origEmptyState;
                        ev.Plugin.Dirty = !ev.NewStateIsClean;
                    }
                }
                void redoAction(UndoRedoEventArgs ev)
                {
                    ev.Map.Bounds = newBounds;
                    if (updCells == null)
                    {
                        ev.MapPanel.Invalidate();
                    }
                    else
                    {
                        // Tools that paint from a cloned map update this automatically, but not all tools use a cloned map, and undo/redo
                        // actions can happen after switching to a different tool. Also better to just have it correct in the real map.
                        ev.Map.UpdateResourceOverlays(updCells, true);
                        ev.MapPanel.Invalidate(ev.Map, updCells);
                    }
                    if (ev.Plugin != null)
                    {
                        // Redo can never restore the "empty" state, but CAN be the point at which a save was done.
                        ev.Plugin.Empty = false;
                        ev.Plugin.Dirty = !ev.NewStateIsClean;
                    }
                }
                map.Bounds = newBounds;
                url.Track(undoAction, redoAction, ToolType.Map);
            }
            dragEdge = FacingType.None;
            dragStartPoint = null;
            dragStartBounds = null;
            UpdateStatus();
            if (updCells != null)
            {
                map.UpdateResourceOverlays(updCells, true);
                mapPanel.Invalidate(map, updCells);
            }
            else
            {
                mapPanel.Invalidate();
            }
        }

        private void CommitTileChanges(bool noCheck)
        {
            if (!noCheck && !placementMode)
            {
                return;
            }
            CommitTileChanges(this.url, this.undoTemplates, this.redoTemplates, plugin);
        }

        private static void CommitTileChanges(UndoRedoList<UndoRedoEventArgs, ToolType> url, Dictionary<int, Template> undoTemplates, Dictionary<int, Template> redoTemplates, IGamePlugin plugin)
        {
            if (undoTemplates.Count == 0 || redoTemplates.Count == 0)
            {
                return;
            }
            Dictionary<int, Template> undoTemplates2 = new Dictionary<int, Template>(undoTemplates);
            bool origEmptyState = plugin.Empty;
            plugin.Dirty = true;
            void undoAction(UndoRedoEventArgs ev)
            {
                foreach (KeyValuePair<int, Template> kv in undoTemplates2)
                {
                    ev.Map.Templates[kv.Key] = kv.Value;
                }
                ev.MapPanel.Invalidate(ev.Map, undoTemplates2.Keys);
                if (ev.Plugin != null)
                {
                    ev.Plugin.Empty = origEmptyState;
                    ev.Plugin.Dirty = !ev.NewStateIsClean;
                }
            }
            Dictionary<int, Template> redoTemplates2 = new Dictionary<int, Template>(redoTemplates);
            void redoAction(UndoRedoEventArgs ev)
            {
                foreach (KeyValuePair<int, Template> kv in redoTemplates2)
                {
                    ev.Map.Templates[kv.Key] = kv.Value;
                }
                ev.MapPanel.Invalidate(ev.Map, redoTemplates2.Keys);
                if (ev.Plugin != null)
                {
                    // Redo can never restore the "empty" state, but CAN be the point at which a save was done.
                    ev.Plugin.Empty = false;
                    ev.Plugin.Dirty = !ev.NewStateIsClean;
                }
            }
            undoTemplates.Clear();
            redoTemplates.Clear();
            url.Track(undoAction, redoAction, ToolType.Map);
        }

        public override void UpdateStatus()
        {
            if (placementMode)
            {
                statusLbl.Text = "Left-Click to place template, Right-Click to clear template";
            }
            else if (fillMode)
            {
                statusLbl.Text = "Left-Click to fill all tiles adjacent to the mouse cell of the type under the mouse cell, Right-Click to clear all adjacent tiles of all types under the selected template's cells.";
            }
            else if (boundsMode)
            {
                if (dragEdge != FacingType.None)
                {
                    statusLbl.Text = "Release left button to end dragging map bounds edge";
                }
                else if (dragStartPoint.HasValue && dragStartBounds.HasValue)
                {
                    statusLbl.Text = "Release left button to end dragging map bounds";
                }
                else
                {
                    statusLbl.Text = "Left-Click inside the bounds or on a map bounds edge to start dragging";
                }
            }
            else
            {
                statusLbl.Text = "Shift to enter placement mode, Ctrl to enter map bounds mode, Ctrl-Shift to enter fill mode, Left-Click to pick whole template, Right-Click to pick individual template tile";
            }
        }

        protected override void PreRenderMap()
        {
            base.PreRenderMap();
            previewMap = map.Clone(true);
            if (!placementMode && !fillMode)
            {
                return;
            }
            TemplateType selected = SelectedTemplateType;
            if (selected == null)
            {
                return;
            }
            Point location = navigationWidget.ActualMouseCell;
            if (SelectedIcon.HasValue || selected.IsRandom)
            {
                // When placing a random tile, always place first icon, otherwise it shows as random tile,
                // and the user might think that that specific shown random tile is what will be placed down.
                Point? placeIcon = SelectedIcon ?? new Point(0, 0);
                SetTemplate(map.TemplateTypes, previewMap.Templates, selected, location, placeIcon, null, null, this.random, false);
            }
            else
            {
                int icon = 0;
                for (int y = 0; y < selected.IconHeight; ++y)
                {
                    for (int x = 0; x < selected.IconWidth; ++x, ++icon)
                    {
                        if (selected.IconMask != null && !selected.IconMask[y, x])
                        {
                            continue;
                        }
                        Point subLocation = new Point(location.X + x, location.Y + y);
                        SetTemplate(map.TemplateTypes, previewMap.Templates, selected, subLocation, new Point(x, y), null, null, this.random, false);
                    }
                }
            }
        }

        protected override void PostRenderMap(Graphics graphics, Rectangle visibleCells)
        {
            base.PostRenderMap(graphics, visibleCells);
            HashSet<Point> placementArea = null;
            HashSet<Point> placementAreaClear = null;
            Rectangle placementRect = Rectangle.Empty;
            if (placementMode || fillMode)
            {
                placementArea = new HashSet<Point>();
                placementAreaClear = new HashSet<Point>();
                Point location = navigationWidget.ActualMouseCell;
                TemplateType selected = SelectedTemplateType;
                if (selected == null || SelectedIcon.HasValue)
                {
                    placementRect = new Rectangle(location.X, location.Y, 1, 1);
                    placementArea.Add(location);
                }
                else
                {
                    placementRect = new Rectangle(location.X, location.Y, selected.IconWidth, selected.IconHeight);
                    IEnumerable<Point> points = selected.GetIconPoints(location);
                    placementArea.UnionWith(points);
                    placementAreaClear.UnionWith(placementRect.Points().Where(p => !placementArea.Contains(p)));
                }
            }
            if (Layers.HasAnyFlags(MapLayerFlag.LandTypes | MapLayerFlag.TechnoOccupancy))
            {
                bool renderTechnos = Layers.HasFlag(MapLayerFlag.TechnoOccupancy);
                OccupierSet<ICellOccupier> technos = renderTechnos ? previewMap.Technos : null;
                OccupierSet<ICellOccupier> buildings = renderTechnos ? previewMap.Buildings : null;
                CellGrid<Template> templates = Layers.HasFlag(MapLayerFlag.LandTypes) ? previewMap.Templates : null;
                MapRenderer.RenderHashAreas(graphics, plugin, templates, technos, buildings, Globals.MapTileSize, visibleCells, placementArea, false, false);
            }
            if (placementMode || fillMode)
            {
                CellGrid<Template> templates = previewMap.Templates;
                bool isExtra = !Layers.HasFlag(MapLayerFlag.LandTypes);
                MapRenderer.RenderHashAreas(graphics, plugin, templates, null, null, Globals.MapTileSize, placementRect, placementAreaClear, true, isExtra);
            }
            if (boundsMode)
            {
                MapRenderer.RenderMapSymmetry(graphics, dragBounds, Globals.MapTileSize, Color.Red);
                MapRenderer.RenderMapBoundaries(graphics, dragBounds, visibleCells, Globals.MapTileSize, Color.Red);
            }
            else
            {
                if (Layers.HasFlag(MapLayerFlag.Boundaries))
                {
                    MapRenderer.RenderMapBoundaries(graphics, map, visibleCells, Globals.MapTileSize);
                }
                if (placementMode || fillMode)
                {
                    Point location = navigationWidget.ActualMouseCell;
                    TemplateType selected = SelectedTemplateType;
                    int selWidth = 1;
                    int selHeight = 1;
                    if (selected != null && !SelectedIcon.HasValue)
                    {
                        selWidth = selected.IconWidth;
                        selHeight = selected.IconHeight;
                    }
                    Rectangle singleCell = new Rectangle(
                        location.X * Globals.MapTileWidth, location.Y * Globals.MapTileHeight,
                        Globals.MapTileWidth, Globals.MapTileHeight);
                    Rectangle previewBounds = new Rectangle(
                        location.X * Globals.MapTileWidth,
                        location.Y * Globals.MapTileHeight,
                        selWidth * Globals.MapTileWidth,
                        selHeight * Globals.MapTileHeight
                    );
                    using (Pen previewPen = new Pen(fillMode ? Color.Red : Color.Green, Math.Max(1, Globals.MapTileSize.Width / 16.0f)))
                    {
                        graphics.DrawRectangle(previewPen, previewBounds);
                        // Special indicator to tell the user that the top-left cell has a special purpose:
                        // when pattern-filling, only this cell will serve as fill origin.
                        if (fillMode && (selWidth != 1 || selHeight != 1))
                        {
                            graphics.DrawRectangle(previewPen, singleCell);
                        }
                    }
                }
            }
            // Render this after the bounding box.
            if (Layers.HasFlag(MapLayerFlag.Waypoints | MapLayerFlag.HomeAreaBox) && plugin.Map.BasicSection.SoloMission)
            {
                MapRenderer.RenderHomeWayPointBox(graphics, plugin, map, visibleCells, Globals.MapTileSize, null, Color.Orange);
            }
        }

        public override void Activate()
        {
            base.Activate();
            this.Deactivate(true);
            this.mapPanel.MouseDown += MapPanel_MouseDown;
            this.mapPanel.MouseUp += MapPanel_MouseUp;
            this.mapPanel.MouseMove += MapPanel_MouseMove;
            this.mapPanel.MouseLeave += MapPanel_MouseLeave;
            this.mapPanel.MouseWheel += MapPanel_MouseWheel;
            this.mapPanel.LostFocus += MapPanel_MouseLeave;
            this.mapPanel.SuspendMouseZoomKeys = Keys.Control;
            (this.mapPanel as Control).KeyDown += TemplateTool_KeyDown;
            (this.mapPanel as Control).KeyUp += TemplateTool_KeyUp;
            this.navigationWidget.MouseCellChanged += MouseoverWidget_MouseCellChanged;
            this.navigationWidget.ClosestMouseCellBorderChanged += MouseoverWidget_ClosestMouseCellBorderChanged;
            this.templateTypeNavigationWidget?.Activate();
            this.url.Undone += Url_UndoRedoDone;
            this.url.Redone += Url_UndoRedoDone;
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
                this.ExitAllModes();
                base.Deactivate();
            }
            this.mapPanel.MouseDown -= MapPanel_MouseDown;
            this.mapPanel.MouseUp -= MapPanel_MouseUp;
            this.mapPanel.MouseMove -= MapPanel_MouseMove;
            this.mapPanel.MouseLeave -= MapPanel_MouseLeave;
            this.mapPanel.MouseWheel -= MapPanel_MouseWheel;
            this.mapPanel.LostFocus -= MapPanel_MouseLeave;
            this.mapPanel.SuspendMouseZoomKeys = Keys.None;
            (this.mapPanel as Control).KeyDown -= TemplateTool_KeyDown;
            (this.mapPanel as Control).KeyUp -= TemplateTool_KeyUp;
            this.navigationWidget.CurrentCursor = Cursors.Default;
            this.navigationWidget.ClosestMouseCellBorderChanged -= MouseoverWidget_ClosestMouseCellBorderChanged;
            this.navigationWidget.MouseCellChanged -= MouseoverWidget_MouseCellChanged;
            this.templateTypeNavigationWidget?.Deactivate();
            this.url.Undone -= Url_UndoRedoDone;
            this.url.Redone -= Url_UndoRedoDone;
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
                    this.templateTypeMapPanel.MouseMove -= TemplateTypeMapPanel_MouseMove;
                    this.templateTypeMapPanel.PostRender -= TemplateTypeMapPanel_PostRender;
                }
                disposedValue = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }

}
