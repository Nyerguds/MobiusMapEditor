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
            string noTheme = plugin.Map.ThemeEmpty;
            themeData.Sort(new ExplorerComparer());
            themeData.RemoveAll(v => noTheme.Equals(v, StringComparison.OrdinalIgnoreCase));
            themeData.Insert(0, noTheme);
            themeComboBox.DataSource = themeData;
            // No need for matching to index here; [Basic] saves it by name, not index.
            var movData = plugin.Map.MovieTypes.ToList();
            string noMovie = plugin.Map.MovieEmpty;
            movData.Sort(new ExplorerComparer());
            movData.RemoveAll(v => noMovie.Equals(v, StringComparison.OrdinalIgnoreCase));
            movData.Insert(0, noMovie);
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
            themeComboBox.DataBindings.Add("SelectedItem", basicSection, "Theme");
            switch (plugin.GameType)
            {
                case GameType.TiberianDawn:
                    isSinglePlayerCheckBox.DataBindings.DefaultDataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
                    isSinglePlayerCheckBox.DataBindings.Add("Checked", basicSection, "SoloMission");
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
                    introComboBox.DataBindings.Add("Text", basicSection, "Intro");
                    briefComboBox.DataBindings.Add("Text", basicSection, "Brief");
                    actionComboBox.DataBindings.Add("Text", basicSection, "Action");
                    winComboBox.DataBindings.Add("Text", basicSection, "Win");
                    win2ComboBox.DataBindings.Add("Text", basicSection, "Win2");
                    win3ComboBox.DataBindings.Add("Text", basicSection, "Win3");
                    win4ComboBox.DataBindings.Add("Text", basicSection, "Win4");
                    loseComboBox.DataBindings.Add("Text", basicSection, "Lose");
                    CheckSinglePlayerOptions();
                    break;
                case GameType.RedAlert:
                    isSinglePlayerCheckBox.DataBindings.DefaultDataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
                    isSinglePlayerCheckBox.DataBindings.Add("Checked", basicSection, "SoloMission");
                    buildLevelNud.Visible = buildLevelLabel.Visible = false;
                    baseComboBox.DataBindings.Add("SelectedItem", basicSection, "BasePlayer");
                    hasExpansionUnitsCheckBox.DataBindings.Add("Checked", basicSection, "ExpansionEnabled");
                    introComboBox.DataBindings.Add("SelectedItem", basicSection, "Intro");
                    briefComboBox.DataBindings.Add("SelectedItem", basicSection, "Brief");
                    actionComboBox.DataBindings.Add("SelectedItem", basicSection, "Action");
                    winComboBox.DataBindings.Add("SelectedItem", basicSection, "Win");
                    win2ComboBox.DataBindings.Add("SelectedItem", basicSection, "Win2");
                    win3ComboBox.DataBindings.Add("SelectedItem", basicSection, "Win3");
                    win4ComboBox.DataBindings.Add("SelectedItem", basicSection, "Win4");
                    loseComboBox.DataBindings.Add("SelectedItem", basicSection, "Lose");
                    CheckSinglePlayerOptions();
                    break;
                case GameType.SoleSurvivor:
                    isSinglePlayerCheckBox.Visible = false;
                    buildLevelNud.DataBindings.Add("Value", basicSection, "BuildLevel");
                    baseLabel.Visible = baseComboBox.Visible = false;
                    hasExpansionUnitsCheckBox.Visible = false;
                    introComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    briefComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    actionComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    winComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    introComboBox.DataBindings.Add("Text", basicSection, "Intro");
                    briefComboBox.DataBindings.Add("Text", basicSection, "Brief");
                    actionComboBox.DataBindings.Add("Text", basicSection, "Action");
                    winComboBox.DataBindings.Add("Text", basicSection, "Win");
                    loseComboBox.DataBindings.Add("Text", basicSection, "Lose");
                    // Irrelevant for SS; it's all classic.
                    lblCarryoverClassic.Visible = false;
                    lblThemeClassic.Visible = false;
                    // Disable these. Labels and stuff too.
                    win2Label.Visible = win2ComboBox.Visible = lblWin2Remaster.Visible = false;
                    win3Label.Visible = win3ComboBox.Visible = lblWin3Remaster.Visible = false;
                    win4Label.Visible = win4ComboBox.Visible = lblWin4Remaster.Visible = false;
                    break;
            }
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
