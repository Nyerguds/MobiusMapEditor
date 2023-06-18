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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MobiusEditor.Dialogs
{
    public partial class ImageExportedDialog : Form
    {
        public ImageExportedDialog(string exportedFileName)
        {
            InitializeComponent();
            this.textBox1.Text = exportedFileName;
        }

        private void BtnGoToFile_Click(Object sender, EventArgs e)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start("explorer.exe", "/select,\"" + textBox1.Text + "\"");
            }
            else
            {
                Process.Start(new ProcessStartInfo(Path.GetDirectoryName(textBox1.Text)) { UseShellExecute = true });
            }
        }
    }
}
