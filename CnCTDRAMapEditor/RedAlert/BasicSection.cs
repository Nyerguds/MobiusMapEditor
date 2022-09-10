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
using MobiusEditor.Utility;
using System.ComponentModel;

namespace MobiusEditor.RedAlert
{
    public class BasicSection : Model.BasicSection
    {
        private bool toCarryOver;
        [TypeConverter(typeof(YesNoBooleanTypeConverter))]
        [DefaultValue(false)]
        public bool ToCarryOver { get => toCarryOver; set => SetField(ref toCarryOver, value); }

        private bool toInherit;
        [TypeConverter(typeof(YesNoBooleanTypeConverter))]
        [DefaultValue(false)]
        public bool ToInherit { get => toInherit; set => SetField(ref toInherit, value); }

        private bool timerInherit;
        [TypeConverter(typeof(YesNoBooleanTypeConverter))]
        [DefaultValue(false)]
        public bool TimerInherit { get => timerInherit; set => SetField(ref timerInherit, value); }

        private bool civEvac;
        [TypeConverter(typeof(YesNoBooleanTypeConverter))]
        [DefaultValue(false)]
        public bool CivEvac { get => civEvac; set => SetField(ref civEvac, value); }

        private bool endOfGame;
        [TypeConverter(typeof(YesNoBooleanTypeConverter))]
        [DefaultValue(false)]
        public bool EndOfGame { get => endOfGame; set => SetField(ref endOfGame, value); }

        private bool noSpyPlane;
        [TypeConverter(typeof(YesNoBooleanTypeConverter))]
        [DefaultValue(false)]
        public bool NoSpyPlane { get => noSpyPlane; set => SetField(ref noSpyPlane, value); }

        private bool skipScore;
        [TypeConverter(typeof(YesNoBooleanTypeConverter))]
        [DefaultValue(false)]
        public bool SkipScore { get => skipScore; set => SetField(ref skipScore, value); }

        private bool oneTimeOnly;
        [TypeConverter(typeof(YesNoBooleanTypeConverter))]
        [DefaultValue(false)]
        public bool OneTimeOnly { get => oneTimeOnly; set => SetField(ref oneTimeOnly, value); }

        private bool skipMapSelect;
        [TypeConverter(typeof(YesNoBooleanTypeConverter))]
        [DefaultValue(false)]
        public bool SkipMapSelect { get => skipMapSelect; set => SetField(ref skipMapSelect, value); }

        private bool official;
        [TypeConverter(typeof(YesNoBooleanTypeConverter))]
        [DefaultValue(false)]
        public bool Official { get => official; set => SetField(ref official, value); }

        private bool fillSilos;
        [TypeConverter(typeof(YesNoBooleanTypeConverter))]
        [DefaultValue(false)]
        public bool FillSilos { get => fillSilos; set => SetField(ref fillSilos, value); }

        private bool truckCrate;
        [TypeConverter(typeof(YesNoBooleanTypeConverter))]
        [DefaultValue(false)]
        public bool TruckCrate { get => truckCrate; set => SetField(ref truckCrate, value); }

    }
}
