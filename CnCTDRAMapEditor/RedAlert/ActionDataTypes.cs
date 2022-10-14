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
namespace MobiusEditor.RedAlert
{
    public static class ActionDataTypes
    {
        public enum ThemeType
        {
            THEME_QUIET = -3,
            THEME_PICK_ANOTHER = -2,
            THEME_NONE = -1,
            THEME_BIGF,
            THEME_CRUS,
            THEME_FAC1,
            THEME_FAC2,
            THEME_HELL,
            THEME_RUN1,
            THEME_SMSH,
            THEME_TREN,
            THEME_WORK,
            THEME_AWAIT,
            THEME_DENSE_R,
            THEME_FOGGER1A,
            THEME_MUD1A,
            THEME_RADIO2,
            THEME_ROLLOUT,
            THEME_SNAKE,
            THEME_TERMINAT,
            THEME_TWIN,
            THEME_VECTOR1A,
            THEME_MAP,
            THEME_SCORE,
            THEME_INTRO,
            THEME_CREDITS,
            THEME_2ND_HAND,
            THEME_ARAZOID,
            THEME_BACKSTAB,
            THEME_CHAOS2,
            THEME_SHUT_IT,
            THEME_TWINMIX1,
            THEME_UNDER3,
            THEME_VR2,
            THEME_BOG,
            THEME_FLOAT_V2,
            THEME_GLOOM,
            THEME_GRNDWIRE,
            THEME_RPT,
            THEME_SEARCH,
            THEME_TRACTION,
            THEME_WASTELND,
        };

        public enum VocType
        {
            VOC_NONE = -1,
            VOC_GIRL_OKAY,          // "okay"
            VOC_GIRL_YEAH,          // "yeah?"
            VOC_GUY_OKAY,           // "okay"
            VOC_GUY_YEAH,           // "yeah?"
            VOC_MINELAY1,           // mine layer sound
            VOC_ACKNOWL,            // "acknowledged"
            VOC_AFFIRM,             // "affirmative"
            VOC_AWAIT,              // "awaiting orders"
            VOC_ENG_AFFIRM,         // Engineer: "affirmative"
            VOC_ENG_ENG,            // Engineer: "engineering"
            VOC_NO_PROB,            // "not a problem"
            VOC_READY,              // "ready and waiting"
            VOC_REPORT,             // "reporting"
            VOC_RIGHT_AWAY,         // "right away sir"
            VOC_ROGER,              // "roger"
            VOC_UGOTIT,             // "you got it"
            VOC_VEHIC,              // "vehicle reporting"
            VOC_YESSIR,             // "yes sir"
            VOC_SCREAM1,            // short infantry scream
            VOC_SCREAM3,            // short infantry scream
            VOC_SCREAM4,            // short infantry scream
            VOC_SCREAM5,            // short infantry scream
            VOC_SCREAM6,            // short infantry scream
            VOC_SCREAM7,            // short infantry scream
            VOC_SCREAM10,           // short infantry scream
            VOC_SCREAM11,           // short infantry scream
            VOC_YELL1,              // long infantry scream
            VOC_CHRONO,             // Chronosphere sound.
            VOC_CANNON1,            // Cannon sound (medium).
            VOC_CANNON2,            // Cannon sound (short).
            VOC_IRON1,
            VOC_ENG_MOVEOUT,        // Engineer: "movin' out"
            VOC_SONAR,              // sonar pulse
            VOC_SANDBAG,            // sand bag crunch
            VOC_MINEBLOW,
            VOC_CHUTE1,             // wind swoosh sound
            VOC_DOG_BARK,           // dog bark
            VOC_DOG_WHINE,          // dog whine
            VOC_DOG_GROWL2,         // strong dog growl
            VOC_FIRE_LAUNCH,        // fireball launch sound
            VOC_FIRE_EXPLODE,       // fireball explode sound
            VOC_GRENADE_TOSS,       // grenade toss
            VOC_GUN_5,              // 5 round gun burst (slow).
            VOC_GUN_7,              // 7 round gun burst (fast).
            VOC_ENG_YES,            // Engineer: "yes sir"
            VOC_GUN_RIFLE,          // Rifle shot.
            VOC_HEAL,               // Healing effect.
            VOC_DOOR,               // Hyrdrolic door.
            VOC_INVULNERABLE,       // Invulnerability effect.
            VOC_KABOOM1,            // Long explosion (muffled).
            VOC_KABOOM12,           // Very long explosion (muffled).
            VOC_KABOOM15,           // Very long explosion (muffled).
            VOC_SPLASH,             // Water splash
            VOC_KABOOM22,           // Long explosion (sharp).
            VOC_AACANON3,           // AA-Cannon
            VOC_TANYA_DIE,          // Tanya: scream
            VOC_GUN_5F,             // 5 round gun burst (fast).
            VOC_MISSILE_1,          // Missile with high tech effect.
            VOC_MISSILE_2,          // Long missile launch.
            VOC_MISSILE_3,          // Short missile launch.
            VOC_x6,
            VOC_GUN_5R,             // 5 round gun burst (rattles).
            VOC_BEEP,               // Generic beep sound.
            VOC_CLICK,              // Generic click sound.
            VOC_SILENCER,           // Silencer.
            VOC_CANNON6,            // Long muffled cannon shot.
            VOC_CANNON7,            // Sharp mechanical cannon fire.
            VOC_TORPEDO,            // Torpedo launch.
            VOC_CANNON8,            // Sharp cannon fire.
            VOC_TESLA_POWER_UP,     // Hum charge up.
            VOC_TESLA_ZAP,          // Tesla zap effect.
            VOC_SQUISH,             // Squish effect.
            VOC_SCOLD,              // Scold bleep.
            VOC_RADAR_ON,           // Powering up electronics.
            VOC_RADAR_OFF,          // B movie power down effect.
            VOC_PLACE_BUILDING_DOWN,// Building slam down sound.
            VOC_KABOOM30,           // Short explosion (HE).
            VOC_KABOOM25,           // Short growling explosion.
            VOC_x7,
            VOC_DOG_HURT,           // Dog whine.
            VOC_DOG_YES,            // Dog 'yes sir'.
            VOC_CRUMBLE,            // Building crumble.
            VOC_MONEY_UP,           // Rising money tick.
            VOC_MONEY_DOWN,         // Falling money tick.
            VOC_CONSTRUCTION,       // Building construction sound.
            VOC_GAME_CLOSED,        // Long bleep.
            VOC_INCOMING_MESSAGE,   // Soft happy warble.
            VOC_SYS_ERROR,          // Sharp soft warble.
            VOC_OPTIONS_CHANGED,    // Mid range soft warble.
            VOC_GAME_FORMING,       // Long warble.
            VOC_PLAYER_LEFT,        // Chirp sequence.
            VOC_PLAYER_JOINED,      // Reverse chirp sequence.
            VOC_DEPTH_CHARGE,       // Distant explosion sound.
            VOC_CASHTURN,           // Airbrake.
            VOC_TANYA_CHEW,         // Tanya: "Chew on this"
            VOC_TANYA_ROCK,         // Tanya: "Let's rock"
            VOC_TANYA_LAUGH,        // Tanya: "ha ha ha"
            VOC_TANYA_SHAKE,        // Tanya: "Shake it baby"
            VOC_TANYA_CHING,        // Tanya: "Cha Ching"
            VOC_TANYA_GOT,          // Tanya: "That's all you got"
            VOC_TANYA_KISS,         // Tanya: "Kiss it bye bye"
            VOC_TANYA_THERE,        // Tanya: "I'm there"
            VOC_TANYA_GIVE,         // Tanya: "Give it to me"
            VOC_TANYA_YEA,          // Tanya: "Yea?"
            VOC_TANYA_YES,          // Tanya: "Yes sir?"
            VOC_TANYA_WHATS,        // Tanya: "What's up."
            VOC_WALLKILL2,          // Crushing wall sound.
            VOC_x8,
            VOC_TRIPLE_SHOT,        // Three quick shots in succession.
            VOC_SUBSHOW,            // Submarine surfacing.
            VOC_E_AH,               // Einstein "ah"
            VOC_E_OK,               // Einstein "ok"
            VOC_E_YES,              // Einstein "yes"
            VOC_TRIP_MINE,          // mine explosion sound
            VOC_SPY_COMMANDER,      // Spy: "commander?"
            VOC_SPY_YESSIR,         // Spy: "yes sir"
            VOC_SPY_INDEED,         // Spy: "indeed"
            VOC_SPY_ONWAY,          // Spy: "on my way"
            VOC_SPY_KING,           // Spy: "for king and country"
            VOC_MED_REPORTING,      // Medic: "reporting"
            VOC_MED_YESSIR,         // Medic: "yes sir"
            VOC_MED_AFFIRM,         // Medic: "affirmative"
            VOC_MED_MOVEOUT,        // Medic: "movin' out"
            VOC_BEEP_SELECT,        // map selection beep
            VOC_THIEF_YEA,          // Thief: "yea?"
            VOC_ANTDIE,
            VOC_ANTBITE,
            VOC_THIEF_MOVEOUT,      // Thief: "movin' out"
            VOC_THIEF_OKAY,         // Thief: "ok"
            VOC_x11,
            VOC_THIEF_WHAT,         // Thief: "what"
            VOC_THIEF_AFFIRM,       // Thief: "affirmative"
            VOC_STAVCMDR,
            VOC_STAVCRSE,
            VOC_STAVYES,
            VOC_STAVMOV,
            VOC_BUZZY1,
            VOC_RAMBO1,
            VOC_RAMBO2,
            VOC_RAMBO3,
            VOC_MECHYES1,
            VOC_MECHHOWDY1,
            VOC_MECHRISE1,
            VOC_MECHHUH1,
            VOC_MECHHEAR1,
            VOC_MECHLAFF1,
            VOC_MECHBOSS1,
            VOC_MECHYEEHAW1,
            VOC_MECHHOTDIG1,
            VOC_MECHWRENCH1,
            VOC_STBURN1,
            VOC_STCHRGE1,
            VOC_STCRISP1,
            VOC_STDANCE1,
            VOC_STJUICE1,
            VOC_STJUMP1,
            VOC_STLIGHT1,
            VOC_STPOWER1,
            VOC_STSHOCK1,
            VOC_STYES1,
            VOC_CHRONOTANK1,
            VOC_MECH_FIXIT1,
            VOC_MAD_CHARGE,
            VOC_MAD_EXPLODE,
            VOC_SHOCK_TROOP1,
        };

