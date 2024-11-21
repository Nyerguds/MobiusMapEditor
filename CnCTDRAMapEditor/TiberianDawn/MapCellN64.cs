using System;
using System.Collections.Generic;
using System.IO;

namespace MobiusEditor.TiberianDawn
{
    public class MapCellN64 : IComparable<MapCellN64>, IComparable
    {
        public static readonly Dictionary<int, MapCellN64> DESERT_MAPPING = LoadMapping("classic\\th_desert.nms", Properties.Resources.n64_th_desert);
        public static readonly Dictionary<int, MapCellN64> TEMPERATE_MAPPING = LoadMapping("classic\\th_temperate.nms", Properties.Resources.n64_th_temperate);
        public static readonly Dictionary<int, MapCellN64> DESERT_MAPPING_REVERSED = LoadReverseMapping(DESERT_MAPPING);
        public static readonly Dictionary<int, MapCellN64> TEMPERATE_MAPPING_REVERSED = LoadReverseMapping(TEMPERATE_MAPPING);

        public byte HighByte { get; set; }
        public byte LowByte { get; set; }
        public int Value { get { return this.HighByte << 8 | this.LowByte; } }

        public MapCellN64(byte highByte, byte lowByte)
        {
            this.HighByte = highByte;
            this.LowByte = lowByte;
        }

        public MapCellN64(int value)
        {
            if (value > 0xFFFF)
                throw new ArgumentOutOfRangeException("value");
            this.HighByte = (byte)((value >> 8) & 0xFF);
            this.LowByte = (byte)(value & 0xFF);
        }

        public bool Equals(MapCellN64 cell)
        {
            return ((cell.HighByte == this.HighByte) && (cell.LowByte == this.LowByte));
        }

        public override string ToString()
        {
            return this.Value.ToString("X4");
        }

        public int CompareTo(MapCellN64 other)
        {
            return this.Value.CompareTo(other.Value);
        }

        public int CompareTo(object obj)
        {
            MapCellN64 cell = obj as MapCellN64;
            if (cell != null)
                return this.CompareTo(cell);
            return this.Value.CompareTo(obj);
        }

        public override int GetHashCode()
        {
            return this.Value;
        }

        private static Dictionary<int, MapCellN64> LoadMapping(string filename, byte[] internalFallback)
        {
            string[] errors;
            byte[] mappingBytes;
            string file = Path.GetFullPath(Path.Combine(Program.ApplicationPath, filename));            
            if (File.Exists(file))
                mappingBytes = File.ReadAllBytes(file);
            else
                mappingBytes = internalFallback;
            return LoadMapping(mappingBytes, out errors);
        }

        private static Dictionary<int, MapCellN64> LoadReverseMapping(Dictionary<int, MapCellN64> mapping)
        {
            Dictionary<int, MapCellN64> newmapping = new Dictionary<int, MapCellN64>();
            List<MapCellN64> errorcells;
            Dictionary<int, MapCellN64[]> mapping2 = GetReverseMapping(mapping, out errorcells);
            foreach (int val in mapping2.Keys)
                newmapping.Add(val, mapping2[val][0]);
            return newmapping;
        }

        private static Dictionary<int, MapCellN64> LoadMapping(byte[] fileData, out string[] errors)
        {
            List<string> errorMessages = new List<string>();
            Dictionary<int, MapCellN64> n64MapValues = new Dictionary<int, MapCellN64>();
            Dictionary<int, MapCellN64> reverseValues = new Dictionary<int, MapCellN64>();
            using (MemoryStream ms = new MemoryStream(fileData))
            {
                int amount = (int)ms.Length / 4;
                if (ms.Length != amount * 4)
                    throw new ArgumentException("file size must be divisible by 4!", "fileData");
                byte[] buffer = new byte[4];
                for (int i = 0; i < amount; ++i)
                {
                    if (ms.Read(buffer, 0, 4) == 4)
                    {
                        MapCellN64 N64cell = new MapCellN64(buffer[0], buffer[1]);
                        MapCellN64 PCcell = new MapCellN64(buffer[2], buffer[3]);
                        if (n64MapValues.ContainsKey(N64cell.Value))
                        {
                            n64MapValues.Clear();
                            throw new ApplicationException("File contains duplicate entries!");
                        }
                        if (reverseValues.ContainsKey(PCcell.Value))
                            errorMessages.Add(string.Format("Value {0} - {1} - PC value {1} already mapped on N64 value {2}", N64cell.ToString(), PCcell.ToString(), reverseValues[PCcell.Value].ToString()));
                        else
                            reverseValues.Add(PCcell.Value, N64cell);
                        n64MapValues.Add(N64cell.Value, PCcell);
                    }
                }
            }
            errors = errorMessages.ToArray();
            return n64MapValues;
        }

        private static Dictionary<int, MapCellN64[]> GetReverseMapping(Dictionary<int, MapCellN64> mapping, out List<MapCellN64> errorcells)
        {
            Dictionary<int, MapCellN64[]> newmapping = new Dictionary<int, MapCellN64[]>();
            errorcells = new List<MapCellN64>();
            foreach (int mapval in mapping.Keys)
            {
                MapCellN64 cell = mapping[mapval];
                if (!newmapping.ContainsKey(cell.Value))
                    newmapping.Add(cell.Value, new MapCellN64[] { new MapCellN64(mapval) });
                else
                {
                    MapCellN64[] orig = newmapping[cell.Value];
                    MapCellN64[] arr = new MapCellN64[orig.Length + 1];
                    Array.Copy(orig, arr, orig.Length);
                    arr[orig.Length] = new MapCellN64(mapval);
                    newmapping[cell.Value] = arr;
                    if (!errorcells.Contains(cell))
                        errorcells.Add(cell);
                }
            }
            return newmapping;
        }
    }
}
