using MobiusEditor.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
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
        static extern int StrCmpLogicalW(string x, string y);

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
        public static INI GetIniContents(string path, FileType fileType)
        {
            try
            {
                Encoding encDOS = Encoding.GetEncoding(437);
                String iniContents = null;
                switch (fileType)
                {
                    case FileType.INI:
                    case FileType.BIN:
                        string iniPath = fileType == FileType.INI ? path : Path.ChangeExtension(path, ".ini");
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
        /// <param name="iniText">The ini text, as string array.</param>
        /// <param name="writer">BinaryWriter to save to.</param>
        /// <param name="normalEncoding">Default encoding to use.</param>
        /// <param name="altEncoding">Alternate encoding to use.</param>
        /// <param name="toAltEncode">
        /// Pairs of (section, key) indicating which lines to use the alternate encoding for.
        /// Set "key" to null to write the entire section with the alternate encoding.
        /// </param>
        /// <param name="lineEnd">Bytes to use as line ends when writing the data to the writer.</param>
        public static void WriteMultiEncoding(string[] iniText, BinaryWriter writer, Encoding normalEncoding,
            Encoding altEncoding, (string, string)[] toAltEncode, Byte[] lineEnd)
        {
            Dictionary<string, bool> inSection = new Dictionary<string, bool>();
            foreach ((string section, string key) in toAltEncode)
            {
                inSection[section] = false;
            }
            Dictionary<string, List<Regex>> toTreat = new Dictionary<string, List<Regex>>();
            foreach ((string section, string key) in toAltEncode)
            {
                if (!toTreat.ContainsKey(section))
                {
                    toTreat[section] = key == null ? null : new List<Regex>();
                }
                // Any null item in the mapping will clear this and cause it to treat the whole section as alt encoded.
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
                    buffer = altEncoding.GetBytes(currLine);
                    writer.Write(buffer, 0, buffer.Length);
                    writer.Write(lineEnd, 0, lineEnd.Length);
                }
                else
                {
                    buffer = normalEncoding.GetBytes(currLine);
                    writer.Write(buffer, 0, buffer.Length);
                    writer.Write(lineEnd, 0, lineEnd.Length);
                }
            }
        }

        public static string MakeNew4CharName(IEnumerable<string> currentList, string fallback, params string[] reservedNames)
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
                            if (!currentList.Contains(name, StringComparer.InvariantCultureIgnoreCase) && !reservedNames.Contains(name, StringComparer.OrdinalIgnoreCase))
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

        /// <summary>
        /// ArgumentException messes with the Message property, and dumps its own extra (localised) bit of
        /// text with the argument onto the end of the message. This uses serialisation to retrieve the 
        /// original internal message of the exception without added junk.
        /// </summary>
        /// <param name="argex">The ArgumentException to retrieve the message from</param>
        /// <param name="fallback">True to construct a fallback message if the error message is empty.</param>
        /// <returns>The actual message given when the ArgumentException was created.</returns>
        public static string RecoverArgExceptionMessage(ArgumentException argex, Boolean fallback)
        {
            if (argex == null)
                return null;
            SerializationInfo info = new SerializationInfo(typeof(ArgumentException), new FormatterConverter());
            argex.GetObjectData(info, new StreamingContext(StreamingContextStates.Clone));
            string message = info.GetString("Message");
            if (!String.IsNullOrEmpty(message))
                return message;
            if (!fallback)
                return String.Empty;
            // Fallback: if no message, provide basic info.
            if (String.IsNullOrEmpty(argex.ParamName))
                return String.Empty;
            if (argex is ArgumentNullException)
                return String.Format("\"{0}\" is null.", argex.ParamName);
            if (argex is ArgumentOutOfRangeException)
                return String.Format("\"{0}\" out of range.", argex.ParamName);
            return argex.ParamName;
        }

        public static bool[,] GetMaskFromString(int width, int height, string maskString)
        {
            bool[,] mask = new bool[height, width];
            if (String.IsNullOrWhiteSpace(maskString))
            {
                mask.Clear(true);
                return mask;
            }
            int charIndex = 0;
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x, ++charIndex)
                {
                    // The format allows whitespace for clarity. Skip without consequence.
                    while (charIndex < maskString.Length && maskString[charIndex] == ' ')
                    {
                        charIndex++;
                    }
                    mask[y, x] = charIndex < maskString.Length && maskString[charIndex] != '0';
                }
            }
            return mask;
        }

        public static string GetStringFromMask(bool[,] mask)
        {
            int baseMaskY = mask.GetLength(0);
            int baseMaskX = mask.GetLength(1);
            StringBuilder occupyMask = new StringBuilder();
            int lastY = baseMaskY - 1;
            for (var y = 0; y < baseMaskY; ++y)
            {
                for (var x = 0; x < baseMaskX; ++x)
                {
                    occupyMask.Append(mask[y, x] ? 1 : 0);
                }
                // Not really needed, but eh, it's prettier.
                if (y < lastY)
                {
                    occupyMask.Append(" ");
                }
            }
            return occupyMask.ToString();
        }

        public static String ReplaceLinebreaks(String input, char replacement)
        {
            return input
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\\", "\\\\")
                .Replace("" + replacement, "\\" + replacement)
                .Replace('\n', replacement);
        }

        public static String RestoreLinebreaks(String input, char replacement, String lineBreak)
        {
            // Ensure there are no line breaks in the original string.
            input = Regex.Replace(input, "[\\r\\n]+", "", RegexOptions.None);
            StringBuilder sb = new StringBuilder();
            int len = input.Length;
            for (int i = 0; i < len; ++i)
            {
                char c = input[i];
                if (c == replacement)
                {
                    // Replace escaped line break with real one.
                    sb.Append(lineBreak);
                    continue;
                }
                else if (c == '\\')
                {
                    // Skip escape character.
                    i++;
                    if (i >= len)
                    {
                        // Must be broken; escape character as last character of the whole string.
                        sb.Append(c);
                        break;
                    }
                    // Add just the escaped character.
                    c = input[i];
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

    }
}
