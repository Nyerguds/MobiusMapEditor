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
            int origHeight = original.Length;
            if (origHeight == 0)
                return new T[0][];
            // Since this is for images, it is assumed that the array is a perfectly rectangular matrix
            int origWidth = original[0].Length;

            T[][] swapped = new T[origWidth][];
            for (int newHeight = 0; newHeight < origWidth; ++newHeight)
            {
                swapped[newHeight] = new T[origHeight];
                for (int newWidth = 0; newWidth < origHeight; ++newWidth)
                    swapped[newHeight][newWidth] = original[newWidth][newHeight];
            }
            return swapped;
        }

        public static bool ArraysAreEqual<T>(T[] arr1, T[] arr2) where T : IEquatable<T>
        {
            // There's probably a Linq version of this though... Probably .All() or something.
            // But this is with simple arrays.
            if (arr1 == null && arr2 == null)
                return true;
            if (arr1 == null || arr2 == null)
                return false;
            int arr1len = arr1.Length;
            if (arr1len != arr2.Length)
                return false;
            for (int i = 0; i < arr1len; ++i)
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
            int length = 0;
            int nrOfArrays = arrays.Length;
            for (int i = 0; i < nrOfArrays; ++i)
            {
                T[] array = arrays[i];
                if (array != null)
                    length += array.Length;
            }
            T[] result = new T[length];
            int copyIndex = 0;
            for (int i = 0; i < nrOfArrays; ++i)
            {
                T[] array = arrays[i];
                if (array == null)
                    continue;
                array.CopyTo(result, copyIndex);
                copyIndex += array.Length;
            }
            return result;
        }

        public static T[] CloneArray<T>(this T[] array)
        {
            int len = array.Length;
            T[] copy = new T[len];
            Array.Copy(array, copy, len);
            return copy;
        }

        public static ulong ReadIntFromByteArray(byte[] data, int startIndex, int bytes, bool littleEndian)
        {
            int lastByte = bytes - 1;
            if (data.Length < startIndex + bytes)
                throw new ArgumentOutOfRangeException("startIndex", "Data array is too small to read a " + bytes + "-byte value at offset " + startIndex + ".");
            ulong value = 0;
            for (int index = 0; index < bytes; ++index)
            {
                int offs = startIndex + (littleEndian ? index : lastByte - index);
                // "index << 3" is "index * 8"
                value |= ((ulong)data[offs]) << (index << 3);
            }
            return value;
        }

        #region ReadIntFromByteArray shortcuts
        public static short ReadInt16FromByteArrayLe(byte[] data, int startIndex)
        {
            return (short)ReadIntFromByteArray(data, startIndex, 2, true);
        }

        public static short ReadInt16FromByteArrayBe(byte[] data, int startIndex)
        {
            return (short)ReadIntFromByteArray(data, startIndex, 2, false);
        }

        public static ushort ReadUInt16FromByteArrayLe(byte[] data, int startIndex)
        {
            return (ushort)ReadIntFromByteArray(data, startIndex, 2, true);
        }

        public static ushort ReadUInt16FromByteArrayBe(byte[] data, int startIndex)
        {
            return (ushort)ReadIntFromByteArray(data, startIndex, 2, false);
        }

        public static int ReadInt32FromByteArrayLe(byte[] data, int startIndex)
        {
            return (int)ReadIntFromByteArray(data, startIndex, 4, true);
        }

        public static int ReadInt32FromByteArrayBe(byte[] data, int startIndex)
        {
            return (int)ReadIntFromByteArray(data, startIndex, 4, false);
        }

        public static uint ReadUInt32FromByteArrayLe(byte[] data, int startIndex)
        {
            return (uint)ReadIntFromByteArray(data, startIndex, 4, true);
        }

        public static uint ReadUInt32FromByteArrayBe(byte[] data, int startIndex)
        {
            return (uint)ReadIntFromByteArray(data, startIndex, 4, false);
        }

        public static long ReadInt64FromByteArrayLe(byte[] data, int startIndex)
        {
            return (long)ReadIntFromByteArray(data, startIndex, 8, true);
        }

        public static long ReadInt64FromByteArrayBe(byte[] data, int startIndex)
        {
            return (long)ReadIntFromByteArray(data, startIndex, 8, true);
        }

        public static ulong ReadUInt64FromByteArrayLe(byte[] data, int startIndex)
        {
            return ReadIntFromByteArray(data, startIndex, 8, true);
        }

        public static ulong ReadUInt64FromByteArrayBe(byte[] data, int startIndex)
        {
            return ReadIntFromByteArray(data, startIndex, 8, true);
        }
        #endregion

        public static void WriteIntToByteArray(byte[] data, int startIndex, int bytes, bool littleEndian, ulong value)
        {
            int lastByte = bytes - 1;
            if (data.Length < startIndex + bytes)
                throw new ArgumentOutOfRangeException("startIndex", "Data array is too small to write a " + bytes + "-byte value at offset " + startIndex + ".");
            for (int index = 0; index < bytes; ++index)
            {
                int offs = startIndex + (littleEndian ? index : lastByte - index);
                // "index << 3" is "index * 8"
                data[offs] = (byte)(value >> (index << 3) & 0xFF);
            }
        }

        #region WriteIntToByteArray shortcuts
        public static void WriteInt16ToByteArrayLe(byte[] data, int startIndex, short value)
        {
            WriteIntToByteArray(data, startIndex, 2, true, (ulong)value);
        }

        public static void WriteInt16ToByteArrayBe(byte[] data, int startIndex, short value)
        {
            WriteIntToByteArray(data, startIndex, 2, false, (ulong)value);
        }

        public static void WriteUInt16ToByteArrayLe(byte[] data, int startIndex, ushort value)
        {
            WriteIntToByteArray(data, startIndex, 2, true, value);
        }

        public static void WriteUInt16ToByteArrayBe(byte[] data, int startIndex, ushort value)
        {
            WriteIntToByteArray(data, startIndex, 2, false, value);
        }

        public static void WriteInt32ToByteArrayLe(byte[] data, int startIndex, int value)
        {
            WriteIntToByteArray(data, startIndex, 4, true, (ulong)value);
        }

        public static void WriteInt32ToByteArrayBe(byte[] data, int startIndex, int value)
        {
            WriteIntToByteArray(data, startIndex, 4, false, (ulong)value);
        }

        public static void WriteUInt32ToByteArrayLe(byte[] data, int startIndex, uint value)
        {
            WriteIntToByteArray(data, startIndex, 4, true, value);
        }

        public static void WriteUInt32ToByteArrayBe(byte[] data, int startIndex, uint value)
        {
            WriteIntToByteArray(data, startIndex, 4, false, value);
        }

        public static void WriteInt64ToByteArrayLe(byte[] data, int startIndex, long value)
        {
            WriteIntToByteArray(data, startIndex, 8, true, (ulong)value);
        }

        public static void WriteInt64ToByteArrayBe(byte[] data, int startIndex, long value)
        {
            WriteIntToByteArray(data, startIndex, 8, true, (ulong)value);
        }

        public static void WriteUInt64ToByteArrayLe(byte[] data, int startIndex, ulong value)
        {
            WriteIntToByteArray(data, startIndex, 8, true, value);
        }

        public static void WriteUInt64ToByteArrayBe(byte[] data, int startIndex, ulong value)
        {
            WriteIntToByteArray(data, startIndex, 8, true, value);
        }
        #endregion

        public static int ReadBitsFromByteArray(byte[] dataArr, ref int bitIndex, int codeLen, int bufferInEnd)
        {
            int intCode = 0;
            int byteIndex = bitIndex / 8;
            int ignoreBitsAtIndex = bitIndex % 8;
            int bitsToReadAtIndex = Math.Min(codeLen, 8 - ignoreBitsAtIndex);
            int totalUsedBits = 0;
            while (codeLen > 0)
            {
                if (byteIndex >= bufferInEnd)
                    return -1;

                int toAdd = (dataArr[byteIndex] >> ignoreBitsAtIndex) & ((1 << bitsToReadAtIndex) - 1);
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

        public static void WriteBitsToByteArray(byte[] dataArr, int bitIndex, int codeLen, int intCode)
        {
            int byteIndex = bitIndex / 8;
            int usedBitsAtIndex = bitIndex % 8;
            int bitsToWriteAtIndex = Math.Min(codeLen, 8 - usedBitsAtIndex);
            while (codeLen > 0)
            {
                int codeToWrite = (intCode & ((1 << bitsToWriteAtIndex) - 1)) << usedBitsAtIndex;
                intCode = intCode >> bitsToWriteAtIndex;
                dataArr[byteIndex] |= (byte) codeToWrite;
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

        private static void AdjustEndianness(Type type, byte[] data, Endianness endianness, int startOffset)
        {
            // nothing to change => return
            if (BitConverter.IsLittleEndian == (endianness == Endianness.LittleEndian))
                return;
            FieldInfo[] fields = type.GetFields();
            int fieldsLen = fields.Length;
            for (int i = 0; i < fieldsLen; ++i)
            {
                FieldInfo field = fields[i];
                Type fieldType = field.FieldType;
                if (field.IsStatic)
                    // don't process static fields
                    continue;
                if (fieldType == typeof(string))
                    // don't swap bytes for strings
                    continue;
                int offset = Marshal.OffsetOf(type, field.Name).ToInt32();
                // handle enums
                if (fieldType.IsEnum)
                    fieldType = Enum.GetUnderlyingType(fieldType);
                // check for sub-fields to recurse if necessary
                FieldInfo[] subFields = fieldType.GetFields().Where(subField => subField.IsStatic == false).ToArray();
                int effectiveOffset = startOffset + offset;
                if (subFields.Length == 0)
                    Array.Reverse(data, effectiveOffset, Marshal.SizeOf(fieldType));
                else // recurse
                    AdjustEndianness(fieldType, data, endianness, effectiveOffset);
            }
        }

        public static T StructFromByteArray<T>(byte[] rawData, Endianness endianness) where T : struct
        {
            return ReadStructFromByteArray<T>(rawData, 0, endianness);
        }

        public static T ReadStructFromByteArray<T>(byte[] rawData, int offset, Endianness endianness) where T : struct
        {
            Type tType = typeof (T);
            int size = Marshal.SizeOf(tType);
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
                object obj = Marshal.PtrToStructure(ptr, tType);
                return (T) obj;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        public static byte[] StructToByteArray<T>(T obj, Endianness endianness) where T : struct
        {
            int size = Marshal.SizeOf(typeof (T));
            byte[] target = new byte[size];
            WriteStructToByteArray(obj, target, 0, endianness);
            return target;
        }

        public static void WriteStructToByteArray<T>(T obj, byte[] target, int index, Endianness endianness) where T : struct
        {
            Type tType = typeof (T);
            int size = Marshal.SizeOf(tType);
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
