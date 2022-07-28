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
using System;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    partial class TeamTypesDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAdd = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.settingsPanel = new System.Windows.Forms.Panel();
            this.teamTypeTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.houseComboBox = new System.Windows.Forms.ComboBox();
            this.roundaboutCheckBox = new System.Windows.Forms.CheckBox();
            this.learningCheckBox = new System.Windows.Forms.CheckBox();
            this.suicideCheckBox = new System.Windows.Forms.CheckBox();
            this.autocreateCheckBox = new System.Windows.Forms.CheckBox();
            this.mercernaryCheckBox = new System.Windows.Forms.CheckBox();
            this.reinforcableCheckBox = new System.Windows.Forms.CheckBox();
            this.prebuiltCheckBox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.initNumNud = new MobiusEditor.Controls.EnhNumericUpDown();
            this.maxAllowedNud = new MobiusEditor.Controls.EnhNumericUpDown();
            this.fearNud = new MobiusEditor.Controls.EnhNumericUpDown();
            this.waypointLabel = new System.Windows.Forms.Label();
            this.triggerLabel = new System.Windows.Forms.Label();
            this.waypointComboBox = new System.Windows.Forms.ComboBox();
            this.triggerComboBox = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.recruitPriorityNud = new MobiusEditor.Controls.EnhNumericUpDown();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.teamsDataGridView = new System.Windows.Forms.DataGridView();
            this.teamsTypeColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.teamsCountColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.missionsDataGridView = new System.Windows.Forms.DataGridView();
            this.missionsMissionColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.missionsArgumentColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.teamTypesListView = new System.Windows.Forms.ListView();
            this.nameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.teamTypesContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addTeamTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameTeamTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeTeamTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.settingsPanel.SuspendLayout();
            this.teamTypeTableLayoutPanel.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.initNumNud)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxAllowedNud)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fearNud)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.recruitPriorityNud)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.teamsDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.missionsDataGridView)).BeginInit();
            this.teamTypesContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.settingsPanel, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.teamTypesListView, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(875, 416);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel2.Controls.Add(this.btnAdd);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 379);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(169, 34);
            this.flowLayoutPanel2.TabIndex = 1;
            // 
            // btnAdd
            // 
            this.btnAdd.AutoSize = true;
            this.btnAdd.Location = new System.Drawing.Point(2, 2);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(2);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(168, 30);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "&Add Teamtype";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.btnCancel);
            this.flowLayoutPanel1.Controls.Add(this.btnOK);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(178, 379);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(694, 34);
            this.flowLayoutPanel1.TabIndex = 3;
            // 
            // btnCancel
            // 
            this.btnCancel.AutoSize = true;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(624, 2);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(68, 30);
            this.btnCancel.TabIndex = 41;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.AutoSize = true;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(570, 2);
            this.btnOK.Margin = new System.Windows.Forms.Padding(2);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(50, 30);
            this.btnOK.TabIndex = 40;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // settingsPanel
            // 
            this.settingsPanel.Controls.Add(this.teamTypeTableLayoutPanel);
            this.settingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsPanel.Location = new System.Drawing.Point(185, 10);
            this.settingsPanel.Margin = new System.Windows.Forms.Padding(10);
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.Size = new System.Drawing.Size(680, 356);
            this.settingsPanel.TabIndex = 2;
            // 
            // teamTypeTableLayoutPanel
            // 
            this.teamTypeTableLayoutPanel.ColumnCount = 3;
            this.teamTypeTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.teamTypeTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.teamTypeTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.teamTypeTableLayoutPanel.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.teamTypeTableLayoutPanel.Controls.Add(this.tableLayoutPanel3, 2, 0);
            this.teamTypeTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.teamTypeTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.teamTypeTableLayoutPanel.Margin = new System.Windows.Forms.Padding(2);
            this.teamTypeTableLayoutPanel.Name = "teamTypeTableLayoutPanel";
            this.teamTypeTableLayoutPanel.RowCount = 1;
            this.teamTypeTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.teamTypeTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 356F));
            this.teamTypeTableLayoutPanel.Size = new System.Drawing.Size(680, 356);
            this.teamTypeTableLayoutPanel.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.houseComboBox, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.roundaboutCheckBox, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.learningCheckBox, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.suicideCheckBox, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.autocreateCheckBox, 1, 4);
            this.tableLayoutPanel2.Controls.Add(this.mercernaryCheckBox, 1, 5);
            this.tableLayoutPanel2.Controls.Add(this.reinforcableCheckBox, 1, 6);
            this.tableLayoutPanel2.Controls.Add(this.prebuiltCheckBox, 1, 7);
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 9);
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 10);
            this.tableLayoutPanel2.Controls.Add(this.label4, 0, 11);
            this.tableLayoutPanel2.Controls.Add(this.initNumNud, 1, 9);
            this.tableLayoutPanel2.Controls.Add(this.maxAllowedNud, 1, 10);
            this.tableLayoutPanel2.Controls.Add(this.fearNud, 1, 11);
            this.tableLayoutPanel2.Controls.Add(this.waypointLabel, 0, 12);
            this.tableLayoutPanel2.Controls.Add(this.triggerLabel, 0, 13);
            this.tableLayoutPanel2.Controls.Add(this.waypointComboBox, 1, 12);
            this.tableLayoutPanel2.Controls.Add(this.triggerComboBox, 1, 13);
            this.tableLayoutPanel2.Controls.Add(this.label9, 0, 8);
            this.tableLayoutPanel2.Controls.Add(this.recruitPriorityNud, 1, 8);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(2, 2);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 15;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(206, 352);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(2, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "House";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.label1, "Owner of this team.");
            // 
            // houseComboBox
            // 
            this.houseComboBox.DisplayMember = "Name";
            this.houseComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.houseComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.houseComboBox.FormattingEnabled = true;
            this.houseComboBox.Location = new System.Drawing.Point(73, 2);
            this.houseComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.houseComboBox.Name = "houseComboBox";
            this.houseComboBox.Size = new System.Drawing.Size(131, 21);
            this.houseComboBox.TabIndex = 10;
            this.houseComboBox.ValueMember = "Type";
            // 
            // roundaboutCheckBox
            // 
            this.roundaboutCheckBox.AutoSize = true;
            this.roundaboutCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.roundaboutCheckBox.Location = new System.Drawing.Point(73, 27);
            this.roundaboutCheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.roundaboutCheckBox.Name = "roundaboutCheckBox";
            this.roundaboutCheckBox.Size = new System.Drawing.Size(131, 17);
            this.roundaboutCheckBox.TabIndex = 11;
            this.roundaboutCheckBox.Text = "Roundabout";
            this.toolTip1.SetToolTip(this.roundaboutCheckBox, "Avoid high-threat areas.");
            this.roundaboutCheckBox.UseVisualStyleBackColor = true;
            // 
            // learningCheckBox
            // 
            this.learningCheckBox.AutoSize = true;
            this.learningCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.learningCheckBox.Location = new System.Drawing.Point(73, 48);
            this.learningCheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.learningCheckBox.Name = "learningCheckBox";
            this.learningCheckBox.Size = new System.Drawing.Size(131, 17);
            this.learningCheckBox.TabIndex = 12;
            this.learningCheckBox.Text = "Learning";
            this.toolTip1.SetToolTip(this.learningCheckBox, "The team learns from mistakes.");
            this.learningCheckBox.UseVisualStyleBackColor = true;
            // 
            // suicideCheckBox
            // 
            this.suicideCheckBox.AutoSize = true;
            this.suicideCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.suicideCheckBox.Location = new System.Drawing.Point(73, 69);
            this.suicideCheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.suicideCheckBox.Name = "suicideCheckBox";
            this.suicideCheckBox.Size = new System.Drawing.Size(131, 17);
            this.suicideCheckBox.TabIndex = 13;
            this.suicideCheckBox.Text = "Suicide";
            this.toolTip1.SetToolTip(this.suicideCheckBox, "The team won\'t stop until it achieves its mission or it\'s dead.");
            this.suicideCheckBox.UseVisualStyleBackColor = true;
            // 
            // autocreateCheckBox
            // 
            this.autocreateCheckBox.AutoSize = true;
            this.autocreateCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.autocreateCheckBox.Location = new System.Drawing.Point(73, 90);
            this.autocreateCheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.autocreateCheckBox.Name = "autocreateCheckBox";
            this.autocreateCheckBox.Size = new System.Drawing.Size(131, 17);
            this.autocreateCheckBox.TabIndex = 14;
            this.autocreateCheckBox.Text = "Auto-create";
            this.toolTip1.SetToolTip(this.autocreateCheckBox, "Make this part of the pool of teams to be produced\r\nwhen enabling Autocreate for " +
        "this House.");
            this.autocreateCheckBox.UseVisualStyleBackColor = true;
            // 
            // mercernaryCheckBox
            // 
            this.mercernaryCheckBox.AutoSize = true;
            this.mercernaryCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mercernaryCheckBox.Location = new System.Drawing.Point(73, 111);
            this.mercernaryCheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.mercernaryCheckBox.Name = "mercernaryCheckBox";
            this.mercernaryCheckBox.Size = new System.Drawing.Size(131, 17);
            this.mercernaryCheckBox.TabIndex = 15;
            this.mercernaryCheckBox.Text = "Mercernary";
            this.toolTip1.SetToolTip(this.mercernaryCheckBox, "Will change sides if they start to lose.");
            this.mercernaryCheckBox.UseVisualStyleBackColor = true;
            // 
            // reinforcableCheckBox
            // 
            this.reinforcableCheckBox.AutoSize = true;
            this.reinforcableCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.reinforcableCheckBox.Location = new System.Drawing.Point(73, 132);
            this.reinforcableCheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.reinforcableCheckBox.Name = "reinforcableCheckBox";
            this.reinforcableCheckBox.Size = new System.Drawing.Size(131, 17);
            this.reinforcableCheckBox.TabIndex = 16;
            this.reinforcableCheckBox.Text = "Reinforcable";
            this.toolTip1.SetToolTip(this.reinforcableCheckBox, "Allow recruitment of new members if some die.\r\nIf false, acts similar to Suicide," +
        " but they will defend themselves.");
            this.reinforcableCheckBox.UseVisualStyleBackColor = true;
            // 
            // prebuiltCheckBox
            // 
            this.prebuiltCheckBox.AutoSize = true;
            this.prebuiltCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.prebuiltCheckBox.Location = new System.Drawing.Point(73, 153);
            this.prebuiltCheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.prebuiltCheckBox.Name = "prebuiltCheckBox";
            this.prebuiltCheckBox.Size = new System.Drawing.Size(131, 17);
            this.prebuiltCheckBox.TabIndex = 17;
            this.prebuiltCheckBox.Text = "Prebuilt";
            this.toolTip1.SetToolTip(this.prebuiltCheckBox, "Computer should build members to fill a team of this type\r\nregardless of whether " +
        "there is a team of this type active.");
            this.prebuiltCheckBox.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(2, 196);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 24);
            this.label2.TabIndex = 9;
            this.label2.Text = "Init Num";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.label2, "Initial amount of this type of team.");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(2, 220);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 24);
            this.label3.TabIndex = 10;
            this.label3.Text = "Max Allowed";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.label3, "Maximum amount of this type of team allowed at one time.");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(2, 244);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 24);
            this.label4.TabIndex = 11;
            this.label4.Text = "Fear";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.label4, "Fear level of this team. No known effect.");
            // 
            // initNumNud
            // 
            this.initNumNud.Dock = System.Windows.Forms.DockStyle.Fill;
            this.initNumNud.EnteredValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.initNumNud.Location = new System.Drawing.Point(73, 198);
            this.initNumNud.Margin = new System.Windows.Forms.Padding(2);
            this.initNumNud.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.initNumNud.Name = "initNumNud";
            this.initNumNud.SelectedText = "";
            this.initNumNud.SelectionLength = 0;
            this.initNumNud.SelectionStart = 0;
            this.initNumNud.Size = new System.Drawing.Size(131, 20);
            this.initNumNud.TabIndex = 19;
            // 
            // maxAllowedNud
            // 
            this.maxAllowedNud.Dock = System.Windows.Forms.DockStyle.Fill;
            this.maxAllowedNud.EnteredValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.maxAllowedNud.Location = new System.Drawing.Point(73, 222);
            this.maxAllowedNud.Margin = new System.Windows.Forms.Padding(2);
            this.maxAllowedNud.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.maxAllowedNud.Name = "maxAllowedNud";
            this.maxAllowedNud.SelectedText = "";
            this.maxAllowedNud.SelectionLength = 0;
            this.maxAllowedNud.SelectionStart = 0;
            this.maxAllowedNud.Size = new System.Drawing.Size(131, 20);
            this.maxAllowedNud.TabIndex = 20;
            // 
            // fearNud
            // 
            this.fearNud.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fearNud.EnteredValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.fearNud.Location = new System.Drawing.Point(73, 246);
            this.fearNud.Margin = new System.Windows.Forms.Padding(2);
            this.fearNud.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.fearNud.Name = "fearNud";
            this.fearNud.SelectedText = "";
            this.fearNud.SelectionLength = 0;
            this.fearNud.SelectionStart = 0;
            this.fearNud.Size = new System.Drawing.Size(131, 20);
            this.fearNud.TabIndex = 21;
            // 
            // waypointLabel
            // 
            this.waypointLabel.AutoSize = true;
            this.waypointLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waypointLabel.Location = new System.Drawing.Point(2, 268);
            this.waypointLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.waypointLabel.Name = "waypointLabel";
            this.waypointLabel.Size = new System.Drawing.Size(67, 25);
            this.waypointLabel.TabIndex = 15;
            this.waypointLabel.Text = "Waypoint";
            this.waypointLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // triggerLabel
            // 
            this.triggerLabel.AutoSize = true;
            this.triggerLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.triggerLabel.Location = new System.Drawing.Point(2, 293);
            this.triggerLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.triggerLabel.Name = "triggerLabel";
            this.triggerLabel.Size = new System.Drawing.Size(67, 25);
            this.triggerLabel.TabIndex = 16;
            this.triggerLabel.Text = "Trigger";
            this.triggerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // waypointComboBox
            // 
            this.waypointComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waypointComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.waypointComboBox.FormattingEnabled = true;
            this.waypointComboBox.Location = new System.Drawing.Point(73, 270);
            this.waypointComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.waypointComboBox.Name = "waypointComboBox";
            this.waypointComboBox.Size = new System.Drawing.Size(131, 21);
            this.waypointComboBox.TabIndex = 22;
            // 
            // triggerComboBox
            // 
            this.triggerComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.triggerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.triggerComboBox.FormattingEnabled = true;
            this.triggerComboBox.Location = new System.Drawing.Point(73, 295);
            this.triggerComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.triggerComboBox.Name = "triggerComboBox";
            this.triggerComboBox.Size = new System.Drawing.Size(131, 21);
            this.triggerComboBox.TabIndex = 23;
            // 
            // label9
            // 
            this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label9.Location = new System.Drawing.Point(2, 172);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(67, 24);
            this.label9.TabIndex = 19;
            this.label9.Text = "Priority";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.label9, "Priority given the team for recruiting purposes; higher\r\npriority means it can st" +
        "eal members from other teams.");
            // 
            // recruitPriorityNud
            // 
            this.recruitPriorityNud.Dock = System.Windows.Forms.DockStyle.Fill;
            this.recruitPriorityNud.EnteredValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.recruitPriorityNud.Location = new System.Drawing.Point(73, 174);
            this.recruitPriorityNud.Margin = new System.Windows.Forms.Padding(2);
            this.recruitPriorityNud.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.recruitPriorityNud.Minimum = new decimal(new int[] {
            -2147483648,
            0,
            0,
            -2147483648});
            this.recruitPriorityNud.Name = "recruitPriorityNud";
            this.recruitPriorityNud.SelectedText = "";
            this.recruitPriorityNud.SelectionLength = 0;
            this.recruitPriorityNud.SelectionStart = 0;
            this.recruitPriorityNud.Size = new System.Drawing.Size(131, 20);
            this.recruitPriorityNud.TabIndex = 18;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.label7, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.label8, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.teamsDataGridView, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.missionsDataGridView, 1, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(212, 2);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(466, 352);
            this.tableLayoutPanel3.TabIndex = 24;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(2, 0);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(39, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Teams";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(235, 0);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(38, 13);
            this.label8.TabIndex = 1;
            this.label8.Text = "Orders";
            // 
            // teamsDataGridView
            // 
            this.teamsDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.teamsDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.teamsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.teamsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.teamsTypeColumn,
            this.teamsCountColumn});
            this.teamsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.teamsDataGridView.Location = new System.Drawing.Point(2, 15);
            this.teamsDataGridView.Margin = new System.Windows.Forms.Padding(2);
            this.teamsDataGridView.Name = "teamsDataGridView";
            this.teamsDataGridView.RowTemplate.Height = 28;
            this.teamsDataGridView.Size = new System.Drawing.Size(229, 335);
            this.teamsDataGridView.TabIndex = 30;
            this.teamsDataGridView.VirtualMode = true;
            this.teamsDataGridView.CancelRowEdit += new System.Windows.Forms.QuestionEventHandler(this.teamsDataGridView_CancelRowEdit);
            this.teamsDataGridView.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.teamsDataGridView_CellEnter);
            this.teamsDataGridView.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.teamsDataGridView_CellMouseDown);
            this.teamsDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(teamsDataGridView_CellValueChanged);
            this.teamsDataGridView.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.teamsDataGridView_CellValueNeeded);
            this.teamsDataGridView.CellValuePushed += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.teamsDataGridView_CellValuePushed);
            this.teamsDataGridView.CurrentCellDirtyStateChanged += new System.EventHandler(this.teamsDataGridView_CurrentCellDirtyStateChanged);
            this.teamsDataGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DataGridView_DataError);
            this.teamsDataGridView.NewRowNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.teamsDataGridView_NewRowNeeded);
            this.teamsDataGridView.RowDirtyStateNeeded += new System.Windows.Forms.QuestionEventHandler(this.teamsDataGridView_RowDirtyStateNeeded);
            this.teamsDataGridView.RowValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.teamsDataGridView_RowValidated);
            this.teamsDataGridView.UserAddedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.teamsDataGridView_UserAddedRow);
            this.teamsDataGridView.UserDeletedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.teamsDataGridView_UserDeletedRow);
            this.teamsDataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.teamsDataGridView_UserDeletingRow);
            this.teamsDataGridView.Leave += new System.EventHandler(this.teamsDataGridView_Leave);
            // 
            // teamsTypeColumn
            // 
            this.teamsTypeColumn.HeaderText = "Type";
            this.teamsTypeColumn.Name = "teamsTypeColumn";
            this.teamsTypeColumn.Width = 39;
            // 
            // teamsCountColumn
            // 
            this.teamsCountColumn.HeaderText = "Count";
            this.teamsCountColumn.Name = "teamsCountColumn";
            this.teamsCountColumn.Width = 60;
            // 
            // missionsDataGridView
            // 
            this.missionsDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.missionsDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.missionsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.missionsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.missionsMissionColumn,
            this.missionsArgumentColumn});
            this.missionsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.missionsDataGridView.Location = new System.Drawing.Point(235, 15);
            this.missionsDataGridView.Margin = new System.Windows.Forms.Padding(2);
            this.missionsDataGridView.Name = "missionsDataGridView";
            this.missionsDataGridView.RowTemplate.Height = 28;
            this.missionsDataGridView.Size = new System.Drawing.Size(229, 335);
            this.missionsDataGridView.TabIndex = 31;
            this.missionsDataGridView.VirtualMode = true;
            this.missionsDataGridView.CancelRowEdit += new System.Windows.Forms.QuestionEventHandler(this.missionsDataGridView_CancelRowEdit);
            this.missionsDataGridView.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.missionsDataGridView_CellEnter);
            this.missionsDataGridView.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.missionsDataGridView_CellMouseDown);
            this.missionsDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.missionsDataGridView_CellValueChanged);
            this.missionsDataGridView.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.missionsDataGridView_CellValueNeeded);
            this.missionsDataGridView.CellValuePushed += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.missionsDataGridView_CellValuePushed);
            this.missionsDataGridView.CurrentCellDirtyStateChanged += new System.EventHandler(this.missionsDataGridView_CurrentCellDirtyStateChanged);
            this.missionsDataGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DataGridView_DataError);
            this.missionsDataGridView.NewRowNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.missionsDataGridView_NewRowNeeded);
            this.missionsDataGridView.RowDirtyStateNeeded += new System.Windows.Forms.QuestionEventHandler(this.missionsDataGridView_RowDirtyStateNeeded);
            this.missionsDataGridView.RowValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.missionsDataGridView_RowValidated);
            this.missionsDataGridView.UserAddedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.missionsDataGridView_UserAddedRow);
            this.missionsDataGridView.UserDeletedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.missionsDataGridView_UserDeletedRow);
            this.missionsDataGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.missionsDataGridView_UserDeletingRow);
            this.missionsDataGridView.Leave += new System.EventHandler(this.missionsDataGridView_Leave);
            // 
            // missionsMissionColumn
            // 
            this.missionsMissionColumn.HeaderText = "Order";
            this.missionsMissionColumn.Name = "missionsMissionColumn";
            this.missionsMissionColumn.Width = 39;
            // 
            // missionsArgumentColumn
            // 
            this.missionsArgumentColumn.HeaderText = "Argument";
            this.missionsArgumentColumn.Name = "missionsArgumentColumn";
            this.missionsArgumentColumn.Width = 77;
            // 
            // teamTypesListView
            // 
            this.teamTypesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumnHeader});
            this.teamTypesListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.teamTypesListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.teamTypesListView.HideSelection = false;
            this.teamTypesListView.LabelEdit = true;
            this.teamTypesListView.Location = new System.Drawing.Point(2, 2);
            this.teamTypesListView.Margin = new System.Windows.Forms.Padding(2);
            this.teamTypesListView.MultiSelect = false;
            this.teamTypesListView.Name = "teamTypesListView";
            this.teamTypesListView.ShowItemToolTips = true;
            this.teamTypesListView.Size = new System.Drawing.Size(171, 372);
            this.teamTypesListView.TabIndex = 0;
            this.teamTypesListView.UseCompatibleStateImageBehavior = false;
            this.teamTypesListView.View = System.Windows.Forms.View.Details;
            this.teamTypesListView.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.teamTypesListView_AfterLabelEdit);
            this.teamTypesListView.SelectedIndexChanged += new System.EventHandler(this.teamTypesListView_SelectedIndexChanged);
            this.teamTypesListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.teamTypesListView_KeyDown);
            this.teamTypesListView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.teamTypesListView_MouseDown);
            // 
            // teamTypesContextMenuStrip
            // 
            this.teamTypesContextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.teamTypesContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addTeamTypeToolStripMenuItem,
            this.renameTeamTypeToolStripMenuItem,
            this.removeTeamTypeToolStripMenuItem});
            this.teamTypesContextMenuStrip.Name = "teamTypesContextMenuStrip";
            this.teamTypesContextMenuStrip.Size = new System.Drawing.Size(204, 70);
            // 
            // addTeamTypeToolStripMenuItem
            // 
            this.addTeamTypeToolStripMenuItem.Name = "addTeamTypeToolStripMenuItem";
            this.addTeamTypeToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.addTeamTypeToolStripMenuItem.Text = "&Add Team Type (Ctrl+A)";
            this.addTeamTypeToolStripMenuItem.Click += new System.EventHandler(this.addTeamTypeToolStripMenuItem_Click);
            // 
            // renameTeamTypeToolStripMenuItem
            // 
            this.renameTeamTypeToolStripMenuItem.Name = "renameTeamTypeToolStripMenuItem";
            this.renameTeamTypeToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.renameTeamTypeToolStripMenuItem.Text = "Re&name Team Type (F2)";
            this.renameTeamTypeToolStripMenuItem.Click += new System.EventHandler(this.renameTeamTypeToolStripMenuItem_Click);
            // 
            // removeTeamTypeToolStripMenuItem
            // 
            this.removeTeamTypeToolStripMenuItem.Name = "removeTeamTypeToolStripMenuItem";
            this.removeTeamTypeToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.removeTeamTypeToolStripMenuItem.Text = "&Remove Team Type (Del)";
            this.removeTeamTypeToolStripMenuItem.Click += new System.EventHandler(this.removeTeamTypeToolStripMenuItem_Click);
            // 
            // TeamTypesDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(875, 416);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(650, 370);
            this.Name = "TeamTypesDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Team Types";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TeamTypesDialog_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TeamTypesDialog_KeyDown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.settingsPanel.ResumeLayout(false);
            this.teamTypeTableLayoutPanel.ResumeLayout(false);
            this.teamTypeTableLayoutPanel.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.initNumNud)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxAllowedNud)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fearNud)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.recruitPriorityNud)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.teamsDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.missionsDataGridView)).EndInit();
            this.teamTypesContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Panel settingsPanel;
        private System.Windows.Forms.ListView teamTypesListView;
        private System.Windows.Forms.ColumnHeader nameColumnHeader;
        private System.Windows.Forms.ContextMenuStrip teamTypesContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem addTeamTypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeTeamTypeToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel teamTypeTableLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox houseComboBox;
        private System.Windows.Forms.CheckBox roundaboutCheckBox;
        private System.Windows.Forms.CheckBox learningCheckBox;
        private System.Windows.Forms.CheckBox suicideCheckBox;
        private System.Windows.Forms.CheckBox autocreateCheckBox;
        private System.Windows.Forms.CheckBox mercernaryCheckBox;
        private System.Windows.Forms.CheckBox reinforcableCheckBox;
        private System.Windows.Forms.CheckBox prebuiltCheckBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private MobiusEditor.Controls.EnhNumericUpDown initNumNud;
        private MobiusEditor.Controls.EnhNumericUpDown maxAllowedNud;
        private MobiusEditor.Controls.EnhNumericUpDown fearNud;
        private System.Windows.Forms.Label waypointLabel;
        private System.Windows.Forms.Label triggerLabel;
        private System.Windows.Forms.ComboBox waypointComboBox;
        private System.Windows.Forms.ComboBox triggerComboBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DataGridView teamsDataGridView;
        private System.Windows.Forms.DataGridView missionsDataGridView;
        private System.Windows.Forms.Label label9;
        private MobiusEditor.Controls.EnhNumericUpDown recruitPriorityNud;
        private System.Windows.Forms.DataGridViewComboBoxColumn teamsTypeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn teamsCountColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn missionsMissionColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn missionsArgumentColumn;
        private FlowLayoutPanel flowLayoutPanel2;
        private Button btnAdd;
        private ToolStripMenuItem renameTeamTypeToolStripMenuItem;
        private ToolTip toolTip1;
    }
}