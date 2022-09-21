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
    public partial class CrateSettings : UserControl
    {
        public CrateSettings(PropertyTracker<SoleSurvivor.CratesSection> cratesSection)
        {
            InitializeComponent();
            nudAddStrength.DataBindings.Add("IntValue", cratesSection, "AddStrength");
            nudAddWeapon.DataBindings.Add("IntValue", cratesSection, "AddWeapon");
            nudAddSpeed.DataBindings.Add("IntValue", cratesSection, "AddSpeed");
            nudRapidReload.DataBindings.Add("IntValue", cratesSection, "RapidReload");
            nudAddRange.DataBindings.Add("IntValue", cratesSection, "AddRange");
            nudHeal.DataBindings.Add("IntValue", cratesSection, "Heal");
            nudBomb.DataBindings.Add("IntValue", cratesSection, "Bomb");
            nudStealth.DataBindings.Add("IntValue", cratesSection, "Stealth");
            nudTeleport.DataBindings.Add("IntValue", cratesSection, "Teleport");
            nudKill.DataBindings.Add("IntValue", cratesSection, "Kill");
            nudUncloakAll.DataBindings.Add("IntValue", cratesSection, "UncloakAll");
            nudReshroud.DataBindings.Add("IntValue", cratesSection, "Reshroud");
            nudUnshroud.DataBindings.Add("IntValue", cratesSection, "Unshroud");
            nudRadar.DataBindings.Add("IntValue", cratesSection, "Radar");
            nudArmageddon.DataBindings.Add("IntValue", cratesSection, "Armageddon");
            nudSuper.DataBindings.Add("IntValue", cratesSection, "Super");
            nudDensity.DataBindings.Add("IntValue", cratesSection, "Density");
            nudIonFactor.DataBindings.Add("IntValue", cratesSection, "IonFactor");
            nudCrateTimer.DataBindings.Add("IntValue", cratesSection, "CrateTimer");
        }

    }
}
