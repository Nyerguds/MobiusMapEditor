using MobiusEditor.Interface;
using MobiusEditor.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MobiusEditor.Utility
{
    public static class StartupLoader
    {

        public static string[] GetModPaths(string steamId, string modstoLoad, string modsFolder, string modIdentifier)
        {
            Regex numbersOnly = new Regex("^\\d+$");
            Regex modregex = new Regex("\"game_type\"\\s*:\\s*\"" + modIdentifier + "\"");
            const string contentFile = "ccmod.json";
            string[] steamLibraryFolders = SteamAssist.GetLibraryFoldersForAppId(steamId);
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
                // Lookup by Steam ID. Only happens if it's numeric.
                if (numbersOnly.IsMatch(modDef))
                {
                    addonModPath = SteamAssist.TryGetSteamWorkshopFolder(steamId, modDef, contentFile, null);
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
                addonModPath = Path.Combine(modsFolder, modDef);
                if (CheckAddonPathModType(addonModPath, contentFile, modregex))
                {
                    modPaths.Add(addonModPath);
                    // Found in local mods; don't check Steam ones.
                    continue;
                }
                if (steamLibraryFolders == null)
                {
                    continue;
                }
                // try to find mod in steam library.
                foreach (string libFolder in steamLibraryFolders)
                {
                    string modPath = Path.Combine(libFolder, "steamapps", "workshop", "content", steamId);
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
            return modPaths.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
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

        public static String GetRemasterRunPath(string gameId, bool askIfNotFound)
        {
            // Do a test for CONFIG.MEG
            string runPath = null;
            if (RemasterFileTest(Environment.CurrentDirectory))
            {
                return Environment.CurrentDirectory;
            }
            // If it does not exist, try to use the directory from the settings.
            bool validSavedDirectory = false;
            string savedPath = (Properties.Settings.Default.GameDirectoryPath ?? String.Empty).Trim();
            if (savedPath.Length > 0 && Directory.Exists(savedPath))
            {
                if (RemasterFileTest(savedPath))
                {
                    runPath = savedPath;
                    validSavedDirectory = true;
                }
            }
            // Before showing a dialog to ask, try to autodetect the Steam path.
            if (!validSavedDirectory)
            {
                string gameFolder = null;
                try
                {
                    gameFolder = SteamAssist.TryGetSteamGameFolder(gameId, "TiberianDawn.dll", "RedAlert.dll");
                }
                catch { /* ignore */ }
                if (gameFolder != null)
                {
                    if (RemasterFileTest(gameFolder))
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
                using (GameInstallationPathForm gameInstallationPathForm = new GameInstallationPathForm())
                {
                    gameInstallationPathForm.StartPosition = FormStartPosition.CenterScreen;
                    gameInstallationPathForm.LabelInfo =
                        "To skip this dialog and always start with the classic graphics, edit CnCTDRAMapEditor.exe.config and set the \"UseClassicFiles\" setting to True.";
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
            }
            return runPath;
        }

        private static bool RemasterFileTest(String basePath)
        {
            return File.Exists(Path.Combine(basePath, "DATA", "CONFIG.MEG"));
        }

        public static bool LoadEditorRemastered(String runPath, Dictionary<GameType, string[]> modPaths)
        {
            // Initialize megafiles
            Dictionary<GameType, String> gameFolders = new Dictionary<GameType, string>();
            foreach (GameInfo gi in GameTypeFactory.GetGameInfos())
            {
                gameFolders.Add(gi.GameType, gi.ClassicFolderRemaster);
            }
            MegafileManager mfm = new MegafileManager(Path.Combine(runPath, Globals.MegafilePath), runPath, modPaths, gameFolders);
            var megafilesLoaded = true;
            megafilesLoaded &= mfm.LoadArchive("CONFIG.MEG");
            megafilesLoaded &= mfm.LoadArchive("TEXTURES_COMMON_SRGB.MEG");
            megafilesLoaded &= mfm.LoadArchive("TEXTURES_RA_SRGB.MEG");
            megafilesLoaded &= mfm.LoadArchive("TEXTURES_SRGB.MEG");
            megafilesLoaded &= mfm.LoadArchive("TEXTURES_TD_SRGB.MEG");
            if (!megafilesLoaded)
            {
                MessageBox.Show("Required data is missing or corrupt; please validate your installation.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            // Classic main.mix and theater files, for rules reading and template land type detection in RA.
            List<string> loadErrors = new List<string>();
            List<string> fileLoadErrors = new List<string>();
            GameInfo[] gameTypeInfo = GameTypeFactory.GetGameInfos();
            foreach (GameInfo gic in gameTypeInfo)
            {
                gic.InitClassicFiles(mfm.ClassicFileManager, loadErrors, fileLoadErrors, true);
            }
            mfm.Reset(GameType.None, null);
            if (loadErrors.Count > 0)
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("Required classic data is missing or corrupt; please validate your installation. The following mix files could not be opened:\n");
                string errors = String.Join("\n", loadErrors.ToArray());
                msg.Append(errors);
                MessageBox.Show(msg.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (fileLoadErrors.Count > 0)
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("Required classic data is missing or corrupt; please validate your installation. The following data files could not be opened:\n");
                string errors = String.Join("\n", fileLoadErrors.ToArray());
                msg.Append(errors);
                MessageBox.Show(msg.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            Globals.TheArchiveManager = mfm;
            // Initialize tileset, team color, and game text managers
            TilesetManager tsm = new TilesetManager(mfm, Globals.TilesetsXMLPath, Globals.TexturesPath);
            Globals.TheTilesetManager = tsm;
            foreach (GameInfo gic in gameTypeInfo)
            {
                foreach (TheaterType theater in gic.AllTheaters)
                {
                    theater.IsRemasterTilesetFound = tsm.TilesetExists(theater.MainTileset);
                }
            }
            Globals.TheTeamColorManager = new TeamColorManager(mfm);
            // Text manager.
            const string fallbackCulture = "EN-US";
            string gameTextFilename = null;
            string forcedLanguage = (Globals.ForceLanguage ?? String.Empty).Trim().ToUpperInvariant();
            // Force to setting.
            if (!String.IsNullOrEmpty(forcedLanguage) && !"NONE".Equals(forcedLanguage, StringComparison.OrdinalIgnoreCase))
            {
                gameTextFilename = string.Format(Globals.GameTextFilenameFormat, forcedLanguage);
            }
            // Not forced, or not found: fall back to system language.
            if (gameTextFilename == null || !Globals.TheArchiveManager.FileExists(gameTextFilename))
            {
                var cultureName = CultureInfo.CurrentUICulture.Name;
                gameTextFilename = string.Format(Globals.GameTextFilenameFormat, cultureName.ToUpper());
            }
            // Not found: fall back to default English.
            if (gameTextFilename == null || !Globals.TheArchiveManager.FileExists(gameTextFilename))
            {
                gameTextFilename = string.Format(Globals.GameTextFilenameFormat, fallbackCulture.ToUpper());
            }
            GameTextManager gtm = new GameTextManager(Globals.TheArchiveManager, gameTextFilename);
            //gtm.Dump(Path.Combine(Program.ApplicationPath, "alltext.txt"));
            AddMissingRemasterText(gtm);
            Globals.TheGameTextManager = gtm;
            return true;
        }

        public static bool LoadEditorClassic(string applicationPath, Dictionary<GameType, string[]> modpaths)
        {
            // The system should scan all mix archives for known filenames of other mix archives so it can do recursive searches.
            // Mix files should be given in order or depth, so first give ones that are in the folder, then ones that may occur inside others.
            // The order of load determines the file priority; only the first found occurrence of a file is used.
            GameType[] gameTypes = GameTypeFactory.GetGameTypes();
            GameInfo[] gameTypeInfo = new GameInfo[gameTypes.Length];
            Dictionary<GameType, String> gameFolders = new Dictionary<GameType, string>();
            foreach (GameType gi in gameTypes)
            {
                GameInfo gic = GameTypeFactory.GetGameInfo(gi);
                gameTypeInfo[(int)gic.GameType] = gic;
                String path = gic.ClassicFolder;
                String pathFull = Path.GetFullPath(Path.Combine(applicationPath, gic.ClassicFolder));
                if (!Directory.Exists(pathFull))
                {
                    // Revert to default.
                    path = gic.ClassicFolderDefault;
                    pathFull = Path.GetFullPath(Path.Combine(applicationPath, path));
                    if (!Directory.Exists(pathFull))
                    {
                        // As last-ditch effort, try to see if applicationPath is the remastered game folder.
                        pathFull = Path.GetFullPath(Path.Combine(applicationPath, gic.ClassicFolderRemaster));
                        if (Directory.Exists(pathFull))
                        {
                            path = gic.ClassicFolderRemaster;
                        }
                    }
                }
                gameFolders.Add(gi, path);
            }
            // Check files
            MixfileManager mfm = new MixfileManager(applicationPath, gameFolders, modpaths);
            List<string> loadErrors = new List<string>();
            List<string> fileLoadErrors = new List<string>();
            foreach (GameType gi in gameTypes)
            {
                GameInfo gic = gameTypeInfo[(int)gi];
                gic.InitClassicFiles(mfm, loadErrors, fileLoadErrors, false);
            }
            mfm.Reset(GameType.None, null);
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
            Globals.TheArchiveManager = mfm;
            // Initialize texture, tileset, team color, and game text managers
            Globals.TheTilesetManager = new TilesetManagerClassic(mfm);
            Globals.TheTeamColorManager = new TeamRemapManager(mfm);
            Dictionary<GameType, String> gameStringsFiles = new Dictionary<GameType, string>();
            foreach (GameType gi in gameTypes)
            {
                GameInfo gic = gameTypeInfo[(int)gi];
                gameStringsFiles.Add(gic.GameType, gic.ClassicStringsFile);
            }
            GameTextManagerClassic gtm = new GameTextManagerClassic(mfm, gameStringsFiles);
            AddMissingClassicText(gtm);
            Globals.TheGameTextManager = gtm;
            return true;
        }


        /// <summary>
        /// Loads the mix file for a theater.
        /// </summary>
        /// <param name="mfm">The mixfile manager</param>
        /// <param name="game">Game the theater belongs to.</param>
        /// <param name="theater">Theater object.</param>
        /// <param name="forRa">True if this file can use the new mix format, and can be embedded in another archive.</param>
        /// <returns>True if the mix file was found as bare file.</returns>
        public static bool LoadClassicTheater(MixfileManager mfm, GameType game, TheaterType theater, bool forRa)
        {
            if (theater == null)
            {
                return false;
            }
            string name = theater.ClassicTileset + ".mix";
            return mfm.LoadArchive(game, name, true, false, forRa, forRa);
        }

        /// <summary>
        /// Tests if a mix file, or allowed equivalents of the mix file, exist.
        /// </summary>
        /// <param name="loadedFiles">The list of mix files loaded by the mixfile manager</param>
        /// <param name="errors">Current list of errors to potentially add more to.</param>
        /// <param name="prefix">Prefix string to put before the filename on the newly added error line.</param>
        /// <param name="fileNames">One or more mix files to check. If multiple are given they are seen as interchangeable; if one exists in <paramref name="loadedFiles"/>, the check passes.</param>
        public static void TestMixExists(List<string> loadedFiles, List<string> errors, string prefix, params string[] fileNames)
        {
            TestMixExists(loadedFiles, errors, prefix, null, true, fileNames);
        }

        /// <summary>
        /// Tests if a theater mix file exist.
        /// </summary>
        /// <param name="loadedFiles">The list of mix files loaded by the mixfile manager.</param>
        /// <param name="errors">Current list of errors to potentially add more to.</param>
        /// <param name="prefix">Prefix string to put before the filename on the newly added error line.</param>
        /// <param name="toInit">The theater for which the mix archive ahould be checked. Marks in the theater info whether it was found.</param>
        /// <param name="giveError">True to add an error in <paramref name="errors"/> if the file is not present.</param>
        public static void TestMixExists(List<string> loadedFiles, List<string> errors, string prefix, TheaterType toInit, bool giveError)
        {
            if (toInit == null)
            {
                return;
            }
            string name = toInit.ClassicTileset + ".mix";
            TestMixExists(loadedFiles, errors, prefix, toInit, giveError, name);
        }

        /// <summary>
        /// Tests if a mix file, or allowed equivalents of the mix file, exist.
        /// </summary>
        /// <param name="loadedFiles">The list of mix files loaded by the mixfile manager.</param>
        /// <param name="errors">Current list of errors to potentially add more to.</param>
        /// <param name="prefix">Prefix string to put before the filename on the newly added error line.</param>
        /// <param name="toInit">If given, indicates that this mix file is the theater archive of a specific theater. Marks in the theater info whether it was found.</param>
        /// <param name="giveError">True to add an error in <paramref name="errors"/> if the file is not present.</param>
        /// <param name="fileNames">One or more mix files to check. If multiple are given they are seen as interchangeable; if one exists in <paramref name="loadedFiles"/>, the check passes.</param>
        public static void TestMixExists(List<string> loadedFiles, List<string> errors, string prefix, TheaterType toInit, bool giveError, params string[] fileNames)
        {
            bool anyExist = false;
            foreach (string fileName in fileNames)
            {
                if (loadedFiles.Contains(fileName))
                {
                    anyExist = true;
                }
            }
            if (toInit != null)
            {
                toInit.IsClassicMixFound = anyExist;
            }
            if (!anyExist && giveError)
            {
                errors.Add(prefix + String.Join(" / ", fileNames));
            }
        }

        /// <summary>
        /// Tests if a file can be found in the currently loaded mix archives.
        /// </summary>
        /// <param name="mixFileManager">Mix file manager</param>
        /// <param name="errors">Current list of errors to potentially add more to.</param>
        /// <param name="prefix">Prefix string to put before the filename on the newly added error line.</param>
        /// <param name="fileName">file name to check.</param>
        public static void TestFileExists(MixfileManager mixFileManager, List<string> errors, string prefix, string fileName)
        {
            if (!mixFileManager.FileExists(fileName))
            {
                errors.Add(prefix + fileName);
            }
        }

        private static void AddMissingRemasterText(IGameTextManager gtm)
        {
            // == Buildings ==
            String fake = " (" + gtm["TEXT_UI_FAKE"] + ")";
            if (!gtm["TEXT_STRUCTURE_RA_WEAF"].EndsWith(fake)) gtm["TEXT_STRUCTURE_RA_WEAF"] += fake;
            if (!gtm["TEXT_STRUCTURE_RA_FACF"].EndsWith(fake)) gtm["TEXT_STRUCTURE_RA_FACF"] += fake;
            if (!gtm["TEXT_STRUCTURE_RA_SYRF"].EndsWith(fake)) gtm["TEXT_STRUCTURE_RA_SYRF"] += fake;
            if (!gtm["TEXT_STRUCTURE_RA_SPEF"].EndsWith(fake)) gtm["TEXT_STRUCTURE_RA_SPEF"] += fake;
            if (!gtm["TEXT_STRUCTURE_RA_DOMF"].EndsWith(fake)) gtm["TEXT_STRUCTURE_RA_DOMF"] += fake;
            // == Civilian buildings ==
            gtm["TEXT_STRUCTURE_TITLE_OIL_PUMP"] = "Oil Pump";
            gtm["TEXT_STRUCTURE_TITLE_OIL_TANKER"] = "Oil Tanker";
            // Church. Extra ID added for classic support. String exists only once in the Remaster, but twice in the classic games.
            gtm["TEXT_STRUCTURE_TITLE_CIV1B"] = gtm["TEXT_STRUCTURE_TITLE_CIV1"];
            // Haystacks. Extra ID added for classic support. Remaster only has the string "Haystack", so we'll just copy it.
            gtm["TEXT_STRUCTURE_TITLE_CIV12B"] = gtm["TEXT_STRUCTURE_TITLE_CIV12"];
            // == Overlay ==
            gtm["TEXT_OVERLAY_CONCRETE_PAVEMENT"] = "Concrete";
            gtm["TEXT_OVERLAY_ROAD"] = "Road";
            gtm["TEXT_OVERLAY_ROAD_FULL"] = "Road (full)";
            gtm["TEXT_OVERLAY_TIBERIUM"] = "Tiberium";
            // Sole Survivor Teleporter
            gtm["TEXT_OVERLAY_TELEPORTER"] = "Teleporter";
            // "Gold" exists as "TEXT_CURRENCY_TACTICAL", so it does not need to be added.
            gtm["TEXT_OVERLAY_GEMS"] = "Gems";
            gtm["TEXT_OVERLAY_WCRATE"] = "Wood Crate";
            gtm["TEXT_OVERLAY_SCRATE"] = "Steel Crate";
            gtm["TEXT_OVERLAY_WATER_CRATE"] = "Water Crate";
            // == Terrain ==
            gtm["TEXT_PROP_TITLE_TREES"] = "Trees";
            // == Smudge ==
            gtm["TEXT_SMUDGE_CRATER"] = "Crater";
            gtm["TEXT_SMUDGE_SCORCH"] = "Scorch Mark";
            gtm["TEXT_SMUDGE_BIB"] = "Road Bib";
        }

        private static void AddMissingClassicText(IGameTextManager gtm)
        {
            // Classic game text manager does not clear these extra strings when resetting the strings table.
            // TD Overlay
            gtm["TEXT_OVERLAY_ROAD_FULL"] = "Road (full)";
            // Sole Survivor Teleporter
            gtm["TEXT_OVERLAY_TELEPORTER"] = "Teleporter";
            // TD Terrain
            gtm["TEXT_PROP_TITLE_CACTUS"] = "Cactus";
            // Terrain general
            gtm["TEXT_PROP_TITLE_TREES"] = "Trees";
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

    }
}
