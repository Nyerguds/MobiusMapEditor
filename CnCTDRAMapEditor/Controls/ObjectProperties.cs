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
using MobiusEditor.Render;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public partial class ObjectProperties : UserControl
    {
        private bool isMockObject;

        public IGamePlugin Plugin { get; private set; }

        private string triggerToolTip;

        private INotifyPropertyChanged obj;
        public INotifyPropertyChanged Object
        {
            get => obj;
            set
            {
                if (obj != value)
                {
                    if (obj != null)
                    {
                        obj.PropertyChanged -= Obj_PropertyChanged;
                    }

                    obj = value;

                    if (obj != null)
                    {
                        obj.PropertyChanged += Obj_PropertyChanged;
                    }

                    Rebind();
                }
            }
        }

        public ObjectProperties()
        {
            InitializeComponent();
        }

        public void Initialize(IGamePlugin plugin, bool isMockObject)
        {
            this.isMockObject = isMockObject;
            Plugin = plugin;
            plugin.Map.Triggers.CollectionChanged -= Triggers_CollectionChanged;
            plugin.Map.Triggers.CollectionChanged += Triggers_CollectionChanged;

            houseComboBox.DataSource = plugin.Map.Houses.Select(t => new TypeItem<HouseType>(t.Type.Name, t.Type)).ToArray();
            missionComboBox.DataSource = plugin.Map.MissionTypes;

            UpdateDataSource();

            Disposed += (sender, e) =>
            {
                Object = null;
                plugin.Map.Triggers.CollectionChanged -= Triggers_CollectionChanged;
            };
        }

        private void Triggers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateDataSource();
        }

        private void UpdateDataSource()
        {
            string selected = triggerComboBox.SelectedItem as string;
            triggerComboBox.DataSource = null;
            triggerComboBox.Items.Clear();
            string[] items;
            string[] filteredTypes;
            switch (obj)
            {
                case Infantry infantry:
                case Unit unit:
                    items = Trigger.None.Yield().Concat(Plugin.Map.FilterUnitTriggers().Select(t => t.Name).Distinct()).ToArray();
                    filteredTypes = Plugin.Map.EventTypes.Where(ev => Plugin.Map.UnitEventTypes.Contains(ev)).Distinct().ToArray();
                    break;
                case Building building:
                    items = Trigger.None.Yield().Concat(Plugin.Map.FilterStructureTriggers().Select(t => t.Name).Distinct()).ToArray();
                    filteredTypes = Plugin.Map.EventTypes.Where(ev => Plugin.Map.StructureEventTypes.Contains(ev)).Distinct().ToArray();
                    break;
                default:
                    items = Trigger.None.Yield().Concat(Plugin.Map.Triggers.Select(t => t.Name).Distinct()).ToArray();
                    filteredTypes = null;
                    break;
            }
            int selectIndex = selected == null ? 0 : Enumerable.Range(0, items.Length).FirstOrDefault(x => String.Equals(items[x], selected, StringComparison.InvariantCultureIgnoreCase));
            triggerComboBox.DataSource = items;
            triggerComboBox.SelectedIndex = selectIndex;
            triggerToolTip = filteredTypes == null ? null : "Allowed trigger events:\n\u2022 " + String.Join("\n\u2022 ", filteredTypes);
        }

        private void Rebind()
        {
            houseComboBox.DataBindings.Clear();
            strengthNud.DataBindings.Clear();
            directionComboBox.DataBindings.Clear();
            missionComboBox.DataBindings.Clear();
            triggerComboBox.DataBindings.Clear();
            basePriorityNud.DataBindings.Clear();
            prebuiltCheckBox.DataBindings.Clear();
            sellableCheckBox.DataBindings.Clear();
            rebuildCheckBox.DataBindings.Clear();

            if (obj == null)
            {
                return;
            }

            switch (obj)
            {
                case Infantry infantry:
                    {
                        houseComboBox.Enabled = true;
                        directionComboBox.DataSource = Plugin.Map.DirectionTypes
                            .Where(t => t.Facing != FacingType.None)
                            .Select(t => new TypeItem<DirectionType>(t.Name, t)).ToArray();

                        missionComboBox.DataBindings.Add("SelectedItem", obj, "Mission");
                        missionLabel.Visible = missionComboBox.Visible = true;
                        basePriorityLabel.Visible = basePriorityNud.Visible = false;
                        prebuiltCheckBox.Visible = false;
                        sellableCheckBox.Visible = false;
                        rebuildCheckBox.Visible = false;
                    }
                    break;
                case Unit unit:
                    {
                        houseComboBox.Enabled = true;
                        directionComboBox.DataSource = Plugin.Map.DirectionTypes.Select(t => new TypeItem<DirectionType>(t.Name, t)).ToArray();
                        missionComboBox.DataBindings.Add("SelectedItem", obj, "Mission");
                        missionLabel.Visible = missionComboBox.Visible = true;
                        basePriorityLabel.Visible = basePriorityNud.Visible = false;
                        prebuiltCheckBox.Visible = false;
                        sellableCheckBox.Visible = false;
                        rebuildCheckBox.Visible = false;
                    }
                    break;
                case Building building:
                    {
                        houseComboBox.Enabled = building.IsPrebuilt;
                        bool directionVisible = (building.Type != null) && building.Type.HasTurret;
                        directionComboBox.DataSource = Plugin.Map.DirectionTypes.Select(t => new TypeItem<DirectionType>(t.Name, t)).ToArray();
                        directionLabel.Visible = directionVisible;
                        directionComboBox.Visible = directionVisible;
                        missionLabel.Visible = missionComboBox.Visible = false;
                        basePriorityLabel.Visible = basePriorityNud.Visible = true;
                        prebuiltCheckBox.Visible = true;
                        prebuiltCheckBox.Enabled = building.BasePriority >= 0;

                        basePriorityNud.DataBindings.Add("Value", obj, "BasePriority");
                        prebuiltCheckBox.DataBindings.Add("Checked", obj, "IsPrebuilt");

                        switch (Plugin.GameType)
                        {
                            case GameType.TiberianDawn:
                                {
                                    sellableCheckBox.Visible = false;
                                    rebuildCheckBox.Visible = false;
                                } break;
                            case GameType.RedAlert:
                                {
                                    sellableCheckBox.DataBindings.Add("Checked", obj, "Sellable");
                                    rebuildCheckBox.DataBindings.Add("Checked", obj, "Rebuild");
                                    sellableCheckBox.Visible = true;
                                    rebuildCheckBox.Visible = true;
                                } break;
                        }
                    }
                    break;
            }

            houseComboBox.DataBindings.Add("SelectedValue", obj, "House");
            strengthNud.DataBindings.Add("Value", obj, "Strength");
            directionComboBox.DataBindings.Add("SelectedValue", obj, "Direction");
            UpdateDataSource();
            triggerComboBox.DataBindings.Add("SelectedItem", obj, "Trigger");
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
                case "BasePriority":
                    {
                        if (obj is Building building)
                        {
                            prebuiltCheckBox.Enabled = building.BasePriority >= 0;
                        }
                    }
                    break;
                case "IsPrebuilt":
                    {
                        if (obj is Building building)
                        {
                            if (!building.IsPrebuilt)
                            {
                                var basePlayer = Plugin.Map.HouseTypes.Where(h => h.Equals(Plugin.Map.BasicSection.BasePlayer)).FirstOrDefault() ?? Plugin.Map.HouseTypes.First();
                                building.House = basePlayer;
                            }
                            houseComboBox.Enabled = building.IsPrebuilt;
                        }
                    } break;
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

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            foreach (Binding binding in (sender as CheckBox).DataBindings)
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

    public class ObjectPropertiesPopup : ToolStripDropDown
    {
        private readonly ToolStripControlHost host;

        public ObjectProperties ObjectProperties { get; private set; }

        public ObjectPropertiesPopup(IGamePlugin plugin, INotifyPropertyChanged obj)
        {
            ObjectProperties = new ObjectProperties();
            ObjectProperties.Initialize(plugin, false);
            ObjectProperties.Object = obj;

            host = new ToolStripControlHost(ObjectProperties);
            Padding = Margin = host.Padding = host.Margin = Padding.Empty;
            MinimumSize = ObjectProperties.MinimumSize;
            ObjectProperties.MinimumSize = ObjectProperties.Size;
            MaximumSize = ObjectProperties.MaximumSize;
            ObjectProperties.MaximumSize = ObjectProperties.Size;
            Size = ObjectProperties.Size;
            Items.Add(host);
            ObjectProperties.Disposed += (sender, e) =>
            {
                ObjectProperties = null;
                Dispose(true);
            };
        }

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e)
        {
            base.OnClosed(e);

            ObjectProperties.Object = null;
        }
    }
}
