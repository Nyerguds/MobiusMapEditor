using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
