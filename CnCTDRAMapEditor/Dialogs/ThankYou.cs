using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class ThankYou : Form
    {
        public ThankYou()
        {
            InitializeComponent();
            this.Text = "About " + Program.ProgramVersionTitle;
        }
    }
}
