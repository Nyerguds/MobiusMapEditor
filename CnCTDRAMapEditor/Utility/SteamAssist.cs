// This file is written by Maarten Meuris, aka Nyerguds. Use it however you want.
// I'm sure there are more elegant methods to detect games on Steam anyway, like
// an actual parser, so this little piece of code is public domain.

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MobiusEditor.Utility
{
    public class SteamAssist
    {
        /// <summary>Simple regex to identify a key-value pair on one line in Steam's special snowflake variant of json.</summary>
        private static Regex SteamKeyVal = new Regex("^\\s*\"([^\"]+)\"\\s*\"([^\"]+)\"\\s*$");

        /// <summary>
        /// Attempts to retrieve the folder that a Steam game is installed in by scanning the Steam library information.
        /// </summary>
        /// <param name="steamId">Steam game ID</param>
        /// <param name="identifyingFiles">Optional list of files that need to be present inside the found game folder.</param>
        /// <returns>The first found folder that matches the criteria and that exists.</returns>
        public static String TryGetSteamGameFolder(string steamId, params String[] identifyingFiles)
        {
            if (steamId == null)
            {
                throw new ArgumentNullException("steamId");
            }
            String steamFolder = GetSteamFolder();
            if (steamFolder == null)
            {
                return null;
            }
            // Some versions of the registry keys seem to use slashes. Probably not a problem, but whatever.
            steamFolder = steamFolder.Replace('/', '\\');
            List<string> foundLibraryFolders = GetLibraryFoldersForAppId(steamFolder, steamId);
            if (foundLibraryFolders == null)
            {
                return null;
            }
            return GetGameFolder(foundLibraryFolders, steamId, identifyingFiles);
        }

        /// <summary>
        /// Retrieves the Steam install folder from the registry.
        /// </summary>
        /// <returns>The Steam install folder, or null if nothing was found.</returns>
        private static string GetSteamFolder()
        {
            object path;
            // First try 64-bit registry...
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam"))
                {
                    if (key != null && (path = key.GetValue("InstallPath")) != null)
                    {
                        return path.ToString();
                    }
                }
            }
            catch
            {
                // Ignore and continue.
            }
            // Then try 32-bit registry...
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                {
                    if (key != null && (path = key.GetValue("InstallPath")) != null)
                    {
                        return path.ToString();
                    }
                }
            }
            catch
            {
                // Ignore and continue.
            }
            // Finally, try local user registry.
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                {
                    if (key != null && (path = key.GetValue("SteamPath")) != null)
                    {
                        return path.ToString();
                    }
                }
            }
            catch
            {
                // Ignore and continue.
            }
            return null;
        }

        /// <summary>
        /// Looks up all library folders in which the given app ID can be found.
        /// </summary>
        /// <param name="steamFolder">Folder of the steam installation.</param>
        /// <param name="steamId">Steam game ID.</param>
        /// <returns>A list of all library folder paths in which the given app ID was found.</returns>
        private static List<string> GetLibraryFoldersForAppId(string steamFolder, string steamId)
        {
            if (steamFolder == null)
            {
                throw new ArgumentNullException("steamFolder");
            }
            if (steamId == null)
            {
                throw new ArgumentNullException("steamId");
            }
            String libraryInfo = Path.Combine(steamFolder, "steamapps\\libraryfolders.vdf");
            if (!File.Exists(libraryInfo))
                return null;
            // Fairly naive implementation; the regex doesn't properly deal with escaping. But for this
            // purpose it should do fine; the info we're scanning will never contain escaped quote chars.
            List<string> foundFolders = new List<string>();
            // Tree depth
            int level = 0;
            // Current library name
            string currentlibEntry = null;
            // Folder of current library
            string currentlibFolder = null;
            // True if we detected the start of a library entry's games list on level 2.
            bool insideGamesList = false;
            // True if the app id was found inside a library entry's games list.
            bool appFound = false;
            using (StreamReader sr = new StreamReader(libraryInfo))
            {
                string currentLine;
                // Ensure we're in the correct file by finding the main level property we need.
                bool foundLibFolders = false;
                while (!foundLibFolders && (currentLine = sr.ReadLine()) != null)
                {
                    foundLibFolders = "libraryfolders".Equals(currentLine.Trim().Trim('"'));
                }
                if (!foundLibFolders)
                {
                    return null;
                }
                // Scan library folders hierarchy.
                while ((currentLine = sr.ReadLine()) != null)
                {
                    currentLine = currentLine.Trim();
                    if (currentLine.Length == 0)
                    {
                        continue;
                    }
                    if (currentLine == "{")
                    {
                        level++;
                        continue;
                    }
                    if (currentLine == "}")
                    {
                        level--;
                        if (level == 0)
                        {
                            // Reached end of "libraryfolders" block; abort completely.
                            break;
                        }
                        if (level == 1)
                        {
                            currentlibEntry = null;
                            currentlibFolder = null;
                            insideGamesList = false;
                            appFound = false;
                            continue;
                        }
                        if (level == 2)
                        {
                            insideGamesList = false;
                        }
                        continue;
                    }
                    // We got what we needed; ignore all other lines until we leave this library entry.
                    if (appFound && currentlibFolder != null)
                    {
                        continue;
                    }
                    if (level == 1)
                    {
                        // Only thing ever read from level 1 that aren't brackets is a library entry name.
                        currentlibEntry = currentLine.Trim('"');
                        currentlibFolder = null;
                    }
                    else if (level == 2 && currentlibEntry != null)
                    {
                        if (currentlibFolder == null)
                        {
                            Match kvm = SteamKeyVal.Match(currentLine);
                            if (kvm.Success && "path".Equals(kvm.Groups[1].Value))
                            {
                                currentlibFolder = kvm.Groups[2].Value.Replace("\\\\", "\\");
                            }
                        }
                        insideGamesList = "apps".Equals(currentLine.Trim('"'));
                    }
                    else if (level == 3 && currentlibEntry != null && insideGamesList)
                    {
                        Match kvm = SteamKeyVal.Match(currentLine);
                        if (kvm.Success && steamId.Equals(kvm.Groups[1].Value))
                        {
                            appFound = true;
                        }
                    }
                    if (appFound && currentlibFolder != null)
                    {
                        foundFolders.Add(currentlibFolder);
                    }
                }
            }
            return foundFolders;
        }

        /// <summary>
        /// Scans the given Steam library folders for the given Steam game ID, and returns the first matching one.
        /// </summary>
        /// <param name="libraryFolders">A list of Steam library folders.</param>
        /// <param name="steamId">Steam game ID.</param>
        /// <param name="identifyingFiles">Optional list of files that need to be present inside the found game folder.</param>
        /// <returns>The first matching game folder for that id that is found, or null if no existing match was found.<</returns>
        private static string GetGameFolder(List<string> libraryFolders, string steamId, params string[] identifyingFiles)
        {
            if (steamId == null)
            {
                throw new ArgumentNullException("steamId");
            }
            if (libraryFolders == null)
            {
                return null;
            }
            foreach (string path in libraryFolders)
            {
                string appsPath = Path.Combine(path, "steamapps");
                if (!Directory.Exists(appsPath))
                {
                    continue;
                }
                string commonAppsPath = Path.Combine(appsPath, "common");
                if (!Directory.Exists(appsPath))
                {
                    continue;
                }
                string manifest = Path.Combine(appsPath, "appmanifest_" + steamId + ".acf");
                if (!File.Exists(manifest))
                {
                    continue;
                }
                string gameFolder;
                using (StreamReader sr = new StreamReader(manifest))
                {
                    string currentLine;
                    // I'm going to be lazy with this one; no hierarchical mess, just find the "installdir" line.
                    while ((currentLine = sr.ReadLine()) != null)
                    {
                        Match kvm = SteamKeyVal.Match(currentLine);
                        if (!kvm.Success || !"installdir".Equals(kvm.Groups[1].Value))
                        {
                            continue;
                        }
                        gameFolder = kvm.Groups[2].Value.Replace("\\\\", "\\");
                        string actualFolder = Path.Combine(commonAppsPath, gameFolder);
                        if (!Directory.Exists(actualFolder))
                        {
                            continue;
                        }
                        // No checks; just return the folder.
                        if (identifyingFiles == null || identifyingFiles.Length == 0)
                        {
                            return actualFolder;
                        }
                        // Check if files exist in the found folder.
                        if (identifyingFiles.All(fn => File.Exists(Path.Combine(actualFolder, fn))))
                        {
                            return actualFolder;
                        }
                    }
                }
            }
            return null;
        }

    }
}
