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
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public partial class PlayerSettings : UserControl
    {
        private readonly PropertyTracker<House> houseSettingsTracker;
        IGamePlugin plugin;
        private bool initDone = false;

        public PlayerSettings(IGamePlugin plugin, PropertyTracker<House> houseSettingsTracker)
        {
            this.plugin = plugin;
            this.houseSettingsTracker = houseSettingsTracker;
            InitializeComponent();
            edgeComboBox.DataSource = Globals.MapEdges.Select(me => ListItem.Create(me)).ToArray();
            edgeComboBox.ValueMember = "Value";
            edgeComboBox.DisplayMember = "Label";
            edgeComboBox.DataBindings.Add("SelectedValue", houseSettingsTracker, "Edge", false, DataSourceUpdateMode.OnPropertyChanged);
            creditsNud.DataBindings.Add("Value", houseSettingsTracker, "Credits", false, DataSourceUpdateMode.OnPropertyChanged);
            maxBuildingsNud.DataBindings.Add("Value", houseSettingsTracker, "MaxBuilding", false, DataSourceUpdateMode.OnPropertyChanged);
            maxUnitsNud.DataBindings.Add("Value", houseSettingsTracker, "MaxUnit", false, DataSourceUpdateMode.OnPropertyChanged);

            switch (plugin.GameInfo.GameType)
            {
                case GameType.TiberianDawn:
                case GameType.SoleSurvivor:
                    maxInfantryNud.Visible = maxInfantryLbl.Visible = false;
                    maxVesselsNud.Visible = maxVesselsLbl.Visible = false;
                    techLevelNud.Visible = techLevelLbl.Visible = false;
                    iqNud.Visible = iqLbl.Visible = false;
                    playerControlCheckBox.Visible = playerControlLbl.Visible = false;
                    break;
                case GameType.RedAlert:
                    maxInfantryNud.DataBindings.Add("Value", houseSettingsTracker, "MaxInfantry", false, DataSourceUpdateMode.OnPropertyChanged);
                    maxVesselsNud.DataBindings.Add("Value", houseSettingsTracker, "MaxVessel", false, DataSourceUpdateMode.OnPropertyChanged);
                    techLevelNud.DataBindings.Add("Value", houseSettingsTracker, "TechLevel", false, DataSourceUpdateMode.OnPropertyChanged);
                    iqNud.DataBindings.Add("Value", houseSettingsTracker, "IQ", false, DataSourceUpdateMode.OnPropertyChanged);
                    playerControlCheckBox.DataBindings.Add("Checked", houseSettingsTracker, "PlayerControl", false, DataSourceUpdateMode.OnPropertyChanged);
                    break;
            }
        }

        /// <summary>
        /// Call this after the control is added, to ensure all resize and selecting weirdness is complete.
        /// </summary>
        public void InitAlliances()
        {
            playersListBox.BeginUpdate();
            playersListBox.SelectedIndexChanged -= playersListBox_SelectedIndexChanged;
            playersListBox.Items.Clear();
            ListItem<int>[] housesArray = plugin.Map.HousesForAlliances.Select(h => ListItem.Create(h.Type.ID, h.Type.Name)).ToArray();
            playersListBox.Items.AddRange(housesArray);
            if (houseSettingsTracker.TryGetMember("Allies", out AlliesMask mask))
            {
                foreach (var id in mask)
                {
                    int index = ListItem.GetIndexInList(id, housesArray);
                    if (index != -1)
                    {
                        playersListBox.SetSelected(index, true);
                    }
                }
            }
            initDone = true;
            playersListBox_Resize(playersListBox, new EventArgs());
            playersListBox.EndUpdate();
            playersListBox.SelectedIndexChanged += playersListBox_SelectedIndexChanged;
        }

        private void playersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var allies = 0;
            foreach (ListItem<int> selectedItem in playersListBox.SelectedItems)
            {
                allies |= 1 << selectedItem.Value;
            }
            houseSettingsTracker.TrySetMember("Allies", new AlliesMask(allies));
        }

        private void playersListBox_Resize(object sender, EventArgs e)
        {
            if (!initDone)
                return;
            int min = -1;
            int max = -1;
            foreach (int index in playersListBox.SelectedIndices)
            {
                min = min == -1 ? index : Math.Min(min, index);
                max = max == -1 ? index : Math.Max(max, index);
            }
            if (min == -1 || max == -1)
            {
                playersListBox.TopIndex = 0;
                return;
            }
            int diff = max - min + 1;
            int items = (playersListBox.ClientRectangle.Height + (playersListBox.ItemHeight - 1)) / playersListBox.ItemHeight;
            int itemDiff = (items - diff) / 2;
            playersListBox.TopIndex = Math.Max(0, min - itemDiff);
            playersListBox.Invalidate();
        }
    }
}
