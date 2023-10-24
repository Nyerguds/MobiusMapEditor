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
            this.lblEditorInfo = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblLink = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblEditorInfo
            // 
            this.lblEditorInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblEditorInfo.Location = new System.Drawing.Point(12, 174);
            this.lblEditorInfo.Name = "lblEditorInfo";
            this.lblEditorInfo.Size = new System.Drawing.Size(390, 134);
            this.lblEditorInfo.TabIndex = 1;
            this.lblEditorInfo.Text = "Line 1\r\nLine 2\r\nLine 3\r\nLine 4\r\nLine 5\r\nLine 6\r\nLine 7\r\nLine 8\r\nLine 9";
            this.lblEditorInfo.TextAlign = System.Drawing.ContentAlignment.TopCenter;
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
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label1.Image = global::MobiusEditor.Properties.Resources.Mobius;
            this.label1.Location = new System.Drawing.Point(75, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(264, 156);
            this.label1.TabIndex = 0;
            this.label1.Click += new System.EventHandler(this.lblImage_Click);
            // 
            // lblLink
            // 
            this.lblLink.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lblLink.AutoSize = true;
            this.lblLink.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblLink.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLink.ForeColor = System.Drawing.Color.Blue;
            this.lblLink.Location = new System.Drawing.Point(12, 308);
            this.lblLink.Name = "lblLink";
            this.lblLink.Size = new System.Drawing.Size(28, 13);
            this.lblLink.TabIndex = 3;
            this.lblLink.Text = "linky";
            this.lblLink.Click += new System.EventHandler(this.lblLink_Click);
            // 
            // ThankYouDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(414, 374);
            this.Controls.Add(this.lblLink);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblEditorInfo);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = global::MobiusEditor.Properties.Resources.GameIcon00;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ThankYouDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Thank You!";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ThankYou_FormClosing);
            this.Shown += new System.EventHandler(this.ThankYouDialog_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblEditorInfo;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblLink;
    }
}