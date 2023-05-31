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
using MobiusEditor.Interface;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Xml;

namespace MobiusEditor.Utility
{
    public class TeamColor : ITeamColor
    {
        private readonly TeamColorManager teamColorManager;

        public string Variant { get; private set; }

        public string Name { get; private set; }

        private Color? lowerBounds;
        public Color LowerBounds => lowerBounds.HasValue ? lowerBounds.Value : ((Variant != null) ? teamColorManager.GetItem(Variant).LowerBounds : default);

        private Color? upperBounds;
        public Color UpperBounds => upperBounds.HasValue ? upperBounds.Value : ((Variant != null) ? teamColorManager.GetItem(Variant).UpperBounds : default);

        private float? fudge;
        public float Fudge => fudge.HasValue ? fudge.Value : ((Variant != null) ? teamColorManager.GetItem(Variant).Fudge : default);

        private Vector3? hsvShift;
        public Vector3 HSVShift => hsvShift.HasValue ? hsvShift.Value : ((Variant != null) ? teamColorManager.GetItem(Variant).HSVShift : default);

        private Vector3? inputLevels;
        public Vector3 InputLevels => inputLevels.HasValue ? inputLevels.Value : ((Variant != null) ? teamColorManager.GetItem(Variant).InputLevels : default);

        private Vector2? outputLevels;
        public Vector2 OutputLevels => outputLevels.HasValue ? outputLevels.Value : ((Variant != null) ? teamColorManager.GetItem(Variant).OutputLevels : default);

        private Vector3? overallInputLevels;
        public Vector3 OverallInputLevels => overallInputLevels.HasValue ? overallInputLevels.Value : ((Variant != null) ? teamColorManager.GetItem(Variant).OverallInputLevels : default);

        private Vector2? overallOutputLevels;
        public Vector2 OverallOutputLevels => overallOutputLevels.HasValue ? overallOutputLevels.Value : ((Variant != null) ? teamColorManager.GetItem(Variant).OverallOutputLevels : default);

        private Color? radarMapColor;
        public Color RadarMapColor => radarMapColor.HasValue ? radarMapColor.Value : ((Variant != null) ? teamColorManager.GetItem(Variant).RadarMapColor : default);

        private Color? baseColor;
        public Color BaseColor
        {
            get
            {
                if (!baseColor.HasValue)
                    baseColor = this.GetTeamColor();
                return baseColor.Value;
            }
        }

        public TeamColor(TeamColorManager teamColorManager)
        {
            this.teamColorManager = teamColorManager;
        }

        public TeamColor(TeamColorManager teamColorManager, string newName, string variant, Vector3 hsvShiftOverride)
        {
            this.teamColorManager = teamColorManager;
            this.Variant = variant;
            this.Flatten();
            this.Name = newName;
            this.hsvShift = hsvShiftOverride;
        }

        public TeamColor(TeamColorManager teamColorManager, TeamColor col, string newName, Vector3 hsvShiftOverride)
        {
            this.teamColorManager = teamColorManager;
            this.Load(col, newName);
            this.hsvShift = hsvShiftOverride;
        }

        public void Load(TeamColor col, string newName)
        {
            this.Name = newName ?? col.Name;
            this.Variant = col.Variant;
            this.lowerBounds = col.LowerBounds;
            this.upperBounds = col.UpperBounds;
            this.fudge = col.Fudge;
            this.hsvShift = col.HSVShift;
            this.inputLevels = col.InputLevels;
            this.outputLevels = col.OutputLevels;
            this.overallInputLevels = col.OverallInputLevels;
            this.overallOutputLevels = col.OverallOutputLevels;
            this.radarMapColor = col.RadarMapColor;
        }

        public void Load(string name, string variant, Color? lowerBounds, Color? upperBounds, float? fudge, Vector3? hsvShift, Vector3? inputLevels, Vector2? outputLevels, Vector3? overallInputLevels, Vector2? overallOutputLevels, Color? radarMapColor)
        {
            this.Name = name;
            this.Variant = variant;
            this.lowerBounds = lowerBounds;
            this.upperBounds = upperBounds;
            this.fudge = fudge;
            this.hsvShift = hsvShift;
            this.inputLevels = inputLevels;
            this.outputLevels = outputLevels;
            this.overallInputLevels = overallInputLevels;
            this.overallOutputLevels = overallOutputLevels;
            this.radarMapColor = radarMapColor;
        }

