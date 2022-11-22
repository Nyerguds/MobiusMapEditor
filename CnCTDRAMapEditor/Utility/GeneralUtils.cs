using MobiusEditor.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace MobiusEditor.Utility
{

    public class CustomComponentResourceManager : ComponentResourceManager
    {
        public CustomComponentResourceManager(Type type, string resourceName)
           : base(type)
        {
            this.BaseNameField = resourceName;
        }
    }

    public class ExplorerComparer : IComparer<string>
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        static extern int StrCmpLogicalW(String x, String y);

        public int Compare(string x, string y)
        {
            return StrCmpLogicalW(x, y);
        }
    }

    public static class GeneralUtils
    {

        /// <summary>
        /// Returns the contents of the ini, or null if no ini content could be found in the file.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="fileType">Detected file type.</param>
        /// <returns></returns>
        public static INI GetIniContents(String path, FileType fileType)
        {
            try
            {
                Encoding encDOS = Encoding.GetEncoding(437);
                String iniContents = null;
                switch (fileType)
                {
                    case FileType.INI:
                    case FileType.BIN:
                        String iniPath = fileType == FileType.INI ? path : Path.ChangeExtension(path, ".ini");
                        Byte[] bytes = File.ReadAllBytes(path);
                        iniContents = encDOS.GetString(bytes);
                        break;
                    case FileType.MEG:
                    case FileType.PGM:
                        using (var megafile = new Megafile(path))
                        {
                            Regex ext = new Regex("^\\.((ini)|(mpr))$");
                            var testIniFile = megafile.Where(p => ext.IsMatch(Path.GetExtension(p).ToLower())).FirstOrDefault();
                            if (testIniFile != null)
                            {
                                using (var iniReader = new StreamReader(megafile.Open(testIniFile), encDOS))
                                {
                                    iniContents = iniReader.ReadToEnd();
                                }
                            }
                        }
                        break;
                }
                if (iniContents == null)
                {
                    return null;
                }
                INI checkIni = new INI();
                checkIni.Parse(iniContents);
                return checkIni.Sections.Count == 0 ? null : checkIni;
            }
            catch
            {
                return null;
            }
        }

        public static byte[] ReadAllBytes(this BinaryReader reader)
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Allows writing an ini file where certain lines are differently encoded.
        /// </summary>
        /// <param name="iniText">ini text, as lines.</param>
        /// <param name="iniWriter">writer to save to.</param>
        /// <param name="normalEncoding">Default encoding to use.</param>
        /// <param name="altEncoding">Alternate encoding to use.</param>
        /// <param name="toAltEncode">Pairs of (section, key) indicating which lines to use the alternate encoding for. Set "key" to null to write the entire section with the alternate encoding.</param>
        /// <param name="lineEnd">Bytes to use as line ends.</param>
        public static void WriteMultiEncoding(string[] iniText, BinaryWriter iniWriter, Encoding normalEncoding, Encoding altEncoding,
            (string, string)[] toAltEncode, Byte[] lineEnd)
        {
            Dictionary<string, bool> inSection = new Dictionary<string, bool>();
            foreach ((string section, string key) in toAltEncode)
            {
                inSection[section] = false;
            }
            Dictionary<string, List<Regex>> toTreat = new Dictionary<String, List<Regex>>();
            foreach ((string section, string key) in toAltEncode)
            {
                if (!toTreat.ContainsKey(section))
                {
                    toTreat[section] = key == null ? null : new List<Regex>();
                }
                if (toTreat[section] != null)
                {
                    if (key == null)
                    {
                        toTreat[section] = null;
                    }
                    else
                    {
                        toTreat[section].Add(new Regex("^" + Regex.Escape(key) + "\\s*=", RegexOptions.IgnoreCase));
                    }
                }
            }
            byte[] buffer;
            for (int i = 0; i < iniText.Length; i++)
            {
                string currLine = iniText[i].Trim();
                bool foundAlt = false;
                if (currLine.Length > 0)
                {
                    if (currLine.StartsWith("["))
                    {
                        foreach (string key in toTreat.Keys)
                        {
                            inSection[key] = ("[" + key + "]").Equals(currLine, StringComparison.InvariantCultureIgnoreCase);
                        }
                    }
                    foreach (string section in toTreat.Keys)
                    {
                        if (!inSection[section])
                            continue;
                        foundAlt = toTreat[section] == null || toTreat[section].Any(keyStr => keyStr.IsMatch(currLine));
                        if (foundAlt)
                            break;
                    }
                }
                if (foundAlt)
                {
                    // Allow utf-8 name too, I guess. Not a fan, but necessary for the Remaster.
                    buffer = altEncoding.GetBytes(currLine);
                    iniWriter.Write(buffer, 0, buffer.Length);
                    iniWriter.Write(lineEnd, 0, lineEnd.Length);
                }
                else
                {
                    buffer = normalEncoding.GetBytes(currLine);
                    iniWriter.Write(buffer, 0, buffer.Length);
                    iniWriter.Write(lineEnd, 0, lineEnd.Length);
                }
            }
        }

        public static bool CheckForIniInfo(INI iniContents, string section)
        {
            return CheckForIniInfo(iniContents, section, null, null);
        }

        public static bool CheckForIniInfo(INI iniContents, string section, string key, string value)
        {
            INISection iniSection = iniContents[section];
            if (key == null || value == null)
            {
                return iniSection != null;
            }
            return iniSection != null && iniSection.Keys.Contains(key) && iniSection[key].Trim() == value;
        }

        public static String MakeNew4CharName(IEnumerable<string> currentList, string fallback, params string[] reservedNames)
        {
            string name = string.Empty;
            // generate names in a way that will never run out before some maximum is reached.
            for (int i = 'a'; i <= 'z'; ++i)
            {
                for (int j = 'a'; j <= 'z'; ++j)
                {
                    for (int k = 'a'; k <= 'z'; ++k)
                    {
                        for (int l = 'a'; l <= 'z'; ++l)
                        {
                            name = String.Concat((char)i, (char)j, (char)k, (char)l);
                            if (!currentList.Contains(name, StringComparer.InvariantCultureIgnoreCase) && !reservedNames.Contains(name, StringComparer.InvariantCultureIgnoreCase))
                            {
                                return name;
                            }
                        }
                    }
                }
            }
            return fallback;
        }

        public static string TrimRemarks(string value, bool trimResult, params char[] cutFrom)
        {
            if (String.IsNullOrEmpty(value))
                return value;
            int index = value.IndexOfAny(cutFrom);
            if (index == -1)
                return value;
            value = value.Substring(0, index);
            if (trimResult)
                value = value.TrimEnd();
            return value;
        }

        public static string AddRemarks(string value, string defaultVal, Boolean trimSource, IEnumerable<string> valuesToDetect, string remarkToAdd)
        {
            if (String.IsNullOrEmpty(value))
                return defaultVal;
            string valTrimmed = value;
            if (trimSource)
                valTrimmed = valTrimmed.Trim();
            foreach (string val in valuesToDetect)
            {
                if ((val ?? String.Empty).Trim().Equals(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    return valTrimmed + remarkToAdd;
                }
            }
            return value;
        }

        public static string FilterToExisting(string value, string defaultVal, Boolean trimSource, IEnumerable<string> existing)
        {
            if (String.IsNullOrEmpty(value))
                return defaultVal;
            string valTrimmed = value;
            if (trimSource)
                valTrimmed = valTrimmed.Trim();
            foreach (string val in existing)
            {
                if ((val ?? String.Empty).Trim().Equals(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    return value;
                }
            }
            return defaultVal;
        }

        public static StringBuilder TrimEnd(this StringBuilder sb, params char[] toTrim)
        {
            if (sb == null || sb.Length == 0) return sb;
            int i = sb.Length - 1;
            if (toTrim.Length > 0)
            {
                for (; i >= 0; --i)
                    if (!toTrim.Contains(sb[i]))
                        break;
            }
            else
            {
                for (; i >= 0; --i)
                    if (!char.IsWhiteSpace(sb[i]))
                        break;
            }
            if (i < sb.Length - 1)
                sb.Length = i + 1;
            return sb;
        }

        public static Rectangle GetBoundingBoxCenter(int imgWidth, int imgHeight, int maxWidth, int maxHeight)
        {
            double previewScaleW = (double)maxWidth / imgWidth;
            double previewScaleH = (double)maxHeight / imgHeight;
            bool maxIsW = previewScaleW < previewScaleH;
            double previewScale = maxIsW ? previewScaleW : previewScaleH;
            int width = (int)Math.Floor(imgWidth * previewScale);
            int height = (int)Math.Floor(imgHeight * previewScale);
            int offsetX = maxIsW ? 0 : (maxWidth - width) / 2;
            int offsetY = maxIsW ? (maxHeight - height) / 2 : 0;
            return new Rectangle(offsetX, offsetY, width, height);
        }
    }
}
