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
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public partial class SmudgeProperties : UserControl
    {
        private bool isMockObject;

        public IGamePlugin Plugin { get; private set; }

        private Smudge smudge;
        public Smudge Smudge
        {
            get => smudge;
            set
            {
                if (smudge == value)
                    return;
                if (smudge != null)
                    smudge.PropertyChanged -= Obj_PropertyChanged;
                smudge = value;
                if (smudge != null)
                    smudge.PropertyChanged += Obj_PropertyChanged;
                Rebind();
            }
        }

        public SmudgeProperties()
        {
            InitializeComponent();
        }

        public void Initialize(IGamePlugin plugin, bool isMockObject)
        {
            this.isMockObject = isMockObject;
            Plugin = plugin;
            UpdateDataSource();
        }

        private void UpdateDataSource()
        {
            int[] data;
            if (smudge != null && smudge.Type.Icons > 1)
                data = Enumerable.Range(0, smudge.Type.Icons).ToArray();
            else
                data = new int[] { 0 };
            stateComboBox.DataSource = data;
        }

        private void Rebind()
        {
            stateComboBox.DataBindings.Clear();
            stateComboBox.DataSource = null;
            stateComboBox.Items.Clear();
            if (smudge == null)
            {
                return;
            }
            UpdateDataSource();
            if (stateComboBox.Items.Count > 1)
            {
                stateComboBox.DataBindings.Add("SelectedItem", smudge, "Icon");
            }
            stateComboBox.Enabled = stateComboBox.Items.Count > 1;
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
            // The undo/redo system now handles plugin dirty state.
        }

        private void comboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            foreach (Binding binding in (sender as ComboBox).DataBindings)
            {
                binding.WriteValue();
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            Smudge = null;
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class SmudgePropertiesPopup : ToolStripDropDown
    {
        private ToolStripControlHost host;

        public SmudgeProperties SmudgeProperties { get; private set; }

        public SmudgePropertiesPopup(IGamePlugin plugin, Smudge smudge)
        {
            SmudgeProperties = new SmudgeProperties();
            SmudgeProperties.Smudge = smudge;
            SmudgeProperties.Initialize(plugin, false);

            host = new ToolStripControlHost(SmudgeProperties);
            Padding = Margin = host.Padding = host.Margin = Padding.Empty;
            MinimumSize = SmudgeProperties.MinimumSize;
            SmudgeProperties.MinimumSize = SmudgeProperties.Size;
            MaximumSize = SmudgeProperties.MaximumSize;
            SmudgeProperties.MaximumSize = SmudgeProperties.Size;
            Size = SmudgeProperties.Size;
            Items.Add(host);
        }

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e)
        {
            // Since dispose doesn't seem to auto-trigger, dispose and remove all this manually.
            SmudgeProperties.Smudge = null;
            SmudgeProperties = null;
            Items.Remove(host);
            host.Dispose();
            host = null;
            base.OnClosed(e);
        }
    }
}
