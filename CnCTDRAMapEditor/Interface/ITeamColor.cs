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
using System.Drawing;

namespace MobiusEditor.Interface
{
    public interface ITeamColor
    {
        /// <summary>Name of the color, used as identifier.</summary>
        string Name { get; }

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

        /// <summary>
        /// Applies the team colour to the image bytes.
        /// </summary>
        /// <param name="bytes">Image contents</param>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <param name="bytesPerPixel">Bytes per pixel</param>
        /// <param name="stride">Stride of the image</param>
        /// <param name="opaqueBounds">Opaque bounds. If given, this might optimize the operation.</param>
        void ApplyToImage(byte[] bytes, int width, int height, int bytesPerPixel, int stride, Rectangle? opaqueBounds);
    }
}