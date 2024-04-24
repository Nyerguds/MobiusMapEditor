//         DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//                     Version 2, December 2004
//
//  Copyright (C) 2004 Sam Hocevar<sam@hocevar.net>
//
//  Everyone is permitted to copy and distribute verbatim or modified
//  copies of this license document, and changing it is allowed as long
//  as the name is changed.
//
//             DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
//    TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
//
//   0. You just DO WHAT THE FUCK YOU WANT TO.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MobiusEditor.Interface;
using MobiusEditor.Model;
using MobiusEditor.Utility;

namespace MobiusEditor.TiberianDawn
{
    public class GameInfoTibDawn : GameInfo
    {
        public override GameType GameType => GameType.TiberianDawn;
        public override string Name => "Tiberian Dawn";
        public override string DefaultSaveDirectory => Path.Combine(Globals.RootSaveDirectory, "Tiberian_Dawn");
        public override string OpenFilter => Constants.FileFilter;
        public override string SaveFilter => Constants.FileFilter;
        public override string DefaultExtension => ".ini";
        public override string DefaultExtensionFromMix => ".ini";
        public override string ModFolder => Path.Combine(Globals.ModDirectory, "Tiberian_Dawn");
        public override string ModIdentifier => "TD";
        public override string ModsToLoad => Properties.Settings.Default.ModsToLoadTD;
        public override string ModsToLoadSetting => "ModsToLoadTD";
        public override string WorkshopTypeId => "TD";
        public override string ClassicFolder => Properties.Settings.Default.ClassicPathTD;
        public override string ClassicFolderRemaster => "CNCDATA\\TIBERIAN_DAWN\\CD1";
        public override string ClassicFolderDefault => "Classic\\TD\\";
        public override string ClassicFolderSetting => "ClassicPathTD";
        public override string ClassicStringsFile => "conquer.eng";
        public override string ClassicFontTriggers => "scorefnt.fnt";
        public override TheaterType[] AllTheaters => TheaterTypes.GetAllTypes().ToArray();
        public override TheaterType[] AvailableTheaters => TheaterTypes.GetTypes().ToArray();
        public override bool MegamapSupport => true;
        public override bool MegamapOptional => true;
        public override bool MegamapDefault => false;
        public override bool MegamapOfficial => false;
        public override bool HasSinglePlayer => true;
        public override int MaxTriggers => Constants.MaxTriggers;
        public override int MaxTeams => Constants.MaxTeams;
        public override int HitPointsGreenMinimum => 127;
        public override int HitPointsYellowMinimum => 63;
        public override OverlayTypeFlag OverlayIconType => OverlayTypeFlag.Crate;
        public override IGamePlugin CreatePlugin(Boolean mapImage, Boolean megaMap) => new GamePluginTD(mapImage, megaMap);

        public override void InitClassicFiles(MixfileManager mfm, List<string> loadErrors, List<string> fileLoadErrors, bool forRemaster)
        {
            mfm.Reset(GameType.None, null);
            // Contains cursors / strings file
            mfm.LoadArchive(GameType.TiberianDawn, "local.mix", false);
            mfm.LoadArchive(GameType.TiberianDawn, "cclocal.mix", false);
            // Mod addons
            mfm.LoadArchives(GameType.TiberianDawn, "sc*.mix", false);
            mfm.LoadArchive(GameType.TiberianDawn, "conquer.mix", false);
            // Theaters
            foreach (TheaterType tdTheater in AllTheaters)
            {
                StartupLoader.LoadClassicTheater(mfm, GameType.TiberianDawn, tdTheater, false);
            }
            // Check files.
            mfm.Reset(GameType.TiberianDawn, null);
            List<string> loadedFiles = mfm.ToList();
            const string prefix = "TD: ";
            if (!forRemaster)
            {
                StartupLoader.TestMixExists(loadedFiles, loadErrors, prefix, "local.mix", "cclocal.mix");
                StartupLoader.TestMixExists(loadedFiles, loadErrors, prefix, "conquer.mix");
            }
            foreach (TheaterType tdTheater in AllTheaters)
            {
                StartupLoader.TestMixExists(loadedFiles, loadErrors, prefix, tdTheater, !tdTheater.IsModTheater);
            }
            if (!forRemaster)
            {
                StartupLoader.TestFileExists(mfm, fileLoadErrors, prefix, "conquer.eng");
            }
        }

        public override string GetClassicOpposingPlayer(string player) => HouseTypes.GetClassicOpposingPlayer(player);
        
