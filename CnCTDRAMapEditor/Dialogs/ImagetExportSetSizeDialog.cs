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

        public ImagetExportSetSizeDialog(int dimension)
        {
            InitializeComponent();
            this.Dimension = dimension;
        }

        private void txtDimension_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }
            string pattern = "^\\d*$";
            if (Regex.IsMatch(textBox.Text, pattern))
            {
                return;
            }
            // something snuck in, probably with ctrl+v. Remove it.
            System.Media.SystemSounds.Beep.Play();
            StringBuilder text = new StringBuilder();
            string txt = textBox.Text.ToUpperInvariant();
            int txtLen = txt.Length;
            int firstIllegalChar = -1;
            for (int i = 0; i < txtLen; ++i)
            {
                char c = txt[i];
                bool isNumRange = c >= '0' && c <= '9';
                if (!isNumRange)
                {
                    if (firstIllegalChar == -1)
                        firstIllegalChar = i;
                    continue;
                }
                text.Append(c);
            }
            string filteredText = text.ToString();
            int value;
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