        public void Load(string xml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            var node = xmlDoc.FirstChild;
            this.Name = node.Attributes["Name"].Value;
            this.Variant = node.Attributes["Variant"]?.Value;

            var lowerBoundsNode = node.SelectSingleNode("LowerBounds");
            if (lowerBoundsNode != null)
            {
                lowerBounds = Color.FromArgb(
                    (int)(float.Parse(lowerBoundsNode.SelectSingleNode("R").InnerText) * 255),
                    (int)(float.Parse(lowerBoundsNode.SelectSingleNode("G").InnerText) * 255),
                    (int)(float.Parse(lowerBoundsNode.SelectSingleNode("B").InnerText) * 255)
                );
            }

            var upperBoundsNode = node.SelectSingleNode("UpperBounds");
            if (upperBoundsNode != null)
            {
                this.upperBounds = Color.FromArgb(
                    (int)(float.Parse(upperBoundsNode.SelectSingleNode("R").InnerText) * 255),
                    (int)(float.Parse(upperBoundsNode.SelectSingleNode("G").InnerText) * 255),
                    (int)(float.Parse(upperBoundsNode.SelectSingleNode("B").InnerText) * 255)
                );
            }

            var fudgeNode = node.SelectSingleNode("Fudge");
            if (fudgeNode != null)
            {
                this.fudge = float.Parse(fudgeNode.InnerText);
            }

            var hsvShiftNode = node.SelectSingleNode("HSVShift");
            if (hsvShiftNode != null)
            {
                this.hsvShift = new Vector3(
                    float.Parse(hsvShiftNode.SelectSingleNode("X").InnerText),
                    float.Parse(hsvShiftNode.SelectSingleNode("Y").InnerText),
                    float.Parse(hsvShiftNode.SelectSingleNode("Z").InnerText)
                );
            }

            var inputLevelsNode = node.SelectSingleNode("InputLevels");
            if (inputLevelsNode != null)
            {
                this.inputLevels = new Vector3(
                    float.Parse(inputLevelsNode.SelectSingleNode("X").InnerText),
                    float.Parse(inputLevelsNode.SelectSingleNode("Y").InnerText),
                    float.Parse(inputLevelsNode.SelectSingleNode("Z").InnerText)
                );
            }

            var outputLevelsNode = node.SelectSingleNode("OutputLevels");
            if (outputLevelsNode != null)
            {
                this.outputLevels = new Vector2(
                    float.Parse(outputLevelsNode.SelectSingleNode("X").InnerText),
                    float.Parse(outputLevelsNode.SelectSingleNode("Y").InnerText)
                );
            }

            var overallInputLevelsNode = node.SelectSingleNode("OverallInputLevels");
            if (overallInputLevelsNode != null)
            {
                this.overallInputLevels = new Vector3(
                    float.Parse(overallInputLevelsNode.SelectSingleNode("X").InnerText),
                    float.Parse(overallInputLevelsNode.SelectSingleNode("Y").InnerText),
                    float.Parse(overallInputLevelsNode.SelectSingleNode("Z").InnerText)
                );
            }

            var overallOutputLevelsNode = node.SelectSingleNode("OverallOutputLevels");
            if (outputLevelsNode != null)
            {
                this.overallOutputLevels = new Vector2(
                    float.Parse(overallOutputLevelsNode.SelectSingleNode("X").InnerText),
                    float.Parse(overallOutputLevelsNode.SelectSingleNode("Y").InnerText)
                );
            }

            var radarMapColorNode = node.SelectSingleNode("RadarMapColor");
            if (radarMapColorNode != null)
            {
                this.radarMapColor = Color.FromArgb(
                    (int)(float.Parse(radarMapColorNode.SelectSingleNode("R").InnerText) * 255),
                    (int)(float.Parse(radarMapColorNode.SelectSingleNode("G").InnerText) * 255),
                    (int)(float.Parse(radarMapColorNode.SelectSingleNode("B").InnerText) * 255)
                );
            }
        }

        public void Flatten()
        {
            this.lowerBounds = this.LowerBounds;
            this.upperBounds = this.UpperBounds;
            this.fudge = this.Fudge;
            this.hsvShift = this.HSVShift;
            this.inputLevels = this.InputLevels;
            this.outputLevels = this.OutputLevels;
            this.overallInputLevels = this.OverallInputLevels;
            this.overallOutputLevels = this.OverallOutputLevels;
            this.radarMapColor = this.RadarMapColor;
        }

