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
        /// <summary>Name of the color, used as identifier.</summary>
        string Name { get; }
        /// <summary>A general color representing this team color.</summary>
        Color BaseColor { get; }

        /// <summary>
        /// Apply this color to a given image.
        /// </summary>
        /// <param name="image">The image to process.</param>
        void ApplyToImage(Bitmap image);

        /// <summary>
        /// Apply this color to a given image, and output the actually-used bounds of the graphics.
        /// </summary>
        /// <param name="image">The image to process.</param>
        /// <param name="opaqueBounds">The actually-used nontransparent area of the graphics that need to be painted.</param>
        void ApplyToImage(Bitmap image, out Rectangle opaqueBounds);
    }
}