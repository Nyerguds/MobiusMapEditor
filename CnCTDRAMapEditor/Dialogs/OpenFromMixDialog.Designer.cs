﻿//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
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
namespace MobiusEditor.Dialogs
{
    partial class OpenFromMixDialog
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
            this.mixContentsListView = new System.Windows.Forms.ListView();
            this.nameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.typeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SizeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DescColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.infoColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.indexColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.offsetColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnCloseFile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // mixContentsListView
            // 
            this.mixContentsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mixContentsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumnHeader,
            this.typeColumnHeader,
            this.SizeColumnHeader,
            this.DescColumnHeader,
            this.infoColumnHeader,
            this.indexColumnHeader,
            this.offsetColumnHeader});
            this.mixContentsListView.FullRowSelect = true;
            this.mixContentsListView.HideSelection = false;
            this.mixContentsListView.LabelWrap = false;
            this.mixContentsListView.Location = new System.Drawing.Point(8, 8);
            this.mixContentsListView.Margin = new System.Windows.Forms.Padding(2);
            this.mixContentsListView.MultiSelect = false;
            this.mixContentsListView.Name = "mixContentsListView";
            this.mixContentsListView.ShowItemToolTips = true;
            this.mixContentsListView.Size = new System.Drawing.Size(965, 406);
            this.mixContentsListView.TabIndex = 1;
            this.mixContentsListView.UseCompatibleStateImageBehavior = false;
            this.mixContentsListView.View = System.Windows.Forms.View.Details;
            this.mixContentsListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.mixContentsListView_ColumnClick);
            this.mixContentsListView.ColumnWidthChanging += new System.Windows.Forms.ColumnWidthChangingEventHandler(this.mixContentsListView_ColumnWidthChanging);
            this.mixContentsListView.SizeChanged += new System.EventHandler(this.MixContentsListView_SizeChanged);
            this.mixContentsListView.DoubleClick += new System.EventHandler(this.MixContentsListView_DoubleClick);
            this.mixContentsListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MixContentsListView_KeyDown);
            // 
            // nameColumnHeader
            // 
            this.nameColumnHeader.Tag = "-120";
            this.nameColumnHeader.Text = "Name";
            this.nameColumnHeader.Width = 120;
            // 
            // typeColumnHeader
            // 
            this.typeColumnHeader.Tag = "-80";
            this.typeColumnHeader.Text = "Type";
            this.typeColumnHeader.Width = 80;
            // 
            // SizeColumnHeader
            // 
            this.SizeColumnHeader.Tag = "-80";
            this.SizeColumnHeader.Text = "Size";
            this.SizeColumnHeader.Width = 80;
            // 
            // DescColumnHeader
            // 
            this.DescColumnHeader.Tag = "1";
            this.DescColumnHeader.Text = "Description";
            this.DescColumnHeader.Width = 100;
            // 
            // infoColumnHeader
            // 
            this.infoColumnHeader.Tag = "1";
            this.infoColumnHeader.Text = "Info";
            this.infoColumnHeader.Width = 100;
            // 
            // indexColumnHeader
            // 
            this.indexColumnHeader.Tag = "-40";
            this.indexColumnHeader.Text = "Index";
            this.indexColumnHeader.Width = 40;
            // 
            // offsetColumnHeader
            // 
            this.offsetColumnHeader.Tag = "-80";
            this.offsetColumnHeader.Text = "File offset";
            this.offsetColumnHeader.Width = 80;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(897, 426);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpen.Location = new System.Drawing.Point(816, 426);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 2;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.BtnOpen_Click);
            // 
            // btnCloseFile
            // 
            this.btnCloseFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCloseFile.Enabled = false;
            this.btnCloseFile.Location = new System.Drawing.Point(12, 426);
            this.btnCloseFile.Name = "btnCloseFile";
            this.btnCloseFile.Size = new System.Drawing.Size(88, 23);
            this.btnCloseFile.TabIndex = 4;
            this.btnCloseFile.Text = "Back to parent";
            this.btnCloseFile.UseVisualStyleBackColor = true;
            this.btnCloseFile.Click += new System.EventHandler(this.BtnCloseFile_Click);
            // 
            // OpenFromMixDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(984, 461);
            this.Controls.Add(this.btnCloseFile);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.mixContentsListView);
            this.Icon = global::MobiusEditor.Properties.Resources.GameIcon00;
            this.MinimumSize = new System.Drawing.Size(500, 300);
            this.Name = "OpenFromMixDialog";
            this.Text = "Open from Mix file";
            this.Load += new System.EventHandler(this.OpenFromMixDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView mixContentsListView;
        private System.Windows.Forms.ColumnHeader nameColumnHeader;
        private System.Windows.Forms.ColumnHeader typeColumnHeader;
        private System.Windows.Forms.ColumnHeader infoColumnHeader;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnCloseFile;
        private System.Windows.Forms.ColumnHeader SizeColumnHeader;
        private System.Windows.Forms.ColumnHeader DescColumnHeader;
        private System.Windows.Forms.ColumnHeader indexColumnHeader;
        private System.Windows.Forms.ColumnHeader offsetColumnHeader;
    }
}