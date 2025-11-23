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
using System.IO;

namespace MobiusEditor.TiberianDawn
{
    public static class N64MapConverter
    {
        public static readonly Dictionary<int, ushort> DESERT_MAPPING;
        public static readonly Dictionary<int, ushort> DESERT_MAPPING_REVERSED;
        public static readonly Dictionary<int, ushort> TEMPERATE_MAPPING;
        public static readonly Dictionary<int, ushort> TEMPERATE_MAPPING_REVERSED;
        public static readonly string[] loadErrors;

        static N64MapConverter()
        {
            List<string> errors = new List<string>();
            Dictionary<int, ushort> mapping;
            Dictionary<int, ushort> mappingRev;
            // .nms = "N64 Map Scan" originally created by the N64 Map Converter tool. These mappings are the result of scanning all N64 maps
            // that are identical in content to their PC equivalent, and thus building up a full mapping of the cell values of the two types.
            bool success = LoadMapping("classic\\n64_th_desert.nms", Properties.Resources.n64_th_desert, out mapping, out mappingRev, errors);
            if (success)
            {
                DESERT_MAPPING = mapping;
                DESERT_MAPPING_REVERSED = mappingRev;
            }
            success = LoadMapping("classic\\n64_th_desert.nms", Properties.Resources.n64_th_temperate, out mapping, out mappingRev, errors);
            if (success)
            {
                TEMPERATE_MAPPING = mapping;
                TEMPERATE_MAPPING_REVERSED = mappingRev;
            }
            loadErrors = errors.ToArray();
        }

        /// <summary>
        /// Reads the mappings file to convert the binary template maps between PC and N64 versions.
        /// </summary>
        /// <param name="filename">File to read</param>
        /// <param name="internalFallback">Internal fallback in case no file exists.</param>
        /// <param name="mapping">Output parameter for the mapping</param>
        /// <param name="inverseMapping">Output parameter for the inverse mapping; the mapping should always work two-way.</param>
        /// <param name="errors">Strings list in which to add any loading errors. Can be null to get no detailed feedback.</param>
        /// <returns>True if the mapping and reverse-mapping successfully generated.</returns>
        private static bool LoadMapping(string filename, byte[] internalFallback, out Dictionary<int, ushort> mapping, out Dictionary<int, ushort> inverseMapping, List<string> errors)
        {
            byte[] mappingBytes;
            string file = Path.GetFullPath(Path.Combine(Program.ApplicationPath, filename));            
            if (File.Exists(file))
                mappingBytes = File.ReadAllBytes(file);
            else
                mappingBytes = internalFallback;
            return LoadMapping(mappingBytes, out mapping, out inverseMapping, errors);
        }

        /// <summary>
        /// Reads the byte data as mapping.
        /// </summary>
        /// <param name="fileData">Mapping data. This should be a simple list of little-endian ushort value pairs, with first the N64 value and then the PC one.</param>
        /// <param name="mapping">Output parameter for the mapping from N64 to PC values.</param>
        /// <param name="inverseMapping">Output parameter for the inverse mapping, from PC to N64. Note that this will not cover the full tilesets since the N64 version has less tiles.</param>
        /// <param name="errors">Strings list in which to add any loading errors. Can be null to get no detailed feedback.</param>
        /// <returns>True if the mapping and reverse-mapping successfully generated.</returns>
        /// <exception cref="ApplicationException"></exception>
        private static bool LoadMapping(byte[] fileData, out Dictionary<int, ushort> mapping, out Dictionary<int, ushort> inverseMapping, List<string> errors)
        {
            bool hasErrors = false;
            mapping = new Dictionary<int, ushort>();
            inverseMapping = new Dictionary<int, ushort>();
            using (MemoryStream ms = new MemoryStream(fileData))
            {
                int amount = (int)ms.Length / 4;
                if (ms.Length != amount * 4)
                {
                    if (errors != null)
                    {
                        errors.Add("file size must be divisible by 4!");
                    }
                    return false;
                }
                byte[] buffer = new byte[4];
                for (int i = 0; i < amount; ++i)
                {
                    if (ms.Read(buffer, 0, 4) == 4)
                    {
                        ushort cellValN64 = (ushort)(buffer[0] << 8 | buffer[1]);
                        ushort cellValPc = (ushort)(buffer[2] << 8 | buffer[3]);
                        if (mapping.ContainsKey(cellValN64))
                        {
                            int mapped = mapping[cellValN64];
                            bool warnOnly = cellValPc == mapping[cellValN64];
                            if (errors != null)
                            {
                                errors.Add(String.Format("File contains duplicate mapping for value {0}.", cellValN64));
                            }
                            return false;
                        }
                        if (inverseMapping.ContainsKey(cellValPc))
                        {
                            hasErrors = true;
                            if (errors != null)
                            {
                                errors.Add(String.Format("Value {0} - {1} - PC value {1} already mapped on N64 value {2}",
                                    cellValN64.ToString("X4"), cellValPc.ToString("X4"), inverseMapping[cellValPc].ToString("X4")));
                            }
                        }
                        else
                        {
                            inverseMapping.Add(cellValPc, cellValN64);
                        }
                        mapping.Add(cellValN64, cellValPc);
                    }
                }
            }
            return !hasErrors;
        }
    }
}
