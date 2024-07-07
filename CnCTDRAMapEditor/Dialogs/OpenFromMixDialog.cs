using MobiusEditor.Utility;
using MobiusEditor.Utility.Hashing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class OpenFromMixDialog : Form, IHasStatusLabel
    {
        private bool resizing = false;
        private string[] initPaths = null;
        private List<MixFile> openedMixFiles = new List<MixFile>();
        private MixFileNameGenerator romfis;
        private SimpleMultiThreading analysisMultiThreader;
        List<MixEntry> currentMixInfo;
        private string titleMain;
        private readonly object abortLockObj = new object();
        private bool abortRequested = false;
        public string SelectedFile { get; set; }
        private string identifiedGame;
        private HashMethod identifiedHasher;

        public Label StatusLabel { get; set; }

        /// <summary>
        /// Initialises a new OpenFromMixDialog
        /// </summary>
        /// <param name="baseMix">Mix file to open</param>
        /// <param name="internalMixParts">Deeper path of internal mix files to open.</param>
        /// <param name="romfis"></param>
        public OpenFromMixDialog(MixFile baseMix, string[] internalMixParts, MixFileNameGenerator romfis)
        {
            InitializeComponent();
            titleMain = this.Text;
            this.romfis = romfis;
            identifiedHasher = null;
            if (this.romfis != null)
            {
                identifiedGame = this.romfis.IdentifyMixFile(baseMix, null, ref identifiedHasher);
                // don't specifically save it if we have a game ref.
                if (identifiedGame != null)
                {
                    identifiedHasher = null;
                }
            }
            openedMixFiles.Add(baseMix);
            initPaths = internalMixParts;
            analysisMultiThreader = new SimpleMultiThreading(this);
            analysisMultiThreader.ProcessingLabelBorder = BorderStyle.Fixed3D;
        }

        private void SetTitle()
        {
            this.Text = titleMain + " - " + string.Join(" -> ", openedMixFiles.Select(mix => mix.FileName).ToArray());
        }

        private void LoadMixContents(uint? idToSelect)
        {
            MixFile current = GetCurrentMix();
            if (current == null)
            {
                FillList(null, null);
            }
            analysisMultiThreader.ExecuteThreaded(
                () => MixContentAnalysis.AnalyseFiles(current, false, () => this.CheckAbort()),
                (list) => FillList(list, idToSelect), true,
                (bl, str) => EnableDisableUi(bl, str, analysisMultiThreader),
                "Analysis in progress");
        }

        private bool CheckAbort()
        {
            Boolean abort = false;
            lock (abortLockObj)
            {
                abort = abortRequested;
            }
            return abort;
        }

        private void EnableDisableUi(bool enableUI, string label, SimpleMultiThreading currentMultiThreader)
        {
            this.mixContentsListView.Enabled = enableUI;
            this.btnOpen.Enabled = enableUI;
            if (!enableUI)
            {
                this.btnCloseFile.Enabled = false;
                currentMultiThreader.CreateBusyLabel(this, label);
            }
            else
            {
                SimpleMultiThreading.RemoveBusyLabel(this);
                btnCloseFile.Enabled = openedMixFiles.Count > 1;
                mixContentsListView.Focus();
            }
        }

        private MixFile GetCurrentMix()
        {
            return openedMixFiles.LastOrDefault();
        }

        private void MixContentsListView_DoubleClick(object sender, EventArgs e)
        {
            OpenCurrentlySelected();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (analysisMultiThreader.IsExecuting)
            {
                lock (abortLockObj)
                {
                    abortRequested = true;
                }
                analysisMultiThreader.AbortThreadedOperation(5000);
            }
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            OpenCurrentlySelected();
        }

        private void MixContentsListView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData) {
                case Keys.Enter:
                    OpenCurrentlySelected();
                    e.Handled = true;
                    break;
                case Keys.Back:
                    CloseCurrentMixFile();
                    e.Handled = true;
                    break;
                case Keys.S | Keys.Control:
                    SaveCurrentlySelected();
                    e.Handled = true;
                    break;
            }
        }

        private void BtnCloseFile_Click(object sender, EventArgs e)
        {
            CloseCurrentMixFile();
        }

        private void SaveCurrentlySelected()
        {
            if (mixContentsListView.SelectedItems.Count == 0)
            {
                return;
            }
            MixEntry selected = mixContentsListView.SelectedItems[0].Tag as MixEntry;
            string savePath = null;
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "All files (*.*)|*.*";
                sfd.FileName = selected.Name ?? (selected.IdStringBare + "." + GetExtension(selected.Type));
                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    savePath = sfd.FileName;
                }
            }
            if (savePath != null)
            {
                MixFile current = GetCurrentMix();
                using (Stream readStream = current.OpenFile(selected))
                using (FileStream writeStream = new FileStream(savePath, FileMode.Create))
                {
                    readStream.CopyTo(writeStream);
                }
            }
        }

        private string GetExtension(MixContentType type)
        {
            switch (type)
            {
                case MixContentType.Mix:
                    return "mix";
                case MixContentType.MapTd:
                    return "ini";
                case MixContentType.MapRa:
                    return "ini";
                case MixContentType.MapSole:
                    return "ini";
                case MixContentType.Ini:
                    return "ini";
                case MixContentType.Strings:
                    return "eng";
                case MixContentType.Text:
                    return "txt";
                case MixContentType.Bin:
                    return "bin";
                case MixContentType.BinSole:
                    return "bin";
                case MixContentType.ShpD2:
                    return "shp";
                case MixContentType.ShpTd:
                    return "shp";
                case MixContentType.TmpTd:
                    return "tmp";
                case MixContentType.TmpRa:
                    return "tmp";
                case MixContentType.Cps:
                    return "cps";
                case MixContentType.Wsa:
                    return "wsa";
                case MixContentType.Font:
                    return "fnt";
                case MixContentType.Pcx:
                    return "pcx";
                case MixContentType.Pal:
                    return "pal";
                case MixContentType.PalTbl:
                    return "pal";
                case MixContentType.Mrf:
                    return "mrf";
                case MixContentType.Audio:
                    return "aud";
                case MixContentType.Vqa:
                    return "vqa";
                case MixContentType.Vqp:
                    return "vqp";
                default:
                    return "dat";
            }
        }

        private void OpenCurrentlySelected()
        {
            if (mixContentsListView.SelectedItems.Count == 0)
            {
                return;
            }
            MixEntry selected = mixContentsListView.SelectedItems[0].Tag as MixEntry;
            string name = selected.Name;
            MixContentType type = selected.Type;
            if (type == MixContentType.Mix)
            {
                MixFile current = GetCurrentMix();
                MixFile subMix = null;
                try
                {
                    subMix = name != null ? new MixFile(current, name) : new MixFile(current, selected.Id);
                }
                catch
                {
                    // Not sure how this would ever be possible; detection works by successfully opening it.
                }
                if (subMix != null)
                {
                    // Game type is already determined by parent mix.
                    if (this.romfis != null)
                    {
                        HashMethod forcedHasher = identifiedHasher;
                        romfis.IdentifyMixFile(subMix, identifiedGame, ref forcedHasher);
                    }
                    openedMixFiles.Add(subMix);
                }
                LoadMixContents(null);
                btnCloseFile.Enabled = openedMixFiles.Count > 1;
            }
            else if (type == MixContentType.MapRa)
            {
                // Open that mofo!
                SelectedFile = MixPath.BuildMixPath(openedMixFiles, selected);
                this.DialogResult = DialogResult.OK;
            }
            else if (type == MixContentType.MapTd || type == MixContentType.MapSole)
            {
                MixEntry iniEntry = selected;
                MixEntry binEntry;
                if (name == null)
                {
                    // TODO Inform user that accompanying bin is impossible to find without name, and ask if user wants to open it on a blank terrain map.
                    binEntry = null;
                }
                else
                {
                    // try to find accompanying .bin file
                    string binName = Path.GetFileNameWithoutExtension(name) + ".bin";
                    binEntry = currentMixInfo.FirstOrDefault(me => binName.Equals(me.Name, StringComparison.OrdinalIgnoreCase));
                    if (binEntry == null)
                    {
                        // TODO Inform user that accompanying bin was not found, and ask if user wants to open it on a blank terrain map.
                    }
                }
                SelectedFile = MixPath.BuildMixPath(openedMixFiles, iniEntry, binEntry);
                this.DialogResult = DialogResult.OK;
            }
            else if (type == MixContentType.Bin || type == MixContentType.BinSole)
            {
                MixEntry iniEntry;
                MixEntry binEntry = selected;
                if (name == null)
                {
                    // TODO Inform user that accompanying ini is impossible to find without name, and that a map can't be opened without ini.
                    return;
                }
                // try to find accompanying .ini file
                string iniName = Path.GetFileNameWithoutExtension(name) + ".ini";
                iniEntry = currentMixInfo.FirstOrDefault(me => iniName.Equals(me.Name, StringComparison.OrdinalIgnoreCase));
                if (iniEntry == null)
                {
                    // TODO Inform user that accompanying bin was not found, and that a map can't be opened without ini.
                    return;
                }
                SelectedFile = MixPath.BuildMixPath(openedMixFiles, iniEntry, binEntry);
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("This file type cannot be handled by the map editor. Only map files (indicated as green) and mix files (indicated as yellow) can be opened.", titleMain);
            }
        }

        private void CloseCurrentMixFile()
        {
            if (openedMixFiles.Count <= 1)
            {
                return;
            }
            MixFile current = GetCurrentMix();
            openedMixFiles.Remove(current);
            btnCloseFile.Enabled = openedMixFiles.Count > 1;
            uint id = current.FileName == null ? current.FileId : GetCurrentMix().GetFileId(current.FileName);
            LoadMixContents(id);
        }

        private void FillList(List<MixEntry> mixInfo, uint? idToSelect)
        {
            currentMixInfo = mixInfo;
            if (mixInfo == null)
            {
                mixContentsListView.Items.Clear();
                return;
            }
            SetTitle();
            mixContentsListView.BeginUpdate();
            mixContentsListView.Items.Clear();
            int nrofFiles = mixInfo.Count;
            bool selectFirst = !idToSelect.HasValue;
            ListViewItem selected = null;
            for (int i = 0; i < nrofFiles; ++i)
            {
                MixEntry mixFileInfo = mixInfo[i];
                MixContentType mt = mixFileInfo.Type;
                ListViewItem item = new ListViewItem(mixFileInfo.DisplayName)
                {
                    Tag = mixFileInfo,
                };
                if (selectFirst || idToSelect.HasValue && mixFileInfo.Id == idToSelect.Value)
                {
                    selected = item;
                    idToSelect = null;
                    selectFirst = false;
                }
                switch (mixFileInfo.Type)
                {
                    case MixContentType.MapTd:
                    case MixContentType.MapSole:
                    case MixContentType.Bin:
                    case MixContentType.BinSole:
                    case MixContentType.MapRa:
                        item.BackColor = Color.FromArgb(0xFF, 0xD0, 0xFF, 0xD0); //Color.LightGreen;
                        break;
                    case MixContentType.Mix:
                        item.BackColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0x80); // Color.LightYellow
                        break;
                }
                item.SubItems.Add(mixFileInfo.Type.ToString());
                item.SubItems.Add(mixFileInfo.Length.ToString());
                item.SubItems.Add(mixFileInfo.Description);
                item.SubItems.Add(mixFileInfo.Info);
                mixContentsListView.Items.Add(item).ToolTipText = mixFileInfo.Name ?? mixFileInfo.IdString;
            }
            mixContentsListView.EndUpdate();
            if (selected != null)
            {
                selected.Selected = true;
            }
        }

        private void MixContentsListView_SizeChanged(object sender, EventArgs e)
        {
            if (resizing)
            {
                return;
            }
            resizing = true;
            ListView listView = sender as ListView;
            try
            {
                if (listView == null)
                {
                    return;
                }
                listView.BeginUpdate();
                float totalColumnWidth = 0;
                int totalAvailablewidth = listView.ClientRectangle.Width;
                int availablewidth = totalAvailablewidth;
                int columns = listView.Columns.Count;
                int[] tagWidths = new int[columns];
                // Get the sum of all column tags
                for (int i = 0; i < columns; ++i)
                {
                    int tagWidth;
                    if (Int32.TryParse((listView.Columns[i].Tag ?? String.Empty).ToString(), out tagWidth))
                    {
                        tagWidths[i] = tagWidth;
                        if (tagWidth > 0)
                        {
                            totalColumnWidth += tagWidth;
                        }
                        else
                        {
                            availablewidth = Math.Max(0, availablewidth + tagWidth);
                        }
                    }
                }
                float fraction = availablewidth / totalColumnWidth;
                int[] colWidths = new int[columns];
                int total = 0;
                for (int i = 0; i < columns; ++i)
                {
                    int tagwidth = tagWidths[i];
                    int actualWidth = tagwidth > 0 ? (int)(fraction * tagwidth) : -1 * tagwidth;
                    colWidths[i] = actualWidth;
                    total += actualWidth;
                }
                int diff = totalAvailablewidth - total;
                if (columns > 0)
                {
                    while (diff > 0)
                    {
                        for (int i = 0; i < columns; ++i)
                        {
                            if (tagWidths[i] > 0 && colWidths[i] > 0)
                            {
                                colWidths[i]--;
                                diff--;
                                if (diff == 0)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    while (diff < 0)
                    {
                        for (int i = 0; i < columns; ++i)
                        {
                            colWidths[i]++;
                            diff++;
                            if (diff == 0)
                            {
                                break;
                            }
                        }
                    }
                }
                for (int i = 0; i < columns; ++i)
                {
                    listView.Columns[i].Width = colWidths[i];
                }
            }
            finally
            {
                listView.EndUpdate();
                resizing = false;
            }
        }

        private void OpenFromMixDialog_Load(object sender, EventArgs e)
        {
            MixContentsListView_SizeChanged(mixContentsListView, EventArgs.Empty);
            if (initPaths != null)
            {
                foreach (string mixName in initPaths)
                {
                    MixFile currentMix = GetCurrentMix();
                    MixEntry[] fileInfos = currentMix.GetFullFileInfo(mixName);
                    bool found = false;
                    foreach (MixEntry fileInfo in fileInfos)
                    {
                        if (fileInfo == null || !MixFile.CheckValidMix(currentMix, fileInfo, true))
                        {
                            continue;
                        }
                        MixFile subMix = new MixFile(currentMix, fileInfo, true);
                        if (this.romfis != null)
                        {
                            romfis.IdentifyMixFile(subMix, identifiedGame);
                        }
                        openedMixFiles.Add(subMix);
                        found = true;
                        break;
                    }
                    // If any parts of the mix path are not found, stop going deeper.
                    if (!found)
                    {
                        break;
                    }
                }
            }
            LoadMixContents(null);
        }

        private void mixContentsListView_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            if (resizing || !(sender is ListView lv))
                return;
            e.Cancel = true;
            e.NewWidth = lv.Columns[e.ColumnIndex].Width;
        }
    }
}