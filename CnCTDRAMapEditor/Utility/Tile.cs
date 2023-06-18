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
using System.Drawing;

namespace MobiusEditor.Utility
{
    /// <summary>
    /// Split off from Tileset.cs, since Tileset class is only used internally
    /// in TilesetManager, while this one is publicly used throughout the project.
    /// </summary>
    public class Tile : IDisposable
    {
        public Bitmap Image { get; private set; }

        public Rectangle OpaqueBounds { get; private set; }

        public Tile(Bitmap image, Rectangle opaqueBounds)
        {
            Image = image;
            OpaqueBounds = opaqueBounds;
        }

        public Tile(Bitmap image)
            : this(image, new Rectangle(0, 0, image.Width, image.Height))
        {
        }

        public void Dispose()
        {
            Bitmap image = this.Image;
            this.Image = null;
            try
            {
                if (image != null)
                {
                    image.Dispose();
                }
            }
            catch
            {
                // ignore.
            }

        }
    }
}