        public static readonly string[] VocNames = new []
        {
            "GIRLOKAY",
            "GIRLYEAH",
            "GUYOKAY1",
            "GUYYEAH1",
            "MINELAY1",
            "ACKNO",
            "AFFIRM1",
            "AWAIT1",
            "EAFFIRM1",
            "EENGIN1",
            "NOPROB",
            "READY",
            "REPORT1",
            "RITAWAY",
            "ROGER",
            "UGOTIT",
            "VEHIC1",
            "YESSIR1",
            "DEDMAN1",
            "DEDMAN2",
            "DEDMAN3",
            "DEDMAN4",
            "DEDMAN5",
            "DEDMAN6",
            "DEDMAN7",
            "DEDMAN8",
            "DEDMAN10",
            "CHRONO2",
            "CANNON1",
            "CANNON2",
            "IRONCUR9",
            "EMOVOUT1",
            "SONPULSE",
            "SANDBAG2",
            "MINEBLO1",
            "CHUTE1",
            "DOGY1",
            "DOGW5",
            "DOGG5P",
            "FIREBL3",
            "FIRETRT1",
            "GRENADE1",
            "GUN11",
            "GUN13",
            "EYESSIR1",
            "GUN27",
            "HEAL2",
            "HYDROD1",
            "INVUL2",
            "KABOOM1",
            "KABOOM12",
            "KABOOM15",
            "SPLASH9",
            "KABOOM22",
            "AACANON3",
            "TANDETH1",
            "MGUNINF1",
            "MISSILE1",
            "MISSILE6",
            "MISSILE7",
            "x",
            "PILLBOX1",
            "RABEEP1",
            "RAMENU1",
            "SILENCER",
            "TANK5",
            "TANK6",
            "TORPEDO1",
            "TURRET1",
            "TSLACHG2",
            "TESLA1",
            "SQUISHY2",
            "SCOLDY1",
            "RADARON2",
            "RADARDN1",
            "PLACBLDG",
            "KABOOM30",
            "KABOOM25",
            "x",
            "DOGW7",
            "DOGW3PX",
            "CRMBLE2",
            "CASHUP1",
            "CASHDN1",
            "BUILD5",
            "BLEEP9",
            "BLEEP6",
            "BLEEP5",
            "BLEEP17",
            "BLEEP13",
            "BLEEP12",
            "BLEEP11",
            "H2OBOMB2",
            "CASHTURN",
            "TUFFGUY1",
            "ROKROLL1",
            "LAUGH1",
            "CMON1",
            "BOMBIT1",
            "GOTIT1",
            "KEEPEM1",
            "ONIT1",
            "LEFTY1",
            "YEAH1",
            "YES1",
            "YO1",
            "WALLKIL2",
            "x",
            "GUN5",
            "SUBSHOW1",
            "EINAH1",
            "EINOK1",
            "EINYES1",
            "MINE1",
            "SCOMND1",
            "SYESSIR1",
            "SINDEED1",
            "SONWAY1",
            "SKING1",
            "MRESPON1",
            "MYESSIR1",
            "MAFFIRM1",
            "MMOVOUT1",
            "BEEPSLCT",
            "SYEAH1",
            "ANTDIE",
            "ANTBITE",
            "SMOUT1",
            "SOKAY1",
            "x",
            "SWHAT1",
            "SAFFIRM1",
            "STAVCMDR",
            "STAVCRSE",
            "STAVYES",
            "STAVMOV",
            "BUZZY1",
            "RAMBO1",
            "RAMBO2",
            "RAMBO3",
            "MYES1",
            "MHOWDY1",
            "MRISE1",
            "MHUH1",
            "MHEAR1",
            "MLAFF1",
            "MBOSS1",
            "MYEEHAW1",
            "MHOTDIG1",
            "MWRENCH1",
            "JBURN1",
            "JCHRGE1",
            "JCRISP1",
            "JDANCE1",
            "JJUICE1",
            "JJUMP1",
            "JLIGHT1",
            "JPOWER1",
            "JSHOCK1",
            "JYES1",
            "CHROTNK1",
            "FIXIT1",
            "MADCHRG2",
            "MADEXPLO",
            "SHKTROP1",
            "BEACON",
        };

