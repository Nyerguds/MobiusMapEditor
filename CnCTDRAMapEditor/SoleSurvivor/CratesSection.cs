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
using MobiusEditor.Model;
using System.ComponentModel;

namespace MobiusEditor.SoleSurvivor
{
    public class CratesSection : NotifiableIniSection
    {
        private int addStrength;
        [DefaultValue(100)]
        public int AddStrength { get => addStrength; set => SetField(ref addStrength, value); }

        private int addWeapon;
        [DefaultValue(100)]
        public int AddWeapon { get => addWeapon; set => SetField(ref addWeapon, value); }

        private int addSpeed;
        [DefaultValue(150)]
        public int AddSpeed { get => addSpeed; set => SetField(ref addSpeed, value); }

        private int rapidReload;
        [DefaultValue(100)]
        public int RapidReload { get => rapidReload; set => SetField(ref rapidReload, value); }

        private int addRange;
        [DefaultValue(100)]
        public int AddRange { get => addRange; set => SetField(ref addRange, value); }

        private int heal;
        [DefaultValue(200)]
        public int Heal { get => heal; set => SetField(ref heal, value); }

        private int bomb;
        [DefaultValue(200)]
        public int Bomb { get => bomb; set => SetField(ref bomb, value); }

        private int stealth;
        [DefaultValue(50)]
        public int Stealth { get => stealth; set => SetField(ref stealth, value); }

        private int teleport;
        [DefaultValue(50)]
        public int Teleport { get => teleport; set => SetField(ref teleport, value); }

        private int kill;
        [DefaultValue(0)]
        public int Kill { get => kill; set => SetField(ref kill, value); }

        private int uncloakAll;
        [DefaultValue(10)]
        public int UncloakAll { get => uncloakAll; set => SetField(ref uncloakAll, value); }

        private int reshroud;
        [DefaultValue(5)]
        public int Reshroud { get => reshroud; set => SetField(ref reshroud, value); }

        private int unshroud;
        [DefaultValue(5)]
        public int Unshroud { get => unshroud; set => SetField(ref unshroud, value); }

        private int radar;
        [DefaultValue(20)]
        public int Radar { get => radar; set => SetField(ref radar, value); }

        private int armageddon;
        [DefaultValue(1)]
        public int Armageddon { get => armageddon; set => SetField(ref armageddon, value); }

        private int super;
        [DefaultValue(1)]
        public int Super { get => super; set => SetField(ref super, value); }

        private int density;
        [DefaultValue(200)]
        public int Density { get => density; set => SetField(ref density, value); }

        private int ionFactor;
        [DefaultValue(400)]
        public int IonFactor { get => ionFactor; set => SetField(ref ionFactor, value); }

        private int crateTimer;
        [DefaultValue(300)]
        public int CrateTimer { get => crateTimer; set => SetField(ref crateTimer, value); }
    }
}
