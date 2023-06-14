using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MobiusEditor.Model;

namespace MobiusEditor.Interface
{
    public interface IArchiveManager : IEnumerable<string>, IEnumerable, IDisposable
    {
        String LoadRoot { get; }

        bool FileExists(string path);
        Stream OpenFile(string path);
        void Reset(GameType gameType, TheaterType theater);
        string[] ExpandModPaths { get; set; }
    }
}
