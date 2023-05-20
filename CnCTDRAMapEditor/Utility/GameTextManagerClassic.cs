using MobiusEditor.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MobiusEditor.Utility
{
    public class GameTextManagerClassic : IGameTextManager
    {
        private IArchiveManager fileManager;
        // Name of the strings file for each game.
        private Dictionary<GameType, string> gameTextPaths;
        private readonly Dictionary<string, int> gameTextMapping = new Dictionary<string, int>();
        private List<byte[]> stringsFile;
        private readonly Encoding encoding = Encoding.GetEncoding(437);

        public string this[string key] => GetString(key);

        public void Reset(GameType gameType)
        {
            stringsFile = null;
            this.LoadGameMappings(gameType);
            if (gameTextPaths.TryGetValue(gameType, out string gameTextFile))
            {
                try
                {
                    // File manager should be initialised for a specific game by getting a list of all
                    // .mix files to open, and all .mix files that can occur inside those mix files.
                    // This way, it can do a recursive search through everything.
                    using (Stream stream = fileManager.OpenFile(gameTextFile))
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        byte[] file = reader.ReadAllBytes();
                        stringsFile = LoadFile(file);
                    }
                }
                catch { /*ignore; just gonna be empty I guess */ }
            }
        }

        private string GetString(string key)
        {
            if (stringsFile == null || !gameTextMapping.TryGetValue(key, out int val) || val >= stringsFile.Count)
            {
                return String.Empty;
            }
            return encoding.GetString(stringsFile[val]);
        }

        public GameTextManagerClassic(IArchiveManager fileManager, Dictionary<GameType, string> gameTextPaths)
        {
            this.fileManager = fileManager;
            this.gameTextPaths = gameTextPaths;
        }

        public List<byte[]> LoadFile(byte[] fileData)
        {
            int len = fileData.Length;
            if (len < 2)
                throw new ArgumentOutOfRangeException("fileData", "File is too short to contain any entries!");
            // Map all covered content to prevent data overlaps. Though I guess technically
            // the format could support it... but let's not go into that contrived madness.
            bool[] covered = new bool[len];
            bool[] coveredIndex = new bool[len];
            int ptr = 0;
            List<byte[]> strings = new List<byte[]>();
            while (ptr + 2 <= len && !covered[ptr])
            {
                ushort cur = (ushort)(fileData[ptr] | (fileData[ptr + 1] << 8));
                coveredIndex[ptr] = true;
                coveredIndex[ptr + 1] = true;
                if (cur >= len)
                    throw new ArgumentOutOfRangeException("fileData", "File contains addresses larger than the file's size.");
                if (coveredIndex[cur])
                    throw new ArgumentOutOfRangeException("fileData", "Index refers to previously-read data inside index table.");
                int end = cur;
                while (end < len && fileData[end] != 0)
                    end++;
                int curStrLen = end - cur;
                byte[] curStr = new byte[curStrLen];
                Array.Copy(fileData, cur, curStr, 0, curStrLen);
                strings.Add(curStr);
                // Account for the 0 at the end. All strings should have this.
                end++;
                for (int i = cur; i < end && i < len; ++i)
                    covered[i] = true;
                ptr += 2;
            }
            return strings;
        }

        private void LoadGameMappings(GameType gameType)
        {
            {
                this.gameTextMapping.Clear();
                if (gameType == GameType.TiberianDawn || gameType == GameType.SoleSurvivor)
                {
                    // TODO adapt this to the text IDs used in the remaster
                    this.gameTextMapping.Union(new (string, int)[]
                    {
                        //("", 0), // Null
                        //("%3d.%02d", 0), // %3d.%02d
                        ("TEXT_UPGRADE", 2), // Upgrade
                        //("Upgrade Structure", 3), // Upgrade Structure
                        ("TEXT_UI_TACTICAL_UPGRADE_ALL", 4), // Upgrade
                        //("Sell", 5), // Sell
                        //("Sell Structure", 6), // Sell Structure
                        //("Demolish Structure", 7), // Demolish Structure
                        //("Repair", 8), // Repair
                        //("Repair Structure", 9), // Repair Structure
                        //("Repair", 10), // Repair
                        //("You:", 11), // You:
                        //("Enemy:", 12), // Enemy:
                        //("Buildings Destroyed By", 13), // Buildings Destroyed By
                        //("Units Destroyed By", 14), // Units Destroyed By
                        //("Tiberium Harvested By", 15), // Tiberium Harvested By
                        //("Score: %d", 16), // Score: %d
                        //("You have attained the rank of", 17), // You have attained the rank of
                        ("TEXT_YES", 18), // Yes
                        ("TEXT_NO", 19), // No
                        ("TEXT_READY", 20), // Ready
                        //("Holding", 21), // Holding
                        //("Accomplished", 22), // Accomplished
                        //("Failed", 23), // Failed
                        ("TEXT_UI_CHOOSE_YOUR_SIDE", 24), // Choose Your Side
                        ("TEXT_START_NEW_GAME", 25), // Start New Game
                        ("TEXT_INTRO_AND_SNEAK_PEEK", 26), // Intro & Sneak Peek
                        ("TEXT_CANCEL", 27), // Cancel
                        ("TEXT_PROP_TITLE_ROCK", 28), // Rock
                        ("TEXT_RESUME_GAME", 29), // Resume Game
                        //("Build This", 30), // Build This
                        //("Thank you for playing Command & Conquer.^", 31), // Thank you for playing Command & Conquer.^
                        //("Hall of Fame", 32), // Hall of Fame
                        ("TEXT_GLOBAL_DEFENSE_INITIATIVE", 33), // Global Defense Initiative
                        ("TEXT_BROTHERHOOD_OF_NOD", 34), // Brotherhood of Nod
                        ("TEXT_UNIT_TITLE_CIVILIAN", 35), // Civilian
                        ("TEXT_FACTION_NAME_FACTION_JURASSIC", 36), // Containment Team
                        ("TEXT_OK", 37), // OK
                        ("TEXT_PROP_TITLE_TREE", 38), // Tree
                        //("<", 39), // <
                        //(">", 40), // >
                        //("", 41), // 
                        //("", 42), // 
                        //("Clear the map", 43), // Clear the map
                        //("Inherit previous map", 44), // Inherit previous map
                        //("Clear", 45), // Clear
                        //("Water", 46), // Water
                        //("Road", 47), // Road
                        //("Tile Object", 48), // Tile Object
                        //("Slope", 49), // Slope
                        //("Brush", 50), // Brush
                        //("Patch", 51), // Patch
                        //("River", 52), // River
                        ("TEXT_LOAD_GAME", 53), // Load Mission
                        ("TEXT_SAVE", 54), // Save Mission
                        ("TEXT_OPTIONS_DELETE_SAVE", 55), // Delete Mission
                        //("Load", 56), // Load
                        //("Save", 57), // Save
                        //("Delete", 58), // Delete
                        //("Game Controls", 59), // Game Controls
                        //("Sound Controls", 60), // Sound Controls
                        //("Resume Mission", 61), // Resume Mission
                        //("Visual Controls", 62), // Visual Controls
                        //("Abort Mission", 63), // Abort Mission
                        //("Exit Game", 64), // Exit Game
                        //("Options", 65), // Options
                        ("TEXT_OVERLAY_TIBERIUM", 66), // Tiberium
                        //("Tiberium On", 67), // Tiberium On
                        //("Tiberium Off", 68), // Tiberium Off
                        //("Squish mark", 69), // Squish mark
                        ("TEXT_SMUDGE_CRATER", 70), // Crater
                        ("TEXT_SMUDGE_SCORCH", 71), // Scorch Mark
                        //("BRIGHTNESS:", 72), // BRIGHTNESS:
                        //("MUSIC VOLUME", 73), // MUSIC VOLUME
                        //("SOUND VOLUME", 74), // SOUND VOLUME
                        //("TINT:", 75), // TINT:
                        //("CONTRAST:", 76), // CONTRAST:
                        //("GAME SPEED:", 77), // GAME SPEED:
                        //("SCROLL RATE:", 78), // SCROLL RATE:
                        //("COLOR:", 79), // COLOR:
                        //("Return to game", 80), // Return to game
                        ("TEXT_TOOLTIP_ENEMY_INFANTRY", 81), // Enemy Soldier
                        ("TEXT_TOOLTIP_ENEMY_UNIT", 82), // Enemy Vehicle
                        ("TEXT_TOOLTIP_ENEMY_BUILDING", 83), // Enemy Structure
                        ("TEXT_UNIT_TITLE_NOD_FLAME_TANK", 84), // Flame Tank
                        ("TEXT_UNIT_TITLE_NOD_STEALTH_TANK", 85), // Stealth Tank
                        ("TEXT_UNIT_TITLE_NOD_LIGHT_TANK", 86), // Light Tank
                        ("TEXT_UNIT_TITLE_GDI_MED_TANK", 87), // Medium Tank
                        ("TEXT_UNIT_TITLE_GDI_MAMMOTH_TANK", 88), // Mammoth Tank
                        ("TEXT_UNIT_TITLE_NOD_NOD_BUGGY", 89), // Nod Buggy
                        ("TEXT_STRUCTURE_TITLE_NOD_SAM_SITE", 90), // SAM Site
                        ("TEXT_STRUCTURE_TITLE_GDI_ADV_COMM_CENTER", 91), // Advanced Com. Center
                        ("TEXT_UNIT_TITLE_GDI_MRLS", 92), // Rocket Launcher
                        ("TEXT_UNIT_TITLE_MHQ", 93), // Mobile HQ
                        ("TEXT_UNIT_TITLE_GDI_HUMVEE", 94), // Hum-vee
                        ("TEXT_UNIT_TITLE_GDI_TRANSPORT", 95), // Chinook Transport
                        ("TEXT_UNIT_TITLE_A10", 96), // A10
                        ("TEXT_UNIT_TITLE_C17", 97), // C17
                        ("TEXT_UNIT_TITLE_GDI_HARVESTER", 98), // Harvester
                        ("TEXT_UNIT_TITLE_NOD_ARTILLERY", 99), // Artillery
                        ("TEXT_UNIT_TITLE_GDI_MLRS", 100), // S.S.M. Launcher
                        ("TEXT_UNIT_TITLE_GDI_MINIGUNNER", 101), // Minigunner
                        ("TEXT_UNIT_TITLE_GDI_GRENADIER", 102), // Grenadier
                        ("TEXT_UNIT_TITLE_GDI_ROCKET_SOLDIER", 103), // Bazooka
                        ("TEXT_UNIT_TITLE_NOD_FLAMETHROWER", 104), // Flamethrower
                        ("TEXT_UNIT_TITLE_NOD_CHEM_WARRIOR", 105), // Chem-warrior
                        ("TEXT_UNIT_TITLE_GDI_COMMANDO", 106), // Commando
                        ("TEXT_UNIT_TITLE_LST", 107), // Hovercraft
                        ("TEXT_UNIT_TITLE_NOD_HELICOPTER", 108), // Apache
                        ("TEXT_UNIT_TITLE_GDI_ORCA", 109), // Orca
                        ("TEXT_UNIT_TITLE_GDI_APC", 110), // APC
                        ("TEXT_STRUCTURE_TITLE_GDI_GUARD_TOWER", 111), // Guard Tower
                        ("TEXT_STRUCTURE_TITLE_GDI_COMM_CENTER", 112), // Communications Center
                        ("TEXT_STRUCTURE_TITLE_GDI_HELIPAD", 113), // Helicopter Pad
                        ("TEXT_STRUCTURE_TITLE_NOD_AIRFIELD", 114), // Airstrip
                        ("TEXT_STRUCTURE_TITLE_GDI_SILO", 115), // Tiberium Silo
                        ("TEXT_STRUCTURE_TITLE_GDI_CONSTRUCTION_YARD", 116), // Construction Yard
                        ("TEXT_STRUCTURE_TITLE_GDI_REFINERY", 117), // Tiberium Refinery
                        ("TEXT_STRUCTURE_TITLE_CIV1", 118), // Church
                        ("TEXT_STRUCTURE_TITLE_CIV2", 119), // Han's and Gretel's
                        ("TEXT_STRUCTURE_TITLE_CIV3", 120), // Hewitt's Manor
                        ("TEXT_STRUCTURE_TITLE_CIV4", 121), // Ricktor's House
                        ("TEXT_STRUCTURE_TITLE_CIV5", 122), // Gretchin's House
                        ("TEXT_STRUCTURE_TITLE_CIV6", 123), // The Barn
                        ("TEXT_STRUCTURE_TITLE_CIV7", 124), // Damon's pub
                        ("TEXT_STRUCTURE_TITLE_CIV8", 125), // Fran's House
                        ("TEXT_STRUCTURE_TITLE_CIV9", 126), // Music Factory
                        ("TEXT_STRUCTURE_TITLE_CIV10", 127), // Toymaker's
                        ("TEXT_STRUCTURE_TITLE_CIV11", 128), // Ludwig's House
                        //("TEXT_STRUCTURE_TITLE_CIV12", 129), // Haystacks
                        ("TEXT_STRUCTURE_TITLE_CIV12", 130), // Haystack
                        ("TEXT_STRUCTURE_TITLE_CIV13", 131), // Wheat Field
                        ("TEXT_STRUCTURE_TITLE_CIV14", 132), // Fallow Field
                        ("TEXT_STRUCTURE_TITLE_CIV15", 133), // Corn Field
                        ("TEXT_STRUCTURE_TITLE_CIV16", 134), // Celery Field
                        ("TEXT_STRUCTURE_TITLE_CIV17", 135), // Potato Field
                        ("TEXT_STRUCTURE_TITLE_CIV18", 136), // Sala's House
                        ("TEXT_STRUCTURE_TITLE_CIV19", 137), // Abdul's House
                        ("TEXT_STRUCTURE_TITLE_CIV20", 138), // Pablo's Wicked Pub
                        ("TEXT_STRUCTURE_TITLE_CIV21", 139), // Village Well
                        ("TEXT_STRUCTURE_TITLE_CIV2é", 140), // Camel Trader
                        //("TEXT_STRUCTURE_TITLE_CIV1", 141), // Church
                        ("TEXT_STRUCTURE_TITLE_CIV23", 142), // Ali's House
                        ("TEXT_STRUCTURE_TITLE_CIV24", 143), // Trader Ted's
                        ("TEXT_STRUCTURE_TITLE_CIV25", 144), // Menelik's House
                        ("TEXT_STRUCTURE_TITLE_CIV26", 145), // Prestor John's House
                        ("TEXT_STRUCTURE_TITLE_CIV27", 146), // Village Well
                        ("TEXT_STRUCTURE_TITLE_CIV28", 147), // Witch Doctor's Hut
                        ("TEXT_STRUCTURE_TITLE_CIV29", 148), // Rikitikitembo's Hut
                        ("TEXT_STRUCTURE_TITLE_CIV30", 149), // Roarke's Hut
                        ("TEXT_STRUCTURE_TITLE_CIV31", 150), // Mubasa's Hut
                        ("TEXT_STRUCTURE_TITLE_CIV32", 151), // Aksum's Hut
                        ("TEXT_STRUCTURE_TITLE_CIV33", 152), // Mambo's Hut
                        ("TEXT_STRUCTURE_TITLE_CIV34", 153), // The Studio
                        ("TEXT_UNIT_TITLE_MISS", 154), // Technology Center
                        ("TEXT_STRUCTURE_TITLE_NOD_TURRET", 155), // Gun Turret
                        ("TEXT_UNIT_TITLE_WAKE", 156), // Gun Boat
                        ("TEXT_UNIT_TITLE_GDI_MCV", 157), // Mobile Construction Vehicle
                        ("TEXT_UNIT_TITLE_NOD_RECON_BIKE", 158), // Recon Bike
                        ("TEXT_STRUCTURE_TITLE_GDI_POWER_PLANT", 159), // Power Plant
                        ("TEXT_STRUCTURE_TITLE_GDI_ADV_POWER_PLANT", 160), // Advanced Power Plant
                        ("TEXT_UNIT_TITLE_HOSP", 161), // Hospital
                        ("TEXT_STRUCTURE_TITLE_GDI_BARRACKS", 162), // Barracks
                        ("TEXT_OVERLAY_CONCRETE_PAVEMENT", 163), // Concrete
                        //("TEXT_OVERLAY_CONCRETE_ROAD", 163), // Concrete
                        //("TEXT_OVERLAY_CONCRETE_ROAD_FULL", 163), // Concrete
                        ("TEXT_STRUCTURE_TITLE_OIL_PUMP", 164), // Oil Pump
                        ("TEXT_STRUCTURE_TITLE_OIL_TANKER", 165), // Oil Tanker
                        ("TEXT_STRUCTURE_TITLE_GDI_SANDBAGS", 166), // Sandbag Wall
                        ("TEXT_STRUCTURE_TITLE_GDI_CHAIN_LINK", 167), // Chain Link Fence
                        ("TEXT_STRUCTURE_TITLE_GDI_CONCRETE", 168), // Concrete Wall
                        ("TEXT_STRUCTURE_RA_BARB", 169), // Barbwire Fence
                        ("TEXT_STRUCTURE_TD_WOOD", 170), // Wood Fence
                        ("TEXT_STRUCTURE_TITLE_GDI_WEAPONS_FACTORY", 171), // Weapons Factory
                        ("TEXT_STRUCTURE_TITLE_GDI_ADV_GUARD_TOWER", 172), // Advanced Guard Tower
                        ("TEXT_STRUCTURE_TITLE_NOD_OBELISK", 173), // Obelisk of Light
                        ("TEXT_UNIT_TITLE_BIO", 174), // Bio-Research Laboratory
                        ("TEXT_STRUCTURE_TITLE_NOD_HAND_OF_NOD", 175), // Hand of Nod
                        ("TEXT_STRUCTURE_TITLE_NOD_TEMPLE_OF_NOD", 176), // Temple Of Nod
                        ("TEXT_STRUCTURE_TITLE_GDI_REPAIR_FACILITY", 177), // Repair Bay
                        //("Sidebar", 178), // Sidebar
                        ("TEXT_MAIN_MENU_OPTIONS", 179), // Options
                        //("Database", 180), // Database
                        ("TEXT_TOOLTIP_UNREVEALED_TERRAIN", 181), // Unrevealed Terrain
                        ("TEXT_OPTIONS_MENU", 182), // Options Menu
                        ("TEXT_TOOLTIP_STOP", 183), // STOP
                        ("TEXT_PLAY", 184), // PLAY
                        ("TEXT_SHUFFLE_CUSTOM_PLAYLIST", 185), // SHUFFLE
                        //("REPEAT", 186), // REPEAT
                        ("TEXT_VOLUME_MUSIC", 187), // Music volume:
                        ("TEXT_VOLUME_SFX_GAMEPLAY", 188), // Sound volume:
                        //("On", 189), // On
                        //("Off", 190), // Off
                        ("TEXT_MUSIC_TDC_MUS_ACT_ON_INSTINCT", 191), // Act On Instinct
                        ("TEXT_MUSIC_TDC_MUS_IN_TROUBLE", 192), // Looks Like Trouble
                        ("TEXT_MUSIC_TDC_MUS_INDUSTRIAL", 193), // Industrial
                        ("TEXT_MUSIC_TDC_MUS_REACHING_OUT", 194), // Reaching Out
                        ("TEXT_MUSIC_TDC_MUS_ON_THE_PROWL", 195), // On The Prowl
                        ("TEXT_MUSIC_TDC_MUS_PREPARE_FOR_BATTLE", 196), // Prepare For Battle
                        ("TEXT_MUSIC_TDC_MUS_JUST_DO_IT_UP", 197), // Just Do It!
                        ("TEXT_MUSIC_TDC_MUS_IN_THE_LINE_OF_FIRE", 198), // In The Line Of Fire
                        ("TEXT_MUSIC_TDC_MUS_MARCH_TO_DOOM", 199), // March To Doom
                        ("TEXT_MUSIC_TDC_MUS_WE_WILL_STOP_THEM_DECEPTION", 200), // We Will Stop Them (Deception)
                        ("TEXT_MUSIC_TDC_MUS_CC_THANG", 201), // C&C Thang
                        ("TEXT_MUSIC_TDC_MUS_TO_BE_FEARED", 202), // Enemies To Be Feared
                        ("TEXT_MUSIC_TDC_MUS_WARFARE_FULL_STOP", 203), // Warfare (Full Stop)
                        ("TEXT_MUSIC_TDC_MUS_FIGHT_WIN_PREVAIL", 204), // Fight, Win, Prevail
                        ("TEXT_MUSIC_TDC_MUS_DIE", 205), // Die!!
                        ("TEXT_MUSIC_TDC_MUS_NO_MERCY", 206), // No Mercy
                        ("TEXT_MUSIC_TDC_MUS_TARGET_MECHANICAL_MAN", 207), // Mechanical Man
                        //("I Am - Destructible Times", 208), // I Am - Destructible Times
                        ("TEXT_MUSIC_TDC_MUS_GREAT_SHOT", 209), // Great Shot!
                        //("Multiplayer Game", 210), // Multiplayer Game
                        //("No files available", 211), // No files available
                        //("Do you want to delete this file?", 212), // Do you want to delete this file?
                        //("Do you want to delete %d files?", 213), // Do you want to delete %d files?
                        //("Reset Values", 214), // Reset Values
                        //("Confirmation", 215), // Confirmation
                        //("Do you want to abort the mission?", 216), // Do you want to abort the mission?
                        ("TEXT_BRIEFING", 217), // Mission Description
                        ("TEXT_UNIT_TITLE_CIV1", 218), // Joe
                        ("TEXT_UNIT_TITLE_CIV2", 219), // Bill
                        ("TEXT_UNIT_TITLE_CIV3", 220), // Shelly
                        ("TEXT_UNIT_TITLE_CIV4", 221), // Maria
                        ("TEXT_UNIT_TITLE_CIV5", 222), // Eydie
                        ("TEXT_UNIT_TITLE_CIV6", 223), // Dave
                        ("TEXT_UNIT_TITLE_CIV7", 224), // Phil
                        ("TEXT_UNIT_TITLE_CIV8", 225), // Dwight
                        ("TEXT_UNIT_TITLE_CIV9", 226), // Erik
                        ("TEXT_UNIT_TITLE_MOEBIUS", 227), // Dr. Moebius
                        ("TEXT_SMUDGE_BIB", 228), // Road Bib
                        ("TEXT_ID_GAME_SPEED_7", 229), // Faster
                        ("TEXT_ID_GAME_SPEED_2", 230), // Slower
                        ("TEXT_UNIT_TITLE_GDI_IONCANNON", 231), // Ion Cannon
                        ("TEXT_UNIT_TITLE_NOD_NUCLEAR_STRIKE", 232), // Nuclear Strike
                        ("TEXT_UNIT_TITLE_GDI_AIRSTRIKE", 233), // Air Strike
                        ("TEXT_UNIT_TITLE_TREX", 234), // Tyrannosaurus Rex
                        ("TEXT_UNIT_TITLE_TRIC", 235), // Triceratops
                        ("TEXT_UNIT_TITLE_RAPT", 236), // Velociraptor
                        ("TEXT_UNIT_TITLE_STEG", 237), // Stegosaurus
                        ("TEXT_OVERLAY_SCRATE", 238), // Steel Crate
                        ("TEXT_OVERLAY_WCRATE", 239), // Wood Crate
                        ("TEXT_CF_ONHOVER_SPOT", 240), // Flag Location
                        ("TEXT_FACTION_NAME_FACTION_1", 241), // GDI
                        ("TEXT_FACTION_NAME_FACTION_2", 242), // NOD
                        //("Unable to read scenario!", 243), // Unable to read scenario!
                        //("Error loading game!", 244), // Error loading game!
                        //("Obsolete saved game.", 245), // Obsolete saved game.
                        //("You must enter a description!", 246), // You must enter a description!
                        //("Error saving game!", 247), // Error saving game!
                        //("Delete this file?", 248), // Delete this file?
                        //("[EMPTY SLOT]", 249), // [EMPTY SLOT]
                        //("Select Multiplayer Game", 250), // Select Multiplayer Game
                        //("Modem/Serial", 251), // Modem/Serial
                        //("Network", 252), // Network
                        //("Unable to initialize network!", 253), // Unable to initialize network!
                        //("Join Network Game", 254), // Join Network Game
                        //("New", 255), // New
                        //("Join", 256), // Join
                        //("Send Message", 257), // Send Message
                        //("Your Name:", 258), // Your Name:
                        //("Side:", 259), // Side:
                        //("Color:", 260), // Color:
                        //("Games", 261), // Games
                        //("Players", 262), // Players
                        //("Scenario:", 263), // Scenario:
                        //(">> NOT FOUND <<", 264), // >> NOT FOUND <<
                        //("Starting Credits:", 265), // Starting Credits:
                        //("Bases:", 266), // Bases:
                        //("Tiberium:", 267), // Tiberium:
                        //("Crates:", 268), // Crates:
                        //("AI Players:", 269), // AI Players:
                        //("Request denied.", 270), // Request denied.
                        //("Unable to play; scenario not found.", 271), // Unable to play; scenario not found.
                        //("Nothing to join!", 272), // Nothing to join!
                        //("You must enter a name!", 273), // You must enter a name!
                        //("Duplicate names are not allowed.", 274), // Duplicate names are not allowed.
                        //("Your game version is outdated.", 275), // Your game version is outdated.
                        //("Destination game version is outdated. Run C&C with the -o parameter to enable old version compatability.", 276), // Destination game version is outdated. Run C&C with the -o parameter to enable old version compatability.
                        //("%s's Game", 277), // %s's Game
                        //("[%s's Game]", 278), // [%s's Game]
                        //("Network Game Setup", 279), // Network Game Setup
                        //("Reject", 280), // Reject
                        //("You can't reject yourself!  You might develop serious self-esteem problems!", 281), // You can't reject yourself!  You might develop serious self-esteem problems!
                        //("You must select a player to reject.", 282), // You must select a player to reject.
                        //("Bases On", 283), // Bases On
                        //("Bases Off", 284), // Bases Off
                        //("Crates On", 285), // Crates On
                        //("Crates Off", 286), // Crates Off
                        //("AI Players On", 287), // AI Players On
                        //("AI Players Off", 288), // AI Players Off
                        //("Scenarios", 289), // Scenarios
                        //("Starting Credits", 290), // Starting Credits
                        //("Only one player?", 291), // Only one player?
                        //("Oops!", 292), // Oops!
                        //("To %s:", 293), // To %s:
                        //("To All:", 294), // To All:
                        //("Message:", 295), // Message:
                        //("Connection to %s lost!", 296), // Connection to %s lost!
                        //("%s has left the game.", 297), // %s has left the game.
                        //("%s has been defeated!", 298), // %s has been defeated!
                        //("Waiting to Connect...", 299), // Waiting to Connect...
                        //("Connection error!^Check your cables.^Attempting to Reconnect...", 300), // Connection error!^Check your cables.^Attempting to Reconnect...
                        //("Connection error!^Redialing...", 301), // Connection error!^Redialing...
                        //("Connection error!^Waiting for Call...", 302), // Connection error!^Waiting for Call...
                        //("Select Serial Game", 303), // Select Serial Game
                        //("Dial Modem", 304), // Dial Modem
                        //("Answer Modem", 305), // Answer Modem
                        //("Null Modem", 306), // Null Modem
                        //("Settings", 307), // Settings
                        //("Port:", 308), // Port:
                        //("IRQ:", 309), // IRQ:
                        //("Baud:", 310), // Baud:
                        //("Init String:", 311), // Init String:
                        //("Call Waiting String:", 312), // Call Waiting String:
                        //("Tone Dialing", 313), // Tone Dialing
                        //("Pulse Dialing", 314), // Pulse Dialing
                        //("Host Serial Game", 315), // Host Serial Game
                        //("Opponent:", 316), // Opponent:
                        //("User signed off!", 317), // User signed off!
                        //("Join Serial Game", 318), // Join Serial Game
                        //("Phone List", 319), // Phone List
                        //("Add", 320), // Add
                        //("Edit", 321), // Edit
                        //("Dial", 322), // Dial
                        //("Default", 323), // Default
                        //("Default Settings", 324), // Default Settings
                        //("Custom Settings", 325), // Custom Settings
                        //("Phone Listing", 326), // Phone Listing
                        //("Name:", 327), // Name:
                        //("Number:", 328), // Number:
                        //("Unable to find modem.^Check power and cables.", 329), // Unable to find modem.^Check power and cables.
                        //("No carrier.", 330), // No carrier.
                        //("Line busy.", 331), // Line busy.
                        //("Number invalid.", 332), // Number invalid.
                        //("Other system not responding!", 333), // Other system not responding!
                        //("Games are out of sync!", 334), // Games are out of sync!
                        //("Packet received too late!", 335), // Packet received too late!
                        //("Other player has left the game.", 336), // Other player has left the game.
                        //("From %s:%s", 337), // From %s:%s
                        //("2,728,000", 338), // 2,728,000
                        //("38,385,000", 339), // 38,385,000
                        //("10,373,000", 340), // 10,373,000
                        //("51,994,000", 341), // 51,994,000
                        //("80,387,000", 342), // 80,387,000
                        //("10,400,000", 343), // 10,400,000
                        //("5,300,000", 344), // 5,300,000
                        //("7,867,000", 345), // 7,867,000
                        //("10,333,000", 346), // 10,333,000
                        //("1,974,000", 347), // 1,974,000
                        //("23,169,000", 348), // 23,169,000
                        //("10,064,000", 349), // 10,064,000
                        //("3,285,000", 350), // 3,285,000
                        //("8,868,000", 351), // 8,868,000
                        //("10,337,000", 352), // 10,337,000
                        //("4,365,000", 353), // 4,365,000
                        //("1,607,000", 354), // 1,607,000
                        //("4,485,000", 355), // 4,485,000
                        //("56,386,000", 356), // 56,386,000
                        //("28,305,000", 357), // 28,305,000
                        //("5,238,000", 358), // 5,238,000
                        //("2,059,000", 359), // 2,059,000
                        //("13,497,000", 360), // 13,497,000
                        //("4,997,000", 361), // 4,997,000
                        //("88,500,000", 362), // 88,500,000
                        //("1,106,000", 363), // 1,106,000
                        //("12,658,000", 364), // 12,658,000
                        //("3,029,000", 365), // 3,029,000
                        //("39,084,000", 366), // 39,084,000
                        //("23,154,000", 367), // 23,154,000
                        //("8,902,000", 368), // 8,902,000
                        //("27,791,000", 369), // 27,791,000
                        //("1,574,000", 370), // 1,574,000
                        //("15,469,000", 371), // 15,469,000
                        //("1,300,000", 372), // 1,300,000
                        //("41,688,000", 373), // 41,688,000
                        //("24,900 SQ. MI.", 374), // 24,900 SQ. MI.
                        //("120,727 SQ. MI.", 375), // 120,727 SQ. MI.
                        //("80,134 SQ. MI.", 376), // 80,134 SQ. MI.
                        //("233,100 SQ. MI.", 377), // 233,100 SQ. MI.
                        //("137,838 SQ. MI.", 378), // 137,838 SQ. MI.
                        //("30,449 SQ. MI.", 379), // 30,449 SQ. MI.
                        //("18,932 SQ. MI.", 380), // 18,932 SQ. MI.
                        //("32,377 SQ. MI.", 381), // 32,377 SQ. MI.
                        //("35,919 SQ. MI.", 382), // 35,919 SQ. MI.
                        //("7,819 SQ. MI.", 383), // 7,819 SQ. MI.
                        //("91,699 SQ. MI.", 384), // 91,699 SQ. MI.
                        //("51,146 SQ. MI.", 385), // 51,146 SQ. MI.
                        //("11,100 SQ. MI.", 386), // 11,100 SQ. MI.
                        //("44,365 SQ. MI.", 387), // 44,365 SQ. MI.
                        //("39,449 SQ. MI.", 388), // 39,449 SQ. MI.
                        //("19,741 SQ. MI.", 389), // 19,741 SQ. MI.
                        //("17,413 SQ. MI.", 390), // 17,413 SQ. MI.
                        //("RIGA", 391), // RIGA
                        //("WARSAW", 392), // WARSAW
                        //("MINSK", 393), // MINSK
                        //("KIEV", 394), // KIEV
                        //("BERLIN", 395), // BERLIN
                        //("PRAGUE", 396), // PRAGUE
                        //("BRATISLAVA", 397), // BRATISLAVA
                        //("VIENNA", 398), // VIENNA
                        //("BUDAPEST", 399), // BUDAPEST
                        //("LJUBLJANA", 400), // LJUBLJANA
                        //("BUCHAREST", 401), // BUCHAREST
                        //("ATHENS", 402), // ATHENS
                        //("TIRANA", 403), // TIRANA
                        //("SOFIA", 404), // SOFIA
                        //("BELGRADE", 405), // BELGRADE
                        //("SARAJEVO", 406), // SARAJEVO
                        //("TALLINN", 407), // TALLINN
                        //("TRIPOLI", 408), // TRIPOLI
                        //("CAIRO", 409), // CAIRO
                        //("KHARTOUM", 410), // KHARTOUM
                        //("N'DJAMENA", 411), // N'DJAMENA
                        //("NOUAKCHOTT", 412), // NOUAKCHOTT
                        //("YAMOUSSOUKRO", 413), // YAMOUSSOUKRO
                        //("PORTO-NOVO", 414), // PORTO-NOVO
                        //("ABUJA", 415), // ABUJA
                        //("LIBREVILLE", 416), // LIBREVILLE
                        //("YAOUNDE", 417), // YAOUNDE
                        //("BANGUI", 418), // BANGUI
                        //("KINSHASA", 419), // KINSHASA
                        //("CAIRO", 420), // CAIRO
                        //("LUANDA", 421), // LUANDA
                        //("DAR-ES-SALAAM", 422), // DAR-ES-SALAAM
                        //("WINDHOEK", 423), // WINDHOEK
                        //("MAPUTO", 424), // MAPUTO
                        //("GABARONE", 425), // GABARONE
                        //("CAPE TOWN", 426), // CAPE TOWN
                        //("NEGLIGIBLE", 427), // NEGLIGIBLE
                        //("$162.7 BLN", 428), // $162.7 BLN
                        //("$47.6 BLN", 429), // $47.6 BLN
                        //("$1,131 BLN", 430), // $1,131 BLN
                        //("$120 BLN", 431), // $120 BLN
                        //("$164 BLN", 432), // $164 BLN
                        //("$60.1 BLN", 433), // $60.1 BLN
                        //("$21 BLN", 434), // $21 BLN
                        //("$71.9 BLN", 435), // $71.9 BLN
                        //("$77 BLN", 436), // $77 BLN
                        //("$4.0 BLN", 437), // $4.0 BLN
                        //("$47.3 BLN", 438), // $47.3 BLN
                        //("$120.1 BLN", 439), // $120.1 BLN
                        //("$14.0 BLN", 440), // $14.0 BLN
                        //("$28.9 BLN", 441), // $28.9 BLN
                        //("$39.2 BLN", 442), // $39.2 BLN
                        //("$12.1 BLN", 443), // $12.1 BLN
                        //("$1.0 BLN", 444), // $1.0 BLN
                        //("$10.0 BLN", 445), // $10.0 BLN
                        //("$1.7 BLN", 446), // $1.7 BLN
                        //("$28.0 BLN", 447), // $28.0 BLN
                        //("$5.3 BLN", 448), // $5.3 BLN
                        //("$11.6 BLN", 449), // $11.6 BLN
                        //("$1.3 BLN", 450), // $1.3 BLN
                        //("$6.6 BLN", 451), // $6.6 BLN
                        //("$8.3 BLN", 452), // $8.3 BLN
                        //("$6.9 BLN", 453), // $6.9 BLN
                        //("$2.0 BLN", 454), // $2.0 BLN
                        //("$3.1 BLN", 455), // $3.1 BLN
                        //("$104.0 BLN", 456), // $104.0 BLN
                        //("JELGAVA", 457), // JELGAVA
                        //("GDANSK", 458), // GDANSK
                        //("BYELISTOK", 459), // BYELISTOK
                        //("BOBYRUSK", 460), // BOBYRUSK
                        //("IVANO-FRANKOVSK", 461), // IVANO-FRANKOVSK
                        //("HANOVER", 462), // HANOVER
                        //("DRESDEN", 463), // DRESDEN
                        //("OSTRAVA", 464), // OSTRAVA
                        //("BRATISLAVA", 465), // BRATISLAVA
                        //("SALZBURG", 466), // SALZBURG
                        //("BUDAPEST", 467), // BUDAPEST
                        //("TRIESTE", 468), // TRIESTE
                        //("ARAD", 469), // ARAD
                        //("CORINTH", 470), // CORINTH
                        //("SHKODER", 471), // SHKODER
                        //("SOFIA", 472), // SOFIA
                        //("NIS", 473), // NIS
                        //("BELGRADE", 474), // BELGRADE
                        //("?", 475), // ?
                        //("PARNU", 476), // PARNU
                        //("TMASSAH", 477), // TMASSAH
                        //("AL-ALAMYN", 478), // AL-ALAMYN
                        //("AL-KHARIJAH", 479), // AL-KHARIJAH
                        //("AL-UBAYYID", 480), // AL-UBAYYID
                        //("KAFIA-KINGI", 481), // KAFIA-KINGI
                        //("OUM HADJER", 482), // OUM HADJER
                        //("MAO", 483), // MAO
                        //("TIDJIKDJA", 484), // TIDJIKDJA
                        //("ABIDJAN", 485), // ABIDJAN
                        //("PORTO-NOVO", 486), // PORTO-NOVO
                        //("ABUJA", 487), // ABUJA
                        //("KOULA-MOUTOU", 488), // KOULA-MOUTOU
                        //("BERTOUA", 489), // BERTOUA
                        //("BANGASSOU", 490), // BANGASSOU
                        //("LODJA", 491), // LODJA
                        //("KINSHASA", 492), // KINSHASA
                        //("LUXOR", 493), // LUXOR
                        //("CAIUNDO", 494), // CAIUNDO
                        //("MZUZU", 495), // MZUZU
                        //("KEETMANSHOOP", 496), // KEETMANSHOOP
                        //("XAI-XAI", 497), // XAI-XAI
                        //("GHANZI", 498), // GHANZI
                        //("CAPE TOWN", 499), // CAPE TOWN
                        ("TEXT_GDI_PROGRESSION", 500), // GDI PROGRESSION
                        ("TEXT_NOD_PROGRESSION", 501), // NOD PROGRESSION
                        //("LOCATING COORDINATES", 502), // LOCATING COORDINATES
                        //("OF NEXT MISSION", 503), // OF NEXT MISSION
                        //("SELECT TERRITORY", 504), // SELECT TERRITORY
                        //("TO ATTACK", 505), // TO ATTACK
                        //("POPULATION:", 506), // POPULATION:
                        //("GEOGRAPHIC AREA:", 507), // GEOGRAPHIC AREA:
                        //("CAPITAL:", 508), // CAPITAL:
                        //("GOVERNMENT:", 509), // GOVERNMENT:
                        //("GROSS DOMESTIC PRODUCT:", 510), // GROSS DOMESTIC PRODUCT:
                        //("POINT OF CONFLICT:", 511), // POINT OF CONFLICT:
                        //("MILITARY POWER:", 512), // MILITARY POWER:
                        //("EXPENDABILITY:", 513), // EXPENDABILITY:
                        //("GOVT CORRUPTABILITY:", 514), // GOVT CORRUPTABILITY:
                        //("NET WORTH:", 515), // NET WORTH:
                        //("MILITARY STRENGTH:", 516), // MILITARY STRENGTH:
                        //("MILITARY RESISTANCE:", 517), // MILITARY RESISTANCE:
                        //("LATVIA", 518), // LATVIA
                        //("POLAND", 519), // POLAND
                        //("BELARUS", 520), // BELARUS
                        //("UKRAINE", 521), // UKRAINE
                        //("GERMANY", 522), // GERMANY
                        //("CZECH REPUBLIC", 523), // CZECH REPUBLIC
                        //("SLOVAKIA", 524), // SLOVAKIA
                        //("AUSTRIA", 525), // AUSTRIA
                        //("HUNGARY", 526), // HUNGARY
                        //("SLOVENIA", 527), // SLOVENIA
                        //("ROMANIA", 528), // ROMANIA
                        //("GREECE", 529), // GREECE
                        //("ALBANIA", 530), // ALBANIA
                        //("BULGARIA", 531), // BULGARIA
                        //("YUGOSLAVIA", 532), // YUGOSLAVIA
                        //("BOSNIA/HERZOGOVINA", 533), // BOSNIA/HERZOGOVINA
                        //("LIBYA", 534), // LIBYA
                        //("EGYPT", 535), // EGYPT
                        //("SUDAN", 536), // SUDAN
                        //("CHAD", 537), // CHAD
                        //("MAURITANIA", 538), // MAURITANIA
                        //("IVORY COAST", 539), // IVORY COAST
                        //("BENIN", 540), // BENIN
                        //("NIGERIA", 541), // NIGERIA
                        //("GABON", 542), // GABON
                        //("CAMEROON", 543), // CAMEROON
                        //("CENTRAL AFRICAN REPUBLIC", 544), // CENTRAL AFRICAN REPUBLIC
                        //("ZAIRE", 545), // ZAIRE
                        //("ANGOLA", 546), // ANGOLA
                        //("TANZANIA", 547), // TANZANIA
                        //("NAMIBIA", 548), // NAMIBIA
                        //("MOZAMBIQUE", 549), // MOZAMBIQUE
                        //("BOTSWANA", 550), // BOTSWANA
                        //("SOUTH AFRICA", 551), // SOUTH AFRICA
                        //("ESTONIA", 552), // ESTONIA
                        //("REPUBLIC", 553), // REPUBLIC
                        //("DEMOCRATIC STATE", 554), // DEMOCRATIC STATE
                        //("FEDERAL REPUBLIC", 555), // FEDERAL REPUBLIC
                        //("CONST. REPUBLIC", 556), // CONST. REPUBLIC
                        //("PARL. DEMOCRACY", 557), // PARL. DEMOCRACY
                        //("PRES. PARL. REPUBLIC", 558), // PRES. PARL. REPUBLIC
                        //("DEMOCRACY", 559), // DEMOCRACY
                        //("IN TRANSITION", 560), // IN TRANSITION
                        //("ISLAMIC SOCIALIST", 561), // ISLAMIC SOCIALIST
                        //("MILITARY", 562), // MILITARY
                        //("ISLAMIC REPUBLIC", 563), // ISLAMIC REPUBLIC
                        //("PARL. REPUBLIC", 564), // PARL. REPUBLIC
                        //("LOCAL MILITIA", 565), // LOCAL MILITIA
                        //("STATE MILITIA", 566), // STATE MILITIA
                        //("NATIONAL GUARD", 567), // NATIONAL GUARD
                        //("FREE STANDING ARMY", 568), // FREE STANDING ARMY
                        //("?", 569), // ?
                        //("NATIONAL POWER", 570), // NATIONAL POWER
                        //("RESPECTABLE", 571), // RESPECTABLE
                        //("FORMIDABLE", 572), // FORMIDABLE
                        //("LAUGHABLE", 573), // LAUGHABLE
                        //("REASONABLE", 574), // REASONABLE
                        //("INSIGNIFICANT", 575), // INSIGNIFICANT
                        //("CLICK TO CONTINUE", 576), // CLICK TO CONTINUE
                        //("LOW", 577), // LOW
                        //("MEDIUM", 578), // MEDIUM
                        //("HIGH", 579), // HIGH
                        //("TIME:", 580), // TIME:
                        ("TEXT_SCORE_LEADERSHIP_HEADER", 581), // LEADERSHIP:
                        ("TEXT_SCORE_EFFICIENCY_HEADER", 582), // EFFICIENCY:
                        ("TEXT_SCORE_TOTAL_SCORE_HEADER", 583), // TOTAL SCORE:
                        ("TEXT_SCORE_CASUALTIES_HEADER", 584), // CASUALTIES:
                        //("NEUTRAL:", 585), // NEUTRAL:
                        //("GDI:", 586), // GDI:
                        ("TEXT_SCORE_BUILDINGS_LOST_HEADER", 587), // BUILDINGS LOST
                        //("BUILDINGS", 588), // BUILDINGS
                        //("LOST:", 589), // LOST:
                        ("TEXT_SCORE_SCREEN_TOP_SCORES", 590), // TOP SCORES
                        ("TEXT_SCORE_ENDING_CREDITS_HEADER", 591), // ENDING CREDITS:
                        //("%dh %dm", 592), // %dh %dm
                        //("%dm", 593), // %dm
                        //("NOD:", 594), // NOD:
                        //("Dialing...", 595), // Dialing...
                        //("Dialing Canceled", 596), // Dialing Canceled
                        //("Waiting for Call...", 597), // Waiting for Call...
                        //("Answering Canceled", 598), // Answering Canceled
                        ("TEXT_UNIT_TITLE_GDI_ENGINEER", 599), // Engineer
                        //("Special Options", 600), // Special Options
                        //("Targeting flash visible to all.", 601), // Targeting flash visible to all.
                        //("Allow targeting of trees.", 602), // Allow targeting of trees.
                        //("Allow undeploy of construction yard.", 603), // Allow undeploy of construction yard.
                        //("Employ smarter self defense logic.", 604), // Employ smarter self defense logic.
                        //("Moderate production speed.", 605), // Moderate production speed.
                        //("Use three point turn logic.", 606), // Use three point turn logic.
                        //("Tiberium will grow.", 607), // Tiberium will grow.
                        //("Tiberium will spread.", 608), // Tiberium will spread.
                        //("Disable building \"bib\" pieces.", 609), // Disable building \"bib\" pieces.
                        //("Allow running from immediate threats.", 610), // Allow running from immediate threats.
                        //("Not a Null Modem Cable Attached!^It is a modem or loopback cable.", 611), // Not a Null Modem Cable Attached!^It is a modem or loopback cable.
                        //("Map", 612), // Map
                        //("From Computer:", 613), // From Computer:
                        ("TEXT_COMP_TAUNT_MSG1", 614), // Prepare to die!
                        ("TEXT_COMP_TAUNT_MSG2", 615), // How about a bullet sandwich?!
                        ("TEXT_COMP_TAUNT_MSG3", 616), // Incoming!
                        ("TEXT_COMP_TAUNT_MSG4", 617), // I see you!
                        ("TEXT_COMP_TAUNT_MSG5", 618), // Hey, I'm over here!
                        ("TEXT_COMP_TAUNT_MSG6", 619), // Come get some!
                        ("TEXT_COMP_TAUNT_MSG7", 620), // I got you!
                        ("TEXT_COMP_TAUNT_MSG8", 621), // You humans are never a challenge!
                        ("TEXT_COMP_TAUNT_MSG9", 622), // Abort, Retry, Ignore? (Ha ha!)
                        ("TEXT_COMP_TAUNT_MSG10", 623), // Format another? (Just kidding!)
                        ("TEXT_COMP_TAUNT_MSG11", 624), // Beat me and I'll reboot!
                        ("TEXT_COMP_TAUNT_MSG12", 625), // You're artificial intelligence!
                        ("TEXT_COMP_TAUNT_MSG13", 626), // My AI is better than your AI.
                        ("TEXT_MUSIC_TDC_MUS_AIRSTRIKE", 627), // Air Strike
                        ("TEXT_MUSIC_TDC_MUS_DEMOLITION", 628), // Demolition
                        ("TEXT_MUSIC_TDC_MUS_UNTAMED_LAND", 629), // Untamed Land
                        ("TEXT_MUSIC_TDC_MUS_TAKE_EM_OUT", 630), // Take 'em Out
                        ("TEXT_MUSIC_TDC_MUS_RADIO", 631), // Radio
                        ("TEXT_MUSIC_TDC_MUS_RAIN_IN_THE_NIGHT", 632), // Rain In The Night
                        ("TEXT_MUSIC_TDC_MUS_CANYON_CHASE", 633), // Canyon Chase
                        ("TEXT_MUSIC_TDC_MUS_HEARTBREAK", 634), // Heartbreak
                        ("TEXT_PROP_TITLE_BLOSSOM_TREE", 635), // Blossom Tree
                        //("Restate", 636), // Restate
                        //("Computer", 637), // Computer
                        //("Unit Count:", 638), // Unit Count:
                        //("Tech Level:", 639), // Tech Level:
                        //("Opponent", 640), // Opponent
                        //("Kills:", 641), // Kills:
                        //("Video", 642), // Video
                        ("TEXT_UNIT_TITLE_C10", 643), // Nikoomba
                        ("TEXT_GAMETYPE_CF", 644), // Capture The Flag
                        ("TEXT_MUSIC_TDC_MUS_RIDE_OF_THE_VALKYRIES", 645), // Ride of the Valkyries
                        //("Mission Objective", 646), // Mission Objective
                        //("Mission", 647), // Mission
                        //("No saved games available.", 648), // No saved games available.
                        ("TEXT_STRUCTURE_CIVILIAN_TITLE", 649), // Civilian Building
                        ("TEXT_UNIT_TITLE_CIV13", 650), // Technician
                        ("TEXT_UNIT_TITLE_VICE", 651), // Visceroid
                        //("Save game options are not allowed during a multiplayer session.", 652), // Save game options are not allowed during a multiplayer session.
                        //("Defender has the advantage.", 653), // Defender has the advantage.
                        //("Show true object names.", 654), // Show true object names.
                        ("TEXT_UNIT_TITLE_DELPHI", 655), // Agent Delphi
                        //("Would you like to replay this mission?", 656), // Would you like to replay this mission?
                        //("Reconnecting to %s.", 657), // Reconnecting to %s.
                        //("Please wait %02d seconds.", 658), // Please wait %02d seconds.
                        ("TEXT_SURRENDER_CONFIRMATION", 659), // Do you wish^to surrender?
                        //("GLOBAL DEFENSE INITIATIVE", 660), // GLOBAL DEFENSE INITIATIVE
                        //("BROTHERHOOD OF NOD", 661), // BROTHERHOOD OF NOD
                        ("TEXT_SELECT_TRANSMISSION", 662), // SELECT TRANSMISSION
                        //("Your game name must be unique.", 663), // Your game name must be unique.
                        //("Game has already started.", 664), // Game has already started.
                        //("Your name must be unique.", 665), // Your name must be unique.
                        //("Reconnecting to %s", 666), // Reconnecting to %s
                        //("Waiting for connections...", 667), // Waiting for connections...
                        //("Time allowed: %02d seconds", 668), // Time allowed: %02d seconds
                        //("Press ESC to cancel.", 669), // Press ESC to cancel.
                        ("TEXT_COMP_TAUNT_NO_HUMANS", 670), // From Computer: It's just you and me now!
                        //("Capture the Flag:", 671), // Capture the Flag:
                        ("TEXT_UNIT_TITLE_CHAN", 672), // Dr. Chan
                        //("%s has allied with %s", 673), // %s has allied with %s
                        //("%s declares war on %s", 674), // %s declares war on %s
                        ("TEXT_NOD_SELECT_TARGET", 675), // Select a target
                        //("Allow separate helipad purchase", 676), // Allow separate helipad purchase
                        //("Resign Game", 677), // Resign Game
                        ("Tiberium grows quickly.", 678), // Tiberium grows quickly.
                        //("Answering...", 679), // Answering...
                        //("Initializing Modem...", 680), // Initializing Modem...
                        //("Scenarios don't match.", 681), // Scenarios don't match.
                        //("Power Output", 682), // Power Output
                        //("Power Output (low)", 683), // Power Output (low)
                        ("TEXT_CONTINUE", 684), // Continue
                        //("Data Queue Overflow", 685), // Data Queue Overflow
                        //("%s changed game options!", 686), // %s changed game options!
                        //("Please insert a Command & Conquer CD into the CD-ROM drive.", 687), // Please insert a Command & Conquer CD into the CD-ROM drive.
                        //("Please insert CD %d (%s) into the CD-ROM drive.", 688), // Please insert CD %d (%s) into the CD-ROM drive.
                        //("Command & Conquer is unable to detect your CD ROM drive.", 689), // Command & Conquer is unable to detect your CD ROM drive.
                        //("No Sound Card Detected", 690), // No Sound Card Detected
                        //("UNKNOWN", 691), // UNKNOWN
                        //("(old)", 692), // (old)
                        //("Insufficient Disk Space to run Command & Conquer.", 693), // Insufficient Disk Space to run Command & Conquer.
                        //("You must have %d megabytes of free disk space.", 694), // You must have %d megabytes of free disk space.
                        //("Run SETUP program first.", 695), // Run SETUP program first.
                        //("Waiting for Opponent", 696), // Waiting for Opponent
                        //("Please select 'Settings' to setup default configuration", 697), // Please select 'Settings' to setup default configuration
                        ("TEXT_STRUCTURE_TITLE_CIV35", 698), // Prison
                        ("TEXT_GAME_SAVED", 699), // Game Saved
                        //("Insufficient disk space to save a game.  Please delete a previous save to free up some disk space and try again.", 700), // Insufficient disk space to save a game.  Please delete a previous save to free up some disk space and try again.
                        //("Invalid Port/Address.^COM 1-4 OR ADDRESS", 701), // Invalid Port/Address.^COM 1-4 OR ADDRESS
                        //("Invalid Port and/or IRQ settings", 702), // Invalid Port and/or IRQ settings
                        //("IRQ already in use", 703), // IRQ already in use
                        //("Abort", 704), // Abort
                        ("TEXT_RESTART", 705), // Restart
                        //("Mission is restarting.^Please wait...", 706), // Mission is restarting.^Please wait...
                        //("TEXT_SAVE_LOAD_LOADING_MESSAGE", 707), // Mission is loading.^Please wait...
                        //("Error in the InitString", 708), // Error in the InitString
                        //("Order Info", 709), // Order Info
                        //("Scenes", 710), // Scenes
                        //("New Missions", 711), // New Missions
                        ("TEXT_MUSIC_TDC_MUS_DEPTH_CHARGE", 712), // Depth Charge
                        ("TEXT_MUSIC_TDC_MUS_DRONE", 713), // Drone
                        ("TEXT_MUSIC_TDC_MUS_IRON_FIST", 714), // Iron Fist
                        ("TEXT_MUSIC_TDC_MUS_CREEPING_UPON", 715), // Creeping Upon
                        ("TEXT_MUSIC_TDC_MUS_CC_80S_MIX", 716), // C&C 80's Mix
                        ("TEXT_MUSIC_TDC_MUS_DRILL", 717), // Drill
                        //("Please insert the Covert Missions CD into the CD-ROM drive.", 718), // Please insert the Covert Missions CD into the CD-ROM drive.
                        ("TEXT_MUSIC_TDC_MUS_RECON", 719), // Recon
                        ("TEXT_MUSIC_TDC_MUS_VOICE_RHYTHM", 720), // Voice Rhythm
                        //("Error - modem did not respond to initialization string.", 721), // Error - modem did not respond to initialization string.
                        //("Error - Modem failed to respond to flow control command. Your Windows modem configuration may be incorrect.", 722), // Error - Modem failed to respond to flow control command. Your Windows modem configuration may be incorrect.
                        //("Error - Modem failed to respond to compression command. Your Windows modem configuration may be incorrect.", 723), // Error - Modem failed to respond to compression command. Your Windows modem configuration may be incorrect.
                        //("Error - Modem failed to respond to error correction command. Your Windows modem configuration may be incorrect.", 724), // Error - Modem failed to respond to error correction command. Your Windows modem configuration may be incorrect.
                        //("Error - unable to disable modem auto answer.", 725), // Error - unable to disable modem auto answer.
                        //("Error - Too many errors initializing modem - Aborting.", 726), // Error - Too many errors initializing modem - Aborting.
                        //("Ignore", 727), // Ignore
                        //("Connecting... Please Wait.", 728), // Connecting... Please Wait.
                        //("To play Command & Conquer via the internet you must be connected to an internet services provider and be registered with Planet Westwood", 729), // To play Command & Conquer via the internet you must be connected to an internet services provider and be registered with Planet Westwood
                        //("Register", 730), // Register
                        //("Wchat not installed. Please install it from either CD.", 731), // Wchat not installed. Please install it from either CD.
                        ("TEXT_MAIN_MENU_ONLINE", 732), // Internet Game
                        //("Command & Conquer is unable to detect your mouse driver", 733), // Command & Conquer is unable to detect your mouse driver
                        //("Error - Unable to allocate primary video buffer - aborting.", 734), // Error - Unable to allocate primary video buffer - aborting.
                        //("No dial tone. Ensure your modem is connected to the phone line and try again.", 735), // No dial tone. Ensure your modem is connected to the phone line and try again.
                        //("Modem Initialization", 736), // Modem Initialization
                        //("Data Compression", 737), // Data Compression
                        //("Error Correction", 738), // Error Correction
                        //("Hardware Flow Control", 739), // Hardware Flow Control
                        //("Advanced", 740), // Advanced
                    }.Select(val => new KeyValuePair<string, int>(val.Item1, val.Item2)));
                }
                else if (gameType == GameType.RedAlert)
                {
                    this.gameTextMapping.Union(new (string, int)[]
                    {
                        ("", 0),
                    }.Select(val => new KeyValuePair<string, int>(val.Item1, val.Item2)));
                }
            }
        }
    }
}
