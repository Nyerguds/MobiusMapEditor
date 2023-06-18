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
namespace MobiusEditor.Controls.ControlsList
{
    /// <summary>
    /// This interface should be implemented by an object that can manage the listed info objects. It will get updates from the listed objects to adjust its versions of these objects.
    /// </summary>
    /// <typeparam name="TU">The type of the info object. Can be chosen freely.</typeparam>
    public interface ListedControlController<in TU>
    {
        /// <summary>
        /// To sends an update from the UI back to the controller, and adjust the values kept there.
        /// A call to this should be linked to listeners on the controls.
        /// </summary>
        /// <param name="updateInfo">The info object adjusted to the new UI data.</param>
        void UpdateControlInfo(TU updateInfo);
    }
}
