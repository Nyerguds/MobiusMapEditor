// The Enhanced NumericUpDown is created by Nyerguds, and released under the WTFPL.
// So go nuts. Use it, steal it, sell it, print it out and burn it in bizarre rituals while dancing naked under the moonlight.
// I don't judge.
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    /// <summary>
    /// Enhanced NumericUpDown that allows catching the specific "value up/down" and "value entered" events
    /// instead of "value changed", to avoid unnecessary calls on boxes where values are often typed in.
    /// Also offers a property to change the amount of items scrolled by the mouse scroll wheel.
    /// </summary>
    public class EnhNumericUpDown : NumericUpDown
    {
        [DefaultValue(1)]
        [Category("Data")]
        [Description("Indicates the amount to increment or decrement on mouse wheel scroll.")]
        public Int32 MouseWheelIncrement { get; set; }
        [Category("Action")]
        [Description("Occurs when the value is changed a single tick through either the up-down arrow keys, the up-down buttons or the scrollwheel.")]
        public event EventHandler<UpDownEventArgs> ValueUpDown;
        [Category("Action")]
        [Description("Occurs when the user presses the Enter key after changing the value.")]
        public event EventHandler<ValueEnteredEventArgs> ValueEntered;
        [Category("Data")]
        [Description("True to make the scrollwheel action cause validation on EnteredValue.")]
        [DefaultValue(true)]
        public Boolean ScrollValidatesEnter { get { return this._ScrollValidatesEnter; } set { this._ScrollValidatesEnter = value; } }
        [Category("Data")]
        [DefaultValue(true)]
        [Description("True to make the up-down arrow keys or controls cause validation on EnteredValue.")]
        public Boolean UpDownValidatesEnter { get { return this._UpDownValidatesEnter; } set { this._UpDownValidatesEnter = value; } }
        [Category("Data")]
        [DefaultValue(true)]
        [Description("True to make focus loss cause validation on EnteredValue.")]
        public Boolean FocusLossValidatesEnter { get { return this._UpDownValidatesEnter; } set { this._UpDownValidatesEnter = value; } }

        /// <summary>
        /// Last validated entered value.
        /// </summary>
        [Category("Data")]
        [DefaultValue(0)]
        [Description("The last validated value of the EnhNumericUpDownControl.")]
        public Decimal EnteredValue
        {
            get { return this.Constrain(this._EnteredValue);  }
            set
            {
                this.Value = this.Constrain(value);
                this.ValidateValue();
            }
        }

        public Int32 IntValue
        {
            get { return (Int32)this.Value; }
            set
            {
                this.Value = this.Constrain(value);
            }
        }

        private Decimal _EnteredValue;
        private Boolean _ScrollValidatesEnter = true;
        private Boolean _UpDownValidatesEnter = true;
        private TextBox _TextBox;

        public EnhNumericUpDown()
        {
            this.MouseWheelIncrement = 1;
            this.KeyDown += this.CheckKeyPress;
            foreach (Control control in this.Controls)
            {
                if (control is TextBox)
                {
                    this._TextBox = control as TextBox;
                    break;
                }
            }
        }

        public TextBox TextBox { get { return this._TextBox; } }

        protected override void OnTextChanged(EventArgs e)
        {
            Boolean allowminus = this.Minimum < 0;
            Boolean allowHex = this.Hexadecimal;
            String pattern = (allowminus ? "-?" : String.Empty) + (allowHex ? "[A-F0-9]*" : "\\d*");
            if (Regex.IsMatch(this.Text, "^" + pattern + "$", RegexOptions.IgnoreCase) && !"-".Equals(this.Text))
                return;
            // something snuck in, probably with ctrl+v. Remove it.
            System.Media.SystemSounds.Beep.Play();
            StringBuilder text = new StringBuilder();
            String txt = this.Text.ToUpperInvariant();
            Int32 txtLen = txt.Length;
            Int32 firstIllegalChar = -1;
            for (Int32 i = 0; i < txtLen; ++i)
            {
                Char c = txt[i];
                Boolean isNumRange = (c >= '0' && c <= '9');
                Boolean isAllowedHexRange = allowHex && (c >= 'A' && c <= 'F');
                Boolean isAllowedMinus = (i == 0 && c == '-');
                if (!isNumRange && !isAllowedHexRange && !isAllowedMinus)
                {
                    if (firstIllegalChar == -1)
                        firstIllegalChar = i;
                    continue;
                }
                text.Append(c);
            }
            String filteredText = text.ToString();
            Decimal value;
            NumberStyles ns = allowHex ? NumberStyles.HexNumber : NumberStyles.Number;
            // Setting "this.Text" will trigger this function again, but that's okay, it'll immediately succeed in the regex and abort.
            if (Decimal.TryParse(filteredText, ns, NumberFormatInfo.CurrentInfo, out value))
            {
                value = Math.Max((Int32)this.Minimum, Math.Min(this.Maximum, value));
                this.Text = value.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                this.Text = filteredText;
            }
            if (firstIllegalChar == -1)
                firstIllegalChar = 0;
            this.Select(firstIllegalChar, 0);
        }

        /// <summary>Gets or sets the starting point of text selected in the text box.</summary>
        public Int32 SelectionStart
        {
            get { return this._TextBox.SelectionStart; }
            set { this._TextBox.SelectionStart = value; }
        }

        /// <summary>Gets or sets the number of characters selected in the text box.</summary>
        public Int32 SelectionLength
        {
            get { return this._TextBox.SelectionLength; }
            set { this._TextBox.SelectionLength = value; }
        }

        /// <summary>Gets or sets a value indicating the currently selected text in the control.</summary>
        public String SelectedText
        {
            get { return this._TextBox.SelectedText; }
            set { this._TextBox.SelectedText = value; }
        }

        public void SelectAll()
        {
            this._TextBox.SelectionStart = 0;
            this._TextBox.SelectionLength = this.TextBox.TextLength;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            HandledMouseEventArgs hme = e as HandledMouseEventArgs;
            if (hme != null)
                hme.Handled = true;
            Int32 delta = e.Delta;
            Int32 scroll = this.MouseWheelIncrement;
            // Negative increment is perfectly allowed, but will simply be handled as opposite direction scrolling.
            if (scroll < 0)
            {
                delta = -delta;
                scroll = -scroll;
            }
            UpDownAction action;
            if (delta > 0)
            {
                Decimal value = this.Value + scroll;
                this.Value = Math.Min(this.Maximum, value);
                action = UpDownAction.Up;
            }
            else if (delta < 0)
            {
                Decimal value = this.Value - scroll;
                this.Value = Math.Max(this.Minimum, value);
                action = UpDownAction.Down;
            }
            else
                return;
            if (this.ScrollValidatesEnter)
                this.ValidateValue();
            if (this.ValueUpDown != null)
                this.ValueUpDown(this, new UpDownEventArgs(action, scroll, true));
        }

        protected override void OnLostFocus(EventArgs e)
        {
            if (this.FocusLossValidatesEnter)
            {
                this.Text = this.Text;
                this.ValidateValue();
            }
        }

        private void CheckKeyPress(Object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = this.ValidateValue();
            }
        }

        private Boolean ValidateValue()
        {
            Decimal oldval = this._EnteredValue;
            this._EnteredValue = this.Value;
            if (this.ValueEntered != null)
                this.ValueEntered(this, new ValueEnteredEventArgs(oldval));
            return true;
        }

        public Decimal Constrain(Decimal value)
        {
            if (value < this.Minimum)
                value = this.Minimum;
            else if (value > this.Maximum)
                value = this.Maximum;
            return value;
        }

        /// <summary>
        /// Decrements the value of the spin box (also known as an up-down control).
        /// </summary>
        public override void DownButton()
        {
            base.DownButton();
            //Decimal value = this.Value;
            //this.Value = Math.Max(this.Minimum, value);
            if (this.UpDownValidatesEnter)
                this.ValidateValue();
            if (this.ValueUpDown != null)
                this.ValueUpDown(this, new UpDownEventArgs(UpDownAction.Down));
        }

        /// <summary>
        /// Increments the value of the spin box (also known as an up-down control).
        /// </summary>
        public override void UpButton()
        {
            base.UpButton();
            //Decimal value = this.Value;
            //this.Value = Math.Min(this.Maximum, value);
            if (this.UpDownValidatesEnter)
                this.ValidateValue();
            if (this.ValueUpDown != null)
                this.ValueUpDown(this, new UpDownEventArgs(UpDownAction.Up));
        }

        // Sets the value without triggering the "OnValueChanged" event.
        protected void SetInternalValue(Int32 value)
        {
            Type numUpDownType = this.GetType();
            FieldInfo init = numUpDownType.GetField("initializing");
            Boolean initializing = (Boolean)init.GetValue(this);

            if (!initializing && ((value < Minimum) || (value > Maximum)))
            {
                // Let the system take care of the 'out of range' exception.
                this.Value = value;
            }
            else
            {
                FieldInfo val = numUpDownType.GetField("currentValue");
                val.SetValue(this, value);
                FieldInfo valChanged = numUpDownType.GetField("currentValueChanged");
                valChanged.SetValue(this, true);
                UpdateEditText();
            }
        }
    }

    public class ValueEnteredEventArgs : EventArgs
    {
        public Decimal Oldvalue;

        public ValueEnteredEventArgs(Decimal oldvalue)
        {
            this.Oldvalue = oldvalue;
        }
    }

    public class UpDownEventArgs : EventArgs
    {
        public UpDownAction Direction;
        public Int32 Increment;
        public Boolean FromMouseWheel;

        public UpDownEventArgs(UpDownAction direction)
            : this(direction, 1, false)
        { }

        public UpDownEventArgs(UpDownAction direction, Int32 increment, Boolean fromMouseWheel)
        {
            this.Direction = direction;
            this.Increment = increment;
            this.FromMouseWheel = fromMouseWheel;
        }
    }

    public enum UpDownAction
    {
        Up,
        Down
    }
}
