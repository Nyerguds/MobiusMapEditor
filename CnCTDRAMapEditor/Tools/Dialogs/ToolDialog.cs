using MobiusEditor.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MobiusEditor.Tools.Dialogs
{
    public abstract class ToolDialog<T> : Form, IToolDialog where T : ITool
    {
        public T Tool { get; set; }
        public ITool GetTool() => Tool;
        public void SetTool(ITool value) => Tool = (T)value;

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Tool.Activate();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Tool.Deactivate();
        }
    }
}
