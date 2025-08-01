//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// The Command & Conquer Map Editor and corresponding source code is distributed
// in the hope that it will be useful, but with permitted additional restrictions
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT
// distributed with this program. You should have received a copy of the
// GNU General Public License along with permitted additional restrictions
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MobiusEditor.Utility
{
    public static class ExtensionMethods
    {
        public static float ToLinear(this float v)
        {
            return (v < 0.04045f) ? (v * 25.0f / 323.0f) : (float)Math.Pow(((200.0f * v) + 11.0f) / 211.0f, 12.0f / 5.0f);
        }

        public static float ToLinear(this byte v)
        {
            return (v / 255.0f).ToLinear();
        }

        public static float ToSRGB(this float v)
        {
            return (v < 0.0031308) ? (v * 323.0f / 25.0f) : ((((float)Math.Pow(v, 5.0f / 12.0f) * 211.0f) - 11.0f) / 200.0f);
        }

        public static void SetDefault<T>(this T data)
        {
            var properties = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetSetMethod() != null);
            foreach (var property in properties)
            {
                if (property.GetCustomAttribute(typeof(DefaultValueAttribute)) is DefaultValueAttribute defaultValueAttr)
                {
                    property.SetValue(data, defaultValueAttr.Value);
                }
            }
        }

        public static int CheckRange(this int value, int minValue, int maxValue, int errorValue)
        {
            return value < minValue || value > maxValue ? errorValue : value;
        }

        public static long CheckRange(this long value, long minValue, long maxValue, long errorValue)
        {
            return value < minValue || value > maxValue ? errorValue : value;
        }

        public static int Restrict(this int value, int minValue, int maxValue)
        {
            return Math.Max(minValue, Math.Min(value, maxValue));
        }

        public static long Restrict(this long value, long minValue, long maxValue)
        {
            return Math.Max(minValue, Math.Min(value, maxValue));
        }

        public static float Restrict(this float value, float minValue, float maxValue)
        {
            return Math.Max(minValue, Math.Min(value, maxValue));
        }

        public static double Restrict(this double value, double minValue, double maxValue)
        {
            return Math.Max(minValue, Math.Min(value, maxValue));
        }

        /// <summary>
        /// Alternative to <see cref="Enum.HasFlag(Enum)"/> that returns <see langword="true"/> even on a partial match of the bits.
        /// </summary>
        /// <typeparam name="T">Type of the enum.</typeparam>
        /// <param name="source">Source to check the value of.</param>
        /// <param name="flag">Flag or flags to check.</param>
        /// <returns><see langword="true"/> if any of the bits in the given <paramref name="flag"/> are enabled in the <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="flag"/> or <paramref name="source"/> are null.</exception>
        public static bool HasAnyFlags<T>(this T source, T flag) where T : Enum
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (flag == null)
            {
                throw new ArgumentNullException("flag");
            }
            // Get the type code of the enumeration
            TypeCode typeCode = source.GetTypeCode();
            // If the underlying type of the flag is signed
            if (typeCode == TypeCode.SByte || typeCode == TypeCode.Int16 || typeCode == TypeCode.Int32 || typeCode == TypeCode.Int64)
            {
                return (Convert.ToInt64(source) & Convert.ToInt64(flag)) != 0;
            }
            // If the underlying type of the flag is unsigned
            if (typeCode == TypeCode.Byte || typeCode == TypeCode.UInt16 || typeCode == TypeCode.UInt32 || typeCode == TypeCode.UInt64)
            {
                return (Convert.ToUInt64(source) & Convert.ToUInt64(flag)) != 0;
            }
            return false;
        }

        /// <summary>
        /// Copies all public properties of one object into another.
        /// </summary>
        /// <typeparam name="T">Type of the objects.</typeparam>
        /// <param name="data">Data source to copy from</param>
        /// <param name="other">Destination to copy into.</param>
        /// <param name="ignoreAttributes">If an attribute of one of these specified types is found on a property, that property will not be copied.</param>
        public static void CopyTo<T>(this T data, T other, params Type[] ignoreAttributes)
        {
            List<Type> ignoreAttrTypeChecked = ignoreAttributes.Where(t => t != null && typeof(Attribute).IsAssignableFrom(t)).ToList();
            var properties = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => (p.GetSetMethod() != null) && (p.GetGetMethod() != null));
            foreach (var property in properties)
            {
                bool ignore = false;
                foreach (Type tp in ignoreAttrTypeChecked)
                {
                    Attribute att = Attribute.GetCustomAttribute(property, tp);
                    if (att != null)
                    {
                        ignore = true;
                        break;
                    }
                }
                if (ignore)
                {
                    continue;
                }
                var defaultValueAttr = property.GetCustomAttribute(typeof(DefaultValueAttribute)) as DefaultValueAttribute;
                property.SetValue(other, property.GetValue(data));
            }
        }

        public static Point CenterPoint(this Rectangle rectangle)
        {
            return new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);
        }

        public static Point TopLeft(this Rectangle rectangle)
        {
            return new Point(rectangle.X, rectangle.Y);
        }

        /// <summary>Returns a new point that is offset compared to the origin point.</summary>
        /// <remarks>Unlike <see cref="Point.Offset(int, int)"/>, this does not change the original point, but returns a new one.</remarks>
        /// <param name="origin">Origin point</param>
        /// <param name="dx">The amount to offset the x-coordinate.</param>
        /// <param name="dy">The amount to offset the y-coordinate.</param>
        /// <returns>A new point that is offset compared to the origin point.</returns>
        public static Point OffsetPoint(this Point origin, int dx, int dy)
        {
            Point p = new Point(origin.X, origin.Y);
            p.Offset(dx, dy);
            return p;
        }

        public static Size GetDimensions(this bool[,] mask)
        {
            return mask == null ? Size.Empty : new Size(mask.GetLength(1), mask.GetLength(0));
        }

        public static IEnumerable<Point> Points(this Rectangle rectangle)
        {
            for (var y = rectangle.Top; y < rectangle.Bottom; ++y)
            {
                for (var x = rectangle.Left; x < rectangle.Right; ++x)
                {
                    yield return new Point(x, y);
                }
            }
        }

        /// <summary>
        /// Returns the points contained in a specified border of a given rectangle. The border is only considered to be inside the rectangle area.
        /// If the thickness it exceeds Y/2 or X/2, the entire contents of the rectangle will be returned.
        /// </summary>
        /// <param name="rectangle">Rectangle</param>
        /// <param name="thickness">Thickness of the border inside the rectangle.</param>
        /// <returns></returns>
        public static IEnumerable<Point> BorderCells(this Rectangle rectangle, int thickness)
        {
            int startYTop = rectangle.Top;
            int endYTop = Math.Min(rectangle.Top + thickness, rectangle.Bottom);
            int startYBottom = Math.Max(rectangle.Bottom - thickness, endYTop);
            int endYBottom = rectangle.Bottom;

            int startXLeft = rectangle.Left;
            int endXLeft = Math.Min(rectangle.Left + thickness, rectangle.Right);
            int startXRight = Math.Max(rectangle.Right - thickness, endXLeft);
            int endXRight = rectangle.Right;

            // Top block: all points
            for (var y = startYTop; y < endYTop; ++y)
            {
                for (var x = rectangle.Left; x < rectangle.Right; ++x)
                {
                    yield return new Point(x, y);
                }
            }
            // Center block
            for (var y = endYTop; y < startYBottom; ++y)
            {
                // Left side
                for (var x = startXLeft; x < endXLeft; ++x)
                {
                    yield return new Point(x, y);
                }
                // Right side
                for (var x = startXRight; x < endXRight; ++x)
                {
                    yield return new Point(x, y);
                }
            }
            // Bottom block
            for (var y = startYBottom; y < endYBottom; ++y)
            {
                for (var x = rectangle.Left; x < rectangle.Right; ++x)
                {
                    yield return new Point(x, y);
                }
            }
        }

        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }

        public static void Clear<T>(this T[] array, T clearElement)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                array[i] = clearElement;
            }
        }

        public static void Clear<T>(this T[,] array, T clearElement)
        {
            int yMax = array.GetLength(0);
            int xMax = array.GetLength(1);
            for (int y = 0; y < yMax; ++y)
            {
                for (int x = 0; x < xMax; ++x)
                {
                    array[y, x] = clearElement;
                }
            }
        }

        /*/
        // Was added to make the editor work in .Net 4.6.2; No longer needed on 4.7.2.
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
        {
            return new HashSet<T>(source, comparer);
        }
        //*/

        public static IEnumerable<byte[]> Split(this byte[] bytes, int length)
        {
            for (int i = 0; i < bytes.Length; i += length)
            {
                yield return bytes.Skip(i).Take(Math.Min(length, bytes.Length - i)).ToArray();
            }
        }

        public static IEnumerable<string> Split(this string str, int length)
        {
            for (int i = 0; i < str.Length; i += length)
            {
                yield return str.Substring(i, Math.Min(length, str.Length - i));
            }
        }

        public static Font GetAdjustedFont(this Graphics graphics, string text, Font originalFont, int fitWidth, int fitHeight,
            int minSize, int maxSize, StringFormat stringFormat, bool smallestOnFail)
        {
            if (minSize > maxSize)
            {
                throw new ArgumentOutOfRangeException("minSize");
            }
            for (var size = maxSize; size >= minSize; --size)
            {
                Font font = new Font(originalFont.Name, size, originalFont.Style);
                int linesFilled;
                SizeF textSize = graphics.MeasureString(text, font, new SizeF(fitWidth, fitHeight), stringFormat, out _, out linesFilled);
                if (linesFilled == 1 && Convert.ToInt32(textSize.Width) < fitWidth && Convert.ToInt32(textSize.Width) < fitHeight)
                {
                    return font;
                }
            }
            return smallestOnFail ? new Font(originalFont.Name, minSize, originalFont.Style) : originalFont;
        }

        /// <summary>
        /// Returns true if both strings are equal, or if both are null, empty, or the given default value.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="other"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static bool EqualsOrDefaultIgnoreCase(this string str, string other, string def)
        {
            return EqualsOrDefault(str, other, def, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Returns true if both strings are equal, or if both are null, empty, or the given default value.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="other"></param>
        /// <param name="def"></param>
        /// <param name="sc"></param>
        /// <returns></returns>
        public static bool EqualsOrDefault(this string str, string other, string def, StringComparison sc)
        {
            return ((String.IsNullOrEmpty(str) || String.Equals(str, def, sc)) &&
                  (String.IsNullOrEmpty(other) || String.Equals(other, def, sc)))
                || String.Equals(str, other, sc);
        }

        public static T2 TryGetOrDefault<T1, T2>(this Dictionary<T1, T2> dictionary, T1 key, T2 defVal)
        {
            return dictionary == null || !dictionary.ContainsKey(key) ? defVal : dictionary[key];
        }

        public static void MergeWith<T1, T2>(this Dictionary<T1, T2> mergeTarget, params Dictionary<T1, T2>[] dictionariesToCombine)
        {
            foreach (Dictionary<T1, T2> dict in dictionariesToCombine)
            {
                foreach (KeyValuePair<T1, T2> item in dict)
                {
                    if (!mergeTarget.ContainsKey(item.Key))
                    {
                        mergeTarget.Add(item.Key, item.Value);
                    }
                }
            }
        }

        public static Bitmap FitToBoundingBox(this Image image, int maxWidth, int maxHeight)
        {
            return FitToBoundingBox(image, maxWidth, maxHeight, Color.Transparent);
        }

        public static Bitmap FitToBoundingBox(this Image image, int maxWidth, int maxHeight, Color clearColor)
        {
            return FitToBoundingBox(image, new Rectangle(0, 0, image.Width, image.Height), maxWidth, maxHeight, clearColor);
        }

        public static Bitmap FitToBoundingBox(this Image image, Rectangle cutout, int maxWidth, int maxHeight, Color clearColor)
        {
            Bitmap newImg = new Bitmap(maxWidth, maxHeight);
            newImg.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            Rectangle resized = GeneralUtils.GetBoundingBoxCenter(cutout.Width, cutout.Height, maxWidth, maxHeight);
            using (Graphics g = Graphics.FromImage(newImg))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                if (clearColor.ToArgb() != Color.Transparent.ToArgb())
                {
                    g.Clear(clearColor);
                }
                if (cutout.Width > 0 && cutout.Height > 0)
                {
                    g.DrawImage(image, resized, cutout, GraphicsUnit.Pixel);
                }
                g.Flush();
            }
            return newImg;
        }

        public static Rectangle GetBoundingBoxCenter(this Image image, int maxWidth, int maxHeight)
        {
            return GeneralUtils.GetBoundingBoxCenter(image.Width, image.Height, maxWidth, maxHeight);
        }

        public static void RemoveAlphaOnCurrent(this Bitmap bitmap)
        {
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            if ((bitmap.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
            {
                ColorPalette pal = bitmap.Palette;
                for (int i = 0; i < pal.Entries.Length; ++i)
                {
                    pal.Entries[i] = Color.FromArgb(255, pal.Entries[i]);
                }
                return;
            }
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                // can't handle.
                return;
            }
            BitmapData sourceData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int actualDataWidth = rect.Width * 4;
            int h = bitmap.Height;
            int origStride = sourceData.Stride;
            byte[] imageData = new byte[actualDataWidth];
            long sourcePos = sourceData.Scan0.ToInt64();
            // Copy line by line, skipping by stride but copying actual data width
            for (int y = 0; y < h; ++y)
            {
                Marshal.Copy(new IntPtr(sourcePos), imageData, 0, actualDataWidth);
                for (int i = 3; i < actualDataWidth; i += 4)
                {
                    // Clear alpha
                    imageData[i] = 255;
                }
                Marshal.Copy(imageData, 0, new IntPtr(sourcePos), actualDataWidth);
                sourcePos += origStride;
            }
            bitmap.UnlockBits(sourceData);
            return;
        }

        public static Bitmap RemoveAlpha(this Bitmap bitmap)
        {
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            Bitmap targetImage = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            targetImage.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);
            BitmapData sourceData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData targetData = targetImage.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            int actualDataWidth = ((Image.GetPixelFormatSize(PixelFormat.Format32bppArgb) * rect.Width) + 7) / 8;
            int h = bitmap.Height;
            int origStride = sourceData.Stride;
            int targetStride = targetData.Stride;
            byte[] imageData = new byte[actualDataWidth];
            long sourcePos = sourceData.Scan0.ToInt64();
            long destPos = targetData.Scan0.ToInt64();
            // Copy line by line, skipping by stride but copying actual data width
            for (int y = 0; y < h; ++y)
            {
                Marshal.Copy(new IntPtr(sourcePos), imageData, 0, actualDataWidth);
                for (int i = 3; i < actualDataWidth; i += 4)
                {
                    // Clear alpha
                    imageData[i] = 255;
                }
                Marshal.Copy(imageData, 0, new IntPtr(destPos), actualDataWidth);
                sourcePos += origStride;
                destPos += targetStride;
            }
            targetImage.UnlockBits(targetData);
            bitmap.UnlockBits(sourceData);
            targetImage.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);
            return targetImage;
        }

        public static Bitmap Sharpen(this Bitmap bitmap, double strength)
        {
            if (bitmap == null)
            {
                return null;
            }
            // Create sharpening filter.
            const int filterSize = 5;
            double[,] filter = new double[filterSize, filterSize]
            {
                {-1, -1, -1, -1, -1},
                {-1,  2,  2,  2, -1},
                {-1,  2, 16,  2, -1},
                {-1,  2,  2,  2, -1},
                {-1, -1, -1, -1, -1}
            };
            double bias = 1.0 - strength;
            double factor = strength / 16.0;
            const int border = filterSize / 2;
            var result = new Color[bitmap.Width, bitmap.Height];
            var sharpenImage = new Bitmap(bitmap);
            // Not sure if needed with a clone like this... still, better be sure.
            sharpenImage.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);
            int width = bitmap.Width;
            int height = bitmap.Height;
            // Lock image bits for read/write.
            BitmapData pbits = sharpenImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            // Declare an array to hold the bytes of the bitmap.
            int bytes = pbits.Stride * height;
            var rgbValues = new byte[bytes];
            // Copy the RGB values into the array.
            Marshal.Copy(pbits.Scan0, rgbValues, 0, bytes);
            int rgb;
            // Fill the color array with the new sharpened color values.
            for (int x = border; x < width - border; ++x)
            {
                for (int y = border; y < height - border; ++y)
                {
                    double red = 0.0, green = 0.0, blue = 0.0;

                    for (int filterX = 0; filterX < filterSize; ++filterX)
                    {
                        for (int filterY = 0; filterY < filterSize; ++filterY)
                        {
                            int imageX = (x - border + filterX + width) % width;
                            int imageY = (y - border + filterY + height) % height;
                            rgb = imageY * pbits.Stride + 3 * imageX;
                            red += rgbValues[rgb + 2] * filter[filterX, filterY];
                            green += rgbValues[rgb + 1] * filter[filterX, filterY];
                            blue += rgbValues[rgb + 0] * filter[filterX, filterY];
                        }
                        rgb = y * pbits.Stride + 3 * x;
                        int r = Math.Min(Math.Max((int)(factor * red + (bias * rgbValues[rgb + 2])), 0), 255);
                        int g = Math.Min(Math.Max((int)(factor * green + (bias * rgbValues[rgb + 1])), 0), 255);
                        int b = Math.Min(Math.Max((int)(factor * blue + (bias * rgbValues[rgb + 0])), 0), 255);
                        result[x, y] = Color.FromArgb(r, g, b);
                    }
                }
            }
            // Update the image with the sharpened pixels.
            for (int x = border; x < width - border; ++x)
            {
                for (int y = border; y < height - border; ++y)
                {
                    rgb = y * pbits.Stride + 3 * x;
                    rgbValues[rgb + 2] = result[x, y].R;
                    rgbValues[rgb + 1] = result[x, y].G;
                    rgbValues[rgb + 0] = result[x, y].B;
                }
            }
            // Copy the RGB values back to the bitmap.
            Marshal.Copy(rgbValues, 0, pbits.Scan0, bytes);
            // Release image bits.
            sharpenImage.UnlockBits(pbits);
            return sharpenImage;
        }

        /// <summary>
        /// Scales an image while avoiding edge artifacts, by first putting it in a larger frame, then scaling,
        /// and then cropping the result.
        /// </summary>
        /// <param name="bitmap">Bitmap to scale</param>
        /// <param name="width">Target width</param>
        /// <param name="height">Target height</param>
        /// <returns>A new bitmap, scaled to the requested size.</returns>
        public static Bitmap HighQualityScale(this Bitmap bitmap, int width, int height)
        {
            return HighQualityScale(bitmap, width, height, CompositingQuality.HighQuality,
                InterpolationMode.HighQualityBicubic, SmoothingMode.HighQuality, PixelOffsetMode.HighQuality);
        }

        /// <summary>
        /// Scales an image while avoiding edge artifacts, by first putting it in a larger frame, then scaling,
        /// and then cropping the result.
        /// </summary>
        /// <param name="bitmap">Bitmap to scale</param>
        /// <param name="width">Target width</param>
        /// <param name="height">Target height</param>
        /// <param name="compositingQuality">Compositing quality for the Graphics object that performs the scaling.</param>
        /// <param name="interpolationMode">Interpolation mode for the Graphics object that performs the scaling.</param>
        /// <param name="smoothingMode">Smoothing mode for the Graphics object that performs the scaling.</param>
        /// <param name="pixelOffsetMode">PixelOffset mode for the Graphics object that performs the scaling.</param>
        /// <returns>A new bitmap, scaled to the requested size using the given graphics settings.</returns>
        public static Bitmap HighQualityScale(this Bitmap bitmap, int width, int height, CompositingQuality compositingQuality,
            InterpolationMode interpolationMode, SmoothingMode smoothingMode, PixelOffsetMode pixelOffsetMode)
        {
            // The principle: make a frame that's larger than the original, fill the edges with repeats
            // of the image's outer pixels, scale to desired size, then crop. The edge size is calculated
            // to make the border in the resulting image at least 8 pixels wide.
            int bmWidth = bitmap.Width;
            int bmHeight = bitmap.Height;
            Size imageSize = new Size(bmWidth, bmHeight);
            Size maxSize = new Size(width, height);
            Size newSize = new Size(width, height);
            // If graphics are too large, scale them down using the largest dimension
            if (imageSize.Width > imageSize.Height)
            {
                newSize.Height = imageSize.Height * maxSize.Width / imageSize.Width;
                newSize.Width = maxSize.Width;
            }
            else if (imageSize.Height > imageSize.Width)
            {
                newSize.Width = imageSize.Width * maxSize.Height / imageSize.Height;
                newSize.Height = maxSize.Height;
            }
            // center graphics inside bounding box
            int locX = (maxSize.Width - newSize.Width) / 2;
            int locY = (maxSize.Height - newSize.Height) / 2;

            int borderFractionX = 1;
            const int fractionDividerX = 16;
            const int minimumEdge = 8;
            // Use larger border fraction if result is less than 'minimumEdge' pixels.
            while (newSize.Width * borderFractionX / fractionDividerX < minimumEdge || bmWidth * borderFractionX / fractionDividerX < minimumEdge)
            {
                // Increase is exponential until the full size (16/16) is reached, then the full size is added each time.
                borderFractionX = borderFractionX < fractionDividerX ? (borderFractionX * 2) : (borderFractionX + fractionDividerX);
            }
            int bmBorderWidth = bmWidth * borderFractionX / fractionDividerX;
            int borderFractionY = 1;
            const int fractionDividerY = 16;
            while (newSize.Height * borderFractionY / fractionDividerY < minimumEdge || bmHeight * borderFractionY / fractionDividerY < minimumEdge)
            {
                // Increase is exponential until the full size (16/16) is reached, then the full size is added each time.
                borderFractionY = borderFractionY < fractionDividerY ? (borderFractionY * 2) : (borderFractionY + fractionDividerY);
            }
            int bmBorderHeight = bmHeight * borderFractionY / fractionDividerY;
            // Get original image data.
            BitmapData sourceData = bitmap.LockBits(new Rectangle(0, 0, bmWidth, bmHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int stride = sourceData.Stride;
            byte[] bitmapData = new byte[stride * bmHeight];
            Marshal.Copy(sourceData.Scan0, bitmapData, 0, bitmapData.Length);
            bitmap.UnlockBits(sourceData);
            // Create buffer for expanded image.
            int expandWidth = bmWidth + bmBorderWidth * 2;
            int expandHeight = bmHeight + bmBorderHeight * 2;
            int expandStride = expandWidth * 4;
            byte[] expandData = new byte[expandStride * expandHeight];
            // Length of border width in bytes
            int borderWidthLength = bmBorderWidth * 4;
            // Define some general variables to reuse
            int readIndex = 0;
            int writeIndex = 0;
            byte[] colArr = new byte[4];
            // Copy start and end pixels of image to fill whole line, with image line in between
            readIndex = 0;
            writeIndex = expandStride * bmBorderHeight;
            for (int y = 0; y < bmHeight; ++y)
            {
                // Get start color and use it to fill the padded start of the line.
                Array.Copy(bitmapData, readIndex, colArr, 0, 4);
                for (int x = 0; x < bmBorderWidth; ++x)
                {
                    Array.Copy(colArr, 0, expandData, writeIndex, 4);
                    writeIndex += 4;
                }
                // Copy original image line.
                Array.Copy(bitmapData, readIndex, expandData, writeIndex, stride);
                writeIndex += stride;
                // Get end color and and use it to fill the padded end of the line.
                Array.Copy(bitmapData, readIndex + stride - 4, colArr, 0, 4);
                for (int x = 0; x < bmBorderWidth; ++x)
                {
                    Array.Copy(colArr, 0, expandData, writeIndex, 4);
                    writeIndex += 4;
                }
                readIndex += stride;
            }
            // Copy first constructed line over the entire top border part
            readIndex = expandStride * bmBorderHeight;
            writeIndex = 0;
            for (int y = 0; y < bmBorderHeight; ++y)
            {
                Array.Copy(expandData, readIndex, expandData, writeIndex, expandStride);
                writeIndex += expandStride;
            }
            // Copy last constructed line over the entire bottom border part
            readIndex = (bmBorderHeight + bmHeight - 1) * expandStride;
            writeIndex = (bmBorderHeight + bmHeight) * expandStride;
            for (int y = 0; y < bmBorderHeight; ++y)
            {
                Array.Copy(expandData, readIndex, expandData, writeIndex, expandStride);
                writeIndex += expandStride;
            }
            // Construct image from the data, scale it, crop it, and return the result.
            using (Bitmap expandImage = new Bitmap(expandWidth, expandHeight, PixelFormat.Format32bppArgb))
            {
                BitmapData targetData = expandImage.LockBits(new Rectangle(0, 0, expandWidth, expandHeight), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                int targetStride = targetData.Stride;
                long scan0 = targetData.Scan0.ToInt64();
                for (int y = 0; y < expandHeight; ++y)
                    Marshal.Copy(expandData, y * expandStride, new IntPtr(scan0 + y * targetStride), expandStride);
                expandImage.UnlockBits(targetData);
                expandImage.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);
                int borderWidth = newSize.Width * borderFractionX / fractionDividerX;
                int borderHeight = newSize.Height * borderFractionY / fractionDividerY;
                using (Bitmap scaledImage = new Bitmap(newSize.Width + borderWidth * 2, newSize.Height + borderHeight * 2, PixelFormat.Format32bppArgb))
                {
                    scaledImage.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);
                    // Scale expanded image to the intended size, accounting for the added border.
                    using (Graphics g2 = Graphics.FromImage(scaledImage))
                    {
                        g2.CompositingQuality = compositingQuality;
                        g2.CompositingMode = CompositingMode.SourceCopy;
                        g2.InterpolationMode = interpolationMode;
                        g2.SmoothingMode = smoothingMode;
                        g2.PixelOffsetMode = pixelOffsetMode;
                        g2.DrawImage(expandImage, 
                            new Rectangle(0, 0, newSize.Width + borderWidth * 2, newSize.Height + borderHeight * 2), 
                            new Rectangle(0, 0, expandWidth, expandHeight),
                            GraphicsUnit.Pixel);
                    }
                    // Finally, create actual image at intended size.
                    Bitmap cutoutImage = new Bitmap(newSize.Width, newSize.Height, PixelFormat.Format32bppArgb);
                    // Copy center part out of stretched image.
                    using (Graphics g3 = Graphics.FromImage(cutoutImage))
                    {
                        g3.CompositingQuality = CompositingQuality.AssumeLinear;
                        g3.CompositingMode = CompositingMode.SourceCopy;
                        g3.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g3.SmoothingMode = SmoothingMode.None;
                        g3.PixelOffsetMode = PixelOffsetMode.Half;
                        g3.DrawImage(scaledImage, 
                            new Rectangle(0, 0, newSize.Width, newSize.Height), 
                            new Rectangle(borderWidth, borderHeight, newSize.Width, newSize.Height), 
                            GraphicsUnit.Pixel);
                    }
                    //expandImage.Save(System.IO.Path.Combine(Program.ApplicationPath, "test_1_expand.png"), ImageFormat.Png);
                    //scaledImage.Save(System.IO.Path.Combine(Program.ApplicationPath, "test_2_scaled.png"), ImageFormat.Png);
                    //cutoutImage.Save(System.IO.Path.Combine(Program.ApplicationPath, "test_3_cutout.png"), ImageFormat.Png);
                    return cutoutImage;
                }
            }
        }
    }
}
