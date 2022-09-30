using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace MobiusEditor.Utility
{
    public interface IHasStatusLabel
    {
        Label StatusLabel { get; set; }
    }
    
    // For easy access
    public static class SimpleMultiThreading
    {
        public static SimpleMultiThreading<T> Make<T>(T attachForm) where T : Form, IHasStatusLabel
        {
            return new SimpleMultiThreading<T>(attachForm);
        }
    }

    /// <summary>
    /// Simple multithreading for heavy operations to not freeze the UI. This just needs a form with a public
    /// property to get and set a "busy" state label, and the type that is produced by the heavy operation.
    /// </summary>
    /// <typeparam name="T">Form to attach the label to.</typeparam>
    /// <typeparam name="U">The type produced by the heavy operation. If the operation returns the type default, then it is treated as failed, and a message box is shown.</typeparam>
    public class SimpleMultiThreading<T> where T: Form, IHasStatusLabel
    {
        public delegate void InvokeDelegateEnableControls(Boolean enabled, String processingLabel);
        public delegate DialogResult InvokeDelegateMessageBox(String message, MessageBoxButtons buttons, MessageBoxIcon icon);
        public delegate void InvokeDelegateResult<U>(U resultObject);
        private Thread m_ProcessingThread;
        private T attachForm;

        public SimpleMultiThreading(T attachForm)
        {
            this.attachForm = attachForm;
        }

        /// <summary>
        /// Executes a threaded operation while locking the UI. 
        /// </summary>
        /// <param name="function">The heavy processing function to run on a different thread.
        /// <param name="resultFunction">Optional function to call after <paramref name="function"/> returns a non-null result.</param>
        /// <paramref name="resultFuncIsInvoked"/>true if the result function is Invoked on the main form.</param>
        /// <param name="enableFunction">Function to enable/disable UI controls. This should also include a call to <see cref="CreateBusyLabel"/> to create the busy status label. This function is Invoked on the main form.</param>
        /// <param name="operationType">Label to show while the operation is busy. This will be passed on as arg to <paramref name="enableFunction"/>.</param>
        public void ExecuteThreaded<U>(Func<U> function, Action<U> resultFunction, bool resultFuncIsInvoked, Action<bool, string> enableFunction, String operationType)
        {
            if (this.m_ProcessingThread != null && this.m_ProcessingThread.IsAlive)
                return;
            //Arguments: func returning SupportedFileType, reset palettes, reset index, reset auto-zoom, process type indication string.
            Object[] arrParams = { function, resultFunction, resultFuncIsInvoked, enableFunction, operationType };
            this.m_ProcessingThread = new Thread(this.ExecuteThreadedActual<U>);
            this.m_ProcessingThread.Start(arrParams);
        }

        /// <summary>
        /// Executes a threaded operation while locking the UI.
        /// "parameters" must be an array of Object containing 4 items:
        /// a <see cref="Func{TResult}"/> to execute, returning <see cref="U"/>,
        /// an <see cref="Action"/> taking a parameter of type <see cref="U"/> to execute after successful processing (optional, can be null),
        /// an <see cref="Action"/> to enable form controls, taking a parameter of type <see cref="bool"/> (whether to enable or disable controls) and <see cref="string"/> (message to show on disabled UI),
        /// a <see cref="string"/> to indicate the process type being executed (eg. "Saving").
        /// </summary>
        /// <param name="parameters">
        ///     Array of Object, containing 4 items: a <see cref="Func{TResult}"/> to execute, returning <see cref="U"/>,
        ///     an <see cref="Action"/> taking a parameter of type <see cref="U"/> to execute after successful processing (optional, can be null),
        ///     an <see cref="Action"/> to enable form controls, taking a parameter of type <see cref="bool"/> (whether to enable or disable controls) and <see cref="string"/> (message to show on disabled UI),
        ///     and a <see cref="string"/> to indicate the process type being executed (eg. "Saving").
        /// </param>
        private void ExecuteThreadedActual<U>(Object parameters)
        {
            Object[] arrParams = parameters as Object[];
            Func<U> func;
            Action<U> resAct;
            Action<bool, string> enableControls;
            if (arrParams == null || arrParams.Length < 5
                || ((func = arrParams[0] as Func<U>) == null)
                || ((resAct = arrParams[1] as Action<U>) == null && arrParams[1] != null)
                || !(arrParams[2] is bool)
                || ((enableControls = arrParams[3] as Action<bool, string>) == null))
            {
                return;
            }
            bool resActIsInvoked = (bool)arrParams[2];
            String operationType = arrParams[4] as String;
            this.attachForm.Invoke(new InvokeDelegateEnableControls(enableControls), false, operationType);
            operationType = String.IsNullOrEmpty(operationType) ? "Operation" : operationType.Trim();
            U result = default(U);
            try
            {
                // Processing code.
                result = func();
            }
            catch (ThreadAbortException)
            {
                // Ignore. Thread is aborted.
            }
            catch (Exception ex)
            {
                String message = operationType + " failed:\n" + ex.Message + "\n" + ex.StackTrace;
                this.attachForm.Invoke(new InvokeDelegateMessageBox(this.ShowMessageBox), message, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.attachForm.Invoke(new InvokeDelegateEnableControls(enableControls), true, null);
            }
            try
            {
                this.attachForm.Invoke(new InvokeDelegateEnableControls(enableControls), true, null);
                if (resAct != null && !EqualityComparer<U>.Default.Equals(result, default(U)))
                {
                    if (resActIsInvoked)
                    {
                        this.attachForm.Invoke(new InvokeDelegateResult<U>(resAct), result);
                    }
                    else
                    {
                        resAct(result);
                    }
                }
            }
            catch (InvalidOperationException) { /* ignore */ }
        }

        private DialogResult ShowMessageBox(String message, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            if (message == null)
                return DialogResult.Cancel;
            bool allowedDrop = attachForm.AllowDrop;
            attachForm.AllowDrop = false;
            DialogResult result = MessageBox.Show(attachForm, message, attachForm.Text, buttons, icon);
            attachForm.AllowDrop = allowedDrop;
            return result;
        }

        /// <summary>
        /// Can be used to create a "busy" label on the UI while a heavy operation is running in a different thread.
        /// This should be called from the "enableFunction" when calling <see cref="ExecuteThreaded"/>.
        /// </summary>
        /// <param name="processingLabel">Processing label. Set to null to remove the label.</param>
        public void CreateBusyLabel(string processingLabel)
        {
            // Remove old busy status label if it exists.
            RemoveBusyLabel();
            if (processingLabel == null)
            {
                return;
            }
            // Create busy status label.
            Label busyStatusLabel = new Label();
            busyStatusLabel.Text = (String.IsNullOrEmpty(processingLabel) ? "Processing" : processingLabel) + "...";
            busyStatusLabel.TextAlign = ContentAlignment.MiddleCenter;
            busyStatusLabel.Font = new Font(busyStatusLabel.Font.FontFamily, 15F, FontStyle.Regular, GraphicsUnit.Pixel, 0);
            busyStatusLabel.AutoSize = false;
            busyStatusLabel.Size = new Size(300, 100);
            busyStatusLabel.Anchor = AnchorStyles.None; // Always floating in the middle, even on resize.
            busyStatusLabel.BorderStyle = BorderStyle.FixedSingle;
            Int32 x = (attachForm.ClientRectangle.Width - 300) / 2;
            Int32 y = (attachForm.ClientRectangle.Height - 100) / 2;
            busyStatusLabel.Location = new Point(x, y);
            attachForm.Controls.Add(busyStatusLabel);
            attachForm.StatusLabel = busyStatusLabel;
            busyStatusLabel.Visible = true;
            busyStatusLabel.BringToFront();
        }

        public void RemoveBusyLabel()
        {
            Label busyStatusLabel = attachForm.StatusLabel;
            if (busyStatusLabel == null)
                return;
            attachForm.Controls.Remove(busyStatusLabel);
            try { busyStatusLabel.Dispose(); }
            catch { /* ignore */ }
            attachForm.StatusLabel = null;
        }

    }
}