        public static readonly string[] VocDesc = new[]
        {
            "\"Okay\" (female)",
            "\"Yeah?\" (female)",
            "\"Okay\" (male)",
            "\"Yeah?\" (male)",
            "Mine placed",
            "\"Acknowledged\"",
            "\"Affirmative\"",
            "\"Awaiting orders\"",
            "\"Affirmative\" (Engineer)",
            "\"Engineering\" (Engineer)",
            "\"Of course\"",
            "\"Ready and waiting\"",
            "\"Reporting\"",
            "\"At once\"",
            "\"Agreed\"",
            "\"Very well\"",
            "\"Vehicle reporting\"",
            "\"Yes sir?\"",
            "Man dies #1",
            "Man dies #2",
            "Man dies #3",
            "Man dies #4",
            "Man dies #5",
            "Man dies #6",
            "Man dies #7",
            "Man dies #8",
            "Man dies #9",
            "Chronosphere",
            "Mammoth Tank gun",
            "Light Tank gun",
            "Iron Curtain",
            "\"Movin' out\" (Engineer)",
            "Sonar pulse",
            "Sandbag crushed",
            "AT mine explodes",
            "Parachute",
            "Dog bark",
            "Dog whining",
            "Dog angry",
            "Fireball",
            "Fireball impact",
            "Grenade throw",
            "Rifle",
            "Pillbox machinegun",
            "\"Yes sir\" (Engineer)",
            "Pistol #1",
            "Healing",
            "Hissing",
            "Vworap",
            "Building half-destroyed",
            "tank shell impact",
            "Explosion",
            "Water impace",
            "big explosion",
            "AA gun",
            "Tanya screams",
            "Machinegun",
            "AA missile",
            "Cruiser missile",
            "MIG missile",
            "x",
            "Ranger machinegun",
            "High-pitched beep",
            "Menu click",
            "Silenced rifle",
            "Artillery fire",
            "Cruiser cannon",
            "Torpedo",
            "Turret shot",
            "Tesla charging",
            "Tesla firing",
            "Person crushed",
            "Blip",
            "Radar online",
            "Radar offline",
            "Building placed",
            "Explosion",
            "Artillery impact",
            "x",
            "Dog dies",
            "Dog response",
            "Building crumbles",
            "Cash coming in",
            "Cash going out",
            "Building up",
            "Radar powering up",
            "Information message",
            "Alarm",
            "Soft bleep",
            "soft low bleep",
            "high-pitched bleep down",
            "High-pitched bleep up",
            "Water explosion",
            "Selling sound",
            "\"Chew on this!\" (Tanya)",
            "\"Let's rock!\" (Tanya)",
            "Tanya laughing",
            "\"Shake it, baby!\" (Tanya)",
            "\"Cha-ching!\" (Tanya)",
            "\"That's all you got?\" (Tanya)",
            "\"Kiss is bye-bye!\" (Tanya)",
            "\"I'm there!\" (Tanya)",
            "\"Give it to me!\" (Tanya)",
            "\"Yeah?\" (Tanya)",
            "\"Yes, sir?\" (Tanya)",
            "\"What's up?\" (Tanya)",
            "Fence crushed",
            "x",
            "Pistol #2",
            "Submarine surfacing",
            "\"Ah?\" (Einstein)",
            "\"Incredible!\" (Einstein)",
            "\"Yes.\" (Einstein)",
            "AP mine explodes",
            "\"Commander?\" (Spy)",
            "\"Yes, sir?\" (Sky)",
            "\"Indeed\" (Spy)",
            "\"On my way\" (Spy)",
            "\"For king and country\" (Spy)",
            "\"Medic reporting\" (Medic)",
            "\"Yes, sir\" (Medic)",
            "\"Affirmative\" (Medic)",
            "\"Moving out\" (Medic)",
            "Select beep",
            "\"Yeah?\" (Thief)",
            "Ant dies",
            "Ant bites",
            "\"Moving out\" (Thief)",
            "\"Okay\" (Thief)",
            "x",
            "\"What?\" (Thief)",
            "\"Affirmative\" (Thief)",
            "\"Commander?\" (Stavros)",
            "\"Of course\" (Stavros)",
            "\"Yes\" (Stavros)",
            "\"Move out\" (Stavros)",
            "Warning siren",
            "\"I've got a present for ya!\" (Commando)",
            "Commando laugh",
            "\"Real tough guy!\" (Commando)",
            "\"Yes sir\" (Mechanic)",
            "\"Howdy?\" (Mechanic)",
            "\"Rise 'n' shine!\" (Mechanic)",
            "\"Huh?\" (Mechanic)",
            "\"I hear ya\" (Mechanic)",
            "Mechanic laugh",
            "\"Sure thing, boss\" (Mechanic)",
            "\"Yee-haw!\" (Mechanic)",
            "\"Hot diggity!\" (Mechanic)",
            "\"I'll get my wrench\" (Mechanic)",
            "\"Burn, baby, burn!\" (Shock Trooper)",
            "\"Fully charged!\" (Shock Trooper)",
            "\"Extra crispy!\" (Shock Trooper)",
            "\"Let's dance!\" (Shock Trooper)",
            "\"Got juice?\" (Shock Trooper)",
            "\"Need a jump?\" (Shock Trooper)",
            "\"Lights out\" (Shock Trooper)",
            "\"Power on!\" (Shock Trooper)",
            "\"Shocking!\" (Shock Trooper)",
            "\"Yes!\" (Shock Trooper)",
            "Chrono tank",
            "Wrench repair sound",
            "M.A.D. tank charging",
            "M.A.D. tank explosion",
            "Shock trooper tesla",
            "Beacon sound",
        };

