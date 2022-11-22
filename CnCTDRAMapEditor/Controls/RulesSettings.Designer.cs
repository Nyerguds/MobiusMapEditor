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
    partial class RulesSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RulesSettings));
            this.txtRules = new System.Windows.Forms.TextBox();
            this.lblRules = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblTextEncoding = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtRules
            // 
            this.txtRules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRules.Location = new System.Drawing.Point(2, 97);
            this.txtRules.Margin = new System.Windows.Forms.Padding(2);
            this.txtRules.Multiline = true;
            this.txtRules.Name = "txtRules";
            this.txtRules.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtRules.Size = new System.Drawing.Size(396, 325);
            this.txtRules.TabIndex = 1;
            this.txtRules.Leave += new System.EventHandler(this.txtRules_Leave);
            // 
            // lblRules
            // 
            this.lblRules.AutoSize = true;
            this.lblRules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRules.Location = new System.Drawing.Point(3, 0);
            this.lblRules.Name = "lblRules";
            this.lblRules.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.lblRules.Size = new System.Drawing.Size(394, 67);
            this.lblRules.TabIndex = 0;
            this.lblRules.Text = resources.GetString("lblRules.Text");
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lblRules, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtRules, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblTextEncoding, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(400, 424);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // lblTextEncoding
            // 
            this.lblTextEncoding.AutoSize = true;
            this.lblTextEncoding.Location = new System.Drawing.Point(3, 67);
            this.lblTextEncoding.Name = "lblTextEncoding";
            this.lblTextEncoding.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.lblTextEncoding.Size = new System.Drawing.Size(378, 28);
            this.lblTextEncoding.TabIndex = 2;
            this.lblTextEncoding.Text = "Note that all text here is treated as DOS content, meaning it will be loaded and " +
    "saved using DOS-437 text encoding.";
            // 
            // RulesSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "RulesSettings";
            this.Size = new System.Drawing.Size(400, 424);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TextBox txtRules;
        private System.Windows.Forms.Label lblRules;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblTextEncoding;
    }
}
