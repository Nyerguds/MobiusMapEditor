//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
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
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public partial class BriefingSettings : UserControl
    {
        const string WarnRa = "Contains semicolon!";
        const string WarnTd = "Contains line breaks!";
        const string WarnRaFull = "Classic Red Alert's briefing text do not support semicolons\n" +
                                  "because the semicolon is a special character in the INI format.";
        const string WarnTdFull = "Classic Tiberian Dawn's briefing text does not support\n" +
                                  "line breaks unless you use the unofficial v1.06 patch.";
        const string WarnTd106 = "\nv1.06 line breaks are currently configured to be applied.";
        const string WarnTdAt = "\n\"@\" line breaks are currently configured to be applied.";
        private readonly IGamePlugin plugin;
        private readonly string lenText;

        public BriefingSettings(IGamePlugin plugin, PropertyTracker<BriefingSection> briefingSection)
        {
            InitializeComponent();
            this.plugin = plugin;
            lenText = lblLength.Text.Trim();
            txtBriefing.DataBindings.Add("Text", briefingSection, "Briefing");
            bool isRa = plugin.GameInfo.GameType == GameType.RedAlert;
            lblWarning.Text = isRa ? WarnRa : WarnTd;
            string warnFull;
            if (isRa)
            {
                warnFull = WarnRaFull;
            }
            else
            {
                warnFull = WarnTdFull;
                if (Globals.EnableTdClassicMultiLine) warnFull += WarnTdAt;
                else if (Globals.EnableTd106LineBreaks) warnFull += WarnTd106;
            }
            toolTip1.SetToolTip(lblWarning, warnFull);
        }

        private void TxtBriefing_TextChanged(object sender, EventArgs e)
        {
            string briefing = txtBriefing.Text.Replace('\t', ' ').Replace("\r\n", "\n").Replace('\r', '\n').Trim('\n', ' ');
            lblLength.Text = lenText + " " + briefing.Length;
            // Warn on TD line classic breaks even if Globals.UseTd106LineBreaks is enabled
            lblWarning.Visible = (plugin.GameInfo.GameType == GameType.RedAlert && Globals.WriteClassicBriefing && briefing.Contains(";"))
                || (plugin.GameInfo.GameType == GameType.TiberianDawn && Globals.WriteClassicBriefing && briefing.Contains("\n"));
            
        }
    }
}
