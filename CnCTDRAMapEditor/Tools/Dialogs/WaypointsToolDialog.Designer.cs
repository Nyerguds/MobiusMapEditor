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
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnEditTeam = new System.Windows.Forms.Button();
            this.lblMode = new System.Windows.Forms.Label();
            this.btnJumpToTeam = new System.Windows.Forms.Button();
            this.cmbTeamTypes = new MobiusEditor.Controls.ComboBoxSmartWidth();
            this.lblRoute = new System.Windows.Forms.Label();
            this.lblWaypoint = new System.Windows.Forms.Label();
            this.cmbWaypoints = new MobiusEditor.Controls.ComboBoxSmartWidth();
            this.btnJumpTo = new System.Windows.Forms.Button();
            this.btnModeSwitch = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.AutoSize = true;
            this.tableLayoutPanel.ColumnCount = 3;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel.Controls.Add(this.btnEditTeam, 2, 3);
            this.tableLayoutPanel.Controls.Add(this.lblMode, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.btnJumpToTeam, 1, 4);
            this.tableLayoutPanel.Controls.Add(this.cmbTeamTypes, 1, 3);
            this.tableLayoutPanel.Controls.Add(this.lblRoute, 0, 3);
            this.tableLayoutPanel.Controls.Add(this.lblWaypoint, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.cmbWaypoints, 1, 1);
            this.tableLayoutPanel.Controls.Add(this.btnJumpTo, 1, 2);
            this.tableLayoutPanel.Controls.Add(this.btnModeSwitch, 1, 0);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 6;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(214, 81);
            this.tableLayoutPanel.TabIndex = 3;
            // 
            // btnEditTeam
            // 
            this.btnEditTeam.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnEditTeam.Location = new System.Drawing.Point(176, 83);
            this.btnEditTeam.Margin = new System.Windows.Forms.Padding(2);
            this.btnEditTeam.Name = "btnEditTeam";
            this.btnEditTeam.Size = new System.Drawing.Size(36, 23);
            this.btnEditTeam.TabIndex = 8;
            this.btnEditTeam.Text = "Edit";
            this.btnEditTeam.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnEditTeam.UseVisualStyleBackColor = true;
            this.btnEditTeam.Visible = false;
            this.btnEditTeam.MouseEnter += new System.EventHandler(this.BtnEditTeam_MouseEnter);
            this.btnEditTeam.MouseLeave += new System.EventHandler(this.HideToolTip);
            this.btnEditTeam.MouseMove += new System.Windows.Forms.MouseEventHandler(this.BtnEditTeam_MouseMove);
            // 
            // lblMode
            // 
            this.lblMode.AutoSize = true;
            this.lblMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMode.Location = new System.Drawing.Point(3, 0);
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new System.Drawing.Size(61, 27);
            this.lblMode.TabIndex = 7;
            this.lblMode.Text = "&Mode";
            this.lblMode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnJumpToTeam
            // 
            this.tableLayoutPanel.SetColumnSpan(this.btnJumpToTeam, 2);
            this.btnJumpToTeam.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnJumpToTeam.Location = new System.Drawing.Point(69, 110);
            this.btnJumpToTeam.Margin = new System.Windows.Forms.Padding(2);
            this.btnJumpToTeam.Name = "btnJumpToTeam";
            this.btnJumpToTeam.Size = new System.Drawing.Size(143, 23);
            this.btnJumpToTeam.TabIndex = 5;
            this.btnJumpToTeam.Text = "Jump to first";
            this.btnJumpToTeam.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnJumpToTeam.UseVisualStyleBackColor = true;
            this.btnJumpToTeam.Visible = false;
            // 
            // cmbTeamTypes
            // 
            this.cmbTeamTypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbTeamTypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTeamTypes.FormattingEnabled = true;
            this.cmbTeamTypes.Location = new System.Drawing.Point(70, 84);
            this.cmbTeamTypes.Name = "cmbTeamTypes";
            this.cmbTeamTypes.Size = new System.Drawing.Size(101, 21);
            this.cmbTeamTypes.TabIndex = 4;
            this.cmbTeamTypes.Visible = false;
            this.cmbTeamTypes.MouseEnter += new System.EventHandler(this.CmbTeamTypes_MouseEnter);
            this.cmbTeamTypes.MouseLeave += new System.EventHandler(this.HideToolTip);
            this.cmbTeamTypes.MouseMove += new System.Windows.Forms.MouseEventHandler(this.CmbTrigger_MouseMove);
            // 
            // lblRoute
            // 
            this.lblRoute.AutoSize = true;
            this.lblRoute.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRoute.Location = new System.Drawing.Point(3, 81);
            this.lblRoute.Name = "lblRoute";
            this.lblRoute.Size = new System.Drawing.Size(61, 27);
            this.lblRoute.TabIndex = 3;
            this.lblRoute.Text = "Show route";
            this.lblRoute.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblRoute.Visible = false;
            // 
            // lblWaypoint
            // 
            this.lblWaypoint.AutoSize = true;
            this.lblWaypoint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblWaypoint.Location = new System.Drawing.Point(3, 27);
            this.lblWaypoint.Name = "lblWaypoint";
            this.lblWaypoint.Size = new System.Drawing.Size(61, 27);
            this.lblWaypoint.TabIndex = 0;
            this.lblWaypoint.Text = "Waypoint";
            this.lblWaypoint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbWaypoints
            // 
            this.tableLayoutPanel.SetColumnSpan(this.cmbWaypoints, 2);
            this.cmbWaypoints.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbWaypoints.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbWaypoints.FormattingEnabled = true;
            this.cmbWaypoints.Location = new System.Drawing.Point(70, 30);
            this.cmbWaypoints.Name = "cmbWaypoints";
            this.cmbWaypoints.Size = new System.Drawing.Size(141, 21);
            this.cmbWaypoints.TabIndex = 1;
            // 
            // btnJumpTo
            // 
            this.tableLayoutPanel.SetColumnSpan(this.btnJumpTo, 2);
            this.btnJumpTo.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnJumpTo.Location = new System.Drawing.Point(69, 56);
            this.btnJumpTo.Margin = new System.Windows.Forms.Padding(2);
            this.btnJumpTo.Name = "btnJumpTo";
            this.btnJumpTo.Size = new System.Drawing.Size(143, 23);
            this.btnJumpTo.TabIndex = 2;
            this.btnJumpTo.Text = "Jump to";
            this.btnJumpTo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnJumpTo.UseVisualStyleBackColor = true;
            // 
            // btnModeSwitch
            // 
            this.tableLayoutPanel.SetColumnSpan(this.btnModeSwitch, 2);
            this.btnModeSwitch.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnModeSwitch.Location = new System.Drawing.Point(69, 2);
            this.btnModeSwitch.Margin = new System.Windows.Forms.Padding(2);
            this.btnModeSwitch.Name = "btnModeSwitch";
            this.btnModeSwitch.Size = new System.Drawing.Size(143, 23);
            this.btnModeSwitch.TabIndex = 6;
            this.btnModeSwitch.Text = "Waypoint";
            this.btnModeSwitch.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnModeSwitch.UseVisualStyleBackColor = true;
            // 
            // WaypointsToolDialog
            // 
            this.AcceptButton = this.btnJumpTo;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(214, 81);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(145, 73);
            this.Name = "WaypointsToolDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Waypoints";
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Label lblWaypoint;
        private MobiusEditor.Controls.ComboBoxSmartWidth cmbWaypoints;
        private System.Windows.Forms.Button btnJumpTo;
        private Controls.ComboBoxSmartWidth cmbTeamTypes;
        private System.Windows.Forms.Label lblRoute;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnJumpToTeam;
        private System.Windows.Forms.Label lblMode;
        private System.Windows.Forms.Button btnModeSwitch;
        private System.Windows.Forms.Button btnEditTeam;
    }
}