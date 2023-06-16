using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MobiusEditor.Interface;
using MobiusEditor.Model;

namespace MobiusEditor.Utility
{
    public class MixfileManager : IArchiveManager
    {
        private class MixInfo
        {
            public string Name { get; set; }
            public bool IsContainer { get; set; }
            public bool CanBeEmbedded { get; set; }
            public bool IsTheater { get; set; }


            public MixInfo(String name, bool isContainer, bool canBeEmbedded, bool isTheater)
            {
                this.Name = name;
                this.IsContainer = isContainer;
                this.CanBeEmbedded = canBeEmbedded;
                this.IsTheater = isTheater;
            }
        }

        private string applicationPath;
        private Dictionary<GameType, string> gameFolders;
        private Dictionary<GameType, List<MixInfo>> gameArchives;
        private readonly List<MixInfo> currentMixFileInfo = new List<MixInfo>();
        private List<string> currentMixNames = new List<string>();
        private Dictionary<string, Mixfile> currentMixFiles;
        private GameType currentGameType = GameType.None;

        public string LoadRoot { get { return applicationPath; } }
        public string[] ExpandModPaths { get; set; }

        public MixfileManager(String applicationPath, Dictionary<GameType, String> gameFolders)
        {
            this.applicationPath = applicationPath;
            this.gameFolders = gameFolders;
            this.gameArchives = new Dictionary<GameType, List<MixInfo>>();
            //this.Reset(currentGameType, null);
        }

        public bool FileExists(String path)
        {
            // TODO check mod paths first? Not sure; files tend to need mixfiles
            using (Stream str = this.OpenFile(path))
            {
                return str != null;
            }
        }
        public bool LoadArchive(GameType gameType, String archivePath, bool isContainer, bool canBeEmbedded)
        {
            return LoadArchive(gameType, archivePath, isContainer, canBeEmbedded, false);
        }

        public bool LoadArchive(GameType gameType, String archivePath, bool isContainer, bool canBeEmbedded, bool isTheater)
        {
            // Doesn't really 'load' the archive, but instead registers it as known filename for this game type.
            // The actual loading won't happen until a Reset(...) is executed to specify the game to initialise.
            if (!gameFolders.TryGetValue(gameType, out string gamePath))
            {
                return false;
            }
            List<MixInfo> archivesForGame;
            if (!gameArchives.TryGetValue(gameType, out archivesForGame))
            {
                archivesForGame = new List<MixInfo>();
                gameArchives[gameType] = archivesForGame;
            }
            // Not using hash map since order and iteration will be important.
            if (archivesForGame.FindIndex(info => String.Equals(info.Name, archivePath, StringComparison.InvariantCultureIgnoreCase)) == -1)
            {
                archivesForGame.Add(new MixInfo(archivePath, isContainer, canBeEmbedded, isTheater));
            }
            String fullPath = Path.Combine(applicationPath, gamePath, archivePath);
            // Mod paths might still add it, but this initial check is returned.
            return File.Exists(fullPath);
        }

        public Stream OpenFile(String path)
        {
            // Game folders dictionary determines which games are "known" to the system.
            if (!gameFolders.TryGetValue(currentGameType, out string gamePath))
            {
                return null;
            }
            // 1. Loose files in game files path
            string loosePath = Path.Combine(gamePath, path);
            if (File.Exists(loosePath))
            {
                return File.Open(loosePath, FileMode.Open, FileAccess.Read);
            }
            // 2. Loose files in mod path
            if (ExpandModPaths != null && ExpandModPaths.Length > 0)
            {
                foreach (string modFilePath in ExpandModPaths)
                {
                    string modPath = Path.Combine(modFilePath, "ccdata", path);
                    if (File.Exists(modPath))
                    {
                        return File.Open(modPath, FileMode.Open, FileAccess.Read);
                    }
                }
            }
            // 3. Contained inside mix files. Note that this automatically takes mods and
            // embedded mix files into account, since they are loaded in the Reset function.
            foreach (MixInfo mixInfo in currentMixFileInfo)
            {
                if (currentMixFiles != null && currentMixFiles.TryGetValue(mixInfo.Name, out Mixfile archive))
                {
                    Stream stream = archive.OpenFile(path);
                    if (stream != null)
                    {
                        return stream;
                    }
                }
            }
            return null;
        }

