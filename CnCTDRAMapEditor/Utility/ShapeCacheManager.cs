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

        public void RemoveAllShapesFor(string identifierStart)
        {
            List<string> toRemove = new List<string>(shapes.Keys.Where(k => k.StartsWith(identifierStart, StringComparison.OrdinalIgnoreCase)));
            foreach (string identifier in toRemove)
            {
                shapes.Remove(identifier);
            }
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

        public void RemoveAllImagesFor(string identifierStart)
        {
            List<string> toRemove = new List<string>(images.Keys.Where(k => k.StartsWith(identifierStart, StringComparison.OrdinalIgnoreCase)));
            foreach (string identifier in toRemove)
            {
                Bitmap bitmap = images[identifier];
                shapes.Remove(identifier);
                try
                {
                    bitmap.Dispose();
                }
                catch { /* ignore */ }
            }
        }

        public void Dispose()
        {
            this.Reset();
        }
    }
}
