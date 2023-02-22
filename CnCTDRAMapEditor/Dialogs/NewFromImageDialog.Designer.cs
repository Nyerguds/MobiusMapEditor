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
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstTemplates.Enabled = false;
            this.lstTemplates.FormattingEnabled = true;
            this.lstTemplates.Location = new System.Drawing.Point(274, 30);
            this.lstTemplates.Name = "lstTemplates";
            this.lstTemplates.Size = new System.Drawing.Size(184, 290);
            this.lstTemplates.TabIndex = 2;
            this.lstTemplates.SelectedIndexChanged += new System.EventHandler(this.lstTemplates_SelectedIndexChanged);
            // 
            // lstMappings
            // 
            this.lstMappings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstMappings.FormattingEnabled = true;
            this.lstMappings.Location = new System.Drawing.Point(740, 30);
            this.lstMappings.Name = "lstMappings";
            this.lstMappings.Size = new System.Drawing.Size(184, 290);
            this.lstMappings.TabIndex = 2;
            this.lstMappings.SelectedIndexChanged += new System.EventHandler(this.lstMappings_SelectedIndexChanged);
            this.lstMappings.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lstMappings_MouseDown);
            // 
            // lblColor
            // 
            this.lblColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblColor.AutoSize = true;
            this.lblColor.Location = new System.Drawing.Point(12, 299);
            this.lblColor.Name = "lblColor";
            this.lblColor.Size = new System.Drawing.Size(78, 13);
            this.lblColor.TabIndex = 8;
            this.lblColor.Text = "Selected color:";
            // 
            // lblColorVal
            // 
            this.lblColorVal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblColorVal.Location = new System.Drawing.Point(96, 295);
            this.lblColorVal.Name = "lblColorVal";
            this.lblColorVal.Size = new System.Drawing.Size(91, 21);
            this.lblColorVal.TabIndex = 8;
            this.lblColorVal.Text = "-";
            this.lblColorVal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMappings
            // 
            this.lblMappings.AutoSize = true;
            this.lblMappings.Location = new System.Drawing.Point(740, 9);
            this.lblMappings.Name = "lblMappings";
            this.lblMappings.Size = new System.Drawing.Size(56, 13);
            this.lblMappings.TabIndex = 8;
            this.lblMappings.Text = "Mappings:";
            // 
            // lblImage
            // 
            this.lblImage.AutoSize = true;
            this.lblImage.Location = new System.Drawing.Point(12, 9);
            this.lblImage.Name = "lblImage";
            this.lblImage.Size = new System.Drawing.Size(39, 13);
            this.lblImage.TabIndex = 9;
            this.lblImage.Text = "Image:";
            // 
            // lblTemplates
            // 
            this.lblTemplates.AutoSize = true;
            this.lblTemplates.Location = new System.Drawing.Point(271, 9);
            this.lblTemplates.Name = "lblTemplates";
            this.lblTemplates.Size = new System.Drawing.Size(59, 13);
            this.lblTemplates.TabIndex = 9;
            this.lblTemplates.Text = "Templates:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(461, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Tiles:";
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(765, 333);
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
            this.btnCancel.Location = new System.Drawing.Point(846, 333);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnChooseColor
            // 
            this.btnChooseColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnChooseColor.Location = new System.Drawing.Point(193, 294);
            this.btnChooseColor.Name = "btnChooseColor";
            this.btnChooseColor.Size = new System.Drawing.Size(75, 23);
            this.btnChooseColor.TabIndex = 10;
            this.btnChooseColor.Text = "Choose";
            this.btnChooseColor.UseVisualStyleBackColor = true;
            // 
            // templateTypeMapPanel
            // 
            this.templateTypeMapPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.templateTypeMapPanel.Enabled = false;
            this.templateTypeMapPanel.Location = new System.Drawing.Point(464, 30);
            this.templateTypeMapPanel.MapImage = null;
            this.templateTypeMapPanel.MaxZoom = 8D;
            this.templateTypeMapPanel.MinZoom = 1D;
            this.templateTypeMapPanel.Name = "templateTypeMapPanel";
            this.templateTypeMapPanel.Size = new System.Drawing.Size(270, 290);
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
            this.picZoom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.picZoom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picZoom.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picZoom.Location = new System.Drawing.Point(12, 30);
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
            this.ClientSize = new System.Drawing.Size(933, 368);
            this.ControlBox = false;
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
            this.Name = "NewFromImageDialog";
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