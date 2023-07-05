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

namespace MobiusEditor.Interface
{
    /// <summary>
    /// Split off so things can be more easily iterable as techno types.
    /// </summary>
    public interface ITechno
    {
        bool IsPreview { get; set; }
        HouseType House { get; set; }
        int Strength { get; set; }
        string Trigger { get; set; }
    }
}
