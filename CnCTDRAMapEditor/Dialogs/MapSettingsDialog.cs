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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class MapSettingsDialog : Form
    {
        private string originalExtraIniText;
        public string ExtraIniText { get; set; }

        private const int TVIF_STATE = 0x8;
        private const int TVIS_STATEIMAGEMASK = 0xF000;
        private const int TV_FIRST = 0x1100;
        private const int TVM_SETITEM = TV_FIRST + 63;

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private struct TVITEM
        {
            public int mask;
            public IntPtr hItem;
            public int state;
            public int stateMask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public String lpszText;
            public int cchTextMax;
            public int iImage;
            public int iSelectedImage;
            public int cChildren;
            public IntPtr lParam;
        }

        private readonly IGamePlugin plugin;
        private readonly PropertyTracker<BasicSection> basicSettingsTracker;
        private readonly PropertyTracker<BriefingSection> briefingSettingsTracker;
        private readonly IDictionary<House, PropertyTracker<House>> houseSettingsTrackers;
        private TreeNode playersNode;
        private bool expansionWasEnabled;
        private string currentSelection;


        public MapSettingsDialog(IGamePlugin plugin, PropertyTracker<BasicSection> basicSettingsTracker, PropertyTracker<BriefingSection> briefingSettingsTracker,
            IDictionary<House, PropertyTracker<House>> houseSettingsTrackers, string extraIniText)
        {
            InitializeComponent();
            this.plugin = plugin;
            this.originalExtraIniText = extraIniText ?? String.Empty;
            this.ExtraIniText = extraIniText ?? String.Empty;
            expansionWasEnabled = plugin.Map.BasicSection.ExpansionEnabled;
            this.basicSettingsTracker = basicSettingsTracker;
            this.basicSettingsTracker.PropertyChanged += this.BasicSettingsTracker_PropertyChanged;
            this.briefingSettingsTracker = briefingSettingsTracker;
            this.houseSettingsTrackers = houseSettingsTrackers;

            ResetSettingsTree(this.plugin.Map.BasicSection.SoloMission);
        }

        private void BasicSettingsTracker_PropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SoloMission")
            {
                if (basicSettingsTracker.TryGetMember("SoloMission", out object result) && (result is bool solo))
                {
                    ResetSettingsTree(solo);
                }
            }
        }

        private void ResetSettingsTree(bool isSoloMission)
        {
            this.currentSelection = settingsTreeView.SelectedNode?.Name;
            settingsTreeView.BeginUpdate();
            settingsTreeView.Nodes.Clear();
            settingsTreeView.Nodes.Add("BASIC", "Basic");
            if (this.plugin.GameType == GameType.RedAlert)
            {
                if (isSoloMission)
                {
                    settingsTreeView.Nodes.Add("SCENARIO", "Scenario");
                }
                settingsTreeView.Nodes.Add("RULES", "Rules");
            }
            settingsTreeView.Nodes.Add("BRIEFING", "Briefing");
            playersNode = settingsTreeView.Nodes.Add("Players");
            foreach (var player in this.plugin.Map.Houses)
            {
                var playerNode = playersNode.Nodes.Add(player.Type.Name, player.Type.Name);
                playerNode.Checked = player.Enabled;
            }
            playersNode.Expand();
            settingsTreeView.EndUpdate();
            settingsTreeView.SelectedNode = settingsTreeView.Nodes[0];
        }

        private void settingsTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (this.currentSelection != null && this.currentSelection == settingsTreeView.SelectedNode?.Name)
            {
                return;
            }
            List<Control> oldControls = new List<Control>();
            foreach (Control ctrl in settingsPanel.Controls)
            {
                oldControls.Add(ctrl);
            }
            settingsPanel.Controls.Clear();
            // Cleanup.
            foreach (Control ctrl in settingsPanel.Controls)
            {
                if (ctrl is RulesSettings ruleSet)
                {
                    ruleSet.TextNeedsUpdating -= this.RulesPanel_TextNeedsUpdating;
                }
                ctrl.Dispose();
            }
            this.currentSelection = settingsTreeView.SelectedNode?.Name;
            switch (settingsTreeView.SelectedNode.Name)
            {
                case "BASIC":
                    {
                        BasicSettings basicPanel = new BasicSettings(plugin, basicSettingsTracker);
                        settingsPanel.Controls.Add(basicPanel);
                        basicPanel.Dock = DockStyle.Fill;
                    }
                    break;
                case "SCENARIO":
                    {
                        ScenarioSettings scenPanel = new ScenarioSettings(basicSettingsTracker);
                        settingsPanel.Controls.Add(scenPanel);
                        scenPanel.Dock = DockStyle.Fill;
                    }
                    break;
                case "RULES":
                    {
                        // TODO clone briefing panel, add extra logic.
                        RulesSettings rulesPanel = new RulesSettings(ExtraIniText);
                        rulesPanel.TextNeedsUpdating += this.RulesPanel_TextNeedsUpdating;
                        settingsPanel.Controls.Add(rulesPanel);
                        rulesPanel.Dock = DockStyle.Fill;
                    }
                    break;
                case "BRIEFING":
                    {
                        BriefingSettings briefPanel = new BriefingSettings(plugin, briefingSettingsTracker);
                        settingsPanel.Controls.Add(briefPanel);
                        briefPanel.Dock = DockStyle.Fill;
                    }
                    break;
                default:
                    {
                        var player = plugin.Map.Houses.Where(h => h.Type.Name == settingsTreeView.SelectedNode.Name).FirstOrDefault();
                        if (player != null)
                        {
                            PlayerSettings playerPanel = new PlayerSettings(plugin, houseSettingsTrackers[player]);
                            settingsPanel.Controls.Add(playerPanel);
                            playerPanel.Dock = DockStyle.Fill;
                        }
                    }
                    break;
            }
        }

        private void RulesPanel_TextNeedsUpdating(Object sender, EventArgs e)
        {
            if (sender is TextBox tb)
            {
                this.ExtraIniText = tb.Text;
            }
        }

        private void settingsTreeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (!playersNode.Nodes.Contains(e.Node))
            {
                HideCheckBox(e.Node);
                e.DrawDefault = true;
            }
            else
            {
                using (var brush = new SolidBrush(settingsTreeView.ForeColor))
                {
                    e.Graphics.DrawString(e.Node.Text, e.Node.TreeView.Font, brush, e.Node.Bounds.X, e.Node.Bounds.Y);
                }
            }
        }

        private void HideCheckBox(TreeNode node)
        {
            TVITEM tvi = new TVITEM
            {
                hItem = node.Handle,
                mask = TVIF_STATE,
                stateMask = TVIS_STATEIMAGEMASK,
                state = 0,
                lpszText = null,
                cchTextMax = 0,
                iImage = 0,
                iSelectedImage = 0,
                cChildren = 0,
                lParam = IntPtr.Zero
            };
            IntPtr lparam = Marshal.AllocHGlobal(Marshal.SizeOf(tvi));
            Marshal.StructureToPtr(tvi, lparam, false);
            SendMessage(node.TreeView.Handle, TVM_SETITEM, IntPtr.Zero, lparam);
        }

        private void settingsTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            var player = plugin.Map.Houses.Where(h => h.Type.Name == e.Node.Name).FirstOrDefault();
            if (player != null)
            {
                ((dynamic)houseSettingsTrackers[player]).Enabled = e.Node.Checked;
            }
        }

        private void MapSettingsDialog_FormClosing(Object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK || plugin == null)
            {
                return;
            }
            // Check if RA rules were changed.
            bool rulesChanged = plugin.GameType == GameType.RedAlert && !(this.ExtraIniText ?? String.Empty).Equals(originalExtraIniText);
            // Check if RA expansion units were disabled.
            bool expansionWarn = plugin.GameType == GameType.RedAlert && expansionWasEnabled
                                    && basicSettingsTracker.TryGetMember("ExpansionEnabled", out object res) && (res is bool expOn) && !expOn;
            if (expansionWarn)
            {
                // Check if a warning is actually necessary.
                expansionWarn = plugin.Map.GetAllTechnos().Any(t => (t is Unit un && un.Type.IsExpansionUnit) || (t is Infantry it && it.Type.IsExpansionUnit))
                    || plugin.Map.TeamTypes.Any(tt => tt.Classes.Any(cl => (cl.Type is UnitType ut && ut.IsExpansionUnit) || (cl.Type is InfantryType it && it.IsExpansionUnit)));
            }
            if (expansionWarn || rulesChanged)
            {
                StringBuilder msg = new StringBuilder();
                if (expansionWarn)
                {
                    // The actual cleanup this refers to can be found in the ViewTool class, in the BasicSection_PropertyChanged function.
                    msg.Append("Expansion units have been disabled. This will remove all expansion units currently present on the map and in team types. Because of its complexity, this cleanup cannot be undone.\n\n");
                }
                if (rulesChanged)
                {
                    msg.Append("Rules have been changed.\n\n");
                }
                // The undo/redo clearing is done in the MainForm function that opens this form.
                msg.Append("The Undo/Redo history will be cleared to avoid conflicts with previous-performed actions involving any objects affected by this.")
                    .Append("\n\nAre you sure you want to continue?");
                DialogResult dres = MessageBox.Show(msg.ToString() , "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (dres != DialogResult.Yes)
                {
                    e.Cancel = true;
                }
            }
            
        }
    }
}
