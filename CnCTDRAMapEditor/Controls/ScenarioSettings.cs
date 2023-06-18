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
using MobiusEditor.Model;
using MobiusEditor.Utility;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public partial class ScenarioSettings : UserControl
    {
        public ScenarioSettings(PropertyTracker<BasicSection> basicSection)
        {
            InitializeComponent();
            chkToCarryOver.DataBindings.Add("Checked", basicSection, "ToCarryOver");
            chkToInherit.DataBindings.Add("Checked", basicSection, "ToInherit");
            chkCivEvac.DataBindings.Add("Checked", basicSection, "CivEvac");
            chkEndOfGame.DataBindings.Add("Checked", basicSection, "EndOfGame");
            chkTimerInherit.DataBindings.Add("Checked", basicSection, "TimerInherit");
            chkNoSpyPlane.DataBindings.Add("Checked", basicSection, "NoSpyPlane");
            chkSkipScore.DataBindings.Add("Checked", basicSection, "SkipScore");
            chkSkipMapSelect.DataBindings.Add("Checked", basicSection, "SkipMapSelect");
            chkOneTimeOnly.DataBindings.Add("Checked", basicSection, "OneTimeOnly");
            chkTruckCrate.DataBindings.Add("Checked", basicSection, "TruckCrate");
            chkFillSilos.DataBindings.Add("Checked", basicSection, "FillSilos");
        }
    }
}
