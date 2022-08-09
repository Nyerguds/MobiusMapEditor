using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MobiusEditor.Controls.ControlsList
{
    /// <summary>
    /// Information for a custom control managed by an information object.
    /// </summary>
    /// <typeparam name="T">Type of the user controls with which to populate the list.</typeparam>
    /// <typeparam name="TU">Type of the information objects that contain all information to create/manage a listed control.</typeparam>
    public abstract class CustomControlInfo<T, TU> where T : Control
    {
        public String Name { get; set; }
        public TU[] Properties { get; set; }

        public abstract T MakeControl(TU property, ListedControlController<TU> controller);
        public abstract void UpdateControl(TU property, ListedControlController<TU> controller, T control);
        public abstract T GetControlByProperty(TU property, IEnumerable<T> controls);

        public override String ToString()
        {
            return this.Name;
        }
    }
}