        public enum VoxType
        {
            VOX_NONE = -1,
            VOX_ACCOMPLISHED,        // mission accomplished
            VOX_FAIL,                // your mission has failed
            VOX_NO_FACTORY,          // unable to comply, building in progress
            VOX_CONSTRUCTION,        // construction complete
            VOX_UNIT_READY,          // unit ready
            VOX_NEW_CONSTRUCT,       // new construction options
            VOX_DEPLOY,              // cannot deploy here
            VOX_STRUCTURE_DESTROYED, // structure destroyed
            VOX_INSUFFICIENT_POWER,  // insufficient power
            VOX_NO_CASH,             // insufficient funds
            VOX_CONTROL_EXIT,        // battle control terminated
            VOX_REINFORCEMENTS,      // reinforcements have arrived
            VOX_CANCELED,            // canceled
            VOX_BUILDING,            // building
            VOX_LOW_POWER,           // low power
            VOX_NEED_MO_MONEY,       // need more funds
            VOX_BASE_UNDER_ATTACK,   // our base is under attack
            VOX_UNABLE_TO_BUILD,     // unable to build more
            VOX_PRIMARY_SELECTED,    // primary building selected
            VOX_MADTANK_DEPLOYED,    // M.A.D. Tank Deployed
            VOX_none4,
            VOX_UNIT_LOST,           // unit lost
            VOX_SELECT_TARGET,       // select target
            VOX_PREPARE,             // enemy approaching
            VOX_NEED_MO_CAPACITY,    // silos needed
            VOX_SUSPENDED,           // on hold
            VOX_REPAIRING,           // repairing
            VOX_none5,
            VOX_none6,
            VOX_AIRCRAFT_LOST,
            VOX_none7,
            VOX_ALLIED_FORCES_APPROACHING,
            VOX_ALLIED_APPROACHING,
            VOX_none8,
            VOX_none9,
            VOX_BUILDING_INFILTRATED,
            VOX_CHRONO_CHARGING,
            VOX_CHRONO_READY,
            VOX_CHRONO_TEST,
            VOX_HQ_UNDER_ATTACK,
            VOX_CENTER_DEACTIVATED,
            VOX_CONVOY_APPROACHING,
            VOX_CONVOY_UNIT_LOST,
            VOX_EXPLOSIVE_PLACED,
            VOX_MONEY_STOLEN,
            VOX_SHIP_LOST,
            VOX_SATALITE_LAUNCHED,
            VOX_SONAR_AVAILABLE,
            VOX_none10,
            VOX_SOVIET_FORCES_APPROACHING,
            VOX_SOVIET_REINFORCEMENTS,
            VOX_TRAINING,
            VOX_ABOMB_READY,
            VOX_ABOMB_LAUNCH,
            VOX_ALLIES_N,
            VOX_ALLIES_S,
            VOX_ALLIES_E,
            VOX_ALLIES_W,
            VOX_OBJECTIVE1,
            VOX_OBJECTIVE2,
            VOX_OBJECTIVE3,
            VOX_IRON_CHARGING,
            VOX_IRON_READY,
            VOX_RESCUED,
            VOX_OBJECTIVE_NOT,
            VOX_SIGNAL_N,
            VOX_SIGNAL_S,
            VOX_SIGNAL_E,
            VOX_SIGNAL_W,
            VOX_SPY_PLANE,
            VOX_FREED,
            VOX_UPGRADE_ARMOR,
            VOX_UPGRADE_FIREPOWER,
            VOX_UPGRADE_SPEED,
            VOX_MISSION_TIMER,
            VOX_UNIT_FULL,
            VOX_UNIT_REPAIRED,
            VOX_TIME_40,
            VOX_TIME_30,
            VOX_TIME_20,
            VOX_TIME_10,
            VOX_TIME_5,
            VOX_TIME_4,
            VOX_TIME_3,
            VOX_TIME_2,
            VOX_TIME_1,
            VOX_TIME_STOP,
            VOX_UNIT_SOLD,
            VOX_TIMER_STARTED,
            VOX_TARGET_RESCUED,
            VOX_TARGET_FREED,
            VOX_TANYA_RESCUED,
            VOX_STRUCTURE_SOLD,
            VOX_SOVIET_FORCES_FALLEN,
            VOX_SOVIET_SELECTED,
            VOX_SOVIET_EMPIRE_FALLEN,
            VOX_OPERATION_TERMINATED,
            VOX_OBJECTIVE_REACHED,
            VOX_OBJECTIVE_NOT_REACHED,
            VOX_OBJECTIVE_MET,
            VOX_MERCENARY_RESCUED,
            VOX_MERCENARY_FREED,
            VOX_KOSOYGEN_FREED,
            VOX_FLARE_DETECTED,
            VOX_COMMANDO_RESCUED,
            VOX_COMMANDO_FREED,
            VOX_BUILDING_IN_PROGRESS,
            VOX_ATOM_PREPPING,
            VOX_ALLIED_SELECTED,
            VOX_ABOMB_PREPPING,
            VOX_ATOM_LAUNCHED,
            VOX_ALLIED_FORCES_FALLEN,
            VOX_ABOMB_AVAILABLE,
            VOX_ALLIED_REINFORCEMENTS,
            VOX_SAVE1,
            VOX_LOAD1,
        };

