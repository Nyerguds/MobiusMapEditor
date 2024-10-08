﻿//
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

namespace MobiusEditor.Dialogs
{
    partial class TriggersDialog
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnSetFilter = new System.Windows.Forms.Button();
            this.settingsPanel = new System.Windows.Forms.Panel();
            this.triggersTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.houseComboBox = new MobiusEditor.Controls.ComboBoxSmartWidth();
            this.typeLabel = new System.Windows.Forms.Label();
            this.event1Label = new System.Windows.Forms.Label();
            this.event2Label = new System.Windows.Forms.Label();
            this.action1Label = new System.Windows.Forms.Label();
            this.action2Label = new System.Windows.Forms.Label();
            this.action1ComboBox = new MobiusEditor.Controls.ComboBoxSmartWidth();
            this.action2ComboBox = new MobiusEditor.Controls.ComboBoxSmartWidth();
            this.persistenceLabel = new System.Windows.Forms.Label();
            this.persistenceComboBox = new MobiusEditor.Controls.ComboBoxSmartWidth();
            this.typeComboBox = new MobiusEditor.Controls.ComboBoxSmartWidth();
            this.event1ComboBox = new MobiusEditor.Controls.ComboBoxSmartWidth();
            this.event2ComboBox = new MobiusEditor.Controls.ComboBoxSmartWidth();
            this.teamLabel = new System.Windows.Forms.Label();
            this.teamComboBox = new MobiusEditor.Controls.ComboBoxSmartWidth();
            this.event1Flp = new System.Windows.Forms.FlowLayoutPanel();
            this.event1Nud = new MobiusEditor.Controls.EnhNumericUpDown();
            this.event1ValueComboBox = new MobiusEditor.Controls.ComboBoxSmartWidth();
            this.event2Flp = new System.Windows.Forms.FlowLayoutPanel();
            this.event2Nud = new MobiusEditor.Controls.EnhNumericUpDown();
            this.event2ValueComboBox = new MobiusEditor.Controls.ComboBoxSmartWidth();
            this.action1Flp = new System.Windows.Forms.FlowLayoutPanel();
            this.action1Nud = new MobiusEditor.Controls.EnhNumericUpDown();
            this.action1ValueComboBox = new MobiusEditor.Controls.ComboBoxSmartWidth();
            this.action2Flp = new System.Windows.Forms.FlowLayoutPanel();
            this.action2Nud = new MobiusEditor.Controls.EnhNumericUpDown();
            this.action2ValueComboBox = new MobiusEditor.Controls.ComboBoxSmartWidth();
            this.triggersListView = new System.Windows.Forms.ListView();
            this.nameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.lblTooLong = new System.Windows.Forms.Label();
            this.btnCheck = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblFilterDetails = new System.Windows.Forms.Label();
            this.triggersContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiAddTrigger = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRenameTrigger = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCloneTrigger = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRemoveTrigger = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.settingsPanel.SuspendLayout();
            this.triggersTableLayoutPanel.SuspendLayout();
            this.event1Flp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.event1Nud)).BeginInit();
            this.event2Flp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.event2Nud)).BeginInit();
            this.action1Flp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.action1Nud)).BeginInit();
            this.action2Flp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.action2Nud)).BeginInit();
            this.pnlButtons.SuspendLayout();
            this.triggersContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.btnAdd, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnSetFilter, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.settingsPanel, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.triggersListView, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pnlButtons, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblFilterDetails, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(554, 461);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // btnAdd
            // 
            this.btnAdd.AutoSize = true;
            this.btnAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAdd.Location = new System.Drawing.Point(2, 423);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(2);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(138, 36);
            this.btnAdd.TabIndex = 35;
            this.btnAdd.Text = "&Add Trigger";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
            // 
            // btnSetFilter
            // 
            this.btnSetFilter.AutoSize = true;
            this.btnSetFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSetFilter.Location = new System.Drawing.Point(2, 396);
            this.btnSetFilter.Margin = new System.Windows.Forms.Padding(2);
            this.btnSetFilter.Name = "btnSetFilter";
            this.btnSetFilter.Size = new System.Drawing.Size(138, 23);
            this.btnSetFilter.TabIndex = 4;
            this.btnSetFilter.Text = "Set &Filter";
            this.btnSetFilter.UseVisualStyleBackColor = true;
            this.btnSetFilter.Click += new System.EventHandler(this.BtnSetFilter_Click);
            // 
            // settingsPanel
            // 
            this.settingsPanel.Controls.Add(this.triggersTableLayoutPanel);
            this.settingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsPanel.Location = new System.Drawing.Point(152, 10);
            this.settingsPanel.Margin = new System.Windows.Forms.Padding(10);
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.Size = new System.Drawing.Size(392, 374);
            this.settingsPanel.TabIndex = 2;
            // 
            // triggersTableLayoutPanel
            // 
            this.triggersTableLayoutPanel.AutoSize = true;
            this.triggersTableLayoutPanel.ColumnCount = 5;
            this.triggersTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.triggersTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.triggersTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.triggersTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.triggersTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.triggersTableLayoutPanel.Controls.Add(this.label1, 0, 0);
            this.triggersTableLayoutPanel.Controls.Add(this.houseComboBox, 1, 0);
            this.triggersTableLayoutPanel.Controls.Add(this.typeLabel, 0, 2);
            this.triggersTableLayoutPanel.Controls.Add(this.event1Label, 0, 3);
            this.triggersTableLayoutPanel.Controls.Add(this.event2Label, 0, 4);
            this.triggersTableLayoutPanel.Controls.Add(this.action1Label, 0, 5);
            this.triggersTableLayoutPanel.Controls.Add(this.action2Label, 0, 6);
            this.triggersTableLayoutPanel.Controls.Add(this.action1ComboBox, 1, 5);
            this.triggersTableLayoutPanel.Controls.Add(this.action2ComboBox, 1, 6);
            this.triggersTableLayoutPanel.Controls.Add(this.persistenceLabel, 0, 1);
            this.triggersTableLayoutPanel.Controls.Add(this.persistenceComboBox, 1, 1);
            this.triggersTableLayoutPanel.Controls.Add(this.typeComboBox, 1, 2);
            this.triggersTableLayoutPanel.Controls.Add(this.event1ComboBox, 1, 3);
            this.triggersTableLayoutPanel.Controls.Add(this.event2ComboBox, 1, 4);
            this.triggersTableLayoutPanel.Controls.Add(this.teamLabel, 0, 7);
            this.triggersTableLayoutPanel.Controls.Add(this.teamComboBox, 1, 7);
            this.triggersTableLayoutPanel.Controls.Add(this.event1Flp, 2, 3);
            this.triggersTableLayoutPanel.Controls.Add(this.event2Flp, 2, 4);
            this.triggersTableLayoutPanel.Controls.Add(this.action1Flp, 2, 5);
            this.triggersTableLayoutPanel.Controls.Add(this.action2Flp, 2, 6);
            this.triggersTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.triggersTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.triggersTableLayoutPanel.Margin = new System.Windows.Forms.Padding(2);
            this.triggersTableLayoutPanel.Name = "triggersTableLayoutPanel";
            this.triggersTableLayoutPanel.RowCount = 17;
            this.triggersTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.triggersTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.triggersTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.triggersTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.triggersTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.triggersTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.triggersTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.triggersTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.triggersTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.triggersTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.triggersTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.triggersTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.triggersTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.triggersTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.triggersTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.triggersTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.triggersTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.triggersTableLayoutPanel.Size = new System.Drawing.Size(392, 374);
            this.triggersTableLayoutPanel.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(2, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.label1.Size = new System.Drawing.Size(67, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "House";
            // 
            // houseComboBox
            // 
            this.triggersTableLayoutPanel.SetColumnSpan(this.houseComboBox, 2);
            this.houseComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.houseComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.houseComboBox.FormattingEnabled = true;
            this.houseComboBox.Location = new System.Drawing.Point(73, 2);
            this.houseComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.houseComboBox.Name = "houseComboBox";
            this.houseComboBox.Size = new System.Drawing.Size(240, 21);
            this.houseComboBox.TabIndex = 1;
            // 
            // typeLabel
            // 
            this.typeLabel.AutoSize = true;
            this.typeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.typeLabel.Location = new System.Drawing.Point(2, 50);
            this.typeLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.typeLabel.Name = "typeLabel";
            this.typeLabel.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.typeLabel.Size = new System.Drawing.Size(67, 25);
            this.typeLabel.TabIndex = 6;
            this.typeLabel.Text = "Type";
            // 
            // event1Label
            // 
            this.event1Label.AutoSize = true;
            this.event1Label.Dock = System.Windows.Forms.DockStyle.Fill;
            this.event1Label.Location = new System.Drawing.Point(2, 75);
            this.event1Label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.event1Label.Name = "event1Label";
            this.event1Label.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.event1Label.Size = new System.Drawing.Size(67, 50);
            this.event1Label.TabIndex = 9;
            this.event1Label.Text = "Event 1";
            // 
            // event2Label
            // 
            this.event2Label.AutoSize = true;
            this.event2Label.Dock = System.Windows.Forms.DockStyle.Fill;
            this.event2Label.Location = new System.Drawing.Point(2, 125);
            this.event2Label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.event2Label.Name = "event2Label";
            this.event2Label.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.event2Label.Size = new System.Drawing.Size(67, 50);
            this.event2Label.TabIndex = 12;
            this.event2Label.Text = "Event 2";
            // 
            // action1Label
            // 
            this.action1Label.AutoSize = true;
            this.action1Label.Dock = System.Windows.Forms.DockStyle.Fill;
            this.action1Label.Location = new System.Drawing.Point(2, 175);
            this.action1Label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.action1Label.Name = "action1Label";
            this.action1Label.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.action1Label.Size = new System.Drawing.Size(67, 50);
            this.action1Label.TabIndex = 15;
            this.action1Label.Text = "Action 1";
            // 
            // action2Label
            // 
            this.action2Label.AutoSize = true;
            this.action2Label.Dock = System.Windows.Forms.DockStyle.Fill;
            this.action2Label.Location = new System.Drawing.Point(2, 225);
            this.action2Label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.action2Label.Name = "action2Label";
            this.action2Label.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.action2Label.Size = new System.Drawing.Size(67, 50);
            this.action2Label.TabIndex = 18;
            this.action2Label.Text = "Action 2";
            // 
            // action1ComboBox
            // 
            this.action1ComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.action1ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.action1ComboBox.FormattingEnabled = true;
            this.action1ComboBox.Location = new System.Drawing.Point(73, 177);
            this.action1ComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.action1ComboBox.Name = "action1ComboBox";
            this.action1ComboBox.Size = new System.Drawing.Size(141, 21);
            this.action1ComboBox.TabIndex = 16;
            this.action1ComboBox.SelectedIndexChanged += new System.EventHandler(this.Action1ComboBox_SelectedIndexChanged);
            this.action1ComboBox.MouseEnter += new System.EventHandler(this.Action1ComboBox_MouseEnter);
            this.action1ComboBox.MouseLeave += new System.EventHandler(this.ToolTipComboBox_MouseLeave);
            this.action1ComboBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Action1ComboBox_MouseMove);
            // 
            // action2ComboBox
            // 
            this.action2ComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.action2ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.action2ComboBox.FormattingEnabled = true;
            this.action2ComboBox.Location = new System.Drawing.Point(73, 227);
            this.action2ComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.action2ComboBox.Name = "action2ComboBox";
            this.action2ComboBox.Size = new System.Drawing.Size(141, 21);
            this.action2ComboBox.TabIndex = 19;
            this.action2ComboBox.SelectedIndexChanged += new System.EventHandler(this.Action2ComboBox_SelectedIndexChanged);
            // 
            // persistenceLabel
            // 
            this.persistenceLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.persistenceLabel.Location = new System.Drawing.Point(2, 25);
            this.persistenceLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.persistenceLabel.Name = "persistenceLabel";
            this.persistenceLabel.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.persistenceLabel.Size = new System.Drawing.Size(67, 25);
            this.persistenceLabel.TabIndex = 3;
            this.persistenceLabel.Text = "Executes";
            // 
            // persistenceComboBox
            // 
            this.triggersTableLayoutPanel.SetColumnSpan(this.persistenceComboBox, 2);
            this.persistenceComboBox.DisplayMember = "Label";
            this.persistenceComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.persistenceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.persistenceComboBox.FormattingEnabled = true;
            this.persistenceComboBox.Location = new System.Drawing.Point(73, 27);
            this.persistenceComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.persistenceComboBox.Name = "persistenceComboBox";
            this.persistenceComboBox.Size = new System.Drawing.Size(240, 21);
            this.persistenceComboBox.TabIndex = 4;
            this.persistenceComboBox.ValueMember = "Value";
            // 
            // typeComboBox
            // 
            this.triggersTableLayoutPanel.SetColumnSpan(this.typeComboBox, 2);
            this.typeComboBox.DisplayMember = "Label";
            this.typeComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.typeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.typeComboBox.FormattingEnabled = true;
            this.typeComboBox.Location = new System.Drawing.Point(73, 52);
            this.typeComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.typeComboBox.Name = "typeComboBox";
            this.typeComboBox.Size = new System.Drawing.Size(240, 21);
            this.typeComboBox.TabIndex = 7;
            this.typeComboBox.ValueMember = "Value";
            this.typeComboBox.SelectedValueChanged += new System.EventHandler(this.TypeComboBox_SelectedValueChanged);
            // 
            // event1ComboBox
            // 
            this.event1ComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.event1ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.event1ComboBox.FormattingEnabled = true;
            this.event1ComboBox.Location = new System.Drawing.Point(73, 77);
            this.event1ComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.event1ComboBox.Name = "event1ComboBox";
            this.event1ComboBox.Size = new System.Drawing.Size(141, 21);
            this.event1ComboBox.TabIndex = 10;
            this.event1ComboBox.SelectedIndexChanged += new System.EventHandler(this.Event1ComboBox_SelectedIndexChanged);
            // 
            // event2ComboBox
            // 
            this.event2ComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.event2ComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.event2ComboBox.FormattingEnabled = true;
            this.event2ComboBox.Location = new System.Drawing.Point(73, 127);
            this.event2ComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.event2ComboBox.Name = "event2ComboBox";
            this.event2ComboBox.Size = new System.Drawing.Size(141, 21);
            this.event2ComboBox.TabIndex = 13;
            this.event2ComboBox.SelectedIndexChanged += new System.EventHandler(this.Event2ComboBox_SelectedIndexChanged);
            // 
            // teamLabel
            // 
            this.teamLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.teamLabel.Location = new System.Drawing.Point(2, 275);
            this.teamLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.teamLabel.Name = "teamLabel";
            this.teamLabel.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.teamLabel.Size = new System.Drawing.Size(67, 25);
            this.teamLabel.TabIndex = 21;
            this.teamLabel.Text = "Team";
            // 
            // teamComboBox
            // 
            this.triggersTableLayoutPanel.SetColumnSpan(this.teamComboBox, 2);
            this.teamComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.teamComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.teamComboBox.FormattingEnabled = true;
            this.teamComboBox.Location = new System.Drawing.Point(73, 277);
            this.teamComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.teamComboBox.Name = "teamComboBox";
            this.teamComboBox.Size = new System.Drawing.Size(240, 21);
            this.teamComboBox.TabIndex = 22;
            this.teamComboBox.SelectedIndexChanged += new System.EventHandler(this.TeamComboBox_SelectedIndexChanged);
            this.teamComboBox.MouseEnter += new System.EventHandler(this.TeamComboBox_MouseEnter);
            this.teamComboBox.MouseLeave += new System.EventHandler(this.ToolTipComboBox_MouseLeave);
            this.teamComboBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TeamComboBox_MouseMove);
            // 
            // event1Flp
            // 
            this.event1Flp.AutoSize = true;
            this.event1Flp.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.triggersTableLayoutPanel.SetColumnSpan(this.event1Flp, 2);
            this.event1Flp.Controls.Add(this.event1Nud);
            this.event1Flp.Controls.Add(this.event1ValueComboBox);
            this.event1Flp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.event1Flp.Location = new System.Drawing.Point(216, 75);
            this.event1Flp.Margin = new System.Windows.Forms.Padding(0);
            this.event1Flp.Name = "event1Flp";
            this.event1Flp.Size = new System.Drawing.Size(170, 50);
            this.event1Flp.TabIndex = 11;
            // 
            // event1Nud
            // 
            this.event1Nud.Location = new System.Drawing.Point(2, 2);
            this.event1Nud.Margin = new System.Windows.Forms.Padding(2, 2, 2, 3);
            this.event1Nud.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.event1Nud.Minimum = new decimal(new int[] {
            -2147483648,
            0,
            0,
            -2147483648});
            this.event1Nud.Name = "event1Nud";
            this.event1Nud.Size = new System.Drawing.Size(95, 20);
            this.event1Nud.TabIndex = 0;
            this.event1Nud.ValueChanged += new System.EventHandler(this.Event1Nud_ValueChanged);
            // 
            // event1ValueComboBox
            // 
            this.event1ValueComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.event1ValueComboBox.FormattingEnabled = true;
            this.event1ValueComboBox.Location = new System.Drawing.Point(2, 27);
            this.event1ValueComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.event1ValueComboBox.Name = "event1ValueComboBox";
            this.event1ValueComboBox.Size = new System.Drawing.Size(166, 21);
            this.event1ValueComboBox.TabIndex = 1;
            // 
            // event2Flp
            // 
            this.event2Flp.AutoSize = true;
            this.triggersTableLayoutPanel.SetColumnSpan(this.event2Flp, 2);
            this.event2Flp.Controls.Add(this.event2Nud);
            this.event2Flp.Controls.Add(this.event2ValueComboBox);
            this.event2Flp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.event2Flp.Location = new System.Drawing.Point(216, 125);
            this.event2Flp.Margin = new System.Windows.Forms.Padding(0);
            this.event2Flp.Name = "event2Flp";
            this.event2Flp.Size = new System.Drawing.Size(170, 50);
            this.event2Flp.TabIndex = 14;
            // 
            // event2Nud
            // 
            this.event2Nud.Location = new System.Drawing.Point(2, 2);
            this.event2Nud.Margin = new System.Windows.Forms.Padding(2, 2, 2, 3);
            this.event2Nud.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.event2Nud.Minimum = new decimal(new int[] {
            -2147483648,
            0,
            0,
            -2147483648});
            this.event2Nud.Name = "event2Nud";
            this.event2Nud.Size = new System.Drawing.Size(95, 20);
            this.event2Nud.TabIndex = 0;
            this.event2Nud.ValueChanged += new System.EventHandler(this.Event2Nud_ValueChanged);
            // 
            // event2ValueComboBox
            // 
            this.event2ValueComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.event2ValueComboBox.FormattingEnabled = true;
            this.event2ValueComboBox.Location = new System.Drawing.Point(2, 27);
            this.event2ValueComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.event2ValueComboBox.Name = "event2ValueComboBox";
            this.event2ValueComboBox.Size = new System.Drawing.Size(166, 21);
            this.event2ValueComboBox.TabIndex = 1;
            // 
            // action1Flp
            // 
            this.action1Flp.AutoSize = true;
            this.triggersTableLayoutPanel.SetColumnSpan(this.action1Flp, 2);
            this.action1Flp.Controls.Add(this.action1Nud);
            this.action1Flp.Controls.Add(this.action1ValueComboBox);
            this.action1Flp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.action1Flp.Location = new System.Drawing.Point(216, 175);
            this.action1Flp.Margin = new System.Windows.Forms.Padding(0);
            this.action1Flp.Name = "action1Flp";
            this.action1Flp.Size = new System.Drawing.Size(170, 50);
            this.action1Flp.TabIndex = 17;
            // 
            // action1Nud
            // 
            this.action1Nud.Location = new System.Drawing.Point(2, 2);
            this.action1Nud.Margin = new System.Windows.Forms.Padding(2, 2, 2, 3);
            this.action1Nud.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.action1Nud.Minimum = new decimal(new int[] {
            -2147483648,
            0,
            0,
            -2147483648});
            this.action1Nud.Name = "action1Nud";
            this.action1Nud.Size = new System.Drawing.Size(95, 20);
            this.action1Nud.TabIndex = 0;
            this.action1Nud.ValueChanged += new System.EventHandler(this.Action1Nud_ValueChanged);
            // 
            // action1ValueComboBox
            // 
            this.action1ValueComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.action1ValueComboBox.FormattingEnabled = true;
            this.action1ValueComboBox.Location = new System.Drawing.Point(2, 27);
            this.action1ValueComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.action1ValueComboBox.Name = "action1ValueComboBox";
            this.action1ValueComboBox.Size = new System.Drawing.Size(166, 21);
            this.action1ValueComboBox.TabIndex = 1;
            this.action1ValueComboBox.SelectedIndexChanged += new System.EventHandler(this.Action1ValueComboBox_SelectedIndexChanged);
            this.action1ValueComboBox.MouseEnter += new System.EventHandler(this.Action1ValueComboBox_MouseEnter);
            this.action1ValueComboBox.MouseLeave += new System.EventHandler(this.ToolTipComboBox_MouseLeave);
            this.action1ValueComboBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Action1ValueComboBox_MouseMove);
            // 
            // action2Flp
            // 
            this.action2Flp.AutoSize = true;
            this.triggersTableLayoutPanel.SetColumnSpan(this.action2Flp, 2);
            this.action2Flp.Controls.Add(this.action2Nud);
            this.action2Flp.Controls.Add(this.action2ValueComboBox);
            this.action2Flp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.action2Flp.Location = new System.Drawing.Point(216, 225);
            this.action2Flp.Margin = new System.Windows.Forms.Padding(0);
            this.action2Flp.Name = "action2Flp";
            this.action2Flp.Size = new System.Drawing.Size(170, 50);
            this.action2Flp.TabIndex = 20;
            // 
            // action2Nud
            // 
            this.action2Nud.Location = new System.Drawing.Point(2, 2);
            this.action2Nud.Margin = new System.Windows.Forms.Padding(2, 2, 2, 3);
            this.action2Nud.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.action2Nud.Minimum = new decimal(new int[] {
            -2147483648,
            0,
            0,
            -2147483648});
            this.action2Nud.Name = "action2Nud";
            this.action2Nud.Size = new System.Drawing.Size(95, 20);
            this.action2Nud.TabIndex = 0;
            this.action2Nud.ValueChanged += new System.EventHandler(this.Action2Nud_ValueChanged);
            // 
            // action2ValueComboBox
            // 
            this.action2ValueComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.action2ValueComboBox.FormattingEnabled = true;
            this.action2ValueComboBox.Location = new System.Drawing.Point(2, 27);
            this.action2ValueComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.action2ValueComboBox.Name = "action2ValueComboBox";
            this.action2ValueComboBox.Size = new System.Drawing.Size(166, 21);
            this.action2ValueComboBox.TabIndex = 1;
            this.action2ValueComboBox.SelectedIndexChanged += new System.EventHandler(this.Action2ValueComboBox_SelectedIndexChanged);
            this.action2ValueComboBox.MouseEnter += new System.EventHandler(this.Action2ValueComboBox_MouseEnter);
            this.action2ValueComboBox.MouseLeave += new System.EventHandler(this.ToolTipComboBox_MouseLeave);
            this.action2ValueComboBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Action2ValueComboBox_MouseMove);
            // 
            // triggersListView
            // 
            this.triggersListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumnHeader});
            this.triggersListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.triggersListView.FullRowSelect = true;
            this.triggersListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.triggersListView.HideSelection = false;
            this.triggersListView.LabelEdit = true;
            this.triggersListView.Location = new System.Drawing.Point(2, 2);
            this.triggersListView.Margin = new System.Windows.Forms.Padding(2);
            this.triggersListView.MultiSelect = false;
            this.triggersListView.Name = "triggersListView";
            this.triggersListView.ShowItemToolTips = true;
            this.triggersListView.Size = new System.Drawing.Size(138, 390);
            this.triggersListView.TabIndex = 0;
            this.triggersListView.UseCompatibleStateImageBehavior = false;
            this.triggersListView.View = System.Windows.Forms.View.Details;
            this.triggersListView.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.triggersListView_AfterLabelEdit);
            this.triggersListView.SelectedIndexChanged += new System.EventHandler(this.triggersListView_SelectedIndexChanged);
            this.triggersListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.triggersListView_KeyDown);
            this.triggersListView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.triggersListView_MouseDown);
            // 
            // nameColumnHeader
            // 
            this.nameColumnHeader.Width = 110;
            // 
            // pnlButtons
            // 
            this.pnlButtons.Controls.Add(this.lblTooLong);
            this.pnlButtons.Controls.Add(this.btnCheck);
            this.pnlButtons.Controls.Add(this.btnOK);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButtons.Location = new System.Drawing.Point(145, 424);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(406, 34);
            this.pnlButtons.TabIndex = 3;
            // 
            // lblTooLong
            // 
            this.lblTooLong.AutoSize = true;
            this.lblTooLong.ForeColor = System.Drawing.Color.Red;
            this.lblTooLong.Location = new System.Drawing.Point(9, 11);
            this.lblTooLong.Name = "lblTooLong";
            this.lblTooLong.Size = new System.Drawing.Size(164, 13);
            this.lblTooLong.TabIndex = 33;
            this.lblTooLong.Text = "Trigger length exceeds maximum!";
            this.lblTooLong.Visible = false;
            // 
            // btnCheck
            // 
            this.btnCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheck.Location = new System.Drawing.Point(201, 2);
            this.btnCheck.Margin = new System.Windows.Forms.Padding(2);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(75, 30);
            this.btnCheck.TabIndex = 32;
            this.btnCheck.Text = "Check...";
            this.btnCheck.UseVisualStyleBackColor = true;
            this.btnCheck.Click += new System.EventHandler(this.btnCheck_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.AutoSize = true;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(282, 2);
            this.btnOK.Margin = new System.Windows.Forms.Padding(2);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(50, 30);
            this.btnOK.TabIndex = 30;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.AutoSize = true;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(336, 2);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(68, 30);
            this.btnCancel.TabIndex = 31;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblFilterDetails
            // 
            this.lblFilterDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFilterDetails.Location = new System.Drawing.Point(145, 394);
            this.lblFilterDetails.Name = "lblFilterDetails";
            this.lblFilterDetails.Size = new System.Drawing.Size(406, 27);
            this.lblFilterDetails.TabIndex = 34;
            this.lblFilterDetails.Text = "[filter details]";
            this.lblFilterDetails.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // triggersContextMenuStrip
            // 
            this.triggersContextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.triggersContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAddTrigger,
            this.tsmiRenameTrigger,
            this.tsmiCloneTrigger,
            this.tsmiRemoveTrigger});
            this.triggersContextMenuStrip.Name = "teamTypesContextMenuStrip";
            this.triggersContextMenuStrip.Size = new System.Drawing.Size(191, 92);
            // 
            // tsmiAddTrigger
            // 
            this.tsmiAddTrigger.Name = "tsmiAddTrigger";
            this.tsmiAddTrigger.Size = new System.Drawing.Size(190, 22);
            this.tsmiAddTrigger.Text = "&Add Trigger (Ctrl+A)";
            this.tsmiAddTrigger.Click += new System.EventHandler(this.TsmiAddTrigger_Click);
            // 
            // tsmiRenameTrigger
            // 
            this.tsmiRenameTrigger.Name = "tsmiRenameTrigger";
            this.tsmiRenameTrigger.Size = new System.Drawing.Size(190, 22);
            this.tsmiRenameTrigger.Text = "Re&name Trigger (F2)";
            this.tsmiRenameTrigger.Click += new System.EventHandler(this.TsmiRenameTrigger_Click);
            // 
            // tsmiCloneTrigger
            // 
            this.tsmiCloneTrigger.Name = "tsmiCloneTrigger";
            this.tsmiCloneTrigger.Size = new System.Drawing.Size(190, 22);
            this.tsmiCloneTrigger.Text = "&Clone Trigger (Ctrl+C)";
            this.tsmiCloneTrigger.Click += new System.EventHandler(this.TsmiCloneTrigger_Click);
            // 
            // tsmiRemoveTrigger
            // 
            this.tsmiRemoveTrigger.Name = "tsmiRemoveTrigger";
            this.tsmiRemoveTrigger.Size = new System.Drawing.Size(190, 22);
            this.tsmiRemoveTrigger.Text = "&Remove Trigger (Del)";
            this.tsmiRemoveTrigger.Click += new System.EventHandler(this.TsmiRemoveTrigger_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.UseAnimation = false;
            this.toolTip1.UseFading = false;
            // 
            // TriggersDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(554, 461);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = global::MobiusEditor.Properties.Resources.GameIcon00;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(570, 400);
            this.Name = "TriggersDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Triggers";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TriggersDialog_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TriggersDialog_KeyDown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.settingsPanel.ResumeLayout(false);
            this.settingsPanel.PerformLayout();
            this.triggersTableLayoutPanel.ResumeLayout(false);
            this.triggersTableLayoutPanel.PerformLayout();
            this.event1Flp.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.event1Nud)).EndInit();
            this.event2Flp.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.event2Nud)).EndInit();
            this.action1Flp.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.action1Nud)).EndInit();
            this.action2Flp.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.action2Nud)).EndInit();
            this.pnlButtons.ResumeLayout(false);
            this.pnlButtons.PerformLayout();
            this.triggersContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Panel settingsPanel;
        private System.Windows.Forms.ListView triggersListView;
        private System.Windows.Forms.ColumnHeader nameColumnHeader;
        private System.Windows.Forms.ContextMenuStrip triggersContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem tsmiAddTrigger;
        private System.Windows.Forms.ToolStripMenuItem tsmiRemoveTrigger;
        private System.Windows.Forms.TableLayoutPanel triggersTableLayoutPanel;
        private System.Windows.Forms.Label label1;
        private MobiusEditor.Controls.ComboBoxSmartWidth houseComboBox;
        private System.Windows.Forms.Label typeLabel;
        private System.Windows.Forms.Label event1Label;
        private System.Windows.Forms.Label event2Label;
        private System.Windows.Forms.Label action2Label;
        private MobiusEditor.Controls.ComboBoxSmartWidth action2ComboBox;
        private System.Windows.Forms.Label persistenceLabel;
        private MobiusEditor.Controls.ComboBoxSmartWidth persistenceComboBox;
        private MobiusEditor.Controls.ComboBoxSmartWidth typeComboBox;
        private MobiusEditor.Controls.ComboBoxSmartWidth event1ComboBox;
        private MobiusEditor.Controls.ComboBoxSmartWidth event2ComboBox;
        private MobiusEditor.Controls.ComboBoxSmartWidth action1ComboBox;
        private System.Windows.Forms.Label action1Label;
        private System.Windows.Forms.Label teamLabel;
        private MobiusEditor.Controls.ComboBoxSmartWidth teamComboBox;
        private System.Windows.Forms.FlowLayoutPanel event1Flp;
        private MobiusEditor.Controls.EnhNumericUpDown event1Nud;
        private System.Windows.Forms.FlowLayoutPanel event2Flp;
        private MobiusEditor.Controls.EnhNumericUpDown event2Nud;
        private System.Windows.Forms.FlowLayoutPanel action1Flp;
        private MobiusEditor.Controls.EnhNumericUpDown action1Nud;
        private System.Windows.Forms.FlowLayoutPanel action2Flp;
        private MobiusEditor.Controls.EnhNumericUpDown action2Nud;
        private MobiusEditor.Controls.ComboBoxSmartWidth event1ValueComboBox;
        private MobiusEditor.Controls.ComboBoxSmartWidth event2ValueComboBox;
        private MobiusEditor.Controls.ComboBoxSmartWidth action1ValueComboBox;
        private MobiusEditor.Controls.ComboBoxSmartWidth action2ValueComboBox;
        private System.Windows.Forms.ToolStripMenuItem tsmiRenameTrigger;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnCheck;
        private System.Windows.Forms.Label lblTooLong;
        private System.Windows.Forms.ToolStripMenuItem tsmiCloneTrigger;
        private System.Windows.Forms.Button btnSetFilter;
        private System.Windows.Forms.Label lblFilterDetails;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}