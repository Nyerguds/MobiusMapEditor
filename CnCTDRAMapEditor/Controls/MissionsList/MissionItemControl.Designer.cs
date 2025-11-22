//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                     Version 2, December 2004
//
//  Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
//  Everyone is permitted to copy and distribute verbatim or modified
//  copies of this license document, and changing it is allowed as long
//  as the name is changed.
//
//             DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//    TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//   0. You just DO WHAT THE FUCK YOU WANT TO.
namespace MobiusEditor.Controls
{
    partial class MissionItemControl
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
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.numValue = new MobiusEditor.Controls.EnhNumericUpDown();
            this.cmbValue = new MobiusEditor.Controls.ComboBoxSmartWidth();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cmbMission = new MobiusEditor.Controls.ComboBoxSmartWidth();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnOptions = new System.Windows.Forms.Button();
            this.cmsOptions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiMoveUp = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiMoveDown = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiInsert = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDuplicate = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numValue)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.cmsOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(300, 23);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.numValue);
            this.panel2.Controls.Add(this.cmbValue);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(150, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(123, 23);
            this.panel2.TabIndex = 2;
            // 
            // numValue
            // 
            this.numValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.numValue.Location = new System.Drawing.Point(2, 1);
            this.numValue.Margin = new System.Windows.Forms.Padding(0);
            this.numValue.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numValue.Name = "numValue";
            this.numValue.Size = new System.Drawing.Size(121, 20);
            this.numValue.TabIndex = 2;
            this.numValue.ValueChanged += new System.EventHandler(this.NumValue_ValueChanged);
            this.numValue.MouseLeave += Control_MouseLeave;
            // 
            // cmbValue
            // 
            this.cmbValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbValue.FormattingEnabled = true;
            this.cmbValue.Location = new System.Drawing.Point(2, 1);
            this.cmbValue.Margin = new System.Windows.Forms.Padding(0);
            this.cmbValue.Name = "cmbValue";
            this.cmbValue.Size = new System.Drawing.Size(120, 21);
            this.cmbValue.TabIndex = 3;
            this.cmbValue.Visible = false;
            this.cmbValue.SelectedIndexChanged += new System.EventHandler(this.CmbValue_SelectedIndexChanged);
            this.cmbValue.MouseLeave += Control_MouseLeave;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cmbMission);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(150, 23);
            this.panel1.TabIndex = 1;
            // 
            // cmbMission
            // 
            this.cmbMission.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbMission.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMission.FormattingEnabled = true;
            this.cmbMission.Location = new System.Drawing.Point(2, 1);
            this.cmbMission.Margin = new System.Windows.Forms.Padding(0);
            this.cmbMission.Name = "cmbMission";
            this.cmbMission.Size = new System.Drawing.Size(146, 21);
            this.cmbMission.TabIndex = 1;
            this.cmbMission.SelectedIndexChanged += new System.EventHandler(this.CmbMission_SelectedIndexChanged);
            this.cmbMission.MouseLeave += Control_MouseLeave;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnOptions);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(273, 0);
            this.panel3.Margin = new System.Windows.Forms.Padding(0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(27, 23);
            this.panel3.TabIndex = 3;
            // 
            // btnOptions
            // 
            this.btnOptions.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnOptions.Location = new System.Drawing.Point(2, 0);
            this.btnOptions.Margin = new System.Windows.Forms.Padding(0);
            this.btnOptions.Name = "btnOptions";
            this.btnOptions.Size = new System.Drawing.Size(21, 21);
            this.btnOptions.TabIndex = 0;
            this.btnOptions.Text = "🖊️";
            this.btnOptions.UseVisualStyleBackColor = true;
            this.btnOptions.Click += new System.EventHandler(this.BtnOptions_Click);
            // 
            // contextMenuStrip1
            // 
            this.cmsOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiDelete,
            this.tsmiMoveUp,
            this.tsmiMoveDown,
            this.tsmiInsert,
            this.tsmiDuplicate});
            this.cmsOptions.Name = "cmsOptions";
            this.cmsOptions.Size = new System.Drawing.Size(181, 92);
            // 
            // tsmiDelete
            // 
            this.tsmiDelete.Name = "tsmiDelete";
            this.tsmiDelete.Size = new System.Drawing.Size(180, 22);
            this.tsmiDelete.Text = "&Remove";
            this.tsmiDelete.Click += new System.EventHandler(this.TsmiDelete_Click);
            // 
            // tsmiMoveUp
            // 
            this.tsmiMoveUp.Name = "tsmiMoveUp";
            this.tsmiMoveUp.Size = new System.Drawing.Size(180, 22);
            this.tsmiMoveUp.Text = "Move &Up";
            this.tsmiMoveUp.Click += new System.EventHandler(this.TsmiMoveUp_Click);
            // 
            // tsmiMoveDown
            // 
            this.tsmiMoveDown.Name = "tsmiMoveDown";
            this.tsmiMoveDown.Size = new System.Drawing.Size(180, 22);
            this.tsmiMoveDown.Text = "Move D&own";
            this.tsmiMoveDown.Click += new System.EventHandler(this.TsmiMoveDown_Click);
            // 
            // tsmiDuplicate
            // 
            this.tsmiInsert.Name = "tsmiInsert";
            this.tsmiInsert.Size = new System.Drawing.Size(180, 22);
            this.tsmiInsert.Text = "Insert &New";
            this.tsmiInsert.Click += new System.EventHandler(this.TsmiInsert_Click);
            // 
            // tsmiDuplicate
            // 
            this.tsmiDuplicate.Name = "tsmiDuplicate";
            this.tsmiDuplicate.Size = new System.Drawing.Size(180, 22);
            this.tsmiDuplicate.Text = "&Duplicate";
            this.tsmiDuplicate.Click += new System.EventHandler(this.TsmiDuplicate_Click);
            // 
            // MissionItemControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "MissionItemControl";
            this.Size = new System.Drawing.Size(300, 23);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numValue)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.cmsOptions.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private EnhNumericUpDown numValue;
        private System.Windows.Forms.Panel panel1;
        private ComboBoxSmartWidth cmbMission;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnOptions;
        private ComboBoxSmartWidth cmbValue;
        private System.Windows.Forms.ContextMenuStrip cmsOptions;
        private System.Windows.Forms.ToolStripMenuItem tsmiDelete;
        private System.Windows.Forms.ToolStripMenuItem tsmiMoveUp;
        private System.Windows.Forms.ToolStripMenuItem tsmiMoveDown;
        private System.Windows.Forms.ToolStripMenuItem tsmiInsert;
        private System.Windows.Forms.ToolStripMenuItem tsmiDuplicate;
    }
}
