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
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public class ComboBoxSmartWidth : ComboBox
    {
        protected override void OnDropDown(EventArgs e)
        {
            SetDropDownWidth(e);
            base.OnDropDown(e);
        }

        private void SetDropDownWidth(EventArgs e)
        {
            int widestStringInPixels = Width;
            bool hasScrollBar = Items.Count * ItemHeight > DropDownHeight;
            if (hasScrollBar)
                widestStringInPixels -= SystemInformation.VerticalScrollBarWidth;
            bool noDisplayMember = String.IsNullOrEmpty(DisplayMember);
            foreach (object o in Items)
            {
                if (o == null)
                    continue;
                string toCheck;
                if (noDisplayMember)
                    toCheck = o.ToString();
                else
                {
                    PropertyInfo pi = o.GetType().GetProperty(DisplayMember);
                    object val = pi == null ? null : pi.GetValue(o, null);
                    toCheck = val == null ? String.Empty : val.ToString();
                }
                if (toCheck.Length <= 0)
                    continue;
                // Neither of these methods are quite correct. Haven't found anything better though.
                int newWidth = TextRenderer.MeasureText(toCheck, Font).Width;
                int newWidth2;
                using (Graphics g = CreateGraphics())
                    newWidth2 = g.MeasureString(toCheck, Font).ToSize().Width;
                newWidth = Math.Max(newWidth, newWidth2);
                if (DrawMode == DrawMode.OwnerDrawFixed)
                    newWidth += 4;
                if (newWidth > widestStringInPixels)
                    widestStringInPixels = newWidth;
            }
            if (hasScrollBar)
                widestStringInPixels += SystemInformation.VerticalScrollBarWidth;
            DropDownWidth = widestStringInPixels;
        }
    }
}