        public static readonly string[] VoxNames = new[]
        {
            "MISNWON1",  // VOX_ACCOMPLISHED              // mission accomplished
            "MISNLST1",  // VOX_FAIL                      // your mission has failed
            "PROGRES1",  // VOX_NO_FACTORY                // unable to comply, building in progress
            "CONSCMP1",  // VOX_CONSTRUCTION              // construction complete
            "UNITRDY1",  // VOX_UNIT_READY                // unit ready
            "NEWOPT1",   // VOX_NEW_CONSTRUCT             // new construction options
            "NODEPLY1",  // VOX_DEPLOY                    // cannot deploy here
            "STRCKIL1",  // VOX_STRUCTURE_DESTROYED       // structure destroyed
            "NOPOWR1",   // VOX_INSUFFICIENT_POWER        // insufficient power
            "NOFUNDS1",  // VOX_NO_CASH                   // insufficient funds
            "BCT1",      // VOX_CONTROL_EXIT              // battle control terminated
            "REINFOR1",  // VOX_REINFORCEMENTS            // reinforcements have arrived
            "CANCLD1",   // VOX_CANCELED                  // canceled
            "ABLDGIN1",  // VOX_BUILDING                  // building
            "LOPOWER1",  // VOX_LOW_POWER                 // low power
            "NOFUNDS1",  // VOX_NEED_MO_MONEY             // insufficent funds
            "BASEATK1",  // VOX_BASE_UNDER_ATTACK         // our base is under attack
            "NOBUILD1",  // VOX_UNABLE_TO_BUILD           // unable to build more
            "PRIBLDG1",  // VOX_PRIMARY_SELECTED          // primary building selected
            "TANK01",    // VOX_MADTANK_DEPLOYED          // M.A.D. Tank Deployed
            "none",      // VOX_SOVIET_CAPTURED           // Allied building captured
            "UNITLST1",  // VOX_UNIT_LOST                 // unit lost
            "SLCTTGT1",  // VOX_SELECT_TARGET             // select target
            "ENMYAPP1",  // VOX_PREPARE                   // enemy approaching
            "SILOND1",   // VOX_NEED_MO_CAPACITY          // silos needed
            "ONHOLD1",   // VOX_SUSPENDED                 // on hold
            "REPAIR1",   // VOX_REPAIRING                 // repairing
            "none",
            "none",
            "AUNITL1",   // VOX_AIRCRAFT_LOST             // airborne unit lost
            "none",
            "AAPPRO1",   // VOX_ALLIED_FORCES_APPROACHING // allied forces approaching
            "AARRIVE1",  // VOX_ALLIED_APPROACHING        // allied reinforcements have arrived
            "none",
            "none",
            "BLDGINF1",  // VOX_BUILDING_INFILTRATED      // building infiltrated
            "CHROCHR1",  // VOX_CHRONO_CHARGING           // chronosphere charging
            "CHRORDY1",  // VOX_CHRONO_READY              // chronosphere ready
            "CHROYES1",  // VOX_CHRONO_TEST               // chronosphere test successful
            "CMDCNTR1",  // VOX_HQ_UNDER_ATTACK           // command center under attack
            "CNTLDED1",  // VOX_CENTER_DEACTIVATED        // control center deactivated
            "CONVYAP1",  // VOX_CONVOY_APPROACHING        // convoy approaching
            "CONVLST1",  // VOX_CONVOY_UNIT_LOST          // convoy unit lost
            "XPLOPLC1",  // VOX_EXPLOSIVE_PLACED          // explosive charge placed
            "CREDIT1",   // VOX_MONEY_STOLEN              // credits stolen
            "NAVYLST1",  // VOX_SHIP_LOST                 // naval unit lost
            "SATLNCH1",  // VOX_SATALITE_LAUNCHED         // satalite launched
            "PULSE1",    // VOX_SONAR_AVAILABLE           // sonar pulse available
            "none",
            "SOVFAPP1",  // VOX_SOVIET_FORCES_APPROACHING // soviet forces approaching
            "SOVREIN1",  // VOX_SOVIET_REINFROCEMENTS     // soviet reinforcements have arrived
            "TRAIN1",    // VOX_TRAINING                  // training
            "AREADY1",   // VOX_ABOMB_READY
            "ALAUNCH1",  // VOX_ABOMB_LAUNCH
            "AARRIVN1",  // VOX_ALLIES_N
            "AARRIVS1",  // VOX_ALLIES_S
            "AARIVE1",   // VOX_ALLIES_E
            "AARRIVW1",  // VOX_ALLIES_W
            "1OBJMET1",  // VOX_OBJECTIVE1
            "2OBJMET1",  // VOX_OBJECTIVE2
            "3OBJMET1",  // VOX_OBJECTIVE3
            "IRONCHG1",  // VOX_IRON_CHARGING
            "IRONRDY1",  // VOX_IRON_READY
            "KOSYRES1",  // VOX_RESCUED
            "OBJNMET1",  // VOX_OBJECTIVE_NOT
            "FLAREN1",   // VOX_SIGNAL_N
            "FLARES1",   // VOX_SIGNAL_S
            "FLAREE1",   // VOX_SIGNAL_E
            "FLAREW1",   // VOX_SIGNAL_W
            "SPYPLN1",   // VOX_SPY_PLANE
            "TANYAF1",   // VOX_FREED
            "ARMORUP1",  // VOX_UPGRADE_ARMOR
            "FIREPO1",   // VOX_UPGRADE_FIREPOWER
            "UNITSPD1",  // VOX_UPGRADE_SPEED
            "MTIMEIN1",  // VOX_MISSION_TIMER
            "UNITFUL1",  // VOX_UNIT_FULL
            "UNITREP1",  // VOX_UNIT_REPAIRED
            "40MINR",    // VOX_TIME_40
            "30MINR",    // VOX_TIME_30
            "20MINR",    // VOX_TIME_20
            "10MINR",    // VOX_TIME_10
            "5MINR",     // VOX_TIME_5
            "4MINR",     // VOX_TIME_4
            "3MINR",     // VOX_TIME_3
            "2MINR",     // VOX_TIME_2
            "1MINR",     // VOX_TIME_1
            "TIMERNO1",  // VOX_TIME_STOP
            "UNITSLD1",  // VOX_UNIT_SOLD
            "TIMERGO1",  // VOX_TIMER_STARTED
            "TARGRES1",  // VOX_TARGET_RESCUED
            "TARGFRE1",  // VOX_TARGET_FREED
            "TANYAR1",   // VOX_TANYA_RESCUED
            "STRUSLD1",  // VOX_STRUCTURE_SOLD
            "SOVFORC1",  // VOX_SOVIET_FORCES_FALLEN
            "SOVEMP1",   // VOX_SOVIET_SELECTED
            "SOVEFAL1",  // VOX_SOVIET_EMPIRE_FALLEN
            "OPTERM1",   // VOX_OPERATION_TERMINATED
            "OBJRCH1",   // VOX_OBJECTIVE_REACHED
            "OBJNRCH1",  // VOX_OBJECTIVE_NOT_REACHED
            "OBJMET1",   // VOX_OBJECTIVE_MET
            "MERCR1",    // VOX_MERCENARY_RESCUED
            "MERCF1",    // VOX_MERCENARY_FREED
            "KOSYFRE1",  // VOX_KOSOYGEN_FREED
            "FLARE1",    // VOX_FLARE_DETECTED
            "COMNDOR1",  // VOX_COMMANDO_RESCUED
            "COMNDOF1",  // VOX_COMMANDO_FREED
            "BLDGPRG1",  // VOX_BUILDING_IN_PROGRESS
            "ATPREP1",   // VOX_ATOM_PREPPING
            "ASELECT1",  // VOX_ALLIED_SELECTED
            "APREP1",    // VOX_ABOMB_PREPPING
            "ATLNCH1",   // VOX_ATOM_LAUNCHED
            "AFALLEN1",  // VOX_ALLIED_FORCES_FALLEN
            "AAVAIL1",   // VOX_ABOMB_AVAILABLE
            "AARRIVE1",  // VOX_ALLIED_REINFORCEMENTS
            "SAVE1",     // VOX_MISSION_SAVED
            "LOAD1"      // VOX_MISSION_LOADED
        };

