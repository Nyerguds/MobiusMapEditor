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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace MobiusEditor
{
    public partial class MainForm : Form, IFeedBackHandler
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
                if (activeToolType != firstAvailableTool)
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
                        // Save some processing by just always removing this one.
                        if (plugin.GameType == GameType.TiberianDawn)
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

        static MainForm()
        {
            toolTypes = ((IEnumerable<ToolType>)Enum.GetValues(typeof(ToolType))).Where(t => t != ToolType.None).ToArray();
        }

        public MainForm(String fileToOpen)
        {
            this.filename = fileToOpen;

            InitializeComponent();
            // Obey the setting.
            this.mapPanel.SmoothScale = Globals.MapSmoothScale;
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
                cellTriggersToolStripButton
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
        }

        private void SetTitle()
        {
            string file = filename;
            if (plugin != null && file == null)
            {
                file = "Untitled" + (plugin.GameType == GameType.TiberianDawn ? ".ini" : ".mpr");
            }
            String mainTitle = GetProgramVersionTitle();
            if (file != null)
            {
                this.Text = string.Format("{0} - {1}{2}", mainTitle, file, plugin != null && plugin.Dirty ? "*" : String.Empty);
            }
            else
            {
                this.Text = mainTitle;
            }
        }

        private String GetProgramVersionTitle()
        {
            AssemblyName assn = Assembly.GetExecutingAssembly().GetName();
            System.Version currentVersion = assn.Version;
            return string.Format("CnC TDRA Map Editor v{0}", currentVersion);
        }

        private void SteamUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (SteamworksUGC.IsInit)
            {
                SteamworksUGC.Service();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RefreshAvailableTools();
            UpdateVisibleLayers();
            //filePublishMenuItem.Enabled = SteamworksUGC.IsInit;
            steamUpdateTimer.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            steamUpdateTimer.Stop();
            steamUpdateTimer.Dispose();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Q)
            {
                mapToolStripButton.PerformClick();
                return true;
            }
            else if (keyData == Keys.W)
            {
                smudgeToolStripButton.PerformClick();
                return true;
            }
            else if (keyData == Keys.E)
            {
                overlayToolStripButton.PerformClick();
                return true;
            }
            else if (keyData == Keys.R)
            {
                terrainToolStripButton.PerformClick();
                return true;
            }
            else if (keyData == Keys.T)
            {
                infantryToolStripButton.PerformClick();
                return true;
            }
            else if (keyData == Keys.Y)
            {
                unitToolStripButton.PerformClick();
                return true;
            }
            else if (keyData == Keys.A)
            {
                buildingToolStripButton.PerformClick();
                return true;
            }
            else if (keyData == Keys.S)
            {
                resourcesToolStripButton.PerformClick();
                return true;
            }
            else if (keyData == Keys.D)
            {
                wallsToolStripButton.PerformClick();
                return true;
            }
            else if (keyData == Keys.F)
            {
                waypointsToolStripButton.PerformClick();
                return true;
            }
            else if (keyData == Keys.G)
            {
                cellTriggersToolStripButton.PerformClick();
                return true;
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

        private void FileNewMenuItem_Click(object sender, EventArgs e)
        {
            if (!PromptSaveMap())
            {
                return;
            }
            using (NewMapDialog nmd = new NewMapDialog())
            {
                if (nmd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                if (plugin != null)
                {
                    plugin.Dispose();
                }
                plugin = null;
                string[] modPaths = null;
                if (ModPaths != null)
                {
                    ModPaths.TryGetValue(nmd.GameType, out modPaths);
                }
                Globals.TheTextureManager.ExpandModPaths = modPaths;
                Globals.TheTextureManager.Reset();
                Globals.TheTilesetManager.ExpandModPaths = modPaths;
                Globals.TheTilesetManager.Reset();
                Globals.TheTeamColorManager.ExpandModPaths = modPaths;
                if (nmd.GameType == GameType.TiberianDawn)
                {
                    Globals.TheTeamColorManager.Reset();
                    Globals.TheTeamColorManager.Load(@"DATA\XML\CNCTDTEAMCOLORS.XML");
                    plugin = new TiberianDawn.GamePlugin(this);
                    plugin.New(nmd.TheaterName);
                }
                else if (nmd.GameType == GameType.RedAlert)
                {
                    Globals.TheTeamColorManager.Reset();
                    Globals.TheTeamColorManager.Load(@"DATA\XML\CNCRATEAMCOLORS.XML");
                    plugin = new RedAlert.GamePlugin(this);
                    plugin.New(nmd.TheaterName);
                }
                if (SteamworksUGC.IsInit)
                {
                    plugin.Map.BasicSection.Author = SteamFriends.GetPersonaName();
                }
                LoadIcons(plugin);
                mapPanel.MapImage = plugin.MapImage;
                filename = null;
                SetTitle();
                url.Clear();
                ClearAllTools();
                RefreshAvailableTools();
                RefreshActiveTool();
            }
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
            }
            else
            {
                String errors = plugin.Validate();
                if (errors != null)
                {
                    MessageBox.Show(errors, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return;
                }
                var fileInfo = new FileInfo(filename);
                if (SaveFile(fileInfo.FullName, loadedFileType))
                {
                    mru.Add(fileInfo);
                }
                else
                {
                    mru.Remove(fileInfo);
                }
            }
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
                if (SaveFile(fileInfo.FullName, FileType.INI))
                {
                    mru.Add(fileInfo);
                }
                else
                {
                    mru.Remove(fileInfo);
                }
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
                url.Undo(new UndoRedoEventArgs(mapPanel, plugin.Map));
            }
        }

        private void EditRedoMenuItem_Click(object sender, EventArgs e)
        {
            if (url.CanRedo)
            {
                url.Redo(new UndoRedoEventArgs(mapPanel, plugin.Map));
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
            var basicSettings = new PropertyTracker<BasicSection>(plugin.Map.BasicSection);
            var briefingSettings = new PropertyTracker<BriefingSection>(plugin.Map.BriefingSection);
            var houseSettingsTrackers = plugin.Map.Houses.ToDictionary(h => h, h => new PropertyTracker<House>(h));
            using (MapSettingsDialog msd = new MapSettingsDialog(plugin, basicSettings, briefingSettings, houseSettingsTrackers))
            {
                msd.StartPosition = FormStartPosition.CenterParent;
                if (msd.ShowDialog(this) == DialogResult.OK)
                {
                    basicSettings.Commit();
                    briefingSettings.Commit();
                    foreach (var houseSettingsTracker in houseSettingsTrackers.Values)
                    {
                        houseSettingsTracker.Commit();
                    }
                    plugin.Dirty = true;
                }
            }
            if (expansionEnabled && !plugin.Map.BasicSection.ExpansionEnabled)
            {
                // If Aftermath units were disbled, we can't guarantee none of them are still in
                // the undo/redo history, so the undo/redo history is cleared to avoid issues.
                // The rest of the cleanup can be found in the ViewTool class, in the BasicSection_PropertyChanged function.
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
                    {
                        maxTeams = TiberianDawn.Constants.MaxTeams;
                    }
                    break;
                case GameType.RedAlert:
                    {
                        maxTeams = RedAlert.Constants.MaxTeams;
                    }
                    break;
            }
            using (TeamTypesDialog ttd = new TeamTypesDialog(plugin, maxTeams))
            {
                ttd.StartPosition = FormStartPosition.CenterParent;
                if (ttd.ShowDialog(this) == DialogResult.OK)
                {
                    plugin.Map.TeamTypes.Clear();
                    plugin.Map.TeamTypes.AddRange(ttd.TeamTypes.OrderBy(t => t.Name, new ExplorerComparer()).Select(t => t.Clone()));
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
                    {
                        maxTriggers = TiberianDawn.Constants.MaxTriggers;
                    }
                    break;
                case GameType.RedAlert:
                    {
                        maxTriggers = RedAlert.Constants.MaxTriggers;
                    }
                    break;
            }
            using (TriggersDialog td = new TriggersDialog(plugin, maxTriggers))
            {
                td.StartPosition = FormStartPosition.CenterParent;
                if (td.ShowDialog(this) == DialogResult.OK)
                {
                    List<Trigger> reordered = td.Triggers.OrderBy(t => t.Name, new ExplorerComparer()).ToList();
                    if (Trigger.CheckForChanges(plugin.Map.Triggers.ToList(), reordered))
                    {
                        plugin.Dirty = true;
                        plugin.Map.Triggers = reordered;
                        RefreshAvailableTools();
                    }
                }
            }
        }

        private void ToolsPowerMenuItem_Click(Object sender, EventArgs e)
        {
            if (plugin == null)
            {
                return;
            }
            using (ErrorMessageBox emb = new ErrorMessageBox())
            {
                emb.Title = "Power usage";
                emb.Message = "Power balance per House:";
                emb.Errors = plugin.Map.AssessPower(plugin.GameType);
                emb.StartPosition = FormStartPosition.CenterParent;
                emb.ShowDialog(this);
            }
        }

        private void ToolsExportImage_Click(Object sender, EventArgs e)
        {
            if (plugin == null)
            {
                return;
            }
            string savePath = null;
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.AutoUpgradeEnabled = false;
                sfd.RestoreDirectory = true;
                sfd.AddExtension = true;
                sfd.Filter = "PNG files (*.png)|*.png";
                if (!string.IsNullOrEmpty(filename))
                {
                    sfd.InitialDirectory = Path.GetDirectoryName(filename);
                    sfd.FileName = Path.GetFileNameWithoutExtension(filename) + ".png";
                }
                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    savePath = sfd.FileName;
                }
            }
            if (savePath != null)
            {
                Size size = new Size(plugin.Map.Metrics.Width * Globals.ExportTileWidth, plugin.Map.Metrics.Height * Globals.ExportTileHeight);
                using (Bitmap pr = plugin.Map.GeneratePreview(size, plugin.GameType, ActiveLayers, Globals.ExportSmoothScale, false, false).ToBitmap())
                {
                    using (Graphics g = Graphics.FromImage(pr))
                    {
                        ViewTool.PostRenderMap(g, plugin.Map, Globals.ExportTileScale, MapLayerFlag.None, MapLayerFlag.None, ActiveLayers);
                    }
                    pr.Save(savePath, ImageFormat.Png);
                }
            }
        }

        private void Mru_FileSelected(object sender, FileInfo e)
        {
            OpenFile(e.FullName, true);
        }

        private void MapPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (plugin != null)
            {
                var mapPoint = mapPanel.ClientToMap(e.Location);
                var location = new Point((int)Math.Floor((double)mapPoint.X / Globals.MapTileWidth), (int)Math.Floor((double)mapPoint.Y / Globals.MapTileHeight));
                if (plugin.Map.Metrics.GetCell(location, out int cell))
                {
                    var sb = new StringBuilder();
                    sb.AppendFormat("X = {0}, Y = {1}, Cell = {2}", location.X, location.Y, cell);
                    var template = plugin.Map.Templates[cell];
                    var templateType = template?.Type;  
                    if (templateType != null)
                    {
                        sb.AppendFormat(", Template = {0} ({1})", templateType.DisplayName, template.Icon);
                    }
                    var smudge = plugin.Map.Smudge[cell];
                    var smudgeType = smudge?.Type;
                    if (smudgeType != null)
                    {
                        sb.AppendFormat(", Smudge = {0}", smudgeType.DisplayName);
                    }
                    var overlay = plugin.Map.Overlay[cell];
                    var overlayType = overlay?.Type;
                    if (overlayType != null)
                    {
                        sb.AppendFormat(", Overlay = {0}", overlayType.DisplayName);
                    }
                    var terrain = plugin.Map.Technos[location] as Terrain;
                    var terrainType = terrain?.Type;
                    if (terrainType != null)
                    {
                        sb.AppendFormat(", Terrain = {0}", terrainType.DisplayName);
                    }
                    if (plugin.Map.Technos[location] is InfantryGroup infantryGroup)
                    {
                        var subPixel = new Point(
                            (mapPoint.X * Globals.PixelWidth / Globals.MapTileWidth) % Globals.PixelWidth,
                            (mapPoint.Y * Globals.PixelHeight / Globals.MapTileHeight) % Globals.PixelHeight
                        );
                        var i = InfantryGroup.ClosestStoppingTypes(subPixel).Cast<int>().First();
                        if (infantryGroup.Infantry[i] != null)
                        {
                            sb.AppendFormat(", Infantry = {0}", infantryGroup.Infantry[i].Type.DisplayName);
                        }
                    }
                    var unit = plugin.Map.Technos[location] as Unit;
                    var unitType = unit?.Type;
                    if (unitType != null)
                    {
                        sb.AppendFormat(", Unit = {0}", unitType.DisplayName);
                    }
                    var building = plugin.Map.Buildings[location] as Building;
                    var buildingType = building?.Type;
                    if (buildingType != null)
                    {
                        sb.AppendFormat(", Building = {0}", buildingType.DisplayName);
                    }
                    cellStatusLabel.Text = sb.ToString();
                }
                else
                {
                    cellStatusLabel.Text = "No cell";
                }
            }
        }

        private void OpenFile(String fileName, bool askSave)
        {
            if (askSave && !PromptSaveMap())
            {
                return;
            }
            var fileInfo = new FileInfo(fileName);
            if (LoadFile(fileInfo.FullName))
            {
                mru.Add(fileInfo);
            }
            else
            {
                mru.Remove(fileInfo);
                MessageBox.Show(string.Format("Error loading {0}.", fileName), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool LoadFile(string loadFilename)
        {
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
            FileType fileType = FileType.None;
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
            if (fileType == FileType.None)
            {
                long filesize = 0;
                try
                {
                    filesize = new FileInfo(loadFilename).Length;
                    var bytes = File.ReadAllBytes(loadFilename);
                    var enc = new UTF8Encoding(false, true);
                    var inicontents = enc.GetString(bytes);
                    var ini = new INI();
                    ini.Parse(inicontents);
                    // if it gets to this point, the file is a text document.
                    fileType = FileType.INI;
                }
                catch
                {
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
            GameType gameType = GameType.None;
            switch (fileType)
            {
                case FileType.INI:
                {
                    gameType = File.Exists(Path.ChangeExtension(loadFilename, ".bin")) ? GameType.TiberianDawn : GameType.RedAlert;
                    break;
                }
                case FileType.BIN:
                {
                    gameType = File.Exists(Path.ChangeExtension(loadFilename, ".ini")) ? GameType.TiberianDawn : GameType.None;
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
            if (gameType == GameType.None)
            {
                return false;
            }
            Unload();
            if (plugin != null)
            {
                plugin.Dispose();
            }
            plugin = null;
            string[] modPaths = null;
            if (ModPaths != null)
            {
                ModPaths.TryGetValue(gameType, out modPaths);
            }
            Globals.TheTextureManager.ExpandModPaths = modPaths;
            Globals.TheTextureManager.Reset();
            Globals.TheTilesetManager.ExpandModPaths = modPaths;
            Globals.TheTilesetManager.Reset();
            Globals.TheTeamColorManager.ExpandModPaths = modPaths;
            switch (gameType)
            {
                case GameType.TiberianDawn:
                    {
                        Globals.TheTeamColorManager.Reset();
                        Globals.TheTeamColorManager.Load(@"DATA\XML\CNCTDTEAMCOLORS.XML");
                        plugin = new TiberianDawn.GamePlugin(this);
                    }
                    break;
                case GameType.RedAlert:
                    {
                        Globals.TheTeamColorManager.Reset();
                        Globals.TheTeamColorManager.Load(@"DATA\XML\CNCRATEAMCOLORS.XML");
                        plugin = new RedAlert.GamePlugin(this);
                    }
                    break;
            }
            string[] errors;
            try
            {
                errors = plugin.Load(loadFilename, fileType).ToArray();
                LoadIcons(plugin);
                if (errors.Length > 0)
                {
                    using (ErrorMessageBox emb = new ErrorMessageBox())
                    {
                        emb.Errors = errors;
                        emb.StartPosition = FormStartPosition.CenterParent;
                        emb.ShowDialog(this);
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error: " + ex.Message + "\n\n" + ex.StackTrace);
                this.Unload();
#if DEVELOPER
                throw;
#else
                return false;
#endif
            }
            mapPanel.MapImage = plugin.MapImage;
            filename = loadFilename;
            loadedFileType = fileType;
            plugin.Dirty = errors != null && errors.Length > 0;
            url.Clear();
            ClearAllTools();
            RefreshAvailableTools();
            RefreshActiveTool();
            return true;
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
                ActiveToolType = ToolType.None;
                this.ActiveControl = null;
                ClearAllTools();
                // Unlink plugin
                IGamePlugin pl = plugin;
                plugin = null;
                // Remove tools
                RefreshAvailableTools();
                // Clear UI
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

        private bool SaveFile(string saveFilename, FileType inputNameType)
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
                if (inputNameType == FileType.None)
                {
                    return false;
                }
                else
                {
                    fileType = inputNameType;
                }
            }
            if (string.IsNullOrEmpty(plugin.Map.SteamSection.Title))
            {
                plugin.Map.SteamSection.Title = plugin.Map.BasicSection.Name;
            }
            if (!plugin.Save(saveFilename, fileType))
            {
                return false;
            }
            var fileInfo = new FileInfo(saveFilename);
            if (fileInfo.Exists && fileInfo.Length > Globals.MaxMapSize)
            {
                MessageBox.Show(string.Format("Map file exceeds the maximum size of {0} bytes.", Globals.MaxMapSize), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            plugin.Dirty = false;
            filename = saveFilename;
            SetTitle();
            return true;
        }

        private void RefreshAvailableTools()
        {
            // Menu items
            if (plugin != null)
            {
                fileSaveMenuItem.Enabled = true;
                fileSaveAsMenuItem.Enabled = true;
                filePublishMenuItem.Enabled = true;
                fileExportMenuItem.Enabled = true;
                editUndoMenuItem.Enabled = url.CanUndo;
                editRedoMenuItem.Enabled = url.CanRedo;
                editClearUndoRedoMenuItem.Enabled = url.CanUndo || url.CanRedo;
                settingsMapSettingsMenuItem.Enabled = true;
                settingsTeamTypesMenuItem.Enabled = true;
                settingsTriggersMenuItem.Enabled = true;
                toolsPowerMenuItem.Enabled = true;
                toolsExportImageMenuItem.Enabled = true;
                developerGoToINIMenuItem.Enabled = true;
                developerDebugShowOverlapCellsMenuItem.Enabled = true;
                developerGenerateMapPreviewDirectoryMenuItem.Enabled = true;
                viewIndicatorsBuildingFakeLabelsMenuItem.Visible = plugin.GameType == GameType.RedAlert;
            }
            // Tools
            availableToolTypes = ToolType.None;
            if (plugin != null)
            {
                TheaterType th = plugin.Map.Theater;
                availableToolTypes |= ToolType.Waypoint;
                if (plugin.Map.TemplateTypes.Any(t => t.Theaters == null || t.Theaters.Contains(th))) availableToolTypes |= ToolType.Map;
                if (plugin.Map.SmudgeTypes.Any()) availableToolTypes |= ToolType.Smudge;
                if (plugin.Map.OverlayTypes.Any(t => t.IsPlaceable && ((t.Theaters == null) || t.Theaters.Contains(th)))) availableToolTypes |= ToolType.Overlay;
                if (plugin.Map.TerrainTypes.Any(t => t.Theaters == null || t.Theaters.Contains(th))) availableToolTypes |= ToolType.Terrain;
                if (plugin.Map.InfantryTypes.Any()) availableToolTypes |= ToolType.Infantry;
                if (plugin.Map.UnitTypes.Any()) availableToolTypes |= ToolType.Unit;
                if (plugin.Map.BuildingTypes.Any(t => t.Theaters == null || t.Theaters.Contains(th))) availableToolTypes |= ToolType.Building;
                if (plugin.Map.OverlayTypes.Any(t => t.IsResource && (t.Theaters == null || t.Theaters.Contains(th)))) availableToolTypes |= ToolType.Resources;
                if (plugin.Map.OverlayTypes.Any(t => t.IsWall && (t.Theaters == null || t.Theaters.Contains(th)))) availableToolTypes |= ToolType.Wall;
                availableToolTypes |= ToolType.CellTrigger;
            }
            foreach (var toolStripButton in viewToolStripButtons)
            {
                toolStripButton.Enabled = (availableToolTypes & toolStripButton.ToolType) != ToolType.None;
            }
            ActiveToolType = activeToolType;
        }

        private void ClearAllTools()
        {
            // Menu items
            fileSaveMenuItem.Enabled = false;
            fileSaveAsMenuItem.Enabled = false;
            filePublishMenuItem.Enabled = false;
            fileExportMenuItem.Enabled = false;
            editUndoMenuItem.Enabled = false;
            editRedoMenuItem.Enabled = false;
            editClearUndoRedoMenuItem.Enabled = false;
            settingsMapSettingsMenuItem.Enabled = false;
            settingsTeamTypesMenuItem.Enabled = false;
            settingsTriggersMenuItem.Enabled = false;
            toolsPowerMenuItem.Enabled = false;
            toolsExportImageMenuItem.Enabled = false;
            developerGoToINIMenuItem.Enabled = false;
            developerDebugShowOverlapCellsMenuItem.Enabled = false;
            developerGenerateMapPreviewDirectoryMenuItem.Enabled = false;
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
            if (!found)
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
                }
                if (toolDialog != null)
                {
                    toolForms.Add(ActiveToolType, toolDialog);
                }
            }
            MapLayerFlag active = ActiveLayers;
            // Save some processing by just always removing this one.
            if (plugin.GameType == GameType.TiberianDawn)
            {
                active &= ~MapLayerFlag.BuildingFakes;
            }
            if (toolDialog != null)
            {
                activeToolForm = (Form)toolDialog;
                toolDialog.Initialize(mapPanel, active, toolStatusLabel, mouseToolTip, plugin, url);
                activeTool = toolDialog.GetTool();
                activeToolForm.ResizeEnd -= ActiveToolForm_ResizeEnd;
                activeToolForm.Shown -= this.ActiveToolForm_Shown;
                activeToolForm.Shown += this.ActiveToolForm_Shown;
                activeToolForm.Show(this);
                activeTool.Activate();
                activeToolForm.ResizeEnd += ActiveToolForm_ResizeEnd;
            }
            switch (plugin.GameType)
            {
                case GameType.TiberianDawn:
                    mapPanel.MaxZoom = 8;
                    mapPanel.ZoomStep = 0.15;
                    break;
                case GameType.RedAlert:
                    mapPanel.MaxZoom = 16;
                    mapPanel.ZoomStep = 0.2;
                    break;
            }
            // Refresh toolstrip button checked states
            foreach (var toolStripButton in viewToolStripButtons)
            {
                toolStripButton.Checked = ActiveToolType == toolStripButton.ToolType;
            }
            Focus();
            UpdateVisibleLayers();
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
            Rectangle bounds = toolform.DesktopBounds;
            Rectangle workingArea = Screen.FromControl(toolform).WorkingArea;
            if (bounds.Right > workingArea.Right)
            {
                bounds.X = workingArea.Right - bounds.Width;
            }
            if (bounds.X < workingArea.Left)
            {
                bounds.X = workingArea.Left;
            }
            if (bounds.Bottom > workingArea.Bottom)
            {
                bounds.Y = workingArea.Bottom - bounds.Height;
            }
            if (bounds.Y < workingArea.Top)
            {
                bounds.Y = workingArea.Top;
            }
            toolform.DesktopBounds = bounds;
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

        private void UpdateVisibleLayers()
        {
            MapLayerFlag layers = MapLayerFlag.All;
            if (!viewIndicatorsBoundariesMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.Boundaries;
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
            if (!viewIndicatorsWaypointsMenuItem.Checked)
            {
                layers &= ~MapLayerFlag.Waypoints;
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
            ActiveLayers = layers;
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
                viewIndicatorsBoundariesMenuItem.Checked = true;
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
                viewIndicatorsBoundariesMenuItem.Checked = false;
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

        private void toolTabControl_Selected(object sender, TabControlEventArgs e)
        {
            if (plugin == null)
            {
                return;
            }
        }

        private void developerGenerateMapPreviewMenuItem_Click(object sender, EventArgs e)
        {
#if DEVELOPER
            if ((plugin == null) || string.IsNullOrEmpty(filename))
            {
                return;
            }
            plugin.Map.GenerateMapPreview().Save(Path.ChangeExtension(filename, ".tga"));
#endif
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
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                var extensions = new string[] { ".ini", ".mpr" };
                foreach (var file in Directory.EnumerateFiles(fbd.SelectedPath).Where(file => extensions.Contains(Path.GetExtension(file).ToLower())))
                {
                    GameType gameType = GameType.None;
                    var ini = new INI();
                    using (var reader = new StreamReader(file))
                    {
                        ini.Parse(reader);
                    }
                    gameType = ini.Sections.Contains("MapPack") ? GameType.RedAlert : GameType.TiberianDawn;
                    if (gameType == GameType.None)
                    {
                        continue;
                    }
                    IGamePlugin plugin = null;
                    switch (gameType)
                    {
                        case GameType.TiberianDawn:
                            {
                                plugin = new TiberianDawn.GamePlugin(false);
                            }
                            break;
                        case GameType.RedAlert:
                            {
                                plugin = new RedAlert.GamePlugin(false);
                            }
                            break;
                    }

                    plugin.Load(file, FileType.INI);
                    plugin.Map.GenerateMapPreview().Save(Path.ChangeExtension(file, ".tga"));
                    plugin.Dispose();
                }
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
            using (var sd = new SteamDialog(plugin))
            {
                sd.ShowDialog();
            }
            fileSaveMenuItem.PerformClick();
        }

        private void mainToolStrip_MouseMove(object sender, MouseEventArgs e)
        {
            mainToolStrip.Focus();
        }

        private void MainForm_Shown(object sender, System.EventArgs e)
        {
            ClearAllTools();
            RefreshAvailableTools();
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
            Tile overlayTile = null;
            if (overlay != null) Globals.TheTilesetManager.GetTileData(plugin.Map.Theater.Tilesets, overlay.Name, 0, out overlayTile, false, true);
            TerrainType terrain = plugin.Map.TerrainTypes.Where(tr => tr.Theaters == null || tr.Theaters.Contains(plugin.Map.Theater)).OrderBy(tr => tr.ID).FirstOrDefault();;
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
        }

        private void LoadNewIcon(ViewToolStripButton button, Bitmap image, IGamePlugin plugin, int index)
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
                Rectangle opaqueBounds = TextureManager.CalculateOpaqueBounds(image);
                Bitmap img = image.FitToBoundingBox(opaqueBounds, 24, 24, Color.Transparent);
                theaterIcons[id] = img;
                button.Image = img;
            }
        }
    }
}
