using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace MobiusEditor.Utility
{
    /// <summary>
    /// Simple class for caching objects and region data generated for painting on the UI.
    /// </summary>
    public class ShapeCacheManager: IDisposable
    {
        Dictionary<string, RegionData> shapes = new Dictionary<string, RegionData>();
        Dictionary<string, Bitmap> images = new Dictionary<string, Bitmap>();

        /// <summary>
        /// Resets the ShapeManager instance to prepare it for a new game. This clears all cached objects.
        /// </summary>
        public void Reset()
        {
            shapes.Clear();
            List<Bitmap> bitmaps = images.Values.ToList();
            images.Clear();
            foreach (Bitmap bitmap in bitmaps)
            {
                try
                {
                    bitmap.Dispose();
                }
                catch { /* ignore */ }
            }
        }
        public void AddShape(string identifier, RegionData shape)
        {
            if (!shapes.ContainsKey(identifier))
            {
                shapes.Add(identifier, shape);
            }
        }

        public RegionData GetShape(string identifier)
        {
            if (shapes.ContainsKey(identifier))
            {
                return shapes[identifier];
            }
            return null;
        }

        public void AddImage(string identifier, Bitmap image)
        {
            if (!images.ContainsKey(identifier))
            {
                images.Add(identifier, image);
            }
        }

        public Bitmap GetImage(string identifier)
        {
            if (images.ContainsKey(identifier))
            {
                return images[identifier];
            }
            return null;
        }

        public void Dispose()
        {
            this.Reset();
        }
    }
}
