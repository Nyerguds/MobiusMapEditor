using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
            // Store colour frequencies in a dictionary.
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
                    //Get colour components out. "ARGB" is actually the order in the final integer which is read as little-endian, so the real order is BGRA.
                    Color col = Color.FromArgb(imageData[inputOffs + 3], imageData[inputOffs + 2], imageData[inputOffs + 1], imageData[inputOffs]);
                    // Only look at nontransparent pixels; cut off at 127.
                    if (col.A > 127)
                    {
                        // Save as pure colour without alpha
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

        public static Rectangle CalculateOpaqueBounds(Bitmap bitmap)
        {
            BitmapData data = null;
            try
            {
                data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                var bpp = Image.GetPixelFormatSize(data.PixelFormat) / 8;
                var bytes = new byte[data.Stride * data.Height];
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                return ImageUtils.CalculateOpaqueBounds(bytes, data.Width, data.Height, bpp, data.Stride);
            }
            finally
            {
                if (data != null)
                {
                    bitmap.UnlockBits(data);
                }
            }
        }

        public static Rectangle CalculateOpaqueBounds(byte[] data, int width, int height, int bytespp, int stride)
        {
            // Modified this function to result in (0,0,0,0) when the image is empty, rather than retaining the full size.
            int lineWidth = width * bytespp;
            bool isTransparentRow(int y)
            {
                var start = y * stride;
                for (var i = bytespp - 1; i < lineWidth; i += bytespp)
                {
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

    }
}
