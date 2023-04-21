﻿//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free 
// software: you can redistribute it and/or modify it under the terms of 
// the GNU General Public License as published by the Free Software Foundation, 
// either version 3 of the License, or (at your option) any later version.

// The Command & Conquer Map Editor and corresponding source code is distributed 
// in the hope that it will be useful, but with permitted additional restrictions 
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT 
// distributed with this program. You should have received a copy of the 
// GNU General Public License along with permitted additional restrictions 
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using MobiusEditor.Controls;
using MobiusEditor.Dialogs;
using MobiusEditor.Event;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Tools;
using MobiusEditor.Tools.Dialogs;
using MobiusEditor.Utility;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MobiusEditor
{
    public partial class MainForm : Form, IFeedBackHandler, IHasStatusLabel
    {

        public delegate Object FunctionInvoker();

        public Dictionary<GameType, string[]> ModPaths { get; set; }

        private Dictionary<int, Bitmap> theaterIcons = new Dictionary<int, Bitmap>();

        private static readonly ToolType[] toolTypes;

        private ToolType availableToolTypes = ToolType.None;

        private ToolType activeToolType = ToolType.None;
        private ToolType ActiveToolType
        {
            get => activeToolType;
            set
            {
                var firstAvailableTool = value;
                if ((availableToolTypes & firstAvailableTool) == ToolType.None)
                {
                    var otherAvailableToolTypes = toolTypes.Where(t => (availableToolTypes & t) != ToolType.None);
                    firstAvailableTool = otherAvailableToolTypes.Any() ? otherAvailableToolTypes.First() : ToolType.None;
                }
                if (activeToolType != firstAvailableTool || activeTool == null)
                {
                    activeToolType = firstAvailableTool;
                    RefreshActiveTool();
                }
            }
        }

        private MapLayerFlag activeLayers;
        public MapLayerFlag ActiveLayers
        {
            get => activeLayers;
            set
            {
                if (activeLayers != value)
                {
                    activeLayers = value;
                    if (activeTool != null)
                    {
                        activeTool.Layers = activeLayers;
                    }
                }
            }
        }

        private ITool activeTool;
        private Form activeToolForm;

        // Save and re-use tool instances
        private Dictionary<ToolType, IToolDialog> toolForms;
        private ViewToolStripButton[] viewToolStripButtons;

        private IGamePlugin plugin;
        private FileType loadedFileType;
        private string filename;
        private readonly object jumpToBounds_lock = new Object();
        private bool jumpToBounds;

        private readonly MRU mru;

        private readonly UndoRedoList<UndoRedoEventArgs> url = new UndoRedoList<UndoRedoEventArgs>(Globals.UndoRedoStackSize);

        private readonly Timer steamUpdateTimer = new Timer();

        private SimpleMultiThreading loadMultiThreader;
        private SimpleMultiThreading saveMultiThreader;
        public Label StatusLabel { get; set; }

        static MainForm()
        {
            toolTypes = ((IEnumerable<ToolType>)Enum.GetValues(typeof(ToolType))).Where(t => t != ToolType.None).ToArray();
        }

        public MainForm(String fileToOpen)
        {
            this.filename = fileToOpen;

            InitializeComponent();
            // Loaded from global settings.
            toolsOptionsBoundsObstructFillMenuItem.Checked = Globals.BoundsObstructFill;
            toolsOptionsSafeDraggingMenuItem.Checked = Globals.TileDragProtect;
            toolsOptionsRandomizeDragPlaceMenuItem.Checked = Globals.TileDragRandomize;
            toolsOptionsPlacementGridMenuItem.Checked = Globals.ShowPlacementGrid;
            toolsOptionsOutlineAllCratesMenuItem.Checked = Globals.OutlineAllCrates;
            toolsOptionsCratesOnTopMenuItem.Checked = Globals.CratesOnTop;
            viewExtraIndicatorsMapGridMenuItem.Checked = Globals.ShowMapGrid;
            // Obey the settings.
            this.mapPanel.SmoothScale = Globals.MapSmoothScale;
            this.mapPanel.BackColor = Globals.MapBackColor;
            SetTitle();
            toolForms = new Dictionary<ToolType, IToolDialog>();
            viewToolStripButtons = new ViewToolStripButton[]
            {
                mapToolStripButton,
                smudgeToolStripButton,
                overlayToolStripButton,
                terrainToolStripButton,
                infantryToolStripButton,
                unitToolStripButton,
                buildingToolStripButton,
                resourcesToolStripButton,
                wallsToolStripButton,
                waypointsToolStripButton,
                cellTriggersToolStripButton,
                selectToolStripButton,
            };
            mru = new MRU("Software\\Petroglyph\\CnCRemasteredEditor", 10, fileRecentFilesMenuItem);
            mru.FileSelected += Mru_FileSelected;
            foreach (ToolStripButton toolStripButton in mainToolStrip.Items)
            {
                toolStripButton.MouseMove += MainToolStrip_MouseMove;
            }
#if !DEVELOPER
            fileExportMenuItem.Enabled = false;
            fileExportMenuItem.Visible = false;
            developerToolStripMenuItem.Visible = false;
#endif
            url.Tracked += UndoRedo_Updated;
            url.Undone += UndoRedo_Updated;
            url.Redone += UndoRedo_Updated;
            UpdateUndoRedo();
            steamUpdateTimer.Interval = 500;
            steamUpdateTimer.Tick += SteamUpdateTimer_Tick;
            loadMultiThreader = new SimpleMultiThreading(this);
            loadMultiThreader.ProcessingLabelBorder = BorderStyle.Fixed3D;
            saveMultiThreader = new SimpleMultiThreading(this);
            saveMultiThreader.ProcessingLabelBorder = BorderStyle.Fixed3D;
        }

        private void SetTitle()
        {
            const string noname = "Untitled";
            String mainTitle = GetProgramVersionTitle();
            if (plugin == null)
            {
                this.Text = mainTitle;
                return;
            }
            string mapName = plugin.Map.BasicSection.Name;
            if (plugin.MapNameIsEmpty(mapName))
            {
                if (filename != null)
                {
                    mapName = Path.GetFileName(filename);
                }
                else
                {
                    mapName = noname;
                }
            }
            this.Text = string.Format("{0} [{1}] - {2}{3}", mainTitle, plugin.Name, mapName, plugin != null && plugin.Dirty ? " *" : String.Empty);
        }

        private String GetProgramVersionTitle()
        {
            AssemblyName assn = Assembly.GetExecutingAssembly().GetName();
            System.Version currentVersion = assn.Version;
            return string.Format("Mobius Editor v{0}", currentVersion);
        }

        private void SteamUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (SteamworksUGC.IsInit)
            {
                SteamworksUGC.Service();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((keyData & (Keys.Shift | Keys.Control | Keys.Alt)) == Keys.None)
            {
                // Evaluates the scan codes directly, so this will automatically turn into a, z, e, r, t, y, etc on an azerty keyboard.
                switch (Keyboard.GetScanCode(msg))
                {
                    case OemScanCode.Q:
                        mapToolStripButton.PerformClick();
                        return true;
                    case OemScanCode.W:
                        smudgeToolStripButton.PerformClick();
                        return true;
                    case OemScanCode.E:
                        overlayToolStripButton.PerformClick();
                        return true;
                    case OemScanCode.R:
                        terrainToolStripButton.PerformClick();
                        return true;
                    case OemScanCode.T:
                        infantryToolStripButton.PerformClick();
                        return true;
                    case OemScanCode.Y:
                        unitToolStripButton.PerformClick();
                        return true;
                    case OemScanCode.A:
                        buildingToolStripButton.PerformClick();
                        return true;
                    case OemScanCode.S:
                        resourcesToolStripButton.PerformClick();
                        return true;
                    case OemScanCode.D:
                        wallsToolStripButton.PerformClick();
                        return true;
                    case OemScanCode.F:
                        waypointsToolStripButton.PerformClick();
                        return true;
                    case OemScanCode.G:
                        cellTriggersToolStripButton.PerformClick();
                        return true;
                    case OemScanCode.H:
                        selectToolStripButton.PerformClick();
                        return true;
                }
                // Map navigation shortcuts (zoom and move the camera around)
                if (plugin != null && mapPanel.MapImage != null && activeTool != null)
                {
                    Point delta = Point.Empty;
                    switch (keyData)
                    {
                        case Keys.Up:
                            delta.Y -= 1;
                            break;
                        case Keys.Down:
                            delta.Y += 1;
                            break;
                        case Keys.Left:
                            delta.X -= 1;
                            break;
                        case Keys.Right:
                            delta.X += 1;
                            break;
                        case Keys.Oemplus:
                        case Keys.Add:
                            mapPanel.IncreaseZoomStep();
                            return true;
                        case Keys.OemMinus:
                        case Keys.Subtract:
                            mapPanel.DecreaseZoomStep();
                            return true;
                    }
                    if (delta != Point.Empty)
                    {
                        Point curPoint = mapPanel.AutoScrollPosition;
                        SizeF zoomedCell = activeTool.NavigationWidget.ZoomedCellSize;
                        // autoscrollposition is WEIRD. Exposed as negative, needs to be given as positive.
                        mapPanel.AutoScrollPosition = new Point(-curPoint.X + (int)(delta.X * zoomedCell.Width), -curPoint.Y + (int)(delta.Y * zoomedCell.Width));
                        return true;
                    }
                }
            }
            else if (keyData == (Keys.Control | Keys.Z))
            {
                if (editUndoMenuItem.Enabled)
                {
                    EditUndoMenuItem_Click(this, new EventArgs());
                }
                return true;
            }
            else if (keyData == (Keys.Control | Keys.Y))
            {
                if (editRedoMenuItem.Enabled)
                {
                    EditRedoMenuItem_Click(this, new EventArgs());
                }
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void UpdateUndoRedo()
        {
            editUndoMenuItem.Enabled = url.CanUndo;
            editRedoMenuItem.Enabled = url.CanRedo;
            editClearUndoRedoMenuItem.Enabled = url.CanUndo || url.CanRedo;
        }

        private void UndoRedo_Updated(object sender, EventArgs e)
        {
            UpdateUndoRedo();
        }

        #region listeners

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RefreshUI();
            UpdateVisibleLayers();
            steamUpdateTimer.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            steamUpdateTimer.Stop();
            steamUpdateTimer.Dispose();
        }

        private void FileNewMenuItem_Click(object sender, EventArgs e)
        {
            NewFileAsk(false, null, false);
        }

        private void FileNewFromImageMenuItem_Click(object sender, EventArgs e)
        {
            NewFileAsk(true, null, false);
        }

        private void FileOpenMenuItem_Click(object sender, EventArgs e)
        {
            PromptSaveMap(OpenFile, false);
        }

        private void OpenFile()
        {
            // Always remove the label when showing an Open File dialog.
            SimpleMultiThreading.RemoveBusyLabel(this);
            var pgmFilter = "|PGM files (*.pgm)|*.pgm";
            string allSupported = "All supported types (*.ini;*.bin;*.mpr;*.pgm)|*.ini;*.bin;*.mpr;*.pgm";
            String selectedFileName = null;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.AutoUpgradeEnabled = false;
                ofd.RestoreDirectory = true;
                ofd.Filter = allSupported + "|Tiberian Dawn files (*.ini;*.bin)|*.ini;*.bin|Red Alert files (*.mpr;*.ini)|*.mpr;*.ini" + pgmFilter + "|All files (*.*)|*.*";
                if (plugin != null)
                {
                    switch (plugin.GameType)
                    {
                        case GameType.TiberianDawn:
                        case GameType.SoleSurvivor:
                            ofd.InitialDirectory = Path.GetDirectoryName(filename) ?? TiberianDawn.Constants.SaveDirectory;
                            //ofd.FilterIndex = 2;
                            break;
                        case GameType.RedAlert:
                            ofd.InitialDirectory = Path.GetDirectoryName(filename) ?? RedAlert.Constants.SaveDirectory;
                            //ofd.FilterIndex = 3;
                            break;
                    }
                }
                else
                {
                    ofd.InitialDirectory = Globals.RootSaveDirectory;
                }
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    selectedFileName = ofd.FileName;
                }
            }
            if (selectedFileName != null)
            {
                OpenFile(selectedFileName);
            }
        }

        private void FileSaveMenuItem_Click(object sender, EventArgs e)
        {
            SaveAction(false, null);
        }

        private void SaveAction(bool dontResavePreview, Action afterSaveDone)
        {
            if (plugin == null)
            {
                afterSaveDone?.Invoke();
                return;
            }
            if (string.IsNullOrEmpty(filename) || !Directory.Exists(Path.GetDirectoryName(filename)))
            {
                SaveAsAction(afterSaveDone);
                return;
            }
            String errors = plugin.Validate();
            if (errors != null)
            {
                MessageBox.Show(errors, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                return;
            }
            var fileInfo = new FileInfo(filename);
            SaveChosenFile(fileInfo.FullName, loadedFileType, dontResavePreview, afterSaveDone);
        }

        private void FileSaveAsMenuItem_Click(object sender, EventArgs e)
        {
            SaveAsAction(null);
        }

        private void SaveAsAction(Action afterSaveDone)
        {
            if (plugin == null)
            {
                afterSaveDone?.Invoke();
                return;
            }
            String errors = plugin.Validate();
            if (errors != null)
            {
                MessageBox.Show(errors, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string savePath = null;
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.AutoUpgradeEnabled = false;
                sfd.RestoreDirectory = true;

                var filters = new List<string>();
                switch (plugin.GameType)
                {
                    case GameType.TiberianDawn:
                    case GameType.SoleSurvivor:
                        filters.Add("Tiberian Dawn files (*.ini;*.bin)|*.ini;*.bin");
                        sfd.InitialDirectory = TiberianDawn.Constants.SaveDirectory;
                        break;
                    case GameType.RedAlert:
                        filters.Add("Red Alert files (*.mpr;*.ini)|*.mpr;*.ini");
                        sfd.InitialDirectory = RedAlert.Constants.SaveDirectory;
                        break;
                }
                filters.Add("All files (*.*)|*.*");
                sfd.Filter = string.Join("|", filters);
                if (!string.IsNullOrEmpty(filename))
                {
                    sfd.InitialDirectory = Path.GetDirectoryName(filename);
                    sfd.FileName = Path.GetFileName(filename);
                }
                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    savePath = sfd.FileName;
                }
            }
            if (savePath == null)
            {
                afterSaveDone?.Invoke();
            }
            else
            {
                var fileInfo = new FileInfo(savePath);
                SaveChosenFile(fileInfo.FullName, FileType.INI, false, afterSaveDone);
            }
        }

        private void FileExportMenuItem_Click(object sender, EventArgs e)
        {
#if DEVELOPER
            if (plugin == null)
            {
                return;
            }
            String errors = plugin.Validate();
            if (errors != null)
            {
                MessageBox.Show(errors, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string savePath = null;
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.AutoUpgradeEnabled = false;
                sfd.RestoreDirectory = true;

                sfd.Filter = "PGM files (*.pgm)|*.pgm";
                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    savePath = sfd.FileName;
                }
            }
            if (savePath != null)
            {
                plugin.Save(savePath, FileType.MEG);
            }
#endif
        }

        private void FileExitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void EditUndoMenuItem_Click(object sender, EventArgs e)
        {
            if (url.CanUndo)
            {
                url.Undo(new UndoRedoEventArgs(mapPanel, plugin));
            }
        }

        private void EditRedoMenuItem_Click(object sender, EventArgs e)
        {
            if (url.CanRedo)
            {
                url.Redo(new UndoRedoEventArgs(mapPanel, plugin));
            }
        }

        private void EditClearUndoRedoMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("This will remove all undo/redo information. Are you sure?", GetProgramVersionTitle(), MessageBoxButtons.YesNo))
            {
                url.Clear();
            }
        }

        private void SettingsMapSettingsMenuItem_Click(object sender, EventArgs e)
        {
            if (plugin == null)
            {
                return;
            }
            bool expansionEnabled = plugin.Map.BasicSection.ExpansionEnabled;
            bool rulesChanged = false;
            PropertyTracker<BasicSection> basicSettings = new PropertyTracker<BasicSection>(plugin.Map.BasicSection);
            PropertyTracker<BriefingSection> briefingSettings = new PropertyTracker<BriefingSection>(plugin.Map.BriefingSection);
            PropertyTracker<SoleSurvivor.CratesSection> cratesSettings = null;
            if (plugin.GameType == GameType.SoleSurvivor && plugin is SoleSurvivor.GamePluginSS ssPlugin)
            {
                cratesSettings = new PropertyTracker<SoleSurvivor.CratesSection>(ssPlugin.CratesSection);
            }
            string extraIniText = plugin.ExtraIniText;
            if (extraIniText.Trim('\r', '\n').Length == 0)
                extraIniText = String.Empty;
            Dictionary<House, PropertyTracker<House>> houseSettingsTrackers = plugin.Map.Houses.ToDictionary(h => h, h => new PropertyTracker<House>(h));
            using (MapSettingsDialog msd = new MapSettingsDialog(plugin, basicSettings, briefingSettings, cratesSettings, houseSettingsTrackers, extraIniText))
            {
                msd.StartPosition = FormStartPosition.CenterParent;
                if (msd.ShowDialog(this) == DialogResult.OK)
                {
                    bool hasChanges = basicSettings.HasChanges || briefingSettings.HasChanges;
                    basicSettings.Commit();
                    briefingSettings.Commit();
                    if (cratesSettings != null)
                    {
                        cratesSettings.Commit();
                    }
                    foreach (var houseSettingsTracker in houseSettingsTrackers.Values)
                    {
                        if (houseSettingsTracker.HasChanges)
                            hasChanges = true;
                        houseSettingsTracker.Commit();
                    }
                    // Combine diacritics into their characters, and remove characters not included in DOS-437.
                    string normalised = (msd.ExtraIniText ?? String.Empty).Normalize(NormalizationForm.FormC);
                    Encoding dos437 = Encoding.GetEncoding(437);
                    // DOS chars excluding specials at the start and end. Explicitly add tab, then the normal range from 32 to 254.
                    HashSet<Char> dos437chars = ("\t\r\n" + String.Concat(Enumerable.Range(32, 256 - 32 - 1).Select(i => dos437.GetString(new Byte[] { (byte)i })))).ToHashSet();
                    normalised = new String(normalised.Where(ch => dos437chars.Contains(ch)).ToArray());
                    // Ignore trivial line changes. This will not detect any irrelevant but non-trivial changes like swapping lines, though.
                    String checkTextNew = Regex.Replace(normalised, "[\\r\\n]+", "\n").Trim('\n');
                    String checkTextOrig = Regex.Replace(extraIniText ?? String.Empty, "[\\r\\n]+", "\n").Trim('\n');
                    if (!checkTextOrig.Equals(checkTextNew, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            plugin.ExtraIniText = normalised;
                        }
                        catch (Exception ex)
                        {
                            //MessageBox.Show("Errors occurred when applying rule changes:\n\n" + ex.Message, GetProgramVersionTitle());
                            using (ErrorMessageBox emb = new ErrorMessageBox())
                            {
                                emb.Title = GetProgramVersionTitle();
                                emb.Message = "Errors occurred when applying rule changes:";
                                emb.Errors = ex.Message.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
                                emb.StartPosition = FormStartPosition.CenterParent;
                                emb.ShowDialog(this);
                            }
                        }
                        rulesChanged = plugin.GameType == GameType.RedAlert;
                        hasChanges = true;
                    }
                    plugin.Dirty = hasChanges;
                }
            }
            if (rulesChanged || (expansionEnabled && !plugin.Map.BasicSection.ExpansionEnabled))
            {
                // If Aftermath units were disabled, we can't guarantee none of them are still in
                // the undo/redo history, so the undo/redo history is cleared to avoid issues.
                // The rest of the cleanup can be found in the ViewTool class, in the BasicSection_PropertyChanged function.
                // Rule changes will clear undo to avoid conflicts with placed smudge types.
                url.Clear();
            }
        }

        private void SettingsTeamTypesMenuItem_Click(object sender, EventArgs e)
        {
            if (plugin == null)
            {
                return;
            }
            int maxTeams = 0;
            switch (plugin.GameType)
            {
                case GameType.TiberianDawn:
                case GameType.SoleSurvivor:
                    maxTeams = TiberianDawn.Constants.MaxTeams;
                    break;
                case GameType.RedAlert:
                    maxTeams = RedAlert.Constants.MaxTeams;
                    break;
            }
            using (TeamTypesDialog ttd = new TeamTypesDialog(plugin, maxTeams))
            {
                ttd.StartPosition = FormStartPosition.CenterParent;
                if (ttd.ShowDialog(this) == DialogResult.OK)
                {
                    List<TeamType> oldTeamTypes = plugin.Map.TeamTypes.ToList();
                    // Clone of old triggers
                    List<Trigger> oldTriggers = plugin.Map.Triggers.Select(tr => tr.Clone()).ToList();
                    plugin.Map.TeamTypes.Clear();
                    plugin.Map.ApplyTeamTypeRenames(ttd.RenameActions);
                    // Triggers in their new state after the rename.
                    List<Trigger> newTriggers = plugin.Map.Triggers.Select(tr => tr.Clone()).ToList();
                    plugin.Map.TeamTypes.AddRange(ttd.TeamTypes.OrderBy(t => t.Name, new ExplorerComparer()).Select(t => t.Clone()));
                    List<TeamType> newTeamTypes = plugin.Map.TeamTypes.ToList();
                    bool origDirtyState = plugin.Dirty;
                    void undoAction(UndoRedoEventArgs ev)
                    {
                        DialogResult dr = MessageBox.Show(this, "This will undo all teamtype editing actions you performed. Are you sure you want to continue?",
                            "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (dr == DialogResult.No)
                        {
                            ev.Cancelled = true;
                            return;
                        }
                        if (ev.Plugin != null)
                        {
                            ev.Map.Triggers = oldTriggers;
                            ev.Map.TeamTypes.Clear();
                            ev.Map.TeamTypes.AddRange(oldTeamTypes);
                            ev.Plugin.Dirty = origDirtyState;
                        }
                    }
                    void redoAction(UndoRedoEventArgs ev)
                    {
                        DialogResult dr = MessageBox.Show(this, "This will redo all teamtype editing actions you undid. Are you sure you want to continue?",
                            "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (dr == DialogResult.No)
                        {
                            ev.Cancelled = true;
                            return;
                        }
                        if (ev.Plugin != null)
                        {
                            ev.Map.TeamTypes.Clear();
                            ev.Map.TeamTypes.AddRange(newTeamTypes);
                            ev.Map.Triggers = newTriggers;
                            ev.Plugin.Dirty = true;
                        }
                    }
                    url.Track(undoAction, redoAction);
                    plugin.Dirty = true;
                }
            }
        }

        private void SettingsTriggersMenuItem_Click(object sender, EventArgs e)
        {
            if (plugin == null)
            {
                return;
            }
            int maxTriggers = 0;
            switch (plugin.GameType)
            {
                case GameType.TiberianDawn:
                case GameType.SoleSurvivor:
                    maxTriggers = TiberianDawn.Constants.MaxTriggers;
                    break;
                case GameType.RedAlert:
                    maxTriggers = RedAlert.Constants.MaxTriggers;
                    break;
            }
            using (TriggersDialog td = new TriggersDialog(plugin, maxTriggers))
            {
                td.StartPosition = FormStartPosition.CenterParent;
                if (td.ShowDialog(this) == DialogResult.OK)
                {
                    List<Trigger> newTriggers = td.Triggers.OrderBy(t => t.Name, new ExplorerComparer()).ToList();
                    if (Trigger.CheckForChanges(plugin.Map.Triggers.ToList(), newTriggers))
                    {
                        bool origDirtyState = plugin.Dirty;
                        Dictionary<object, string> undoList;
                        Dictionary<object, string> redoList;
                        Dictionary<CellTrigger, int> cellTriggerLocations;
                        // Applies all the rename actions, and returns lists of actual changes. Also cleans up objects that are now linked
                        // to incorrect triggers. This action may modify the triggers in the 'newTriggers' list to clean up inconsistencies.
                        plugin.Map.ApplyTriggerChanges(td.RenameActions, out undoList, out redoList, out cellTriggerLocations, newTriggers);
                        // New triggers are cloned, so these are safe to take as backup.
                        List<Trigger> oldTriggers = plugin.Map.Triggers.ToList();
                        // This will notify tool windows to update their trigger lists.
                        plugin.Map.Triggers = newTriggers;
                        plugin.Dirty = true;
                        void undoAction(UndoRedoEventArgs ev)
                        {
                            DialogResult dr = MessageBox.Show(this, "This will undo all trigger editing actions you performed. Are you sure you want to continue?",
                                "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            if (dr == DialogResult.No)
                            {
                                ev.Cancelled = true;
                                return;
                            }
                            foreach (Object obj in undoList.Keys)
                            {
                                if (obj is ITechno techno)
                                {
                                    techno.Trigger = undoList[obj];
                                }
                                else if (obj is TeamType teamType)
                                {
                                    teamType.Trigger = undoList[obj];
                                }
                                else if (obj is CellTrigger celltrigger)
                                {
                                    celltrigger.Trigger = undoList[obj];
                                    // In case it's removed, restore.
                                    if (ev.Map != null)
                                    {
                                        ev.Map.CellTriggers[cellTriggerLocations[celltrigger]] = celltrigger;
                                    }
                                }
                            }
                            if (ev.Plugin != null)
                            {
                                ev.Map.Triggers = oldTriggers;
                                ev.Plugin.Dirty = origDirtyState;
                            }
                            // Repaint map labels
                            ev.MapPanel?.Invalidate();
                        }
                        void redoAction(UndoRedoEventArgs ev)
                        {
                            DialogResult dr = MessageBox.Show(this, "This will redo all trigger editing actions you undid. Are you sure you want to continue?",
                                "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            if (dr == DialogResult.No)
                            {
                                ev.Cancelled = true;
                                return;
                            }
                            foreach (Object obj in redoList.Keys)
                            {
                                if (obj is ITechno techno)
                                {
                                    techno.Trigger = redoList[obj];
                                }
                                else if (obj is TeamType teamType)
                                {
                                    teamType.Trigger = redoList[obj];
                                }
                                else if (obj is CellTrigger celltrigger)
                                {
                                    celltrigger.Trigger = redoList[obj];
                                    if (Trigger.IsEmpty(celltrigger.Trigger) && ev.Map != null)
                                    {
                                        ev.Map.CellTriggers[cellTriggerLocations[celltrigger]] = null;
                                    }
                                }
                            }
                            if (ev.Plugin != null)
                            {
                                ev.Map.Triggers = newTriggers;
                                ev.Plugin.Dirty = true;
                            }
                            // Repaint map labels
                            ev.MapPanel?.Invalidate();
                        }
                        url.Track(undoAction, redoAction);
                        // No longer a full refresh, since celltriggers function is no longer disabled when no triggers are found.
                        mapPanel.Invalidate();
                    }
                }
            }
        }

        private void ToolsOptionsBoundsObstructFillMenuItem_CheckedChanged(Object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tsmi)
            {
                Globals.BoundsObstructFill = tsmi.Checked;
            }
        }

        private void ToolsOptionsSafeDraggingMenuItem_CheckedChanged(Object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tsmi)
            {
                Globals.TileDragProtect = tsmi.Checked;
            }
        }

        private void ToolsOptionsRandomizeDragPlaceMenuItem_CheckedChanged(Object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tsmi)
            {
                Globals.TileDragRandomize = tsmi.Checked;
            }
        }

        private void ToolsOptionsPlacementGridMenuItem_CheckedChanged(Object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tsmi)
            {
                Globals.ShowPlacementGrid = tsmi.Checked;
            }
        }

        private void ToolsOptionsCratesOnTopMenuItem_CheckedChanged(Object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tsmi)
            {
                Globals.CratesOnTop = tsmi.Checked;
            }
            if (plugin != null)
            {
                Map map = plugin.Map;
                CellMetrics cm = map.Metrics;
                mapPanel.Invalidate(map, map.Overlay.Select(ov => cm.GetLocation(ov.Cell)).Where(c => c.HasValue).Cast<Point>());
            }
        }

        private void toolsOptionsOutlineAllCratesMenuItem_Click(Object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tsmi)
            {
                Globals.OutlineAllCrates = tsmi.Checked;
            }
            mapPanel.Invalidate();
        }

        private void ToolsStatsGameObjectsMenuItem_Click(Object sender, EventArgs e)
        {
            if (plugin == null)
            {
                return;
            }
            using (ErrorMessageBox emb = new ErrorMessageBox())
            {
                emb.Title = "Map objects";
                emb.Message = "Map objects overview:";
                emb.Errors = plugin.AssessMapItems();
                emb.StartPosition = FormStartPosition.CenterParent;
                emb.ShowDialog(this);
            }
        }

        private void ToolsStatsPowerMenuItem_Click(Object sender, EventArgs e)
        {
            if (plugin == null)
            {
                return;
            }
            using (ErrorMessageBox emb = new ErrorMessageBox())
            {
                emb.Title = "Power usage";
                emb.Message = "Power balance per House:";
                emb.Errors = plugin.Map.AssessPower(plugin.GetHousesWithProduction());
                emb.StartPosition = FormStartPosition.CenterParent;
                emb.ShowDialog(this);
            }
        }

        private void ToolsStatsStorageMenuItem_Click(Object sender, EventArgs e)
        {
            if (plugin == null)
            {
                return;
            }
            using (ErrorMessageBox emb = new ErrorMessageBox())
            {
                emb.Title = "Silo storage";
                emb.Message = "Available silo storage per House:";
                emb.Errors = plugin.Map.AssessStorage(plugin.GetHousesWithProduction());
                emb.StartPosition = FormStartPosition.CenterParent;
                emb.ShowDialog(this);
            }
        }

        private void ToolsRandomizeTilesMenuItem_Click(Object sender, EventArgs e)
        {
            if (plugin != null)
            {
                String feedback = TemplateTool.RandomizeTiles(plugin, mapPanel, url);
                MessageBox.Show(feedback, GetProgramVersionTitle());
            }
        }

        private void ToolsExportImage_Click(Object sender, EventArgs e)
        {
            if (plugin == null)
            {
                return;
            }
            using (ImageExportDialog imex = new ImageExportDialog(plugin, activeLayers, filename))
            {
                imex.StartPosition = FormStartPosition.CenterParent;
                imex.ShowDialog(this);
            }
        }

        private void Mru_FileSelected(object sender, FileInfo e)
        {
            if (File.Exists(e.FullName))
            {
                OpenFileAsk(e.FullName, false);
            }
            else
            {
                MessageBox.Show(string.Format("Error loading {0}: the file was not found.", e.Name), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                mru.Remove(e);
            }
            
        }

        private void MapPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (plugin == null)
            {
                return;
            }
            var mapPoint = mapPanel.ClientToMap(e.Location);
            var location = new Point((int)Math.Floor((double)mapPoint.X / Globals.MapTileWidth), (int)Math.Floor((double)mapPoint.Y / Globals.MapTileHeight));
            var subPixel = new Point(
                (mapPoint.X * Globals.PixelWidth / Globals.MapTileWidth) % Globals.PixelWidth,
                (mapPoint.Y * Globals.PixelHeight / Globals.MapTileHeight) % Globals.PixelHeight
            );
            cellStatusLabel.Text = plugin.Map.GetCellDescription(location, subPixel);
        }
        #endregion

        #region Additional logic for listeners

        private void NewFileAsk(bool withImage, string imagePath, bool skipPrompt)
        {
            if (skipPrompt)
            {
                NewFile(withImage, imagePath);
            }
            else {
                PromptSaveMap(() => NewFile(withImage, imagePath), false);
            }
        }

        private void NewFile(bool withImage, string imagePath)
        {
            GameType gameType = GameType.None;
            string theater = null;
            bool isTdMegaMap = false;
            using (NewMapDialog nmd = new NewMapDialog(withImage))
            {
                nmd.StartPosition = FormStartPosition.CenterParent;
                if (nmd.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                gameType = nmd.GameType;
                isTdMegaMap = nmd.MegaMap;
                theater = nmd.TheaterName;
            }
            if (withImage && imagePath == null)
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.AutoUpgradeEnabled = false;
                    ofd.RestoreDirectory = true;
                    ofd.Filter = "Image Files (*.png, *.bmp, *.gif)|*.png;*.bmp;*.gif|All Files (*.*)|*.*";
                    if (ofd.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }
                    imagePath = ofd.FileName;
                }
            }
            string[] modPaths = null;
            if (ModPaths != null)
            {
                ModPaths.TryGetValue(gameType, out modPaths);
            }
            Unload();
            String loading = "Loading new map";
            if (withImage)
                loading += " from image";
            loadMultiThreader.ExecuteThreaded(
                () => NewFile(gameType, imagePath, theater, isTdMegaMap, modPaths, this),
                PostLoad, true,
                (e, l) => LoadUnloadUi(e, l, loadMultiThreader),
                loading);
        }

        private void OpenFileAsk(String fileName, bool skipPrompt)
        {
            if (skipPrompt)
            {
                OpenFile(fileName);
            }
            else
            {
                PromptSaveMap(() => OpenFile(fileName), false);
            }
        }
        private void OpenFile(String fileName)
        {
            var fileInfo = new FileInfo(fileName);
            String name = fileInfo.FullName;
            if (!IdentifyMap(name, out FileType fileType, out GameType gameType, out bool isTdMegaMap))
            {
                string extension = Path.GetExtension(name).TrimStart('.');
                // No point in supporting jpeg here; the mapping needs distinct colours without fades.
                if ("PNG".Equals(extension, StringComparison.OrdinalIgnoreCase)
                    || "BMP".Equals(extension, StringComparison.OrdinalIgnoreCase)
                    || "GIF".Equals(extension, StringComparison.OrdinalIgnoreCase)
                    || "TIF".Equals(extension, StringComparison.OrdinalIgnoreCase)
                    || "TIFF".Equals(extension, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        using (Bitmap bm = new Bitmap(name))
                        {
                            // Don't need to do anything except open this to confirm it's supported
                        }
                        NewFileAsk(true, name, true);
                        return;
                    }
                    catch
                    {
                        // Ignore and just fall through.
                    }
                }
                MessageBox.Show(string.Format("Error loading {0}: {1}", fileInfo.Name, "Could not identify map type."), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string[] modPaths = null;
            if (ModPaths != null)
            {
                ModPaths.TryGetValue(gameType, out modPaths);
            }
            loadMultiThreader.ExecuteThreaded(
                () => LoadFile(name, fileType, gameType, isTdMegaMap, modPaths),
                PostLoad, true,
                (e,l) => LoadUnloadUi(e, l, loadMultiThreader),
                "Loading map");
        }

        private void SaveChosenFile(string saveFilename, FileType inputNameType, bool dontResavePreview, Action afterSaveDone)
        {
            // This part assumes validation is already done.
            FileType fileType = FileType.None;
            switch (Path.GetExtension(saveFilename).ToLower())
            {
                case ".ini":
                case ".mpr":
                    fileType = FileType.INI;
                    break;
                case ".bin":
                    fileType = FileType.BIN;
                    break;
            }
            if (fileType == FileType.None)
            {
                if (inputNameType != FileType.None)
                {
                    fileType = inputNameType;
                }
                else
                {
                    // Just default to ini
                    fileType = FileType.INI;
                }
            }
            // Once saved, leave it to be manually handled on steam publish.
            if (string.IsNullOrEmpty(plugin.Map.SteamSection.Title) || plugin.Map.SteamSection.PublishedFileId == 0)
            {
                plugin.Map.SteamSection.Title = plugin.Map.BasicSection.Name;
            }
            ToolType current = ActiveToolType;
            // Different multithreader, so save prompt can start a map load.
            saveMultiThreader.ExecuteThreaded(
                () => SaveFile(plugin, saveFilename, fileType, dontResavePreview),
                (si) => PostSave(si, afterSaveDone), true,
                (bl, str) => EnableDisableUi(bl, str, current, saveMultiThreader),
                "Saving map");
        }

        private Boolean IdentifyMap(String loadFilename, out FileType fileType, out GameType gameType, out bool isTdMegaMap)
        {
            fileType = FileType.None;
            gameType = GameType.None;
            isTdMegaMap = false;
            try
            {
                if (!File.Exists(loadFilename))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            switch (Path.GetExtension(loadFilename).ToLower())
            {
                case ".ini":
                case ".mpr":
                    fileType = FileType.INI;
                    break;
                case ".bin":
                    fileType = FileType.BIN;
                    break;
                case ".pgm":
                    fileType = FileType.PGM;
                    break;
                case ".meg":
                    fileType = FileType.MEG;
                    break;
            }
            INI iniContents = null;
            bool iniWasFetched = false;
            if (fileType == FileType.None)
            {
                long filesize = 0;
                try
                {
                    filesize = new FileInfo(loadFilename).Length;
                    iniWasFetched = true;
                    iniContents = GeneralUtils.GetIniContents(loadFilename, FileType.INI);
                    if (iniContents != null)
                    {
                        fileType = FileType.INI;
                    }
                }
                catch
                {
                    iniContents = null;
                }
                if (iniContents == null)
                {
                    // Check if it's a classic 64x64 map.
                    Size tdMax = TiberianDawn.Constants.MaxSize;
                    if (filesize == tdMax.Width * tdMax.Height * 2)
                    {
                        fileType = FileType.BIN;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            string iniFile = fileType != FileType.BIN ? loadFilename : Path.ChangeExtension(loadFilename, ".ini");
            if (!iniWasFetched)
            {
                iniContents = GeneralUtils.GetIniContents(iniFile, fileType);
            }
            if (iniContents == null || !INITools.CheckForIniInfo(iniContents, "Map") || !INITools.CheckForIniInfo(iniContents, "Basic"))
            {
                return false;
            }
            switch (fileType)
            {
                case FileType.INI:
                    {
                        gameType = RedAlert.GamePluginRA.CheckForRAMap(iniContents) ? GameType.RedAlert : GameType.TiberianDawn;
                        break;
                    }
                case FileType.BIN:
                    {
                        gameType = File.Exists(iniFile) ? GameType.TiberianDawn : GameType.None;
                        break;
                    }
                case FileType.PGM:
                    {
                        try
                        {
                            using (var megafile = new Megafile(loadFilename))
                            {
                                if (megafile.Any(f => Path.GetExtension(f).ToLower() == ".mpr"))
                                {
                                    gameType = GameType.RedAlert;
                                }
                                else
                                {
                                    gameType = GameType.TiberianDawn;
                                }
                            }
                        }
                        catch (FileNotFoundException)
                        {
                            return false;
                        }
                        break;
                    }
            }
            if (gameType == GameType.TiberianDawn)
            {
                isTdMegaMap = TiberianDawn.GamePluginTD.CheckForMegamap(iniContents);
                if (SoleSurvivor.GamePluginSS.CheckForSSmap(iniContents))
                {
                    gameType = GameType.SoleSurvivor;
                }
            }
            return gameType != GameType.None;
        }

        /// <summary>
        /// WARNING: this function is meant for map load, meaning it unloads the current plugin in addition to disabling all controls!
        /// </summary>
        /// <param name="enableUI"></param>
        /// <param name="label"></param>
        private void LoadUnloadUi(bool enableUI, string label, SimpleMultiThreading currentMultiThreader)
        {
            fileNewMenuItem.Enabled = enableUI;
            fileNewFromImageMenuItem.Enabled = enableUI;
            fileOpenMenuItem.Enabled = enableUI;
            fileRecentFilesMenuItem.Enabled = enableUI;
            viewLayersToolStripMenuItem.Enabled = enableUI;
            viewIndicatorsToolStripMenuItem.Enabled = enableUI;
            if (!enableUI)
            {
                Unload();
                currentMultiThreader.CreateBusyLabel(this, label);
            }
        }

        /// <summary>
        /// The 'lighter' enable/disable UI function, for map saving.
        /// </summary>
        /// <param name="enableUI"></param>
        /// <param name="label"></param>
        private void EnableDisableUi(bool enableUI, string label, ToolType storedToolType, SimpleMultiThreading currentMultiThreader)
        {
            fileNewMenuItem.Enabled = enableUI;
            fileNewFromImageMenuItem.Enabled = enableUI;
            fileOpenMenuItem.Enabled = enableUI;
            fileRecentFilesMenuItem.Enabled = enableUI;
            viewLayersToolStripMenuItem.Enabled = enableUI;
            viewIndicatorsToolStripMenuItem.Enabled = enableUI;
            EnableDisableMenuItems(enableUI);
            mapPanel.Enabled = enableUI;
            if (enableUI)
            {
                RefreshUI(storedToolType);
            }
            else
            {
                ClearActiveTool();
                foreach (var toolStripButton in viewToolStripButtons)
                {
                    toolStripButton.Enabled = false;
                }
                currentMultiThreader.CreateBusyLabel(this, label);
            }
        }

        private static IGamePlugin LoadNewPlugin(GameType gameType, bool isTdMegaMap, string[] modPaths)
        {
            return LoadNewPlugin(gameType, isTdMegaMap, modPaths, false);
        }

        private static IGamePlugin LoadNewPlugin(GameType gameType, bool isTdMegaMap, string[] modPaths, bool noImage)
        {
            Globals.TheTextureManager.ExpandModPaths = modPaths;
            Globals.TheTextureManager.Reset();
            Globals.TheTilesetManager.ExpandModPaths = modPaths;
            Globals.TheTilesetManager.Reset();
            Globals.TheTeamColorManager.ExpandModPaths = modPaths;
            IGamePlugin plugin = null;
            if (gameType == GameType.TiberianDawn)
            {
                Globals.TheTeamColorManager.Reset();
                AddTeamColorNone(Globals.TheTeamColorManager);
                // TODO split classic and remaster team color load.
                Globals.TheTeamColorManager.Load(@"DATA\XML\CNCTDTEAMCOLORS.XML");
                plugin = new TiberianDawn.GamePluginTD(!noImage, isTdMegaMap);
            }
            else if (gameType == GameType.RedAlert)
            {
                Globals.TheTeamColorManager.Reset();
                Globals.TheTeamColorManager.Load(@"DATA\XML\CNCRATEAMCOLORS.XML");
                plugin = new RedAlert.GamePluginRA(!noImage);
            }
            else if (gameType == GameType.SoleSurvivor)
            {
                Globals.TheTeamColorManager.Reset();
                AddTeamColorNone(Globals.TheTeamColorManager);
                Globals.TheTeamColorManager.Load(@"DATA\XML\CNCTDTEAMCOLORS.XML");
                plugin = new SoleSurvivor.GamePluginSS(!noImage, isTdMegaMap);
            }
            return plugin;
        }

        private static void AddTeamColorNone(TeamColorManager teamColorManager)
        {
            // Add default black for unowned.
            var teamColorNone = new TeamColor(teamColorManager);
            teamColorNone.Load("NONE", "BASE_TEAM",
                Color.FromArgb(66, 255, 0), Color.FromArgb(0, 255, 56), 0,
                new Vector3(0.30f, -1.00f, 0.00f), new Vector3(0f, 1f, 1f), new Vector2(0.0f, 0.1f),
                new Vector3(0, 1, 1), new Vector2(0, 1), Color.FromArgb(61, 61, 59));
            teamColorManager.AddTeamColor(teamColorNone);
        }

        /// <summary>
        /// The separate-threaded part for making a new map.
        /// </summary>
        /// <param name="gameType"></param>
        /// <param name="theater"></param>
        /// <param name="isTdMegaMap"></param>
        /// <param name="modPaths"></param>
        /// <returns></returns>
        private static MapLoadInfo NewFile(GameType gameType, String imagePath, string theater, bool isTdMegaMap, string[] modPaths, MainForm showTarget)
        {
            int imageWidth = 0;
            int imageHeight = 0;
            Byte[] imageData = null;
            if (imagePath != null)
            {
                try
                {
                    using (Bitmap bm = new Bitmap(imagePath))
                    {
                        bm.SetResolution(96, 96);
                        imageWidth = bm.Width;
                        imageHeight = bm.Height;
                        imageData = ImageUtils.GetImageData(bm, PixelFormat.Format32bppArgb);
                    }
                }
                catch (Exception ex)
                {
                    List<string> errorMessage = new List<string>();
                    errorMessage.Add("Error loading image: " + ex.Message);
#if DEBUG
                    errorMessage.Add(ex.StackTrace);
#endif
                    return new MapLoadInfo(null, FileType.None, null, errorMessage.ToArray());
                }
            }
            try
            {
                IGamePlugin plugin = LoadNewPlugin(gameType, isTdMegaMap, modPaths);
                // This initialises the theater
                plugin.New(theater);
                if (SteamworksUGC.IsInit)
                {
                    plugin.Map.BasicSection.Author = SteamFriends.GetPersonaName();
                }
                if (imageData != null)
                {
                    Dictionary<int, string> types = (Dictionary<int, string>) showTarget
                        .Invoke((FunctionInvoker)(() => ShowNewFromImageDialog(plugin, imageWidth, imageHeight, imageData, showTarget)));
                    if (types == null)
                    {
                        return null;
                    }
                    plugin.Map.SetMapTemplatesRaw(imageData, imageWidth, imageHeight, types, null);
                }
                return new MapLoadInfo(null, FileType.None, plugin, null);
            }
            catch (Exception ex)
            {
                List<string> errorMessage = new List<string>();
                if (ex is ArgumentException argex)
                {
                    errorMessage.Add(GeneralUtils.RecoverArgExceptionMessage(argex, false));
                }
                else
                {
                    errorMessage.Add(ex.Message);
                }
#if DEBUG
                errorMessage.Add(ex.StackTrace);
#endif
                return new MapLoadInfo(null, FileType.None, null, errorMessage.ToArray());
            }
        }

        private static Dictionary<int, string> ShowNewFromImageDialog(IGamePlugin plugin, int imageWidth, int imageHeight, byte[] imageData, MainForm showTarget)
        {
            Color[] mostCommon = ImageUtils.FindMostCommonColors(2, imageData, imageWidth, imageHeight, imageWidth * 4);
            Dictionary<int, string> mappings = new Dictionary<int, string>();
            // This is ignored in the mappings, but eh. Everything unmapped defaults to clear since that's what the map is initialised with.
            if (mostCommon.Length > 0)
                mappings.Add(mostCommon[0].ToArgb(), "CLEAR1");
            if (mostCommon.Length > 1)
                mappings.Add(mostCommon[1].ToArgb(), plugin.Map.Theater.Name == RedAlert.TheaterTypes.Interior.Name ? "FLOR0001:0" : "W1:0");
            using (NewFromImageDialog nfi = new NewFromImageDialog(plugin, imageWidth, imageHeight, imageData, mappings))
            {
                nfi.StartPosition = FormStartPosition.CenterParent;
                if (nfi.ShowDialog(showTarget) == DialogResult.Cancel)
                    return null;
                return nfi.Mappings;
            }
        }

        /// <summary>
        /// The separate-threaded part for loading a map.
        /// </summary>
        /// <param name="loadFilename"></param>
        /// <param name="fileType"></param>
        /// <param name="gameType"></param>
        /// <param name="isTdMegaMap"></param>
        /// <param name="modPaths"></param>
        /// <returns></returns>
        private static MapLoadInfo LoadFile(string loadFilename, FileType fileType, GameType gameType, bool isTdMegaMap, string[] modPaths)
        {
            try
            {
                IGamePlugin plugin = LoadNewPlugin(gameType, isTdMegaMap, modPaths);
                string[] errors = plugin.Load(loadFilename, fileType).ToArray();
                return new MapLoadInfo(loadFilename, fileType, plugin, errors);
            }
            catch (Exception ex)
            {
                List<string> errorMessage = new List<string>();
                if (ex is ArgumentException argex)
                {
                    errorMessage.Add(GeneralUtils.RecoverArgExceptionMessage(argex, false));
                }
                else
                {
                    errorMessage.Add(ex.Message);
                }
#if DEBUG
                errorMessage.Add(ex.StackTrace);
#endif
                return new MapLoadInfo(loadFilename, fileType, null, errorMessage.ToArray());
            }
        }

        private static (string FileName, bool SavedOk, string error) SaveFile(IGamePlugin plugin, string saveFilename, FileType fileType, bool dontResavePreview)
        {
            try
            {
                plugin.Save(saveFilename, fileType, null, dontResavePreview);
                return (saveFilename, true, null);
            }
            catch (Exception ex)
            {
                string errorMessage = "Error loading map: " + ex.Message;
#if DEBUG
                errorMessage += "\n\n" + ex.StackTrace;
#endif
                return (saveFilename, false, errorMessage);
            }
        }

        private void PostLoad(MapLoadInfo loadInfo)
        {
            if (loadInfo == null)
            {
                // Absolute abort
                SimpleMultiThreading.RemoveBusyLabel(this);
                return;
            }
            string[] errors = loadInfo.Errors ?? new string[0];
            // Plugin set to null indicates a fatal processing error where no map was loaded at all.
            if (loadInfo.Plugin == null)
            {
                if (loadInfo.FileName != null)
                {
                    var fileInfo = new FileInfo(loadInfo.FileName);
                    mru.Remove(fileInfo);
                }
                // In case of actual error, remove label.
                SimpleMultiThreading.RemoveBusyLabel(this);
                MessageBox.Show(string.Format("Error loading {0}: {1}", loadInfo.FileName ?? "new map", String.Join("\n", errors)), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                this.plugin = loadInfo.Plugin;
                plugin.FeedBackHandler = this;
                LoadIcons(plugin);
                if (errors.Length > 0)
                {
                    using (ErrorMessageBox emb = new ErrorMessageBox())
                    {
                        emb.Title = "Error Report - " + Path.GetFileName(loadInfo.FileName);
                        emb.Errors = errors;
                        emb.StartPosition = FormStartPosition.CenterParent;
                        emb.ShowDialog(this);
                    }
                }
#if !DEVELOPER
                // Don't allow re-save as PGM; act as if this is a new map.
                if (loadInfo.FileType == FileType.PGM || loadInfo.FileType == FileType.MEG)
                {
                    bool isRA = loadInfo.Plugin.GameType == GameType.RedAlert;
                    loadInfo.FileType = FileType.INI;
                    loadInfo.FileName = null;
                }
#endif
                mapPanel.MapImage = plugin.MapImage;
                filename = loadInfo.FileName;
                loadedFileType = loadInfo.FileType;
                lock (jumpToBounds_lock)
                {
                    this.jumpToBounds = Globals.ZoomToBoundsOnLoad;
                }
                url.Clear();
                CleanupTools();
                RefreshUI();
                //RefreshActiveTool(); // done by UI refresh
                SetTitle();
                if (loadInfo.FileName != null)
                {
                    var fileInfo = new FileInfo(loadInfo.FileName);
                    mru.Add(fileInfo);
                }
            }
        }

        private void PostSave((string FileName, bool SavedOk, string Error) saveInfo, Action afterSaveDone)
        {
            var fileInfo = new FileInfo(saveInfo.FileName);
            if (saveInfo.SavedOk)
            {
                if (fileInfo.Exists && fileInfo.Length > Globals.MaxMapSize)
                {
                    MessageBox.Show(string.Format("Map file exceeds the maximum size of {0} bytes.", Globals.MaxMapSize), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                plugin.Dirty = false;
                filename = saveInfo.FileName;
                SetTitle();
                mru.Add(fileInfo);
                afterSaveDone?.Invoke();
            }
            else
            {
                MessageBox.Show(string.Format("Error saving {0}: {1}", saveInfo.FileName, saveInfo.Error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error));
                mru.Remove(fileInfo);
            }
        }

        /// <summary>
        /// This clears the UI and plugin in a safe way, ending up with a blank slate.
        /// </summary>
        private void Unload()
        {
            try
            {
                url.Clear();
                // Disable all tools
                ActiveToolType = ToolType.None; // Always re-defaults to map anyway, so nicer if nothing is selected during load.
                this.ActiveControl = null;
                CleanupTools();
                // Unlink plugin
                IGamePlugin pl = plugin;
                plugin = null;
                // Refresh UI to plugin-less state
                RefreshUI();
                // Reset map panel. Looks odd if the zoom/position is preserved, so zoom out first.
                mapPanel.Zoom = 1.0;
                mapPanel.MapImage = null;
                mapPanel.Invalidate();
                // Dispose plugin
                if (pl != null)
                {
                    pl.Dispose();
                }
                // Unload graphics
                Globals.TheTilesetManager.Reset();
                Globals.TheTextureManager.Reset();
                // Clean up loaded file status
                filename = null;
                loadedFileType = FileType.None;
                SetTitle();
            }
            catch
            {
                // Ignore.
            }
        }

        private void RefreshUI()
        {
            RefreshUI(this.activeToolType);
        }

        private void RefreshUI(ToolType activeToolType)
        {
            // Menu items
            EnableDisableMenuItems(true);
            // Tools
            availableToolTypes = ToolType.None;
            if (plugin != null)
            {
                TheaterType th = plugin.Map.Theater;
                availableToolTypes |= ToolType.Waypoint;
                availableToolTypes |= plugin.Map.TemplateTypes.Any(t => t.Theaters == null || t.Theaters.Contains(th)) ? ToolType.Map : ToolType.None;
                availableToolTypes |= plugin.Map.SmudgeTypes.Any(t => !Globals.FilterTheaterObjects || t.Theaters == null || t.Theaters.Contains(th)) ? ToolType.Smudge : ToolType.None;
                availableToolTypes |= plugin.Map.OverlayTypes.Any(t => t.IsOverlay && (!Globals.FilterTheaterObjects || t.Theaters == null || t.Theaters.Contains(th))) ? ToolType.Overlay : ToolType.None;
                availableToolTypes |= plugin.Map.TerrainTypes.Any(t => !Globals.FilterTheaterObjects || t.Theaters == null || t.Theaters.Contains(th)) ? ToolType.Terrain : ToolType.None;
                availableToolTypes |= plugin.Map.InfantryTypes.Any() ? ToolType.Infantry : ToolType.None;
                availableToolTypes |= plugin.Map.UnitTypes.Any() ? ToolType.Unit : ToolType.None;
                availableToolTypes |= plugin.Map.BuildingTypes.Any(t => !Globals.FilterTheaterObjects || t.Theaters == null || t.Theaters.Contains(th)) ? ToolType.Building : ToolType.None;
                availableToolTypes |= plugin.Map.OverlayTypes.Any(t => t.IsResource && (!Globals.FilterTheaterObjects || t.Theaters == null || t.Theaters.Contains(th))) ? ToolType.Resources : ToolType.None;
                availableToolTypes |= plugin.Map.OverlayTypes.Any(t => t.IsWall && (!Globals.FilterTheaterObjects || t.Theaters == null || t.Theaters.Contains(th))) ? ToolType.Wall : ToolType.None;
                // Always allow celltrigger tool, even if triggers list is empty; it contains a tooltip saying which triggers are eligible.
                availableToolTypes |= ToolType.CellTrigger;
                // TODO - "Select" tool will always be enabled
                //availableToolTypes |= ToolType.Select;
            }
            foreach (var toolStripButton in viewToolStripButtons)
            {
                toolStripButton.Enabled = (availableToolTypes & toolStripButton.ToolType) != ToolType.None;
            }
            ActiveToolType = activeToolType;
        }

        private void EnableDisableMenuItems(bool enable)
        {
            bool hasPlugin = plugin != null;
            fileSaveMenuItem.Enabled = enable && hasPlugin;
            fileSaveAsMenuItem.Enabled = enable && hasPlugin;
            filePublishMenuItem.Enabled = enable && hasPlugin;
#if DEVELOPER
            fileExportMenuItem.Enabled = enable && hasPlugin;
#endif
            editUndoMenuItem.Enabled = enable && hasPlugin && url.CanUndo;
            editRedoMenuItem.Enabled = enable && hasPlugin && url.CanRedo;
            editClearUndoRedoMenuItem.Enabled = enable && hasPlugin && url.CanUndo || url.CanRedo;
            settingsMapSettingsMenuItem.Enabled = enable && hasPlugin;
            settingsTeamTypesMenuItem.Enabled = enable && hasPlugin;
            settingsTriggersMenuItem.Enabled = enable && hasPlugin;
            toolsStatsGameObjectsMenuItem.Enabled = enable && hasPlugin;
            toolsStatsPowerMenuItem.Enabled = enable && hasPlugin;
            toolsStatsStorageMenuItem.Enabled = enable && hasPlugin;
            toolsRandomizeTilesMenuItem.Enabled = enable && hasPlugin;
            toolsExportImageMenuItem.Enabled = enable && hasPlugin;
#if DEVELOPER
            developerGoToINIMenuItem.Enabled = enable && hasPlugin;
            developerDebugToolStripMenuItem.Enabled = enable && hasPlugin;
            developerGenerateMapPreviewDirectoryMenuItem.Enabled = enable && hasPlugin;
#endif
            viewLayersToolStripMenuItem.Enabled = enable;
            viewIndicatorsToolStripMenuItem.Enabled = enable;

            // Special rules per game. These should be kept identical to those in ImageExportDialog.SetLayers
            viewIndicatorsBuildingFakeLabelsMenuItem.Visible = !hasPlugin || plugin.GameType == GameType.RedAlert;
            viewExtraIndicatorsEffectAreaRadiusMenuItem.Visible = !hasPlugin || plugin.GameType == GameType.RedAlert;
            viewLayersBuildingsMenuItem.Visible = !hasPlugin || plugin.GameType != GameType.SoleSurvivor || !Globals.NoOwnedObjectsInSole;
            viewLayersUnitsMenuItem.Visible = !hasPlugin || plugin.GameType != GameType.SoleSurvivor || !Globals.NoOwnedObjectsInSole;
            viewLayersInfantryMenuItem.Visible = !hasPlugin || plugin.GameType != GameType.SoleSurvivor || !Globals.NoOwnedObjectsInSole;
            viewIndicatorsBuildingRebuildLabelsMenuItem.Visible = !hasPlugin || plugin.GameType != GameType.SoleSurvivor;
            viewIndicatorsFootballAreaMenuItem.Visible = !hasPlugin || plugin.GameType == GameType.SoleSurvivor;
            viewIndicatorsCrateOutlinesMenuItem.Visible = !hasPlugin || plugin.GameType != GameType.SoleSurvivor;
        }

        private void CleanupTools()
        {
            // Tools
            ClearActiveTool();
            foreach (var kvp in toolForms)
            {
                kvp.Value.Dispose();
            }
            toolForms.Clear();
        }

        private void ClearActiveTool()
        {
            activeTool?.Deactivate();
            activeTool = null;
            if (activeToolForm != null)
            {
                activeToolForm.ResizeEnd -= ActiveToolForm_ResizeEnd;
                activeToolForm.Hide();
                activeToolForm = null;
            }
            toolStatusLabel.Text = string.Empty;
        }

        private void RefreshActiveTool()
        {
            if (plugin == null)
            {
                return;
            }
            if (activeTool == null)
            {
                activeLayers = MapLayerFlag.None;
            }
            ClearActiveTool();
            bool found = toolForms.TryGetValue(ActiveToolType, out IToolDialog toolDialog);
            if (!found || (toolDialog is Form toolFrm && toolFrm.IsDisposed))
            {
                switch (ActiveToolType)
                {
                    case ToolType.Map:
                        {
                            toolDialog = new TemplateToolDialog(this);
                        }
                        break;
                    case ToolType.Smudge:
                        {
                            toolDialog = new SmudgeToolDialog(this, plugin);
                        }
                        break;
                    case ToolType.Overlay:
                        {
                            toolDialog = new OverlayToolDialog(this);
                        }
                        break;
                    case ToolType.Resources:
                        {
                            toolDialog = new ResourcesToolDialog(this);
                        }
                        break;
                    case ToolType.Terrain:
                        {
                            toolDialog = new TerrainToolDialog(this, plugin);
                        }
                        break;
                    case ToolType.Infantry:
                        {
                            toolDialog = new InfantryToolDialog(this, plugin);
                        }
                        break;
                    case ToolType.Unit:
                        {
                            toolDialog = new UnitToolDialog(this, plugin);
                        }
                        break;
                    case ToolType.Building:
                        {
                            toolDialog = new BuildingToolDialog(this, plugin);
                        }
                        break;
                    case ToolType.Wall:
                        {
                            toolDialog = new WallsToolDialog(this);
                        }
                        break;
                    case ToolType.Waypoint:
                        {
                            toolDialog = new WaypointsToolDialog(this);
                        }
                        break;
                    case ToolType.CellTrigger:
                        {
                            toolDialog = new CellTriggersToolDialog(this);
                        }
                        break;
                    case ToolType.Select:
                        {
                            // TODO: select/copy/paste function
                            toolDialog = null; // new SelectToolDialog(this);
                        }
                        break;
                }
                if (toolDialog != null)
                {
                    toolForms[ActiveToolType] = toolDialog;
                }
            }
            MapLayerFlag active = ActiveLayers;
            // Save some processing by just always removing this one.
            if (plugin.GameType == GameType.TiberianDawn || plugin.GameType == GameType.SoleSurvivor)
            {
                active &= ~MapLayerFlag.BuildingFakes;
            }
            if (toolDialog != null)
            {
                activeToolForm = (Form)toolDialog;
                // Creates the actual Tool class
                toolDialog.Initialize(mapPanel, active, toolStatusLabel, mouseToolTip, plugin, url);
                activeTool = toolDialog.GetTool();
                activeToolForm.ResizeEnd -= ActiveToolForm_ResizeEnd;
                activeToolForm.Shown -= this.ActiveToolForm_Shown;
                activeToolForm.Shown += this.ActiveToolForm_Shown;
                activeToolForm.Show(this);
                activeTool.Activate();
                activeToolForm.ResizeEnd += ActiveToolForm_ResizeEnd;
            }
            if (plugin.IsMegaMap)
            {
                mapPanel.MaxZoom = 16;
                mapPanel.ZoomStep = 0.2;
            }
            else
            {
                mapPanel.MaxZoom = 8;
                mapPanel.ZoomStep = 0.15;
            }
            // Refresh toolstrip button checked states
            foreach (var toolStripButton in viewToolStripButtons)
            {
                toolStripButton.Checked = ActiveToolType == toolStripButton.ToolType;
            }

            // this somehow fixes the fact that the keyUp and keyDown events of the navigation widget don't come through.
            mainToolStrip.Focus();
            mapPanel.Focus();
            // refresh for tool
            UpdateVisibleLayers();
            // refresh to paint the actual tool's post-render layers
            mapPanel.Invalidate();
        }

        private void ClampActiveToolForm()
        {
            ClampForm(activeToolForm);
        }

        public static void ClampForm(Form toolform)
        {
            if (toolform == null)
            {
                return;
            }
            Size maxAllowed = Globals.MinimumClampSize;
            Rectangle toolBounds = toolform.DesktopBounds;
            if (maxAllowed == Size.Empty)
            {
                maxAllowed = toolform.Size;
            }
            else
            {
                maxAllowed = new Size(Math.Min(maxAllowed.Width, toolBounds.Width), Math.Min(maxAllowed.Height, toolBounds.Height));
            }
            Rectangle workingArea = Screen.FromControl(toolform).WorkingArea;
            if (toolBounds.Left + maxAllowed.Width > workingArea.Right)
            {
                toolBounds.X = workingArea.Right - maxAllowed.Width;
            }
            if (toolBounds.X + toolBounds.Width - maxAllowed.Width < workingArea.Left)
            {
                toolBounds.X = workingArea.Left - toolBounds.Width + maxAllowed.Width;
            }
            if (toolBounds.Top + maxAllowed.Height > workingArea.Bottom)
            {
                toolBounds.Y = workingArea.Bottom - maxAllowed.Height;
            }
            // Leave this; don't allow it to disappear under the top
            if (toolBounds.Y < workingArea.Top)
            {
                toolBounds.Y = workingArea.Top;
            }
            toolform.DesktopBounds = toolBounds;
        }

        private void ActiveToolForm_ResizeEnd(object sender, EventArgs e)
        {
            ClampActiveToolForm();
        }

        private void ActiveToolForm_Shown(object sender, EventArgs e)
        {
            Form tool = sender as Form;
            if (tool != null)
            {
                ClampForm(tool);
            }
        }

        private void UpdateVisibleLayers()
        {
            MapLayerFlag layers = MapLayerFlag.All;
            if (!viewIndicatorsMapBoundariesMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.Boundaries;
            }
            if (!viewExtraIndicatorsMapSymmetryMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.MapSymmetry;
            }
            if (!viewExtraIndicatorsMapGridMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.MapGrid;
            }
            if (!viewLayersBuildingsMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.Buildings;
            }
            if (!viewLayersUnitsMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.Units;
            }
            if (!viewLayersInfantryMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.Infantry;
            }
            if (!viewLayersTerrainMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.Terrain;
            }
            if (!viewLayersOverlayMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.OverlayAll;
            }
            if (!viewLayersSmudgeMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.Smudge;
            }
            if (!viewLayersWaypointsMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.Waypoints;
            }
            if (!viewIndicatorsWaypointsMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.WaypointsIndic;
            }
            if (!viewIndicatorsCellTriggersMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.CellTriggers;
            }
            if (!viewIndicatorsObjectTriggersMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.TechnoTriggers;
            }
            if (!viewIndicatorsBuildingFakeLabelsMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.BuildingFakes;
            }
            if (!viewIndicatorsBuildingRebuildLabelsMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.BuildingRebuild;
            }
            if (!viewIndicatorsFootballAreaMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.FootballArea;
            }
            if (!viewExtraIndicatorsWaypointRevealRadiusMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.WaypointRadius;
            }
            if (!viewExtraIndicatorsEffectAreaRadiusMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.EffectRadius;
            }
            if (!viewIndicatorsCrateOutlinesMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.CrateOutlines;
            }
            ActiveLayers = layers;
        }

#endregion

        private void mainToolStripButton_Click(object sender, EventArgs e)
        {
            if (plugin == null)
            {
                return;
            }
            ActiveToolType = ((ViewToolStripButton)sender).ToolType;
        }

        private void MapPanel_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                String[] files = (String[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1)
                    e.Effect = DragDropEffects.Copy;
            }
        }

        private void MapPanel_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            String[] files = (String[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length != 1)
                return;
            OpenFileAsk(files[0], false);
        }

        private void ViewMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            UpdateVisibleLayers();
        }

        private void ViewLayersEnableAllMenuItem_Click(object sender, EventArgs e)
        {
            EnableDisableLayersCategory(true, true);
        }

        private void ViewLayersDisableAllMenuItem_Click(object sender, EventArgs e)
        {
            EnableDisableLayersCategory(true, false);
        }

        private void ViewIndicatorsEnableAllToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            EnableDisableLayersCategory(false, true);
        }

        private void ViewIndicatorsDisableAllToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            EnableDisableLayersCategory(false, false);
        }

        private void EnableDisableLayersCategory(bool baseLayers, bool enabled)
        {
            ITool activeTool = this.activeTool;
            try
            {
                // Suppress updates.
                this.activeTool = null;
                SwitchLayers(baseLayers, enabled);
            }
            finally
            {
                // Re-enable tool, force refresh.
                MapLayerFlag layerBackup = this.activeLayers;
                // Clear without refresh
                this.activeLayers = MapLayerFlag.None;
                // Restore tool
                this.activeTool = activeTool;
                // Set with refresh
                ActiveLayers = layerBackup;
            }
        }

        private void SwitchLayers(bool baseLayers, bool enabled)
        {
            if (baseLayers)
            {
                viewLayersBuildingsMenuItem.Checked = enabled;
                viewLayersInfantryMenuItem.Checked = enabled;
                viewLayersUnitsMenuItem.Checked = enabled;
                viewLayersTerrainMenuItem.Checked = enabled;
                viewLayersOverlayMenuItem.Checked = enabled;
                viewLayersSmudgeMenuItem.Checked = enabled;
                viewLayersWaypointsMenuItem.Checked = enabled;
            }
            else
            {
                viewIndicatorsMapBoundariesMenuItem.Checked = enabled;
                viewIndicatorsWaypointsMenuItem.Checked = enabled;
                viewIndicatorsFootballAreaMenuItem.Checked = enabled;
                viewIndicatorsCellTriggersMenuItem.Checked = enabled;
                viewIndicatorsObjectTriggersMenuItem.Checked = enabled;
                viewIndicatorsBuildingRebuildLabelsMenuItem.Checked = enabled;
                viewIndicatorsBuildingFakeLabelsMenuItem.Checked = enabled;
                viewIndicatorsCrateOutlinesMenuItem.Checked = enabled;
            }
        }

        private void DeveloperGoToINIMenuItem_Click(object sender, EventArgs e)
        {
#if DEVELOPER
            if ((plugin == null) || string.IsNullOrEmpty(filename))
            {
                return;
            }
            var path = Path.ChangeExtension(filename, ".mpr");
            if (!File.Exists(path))
            {
                path = Path.ChangeExtension(filename, ".ini");
            }
            try
            {
                System.Diagnostics.Process.Start(path);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                System.Diagnostics.Process.Start("notepad.exe", path);
            }
            catch (Exception) { }
#endif
        }

        private void DeveloperGenerateMapPreviewDirectoryMenuItem_Click(object sender, EventArgs e)
        {
#if DEVELOPER
            FolderBrowserDialog fbd = new FolderBrowserDialog
            {
                ShowNewFolderButton = false
            };
            if (fbd.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            var extensions = new string[] { ".ini", ".mpr" };
            foreach (var file in Directory.EnumerateFiles(fbd.SelectedPath).Where(file => extensions.Contains(Path.GetExtension(file).ToLower())))
            {
                bool valid = GetPluginOptions(file, out FileType fileType, out GameType gameType, out bool isTdMegaMap);
                IGamePlugin plugin = LoadNewPlugin(gameType, isTdMegaMap, null, true);
                plugin.Load(file, fileType);
                plugin.Map.GenerateMapPreview(gameType, true).Save(Path.ChangeExtension(file, ".tga"));
                plugin.Dispose();
            }
#endif
        }

        private void DeveloperDebugShowOverlapCellsMenuItem_CheckedChanged(object sender, EventArgs e)
        {
#if DEVELOPER
            Globals.Developer.ShowOverlapCells = developerDebugShowOverlapCellsMenuItem.Checked;
#endif
        }

        private void FilePublishMenuItem_Click(object sender, EventArgs e)
        {
            if (plugin == null)
            {
                return;
            }
            if (plugin.GameType == GameType.SoleSurvivor)
            {
                MessageBox.Show("Sole Survivor maps cannot be published to Steam; they are not usable by the C&C Remastered Collection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (plugin.GameType == GameType.TiberianDawn && plugin.IsMegaMap)
            {
                //if (DialogResult.Yes != MessageBox.Show("Megamaps are not supported by the C&C Remastered Collection without modding! Are you sure you want to publish a map that will be incompatible with the standard unmodded game?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
                //{
                //    return;
                //}
                MessageBox.Show("Tiberian Dawn megamaps cannot be published to Steam; they are not usable by the C&C Remastered Collection without modding, and may cause issues on the official servers.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!SteamworksUGC.IsInit)
            {
                MessageBox.Show("Steam interface is not initialized. To enable Workshop publishing, log into Steam and restart the editor.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            PromptSaveMap(ShowPublishDialog, false);
        }

        private void ShowPublishDialog()
        {
            if (plugin.Dirty)
            {
                MessageBox.Show("Map must be saved before publishing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (new FileInfo(filename).Length > Globals.MaxMapSize)
            {
                return;
            }
            // Check if we need to save.
            ulong oldId = plugin.Map.SteamSection.PublishedFileId;
            string oldName = plugin.Map.SteamSection.Title;
            string oldDescription = plugin.Map.SteamSection.Description;
            string oldPreview = plugin.Map.SteamSection.PreviewFile;
            int oldVisibility = (int)plugin.Map.SteamSection.Visibility;
            // Open publish dialog
            bool wasPublished;
            using (var sd = new SteamDialog(plugin))
            {
                sd.ShowDialog();
                wasPublished = sd.MapWasPublished;
            }
            // Only re-save is it was published and something actually changed.
            if (wasPublished && (oldId != plugin.Map.SteamSection.PublishedFileId
                || oldName != plugin.Map.SteamSection.Title
                || oldDescription != plugin.Map.SteamSection.Description
                || oldPreview != plugin.Map.SteamSection.PreviewFile
                || oldVisibility != (int)plugin.Map.SteamSection.Visibility))
            {
                // This takes care of saving the Steam info into the map.
                // This specific overload only saves the map, without resaving the preview.
                SaveAction(true, null);
            }
        }

        private void MainToolStrip_MouseMove(object sender, MouseEventArgs e)
        {
            if (Form.ActiveForm != null)
            {
                mainToolStrip.Focus();
            }
        }

        private void MainForm_Shown(object sender, System.EventArgs e)
        {
            CleanupTools();
            RefreshUI();
            UpdateUndoRedo();
            if (filename != null)
                this.OpenFileAsk(filename, true);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Form.Close() after the save will re-trigger this FormClosing event handler, but the
            // plugin will not be dirty, so it will just succeed and go on to the CleanupOnClose() call.
            // Also note, if the save fails for some reason, Form.Close() is never called.
            Boolean abort = !PromptSaveMap(this.Close, true);
            e.Cancel = abort;
            if (!abort)
            {
                CleanupOnClose();
            }
        }

        private void CleanupOnClose()
        {
            // If loading, abort. Wait for confirmation of abort before continuing the unloading.
            if (loadMultiThreader != null)
            {
                loadMultiThreader.AbortThreadedOperation(5000);
            }
            // Restore default icons, then dispose custom ones.
            // Form dispose should take care of the default ones.
            LoadNewIcon(mapToolStripButton, null, null, 0);
            LoadNewIcon(smudgeToolStripButton, null, null, 1);
            LoadNewIcon(overlayToolStripButton, null, null, 2);
            LoadNewIcon(terrainToolStripButton, null, null, 3);
            LoadNewIcon(infantryToolStripButton, null, null, 4);
            LoadNewIcon(unitToolStripButton, null, null, 5);
            LoadNewIcon(buildingToolStripButton, null, null, 6);
            LoadNewIcon(resourcesToolStripButton, null, null, 7);
            LoadNewIcon(wallsToolStripButton, null, null, 8);
            LoadNewIcon(waypointsToolStripButton, null, null, 9);
            LoadNewIcon(cellTriggersToolStripButton, null, null, 10);
            List<Bitmap> toDispose = new List<Bitmap>();
            foreach (int key in theaterIcons.Keys)
            {
                toDispose.Add(theaterIcons[key]);
            }
            theaterIcons.Clear();
            foreach (Bitmap bm in toDispose)
            {
                try
                {
                    bm.Dispose();
                }
                catch
                {
                    // Ignore
                }
            }
        }

        /// <summary>
        /// Returns false if the action in progress should be considered aborted.
        /// </summary>
        /// <param name="nextAction">Action to perform after the check. If this is after the save, the function will still return false.</param>ormcl
        /// <param name="onlyAfterSave">Only perform nextAction after a save operation, not when the user pressed "no".</param>
        /// <returns>false if the action was aborted.</returns>
        private bool PromptSaveMap(Action nextAction, bool onlyAfterSave)
        {
#if !DEVELOPER
            if (loadedFileType == FileType.PGM || loadedFileType == FileType.MEG)
            {
                return true;
            }
#endif
            if (plugin?.Dirty ?? false)
            {
                var message = string.IsNullOrEmpty(filename) ? "Save new map?" : string.Format("Save map '{0}'?", filename);
                var result = MessageBox.Show(message, "Save", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                switch (result)
                {
                    case DialogResult.Yes:
                        {
                            String errors = plugin.Validate();
                            if (errors != null)
                            {
                                MessageBox.Show(errors, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }
                            // Need to change this: should start multithreaded operation, and then perform the original asked operation.
                            // toPerformAfterSave
                            if (string.IsNullOrEmpty(filename))
                            {
                                SaveAsAction(nextAction);
                            }
                            else
                            {
                                SaveAction(false, nextAction);
                            }
                            // Cancel current operation, since stuff after multithreading will take care of the operation.
                            return false;
                        }
                    case DialogResult.No:
                        break;
                    case DialogResult.Cancel:
                        return false;
                }
            }            
            if (!onlyAfterSave && nextAction != null)
            {
                nextAction();
            }
            return true;
        }

        public void UpdateStatus()
        {
            SetTitle();
        }

        private void LoadIcons(IGamePlugin plugin)
        {
            TemplateType template = plugin.Map.TemplateTypes.Where(tt => (tt.Flag & TemplateTypeFlag.Clear) != TemplateTypeFlag.Clear && tt.IconWidth == 1 && tt.IconHeight == 1
                && (tt.Theaters == null || tt.Theaters.Contains(plugin.Map.Theater)))
                .OrderBy(tt => tt.Name).FirstOrDefault();
            Tile templateTile = null;
            if (template != null)
            {
                Globals.TheTilesetManager.GetTileData(plugin.Map.Theater.Tilesets, template.Name, template.GetIconIndex(template.GetFirstValidIcon()), out templateTile, false, true);
            }
            // For the following, check if the thumbnail was initialised.
            SmudgeType smudge = plugin.Map.SmudgeTypes.Where(sm => !sm.IsAutoBib && sm.Icons == 1 && sm.Size.Width == 1 && sm.Size.Height == 1 && sm.Thumbnail != null
                && (!Globals.FilterTheaterObjects || sm.Theaters == null || sm.Theaters.Contains(plugin.Map.Theater)))
                .OrderBy(sm => sm.ID).FirstOrDefault();
            OverlayType overlay = plugin.Map.OverlayTypes.Where(ov => (ov.Flag & OverlayTypeFlag.Crate) != OverlayTypeFlag.None && ov.Thumbnail != null
                && (!Globals.FilterTheaterObjects || ov.Theaters == null || ov.Theaters.Contains(plugin.Map.Theater)))
                .OrderBy(ov => ov.ID).FirstOrDefault();
            if (overlay == null)
            {
                overlay = plugin.Map.OverlayTypes.Where(ov => (ov.Flag & OverlayTypeFlag.Flag) == OverlayTypeFlag.Flag && ov.Thumbnail != null
                    && (!Globals.FilterTheaterObjects || ov.Theaters == null || ov.Theaters.Contains(plugin.Map.Theater)))
                    .OrderBy(ov => ov.ID).FirstOrDefault();
            }
            TerrainType terrain = plugin.Map.TerrainTypes.Where(tr => tr.Thumbnail != null &&
                (!Globals.FilterTheaterObjects || tr.Theaters == null || tr.Theaters.Contains(plugin.Map.Theater)))
                .OrderBy(tr => tr.ID).FirstOrDefault();
            InfantryType infantry = plugin.Map.InfantryTypes.FirstOrDefault();
            UnitType unit = plugin.Map.UnitTypes.FirstOrDefault();
            BuildingType building = plugin.Map.BuildingTypes.Where(bl => bl.Size.Width == 2 && bl.Size.Height == 2
                                        && (!Globals.FilterTheaterObjects || bl.Theaters == null || bl.Theaters.Contains(plugin.Map.Theater))).OrderBy(bl => bl.ID).FirstOrDefault();
            OverlayType resource = plugin.Map.OverlayTypes.Where(ov => (ov.Flag & OverlayTypeFlag.TiberiumOrGold) == OverlayTypeFlag.TiberiumOrGold
                                        && (!Globals.FilterTheaterObjects || ov.Theaters == null || ov.Theaters.Contains(plugin.Map.Theater))).OrderBy(ov => ov.ID).FirstOrDefault();
            OverlayType wall = plugin.Map.OverlayTypes.Where(ov => (ov.Flag & OverlayTypeFlag.Wall) == OverlayTypeFlag.Wall
                                        && (!Globals.FilterTheaterObjects || ov.Theaters == null || ov.Theaters.Contains(plugin.Map.Theater))).OrderBy(ov => ov.ID).FirstOrDefault();
            bool gotBeacon = Globals.TheTilesetManager.GetTileData(plugin.Map.Theater.Tilesets, "beacon", 0, out Tile waypoint, false, true);
            if (!gotBeacon)
            {
                // Beacon only exists in rematered graphics. Get fallback.
                Globals.TheTilesetManager.GetTileData(plugin.Map.Theater.Tilesets, "armor", 6, out waypoint, false, true);
            }
            Globals.TheTilesetManager.GetTileData(plugin.Map.Theater.Tilesets, "mine", 3, out Tile cellTrigger, false, true);
            LoadNewIcon(mapToolStripButton, templateTile?.Image, plugin, 0);
            LoadNewIcon(smudgeToolStripButton, smudge?.Thumbnail, plugin, 1);
            //LoadNewIcon(overlayToolStripButton, overlayTile?.Image, plugin, 2);
            LoadNewIcon(overlayToolStripButton, overlay?.Thumbnail, plugin, 2);
            LoadNewIcon(terrainToolStripButton, terrain?.Thumbnail, plugin, 3);
            LoadNewIcon(infantryToolStripButton, infantry?.Thumbnail, plugin, 4);
            LoadNewIcon(unitToolStripButton, unit?.Thumbnail, plugin, 5);
            LoadNewIcon(buildingToolStripButton, building?.Thumbnail, plugin, 6);
            LoadNewIcon(resourcesToolStripButton, resource?.Thumbnail, plugin, 7);
            LoadNewIcon(wallsToolStripButton, wall?.Thumbnail, plugin, 8);
            LoadNewIcon(waypointsToolStripButton, waypoint?.Image, plugin, 9);
            LoadNewIcon(cellTriggersToolStripButton, cellTrigger?.Image, plugin, 10);
            // The Texture manager returns a clone of its own cached image. The Tileset manager caches those clones,
            // and is responsible for their cleanup, but if we use it directly it needs to be disposed.
            // Icon: chrono cursor from TEXTURES_SRGB.MEG
            using (Bitmap select = Globals.TheTextureManager.GetTexture(@"DATA\ART\TEXTURES\SRGB\ICON_SELECT_GREEN_04.DDS", null, false).Item1)
            {
                LoadNewIcon(selectToolStripButton, select, plugin, 11, false);
            }
        }
        private void LoadNewIcon(ViewToolStripButton button, Bitmap image, IGamePlugin plugin, int index)
        {
            LoadNewIcon(button, image, plugin, index, true);
        }

        private void LoadNewIcon(ViewToolStripButton button, Bitmap image, IGamePlugin plugin, int index, bool crop)
        {
            if (button.Tag == null && button.Image != null)
            {
                // Backup default image
                button.Tag = button.Image;
            }
            if (image == null || plugin == null)
            {
                if (button.Tag is Image img)
                {
                    button.Image = img;
                }
                return;
            }
            int id = ((int)plugin.GameType) << 8 | Enumerable.Range(0, plugin.Map.TheaterTypes.Count).FirstOrDefault(i => plugin.Map.TheaterTypes[i].Equals(plugin.Map.Theater)) << 4 | index;
            if (theaterIcons.TryGetValue(id, out Bitmap bm))
            {
                button.Image = bm;
            }
            else
            {
                Rectangle opaqueBounds = crop ? ImageUtils.CalculateOpaqueBounds(image) : new Rectangle(0, 0, image.Width, image.Height);
                if (opaqueBounds.IsEmpty)
                {
                    if (button.Tag is Image tagImg)
                    {
                        button.Image = tagImg;
                    }
                    return;
                }
                Bitmap img = image.FitToBoundingBox(opaqueBounds, 24, 24, Color.Transparent);
                theaterIcons[id] = img;
                button.Image = img;
            }
        }

        private void mapPanel_PostRender(Object sender, RenderEventArgs e)
        {
            // Only clear this after all rendering is complete.
            if (!loadMultiThreader.IsExecuting && !saveMultiThreader.IsExecuting)
            {
                SimpleMultiThreading.RemoveBusyLabel(this);
                bool performJump = false;
                lock (jumpToBounds_lock)
                {
                    if (jumpToBounds)
                    {
                        jumpToBounds = false;
                        performJump = true;
                    }
                }
                if (performJump)
                {
                    //MessageBox.Show("jumping");
                    if (plugin != null && plugin.Map != null && mapPanel.MapImage != null)
                    {
                        Rectangle rect = plugin.Map.Bounds;
                        rect.Inflate(1, 1);
                        if (plugin.Map.Metrics.Bounds != rect)
                        {
                            mapPanel.JumpToPosition(plugin.Map.Metrics, rect, true);
                        }
                    }
                }
            }
        }
    }
}
