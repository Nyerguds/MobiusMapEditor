//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// The Command & Conquer Map Editor and corresponding source code is distributed
// in the hope that it will be useful, but with permitted additional restrictions
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT
// distributed with this program. You should have received a copy of the
// GNU General Public License along with permitted additional restrictions
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MobiusEditor.Interface;
using MobiusEditor.Model;

namespace MobiusEditor.Utility
{
    public class MegafileManager : IArchiveManager
    {
        private Dictionary<GameType, string[]> modPathsPerGame;
        private readonly string looseFilePath;
        private MixfileManager mixFm;
        public MixfileManager ClassicFileManager { get { return mixFm; } }

        public GameType CurrentGameType { get; private set; }

        public TheaterType CurrentTheater { get; private set; }

        public String LoadRoot { get; private set; }

        private readonly List<Megafile> megafiles = new List<Megafile>();

        private readonly HashSet<string> filenames = new HashSet<string>();

        public MegafileManager(string loadRoot, string looseFilePath, Dictionary<GameType, string[]> modPaths,
            MixFileNameGenerator romfis, Dictionary<GameType, string> classicGameFolders)
        {
            this.looseFilePath = looseFilePath;
            this.modPathsPerGame = modPaths;
            mixFm = new MixfileManager(loadRoot, romfis, classicGameFolders, modPaths);
            this.LoadRoot = Path.GetFullPath(loadRoot);
        }

        public bool LoadArchive(string archivePath)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (!Path.IsPathRooted(archivePath))
            {
                archivePath = Path.Combine(LoadRoot, archivePath);
            }
            if (!File.Exists(archivePath))
            {
                return false;
            }
            var megafile = new Megafile(archivePath);
            filenames.UnionWith(megafile);
            megafiles.Add(megafile);
            return true;
        }

        public bool FileExists(string path)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (path == null)
            {
                return false;
            }
            if (modPathsPerGame != null && modPathsPerGame.TryGetValue(CurrentGameType, out string[] modPaths) && modPaths != null && modPaths.Length > 0)
            {
                foreach (string modPath in modPaths)
                {
                    if (File.Exists(Path.Combine(path, modPath)))
                    {
                        return true;
                    }
                }
            }
            return File.Exists(Path.Combine(looseFilePath, path)) || filenames.Contains(path.ToUpper());
        }

        public Stream OpenFile(string path)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            string loosePath = Path.Combine(looseFilePath, path);
            if (File.Exists(loosePath))
            {
                return File.Open(loosePath, FileMode.Open, FileAccess.Read);
            }
            if (modPathsPerGame != null && modPathsPerGame.TryGetValue(CurrentGameType, out string[] modPaths) && modPaths != null && modPaths.Length > 0)
            {
                foreach (string modFilePath in modPaths)
                {
                    string modPath = Path.Combine(modFilePath, path);
                    if (File.Exists(modPath))
                    {
                        return File.Open(modPath, FileMode.Open, FileAccess.Read);
                    }
                }
            }
            foreach (var megafile in megafiles)
            {
                var stream = megafile.OpenFile(path.ToUpper());
                if (stream != null)
                {
                    return stream;
                }
            }
            return null;
        }

        public Byte[] ReadFile(string path)
        {
            using (Stream file = OpenFile(path))
            {
                if (file == null)
                {
                    return null;
                }
                return file.ReadAllBytes();
            }
        }

        public bool ClassicFileExists(string path)
        {
            using (Stream file = OpenFileClassic(path))
            {
                return file != null;
            }
        }

        public Stream OpenFileClassic(String path)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            return mixFm.OpenFile(path);
        }

        public Byte[] ReadFileClassic(string path)
        {
            using (Stream file = mixFm.OpenFile(path))
            {
                if (file == null)
                {
                    return null;
                }
                return file.ReadAllBytes();
            }
        }

        public void Reset(GameType gameType, TheaterType theater)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            this.CurrentGameType = gameType;
            this.CurrentTheater = theater;
            mixFm.Reset(gameType, theater);
        }

        public IEnumerator<string> GetEnumerator()
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            return filenames.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            return this.GetEnumerator();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    megafiles.ForEach(m => m.Dispose());
                    megafiles.Clear();
                    mixFm.Dispose();
                    mixFm = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
