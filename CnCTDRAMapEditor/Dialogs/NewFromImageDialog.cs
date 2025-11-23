//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                     Version 2, December 2004
//
//  Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
//  Everyone is permitted to copy and distribute verbatim or modified
//  copies of this license document, and changing it is allowed as long
//  as the name is changed.
//
//             DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//    TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//   0. You just DO WHAT THE FUCK YOU WANT TO.
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using MobiusEditor.Widgets;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class NewFromImageDialog : Form
    {

        private Dictionary<int, string> mappingsTemplate = new Dictionary<int, string>();
        private Dictionary<int, int> mappingsIcons = new Dictionary<int, int>();
        private NavigationWidget templateTypeNavigationWidget;
        private Bitmap finalImage;
        private IGamePlugin plugin;
        private Color? _selectedColor;
        private TemplateType _selectedTemplate;
        private Point? _selectedIcon;
        private int[] customColors { get; set; }

        private Color? SelectedColor
        {
            get
            {
                return _selectedColor;
            }
            set
            {
                if (_selectedColor == value)
                {
                    return;
                }
                lstTemplates.Enabled = true;
                templateTypeMapPanel.Enabled = true;
                lblColorVal.Text = value.HasValue ? "#" + ((uint)value.Value.ToArgb() & 0xFFFFFF).ToString("X6") : "-";
                Color foreCol = SystemColors.ControlText;
                Color backCol = SystemColors.Control;
                if (value.HasValue)
                {
                    Color col = value.Value;
                    foreCol = col;
                    backCol = col.GetBrightness() > 0.5 ? Color.Black : Color.White;
                }
                lblColorVal.ForeColor = foreCol;
                lblColorVal.BackColor = backCol;
                _selectedColor = value;
                int selectedCol;
                if (value.HasValue && mappingsTemplate.TryGetValue((selectedCol = value.Value.ToArgb()), out string tileInfo))
                {
                    this.plugin.Map.SplitTileInfo(tileInfo, out TemplateType template, out int tileIcon, null, true);
                    _selectedTemplate = template;
                    if (mappingsIcons.TryGetValue(selectedCol, out int cell))
                    {
                        _selectedIcon = template.GetIconPoint(cell);
                    }
                    else
                    {
                        _selectedIcon = null;
                    }
                    lstTemplates.SelectedItem = template.Name;
                    RefreshPreviewPanel();
                    SelectMapping();
                }
                else
                {
                    lstTemplates.SelectedIndex = 0;
                    _selectedIcon = null;
                    RefreshPreviewPanel();
                    SelectMapping();
                }
                lstTemplates.Enabled = value.HasValue;
                templateTypeMapPanel.Enabled = value.HasValue;
            }
        }

        private TemplateType SelectedTemplate
        {
            get
            {
                return _selectedTemplate;
            }
            set
            {
                if (_selectedTemplate == value)
                {
                    return;
                }
                _selectedTemplate = value;
                SelectedIcon = null;
                if (value != null)
                {
                    lstTemplates.SelectedItem = value.Name;
                }
                else
                {
                    lstTemplates.SelectedIndex = 0;
                }
                    RefreshPreviewPanel();
            }
        }

        private Point? SelectedIcon
        {
            get => _selectedIcon;
            set
            {
                if (_selectedIcon == value)
                {
                    return;
                }
                if (_selectedTemplate != null && _selectedColor.HasValue)
                {
                    Point first = _selectedTemplate.GetFirstValidIcon();
                    Color selCol = _selectedColor.Value;
                    int col = selCol.ToArgb();
                    Point selectedIcon;
                    if (!value.HasValue || !_selectedTemplate.IsValidIcon(value.Value))
                    {
                        selectedIcon = first;
                    }
                    else
                    {
                        selectedIcon = value.Value;
                    }
                    if (selectedIcon == first)
                    {
                        _selectedIcon = null;
                        mappingsIcons.Remove(col);
                    }
                    else
                    {
                        _selectedIcon = selectedIcon;
                        mappingsIcons[col] = _selectedTemplate.GetIconIndex(selectedIcon);
                    }
                    RefreshMapping(selCol);
                }
                else
                {
                    _selectedIcon = null;
                }
                templateTypeMapPanel.Invalidate();
            }
        }

        public Dictionary<int, string> Mappings
        {
            get
            {
                Dictionary<int, string> map = new Dictionary<int, string>();
                foreach (KeyValuePair<int, string> pair in mappingsTemplate)
                {
                    string cur = pair.Value;
                    if (mappingsIcons.ContainsKey(pair.Key))
                    {
                        cur += ":" + mappingsIcons[pair.Key];
                    }
                    map[pair.Key] = cur;
                }
                return map;
            }
        }


        public NewFromImageDialog(IGamePlugin plugin, int width, int height, byte[] imageData, Dictionary<int, string> mappings)
        {
            this.plugin = plugin;
            InitializeComponent();
            foreach (KeyValuePair<int, string> kvp in mappings)
            {
                string tileType = kvp.Value;
                int tileIcon;
                TemplateType tile;
                plugin.Map.SplitTileInfo(tileType, out tile, out tileIcon, "mappings", false);
                mappingsTemplate[kvp.Key] = tile.Name;
                if (tile != null && tileIcon > 0 && tileIcon < tile.NumIcons && tileIcon < tile.NumIcons &&
                    tile.IconMask[tileIcon / tile.IconWidth, tileIcon % tile.IconWidth])
                {
                    mappingsIcons[kvp.Key] = tileIcon;
                }
            }
            int maxWidth = plugin.Map.Metrics.Width;
            int maxHeight = plugin.Map.Metrics.Height;
            int actualWidth = Math.Min(width, maxWidth);
            int actualHeight = Math.Min(height, maxHeight);
            finalImage = new Bitmap(actualWidth, actualHeight, PixelFormat.Format32bppArgb);
            finalImage.SetResolution(96, 96);
            byte[] imgData = new byte[imageData.Length];
            Array.Copy(imageData, 0, imgData, 0, imageData.Length);
            using (Bitmap origData = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                origData.SetResolution(96, 96);
                // Remove all alpha.
                for (int i = 3; i < imgData.Length; i += 4)
                {
                    imgData[i] = 0xFF;
                }
                BitmapData sourceData = origData.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(imgData, 0, sourceData.Scan0, imgData.Length);
                origData.UnlockBits(sourceData);
                using (Graphics g = Graphics.FromImage(finalImage))
                {
                    Rectangle dims = new Rectangle(0, 0, actualWidth, actualHeight);
                    g.DrawImage(origData, dims, dims, GraphicsUnit.Pixel);
                }
            }
            picZoom.Image = finalImage;
            Regex CategoryRegex = new Regex(@"^([a-z]*)", RegexOptions.Compiled);
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
            var templateTypes = plugin.Map.TemplateTypes
                .Where(t => t.ExistsInTheater && t.Thumbnail != null
                    && !t.Flags.HasFlag(TemplateTypeFlag.Clear)
                    && !t.Flags.HasFlag(TemplateTypeFlag.IsGrouped))
                .OrderBy(t => t.Name, expl)
                .GroupBy(t => templateCategory(t)).OrderBy(g => g.Key, expl);
            lstTemplates.Items.Add(clear.Name);
            // Not sure if the grouping makes the order different, but I'll keep it like in the actual tool.
            foreach (IGrouping<string, TemplateType> group in templateTypes)
            {
                foreach (TemplateType template in group)
                {
                    lstTemplates.Items.Add(template.Name);
                }
            }
            lstTemplates.SelectedIndex = 0;
            RebuildMappings(null);
        }

        private void RebuildMappings(Color? toSelect)
        {
            bool select = toSelect.HasValue;
            int valToSelect = select ? toSelect.Value.ToArgb() : 0;
            int indexToSelect = -1;
            int[] keys = mappingsTemplate.Keys.OrderBy(c => c & 0xFFFFFF).ToArray();
            lstMappings.Items.Clear();
            for (int i = 0; i < keys.Length; ++i)
            {
                int col = keys[i];
                if (select && col == valToSelect)
                {
                    indexToSelect = i;
                }
                string entry = MakeMappingString(col);
                if (entry == null)
                {
                    mappingsTemplate.Remove(col);
                    mappingsIcons.Remove(col);
                    continue;
                }
                lstMappings.Items.Add(entry);
            }
            if (indexToSelect != -1)
            {
                lstMappings.SelectedIndex = indexToSelect;
            }
        }

        protected string MakeMappingString(int col)
        {
            string tile;
            if (!mappingsTemplate.TryGetValue(col, out tile))
            {
                return null;
            }
            TemplateType tmpl = plugin.Map.TemplateTypes.Where(tmp => tile.Equals(tmp.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (tmpl == null || tmpl.Flags.HasFlag(TemplateTypeFlag.Clear))
            {
                return null;
            }
            int icon = mappingsIcons.ContainsKey(col) ? mappingsIcons[col] : tmpl.GetIconIndex(tmpl.GetFirstValidIcon());
            return String.Format("#{0:X6} → {1}:{2}", col & 0xFFFFFF, tmpl.Name, icon);
        }

        private void picZoom_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (sender is Control ctr)
                    ctr.Cursor = Cursors.Hand;
                HandlePickColor(e.Location.X, e.Location.Y);
            }
        }

        private void picZoom_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (sender is Control ctr)
                    ctr.Cursor = Cursors.Hand;
                HandlePickColor(e.Location.X, e.Location.Y);
            }
        }

        private void HandlePickColor(int mouseX, int mouseY)
        {
            if (picZoom.Image == null)
            {
                SelectedColor = null;
                return;
            }
            if (picZoom.Image is Bitmap image)
            {
                int realW = image.Width;
                int realH = image.Height;
                int currentW = picZoom.ClientRectangle.Width;
                int currentH = picZoom.ClientRectangle.Height;
                double zoomX = (currentW / (Double)realW);
                double zoomY = (currentH / (Double)realH);
                double zoomActual = Math.Min(zoomX, zoomY);
                double sizeX = zoomActual == zoomX ? currentW : zoomActual * realW;
                double sizeY = zoomActual == zoomY ? currentH : zoomActual * realH;
                int padX = (int)(currentW - sizeX) / 2;
                int padY = (int)(currentH - sizeY) / 2;
                bool outOfBounds = mouseX < 0 || mouseX >= currentW || mouseY < 0 || mouseY >= currentH;
                int realX = zoomActual <= 0 ? 0 : (int)((mouseX - padX) / zoomActual);
                int realY = zoomActual <= 0 ? 0 : (int)((mouseY - padY) / zoomActual);
                bool inImageX = !outOfBounds && realX > 0 && realX < realW;
                bool inImageY = !outOfBounds && realY > 0 && realY < realH;
                SelectedColor = inImageX && inImageY ? (Color?)image.GetPixel(realX, realY) : null;
            }
        }

        protected void RefreshPreviewPanel()
        {
            if (templateTypeNavigationWidget != null)
            {
                templateTypeNavigationWidget.Dispose();
                templateTypeNavigationWidget = null;
            }
            TemplateType selected = SelectedTemplate;
            if (selected == null)
            {
                selected = plugin.Map.TemplateTypes.FirstOrDefault(t => t.Flags.HasFlag(TemplateTypeFlag.Clear));
            }
            if (selected != null && selected.Thumbnail != null && SelectedColor.HasValue)
            {
                templateTypeMapPanel.MapImage = selected.Thumbnail;
                var templateTypeMetrics = new CellMetrics(selected.ThumbnailIconWidth, selected.ThumbnailIconHeight);
                templateTypeNavigationWidget = new NavigationWidget(templateTypeMapPanel, templateTypeMetrics, Globals.PreviewTileSize, false);
                templateTypeNavigationWidget.MouseoverSize = Size.Empty;
                templateTypeNavigationWidget.Activate();
            }
            else
            {
                templateTypeMapPanel.MapImage = null;
            }
        }

        private int RefreshMapping(Color? selected)
        {
            if (!selected.HasValue)
            {
                return -1;
            }
            string mappingStr = MakeMappingString(selected.Value.ToArgb());
            int selectedMapping = GetSelectedMapping(selected);
            if (selectedMapping >= 0)
            {
                if (mappingStr == null)
                {
                    lstMappings.Items.RemoveAt(selectedMapping);
                    lstMappings.ClearSelected();
                }
                else
                {
                    lstMappings.Items[selectedMapping] = mappingStr;
                    lstMappings.SelectedIndex = selectedMapping;
                }
            }
            else
            {
                RebuildMappings(selected);
            }
            return selectedMapping;
        }

        protected void SelectMapping()
        {
            int toSelect = GetSelectedMapping(_selectedColor);
            if (toSelect == -1)
            {
                lstMappings.ClearSelected();
            }
            else
            {
                lstMappings.SelectedIndex = toSelect;
            }
        }

        protected int GetSelectedMapping(Color? selected)
        {
            if (!selected.HasValue)
            {
                return -1;
            }
            string col = String.Format("#{0:X6} ", selected.Value.ToArgb() & 0xFFFFFF);
            int mappings = lstMappings.Items.Count;
            for (int i = 0; i < mappings; ++i)
            {
                string entry = (lstMappings.Items[i] ?? String.Empty).ToString();
                if (entry.StartsWith(col))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
                try
                {
                    if (this.finalImage != null)
                    {
                        this.finalImage.Dispose();
                    }
                }
                catch { }
            }
            base.Dispose(disposing);
        }

        private void lstTemplates_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstTemplates.SelectedItem == null)
            {
                SelectedTemplate = null;
                return;
            }
            string selectedItem = lstTemplates.SelectedItem.ToString();
            TemplateType selectedTemplate = plugin.Map.TemplateTypes.Where(tmp => selectedItem.Equals(tmp.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (selectedTemplate == null || selectedTemplate == _selectedTemplate)
            {
                return;
            }
            if (!_selectedColor.HasValue)
            {
                return;
            }
            // new value in list was picked.
            Color colVal = _selectedColor.Value;
            int colIntVal = colVal.ToArgb();
            mappingsIcons.Remove(colIntVal);
            Point? selIcon = null;
            if (mappingsTemplate.ContainsKey(colIntVal))
            {
                string currentMap = mappingsTemplate[colIntVal];
                if (!currentMap.Equals(selectedItem))
                {
                    // new mapping was picked
                    if (selectedTemplate.Flags.HasFlag(TemplateTypeFlag.Clear))
                    {
                        // Clear; remove mapping
                        mappingsTemplate.Remove(colIntVal);
                    }
                    else
                    {
                        mappingsTemplate[colIntVal] = selectedTemplate.Name;
                    }
                    selIcon = selectedTemplate.GetFirstValidIcon();
                }
                else
                {
                    // else: current mapping is selected. Look up icon.
                    Point? iconPoint = null;
                    if (mappingsIcons.TryGetValue(colIntVal, out int cell))
                    {
                        iconPoint = selectedTemplate.GetIconPoint(cell);
                    }
                    selIcon = iconPoint ?? selectedTemplate.GetFirstValidIcon();
                }
            }
            else
            {
                // add new mapping
                mappingsIcons.Remove(colIntVal);
                if (selectedTemplate.Flags.HasFlag(TemplateTypeFlag.Clear))
                {
                    mappingsTemplate.Remove(colIntVal);
                }
                else
                {
                    mappingsTemplate[colIntVal] = selectedTemplate.Name;
                    selIcon = selectedTemplate.GetFirstValidIcon();
                }
            }
            // Only do this at the very end.
            SelectedTemplate = selectedTemplate;
            RefreshMapping(colVal);
        }

        private void templateTypeMapPanel_PostRender(object sender, Event.RenderEventArgs e)
        {
            e.Graphics.Transform = new Matrix();
            if (!_selectedColor.HasValue || _selectedTemplate == null || _selectedTemplate.Flags.HasFlag(TemplateTypeFlag.Clear))
            {
                return;
            }
            TemplateType selected = _selectedTemplate;
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
            Point selectedIcon = _selectedIcon ?? selected.GetFirstValidIcon();
            using (var selectedIconPen = new Pen(Color.Yellow, Math.Max(1, scale / 16)))
            {
                var cellSize = new Size(scale, scale);
                var rect = new Rectangle(new Point(padX + selectedIcon.X * cellSize.Width, padY + selectedIcon.Y * cellSize.Height), cellSize);
                e.Graphics.DrawRectangle(selectedIconPen, rect);
            }
        }

        private void templateTypeMapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            // This code picks the single icon from the preview pane.
            TemplateType selected = SelectedTemplate;
            bool isRandom = selected != null && selected.IsRandom;
            if (selected == null)
            {
                SelectedIcon = null;
                return;
            }
            var templateTypeMouseCell = templateTypeNavigationWidget.MouseCell;
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
                    if (isRandom)
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

        private void templateTypeMapPanel_MouseMove(object sender, MouseEventArgs e)
        {
            // The function will filter out the mouse buttons.
            templateTypeMapPanel_MouseDown(sender, e);
        }

        private void lstMappings_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(sender is ListBox lb) || lb.SelectedItem == null)
            {
                return;
            }
            string mapping = lb.SelectedItem.ToString();
            Regex mappingRegex = new Regex("^#([0-9A-F]{6}) .*?");
            Match match = mappingRegex.Match(mapping);
            if (!match.Success)
            {
                return;
            }
            // Expand XXXXXX to FFXXXXXX
            int color = int.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
            SelectedColor = Color.FromArgb(0xFF, Color.FromArgb(color));
        }

        private void lstMappings_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is ListBox lb)
            {
                int index = lb.IndexFromPoint(new Point(e.X, e.Y));
                if (index == -1)
                {
                    lb.ClearSelected();
                    SelectedColor = null;
                }
            }
        }

        private void btnChooseColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog cdl = new ColorDialog())
            {
                cdl.Color = Color.Black;
                cdl.FullOpen = true;
                cdl.CustomColors = this.customColors;
                DialogResult res = cdl.ShowDialog();
                this.customColors = cdl.CustomColors;
                if (res == DialogResult.OK || res == DialogResult.Yes)
                {
                    SelectedColor = cdl.Color;
                }
            }
        }
    }
}
