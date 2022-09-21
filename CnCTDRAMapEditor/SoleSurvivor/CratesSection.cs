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
