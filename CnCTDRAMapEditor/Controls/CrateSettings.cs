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

        private void BtnDefaults_Click(object sender, EventArgs e)
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
            nudAddStrength.DataBindings.Add("IntValue", cratesSettings, "AddStrength", false, DataSourceUpdateMode.OnPropertyChanged);
            nudAddWeapon.DataBindings.Add("IntValue", cratesSettings, "AddWeapon", false, DataSourceUpdateMode.OnPropertyChanged);
            nudAddSpeed.DataBindings.Add("IntValue", cratesSettings, "AddSpeed", false, DataSourceUpdateMode.OnPropertyChanged);
            nudRapidReload.DataBindings.Add("IntValue", cratesSettings, "RapidReload", false, DataSourceUpdateMode.OnPropertyChanged);
            nudAddRange.DataBindings.Add("IntValue", cratesSettings, "AddRange", false, DataSourceUpdateMode.OnPropertyChanged);
            nudHeal.DataBindings.Add("IntValue", cratesSettings, "Heal", false, DataSourceUpdateMode.OnPropertyChanged);
            nudBomb.DataBindings.Add("IntValue", cratesSettings, "Bomb", false, DataSourceUpdateMode.OnPropertyChanged);
            nudStealth.DataBindings.Add("IntValue", cratesSettings, "Stealth", false, DataSourceUpdateMode.OnPropertyChanged);
            nudTeleport.DataBindings.Add("IntValue", cratesSettings, "Teleport", false, DataSourceUpdateMode.OnPropertyChanged);
            nudKill.DataBindings.Add("IntValue", cratesSettings, "Kill", false, DataSourceUpdateMode.OnPropertyChanged);
            nudUncloakAll.DataBindings.Add("IntValue", cratesSettings, "UncloakAll", false, DataSourceUpdateMode.OnPropertyChanged);
            nudReshroud.DataBindings.Add("IntValue", cratesSettings, "Reshroud", false, DataSourceUpdateMode.OnPropertyChanged);
            nudUnshroud.DataBindings.Add("IntValue", cratesSettings, "Unshroud", false, DataSourceUpdateMode.OnPropertyChanged);
            nudRadar.DataBindings.Add("IntValue", cratesSettings, "Radar", false, DataSourceUpdateMode.OnPropertyChanged);
            nudArmageddon.DataBindings.Add("IntValue", cratesSettings, "Armageddon", false, DataSourceUpdateMode.OnPropertyChanged);
            nudSuper.DataBindings.Add("IntValue", cratesSettings, "Super", false, DataSourceUpdateMode.OnPropertyChanged);
            nudDensity.DataBindings.Add("IntValue", cratesSettings, "Density", false, DataSourceUpdateMode.OnPropertyChanged);
            nudIonFactor.DataBindings.Add("IntValue", cratesSettings, "IonFactor", false, DataSourceUpdateMode.OnPropertyChanged);
            nudCrateTimer.DataBindings.Add("IntValue", cratesSettings, "CrateTimer", false, DataSourceUpdateMode.OnPropertyChanged);
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
