using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiusEditor.Interface
{
    public interface IGameTextManager
    {
        String this[string key] { get; set; }
        void Reset(GameType gameType);
    }
}
