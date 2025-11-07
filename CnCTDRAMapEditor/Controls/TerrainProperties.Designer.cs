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
namespace MobiusEditor.Controls
{
    partial class TerrainProperties
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbTrigger = new MobiusEditor.Controls.PropertiesComboBox();
            this.lblTriggerTypesInfo = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 31.25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 68.75F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.cmbTrigger, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblTriggerTypesInfo, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(230, 38);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 28);
            this.label1.TabIndex = 4;
            this.label1.Text = "Trigger";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // triggerComboBox
            // 
            this.cmbTrigger.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbTrigger.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTrigger.FormattingEnabled = true;
            this.cmbTrigger.Location = new System.Drawing.Point(66, 3);
            this.cmbTrigger.Name = "cmbTrigger";
            this.cmbTrigger.Size = new System.Drawing.Size(132, 28);
            this.cmbTrigger.TabIndex = 9;
            this.cmbTrigger.SelectedIndexChanged += new System.EventHandler(this.TriggerComboBox_SelectedValueChanged);
            this.cmbTrigger.MouseEnter += new System.EventHandler(this.TriggerComboBox_MouseEnter);
            this.cmbTrigger.MouseLeave += new System.EventHandler(this.HideToolTip);
            this.cmbTrigger.MouseMove += new System.Windows.Forms.MouseEventHandler(TriggerComboBox_MouseMove);
            // 
            // lblTriggerTypesInfo
            // 
            this.lblTriggerTypesInfo.AutoSize = true;
            this.lblTriggerTypesInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTriggerTypesInfo.Location = new System.Drawing.Point(201, 0);
            this.lblTriggerTypesInfo.Margin = new System.Windows.Forms.Padding(0);
            this.lblTriggerTypesInfo.Name = "lblTriggerTypesInfo";
            this.lblTriggerTypesInfo.Size = new System.Drawing.Size(29, 27);
            this.lblTriggerTypesInfo.TabIndex = 10;
            this.lblTriggerTypesInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTriggerTypesInfo.MouseEnter += new System.EventHandler(this.LblTriggerTypesInfo_MouseEnter);
            this.lblTriggerTypesInfo.MouseLeave += new System.EventHandler(this.HideToolTip);
            this.lblTriggerTypesInfo.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LblTriggerTypesInfo_MouseMove);
            // 
            // TerrainProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "TerrainProperties";
            this.Size = new System.Drawing.Size(230, 38);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private MobiusEditor.Controls.PropertiesComboBox cmbTrigger;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label lblTriggerTypesInfo;
    }
}
