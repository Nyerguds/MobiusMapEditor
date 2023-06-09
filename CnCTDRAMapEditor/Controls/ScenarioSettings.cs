//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.

// The Command & Conquer Map Editor and corresponding source code is distributed
// in the hope that it will be useful, but with permitted additional restrictions
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT
// distributed with this program. You should have received a copy of the
// GNU General Public License along with permitted additional restrictions
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using System;
using System.Data;
using System.Linq;
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
