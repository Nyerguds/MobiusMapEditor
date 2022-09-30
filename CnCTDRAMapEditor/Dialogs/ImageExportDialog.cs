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
        public delegate void InvokeDelegateEnableControls(Boolean enabled, String processingLabel);
        public delegate DialogResult InvokeDelegateMessageBox(String message, MessageBoxButtons buttons, MessageBoxIcon icon);
        private SimpleMultiThreading<ImageExportDialog> multiThreader;
        private Label m_BusyStatusLabel;

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
            "Waypoint labels",
            "Football goal areas",
            "Cell triggers",
            "Object triggers",
            "Building rebuild priorities",
            "Building 'fake' labels"
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

        public Label StatusLabel
        {
            get { return m_BusyStatusLabel; }
            set { m_BusyStatusLabel = value; }
        }

        private string inputFilename;

        public ImageExportDialog(IGamePlugin gamePlugin, MapLayerFlag layers, string filename)
        {
            InitializeComponent();
            this.gamePlugin = gamePlugin;
            inputFilename = filename;
            txtScale.Text = Globals.ExportTileScale.ToString(CultureInfo.InvariantCulture);
            chkSmooth.Checked = Globals.ExportSmoothScale;
            SetSizeLabel();
            SetLayers(layers);
            txtScale.Select(0, 0);
            // Could make this at the moment of the call, too, but it also has a
            // system to ignore further calls if the running one isn't finished.
            multiThreader = SimpleMulti_Threading.Make(this);
        }

        private void SetSizeLabel()
        {
            if (Double.TryParse(txtScale.Text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double scale))
            {
                int width = gamePlugin.Map.Metrics.Width * Math.Max(1, (int)(Globals.OriginalTileWidth * scale));
                int height = gamePlugin.Map.Metrics.Height * Math.Max(1, (int)(Globals.OriginalTileHeight * scale));
                lblSize.Text = String.Format("(Size: {0}×{1})", width, height);
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
            String pattern = "^\\d*(\\.\\d*)?$";
            if (Regex.IsMatch(txtScale.Text, pattern))
            {
                SetSizeLabel();
                return;
            }
            // something snuck in, probably with ctrl+v. Remove it.
            System.Media.SystemSounds.Beep.Play();
            StringBuilder text = new StringBuilder();
            String txt = txtScale.Text.ToUpperInvariant();
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
                txtScale.Text = value.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                txtScale.Text = filteredText;
            }
            if (firstIllegalChar == -1)
                firstIllegalChar = 0;
            txtScale.Select(firstIllegalChar, 0);
            SetSizeLabel();
        }

        private void btnPickFile_Click(Object sender, EventArgs e)
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
                }
            }
        }

        private void btnExport_Click(Object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtPath.Text))
            {
                MessageBox.Show("Please select a filename to export to.", "Error");
                return;
            }
            if (!Double.TryParse(txtScale.Text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double scale))
            {
                MessageBox.Show("Could not parse scale factor!", "Error");
                return;
            }
            MapLayerFlag layers = GetLayers() | MapLayerFlag.Template;
            bool smooth = chkSmooth.Checked;
            string path = txtPath.Text;
            Func<String> saveOperation = () => SaveImage(gamePlugin, layers, scale, smooth, path);
            multiThreader.ExecuteThreaded(saveOperation, ShowResult, true, EnableControls, "Exporting image");
        }

        private static String SaveImage(IGamePlugin gamePlugin, MapLayerFlag layers, double scale, bool smooth, string outputPath)
        {
            int tileWidth = Math.Max(1, (int)(Globals.OriginalTileWidth * scale));
            int tileHeight = Math.Max(1, (int)(Globals.OriginalTileHeight * scale));
            Size size = new Size(gamePlugin.Map.Metrics.Width * tileWidth, gamePlugin.Map.Metrics.Height * tileHeight);
            using (Bitmap pr = gamePlugin.Map.GeneratePreview(size, gamePlugin.GameType, layers, smooth, false, false).ToBitmap())
            {
                using (Graphics g = Graphics.FromImage(pr))
                {
                    ViewTool.PostRenderMap(g, gamePlugin.GameType, gamePlugin.Map, scale, layers, MapLayerFlag.None);
                }
                pr.Save(outputPath, ImageFormat.Png);
            }
            return outputPath;
        }

        private void ShowResult(String path)
        {
            using (ImageExportedDialog imexd = new ImageExportedDialog(path))
            {
                imexd.ShowDialog(this);
            }
            this.DialogResult = DialogResult.OK;
        }

        private void EnableControls(Boolean enabled, String processingLabel)
        {
            txtScale.Enabled = enabled;
            chkSmooth.Enabled = enabled;
            layersListBox.Enabled = enabled;
            indicatorsListBox.Enabled = enabled;
            btnPickFile.Enabled = enabled;
            btnExport.Enabled = enabled;
            btnCancel.Enabled = enabled;
            if (enabled)
            {
                this.multiThreader.RemoveBusyLabel();
            }
            else
            {
                this.multiThreader.CreateBusyLabel(processingLabel);
            }
        }
    }
}
