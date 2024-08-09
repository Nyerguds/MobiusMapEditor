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
using MobiusEditor.Utility;
using System.ComponentModel;

namespace MobiusEditor.RedAlert
{
    public class LandIniSection
    {
        public LandIniSection(int footSpeed, int trackSpeed, int wheelSpeed, int floatSpeed, bool buildable)
        {
            this.Foot = footSpeed;
            this.Track = trackSpeed;
            this.Wheel = wheelSpeed;
            this.Float = floatSpeed;
            this.Buildable = buildable;
        }

        [DefaultValue(0)]
        [TypeConverter(typeof(PercentageTypeConverter))]
        public int Foot { get; set; }

        [DefaultValue(0)]
        [TypeConverter(typeof(PercentageTypeConverter))]
        public int Track { get; set; }

        [DefaultValue(0)]
        [TypeConverter(typeof(PercentageTypeConverter))]
        public int Wheel { get; set; }

        [DefaultValue(0)]
        [TypeConverter(typeof(PercentageTypeConverter))]
        public int Float { get; set; }

        [TypeConverter(typeof(OneZeroBooleanTypeConverter))]
        [DefaultValue(0)]
        public bool Buildable { get; set; }
    }
}
