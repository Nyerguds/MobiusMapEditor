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

using System.Drawing;
using System;
using System.Reflection;
using System.Windows.Forms;
using MobiusEditor.Controls;
using MobiusEditor.Event;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using MobiusEditor.Dialogs;

namespace MobiusEditor.Tools.Dialogs
{
    public partial class WaypointsToolDialog :
#if !EDITMODE
        ToolDialog<WaypointsTool>
#else
        Form
#endif
    {
        private Control tooltipShownOn;
        private const string MODE_WP = "Waypoints";
        private const string MODE_RT = "Team routes";

        public WaypointsToolDialog(Form parentForm)
#if !EDITMODE
        : base(parentForm)
#endif
        {
            InitializeComponent();
        }

#if !EDITMODE
        protected override void InitializeInternal(MapPanel mapPanel, MapLayerFlag activeLayers, ToolStripStatusLabel toolStatusLabel, ToolTip mouseToolTip,
            IGamePlugin plugin, UndoRedoList<UndoRedoEventArgs, ToolType> undoRedoList)
        {
            Tool = new WaypointsTool(mapPanel, activeLayers, toolStatusLabel, cmbWaypoints, btnJumpTo, cmbTeamTypes, btnJumpToTeam, btnEditTeam, btnModeSwitch, plugin, undoRedoList);
            Tool.OnTeamTypeChanged += Tool_OnTeamTypeChanged;
            Tool.OnModeChanged += Tool_OnModeChanged;
            Tool.DoTeamTypeEdit += Tool_DoTeamTypeEdit;
        }
#endif

        private void Tool_DoTeamTypeEdit(object sender, EventArgs e)
        {
            if (!(sender is ComboBox teams))
            {
                return;
            }
            string selected = teams.SelectedItem as string;
            if (selected != null && !TeamType.None.Equals(selected, StringComparison.OrdinalIgnoreCase))
            {
#if !EDITMODE
                TeamTypesDialog.ShowTeamTypesEditor(parentForm, Tool.Plugin, Tool.Url, selected);
                Tool.TeamTypeEditDone();
#endif
            }
        }

        private void Tool_OnModeChanged(object sender, EventArgs e)
        {
            if (!(sender is WaypointsTool wpt))
            {
                return;
            }
            bool routesMode = wpt.RoutesMode;
            SuspendLayout();
            btnModeSwitch.Text = routesMode ? MODE_RT : MODE_WP;
            lblWaypoint.Visible = !routesMode;
            cmbWaypoints.Visible = !routesMode;
            lblRoute.Visible = routesMode;
            cmbTeamTypes.Visible = routesMode;
            btnJumpTo.Visible = !routesMode;
            btnJumpToTeam.Visible = routesMode;
            btnEditTeam.Visible = routesMode;
            AcceptButton = routesMode ? btnJumpToTeam : btnJumpTo;
            ResumeLayout();
        }

        private void Tool_OnTeamTypeChanged(object sender, EventArgs e)
        {
            Point pt = MousePosition;
            Point cmbPos = cmbTeamTypes.PointToScreen(Point.Empty);
            Rectangle cmbTrigRect = new Rectangle(cmbPos, cmbTeamTypes.Size);
            if (cmbTrigRect.Contains(pt))
            {
                this.toolTip1.Hide(cmbTeamTypes);
                CmbTeamTypes_MouseEnter(cmbTeamTypes, e);
            }
        }

        private void CmbTeamTypes_MouseEnter(object sender, EventArgs e)
        {
            Control target = sender as Control;
#if !EDITMODE
            ShowToolTip(target, Tool.TeamTypeToolTip);
#endif
        }

        private void CmbTrigger_MouseMove(object sender, MouseEventArgs e)
        {
            if (tooltipShownOn != sender)
            {
                CmbTeamTypes_MouseEnter(sender, e);
            }
        }

        private void BtnEditTeam_MouseEnter(object sender, EventArgs e)
        {
            Control target = sender as Control;
            ShowToolTip(target, "Edit teamtype");
        }

        private void BtnEditTeam_MouseMove(object sender, MouseEventArgs e)
        {
            if (tooltipShownOn != sender)
            {
                BtnEditTeam_MouseEnter(sender, e);
            }
        }

        private void ShowToolTip(Control target, string message)
        {
            if (target == null || message == null)
            {
                HideToolTip(target, null);
                return;
            }
            Point resPoint = target.PointToScreen(new Point(0, target.Height));
            MethodInfo m = toolTip1.GetType().GetMethod("SetTool",
                       BindingFlags.Instance | BindingFlags.NonPublic);
            m.Invoke(toolTip1, new object[] { target, message, 2, resPoint });
            tooltipShownOn = target;
        }

        private void HideToolTip(object sender, EventArgs e)
        {
            try
            {
                if (tooltipShownOn != null)
                {
                    toolTip1.Hide(tooltipShownOn);
                }
                if (sender is Control target)
                {
                    toolTip1.Hide(target);
                }
            }
            catch { /* ignore */ }
            tooltipShownOn = null;
        }
    }
}
