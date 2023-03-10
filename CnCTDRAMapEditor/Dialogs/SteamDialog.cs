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
using MobiusEditor.Controls;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class SteamDialog : Form
    {
        private static readonly string PreviewDirectory = Path.Combine(Path.GetTempPath(), "CnCRCMapEditor");
        private static readonly string PublishTempDirectory = Path.Combine(Path.GetTempPath(), "CnCRCMapEditorPublishUGC");
        private string defaultPreview;

        private readonly IGamePlugin plugin;
        private readonly Timer statusUpdateTimer = new Timer();
        private SimpleMultiThreading multiThreader;

        private bool isPublishing = false;
        SteamSection steamCloneSection = null;
        private bool mapWasPublished = false;
        public bool MapWasPublished => mapWasPublished;


        public SteamDialog(IGamePlugin plugin)
        {
            this.plugin = plugin;
            InitializeComponent();
            cmbVisibility.DataSource = new[]
            {
                new { Name = "Public", Value = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic },
                new { Name = "Friends Only", Value = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityFriendsOnly },
                new { Name = "Private", Value = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate }
            };
            multiThreader = new SimpleMultiThreading(this);
            statusUpdateTimer.Interval = 500;
            statusUpdateTimer.Tick += StatusUpdateTimer_Tick;
            Disposed += (o, e) => { (txtPreview.Tag as Image)?.Dispose(); };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (String.IsNullOrEmpty(plugin.Map.SteamSection.Title) && !String.IsNullOrEmpty(plugin.Map.BasicSection.Name))
            {
                plugin.Map.SteamSection.Title = plugin.Map.BasicSection.Name;
            }
            lblMapTitleData.Text = plugin.Map.BasicSection.Name;
            btnCopyFromMap.Enabled = !String.IsNullOrEmpty(lblMapTitleData.Text);
            txtTitle.Text = plugin.Map.SteamSection.Title ?? String.Empty;
            string description = plugin.Map.SteamSection.Description ?? String.Empty;
            txtDescription.Text = GeneralUtils.RestoreLinebreaks(description, '@', Environment.NewLine);
            txtPreview.Text = plugin.Map.SteamSection.PreviewFile ?? String.Empty;
            cmbVisibility.SelectedValue = plugin.Map.SteamSection.Visibility;
            UpdatePublishButton();
            btnFromBriefing.Enabled = plugin.Map.BasicSection.SoloMission;
            FixSizeAndLocation(800, 500);
            statusUpdateTimer.Start();
        }

        private void FixSizeAndLocation(Int32 width, Int32 height)
        {
            // For some reason, setting the form's default size larger than the minimum size
            // makes the UI mess up when manually resizing it to a smaller size after it is shown.
            // So instead it is initialised with the minimum size, and resized and repositioned
            // after it is initialised, which fixes the issue.
            Size oldSize = this.Size;
            int offsX = (width - oldSize.Width) / 2;
            int offsY = (height - oldSize.Height) / 2;
            this.Size = new Size(width, height);
            Point curLoc = this.Location;
            this.Location = new Point(Math.Max(0, curLoc.X - offsX), Math.Max(0, curLoc.Y - offsY));
        }

        private void UpdatePublishButton()
        {
            bool isPublished = plugin.Map.SteamSection.PublishedFileId != PublishedFileId_t.Invalid.m_PublishedFileId;
            btnPublishMap.SplitWidth = isPublished ? MenuButton.DefaultSplitWidth : 0;
            btnGoToSteam.Text = "&Go to " + (isPublished ? "published item" : "Steam Workshop");
        }

        private void StatusUpdateTimer_Tick(object sender, EventArgs e)
        {
            var status = SteamworksUGC.CurrentOperation?.Status;
            if (!string.IsNullOrEmpty(status))
            {
                lblStatus.Text = status;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            statusUpdateTimer.Stop();
            statusUpdateTimer.Dispose();
        }

        protected virtual void OnPublishSuccess()
        {
            lblStatus.Text = "Map published.";
            isPublishing = false;
            if (steamCloneSection != null)
            {
                plugin.Map.SteamSection.PublishedFileId = steamCloneSection.PublishedFileId;
                steamCloneSection = null;
            }
            mapWasPublished = true;
            UpdatePublishButton();
            EnableControls(true);
        }

        protected virtual void OnOperationFailed(string status)
        {
            lblStatus.Text = status;
            isPublishing = false;
            EnableControls(true);
        }

        private void EnableControls(bool enable, string labelText)
        {
            EnableControls(enable);
            lblStatus.Text = labelText ?? String.Empty;
        }

        private void EnableControls(bool enable)
        {
            lblMapTitle.Enabled = enable;
            lblMapTitleData.Enabled = enable;
            lblSteamTitle.Enabled = enable;
            txtTitle.Enabled = enable;
            btnCopyFromMap.Enabled = enable;
            lblVisibility.Enabled = enable;
            cmbVisibility.Enabled = enable;
            lblPreview.Enabled = enable;
            txtPreview.Enabled = enable;
            btnPreview.Enabled = enable;
            btnDefaultPreview.Enabled = enable;
            lblDescription.Enabled = enable;
            btnFromBriefing.Enabled = enable && plugin != null && plugin.Map.BasicSection.SoloMission;
            txtDescription.Enabled = enable;
            btnPublishMap.Enabled = enable;
            btnGoToSteam.Enabled = enable;
            btnClose.Enabled = !isPublishing || enable;
            lblLegal.Enabled = enable;
        }

        private void btnGoToSteam_Click(object sender, EventArgs e)
        {
            String workshopUrl;
            ulong publishId = plugin.Map.SteamSection.PublishedFileId;
            if (publishId == 0)
            {
                workshopUrl = SteamworksUGC.WorkshopURL;
            }
            else
            {
                workshopUrl = SteamworksUGC.GetWorkshopItemURL(publishId);
            }
            if (!string.IsNullOrEmpty(workshopUrl))
            {
                Process.Start(workshopUrl);
            }
        }

        private void btnPublishMap_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTitle.Text))
            {
                MessageBox.Show(this, "Steam Title is required.");
                return;
            }
            if (string.IsNullOrEmpty(txtDescription.Text))
            {
                MessageBox.Show("Description is required.");
                return;
            }
            if (txtPreview.Tag == null)
            {
                MessageBox.Show("Preview image is required.");
                return;
            }
            if (string.IsNullOrEmpty(plugin.Map.BasicSection.Name))
            {
                plugin.Map.BasicSection.Name = txtTitle.Text;
            }
            if (string.IsNullOrEmpty(plugin.Map.BasicSection.Author))
            {
                plugin.Map.BasicSection.Author = SteamFriends.GetPersonaName();
            }
            plugin.Map.SteamSection.PreviewFile = txtPreview.Text;
            plugin.Map.SteamSection.Title = txtTitle.Text;
            plugin.Map.SteamSection.Description = GeneralUtils.ReplaceLinebreaks(txtDescription.Text, '@');
            plugin.Map.SteamSection.Visibility = (ERemoteStoragePublishedFileVisibility)cmbVisibility.SelectedValue;
            Directory.CreateDirectory(PublishTempDirectory);
            foreach (var file in new DirectoryInfo(PublishTempDirectory).EnumerateFiles())
            {
                file.Delete();
            }
            var pgmPath = Path.Combine(PublishTempDirectory, "MAPDATA.PGM");
            multiThreader.ExecuteThreaded(() => SavePgm(PublishTempDirectory, pgmPath), SaveDone, true, EnableControls, "Saving map");
        }

        private string SavePgm(string tempPath, string pgmPath)
        {
            try
            {
                plugin.Save(pgmPath, FileType.PGM, txtPreview.Tag as Bitmap, false);
            }
            catch
            {
                return null;
            }
            return tempPath;
        }

        private void SaveDone(String sendPath)
        {
            if (sendPath == null)
            {
                lblStatus.Text = "Save failed.";
                return;
            }
            var tags = new List<string>();
            switch (plugin.GameType)
            {
                case GameType.TiberianDawn:
                    tags.Add("TD");
                    break;
                case GameType.RedAlert:
                    tags.Add("RA");
                    break;
            }
            if (plugin.Map.BasicSection.SoloMission)
            {
                tags.Add("SinglePlayer");
            }
            else
            {
                tags.Add("MultiPlayer");
            }
            // Clone to have version without line breaks to give to the Steam publish.
            INI ini = new INI();
            INI.WriteSection(new MapContext(plugin.Map, false), ini.Sections.Add("Steam"), plugin.Map.SteamSection);
            var steamSection = ini.Sections.Extract("Steam");
            steamCloneSection = new SteamSection();
            INI.ParseSection(new MapContext(plugin.Map, false), steamSection, steamCloneSection);
            // Restore original description from control, with actual line breaks instead of '@' replacements.
            steamCloneSection.Description = txtDescription.Text;
            if (SteamworksUGC.PublishUGC(sendPath, steamCloneSection, tags, OnPublishSuccess, OnOperationFailed))
            {
                isPublishing = true;
                lblStatus.Text = SteamworksUGC.CurrentOperation.Status;
                EnableControls(false);
            }
        }

        private void previewBtn_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                AutoUpgradeEnabled = false,
                RestoreDirectory = true,
                Filter = "Preview Files (*.png)|*.png",
                CheckFileExists = true,
            };
            if (!string.IsNullOrEmpty(txtPreview.Text) && File.Exists(txtPreview.Text))
            {
                ofd.InitialDirectory = Path.GetDirectoryName(txtPreview.Text);
                ofd.FileName = Path.GetFileName(txtPreview.Text);
            }
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtPreview.Text = ofd.FileName;
            }
        }

        private void publishAsNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            plugin.Map.SteamSection.PublishedFileId = PublishedFileId_t.Invalid.m_PublishedFileId;
            UpdatePublishButton();
            btnPublishMap_Click(btnPublishMap, new EventArgs());
        }

        private void TxtPreview_TextChanged(object sender, EventArgs e)
        {
            refreshPreviewImage();
        }

        private void refreshPreviewImage()
        {
            try
            {
                (txtPreview.Tag as Image)?.Dispose();
                Bitmap preview = null;
                if (File.Exists(txtPreview.Text))
                {
                    using (Bitmap b = new Bitmap(txtPreview.Text))
                    {
                        preview = b.FitToBoundingBox(Globals.MapPreviewSize.Width, Globals.MapPreviewSize.Height, Color.Black);
                    }
                }
                txtPreview.Tag = preview;
            }
            catch (Exception)
            {
                txtPreview.Tag = null;
            }
        }

        private void BtnCopyFromMap_Click(Object sender, EventArgs e)
        {
            txtTitle.Text = lblMapTitleData.Text;
        }

        private void BtnGenerateDescription_Click(Object sender, EventArgs e)
        {
            txtDescription.Text = plugin.Map.BriefingSection.Briefing;
        }

        private void btnDefaultPreview_Click(Object sender, EventArgs e)
        {
            txtPreview.Text = defaultPreview;
        }

        private void SteamDialog_Shown(Object sender, EventArgs e)
        {
            multiThreader.ExecuteThreaded(() => GeneratePreviews(plugin), HandleGeneratedPreview, true, EnableControls, "Generating map previews");
        }

        private String GeneratePreviews(IGamePlugin plugin)
        {
            Directory.CreateDirectory(PreviewDirectory);
            string defaultPreview = Path.Combine(PreviewDirectory, "Minimap.png");
            // Now generates all contents.
            using (Bitmap pr = plugin.Map.GenerateWorkshopPreview(plugin.GameType, true).ToBitmap())
            {
                pr.Save(defaultPreview, ImageFormat.Png);
            }
            if (plugin.Map.BasicSection.SoloMission)
            {
                var soloBannerPath = Path.Combine(PreviewDirectory, "SoloBanner.png");
                using (Bitmap bm = new Bitmap(Properties.Resources.UI_CustomMissionPreviewDefault))//, Globals.WorkshopPreviewSize))
                {
                    bm.SetResolution(96, 96);
                    bm.Save(soloBannerPath, ImageFormat.Png);
                }
                defaultPreview = soloBannerPath;
            }
            return defaultPreview;
        }

        private void HandleGeneratedPreview(string defaultPreview)
        {
            if (defaultPreview == null)
            {
                MessageBox.Show("There was an error generating the default previews!");
                return;
            }
            lblStatus.Text = "Ready.";
            this.defaultPreview = defaultPreview;
            if (string.IsNullOrEmpty(txtPreview.Text) || !File.Exists(txtPreview.Text))
            {
                txtPreview.Text = defaultPreview;
            }
            if (txtPreview.Tag == null)
            {
                refreshPreviewImage();
            }
        }

        private void SteamDialog_FormClosing(Object sender, FormClosingEventArgs e)
        {
            if (multiThreader.IsExecuting)
            {
                multiThreader.AbortThreadedOperation(5000);
            }
            DirectoryInfo previewDir = new DirectoryInfo(PreviewDirectory);
            if (previewDir.Exists)
            {
                foreach (var file in previewDir.EnumerateFiles())
                {
                    try { file.Delete(); }
                    catch { /* ignore */}
                }
                try { previewDir.Delete(); }
                catch { /* ignore */}
            }           
            DirectoryInfo publishDir = new DirectoryInfo(PublishTempDirectory);
            if (publishDir.Exists)
            {
                foreach (var file in publishDir.EnumerateFiles())
                {
                    try { file.Delete(); }
                    catch { /* ignore */}
                }
                try { publishDir.Delete(); }
                catch { /* ignore */}
            }
        }

        private void btnClose_Click(Object sender, EventArgs e)
        {
            if (!multiThreader.IsExecuting && !isPublishing)
            {
                return;
            }
            String operation = lblStatus.Text;
            DialogResult dr = MessageBox.Show("The following operation is currently in progress:\n\n" + operation + "\n\nAre you sure you want to abort?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.No)
            {
                this.DialogResult = DialogResult.None;
            }
        }

        private void TxtPreview_MouseEnter(Object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtPreview.Text) && txtPreview.Tag != null)
            {
                Point resPoint = txtPreview.PointToScreen(new Point(0, txtPreview.Height));
                ShowToolTip(txtPreview, resPoint, Path.GetFileName(txtPreview.Text));
            }
        }

        private void TxtPreview_MouseLeave(Object sender, EventArgs e)
        {
            this.imageTooltip.Hide(txtPreview);
        }

        private void ShowToolTip(Control target, Point resPoint, string message)
        {
            if (target == null || message == null)
            {
                this.imageTooltip.Hide(target);
                return;
            }
            imageTooltip.SetToolExt(target, message, ImageTooltip.TipInfoType.Absolute, resPoint);
        }
    }
}
