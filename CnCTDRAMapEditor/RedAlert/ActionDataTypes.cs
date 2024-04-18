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
namespace MobiusEditor.RedAlert
{
    public static class ActionDataTypes
    {

        /// <summary>
        /// Not used; retained here for reference.
        /// </summary>
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

        /// <summary>
        /// Not used; retained here for reference.
        /// </summary>
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
            "[RA] Objective 1 Complete",
            "[RA] Objective 2 Complete",
            "[RA] Objective 3 Complete",
            "[RA] Defend Command Center",
            "[RA] Destroy all Allied units and structures",
            "[RA] Destroy all Soviet units and structures",
            "[RA] Build your base",
            "[RA] Find Einstein.",
            "[RA] Get Einstein to the helicopter",
            "[RA] Clear the way for the convoy",
            "[RA] Time is running out!",
            "[RA] Convoy approaching",
            "[RA] Destroy all bridges",
            "[RA] Get Spy into War Factory",
            "[RA] Destroy all SAM sites",
            "[RA] Get Tanya to the helicopter",
            "[RA] Capture Radar Dome",
            "[RA] Destroy Sub Pens",
            "[RA] Keep the Chronosphere on-line",
            "[RA] Restore full power",
            "[RA] Get a spy into Command Center",
            "[RA] Bring Kosygin back to your base",
            "[RA] INCOMING TRANSMISSION",
            "[RA] Capture the Command center!",
            "[RA] Get engineers to control computers",
            "[RA] Clear the naval channel",
            "[RA] Capture all Tech centers",
            "[RA] Destroy the Iron Curtain",
            "[RA] Use engineers to operate computers",
            "[RA] Re-program all generator computers",
            "[RA] Hangar turret powering up. Standby",
            "[RA] Turret deactivated",
            "[RA] Acquire money to build your base",
            "[RA] Destroy civilian forces and town",
            "[RA] Secure the middle island",
            "[RA] Get the convoy across the map",
            "[RA] Run for it!",
            "[RA] Capture the other tech centers",
            "[RA] Don’t approach the Chronosphere!",
            "[RA] Kill the enemy spy",
            "[RA] Disrupt Allied communications",
            "[RA] Get trucks to other shore",
            "[RA] Get engineers to coolant stations!",
            "[RA] Use main terminal to shut down core",
            "[RA] Meltdown Imminent!",
            "[RA] Destroy convoy truck",
            "[RA] Destroy Allied naval base",
            "[RA] Destroy Radar domes",
            "[RA] Capture the Chronosphere!",
            "[RA] Get spy into enemy tech center",
            "[RA] Einstein was killed",
            "[RA] Tanya was killed",
            "[RA] Radar Dome was destroyed",
            "[RA] Command Center destroyed",
            "[RA] Chronosphere self-destructed",
            "[RA] All Engineers killed",
            "[RA] Spy escaped",
            "[RA] Time ran out",
            "[RA] Convoy destroyed",
            "[RA] Spy killed",
            "[RA] Kosygin killed",
            "[RA] Einstein was in tech center",
            "[RA] Not enough available power",
            "[RA] Charge placed on Generator",
            "[RA] Find and Rescue captured Engineers",
            "[CS] Sarin facility destroyed",
            "[CS] Civilian town under attack",
            "[CS] Civilians evacuated",
            "[CS] Destroy power to Tesla Coils",
            "[CS] Evacuate the base!",
            "[CS] Escort Stavros to Allied base",
            "[CS] Get Stavros to evac point",
            "[CS] First convoy due in 20 minutes",
            "[CS] A convoy truck escaped",
            "[CS] All trucks destroyed!",
            "[CS] Stavros was killed",
            "[CS] Destroy all convoy trucks",
            "[CS] Get to other side of facility",
            "[CS] Capture Sarin facilities",
            "[CS] Evac civilians to island",
            "[CS] Civilians were killed",
            "[CS] Capture Allied helicopter",
            "[CS] Deactivate Tech Center",
            "[CS] Threaten Civilians",
            "[CS] Tech center was destroyed",
            "[CS] Convoy truck attempting to escape!",
            "[CS] Nest gassed",
            "[CS] All specialists killed",
            "[CS] Reinforcements arrive in 30 minutes",
            "[CS] Communications re-activated.",
            "[CS] Move to waypoint A. Shown by flare.",
            "[CS] Move to waypoint B. Shown by flare.",
            "[CS] Move to waypoint C. Shown by flare.",
            "[CS] Move to waypoint D. Shown by flare.",
            "[CS] Unauthorized units have entered the area!!!",
            "[CS] Protect Command Center at all costs!!!",
            "[CS] Exercise Complete! Proceed to...",
            "[CS] Alert!!! Alert!!!",
            "[CS] Objective Failed.  Stavros has escaped.",
            "[CS] Civilian Town is under Attack!",
            "[CS] Base Defense Compromised! Mission Aborted!",
            "[CS] Self Destruct Sequence Activated!",
            "[AM] All fake structures destroyed.",
            "[AM] Defend base until reinforcements arrive.",
            "[AM] Reinforcements arriving to the northeast",
            "[AM] Reinforcements arriving to the northwest",
            "[AM] Redirecting Badger Bombers.",
            "[AM] Soviet forces approaching.",
            "[AM] Destroy all technology centers.",
            "[AM] Prisoners freed.",
            "[AM] Reinforcements.",
            "[AM] Rescue the scientists.",
            "[AM] Keep the bridge intact.",
            "[AM] Get the scientists to safety.",
            "[AM] Use the LST to the north.",
            "[AM] RUN FOR IT!",
            "[AM] Clear the area of all opposition.",
            "[AM] STOP THEM!",
            "[AM] Locate & free the hostages.",
            "[AM] Get hostages to church",
            "[AM] Signal for reinforcements",
            "[AM] All hostages were killed.",
            "[AM] The church was destroyed.",
            "[AM] Reinforcements arrive in 10 minutes.",
            "[AM] Bring down the Allied communications.",
            "[AM] Destroy all remaining forces.",
            "[AM] Plans stolen... erasing all data.",
            "[AM] Power failure... backup power in 5 min.",
            "[AM] Backup power online.",
            "[AM] CRITICAL OVERLOAD!! MELTDOWN IN 45 MIN.",
            "[AM] Find & steal the vehicle plans.",
            "[AM] Infiltrate the research center.",
            "[AM] Eavesdrop on the Molotov Brothers.",
            "[AM] Brother! Come here! Guards report a",
            "[AM] plane passed by recently!",
            "[AM] Planes \"passing by\" don't concern me.",
            "[AM] Have you heard from our customers?",
            "[AM] They're asking for more information.",
            "[AM] They are getting too demanding!",
            "[AM] Hmm. You may be right. Let's discuss",
            "[AM] this somewhere a bit more private. Come.",
            "[AM] So Yuri, what did they want now? More",
            "[AM] information on troop movements?",
            "[AM] Yes. They're getting nervous about the",
            "[AM] troop massings on the eastern borders.",
            "[AM] You tell that General Stavros tha--",
            "[AM] Hey! Who's that?! He's not one of ours!",
            "[AM] Signal for reinforcements!",
            "[AM] Destroy the Molotov Brothers' base.",
            "[AM] Poison Allied water supply.",
            "[AM] Capture Allied Chronosphere.",
            "[AM] Volkov Contacted! His location is in a ",
            "[AM] Fake Factory in the East! Look for the ",
            "[AM] Flares & infiltrate it with scientists! ",
            "[AM] Stop him! Release the dogs!",
            "[AM] Daniel, you idiot! You killed the dogs!",
            "[AM] Retrieval Failed! Volkov has been re-",
            "[AM] programmed. Destroy him and all Allies!",
            "[AM] Red Alert! Civilian heavy weaponry ",
            "[AM] detected. Head Northeast immediately",
            "[AM] for reinforcements. Repeat! Head north-",
            "[AM] east immediately for reinforcements!",
            "[AM] Civilian Base located in Northwest",
            "[AM] corner of this region. Allied and",
            "[AM] Soviet structures detected. Destroy",
            "[AM] the base and all enemy units!",
            "[AM] This war is wrong! Down with Stalin!",
            "[AM] We wish freedom from the Regime!",
            "[AM] We stand on our own.  Leave us be!",
            "[AM] Let us send out our women and children!",
            "[AM] Here they come. Please do not attack ",
            "[AM] them. They are only women and children!",
            "[AM] We are Stalin's personal guard.  We are",
            "[AM] here to insure that \"ALL\" of the enemy",
            "[AM] is destroyed. Do Not Get In Our Way!",
            "[AM] Help us Please! They are killing kids.",
            "[AM] Please, we cannot stop them.........",
            "[AM] Locate & evacuate with the transport.",
            "[AM] You've been detected.",
            "[AM] Prisoners executed.",
            "[AM] Hurry and leave!",
            "[AM] Data recieved & Triangulation complete.",
            "[AM] Location of control center is in the ",
            "[AM] southeast. Look for the flares & ",
            "[AM] destroy the center!",
            "[AM] Mad Tank detonation imminent!",
            "[AM] DESTROY THE TOWN! KILL EVERYTHING! ",
            "[AM] Destroy Research Center",
            "[AM] Objective Destroyed! Abort Mission Now!",
            "[AM] Intruder Alert! Release the dogs!",
            "[AM] Volkov located & reprogramming aborted,",
            "[AM] but he is now unstable and is attacking ",
            "[AM] anyone & everyone. Eliminate him! ",
            "[AM] Clear the way!",
            "[AM] Destroy Sub Pen.",
            "[AM] Don't let the Missile Subs escape!",
            "[AM] Infiltrate Bio-Research facility.",
            "[AM] Thank you! I'll help you get into town!",
            "[AM] Uh oh, a patrol is coming this way.",
            "[AM] Come this way! Hurry!",
            "[AM] It's safe to move now. Let's go.",
            "[AM] Follow me!",
            "[AM] Powering up vehicle.",
            "[AM] Find and repair Allied outpost.",
            "[AM] Find and evacuate Dr. Demetri.",
            "[AM] Infiltrate the Radar Dome.",
            "[AM] Destroy the Radar Domes that control",
            "[AM] the SAM Sites.",
            "[AM] Destroy the two missile silos.",
        };

        /// <summary>
        /// Not used; retained here for reference.
        /// </summary>
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

        /// <summary>
        /// Not used; retained here for reference.
        /// This information is used in the TeamMissionTypes.Attack object.
        /// </summary>
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

        public static readonly string[] UnitVocNames = new[]
        {
            "ACKNO",
            "AFFIRM1",
            "AWAIT1",
            "EAFFIRM1",
            "EENGIN1",
            "NOPROB",
            "OVEROUT",
            "READY",
            "REPORT1",
            "RITAWAY",
            "ROGER",
            "UGOTIT",
            "VEHIC1",
            "YESSIR1",
        };
    }
}
