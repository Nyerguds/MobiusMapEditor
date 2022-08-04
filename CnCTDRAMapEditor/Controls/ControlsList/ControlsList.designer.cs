namespace MobiusEditor.Controls.ControlsList
{
    partial class ControlsList<T,TU>
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
            this.lblTypeName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // lblEffectName
            //
            this.lblTypeName.AutoSize = true;
            this.lblTypeName.Location = new System.Drawing.Point(4, 5);
            this.lblTypeName.Name = "lblTypeName";
            this.lblTypeName.Size = new System.Drawing.Size(0, 13);
            this.lblTypeName.TabIndex = 0;
            //
            // EffectBarList
            //
            this.Controls.Add(this.lblTypeName);
            this.Name = "ControlsList";
            this.Resize += new System.EventHandler(this.EffectBarList_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTypeName;

    }
}
