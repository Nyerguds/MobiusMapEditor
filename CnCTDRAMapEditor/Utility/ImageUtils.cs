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

namespace MobiusEditor.Utility
{
    public class ImageUtils
    {

        /// <summary>
        /// Gets the raw bytes from an image in its original pixel format. This automatically
        /// collapses the stride, and returns the data using the minimum stride.
        /// </summary>
        /// <param name="sourceImage">The image to get the bytes from.</param>
        /// <returns>The raw bytes of the image.</returns>
        public static Byte[] GetImageData(Bitmap sourceImage)
        {
            Int32 stride;
            return GetImageData(sourceImage, out stride, sourceImage.PixelFormat, true);
        }

        /// <summary>
        /// Gets the raw bytes from an image in its original pixel format, with its original in-memory stride.
        /// </summary>
        /// <param name="sourceImage">The image to get the bytes from.</param>
        /// <param name="stride">Stride of the retrieved image data.</param>
        /// <returns>The raw bytes of the image.</returns>
        public static Byte[] GetImageData(Bitmap sourceImage, out Int32 stride)
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
        public static Byte[] GetImageData(Bitmap sourceImage, PixelFormat desiredPixelFormat)
        {
            Int32 stride;
            return GetImageData(sourceImage, out stride, desiredPixelFormat, true);
        }

