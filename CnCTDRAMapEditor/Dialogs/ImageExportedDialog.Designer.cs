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
    partial class ImageExportedDialog
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
            this.btnGoToFile = new System.Windows.Forms.Button();
            this.btnDone = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.lblExported = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.btnExportAgain = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnGoToFile
            // 
            this.btnGoToFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGoToFile.Location = new System.Drawing.Point(175, 89);
            this.btnGoToFile.Name = "btnGoToFile";
            this.btnGoToFile.Size = new System.Drawing.Size(75, 23);
            this.btnGoToFile.TabIndex = 4;
            this.btnGoToFile.Text = "Go to file";
            this.btnGoToFile.UseVisualStyleBackColor = true;
            this.btnGoToFile.Click += new System.EventHandler(this.BtnGoToFile_Click);
            // 
            // btnDone
            // 
            this.btnDone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDone.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnDone.Location = new System.Drawing.Point(347, 89);
            this.btnDone.Name = "btnDone";
            this.btnDone.Size = new System.Drawing.Size(75, 23);
            this.btnDone.TabIndex = 0;
            this.btnDone.Text = "Done";
            this.btnDone.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(15, 52);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(407, 20);
            this.textBox1.TabIndex = 3;
            // 
            // lblExported
            // 
            this.lblExported.AutoSize = true;
            this.lblExported.Location = new System.Drawing.Point(12, 12);
            this.lblExported.Margin = new System.Windows.Forms.Padding(3);
            this.lblExported.Name = "lblExported";
            this.lblExported.Size = new System.Drawing.Size(143, 13);
            this.lblExported.TabIndex = 1;
            this.lblExported.Text = "Image exported successfully!";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(12, 31);
            this.lblName.Margin = new System.Windows.Forms.Padding(3);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(50, 13);
            this.lblName.TabIndex = 2;
            this.lblName.Text = "File path:";
            // 
            // btnExportAgain
            // 
            this.btnExportAgain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportAgain.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnExportAgain.Location = new System.Drawing.Point(256, 89);
            this.btnExportAgain.Name = "btnExportAgain";
            this.btnExportAgain.Size = new System.Drawing.Size(85, 23);
            this.btnExportAgain.TabIndex = 5;
            this.btnExportAgain.Text = "Export again...";
            this.btnExportAgain.UseVisualStyleBackColor = true;
            // 
            // ImageExportedDialog
            // 
            this.AcceptButton = this.btnDone;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnExportAgain;
            this.ClientSize = new System.Drawing.Size(434, 124);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnExportAgain);
            this.Controls.Add(this.btnDone);
            this.Controls.Add(this.btnGoToFile);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblExported);
            this.Icon = global::MobiusEditor.Properties.Resources.GameIcon00;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImageExportedDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Image Export";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnGoToFile;
        private System.Windows.Forms.Button btnDone;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label lblExported;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Button btnExportAgain;
    }
}