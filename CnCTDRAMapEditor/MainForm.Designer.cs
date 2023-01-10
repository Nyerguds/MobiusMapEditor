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
namespace MobiusEditor
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                activeTool?.Dispose();
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileNewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileNewFromImageMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileOpenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileSaveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileSaveAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.fileExportMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filePublishMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.fileRecentFilesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.fileExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editUndoMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editRedoMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editClearUndoRedoMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsMapSettingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsTeamTypesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsTriggersMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statisticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsStatsGameObjectsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsStatsPowerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsStatsStorageMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsRandomizeTilesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsExportImageMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMapEnableAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMapDisableAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMapBuildingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMapInfantryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMapUnitsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMapTerrainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMapOverlayMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMapSmudgeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMapWaypointsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boundariesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewBoundariesBoundariesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewBoundariesMapSymmetryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewIndicatorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewIndicatorsEnableAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewIndicatorsDisableAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewIndicatorsWaypointsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewIndicatorsFootballAreaMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewIndicatorsCellTriggersMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewIndicatorsObjectTriggersMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewIndicatorsBuildingRebuildLabelsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewIndicatorsBuildingFakeLabelsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.developerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.developerGenerateMapPreviewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.developerGenerateMapPreviewDirectoryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.developerGoToINIMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.developerDebugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.developerDebugShowOverlapCellsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainStatusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.cellStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.copyrightStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.mouseToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.mainToolStrip = new System.Windows.Forms.ToolStrip();
            this.mapToolStripButton = new MobiusEditor.Controls.ViewToolStripButton();
            this.smudgeToolStripButton = new MobiusEditor.Controls.ViewToolStripButton();
            this.overlayToolStripButton = new MobiusEditor.Controls.ViewToolStripButton();
            this.terrainToolStripButton = new MobiusEditor.Controls.ViewToolStripButton();
            this.infantryToolStripButton = new MobiusEditor.Controls.ViewToolStripButton();
            this.unitToolStripButton = new MobiusEditor.Controls.ViewToolStripButton();
            this.buildingToolStripButton = new MobiusEditor.Controls.ViewToolStripButton();
            this.resourcesToolStripButton = new MobiusEditor.Controls.ViewToolStripButton();
            this.wallsToolStripButton = new MobiusEditor.Controls.ViewToolStripButton();
            this.waypointsToolStripButton = new MobiusEditor.Controls.ViewToolStripButton();
            this.cellTriggersToolStripButton = new MobiusEditor.Controls.ViewToolStripButton();
            this.selectToolStripButton = new MobiusEditor.Controls.ViewToolStripButton();
            this.mapPanel = new MobiusEditor.Controls.MapPanel();
            this.settingsSafeTileDraggingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainMenuStrip.SuspendLayout();
            this.mainStatusStrip.SuspendLayout();
            this.mainToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.developerToolStripMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Size = new System.Drawing.Size(1008, 24);
            this.mainMenuStrip.TabIndex = 1;
            this.mainMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileNewMenuItem,
            this.fileNewFromImageMenuItem,
            this.fileOpenMenuItem,
            this.fileSaveMenuItem,
            this.fileSaveAsMenuItem,
            this.toolStripMenuItem4,
            this.fileExportMenuItem,
            this.filePublishMenuItem,
            this.toolStripMenuItem3,
            this.fileRecentFilesMenuItem,
            this.toolStripMenuItem1,
            this.fileExitMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // fileNewMenuItem
            // 
            this.fileNewMenuItem.Name = "fileNewMenuItem";
            this.fileNewMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.fileNewMenuItem.Size = new System.Drawing.Size(214, 22);
            this.fileNewMenuItem.Text = "&New...";
            this.fileNewMenuItem.Click += new System.EventHandler(this.FileNewMenuItem_Click);
            // 
            // fileNewFromImageMenuItem
            // 
            this.fileNewFromImageMenuItem.Name = "fileNewFromImageMenuItem";
            this.fileNewFromImageMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.fileNewFromImageMenuItem.Size = new System.Drawing.Size(214, 22);
            this.fileNewFromImageMenuItem.Text = "New from image...";
            this.fileNewFromImageMenuItem.Click += new System.EventHandler(this.FileNewFromImageMenuItem_Click);
            // 
            // fileOpenMenuItem
            // 
            this.fileOpenMenuItem.Name = "fileOpenMenuItem";
            this.fileOpenMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.fileOpenMenuItem.Size = new System.Drawing.Size(214, 22);
            this.fileOpenMenuItem.Text = "&Open...";
            this.fileOpenMenuItem.Click += new System.EventHandler(this.FileOpenMenuItem_Click);
            // 
            // fileSaveMenuItem
            // 
            this.fileSaveMenuItem.Name = "fileSaveMenuItem";
            this.fileSaveMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.fileSaveMenuItem.Size = new System.Drawing.Size(214, 22);
            this.fileSaveMenuItem.Text = "&Save";
            this.fileSaveMenuItem.Click += new System.EventHandler(this.FileSaveMenuItem_Click);
            // 
            // fileSaveAsMenuItem
            // 
            this.fileSaveAsMenuItem.Name = "fileSaveAsMenuItem";
            this.fileSaveAsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.fileSaveAsMenuItem.Size = new System.Drawing.Size(214, 22);
            this.fileSaveAsMenuItem.Text = "Save &As...";
            this.fileSaveAsMenuItem.Click += new System.EventHandler(this.FileSaveAsMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(211, 6);
            // 
            // fileExportMenuItem
            // 
            this.fileExportMenuItem.Name = "fileExportMenuItem";
            this.fileExportMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.fileExportMenuItem.Size = new System.Drawing.Size(214, 22);
            this.fileExportMenuItem.Text = "&Export...";
            this.fileExportMenuItem.Click += new System.EventHandler(this.FileExportMenuItem_Click);
            // 
            // filePublishMenuItem
            // 
            this.filePublishMenuItem.Name = "filePublishMenuItem";
            this.filePublishMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.filePublishMenuItem.Size = new System.Drawing.Size(214, 22);
            this.filePublishMenuItem.Text = "&Publish...";
            this.filePublishMenuItem.Click += new System.EventHandler(this.FilePublishMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(211, 6);
            // 
            // fileRecentFilesMenuItem
            // 
            this.fileRecentFilesMenuItem.Name = "fileRecentFilesMenuItem";
            this.fileRecentFilesMenuItem.Size = new System.Drawing.Size(214, 22);
            this.fileRecentFilesMenuItem.Text = "&Recent Files";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(211, 6);
            // 
            // fileExitMenuItem
            // 
            this.fileExitMenuItem.Name = "fileExitMenuItem";
            this.fileExitMenuItem.Size = new System.Drawing.Size(214, 22);
            this.fileExitMenuItem.Text = "E&xit";
            this.fileExitMenuItem.Click += new System.EventHandler(this.FileExitMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editUndoMenuItem,
            this.editRedoMenuItem,
            this.editClearUndoRedoMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // editUndoMenuItem
            // 
            this.editUndoMenuItem.Name = "editUndoMenuItem";
            this.editUndoMenuItem.ShortcutKeyDisplayString = "Ctrl + Z";
            this.editUndoMenuItem.Size = new System.Drawing.Size(165, 22);
            this.editUndoMenuItem.Text = "&Undo";
            this.editUndoMenuItem.Click += new System.EventHandler(this.EditUndoMenuItem_Click);
            // 
            // editRedoMenuItem
            // 
            this.editRedoMenuItem.Name = "editRedoMenuItem";
            this.editRedoMenuItem.ShortcutKeyDisplayString = "Ctrl + Y";
            this.editRedoMenuItem.Size = new System.Drawing.Size(165, 22);
            this.editRedoMenuItem.Text = "&Redo";
            this.editRedoMenuItem.Click += new System.EventHandler(this.EditRedoMenuItem_Click);
            // 
            // editClearUndoRedoMenuItem
            // 
            this.editClearUndoRedoMenuItem.Name = "editClearUndoRedoMenuItem";
            this.editClearUndoRedoMenuItem.Size = new System.Drawing.Size(165, 22);
            this.editClearUndoRedoMenuItem.Text = "Clear Undo/Redo";
            this.editClearUndoRedoMenuItem.Click += new System.EventHandler(this.EditClearUndoRedoMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsMapSettingsMenuItem,
            this.settingsTeamTypesMenuItem,
            this.settingsTriggersMenuItem,
            this.settingsSafeTileDraggingToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "&Settings";
            // 
            // settingsMapSettingsMenuItem
            // 
            this.settingsMapSettingsMenuItem.Name = "settingsMapSettingsMenuItem";
            this.settingsMapSettingsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this.settingsMapSettingsMenuItem.Size = new System.Drawing.Size(197, 22);
            this.settingsMapSettingsMenuItem.Text = "&Map Settings...";
            this.settingsMapSettingsMenuItem.Click += new System.EventHandler(this.SettingsMapSettingsMenuItem_Click);
            // 
            // settingsTeamTypesMenuItem
            // 
            this.settingsTeamTypesMenuItem.Name = "settingsTeamTypesMenuItem";
            this.settingsTeamTypesMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.settingsTeamTypesMenuItem.Size = new System.Drawing.Size(197, 22);
            this.settingsTeamTypesMenuItem.Text = "&Team Types...";
            this.settingsTeamTypesMenuItem.Click += new System.EventHandler(this.SettingsTeamTypesMenuItem_Click);
            // 
            // settingsTriggersMenuItem
            // 
            this.settingsTriggersMenuItem.Name = "settingsTriggersMenuItem";
            this.settingsTriggersMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.settingsTriggersMenuItem.Size = new System.Drawing.Size(197, 22);
            this.settingsTriggersMenuItem.Text = "T&riggers...";
            this.settingsTriggersMenuItem.Click += new System.EventHandler(this.SettingsTriggersMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statisticsToolStripMenuItem,
            this.toolsRandomizeTilesMenuItem,
            this.toolsExportImageMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.toolsToolStripMenuItem.Text = "&Tools";
            // 
            // statisticsToolStripMenuItem
            // 
            this.statisticsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolsStatsGameObjectsMenuItem,
            this.toolsStatsPowerMenuItem,
            this.toolsStatsStorageMenuItem});
            this.statisticsToolStripMenuItem.Name = "statisticsToolStripMenuItem";
            this.statisticsToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.statisticsToolStripMenuItem.Text = "&Statistics";
            // 
            // toolsStatsGameObjectsMenuItem
            // 
            this.toolsStatsGameObjectsMenuItem.Name = "toolsStatsGameObjectsMenuItem";
            this.toolsStatsGameObjectsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.toolsStatsGameObjectsMenuItem.Size = new System.Drawing.Size(205, 22);
            this.toolsStatsGameObjectsMenuItem.Text = "&Map Objects";
            this.toolsStatsGameObjectsMenuItem.Click += new System.EventHandler(this.ToolsStatsGameObjectsMenuItem_Click);
            // 
            // toolsStatsPowerMenuItem
            // 
            this.toolsStatsPowerMenuItem.Name = "toolsStatsPowerMenuItem";
            this.toolsStatsPowerMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.toolsStatsPowerMenuItem.Size = new System.Drawing.Size(205, 22);
            this.toolsStatsPowerMenuItem.Text = "&Power Balance...";
            this.toolsStatsPowerMenuItem.Click += new System.EventHandler(this.ToolsStatsPowerMenuItem_Click);
            // 
            // toolsStatsStorageMenuItem
            // 
            this.toolsStatsStorageMenuItem.Name = "toolsStatsStorageMenuItem";
            this.toolsStatsStorageMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.toolsStatsStorageMenuItem.Size = new System.Drawing.Size(205, 22);
            this.toolsStatsStorageMenuItem.Text = "&Silo Storage...";
            this.toolsStatsStorageMenuItem.Click += new System.EventHandler(this.ToolsStatsStorageMenuItem_Click);
            // 
            // toolsRandomizeTilesMenuItem
            // 
            this.toolsRandomizeTilesMenuItem.Name = "toolsRandomizeTilesMenuItem";
            this.toolsRandomizeTilesMenuItem.Size = new System.Drawing.Size(204, 22);
            this.toolsRandomizeTilesMenuItem.Text = "&Re-randomize tiles";
            this.toolsRandomizeTilesMenuItem.Click += new System.EventHandler(this.ToolsRandomizeTilesMenuItem_Click);
            // 
            // toolsExportImageMenuItem
            // 
            this.toolsExportImageMenuItem.Name = "toolsExportImageMenuItem";
            this.toolsExportImageMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.toolsExportImageMenuItem.Size = new System.Drawing.Size(204, 22);
            this.toolsExportImageMenuItem.Text = "&Export as Image...";
            this.toolsExportImageMenuItem.Click += new System.EventHandler(this.ToolsExportImage_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewMapToolStripMenuItem,
            this.boundariesToolStripMenuItem,
            this.viewIndicatorsToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // viewMapToolStripMenuItem
            // 
            this.viewMapToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewMapEnableAllMenuItem,
            this.viewMapDisableAllMenuItem,
            this.viewMapBuildingsMenuItem,
            this.viewMapInfantryMenuItem,
            this.viewMapUnitsMenuItem,
            this.viewMapTerrainMenuItem,
            this.viewMapOverlayMenuItem,
            this.viewMapSmudgeMenuItem,
            this.viewMapWaypointsMenuItem});
            this.viewMapToolStripMenuItem.Name = "viewMapToolStripMenuItem";
            this.viewMapToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.viewMapToolStripMenuItem.Text = "&Map";
            // 
            // viewMapEnableAllMenuItem
            // 
            this.viewMapEnableAllMenuItem.Name = "viewMapEnableAllMenuItem";
            this.viewMapEnableAllMenuItem.Size = new System.Drawing.Size(130, 22);
            this.viewMapEnableAllMenuItem.Text = "Enable all";
            this.viewMapEnableAllMenuItem.Click += new System.EventHandler(this.ViewMapEnableAllMenuItem_Click);
            // 
            // viewMapDisableAllMenuItem
            // 
            this.viewMapDisableAllMenuItem.Name = "viewMapDisableAllMenuItem";
            this.viewMapDisableAllMenuItem.Size = new System.Drawing.Size(130, 22);
            this.viewMapDisableAllMenuItem.Text = "Disable all";
            this.viewMapDisableAllMenuItem.Click += new System.EventHandler(this.ViewMapDisableAllMenuItem_Click);
            // 
            // viewMapBuildingsMenuItem
            // 
            this.viewMapBuildingsMenuItem.Checked = true;
            this.viewMapBuildingsMenuItem.CheckOnClick = true;
            this.viewMapBuildingsMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewMapBuildingsMenuItem.Name = "viewMapBuildingsMenuItem";
            this.viewMapBuildingsMenuItem.Size = new System.Drawing.Size(130, 22);
            this.viewMapBuildingsMenuItem.Text = "&Buildings";
            this.viewMapBuildingsMenuItem.CheckedChanged += new System.EventHandler(this.ViewMenuItem_CheckedChanged);
            // 
            // viewMapInfantryMenuItem
            // 
            this.viewMapInfantryMenuItem.Checked = true;
            this.viewMapInfantryMenuItem.CheckOnClick = true;
            this.viewMapInfantryMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewMapInfantryMenuItem.Name = "viewMapInfantryMenuItem";
            this.viewMapInfantryMenuItem.Size = new System.Drawing.Size(130, 22);
            this.viewMapInfantryMenuItem.Text = "&Infantry";
            this.viewMapInfantryMenuItem.CheckedChanged += new System.EventHandler(this.ViewMenuItem_CheckedChanged);
            // 
            // viewMapUnitsMenuItem
            // 
            this.viewMapUnitsMenuItem.Checked = true;
            this.viewMapUnitsMenuItem.CheckOnClick = true;
            this.viewMapUnitsMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewMapUnitsMenuItem.Name = "viewMapUnitsMenuItem";
            this.viewMapUnitsMenuItem.Size = new System.Drawing.Size(130, 22);
            this.viewMapUnitsMenuItem.Text = "&Units";
            this.viewMapUnitsMenuItem.CheckedChanged += new System.EventHandler(this.ViewMenuItem_CheckedChanged);
            // 
            // viewMapTerrainMenuItem
            // 
            this.viewMapTerrainMenuItem.Checked = true;
            this.viewMapTerrainMenuItem.CheckOnClick = true;
            this.viewMapTerrainMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewMapTerrainMenuItem.Name = "viewMapTerrainMenuItem";
            this.viewMapTerrainMenuItem.Size = new System.Drawing.Size(130, 22);
            this.viewMapTerrainMenuItem.Text = "&Terrain";
            this.viewMapTerrainMenuItem.CheckedChanged += new System.EventHandler(this.ViewMenuItem_CheckedChanged);
            // 
            // viewMapOverlayMenuItem
            // 
            this.viewMapOverlayMenuItem.Checked = true;
            this.viewMapOverlayMenuItem.CheckOnClick = true;
            this.viewMapOverlayMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewMapOverlayMenuItem.Name = "viewMapOverlayMenuItem";
            this.viewMapOverlayMenuItem.Size = new System.Drawing.Size(130, 22);
            this.viewMapOverlayMenuItem.Text = "&Overlay";
            this.viewMapOverlayMenuItem.CheckedChanged += new System.EventHandler(this.ViewMenuItem_CheckedChanged);
            // 
            // viewMapSmudgeMenuItem
            // 
            this.viewMapSmudgeMenuItem.Checked = true;
            this.viewMapSmudgeMenuItem.CheckOnClick = true;
            this.viewMapSmudgeMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewMapSmudgeMenuItem.Name = "viewMapSmudgeMenuItem";
            this.viewMapSmudgeMenuItem.Size = new System.Drawing.Size(130, 22);
            this.viewMapSmudgeMenuItem.Text = "&Smudge";
            this.viewMapSmudgeMenuItem.CheckedChanged += new System.EventHandler(this.ViewMenuItem_CheckedChanged);
            // 
            // viewMapWaypointsMenuItem
            // 
            this.viewMapWaypointsMenuItem.Checked = true;
            this.viewMapWaypointsMenuItem.CheckOnClick = true;
            this.viewMapWaypointsMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewMapWaypointsMenuItem.Name = "viewMapWaypointsMenuItem";
            this.viewMapWaypointsMenuItem.Size = new System.Drawing.Size(130, 22);
            this.viewMapWaypointsMenuItem.Text = "&Waypoints";
            this.viewMapWaypointsMenuItem.CheckedChanged += new System.EventHandler(this.ViewMenuItem_CheckedChanged);
            // 
            // boundariesToolStripMenuItem
            // 
            this.boundariesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewBoundariesBoundariesMenuItem,
            this.viewBoundariesMapSymmetryMenuItem});
            this.boundariesToolStripMenuItem.Name = "boundariesToolStripMenuItem";
            this.boundariesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.boundariesToolStripMenuItem.Text = "Boundaries";
            // 
            // viewBoundariesBoundariesMenuItem
            // 
            this.viewBoundariesBoundariesMenuItem.Checked = true;
            this.viewBoundariesBoundariesMenuItem.CheckOnClick = true;
            this.viewBoundariesBoundariesMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewBoundariesBoundariesMenuItem.Name = "viewBoundariesBoundariesMenuItem";
            this.viewBoundariesBoundariesMenuItem.Size = new System.Drawing.Size(160, 22);
            this.viewBoundariesBoundariesMenuItem.Text = "&Map boundaries";
            this.viewBoundariesBoundariesMenuItem.CheckedChanged += new System.EventHandler(this.ViewMenuItem_CheckedChanged);
            // 
            // viewBoundariesMapSymmetryMenuItem
            // 
            this.viewBoundariesMapSymmetryMenuItem.CheckOnClick = true;
            this.viewBoundariesMapSymmetryMenuItem.Name = "viewBoundariesMapSymmetryMenuItem";
            this.viewBoundariesMapSymmetryMenuItem.Size = new System.Drawing.Size(160, 22);
            this.viewBoundariesMapSymmetryMenuItem.Text = "Map &symmetry";
            this.viewBoundariesMapSymmetryMenuItem.CheckedChanged += new System.EventHandler(this.ViewMenuItem_CheckedChanged);
            // 
            // viewIndicatorsToolStripMenuItem
            // 
            this.viewIndicatorsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewIndicatorsEnableAllMenuItem,
            this.viewIndicatorsDisableAllMenuItem,
            this.viewIndicatorsWaypointsMenuItem,
            this.viewIndicatorsFootballAreaMenuItem,
            this.viewIndicatorsCellTriggersMenuItem,
            this.viewIndicatorsObjectTriggersMenuItem,
            this.viewIndicatorsBuildingRebuildLabelsMenuItem,
            this.viewIndicatorsBuildingFakeLabelsMenuItem});
            this.viewIndicatorsToolStripMenuItem.Name = "viewIndicatorsToolStripMenuItem";
            this.viewIndicatorsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.viewIndicatorsToolStripMenuItem.Text = "&Indicators";
            // 
            // viewIndicatorsEnableAllMenuItem
            // 
            this.viewIndicatorsEnableAllMenuItem.Name = "viewIndicatorsEnableAllMenuItem";
            this.viewIndicatorsEnableAllMenuItem.Size = new System.Drawing.Size(207, 22);
            this.viewIndicatorsEnableAllMenuItem.Text = "Enable all";
            this.viewIndicatorsEnableAllMenuItem.Click += new System.EventHandler(this.ViewIndicatorsEnableAllToolStripMenuItem_Click);
            // 
            // viewIndicatorsDisableAllMenuItem
            // 
            this.viewIndicatorsDisableAllMenuItem.Name = "viewIndicatorsDisableAllMenuItem";
            this.viewIndicatorsDisableAllMenuItem.Size = new System.Drawing.Size(207, 22);
            this.viewIndicatorsDisableAllMenuItem.Text = "Disable all";
            this.viewIndicatorsDisableAllMenuItem.Click += new System.EventHandler(this.ViewIndicatorsDisableAllToolStripMenuItem_Click);
            // 
            // viewIndicatorsWaypointsMenuItem
            // 
            this.viewIndicatorsWaypointsMenuItem.Checked = true;
            this.viewIndicatorsWaypointsMenuItem.CheckOnClick = true;
            this.viewIndicatorsWaypointsMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewIndicatorsWaypointsMenuItem.Name = "viewIndicatorsWaypointsMenuItem";
            this.viewIndicatorsWaypointsMenuItem.Size = new System.Drawing.Size(207, 22);
            this.viewIndicatorsWaypointsMenuItem.Text = "&Waypoint labels";
            this.viewIndicatorsWaypointsMenuItem.CheckedChanged += new System.EventHandler(this.ViewMenuItem_CheckedChanged);
            // 
            // viewIndicatorsFootballAreaMenuItem
            // 
            this.viewIndicatorsFootballAreaMenuItem.Checked = true;
            this.viewIndicatorsFootballAreaMenuItem.CheckOnClick = true;
            this.viewIndicatorsFootballAreaMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewIndicatorsFootballAreaMenuItem.Name = "viewIndicatorsFootballAreaMenuItem";
            this.viewIndicatorsFootballAreaMenuItem.Size = new System.Drawing.Size(207, 22);
            this.viewIndicatorsFootballAreaMenuItem.Text = "Football &goal areas";
            this.viewIndicatorsFootballAreaMenuItem.CheckedChanged += new System.EventHandler(this.ViewMenuItem_CheckedChanged);
            // 
            // viewIndicatorsCellTriggersMenuItem
            // 
            this.viewIndicatorsCellTriggersMenuItem.Checked = true;
            this.viewIndicatorsCellTriggersMenuItem.CheckOnClick = true;
            this.viewIndicatorsCellTriggersMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewIndicatorsCellTriggersMenuItem.Name = "viewIndicatorsCellTriggersMenuItem";
            this.viewIndicatorsCellTriggersMenuItem.Size = new System.Drawing.Size(207, 22);
            this.viewIndicatorsCellTriggersMenuItem.Text = "&Cell triggers";
            this.viewIndicatorsCellTriggersMenuItem.CheckedChanged += new System.EventHandler(this.ViewMenuItem_CheckedChanged);
            // 
            // viewIndicatorsObjectTriggersMenuItem
            // 
            this.viewIndicatorsObjectTriggersMenuItem.Checked = true;
            this.viewIndicatorsObjectTriggersMenuItem.CheckOnClick = true;
            this.viewIndicatorsObjectTriggersMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewIndicatorsObjectTriggersMenuItem.Name = "viewIndicatorsObjectTriggersMenuItem";
            this.viewIndicatorsObjectTriggersMenuItem.Size = new System.Drawing.Size(207, 22);
            this.viewIndicatorsObjectTriggersMenuItem.Text = "Object &triggers";
            this.viewIndicatorsObjectTriggersMenuItem.CheckedChanged += new System.EventHandler(this.ViewMenuItem_CheckedChanged);
            // 
            // viewIndicatorsBuildingRebuildLabelsMenuItem
            // 
            this.viewIndicatorsBuildingRebuildLabelsMenuItem.Checked = true;
            this.viewIndicatorsBuildingRebuildLabelsMenuItem.CheckOnClick = true;
            this.viewIndicatorsBuildingRebuildLabelsMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewIndicatorsBuildingRebuildLabelsMenuItem.Name = "viewIndicatorsBuildingRebuildLabelsMenuItem";
            this.viewIndicatorsBuildingRebuildLabelsMenuItem.Size = new System.Drawing.Size(207, 22);
            this.viewIndicatorsBuildingRebuildLabelsMenuItem.Text = "Building &rebuild priorities";
            this.viewIndicatorsBuildingRebuildLabelsMenuItem.CheckedChanged += new System.EventHandler(this.ViewMenuItem_CheckedChanged);
            // 
            // viewIndicatorsBuildingFakeLabelsMenuItem
            // 
            this.viewIndicatorsBuildingFakeLabelsMenuItem.Checked = true;
            this.viewIndicatorsBuildingFakeLabelsMenuItem.CheckOnClick = true;
            this.viewIndicatorsBuildingFakeLabelsMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewIndicatorsBuildingFakeLabelsMenuItem.Name = "viewIndicatorsBuildingFakeLabelsMenuItem";
            this.viewIndicatorsBuildingFakeLabelsMenuItem.Size = new System.Drawing.Size(207, 22);
            this.viewIndicatorsBuildingFakeLabelsMenuItem.Text = "Building \'&fake\' Labels";
            this.viewIndicatorsBuildingFakeLabelsMenuItem.CheckedChanged += new System.EventHandler(this.ViewMenuItem_CheckedChanged);
            // 
            // developerToolStripMenuItem
            // 
            this.developerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.developerGenerateMapPreviewMenuItem,
            this.developerGoToINIMenuItem,
            this.toolStripMenuItem2,
            this.developerDebugToolStripMenuItem});
            this.developerToolStripMenuItem.Name = "developerToolStripMenuItem";
            this.developerToolStripMenuItem.Size = new System.Drawing.Size(72, 20);
            this.developerToolStripMenuItem.Text = "&Developer";
            // 
            // developerGenerateMapPreviewMenuItem
            // 
            this.developerGenerateMapPreviewMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.developerGenerateMapPreviewDirectoryMenuItem});
            this.developerGenerateMapPreviewMenuItem.Name = "developerGenerateMapPreviewMenuItem";
            this.developerGenerateMapPreviewMenuItem.Size = new System.Drawing.Size(192, 22);
            this.developerGenerateMapPreviewMenuItem.Text = "&Generate map preview";
            // 
            // developerGenerateMapPreviewDirectoryMenuItem
            // 
            this.developerGenerateMapPreviewDirectoryMenuItem.Name = "developerGenerateMapPreviewDirectoryMenuItem";
            this.developerGenerateMapPreviewDirectoryMenuItem.Size = new System.Drawing.Size(131, 22);
            this.developerGenerateMapPreviewDirectoryMenuItem.Text = "&Directory...";
            this.developerGenerateMapPreviewDirectoryMenuItem.Click += new System.EventHandler(this.DeveloperGenerateMapPreviewDirectoryMenuItem_Click);
            // 
            // developerGoToINIMenuItem
            // 
            this.developerGoToINIMenuItem.Name = "developerGoToINIMenuItem";
            this.developerGoToINIMenuItem.Size = new System.Drawing.Size(192, 22);
            this.developerGoToINIMenuItem.Text = "Go to &INI";
            this.developerGoToINIMenuItem.Click += new System.EventHandler(this.DeveloperGoToINIMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(189, 6);
            // 
            // developerDebugToolStripMenuItem
            // 
            this.developerDebugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.developerDebugShowOverlapCellsMenuItem});
            this.developerDebugToolStripMenuItem.Name = "developerDebugToolStripMenuItem";
            this.developerDebugToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.developerDebugToolStripMenuItem.Text = "&Debug";
            // 
            // developerDebugShowOverlapCellsMenuItem
            // 
            this.developerDebugShowOverlapCellsMenuItem.CheckOnClick = true;
            this.developerDebugShowOverlapCellsMenuItem.Name = "developerDebugShowOverlapCellsMenuItem";
            this.developerDebugShowOverlapCellsMenuItem.Size = new System.Drawing.Size(173, 22);
            this.developerDebugShowOverlapCellsMenuItem.Text = "Show &Overlap cells";
            this.developerDebugShowOverlapCellsMenuItem.CheckedChanged += new System.EventHandler(this.DeveloperDebugShowOverlapCellsMenuItem_CheckedChanged);
            // 
            // mainStatusStrip
            // 
            this.mainStatusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.mainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStatusLabel,
            this.cellStatusLabel,
            this.copyrightStatusLabel});
            this.mainStatusStrip.Location = new System.Drawing.Point(0, 539);
            this.mainStatusStrip.Name = "mainStatusStrip";
            this.mainStatusStrip.Padding = new System.Windows.Forms.Padding(2, 0, 14, 0);
            this.mainStatusStrip.Size = new System.Drawing.Size(1008, 22);
            this.mainStatusStrip.TabIndex = 2;
            this.mainStatusStrip.Text = "statusStrip1";
            // 
            // toolStatusLabel
            // 
            this.toolStatusLabel.Name = "toolStatusLabel";
            this.toolStatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // cellStatusLabel
            // 
            this.cellStatusLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.cellStatusLabel.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.cellStatusLabel.Name = "cellStatusLabel";
            this.cellStatusLabel.Size = new System.Drawing.Size(4, 17);
            // 
            // copyrightStatusLabel
            // 
            this.copyrightStatusLabel.Name = "copyrightStatusLabel";
            this.copyrightStatusLabel.Size = new System.Drawing.Size(988, 17);
            this.copyrightStatusLabel.Spring = true;
            this.copyrightStatusLabel.Text = "©2020 Electronic Arts Inc.";
            this.copyrightStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // mainToolStrip
            // 
            this.mainToolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mapToolStripButton,
            this.smudgeToolStripButton,
            this.overlayToolStripButton,
            this.terrainToolStripButton,
            this.infantryToolStripButton,
            this.unitToolStripButton,
            this.buildingToolStripButton,
            this.resourcesToolStripButton,
            this.wallsToolStripButton,
            this.waypointsToolStripButton,
            this.cellTriggersToolStripButton,
            this.selectToolStripButton});
            this.mainToolStrip.Location = new System.Drawing.Point(0, 24);
            this.mainToolStrip.Name = "mainToolStrip";
            this.mainToolStrip.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.mainToolStrip.Size = new System.Drawing.Size(1008, 31);
            this.mainToolStrip.TabIndex = 3;
            this.mainToolStrip.Text = "toolStrip1";
            this.mainToolStrip.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MainToolStrip_MouseMove);
            // 
            // mapToolStripButton
            // 
            this.mapToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("mapToolStripButton.Image")));
            this.mapToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mapToolStripButton.Name = "mapToolStripButton";
            this.mapToolStripButton.Size = new System.Drawing.Size(59, 28);
            this.mapToolStripButton.Text = "Map";
            this.mapToolStripButton.ToolType = MobiusEditor.Interface.ToolType.Map;
            this.mapToolStripButton.Click += new System.EventHandler(this.mainToolStripButton_Click);
            // 
            // smudgeToolStripButton
            // 
            this.smudgeToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("smudgeToolStripButton.Image")));
            this.smudgeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.smudgeToolStripButton.Name = "smudgeToolStripButton";
            this.smudgeToolStripButton.Size = new System.Drawing.Size(79, 28);
            this.smudgeToolStripButton.Text = "Smudge";
            this.smudgeToolStripButton.ToolType = MobiusEditor.Interface.ToolType.Smudge;
            this.smudgeToolStripButton.Click += new System.EventHandler(this.mainToolStripButton_Click);
            // 
            // overlayToolStripButton
            // 
            this.overlayToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("overlayToolStripButton.Image")));
            this.overlayToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.overlayToolStripButton.Name = "overlayToolStripButton";
            this.overlayToolStripButton.Size = new System.Drawing.Size(75, 28);
            this.overlayToolStripButton.Text = "Overlay";
            this.overlayToolStripButton.ToolType = MobiusEditor.Interface.ToolType.Overlay;
            this.overlayToolStripButton.Click += new System.EventHandler(this.mainToolStripButton_Click);
            // 
            // terrainToolStripButton
            // 
            this.terrainToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("terrainToolStripButton.Image")));
            this.terrainToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.terrainToolStripButton.Name = "terrainToolStripButton";
            this.terrainToolStripButton.Size = new System.Drawing.Size(70, 28);
            this.terrainToolStripButton.Text = "Terrain";
            this.terrainToolStripButton.ToolType = MobiusEditor.Interface.ToolType.Terrain;
            this.terrainToolStripButton.Click += new System.EventHandler(this.mainToolStripButton_Click);
            // 
            // infantryToolStripButton
            // 
            this.infantryToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("infantryToolStripButton.Image")));
            this.infantryToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.infantryToolStripButton.Name = "infantryToolStripButton";
            this.infantryToolStripButton.Size = new System.Drawing.Size(76, 28);
            this.infantryToolStripButton.Text = "Infantry";
            this.infantryToolStripButton.ToolType = MobiusEditor.Interface.ToolType.Infantry;
            this.infantryToolStripButton.Click += new System.EventHandler(this.mainToolStripButton_Click);
            // 
            // unitToolStripButton
            // 
            this.unitToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("unitToolStripButton.Image")));
            this.unitToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.unitToolStripButton.Name = "unitToolStripButton";
            this.unitToolStripButton.Size = new System.Drawing.Size(62, 28);
            this.unitToolStripButton.Text = "Units";
            this.unitToolStripButton.ToolType = MobiusEditor.Interface.ToolType.Unit;
            this.unitToolStripButton.Click += new System.EventHandler(this.mainToolStripButton_Click);
            // 
            // buildingToolStripButton
            // 
            this.buildingToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("buildingToolStripButton.Image")));
            this.buildingToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buildingToolStripButton.Name = "buildingToolStripButton";
            this.buildingToolStripButton.Size = new System.Drawing.Size(88, 28);
            this.buildingToolStripButton.Text = "Structures";
            this.buildingToolStripButton.ToolType = MobiusEditor.Interface.ToolType.Building;
            this.buildingToolStripButton.Click += new System.EventHandler(this.mainToolStripButton_Click);
            // 
            // resourcesToolStripButton
            // 
            this.resourcesToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("resourcesToolStripButton.Image")));
            this.resourcesToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.resourcesToolStripButton.Name = "resourcesToolStripButton";
            this.resourcesToolStripButton.Size = new System.Drawing.Size(88, 28);
            this.resourcesToolStripButton.Text = "Resources";
            this.resourcesToolStripButton.ToolType = MobiusEditor.Interface.ToolType.Resources;
            this.resourcesToolStripButton.Click += new System.EventHandler(this.mainToolStripButton_Click);
            // 
            // wallsToolStripButton
            // 
            this.wallsToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("wallsToolStripButton.Image")));
            this.wallsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.wallsToolStripButton.Name = "wallsToolStripButton";
            this.wallsToolStripButton.Size = new System.Drawing.Size(63, 28);
            this.wallsToolStripButton.Text = "Walls";
            this.wallsToolStripButton.ToolType = MobiusEditor.Interface.ToolType.Wall;
            this.wallsToolStripButton.Click += new System.EventHandler(this.mainToolStripButton_Click);
            // 
            // waypointsToolStripButton
            // 
            this.waypointsToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("waypointsToolStripButton.Image")));
            this.waypointsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.waypointsToolStripButton.Name = "waypointsToolStripButton";
            this.waypointsToolStripButton.Size = new System.Drawing.Size(91, 28);
            this.waypointsToolStripButton.Text = "Waypoints";
            this.waypointsToolStripButton.ToolType = MobiusEditor.Interface.ToolType.Waypoint;
            this.waypointsToolStripButton.Click += new System.EventHandler(this.mainToolStripButton_Click);
            // 
            // cellTriggersToolStripButton
            // 
            this.cellTriggersToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("cellTriggersToolStripButton.Image")));
            this.cellTriggersToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cellTriggersToolStripButton.Name = "cellTriggersToolStripButton";
            this.cellTriggersToolStripButton.Size = new System.Drawing.Size(99, 28);
            this.cellTriggersToolStripButton.Text = "Cell Triggers";
            this.cellTriggersToolStripButton.ToolType = MobiusEditor.Interface.ToolType.CellTrigger;
            this.cellTriggersToolStripButton.Click += new System.EventHandler(this.mainToolStripButton_Click);
            // 
            // selectToolStripButton
            // 
            this.selectToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("selectToolStripButton.Image")));
            this.selectToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.selectToolStripButton.Name = "selectToolStripButton";
            this.selectToolStripButton.Size = new System.Drawing.Size(66, 28);
            this.selectToolStripButton.Text = "Select";
            this.selectToolStripButton.ToolType = MobiusEditor.Interface.ToolType.Select;
            this.selectToolStripButton.Visible = false;
            this.selectToolStripButton.Click += new System.EventHandler(this.mainToolStripButton_Click);
            // 
            // mapPanel
            // 
            this.mapPanel.AllowDrop = true;
            this.mapPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.mapPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapPanel.FocusOnMouseEnter = true;
            this.mapPanel.Location = new System.Drawing.Point(0, 55);
            this.mapPanel.MapImage = null;
            this.mapPanel.MaxZoom = 8D;
            this.mapPanel.MinZoom = 1D;
            this.mapPanel.Name = "mapPanel";
            this.mapPanel.Size = new System.Drawing.Size(1008, 484);
            this.mapPanel.SmoothScale = false;
            this.mapPanel.TabIndex = 4;
            this.mapPanel.Zoom = 1D;
            this.mapPanel.ZoomStep = 1D;
            this.mapPanel.PostRender += new System.EventHandler<MobiusEditor.Event.RenderEventArgs>(this.mapPanel_PostRender);
            this.mapPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.MapPanel_DragDrop);
            this.mapPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.MapPanel_DragEnter);
            this.mapPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MapPanel_MouseMove);
            // 
            // settingsSafeTileDraggingToolStripMenuItem
            // 
            this.settingsSafeTileDraggingToolStripMenuItem.Checked = true;
            this.settingsSafeTileDraggingToolStripMenuItem.CheckOnClick = true;
            this.settingsSafeTileDraggingToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.settingsSafeTileDraggingToolStripMenuItem.Name = "settingsSafeTileDraggingToolStripMenuItem";
            this.settingsSafeTileDraggingToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.settingsSafeTileDraggingToolStripMenuItem.Text = "Safe tile dragging";
            this.settingsSafeTileDraggingToolStripMenuItem.Click += new System.EventHandler(this.SettingsSafeTileDraggingToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 561);
            this.Controls.Add(this.mapPanel);
            this.Controls.Add(this.mainToolStrip);
            this.Controls.Add(this.mainStatusStrip);
            this.Controls.Add(this.mainMenuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.mainMenuStrip;
            this.Name = "MainForm";
            this.Text = "CnC TDRA Map Editor";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.mainStatusStrip.ResumeLayout(false);
            this.mainStatusStrip.PerformLayout();
            this.mainToolStrip.ResumeLayout(false);
            this.mainToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileOpenMenuItem;
        private System.Windows.Forms.StatusStrip mainStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel cellStatusLabel;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileSaveAsMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem fileExitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileNewMenuItem;
        private System.Windows.Forms.ToolStripMenuItem developerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem developerGenerateMapPreviewMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileExportMenuItem;
        private System.Windows.Forms.ToolStripMenuItem developerGenerateMapPreviewDirectoryMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsMapSettingsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editUndoMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editRedoMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem developerDebugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem developerDebugShowOverlapCellsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filePublishMenuItem;
        private System.Windows.Forms.ToolTip mouseToolTip;
        private System.Windows.Forms.ToolStrip mainToolStrip;
        private MobiusEditor.Controls.ViewToolStripButton mapToolStripButton;
        private MobiusEditor.Controls.ViewToolStripButton smudgeToolStripButton;
        private MobiusEditor.Controls.ViewToolStripButton overlayToolStripButton;
        private MobiusEditor.Controls.ViewToolStripButton terrainToolStripButton;
        private MobiusEditor.Controls.ViewToolStripButton infantryToolStripButton;
        private MobiusEditor.Controls.ViewToolStripButton unitToolStripButton;
        private MobiusEditor.Controls.ViewToolStripButton buildingToolStripButton;
        private MobiusEditor.Controls.ViewToolStripButton resourcesToolStripButton;
        private MobiusEditor.Controls.ViewToolStripButton wallsToolStripButton;
        private MobiusEditor.Controls.ViewToolStripButton waypointsToolStripButton;
        private Controls.MapPanel mapPanel;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem fileRecentFilesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileSaveMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem developerGoToINIMenuItem;
        private MobiusEditor.Controls.ViewToolStripButton cellTriggersToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem settingsTeamTypesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsTriggersMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewMapTerrainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewMapOverlayMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel copyrightStatusLabel;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsExportImageMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewMapBuildingsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewMapUnitsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewMapInfantryMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewMapSmudgeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewMapWaypointsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewMapEnableAllMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewIndicatorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewIndicatorsEnableAllMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewIndicatorsDisableAllMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewIndicatorsObjectTriggersMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewIndicatorsBuildingRebuildLabelsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewIndicatorsBuildingFakeLabelsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewMapDisableAllMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewIndicatorsWaypointsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewIndicatorsCellTriggersMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editClearUndoRedoMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsRandomizeTilesMenuItem;
        private Controls.ViewToolStripButton selectToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem viewIndicatorsFootballAreaMenuItem;
        private System.Windows.Forms.ToolStripMenuItem statisticsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsStatsGameObjectsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsStatsPowerMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsStatsStorageMenuItem;
        private System.Windows.Forms.ToolStripMenuItem boundariesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewBoundariesBoundariesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewBoundariesMapSymmetryMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileNewFromImageMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsSafeTileDraggingToolStripMenuItem;
    }
}

