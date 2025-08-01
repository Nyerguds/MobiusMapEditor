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
using MobiusEditor.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MobiusEditor.Utility
{
    public static class MixContentAnalysis
    {
        private static readonly char[] badIniHeaderRange = Enumerable.Range(0, 0x20).Select(i => (char)i).ToArray();
        private static readonly HashSet<byte> badTextRange = Enumerable.Range(0, 0x20).Where(v => v != '\t' && v != '\r' && v != '\n' && v != ' ').Select(i => (byte)i).ToHashSet();

        private static readonly int HighestTdMapVal = TiberianDawn.TemplateTypes.GetTypes().Max(t => (int)t.ID);
        /// <summary>Maximum file size that gets processed by byte array (5 MiB).</summary>
        private const uint maxProcessed = 0x500000;

        const int PalLength = 0x300;
        const int PalMax = 0x3F;
        const int XccHeaderLength = 0x34;

        /// <summary>
        /// Analyses the files inside a mix archive to identify the file types.
        /// </summary>
        /// <param name="current">mix file to analyse.</param>
        /// <param name="missionsOnly">If enabled, the procedure will only identify missions and mix files.</param>
        /// <param name="checkAbort">Abort trigger to check.</param>
        /// <returns>The result of the analysis.</returns>
        public static List<MixEntry> AnalyseFiles(MixFile current, bool missionsOnly, Func<bool> checkAbort)
        {
            List<uint> filesList = current.HeaderIds.ToList();
            Dictionary<uint,int> encountered = new Dictionary<uint,int>();
            List<MixEntry> fileInfo = new List<MixEntry>();
            foreach (uint fileId in filesList)
            {
                if (checkAbort != null && checkAbort())
                {
                    return null;
                }
                MixEntry[] entries = current.GetFullFileInfo(fileId);
                int index = encountered.TryGetOrDefault(fileId, 0);
                encountered[fileId] = index + 1;
                MixEntry mixInfo = entries[index];
                fileInfo.Add(mixInfo);
                // Only do identification if type is set to unknown.
                if (mixInfo.Type == MixContentType.Unknown)
                {
                    using (Stream file = current.OpenFile(mixInfo))
                    {
                        TryIdentifyFile(file, mixInfo, current, missionsOnly);
                    }
                }
                
            }
            // Header order
            return fileInfo;
            // Filename order
            //return fileInfo.OrderBy(x => x.SortName).ToList();
            // File offset order
            //return fileInfo.OrderBy(x => x.Offset).ToList();
        }

        private static void TryIdentifyFile(Stream fileStream, MixEntry mixInfo, MixFile source, bool missionsAndMixFilesOnly)
        {
            mixInfo.Type = MixContentType.Unknown;
            string extension = Path.GetExtension(mixInfo.Name);
            if (mixInfo.Length == 0)
            {
                mixInfo.Type = MixContentType.Empty;
                mixInfo.Info = "Empty file";
                return;
            }
            // Very strict requirements, and jumps over the majority of file contents while checking, so check this first.
            // These are ALWAYS identified, even if set to "missions only", because not identifying them slows down the rest of the analysis.
            // Ideally, all file types will eventually end up in here. Sadly, for ini that won't be the case.
            if (IdentifyShp(fileStream, mixInfo))
                return;
            if (IdentifyD2Shp(fileStream, mixInfo))
                return;
            if (IdentifyMix(source, mixInfo))
                return;
            if (IdentifyAud(fileStream, mixInfo))
                return;
            if (IdentifyVqa(fileStream, mixInfo))
                return;
            if (IdentifyVqp(fileStream, mixInfo))
                return;
            if (IdentifyPalTable(fileStream, mixInfo))
                return;
            if (IdentifyBmp(fileStream, mixInfo))
                return;
            if (IdentifyXccNames(fileStream, mixInfo))
                return;
            if (IdentifyRaMixNames(fileStream, mixInfo))
                return;
            if (IdentifyPcx(fileStream, mixInfo))
                return;
            if (IdentifyCps(fileStream, mixInfo))
                return;
            if (IdentifyLut(fileStream, mixInfo))
                return;
            if (IdentifyWsa(fileStream, mixInfo))
                return;
            if (IdentifyCcTmp(fileStream, mixInfo))
                return;
            if (IdentifyRaTmp(fileStream, mixInfo))
                return;
            // These types analyse the full file from byte array. I'm restricting the buffer for them to 5mb; they shouldn't need more.
            // Eventually, all of these (except ini I guess) should ideally be switched to stream to speed up the processing.
            if (mixInfo.Length <= maxProcessed)
            {
                int fileLength = (int)mixInfo.Length;
                byte[] fileContents = new byte[fileLength];
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.Read(fileContents, 0, fileLength);
                if (!missionsAndMixFilesOnly)
                {
                    if (IdentifyCcFont(fileContents, mixInfo))
                        return;
                }
                if (IdentifyIni(fileContents, mixInfo))
                    return;
                if (!missionsAndMixFilesOnly)
                {
                    if (IdentifyStringsFile(fileContents, mixInfo))
                        return;
                    if (IdentifyText(fileContents, mixInfo))
                        return;
                }
                if (IdentifyTdMap(fileStream, mixInfo))
                    return;
                // Always needs to happen before sole maps since all palettes will technically match that format.
                if (IdentifyPalette(fileStream, mixInfo))
                    return;
                // Only check for sole map if name is known and type is correct.
                if (".bin".Equals(extension, StringComparison.OrdinalIgnoreCase) && IdentifySoleMap(fileStream, mixInfo))
                    return;
            }
            // Only do this if it passes the check on extension.
            if (!missionsAndMixFilesOnly && ".mrf".Equals(extension, StringComparison.OrdinalIgnoreCase) && IdentifyMrf(fileStream, mixInfo))
            {
                return;
            }
            mixInfo.Type = MixContentType.Unknown;
            mixInfo.Info = string.Empty;
        }

        private static bool IdentifyShp(Stream fileStream, MixEntry mixInfo)
        {
            try
            {
                byte[][] shpData = ClassicSpriteLoader.GetCcShpData(fileStream, (int)mixInfo.Length, out int width, out int height, false);
                if (shpData != null)
                {
                    mixInfo.Type = MixContentType.ShpTd;
                    mixInfo.Info = string.Format("C&C SHP; {0} frame{1}, {2}×{3}", shpData.Length, shpData.Length == 1 ? string.Empty : "s", width, height);
                    return true;
                }
            }
            catch (FileTypeLoadException) { /* ignore */ }
            return false;
        }

        private static bool IdentifyPcx(Stream fileStream, MixEntry mixInfo)
        {
            try
            {
                if (mixInfo.Length < 128)
                {
                    return false;
                }
                byte[] fileContents = new byte[128];
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.Read(fileContents, 0, fileContents.Length);
                if (fileContents[0] != 10) // ID byte
                {
                    return false;
                }
                byte version = fileContents[1];
                byte encoding = fileContents[2];
                if (encoding > 1)
                {
                    return false;
                }
                bool reservedByteFree = fileContents[64] == 0; // reserved byte
                for (int i = 74; i < 128; ++i)
                {
                    // End of header reserved space
                    if (fileContents[i] != 0)
                    {
                        return false;
                    }
                }
                int bitsPerPlane = fileContents[3]; // Number of bits to represent a pixel (per Plane) - 1, 2, 4, or 8
                if (bitsPerPlane != 1 && bitsPerPlane != 2 && bitsPerPlane != 4 && bitsPerPlane != 8 && bitsPerPlane != 24 && bitsPerPlane != 32)
                {
                    return false;
                }
                ushort windowXmin = ArrayUtils.ReadUInt16FromByteArrayLe(fileContents, 4);
                ushort windowYmin = ArrayUtils.ReadUInt16FromByteArrayLe(fileContents, 6);
                ushort windowXmax = ArrayUtils.ReadUInt16FromByteArrayLe(fileContents, 8);
                ushort windowYmax = ArrayUtils.ReadUInt16FromByteArrayLe(fileContents, 10);
                if (windowXmax < windowXmin || windowYmax < windowYmin)
                    return false;
                //ushort hDpi = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 12); // Horizontal Resolution of image in DPI
                //ushort vDpi = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 14); // Vertical Resolution of image in DPI
                byte numPlanes = fileContents[65]; // Number of color planes
                if (numPlanes > 4)
                {
                    return false;
                }
                ushort bytesPerLine = ArrayUtils.ReadUInt16FromByteArrayLe(fileContents, 66); // Number of bytes to allocate for a scanline plane.  MUST be an EVEN number.  Do NOT calculate from Xmax-Xmin.
                ushort paletteInfo = ArrayUtils.ReadUInt16FromByteArrayLe(fileContents, 68); // How to interpret palette: 1 = Color/BW, 2 = Grayscale (ignored in PB IV/ IV Plus)
                //ushort hscreenSize = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 70); // Horizontal screen size in pixels. New field found only in PB IV/IV Plus
                //ushort vscreenSize = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 72); // Vertical screen size in pixels. New field found only in PB IV/IV Plus
                int width = windowXmax - windowXmin + 1;
                int height = windowYmax - windowYmin + 1;
                int bitsPerPixel = numPlanes * bitsPerPlane;

                if (bitsPerPixel != 1 && bitsPerPixel != 2 && bitsPerPixel != 4 && bitsPerPixel != 8 && bitsPerPixel != 24 && bitsPerPixel != 32)
                {
                    return false;
                }
                mixInfo.Type = MixContentType.Pcx;
                mixInfo.Info = string.Format("PCX Image; {0}×{1}, {2} bpp", width, height, bitsPerPixel);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IdentifyBmp(Stream fileStream, MixEntry mixInfo)
        {
            uint dataLen = mixInfo.Length;
            // Header is 14 in length.
            // The next 4 are the start of the specific DIB header, which says how large the header is.
            // The next 8 are two int32 values with the width and height of the image.
            // The next 4 are two int16 values with the amount of planes and the bits per pixel.
            const int headerLen = 0x1E;
            if (dataLen < headerLen)
                return false;
            byte[] header1 = new byte[headerLen];
            fileStream.Seek(0, SeekOrigin.Begin);
            fileStream.Read(header1 , 0, header1.Length);
            if (header1[0] != 0x42 || header1[1] != 0x4D)
                return false;
            uint size = ArrayUtils.ReadUInt32FromByteArrayLe(header1, 0x02);
            uint reserved = ArrayUtils.ReadUInt32FromByteArrayLe(header1, 0x06);
            int headerEnd = ArrayUtils.ReadInt32FromByteArrayLe(header1, 0x0A);
            if (size != dataLen || reserved != 0 || dataLen < headerEnd)
                return false;
            // Read size from DIB hheader
            int headerSize = ArrayUtils.ReadInt32FromByteArrayLe(header1, 0x0E);
            int width = ArrayUtils.ReadInt32FromByteArrayLe(header1, 0x12);
            int height = ArrayUtils.ReadInt32FromByteArrayLe(header1, 0x16);
            int planes = ArrayUtils.ReadInt16FromByteArrayLe(header1, 0x1A);
            int bitsPerPixel = ArrayUtils.ReadInt16FromByteArrayLe(header1, 0x1C);
            if (headerEnd < headerSize + 14)
                return false;
            try
            {
                int dibformat;
                if (headerSize == 40)
                {
                    dibformat = 1;
                }
                else if (headerSize == 124)
                {
                    dibformat = 5;
                }
                else
                {
                    return false;
                }
                mixInfo.Type = MixContentType.Bmp;
                mixInfo.Info = string.Format("Bitmap Image; DIB v{0}, {1}×{2}, {3} bpp", dibformat, width, height, bitsPerPixel);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IdentifyD2Shp(Stream fileStream, MixEntry mixInfo)
        {
            try
            {
                byte[][] shpData = ClassicSpriteLoader.GetD2ShpData(fileStream, (int)mixInfo.Length, out int[] widths, out int[] heights, false);
                if (shpData != null)
                {
                    mixInfo.Type = MixContentType.ShpD2;
                    mixInfo.Info = string.Format("Dune II SHP; {0} frame{1}, {2}×{3}", shpData.Length, shpData.Length == 1 ? string.Empty : "s", widths.Max(), heights.Max());
                    return true;
                }
            }
            catch (FileTypeLoadException) { /* ignore */ }
            return false;
        }

        private static bool IdentifyCps(Stream fileStream, MixEntry mixInfo)
        {
            try
            {
                const int headerLen = 10;
                if (mixInfo.Length < 10)
                {
                    return false;
                }
                byte[] header = new byte[headerLen];
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.Read(header, 0, header.Length);
                int fileSize = ArrayUtils.ReadUInt16FromByteArrayLe(header, 0);
                int compression = ArrayUtils.ReadUInt16FromByteArrayLe(header, 2);
                if (compression != 0 && compression != 4)
                {
                    return false;
                }
                fileSize += 2;
                if (fileSize != mixInfo.Length)
                {
                    return false;
                }
                int bufferSize = ArrayUtils.ReadInt32FromByteArrayLe(header, 4);
                int paletteLength = ArrayUtils.ReadInt16FromByteArrayLe(header, 8);
                if (bufferSize != 64000)
                {
                    return false;
                }
                if (paletteLength > 0)
                {
                    if (paletteLength % 3 != 0 || paletteLength + 10 > fileSize)
                    {
                        return false;
                    }
                    byte[] pal = new byte[paletteLength];
                    fileStream.Read(pal, 0, paletteLength);
                    for (int i = 0; i < paletteLength; ++i)
                    {
                        // verify 6-bit palette
                        if (pal[i] > PalMax)
                        {
                            return false;
                        }
                    }
                }
                mixInfo.Type = MixContentType.Cps;
                mixInfo.Info = "CPS Image; 320×200";
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        private static bool IdentifyLut(Stream fileStream, MixEntry mixInfo)
        {
            // RA chrono vortex lookup table
            const int LutDimensions = 64;
            const int LutSize = LutDimensions * LutDimensions * 3;
            const int LutMaxBrightness = 16;
            if (mixInfo.Length != LutSize)
            {
                return false;
            }
            try
            {
                byte[] table = new byte[LutSize];
                fileStream.Seek(0, SeekOrigin.Begin);
                int read = fileStream.Read(table, 0, LutSize);
                if (read < LutSize)
                {
                    return false;
                }
                for (int i = 0; i < LutSize; i += 3)
                {
                    int x = table[i + 0];
                    int y = table[i + 1];
                    int b = table[i + 2];
                    // boundaries check. Pretty much all we can do.
                    if (x >= LutDimensions || y >= LutDimensions || b >= LutMaxBrightness)
                    {
                        return false;
                    }
                }
                mixInfo.Type = MixContentType.VortexLut;
                mixInfo.Info = "Chrono vortex distortion effect lookup table";
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IdentifyWsa(Stream fileStream, MixEntry mixInfo)
        {
            try
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                long fileLength = mixInfo.Length;
                byte[] buffer = new byte[14];
                int headerLen = fileStream.Read(buffer, 0, buffer.Length);
                if (headerLen < 14)
                {
                    return false;
                }
                ushort nrOfFrames = ArrayUtils.ReadUInt16FromByteArrayLe(buffer, 0);
                if (nrOfFrames == 0)
                {
                    return false;
                }
                ushort xPos = ArrayUtils.ReadUInt16FromByteArrayLe(buffer, 2);
                ushort yPos = ArrayUtils.ReadUInt16FromByteArrayLe(buffer, 4);
                ushort xorWidth = ArrayUtils.ReadUInt16FromByteArrayLe(buffer, 6);
                ushort xorHeight = ArrayUtils.ReadUInt16FromByteArrayLe(buffer, 8);
                int buffSize = 2;
                uint deltaBufferSize = (uint)ArrayUtils.ReadIntFromByteArray(buffer, 0x0A, buffSize, true);
                ushort flags = ArrayUtils.ReadUInt16FromByteArrayLe(buffer, 0x0A + buffSize);
                int headerSize = 0x0C + buffSize;
                // Target resolution is maximum 320x200
                if (xorWidth == 0 || (xPos + xorWidth) > 320 || xorHeight == 0 || (yPos + xorHeight) > 200)
                {
                    return false;
                }
                int dataIndexOffset = headerSize;
                int paletteOffset = dataIndexOffset + (nrOfFrames + 2) * 4;
                bool hasPalette = (flags & 1) != 0;
                uint[] frameOffsets = new uint[nrOfFrames + 2];
                for (int i = 0; i < nrOfFrames + 2; ++i)
                {
                    if (fileLength <= dataIndexOffset + 4)
                    {
                        return false;
                    }
                    fileStream.Seek(dataIndexOffset, SeekOrigin.Begin);
                    fileStream.Read(buffer, 0, 4);
                    uint curOffs = ArrayUtils.ReadUInt32FromByteArrayLe(buffer, 0);
                    frameOffsets[i] = curOffs;
                    if (hasPalette)
                        curOffs += PalLength;
                    if (curOffs > fileLength)
                    {
                        return false;
                    }
                    dataIndexOffset += 4;
                }
                bool hasNoStart = frameOffsets[0] == 0;
                bool hasLoopFrame = frameOffsets[nrOfFrames + 1] != 0;
                uint endOffset = frameOffsets[nrOfFrames + (hasLoopFrame ? 1 : 0)];
                if (hasPalette)
                    endOffset += PalLength;
                if (endOffset != fileLength)
                {
                    return false;
                }
                if (hasPalette)
                {
                    if (fileLength < paletteOffset + PalLength)
                    {
                        return false;
                    }
                    fileStream.Seek(paletteOffset, SeekOrigin.Begin);
                    byte[] palData = new byte[PalLength];
                    fileStream.Read(palData, 0, PalLength);
                    for (int i = 0; i < PalLength; ++i)
                    {
                        // verify 6-bit palette
                        if (palData[i] > PalMax)
                        {
                            return false;
                        }
                    }
                }
                mixInfo.Type = MixContentType.Wsa;
                string posInfo = xPos != 0 && yPos != 0 ? string.Format(" at position {0}×{1}", xPos, yPos) : string.Empty;
                mixInfo.Info = string.Format("WSA Animation; {0}×{1}{2}, {3} frame{4}{5}{6}",
                    xorWidth, xorHeight, posInfo, nrOfFrames, 
                    nrOfFrames == 1 ? string.Empty : "s",
                    hasLoopFrame ? " + loop frame" : string.Empty,
                    hasNoStart ? ", missing start" : string.Empty);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IdentifyCcTmp(Stream fileStream, MixEntry mixInfo)
        {
            try
            {
                uint fileLen = mixInfo.Length;
                if (fileLen < 0x20)
                {
                    return false;
                }
                fileStream.Seek(0, SeekOrigin.Begin);
                long fileLength = mixInfo.Length;
                byte[] buffer = new byte[0x20];
                int headerLen = fileStream.Read(buffer, 0, buffer.Length);
                short hdrWidth = ArrayUtils.ReadInt16FromByteArrayLe(buffer, 0x00);
                short hdrHeight = ArrayUtils.ReadInt16FromByteArrayLe(buffer, 0x02);
                // Amount of icons to form the full icon set. Not necessarily the same as the amount of actual icons.
                short hdrCount = ArrayUtils.ReadInt16FromByteArrayLe(buffer, 0x04);
                // Always 0
                short hdrAllocated = ArrayUtils.ReadInt16FromByteArrayLe(buffer, 0x06);
                int hdrSize = ArrayUtils.ReadInt32FromByteArrayLe(buffer, 0x08);
                // Offset of start of actual icon data. Generally always 0x20
                int hdrIconsPtr = ArrayUtils.ReadInt32FromByteArrayLe(buffer, 0x0C);
                // Offset of start of palette data. Probably always 0.
                int hdrPalettesPtr = ArrayUtils.ReadInt32FromByteArrayLe(buffer, 0x10);
                // Offset of remaps data. Dune II leftover of 4 bit to 8 bit translation tables.
                // Always fixed value 0x0D1AFFFF, which makes no sense as ptr.
                int hdrRemapsPtr = ArrayUtils.ReadInt32FromByteArrayLe(buffer, 0x14);
                // Offset of 'transparency flags'? Generally points to an empty array at the end of the file.
                int hdrTransFlagPtr = ArrayUtils.ReadInt32FromByteArrayLe(buffer, 0x18);
                // Offset of actual icon set definition, defining for each index which icon data to use. FF for none.
                int hdrMapPtr = ArrayUtils.ReadInt32FromByteArrayLe(buffer, 0x1C);
                // File size check
                if (hdrSize != fileLen)
                {
                    return false;
                }
                // Only allowing standard 24x24 size
                if (hdrHeight != 24 || hdrWidth != 24)
                {
                    return false;
                }
                // Checking some normally hardcoded values
                if (hdrAllocated != 0 || hdrPalettesPtr != 0)// || hdrRemapsPtr != 0x0D1AFFFF)
                {
                    return false;
                }
                if (hdrCount == 0)
                {
                    return false;
                }
                // Checking if data is all inside the file
                if (hdrIconsPtr >= fileLen || (hdrMapPtr + hdrCount) > fileLen)
                {
                    return false;
                }
                mixInfo.Type = MixContentType.TmpTd;
                mixInfo.Info = string.Format("C&C Template; {0} tile{1}", hdrCount, hdrCount == 1 ? string.Empty : "s");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IdentifyRaTmp(Stream fileStream, MixEntry mixInfo)
        {
            try
            {
                const int headerSize = 0x28;
                fileStream.Seek(0, SeekOrigin.Begin);
                long fileLength = mixInfo.Length;
                if (fileLength < headerSize)
                {
                    return false;
                }
                byte[] buffer = new byte[headerSize];
                int headerLen = fileStream.Read(buffer, 0, buffer.Length);
                short hdrWidth = ArrayUtils.ReadInt16FromByteArrayLe(buffer, 0x00);
                short hdrHeight = ArrayUtils.ReadInt16FromByteArrayLe(buffer, 0x02);
                // Amount of icons to form the full icon set. Not necessarily the same as the amount of actual icons.
                short hdrCount = ArrayUtils.ReadInt16FromByteArrayLe(buffer, 0x04);
                // Always 0
                short hdrAllocated = ArrayUtils.ReadInt16FromByteArrayLe(buffer, 0x06);
                // New in RA
                int tilesX = ArrayUtils.ReadInt16FromByteArrayLe(buffer, 0x08);
                int tilesY = ArrayUtils.ReadInt16FromByteArrayLe(buffer, 0x0A);
                int hdrSize = ArrayUtils.ReadInt32FromByteArrayLe(buffer, 0x0C);
                // Offset of start of actual icon data. Generally always 0x20
                int hdrIconsPtr = ArrayUtils.ReadInt32FromByteArrayLe(buffer, 0x10);
                // Offset of start of palette data. Probably always 0.
                int hdrPalettesPtr = ArrayUtils.ReadInt32FromByteArrayLe(buffer, 0x14);
                // Offset of remaps data. Dune II leftover of 4 bit to 8 bit translation tables.
                // Always seems to be 0x2C730FXX (with values differing for the lowest byte), which makes no sense as ptr.
                int hdrRemapsPtr = ArrayUtils.ReadInt32FromByteArrayLe(buffer, 0x18);
                // Offset of 'transparency flags'? Generally points to an empty array at the end of the file.
                int hdrTransFlagPtr = ArrayUtils.ReadInt32FromByteArrayLe(buffer, 0x1C);
                // Offset of 'color' map, indicating the terrain type for each type. This includes unused cells, which are usually indicated as 0.
                int hdrColorMapPtr = ArrayUtils.ReadInt32FromByteArrayLe(buffer, 0x20);
                // Offset of actual icon set definition, defining for each index which icon data to use. FF for none.
                int hdrMapPtr = ArrayUtils.ReadInt32FromByteArrayLe(buffer, 0x24);
                // File size check
                if (hdrSize != fileLength)
                {
                    return false;
                }
                // Only allowing standard 24x24 size
                if (hdrHeight != 24 || hdrWidth != 24)
                {
                    return false;
                }
                // Checking some normally hardcoded values
                if (hdrAllocated != 0 || hdrPalettesPtr != 0)
                {
                    return false;
                }
                if (hdrCount == 0)
                {
                    return false;
                }
                // Checking if data is all inside the file
                if (hdrIconsPtr >= fileLength || (hdrMapPtr + hdrCount) > fileLength)
                {
                    return false;
                }
                int tileSize = hdrWidth * hdrHeight;
                // Maps the available images onto the full iconset definition
                byte[] map = new byte[hdrCount];
                fileStream.Seek(hdrMapPtr, SeekOrigin.Begin);
                fileStream.Read(map, 0, hdrCount);
                byte[] landTypesInfo = new byte[Math.Max(1, tilesX) * Math.Max(1, tilesY)];
                if (hdrMapPtr + landTypesInfo.Length > fileLength)
                {
                    return false;
                }
                fileStream.Seek(hdrColorMapPtr, SeekOrigin.Begin);
                fileStream.Read(landTypesInfo, 0, landTypesInfo.Length);
                // Get max index plus one for real images count. Nothing in the file header actually specifies this directly.
                int actualImages = map.Max(x => x == 0xff ? -1 : x) + 1;
                if (hdrTransFlagPtr + actualImages > fileLength)
                {
                    return false;
                }
                if (hdrIconsPtr + actualImages * tileSize > fileLength)
                {
                    return false;
                }
                string tilesInfo;
                if (tilesX == -1 && tilesY == -1)
                {
                    tilesInfo = string.Format("{0} images", actualImages);
                }
                else
                {
                    tilesInfo = string.Format("{0}×{1}", tilesX, tilesY);
                }
                mixInfo.Type = MixContentType.TmpRa;
                mixInfo.Info = "RA Template; " + tilesInfo;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IdentifyCcFont(byte[] fileContents, MixEntry mixInfo)
        {
            try
            {
                byte[][] shpData = ClassicSpriteLoader.GetCCFontData(fileContents, out int[] widths, out int height, false);
                if (shpData != null)
                {
                    mixInfo.Type = MixContentType.Font;
                    mixInfo.Info = string.Format("Font; {0} symbol{1}, {2}×{3}", shpData.Length, shpData.Length != 1 ? "s" : String.Empty, widths.Max(), height);
                    return true;
                }
            }
            catch (FileTypeLoadException) { /* ignore */ }
            return false;
        }

        private static bool IdentifyIni(byte[] fileContents, MixEntry mixInfo)
        {
            try
            {
                INI ini = new INI();
                Encoding encDOS = Encoding.GetEncoding(437);
                string iniText = encDOS.GetString(fileContents);
                string iniTextUtf = Encoding.UTF8.GetString(fileContents);
                // Always exclude anything containing 00 bytes.
                if (iniText.Contains("\0") && iniTextUtf.Contains('\0'))
                {
                    return false;
                }
                ini.Parse(iniText);
                if (ini.Sections.Count > 0 && ini.Sections.Any(s => s.Keys.Count > 0))
                {
                    // Plausible that it might indeed be an ini file.
                    if (INITools.CheckForIniInfo(ini, "Map") && INITools.CheckForIniInfo(ini, "Basic"))
                    {
                        // Likely that it is a C&C ini file.
                        INISection map = ini["Map"];
                        INISection bas = ini["Basic"];
                        string mapWidth = map.TryGetValue("Width") ?? "?";
                        string mapheight = map.TryGetValue("Height") ?? "?";
                        string mapTheater = map.TryGetValue("Theater") ?? "?";
                        string mapName = bas.TryGetValue("Name");
                        List<string> mapDesc = new List<string>();
                        mapDesc.Add(string.Format("; {0}×{1}", mapWidth, mapheight));
                        MixContentType mapType = MixContentType.MapTd;
                        if (SoleSurvivor.GamePluginSS.CheckForSSmap(ini))
                            mapType = MixContentType.MapSole;
                        else if (RedAlert.GamePluginRA.CheckForRAMap(ini))
                            mapType = MixContentType.MapRa;
                        mixInfo.Type = mapType;
                        IEnumerable<HouseType> houses = null;
                        IEnumerable<TheaterType> theaters = null;
                        switch (mapType)
                        {
                            case MixContentType.MapTd:
                                houses = TiberianDawn.HouseTypes.GetTypes();
                                theaters = TiberianDawn.TheaterTypes.GetTypes();
                                mixInfo.Info = "Tiberian Dawn Map";
                                break;
                            case MixContentType.MapSole:
                                theaters = SoleSurvivor.TheaterTypes.GetTypes();
                                mixInfo.Info = "Sole Survivor Map";
                                break;
                            case MixContentType.MapRa:
                                houses = RedAlert.HouseTypes.GetTypes();
                                theaters = RedAlert.TheaterTypes.GetTypes();
                                mixInfo.Info = "Red Alert Map";
                                break;
                        }
                        TheaterType theater = theaters.FirstOrDefault(th => th.Name.Equals(mapTheater, StringComparison.OrdinalIgnoreCase));
                        mapDesc.Add(theater != null ? theater.Name : mapTheater);
                        string mapDescr;
                        if (mapType != MixContentType.MapSole)
                        {
                            string mapPlayer = bas.TryGetValue("Player");
                            bool notMulti = mapPlayer != null && !mapPlayer.StartsWith("Multi", StringComparison.OrdinalIgnoreCase);
                            bool hasBrief = ini["Briefing"] != null && ini["Briefing"].Keys.Count > 0;
                            if (hasBrief || notMulti)
                            {
                                HouseType house = houses.FirstOrDefault(hs => hs.Name.Equals(mapPlayer, StringComparison.OrdinalIgnoreCase));
                                mapDesc.Add(house != null ? house.Name : mapPlayer);
                            }
                        }
                        mapDescr = string.Join(", ", mapDesc.ToArray());
                        if (!string.IsNullOrEmpty(mapName))
                        {
                            mapDescr += ": \"" + mapName + "\"";
                        }
                        mixInfo.Info += mapDescr;
                        return true;
                    }
                    else if (!ini.Sections.Any(s => s.Name.IndexOfAny(badIniHeaderRange) > 0
                        || s.Keys.Any(k => k.Key.IndexOfAny(badIniHeaderRange) > 0 || k.Value.IndexOfAny(badIniHeaderRange) > 0)))
                    {
                        mixInfo.Type = MixContentType.Ini;
                        mixInfo.Info = string.Format("INI file");
                        return true;
                    }
                }
            }
            catch { /* ignore */ }
            return false;
        }

        private static bool IdentifyStringsFile(byte[] fileContents, MixEntry mixInfo)
        {
            try
            {
                List<ushort> indices = new List<ushort>();
                List<byte[]> strings = GameTextManagerClassic.LoadFile(fileContents, indices, true);
                // Check bad text range, but make exceptions for single-char strings with the DOS ► ◄ ▲ ▼ symbols.
                bool hasBadChars = strings.Any(str => str.Any(b => badTextRange.Contains(b))
                                    && !(str.Length == 1 && (str[0] == 16 || str[0] == 17 || str[0] == 30 || str[0] == 31)));
                if (indices.Count > 0 && !hasBadChars && (indices[0] - indices.Count * 2) == 0 && strings.Any(s => s.Length > 0))
                {
                    mixInfo.Type = MixContentType.Strings;
                    mixInfo.Info = string.Format("Strings File; {0} entr{1}", strings.Count, strings.Count == 1 ? "y" : "ies");
                    return true;
                }
            }
            catch (ArgumentOutOfRangeException) { /* ignore */ }
            return false;
        }

        private static bool IdentifyText(byte[] fileContents, MixEntry mixInfo)
        {
            string text = null;
            try
            {
                UTF8Encoding encoding = new UTF8Encoding(false, true);
                // IF this succeeds, it fits the criteria for ASCII or UTF-8 text.
                text = encoding.GetString(fileContents).TrimStart('\r', '\n');
                // text contains characters in the 0-1F range of ASCII control characters. Don't know if UTF-8 complains about that, but it's not valid text.
                if (text.Any(b => badTextRange.Contains((byte)b)))
                {
                    text = null;
                }
                else if (text.Length > 0 && text[0] == '\uFEFF')
                {
                    // Remove BOM.
                    text = text.Substring(1);
                }
            }
            catch { /* ignore */ }
            if (text == null && fileContents.All(b => !badTextRange.Contains(b)))
            {
                // Fits the general criteria for extended-ascii type text.
                text = Encoding.GetEncoding(437).GetString(fileContents).TrimStart('\r', '\n');
            }
            if (text != null)
            {
                mixInfo.Type = MixContentType.Text;
                int cutoff = text.IndexOf('\n');
                if (cutoff < 0 || cutoff > 80)
                {
                    cutoff = Math.Min(80, text.Length);
                }
                mixInfo.Info = "Text file: \"" + text.Substring(0, cutoff).TrimEnd('\r', '\n') + "\"";
                return true;
            }
            return false;
        }

        private static bool IdentifyPalette(Stream fileStream, MixEntry mixInfo)
        {
            if (mixInfo.Length != PalLength)
            {
                return false;
            }
            byte[] fileContents = new byte[PalLength];
            fileStream.Seek(0, SeekOrigin.Begin);
            fileStream.Read(fileContents, 0, PalLength);
            if (fileContents.Any(b => b > PalMax))
            {
                return false;
            }
            mixInfo.Type = MixContentType.Pal;
            mixInfo.Info = "6-bit colour palette";
            return true;
        }

        private static bool IdentifyMix(MixFile source, MixEntry mixInfo)
        {
            try
            {
                // Check if it's a mix file
                if (MixFile.CheckValidMix(source, mixInfo, true))
                {
                    using (MixFile mf = new MixFile(source, mixInfo))
                    {
                        int mixContents = mf.FileCount;
                        bool encrypted = mf.HasEncryption;
                        bool newType = mf.IsNewFormat;
                        if (mixContents > 0)
                        {
                            mixInfo.Type = MixContentType.Mix;
                            string formatInfo = newType ? ("new format; " + (encrypted ? string.Empty : "not ") + "encrypted; ") : string.Empty;
                            mixInfo.Info = string.Format("Mix file; {0}{1} file{2}", formatInfo, mixContents, mixContents == 1 ? string.Empty : "s");
                            return true;
                        }
                    }
                }
            }
            catch { /* ignore */ }
            return false;
        }
        private static bool IdentifyAud(Stream fileStream, MixEntry mixInfo)
        {
            fileStream.Seek(0, SeekOrigin.Begin);
            long fileLength = mixInfo.Length;
            if (fileLength <= 12)
            {
                return false;
            }
            // AUD file:
            // 00  Int16 frequency
            // 02  Int32 Size
            // 06  Int32 outputSize
            // 0A  Byte flags
            // 0B  Byte compression
            // ----12 bytes
            byte[] header = new byte[12];
            byte[] chunk = new byte[8];
            fileStream.Read(header, 0, header.Length);
            int frequency = ArrayUtils.ReadUInt16FromByteArrayLe(header, 0);
            int fileSize = ArrayUtils.ReadInt32FromByteArrayLe(header, 2);
            int uncompressedSize = ArrayUtils.ReadInt32FromByteArrayLe(header, 6);
            int flags = header[10];
            bool isStereo = (flags & 1) != 0;
            bool is16Bit = (flags & 2) != 0;
            int compression = header[11];
            int ptr = 12;
            // Gonna need at least one chunk to confirm it's AUD, so don't accept 0 as size.
            if (fileSize == 0 || fileLength != fileSize + ptr || (compression != 01 && compression != 99))
            {
                return false;
            }
            int chunks = 0;
            int outputLength = 0;
            // This hops from chunk to chunk and builds up the total uncompressed size to check if the aud is valid.
            while (ptr < fileLength)
            {
                if (ptr + 8 > fileLength)
                {
                    // padded bytes? Don't allow.
                    return false;
                }
                fileStream.Seek(ptr, SeekOrigin.Begin);
                fileStream.Read(chunk, 0, chunk.Length);
                int chunkLength = ArrayUtils.ReadInt16FromByteArrayLe(chunk, 0);
                int chunkOutputLength = ArrayUtils.ReadInt16FromByteArrayLe(chunk, 2);
                int id = ArrayUtils.ReadInt32FromByteArrayLe(chunk, 4);
                // "DEAF", for an audio format. Someone had fun with that file design.
                if (id != 0x0000DEAF)
                {
                    return false;
                }
                chunks++;
                outputLength += chunkOutputLength;
                ptr += 8 + chunkLength;
                if (ptr > fileLength)
                {
                    // Current chunk exceeds file bounds; that's an error.
                    return false;
                }
            }
            if (uncompressedSize != outputLength)
            {
                return false;
            }
            mixInfo.Type = MixContentType.Audio;
            mixInfo.Info = string.Format("Audio file; {0} Hz, {1}-bit {2}, compression {3}, {4} chunks.",
                frequency, is16Bit ? 16 : 8, isStereo ? "stereo" : "mono", compression, chunks);
            return true;
        }

        private static bool IdentifyVqa(Stream fileStream, MixEntry mixInfo)
        {
            fileStream.Seek(0, SeekOrigin.Begin);
            // FORM chunk + VQHD chunk + FINF/LINF chunk name + its size
            const int vqHdrSize = 12 + 8 + 42;
            long fileLength = mixInfo.Length;
            if (fileLength <= vqHdrSize)
            {
                return false;
            }
            byte[] headerInfo = new byte[vqHdrSize];
            fileStream.Read(headerInfo, 0, vqHdrSize);
            string strForm = Encoding.ASCII.GetString(headerInfo, 0, 4);
            if (!"FORM".Equals(strForm))
            {
                return false;
            }
            // Always either the header length plus the following
            int formLength = ArrayUtils.ReadInt32FromByteArrayBe(headerInfo, 4);
            if (formLength < vqHdrSize)
            {
                return false;
            }
            string strWvqa = Encoding.ASCII.GetString(headerInfo, 8, 4);
            if (!"WVQA".Equals(strWvqa))
            {
                return false;
            }
            string strVqhd = Encoding.ASCII.GetString(headerInfo, 12, 4);
            if (!"VQHD".Equals(strVqhd))
            {
                return false;
            }
            int vqHeaderLength = ArrayUtils.ReadInt32FromByteArrayBe(headerInfo, 16);
            if (vqHeaderLength != 42)
            {
                return false;
            }
            // start of hdr
            int ptr = 20;
            int version = ArrayUtils.ReadUInt16FromByteArrayLe(headerInfo, ptr + 0);
            int numFrames = ArrayUtils.ReadUInt16FromByteArrayLe(headerInfo, ptr + 4);
            int width = ArrayUtils.ReadUInt16FromByteArrayLe(headerInfo, ptr + 6);
            int height = ArrayUtils.ReadUInt16FromByteArrayLe(headerInfo, ptr + 8);
            int frameRate = headerInfo[ptr + 12];
            int fullSeconds = numFrames / frameRate;
            int seconds = fullSeconds % 60;
            if ((numFrames / (double)frameRate) - fullSeconds >= 0.5 || (fullSeconds == 0 && numFrames > 0))
                seconds++;
            int fullMinutes = fullSeconds / 60;
            int minutes = fullMinutes % 60;
            int hours = fullMinutes / 60;
            string time = hours > 0 ? string.Format("{0}:{1:D2}:{2:D2}", hours, minutes, seconds) : string.Format("{0}:{1:D2}", minutes, seconds);
            mixInfo.Type = MixContentType.Vqa;
            mixInfo.Info = string.Format("Video file; VQA v{0}, {1}×{2}, {3}, {4}fps",
                version, width, height, time, frameRate);
            return true;
        }

        private static bool IdentifyVqp(Stream fileStream, MixEntry mixInfo)
        {
            fileStream.Seek(0, SeekOrigin.Begin);
            long fileLength = mixInfo.Length;
            if (fileLength <= 4)
            {
                return false;
            }
            byte[] headerInfo = new byte[4];
            fileStream.Read(headerInfo, 0, 4);
            int blocks = ArrayUtils.ReadInt32FromByteArrayLe(headerInfo, 0);

            if (fileLength != 4 + (32896 * blocks))
            {
                return false;
            }
            for (int i = 0; i < blocks; ++i)
            {
                int readAddress = 4 + i * 32896;
                fileStream.Seek(readAddress, SeekOrigin.Begin);
                // There is a structure in these tables; first it gives the closest match for [0,0], then for [1,0], [1,1],
                // then for [2,0], [2,1], [2,2], then for [3,0], [3,1], [3,2], [3,3], etc. This should mean all pairs with
                // identical index coords should match the index itself, but this is not true when the palette contains
                // duplicate colours, so a check on that won't actually work. But for the first one, it will always work.
                int cur = fileStream.ReadByte();
                if (cur != 0)
                {
                    return false;
                }
            }
            mixInfo.Type = MixContentType.Vqp;
            mixInfo.Info = string.Format("Video stretch table; {0} block{1}", blocks, blocks == 1 ? string.Empty : "s");
            return true;
        }

        private static bool IdentifyPalTable(Stream fileStream, MixEntry mixInfo)
        {
            const int tblSize = 65536;
            fileStream.Seek(0, SeekOrigin.Begin);
            long fileLength = mixInfo.Length;
            if (fileLength != tblSize)
            {
                return false;
            }
            byte[] table = new byte[tblSize];
            fileStream.Read(table, 0, tblSize);
            for (int y = 0; y < 256; ++y)
            {
                for (int x = 0; x < 256; ++x)
                {
                    // The 100% symmetrical structure makes it very easy to identify correctly.
                    if (table[(x << 8) + y] != table[(y << 8) + x])
                    {
                        return false;
                    }
                }
            }
            mixInfo.Type = MixContentType.PalTbl;
            mixInfo.Info = string.Format("Palette stretch table");
            return true;
        }

        public static bool IdentifyXccNames(Stream fileStream, MixEntry mixInfo)
        {
            byte[] fileContents = new byte[XccHeaderLength];
            fileStream.Seek(0, SeekOrigin.Begin);
            int amountRead = fileStream.Read(fileContents, 0, XccHeaderLength);
            if (amountRead != XccHeaderLength)
            {
                return false;
            }
            return IdentifyXccNames(fileContents, mixInfo);
        }

        private static bool IdentifyXccNames(byte[] fileContents, MixEntry mixInfo)
        {
            if (fileContents.Length < XccHeaderLength)
            {
                return false;
            }
            const string xccCheck = "XCC by Olaf van der Spek";
            byte[] xccPattern = Encoding.ASCII.GetBytes(xccCheck);
            for (int i = 0; i < xccPattern.Length; ++i)
            {
                if (fileContents[i] != xccPattern[i])
                {
                    return false;
                }
            }
            int fileSize = ArrayUtils.ReadInt32FromByteArrayLe(fileContents, 0x20);
            if (fileSize != mixInfo.Length)
            {
                return false;
            }
            int files = ArrayUtils.ReadInt32FromByteArrayLe(fileContents, 0x30);
            mixInfo.Type = MixContentType.XccNames;
            mixInfo.Info = string.Format("XCC filenames database ({0} file{1})", files, files == 1 ? string.Empty : "s");
            return true;
        }

        public static bool IdentifyRaMixNames(Stream fileStream, MixEntry mixInfo)
        {
            const string RaMixCheck = "RA-MIXer 5.1, (C) MoehrchenSoft, moehrchen@bigfoot.com";
            const int entrySize = 0x44;
            const int expectedHeaderSize = 0x100;
            int minsize = expectedHeaderSize + 8 + entrySize;
            byte[] raMixPattern = Encoding.ASCII.GetBytes(RaMixCheck);
            if (mixInfo.Length < minsize)
            {
                return false;
            }
            fileStream.Seek(0, SeekOrigin.Begin);
            byte[] fileContents = new byte[minsize];
            fileStream.Read(fileContents, 0, minsize);
            int headerSize = ArrayUtils.ReadInt16FromByteArrayLe(fileContents, 0);
            if (headerSize != expectedHeaderSize)
            {
                return false;
            }
            int files = ArrayUtils.ReadInt16FromByteArrayLe(fileContents, 2);
            int expectedFileSize = 2 + headerSize + 6 + files * entrySize;
            // check expected file length and pascal string length of header check string.
            if (mixInfo.Length != expectedFileSize || fileContents[4] != raMixPattern.Length)
            {
                return false;
            }
            // check actual string
            for (int i = 0; i < raMixPattern.Length; ++i)
            {
                if (fileContents[5+i] != raMixPattern[i])
                {
                    return false;
                }
            }
            mixInfo.Type = MixContentType.RaMixNames;
            mixInfo.Info = string.Format("RAMIX filenames database ({0} file{1})", files, files == 1 ? string.Empty : "s");
            return true;
        }

        private static bool IdentifyTdMap(Stream fileStream, MixEntry mixInfo)
        {
            const int TdMapSize = 8192;
            if (mixInfo.Length != TdMapSize)
            {
                return false;
            }
            int fileLength = (int)mixInfo.Length;
            fileStream.Seek(0, SeekOrigin.Begin);
            byte[] fileContents = new byte[fileLength];
            fileLength = fileStream.Read(fileContents, 0, fileLength);
            if (fileLength != TdMapSize)
            {
                return false;
            }
            for (int i = 0; i < TdMapSize; i += 2)
            {
                byte val = fileContents[i];
                if (val > HighestTdMapVal && val != 0xFF)
                {
                    return false;
                }
            }
            mixInfo.Type = MixContentType.Bin;
            mixInfo.Info = "Tiberian Dawn Map Tile Data (64×64)";
            return true;
        }

        private static bool IdentifySoleMap(Stream fileStream, MixEntry mixInfo)
        {
            const int maxCell = 0x4000; // 128 * 128;
            int fileLength = (int)mixInfo.Length;
            // 0x400 is chosen because it's larger than a colour palette, and
            // all official maps are about 0x4000; 10 times larger than that.
            if (fileLength % 4 != 0 || fileLength <= 0x400 || fileLength > 4 * maxCell)
            {
                return false;
            }
            byte[] fileContents = new byte[fileLength];
            fileStream.Seek(0, SeekOrigin.Begin);
            fileStream.Read(fileContents, 0, fileLength);
            // This scan assumes all cells get consecutively higher. Even if the format can work
            // with unordered cells, it should never actually happen, especially inside mix files.
            int previousCell = -1;
            for (int i = 0; i < fileLength; i += 4)
            {
                byte cellLow = fileContents[i];
                byte cellHi = fileContents[i + 1];
                int cell = (cellHi << 8) | cellLow;
                // The id of the tile on that cell.
                byte val = fileContents[i + 2];
                if (cell <= previousCell || cell >= maxCell || (val > HighestTdMapVal && val != 0xFF))
                {
                    return false;
                }
                previousCell = cell;
            }
            mixInfo.Type = MixContentType.BinSole;
            mixInfo.Info = "Tiberian Dawn / Sole Survivor Map Tile Data (128×128)";
            return true;
        }

        private static bool IdentifyMrf(Stream fileStream, MixEntry mixInfo)
        {
            const int mrfLen = 0x100;
            if (mixInfo.Length < mrfLen || mixInfo.Length % mrfLen != 0)
            {
                return false;
            }
            int blocks = (int)(mixInfo.Length / mrfLen);
            bool hasIndex = false;
            byte[] firstBlock = new byte[mrfLen];
            fileStream.Seek(0, SeekOrigin.Begin);
            fileStream.Read(firstBlock, 0, mrfLen);
            List<byte> indices = firstBlock.Where(b => b != 0xFF).ToList();
            int blocksNoIndex = blocks - 1;
            // If all non-FF values that are found in the first block are valid indices of other blocks in the file, it's considered valid.
            if (blocksNoIndex > 0 && indices.Count == blocksNoIndex && Enumerable.Range(0, blocksNoIndex).All(ind => indices.Contains((byte)ind)))
            {
                hasIndex = true;
            }
            mixInfo.Type = MixContentType.Mrf;
            int reportBlocks = hasIndex ? blocksNoIndex : blocks;
            mixInfo.Info = string.Format("Fading table{0} ({1} table{2})", hasIndex ? " with index" : string.Empty, reportBlocks, reportBlocks != 1 ? "s" : string.Empty);
            return true;
        }
    }

    public enum MixContentType
    {
        /// <summary>Dummy type used to indicate file entries identified through embedded file names database.</summary>
        DbTmp = -1,
        Unknown = 0,
        Empty,
        Mix,
        MapTd,
        MapRa,
        MapSole,
        Ini,
        Strings,
        Text,
        Bin,
        BinSole,
        ShpD2,
        ShpTd,
        TmpTd,
        TmpRa,
        Cps,
        VortexLut,
        Wsa,
        Font,
        Pcx,
        Bmp,
        Pal,
        PalTbl,
        Mrf,
        Audio,
        Vqa,
        Vqp,
        XccNames,
        RaMixNames,
    }
}
