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

// EDITMODE: uncomment this to edit the form in the visual editor
//#define EDITMODE

using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using MobiusEditor.Controls;
using MobiusEditor.Event;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;

namespace MobiusEditor.Tools.Dialogs
{
    public partial class CellTriggersToolDialog :
#if !EDITMODE
        ToolDialog<CellTriggersTool>
#else
        Form
#endif
    {
        private Bitmap infoImage;
        private Control tooltipShownOn;

        public CellTriggersToolDialog(Form parentForm)
#if !EDITMODE
        : base(parentForm)
#endif
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
#if !EDITMODE
        protected override void InitializeInternal(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel, ToolTip mouseToolTip,
            IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> undoRedoList)
        {
            Tool = new CellTriggersTool(mapPanel, activeLayers, toolStatusLabel, cmbTrigger, btnJumpTo, plugin, undoRedoList);
            Tool.OnTriggerChanged += this.Tool_OnTriggerChanged;
        }
#endif
        private void Tool_OnTriggerChanged(Object sender, EventArgs e)
        {
            Point pt = MousePosition;
            Point lblPos = lblTriggerTypesInfo.PointToScreen(Point.Empty);
            Point cmbPos = cmbTrigger.PointToScreen(Point.Empty);
            Rectangle lblInfoRect = new Rectangle(lblPos, lblTriggerTypesInfo.Size);
            Rectangle cmbTrigRect = new Rectangle(cmbPos, cmbTrigger.Size);
            if (lblInfoRect.Contains(pt))
            {
                this.toolTip1.Hide(lblTriggerTypesInfo);
                LblTriggerTypesInfo_MouseEnter(lblTriggerTypesInfo, e);
            }
            else if(cmbTrigRect.Contains(pt))
            {
                this.toolTip1.Hide(cmbTrigger);
                CmbTrigger_MouseEnter(cmbTrigger, e);
            }
        }

        private void LblTriggerTypesInfo_MouseEnter(Object sender, EventArgs e)
        {
            Control target = sender as Control;
            ShowToolTip(target, Tool.TriggerInfoToolTip);
        }

        private void LblTriggerTypesInfo_MouseMove(object sender, MouseEventArgs e)
        {
            if (tooltipShownOn != sender)
            {
                LblTriggerTypesInfo_MouseEnter(sender, e);
            }
        }

        private void CmbTrigger_MouseEnter(object sender, EventArgs e)
        {
            Control target = sender as Control;
            ShowToolTip(target, Tool.TriggerToolTip);
        }

        private void CmbTrigger_MouseMove(object sender, MouseEventArgs e)
        {
            if (tooltipShownOn != sender)
            {
                CmbTrigger_MouseEnter(sender, e);
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
}
