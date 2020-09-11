using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MobiusEditor
{
    public partial class GameInstallationPathForm : Form
    {
        private OpenFileDialog fileDialog;
        public string SelectedPath => textBox1.Text;

        public GameInstallationPathForm()
        {
            InitializeComponent();
            fileDialog = new OpenFileDialog();
            fileDialog.Filter = "C&C Remastered Executable (ClientG.exe)|ClientG.exe";
            fileDialog.Title = "Select C&C Remastered Executable (ClientG.exe)";
            fileDialog.CheckPathExists = true;
            fileDialog.InitialDirectory = Environment.CurrentDirectory;

            textBox1.Text = Environment.CurrentDirectory;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = fileDialog.FileName;
            }
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            if (!File.Exists(Path.Combine(Path.GetDirectoryName(textBox1.Text), "DATA", "CONFIG.MEG")))
            {
                MessageBox.Show("Required data is missing, please enter the valid " +
                    "installation path for the C&C Remastered Collection. The " +
                    "installation directory is where the main executables of the " +
                    "collection (ClientG.exe and ClientLauncherG.exe) reside.", "Invalid directory");
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }
    }
}