        public static readonly string[] VoxDesc = new[]
        {
            "Mission accomplished",
            "Your mission has failed",
            "Building in progress",
            "Construction complete",
            "Unit ready",
            "New construction options",
            "Cannot deploy here",
            "Structure destroyed",
            "Insufficient power",
            "Insufficient funds",
            "Battle control terminated",
            "Reinforcements arrived",
            "Canceled",
            "Building",
            "Low power",
            "Insufficent funds",
            "Our base is under attack",
            "Unable to build more",
            "Primary building selected",
            "M.A.D. Tank Deployed",
            "None",
            "Unit lost",
            "Select target",
            "Enemy approaching",
            "Silos needed",
            "On hold",
            "Repairing",
            "None",
            "None",
            "Airborne unit lost",
            "None",
            "Allied forces appr.",
            "Allied reinf. arrived",
            "None",
            "None",
            "Building infiltrated",
            "Chronosphere charging",
            "Chronosphere ready",
            "Chrono test successful",
            "Command cntr under attack",
            "Control center deactiv.",
            "Convoy approaching",
            "Convoy unit lost",
            "Explosive placed",
            "Credits stolen",
            "Naval unit lost",
            "Sattelite launched",
            "Sonar pulse available",
            "None",
            "Soviet forces approaching",
            "Soviet reinf. arrived",
            "Training",
            "A-bomb ready",
            "A-bomb launch detected",
            "Allied reinf. north",
            "Allied reinf. south",
            "Allied reinf. east",
            "Allied reinf. west",
            "1st objective met",
            "2nd objective met",
            "3rd objective met",
            "Iron Curtain charging",
            "Iron Curtain ready",
            "Kosygin rescued",
            "Objective not met",
            "Signal flare north",
            "Signal flare south",
            "Signal flare east",
            "Signal flare west",
            "Spy plane ready",
            "Tanya Freed",
            "Unit armor upgraded",
            "Unit firepower upgraded",
            "Unit speed upgraded",
            "Mission timer initialised",
            "Unit full",
            "Unit repaired",
            "40 minutes remaining",
            "30 minutes remaining",
            "20 minutes remaining",
            "10 minutes remaining",
            "5 minutes remaining",
            "4 minutes remaining",
            "3 minutes remaining",
            "2 minutes remaining",
            "1 minutes remaining",
            "Timer stopped",
            "Unit sold",
            "Timer started",
            "Target rescued",
            "Target freed",
            "Tanya rescued",
            "Structure sold",
            "Soviet forces have fallen",
            "Soviet Empire selected",
            "Soviet Empire has fallen",
            "Operation control terminated",
            "Objective reached",
            "Objective not reached",
            "Objective met",
            "Mercenary rescued",
            "Mercenary freed",
            "Kosygin freed",
            "Signal flare detected",
            "Commando rescued",
            "Commando freed",
            "Building in progress",
            "Atom bomb prepping",
            "Allied forces selected",
            "A-bomb prepping",
            "Atom bomb launch detected",
            "Allied forces have fallen",
            "A-bomb available",
            "Allied reinf. arrived",
            "Mission saved",
            "Mission loaded"
        };

