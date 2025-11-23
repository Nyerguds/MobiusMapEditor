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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MobiusEditor.Utility
{
    public class ImageUtils
    {
        /// <summary>
        /// Gets the minimum stride required for containing an image of the given width and bits per pixel.
        /// </summary>
        /// <param name="width">Image width.</param>
        /// <param name="bitsLength">bits length of each pixel.</param>
        /// <returns>The minimum stride required for containing an image of the given width and bits per pixel.</returns>
        public static int GetMinimumStride(int width, int bitsLength)
        {
            return ((bitsLength * width) + 7) / 8;
        }

        /// <summary>
        /// Gets the classic stride rounded to 4 bytes for an image with the given width and bits per pixel.
        /// </summary>
        /// <param name="width">Image width.</param>
        /// <param name="bitsLength">bits length of each pixel.</param>
        /// <returns>The classic stride rounded to 4 bytes for an image with the given width and bits per pixel.</returns>
        public static int GetClassicStride(int width, int bitsLength)
        {
            return (((((bitsLength * width) + 7) / 8) + 3) / 4) * 4;
        }

        /// <summary>
        /// Converts bit per pixel for indexed formats (1, 4 or 8) to a PixelFormat enum.
        /// </summary>
        /// <param name="bpp">bits length of each pixel.</param>
        /// <returns>The PixelFormat enum for this bpp value.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static PixelFormat GetIndexedPixelFormat(int bpp)
        {
            switch (bpp)
            {
                case 1: return PixelFormat.Format1bppIndexed;
                case 4: return PixelFormat.Format4bppIndexed;
                case 8: return PixelFormat.Format8bppIndexed;
                default: throw new ArgumentException("Unsupported indexed pixel format '" + bpp + "'.", "bpp");
            }
        }

        /// <summary>
        /// Gets the raw bytes from an image in its original pixel format. This automatically
        /// collapses the stride, and returns the data using the minimum stride.
        /// </summary>
        /// <param name="sourceImage">The image to get the bytes from.</param>
        /// <returns>The raw bytes of the image.</returns>
        public static byte[] GetImageData(Bitmap sourceImage)
        {
            int stride;
            return GetImageData(sourceImage, out stride, sourceImage.PixelFormat, true);
        }

        /// <summary>
        /// Gets the raw bytes from an image in its original pixel format, with its original in-memory stride.
        /// </summary>
        /// <param name="sourceImage">The image to get the bytes from.</param>
        /// <param name="stride">Stride of the retrieved image data.</param>
        /// <returns>The raw bytes of the image.</returns>
        public static byte[] GetImageData(Bitmap sourceImage, out int stride)
        {
            return GetImageData(sourceImage, out stride, sourceImage.PixelFormat, false);
        }

        /// <summary>
        /// Gets the raw bytes from an image in the desired pixel format. This automatically
        /// collapses the stride, and returns the data using the minimum stride.
        /// </summary>
        /// <param name="sourceImage">The image to get the bytes from.</param>
        /// <param name="desiredPixelFormat">PixelFormat in which the data needs to be retrieved. Use <paramref name="sourceImage"/>.PixelFormat for no conversion.</param>
        /// <returns>The raw bytes of the image.</returns>
        public static byte[] GetImageData(Bitmap sourceImage, PixelFormat desiredPixelFormat)
        {
            int stride;
            return GetImageData(sourceImage, out stride, desiredPixelFormat, true);
        }

        /// <summary>
        /// Gets the raw bytes from an image in its original pixel format.
        /// </summary>
        /// <param name="sourceImage">The image to get the bytes from.</param>
        /// <param name="collapseStride">Collapse the stride to the minimum required for the image data.</param>
        /// <returns>The raw bytes of the image.</returns>
        public static byte[] GetImageData(Bitmap sourceImage, bool collapseStride)
        {
            int stride;
            return GetImageData(sourceImage, out stride, sourceImage.PixelFormat, collapseStride);
        }

        /// <summary>
        /// Gets the raw bytes from an image, in the given <see cref="System.Drawing.Imaging.PixelFormat">PixelFormat</see>, with its original in-memory stride.
        /// </summary>
        /// <param name="sourceImage">The image to get the bytes from.</param>
        /// <param name="stride">Stride of the retrieved image data.</param>
        /// <param name="desiredPixelFormat">PixelFormat in which the data needs to be retrieved. Use <paramref name="sourceImage"/>.PixelFormat for no conversion.</param>
        /// <returns>The raw bytes of the image.</returns>
        /// <remarks>
        ///   Note that <paramref name="desiredPixelFormat"/> has limitations when it comes to indexed formats:
        ///   giving an indexed pixel format if the sourceImage is an indexed image with a lower bpp will throw an exception, since GDI+ does not support that,
        ///   and if you give an indexed pixel format and the source is non-indexed, the colors will be matched to the standard Windows palette for that format.
        /// </remarks>
        public static byte[] GetImageData(Bitmap sourceImage, out int stride, PixelFormat desiredPixelFormat)
        {
            return GetImageData(sourceImage, out stride, desiredPixelFormat, false);
        }

        /// <summary>
        /// Gets the raw bytes from an image, in its original <see cref="System.Drawing.Imaging.PixelFormat">PixelFormat</see>.
        /// </summary>
        /// <param name="sourceImage">The image to get the bytes from.</param>
        /// <param name="stride">Stride of the retrieved image data.</param>
        /// <param name="collapseStride">Collapse the stride to the minimum required for the image data.</param>
        /// <returns>The raw bytes of the image.</returns>
        public static byte[] GetImageData(Bitmap sourceImage, out int stride, bool collapseStride)
        {
            if (sourceImage == null)
                throw new ArgumentNullException("sourceImage", "Source image is null!");
            Rectangle rect = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
            return GetImageDataInternal(sourceImage, out stride, ref rect, sourceImage.PixelFormat, collapseStride);
        }

        /// <summary>
        /// Gets the raw bytes from an image, in the desired <see cref="System.Drawing.Imaging.PixelFormat">PixelFormat</see>.
        /// </summary>
        /// <param name="sourceImage">The image to get the bytes from.</param>
        /// <param name="desiredPixelFormat">PixelFormat in which the data needs to be retrieved. Use <paramref name="sourceImage"/>.PixelFormat for no conversion.</param>
        /// <param name="collapseStride">Collapse the stride to the minimum required for the image data.</param>
        /// <returns>The raw bytes of the image.</returns>
        /// <remarks>
        ///   Note that <paramref name="desiredPixelFormat"/> has limitations when it comes to indexed formats:
        ///   giving an indexed pixel format if the sourceImage is an indexed image with a lower bpp will throw an exception, since GDI+ does not support that,
        ///   and if you give an indexed pixel format and the source is non-indexed, the colors will be matched to the standard Windows palette for that format.
        /// </remarks>
        public static byte[] GetImageData(Bitmap sourceImage, PixelFormat desiredPixelFormat, bool collapseStride)
        {
            int stride;
            return GetImageData(sourceImage, out stride, desiredPixelFormat, collapseStride);
        }

        /// <summary>
        /// Gets the raw bytes from an image, in the desired <see cref="System.Drawing.Imaging.PixelFormat">PixelFormat</see>.
        /// </summary>
        /// <param name="sourceImage">The image to get the bytes from.</param>
        /// <param name="stride">Stride of the retrieved image data.</param>
        /// <param name="desiredPixelFormat">PixelFormat in which the data needs to be retrieved. Use <paramref name="sourceImage"/>.PixelFormat for no conversion.</param>
        /// <param name="collapseStride">Collapse the stride to the minimum required for the image data.</param>
        /// <returns>The raw bytes of the image.</returns>
        /// <remarks>
        ///   Note that <paramref name="desiredPixelFormat"/> has limitations when it comes to indexed formats:
        ///   giving an indexed pixel format if the sourceImage is an indexed image with a lower bpp will throw an exception, since GDI+ does not support that,
        ///   and if you give an indexed pixel format and the source is non-indexed, the colors will be matched to the standard Windows palette for that format.
        /// </remarks>
        public static byte[] GetImageData(Bitmap sourceImage, out int stride, PixelFormat desiredPixelFormat, bool collapseStride)
        {
            Rectangle rect = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
            return GetImageData(sourceImage, out stride, ref rect, desiredPixelFormat, collapseStride);
        }

        /// <summary>
        /// Gets the raw bytes from an image, in the desired <see cref="System.Drawing.Imaging.PixelFormat">PixelFormat</see>.
        /// </summary>
        /// <param name="sourceImage">The image to get the bytes from.</param>
        /// <param name="stride">Stride of the retrieved image data.</param>
        /// <param name="desiredPixelFormat">PixelFormat in which the data needs to be retrieved. Use <paramref name="sourceImage"/>.PixelFormat for no conversion.</param>
        /// <param name="collapseStride">Collapse the stride to the minimum required for the image data.</param>
        /// <returns>The raw bytes of the image.</returns>
        /// <remarks>
        ///   Note that <paramref name="desiredPixelFormat"/> has limitations when it comes to indexed formats:
        ///   giving an indexed pixel format if the sourceImage is an indexed image with a lower bpp will throw an exception, since GDI+ does not support that,
        ///   and if you give an indexed pixel format and the source is non-indexed, the colors will be matched to the standard Windows palette for that format.
        /// </remarks>
        public static byte[] GetImageData(Bitmap sourceImage, out int stride, ref Rectangle area, PixelFormat desiredPixelFormat, bool collapseStride)
        {
            if (sourceImage == null)
                throw new ArgumentNullException("sourceImage", "Source image is null!");
            PixelFormat sourcePf = sourceImage.PixelFormat;
            if (sourcePf != desiredPixelFormat && (sourcePf & PixelFormat.Indexed) != 0 && (desiredPixelFormat & PixelFormat.Indexed) != 0
                && Image.GetPixelFormatSize(sourcePf) > Image.GetPixelFormatSize(desiredPixelFormat))
                throw new ArgumentException("Cannot convert from a higher to a lower indexed pixel format! Use ConvertTo8Bit / ConvertFrom8Bit instead!", "desiredPixelFormat");
            return GetImageDataInternal(sourceImage, out stride, ref area, desiredPixelFormat, collapseStride);
        }

        private static byte[] GetImageDataInternal(Bitmap sourceImage, out int stride, ref Rectangle area, PixelFormat desiredPixelFormat, bool collapseStride)
        {
            int imageWidth = sourceImage.Width;
            int imageHeight = sourceImage.Height;
            bool useArea = area.X > 0 || area.Y > 0 || area.Width != imageWidth || area.Height != imageHeight;
            if (useArea)
            {
                collapseStride = true;
                area.Width = Math.Min(Math.Max(0, imageWidth - area.X), area.Width);
                area.Height = Math.Min(Math.Max(0, imageHeight - area.Y), area.Height);
                if (area.Width == 0 || area.Height == 0)
                {
                    stride = 0;
                    return new byte[0];
                }
            }
            byte[] data;
            BitmapData sourceData = null;
            try
            {
                int width = imageWidth;
                int height = imageHeight;
                sourceData = sourceImage.LockBits(area, ImageLockMode.ReadOnly, desiredPixelFormat);
                stride = sourceData.Stride;
                int pixelFormatSize = Image.GetPixelFormatSize(desiredPixelFormat);
                int actualDataWidth = ((Image.GetPixelFormatSize(desiredPixelFormat) * area.Width) + 7) / 8;
                if (collapseStride && (useArea || actualDataWidth != stride))
                {
                    long sourcePos = sourceData.Scan0.ToInt64();
                    int destPos = 0;
                    data = new byte[actualDataWidth * area.Height];
                    byte clearMask = 0xFF;
                    if (pixelFormatSize < 8 && (width % 8) != 0)
                    {
                        int lastByteRemainder = width % 8;
                        clearMask = (byte)(~((pixelFormatSize == 1 ? (0xFF >> lastByteRemainder) : (0xFF << lastByteRemainder)) & 0xFF));
                    }
                    for (int y = 0; y < area.Height; ++y)
                    {
                        Marshal.Copy(new IntPtr(sourcePos), data, destPos, actualDataWidth);
                        sourcePos += stride;
                        destPos += actualDataWidth;
                        if (clearMask != 0xFF)
                            data[destPos - 1] &= clearMask;
                    }
                    stride = actualDataWidth;
                }
                else
                {
                    data = new byte[stride * height];
                    Marshal.Copy(sourceData.Scan0, data, 0, data.Length);
                }
                return data;
            }
            finally
            {
                if (sourceData != null)
                {
                    sourceImage.UnlockBits(sourceData);
                }
            }
        }

        /// <summary>
        /// Clones an image object to free it from any backing resources.
        /// Code taken from http://stackoverflow.com/a/3661892/ with some extra fixes.
        /// </summary>
        /// <param name="sourceImage">The image to clone.</param>
        /// <returns>The cloned image.</returns>
        public static Bitmap CloneImage(Bitmap sourceImage)
        {
            Rectangle rect = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
            Bitmap targetImage = new Bitmap(rect.Width, rect.Height, sourceImage.PixelFormat);
            targetImage.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
            BitmapData sourceData = sourceImage.LockBits(rect, ImageLockMode.ReadOnly, sourceImage.PixelFormat);
            BitmapData targetData = targetImage.LockBits(rect, ImageLockMode.WriteOnly, targetImage.PixelFormat);
            int actualDataWidth = ((Image.GetPixelFormatSize(sourceImage.PixelFormat) * rect.Width) + 7) / 8;
            int h = sourceImage.Height;
            int origStride = sourceData.Stride;
            int targetStride = targetData.Stride;
            byte[] imageData = new byte[actualDataWidth];
            long sourcePos = sourceData.Scan0.ToInt64();
            long destPos = targetData.Scan0.ToInt64();
            // Copy line by line, skipping by stride but copying actual data width
            for (int y = 0; y < h; ++y)
            {
                Marshal.Copy(new IntPtr(sourcePos), imageData, 0, actualDataWidth);
                Marshal.Copy(imageData, 0, new IntPtr(destPos), actualDataWidth);
                sourcePos += origStride;
                destPos += targetStride;
            }
            targetImage.UnlockBits(targetData);
            sourceImage.UnlockBits(sourceData);
            // For indexed images, restore the palette. This is not linking to a referenced
            // object in the original image; the getter of Palette creates a new object when called.
            if ((sourceImage.PixelFormat & PixelFormat.Indexed) != 0)
                targetImage.Palette = sourceImage.Palette;
            // Restore DPI settings
            targetImage.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
            return targetImage;
        }

        /// <summary>
        /// Creates a bitmap based on data, width, height, stride and pixel format.
        /// </summary>
        /// <param name="sourceData">Byte array of raw source data.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="stride">Scanline length inside the data.</param>
        /// <param name="pixelFormat">Pixel format.</param>
        /// <param name="palette">Color palette.</param>
        /// <param name="defaultColor">Default color to fill in on the palette if the given colors don't fully fill it.</param>
        /// <returns>The new image.</returns>
        public static Bitmap BuildImage(byte[] sourceData, int width, int height, int stride, PixelFormat pixelFormat, Color[] palette, Color? defaultColor)
        {
            Bitmap newImage = new Bitmap(width, height, pixelFormat);
            newImage.SetResolution(96, 96);
            BitmapData targetData = newImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, newImage.PixelFormat);
            int newDataWidth = ((Image.GetPixelFormatSize(pixelFormat) * width) + 7) / 8;
            int targetStride = targetData.Stride;
            long scan0 = targetData.Scan0.ToInt64();
            for (int y = 0; y < height; ++y)
                Marshal.Copy(sourceData, y * stride, new IntPtr(scan0 + y * targetStride), newDataWidth);
            newImage.UnlockBits(targetData);
            // For indexed images, set the palette.
            if ((pixelFormat & PixelFormat.Indexed) != 0 && (palette != null || defaultColor.HasValue))
            {
                if (palette == null)
                    palette = new Color[0];
                ColorPalette pal = newImage.Palette;
                int palLenNew = pal.Entries.Length;
                int minLen = Math.Min(palLenNew, palette.Length);
                for (int i = 0; i < minLen; ++i)
                    pal.Entries[i] = palette[i];
                // Fill in remainder with default if needed.
                if (palLenNew > palette.Length && defaultColor.HasValue)
                    for (int i = palette.Length; i < palLenNew; ++i)
                        pal.Entries[i] = defaultColor.Value;
                // Palette property getter creates a copy, so the newly filled in palette
                // is not actually referenced in the image until you set it again explicitly.
                newImage.Palette = pal;
            }
            return newImage;
        }


        public static Bitmap PaintOn32bpp(Image image, Color? transparencyFillColor)
        {
            Bitmap bp = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
            bp.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using (Graphics gr = Graphics.FromImage(bp))
            {
                if (transparencyFillColor.HasValue)
                    using (SolidBrush myBrush = new SolidBrush(Color.FromArgb(255, transparencyFillColor.Value)))
                        gr.FillRectangle(myBrush, new Rectangle(0, 0, image.Width, image.Height));
                gr.DrawImage(image, new Rectangle(0, 0, bp.Width, bp.Height));
            }
            return bp;
        }

        /// <summary>
        /// Find the most common color in an image.
        /// </summary>
        /// <param name="image">Input image.</param>
        /// <returns>The most common color found in the image. If there are multiple with the same frequency, the first one that was encountered is returned.</returns>
        public static Color FindMostCommonColor(Image image)
        {
            // Avoid unnecessary getter calls
            int height = image.Height;
            int width = image.Width;
            int stride;
            byte[] imageData;
            // Get image data, in 32bpp
            using (Bitmap bm = PaintOn32bpp(image, Color.Empty))
                imageData = GetImageData(bm, out stride);
            Color[] colors = FindMostCommonColors(1, imageData, width, height, stride);
            return colors.Length > 0 ? colors[0] : Color.Empty;
        }

        /// <summary>
        /// Find the most common colors in the data of a 32-bit image.
        /// </summary>
        /// <param name="image">Input image.</param>
        /// <returns>The most common color found in the image. If there are multiple with the same frequency, the first one that was encountered is returned.</returns>
        public static Color[] FindMostCommonColors(int amount, byte[] imageData, int width, int height, int stride)
        {
            // Store color frequencies in a dictionary.
            if (amount < 0)
                amount = int.MaxValue;
            Dictionary<Color, int> colorFreq = new Dictionary<Color, int>();
            int lineStart = 0;
            for (int y = 0; y < height; ++y)
            {
                // Reset offset on every line, since stride is not guaranteed to always be width * pixel size.
                int inputOffs = lineStart;
                //Final offset = y * linelength + x * pixellength.
                // To avoid recalculating that offset each time we just increase it with the pixel size at the end of each x iteration,
                // and increase the line start with the stride at the end of each y iteration.
                for (int x = 0; x < width; ++x)
                {
                    //Get color components out. "ARGB" is actually the order in the final integer which is read as little-endian, so the real order is BGRA.
                    Color col = Color.FromArgb(imageData[inputOffs + 3], imageData[inputOffs + 2], imageData[inputOffs + 1], imageData[inputOffs]);
                    // Only look at nontransparent pixels; cut off at 127.
                    if (col.A > 127)
                    {
                        // Save as pure color without alpha
                        Color bareCol = Color.FromArgb(255, col);
                        if (!colorFreq.ContainsKey(bareCol))
                            colorFreq.Add(bareCol, 1);
                        else
                            colorFreq[bareCol]++;
                    }
                    // Increase the offset by the pixel width. For 32bpp ARGB, each pixel is 4 bytes.
                    inputOffs += 4;
                }
                lineStart += stride;
            }
            // Get the maximum value in the dictionary values
            return colorFreq.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).Take(amount).ToArray();
        }

        /// <summary>
        /// Copies a piece out of an 8-bit image. The stride of the output will always equal the width.
        /// </summary>
        /// <param name="imageData">Byte data of the image.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="stride">Stride of the image.</param>
        /// <param name="copyArea">The area to copy.</param>
        /// <returns>The requested piece of the image.</returns>
        public static byte[] CopyFrom8bpp(byte[] imageData, int width, int height, int stride, Rectangle copyArea)
        {
            byte[] copiedPicture = new byte[copyArea.Width * copyArea.Height];
            int maxY = Math.Min(height - copyArea.Y, copyArea.Height);
            int maxX = Math.Min(width - copyArea.X, copyArea.Width);

            for (int y = 0; y < maxY; ++y)
            {
                for (int x = 0; x < maxX; ++x)
                {
                    // This will hit the same byte multiple times
                    int indexSource = (copyArea.Y + y) * stride + copyArea.X + x;
                    // This will always get a new index
                    int indexDest = y * copyArea.Width + x;
                    copiedPicture[indexDest] = imageData[indexSource];
                }
            }
            return copiedPicture;
        }

        /// <summary>
        /// Pastes 8-bit data on an 8-bit image.
        /// </summary>
        /// <param name="destData">Byte data of the image that is pasted on.</param>
        /// <param name="destWidth">Width of the image that is pasted on.</param>
        /// <param name="destHeight">Height of the image that is pasted on.</param>
        /// <param name="destStride">Stride of the image that is pasted on.</param>
        /// <param name="pasteData">Byte data of the image to paste.</param>
        /// <param name="pasteWidth">Width of the image to paste.</param>
        /// <param name="pasteHeight">Height of the image to paste.</param>
        /// <param name="pasteStride">Stride of the image to paste.</param>
        /// <param name="targetPos">Position at which to paste the image.</param>
        /// <param name="palTransparencyMask">Boolean array determining which offsets on the color palette will be treated as transparent. Use null for no transparency.</param>
        /// <param name="modifyOrig">True to modify the original array rather than returning a copy.</param>
        /// <returns>A new byte array with the combined data, and the same stride as the source image.</returns>
        public static byte[] PasteOn8bpp(byte[] destData, int destWidth, int destHeight, int destStride,
            byte[] pasteData, int pasteWidth, int pasteHeight, int pasteStride,
            Rectangle targetPos, bool[] palTransparencyMask, bool modifyOrig)
        {
            return PasteOn8bpp(destData, destWidth, destHeight, destStride, pasteData, pasteWidth, pasteHeight, pasteStride, targetPos, palTransparencyMask, modifyOrig, null);
        }

        /// <summary>
        /// Pastes 8-bit data on an 8-bit image.
        /// </summary>
        /// <param name="destData">Byte data of the image that is pasted on.</param>
        /// <param name="destWidth">Width of the image that is pasted on.</param>
        /// <param name="destHeight">Height of the image that is pasted on.</param>
        /// <param name="destStride">Stride of the image that is pasted on.</param>
        /// <param name="pasteData">Byte data of the image to paste.</param>
        /// <param name="pasteWidth">Width of the image to paste.</param>
        /// <param name="pasteHeight">Height of the image to paste.</param>
        /// <param name="pasteStride">Stride of the image to paste.</param>
        /// <param name="targetPos">Position at which to paste the image.</param>
        /// <param name="palTransparencyMask">Boolean array determining which offsets on the color palette will be treated as transparent. Use null for no transparency.</param>
        /// <param name="modifyOrig">True to modify the original array rather than returning a copy.</param>
        /// <param name="transparencyMask">For image-based transparency masking rather than palette based. Values in the array set to true are treated as transparent.
        /// If given, should have a size of exactly <see cref="pasteWidth"/> * <see cref="pasteHeight"/>.</param>
        /// <returns>A new byte array with the combined data, and the same stride as the source image.</returns>
        public static byte[] PasteOn8bpp(byte[] destData, int destWidth, int destHeight, int destStride,
            byte[] pasteData, int pasteWidth, int pasteHeight, int pasteStride,
            Rectangle targetPos, bool[] palTransparencyMask, bool modifyOrig, bool[] transparencyMask)
        {
            if (targetPos.Width != pasteWidth || targetPos.Height != pasteHeight)
                pasteData = CopyFrom8bpp(pasteData, pasteWidth, pasteHeight, pasteStride, new Rectangle(0, 0, targetPos.Width, targetPos.Height));
            byte[] finalFileData;
            if (modifyOrig)
            {
                finalFileData = destData;
            }
            else
            {
                finalFileData = new byte[destData.Length];
                Array.Copy(destData, finalFileData, destData.Length);
            }
            bool[] isTransparent = new bool[256];
            if (palTransparencyMask != null)
            {
                int len = Math.Min(isTransparent.Length, palTransparencyMask.Length);
                for (int i = 0; i < len; ++i)
                    isTransparent[i] = palTransparencyMask[i];
            }
            bool transMaskGiven = transparencyMask != null && transparencyMask.Length == pasteWidth * pasteHeight;
            int maxY = Math.Min(destHeight - targetPos.Y, targetPos.Height);
            int maxX = Math.Min(destWidth - targetPos.X, targetPos.Width);
            for (int y = 0; y < maxY; ++y)
            {
                for (int x = 0; x < maxX; ++x)
                {
                    int indexSource = y * pasteStride + x;
                    int indexTrans = transMaskGiven ? y * pasteWidth + x : 0;
                    byte data = pasteData[indexSource];
                    if (isTransparent[data] || (transMaskGiven && transparencyMask[indexTrans]))
                        continue;
                    int indexDest = (targetPos.Y + y) * destStride + targetPos.X + x;
                    // This will always get a new index
                    finalFileData[indexDest] = data;
                }
            }
            return finalFileData;
        }

        /// <summary>
        /// Converts given raw image data for a paletted image to 8-bit, so we have a simple one-byte-per-pixel format to work with.
        /// Stride is assumed to be the minimum needed to contain the data. Output stride will be the same as the width.
        /// </summary>
        /// <param name="fileData">The file data.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="start">Start offset of the image data in the fileData parameter.</param>
        /// <param name="bitsLength">Amount of bits used by one pixel.</param>
        /// <param name="bigEndian">True if the bits in the original image data are stored as big-endian.</param>
        /// <returns>The image data in a 1-byte-per-pixel format, with a stride exactly the same as the width.</returns>
        public static byte[] ConvertTo8Bit(byte[] fileData, int width, int height, int start, int bitsLength, bool bigEndian)
        {
            int stride = GetMinimumStride(width, bitsLength);
            return ConvertTo8Bit(fileData, width, height, start, bitsLength, bigEndian, ref stride);
        }

        /// <summary>
        /// Converts given raw image data for a paletted image to 8-bit, so we have a simple one-byte-per-pixel format to work with.
        /// The new stride at the end of the operation will always equal the width.
        /// </summary>
        /// <param name="fileData">The file data.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="start">Start offset of the image data in the fileData parameter.</param>
        /// <param name="bitsLength">Amount of bits used by one pixel.</param>
        /// <param name="bigEndian">True if the bits in the original image data are stored as big-endian.</param>
        /// <param name="stride">Stride used in the original image data. Will be adjusted to the new stride value, which will always equal the width.</param>
        /// <returns>The image data in a 1-byte-per-pixel format, with a stride exactly the same as the width.</returns>
        public static byte[] ConvertTo8Bit(byte[] fileData, int width, int height, int start, int bitsLength, bool bigEndian, ref int stride)
        {
            if (bitsLength != 1 && bitsLength != 2 && bitsLength != 4 && bitsLength != 8)
                throw new ArgumentOutOfRangeException("Cannot handle image data with " + bitsLength + "bits per pixel.");
            // Full array
            byte[] data8bit = new byte[width * height];
            // Amount of runs that end up on the same pixel
            int parts = 8 / bitsLength;
            // Amount of bytes to read per width
            int newStride = width;
            // Bit mask for reducing read and shifted data to actual bits length
            int bitmask = (1 << bitsLength) - 1;
            int size = stride * height;
            // File check, and getting actual data.
            if (start + size > fileData.Length)
                throw new IndexOutOfRangeException("Data exceeds array bounds!");
            // Actual conversion process.
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    // This will hit the same byte multiple times
                    int indexXbit = start + y * stride + x / parts;
                    // This will always get a new index
                    int index8bit = y * newStride + x;
                    // Amount of bits to shift the data to get to the current pixel data
                    int shift = (x % parts) * bitsLength;
                    // Reversed for big-endian
                    if (bigEndian)
                        shift = 8 - shift - bitsLength;
                    // Get data and store it.
                    data8bit[index8bit] = (byte)((fileData[indexXbit] >> shift) & bitmask);
                }
            }
            stride = newStride;
            return data8bit;
        }

        public static Rectangle CalculateOpaqueBounds(Bitmap bitmap)
        {
            BitmapData data = null;
            try
            {
                PixelFormat dataFormat = bitmap.PixelFormat;
                bool isIndexed = dataFormat.HasFlag(PixelFormat.Indexed);
                if (dataFormat == PixelFormat.Format24bppRgb || dataFormat == PixelFormat.Format32bppRgb)
                {
                    return new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                }
                List<int> transColors = null;
                if (isIndexed)
                {
                    Color[] entries = bitmap.Palette.Entries;
                    transColors = new List<int>();
                    for (int i = 0; i < entries.Length; ++i)
                    {
                        if (entries[i].A == 0)
                            transColors.Add(i);
                    }
                }
                data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, dataFormat);
                int stride = data.Stride;
                byte[] bytes = new byte[stride * data.Height];
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                int pfs = Image.GetPixelFormatSize(bitmap.PixelFormat);
                if (isIndexed)
                {
                    if (dataFormat != PixelFormat.Format8bppIndexed)
                    {
                        bytes = ConvertTo8Bit(bytes, bitmap.Width, bitmap.Height, 0, pfs, pfs == 1, ref stride);
                    }
                    return CalculateOpaqueBounds8bpp(bytes, data.Width, data.Height, stride, transColors.ToArray());
                }
                else
                {
                    return CalculateOpaqueBoundsHiCol(bytes, data.Width, data.Height, pfs / 8, stride);
                }
            }
            finally
            {
                if (data != null)
                {
                    bitmap.UnlockBits(data);
                }
            }
        }

        /// <summary>
        /// Calculates the actually opaque bounds of a 24bpp or 32bpp image given as bytes.
        /// </summary>
        /// <param name="data">Image data.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="bytespp">Bytes per pixel.</param>
        /// <param name="stride">Stride of the image.</param>
        /// <returns></returns>
        public static Rectangle CalculateOpaqueBoundsHiCol(byte[] data, int width, int height, int bytespp, int stride)
        {
            // Only handle 32bpp data.
            if (bytespp != 4)
            {
                return new Rectangle(0, 0, width, height);
            }
            // Modified this function to result in (0,0,0,0) when the image is empty, rather than retaining the full size.
            int lineWidth = width * bytespp;
            bool isTransparentRow(int y)
            {
                var start = y * stride;
                for (var i = bytespp - 1; i < lineWidth; i += bytespp)
                {
                    // Bytes are [A,R,G,B], so alpha is first.
                    if (data[start + i] != 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            var opaqueBounds = new Rectangle(0, 0, width, height);
            for (int y = height - 1; y >= 0; --y)
            {
                if (!isTransparentRow(y))
                {
                    break;
                }
                opaqueBounds.Height = y;
            }
            int endHeight = opaqueBounds.Height;
            for (int y = 0; y < endHeight; ++y)
            {
                if (!isTransparentRow(y))
                {
                    opaqueBounds.Y = y;
                    opaqueBounds.Height = endHeight - y;
                    break;
                }
            }
            bool isTransparentColumn(int x)
            {
                var start = (x * bytespp) + (bytespp - 1);
                for (var y = opaqueBounds.Top; y < opaqueBounds.Bottom; ++y)
                {
                    if (data[start + (y * stride)] != 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            for (int x = width - 1; x >= 0; --x)
            {
                if (!isTransparentColumn(x))
                {
                    break;
                }
                opaqueBounds.Width = x;
            }
            int endWidth = opaqueBounds.Width;
            for (int x = 0; x < endWidth; ++x)
            {
                if (!isTransparentColumn(x))
                {
                    opaqueBounds.X = x;
                    opaqueBounds.Width = endWidth - x;
                    break;
                }
            }
            return opaqueBounds;
        }

        public static Rectangle CalculateOpaqueBounds8bpp(byte[] data, int width, int height, int stride, params int[] transparentColors)
        {
            HashSet<int> trMap = new HashSet<int>(transparentColors);
            // Only handle 32bpp data.
            // Modified this function to result in (0,0,0,0) when the image is empty, rather than retaining the full size.
            int lineWidth = width;
            bool isTransparentRow(int y)
            {
                int start = y * stride;
                int end = start + lineWidth;
                for (var i = start; i < end; ++i)
                {
                    if (!trMap.Contains(data[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
            var opaqueBounds = new Rectangle(0, 0, width, height);
            for (int y = height - 1; y >= 0; --y)
            {
                if (!isTransparentRow(y))
                {
                    break;
                }
                opaqueBounds.Height = y;
            }
            int endHeight = opaqueBounds.Height;
            for (int y = 0; y < endHeight; ++y)
            {
                if (!isTransparentRow(y))
                {
                    opaqueBounds.Y = y;
                    opaqueBounds.Height = endHeight - y;
                    break;
                }
            }
            bool isTransparentColumn(int x)
            {
                var pos = opaqueBounds.Top * stride + x;
                for (var y = 0; y < opaqueBounds.Height; ++y)
                {
                    if (!trMap.Contains(data[pos]))
                    {
                        return false;
                    }
                    pos += stride;
                }
                return true;
            }
            for (int x = width - 1; x >= 0; --x)
            {
                if (!isTransparentColumn(x))
                {
                    break;
                }
                opaqueBounds.Width = x;
            }
            int endWidth = opaqueBounds.Width;
            for (int x = 0; x < endWidth; ++x)
            {
                if (!isTransparentColumn(x))
                {
                    opaqueBounds.X = x;
                    opaqueBounds.Width = endWidth - x;
                    break;
                }
            }
            return opaqueBounds;
        }

        /// <summary>
        /// Generates an area to fill to create an outline around an image.
        /// </summary>
        /// <param name="tileSize">Size of the tile, in pixels.</param>
        /// <param name="image">Image to detect outline on.</param>
        /// <param name="outline">Outline thickness, in fraction of tile size. Give 0 to instead return the detected area.</param>
        /// <param name="alphaThreshold">Alpha threshold for the detection.</param>
        /// <param name="includeBlack">Include non-transparent black in the criteria for what to consider "transparent". This can be used to ignore black shadows.</param>
        /// <returns>Region data to fill to create an outline of the object in the image.</returns>
        public static RegionData GetOutline(Size tileSize, Bitmap image, float outline, byte alphaThreshold, bool includeBlack)
        {
            byte[] imgData = ImageUtils.GetImageData(image, out int stride, PixelFormat.Format32bppArgb, true);
            int actualOutlineX = (int)Math.Max(1, outline * tileSize.Width);
            int actualOutlineY = (int)Math.Max(1, outline * tileSize.Height);
            Rectangle imageBounds = new Rectangle(Point.Empty, image.Size);

            bool isOpaque(byte[] mapdata, int yVal, int xVal, bool checkBlack)
            {
                int address = yVal * stride + xVal * 4;
                // Check alpha
                byte alpha = mapdata[address + 3];
                if (mapdata[address + 3] <= alphaThreshold)
                {
                    return false;
                }
                if (alpha == 255 || !checkBlack)
                {
                    return true;
                }
                // Check brightness to exclude shadow
                byte red = mapdata[address + 2];
                byte grn = mapdata[address + 1];
                byte blu = mapdata[address + 0];
                // Integer method.
                int redBalanced = red * red * 2126;
                int grnBalanced = grn * grn * 7152;
                int bluBalanced = blu * blu * 0722;
                int lum = (redBalanced + grnBalanced + bluBalanced) / 255 / 255;
                // The integer division will automatically reduce anything near-black
                // to zero, so actually checking against a threshold is unnecessary.
                return lum > 0; // lum > lumThresholdSq * 1000
            };

            //Func<byte[], int, int, bool> isOpaque_ = (mapdata, yVal, xVal) => mapdata[yVal * stride + xVal * 4 + 3] >= alphaThreshold;
            List<List<Point>> blobs = BlobDetection.FindBlobs(imgData, image.Width, image.Height, (d, y, x) => isOpaque(d, y, x, !includeBlack), true, false);
            HashSet<Point> allblobs = new HashSet<Point>();
            foreach (List<Point> blob in blobs)
            {
                foreach (Point p in blob)
                {
                    allblobs.Add(p);
                }
            }
            HashSet<Point> drawPoints = new HashSet<Point>();
            HashSet<Point> removePoints = new HashSet<Point>();
            foreach (Point p in allblobs)
            {
                Rectangle rect = new Rectangle(p.X, p.Y, 1, 1);
                // If outline is specified as 0, return inner area.
                if (outline > 0)
                {
                    removePoints.UnionWith(rect.Points());
                    rect.Inflate(actualOutlineX, actualOutlineY);
                }
                rect.Intersect(imageBounds);
                if (!rect.IsEmpty)
                {
                    drawPoints.UnionWith(rect.Points());
                }
            }
            foreach (Point p in removePoints)
            {
                drawPoints.Remove(p);
            }
            RegionData rData;
            using (Region r = new Region())
            {
                r.MakeEmpty();
                Size pixelSize = new Size(1, 1);
                foreach (Point p in drawPoints)
                {
                    r.Union(new Rectangle(p, pixelSize));
                }
                rData = r.GetRegionData();
            }
            return rData;
        }

        /// <summary>
        /// Converts an image to paletted format.
        /// </summary>
        /// <param name="originalImage">Original image.</param>
        /// <param name="bpp">Desired bits per pixel for the paletted image (should be less than or equal to 8).</param>
        /// <param name="palette">The color palette.</param>
        /// <returns>A bitmap of the desired color depth matched to the given palette.</returns>
        public static Bitmap ConvertToPalette(Bitmap originalImage, int bpp, Color[] palette)
        {
            PixelFormat pf = GetIndexedPixelFormat(bpp);
            int stride;
            byte[] imageData;
            if (originalImage.PixelFormat != PixelFormat.Format32bppArgb)
            {
                using (Bitmap bm32bpp = PaintOn32bpp(originalImage, Color.Black))
                    imageData = GetImageData(bm32bpp, out stride);
            }
            else
                imageData = GetImageData(originalImage, out stride);
            byte[] palettedData = Convert32BitToPaletted(imageData, originalImage.Width, originalImage.Height, bpp, true, palette, ref stride);
            return BuildImage(palettedData, originalImage.Width, originalImage.Height, stride, pf, palette, Color.Black);
        }

        /// <summary>
        /// Converts 32 bit per pixel image data to match a given color palette, and returns it as array in the desired pixel format.
        /// </summary>
        /// <param name="imageData">Image data.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="bpp">Bits per pixel.</param>
        /// <param name="bigEndianBits">True to use big endian ordered data in the indexed array if <paramref name="bpp "/> is less than 8.</param>
        /// <param name="palette">Color palette to match to.</param>
        /// <param name="stride">Stride. Will be adjusted by the function.</param>
        /// <returns>The converted indexed data.</returns>
        public static byte[] Convert32BitToPaletted(byte[] imageData, int width, int height, int bpp, bool bigEndianBits, Color[] palette, ref int stride)
        {
            if (stride < width * 4)
                throw new ArgumentException("Stride is smaller than one pixel line.", "stride");
            byte[] newImageData = new byte[width * height];
            List<int> transparentIndices = new List<int>();
            int maxLen = Math.Min(0x100, palette.Length);
            for (int i = 0; i < maxLen; ++i)
                if (palette[i].A == 0)
                    transparentIndices.Add(i);
            int firstTransIndex = transparentIndices.Count > 0 ? transparentIndices[0] : -1;
            // Mapping table. Takes more memory, but it's way faster, especially on sprites with clear backgrounds.
            Dictionary<uint, byte> colorMapping = new Dictionary<uint, byte>();
            for (int y = 0; y < height; ++y)
            {
                int inputOffs = y * stride;
                int outputOffs = y * width;
                for (int x = 0; x < width; ++x)
                {
                    Color c = Color.FromArgb(imageData[inputOffs + 3], imageData[inputOffs + 2], imageData[inputOffs + 1], imageData[inputOffs]);
                    uint colKey = (uint)c.ToArgb();
                    byte outInd;
                    if (colorMapping.ContainsKey(colKey))
                    {
                        outInd = colorMapping[colKey];
                    }
                    else
                    {
                        if (firstTransIndex >= 0 && c.A < 128)
                            outInd = (byte)firstTransIndex;
                        else
                            outInd = (byte)GetClosestPaletteIndexMatch(c, palette, transparentIndices);
                        colorMapping.Add(colKey, outInd);
                    }
                    newImageData[outputOffs] = outInd;
                    inputOffs += 4;
                    outputOffs++;
                }
            }
            stride = width;
            if (bpp < 8)
                newImageData = ConvertFrom8Bit(newImageData, width, height, bpp, bigEndianBits, ref stride);
            return newImageData;
        }

        /// <summary>
        /// Uses Pythagorean distance in 3D color space to find the closest match to a given color on
        /// a given color palette, and returns the index on the palette at which that match was found.
        /// </summary>
        /// <param name="col">The color to find the closest match to</param>
        /// <param name="colorPalette">The palette of available colors to match</param>
        /// <param name="excludedindices">List of palette indices that are specifically excluded from the search.</param>
        /// <returns>The index on the palette of the color that is the closest to the given color.</returns>
        public static int GetClosestPaletteIndexMatch(Color col, Color[] colorPalette, IEnumerable<int> excludedindexes)
        {
            int colorMatch = 0;
            int leastDistance = int.MaxValue;
            int red = col.R;
            int green = col.G;
            int blue = col.B;
            for (int i = 0; i < colorPalette.Length; ++i)
            {
                if (excludedindexes != null && excludedindexes.Contains(i))
                    continue;
                Color paletteColor = colorPalette[i];
                int redDistance = paletteColor.R - red;
                int greenDistance = paletteColor.G - green;
                int blueDistance = paletteColor.B - blue;
                int distance = (redDistance * redDistance) + (greenDistance * greenDistance) + (blueDistance * blueDistance);
                if (distance >= leastDistance)
                    continue;
                colorMatch = i;
                leastDistance = distance;
                if (distance == 0)
                    return i;
            }
            return colorMatch;
        }

        /// <summary>
        /// Converts given raw image data for a paletted 8-bit image to lower amount of bits per pixel.
        /// Stride is assumed to be the same as the width. Output stride is the minimum needed to contain the data.
        /// </summary>
        /// <param name="data8bit">The eight bit per pixel image data</param>
        /// <param name="width">The width of the image</param>
        /// <param name="height">The height of the image</param>
        /// <param name="newBitLength">The new amount of bits per pixel</param>
        /// <param name="bigEndian">True if the blocks of pixels in the new image data are to be stored as big-endian, meaning, the highest values are the leftmost pixels.</param>
        /// <returns>The image data converted to the requested amount of bits per pixel.</returns>
        public static byte[] ConvertFrom8Bit(byte[] data8bit, int width, int height, int newBitLength, bool bigEndian)
        {
            int stride = width;
            return ConvertFrom8Bit(data8bit, width, height, newBitLength, bigEndian, ref stride);
        }

        /// <summary>
        /// Converts given raw image data for a paletted 8-bit image to lower amount of bits per pixel.
        /// </summary>
        /// <param name="data8bit">The eight bit per pixel image data.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="newBitLength">The new amount of bits per pixel.</param>
        /// <param name="bigEndian">True if the blocks of pixels in the new image data are to be stored as big-endian, meaning, the highest values are the leftmost pixels.</param>
        /// <param name="stride">Stride used in the original image data. Will be adjusted to the new stride value.</param>
        /// <returns>The image data converted to the requested amount of bits per pixel.</returns>
        public static byte[] ConvertFrom8Bit(byte[] data8bit, int width, int height, int newBitLength, bool bigEndian, ref int stride)
        {
            if (newBitLength > 8)
                throw new ArgumentException("Cannot convert to bit format greater than 8.", "newBitLength");
            if (stride < width)
                throw new ArgumentException("Stride is too small for the given width.", "stride");
            if (data8bit.Length < stride * height)
                throw new ArgumentException("Data given data is too small to contain an 8-bit image of the given dimensions", "data8bit");
            int parts = 8 / newBitLength;
            // Amount of bytes to write per line
            int newStride = GetMinimumStride(width, newBitLength);
            // Bit mask for reducing original data to actual bits maximum.
            // Should not be needed if data is correct, but eh.
            int bitmask = (1 << newBitLength) - 1;
            byte[] dataXbit = new byte[newStride * height];
            // Actual conversion process.
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    // This will hit the same byte multiple times
                    int indexXbit = y * newStride + x / parts;
                    // This will always get a new index
                    int index8bit = y * stride + x;
                    // Amount of bits to shift the data to get to the current pixel data
                    int shift = (x % parts) * newBitLength;
                    // Reversed for big-endian
                    if (bigEndian)
                        shift = 8 - shift - newBitLength;
                    // Get data, reduce to bit rate, shift it and store it.
                    dataXbit[indexXbit] |= (byte)((data8bit[index8bit] & bitmask) << shift);
                }
            }
            stride = newStride;
            return dataXbit;
        }

        public static byte[] SetPngTextChunk(byte[] data, string keyword, string value)
        {
            const string TEXTCHUNK = "tEXt";
            const string IDATCHUNK = "IDAT";
            if (!PngHandler.IsPng(data))
            {
                return data;
            }
            Encoding enc = Encoding.GetEncoding("ISO-8859-1");
            List<int> textChunkOffsets = new List<int>();
            List<string> textChunkKeywords = new List<string>();
            int foundOffs = 0;
            int foundLen = 0;
            int offs = 0;
            // Try to find first existing match for keyword
            while (offs != -1)
            {
                int newOffs = PngHandler.FindPngChunk(data, offs, TEXTCHUNK);
                if (newOffs == -1)
                {
                    break;
                }
                int len = PngHandler.GetPngChunkDataLength(data, newOffs);
                byte[] foundTextData = PngHandler.GetPngChunkData(data, newOffs, len);
                string key = enc.GetString(foundTextData.TakeWhile(b => b != 0).ToArray());
                if (key == keyword)
                {
                    foundOffs = offs;
                    foundLen = PngHandler.GetPngChunkDataLength(data, offs);
                    break;
                }
                offs = newOffs + len + 12;
            }
            // No existing match found. Place the chunk before the image data chunk
            if (foundOffs == 0)
            {
                foundOffs = PngHandler.FindPngChunk(data, 0, IDATCHUNK);
                // Required chunk
                if (foundOffs == -1)
                {
                    return data;
                }
            }
            int afterChunkOffs = foundOffs + foundLen;
            int afterChunkLen = data.Length - afterChunkOffs;
            // Make data
            byte[] keywordData = enc.GetBytes(keyword);
            byte[] valueData = enc.GetBytes(value);
            int textLen = keywordData.Length + 1 + valueData.Length;
            byte[] textData = new byte[keywordData.Length + 1 + valueData.Length];
            Array.Copy(keywordData, 0, textData, 0, keywordData.Length);
            Array.Copy(valueData, 0, textData, keywordData.Length + 1, valueData.Length);
            // Make chunk
            byte[] textChunk = PngHandler.MakePngChunk(TEXTCHUNK, textData);
            // Stitch together the new png data
            byte[] newData = new byte[foundOffs + textChunk.Length + afterChunkLen];
            Array.Copy(data, 0, newData, 0, foundOffs);
            Array.Copy(textChunk, 0, newData, foundOffs, textChunk.Length);
            Array.Copy(data, afterChunkOffs, newData, foundOffs + textChunk.Length, afterChunkLen);
            return newData;
        }
    }
}
