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
using System.Linq;
using System.Windows.Forms;

namespace MobiusEditor.Controls.ControlsList
{
    /// <summary>
    /// Offers the ability to list user controls, which can send updates of their child controls back to a controller.
    /// </summary>
    /// <typeparam name="T">Type of the user controls with which to populate the list.</typeparam>
    /// <typeparam name="TU">Type of the information objects that contain all information to create/manage a listed control.</typeparam>
    public abstract partial class ControlsList<T,TU, TA, TR> : UserControl where T : Control
    {
        // List of actual controls.
        protected List<T> contents = new List<T>();
        // List of control info. Might be less long than controls list.
        protected CustomControlInfo<T, TU, TA, TR> customControlInfo;

        public T[] Contents => contents.ToArray();

        protected ControlsList()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Populate the list with controls.
        /// </summary>
        /// <param name="cci">Contains a list of information objects with which to create the custom controls.</param>
        /// <param name="ebc">The controller to assign to the created custom controls.</param>
        public void Populate(CustomControlInfo<T, TU, TA, TR> cci, IListedControlController<TU, TA, TR> ebc)
        {
            // Optimised reset
            int oldCount = customControlInfo?.Properties?.Length ?? 0;
            int newCount = cci?.Properties?.Length ?? 0;
            SuspendLayout();
            // Reuse all existing controls, but just reinitialize them.
            // Remove excess:
            if (newCount < contents.Count)
            {
                for (int i = newCount; i < contents.Count; ++i)
                {
                    T control = contents[i];
                    control.Visible = false;
                    // "Enabled" is only changed because unlike "Visible", it is unaffected by layout
                    // operations, meaning it accurately represents the user-set value at all times.
                    control.Enabled = false;
                }
            }
            customControlInfo = cci;
            if (cci == null)
            {
                Size = new Size(Size.Width, GetYPos(true));
                PerformLayout();
                return;
            }
            TU[] props = cci.Properties;
            lblTypeName.Text = cci.Name;
            lblTypeName.Visible = !String.IsNullOrEmpty(cci.Name);
            int finalHeight = 0;
            // Get trimmed height as start point.
            if (oldCount > 0)
            {
                T lastControl = contents[oldCount - 1];
                finalHeight = lastControl.Location.Y + lastControl.Size.Height;
            }
            for (int i = 0; i < newCount; ++i)
            {
                try
                {
                    T newControl;
                    if (i < contents.Count)
                    {
                        newControl = contents[i];
                        newControl.Visible = true;
                        newControl.Enabled = true;
                        finalHeight = newControl.Location.Y + newControl.Size.Height;
                        cci.UpdateControl(props[i], ebc, newControl);
                    }
                    else
                    {
                        newControl = cci.MakeControl(props[i], ebc);
                        finalHeight = AddControl(newControl, false);
                    }
                }
                catch (NotImplementedException)
                {
                    /* ignore */
                }
            }
            // Only update height at the end.
            Size = new Size(Size.Width, finalHeight);
            PerformLayout();
        }

        public virtual T GetListedControlByInfoObject(TU infoObject)
        {
            if (customControlInfo == null)
                return null;
            return customControlInfo.GetControlByProperty(infoObject, contents);
        }

        /// <summary>
        /// Focus the first listed item.
        /// </summary>
        public void FocusFirst()
        {
            if (contents.Count == 0)
                return;
            //this.Select();
            FocusItem(contents[0]);
        }

        /// <summary>
        /// Focus the item. Can be overridden to focus a specific sub-control on the item.
        /// </summary>
        /// <param name="control">The control to focus.</param>
        protected virtual void FocusItem(T control)
        {
            control.Select();
        }

        protected int AddControl(T control, bool refresh)
        {
            if (refresh)
                SuspendLayout();
            int yPos = GetYPos(false);
            control.Location = new Point(0, yPos);
            contents.Add(control);
            Controls.Add(control);
            control.TabIndex = Controls.Count;
            control.Size = new Size(DisplayRectangle.Width, control.Size.Height);
            control.Visible = true;
            control.Enabled = true;
            int newHeight = yPos + control.Size.Height;
            if (refresh)
                Size = new Size(Size.Width, newHeight);
            if (refresh)
                PerformLayout();
            return newHeight;
        }

        /// <summary>
        /// Get the Y position at which the next control would be shown.
        /// This is also used to resize the control to only show visible items.
        /// </summary>
        /// <param name="forActive">True if this is based on activated items rather than on all existing items in <see cref="contents"/>.</param>
        /// <returns></returns>
        private int GetYPos(bool forActive)
        {
            int ySpacing = lblTypeName.Location.Y;
            // Can't count on "lblTypeName.Visible" inside suspended layout.
            bool addSpacing = !String.IsNullOrEmpty(lblTypeName.Text);
            int yPos = ySpacing + (addSpacing ? lblTypeName.Height + ySpacing : 0);
            if (contents.Count > 0 && !forActive)
            {
                T lastControl = contents[contents.Count - 1];
                return lastControl.Location.Y + lastControl.Size.Height;
            }
            for (int i = 0; i < contents.Count; ++i)
            {
                T control = contents[i];
                if (control.Enabled)
                {
                    yPos = Math.Max(yPos, control.Location.Y + control.Height);
                }
            }
            return yPos;
        }

        public void Reset(bool clearItems)
        {
            SuspendLayout();
            lblTypeName.Text = String.Empty;
            int contentsCount = contents.Count;
            for (int i = 0; i < contentsCount; ++i)
            {
                T c = contents[i];
                if (clearItems)
                {
                    Controls.Remove(c);
                    c.Dispose();
                }
                else
                {
                    c.Visible = false;
                    c.Enabled = false;
                }
            }
            if (clearItems)
            {
                contents.Clear();
            }
            Size = new Size(Size.Width, GetYPos(true));
            PerformLayout();
        }

        protected void ControlsList_Resize(object sender, EventArgs e)
        {
            SuspendLayout();
            int contentsCount = contents.Count;
            for (int i = 0; i < contentsCount; ++i)
            {
                T c = contents[i];
                c.Size = new Size(DisplayRectangle.Width, c.Size.Height);
            }
            PerformLayout();
        }
    }
}
