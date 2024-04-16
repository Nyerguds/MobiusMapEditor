using MobiusEditor.Utility.Hashing;
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
        private static readonly char[] badIniHeaderRange = Enumerable.Range(0, 0x1F).Select(i => (char)i).ToArray();
        private static readonly HashSet<byte> badTextRange = Enumerable.Range(0, 0x1F).Where(v => v != '\t' && v != '\r' && v != '\n').Select(i => (byte)i).ToHashSet();
        private const String xccCheck = "XCC by Olaf van der Spek";
        private const uint xccId = 0x54C2D545;

        public static List<MixEntry> AnalyseFiles(MixFile current, Dictionary<uint, string> encodedFilenames, Func<bool> checkAbort)
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
                    if (fileId == xccId && entry.Length < 500000 && entry.Length > 0x34)
                    {
                        entry.Name = "local mix database.dat";
                        entry.Type = MixContentType.XccNames;
                        entry.Info = "XCC filenames database";
                        fileInfo.Add(entry);
                        using (Stream file = current.OpenFile(fileId))
                        {
                            Byte[] fileContents = new byte[entry.Length];
                            file.Seek(0, SeekOrigin.Begin);
                            file.Read(fileContents, 0, (int)entry.Length);
                            Byte[] xccPattern = Encoding.ASCII.GetBytes(xccCheck);
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
                                        for (endOffs = readOffs; endOffs < fileSize && fileContents[endOffs] != 0; ++endOffs);
                                        string filename = Encoding.ASCII.GetString(fileContents, readOffs, endOffs - readOffs);
                                        readOffs = endOffs + 1;
                                        xccInfoFilenames.Add(hasher.GetNameId(filename), filename);
                                    }
                                }
                            }
                            catch { /* ignore */ }
                        }
                        break;
                    }
                }
            }
            foreach (uint fileId in filesList)
            {
                MixEntry[] entries = current.GetFullFileInfo(fileId);
                for (int i = 0; i < entries.Length; ++i) {
                    MixEntry mixInfo = entries[i];
                    if (fileId == xccId && xccInfoFilenames != null)
                    {
                        // if the xcc info is filled in, this is already added
                        continue;
                    }
                    string name = null;
                    //uint fileIdm1 = fileId == 0 ? 0 : fileId - 1;
                    if (xccInfoFilenames == null || !xccInfoFilenames.TryGetValue(fileId, out name)) {
                        if (!encodedFilenames.TryGetValue(fileId, out name))
                        {
                            name = null;
                        }
                    }
                    if (name != null)
                    {
                        if (i > 0)
                        {
                            name += " (" + i + ")";
                        }
                        mixInfo.Name = name;
                    }
                    fileInfo.Add(mixInfo);
                    using (Stream file = current.OpenFile(fileId))
                    {
                        TryIdentifyFile(file, mixInfo, current);
                    }
                }
            }
            return fileInfo.OrderBy(x => x.SortName).ToList();
        }

        private static void TryIdentifyFile(Stream fileStream, MixEntry mixInfo, MixFile source)
        {
            long fileLengthFull = fileStream.Length;
            byte[] fileContents = null;
            int fileLength = 0;
            if (fileLengthFull < 500000)
            {
                fileLength = (int)fileLengthFull;
                fileContents = new byte[fileLength];
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.Read(fileContents, 0, fileLength);
                try
                {
                    Byte[][] shpData = ClassicSpriteLoader.GetCcShpData(fileContents, out int width, out int height);
                    mixInfo.Type = MixContentType.ShpTd;
                    mixInfo.Info = String.Format("C&C SHP; {0} frames, {1}x{2}", shpData.Length, width, height);
                    return;
                }
                catch (FileTypeLoadException) { /* ignore */ }
                try
                {
                    Byte[][] shpData = ClassicSpriteLoader.GetD2ShpData(fileContents, out int[] widths, out int[] heights);
                    mixInfo.Type = MixContentType.ShpD2;
                    mixInfo.Info = String.Format("Dune II SHP; {0} frames, {1}x{2}", shpData.Length, widths.Max(), heights.Max());
                    return;
                }
                catch (FileTypeLoadException) { /* ignore */ }
                try
                {
                    Byte[] cpsData = ClassicSpriteLoader.GetCpsData(fileContents, out Color[] palette);
                    mixInfo.Type = MixContentType.Cps;
                    mixInfo.Info = "CPS; 320x200";
                    return;
                }
                catch (FileTypeLoadException) { /* ignore */ }
                try
                {
                    Byte[][] shpData = ClassicSpriteLoader.GetCcTmpData(fileContents, out int[] widths, out int[] heights);
                    mixInfo.Type = MixContentType.TmpTd;
                    mixInfo.Info = String.Format("C&C Template; {0} frames", shpData.Length);
                    return;
                }
                catch (FileTypeLoadException) { /* ignore */ }
                try
                {
                    Byte[][] shpData = ClassicSpriteLoader.GetRaTmpData(fileContents, out int[] widths, out int[] heights, out byte[] landTypesInfo, out bool[] tileUseList, out int headerWidth, out int headerHeight);
                    mixInfo.Type = MixContentType.TmpRa;
                    mixInfo.Info = String.Format("RA Template; {0}x{1}", headerWidth, headerHeight);
                    return;
                }
                catch (FileTypeLoadException) { /* ignore */ }
                try
                {
                    Byte[][] shpData = ClassicSpriteLoader.GetCCFontData(fileContents, out int[] widths, out int height);
                    mixInfo.Type = MixContentType.Font;
                    mixInfo.Info = String.Format("Font; {0} symbols, {1}x{2}", shpData.Length, widths.Max(), height);
                    return;
                }
                catch (FileTypeLoadException) { /* ignore */ }
                try
                {
                    INI ini = new INI();
                    Encoding encDOS = Encoding.GetEncoding(437);
                    string iniText = encDOS.GetString(fileContents);
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
                            string mapDesc = String.Format("; {0}x{1}, {2}", mapWidth, mapheight, mapTheater);
                            if (SoleSurvivor.GamePluginSS.CheckForSSmap(ini))
                            {
                                mixInfo.Type = MixContentType.MapSole;
                                if (!String.IsNullOrEmpty(mapName))
                                {
                                    mapDesc += ": \"" + mapName + "\"";
                                }
                                mixInfo.Info = "Sole Map" + mapDesc;
                                return;
                            }
                            string mapPlayer = bas.TryGetValue("Player");
                            bool notMulti = mapPlayer != null && !mapPlayer.StartsWith("Multi", StringComparison.OrdinalIgnoreCase);
                            bool hasBrief = ini["Briefing"] != null && ini["Briefing"].Keys.Count > 0;
                            string mapDesc2 = (hasBrief || notMulti ? mapPlayer : String.Empty) + (String.IsNullOrEmpty(mapName) ? String.Empty : ": \"" + mapName + "\"");
                            if (mapDesc2.Length > 0)
                            {
                                mapDesc += ", " + mapDesc2;
                            }
                            if (RedAlert.GamePluginRA.CheckForRAMap(ini))
                            {
                                mixInfo.Type = MixContentType.MapRa;
                                mixInfo.Info = "RA Map" + mapDesc;
                                return;
                            }
                            mixInfo.Type = MixContentType.MapTd;
                            mixInfo.Info = "TD Map" + mapDesc;
                            return;
                        }
                        else if (!ini.Sections.Any(s => s.Name.IndexOfAny(badIniHeaderRange) > 0
                            || s.Keys.Any(k => k.Key.IndexOfAny(badIniHeaderRange) > 0 || k.Value.IndexOfAny(badIniHeaderRange) > 0)))
                        {
                            mixInfo.Type = MixContentType.Ini;
                            mixInfo.Info = String.Format("INI file (unknown type)");
                            return;
                        }
                    }
                }
                catch { /* ignore */ }
                try
                {
                    List<ushort> indices = new List<ushort>();
                    List<byte[]> strings = GameTextManagerClassic.LoadFile(fileContents, indices, true);
                    bool hasBadChars = strings.Any(str => str.Any(b => badTextRange.Contains(b)));
                    if (indices.Count > 0 && !hasBadChars && indices[0] - indices.Count * 2 == 0 && strings.Any(s => s.Length > 0))
                    {
                        mixInfo.Type = MixContentType.Strings;
                        mixInfo.Info = String.Format("Strings File; {0} entries", strings.Count);
                        return;
                    }
                }
                catch (ArgumentOutOfRangeException) { /* ignore */ }
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
                    return;
                }
                if (fileLength == 768 && fileContents.All(b => b < 0x3F))
                {
                    mixInfo.Type = MixContentType.Palette;
                    mixInfo.Info = "6-bit colour palette";
                }
                int highestTdMapVal = TiberianDawn.TemplateTypes.GetTypes().Max(t => (int)t.ID);

                if (fileLength == 8192)
                {
                    bool isMap = true;
                    for (int i = 0; i < 8192; i += 2)
                    {
                        byte val = fileContents[i];
                        if (val > highestTdMapVal && val != 0xFF)
                        {
                            isMap = false;
                            break;
                        }
                    }
                    if (isMap)
                    {
                        mixInfo.Type = MixContentType.Bin;
                        mixInfo.Info = "Tiberian Dawn / Sole Survivor 64x64 Map";
                        return;
                    }
                }
                // Probably gonna get mismatches on this, but whatev.
                if (fileLength % 4 == 0)
                {
                    bool isMap = true;
                    int maxCell = 128 * 128;
                    for (int i = 0; i < fileLength; i += 4)
                    {
                        byte cellLow = fileContents[i];
                        byte cellHi = fileContents[i + 1];
                        byte val = fileContents[i + 2];
                        int cell = (cellHi << 8) | cellLow;
                        if (cell >= maxCell || (val > highestTdMapVal && val != 0xFF))
                        {
                            isMap = false;
                            break;
                        }
                    }
                    if (isMap)
                    {
                        mixInfo.Type = MixContentType.BinSole;
                        mixInfo.Info = "Tiberian Dawn / Sole Survivor 128x128 Map";
                    }
                }
            }
            // File is either above 5 MB, or none of the above types.
            fileStream.Seek(0, SeekOrigin.Begin);
            try
            {
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
            // TODO identify as mix file
            mixInfo.Type = MixContentType.Unknown;
            mixInfo.Info = String.Empty;
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
        XccNames
    }


}