        public static readonly string[] TextDesc = new[]
        {
            "Objective 1 Complete",
            "Objective 2 Complete",
            "Objective 3 Complete",
            "Defend Command Center",
            "Destroy all Allied units and structures",
            "Destroy all Soviet units and structures",
            "Build your base",
            "Find Einstein.",
            "Get Einstein to the helicopter",
            "Clear the way for the convoy",
            "Time is running out!",
            "Convoy approaching",
            "Destroy all bridges",
            "Get Spy into War Factory",
            "Destroy all SAM sites",
            "Get Tanya to the helicopter",
            "Capture Radar Dome",
            "Destroy Sub Pens",
            "Keep the Chronosphere on-line",
            "Restore full power",
            "Get a spy into Command Center",
            "Bring Kosygin back to your base",
            "INCOMING TRANSMISSION",
            "Capture the Command center!",
            "Get engineers to control computers",
            "Clear the naval channel",
            "Capture all Tech centers",
            "Destroy the Iron Curtain",
            "Use engineers to operate computers",
            "Re-program all generator computers",
            "Hangar turret powering up. Standby",
            "Turret deactivated",
            "Acquire money to build your base",
            "Destroy civilian forces and town",
            "Secure the middle island",
            "Get the convoy across the map",
            "Run for it!",
            "Capture the other tech centers",
            "Don’t approach the Chronosphere!",
            "Kill the enemy spy",
            "Disrupt Allied communications",
            "Get trucks to other shore",
            "Get engineers to coolant stations!",
            "Use main terminal to shut down core",
            "Meltdown Imminent!",
            "Destroy convoy truck",
            "Destroy Allied naval base",
            "Destroy Radar domes",
            "Capture the Chronosphere!",
            "Get spy into enemy tech center",
            "Einstein was killed",
            "Tanya was killed",
            "Radar Dome was destroyed",
            "Command Center destroyed",
            "Chronosphere self-destructed",
            "All Engineers killed",
            "Spy escaped",
            "Time ran out",
            "Convoy destroyed",
            "Spy killed",
            "Kosygin killed",
            "Einstein was in tech center",
            "Not enough available power",
            "Charge placed on Generator",
            "Find and Rescue captured Engineers",
            "Sarin facility destroyed",
            "Civilian town under attack",
            "Civilians evacuated",
            "Destroy power to Tesla Coils",
            "Evacuate the base!",
            "Escort Stavros to Allied base",
            "Get Stavros to evac point",
            "First convoy due in 20 minutes",
            "A convoy truck escaped",
            "All trucks destroyed!",
            "Stavros was killed",
            "Destroy all convoy trucks",
            "Get to other side of facility",
            "Capture Sarin facilities",
            "Evac civilians to island",
            "Civilians were killed",
            "Capture Allied helicopter",
            "Deactivate Tech Center",
            "Threaten Civilians",
            "Tech center was destroyed",
            "Convoy truck attempting to escape!",
            "Nest gassed",
            "All specialists killed",
            "Reinforcements arrive in 30 minutes",
            "Communications re-activated.",
            "Move to waypoint A. Shown by flare.",
            "Move to waypoint B. Shown by flare.",
            "Move to waypoint C. Shown by flare.",
            "Move to waypoint D. Shown by flare.",
            "Unauthorized units have entered the area!!!",
            "Protect Command Center at all costs!!!",
            "Exercise Complete! Proceed to...",
            "Alert!!! Alert!!!",
            "Objective Failed.  Stavros has escaped.",
            "Civilian Town is under Attack!",
            "Base Defense Compromised! Mission Aborted!",
            "Self Destruct Sequence Activated!",
            "All fake structures destroyed.",
            "Defend base until reinforcements arrive.",
            "Reinforcements arriving to the northeast",
            "Reinforcements arriving to the northwest",
            "Redirecting Badger Bombers.",
            "Soviet forces approaching.",
            "Destroy all technology centers.",
            "Prisoners freed.",
            "Reinforcements.",
            "Rescue the scientists.",
            "Keep the bridge intact.",
            "Get the scientists to safety.",
            "Use the LST to the north.",
            "RUN FOR IT!",
            "Clear the area of all opposition.",
            "STOP THEM!",
            "Locate & free the hostages.",
            "Get hostages to church",
            "Signal for reinforcements",
            "All hostages were killed.",
            "The church was destroyed.",
            "Reinforcements arrive in 10 minutes.",
            "Bring down the Allied communications.",
            "Destroy all remaining forces.",
            "Plans stolen... erasing all data.",
            "Power failure... backup power in 5 min.",
            "Backup power online.",
            "CRITICAL OVERLOAD!! MELTDOWN IN 45 MIN.",
            "Find & steal the vehicle plans.",
            "Infiltrate the research center.",
            "Eavesdrop on the Molotov Brothers.",
            "Brother! Come here! Guards report a",
            "plane passed by recently!",
            "Planes \"passing by\" don't concern me.",
            "Have you heard from our customers?",
            "They're asking for more information.",
            "They are getting too demanding!",
            "Hmm. You may be right. Let's discuss",
            "this somewhere a bit more private. Come.",
            "So Yuri, what did they want now? More",
            "information on troop movements?",
            "Yes. They're getting nervous about the",
            "troop massings on the eastern borders.",
            "You tell that General Stavros tha--",
            "Hey! Who's that?! He's not one of ours!",
            "Signal for reinforcements!",
            "Destroy the Molotov Brothers' base.",
            "Poison Allied water supply.",
            "Capture Allied Chronosphere.",
            "Volkov Contacted! His location is in a ",
            "Fake Factory in the East! Look for the ",
            "Flares & infiltrate it with scientists! ",
            "Stop him! Release the dogs!",
            "Daniel, you idiot! You killed the dogs!",
            "Retrieval Failed! Volkov has been re-",
            "programmed. Destroy him and all Allies!",
            "Red Alert! Civilian heavy weaponry ",
            "detected. Head Northeast immediately",
            "for reinforcements. Repeat! Head north-",
            "east immediately for reinforcements!",
            "Civilian Base located in Northwest",
            "corner of this region. Allied and",
            "Soviet structures detected. Destroy",
            "the base and all enemy units!",
            "This war is wrong! Down with Stalin!",
            "We wish freedom from the Regime!",
            "We stand on our own.  Leave us be!",
            "Let us send out our women and children!",
            "Here they come. Please do not attack ",
            "them. They are only women and children!",
            "We are Stalin's personal guard.  We are",
            "here to insure that \"ALL\" of the enemy",
            "is destroyed. Do Not Get In Our Way!",
            "Help us Please! They are killing kids.",
            "Please, we cannot stop them.........",
            "Locate & evacuate with the transport.",
            "You've been detected.",
            "Prisoners executed.",
            "Hurry and leave!",
            "Data recieved & Triangulation complete.",
            "Location of control center is in the ",
            "southeast. Look for the flares & ",
            "destroy the center!",
            "Mad Tank detonation imminent!",
            "DESTROY THE TOWN! KILL EVERYTHING! ",
            "Destroy Research Center",
            "Objective Destroyed! Abort Mission Now!",
            "Intruder Alert! Release the dogs!",
            "Volkov located & reprogramming aborted,",
            "but he is now unstable and is attacking ",
            "anyone & everyone. Eliminate him! ",
            "Clear the way!",
            "Destroy Sub Pen.",
            "Don't let the Missile Subs escape!",
            "Infiltrate Bio-Research facility.",
            "Thank you! I'll help you get into town!",
            "Uh oh, a patrol is coming this way.",
            "Come this way! Hurry!",
            "It's safe to move now. Let's go.",
            "Follow me!",
            "Powering up vehicle.",
            "Find and repair Allied outpost.",
            "Find and evacuate Dr. Demetri.",
            "Infiltrate the Radar Dome.",
            "Destroy the Radar Domes that control",
            "the SAM Sites.",
            "Destroy the two missile silos.",
        };