        public override bool SupportsMapLayer(MapLayerFlag mlf)
        {
            MapLayerFlag badLayers = MapLayerFlag.BuildingFakes | MapLayerFlag.EffectRadius | MapLayerFlag.FootballArea;
            return mlf == (mlf & ~badLayers);
        }

        public override Bitmap GetWaypointIcon()
        {
            return GetTile("beacon", 0, "mouse", 12);
        }

        public override Bitmap GetCellTriggerIcon()
        {
            return GetTile("mine", 3, "mine.shp", 3);
        }

        public override Bitmap GetSelectIcon()
        {
            // Remaster: Chronosphere cursor from TEXTURES_SRGB.MEG
            // Alt: @"DATA\ART\TEXTURES\SRGB\ICON_IONCANNON_15.DDS
            // Classic: Ion Cannon cursor
            return GetTexture(@"DATA\ART\TEXTURES\SRGB\ICON_SELECT_GREEN_04.DDS", "mouse", 118);
        }

        public override string EvaluateBriefing(string briefing)
        {
            if (!Globals.WriteClassicBriefing)
            {
                return null;
            }
            string briefText = (briefing ?? String.Empty).Replace('\t', ' ').Trim('\r', '\n', ' ').Replace("\r\n", "\n").Replace("\r", "\n");
            // Remove duplicate spaces
            briefText = Regex.Replace(briefText, " +", " ");
            if (briefText.Length > Constants.MaxBriefLengthClassic)
            {
                return "Classic Tiberian Dawn briefings cannot exceed " + Constants.MaxBriefLengthClassic + " characters. This includes line breaks.\n\nThis will not affect the mission when playing in the Remaster, but the briefing will be truncated when playing in the original game.";
            }
            return null;
        }

        public override bool MapNameIsEmpty(string name)
        {
            return String.IsNullOrEmpty(name) || Constants.EmptyMapName.Equals(name, StringComparison.OrdinalIgnoreCase);
        }

        public override TeamRemap GetClassicFontTriggerRemap(TilesetManagerClassic tsmc, Color textColor)
        {
            return GetClassicFontRemapSimple(ClassicFontTriggers, tsmc, textColor);
        }

        private static readonly string[] unitVocNames = new string[]
        {
            /*
            **        Infantry and vehicle responses.
            */
            "2dangr1", // VOC_2DANGER        "negative, too dangerous"
            "ackno",   // VOC_ACKNOWL        "acknowledged"
            "affirm1", // VOC_AFFIRM         "affirmative"
            "await1",  // VOC_AWAIT1         "awaiting orders"
            "backup",  // VOC_BACKUP         "send backup"
            "help",    // VOC_HELP           "send help"
            "movout1", // VOC_MOVEOUT        "movin' out"
            "negatv1", // VOC_NEGATIVE       "negative"
            "noprob",  // VOC_NO_PROB        "not a problem"
            "ready",   // VOC_READY          "ready and waiting"
            "report1", // VOC_REPORT         "reporting"
            "ritaway", // VOC_RIGHT_AWAY     "right away sir"
            "roger",   // VOC_ROGER          "roger"
            "sir1",    // VOC_SIR1           "sir?"
            "squad1",  // VOC_SQUAD1         "squad reporting"
            "target1", // VOC_PRACTICE       "target practice"
            "ugotit",  // VOC_UGOTIT         "you got it"
            "unit1",   // VOC_UNIT1          "unit reporting"
            "vehic1",  // VOC_VEHIC1         "vehicle reporting"
            "yessir1", // VOC_YESSIR         "yes sir"
        };

