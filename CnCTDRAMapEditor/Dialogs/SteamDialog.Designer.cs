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
namespace MobiusEditor.Dialogs
{
    partial class SteamDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SteamDialog));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnFromBriefing = new System.Windows.Forms.Button();
            this.lblMapTitle = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnCopyFromMap = new System.Windows.Forms.Button();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.lblMapTitleData = new System.Windows.Forms.Label();
            this.lblSteamTitle = new System.Windows.Forms.Label();
            this.lblVisibility = new System.Windows.Forms.Label();
            this.lblPreview = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.cmbVisibility = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtPreview = new System.Windows.Forms.TextBox();
            this.btnDefaultPreview = new System.Windows.Forms.Button();
            this.btnPreview = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnGoToSteam = new System.Windows.Forms.Button();
            this.publicMapContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.publishAsNewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblLegal = new System.Windows.Forms.Label();
            this.btnPublishMap = new MobiusEditor.Controls.MenuButton();
            this.imageTooltip = new MobiusEditor.Controls.ImageTooltip();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.publicMapContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblLegal, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(484, 311);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.lblMapTitle, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.panel3, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.lblMapTitleData, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblSteamTitle, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.lblVisibility, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.lblPreview, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.lblDescription, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.txtDescription, 0, 5);
            this.tableLayoutPanel2.Controls.Add(this.cmbVisibility, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.panel1, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.btnFromBriefing, 1, 4);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(2, 2);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 6;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(482, 159);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // btnFromBriefing
            // 
            this.btnFromBriefing.AutoSize = true;
            this.btnFromBriefing.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnFromBriefing.Location = new System.Drawing.Point(353, 105);
            this.btnFromBriefing.Margin = new System.Windows.Forms.Padding(2);
            this.btnFromBriefing.Name = "btnFromBriefing";
            this.btnFromBriefing.Size = new System.Drawing.Size(127, 23);
            this.btnFromBriefing.TabIndex = 41;
            this.btnFromBriefing.Text = "Copy from briefing";
            this.btnFromBriefing.UseVisualStyleBackColor = true;
            this.btnFromBriefing.Click += new System.EventHandler(this.BtnGenerateDescription_Click);
            // 
            // lblMapTitle
            // 
            this.lblMapTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMapTitle.Location = new System.Drawing.Point(2, 0);
            this.lblMapTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMapTitle.Name = "lblMapTitle";
            this.lblMapTitle.Size = new System.Drawing.Size(69, 24);
            this.lblMapTitle.TabIndex = 0;
            this.lblMapTitle.Text = "Map Title";
            this.lblMapTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnCopyFromMap);
            this.panel3.Controls.Add(this.txtTitle);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(75, 26);
            this.panel3.Margin = new System.Windows.Forms.Padding(2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(405, 23);
            this.panel3.TabIndex = 11;
            // 
            // btnCopyFromMap
            // 
            this.btnCopyFromMap.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopyFromMap.AutoSize = true;
            this.btnCopyFromMap.Location = new System.Drawing.Point(317, 0);
            this.btnCopyFromMap.Margin = new System.Windows.Forms.Padding(2);
            this.btnCopyFromMap.Name = "btnCopyFromMap";
            this.btnCopyFromMap.Size = new System.Drawing.Size(87, 24);
            this.btnCopyFromMap.TabIndex = 1;
            this.btnCopyFromMap.Text = "Copy from map";
            this.btnCopyFromMap.UseVisualStyleBackColor = true;
            this.btnCopyFromMap.Click += new System.EventHandler(this.BtnCopyFromMap_Click);
            // 
            // txtTitle
            // 
            this.txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTitle.Location = new System.Drawing.Point(0, 2);
            this.txtTitle.Margin = new System.Windows.Forms.Padding(2);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(313, 20);
            this.txtTitle.TabIndex = 0;
            // 
            // lblMapTitleData
            // 
            this.lblMapTitleData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMapTitleData.Location = new System.Drawing.Point(75, 0);
            this.lblMapTitleData.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMapTitleData.Name = "lblMapTitleData";
            this.lblMapTitleData.Size = new System.Drawing.Size(405, 24);
            this.lblMapTitleData.TabIndex = 1;
            this.lblMapTitleData.Text = "[MAPTITLE]";
            this.lblMapTitleData.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSteamTitle
            // 
            this.lblSteamTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSteamTitle.Location = new System.Drawing.Point(2, 24);
            this.lblSteamTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblSteamTitle.Name = "lblSteamTitle";
            this.lblSteamTitle.Size = new System.Drawing.Size(69, 27);
            this.lblSteamTitle.TabIndex = 10;
            this.lblSteamTitle.Text = "Steam Title";
            this.lblSteamTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblVisibility
            // 
            this.lblVisibility.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblVisibility.Location = new System.Drawing.Point(2, 51);
            this.lblVisibility.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblVisibility.Name = "lblVisibility";
            this.lblVisibility.Size = new System.Drawing.Size(69, 25);
            this.lblVisibility.TabIndex = 20;
            this.lblVisibility.Text = "Visibility";
            this.lblVisibility.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPreview
            // 
            this.lblPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPreview.Location = new System.Drawing.Point(2, 76);
            this.lblPreview.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new System.Drawing.Size(69, 27);
            this.lblPreview.TabIndex = 30;
            this.lblPreview.Text = "Preview";
            this.lblPreview.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblDescription
            // 
            this.lblDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDescription.Location = new System.Drawing.Point(2, 103);
            this.lblDescription.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(69, 27);
            this.lblDescription.TabIndex = 40;
            this.lblDescription.Text = "Description";
            this.lblDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtDescription
            // 
            this.tableLayoutPanel2.SetColumnSpan(this.txtDescription, 2);
            this.txtDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDescription.Location = new System.Drawing.Point(2, 132);
            this.txtDescription.Margin = new System.Windows.Forms.Padding(2);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(478, 27);
            this.txtDescription.TabIndex = 50;
            // 
            // cmbVisibility
            // 
            this.cmbVisibility.DisplayMember = "Label";
            this.cmbVisibility.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbVisibility.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbVisibility.FormattingEnabled = true;
            this.cmbVisibility.Location = new System.Drawing.Point(75, 53);
            this.cmbVisibility.Margin = new System.Windows.Forms.Padding(2);
            this.cmbVisibility.Name = "cmbVisibility";
            this.cmbVisibility.Size = new System.Drawing.Size(405, 21);
            this.cmbVisibility.TabIndex = 21;
            this.cmbVisibility.ValueMember = "Value";
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.txtPreview);
            this.panel1.Controls.Add(this.btnDefaultPreview);
            this.panel1.Controls.Add(this.btnPreview);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(75, 78);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(405, 23);
            this.panel1.TabIndex = 31;
            // 
            // txtPreview
            // 
            this.txtPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPreview.Location = new System.Drawing.Point(2, 2);
            this.txtPreview.Margin = new System.Windows.Forms.Padding(2);
            this.txtPreview.Name = "txtPreview";
            this.txtPreview.ReadOnly = true;
            this.txtPreview.Size = new System.Drawing.Size(318, 20);
            this.txtPreview.TabIndex = 0;
            this.txtPreview.TextChanged += new System.EventHandler(this.TxtPreview_TextChanged);
            this.txtPreview.MouseEnter += new System.EventHandler(this.TxtPreview_MouseEnter);
            this.txtPreview.MouseLeave += new System.EventHandler(this.TxtPreview_MouseLeave);
            // 
            // btnDefaultPreview
            // 
            this.btnDefaultPreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDefaultPreview.AutoSize = true;
            this.btnDefaultPreview.Location = new System.Drawing.Point(352, 0);
            this.btnDefaultPreview.Margin = new System.Windows.Forms.Padding(2);
            this.btnDefaultPreview.Name = "btnDefaultPreview";
            this.btnDefaultPreview.Size = new System.Drawing.Size(51, 23);
            this.btnDefaultPreview.TabIndex = 2;
            this.btnDefaultPreview.Text = "Default";
            this.btnDefaultPreview.UseVisualStyleBackColor = true;
            this.btnDefaultPreview.Click += new System.EventHandler(this.btnDefaultPreview_Click);
            // 
            // btnPreview
            // 
            this.btnPreview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPreview.AutoSize = true;
            this.btnPreview.Location = new System.Drawing.Point(324, 0);
            this.btnPreview.Margin = new System.Windows.Forms.Padding(2);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(26, 23);
            this.btnPreview.TabIndex = 1;
            this.btnPreview.Text = "...";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.previewBtn_Click);
            // 
            // panel2
            // 
            this.panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel2.Controls.Add(this.btnClose);
            this.panel2.Controls.Add(this.btnGoToSteam);
            this.panel2.Controls.Add(this.btnPublishMap);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.lblStatus);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(2, 165);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(482, 34);
            this.panel2.TabIndex = 60;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.AutoSize = true;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(412, 2);
            this.btnClose.Margin = new System.Windows.Forms.Padding(2);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(68, 30);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnGoToSteam
            // 
            this.btnGoToSteam.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGoToSteam.Location = new System.Drawing.Point(280, 2);
            this.btnGoToSteam.Margin = new System.Windows.Forms.Padding(2);
            this.btnGoToSteam.Name = "btnGoToSteam";
            this.btnGoToSteam.Size = new System.Drawing.Size(128, 30);
            this.btnGoToSteam.TabIndex = 3;
            this.btnGoToSteam.Text = "Go to &Steam Workshop";
            this.btnGoToSteam.UseVisualStyleBackColor = true;
            this.btnGoToSteam.Click += new System.EventHandler(this.btnGoToSteam_Click);
            // 
            // publicMapContextMenuStrip
            // 
            this.publicMapContextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.publicMapContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.publishAsNewToolStripMenuItem});
            this.publicMapContextMenuStrip.Name = "publicMapContextMenuStrip";
            this.publicMapContextMenuStrip.Size = new System.Drawing.Size(157, 26);
            // 
            // publishAsNewToolStripMenuItem
            // 
            this.publishAsNewToolStripMenuItem.Name = "publishAsNewToolStripMenuItem";
            this.publishAsNewToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.publishAsNewToolStripMenuItem.Text = "Publish As New";
            this.publishAsNewToolStripMenuItem.Click += new System.EventHandler(this.publishAsNewToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Status:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.ForeColor = System.Drawing.Color.Red;
            this.lblStatus.Location = new System.Drawing.Point(46, 1);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(115, 31);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblLegal
            // 
            this.lblLegal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLegal.Location = new System.Drawing.Point(3, 201);
            this.lblLegal.Name = "lblLegal";
            this.lblLegal.Size = new System.Drawing.Size(480, 110);
            this.lblLegal.TabIndex = 70;
            this.lblLegal.Text = resources.GetString("lblLegal.Text");
            // 
            // btnPublishMap
            // 
            this.btnPublishMap.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPublishMap.AutoSize = true;
            this.btnPublishMap.Location = new System.Drawing.Point(165, 2);
            this.btnPublishMap.Margin = new System.Windows.Forms.Padding(2);
            this.btnPublishMap.Menu = this.publicMapContextMenuStrip;
            this.btnPublishMap.Name = "btnPublishMap";
            this.btnPublishMap.Size = new System.Drawing.Size(111, 30);
            this.btnPublishMap.TabIndex = 2;
            this.btnPublishMap.Text = "&Publish Map";
            this.btnPublishMap.UseVisualStyleBackColor = true;
            this.btnPublishMap.Click += new System.EventHandler(this.btnPublishMap_Click);
            // 
            // imageTooltip
            // 
            this.imageTooltip.MaxSize = new System.Drawing.Size(0, 0);
            this.imageTooltip.OwnerDraw = true;
            this.imageTooltip.ShowAlways = true;
            // 
            // SteamDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(484, 311);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = global::MobiusEditor.Properties.Resources.GameIcon00;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 350);
            this.Name = "SteamDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Steam Workshop: Publish Custom Map";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SteamDialog_FormClosing);
            this.Shown += new System.EventHandler(this.SteamDialog_Shown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.publicMapContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label lblSteamTitle;
        private System.Windows.Forms.Label lblVisibility;
        private System.Windows.Forms.Label lblPreview;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.ComboBox cmbVisibility;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txtPreview;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnGoToSteam;
        private MobiusEditor.Controls.MenuButton btnPublishMap;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ContextMenuStrip publicMapContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem publishAsNewToolStripMenuItem;
        private Controls.ImageTooltip imageTooltip;
        private System.Windows.Forms.Label lblLegal;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnCopyFromMap;
        private System.Windows.Forms.Label lblMapTitle;
        private System.Windows.Forms.Label lblMapTitleData;
        private System.Windows.Forms.Button btnFromBriefing;
        private System.Windows.Forms.Button btnDefaultPreview;
        private System.Windows.Forms.Label label1;
    }
}