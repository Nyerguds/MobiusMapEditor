using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MobiusEditor.Interface
{
    public interface IArchiveManager : IEnumerable<string>, IEnumerable, IDisposable
    {
        String LoadRoot { get; }

        bool LoadArchive(string archivePath);
        bool FileExists(string path);
        Stream OpenFile(string path);
        void Reset(GameType gameType);
    }
}
