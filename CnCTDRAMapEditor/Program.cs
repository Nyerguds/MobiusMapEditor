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
using MobiusEditor.Dialogs;
using MobiusEditor.Model;
using MobiusEditor.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace MobiusEditor
{
    static class Program
    {
        public const string RemasterSteamId = "1213210";

        public const string ProgramName = "Mobius Map Editor";
        public const string GithubOwner = "Nyerguds";
        public const string GithubProject = "MobiusMapEditor";
        public const string GithubUrl = "https://github.com/" + GithubOwner + "/" + GithubProject;
        public const string GithubManualUrl = GithubUrl + "/blob/master/MANUAL.md";
        public const string GithubVerCheckUrl = "https://api.github.com/repos/" + GithubOwner + "/" + GithubProject + "/releases?per_page=1";
        public const string ProgramInfo =
            "Originally created by Petroglyph Games for the Command & Conquer: Remastered project, " +
            "and gracefully released as open source licensed under GPL V3 by Electronic Arts.\n" +
            "\n" +
            "Upgraded by Maarten 'Nyerguds' Meuris.\n" +
            "\n" +
            "Initial upgrading work started by Rami Pasanen, aka Rampastring.\n" +
            "\n" +
            "Main testers and supporters:\n" +
            "Chad1233\n" +
            "DDF3\n" +
            "Zaptagious\n" +
            "GotAPresentForYa\n" +
            "\n" +
            "Boundless moral support:\n" +
            "N3tRunn3r\n" +
            "\n" +
            "Thank you for using " + ProgramName + "!";
        public const string ClassicInstructions = "To skip this dialog and always start with the classic graphics, edit {0}.config in a text editor and set the \"{1}\" setting to True.";
        public const string ClassicSetting = "UseClassicFiles";

        public static readonly string ApplicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string ApplicationCompany;
        public static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        public static readonly string ProgramVersionTitle = ProgramName + " v" + Assembly.GetExecutingAssembly().GetName().Version;
        public static string RemasterRunPath { get; private set; }

        static Program()
        {
            Assembly currentAssem = typeof(Program).Assembly;
            object[] attribs = currentAssem.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
            ApplicationCompany = attribs.Length > 0 ? ((AssemblyCompanyAttribute)attribs[0]).Company : String.Empty;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (Globals.EnableDpiAwareness)
            {
                TryEnableDPIAware();
            }
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
                Properties.Settings.Default.LastCheckVersion = null;
                Properties.Settings.Default.Save();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Dictionary<GameType, string[]> modPaths = new Dictionary<GameType, string[]>();
            // Check if any mods are allowed to override the default stuff to load.
            foreach (GameInfo gameInfo in GameTypeFactory.GetGameInfos())
            {
                if (gameInfo != null)
                {
                    modPaths.Add(gameInfo.GameType, StartupLoader.GetModPaths(RemasterSteamId, gameInfo.ModsToLoad, gameInfo.ModFolder, gameInfo.ModIdentifier));
                }
            }
            RemasterRunPath = StartupLoader.GetRemasterRunPath(RemasterSteamId, !Globals.UseClassicFiles);
            bool loadOk = false;
            string mixContentFile = Globals.MixContentInfoFile;
            if (!String.IsNullOrEmpty(mixContentFile)) {
                mixContentFile = mixContentFile.Replace('\\', Path.DirectorySeparatorChar);
            }
            string mixPath = Path.Combine(Program.ApplicationPath, Globals.MixContentInfoFile);
            MixFileNameGenerator romfis = null;
            try
            {
                if (File.Exists(mixPath))
                {
                    romfis = new MixFileNameGenerator(mixPath);
                }
                else
                {
                    // Fallback: use embedded resources to initialise romfis.
                    INI iniMain = new INI();
                    iniMain.Parse(Properties.Resources.mixcontent);
                    INI iniTd = new INI();
                    iniTd.Parse(Properties.Resources.mixcontent_td);
                    INI iniRa = new INI();
                    iniRa.Parse(Properties.Resources.mixcontent_ra1);
                    INI iniSole = new INI();
                    iniSole.Parse(Properties.Resources.mixcontent_sole);
                    Dictionary<string, INI> sideInis = new Dictionary<string, INI>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "mixcontent_td.ini", iniTd },
                        { "mixcontent_ra1.ini", iniRa },
                        { "mixcontent_sole.ini", iniSole },
                    };
                    romfis = new MixFileNameGenerator(iniMain, sideInis);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Error parsing mix file identification data: {0}\n\nNo mix contents will be identified.", ex.Message),"Error");
            }
            if (romfis.ProcessErrors.Count > 0)
            {
                // todo: log this somehow.
            }
            if (!Globals.UseClassicFiles && RemasterRunPath != null)
            {
                loadOk = StartupLoader.LoadEditorRemastered(RemasterRunPath, modPaths, romfis);
            }
            else if (Globals.UseClassicFiles)
            {
                loadOk = StartupLoader.LoadEditorClassic(ApplicationPath, modPaths, romfis);
            }
            if (!loadOk)
            {
                return;
            }
            // Always default the run path to the app path.
            Environment.CurrentDirectory = Program.ApplicationPath;
            bool steamEnabled = false;
            if (!Properties.Settings.Default.LazyInitSteam)
            {
                string runPath = RemasterRunPath;
                if (runPath != null)
                {
                    // Required for Steam interface to work.
                    Environment.CurrentDirectory = runPath;
                    if (SteamAssist.TryGetSteamId(Environment.CurrentDirectory) != null)
                    {
                        try
                        {
                            steamEnabled = SteamworksUGC.Init();
                        }
                        catch (DllNotFoundException ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
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
            string arg = null;
            try
            {
                if (args.Length > 0 && File.Exists(args[0]))
                    arg = args[0];
            }
            catch
            {
                arg = null;
            }
            using (MainForm mainForm = new MainForm(arg, romfis))
            {
                Application.Run(mainForm);
            }
            if (SteamworksUGC.IsSteamBuild)
            {
                SteamworksUGC.Shutdown();
            }
            Globals.TheArchiveManager.Dispose();
        }

        [DllImport("SHCore.dll")]
        private static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

        private enum PROCESS_DPI_AWARENESS
        {
            Process_DPI_Unaware           /**/ = 0,
            Process_System_DPI_Aware      /**/ = 1,
            Process_Per_Monitor_DPI_Aware /**/ = 2
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

        /// <summary>
        /// Ports settings over from older versions, and ensures only one settings folder remains.
        /// Taken from https://stackoverflow.com/a/14845448 and adapted to scan deeper.
        /// </summary>
        /// <param name="currentSettingsVer">Curent version fetched from the settings.</param>
        /// <param name="versionSetter">Delegate to set the version into the settings after the process is complete.</param>
        private static void CopyLastUserConfig(string currentSettingsVer, Action<string> versionSetter)
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
            string previousVersionConfigFile = previousSettingsDir == null ? null : String.Concat(previousSettingsDir.FullName, @"\", userConfigFileName);
            string currentVersionConfigFile = String.Concat(currentVersionConfigFileDir.FullName, @"\", userConfigFileName);
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

    }
}
