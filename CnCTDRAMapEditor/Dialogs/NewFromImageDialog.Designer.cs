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
namespace MobiusEditor.Dialogs
{
    partial class NewFromImageDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lstTemplates = new System.Windows.Forms.ListBox();
            this.lstMappings = new System.Windows.Forms.ListBox();
            this.lblColor = new System.Windows.Forms.Label();
            this.lblColorVal = new System.Windows.Forms.Label();
            this.lblMappings = new System.Windows.Forms.Label();
            this.lblImage = new System.Windows.Forms.Label();
            this.lblTemplates = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnChooseColor = new System.Windows.Forms.Button();
            this.templateTypeMapPanel = new MobiusEditor.Controls.MapPanel();
            this.picZoom = new MobiusEditor.Controls.PixelBox();
            ((System.ComponentModel.ISupportInitialize)(this.picZoom)).BeginInit();
            this.SuspendLayout();
            // 
            // lstTemplates
            // 
            this.lstTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstTemplates.Enabled = false;
            this.lstTemplates.FormattingEnabled = true;
            this.lstTemplates.Location = new System.Drawing.Point(274, 38);
            this.lstTemplates.Name = "lstTemplates";
            this.lstTemplates.Size = new System.Drawing.Size(184, 251);
            this.lstTemplates.TabIndex = 2;
            this.lstTemplates.SelectedIndexChanged += new System.EventHandler(this.lstTemplates_SelectedIndexChanged);
            // 
            // lstMappings
            // 
            this.lstMappings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstMappings.FormattingEnabled = true;
            this.lstMappings.Location = new System.Drawing.Point(726, 38);
            this.lstMappings.Name = "lstMappings";
            this.lstMappings.Size = new System.Drawing.Size(184, 264);
            this.lstMappings.TabIndex = 2;
            this.lstMappings.SelectedIndexChanged += new System.EventHandler(this.lstMappings_SelectedIndexChanged);
            this.lstMappings.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lstMappings_MouseDown);
            // 
            // lblColor
            // 
            this.lblColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblColor.AutoSize = true;
            this.lblColor.Location = new System.Drawing.Point(15, 313);
            this.lblColor.Name = "lblColor";
            this.lblColor.Size = new System.Drawing.Size(78, 13);
            this.lblColor.TabIndex = 8;
            this.lblColor.Text = "Selected color:";
            // 
            // lblColorVal
            // 
            this.lblColorVal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblColorVal.Location = new System.Drawing.Point(99, 309);
            this.lblColorVal.Name = "lblColorVal";
            this.lblColorVal.Size = new System.Drawing.Size(91, 21);
            this.lblColorVal.TabIndex = 8;
            this.lblColorVal.Text = "-";
            this.lblColorVal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMappings
            // 
            this.lblMappings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMappings.AutoSize = true;
            this.lblMappings.Location = new System.Drawing.Point(725, 9);
            this.lblMappings.Name = "lblMappings";
            this.lblMappings.Size = new System.Drawing.Size(154, 26);
            this.lblMappings.TabIndex = 8;
            this.lblMappings.Text = "Mappings:\r\nClick to select existing mapping";
            // 
            // lblImage
            // 
            this.lblImage.AutoSize = true;
            this.lblImage.Location = new System.Drawing.Point(12, 9);
            this.lblImage.Name = "lblImage";
            this.lblImage.Size = new System.Drawing.Size(99, 26);
            this.lblImage.TabIndex = 9;
            this.lblImage.Text = "Image:\r\nClick to select color";
            // 
            // lblTemplates
            // 
            this.lblTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTemplates.AutoSize = true;
            this.lblTemplates.Location = new System.Drawing.Point(271, 9);
            this.lblTemplates.Name = "lblTemplates";
            this.lblTemplates.Size = new System.Drawing.Size(146, 26);
            this.lblTemplates.TabIndex = 9;
            this.lblTemplates.Text = "Templates:\r\nClick to map color to template";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(461, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(139, 26);
            this.label1.TabIndex = 9;
            this.label1.Text = "Tiles:\r\nClick to map to specific icon";
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(754, 308);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 10;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(835, 308);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnChooseColor
            // 
            this.btnChooseColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChooseColor.Location = new System.Drawing.Point(196, 308);
            this.btnChooseColor.Name = "btnChooseColor";
            this.btnChooseColor.Size = new System.Drawing.Size(75, 23);
            this.btnChooseColor.TabIndex = 10;
            this.btnChooseColor.Text = "Choose";
            this.btnChooseColor.UseVisualStyleBackColor = true;
            this.btnChooseColor.Click += new System.EventHandler(this.btnChooseColor_Click);
            // 
            // templateTypeMapPanel
            // 
            this.templateTypeMapPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.templateTypeMapPanel.Enabled = false;
            this.templateTypeMapPanel.Location = new System.Drawing.Point(464, 38);
            this.templateTypeMapPanel.MapImage = null;
            this.templateTypeMapPanel.MaxZoom = 8D;
            this.templateTypeMapPanel.MinZoom = 1D;
            this.templateTypeMapPanel.Name = "templateTypeMapPanel";
            this.templateTypeMapPanel.Size = new System.Drawing.Size(256, 256);
            this.templateTypeMapPanel.SmoothScale = false;
            this.templateTypeMapPanel.TabIndex = 7;
            this.templateTypeMapPanel.Zoom = 1D;
            this.templateTypeMapPanel.ZoomStep = 1D;
            this.templateTypeMapPanel.PostRender += new System.EventHandler<MobiusEditor.Event.RenderEventArgs>(this.templateTypeMapPanel_PostRender);
            this.templateTypeMapPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.templateTypeMapPanel_MouseDown);
            this.templateTypeMapPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.templateTypeMapPanel_MouseMove);
            // 
            // picZoom
            // 
            this.picZoom.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picZoom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picZoom.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picZoom.Location = new System.Drawing.Point(15, 38);
            this.picZoom.Name = "picZoom";
            this.picZoom.Size = new System.Drawing.Size(256, 256);
            this.picZoom.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picZoom.TabIndex = 1;
            this.picZoom.TabStop = false;
            this.picZoom.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picZoom_MouseDown);
            this.picZoom.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picZoom_MouseMove);
            // 
            // NewFromImageDialog
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(934, 341);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnChooseColor);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblTemplates);
            this.Controls.Add(this.lblImage);
            this.Controls.Add(this.lblMappings);
            this.Controls.Add(this.lblColorVal);
            this.Controls.Add(this.lblColor);
            this.Controls.Add(this.templateTypeMapPanel);
            this.Controls.Add(this.lstMappings);
            this.Controls.Add(this.lstTemplates);
            this.Controls.Add(this.picZoom);
            this.Icon = global::MobiusEditor.Properties.Resources.GameIcon00;
            this.MinimizeBox = false;
            this.Name = "NewFromImageDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Map colors to tiles";
            ((System.ComponentModel.ISupportInitialize)(this.picZoom)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.PixelBox picZoom;
        private System.Windows.Forms.ListBox lstTemplates;
        private System.Windows.Forms.ListBox lstMappings;
        private Controls.MapPanel templateTypeMapPanel;
        private System.Windows.Forms.Label lblColor;
        private System.Windows.Forms.Label lblColorVal;
        private System.Windows.Forms.Label lblMappings;
        private System.Windows.Forms.Label lblImage;
        private System.Windows.Forms.Label lblTemplates;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnChooseColor;
    }
}