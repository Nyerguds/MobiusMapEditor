//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                     Version 2, December 2004
//
//  Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
//  Everyone is permitted to copy and distribute verbatim or modified
//  copies of this license document, and changing it is allowed as long
//  as the name is changed.
//
//             DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//    TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//   0. You just DO WHAT THE FUCK YOU WANT TO.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MobiusEditor.Utility
{
    public class ToolTipFixer: IDisposable
    {
        private ToolTip tooltip;
        private Dictionary<Type, int> extraWidths;
        private int showDuration;
        private List<Control> attachedControls;

        public ToolTipFixer(Form form, ToolTip tooltip, int showDuration, Dictionary<Type, int> extraWidths)
        {
            this.tooltip = tooltip;
            this.showDuration = showDuration;
            this.extraWidths = extraWidths;
            this.FixToolTips(form, null);
        }

        private void FixToolTips(Control control, List<Control> tooltipOwners)
        {
            // Allows tooltips that remain open for longer than 5 seconds.
            bool mainCall = tooltipOwners == null;
            if (mainCall)
                tooltipOwners = new List<Control>();
            foreach (Control c in control.Controls)
            {
                string s = this.tooltip.GetToolTip(c);
                if (!String.IsNullOrEmpty(s))
                    tooltipOwners.Add(c);
                if (c.Controls.Count > 0)
                    FixToolTips(c, tooltipOwners);
            }
            if (mainCall)
            {
                foreach (Control ctrl in tooltipOwners)
                {
                    string tooltip = this.tooltip.GetToolTip(ctrl);
                    this.tooltip.SetToolTip(ctrl, null);
                    if (String.IsNullOrEmpty(tooltip))
                        continue;
                    ctrl.Tag = tooltip;
                    // Avoid double bindings.
                    ctrl.MouseHover -= ShowToolTip;
                    ctrl.MouseLeave -= HideToolTip;
                    // Attach listeners.
                    ctrl.MouseHover += ShowToolTip;
                    ctrl.MouseLeave += HideToolTip;
                }
                this.attachedControls = tooltipOwners;
            }
        }

        private void ShowToolTip(object sender, EventArgs e)
        {
            Control target = sender as Control;
            string tooltipText;
            if (target == null || (tooltipText = target.Tag as string) == null)
                return;
            int offset = target.Width;
            // Add width of the next column.
            int extraWidth;
            if (extraWidths.TryGetValue(sender.GetType(), out extraWidth))
                offset += extraWidth;
            this.tooltip.Show(tooltipText, target, offset, 0, showDuration);
        }

        private void HideToolTip(object sender, EventArgs e)
        {
            Control target = sender as Control;
            if (target != null)
                this.tooltip.Hide(target);
        }

        public void Dispose()
        {
            if (this.attachedControls != null)
            {
                foreach (Control control in attachedControls)
                {
                    if (control == null || control.IsDisposed)
                        continue;
                    try
                    {
                        control.MouseHover -= ShowToolTip;
                        control.MouseLeave -= HideToolTip;
                    }
                    catch
                    {
                        // Ignore
                    }
                }
            }
            attachedControls.Clear();
            this.tooltip = null;
        }

        /// <summary>
        /// Call this on a listbox's MouseMove event.
        /// </summary>
        /// <param name="lbSender"></param>
        /// <param name="formMousePosition"></param>
        /// <param name="tooltip"></param>
        /// <param name="itemDescriptions"></param>
        /// <param name="defaultVal"></param>
        public static void ToolTipForListBox(ListBox lbSender, Point formMousePosition, ToolTip tooltip, Dictionary<string, string> itemDescriptions, string defaultVal)
        {
            if (lbSender == null || tooltip == null || itemDescriptions == null)
                return;
            //Point formMousePosition = lbSender.FindForm().MousePosition;
            Point listBoxClientAreaPosition = lbSender.PointToClient(formMousePosition);
            int hoveredIndex = lbSender.IndexFromPoint(listBoxClientAreaPosition);
            string hoveredItem = hoveredIndex == -1 ? null : lbSender.Items[hoveredIndex] as string;
            if (hoveredItem == null)
            {
                tooltip.Active = false;
                tooltip.Tag = null;
                tooltip.SetToolTip(lbSender, null);
            }
            else
            {
                if (tooltip.Tag as string == hoveredItem)
                    return;
                string toolTipText = itemDescriptions.TryGetValue(hoveredItem, out toolTipText) ? toolTipText : defaultVal;
                tooltip.Active = false;
                tooltip.SetToolTip(lbSender, toolTipText);
                tooltip.Active = true;
                tooltip.Tag = hoveredItem;
            }
        }
    }
}
