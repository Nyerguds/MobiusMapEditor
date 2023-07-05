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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace MobiusEditor
{
    static class Program
    {
        const string gameId = "1213210";
        public static readonly String ApplicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            TryEnableDPIAware();
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
            Dictionary<GameType, string[]> modPaths = new Dictionary<GameType, string[]>();
            // Check if any mods are allowed to override the default stuff to load.
            const string tdModFolder = "Tiberian_Dawn";
            const string raModFolder = "Red_Alert";
            modPaths.Add(GameType.TiberianDawn, GetModPaths(gameId, Properties.Settings.Default.ModsToLoadTD, tdModFolder, "TD"));
            modPaths.Add(GameType.RedAlert, GetModPaths(gameId, Properties.Settings.Default.ModsToLoadRA, raModFolder, "RA"));
            modPaths.Add(GameType.SoleSurvivor, GetModPaths(gameId, Properties.Settings.Default.ModsToLoadSS, tdModFolder, "TD"));
            String runPath = GetRemasterRunPath(!Globals.UseClassicFiles);
            if (runPath != null)
            {
                // Required for Steam interface to work.
                Environment.CurrentDirectory = runPath;
            }
            bool loadOk = false;
            if (!Globals.UseClassicFiles && runPath != null)
            {
                loadOk = LoadEditorRemastered(runPath, modPaths);
            }
            else if (Globals.UseClassicFiles)
            {
                loadOk = LoadEditorClassic(modPaths);
            }
            if (!loadOk)
            {
                return;
            }
            bool steamEnabled = false;
            if (SteamworksUGC.IsSteamBuild)
            {
                // Ignore result from this.
                steamEnabled = SteamworksUGC.Init();
            }
            if (steamEnabled & Properties.Settings.Default.ShowInviteWarning)
            {
                using (var inviteMessageBox = new InviteMessageBox())
                {
                    // Ensures the dialog does not get lost by showing its icon in the taskbar.
                    inviteMessageBox.ShowInTaskbar = true;
                    inviteMessageBox.StartPosition = FormStartPosition.CenterScreen;
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
                Application.Run(mainForm);
            }
            if (SteamworksUGC.IsSteamBuild)
            {
                SteamworksUGC.Shutdown();
            }
            Globals.TheArchiveManager.Dispose();
        }

        private static bool LoadEditorClassic(Dictionary<GameType, string[]> modpaths)
        {
            // The system should scan all mix archives for known filenames of other mix archives so it can do recursive searches.
            // Mix files should be given in order or depth, so first give ones that are in the folder, then ones that may occur inside others.
            // The order of load determines the file priority; only the first found occurrence of a file is used.
            String tdPath = Properties.Settings.Default.ClassicPathTD;
            String tdPathFull = Path.GetFullPath(Path.Combine(ApplicationPath, tdPath));
            if (!Directory.Exists(tdPathFull))
            {
                tdPath = "Classic\\TD\\";
                tdPathFull = Path.GetFullPath(Path.Combine(ApplicationPath, tdPath));
            }
            String raPath = Properties.Settings.Default.ClassicPathRA;
            String raPathFull = Path.GetFullPath(Path.Combine(ApplicationPath, raPath));
            if (!Directory.Exists(raPathFull))
            {
                raPath = "Classic\\RA\\";
                raPathFull = Path.GetFullPath(Path.Combine(ApplicationPath, raPath));
            }
            String ssPath = Properties.Settings.Default.ClassicPathSS;
            String ssPathFull = Path.GetFullPath(Path.Combine(ApplicationPath, ssPath));
            if (!Directory.Exists(ssPathFull))
            {
                ssPath = "Classic\\TD\\";
                ssPathFull = Path.GetFullPath(Path.GetFullPath(Path.Combine(ApplicationPath, ssPath)));
            }
            if (String.Equals(Path.GetFullPath(raPathFull), Path.GetFullPath(tdPathFull), StringComparison.InvariantCultureIgnoreCase))
            {
                MessageBox.Show("Error while loading files: Tiberian Dawn and Red Alert classic data paths are identical!");
                return false;
            }
            if (String.Equals(Path.GetFullPath(raPathFull), Path.GetFullPath(ssPathFull), StringComparison.InvariantCultureIgnoreCase))
            {
                MessageBox.Show("Error while loading files: Sole Survivor and Red Alert classic data paths are identical!");
                return false;
            }
            Dictionary<GameType, String> gameFolders = new Dictionary<GameType, string>();
            gameFolders.Add(GameType.TiberianDawn, tdPath);
            gameFolders.Add(GameType.RedAlert, raPath);
            gameFolders.Add(GameType.SoleSurvivor, ssPath);
            MixfileManager mfm = new MixfileManager(ApplicationPath, gameFolders, modpaths);
            Globals.TheArchiveManager = mfm;
            List<string> loadErrors = new List<string>();
            List<string> fileLoadErrors = new List<string>();
            // This will map the mix files to the respective games, and look for them in the respective folders.
            // Tiberian Dawn
            mfm.LoadArchive(GameType.TiberianDawn, "local.mix", false);
            mfm.LoadArchive(GameType.TiberianDawn, "cclocal.mix", false);
            // Mod addons
            mfm.LoadArchives(GameType.TiberianDawn, "sc*.mix", false);
            mfm.LoadArchive(GameType.TiberianDawn, "conquer.mix", false);
            // Tiberian Dawn Theaters
            mfm.LoadArchive(GameType.TiberianDawn, "desert.mix", true);
            mfm.LoadArchive(GameType.TiberianDawn, "temperat.mix", true);
            mfm.LoadArchive(GameType.TiberianDawn, "winter.mix", true);
            // Check files
            modpaths.TryGetValue(GameType.TiberianDawn, out string[] tdModPaths);
            modpaths.TryGetValue(GameType.SoleSurvivor, out string[] ssModPaths);
            bool tdSsEqual = ssModPaths.SequenceEqual(tdModPaths) && tdPathFull.Equals(ssPathFull);
            mfm.Reset(GameType.TiberianDawn, null);
            List<String> loadedFiles = mfm.ToList();
            string prefix = tdSsEqual ? "TD/SS: " : "TD: ";
            if (!loadedFiles.Contains("local.mix") && !loadedFiles.Contains("cclocal.mix")) loadErrors.Add(prefix + "local.mix / cclocal.mix");
            if (!loadedFiles.Contains("conquer.mix")) loadErrors.Add(prefix + "conquer.mix");
            if (!loadedFiles.Contains("desert.mix")) loadErrors.Add(prefix + "desert.mix");
            if (!loadedFiles.Contains("temperat.mix")) loadErrors.Add(prefix + "temperat.mix");
            if (!loadedFiles.Contains("winter.mix")) loadErrors.Add(prefix + "winter.mix");
            if (!mfm.FileExists("conquer.eng")) fileLoadErrors.Add(prefix + "conquer.eng");

            // Sole Survivor
            mfm.LoadArchive(GameType.SoleSurvivor, "local.mix", false);
            mfm.LoadArchive(GameType.SoleSurvivor, "cclocal.mix", false);
            // Mod addons
            mfm.LoadArchives(GameType.SoleSurvivor, "sc*.mix", false);
            mfm.LoadArchive(GameType.SoleSurvivor, "conquer.mix", false);
            // Sole Survivor Theaters
            mfm.LoadArchive(GameType.SoleSurvivor, "desert.mix", true);
            mfm.LoadArchive(GameType.SoleSurvivor, "temperat.mix", true);
            mfm.LoadArchive(GameType.SoleSurvivor, "winter.mix", true);
            // Check files
            if (!tdSsEqual)
            {
                mfm.Reset(GameType.SoleSurvivor, null);
                loadedFiles = mfm.ToList();
                prefix = "SS: ";
                if (!loadedFiles.Contains("local.mix") && !loadedFiles.Contains("cclocal.mix")) loadErrors.Add(prefix + "local.mix / cclocal.mix");
                if (!loadedFiles.Contains("conquer.mix")) loadErrors.Add(prefix + "conquer.mix");
                if (!loadedFiles.Contains("desert.mix")) loadErrors.Add(prefix + "desert.mix");
                if (!loadedFiles.Contains("temperat.mix")) loadErrors.Add(prefix + "temperat.mix");
                if (!loadedFiles.Contains("winter.mix")) loadErrors.Add(prefix + "winter.mix");
                if (!mfm.FileExists("conquer.eng")) fileLoadErrors.Add(prefix + "conquer.eng");
            }
            // Red Alert
            // Aftermath expand file. Required. Contains latest strings file.
            mfm.LoadArchive(GameType.RedAlert, "expand2.mix", false, false, false, true);
            // Counterstrike expand file. All graphics from expand are also in expand2.mix,
            // but it could be used in modding to override different files. Not considered vital.
            mfm.LoadArchive(GameType.RedAlert, "expand.mix", false, false, false, true);
            // Container archives.
            mfm.LoadArchive(GameType.RedAlert, "redalert.mix", false, true, false, true);
            mfm.LoadArchive(GameType.RedAlert, "main.mix", false, true, false, true);
            // Needed for theater palettes and the remap settings in palette.cps
            mfm.LoadArchive(GameType.RedAlert, "local.mix", false, false, true, true);
            // Not normally needed, but in the beta this contains palette.cps.
            mfm.LoadArchive(GameType.RedAlert, "general.mix", false, false, true, true);
            // Mod addons
            mfm.LoadArchives(GameType.RedAlert, "sc*.mix", true);
            // Main graphics archive
            mfm.LoadArchive(GameType.RedAlert, "conquer.mix", false, false, true, true);
            // Infantry
            mfm.LoadArchive(GameType.RedAlert, "lores.mix", false, false, true, true);
            // Expansion infantry
            mfm.LoadArchive(GameType.RedAlert, "lores1.mix", false, false, true, true);
            // Theaters
            mfm.LoadArchive(GameType.RedAlert, "temperat.mix", true, false, true, true);
            mfm.LoadArchive(GameType.RedAlert, "snow.mix", true, false, true, true);
            mfm.LoadArchive(GameType.RedAlert, "interior.mix", true, false, true, true);

            // Check files
            modpaths.TryGetValue(GameType.RedAlert, out string[] raModPaths);
            mfm.Reset(GameType.RedAlert, null);
            loadedFiles = mfm.ToList();
            prefix = "RA: ";
            if (!loadedFiles.Contains("expand2.mix")) loadErrors.Add(prefix + "expand2.mix");
            if (!loadedFiles.Contains("local.mix")) loadErrors.Add(prefix + "local.mix");
            if (!loadedFiles.Contains("conquer.mix")) loadErrors.Add(prefix + "conquer.mix");
            if (!loadedFiles.Contains("lores.mix")) loadErrors.Add(prefix + "lores.mix");
            if (!loadedFiles.Contains("lores1.mix")) loadErrors.Add(prefix + "lores1.mix");
            if (!loadedFiles.Contains("temperat.mix")) loadErrors.Add(prefix + "temperat.mix");
            if (!loadedFiles.Contains("snow.mix")) loadErrors.Add(prefix + "snow.mix");
            if (!loadedFiles.Contains("interior.mix")) loadErrors.Add(prefix + "interior.mix");
            if (!mfm.FileExists("palette.cps")) fileLoadErrors.Add(prefix + "palette.cps");
            if (!mfm.FileExists("conquer.eng")) fileLoadErrors.Add(prefix + "conquer.eng");
            mfm.Reset(GameType.None, null);

#if !DEVELOPER
            if (loadErrors.Count > 0)
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("Required data is missing or corrupt. The following mix files could not be opened:").Append('\n');
                string errors = String.Join("\n", loadErrors.ToArray());
                msg.Append(errors);
                MessageBox.Show(msg.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (fileLoadErrors.Count > 0)
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("Required data is missing or corrupt. The following data files could not be opened:").Append('\n');
                string errors = String.Join("\n", fileLoadErrors.ToArray());
                msg.Append(errors);
                MessageBox.Show(msg.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
#endif
            // Initialize texture, tileset, team color, and game text managers
            // TilesetManager: is the system graphics are requested from, possibly with house remap.
            Globals.TheTilesetManager = new TilesetManagerClassic(mfm);

            Globals.TheTeamColorManager = new TeamRemapManager(mfm);
            // All the same. Would introduce region-based language differences, but the French and German files are... also called "conquer.eng".
            Dictionary<GameType, String> gameStringsFiles = new Dictionary<GameType, string>();
            gameStringsFiles.Add(GameType.TiberianDawn, "conquer.eng");
            gameStringsFiles.Add(GameType.RedAlert, "conquer.eng");
            gameStringsFiles.Add(GameType.SoleSurvivor, "conquer.eng");
            GameTextManagerClassic gtm = new GameTextManagerClassic(mfm, gameStringsFiles);
            AddMissingClassicText(gtm);
            Globals.TheGameTextManager = gtm;
            return true;
        }

        private static bool LoadEditorRemastered(String runPath, Dictionary<GameType, string[]> modPaths)
        {
            // Initialize megafiles
            Dictionary<GameType, String> gameFolders = new Dictionary<GameType, string>();
            gameFolders.Add(GameType.TiberianDawn, "CNCDATA\\TIBERIAN_DAWN\\CD1");
            gameFolders.Add(GameType.RedAlert, "CNCDATA\\RED_ALERT\\AFTERMATH");
            gameFolders.Add(GameType.SoleSurvivor, "CNCDATA\\TIBERIAN_DAWN\\CD1");

            MegafileManager mfm = new MegafileManager(Path.Combine(runPath, Globals.MegafilePath), runPath, modPaths, gameFolders);
            var megafilesLoaded = true;
            megafilesLoaded &= mfm.LoadArchive("CONFIG.MEG");
            megafilesLoaded &= mfm.LoadArchive("TEXTURES_COMMON_SRGB.MEG");
            megafilesLoaded &= mfm.LoadArchive("TEXTURES_RA_SRGB.MEG");
            megafilesLoaded &= mfm.LoadArchive("TEXTURES_SRGB.MEG");
            megafilesLoaded &= mfm.LoadArchive("TEXTURES_TD_SRGB.MEG");
            // Classic main.mix and theater files, for template land type detection in RA.
            mfm.LoadArchiveClassic(GameType.RedAlert, "main.mix", false, true, false, true);
            mfm.LoadArchiveClassic(GameType.RedAlert, "temperat.mix", true, false, true, true);
            mfm.LoadArchiveClassic(GameType.RedAlert, "snow.mix", true, false, true, true);
            mfm.LoadArchiveClassic(GameType.RedAlert, "interior.mix", true, false, true, true);
#if !DEVELOPER
            if (!megafilesLoaded)
            {
                MessageBox.Show("Required data is missing or corrupt, please validate your installation.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
#endif
            Globals.TheArchiveManager = mfm;
            // Initialize texture, tileset, team color, and game text managers
            TextureManager txm = new TextureManager(Globals.TheArchiveManager);
            Globals.TheTilesetManager = new TilesetManager(Globals.TheArchiveManager, txm, Globals.TilesetsXMLPath, Globals.TexturesPath);
            Globals.TheTeamColorManager = new TeamColorManager(Globals.TheArchiveManager);

            // Text manager.
            var cultureName = CultureInfo.CurrentUICulture.Name;
            var gameTextFilename = string.Format(Globals.GameTextFilenameFormat, cultureName.ToUpper());
            if (!Globals.TheArchiveManager.FileExists(gameTextFilename))
            {
                gameTextFilename = string.Format(Globals.GameTextFilenameFormat, "EN-US");
            }
            GameTextManager gtm = new GameTextManager(Globals.TheArchiveManager, gameTextFilename);
            //gtm.Dump("alltext.txt");
            AddMissingRemasterText(gtm);
            Globals.TheGameTextManager = gtm;
            return true;
        }

        private static String GetRemasterRunPath(bool askIfNotFound)
        {
            // Do a test for CONFIG.MEG
            string runPath = Environment.CurrentDirectory;
            if (FileTest(runPath))
            {
                return runPath;
            }
            // If it does not exist, try to use the directory from the settings.
            bool validSavedDirectory = false;
            string savedPath = (Properties.Settings.Default.GameDirectoryPath ?? String.Empty).Trim();
            if (savedPath.Length > 0 && Directory.Exists(savedPath))
            {
                if (FileTest(savedPath))
                {
                    runPath = savedPath;
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
            if (!validSavedDirectory && askIfNotFound)
            {
                var gameInstallationPathForm = new GameInstallationPathForm();
                gameInstallationPathForm.StartPosition = FormStartPosition.CenterScreen;
                switch (gameInstallationPathForm.ShowDialog())
                {
                    case DialogResult.OK:
                        runPath = Path.GetDirectoryName(gameInstallationPathForm.SelectedPath);
                        Properties.Settings.Default.GameDirectoryPath = runPath;
                        Properties.Settings.Default.Save();
                        break;
                    case DialogResult.No: // No longer used; cancelling will always fall back to classic graphics.
                        return null;
                    case DialogResult.Cancel:
                        Globals.UseClassicFiles = true;
                        return null;
                }
            }
            return runPath;
        }

        private static void AddMissingRemasterText(IGameTextManager gtm)
        {
            // Buildings
            gtm["TEXT_STRUCTURE_TITLE_OIL_PUMP"] = "Oil Pump";
            gtm["TEXT_STRUCTURE_TITLE_OIL_TANKER"] = "Oil Tanker";
            String fake = " (" + gtm["TEXT_UI_FAKE"] + ")";
            if (!gtm["TEXT_STRUCTURE_RA_WEAF"].EndsWith(fake)) gtm["TEXT_STRUCTURE_RA_WEAF"] += fake;
            if (!gtm["TEXT_STRUCTURE_RA_FACF"].EndsWith(fake)) gtm["TEXT_STRUCTURE_RA_FACF"] += fake;
            if (!gtm["TEXT_STRUCTURE_RA_SYRF"].EndsWith(fake)) gtm["TEXT_STRUCTURE_RA_SYRF"] += fake;
            if (!gtm["TEXT_STRUCTURE_RA_SPEF"].EndsWith(fake)) gtm["TEXT_STRUCTURE_RA_SPEF"] += fake;
            if (!gtm["TEXT_STRUCTURE_RA_DOMF"].EndsWith(fake)) gtm["TEXT_STRUCTURE_RA_DOMF"] += fake;
            // Overlay
            gtm["TEXT_OVERLAY_CONCRETE_PAVEMENT"] = "Concrete";
            gtm["TEXT_OVERLAY_CONCRETE_ROAD"] = "Concrete Road";
            gtm["TEXT_OVERLAY_CONCRETE_ROAD_FULL"] = "Concrete Road (full)";
            gtm["TEXT_OVERLAY_TIBERIUM"] = "Tiberium";
            // Sole Survivor Teleporter
            gtm["TEXT_OVERLAY_TELEPORTER"] = "Teleporter";
            // Haystacks. Extra ID added for classic support. Remaster only has the string "Haystack", so we'll just copy it.
            gtm["TEXT_STRUCTURE_TITLE_CIV12B"] = gtm["TEXT_STRUCTURE_TITLE_CIV12"];
            // "Gold" exists as "TEXT_CURRENCY_TACTICAL"
            gtm["TEXT_OVERLAY_GEMS"] = "Gems";
            gtm["TEXT_OVERLAY_WCRATE"] = "Wood Crate";
            gtm["TEXT_OVERLAY_SCRATE"] = "Steel Crate";
            gtm["TEXT_OVERLAY_WATER_CRATE"] = "Water Crate";
            // Smudge
            gtm["TEXT_SMUDGE_CRATER"] = "Crater";
            gtm["TEXT_SMUDGE_SCORCH"] = "Scorch Mark";
            gtm["TEXT_SMUDGE_BIB"] = "Road Bib";
        }

        private static void AddMissingClassicText(IGameTextManager gtm)
        {
            // Classic game text manager does not clear these extra strings when resetting the strings table.
            // TD Overlay
            gtm["TEXT_OVERLAY_CONCRETE_ROAD"] = "Concrete Road";
            gtm["TEXT_OVERLAY_CONCRETE_ROAD_FULL"] = "Concrete Road (full)";
            // Sole Survivor Teleporter
            gtm["TEXT_OVERLAY_TELEPORTER"] = "Teleporter";
            // TD Terrain
            gtm["TEXT_PROP_TITLE_CACTUS"] = "Cactus";
            // RA Misc
            gtm["TEXT_UI_FAKE"] = "FAKE";
            // RA ants
            gtm["TEXT_UNIT_RA_ANT1"] = "Warrior Ant";
            gtm["TEXT_UNIT_RA_ANT2"] = "Fire Ant";
            gtm["TEXT_UNIT_RA_ANT3"] = "Scout Ant";
            gtm["TEXT_STRUCTURE_RA_QUEE"] = "Queen Ant";
            gtm["TEXT_STRUCTURE_RA_LAR1"] = "Larva";
            gtm["TEXT_STRUCTURE_RA_LAR2"] = "Larvae";
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
