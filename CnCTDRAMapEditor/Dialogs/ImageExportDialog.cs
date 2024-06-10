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
        private string lastOpenedFolder;

        public ImageExportDialog(IGamePlugin gamePlugin, MapLayerFlag layers, string filename, string lastOpenedFolder)
        {
            InitializeComponent();
            this.gamePlugin = gamePlugin;
            inputFilename = filename;
            this.lastOpenedFolder = lastOpenedFolder;
            txtScale.Text = Globals.ExportTileScale.ToString(CultureInfo.InvariantCulture);
            chkSmooth.Checked = Globals.ExportSmoothScale;
            // For multiplayer maps, default to only exporting the bounds.
            chkBoundsOnly.Checked = !gamePlugin.Map.BasicSection.SoloMission;
            SetSizeLabels();
            SetLayers(layers);
            txtScale.Select(0, 0);
            // Could make this at the moment of the call, too, but it also has a
            // system to ignore further calls if the running one isn't finished.
            multiThreader = new SimpleMultiThreading(this);
            multiThreader.ProcessingLabelBorder = BorderStyle.Fixed3D;
        }

        private void SetSizeLabels()
        {
            if (Double.TryParse(txtScale.Text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double scale))
            {
                int scaleWidth = Math.Max(1, (int)Math.Floor(Globals.OriginalTileWidth * scale + 0.0001));
                int scaleHeight = Math.Max(1, (int)Math.Floor(Globals.OriginalTileHeight * scale + 0.0001));
                int width = gamePlugin.Map.Metrics.Width * scaleWidth;
                int height = gamePlugin.Map.Metrics.Height * scaleHeight;
                lblSize.Text = String.Format("Size: {0}×{1}", width, height);
                lblCellSize.Text = String.Format("Cell size: {0}×{1}", scaleWidth, scaleHeight);
                int boundsWidth = gamePlugin.Map.Bounds.Width * scaleWidth;
                int boundsHeight = gamePlugin.Map.Bounds.Height * scaleHeight;
                lblSizeBounds.Text = String.Format("{0}×{1}", boundsWidth, boundsHeight);
            }
        }

        private void SetLayers(MapLayerFlag layers)
        {
            layersListBox.Items.Clear();
            indicatorsListBox.Items.Clear();
            String[] names = Map.MapLayerNames;
            int len = names.Length;
            for (int i = 0; i < len; ++i)
            {
                // Get layer flag from index. This only works if the flags are incremental bit flags without gaps.
                MapLayerFlag mlf = (MapLayerFlag)(1 << i);
                if (!gamePlugin.GameInfo.SupportsMapLayer(mlf))
                {
                    continue;
                }
                ListItem<MapLayerFlag> mli = new ListItem<MapLayerFlag>(mlf, names[i]);
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
                    index = indicatorsListBox.Items.Add(new ListItem<MapLayerFlag>(mlf, names[i]));
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
                SetSizeLabels();
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
            SetSizeLabels();
        }

        private void BtnSetDimensions_Click(Object sender, EventArgs e)
        {
            double scale;
            if (!Double.TryParse(txtScale.Text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out scale))
            {
                scale = 1;
            }
            int curSize = gamePlugin.Map.Metrics.Width * Math.Max(1, (int)Math.Round(Globals.OriginalTileWidth * scale));
            using (ImagetExportSetSizeDialog dimdialog = new ImagetExportSetSizeDialog(curSize))
            {
                dimdialog.StartPosition = FormStartPosition.CenterParent;
                if (DialogResult.OK == dimdialog.ShowDialog(this))
                {
                    // Can never be less than 1 pixel per cell.
                    curSize = Math.Max(dimdialog.Dimension, gamePlugin.Map.Metrics.Width);
                    scale = curSize / (double)(Globals.OriginalTileWidth * gamePlugin.Map.Metrics.Width);
                    int intW = (int)Math.Floor(Globals.OriginalTileWidth * scale + 0.0001);
                    scale = (double)intW / Globals.OriginalTileWidth;

                    txtScale.Text = scale.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        private void BtnSetCellSize_Click(Object sender, EventArgs e)
        {
            double scale;
            if (!Double.TryParse(txtScale.Text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out scale))
            {
                scale = 1;
            }
            int curCellSize = (int)Math.Floor(Globals.OriginalTileWidth * scale + 0.0001);
            using (ImagetExportSetSizeDialog dimdialog = new ImagetExportSetSizeDialog(curCellSize))
            {
                dimdialog.StartPosition = FormStartPosition.CenterParent;
                if (DialogResult.OK == dimdialog.ShowDialog(this))
                {
                    // Can never be less than 1 pixel per cell.
                    curCellSize = Math.Max(1, dimdialog.Dimension);
                    scale = curCellSize / (double)Globals.OriginalTileWidth;
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
            MapLayerFlag layers = GetLayers();
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
                sfd.RestoreDirectory = false;
                sfd.AddExtension = true;
                sfd.Filter = "PNG files (*.png)|*.png|JPEG files (*.jpg;*.jpeg)|*.jpg;*.jpeg";
                string current = string.IsNullOrEmpty(txtPath.Text) ? inputFilename : txtPath.Text;
                if (!String.IsNullOrEmpty(current))
                {
                    sfd.InitialDirectory = Path.GetDirectoryName(current);
                    bool isJpeg = Regex.IsMatch(Path.GetExtension(current), "^\\.jpe?g$", RegexOptions.IgnoreCase);
                    sfd.FilterIndex = isJpeg ? 2 : 1;
                    // If already detected as jpeg by extension, there's no need to change the extension.
                    sfd.FileName = isJpeg ? current : Path.ChangeExtension(current, ".png");
                }
                else
                {
                    bool classicLogic = Globals.UseClassicFiles && Globals.ClassicNoRemasterLogic;
                    string lastFolder = lastOpenedFolder;
                    string defFolder = gamePlugin.GameInfo.DefaultSaveDirectory;
                    string constFolder = Directory.Exists(defFolder) ? defFolder : Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    sfd.InitialDirectory = lastFolder ?? (classicLogic ? Program.ApplicationPath : constFolder);
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
            int tileWidth = Math.Max(1, (int)Math.Round(Globals.OriginalTileWidth * scale));
            int tileHeight = Math.Max(1, (int)Math.Round(Globals.OriginalTileHeight * scale));
            int fullWidth = gamePlugin.Map.Metrics.Width;
            int fullHeight = gamePlugin.Map.Metrics.Height;
            int width = inBounds ? gamePlugin.Map.Bounds.Width : fullWidth;
            int height = inBounds ? gamePlugin.Map.Bounds.Height : fullHeight;
            Size size = new Size(width * tileWidth, height * tileHeight);
            bool clearBg = layers.HasFlag(MapLayerFlag.Template);
            using (Bitmap exportImage = gamePlugin.Map.GeneratePreview(size, gamePlugin, layers, clearBg, smooth, inBounds, false).ToBitmap())
            {
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
                imexd.StartPosition = FormStartPosition.CenterParent;
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
