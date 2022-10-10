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
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class SteamDialog : Form
    {
        private static readonly string PreviewDirectory = Path.Combine(Path.GetTempPath(), "CnCRCMapEditor");
        private string defaultPreview;

        private readonly IGamePlugin plugin;
        private readonly Timer statusUpdateTimer = new Timer();
        private SimpleMultiThreading multiThreader;

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
            txtDescription.Text = (plugin.Map.SteamSection.Description ?? String.Empty).Replace("@@", Environment.NewLine);
            txtPreview.Text = plugin.Map.SteamSection.PreviewFile ?? String.Empty;
            cmbVisibility.SelectedValue = plugin.Map.SteamSection.Visibility;
            btnPublishMap.SplitWidth = (plugin.Map.SteamSection.PublishedFileId != PublishedFileId_t.Invalid.m_PublishedFileId) ? MenuButton.DefaultSplitWidth : 0;
            btnFromBriefing.Enabled = plugin.Map.BasicSection.SoloMission;
            statusUpdateTimer.Start();
            UpdateControls();
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
            lblStatus.Text = "Done.";
            mapWasPublished = true;
            EnableControls(true);
        }

        protected virtual void OnOperationFailed(string status)
        {
            lblStatus.Text = status;
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
            btnClose.Enabled = enable;
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
            plugin.Map.SteamSection.Description = txtDescription.Text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "@@");
            plugin.Map.SteamSection.Visibility = (ERemoteStoragePublishedFileVisibility)cmbVisibility.SelectedValue;
            var tempPath = Path.Combine(Path.GetTempPath(), "CnCRCMapEditorPublishUGC");
            Directory.CreateDirectory(tempPath);
            foreach (var file in new DirectoryInfo(tempPath).EnumerateFiles()) file.Delete();
            var pgmPath = Path.Combine(tempPath, "MAPDATA.PGM");
            multiThreader.ExecuteThreaded(() => SavePgm(tempPath, pgmPath), SaveDone, true, EnableControls, "Saving map");
        }

        private string SavePgm(string tempPath, string pgmPath)
        {
            try
            {
                plugin.Save(pgmPath, FileType.PGM, txtPreview.Tag as Bitmap);
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
            SteamSection steamCloneSection = new SteamSection();
            INI.ParseSection(new MapContext(plugin.Map, true), steamSection, steamCloneSection);
            // Restore original description from control
            steamCloneSection.Description = txtDescription.Text;
            if (SteamworksUGC.PublishUGC(sendPath, steamCloneSection, tags, OnPublishSuccess, OnOperationFailed))
            {
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
            btnPublishMap.PerformClick();
        }

        private void previewTxt_TextChanged(object sender, EventArgs e)
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
            UpdateControls();
        }

        private void descriptionTxt_TextChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void UpdateControls()
        {
            btnPublishMap.Enabled = (txtPreview.Tag != null) && !string.IsNullOrEmpty(txtDescription.Text);
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
            multiThreader.ExecuteThreaded(() => GeneratePreviews(plugin), HandleGeneratedPreview, true, EnableControls, "Generating previews");
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
            imageTooltip.SetToolTip(txtPreview, "Preview.png");
        }

    }
}
