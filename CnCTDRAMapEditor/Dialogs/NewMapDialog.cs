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

        public String Theater
        {
            get { return ListItem.GetValueFromListBox<TheaterType>(lbTheaters)?.Name; }
        }

        public bool MegaMap
        {
            get
            {
                GameInfo gi = gameInfos[(int)GameType];
                return gi.MegamapSupport && (!gi.MegamapOptional || chkMegamap.Checked);
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
                lbGames.Items.Add(new ListItem<GameType>(gt, this.gameInfos[(int)gt].Name));
            }
            lbGames.SelectedIndex = 0;
            LbGames_SelectedIndexChanged(null, null);
        }

        private void LbGames_SelectedIndexChanged(Object sender, EventArgs e)
        {
            try
            {
                loading = true;
                if (lbGames.SelectedIndex >= gameTypes.Length)
                {
                    lbGames.SelectedIndex = 0;
                    return;
                }
                String selectedTheater = lbTheaters.Text;
                GameType gameType = GameType;
                lbTheaters.Items.Clear();
                int selectIndex = -1;
                GameInfo gi = gameInfos[(int)gameType];
                TheaterType[] ttypes = gi.AvailableTheaters;
                for (Int32 i = 0; i < ttypes.Length; ++i)
                {
                    TheaterType tt = ttypes[i];
                    lbTheaters.Items.Add(new ListItem<TheaterType>(tt, tt.Name));
                    if (theaterChanged && String.Equals(selectedTheater, tt.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        selectIndex = i;
                    }
                }
                lbTheaters.SelectedIndex = selectIndex != -1 ? selectIndex : 0;
                if (!gi.MegamapSupport)
                {
                    chkMegamap.Checked = false;
                    chkMegamap.Visible = false;
                }
                else
                {
                    if (gi.MegamapOptional)
                    {
                        chkMegamap.Visible = true;
                        chkMegamap.Checked = gi.MegamapDefault || optionalsChecked[(int)gameType];
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

        private void lbTheaters_SelectedIndexChanged(Object sender, EventArgs e)
        {
            AdjustBottomInfo();
            if (!loading)
            {
                theaterChanged = true;
            }

        }

        private void ChkMegamap_CheckedChanged(Object sender, EventArgs e)
        {
            GameType gameType = GameType;
            GameInfo gi = gameInfos[(int)gameType];
            if (gi.MegamapSupport && gi.MegamapOptional && !gi.MegamapDefault)
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
            bool allowMegamap = gi.MegamapSupport && gi.MegamapOptional;
            chkMegamap.Visible = allowMegamap;
            lblWarnMegamap.Visible = allowMegamap && chkMegamap.Checked && !gi.MegamapOfficial;
            TheaterType selected = ListItem.GetValueFromListBox<TheaterType>(lbTheaters);
            lblWarnModTheater.Visible = selected != null && selected.IsModTheater;
        }

        private void LbTheaters_MouseDoubleClick(Object sender, MouseEventArgs e)
        {
            int index = this.lbTheaters.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                this.DialogResult = DialogResult.OK;
            }
        }
    }
}
