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
            bool success = LoadMapping("classic\\th_desert.nms", Properties.Resources.n64_th_desert, out mapping, out mappingRev, errors);
            if (success)
            {
                DESERT_MAPPING = mapping;
                DESERT_MAPPING_REVERSED = mappingRev;
            }
            success = LoadMapping("classic\\th_temperate.nms", Properties.Resources.n64_th_temperate, out mapping, out mappingRev, errors);
            if (success)
            {
                TEMPERATE_MAPPING = mapping;
                TEMPERATE_MAPPING_REVERSED = mappingRev;
            }
            loadErrors = errors.ToArray();
        }

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
                            mapping.Clear();
                            throw new ApplicationException("File contains duplicate entries!");
                        }
                        if (inverseMapping.ContainsKey(cellValPc))
                        {
                            hasErrors = true;
                            if (errors != null)
                            {
                                errors.Add(string.Format("Value {0} - {1} - PC value {1} already mapped on N64 value {2}",
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
