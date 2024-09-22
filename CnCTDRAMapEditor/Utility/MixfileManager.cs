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

            public MixInfo(string name, bool isContainer, bool canBeEmbedded, bool isTheater, bool canUseNewFormat)
            {
                this.Name = name;
                this.IsContainer = isContainer;
                this.CanBeEmbedded = canBeEmbedded;
                this.IsTheater = isTheater;
                this.CanUseNewFormat = canUseNewFormat;
            }
        }

        private string applicationPath;
        private MixFileNameGenerator romfis;
        Dictionary<GameType, string[]> modPathsPerGame;
        private Dictionary<GameType, string> gameFolders;
        private Dictionary<GameType, List<MixInfo>> gameArchives;
        private readonly List<MixInfo> currentMixFileInfo = new List<MixInfo>();
        private List<string> currentMixNames = new List<string>();
        private Dictionary<string, MixFile> currentMixFiles;

        public string LoadRoot { get { return applicationPath; } }

        public GameType CurrentGameType { get; private set; }

        public TheaterType CurrentTheater { get; private set; }

        public MixfileManager(string applicationPath, MixFileNameGenerator romfis, Dictionary<GameType, string> gameFolders, Dictionary<GameType, string[]> modPaths)
        {
            this.applicationPath = applicationPath ?? Path.GetFullPath(".");
            this.romfis = romfis;
            this.gameFolders = gameFolders;
            if (gameFolders != null)
            {
                // Making 100% sure that that isn't in there
                gameFolders.Remove(GameType.None);
            }
            this.gameArchives = new Dictionary<GameType, List<MixInfo>>();
            this.modPathsPerGame = modPaths;
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
            // TODO check mod paths first? Not sure; files tend to need mixfiles
            using (Stream str = this.OpenFile(path))
            {
                return str != null;
            }
        }

        /// <summary>
        /// Registers an archive to be loaded the next time <see cref="Reset(GameType, TheaterType)"/> is called. This overload does not expose Red Alert's extended options.
        /// </summary>
        /// <param name="gameType">Game type to register this mix file for.</param>
        /// <param name="archivePath">Name of the archive.</param>
        /// <param name="isTheater">True if this is a Theater archive. This excludes the archive from getting loaded on <see cref="Reset(GameType, TheaterType)"/> when it does not match the specified theater.</param>
        /// <returns>True if the archive file was found.</returns>
        /// <exception cref="ObjectDisposedException">The MixFileManager is disposed.</exception>
        public bool LoadArchive(GameType gameType, string archivePath, bool isTheater)
        {
            return this.LoadArchive(gameType, archivePath, isTheater, false, false, false);
        }

        /// <summary>
        /// Registers an archive to be loaded the next time <see cref="Reset(GameType, TheaterType)"/> is called.
        /// </summary>
        /// <param name="gameType">Game type to register this mix file for.</param>
        /// <param name="archivePath">Name of the archive.</param>
        /// <param name="isTheater">True if this is a Theater archive. This excludes the archive from getting loaded on <see cref="Reset(GameType, TheaterType)"/> when it does not match the specified theater.</param>
        /// <param name="isContainer">True if this is a container archive that can contain other archives.</param>
        /// <param name="canBeEmbedded">True if this archive can be read from inside another archive. Note that it will only be searched inside archives loaded before this one.</param>
        /// <param name="canUseNewFormat">Allow RA's newer mix file format.</param>
        /// <returns>True if the archive file was found. Note that if it was not found, it might still be loaded from inside another mix file, but this is only handled on Reset.</returns>
        /// <exception cref="ObjectDisposedException">The MixFileManager is disposed.</exception>
        public bool LoadArchive(GameType gameType, string archivePath, bool isTheater, bool isContainer, bool canBeEmbedded, bool canUseNewFormat)
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
            if (archivesForGame.FindIndex(info => string.Equals(info.Name, archivePath, StringComparison.InvariantCultureIgnoreCase)) == -1)
            {
                archivesForGame.Add(new MixInfo(archivePath, isContainer, canBeEmbedded, isTheater, canUseNewFormat));
            }
            if (!Path.IsPathRooted(gamePath))
            {
                gamePath = Path.Combine(applicationPath, gamePath);
            }
            string fullPath = Path.Combine(gamePath, archivePath);
            // Mod paths might still add it, but this initial check is returned.
            return canBeEmbedded || File.Exists(fullPath);
        }

        /// <summary>
        /// Registers archives with a wildcard to be loaded the next time <see cref="Reset(GameType, TheaterType)"/> is called.
        /// Will search all available sources for these files, but cannot look inside other .mix files.
        /// </summary>
        /// <param name="gameType">Game type to register the files for.</param>
        /// <param name="archiveMask">file mask of archive.</param>
        /// <param name="canUseNewFormat">Allow RA's newer mix file format.</param>
        /// <returns>The amount of found archives.</returns>
        /// <exception cref="ObjectDisposedException">The MixFileManager is disposed.</exception>
        public int LoadArchives(GameType gameType, string archiveMask, bool canUseNewFormat)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (gameFolders == null || !gameFolders.TryGetValue(gameType, out string gamePath))
            {
                return 0;
            }
            // Doesn't really 'load' the archive, but instead registers it as known filename for this game type.
            // The actual loading won't happen until a Reset(...) is executed to specify the game to initialise.
            List<string> foundFiles = new List<string>();
            Dictionary<string, string> foundPaths = new Dictionary<string, string>();
            Regex filter = GeneralUtils.FileMaskToRegex(archiveMask);
            List<MixInfo> archivesForGame;
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
                    string mixPath = Path.Combine(modPath, "ccdata");
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

        public Stream OpenFile(string path)
        {
            return OpenFile(path, CurrentGameType, currentMixFileInfo);
        }

        public byte[] ReadFile(string path)
        {
            using (Stream file = OpenFile(path, CurrentGameType, currentMixFileInfo))
            {
                return file?.ReadAllBytes();
            }
        }

        public bool ClassicFileExists(string path)
        {
            return FileExists(path);
        }

        public Stream OpenFileClassic(string path)
        {
            return OpenFile(path, CurrentGameType, currentMixFileInfo);
        }

        public byte[] ReadFileClassic(string path)
        {
            return ReadFile(path);
        }

        private Stream OpenFile(string path, GameType gameType, List<MixInfo> mixFilesInfo)
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
            string rootPath = Path.Combine(applicationPath, path);
            // 0. Overrides in actual program folder.
            if (File.Exists(rootPath))
            {
                return File.Open(rootPath, FileMode.Open, FileAccess.Read);
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
                if (currentMixFiles != null && currentMixFiles.TryGetValue(mixInfo.Name, out MixFile archive))
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
                foreach (MixFile oldMixFile in currentMixFiles.Values)
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
            this.CurrentGameType = GameType.None;
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
            Dictionary<string, MixFile> foundMixFiles = new Dictionary<string, MixFile>();
            if (modPathsPerGame != null && modPathsPerGame.TryGetValue(gameType, out string[] modPaths) && modPaths != null && modPaths.Length > 0)
            {
                // In each mod folder, try to read all mix files.
                foreach (string modPath in modPaths)
                {
                    foreach (MixInfo mixInfo in newMixFileInfo)
                    {
                        // Only load one theater mix file.
                        if (theaterMixFile != null && mixInfo.IsTheater && !string.Equals(theaterMixFile, mixInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }
                        string mixPath = Path.Combine(modPath, "ccdata");
                        // This automatically excludes already-loaded files.
                        this.AddMixFileIfPresent(foundMixFiles, newMixFileInfo, mixInfo, mixPath);
                    }
                }
            }
            foreach (MixInfo mixInfo in newMixFileInfo)
            {
                if (theaterMixFile != null && mixInfo.IsTheater && !string.Equals(theaterMixFile, mixInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                // This automatically excludes already-loaded files.
                this.AddMixFileIfPresent(foundMixFiles, newMixFileInfo, mixInfo, gamePath);
            }
            this.CurrentGameType = gameType;
            this.CurrentTheater = theater;
            currentMixFiles = foundMixFiles;
            currentMixNames = foundMixFiles.Select(info => info.Key).ToList();
            HashSet<string> foundNames = currentMixNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
            currentMixFileInfo.Clear();
            currentMixFileInfo.AddRange(newMixFileInfo.Where(mi => foundNames.Contains(mi.Name)));
        }

        private bool AddMixFileIfPresent(Dictionary<string, MixFile> readMixFiles, List<MixInfo> readMixNames, MixInfo mixToAdd, string readFolder)
        {
            // 1. Look for file in given folder
            // 2. if 'CanBeEmbedded', look for file inside archives inside mix files list
            // 3. If found in either, add to list of read mix files
            //if (File.Exists(newMixPath)) { }
            string mixName = mixToAdd.Name;
            if (readMixFiles.ContainsKey(mixName))
            {
                return false;
            }
            string localPath = Path.Combine(readFolder, mixName);
            MixFile mixFile = null;
            if (File.Exists(localPath))
            {
                try
                {
                    mixFile = new MixFile(localPath, mixToAdd.CanUseNewFormat);
                }
                catch (MixParseException)
                {
                    return false;
                }
            }
            if (mixFile == null && mixToAdd.CanBeEmbedded)
            {
                foreach (MixInfo readArchive in readMixNames)
                {
                    if (readArchive.IsContainer && readMixFiles.TryGetValue(readArchive.Name, out MixFile container))
                    {
                        // Check if file exists
                        MixEntry[] allInfo = container.GetFullFileInfo(mixName);
                        if (allInfo != null && allInfo.Length > 0)
                        {
                            MixEntry info = allInfo[0];
                            info.Name = mixName;
                            // Create as embedded mix file
                            mixFile = new MixFile(container, info, mixToAdd.CanUseNewFormat);
                        }
                    }
                }
            }
            if (mixFile != null)
            {
                if (romfis != null)
                {
                    romfis.IdentifyMixFile(mixFile);
                }
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