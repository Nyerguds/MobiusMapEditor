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
    partial class WaypointsToolDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WaypointsToolDialog));
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.lblWaypoint = new System.Windows.Forms.Label();
            this.waypointCombo = new MobiusEditor.Controls.ComboBoxSmartWidth();
            this.btnJumpTo = new System.Windows.Forms.Button();
            this.tableLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.AutoSize = true;
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.Controls.Add(this.lblWaypoint, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.waypointCombo, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.btnJumpTo, 1, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.Size = new System.Drawing.Size(145, 77);
            this.tableLayoutPanel4.TabIndex = 3;
            // 
            // lblWaypoint
            // 
            this.lblWaypoint.AutoSize = true;
            this.lblWaypoint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblWaypoint.Location = new System.Drawing.Point(3, 0);
            this.lblWaypoint.Name = "lblWaypoint";
            this.lblWaypoint.Size = new System.Drawing.Size(52, 27);
            this.lblWaypoint.TabIndex = 0;
            this.lblWaypoint.Text = "Waypoint";
            this.lblWaypoint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // waypointCombo
            // 
            this.waypointCombo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waypointCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.waypointCombo.FormattingEnabled = true;
            this.waypointCombo.Location = new System.Drawing.Point(61, 3);
            this.waypointCombo.Name = "waypointCombo";
            this.waypointCombo.Size = new System.Drawing.Size(81, 21);
            this.waypointCombo.TabIndex = 1;
            // 
            // btnJumpTo
            // 
            this.btnJumpTo.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnJumpTo.Location = new System.Drawing.Point(61, 30);
            this.btnJumpTo.Name = "btnJumpTo";
            this.btnJumpTo.Size = new System.Drawing.Size(81, 23);
            this.btnJumpTo.TabIndex = 2;
            this.btnJumpTo.Text = "Jump to...";
            this.btnJumpTo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnJumpTo.UseVisualStyleBackColor = true;
            // 
            // WaypointsToolDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(200, 77);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel4);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(145, 73);
            this.Name = "WaypointsToolDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Waypoints";
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label lblWaypoint;
        private MobiusEditor.Controls.ComboBoxSmartWidth waypointCombo;
        private System.Windows.Forms.Button btnJumpTo;
    }
}