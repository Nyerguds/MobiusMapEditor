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
using MobiusEditor.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
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
        /// Returns the contents of the ini, or null if no ini content could be found in the file or any accompanying files.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="fileType">Detected file type.</param>
        /// <returns></returns>
        public static INI GetIniContents(string path, FileType fileType)
        {
            try
            {
                Encoding encDOS = Encoding.GetEncoding(437);
                byte[] bytes;
                string iniContents = null;
                switch (fileType)
                {
                    case FileType.INI:
                        bytes = File.ReadAllBytes(path);
                        iniContents = encDOS.GetString(bytes);
                        break;
                    case FileType.BIN:
                        string iniPath = fileType == FileType.INI ? path : Path.ChangeExtension(path, ".ini");
                        if (File.Exists(iniPath))
                        {
                            bytes = File.ReadAllBytes(iniPath);
                            iniContents = encDOS.GetString(bytes);
                        }
                        break;
                    case FileType.MEG:
                    case FileType.PGM:
                        using (var megafile = new Megafile(path))
                        {
                            Regex ext = new Regex("^\\.((ini)|(mpr))$");
                            string testIniFile = megafile.Where(p => ext.IsMatch(Path.GetExtension(p).ToLower())).FirstOrDefault();
                            if (testIniFile != null)
                            {
                                using (StreamReader iniReader = new StreamReader(megafile.OpenFile(testIniFile), encDOS))
                                {
                                    iniContents = iniReader.ReadToEnd();
                                }
                            }
                        }
                        break;
                    case FileType.MIX:
                        bytes = MixPath.ReadFile(path, FileType.INI, out _);
                        if (bytes != null)
                        {
                            iniContents = encDOS.GetString(bytes);
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

        /// <summary>
        /// Reads all remaining bytes from a stream behind the current Position. This does not close the stream.
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>The contents of the stream, behind the current Position.</returns>
        public static byte[] ReadAllBytes(this Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream, new UTF8Encoding(), true))
            {
                return ReadAllBytes(reader);
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

        public static string DoubleAmpersands(string input)
        {
            if (input == null)
            {
                return null;
            }
            if (!input.Contains('&'))
            {
                return input;
            }
            return Regex.Replace(input, "&", "&&");
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
            Encoding altEncoding, (string section, string key)[] toAltEncode, byte[] lineEnd)
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
                            name = string.Concat((char)i, (char)j, (char)k, (char)l);
                            if (!currentList.Contains(name, StringComparer.OrdinalIgnoreCase) && !reservedNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                            {
                                return name;
                            }
                        }
                    }
                }
            }
            return fallback;
        }

        /// <summary>
        /// Removes the end of the given string starting from the point at which a character from the given characters array is encountered.
        /// </summary>
        /// <param name="value">Value to trim.</param>
        /// <param name="trimResult">True to trim spaces off the resulting string.</param>
        /// <param name="cutFrom">characters that indicate the remark started.</param>
        /// <returns></returns>
        public static string TrimRemarks(string value, bool trimResult, params char[] cutFrom)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            int index = value.IndexOfAny(cutFrom);
            if (index == -1)
                return value;
            value = value.Substring(0, index);
            if (trimResult)
                value = value.TrimEnd();
            return value;
        }


        /// <summary>
        /// Adds a remark to the end of the given value if it is one of the values inside the given list.
        /// If no remark is added, the original value is returned.
        /// </summary>
        /// <param name="value">Value to evaluate.</param>
        /// <param name="defaultVal">If <paramref name="value"/> is empty, this value is returned.</param>
        /// <param name="trimSource">True to trim the source before adding the remark. This does not affect the list check, only the output.</param>
        /// <param name="valuesToDetect">List of values to check to see if the remark should be added. Case-insensitive.</param>
        /// <param name="remarkToAdd">Remark to add if <paramref name="value"/> matches the criteria.</param>
        /// <returns>The value string, possibly with the remark added.</returns>
        public static string AddRemarks(string value, string defaultVal, bool trimSource, IEnumerable<string> valuesToDetect, string remarkToAdd)
        {
            return AddRemarks(value, defaultVal, trimSource, valuesToDetect, remarkToAdd, false, out _);
        }


        /// <summary>
        /// Adds a remark to the end of the given value if it is one of the values inside the given list.
        /// If no remark is added, the original value is returned.
        /// </summary>
        /// <param name="value">Value to evaluate.</param>
        /// <param name="defaultVal">If <paramref name="value"/> is empty, this value is returned.</param>
        /// <param name="trimSource">True to trim the source before adding the remark. This does not affect the list check, only the output.</param>
        /// <param name="valuesToDetect">List of values to check to see if the remark should be added. Case-insensitive.</param>
        /// <param name="remarkToAdd">Remark to add if <paramref name="value"/> matches the criteria.</param>
        /// <param name="changed">returns true if the remark was added.</param>
        /// <returns>The value string, possibly with the remark added.</returns>
        public static string AddRemarks(string value, string defaultVal, bool trimSource, IEnumerable<string> valuesToDetect, string remarkToAdd, out bool changed)
        {
            return AddRemarks(value, defaultVal, trimSource, valuesToDetect, remarkToAdd, false, out changed);
        }


        /// <summary>
        /// Adds a remark to the end of the given value if it is one of the values inside the given list. If <paramref name="negativeCheck"/> is enabled,
        /// the remark is only added if the value does not appear in the list. If no remark is added, the original value is returned.
        /// </summary>
        /// <param name="value">Value to evaluate.</param>
        /// <param name="defaultVal">If <paramref name="value"/> is empty, this value is returned.</param>
        /// <param name="trimSource">True to trim the source before adding the remark. This does not affect the list check, only the output.</param>
        /// <param name="valuesToDetect">List of values to check to see if the remark should be added. Case-insensitive.</param>
        /// <param name="remarkToAdd">Remark to add if <paramref name="value"/> matches the criteria.</param>
        /// <param name="negativeCheck">True to only add the remark if <paramref name="value"/> does <b>not</b> occur in <paramref name="valuesToDetect"/>.</param>
        /// <returns>The value string, possibly with the remark added.</returns>
        public static string AddRemarks(string value, string defaultVal, bool trimSource, IEnumerable<string> valuesToDetect, string remarkToAdd, bool negativeCheck)
        {
            return AddRemarks(value, defaultVal, trimSource, valuesToDetect, remarkToAdd, negativeCheck, out _);
        }

        /// <summary>
        /// Adds a remark to the end of the given value if it is one of the values inside the given list. If <paramref name="negativeCheck"/> is enabled,
        /// the remark is only added if the value does not appear in the list. If no remark is added, the original value is returned.
        /// </summary>
        /// <param name="value">Value to evaluate.</param>
        /// <param name="defaultVal">If <paramref name="value"/> is empty, this value is returned.</param>
        /// <param name="trimSource">True to trim the source before adding the remark. This does not affect the list check, only the output.</param>
        /// <param name="valuesToDetect">List of values to check to see if the remark should be added. Case-insensitive.</param>
        /// <param name="remarkToAdd">Remark to add if <paramref name="value"/> matches the criteria.</param>
        /// <param name="negativeCheck">True to only add the remark if <paramref name="value"/> does <b>not</b> occur in <paramref name="valuesToDetect"/>.</param>
        /// <param name="changed">returns true if the remark was added.</param>
        /// <returns>The value string, possibly with the remark added.</returns>
        public static string AddRemarks(string value, string defaultVal, bool trimSource, IEnumerable<string> valuesToDetect, string remarkToAdd, bool negativeCheck, out bool changed)
        {
            changed = false;
            if (string.IsNullOrEmpty(value))
                return defaultVal;
            string valTrimmed = value;
            if (trimSource)
                valTrimmed = valTrimmed.Trim();
            bool found = false;
            if (valuesToDetect != null)
            {
                foreach (string val in valuesToDetect)
                {
                    if (val == null || !val.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                    found = true;
                    break;
                }
            }
            if ((!negativeCheck && found) || (negativeCheck && !found))
            {
                changed = true;
                return valTrimmed + remarkToAdd;
            }
            return value;
        }

        public static string FilterToExisting(string value, string defaultVal, bool trimSource, IEnumerable<string> existing)
        {
            if (string.IsNullOrEmpty(value))
                return defaultVal;
            string valTrimmed = value;
            if (trimSource)
                valTrimmed = valTrimmed.Trim();
            foreach (string val in existing)
            {
                if ((val ?? string.Empty).Trim().Equals(value, StringComparison.InvariantCultureIgnoreCase))
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

        /// <summary>
        /// Returns a string array that contains the substrings in this string that are
        /// delimited by a specified Unicode character. A parameter specifies whether
        /// to return empty array elements.
        /// </summary>
        /// <param name="str">Source string</param>
        /// <param name="separator">A Unicode character that delimits the substrings in this string</param>
        /// <param name="options">Specify System.StringSplitOptions.RemoveEmptyEntries to omit empty array elements from the array returned, or System.StringSplitOptions.None to include empty array elements in the array returned.</param>
        /// <returns>An array whose elements contain the substrings in this string that are delimited by one or more characters in separator.</returns>
        /// <exception cref="System.ArgumentException">options is not one of the System.StringSplitOptions values.</exception>
        public static string[] Split(this string str, char separator, StringSplitOptions options)
        {
            return str.Split(new char[] { separator }, options);
        }

        /// <summary>
        /// Returns a string array that contains the substrings in this string that are delimited
        /// by a specified Unicode character. Additional parameters specify whether to return
        /// empty array elements, and whether to do a simple whitespace trim on all elements.
        /// If trimming and removing empty elements are both enabled, elements found to be empty
        /// after the trim are also removed.
        /// </summary>
        /// <param name="str">Source string</param>
        /// <param name="separator">A Unicode character that delimits the substrings in this string</param>
        /// <param name="options">Specify System.StringSplitOptions.RemoveEmptyEntries to omit empty array elements from the array returned, or System.StringSplitOptions.None to include empty array elements in the array returned.</param>
        /// <param name="trimContents">True to trim the contents of each split part.</param>
        /// <returns>An array whose elements contain the substrings in this string that are delimited by one or more characters in separator.</returns>
        /// <exception cref="System.ArgumentException">options is not one of the System.StringSplitOptions values.</exception>
        public static string[] Split(this string str, char separator, StringSplitOptions options, bool trimContents)
        {
            return str.Split(new char[] { separator }, options, trimContents);
        }

        /// <summary>
        /// Returns a string array that contains the substrings in this string that are delimited
        /// by a specified Unicode character. A simple whitespace trim can be done on all elements.
        /// </summary>
        /// <param name="str">Source string</param>
        /// <param name="separator">A Unicode character that delimits the substrings in this string</param>
        /// <param name="trimContents">True to trim the contents of each split part.</param>
        /// <returns>An array whose elements contain the substrings in this string that are delimited by one or more characters in separator.</returns>
        /// <exception cref="System.ArgumentException">options is not one of the System.StringSplitOptions values.</exception>
        public static string[] Split(this string str, char separator, bool trimContents)
        {
            return str.Split(new char[] { separator }, StringSplitOptions.None, trimContents);
        }

        /// <summary>
        /// Returns a string array that contains the substrings in this string that are delimited by
        /// Unicode characters specified in the given array.  A simple whitespace trim can be done
        /// on all elements.
        /// </summary>
        /// <param name="str">Source string</param>
        /// <param name="separators">An array of Unicode characters that delimits the substrings in this string</param>
        /// <param name="trimContents">True to trim the contents of each split part.</param>
        /// <returns>An array whose elements contain the substrings in this string that are delimited by one or more characters in separator.</returns>
        /// <exception cref="System.ArgumentException">options is not one of the System.StringSplitOptions values.</exception>
        public static string[] Split(this string str, char[] separators, bool trimContents)
        {
            return str.Split(separators, StringSplitOptions.None, trimContents);
        }

        /// <summary>
        ///     Returns a string array that contains the substrings in this string that are delimited by
        ///     Unicode characters specified in the given array. Additional parameters specify whether to
        ///     return empty array elements, and whether to do a simple whitespace trim on all elements.
        ///     If trimming and removing empty elements are both enabled, elements found to be empty
        ///     after the trim are also removed.
        /// </summary>
        /// <param name="str">Source string</param>
        /// <param name="separators">An array of Unicode characters that delimits the substrings in this string</param>
        /// <param name="options">Specify System.StringSplitOptions.RemoveEmptyEntries to omit empty array elements from the array returned, or System.StringSplitOptions.None to include empty array elements in the array returned.</param>
        /// <param name="trimContents">True to trim the contents of each split part. In combination with RemoveEmptyEntries this will also remove any entries that become empty after trimming.</param>
        /// <returns>An array whose elements contain the substrings in this string that are delimited by one or more characters in separator.</returns>
        /// <exception cref="System.ArgumentException">options is not one of the System.StringSplitOptions values.</exception>
        public static string[] Split(this string str, char[] separators, StringSplitOptions options, bool trimContents)
        {
            string[] split = str.Split(separators, options);
            if (!trimContents)
            {
                return split;
            }
            if (options != StringSplitOptions.RemoveEmptyEntries)
            {
                for (int i = 0; i < split.Length; i++)
                    split[i] = split[i].Trim();
                return split;
            }
            // code to remove additional empty entries after trim.
            int actualIndex = 0;
            for (int i = 0; i < split.Length; i++)
            {
                split[actualIndex] = split[i].Trim();
                if (split[actualIndex].Length > 0)
                    actualIndex++;
            }
            if (actualIndex < split.Length)
            {
                string[] split2 = new string[actualIndex];
                if (actualIndex > 0)
                    Array.Copy(split, split2, actualIndex);
                split = split2;
            }
            return split;
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
        /// Gets the actual bounds box to use on an image, based on a cell number, a size in cells, a cell-based radius to extend
        /// from the center cell, and the tile size. Note that this calculates a deliberately rounded-down "center cell", as the game
        /// does it, and then centers the box on that cell, so this does not necessarily match the actual center of the building.
        /// </summary>
        /// <param name="cell">The top left cell of the object</param>
        /// <param name="cellsWide">How wide the object is, in cells. Should be at least 1.</param>
        /// <param name="cellsHigh">How high the object is, in cells. Should be at least 1.</param>
        /// <param name="radiusX">Cell radius in X-direction from the center of the object.</param>
        /// <param name="radiusY">Cell radius in Y-direction from the center of the object.</param>
        /// <param name="tileSize">Tile size to translate cells to actual dimensions.</param>
        /// <returns>Bounds for the radius around the object.</returns>
        public static Rectangle GetBoxFromCenterCell(Point cell, int cellsWide, int cellsHigh, double radiusX, double radiusY, Size tileSize)
        {
            return GetBoxFromCenterCell(cell, cellsWide, cellsHigh, radiusX, radiusY, tileSize, out _);
        }

        /// <summary>
        /// Gets the actual bounds box to use on an image, based on a cell number, a size in cells, a cell-based radius to extend
        /// from the center cell, and the tile size. Note that this calculates a deliberately rounded-down "center cell", as the game
        /// does it, and then centers the box on that cell, so this does not necessarily match the actual center of the building.
        /// </summary>
        /// <remarks>This function is used for indicating the area-of-effect circle of special abilities.</remarks>
        /// <param name="cell">The top left cell of the object</param>
        /// <param name="cellsWide">How wide the object is, in cells. Should be at least 1.</param>
        /// <param name="cellsHigh">How high the object is, in cells. Should be at least 1.</param>
        /// <param name="radiusX">Cell radius in X-direction from the center of the object.</param>
        /// <param name="radiusY">Cell radius in Y-direction from the center of the object.</param>
        /// <param name="tileSize">Tile size to translate cells to actual dimensions.</param>
        /// <param name="center">Output parameter giving the center coordinates.</param>
        /// <returns>Bounds for the radius around the object.</returns>
        public static Rectangle GetBoxFromCenterCell(Point cell, int cellsWide, int cellsHigh, double radiusX, double radiusY, Size tileSize, out Point center)
        {
            cellsWide = Math.Max(1, Math.Abs(cellsWide));
            cellsHigh = Math.Max(1, Math.Abs(cellsHigh));
            int realDiamX = (int)Math.Round((radiusX * 2 + 1) * tileSize.Width);
            int realRadX = realDiamX / 2;
            int realDiamY = (int)Math.Round((radiusY * 2 + 1) * tileSize.Height);
            int realRadY = realDiamY / 2;
            int centerX = (cell.X + (cellsWide - 1) / 2) * tileSize.Width + tileSize.Width / 2;
            int centerY = (cell.Y + (cellsHigh - 1) / 2) * tileSize.Height + tileSize.Height / 2;
            center = new Point(centerX, centerY);
            return new Rectangle(centerX - realRadX, centerY - realRadY, realDiamX, realDiamY);
        }

        /// <summary>
        /// ArgumentException messes with the Message property, and dumps its own extra (localised) bit of
        /// text with the argument onto the end of the message. This uses serialisation to retrieve the
        /// original internal message of the exception without added junk.
        /// </summary>
        /// <param name="argex">The ArgumentException to retrieve the message from</param>
        /// <param name="fallback">True to construct a fallback message if the error message is empty.</param>
        /// <returns>The actual message given when the ArgumentException was created.</returns>
        public static string RecoverArgExceptionMessage(ArgumentException argex, bool fallback)
        {
            if (argex == null)
                return null;
            SerializationInfo info = new SerializationInfo(typeof(ArgumentException), new FormatterConverter());
            argex.GetObjectData(info, new StreamingContext(StreamingContextStates.Clone));
            string message = info.GetString("Message");
            if (!string.IsNullOrEmpty(message))
                return message;
            if (!fallback)
                return string.Empty;
            // Fallback: if no message, provide basic info.
            if (string.IsNullOrEmpty(argex.ParamName))
                return string.Empty;
            if (argex is ArgumentNullException)
                return string.Format("\"{0}\" is null.", argex.ParamName);
            if (argex is ArgumentOutOfRangeException)
                return string.Format("\"{0}\" out of range.", argex.ParamName);
            return argex.ParamName;
        }

        public static bool[,] GetMaskFromString(int width, int height, string maskString, char clearChar, params char[] ignoreChars)
        {
            bool[,] mask = new bool[height, width];
            if (string.IsNullOrWhiteSpace(maskString))
            {
                // No mask is considered as fully impassable.
                mask.Clear(true);
                return mask;
            }
            if (ignoreChars.Length > 0)
            {
                Regex clearIgnore = new Regex("[" + Regex.Escape(new string(ignoreChars)) + "]+", RegexOptions.IgnoreCase);
                maskString = clearIgnore.Replace(maskString, string.Empty);
            }
            int charIndex = 0;
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x, ++charIndex)
                {
                    mask[y, x] = charIndex < maskString.Length && maskString[charIndex] != clearChar;
                }
            }
            return mask;
        }

        public static string GetStringFromMask(bool[,] mask, char occupiedChar, char clearChar, char? separatorChar)
        {
            bool hasSeparator = separatorChar.HasValue;
            char separator = separatorChar ?? ' ';
            int baseMaskY = mask.GetLength(0);
            int baseMaskX = mask.GetLength(1);
            StringBuilder occupyMask = new StringBuilder();
            int lastY = baseMaskY - 1;
            for (var y = 0; y < baseMaskY; ++y)
            {
                for (var x = 0; x < baseMaskX; ++x)
                {
                    occupyMask.Append(mask[y, x] ? occupiedChar : clearChar);
                }
                if (y < lastY && hasSeparator)
                {
                    occupyMask.Append(separator);
                }
            }
            return occupyMask.ToString();
        }

        /// <summary>
        /// Chops up an image into cells and determines for each sub-cell of each cell if it can be considered "mostly opaque".
        /// </summary>
        /// <param name="image">Image to scan.</param>
        /// <param name="size">Size of the object in cells.</param>
        /// <param name="borderPercentage">Percentage of the width/height of the cell to see as secondary border around a center.</param>
        /// <param name="centerThreshold">Threshold percentage of pixels in the center that make the image count as opaque.</param>
        /// <param name="borderThreshold">If center is not deemed opaque enough, threshold percentage of pixels in the outer border that make the image still count as opaque.</param>
        /// <param name="minAlpha">Minimum alpha value to consider a pixel to be opaque.</param>
        /// <returns></returns>
        public static bool[,][] MakeOpaqueMask(Bitmap image, Size size, int borderPercentage, int centerThreshold, int borderThreshold, int minAlpha, bool ignoreBlack)
        {
            int width = image.Width;
            int height = image.Height;
            bool[,][] cellEvaluation = new bool[size.Height, size.Width][];
            int fullCellWidth = width / size.Width;
            int fullCellHeight = height / size.Height;
            int fullCellsize = Math.Min(fullCellWidth, fullCellHeight);
            int padX = width - size.Width * fullCellsize;
            int padY = height - size.Height * fullCellsize;
            Bitmap imageToUse = image;
            bool disposeImage = false;
            try
            {
                if (padX > 0 || padY > 0)
                {
                    disposeImage = true;
                    width = size.Width * fullCellsize;
                    height = size.Height * fullCellsize;
                    imageToUse = new Bitmap(width, height);
                    imageToUse.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                    using (Graphics g = Graphics.FromImage(imageToUse))
                    {
                        g.DrawImage(imageToUse, new Rectangle(padX, padY, image.Width, image.Height),
                                    new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                    }
                }
                for (int y = 0; y < size.Height; ++y)
                {
                    for (int x = 0; x < size.Width; ++x)
                    {
                        Rectangle cellRect = new Rectangle(fullCellsize * x, fullCellsize * y, fullCellsize, fullCellsize);
                        cellEvaluation[y, x] = AreSubCellsOpaque(image, cellRect, borderPercentage, centerThreshold, borderThreshold, minAlpha, ignoreBlack);
                    }
                }
            }
            finally
            {
                if (disposeImage && imageToUse != null)
                {
                    try { imageToUse.Dispose(); }
                    catch { /* ignore.*/ }
                }
            }
            return cellEvaluation;
        }

        private static bool[] AreSubCellsOpaque(Bitmap image, Rectangle cellRect, int borderPercentage, int centerThreshold, int borderThreshold, int minAlpha, bool ignoreBlack)
        {
            Point[] subCellCoords = new Point[] { new Point(1, 1), new Point(0, 0), new Point(2, 0), new Point(0, 2), new Point(2, 2) };
            bool[] subCells = new bool[5];
            int cellWidth = cellRect.Width;
            int cellHeight = cellRect.Height;
            int subCellWidth = Math.Max(1, cellWidth / 2);
            int subCellHeight = Math.Max(1, cellHeight / 2);
            for (int pt = 0; pt < subCellCoords.Length; ++pt)
            {
                Point subCellMul = subCellCoords[pt];
                Rectangle subCellRect = new Rectangle(cellRect.X + cellWidth * subCellMul.X / 4, cellRect.Y + cellHeight * subCellMul.Y / 4, subCellWidth, subCellHeight);
                subCells[pt] = IsSubCellOpaque(image, subCellRect, borderPercentage, centerThreshold, borderThreshold, minAlpha, ignoreBlack);
            }
            return subCells;
        }

        private static bool IsSubCellOpaque(Bitmap image, Rectangle subCellRect, int borderPercentage, int centerThreshold, int borderThreshold, int minAlpha, bool ignoreBlack)
        {
            int subCellWidth = subCellRect.Width;
            int subCellHeight = subCellRect.Height;
            int borderWidth = subCellWidth * borderPercentage / 100;
            int borderHeight = subCellHeight * borderPercentage / 100;
            int centerWidth = subCellWidth - borderWidth * 2;
            int centerHeight = subCellHeight - borderHeight * 2;

            Rectangle centerRect = new Rectangle(subCellRect.X + borderWidth, subCellRect.Y + borderHeight, centerWidth, centerHeight);
            int centerPixels = centerRect.Width * centerRect.Height;
            int centerPixelsThreshold = centerPixels * centerThreshold / 100;
            int stride;
            byte[] data = ImageUtils.GetImageData(image, out stride, ref centerRect, PixelFormat.Format32bppArgb, true);
            int centerOpaquePixels = 0;
            bool centerMatch = EvalAreaAlpha(data, stride, minAlpha, 0, centerHeight, 0, centerWidth, centerPixelsThreshold, ref centerOpaquePixels, ignoreBlack);
            if (centerMatch)
            {
                return true;
            }
            // center didn't match. Check surrounding area.
            int borderPixels = subCellWidth * subCellHeight - centerPixels;
            int borderPixelsThreshold = (borderPixels) * borderThreshold / 100;
            data = ImageUtils.GetImageData(image, out stride, ref subCellRect, PixelFormat.Format32bppArgb, true);
            int borderOpaquePixels = 0;
            int topPartMaxY = borderHeight;
            bool edgeTopMatch = EvalAreaAlpha(data, stride, minAlpha, 0, topPartMaxY, 0, subCellWidth, borderPixelsThreshold, ref borderOpaquePixels, ignoreBlack);
            if (edgeTopMatch)
            {
                return true;
            }
            int bottomPartStartY = centerHeight + borderHeight;
            bool edgeBottomMatch = EvalAreaAlpha(data, stride, minAlpha, bottomPartStartY, subCellHeight, 0, subCellWidth, borderPixelsThreshold, ref borderOpaquePixels, ignoreBlack);
            if (edgeBottomMatch)
            {
                return true;
            }
            int leftPartEndX = borderWidth;
            bool edgeLeftMatch = EvalAreaAlpha(data, stride, minAlpha, topPartMaxY, bottomPartStartY, 0, leftPartEndX, borderPixelsThreshold, ref borderOpaquePixels, ignoreBlack);
            if (edgeLeftMatch)
            {
                return true;
            }
            int rightPartStartX = centerWidth + borderWidth;
            bool edgeRightMatch = EvalAreaAlpha(data, stride, minAlpha, topPartMaxY, bottomPartStartY, rightPartStartX, subCellWidth, borderPixelsThreshold, ref borderOpaquePixels, ignoreBlack);
            if (edgeRightMatch)
            {
                return true;
            }
            return false;
        }

        private static bool EvalAreaAlpha(byte[] imgData, int stride, int minAlpha, int yStart, int yEnd, int xStart, int xEnd, int threshold, ref int curAmount, bool ignoreBlack)
        {
            int lineAddr = yStart * stride;
            for (int cellY = yStart; cellY < yEnd; ++cellY)
            {
                int addr = lineAddr + xStart;
                for (int cellX = xStart; cellX < xEnd; ++cellX)
                {
                    int curAddr = addr;
                    addr += 4;
                    if (imgData[curAddr + 3] < minAlpha)
                    {
                        continue;
                    }
                    if (ignoreBlack)
                    {
                        // Check brightness to exclude shadow
                        byte red = imgData[curAddr + 2];
                        byte grn = imgData[curAddr + 1];
                        byte blu = imgData[curAddr + 0];
                        // Integer method.
                        int redBalanced = red * red * 2126;
                        int grnBalanced = grn * grn * 7152;
                        int bluBalanced = blu * blu * 0722;
                        int lum = (redBalanced + grnBalanced + bluBalanced) / 255 / 255;
                        // The integer division will automatically reduce anything near-black
                        // to zero, so actually checking against a threshold is unnecessary.
                        // if (lum > lumThresholdSq * 1000)
                        if (lum == 0)
                        {
                            continue;
                        }
                    }
                    curAmount++;
                    if (curAmount > threshold)
                    {
                        return true;
                    }
                }
                lineAddr += stride;
            }
            return false;
        }

        public static Point GetOccupiedCenter(bool[,] occupyMask, Size cellSize)
        {
            int width = occupyMask.GetLength(1);
            int height = occupyMask.GetLength(0);
            int minOccX = int.MaxValue;
            int maxOccX = -1;
            int minOccY = int.MaxValue;
            int maxOccY = -1;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (occupyMask[y, x])
                    {
                        minOccX = Math.Min(x, minOccX);
                        maxOccX = Math.Max(x, maxOccX);
                        minOccY = Math.Min(y, minOccY);
                        maxOccY = Math.Max(y, maxOccY);
                    }
                }
            }
            int usedWidth = (minOccX == int.MaxValue || maxOccX == -1) ? 0 : maxOccX - minOccX + 1;
            int usedHeight = (minOccY == int.MaxValue || maxOccY == -1) ? 0 : maxOccY - minOccY + 1;
            int centerX = usedWidth == -1 ? (width * cellSize.Width / 2) : minOccX * cellSize.Width + (usedWidth * cellSize.Width) / 2;
            int centerY = usedHeight == -1 ? (height * cellSize.Height / 2) : minOccY * cellSize.Height + (usedHeight * cellSize.Height) / 2;
            return new Point(centerX, centerY);
        }

        public static string ReplaceLinebreaks(string input, char replacement)
        {
            return input
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\\", "\\\\")
                .Replace("" + replacement, "\\" + replacement)
                .Replace('\n', replacement);
        }

        public static string RestoreLinebreaks(string input, char replacement, string lineBreak)
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

        public static Regex FileMaskToRegex(string fileMask)
        {
            return FileMaskToRegex(fileMask, true);
        }

        public static Regex FileMaskToRegex(string fileMask, bool ignoreCase)
        {
            string convertedMask = "^" + Regex.Escape(fileMask).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            return new Regex(convertedMask, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[0x8000];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }

        public static MemoryStream CopyToMemoryStream(Stream stream)
        {
            MemoryStream ms = new MemoryStream();
            CopyStream(stream, ms);
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Sends a message to a window control.
        /// </summary>
        /// <param name="hWnd">Pointer to the control's handle.</param>
        /// <param name="msg">The message to be sent.</param>
        /// <param name="wParam">Additional message-specific information</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
