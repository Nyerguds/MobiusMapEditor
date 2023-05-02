//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free 
// software: you can redistribute it and/or modify it under the terms of 
// the GNU General Public License as published by the Free Software Foundation, 
// either version 3 of the License, or (at your option) any later version.

using System.Drawing;

namespace MobiusEditor.Interface
{
    public interface ITeamColor
    {
        string Name { get; }
        Color BaseColor { get; }
        void ApplyToImage(Bitmap image);
        void ApplyToImage(Bitmap image, out Rectangle opaqueBounds);
        void ApplyToImage(byte[] bytes, int width, int height, int bytesPerPixel, int stride, Rectangle? opaqueBounds);
    }
}