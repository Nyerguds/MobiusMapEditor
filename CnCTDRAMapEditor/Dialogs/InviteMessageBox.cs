﻿//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// The Command & Conquer Map Editor and corresponding source code is distributed
// in the hope that it will be useful, but with permitted additional restrictions
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT
// distributed with this program. You should have received a copy of the
// GNU General Public License along with permitted additional restrictions
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class InviteMessageBox : Form
    {
        public bool DontShowAgain => checkBoxDontShowThisAgain.Checked;

        public InviteMessageBox()
        {
            InitializeComponent();
        }

        private void InviteMessageBox_Load(object sender, EventArgs e)
        {
            using (var infoIcon = new Icon(SystemIcons.Information, pictureBoxIcon.Width, pictureBoxIcon.Height))
            {
                pictureBoxIcon.Image = infoIcon.ToBitmap();
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                var img = pictureBoxIcon.Image;
                pictureBoxIcon.Image = null;
                try { img.Dispose(); }
                catch { /* ignore */ }
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
