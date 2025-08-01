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
        public int MouseWheelIncrement { get; set; }
        [Category("Action")]
        [Description("Occurs when the value is changed a single tick through either the up-down arrow keys, the up-down buttons or the scrollwheel.")]
        public event EventHandler<UpDownEventArgs> ValueUpDown;
        [Category("Action")]
        [Description("Occurs when the user presses the Enter key after changing the value.")]
        public event EventHandler<ValueEnteredEventArgs> ValueEntered;
        [Category("Data")]
        [Description("True to make the scrollwheel action cause validation on EnteredValue.")]
        [DefaultValue(true)]
        public bool ScrollValidatesEnter { get { return _ScrollValidatesEnter; } set { _ScrollValidatesEnter = value; } }
        [Category("Data")]
        [DefaultValue(true)]
        [Description("True to make the up-down arrow keys or controls cause validation on EnteredValue.")]
        public bool UpDownValidatesEnter { get { return _UpDownValidatesEnter; } set { _UpDownValidatesEnter = value; } }
        [Category("Data")]
        [DefaultValue(true)]
        [Description("True to make focus loss cause validation on EnteredValue.")]
        public bool FocusLossValidatesEnter { get { return _FocusLossValidatesEnter; } set { _FocusLossValidatesEnter = value; } }

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
                        Value = value;
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
        protected bool IsInitalising
        {
            get
            {
                Type numUpDownType = typeof(NumericUpDown);
                FieldInfo init = numUpDownType.GetField("initializing", BindingFlags.Instance | BindingFlags.NonPublic);
                return init != null && (bool)init.GetValue(this);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int IntValue
        {
            get { return (int)Value; }
            set { Value = Constrain(value); }
        }

        /// <summary>Gets or sets the starting point of text selected in the text box.</summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionStart
        {
            get { return _TextBox.SelectionStart; }
            set { _TextBox.SelectionStart = value; }
        }

        /// <summary>Gets or sets the number of characters selected in the text box.</summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionLength
        {
            get { return _TextBox.SelectionLength; }
            set { _TextBox.SelectionLength = value; }
        }

        /// <summary>Gets or sets a value indicating the currently selected text in the control.</summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedText
        {
            get { return _TextBox.SelectedText; }
            set { _TextBox.SelectedText = value; }
        }

        private bool _ScrollValidatesEnter = true;
        private bool _UpDownValidatesEnter = true;
        private bool _FocusLossValidatesEnter = true;
        private readonly TextBox _TextBox;

        public EnhNumericUpDown()
        {
            MouseWheelIncrement = 1;
            KeyDown += CheckKeyPress;
            foreach (Control control in Controls)
            {
                if (control is TextBox)
                {
                    _TextBox = control as TextBox;
                    break;
                }
            }
        }

        public TextBox TextBox { get { return _TextBox; } }

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
            bool allowMinus = Minimum < 0;
            bool allowHex = Hexadecimal;
            string curText = Text;
            string pattern = (allowMinus ? "-?" : String.Empty) + (allowHex ? "[A-F0-9]*" : "\\d*");
            if (Regex.IsMatch(curText, "^" + pattern + "$", RegexOptions.IgnoreCase)) // && !"-".Equals(curText))
                return;
            // something snuck in, probably with ctrl+v. Remove it.
            System.Media.SystemSounds.Beep.Play();
            StringBuilder text = new StringBuilder();
            string txt = curText.ToUpperInvariant();
            int txtLen = txt.Length;
            int firstIllegalChar = -1;
            for (int i = 0; i < txtLen; ++i)
            {
                char c = txt[i];
                bool isNumRange = c >= '0' && c <= '9';
                bool isAllowedHexRange = allowHex && c >= 'A' && c <= 'F';
                bool isAllowedMinus = allowMinus && i == 0 && c == '-';
                if (!isNumRange && !isAllowedHexRange && !isAllowedMinus)
                {
                    if (firstIllegalChar == -1)
                        firstIllegalChar = i;
                    continue;
                }
                text.Append(c);
            }
            string filteredText = text.ToString();
            Decimal value;
            NumberStyles ns = allowHex ? NumberStyles.HexNumber : NumberStyles.Number;
            // Setting "this.Text" will trigger this function again, but that's okay, it'll immediately succeed in the regex and abort.
            if (Decimal.TryParse(filteredText, ns, NumberFormatInfo.CurrentInfo, out value))
            {
                value = Math.Max((int)Minimum, Math.Min(Maximum, value));
                Text = GetNumberText(value);
            }
            else
            {
                Text = filteredText;
            }
            if (firstIllegalChar == -1)
                firstIllegalChar = 0;
            Select(firstIllegalChar, 0);
        }

        public void SelectAll()
        {
            _TextBox.SelectionStart = 0;
            _TextBox.SelectionLength = TextBox.TextLength;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (ReadOnly)
            {
                return;
            }
            Decimal oldval = CurrentInternalValue;
            HandledMouseEventArgs hme = e as HandledMouseEventArgs;
            if (hme != null)
                hme.Handled = true;
            int delta = e.Delta;
            if (delta == 0)
            {
                return;
            }
            int scroll = MouseWheelIncrement;
            if (delta < 0)
            {
                scroll = -scroll;
            }
            // Negative increment is perfectly allowed, but will simply be handled as opposite direction scrolling.
            if (MouseWheelIncrement < 0)
            {
                delta = -delta;
            }
            decimal oldValue = Value;
            decimal newValue = Constrain(oldValue + scroll);
            if (oldValue != newValue)
            {
                Value = newValue;
                UpDownAction action = delta > 0 ? UpDownAction.Up : UpDownAction.Down;
                if (ScrollValidatesEnter)
                    ValidateValue(oldval);
                if (ValueUpDown != null)
                    ValueUpDown(this, new UpDownEventArgs(action, MouseWheelIncrement, true));
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            if (ReadOnly)
            {
                return;
            }
            if (FocusLossValidatesEnter)
            {
                Decimal oldval = CurrentInternalValue;
                ValidateValue(oldval);
            }
        }

        private void CheckKeyPress(object sender, KeyEventArgs e)
        {
            if (ReadOnly)
            {
                return;
            }
            if (e.KeyCode == Keys.Enter)
            {
                Decimal oldval = CurrentInternalValue;
                e.SuppressKeyPress = ValidateValue(oldval);
            }
        }

        private bool ValidateValue(decimal oldValue)
        {
            base.ValidateEditText();
            decimal newVal = Value;
            if (ValueEntered != null)
                ValueEntered(this, new ValueEnteredEventArgs(oldValue, newVal));
            return true;
        }

        public Decimal Constrain(Decimal value)
        {
            if (value > Maximum)
                value = Maximum;
            else if (value < Minimum)
                value = Minimum;
            return value;
        }

        /// <summary>
        /// Decrements the value of the spin box (also known as an up-down control).
        /// </summary>
        public override void DownButton()
        {
            if (ReadOnly)
            {
                return;
            }
            Decimal oldval = CurrentInternalValue;
            base.DownButton();
            if (UpDownValidatesEnter)
                ValidateValue(oldval);
            if (ValueUpDown != null)
                ValueUpDown(this, new UpDownEventArgs(UpDownAction.Down));
        }

        /// <summary>
        /// Increments the value of the spin box (also known as an up-down control).
        /// </summary>
        public override void UpButton()
        {
            if (ReadOnly)
            {
                return;
            }
            Decimal oldval = CurrentInternalValue;
            base.UpButton();
            if (UpDownValidatesEnter)
                ValidateValue(oldval);
            if (ValueUpDown != null)
                ValueUpDown(this, new UpDownEventArgs(UpDownAction.Up));
        }
    }

    public class ValueEnteredEventArgs : EventArgs
    {
        public Decimal Oldvalue { get; set; }
        public Decimal Newvalue { get; set; }

        public ValueEnteredEventArgs(Decimal oldvalue, Decimal newvalue)
        {
            Oldvalue = oldvalue;
            Newvalue = newvalue;
        }
    }

    public class UpDownEventArgs : EventArgs
    {
        public UpDownAction Direction;
        public int Increment;
        public bool FromMouseWheel;

        public UpDownEventArgs(UpDownAction direction)
            : this(direction, 1, false)
        { }

        public UpDownEventArgs(UpDownAction direction, int increment, bool fromMouseWheel)
        {
            Direction = direction;
            Increment = increment;
            FromMouseWheel = fromMouseWheel;
        }
    }

    public enum UpDownAction
    {
        Up,
        Down
    }
}
