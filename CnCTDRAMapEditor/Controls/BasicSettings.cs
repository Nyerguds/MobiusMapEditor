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
using System.Collections.Generic;
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
            playerComboBox.ValueMember = "Value";
            playerComboBox.DisplayMember = "Label";
            playerComboBox.DataSource = plugin.Map.Houses.Select(t => ListItem.Create(t.Type.Name)).ToArray();
            baseComboBox.ValueMember = "Value";
            baseComboBox.DisplayMember = "Label";
            baseComboBox.DataSource = plugin.Map.Houses.Select(h => ListItem.Create(h.Type.Name)).ToArray();
            List<string> themeData = plugin.Map.ThemeTypes.ToList();
            string noTheme = plugin.Map.ThemeEmpty;
            themeData.Sort(new ExplorerComparer());
            themeData.RemoveAll(v => noTheme.Equals(v, StringComparison.OrdinalIgnoreCase));
            themeData.Insert(0, noTheme);
            themeComboBox.ValueMember = "Value";
            themeComboBox.DisplayMember = "Label";
            themeComboBox.DataSource = themeData.Select(v => ListItem.Create(v, v)).ToArray();
            // No need for matching to index here; [Basic] saves it by name, not index.
            List<string> movData = plugin.Map.MovieTypes.ToList();
            string noMovie = plugin.Map.MovieEmpty;
            movData.Sort(new ExplorerComparer());
            movData.RemoveAll(v => noMovie.Equals(v, StringComparison.OrdinalIgnoreCase));
            movData.Insert(0, noMovie);
            introComboBox.ValueMember = "Value";
            introComboBox.DisplayMember = "Label";
            introComboBox.DataSource = movData.Select(v => ListItem.Create(v)).ToArray();
            briefComboBox.ValueMember = "Value";
            briefComboBox.DisplayMember = "Label";
            briefComboBox.DataSource = movData.Select(v => ListItem.Create(v)).ToArray();
            actionComboBox.ValueMember = "Value";
            actionComboBox.DisplayMember = "Label";
            actionComboBox.DataSource = movData.Select(v => ListItem.Create(v)).ToArray();
            winComboBox.ValueMember = "Value";
            winComboBox.DisplayMember = "Label";
            winComboBox.DataSource = movData.Select(v => ListItem.Create(v)).ToArray();
            win2ComboBox.ValueMember = "Value";
            win2ComboBox.DisplayMember = "Label";
            win2ComboBox.DataSource = movData.Select(v => ListItem.Create(v)).ToArray();
            win3ComboBox.ValueMember = "Value";
            win3ComboBox.DisplayMember = "Label";
            win3ComboBox.DataSource = movData.Select(v => ListItem.Create(v)).ToArray();
            win4ComboBox.ValueMember = "Value";
            win4ComboBox.DisplayMember = "Label";
            win4ComboBox.DataSource = movData.Select(v => ListItem.Create(v)).ToArray();
            loseComboBox.ValueMember = "Value";
            loseComboBox.DisplayMember = "Label";
            loseComboBox.DataSource = movData.Select(v => ListItem.Create(v)).ToArray();
            carryOverMoneyNud.DataBindings.Add("Value", basicSection, "CarryOverMoney", false, DataSourceUpdateMode.OnPropertyChanged);
            nameTxt.DataBindings.Add("Text", basicSection, "Name", false, DataSourceUpdateMode.OnPropertyChanged);
            // Disable "Percent"; it's a useless Dune II leftover.
            //percentNud.DataBindings.Add("Value", basicSection, "Percent");
            percentNud.Visible = false;
            percentLabel.Visible = false;
            playerComboBox.DataBindings.Add("SelectedValue", basicSection, "Player", false, DataSourceUpdateMode.OnPropertyChanged);
            authorTxt.DataBindings.Add("Text", basicSection, "Author", false, DataSourceUpdateMode.OnPropertyChanged);
            themeComboBox.DataBindings.Add("SelectedValue", basicSection, "Theme", false, DataSourceUpdateMode.OnPropertyChanged);
            switch (plugin.GameInfo.GameType)
            {
                case GameType.TiberianDawn:
                    isSinglePlayerCheckBox.DataBindings.Add("Checked", basicSection, "SoloMission", false, DataSourceUpdateMode.OnPropertyChanged);
                    buildLevelNud.DataBindings.Add("Value", basicSection, "BuildLevel", false, DataSourceUpdateMode.OnPropertyChanged);
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
                    introComboBox.DataBindings.Add("SelectedValue", basicSection, "Intro", false, DataSourceUpdateMode.OnPropertyChanged);
                    briefComboBox.DataBindings.Add("SelectedValue", basicSection, "Brief", false, DataSourceUpdateMode.OnPropertyChanged);
                    actionComboBox.DataBindings.Add("SelectedValue", basicSection, "Action", false, DataSourceUpdateMode.OnPropertyChanged);
                    winComboBox.DataBindings.Add("SelectedValue", basicSection, "Win", false, DataSourceUpdateMode.OnPropertyChanged);
                    win2ComboBox.DataBindings.Add("SelectedValue", basicSection, "Win2", false, DataSourceUpdateMode.OnPropertyChanged);
                    win3ComboBox.DataBindings.Add("SelectedValue", basicSection, "Win3", false, DataSourceUpdateMode.OnPropertyChanged);
                    win4ComboBox.DataBindings.Add("SelectedValue", basicSection, "Win4", false, DataSourceUpdateMode.OnPropertyChanged);
                    loseComboBox.DataBindings.Add("SelectedValue", basicSection, "Lose", false, DataSourceUpdateMode.OnPropertyChanged);
                    CheckSinglePlayerOptions();
                    break;
                case GameType.RedAlert:
                    isSinglePlayerCheckBox.DataBindings.Add("Checked", basicSection, "SoloMission", false, DataSourceUpdateMode.OnPropertyChanged);
                    buildLevelNud.Visible = buildLevelLabel.Visible = false;
                    baseComboBox.DataBindings.Add("SelectedValue", basicSection, "BasePlayer", false, DataSourceUpdateMode.OnPropertyChanged);
                    hasExpansionUnitsCheckBox.DataBindings.Add("Checked", basicSection, "ExpansionEnabled", false, DataSourceUpdateMode.OnPropertyChanged);
                    introComboBox.DataBindings.Add("SelectedValue", basicSection, "Intro", false, DataSourceUpdateMode.OnPropertyChanged);
                    briefComboBox.DataBindings.Add("SelectedValue", basicSection, "Brief", false, DataSourceUpdateMode.OnPropertyChanged);
                    actionComboBox.DataBindings.Add("SelectedValue", basicSection, "Action", false, DataSourceUpdateMode.OnPropertyChanged);
                    winComboBox.DataBindings.Add("SelectedValue", basicSection, "Win", false, DataSourceUpdateMode.OnPropertyChanged);
                    win2ComboBox.DataBindings.Add("SelectedValue", basicSection, "Win2", false, DataSourceUpdateMode.OnPropertyChanged);
                    win3ComboBox.DataBindings.Add("SelectedValue", basicSection, "Win3", false, DataSourceUpdateMode.OnPropertyChanged);
                    win4ComboBox.DataBindings.Add("SelectedValue", basicSection, "Win4", false, DataSourceUpdateMode.OnPropertyChanged);
                    loseComboBox.DataBindings.Add("SelectedValue", basicSection, "Lose", false, DataSourceUpdateMode.OnPropertyChanged);
                    CheckSinglePlayerOptions();
                    break;
                case GameType.SoleSurvivor:
                    isSinglePlayerCheckBox.Visible = false;
                    buildLevelNud.DataBindings.Add("Value", basicSection, "BuildLevel", false, DataSourceUpdateMode.OnPropertyChanged);
                    baseLabel.Visible = baseComboBox.Visible = false;
                    hasExpansionUnitsCheckBox.Visible = false;
                    introComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    briefComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    actionComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    winComboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    introComboBox.DataBindings.Add("Text", basicSection, "Intro", false, DataSourceUpdateMode.OnPropertyChanged);
                    briefComboBox.DataBindings.Add("Text", basicSection, "Brief", false, DataSourceUpdateMode.OnPropertyChanged);
                    actionComboBox.DataBindings.Add("Text", basicSection, "Action", false, DataSourceUpdateMode.OnPropertyChanged);
                    winComboBox.DataBindings.Add("Text", basicSection, "Win", false, DataSourceUpdateMode.OnPropertyChanged);
                    loseComboBox.DataBindings.Add("Text", basicSection, "Lose", false, DataSourceUpdateMode.OnPropertyChanged);
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
