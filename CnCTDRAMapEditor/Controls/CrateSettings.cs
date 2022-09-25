using MobiusEditor.Utility;
using System;
using System.Drawing;
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

        private void CrateSettings_Load(Object sender, EventArgs e)
        {
            AdjustPanelSize(this, tableLayoutPanel1);
        }

        private void AdjustPanelSize(ScrollableControl panel, TableLayoutPanel tableLayoutPanel)
        {
            int maxX = 0;
            int maxY = 0;
            foreach (Control c in tableLayoutPanel.Controls)
            {
                maxX = Math.Max(maxX, c.Location.X + c.Width);
                maxY = Math.Max(maxY, c.Location.Y + c.Height);
            }
            panel.AutoScrollMinSize = new Size(maxX, maxY);
        }
    }
}
