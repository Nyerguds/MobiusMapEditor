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
using LarchenkoCRC32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiusEditor.Utility.Hashing
{
    public class HashCrc32 : HashMethod
    {
        public override UInt32 GetNameIdCorrectCase(String name)
        {
            Byte[] data = Encoding.ASCII.GetBytes(name);
            return GetNameIdCorrectCase(data);
        }

        public override UInt32 GetNameIdCorrectCase(Byte[] data)
        {
            int l1 = data.Length;
            // Fill buffer up to next multiple of 4 bytes
            if ((l1 & 3) != 0)
            {
                Int32 l2 = (l1 + 3) & ~3;
                Byte[] data2 = new Byte[l2];
                Array.Copy(data, 0, data2, 0, l1);
                Int32 a = l1 >> 2;
                data2[l1] = (byte)(l1 - (a << 2));
                for (int i = l1 + 1; i < l2; i++)
                {
                    data2[i] = data2[a << 2];
                }
                data = data2;
            }
            return ParallelCRC.Compute(new ArraySegment<Byte>(data));
        }


        public override String GetDisplayName()
        {
            return "CRC32 (TS/RA2)";
        }

        public override String GetSimpleName()
        {
            return "CRC32";
        }
    }
}
