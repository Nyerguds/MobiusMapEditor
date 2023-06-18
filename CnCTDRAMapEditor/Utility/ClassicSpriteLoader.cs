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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace MobiusEditor.Utility
{
    public static class ClassicSpriteLoader
    {
        private static readonly byte[] ConvertToEightBit = new byte[64];

        static ClassicSpriteLoader()
        {
            // Build an easy lookup table for this, so no calculations are ever needed for it later.
            for (Int32 i = 0; i < 64; ++i)
            {
                ConvertToEightBit[i] = (byte)Math.Round(i * 255.0 / 63.0, MidpointRounding.ToEven);
            }
        }

        /// <summary>
        /// Retrieves the CPS image.
        /// </summary>
        /// <param name="fileData">Original file data.</param>
        /// <returns>The raw 8-bit linear image data in a 64000 byte array.</returns>
        public static Bitmap LoadCpsFile(Byte[] fileData)
        {
            Byte[] imageData = GetCpsData(fileData, out Color[] palette);
            if (palette == null)
                palette = Enumerable.Range(0, 0x100).Select(i => Color.FromArgb(i, i, i)).ToArray();
            return ImageUtils.BuildImage(imageData, 320, 200, 320, PixelFormat.Format8bppIndexed, palette, null);
        }

        /// <summary>
        /// Retrieves the CPS image data.
        /// </summary>
        /// <param name="fileData">Original file data.</param>
        /// <returns>The raw 8-bit linear image data in a 64000 byte array.</returns>
        /// <exception cref="ArgumentException">Thrown when parsing failed, indicating this is not a valid file of this format.</exception>
        public static Byte[] GetCpsData(Byte[] fileData, out Color[] palette)
        {
            int dataLen = fileData.Length;
            if (dataLen < 10)
                throw new ArgumentException("File is not long enough to be a valid CPS file.", "fileData");
            int fileSize = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0);
            int compression = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 2);
            if (compression < 0 || compression > 4)
                throw new ArgumentException("Unknown compression type " + compression, "fileData");
            // compressions other than 0 and 4 count the full file including size header.
            if (compression == 0 || compression == 4)
                fileSize += 2;
            else
                throw new ArgumentException("LZW and RLE compression are not supported.", "fileData");
            if (fileSize != dataLen)
                throw new ArgumentException("File size in header does not match!", "fileData");
            Int32 bufferSize = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 4);
            Int32 paletteLength = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 8);
            if (bufferSize != 64000)
                throw new ArgumentException("Unknown CPS type.", "fileData");
            if (paletteLength > 0)
            {
                Int32 palStart = 10;
                if (paletteLength % 3 != 0)
                    throw new ArgumentException("Bad length for 6-bit CPS palette.", "fileData");
                Int32 colors = paletteLength / 3;
                palette = LoadSixBitPalette(fileData, palStart, colors);
            }
            else
                palette = null;
            Byte[] imageData;
            Int32 dataOffset = 10 + paletteLength;
            if (compression == 0 && dataLen < dataOffset + bufferSize)
                throw new ArgumentException("File is not long enough to contain the image data!", "fileData");
            try
            {
                switch (compression)
                {
                    case 0:
                        imageData = new Byte[bufferSize];
                        Array.Copy(fileData, dataOffset, imageData, 0, bufferSize);
                        break;
                    case 4:
                        imageData = new Byte[bufferSize];
                        WWCompression.LcwDecompress(fileData, ref dataOffset, imageData, 0);
                        break;
                    default:
                        throw new ArgumentException("Unsupported compression format \"" + compression + "\".", "fileData");
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException("Error decompressing image data: " + e.Message, "fileData", e);
            }
            if (imageData == null)
                throw new ArgumentException("Error decompressing image data.", "fileData");
            return imageData;
        }

        /// <summary>
        /// Retrieves the C&C1/RA1 SHP frames.
        /// </summary>
        /// <param name="fileData">Original file data.</param>
        /// <param name="palette">Color palette</param>
        /// <returns>The SHP file's frames as bitmaps.</returns>
        public static Bitmap[] LoadCCShpFile(Byte[] fileData, Color[] palette)
        {
            return LoadCCShpFile(fileData, palette, null);
        }

        /// <summary>
        /// Retrieves the C&C1/RA1 SHP frames.
        /// </summary>
        /// <param name="fileData">Original file data.</param>
        /// <param name="palette">Color palette</param>
        /// <param name="remapTable">Optional remap table. Give null for no remapping.</param>
        /// <returns>The SHP file's frames as bitmaps.</returns>
        public static Bitmap[] LoadCCShpFile(Byte[] fileData, Color[] palette, byte[] remapTable)
        {
            Byte[][] frameData;
            int width;
            int height;
            try
            {
                frameData = GetCcShpData(fileData, out width, out height);
            }
            catch (ArgumentException)
            {
                return null;
            }
            int length = frameData.Length;
            int frameLength = width * height;
            if (remapTable != null && remapTable.Length >= 0x100)
            {
                for (int i = 0; i < length; ++i)
                {
                    Byte[] curFrame = frameData[i];
                    for (int j = 0; j < frameLength; j++)
                    {
                        curFrame[i] = remapTable[curFrame[i]];
                    }
                }
            }
            if (palette == null)
                palette = Enumerable.Range(0, 0x100).Select(i => Color.FromArgb(i, i, i)).ToArray();
            Bitmap[] frames = new Bitmap[length];
            for (int i = 0; i < length; ++i)
            {
                frames[i] = ImageUtils.BuildImage(frameData[i], width, height, width, PixelFormat.Format8bppIndexed, palette, Color.Black);
            }
            return frames;
        }

        /// <summary>
        /// Retrieves the C&C1 / RA1 SHP image data.
        /// </summary>
        /// <param name="fileData">File data</param>
        /// <param name="width">The width of all frames</param>
        /// <param name="height">The height of all frames</param>
        /// <returns>An array of byte arrays containing the 8-bit image data for each frame.</returns>
        /// <exception cref="ArgumentException">Thrown when parsing failed, indicating this is not a valid file of this format.</exception>
        public static Byte[][] GetCcShpData(Byte[] fileData, out int width, out int height)
        {
            // OffsetInfo / ShapeFileHeader
            Int32 hdrSize = 0x0E;
            if (fileData.Length < hdrSize)
                throw new ArgumentException("File is not long enough for header.", "fileData");
            UInt16 hdrFrames = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 0);
            //UInt16 hdrXPos = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 2);
            //UInt16 hdrYPos = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 4);
            UInt16 hdrWidth = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 6);
            UInt16 hdrHeight = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 8);
            //UInt16 hdrDeltaSize = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 0x0A);
            //UInt16 hdrFlags = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 0x0C);
            if (hdrFrames == 0) // Can be TS SHP; it identifies with an empty first byte IIRC.
                throw new ArgumentException("Not a C&C1/RA1 SHP file!", "fileData");
            if (hdrWidth == 0 || hdrHeight == 0)
                throw new ArgumentException("Illegal values in header!", "fileData");
            Dictionary<Int32, Int32> offsetIndices = new Dictionary<Int32, Int32>();
            Int32 offsSize = 8;
            Int32 fileSizeOffs = hdrSize + offsSize * (hdrFrames + 1);
            if (fileData.Length < hdrSize + offsSize * (hdrFrames + 2))
                throw new ArgumentException("File is not long enough to read the entire frames header!", "fileData");

            Int32 fileSize = (Int32)ArrayUtils.ReadIntFromByteArray(fileData, fileSizeOffs, 3, true);
            Boolean hasLoopFrame;
            if (fileSize != 0)
            {
                hasLoopFrame = true;
                hdrFrames++;
            }
            else
            {
                hasLoopFrame = false;
                fileSizeOffs -= offsSize;
                fileSize = (Int32)ArrayUtils.ReadIntFromByteArray(fileData, fileSizeOffs, 3, true);
            }
            Byte[][] frames = new Byte[hdrFrames][];
            CCShpOffsetInfo[] offsets = new CCShpOffsetInfo[hdrFrames];
            if (fileData.Length != fileSize)
                throw new ArgumentException("File size does not match size value in header!", "fileData");
            List<CcShpFrameFormat> frameFormats = Enum.GetValues(typeof(CcShpFrameFormat)).Cast<CcShpFrameFormat>().ToList();
            byte[][] framesList = new Byte[hasLoopFrame ? hdrFrames - 1 : hdrFrames][];
            width = hdrWidth;
            height = hdrHeight;
            // Frames decompression
            Int32 curOffs = hdrSize;
            Int32 frameSize = hdrWidth * hdrHeight;
            // Read is always safe; we already checked that the header size is inside the file bounds.
            CCShpOffsetInfo currentFrame = CCShpOffsetInfo.Read(fileData, curOffs);
            if (currentFrame.DataFormat != CcShpFrameFormat.Lcw)
                throw new ArgumentException("Error on frame 0: first frame needs to be LCW.", "fileData");
            if (currentFrame.ReferenceFormat != CcShpFrameFormat.Empty)
                throw new ArgumentException("Error on frame 0: LCW with illegal reference format.", "fileData");
            Int32 lastKeyFrameNr = 0;
            CCShpOffsetInfo lastKeyFrame = currentFrame;
            Int32 frameOffs = currentFrame.DataOffset;
            for (Int32 i = 0; i < hdrFrames; ++i)
            {
                if (!offsetIndices.ContainsKey(currentFrame.DataOffset))
                {
                    offsetIndices.Add(currentFrame.DataOffset, i);
                }
                offsets[i] = currentFrame;
                curOffs += offsSize;
                CCShpOffsetInfo nextFrame = CCShpOffsetInfo.Read(fileData, curOffs);
                if (!frameFormats.Contains(nextFrame.DataFormat))
                    throw new ArgumentException("Error on frame " + (i + 1) + ": Unknown frame type \"" + nextFrame.DataFormat.ToString("X2") + "\".", "fileData");
                if (!frameFormats.Contains(nextFrame.ReferenceFormat))
                    throw new ArgumentException("Error on frame " + (i + 1) + ": Unknown reference type \"" + nextFrame.ReferenceFormat.ToString("X2") + "\".", "fileData");
                Int32 frameOffsEnd = nextFrame.DataOffset;
                Int32 frameStart = frameOffs;
                CcShpFrameFormat frameOffsFormat = currentFrame.DataFormat;
                //Int32 dataLen = frameOffsEnd - frameOffs;
                if (frameOffs > fileData.Length || frameOffsEnd > fileData.Length)
                    throw new ArgumentException("Error on frame " + i + ": File is too small to contain all frame data!", "fileData");
                Byte[] frame = new Byte[frameSize];
                switch (frameOffsFormat)
                {
                    case CcShpFrameFormat.Lcw:
                        if (currentFrame.ReferenceFormat != CcShpFrameFormat.Empty)
                            throw new ArgumentException("Error on frame " + i + ": LCW with illegal reference format.");
                        // Actual LCW-compressed frame data.
                        WWCompression.LcwDecompress(fileData, ref frameOffs, frame, 0);
                        lastKeyFrame = currentFrame;
                        lastKeyFrameNr = i;
                        break;
                    case CcShpFrameFormat.XorChain:
                        // 0x20 = XOR with previous frame. Only used for chaining to previous XOR frames.
                        // Don't actually need this, but I do the integrity checks:
                        int refIndex20 = currentFrame.ReferenceOffset;
                        CcShpFrameFormat refFormat = currentFrame.ReferenceFormat;
                        if (i < 2 || refFormat != CcShpFrameFormat.XorChainRef || refIndex20 >= i || offsets[refIndex20].DataFormat != CcShpFrameFormat.XorBase
                            || (offsets[i - 1].DataFormat != CcShpFrameFormat.XorBase && offsets[i - 1].DataFormat != CcShpFrameFormat.XorChain))
                            throw new ArgumentException("Error on frame " + i + ": Bad frame reference information.", "fileData");
                        frames[i - 1].CopyTo(frame, 0);
                        WWCompression.ApplyXorDelta(frame, fileData, ref frameOffs, 0);
                        break;
                    case CcShpFrameFormat.XorBase:
                        if (currentFrame.ReferenceFormat != CcShpFrameFormat.Lcw)
                            throw new ArgumentException("Error on frame " + i + ": XOR base frames can only reference LCW frames!", "fileData");
                        // 0x40 = XOR with a previous frame. Could technically reference anything, but normally only references the last LCW "keyframe".
                        // This load method ignores the format saved in ReferenceFormat since the decompressed frame is stored already.
                        Int32 refIndex;
                        if (lastKeyFrame.DataOffset == currentFrame.ReferenceOffset)
                            refIndex = lastKeyFrameNr;
                        else if (!offsetIndices.TryGetValue(currentFrame.ReferenceOffset, out refIndex))
                        {
                            // not found as referenced frame, but in the file anyway?? Whatever; if it's LCW, just read it.
                            Int32 readOffs = currentFrame.ReferenceOffset;
                            if (readOffs >= fileData.Length)
                                throw new ArgumentException("Error on frame " + i + ": File is too small to contain all frame data.", "fileData");
                            WWCompression.LcwDecompress(fileData, ref readOffs, frame, 0);
                            refIndex = -1;
                        }
                        if (refIndex >= i)
                            throw new ArgumentException("Error on frame " + i + ": XOR cannot reference later frames.", "fileData");
                        if (refIndex >= 0)
                            frames[refIndex].CopyTo(frame, 0);
                        WWCompression.ApplyXorDelta(frame, fileData, ref frameOffs, 0);
                        break;
                    default:
                        throw new ArgumentException("Error on frame " + i + ": Unknown frame type \"" + frameOffsFormat.ToString("X2") + "\".", "fileData");
                }
                frames[i] = frame;
                Boolean brokenLoop = false;
                if (hasLoopFrame && i + 1 == hdrFrames)
                {
                    brokenLoop = !frame.SequenceEqual(frames[0]);
                }
                if (!hasLoopFrame || i + 1 < hdrFrames || brokenLoop)
                {
                    framesList[i] = frame;
                }
                if (frameOffsEnd == fileData.Length)
                    break;
                // Prepare for next loop
                currentFrame = nextFrame;
                frameOffs = frameOffsEnd;
            }
            return framesList;
        }

        /// <summary>
        /// Retrieves the Dune II SHP frames.
        /// </summary>
        /// <param name="fileData">Original file data.</param>
        /// <returns>The SHP file's frames as bitmaps.</returns>
        public static Bitmap[] LoadD2ShpFile(Byte[] fileData, Color[] palette)
        {
            Byte[][] frameData;
            int[] widths;
            int[] heights;
            try
            {
                frameData = frameData = GetD2ShpData(fileData, out widths, out heights);
            }
            catch (ArgumentException)
            {
                return null;
            }
            if (palette == null)
                palette = Enumerable.Range(0, 0x100).Select(i => Color.FromArgb(i, i, i)).ToArray();
            int length = frameData.Length;
            Bitmap[] frames = new Bitmap[length];
            for (int i = 0; i < length; ++i)
            {
                frames[i] = ImageUtils.BuildImage(frameData[i], widths[i], heights[i], widths[i], PixelFormat.Format8bppIndexed, palette, Color.Black);
            }
            return frames;
        }

        /// <summary>
        /// Retrieves the Dune II SHP image data. (used for mouse cursors in C&C1/RA1)
        /// </summary>
        /// <param name="fileData">File data</param>
        /// <param name="widths">The widths of all frames</param>
        /// <param name="heights">The heights of all frames</param>
        /// <returns>An array of byte arrays containing the 8-bit image data for each frame.</returns>
        /// <exception cref="ArgumentException">Thrown when parsing failed, indicating this is not a valid file of this format.</exception>
        public static Byte[][] GetD2ShpData(Byte[] fileData, out int[] widths, out int[] heights)
        {
            // OffsetInfo / ShapeFileHeader
            if (fileData.Length < 6)
                throw new ArgumentException("Not long enough for header.", "fileData");
            Int32 hdrFrames = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 0);
            if (hdrFrames == 0)
                throw new ArgumentException("Not a Dune II SHP file", "fileData");
            if (fileData.Length < 2 + (hdrFrames + 1) * 2)
                throw new ArgumentException("Not long enough for frames index.", "fileData");
            // Length. Done -2 because everything that follows is relative to the location after the header
            UInt32 endoffset = (UInt32)fileData.Length;
            Boolean isVersion107;
            // test v1.00 first, since it might accidentally be possible that the offset 2x as far happens to contain data matching the file end address.
            // However, in 32-bit addressing, it is impossible for even partial addresses halfway down the array to ever match the file end value.
            if (endoffset < UInt16.MaxValue && (endoffset >= 2 + (hdrFrames + 1) * 2 && ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 2 + hdrFrames * 2) == endoffset))
                isVersion107 = false;
            else if (endoffset >= 2 + (hdrFrames + 1) * 4 && ArrayUtils.ReadUInt32FromByteArrayLe(fileData, 2 + hdrFrames * 4) == endoffset - 2)
                isVersion107 = true;
            else
                throw new ArgumentException("File size in header does not match; cannot detect version.", "fileData");
            // v1.07 is relative to offsets array start, so the found end offset will be 2 lower.
            if (isVersion107)
                endoffset -= 2;

            byte[][] framesList = new byte[hdrFrames][];
            widths = new int[hdrFrames];
            heights = new int[hdrFrames];

            Boolean[] remapped = new Boolean[hdrFrames];
            Boolean[] notCompressed = new Boolean[hdrFrames];
            // Frames
            Int32 curOffs = 2;
            Int32 readLen = isVersion107 ? 4 : 2;
            Int32 nextOFfset = (Int32)ArrayUtils.ReadIntFromByteArray(fileData, curOffs, readLen, true);
            for (Int32 i = 0; i < hdrFrames; ++i)
            {
                // Set current read address to previously-fetched "next entry" address
                Int32 readOffset = nextOFfset;
                // Reached end; process completed.
                if (endoffset == readOffset)
                    break;
                // Check illegal values.
                if (readOffset <= 0 || readOffset + 0x0A > endoffset)
                    throw new ArgumentException("Illegal address in frame indices.", "fileData");

                // Set header ptr to next address
                curOffs += readLen;
                // Read next entry address, to act as end of current entry.
                nextOFfset = (Int32)ArrayUtils.ReadIntFromByteArray(fileData, curOffs, readLen, true);

                // Compensate for header size
                Int32 realReadOffset = readOffset;
                if (isVersion107)
                    realReadOffset += 2;

                Dune2ShpFrameFlags frameFlags = (Dune2ShpFrameFlags)ArrayUtils.ReadUInt16FromByteArrayLe(fileData, realReadOffset + 0x00);
                Byte frmSlices = fileData[realReadOffset + 0x02];
                UInt16 frmWidth = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, realReadOffset + 0x03);
                Byte frmHeight = fileData[realReadOffset + 0x05];
                // Size of all frame data: header, lookup table, and compressed data.
                UInt16 frmDataSize = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, realReadOffset + 0x06);
                UInt16 frmZeroCompressedSize = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, realReadOffset + 0x08);
                realReadOffset += 0x0A;
                // Bit 1: Contains remap palette
                // Bit 2: Don't decompress with LCW
                // Bit 3: Has custom remap palette size.
                Boolean hasRemap = (frameFlags & Dune2ShpFrameFlags.HasRemapTable) != 0;
                Boolean noLcw = (frameFlags & Dune2ShpFrameFlags.NoLcw) != 0;
                notCompressed[i] = noLcw;
                Boolean customRemap = (frameFlags & Dune2ShpFrameFlags.CustomSizeRemap) != 0;
                remapped[i] = hasRemap;
                Int32 curEndOffset = readOffset + frmDataSize;
                if (curEndOffset > endoffset) // curEndOffset > nextOFfset
                    throw new ArgumentException("Illegal address in frame indices.", "fileData");
                // I assume this is illegal...?
                if (frmWidth == 0 || frmHeight == 0)
                    throw new ArgumentException("Illegal values in frame header!", "fileData");

                Int32 remapSize;
                Byte[] remapTable;
                if (hasRemap)
                {
                    if (customRemap)
                    {
                        remapSize = fileData[realReadOffset];
                        realReadOffset++;
                    }
                    else
                        remapSize = 16;
                    remapTable = new Byte[remapSize];
                    Array.Copy(fileData, realReadOffset, remapTable, 0, remapSize);
                    realReadOffset += remapSize;
                }
                else
                {
                    remapSize = 0;
                    remapTable = null;
                    // Dunno if this should be done?
                    if (customRemap)
                        realReadOffset++;
                }
                Byte[] zeroDecompressData = new Byte[frmZeroCompressedSize];
                if (noLcw)
                {
                    Array.Copy(fileData, realReadOffset, zeroDecompressData, 0, frmZeroCompressedSize);
                }
                else
                {
                    Byte[] lcwDecompressData = new Byte[frmZeroCompressedSize * 3];
                    Int32 predictedEndOff = realReadOffset + frmDataSize - remapSize;
                    if (customRemap)
                        predictedEndOff--;
                    Int32 lcwReadOffset = realReadOffset;
                    Int32 decompressedSize = WWCompression.LcwDecompress(fileData, ref lcwReadOffset, lcwDecompressData, 0);
                    if (decompressedSize != frmZeroCompressedSize)
                        throw new ArgumentException("LCW decompression failed.", "fileData");
                    if (lcwReadOffset > predictedEndOff)
                        throw new ArgumentException("LCW decompression exceeded data bounds!", "fileData");
                    Array.Copy(lcwDecompressData, zeroDecompressData, frmZeroCompressedSize);

                }
                Int32 refOffs = 0;
                Byte[] fullFrame = WWCompression.RleZeroD2Decompress(zeroDecompressData, ref refOffs, frmWidth, frmSlices);
                if (remapTable != null)
                {
                    Byte[] remap = remapTable;
                    Int32 remapLen = remap.Length;
                    for (Int32 j = 0; j < fullFrame.Length; ++j)
                    {
                        Byte val = fullFrame[j];
                        if (val < remapLen)
                            fullFrame[j] = remap[val];
                        else
                            throw new ArgumentException("Remapping failed: value is larger than remap table!", "fileData");
                    }
                }
                framesList[i] = fullFrame;
                widths[i] = frmWidth;
                heights[i] = frmHeight;
            }
            return framesList;
        }

        public static Byte[][] GetCcTmpData(Byte[] fileData, out int[] widths, out int[] heights)
        {
            Int32 fileLen = fileData.Length;
            if (fileLen < 0x20)
                throw new ArgumentException("File is not long enough to be a C&C Template file.", "fileData");
            Int16 hdrWidth = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x00);
            Int16 hdrHeight = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x02);
            // Amount of icons to form the full icon set. Not necessarily the same as the amount of actual icons.
            Int16 hdrCount = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x04);
            // Always 0
            Int16 hdrAllocated = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x06);
            Int32 hdrSize = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x08);
            // Offset of start of actual icon data. Generally always 0x20
            Int32 hdrIconsPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x0C);
            // Offset of start of palette data. Probably always 0.
            Int32 hdrPalettesPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x10);
            // Offset of remaps data. Dune II leftover of 4 bit to 8 bit translation tables.
            // Always fixed value 0x0D1AFFFF, which makes no sense as ptr.
            Int32 hdrRemapsPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x14);
            // Offset of 'transparency flags'? Generally points to an empty array at the end of the file.
            Int32 hdrTransFlagPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x18);
            // Offset of actual icon set definition, defining for each index which icon data to use. FF for none.
            Int32 hdrMapPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x1C);
            // File size check
            if (hdrSize != fileData.Length)
                throw new ArgumentException("File size in header does not match.", "fileData");
            // Only allowing standard 24x24 size
            if (hdrHeight != 24 || hdrWidth != 24)
                throw new ArgumentException("Only 24×24 pixel tiles are supported.", "fileData");
            // Checking some normally hardcoded values
            if (hdrAllocated != 0 || hdrPalettesPtr != 0)// || hdrRemapsPtr != 0x0D1AFFFF)
                throw new ArgumentException("Invalid values encountered in header.");
            if (hdrCount == 0)
                throw new ArgumentException("Tileset files with 0 tiles are not supported!", "fileData");
            // Checking if data is all inside the file
            if (hdrIconsPtr >= fileLen || (hdrMapPtr + hdrCount) > fileLen)
                throw new ArgumentException("Invalid header values: indices outside file range.", "fileData");
            Int32 tileSize = hdrWidth * hdrHeight;
            // Maps the available images onto the full iconset definition
            Byte[] map = new Byte[hdrCount];
            Array.Copy(fileData, hdrMapPtr, map, 0, hdrCount);
            // Get max index plus one for real images count. Nothing in the file header actually specifies this directly.
            Int32 actualImages = map.Max(x => x == 0xFF ? -1 : x) + 1;
            if (hdrTransFlagPtr + actualImages > fileLen)
                throw new ArgumentException("Invalid header values: indices outside file range.", "fileData");
            if (hdrIconsPtr + actualImages * tileSize > fileLen)
                throw new ArgumentException("Tile image data outside file range.", "fileData");
            Byte[] imagesIndex = new Byte[actualImages];
            Array.Copy(fileData, hdrTransFlagPtr, imagesIndex, 0, actualImages);
            Byte[][] tiles = new Byte[hdrCount][];
            widths = new int[hdrCount];
            heights = new int[hdrCount];
            Boolean[] tileUseList = new Boolean[map.Length];
            for (Int32 i = 0; i < map.Length; ++i)
            {
                Byte dataIndex = map[i];
                Boolean used = dataIndex != 0xFF;
                tileUseList[i] = used;
                Byte[] tileData = new Byte[tileSize];
                if (used)
                {
                    Int32 offset = hdrIconsPtr + dataIndex * tileSize;
                    if ((offset + tileSize) > fileLen)
                        throw new ArgumentException("Tile data outside file range.", "fileData");
                    Array.Copy(fileData, offset, tileData, 0, tileSize);
                    tiles[i] = tileData;
                    widths[i] = hdrWidth;
                    heights[i] = hdrHeight;
                }
            }
            return tiles;
        }

        public static Byte[][] GetRaTmpData(Byte[] fileData, out int[] widths, out int[] heights)
        {
            Int32 fileLen = fileData.Length;
            if (fileLen < 0x28)
                throw new ArgumentException("File is not long enough to be a C&C Template file.", "fileData");
            Int16 hdrWidth = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x00);
            Int16 hdrHeight = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x02);
            // Amount of icons to form the full icon set. Not necessarily the same as the amount of actual icons.
            Int16 hdrCount = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x04);
            // Always 0
            Int16 hdrAllocated = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x06);
            // New in RA
            Int16 hdrMapWidth = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x08);
            Int16 hdrMapHeight = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x0A);
            Int32 hdrSize = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x0C);
            // Offset of start of actual icon data. Generally always 0x20
            Int32 hdrIconsPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x10);
            // Offset of start of palette data. Probably always 0.
            Int32 hdrPalettesPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x14);
            // Offset of remaps data. Dune II leftover of 4 bit to 8 bit translation tables.
            // Always seems to be 0x2C730FXX (with values differing for the lowest byte), which makes no sense as ptr.
            Int32 hdrRemapsPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x18);
            // Offset of 'transparency flags'? Generally points to an empty array at the end of the file.
            Int32 hdrTransFlagPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x1C);
            // Offset of 'color' map, indicating the terrain type for each type. This includes unused cells, which are usually indicated as 0.
            Int32 hdrColorMapPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x20);
            // Offset of actual icon set definition, defining for each index which icon data to use. FF for none.
            Int32 hdrMapPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x24);
            // File size check
            if (hdrSize != fileData.Length)
                throw new ArgumentException("File size in header does not match.", "fileData");
            // Only allowing standard 24x24 size
            if (hdrHeight != 24 || hdrWidth != 24)
                throw new ArgumentException("Only 24×24 pixel tiles are supported.", "fileData");
            // Checking some normally hardcoded values
            if (hdrAllocated != 00 || hdrPalettesPtr != 0)
                throw new ArgumentException("Invalid values encountered in header.");
            if (hdrCount == 0)
                throw new ArgumentException("Tileset files with 0 tiles are not supported!", "fileData");
            // Checking if data is all inside the file
            if (hdrIconsPtr >= fileLen || (hdrMapPtr + hdrCount) > fileLen)
                throw new ArgumentException("Invalid header values: indices outside file range.", "fileData");
            Int32 tileSize = hdrWidth * hdrHeight;
            // Maps the available images onto the full iconset definition
            Byte[] map = new Byte[hdrCount];
            Array.Copy(fileData, hdrMapPtr, map, 0, hdrCount);
            // Get max index plus one for real images count. Nothing in the file header actually specifies this directly.
            Int32 actualImages = map.Max(x => x == 0xFF ? -1 : (Int32)x) + 1;
            if (hdrTransFlagPtr + actualImages > fileLen)
                throw new ArgumentException("Invalid header values: indices outside file range.", "fileData");
            if (hdrIconsPtr + actualImages * tileSize > fileLen)
                throw new ArgumentException("Tile image data outside file range.", "fileData");
            Byte[] imagesIndex = new Byte[actualImages];
            Array.Copy(fileData, hdrTransFlagPtr, imagesIndex, 0, actualImages);
            Byte[][] tiles = new Byte[hdrCount][];
            widths = new int[hdrCount];
            heights = new int[hdrCount];
            Boolean[] tileUseList = new Boolean[map.Length];
            for (Int32 i = 0; i < map.Length; ++i)
            {
                Byte dataIndex = map[i];
                Boolean used = dataIndex != 0xFF;
                tileUseList[i] = used;
                Byte[] tileData = new Byte[tileSize];
                if (used)
                {
                    Int32 offset = hdrIconsPtr + dataIndex * tileSize;
                    if ((offset + tileSize) > fileLen)
                        throw new ArgumentException("Tile data outside file range.", "fileData");
                    Array.Copy(fileData, offset, tileData, 0, tileSize);
                    tiles[i] = tileData;
                    widths[i] = hdrWidth;
                    heights[i] = hdrHeight;
                }
            }
            return tiles;
        }

        public static Color[] LoadSixBitPalette(Byte[] fileData, int palStart, int colors)
        {
            Color[] palette = Enumerable.Repeat(Color.Black, colors).ToArray();
            // Palette data should always be be 0x300 long, but this code works regardless of that.
            int len = Math.Min(fileData.Length / 3, colors);
            int offs = palStart;
            for (int i = 0; i < len; ++i)
            {
                byte r = ConvertToEightBit[fileData[offs + 0] & 0x3F];
                byte g = ConvertToEightBit[fileData[offs + 1] & 0x3F];
                byte b = ConvertToEightBit[fileData[offs + 2] & 0x3F];
                palette[i] = Color.FromArgb(r, g, b);
                offs += 3;
            }
            return palette;
        }

        #region private classes for processing the types

        private class CCShpOffsetInfo
        {
            public Int32 DataOffset { get; set; }
            public CcShpFrameFormat DataFormat { get; set; }
            public Int32 ReferenceOffset { get; set; }
            public CcShpFrameFormat ReferenceFormat { get; set; }

            public CCShpOffsetInfo(Int32 dataOffset, CcShpFrameFormat dataFormat, Int32 referenceOffset, CcShpFrameFormat referenceFormat)
            {
                this.DataOffset = dataOffset;
                this.DataFormat = dataFormat;
                this.ReferenceOffset = referenceOffset;
                this.ReferenceFormat = referenceFormat;
            }

            public static CCShpOffsetInfo Read(Byte[] fileData, Int32 offset)
            {
                Int32 dataOffset = (Int32)ArrayUtils.ReadIntFromByteArray(fileData, offset, 3, true);
                CcShpFrameFormat dataFormat = (CcShpFrameFormat)fileData[offset + 3];
                Int32 referenceOffset = (Int32)ArrayUtils.ReadIntFromByteArray(fileData, offset + 4, 3, true);
                CcShpFrameFormat referenceFormat = (CcShpFrameFormat)fileData[offset + 7];
                return new CCShpOffsetInfo(dataOffset, dataFormat, referenceOffset, referenceFormat);
            }
        }

        private enum CcShpFrameFormat
        {
            Empty = 0x00,
            XorChain = 0x20,
            XorBase = 0x40,
            XorChainRef = 0x48,
            Lcw = 0x80,
        }

        [Flags]
        private enum Dune2ShpFrameFlags
        {
            Empty = 0x00,
            // Bit 1: Contains remap table
            HasRemapTable = 0x01,
            // Bit 2: Don't decompress with LCW
            NoLcw = 0x02,
            // Bit 3: Has custom remap table size.
            CustomSizeRemap = 0x04
        }
        #endregion
    }
}
