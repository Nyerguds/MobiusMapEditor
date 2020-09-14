using MobiusEditor.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiusEditor.Tools.Dialogs
{
    public interface IToolDialog : IDisposable
    {
        ITool GetTool();
        void SetTool(ITool value);
    }
}
