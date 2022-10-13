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
namespace MobiusEditor.Controls
{
    partial class BriefingSettings
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtBriefing = new System.Windows.Forms.TextBox();
            this.lblBriefing = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtBriefing
            // 
            this.txtBriefing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtBriefing.Location = new System.Drawing.Point(2, 23);
            this.txtBriefing.Margin = new System.Windows.Forms.Padding(2, 10, 2, 2);
            this.txtBriefing.Multiline = true;
            this.txtBriefing.Name = "txtBriefing";
            this.txtBriefing.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBriefing.Size = new System.Drawing.Size(396, 399);
            this.txtBriefing.TabIndex = 1;
            // 
            // lblBriefing
            // 
            this.lblBriefing.AutoSize = true;
            this.lblBriefing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBriefing.Location = new System.Drawing.Point(3, 0);
            this.lblBriefing.Name = "lblBriefing";
            this.lblBriefing.Size = new System.Drawing.Size(394, 13);
            this.lblBriefing.TabIndex = 0;
            this.lblBriefing.Text = "Enter mission briefing text here:";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lblBriefing, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtBriefing, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(400, 424);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // BriefingSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "BriefingSettings";
            this.Size = new System.Drawing.Size(400, 424);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox txtBriefing;
        private System.Windows.Forms.Label lblBriefing;
    }
}
