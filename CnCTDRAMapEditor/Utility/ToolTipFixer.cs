using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MobiusEditor.Utility
{
    public class ToolTipFixer: IDisposable
    {
        private ToolTip tooltip;
        private Dictionary<Type, Int32> extraWidths;
        private int showDuration;
        private List<Control> attachedControls;

        public ToolTipFixer(Form form, ToolTip tooltip, int showDuration, Dictionary<Type, Int32> extraWidths) 
        {
            this.tooltip = tooltip;
            this.showDuration = showDuration;
            this.extraWidths = extraWidths;
            this.FixToolTips(form, null);
        }

        private void FixToolTips(Control control, List<Control> tooltipOwners)
        {
            // Allows tooltips that remain open for longer than 5 seconds.
            Boolean mainCall = tooltipOwners == null;
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
                    ctrl.MouseHover += ShowToolTip;
                }
                this.attachedControls = tooltipOwners;
            }
        }

        private void ShowToolTip(object sender, EventArgs e)
        {
            Control target = sender as Control;
            String tooltipText;
            if (target == null || (tooltipText = target.Tag as String) == null)
                return;
            int offset = target.Width;
            // Add width of the next column.
            int extraWidth;
            if (extraWidths.TryGetValue(sender.GetType(), out extraWidth))
                offset += extraWidth;
            this.tooltip.Show(tooltipText, target, offset, 0, showDuration);
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
    }
}
