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
using System.Text;
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
                lblColorVal.Text = value.HasValue ? "#" + ((UInt32)value.Value.ToArgb() & 0xFFFFFF).ToString("X6") : "-";
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
                    String cur = pair.Value;
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
            byte[] imgData = new byte[imageData.Length];
            Array.Copy(imageData, 0, imgData, 0, imageData.Length);
            using (Bitmap origData = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                // Remove all alpha.
                for (Int32 i = 3; i < imgData.Length; i += 4)
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
            TemplateType clear = plugin.Map.TemplateTypes.Where(t => (t.Flag & TemplateTypeFlag.Clear) == TemplateTypeFlag.Clear).FirstOrDefault();
            lstTemplates.Items.Add(clear.Name);
            // Not sure if the grouping makes the order different, but I'll keep it like in the actual tool.
            foreach (IGrouping<String, TemplateType> group in templateTypes)
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
            for (Int32 i = 0; i < keys.Length; i++)
            {
                Int32 col = keys[i];
                if (select && col == valToSelect)
                {
                    indexToSelect = i;
                }
                String entry = MakeMappingString(col);
                if (entry == null)
                {
                    mappingsTemplate.Remove(col);
                    mappingsIcons.Remove(col);
                    continue;
                }
                else
                {
                    lstMappings.Items.Add(entry);
                }
            }
            if (indexToSelect != -1)
            {
                lstMappings.SelectedIndex = indexToSelect;
            }
        }

        protected String MakeMappingString(int col)
        {
            String tile;
            if (!mappingsTemplate.TryGetValue(col, out tile))
            {
                return null;
            }
            TemplateType tmpl = plugin.Map.TemplateTypes.Where(tmp => tile.Equals(tmp.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (tmpl == null || (tmpl.Flag & TemplateTypeFlag.Clear) != TemplateTypeFlag.None)
            {
                return null;
            }
            int icon = mappingsIcons.ContainsKey(col) ? mappingsIcons[col] : tmpl.GetIconIndex(tmpl.GetFirstValidIcon());
            return String.Format("#{0:X6} -> {1}:{2}", col & 0xFFFFFF, tmpl.Name, icon);
        }

        private void picZoom_MouseDown(Object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (sender is Control ctr)
                    ctr.Cursor = Cursors.Hand;
                HandlePickColor(e.Location.X, e.Location.Y);
            }
        }

        private void picZoom_MouseMove(Object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (sender is Control ctr)
                    ctr.Cursor = Cursors.Hand;
                HandlePickColor(e.Location.X, e.Location.Y);
            }

        }

        private void HandlePickColor(Int32 mouseX, Int32 mouseY)
        {
            if (picZoom.Image == null)
            {
                SelectedColor = null;
                return;
            }
            if (picZoom.Image is Bitmap image)
            {
                Int32 realW = image.Width;
                Int32 realH = image.Height;
                Int32 currentW = picZoom.ClientRectangle.Width;
                Int32 currentH = picZoom.ClientRectangle.Height;
                Double zoomX = (currentW / (Double)realW);
                Double zoomY = (currentH / (Double)realH);
                Double zoomActual = Math.Min(zoomX, zoomY);
                Double sizeX = zoomActual == zoomX ? currentW : zoomActual * realW;
                Double sizeY = zoomActual == zoomY ? currentH : zoomActual * realH;
                Int32 padX = (Int32)(currentW - sizeX) / 2;
                Int32 padY = (Int32)(currentH - sizeY) / 2;
                Boolean outOfBounds = mouseX < 0 || mouseX >= currentW || mouseY < 0 || mouseY >= currentH;
                Int32 realX = zoomActual <= 0 ? 0 : (Int32)((mouseX - padX) / zoomActual);
                Int32 realY = zoomActual <= 0 ? 0 : (Int32)((mouseY - padY) / zoomActual);
                Boolean inImageX = !outOfBounds && realX > 0 && realX < realW;
                Boolean inImageY = !outOfBounds && realY > 0 && realY < realH;
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
            if (selected != null && SelectedColor.HasValue)
            {
                if (selected.Thumbnail == null)
                {
                    // Special case: tile is not initialised. Initialise with forced dummy generation.
                    // This is really only for the tile FF "research mode" on RA maps.
                    selected.Init(plugin.Map.Theater, true);
                }
                templateTypeMapPanel.MapImage = selected.Thumbnail;
                var templateTypeMetrics = new CellMetrics(selected.ThumbnailIconWidth, selected.ThumbnailIconHeight);
                templateTypeNavigationWidget = new NavigationWidget(templateTypeMapPanel, templateTypeMetrics, Globals.OriginalTileSize, false);
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
            String mappingStr = MakeMappingString(selected.Value.ToArgb());
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
            String col = String.Format("#{0:X6} ", selected.Value.ToArgb() & 0xFFFFFF);
            int mappings = lstMappings.Items.Count;
            
            for (int i = 0; i < mappings; i++)
            {
                String entry = (lstMappings.Items[i] ?? String.Empty).ToString();
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

        private void lstTemplates_SelectedIndexChanged(Object sender, EventArgs e)
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
                    if ((selectedTemplate.Flag & TemplateTypeFlag.Clear) != TemplateTypeFlag.None)
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
                if ((selectedTemplate.Flag & TemplateTypeFlag.Clear) != TemplateTypeFlag.None)
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

        private void templateTypeMapPanel_PostRender(Object sender, Event.RenderEventArgs e)
        {
            e.Graphics.Transform = new Matrix();
            if (!_selectedColor.HasValue || _selectedTemplate == null || (_selectedTemplate.Flag & TemplateTypeFlag.Clear) != TemplateTypeFlag.None)
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

        private void templateTypeMapPanel_MouseDown(Object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            // This code picks the single icon from the preview pane.
            TemplateType selected = SelectedTemplate;
            bool isRandom = selected != null && (selected.Flag & TemplateTypeFlag.RandomCell) != TemplateTypeFlag.None;
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

        private void templateTypeMapPanel_MouseMove(Object sender, MouseEventArgs e)
        {
            // The function will filter out the mouse buttons.
            templateTypeMapPanel_MouseDown(sender, e);
        }

        private void lstMappings_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if (!(sender is ListBox lb) || lb.SelectedItem == null)
            {
                return;
            }
            String mapping = lb.SelectedItem.ToString();
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

        private void lstMappings_MouseDown(Object sender, MouseEventArgs e)
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
    }
}
