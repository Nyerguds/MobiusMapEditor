//
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
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using static MobiusEditor.Utility.SimpleMultiThreading;

namespace MobiusEditor
{
    public partial class MainForm : Form, IFeedBackHandler, IHasStatusLabel
    {
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
                        MapLayerFlag active = activeLayers;
                        // Save some processing by just always removing these.
                        if (plugin.GameType != GameType.SoleSurvivor)
                        {
                            active &= ~MapLayerFlag.FootballArea;
                        }
                        else if (plugin.GameType != GameType.RedAlert)
                        {
                            active &= ~MapLayerFlag.BuildingFakes;
                        }
                        activeTool.Layers = active;
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

        private readonly MRU mru;

        private readonly UndoRedoList<UndoRedoEventArgs> url = new UndoRedoList<UndoRedoEventArgs>(Globals.UndoRedoStackSize);

        private readonly Timer steamUpdateTimer = new Timer();

        private SimpleMultiThreading multiThreader;
        Label busyStatusLabel;
        public Label StatusLabel
        {
            get { return busyStatusLabel; }
            set { busyStatusLabel = value; }
        }

        static MainForm()
        {
            toolTypes = ((IEnumerable<ToolType>)Enum.GetValues(typeof(ToolType))).Where(t => t != ToolType.None).ToArray();
        }

        public MainForm(String fileToOpen)
        {
            this.filename = fileToOpen;

            InitializeComponent();
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
                toolStripButton.MouseMove += mainToolStrip_MouseMove;
            }
#if !DEVELOPER
            fileExportMenuItem.Visible = false;
            developerToolStripMenuItem.Visible = false;
#endif
            url.Tracked += UndoRedo_Updated;
            url.Undone += UndoRedo_Updated;
            url.Redone += UndoRedo_Updated;
            UpdateUndoRedo();
            steamUpdateTimer.Interval = 500;
            steamUpdateTimer.Tick += SteamUpdateTimer_Tick;
            multiThreader = new SimpleMultiThreading(this);
            multiThreader.ProcessingLabelBorder = BorderStyle.Fixed3D;
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
            NewFile();
        }

