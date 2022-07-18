//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free 
// software: you can redistribute it and/or modify it under the terms of 
// the GNU General Public License as published by the Free Software Foundation, 
// either version 3 of the License, or (at your option) any later version.

// The Command & Conquer Map Editor and corresponding source code is distributed 
// in the hope that it will be useful, but with permitted additional restrictions 
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT 
// distributed with this program. You should have received a copy of the 
// GNU General Public License along with permitted additional restrictions 
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using Microsoft.Win32;
using MobiusEditor.Dialogs;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace MobiusEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Change current culture to en-US
            if (Thread.CurrentThread.CurrentCulture.Name != "en-US")
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Do a test for CONFIG.MEG
            if (!FileTest())
            {
                // If it does not exist, then try to use the directory from settings
                bool validSavedDirectory = false;
                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.GameDirectoryPath) &&
                    Directory.Exists(Properties.Settings.Default.GameDirectoryPath))
                {
                    Environment.CurrentDirectory = Properties.Settings.Default.GameDirectoryPath;
                    if (FileTest())
                    {
                        validSavedDirectory = true;
                    }
                }
                // Before showing dialog to ask, try to autodetect the Steam path.
                if (!validSavedDirectory)
                {
                    String gameFolder = TryGetSteamGameFolder("1213210", "TiberianDawn.dll", "RedAlert.dll");
                    if (gameFolder != null)
                    {
                        Environment.CurrentDirectory = gameFolder;
                        if (FileTest())
                        {
                            validSavedDirectory = true;
                            Properties.Settings.Default.GameDirectoryPath = Environment.CurrentDirectory;
                            Properties.Settings.Default.Save();
                        }
                    }
                }
                // If the directory in settings is wrong too, then we need to ask the user for the installation dir
                if (!validSavedDirectory)
                {
                    var gameInstallationPathForm = new GameInstallationPathForm();
                    if (gameInstallationPathForm.ShowDialog() == DialogResult.No)
                        return;
                    Environment.CurrentDirectory = Path.GetDirectoryName(gameInstallationPathForm.SelectedPath);
                    Properties.Settings.Default.GameDirectoryPath = Environment.CurrentDirectory;
                    Properties.Settings.Default.Save();
                }
            }

            // Initialize megafiles
            var runPath = Environment.CurrentDirectory;
            Globals.TheMegafileManager = new MegafileManager(runPath);

            var megafilesLoaded = true;
            var megafilePath = Path.Combine(runPath, "DATA");
            megafilesLoaded &= Globals.TheMegafileManager.Load(Path.Combine(megafilePath, "CONFIG.MEG"));
            megafilesLoaded &= Globals.TheMegafileManager.Load(Path.Combine(megafilePath, "TEXTURES_COMMON_SRGB.MEG"));
            megafilesLoaded &= Globals.TheMegafileManager.Load(Path.Combine(megafilePath, "TEXTURES_RA_SRGB.MEG"));
            megafilesLoaded &= Globals.TheMegafileManager.Load(Path.Combine(megafilePath, "TEXTURES_SRGB.MEG"));
            megafilesLoaded &= Globals.TheMegafileManager.Load(Path.Combine(megafilePath, "TEXTURES_TD_SRGB.MEG"));
#if !DEVELOPER
            if (!megafilesLoaded)
            {
                MessageBox.Show("Required data is missing or corrupt, please validate your installation.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
#endif

            // Initialize texture, tileset, team color, and game text managers
            Globals.TheTextureManager = new TextureManager(Globals.TheMegafileManager);
            Globals.TheTilesetManager = new TilesetManager(Globals.TheMegafileManager, Globals.TheTextureManager, Globals.TilesetsXMLPath, Globals.TexturesPath);
            Globals.TheTeamColorManager = new TeamColorManager(Globals.TheMegafileManager);

            var cultureName = CultureInfo.CurrentUICulture.Name;
            var gameTextFilename = string.Format(Globals.GameTextFilenameFormat, cultureName.ToUpper());
            if (!Globals.TheMegafileManager.Exists(gameTextFilename))
            {
                gameTextFilename = string.Format(Globals.GameTextFilenameFormat, "EN-US");
            }
            Globals.TheGameTextManager = new GameTextManager(Globals.TheMegafileManager, gameTextFilename);

            // Initialize Steam if this is a Steam build
            if (SteamworksUGC.IsSteamBuild)
            {
                if (!SteamworksUGC.Init())
                {
#if !DEVELOPER
                    //MessageBox.Show("Unable to initialize Steam interface.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //return;
#endif
                }
            }
            if (Properties.Settings.Default.ShowInviteWarning)
            {
                using (var inviteMessageBox = new InviteMessageBox())
                {
                    // Ensures the dialog does not get lost by showing its icon in the taskbar.
                    inviteMessageBox.ShowInTaskbar = true;
                    inviteMessageBox.ShowDialog();
                    Properties.Settings.Default.ShowInviteWarning = !inviteMessageBox.DontShowAgain;
                    Properties.Settings.Default.Save();
                }
            }
            String arg = null;
            try
            {
                if (args.Length > 0 && File.Exists(args[0]))
                    arg = args[0];
            }
            catch
            {
                arg = null;
            }

            Application.Run(new MainForm(arg));

            if (SteamworksUGC.IsSteamBuild)
            {
                SteamworksUGC.Shutdown();
            }

            Globals.TheMegafileManager.Dispose();
        }

        private static String TryGetSteamGameFolder(string steamId, params String[] identifyingFiles)
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
            }
            catch (Exception ex)
            {
                return null;
            }
            if (steamFolder == null)
                return null;
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
                while (!foundLibFolders)
                {
                    currentLine = sr.ReadLine();
                    foundLibFolders = currentLine == "\"libraryfolders\"";
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
                String[] gameFolders = Directory.GetDirectories(appsPath);
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

        static bool FileTest()
        {
            return FileTest(Environment.CurrentDirectory);
        }

        static bool FileTest(String basePath)
        {
            return File.Exists(Path.Combine(basePath, "DATA", "CONFIG.MEG"));
        }
    }
}
