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
                string iniContents = null;
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
            Encoding altEncoding, (string section, string key)[] toAltEncode, Byte[] lineEnd)
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

        /// <summary>
        /// If the given value is one of the values inside the given list, then add the remark to its end. Otherwise, return the original string.
        /// </summary>
        /// <param name="value">Value to evaluate.</param>
        /// <param name="defaultVal">Value to return if the input is empty.</param>
        /// <param name="trimSource">True to trim the source before adding the remark.</param>
        /// <param name="valuesToDetect">List of values to add the remark to. Case-insensitive.</param>
        /// <param name="remarkToAdd">Remark to add to the detected values.</param>
        /// <returns></returns>
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

        public static bool[,] GetMaskFromString(int width, int height, string maskString, char clearChar, params char[] ignoreChars)
        {
            bool[,] mask = new bool[height, width];
            if (String.IsNullOrWhiteSpace(maskString))
            {
                mask.Clear(true);
                return mask;
            }
            if (ignoreChars.Length > 0)
            {
                Regex clearIgnore = new Regex("[" + Regex.Escape(new String(ignoreChars)) + "]+", RegexOptions.IgnoreCase);
                maskString = clearIgnore.Replace(maskString, String.Empty);
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

        public static Boolean[,] FindOpaqueCells(Bitmap image, Size size, int maxPercentage, int borderPercentage, int minAlpha)
        {
            int width = image.Width;
            int height = image.Height;
            Boolean[,] cellEvaluation = new bool[size.Height, size.Width];
            int fullCellWidth = width / size.Width;
            int fullCellHeight = height / size.Height;
            int cellBorderX = fullCellWidth * borderPercentage / 100;
            int cellBorderY = fullCellHeight * borderPercentage / 100;
            int usedCellWidth = fullCellWidth - cellBorderX * 2;
            int usedCellHeight = fullCellHeight - cellBorderY * 2;
            for (int y = 0; y < size.Height; ++y)
            {
                for (int x = 0; x < size.Width; ++x)
                {
                    Rectangle cellRect = new Rectangle(fullCellHeight * x + cellBorderX, fullCellHeight * y + cellBorderY, usedCellWidth, usedCellHeight);
                    int cellWidth = cellRect.Width;
                    int cellHeight = cellRect.Height;
                    int cellPixels = cellWidth * cellHeight;
                    int threshold = cellPixels * maxPercentage / 100;
                    int stride;
                    Byte[] data = ImageUtils.GetImageData(image, out stride, ref cellRect, PixelFormat.Format32bppArgb, true);
                    int opaquePixels = 0;
                    int lineAddr = 0;
                    for (int cellY = 0; cellY < cellWidth; ++cellY)
                    {
                        int addr = lineAddr;
                        for (int cellX = 0; cellX < cellHeight; ++cellX)
                        {
                            if (data[addr + 3] >= minAlpha)
                            {
                                opaquePixels++;
                                if (opaquePixels > threshold)
                                {
                                    cellY = cellHeight;
                                    break;
                                }
                            }
                            addr += 4;
                        }
                        lineAddr += stride;
                    }
                    cellEvaluation[y, x] = opaquePixels > threshold;
                }
            }
            return cellEvaluation;
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
            String convertedMask = "^" + Regex.Escape(fileMask).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            return new Regex(convertedMask, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        }

        public static void CopyStream(Stream input, Stream output)
        {
            Byte[] buffer = new Byte[0x8000];
            Int32 read;
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

    }
}
