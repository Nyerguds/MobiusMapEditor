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
    public abstract partial class ControlsList<T,TU> : UserControl where T : Control
    {
        protected List<T> m_Contents = new List<T>();
        protected CustomControlInfo<T, TU> m_CustomControlInfo;

        public T[] Contents => m_Contents.ToArray();

        protected ControlsList()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populate the list with controls.
        /// </summary>
        /// <param name="cci">Contains a list of information objects with which to create the custom controls.</param>
        /// <param name="ebc">The controller to assign to the created custom controls.</param>
        public void Populate(CustomControlInfo<T, TU> cci, ListedControlController<TU> ebc)
        {
            if (cci == null)
            {
                this.Reset();
                return;
            }
            // Optimised reset
            int contentsCount = this.m_CustomControlInfo == null ? 0 : this.m_CustomControlInfo.Properties.Length;
            Dictionary<TU, T> mutualControls = new Dictionary<TU, T>();
            this.SuspendLayout();
            if (contentsCount > 0) {

                for (Int32 i = 0; i < contentsCount; ++i)
                {
                    TU prop = this.m_CustomControlInfo.Properties[i];
                    T control = this.m_CustomControlInfo.GetControlByProperty(prop, this.m_Contents);
                    this.Controls.Remove(control);
                    if (cci.Properties.Contains(prop))
                    {
                        mutualControls[prop] = control;
                    }
                    else
                    {
                        control.Dispose();
                    }
                }
                this.m_Contents.Clear();
            }
            this.m_CustomControlInfo = cci;
            this.lblTypeName.Text = cci.Name;
            this.lblTypeName.Visible = !String.IsNullOrEmpty(cci.Name);
            TU[] props = cci.Properties;
            Int32 nrOfProps = props.Length;
            int finalHeight = 0;
            for (Int32 i = 0; i < nrOfProps; ++i)
            {
                try
                {
                    T newControl;
                    if (mutualControls.TryGetValue(props[i], out newControl))
                    {
                        cci.UpdateControl(props[i], ebc, newControl);
                    }
                    else
                    {
                        newControl = cci.MakeControl(props[i], ebc);
                    }
                    finalHeight = this.AddControl(newControl, false);
                }
                catch (NotImplementedException)
                {
                    /* ignore */
                }
            }
            // Only update height at the end.
            this.Size = new Size(this.Size.Width, finalHeight);
            this.PerformLayout();
        }

        public virtual T GetListedControlByInfoObject(TU infoObject)
        {
            if (this.m_CustomControlInfo == null)
                return null;
            return this.m_CustomControlInfo.GetControlByProperty(infoObject, m_Contents);
        }

        /// <summary>
        /// Focus the first listed item.
        /// </summary>
        public void FocusFirst()
        {
            if (this.m_Contents.Count == 0)
                return;
            //this.Select();
            this.FocusItem(this.m_Contents[0]);
        }

        /// <summary>
        /// Focus the item. Can be overridden to focus a specific sub-control on the item.
        /// </summary>
        /// <param name="control">The control to focus.</param>
        protected virtual void FocusItem(T control)
        {
            control.Select();
        }

        protected int AddControl(T control, Boolean refresh)
        {
            if (refresh)
                this.SuspendLayout();
            Int32 ySpacing = this.lblTypeName.Location.Y;
            // Can't count on "lblTypeName.Visible" inside suspended layout.
            Boolean addSpacing = !String.IsNullOrEmpty(lblTypeName.Text);
            Int32 YPos;
            if (this.m_Contents.Count == 0)
                YPos = ySpacing + (addSpacing ? this.lblTypeName.Height + ySpacing : 0);
            else
            {
                T lastControl = this.m_Contents[this.m_Contents.Count - 1];
                YPos = lastControl.Location.Y + lastControl.Size.Height;
            }
            control.Location = new Point(0, YPos);
            this.m_Contents.Add(control);
            this.Controls.Add(control);
            control.TabIndex = this.Controls.Count;
            control.Size = new Size(this.DisplayRectangle.Width, control.Size.Height);
            int newHeight = YPos + control.Size.Height;
            if (refresh)
                this.Size = new Size(this.Size.Width, newHeight);
            control.Visible = true;
            if (refresh)
                this.PerformLayout();
            return newHeight;
        }

        public void Reset()
        {
            this.SuspendLayout();
            this.lblTypeName.Text = String.Empty;
            Int32 contentsCount = this.m_Contents.Count;
            for (Int32 i = 0; i < contentsCount; ++i)
            {
                T c = this.m_Contents[i];
                this.Controls.Remove(c);
                c.Dispose();
            }
            this.m_Contents.Clear();
            this.PerformLayout();
        }

        protected void EffectBarList_Resize(Object sender, EventArgs e)
        {
            this.SuspendLayout();
            Int32 contentsCount = this.m_Contents.Count;
            for (Int32 i = 0; i < contentsCount; ++i)
            {
                T c = this.m_Contents[i];
                c.Size = new Size(this.DisplayRectangle.Width, c.Size.Height);
            }
            this.PerformLayout();
        }
    }
}
