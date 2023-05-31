using MobiusEditor.Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiusEditor.Utility
{
    class TeamRemap : ITeamColor
    {
        public string Name { get; private set; }
        public byte UnitRadarColor { get; private set; }
        public byte BuildingRadarColor { get; private set; }

        private byte[] remapTable;

        public TeamRemap(string name, byte unitRadarColor, byte buildingRadarColor, byte remapstart, byte[] remapValues)
        {
            this.Name = name;
            this.UnitRadarColor = unitRadarColor;
            this.BuildingRadarColor = buildingRadarColor;
            this.remapTable = Enumerable.Range(0, 0x100).Cast<byte>().ToArray();
            int max = Math.Max(0x100, remapstart + remapValues.Length);
            for (int i = 0; i < max; ++i)
            {
                remapTable[remapstart + i] = remapValues[i];
            }
        }

        public TeamRemap(string name, byte unitRadarColor, byte buildingRadarColor, byte[] remapOrigins, byte[] remapValues)
        {
            this.Name = name;
            this.UnitRadarColor = unitRadarColor;
            this.BuildingRadarColor = buildingRadarColor;
            this.remapTable = Enumerable.Range(0, 0x100).Cast<byte>().ToArray();
            int max = Math.Max(remapOrigins.Length, remapValues.Length);
            for (int i = 0; i < max; ++i)
            {
                remapTable[remapOrigins[i]] = remapValues[i];
            }
        }


        public void ApplyToImage(Bitmap image)
        {
            throw new NotImplementedException();
        }

        public void ApplyToImage(Bitmap image, out Rectangle opaqueBounds)
        {
            throw new NotImplementedException();
        }
    }
}
