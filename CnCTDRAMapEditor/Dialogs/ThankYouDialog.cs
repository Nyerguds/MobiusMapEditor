﻿//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                     Version 2, December 2004
//
//  Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
//  Everyone is permitted to copy and distribute verbatim or modified
//  copies of this license document, and changing it is allowed as long
//  as the name is changed.
//
//             DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//    TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//   0. You just DO WHAT THE FUCK YOU WANT TO.
using MobiusEditor.Utility;
using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class ThankYouDialog : Form
    {
        readonly Object spLock = new Object();
        readonly Random random;
        SoundPlayer soundPlayer;
        int clicks = 0;
        const int maxclicks = 5;

        public ThankYouDialog()
        {
            InitializeComponent();
            // Compensate for scrollbar so text is still centered.
            this.txtEditorInfo.Width -= SystemInformation.VerticalScrollBarWidth;
            this.txtEditorInfo.Left += SystemInformation.VerticalScrollBarWidth;
            // Init randomizer on current clock ticks.
            this.random = new Random((int)(DateTime.Now.Ticks & 0xFFFFFFF));
            this.Text = "About " + Program.ProgramVersionTitle;
            this.txtEditorInfo.Text = Program.ProgramInfo.Replace("\n", Environment.NewLine);
            // Is re-centered for its new contents in the "shown" event.
            this.lblLink.Text = GeneralUtils.DoubleAmpersands(Program.GithubUrl);
        }

        private void lblImage_Click(Object sender, EventArgs e)
        {
            this.PlaySound(this.clicks);
            if (this.clicks < maxclicks)
            {
                this.clicks++;
            }
        }

        private void StopSound()
        {
            // Ensures this is only executed at the start of the threaded call, to stop and clean up the previous player.
            lock (spLock)
            {
                if (soundPlayer != null)
                {
                    Stream stream = soundPlayer.Stream;
                    try { soundPlayer.Stop(); } catch { /* ignore */ }
                    try { soundPlayer.Dispose(); } catch { /* ignore */ }
                    if (stream != null)
                    {
                        try { stream.Dispose(); } catch { /* ignore */ }
                    }
                }
            }
        }

        private void PlaySound(int clicks)
        {
            StopSound();
            using (MemoryStream ms = GetClipFromClicks(clicks))
            {
                if (ms != null)
                {
                    lock (spLock)
                    {
                        soundPlayer = new SoundPlayer(ms);
                        soundPlayer.Play();
                    }
                }
            }
        }

        private MemoryStream GetClipFromClicks(int clicks)
        {
            // Before reaching maxclicks: take sounds from switch-case, in reverse order.
            if (clicks < maxclicks)
            {
                // For a max of 5, this sends 4, 3, 2, 1, 0; three defaults, then the two specials.
                return GetClip(maxclicks - clicks - 1);
            }
            // After reaching maxclicks: take random sound.
            int randomNr = this.random.Next(maxclicks);
            return GetClip(randomNr);
        }

        private MemoryStream GetClip(int number)
        {
            switch (number)
            {
                case 0:
                    return GetMemoryStream(Properties.Resources.Mmg1);
                case 1:
                    return GetMemoryStream(Properties.Resources.Mtiber1);
                default:
                    return GetMemoryStream(Properties.Resources.Mthanks1);
            }
        }


        private MemoryStream GetMemoryStream(UnmanagedMemoryStream umm)
        {
            using (umm)
            {
                return GeneralUtils.CopyToMemoryStream(umm);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
                this.StopSound();
            }
            base.Dispose(disposing);
        }

        private void ThankYou_FormClosing(Object sender, FormClosingEventArgs e)
        {
            this.StopSound();
        }

        private void ThankYouDialog_Shown(Object sender, EventArgs e)
        {
            this.lblLink.Left = (this.ClientSize.Width - this.lblLink.Width) / 2;
        }

        private void lblLink_Click(Object sender, EventArgs e)
        {
            Process.Start(Program.GithubUrl);
        }
    }
}
