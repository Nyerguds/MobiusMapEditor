//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// The Command & Conquer Map Editor and corresponding source code is distributed
// in the hope that it will be useful, but with permitted additional restrictions
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT
// distributed with this program. You should have received a copy of the
// GNU General Public License along with permitted additional restrictions
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
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

    /// <summary>
    /// This class grew out of the original Program.cs file, but pretty much everything
    //  in here is new besides the original remaster loading logic.
    /// </summary>
    public static class StartupLoader
    {
        private const string DEFAULT_CULTURE = "EN-US";

        public static string[] GetModPaths(string steamId, string modstoLoad, string modsFolder, string modIdentifier)
        {
            Regex numbersOnly = new Regex("^\\d+$");
            Regex modregex = new Regex("\"game_type\"\\s*:\\s*\"" + modIdentifier + "\"");
            const string contentFile = "ccmod.json";
            string[] steamLibraryFolders = SteamAssist.GetLibraryFoldersForAppId(steamId);
            string[] mods = (modstoLoad ?? String.Empty).Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            // Not using Dictionary<> because I want to preserve the order of the folders.
            HashSet<string> foundMods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> modPaths = new List<string>();
            for (int i = 0; i < mods.Length; ++i)
            {
                string modDef = mods[i].Trim();
                if (String.IsNullOrEmpty(modDef) || foundMods.Contains(modDef))
                {
                    continue;
                }
                string addonModPath;
                // Lookup by Steam ID. Only happens if it's numeric.
                if (numbersOnly.IsMatch(modDef) && steamLibraryFolders != null)
                {
                    addonModPath = SteamAssist.GetWorkshopFolder(steamLibraryFolders, steamId, modDef, contentFile, null, out string addonModBasePath);
                    //SteamAssist.TryGetSteamWorkshopFolder(steamId, modDef, contentFile, null);
                    if (addonModPath != null)
                    {
                        addonModPath = Path.GetFullPath(addonModPath);
                        addonModBasePath = Path.GetFullPath(addonModBasePath);
                        string modId = addonModPath.Substring(addonModBasePath.Length).Trim(new[] { '/', '\\' });
                        if (CheckAddonPathModType(addonModPath, contentFile, modregex) && !String.IsNullOrEmpty(modId) && !foundMods.Contains(modId))
                        {
                            foundMods.Add(modId);
                            modPaths.Add(addonModPath);
                        }
                        continue;
                    }
                }
                // Lookup by folder name. Look in local mod folders (under user 'Documents') first.
                addonModPath = Path.Combine(modsFolder, modDef);
                if (CheckAddonPathModType(addonModPath, contentFile, modregex))
                {
                    foundMods.Add(modDef);
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
                            // Don't need to check on 'foundMods'; it's done at the loop start.
                            foundMods.Add(modDef);
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

        public static string GetRemasterRunPath(string gameId, bool askIfNotFound)
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
                    string exeFile = Path.GetFileName(Program.ApplicationPath);
                    string labelInfo = String.Format(Program.ClassicInstructions, exeFile, Program.ClassicSetting);
                    gameInstallationPathForm.StartPosition = FormStartPosition.CenterScreen;
                    gameInstallationPathForm.LabelInfo = labelInfo;
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

        public static bool LoadEditorRemastered(String runPath, Dictionary<GameType, string[]> modPaths, MixFileNameGenerator romfis)
        {
            // Initialize megafiles
            Dictionary<GameType, String> gameFolders = new Dictionary<GameType, string>();
            GameInfo[] gameTypeInfo = GameTypeFactory.GetGameInfos();
            foreach (GameInfo gameInfo in gameTypeInfo)
            {
                if (gameInfo != null)
                {
                    gameFolders.Add(gameInfo.GameType, gameInfo.ClassicFolderRemasterData);
                }
            }
            MegafileManager mfm = new MegafileManager(Path.Combine(runPath, Globals.MegafilePath), runPath, modPaths, romfis, gameFolders);
            HashSet<string> remasterFilesFound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> remasterFilesToLoad = new List<string>();
            foreach (GameInfo gameInfo in gameTypeInfo)
            {
                if (gameInfo == null)
                {
                    continue;
                }
                string[] files = gameInfo.RemasterMegFiles;
                foreach (string file in files)
                {
                    if (!remasterFilesFound.Contains(file))
                    {
                        remasterFilesToLoad.Add(file);
                        remasterFilesFound.Add(file);
                    }
                }
            }
            List<string> megFileLoadErrors = new List<string>();
            foreach (string remFile in remasterFilesToLoad)
            {
                if (!mfm.LoadArchive(remFile))
                {
                    megFileLoadErrors.Add(remFile);
                }
            }
            if (megFileLoadErrors.Count > 0)
            {
                StringBuilder msg = new StringBuilder();
                string arg = megFileLoadErrors.Count == 1 ? String.Empty : "s";
                msg.Append(String.Format("Required data is missing or corrupt; please validate your installation. The following file{0} could not be opened:\n", arg));
                string errors = String.Join("\n", megFileLoadErrors.ToArray());
                msg.Append(errors);
                MessageBox.Show(msg.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            // Classic main.mix and theater files, for rules reading and template land type detection in RA.
            List<string> loadErrors = new List<string>();
            List<string> fileLoadErrors = new List<string>();
            foreach (GameInfo gameInfo in gameTypeInfo)
            {
                if (gameInfo != null)
                {
                    gameInfo.InitClassicFiles(mfm.ClassicFileManager, loadErrors, fileLoadErrors, true);
                }
            }
            mfm.Reset(GameType.None, null);
            if (loadErrors.Count > 0)
            {
                StringBuilder msg = new StringBuilder();
                string arg = loadErrors.Count == 1 ? String.Empty : "s";
                msg.Append(String.Format("Required classic data is missing or corrupt; please validate your installation. The following mix file{0} could not be opened:\n", arg));
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
            Globals.TheShapeCacheManager = new ShapeCacheManager();
            foreach (GameInfo gameInfo in gameTypeInfo)
            {
                if (gameInfo != null)
                {
                    foreach (TheaterType theater in gameInfo.AllTheaters)
                    {
                        theater.IsRemasterTilesetFound = tsm.TilesetExists(theater.MainTileset);
                    }
                }
            }
            Globals.TheTeamColorManager = new TeamColorManager(mfm);
            // Text manager.

            string cultureName = (Globals.EditorLanguage ?? String.Empty).Trim().ToUpperInvariant();
            switch (cultureName)
            {
                case "AUTO":
                    cultureName = FindCompatibleCulture(mfm);
                    break;
                case "":
                case "NONE":
                case "DEFAULT":
                    cultureName = DEFAULT_CULTURE;
                    break;
            }
            string gameTextFilename = string.Format(Globals.GameTextFilenameFormat, cultureName.ToUpper());
            if (!Globals.TheArchiveManager.FileExists(gameTextFilename))
            {
                cultureName = DEFAULT_CULTURE;
                gameTextFilename = string.Format(Globals.GameTextFilenameFormat, cultureName.ToUpper());
            }
            GameTextManager gtm = new GameTextManager(Globals.TheArchiveManager, gameTextFilename);
            //gtm.Dump(Path.Combine(Program.ApplicationPath, "alltext.txt"));
            AddMissingRemasterText(gtm);
            Globals.TheGameTextManager = gtm;
            return true;
        }

        private static string FindCompatibleCulture(MegafileManager mfm)
        {
            string currentCulture = CultureInfo.CurrentUICulture.Name.ToUpper();
            if (mfm.FileExists(String.Format(Globals.GameTextFilenameFormat, currentCulture)))
            {
                return currentCulture;
            }
            Regex stringsFileMatch = new Regex(
                String.Format(Regex.Escape(Globals.GameTextFilenameFormat).Replace(Regex.Escape("{0}"), "{0}"),
                                "([a-zA-Z\\-]+)"), RegexOptions.Compiled);
            List<string> supportedCultures = new List<string>();
            foreach (string filename in mfm)
            {
                Match match = stringsFileMatch.Match(filename);
                if (match.Success)
                {
                    supportedCultures.Add(match.Groups[1].Value.ToUpperInvariant());
                }
            }
            if (supportedCultures.Contains(currentCulture))
            {
                return currentCulture;
            }
            List<string> languages = new List<string>();
            string lang = currentCulture.Split('-')[0];
            languages.Add(lang);
            if (!"EN".Equals(lang))
                languages.Add("EN");
            foreach (string language in languages)
            {
                foreach (string cultureName in supportedCultures)
                {
                    string cultureLang = cultureName.Split('-')[0];
                    if (language.Equals(cultureLang))
                        return cultureName;
                }
            }
            return DEFAULT_CULTURE;
        }

        public static bool LoadEditorClassic(string applicationPath, Dictionary<GameType, string[]> modpaths, MixFileNameGenerator romfis)
        {
            // The system should scan all mix archives for known filenames of other mix archives so it can do recursive searches.
            // Mix files should be given in order or depth, so first give ones that are in the folder, then ones that may occur inside others.
            // The order of load determines the file priority; only the first found occurrence of a file is used.
            GameInfo[] gameTypeInfo = GameTypeFactory.GetGameInfos();
            Dictionary<GameType, string> gameFolders = new Dictionary<GameType, string>();
            foreach (GameInfo gameInfo in gameTypeInfo)
            {
                if (gameInfo == null)
                {
                    continue;
                }
                string path = gameInfo.ClassicFolder;
                string pathFull = Path.GetFullPath(Path.Combine(applicationPath, gameInfo.ClassicFolder));
                if (!Directory.Exists(pathFull))
                {
                    // Revert to default.
                    path = gameInfo.ClassicFolderDefault;
                    pathFull = Path.GetFullPath(Path.Combine(applicationPath, path));
                    if (!Directory.Exists(pathFull))
                    {
                        // As last-ditch effort, try to see if applicationPath is the remastered game folder.
                        pathFull = Path.GetFullPath(Path.Combine(applicationPath, gameInfo.ClassicFolderRemasterData));
                        if (Directory.Exists(pathFull))
                        {
                            path = gameInfo.ClassicFolderRemasterData;
                        }
                    }
                }
                gameFolders.Add(gameInfo.GameType, path);
            }
            // Check files
            MixfileManager mfm = new MixfileManager(applicationPath, romfis, gameFolders, modpaths);
            List<string> loadErrors = new List<string>();
            List<string> fileLoadErrors = new List<string>();
            foreach (GameInfo gameInfo in gameTypeInfo)
            {
                if (gameInfo != null)
                {
                    gameInfo.InitClassicFiles(mfm, loadErrors, fileLoadErrors, false);
                }
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
            Globals.TheShapeCacheManager = new ShapeCacheManager();
            Globals.TheTeamColorManager = new TeamRemapManager(mfm);
            Dictionary<GameType, String> gameStringsFiles = new Dictionary<GameType, string>();
            foreach (GameInfo gameInfo in gameTypeInfo)
            {
                if (gameInfo != null)
                {
                    gameStringsFiles.Add(gameInfo.GameType, gameInfo.ClassicStringsFile);
                }
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
        public static void TestMixExists(HashSet<string> loadedFiles, List<string> errors, string prefix, params string[] fileNames)
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
        public static void TestMixExists(HashSet<string> loadedFiles, List<string> errors, string prefix, TheaterType toInit, bool giveError)
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
        public static void TestMixExists(HashSet<string> loadedFiles, List<string> errors, string prefix, TheaterType toInit, bool giveError, params string[] fileNames)
        {
            bool anyExist = false;
            foreach (string fileName in fileNames)
            {
                if (loadedFiles.Contains(fileName))
                {
                    anyExist = true;
                    break;
                }
            }
            if (toInit != null)
            {
                toInit.IsClassicMixFound = anyExist;
            }
            if (!anyExist && giveError && errors != null)
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

        /// <summary>
        /// Adds / corrects strings that are either missing or incomplete in the Remaster for their intended use
        /// in the map editor.
        /// </summary>
        /// <param name="gtm">The game text manager to apply these changes on.</param>
        private static void AddMissingRemasterText(IGameTextManager gtm)
        {
            // == Buildings ==
            string fake = " (" + gtm["TEXT_UI_FAKE"] + ")";
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
            gtm["TEXT_OVERLAY_SQUISH_MARK"] = "Squish mark";
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

        /// <summary>
        /// Adds strings that are missing in the classic files. Note that unlike for Remastered text,
        /// the strings in this function cannot be composed from other strings; the actual strings files differ
        /// per game, and this function is called before any maps are opened, meaning no game is chosen yet,
        /// and no actual game files are loaded.
        /// </summary>
        /// <param name="gtm">The game text manager to apply these changes on.</param>
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
