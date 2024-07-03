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
using System.Drawing;

namespace MobiusEditor.Interface
{
    /// <summary>
    /// Techno objects.
    /// </summary>
    public interface ITechno
    {
        /// <summary>True if this is a preview dummy object.</summary>
        bool IsPreview { get; set; }
        /// <summary>House that owns this object.</summary>
        HouseType House { get; set; }
        /// <summary>The Type, as TechnoType.</summary>
        ITechnoType TechnoType { get; }
        /// <summary>Strength of the object, on a scale of 1 to 256.</summary>
        int Strength { get; set; }
        /// <summary>Trigger attached to this object.</summary>
        string Trigger { get; set; }
        /// <summary>Direction that the object is facing.</summary>
        DirectionType Direction { get; set; }
        /// <summary>Color modifier for drawing this object on the map. Generally used to tweak the alpha.</summary>
        Color Tint { get; set; }
    }
}
