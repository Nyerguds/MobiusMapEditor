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
    /// <summary>File load exceptions. These are typically ignored in favour of checking the next type to try.</summary>
    [Serializable]
    public class FileTypeLoadException : Exception
    {
        /// <summary>USed to store the attempted load type in the Data dictionary to allow serialization.</summary>
        protected readonly string DataAttemptedLoadedType = "AttemptedLoadedType";

        /// <summary>File type that was attempted to be loaded and threw this exception.</summary>
        public string AttemptedLoadedType
        {
            get { return this.Data[this.DataAttemptedLoadedType] as string; }
            set { this.Data[this.DataAttemptedLoadedType] = value; }
        }

        public FileTypeLoadException() { }
        public FileTypeLoadException(string message) : base(message) { }
        public FileTypeLoadException(string message, Exception innerException) : base(message, innerException) { }
        public FileTypeLoadException(string message, string attemptedLoadedType)
            : base(message)
        {
            this.AttemptedLoadedType = attemptedLoadedType;
        }
        public FileTypeLoadException(string message, string attemptedLoadedType, Exception innerException)
            : base(message, innerException)
        {
            this.AttemptedLoadedType = attemptedLoadedType;
        }
    }

    /// <summary>
    /// This class contains the code for loading classic sprite formats; TD/RA SHP, Dune II SHP, TD Template, RA Template, and CPS.
    /// Most of this code was originally written for the Engie File Converter tool.
    /// </summary>
    public static class ClassicSpriteLoader
    {
        private static readonly byte[] ConvertToEightBit = new byte[64];

        static ClassicSpriteLoader()
        {
            // Build an easy lookup table for this, so no calculations are ever needed for it later.
            for (int i = 0; i < 64; ++i)
            {
                ConvertToEightBit[i] = (byte)Math.Round(i * 255.0 / 63.0, MidpointRounding.AwayFromZero);
            }
        }

        /// <summary>
        /// Retrieves the CPS image.
        /// </summary>
        /// <param name="fileData">Original file data.</param>
        /// <param name="throwWhenParsing">If false, parse errors will simply return <see langword="false"/> instead of throwing an exception.</param>
        /// <returns>The raw 8-bit linear image data in a 64000 byte array.</returns>
        public static Bitmap LoadCpsFile(byte[] fileData, bool throwWhenParsing)
        {
            byte[] imageData = GetCpsData(fileData, out Color[] palette, throwWhenParsing);
            if (imageData == null)
            {
                return null;
            }
            if (palette == null)
                palette = Enumerable.Range(0, 0x100).Select(i => Color.FromArgb(i, i, i)).ToArray();
            return ImageUtils.BuildImage(imageData, 320, 200, 320, PixelFormat.Format8bppIndexed, palette, null);
        }

        /// <summary>
        /// Retrieves the CPS image data.
        /// </summary>
        /// <param name="fileData">Original file data.</param>
        /// <returns>The raw 8-bit linear image data in a 64000 byte array.</returns>
        /// <param name="throwWhenParsing">If false, parse errors will simply return <see langword="false"/> instead of throwing an exception.</param>
        /// <exception cref="FileTypeLoadException">Thrown when parsing failed, indicating this is not a valid file of this format.</exception>
        public static byte[] GetCpsData(byte[] fileData, out Color[] palette, bool throwWhenParsing)
        {
            palette = null;
            int dataLen = fileData.Length;
            if (dataLen < 10)
            {
                if (!throwWhenParsing) 
                {
                    return null;
                }
                throw new FileTypeLoadException("File is not long enough to be a valid CPS file.", "CPS file");
            }
            int fileSize = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 0);
            int compression = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 2);
            if (compression < 0 || compression > 4)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }

                throw new FileTypeLoadException("Unknown compression type " + compression, "CPS file");
            }
            // compressions other than 0 and 4 count the full file including size header.
            if (compression == 0 || compression == 4)
                fileSize += 2;
            else
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("LZW and RLE compression are not supported.", "CPS file");
            }
            if (fileSize != dataLen)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("File size in header does not match.", "CPS file");
            }
            int bufferSize = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 4);
            int paletteLength = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 8);
            if (bufferSize != 64000)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Unknown CPS type.", "fileData");
            }
            if (paletteLength > 0)
            {
                int palStart = 10;
                if (paletteLength % 3 != 0)
                {
                    if (!throwWhenParsing)
                    {
                        return null;
                    }
                    throw new FileTypeLoadException("Bad length for 6-bit CPS palette.", "CPS file");
                }
                int colors = paletteLength / 3;
                palette = LoadSixBitPalette(fileData, palStart, colors);
            }
            byte[] imageData;
            int dataOffset = 10 + paletteLength;
            if (compression == 0 && dataLen < dataOffset + bufferSize)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("File is not long enough to contain the image data.", "CPS file");
            }
            try
            {
                switch (compression)
                {
                    case 0:
                        imageData = new byte[bufferSize];
                        Array.Copy(fileData, dataOffset, imageData, 0, bufferSize);
                        break;
                    case 4:
                        imageData = new byte[bufferSize];
                        WWCompression.LcwDecompress(fileData, ref dataOffset, imageData, 0);
                        break;
                    default:
                        throw new FileTypeLoadException("Unsupported compression format \"" + compression + "\".", "CPS file");
                }
            }
            catch (Exception e)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Error decompressing image data: " + e.Message, "CPS file", e);
            }
            if (imageData == null)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Error decompressing image data.", "CPS file");
            }
            return imageData;
        }

        /// <summary>
        /// Retrieves the C&amp;C1/RA1 SHP frames.
        /// </summary>
        /// <param name="fileData">Original file data.</param>
        /// <param name="palette">Color palette</param>
        /// <param name="throwWhenParsing">If false, parse errors will simply return <see langword="false"/> instead of throwing an exception.</param>
        /// <returns>The SHP file's frames as bitmaps.</returns>
        public static Bitmap[] LoadCCShpFile(byte[] fileData, Color[] palette, bool throwWhenParsing)
        {
            return LoadCCShpFile(fileData, palette, null, throwWhenParsing);
        }

        /// <summary>
        /// Retrieves the C&amp;C1/RA1 SHP frames.
        /// </summary>
        /// <param name="fileData">Original file data.</param>
        /// <param name="palette">Color palette</param>
        /// <param name="remapTable">Optional remap table. Give null for no remapping.</param>
        /// <param name="throwWhenParsing">If false, parse errors will simply return <see langword="false"/> instead of throwing an exception.</param>
        /// <returns>The SHP file's frames as bitmaps.</returns>
        public static Bitmap[] LoadCCShpFile(byte[] fileData, Color[] palette, byte[] remapTable, bool throwWhenParsing)
        {
            byte[][] frameData;
            int width;
            int height;
            frameData = GetCcShpData(fileData, out width, out height, throwWhenParsing);
            if (frameData == null)
            {
                return null;
            }
            int length = frameData.Length;
            int frameLength = width * height;
            if (remapTable != null && remapTable.Length >= 0x100)
            {
                for (int i = 0; i < length; ++i)
                {
                    byte[] curFrame = frameData[i];
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
        /// Retrieves the C&amp;C1 / RA1 SHP image data.
        /// </summary>
        /// <param name="fileData">File data</param>
        /// <param name="width">The width of all frames</param>
        /// <param name="height">The height of all frames</param>
        /// <param name="throwWhenParsing">If false, parse errors will simply return <see langword="false"/> instead of throwing an exception.</param>
        /// <returns>An array of byte arrays containing the 8-bit image data for each frame.</returns>
        /// <exception cref="FileTypeLoadException">Thrown when parsing failed, indicating this is not a valid file of this format.</exception>
        public static byte[][] GetCcShpData(byte[] fileData, out int width, out int height, bool throwWhenParsing)
        {
            // OffsetInfo / ShapeFileHeader
            int hdrSize = 0x0E;
            width = 0;
            height = 0;
            if (fileData.Length < hdrSize)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("File is not long enough for header.", "TD/RA SHP file");
            }
            ushort hdrFrames = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 0);
            //UInt16 hdrXPos = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 2);
            //UInt16 hdrYPos = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 4);
            ushort hdrWidth = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 6);
            ushort hdrHeight = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 8);
            //UInt16 hdrDeltaSize = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 0x0A);
            //UInt16 hdrFlags = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 0x0C);
            if (hdrFrames == 0) // Can be TS SHP; it identifies with an empty first byte IIRC.
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Not a C&C1/RA1 SHP file.", "TD/RA SHP file");
            }
            if (hdrWidth == 0 || hdrHeight == 0)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Illegal values in header.", "TD/RA SHP file");
            }
            Dictionary<int, int> offsetIndices = new Dictionary<int, int>();
            int offsSize = 8;
            int fileSizeOffs = hdrSize + offsSize * (hdrFrames + 1);
            if (fileData.Length < hdrSize + offsSize * (hdrFrames + 2))
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("File is not long enough to read the entire frames header.", "TD/RA SHP file");
            }
            int fileSize = (int)ArrayUtils.ReadIntFromByteArray(fileData, fileSizeOffs, 3, true);
            bool hasLoopFrame;
            if (fileSize != 0)
            {
                hasLoopFrame = true;
                hdrFrames++;
            }
            else
            {
                hasLoopFrame = false;
                fileSizeOffs -= offsSize;
                fileSize = (int)ArrayUtils.ReadIntFromByteArray(fileData, fileSizeOffs, 3, true);
            }
            byte[][] frames = new byte[hdrFrames][];
            CCShpOffsetInfo[] offsets = new CCShpOffsetInfo[hdrFrames];
            if (fileData.Length != fileSize)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("File size does not match size value in header.", "TD/RA SHP file");
            }
            List<CcShpFrameFormat> frameFormats = Enum.GetValues(typeof(CcShpFrameFormat)).Cast<CcShpFrameFormat>().ToList();
            byte[][] framesList = new byte[hasLoopFrame ? hdrFrames - 1 : hdrFrames][];
            width = hdrWidth;
            height = hdrHeight;
            // Frames decompression
            int curOffs = hdrSize;
            int frameSize = hdrWidth * hdrHeight;
            // Read is always safe; we already checked that the header size is inside the file bounds.
            CCShpOffsetInfo currentFrame = CCShpOffsetInfo.Read(fileData, curOffs);
            if (currentFrame.DataFormat != CcShpFrameFormat.Lcw)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Error on frame 0: first frame needs to be LCW.", "TD/RA SHP file");
            }
            if (currentFrame.ReferenceFormat != CcShpFrameFormat.Empty)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Error on frame 0: LCW with illegal reference format.", "TD/RA SHP file");
            }
            int lastKeyFrameNr = 0;
            CCShpOffsetInfo lastKeyFrame = currentFrame;
            int frameOffs = currentFrame.DataOffset;
            for (int i = 0; i < hdrFrames; ++i)
            {
                if (!offsetIndices.ContainsKey(currentFrame.DataOffset))
                {
                    offsetIndices.Add(currentFrame.DataOffset, i);
                }
                offsets[i] = currentFrame;
                curOffs += offsSize;
                CCShpOffsetInfo nextFrame = CCShpOffsetInfo.Read(fileData, curOffs);
                if (!frameFormats.Contains(nextFrame.DataFormat))
                {
                    if (!throwWhenParsing)
                    {
                        return null;
                    }
                    throw new FileTypeLoadException("Error on frame " + (i + 1) + ": Unknown frame type \"" + nextFrame.DataFormat.ToString("X2") + "\".", "TD/RA SHP file");
                }
                if (!frameFormats.Contains(nextFrame.ReferenceFormat))
                {
                    if (!throwWhenParsing)
                    {
                        return null;
                    }
                    throw new FileTypeLoadException("Error on frame " + (i + 1) + ": Unknown reference type \"" + nextFrame.ReferenceFormat.ToString("X2") + "\".", "TD/RA SHP file");
                }
                int frameOffsEnd = nextFrame.DataOffset;
                int frameStart = frameOffs;
                CcShpFrameFormat frameOffsFormat = currentFrame.DataFormat;
                //Int32 dataLen = frameOffsEnd - frameOffs;
                if (frameOffs > fileData.Length || frameOffsEnd > fileData.Length)
                {
                    if (!throwWhenParsing)
                    {
                        return null;
                    }
                    throw new FileTypeLoadException("Error on frame " + i + ": File is too small to contain all frame data.", "TD/RA SHP file");
                }
                byte[] frame = new byte[frameSize];
                switch (frameOffsFormat)
                {
                    case CcShpFrameFormat.Lcw:
                        if (currentFrame.ReferenceFormat != CcShpFrameFormat.Empty)
                        {
                            if (!throwWhenParsing)
                            {
                                return null;
                            }
                            throw new ArgumentException("Error on frame " + i + ": LCW with illegal reference format.");
                        }
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
                        {
                            if (!throwWhenParsing)
                            {
                                return null;
                            }
                            throw new FileTypeLoadException("Error on frame " + i + ": Bad frame reference information.", "TD/RA SHP file");
                        }
                        frames[i - 1].CopyTo(frame, 0);
                        WWCompression.ApplyXorDelta(frame, fileData, ref frameOffs, 0);
                        break;
                    case CcShpFrameFormat.XorBase:
                        if (currentFrame.ReferenceFormat != CcShpFrameFormat.Lcw)
                        {
                            if (!throwWhenParsing)
                            {
                                return null;
                            }
                            throw new FileTypeLoadException("Error on frame " + i + ": XOR base frames can only reference LCW frames.", "TD/RA SHP file");
                        }
                        // 0x40 = XOR with a previous frame. Could technically reference anything, but normally only references the last LCW "keyframe".
                        // This load method ignores the format saved in ReferenceFormat since the decompressed frame is stored already.
                        int refIndex;
                        if (lastKeyFrame.DataOffset == currentFrame.ReferenceOffset)
                        {
                            refIndex = lastKeyFrameNr;
                        }
                        else if (!offsetIndices.TryGetValue(currentFrame.ReferenceOffset, out refIndex))
                        {
                            // not found as referenced frame, but in the file anyway?? Whatever; if it's LCW, just read it.
                            int readOffs = currentFrame.ReferenceOffset;
                            if (readOffs >= fileData.Length)
                            {
                                if (!throwWhenParsing)
                                {
                                    return null;
                                }
                                throw new FileTypeLoadException("Error on frame " + i + ": File is too small to contain all frame data.", "TD/RA SHP file");
                            }
                            WWCompression.LcwDecompress(fileData, ref readOffs, frame, 0);
                            refIndex = -1;
                        }
                        if (refIndex >= i)
                        {
                            if (!throwWhenParsing)
                            {
                                return null;
                            }
                            throw new FileTypeLoadException("Error on frame " + i + ": XOR cannot reference later frames.", "TD/RA SHP file");
                        }
                        if (refIndex >= 0)
                        {
                            frames[refIndex].CopyTo(frame, 0);
                        }
                        WWCompression.ApplyXorDelta(frame, fileData, ref frameOffs, 0);
                        break;
                    default:
                        if (!throwWhenParsing)
                        {
                            return null;
                        }
                        throw new FileTypeLoadException("Error on frame " + i + ": Unknown frame type \"" + frameOffsFormat.ToString("X2") + "\".", "TD/RA SHP file");
                }
                frames[i] = frame;
                bool brokenLoop = false;
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
        public static Bitmap[] LoadD2ShpFile(byte[] fileData, Color[] palette)
        {
            byte[][] frameData;
            int[] widths;
            int[] heights;
            try
            {
                frameData = frameData = GetD2ShpData(fileData, out widths, out heights, false);
            }
            catch (FileTypeLoadException)
            {
                return null;
            }
            if (frameData == null)
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
        /// Retrieves the Dune II SHP image data. (used for mouse cursors in C&amp;C1/RA1)
        /// </summary>
        /// <param name="fileData">File data</param>
        /// <param name="widths">The widths of all frames</param>
        /// <param name="heights">The heights of all frames</param>
        /// <param name="throwWhenParsing">If false, parse errors will simply return <see langword="false"/> instead of throwing an exception.</param>
        /// <returns>An array of byte arrays containing the 8-bit image data for each frame.</returns>
        /// <exception cref="ArgumentException">Thrown when parsing failed, indicating this is not a valid file of this format.</exception>
        public static byte[][] GetD2ShpData(byte[] fileData, out int[] widths, out int[] heights, bool throwWhenParsing)
        {
            widths = null;
            heights = null;
            // OffsetInfo / ShapeFileHeader
            if (fileData.Length < 6)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Not long enough for header.", "Dune II SHP file");
            }
            int hdrFrames = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 0);
            if (hdrFrames == 0)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Not a Dune II SHP file", "Dune II SHP file");
            }
            if (fileData.Length < 2 + (hdrFrames + 1) * 2)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Not long enough for frames index.", "Dune II SHP file");
            }
            // Length. Done -2 because everything that follows is relative to the location after the header
            uint endoffset = (uint)fileData.Length;
            bool isVersion107;
            // test v1.00 first, since it might accidentally be possible that the offset 2x as far happens to contain data matching the file end address.
            // However, in 32-bit addressing, it is impossible for even partial addresses halfway down the array to ever match the file end value.
            if (endoffset < ushort.MaxValue && endoffset >= 2 + (hdrFrames + 1) * 2 && ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 2 + hdrFrames * 2) == endoffset)
            {
                isVersion107 = false;
            }
            else if (endoffset >= 2 + (hdrFrames + 1) * 4 && ArrayUtils.ReadUInt32FromByteArrayLe(fileData, 2 + hdrFrames * 4) == endoffset - 2)
            {
                isVersion107 = true;
            }
            else
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("File size in header does not match; cannot detect version.", "Dune II SHP file");
            }
            // v1.07 is relative to offsets array start, so the found end offset will be 2 lower.
            if (isVersion107)
                endoffset -= 2;
            byte[][] framesList = new byte[hdrFrames][];
            widths = new int[hdrFrames];
            heights = new int[hdrFrames];
            bool[] remapped = new bool[hdrFrames];
            bool[] notCompressed = new bool[hdrFrames];
            // Frames
            int curOffs = 2;
            int readLen = isVersion107 ? 4 : 2;
            int nextOFfset = (int)ArrayUtils.ReadIntFromByteArray(fileData, curOffs, readLen, true);
            for (int i = 0; i < hdrFrames; ++i)
            {
                // Set current read address to previously-fetched "next entry" address
                int readOffset = nextOFfset;
                // Reached end; process completed.
                if (endoffset == readOffset)
                {
                    break;
                }
                // Check illegal values.
                if (readOffset <= 0 || readOffset + 0x0A > endoffset)
                {
                    if (!throwWhenParsing)
                    {
                        return null;
                    }
                    throw new FileTypeLoadException("Illegal address in frame indices.", "Dune II SHP file");
                }
                // Set header ptr to next address
                curOffs += readLen;
                // Read next entry address, to act as end of current entry.
                nextOFfset = (int)ArrayUtils.ReadIntFromByteArray(fileData, curOffs, readLen, true);
                // Compensate for header size
                int realReadOffset = readOffset;
                if (isVersion107)
                {
                    realReadOffset += 2;
                }
                Dune2ShpFrameFlags frameFlags = (Dune2ShpFrameFlags)ArrayUtils.ReadUInt16FromByteArrayLe(fileData, realReadOffset + 0x00);
                byte frmSlices = fileData[realReadOffset + 0x02];
                ushort frmWidth = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, realReadOffset + 0x03);
                byte frmHeight = fileData[realReadOffset + 0x05];
                // Size of all frame data: header, lookup table, and compressed data.
                ushort frmDataSize = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, realReadOffset + 0x06);
                ushort frmZeroCompressedSize = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, realReadOffset + 0x08);
                realReadOffset += 0x0A;
                // Bit 1: Contains remap palette
                // Bit 2: Don't decompress with LCW
                // Bit 3: Has custom remap palette size.
                bool hasRemap = (frameFlags & Dune2ShpFrameFlags.HasRemapTable) != 0;
                bool noLcw = (frameFlags & Dune2ShpFrameFlags.NoLcw) != 0;
                notCompressed[i] = noLcw;
                bool customRemap = (frameFlags & Dune2ShpFrameFlags.CustomSizeRemap) != 0;
                remapped[i] = hasRemap;
                int curEndOffset = readOffset + frmDataSize;
                if (curEndOffset > endoffset) // curEndOffset > nextOFfset
                {
                    if (!throwWhenParsing)
                    {
                        return null;
                    }
                    throw new FileTypeLoadException("Illegal address in frame indices.", "Dune II SHP file");
                }
                // I assume this is illegal...?
                if (frmWidth == 0 || frmHeight == 0)
                {
                    if (!throwWhenParsing)
                    {
                        return null;
                    }
                    throw new FileTypeLoadException("Illegal values in frame header.", "Dune II SHP file");
                }
                int remapSize;
                byte[] remapTable;
                if (hasRemap)
                {
                    if (customRemap)
                    {
                        remapSize = fileData[realReadOffset];
                        realReadOffset++;
                    }
                    else
                    {
                        remapSize = 16;
                    }
                    remapTable = new byte[remapSize];
                    Array.Copy(fileData, realReadOffset, remapTable, 0, remapSize);
                    realReadOffset += remapSize;
                }
                else
                {
                    remapSize = 0;
                    remapTable = null;
                    // Dunno if this should be done?
                    if (customRemap)
                    {
                        realReadOffset++;
                    }
                }
                byte[] zeroDecompressData = new byte[frmZeroCompressedSize];
                if (noLcw)
                {
                    Array.Copy(fileData, realReadOffset, zeroDecompressData, 0, frmZeroCompressedSize);
                }
                else
                {
                    byte[] lcwDecompressData = new byte[frmZeroCompressedSize * 3];
                    int predictedEndOff = realReadOffset + frmDataSize - remapSize;
                    if (customRemap)
                    {
                        predictedEndOff--;
                    }
                    int lcwReadOffset = realReadOffset;
                    int decompressedSize = WWCompression.LcwDecompress(fileData, ref lcwReadOffset, lcwDecompressData, 0);
                    if (decompressedSize != frmZeroCompressedSize)
                    {
                        if (!throwWhenParsing)
                        {
                            return null;
                        }
                        throw new FileTypeLoadException("LCW decompression failed.", "Dune II SHP file");
                    }
                    if (lcwReadOffset > predictedEndOff)
                    {
                        if (!throwWhenParsing)
                        {
                            return null;
                        }
                        throw new FileTypeLoadException("LCW decompression exceeded data bounds.", "Dune II SHP file");
                    }
                    Array.Copy(lcwDecompressData, zeroDecompressData, frmZeroCompressedSize);
                }
                int refOffs = 0;
                byte[] fullFrame = WWCompression.RleZeroD2Decompress(zeroDecompressData, ref refOffs, frmWidth, frmSlices);
                if (remapTable != null)
                {
                    byte[] remap = remapTable;
                    int remapLen = remap.Length;
                    for (int j = 0; j < fullFrame.Length; ++j)
                    {
                        byte val = fullFrame[j];
                        if (val < remapLen)
                        {
                            fullFrame[j] = remap[val];
                        }
                        else
                        {
                            if (!throwWhenParsing)
                            {
                                return null;
                            }
                            throw new FileTypeLoadException("Remapping failed: value is larger than remap table.", "Dune II SHP file");
                        }
                    }
                }
                framesList[i] = fullFrame;
                widths[i] = frmWidth;
                heights[i] = frmHeight;
            }
            return framesList;
        }

        /// <summary>
        /// Parses C&C TMP format.
        /// </summary>
        /// <param name="fileData">File data to parse</param>
        /// <param name="widths">Widths of all returned frames</param>
        /// <param name="heights">Heights of all returned frames</param>
        /// <param name="throwWhenParsing">If false, parse errors will simply return <see langword="false"/> instead of throwing an exception.</param>
        /// <returns>The returned frames, as 8bpp byte arrays.</returns>
        /// <exception cref="FileTypeLoadException"></exception>
        public static byte[][] GetCcTmpData(byte[] fileData, out int[] widths, out int[] heights, bool throwWhenParsing)
        {
            int fileLen = fileData.Length;
            widths = null;
            heights = null;
            if (fileLen < 0x20)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("File is not long enough to be a C&C Template file.", "TD TMP file");
            }
            short hdrWidth = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x00);
            short hdrHeight = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x02);
            // Amount of icons to form the full icon set. Not necessarily the same as the amount of actual icons.
            short hdrCount = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x04);
            // Always 0
            short hdrAllocated = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x06);
            int hdrSize = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x08);
            // Offset of start of actual icon data. Generally always 0x20
            int hdrIconsPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x0C);
            // Offset of start of palette data. Probably always 0.
            int hdrPalettesPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x10);
            // Offset of remaps data. Dune II leftover of 4 bit to 8 bit translation tables.
            // Always fixed value 0x0D1AFFFF, which makes no sense as ptr.
            int hdrRemapsPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x14);
            // Offset of 'transparency flags'? Generally points to an empty array at the end of the file.
            int hdrTransFlagPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x18);
            // Offset of actual icon set definition, defining for each index which icon data to use. FF for none.
            int hdrMapPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x1C);
            // File size check
            if (hdrSize != fileData.Length)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("File size in header does not match.", "TD TMP file");
            }
            // Only allowing standard 24x24 size
            if (hdrHeight != 24 || hdrWidth != 24)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Only 24×24 pixel tiles are supported.", "TD TMP file");
            }
            // Checking some normally hardcoded values
            if (hdrAllocated != 0 || hdrPalettesPtr != 0)// || hdrRemapsPtr != 0x0D1AFFFF)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Invalid values encountered in header.");
            }
            if (hdrCount == 0)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Tileset files with 0 tiles are not supported.", "TD TMP file");
            }
            // Checking if data is all inside the file
            if (hdrIconsPtr >= fileLen || (hdrMapPtr + hdrCount) > fileLen)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Invalid header values: indices outside file range.", "TD TMP file");
            }
            int tileSize = hdrWidth * hdrHeight;
            // Maps the available images onto the full iconset definition
            byte[] map = new byte[hdrCount];
            Array.Copy(fileData, hdrMapPtr, map, 0, hdrCount);
            // Get max index plus one for real images count. Nothing in the file header actually specifies this directly.
            int actualImages = map.Max(x => x == 0xFF ? -1 : x) + 1;
            if (hdrTransFlagPtr + actualImages > fileLen)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Invalid header values: indices outside file range.", "TD TMP file");
            }
            if (hdrIconsPtr + actualImages * tileSize > fileLen)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Tile image data outside file range.", "TD TMP file");
            }
            byte[] imagesIndex = new byte[actualImages];
            Array.Copy(fileData, hdrTransFlagPtr, imagesIndex, 0, actualImages);
            byte[][] tiles = new byte[hdrCount][];
            widths = new int[hdrCount];
            heights = new int[hdrCount];
            bool[] tileUseList = new bool[map.Length];
            for (int i = 0; i < map.Length; ++i)
            {
                byte dataIndex = map[i];
                bool used = dataIndex != 0xFF;
                tileUseList[i] = used;
                byte[] tileData = new byte[tileSize];
                if (used)
                {
                    int offset = hdrIconsPtr + dataIndex * tileSize;
                    if ((offset + tileSize) > fileLen)
                    {
                        if (!throwWhenParsing)
                        {
                            return null;
                        }
                        throw new FileTypeLoadException("Tile data outside file range.", "TD TMP file");
                    }
                    Array.Copy(fileData, offset, tileData, 0, tileSize);
                    tiles[i] = tileData;
                    widths[i] = hdrWidth;
                    heights[i] = hdrHeight;
                }
            }
            return tiles;
        }

        /// <summary>
        /// Parses RA Template data format.
        /// </summary>
        /// <param name="fileData">File data to parse.</param>
        /// <param name="widths">Widths of all returned frames.</param>
        /// <param name="heights">Heights of all returned frames.</param>
        /// <param name="landTypesInfo">Land type info for each frame.</param>
        /// <param name="tileUseList">Array indicating which tiles are used.</param>
        /// <param name="headerWidth">Width of tileset piece in amount of tiles.</param>
        /// <param name="headerHeight">Height of tileset piece in amount of tiles.</param>
        /// <param name="throwWhenParsing">If false, parse errors will simply return <see langword="false"/> instead of throwing an exception.</param>
        /// <returns></returns>
        /// <exception cref="FileTypeLoadException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static byte[][] GetRaTmpData(byte[] fileData, out int[] widths, out int[] heights, out byte[] landTypesInfo, out bool[] tileUseList, out int headerWidth, out int headerHeight, bool throwWhenParsing)
        {
            widths = null;
            heights = null;
            landTypesInfo = null;
            tileUseList = null;
            headerWidth = 0;
            headerHeight = 0;
            int fileLen = fileData.Length;
            if (fileLen < 0x28)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("File is not long enough to be an RA Template file.", "RA TMP file");
            }
            short hdrWidth = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x00);
            short hdrHeight = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x02);
            // Amount of icons to form the full icon set. Not necessarily the same as the amount of actual icons.
            short hdrCount = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x04);
            // Always 0
            short hdrAllocated = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x06);
            // New in RA
            headerWidth = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x08);
            headerHeight = ArrayUtils.ReadInt16FromByteArrayLe(fileData, 0x0A);
            int hdrSize = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x0C);
            // Offset of start of actual icon data. Generally always 0x20
            int hdrIconsPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x10);
            // Offset of start of palette data. Probably always 0.
            int hdrPalettesPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x14);
            // Offset of remaps data. Dune II leftover of 4 bit to 8 bit translation tables.
            // Always seems to be 0x2C730FXX (with values differing for the lowest byte), which makes no sense as ptr.
            int hdrRemapsPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x18);
            // Offset of 'transparency flags'? Generally points to an empty array at the end of the file.
            int hdrTransFlagPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x1C);
            // Offset of 'color' map, indicating the terrain type for each type. This includes unused cells, which are usually indicated as 0.
            int hdrColorMapPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x20);
            // Offset of actual icon set definition, defining for each index which icon data to use. FF for none.
            int hdrMapPtr = ArrayUtils.ReadInt32FromByteArrayLe(fileData, 0x24);
            // File size check
            if (hdrSize != fileData.Length)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("File size in header does not match.", "RA TMP file");
            }
            // Only allowing standard 24x24 size
            if (hdrHeight != 24 || hdrWidth != 24)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Only 24×24 pixel tiles are supported.", "RA TMP file");
            }
            // Checking some normally hardcoded values
            if (hdrAllocated != 00 || hdrPalettesPtr != 0)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new ArgumentException("Invalid values encountered in header.");
            }
            if (hdrCount == 0)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Tileset files with 0 tiles are not supported.", "RA TMP file");
            }
            // Checking if data is all inside the file
            if (hdrIconsPtr >= fileLen || (hdrMapPtr + hdrCount) > fileLen)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Invalid header values: indices outside file range.", "RA TMP file");
            }
            int tileSize = hdrWidth * hdrHeight;
            // Maps the available images onto the full iconset definition
            byte[] map = new byte[hdrCount];
            Array.Copy(fileData, hdrMapPtr, map, 0, hdrCount);
            landTypesInfo = new byte[Math.Max(1, headerWidth) * Math.Max(1, headerHeight)];
            if (hdrMapPtr + landTypesInfo.Length > fileLen)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Invalid header values: land types outside file range.", "RA TMP file");
            }
            Array.Copy(fileData, hdrColorMapPtr, landTypesInfo, 0, landTypesInfo.Length);
            // Get max index plus one for real images count. Nothing in the file header actually specifies this directly.
            int actualImages = map.Max(x => x == 0xff ? -1 : x) + 1;
            if (hdrTransFlagPtr + actualImages > fileLen)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Invalid header values: indices outside file range.", "RA TMP file");
            }
            if (hdrIconsPtr + actualImages * tileSize > fileLen)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("Tile image data outside file range.", "RA TMP file");
            }
            byte[] imagesIndex = new byte[actualImages];
            Array.Copy(fileData, hdrTransFlagPtr, imagesIndex, 0, actualImages);
            byte[][] tiles = new byte[hdrCount][];
            widths = new int[hdrCount];
            heights = new int[hdrCount];
            tileUseList = new bool[map.Length];
            for (int i = 0; i < map.Length; ++i)
            {
                byte dataIndex = map[i];
                bool used = dataIndex != 0xFF;
                tileUseList[i] = used;
                byte[] tileData = new byte[tileSize];
                if (used)
                {
                    int offset = hdrIconsPtr + dataIndex * tileSize;
                    if ((offset + tileSize) > fileLen)
                    {
                        if (!throwWhenParsing)
                        {
                            return null;
                        }
                        throw new FileTypeLoadException("Tile data outside file range.", "RA TMP file");
                    }
                    Array.Copy(fileData, offset, tileData, 0, tileSize);
                }
                tiles[i] = tileData;
                widths[i] = hdrWidth;
                heights[i] = hdrHeight;
            }
            return tiles;
        }

        /// <summary>
        /// Retrieves the Dune II SHP frames.
        /// </summary>
        /// <param name="fileData">Original file data.</param>
        /// <returns>The SHP file's frames as bitmaps.</returns>
        public static Bitmap[] LoadFontFile(byte[] fileData, Color[] palette, byte[] subPalette, byte[] indices)
        {
            byte[][] frameData;
            int[] widths;
            int height;
            try
            {
                frameData = frameData = GetCCFontData(fileData, out widths, out height, false);
            }
            catch (ArgumentException)
            {
                return null;
            }
            if (frameData == null)
            {
                return null;
            }
            if (palette == null)
                palette = Enumerable.Range(0, 0x10).Select(i => Color.FromArgb(i, i, i)).ToArray();
            else if (palette.Length == 0x100 && subPalette != null && subPalette.Length == 0x10)
            {
                Color[] fontPalette = new Color[0x10];
                for (int i = 0; i < 0x10; ++i)
                {
                    fontPalette[i] = palette[subPalette[i]];
                }
                palette = fontPalette;
            }
            int length = frameData.Length;
            Bitmap[] frames = new Bitmap[length];
            HashSet<int> indicesSet = indices == null ? null : indices.Select(x => (int)x).ToHashSet();
            for (int i = 0; i < length; ++i)
            {
                if (indicesSet != null && !indicesSet.Contains(i))
                {
                    continue;
                }
                frames[i] = ImageUtils.BuildImage(frameData[i], widths[i], height, widths[i], PixelFormat.Format4bppIndexed, palette, Color.Black);
            }
            return frames;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="widths"></param>
        /// <param name="height"></param>
        /// <param name="throwWhenParsing">If false, parse errors will simply return <see langword="false"/> instead of throwing an exception.</param>
        /// <returns></returns>
        /// <exception cref="FileTypeLoadException"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static byte[][] GetCCFontData(byte[] fileData, out int[] widths, out int height, bool throwWhenParsing)
        {
            widths = null;
            height = 0;
            int fileLength = fileData.Length;
            if (fileLength < 0x14)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException();
            }
            int fontHeaderLength = (ushort)ArrayUtils.ReadIntFromByteArray(fileData, 0x00, 2, true);
            if (fontHeaderLength != fileLength)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("File is not long enough to be an C&C Font file.", "Font file");
            }
            byte fontHeaderCompress = fileData[0x02];
            //Byte dataBlocks = fileData[0x03];
            //Int16 infoBlockOffset = (UInt16)ArrayUtils.ReadIntFromByteArray(fileData, 0x04, 2, true);
            int fontHeaderOffsetBlockOffset = (ushort)ArrayUtils.ReadIntFromByteArray(fileData, 0x06, 2, true);
            int fontHeaderWidthBlockOffset = (ushort)ArrayUtils.ReadIntFromByteArray(fileData, 0x08, 2, true);
            // use this for pos on TS format
            int fontHeaderDataBlockOffset = (ushort)ArrayUtils.ReadIntFromByteArray(fileData, 0x0A, 2, true);
            int fontHeaderHeightOffset = (ushort)ArrayUtils.ReadIntFromByteArray(fileData, 0x0C, 2, true);
            //UInt16 unknown0E = (UInt16)ArrayUtils.ReadIntFromByteArray(fileData, 0x0E, 2, true);
            //Byte AlwaysZero = fileData[0x10];
            int nrOfSymbols;
            if (fontHeaderCompress == 0x02)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("This is Tiberian Sun font format!", "Font file");
            }
            else if (fontHeaderCompress == 0x00)
            {
                nrOfSymbols = fileData[0x11] + 1; // "last symbol" byte, so actual amount is this value + 1.
            }
            else
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException(string.Format("Unknown font type identifier, '{0}'.", fontHeaderCompress), "Font file");
            }
            height = fileData[0x12]; // MaxHeight
            int width = fileData[0x13]; // MaxWidth
            if (fontHeaderOffsetBlockOffset + nrOfSymbols * 2 > fileLength)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("File data too short for offsets list!", "Font file");
            }
            if (fontHeaderWidthBlockOffset + nrOfSymbols > fileLength)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("File data too short for symbol widths list starting from offset!", "Font file");
            }
            if (fontHeaderHeightOffset + nrOfSymbols * 2 > fileLength)
            {
                if (!throwWhenParsing)
                {
                    return null;
                }
                throw new FileTypeLoadException("File data too short for symbol heights list!", "Font file");
            }
            //FontDataOffset
            int[] fontDataOffsetsList = new int[nrOfSymbols];
            for (int i = 0; i < nrOfSymbols; ++i)
                fontDataOffsetsList[i] = (ushort)ArrayUtils.ReadIntFromByteArray(fileData, fontHeaderOffsetBlockOffset + i * 2, 2, true);
            List<byte> widthsList = new List<byte>();
            for (int i = 0; i < nrOfSymbols; ++i)
            {
                byte fWidth = fileData[fontHeaderWidthBlockOffset + i];
                if (fWidth > width)
                {
                    // Font width has no real impact anyway. Allow this.
                    width = fWidth;
                }
                widthsList.Add(fWidth);
            }
            List<byte> yOffsetsList = new List<byte>();
            List<byte> heightsList = new List<byte>();
            for (int i = 0; i < nrOfSymbols; ++i)
            {
                yOffsetsList.Add(fileData[fontHeaderHeightOffset + i * 2]);
                byte fHeight = fileData[fontHeaderHeightOffset + i * 2 + 1];
                if (fHeight > height)
                {
                    if (!throwWhenParsing)
                    {
                        return null;
                    }
                    throw new FileTypeLoadException(string.Format("Illegal value '{0}' in symbol heights list at entry #{1}: the value is larger than global height '{2}'.", fHeight, i, height));
                }
                heightsList.Add(fHeight);
            }
            // End of FileTypeLoadExceptions. After this, assume the type is identified.
            byte[][] fontList = new byte[nrOfSymbols][];
            bool[] trmask = new bool[] { true };
            for (int i = 0; i < nrOfSymbols; ++i)
            {
                int start = fontDataOffsetsList[i];
                byte sbWidth = widthsList[i];
                byte sbHeight = heightsList[i];
                byte yOffset = yOffsetsList[i];
                
                byte[] dataFullFrame;
                try
                {
                    byte[] data8Bit = ImageUtils.ConvertTo8Bit(fileData, sbWidth, sbHeight, start, 4, false);
                    dataFullFrame = new byte[sbWidth * height];
                    ImageUtils.PasteOn8bpp(dataFullFrame, sbWidth, height, sbWidth, data8Bit, sbWidth, sbHeight, sbWidth, new Rectangle(0, yOffset, sbWidth, sbHeight), trmask, true);
                }
                catch (IndexOutOfRangeException ex)
                {
                    if (!throwWhenParsing)
                    {
                        return null;
                    }
                    throw new IndexOutOfRangeException(string.Format("Data for font entry #{0} exceeds file bounds!", i), ex);
                }
                fontList[i] = dataFullFrame;
            }
            widths = widthsList.Select(x => (int)x).ToArray();
            return fontList;
        }

        public static Color[] LoadSixBitPalette(byte[] fileData, int palStart, int colors)
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
            public int DataOffset { get; set; }
            public CcShpFrameFormat DataFormat { get; set; }
            public int ReferenceOffset { get; set; }
            public CcShpFrameFormat ReferenceFormat { get; set; }

            public CCShpOffsetInfo(int dataOffset, CcShpFrameFormat dataFormat, int referenceOffset, CcShpFrameFormat referenceFormat)
            {
                this.DataOffset = dataOffset;
                this.DataFormat = dataFormat;
                this.ReferenceOffset = referenceOffset;
                this.ReferenceFormat = referenceFormat;
            }

            public static CCShpOffsetInfo Read(byte[] fileData, int offset)
            {
                int dataOffset = (int)ArrayUtils.ReadIntFromByteArray(fileData, offset, 3, true);
                CcShpFrameFormat dataFormat = (CcShpFrameFormat)fileData[offset + 3];
                int referenceOffset = (int)ArrayUtils.ReadIntFromByteArray(fileData, offset + 4, 3, true);
                CcShpFrameFormat referenceFormat = (CcShpFrameFormat)fileData[offset + 7];
                return new CCShpOffsetInfo(dataOffset, dataFormat, referenceOffset, referenceFormat);
            }
        }

        private enum CcShpFrameFormat
        {
            Empty       /**/ = 0x00,
            XorChain    /**/ = 0x20,
            XorBase     /**/ = 0x40,
            XorChainRef /**/ = 0x48,
            Lcw         /**/ = 0x80,
        }

        [Flags]
        private enum Dune2ShpFrameFlags
        {
            Empty           /**/ = 0,
            HasRemapTable   /**/ = 1 >> 0, // Bit 1: Contains remap table
            NoLcw           /**/ = 1 >> 1, // Bit 2: Don't decompress with LCW
            CustomSizeRemap /**/ = 1 >> 2  // Bit 3: Has custom remap table size.
        }
        #endregion
    }
}
