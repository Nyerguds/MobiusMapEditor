namespace MobiusEditor.Dialogs
{
    partial class ImageExportDialog
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
            this.layersListBox = new System.Windows.Forms.ListBox();
            this.indicatorsListBox = new System.Windows.Forms.ListBox();
            this.txtScale = new System.Windows.Forms.TextBox();
            this.lblScale = new System.Windows.Forms.Label();
            this.lblSize = new System.Windows.Forms.Label();
            this.chkSmooth = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.btnPickFile = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // layersListBox
            // 
            this.layersListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layersListBox.FormattingEnabled = true;
            this.layersListBox.Location = new System.Drawing.Point(3, 23);
            this.layersListBox.Name = "layersListBox";
            this.layersListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.layersListBox.Size = new System.Drawing.Size(159, 164);
            this.layersListBox.TabIndex = 0;
            // 
            // indicatorsListBox
            // 
            this.indicatorsListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.indicatorsListBox.FormattingEnabled = true;
            this.indicatorsListBox.Location = new System.Drawing.Point(183, 23);
            this.indicatorsListBox.Name = "indicatorsListBox";
            this.indicatorsListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.indicatorsListBox.Size = new System.Drawing.Size(160, 164);
            this.indicatorsListBox.TabIndex = 0;
            // 
            // txtScale
            // 
            this.txtScale.Location = new System.Drawing.Point(67, 9);
            this.txtScale.Name = "txtScale";
            this.txtScale.Size = new System.Drawing.Size(100, 20);
            this.txtScale.TabIndex = 2;
            this.txtScale.Text = "0.5";
            this.txtScale.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtScale.TextChanged += new System.EventHandler(this.txtScale_TextChanged);
            // 
            // lblScale
            // 
            this.lblScale.AutoSize = true;
            this.lblScale.Location = new System.Drawing.Point(12, 12);
            this.lblScale.Name = "lblScale";
            this.lblScale.Size = new System.Drawing.Size(37, 13);
            this.lblScale.TabIndex = 2;
            this.lblScale.Text = "Scale:";
            // 
            // lblSize
            // 
            this.lblSize.AutoSize = true;
            this.lblSize.Location = new System.Drawing.Point(173, 12);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(63, 13);
            this.lblSize.TabIndex = 2;
            this.lblSize.Text = "(Size: X * Y)";
            // 
            // chkSmooth
            // 
            this.chkSmooth.AutoSize = true;
            this.chkSmooth.Checked = true;
            this.chkSmooth.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSmooth.Location = new System.Drawing.Point(67, 35);
            this.chkSmooth.Name = "chkSmooth";
            this.chkSmooth.Size = new System.Drawing.Size(98, 17);
            this.chkSmooth.TabIndex = 2;
            this.chkSmooth.Text = "Smooth scaling";
            this.chkSmooth.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 252);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Output filename:";
            // 
            // txtPath
            // 
            this.txtPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPath.Location = new System.Drawing.Point(14, 273);
            this.txtPath.Name = "txtPath";
            this.txtPath.ReadOnly = true;
            this.txtPath.Size = new System.Drawing.Size(304, 20);
            this.txtPath.TabIndex = 4;
            // 
            // btnPickFile
            // 
            this.btnPickFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPickFile.Location = new System.Drawing.Point(324, 270);
            this.btnPickFile.Name = "btnPickFile";
            this.btnPickFile.Size = new System.Drawing.Size(31, 23);
            this.btnPickFile.TabIndex = 5;
            this.btnPickFile.Text = "...";
            this.btnPickFile.UseVisualStyleBackColor = true;
            this.btnPickFile.Click += new System.EventHandler(this.btnPickFile_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.layersListBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.indicatorsListBox, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 2, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 58);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(346, 190);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(159, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Map layers:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(183, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(160, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = "Indicators:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnExport
            // 
            this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExport.Location = new System.Drawing.Point(184, 301);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(84, 23);
            this.btnExport.TabIndex = 0;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(274, 301);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(84, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // ImageExportDialog
            // 
            this.AcceptButton = this.btnExport;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(370, 336);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnPickFile);
            this.Controls.Add(this.chkSmooth);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.lblSize);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblScale);
            this.Controls.Add(this.txtScale);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImageExportDialog";
            this.ShowIcon = false;
            this.Text = "Export as image";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox layersListBox;
        private System.Windows.Forms.ListBox indicatorsListBox;
        private System.Windows.Forms.TextBox txtScale;
        private System.Windows.Forms.Label lblScale;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.CheckBox chkSmooth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Button btnPickFile;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}