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
using Newtonsoft.Json.Linq;
using Pfim;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using TGASharpLib;

namespace MobiusEditor.Utility
{
    public class TextureManager
    {

        private static string MissingTexture = "DATA\\ART\\TEXTURES\\SRGB\\COMMON\\MISC\\MISSING.TGA";
        private bool processedMissingTexture = false;

        public string[] ExpandModPaths { get; set; }

#if false
        private class ImageData
        {
            public TGA TGA;
            public JObject Metadata;
        }
#endif

        private readonly MegafileManager megafileManager;

        private Dictionary<string, Bitmap> cachedTextures = new Dictionary<string, Bitmap>();
        private Dictionary<(string, TeamColor), (Bitmap, Rectangle)> teamColorTextures = new Dictionary<(string, TeamColor), (Bitmap, Rectangle)>();

        public TextureManager(MegafileManager megafileManager, params String[] expandModPaths)
        {
            this.megafileManager = megafileManager;
            this.ExpandModPaths = expandModPaths;
        }

        public void Reset()
        {
            Bitmap[] cachedImages = cachedTextures.Values.ToArray();
            cachedTextures.Clear();
            // Bitmaps need to be specifically disposed.
            for (int i = 0; i < cachedImages.Length; ++i)
            {
                try
                {
                    cachedImages[i].Dispose();
                }
                catch
                {
                    // Ignore.
                }
            }
            (Bitmap, Rectangle)[] cachedTeamImages = teamColorTextures.Values.ToArray();
            teamColorTextures.Clear();
            for (int i = 0; i < cachedTeamImages.Length; ++i)
            {
                try
                {
                    (Bitmap bitmap, Rectangle opaqueBounds) = cachedTeamImages[i];
                    bitmap.Dispose();
                }
                catch
                {
                    // Ignore.
                }
            }
        }