        private static readonly string[] soundFiles = new string[]
        {
            "struggle", // Intro static
            // Special voices (typically associated with the commando).
            "bombit1",  // VOC_RAMBO_PRESENT "I've got a present for ya"
            "cmon1",    // VOC_RAMBO_CMON    "c'mon"
            "gotit1",   // VOC_RAMBO_UGOTIT  "you got it"
            "keepem1",  // VOC_RAMBO_COMIN   "keep 'em commin'"
            "laugh1",   // VOC_RAMBO_LAUGH   "hahaha"
            "lefty1",   // VOC_RAMBO_LEFTY   "that was left handed"
            "noprblm1", // VOC_RAMBO_NOPROB  "no problem"
            "ohsh1",    // VOC_RAMBO_OHSH    "oh shiiiiii...."
            "onit1",    // VOC_RAMBO_ONIT    "I'm on it"
            "ramyell1", // VOC_RAMBO_YELL    "ahhhhhhh"
            "rokroll1", // VOC_RAMBO_ROCK    "time to rock and roll"
            "tuffguy1", // VOC_RAMBO_TUFF    "real tuff guy"
            "yeah1",    // VOC_RAMBO_YEA     "yea"
            "yes1",     // VOC_RAMBO_YES     "yes"
            "yo1",      // VOC_RAMBO_YO      "yo"
            // Civilian voices (technicians too).
            "girlokay", // VOC_GIRL_OKAY     "Okay"
            "girlyeah", // VOC_GIRL_YEAH     "Yeah?"
            "guyokay1", // VOC_GUY_OKAY      "Okay"
            "guyyeah1", // VOC_GUY_YEAH      "Yeah?"
            // Sound effects that have a juvenile counterpart.
            "bazook1",  // VOC_BAZOOKA       Gunfire
            "bleep2",   // VOC_BLEEP         Clean metal bing
            "bomb1",    // VOC_BOMB1         Crunchy parachute bomb type explosion
            "button",   // VOC_BUTTON        Dungeon Master button click
            "comcntr1", // VOC_RADAR_ON      Elecronic static with beeps
            "constru2", // VOC_CONSTRUCTION  construction sounds
            "crumble",  // VOC_CRUMBLE       muffled crumble sound
            "flamer2",  // VOC_FLAMER1       flame thrower
            "gun18",    // VOC_RIFLE         rifle shot
            "gun19",    // VOC_M60           machine gun burst -- 6 rounds
            "gun20",    // VOC_GUN20         bat hitting heavy metal door
            "gun5",     // VOC_M60A          medium machine gun burst
            "gun8",     // VOC_MINI          mini gun burst
            "gunclip1", // VOC_RELOAD        gun clip reload
            "hvydoor1", // VOC_SLAM          metal plates slamming together
            "hvygun10", // VOC_HVYGUN10      loud sharp cannon
            "ion1",     // VOC_ION_CANNON    partical beam
            "mgun11",   // VOC_MGUN11        alternate tripple burst
            "mgun2",    // VOC_MGUN2         M-16 tripple burst
            "nukemisl", // VOC_NUKE_FIRE     long missile sound
            "nukexplo", // VOC_NUKE_EXPLODE  long but not loud explosion
            "obelray1", // VOC_LASER         humming laser beam
            "obelpowr", // VOC_LASER_POWER   warming-up sound of laser beam
            "powrdn1",  // VOC_RADAR_OFF     doom door slide
            "ramgun2",  // VOC_SNIPER        silenced rifle fire
            "rocket1",  // VOC_ROCKET1       rocket launch variation #1
            "rocket2",  // VOC_ROCKET2       rocket launch variation #2
            "sammotr2", // VOC_MOTOR         dentists drill
            "scold2",   // VOC_SCOLD         cannot perform action feedback tone
            "sidbar1c", // VOC_SIDEBAR_OPEN  xylophone clink
            "sidbar2c", // VOC_SIDEBAR_CLOSE xylophone clink
            "squish2",  // VOC_SQUISH2       crushing infantry
            "tnkfire2", // VOC_TANK1         sharp tank fire with recoil
            "tnkfire3", // VOC_TANK2         sharp tank fire
            "tnkfire4", // VOC_TANK3         sharp tank fire
            "tnkfire6", // VOC_TANK4         big gun tank fire
            "tone15",   // VOC_UP            credits counting up
            "tone16",   // VOC_DOWN          credits counting down
            "tone2",    // VOC_TARGET        target sound
            "tone5",    // VOC_SONAR         sonar echo
            "toss",     // VOC_TOSS          air swish
            "trans1",   // VOC_CLOAK         stealth tank
            "treebrn1", // VOC_BURN          burning crackle
            "turrfir5", // VOC_TURRET        muffled gunfire
            "xplobig4", // VOC_XPLOBIG4      very long muffled explosion
            "xplobig6", // VOC_XPLOBIG6      very long muffled explosion
            "xplobig7", // VOC_XPLOBIG7      very long muffled explosion
            "xplode",   // VOC_XPLODE        long soft muffled explosion
            "xplos",    // VOC_XPLOS         short crunchy explosion
            "xplosml2", // VOC_XPLOSML2      muffled mechanical explosion
            // Generic sound effects (no variations).
            "nuyell1",  // VOC_SCREAM1       short infantry scream
            "nuyell3",  // VOC_SCREAM3       short infantry scream
            "nuyell4",  // VOC_SCREAM4       short infantry scream
            "nuyell5",  // VOC_SCREAM5       short infantry scream
            "nuyell6",  // VOC_SCREAM6       short infantry scream
            "nuyell7",  // VOC_SCREAM7       short infantry scream
            "nuyell10", // VOC_SCREAM10      short infantry scream
            "nuyell11", // VOC_SCREAM11      short infantry scream
            "nuyell12", // VOC_SCREAM12      short infantry scream
            "yell1",    // VOC_YELL1         long infantry scream
            "myes1",    // VOC_YES           "Yes?"
            "mcomnd1",  // VOC_COMMANDER     "Commander?"
            "mhello1",  // VOC_HELLO         "Hello?"
            "mhmmm1",   // VOC_HMMM          "Hmmm?"
            "mhaste1",  // VOC_PROCEED1      "I will proceed, post haste."
            "monce1",   // VOC_PROCEED2      "I will proceed, at once."
            "mimmd1",   // VOC_PROCEED3      "I will proceed, immediately."
            "mplan1",   // VOC_EXCELLENT1    "That is an excellent plan."
            "mplan2",   // VOC_EXCELLENT2    "Yes, that is an excellent plan."
            "mplan3",   // VOC_EXCELLENT3    "A wonderful plan."
            "maction1", // VOC_EXCELLENT4    "Astounding plan of action commander."
            "mremark1", // VOC_EXCELLENT5    "Remarkable contrivance."
            "mcourse1", // VOC_OF_COURSE     "Of course."
            "myesyes1", // VOC_YESYES        "Yes yes yes."
            "mtiber1",  // VOC_QUIP1         "Mind the Tiberium."
            "mmg1",     // VOC_QUIP2         "A most remarkable  Metasequoia Glyptostroboides."
            "mthanks1", // VOC_THANKS        "Thank you."
            "cashturn", // VOC_CASHTURN      Sound of money being piled up.
            "bleep2",   // VOC_BLEEPY3       Clean computer bleep sound.
            "dinomout", // VOC_DINOMOUT      Movin' out in dino-speak.
            "dinoyes",  // VOC_DINOYES       Yes Sir in dino-speak.
            "dinoatk1", // VOC_DINOATK1      Dino attack sound.
            "dinodie1", // VOC_DINODIE1      Dino die sound.
            //"beacon",   // VOC_BEACON        Beacon sound.
        };

