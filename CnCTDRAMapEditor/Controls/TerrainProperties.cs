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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public partial class TerrainProperties : UserControl
    {
        private bool isMockObject;

        public IGamePlugin Plugin { get; private set; }

        private string triggerToolTip;

        private Terrain terrain;
        public Terrain Terrain
        {
            get => terrain;
            set
            {
                if (terrain == value)
                    return;
                if (terrain != null)
                    terrain.PropertyChanged -= Obj_PropertyChanged;
                terrain = value;
                if (terrain != null)
                    terrain.PropertyChanged += Obj_PropertyChanged;
                Rebind();
            }
        }

        public TerrainProperties()
        {
            InitializeComponent();
        }

        public void Initialize(IGamePlugin plugin, bool isMockObject)
        {
            this.isMockObject = isMockObject;

            Plugin = plugin;
            plugin.Map.Triggers.CollectionChanged -= Triggers_CollectionChanged;
            plugin.Map.Triggers.CollectionChanged += Triggers_CollectionChanged;

            UpdateDataSource();

            Disposed += (sender, e) =>
            {
                Terrain = null;
                plugin.Map.Triggers.CollectionChanged -= Triggers_CollectionChanged;
            };
        }

        private void Triggers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateDataSource();
        }

        private void UpdateDataSource()
        {
            string selected = triggerComboBox.SelectedItem as string;
            triggerComboBox.DataSource = null;
            triggerComboBox.Items.Clear();
            string[] items =  Trigger.None.Yield().Concat(Plugin.Map.FilterTerrainTriggers().Select(t => t.Name).Distinct()).ToArray();
            string[] filteredTypes = Plugin.Map.EventTypes.Where(ev => Plugin.Map.TerrainEventTypes.Contains(ev)).Distinct().ToArray();
            int selectIndex = selected == null ? 0 : Enumerable.Range(0, items.Length).FirstOrDefault(x => String.Equals(items[x], selected, StringComparison.InvariantCultureIgnoreCase));
            triggerComboBox.DataSource = items;
            triggerComboBox.SelectedIndex = selectIndex;
            triggerToolTip = filteredTypes == null ? null : "Allowed trigger events:\n\u2022 " + String.Join("\n\u2022 ", filteredTypes);
        }

        private void Rebind()
        {
            triggerComboBox.DataBindings.Clear();

            if (terrain == null)
            {
                return;
            }
            UpdateDataSource();
            triggerComboBox.DataBindings.Add("SelectedItem", terrain, "Trigger");
        }

        private void Obj_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Type":
                    {
                        Rebind();
                    }
                    break;
            }
            if (!isMockObject)
            {
                Plugin.Dirty = true;
            }
        }

        private void comboBox_SelectedValueChanged(object sender, EventArgs e)
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

        private void LblTriggerInfo_Paint(Object sender, PaintEventArgs e)
        {
            Control lbl = sender as Control;
            int iconDim = (int)Math.Round(Math.Min(lbl.ClientSize.Width, lbl.ClientSize.Height) * .8f);
            int x = (lbl.ClientSize.Width - iconDim) / 2;
            int y = (lbl.ClientSize.Height - iconDim) / 2;
            e.Graphics.DrawIcon(SystemIcons.Information, new Rectangle(x, y, iconDim, iconDim));
        }


        private void LblTriggerInfo_MouseEnter(Object sender, EventArgs e)
        {
            Control target = sender as Control;
            if (target == null || triggerToolTip == null)
            {
                this.toolTip1.Hide(target);
                return;
            }
            Point resPoint = target.PointToScreen(new Point(0, target.Height));
            MethodInfo m = toolTip1.GetType().GetMethod("SetTool",
                       BindingFlags.Instance | BindingFlags.NonPublic);
            // private void SetTool(IWin32Window win, string text, TipInfo.Type type, Point position)
            m.Invoke(toolTip1, new object[] { target, triggerToolTip, 2, resPoint });
            //this.toolTip1.Show(triggerToolTip, target, target.Width, 0, 10000);
        }

        private void LblTriggerInfo_MouseLeave(Object sender, EventArgs e)
        {
            Control target = sender as Control;
            this.toolTip1.Hide(target);
        }
    }

    public class TerrainPropertiesPopup : ToolStripDropDown
    {
        private readonly ToolStripControlHost host;

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
            TerrainProperties.Disposed += (sender, e) =>
            {
                TerrainProperties = null;
                Dispose(true);
            };
        }

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e)
        {
            base.OnClosed(e);

            TerrainProperties.Terrain = null;
        }
    }
}