        public void Reset(GameType gameType, TheaterType theater)
        {
            String theaterMixFile = theater == null ? null : theater.ClassicTileset + ".mix";
            // Clean up previously loaded files.
            if (currentMixFiles != null)
            {
                foreach (Mixfile oldMixFile in currentMixFiles.Values)
                {
                    try
                    {
                        oldMixFile.Dispose();
                    }
                    catch { /* ignore */ }
                }
                currentMixFiles = null;
            }
            currentMixFileInfo.Clear();
            currentMixNames = new List<string>();
            this.currentGameType = GameType.None;
            // Load current files
            // Game folders dictionary determines which games are "known" to the system.
            if (!gameFolders.TryGetValue(gameType, out string gamePath))
            {
                return;
            }
            List<MixInfo> newMixFileInfo = gameArchives.Where(kv => kv.Key == gameType).SelectMany(kv => kv.Value).ToList();
            Dictionary<string, Mixfile> newMixFiles = new Dictionary<string, Mixfile>();
            if (ExpandModPaths != null && ExpandModPaths.Length > 0)
            {
                // In each mod folder, try to read all mix files.
                foreach (string modPath in ExpandModPaths)
                {
                    foreach (MixInfo mixInfo in newMixFileInfo)
                    {
                        if (mixInfo.IsTheater && !String.Equals(theaterMixFile, mixInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }
                        String mixPath = Path.Combine(modPath, "ccdata");
                        // This automatically excludes already-loaded files.
                        this.AddMixFileIfPresent(newMixFiles, newMixFileInfo, mixInfo, mixPath);
                    }
                }
            }
            foreach (MixInfo mixInfo in newMixFileInfo)
            {
                if (mixInfo.IsTheater && !String.Equals(theaterMixFile, mixInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                // This automatically excludes already-loaded files.
                this.AddMixFileIfPresent(newMixFiles, newMixFileInfo, mixInfo, gamePath);
            }
            this.currentGameType = gameType;
            currentMixFiles = newMixFiles;
            currentMixFileInfo.Clear();
            currentMixFileInfo.AddRange(newMixFileInfo);
            currentMixNames = currentMixFileInfo.Select(info => info.Name).ToList();
        }

        private bool AddMixFileIfPresent(Dictionary<String, Mixfile> readMixFiles, List<MixInfo> readMixNames, MixInfo mixToAdd, string readFolder)
        {
            // 1. Look for file in given folder
            // 2. if 'canBeEmbedded', look for file inside archives inside mix files list
            // 3. If found in either, add to list of read mix files
            //if (File.Exists(newMixPath)) { }
            string mixName = mixToAdd.Name;
            if (readMixFiles.ContainsKey(mixName))
            {
                return false;
            }
            string localPath = Path.Combine(readFolder, mixName);
            Mixfile mixFile = null;
            if (File.Exists(localPath))
            {
                try
                {
                    mixFile = new Mixfile(localPath);
                }
                catch(Exception ex)
                {
                    return false;
                }
            }
            if (mixFile == null && mixToAdd.CanBeEmbedded)
            {
                foreach (MixInfo readArchive in readMixNames)
                {
                    if (readArchive.IsContainer && readMixFiles.TryGetValue(readArchive.Name, out Mixfile container))
                    {
                        // Check if file exists
                        if (container.GetFileInfo(mixName, out _, out _))
                        {
                            // Create as embedded mix file
                            mixFile = new Mixfile(container, mixName);
                        }
                    }
                }
            }
            if (mixFile != null)
            {
                readMixFiles.Add(mixName, mixFile);
            }
            return mixFile != null;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return currentMixNames.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
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
                    //megafiles.ForEach(m => m.Dispose());
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