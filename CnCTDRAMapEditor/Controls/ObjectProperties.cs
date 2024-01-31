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
        private Bitmap infoImage;
        private bool isMockObject;
        private HouseType originalHouse;
        private String originalTrigger;
        private int originalStrength;
        string[] filteredEvents;
        string[] filteredActions;

        public IGamePlugin Plugin { get; private set; }

        private string triggerToolTip;

        private INotifyPropertyChanged obj;
        public INotifyPropertyChanged Object
        {
            get => obj;
            set
            {
                if (!ReferenceEquals(obj, value))
                {
                    // old obj
                    if (obj != null)
                    {
                        obj.PropertyChanged -= Obj_PropertyChanged;
                    }
                    // new obj
                    obj = value;
                    if (obj != null)
                    {
                        if (obj is Building bld)
                        {
                            AdjustToStructurePrebuiltStatus(bld, GetHouseComboBox());
                        }
                        obj.PropertyChanged += Obj_PropertyChanged;
                    }
                    Rebind();
                }
            }
        }

        public ObjectProperties()
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

        private void ObjectProperties_Load(Object sender, EventArgs e)
        {
            // Fix for the fact the resize in the very first Rebind() call never works correctly,
            // because the UI is not initialised yet at that point.
            //this.Height = tableLayoutPanel1.PreferredSize.Height;
        }

        private void ObjectProperties_Resize(Object sender, EventArgs e)
        {
            //int prefH = tableLayoutPanel1.PreferredSize.Height + 10;
            //if (this.Height != prefH)
            //{
            //    this.Height = prefH;
            //}
        }

        public void Initialize(IGamePlugin plugin, bool isMockObject)
        {
            this.isMockObject = isMockObject;
            Plugin = plugin;
            plugin.Map.TriggersUpdated -= Triggers_CollectionChanged;
            plugin.Map.TriggersUpdated += Triggers_CollectionChanged;
            houseComboBox.DataSource = plugin.Map.Houses.Select(t => new TypeItem<HouseType>(t.Type.Name, t.Type)).ToArray();
            missionComboBox.DataSource = plugin.Map.MissionTypes;
            UpdateDataSource();
            Disposed += (sender, e) =>
            {
                Object = null;
                plugin.Map.TriggersUpdated -= Triggers_CollectionChanged;
            };
        }

        private void Triggers_CollectionChanged(object sender, EventArgs e)
        {
            UpdateDataSource();
        }

        private void UpdateDataSource()
        {
            string selected = triggerComboBox.SelectedItem as string;
            triggerComboBox.DataBindings.Clear();
            triggerComboBox.SelectedIndexChanged -= this.TriggerComboBox_SelectedIndexChanged;
            triggerComboBox.DataSource = null;
            triggerComboBox.Items.Clear();
            string[] items;
            Boolean isAircraft = obj is Unit un && un.Type.IsAircraft;
            Boolean isOnMap = true;
            switch (obj)
            {
                case Infantry infantry:
                case Unit unit:
                    items = Plugin.Map.FilterUnitTriggers().Select(t => t.Name).Distinct().ToArray();
                    filteredEvents = Plugin.Map.EventTypes.Where(ev => Plugin.Map.UnitEventTypes.Contains(ev)).Distinct().ToArray();
                    filteredActions = Plugin.Map.ActionTypes.Where(ac => Plugin.Map.UnitActionTypes.Contains(ac)).Distinct().ToArray();
                    break;
                case Building building:
                    isOnMap = building.IsPrebuilt;
                    items = Plugin.Map.FilterStructureTriggers().Select(t => t.Name).Distinct().ToArray();
                    filteredEvents = Plugin.Map.EventTypes.Where(ac => Plugin.Map.BuildingEventTypes.Contains(ac)).Distinct().ToArray();
                    filteredActions = Plugin.Map.ActionTypes.Where(ac => Plugin.Map.BuildingActionTypes.Contains(ac)).Distinct().ToArray();
                    break;
                default:
                    items = Plugin.Map.Triggers.Select(t => t.Name).Distinct().ToArray();
                    filteredEvents = null;
                    filteredActions = null;
                    break;
            }
            HashSet<string> allowedTriggers = new HashSet<string>(items);
            items = Trigger.None.Yield().Concat(Plugin.Map.Triggers.Select(t => t.Name).Where(t => allowedTriggers.Contains(t)).Distinct()).ToArray();
            int selectIndex = selected == null ? 0 : Enumerable.Range(0, items.Length).FirstOrDefault(x => String.Equals(items[x], selected, StringComparison.OrdinalIgnoreCase));
            triggerComboBox.DataSource = items;
            triggerComboBox.Enabled = !isAircraft && isOnMap;
            triggerToolTip = Map.MakeAllowedTriggersToolTip(filteredEvents, filteredActions);
            if (obj != null)
            {
                triggerComboBox.DataBindings.Add("SelectedItem", obj, "Trigger");
            }
            triggerComboBox.SelectedIndexChanged += this.TriggerComboBox_SelectedIndexChanged;
            triggerComboBox.SelectedItem = items[selectIndex];
            TriggerComboBox_SelectedIndexChanged(triggerComboBox, new EventArgs());
        }

        private void TriggerComboBox_SelectedIndexChanged(Object sender, EventArgs e)
        {
            if (filteredEvents == null || filteredActions == null)
            {
                return;
            }
            string selected = triggerComboBox.SelectedItem as string;
            Trigger trig = this.Plugin.Map.Triggers.FirstOrDefault(t => String.Equals(t.Name, selected, StringComparison.OrdinalIgnoreCase));
            triggerToolTip = Map.MakeAllowedTriggersToolTip(filteredEvents, filteredActions, trig);
            Point pt = MousePosition;
            Point lblPos = lblTriggerTypesInfo.PointToScreen(Point.Empty);
            Rectangle lblRect = new Rectangle(lblPos, lblTriggerTypesInfo.Size);
            if (lblRect.Contains(pt))
            {
                this.toolTip1.Hide(lblTriggerTypesInfo);
                LblTriggerTypesInfo_MouseEnter(lblTriggerTypesInfo, e);
            }
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
                        directionComboBox.DataSource = Plugin.Map.UnitDirectionTypes.Select(t => new TypeItem<DirectionType>(t.Name, t)).ToArray();
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
                        directionComboBox.DataSource = Plugin.Map.UnitDirectionTypes.Select(t => new TypeItem<DirectionType>(t.Name, t)).ToArray();
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
                        directionComboBox.DataSource = Plugin.Map.BuildingDirectionTypes.Select(t => new TypeItem<DirectionType>(t.Name, t)).ToArray();
                        directionLabel.Visible = directionVisible;
                        directionComboBox.Visible = directionVisible;
                        missionLabel.Visible = missionComboBox.Visible = false;
                        switch (Plugin.GameInfo.GameType)
                        {
                            case GameType.TiberianDawn:
                                {
                                    basePriorityLabel.Visible = basePriorityNud.Visible = true;
                                    prebuiltCheckBox.Visible = true;
                                    prebuiltCheckBox.Enabled = building.BasePriority >= 0;
                                    basePriorityNud.DataBindings.Add("Value", obj, "BasePriority");
                                    prebuiltCheckBox.DataBindings.Add("Checked", obj, "IsPrebuilt");
                                    sellableCheckBox.Visible = false;
                                    rebuildCheckBox.Visible = false;
                                }
                                break;
                            case GameType.RedAlert:
                                {
                                    basePriorityLabel.Visible = basePriorityNud.Visible = true;
                                    prebuiltCheckBox.Visible = true;
                                    prebuiltCheckBox.Enabled = building.BasePriority >= 0;
                                    basePriorityNud.DataBindings.Add("Value", obj, "BasePriority");
                                    prebuiltCheckBox.DataBindings.Add("Checked", obj, "IsPrebuilt");
                                    sellableCheckBox.DataBindings.Add("Checked", obj, "Sellable");
                                    rebuildCheckBox.DataBindings.Add("Checked", obj, "Rebuild");
                                    sellableCheckBox.Visible = true;
                                    rebuildCheckBox.Visible = true;
                                }
                                break;
                            case GameType.SoleSurvivor:
                                {
                                    basePriorityLabel.Visible = basePriorityNud.Visible = false;
                                    prebuiltCheckBox.Visible = false;
                                    sellableCheckBox.Visible = false;
                                    rebuildCheckBox.Visible = false;
                                }
                                break;
                        }
                    }
                    break;
            }
            // Collapse control to minimum required height.
            this.Height = tableLayoutPanel1.PreferredSize.Height;
            houseComboBox.DataBindings.Add("SelectedValue", obj, "House");
            strengthNud.DataBindings.Add("Value", obj, "Strength");
            directionComboBox.DataBindings.Add("SelectedValue", obj, "Direction");
            UpdateDataSource();
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
                            // Fix for the illegal state of it being set to not rebuild and not be prebuilt.
                            if (building.BasePriority < 0 && !building.IsPrebuilt)
                            {
                                building.IsPrebuilt = true;
                            }
                            AdjustToStructurePrebuiltStatus(building, GetHouseComboBox());
                        }
                    }
                    break;
                case "IsPrebuilt":
                    {
                        if (obj is Building building)
                        {
                            if (!building.IsPrebuilt)
                            {
                                originalHouse = building.House;
                                originalTrigger = building.Trigger;
                                originalStrength = building.Strength;
                            }
                            AdjustToStructurePrebuiltStatus(building, GetHouseComboBox());
                            if (building.IsPrebuilt)
                            {
                                if (originalHouse != null) building.House = originalHouse;
                                if (originalTrigger != null) building.Trigger = originalTrigger;
                                if (originalStrength != 0) building.Strength = originalStrength;
                            }
                        }
                    }
                    break;
            }
            // The undo/redo system now handles plugin dirty state.
        }

        private PropertiesComboBox GetHouseComboBox()
        {
            return houseComboBox;
        }

        private void AdjustToStructurePrebuiltStatus(Building building, PropertiesComboBox houseComboBox)
        {
            if (building.BasePriority >= 0 && !building.IsPrebuilt)
            {
                HouseType house = Plugin.Map.GetBaseHouse(Plugin.GameInfo);
                if (house.ID >= 0)
                {
                    building.House = house;
                }
                else
                {
                    // Fix for changing the combobox to one only contain "None".
                    houseComboBox.DataBindings.Clear();
                    houseComboBox.DataSource = house.Yield().Select(t => new TypeItem<HouseType>(t.Name, t)).ToArray();
                    houseComboBox.SelectedIndex = 0;
                    building.House = house;
                    houseComboBox.DataBindings.Add("SelectedValue", obj, "House");
                }
            }
            else
            {
                // Fix for restoring "None" to a normal House. Only needed for TD.
                HouseType selected = houseComboBox.SelectedValue as HouseType;
                if (selected != null && (selected.Flags & HouseTypeFlag.BaseHouse) == HouseTypeFlag.BaseHouse)
                {
                    houseComboBox.DataBindings.Clear();
                    TypeItem<HouseType>[] houses = Plugin.Map.Houses.Select(t => new TypeItem<HouseType>(t.Type.Name, t.Type)).ToArray();
                    houseComboBox.DataSource = houses;
                    String opposing = Plugin.GameInfo.GetClassicOpposingPlayer(Plugin.Map.BasicSection.Player);
                    HouseType restoredHouse = Plugin.Map.Houses.Where(h => h.Type.Equals(opposing)).FirstOrDefault()?.Type ?? houses.First().Type;
                    building.House = restoredHouse;
                    houseComboBox.DataBindings.Add("SelectedValue", obj, "House");
                }
            }
            if (!building.IsPrebuilt)
            {
                building.Strength = 256;
                building.Direction = Plugin.Map.BuildingDirectionTypes.Where(d => d.Equals(FacingType.North)).First();
                building.Trigger = Trigger.None;
                building.Sellable = false;
                building.Rebuild = false;
            }
            if (directionComboBox.Visible)
            {
                directionComboBox.Enabled = building.IsPrebuilt;
            }
            houseComboBox.Enabled = building.IsPrebuilt;
            strengthNud.Enabled = building.IsPrebuilt;
            triggerComboBox.Enabled = building.IsPrebuilt;
            if (sellableCheckBox.Visible)
            {
                sellableCheckBox.Enabled = building.IsPrebuilt;
            }
            if (rebuildCheckBox.Visible)
            {
                rebuildCheckBox.Enabled = building.IsPrebuilt;
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

        private void LblTriggerTypesInfo_MouseEnter(Object sender, EventArgs e)
        {
            Control target = sender as Control;
            string tooltip;
            if (Object is Building bld && !bld.IsPrebuilt)
            {
                tooltip = "Triggers can only be linked to prebuilt structures.";
            }
            else if (Object is Unit un && un.Type.IsAircraft)
            {
                tooltip = "Triggers can not be linked to aircraft.";
            }
            else
            {
                tooltip = triggerToolTip;
            }
            ShowToolTip(target, tooltip);
        }

        private void ShowToolTip(Control target, string message)
        {
            if (target == null || message == null)
            {
                this.toolTip1.Hide(target);
                return;
            }
            Point resPoint = target.PointToScreen(new Point(0, target.Height));
            MethodInfo m = toolTip1.GetType().GetMethod("SetTool",
                       BindingFlags.Instance | BindingFlags.NonPublic);
            // private void SetTool(IWin32Window win, string text, TipInfo.Type type, Point position)
            m.Invoke(toolTip1, new object[] { target, message, 2, resPoint });
            //this.toolTip1.Show(triggerToolTip, target, target.Width, 0, 10000);
        }

        private void LblTriggerInfo_MouseLeave(Object sender, EventArgs e)
        {
            Control target = sender as Control;
            this.toolTip1.Hide(target);
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
            // Fix for the fact the popup got a different font and that made it an incorrect size.
            this.Font = ObjectProperties.Font;
            Padding = Margin = host.Padding = host.Margin = Padding.Empty;
            MinimumSize = ObjectProperties.MinimumSize;
            ObjectProperties.MinimumSize = ObjectProperties.Size;
            MaximumSize = ObjectProperties.MaximumSize;
            ObjectProperties.MaximumSize = ObjectProperties.Size;
            Items.Add(host);
            ObjectProperties.Size = ObjectProperties.PreferredSize;
            Size = ObjectProperties.Size;
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