        private static readonly string[] evaVoxNames = new string[]
        {
            "accom1",   // mission accomplished
            "fail1",    // your mission has failed
            "bldg1",    // unable to comply, building in progress
            "constru1", // construction complete
            "unitredy", // unit ready
            "newopt1",  // new construction options
            "deploy1",  // cannot deploy here
            "gdidead1", // GDI unit destroyed
            "noddead1", // Nod unit destroyed
            "civdead1", // civilian killed
            "evayes1",  // affirmative
            "evano1",   // negative
            "upunit1",  // upgrade complete, new unit available
            "upstruc1", // upgrade complete, new structure available
            "nocash1",  // insufficient funds
            "batlcon1", // battle control terminated
            "reinfor1", // reinforcements have arrived
            "cancel1",  // canceled
            "bldging1", // building
            "lopower1", // low power
            "nopower1", // insufficient power
            "mocash1",  // need more funds
            "baseatk1", // our base is under attack
            "income1",  // incoming missile
            "enemya",   // enemy planes approaching
            "nuke1",    // nuclear warhead approaching - VOX_INCOMING_NUKE
            "radok1",   // radiation levels are acceptable
            "radfatl1", // radiation levels are fatal
            "nobuild1", // unable to build more
            "pribldg1", // primary building selected
            "repdone1", // repairs completed
            "nodcapt1", // Nod building captured
            "gdicapt1", // GDI building captured
            "sold1",    // structure sold
            "ionchrg1", // ion cannon charging
            "ionredy1", // ion cannon ready
            "nukavail", // nuclear weapon available
            "nuklnch1", // nuclear weapon launched - VOX_NUKE_LAUNCHED
            "unitlost", // unit lost
            "strclost", // structure lost
            "needharv", // need harvester
            "select1",  // select target
            "airredy1", // airstrike ready
            "noredy1",  // not ready
            "transsee", // Nod transport sighted
            "tranload", // Nod transport loaded
            "enmyapp1", // enemy approaching
            "silos1",   // silos needed
            "onhold1",  // on hold
            "repair1",  // repairing
            "estrucx",  // enemy structure destroyed
            "gstruc1",  // GDI structure destroyed
            "nstruc1",  // NOD structure destroyed
            "enmyunit", // Enemy unit destroyed
        };

