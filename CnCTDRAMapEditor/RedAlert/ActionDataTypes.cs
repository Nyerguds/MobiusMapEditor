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
using MobiusEditor.Model;

namespace MobiusEditor.RedAlert
{
    public static class ActionDataTypes
    {

        public static readonly ListItem<string>[] VocTypes = new[]
        {
            new ListItem<string>("GIRLOKAY", "\"Okay\" (female)"),                         // VOC_GIRL_OKAY
            new ListItem<string>("GIRLYEAH", "\"Yeah?\" (female)"),                        // VOC_GIRL_YEAH
            new ListItem<string>("GUYOKAY1", "\"Okay\" (male)"),                           // VOC_GUY_OKAY
            new ListItem<string>("GUYYEAH1", "\"Yeah?\" (male)"),                          // VOC_GUY_YEAH
            new ListItem<string>("MINELAY1", "Mine placed"),                               // VOC_MINELAY1
            new ListItem<string>("ACKNO",    "\"Acknowledged\""),                          // VOC_ACKNOWL
            new ListItem<string>("AFFIRM1",  "\"Affirmative\""),                           // VOC_AFFIRM
            new ListItem<string>("AWAIT1",   "\"Awaiting orders\""),                       // VOC_AWAIT
            new ListItem<string>("EAFFIRM1", "\"Affirmative\" (Engineer)"),                // VOC_ENG_AFFIRM
            new ListItem<string>("EENGIN1",  "\"Engineering\" (Engineer)"),                // VOC_ENG_ENG
            new ListItem<string>("NOPROB",   "\"Of course\""),                             // VOC_NO_PROB
            new ListItem<string>("READY",    "\"Ready and waiting\""),                     // VOC_READY
            new ListItem<string>("REPORT1",  "\"Reporting\""),                             // VOC_REPORT
            new ListItem<string>("RITAWAY",  "\"At once\""),                               // VOC_RIGHT_AWAY
            new ListItem<string>("ROGER",    "\"Agreed\""),                                // VOC_ROGER
            new ListItem<string>("UGOTIT",   "\"Very well\""),                             // VOC_UGOTIT
            new ListItem<string>("VEHIC1",   "\"Vehicle reporting\""),                     // VOC_VEHIC
            new ListItem<string>("YESSIR1",  "\"Yes sir?\""),                              // VOC_YESSIR
            new ListItem<string>("DEDMAN1",  "Man dies #1"),                               // VOC_SCREAM1
            new ListItem<string>("DEDMAN2",  "Man dies #2"),                               // VOC_SCREAM3
            new ListItem<string>("DEDMAN3",  "Man dies #3"),                               // VOC_SCREAM4
            new ListItem<string>("DEDMAN4",  "Man dies #4"),                               // VOC_SCREAM5
            new ListItem<string>("DEDMAN5",  "Man dies #5"),                               // VOC_SCREAM6
            new ListItem<string>("DEDMAN6",  "Man dies #6"),                               // VOC_SCREAM7
            new ListItem<string>("DEDMAN7",  "Man dies #7"),                               // VOC_SCREAM10
            new ListItem<string>("DEDMAN8",  "Man dies #8"),                               // VOC_SCREAM11
            new ListItem<string>("DEDMAN10", "Man dies #9"),                               // VOC_YELL1
            new ListItem<string>("CHRONO2",  "Chronosphere"),                              // VOC_CHRONO
            new ListItem<string>("CANNON1",  "Mammoth Tank gun"),                          // VOC_CANNON1
            new ListItem<string>("CANNON2",  "Light Tank gun"),                            // VOC_CANNON2
            new ListItem<string>("IRONCUR9", "Iron Curtain"),                              // VOC_IRON1
            new ListItem<string>("EMOVOUT1", "\"Movin' out\" (Engineer)"),                 // VOC_ENG_MOVEOUT
            new ListItem<string>("SONPULSE", "Sonar pulse"),                               // VOC_SONAR
            new ListItem<string>("SANDBAG2", "Sandbag crushed"),                           // VOC_SANDBAG
            new ListItem<string>("MINEBLO1", "AT mine explodes"),                          // VOC_MINEBLOW
            new ListItem<string>("CHUTE1",   "Parachute"),                                 // VOC_CHUTE1
            new ListItem<string>("DOGY1",    "Dog bark"),                                  // VOC_DOG_BARK
            new ListItem<string>("DOGW5",    "Dog whining"),                               // VOC_DOG_WHINE
            new ListItem<string>("DOGG5P",   "Dog angry"),                                 // VOC_DOG_GROWL2
            new ListItem<string>("FIREBL3",  "Fireball"),                                  // VOC_FIRE_LAUNCH
            new ListItem<string>("FIRETRT1", "Fireball impact"),                           // VOC_FIRE_EXPLODE
            new ListItem<string>("GRENADE1", "Grenade throw"),                             // VOC_GRENADE_TOSS
            new ListItem<string>("GUN11",    "Rifle"),                                     // VOC_GUN_5
            new ListItem<string>("GUN13",    "Pillbox machinegun"),                        // VOC_GUN_7
            new ListItem<string>("EYESSIR1", "\"Yes sir\" (Engineer)"),                    // VOC_ENG_YES
            new ListItem<string>("GUN27",    "Pistol #1"),                                 // VOC_GUN_RIFLE
            new ListItem<string>("HEAL2",    "Healing"),                                   // VOC_HEAL
            new ListItem<string>("HYDROD1",  "Hissing"),                                   // VOC_DOOR
            new ListItem<string>("INVUL2",   "Vworap"),                                    // VOC_INVULNERABLE
            new ListItem<string>("KABOOM1",  "Building half-destroyed"),                   // VOC_KABOOM1
            new ListItem<string>("KABOOM12", "tank shell impact"),                         // VOC_KABOOM12
            new ListItem<string>("KABOOM15", "Explosion"),                                 // VOC_KABOOM15
            new ListItem<string>("SPLASH9",  "Water impact"),                              // VOC_SPLASH
            new ListItem<string>("KABOOM22", "big explosion"),                             // VOC_KABOOM22
            new ListItem<string>("AACANON3", "AA gun"),                                    // VOC_AACANON3
            new ListItem<string>("TANDETH1", "Tanya screams"),                             // VOC_TANYA_DIE
            new ListItem<string>("MGUNINF1", "Machinegun"),                                // VOC_GUN_5F
            new ListItem<string>("MISSILE1", "AA missile"),                                // VOC_MISSILE_1
            new ListItem<string>("MISSILE6", "Cruiser missile"),                           // VOC_MISSILE_2
            new ListItem<string>("MISSILE7", "MIG missile"),                               // VOC_MISSILE_3
            new ListItem<string>("x",        "x"),                                         // VOC_x6
            new ListItem<string>("PILLBOX1", "Ranger machinegun"),                         // VOC_GUN_5R
            new ListItem<string>("RABEEP1",  "High-pitched beep"),                         // VOC_BEEP
            new ListItem<string>("RAMENU1",  "Menu click"),                                // VOC_CLICK
            new ListItem<string>("SILENCER", "Silenced rifle"),                            // VOC_SILENCER
            new ListItem<string>("TANK5",    "Artillery fire"),                            // VOC_CANNON6
            new ListItem<string>("TANK6",    "Cruiser cannon"),                            // VOC_CANNON7
            new ListItem<string>("TORPEDO1", "Torpedo"),                                   // VOC_TORPEDO
            new ListItem<string>("TURRET1",  "Turret shot"),                               // VOC_CANNON8
            new ListItem<string>("TSLACHG2", "Tesla charging"),                            // VOC_TESLA_POWER_UP
            new ListItem<string>("TESLA1",   "Tesla firing"),                              // VOC_TESLA_ZAP
            new ListItem<string>("SQUISHY2", "Person crushed"),                            // VOC_SQUISH
            new ListItem<string>("SCOLDY1",  "Blip"),                                      // VOC_SCOLD
            new ListItem<string>("RADARON2", "Radar online"),                              // VOC_RADAR_ON
            new ListItem<string>("RADARDN1", "Radar offline"),                             // VOC_RADAR_OFF
            new ListItem<string>("PLACBLDG", "Building placed"),                           // VOC_PLACE_BUILDING_DOWN
            new ListItem<string>("KABOOM30", "Explosion"),                                 // VOC_KABOOM30
            new ListItem<string>("KABOOM25", "Artillery impact"),                          // VOC_KABOOM25
            new ListItem<string>("x",        "x"),                                         // VOC_x7
            new ListItem<string>("DOGW7",    "Dog dies"),                                  // VOC_DOG_HURT
            new ListItem<string>("DOGW3PX",  "Dog response"),                              // VOC_DOG_YES
            new ListItem<string>("CRMBLE2",  "Building crumbles"),                         // VOC_CRUMBLE
            new ListItem<string>("CASHUP1",  "Cash coming in"),                            // VOC_MONEY_UP
            new ListItem<string>("CASHDN1",  "Cash going out"),                            // VOC_MONEY_DOWN
            new ListItem<string>("BUILD5",   "Building up"),                               // VOC_CONSTRUCTION
            new ListItem<string>("BLEEP9",   "Radar powering up"),                         // VOC_GAME_CLOSED
            new ListItem<string>("BLEEP6",   "Information message"),                       // VOC_INCOMING_MESSAGE
            new ListItem<string>("BLEEP5",   "Alarm"),                                     // VOC_SYS_ERROR
            new ListItem<string>("BLEEP17",  "Soft bleep"),                                // VOC_OPTIONS_CHANGED
            new ListItem<string>("BLEEP13",  "soft low bleep"),                            // VOC_GAME_FORMING
            new ListItem<string>("BLEEP12",  "high-pitched bleep down"),                   // VOC_PLAYER_LEFT
            new ListItem<string>("BLEEP11",  "High-pitched bleep up"),                     // VOC_PLAYER_JOINED
            new ListItem<string>("H2OBOMB2", "Water explosion"),                           // VOC_DEPTH_CHARGE
            new ListItem<string>("CASHTURN", "Selling sound"),                             // VOC_CASHTURN
            new ListItem<string>("TUFFGUY1", "\"Chew on this!\" (Tanya)"),                 // VOC_TANYA_CHEW
            new ListItem<string>("ROKROLL1", "\"Let's rock!\" (Tanya)"),                   // VOC_TANYA_ROCK
            new ListItem<string>("LAUGH1",   "Tanya laughing"),                            // VOC_TANYA_LAUGH
            new ListItem<string>("CMON1",    "\"Shake it, baby!\" (Tanya)"),               // VOC_TANYA_SHAKE
            new ListItem<string>("BOMBIT1",  "\"Cha-ching!\" (Tanya)"),                    // VOC_TANYA_CHING
            new ListItem<string>("GOTIT1",   "\"That's all you got?\" (Tanya)"),           // VOC_TANYA_GOT
            new ListItem<string>("KEEPEM1",  "\"Kiss is bye-bye!\" (Tanya)"),              // VOC_TANYA_KISS
            new ListItem<string>("ONIT1",    "\"I'm there!\" (Tanya)"),                    // VOC_TANYA_THERE
            new ListItem<string>("LEFTY1",   "\"Give it to me!\" (Tanya)"),                // VOC_TANYA_GIVE
            new ListItem<string>("YEAH1",    "\"Yeah?\" (Tanya)"),                         // VOC_TANYA_YEA
            new ListItem<string>("YES1",     "\"Yes, sir?\" (Tanya)"),                     // VOC_TANYA_YES
            new ListItem<string>("YO1",      "\"What's up?\" (Tanya)"),                    // VOC_TANYA_WHATS
            new ListItem<string>("WALLKIL2", "Fence crushed"),                             // VOC_WALLKILL2
            new ListItem<string>("x",        "x"),                                         // VOC_x8
            new ListItem<string>("GUN5",     "Pistol #2"),                                 // VOC_TRIPLE_SHOT
            new ListItem<string>("SUBSHOW1", "Submarine surfacing"),                       // VOC_SUBSHOW
            new ListItem<string>("EINAH1",   "\"Ah?\" (Einstein)"),                        // VOC_E_AH
            new ListItem<string>("EINOK1",   "\"Incredible!\" (Einstein)"),                // VOC_E_OK
            new ListItem<string>("EINYES1",  "\"Yes.\" (Einstein)"),                       // VOC_E_YES
            new ListItem<string>("MINE1",    "AP mine explodes"),                          // VOC_TRIP_MINE
            new ListItem<string>("SCOMND1",  "\"Commander?\" (Spy)"),                      // VOC_SPY_COMMANDER
            new ListItem<string>("SYESSIR1", "\"Yes, sir?\" (Sky)"),                       // VOC_SPY_YESSIR
            new ListItem<string>("SINDEED1", "\"Indeed\" (Spy)"),                          // VOC_SPY_INDEED
            new ListItem<string>("SONWAY1",  "\"On my way\" (Spy)"),                       // VOC_SPY_ONWAY
            new ListItem<string>("SKING1",   "\"For king and country\" (Spy)"),            // VOC_SPY_KING
            new ListItem<string>("MRESPON1", "\"Medic reporting\" (Medic)"),               // VOC_MED_REPORTING
            new ListItem<string>("MYESSIR1", "\"Yes, sir\" (Medic)"),                      // VOC_MED_YESSIR
            new ListItem<string>("MAFFIRM1", "\"Affirmative\" (Medic)"),                   // VOC_MED_AFFIRM
            new ListItem<string>("MMOVOUT1", "\"Moving out\" (Medic)"),                    // VOC_MED_MOVEOUT
            new ListItem<string>("BEEPSLCT", "Select beep"),                               // VOC_BEEP_SELECT
            new ListItem<string>("SYEAH1",   "\"Yeah?\" (Thief)"),                         // VOC_THIEF_YEA
            new ListItem<string>("ANTDIE",   "Ant dies"),                                  // VOC_ANTDIE
            new ListItem<string>("ANTBITE",  "Ant bites"),                                 // VOC_ANTBITE
            new ListItem<string>("SMOUT1",   "\"Moving out\" (Thief)"),                    // VOC_THIEF_MOVEOUT
            new ListItem<string>("SOKAY1",   "\"Okay\" (Thief)"),                          // VOC_THIEF_OKAY
            new ListItem<string>("x",        "x"),                                         // VOC_x11
            new ListItem<string>("SWHAT1",   "\"What?\" (Thief)"),                         // VOC_THIEF_WHAT
            new ListItem<string>("SAFFIRM1", "\"Affirmative\" (Thief)"),                   // VOC_THIEF_AFFIRM
            new ListItem<string>("STAVCMDR", "\"Commander?\" (Stavros)"),                  // VOC_STAVCMDR
            new ListItem<string>("STAVCRSE", "\"Of course\" (Stavros)"),                   // VOC_STAVCRSE
            new ListItem<string>("STAVYES",  "\"Yes\" (Stavros)"),                         // VOC_STAVYES
            new ListItem<string>("STAVMOV",  "\"Move out\" (Stavros)"),                    // VOC_STAVMOV
            new ListItem<string>("BUZZY1",   "Warning siren"),                             // VOC_BUZZY1
            new ListItem<string>("RAMBO1",   "\"I've got a present for ya!\" (Commando)"), // VOC_RAMBO1
            new ListItem<string>("RAMBO2",   "Commando laugh"),                            // VOC_RAMBO2
            new ListItem<string>("RAMBO3",   "\"Real tough guy!\" (Commando)"),            // VOC_RAMBO3
            new ListItem<string>("MYES1",    "\"Yes sir\" (Mechanic)"),                    // VOC_MECHYES1
            new ListItem<string>("MHOWDY1",  "\"Howdy?\" (Mechanic)"),                     // VOC_MECHHOWDY1
            new ListItem<string>("MRISE1",   "\"Rise 'n' shine!\" (Mechanic)"),            // VOC_MECHRISE1
            new ListItem<string>("MHUH1",    "\"Huh?\" (Mechanic)"),                       // VOC_MECHHUH1
            new ListItem<string>("MHEAR1",   "\"I hear ya\" (Mechanic)"),                  // VOC_MECHHEAR1
            new ListItem<string>("MLAFF1",   "Mechanic laugh"),                            // VOC_MECHLAFF1
            new ListItem<string>("MBOSS1",   "\"Sure thing, boss\" (Mechanic)"),           // VOC_MECHBOSS1
            new ListItem<string>("MYEEHAW1", "\"Yee-haw!\" (Mechanic)"),                   // VOC_MECHYEEHAW1
            new ListItem<string>("MHOTDIG1", "\"Hot diggity!\" (Mechanic)"),               // VOC_MECHHOTDIG1
            new ListItem<string>("MWRENCH1", "\"I'll get my wrench\" (Mechanic)"),         // VOC_MECHWRENCH1
            new ListItem<string>("JBURN1",   "\"Burn, baby, burn!\" (Shock Trooper)"),     // VOC_STBURN1
            new ListItem<string>("JCHRGE1",  "\"Fully charged!\" (Shock Trooper)"),        // VOC_STCHRGE1
            new ListItem<string>("JCRISP1",  "\"Extra crispy!\" (Shock Trooper)"),         // VOC_STCRISP1
            new ListItem<string>("JDANCE1",  "\"Let's dance!\" (Shock Trooper)"),          // VOC_STDANCE1
            new ListItem<string>("JJUICE1",  "\"Got juice?\" (Shock Trooper)"),            // VOC_STJUICE1
            new ListItem<string>("JJUMP1",   "\"Need a jump?\" (Shock Trooper)"),          // VOC_STJUMP1
            new ListItem<string>("JLIGHT1",  "\"Lights out\" (Shock Trooper)"),            // VOC_STLIGHT1
            new ListItem<string>("JPOWER1",  "\"Power on!\" (Shock Trooper)"),             // VOC_STPOWER1
            new ListItem<string>("JSHOCK1",  "\"Shocking!\" (Shock Trooper)"),             // VOC_STSHOCK1
            new ListItem<string>("JYES1",    "\"Yes!\" (Shock Trooper)"),                  // VOC_STYES1
            new ListItem<string>("CHROTNK1", "Chrono tank"),                               // VOC_CHRONOTANK1
            new ListItem<string>("FIXIT1",   "Wrench repair sound"),                       // VOC_MECH_FIXIT1
            new ListItem<string>("MADCHRG2", "M.A.D. tank charging"),                      // VOC_MAD_CHARGE
            new ListItem<string>("MADEXPLO", "M.A.D. tank explosion"),                     // VOC_MAD_EXPLODE
            new ListItem<string>("SHKTROP1", "Shock trooper tesla"),                       // VOC_SHOCK_TROOP1
            new ListItem<string>("BEACON",   "Beacon sound"),                              // VOC_BEACON
        };

