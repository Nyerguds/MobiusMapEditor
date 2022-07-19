// This file is written by Maarten Meuris, aka Nyerguds. Use it however you want.
// I'm sure there's more elegant methods to detect games on Steam anyway, like an
// actual parser, so this little piece of code is public domain.

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

        public static String TryGetSteamGameFolder(string steamId, params String[] identifyingFiles)
        {
            String steamFolder = null;
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam"))
                {
                    Object path;
                    if (key != null && (path = key.GetValue("InstallPath")) != null)
                    {
                        steamFolder = path.ToString();
                    }
                }
                if (steamFolder == null)
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                    {
                        Object path;
                        if (key != null && (path = key.GetValue("InstallPath")) != null)
                        {
                            steamFolder = path.ToString();
                        }
                    }
                }
                if (steamFolder == null)
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                    {
                        Object path;
                        if (key != null && (path = key.GetValue("SteamPath")) != null)
                        {
                            steamFolder = path.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            if (steamFolder == null)
                return null;
            // Some versions of the above keys use slashes. Probably not a problem, but whatever.
            steamFolder = steamFolder.Replace('/', '\\');
            String libraryInfo = Path.Combine(steamFolder, "steamapps\\libraryfolders.vdf");
            if (!File.Exists(libraryInfo))
                return null;
            // Fairly naive implementation; doesn't properly deal with escaping. But for this purpose it should do.
            Regex keyval = new Regex("^\\s*\"([^\"]+)\"\\s*\"([^\"]+)\"\\s*$");
            List<string> foundFolders = new List<string>();
            int level = 0;
            Boolean foundLibFolders = false;
            String currentlibEntry = null;
            String currentlibFolder = null;
            Boolean insideGamesList = false;
            Boolean appFound = false;
            using (StreamReader sr = new StreamReader(libraryInfo))
            {
                String currentLine;
                while (!foundLibFolders && (currentLine = sr.ReadLine()) != null)
                {
                    foundLibFolders = "libraryfolders".Equals(currentLine.Trim().Trim('"'));
                }
                if (!foundLibFolders)
                    return null;
                while ((currentLine = sr.ReadLine()) != null)
                {
                    if (currentLine.Trim() == "{")
                    {
                        level++;
                        continue;
                    }
                    if (currentLine.Trim() == "}")
                    {
                        level--;
                        if (level == 0)
                        {
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
                    if (level == 1)
                    {
                        currentlibEntry = currentLine.Trim().Trim('"');
                        currentlibFolder = null;
                    }
                    else if (level == 2 && currentlibEntry != null)
                    {
                        if (currentlibFolder == null)
                        {
                            Match kvm = keyval.Match(currentLine);
                            if (kvm.Success && "path".Equals(kvm.Groups[1].Value))
                            {
                                currentlibFolder = kvm.Groups[2].Value;
                            }
                        }
                        insideGamesList = "apps".Equals(currentLine.Trim().Trim('"'));
                    }
                    else if (level == 3 && currentlibEntry != null && insideGamesList)
                    {
                        Match kvm = keyval.Match(currentLine);
                        if (kvm.Success && steamId.Equals(kvm.Groups[1].Value))
                        {
                            appFound = true;
                        }
                    }
                    if (appFound && currentlibFolder != null)
                    {
                        foundFolders.Add(currentlibFolder.Replace("\\\\", "\\"));
                        appFound = false;
                    }
                }
            }
            foreach (String path in foundFolders)
            {
                String appsPath = Path.Combine(path, "steamapps");
                if (!Directory.Exists(appsPath))
                    continue;
                String commonAppsPath = Path.Combine(appsPath, "common");
                if (!Directory.Exists(appsPath))
                    continue;
                String manifest = Path.Combine(appsPath, "appmanifest_" + steamId + ".acf");
                if (!File.Exists(manifest))
                    continue;
                String gameFolder;
                using (StreamReader sr = new StreamReader(manifest))
                {
                    String currentLine;
                    // Going to be lazy with this one. No hierarchival mess; just find the "installdir" line.
                    while ((currentLine = sr.ReadLine()) != null)
                    {
                        Match kvm = keyval.Match(currentLine);
                        if (!kvm.Success || !"installdir".Equals(kvm.Groups[1].Value))
                            continue;
                        gameFolder = kvm.Groups[2].Value.Replace("\\\\", "\\");
                        String actualFolder = Path.Combine(commonAppsPath, gameFolder);
                        if (!Directory.Exists(actualFolder))
                            continue;
                        if (identifyingFiles.Length == 0)
                            return actualFolder;
                        if (identifyingFiles.All(fn => File.Exists(Path.Combine(actualFolder, fn))))
                            return actualFolder;
                    }
                }
            }
            return null;
        }
    }
}