        public static readonly string[] SuperTypes = new[]
        {
            "Sonar Pulse",   // Momentarily reveals submarines.
            "Nuclear Bomb",  // Tactical nuclear weapon.
            "Chronosphere",  // Paradox device, for teleportation
            "Parabombs",     // Parachute bomb delivery.
            "Paratroopers",  // Parachute reinforcement delivery.
            "Spy Plane",     // Spy plane to take photo recon mission.
            "Iron Curtain",  // Bestow invulnerability on a unit/building
            "GPS",           // give allies free unjammable radar.
        };

        public enum SpecialWeaponType
        {
            SPC_NONE = -1,
            SPC_SONAR_PULSE,   // Momentarily reveals submarines.
            SPC_NUCLEAR_BOMB,  // Tactical nuclear weapon.
            SPC_CHRONOSPHERE,  // Paradox device, for teleportation
            SPC_PARA_BOMB,     // Parachute bomb delivery.
            SPC_PARA_INFANTRY, // Parachute reinforcement delivery.
            SPC_SPY_MISSION,   // Spy plane to take photo recon mission.
            SPC_IRON_CURTAIN,  // Bestow invulnerability on a unit/building
            SPC_GPS,           // give allies free unjammable radar.
        };

        public enum QuarryType
        {
            QUARRY_NONE,
            QUARRY_ANYTHING,      // Attack any enemy (same as "hunt").
            QUARRY_BUILDINGS,     // Attack buildings (in general).
            QUARRY_HARVESTERS,    // Attack harvesters or refineries.
            QUARRY_INFANTRY,      // Attack infantry.
            QUARRY_VEHICLES,      // Attack combat vehicles.
            QUARRY_VESSELS,       // Attach ships.
            QUARRY_FACTORIES,     // Attack factories (all types).
            QUARRY_DEFENSE,       // Attack base defense buildings.
            QUARRY_THREAT,        // Attack enemies near friendly base.
            QUARRY_POWER,         // Attack power facilities.
            QUARRY_FAKES,         // Prefer to attack fake buildings.
        };

        public enum VQType
        {
            VQ_NONE = -1,
            VQ_AAGUN,
            VQ_MIG,
            VQ_SFROZEN,
            VQ_AIRFIELD,
            VQ_BATTLE,
            VQ_BMAP,
            VQ_BOMBRUN,
            VQ_DPTHCHRG,
            VQ_GRVESTNE,
            VQ_MONTPASS,
            VQ_MTNKFACT,
            VQ_CRONTEST,
            VQ_OILDRUM,
            VQ_ALLYEND,
            VQ_RADRRAID,
            VQ_SHIPYARD,
            VQ_SHORBOMB,
            VQ_SITDUCK,
            VQ_SLNTSRVC,
            VQ_SNOWBASE,
            VQ_EXECUTE,
            VQ_TITLE,               // Low res.
            VQ_NUKESTOK,
            VQ_V2ROCKET,
            VQ_SEARCH,
            VQ_BINOC,
            VQ_ELEVATOR,
            VQ_FROZEN,
            VQ_MCV,
            VQ_SHIPSINK,
            VQ_SOVMCV,
            VQ_TRINITY,
            VQ_ALLYMORF,
            VQ_APCESCPE,
            VQ_BRDGTILT,
            VQ_CRONFAIL,
            VQ_STRAFE,
            VQ_DESTROYR,
            VQ_DOUBLE,
            VQ_FLARE,
            VQ_SNSTRAFE,
            VQ_LANDING,
            VQ_ONTHPRWL,
            VQ_OVERRUN,
            VQ_SNOWBOMB,
            VQ_SOVCEMET,
            VQ_TAKE_OFF,
            VQ_TESLA,
            VQ_SOVIET8,
            VQ_SPOTTER,
            VQ_SCENE1,
            VQ_SCENE2,
            VQ_SCENE4,
            VQ_SOVFINAL,
            VQ_ASSESS,
            VQ_SOVIET10,
            VQ_DUD,
            VQ_MCV_LAND,
            VQ_MCVBRDGE,
            VQ_PERISCOP,
            VQ_SHORBOM1,
            VQ_SHORBOM2,
            VQ_SOVBATL,
            VQ_SOVTSTAR,
            VQ_AFTRMATH,
            VQ_SOVIET11,
            VQ_MASASSLT,
            VQ_REDINTRO,        // High res
            VQ_SOVIET1,
            VQ_SOVIET2,
            VQ_SOVIET3,
            VQ_SOVIET4,
            VQ_SOVIET5,
            VQ_SOVIET6,
            VQ_SOVIET7,
            VQ_INTRO_MOVIE,
            VQ_AVERTED,
            VQ_COUNTDWN,
            VQ_MOVINGIN,
            VQ_ALLIED10,
            VQ_ALLIED12,
            VQ_ALLIED5,
            VQ_ALLIED6,
            VQ_ALLIED8,
            VQ_TANYA1,
            VQ_TANYA2,
            VQ_ALLY10B,
            VQ_ALLY11,
            VQ_ALLY14,
            VQ_ALLY9,
            VQ_SPY,
            VQ_TOOFAR,
            VQ_SOVIET12,
            VQ_SOVIET13,
            VQ_SOVIET9,
            VQ_BEACHEAD,
            VQ_SOVIET14,
            VQ_SIZZLE,
            VQ_SIZZLE2,
            VQ_ANTEND,
            VQ_ANTINTRO,
            VQ_RETALIATION_ALLIED1,
            VQ_RETALIATION_ALLIED2,
            VQ_RETALIATION_ALLIED3,
            VQ_RETALIATION_ALLIED4,
            VQ_RETALIATION_ALLIED5,
            VQ_RETALIATION_ALLIED6,
            VQ_RETALIATION_ALLIED7,
            VQ_RETALIATION_ALLIED8,
            VQ_RETALIATION_ALLIED9,
            VQ_RETALIATION_ALLIED10,
            VQ_RETALIATION_SOVIET1,
            VQ_RETALIATION_SOVIET2,
            VQ_RETALIATION_SOVIET3,
            VQ_RETALIATION_SOVIET4,
            VQ_RETALIATION_SOVIET5,
            VQ_RETALIATION_SOVIET6,
            VQ_RETALIATION_SOVIET7,
            VQ_RETALIATION_SOVIET8,
            VQ_RETALIATION_SOVIET9,
            VQ_RETALIATION_SOVIET10,
            VQ_RETALIATION_WINA,
            VQ_RETALIATION_WINS,
            VQ_RETALIATION_ANTS,
        };
    }
}
