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
    partial class NewMapDialog
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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lbGames = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lbTheaters = new System.Windows.Forms.ListBox();
            this.chkSingleplayer = new System.Windows.Forms.CheckBox();
            this.chkMegamap = new System.Windows.Forms.CheckBox();
            this.lblWarnMegamap = new System.Windows.Forms.Label();
            this.lblWarnModTheater = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(356, 352);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.btnCancel);
            this.flowLayoutPanel1.Controls.Add(this.btnOK);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(2, 323);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(352, 27);
            this.flowLayoutPanel1.TabIndex = 4;
            // 
            // btnCancel
            // 
            this.btnCancel.AutoSize = true;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(300, 2);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(50, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.AutoSize = true;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(246, 2);
            this.btnOK.Margin = new System.Windows.Forms.Padding(2);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(50, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.groupBox1);
            this.flowLayoutPanel2.Controls.Add(this.groupBox2);
            this.flowLayoutPanel2.Controls.Add(this.chkSingleplayer);
            this.flowLayoutPanel2.Controls.Add(this.chkMegamap);
            this.flowLayoutPanel2.Controls.Add(this.lblWarnMegamap);
            this.flowLayoutPanel2.Controls.Add(this.lblWarnModTheater);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(2, 2);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(352, 317);
            this.flowLayoutPanel2.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lbGames);
            this.groupBox1.Location = new System.Drawing.Point(2, 2);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(339, 95);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Game Type";
            // 
            // lbGames
            // 
            this.lbGames.FormattingEnabled = true;
            this.lbGames.Location = new System.Drawing.Point(5, 18);
            this.lbGames.Name = "lbGames";
            this.lbGames.Size = new System.Drawing.Size(326, 69);
            this.lbGames.TabIndex = 0;
            this.lbGames.SelectedIndexChanged += new System.EventHandler(this.LbGames_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.lbTheaters);
            this.groupBox2.Location = new System.Drawing.Point(2, 101);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(339, 95);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Theater";
            // 
            // lbTheaters
            // 
            this.lbTheaters.FormattingEnabled = true;
            this.lbTheaters.Location = new System.Drawing.Point(5, 18);
            this.lbTheaters.Name = "lbTheaters";
            this.lbTheaters.Size = new System.Drawing.Size(326, 69);
            this.lbTheaters.TabIndex = 0;
            this.lbTheaters.SelectedIndexChanged += new System.EventHandler(this.lbTheaters_SelectedIndexChanged);
            this.lbTheaters.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LbTheaters_MouseDoubleClick);
            // 
            // chkSingleplayer
            // 
            this.chkSingleplayer.AutoSize = true;
            this.chkSingleplayer.Location = new System.Drawing.Point(7, 201);
            this.chkSingleplayer.Margin = new System.Windows.Forms.Padding(7, 3, 3, 3);
            this.chkSingleplayer.Name = "chkSingleplayer";
            this.chkSingleplayer.Size = new System.Drawing.Size(132, 17);
            this.chkSingleplayer.TabIndex = 2;
            this.chkSingleplayer.Text = "Single-Player Scenario";
            this.toolTip1.SetToolTip(this.chkSingleplayer, "This can still be modified later, through Settings → Map Settings");
            this.chkSingleplayer.UseVisualStyleBackColor = true;
            this.chkSingleplayer.CheckedChanged += new System.EventHandler(this.ChkMegamap_CheckedChanged);
            // 
            // chkMegamap
            // 
            this.chkMegamap.AutoSize = true;
            this.chkMegamap.Location = new System.Drawing.Point(7, 224);
            this.chkMegamap.Margin = new System.Windows.Forms.Padding(7, 3, 3, 3);
            this.chkMegamap.Name = "chkMegamap";
            this.chkMegamap.Size = new System.Drawing.Size(177, 17);
            this.chkMegamap.TabIndex = 3;
            this.chkMegamap.Text = "Megamap (Sole Survivor format)";
            this.chkMegamap.UseVisualStyleBackColor = true;
            this.chkMegamap.CheckedChanged += new System.EventHandler(this.ChkMegamap_CheckedChanged);
            // 
            // lblWarnMegamap
            // 
            this.lblWarnMegamap.AutoSize = true;
            this.lblWarnMegamap.ForeColor = System.Drawing.Color.Red;
            this.lblWarnMegamap.Location = new System.Drawing.Point(7, 247);
            this.lblWarnMegamap.Margin = new System.Windows.Forms.Padding(7, 3, 3, 3);
            this.lblWarnMegamap.Name = "lblWarnMegamap";
            this.lblWarnMegamap.Size = new System.Drawing.Size(331, 26);
            this.lblWarnMegamap.TabIndex = 3;
            this.lblWarnMegamap.Text = "Warning: the Sole Survivor mega map format is not supported by the game without m" +
    "odding.";
            this.lblWarnMegamap.Visible = false;
            // 
            // lblWarnModTheater
            // 
            this.lblWarnModTheater.AutoSize = true;
            this.lblWarnModTheater.ForeColor = System.Drawing.Color.Red;
            this.lblWarnModTheater.Location = new System.Drawing.Point(7, 279);
            this.lblWarnModTheater.Margin = new System.Windows.Forms.Padding(7, 3, 3, 3);
            this.lblWarnModTheater.Name = "lblWarnModTheater";
            this.lblWarnModTheater.Size = new System.Drawing.Size(333, 26);
            this.lblWarnModTheater.TabIndex = 3;
            this.lblWarnModTheater.Text = "Warning: this is a modded theater. It is not supported by the game by default.";
            this.lblWarnModTheater.Visible = false;
            // 
            // NewMapDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(356, 352);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::MobiusEditor.Properties.Resources.GameIcon00;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewMapDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New Map";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chkMegamap;
        private System.Windows.Forms.Label lblWarnMegamap;
        private System.Windows.Forms.ListBox lbGames;
        private System.Windows.Forms.ListBox lbTheaters;
        private System.Windows.Forms.CheckBox chkSingleplayer;
        private System.Windows.Forms.Label lblWarnModTheater;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}