        /// <summary>
        /// Gets the raw bytes from an image in its original pixel format.
        /// </summary>
        /// <param name="sourceImage">The image to get the bytes from.</param>
        /// <param name="collapseStride">Collapse the stride to the minimum required for the image data.</param>
        /// <returns>The raw bytes of the image.</returns>
        public static Byte[] GetImageData(Bitmap sourceImage, Boolean collapseStride)
        {
            Int32 stride;
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
        public static Byte[] GetImageData(Bitmap sourceImage, out Int32 stride, PixelFormat desiredPixelFormat)
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
        public static Byte[] GetImageData(Bitmap sourceImage, out Int32 stride, Boolean collapseStride)
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
        public static Byte[] GetImageData(Bitmap sourceImage, PixelFormat desiredPixelFormat, Boolean collapseStride)
        {
            Int32 stride;
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
        public static Byte[] GetImageData(Bitmap sourceImage, out Int32 stride, PixelFormat desiredPixelFormat, Boolean collapseStride)
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
        public static Byte[] GetImageData(Bitmap sourceImage, out Int32 stride, ref Rectangle area, PixelFormat desiredPixelFormat, Boolean collapseStride)
        {
            if (sourceImage == null)
                throw new ArgumentNullException("sourceImage", "Source image is null!");
            PixelFormat sourcePf = sourceImage.PixelFormat;
            if (sourcePf != desiredPixelFormat && (sourcePf & PixelFormat.Indexed) != 0 && (desiredPixelFormat & PixelFormat.Indexed) != 0
                && Image.GetPixelFormatSize(sourcePf) > Image.GetPixelFormatSize(desiredPixelFormat))
                throw new ArgumentException("Cannot convert from a higher to a lower indexed pixel format! Use ConvertTo8Bit / ConvertFrom8Bit instead!", "desiredPixelFormat");
            return GetImageDataInternal(sourceImage, out stride, ref area, desiredPixelFormat, collapseStride);
        }

        private static Byte[] GetImageDataInternal(Bitmap sourceImage, out Int32 stride, ref Rectangle area, PixelFormat desiredPixelFormat, Boolean collapseStride)
        {
            Int32 imageWidth = sourceImage.Width;
            Int32 imageHeight = sourceImage.Height;
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
            Byte[] data;
            BitmapData sourceData = null;
            try
            {
                Int32 width = imageWidth;
                Int32 height = imageHeight;
                sourceData = sourceImage.LockBits(area, ImageLockMode.ReadOnly, desiredPixelFormat);
                stride = sourceData.Stride;
                Int32 pixelFormatSize = Image.GetPixelFormatSize(desiredPixelFormat);
                Int32 actualDataWidth = ((Image.GetPixelFormatSize(desiredPixelFormat) * area.Width) + 7) / 8;
                if (collapseStride && (useArea || actualDataWidth != stride))
                {
                    Int64 sourcePos = sourceData.Scan0.ToInt64();
                    Int32 destPos = 0;
                    data = new Byte[actualDataWidth * area.Height];
                    Byte clearMask = 0xFF;
                    if (pixelFormatSize < 8 && (width % 8) != 0)
                    {
                        int lastByteRemainder = width % 8;
                        clearMask = (Byte)(~((pixelFormatSize == 1 ? (0xFF >> lastByteRemainder) : (0xFF << lastByteRemainder)) & 0xFF));
                    }
                    for (Int32 y = 0; y < area.Height; ++y)
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
                    data = new Byte[stride * height];
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
            Int32 actualDataWidth = ((Image.GetPixelFormatSize(sourceImage.PixelFormat) * rect.Width) + 7) / 8;
            Int32 h = sourceImage.Height;
            Int32 origStride = sourceData.Stride;
            Int32 targetStride = targetData.Stride;
            Byte[] imageData = new Byte[actualDataWidth];
            Int64 sourcePos = sourceData.Scan0.ToInt64();
            Int64 destPos = targetData.Scan0.ToInt64();
            // Copy line by line, skipping by stride but copying actual data width
            for (Int32 y = 0; y < h; ++y)
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
        public static Bitmap BuildImage(Byte[] sourceData, Int32 width, Int32 height, Int32 stride, PixelFormat pixelFormat, Color[] palette, Color? defaultColor)
        {
            Bitmap newImage = new Bitmap(width, height, pixelFormat);
            newImage.SetResolution(96, 96);
            BitmapData targetData = newImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, newImage.PixelFormat);
            Int32 newDataWidth = ((Image.GetPixelFormatSize(pixelFormat) * width) + 7) / 8;
            Int32 targetStride = targetData.Stride;
            Int64 scan0 = targetData.Scan0.ToInt64();
            for (Int32 y = 0; y < height; ++y)
                Marshal.Copy(sourceData, y * stride, new IntPtr(scan0 + y * targetStride), newDataWidth);
            newImage.UnlockBits(targetData);
            // For indexed images, set the palette.
            if ((pixelFormat & PixelFormat.Indexed) != 0 && (palette != null || defaultColor.HasValue))
            {
                if (palette == null)
                    palette = new Color[0];
                ColorPalette pal = newImage.Palette;
                Int32 palLenNew = pal.Entries.Length;
                Int32 minLen = Math.Min(palLenNew, palette.Length);
                for (Int32 i = 0; i < minLen; ++i)
                    pal.Entries[i] = palette[i];
                // Fill in remainder with default if needed.
                if (palLenNew > palette.Length && defaultColor.HasValue)
                    for (Int32 i = palette.Length; i < palLenNew; ++i)
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
            Int32 height = image.Height;
            Int32 width = image.Width;
            Int32 stride;
            Byte[] imageData;
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
                amount = Int32.MaxValue;
            Dictionary<Color, Int32> colorFreq = new Dictionary<Color, Int32>();
            Int32 lineStart = 0;
            for (Int32 y = 0; y < height; ++y)
            {
                // Reset offset on every line, since stride is not guaranteed to always be width * pixel size.
                Int32 inputOffs = lineStart;
                //Final offset = y * linelength + x * pixellength.
                // To avoid recalculating that offset each time we just increase it with the pixel size at the end of each x iteration,
                // and increase the line start with the stride at the end of each y iteration.
                for (Int32 x = 0; x < width; ++x)
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
        /// Gets the minimum stride required for containing an image of the given width and bits per pixel.
        /// </summary>
        /// <param name="width">Image width.</param>
        /// <param name="bitsLength">bits length of each pixel.</param>
        /// <returns>The minimum stride required for containing an image of the given width and bits per pixel.</returns>
        public static Int32 GetMinimumStride(Int32 width, Int32 bitsLength)
        {
            return ((bitsLength * width) + 7) / 8;
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
        public static Byte[] CopyFrom8bpp(Byte[] imageData, Int32 width, Int32 height, Int32 stride, Rectangle copyArea)
        {
            Byte[] copiedPicture = new Byte[copyArea.Width * copyArea.Height];
            Int32 maxY = Math.Min(height - copyArea.Y, copyArea.Height);
            Int32 maxX = Math.Min(width - copyArea.X, copyArea.Width);

            for (Int32 y = 0; y < maxY; ++y)
            {
                for (Int32 x = 0; x < maxX; ++x)
                {
                    // This will hit the same byte multiple times
                    Int32 indexSource = (copyArea.Y + y) * stride + copyArea.X + x;
                    // This will always get a new index
                    Int32 indexDest = y * copyArea.Width + x;
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
        /// <returns>A new Byte array with the combined data, and the same stride as the source image.</returns>
        public static Byte[] PasteOn8bpp(Byte[] destData, Int32 destWidth, Int32 destHeight, Int32 destStride,
            Byte[] pasteData, Int32 pasteWidth, Int32 pasteHeight, Int32 pasteStride,
            Rectangle targetPos, Boolean[] palTransparencyMask, Boolean modifyOrig)
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
        /// <returns>A new Byte array with the combined data, and the same stride as the source image.</returns>
        public static Byte[] PasteOn8bpp(Byte[] destData, Int32 destWidth, Int32 destHeight, Int32 destStride,
            Byte[] pasteData, Int32 pasteWidth, Int32 pasteHeight, Int32 pasteStride,
            Rectangle targetPos, Boolean[] palTransparencyMask, Boolean modifyOrig, Boolean[] transparencyMask)
        {
            if (targetPos.Width != pasteWidth || targetPos.Height != pasteHeight)
                pasteData = CopyFrom8bpp(pasteData, pasteWidth, pasteHeight, pasteStride, new Rectangle(0, 0, targetPos.Width, targetPos.Height));
            Byte[] finalFileData;
            if (modifyOrig)
            {
                finalFileData = destData;
            }
            else
            {
                finalFileData = new Byte[destData.Length];
                Array.Copy(destData, finalFileData, destData.Length);
            }
            Boolean[] isTransparent = new Boolean[256];
            if (palTransparencyMask != null)
            {
                Int32 len = Math.Min(isTransparent.Length, palTransparencyMask.Length);
                for (Int32 i = 0; i < len; ++i)
                    isTransparent[i] = palTransparencyMask[i];
            }
            Boolean transMaskGiven = transparencyMask != null && transparencyMask.Length == pasteWidth * pasteHeight;
            Int32 maxY = Math.Min(destHeight - targetPos.Y, targetPos.Height);
            Int32 maxX = Math.Min(destWidth - targetPos.X, targetPos.Width);
            for (Int32 y = 0; y < maxY; ++y)
            {
                for (Int32 x = 0; x < maxX; ++x)
                {
                    Int32 indexSource = y * pasteStride + x;
                    Int32 indexTrans = transMaskGiven ? y * pasteWidth + x : 0;
                    Byte data = pasteData[indexSource];
                    if (isTransparent[data] || (transMaskGiven && transparencyMask[indexTrans]))
                        continue;
                    Int32 indexDest = (targetPos.Y + y) * destStride + targetPos.X + x;
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
        public static Byte[] ConvertTo8Bit(Byte[] fileData, Int32 width, Int32 height, Int32 start, Int32 bitsLength, Boolean bigEndian)
        {
            Int32 stride = GetMinimumStride(width, bitsLength);
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
        public static Byte[] ConvertTo8Bit(Byte[] fileData, Int32 width, Int32 height, Int32 start, Int32 bitsLength, Boolean bigEndian, ref Int32 stride)
        {
            if (bitsLength != 1 && bitsLength != 2 && bitsLength != 4 && bitsLength != 8)
                throw new ArgumentOutOfRangeException("Cannot handle image data with " + bitsLength + "bits per pixel.");
            // Full array
            Byte[] data8bit = new Byte[width * height];
            // Amount of runs that end up on the same pixel
            Int32 parts = 8 / bitsLength;
            // Amount of bytes to read per width
            Int32 newStride = width;
            // Bit mask for reducing read and shifted data to actual bits length
            Int32 bitmask = (1 << bitsLength) - 1;
            Int32 size = stride * height;
            // File check, and getting actual data.
            if (start + size > fileData.Length)
                throw new IndexOutOfRangeException("Data exceeds array bounds!");
            // Actual conversion process.
            for (Int32 y = 0; y < height; ++y)
            {
                for (Int32 x = 0; x < width; ++x)
                {
                    // This will hit the same byte multiple times
                    Int32 indexXbit = start + y * stride + x / parts;
                    // This will always get a new index
                    Int32 index8bit = y * newStride + x;
                    // Amount of bits to shift the data to get to the current pixel data
                    Int32 shift = (x % parts) * bitsLength;
                    // Reversed for big-endian
                    if (bigEndian)
                        shift = 8 - shift - bitsLength;
                    // Get data and store it.
                    data8bit[index8bit] = (Byte)((fileData[indexXbit] >> shift) & bitmask);
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
                if (!dataFormat.HasFlag(PixelFormat.Indexed) && dataFormat != PixelFormat.Format24bppRgb || dataFormat != PixelFormat.Format32bppRgb)
                {
                    return new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                }
                List<int> transColors = null;
                if ((dataFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
                {
                    dataFormat = PixelFormat.Format8bppIndexed;
                    Color[] entries = bitmap.Palette.Entries;
                    transColors = new List<int>();
                    for (int i = 0; i < entries.Length; i++)
                    {
                        if (entries[i].A == 0)
                            transColors.Add(i);
                    }
                }
                data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, dataFormat);
                int stride = data.Stride;
                byte[] bytes = new byte[stride * data.Height];
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                int pfs = Image.GetPixelFormatSize(bitmap.PixelFormat);
                if (dataFormat.HasFlag(PixelFormat.Indexed))
                {
                    if (dataFormat != PixelFormat.Format8bppIndexed)
                    {
                        bytes = ConvertTo8Bit(bytes, bitmap.Width, bitmap.Height, 0, pfs, pfs == 1, ref stride);
                    }
                    return CalculateOpaqueBounds8bpp(bytes, data.Width, data.Height, stride, transColors.ToArray());
                }
                else
                {
                    return CalculateOpaqueBoundsHiCol(bytes, data.Width, data.Height, pfs / 8, data.Stride);
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
    }
}
