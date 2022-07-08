using MobiusEditor.Interface;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace MobiusEditor.Controls
{
    public class ViewToolStripButton : ToolStripButton
    {
        [Category("Behavior")]
        public ToolType ToolType { get; set; }

    }
}
