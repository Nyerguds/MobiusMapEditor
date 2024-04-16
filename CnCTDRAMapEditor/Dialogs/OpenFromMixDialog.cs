using MobiusEditor.Controls;
using MobiusEditor.Interface;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class OpenFromMixDialog : Form, IHasStatusLabel
    {
        private bool resizing = false;
        private List<MixFile> openedMixFiles = new List<MixFile>();
        private MixFile baseMix;
        private Dictionary<uint, string> encodedFilenames;
        private List<MixEntry> currentMixInfo;
        private SimpleMultiThreading analysisMultiThreader;

        public Label StatusLabel { get; set; }

        public OpenFromMixDialog(MixFile baseMix, Dictionary<uint, string> encodedFilenames)
        {
            InitializeComponent();
            this.encodedFilenames = encodedFilenames;
            this.baseMix = baseMix;
            openedMixFiles.Add(baseMix);
            analysisMultiThreader = new SimpleMultiThreading(this);
            analysisMultiThreader.ProcessingLabelBorder = BorderStyle.Fixed3D;
        }

        private void LoadMixContents()
        {
            MixFile current = GetCurrentMix();
            if (current == null)
            {
                FillList(null);
            }
            analysisMultiThreader.ExecuteThreaded(
                () => MixContentAnalysis.AnalyseFiles(current, this.encodedFilenames),
                (list) => FillList(list), true,
                (bl, str) => EnableDisableUi(bl, str, analysisMultiThreader),
                "Analysing MIX Contents");
        }

        private void EnableDisableUi(bool enableUI, string label, SimpleMultiThreading currentMultiThreader)
        {
            if (!enableUI)
            {
                currentMultiThreader.CreateBusyLabel(this, label);
            }
            else
            {
                SimpleMultiThreading.RemoveBusyLabel(this);
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

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenCurrentlySelected();
        }

        private void OpenCurrentlySelected()
        {
            if (mixContentsListView.SelectedItems.Count == 0)
            {
                return;
            }
            MixEntry selected = mixContentsListView.SelectedItems[0].Tag as MixEntry;
            if (selected.Type == MixContentType.Mix)
            {
                MixFile current = GetCurrentMix();
                MixFile subMix = null;
                try
                {
                    subMix = selected.Name != null ? new MixFile(current, selected.Name) : new MixFile(current, selected.Id);
                }
                catch
                {
                    // Ignore for now.
                }
                if(subMix != null)
                openedMixFiles.Add(subMix);
                LoadMixContents();
                btnCloseFile.Enabled = openedMixFiles.Count > 1;
            }
        }

        private void BtnCloseFile_Click(object sender, EventArgs e)
        {
            if (openedMixFiles.Count <= 1)
            {
                return;
            }
            MixFile current = GetCurrentMix();
            openedMixFiles.Remove(current);
            btnCloseFile.Enabled = openedMixFiles.Count > 1;
            LoadMixContents();
        }

        private void OpenFromMixDialog_Load(object sender, EventArgs e)
        {
            if (currentMixInfo != null)
            {
                FillList(currentMixInfo);
            }
        }

        private void FillList(List<MixEntry> mixInfo)
        {
            mixContentsListView.BeginUpdate();
            mixContentsListView.Items.Clear();
            if (mixInfo == null)
            {
                return;
            }
            int nrofFiles = mixInfo.Count;
            for (int i = 0; i < nrofFiles; ++i)
            {
                MixEntry mixFileInfo = mixInfo[i];
                var item = new ListViewItem(mixFileInfo.DisplayName)
                {
                    Tag = mixFileInfo
                };
                item.SubItems.Add(mixFileInfo.Type.ToString());
                item.SubItems.Add(mixFileInfo.Info);
                mixContentsListView.Items.Add(item).ToolTipText = mixFileInfo.Name;
            }
            mixContentsListView.EndUpdate();
        }

        private void MixContentsListView_SizeChanged(object sender, EventArgs e)
        {
            if (resizing)
            {
                return;
            }
            try
            {
                resizing = true;
                ListView listView = sender as ListView;
                if (listView == null)
                {
                    return;
                }
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
                    int actualWidth = tagwidth >= 0 ? (int)(fraction * tagwidth) : -1 * tagwidth;
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
                            if (colWidths[i] > 0)
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
                resizing = false;
            }
        }

        private void OpenFromMixDialog_Shown(object sender, EventArgs e)
        {
            MixContentsListView_SizeChanged(mixContentsListView, EventArgs.Empty);
            LoadMixContents();
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