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
using MobiusEditor.Controls.ControlsList;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public partial class TeamItemControl : UserControl
    {
        public TeamTypeClass Info { get; set; }
        private bool m_Loading;
        private ListedControlController<TeamTypeClass> m_Controller;
        private ITechnoType defaultType;


        public TeamItemControl()
            :this(null, null, null)
        {
        }

        public TeamItemControl(TeamTypeClass info, ListedControlController<TeamTypeClass> controller, IEnumerable<ITechnoType> technos)
        {
            InitializeComponent();
            SetInfo(info, controller, technos);
        }

        public void SetInfo(TeamTypeClass info, ListedControlController<TeamTypeClass> controller, IEnumerable<ITechnoType> technos)
        {
            try
            {
                this.m_Loading = true;
                this.Info = null;
                this.m_Controller = controller;
                ITechnoType[] technoTypes = technos.ToArray();
                this.defaultType = technoTypes.FirstOrDefault();
                this.cmbTechno.DisplayMember = null;
                this.cmbTechno.DataSource = null;
                this.cmbTechno.Items.Clear();
                this.cmbTechno.DataSource = technoTypes;
                this.cmbTechno.DisplayMember = "DisplayName";
            }
            finally
            {
                this.m_Loading = false;
            }
            if (info != null)
                UpdateInfo(info);
        }

        public void UpdateInfo(TeamTypeClass info)
        {
            try
            {
                this.m_Loading = true;
                this.Info = info;
                this.cmbTechno.SelectedItem = info != null ? info.Type : defaultType;
                this.numAmount.Value = info != null ? info.Count : 0;
            }
            finally
            {
                this.m_Loading = false;
            }
        }

        public void FocusValue()
        {
            this.cmbTechno.Select();
        }

        private void cmbTechno_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if (this.m_Loading || this.Info == null)
            {
                return;
            }
            this.Info.Type = this.cmbTechno.SelectedItem as ITechnoType;
        }

        private void numAmount_ValueChanged(Object sender, EventArgs e)
        {
            if (this.m_Loading || this.Info == null)
            {
                return;
            }
            this.Info.Count = (Byte)this.numAmount.Value;
            if (this.m_Controller != null)
                this.m_Controller.UpdateControlInfo(this.Info);
        }

        private void btnRemove_Click(Object sender, EventArgs e)
        {
            if (this.m_Loading || this.Info == null)
            {
                return;
            }
            // Setting type to null is the signal to delete.
            this.Info.Type = null;
            if (this.m_Controller != null)
                this.m_Controller.UpdateControlInfo(this.Info);
        }
    }
}
