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
    partial class ObjectToolDialog<T>
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
            CustomComponentResourceManager resources = new CustomComponentResourceManager(typeof(ObjectToolDialog<>), "ObjectToolDialog");
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.objectTypeListBox = new MobiusEditor.Controls.TypeListBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.objectTypeMapPanel = new MobiusEditor.Controls.MapPanel();
            this.objectProperties = new MobiusEditor.Controls.ObjectProperties();
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
            this.tableLayoutPanel1.Controls.Add(this.objectTypeListBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(484, 461);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // objectTypeListBox
            // 
            this.objectTypeListBox.DisplayMember = "Name";
            this.objectTypeListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.objectTypeListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.objectTypeListBox.FormattingEnabled = true;
            this.objectTypeListBox.Location = new System.Drawing.Point(4, 4);
            this.objectTypeListBox.Margin = new System.Windows.Forms.Padding(4);
            this.objectTypeListBox.MissingThumbnail = ((System.Drawing.Image)(resources.GetObject("objectTypeListBox.MissingThumbnail")));
            this.objectTypeListBox.Name = "objectTypeListBox";
            this.objectTypeListBox.Size = new System.Drawing.Size(234, 453);
            this.objectTypeListBox.TabIndex = 2;
            this.objectTypeListBox.ValueMember = "Type";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.objectTypeMapPanel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.objectProperties, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(246, 4);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(234, 453);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // objectTypeMapPanel
            // 
            this.objectTypeMapPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.objectTypeMapPanel.Location = new System.Drawing.Point(4, 4);
            this.objectTypeMapPanel.MapImage = null;
            this.objectTypeMapPanel.Margin = new System.Windows.Forms.Padding(4);
            this.objectTypeMapPanel.MaxZoom = 8D;
            this.objectTypeMapPanel.MinZoom = 1D;
            this.objectTypeMapPanel.Name = "objectTypeMapPanel";
            this.objectTypeMapPanel.Size = new System.Drawing.Size(226, 203);
            this.objectTypeMapPanel.SmoothScale = false;
            this.objectTypeMapPanel.TabIndex = 3;
            this.objectTypeMapPanel.Zoom = 1D;
            this.objectTypeMapPanel.ZoomStep = 1D;
            // 
            // objectProperties
            // 
            this.objectProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.objectProperties.Location = new System.Drawing.Point(5, 216);
            this.objectProperties.Margin = new System.Windows.Forms.Padding(5);
            this.objectProperties.MinimumSize = new System.Drawing.Size(230, 213);
            this.objectProperties.Name = "objectProperties";
            this.objectProperties.Object = null;
            this.objectProperties.Size = new System.Drawing.Size(230, 232);
            this.objectProperties.TabIndex = 4;
            // 
            // ObjectToolDialog
            // 
            this.ClientSize = new System.Drawing.Size(484, 461);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 500);
            this.Name = "ObjectToolDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Map";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Controls.TypeListBox objectTypeListBox;
        private Controls.MapPanel objectTypeMapPanel;
        private Controls.ObjectProperties objectProperties;
    }
}