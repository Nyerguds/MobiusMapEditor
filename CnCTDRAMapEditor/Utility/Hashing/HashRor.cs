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
using System;

namespace MobiusEditor.Utility.Hashing
{
    public class HashRor : HashMethod
    {
        public override string DisplayName { get { return "ROR (Lands of Lore 3)"; } }
        public override string SimpleName { get { return "ROR"; } }

        public override uint GetNameIdCorrectCase(byte[] data)
        {
            uint id = 0;
            for (int i = 0; i < data.Length; ++i)
            {
                uint rotatedValue = BitFunctions.RotateRight(id, 6);
                id = (uint)((data[i] - 48) & 63) + rotatedValue;
            }
            return id;
        }

        public override uint GetNameIdCorrectCase(string name)
        {
            uint id = 0;
            for (int i = 0; i < name.Length; ++i)
            {
                uint rotatedValue = BitFunctions.RotateRight(id, 6);
                id = (uint)((name[i] - 48) & 63) + rotatedValue;
            }
            return id;
        }
    }
}
