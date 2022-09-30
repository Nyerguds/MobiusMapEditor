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
using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using MobiusEditor.Controls;
using MobiusEditor.Event;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;

namespace MobiusEditor.Tools.Dialogs
{
    public partial class CellTriggersToolDialog : ToolDialog<CellTriggersTool>
    {
        private Bitmap infoImage;
        public ComboBox TriggerCombo => triggerComboBox;

        public CellTriggersToolDialog(Form parentForm)
            : base(parentForm)
        {
            InitializeComponent();
            infoImage = new Bitmap(27, 27);
            using (Graphics g = Graphics.FromImage(infoImage))
            {
                g.DrawIcon(SystemIcons.Information, new Rectangle(0, 0, infoImage.Width, infoImage.Height));
            }
            lblTriggerInfo.Image = infoImage;
            lblTriggerInfo.ImageAlign = ContentAlignment.MiddleCenter;
        }

        protected override void InitializeInternal(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel, ToolTip mouseToolTip, IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs> undoRedoList)
        {
            Tool = new CellTriggersTool(mapPanel, activeLayers, toolStatusLabel, TriggerCombo, plugin, undoRedoList);
        }

        private void LblTriggerInfo_MouseEnter(Object sender, EventArgs e)
        {
            Control target = sender as Control;
            if (target == null || Tool == null || Tool.TriggerToolTip == null)
            {
                this.toolTip1.Hide(target);
                return;
            }
            Point resPoint = target.PointToScreen(new Point(0, target.Height));
            MethodInfo m = toolTip1.GetType().GetMethod("SetTool",
                       BindingFlags.Instance | BindingFlags.NonPublic);
            // private void SetTool(IWin32Window win, string text, TipInfo.Type type, Point position)
            m.Invoke(toolTip1, new object[] { target, Tool.TriggerToolTip, 2, resPoint });
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
                    lblTriggerInfo.Image = null;
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
}
