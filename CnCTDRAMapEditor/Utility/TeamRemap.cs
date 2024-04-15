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
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace MobiusEditor.Utility
{
    public class TeamRemap : ITeamColor
    {
        public string Name { get; private set; }
        public byte UnitRadarColor { get; private set; }
        public byte BuildingRadarColor { get; private set; }
        public byte[] RemapTable
        {
            get
            {
                byte[] remapCopy = new byte[0x100];
                Array.Copy(this.remapTable, remapCopy, remapCopy.Length);
                return remapCopy;
            }
        }
        private byte[] remapTable;

        public TeamRemap(string newName, TeamRemap baseRemap)
        {
            this.Name = newName;
            this.UnitRadarColor = baseRemap.UnitRadarColor;
            this.BuildingRadarColor = baseRemap.BuildingRadarColor;
            this.remapTable = new byte[0x100];
            Array.Copy(baseRemap.remapTable, remapTable, remapTable.Length);
        }

        public TeamRemap(string name, byte unitRadarColor, byte buildingRadarColor, byte remapstart, byte[] remapValues)
        {
            this.Name = name;
            this.UnitRadarColor = unitRadarColor;
            this.BuildingRadarColor = buildingRadarColor;
            this.remapTable = Enumerable.Range(0, 0x100).Select(b => (byte)b).ToArray();
            int max = Math.Min(0x100, remapstart + remapValues.Length) - remapstart;
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
            this.remapTable = Enumerable.Range(0, 0x100).Select(b => (byte)b).ToArray();
            int max = Math.Max(remapOrigins.Length, remapValues.Length);
            for (int i = 0; i < max; ++i)
            {
                remapTable[remapOrigins[i]] = remapValues[i];
            }
        }

        public void ApplyToImage(Bitmap image)
        {
            ApplyToImage(image, out _);
        }

        public void ApplyToImage(Bitmap image, out Rectangle opaqueBounds)
        {
            if (image == null)
            {
                opaqueBounds = Rectangle.Empty;
                return;
            }
            int bytesPerPixel = Image.GetPixelFormatSize(image.PixelFormat) / 8;
            if (bytesPerPixel != 3 && bytesPerPixel != 4)
            {
                opaqueBounds = new Rectangle(Point.Empty, image.Size);
                return;
            }
            BitmapData data = null;
            try
            {
                data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);
                byte[] bytes = new byte[data.Stride * data.Height];
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                int width = data.Width;
                int height = data.Height;
                int stride = data.Stride;
                opaqueBounds = ImageUtils.CalculateOpaqueBounds8bpp(bytes, width, height, stride, 0);
                ApplyToImage(bytes, width, height, bytesPerPixel, stride, opaqueBounds);
                Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            }
            finally
            {
                if (data != null)
                {
                    image.UnlockBits(data);
                }
            }
        }

        public void ApplyToImage(byte[] bytes, int width, int height, int bytesPerPixel, int stride, Rectangle? opaqueBounds)
        {
            // Only handle 8bpp data.
            if (bytesPerPixel != 1)
            {
                return;
            }
            Rectangle bounds = opaqueBounds ?? new Rectangle(0, 0, width, height);
            int boundsBottom = Math.Min(height, bounds.Bottom);
            int boundsWidth = Math.Min(Math.Max(0, width - bounds.Left), bounds.Width);
            int linePtr = bounds.Top * stride;
            for (int y = bounds.Top; y < boundsBottom; y++)
            {
                int ptr = linePtr + bounds.Left;
                for (int x = 0; x < boundsWidth; x++)
                {
                    bytes[ptr] = remapTable[bytes[ptr]];
                    ptr++;
                }
                linePtr += stride;
            }
        }
    }
}
