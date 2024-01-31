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
using MobiusEditor.Interface;
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
        public const string GameId = "1213210";
        
        public const string ProgramName = "Mobius Map Editor";
        public const string GithubOwner = "Nyerguds";
        public const string GithubProject = "MobiusMapEditor";
        public const string GithubUrl = "https://github.com/" + GithubOwner + "/" + GithubProject;
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

        public static readonly String ApplicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static String ProgramVersionTitle
        {
            get
            {
                AssemblyName assn = Assembly.GetExecutingAssembly().GetName();
                System.Version currentVersion = assn.Version;
                return string.Format(ProgramName + " v{0}", currentVersion);
            }
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
            foreach (GameInfo gic in GameTypeFactory.GetGameInfos())
            {
                modPaths.Add(gic.GameType, StartupLoader.GetModPaths(GameId, gic.ModsToLoad, gic.ModFolder, gic.ModIdentifier));
            }
            String runPath = StartupLoader.GetRemasterRunPath(GameId, !Globals.UseClassicFiles);
            if (runPath != null)
            {
                // Required for Steam interface to work.
                Environment.CurrentDirectory = runPath;
            }
            bool loadOk = false;
            if (!Globals.UseClassicFiles && runPath != null)
            {
                loadOk = StartupLoader.LoadEditorRemastered(runPath, modPaths);
            }
            else if (Globals.UseClassicFiles)
            {
                loadOk = StartupLoader.LoadEditorClassic(ApplicationPath, modPaths);
            }
            if (!loadOk)
            {
                return;
            }
            bool steamEnabled = false;
            if (SteamworksUGC.IsSteamBuild)
            {
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

    }
}
