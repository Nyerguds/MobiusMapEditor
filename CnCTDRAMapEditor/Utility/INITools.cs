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
    public static class INITools
    {
        /// <summary>
        /// Gets the ini contents of a byte array, interpreted as DOS-437 encoding. This is used for quick checks on the existence of certain ini elements.
        /// </summary>
        /// <param name="contents">File contents as byte array.</param>
        /// <returns>The ini contents of the byte array, interpreted as DOS-437 encoding.</returns>
        public static INI GetIniContents(byte[] contents)
        {
            try
            {
                Encoding encDOS = Encoding.GetEncoding(437);
                string stringContents = null;
                using (MemoryStream ms = new MemoryStream(contents))
                using (StreamReader iniReader = new StreamReader(ms, encDOS))
                {
                    stringContents = iniReader.ReadToEnd();
                }
                INI iniContents = new INI();
                iniContents.Parse(stringContents);
                return iniContents.Sections.Count == 0 ? null : iniContents;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Returns whether certain ini information was found in the given ini data.
        /// </summary>
        /// <param name="ini">ini data.</param>
        /// <param name="section">Section to find.</param>
        /// <returns>True if the ini section was found.</returns>
        public static bool CheckForIniInfo(INI ini, string section)
        {
            return CheckForIniInfo(ini, section, null, null);
        }

        /// <summary>
        /// Returns whether certain ini information was found in the given ini data.
        /// </summary>
        /// <param name="ini">ini data.</param>
        /// <param name="section">Section to find.</param>
        /// <param name="key">Optional key to find.</param>
        /// <returns>True if the ini information was found.</returns>
        public static bool CheckForIniInfo(INI ini, string section, string key)
        {
            return CheckForIniInfo(ini, section, key, null);
        }

        /// <summary>
        /// Returns whether certain ini information was found in the given ini data.
        /// </summary>
        /// <param name="ini">ini data.</param>
        /// <param name="section">Section to find.</param>
        /// <param name="key">Optional key to find.</param>
        /// <param name="value">Optional value to find.</param>
        /// <returns>True if the ini information was found.</returns>
        public static bool CheckForIniInfo(INI ini, string section, string key, string value)
        {
            if (ini == null) throw new ArgumentNullException("ini");
            if (section == null) throw new ArgumentNullException("section");
            INISection iniSection = ini[section];
            if (key == null)
            {
                return iniSection != null;
            }
            bool hasKey = iniSection != null && iniSection.Keys.Contains(key);
            if (value == null)
            {
                return hasKey;
            }
            return hasKey && iniSection[key].Trim() == value;
        }

        /// <summary>
        /// Checks if the given string is a valid ini key in an ASCII context.
        /// </summary>
        /// <param name="iniKey">The key to check.</param>
        /// <param name="reservedNames">Optional array of reserved names. IF given, any entry in this list will also return false.</param>
        /// <returns>True if the given string is a valid ini key in an ASCII context.</returns>
        public static bool IsValidKey(string iniKey, params string[] reservedNames)
        {
            if (reservedNames != null)
            {
                foreach (string name in reservedNames)
                {
                    if (name.Equals(iniKey, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }
            return iniKey.All(c => c > ' ' && c <= '~' && c != '=' && c != '[' && c != ']');
        }

        /// <summary>
        /// Will find a section in the ini information, parse its data into the given data object, remove all
        /// keys managed by the data object from the ini section, and, if empty, remove the section from the ini.
        /// </summary>
        /// <typeparam name="T">Type of the data.</typeparam>
        /// <param name="ini">Ini object.</param>
        /// <param name="name">Name of the section.</param>
        /// <param name="data">Data object.</param>
        /// <param name="context">Map context to read data.</param>
        /// <returns>Null if the section was not found, otherwise the trimmed section.</returns>
        public static INISection ParseAndLeaveRemainder(INI ini, string name, object data, MapContext context)
        {
            return ParseAndLeaveRemainder(ini, name, data, context, null);
        }

        /// <summary>
        /// Will find a section in the ini information, parse its data into the given data object, remove all
        /// keys managed by the data object from the ini section, and, if empty, remove the section from the ini.
        /// </summary>
        /// <typeparam name="T">Type of the data.</typeparam>
        /// <param name="ini">Ini object.</param>
        /// <param name="name">Name of the section.</param>
        /// <param name="data">Data object.</param>
        /// <param name="context">Map context to read data.</param>
        /// <param name="modified">True if the parsing failed on some parts and the resulting object does not completely correspond to the given ini data.</param>
        /// <param name="errors">List of errors that was encountered, as ordered key-value data.</param>
        /// <returns>Null if the section was not found, otherwise the trimmed section.</returns>
        public static INISection ParseAndLeaveRemainder(INI ini, string name, object data, MapContext context, ref bool modified, List<(string, string)> errors)
        {
            bool addErrors = errors != null;
            List<(string, string)> newErrors = addErrors ? new List<(string, string)>() : null;
            INISection section = ParseAndLeaveRemainder(ini, name, data, context, newErrors);
            if (addErrors && newErrors.Count > 0)
            {
                errors.AddRange(newErrors);
                modified = true;
            }
            return section;
        }

        /// <summary>
        /// Will find a section in the ini information, parse its data into the given data object, remove all
        /// keys managed by the data object from the ini section, and, if empty, remove the section from the ini.
        /// </summary>
        /// <typeparam name="T">Type of the data.</typeparam>
        /// <param name="ini">Ini object.</param>
        /// <param name="name">Name of the section.</param>
        /// <param name="data">Data object.</param>
        /// <param name="context">Map context to read data.</param>
        /// <param name="errors">List of errors that was encountered, as ordered key-value data.</param>
        /// <returns>Null if the section was not found, otherwise the trimmed section.</returns>
        public static INISection ParseAndLeaveRemainder(INI ini, string name, object data, MapContext context, List<(string, string)> errors)
        {
            var dataSection = ini.Sections[name];
            if (dataSection == null)
                return null;
            bool addErrors = errors != null;
            List<(string, string)> addedErrors = INI.ParseSection(context, dataSection, data, addErrors);
            if (addErrors)
            {
                errors.AddRange(addedErrors);
            }
            INI.RemoveHandledKeys(dataSection, data);
            if (dataSection.Keys.Count() == 0)
                ini.Sections.Remove(name);
            return dataSection;
        }

        /// <summary>
        /// Will extract a section from the ini information, add the current data to it, and re-add it
        /// at the end of the ini object. If the <see cref="shouldAdd" /> argument is false, and no section
        /// with this name is found in the current ini object, the object is not added. Otherwise it
        /// will be added, with the data object info added into it.
        /// </summary>
        /// <typeparam name="T">Type of the data.</typeparam>
        /// <param name="ini">Ini object.</param>
        /// <param name="name">Name of the section.</param>
        /// <param name="data">Data object.</param>
        /// <param name="context">Map context to write data.</param>
        /// <param name="shouldAdd">False if the object is not supposed to be added. This will be ignored if a section with that name is found that contains keys not managed by the data object.</param>
        /// <returns>Null if the section was not found, otherwise the final re-added section.</returns>
        public static INISection FillAndReAdd(INI ini, string name, object data, MapContext context, bool shouldAdd)
        {
            // Removes the section from the ini object.
            INISection originalDataSection = ini.Sections.Extract(name);
            // Clone to store existing data to re-add.
            INISection existingDataSection = null;
            if (originalDataSection != null)
            {
                existingDataSection = originalDataSection.Clone();
                INI.RemoveHandledKeys(existingDataSection, data);
                if (existingDataSection.Keys.Count > 0)
                {
                    // Contains extra keys.
                    shouldAdd = true;
                }
            }
            if (!shouldAdd)
            {
                return null;
            }
            // either recycle the old one, or make a new one. This retains the original object, which can be important for references.
            INISection newDataSection = originalDataSection ?? new INISection(name);
            ini.Sections.Add(newDataSection);
            // Clear, in case it was the original one.
            newDataSection.Clear();
            // Add object's data into the section.
            INI.WriteSection(context, newDataSection, data);
            // Add any custom added keys into the section.
            if (existingDataSection != null)
            {
                foreach (KeyValuePair<string, string> kvp in existingDataSection)
                {
                    newDataSection[kvp.Key] = kvp.Value;
                }
            }
            return newDataSection;
        }

        /// <summary>
        /// Will seek a section in the ini, remove any information in it that is handled by the data object,
        /// and remove the section from the ini if no keys remain in it.
        /// </summary>
        /// <typeparam name="T">Type of the data.</typeparam>
        /// <param name="ini">Ini object.</param>
        /// <param name="data">Data object.</param>
        /// <param name="name">Name of the section.</param>
        public static void ClearDataFrom(INI ini, string name, object data)
        {
            var basicSection = ini.Sections[name];
            if (basicSection != null)
            {
                INI.RemoveHandledKeys(basicSection, data);
                if (basicSection.Keys.Count() == 0)
                    ini.Sections.Remove(name);
            }
        }

        public static byte[] DecompressLCWSection(INISection section, CellMetrics metrics, int bytesPerCell, List<string> errors, ref bool modified)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> kvp in section)
            {
                sb.Append(kvp.Value);
            }
            byte[] compressedBytes;
            try
            {
                compressedBytes = Convert.FromBase64String(sb.ToString());
            }
            catch (FormatException)
            {
                errors.Add("Failed to unpack [" + section.Name + "] from Base64.");
                modified = true;
                return null;
            }
            int readPtr = 0;
            int writePtr = 0;
            byte[] decompressedBytes = new byte[metrics.Width * metrics.Height * bytesPerCell];
            while ((readPtr + 4) <= compressedBytes.Length)
            {
                uint uLength;
                using (BinaryReader reader = new BinaryReader(new MemoryStream(compressedBytes, readPtr, 4)))
                {
                    uLength = reader.ReadUInt32();
                }
                int outputLength = (int)((uLength >> 16) & 0xFFFF);
                int length = (int)(uLength & 0xFFFF);
                readPtr += 4;
                byte[] dest = new byte[outputLength];
                int readPtr2 = readPtr;
                int decompressed;
                try
                {
                    decompressed = WWCompression.LcwDecompress(compressedBytes, ref readPtr2, dest, 0);
                }
                catch
                {
                    errors.Add("Error decompressing [" + section.Name + "].");
                    modified = true;
                    return decompressedBytes;
                }
                if (writePtr + decompressed > decompressedBytes.Length)
                {
                    errors.Add("Failed to decompress [" + section.Name + "]: data exceeds map size.");
                    modified = true;
                    return decompressedBytes;
                }
                Array.Copy(dest, 0, decompressedBytes, writePtr, decompressed);
                readPtr += length;
                writePtr += decompressed;
            }
            return decompressedBytes;
        }

        public static INISection CompressLCWSection(INISection section, byte[] uncompressedBytes)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                foreach (byte[] uncompressedChunk in uncompressedBytes.Split(8192))
                {
                    byte[] compressedChunk = WWCompression.LcwCompress(uncompressedChunk);
                    writer.Write((ushort)compressedChunk.Length);
                    writer.Write((ushort)uncompressedChunk.Length);
                    writer.Write(compressedChunk);
                }
                writer.Flush();
                stream.Position = 0;
                string[] values = Convert.ToBase64String(stream.ToArray()).Split(70).ToArray();
                for (int i = 0; i < values.Length; ++i)
                {
                    section[(i + 1).ToString()] = values[i];
                }
            }
            return section;
        }
    }
}
