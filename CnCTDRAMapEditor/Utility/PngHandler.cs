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
using MobiusEditor.Utility.Hashing;
using System;
using System.Text;

namespace MobiusEditor.Utility
{
    /// <summary>
    /// This class allows manipulating PNG files per chunk.
    /// </summary>
    public static class PngHandler
    {
        /// <summary>An array containing the identifying bytes required at the start of a PNG image file.</summary>
        private static readonly byte[] PNG_IDENTIFIER = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        /// <summary>Returns an array containing the identifying bytes required at the start of a PNG image file.</summary>
        /// <returns>An array containing the identifying bytes required at the start of a PNG image file.</returns>
        public static byte[] GetPngIdentifier() { return PNG_IDENTIFIER.CloneArray(); }

        /// <summary>The contents of the IDAT chunk for a 1x1 8-bit indexed image with pixel value 0.</summary>
        private static readonly byte[] PNG_BLANK = { 0x08, 0xD7, 0x63, 0x60, 0x00, 0x00, 0x00, 0x02, 0x00, 0x01 };
        /// <summary>
        /// Returns the contents of the IDAT chunk for a 1x1 8-bit indexed image with pixel value 0.
        /// Used as dummy for generating custom-sized palettes.
        /// </summary>
        /// <returns>The contents of the IDAT chunk for a 1x1 8-bit indexed image with pixel value 0.</returns>
        public static byte[] GetBlankPngIdatContents() { return PNG_BLANK.CloneArray(); }

        /// <summary>
        /// Checks the start of a byte array to see if it matches the identifying bytes required at the start of a PNG image file.
        /// This will not do a full integrity check on chunk CRCs.
        /// </summary>
        /// <param name="data">The data to check.</param>
        /// <returns>True if the start of the data matches the PNG identifier.</returns>
        public static bool IsPng(byte[] data)
        {
            int idLen = PNG_IDENTIFIER.Length;
            if (data.Length < PNG_IDENTIFIER.Length)
                return false;
            for (int i = 0; i < idLen; ++i)
                if (data[i] != PNG_IDENTIFIER[i])
                    return false;
            return true;
        }

        /// <summary>
        /// Finds the start of a png chunk. This assumes the image is already identified as PNG.
        /// It does not go over the first 8 bytes, but starts at the start of the header chunk.
        /// </summary>
        /// <param name="data">The bytes of the png image.</param>
        /// <param name="start">Where to start searching. This value must be a valid chunk start offset. If 0, this starts at the Header chunk.</param>
        /// <param name="chunkName">The name of the chunk to find.</param>
        /// <returns>The index of the start of the png chunk, or -1 if the chunk was not found.</returns>
        public static int FindPngChunk(byte[] data, int start, string chunkName)
        {
            if (data == null)
                throw new ArgumentNullException("data", "No data given.");
            if (chunkName == null)
                throw new ArgumentNullException("chunkName", "No chunk name given.");
            if (!IsPng(data))
                throw new ArgumentException("Data does not contain a png header.", "data");
            byte[] chunkNamebytes = GetChunkNameBytes(chunkName);
            int offset = Math.Max(start, PNG_IDENTIFIER.Length);
            int end = data.Length;
            // continue until either the end is reached, or there is not enough space behind it for reading a new chunk
            while (offset + 12 <= end)
            {
                int nameStart = offset + 4;
                bool isMatch = true;
                for (int i = 0; i < 4; ++i)
                {
                    if (chunkNamebytes[i] != data[nameStart + i])
                    {
                        isMatch = false;
                        break;
                    }
                }
                int chunkLength = GetPngChunkDataLength(data, offset);
                if (isMatch)
                {
                    // For efficiency, only check checksum on found chunk.
                    if (!PngChecksumMatches(data, offset, chunkLength))
                        throw new ArgumentException(String.Format("Incorrect checksum on chunk data at {0}", offset), "data");
                    return offset;
                }
                // chunk size + chunk header + chunk checksum = 12 bytes.
                offset += 12 + chunkLength;
            }
            return -1;
        }

        /// <summary>
        /// Creates a png data chunk and returns it as byte array.
        /// </summary>
        /// <param name="chunkName">4-character chunk name.</param>
        /// <param name="chunkData">Data to write into the new chunk.</param>
        /// <returns>The new offset after writing the new chunk. Always equal to the offset plus the length of chunk data plus 12.</returns>
        public static byte[] MakePngChunk(string chunkName, byte[] chunkData)
        {
            byte[] data = new byte[12 + chunkData.Length];
            WritePngChunk(data, 0, chunkName, chunkData);
            return data;
        }

        /// <summary>
        /// Writes a png data chunk.
        /// </summary>
        /// <param name="target">Target array to write into.</param>
        /// <param name="offset">Offset in the array to write the data to.</param>
        /// <param name="chunkName">4-character chunk name.</param>
        /// <param name="chunkData">Data to write into the new chunk.</param>
        /// <returns>The new offset after writing the new chunk. Always equal to the offset plus the length of chunk data plus 12.</returns>
        public static int WritePngChunk(byte[] target, int offset, string chunkName, byte[] chunkData)
        {
            if (offset + chunkData.Length + 12 > target.Length)
                throw new ArgumentException("Data does not fit in target array.", "chunkData");
            byte[] chunkNamebytes = GetChunkNameBytes(chunkName);
            ArrayUtils.WriteInt32ToByteArrayBe(target, offset, chunkData.Length);
            offset += 4;
            int nameOffset = offset;
            Array.Copy(chunkNamebytes, 0, target, offset, 4);
            offset += 4;
            int curLength = chunkData.Length;
            Array.Copy(chunkData, 0, target, offset, curLength);
            offset += curLength;
            uint crcval = CRC.Calculate(target, nameOffset, chunkData.Length + 4);
            ArrayUtils.WriteInt32ToByteArrayBe(target, offset, (int)crcval);
            offset += 4;
            return offset;
        }

