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
using MobiusEditor.Interface;

namespace MobiusEditor.Utility
{
    public class TextureManager
    {

        private static string MissingTexture = "DATA\\ART\\TEXTURES\\SRGB\\COMMON\\MISC\\MISSING.TGA";
        private bool processedMissingTexture = false;

#if false
        private class ImageData
        {
            public TGA TGA;
            public JObject Metadata;
        }
#endif

        private readonly IArchiveManager megafileManager;

        private Dictionary<string, Bitmap> cachedTextures = new Dictionary<string, Bitmap>();
        private Dictionary<(string, ITeamColor), (Bitmap, Rectangle)> teamColorTextures = new Dictionary<(string, ITeamColor), (Bitmap, Rectangle)>();

        public TextureManager(IArchiveManager megafileManager)
        {
            this.megafileManager = megafileManager;
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

        public (Bitmap, Rectangle) GetTexture(string filename, ITeamColor teamColor, bool generateFallback)
        {
            if (!cachedTextures.ContainsKey(filename) && generateFallback)
            {
                (Bitmap bm, Rectangle bounds) = GetDummyImage();
                if (bm != null)
                {
                    if (!cachedTextures.ContainsKey(filename))
                    {
                        Bitmap cacheCopy = new Bitmap(bm);
                        cacheCopy.SetResolution(96, 96);
                        cachedTextures.Add(filename, cacheCopy);
                    }
                    Bitmap retCopy = new Bitmap(bm);
                    retCopy.SetResolution(96, 96);
                    return (retCopy, bounds);
                }
            }
            if (teamColorTextures.TryGetValue((filename, teamColor), out (Bitmap bitmap, Rectangle opaqueBounds) result))
            {
                Bitmap retCopy = new Bitmap(result.bitmap);
                retCopy.SetResolution(96, 96);
                return (retCopy, result.opaqueBounds);
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
                    // First attempt to find the texture in an archive
                    if (tga == null)
                    {
                        using (Stream fileStream = megafileManager.OpenFile(archivePath))
                        {
                            LoadTgaFromZipFileStream(fileStream, name, ref tga, ref metadata);
                        }
                    }
                    // Next attempt to load a standalone file
                    if (tga == null)
                    {
                        using (Stream fileStream = megafileManager.OpenFile(filename))
                        {
                            // megafileManager.OpenFile might return null if not found, so always check on this.
                            // The Load???FromFileStream functions do this check internally.
                            if (fileStream != null)
                            {
                                tga = new TGA(fileStream);
                            }
                        }
                        if (tga != null)
                        {
                            var meta = Path.ChangeExtension(filename, ".meta");
                            using (Stream metaStream = megafileManager.OpenFile(meta))
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
                        if (metadata != null)
                        {
                            using (var bitmap = tga.ToBitmap(true))
                            {
                                bitmap.SetResolution(96, 96);
                                var size = new Size(metadata["size"][0].ToObject<int>(), metadata["size"][1].ToObject<int>());
                                var crop = Rectangle.FromLTRB(
                                    metadata["crop"][0].ToObject<int>(),
                                    metadata["crop"][1].ToObject<int>(),
                                    metadata["crop"][2].ToObject<int>(),
                                    metadata["crop"][3].ToObject<int>()
                                );
                                var uncroppedBitmap = new Bitmap(size.Width, size.Height, bitmap.PixelFormat);
                                uncroppedBitmap.SetResolution(96, 96);
                                using (var g = Graphics.FromImage(uncroppedBitmap))
                                {
                                    g.DrawImage(bitmap, crop, new Rectangle(Point.Empty, bitmap.Size), GraphicsUnit.Pixel);
                                }
                                cachedTextures[filename] = uncroppedBitmap;
                            }
                        }
                        else
                        {
                            var bitmap = tga.ToBitmap(true);
                            bitmap.SetResolution(96, 96);
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
                                        uncroppedBitmap.SetResolution(96, 96);
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
                    using (Stream fileStream = megafileManager.OpenFile(ddsFilename))
                    {
                        bitmap = LoadDDSFromFileStream(fileStream);
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
            Bitmap resBm = new Bitmap(result.bitmap);
            resBm.SetResolution(96, 96);
            result.bitmap = resBm;
            if (teamColor != null)
            {
                Rectangle opaqueBounds;
                teamColor.ApplyToImage(resBm, out opaqueBounds);
                result.opaqueBounds = opaqueBounds;
                // EXPERIMENTAL: might be better not to cache this?
                //teamColorTextures[(filename, teamColor)] = (new Bitmap(result.bitmap), result.opaqueBounds);
            }
            else
            {
                result.opaqueBounds = ImageUtils.CalculateOpaqueBounds(resBm);
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
                // For some reason, accessing Entries (which triggers the initial read) throws
                // ungodly amounts of internal ArgumentException that really slow down debugging.
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
                bitmap.SetResolution(96, 96);
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
                    bm.SetResolution(96, 96);
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
                newBm.SetResolution(96, 96);
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
    }
}