        public static readonly ListItem<string>[] VoxTypes = new[]
        {
            new ListItem<string>("MISNWON1", "Mission accomplished"),         // VOX_ACCOMPLISHED
            new ListItem<string>("MISNLST1", "Your mission has failed"),      // VOX_FAIL
            new ListItem<string>("PROGRES1", "Building in progress"),         // VOX_NO_FACTORY
            new ListItem<string>("CONSCMP1", "Construction complete"),        // VOX_CONSTRUCTION
            new ListItem<string>("UNITRDY1", "Unit ready"),                   // VOX_UNIT_READY
            new ListItem<string>("NEWOPT1",  "New construction options"),     // VOX_NEW_CONSTRUCT
            new ListItem<string>("NODEPLY1", "Cannot deploy here"),           // VOX_DEPLOY
            new ListItem<string>("STRCKIL1", "Structure destroyed"),          // VOX_STRUCTURE_DESTROYED
            new ListItem<string>("NOPOWR1",  "Insufficient power"),           // VOX_INSUFFICIENT_POWER
            new ListItem<string>("NOFUNDS1", "Insufficient funds"),           // VOX_NO_CASH
            new ListItem<string>("BCT1",     "Battle control terminated"),    // VOX_CONTROL_EXIT
            new ListItem<string>("REINFOR1", "Reinforcements arrived"),       // VOX_REINFORCEMENTS
            new ListItem<string>("CANCLD1",  "Canceled"),                     // VOX_CANCELED
            new ListItem<string>("ABLDGIN1", "Building"),                     // VOX_BUILDING
            new ListItem<string>("LOPOWER1", "Low power"),                    // VOX_LOW_POWER
            new ListItem<string>("NOFUNDS1", "Insufficent funds"),            // VOX_NEED_MO_MONEY
            new ListItem<string>("BASEATK1", "Our base is under attack"),     // VOX_BASE_UNDER_ATTACK
            new ListItem<string>("NOBUILD1", "Unable to build more"),         // VOX_UNABLE_TO_BUILD
            new ListItem<string>("PRIBLDG1", "Primary building selected"),    // VOX_PRIMARY_SELECTED
            new ListItem<string>("TANK01",   "M.A.D. Tank Deployed"),         // VOX_MADTANK_DEPLOYED
            new ListItem<string>("none",     "None"),                         // VOX_SOVIET_CAPTURED
            new ListItem<string>("UNITLST1", "Unit lost"),                    // VOX_UNIT_LOST
            new ListItem<string>("SLCTTGT1", "Select target"),                // VOX_SELECT_TARGET
            new ListItem<string>("ENMYAPP1", "Enemy approaching"),            // VOX_PREPARE
            new ListItem<string>("SILOND1",  "Silos needed"),                 // VOX_NEED_MO_CAPACITY
            new ListItem<string>("ONHOLD1",  "On hold"),                      // VOX_SUSPENDED
            new ListItem<string>("REPAIR1",  "Repairing"),                    // VOX_REPAIRING
            new ListItem<string>("none",     "None"),                         // VOX_none5
            new ListItem<string>("none",     "None"),                         // VOX_none6
            new ListItem<string>("AUNITL1",  "Airborne unit lost"),           // VOX_AIRCRAFT_LOST
            new ListItem<string>("none",     "None"),                         // VOX_none7
            new ListItem<string>("AAPPRO1",  "Allied forces appr."),          // VOX_ALLIED_FORCES_APPROACHING
            new ListItem<string>("AARRIVE1", "Allied reinf. arrived"),        // VOX_ALLIED_APPROACHING
            new ListItem<string>("none",     "None"),                         // VOX_none8
            new ListItem<string>("none",     "None"),                         // VOX_none9
            new ListItem<string>("BLDGINF1", "Building infiltrated"),         // VOX_BUILDING_INFILTRATED
            new ListItem<string>("CHROCHR1", "Chronosphere charging"),        // VOX_CHRONO_CHARGING
            new ListItem<string>("CHRORDY1", "Chronosphere ready"),           // VOX_CHRONO_READY
            new ListItem<string>("CHROYES1", "Chrono test successful"),       // VOX_CHRONO_TEST
            new ListItem<string>("CMDCNTR1", "Command cntr under attack"),    // VOX_HQ_UNDER_ATTACK
            new ListItem<string>("CNTLDED1", "Control center deactiv."),      // VOX_CENTER_DEACTIVATED
            new ListItem<string>("CONVYAP1", "Convoy approaching"),           // VOX_CONVOY_APPROACHING
            new ListItem<string>("CONVLST1", "Convoy unit lost"),             // VOX_CONVOY_UNIT_LOST
            new ListItem<string>("XPLOPLC1", "Explosive placed"),             // VOX_EXPLOSIVE_PLACED
            new ListItem<string>("CREDIT1",  "Credits stolen"),               // VOX_MONEY_STOLEN
            new ListItem<string>("NAVYLST1", "Naval unit lost"),              // VOX_SHIP_LOST
            new ListItem<string>("SATLNCH1", "Sattelite launched"),           // VOX_SATALITE_LAUNCHED
            new ListItem<string>("PULSE1",   "Sonar pulse available"),        // VOX_SONAR_AVAILABLE
            new ListItem<string>("none",     "None"),                         // VOX_none10
            new ListItem<string>("SOVFAPP1", "Soviet forces approaching"),    // VOX_SOVIET_FORCES_APPROACHING
            new ListItem<string>("SOVREIN1", "Soviet reinf. arrived"),        // VOX_SOVIET_REINFROCEMENTS
            new ListItem<string>("TRAIN1",   "Training"),                     // VOX_TRAINING
            new ListItem<string>("AREADY1",  "A-bomb ready"),                 // VOX_ABOMB_READY
            new ListItem<string>("ALAUNCH1", "A-bomb launch detected"),       // VOX_ABOMB_LAUNCH
            new ListItem<string>("AARRIVN1", "Allied reinf. north"),          // VOX_ALLIES_N
            new ListItem<string>("AARRIVS1", "Allied reinf. south"),          // VOX_ALLIES_S
            new ListItem<string>("AARIVE1",  "Allied reinf. east"),           // VOX_ALLIES_E
            new ListItem<string>("AARRIVW1", "Allied reinf. west"),           // VOX_ALLIES_W
            new ListItem<string>("1OBJMET1", "1st objective met"),            // VOX_OBJECTIVE1
            new ListItem<string>("2OBJMET1", "2nd objective met"),            // VOX_OBJECTIVE2
            new ListItem<string>("3OBJMET1", "3rd objective met"),            // VOX_OBJECTIVE3
            new ListItem<string>("IRONCHG1", "Iron Curtain charging"),        // VOX_IRON_CHARGING
            new ListItem<string>("IRONRDY1", "Iron Curtain ready"),           // VOX_IRON_READY
            new ListItem<string>("KOSYRES1", "Kosygin rescued"),              // VOX_RESCUED
            new ListItem<string>("OBJNMET1", "Objective not met"),            // VOX_OBJECTIVE_NOT
            new ListItem<string>("FLAREN1",  "Signal flare north"),           // VOX_SIGNAL_N
            new ListItem<string>("FLARES1",  "Signal flare south"),           // VOX_SIGNAL_S
            new ListItem<string>("FLAREE1",  "Signal flare east"),            // VOX_SIGNAL_E
            new ListItem<string>("FLAREW1",  "Signal flare west"),            // VOX_SIGNAL_W
            new ListItem<string>("SPYPLN1",  "Spy plane ready"),              // VOX_SPY_PLANE
            new ListItem<string>("TANYAF1",  "Tanya Freed"),                  // VOX_FREED
            new ListItem<string>("ARMORUP1", "Unit armor upgraded"),          // VOX_UPGRADE_ARMOR
            new ListItem<string>("FIREPO1",  "Unit firepower upgraded"),      // VOX_UPGRADE_FIREPOWER
            new ListItem<string>("UNITSPD1", "Unit speed upgraded"),          // VOX_UPGRADE_SPEED
            new ListItem<string>("MTIMEIN1", "Mission timer initialised"),    // VOX_MISSION_TIMER
            new ListItem<string>("UNITFUL1", "Unit full"),                    // VOX_UNIT_FULL
            new ListItem<string>("UNITREP1", "Unit repaired"),                // VOX_UNIT_REPAIRED
            new ListItem<string>("40MINR",   "40 minutes remaining"),         // VOX_TIME_40
            new ListItem<string>("30MINR",   "30 minutes remaining"),         // VOX_TIME_30
            new ListItem<string>("20MINR",   "20 minutes remaining"),         // VOX_TIME_20
            new ListItem<string>("10MINR",   "10 minutes remaining"),         // VOX_TIME_10
            new ListItem<string>("5MINR",    "5 minutes remaining"),          // VOX_TIME_5
            new ListItem<string>("4MINR",    "4 minutes remaining"),          // VOX_TIME_4
            new ListItem<string>("3MINR",    "3 minutes remaining"),          // VOX_TIME_3
            new ListItem<string>("2MINR",    "2 minutes remaining"),          // VOX_TIME_2
            new ListItem<string>("1MINR",    "1 minutes remaining"),          // VOX_TIME_1
            new ListItem<string>("TIMERNO1", "Timer stopped"),                // VOX_TIME_STOP
            new ListItem<string>("UNITSLD1", "Unit sold"),                    // VOX_UNIT_SOLD
            new ListItem<string>("TIMERGO1", "Timer started"),                // VOX_TIMER_STARTED
            new ListItem<string>("TARGRES1", "Target rescued"),               // VOX_TARGET_RESCUED
            new ListItem<string>("TARGFRE1", "Target freed"),                 // VOX_TARGET_FREED
            new ListItem<string>("TANYAR1",  "Tanya rescued"),                // VOX_TANYA_RESCUED
            new ListItem<string>("STRUSLD1", "Structure sold"),               // VOX_STRUCTURE_SOLD
            new ListItem<string>("SOVFORC1", "Soviet forces have fallen"),    // VOX_SOVIET_FORCES_FALLEN
            new ListItem<string>("SOVEMP1",  "Soviet Empire selected"),       // VOX_SOVIET_SELECTED
            new ListItem<string>("SOVEFAL1", "Soviet Empire has fallen"),     // VOX_SOVIET_EMPIRE_FALLEN
            new ListItem<string>("OPTERM1",  "Operation control terminated"), // VOX_OPERATION_TERMINATED
            new ListItem<string>("OBJRCH1",  "Objective reached"),            // VOX_OBJECTIVE_REACHED
            new ListItem<string>("OBJNRCH1", "Objective not reached"),        // VOX_OBJECTIVE_NOT_REACHED
            new ListItem<string>("OBJMET1",  "Objective met"),                // VOX_OBJECTIVE_MET
            new ListItem<string>("MERCR1",   "Mercenary rescued"),            // VOX_MERCENARY_RESCUED
            new ListItem<string>("MERCF1",   "Mercenary freed"),              // VOX_MERCENARY_FREED
            new ListItem<string>("KOSYFRE1", "Kosygin freed"),                // VOX_KOSOYGEN_FREED
            new ListItem<string>("FLARE1",   "Signal flare detected"),        // VOX_FLARE_DETECTED
            new ListItem<string>("COMNDOR1", "Commando rescued"),             // VOX_COMMANDO_RESCUED
            new ListItem<string>("COMNDOF1", "Commando freed"),               // VOX_COMMANDO_FREED
            new ListItem<string>("BLDGPRG1", "Building in progress"),         // VOX_BUILDING_IN_PROGRESS
            new ListItem<string>("ATPREP1",  "Atom bomb prepping"),           // VOX_ATOM_PREPPING
            new ListItem<string>("ASELECT1", "Allied forces selected"),       // VOX_ALLIED_SELECTED
            new ListItem<string>("APREP1",   "A-bomb prepping"),              // VOX_ABOMB_PREPPING
            new ListItem<string>("ATLNCH1",  "Atom bomb launch detected"),    // VOX_ATOM_LAUNCHED
            new ListItem<string>("AFALLEN1", "Allied forces have fallen"),    // VOX_ALLIED_FORCES_FALLEN
            new ListItem<string>("AAVAIL1",  "A-bomb available"),             // VOX_ABOMB_AVAILABLE
            new ListItem<string>("AARRIVE1", "Allied reinf. arrived"),        // VOX_ALLIED_REINFORCEMENTS
            new ListItem<string>("SAVE1",    "Mission saved"),                // VOX_MISSION_SAVED
            new ListItem<string>("LOAD1",    "Mission loaded"),               // VOX_MISSION_LOADED
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
