﻿//
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
using MobiusEditor.Utility;
using System;
using System.ComponentModel;
using System.Globalization;

namespace MobiusEditor.Model
{
    public class PercentageTypeConverter : TypeConverter
    {

        bool addPercentageSign;

        public PercentageTypeConverter()
        {
            addPercentageSign = false;
        }

        public PercentageTypeConverter(bool withSign)
        {
            addPercentageSign = withSign;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (context is MapContext) && (sourceType == typeof(string));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (context is MapContext) && (destinationType == typeof(string));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (!(value is int percent) || !CanConvertTo(context, destinationType))
            {
                return null;
            }
            var mapContext = context as MapContext;
            string retVal = mapContext != null && mapContext.FractionalPercentages ? (percent / 100M).ToString("D2") : percent.ToString();
            if (addPercentageSign)
                retVal += "%";
            return retVal;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string str) || !CanConvertFrom(context, value?.GetType()))
            {
                return null;
            }
            str = str.Trim();
            var mapContext = context as MapContext;
            bool isPercentage = str.EndsWith("%");
            if (isPercentage)
                str = str.Substring(0, str.Length - 1);
            if (mapContext != null && mapContext.FractionalPercentages && !isPercentage && str.TrimStart('0').StartsWith("."))
            {
                if (!decimal.TryParse(str, out decimal percent))
                {
                    return null;
                }
                return (int)(percent * 100);
            }
            else
            {
                if (!int.TryParse(str, out int percent))
                {
                    return null;
                }
                return percent;
            }
        }
    }

    public class BasicSection : NotifiableIniSection
    {
        private string name;
        [DefaultValue(null)]
        public string Name { get => name; set => SetField(ref name, value); }

        private int carryOverCap;
        [TypeConverter(typeof(PercentageTypeConverter))]
        [DefaultValue(-1)]
        public int CarryOverCap { get => carryOverCap; set => SetField(ref carryOverCap, value); }

        private int carryOverMoney;
        [TypeConverter(typeof(PercentageTypeConverter))]
        [DefaultValue(100)]
        public int CarryOverMoney { get => carryOverMoney; set => SetField(ref carryOverMoney, value); }

        private string intro;
        [DefaultValue("x")]
        public string Intro { get => intro; set => SetField(ref intro, value); }

        private string theme;
        [DefaultValue("No Theme")]
        public string Theme { get => theme; set => SetField(ref theme, value); }

        private int percent;
        [TypeConverter(typeof(PercentageTypeConverter))]
        [DefaultValue(100)]
        public int Percent { get => percent; set => SetField(ref percent, value); }

        public string player;
        [DefaultValue(null)]
        public string Player { get => player; set => SetField(ref player, value); }

        private string action;
        [DefaultValue("x")]
        public string Action { get => action; set => SetField(ref action, value); }

        private string lose;
        [DefaultValue("x")]
        public string Lose { get => lose; set => SetField(ref lose, value); }

        private string win;
        [DefaultValue("x")]
        public string Win { get => win; set => SetField(ref win, value); }

        private string win2;
        [DefaultValue("x")]
        public string Win2 { get => win2; set => SetField(ref win2, value); }

        private string win3;
        [DefaultValue("x")]
        public string Win3 { get => win3; set => SetField(ref win3, value); }

        private string win4;
        [DefaultValue("x")]
        public string Win4 { get => win4; set => SetField(ref win4, value); }

        private string brief;
        [DefaultValue("x")]
        public string Brief { get => brief; set => SetField(ref brief, value); }

        private string author;
        [DefaultValue(null)]
        public string Author { get => author; set => SetField(ref author, value); }

        private string basePlayer;
        [NonSerializedINIKey]
        [DefaultValue(null)]
        public string BasePlayer { get => basePlayer; set => SetField(ref basePlayer, value); }

        private bool expansionEnabled;
        [NonSerializedINIKey]
        [DefaultValue(true)]
        public bool ExpansionEnabled { get => expansionEnabled; set => SetField(ref expansionEnabled, value); }

        private bool soloMission;
        [DefaultValue(false)]
        public bool SoloMission { get => soloMission; set => SetField(ref soloMission, value); }
    }
}
