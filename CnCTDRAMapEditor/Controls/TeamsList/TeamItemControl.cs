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
        private IListedControlController<TeamTypeClass, char, int> m_Controller;
        private ITechnoType defaultType;


        public TeamItemControl()
            :this(null, null, null)
        {
        }

        public TeamItemControl(TeamTypeClass info, IListedControlController<TeamTypeClass, char, int> controller, IEnumerable<ITechnoType> technos)
        {
            InitializeComponent();
            SetInfo(info, controller, technos);
        }

        public void SetInfo(TeamTypeClass info, IListedControlController<TeamTypeClass, char, int> controller, IEnumerable<ITechnoType> technos)
        {
            try
            {
                m_Loading = true;
                Info = null;
                m_Controller = controller;
                ListItem<ITechnoType>[] technoTypes = technos.Select(t => ListItem.Create(t, t.DisplayName)).ToArray();
                defaultType = technoTypes.FirstOrDefault()?.Value;
                cmbTechno.DisplayMember = null;
                cmbTechno.DataSource = null;
                cmbTechno.Items.Clear();
                cmbTechno.DataSource = technoTypes;
                cmbTechno.DisplayMember = "DisplayName";
            }
            finally
            {
                m_Loading = false;
            }
            if (info != null)
                UpdateInfo(info);
        }

        public void UpdateInfo(TeamTypeClass info)
        {
            try
            {
                m_Loading = true;
                Info = info;
                cmbTechno.SelectedIndex = ListItem.GetIndexInComboBox(info != null ? info.Type : defaultType, cmbTechno);
                //this.cmbTechno.SelectedItem = info != null ? info.Type : defaultType;
                numAmount.Value = info != null ? info.Count : 0;
            }
            finally
            {
                m_Loading = false;
            }
        }

        public void FocusValue()
        {
            cmbTechno.Select();
        }

        private void CmbTechno_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_Loading || Info == null)
            {
                return;
            }
            Info.Type = ListItem.GetValueFromComboBox(cmbTechno, defaultType);
        }

        private void NumAmount_ValueChanged(object sender, EventArgs e)
        {
            if (m_Loading || Info == null)
            {
                return;
            }
            Info.Count = (byte)numAmount.Value;
            if (m_Controller != null)
            {
                m_Controller.UpdateControlInfo(Info, 'E');
            }
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (m_Loading || Info == null)
            {
                return;
            }
            if (m_Controller != null)
            {
                m_Controller.UpdateControlInfo(Info, 'R');
            }
        }
    }
}
