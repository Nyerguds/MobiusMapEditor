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
using MobiusEditor.Utility;
using System;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public partial class CrateSettings : UserControl
    {
        private PropertyTracker<SoleSurvivor.CratesSection> cratesSettings;

        public CrateSettings(PropertyTracker<SoleSurvivor.CratesSection> cratesSection)
        {
            this.cratesSettings = cratesSection;
            InitializeComponent();
            AddDataBindings();
        }

        private void btnDefaults_Click(Object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("This will reset all crate values to their game defaults. Are you sure you want to continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1))
            {
                ResetToDefaultValues();
            }
        }

        private void ResetToDefaultValues()
        {
            RemoveDataBindings();
            SoleSurvivor.CratesSection cratesSection = new SoleSurvivor.CratesSection();
            cratesSection.SetDefault();
            cratesSettings.SetValues(cratesSection);
            AddDataBindings();
        }

        private void AddDataBindings()
        {
            nudAddStrength.DataBindings.Add("IntValue", cratesSettings, "AddStrength");
            nudAddWeapon.DataBindings.Add("IntValue", cratesSettings, "AddWeapon");
            nudAddSpeed.DataBindings.Add("IntValue", cratesSettings, "AddSpeed");
            nudRapidReload.DataBindings.Add("IntValue", cratesSettings, "RapidReload");
            nudAddRange.DataBindings.Add("IntValue", cratesSettings, "AddRange");
            nudHeal.DataBindings.Add("IntValue", cratesSettings, "Heal");
            nudBomb.DataBindings.Add("IntValue", cratesSettings, "Bomb");
            nudStealth.DataBindings.Add("IntValue", cratesSettings, "Stealth");
            nudTeleport.DataBindings.Add("IntValue", cratesSettings, "Teleport");
            nudKill.DataBindings.Add("IntValue", cratesSettings, "Kill");
            nudUncloakAll.DataBindings.Add("IntValue", cratesSettings, "UncloakAll");
            nudReshroud.DataBindings.Add("IntValue", cratesSettings, "Reshroud");
            nudUnshroud.DataBindings.Add("IntValue", cratesSettings, "Unshroud");
            nudRadar.DataBindings.Add("IntValue", cratesSettings, "Radar");
            nudArmageddon.DataBindings.Add("IntValue", cratesSettings, "Armageddon");
            nudSuper.DataBindings.Add("IntValue", cratesSettings, "Super");
            nudDensity.DataBindings.Add("IntValue", cratesSettings, "Density");
            nudIonFactor.DataBindings.Add("IntValue", cratesSettings, "IonFactor");
            nudCrateTimer.DataBindings.Add("IntValue", cratesSettings, "CrateTimer");
        }

        private void RemoveDataBindings()
        {
            nudAddStrength.DataBindings.Clear();
            nudAddWeapon.DataBindings.Clear();
            nudAddSpeed.DataBindings.Clear();
            nudRapidReload.DataBindings.Clear();
            nudAddRange.DataBindings.Clear();
            nudHeal.DataBindings.Clear();
            nudBomb.DataBindings.Clear();
            nudStealth.DataBindings.Clear();
            nudTeleport.DataBindings.Clear();
            nudKill.DataBindings.Clear();
            nudUncloakAll.DataBindings.Clear();
            nudReshroud.DataBindings.Clear();
            nudUnshroud.DataBindings.Clear();
            nudRadar.DataBindings.Clear();
            nudArmageddon.DataBindings.Clear();
            nudSuper.DataBindings.Clear();
            nudDensity.DataBindings.Clear();
            nudIonFactor.DataBindings.Clear();
            nudCrateTimer.DataBindings.Clear();
        }
    }
}
