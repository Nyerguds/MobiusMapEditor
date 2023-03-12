using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Tools;
using MobiusEditor.Utility;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class ImageExportDialog : Form, IHasStatusLabel
    {
        private SimpleMultiThreading multiThreader;
        private string boundsLabel;

        private String[] MapLayerNames = {
            // Map layers
            "Template",
            "Terrain",
            "Resources",
            "Walls",
            "Overlay",
            "Smudge",
            "Infantry",
            "Units",
            "Buildings",
            "Waypoints",
            // Indicators
            "Map boundaries",
            "Map symmetry",
            "Map grid",
            "Waypoint labels",
            "Football goal areas",
            "Cell triggers",
            "Object triggers",
            "Building rebuild priorities",
            "Building 'fake' labels",
            "Jam / gap radiuses",
            "Waypoint reveal radiuses",
        };

        IGamePlugin gamePlugin;

        private MapLayerFlag renderLayers;
        public MapLayerFlag RenderLayers
        {
            get { return renderLayers; }
            private set { renderLayers = value; }
        }

        public string Filename
        {
            get { return txtPath.Text; }
            set { txtPath.Text = value; }
        }

        public bool SmoothScale
        {
            get { return chkSmooth.Checked; }
            set { chkSmooth.Checked = value; }
        }

        public Label StatusLabel { get; set; }

        private string inputFilename;

        public ImageExportDialog(IGamePlugin gamePlugin, MapLayerFlag layers, string filename)
        {
            InitializeComponent();
            // Store to adapt later
            this.boundsLabel = chkBoundsOnly.Text;
            this.gamePlugin = gamePlugin;
            inputFilename = filename;
            txtScale.Text = Globals.ExportTileScale.ToString(CultureInfo.InvariantCulture);
            chkSmooth.Checked = Globals.ExportSmoothScale;
            // For multiplayer maps, default to only exporting the bounds.
            chkBoundsOnly.Checked = !gamePlugin.Map.BasicSection.SoloMission;
            SetSizeLabel();
            SetLayers(layers);
            txtScale.Select(0, 0);
            // Could make this at the moment of the call, too, but it also has a
            // system to ignore further calls if the running one isn't finished.
            multiThreader = new SimpleMultiThreading(this);
            multiThreader.ProcessingLabelBorder = BorderStyle.Fixed3D;
        }

        private void SetSizeLabel()
        {
            if (Double.TryParse(txtScale.Text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double scale))
            {
                int scaleWidth = Math.Max(1, (int)(Globals.OriginalTileWidth * scale));
                int scaleHeight = Math.Max(1, (int)(Globals.OriginalTileHeight * scale));
                int width = gamePlugin.Map.Metrics.Width * scaleWidth;
                int height = gamePlugin.Map.Metrics.Height * scaleHeight;
                lblSize.Text = String.Format("(Size: {0}×{1})", width, height);
                int boundsWidth = gamePlugin.Map.Bounds.Width * scaleWidth;
                int boundsHeight = gamePlugin.Map.Bounds.Height * scaleHeight;
                chkBoundsOnly.Text = boundsLabel + String.Format(" ({0}×{1})", boundsWidth, boundsHeight);
            }
        }

        private void SetLayers(MapLayerFlag layers)
        {
            layersListBox.Items.Clear();
            indicatorsListBox.Items.Clear();
            int len = MapLayerNames.Length;
            for (int i = 1; i < len; ++i)
            {
                MapLayerFlag mlf = (MapLayerFlag)(1 << i);
                if (gamePlugin.GameType != GameType.RedAlert && mlf == MapLayerFlag.BuildingFakes
                    || gamePlugin.GameType != GameType.RedAlert && mlf == MapLayerFlag.GapRadius
                    || gamePlugin.GameType != GameType.SoleSurvivor && mlf == MapLayerFlag.FootballArea)
                {
                    continue;
                }
                ListItem<MapLayerFlag> mli = new ListItem<MapLayerFlag>(mlf, MapLayerNames[i]);
                int index;
                if ((MapLayerFlag.MapLayers & mlf) != MapLayerFlag.None)
                {
                    index = layersListBox.Items.Add(mli);
                    if ((layers & mlf) != MapLayerFlag.None)
                    {
                        layersListBox.SetSelected(index, true);
                    }
                }
                if ((MapLayerFlag.Indicators & mlf) != MapLayerFlag.None)
                {
                    index = indicatorsListBox.Items.Add(new ListItem<MapLayerFlag>(mlf, MapLayerNames[i]));
                    if ((layers & mlf) != MapLayerFlag.None)
                    {
                        indicatorsListBox.SetSelected(index, true);
                    }
                }
            }
        }

        public MapLayerFlag GetLayers()
        {
            MapLayerFlag value = MapLayerFlag.None;
            foreach (ListItem<MapLayerFlag> mli in layersListBox.SelectedItems)
            {
                value |= mli.Value;
            }
            foreach (ListItem<MapLayerFlag> mli in indicatorsListBox.SelectedItems)
            {
                value |= mli.Value;
            }
            return value;
        }

        private void txtScale_TextChanged(Object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }
            String pattern = "^\\d*(\\.\\d*)?$";
            if (Regex.IsMatch(textBox.Text, pattern))
            {
                SetSizeLabel();
                return;
            }
            // something snuck in, probably with ctrl+v. Remove it.
            System.Media.SystemSounds.Beep.Play();
            StringBuilder text = new StringBuilder();
            String txt = textBox.Text.ToUpperInvariant();
            Int32 txtLen = txt.Length;
            Int32 firstIllegalChar = -1;
            Int32 firstDot = txt.IndexOf(".");
            for (Int32 i = 0; i < txtLen; ++i)
            {
                Char c = txt[i];
                Boolean isNumRange = c >= '0' && c <= '9';
                Boolean isLegalDot = c == '.' && i == firstDot;
                if (!isNumRange && !isLegalDot)
                {
                    if (firstIllegalChar == -1)
                        firstIllegalChar = i;
                    continue;
                }
                text.Append(c);
            }
            String filteredText = text.ToString();
            Decimal value;
            NumberStyles ns = NumberStyles.Number | NumberStyles.AllowDecimalPoint;
            // Setting "this.Text" will trigger this function again, but that's okay, it'll immediately succeed in the regex and abort.
            if (Decimal.TryParse(filteredText, ns, NumberFormatInfo.CurrentInfo, out value))
            {
                textBox.Text = value.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                textBox.Text = filteredText;
            }
            if (firstIllegalChar == -1)
                firstIllegalChar = 0;
            textBox.Select(firstIllegalChar, 0);
            SetSizeLabel();
        }

        private void BtnSetDimensions_Click(Object sender, EventArgs e)
        {
            double scale;
            if (!Double.TryParse(txtScale.Text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out scale))
            {
                return;
            }
            int curSize = gamePlugin.Map.Metrics.Width * Math.Max(1, (int)(Globals.OriginalTileWidth * scale));
            using (ImagetExportSetSizeDialog dimdialog = new ImagetExportSetSizeDialog(curSize))
            {
                dimdialog.StartPosition = FormStartPosition.CenterParent;
                if (DialogResult.OK == dimdialog.ShowDialog(this))
                {
                    // Can never be less than 1 pixel per cell.
                    curSize = Math.Max(dimdialog.Dimension, gamePlugin.Map.Metrics.Width);
                    scale = curSize / (double)(Globals.OriginalTileWidth * gamePlugin.Map.Metrics.Width);
                    txtScale.Text = scale.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        private void btnPickFile_Click(Object sender, EventArgs e)
        {
            SelectPath();
        }

        private void btnExport_Click(Object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtPath.Text))
            {
                //MessageBox.Show("Please select a filename to export to.", "Error");
                if (!SelectPath())
                {
                    return;
                }
            }
            if (!Double.TryParse(txtScale.Text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double scale))
            {
                MessageBox.Show("Could not parse scale factor!", "Error");
                return;
            }
            MapLayerFlag layers = GetLayers() | MapLayerFlag.Template;
            bool smooth = chkSmooth.Checked;
            bool inBounds = chkBoundsOnly.Checked;
            string path = txtPath.Text;
            Func<String> saveOperation = () => SaveImage(gamePlugin, layers, scale, smooth, inBounds, path);
            multiThreader.ExecuteThreaded(saveOperation, ShowResult, true, EnableControls, "Exporting image");
        }

        private bool SelectPath()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.AutoUpgradeEnabled = false;
                sfd.RestoreDirectory = true;
                sfd.AddExtension = true;
                sfd.Filter = "PNG files (*.png)|*.png|JPEG files (*.jpg)|*.jpg";
                string current = string.IsNullOrEmpty(txtPath.Text) ? inputFilename : txtPath.Text;
                if (!String.IsNullOrEmpty(current))
                {
                    sfd.InitialDirectory = Path.GetDirectoryName(current);
                    bool isJpeg = "jpg".Equals(Path.GetExtension(current), StringComparison.OrdinalIgnoreCase);
                    sfd.FilterIndex = isJpeg ? 2 : 1;
                    sfd.FileName = Path.ChangeExtension(current, isJpeg ? "jpg" : "png");
                }
                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    txtPath.Text = sfd.FileName;
                    return true;
                }
                return false;
            }
        }

        private static String SaveImage(IGamePlugin gamePlugin, MapLayerFlag layers, double scale, bool smooth, bool inBounds, string outputPath)
        {
            int tileWidth = Math.Max(1, (int)(Globals.OriginalTileWidth * scale));
            int tileHeight = Math.Max(1, (int)(Globals.OriginalTileHeight * scale));
            int fullWidth = gamePlugin.Map.Metrics.Width;
            int fullHeight = gamePlugin.Map.Metrics.Height;
            int width = inBounds ? gamePlugin.Map.Bounds.Width : fullWidth;
            int height = inBounds ? gamePlugin.Map.Bounds.Height : fullHeight;
            Size fullSize = new Size(fullWidth * tileWidth, fullHeight * tileHeight);
            Size size = new Size(width * tileWidth, height * tileHeight);
            using (Bitmap exportImage = gamePlugin.Map.GeneratePreview(size, gamePlugin.GameType, layers, smooth, inBounds, false).ToBitmap())
            {
                if ((layers & MapLayerFlag.Indicators) != MapLayerFlag.None)
                {
                    // Draw on new transparent layer, then paint over image.
                    using (Graphics gExportImage = Graphics.FromImage(exportImage))
                    using (Bitmap overlaysImage = new Bitmap(fullSize.Width, fullSize.Height))
                    {
                        using (Graphics gOverlaysImage = Graphics.FromImage(overlaysImage))
                        {
                            ViewTool.PostRenderMap(gOverlaysImage, gamePlugin, gamePlugin.Map, scale, layers, MapLayerFlag.None, false);
                        }
                        Rectangle fullRect = new Rectangle(new Point(0, 0), size);
                        Rectangle boundsRect = inBounds ? new Rectangle(new Point(gamePlugin.Map.Bounds.X * tileWidth, gamePlugin.Map.Bounds.Y * tileHeight), size) : fullRect;
                        gExportImage.DrawImage(overlaysImage, fullRect, boundsRect, GraphicsUnit.Pixel);
                    }
                }
                exportImage.Save(outputPath, ImageFormat.Png);
            }
            return outputPath;
        }

        private void ShowResult(String path)
        {
            if (path == null)
            {
                MessageBox.Show("Image saving failed!", "Image Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            using (ImageExportedDialog imexd = new ImageExportedDialog(path))
            {
                if (imexd.ShowDialog(this) == DialogResult.OK)
                {
                    this.DialogResult = DialogResult.OK;
                }
            }
            
        }

        private void EnableControls(Boolean enabled, String processingLabel)
        {
            txtScale.Enabled = enabled;
            btnSetDimensions.Enabled = enabled;
            chkSmooth.Enabled = enabled;
            chkBoundsOnly.Enabled = enabled;
            layersListBox.Enabled = enabled;
            indicatorsListBox.Enabled = enabled;
            btnPickFile.Enabled = enabled;
            btnExport.Enabled = enabled;
            btnCancel.Enabled = enabled;
            if (enabled)
            {
                SimpleMultiThreading.RemoveBusyLabel(this);
            }
            else
            {
                this.multiThreader.CreateBusyLabel(this, processingLabel);
            }
        }
    }
}
