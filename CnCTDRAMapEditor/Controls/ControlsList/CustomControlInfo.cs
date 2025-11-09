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
        public string Name { get; set; }
        public TU[] Properties { get; set; }

        public abstract T MakeControl(TU property, ListedControlController<TU> controller);
        public abstract void UpdateControl(TU property, ListedControlController<TU> controller, T control);
        public abstract T GetControlByProperty(TU property, IEnumerable<T> controls);

        public override string ToString()
        {
            return this.Name;
        }
    }
}
