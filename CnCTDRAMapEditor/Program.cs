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
using MobiusEditor.Utility;
using System;
using System.Globalization;
using System.IO;
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
                    String gameFolder = SteamAssist.TryGetSteamGameFolder("1213210", "TiberianDawn.dll", "RedAlert.dll");
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
