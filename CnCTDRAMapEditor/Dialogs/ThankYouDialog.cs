using MobiusEditor.Utility;
using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Text;
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
            random = new Random((int)(DateTime.Now.Ticks & 0xFFFFFFF));
            this.Text = "About " + Program.ProgramVersionTitle;
            txtEditorInfo.Text = GeneralUtils.DoubleAmpersands(Program.ProgramInfo.Replace("\n", "\r\n"));
            lblLink.Text = GeneralUtils.DoubleAmpersands(Program.GithubUrl);
        }

        private void lblImage_Click(Object sender, EventArgs e)
        {
            this.PlaySoundFile(this.clicks);
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

        private void PlaySoundFile(int clicks)
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
            if (clicks < maxclicks)
            {
                return GetClip(maxclicks - clicks - 1);
            }
            clicks = this.random.Next(maxclicks);
            return GetClip(clicks);
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
                return CopyToBuffer(umm);
            }
        }

        private MemoryStream CopyToBuffer(UnmanagedMemoryStream umm)
        {
            byte[] buffer = new byte[umm.Length];
            umm.Read(buffer, 0, buffer.Length);
            MemoryStream ms = new MemoryStream();
            ms.Write(buffer, 0, buffer.Length);
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                StopSound();
            }
            base.Dispose(disposing);
        }

        private void ThankYou_FormClosing(Object sender, FormClosingEventArgs e)
        {
            StopSound();
        }

        private void ThankYouDialog_Shown(Object sender, EventArgs e)
        {
            lblLink.Left = (this.ClientSize.Width - lblLink.Width) / 2;
        }

        private void lblLink_Click(Object sender, EventArgs e)
        {
            Process.Start(Program.GithubUrl);
        }
    }
}
