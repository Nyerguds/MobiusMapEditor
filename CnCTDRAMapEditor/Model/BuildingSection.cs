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
using MobiusEditor.Utility;
using System.ComponentModel;

namespace MobiusEditor.Model
{
    /// <summary>
    /// This class is used to read Red Alert rules.ini style building tweaks. It is also
    /// used for C&C95 v1.06c mission tweaks, though only "Capturable" exists there.
    /// </summary>
    public class BuildingSection
    {
        [DefaultValue(0)]
        public int Power { get; set; }

        [DefaultValue(0)]
        public int Storage { get; set; }

        [DefaultValue(null)]
        public string Name { get; set; }

        [DefaultValue(null)]
        public string Image { get; set; }

        [TypeConverter(typeof(OneZeroBooleanTypeConverter))]
        [DefaultValue(0)]
        public bool Bib { get; set; }

        [TypeConverter(typeof(OneZeroBooleanTypeConverter))]
        [DefaultValue(0)]
        public bool Capturable { get; set; }
    }
}
