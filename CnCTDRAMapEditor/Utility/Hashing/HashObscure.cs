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
using System.Text;

namespace MobiusEditor.Utility.Hashing
{
    public class HashObscure : HashMethod
    {
        private const UInt32 OBSCURE_MAGIC_NUM = 0x516150;

        public override UInt32 GetNameIdCorrectCase(String name)
        {
            return GetNameIdCorrectCase(Encoding.ASCII.GetBytes(name));
        }

        public override UInt32 GetNameIdCorrectCase(Byte[] data)
        {
            if (data.Length < 7)
            {
                Byte[] values2 = new Byte[7];
                Array.Copy(data, values2, data.Length);
                data = values2;
            }
            Byte v8 = data[0]; // [sp+1Ch] [bp-104h]@1
            Byte v9 = data[1]; // [sp+1Dh] [bp-103h]@3
            Byte v10 = data[3]; // [sp+1Fh] [bp-101h]@3
            Byte v11 = data[4]; // [sp+20h] [bp-100h]@3
            Byte v12 = data[5]; // [sp+21h] [bp-FFh]@3
            Byte v13 = data[6]; // [sp+22h] [bp-FEh]@3
            Int64 v3 = v13 + 0xA * (v12 + 0xA * (v11 + 0xA * (v10 + 0xA * (v9 + 0xA * v8)))) - 0x516150;
            return (UInt32)v3;
        }

        public override String GetDisplayName()
        {
            return "Poor Man's (SETUP.MIX)";
        }

        public override String GetSimpleName()
        {
            return "Obscure";
        }
    }
}
