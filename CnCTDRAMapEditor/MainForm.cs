//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MobiusEditor
{
    public partial class MainForm : Form, IFeedBackHandler, IHasStatusLabel
    {

        private Dictionary<string, Bitmap> theaterIcons = new Dictionary<string, Bitmap>();

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
        private GameType oldMockGame;
        private ToolType oldSelectedTool = ToolType.None;
        private Dictionary<ToolType, Object> oldMockObjects;
        private ViewToolStripButton[] viewToolStripButtons;

        private IGamePlugin plugin;
        private FileType loadedFileType;
        private string filename;
        private bool shouldCheckUpdate;
        private bool startedUpdate;
        // Not sure if this lock works; multiple functions can somehow run simultaneously on the same UI update thread?
        private readonly object jumpToBounds_lock = new Object();
        private bool jumpToBounds;

        private readonly MRU mru;

        private readonly UndoRedoList<UndoRedoEventArgs, ToolType> url = new UndoRedoList<UndoRedoEventArgs, ToolType>(Globals.UndoRedoStackSize);

        private readonly Timer steamUpdateTimer = new Timer();

        private SimpleMultiThreading loadMultiThreader;
        private SimpleMultiThreading saveMultiThreader;
        public Label StatusLabel { get; set; }
        private Point lastInfoPoint = new Point(-1, -1);
        private Point lastInfoSubPixelPoint = new Point(-1, -1);
        private String lastDescription = null;

        static MainForm()
        {
            toolTypes = ((IEnumerable<ToolType>)Enum.GetValues(typeof(ToolType))).Where(t => t != ToolType.None).ToArray();
        }

        public MainForm(String fileToOpen)
        {
            this.filename = fileToOpen;

            InitializeComponent();
            mapPanel.SmoothScale = Globals.MapSmoothScale;
            // Show on monitor that the mouse is in, since that's where the user is probably looking.
            Screen s = Screen.FromPoint(Cursor.Position);
            Point location = s.Bounds.Location;
            this.Left = location.X;
            this.Top = location.Y;

            // Synced from app settings.
            this.toolsOptionsBoundsObstructFillMenuItem.Checked = Globals.BoundsObstructFill;
            this.toolsOptionsSafeDraggingMenuItem.Checked = Globals.TileDragProtect;
            this.toolsOptionsRandomizeDragPlaceMenuItem.Checked = Globals.TileDragRandomize;
            this.toolsOptionsPlacementGridMenuItem.Checked = Globals.ShowPlacementGrid;
            this.toolsOptionsOutlineAllCratesMenuItem.Checked = Globals.OutlineAllCrates;
            this.toolsOptionsCratesOnTopMenuItem.Checked = Globals.CratesOnTop;

            // Obey the settings.
            this.mapPanel.SmoothScale = Globals.MapSmoothScale;
            this.mapPanel.BackColor = Globals.MapBackColor;
            SetTitle();
            toolForms = new Dictionary<ToolType, IToolDialog>();
            oldMockGame = GameType.None;
            oldMockObjects = Globals.RememberToolData ? new Dictionary<ToolType, object>() : null;
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
            url.Tracked += UndoRedo_Tracked;
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
            String mainTitle = Program.ProgramVersionTitle;
            string updating = this.startedUpdate ? " [CHECKING FOR UPDATES]" : String.Empty;
            if (plugin == null)
            {
                this.Text = mainTitle + updating;
                return;
            }
            string mapName = plugin.Map.BasicSection.Name;
            bool mapNameEmpty = plugin.GameInfo.MapNameIsEmpty(mapName);
            bool fileNameEmpty = filename == null;
            string mapFilename = "\"" + (fileNameEmpty ? noname + plugin.GameInfo.DefaultExtension : Path.GetFileName(filename)) + "\"";
            string mapShowName;
            if (!mapNameEmpty && !fileNameEmpty)
            {
                mapShowName = mapFilename + " - " + mapName;
            }
            else if (!mapNameEmpty)
            {
                mapShowName = mapName;
            }
            else
            {
                mapShowName = mapFilename;
            }
            this.Text = string.Format("{0}{1} [{2}] - {3}{4}", mainTitle, updating, plugin.GameInfo.Name, mapShowName, plugin != null && plugin.Dirty ? " *" : String.Empty);
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
                        //selectToolStripButton.PerformClick();
                        return true;
                    case OemScanCode.NumPadAsterisk:
                        viewZoomResetMenuItem.PerformClick();
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
                            ZoomIn();
                            return true;
                        case Keys.OemMinus:
                        case Keys.Subtract:
                            ZoomOut();
                            return true;
                    }
                    if (delta != Point.Empty)
                    {
                        Point curPoint = mapPanel.AutoScrollPosition;
                        SizeF zoomedCell = activeTool.NavigationWidget.ZoomedCellSize;
                        // autoscrollposition is WEIRD. Exposed as negative, needs to be given as positive.
                        mapPanel.AutoScrollPosition = new Point(-curPoint.X + (int)Math.Round(delta.X * zoomedCell.Width), -curPoint.Y + (int)Math.Round(delta.Y * zoomedCell.Width));
                        mapPanel.InvalidateScroll();
                        // Map moved without mouse movement. Pretend mouse moved.
                        activeTool.NavigationWidget.Refresh();
                        UpdateCellStatusLabel(true);
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

        private void MainForm_KeyPress(Object sender, KeyPressEventArgs e)
        {
            // Workaround for localised non-numpad versions of keys.
            char typedChar = e.KeyChar;
            bool handled = true;
            switch (typedChar)
            {
                case '*':
                    ZoomReset();
                    break;
                case '+':
                    ZoomIn();
                    break;
                case '-':
                    ZoomOut();
                    break;
                default:
                    handled = false;
                    break;
            }
            if (handled)
            {
                e.Handled = true;
            }
        }

        private void ZoomIn()
        {
            if (activeTool == null || activeTool.NavigationWidget.IsDragging())
            {
                return;
            }
            mapPanel.IncreaseZoomStep();
        }

        private void ZoomOut()
        {
            if (activeTool == null || activeTool.NavigationWidget.IsDragging())
            {
                return;
            }
            mapPanel.DecreaseZoomStep();
        }

        private void ZoomReset()
        {
            mapPanel.Zoom = 1.0;
        }

        private void UpdateUndoRedo()
        {
            editUndoMenuItem.Enabled = url.CanUndo;
            editRedoMenuItem.Enabled = url.CanRedo;
            editClearUndoRedoMenuItem.Enabled = url.CanUndo || url.CanRedo;
            // Some action has occurred; probably something was placed or removed. Force-refresh current cell.
            UpdateCellStatusLabel(true);
        }

        private void UpdateCellStatusLabel(bool force)
        {
            if (plugin == null || activeTool == null || activeTool.NavigationWidget == null)
            {
                return;
            }
            Point location = activeTool.NavigationWidget.ActualMouseCell;
            Point subPixel = activeTool.NavigationWidget.MouseSubPixel;
            if (force)
            {
                activeTool.NavigationWidget.GetMouseCellPosition(location, out subPixel);
            }
            if (force || location != lastInfoPoint || subPixel != lastInfoSubPixelPoint)
            {
                String description = plugin.Map.GetCellDescription(location, subPixel);
                if (force || lastDescription != description)
                {
                    lastInfoPoint = location;
                    lastInfoSubPixelPoint = subPixel;
                    lastDescription = description;
                    cellStatusLabel.Text = description;
                }
            }
        }

        private void UndoRedo_Tracked(object sender, EventArgs e)
        {
            UpdateUndoRedo();
        }

        private void UndoRedo_Updated(object sender, UndoRedoEventArgs e)
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
            mru.Dispose();
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
            List<string> filters = new List<string>();
            filters.Add("All supported types (*.ini;*.bin;*.mpr;*.pgm)|*.ini;*.bin;*.mpr;*.pgm");
            filters.Add(TiberianDawn.Constants.FileFilter);
            filters.Add(RedAlert.Constants.FileFilter);
            filters.Add("PGM files (*.pgm)|*.pgm");
            filters.Add("All files (*.*)|*.*");
            string selectedFileName = null;
            ClearActiveTool();
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.AutoUpgradeEnabled = false;
                ofd.RestoreDirectory = true;
                ofd.Filter = String.Join("|", filters);
                bool classicLogic = Globals.UseClassicFiles && Globals.ClassicNoRemasterLogic;
                string lastFolder = mru.Files.Select(f => f.DirectoryName).Where(d => Directory.Exists(d)).FirstOrDefault();
                if (plugin != null)
                {
                    string openFolder = Path.GetDirectoryName(filename);
                    string defFolder = plugin.GameInfo.DefaultSaveDirectory;
                    string constFolder = Directory.Exists(defFolder) ? defFolder : Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    ofd.InitialDirectory = openFolder ?? lastFolder ?? (classicLogic ? Program.ApplicationPath : constFolder);
                }
                else
                {
                    ofd.InitialDirectory = lastFolder ?? (classicLogic ? Program.ApplicationPath : Globals.RootSaveDirectory);
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
            else
            {
                RefreshActiveTool();
            }
        }

        private void FileOpenFromMixMenuItem_Click(object sender, EventArgs e)
        {
            PromptSaveMap(OpenFileFromMix, false);
        }

        private void OpenFileFromMix()
        {
            // TODO make mix browsing ui
        }

        private void FileSaveMenuItem_Click(object sender, EventArgs e)
        {
            SaveAction(false, null, false, false);
        }

        /// <summary>
        /// Performs the saving action, and optionally executes another action after the save is done.
        /// </summary>
        /// <param name="dontResavePreview">Suppress generation of the preview image.</param>
        /// <param name="afterSaveDone">Action to execute after the save is done.</param>
        /// <param name="skipValidation">True to skip validation when saving.</param>
        /// <param name="continueOnError">True to execute <paramref name="afterSaveDone"/> even if errors occurred.</param>
        private void SaveAction(bool dontResavePreview, Action afterSaveDone, bool skipValidation, bool continueOnError)
        {
            if (plugin == null)
            {
                afterSaveDone?.Invoke();
                return;
            }
            if (string.IsNullOrEmpty(filename) || !Directory.Exists(Path.GetDirectoryName(filename)))
            {
                SaveAsAction(afterSaveDone, skipValidation);
                return;
            }
            if (!this.DoValidate())
            {
                if (continueOnError)
                {
                    afterSaveDone();
                }
                return;
            }
            var fileInfo = new FileInfo(filename);
            SaveChosenFile(fileInfo.FullName, loadedFileType, dontResavePreview, afterSaveDone);
        }

        private void FileSaveAsMenuItem_Click(object sender, EventArgs e)
        {
            SaveAsAction(null, false);
        }

        private void SaveAsAction(Action afterSaveDone, bool skipValidation)
        {
            if (plugin == null)
            {
                afterSaveDone?.Invoke();
                return;
            }
            if (!skipValidation && !this.DoValidate())
            {
                return;
            }
            ClearActiveTool();
            string savePath = null;
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.AutoUpgradeEnabled = false;
                sfd.RestoreDirectory = false;
                bool classicLogic = Globals.UseClassicFiles && Globals.ClassicNoRemasterLogic;
                string lastFolder = mru.Files.Select(f => f.DirectoryName).Where(d => Directory.Exists(d)).FirstOrDefault();
                string openFolder = Path.GetDirectoryName(filename);
                string defFolder = plugin.GameInfo.DefaultSaveDirectory;
                string constFolder = Directory.Exists(defFolder) ? defFolder : Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var filters = new List<string>();
                filters.Add(plugin.GameInfo.SaveFilter);
                filters.Add("All files (*.*)|*.*");
                sfd.InitialDirectory = openFolder ?? lastFolder ?? (classicLogic ? Program.ApplicationPath : constFolder);
                sfd.Filter = string.Join("|", filters);
                if (!string.IsNullOrEmpty(filename))
                {
                    sfd.FileName = Path.GetFileName(filename);
                }
                else if (!plugin.GameInfo.MapNameIsEmpty(plugin.Map.BasicSection.Name))
                {
                    sfd.FileName = Path.GetFileName(plugin.Map.BasicSection.Name);
                }
                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    savePath = sfd.FileName;
                }
            }
            if (savePath == null)
            {
                if (afterSaveDone != null)
                {
                    afterSaveDone.Invoke();
                }
                else
                {
                    RefreshActiveTool();
                }
            }
            else
            {
                var fileInfo = new FileInfo(savePath);
                SaveChosenFile(fileInfo.FullName, FileType.INI, false, afterSaveDone);
            }
        }

        private bool DoValidate()
        {
            String errors = plugin.Validate(true);
            if (!String.IsNullOrEmpty(errors))
            {
                String message = errors + "\n\nContinue map save?";
                DialogResult dr = SimpleMultiThreading.ShowMessageBoxThreadSafe(this, message, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.No)
                {
                    return false;
                }
            }
            errors = plugin.Validate(false);
            if (errors != null)
            {
                SimpleMultiThreading.ShowMessageBoxThreadSafe(this, errors, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
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
            if (activeTool == null || activeTool.IsBusy)
            {
                return;
            }
            if (url.CanUndo)
            {
                url.Undo(new UndoRedoEventArgs(mapPanel, plugin));
            }
        }

        private void EditRedoMenuItem_Click(object sender, EventArgs e)
        {
            if (activeTool == null || activeTool.IsBusy)
            {
                return;
            }
            if (url.CanRedo)
            {
                url.Redo(new UndoRedoEventArgs(mapPanel, plugin));
            }
        }

        private void EditClearUndoRedoMenuItem_Click(object sender, EventArgs e)
        {
            ClearActiveTool();
            if (DialogResult.Yes == MessageBox.Show("This will remove all undo/redo information. Are you sure?", Program.ProgramVersionTitle, MessageBoxButtons.YesNo))
            {
                url.Clear();
            }
            RefreshActiveTool();
        }

        private void SettingsMapSettingsMenuItem_Click(object sender, EventArgs e)
        {
            if (plugin == null)
            {
                return;
            }
            bool wasSolo = plugin.Map.BasicSection.SoloMission;
            bool wasExpanded = plugin.Map.BasicSection.ExpansionEnabled;
            PropertyTracker<BasicSection> basicSettings = new PropertyTracker<BasicSection>(plugin.Map.BasicSection);
            PropertyTracker<BriefingSection> briefingSettings = new PropertyTracker<BriefingSection>(plugin.Map.BriefingSection);
            PropertyTracker<SoleSurvivor.CratesSection> cratesSettings = null;
            if (plugin.GameInfo.GameType == GameType.SoleSurvivor && plugin is SoleSurvivor.GamePluginSS ssPlugin)
            {
                cratesSettings = new PropertyTracker<SoleSurvivor.CratesSection>(ssPlugin.CratesSection);
            }
            string extraIniText = plugin.GetExtraIniText();
            if (extraIniText.Trim('\r', '\n').Length == 0)
                extraIniText = String.Empty;
            Dictionary<House, PropertyTracker<House>> houseSettingsTrackers = plugin.Map.Houses.ToDictionary(h => h, h => new PropertyTracker<House>(h));
            bool amStatusChanged = false;
            bool multiStatusChanged = false;
            bool iniTextChanged = false;
            bool footPrintsChanged = false;
            ClearActiveTool();
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
                    amStatusChanged = wasExpanded != plugin.Map.BasicSection.ExpansionEnabled;
                    multiStatusChanged = wasSolo != plugin.Map.BasicSection.SoloMission;
                    iniTextChanged = !checkTextOrig.Equals(checkTextNew, StringComparison.OrdinalIgnoreCase);
                    // All three of those warrant a rules reset.
                    // TODO: give warning on the multiplay rules changes.
                    if (amStatusChanged || multiStatusChanged || iniTextChanged)
                    {
                        IEnumerable<string> errors = plugin.SetExtraIniText(normalised, out footPrintsChanged);
                        if (errors != null && errors.Count() > 0)
                        {
                            using (ErrorMessageBox emb = new ErrorMessageBox())
                            {
                                emb.Title = Program.ProgramVersionTitle;
                                emb.Message = "Errors occurred when applying rule changes:";
                                emb.Errors = errors;
                                emb.StartPosition = FormStartPosition.CenterParent;
                                emb.ShowDialog(this);
                            }
                        }
                        // Maybe make more advanced logic to check if any bibs changed, and don't clear if not needed?
                        hasChanges = true;
                    }
                    plugin.Dirty = hasChanges;
                }
            }
            RefreshActiveTool();
            if (footPrintsChanged || amStatusChanged)
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
            ClearActiveTool();
            using (TeamTypesDialog ttd = new TeamTypesDialog(plugin))
            {
                ttd.StartPosition = FormStartPosition.CenterParent;
                if (ttd.ShowDialog(this) == DialogResult.OK)
                {
                    List<TeamType> oldTeamTypes = plugin.Map.TeamTypes.ToList();
                    // Clone of old triggers
                    List<Trigger> oldTriggers = plugin.Map.Triggers.Select(tr => tr.Clone()).ToList();
                    plugin.Map.TeamTypes.Clear();
                    plugin.Map.ApplyTeamTypeRenames(ttd.RenameActions);
                    // Triggers in their new state after the teamtype item renames.
                    List<Trigger> newTriggers = plugin.Map.Triggers.Select(tr => tr.Clone()).ToList();
                    plugin.Map.TeamTypes.AddRange(ttd.TeamTypes.OrderBy(t => t.Name, new ExplorerComparer()).Select(t => t.Clone()));
                    List<TeamType> newTeamTypes = plugin.Map.TeamTypes.ToList();
                    bool origDirtyState = plugin.Dirty;
                    void undoAction(UndoRedoEventArgs ev)
                    {
                        ClearActiveTool();
                        DialogResult dr = MessageBox.Show(this, "This will undo all teamtype editing actions you performed. Are you sure you want to continue?",
                            "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (dr == DialogResult.No)
                        {
                            ev.Cancelled = true;
                        }
                        else if (ev.Plugin != null)
                        {
                            ev.Map.Triggers = oldTriggers;
                            ev.Map.TeamTypes.Clear();
                            ev.Map.TeamTypes.AddRange(oldTeamTypes);
                            ev.Plugin.Dirty = origDirtyState;
                        }
                        RefreshActiveTool();
                    }
                    void redoAction(UndoRedoEventArgs ev)
                    {
                        ClearActiveTool();
                        DialogResult dr = MessageBox.Show(this, "This will redo all teamtype editing actions you undid. Are you sure you want to continue?",
                            "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (dr == DialogResult.No)
                        {
                            ev.Cancelled = true;
                        }
                        else if (ev.Plugin != null)
                        {
                            ev.Map.TeamTypes.Clear();
                            ev.Map.TeamTypes.AddRange(newTeamTypes);
                            ev.Map.Triggers = newTriggers;
                            ev.Plugin.Dirty = true;
                        }
                        RefreshActiveTool();
                    }
                    url.Track(undoAction, redoAction, ToolType.None);
                    plugin.Dirty = true;
                }
            }
            RefreshActiveTool();
        }

        private void SettingsTriggersMenuItem_Click(object sender, EventArgs e)
        {
            if (plugin == null)
            {
                return;
            }
            ClearActiveTool();
            using (TriggersDialog td = new TriggersDialog(plugin))
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
                            ClearActiveTool();
                            DialogResult dr = MessageBox.Show(this, "This will undo all trigger editing actions you performed. Are you sure you want to continue?",
                                "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            if (dr == DialogResult.No)
                            {
                                ev.Cancelled = true;
                                RefreshActiveTool();
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
                            RefreshActiveTool();
                        }
                        void redoAction(UndoRedoEventArgs ev)
                        {
                            ClearActiveTool();
                            DialogResult dr = MessageBox.Show(this, "This will redo all trigger editing actions you undid. Are you sure you want to continue?",
                                "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            if (dr == DialogResult.No)
                            {
                                ev.Cancelled = true;
                                RefreshActiveTool();
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
                            RefreshActiveTool();
                        }
                        // These changes can affect a whole lot of tools.
                        url.Track(undoAction, redoAction, ToolType.Terrain | ToolType.Infantry | ToolType.Unit | ToolType.Building | ToolType.CellTrigger);
                        // No longer a full refresh, since celltriggers function is no longer disabled when no triggers are found.
                        mapPanel.Invalidate();
                    }
                }
            }
            RefreshActiveTool();
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
            ClearActiveTool();
            using (ErrorMessageBox emb = new ErrorMessageBox())
            {
                emb.Title = "Map objects";
                emb.Message = "Map objects overview:";
                emb.Errors = plugin.AssessMapItems();
                emb.StartPosition = FormStartPosition.CenterParent;
                emb.ShowDialog(this);
            }
            RefreshActiveTool();
        }

        private void ToolsStatsPowerMenuItem_Click(Object sender, EventArgs e)
        {
            if (plugin == null)
            {
                return;
            }
            ClearActiveTool();
            using (ErrorMessageBox emb = new ErrorMessageBox())
            {
                emb.Title = "Power usage";
                emb.Message = "Power balance per House:";
                emb.Errors = plugin.Map.AssessPower(plugin.GetHousesWithProduction());
                emb.StartPosition = FormStartPosition.CenterParent;
                emb.ShowDialog(this);
            }
            RefreshActiveTool();
        }

        private void ToolsStatsStorageMenuItem_Click(Object sender, EventArgs e)
        {
            if (plugin == null)
            {
                return;
            }
            ClearActiveTool();
            using (ErrorMessageBox emb = new ErrorMessageBox())
            {
                emb.Title = "Silo storage";
                emb.Message = "Available silo storage per House:";
                emb.Errors = plugin.Map.AssessStorage(plugin.GetHousesWithProduction());
                emb.StartPosition = FormStartPosition.CenterParent;
                emb.ShowDialog(this);
            }
            RefreshActiveTool();
        }

        private void ToolsRandomizeTilesMenuItem_Click(Object sender, EventArgs e)
        {
            if (plugin != null)
            {
                ClearActiveTool();
                String feedback = TemplateTool.RandomizeTiles(plugin, mapPanel, url);
                MessageBox.Show(feedback, Program.ProgramVersionTitle);
                RefreshActiveTool();
            }
        }

        private void ToolsExportImage_Click(Object sender, EventArgs e)
        {
            if (plugin == null)
            {
                return;
            }
            ClearActiveTool();
            string lastFolder = mru.Files.Select(f => f.DirectoryName).Where(d => Directory.Exists(d)).FirstOrDefault();
            using (ImageExportDialog imex = new ImageExportDialog(plugin, activeLayers, filename, lastFolder))
            {
                imex.StartPosition = FormStartPosition.CenterParent;
                imex.ShowDialog(this);
            }
            RefreshActiveTool();
        }

        private void ViewZoomInMenuItem_Click(Object sender, EventArgs e)
        {
            ZoomIn();
        }

        private void ViewZoomOutMenuItem_Click(Object sender, EventArgs e)
        {
            ZoomOut();
        }

        private void ViewZoomResetMenuItem_Click(Object sender, EventArgs e)
        {
            ZoomReset();
        }

        private void ViewZoomBoundsMenuItem_Click(Object sender, EventArgs e)
        {
            lock (jumpToBounds_lock)
            {
                this.jumpToBounds = true;
            }
            mapPanel.Refresh();
        }

        private void Mru_FileSelected(object sender, FileInfo e)
        {
            if (File.Exists(e.FullName))
            {
                OpenFileAsk(e.FullName);
            }
            else
            {
                ClearActiveTool();
                MessageBox.Show(string.Format("Error loading {0}: the file was not found.", e.Name), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                mru.Remove(e);
                RefreshActiveTool();
            }
        }

        private void ViewTool_RequestMouseInfoRefresh(object sender, EventArgs e)
        {
            // Viewtool has asked a deliberate refresh; probably the map position jumped without the mouse moving.
            UpdateCellStatusLabel(true);
        }

        private void MapPanel_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateCellStatusLabel(false);
        }

        #endregion

        #region Additional logic for listeners

        private void NewFileAsk(bool withImage, string imagePath, bool skipPrompt)
        {
            if (skipPrompt)
            {
                NewFile(withImage, imagePath);
            }
            else
            {
                PromptSaveMap(() => NewFile(withImage, imagePath), false);
            }
        }

        private void NewFile(bool withImage, string imagePath)
        {
            GameType gameType = GameType.None;
            string theater = null;
            bool isTdMegaMap = false;
            bool isSinglePlay = false;
            using (NewMapDialog nmd = new NewMapDialog(withImage))
            {
                nmd.StartPosition = FormStartPosition.CenterParent;
                if (nmd.ShowDialog(this) != DialogResult.OK)
                {
                    RefreshActiveTool();
                    return;
                }
                gameType = nmd.GameType;
                isTdMegaMap = nmd.MegaMap;
                isSinglePlay = nmd.SinglePlayer;
                theater = nmd.Theater;
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
                        RefreshActiveTool();
                        return;
                    }
                    imagePath = ofd.FileName;
                }
            }
            Unload();
            String loading = "Loading new map";
            if (withImage)
                loading += " from image";
            loadMultiThreader.ExecuteThreaded(
                () => NewFile(gameType, imagePath, theater, isTdMegaMap, isSinglePlay, this),
                PostLoad, true,
                (e, l) => LoadUnloadUi(e, l, loadMultiThreader),
                loading);
        }

        private void OpenFileAsk(String fileName)
        {
            PromptSaveMap(() => OpenFile(fileName), false);
        }

        private void OpenFile(String fileName)
        {
            ClearActiveTool();
            var fileInfo = new FileInfo(fileName);
            String name = fileInfo.FullName;
            if (!IdentifyMap(name, out FileType fileType, out GameType gameType, out bool isMegaMap, out string theater))
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
                MessageBox.Show(string.Format("Error loading {0}: Could not identify map type.", fileInfo.Name), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                RefreshActiveTool();
                return;
            }
            GameInfo gType = GameTypeFactory.GetGameInfo(gameType);
            TheaterType[] theaters = gType != null ? gType.AllTheaters : null;
            TheaterType theaterObj = theaters == null ? null : theaters.Where(th => th.Name.Equals(theater, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (theaterObj == null)
            {
                MessageBox.Show(String.Format("Unknown {0} theater \"{1}\"", gType.Name, theater), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                RefreshActiveTool();
                return;
            }
            if (!theaterObj.IsAvailable())
            {
                string graphicsMode = Globals.UseClassicFiles ? "Classic" : "Remastered";
                string message = string.Format("Error loading {0}: No assets found for {1} theater \"{2}\" in {3} graphics mode.",
                    fileInfo.Name, gType.Name, theaterObj.Name, graphicsMode);
                if (Globals.UseClassicFiles)
                {
                    message += String.Format("\n\nYou may need to adjust the \"{0}\" setting to point to a game folder containing {1}, or add {1} to the configured folder.",
                        gType.ClassicFolderSetting, theaterObj.ClassicTileset + ".mix");
                }
                else
                {
                    message += "\n\nYou may need to switch to Classic graphics mode by enabling the \"UseClassicFiles\" setting to use this theater.";
                }
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                RefreshActiveTool();
                return;
            }
            loadMultiThreader.ExecuteThreaded(
                () => LoadFile(name, fileType, gType, theater, isMegaMap),
                PostLoad, true,
                (e, l) => LoadUnloadUi(e, l, loadMultiThreader),
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
            if (plugin.GameInfo.MapNameIsEmpty(plugin.Map.BasicSection.Name))
            {
                plugin.Map.BasicSection.Name = Path.GetFileNameWithoutExtension(saveFilename);
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

        private Boolean IdentifyMap(String loadFilename, out FileType fileType, out GameType gameType, out bool isMegaMap, out string theater)
        {
            fileType = FileType.None;
            gameType = GameType.None;
            theater = null;
            isMegaMap = false;
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
            theater = (iniContents["Map"].TryGetValue("Theater") ?? String.Empty).ToLower();
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
                isMegaMap = TiberianDawn.GamePluginTD.CheckForMegamap(iniContents);
                if (SoleSurvivor.GamePluginSS.CheckForSSmap(iniContents))
                {
                    gameType = GameType.SoleSurvivor;
                }
            }
            else if (gameType == GameType.RedAlert)
            {
                // Not actually used for RA at the moment.
                isMegaMap = true;
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
            InfoCheckForUpdatesMenuItem.Enabled = enableUI;
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
            InfoCheckForUpdatesMenuItem.Enabled = enableUI;
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

        private static IGamePlugin LoadNewPlugin(GameInfo gameInfo, string theater, bool isMegaMap)
        {
            return LoadNewPlugin(gameInfo, theater, isMegaMap, false);
        }

        private static IGamePlugin LoadNewPlugin(GameInfo gameInfo, string theater, bool isMegaMap, bool noImage)
        {
            GameType gameType = gameInfo.GameType;
            // Get plugin type
            IGamePlugin plugin = gameInfo.CreatePlugin(!noImage, isMegaMap);
            // Get theater object
            TheaterTypeConverter ttc = new TheaterTypeConverter();
            TheaterType theaterType = ttc.ConvertFrom(new MapContext(plugin.Map, false), theater);
            // Resetting to a specific game type will take care of classic mode.
            Globals.TheArchiveManager.Reset(gameType, theaterType);
            Globals.TheGameTextManager.Reset(gameType);
            Globals.TheTilesetManager.Reset(gameType, theaterType);
            Globals.TheTeamColorManager.Reset(gameType, theaterType);
            // Load game-specific data
            plugin.Initialize();
            // Needs to be done after the whole init, so colors reading is properly initialised.
            plugin.Map.FlagColors = plugin.GetFlagColors();
            return plugin;
        }

        /// <summary>
        /// The separate-threaded part for making a new map.
        /// </summary>
        /// <param name="gameType">Game type</param>
        /// <param name="imagePath">Image path, indicating the map is being created from image</param>
        /// <param name="theater">Theater of the new map</param>
        /// <param name="isTdMegaMap">Is megamap</param>
        /// <param name="isSinglePlay">Is singleplayer scenario</param>
        /// <param name="showTarget">The form to use as target for showing messages / dialogs on.</param>
        /// <returns></returns>
        private static MapLoadInfo NewFile(GameType gameType, String imagePath, string theater, bool isTdMegaMap, bool isSinglePlay, MainForm showTarget)
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
            IGamePlugin plugin = null;
            bool mapLoaded = false;
            try
            {
                GameInfo gType = GameTypeFactory.GetGameInfo(gameType);
                plugin = LoadNewPlugin(gType, theater, isTdMegaMap);
                // This initialises the theater
                plugin.New(theater);
                mapLoaded = true;
                plugin.Map.BasicSection.SoloMission = isSinglePlay;
                if (SteamworksUGC.IsInit)
                {
                    try
                    {
                        plugin.Map.BasicSection.Author = SteamFriends.GetPersonaName();
                    }
                    catch { /* ignore */ }
                }
                if (imageData != null)
                {
                    Dictionary<int, string> types = (Dictionary<int, string>)showTarget.Invoke(
                        new Func<Dictionary<int, string>>(() => ShowNewFromImageDialog(plugin, imageWidth, imageHeight, imageData, showTarget)));
                    if (types == null)
                    {
                        return null;
                    }
                    plugin.Map.SetMapTemplatesRaw(imageData, imageWidth, imageHeight, types, null);
                }
                return new MapLoadInfo(null, FileType.None, plugin, null, true);
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
                return new MapLoadInfo(null, FileType.None, plugin, errorMessage.ToArray(), mapLoaded);
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
            {
                ExplorerComparer expl = new ExplorerComparer();
                TheaterType theater = plugin.Map.Theater;
                TemplateType tt = plugin.Map.TemplateTypes.Where(t => t.ExistsInTheater
                    //&& (!Globals.FilterTheaterObjects || t.Theaters == null || t.Theaters.Length == 0 || t.Theaters.Contains(plugin.Map.Theater.Name))
                    && (t.Flag & TemplateTypeFlag.Clear) == TemplateTypeFlag.DefaultFill
                    && (t.Flag & TemplateTypeFlag.IsGrouped) == TemplateTypeFlag.None)
                .OrderBy(t => t.Name, expl).FirstOrDefault();
                if (tt != null)
                {
                    mappings.Add(mostCommon[1].ToArgb(), tt.Name + ":0");
                }
            }
            using (NewFromImageDialog nfi = new NewFromImageDialog(plugin, imageWidth, imageHeight, imageData, mappings))
            {
                nfi.StartPosition = FormStartPosition.CenterParent;
                if (nfi.ShowDialog(showTarget) == DialogResult.Cancel)
                {
                    return null;
                }
                return nfi.Mappings;
            }
        }

        /// <summary>
        /// The separate-threaded part for loading a map.
        /// </summary>
        /// <param name="loadFilename">File to load.</param>
        /// <param name="fileType">Type of the loaded file (detected in advance).</param>
        /// <param name="gameInfo">Game type info (detected in advance)</param>
        /// <param name="isMegaMap">True if this is a megamap.</param>
        /// <returns></returns>
        private static MapLoadInfo LoadFile(string loadFilename, FileType fileType, GameInfo gameInfo, string theater, bool isMegaMap)
        {
            IGamePlugin plugin = null;
            bool mapLoaded = false;
            try
            {
                plugin = LoadNewPlugin(gameInfo, theater, isMegaMap);
                string[] errors = plugin.Load(loadFilename, fileType).ToArray();
                mapLoaded = true;
                return new MapLoadInfo(loadFilename, fileType, plugin, errors, true);
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
                return new MapLoadInfo(loadFilename, fileType, plugin, errorMessage.ToArray(), mapLoaded);
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
                string errorMessage = "Error saving map: " + ex.Message;
                errorMessage += "\n\n" + ex.StackTrace;
                return (saveFilename, false, errorMessage);
            }
        }

        private void PostLoad(MapLoadInfo loadInfo)
        {
            if (loadInfo == null)
            {
                // Absolute abort
                SimpleMultiThreading.RemoveBusyLabel(this);
                RefreshActiveTool();
                if (this.shouldCheckUpdate)
                {
                    CheckForUpdates(true);
                }
                return;
            }
            IGamePlugin oldPlugin = this.plugin;
            string[] errors = loadInfo.Errors ?? new string[0];
            // Plugin set to null indicates a fatal processing error where no map was loaded at all.
            if (loadInfo.Plugin == null || (loadInfo.Plugin != null && !loadInfo.MapLoaded))
            {
                // Attempted to load file, loading went OK, but map was not loaded.
#if !DEBUG
                if (loadInfo.FileName != null && loadInfo.Plugin != null && !loadInfo.MapLoaded)
                {
                    var fileInfo = new FileInfo(loadInfo.FileName);
                    mru.Remove(fileInfo);
                }
#endif
                // In case of actual error, remove label.
                SimpleMultiThreading.RemoveBusyLabel(this);
                MessageBox.Show(string.Format("Error loading {0}: {1}", loadInfo.FileName ?? "new map", String.Join("\n", errors)), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                RefreshActiveTool();
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
                    loadInfo.FileType = FileType.INI;
                    loadInfo.FileName = null;
                }
#endif
                mapPanel.MapImage = plugin.MapImage;
                filename = loadInfo.FileName;
                loadedFileType = loadInfo.FileType;
                if (Globals.ZoomToBoundsOnLoad)
                {
                    lock (jumpToBounds_lock)
                    {
                        this.jumpToBounds = true;
                    }
                }
                else
                {
                    ZoomReset();
                }
                url.Clear();
                CleanupTools(oldPlugin?.GameInfo?.GameType ?? GameType.None);
                RefreshUI(oldSelectedTool);
                oldSelectedTool = ToolType.None;
                //RefreshActiveTool(); // done by UI refresh
                SetTitle();
                if (loadInfo.FileName != null)
                {
                    var fileInfo = new FileInfo(loadInfo.FileName);
                    mru.Add(fileInfo);
                }
            }
            if (this.shouldCheckUpdate)
            {
                CheckForUpdates(true);
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
                if (afterSaveDone != null)
                {
                    afterSaveDone.Invoke();
                }
                else
                {
                    RefreshActiveTool();
                }
            }
            else
            {
                MessageBox.Show(string.Format("Error saving {0}: {1}", saveInfo.FileName, saveInfo.Error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error));
                //mru.Remove(fileInfo);
                RefreshActiveTool();
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
                if (ActiveToolType != ToolType.None)
                {
                    oldSelectedTool = ActiveToolType;
                }
                ActiveToolType = ToolType.None; // Always re-defaults to map anyway, so nicer if nothing is selected during load.
                this.ActiveControl = null;
                CleanupTools(plugin?.GameInfo?.GameType ?? GameType.None);
                // Unlink plugin
                IGamePlugin pl = plugin;
                plugin = null;
                // Clean up UI caching
                this.lastInfoPoint = new Point(-1, -1);
                this.lastInfoSubPixelPoint = new Point(-1, -1);
                this.lastDescription = null;
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
                Globals.TheTilesetManager.Reset(GameType.None, null);
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
                string th = plugin.Map.Theater.Name;
                availableToolTypes |= ToolType.Map; // Should always show clear terrain, no matter what.
                availableToolTypes |= plugin.Map.SmudgeTypes.Any(t => !Globals.FilterTheaterObjects || t.ExistsInTheater) ? ToolType.Smudge : ToolType.None;
                availableToolTypes |= plugin.Map.OverlayTypes.Any(t => t.IsOverlay && (!Globals.FilterTheaterObjects || t.ExistsInTheater)) ? ToolType.Overlay : ToolType.None;
                availableToolTypes |= plugin.Map.TerrainTypes.Any(t => !Globals.FilterTheaterObjects || t.Theaters == null || t.Theaters.Contains(th)) ? ToolType.Terrain : ToolType.None;
                availableToolTypes |= plugin.Map.InfantryTypes.Any() ? ToolType.Infantry : ToolType.None;
                availableToolTypes |= plugin.Map.UnitTypes.Any() ? ToolType.Unit : ToolType.None;
                availableToolTypes |= plugin.Map.BuildingTypes.Any(t => !Globals.FilterTheaterObjects || !t.IsTheaterDependent || t.ExistsInTheater) ? ToolType.Building : ToolType.None;
                availableToolTypes |= plugin.Map.OverlayTypes.Any(t => t.IsResource && (!Globals.FilterTheaterObjects || t.ExistsInTheater)) ? ToolType.Resources : ToolType.None;
                availableToolTypes |= plugin.Map.OverlayTypes.Any(t => t.IsWall && (!Globals.FilterTheaterObjects || t.ExistsInTheater)) ? ToolType.Wall : ToolType.None;
                // Waypoints are always available.
                availableToolTypes |= ToolType.Waypoint;
                // Always allow celltrigger tool, even if triggers list is empty; it contains a tooltip saying which trigger types are eligible.
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
            viewIndicatorsBuildingFakeLabelsMenuItem.Visible = !hasPlugin || plugin.GameInfo.SupportsMapLayer(MapLayerFlag.BuildingFakes);
            viewExtraIndicatorsEffectAreaRadiusMenuItem.Visible = !hasPlugin || plugin.GameInfo.SupportsMapLayer(MapLayerFlag.EffectRadius);
            viewLayersBuildingsMenuItem.Visible = !hasPlugin || plugin.GameInfo.SupportsMapLayer(MapLayerFlag.Buildings);
            viewLayersUnitsMenuItem.Visible = !hasPlugin || plugin.GameInfo.SupportsMapLayer(MapLayerFlag.Units);
            viewLayersInfantryMenuItem.Visible = !hasPlugin || plugin.GameInfo.SupportsMapLayer(MapLayerFlag.Infantry);
            viewIndicatorsBuildingRebuildLabelsMenuItem.Visible = !hasPlugin || plugin.GameInfo.SupportsMapLayer(MapLayerFlag.BuildingRebuild);
            viewIndicatorsFootballAreaMenuItem.Visible = !hasPlugin || plugin.GameInfo.SupportsMapLayer(MapLayerFlag.FootballArea);
            viewIndicatorsOutlinesMenuItem.Visible = !hasPlugin || plugin.GameInfo.SupportsMapLayer(MapLayerFlag.OverlapOutlines);
        }

        private void CleanupTools(GameType gameType)
        {
            // Tools
            ClearActiveTool();
            if (oldMockObjects != null && gameType != GameType.None && toolForms.Count > 0)
            {
                oldMockGame = gameType;
            }
            foreach (var kvp in toolForms)
            {
                ITool tool;
                Object obj;
                if (oldMockObjects != null && gameType != GameType.None && kvp.Value != null && (tool = kvp.Value.GetTool()) != null && (obj = tool.CurrentObject) != null)
                {
                    oldMockObjects.Add((kvp.Key), obj);
                }
                kvp.Value.Dispose();
            }
            toolForms.Clear();
        }

        private void ClearActiveTool()
        {
            if (activeTool != null)
            {
                activeTool.RequestMouseInfoRefresh -= ViewTool_RequestMouseInfoRefresh;
                activeTool.Deactivate();
            }
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
            ToolType curType = ActiveToolType;
            bool found = toolForms.TryGetValue(curType, out IToolDialog toolDialog);
            if (!found || (toolDialog is Form toolFrm && toolFrm.IsDisposed))
            {
                switch (curType)
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
                    toolForms[curType] = toolDialog;
                }
            }
            MapLayerFlag active = ActiveLayers;
            if (toolDialog != null)
            {
                activeToolForm = (Form)toolDialog;
                ITool oldTool = toolDialog.GetTool();
                Object mockObject = null;
                bool fromBackup = false;
                if (oldMockGame != this.plugin.GameInfo.GameType && oldMockObjects != null && oldMockObjects.Count() > 0)
                {
                    oldMockObjects.Clear();
                    oldMockGame = GameType.None;
                }
                if (oldTool != null && oldTool.Plugin == plugin)
                {
                    // Same map edit session; restore old data
                    mockObject = oldTool.CurrentObject;
                }
                else if (oldMockGame == this.plugin.GameInfo.GameType && oldMockObjects != null && oldMockObjects.TryGetValue(curType, out object mock))
                {
                    mockObject = mock;
                    // Retrieve once and remove.
                    oldMockObjects.Remove(curType);
                    fromBackup = true;
                }
                // Creates the actual Tool class
                toolDialog.Initialize(mapPanel, active, toolStatusLabel, mouseToolTip, plugin, url);
                activeTool = toolDialog.GetTool();
                // If an active House is set, and the current tool has a techno type, copy it out so its house can be adjusted.
                if (plugin.ActiveHouse != null && mockObject == null && activeTool.CurrentObject is ITechno)
                {
                    mockObject = activeTool.CurrentObject;
                }
                // If an active House is set, and the mock object is an ITechno, adjust its house (regardless of source)
                if (plugin.ActiveHouse != null && mockObject is ITechno techno)
                {
                    techno.House = plugin.ActiveHouse;
                }
                if (fromBackup && mockObject is ITechno trtechno)
                {
                    // Do not inherit trigger names from a different session.
                    trtechno.Trigger = Trigger.None;
                }
                // Sets backed up / adjusted object in current tool.
                if (mockObject != null)
                {
                    activeTool.CurrentObject = mockObject;
                }
                // Allow the tool to refresh the cell info under the mouse cursor.
                activeTool.RequestMouseInfoRefresh += ViewTool_RequestMouseInfoRefresh;
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
                toolStripButton.Checked = curType == toolStripButton.ToolType;
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
            if (!viewExtraIndicatorsMapPassabilityMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.LandTypes;
            }
            if (!viewIndicatorsOutlinesMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.OverlapOutlines;
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
            OpenFileAsk(files[0]);
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
                viewIndicatorsOutlinesMenuItem.Checked = enabled;
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
            if (!SteamworksUGC.IsInit)
            {
                MessageBox.Show("Steam interface is not initialized. To enable Workshop publishing, log into Steam and restart the editor.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (plugin.GameInfo.WorkshopTypeId == null)
            {
                MessageBox.Show(plugin.GameInfo.Name + " maps cannot be published to the Steam Workshop; they are not usable by the C&C Remastered Collection.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (plugin.Map.Theater.IsModTheater)
            {
                if (!plugin.Map.BasicSection.SoloMission)
                {
                    MessageBox.Show("Multiplayer maps with nonstandard theaters cannot be published to the Steam Workshop;" +
                        " they are not usable by the C&C Remastered Collection without modding, and may cause issues on the official servers.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // If the mission is already published on Steam, don't bother asking this and just continue.
                if (plugin.Map.SteamSection.PublishedFileId == 0
                        && DialogResult.Yes != MessageBox.Show("This map uses a nonstandard theater that is not usable by the C&C Remastered Collection without modding!" +
                        " Are you sure you want to publish a mission that will be incompatible with the standard unmodded game?", "Warning",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
                {
                    return;
                }
            }
            if (plugin.IsMegaMap && !plugin.GameInfo.MegamapOfficial)
            {
                if (!plugin.Map.BasicSection.SoloMission)
                {
                    MessageBox.Show(plugin.GameInfo.Name + " multiplayer megamaps cannot be published to the Steam Workshop;" +
                        " they are not usable by the C&C Remastered Collection without modding, and may cause issues on the official servers.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // If the mission is already published on Steam, don't bother asking this and just continue.
                if (plugin.Map.SteamSection.PublishedFileId == 0
                        && DialogResult.Yes != MessageBox.Show("Megamaps are not supported by " + plugin.GameInfo.Name + " Remastered without modding!" +
                        " Are you sure you want to publish a mission that will be incompatible with the standard unmodded game?", "Warning",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
                {
                    return;
                }
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
            ClearActiveTool();
            // Check if we need to save.
            ulong oldId = plugin.Map.SteamSection.PublishedFileId;
            string oldName = plugin.Map.SteamSection.Title;
            string oldDescription = plugin.Map.SteamSection.Description;
            string oldPreview = plugin.Map.SteamSection.PreviewFile;
            string oldVisibility = plugin.Map.SteamSection.Visibility;
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
                || oldVisibility != plugin.Map.SteamSection.Visibility))
            {
                // This takes care of saving the Steam info into the map.
                // This specific overload only saves the map, without resaving the preview.
                SaveAction(true, null, false, false);
            }
            else
            {
                RefreshActiveTool();
            }
        }

        private void InfoAboutMenuItem_Click(Object sender, EventArgs e)
        {
            ClearActiveTool();
            using (ThankYouDialog tyForm = new ThankYouDialog())
            {
                tyForm.StartPosition = FormStartPosition.CenterParent;
                tyForm.ShowDialog(this);
            }
            RefreshActiveTool();
        }

        private void InfoWebsiteMenuItem_Click(Object sender, EventArgs e)
        {
            Process.Start(Program.GithubUrl);
        }

        private void InfoCheckForUpdatesMenuItem_Click(Object sender, EventArgs e)
        {
            CheckForUpdates(false);
        }

        private async void CheckForUpdates(bool onlyShowWhenNew)
        {
            this.shouldCheckUpdate = false;
            string title = Program.ProgramVersionTitle;
            if (this.startedUpdate)
            {
                if (!onlyShowWhenNew)
                {
                    MessageBox.Show(this, "Update check already started. Please wait.", title, MessageBoxButtons.OK);
                }
                return;
            }
            this.startedUpdate = true;
            this.SetTitle();
            bool newFound = false;
            string tag = null;
            const string checkError = "An error occurred when checking the version:";
            AssemblyName assn = Assembly.GetExecutingAssembly().GetName();
            System.Version curVer = assn.Version;
            Uri downloadUri = new Uri(Program.GithubVerCheckUrl);
            byte[] content = null;
            String returnMessage = null;
            try
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, downloadUri))
                    {
                        // GitHub API won't accept the request without header.
                        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("GithubProject", curVer.ToString()));
                        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("(" + Program.GithubUrl + ")"));
                        using (HttpResponseMessage response = await client.SendAsync(request))
                        using (var bytes = new MemoryStream())
                        {
                            await response.Content.CopyToAsync(bytes);
                            content = bytes.ToArray();
                        }
                    }
                }
                catch (Exception ex)
                {
                    string type = ex.GetType().Name;
                    string message = ex.Message;
                    // if the inner is a web exception, use that; it is more descriptive than the generic "an error occurred" one from the HttpRequestException.
                    if (ex.InnerException is WebException wex)
                    {
                        type = wex.GetType().Name;
                        message = wex.Message;
                        // Default "The remote name could not be resolved" message you get when not connected to the internet.
                        if ((UInt32)wex.HResult == 0x80131509)
                        {
                            message += "\n\nPlease ensure you are connected to the internet.";
                        }
                    }
                    returnMessage = checkError + "\n\n" + type + ": " + message;
                    return;
                }
                if (content == null || content.Length == 0)
                {
                    returnMessage = checkError + "\n\nThe response from the server contained no data.";
                    return;
                }
                String text = Encoding.UTF8.GetString(content);
                // search string (can't be bothered parsing) is
                Regex regex = new Regex("\"tag_name\":\\s*\"(v((\\d+)\\.(\\d+)(\\.(\\d+))?(\\.(\\d+))?))\"");
                Match match = regex.Match(text);
                const string dots = " (...)";
                const int maxLen = 1500 - 6;
                if (!match.Success)
                {
                    text = text.Trim('\r', '\n');
                    if (text.Length > maxLen)
                    {
                        text = text.Substring(0, maxLen) + dots;
                    }
                    returnMessage = checkError + " could not find version in returned data.\n\nReturned data:\n" + text;
                    return;
                }
                tag = match.Groups[1].Value;
                string versionMajStr = match.Groups[3].Value;
                string versionMinStr = match.Groups[4].Value;
                string versionBldStr = match.Groups[6].Value;
                string versionRevStr = match.Groups[8].Value;
                int versionMaj = String.IsNullOrEmpty(versionMajStr) ? 0 : Int32.Parse(versionMajStr);
                int versionMin = String.IsNullOrEmpty(versionMinStr) ? 0 : Int32.Parse(versionMinStr);
                int versionBld = String.IsNullOrEmpty(versionBldStr) ? 0 : Int32.Parse(versionBldStr);
                int versionRev = String.IsNullOrEmpty(versionRevStr) ? 0 : Int32.Parse(versionRevStr);
                System.Version serverVer = new System.Version(versionMaj, versionMin, versionBld, versionRev);
                System.Version.TryParse(Properties.Settings.Default.LastCheckVersion, out System.Version lastChecked);
                bool isDifferent = serverVer != lastChecked;
                if (onlyShowWhenNew && !isDifferent)
                {
                    return;
                }
                if (isDifferent)
                {
                    Properties.Settings.Default.LastCheckVersion = serverVer.ToString();
                    Properties.Settings.Default.Save();
                }
                StringBuilder versionMessage = new StringBuilder();
                if (curVer < serverVer)
                {
                    newFound = true;
                    versionMessage.Append("A newer version ").Append(serverVer.ToString()).Append(" was released on GitHub.\n\n")
                        .Append("Press \"OK\" to open the download page of the latest version.");
                }
                else
                {
                    versionMessage.Append("The latest version on GitHub is ").Append(serverVer.ToString()).Append(". ");
                    versionMessage.Append(curVer == serverVer ? "You are up to date." : "Looks like you're using a super-exclusive unreleased version!");
                }
                returnMessage = versionMessage.ToString();
            }
            finally
            {
                this.startedUpdate = false;
                this.SetTitle();
                if (returnMessage != null && (!onlyShowWhenNew || newFound))
                {
                    ClearActiveTool();
                    if (!newFound)
                    {
                        MessageBox.Show(this, returnMessage, title);
                    }
                    else
                    {
                        if (DialogResult.OK == MessageBox.Show(this, returnMessage, title, MessageBoxButtons.OKCancel, MessageBoxIcon.None, MessageBoxDefaultButton.Button1))
                        {
                            Process.Start(Program.GithubUrl + "/releases/tag/" + tag);
                        }
                    }
                    RefreshActiveTool();
                }
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
            CleanupTools(GameType.None);
            RefreshUI();
            UpdateUndoRedo();
            if (filename != null)
            {
                this.shouldCheckUpdate = Globals.CheckUpdatesOnStartup;
                OpenFile(filename);
            }
            else if (Globals.CheckUpdatesOnStartup)
            {
                CheckForUpdates(true);
            }
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
            foreach (string key in theaterIcons.Keys)
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
                ClearActiveTool();
                var message = string.IsNullOrEmpty(filename) ? "Save new map?" : string.Format("Save map '{0}'?", filename);
                var result = MessageBox.Show(message, "Save", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                switch (result)
                {
                    case DialogResult.Yes:
                        {
                            if (!this.DoValidate())
                            {
                                return false;
                            }
                            if (string.IsNullOrEmpty(filename))
                            {
                                SaveAsAction(nextAction, true);
                            }
                            else
                            {
                                SaveAction(false, nextAction, true, false);
                            }
                            if (nextAction == null)
                            {
                                RefreshActiveTool();
                            }
                            // Cancel current operation, since stuff after multithreading will take care of the operation.
                            return false;
                        }
                    case DialogResult.No:
                        break;
                    case DialogResult.Cancel:
                        RefreshActiveTool();
                        return false;
                }
            }
            if (!onlyAfterSave && nextAction != null)
            {
                nextAction();
            }
            else
            {
                RefreshActiveTool();
            }
            return true;
        }

        public void UpdateStatus()
        {
            SetTitle();
        }

        private void LoadIcons(IGamePlugin plugin)
        {
            TheaterType theater = plugin.Map.Theater;
            string th = theater.Name;
            TemplateType template = plugin.Map.TemplateTypes.Where(tt => tt.ExistsInTheater && (tt.Flag & TemplateTypeFlag.Clear) != TemplateTypeFlag.Clear
                && tt.IconWidth == 1 && tt.IconHeight == 1) //&& (tt.Theaters == null || tt.Theaters.Contains(th)))
                .OrderBy(tt => tt.Name).FirstOrDefault();
            Tile templateTile = null;
            if (template != null)
            {
                Globals.TheTilesetManager.GetTileData(template.Name, template.GetIconIndex(template.GetFirstValidIcon()), out templateTile);
            }
            // For the following, check if the thumbnail was initialised.
            SmudgeType smudge = plugin.Map.SmudgeTypes.Where(sm => !sm.IsAutoBib && sm.Icons == 1 && sm.Size.Width == 1 && sm.Size.Height == 1 && sm.Thumbnail != null
                && (!Globals.FilterTheaterObjects || sm.ExistsInTheater))
                .OrderBy(sm => sm.ID).FirstOrDefault();
            if (smudge == null)
            {
                smudge = plugin.Map.SmudgeTypes.Where(sm => !sm.IsAutoBib && sm.Size.Width == 1 && sm.Size.Height == 1 && sm.Thumbnail != null
                    && (!Globals.FilterTheaterObjects || sm.ExistsInTheater))
                    .OrderBy(sm => sm.ID).FirstOrDefault();
                if (smudge == null)
                {
                    smudge = plugin.Map.SmudgeTypes.Where(sm => !sm.IsAutoBib && sm.Thumbnail != null
                        && (!Globals.FilterTheaterObjects || sm.ExistsInTheater))
                        .OrderByDescending(sm => sm.ID).FirstOrDefault();
                }
            }
            OverlayType overlay = plugin.Map.OverlayTypes.Where(ov => (ov.Flag & OverlayTypeFlag.Crate) != OverlayTypeFlag.None && ov.Thumbnail != null
                && (!Globals.FilterTheaterObjects || ov.ExistsInTheater))
                .OrderBy(ov => ov.ID).FirstOrDefault();
            if (overlay == null)
            {
                overlay = plugin.Map.OverlayTypes.Where(ov => (ov.Flag & OverlayTypeFlag.Flag) == OverlayTypeFlag.Flag && ov.Thumbnail != null
                    && (!Globals.FilterTheaterObjects || ov.ExistsInTheater))
                    .OrderBy(ov => ov.ID).FirstOrDefault();
            }
            TerrainType terrain = plugin.Map.TerrainTypes.Where(tr => tr.Thumbnail != null &&
                (!Globals.FilterTheaterObjects || tr.Theaters == null || tr.Theaters.Contains(th)))
                .OrderBy(tr => tr.ID).FirstOrDefault();
            InfantryType infantry = plugin.Map.InfantryTypes.FirstOrDefault();
            UnitType unit = plugin.Map.UnitTypes.FirstOrDefault();
            BuildingType building = plugin.Map.BuildingTypes.Where(bl => bl.Size.Width == 2 && bl.Size.Height == 2
                                        && (!Globals.FilterTheaterObjects || !bl.IsTheaterDependent || bl.ExistsInTheater)).OrderBy(bl => bl.ID).FirstOrDefault();
            OverlayType resource = plugin.Map.OverlayTypes.Where(ov => (ov.Flag & OverlayTypeFlag.TiberiumOrGold) == OverlayTypeFlag.TiberiumOrGold
                                        && (!Globals.FilterTheaterObjects || ov.ExistsInTheater)).OrderBy(ov => ov.ID).FirstOrDefault();
            OverlayType wall = plugin.Map.OverlayTypes.Where(ov => (ov.Flag & OverlayTypeFlag.Wall) == OverlayTypeFlag.Wall
                                        && (!Globals.FilterTheaterObjects || ov.ExistsInTheater)).OrderBy(ov => ov.ID).FirstOrDefault();
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
            // These functions return a new image that needs to be disposed afterwards.
            // Since LoadNewIcon clones it, we can just immediately dispose it here.
            using (Bitmap waypoint = plugin.GameInfo.GetWaypointIcon())
                LoadNewIcon(waypointsToolStripButton, waypoint, plugin, 9);
            using (Bitmap cellTrigger = plugin.GameInfo.GetCellTriggerIcon())
                LoadNewIcon(cellTriggersToolStripButton, cellTrigger, plugin, 10);
            using (Bitmap select = plugin.GameInfo.GetSelectIcon())
                LoadNewIcon(selectToolStripButton, select, plugin, 11, false);
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
            string id = ((int)plugin.GameInfo.GameType) + "_"
                + Enumerable.Range(0, plugin.Map.TheaterTypes.Count).FirstOrDefault(i => plugin.Map.TheaterTypes[i].ID.Equals(plugin.Map.Theater.ID))
                + "_" + index;
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

        private void MapPanel_PostRender(object sender, RenderEventArgs e)
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
                    if (plugin != null && plugin.Map != null && mapPanel.MapImage != null)
                    {
                        Rectangle rect = plugin.Map.Bounds;
                        rect.Inflate(1, 1);
                        if (plugin.Map.Metrics.Bounds == rect)
                        {
                            mapPanel.Zoom = 1.0;
                        }
                        else
                        {
                            mapPanel.JumpToPosition(plugin.Map.Metrics, rect, true);
                        }
                    }
                }
            }
        }
    }
}
