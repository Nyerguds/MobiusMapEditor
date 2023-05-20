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
using MobiusEditor.Dialogs;
using MobiusEditor.Interface;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
            TryEnableDPIAware();
            const string gameId = "1213210";
            // Change current culture to en-US
            if (Thread.CurrentThread.CurrentCulture.Name != "en-US")
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            }
            
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            if (!Version.TryParse(Properties.Settings.Default.ApplicationVersion, out Version oldVersion) || oldVersion < version)
            {
                Properties.Settings.Default.Upgrade();
                if (String.IsNullOrEmpty(Properties.Settings.Default.ApplicationVersion))
                {
                    CopyLastUserConfig(Properties.Settings.Default.ApplicationVersion, v => Properties.Settings.Default.ApplicationVersion = v);
                }
                Properties.Settings.Default.ApplicationVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                Properties.Settings.Default.Save();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Do a test for CONFIG.MEG
            string runPath = Environment.CurrentDirectory;
            if (!FileTest(runPath))
            {
                // If it does not exist, try to use the directory from the settings.
                bool validSavedDirectory = false;
                if (!String.IsNullOrWhiteSpace(Properties.Settings.Default.GameDirectoryPath) &&
                    Directory.Exists(Properties.Settings.Default.GameDirectoryPath))
                {
                    if (FileTest(Properties.Settings.Default.GameDirectoryPath))
                    {
                        runPath = Properties.Settings.Default.GameDirectoryPath;
                        validSavedDirectory = true;
                    }
                }
                // Before showing a dialog to ask, try to autodetect the Steam path.
                if (!validSavedDirectory)
                {
                    string gameFolder = SteamAssist.TryGetSteamGameFolder(gameId, "TiberianDawn.dll", "RedAlert.dll");
                    if (gameFolder != null)
                    {
                        if (FileTest(gameFolder))
                        {
                            runPath = gameFolder;
                            validSavedDirectory = true;
                            Properties.Settings.Default.GameDirectoryPath = gameFolder;
                            Properties.Settings.Default.Save();
                        }
                    }
                }
                // If the directory in the settings is wrong, and it can not be autodetected, we need to ask the user for the installation dir.
                if (!validSavedDirectory)
                {
                    var gameInstallationPathForm = new GameInstallationPathForm();
                    if (gameInstallationPathForm.ShowDialog() == DialogResult.No)
                        return;
                    runPath = Path.GetDirectoryName(gameInstallationPathForm.SelectedPath);
                    Properties.Settings.Default.GameDirectoryPath = runPath;
                    Properties.Settings.Default.Save();
                }
            }
            // TODO split off .mix reader.
            // mix file manager constructor should get two arguments:
            // -A mapping of the games to their respective folders
            // -A mapping of the games to a list of all .mix archives used by that game.
            // The system should scan all mix archives for known filenames of other mix archives so it can do recursive searches.

            // Initialize megafiles
            Globals.TheArchiveManager = new MegafileManager(Path.Combine(runPath, Globals.MegafilePath), runPath);

            var megafilesLoaded = true;
            megafilesLoaded &= Globals.TheArchiveManager.LoadArchive("CONFIG.MEG");
            megafilesLoaded &= Globals.TheArchiveManager.LoadArchive("TEXTURES_COMMON_SRGB.MEG");
            megafilesLoaded &= Globals.TheArchiveManager.LoadArchive("TEXTURES_RA_SRGB.MEG");
            megafilesLoaded &= Globals.TheArchiveManager.LoadArchive("TEXTURES_SRGB.MEG");
            megafilesLoaded &= Globals.TheArchiveManager.LoadArchive("TEXTURES_TD_SRGB.MEG");
#if !DEVELOPER
            if (!megafilesLoaded)
            {
                MessageBox.Show("Required data is missing or corrupt, please validate your installation.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
#endif
            // Check if any mods are allowed to override the default stuff to load.
            Dictionary<GameType, string[]> modPaths = new Dictionary<GameType, string[]>();
            const string tdModFolder = "Tiberian_Dawn";
            const string raModFolder = "Red_Alert";
            modPaths.Add(GameType.TiberianDawn, GetModPaths(gameId, Properties.Settings.Default.ModsToLoadTD, tdModFolder, "TD"));
            modPaths.Add(GameType.RedAlert, GetModPaths(gameId, Properties.Settings.Default.ModsToLoadRA, raModFolder, "RA"));
            modPaths.Add(GameType.SoleSurvivor, GetModPaths(gameId, Properties.Settings.Default.ModsToLoadSS, tdModFolder, "TD"));
            // Initialize texture, tileset, team color, and game text managers
            Globals.TheTextureManager = new TextureManager(Globals.TheArchiveManager);
            Globals.TheTilesetManager = new TilesetManager(Globals.TheArchiveManager, Globals.TheTextureManager, Globals.TilesetsXMLPath, Globals.TexturesPath);
            Globals.TheTeamColorManager = new TeamColorManager(Globals.TheArchiveManager);
            // Not adapted to mods for now...
            var cultureName = CultureInfo.CurrentUICulture.Name;
            var gameTextFilename = string.Format(Globals.GameTextFilenameFormat, cultureName.ToUpper());
            if (!Globals.TheArchiveManager.FileExists(gameTextFilename))
            {
                gameTextFilename = string.Format(Globals.GameTextFilenameFormat, "EN-US");
            }
            GameTextManager gtm = new GameTextManager(Globals.TheArchiveManager, gameTextFilename);
            //gtm.Dump("alltext.txt");
            Globals.TheGameTextManager = gtm;
            AddMissingRemasterText(gtm);
            if (SteamworksUGC.IsSteamBuild)
            {
                // Ignore result from this.
                SteamworksUGC.Init();
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
            using (MainForm mainForm = new MainForm(arg))
            {
                mainForm.ModPaths = modPaths;
                Application.Run(mainForm);
            }
            if (SteamworksUGC.IsSteamBuild)
            {
                SteamworksUGC.Shutdown();
            }
            Globals.TheArchiveManager.Dispose();
        }

        private static void AddMissingRemasterText(GameTextManager gtm)
        {
            // Buildings
            gtm["TEXT_STRUCTURE_TITLE_OIL_PUMP"] = "Oil Pump";
            gtm["TEXT_STRUCTURE_TITLE_OIL_TANKER"] = "Oil Tanker";

            // Overlay
            gtm["TEXT_OVERLAY_CONCRETE_PAVEMENT"] = "Concrete";
            gtm["TEXT_OVERLAY_CONCRETE_ROAD"] = "Concrete Road";
            gtm["TEXT_OVERLAY_CONCRETE_ROAD_FULL"] = "Concrete Road (full)";
            gtm["TEXT_OVERLAY_TIBERIUM"] = "Tiberium";
            // "Gold" exists as "TEXT_CURRENCY_TACTICAL"
            gtm["TEXT_OVERLAY_GEMS"] = "Gems";
            gtm["TEXT_OVERLAY_WCRATE"] = "Wooden Crate";
            gtm["TEXT_OVERLAY_SCRATE"] = "Steel Crate";
            gtm["TEXT_OVERLAY_WATER_CRATE"] = "Water Crate";

            // Smudge
            gtm["TEXT_SMUDGE_CRATER"] = "Crater";
            gtm["TEXT_SMUDGE_SCORCH"] = "Scorch Mark";
            gtm["TEXT_SMUDGE_BIB"] = "Road Bib";
        }

        private static void AddMissingClassicText(GameTextManager gtm)
        {
            // Overlay
            gtm["TEXT_OVERLAY_CONCRETE_ROAD"] = "Concrete Road";
            gtm["TEXT_OVERLAY_CONCRETE_ROAD_FULL"] = "Concrete Road (full)";
        }

        [DllImport("SHCore.dll")]
        private static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

        private enum PROCESS_DPI_AWARENESS
        {
            Process_DPI_Unaware = 0,
            Process_System_DPI_Aware = 1,
            Process_Per_Monitor_DPI_Aware = 2
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetProcessDPIAware();

        internal static void TryEnableDPIAware()
        {
            try
            {
                SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.Process_Per_Monitor_DPI_Aware);
            }
            catch
            {
                try
                { // fallback, use (simpler) internal function
                    SetProcessDPIAware();
                }
                catch { }
            }
        }

        private static string[] GetModPaths(string gameId, string modstoLoad, string modFolder, string modIdentifier)
        {
            Regex numbersOnly = new Regex("^\\d+$");
            Regex modregex = new Regex("\"game_type\"\\s*:\\s*\""+ modIdentifier + "\"");
            const string contentFile = "ccmod.json";
            string modsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CnCRemastered", "Mods");
            string[] steamLibraryFolders = SteamAssist.GetLibraryFoldersForAppId(gameId);
            string[] mods = (modstoLoad ?? String.Empty).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> modPaths = new List<string>();
            for (int i = 0; i < mods.Length; ++i)
            {
                string modDef = mods[i].Trim();
                if (String.IsNullOrEmpty(modDef))
                {
                    continue;
                }
                string addonModPath;
                // Lookup by Steam ID
                if (numbersOnly.IsMatch(modDef))
                {
                    addonModPath = SteamAssist.TryGetSteamWorkshopFolder(gameId, modDef, contentFile, null);
                    if (addonModPath != null)
                    {
                        if (CheckAddonPathModType(addonModPath, contentFile, modregex))
                        {
                            modPaths.Add(addonModPath);
                        }
                    }
                    // don't bother checking more on a numbers-only entry.
                    continue;
                }
                // Lookup by folder name
                addonModPath = Path.Combine(modsFolder, modFolder, modDef);
                if (CheckAddonPathModType(addonModPath, contentFile, modregex))
                {
                    modPaths.Add(addonModPath);
                    // Found in local mods; don't check Steam ones.
                    continue;
                }
                // try to find mod in steam library.
                foreach (string libFolder in steamLibraryFolders)
                {
                    string modPath = Path.Combine(libFolder, "steamapps", "workshop", "content", gameId);
                    if (!Directory.Exists(modPath))
                    {
                        continue;
                    }
                    foreach (string modPth in Directory.GetDirectories(modPath))
                    {
                        addonModPath = Path.Combine(modPth, modDef);
                        if (CheckAddonPathModType(addonModPath, contentFile, modregex))
                        {
                            modPaths.Add(addonModPath);
                            break;
                        }
                    }
                }
            }
            return modPaths.Distinct(StringComparer.CurrentCultureIgnoreCase).ToArray();
        }

        private static bool CheckAddonPathModType(string addonModPath, string contentFile, Regex modregex)
        {
            try
            {
                string checkPath = Path.Combine(addonModPath, contentFile);
                if (!File.Exists(checkPath))
                {
                    return false;
                }
                string ccModDefContents = File.ReadAllText(checkPath);
                return modregex.IsMatch(ccModDefContents);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ports settings over from older versions, and ensures only one settings folder remains.
        /// Taken from https://stackoverflow.com/a/14845448 and adapted to scan deeper.
        /// </summary>
        /// <param name="currentSettingsVer">Curent version fetched from the settings.</param>
        /// <param name="versionSetter">Delegate to set the version into the settings after the process is complete.</param>
        private static void CopyLastUserConfig(String currentSettingsVer, Action<String> versionSetter)
        {
            AssemblyName assn = Assembly.GetExecutingAssembly().GetName();
            Version currentVersion = assn.Version;
            if (currentVersion.ToString() == currentSettingsVer)
            {
                return;
            }
            string userConfigFileName = "user.config";
            // Expected location of the current user config
            DirectoryInfo currentVersionConfigFileDir = new FileInfo(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath).Directory;
            if (currentVersionConfigFileDir == null)
            {
                return;
            }
            DirectoryInfo currentParent = currentVersionConfigFileDir.Parent;
            DirectoryInfo previousSettingsDir = null;
            if (currentParent != null && currentParent.Exists)
            {
                try
                {
                    // Location of the previous user config
                    // grab the most recent folder from the list of user's settings folders, prior to the current version
                    previousSettingsDir = (from dir in currentParent.GetDirectories()
                                           let dirVer = new { Dir = dir, Ver = new Version(dir.Name) }
                                           where dirVer.Ver < currentVersion
                                           orderby dirVer.Ver descending
                                           select dir).FirstOrDefault();
                }
                catch
                {
                    // Ignore.
                }
            }
            // Find any other folders for this application.
            DirectoryInfo[] otherSettingsFolders = null;
            string dirNameProgPart = currentParent?.Name;
            const string dirnameCutoff = "_Url_";
            int dirNameProgPartLen = dirNameProgPart == null ? -1 : dirNameProgPart.LastIndexOf(dirnameCutoff, StringComparison.OrdinalIgnoreCase);
            if (dirNameProgPartLen == -1)
            {
                // Fallback: reproduce what's done to the exe string to get a folder. Just to be sure. From observation, actual exe cutoff length is 25.
                dirNameProgPart = Path.GetFileName(assn.CodeBase).Replace(' ', '_');
                dirNameProgPart = dirNameProgPart.Substring(0, Math.Min(20, dirNameProgPart.Length));
            }
            else
            {
                dirNameProgPart = dirNameProgPart.Substring(0, dirNameProgPartLen + dirnameCutoff.Length);
            }
            if (currentParent != null && currentParent.Parent != null && currentParent.Parent.Exists)
            {
                otherSettingsFolders = currentParent.Parent.GetDirectories(dirNameProgPart + "*").OrderBy(p => p.CreationTime).Reverse().ToArray();
            }
            if (otherSettingsFolders != null && otherSettingsFolders.Length > 0 && previousSettingsDir == null)
            {
                foreach (DirectoryInfo parDir in otherSettingsFolders)
                {
                    if (parDir.Name == currentParent.Name)
                        continue;
                    try
                    {
                        // see if there's a same-version folder in other parent folder
                        previousSettingsDir = (from dir in parDir.GetDirectories()
                                                let dirVer = new { Dir = dir, Ver = new Version(dir.Name) }
                                                where dirVer.Ver == currentVersion
                                                orderby dirVer.Ver descending
                                                select dir).FirstOrDefault();
                    }
                    catch
                    {
                        // Ignore.
                    }
                    if (previousSettingsDir != null)
                        break;
                    try
                    {
                        // see if there's an older version folder in other parent folder
                        previousSettingsDir = (from dir in parDir.GetDirectories()
                                                let dirVer = new { Dir = dir, Ver = new Version(dir.Name) }
                                                where dirVer.Ver < currentVersion
                                                orderby dirVer.Ver descending
                                                select dir).FirstOrDefault();
                    }
                    catch
                    {
                        // Ignore.
                    }
                    if (previousSettingsDir != null)
                        break;
                }
            }
            
            string previousVersionConfigFile = previousSettingsDir == null ? null : string.Concat(previousSettingsDir.FullName, @"\", userConfigFileName);
            string currentVersionConfigFile = string.Concat(currentVersionConfigFileDir.FullName, @"\", userConfigFileName);
            if (!currentVersionConfigFileDir.Exists)
            {
                Directory.CreateDirectory(currentVersionConfigFileDir.FullName);
            }
            if (previousVersionConfigFile != null)
            {
                File.Copy(previousVersionConfigFile, currentVersionConfigFile, true);
            }
            if (File.Exists(currentVersionConfigFile))
            {
                Properties.Settings.Default.Reload();
            }
            versionSetter(currentVersion.ToString());
            Properties.Settings.Default.Save();
            if (otherSettingsFolders != null && otherSettingsFolders.Length > 0)
            {
                foreach (DirectoryInfo parDir in otherSettingsFolders)
                {
                    if (parDir.Name == currentParent.Name)
                        continue;
                    // Wipe them out. All of them.
                    try
                    {
                        parDir.Delete(true);
                    }
                    catch
                    {
                        // Ignore
                    }
                }
            }
        }

        static bool FileTest(String basePath)
        {
            return File.Exists(Path.Combine(basePath, "DATA", "CONFIG.MEG"));
        }
    }
}
