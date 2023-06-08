using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobiusEditor.Utility
{
    public class ClassicSpriteLoader
    {

        private static readonly Byte[] ConvertToEightBit = new Byte[64];

        static ClassicSpriteLoader()
        {
            // Build easy lookup tables for this, so no calculations are ever needed for this later.
            for (Int32 i = 0; i < 64; ++i)
                ConvertToEightBit[i] = (Byte)Math.Round(i * 255.0 / 63.0, MidpointRounding.ToEven);
        }


        /// <summary>
        /// Retrieves the CPS image.
        /// </summary>
        /// <param name="fileData">Original file data.</param>
        /// <param name="start">Start offset of the data.</param>
        /// <returns>The raw 8-bit linear image data in a 64000 byte array.</returns>
        public static Bitmap LoadCpsFile(Byte[] fileData, Int32 start)
        {
            Byte[] imageData = GetCpsData(fileData, 0, out Color[] palette);
            if (palette == null)
                palette = Enumerable.Range(0, 0x100).Select(i => Color.FromArgb(i, i, i)).ToArray();
            return ImageUtils.BuildImage(imageData, 320, 200, 200, PixelFormat.Format8bppIndexed, palette, null);
        }

        /// <summary>
        /// Retrieves the CPS image.
        /// </summary>
        /// <param name="fileData">Original file data.</param>
        /// <param name="start">Start offset of the data.</param>
        /// <returns>The raw 8-bit linear image data in a 64000 byte array.</returns>
        public static Byte[] GetCpsData(Byte[] fileData, Int32 start, out Color[] palette)
        {
            int dataLen = fileData.Length - start;
            if (dataLen < 10)
                throw new ArgumentException("File is not long enough to be a valid CPS file.", "fileData");
            int fileSize = ArrayUtils.ReadInt16FromByteArrayLe(fileData, start + 0);
            int compression = ArrayUtils.ReadUInt16FromByteArrayLe(fileData, start + 2);
            if (compression < 0 || compression > 4)
                throw new ArgumentException("Unknown compression type " + compression, "fileData");
            // compressions other than 0 and 4 count the full file including size header.
            if (compression == 0 || compression == 4)
                fileSize += 2;
            else
                throw new ArgumentException("LZW and RLE compression are not supported.", "fileData");
            if (fileSize != dataLen)
                throw new ArgumentException("File size in header does not match!", "fileData");
            Int32 bufferSize = ArrayUtils.ReadInt32FromByteArrayLe(fileData, start + 4);
            Int32 paletteLength = ArrayUtils.ReadInt16FromByteArrayLe(fileData, start + 8);
            Boolean isPc = bufferSize == 64000;
            if (bufferSize != 64000)
                throw new ArgumentException("Unknown CPS type.", "fileData");
            if (paletteLength > 0)
            {
                Int32 palStart = start + 10;
                if (paletteLength % 3 != 0)
                    throw new ArgumentException("Bad length for 6-bit CPS palette.", "fileData");
                Int32 colors = paletteLength / 3;
                palette = ReadSixBitPaletteAsEightBit(fileData, palStart, colors);
            }
            else
                palette = null;
            Byte[] imageData;
            Int32 dataOffset = start + 10 + paletteLength;
            if (compression == 0 && dataLen < dataOffset + bufferSize)
                throw new ArgumentException("File is not long enough to contain the image data!", "fileData");
            try
            {
                switch (compression)
                {
                    case 0:
                        imageData = new Byte[bufferSize];
                        Array.Copy(fileData, dataOffset, imageData, 0, bufferSize);
                        break;
                    case 4:
                        imageData = new Byte[bufferSize];
                        WWCompression.LcwDecompress(fileData, ref dataOffset, imageData, 0);
                        break;
                    default:
                        throw new ArgumentException("Unsupported compression format \"" + compression + "\".", "fileData");
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException("Error decompressing image data: " + e.Message, "fileData", e);
            }
            if (imageData == null)
                throw new ArgumentException("Error decompressing image data.", "fileData");
            return imageData;
        }

        public static Color[] ReadSixBitPaletteAsEightBit(Byte[] fileData, int palStart, int colors)
        {
            Color[] palette = Enumerable.Repeat(Color.Black, colors).ToArray();

            // Palette data should always be be 0x300 long, but this code works regardless of that.
            int len = Math.Min(fileData.Length / 3, colors);
            int offs = palStart;
            for (int i = 0; i < len; ++i)
            {
                byte r = ConvertToEightBit[fileData[offs + 0] & 0x3F];
                byte g = ConvertToEightBit[fileData[offs + 1] & 0x3F];
                byte b = ConvertToEightBit[fileData[offs + 2] & 0x3F];
                palette[i] = Color.FromArgb(r, g, b);
                offs += 3;
            }
            return palette;
        }
    }
}
