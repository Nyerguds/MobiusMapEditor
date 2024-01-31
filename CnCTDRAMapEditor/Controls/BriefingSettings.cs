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
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public partial class BriefingSettings : UserControl
    {
        IGamePlugin plugin;
        string lenText;
        public BriefingSettings(IGamePlugin plugin, PropertyTracker<BriefingSection> briefingSection)
        {
            InitializeComponent();
            this.plugin = plugin;
            lenText = lblLength.Text;
            txtBriefing.DataBindings.Add("Text", briefingSection, "Briefing");
        }

        private void txtBriefing_TextChanged(System.Object sender, System.EventArgs e)
        {
            string briefing = txtBriefing.Text.Replace('\t', ' ').Replace("\r\n", "\n").Replace('\r', '\n').Trim('\n', ' ');
            lblLength.Text = lenText.Trim() + " " + briefing.Length;
            lblSemicolon.Visible = plugin.GameInfo.GameType == GameType.RedAlert && Globals.WriteClassicBriefing && briefing.Contains(";");
        }
    }
}
