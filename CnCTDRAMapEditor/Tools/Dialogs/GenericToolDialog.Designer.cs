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
using MobiusEditor.Utility;

namespace MobiusEditor.Tools.Dialogs
{
    partial class GenericToolDialog<T> : ToolDialog<T> where T : ITool
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
            CustomComponentResourceManager resources = new CustomComponentResourceManager(typeof(GenericToolDialog<>), "GenericToolDialog");
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.genericTypeMapPanel = new MobiusEditor.Controls.MapPanel();
            this.genericTypeListBox = new MobiusEditor.Controls.TypeListBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.genericTypeMapPanel, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.genericTypeListBox, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(474, 254);
            this.tableLayoutPanel1.TabIndex = 0;
            //
            // genericTypeListBox
            //
            this.genericTypeListBox.DisplayMember = "Name";
            this.genericTypeListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.genericTypeListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.genericTypeListBox.FormattingEnabled = true;
            this.genericTypeListBox.Location = new System.Drawing.Point(3, 3);
            this.genericTypeListBox.MissingThumbnail = ((System.Drawing.Image)(resources.GetObject("genericTypeListBox.MissingThumbnail")));
            this.genericTypeListBox.Name = "genericTypeListBox";
            this.genericTypeListBox.Size = new System.Drawing.Size(268, 268);
            this.genericTypeListBox.TabIndex = 2;
            this.genericTypeListBox.ValueMember = "Type";
            // 
            // genericTypeMapPanel
            // 
            this.genericTypeMapPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.genericTypeMapPanel.Location = new System.Drawing.Point(3, 30);
            this.genericTypeMapPanel.MaxZoom = 8;
            this.genericTypeMapPanel.MinZoom = 1;
            this.genericTypeMapPanel.Name = "genericTypeMapPanel";
            this.genericTypeMapPanel.SmoothScale = false;
            this.genericTypeMapPanel.Size = new System.Drawing.Size(268, 221);
            this.genericTypeMapPanel.TabIndex = 3;
            this.genericTypeMapPanel.Zoom = 1;
            // 
            // GenericToolDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 254);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 293);
            this.Size = new System.Drawing.Size(560, 293);
            this.Name = "GenericToolDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Map";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Controls.TypeListBox genericTypeListBox;
        private Controls.MapPanel genericTypeMapPanel;
    }
}