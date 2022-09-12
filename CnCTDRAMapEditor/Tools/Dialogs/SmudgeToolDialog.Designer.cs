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
using MobiusEditor.Interface;

namespace MobiusEditor.Tools.Dialogs
{
    partial class SmudgeToolDialog : ToolDialog<SmudgeTool>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SmudgeToolDialog));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.smudgeTypeListBox = new MobiusEditor.Controls.TypeListBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.smudgeTypeMapPanel = new MobiusEditor.Controls.MapPanel();
            this.smudgeProperties = new MobiusEditor.Controls.SmudgeProperties();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.smudgeTypeListBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(504, 320);
            this.tableLayoutPanel1.TabIndex = 0;
            //
            // smudgeTypeListBox
            //
            this.smudgeTypeListBox.DisplayMember = "Name";
            this.smudgeTypeListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.smudgeTypeListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.smudgeTypeListBox.FormattingEnabled = true;
            this.smudgeTypeListBox.Location = new System.Drawing.Point(4, 4);
            this.smudgeTypeListBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.smudgeTypeListBox.MissingThumbnail = ((System.Drawing.Image)(resources.GetObject("smudgeTypeListBox.MissingThumbnail")));
            this.smudgeTypeListBox.Name = "smudgeTypeListBox";
            this.smudgeTypeListBox.Size = new System.Drawing.Size(244, 310);
            this.smudgeTypeListBox.TabIndex = 2;
            this.smudgeTypeListBox.ValueMember = "Type";
            //
            // tableLayoutPanel2
            //
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.smudgeTypeMapPanel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.smudgeProperties, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(256, 4);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(244, 312);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // smudgeTypeMapPanel
            // 
            this.smudgeTypeMapPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.smudgeTypeMapPanel.Location = new System.Drawing.Point(4, 5);
            this.smudgeTypeMapPanel.MapImage = null;
            this.smudgeTypeMapPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.smudgeTypeMapPanel.MaxZoom = 8;
            this.smudgeTypeMapPanel.MinZoom = 1;
            this.smudgeTypeMapPanel.Name = "smudgeTypeMapPanel";
            this.smudgeTypeMapPanel.SmoothScale = false;
            this.smudgeTypeMapPanel.Size = new System.Drawing.Size(236, 200);
            this.smudgeTypeMapPanel.TabIndex = 2;
            this.smudgeTypeMapPanel.Zoom = 1;
            this.smudgeTypeMapPanel.ZoomStep = 1;
            // 
            // smudgeProperties
            // 
            this.smudgeProperties.Dock = System.Windows.Forms.DockStyle.Top;
            this.smudgeProperties.Location = new System.Drawing.Point(4, 215);
            this.smudgeProperties.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.smudgeProperties.Name = "smudgeProperties";
            this.smudgeProperties.Size = new System.Drawing.Size(236, 30);
            this.smudgeProperties.TabIndex = 3;
            // 
            // SmudgeToolDialog
            // 
            this.ClientSize = new System.Drawing.Size(504, 320);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(520, 336);
            this.Name = "SmudgeToolDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Smudge";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Controls.TypeListBox smudgeTypeListBox;
        private Controls.MapPanel smudgeTypeMapPanel;
        private Controls.SmudgeProperties smudgeProperties;
    }
}