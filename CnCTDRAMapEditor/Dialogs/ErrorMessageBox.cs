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
using System.Collections.Generic;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class ErrorMessageBox : Form
    {
        public string Title
        {
            set { this.Text = value; }
            get { return this.Text;  }
        }

        public string Message
        {
            set { lblMessage.Text = value; }
            get { return lblMessage.Text; }
        }

        public IEnumerable<string> Errors
        {
            set { txtErrors.Text = value == null ? String.Empty : string.Join(Environment.NewLine, value); }
            get { return (txtErrors.Text ?? String.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'); }
        }

        public bool UseWordWrap
        {
            set { txtErrors.WordWrap = value; }
            get { return txtErrors.WordWrap; }
        }

        public ErrorMessageBox()
            :this(false)
        {
        }

        public ErrorMessageBox(bool showCancel)
        {
            InitializeComponent();
            btnCancel.Visible = showCancel;
            SetWrap(txtErrors.WordWrap);
            if (!showCancel)
            {
                // Enables "esc" for closing the form.
                this.CancelButton = btnOk;
            }
        }

        private void SetWrap(Boolean wrap)
        {
            txtErrors.WordWrap = wrap;
            lblWordWrap.Text = String.Format("Word wrap: [{0}]", wrap ? "ON" : "OFF");
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtErrors.Text);
        }

        private void btnWordWrap_Click(Object sender, EventArgs e)
        {
            SetWrap(!txtErrors.WordWrap);
        }

        private void ErrorMessageBox_Shown(Object sender, EventArgs e)
        {
            txtErrors.SelectionLength = 0;
            txtErrors.SelectionStart = 0;
        }
    }
}