        private static readonly string[] superweapons = new string[]
        {
            "ion",
            "atom",
            "bomb"
        };

        private static readonly string[] animations = new string[]
        {
            "samfire",
            "smokland",
            // Flammable object burning animations. Primarily used on trees and buildings.
            "burn-s",
            "burn-m",
            "burn-l",
            // Flame thrower animations. These are direction specific.
            "flame-n",
            "flame-nw",
            "flame-w",
            "flame-sw",
            "flame-s",
            "flame-se",
            "flame-e",
            "flame-ne",
            // Chem sprayer animations. These are direction specific.
            "chem-n",
            "chem-nw",
            "chem-w",
            "chem-sw",
            "chem-s",
            "chem-se",
            "chem-e",
            "chem-ne",
            "veh-hit2",
            "fball1",
            "frag1",
            "frag3",
            "veh-hit1",
            "veh-hit2",
            "veh-hit3",
            "art-exp1",
            "napalm1",
            "napalm2",
            "napalm3",
            "smokey",
            "piff",
            "piffpiff",
            "fire3",
            "fire1",
            "fire4",
            "fire2",
            "flmspt",
            "gunfire",
            "smoke_m",
            // Mini-gun fire effect -- used by guard towers.
            "minigun",
            // Superweapons
            "ionsfx",
            "atomsfx",
            "atomdoor",
            // Crates
            "deviator",
            "dollar",
            "earth",
            "empulse",
            "invun",
            "mine",
            "rapid",
            "stealth2",
            "missile2",
            // Movement flash
            "moveflsh",
            "chemball",
            // Flag animation
            "flagfly",
            // bullet type graphics
            "50cal",
            "120mm",
            "dragon",
            "missile",
            "flame",
            "bomblet",
            "bomb",
            "laser",    // Doesn't actually exist as graphics
            "atomicup",
            "atomicdn",
            "patriot",
            "gore",     // Doesn't actually exist as graphics
            "chew",     // Doesn't actually exist as graphics
            // additional unit/building anims
            "select",   // Repairing animation
            "rrotor",
            "lrotor",
            "wake",
            // shroud edges
            "shadow",
            // UI shapes
            "clock",
            "pips",
            "power",
            "tabs",
            "strip",
            "stripup",
            "stripdn",
            "btn-pl",
            "btn-st",
            "btn-up",
            "btn-dn",
            // Hi-res UI shapes
            "hclock",
            "hmap",
            "hmapf",    // French equivalent in C&C95 v1.06
            "hmapg",    // German equivalent in C&C95 v1.06
            "hsell",
            "hsellf",   // French equivalent in C&C95 v1.06
            "hsellg",   // German equivalent in C&C95 v1.06
            "hrepair",
            "hrepairf", // French equivalent in C&C95 v1.06
            "hrepairg", // German equivalent in C&C95 v1.06
            "hpips",
            "hpips_f",  // French equivalent in C&C95 v1.06
            "hpips_g",  // German equivalent in C&C95 v1.06
            "hside1",
            "hside2",
            "hpower",
            "hpwrbar",
            "htabs",
            "hstrip",
            "hstripup",
            "hstripdn",
            "btn-plh",
            "btn-sth",
            "hbtn-up",
            "hbtn-up2",
            "hbtn-dn",
            "hbtn-dn2",
            "options",  // Various decorations for options screens
            "btexture",
            // Score screen anims
            "creds",
            "time",
            "logos",
            "hiscore1",
            "hiscore2",
            "bar3ylw",
            "bar3red",
        };

        private static readonly string[] animationScenes = new string[]
        {
            "choose",
            "greyerth",
            "e-bwtocl",
            "earth_e",
            "earth_a",
            "europe",
            "bosnia",
            "africa",
            "s_africa",
            "hearth_e",
            "hearth_a",
            "hbosnia",
            "hsafrica",
            "s-gdiin2",
            "scrscn1",
            "mltiplyr",
            "mltsceng", // Added by the v1.06 patch
            "mltscfre", // Added by the v1.06 patch
            "mltscger", // Added by the v1.06 patch
        };

        private static readonly string[] fadingTables = new string[]
        {
            "green",
            "yellow",
            "red",
            "mouse",
            "trans",
            "white",
            "shadow",
            "units",
            "shade",
            "light",
            "clock",
        };

