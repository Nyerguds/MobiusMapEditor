﻿//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
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
using MobiusEditor.Utility;
using MobiusEditor.Utility.Hashing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class OpenFromMixDialog : Form, IHasStatusLabel
    {
        private static readonly Regex isNumber = new Regex("^-?\\d+$");
        // ID string, possibly with repeat index behind it
        private static readonly Regex isId = new Regex("^\\[[0-9A-F]{8}\\]( \\(\\d+\\))?$");

        private ListViewColumnSorter lvwColumnSorter;
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
            lvwColumnSorter = new ListViewColumnSorter();
            Comparer<string>[] columnComparers = new Comparer<string>[7];
            // This follows the same logic as MixEntry.SortName
            Comparer<string> idComparer = Comparer<string>.Create((s1, s2) => {
                string st1 = isId.IsMatch(s1) ? ("zzzzzzzzzzzz" + s1) : s1;
                string st2 = isId.IsMatch(s2) ? ("zzzzzzzzzzzz" + s2) : s2;
                return lvwColumnSorter.DefaultObjectComparer.Compare(st1, st2);
            });
            Comparer<string> numComparer = Comparer<string>.Create((s1, s2) => {
                string st1 = String.IsNullOrEmpty(s1) ? "0" : s1;
                string st2 = String.IsNullOrEmpty(s2) ? "0" : s2;
                if (!isNumber.IsMatch(st1) || !isNumber.IsMatch(st2))
                {
                    return lvwColumnSorter.DefaultObjectComparer.Compare(st1, st2);
                }
                return int.Parse(st1).CompareTo(int.Parse(st2));
            });
            columnComparers[0] = idComparer; // Name or id
            columnComparers[2] = numComparer; // Size
            columnComparers[5] = numComparer; // Index in header
            columnComparers[6] = numComparer; // File offset
            lvwColumnSorter.SpecificComparers = columnComparers;
            this.mixContentsListView.ListViewItemSorter = lvwColumnSorter;
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
            this.Text = titleMain + " - " + string.Join(" → ", openedMixFiles.Select(mix => mix.FileName).ToArray());
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
            if (!(sender is ListView lv))
            {
                return;
            }
            // List navigation is messed up by the fact the actual "selection caret" can't be
            // moved by any program instructions. There also seem to be OS differences in how
            // PageUp and PageDown are handled. So we just ignore it and take full control.
            // Home and End are the only keys not affected by any of this.
            switch (e.KeyData)
            {
                case Keys.Up:
                    MoveSelectionUpDown(lv, true);
                    e.Handled = true;
                    break;
                case Keys.Down:
                    MoveSelectionUpDown(lv, false);
                    e.Handled = true;
                    break;
                case Keys.PageUp:
                    MoveSelectionPageUpDown(lv, true);
                    e.Handled = true;
                    break;
                case Keys.PageDown:
                    MoveSelectionPageUpDown(lv, false);
                    e.Handled = true;
                    break;
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

        private void MoveSelectionUpDown(ListView lv, bool up)
        {
            int oldIndex = lv.SelectedIndices.Count == 0 ? 0 : lv.SelectedIndices[0];
            int newIndex = Math.Min(lv.Items.Count - 1, Math.Max(0, oldIndex + (up ? -1 : 1)));
            if (lv.Items.Count > oldIndex)
            {
                lv.Items[oldIndex].Selected = false;
            }
            if (lv.Items.Count > newIndex)
            {
                lv.Items[newIndex].Selected = true;
                lv.Items[newIndex].EnsureVisible();
            }
        }

        private void MoveSelectionPageUpDown(ListView lv, bool up)
        {
            (int first, int last) = getVisibleItems(lv);
            if (first == -1 || last == -1)
            {
                return;
            }
            int maxScrollAmount = last - first;
            int oldIndex = lv.SelectedIndices.Count == 0 ? 0 : lv.SelectedIndices[0];
            int newIndex;
            bool onExtreme = (up && oldIndex == first) || (!up && oldIndex == last);
            if (oldIndex < first || oldIndex > last || onExtreme)
            {
                // if outside view, or on the last visible item in this direction, just scroll by the max amount.
                newIndex = Math.Min(lv.Items.Count - 1, Math.Max(0, oldIndex + (up ? -1 : 1) * maxScrollAmount));
            }
            else
            {
                // If the selected item is visible, and not on the last visible item in this direction,
                // put it on the last visible item in this direction.
                newIndex = up ? first : last;
            }
            if (lv.Items.Count > oldIndex)
            {
                lv.Items[oldIndex].Selected = false;
            }
            lv.Items[newIndex].Selected = true;
            lv.Items[newIndex].EnsureVisible();
        }

        private (int, int) getVisibleItems(ListView lv)
        {
            ListViewItem first = lv.TopItem;
            if (lv.Items.Count == 0 || first == null)
            {
                return (-1, -1);
            }
            int offset = first.GetBounds(ItemBoundsPortion.Entire).Y;
            Rectangle clientRect = lv.ClientRectangle;
            // Remove header, using offset of first visible item.
            clientRect = new Rectangle(clientRect.X, clientRect.Y + offset, clientRect.Width, clientRect.Height - offset);
            ListViewItem last = null;
            for (int i = first.Index; i < lv.Items.Count; ++i)
            {
                ListViewItem item = lv.Items[i];
                Rectangle itemBounds = item.GetBounds(ItemBoundsPortion.Entire);
                // items in ListView are only selected if they are fully inside.
                if (!clientRect.IntersectsWith(itemBounds) || itemBounds.Bottom > clientRect.Bottom)
                {
                    // passed visible area.
                    break;
                }
                last = item;
            }
            return (first.Index, last == null ? -1 : last.Index);
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
                    subMix = new MixFile(current, selected);
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
                MessageBox.Show("This file type cannot be handled by the map editor. Only map files (indicated as green) and mix files (indicated as yellow) can be opened." +
                    "\n\nHowever, you can press Ctrl+S to extract the selected file from the archive.", titleMain);
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
            if (mixInfo == null)
            {
                currentMixInfo = null;
                mixContentsListView.Items.Clear();
                return;
            }
            lvwColumnSorter.SortColumn = 0;
            lvwColumnSorter.SortOrder = SortOrder.Ascending;
            mixInfo = mixInfo.OrderBy(x => x.SortName).ToList();
            currentMixInfo = mixInfo;
            SetTitle();
            mixContentsListView.BeginUpdate();
            mixContentsListView.Items.Clear();
            int nrofFiles = mixInfo.Count;
            bool selectId = idToSelect.HasValue;
            bool selectFirst = !selectId;
            int toSelectIndex = -1;
            ListViewItem toSelect = null;
            bool selectFirstMission = !selectId;
            int missToSelectIndex = -1;
            int lastMissIndex = -1;
            ListViewItem missToSelect = null;
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
                    toSelect = item;
                    toSelectIndex = i;
                    idToSelect = null;
                    selectFirst = false;
                }
                switch (mt)
                {
                    case MixContentType.MapTd:
                    case MixContentType.MapSole:
                    case MixContentType.Bin:
                    case MixContentType.BinSole:
                    case MixContentType.MapRa:
                        item.BackColor = Color.FromArgb(0xFF, 0xD0, 0xFF, 0xD0); //Color.LightGreen;
                        if (selectFirstMission)
                        {
                            missToSelect = item;
                            missToSelectIndex = i;
                            selectFirstMission = false;
                        }
                        lastMissIndex = i;
                        break;
                    case MixContentType.Mix:
                        item.BackColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0x80); // Color.LightYellow
                        break;
                }
                item.SubItems.Add(mt.ToString());
                item.SubItems.Add(mixFileInfo.Length.ToString());
                item.SubItems.Add(mixFileInfo.Description);
                item.SubItems.Add(mixFileInfo.Info);
                item.SubItems.Add(mixFileInfo.Index.ToString());
                item.SubItems.Add(mixFileInfo.Offset.ToString());
                mixContentsListView.Items.Add(item).ToolTipText = mixFileInfo.Name ?? mixFileInfo.IdString;
            }
            mixContentsListView.EndUpdate();
            if (missToSelect != null && !(selectId && toSelect != null))
            {
                missToSelect.Selected = true;
                mixContentsListView.EnsureVisible(lastMissIndex);
                mixContentsListView.EnsureVisible(missToSelectIndex);
            }
            else if (toSelect != null)
            {
                toSelect.Selected = true;
                mixContentsListView.EnsureVisible(toSelectIndex);
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
                        // Could get stuck if all adjustable columns are zero
                        bool anyHandled = false;
                        for (int i = 0; i < columns; ++i)
                        {
                            if (tagWidths[i] > 0 && colWidths[i] > 0)
                            {
                                anyHandled= true;
                                colWidths[i]--;
                                diff--;
                                if (diff == 0)
                                {
                                    break;
                                }
                            }
                        }
                        if (!anyHandled)
                        {
                            break;
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

        private void mixContentsListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListView listView = sender as ListView;
            if (listView == null)
            {
                return;
            }
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.SortOrder == SortOrder.Ascending)
                {
                    lvwColumnSorter.SortOrder = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.SortOrder = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.SortOrder = SortOrder.Ascending;
            }
            // Perform the sort with these new sort options.
            listView.Sort();
        }
    }
}