using MobiusEditor.Model;
using MobiusEditor.Utility.Hashing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace MobiusEditor.Utility
{
    public static class MixContentAnalysis
    {
        private static readonly char[] badIniHeaderRange = Enumerable.Range(0, 0x20).Select(i => (char)i).ToArray();
        private static readonly HashSet<byte> badTextRange = Enumerable.Range(0, 0x20).Where(v => v != '\t' && v != '\r' && v != '\n' && v != ' ').Select(i => (byte)i).ToHashSet();
        private const String xccCheck = "XCC by Olaf van der Spek";
        private const uint xccId = 0x54C2D545;
        private const uint maxProcessed = 0x500000;

        public static List<MixEntry> AnalyseFiles(MixFile current, Dictionary<uint, MixEntry> encodedFilenames, bool preferMissions, Func<bool> checkAbort)
        {
            List<uint> filesList = current.GetFileIds();
            List<MixEntry> fileInfo = new List<MixEntry>();
            Dictionary<uint, string> xccInfoFilenames = null;
            // Check if there's an xcc filenames database.
            foreach (uint fileId in filesList)
            {
                if (checkAbort != null && checkAbort())
                {
                    return null;
                }
                MixEntry[] entries = current.GetFullFileInfo(fileId);
                if (entries != null)
                {
                    MixEntry entry = entries[0];
                    if (fileId == xccId && entry.Length < maxProcessed && entry.Length > 0x34)
                    {
                        entry.Name = "local mix database.dat";
                        entry.Type = MixContentType.XccNames;
                        entry.Info = "XCC filenames database";
                        fileInfo.Add(entry);
                        byte[] fileContents = current.ReadFile(entry);
                        byte[] xccPattern = Encoding.ASCII.GetBytes(xccCheck);
                        try
                        {
                            bool isXccHeader = true;
                            for (int i = 0; i < xccPattern.Length; ++i)
                            {
                                if (fileContents[i] != xccPattern[i])
                                {
                                    isXccHeader = false;
                                    break;
                                }
                            }
                            int fileSize = 0;
                            if (isXccHeader)
                            {
                                fileSize = fileContents[0x20] | (fileContents[0x21] << 8) | (fileContents[0x22] << 16) | (fileContents[0x23] << 24);
                                if (fileSize != entry.Length)
                                {
                                    isXccHeader = false;
                                }
                            }
                            //int files = fileContents[0x30] | (fileContents[0x31] << 8) | (fileContents[0x32] << 16) | (fileContents[0x33] << 24);
                            if (isXccHeader)
                            {
                                xccInfoFilenames = new Dictionary<uint, string>();
                                int readOffs = 0x34;
                                HashRol1 hasher = new HashRol1();
                                while (readOffs < fileSize)
                                {
                                    int endOffs;
                                    for (endOffs = readOffs; endOffs < fileSize && fileContents[endOffs] != 0; ++endOffs) ;
                                    string filename = Encoding.ASCII.GetString(fileContents, readOffs, endOffs - readOffs);
                                    readOffs = endOffs + 1;
                                    xccInfoFilenames.Add(hasher.GetNameId(filename), filename);
                                }
                            }
                        }
                        catch { /* ignore */ }

                        break;
                    }
                }
            }
            // For testing: disable xcc mix info.
            //xccInfoFilenames = null;
            foreach (uint fileId in filesList)
            {
                MixEntry[] entries = current.GetFullFileInfo(fileId);
                for (int i = 0; i < entries.Length; ++i)
                {
                    MixEntry mixInfo = entries[i];
                    if (fileId == xccId && xccInfoFilenames != null)
                    {
                        // if the xcc info is filled in, this is already added
                        continue;
                    }
                    string name = null;
                    //uint fileIdm1 = fileId == 0 ? 0 : fileId - 1;
                    if (xccInfoFilenames != null && xccInfoFilenames.TryGetValue(fileId, out name))
                    {
                        mixInfo.Name = name;
                    }
                    MixEntry mi;
                    if (encodedFilenames.TryGetValue(fileId, out mi))
                    {
                        if (name == null)
                        {
                            mixInfo.Name = mi.Name;
                            mixInfo.Description = mi.Description;
                        }
                        else if (name.Equals(mi.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            // Don't apply description if xcc info name doesn't match encodedFilenames entry.
                            mixInfo.Description = mi.Description;
                        }
                    }
                    fileInfo.Add(mixInfo);
                    using (Stream file = current.OpenFile(fileId))
                    {
                        TryIdentifyFile(file, mixInfo, current, preferMissions);
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

            // Very strict requirements, and jumps over the majority of file contents while checking, so check this first.
            if (IdentifyAud(fileStream, mixInfo))
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
                if (IdentifyShp(fileContents, mixInfo))
                    return;
                if (IdentifyD2Shp(fileContents, mixInfo))
                    return;
                if (IdentifyCps(fileContents, mixInfo))
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
                using (MixFile mf = new MixFile(source, mixInfo))
                {
                    mixContents = mf.FileCount;
                    encrypted = mf.HasEncryption;
                    newType = mf.IsNewFormat;
                }
                if (mixContents > -1)
                {
                    mixInfo.Type = MixContentType.Mix;
                    mixInfo.Info = "Mix file; " + (newType ? ("new format; " + (encrypted ? string.Empty : "not ") + "encrypted; ") : string.Empty) + mixContents + " files.";
                    return;
                }
            }
            catch (Exception e) { /* ignore */ }
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
                mixInfo.Info = String.Format("C&C SHP; {0} frame{1}, {2}x{3}", shpData.Length, shpData.Length == 1? string.Empty : "s", width, height);
                return true;
            }
            catch (FileTypeLoadException) { /* ignore */ }
            return false;
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
                mixInfo.Info = "CPS; 320x200";
                return true;
            }
            catch (FileTypeLoadException) { /* ignore */ }
            return false;
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
                                mixInfo.Info = "TD Map";
                                break;
                            case MixContentType.MapSole:
                                theaters = SoleSurvivor.TheaterTypes.GetTypes();
                                mixInfo.Info = "Sole Map";
                                break;
                            case MixContentType.MapRa:
                                houses = RedAlert.HouseTypes.GetTypes();
                                theaters = RedAlert.TheaterTypes.GetTypes();
                                mixInfo.Info = "RA Map";
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
                mixInfo.Type = MixContentType.Palette;
                mixInfo.Info = "6-bit colour palette";
                return true;
            }
            return false;
        }

        private static bool IdentifyAud(Stream fileStream, MixEntry mixInfo)
        {
            long fileLength = fileStream.Length;
            if (fileLength <= 12)
            {
                return false;
            }
            // File is either above 5 MB, or none of the above types.
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
            // Gonna need at least one DEAF sequence to confirm it's AUD.
            if (fileSize == 0 || fileLength != fileSize + ptr || (compression != 01 && compression != 99))
            {
                return false;
            }
            int chunks = 0;
            int outputLength = 0;
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
                if (id != 0x0000DEAF)
                {
                    return false;
                }
                chunks++;
                outputLength += chunkOutputLength;
                ptr += 8 + chunkLength;
            }
            if (uncompressedSize != outputLength)
            {
                return false;
            }
            mixInfo.Type = MixContentType.Audio;
            mixInfo.Info = String.Format("Audio file; {0} Hz, {1}-bit {2}, compression {3}, {4} chunks.",
                frequency, is16Bit ? 16 : 8, isStereo ? "stereo" : "mono", compression, chunks);
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
            int highestTdMapVal = TiberianDawn.TemplateTypes.GetTypes().Max(t => (int)t.ID);
            int fileLength = fileContents.Length;
            if (fileLength % 4 != 0)
            {
                return false;
            }
            int maxCell = 128 * 128;
            for (int i = 0; i < fileLength; i += 4)
            {
                byte cellLow = fileContents[i];
                byte cellHi = fileContents[i + 1];
                byte val = fileContents[i + 2];
                int cell = (cellHi << 8) | cellLow;
                if (cell >= maxCell || (val > highestTdMapVal && val != 0xFF))
                {
                    return false;
                }
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
                mixInfo.Type = MixContentType.Remap;
                int reportBlocks = hasIndex ? blocksNoIndex : blocks;
                mixInfo.Info = String.Format("Fading table{0} ({1} table{2})", hasIndex ? " with index" : string.Empty, reportBlocks, reportBlocks != 1 ? "s" : string.Empty);
                return true;
            }
            return false;
        }
    }

    public enum MixContentType
    {
        Unknown,
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
        Palette,
        PalTbl,
        Remap,
        Audio,
        XccNames
    }


}
