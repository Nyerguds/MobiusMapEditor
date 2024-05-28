using MobiusEditor.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace MobiusEditor.Utility
{
    public static class MixContentAnalysis
    {
        private static readonly char[] badIniHeaderRange = Enumerable.Range(0, 0x20).Select(i => (char)i).ToArray();
        private static readonly HashSet<byte> badTextRange = Enumerable.Range(0, 0x20).Where(v => v != '\t' && v != '\r' && v != '\n' && v != ' ').Select(i => (byte)i).ToHashSet();
        /// <summary>Maximum file size that gets processed by byte array (5 MiB).</summary>
        private const uint maxProcessed = 0x500000;

        public static List<MixEntry> AnalyseFiles(MixFile current, bool preferMissions, Func<bool> checkAbort)
        {
            List<uint> filesList = current.FileIds.ToList();
            List<MixEntry> fileInfo = new List<MixEntry>();
            foreach (uint fileId in filesList)
            {
                if (checkAbort != null && checkAbort())
                {
                    return null;
                }
                MixEntry[] entries = current.GetFullFileInfo(fileId);
                for (int i = 0; i < entries.Length; ++i)
                {
                    MixEntry mixInfo = entries[i];
                    fileInfo.Add(mixInfo);
                    // Only do identification if type is set to unknown.
                    if (mixInfo.Type == MixContentType.Unknown)
                    {
                        using (Stream file = current.OpenFile(mixInfo))
                        {
                            TryIdentifyFile(file, mixInfo, current, preferMissions);
                        }
                    }
                }
            }
            return fileInfo.OrderBy(x => x.SortName).ToList();
        }

        private static void TryIdentifyFile(Stream fileStream, MixEntry mixInfo, MixFile source, bool preferMissions)
        {
            long fileLengthFull = fileStream.Length;
            mixInfo.Type = MixContentType.Unknown;
            string extension = Path.GetExtension(mixInfo.Name);
            if (fileLengthFull == 0)
            {
                mixInfo.Type = MixContentType.Empty;
                mixInfo.Info = "Empty file";
                return;
            }

            // Very strict requirements, and jumps over the majority of file contents while checking, so check this first.
            if (IdentifyAud(fileStream, mixInfo))
                return;
            if (IdentifyVqa(fileStream, mixInfo))
                return;
            if (IdentifyVqp(fileStream, mixInfo))
                return;
            if (IdentifyPal(fileStream, mixInfo))
                return;
            // These types analyse the full file from byte array. I'm restricting the buffer for them to 5mb; they shouldn't need more.
            if (fileLengthFull <= maxProcessed)
            {
                int fileLength = (int)fileLengthFull;
                Byte[] fileContents = new byte[fileLength];
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.Read(fileContents, 0, fileLength);
                fileStream.Seek(0, SeekOrigin.Begin);
                if (preferMissions)
                {
                    if (IdentifyIni(fileContents, mixInfo))
                        return;
                    if (IdentifyTdMap(fileContents, mixInfo))
                        return;
                    // Always needs to happen before sole maps since all palettes will technically match that format.
                    if (IdentifyPalette(fileContents, mixInfo))
                        return;
                    if (".bin".Equals(extension, StringComparison.OrdinalIgnoreCase) && IdentifySoleMap(fileContents, mixInfo))
                        return;
                }
                if (IdentifyXccNames(fileContents, mixInfo))
                    return;
                if (IdentifyRaMixNames(fileContents, mixInfo))
                    return;
                if (IdentifyPcx(fileContents, mixInfo))
                    return;
                if (IdentifyShp(fileContents, mixInfo))
                    return;
                if (IdentifyD2Shp(fileContents, mixInfo))
                    return;
                if (IdentifyCps(fileContents, mixInfo))
                    return;
                if (IdentifyWsa(fileContents, mixInfo))
                    return;
                if (IdentifyCcTmp(fileContents, mixInfo))
                    return;
                if (IdentifyRaTmp(fileContents, mixInfo))
                    return;
                if (IdentifyCcFont(fileContents, mixInfo))
                    return;
                if (!preferMissions && IdentifyIni(fileContents, mixInfo))
                    return;
                if (IdentifyStringsFile(fileContents, mixInfo))
                    return;
                if (IdentifyText(fileContents, mixInfo))
                    return;
                if (!preferMissions)
                {
                    if (IdentifyTdMap(fileContents, mixInfo))
                        return;
                    // Always needs to happen before sole maps since all palettes will technically match that format.
                    if (IdentifyPalette(fileContents, mixInfo))
                        return;
                    // Only check for sole map if name is known and type is correct.
                    if (".bin".Equals(extension, StringComparison.OrdinalIgnoreCase) && IdentifySoleMap(fileContents, mixInfo))
                        return;
                }
            }
            try
            {
                // Check if it's a mix file
                int mixContents = -1;
                bool encrypted = false;
                bool newType = false;
                if (MixFile.CheckValidMix(source, mixInfo, true))
                {
                    using (MixFile mf = new MixFile(source, mixInfo))
                    {
                        mixContents = mf.FileCount;
                        encrypted = mf.HasEncryption;
                        newType = mf.IsNewFormat;
                    }
                }
                if (mixContents > -1)
                {
                    mixInfo.Type = MixContentType.Mix;
                    mixInfo.Info = "Mix file; " + (newType ? ("new format; " + (encrypted ? string.Empty : "not ") + "encrypted; ") : string.Empty) + mixContents + " files.";
                    return;
                }
            }
            catch { /* ignore */ }
            // Only do this if it passes the check on extension.
            if (".mrf".Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                if (IdentifyMrf(fileStream, mixInfo))
                    return;
            }
            mixInfo.Type = MixContentType.Unknown;
            mixInfo.Info = String.Empty;
        }

        private static bool IdentifyShp(byte[] fileContents, MixEntry mixInfo)
        {
            try
            {
                Byte[][] shpData = ClassicSpriteLoader.GetCcShpData(fileContents, out int width, out int height);
                mixInfo.Type = MixContentType.ShpTd;
                mixInfo.Info = String.Format("C&C SHP; {0} frame{1}, {2}x{3}", shpData.Length, shpData.Length == 1 ? string.Empty : "s", width, height);
                return true;
            }
            catch (FileTypeLoadException) { /* ignore */ }
            return false;
        }

        private static bool IdentifyPcx(byte[] fileContents, MixEntry mixInfo)
        {
            try
            {
                if (fileContents.Length < 128)
                {
                    return false;
                }
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
                //UInt16 hDpi = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 12); // Horizontal Resolution of image in DPI
                //UInt16 vDpi = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 14); // Vertical Resolution of image in DPI
                byte numPlanes = fileContents[65]; // Number of color planes
                if (numPlanes > 4)
                {
                    return false;
                }
                ushort bytesPerLine = ArrayUtils.ReadUInt16FromByteArrayLe(fileContents, 66); // Number of bytes to allocate for a scanline plane.  MUST be an EVEN number.  Do NOT calculate from Xmax-Xmin.
                ushort paletteInfo = ArrayUtils.ReadUInt16FromByteArrayLe(fileContents, 68); // How to interpret palette: 1 = Color/BW, 2 = Grayscale (ignored in PB IV/ IV Plus)
                //UInt16 hscreenSize = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 70); // Horizontal screen size in pixels. New field found only in PB IV/IV Plus
                //UInt16 vscreenSize = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, 72); // Vertical screen size in pixels. New field found only in PB IV/IV Plus
                int width = windowXmax - windowXmin + 1;
                int height = windowYmax - windowYmin + 1;
                int bitsPerPixel = numPlanes * bitsPerPlane;

                if (bitsPerPixel != 1 && bitsPerPixel != 2 && bitsPerPixel != 4 && bitsPerPixel != 8 && bitsPerPixel != 24 && bitsPerPixel != 32)
                {
                    return false;
                }
                mixInfo.Type = MixContentType.Pcx;
                mixInfo.Info = String.Format("PCX Image; {0}x{1}, {2} bpp", width, height, bitsPerPixel);
                return true;
            }
            catch (Exception)
            {
                return false;
            }            
        }

        private static bool IdentifyD2Shp(byte[] fileContents, MixEntry mixInfo)
        {
            try
            {
                Byte[][] shpData = ClassicSpriteLoader.GetD2ShpData(fileContents, out int[] widths, out int[] heights);
                mixInfo.Type = MixContentType.ShpD2;
                mixInfo.Info = String.Format("Dune II SHP; {0} frame{1}, {2}x{3}", shpData.Length, shpData.Length == 1 ? string.Empty : "s", widths.Max(), heights.Max());
                return true;
            }
            catch (FileTypeLoadException) { /* ignore */ }
            return false;
        }

        private static bool IdentifyCps(byte[] fileContents, MixEntry mixInfo)
        {
            try
            {
                Byte[] cpsData = ClassicSpriteLoader.GetCpsData(fileContents, out Color[] palette);
                mixInfo.Type = MixContentType.Cps;
                mixInfo.Info = "CPS Image; 320x200";
                return true;
            }
            catch (FileTypeLoadException) { /* ignore */ }
            return false;
        }

        private static bool IdentifyWsa(byte[] fileContents, MixEntry mixInfo)
        {
            try
            {
                int datalen = fileContents.Length;
                if (datalen < 14)
                {
                    return false;
                }
                ushort nrOfFrames = ArrayUtils.ReadUInt16FromByteArrayLe(fileContents, 0);
                if (nrOfFrames == 0)
                {
                    return false;
                }
                ushort xPos = ArrayUtils.ReadUInt16FromByteArrayLe(fileContents, 2);
                ushort yPos = ArrayUtils.ReadUInt16FromByteArrayLe(fileContents, 4);
                ushort xorWidth = ArrayUtils.ReadUInt16FromByteArrayLe(fileContents, 6);
                ushort xorHeight = ArrayUtils.ReadUInt16FromByteArrayLe(fileContents, 8);
                int buffSize = 2;
                uint deltaBufferSize = (uint)ArrayUtils.ReadIntFromByteArray(fileContents, 0x0A, buffSize, true);
                ushort flags = ArrayUtils.ReadUInt16FromByteArrayLe(fileContents, 0x0A + buffSize);
                int headerSize = 0x0C + buffSize;
                if (xorWidth == 0 || xorHeight == 0)
                {
                    return false;
                }
                int dataIndexOffset = headerSize;
                int paletteOffset = dataIndexOffset + (nrOfFrames + 2) * 4;
                bool hasPalette = (flags & 1) != 0;
                uint[] frameOffsets = new uint[nrOfFrames + 2];
                for (int i = 0; i < nrOfFrames + 2; ++i)
                {
                    if (fileContents.Length <= dataIndexOffset + 4)
                    {
                        return false;
                    }
                    uint curOffs = ArrayUtils.ReadUInt32FromByteArrayLe(fileContents, dataIndexOffset);
                    frameOffsets[i] = curOffs;
                    if (hasPalette)
                        curOffs += 300;
                    if (curOffs > fileContents.Length)
                    {
                        return false;
                    }
                    dataIndexOffset += 4;
                }
                bool hasLoopFrame = frameOffsets[nrOfFrames + 1] != 0;
                uint endOffset = frameOffsets[nrOfFrames + (hasLoopFrame ? 1 : 0)];
                if (hasPalette)
                    endOffset += 0x300;
                if (endOffset != fileContents.Length)
                {
                    return false;
                }
                if (hasPalette)
                {
                    if (fileContents.Length < paletteOffset + 0x300)
                    {
                        return false;
                    }
                    int paletteEnd = paletteOffset + 0x300;
                    for (int i = paletteOffset; i < paletteEnd; i++)
                    {
                        // verify 6-bit palette
                        if (fileContents[i] > 0x3F)
                        {
                            return false;
                        }
                    }
                }
                mixInfo.Type = MixContentType.Wsa;
                string posInfo = xPos != 0 && yPos != 0 ? String.Format(" at position {0}x{1}", xPos, yPos) : String.Empty;
                mixInfo.Info = string.Format("WSA Animation; {0}x{1}{2}, {3} frame{4}{5}",
                    xorWidth, xorHeight, posInfo, nrOfFrames, nrOfFrames == 1 ? string.Empty : "s", hasLoopFrame ? " + loop frame" : string.Empty);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IdentifyCcTmp(byte[] fileContents, MixEntry mixInfo)
        {
            try
            {
                Byte[][] shpData = ClassicSpriteLoader.GetCcTmpData(fileContents, out int[] widths, out int[] heights);
                mixInfo.Type = MixContentType.TmpTd;
                mixInfo.Info = String.Format("C&C Template; {0} frame{1}", shpData.Length, shpData.Length == 1 ? string.Empty : "s");
                return true;
            }
            catch (FileTypeLoadException) { /* ignore */ }
            return false;
        }

        private static bool IdentifyRaTmp(byte[] fileContents, MixEntry mixInfo)
        {
            try
            {
                Byte[][] shpData = ClassicSpriteLoader.GetRaTmpData(fileContents, out int[] widths, out int[] heights, out byte[] landTypesInfo, out bool[] tileUseList, out int headerWidth, out int headerHeight);
                mixInfo.Type = MixContentType.TmpRa;
                mixInfo.Info = String.Format("RA Template; {0}x{1}", headerWidth, headerHeight);
                return true;
            }
            catch (FileTypeLoadException) { /* ignore */ }
            return false;
        }

        private static bool IdentifyCcFont(byte[] fileContents, MixEntry mixInfo)
        {
            try
            {
                Byte[][] shpData = ClassicSpriteLoader.GetCCFontData(fileContents, out int[] widths, out int height);
                mixInfo.Type = MixContentType.Font;
                mixInfo.Info = String.Format("Font; {0} symbols, {1}x{2}", shpData.Length, widths.Max(), height);
                return true;
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
                        mapDesc.Add(String.Format("; {0}x{1}", mapWidth, mapheight));
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
                        String mapDescr;
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
                        mapDescr = String.Join(", ", mapDesc.ToArray());
                        if (!String.IsNullOrEmpty(mapName))
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
                        mixInfo.Info = String.Format("INI file");
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
                    mixInfo.Info = String.Format("Strings File; {0} entries", strings.Count);
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

        private static bool IdentifyPalette(byte[] fileContents, MixEntry mixInfo)
        {
            if (fileContents.Length == 0x300 && fileContents.All(b => b < 0x40))
            {
                mixInfo.Type = MixContentType.Pal;
                mixInfo.Info = "6-bit colour palette";
                return true;
            }
            return false;
        }

        private static bool IdentifyAud(Stream fileStream, MixEntry mixInfo)
        {
            fileStream.Seek(0, SeekOrigin.Begin);
            long fileLength = fileStream.Length;
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
            fileStream.Seek(0, SeekOrigin.Begin);
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
                    fileStream.Seek(0, SeekOrigin.Begin);
                    return false;
                }
                chunks++;
                outputLength += chunkOutputLength;
                ptr += 8 + chunkLength;
                if (ptr > fileLength)
                {
                    // Current chunk exceeds file bounds; that's an error.
                    fileStream.Seek(0, SeekOrigin.Begin);
                    return false;
                }
            }
            fileStream.Seek(0, SeekOrigin.Begin);
            if (uncompressedSize != outputLength)
            {
                return false;
            }
            mixInfo.Type = MixContentType.Audio;
            mixInfo.Info = String.Format("Audio file; {0} Hz, {1}-bit {2}, compression {3}, {4} chunks.",
                frequency, is16Bit ? 16 : 8, isStereo ? "stereo" : "mono", compression, chunks);
            return true;
        }

        private static bool IdentifyVqa(Stream fileStream, MixEntry mixInfo)
        {
            fileStream.Seek(0, SeekOrigin.Begin);
            // FORM chunk + VQHD chunk + FINF/LINF chunk name + its size
            const int vqHdrSize = 12 + 8 + 42;
            long fileLength = fileStream.Length;
            if (fileLength <= vqHdrSize)
            {
                return false;
            }
            byte[] headerInfo = new byte[vqHdrSize];
            fileStream.Read(headerInfo, 0, vqHdrSize);
            fileStream.Seek(0, SeekOrigin.Begin);
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

            string time = hours > 0 ? String.Format("{0}:{1:D2}:{2:D2}", hours, minutes, seconds) : String.Format("{0}:{1:D2}", minutes, seconds);

            mixInfo.Type = MixContentType.Vqa;
            mixInfo.Info = String.Format("Video file; VQA v{0}, {1}x{2}, {3}, {4}fps",
                version, width, height, time, frameRate);
            return true;
        }

        private static bool IdentifyVqp(Stream fileStream, MixEntry mixInfo)
        {
            fileStream.Seek(0, SeekOrigin.Begin);
            long fileLength = fileStream.Length;
            if (fileLength <= 4)
            {
                return false;
            }
            byte[] headerInfo = new byte[4];
            fileStream.Read(headerInfo, 0, 4);
            int blocks = ArrayUtils.ReadInt32FromByteArrayLe(headerInfo, 0);

            if (fileLength != 4 + (32896 * blocks))
            {
                fileStream.Seek(0, SeekOrigin.Begin);
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
                    fileStream.Seek(0, SeekOrigin.Begin);
                    return false;
                }
            }
            fileStream.Seek(0, SeekOrigin.Begin);
            mixInfo.Type = MixContentType.Vqp;
            mixInfo.Info = String.Format("Video stretch table; {0} block{1}", blocks, blocks == 1 ? String.Empty : "s");
            return true;
        }

        private static bool IdentifyPal(Stream fileStream, MixEntry mixInfo)
        {
            const int tblSize = 65536;
            fileStream.Seek(0, SeekOrigin.Begin);
            long fileLength = fileStream.Length;
            if (fileLength != tblSize)
            {
                return false;
            }
            byte[] table = new byte[tblSize];
            fileStream.Read(table, 0, tblSize);
            fileStream.Seek(0, SeekOrigin.Begin);
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
            mixInfo.Info = String.Format("Palette stretch table");
            return true;
        }

        public static bool IdentifyXccNames(byte[] fileContents, MixEntry mixInfo)
        {
            const string xccCheck = "XCC by Olaf van der Spek";
            byte[] xccPattern = Encoding.ASCII.GetBytes(xccCheck);
            if (fileContents.Length <= 0x34)
            {
                return false;
            }
            for (int i = 0; i < xccPattern.Length; ++i)
            {
                if (fileContents[i] != xccPattern[i])
                {
                    return false;
                }
            }
            int fileSize = fileContents[0x20] | (fileContents[0x21] << 8) | (fileContents[0x22] << 16) | (fileContents[0x23] << 24);
            if (fileSize != mixInfo.Length)
            {
                return false;
            }
            int files = fileContents[0x30] | (fileContents[0x31] << 8) | (fileContents[0x32] << 16) | (fileContents[0x33] << 24);
            mixInfo.Type = MixContentType.XccNames;
            mixInfo.Info = String.Format("XCC filenames database ({0} file{1})", files, files == 1 ? String.Empty : "s");
            return true;
        }

        public static bool IdentifyRaMixNames(byte[] fileContents, MixEntry mixInfo)
        {
            const string RaMixCheck = "RA-MIXer 5.1, (C) MoehrchenSoft, moehrchen@bigfoot.com";
            const int entrySize = 0x44;
            const int expectedHeaderSize = 0x100;
            byte[] raMixPattern = Encoding.ASCII.GetBytes(RaMixCheck);
            if (fileContents.Length < expectedHeaderSize + 8 + entrySize)
            {
                return false;
            }
            int headerSize = fileContents[0] | (fileContents[1] << 8);
            if (headerSize != expectedHeaderSize)
            {
                return false;
            }
            int files = fileContents[2] | (fileContents[3] << 8);
            int expectedFileSize = 2 + headerSize + 6 + files * entrySize;
            // check expected file length and pascal string length of header check string.
            if (fileContents.Length != expectedFileSize || fileContents[4] != raMixPattern.Length)
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
            mixInfo.Info = String.Format("RAMIX filenames database ({0} file{1})", files, files == 1 ? String.Empty : "s");
            return true;
        }

        private static bool IdentifyTdMap(byte[] fileContents, MixEntry mixInfo)
        {
            int highestTdMapVal = TiberianDawn.TemplateTypes.GetTypes().Max(t => (int)t.ID);
            int fileLength = fileContents.Length;
            if (fileLength != 8192)
            {
                return false;
            }
            for (int i = 0; i < 8192; i += 2)
            {
                byte val = fileContents[i];
                if (val > highestTdMapVal && val != 0xFF)
                {
                    return false;
                }
            }
            mixInfo.Type = MixContentType.Bin;
            mixInfo.Info = "Tiberian Dawn 64x64 Map";
            return true;
        }

        private static bool IdentifySoleMap(byte[] fileContents, MixEntry mixInfo)
        {
            const int maxCell = 0x4000; // 128 * 128;
            int highestTdMapVal = TiberianDawn.TemplateTypes.GetTypes().Max(t => (int)t.ID);
            int fileLength = fileContents.Length;
            // 0x400 is chosen because it's larger than a colour palette, and
            // all official maps are about 0x4000; 10 times larger than that.
            if (fileLength % 4 != 0 || fileLength <= 0x400)
            {
                return false;
            }
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
                if (cell <= previousCell || cell >= maxCell || (val > highestTdMapVal && val != 0xFF))
                {
                    return false;
                }
                previousCell = cell;
            }
            mixInfo.Type = MixContentType.BinSole;
            mixInfo.Info = "Tiberian Dawn / Sole Survivor 128x128 Map";
            return true;
        }

        private static bool IdentifyMrf(Stream fileStream, MixEntry mixInfo)
        {
            const int mrfLen = 0x100;
            int blocks = (int)(mixInfo.Length / mrfLen);
            if (blocks > 0 && mixInfo.Length % mrfLen == 0)
            {
                bool hasIndex = false;
                byte[] firstBlock = new byte[mrfLen];
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.Read(firstBlock, 0, mrfLen);
                List<byte> indices = firstBlock.Where(b => b != 0xFF).ToList();
                int blocksNoIndex = blocks - 1;
                if (blocksNoIndex > 0 && indices.Count == blocksNoIndex && Enumerable.Range(0, blocksNoIndex).All(ind => indices.Contains((byte)ind)))
                {
                    hasIndex = true;
                }
                mixInfo.Type = MixContentType.Mrf;
                int reportBlocks = hasIndex ? blocksNoIndex : blocks;
                mixInfo.Info = String.Format("Fading table{0} ({1} table{2})", hasIndex ? " with index" : string.Empty, reportBlocks, reportBlocks != 1 ? "s" : string.Empty);
                return true;
            }
            return false;
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
        Wsa,
        Font,
        Pcx,
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