        public Color GetTeamColor()
        {
            // Makes a 1x1 green pixel image, and applies the recolor operation to it.
            using (Bitmap bitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb))
            {
                bitmap.SetResolution(96, 96);
                bitmap.SetPixel(0, 0, this.teamColorManager.RemapBaseColor);
                this.ApplyToImage(bitmap);
                return bitmap.GetPixel(0, 0);
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
                opaqueBounds = ImageUtils.CalculateOpaqueBoundsHiCol(bytes, width, height, bytesPerPixel, stride);
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
            // Only handle 24bpp and 32bpp data.
            if (bytesPerPixel != 3 && bytesPerPixel != 4)
            {
                return;
            }
            Rectangle bounds = opaqueBounds ?? new Rectangle(0, 0, width, height);
            float frac(float x) => x - (int)x;
            float lerp(float x, float y, float t) => (x * (1.0f - t)) + (y * t);
            float saturate(float x) => Math.Max(0.0f, Math.Min(1.0f, x));
            // Precalculate some stuff.
            var lowerHue = this.LowerBounds.GetHue() / 360.0f;
            var upperHue = this.UpperBounds.GetHue() / 360.0f;
            var lowerHueFudge = lowerHue - this.Fudge;
            var upperHueFudge = upperHue + this.Fudge;
            var hueError = (upperHueFudge - lowerHueFudge) / (upperHue - lowerHue);
            var hueShift = this.HSVShift.X;
            var satShift = this.HSVShift.Y;
            var valShift = this.HSVShift.Z;
            // Optimisation: since we got the opaque bounds calculated anyway, might as well use them and only process what's inside.
            int lineStart = bounds.Top * stride;
            for (int y = bounds.Top; y < bounds.Bottom; y++)
            {
                int addr = lineStart + bounds.Left * bytesPerPixel;
                for (int x = bounds.Left; x < bounds.Right; ++x)
                {
                    // 4-byte pixel = [B,G,R,A]. This code remains the same if it's 24 bpp since the alpha byte is never processed.
                    var pixel = Color.FromArgb(bytes[addr + 2], bytes[addr + 1], bytes[addr + 0]);
                    (float r, float g, float b) = (pixel.R.ToLinear(), pixel.G.ToLinear(), pixel.B.ToLinear());
                    (float x, float y, float z, float w) K = (0.0f, -1.0f / 3.0f, 2.0f / 3.0f, -1.0f);
                    (float x, float y, float z, float w) p = (g >= b) ? (g, b, K.x, K.y) : (b, g, K.w, K.z);
                    (float x, float y, float z, float w) q = (r >= p.x) ? (r, p.y, p.z, p.x) : (p.x, p.y, p.w, r);
                    (float d, float e) = (q.x - Math.Min(q.w, q.y), 1e-10f);
                    (float hue, float saturation, float value) = (Math.Abs(q.z + (q.w - q.y) / (6.0f * d + e)), d / (q.x + e), q.x);
                    // Processing the pixels that fall in the hue range to change
                    if ((hue >= lowerHue) && (hue <= upperHue))
                    {
                        hue = (hue * hueError) + hueShift;
                        saturation += satShift;
                        value += valShift;
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
                            Math.Min(1.0f, Math.Max(0.0f, r - this.InputLevels.X) / (this.InputLevels.Z - this.InputLevels.X)),
                            Math.Min(1.0f, Math.Max(0.0f, g - this.InputLevels.X) / (this.InputLevels.Z - this.InputLevels.X)),
                            Math.Min(1.0f, Math.Max(0.0f, b - this.InputLevels.X) / (this.InputLevels.Z - this.InputLevels.X))
                        );
                        n.x = (float)Math.Pow(n.x, this.InputLevels.Y);
                        n.y = (float)Math.Pow(n.y, this.InputLevels.Y);
                        n.z = (float)Math.Pow(n.z, this.InputLevels.Y);
                        r = lerp(this.OutputLevels.X, this.OutputLevels.Y, n.x);
                        g = lerp(this.OutputLevels.X, this.OutputLevels.Y, n.y);
                        b = lerp(this.OutputLevels.X, this.OutputLevels.Y, n.z);
                    }
                    // post-processing the overall levels
                    (float x, float y, float z) n2 = (
                        Math.Min(1.0f, Math.Max(0.0f, r - this.OverallInputLevels.X) / (this.OverallInputLevels.Z - this.OverallInputLevels.X)),
                        Math.Min(1.0f, Math.Max(0.0f, g - this.OverallInputLevels.X) / (this.OverallInputLevels.Z - this.OverallInputLevels.X)),
                        Math.Min(1.0f, Math.Max(0.0f, b - this.OverallInputLevels.X) / (this.OverallInputLevels.Z - this.OverallInputLevels.X))
                    );
                    n2.x = (float)Math.Pow(n2.x, this.OverallInputLevels.Y);
                    n2.y = (float)Math.Pow(n2.y, this.OverallInputLevels.Y);
                    n2.z = (float)Math.Pow(n2.z, this.OverallInputLevels.Y);
                    r = lerp(this.OverallOutputLevels.X, this.OverallOutputLevels.Y, n2.x);
                    g = lerp(this.OverallOutputLevels.X, this.OverallOutputLevels.Y, n2.y);
                    b = lerp(this.OverallOutputLevels.X, this.OverallOutputLevels.Y, n2.z);
                    bytes[addr + 2] = (byte)(r.ToSRGB() * 255.0f);
                    bytes[addr + 1] = (byte)(g.ToSRGB() * 255.0f);
                    bytes[addr + 0] = (byte)(b.ToSRGB() * 255.0f);
                    // go to next pixel
                    addr += bytesPerPixel;
                }
                lineStart += stride;
            }
        }
    }
}
