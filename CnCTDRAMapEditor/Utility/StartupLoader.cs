using MobiusEditor.Interface;
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

        public static string[] GetModPaths(string steamId, string modstoLoad, string modFolder, string modIdentifier)
        {
            Regex numbersOnly = new Regex("^\\d+$");
            Regex modregex = new Regex("\"game_type\"\\s*:\\s*\"" + modIdentifier + "\"");
            const string contentFile = "ccmod.json";
            string modsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CnCRemastered", "Mods");
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
                addonModPath = Path.Combine(modsFolder, modFolder, modDef);
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
            if (!megafilesLoaded)
            {
                MessageBox.Show("Required data is missing or corrupt; please validate your installation.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            // Classic main.mix and theater files, for rules reading and template land type detection in RA.
            List<string> loadErrors = new List<string>();
            List<string> fileLoadErrors = new List<string>();
            InitClassicFilesRa(mfm.ClassicFileManager, loadErrors, fileLoadErrors, true);
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
            Globals.TheTilesetManager = new TilesetManager(mfm, Globals.TilesetsXMLPath, Globals.TexturesPath);
            Globals.TheTeamColorManager = new TeamColorManager(mfm);

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

        public static bool LoadEditorClassic(string applicationPath, Dictionary<GameType, string[]> modpaths)
        {
            // The system should scan all mix archives for known filenames of other mix archives so it can do recursive searches.
            // Mix files should be given in order or depth, so first give ones that are in the folder, then ones that may occur inside others.
            // The order of load determines the file priority; only the first found occurrence of a file is used.
            String tdPath = Properties.Settings.Default.ClassicPathTD;
            String tdPathFull = Path.GetFullPath(Path.Combine(applicationPath, tdPath));
            if (!Directory.Exists(tdPathFull))
            {
                tdPath = "Classic\\TD\\";
                tdPathFull = Path.GetFullPath(Path.Combine(applicationPath, tdPath));
            }
            String raPath = Properties.Settings.Default.ClassicPathRA;
            String raPathFull = Path.GetFullPath(Path.Combine(applicationPath, raPath));
            if (!Directory.Exists(raPathFull))
            {
                raPath = "Classic\\RA\\";
                raPathFull = Path.GetFullPath(Path.Combine(applicationPath, raPath));
            }
            String ssPath = Properties.Settings.Default.ClassicPathSS;
            String ssPathFull = Path.GetFullPath(Path.Combine(applicationPath, ssPath));
            if (!Directory.Exists(ssPathFull))
            {
                ssPath = "Classic\\TD\\";
                ssPathFull = Path.GetFullPath(Path.GetFullPath(Path.Combine(applicationPath, ssPath)));
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
            // Check files
            modpaths.TryGetValue(GameType.TiberianDawn, out string[] tdModPaths);
            modpaths.TryGetValue(GameType.SoleSurvivor, out string[] ssModPaths);
            bool tdSsEqual = ssModPaths.SequenceEqual(tdModPaths) && tdPathFull.Equals(ssPathFull);
            MixfileManager mfm = new MixfileManager(applicationPath, gameFolders, modpaths);
            List<string> loadErrors = new List<string>();
            List<string> fileLoadErrors = new List<string>();
            InitClassicFilesTdSs(mfm, tdSsEqual, loadErrors, fileLoadErrors);
            InitClassicFilesRa(mfm, loadErrors, fileLoadErrors, false);
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

        private static void InitClassicFilesTdSs(MixfileManager mfm, bool tdSsEqual, List<string> loadErrors, List<string> fileLoadErrors)
        {
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
            mfm.Reset(GameType.TiberianDawn, null);
            List<string> loadedFiles = mfm.ToList();
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
            mfm.Reset(GameType.None, null);
        }

        private static void InitClassicFilesRa(MixfileManager mfm, List<string> loadErrors, List<string> fileLoadErrors, bool forRemaster)
        {
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
            // Mod addons
            mfm.LoadArchives(GameType.RedAlert, "sc*.mix", true);
            // Not normally needed, but in the beta this contains palette.cps.
            mfm.LoadArchive(GameType.RedAlert, "general.mix", false, false, true, true);
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
            mfm.Reset(GameType.RedAlert, null);
            List<string> loadedFiles = mfm.ToList();
            const string prefix = "RA: ";
            if (!loadedFiles.Contains("expand2.mix")) loadErrors.Add(prefix + "expand2.mix");
            if (!loadedFiles.Contains("local.mix")) loadErrors.Add(prefix + "local.mix");
            if (!forRemaster)
            {
                if (!loadedFiles.Contains("conquer.mix")) loadErrors.Add(prefix + "conquer.mix");
                if (!loadedFiles.Contains("lores.mix")) loadErrors.Add(prefix + "lores.mix");
                if (!loadedFiles.Contains("lores1.mix")) loadErrors.Add(prefix + "lores1.mix");
            }
            if (!loadedFiles.Contains("temperat.mix")) loadErrors.Add(prefix + "temperat.mix");
            if (!loadedFiles.Contains("snow.mix")) loadErrors.Add(prefix + "snow.mix");
            if (!loadedFiles.Contains("interior.mix")) loadErrors.Add(prefix + "interior.mix");
            if (!forRemaster)
            {
                if (!mfm.FileExists("palette.cps")) fileLoadErrors.Add(prefix + "palette.cps");
                if (!mfm.FileExists("conquer.eng")) fileLoadErrors.Add(prefix + "conquer.eng");
            }
            if (!mfm.FileExists("rules.ini")) fileLoadErrors.Add(prefix + "rules.ini");
            if (!mfm.FileExists("aftrmath.ini")) fileLoadErrors.Add(prefix + "aftrmath.ini");
            if (!mfm.FileExists("mplayer.ini")) fileLoadErrors.Add(prefix + "mplayer.ini");
            mfm.Reset(GameType.None, null);
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
            gtm["TEXT_OVERLAY_CONCRETE_ROAD"] = "Concrete Road";
            gtm["TEXT_OVERLAY_CONCRETE_ROAD_FULL"] = "Concrete Road (full)";
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
            gtm["TEXT_OVERLAY_CONCRETE_ROAD"] = "Concrete Road";
            gtm["TEXT_OVERLAY_CONCRETE_ROAD_FULL"] = "Concrete Road (full)";
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
