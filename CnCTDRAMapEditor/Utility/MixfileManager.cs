//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                     Version 2, December 2004
//
//  Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
//  Everyone is permitted to copy and distribute verbatim or modified
//  copies of this license document, and changing it is allowed as long
//  as the name is changed.
//
//             DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//    TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//   0. You just DO WHAT THE FUCK YOU WANT TO.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            public bool CanUseNewFormat { get; set; }

            public MixInfo(String name, bool isContainer, bool canBeEmbedded, bool isTheater, Boolean canUseNewFormat)
            {
                this.Name = name;
                this.IsContainer = isContainer;
                this.CanBeEmbedded = canBeEmbedded;
                this.IsTheater = isTheater;
                this.CanUseNewFormat = canUseNewFormat;
            }
        }

        private string applicationPath;
        Dictionary<GameType, string[]> modPathsPerGame;
        private Dictionary<GameType, string> gameFolders;
        private Dictionary<GameType, List<MixInfo>> gameArchives;
        private readonly List<MixInfo> currentMixFileInfo = new List<MixInfo>();
        private List<string> currentMixNames = new List<string>();
        private Dictionary<string, Mixfile> currentMixFiles;
        private GameType currentGameType = GameType.None;

        public string LoadRoot { get { return applicationPath; } }


        public MixfileManager(String applicationPath, Dictionary<GameType, String> gameFolders, Dictionary<GameType, string[]> modPaths)
        {
            this.applicationPath = applicationPath ?? Path.GetFullPath(".");
            this.gameFolders = gameFolders;
            if (gameFolders != null)
            {
                // Making 100% sure that that isn't in there
                gameFolders.Remove(GameType.None);
            }
            this.gameArchives = new Dictionary<GameType, List<MixInfo>>();
            this.modPathsPerGame = modPaths;
        }

        public bool FileExists(String path)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            // TODO check mod paths first? Not sure; files tend to need mixfiles
            using (Stream str = this.OpenFile(path))
            {
                return str != null;
            }
        }

        public bool LoadArchive(GameType gameType, String archivePath, bool isTheater)
        {
            return this.LoadArchive(gameType, archivePath, isTheater, false, false, false);
        }

        public bool LoadArchive(GameType gameType, String archivePath, bool isTheater, bool isContainer, bool canBeEmbedded, bool canUseNewFormat)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            // Doesn't really 'load' the archive, but instead registers it as known filename for this game type.
            // The actual loading won't happen until a Reset(...) is executed to specify the game to initialise.
            if (gameFolders == null || !gameFolders.TryGetValue(gameType, out string gamePath))
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
                archivesForGame.Add(new MixInfo(archivePath, isContainer, canBeEmbedded, isTheater, canUseNewFormat));
            }
            if (!Path.IsPathRooted(gamePath))
            {
                gamePath = Path.Combine(applicationPath, gamePath);
            }
            String fullPath = Path.Combine(gamePath, archivePath);
            // Mod paths might still add it, but this initial check is returned.
            return canBeEmbedded || File.Exists(fullPath);
        }

        public int LoadArchives(GameType gameType, String archiveMask, bool canUseNewFormat)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            // Doesn't really 'load' the archive, but instead registers it as known filename for this game type.
            // The actual loading won't happen until a Reset(...) is executed to specify the game to initialise.
            if (gameFolders == null || !gameFolders.TryGetValue(gameType, out string gamePath))
            {
                return 0;
            }
            List<string> foundFiles = new List<string>();
            Dictionary<string, string> foundPaths = new Dictionary<string, string>();
            Regex filter = GeneralUtils.FileMaskToRegex(archiveMask);
            List <MixInfo> archivesForGame;
            if (!gameArchives.TryGetValue(gameType, out archivesForGame))
            {
                archivesForGame = new List<MixInfo>();
                gameArchives[gameType] = archivesForGame;
            }
            // Not using hash map since order and iteration will be important.
            foreach (MixInfo file in archivesForGame)
            {
                if (filter.IsMatch(file.Name) && !foundPaths.ContainsKey(file.Name))
                {
                    foundFiles.Add(file.Name);
                    // Previously loaded files are higher in priority. Mark as inaccessible so
                    // it's not registered from other places, but also not added in the end.
                    foundPaths.Add(file.Name, null);
                }
            }
            if (!Path.IsPathRooted(gamePath))
            {
                gamePath = Path.Combine(applicationPath, gamePath);
            }
            if (modPathsPerGame != null && modPathsPerGame.TryGetValue(gameType, out string[] modPaths) && modPaths != null && modPaths.Length > 0)
            {
                // In each mod folder, try to read all mix files.
                foreach (string modPath in modPaths)
                {
                    String mixPath = Path.Combine(modPath, "ccdata");
                    if (!Directory.Exists(mixPath))
                    {
                        continue;
                    }
                    foreach (string filePath in Directory.GetFiles(mixPath, archiveMask))
                    {
                        string name = Path.GetFileName(filePath);
                        foundFiles.Add(name);
                        foundPaths.Add(name, filePath);
                    }
                }
            }
            if (Directory.Exists(gamePath))
            {
                foreach (string filePath in Directory.GetFiles(gamePath, archiveMask))
                {
                    string name = Path.GetFileName(filePath);
                    foundFiles.Add(name);
                    foundPaths.Add(name, filePath);
                }
            }
            int counter = 0;
            foreach (string filename in foundFiles)
            {
                // "filePath == null" indicates that file was already registered before.
                if (foundPaths.TryGetValue(filename, out string filePath) && filePath != null)
                {
                    archivesForGame.Add(new MixInfo(filePath, false, false, false, canUseNewFormat));
                    counter++;
                }
            }
            return counter;
        }

        public Stream OpenFile(String path)
        {
            return OpenFile(path, currentGameType, currentMixFileInfo);
        }

        public Byte[] ReadFile(string path)
        {
            using (Stream file = OpenFile(path, currentGameType, currentMixFileInfo))
            {
                if (file == null)
                {
                    return null;
                }
                return file.ReadAllBytes();
            }
        }

        public Stream OpenFileClassic(String path)
        {
            return OpenFile(path, currentGameType, currentMixFileInfo);
        }

        public Byte[] ReadFileClassic(string path)
        {
            return ReadFile(path);
        }

        private Stream OpenFile(String path, GameType gameType, List<MixInfo> mixFilesInfo)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            // Game folders dictionary determines which games are "known" to the system.
            if (gameFolders == null || !gameFolders.TryGetValue(gameType, out string gamePath))
            {
                return null;
            }
            if (!Path.IsPathRooted(gamePath))
            {
                gamePath = Path.Combine(applicationPath, gamePath);
            }
            // 1. Loose files in game files path
            string loosePath = Path.Combine(gamePath, path);
            if (File.Exists(loosePath))
            {
                return File.Open(loosePath, FileMode.Open, FileAccess.Read);
            }
            // 2. Loose files in mod path
            if (modPathsPerGame != null && modPathsPerGame.TryGetValue(gameType, out string[] modPaths) && modPaths != null && modPaths.Length > 0)
            {
                foreach (string modFilePath in modPaths)
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
            foreach (MixInfo mixInfo in mixFilesInfo)
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
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            string theaterMixFile = theater == null ? null : theater.ClassicTileset + ".mix";
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
            if (gameFolders == null || !gameFolders.TryGetValue(gameType, out string gamePath))
            {
                return;
            }
            if (!Path.IsPathRooted(gamePath))
            {
                gamePath = Path.Combine(applicationPath, gamePath);
            }
            List<MixInfo> newMixFileInfo = gameArchives.Where(kv => kv.Key == gameType).SelectMany(kv => kv.Value).ToList();
            Dictionary<string, Mixfile> foundMixFiles = new Dictionary<string, Mixfile>();
            if (modPathsPerGame != null && modPathsPerGame.TryGetValue(gameType, out string[] modPaths) && modPaths != null && modPaths.Length > 0)
            {
                // In each mod folder, try to read all mix files.
                foreach (string modPath in modPaths)
                {
                    foreach (MixInfo mixInfo in newMixFileInfo)
                    {
                        // Only load one theater mix file.
                        if (theaterMixFile != null && mixInfo.IsTheater && !String.Equals(theaterMixFile, mixInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }
                        String mixPath = Path.Combine(modPath, "ccdata");
                        // This automatically excludes already-loaded files.
                        this.AddMixFileIfPresent(foundMixFiles, newMixFileInfo, mixInfo, mixPath);
                    }
                }
            }
            foreach (MixInfo mixInfo in newMixFileInfo)
            {
                if (theaterMixFile != null && mixInfo.IsTheater && !String.Equals(theaterMixFile, mixInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                // This automatically excludes already-loaded files.
                this.AddMixFileIfPresent(foundMixFiles, newMixFileInfo, mixInfo, gamePath);
            }
            this.currentGameType = gameType;
            currentMixFiles = foundMixFiles;
            currentMixNames = foundMixFiles.Select(info => info.Key).ToList();
            HashSet<string> foundNames = currentMixNames.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            currentMixFileInfo.Clear();
            currentMixFileInfo.AddRange(newMixFileInfo.Where(mi => foundNames.Contains(mi.Name)));
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
                    mixFile = new Mixfile(localPath, mixToAdd.CanUseNewFormat);
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
                            mixFile = new Mixfile(container, mixName, mixToAdd.CanUseNewFormat);
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
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            return currentMixNames.GetEnumerator();
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
                    // This disposes all currently loaded mixfiles.
                    this.Reset(GameType.None, null);
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