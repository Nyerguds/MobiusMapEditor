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
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public partial class TerrainProperties : UserControl
    {
        private Bitmap infoImage;
        private string triggerInfoToolTip;
        private string triggerToolTip;
        private Control tooltipShownOn;
        private string[] filteredEvents;
        private string[] filteredActions;

        private bool isMockObject;

        public IGamePlugin Plugin { get; private set; }

        private Terrain terrain;
        public Terrain Terrain
        {
            get => terrain;
            set
            {
                if (terrain == value)
                {
                    return;
                }
                terrain = value;
                UpdateDataSource();
            }
        }

        public TerrainProperties()
        {
            InitializeComponent();
            infoImage = new Bitmap(27, 27);
            infoImage.SetResolution(96, 96);
            using (Graphics g = Graphics.FromImage(infoImage))
            {
                g.DrawIcon(SystemIcons.Information, new Rectangle(0, 0, infoImage.Width, infoImage.Height));
            }
            lblTriggerTypesInfo.Image = infoImage;
            lblTriggerTypesInfo.ImageAlign = ContentAlignment.MiddleCenter;
        }

        public void Initialize(IGamePlugin plugin, bool isMockObject)
        {
            this.isMockObject = isMockObject;
            Plugin = plugin;
            plugin.Map.TriggersUpdated -= Triggers_CollectionChanged;
            plugin.Map.TriggersUpdated += Triggers_CollectionChanged;
            UpdateDataSource();
            Disposed += (sender, e) =>
            {
                plugin.Map.TriggersUpdated -= Triggers_CollectionChanged;
            };
        }

        private void Triggers_CollectionChanged(object sender, EventArgs e)
        {
            UpdateDataSource();
        }

        private void UpdateDataSource()
        {
            string selected = terrain != null ? terrain.Trigger : triggerComboBox.SelectedItem as string;
            triggerComboBox.DataBindings.Clear();
            triggerComboBox.SelectedIndexChanged -= this.TriggerComboBox_SelectedIndexChanged;
            triggerComboBox.DataSource = null;
            triggerComboBox.Items.Clear();
            HashSet<string> allowedTriggers = Plugin.Map.FilterTerrainTriggers().Select(t => t.Name).Distinct().ToHashSet(StringComparer.OrdinalIgnoreCase);
            filteredEvents = Plugin.Map.EventTypes.Where(ev => Plugin.Map.TerrainEventTypes.Contains(ev)).Distinct().ToArray();
            filteredActions = Plugin.Map.ActionTypes.Where(ev => Plugin.Map.TerrainActionTypes.Contains(ev)).Distinct().ToArray();
            string[] items = Trigger.None.Yield().Concat(Plugin.Map.Triggers.Select(t => t.Name).Where(t => allowedTriggers.Contains(t)).Distinct()).ToArray();
            int selectIndex = String.IsNullOrEmpty(selected) ? 0 : Enumerable.Range(0, items.Length).FirstOrDefault(x => String.Equals(items[x], selected, StringComparison.OrdinalIgnoreCase));
            triggerComboBox.DataSource = items;
            if (terrain != null)
            {
                // Ensure that the object's trigger is in the list.
                terrain.Trigger = items[selectIndex];
                triggerComboBox.DataBindings.Add("SelectedItem", terrain, "Trigger");
            }
            int sel = triggerComboBox.SelectedIndex;
            triggerComboBox.SelectedIndexChanged += this.TriggerComboBox_SelectedIndexChanged;
            triggerComboBox.SelectedIndex = selectIndex;
            if (sel == selectIndex)
            {
                TriggerComboBox_SelectedIndexChanged(triggerComboBox, new EventArgs());
            }
        }

        private void TriggerComboBox_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if (filteredEvents == null || filteredActions == null)
            {
                return;
            }
            string selected = triggerComboBox.SelectedItem as string;
            Trigger trig = this.Plugin.Map.Triggers.FirstOrDefault(t => String.Equals(t.Name, selected, StringComparison.OrdinalIgnoreCase));
            triggerInfoToolTip = Map.MakeAllowedTriggersToolTip(filteredEvents, filteredActions, trig);
            triggerToolTip = Plugin.TriggerSummary(trig, true, false);
            Point pt = MousePosition;
            Point lblPos = lblTriggerTypesInfo.PointToScreen(Point.Empty);
            Point cmbPos = triggerComboBox.PointToScreen(Point.Empty);
            Rectangle lblInfoRect = new Rectangle(lblPos, lblTriggerTypesInfo.Size);
            Rectangle cmbTrigRect = new Rectangle(cmbPos, triggerComboBox.Size);
            if (lblInfoRect.Contains(pt))
            {
                this.toolTip1.Hide(lblTriggerTypesInfo);
                LblTriggerTypesInfo_MouseEnter(lblTriggerTypesInfo, e);
            }
            else if (cmbTrigRect.Contains(pt))
            {
                this.toolTip1.Hide(triggerComboBox);
                TriggerComboBox_MouseEnter(triggerComboBox, e);
            }
        }

        private void TriggerComboBox_MouseEnter(Object sender, EventArgs e)
        {
            Control target = sender as Control;
            ShowToolTip(target, triggerToolTip);
        }

        private void TriggerComboBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (tooltipShownOn != sender)
            {
                TriggerComboBox_MouseEnter(sender, e);
            }
        }

        private void TriggerComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            foreach (Binding binding in (sender as ComboBox).DataBindings)
            {
                binding.WriteValue();
            }
        }

        private void nud_ValueChanged(object sender, EventArgs e)
        {
            foreach (Binding binding in (sender as NumericUpDown).DataBindings)
            {
                binding.WriteValue();
            }
        }

        private void LblTriggerTypesInfo_MouseEnter(Object sender, EventArgs e)
        {
            Control target = sender as Control;
            ShowToolTip(target, triggerInfoToolTip);
        }

        private void LblTriggerTypesInfo_MouseMove(object sender, MouseEventArgs e)
        {
            if (tooltipShownOn != sender)
            {
                TriggerComboBox_MouseEnter(sender, e);
            }
        }

        private void ShowToolTip(Control target, string message)
        {
            if (target == null || message == null)
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

        private void HideToolTip(object sender, EventArgs e)
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

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                try
                {
                    lblTriggerTypesInfo.Image = null;
                }
                catch { /*ignore*/}
                try
                {
                    infoImage.Dispose();
                }
                catch { /*ignore*/}
                infoImage = null;
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class TerrainPropertiesPopup : ToolStripDropDown
    {
        private ToolStripControlHost host;

        public TerrainProperties TerrainProperties { get; private set; }

        public TerrainPropertiesPopup(IGamePlugin plugin, Terrain terrain)
        {
            TerrainProperties = new TerrainProperties();
            TerrainProperties.Initialize(plugin, false);
            TerrainProperties.Terrain = terrain;

            host = new ToolStripControlHost(TerrainProperties);
            Padding = Margin = host.Padding = host.Margin = Padding.Empty;
            MinimumSize = TerrainProperties.MinimumSize;
            TerrainProperties.MinimumSize = TerrainProperties.Size;
            MaximumSize = TerrainProperties.MaximumSize;
            TerrainProperties.MaximumSize = TerrainProperties.Size;
            Size = TerrainProperties.Size;
            Items.Add(host);
        }

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e)
        {
            // Since dispose doesn't seem to auto-trigger, dispose and remove all this manually.
            TerrainProperties.Terrain = null;
            TerrainProperties = null;
            Items.Remove(host);
            host.Dispose();
            host = null;
            base.OnClosed(e);
        }
    }
}
