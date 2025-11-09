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
using System;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public partial class RulesSettings : UserControl
    {
        public event EventHandler TextNeedsUpdating;

        public RulesSettings(string iniText, bool showRulesWarning)
        {
            InitializeComponent();
            txtRules.Text = iniText;
            if (!showRulesWarning)
            {
                lblRaRules.Visible = false;
            }
        }

        private void TxtRules_Leave(object sender, EventArgs e)
        {
            TextNeedsUpdating?.Invoke(sender, e);
        }
    }
}
