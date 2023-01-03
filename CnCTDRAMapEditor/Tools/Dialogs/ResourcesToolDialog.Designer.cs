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
namespace MobiusEditor.Tools.Dialogs
{
    partial class ResourcesToolDialog
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
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.lblResTotal = new System.Windows.Forms.Label();
            this.lblResTotalVal = new System.Windows.Forms.Label();
            this.chkGems = new System.Windows.Forms.CheckBox();
            this.nudBrushSize = new MobiusEditor.Controls.EnhNumericUpDown();
            this.lblBrushSize = new System.Windows.Forms.Label();
            this.lblResBounds = new System.Windows.Forms.Label();
            this.lblResBoundsVal = new System.Windows.Forms.Label();
            this.tableLayoutPanel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudBrushSize)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel6.Controls.Add(this.lblResTotal, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.lblResTotalVal, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.chkGems, 0, 4);
            this.tableLayoutPanel6.Controls.Add(this.nudBrushSize, 1, 3);
            this.tableLayoutPanel6.Controls.Add(this.lblBrushSize, 0, 3);
            this.tableLayoutPanel6.Controls.Add(this.lblResBounds, 0, 1);
            this.tableLayoutPanel6.Controls.Add(this.lblResBoundsVal, 1, 1);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 5;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(241, 90);
            this.tableLayoutPanel6.TabIndex = 1;
            // 
            // lblResTotal
            // 
            this.lblResTotal.AutoSize = true;
            this.lblResTotal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblResTotal.Location = new System.Drawing.Point(3, 0);
            this.lblResTotal.Name = "lblResTotal";
            this.lblResTotal.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.lblResTotal.Size = new System.Drawing.Size(130, 17);
            this.lblResTotal.TabIndex = 0;
            this.lblResTotal.Text = "Total Resources:";
            // 
            // lblResTotalVal
            // 
            this.lblResTotalVal.AutoSize = true;
            this.lblResTotalVal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblResTotalVal.Location = new System.Drawing.Point(139, 0);
            this.lblResTotalVal.Name = "lblResTotalVal";
            this.lblResTotalVal.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.lblResTotalVal.Size = new System.Drawing.Size(99, 17);
            this.lblResTotalVal.TabIndex = 1;
            this.lblResTotalVal.Text = "0";
            // 
            // chkGems
            // 
            this.chkGems.AutoSize = true;
            this.tableLayoutPanel6.SetColumnSpan(this.chkGems, 2);
            this.chkGems.Location = new System.Drawing.Point(3, 66);
            this.chkGems.Name = "chkGems";
            this.chkGems.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.chkGems.Size = new System.Drawing.Size(53, 21);
            this.chkGems.TabIndex = 2;
            this.chkGems.Text = "Gems";
            this.chkGems.UseVisualStyleBackColor = true;
            // 
            // nudBrushSize
            // 
            this.nudBrushSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nudBrushSize.EnteredValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudBrushSize.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudBrushSize.IntValue = 1;
            this.nudBrushSize.Location = new System.Drawing.Point(139, 40);
            this.nudBrushSize.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.nudBrushSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudBrushSize.MouseWheelIncrement = 2;
            this.nudBrushSize.Name = "nudBrushSize";
            this.nudBrushSize.SelectedText = "";
            this.nudBrushSize.SelectionLength = 0;
            this.nudBrushSize.SelectionStart = 0;
            this.nudBrushSize.Size = new System.Drawing.Size(99, 20);
            this.nudBrushSize.TabIndex = 3;
            this.nudBrushSize.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblBrushSize
            // 
            this.lblBrushSize.AutoSize = true;
            this.lblBrushSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBrushSize.Location = new System.Drawing.Point(3, 37);
            this.lblBrushSize.Name = "lblBrushSize";
            this.lblBrushSize.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.lblBrushSize.Size = new System.Drawing.Size(130, 26);
            this.lblBrushSize.TabIndex = 4;
            this.lblBrushSize.Text = "Brush Size";
            this.lblBrushSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblResBounds
            // 
            this.lblResBounds.AutoSize = true;
            this.lblResBounds.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblResBounds.Location = new System.Drawing.Point(3, 17);
            this.lblResBounds.Name = "lblResBounds";
            this.lblResBounds.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.lblResBounds.Size = new System.Drawing.Size(130, 20);
            this.lblResBounds.TabIndex = 4;
            this.lblResBounds.Text = "Resources in map bounds";
            this.lblResBounds.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblResBoundsVal
            // 
            this.lblResBoundsVal.AutoSize = true;
            this.lblResBoundsVal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblResBoundsVal.Location = new System.Drawing.Point(139, 17);
            this.lblResBoundsVal.Name = "lblResBoundsVal";
            this.lblResBoundsVal.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.lblResBoundsVal.Size = new System.Drawing.Size(99, 20);
            this.lblResBoundsVal.TabIndex = 1;
            this.lblResBoundsVal.Text = "0";
            // 
            // ResourcesToolDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(241, 90);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel6);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(212, 106);
            this.Name = "ResourcesToolDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Resources";
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudBrushSize)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.Label lblResTotal;
        private System.Windows.Forms.Label lblResTotalVal;
        private System.Windows.Forms.CheckBox chkGems;
        private MobiusEditor.Controls.EnhNumericUpDown nudBrushSize;
        private System.Windows.Forms.Label lblBrushSize;
        private System.Windows.Forms.Label lblResBounds;
        private System.Windows.Forms.Label lblResBoundsVal;
    }
}