        private static readonly string[] additionalTheaterFiles = new string[]
        {
            "sr1", // Beta bibs
            "sr2", // Beta bibs
        };

        private static readonly string[] additionalFiles = new string[]
        {
            "sides.pal",
            "map1.pal",
            "map_locl.pal",
            "map_gry2.pal",
            "map_prog.pal",
            "map_prob.pal", // Missing stretch table for africa.wsa, added by v1.06
            "lastscng.pal",
            "lastscnb.pal",
            "dark_e.pal",   // Dark palette for Europe and Africa maps
            "map_loc2.pal", // stretch table for dark_e.pal
            "dark_b.pal",   // Dark palette for Bosnia map
            "map_loc3.pal", // stretch table for dark_b.pal
            "dark_sa.pal",  // Dark palette for South-Africa map
            "map_loc4.pal", // Missing stretch table for dark_sa.pal; Added by v1.06
            "satsel.cps",
            "satsel.pal",   // Palette, not stretch table. Unsure if used.
            "satselin.pal", // stretch table for satsel
            "scorpal1.pal",
            "snodpal1.pal",
            "multscor.pal",
            "countrye.shp",
            "countrya.shp",
            "pump.shp",
            "pumpicon.shp",
            "pumpmake.shp",
            "roadicon.shp",
            // Placement grids
            "trans.icn",
            // Radar logos
            "radar.gdi",
            "radar.nod",
            "radar.jp",
            "hradar.gdi",
            "hradar.nod",
            "hradar.jp",
            "hradar.ww",    // Added by C&C95 v1.06
            "hradar.ea",    // Added by C&C95 v1.06
            // C&C95 v1.06 inis
            "lang_eng.ini",
            "lang_fre.ini",
            "lang_ger.ini",
            "lang_jap.ini",
            "rules.ini",
            "themes.ini",
        };

        public override IEnumerable<string> GetGameFiles()
        {
            foreach (string name in GetMissionFiles())
            {
                yield return name;
            }
            foreach (string name in GetGraphicsFiles(TheaterTypes.GetAllTypes()))
            {
                yield return name;
            }
            foreach (string name in GetMediaFiles())
            {
                yield return name;
            }
            foreach (string name in GetAudioFiles())
            {
                yield return name;
            }
            foreach (string name in additionalFiles)
            {
                yield return name;
            }
        }

        public static IEnumerable<string> GetMissionFiles()
        {
            const string iniExt = ".ini";
            const string binExt = ".bin";
            const string cpsExt = ".cps";
            char[] sides = { 'g', 'b' };
            string[] suffixes = { "ea", "eb", "ec", "ed", "ee", "wa", "wb", "wc", "wd", "we", "xa", "xb", "xc", "xd", "xe" };
            string mainSuffix = suffixes[0];
            // Campaign and expansion missions
            for (int c = 0; c < sides.Length; ++c)
            {
                char campaign = sides[c];
                // Main campaigns, and v1.06 minicampaigns
                for (int i = 1; i < 100; ++i)
                {
                    for (int s = 0; s < suffixes.Length; ++s)
                    {
                        string missionName = GetMissionName(campaign, i, suffixes[s]);
                        yield return missionName + iniExt;
                        yield return missionName + binExt;
                        yield return missionName + cpsExt;
                    }
                }
                // Expansion missions (no minicampaigns on these; too much collision)
                for (int i = 100; i < 900; ++i)
                {
                    string missionName = GetMissionName(campaign, i, mainSuffix);
                    yield return missionName + iniExt;
                    yield return missionName + binExt;
                    yield return missionName + cpsExt;
                }
                //  v1.06 minicampaigns on 900 and beyond.
                for (int i = 900; i < 1000; ++i)
                {
                    for (int s = 0; s < suffixes.Length; ++s)
                    {
                        string missionName = GetMissionName(campaign, i, suffixes[s]);
                        yield return missionName + iniExt;
                        yield return missionName + binExt;
                        yield return missionName + cpsExt;
                    }
                }
            }
            for (int i = 1; i < 20; ++i)
            {
                string missionName = GetMissionName('j', i, mainSuffix);
                yield return missionName + iniExt;
                yield return missionName + binExt;
                yield return missionName + cpsExt;
            }
            for (int i = 0; i < 1000; ++i)
            {
                string missionName = GetMissionName('m', i, mainSuffix);
                yield return missionName + iniExt;
                yield return missionName + binExt;
            }
        }

