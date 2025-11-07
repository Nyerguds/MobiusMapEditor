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
using MobiusEditor.Controls;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class SteamDialog : Form
    {
        private static readonly string visibilityWarningToolTip =
            "{0} maps make use of a mirror server,\n" +
            "to make the items accessible to people who do not own the game on\n" +
            "the Steam platform. This does mean that the maps only show up in\n" +
            "the game's maps list if the visibility is set to \"Public\", because\n" +
            "otherwise, the mirror server can't detect and download them.";
        private static readonly string PreviewDirectory = Path.Combine(Path.GetTempPath(), "MobiusMapEditor");
        private static readonly string PublishTempDirectory = Path.Combine(Path.GetTempPath(), "MobiusMapEditorPublishUGC");
        private string defaultPreview;

        private Bitmap visibilityWarningImage;
        private Control tooltipShownOn;

        private readonly IGamePlugin plugin;
        private readonly GameInfo gameInfo;
        private bool extraTagsAvailable;
        private SteamSection steamCloneSection = null;
        private readonly Timer statusUpdateTimer = new Timer();
        private SimpleMultiThreading multiThreader;
        private Dictionary<ToolStripMenuItem, String> previewPaths = new Dictionary<ToolStripMenuItem, string>();
        private string previewPath;
        private Bitmap previewImage;
        private bool isPublishing = false;
        private bool mapWasPublished = false;
        public bool MapWasPublished => mapWasPublished;
        private readonly string currentAppId;

        public SteamDialog(IGamePlugin plugin)
        {
            this.plugin = plugin;
            InitializeComponent();
            InitializeWarningIcon();
            // Actual connected ID
            currentAppId = SteamUtils.GetAppID().m_AppId.ToString();
            gameInfo = plugin.GameInfo;
            if (gameInfo == null)
            {
                throw new ArgumentException("Given game plugin does not support Steam", "plugin");
            }
            if (gameInfo.SteamId != currentAppId)
            {
                throw new ArgumentException("Given game plugin does not match current connected Steam game id", "plugin");
            }
            // Bare gameInfo from plugin is always the remaster one.
            cmbVisibility.ValueMember = "Value";
            cmbVisibility.DisplayMember = "Label";
            cmbVisibility.DataSource = new ListItem<ERemoteStoragePublishedFileVisibility>[]
            {
                ListItem.Create(ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic, "Public"),
                ListItem.Create(ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityFriendsOnly, "Friends Only"),
                ListItem.Create(ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate, "Private")
            };
            multiThreader = new SimpleMultiThreading(this);
            statusUpdateTimer.Interval = 500;
            statusUpdateTimer.Tick += StatusUpdateTimer_Tick;
            Disposed += (o, e) => { previewImage?.Dispose(); };
        }

        private void InitializeWarningIcon()
        {
            visibilityWarningImage = new Bitmap(lblvisibilityWarningImage.Width, lblvisibilityWarningImage.Height);
            visibilityWarningImage.SetResolution(96, 96);
            using (Graphics g = Graphics.FromImage(visibilityWarningImage))
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawIcon(SystemIcons.Warning, new Rectangle(0, 0, visibilityWarningImage.Width, visibilityWarningImage.Height));
            }
            lblvisibilityWarningImage.Image = visibilityWarningImage;
            lblvisibilityWarningImage.ImageAlign = ContentAlignment.MiddleCenter;
            lblvisibilityWarningImage.Text = String.Empty;
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
            previewPath = plugin.Map.SteamSection.PreviewFile ?? String.Empty;
            if (!String.IsNullOrEmpty(previewPath) && !Path.IsPathRooted(previewPath))
            {
                previewPath = Path.Combine(PreviewDirectory, previewPath);
            }
            RefreshPreviewImage();
            cmbVisibility.SelectedIndex = ListItem.GetIndexInComboBox(plugin.Map.SteamSection.VisibilityAsEnum, cmbVisibility, 0);
            bool isSolo = plugin.Map.BasicSection.SoloMission;

            HashSet<string> setTags = (plugin.Map.SteamSection.Tags ?? String.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries).Select(tag => tag.Trim())
                .Where(tag => tag.Length > 0).ToHashSet(StringComparer.OrdinalIgnoreCase);
            string[] extraTags = (isSolo ? gameInfo.SteamSoloExtraTags : gameInfo.SteamMultiExtraTags) ?? new string[0];
            lbExtraTags.Items.Clear();
            foreach (string tag in extraTags)
            {
                int index = lbExtraTags.Items.Add(tag);
                if (setTags.Contains(tag))
                {
                    lbExtraTags.SetSelected(index, true);
                }
            }
            extraTagsAvailable = extraTags.Length > 0;
            if (!extraTagsAvailable)
            {
                lbExtraTags.Items.Add("No additional tags available for " + (isSolo ? "single player missions" : "multiplayer maps") + ".");
                lbExtraTags.Enabled = false;
            }
            UpdatePublishButton();
            btnFromBriefing.Enabled = plugin.Map.BasicSection.SoloMission;
            FixSizeAndLocation(800, 600);
        }

        private void FixSizeAndLocation(int width, int height)
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
            if (!String.IsNullOrEmpty(status))
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

        protected virtual void OnPublishSuccess(SteamSection steamCloneSection)
        {
            lblStatus.Text = "Map published.";
            isPublishing = false;
            if (steamCloneSection != null)
            {
                plugin.Map.SteamSection.Description = GeneralUtils.ReplaceLinebreaks(steamCloneSection.Description, '@');
                plugin.Map.SteamSection.PublishedFileId = steamCloneSection.PublishedFileId;
            }
            mapWasPublished = true;
            UpdatePublishButton();
            EnableControls(true);
        }

        protected virtual void OnOperationFailed(string status, bool needsWorkshopAccept)
        {
            lblStatus.Text = status;
            isPublishing = false;
            if (needsWorkshopAccept)
            {
                SteamUGC.ShowWorkshopEULA();
            }
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
            btnPreview.Enabled = enable;
            btnDefaultPreview.Enabled = enable;
            lblDescription.Enabled = enable;
            btnFromBriefing.Enabled = enable && plugin != null && plugin.Map.BasicSection.SoloMission;
            txtDescription.Enabled = enable;
            lbExtraTags.Enabled = enable && extraTagsAvailable;
            btnPublishMap.Enabled = enable;
            btnGoToSteam.Enabled = enable;
            btnClose.Enabled = !isPublishing || enable;
            lblLegal.Enabled = enable;
        }

        private void btnGoToSteam_Click(object sender, EventArgs e)
        {
            string workshopUrl;
            ulong publishId = plugin.Map.SteamSection.PublishedFileId;
            if (publishId == 0)
            {
                workshopUrl = SteamworksUGC.WorkshopURL;
            }
            else
            {
                workshopUrl = SteamworksUGC.GetWorkshopItemURL(publishId);
            }
            if (!String.IsNullOrEmpty(workshopUrl))
            {
                Process.Start(workshopUrl);
            }
        }

        private void btnPublishMap_Click(object sender, EventArgs e)
        {
            if ((txtTitle.Text ?? String.Empty).Trim().Length < 8)
            {
                MessageBox.Show(this, "Steam title needs to be at least eight characters.", "Error");
                return;
            }
            if ((txtDescription.Text ?? String.Empty).Trim().Length < 8)
            {
                MessageBox.Show(this, "Steam description needs to be at least eight characters.", "Error");
                return;
            }
            if (previewImage == null)
            {
                MessageBox.Show(this, "Preview image is required.", "Error");
                return;
            }
            if (!File.Exists(previewPath))
            {
                MessageBox.Show(this, "Preview image file is missing.", "Error");
                return;
            }
            if (String.IsNullOrEmpty(plugin.Map.BasicSection.Name))
            {
                plugin.Map.BasicSection.Name = txtTitle.Text;
            }
            if (String.IsNullOrEmpty(plugin.Map.BasicSection.Author))
            {
                plugin.Map.BasicSection.Author = SteamFriends.GetPersonaName();
            }
            string previewFolder = Path.GetFullPath(PreviewDirectory);
            string actualPreviewFolder = Path.GetFullPath(Path.GetDirectoryName(previewPath));
            string prevPath = previewPath;
            if (previewFolder == actualPreviewFolder)
            {
                prevPath = Path.GetFileName(previewPath);
            }
            plugin.Map.SteamSection.PreviewFile = prevPath;
            plugin.Map.SteamSection.Title = txtTitle.Text;
            plugin.Map.SteamSection.Author = SteamFriends.GetPersonaName();
            List<string> extraTags = new List<string>();
            if (extraTagsAvailable)
            {
                foreach (string tag in lbExtraTags.SelectedItems)
                {
                    extraTags.Add(tag);
                }
            }
            plugin.Map.SteamSection.Tags = String.Join(",", extraTags);
            plugin.Map.SteamSection.Description = txtDescription.Text;
            plugin.Map.SteamSection.VisibilityAsEnum = ListItem.GetValueFromComboBox<ERemoteStoragePublishedFileVisibility>(cmbVisibility);
            if (Directory.Exists(PublishTempDirectory))
            {
                // Full recursive delete; takes care of absolutely everything in there.
                try { Directory.Delete(PublishTempDirectory, true); }
                catch { /* ignore I guess? */ }
            }
            Directory.CreateDirectory(PublishTempDirectory);
            string fileName = gameInfo.GetSteamWorkshopFileName(plugin);
            string extension = plugin.Map.BasicSection.SoloMission ? gameInfo.SteamFileExtensionSolo : gameInfo.SteamFileExtensionMulti;
            var mapPath = Path.Combine(PublishTempDirectory, fileName + extension);
            statusUpdateTimer.Start();
            multiThreader.ExecuteThreaded(() => SaveMap(PublishTempDirectory, mapPath), (err) => SaveDone(PublishTempDirectory, err), true, EnableControls, "Saving map");
        }

        private string SaveMap(string tempPath, string savePath)
        {
            try
            {
                string errors = plugin.Validate(gameInfo.SteamFileType, true, false);
                if (!String.IsNullOrWhiteSpace(errors))
                {
                    return errors.Split('\n')[0];
                }
                long dataSize = plugin.Save(savePath, gameInfo.SteamFileType, previewImage, false, true);
                if (dataSize == 0 || dataSize > plugin.GameInfo.MaxDataSize)
                {
                    try
                    {
                        if (File.Exists(savePath))
                        {
                            File.Delete(savePath);
                        }
                    }
                    catch { /* ignore */ }
                    return dataSize == 0 ? String.Empty : "Map file size too large.";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return null;
        }

        private void SaveDone(string sendPath, string errorMessage)
        {
            statusUpdateTimer.Stop();
            if (sendPath == null || errorMessage != null)
            {
                lblStatus.Text = "Save failed" + (String.IsNullOrEmpty(errorMessage) ? "." : (": " + errorMessage));
                return;
            }
            List<string> tags = new List<string>();
            tags.AddRange(gameInfo.SteamDefaultTags);
            tags.AddRange(plugin.Map.BasicSection.SoloMission ? gameInfo.SteamSoloTags : gameInfo.SteamMultiTags);
            if (extraTagsAvailable)
            {
                foreach (string tag in lbExtraTags.SelectedItems)
                {
                    tags.Add(tag);
                }
            }
            // Clone to have version without line breaks to give to the Steam publish.
            steamCloneSection = new SteamSection();
            plugin.Map.SteamSection.CopyTo(steamCloneSection, typeof(NonSerializedINIKeyAttribute));
            // Restore original description from control, with actual line breaks instead of '@' replacements.
            steamCloneSection.Description = txtDescription.Text;
            if (!Path.IsPathRooted(steamCloneSection.PreviewFile)) {
                steamCloneSection.PreviewFile = Path.Combine(PreviewDirectory, steamCloneSection.PreviewFile);
            }
            // Final check to see if all data is present
            if (!File.Exists(steamCloneSection.PreviewFile))
            {
                OnOperationFailed("Preview missing", false);
                return;
            }
            if (!Directory.Exists(sendPath) || !Directory.EnumerateFileSystemEntries(sendPath).Any())
            {
                OnOperationFailed("Content not found", false);
                return;
            }
            // We need to pass on the clone section to the succes function since it gets the final steam ID filled in by the publish operation.
            if (SteamworksUGC.PublishUGC(sendPath, steamCloneSection, tags, () => OnPublishSuccess(steamCloneSection), OnOperationFailed))
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
            try
            {
                if (!String.IsNullOrEmpty(previewPath) && File.Exists(previewPath))
                {
                    ofd.InitialDirectory = Path.GetDirectoryName(previewPath);
                    ofd.FileName = Path.GetFileName(previewPath);
                }
                else
                {
                    ofd.InitialDirectory = PreviewDirectory;
                    ofd.FileName = null;
                }
            }
            catch
            {
                if (!Directory.Exists(PreviewDirectory))
                {
                    Directory.CreateDirectory(PreviewDirectory);
                }
                ofd.InitialDirectory = PreviewDirectory;
                ofd.FileName = null;
            }
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                previewPath = ofd.FileName;
                RefreshPreviewImage();
            }
        }

        private void publishAsNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            plugin.Map.SteamSection.PublishedFileId = PublishedFileId_t.Invalid.m_PublishedFileId;
            UpdatePublishButton();
            btnPublishMap_Click(btnPublishMap, new EventArgs());
        }

        private void RefreshPreviewImage()
        {
            CheckSelectedPreview();
            CleanupPreview();
            try
            {
                Bitmap preview = null;
                if (File.Exists(previewPath))
                {
                    using (Bitmap b = new Bitmap(previewPath))
                    {
                        b.SetResolution(96, 96);
                        preview = b.FitToBoundingBox(Globals.MapPreviewSize.Width, Globals.MapPreviewSize.Height, Color.Black);
                    }
                }
                previewImage = preview;
                pnlImage.BackgroundImage = previewImage;
                pnlImage.BackgroundImageLayout = ImageLayout.Zoom;
            }
            catch (Exception)
            {
                previewImage = null;
                pnlImage.BackgroundImage = null;
            }
        }

        private void CheckSelectedPreview()
        {
            bool isChecked = false;
            string linkedPath;
            foreach (ToolStripMenuItem tsm in previewContextMenuStrip.Items)
            {
                if (!isChecked && previewPaths.TryGetValue(tsm, out linkedPath) && linkedPath.Equals(previewPath, StringComparison.OrdinalIgnoreCase))
                {
                    tsm.Checked = true;
                    isChecked = true;
                }
                else
                {
                    tsm.Checked = false;
                }
            }
        }

        private void CleanupPreview()
        {
            pnlImage.BackgroundImage = null;
            if (previewImage != null)
            {
                try { previewImage.Dispose(); }
                catch { /*ignore*/}
                previewImage = null;
            }
        }

        private void ShowToolTip(Control target, string message)
        {
            if (target == null || message == null || !target.Enabled)
            {
                this.HideToolTip(target, null);
                return;
            }
            Point resPoint = target.PointToScreen(new Point(0, target.Height));
            MethodInfo m = toolTip1.GetType().GetMethod("SetTool",
                       BindingFlags.Instance | BindingFlags.NonPublic);
            m.Invoke(toolTip1, new object[] { target, message, 2, resPoint });
            this.tooltipShownOn = target;
        }

        public void HideToolTip(object sender, EventArgs e)
        {
            try
            {
                if (this.tooltipShownOn != null)
                {
                    this.toolTip1.Hide(this.tooltipShownOn);
                }
                if (sender is Control target)
                {
                    this.toolTip1.Hide(target);
                }
            }
            catch { /* ignore */ }
            tooltipShownOn = null;
        }

        private void BtnCopyFromMap_Click(object sender, EventArgs e)
        {
            txtTitle.Text = lblMapTitleData.Text;
        }

        private void BtnFromBriefing_Click(object sender, EventArgs e)
        {
            txtDescription.Text = plugin.Map.BriefingSection.Briefing;
        }

        private void btnDefaultPreview_Click(object sender, EventArgs e)
        {
            previewPath = defaultPreview;
            if (!File.Exists(previewPath))
            {
                GeneratePreviewsThreaded();
            }
            else
            {
                RefreshPreviewImage();
            }
        }

        private void SteamDialog_Shown(object sender, EventArgs e)
        {
            GeneratePreviewsThreaded();
        }

        private void GeneratePreviewsThreaded()
        {
            if (!multiThreader.IsExecuting)
            {
                multiThreader.ExecuteThreaded(() => GeneratePreviews(plugin), HandleGeneratedPreview, true, EnableControls, "Generating map previews");
            }
        }

        private (string, ToolStripItem[]) GeneratePreviews(IGamePlugin plugin)
        {
            List<ToolStripItem> newTsmis = new List<ToolStripItem>();
            if (Directory.Exists(PreviewDirectory))
            {
                // Full recursive delete; takes care of absolutely everything in there.
                try { Directory.Delete(PreviewDirectory, true); }
                catch { /* ignore I guess? */ }
            }
            Directory.CreateDirectory(PreviewDirectory);
            previewPaths.Clear();
            string defaultPreview;
            using (Bitmap genericRem = gameInfo.WorkshopPreviewGeneric)
            {
                if (genericRem != null)
                {
                    string genericBannerPath = Path.Combine(PreviewDirectory, "GenericBanner.png");
                    using (Bitmap bm = new Bitmap(genericRem))
                    {
                        bm.SetResolution(96, 96);
                        bm.Save(genericBannerPath, ImageFormat.Png);
                    }
                    ToolStripMenuItem tsGeneric = new ToolStripMenuItem();
                    tsGeneric.Text = "Generic banner";
                    tsGeneric.Click += TsPreview_Click;
                    previewPaths.Add(tsGeneric, genericBannerPath);
                    newTsmis.Add(tsGeneric);
                }
            }
            using (Bitmap generic = gameInfo.WorkshopPreviewGenericGame)
            {
                if (generic != null)
                {
                    string genericBannerGamePath = Path.Combine(PreviewDirectory, "GenericBanner_" + gameInfo.Name.Replace(" ", "_") + ".png");
                    using (Bitmap bm = new Bitmap(generic))
                    {
                        bm.SetResolution(96, 96);
                        bm.Save(genericBannerGamePath, ImageFormat.Png);
                    }
                    ToolStripMenuItem tsGenGame = new ToolStripMenuItem();
                    tsGenGame.Text = "Generic banner (" + gameInfo.Name + ")";
                    tsGenGame.Click += TsPreview_Click;
                    previewPaths.Add(tsGenGame, genericBannerGamePath);
                    newTsmis.Add(tsGenGame);
                }
            }
            string minimap = Path.Combine(PreviewDirectory, "Minimap.png");
            using (Bitmap pr = plugin.Map.GenerateWorkshopPreview(plugin, plugin.Map.Bounds).ToBitmap())
            {
                SaveMapImage(pr, minimap);
            }
            ToolStripMenuItem tsMinimap = new ToolStripMenuItem();
            tsMinimap.Text = "Full map";
            tsMinimap.Click += TsPreview_Click;
            previewPaths.Add(tsMinimap, minimap);
            newTsmis.Add(tsMinimap);
            defaultPreview = minimap;
            if (plugin.Map.BasicSection.SoloMission)
            {
                string viewPortPath = Path.Combine(PreviewDirectory, "MissionStart.png");
                Size vpsLarge = gameInfo.ViewportSizeLarge;
                Point offsLarge = gameInfo.ViewportOffsetLarge;
                int offsLargeX = Math.Abs(offsLarge.X % Globals.PixelWidth);
                // Cut off half cells at the full-cell boundaries; this will produce a square anyway.
                if (offsLargeX != 0)
                {
                    offsLarge.X +=offsLargeX;
                    vpsLarge.Width -= Globals.PixelWidth;
                }
                Rectangle missionStartLg = plugin.Map.GetSoloViewport(vpsLarge, offsLarge, true, true, Globals.PixelSize);
                missionStartLg = missionStartLg.AdjustToScale(null, Globals.PixelSize);
                using (Bitmap pr = plugin.Map.GenerateWorkshopPreview(plugin, missionStartLg).ToBitmap())
                {
                    SaveMapImage(pr, viewPortPath);
                }
                ToolStripMenuItem tsMissionStart = new ToolStripMenuItem();
                tsMissionStart.Text = "Mission start view (large)";
                tsMissionStart.Click += TsPreview_Click;
                previewPaths.Add(tsMissionStart, viewPortPath);
                newTsmis.Add(tsMissionStart);

                string viewPortPathDos = Path.Combine(PreviewDirectory, "MissionStartSmall.png");
                Size vpsSmall = gameInfo.ViewportSizeSmall;
                Point offsSmall = gameInfo.ViewportOffsetSmall;
                int offsSmallX = Math.Abs(offsSmall.X % Globals.PixelWidth);
                // Cut off half cells at the full-cell boundaries; this will produce a square anyway.
                if (offsSmallX != 0)
                {
                    offsSmall.X += offsSmallX;
                    vpsSmall.Width -= Globals.PixelWidth;
                }
                Rectangle missionStartSm = plugin.Map.GetSoloViewport(vpsSmall, offsSmall, true, true, Globals.PixelSize);
                missionStartSm = missionStartSm.AdjustToScale(null, Globals.PixelSize);
                using (Bitmap pr = plugin.Map.GenerateWorkshopPreview(plugin, missionStartSm).ToBitmap())
                using (MemoryStream ms = new MemoryStream())
                {
                    SaveMapImage(pr, viewPortPathDos);
                }
                ToolStripMenuItem tsMissionStartDos = new ToolStripMenuItem();
                tsMissionStartDos.Text = "Mission start view (small)";
                tsMissionStartDos.Click += TsPreview_Click;
                previewPaths.Add(tsMissionStartDos, viewPortPathDos);
                newTsmis.Add(tsMissionStartDos);

                // Override this for single play missions by using the close-up start situation as default.
                defaultPreview = viewPortPathDos;
            }
            return (defaultPreview, newTsmis.ToArray());
        }

        private void TsPreview_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            string path;
            if (tsmi == null || !previewPaths.TryGetValue(tsmi, out path))
            {
                return;
            }
            if (!File.Exists(path))
            {
                previewPath = path;
                GeneratePreviewsThreaded();
                return;
            }
            previewPath = path;
            RefreshPreviewImage();
        }

        private void SaveMapImage(Bitmap bitmap, string path)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.SetResolution(96, 96);
                bitmap.Save(ms, ImageFormat.Png);
                byte[] pngData = ms.ToArray();
                pngData = ImageUtils.SetPngTextChunk(pngData, "Comment", "Created with " + Program.ProgramVersionTitle);
                pngData = ImageUtils.SetPngTextChunk(pngData, "Software", Program.ProgramVersionTitle);
                File.WriteAllBytes(path, pngData);
            }
        }

        private void HandleGeneratedPreview((string Preview, ToolStripItem[] Tsmis) result)
        {
            string preview = result.Preview;
            if (preview == null)
            {
                MessageBox.Show(this, "There was an error generating the default previews!", "Error");
                return;
            }
            lblStatus.Text = "Ready.";
            this.defaultPreview = preview;
            if (String.IsNullOrEmpty(previewPath) || !File.Exists(previewPath))
            {
                previewPath = preview;
            }
            List<ToolStripItem> oldTsmis = new List<ToolStripItem>();
            foreach (ToolStripItem tsmi in previewContextMenuStrip.Items)
            {
                oldTsmis.Add(tsmi);
            }
            previewContextMenuStrip.Items.Clear();
            foreach (ToolStripItem tsmi in oldTsmis)
            {
                try { tsmi.Dispose(); }
                catch { /* ignore */ }
            }
            ToolStripItem[] tsmis = result.Tsmis;
            if (tsmis != null) {
                previewContextMenuStrip.Items.AddRange(tsmis);
            }
            RefreshPreviewImage();
        }

        private void SteamDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (multiThreader.IsExecuting)
            {
                multiThreader.AbortThreadedOperation(5000);
            }
            string previewDir = PreviewDirectory;
            if (Directory.Exists(previewDir))
            {
                // Full recursive delete; takes care of absolutely everything in there.
                try { Directory.Delete(previewDir, true); }
                catch { /* ignore I guess? */ }

            }
            string publishDir = PublishTempDirectory;
            if (Directory.Exists(publishDir))
            {
                // Full recursive delete; takes care of absolutely everything in there.
                try { Directory.Delete(publishDir, true); }
                catch { /* ignore I guess? */ }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (!multiThreader.IsExecuting && !isPublishing)
            {
                return;
            }
            string operation = lblStatus.Text;
            DialogResult dr = MessageBox.Show(this, "The following operation is currently in progress:\n\n" + operation + "\n\nAre you sure you want to abort?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.No)
            {
                this.DialogResult = DialogResult.None;
            }
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://steamcommunity.com/workshop/workshoplegalagreement/");
        }

        private void CmbVisibility_SelectedIndexChanged(object sender, EventArgs e)
        {
            ERemoteStoragePublishedFileVisibility visibility = ListItem.GetValueFromComboBox<ERemoteStoragePublishedFileVisibility>(cmbVisibility);
            bool visible = gameInfo.PublishedMapsUseMirrorServer &&
                visibility != ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic;
            lblvisibilityWarning.Visible = visible;
            lblvisibilityWarningImage.Visible = visible;
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
            }
            lblvisibilityWarningImage.Image = null;
            if (visibilityWarningImage != null)
            {
                try { visibilityWarningImage.Dispose(); }
                catch { /*ignore*/}
                visibilityWarningImage = null;
            }
            CleanupPreview();
            base.Dispose(disposing);
        }

        private void WarningToolTipItem_MouseEnter(object sender, EventArgs e)
        {
            if (lblvisibilityWarningImage.Visible)
            {
                ShowToolTip(cmbVisibility, String.Format(visibilityWarningToolTip, gameInfo.SteamGameName));
            }
        }

        private void WarningToolTipItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (tooltipShownOn != cmbVisibility)
            {
                WarningToolTipItem_MouseEnter(sender, e);
            }
        }
    }
}
