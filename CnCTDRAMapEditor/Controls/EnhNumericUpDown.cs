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
        public Boolean FocusLossValidatesEnter { get { return this._FocusLossValidatesEnter; } set { this._FocusLossValidatesEnter = value; } }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public decimal CurrentInternalValue
        {
            get
            {
                FieldInfo val = typeof(NumericUpDown).GetField("currentValue", BindingFlags.Instance | BindingFlags.NonPublic);
                return (decimal)val.GetValue(this);
            }
            set
            {
                // Sets the value without triggering the "OnValueChanged" event.
                if (!IsInitalising)
                {
                    if (value < Minimum || value > Maximum)
                    {
                        // Let the system take care of the 'out of range' exception.
                        this.Value = value;
                    }
                    Type numUpDownType = typeof(NumericUpDown);
                    FieldInfo val = numUpDownType.GetField("currentValue", BindingFlags.Instance | BindingFlags.NonPublic);
                    val.SetValue(this, value);
                    FieldInfo valChanged = numUpDownType.GetField("currentValueChanged", BindingFlags.Instance | BindingFlags.NonPublic);
                    valChanged.SetValue(this, true);
                    UpdateEditText();
                }

            }
        }

        /// <summary>As annoying side effect of the fact NumericUpDown implements ISupportInitialize, this needs to be accessible, but isn't.</summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected Boolean IsInitalising
        {
            get
            {
                Type numUpDownType = typeof(NumericUpDown);
                FieldInfo init = numUpDownType.GetField("initializing", BindingFlags.Instance | BindingFlags.NonPublic);
                return init != null && (Boolean)init.GetValue(this);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Int32 IntValue
        {
            get { return (Int32)this.Value; }
            set { this.Value = this.Constrain(value); }
        }

        /// <summary>Gets or sets the starting point of text selected in the text box.</summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Int32 SelectionStart
        {
            get { return this._TextBox.SelectionStart; }
            set { this._TextBox.SelectionStart = value; }
        }

        /// <summary>Gets or sets the number of characters selected in the text box.</summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Int32 SelectionLength
        {
            get { return this._TextBox.SelectionLength; }
            set { this._TextBox.SelectionLength = value; }
        }

        /// <summary>Gets or sets a value indicating the currently selected text in the control.</summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public String SelectedText
        {
            get { return this._TextBox.SelectedText; }
            set { this._TextBox.SelectedText = value; }
        }

        private Boolean _ScrollValidatesEnter = true;
        private Boolean _UpDownValidatesEnter = true;
        private Boolean _FocusLossValidatesEnter = true;
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

        // Private function extracted from base
        protected string GetNumberText(decimal num)
        {
            if (Hexadecimal)
            {
                return ((long)num).ToString("X", CultureInfo.InvariantCulture);
            }
            return num.ToString((ThousandsSeparator ? "N" : "F") + DecimalPlaces.ToString(CultureInfo.CurrentCulture), CultureInfo.CurrentCulture);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (IsInitalising)
            {
                return;
            }
            Boolean allowMinus = this.Minimum < 0;
            Boolean allowHex = this.Hexadecimal;
            String curText = this.Text;
            String pattern = (allowMinus ? "-?" : String.Empty) + (allowHex ? "[A-F0-9]*" : "\\d*");
            if (Regex.IsMatch(curText, "^" + pattern + "$", RegexOptions.IgnoreCase)) // && !"-".Equals(curText))
                return;
            // something snuck in, probably with ctrl+v. Remove it.
            System.Media.SystemSounds.Beep.Play();
            StringBuilder text = new StringBuilder();
            String txt = curText.ToUpperInvariant();
            Int32 txtLen = txt.Length;
            Int32 firstIllegalChar = -1;
            for (Int32 i = 0; i < txtLen; ++i)
            {
                Char c = txt[i];
                Boolean isNumRange = c >= '0' && c <= '9';
                Boolean isAllowedHexRange = allowHex && c >= 'A' && c <= 'F';
                Boolean isAllowedMinus = allowMinus && i == 0 && c == '-';
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
                this.Text = GetNumberText(value);
            }
            else
            {
                this.Text = filteredText;
            }
            if (firstIllegalChar == -1)
                firstIllegalChar = 0;
            this.Select(firstIllegalChar, 0);
        }

        public void SelectAll()
        {
            this._TextBox.SelectionStart = 0;
            this._TextBox.SelectionLength = this.TextBox.TextLength;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (this.ReadOnly)
            {
                return;
            }
            Decimal oldval = this.CurrentInternalValue;
            HandledMouseEventArgs hme = e as HandledMouseEventArgs;
            if (hme != null)
                hme.Handled = true;
            Int32 delta = e.Delta;
            if (delta == 0)
            {
                return;
            }
            Int32 scroll = this.MouseWheelIncrement;
            if (delta < 0)
            {
                scroll = -scroll;
            }
            // Negative increment is perfectly allowed, but will simply be handled as opposite direction scrolling.
            if (this.MouseWheelIncrement < 0)
            {
                delta = -delta;
            }
            decimal oldValue = this.Value;
            decimal newValue = this.Constrain(oldValue + scroll);
            if (oldValue != newValue)
            {
                this.Value = newValue;
                UpDownAction action = delta > 0 ? UpDownAction.Up : UpDownAction.Down;
                if (this.ScrollValidatesEnter)
                    this.ValidateValue(oldval);
                if (this.ValueUpDown != null)
                    this.ValueUpDown(this, new UpDownEventArgs(action, this.MouseWheelIncrement, true));
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            if (this.ReadOnly)
            {
                return;
            }
            if (this.FocusLossValidatesEnter)
            {
                Decimal oldval = this.CurrentInternalValue;
                this.ValidateValue(oldval);
            }
        }

        private void CheckKeyPress(Object sender, KeyEventArgs e)
        {
            if (this.ReadOnly)
            {
                return;
            }
            if (e.KeyCode == Keys.Enter)
            {
                Decimal oldval = this.CurrentInternalValue;
                e.SuppressKeyPress = this.ValidateValue(oldval);
            }
        }

        private Boolean ValidateValue(decimal oldValue)
        {
            base.ValidateEditText();
            decimal newVal = this.Value;
            if (this.ValueEntered != null)
                this.ValueEntered(this, new ValueEnteredEventArgs(oldValue, newVal));
            return true;
        }

        public Decimal Constrain(Decimal value)
        {
            if (value > this.Maximum)
                value = this.Maximum;
            else if (value < this.Minimum)
                value = this.Minimum;
            return value;
        }

        /// <summary>
        /// Decrements the value of the spin box (also known as an up-down control).
        /// </summary>
        public override void DownButton()
        {
            if (this.ReadOnly)
            {
                return;
            }
            Decimal oldval = this.CurrentInternalValue;
            base.DownButton();
            if (this.UpDownValidatesEnter)
                this.ValidateValue(oldval);
            if (this.ValueUpDown != null)
                this.ValueUpDown(this, new UpDownEventArgs(UpDownAction.Down));
        }

        /// <summary>
        /// Increments the value of the spin box (also known as an up-down control).
        /// </summary>
        public override void UpButton()
        {
            if (this.ReadOnly)
            {
                return;
            }
            Decimal oldval = this.CurrentInternalValue;
            base.UpButton();
            if (this.UpDownValidatesEnter)
                this.ValidateValue(oldval);
            if (this.ValueUpDown != null)
                this.ValueUpDown(this, new UpDownEventArgs(UpDownAction.Up));
        }
    }

    public class ValueEnteredEventArgs : EventArgs
    {
        public Decimal Oldvalue { get; set; }
        public Decimal Newvalue { get; set; }

        public ValueEnteredEventArgs(Decimal oldvalue, Decimal newvalue)
        {
            this.Oldvalue = oldvalue;
            this.Newvalue = newvalue;
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