        public static IEnumerable<string> GetGraphicsFiles(IEnumerable<TheaterType> theaterTypes)
        {
            const string shpExt = ".shp";
            const string palExt = ".pal";
            const string mrfExt = ".mrf";
            string[] theaterExts = theaterTypes.Where(th => !th.IsModTheater).Select(tt => "." + tt.ClassicExtension.Trim('.')).ToArray();
            string[] extraThExts = theaterTypes.Where(th => th.IsModTheater).Select(tt => "." + tt.ClassicExtension.Trim('.')).ToArray();
            string[] theaterRoots = theaterTypes.Where(th => !th.IsModTheater).Select(tt => tt.ClassicTileset).ToArray();
            string[] extraThRoots = theaterTypes.Where(th => th.IsModTheater).Select(tt => tt.ClassicTileset).ToArray();
            // Files used / listed in the editor data

            // General
            for (int i = 0; i < theaterRoots.Length; ++i)
            {
                yield return theaterRoots[i] + palExt;
            }
            // Templates
            foreach (TemplateType tmp in TemplateTypes.GetTypes())
            {
                string name = tmp.Name;
                for (int i = 0; i < theaterExts.Length; ++i)
                {
                    yield return name + theaterExts[i];
                }
            }
            // Additional fields that use the theater extension.
            foreach (string thf in additionalTheaterFiles)
            {
                for (int i = 0; i < theaterExts.Length; ++i)
                {
                    yield return thf + theaterExts[i];
                }
            }
            // Buildings, with icons and build-up animations
            foreach (BuildingType bt in BuildingTypes.GetTypes())
            {
                string name = bt.Name;
                yield return name + shpExt;
                yield return name + "make" + shpExt;
                yield return name + "icon" + shpExt;
                if (bt.FactoryOverlay != null)
                {
                    yield return bt.FactoryOverlay + shpExt;
                }
                for (int i = 0; i < theaterExts.Length; ++i)
                {
                    string thExt = theaterExts[i];
                    yield return name + thExt;
                    yield return name + "make" + thExt;
                    yield return name + "icnh" + thExt;
                }
            }
            // Superweapon icons
            foreach (string sw in superweapons)
            {
                yield return sw + "icon" + shpExt;
                for (int i = 0; i < theaterExts.Length; ++i)
                {
                    string thExt = theaterExts[i];
                    yield return sw + "icnh" + thExt;
                }
            }
            // Smudge
            foreach (SmudgeType sm in SmudgeTypes.GetTypes(false))
            {
                string name = sm.Name;
                for (int i = 0; i < theaterExts.Length; ++i)
                {
                    yield return name + theaterExts[i];
                }
            }
            // Terrain
            foreach (TerrainType tr in TerrainTypes.GetTypes())
            {
                string name = tr.Name;
                for (int i = 0; i < theaterExts.Length; ++i)
                {
                    yield return name + theaterExts[i];
                }
            }
            // Infantry
            foreach (InfantryType it in InfantryTypes.GetTypes())
            {
                string name = it.Name;
                yield return name + shpExt;
                yield return name + "icon" + shpExt;
                yield return name + "rot" + shpExt;
                for (int i = 0; i < theaterExts.Length; ++i)
                {
                    yield return name + "icnh" + theaterExts[i];
                }
            }
            // Units
            foreach (UnitType un in UnitTypes.GetTypes(false))
            {
                string name = un.Name;
                yield return name + shpExt;
                yield return name + "icon" + shpExt;
                for (int i = 0; i < theaterExts.Length; ++i)
                {
                    yield return name + "icnh" + theaterExts[i];
                }
            }
            // Overlay
            foreach (OverlayType ov in OverlayTypes.GetAllTypes())
            {
                string name = ov.GraphicsSource;
                yield return name + shpExt;
                // Tiberium exists per theater, and walls are buildable and have icons.
                if (ov.IsWall)
                {
                    yield return name + "icon" + shpExt;
                }
                if (ov.IsTiberiumOrGold || ov.IsWall)
                {
                    for (int i = 0; i < theaterExts.Length; ++i)
                    {
                        if (ov.IsTiberiumOrGold)
                        {
                            yield return name + theaterExts[i];
                        }
                        if (ov.IsWall)
                        {
                            yield return name + "icnh" + theaterExts[i];
                        }
                    }
                }
            }
            // Animations; just a bunch of misc shp files.
            foreach (string animName in animations)
            {
                yield return animName + shpExt;
            }
            // Fading tables: fetched per theater
            foreach (string table in fadingTables)
            {
                for (int i = 0; i < theaterRoots.Length; ++i)
                {
                    yield return theaterRoots[i][0] + table + mrfExt;
                }
            }

            // Extra theaters

            // General
            for (int i = 0; i < extraThRoots.Length; ++i)
            {
                yield return extraThRoots[i] + palExt;
            }
            // Templates
            foreach (TemplateType tmp in TemplateTypes.GetTypes())
            {
                string name = tmp.Name;
                for (int i = 0; i < extraThExts.Length; ++i)
                {
                    yield return name + extraThExts[i];
                }
            }
            // Additional fiels that use the theater extension.
            foreach (string thf in additionalTheaterFiles)
            {
                for (int i = 0; i < extraThExts.Length; ++i)
                {
                    yield return thf + extraThExts[i];
                }
            }
            // Buildings, with icons and build-up animations
            foreach (BuildingType bt in BuildingTypes.GetTypes())
            {
                string name = bt.Name;
                for (int i = 0; i < extraThExts.Length; ++i)
                {
                    string thExt = extraThExts[i];
                    yield return name + thExt;
                    yield return name + "make" + thExt;
                    yield return name + "icnh" + thExt;
                }
            }
            // Superweapon icons
            foreach (string sw in superweapons)
            {
                for (int i = 0; i < extraThExts.Length; ++i)
                {
                    yield return sw + "icnh" + extraThExts[i];
                }
            }
            // Smudge
            foreach (SmudgeType sm in SmudgeTypes.GetTypes(false))
            {
                string name = sm.Name;
                for (int i = 0; i < extraThExts.Length; ++i)
                {
                    yield return name + extraThExts[i];
                }
            }
            // Terrain
            foreach (TerrainType tr in TerrainTypes.GetTypes())
            {
                string name = tr.Name;
                for (int i = 0; i < extraThExts.Length; ++i)
                {
                    yield return name + extraThExts[i];
                }
            }
            // Infantry
            foreach (InfantryType it in InfantryTypes.GetTypes())
            {
                string name = it.Name;
                for (int i = 0; i < extraThExts.Length; ++i)
                {
                    yield return name + "icnh" + extraThExts[i];
                }
            }
            // Units
            foreach (UnitType un in UnitTypes.GetTypes(false))
            {
                string name = un.Name;
                for (int i = 0; i < extraThExts.Length; ++i)
                {
                    yield return name + "icnh" + extraThExts[i];
                }
            }
            // Overlay (tiberium and C&C95 icons for walls)
            foreach (OverlayType ov in OverlayTypes.GetAllTypes())
            {
                // Tiberium exists per theater, and walls are buildable and have icons.
                if (ov.IsTiberiumOrGold || ov.IsWall)
                {
                    string name = ov.GraphicsSource;
                    for (int i = 0; i < extraThExts.Length; ++i)
                    {
                        if (ov.IsTiberiumOrGold)
                        {
                            yield return name + extraThExts[i];
                        }
                        if (ov.IsWall)
                        {
                            yield return name + "icnh" + extraThExts[i];
                        }
                    }
                }
            }
            // Fading tables: fetched per theater
            foreach (string table in fadingTables)
            {
                for (int i = 0; i < extraThRoots.Length; ++i)
                {
                    yield return extraThRoots[i][0] + table + mrfExt;
                }
            }
        }

        public static IEnumerable<string> GetMediaFiles()
        {
            const string vqaExt = ".vqa";
            const string vqpExt = ".vqp";
            const string audExt = ".aud";
            const string varExt = ".var";
            const string wsaExt = ".wsa";
            // Videos
            foreach (string vidName in GamePluginTD.Movies)
            {
                yield return vidName.ToLowerInvariant() + vqaExt;
                yield return vidName.ToLowerInvariant() + vqpExt;
            }
            // Music
            foreach (string audName in GamePluginTD.Themes)
            {
                yield return audName.ToLowerInvariant() + audExt;
                yield return audName.ToLowerInvariant() + varExt;
            }
            foreach (string wsaName in animationScenes)
            {
                yield return wsaName + wsaExt;
            }
        }

        public static IEnumerable<string> GetAudioFiles()
        {
            const string audExt = ".aud";
            foreach (string voc in soundFiles)
            {
                yield return voc + audExt;
            }
            foreach (string vox in unitVocNames)
            {
                yield return vox + ".v00";
                yield return vox + ".v01";
                yield return vox + ".v02";
                yield return vox + ".v03";
            }
            foreach (string vox in evaVoxNames)
            {
                yield return vox + audExt;
            }
        }

    }
}
