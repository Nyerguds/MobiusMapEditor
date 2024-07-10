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
    partial class ThankYouDialog
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
            this.btnClose = new System.Windows.Forms.Button();
            this.lblPicture = new System.Windows.Forms.Label();
            this.lblLink = new System.Windows.Forms.Label();
            this.txtEditorInfo = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(170, 339);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // lblPicture
            // 
            this.lblPicture.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblPicture.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblPicture.Image = global::MobiusEditor.Properties.Resources.Mobius;
            this.lblPicture.Location = new System.Drawing.Point(75, 9);
            this.lblPicture.Name = "lblPicture";
            this.lblPicture.Size = new System.Drawing.Size(264, 156);
            this.lblPicture.TabIndex = 0;
            this.lblPicture.Click += new System.EventHandler(this.lblImage_Click);
            // 
            // lblLink
            // 
            this.lblLink.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lblLink.AutoSize = true;
            this.lblLink.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblLink.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLink.ForeColor = System.Drawing.Color.Blue;
            this.lblLink.Location = new System.Drawing.Point(12, 318);
            this.lblLink.Name = "lblLink";
            this.lblLink.Size = new System.Drawing.Size(28, 13);
            this.lblLink.TabIndex = 3;
            this.lblLink.Text = "linky";
            this.lblLink.Click += new System.EventHandler(this.lblLink_Click);
            // 
            // txtEditorInfo
            // 
            this.txtEditorInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEditorInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtEditorInfo.Location = new System.Drawing.Point(12, 174);
            this.txtEditorInfo.Multiline = true;
            this.txtEditorInfo.Name = "txtEditorInfo";
            this.txtEditorInfo.ReadOnly = true;
            this.txtEditorInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtEditorInfo.Size = new System.Drawing.Size(390, 134);
            this.txtEditorInfo.TabIndex = 4;
            this.txtEditorInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ThankYouDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(414, 374);
            this.Controls.Add(this.txtEditorInfo);
            this.Controls.Add(this.lblLink);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblPicture);
            this.Icon = global::MobiusEditor.Properties.Resources.GameIcon00;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 350);
            this.Name = "ThankYouDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Thank You!";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ThankYou_FormClosing);
            this.Shown += new System.EventHandler(this.ThankYouDialog_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblPicture;
        private System.Windows.Forms.Label lblLink;
        private System.Windows.Forms.TextBox txtEditorInfo;
    }
}