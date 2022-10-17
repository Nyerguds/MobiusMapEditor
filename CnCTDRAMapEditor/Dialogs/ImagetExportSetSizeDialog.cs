using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class ImagetExportSetSizeDialog : Form
    {
        public int Dimension
        {
            get
            {
                int retVal;
                return Int32.TryParse(txtDimension.Text, out retVal) ? retVal : 0;
            }
            set
            {
                txtDimension.Text = value.ToString();
            }
        }

        public ImagetExportSetSizeDialog(Int32 dimension)
        {
            InitializeComponent();
            this.Dimension = dimension;
        }

        private void txtDimension_TextChanged(Object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }
            String pattern = "^\\d*$";
            if (Regex.IsMatch(textBox.Text, pattern))
            {
                return;
            }
            // something snuck in, probably with ctrl+v. Remove it.
            System.Media.SystemSounds.Beep.Play();
            StringBuilder text = new StringBuilder();
            String txt = textBox.Text.ToUpperInvariant();
            Int32 txtLen = txt.Length;
            Int32 firstIllegalChar = -1;
            for (Int32 i = 0; i < txtLen; ++i)
            {
                Char c = txt[i];
                Boolean isNumRange = c >= '0' && c <= '9';
                if (!isNumRange)
                {
                    if (firstIllegalChar == -1)
                        firstIllegalChar = i;
                    continue;
                }
                text.Append(c);
            }
            String filteredText = text.ToString();
            Int32 value;
            NumberStyles ns = NumberStyles.Number | NumberStyles.AllowDecimalPoint;
            // Setting "this.Text" will trigger this function again, but that's okay, it'll immediately succeed in the regex and abort.
            if (Int32.TryParse(filteredText, ns, NumberFormatInfo.CurrentInfo, out value))
            {
                textBox.Text = value.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                textBox.Text = filteredText;
            }
            if (firstIllegalChar == -1)
                firstIllegalChar = 0;
            textBox.Select(firstIllegalChar, 0);
        }
    }
}
