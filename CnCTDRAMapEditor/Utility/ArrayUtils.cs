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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MobiusEditor.Utility
{
    public static class ArrayUtils
    {

        public static T[][] SwapDimensions<T>(T[][] original)
        {
            Int32 origHeight = original.Length;
            if (origHeight == 0)
                return new T[0][];
            // Since this is for images, it is assumed that the array is a perfectly rectangular matrix
            Int32 origWidth = original[0].Length;

            T[][] swapped = new T[origWidth][];
            for (Int32 newHeight = 0; newHeight < origWidth; ++newHeight)
            {
                swapped[newHeight] = new T[origHeight];
                for (Int32 newWidth = 0; newWidth < origHeight; ++newWidth)
                    swapped[newHeight][newWidth] = original[newWidth][newHeight];
            }
            return swapped;
        }

        public static Boolean ArraysAreEqual<T>(T[] arr1, T[] arr2) where T : IEquatable<T>
        {
            // There's probably a Linq version of this though... Probably .All() or something.
            // But this is with simple arrays.
            if (arr1 == null && arr2 == null)
                return true;
            if (arr1 == null || arr2 == null)
                return false;
            Int32 arr1len = arr1.Length;
            if (arr1len != arr2.Length)
                return false;
            for (Int32 i = 0; i < arr1len; ++i)
                if (!arr1[i].Equals(arr2[i]))
                    return false;
            return true;
        }

        /// <summary>
        /// Creates and returns a new array, containing the contents of all the given arrays, in the given order.
        /// </summary>
        /// <typeparam name="T">Type of the arrays</typeparam>
        /// <param name="arrays">Arrays to join together.</param>
        /// <returns>A new array containing the contents of all given arrays, joined together.</returns>
        public static T[] MergeArrays<T>(params T[][] arrays)
        {
            Int32 length = 0;
            Int32 nrOfArrays = arrays.Length;
            for (Int32 i = 0; i < nrOfArrays; ++i)
            {
                T[] array = arrays[i];
                if (array != null)
                    length += array.Length;
            }
            T[] result = new T[length];
            Int32 copyIndex = 0;
            for (Int32 i = 0; i < nrOfArrays; ++i)
            {
                T[] array = arrays[i];
                if (array == null)
                    continue;
                array.CopyTo(result, copyIndex);
                copyIndex += array.Length;
            }
            return result;
        }

        public static T[] CloneArray<T>(T[] array)
        {
            Int32 len = array.Length;
            T[] copy = new T[len];
            Array.Copy(array, copy, len);
            return copy;
        }

        public static UInt64 ReadIntFromByteArray(Byte[] data, Int32 startIndex, Int32 bytes, Boolean littleEndian)
        {
            Int32 lastByte = bytes - 1;
            if (data.Length < startIndex + bytes)
                throw new ArgumentOutOfRangeException("startIndex", "Data array is too small to read a " + bytes + "-byte value at offset " + startIndex + ".");
            UInt64 value = 0;
            for (Int32 index = 0; index < bytes; ++index)
            {
                Int32 offs = startIndex + (littleEndian ? index : lastByte - index);
                // "index << 3" is "index * 8"
                value |= ((UInt64)data[offs]) << (index << 3);
            }
            return value;
        }

        #region ReadIntFromByteArray shortcuts
        public static Int16 ReadInt16FromByteArrayLe(Byte[] data, Int32 startIndex)
        {
            return (Int16)ReadIntFromByteArray(data, startIndex, 2, true);
        }

        public static Int16 ReadInt16FromByteArrayBe(Byte[] data, Int32 startIndex)
        {
            return (Int16)ReadIntFromByteArray(data, startIndex, 2, false);
        }

        public static UInt16 ReadUInt16FromByteArrayLe(Byte[] data, Int32 startIndex)
        {
            return (UInt16)ReadIntFromByteArray(data, startIndex, 2, true);
        }

        public static UInt16 ReadUInt16FromByteArrayBe(Byte[] data, Int32 startIndex)
        {
            return (UInt16)ReadIntFromByteArray(data, startIndex, 2, false);
        }

        public static Int32 ReadInt32FromByteArrayLe(Byte[] data, Int32 startIndex)
        {
            return (Int32)ReadIntFromByteArray(data, startIndex, 4, true);
        }

        public static Int32 ReadInt32FromByteArrayBe(Byte[] data, Int32 startIndex)
        {
            return (Int32)ReadIntFromByteArray(data, startIndex, 4, false);
        }

        public static UInt32 ReadUInt32FromByteArrayLe(Byte[] data, Int32 startIndex)
        {
            return (UInt32)ReadIntFromByteArray(data, startIndex, 4, true);
        }

        public static UInt32 ReadUInt32FromByteArrayBe(Byte[] data, Int32 startIndex)
        {
            return (UInt32)ReadIntFromByteArray(data, startIndex, 4, false);
        }

        public static Int64 ReadInt64FromByteArrayLe(Byte[] data, Int32 startIndex)
        {
            return (Int64)ReadIntFromByteArray(data, startIndex, 8, true);
        }

        public static Int64 ReadInt64FromByteArrayBe(Byte[] data, Int32 startIndex)
        {
            return (Int64)ReadIntFromByteArray(data, startIndex, 8, true);
        }

        public static UInt64 ReadUInt64FromByteArrayLe(Byte[] data, Int32 startIndex)
        {
            return (UInt64)ReadIntFromByteArray(data, startIndex, 8, true);
        }

        public static UInt64 ReadUInt64FromByteArrayBe(Byte[] data, Int32 startIndex)
        {
            return (UInt64)ReadIntFromByteArray(data, startIndex, 8, true);
        }
        #endregion

        public static void WriteIntToByteArray(Byte[] data, Int32 startIndex, Int32 bytes, Boolean littleEndian, UInt64 value)
        {
            Int32 lastByte = bytes - 1;
            if (data.Length < startIndex + bytes)
                throw new ArgumentOutOfRangeException("startIndex", "Data array is too small to write a " + bytes + "-byte value at offset " + startIndex + ".");
            for (Int32 index = 0; index < bytes; ++index)
            {
                Int32 offs = startIndex + (littleEndian ? index : lastByte - index);
                // "index << 3" is "index * 8"
                data[offs] = (Byte)(value >> (index << 3) & 0xFF);
            }
        }

        #region WriteIntToByteArray shortcuts
        public static void WriteInt16ToByteArrayLe(Byte[] data, Int32 startIndex, Int16 value)
        {
            WriteIntToByteArray(data, startIndex, 2, true, (UInt64)value);
        }

        public static void WriteInt16ToByteArrayBe(Byte[] data, Int32 startIndex, Int16 value)
        {
            WriteIntToByteArray(data, startIndex, 2, false, (UInt64)value);
        }

        public static void WriteUInt16ToByteArrayLe(Byte[] data, Int32 startIndex, UInt16 value)
        {
            WriteIntToByteArray(data, startIndex, 2, true, value);
        }

        public static void WriteUInt16ToByteArrayBe(Byte[] data, Int32 startIndex, UInt16 value)
        {
            WriteIntToByteArray(data, startIndex, 2, false, value);
        }

        public static void WriteInt32ToByteArrayLe(Byte[] data, Int32 startIndex, Int32 value)
        {
            WriteIntToByteArray(data, startIndex, 4, true, (UInt64)value);
        }

        public static void WriteInt32ToByteArrayBe(Byte[] data, Int32 startIndex, Int32 value)
        {
            WriteIntToByteArray(data, startIndex, 4, false, (UInt64)value);
        }

        public static void WriteUInt32ToByteArrayLe(Byte[] data, Int32 startIndex, UInt32 value)
        {
            WriteIntToByteArray(data, startIndex, 4, true, value);
        }

        public static void WriteUInt32ToByteArrayBe(Byte[] data, Int32 startIndex, UInt32 value)
        {
            WriteIntToByteArray(data, startIndex, 4, false, value);
        }

        public static void WriteInt64ToByteArrayLe(Byte[] data, Int32 startIndex, Int64 value)
        {
            WriteIntToByteArray(data, startIndex, 8, true, (UInt64)value);
        }

        public static void WriteInt64ToByteArrayBe(Byte[] data, Int32 startIndex, Int64 value)
        {
            WriteIntToByteArray(data, startIndex, 8, true, (UInt64)value);
        }

        public static void WriteUInt64ToByteArrayLe(Byte[] data, Int32 startIndex, UInt64 value)
        {
            WriteIntToByteArray(data, startIndex, 8, true, value);
        }

        public static void WriteUInt64ToByteArrayBe(Byte[] data, Int32 startIndex, UInt64 value)
        {
            WriteIntToByteArray(data, startIndex, 8, true, value);
        }
        #endregion

        public static Int32 ReadBitsFromByteArray(Byte[] dataArr, ref Int32 bitIndex, Int32 codeLen, Int32 bufferInEnd)
        {
            Int32 intCode = 0;
            Int32 byteIndex = bitIndex / 8;
            Int32 ignoreBitsAtIndex = bitIndex % 8;
            Int32 bitsToReadAtIndex = Math.Min(codeLen, 8 - ignoreBitsAtIndex);
            Int32 totalUsedBits = 0;
            while (codeLen > 0)
            {
                if (byteIndex >= bufferInEnd)
                    return -1;

                Int32 toAdd = (dataArr[byteIndex] >> ignoreBitsAtIndex) & ((1 << bitsToReadAtIndex) - 1);
                intCode |= (toAdd << totalUsedBits);
                totalUsedBits += bitsToReadAtIndex;
                codeLen -= bitsToReadAtIndex;
                bitsToReadAtIndex = Math.Min(codeLen, 8);
                ignoreBitsAtIndex = 0;
                byteIndex++;
            }
            bitIndex += totalUsedBits;
            return intCode;
        }

        public static void WriteBitsToByteArray(Byte[] dataArr, Int32 bitIndex, Int32 codeLen, Int32 intCode)
        {
            Int32 byteIndex = bitIndex / 8;
            Int32 usedBitsAtIndex = bitIndex % 8;
            Int32 bitsToWriteAtIndex = Math.Min(codeLen, 8 - usedBitsAtIndex);
            while (codeLen > 0)
            {
                Int32 codeToWrite = (intCode & ((1 << bitsToWriteAtIndex) - 1)) << usedBitsAtIndex;
                intCode = intCode >> bitsToWriteAtIndex;
                dataArr[byteIndex] |= (Byte) codeToWrite;
                codeLen -= bitsToWriteAtIndex;
                bitsToWriteAtIndex = Math.Min(codeLen, 8);
                usedBitsAtIndex = 0;
                byteIndex++;
            }
        }

        public static T CloneStruct<T>(T obj) where T : struct
        {
            Endianness e = BitConverter.IsLittleEndian ? Endianness.LittleEndian : Endianness.BigEndian;
            return ReadStructFromByteArray<T>(StructToByteArray(obj, e), 0, e);
        }

        private static void AdjustEndianness(Type type, Byte[] data, Endianness endianness, Int32 startOffset)
        {
            // nothing to change => return
            if (BitConverter.IsLittleEndian == (endianness == Endianness.LittleEndian))
                return;
            FieldInfo[] fields = type.GetFields();
            Int32 fieldsLen = fields.Length;
            for (Int32 i = 0; i < fieldsLen; ++i)
            {
                FieldInfo field = fields[i];
                Type fieldType = field.FieldType;
                if (field.IsStatic)
                    // don't process static fields
                    continue;
                if (fieldType == typeof (String))
                    // don't swap bytes for strings
                    continue;
                Int32 offset = Marshal.OffsetOf(type, field.Name).ToInt32();
                // handle enums
                if (fieldType.IsEnum)
                    fieldType = Enum.GetUnderlyingType(fieldType);
                // check for sub-fields to recurse if necessary
                FieldInfo[] subFields = fieldType.GetFields().Where(subField => subField.IsStatic == false).ToArray();
                Int32 effectiveOffset = startOffset + offset;
                if (subFields.Length == 0)
                    Array.Reverse(data, effectiveOffset, Marshal.SizeOf(fieldType));
                else // recurse
                    AdjustEndianness(fieldType, data, endianness, effectiveOffset);
            }
        }

        public static T StructFromByteArray<T>(Byte[] rawData, Endianness endianness) where T : struct
        {
            return ReadStructFromByteArray<T>(rawData, 0, endianness);
        }

        public static T ReadStructFromByteArray<T>(Byte[] rawData, Int32 offset, Endianness endianness) where T : struct
        {
            Type tType = typeof (T);
            Int32 size = Marshal.SizeOf(tType);
            if (size + offset > rawData.Length)
                throw new IndexOutOfRangeException("Array is too small to get the requested struct!");
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);
                // Adjust array to preferred endianness
                AdjustEndianness(tType, rawData, endianness, offset);
                Marshal.Copy(rawData, offset, ptr, size);
                // Revert array to original data order
                AdjustEndianness(tType, rawData, endianness, offset);
                Object obj = Marshal.PtrToStructure(ptr, tType);
                return (T) obj;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        public static Byte[] StructToByteArray<T>(T obj, Endianness endianness) where T : struct
        {
            Int32 size = Marshal.SizeOf(typeof (T));
            Byte[] target = new Byte[size];
            WriteStructToByteArray(obj, target, 0, endianness);
            return target;
        }

        public static void WriteStructToByteArray<T>(T obj, Byte[] target, Int32 index, Endianness endianness) where T : struct
        {
            Type tType = typeof (T);
            Int32 size = Marshal.SizeOf(tType);
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(obj, ptr, true);
                Marshal.Copy(ptr, target, index, size);
                AdjustEndianness(typeof (T), target, endianness, 0);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }
    }

    public enum Endianness
    {
        BigEndian,
        LittleEndian
    }
}
