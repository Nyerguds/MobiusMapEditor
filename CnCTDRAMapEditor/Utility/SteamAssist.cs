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
//
// This file is written by Maarten Meuris, aka Nyerguds. Use it however you want.
// I'm sure there are more elegant methods to detect games on Steam anyway, like
// an actual parser, so this little piece of code is public domain.
using Microsoft.Win32;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MobiusEditor.Utility
{
    public class SteamAssist
    {
        /// <summary>Simple regex to identify a key-value pair on one line in Steam's special snowflake variant of json.</summary>
        private static Regex SteamKeyVal = new Regex("^\\s*\"([^\"]+)\"\\s*\"([^\"]+)\"\\s*$");
        private static readonly string AppIdName = "steam_appid.txt";

        public static string TryGetSteamId(string folder)
        {
            try
            {
                string appIdFile = Path.Combine(folder, AppIdName);
                if (!File.Exists(appIdFile))
                {
                    return null;
                }
                string id = File.ReadAllText(appIdFile).Trim("\r\n \t\0".ToCharArray()).Trim();
                if (!Regex.IsMatch(id, "^\\d+$"))
                {
                    return null;
                }
                return id;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to retrieve the folder that a Steam game is installed in by scanning the Steam library information.
        /// </summary>
        /// <param name="steamId">Steam game ID</param>
        /// <param name="identifyingFiles">Optional list of files that need to be present inside the found game folder.</param>
        /// <returns>The first found game folder that matches the criteria and that exists.</returns>
        public static string TryGetSteamGameFolder(string steamId, params string[] identifyingFiles)
        {
            if (steamId == null)
            {
                throw new ArgumentNullException("steamId");
            }
            string steamFolder = GetSteamFolder();
            if (steamFolder == null)
            {
                return null;
            }
            string[] foundLibraryFolders = GetLibraryFoldersForAppId(steamFolder, steamId);
            if (foundLibraryFolders == null)
            {
                return null;
            }
            return GetGameFolder(foundLibraryFolders, steamId, out _, identifyingFiles);
        }

        /// <summary>
        /// Attempts to retrieve the folder that a Steam game is installed in by scanning the Steam library information.
        /// </summary>
        /// <param name="steamId">Steam game ID</param>
        /// <param name="identifyingFiles">Optional list of files that need to be present inside the found game folder.</param>
        /// <returns>The first found game folder that matches the criteria and that exists.</returns>
        public static string GetSteamGameName(string steamId)
        {
            if (steamId == null)
            {
                throw new ArgumentNullException("steamId");
            }
            string steamFolder = GetSteamFolder();
            if (steamFolder == null)
            {
                return null;
            }
            string[] foundLibraryFolders = GetLibraryFoldersForAppId(steamFolder, steamId);
            if (foundLibraryFolders == null)
            {
                return null;
            }
            string path = GetGameFolder(foundLibraryFolders, steamId, out string name);
            return name;
        }

        /// <summary>
        /// Scans the workshop files for the given Steam game ID for a mod. If <paramref name="contentFile"/> is given,
        /// a file with that name must exist somewhere inside the folder or its subfolders for it to be accepted. If no <paramref name="contentFile"/>
        /// is given, the function will return the base folder. Otherwise, the folder in which the first match of <paramref name="contentFile"/> is found is returned.
        /// </summary>
        /// <param name="steamId">The Steam game ID.</param>
        /// <param name="workShopId">The Steam workshop ID.</param>
        /// <param name="contentFile">A filename that must exist somewhere inside the workshop folder. If given, the subfolder in which the file was found is returned.</param>
        /// <param name="contentFolder">If given, <paramref name="contentFile"/> needs to be in this exact subfolder.</param>
        /// <param name="baseFolder">Returns the base folder of the workshop item.</param>
        /// <returns>The folder in which the requested file was found under the requested workshop id folder, or null if no existing match was found.</returns>
        /// <exception cref="ArgumentNullException">if steamId is null</exception>
        public static string GetWorkshopFolder(string steamId, string workShopId, string contentFile, string contentFolder, out string baseFolder)
        {
            baseFolder = null;
            if (steamId == null)
            {
                throw new ArgumentNullException("steamId");
            }
            string steamFolder = GetSteamFolder();
            if (steamFolder == null)
            {
                return null;
            }
            string[] foundLibraryFolders = GetLibraryFoldersForAppId(steamFolder, steamId);
            if (foundLibraryFolders == null)
            {
                return null;
            }
            return GetWorkshopFolder(foundLibraryFolders, steamId, workShopId, contentFile, contentFolder, out baseFolder);
        }

        /// <summary>
        /// Scans the given Steam library folders for a mod under the given Steam game ID. If <paramref name="contentFile"/> is given,
        /// a file with that name must exist somewhere inside the folder or its subfolders for it to be accepted. If no <paramref name="contentFile"/>
        /// is given, the function will return the base folder. Otherwise, the folder in which the first match of <paramref name="contentFile"/> is found is returned.
        /// </summary>
        /// <param name="libraryFolders">A list of Steam library folders. Use this overload if they were already fetched using <see cref="GetLibraryFoldersForAppId"/>.</param>
        /// <param name="steamId">The Steam game ID.</param>
        /// <param name="workShopId">The Steam workshop ID.</param>
        /// <param name="contentFile">A filename that must exist somewhere inside the workshop folder. If given, the subfolder in which the file was found is returned.</param>
        /// <param name="contentFolder">If given, <paramref name="contentFile"/> needs to be in this exact subfolder.</param>
        /// <param name="baseFolder">Returns the base folder of the workshop item.</param>
        /// <returns>The folder in which the requested file was found under the requested workshop id folder, or null if no existing match was found.</returns>
        /// <exception cref="ArgumentNullException">if steamId is null</exception>
        public static string GetWorkshopFolder(IEnumerable<string> libraryFolders, string steamId, string workShopId, string contentFile, string contentFolder, out string baseFolder)
        {
            if (steamId == null)
            {
                throw new ArgumentNullException("steamId");
            }
            if (libraryFolders == null)
            {
                baseFolder = null;
                return null;
            }
            foreach (string path in libraryFolders)
            {
                string modPath = Path.GetFullPath(Path.Combine(path, "steamapps", "workshop", "content", steamId, workShopId));
                if (!Directory.Exists(modPath))
                {
                    continue;
                }
                // No file to check; just accept the path.
                if (String.IsNullOrEmpty(contentFile))
                {
                    baseFolder = modPath;
                    return modPath;
                }
                // If given, it needs to match exactly.
                if (!String.IsNullOrEmpty(contentFolder))
                {
                    string folderPath = Path.Combine(modPath, contentFolder);
                    string contentFilePath = Path.Combine(folderPath, contentFile);
                    if (!File.Exists(contentFilePath))
                    {
                        continue;
                    }
                    baseFolder = modPath;
                    return folderPath;
                }
                // check all folders.
                string[] workshopFiles = Directory.GetFiles(modPath, contentFile, SearchOption.AllDirectories);
                Array.Sort(workshopFiles);
                for (int i = 0; i < workshopFiles.Length; ++i)
                {
                    string foundFile = workshopFiles[i];
                    if (contentFile.Equals(Path.GetFileName(foundFile), StringComparison.OrdinalIgnoreCase))
                    {
                        baseFolder = modPath;
                        return Path.GetDirectoryName(foundFile);
                    }
                }
            }
            baseFolder = null;
            return null;
        }

        /// <summary>
        /// Retrieves the Steam install folder from the registry.
        /// </summary>
        /// <returns>The Steam install folder, or null if nothing was found.</returns>
        public static string GetSteamFolder()
        {
            string steamFolder = GetSteamFolderInternal();
            // Some versions of the registry keys seem to use slashes. Probably not a problem, but whatever.
            return steamFolder == null ? null : steamFolder.Replace('/', '\\');
        }

        /// <summary>
        /// Retrieves the Steam install folder from the registry.
        /// </summary>
        /// <returns>The Steam install folder, or null if nothing was found.</returns>
        private static string GetSteamFolderInternal()
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
        /// <param name="steamId">Steam game ID.</param>
        /// <returns>A list of all library folder paths in which the given app ID was found.</returns>
        public static string[] GetLibraryFoldersForAppId(string steamId)
        {
            if (steamId == null)
            {
                throw new ArgumentNullException("steamId");
            }
            string steamFolder = GetSteamFolder();
            if (steamFolder == null)
            {
                return null;
            }
            return GetLibraryFoldersForAppId(steamFolder, steamId);
        }

        /// <summary>
        /// Looks up all library folders in which the given app ID can be found.
        /// </summary>
        /// <param name="steamFolder">Folder of the steam installation.</param>
        /// <param name="steamId">Steam game ID.</param>
        /// <returns>A list of all library folder paths in which the given app ID was found.</returns>
        private static string[] GetLibraryFoldersForAppId(string steamFolder, string steamId)
        {
            if (steamFolder == null)
            {
                throw new ArgumentNullException("steamFolder");
            }
            if (steamId == null)
            {
                throw new ArgumentNullException("steamId");
            }
            string libraryInfo = Path.Combine(steamFolder, "steamapps\\libraryfolders.vdf");
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
            return foundFolders.ToArray();
        }

        /// <summary>
        /// Scans the given Steam library folders for the given Steam game ID, and returns the first matching one.
        /// </summary>
        /// <param name="libraryFolders">A list of Steam library folders.</param>
        /// <param name="steamId">Steam game ID.</param>
        /// <param name="identifyingFiles">Optional list of files that need to be present inside the found game folder.</param>
        /// <returns>The first matching game folder for that id that is found, or null if no existing match was found.</returns>
        private static string GetGameFolder(IEnumerable<string> libraryFolders, string steamId, out string gameName, params string[] identifyingFiles)
        {
            if (steamId == null)
            {
                throw new ArgumentNullException("steamId");
            }
            gameName = null;
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
                if (!Directory.Exists(commonAppsPath))
                {
                    continue;
                }
                string manifest = Path.Combine(appsPath, "appmanifest_" + steamId + ".acf");
                if (!File.Exists(manifest))
                {
                    continue;
                }
                string gameFolder = null;
                using (StreamReader sr = new StreamReader(manifest))
                {
                    string currentLine;
                    // I'm going to be lazy with this one; no hierarchical mess, just find the "installdir" line.
                    while ((currentLine = sr.ReadLine()) != null)
                    {
                        Match kvm = SteamKeyVal.Match(currentLine);
                        if (!kvm.Success)
                        {
                            continue;
                        }
                        string itemName = kvm.Groups[1].Value;
                        string item = kvm.Groups[2].Value.Replace("\\\\", "\\");
                        if ("name".Equals(itemName))
                        {
                            gameName = item;
                        }
                        if ("installdir".Equals(itemName))
                        {
                            gameFolder = item;
                            string actualFolder = Path.Combine(commonAppsPath, gameFolder);
                            if (!Directory.Exists(actualFolder))
                            {
                                continue;
                            }
                            else
                            {
                                gameFolder = actualFolder;
                            }
                        }
                        if (gameFolder != null && gameName != null)
                        {
                            // Check if files exist in the found folder.
                            if ((identifyingFiles == null || identifyingFiles.Length == 0)
                                || identifyingFiles.All(fn => File.Exists(Path.Combine(gameFolder, fn))))
                            {
                                return gameFolder;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static string GetAppName()
        {
            var appId = SteamUtils.GetAppID().m_AppId.ToString();
            var task = GetAppName(appId);
            task.Wait();
            return task.Result;
        }

        public static async Task<string> GetAppName(string appId)
        {
            string url = $"https://store.steampowered.com/api/appdetails?appids={appId}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    JObject doc = JObject.Parse(result);
                    bool success = (bool)doc[appId]["success"];
                    if ((bool)doc[appId]["success"])
                    {
                        return (string)doc[appId]["data"]["name"];
                    }
                    else
                    {
                        return "App ID not found on the Steam store page!";
                    }
                }
                else
                {
                    return "Error fetching data from the Steam store page!";
                }
            }
        }
    }
}
