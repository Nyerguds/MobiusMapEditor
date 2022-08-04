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