        public (Bitmap, Rectangle) GetTexture(string filename, TeamColor teamColor, bool generateFallback)
        {
            if (!cachedTextures.ContainsKey(filename) && generateFallback)
            {
                (Bitmap bm, Rectangle bounds) = GetDummyImage();
                if (bm != null)
                {
                    if (!cachedTextures.ContainsKey(filename))
                    {
                        cachedTextures.Add(filename, new Bitmap(bm));
                    }
                    return (new Bitmap(bm), bounds);
                }
            }
            if (teamColorTextures.TryGetValue((filename, teamColor), out (Bitmap bitmap, Rectangle opaqueBounds) result))
            {
                return (new Bitmap(result.bitmap), result.opaqueBounds);
            }
            if (!cachedTextures.TryGetValue(filename, out result.bitmap))
            {
                if (Path.GetExtension(filename).ToLower() == ".tga")
                {
                    TGA tga = null;
                    JObject metadata = null;
                    var name = Path.GetFileNameWithoutExtension(filename);
                    var archiveDir = Path.GetDirectoryName(filename);
                    var archivePath = archiveDir + ".ZIP";
                    // First attempt to find the texture in mod folders.
                    if (ExpandModPaths != null && ExpandModPaths.Length > 0)
                    {
                        for (int i = 0; i < ExpandModPaths.Length; ++i)
                        {
                            // First attempt to find the texture in an archive
                            var modArch = Path.Combine(ExpandModPaths[i], archivePath);
                            if (File.Exists(modArch))
                            {
                                using (FileStream fs = new FileStream(modArch, FileMode.Open))
                                {
                                    LoadTgaFromZipFileStream(fs, name, ref tga, ref metadata);
                                }
                            }
                            // Next attempt to load a standalone file
                            if (tga == null)
                            {
                                var modFile = Path.Combine(ExpandModPaths[i], filename);
                                if (File.Exists(modFile))
                                {
                                    using (var fileStream = new FileStream(modFile, FileMode.Open))
                                    {
                                        tga = new TGA(fileStream);
                                    }
                                }
                                if (tga != null)
                                {
                                    var modMeta = Path.ChangeExtension(modFile, ".meta");
                                    if (File.Exists(modMeta))
                                    {
                                        using (var metaStream = new FileStream(modMeta, FileMode.Open))
                                        using (var reader = new StreamReader(metaStream))
                                        {
                                            metadata = JObject.Parse(reader.ReadToEnd());
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // First attempt to find the texture in an archive
                    if (tga == null)
                    {
                        using (var fileStream = megafileManager.Open(archivePath))
                        {
                            LoadTgaFromZipFileStream(fileStream, name, ref tga, ref metadata);
                        }
                    }
                    // Next attempt to load a standalone file
                    if (tga == null)
                    {
                        using (var fileStream = megafileManager.Open(filename))
                        {
                            // megafileManager.Open might return null if not found, so always check on this.
                            // The Load???FromFileStream functions do this check internally.
                            if (fileStream != null)
                            {
                                tga = new TGA(fileStream);
                            }
                        }
                        if (tga != null)
                        {
                            var meta = Path.ChangeExtension(filename, ".meta");
                            using (var metaStream = megafileManager.Open(meta))
                            {
                                if (metaStream != null)
                                {
                                    using (var reader = new StreamReader(metaStream))
                                    {
                                        metadata = JObject.Parse(reader.ReadToEnd());
                                    }
                                }
                            }
                        }
                    }
                    if (tga != null)
                    {
                        var bitmap = tga.ToBitmap(true);
                        if (metadata != null)
                        {
                            var size = new Size(metadata["size"][0].ToObject<int>(), metadata["size"][1].ToObject<int>());
                            var crop = Rectangle.FromLTRB(
                                metadata["crop"][0].ToObject<int>(),
                                metadata["crop"][1].ToObject<int>(),
                                metadata["crop"][2].ToObject<int>(),
                                metadata["crop"][3].ToObject<int>()
                            );
                            var uncroppedBitmap = new Bitmap(size.Width, size.Height, bitmap.PixelFormat);
                            using (var g = Graphics.FromImage(uncroppedBitmap))
                            {
                                g.DrawImage(bitmap, crop, new Rectangle(Point.Empty, bitmap.Size), GraphicsUnit.Pixel);
                            }
                            cachedTextures[filename] = uncroppedBitmap;
                        }
                        else
                        {
                            cachedTextures[filename] = bitmap;
                        }
                    }
#if false
                    // Attempt to load parent directory as archive
                    var archiveDir = Path.GetDirectoryName(filename);
                    var archivePath = archiveDir + ".ZIP";
                    using (var fileStream = megafileManager.Open(archivePath))
                    {
                        if (fileStream != null)
                        {
                            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
                            {
                                var images = new Dictionary<string, ImageData>();
                                foreach (var entry in archive.Entries)
                                {
                                    var name = Path.GetFileNameWithoutExtension(entry.Name);
                                    if (!images.TryGetValue(name, out ImageData imageData))
                                    {
                                        imageData = images[name] = new ImageData { TGA = null, Metadata = null };
                                    }
                                    if ((imageData.TGA == null) && (Path.GetExtension(entry.Name).ToLower() == ".tga"))
                                    {
                                        using (var stream = entry.Open())
                                        using (var memStream = new MemoryStream())
                                        {
                                            stream.CopyTo(memStream);
                                            imageData.TGA = new TGA(memStream);
                                        }
                                    }
                                    else if ((imageData.Metadata == null) && (Path.GetExtension(entry.Name).ToLower() == ".meta"))
                                    {
                                        using (var stream = entry.Open())
                                        using (var reader = new StreamReader(stream))
                                        {
                                            imageData.Metadata = JObject.Parse(reader.ReadToEnd());
                                        }
                                    }
                                    if ((imageData.TGA != null) && (imageData.Metadata != null))
                                    {
                                        var bitmap = imageData.TGA.ToBitmap(true);
                                        var size = new Size(imageData.Metadata["size"][0].ToObject<int>(), imageData.Metadata["size"][1].ToObject<int>());
                                        var crop = Rectangle.FromLTRB(
                                            imageData.Metadata["crop"][0].ToObject<int>(),
                                            imageData.Metadata["crop"][1].ToObject<int>(),
                                            imageData.Metadata["crop"][2].ToObject<int>(),
                                            imageData.Metadata["crop"][3].ToObject<int>()
                                        );
                                        var uncroppedBitmap = new Bitmap(size.Width, size.Height, bitmap.PixelFormat);
                                        using (var g = Graphics.FromImage(uncroppedBitmap))
                                        {
                                            g.DrawImage(bitmap, crop, new Rectangle(Point.Empty, bitmap.Size), GraphicsUnit.Pixel);
                                        }
                                        cachedTextures[Path.Combine(archiveDir, name) + ".tga"] = uncroppedBitmap;
                                        images.Remove(name);
                                    }
                                }
                                foreach (var item in images.Where(x => x.Value.TGA != null))
                                {
                                    cachedTextures[Path.Combine(archiveDir, item.Key) + ".tga"] = item.Value.TGA.ToBitmap(true);
                                }
                            }
                        }
                    }
#endif
                }
                if (!cachedTextures.TryGetValue(filename, out result.bitmap))
                {
                    // Try loading as a DDS
                    var ddsFilename = Path.ChangeExtension(filename, ".DDS");
                    Bitmap bitmap = null;
                    if (ExpandModPaths != null && ExpandModPaths.Length > 0)
                    {
                        for (int i = 0; i < ExpandModPaths.Length; ++i)
                        {
                            var modFile = Path.Combine(ExpandModPaths[i], ddsFilename);
                            if (File.Exists(modFile))
                            {
                                using (FileStream fs = new FileStream(modFile, FileMode.Open))
                                {
                                    bitmap = LoadDDSFromFileStream(fs);
                                }
                            }
                        }
                    }
                    if (bitmap == null)
                    {
                        using (var fileStream = megafileManager.Open(ddsFilename))
                        {
                            bitmap = LoadDDSFromFileStream(fileStream);
                        }
                    }
                    if (bitmap != null)
                    {
                        cachedTextures[filename] = bitmap;
                    }
                }
            }
            if (!cachedTextures.TryGetValue(filename, out result.bitmap))
            {
                return result;
            }
            result.bitmap = new Bitmap(result.bitmap);
            if (teamColor != null)
            {
                float frac(float x) => x - (int)x;
                float lerp(float x, float y, float t) => (x * (1.0f - t)) + (y * t);
                float saturate(float x) => Math.Max(0.0f, Math.Min(1.0f, x));
                BitmapData data = null;
                try
                {
                    data = result.bitmap.LockBits(new Rectangle(0, 0, result.bitmap.Width, result.bitmap.Height), ImageLockMode.ReadWrite, result.bitmap.PixelFormat);
                    var bpp = Image.GetPixelFormatSize(data.PixelFormat) / 8;
                    var bytes = new byte[data.Stride * data.Height];
                    Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                    int lineWidth = ((data.Width * Image.GetPixelFormatSize(data.PixelFormat)) + 7) / 8;
                    int height = data.Height;
                    int stride = data.Stride;
                    result.opaqueBounds = CalculateOpaqueBounds(bytes, data.Width, data.Height, bpp, data.Stride);
                    int lineStart = 0;
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < lineWidth; x += bpp)
                        {
                            int addr = lineStart + x;
                            var pixel = Color.FromArgb(bytes[addr + 2], bytes[addr + 1], bytes[addr + 0]);
                            (float r, float g, float b) = (pixel.R.ToLinear(), pixel.G.ToLinear(), pixel.B.ToLinear());
                            (float x, float y, float z, float w) K = (0.0f, -1.0f / 3.0f, 2.0f / 3.0f, -1.0f);
                            (float x, float y, float z, float w) p = (g >= b) ? (g, b, K.x, K.y) : (b, g, K.w, K.z);
                            (float x, float y, float z, float w) q = (r >= p.x) ? (r, p.y, p.z, p.x) : (p.x, p.y, p.w, r);
                            (float d, float e) = (q.x - Math.Min(q.w, q.y), 1e-10f);
                            (float hue, float saturation, float value) = (Math.Abs(q.z + (q.w - q.y) / (6.0f * d + e)), d / (q.x + e), q.x);
                            var lowerHue = teamColor.LowerBounds.GetHue() / 360.0f;
                            var upperHue = teamColor.UpperBounds.GetHue() / 360.0f;
                            if ((hue >= lowerHue) && (upperHue >= hue))
                            {
                                hue = (hue / (upperHue - lowerHue)) * ((upperHue + teamColor.Fudge) - (lowerHue - teamColor.Fudge));
                                hue += teamColor.HSVShift.X;
                                saturation += teamColor.HSVShift.Y;
                                value += teamColor.HSVShift.Z;
                                (float x, float y, float z, float w) L = (1.0f, 2.0f / 3.0f, 1.0f / 3.0f, 3.0f);
                                (float x, float y, float z) m = (
                                    Math.Abs(frac(hue + L.x) * 6.0f - L.w),
                                    Math.Abs(frac(hue + L.y) * 6.0f - L.w),
                                    Math.Abs(frac(hue + L.z) * 6.0f - L.w)
                                );
                                r = value * lerp(L.x, saturate(m.x - L.x), saturation);
                                g = value * lerp(L.x, saturate(m.y - L.x), saturation);
                                b = value * lerp(L.x, saturate(m.z - L.x), saturation);
                                (float x, float y, float z) n = (
                                    Math.Min(1.0f, Math.Max(0.0f, r - teamColor.InputLevels.X) / (teamColor.InputLevels.Z - teamColor.InputLevels.X)),
                                    Math.Min(1.0f, Math.Max(0.0f, g - teamColor.InputLevels.X) / (teamColor.InputLevels.Z - teamColor.InputLevels.X)),
                                    Math.Min(1.0f, Math.Max(0.0f, b - teamColor.InputLevels.X) / (teamColor.InputLevels.Z - teamColor.InputLevels.X))
                                );
                                n.x = (float)Math.Pow(n.x, teamColor.InputLevels.Y);
                                n.y = (float)Math.Pow(n.y, teamColor.InputLevels.Y);
                                n.z = (float)Math.Pow(n.z, teamColor.InputLevels.Y);
                                r = lerp(teamColor.OutputLevels.X, teamColor.OutputLevels.Y, n.x);
                                g = lerp(teamColor.OutputLevels.X, teamColor.OutputLevels.Y, n.y);
                                b = lerp(teamColor.OutputLevels.X, teamColor.OutputLevels.Y, n.z);
                            }
                            (float x, float y, float z) n2 = (
                                Math.Min(1.0f, Math.Max(0.0f, r - teamColor.OverallInputLevels.X) / (teamColor.OverallInputLevels.Z - teamColor.OverallInputLevels.X)),
                                Math.Min(1.0f, Math.Max(0.0f, g - teamColor.OverallInputLevels.X) / (teamColor.OverallInputLevels.Z - teamColor.OverallInputLevels.X)),
                                Math.Min(1.0f, Math.Max(0.0f, b - teamColor.OverallInputLevels.X) / (teamColor.OverallInputLevels.Z - teamColor.OverallInputLevels.X))
                            );
                            n2.x = (float)Math.Pow(n2.x, teamColor.OverallInputLevels.Y);
                            n2.y = (float)Math.Pow(n2.y, teamColor.OverallInputLevels.Y);
                            n2.z = (float)Math.Pow(n2.z, teamColor.OverallInputLevels.Y);
                            r = lerp(teamColor.OverallOutputLevels.X, teamColor.OverallOutputLevels.Y, n2.x);
                            g = lerp(teamColor.OverallOutputLevels.X, teamColor.OverallOutputLevels.Y, n2.y);
                            b = lerp(teamColor.OverallOutputLevels.X, teamColor.OverallOutputLevels.Y, n2.z);
                            bytes[addr + 2] = (byte)(r.ToSRGB() * 255.0f);
                            bytes[addr + 1] = (byte)(g.ToSRGB() * 255.0f);
                            bytes[addr + 0] = (byte)(b.ToSRGB() * 255.0f);
                        }
                        lineStart += stride;
                    }
                    Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
                }
                finally
                {
                    if (data != null)
                    {
                        result.bitmap.UnlockBits(data);
                    }
                }
                // EXPERIMENTAL: might be better not to cache this?
                //teamColorTextures[(filename, teamColor)] = (new Bitmap(result.bitmap), result.opaqueBounds);
            }
            else
            {
                result.opaqueBounds = CalculateOpaqueBounds(result.bitmap);
            }
            return result;
        }

        private void LoadTgaFromZipFileStream(Stream fileStream, String name, ref TGA tga, ref JObject metadata)
        {
            if (fileStream == null)
            {
                return;
            }
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    if (name != Path.GetFileNameWithoutExtension(entry.Name))
                    {
                        continue;
                    }
                    if (tga == null && ".tga".Equals(Path.GetExtension(entry.Name), StringComparison.InvariantCultureIgnoreCase))
                    {
                        using (var stream = entry.Open())
                        using (var memStream = new MemoryStream())
                        {
                            stream.CopyTo(memStream);
                            tga = new TGA(memStream);
                        }
                    }
                    else if (metadata == null && ".meta".Equals(Path.GetExtension(entry.Name), StringComparison.InvariantCultureIgnoreCase))
                    {
                        using (var stream = entry.Open())
                        using (var reader = new StreamReader(stream))
                        {
                            metadata = JObject.Parse(reader.ReadToEnd());
                        }
                    }
                    if (tga != null && metadata != null)
                    {
                        break;
                    }
                }
            }
        }

        private Bitmap LoadDDSFromFileStream(Stream fileStream)
        {
            if (fileStream == null)
            {
                return null;
            }
            var bytes = new byte[fileStream.Length];
            fileStream.Read(bytes, 0, bytes.Length);
            using (var image = Dds.Create(bytes, new PfimConfig()))
            {
                PixelFormat format;
                switch (image.Format)
                {
                    case Pfim.ImageFormat.Rgb24:
                        format = PixelFormat.Format24bppRgb;
                        break;
                    case Pfim.ImageFormat.Rgba32:
                        format = PixelFormat.Format32bppArgb;
                        break;
                    case Pfim.ImageFormat.R5g5b5:
                        format = PixelFormat.Format16bppRgb555;
                        break;
                    case Pfim.ImageFormat.R5g6b5:
                        format = PixelFormat.Format16bppRgb565;
                        break;
                    case Pfim.ImageFormat.R5g5b5a1:
                        format = PixelFormat.Format16bppArgb1555;
                        break;
                    case Pfim.ImageFormat.Rgb8:
                        format = PixelFormat.Format8bppIndexed;
                        break;
                    default:
                        format = PixelFormat.DontCare;
                        break;
                }
                var bitmap = new Bitmap(image.Width, image.Height, format);
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
                Marshal.Copy(image.Data, 0, bitmapData.Scan0, image.Stride * image.Height);
                bitmap.UnlockBits(bitmapData);
                return bitmap;
            }            
        }

        private (Bitmap, Rectangle) GetDummyImage()
        {
            (Bitmap bm, _) = GetTexture(MissingTexture, null, false);
            Rectangle r = new Rectangle(0, 0, Globals.OriginalTileWidth, Globals.OriginalTileHeight);
            if (!processedMissingTexture || bm == null)
            {
                if (bm == null)
                {
                    // Generate.
                    bm = new Bitmap(48, 48);
                    using (Graphics graphics = Graphics.FromImage(bm))
                    {
                        using (SolidBrush outside = new SolidBrush(Color.FromArgb(128, 107, 107, 107)))
                        using (SolidBrush ring = new SolidBrush(Color.FromArgb(128, 0, 0, 0)))
                        using (SolidBrush center = new SolidBrush(Color.FromArgb(128, 250, 250, 250)))
                        {
                            graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                            graphics.FillRectangle(outside, new Rectangle(0, 0, 48, 48));
                            graphics.FillRectangle(ring, new Rectangle(5, 5, 38, 38));
                            graphics.FillRectangle(center, new Rectangle(7, 7, 34, 34));
                        }
                    }
                }
                // Post-process dummy image.
                Bitmap newBm = new Bitmap(Globals.OriginalTileWidth, Globals.OriginalTileHeight);
                ColorMatrix colorMatrix = new ColorMatrix();
                colorMatrix.Matrix33 = 0.5f;
                var imageAttributes = new ImageAttributes();
                imageAttributes.SetColorMatrix(
                    colorMatrix,
                    ColorMatrixFlag.Default,
                    ColorAdjustType.Bitmap);
                using (Graphics g = Graphics.FromImage(newBm))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.DrawImage(bm, r, 0, 0, bm.Width, bm.Height, GraphicsUnit.Pixel, imageAttributes);
                }
                Bitmap oldBm = bm;
                bm = newBm;
                try
                {
                    oldBm.Dispose();
                }
                catch
                {
                    // Ignore.
                }
                cachedTextures[MissingTexture] = bm;
                processedMissingTexture = true;
            }
            return (bm, r);
        }

        private static Rectangle CalculateOpaqueBounds(byte[] data, int width, int height, int bytespp, int stride)
        {
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
            for (int y = 0; y < height; ++y)
            {
                if (!isTransparentRow(y))
                {
                    opaqueBounds.Offset(0, y);
                    break;
                }
            }
            for (int y = height; y > 0; --y)
            {
                if (!isTransparentRow(y - 1))
                {
                    opaqueBounds.Height = y - opaqueBounds.Top;
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
            for (int x = 0; x < width; ++x)
            {
                if (!isTransparentColumn(x))
                {
                    opaqueBounds.Offset(x, 0);
                    break;
                }
            }
            for (int x = width; x > 0; --x)
            {
                if (!isTransparentColumn(x - 1))
                {
                    opaqueBounds.Width = x - opaqueBounds.Left;
                    break;
                }
            }
            return opaqueBounds;
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
                return CalculateOpaqueBounds(bytes, data.Width, data.Height, bpp, data.Stride);
            }
            finally
            {
                if (data != null)
                {
                    bitmap.UnlockBits(data);
                }
            }
        }
    }
}
