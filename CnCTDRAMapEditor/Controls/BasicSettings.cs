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
    public partial class BasicSettings : UserControl
    {
        public BasicSettings(IGamePlugin plugin, PropertyTracker<BasicSection> basicSection)
        {
            InitializeComponent();
            playerComboBox.DataSource = plugin.Map.Houses.Select(h => h.Type.Name).ToArray();
            baseComboBox.DataSource = plugin.Map.Houses.Select(h => h.Type.Name).ToArray();
            var themeData = plugin.Map.ThemeTypes.ToList();
            themeData.Sort(new ExplorerComparer());
            themeData.RemoveAll(v => "No Theme".Equals(v, StringComparison.InvariantCultureIgnoreCase));
            themeData.Insert(0, "No Theme");           
            themeComboBox.DataSource = themeData;
            // No need for matching to index here; [Basic] saves it by name, not index.
            var movData = plugin.Map.MovieTypes.ToList();
            movData.Sort(new ExplorerComparer());
            movData.RemoveAll(v => "x".Equals(v, StringComparison.InvariantCultureIgnoreCase));
            movData.Insert(0, "x");
            introComboBox.DataSource = movData.ToArray();
            briefComboBox.DataSource = movData.ToArray();
            actionComboBox.DataSource = movData.ToArray();
            winComboBox.DataSource = movData.ToArray();
            win2ComboBox.DataSource = movData.ToArray();
            win3ComboBox.DataSource = movData.ToArray();
            win4ComboBox.DataSource = movData.ToArray();
            loseComboBox.DataSource = movData.ToArray();
            carryOverMoneyNud.DataBindings.Add("Value", basicSection, "CarryOverMoney");
            nameTxt.DataBindings.Add("Text", basicSection, "Name");
            // Disable "Percent"; it's a useless Dune II leftover.
            //percentNud.DataBindings.Add("Value", basicSection, "Percent");
            percentNud.Visible = false;
            percentLabel.Visible = false;
            playerComboBox.DataBindings.Add("SelectedItem", basicSection, "Player");
            authorTxt.DataBindings.Add("Text", basicSection, "Author");
            isSinglePlayerCheckBox.DataBindings.DefaultDataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
            isSinglePlayerCheckBox.DataBindings.Add("Checked", basicSection, "SoloMission");
            themeComboBox.DataBindings.Add("Text", basicSection, "Theme");
            introComboBox.DataBindings.Add("Text", basicSection, "Intro");
            briefComboBox.DataBindings.Add("Text", basicSection, "Brief");
            actionComboBox.DataBindings.Add("Text", basicSection, "Action");
            winComboBox.DataBindings.Add("Text", basicSection, "Win");
            win2ComboBox.DataBindings.Add("Text", basicSection, "Win2");
            win3ComboBox.DataBindings.Add("Text", basicSection, "Win3");
            win4ComboBox.DataBindings.Add("Text", basicSection, "Win4");
            loseComboBox.DataBindings.Add("Text", basicSection, "Lose");
            switch (plugin.GameType)
            {
                case GameType.TiberianDawn:
                    buildLevelNud.DataBindings.Add("Value", basicSection, "BuildLevel");
                    baseLabel.Visible = baseComboBox.Visible = false;
                    hasExpansionUnitsCheckBox.Visible = false;
                    introComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    briefComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    actionComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    winComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    win2ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    win3ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    win4ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    loseComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    break;
                case GameType.RedAlert:
                    buildLevelNud.Visible = buildLevelLabel.Visible = false;
                    baseComboBox.DataBindings.Add("SelectedItem", basicSection, "BasePlayer");
                    hasExpansionUnitsCheckBox.DataBindings.Add("Checked", basicSection, "ExpansionEnabled");
                    break;
            }

            CheckSinglePlayerOptions();
        }

        private void isSinglePlayerCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckSinglePlayerOptions();
        }

        private void CheckSinglePlayerOptions()
        {
            bool sp = isSinglePlayerCheckBox.Checked;
            themeComboBox.Enabled = introComboBox.Enabled = briefComboBox.Enabled = actionComboBox.Enabled = loseComboBox.Enabled = sp;
            winComboBox.Enabled = win2ComboBox.Enabled = win3ComboBox.Enabled = win4ComboBox.Enabled = sp;
        }
    }
}
