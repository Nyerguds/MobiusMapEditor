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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class NewMapDialog : Form
    {
        private GameType[] gameTypes = new[]
        {
            GameType.TiberianDawn,
            GameType.RedAlert,
            GameType.SoleSurvivor
        };

        private Dictionary<GameType, String> gameTypeNames = new Dictionary<GameType, string>
        {
            { GameType.TiberianDawn, "Tiberian Dawn" },
            { GameType.RedAlert, "Red Alert" },
            { GameType.SoleSurvivor, "Sole Survivor" }
        };

        private Dictionary<GameType, TheaterType[]> theaters = new Dictionary<GameType, TheaterType[]>()
        {
            { GameType.TiberianDawn, TiberianDawn.TheaterTypes.GetTypes().ToArray() },
            { GameType.RedAlert, RedAlert.TheaterTypes.GetTypes().ToArray() },
            { GameType.SoleSurvivor, TiberianDawn.TheaterTypes.GetTypes().ToArray() }
        };

        private bool tdMegaMapChecked = false;

        private GameType gameType = GameType.TiberianDawn;
        public GameType GameType
        {
            get
            {
                return ListItem.GetValueFromListBox<GameType>(lbGames);
            }
            set
            {
                lbGames.SelectedIndex = Array.IndexOf(gameTypes, value);
            }
        }

        public String Theater
        {
            get
            {
                return ListItem.GetValueFromListBox<TheaterType>(lbTheaters)?.Name;
            }
        }

        public bool MegaMap
        {
            get
            {
                return chkMegamap.Checked;
            }
        }

        public bool SinglePlayer
        {
            get
            {
                return chkSingleplayer.Checked && gameType != GameType.SoleSurvivor;
            }
        }

        public NewMapDialog(bool fromImage)
        {
            InitializeComponent();
            if (fromImage)
            {
                this.Text = "New Map From Image";
            }
            foreach (GameType gt in this.gameTypes)
            {
                lbGames.Items.Add(new ListItem<GameType>(gt, this.gameTypeNames[gt]));
            }
            lbGames.SelectedIndex = 0;
            lbGames_SelectedIndexChanged(null, null);
        }

        private void lbGames_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if (lbGames.SelectedIndex >= gameTypes.Length)
            {
                lbGames.SelectedIndex = 0;
                return;
            }
            String selectedTheater = lbTheaters.Text;
            gameType = gameTypes[lbGames.SelectedIndex];
            lbTheaters.Items.Clear();
            int selectIndex = -1;
            if (theaters.TryGetValue(gameType, out TheaterType[] ttypes))
            {
                for (Int32 i = 0; i < ttypes.Length; ++i)
                {
                    TheaterType tt = ttypes[i];
                    lbTheaters.Items.Add(new ListItem<TheaterType>(tt, tt.Name));
                    if (String.Equals(selectedTheater, tt.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        selectIndex = i;
                    }
                }
            }
            lbTheaters.SelectedIndex = selectIndex != -1 ? selectIndex : 0;
            switch (gameType)
            {
                case GameType.TiberianDawn:
                    chkMegamap.Checked = tdMegaMapChecked;
                    break;
                case GameType.RedAlert:
                    break;
                case GameType.SoleSurvivor:
                    chkMegamap.Checked = true;
                    break;
            }
            AdjustBottomInfo();
        }

        private void chkMegamap_CheckedChanged(Object sender, EventArgs e)
        {
            if (gameType == GameType.TiberianDawn)
            {
                tdMegaMapChecked = chkMegamap.Checked;
            }
            AdjustBottomInfo();
        }

        private void AdjustBottomInfo()
        {
            chkSingleplayer.Visible = gameType != GameType.SoleSurvivor;
            chkMegamap.Visible = gameType == GameType.TiberianDawn || gameType == GameType.SoleSurvivor;
            lblWarning.Visible = !Globals.UseClassicFiles && (chkMegamap.Visible && chkMegamap.Checked) && gameType != GameType.SoleSurvivor;
        }
    }
}
