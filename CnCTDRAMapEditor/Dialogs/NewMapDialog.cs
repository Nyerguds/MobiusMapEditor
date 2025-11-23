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
using MobiusEditor.Model;
using System;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class NewMapDialog : Form
    {
        private readonly GameInfo[] gameInfos;
        private readonly GameType[] gameTypes;
        private bool loading = false;
        private bool[] optionalsChecked;
        private bool theaterChanged = false;

        public GameType GameType
        {
            get { return ListItem.GetValueFromListBox<GameType>(lbGames); }
            set { lbGames.SelectedIndex = Array.IndexOf(gameTypes, value); }
        }

        public GameInfo GameInfo
        {
            get { return gameInfos[(int)GameType]; }
            set { lbGames.SelectedIndex = value == null ? -1 : Array.IndexOf(gameTypes, value.GameType); }
        }

        public string Theater
        {
            get { return ListItem.GetValueFromListBox<TheaterType>(lbTheaters)?.Name; }
        }

        public bool MegaMap
        {
            get
            {
                GameInfo gi = gameInfos[(int)GameType];
                return gi.MegamapIsSupported && (!gi.MegamapIsOptional || chkMegamap.Checked);
            }
        }

        public bool SinglePlayer
        {
            get
            {
                GameInfo gi = gameInfos[(int)GameType];
                return chkSingleplayer.Checked && gi.HasSinglePlayer;
            }
        }

        public NewMapDialog(bool fromImage)
        {
            this.gameInfos = GameTypeFactory.GetGameInfos();
            this.gameTypes = GameTypeFactory.GetGameTypes();
            optionalsChecked = new bool[this.gameTypes.Length];
            InitializeComponent();
            if (fromImage)
            {
                this.Text = "New Map From Image";
            }
            foreach (GameType gt in this.gameTypes)
            {
                GameInfo gi = this.gameInfos[(int)gt];
                if (gi != null)
                {
                    lbGames.Items.Add(ListItem.Create(gt, gi.Name));
                }
            }
            lbGames.SelectedIndex = 0;
            LbGames_SelectedIndexChanged(null, null);
        }

        private void LbGames_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                loading = true;
                if (lbGames.SelectedIndex >= gameTypes.Length)
                {
                    lbGames.SelectedIndex = 0;
                    return;
                }
                string selectedTheater = lbTheaters.Text;
                GameType gameType = GameType;
                lbTheaters.Items.Clear();
                int selectIndex = -1;
                GameInfo gi = gameInfos[(int)gameType];
                TheaterType[] ttypes = gi.AvailableTheaters;
                for (int i = 0; i < ttypes.Length; ++i)
                {
                    TheaterType tt = ttypes[i];
                    lbTheaters.Items.Add(ListItem.Create(tt, tt.Name));
                    if (theaterChanged && String.Equals(selectedTheater, tt.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        selectIndex = i;
                    }
                }
                lbTheaters.SelectedIndex = selectIndex != -1 ? selectIndex : 0;
                if (!gi.MegamapIsSupported)
                {
                    chkMegamap.Checked = false;
                    chkMegamap.Visible = false;
                }
                else
                {
                    if (gi.MegamapIsOptional)
                    {
                        chkMegamap.Visible = true;
                        chkMegamap.Checked = gi.MegamapIsDefault || optionalsChecked[(int)gameType];
                    }
                    else
                    {
                        chkMegamap.Visible = false;
                        chkMegamap.Checked = true;
                    }
                }
                AdjustBottomInfo();
            }
            finally
            {
                loading = false;
            }
        }

        private void lbTheaters_SelectedIndexChanged(object sender, EventArgs e)
        {
            AdjustBottomInfo();
            if (!loading)
            {
                theaterChanged = true;
            }

        }

        private void ChkMegamap_CheckedChanged(object sender, EventArgs e)
        {
            GameType gameType = GameType;
            GameInfo gi = gameInfos[(int)gameType];
            if (gi.MegamapIsSupported && gi.MegamapIsOptional && !gi.MegamapIsDefault)
            {
                optionalsChecked[(int)gameType] = chkMegamap.Checked;
            }
            AdjustBottomInfo();
        }

        private void AdjustBottomInfo()
        {
            GameType gameType = GameType;
            GameInfo gi = gameInfos[(int)gameType];
            chkSingleplayer.Visible = gi.HasSinglePlayer;
            bool allowMegamap = gi.MegamapIsSupported && gi.MegamapIsOptional;
            chkMegamap.Visible = allowMegamap;
            lblWarnMegamap.Visible = allowMegamap && chkMegamap.Checked && !gi.MegamapIsOfficial;
            TheaterType selected = ListItem.GetValueFromListBox<TheaterType>(lbTheaters);
            lblWarnModTheater.Visible = selected != null && selected.IsModTheater;
        }

        private void LbTheaters_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.lbTheaters.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                this.DialogResult = DialogResult.OK;
            }
        }
    }
}
