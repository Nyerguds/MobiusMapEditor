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
    public class HashRol1 : Rol
    {
        public override UInt32 GetNameIdCorrectCase(String name)
        {
            return GetNameId(name, 1);
        }
        
        public override UInt32 GetNameIdCorrectCase(Byte[] data)
        {
            return GetNameId(data, 1);
        }

        public override String GetDisplayName()
        {
            return "ROL (TD/RA)";
        }

        public override String GetSimpleName()
        {
            return "ROL";
        }
    }


    public class HashRol3 : Rol
    {
        public override UInt32 GetNameIdCorrectCase(String name)
        {
            return GetNameId(name, 3);
        }

        public override UInt32 GetNameIdCorrectCase(Byte[] data)
        {
            return GetNameId(data, 3);
        }

        public override String GetDisplayName()
        {
            return "ROL3 (setup TS/RA2/...)";
        }

        public override String GetSimpleName()
        {
            return "ROL3";
        }
    }

    public abstract class Rol
    {
        public UInt32 GetNameId(String name)
        {
            if (name == null)
                name = String.Empty;
            return GetNameIdCorrectCase(this.NeedsUpperCase ? name.ToUpperInvariant() : name);
        }

        /// <summary>
        /// The hashing method. Assumes that the input is already formatted to the correct case according to NeedsUpperCase.
        /// </summary>
        /// <param name="name">String to hash.</param>
        /// <returns>The hashed value.</returns>
        public abstract UInt32 GetNameIdCorrectCase(String name);

        /// <summary>
        /// The hashing method. Assumes that the input is already formatted to the correct case according to NeedsUpperCase.
        /// </summary>
        /// <param name="name">String to hash, as byte array.</param>
        /// <returns>The hashed value.</returns>
        public abstract UInt32 GetNameIdCorrectCase(Byte[] data);

        /// <summary>
        /// Returns the display name of the hashing method.
        /// </summary>
        /// <returns>The display name of the hashing method.</returns>
        public abstract String GetDisplayName();

        /// <summary>
        /// Returns the short name of the hashing method.
        /// </summary>
        /// <returns>The short name of the hashing method.</returns>
        public abstract String GetSimpleName();

        /// <summary>
        /// Allows supporting methods that are not case insensitive.
        /// </summary>
        public virtual Boolean NeedsUpperCase
        {
            get { return true; }
        }

        protected UInt32 GetNameId(String name, Int32 rot)
        {
            return GetNameId(Encoding.ASCII.GetBytes(name), rot);
        }
        
        protected UInt32 GetNameId(Byte[] values, Int32 rot)
        {
            Int32 i = 0;
            UInt32 id = 0;
            Int32 len = values.Length;          // length of the filename
            while (i < len)
            {
                // get next uint32 chunk
                UInt32 buffer = this.GetUInt32FromBuffer(values, len, ref i);
                if (i <= len)
                    id = RotateLeft(id, rot) + buffer;
                else // Known quirk: final one only does ROL-1
                    id = RotateLeft(id, 1) + buffer;
            }
            return id;
        }

        protected UInt32 GetUInt32FromBuffer(Byte[] values, Int32 length, ref Int32 index)
        {
            UInt32 a = 0;
            for (Int32 i = 0; i < 4; ++i)
            {
                a >>= 8;
                if (index < length)
                    a += ((UInt32)values[index] << 24);
                index++;
            }
            return a;
        }

        public static UInt32 RotateLeft(UInt32 value, Int32 count)
        {
            return (value << count) | (value >> (32 - count));
        }

        public static UInt32 RotateRight(UInt32 value, Int32 count)
        {
            return (value >> count) | (value << (32 - count));
        }
    }
}
