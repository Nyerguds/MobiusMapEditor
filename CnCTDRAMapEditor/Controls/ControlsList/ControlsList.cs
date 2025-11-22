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
        protected List<T> contents = new List<T>();
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
            if (cci == null)
            {
                Reset();
                return;
            }
            // Optimised reset
            TU[] props = cci.Properties;
            int oldCount = customControlInfo == null ? 0 : customControlInfo.Properties.Length;
            int newCount = props.Length;
            SuspendLayout();
            // Reuse all existing controls, but just reinitialize them.
            // Remove excess:
            if (newCount < oldCount)
            {
                List<T> removed = new List<T>();
                for (int i = newCount; i < oldCount; ++i)
                {
                    removed.Add(contents[i]);
                }
                for (int i = 0; i < removed.Count; ++i)
                {
                    T control = removed[i];
                    contents.Remove(control);
                    Controls.Remove(control);
                    control.Dispose();
                }
            }
            customControlInfo = cci;
            lblTypeName.Text = cci.Name;
            lblTypeName.Visible = !String.IsNullOrEmpty(cci.Name);
            int finalHeight = 0;
            // Get trimmed height as start point.
            if (contents.Count > 0)
            {
                T lastControl = contents[contents.Count - 1];
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
            int ySpacing = lblTypeName.Location.Y;
            // Can't count on "lblTypeName.Visible" inside suspended layout.
            bool addSpacing = !String.IsNullOrEmpty(lblTypeName.Text);
            int yPos;
            if (contents.Count == 0)
                yPos = ySpacing + (addSpacing ? lblTypeName.Height + ySpacing : 0);
            else
            {
                T lastControl = contents[contents.Count - 1];
                yPos = lastControl.Location.Y + lastControl.Size.Height;
            }
            control.Location = new Point(0, yPos);
            contents.Add(control);
            Controls.Add(control);
            control.TabIndex = Controls.Count;
            control.Size = new Size(DisplayRectangle.Width, control.Size.Height);
            int newHeight = yPos + control.Size.Height;
            if (refresh)
                Size = new Size(Size.Width, newHeight);
            control.Visible = true;
            if (refresh)
                PerformLayout();
            return newHeight;
        }

        public void Reset()
        {
            SuspendLayout();
            lblTypeName.Text = String.Empty;
            int contentsCount = contents.Count;
            for (int i = 0; i < contentsCount; ++i)
            {
                T c = contents[i];
                Controls.Remove(c);
                c.Dispose();
            }
            contents.Clear();
            PerformLayout();
        }

        protected void EffectBarList_Resize(object sender, EventArgs e)
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
