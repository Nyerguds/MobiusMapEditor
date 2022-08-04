using MobiusEditor.Controls.ControlsList;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public partial class TeamItemControl : UserControl
    {
        public TeamTypeClass Info { get; set; }
        private Boolean m_Loading;
        private ListedControlController<TeamTypeClass> m_Controller;
        private ITechnoType defaultType;


        public TeamItemControl()
            :this(null, null, null)
        {
            
        }

        public TeamItemControl(TeamTypeClass info, ListedControlController<TeamTypeClass> controller, IEnumerable<ITechnoType> technos)
        {
            InitializeComponent();
            this.m_Controller = controller;
            ITechnoType[] technoTypes = technos.ToArray();
            this.defaultType = technoTypes.FirstOrDefault();
            this.cmbTechno.DataSource = technoTypes;
            this.cmbTechno.DisplayMember = "Name";
            if (info != null)
                UpdateInfo(info);
        }

        public void UpdateInfo(TeamTypeClass info)
        {
            try
            {
                m_Loading = true;
                this.Info = info;
                this.cmbTechno.Text = info != null ? info.Type.Name : defaultType.Name;
                this.numAmount.Value = info != null ? info.Count : 0;
            }
            finally
            {
                m_Loading = false;
            }
        }


        public void FocusValue()
        {
            this.cmbTechno.Select();
        }

        private void numAmount_ValueChanged(Object sender, EventArgs e)
        {
            if (this.m_Controller != null)
                this.m_Controller.UpdateControlInfo(this.Info);
        }
    }
}