        private void FileOpenMenuItem_Click(object sender, EventArgs e)
        {
            if (!PromptSaveMap())
            {
                return;
            }

#if DEVELOPER
            var pgmFilter = "|PGM files (*.pgm)|*.pgm";
            string allSupported = "All supported types (*.ini;*.bin;*.mpr;*.pgm)|*.ini;*.bin;*.mpr;*.pmg";
#else
            var pgmFilter = string.Empty;
            string allSupported = "All supported types (*.ini;*.bin;*.mpr)|*.ini;*.bin;*.mpr";
#endif
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
                OpenFile(selectedFileName, false);
            }
        }

        private void FileSaveMenuItem_Click(object sender, EventArgs e)
        {
            if (plugin == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(filename) || !Directory.Exists(Path.GetDirectoryName(filename)))
            {
                fileSaveAsMenuItem.PerformClick();
                return;
            }
            String errors = plugin.Validate();
            if (errors != null)
            {
                MessageBox.Show(errors, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                return;
            }
            var fileInfo = new FileInfo(filename);
            SaveFile(fileInfo.FullName, loadedFileType);
        }

        private void FileSaveAsMenuItem_Click(object sender, EventArgs e)
        {
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
            if (savePath != null)
            {
                var fileInfo = new FileInfo(savePath);
                SaveFile(fileInfo.FullName, FileType.INI);
            }
        }

        private void FileExportMenuItem_Click(object sender, EventArgs e)
        {
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

                sfd.Filter = "MEG files (*.meg)|*.meg";
                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    savePath = sfd.FileName;
                }
            }
            if (savePath != null)
            {
                plugin.Save(savePath, FileType.MEG);
            }
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
            if (plugin is SoleSurvivor.GamePlugin ssPlugin)
            {
                cratesSettings = new PropertyTracker<SoleSurvivor.CratesSection>(ssPlugin.CratesSection);
            }
            string extraIniText = plugin.ExtraIniText;
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
                    if (!extraIniText.Equals(msd.ExtraIniText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            plugin.ExtraIniText = msd.ExtraIniText;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Errors occurred when applying rule changes:\n\n" + ex.Message, GetProgramVersionTitle());
                        }
                        rulesChanged = true;
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
                            plugin.Map.TeamTypes.Clear();
                            plugin.Map.TeamTypes.AddRange(oldTeamTypes);
                            ev.Map.Triggers = oldTriggers;
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
                            plugin.Map.TeamTypes.Clear();
                            plugin.Map.TeamTypes.AddRange(newTeamTypes);
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
                                    ev.Map.CellTriggers[cellTriggerLocations[celltrigger]] = celltrigger;
                                }
                            }
                            if (ev.Plugin != null)
                            {
                                ev.Plugin.Map.Triggers = oldTriggers;
                                ev.Plugin.Dirty = origDirtyState;
                            }
                            // Repaint map labels
                            ev.MapPanel.Invalidate();
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
                                    if (Trigger.IsEmpty(celltrigger.Trigger))
                                    {
                                        ev.Map.CellTriggers[cellTriggerLocations[celltrigger]] = null;
                                    }
                                }
                            }
                            if (ev.Plugin != null)
                            {
                                ev.Plugin.Map.Triggers = newTriggers;
                                ev.Plugin.Dirty = true;
                            }
                            // Repaint map labels
                            ev.MapPanel.Invalidate();
                        }
                        url.Track(undoAction, redoAction);
                        // No longer a full refresh, since celltriggers function is no longer disabled when no triggers are found.
                        mapPanel.Invalidate();
                    }
                }
            }
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
            OpenFile(e.FullName, true);
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

        private void NewFile()
        {
            if (!PromptSaveMap())
            {
                return;
            }
            GameType gameType = GameType.None;
            string theater = null;
            bool isTdMegaMap = false;
            using (NewMapDialog nmd = new NewMapDialog())
            {
                if (nmd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                gameType = nmd.GameType;
                isTdMegaMap = nmd.MegaMap;
                theater = nmd.TheaterName;
            }
            string[] modPaths = null;
            if (ModPaths != null)
            {
                ModPaths.TryGetValue(gameType, out modPaths);
            }
            Unload();
            multiThreader.ExecuteThreaded(() => NewFile(gameType, theater, isTdMegaMap, modPaths), PostLoad, true, LoadUnloadUi, "Loading new map");
        }

        private void OpenFile(String fileName, bool askSave)
        {
            if (askSave && !PromptSaveMap())
            {
                return;
            }
            var fileInfo = new FileInfo(fileName);
            String name = fileInfo.FullName;
            if (!IdentifyMap(name, out FileType fileType, out GameType gameType, out bool isTdMegaMap))
            {
                MessageBox.Show(string.Format("Error loading {0}: {1}", fileInfo.Name, "Could not identify map type."), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string[] modPaths = null;
            if (ModPaths != null)
            {
                ModPaths.TryGetValue(gameType, out modPaths);
            }
            multiThreader.ExecuteThreaded(() => LoadFile(name, fileType, gameType, isTdMegaMap, modPaths), PostLoad, true, LoadUnloadUi, "Loading map");
        }

        private void SaveFile(string saveFilename, FileType inputNameType)
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
            // Replace by multithreaded part
            multiThreader.ExecuteThreaded(() => SaveFile(plugin, saveFilename, fileType), PostSave, true, (bl, str) => EnableDisableUi(bl, str, current), "Saving map");
            //plugin.Save(saveFilename, fileType)
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
#if DEVELOPER
                case ".pgm":
                    fileType = FileType.PGM;
                    break;
#endif
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
            if (iniContents == null || !GeneralUtils.CheckForIniInfo(iniContents, "Map") || !GeneralUtils.CheckForIniInfo(iniContents, "Basic"))
            {
                return false;
            }
            switch (fileType)
            {
                case FileType.INI:
                    {
                        gameType = RedAlert.GamePlugin.CheckForRAMap(iniContents) ? GameType.RedAlert : GameType.TiberianDawn;
                        break;
                    }
                case FileType.BIN:
                    {
                        gameType = File.Exists(iniFile) ? GameType.TiberianDawn : GameType.None;
                        break;
                    }
#if DEVELOPER
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
#endif
            }
            if (gameType == GameType.TiberianDawn)
            {
                isTdMegaMap = TiberianDawn.GamePlugin.CheckForMegamap(iniContents);
                if (SoleSurvivor.GamePlugin.CheckForSSmap(iniContents))
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
        private void LoadUnloadUi(bool enableUI, string label)
        {
            fileNewMenuItem.Enabled = enableUI;
            fileOpenMenuItem.Enabled = enableUI;
            fileRecentFilesMenuItem.Enabled = enableUI;
            viewMapToolStripMenuItem.Enabled = enableUI;
            viewIndicatorsToolStripMenuItem.Enabled = enableUI;
            if (!enableUI)
            {
                Unload();
                multiThreader.CreateBusyLabel(this, label);
            }
        }

        /// <summary>
        /// The 'lighter' enable/disable UI function, for map saving.
        /// </summary>
        /// <param name="enableUI"></param>
        /// <param name="label"></param>
        private void EnableDisableUi(bool enableUI, string label, ToolType storedToolType)
        {
            fileNewMenuItem.Enabled = enableUI;
            fileOpenMenuItem.Enabled = enableUI;
            fileRecentFilesMenuItem.Enabled = enableUI;
            viewMapToolStripMenuItem.Enabled = enableUI;
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
                multiThreader.CreateBusyLabel(this, label);
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
                plugin = new TiberianDawn.GamePlugin(!noImage, isTdMegaMap);
            }
            else if (gameType == GameType.RedAlert)
            {
                Globals.TheTeamColorManager.Reset();
                Globals.TheTeamColorManager.Load(@"DATA\XML\CNCRATEAMCOLORS.XML");
                plugin = new RedAlert.GamePlugin(!noImage);
            }
            else if (gameType == GameType.SoleSurvivor)
            {
                Globals.TheTeamColorManager.Reset();
                AddTeamColorNone(Globals.TheTeamColorManager);
                Globals.TheTeamColorManager.Load(@"DATA\XML\CNCTDTEAMCOLORS.XML");
                plugin = new SoleSurvivor.GamePlugin(!noImage, isTdMegaMap);
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
        private static (string FileName, FileType FileType, IGamePlugin Plugin, string[] Errors) NewFile(GameType gameType, string theater, bool isTdMegaMap, string[] modPaths)
        {
            try
            {
                IGamePlugin plugin = LoadNewPlugin(gameType, isTdMegaMap, modPaths);
                plugin.New(theater);
                if (SteamworksUGC.IsInit)
                {
                    plugin.Map.BasicSection.Author = SteamFriends.GetPersonaName();
                }
                return (null, FileType.None, plugin, null);
            }
            catch (Exception ex)
            {
                List<string> errorMessage = new List<string>();
                errorMessage.Add("Error loading map: " + ex.Message);
#if DEBUG
                errorMessage.Add(ex.StackTrace);
#endif
                return (null, FileType.None, null, errorMessage.ToArray());
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
        private static (string, FileType, IGamePlugin, string[]) LoadFile(string loadFilename, FileType fileType, GameType gameType, bool isTdMegaMap, string[] modPaths)
        {
            try
            {
                IGamePlugin plugin = LoadNewPlugin(gameType, isTdMegaMap, modPaths);
                string[] errors = plugin.Load(loadFilename, fileType).ToArray();
                return (loadFilename, fileType, plugin, errors);
            }
            catch (Exception ex)
            {
                List<string> errorMessage = new List<string>();
                errorMessage.Add("Error loading map: " + ex.Message);
#if DEBUG
                errorMessage.Add(ex.StackTrace);
#endif
                return (loadFilename, fileType, null, errorMessage.ToArray());
            }
        }

        private static (string FileName, bool SavedOk, string error) SaveFile(IGamePlugin plugin, string saveFilename, FileType fileType)
        {
            try
            {
                plugin.Save(saveFilename, fileType);
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

        private void PostLoad((string FileName, FileType FileType, IGamePlugin Plugin, string[] Errors) loadInfo)
        {
            string[] errors = loadInfo.Errors ?? new string[0];
            if (loadInfo.Plugin == null)
            {
                if (loadInfo.FileName != null)
                {
                    var fileInfo = new FileInfo(loadInfo.FileName);
                    mru.Remove(fileInfo);
                }
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
                mapPanel.MapImage = plugin.MapImage;
                filename = loadInfo.FileName;
                loadedFileType = loadInfo.FileType;
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

        private void PostSave((string FileName, bool SavedOk, string Error) saveInfo)
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
                if (plugin.Map.TemplateTypes.Any(t => t.Theaters == null || t.Theaters.Contains(th))) availableToolTypes |= ToolType.Map;
                if (plugin.Map.SmudgeTypes.Any()) availableToolTypes |= ToolType.Smudge;
                if (plugin.Map.OverlayTypes.Any(t => t.IsPlaceable && (!Globals.FilterTheaterObjects || t.Theaters == null || t.Theaters.Contains(th)))) availableToolTypes |= ToolType.Overlay;
                if (plugin.Map.TerrainTypes.Any(t => !Globals.FilterTheaterObjects || t.Theaters == null || t.Theaters.Contains(th))) availableToolTypes |= ToolType.Terrain;
                if (plugin.Map.InfantryTypes.Any()) availableToolTypes |= ToolType.Infantry;
                if (plugin.Map.UnitTypes.Any()) availableToolTypes |= ToolType.Unit;
                if (plugin.Map.BuildingTypes.Any(t => !Globals.FilterTheaterObjects || t.Theaters == null || t.Theaters.Contains(th))) availableToolTypes |= ToolType.Building;
                if (plugin.Map.OverlayTypes.Any(t => t.IsResource && (!Globals.FilterTheaterObjects || t.Theaters == null || t.Theaters.Contains(th)))) availableToolTypes |= ToolType.Resources;
                if (plugin.Map.OverlayTypes.Any(t => t.IsWall && (!Globals.FilterTheaterObjects || t.Theaters == null || t.Theaters.Contains(th)))) availableToolTypes |= ToolType.Wall;
                // Always allow celltrigger tool, even if triggers list is empty; it contains a tooltip saying which triggers are eligible.
                availableToolTypes |= ToolType.CellTrigger;
                // TODO - Select tool will always be enabled
                availableToolTypes |= ToolType.Select;
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
            fileExportMenuItem.Enabled = enable && hasPlugin;
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
            developerGoToINIMenuItem.Enabled = enable && hasPlugin;
            developerDebugToolStripMenuItem.Enabled = enable && hasPlugin;
            developerGenerateMapPreviewDirectoryMenuItem.Enabled = enable && hasPlugin;

            viewMapToolStripMenuItem.Enabled = enable;
            viewIndicatorsToolStripMenuItem.Enabled = enable;

            // Special rules per game.
            viewMapBuildingsMenuItem.Visible = !hasPlugin || plugin.GameType != GameType.SoleSurvivor;
            viewIndicatorsBuildingFakeLabelsMenuItem.Visible = !hasPlugin || plugin.GameType == GameType.RedAlert;
            viewIndicatorsBuildingRebuildLabelsMenuItem.Visible = !hasPlugin || plugin.GameType != GameType.SoleSurvivor;
            viewIndicatorsFootballAreaMenuItem.Visible = !hasPlugin || plugin.GameType == GameType.SoleSurvivor;
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
            if (!viewBoundariesBoundariesMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.Boundaries;
            }
            if (!viewBoundariesMapSymmetryMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.MapSymmetry;
            }
            if (!viewMapBuildingsMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.Buildings;
            }
            if (!viewMapUnitsMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.Units;
            }
            if (!viewMapInfantryMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.Infantry;
            }
            if (!viewMapTerrainMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.Terrain;
            }
            if (!viewMapOverlayMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.OverlayAll;
            }
            if (!viewMapSmudgeMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.Smudge;
            }
            if (!viewMapWaypointsMenuItem.Checked)
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
            OpenFile(files[0], true);
        }

        private void ViewMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            UpdateVisibleLayers();
        }

        private void ViewMapEnableAllMenuItem_Click(object sender, EventArgs e)
        {
            ITool activeTool = this.activeTool;
            try
            {
                // Suppress updates.
                this.activeTool = null;
                viewMapBuildingsMenuItem.Checked = true;
                viewMapUnitsMenuItem.Checked = true;
                viewMapInfantryMenuItem.Checked = true;
                viewMapTerrainMenuItem.Checked = true;
                viewMapOverlayMenuItem.Checked = true;
                viewMapSmudgeMenuItem.Checked = true;
                viewMapWaypointsMenuItem.Checked = true;
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

        private void ViewMapDisableAllMenuItem_Click(object sender, EventArgs e)
        {
            ITool activeTool = this.activeTool;
            try
            {
                // Suppress updates.
                this.activeTool = null;
                viewMapBuildingsMenuItem.Checked = false;
                viewMapUnitsMenuItem.Checked = false;
                viewMapInfantryMenuItem.Checked = false;
                viewMapTerrainMenuItem.Checked = false;
                viewMapOverlayMenuItem.Checked = false;
                viewMapSmudgeMenuItem.Checked = false;
                viewMapWaypointsMenuItem.Checked = false;
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

        private void ViewIndicatorsEnableAllToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            ITool activeTool = this.activeTool;
            try
            {
                // Suppress updates.
                this.activeTool = null;
                viewBoundariesBoundariesMenuItem.Checked = true;
                viewIndicatorsWaypointsMenuItem.Checked = true;
                viewIndicatorsCellTriggersMenuItem.Checked = true;
                viewIndicatorsObjectTriggersMenuItem.Checked = true;
                viewIndicatorsBuildingFakeLabelsMenuItem.Checked = true;
                viewIndicatorsBuildingRebuildLabelsMenuItem.Checked = true;
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

        private void ViewIndicatorsDisableAllToolStripMenuItem_Click(Object sender, EventArgs e)
        {
            ITool activeTool = this.activeTool;
            try
            {
                // Suppress updates.
                this.activeTool = null;
                viewIndicatorsWaypointsMenuItem.Checked = false;
                viewIndicatorsCellTriggersMenuItem.Checked = false;
                viewIndicatorsObjectTriggersMenuItem.Checked = false;
                viewIndicatorsBuildingFakeLabelsMenuItem.Checked = false;
                viewIndicatorsBuildingRebuildLabelsMenuItem.Checked = false;
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

        private void developerGoToINIMenuItem_Click(object sender, EventArgs e)
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

        private void developerGenerateMapPreviewDirectoryMenuItem_Click(object sender, EventArgs e)
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

        private void developerDebugShowOverlapCellsMenuItem_CheckedChanged(object sender, EventArgs e)
        {
#if DEVELOPER
            Globals.Developer.ShowOverlapCells = developerDebugShowOverlapCellsMenuItem.Checked;
#endif
        }

        private void filePublishMenuItem_Click(object sender, EventArgs e)
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
            if (!PromptSaveMap())
            {
                return;
            }
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
                // Fix description
                if (plugin.Map.SteamSection.Description.Any(ch => ch == '\r' || ch == '\n'))
                {
                    plugin.Map.SteamSection.Description = plugin.Map.SteamSection.Description.Replace("\r\n", "\n").Replace("\r", "\n").Replace('\n', '@'); 
                }
                // This takes care of saving the Steam info into the map.
                fileSaveMenuItem.PerformClick();
            }
        }

        private void mainToolStrip_MouseMove(object sender, MouseEventArgs e)
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
                this.OpenFile(filename, false);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !PromptSaveMap();
            if (e.Cancel)
            {
                return;
            }
            // If loading, abort. Wait for confirmation of abort before continuing the unloading.
            if (multiThreader != null)
            {
                multiThreader.AbortThreadedOperation(5000);
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

        private bool PromptSaveMap()
        {
            bool cancel = false;
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
                            if (string.IsNullOrEmpty(filename))
                            {
                                fileSaveAsMenuItem.PerformClick();
                            }
                            else
                            {
                                fileSaveMenuItem.PerformClick();
                            }
                        }
                        break;
                    case DialogResult.No:
                        break;
                    case DialogResult.Cancel:
                        cancel = true;
                        break;
                }
            }
            return !cancel;
        }

        public void UpdateStatus()
        {
            SetTitle();
        }

        private void LoadIcons(IGamePlugin plugin)
        {
            TemplateType template = plugin.Map.TemplateTypes.Where(tt => (tt.Flag & TemplateTypeFlag.Clear) != TemplateTypeFlag.Clear && tt.IconWidth == 1 && tt.IconHeight == 1
                                        && (tt.Theaters == null || tt.Theaters.Contains(plugin.Map.Theater))).OrderBy(tt => tt.Name).FirstOrDefault();
            Tile templateTile = null;
            if (template != null) Globals.TheTilesetManager.GetTileData(plugin.Map.Theater.Tilesets, template.Name, 0, out templateTile, false, true);
            SmudgeType smudge = plugin.Map.SmudgeTypes.Where(sm => !sm.IsAutoBib && sm.Icons == 1 && sm.Size.Width == 1 && sm.Size.Height == 1
                                        && (sm.Theaters == null || sm.Theaters.Contains(plugin.Map.Theater))).OrderBy(sm => sm.ID).FirstOrDefault();
            OverlayType overlay = plugin.Map.OverlayTypes.Where(ov => (ov.Flag & OverlayTypeFlag.Crate) == OverlayTypeFlag.Crate
                                        && (ov.Theaters == null || ov.Theaters.Contains(plugin.Map.Theater))).OrderBy(ov => ov.ID).FirstOrDefault();
            if (overlay == null)
            {
                overlay = plugin.Map.OverlayTypes.Where(ov => (ov.Flag & OverlayTypeFlag.Flag) == OverlayTypeFlag.Flag
                                            && (ov.Theaters == null || ov.Theaters.Contains(plugin.Map.Theater))).OrderBy(ov => ov.ID).FirstOrDefault();
            }
            Tile overlayTile = null;
            if (overlay != null) Globals.TheTilesetManager.GetTileData(plugin.Map.Theater.Tilesets, overlay.Name, 0, out overlayTile, false, true);
            TerrainType terrain = plugin.Map.TerrainTypes.Where(tr => tr.Theaters == null || tr.Theaters.Contains(plugin.Map.Theater)).OrderBy(tr => tr.ID).FirstOrDefault(); ;
            InfantryType infantry = plugin.Map.InfantryTypes.FirstOrDefault();
            UnitType unit = plugin.Map.UnitTypes.FirstOrDefault();
            BuildingType building = plugin.Map.BuildingTypes.Where(bl => bl.Size.Width == 2 && bl.Size.Height == 2
                                        && (bl.Theaters == null || bl.Theaters.Contains(plugin.Map.Theater))).OrderBy(bl => bl.ID).FirstOrDefault();
            OverlayType resource = plugin.Map.OverlayTypes.Where(ov => (ov.Flag & OverlayTypeFlag.TiberiumOrGold) == OverlayTypeFlag.TiberiumOrGold
                                        && (ov.Theaters == null || ov.Theaters.Contains(plugin.Map.Theater))).OrderBy(ov => ov.ID).FirstOrDefault();
            OverlayType wall = plugin.Map.OverlayTypes.Where(ov => (ov.Flag & OverlayTypeFlag.Wall) == OverlayTypeFlag.Wall
                                        && (ov.Theaters == null || ov.Theaters.Contains(plugin.Map.Theater))).OrderBy(ov => ov.ID).FirstOrDefault();
            Globals.TheTilesetManager.GetTileData(plugin.Map.Theater.Tilesets, "beacon", 0, out Tile waypoint, false, true);
            Globals.TheTilesetManager.GetTileData(plugin.Map.Theater.Tilesets, "mine", 3, out Tile cellTrigger, false, true);
            LoadNewIcon(mapToolStripButton, templateTile?.Image, plugin, 0);
            LoadNewIcon(smudgeToolStripButton, smudge?.Thumbnail, plugin, 1);
            LoadNewIcon(overlayToolStripButton, overlayTile?.Image, plugin, 2);
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
            if (image == null || plugin == null)
            {
                if (button.Tag is Image img)
                {
                    button.Image = img;
                }
                return;
            }
            int id = ((int)plugin.GameType) << 8 | Enumerable.Range(0, plugin.Map.TheaterTypes.Count).FirstOrDefault(i => plugin.Map.TheaterTypes[i].Equals(plugin.Map.Theater)) << 4 | index;
            if (button.Tag == null)
            {
                // Backup default image
                button.Tag = button.Image;
            }
            if (theaterIcons.TryGetValue(id, out Bitmap bm))
            {
                button.Image = bm;
            }
            else
            {
                Rectangle opaqueBounds = crop ? TextureManager.CalculateOpaqueBounds(image) : new Rectangle(0, 0, image.Width, image.Height);
                Bitmap img = image.FitToBoundingBox(opaqueBounds, 24, 24, Color.Transparent);
                theaterIcons[id] = img;
                button.Image = img;
            }
        }

        private void mapPanel_PostRender(Object sender, RenderEventArgs e)
        {
            if (!multiThreader.IsExecuting)
            {
                multiThreader.RemoveBusyLabel(this);
            }
        }
    }
}
