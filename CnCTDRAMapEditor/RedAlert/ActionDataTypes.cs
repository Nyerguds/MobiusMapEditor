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

        public static readonly string[] VocNames = new []
        {
            "GIRLOKAY", // VOC_GIRL_OKAY           // "Okay" (female)
            "GIRLYEAH", // VOC_GIRL_YEAH           // "Yeah?" (female)
            "GUYOKAY1", // VOC_GUY_OKAY            // "Okay" (male)
            "GUYYEAH1", // VOC_GUY_YEAH            // "Yeah?" (male)
            "MINELAY1", // VOC_MINELAY1            // Mine placed
            "ACKNO",    // VOC_ACKNOWL             // "Acknowledged"
            "AFFIRM1",  // VOC_AFFIRM              // "Affirmative"
            "AWAIT1",   // VOC_AWAIT               // "Awaiting orders"
            "EAFFIRM1", // VOC_ENG_AFFIRM          // "Affirmative" (Engineer)
            "EENGIN1",  // VOC_ENG_ENG             // "Engineering" (Engineer)
            "NOPROB",   // VOC_NO_PROB             // "Of course"
            "READY",    // VOC_READY               // "Ready and waiting"
            "REPORT1",  // VOC_REPORT              // "Reporting"
            "RITAWAY",  // VOC_RIGHT_AWAY          // "At once"
            "ROGER",    // VOC_ROGER               // "Agreed"
            "UGOTIT",   // VOC_UGOTIT              // "Very well"
            "VEHIC1",   // VOC_VEHIC               // "Vehicle reporting"
            "YESSIR1",  // VOC_YESSIR              // "Yes sir?"
            "DEDMAN1",  // VOC_SCREAM1             // Man dies #1
            "DEDMAN2",  // VOC_SCREAM3             // Man dies #2
            "DEDMAN3",  // VOC_SCREAM4             // Man dies #3
            "DEDMAN4",  // VOC_SCREAM5             // Man dies #4
            "DEDMAN5",  // VOC_SCREAM6             // Man dies #5
            "DEDMAN6",  // VOC_SCREAM7             // Man dies #6
            "DEDMAN7",  // VOC_SCREAM10            // Man dies #7
            "DEDMAN8",  // VOC_SCREAM11            // Man dies #8
            "DEDMAN10", // VOC_YELL1               // Man dies #9
            "CHRONO2",  // VOC_CHRONO              // Chronosphere
            "CANNON1",  // VOC_CANNON1             // Mammoth Tank gun
            "CANNON2",  // VOC_CANNON2             // Light Tank gun
            "IRONCUR9", // VOC_IRON1               // Iron Curtain
            "EMOVOUT1", // VOC_ENG_MOVEOUT         // "Movin' out" (Engineer)
            "SONPULSE", // VOC_SONAR               // Sonar pulse
            "SANDBAG2", // VOC_SANDBAG             // Sandbag crushed
            "MINEBLO1", // VOC_MINEBLOW            // AT mine explodes
            "CHUTE1",   // VOC_CHUTE1              // Parachute
            "DOGY1",    // VOC_DOG_BARK            // Dog bark
            "DOGW5",    // VOC_DOG_WHINE           // Dog whining
            "DOGG5P",   // VOC_DOG_GROWL2          // Dog angry
            "FIREBL3",  // VOC_FIRE_LAUNCH         // Fireball
            "FIRETRT1", // VOC_FIRE_EXPLODE        // Fireball impact
            "GRENADE1", // VOC_GRENADE_TOSS        // Grenade throw
            "GUN11",    // VOC_GUN_5               // Rifle
            "GUN13",    // VOC_GUN_7               // Pillbox machinegun
            "EYESSIR1", // VOC_ENG_YES             // "Yes sir" (Engineer)
            "GUN27",    // VOC_GUN_RIFLE           // Pistol #1
            "HEAL2",    // VOC_HEAL                // Healing
            "HYDROD1",  // VOC_DOOR                // Hissing
            "INVUL2",   // VOC_INVULNERABLE        // Vworap
            "KABOOM1",  // VOC_KABOOM1             // Building half-destroyed
            "KABOOM12", // VOC_KABOOM12            // tank shell impact
            "KABOOM15", // VOC_KABOOM15            // Explosion
            "SPLASH9",  // VOC_SPLASH              // Water impact
            "KABOOM22", // VOC_KABOOM22            // big explosion
            "AACANON3", // VOC_AACANON3            // AA gun
            "TANDETH1", // VOC_TANYA_DIE           // Tanya screams
            "MGUNINF1", // VOC_GUN_5F              // Machinegun
            "MISSILE1", // VOC_MISSILE_1           // AA missile
            "MISSILE6", // VOC_MISSILE_2           // Cruiser missile
            "MISSILE7", // VOC_MISSILE_3           // MIG missile
            "x",        // VOC_x6                  // x
            "PILLBOX1", // VOC_GUN_5R              // Ranger machinegun
            "RABEEP1",  // VOC_BEEP                // High-pitched beep
            "RAMENU1",  // VOC_CLICK               // Menu click
            "SILENCER", // VOC_SILENCER            // Silenced rifle
            "TANK5",    // VOC_CANNON6             // Artillery fire
            "TANK6",    // VOC_CANNON7             // Cruiser cannon
            "TORPEDO1", // VOC_TORPEDO             // Torpedo
            "TURRET1",  // VOC_CANNON8             // Turret shot
            "TSLACHG2", // VOC_TESLA_POWER_UP      // Tesla charging
            "TESLA1",   // VOC_TESLA_ZAP           // Tesla firing
            "SQUISHY2", // VOC_SQUISH              // Person crushed
            "SCOLDY1",  // VOC_SCOLD               // Blip
            "RADARON2", // VOC_RADAR_ON            // Radar online
            "RADARDN1", // VOC_RADAR_OFF           // Radar offline
            "PLACBLDG", // VOC_PLACE_BUILDING_DOWN // Building placed
            "KABOOM30", // VOC_KABOOM30            // Explosion
            "KABOOM25", // VOC_KABOOM25            // Artillery impact
            "x",        // VOC_x7                  // x
            "DOGW7",    // VOC_DOG_HURT            // Dog dies
            "DOGW3PX",  // VOC_DOG_YES             // Dog response
            "CRMBLE2",  // VOC_CRUMBLE             // Building crumbles
            "CASHUP1",  // VOC_MONEY_UP            // Cash coming in
            "CASHDN1",  // VOC_MONEY_DOWN          // Cash going out
            "BUILD5",   // VOC_CONSTRUCTION        // Building up
            "BLEEP9",   // VOC_GAME_CLOSED         // Radar powering up
            "BLEEP6",   // VOC_INCOMING_MESSAGE    // Information message
            "BLEEP5",   // VOC_SYS_ERROR           // Alarm
            "BLEEP17",  // VOC_OPTIONS_CHANGED     // Soft bleep
            "BLEEP13",  // VOC_GAME_FORMING        // soft low bleep
            "BLEEP12",  // VOC_PLAYER_LEFT         // high-pitched bleep down
            "BLEEP11",  // VOC_PLAYER_JOINED       // High-pitched bleep up
            "H2OBOMB2", // VOC_DEPTH_CHARGE        // Water explosion
            "CASHTURN", // VOC_CASHTURN            // Selling sound
            "TUFFGUY1", // VOC_TANYA_CHEW          // "Chew on this!" (Tanya)
            "ROKROLL1", // VOC_TANYA_ROCK          // "Let's rock!" (Tanya)
            "LAUGH1",   // VOC_TANYA_LAUGH         // Tanya laughing
            "CMON1",    // VOC_TANYA_SHAKE         // "Shake it, baby!" (Tanya)
            "BOMBIT1",  // VOC_TANYA_CHING         // "Cha-ching!" (Tanya)
            "GOTIT1",   // VOC_TANYA_GOT           // "That's all you got?" (Tanya)
            "KEEPEM1",  // VOC_TANYA_KISS          // "Kiss is bye-bye!" (Tanya)
            "ONIT1",    // VOC_TANYA_THERE         // "I'm there!" (Tanya)
            "LEFTY1",   // VOC_TANYA_GIVE          // "Give it to me!" (Tanya)
            "YEAH1",    // VOC_TANYA_YEA           // "Yeah?" (Tanya)
            "YES1",     // VOC_TANYA_YES           // "Yes, sir?" (Tanya)
            "YO1",      // VOC_TANYA_WHATS         // "What's up?" (Tanya)
            "WALLKIL2", // VOC_WALLKILL2           // Fence crushed
            "x",        // VOC_x8                  // x
            "GUN5",     // VOC_TRIPLE_SHOT         // Pistol #2
            "SUBSHOW1", // VOC_SUBSHOW             // Submarine surfacing
            "EINAH1",   // VOC_E_AH                // "Ah?" (Einstein)
            "EINOK1",   // VOC_E_OK                // "Incredible!" (Einstein)
            "EINYES1",  // VOC_E_YES               // "Yes." (Einstein)
            "MINE1",    // VOC_TRIP_MINE           // AP mine explodes
            "SCOMND1",  // VOC_SPY_COMMANDER       // "Commander?" (Spy)
            "SYESSIR1", // VOC_SPY_YESSIR          // "Yes, sir?" (Sky)
            "SINDEED1", // VOC_SPY_INDEED          // "Indeed" (Spy)
            "SONWAY1",  // VOC_SPY_ONWAY           // "On my way" (Spy)
            "SKING1",   // VOC_SPY_KING            // "For king and country" (Spy)
            "MRESPON1", // VOC_MED_REPORTING       // "Medic reporting" (Medic)
            "MYESSIR1", // VOC_MED_YESSIR          // "Yes, sir" (Medic)
            "MAFFIRM1", // VOC_MED_AFFIRM          // "Affirmative" (Medic)
            "MMOVOUT1", // VOC_MED_MOVEOUT         // "Moving out" (Medic)
            "BEEPSLCT", // VOC_BEEP_SELECT         // Select beep
            "SYEAH1",   // VOC_THIEF_YEA           // "Yeah?" (Thief)
            "ANTDIE",   // VOC_ANTDIE              // Ant dies
            "ANTBITE",  // VOC_ANTBITE             // Ant bites
            "SMOUT1",   // VOC_THIEF_MOVEOUT       // "Moving out" (Thief)
            "SOKAY1",   // VOC_THIEF_OKAY          // "Okay" (Thief)
            "x",        // VOC_x11                 // x
            "SWHAT1",   // VOC_THIEF_WHAT          // "What?" (Thief)
            "SAFFIRM1", // VOC_THIEF_AFFIRM        // "Affirmative" (Thief)
            "STAVCMDR", // VOC_STAVCMDR            // "Commander?" (Stavros)
            "STAVCRSE", // VOC_STAVCRSE            // "Of course" (Stavros)
            "STAVYES",  // VOC_STAVYES             // "Yes" (Stavros)
            "STAVMOV",  // VOC_STAVMOV             // "Move out" (Stavros)
            "BUZZY1",   // VOC_BUZZY1              // Warning siren
            "RAMBO1",   // VOC_RAMBO1              // "I've got a present for ya!" (Commando)
            "RAMBO2",   // VOC_RAMBO2              // Commando laugh
            "RAMBO3",   // VOC_RAMBO3              // "Real tough guy!" (Commando)
            "MYES1",    // VOC_MECHYES1            // "Yes sir" (Mechanic)
            "MHOWDY1",  // VOC_MECHHOWDY1          // "Howdy?" (Mechanic)
            "MRISE1",   // VOC_MECHRISE1           // "Rise 'n' shine!" (Mechanic)
            "MHUH1",    // VOC_MECHHUH1            // "Huh?" (Mechanic)
            "MHEAR1",   // VOC_MECHHEAR1           // "I hear ya" (Mechanic)
            "MLAFF1",   // VOC_MECHLAFF1           // Mechanic laugh
            "MBOSS1",   // VOC_MECHBOSS1           // "Sure thing, boss" (Mechanic)
            "MYEEHAW1", // VOC_MECHYEEHAW1         // "Yee-haw!" (Mechanic)
            "MHOTDIG1", // VOC_MECHHOTDIG1         // "Hot diggity!" (Mechanic)
            "MWRENCH1", // VOC_MECHWRENCH1         // "I'll get my wrench" (Mechanic)
            "JBURN1",   // VOC_STBURN1             // "Burn, baby, burn!" (Shock Trooper)
            "JCHRGE1",  // VOC_STCHRGE1            // "Fully charged!" (Shock Trooper)
            "JCRISP1",  // VOC_STCRISP1            // "Extra crispy!" (Shock Trooper)
            "JDANCE1",  // VOC_STDANCE1            // "Let's dance!" (Shock Trooper)
            "JJUICE1",  // VOC_STJUICE1            // "Got juice?" (Shock Trooper)
            "JJUMP1",   // VOC_STJUMP1             // "Need a jump?" (Shock Trooper)
            "JLIGHT1",  // VOC_STLIGHT1            // "Lights out" (Shock Trooper)
            "JPOWER1",  // VOC_STPOWER1            // "Power on!" (Shock Trooper)
            "JSHOCK1",  // VOC_STSHOCK1            // "Shocking!" (Shock Trooper)
            "JYES1",    // VOC_STYES1              // "Yes!" (Shock Trooper)
            "CHROTNK1", // VOC_CHRONOTANK1         // Chrono tank
            "FIXIT1",   // VOC_MECH_FIXIT1         // Wrench repair sound
            "MADCHRG2", // VOC_MAD_CHARGE          // M.A.D.tank charging
            "MADEXPLO", // VOC_MAD_EXPLODE         // M.A.D.tank explosion
            "SHKTROP1", // VOC_SHOCK_TROOP1        // Shock trooper tesla
            "BEACON",   // VOC_BEACON			   // Beacon sound
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
            "Water impact",
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

        public static readonly string[] VoxNames = new[]
        {
            "MISNWON1",  // VOX_ACCOMPLISHED              // Mission accomplished
            "MISNLST1",  // VOX_FAIL                      // Your mission has failed
            "PROGRES1",  // VOX_NO_FACTORY                // Building in progress
            "CONSCMP1",  // VOX_CONSTRUCTION              // Construction complete
            "UNITRDY1",  // VOX_UNIT_READY                // Unit ready
            "NEWOPT1",   // VOX_NEW_CONSTRUCT             // New construction options
            "NODEPLY1",  // VOX_DEPLOY                    // Cannot deploy here
            "STRCKIL1",  // VOX_STRUCTURE_DESTROYED       // Structure destroyed
            "NOPOWR1",   // VOX_INSUFFICIENT_POWER        // Insufficient power
            "NOFUNDS1",  // VOX_NO_CASH                   // Insufficient funds
            "BCT1",      // VOX_CONTROL_EXIT              // Battle control terminated
            "REINFOR1",  // VOX_REINFORCEMENTS            // Reinforcements arrived
            "CANCLD1",   // VOX_CANCELED                  // Canceled
            "ABLDGIN1",  // VOX_BUILDING                  // Building
            "LOPOWER1",  // VOX_LOW_POWER                 // Low power
            "NOFUNDS1",  // VOX_NEED_MO_MONEY             // Insufficent funds
            "BASEATK1",  // VOX_BASE_UNDER_ATTACK         // Our base is under attack
            "NOBUILD1",  // VOX_UNABLE_TO_BUILD           // Unable to build more
            "PRIBLDG1",  // VOX_PRIMARY_SELECTED          // Primary building selected
            "TANK01",    // VOX_MADTANK_DEPLOYED          // M.A.D. Tank Deployed
            "none",      // VOX_SOVIET_CAPTURED           // None
            "UNITLST1",  // VOX_UNIT_LOST                 // Unit lost
            "SLCTTGT1",  // VOX_SELECT_TARGET             // Select target
            "ENMYAPP1",  // VOX_PREPARE                   // Enemy approaching
            "SILOND1",   // VOX_NEED_MO_CAPACITY          // Silos needed
            "ONHOLD1",   // VOX_SUSPENDED                 // On hold
            "REPAIR1",   // VOX_REPAIRING                 // Repairing
            "none",      // VOC_none5                     // None
            "none",      // VOC_none6                     // None
            "AUNITL1",   // VOX_AIRCRAFT_LOST             // Airborne unit lost
            "none",      // VOC_none7                     // None
            "AAPPRO1",   // VOX_ALLIED_FORCES_APPROACHING // Allied forces appr.
            "AARRIVE1",  // VOX_ALLIED_APPROACHING        // Allied reinf. arrived
            "none",      // VOC_none8                     // None
            "none",      // VOC_none9                     // None
            "BLDGINF1",  // VOX_BUILDING_INFILTRATED      // Building infiltrated
            "CHROCHR1",  // VOX_CHRONO_CHARGING           // Chronosphere charging
            "CHRORDY1",  // VOX_CHRONO_READY              // Chronosphere ready
            "CHROYES1",  // VOX_CHRONO_TEST               // Chrono test successful
            "CMDCNTR1",  // VOX_HQ_UNDER_ATTACK           // Command cntr under attack
            "CNTLDED1",  // VOX_CENTER_DEACTIVATED        // Control center deactiv.
            "CONVYAP1",  // VOX_CONVOY_APPROACHING        // Convoy approaching
            "CONVLST1",  // VOX_CONVOY_UNIT_LOST          // Convoy unit lost
            "XPLOPLC1",  // VOX_EXPLOSIVE_PLACED          // Explosive placed
            "CREDIT1",   // VOX_MONEY_STOLEN              // Credits stolen
            "NAVYLST1",  // VOX_SHIP_LOST                 // Naval unit lost
            "SATLNCH1",  // VOX_SATALITE_LAUNCHED         // Sattelite launched
            "PULSE1",    // VOX_SONAR_AVAILABLE           // Sonar pulse available
            "none",      // VOC_none10                    // None
            "SOVFAPP1",  // VOX_SOVIET_FORCES_APPROACHING // Soviet forces approaching
            "SOVREIN1",  // VOX_SOVIET_REINFROCEMENTS     // Soviet reinf. arrived
            "TRAIN1",    // VOX_TRAINING                  // Training
            "AREADY1",   // VOX_ABOMB_READY               // A-bomb ready
            "ALAUNCH1",  // VOX_ABOMB_LAUNCH              // A-bomb launch detected
            "AARRIVN1",  // VOX_ALLIES_N                  // Allied reinf. north
            "AARRIVS1",  // VOX_ALLIES_S                  // Allied reinf. south
            "AARIVE1",   // VOX_ALLIES_E                  // Allied reinf. east
            "AARRIVW1",  // VOX_ALLIES_W                  // Allied reinf. west
            "1OBJMET1",  // VOX_OBJECTIVE1                // 1st objective met
            "2OBJMET1",  // VOX_OBJECTIVE2                // 2nd objective met
            "3OBJMET1",  // VOX_OBJECTIVE3                // 3rd objective met
            "IRONCHG1",  // VOX_IRON_CHARGING             // Iron Curtain charging
            "IRONRDY1",  // VOX_IRON_READY                // Iron Curtain ready
            "KOSYRES1",  // VOX_RESCUED                   // Kosygin rescued
            "OBJNMET1",  // VOX_OBJECTIVE_NOT             // Objective not met
            "FLAREN1",   // VOX_SIGNAL_N                  // Signal flare north
            "FLARES1",   // VOX_SIGNAL_S                  // Signal flare south
            "FLAREE1",   // VOX_SIGNAL_E                  // Signal flare east
            "FLAREW1",   // VOX_SIGNAL_W                  // Signal flare west
            "SPYPLN1",   // VOX_SPY_PLANE                 // Spy plane ready
            "TANYAF1",   // VOX_FREED                     // Tanya Freed
            "ARMORUP1",  // VOX_UPGRADE_ARMOR             // Unit armor upgraded
            "FIREPO1",   // VOX_UPGRADE_FIREPOWER         // Unit firepower upgraded
            "UNITSPD1",  // VOX_UPGRADE_SPEED             // Unit speed upgraded
            "MTIMEIN1",  // VOX_MISSION_TIMER             // Mission timer initialised
            "UNITFUL1",  // VOX_UNIT_FULL                 // Unit full
            "UNITREP1",  // VOX_UNIT_REPAIRED             // Unit repaired
            "40MINR",    // VOX_TIME_40                   // 40 minutes remaining
            "30MINR",    // VOX_TIME_30                   // 30 minutes remaining
            "20MINR",    // VOX_TIME_20                   // 20 minutes remaining
            "10MINR",    // VOX_TIME_10                   // 10 minutes remaining
            "5MINR",     // VOX_TIME_5                    // 5 minutes remaining
            "4MINR",     // VOX_TIME_4                    // 4 minutes remaining
            "3MINR",     // VOX_TIME_3                    // 3 minutes remaining
            "2MINR",     // VOX_TIME_2                    // 2 minutes remaining
            "1MINR",     // VOX_TIME_1                    // 1 minutes remaining
            "TIMERNO1",  // VOX_TIME_STOP                 // Timer stopped
            "UNITSLD1",  // VOX_UNIT_SOLD                 // Unit sold
            "TIMERGO1",  // VOX_TIMER_STARTED             // Timer started
            "TARGRES1",  // VOX_TARGET_RESCUED            // Target rescued
            "TARGFRE1",  // VOX_TARGET_FREED              // Target freed
            "TANYAR1",   // VOX_TANYA_RESCUED             // Tanya rescued
            "STRUSLD1",  // VOX_STRUCTURE_SOLD            // Structure sold
            "SOVFORC1",  // VOX_SOVIET_FORCES_FALLEN      // Soviet forces have fallen
            "SOVEMP1",   // VOX_SOVIET_SELECTED           // Soviet Empire selected
            "SOVEFAL1",  // VOX_SOVIET_EMPIRE_FALLEN      // Soviet Empire has fallen
            "OPTERM1",   // VOX_OPERATION_TERMINATED      // Operation control terminated
            "OBJRCH1",   // VOX_OBJECTIVE_REACHED         // Objective reached
            "OBJNRCH1",  // VOX_OBJECTIVE_NOT_REACHED     // Objective not reached
            "OBJMET1",   // VOX_OBJECTIVE_MET             // Objective met
            "MERCR1",    // VOX_MERCENARY_RESCUED         // Mercenary rescued
            "MERCF1",    // VOX_MERCENARY_FREED           // Mercenary freed
            "KOSYFRE1",  // VOX_KOSOYGEN_FREED            // Kosygin freed
            "FLARE1",    // VOX_FLARE_DETECTED            // Signal flare detected
            "COMNDOR1",  // VOX_COMMANDO_RESCUED          // Commando rescued
            "COMNDOF1",  // VOX_COMMANDO_FREED            // Commando freed
            "BLDGPRG1",  // VOX_BUILDING_IN_PROGRESS      // Building in progress
            "ATPREP1",   // VOX_ATOM_PREPPING             // Atom bomb prepping
            "ASELECT1",  // VOX_ALLIED_SELECTED           // Allied forces selected
            "APREP1",    // VOX_ABOMB_PREPPING            // A-bomb prepping
            "ATLNCH1",   // VOX_ATOM_LAUNCHED             // Atom bomb launch detected
            "AFALLEN1",  // VOX_ALLIED_FORCES_FALLEN      // Allied forces have fallen
            "AAVAIL1",   // VOX_ABOMB_AVAILABLE           // A-bomb available
            "AARRIVE1",  // VOX_ALLIED_REINFORCEMENTS     // Allied reinf. arrived
            "SAVE1",     // VOX_MISSION_SAVED             // Mission saved
            "LOAD1"      // VOX_MISSION_LOADED            // Mission loaded
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
            "Mission loaded",
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

        public static readonly string[] SuperTypes = new[]
        {
            "Sonar Pulse",   // SPC_SONAR_PULSE   //Momentarily reveals submarines.
            "Nuclear Bomb",  // SPC_NUCLEAR_BOMB  //Tactical nuclear weapon.
            "Chronosphere",  // SPC_CHRONOSPHERE  //Paradox device, for teleportation
            "Parabombs",     // SPC_PARA_BOMB     //Parachute bomb delivery.
            "Paratroopers",  // SPC_PARA_INFANTRY //Parachute reinforcement delivery.
            "Spy Plane",     // SPC_SPY_MISSION   //Spy plane to take photo recon mission.
            "Iron Curtain",  // SPC_IRON_CURTAIN  //Bestow invulnerability on a unit/building
            "GPS",           // SPC_GPS           //give allies free unjammable radar.
        };

    }
}
