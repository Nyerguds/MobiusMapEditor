using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public class ComboBoxSmartWidth : ComboBox
    {
        protected override void OnDropDown(EventArgs e)
        {
            this.SetDropDownWidth(e);
            base.OnDropDown(e);
        }

        private void SetDropDownWidth(EventArgs e)
        {
            Int32 widestStringInPixels = this.Width;
            Boolean hasScrollBar = this.Items.Count * this.ItemHeight > this.DropDownHeight;
            if (hasScrollBar)
                widestStringInPixels -= SystemInformation.VerticalScrollBarWidth;
            Boolean noDisplayMember = String.IsNullOrEmpty(this.DisplayMember);
            foreach (Object o in this.Items)
            {
                if (o == null)
                    continue;
                String toCheck;
                if (noDisplayMember)
                    toCheck = o.ToString();
                else
                {
                    PropertyInfo pi = o.GetType().GetProperty(this.DisplayMember);
                    Object val = pi == null ? null : pi.GetValue(o, null);
                    toCheck = val == null ? String.Empty : val.ToString();
                }
                if (toCheck.Length <= 0)
                    continue;
                Int32 newWidth = TextRenderer.MeasureText(toCheck, this.Font).Width;
                Int32 newWidth2;
                using (Graphics g = this.CreateGraphics())
                    newWidth2 = g.MeasureString(toCheck, this.Font).ToSize().Width;
                newWidth = Math.Max(newWidth, newWidth2);
                if (this.DrawMode == DrawMode.OwnerDrawFixed)
                    newWidth += 4;
                if (newWidth > widestStringInPixels)
                    widestStringInPixels = newWidth;
            }
            if (hasScrollBar)
                widestStringInPixels += SystemInformation.VerticalScrollBarWidth;
            this.DropDownWidth = widestStringInPixels;
        }
    }
}
