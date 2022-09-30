using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Tools;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class ImageExportDialog : Form
    {
        public delegate void InvokeDelegateEnableControls(Boolean enabled, String processingLabel);
        public delegate DialogResult InvokeDelegateMessageBox(String message, MessageBoxButtons buttons, MessageBoxIcon icon);
        private Thread m_ProcessingThread;
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
            Func<String> saveOperation = () => SaveImage(gamePlugin, (GetLayers() | MapLayerFlag.Template), scale, chkSmooth.Checked, txtPath.Text);
            Action<String> completeOperation = (s) => ShowResult(s);
            ExecuteThreaded(saveOperation, completeOperation, "Exporting image");

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
            this.Invoke((MethodInvoker) (() => 
            {
                using (ImageExportedDialog imexd = new ImageExportedDialog(path))
                {
                    imexd.ShowDialog(this);
                }
                this.DialogResult = DialogResult.OK;
            }));
        }

        /// <summary>
        /// Executes a threaded operation while locking the UI. 
        /// </summary>
        /// <param name="function">A func returning a string</param>
        /// <param name="resetPalettes">True to reset palettes dropdown when loading the file resulting from the operation</param>
        /// <param name="resetIndex">True to reset frames index when loading the file resulting from the operation</param>
        /// <param name="resetZoom">True to reset auto-zoom when loading the file resulting from the operation</param>
        /// <param name="operationType">String to indicate the process type being executed (eg. "Saving")</param>
        private void ExecuteThreaded<T>(Func<T> function, Action<T> resultFunction, String operationType)
        {
            if (this.m_ProcessingThread != null && this.m_ProcessingThread.IsAlive)
                return;
            //Arguments: func returning SupportedFileType, reset palettes, reset index, reset auto-zoom, process type indication string.
            Object[] arrParams = { function, resultFunction, operationType };
            this.m_ProcessingThread = new Thread(this.ExecuteThreadedActual<T>);
            this.m_ProcessingThread.Start(arrParams);
        }

        /// <summary>
        /// Executes a threaded operation while locking the UI.
        /// "parameters" must be an array of Object containing 4 items:
        /// a func returning SupportedFileType,
        /// boolean 'reset palettes dropdown',
        /// boolean 'reset frames index',
        /// boolean 'reset auto-zoom',
        /// and a string to indicate the process type being executed (eg. "Saving").
        /// </summary>
        /// <param name="parameters">
        ///     Array of Object, containing 5 items: func returning SupportedFileType, boolean 'reset palettes dropdown', boolean 'reset frames index',
        ///     boolean 'reset auto-zoom', string to indicate the process type being executed (eg. "Saving").
        /// </param>
        private void ExecuteThreadedActual<T>(Object parameters)
        {
            Object[] arrParams = parameters as Object[];
            Func<T> func;
            Action<T> resAct;
            if (arrParams == null || arrParams.Length < 3 || ((func = arrParams[0] as Func<T>) == null) || ((resAct = arrParams[1] as Action<T>) == null && arrParams[1] != null))
            {
                try { this.Invoke(new InvokeDelegateEnableControls(this.EnableControls), true, null); }
                catch (InvalidOperationException) { /* ignore */ }
                return;
            }
            String operationType = arrParams[2] as String;
            this.Invoke(new InvokeDelegateEnableControls(this.EnableControls), false, operationType);
            operationType = String.IsNullOrEmpty(operationType) ? "Operation" : operationType.Trim();
            T result = default(T);
            try
            {
                // Processing code.
                result = func();
            }
            catch (ThreadAbortException)
            {
                // Ignore. Thread is aborted.
            }
            catch (Exception ex)
            {
                String message = operationType + " failed:\n" + ex.Message + "\n" + ex.StackTrace;
                this.Invoke(new InvokeDelegateMessageBox(this.ShowMessageBox), message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Invoke(new InvokeDelegateEnableControls(this.EnableControls), true, null);
            }
            try
            {
                this.Invoke(new InvokeDelegateEnableControls(this.EnableControls), true, null);
                if (!EqualityComparer<T>.Default.Equals(result, default(T)))
                {
                    resAct?.Invoke(result);
                }
            }
            catch (InvalidOperationException) { /* ignore */ }
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
                RemoveBusyLabel();
            }
            else
            {
                CreateBusyLabel(processingLabel);
            }
        }

        private DialogResult ShowMessageBox(String message, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            if (message == null)
                return DialogResult.Cancel;
            this.AllowDrop = false;
            DialogResult result = MessageBox.Show(this, message, this.Text, buttons, icon);
            this.AllowDrop = true;
            return result;
        }

        private void CreateBusyLabel(string processingLabel)
        {

            // Create busy status label.
            RemoveBusyLabel();
            if (processingLabel == null)
            {
                return;
            }
            this.m_BusyStatusLabel = new Label();
            this.m_BusyStatusLabel.Text = (String.IsNullOrEmpty(processingLabel) ? "Processing" : processingLabel) + "...";
            this.m_BusyStatusLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.m_BusyStatusLabel.Font = new Font(this.m_BusyStatusLabel.Font.FontFamily, 15F, FontStyle.Regular, GraphicsUnit.Pixel, 0);
            this.m_BusyStatusLabel.AutoSize = false;
            this.m_BusyStatusLabel.Size = new Size(300, 100);
            this.m_BusyStatusLabel.Anchor = AnchorStyles.None; // Always floating in the middle, even on resize.
            this.m_BusyStatusLabel.BorderStyle = BorderStyle.FixedSingle;
            Int32 x = (this.ClientRectangle.Width - 300) / 2;
            Int32 y = (this.ClientRectangle.Height - 100) / 2;
            this.m_BusyStatusLabel.Location = new Point(x, y);
            this.Controls.Add(this.m_BusyStatusLabel);
            this.m_BusyStatusLabel.Visible = true;
            this.m_BusyStatusLabel.BringToFront();
        }

        private void RemoveBusyLabel()
        {
            if (this.m_BusyStatusLabel == null)
                return;
            this.Controls.Remove(this.m_BusyStatusLabel);
            try { this.m_BusyStatusLabel.Dispose(); }
            catch { /* ignore */ }
            this.m_BusyStatusLabel = null;
        }

    }
}