        /// <summary>
        /// Returns the length of the data inside a given chunk. This value does not include the additional 12 bytes
        /// for the length value, the chunk ID and the checksum.
        /// </summary>
        /// <param name="data">The PNG file data</param>
        /// <param name="chunkOffset">Offset of the PNG chunk, as found by FindPngChunk.</param>
        /// <returns>The value read from the chunk's length block.</returns>
        public static int GetPngChunkDataLength(byte[] data, int chunkOffset)
        {
            if (chunkOffset + 12 > data.Length)
                throw new IndexOutOfRangeException("Bad chunk size in png image.");
            // Don't want to use BitConverter; then you have to check platform endianness and all that mess.
            //int length = data[offset + 3] + (data[offset + 2] << 8) + (data[offset + 1] << 16) + (data[offset] << 24);
            int length = ArrayUtils.ReadInt32FromByteArrayBe(data, chunkOffset);
            if (length < 0 || chunkOffset + 12 + length > data.Length)
                throw new IndexOutOfRangeException("Bad chunk size in png image.");
            return length;
        }

        /// <summary>
        /// Gets the data from inside a PNG chunk.
        /// </summary>
        /// <param name="data">The PNG file data</param>
        /// <param name="chunkOffset">Offset of the chunk.</param>
        /// <returns>The contents inside the chunk, without the chunk header or CRC footer.</returns>
        public static byte[] GetPngChunkData(byte[] data, int chunkOffset)
        {
            return GetPngChunkData(data, chunkOffset, -1);
        }

        /// <summary>
        /// Gets the data from inside a PNG chunk. This assumes the length was already fetched.
        /// </summary>
        /// <param name="data">The PNG file data</param>
        /// <param name="chunkOffset">Offset of the chunk.</param>
        /// <param name="chunkDataLength">Length of the chunk data, of -1 to auto-fetch using GetPngChunkDataLength. This is 12 bytes less than the full chunk length.</param>
        /// <returns>The contents inside the chunk, without the chunk header or CRC footer.</returns>
        public static byte[] GetPngChunkData(byte[] data, int chunkOffset, int chunkDataLength)
        {
            if (chunkDataLength < 0)
                chunkDataLength = GetPngChunkDataLength(data, chunkOffset);
            if (chunkDataLength == -1)
                return null;
            byte[] chunkData = new byte[chunkDataLength];
            Array.Copy(data, chunkOffset + 8, chunkData, 0, chunkDataLength);
            return chunkData;
        }

        /// <summary>
        /// Checks whether the 4-byte CRC checksum at the end of the chunk matches the contents.
        /// </summary>
        /// <param name="data">The PNG file data</param>
        /// <param name="chunkOffset">Offset of the chunk.</param>
        /// <returns>True if the 4-byte CRC checksum at the end of the chunk matches the contents.</returns>
        public static bool PngChecksumMatches(byte[] data, int chunkOffset)
        {
            return PngChecksumMatches(data, chunkOffset, -1);
        }

        /// <summary>
        /// Checks whether the 4-byte CRC checksum at the end of the chunk matches the contents. This assumes the length was already fetched.
        /// </summary>
        /// <param name="data">The PNG file data</param>
        /// <param name="chunkOffset">Offset of the chunk.</param>
        /// <param name="chunkLength">Length of the chunk, of -1 to auto-fetch using GetPngChunkDataLength.</param>
        /// <returns>True if the 4-byte CRC checksum at the end of the chunk matches the contents.</returns>
        public static bool PngChecksumMatches(byte[] data, int chunkOffset, int chunkLength)
        {
            if (chunkLength < 0)
                chunkLength = GetPngChunkDataLength(data, chunkOffset);
            if (chunkLength == -1)
                return false;
            byte[] checksum = new byte[4];
            Array.Copy(data, chunkOffset + 8 + chunkLength, checksum, 0, 4);
            uint readChecksum = ArrayUtils.ReadUInt32FromByteArrayBe(checksum, 0);
            uint calculatedChecksum = CRC.Calculate(data, chunkOffset + 4, chunkLength + 4);
            return readChecksum == calculatedChecksum;
        }

        private static byte[] GetChunkNameBytes(string chunkName)
        {
            if (chunkName.Length != 4)
                throw new ArgumentException("Chunk name must be exactly 4 characters.", "chunkName");
            for (int i = 0; i < 4; ++i)
            {
                char c = chunkName[i];
                // Only strictly alphabetic values are allowed.
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                    continue;
                throw new ArgumentException("Chunk name must be composed strictly from alphabetic characters.", "chunkName");
            }
            return Encoding.ASCII.GetBytes(chunkName);
        }

    }
}