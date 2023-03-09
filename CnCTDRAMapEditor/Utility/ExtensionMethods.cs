//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free 
// software: you can redistribute it and/or modify it under the terms of 
// the GNU General Public License as published by the Free Software Foundation, 
// either version 3 of the License, or (at your option) any later version.

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

        public static void CopyTo<T>(this T data, T other)
        {
            var properties = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => (p.GetSetMethod() != null) && (p.GetGetMethod() != null));
            foreach (var property in properties)
            {
                var defaultValueAttr = property.GetCustomAttribute(typeof(DefaultValueAttribute)) as DefaultValueAttribute;
                property.SetValue(other, property.GetValue(data));
            }
        }

        public static Point CenterPoint(this Rectangle rectangle)
        {
            return new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);
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
            int startYBottom = Math.Max(rectangle.Bottom-thickness, endYTop);
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
                    array[y,x] = clearElement;
                }
            }
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
        {
            return new HashSet<T>(source, comparer);
        }

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
                || String.Equals(str,other, sc);
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
            Rectangle resized = GeneralUtils.GetBoundingBoxCenter(cutout.Width, cutout.Height, maxWidth, maxHeight);
            using (Graphics g = Graphics.FromImage(newImg))
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
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

        public static Bitmap RemoveAlpha(this Bitmap bitmap)
        {
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            Bitmap targetImage = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            targetImage.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);
            BitmapData sourceData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData targetData = targetImage.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Int32 actualDataWidth = ((Image.GetPixelFormatSize(bitmap.PixelFormat) * rect.Width) + 7) / 8;
            Int32 h = bitmap.Height;
            Int32 origStride = sourceData.Stride;
            Int32 targetStride = targetData.Stride;
            Byte[] imageData = new Byte[actualDataWidth];
            Int64 sourcePos = sourceData.Scan0.ToInt64();
            Int64 destPos = targetData.Scan0.ToInt64();
            // Copy line by line, skipping by stride but copying actual data width
            for (Int32 y = 0; y < h; ++y)
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
    }
}
