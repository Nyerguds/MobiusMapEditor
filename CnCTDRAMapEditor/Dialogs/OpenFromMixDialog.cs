using MobiusEditor.Model;
using MobiusEditor.Utility;
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

namespace MobiusEditor.Dialogs
{
    public partial class OpenFromMixDialog : Form
    {
        private bool resizing = false;
        private List<Mixfile> openedMixFiles = new List<Mixfile>();
        private int depth = -1;
        private Mixfile baseMix;
        private Dictionary<uint, string> encodedFilenames;
        private List<MixFileInfo> currentMixInfo;

        public OpenFromMixDialog(Mixfile baseMix, Dictionary<uint, string> encodedFilenames)
        {
            InitializeComponent();
            this.encodedFilenames = encodedFilenames;
            this.baseMix = baseMix;
            openedMixFiles.Add(baseMix);
            depth = 0;
            currentMixInfo = MixContentAnalysis.AnalyseFiles(this.baseMix, this.encodedFilenames);
        }

        private Mixfile GetCurrentMix()
        {
            if (openedMixFiles.Count == 0)
            {
                return null;
            }
            if (depth >= openedMixFiles.Count)
            {
                depth = openedMixFiles.Count - 1;
            }
            else if (depth <= 0)
            {
                depth = 0;
            }
            return openedMixFiles[depth];
        }


        private void TeamTypesListView_DoubleClick(object sender, EventArgs e)
        {

        }


        private void OpenFromMixDialog_Load(object sender, EventArgs e)
        {
            //
        }

        private void MixContentsListView_SizeChanged(object sender, EventArgs e)
        {
            if (resizing)
            {
                return;
            }
            resizing = true;
            ListView listView = sender as ListView;
            if (listView == null)
            {
                return;
            }
            float totalColumnWidth = 0;
            int totalAvailablewidth = listView.ClientRectangle.Width;
            int availablewidth = totalAvailablewidth;
            int columns = listView.Columns.Count;
            int[] tagWidths = new int[columns];
            // Get the sum of all column tags
            for (int i = 0; i < columns; ++i)
            {
                int tagWidth;
                if (Int32.TryParse((listView.Columns[i].Tag ?? String.Empty).ToString(), out tagWidth))
                {
                    if (tagWidth > 0)
                    {
                        totalColumnWidth += tagWidth;
                    }
                    else
                    {
                        availablewidth = Math.Max(0, availablewidth - tagWidth);
                    }
                }
            }
            float fraction = availablewidth / totalColumnWidth;
            int[] colWidths = new int[columns];
            int total = 0;
            for (int i = 0; i < columns; ++i)
            {
                int tagwidth = tagWidths[i];
                int actualWidth = tagwidth >= 0 ? (int)(fraction * tagwidth) : -1 * tagwidth;
                colWidths[i] = actualWidth;
                total += actualWidth;
            }
            int diff = totalAvailablewidth - total;
            if (columns > 0)
            {
                while (diff > 0)
                {
                    for (int i = 0; i < columns; ++i)
                    {
                        if (colWidths[i] > 0)
                        {
                            colWidths[i]--;
                            diff--;
                            if (diff == 0)
                            {
                                break;
                            }
                        }
                    }
                }
                while (diff < 0)
                {
                    for (int i = 0; i < columns; ++i)
                    {
                        colWidths[i]++;
                        diff++;
                        if (diff == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        private void OpenFromMixDialog_Shown(object sender, EventArgs e)
        {
            MixContentsListView_SizeChanged(mixContentsListView, EventArgs.Empty);
        }
    }
}