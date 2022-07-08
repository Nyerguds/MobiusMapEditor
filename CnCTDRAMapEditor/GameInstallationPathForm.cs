using System;
using System.IO;
using System.Security;
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
            String dir = textBox1.Text;
            Boolean checkPassed = false;
            try
            {
                if ((new FileInfo(dir).Attributes & FileAttributes.Directory) != 0)
                    dir = dir.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                String fileToCheck = Path.Combine(Path.GetDirectoryName(dir), "DATA", "CONFIG.MEG");
                checkPassed = File.Exists(fileToCheck);
            }
            catch (SecurityException) { /* Check not passed */}
            catch (ArgumentException) { /* Check not passed */}
            catch (UnauthorizedAccessException) { /* Check not passed */}
            catch (PathTooLongException) { /* Check not passed */}
            catch (NotSupportedException) { /* Check not passed */}
            if (!checkPassed)
            {
                MessageBox.Show("Required data is missing, please enter the valid " +
                    "installation path for the C&C Remastered Collection. The " +
                    "installation directory is where the main executables of the " +
                    "collection (ClientG.exe and ClientLauncherG.exe) reside.", "Invalid directory");
                return;
            }
            textBox1.Text = dir;
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
