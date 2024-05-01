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
    /// <summary>ROL hashing algorithm (TD/RA)</summary>
    public class HashRol1 : HashRol
    {
        public override string DisplayName { get { return "ROL (TD/RA)"; } }
        public override string SimpleName{ get { return "ROL1"; } }

        public override uint GetNameIdCorrectCase(string name)
        {
            return GetNameId(name, 1);
        }

        public override uint GetNameIdCorrectCase(byte[] data)
        {
            return GetNameId(data, 1);
        }
    }

    public class HashRol3 : HashRol
    {
        public override string DisplayName { get { return "ROL3 (setup TS/RA2/...)"; } }
        public override string SimpleName { get { return "ROL3"; } }

        public override uint GetNameIdCorrectCase(string name)
        {
            return GetNameId(name, 3);
        }

        public override uint GetNameIdCorrectCase(byte[] data)
        {
            return GetNameId(data, 3);
        }
    }

    public abstract class HashRol : HashMethod
    {

        protected uint GetNameId(string name, int rot)
        {
            return GetNameId(Encoding.ASCII.GetBytes(name), rot);
        }

        protected uint GetNameId(byte[] values, int rot)
        {
            int i = 0;
            uint id = 0;
            // length of the filename
            int len = values.Length;
            while (i < len)
            {
                // get next uint32 chunk
                uint buffer = this.GetUInt32FromBuffer(values, len, ref i);
                id = BitFunctions.RotateLeft(id, i <= len ? rot : 1) + buffer;
            }
            return id;
        }

        protected uint GetUInt32FromBuffer(byte[] values, int length, ref int index)
        {
            uint a = 0;
            for (int i = 0; i < 4; ++i)
            {
                a >>= 8;
                if (index < length)
                    a += ((uint)values[index] << 24);
                index++;
            }
            return a;
        }
    }
}
